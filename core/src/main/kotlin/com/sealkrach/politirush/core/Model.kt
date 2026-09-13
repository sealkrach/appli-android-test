package com.sealkrach.politirush.core

/**
 * Modèle de données du jeu. Tout est immuable et indépendant d'Android
 * pour rester testable sur une JVM classique.
 *
 * Coordonnées : x dans [0, 1] (largeur de la piste), y dans [0, 1]
 * (0 = bas de l'écran / position du héros, 1 = haut / ligne d'apparition).
 */

/** Capacité spéciale d'un héros, déclenchée par un tap sur le bouton "spécial". */
enum class Special {
    /** Rafale : cadence de tir x3 pendant quelques secondes. */
    RAFALE,
    /** Uppercut : onde de choc qui repousse tout ce qui est proche du héros. */
    UPPERCUT,
    /** Lames : les projectiles traversent les cibles pendant quelques secondes. */
    LAMES,
    /** Gadget : gèle toutes les cibles à l'écran. */
    GADGET,
    /** Compétences très particulières : tir en éventail sur toute la largeur. */
    COMPETENCES_PARTICULIERES,
    /** Livraison : double le multiplicateur de tir courant (une fois). */
    LIVRAISON,
}

/**
 * Héros jouable. Les héros sont des pastiches de personnages de films d'action
 * (voir docs/GAME_DESIGN.md pour les inspirations) : noms et traits sont
 * volontairement fictifs pour éviter tout usage de marque.
 */
data class Hero(
    val id: String,
    val name: String,
    val tagline: String,
    /** Tirs par seconde. */
    val fireRate: Float,
    /** Dégâts par projectile. */
    val damage: Int,
    /** Vitesse de déplacement latéral (fraction de largeur par seconde). */
    val moveSpeed: Float,
    val special: Special,
    /** Durée de recharge du spécial en secondes. */
    val specialCooldown: Float,
)

/**
 * Rang d'une cible. La résistance, la vitesse et la récompense découlent du rang,
 * et le rang détermine à partir de quelle vague la cible apparaît.
 */
enum class Tier(val label: String, val hp: Int, val speed: Float, val points: Int, val coins: Int, val fromWave: Int, val spawnWeight: Int) {
    HAUT_FONCTIONNAIRE("Haut fonctionnaire", hp = 1, speed = 0.16f, points = 10, coins = 1, fromWave = 1, spawnWeight = 4),
    DEPUTE("Député", hp = 2, speed = 0.13f, points = 20, coins = 2, fromWave = 2, spawnWeight = 2),
    MINISTRE("Ministre", hp = 5, speed = 0.10f, points = 50, coins = 3, fromWave = 4, spawnWeight = 1),
    CHEF_ETAT("Chef d'État", hp = 60, speed = 0.04f, points = 500, coins = 25, fromWave = 5, spawnWeight = 0),
    HYPER_INFLUENT("Hyper-influent", hp = 120, speed = 0.035f, points = 1000, coins = 50, fromWave = 15, spawnWeight = 0),
}

/** Cible : une personnalité en caricature, avec son rang. */
data class PoliticianType(
    val id: String,
    val name: String,
    val tier: Tier,
    val hp: Int,
    /** Vitesse de descente (fraction de hauteur par seconde). */
    val speed: Float,
    val points: Int,
    /** Pièces lâchées à l'élimination. */
    val coins: Int,
    val isBoss: Boolean = false,
)

/** Opération appliquée au multiplicateur de tir quand le héros franchit une porte. */
sealed class GateOp {
    data class Multiply(val factor: Int) : GateOp()
    data class Add(val amount: Int) : GateOp()
    data class Subtract(val amount: Int) : GateOp()
    data class Divide(val divisor: Int) : GateOp()

    fun apply(value: Int): Int = when (this) {
        is Multiply -> value * factor
        is Add -> value + amount
        is Subtract -> value - amount
        is Divide -> value / divisor
    }.coerceIn(GameConfig.MIN_FIREPOWER, GameConfig.MAX_FIREPOWER)

    val label: String
        get() = when (this) {
            is Multiply -> "x$factor"
            is Add -> "+$amount"
            is Subtract -> "-$amount"
            is Divide -> "÷$divisor"
        }

    /** Une porte "bonne" augmente le multiplicateur ; sert au level design et au rendu. */
    val isGood: Boolean
        get() = this is Multiply || this is Add
}

