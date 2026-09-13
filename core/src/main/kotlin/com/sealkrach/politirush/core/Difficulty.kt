package com.sealkrach.politirush.core

/**
 * Courbe de difficulté d'un niveau, en fonction de la vague.
 *
 * Deux axes, volontairement séparés :
 * - la **résistance** dépend du rang de la cible ([Tier.hp]) et grimpe avec les
 *   vagues, d'autant plus vite que le rang est élevé ;
 * - le **nombre** dépend de l'avancée : apparitions plus rapprochées, groupes
 *   plus gros, plafond de cibles à l'écran plus haut, et les rangs élevés
 *   deviennent de plus en plus fréquents.
 */
object Difficulty {

    /** Secondes entre deux apparitions. Vague 1 : 1,1 s. Plancher : 0,25 s (vague 13). */
    fun spawnInterval(wave: Int): Float =
        (GameConfig.BASE_SPAWN_INTERVAL - (wave - 1) * 0.07f).coerceAtLeast(GameConfig.MIN_SPAWN_INTERVAL)

    /** Cibles qui apparaissent ensemble. Vagues 1 à 3 : seules ; 4 à 6 : par deux ; 7 à 9 : par trois ; 10 et plus : par quatre. */
    fun groupSize(wave: Int): Int = (1 + (wave - 1) / 3).coerceAtMost(4)

    /** Plafond de cibles simultanées (garde-fou lisibilité et performance). */
    fun maxAlive(wave: Int): Int = (12 + wave * 3).coerceAtMost(40)

    /**
     * Poids d'apparition d'un rang à une vague donnée. 0 = n'apparaît pas encore.
     * Les hauts fonctionnaires dominent au début puis s'effacent ; les ministres
     * arrivent à la vague 4 et deviennent courants vers la vague 7.
     */
    fun weight(tier: Tier, wave: Int): Int = when (tier) {
        Tier.HAUT_FONCTIONNAIRE -> (6 - (wave - 1) / 2).coerceAtLeast(1)
        Tier.DEPUTE -> if (wave < Tier.DEPUTE.fromWave) 0 else (wave - 1).coerceAtMost(4)
        Tier.MINISTRE -> if (wave < Tier.MINISTRE.fromWave) 0 else (wave - 3).coerceAtMost(3)
        Tier.CHEF_ETAT, Tier.HYPER_INFLUENT -> 0
    }

    /**
     * Points de vie d'une cible courante à une vague donnée : la base du rang,
     * plus un bonus toutes les 3 vagues, multiplié par le rang (1, 2 ou 3).
     * Un ministre passe ainsi de 5 PV (vague 1) à 14 PV (vague 10).
     */
    fun hp(type: PoliticianType, wave: Int): Int =
        type.hp + ((wave - 1) / 3) * (type.tier.ordinal + 1)

    /** Points de vie d'un boss : sa base plus 5 par vague. */
    fun bossHp(type: PoliticianType, wave: Int): Int = type.hp + wave * 5
}
