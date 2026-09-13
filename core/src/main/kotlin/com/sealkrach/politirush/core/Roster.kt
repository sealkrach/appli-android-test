package com.sealkrach.politirush.core

/**
 * Catalogue des héros et des cibles.
 *
 * Tous les noms sont des pastiches : on reconnaît l'archétype sans utiliser
 * une marque ou une personne réelle. Les inspirations sont documentées dans
 * docs/GAME_DESIGN.md.
 */
object Roster {

    val heroes: List<Hero> = listOf(
        Hero(
            id = "rambeau",
            name = "Jean Rambeau",
            tagline = "Bandeau rouge, mitrailleuse à tomates, jamais de repos.",
            fireRate = 6f,
            damage = 1,
            moveSpeed = 1.2f,
            special = Special.RAFALE,
            specialCooldown = 14f,
        ),
        Hero(
            id = "balbo",
            name = "Rocco Balbo",
            tagline = "Le boxeur de Philadelphie. Encaisse, encaisse, puis frappe.",
            fireRate = 3f,
            damage = 3,
            moveSpeed = 1.0f,
            special = Special.UPPERCUT,
            specialCooldown = 12f,
        ),
        Hero(
            id = "machete",
            name = "El Machette",
            tagline = "Il ne texte pas. Il tranche.",
            fireRate = 4f,
            damage = 2,
            moveSpeed = 1.1f,
            special = Special.LAMES,
            specialCooldown = 15f,
        ),
        Hero(
            id = "chauve_souris",
            name = "Le Chevalier Chauve-Souris",
            tagline = "Milliardaire nocturne, ceinture pleine de gadgets.",
            fireRate = 4.5f,
            damage = 1,
            moveSpeed = 1.3f,
            special = Special.GADGET,
            specialCooldown = 16f,
        ),
        Hero(
            id = "papa_particulier",
            name = "Le Papa Très Particulier",
            tagline = "Ex-agent. Il vous trouvera. Il vous tomatera.",
            fireRate = 5f,
            damage = 2,
            moveSpeed = 1.1f,
            special = Special.COMPETENCES_PARTICULIERES,
            specialCooldown = 15f,
        ),
        Hero(
            id = "transporteur",
            name = "Le Transporteur Chauve",
            tagline = "Costume impeccable, règles strictes, livraison garantie.",
            fireRate = 5f,
            damage = 1,
            moveSpeed = 1.4f,
            special = Special.LIVRAISON,
            specialCooldown = 18f,
        ),
    )

    fun hero(id: String): Hero = heroes.first { it.id == id }

    val politicians: List<PoliticianType> = listOf(
        PoliticianType("promettologue", "Le Promettologue", hp = 2, speed = 0.12f, points = 10, coins = 1),
        PoliticianType("girouette", "La Girouette", hp = 1, speed = 0.20f, points = 15, coins = 1),
        PoliticianType("baron", "Le Baron Local", hp = 4, speed = 0.09f, points = 25, coins = 2),
        PoliticianType("technocrate", "Le Technocrate", hp = 3, speed = 0.11f, points = 20, coins = 2),
        PoliticianType("influenceur", "L'Influenceur Populiste", hp = 2, speed = 0.18f, points = 20, coins = 2),
        PoliticianType("dinosaure", "Le Dinosaure du Sénat", hp = 6, speed = 0.07f, points = 40, coins = 3),
    )

    val bosses: List<PoliticianType> = listOf(
        PoliticianType("candidat_eternel", "Le Candidat Éternel", hp = 60, speed = 0.04f, points = 500, coins = 25, isBoss = true),
        PoliticianType("ministre_reformes", "Le Ministre des Réformes Indispensables", hp = 90, speed = 0.035f, points = 800, coins = 40, isBoss = true),
    )
}
