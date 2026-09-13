using System;
using System.Collections.Generic;

namespace PolitiRush.Core
{
    /// <summary>
    /// Moteur de simulation : Step(dt, input) fait avancer un GameState en place et
    /// renvoie les événements du pas. Aucune dépendance Unity. Miroir de GameEngine.kt,
    /// même ordre des étapes (le ramassage précède les collisions).
    /// </summary>
    public sealed class GameEngine
    {
        public GameState State { get; }
        public Level Level { get; }
        readonly Spawner spawner;

        public GameEngine(GameState initialState, int seed = 42, Level level = null, Spawner spawner = null)
        {
            State = initialState; Level = level ?? Levels.Palais; this.spawner = spawner ?? new Spawner(seed, Level);
        }

        public GameEngine(Hero hero, int seed = 42, Level level = null) : this(new GameState(hero), seed, level) { }

        public List<GameEvent> Step(float dt, PlayerInput input = default)
        {
            var s = State; var events = new List<GameEvent>();
            if (s.Status == GameStatus.GameOver || dt <= 0f) return events;

            // 1. Temps, vagues.
            s.Elapsed += dt;
            int wave = (int)(s.Elapsed / GameConfig.WaveDuration) + 1;
            if (wave != s.Wave) { s.Wave = wave; events.Add(new GameEvent.WaveStarted { Wave = wave }); }
            s.Distance += GameConfig.ScrollSpeed * dt;

            // 2. Déplacement du héros.
            if (input.TargetX.HasValue)
            {
                float t = Math.Clamp(input.TargetX.Value, 0f, 1f), delta = t - s.HeroX, maxMove = s.Hero.MoveSpeed * dt;
                s.HeroX = Math.Abs(delta) <= maxMove ? t : s.HeroX + Math.Sign(delta) * maxMove;
            }

            // 3. Effets et recharges.
            var expired = new List<BonusType>();
            foreach (var k in new List<BonusType>(s.Effects.Keys)) { s.Effects[k] -= dt; if (s.Effects[k] <= 0f) expired.Add(k); }
            foreach (var k in expired) s.Effects.Remove(k);
            s.SpecialCooldownRemaining = Math.Max(0f, s.SpecialCooldownRemaining - dt);
            s.SpecialActiveRemaining = Math.Max(0f, s.SpecialActiveRemaining - dt);
            s.ComboTimer += dt;
            if (s.Combo > 0 && s.ComboTimer > GameConfig.ComboWindow) s.Combo = 0;
            if (s.Shock > 0f) s.Shock -= dt;
            if (s.Flash > 0f) s.Flash -= dt;

            // 4. Spécial.
            if (input.UseSpecial && s.SpecialReady) { UseSpecial(s); events.Add(new GameEvent.SpecialUsed { Special = s.Hero.Special }); }

            // 5. Apparitions.
            float waveProgress = (s.Elapsed % GameConfig.WaveDuration) / GameConfig.WaveDuration;
            var batch = spawner.Advance(dt, wave, waveProgress, s.Enemies.Count);
            s.Gates.AddRange(batch.Gates); s.Enemies.AddRange(batch.Enemies); s.Bonuses.AddRange(batch.Bonuses);

            // 6. Défilement.
            float scroll = GameConfig.ScrollSpeed * dt;
            bool frozen = s.HasEffect(BonusType.Scandale) || (s.Hero.Special == Special.Gadget && s.SpecialActiveRemaining > 0f);
            bool magnet = s.HasEffect(BonusType.Meeting);
            foreach (var g in s.Gates) g.Y -= scroll;
            s.Gates.RemoveAll(g => g.Y <= -0.1f);
            foreach (var b in s.Bonuses) b.Y -= scroll;
            s.Bonuses.RemoveAll(b => b.Y <= -0.1f);
            if (!frozen) foreach (var e in s.Enemies) e.Y -= e.Type.Speed * dt;
            foreach (var c in s.CoinsOnTrack)
            {
                if (magnet)
                {
                    float dx = s.HeroX - c.X, dy = GameConfig.HeroY - c.Y, d = Math.Max(1e-4f, Hypot(dx, dy)), step = 2.0f * dt;
                    if (d <= step) { c.X = s.HeroX; c.Y = GameConfig.HeroY; } else { c.X += dx / d * step; c.Y += dy / d * step; }
                }
                else c.Y -= scroll;
            }
            s.CoinsOnTrack.RemoveAll(c => c.Y <= -0.1f);

            // 7. Portes franchies.
            foreach (var g in s.Gates)
            {
                if (g.Consumed || g.Y > GameConfig.HeroY) continue;
                if (s.HeroX >= g.XMin && s.HeroX < g.XMax)
                {
                    s.Firepower = g.Op.Apply(s.Firepower);
                    events.Add(new GameEvent.GatePassed { Op = g.Op, NewFirepower = s.Firepower });
                }
                g.Consumed = true;
            }

            // 8. Ramassage : bonus et pièces (avant les collisions, pour que la motion de censure agisse dans ce pas).
            float pickRadius = GameConfig.HeroRadius + 0.03f;
            var picked = s.Bonuses.FindAll(b => Hypot(b.X - s.HeroX, b.Y - GameConfig.HeroY) <= pickRadius);
            foreach (var b in picked) { s.Bonuses.Remove(b); ApplyBonus(s, b.Type); events.Add(new GameEvent.BonusPicked { Type = b.Type }); }
            int coinsPicked = s.CoinsOnTrack.RemoveAll(c => Hypot(c.X - s.HeroX, c.Y - GameConfig.HeroY) <= pickRadius);
            if (coinsPicked > 0) { s.Coins += coinsPicked; events.Add(new GameEvent.CoinPicked { Total = s.Coins }); }

            // 9. Tir automatique.
            bool rafale = s.Hero.Special == Special.Rafale && s.SpecialActiveRemaining > 0f;
            float fireRate = s.Hero.FireRate * (rafale ? GameConfig.RafaleFactor : 1f);
            bool piercing = s.HasEffect(BonusType.TomateGeante) || (s.Hero.Special == Special.Lames && s.SpecialActiveRemaining > 0f);
            bool fan = s.HasEffect(BonusType.Sondage) || (s.Hero.Special == Special.CompetencesParticulieres && s.SpecialActiveRemaining > 0f);
            s.FireTimer += dt;
            float interval = 1f / fireRate;
            while (s.FireTimer >= interval) { s.FireTimer -= interval; Salvo(s, piercing, fan); }
            if (s.Projectiles.Count > GameConfig.MaxProjectiles) s.Projectiles.RemoveRange(0, s.Projectiles.Count - GameConfig.MaxProjectiles);

            // 10. Projectiles : déplacement et collisions.
            for (int i = s.Projectiles.Count - 1; i >= 0; i--)
            {
                var p = s.Projectiles[i];
                p.Y += GameConfig.ProjectileSpeed * dt; p.X += p.Vx * dt;
                if (p.Y > 1.1f || p.X < -0.05f || p.X > 1.05f) { s.Projectiles.RemoveAt(i); continue; }
                Enemy hit = null;
                foreach (var e in s.Enemies)
                {
                    if (e.Hp <= 0) continue;
                    float r = (e.Type.IsBoss ? 0.11f : GameConfig.EnemyRadius * (e.Type.Tier == Tier.Ministre ? 1.25f : 1f)) + GameConfig.ProjectileRadius;
                    if (Hypot(e.X - p.X, e.Y - p.Y) <= r) { hit = e; break; }
                }
                if (hit == null) continue;
                hit.Hp -= p.Damage;
                if (hit.HurtCooldown <= 0f) { hit.Hurt = 0.1f; hit.HurtCooldown = 0.45f; }
                events.Add(new GameEvent.EnemyHit { EnemyId = hit.Id, X = hit.X, Y = hit.Y });
                if (!p.Piercing) s.Projectiles.RemoveAt(i);
            }

            // 11. Éliminations, score, pièces.
            for (int i = s.Enemies.Count - 1; i >= 0; i--)
            {
                var e = s.Enemies[i];
                if (e.Hp > 0) { if (e.Hurt > 0f) e.Hurt -= dt; if (e.HurtCooldown > 0f) e.HurtCooldown -= dt; continue; }
                s.Combo += 1; s.BestCombo = Math.Max(s.BestCombo, s.Combo); s.ComboTimer = 0f;
                s.Score += e.Type.Points * ComboMultiplier(s.Combo);
                for (int c = 0; c < e.Type.Coins; c++)
                    s.CoinsOnTrack.Add(new Coin(spawner.NextId(), Math.Clamp(e.X + (c - e.Type.Coins / 2) * 0.03f, 0f, 1f), e.Y));
                events.Add(new GameEvent.EnemyKilled { EnemyId = e.Id, Type = e.Type, X = e.X, Y = e.Y, Combo = s.Combo });
                s.Enemies.RemoveAt(i);
            }

            // 12. Cibles arrivées sur le héros.
            int reached = s.Enemies.RemoveAll(e => e.Y <= GameConfig.HeroY + GameConfig.EnemyRadius);
            if (reached > 0)
            {
                s.HeroHp = Math.Max(0, s.HeroHp - reached); s.Combo = 0;
                for (int i = 0; i < reached; i++) events.Add(new GameEvent.HeroHurt());
                if (s.HeroHp <= 0) { s.Status = GameStatus.GameOver; events.Add(new GameEvent.GameOver()); }
            }
            return events;
        }

