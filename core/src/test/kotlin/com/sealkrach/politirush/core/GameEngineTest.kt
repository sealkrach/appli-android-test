package com.sealkrach.politirush.core

import org.junit.jupiter.api.Assertions.assertEquals
import org.junit.jupiter.api.Assertions.assertTrue
import org.junit.jupiter.api.Test

class GameEngineTest {
    private val hero = Roster.hero("rambeau")

    /** Spawner inerte : aucune apparition, pour isoler la mécanique testée. */
    private class SilentSpawner : Spawner(seed = 0L) {
        override fun advance(dt: Float, wave: Int, waveProgress: Float, enemiesAlive: Int) =
            SpawnBatch(emptyList(), emptyList(), emptyList())
    }

    private fun engine(state: GameState = GameState(hero = hero)) = GameEngine(state, spawner = SilentSpawner())

    private fun run(e: GameEngine, seconds: Float, input: PlayerInput = PlayerInput(), dt: Float = 1f / 60f): List<GameEvent> {
        val events = mutableListOf<GameEvent>()
        var t = 0f
        while (t < seconds) {
            events += e.step(dt, input)
            t += dt
        }
        return events
    }

    @Test
    fun `le héros se déplace vers la position visée sans dépasser sa vitesse`() {
        val e = engine()
        e.step(0.1f, PlayerInput(targetX = 1f))
        assertEquals(0.5f + hero.moveSpeed * 0.1f, e.state.heroX, 1e-4f)
        run(e, 2f, PlayerInput(targetX = 1f))
        assertEquals(1f, e.state.heroX, 1e-4f)
    }

    @Test
    fun `franchir une porte du bon côté applique son opération`() {
        val gates = listOf(
            Gate(1, GateOp.Multiply(3), side = 0, y = GameConfig.HERO_Y + 0.01f),
            Gate(2, GateOp.Subtract(1), side = 1, y = GameConfig.HERO_Y + 0.01f),
        )
        val e = engine(GameState(hero = hero, heroX = 0.25f, firepower = 2, gates = gates))
        val events = e.step(0.1f)
        assertEquals(6, e.state.firepower)
        assertEquals(1, events.filterIsInstance<GameEvent.GatePassed>().size)
        assertTrue(e.state.gates.all { it.consumed })
        // Une porte consommée ne s'applique pas une seconde fois.
        e.step(0.1f)
        assertEquals(6, e.state.firepower)
    }

    @Test
    fun `une salve contient autant de projectiles que le multiplicateur`() {
        val e = engine(GameState(hero = hero, firepower = 5))
        e.step(1f / hero.fireRate + 0.001f)
        assertEquals(5, e.state.projectiles.size)
    }

    @Test
    fun `éliminer une cible rapporte score, combo et pièces`() {
        val type = Roster.politicians.first()
        val enemy = Enemy(1, type, x = 0.5f, y = 0.3f, hp = 1)
        val e = engine(GameState(hero = hero, heroX = 0.5f, enemies = listOf(enemy)))
        val events = run(e, 1f)
        val kill = events.filterIsInstance<GameEvent.EnemyKilled>().single()
        assertEquals(type, kill.type)
        assertEquals(1, e.state.combo)
        assertEquals(type.points, e.state.score)
        assertTrue(e.state.enemies.isEmpty())
        assertEquals(type.coins, e.state.coinsOnTrack.size + e.state.coins)
    }

    @Test
    fun `le combo retombe après la fenêtre d'inactivité`() {
        val e = engine(GameState(hero = hero, combo = 7))
        run(e, GameConfig.COMBO_WINDOW + 0.2f)
        assertEquals(0, e.state.combo)
    }

    @Test
    fun `une cible qui atteint le héros lui retire une vie puis termine la partie`() {
        val type = Roster.politicians.first()
        val e = engine(GameState(hero = hero, heroX = 0f, heroHp = 1, enemies = listOf(Enemy(1, type, 1f, GameConfig.HERO_Y + 0.01f, 999))))
        val events = run(e, 1f)
        assertTrue(events.contains(GameEvent.HeroHurt))
        assertTrue(events.contains(GameEvent.GameOver))
        assertEquals(GameStatus.GAME_OVER, e.state.status)
        assertEquals(0, e.state.heroHp)
        // Le moteur ne bouge plus une fois la partie finie.
        val frozen = e.state
        e.step(1f)
        assertEquals(frozen, e.state)
    }

