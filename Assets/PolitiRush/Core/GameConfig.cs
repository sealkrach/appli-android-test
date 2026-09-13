namespace PolitiRush.Core
{
    /// <summary>Constantes d'équilibrage. Miroir de GameConfig.kt.</summary>
    public static class GameConfig
    {
        public const int HeroMaxHp = 3;
        public const int StartFirepower = 1;
        public const int MinFirepower = 1;
        public const int MaxFirepower = 64;

        public const float HeroY = 0.08f;
        public const float HeroRadius = 0.06f;
        public const float EnemyRadius = 0.05f;
        public const float ProjectileRadius = 0.015f;
        public const float ProjectileSpeed = 1.4f;
        public const int MaxProjectiles = 300;
        public const float SpreadWidth = 0.35f;

        public const float ScrollSpeed = 0.35f;
        public const float GateInterval = 6f;
        public const float BonusInterval = 11f;

        public const float WaveDuration = 20f;
        public const float BaseSpawnInterval = 1.1f;
        public const float MinSpawnInterval = 0.25f;
        public const int BossEveryNWaves = 5;

        public const float ComboWindow = 2.5f;
        public const int ComboStep = 10;

        public const float RafaleFactor = 3f;
        public const float RafaleDuration = 4f;
        public const float UppercutRadius = 0.35f;
        public const float LamesDuration = 5f;
        public const float GadgetDuration = 4f;
        public const float CompetencesDuration = 6f;
    }
}
