using System.Linq;
using NUnit.Framework;

namespace PolitiRush.Core.Tests
{
    public class DifficultyTests
    {
        readonly PoliticianType prefet = Roster.Politicians.First(p => p.Id == "prefet");
        readonly PoliticianType depute = Roster.Politicians.First(p => p.Id == "depute_base");
        readonly PoliticianType attal = Roster.Politicians.First(p => p.Id == "attal");

        [Test] public void LaResistanceSuitLeRangEtGrimpeAvecLesVagues()
        {
            Assert.Less(Difficulty.Hp(prefet, 1), Difficulty.Hp(depute, 1));
            Assert.Less(Difficulty.Hp(depute, 1), Difficulty.Hp(attal, 1));
            Assert.AreEqual(5, Difficulty.Hp(attal, 1));
            Assert.AreEqual(14, Difficulty.Hp(attal, 10));
            Assert.Greater(Difficulty.Hp(attal, 10) - Difficulty.Hp(attal, 1), Difficulty.Hp(prefet, 10) - Difficulty.Hp(prefet, 1));
            for (int w = 1; w <= 30; w++) Assert.GreaterOrEqual(Difficulty.Hp(attal, w + 1), Difficulty.Hp(attal, w));
        }

        [Test] public void LeNombreMonteAvecLAvancee()
        {
            for (int w = 1; w <= 30; w++)
            {
                Assert.LessOrEqual(Difficulty.SpawnInterval(w + 1), Difficulty.SpawnInterval(w));
                Assert.GreaterOrEqual(Difficulty.GroupSize(w + 1), Difficulty.GroupSize(w));
                Assert.GreaterOrEqual(Difficulty.MaxAlive(w + 1), Difficulty.MaxAlive(w));
            }
            Assert.AreEqual(1, Difficulty.GroupSize(1));
            Assert.AreEqual(2, Difficulty.GroupSize(4));
            Assert.AreEqual(4, Difficulty.GroupSize(10));
            Assert.AreEqual(GameConfig.MinSpawnInterval, Difficulty.SpawnInterval(40));
            Assert.AreEqual(40, Difficulty.MaxAlive(40));
        }

        [Test] public void LeMelangeDesRangsBasculeVersLesRangsEleves()
        {
            Assert.AreEqual(0, Difficulty.Weight(Tier.Depute, 1));
            Assert.AreEqual(0, Difficulty.Weight(Tier.Ministre, 3));
            Assert.Greater(Difficulty.Weight(Tier.Ministre, 4), 0);
            Assert.Greater(Difficulty.Weight(Tier.HautFonctionnaire, 1), Difficulty.Weight(Tier.HautFonctionnaire, 12));
            Assert.Greater(Difficulty.Weight(Tier.Ministre, 12), Difficulty.Weight(Tier.Ministre, 4));
            Assert.GreaterOrEqual(Difficulty.Weight(Tier.Ministre, 12), Difficulty.Weight(Tier.HautFonctionnaire, 12));
            Assert.AreEqual(0, Difficulty.Weight(Tier.ChefEtat, 20));
        }

        [Test] public void LeSpawnerProduitBienPlusDeCiblesEnVague10()
        {
            int CountPerMinute(int wave)
            {
                var sp = new Spawner(5); int n = 0;
                for (int i = 0; i < 60 * 60; i++) n += sp.Advance(1f / 60f, wave, 0.5f, 0).Enemies.Count(e => !e.Type.IsBoss);
                return n;
            }
            int early = CountPerMinute(1), late = CountPerMinute(10);
            Assert.Greater(late, early * 3);
        }

        [Test] public void LesGroupesRespectentLePlafond()
        {
            var sp = new Spawner(9);
            var batch = sp.Advance(5f, 12, 0.5f, Difficulty.MaxAlive(12) - 1);
            Assert.LessOrEqual(batch.Enemies.Count(e => !e.Type.IsBoss), 1);
        }
    }
}
