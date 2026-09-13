package com.sealkrach.politirush.core

/**
 * Arme du héros, déduite du multiplicateur de tir. Purement visuelle et
 * narrative : la mécanique reste "firepower projectiles par salve", mais le
 * joueur voit son équipement grossir avec sa puissance.
 */
enum class Weapon(val label: String, val minFirepower: Int) {
    MAIN("À la main", 1),
    LANCE_PIERRE("Lance-pierre", 5),
    CANON("Canon à tomates", 13),
    TANK("Tank à tomates", 33);

    companion object {
        fun forFirepower(firepower: Int): Weapon =
            entries.last { firepower >= it.minFirepower }
    }
}
