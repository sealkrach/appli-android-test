package com.sealkrach.politirush.core

import org.junit.jupiter.api.Assertions.assertEquals
import org.junit.jupiter.api.Assertions.assertFalse
import org.junit.jupiter.api.Assertions.assertTrue
import org.junit.jupiter.api.Test

class GateOpTest {
    @Test
    fun `les opérations de porte s'appliquent`() {
        assertEquals(6, GateOp.Multiply(3).apply(2))
        assertEquals(7, GateOp.Add(5).apply(2))
        assertEquals(3, GateOp.Subtract(2).apply(5))
        assertEquals(4, GateOp.Divide(2).apply(9))
    }

    @Test
    fun `le multiplicateur reste borné`() {
        assertEquals(GameConfig.MIN_FIREPOWER, GateOp.Subtract(10).apply(3))
        assertEquals(GameConfig.MIN_FIREPOWER, GateOp.Divide(4).apply(1))
        assertEquals(GameConfig.MAX_FIREPOWER, GateOp.Multiply(3).apply(60))
    }

    @Test
    fun `libellés et classification`() {
        assertEquals("x2", GateOp.Multiply(2).label)
        assertEquals("+5", GateOp.Add(5).label)
        assertEquals("-3", GateOp.Subtract(3).label)
        assertEquals("÷2", GateOp.Divide(2).label)
        assertTrue(GateOp.Add(1).isGood)
        assertFalse(GateOp.Divide(2).isGood)
    }
}
