package com.sealkrach.politirush.ui

import androidx.compose.foundation.Canvas
import androidx.compose.foundation.gestures.detectDragGestures
import androidx.compose.foundation.gestures.detectTapGestures
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.safeDrawingPadding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.runtime.withFrameNanos
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.geometry.Size
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.drawscope.DrawScope
import androidx.compose.ui.graphics.drawscope.Stroke
import androidx.compose.ui.graphics.nativeCanvas
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.sealkrach.politirush.core.BonusType
import com.sealkrach.politirush.core.GameConfig
import com.sealkrach.politirush.core.GameEngine
import com.sealkrach.politirush.core.GameEvent
import com.sealkrach.politirush.core.GameState
import com.sealkrach.politirush.core.GameStatus
import com.sealkrach.politirush.core.PlayerInput
import kotlin.math.min

/**
 * Écran de jeu : boucle de simulation à 60 Hz (pas fixe) et rendu Canvas.
 * Le rendu est volontairement "placeholder" (formes + texte) : le module core
 * fournit tout ce qu'il faut pour brancher plus tard des sprites et des
 * particules à partir des [GameEvent].
 */
@Composable
fun GameScreen(initialState: GameState, onGameOver: (GameState) -> Unit) {
    val engine = remember { GameEngine(initialState, seed = System.currentTimeMillis()) }
    var state by remember { mutableStateOf(engine.state) }
    var targetX by remember { mutableStateOf<Float?>(null) }
    var specialRequested by remember { mutableStateOf(false) }
    var lastEvents by remember { mutableStateOf<List<GameEvent>>(emptyList()) }

    LaunchedEffect(Unit) {
        val step = 1f / 60f
        var accumulator = 0f
        var last = withFrameNanos { it }
        while (state.status == GameStatus.RUNNING) {
            val now = withFrameNanos { it }
            accumulator += min((now - last) / 1_000_000_000f, 0.1f)
            last = now
            val events = mutableListOf<GameEvent>()
            while (accumulator >= step) {
                events += engine.step(step, PlayerInput(targetX = targetX, useSpecial = specialRequested))
                specialRequested = false
                accumulator -= step
            }
            if (events.isNotEmpty()) lastEvents = events
            state = engine.state
        }
        onGameOver(state)
    }

    Box(Modifier.fillMaxSize()) {
        Canvas(
            Modifier
                .fillMaxSize()
                .pointerInput(Unit) {
                    detectDragGestures(
                        onDragStart = { targetX = it.x / size.width },
                        onDragEnd = { targetX = null },
                        onDragCancel = { targetX = null },
                    ) { change, _ -> targetX = change.position.x / size.width }
                }
                .pointerInput(Unit) {
                    detectTapGestures(onPress = { targetX = it.x / size.width })
                },
        ) {
            drawGame(state)
        }
        Hud(state, lastEvents, onSpecial = { specialRequested = true })
    }
}

@Composable
private fun Hud(state: GameState, events: List<GameEvent>, onSpecial: () -> Unit) {
    Column(Modifier.fillMaxSize().safeDrawingPadding().padding(12.dp)) {
        Row(Modifier.fillMaxWidth(), verticalAlignment = Alignment.CenterVertically) {
            Text("Vague ${state.wave}", color = Color.White, fontWeight = FontWeight.Bold, modifier = Modifier.weight(1f))
            Text("${state.score}", color = Color.White, fontSize = 22.sp, fontWeight = FontWeight.Black, modifier = Modifier.weight(1f))
            Text("🪙 ${state.coins}", color = MaterialTheme.colorScheme.secondary, modifier = Modifier.weight(1f))
        }
        Row(Modifier.fillMaxWidth()) {
            Text("❤".repeat(state.heroHp), color = MaterialTheme.colorScheme.primary, modifier = Modifier.weight(1f))
            if (state.combo >= 3) Text("COMBO x${state.combo}", color = MaterialTheme.colorScheme.tertiary, fontWeight = FontWeight.Bold)
        }
        val active = state.effects.joinToString("  ") { "${it.type.label} ${it.remaining.toInt() + 1}s" }
        if (active.isNotEmpty()) Text(active, color = Color.LightGray, fontSize = 12.sp)
        events.filterIsInstance<GameEvent.GatePassed>().lastOrNull()?.let {
            Text("${it.op.label} → tir x${it.newFirepower}", color = if (it.op.isGood) Color.Green else Color.Red, fontWeight = FontWeight.Bold)
        }
        Box(Modifier.weight(1f))
        Row(Modifier.fillMaxWidth(), horizontalArrangement = androidx.compose.foundation.layout.Arrangement.End) {
            Button(
                onClick = onSpecial,
                enabled = state.specialReady,
                shape = CircleShape,
                modifier = Modifier.size(84.dp),
                colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.tertiary),
            ) {
                Text(if (state.specialReady) "SPÉCIAL" else "${state.specialCooldownRemaining.toInt() + 1}s", fontSize = 12.sp, fontWeight = FontWeight.Bold)
            }
        }
    }
}

