package com.sealkrach.politirush.core

import org.junit.jupiter.api.Assertions.assertEquals
import org.junit.jupiter.api.Assertions.assertTrue
import org.junit.jupiter.api.Test

class RosterTest {
    @Test
    fun `l'arme suit la puissance`() {
        assertEquals(Weapon.MAIN, Weapon.forFirepower(1))
        assertEquals(Weapon.MAIN, Weapon.forFirepower(4))
        assertEquals(Weapon.LANCE_PIERRE, Weapon.forFirepower(5))
        assertEquals(Weapon.CANON, Weapon.forFirepower(13))
        assertEquals(Weapon.TANK, Weapon.forFirepower(33))
        assertEquals(Weapon.TANK, Weapon.forFirepower(GameConfig.MAX_FIREPOWER))
        assertEquals(Weapon.LANCE_PIERRE, GameState(hero = Roster.hero("rambeau"), firepower = 8).weapon)
    }

    @Test
    fun `la résistance suit le rang réel`() {
        val hp = { id: String -> Roster.politicians.first { it.id == id }.hp }
        assertTrue(hp("prefet") < hp("depute_base"))
        assertTrue(hp("depute_base") < hp("attal"))
        assertEquals(hp("attal"), hp("filippetti"), "deux ministres ont la même résistance")
        assertTrue(Roster.bosses.first { it.id == "macron" }.hp > hp("attal"))
        assertTrue(Roster.bosses.first { it.id == "musk" }.hp > Roster.bosses.first { it.id == "macron" }.hp)
    }

    @Test
    fun `les chefs d'État précèdent les hyper-influents dans l'échelle des boss`() {
        val tiers = Roster.bosses.map { it.tier }
        val lastHead = tiers.lastIndexOf(Tier.CHEF_ETAT)
        val firstTop = tiers.indexOf(Tier.HYPER_INFLUENT)
        assertTrue(lastHead < firstTop)
        assertEquals(Tier.CHEF_ETAT, Roster.bossForWave(5).tier)
        assertEquals(Tier.CHEF_ETAT, Roster.bossForWave(10).tier)
        assertEquals(Tier.HYPER_INFLUENT, Roster.bossForWave(15).tier)
        assertEquals(Roster.bosses.last(), Roster.bossForWave(500), "le dernier boss se répète")
    }

    @Test
    fun `les ministres n'apparaissent pas dans les premières vagues`() {
        val sp = Spawner(seed = 3L)
        val early = mutableListOf<Enemy>()
        repeat(600) { early += sp.advance(1f / 60f, wave = 1, waveProgress = 0.5f, enemiesAlive = 0).enemies }
        assertTrue(early.isNotEmpty())
        assertTrue(early.all { it.type.tier == Tier.HAUT_FONCTIONNAIRE })
    }
}
