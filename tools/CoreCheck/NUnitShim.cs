// Sous-ensemble minimal de NUnit pour exécuter les tests hors Unity.
// Dans Unity, c'est le vrai NUnit (Unity Test Framework) qui est utilisé.
using System;

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Method)] public sealed class TestAttribute : Attribute { }

    public sealed class AssertionException : Exception { public AssertionException(string m) : base(m) { } }

    public static class Assert
    {
        static void Fail(string m) => throw new AssertionException(m);
        public static void AreEqual(object expected, object actual, string msg = null)
        { if (!Equals(expected, actual)) Fail($"attendu <{expected}> mais obtenu <{actual}> {msg}"); }
        public static void AreEqual(float expected, float actual, float delta)
        { if (Math.Abs(expected - actual) > delta) Fail($"attendu <{expected}> ± {delta} mais obtenu <{actual}>"); }
        public static void IsTrue(bool c, string msg = null) { if (!c) Fail("attendu vrai " + msg); }
        public static void IsFalse(bool c, string msg = null) { if (c) Fail("attendu faux " + msg); }
        public static void Less(IComparable a, IComparable b) { if (a.CompareTo(b) >= 0) Fail($"attendu {a} < {b}"); }
        public static void LessOrEqual(IComparable a, IComparable b) { if (a.CompareTo(b) > 0) Fail($"attendu {a} <= {b}"); }
        public static void Greater(IComparable a, IComparable b) { if (a.CompareTo(b) <= 0) Fail($"attendu {a} > {b}"); }
        public static void GreaterOrEqual(IComparable a, IComparable b) { if (a.CompareTo(b) < 0) Fail($"attendu {a} >= {b}"); }
    }
}
