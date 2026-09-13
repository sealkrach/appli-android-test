using NUnit.Framework;

namespace PolitiRush.Core.Tests
{
    public class GateOpTests
    {
        [Test] public void LesOperationsDePorteSAppliquent()
        {
            Assert.AreEqual(6, new GateOp.Multiply(3).Apply(2));
            Assert.AreEqual(7, new GateOp.Add(5).Apply(2));
            Assert.AreEqual(3, new GateOp.Subtract(2).Apply(5));
            Assert.AreEqual(4, new GateOp.Divide(2).Apply(9));
        }

        [Test] public void LeMultiplicateurResteBorne()
        {
            Assert.AreEqual(GameConfig.MinFirepower, new GateOp.Subtract(10).Apply(3));
            Assert.AreEqual(GameConfig.MinFirepower, new GateOp.Divide(4).Apply(1));
            Assert.AreEqual(GameConfig.MaxFirepower, new GateOp.Multiply(3).Apply(60));
        }

        [Test] public void LibellesEtClassification()
        {
            Assert.AreEqual("x2", new GateOp.Multiply(2).Label);
            Assert.AreEqual("+5", new GateOp.Add(5).Label);
            Assert.AreEqual("-3", new GateOp.Subtract(3).Label);
            Assert.AreEqual("÷2", new GateOp.Divide(2).Label);
            Assert.IsTrue(new GateOp.Add(1).IsGood);
            Assert.IsFalse(new GateOp.Divide(2).IsGood);
        }
    }
}
