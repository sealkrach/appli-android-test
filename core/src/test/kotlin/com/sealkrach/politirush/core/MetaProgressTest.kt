package com.sealkrach.politirush.core

import org.junit.jupiter.api.Assertions.assertEquals
import org.junit.jupiter.api.Assertions.assertFalse
import org.junit.jupiter.api.Assertions.assertTrue
import org.junit.jupiter.api.Test

class MetaProgressTest {
    @Test
    fun `acheter une amélioration débite les pièces et monte le niveau`() {
        val m = MetaProgress(coins = 100)
        assertTrue(m.canBuy(Upgrade.CADENCE))
        val after = m.buy(Upgrade.CADENCE)
        assertEquals(1, after.level(Upgrade.CADENCE))
        assertEquals(50, after.coins)
        assertFalse(after.canBuy(Upgrade.CADENCE), "le niveau 2 coûte plus que 50")
        assertEquals(after, after.buy(Upgrade.CADENCE))
    }

    @Test
    fun `le coût croît avec le niveau et le niveau max est respecté`() {
        assertTrue(Upgrade.DEGATS.cost(3) > Upgrade.DEGATS.cost(2))
        val maxed = MetaProgress(coins = 1_000_000, levels = mapOf(Upgrade.VIES to Upgrade.VIES.maxLevel))
        assertFalse(maxed.canBuy(Upgrade.VIES))
    }

    @Test
    fun `les améliorations s'appliquent à l'état de départ`() {
        val m = MetaProgress(levels = mapOf(Upgrade.VIES to 2, Upgrade.MULTIPLICATEUR_DEPART to 3, Upgrade.CADENCE to 5))
        val s = m.startingState(Roster.hero("balbo"))
        assertEquals(GameConfig.HERO_MAX_HP + 2, s.heroHp)
        assertEquals(GameConfig.START_FIREPOWER + 3, s.firepower)
        assertTrue(s.hero.fireRate > Roster.hero("balbo").fireRate)
    }

    @Test
    fun `la fin de partie alimente la progression`() {
        val end = GameState(hero = Roster.hero("rambeau"), score = 1234, coins = 30, wave = 4)
        val m = MetaProgress(coins = 10, bestScore = 500).afterRun(end)
        assertEquals(40, m.coins)
        assertEquals(1234, m.bestScore)
        assertEquals(4, m.bestWave)
        assertEquals(1, m.runs)
    }
}