private val gateGood = Color(0xFF2A9D8F)
private val gateBad = Color(0xFFE63946)
private val tomato = Color(0xFFE63946)
private val heroColor = Color(0xFFF4A261)
private val coinColor = Color(0xFFFFD166)

private fun DrawScope.drawGame(s: GameState) {
    val w = size.width
    val h = size.height
    fun px(x: Float) = x * w
    fun py(y: Float) = h - y * h

    // Piste : ligne centrale pour matérialiser les deux couloirs de portes.
    drawLine(Color.White.copy(alpha = 0.08f), Offset(w / 2, 0f), Offset(w / 2, h), strokeWidth = 2f)

    // Portes.
    for (g in s.gates) {
        if (g.consumed) continue
        val color = if (g.op.isGood) gateGood else gateBad
        val top = py(g.y + 0.03f)
        drawRect(color.copy(alpha = 0.25f), Offset(px(g.xMin) + 8f, top), Size(px(g.xMax - g.xMin) - 16f, h * 0.06f))
        drawRect(color, Offset(px(g.xMin) + 8f, top), Size(px(g.xMax - g.xMin) - 16f, h * 0.06f), style = Stroke(4f))
        drawText(g.op.label, px((g.xMin + g.xMax) / 2), top + h * 0.045f, 56f, color)
    }

    // Bonus.
    for (b in s.bonuses) {
        drawCircle(Color(0xFF9B5DE5), radius = w * 0.035f, center = Offset(px(b.x), py(b.y)))
        drawText(b.type.label, px(b.x), py(b.y) - w * 0.05f, 30f, Color.White)
    }

    // Pièces.
    for (c in s.coinsOnTrack) drawCircle(coinColor, radius = w * 0.012f, center = Offset(px(c.x), py(c.y)))

    // Cibles : cercle (tête) + barre de vie. Les boss sont plus gros.
    val frozen = s.hasEffect(BonusType.SCANDALE)
    for (e in s.enemies) {
        val r = if (e.type.isBoss) w * 0.11f else w * GameConfig.ENEMY_RADIUS
        val c = Offset(px(e.x), py(e.y))
        drawCircle(if (frozen) Color(0xFF90E0EF) else Color(0xFFF1FAEE), radius = r, center = c)
        drawCircle(Color(0xFF1B1B2F), radius = r * 0.12f, center = c + Offset(-r * 0.3f, -r * 0.2f))
        drawCircle(Color(0xFF1B1B2F), radius = r * 0.12f, center = c + Offset(r * 0.3f, -r * 0.2f))
        val hpFrac = e.hp.toFloat() / (e.type.hp + s.wave * 5).coerceAtLeast(1)
        drawRect(Color.DarkGray, Offset(c.x - r, c.y - r - 14f), Size(2 * r, 8f))
        drawRect(gateBad, Offset(c.x - r, c.y - r - 14f), Size(2 * r * hpFrac.coerceIn(0f, 1f), 8f))
        if (e.type.isBoss) drawText(e.type.name, c.x, c.y - r - 24f, 30f, Color.White)
    }

    // Projectiles.
    for (p in s.projectiles) {
        drawCircle(if (p.piercing) Color(0xFFFF9F1C) else tomato, radius = w * 0.012f, center = Offset(px(p.x), py(p.y)))
    }

    // Héros.
    val hc = Offset(px(s.heroX), py(GameConfig.HERO_Y))
    drawCircle(heroColor, radius = w * GameConfig.HERO_RADIUS, center = hc)
    drawText("x${s.firepower}", hc.x, hc.y + w * 0.1f, 44f, Color.White)
}

private fun DrawScope.drawText(text: String, x: Float, y: Float, sizePx: Float, color: Color) {
    val paint = android.graphics.Paint().apply {
        this.color = android.graphics.Color.argb(
            (color.alpha * 255).toInt(), (color.red * 255).toInt(), (color.green * 255).toInt(), (color.blue * 255).toInt(),
        )
        textSize = sizePx
        textAlign = android.graphics.Paint.Align.CENTER
        isFakeBoldText = true
        isAntiAlias = true
    }
    drawContext.canvas.nativeCanvas.drawText(text, x, y, paint)
}
