package com.sealkrach.politirush.ui

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.safeDrawingPadding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Button
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.sealkrach.politirush.core.GameState
import com.sealkrach.politirush.core.Hero
import com.sealkrach.politirush.core.MetaProgress
import com.sealkrach.politirush.core.Upgrade

@Composable
fun HomeScreen(
    progress: MetaProgress,
    heroes: List<Hero>,
    onPlay: (Hero) -> Unit,
    onShop: () -> Unit,
) {
    var selected by remember { mutableStateOf(heroes.first()) }
    Column(
        Modifier.fillMaxSize().safeDrawingPadding().padding(20.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
    ) {
        Text("POLITIRUSH", fontSize = 40.sp, fontWeight = FontWeight.Black, color = MaterialTheme.colorScheme.primary)
        Text("Tomates, multiplicateurs, caricatures.", color = Color.LightGray)
        Spacer(Modifier.height(12.dp))
        Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
            Text("Pièces : ${progress.coins}", color = MaterialTheme.colorScheme.secondary)
            Text("Record : ${progress.bestScore}", color = Color.White)
        }
        Spacer(Modifier.height(16.dp))
        Text("Choisis ton héros", fontSize = 18.sp, fontWeight = FontWeight.Bold, color = Color.White)
        Spacer(Modifier.height(8.dp))
        LazyColumn(Modifier.weight(1f), verticalArrangement = Arrangement.spacedBy(8.dp)) {
            items(heroes) { hero ->
                val unlocked = hero.id in progress.unlockedHeroes || progress.runs >= 3
                HeroCard(hero, selected = hero == selected, unlocked = unlocked) { if (unlocked) selected = hero }
            }
        }
        Spacer(Modifier.height(12.dp))
        Button(onClick = { onPlay(selected) }, modifier = Modifier.fillMaxWidth().height(56.dp)) {
            Text("JOUER avec ${selected.name}", fontSize = 18.sp, fontWeight = FontWeight.Bold)
        }
        Spacer(Modifier.height(8.dp))
        OutlinedButton(onClick = onShop, modifier = Modifier.fillMaxWidth()) { Text("Boutique") }
    }
}

@Composable
private fun HeroCard(hero: Hero, selected: Boolean, unlocked: Boolean, onClick: () -> Unit) {
    val border = if (selected) MaterialTheme.colorScheme.primary else Color.DarkGray
    Column(
        Modifier
            .fillMaxWidth()
            .background(MaterialTheme.colorScheme.surface, RoundedCornerShape(12.dp))
            .border(2.dp, border, RoundedCornerShape(12.dp))
            .clickable(onClick = onClick)
            .padding(12.dp),
    ) {
        Text(
            if (unlocked) hero.name else "🔒 ${hero.name} (3 parties pour débloquer)",
            fontWeight = FontWeight.Bold,
            color = if (unlocked) Color.White else Color.Gray,
        )
        Text(hero.tagline, color = Color.LightGray, fontSize = 13.sp)
        Text(
            "Cadence ${hero.fireRate}/s · Dégâts ${hero.damage} · Spécial : ${hero.special.name.lowercase().replace('_', ' ')}",
            color = MaterialTheme.colorScheme.tertiary,
            fontSize = 12.sp,
        )
    }
}

@Composable
fun ShopScreen(progress: MetaProgress, onBuy: (Upgrade) -> Unit, onBack: () -> Unit) {
    Column(Modifier.fillMaxSize().safeDrawingPadding().padding(20.dp)) {
        Text("BOUTIQUE", fontSize = 32.sp, fontWeight = FontWeight.Black, color = MaterialTheme.colorScheme.secondary)
        Text("Pièces : ${progress.coins}", color = Color.White)
        Spacer(Modifier.height(16.dp))
        LazyColumn(Modifier.weight(1f), verticalArrangement = Arrangement.spacedBy(8.dp)) {
            items(Upgrade.entries) { u ->
                val lvl = progress.level(u)
                val maxed = lvl >= u.maxLevel
                Row(
                    Modifier.fillMaxWidth().background(MaterialTheme.colorScheme.surface, RoundedCornerShape(12.dp)).padding(12.dp),
                    verticalAlignment = Alignment.CenterVertically,
                ) {
                    Column(Modifier.weight(1f)) {
                        Text(u.label, fontWeight = FontWeight.Bold, color = Color.White)
                        Text("Niveau $lvl / ${u.maxLevel}", color = Color.LightGray, fontSize = 12.sp)
                    }
                    Button(onClick = { onBuy(u) }, enabled = progress.canBuy(u)) {
                        Text(if (maxed) "MAX" else "${u.cost(lvl)} 🪙")
                    }
                }
            }
        }
        Spacer(Modifier.height(12.dp))
        OutlinedButton(onClick = onBack, modifier = Modifier.fillMaxWidth()) { Text("Retour") }
    }
}

@Composable
fun GameOverScreen(finalState: GameState, progress: MetaProgress, onRetry: () -> Unit, onHome: () -> Unit) {
    Column(
        Modifier.fillMaxSize().safeDrawingPadding().padding(24.dp),
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally,
    ) {
        Text("FIN DE PARTIE", fontSize = 36.sp, fontWeight = FontWeight.Black, color = MaterialTheme.colorScheme.primary)
        Spacer(Modifier.height(16.dp))
        Text("Score : ${finalState.score}", fontSize = 24.sp, color = Color.White)
        Text("Vague atteinte : ${finalState.wave}", color = Color.LightGray)
        Text("Pièces ramassées : ${finalState.coins}", color = MaterialTheme.colorScheme.secondary)
        if (finalState.score >= progress.bestScore) {
            Text("NOUVEAU RECORD !", color = MaterialTheme.colorScheme.tertiary, fontWeight = FontWeight.Bold)
        }
        Spacer(Modifier.height(32.dp))
        Button(onClick = onRetry, modifier = Modifier.fillMaxWidth().height(56.dp)) {
            Text("ENCORE UNE !", fontSize = 18.sp, fontWeight = FontWeight.Bold)
        }
        Spacer(Modifier.height(8.dp))
        OutlinedButton(onClick = onHome, modifier = Modifier.fillMaxWidth()) { Text("Menu") }
    }
}
