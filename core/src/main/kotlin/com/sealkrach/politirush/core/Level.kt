package com.sealkrach.politirush.core

/**
 * Niveau à thème : un décor, ses cibles, son échelle de boss et des noms de
 * bonus adaptés. Les règles (portes, armes, vagues) sont communes à tous.
 */
data class Level(
    val id: String,
    val name: String,
    val subtitle: String,
    val politicians: List<PoliticianType>,
    /** Échelle des boss dans l'ordre d'apparition (vague 5, 10, 15...). Le dernier se répète. */
    val bosses: List<PoliticianType>,
    /** Libellés de bonus propres au thème ; les autres gardent le libellé par défaut. */
    val bonusLabels: Map<BonusType, String> = emptyMap(),
) {
    fun bossForWave(wave: Int): PoliticianType =
        bosses[(wave / GameConfig.BOSS_EVERY_N_WAVES - 1).coerceIn(0, bosses.lastIndex)]

    fun bonusLabel(type: BonusType): String = bonusLabels[type] ?: type.label
}

object Levels {
    private fun tier(id: String, name: String, tier: Tier) =
        PoliticianType(id, name, tier = tier, hp = tier.hp, speed = tier.speed, points = tier.points, coins = tier.coins)

    private fun boss(id: String, name: String, tier: Tier, hp: Int, points: Int, coins: Int) =
        PoliticianType(id, name, tier, hp = hp, speed = if (hp > 100) 0.035f else 0.04f, points = points, coins = coins, isBoss = true)

    private val macron = boss("macron", "Emmanuel Macron", Tier.CHEF_ETAT, hp = 60, points = 500, coins = 25)
    private val trump = boss("trump", "Donald Trump", Tier.HYPER_INFLUENT, hp = 160, points = 1300, coins = 60)
    private val musk = boss("musk", "Elon Musk", Tier.HYPER_INFLUENT, hp = 150, points = 1200, coins = 60)

    /** Niveau par défaut : toute la hiérarchie. Reprend le catalogue de [Roster]. */
    val palais = Level(
        id = "palais",
        name = "Le Palais",
        subtitle = "La hiérarchie au grand complet",
        politicians = Roster.politicians,
        bosses = Roster.bosses,
    )

    val carburant = Level(
        id = "carburant",
        name = "Carburant à 2 €",
        subtitle = "Le plein de taxes",
        politicians = listOf(
            tier("inspecteur_taxes", "L'Inspecteur des taxes", Tier.HAUT_FONCTIONNAIRE),
            tier("dir_dgec", "Le Directeur de la DGEC", Tier.HAUT_FONCTIONNAIRE),
            tier("conseillere_budget", "La Conseillère budgétaire", Tier.HAUT_FONCTIONNAIRE),
            tier("depute_taxe_carbone", "Le Député pro-taxe carbone", Tier.DEPUTE),
            tier("deputee_malus", "La Députée du malus", Tier.DEPUTE),
            tier("le_maire", "Bruno Le Maire", Tier.MINISTRE),
            tier("pannier_runacher", "Agnès Pannier-Runacher", Tier.MINISTRE),
            tier("bechu", "Christophe Béchu", Tier.MINISTRE),
            tier("borne", "Élisabeth Borne", Tier.MINISTRE),
        ),
        bosses = listOf(macron, boss("pouyanne", "Patrick Pouyanné", Tier.HYPER_INFLUENT, hp = 130, points = 1000, coins = 50), musk, trump),
        bonusLabels = mapOf(
            BonusType.SONDAGE to "Ristourne",
            BonusType.SCANDALE to "Blocage de raffinerie",
            BonusType.TOMATE_GEANTE to "Bidon géant",
            BonusType.MOTION_DE_CENSURE to "Gilets jaunes",
            BonusType.MEETING to "Chèque carburant",
        ),
    )

    val aeroport = Level(
        id = "aeroport",
        name = "L'aéroport bradé",
        subtitle = "Privatisation express",
        politicians = listOf(
            tier("hf_bercy", "Le Haut fonctionnaire de Bercy", Tier.HAUT_FONCTIONNAIRE),
            tier("expert_participations", "L'Expert des participations", Tier.HAUT_FONCTIONNAIRE),
            tier("dir_concessions", "La Directrice des concessions", Tier.HAUT_FONCTIONNAIRE),
            tier("depute_privatisation", "Le Député pro-privatisation", Tier.DEPUTE),
            tier("rapporteure_budget", "La Rapporteure du budget", Tier.DEPUTE),
            tier("le_maire", "Bruno Le Maire", Tier.MINISTRE),
            tier("borne", "Élisabeth Borne", Tier.MINISTRE),
            tier("djebbari", "Jean-Baptiste Djebbari", Tier.MINISTRE),
        ),
        bosses = listOf(macron, boss("huillard", "Xavier Huillard", Tier.HYPER_INFLUENT, hp = 120, points = 1000, coins = 50), musk, trump),
        bonusLabels = mapOf(
            BonusType.SONDAGE to "Enquête parlementaire",
            BonusType.SCANDALE to "Grève des contrôleurs",
            BonusType.TOMATE_GEANTE to "Tomate long-courrier",
            BonusType.MOTION_DE_CENSURE to "Référendum",
            BonusType.MEETING to "Duty free",
        ),
    )

    val all: List<Level> = listOf(palais, carburant, aeroport)

    fun byId(id: String): Level = all.first { it.id == id }
}
