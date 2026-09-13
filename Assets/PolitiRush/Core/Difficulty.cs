using System;

namespace PolitiRush.Core
{
    /// <summary>
    /// Courbe de difficulté. Miroir de Difficulty.kt.
    /// Résistance : rang + vagues (plus vite pour les rangs élevés).
    /// Nombre : cadence, groupes, plafond, mélange des rangs selon l'avancée.
    /// </summary>
    public static class Difficulty
    {
        public static float SpawnInterval(int wave) => Math.Max(GameConfig.MinSpawnInterval, GameConfig.BaseSpawnInterval - (wave - 1) * 0.07f);
        public static int GroupSize(int wave) => Math.Min(4, 1 + (wave - 1) / 3);
        public static int MaxAlive(int wave) => Math.Min(40, 12 + wave * 3);

        public static int Weight(Tier tier, int wave) => tier switch
        {
            Tier.HautFonctionnaire => Math.Max(1, 6 - (wave - 1) / 2),
            Tier.Depute => wave < Tier.Depute.FromWave() ? 0 : Math.Min(4, wave - 1),
            Tier.Ministre => wave < Tier.Ministre.FromWave() ? 0 : Math.Min(3, wave - 3),
            _ => 0,
        };

        public static int Hp(PoliticianType type, int wave) => type.Hp + ((wave - 1) / 3) * ((int)type.Tier + 1);
        public static int BossHp(PoliticianType type, int wave) => type.Hp + wave * 5;
    }
}
