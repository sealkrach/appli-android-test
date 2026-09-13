package com.sealkrach.politirush.core

/**
 * Catalogue des héros et des cibles.
 *
 * Héros : pastiches de personnages de films d'action, noms fictifs.
 * Cibles : personnalités publiques en caricature, classées par rang réel.
 * La résistance suit la hiérarchie : haut fonctionnaire < député < ministre,
 * puis les chefs d'État en boss et les hyper-influents en boss finaux.
 * Voir docs/GAME_DESIGN.md §3 pour le cadrage juridique.
 */
object Roster {

    val heroes: List<Hero> = listOf(
        Hero("rambeau", "Jean Rambeau", "Bandeau rouge, marcel, jamais de repos.", fireRate = 6f, damage = 1, moveSpeed = 1.2f, special = Special.RAFALE, specialCooldown = 14f),
        Hero("balbo", "Rocco Balbo", "Le boxeur de Philadelphie. Encaisse, puis frappe.", fireRate = 3f, damage = 3, moveSpeed = 1.0f, special = Special.UPPERCUT, specialCooldown = 12f),
        Hero("machete", "El Machette", "Il ne texte pas. Il tranche.", fireRate = 4f, damage = 2, moveSpeed = 1.1f, special = Special.LAMES, specialCooldown = 15f),
        Hero("chauve_souris", "Le Chevalier Chauve-Souris", "Milliardaire nocturne, ceinture pleine de gadgets.", fireRate = 4.5f, damage = 1, moveSpeed = 1.3f, special = Special.GADGET, specialCooldown = 16f),
        Hero("papa_particulier", "Le Papa Très Particulier", "Ex-agent. Il vous trouvera. Il vous tomatera.", fireRate = 5f, damage = 2, moveSpeed = 1.1f, special = Special.COMPETENCES_PARTICULIERES, specialCooldown = 15f),
        Hero("transporteur", "Le Transporteur Chauve", "Costume impeccable, règles strictes, livraison garantie.", fireRate = 5f, damage = 1, moveSpeed = 1.4f, special = Special.LIVRAISON, specialCooldown = 18f),
    )

    fun hero(id: String): Hero = heroes.first { it.id == id }

    private fun tier(id: String, name: String, tier: Tier) =
        PoliticianType(id, name, tier = tier, hp = tier.hp, speed = tier.speed, points = tier.points, coins = tier.coins)

    /** Cibles courantes. Les rangs 1 et 2 sont des archétypes, le rang 3 des ministres nommés. */
    val politicians: List<PoliticianType> = listOf(
        tier("prefet", "Le Préfet", Tier.HAUT_FONCTIONNAIRE),
        tier("inspecteur_finances", "L'Inspecteur des Finances", Tier.HAUT_FONCTIONNAIRE),
        tier("dir_cabinet", "La Directrice de Cabinet", Tier.HAUT_FONCTIONNAIRE),
        tier("depute_base", "Le Député de base", Tier.DEPUTE),
        tier("deputee_marche", "La Députée en marche", Tier.DEPUTE),
        tier("depute_insoumis", "Le Député insoumis", Tier.DEPUTE),
        tier("attal", "Gabriel Attal", Tier.MINISTRE),
        tier("filippetti", "Aurélie Filippetti", Tier.MINISTRE),
        tier("le_maire", "Bruno Le Maire", Tier.MINISTRE),
        tier("darmanin", "Gérald Darmanin", Tier.MINISTRE),
    )

    /**
     * Échelle des boss, dans l'ordre d'apparition (vague 5, 10, 15...) :
     * d'abord les chefs d'État, puis les hyper-influents. Le dernier se répète.
     */
    val bosses: List<PoliticianType> = listOf(
        PoliticianType("macron", "Emmanuel Macron", Tier.CHEF_ETAT, hp = 60, speed = 0.04f, points = 500, coins = 25, isBoss = true),
        PoliticianType("netanyahou", "Benyamin Netanyahou", Tier.CHEF_ETAT, hp = 80, speed = 0.04f, points = 600, coins = 30, isBoss = true),
        PoliticianType("trump", "Donald Trump", Tier.HYPER_INFLUENT, hp = 120, speed = 0.035f, points = 1000, coins = 50, isBoss = true),
        PoliticianType("musk", "Elon Musk", Tier.HYPER_INFLUENT, hp = 150, speed = 0.035f, points = 1200, coins = 60, isBoss = true),
        PoliticianType("poutine", "Vladimir Poutine", Tier.HYPER_INFLUENT, hp = 160, speed = 0.03f, points = 1300, coins = 60, isBoss = true),
        PoliticianType("zuckerberg", "Mark Zuckerberg", Tier.HYPER_INFLUENT, hp = 140, speed = 0.04f, points = 1100, coins = 55, isBoss = true),
    )

    /** Boss de la vague donnée (multiple de [GameConfig.BOSS_EVERY_N_WAVES]). */
    fun bossForWave(wave: Int): PoliticianType =
        bosses[(wave / GameConfig.BOSS_EVERY_N_WAVES - 1).coerceIn(0, bosses.lastIndex)]
}
