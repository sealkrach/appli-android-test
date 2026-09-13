package com.sealkrach.politirush.core

/** Constantes d'équilibrage. Tout ce qui se règle "au feeling" est ici. */
object GameConfig {
    const val HERO_MAX_HP = 3
    const val START_FIREPOWER = 1
    const val MIN_FIREPOWER = 1
    const val MAX_FIREPOWER = 64

    /** Hauteur à laquelle le héros se tient (y). */
    const val HERO_Y = 0.08f
    /** Rayon de collision du héros (pour les pièces et bonus). */
    const val HERO_RADIUS = 0.06f
    /** Rayon de collision d'une cible. */
    const val ENEMY_RADIUS = 0.05f
    /** Rayon de collision d'un projectile. */
    const val PROJECTILE_RADIUS = 0.015f
    /** Vitesse verticale des projectiles (fraction de hauteur / s). */
    const val PROJECTILE_SPEED = 1.4f
    /** Nombre maximum de projectiles simultanés (garde-fou performance). */
    const val MAX_PROJECTILES = 300
    /** Largeur totale de l'éventail de tir, en fraction de largeur, pour firepower projectiles. */
    const val SPREAD_WIDTH = 0.35f

    /** Vitesse de défilement de la piste (portes, bonus, pièces). */
    const val SCROLL_SPEED = 0.35f
    /** Intervalle entre deux paires de portes, en secondes. */
    const val GATE_INTERVAL = 6f
    /** Intervalle entre deux bonus, en secondes. */
    const val BONUS_INTERVAL = 11f

    /** Durée d'une vague en secondes. */
    const val WAVE_DURATION = 20f
    /** Intervalle de base entre deux apparitions de cibles (diminue avec les vagues). */
    const val BASE_SPAWN_INTERVAL = 1.1f
    const val MIN_SPAWN_INTERVAL = 0.25f
    /** Une vague sur N se termine par un boss. */
    const val BOSS_EVERY_N_WAVES = 5

    /** Temps sans élimination avant que le combo retombe à zéro. */
    const val COMBO_WINDOW = 2.5f
    /** Multiplicateur de score par palier de combo (chaque tranche de 10). */
    const val COMBO_STEP = 10

    /** Multiplicateur de cadence pendant la rafale. */
    const val RAFALE_FACTOR = 3f
    const val RAFALE_DURATION = 4f
    const val UPPERCUT_RADIUS = 0.35f
    const val LAMES_DURATION = 5f
    const val GADGET_DURATION = 4f
    const val COMPETENCES_DURATION = 6f
}