    @Test
    fun `le bonus scandale gèle les cibles`() {
        val type = Roster.politicians.first()
        val enemy = Enemy(1, type, x = 0.9f, y = 0.8f, hp = 999)
        val bonus = BonusPickup(2, BonusType.SCANDALE, x = 0.5f, y = GameConfig.HERO_Y)
        val e = engine(GameState(hero = hero, heroX = 0.5f, enemies = listOf(enemy), bonuses = listOf(bonus)))
        val events = e.step(1f / 60f)
        assertTrue(events.contains(GameEvent.BonusPicked(BonusType.SCANDALE)))
        assertTrue(e.state.hasEffect(BonusType.SCANDALE))
        val yBefore = e.state.enemies.single().y
        run(e, 1f)
        assertEquals(yBefore, e.state.enemies.single().y, 1e-5f)
        run(e, BonusType.SCANDALE.durationSeconds + 0.5f)
        assertTrue(e.state.enemies.single().y < yBefore)
    }

    @Test
    fun `la motion de censure élimine tout sauf les boss`() {
        val minion = Enemy(1, Roster.politicians[0], 0.2f, 0.7f, 5)
        val boss = Enemy(2, Roster.bosses[0], 0.8f, 0.9f, Roster.bosses[0].hp)
        val bonus = BonusPickup(3, BonusType.MOTION_DE_CENSURE, 0.5f, GameConfig.HERO_Y)
        val e = engine(GameState(hero = hero, heroX = 0.5f, enemies = listOf(minion, boss), bonuses = listOf(bonus)))
        e.step(1f / 60f)
        assertEquals(1, e.state.enemies.size)
        assertTrue(e.state.enemies.single().type.isBoss)
        assertTrue(e.state.enemies.single().hp < Roster.bosses[0].hp)
    }

    @Test
    fun `le spécial livraison double le multiplicateur et passe en recharge`() {
        val h = Roster.hero("transporteur")
        val e = GameEngine(GameState(hero = h, firepower = 4), spawner = SilentSpawner())
        val events = e.step(1f / 60f, PlayerInput(useSpecial = true))
        assertTrue(events.contains(GameEvent.SpecialUsed(Special.LIVRAISON)))
        assertEquals(8, e.state.firepower)
        assertTrue(!e.state.specialReady)
        // Réutiliser pendant la recharge n'a aucun effet.
        e.step(1f / 60f, PlayerInput(useSpecial = true))
        assertEquals(8, e.state.firepower)
    }

    @Test
    fun `le spécial rafale triple la cadence`() {
        val e1 = engine(GameState(hero = hero, firepower = 1))
        val e2 = engine(GameState(hero = hero, firepower = 1))
        run(e1, 1f)
        run(e2, 1f, PlayerInput(useSpecial = true))
        assertTrue(e2.state.projectiles.size > e1.state.projectiles.size * 2)
    }

    @Test
    fun `une partie complète avec le vrai spawner est déterministe`() {
        fun play(seed: Long): GameState {
            val e = GameEngine(hero, seed = seed)
            var t = 0f
            while (e.state.status == GameStatus.RUNNING && t < 120f) {
                // Le joueur suit toujours la porte "bonne" la plus proche.
                val target = e.state.gates.filter { !it.consumed }.minByOrNull { it.y }
                    ?.let { g -> if (g.op.isGood) (g.xMin + g.xMax) / 2 else 1f - (g.xMin + g.xMax) / 2 }
                e.step(1f / 60f, PlayerInput(targetX = target, useSpecial = true))
                t += 1f / 60f
            }
            return e.state
        }
        val a = play(7L)
        val b = play(7L)
        assertEquals(a.score, b.score)
        assertEquals(a.wave, b.wave)
        assertTrue(a.score > 0, "la partie devrait rapporter des points")
        assertTrue(a.firepower >= GameConfig.MIN_FIREPOWER && a.firepower <= GameConfig.MAX_FIREPOWER)
        assertTrue(a.projectiles.size <= GameConfig.MAX_PROJECTILES)
    }
}