/** Type de bonus ramassable sur la piste. */
enum class BonusType(val label: String, val durationSeconds: Float) {
    /** Sondage : tir en éventail. */
    SONDAGE("Sondage", 5f),
    /** Scandale : les cibles sont gelées. */
    SCANDALE("Scandale", 3f),
    /** Tomate géante : projectiles perçants. */
    TOMATE_GEANTE("Tomate géante", 6f),
    /** Motion de censure : élimine tout ce qui est à l'écran (instantané). */
    MOTION_DE_CENSURE("Motion de censure", 0f),
    /** Meeting : aimant à pièces. */
    MEETING("Meeting", 8f),
}

/** Une porte à franchir. Deux portes sont posées côte à côte à la même hauteur. */
data class Gate(
    val id: Int,
    val op: GateOp,
    /** Côté : 0 = gauche, 1 = droite. */
    val side: Int,
    val y: Float,
    val consumed: Boolean = false,
) {
    val xMin: Float get() = if (side == 0) 0f else 0.5f
    val xMax: Float get() = if (side == 0) 0.5f else 1f
}

data class Enemy(
    val id: Int,
    val type: PoliticianType,
    val x: Float,
    val y: Float,
    val hp: Int,
)

data class Projectile(
    val id: Int,
    val x: Float,
    val y: Float,
    /** Composante horizontale de la vitesse (tir en éventail). */
    val vx: Float,
    val damage: Int,
    val piercing: Boolean,
)

data class BonusPickup(
    val id: Int,
    val type: BonusType,
    val x: Float,
    val y: Float,
)

data class Coin(val id: Int, val x: Float, val y: Float)

/** Effet actif avec son temps restant. */
data class ActiveEffect(val type: BonusType, val remaining: Float)

enum class GameStatus { RUNNING, GAME_OVER }

/** Événement produit pendant un pas de simulation, utile pour le son, les vibrations et les particules. */
sealed class GameEvent {
    data class GatePassed(val op: GateOp, val newFirepower: Int) : GameEvent()
    data class EnemyHit(val enemyId: Int, val x: Float, val y: Float) : GameEvent()
    data class EnemyKilled(val type: PoliticianType, val x: Float, val y: Float, val combo: Int) : GameEvent()
    data class BonusPicked(val type: BonusType) : GameEvent()
    data class CoinPicked(val total: Int) : GameEvent()
    data class SpecialUsed(val special: Special) : GameEvent()
    data class WaveStarted(val wave: Int) : GameEvent()
    data object HeroHurt : GameEvent()
    data object GameOver : GameEvent()
}

/** État complet d'une partie. */
data class GameState(
    val hero: Hero,
    val heroX: Float = 0.5f,
    val heroHp: Int = GameConfig.HERO_MAX_HP,
    val firepower: Int = GameConfig.START_FIREPOWER,
    val wave: Int = 1,
    val score: Int = 0,
    val coins: Int = 0,
    val combo: Int = 0,
    /** Temps depuis la dernière élimination, pour faire retomber le combo. */
    val comboTimer: Float = 0f,
    val elapsed: Float = 0f,
    val distance: Float = 0f,
    val enemies: List<Enemy> = emptyList(),
    val gates: List<Gate> = emptyList(),
    val projectiles: List<Projectile> = emptyList(),
    val bonuses: List<BonusPickup> = emptyList(),
    val coinsOnTrack: List<Coin> = emptyList(),
    val effects: List<ActiveEffect> = emptyList(),
    /** Temps accumulé depuis le dernier tir. */
    val fireTimer: Float = 0f,
    val specialCooldownRemaining: Float = 0f,
    val specialActiveRemaining: Float = 0f,
    val status: GameStatus = GameStatus.RUNNING,
) {
    fun hasEffect(type: BonusType): Boolean = effects.any { it.type == type }
    val weapon: Weapon get() = Weapon.forFirepower(firepower)
    val specialReady: Boolean get() = specialCooldownRemaining <= 0f && status == GameStatus.RUNNING
}

/** Entrées du joueur pour un pas de simulation. */
data class PlayerInput(
    /** Position latérale visée dans [0, 1] (doigt sur l'écran). Null = ne bouge pas. */
    val targetX: Float? = null,
    val useSpecial: Boolean = false,
)
