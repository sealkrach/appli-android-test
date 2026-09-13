package com.sealkrach.politirush

import android.content.Context
import com.sealkrach.politirush.core.MetaProgress
import com.sealkrach.politirush.core.Upgrade

/**
 * Persistance minimale de la progression dans les SharedPreferences.
 * À remplacer par DataStore (et une sauvegarde cloud) quand le jeu grossira.
 */
class ProgressStore(context: Context) {
    private val prefs = context.getSharedPreferences("politirush_progress", Context.MODE_PRIVATE)

    fun load(): MetaProgress = MetaProgress(
        coins = prefs.getInt("coins", 0),
        bestScore = prefs.getInt("bestScore", 0),
        bestWave = prefs.getInt("bestWave", 0),
        runs = prefs.getInt("runs", 0),
        levels = Upgrade.entries.associateWith { prefs.getInt("lvl_${it.name}", 0) }.filterValues { it > 0 },
        unlockedHeroes = prefs.getStringSet("heroes", setOf("rambeau")) ?: setOf("rambeau"),
    )

    fun save(p: MetaProgress) {
        prefs.edit().apply {
            putInt("coins", p.coins)
            putInt("bestScore", p.bestScore)
            putInt("bestWave", p.bestWave)
            putInt("runs", p.runs)
            Upgrade.entries.forEach { putInt("lvl_${it.name}", p.level(it)) }
            putStringSet("heroes", p.unlockedHeroes)
        }.apply()
    }
}
