using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PolitiRush.Core.Tests
{
    public class GameEngineTests
    {
        readonly Hero hero = Roster.Hero("rambeau");

        /// <summary>Spawner inerte, pour isoler la mécanique testée.</summary>
        class SilentSpawner : Spawner
        {
            public SilentSpawner() : base(0) { }
            public override SpawnBatch Advance(float dt, int wave, float waveProgress, int enemiesAlive) => new SpawnBatch();
        }

        GameEngine Engine(GameState state = null) => new GameEngine(state ?? new GameState(hero), spawner: new SilentSpawner());

        static List<GameEvent> Run(GameEngine e, float seconds, PlayerInput input = default, float dt = 1f / 60f)
        {
            var events = new List<GameEvent>();
            for (float t = 0f; t < seconds; t += dt) events.AddRange(e.Step(dt, input));
            return events;
        }

        [Test] public void LeHerosSeDeplaceSansDepasserSaVitesse()
        {
            var e = Engine();
            e.Step(0.1f, new PlayerInput(1f));
            Assert.AreEqual(0.5f + hero.MoveSpeed * 0.1f, e.State.HeroX, 1e-4f);
            Run(e, 2f, new PlayerInput(1f));
            Assert.AreEqual(1f, e.State.HeroX, 1e-4f);
        }

        [Test] public void FranchirUnePorteDuBonCoteAppliqueSonOperation()
        {
            var s = new GameState(hero) { HeroX = 0.25f, Firepower = 2 };
            s.Gates.Add(new Gate(1, new GateOp.Multiply(3), 0, GameConfig.HeroY + 0.01f));
            s.Gates.Add(new Gate(2, new GateOp.Subtract(1), 1, GameConfig.HeroY + 0.01f));
            var e = Engine(s);
            var events = e.Step(0.1f);
            Assert.AreEqual(6, e.State.Firepower);
            Assert.AreEqual(1, events.OfType<GameEvent.GatePassed>().Count());
            Assert.IsTrue(e.State.Gates.All(g => g.Consumed));
            e.Step(0.1f);
            Assert.AreEqual(6, e.State.Firepower);
        }

        [Test] public void UneSalveContientAutantDeProjectilesQueLeMultiplicateur()
        {
            var e = Engine(new GameState(hero) { Firepower = 5 });
            e.Step(1f / hero.FireRate + 0.001f);
            Assert.AreEqual(5, e.State.Projectiles.Count);
        }

        [Test] public void EliminerUneCibleRapporteScoreComboEtPieces()
        {
            var type = Roster.Politicians[0];
            var s = new GameState(hero) { HeroX = 0.5f };
            s.Enemies.Add(new Enemy(1, type, 0.5f, 0.3f, 1));
            var e = Engine(s);
            var events = Run(e, 1f);
            var kill = events.OfType<GameEvent.EnemyKilled>().Single();
            Assert.AreEqual(type, kill.Type);
            Assert.AreEqual(1, e.State.Combo);
            Assert.AreEqual(type.Points, e.State.Score);
            Assert.AreEqual(0, e.State.Enemies.Count);
            Assert.AreEqual(type.Coins, e.State.CoinsOnTrack.Count + e.State.Coins);
        }

        [Test] public void LeComboRetombeApresLaFenetre()
        {
            var e = Engine(new GameState(hero) { Combo = 7 });
            Run(e, GameConfig.ComboWindow + 0.2f);
            Assert.AreEqual(0, e.State.Combo);
        }

        [Test] public void UneCibleQuiAtteintLeHerosTermineLaPartie()
        {
            var s = new GameState(hero) { HeroX = 0f, HeroHp = 1 };
            s.Enemies.Add(new Enemy(1, Roster.Politicians[0], 1f, GameConfig.HeroY + 0.01f, 999));
            var e = Engine(s);
            var events = Run(e, 1f);
            Assert.IsTrue(events.Any(ev => ev is GameEvent.HeroHurt));
            Assert.IsTrue(events.Any(ev => ev is GameEvent.GameOver));
            Assert.AreEqual(GameStatus.GameOver, e.State.Status);
            Assert.AreEqual(0, e.State.HeroHp);
            float elapsed = e.State.Elapsed;
            e.Step(1f);
            Assert.AreEqual(elapsed, e.State.Elapsed);
        }

        [Test] public void LeBonusScandaleGeleLesCibles()
        {
            var s = new GameState(hero) { HeroX = 0.5f };
            s.Enemies.Add(new Enemy(1, Roster.Politicians[0], 0.9f, 0.8f, 999));
            s.Bonuses.Add(new BonusPickup(2, BonusType.Scandale, 0.5f, GameConfig.HeroY));
            var e = Engine(s);
            var events = e.Step(1f / 60f);
            Assert.IsTrue(events.OfType<GameEvent.BonusPicked>().Any(b => b.Type == BonusType.Scandale));
            Assert.IsTrue(e.State.HasEffect(BonusType.Scandale));
            float yBefore = e.State.Enemies[0].Y;
            Run(e, 1f);
            Assert.AreEqual(yBefore, e.State.Enemies[0].Y, 1e-5f);
            Run(e, BonusType.Scandale.Duration() + 0.5f);
            Assert.Less(e.State.Enemies[0].Y, yBefore);
        }

        [Test] public void LaMotionDeCensureElimineToutSaufLesBoss()
        {
            var s = new GameState(hero) { HeroX = 0.5f };
            s.Enemies.Add(new Enemy(1, Roster.Politicians[0], 0.2f, 0.7f, 5));
            s.Enemies.Add(new Enemy(2, Roster.Bosses[0], 0.8f, 0.9f, Roster.Bosses[0].Hp));
            s.Bonuses.Add(new BonusPickup(3, BonusType.MotionDeCensure, 0.5f, GameConfig.HeroY));
            var e = Engine(s);
            e.Step(1f / 60f);
            Assert.AreEqual(1, e.State.Enemies.Count);
            Assert.IsTrue(e.State.Enemies[0].Type.IsBoss);
            Assert.Less(e.State.Enemies[0].Hp, Roster.Bosses[0].Hp);
        }

        [Test] public void LeSpecialLivraisonDoubleLeMultiplicateur()
        {
            var e = new GameEngine(new GameState(Roster.Hero("transporteur")) { Firepower = 4 }, spawner: new SilentSpawner());
            var events = e.Step(1f / 60f, new PlayerInput(null, true));
            Assert.IsTrue(events.OfType<GameEvent.SpecialUsed>().Any(ev => ev.Special == Special.Livraison));
            Assert.AreEqual(8, e.State.Firepower);
            Assert.IsFalse(e.State.SpecialReady);
            e.Step(1f / 60f, new PlayerInput(null, true));
            Assert.AreEqual(8, e.State.Firepower);
        }

        [Test] public void LeSpecialRafaleTripleLaCadence()
        {
            var e1 = Engine(new GameState(hero) { Firepower = 1 });
            var e2 = Engine(new GameState(hero) { Firepower = 1 });
            Run(e1, 1f);
            Run(e2, 1f, new PlayerInput(null, true));
            Assert.Greater(e2.State.Projectiles.Count, e1.State.Projectiles.Count * 2);
        }

        [Test] public void UnePartieCompleteEstDeterministe()
        {
            GameState Play(int seed)
            {
                var e = new GameEngine(hero, seed);
                for (float t = 0f; e.State.Status == GameStatus.Running && t < 120f; t += 1f / 60f)
                {
                    Gate g = null;
                    foreach (var gate in e.State.Gates) if (!gate.Consumed && (g == null || gate.Y < g.Y)) g = gate;
                    float? target = g == null ? (float?)null : (g.Op.IsGood ? (g.XMin + g.XMax) / 2 : 1f - (g.XMin + g.XMax) / 2);
                    e.Step(1f / 60f, new PlayerInput(target, true));
                }
                return e.State;
            }
            var a = Play(7); var b = Play(7);
            Assert.AreEqual(a.Score, b.Score);
            Assert.AreEqual(a.Wave, b.Wave);
            Assert.Greater(a.Score, 0);
            Assert.IsTrue(a.Firepower >= GameConfig.MinFirepower && a.Firepower <= GameConfig.MaxFirepower);
            Assert.LessOrEqual(a.Projectiles.Count, GameConfig.MaxProjectiles);
        }
    }
}
