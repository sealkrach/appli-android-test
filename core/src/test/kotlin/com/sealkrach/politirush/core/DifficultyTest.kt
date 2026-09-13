package com.sealkrach.politirush.core

import org.junit.jupiter.api.Assertions.assertEquals
import org.junit.jupiter.api.Assertions.assertTrue
import org.junit.jupiter.api.Test

class DifficultyTest {
    private val prefet = Roster.politicians.first { it.id == "prefet" }
    private val depute = Roster.politicians.first { it.id == "depute_base" }
    private val attal = Roster.politicians.first { it.id == "attal" }

    @Test
    fun `la résistance suit le rang et grimpe avec les vagues, plus vite pour les rangs élevés`() {
        assertTrue(Difficulty.hp(prefet, 1) < Difficulty.hp(depute, 1))
        assertTrue(Difficulty.hp(depute, 1) < Difficulty.hp(attal, 1))
        assertEquals(5, Difficulty.hp(attal, 1))
        assertEquals(14, Difficulty.hp(attal, 10))
        val gainPrefet = Difficulty.hp(prefet, 10) - Difficulty.hp(prefet, 1)
        val gainAttal = Difficulty.hp(attal, 10) - Difficulty.hp(attal, 1)
        assertTrue(gainAttal > gainPrefet)
        for (w in 1..30) assertTrue(Difficulty.hp(attal, w + 1) >= Difficulty.hp(attal, w))
    }

    @Test
    fun `le nombre monte avec l'avancée (cadence, groupes, plafond)`() {
        for (w in 1..30) {
            assertTrue(Difficulty.spawnInterval(w + 1) <= Difficulty.spawnInterval(w))
            assertTrue(Difficulty.groupSize(w + 1) >= Difficulty.groupSize(w))
            assertTrue(Difficulty.maxAlive(w + 1) >= Difficulty.maxAlive(w))
        }
        assertEquals(1, Difficulty.groupSize(1))
        assertEquals(2, Difficulty.groupSize(4))
        assertEquals(4, Difficulty.groupSize(10))
        assertEquals(4, Difficulty.groupSize(50))
        assertEquals(GameConfig.MIN_SPAWN_INTERVAL, Difficulty.spawnInterval(40))
        assertEquals(40, Difficulty.maxAlive(40))
    }

    @Test
    fun `le mélange des rangs bascule vers les rangs élevés`() {
        assertEquals(0, Difficulty.weight(Tier.DEPUTE, 1))
        assertEquals(0, Difficulty.weight(Tier.MINISTRE, 3))
        assertTrue(Difficulty.weight(Tier.MINISTRE, 4) > 0)
        assertTrue(Difficulty.weight(Tier.HAUT_FONCTIONNAIRE, 1) > Difficulty.weight(Tier.HAUT_FONCTIONNAIRE, 12))
        assertTrue(Difficulty.weight(Tier.MINISTRE, 12) > Difficulty.weight(Tier.MINISTRE, 4))
        assertTrue(Difficulty.weight(Tier.HAUT_FONCTIONNAIRE, 1) > Difficulty.weight(Tier.MINISTRE, 1))
        assertTrue(Difficulty.weight(Tier.MINISTRE, 12) >= Difficulty.weight(Tier.HAUT_FONCTIONNAIRE, 12))
        assertEquals(0, Difficulty.weight(Tier.CHEF_ETAT, 20), "les boss ne sortent jamais du tirage courant")
    }

    @Test
    fun `le spawner produit bien plus de cibles à la vague 10 qu'à la vague 1`() {
        fun countPerMinute(wave: Int): Int {
            val sp = Spawner(seed = 5L)
            var n = 0
            repeat(60 * 60) { n += sp.advance(1f / 60f, wave, waveProgress = 0.5f, enemiesAlive = 0).enemies.count { !it.type.isBoss } }
            return n
        }
        val early = countPerMinute(1)
        val late = countPerMinute(10)
        assertTrue(late > early * 3, "vague 1 : $early, vague 10 : $late")
    }

    @Test
    fun `les groupes respectent le plafond de cibles à l'écran`() {
        val sp = Spawner(seed = 9L)
        val batch = sp.advance(5f, wave = 12, waveProgress = 0.5f, enemiesAlive = Difficulty.maxAlive(12) - 1)
        assertTrue(batch.enemies.count { !it.type.isBoss } <= 1)
    }
}
