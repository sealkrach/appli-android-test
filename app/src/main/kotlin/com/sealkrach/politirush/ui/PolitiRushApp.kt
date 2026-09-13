package com.sealkrach.politirush.ui

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import com.sealkrach.politirush.ProgressStore
import com.sealkrach.politirush.core.GameState
import com.sealkrach.politirush.core.Hero
import com.sealkrach.politirush.core.MetaProgress
import com.sealkrach.politirush.core.Roster

/** Écrans de l'application. Une vraie navigation viendra plus tard. */
sealed class Screen {
    data object Home : Screen()
    data object Shop : Screen()
    data class Playing(val hero: Hero) : Screen()
    data class GameOver(val hero: Hero, val finalState: GameState) : Screen()
}

val Palette = darkColorScheme(
    primary = Color(0xFFE63946),
    secondary = Color(0xFFF4A261),
    tertiary = Color(0xFF2A9D8F),
    background = Color(0xFF1B1B2F),
    surface = Color(0xFF24243E),
)

@Composable
fun PolitiRushApp(progressStore: ProgressStore) {
    var progress by remember { mutableStateOf(progressStore.load()) }
    var screen: Screen by remember { mutableStateOf(Screen.Home) }

    fun updateProgress(p: MetaProgress) {
        progress = p
        progressStore.save(p)
    }

    MaterialTheme(colorScheme = Palette) {
        Box(Modifier.fillMaxSize().background(MaterialTheme.colorScheme.background)) {
            when (val s = screen) {
                Screen.Home -> HomeScreen(
                    progress = progress,
                    heroes = Roster.heroes,
                    onPlay = { hero -> screen = Screen.Playing(hero) },
                    onShop = { screen = Screen.Shop },
                )
                Screen.Shop -> ShopScreen(
                    progress = progress,
                    onBuy = { upgrade -> updateProgress(progress.buy(upgrade)) },
                    onBack = { screen = Screen.Home },
                )
                is Screen.Playing -> GameScreen(
                    initialState = progress.startingState(s.hero),
                    onGameOver = { final ->
                        updateProgress(progress.afterRun(final))
                        screen = Screen.GameOver(s.hero, final)
                    },
                )
                is Screen.GameOver -> GameOverScreen(
                    finalState = s.finalState,
                    progress = progress,
                    onRetry = { screen = Screen.Playing(s.hero) },
                    onHome = { screen = Screen.Home },
                )
            }
        }
    }
}
