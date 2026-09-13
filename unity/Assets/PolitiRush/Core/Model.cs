using System;
using System.Collections.Generic;

namespace PolitiRush.Core
{
    // Modèle de données du jeu. Miroir de Model.kt.
    // Coordonnées : x dans [0, 1] (largeur de la piste), y dans [0, 1]
    // (0 = bas / position du héros, 1 = haut / ligne d'apparition).

    public enum Special { Rafale, Uppercut, Lames, Gadget, CompetencesParticulieres, Livraison }

    public sealed class Hero
    {
        public string Id; public string Name; public string Tagline;
        public float FireRate; public int Damage; public float MoveSpeed;
        public Special Special; public float SpecialCooldown;

        public Hero(string id, string name, string tagline, float fireRate, int damage, float moveSpeed, Special special, float specialCooldown)
        { Id = id; Name = name; Tagline = tagline; FireRate = fireRate; Damage = damage; MoveSpeed = moveSpeed; Special = special; SpecialCooldown = specialCooldown; }

        public Hero Clone() => (Hero)MemberwiseClone();
    }

    /// <summary>Rang d'une cible : la résistance, la vitesse et la récompense en découlent.</summary>
    public enum Tier { HautFonctionnaire, Depute, Ministre, ChefEtat, HyperInfluent }

    public static class TierInfo
    {
        public static string Label(this Tier t) => t switch
        {
            Tier.HautFonctionnaire => "Haut fonctionnaire", Tier.Depute => "Député", Tier.Ministre => "Ministre",
            Tier.ChefEtat => "Chef d'État", _ => "Hyper-influent",
        };
        public static int Hp(this Tier t) => t switch { Tier.HautFonctionnaire => 1, Tier.Depute => 2, Tier.Ministre => 5, Tier.ChefEtat => 60, _ => 120 };
        public static float Speed(this Tier t) => t switch { Tier.HautFonctionnaire => 0.16f, Tier.Depute => 0.13f, Tier.Ministre => 0.10f, Tier.ChefEtat => 0.04f, _ => 0.035f };
        public static int Points(this Tier t) => t switch { Tier.HautFonctionnaire => 10, Tier.Depute => 20, Tier.Ministre => 50, Tier.ChefEtat => 500, _ => 1000 };
        public static int Coins(this Tier t) => t switch { Tier.HautFonctionnaire => 1, Tier.Depute => 2, Tier.Ministre => 3, Tier.ChefEtat => 25, _ => 50 };
        public static int FromWave(this Tier t) => t switch { Tier.HautFonctionnaire => 1, Tier.Depute => 2, Tier.Ministre => 4, Tier.ChefEtat => 5, _ => 15 };
    }

    public sealed class PoliticianType
    {
        public string Id; public string Name; public Tier Tier;
        public int Hp; public float Speed; public int Points; public int Coins; public bool IsBoss;

        public PoliticianType(string id, string name, Tier tier, int hp, float speed, int points, int coins, bool isBoss = false)
        { Id = id; Name = name; Tier = tier; Hp = hp; Speed = speed; Points = points; Coins = coins; IsBoss = isBoss; }

        public static PoliticianType OfTier(string id, string name, Tier tier) =>
            new PoliticianType(id, name, tier, tier.Hp(), tier.Speed(), tier.Points(), tier.Coins());
    }

    /// <summary>Opération appliquée au multiplicateur de tir quand le héros franchit une porte.</summary>
    public abstract class GateOp
    {
        public abstract int ApplyRaw(int value);
        public abstract string Label { get; }
        public abstract bool IsGood { get; }
        public int Apply(int value) => Math.Clamp(ApplyRaw(value), GameConfig.MinFirepower, GameConfig.MaxFirepower);

        public sealed class Multiply : GateOp { public readonly int Factor; public Multiply(int f) { Factor = f; } public override int ApplyRaw(int v) => v * Factor; public override string Label => "x" + Factor; public override bool IsGood => true; }
        public sealed class Add : GateOp { public readonly int Amount; public Add(int a) { Amount = a; } public override int ApplyRaw(int v) => v + Amount; public override string Label => "+" + Amount; public override bool IsGood => true; }
        public sealed class Subtract : GateOp { public readonly int Amount; public Subtract(int a) { Amount = a; } public override int ApplyRaw(int v) => v - Amount; public override string Label => "-" + Amount; public override bool IsGood => false; }
        public sealed class Divide : GateOp { public readonly int Divisor; public Divide(int d) { Divisor = d; } public override int ApplyRaw(int v) => v / Divisor; public override string Label => "÷" + Divisor; public override bool IsGood => false; }
    }

    public enum BonusType { Sondage, Scandale, TomateGeante, MotionDeCensure, Meeting }

