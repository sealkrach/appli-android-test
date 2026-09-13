package com.sealkrach.politirush.core

/**
 * Progression entre les parties : les pièces gagnées achètent des améliorations
 * permanentes. C'est la boucle "encore une partie" du genre.
 */
enum class Upgrade(val label: String, val baseCost: Int, val maxLevel: Int) {
    CADENCE("Cadence de tir", baseCost = 50, maxLevel = 10),
    DEGATS("Dégâts", baseCost = 80, maxLevel = 10),
    MULTIPLICATEUR_DEPART("Multiplicateur de départ", baseCost = 120, maxLevel = 5),
    VIES("Vies", baseCost = 200, maxLevel = 3),
    RECHARGE_SPECIAL("Recharge du spécial", baseCost = 100, maxLevel = 5);

    /** Coût du prochain niveau, croissance géométrique douce. */
    fun cost(currentLevel: Int): Int = (baseCost * Math.pow(1.6, currentLevel.toDouble())).toInt()
}

data class MetaProgress(
    val coins: Int = 0,
    val bestScore: Int = 0,
    val bestWave: Int = 0,
    val runs: Int = 0,
    val levels: Map<Upgrade, Int> = emptyMap(),
    val unlockedHeroes: Set<String> = setOf("rambeau"),
) {
    fun level(u: Upgrade): Int = levels[u] ?: 0

    fun canBuy(u: Upgrade): Boolean = level(u) < u.maxLevel && coins >= u.cost(level(u))

    fun buy(u: Upgrade): MetaProgress {
        if (!canBuy(u)) return this
        val lvl = level(u)
        return copy(coins = coins - u.cost(lvl), levels = levels + (u to lvl + 1))
    }

    /** Enregistre le résultat d'une partie. */
    fun afterRun(finalState: GameState): MetaProgress = copy(
        coins = coins + finalState.coins,
        bestScore = maxOf(bestScore, finalState.score),
        bestWave = maxOf(bestWave, finalState.wave),
        runs = runs + 1,
    )

    /** Applique les améliorations permanentes à un héros du catalogue. */
    fun applyTo(hero: Hero): Hero = hero.copy(
        fireRate = hero.fireRate * (1f + 0.08f * level(Upgrade.CADENCE)),
        damage = hero.damage + level(Upgrade.DEGATS) / 2,
        specialCooldown = hero.specialCooldown * (1f - 0.08f * level(Upgrade.RECHARGE_SPECIAL)),
    )

    fun startingState(hero: Hero): GameState = GameState(
        hero = applyTo(hero),
        heroHp = GameConfig.HERO_MAX_HP + level(Upgrade.VIES),
        firepower = GameConfig.START_FIREPOWER + level(Upgrade.MULTIPLICATEUR_DEPART),
    )
}
