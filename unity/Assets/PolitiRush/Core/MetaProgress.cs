using System;
using System.Collections.Generic;

namespace PolitiRush.Core
{
    /// <summary>Améliorations permanentes achetées avec les pièces. Miroir de MetaProgress.kt.</summary>
    public enum Upgrade { Cadence, Degats, MultiplicateurDepart, Vies, RechargeSpecial }

    public static class UpgradeInfo
    {
        public static string Label(this Upgrade u) => u switch
        {
            Upgrade.Cadence => "Cadence de tir", Upgrade.Degats => "Dégâts", Upgrade.MultiplicateurDepart => "Multiplicateur de départ",
            Upgrade.Vies => "Vies", _ => "Recharge du spécial",
        };
        public static int BaseCost(this Upgrade u) => u switch { Upgrade.Cadence => 50, Upgrade.Degats => 80, Upgrade.MultiplicateurDepart => 120, Upgrade.Vies => 200, _ => 100 };
        public static int MaxLevel(this Upgrade u) => u switch { Upgrade.Cadence => 10, Upgrade.Degats => 10, Upgrade.MultiplicateurDepart => 5, Upgrade.Vies => 3, _ => 5 };
        public static int Cost(this Upgrade u, int currentLevel) => (int)(u.BaseCost() * Math.Pow(1.6, currentLevel));
        public static readonly Upgrade[] All = { Upgrade.Cadence, Upgrade.Degats, Upgrade.MultiplicateurDepart, Upgrade.Vies, Upgrade.RechargeSpecial };
    }

    public sealed class MetaProgress
    {
        public int Coins; public int BestScore; public int BestWave; public int Runs;
        public readonly Dictionary<Upgrade, int> Levels = new Dictionary<Upgrade, int>();
        public readonly HashSet<string> UnlockedHeroes = new HashSet<string> { "rambeau" };

        public int Level(Upgrade u) => Levels.TryGetValue(u, out var l) ? l : 0;
        public bool CanBuy(Upgrade u) => Level(u) < u.MaxLevel() && Coins >= u.Cost(Level(u));

        public bool Buy(Upgrade u)
        {
            if (!CanBuy(u)) return false;
            Coins -= u.Cost(Level(u)); Levels[u] = Level(u) + 1; return true;
        }

        public void AfterRun(GameState final)
        {
            Coins += final.Coins; BestScore = Math.Max(BestScore, final.Score); BestWave = Math.Max(BestWave, final.Wave); Runs += 1;
        }

        public Hero ApplyTo(Hero hero)
        {
            var h = hero.Clone();
            h.FireRate = hero.FireRate * (1f + 0.08f * Level(Upgrade.Cadence));
            h.Damage = hero.Damage + Level(Upgrade.Degats) / 2;
            h.SpecialCooldown = hero.SpecialCooldown * (1f - 0.08f * Level(Upgrade.RechargeSpecial));
            return h;
        }

        public GameState StartingState(Hero hero) => new GameState(ApplyTo(hero))
        {
            HeroHp = GameConfig.HeroMaxHp + Level(Upgrade.Vies),
            Firepower = GameConfig.StartFirepower + Level(Upgrade.MultiplicateurDepart),
        };
    }
}
