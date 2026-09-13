package com.sealkrach.politirush.core

import kotlin.random.Random

/**
 * Génère portes, cibles, bonus au fil du temps. Le tirage est déterministe
 * pour une graine donnée, ce qui rend le moteur testable et permet des
 * "défis du jour" partagés (même graine pour tout le monde).
 */
open class Spawner(seed: Long) {
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
            val boss = Roster.bosses[(wave / GameConfig.BOSS_EVERY_N_WAVES - 1) % Roster.bosses.size]
            enemies += Enemy(nextId(), boss, 0.5f, 1.1f, boss.hp + wave * 5)
        }

        spawnTimer -= dt
        val interval = (GameConfig.BASE_SPAWN_INTERVAL - wave * 0.06f).coerceAtLeast(GameConfig.MIN_SPAWN_INTERVAL)
        while (spawnTimer <= 0f) {
            spawnTimer += interval
            if (enemiesAlive + enemies.size < 40) {
                val pool = Roster.politicians.take((2 + wave).coerceAtMost(Roster.politicians.size))
                val type = pool.random(random)
                // Les vagues avancées ont des cibles plus résistantes.
                val hp = type.hp + (wave - 1) / 2
                enemies += Enemy(nextId(), type, random.nextFloat() * 0.9f + 0.05f, 1.05f, hp)
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
