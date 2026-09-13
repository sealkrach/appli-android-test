package com.sealkrach.politirush.core

import kotlin.math.abs
import kotlin.math.hypot
import kotlin.math.sign

/**
 * Moteur de simulation : un pas de temps `dt` transforme un [GameState] en un
 * nouveau [GameState] et une liste d'événements. Aucune dépendance Android.
 *
 * Boucle : le héros est en bas, tire automatiquement vers le haut. Les portes
 * (x2, +5, -3, ÷2...) descendent vers lui et modifient son multiplicateur de
 * tir selon le côté qu'il choisit. Les cibles descendent et doivent être
 * éliminées avant d'atteindre le héros.
 */
class GameEngine(
    initialState: GameState,
    seed: Long = 42L,
    private val spawner: Spawner = Spawner(seed),
) {
    constructor(hero: Hero, seed: Long = 42L) : this(GameState(hero = hero), seed)

    var state: GameState = initialState
        private set

    fun step(dt: Float, input: PlayerInput = PlayerInput()): List<GameEvent> {
        val (next, events) = simulate(state, dt, input)
        state = next
        return events
    }

    fun simulate(s0: GameState, dt: Float, input: PlayerInput): Pair<GameState, List<GameEvent>> {
        if (s0.status == GameStatus.GAME_OVER || dt <= 0f) return s0 to emptyList()
        val events = mutableListOf<GameEvent>()
        var s = s0

        // 1. Temps, vagues.
        val elapsed = s.elapsed + dt
        val wave = (elapsed / GameConfig.WAVE_DURATION).toInt() + 1
        if (wave != s.wave) events += GameEvent.WaveStarted(wave)
        s = s.copy(
            elapsed = elapsed,
            wave = wave,
            distance = s.distance + GameConfig.SCROLL_SPEED * dt,
        )

        // 2. Déplacement du héros.
        input.targetX?.let { target ->
            val t = target.coerceIn(0f, 1f)
            val delta = t - s.heroX
            val maxMove = s.hero.moveSpeed * dt
            val x = if (abs(delta) <= maxMove) t else s.heroX + sign(delta) * maxMove
            s = s.copy(heroX = x)
        }

        // 3. Effets et recharges.
        s = s.copy(
            effects = s.effects.map { it.copy(remaining = it.remaining - dt) }.filter { it.remaining > 0f },
            specialCooldownRemaining = (s.specialCooldownRemaining - dt).coerceAtLeast(0f),
            specialActiveRemaining = (s.specialActiveRemaining - dt).coerceAtLeast(0f),
            comboTimer = s.comboTimer + dt,
        )
        if (s.combo > 0 && s.comboTimer > GameConfig.COMBO_WINDOW) s = s.copy(combo = 0)

        // 4. Spécial.
        if (input.useSpecial && s.specialReady) {
            s = useSpecial(s)
            events += GameEvent.SpecialUsed(s.hero.special)
        }

        // 5. Apparitions.
        val waveProgress = (elapsed % GameConfig.WAVE_DURATION) / GameConfig.WAVE_DURATION
        val batch = spawner.advance(dt, wave, waveProgress, s.enemies.size)
        s = s.copy(
            gates = s.gates + batch.gates,
            enemies = s.enemies + batch.enemies,
            bonuses = s.bonuses + batch.bonuses,
        )

        // 6. Défilement.
        val scroll = GameConfig.SCROLL_SPEED * dt
        val frozen = s.hasEffect(BonusType.SCANDALE) ||
            (s.hero.special == Special.GADGET && s.specialActiveRemaining > 0f)
        val magnet = s.hasEffect(BonusType.MEETING)
        s = s.copy(
            gates = s.gates.map { it.copy(y = it.y - scroll) }.filter { it.y > -0.1f },
            bonuses = s.bonuses.map { it.copy(y = it.y - scroll) }.filter { it.y > -0.1f },
            enemies = if (frozen) s.enemies else s.enemies.map { it.copy(y = it.y - it.type.speed * dt) },
            coinsOnTrack = s.coinsOnTrack.map { c ->
                if (magnet) {
                    val dx = s.heroX - c.x
                    val dy = GameConfig.HERO_Y - c.y
                    val d = hypot(dx, dy).coerceAtLeast(1e-4f)
                    val step = 2.0f * dt
                    if (d <= step) c.copy(x = s.heroX, y = GameConfig.HERO_Y)
                    else c.copy(x = c.x + dx / d * step, y = c.y + dy / d * step)
                } else c.copy(y = c.y - scroll)
            }.filter { it.y > -0.1f },
        )

        // 7. Portes franchies.
        val gates = s.gates.map { g ->
            if (!g.consumed && g.y <= GameConfig.HERO_Y) {
                if (s.heroX >= g.xMin && s.heroX < g.xMax) {
                    val fp = g.op.apply(s.firepower)
                    s = s.copy(firepower = fp)
                    events += GameEvent.GatePassed(g.op, fp)
                }
                g.copy(consumed = true)
            } else g
        }
        s = s.copy(gates = gates)

        // 8. Ramassage : bonus et pièces (avant les collisions pour que la motion
        //    de censure retire les cibles dans ce même pas de simulation).
        val pickRadius = GameConfig.HERO_RADIUS + 0.03f
        val (picked, keptBonuses) = s.bonuses.partition { hypot(it.x - s.heroX, it.y - GameConfig.HERO_Y) <= pickRadius }
        s = s.copy(bonuses = keptBonuses)
        for (b in picked) {
            s = applyBonus(s, b.type)
            events += GameEvent.BonusPicked(b.type)
        }
        val (coinsPicked, keptCoins) = s.coinsOnTrack.partition { hypot(it.x - s.heroX, it.y - GameConfig.HERO_Y) <= pickRadius }
        if (coinsPicked.isNotEmpty()) {
            s = s.copy(coins = s.coins + coinsPicked.size, coinsOnTrack = keptCoins)
            events += GameEvent.CoinPicked(s.coins)
        }

        // 9. Tir automatique.
        val rafale = s.hero.special == Special.RAFALE && s.specialActiveRemaining > 0f
        val fireRate = s.hero.fireRate * (if (rafale) GameConfig.RAFALE_FACTOR else 1f)
        val piercing = s.hasEffect(BonusType.TOMATE_GEANTE) ||
            (s.hero.special == Special.LAMES && s.specialActiveRemaining > 0f)
        val fan = s.hasEffect(BonusType.SONDAGE) ||
            (s.hero.special == Special.COMPETENCES_PARTICULIERES && s.specialActiveRemaining > 0f)
        var fireTimer = s.fireTimer + dt
        val interval = 1f / fireRate
        val newProjectiles = mutableListOf<Projectile>()
        while (fireTimer >= interval) {
            fireTimer -= interval
            newProjectiles += salvo(s, piercing, fan)
        }
        val projectiles = (s.projectiles + newProjectiles).takeLast(GameConfig.MAX_PROJECTILES)
        s = s.copy(fireTimer = fireTimer, projectiles = projectiles)

        // 10. Projectiles : déplacement et collisions.
        val enemyMap = s.enemies.associateBy { it.id }.toMutableMap()
        val remainingProjectiles = mutableListOf<Projectile>()
        for (p0 in s.projectiles) {
            val p = p0.copy(y = p0.y + GameConfig.PROJECTILE_SPEED * dt, x = p0.x + p0.vx * dt)
            if (p.y > 1.1f || p.x < -0.05f || p.x > 1.05f) continue
            val hit = enemyMap.values.firstOrNull { e ->
                e.hp > 0 && hypot(e.x - p.x, e.y - p.y) <= GameConfig.ENEMY_RADIUS + GameConfig.PROJECTILE_RADIUS
            }
            if (hit == null) {
                remainingProjectiles += p
                continue
            }
            val damaged = hit.copy(hp = hit.hp - p.damage)
            enemyMap[hit.id] = damaged
            events += GameEvent.EnemyHit(hit.id, hit.x, hit.y)
            if (p.piercing) remainingProjectiles += p
        }
        s = s.copy(projectiles = remainingProjectiles)

        // 11. Éliminations, score, pièces.
        var score = s.score
        var combo = s.combo
        var comboTimer = s.comboTimer
        val newCoins = mutableListOf<Coin>()
        val alive = mutableListOf<Enemy>()
        for (e in enemyMap.values) {
            if (e.hp <= 0) {
                combo += 1
                comboTimer = 0f
                score += e.type.points * comboMultiplier(combo)
                repeat(e.type.coins) { i ->
                    newCoins += Coin(spawner.nextId(), (e.x + (i - e.type.coins / 2) * 0.03f).coerceIn(0f, 1f), e.y)
                }
                events += GameEvent.EnemyKilled(e.type, e.x, e.y, combo)
            } else alive += e
        }
        s = s.copy(score = score, combo = combo, comboTimer = comboTimer, enemies = alive, coinsOnTrack = s.coinsOnTrack + newCoins)

        // 12. Cibles arrivées sur le héros.
        val reached = s.enemies.filter { it.y <= GameConfig.HERO_Y + GameConfig.ENEMY_RADIUS }
        if (reached.isNotEmpty()) {
            val hp = s.heroHp - reached.size
            s = s.copy(enemies = s.enemies - reached.toSet(), heroHp = hp.coerceAtLeast(0), combo = 0)
            repeat(reached.size) { events += GameEvent.HeroHurt }
            if (hp <= 0) {
                events += GameEvent.GameOver
                return s.copy(status = GameStatus.GAME_OVER) to events
            }
        }

        return s to events
    }

    private fun comboMultiplier(combo: Int): Int = 1 + combo / GameConfig.COMBO_STEP

    /** Génère les projectiles d'une salve : `firepower` tomates réparties en éventail. */
    private fun salvo(s: GameState, piercing: Boolean, fan: Boolean): List<Projectile> {
        val n = s.firepower
        val width = if (fan) 1f else GameConfig.SPREAD_WIDTH
        return List(n) { i ->
            val t = if (n == 1) 0f else (i.toFloat() / (n - 1)) - 0.5f
            val x = if (fan) 0.5f + t * width else (s.heroX + t * width).coerceIn(0.02f, 0.98f)
            Projectile(
                id = spawner.nextId(),
                x = x,
                y = GameConfig.HERO_Y,
                vx = if (fan) t * 0.6f else 0f,
                damage = s.hero.damage,
                piercing = piercing,
            )
        }
    }

    private fun useSpecial(s: GameState): GameState {
        val base = s.copy(specialCooldownRemaining = s.hero.specialCooldown)
        return when (s.hero.special) {
            Special.RAFALE -> base.copy(specialActiveRemaining = GameConfig.RAFALE_DURATION)
            Special.LAMES -> base.copy(specialActiveRemaining = GameConfig.LAMES_DURATION)
            Special.GADGET -> base.copy(specialActiveRemaining = GameConfig.GADGET_DURATION)
            Special.COMPETENCES_PARTICULIERES -> base.copy(specialActiveRemaining = GameConfig.COMPETENCES_DURATION)
            Special.LIVRAISON -> base.copy(firepower = GateOp.Multiply(2).apply(s.firepower))
            Special.UPPERCUT -> base.copy(
                enemies = s.enemies.map { e ->
                    val d = hypot(e.x - s.heroX, e.y - GameConfig.HERO_Y)
                    if (d <= GameConfig.UPPERCUT_RADIUS) e.copy(hp = e.hp - 10, y = (e.y + 0.3f).coerceAtMost(1.05f)) else e
                },
            )
        }
    }

    private fun applyBonus(s: GameState, type: BonusType): GameState = when (type) {
        BonusType.MOTION_DE_CENSURE -> s.copy(
            enemies = s.enemies.map { e ->
                if (e.type.isBoss) e.copy(hp = e.hp - (e.type.hp * 0.3f).toInt()) else e.copy(hp = 0)
            },
        )
        else -> s.copy(effects = s.effects.filter { it.type != type } + ActiveEffect(type, type.durationSeconds))
    }
}