        static int ComboMultiplier(int combo) => 1 + combo / GameConfig.ComboStep;
        static float Hypot(float a, float b) => (float)Math.Sqrt(a * a + b * b);

        void Salvo(GameState s, bool piercing, bool fan)
        {
            int n = s.Firepower; float width = fan ? 1f : GameConfig.SpreadWidth;
            bool arc = s.Firepower < Weapon.Canon.MinFirepower();
            for (int i = 0; i < n; i++)
            {
                float t = n == 1 ? 0f : (i / (float)(n - 1)) - 0.5f;
                float x = fan ? 0.5f + t * width : Math.Clamp(s.HeroX + t * width, 0.02f, 0.98f);
                s.Projectiles.Add(new Projectile(spawner.NextId(), x, GameConfig.HeroY, fan ? t * 0.6f : 0f, s.Hero.Damage, piercing, arc));
            }
        }

        void UseSpecial(GameState s)
        {
            s.SpecialCooldownRemaining = s.Hero.SpecialCooldown;
            switch (s.Hero.Special)
            {
                case Special.Rafale: s.SpecialActiveRemaining = GameConfig.RafaleDuration; break;
                case Special.Lames: s.SpecialActiveRemaining = GameConfig.LamesDuration; break;
                case Special.Gadget: s.SpecialActiveRemaining = GameConfig.GadgetDuration; break;
                case Special.CompetencesParticulieres: s.SpecialActiveRemaining = GameConfig.CompetencesDuration; break;
                case Special.Livraison: s.Firepower = new GateOp.Multiply(2).Apply(s.Firepower); break;
                case Special.Uppercut:
                    foreach (var e in s.Enemies)
                        if (Hypot(e.X - s.HeroX, e.Y - GameConfig.HeroY) <= GameConfig.UppercutRadius) { e.Hp -= 10; e.Y = Math.Min(1.05f, e.Y + 0.3f); e.Hurt = 0.2f; }
                    s.Shock = 0.3f; break;
            }
        }

        static void ApplyBonus(GameState s, BonusType type)
        {
            if (type == BonusType.MotionDeCensure)
            {
                foreach (var e in s.Enemies) e.Hp = e.Type.IsBoss ? e.Hp - (int)(e.Type.Hp * 0.3f) : 0;
                s.Flash = 0.25f;
            }
            else s.Effects[type] = type.Duration();
        }
    }
}
