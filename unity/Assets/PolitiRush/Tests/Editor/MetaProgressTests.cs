using NUnit.Framework;

namespace PolitiRush.Core.Tests
{
    public class MetaProgressTests
    {
        [Test] public void AcheterDebiteLesPiecesEtMonteLeNiveau()
        {
            var m = new MetaProgress { Coins = 100 };
            Assert.IsTrue(m.CanBuy(Upgrade.Cadence));
            Assert.IsTrue(m.Buy(Upgrade.Cadence));
            Assert.AreEqual(1, m.Level(Upgrade.Cadence));
            Assert.AreEqual(50, m.Coins);
            Assert.IsFalse(m.CanBuy(Upgrade.Cadence));
            Assert.IsFalse(m.Buy(Upgrade.Cadence));
        }

        [Test] public void LeCoutCroitEtLeNiveauMaxEstRespecte()
        {
            Assert.Greater(Upgrade.Degats.Cost(3), Upgrade.Degats.Cost(2));
            var maxed = new MetaProgress { Coins = 1_000_000 }; maxed.Levels[Upgrade.Vies] = Upgrade.Vies.MaxLevel();
            Assert.IsFalse(maxed.CanBuy(Upgrade.Vies));
        }

        [Test] public void LesAmeliorationsSAppliquentALEtatDeDepart()
        {
            var m = new MetaProgress(); m.Levels[Upgrade.Vies] = 2; m.Levels[Upgrade.MultiplicateurDepart] = 3; m.Levels[Upgrade.Cadence] = 5;
            var s = m.StartingState(Roster.Hero("balbo"));
            Assert.AreEqual(GameConfig.HeroMaxHp + 2, s.HeroHp);
            Assert.AreEqual(GameConfig.StartFirepower + 3, s.Firepower);
            Assert.Greater(s.Hero.FireRate, Roster.Hero("balbo").FireRate);
        }

        [Test] public void LaFinDePartieAlimenteLaProgression()
        {
            var end = new GameState(Roster.Hero("rambeau")) { Score = 1234, Coins = 30, Wave = 4 };
            var m = new MetaProgress { Coins = 10, BestScore = 500 }; m.AfterRun(end);
            Assert.AreEqual(40, m.Coins);
            Assert.AreEqual(1234, m.BestScore);
            Assert.AreEqual(4, m.BestWave);
            Assert.AreEqual(1, m.Runs);
        }
    }
}