    public static class BonusInfo
    {
        public static string Label(this BonusType b) => b switch
        {
            BonusType.Sondage => "Sondage", BonusType.Scandale => "Scandale", BonusType.TomateGeante => "Tomate géante",
            BonusType.MotionDeCensure => "Motion de censure", _ => "Meeting",
        };
        public static float Duration(this BonusType b) => b switch
        {
            BonusType.Sondage => 5f, BonusType.Scandale => 3f, BonusType.TomateGeante => 6f, BonusType.MotionDeCensure => 0f, _ => 8f,
        };
        public static readonly BonusType[] All = { BonusType.Sondage, BonusType.Scandale, BonusType.TomateGeante, BonusType.MotionDeCensure, BonusType.Meeting };
    }

    public sealed class Gate
    {
        public int Id; public GateOp Op; public int Side; public float Y; public bool Consumed;
        public Gate(int id, GateOp op, int side, float y) { Id = id; Op = op; Side = side; Y = y; }
        public float XMin => Side == 0 ? 0f : 0.5f;
        public float XMax => Side == 0 ? 0.5f : 1f;
    }

    public sealed class Enemy
    {
        public int Id; public PoliticianType Type; public float X; public float Y; public int Hp; public int MaxHp;
        /// <summary>Temps restant du flash de coup, et délai avant le prochain flash.</summary>
        public float Hurt; public float HurtCooldown;
        public Enemy(int id, PoliticianType type, float x, float y, int hp) { Id = id; Type = type; X = x; Y = y; Hp = hp; MaxHp = hp; }
    }

    public sealed class Projectile
    {
        public int Id; public float X; public float Y; public float Vx; public int Damage; public bool Piercing;
        /// <summary>Trajectoire en cloche (main et lance-pierre), purement visuel.</summary>
        public bool Arc;
        public Projectile(int id, float x, float y, float vx, int damage, bool piercing, bool arc) { Id = id; X = x; Y = y; Vx = vx; Damage = damage; Piercing = piercing; Arc = arc; }
    }

    public sealed class BonusPickup { public int Id; public BonusType Type; public float X; public float Y; public BonusPickup(int id, BonusType t, float x, float y) { Id = id; Type = t; X = x; Y = y; } }
    public sealed class Coin { public int Id; public float X; public float Y; public Coin(int id, float x, float y) { Id = id; X = x; Y = y; } }

    public enum GameStatus { Running, GameOver }

    public abstract class GameEvent
    {
        public sealed class GatePassed : GameEvent { public GateOp Op; public int NewFirepower; }
        public sealed class EnemyHit : GameEvent { public int EnemyId; public float X; public float Y; }
        public sealed class EnemyKilled : GameEvent { public int EnemyId; public PoliticianType Type; public float X; public float Y; public int Combo; }
        public sealed class BonusPicked : GameEvent { public BonusType Type; }
        public sealed class CoinPicked : GameEvent { public int Total; }
        public sealed class SpecialUsed : GameEvent { public Special Special; }
        public sealed class WaveStarted : GameEvent { public int Wave; }
        public sealed class HeroHurt : GameEvent { }
        public sealed class GameOver : GameEvent { }
    }

    /// <summary>État complet d'une partie. Mutable, mis à jour en place par GameEngine.Step.</summary>
    public sealed class GameState
    {
        public Hero Hero;
        public float HeroX = 0.5f;
        public int HeroHp = GameConfig.HeroMaxHp;
        public int Firepower = GameConfig.StartFirepower;
        public int Wave = 1;
        public int Score;
        public int Coins;
        public int Combo;
        public int BestCombo;
        public float ComboTimer;
        public float Elapsed;
        public float Distance;
        public readonly List<Enemy> Enemies = new List<Enemy>();
        public readonly List<Gate> Gates = new List<Gate>();
        public readonly List<Projectile> Projectiles = new List<Projectile>();
        public readonly List<BonusPickup> Bonuses = new List<BonusPickup>();
        public readonly List<Coin> CoinsOnTrack = new List<Coin>();
        public readonly Dictionary<BonusType, float> Effects = new Dictionary<BonusType, float>();
        public float FireTimer;
        public float SpecialCooldownRemaining;
        public float SpecialActiveRemaining;
        /// <summary>Temps restant de l'onde de choc (uppercut), visuel.</summary>
        public float Shock;
        /// <summary>Temps restant du flash blanc (motion de censure), visuel.</summary>
        public float Flash;
        public GameStatus Status = GameStatus.Running;

        public GameState(Hero hero) { Hero = hero; }

        public bool HasEffect(BonusType t) => Effects.TryGetValue(t, out var r) && r > 0f;
        public bool SpecialReady => SpecialCooldownRemaining <= 0f && Status == GameStatus.Running;
        public Weapon Weapon => WeaponInfo.ForFirepower(Firepower);
    }

    public struct PlayerInput
    {
        /// <summary>Position latérale visée dans [0, 1]. Null = ne bouge pas.</summary>
        public float? TargetX;
        public bool UseSpecial;
        public PlayerInput(float? targetX = null, bool useSpecial = false) { TargetX = targetX; UseSpecial = useSpecial; }
    }
}
