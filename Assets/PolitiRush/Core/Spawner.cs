using System;
using System.Collections.Generic;

namespace PolitiRush.Core
{
    public sealed class SpawnBatch
    {
        public readonly List<Gate> Gates = new List<Gate>();
        public readonly List<Enemy> Enemies = new List<Enemy>();
        public readonly List<BonusPickup> Bonuses = new List<BonusPickup>();
    }

    /// <summary>
    /// Génère portes, cibles et bonus au fil du temps. Déterministe pour une graine
    /// donnée (défis du jour partagés). Miroir de Spawner.kt.
    /// </summary>
    public class Spawner
    {
        readonly Random random;
        readonly Level level;
        int nextId = 1;
        float gateTimer = 2f, bonusTimer = 5f, spawnTimer = 1f;
        int bossSpawnedForWave;

        public Spawner(int seed, Level level = null) { random = new Random(seed); this.level = level ?? Levels.Palais; }

        public int NextId() => nextId++;
        float NextFloat() => (float)random.NextDouble();
        T Pick<T>(List<T> list) => list[random.Next(list.Count)];

        public virtual SpawnBatch Advance(float dt, int wave, float waveProgress, int enemiesAlive)
        {
            var batch = new SpawnBatch();

            gateTimer -= dt;
            if (gateTimer <= 0f) { gateTimer += GameConfig.GateInterval; batch.Gates.AddRange(GatePair(wave)); }

            bonusTimer -= dt;
            if (bonusTimer <= 0f)
            {
                bonusTimer += GameConfig.BonusInterval;
                var type = BonusInfo.All[random.Next(BonusInfo.All.Length)];
                batch.Bonuses.Add(new BonusPickup(NextId(), type, NextFloat() * 0.8f + 0.1f, 1.05f));
            }

            bool isBossWave = wave % GameConfig.BossEveryNWaves == 0;
            if (isBossWave && waveProgress > 0.3f && bossSpawnedForWave != wave)
            {
                bossSpawnedForWave = wave;
                var boss = level.BossForWave(wave);
                batch.Enemies.Add(new Enemy(NextId(), boss, 0.5f, 1.1f, Difficulty.BossHp(boss, wave)));
            }

            spawnTimer -= dt;
            float interval = Difficulty.SpawnInterval(wave);
            while (spawnTimer <= 0f)
            {
                spawnTimer += interval;
                var pool = new List<PoliticianType>();
                foreach (var t in level.Politicians) for (int w = Difficulty.Weight(t.Tier, wave); w > 0; w--) pool.Add(t);
                if (pool.Count == 0) continue;
                int group = Difficulty.GroupSize(wave);
                float center = NextFloat() * 0.8f + 0.1f;
                for (int i = 0; i < group; i++)
                {
                    if (enemiesAlive + batch.Enemies.Count >= Difficulty.MaxAlive(wave)) break;
                    var type = Pick(pool);
                    float x = Math.Clamp(center + (i - (group - 1) / 2f) * 0.14f + (NextFloat() - 0.5f) * 0.06f, 0.05f, 0.95f);
                    batch.Enemies.Add(new Enemy(NextId(), type, x, 1.05f + i * 0.03f, Difficulty.Hp(type, wave)));
                }
            }
            return batch;
        }

        List<Gate> GatePair(int wave)
        {
            var good = GoodOp(wave);
            var other = NextFloat() < 0.65f ? BadOp() : GoodOp(wave);
            int goodSide = random.Next(2);
            return new List<Gate> { new Gate(NextId(), good, goodSide, 1.1f), new Gate(NextId(), other, 1 - goodSide, 1.1f) };
        }

        GateOp GoodOp(int wave)
        {
            int k = random.Next(10);
            if (k <= 5) return new GateOp.Add(2 + random.Next(2 + wave));
            if (k <= 8) return new GateOp.Multiply(2);
            return new GateOp.Multiply(3);
        }

        GateOp BadOp() => random.Next(10) <= 6 ? new GateOp.Subtract(1 + random.Next(4)) : new GateOp.Divide(2);
    }
}
