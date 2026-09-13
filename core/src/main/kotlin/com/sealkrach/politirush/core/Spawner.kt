package com.sealkrach.politirush.core

import kotlin.random.Random

/**
 * Génère portes, cibles, bonus au fil du temps. Le tirage est déterministe
 * pour une graine donnée, ce qui rend le moteur testable et permet des
 * "défis du jour" partagés (même graine pour tout le monde).
 */
open class Spawner(seed: Long, private val level: Level = Levels.palais) {
    private val random = Random(seed)
    private var nextId = 1

    private var gateTimer = 2f
    private var bonusTimer = 5f
    private var spawnTimer = 1f
    private var bossSpawnedForWave = 0

    fun nextId(): Int = nextId++

    /** Produit ce qui doit apparaître pendant `dt` secondes. */
    open fun advance(dt: Float, wave: Int, waveProgress: Float, enemiesAlive: Int): SpawnBatch {
        val gates = mutableListOf<Gate>()
        val enemies = mutableListOf<Enemy>()
        val bonuses = mutableListOf<BonusPickup>()

        gateTimer -= dt
        if (gateTimer <= 0f) {
            gateTimer += GameConfig.GATE_INTERVAL
            gates += gatePair(wave)
        }

        bonusTimer -= dt
        if (bonusTimer <= 0f) {
            bonusTimer += GameConfig.BONUS_INTERVAL
            bonuses += BonusPickup(nextId(), BonusType.entries.random(random), random.nextFloat() * 0.8f + 0.1f, 1.05f)
        }

        val isBossWave = wave % GameConfig.BOSS_EVERY_N_WAVES == 0
        if (isBossWave && waveProgress > 0.3f && bossSpawnedForWave != wave) {
            bossSpawnedForWave = wave
            val boss = level.bossForWave(wave)
            enemies += Enemy(nextId(), boss, 0.5f, 1.1f, Difficulty.bossHp(boss, wave))
        }

        spawnTimer -= dt
        val interval = Difficulty.spawnInterval(wave)
        while (spawnTimer <= 0f) {
            spawnTimer += interval
            // Les rangs élevés arrivent avec les vagues et deviennent de plus en plus fréquents ;
            // les groupes grossissent, dans la limite du plafond de cibles à l'écran.
            val pool = level.politicians.flatMap { t -> List(Difficulty.weight(t.tier, wave)) { t } }
            if (pool.isEmpty()) continue
            val group = Difficulty.groupSize(wave)
            val center = random.nextFloat() * 0.8f + 0.1f
            repeat(group) { i ->
                if (enemiesAlive + enemies.size >= Difficulty.maxAlive(wave)) return@repeat
                val type = pool.random(random)
                val x = (center + (i - (group - 1) / 2f) * 0.14f + (random.nextFloat() - 0.5f) * 0.06f).coerceIn(0.05f, 0.95f)
                enemies += Enemy(nextId(), type, x, 1.05f + i * 0.03f, Difficulty.hp(type, wave))
            }
        }

        return SpawnBatch(gates, enemies, bonuses)
    }

    /** Une paire de portes : au moins une bonne, la tentation étant le cœur du genre. */
    private fun gatePair(wave: Int): List<Gate> {
        val good = goodOp(wave)
        val other = if (random.nextFloat() < 0.65f) badOp() else goodOp(wave)
        val goodSide = random.nextInt(2)
        return listOf(
            Gate(nextId(), good, goodSide, 1.1f),
            Gate(nextId(), other, 1 - goodSide, 1.1f),
        )
    }

    private fun goodOp(wave: Int): GateOp = when (random.nextInt(10)) {
        in 0..5 -> GateOp.Add(2 + random.nextInt(2 + wave))
        in 6..8 -> GateOp.Multiply(2)
        else -> GateOp.Multiply(3)
    }

    private fun badOp(): GateOp = when (random.nextInt(10)) {
        in 0..6 -> GateOp.Subtract(1 + random.nextInt(4))
        else -> GateOp.Divide(2)
    }
}

data class SpawnBatch(
    val gates: List<Gate>,
    val enemies: List<Enemy>,
    val bonuses: List<BonusPickup>,
)
