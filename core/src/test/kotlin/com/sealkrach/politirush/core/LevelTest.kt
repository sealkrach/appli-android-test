package com.sealkrach.politirush.core

import org.junit.jupiter.api.Assertions.assertEquals
import org.junit.jupiter.api.Assertions.assertTrue
import org.junit.jupiter.api.Test

class LevelTest {
    @Test
    fun `chaque niveau a les trois rangs courants et une échelle de boss croissante`() {
        for (level in Levels.all) {
            val tiers = level.politicians.map { it.tier }.toSet()
            assertTrue(tiers.containsAll(listOf(Tier.HAUT_FONCTIONNAIRE, Tier.DEPUTE, Tier.MINISTRE)), level.id)
            assertTrue(level.bosses.isNotEmpty(), level.id)
            assertEquals(Tier.CHEF_ETAT, level.bosses.first().tier, "${level.id} commence par un chef d'État")
            assertTrue(level.bosses.all { it.isBoss }, level.id)
        }
    }

    @Test
    fun `les libellés de bonus suivent le thème`() {
        assertEquals("Ristourne", Levels.carburant.bonusLabel(BonusType.SONDAGE))
        assertEquals("Duty free", Levels.aeroport.bonusLabel(BonusType.MEETING))
        assertEquals(BonusType.SONDAGE.label, Levels.palais.bonusLabel(BonusType.SONDAGE))
    }

    @Test
    fun `le spawner d'un niveau ne produit que ses cibles`() {
        val sp = Spawner(seed = 11L, level = Levels.carburant)
        val ids = Levels.carburant.politicians.map { it.id }.toSet()
        val spawned = mutableListOf<Enemy>()
        repeat(2400) { spawned += sp.advance(1f / 60f, wave = 6, waveProgress = 0.5f, enemiesAlive = 0).enemies }
        assertTrue(spawned.isNotEmpty())
        assertTrue(spawned.filter { !it.type.isBoss }.all { it.type.id in ids })
    }

    @Test
    fun `une partie se joue sur un niveau donné`() {
        val e = GameEngine(Roster.hero("rambeau"), seed = 3L, level = Levels.aeroport)
        repeat(60 * 25) { e.step(1f / 60f, PlayerInput(targetX = 0.5f)) }
        assertEquals(Levels.aeroport, e.level)
        val names = e.state.enemies.map { it.type.id }.toSet()
        assertTrue(names.all { id -> Levels.aeroport.politicians.any { it.id == id } || Levels.aeroport.bosses.any { it.id == id } })
    }
}
