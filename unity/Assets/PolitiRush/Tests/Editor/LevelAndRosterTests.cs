using System.Linq;
using NUnit.Framework;

namespace PolitiRush.Core.Tests
{
    public class LevelAndRosterTests
    {
        [Test] public void LArmeSuitLaPuissance()
        {
            Assert.AreEqual(Weapon.Main, WeaponInfo.ForFirepower(1));
            Assert.AreEqual(Weapon.Main, WeaponInfo.ForFirepower(4));
            Assert.AreEqual(Weapon.LancePierre, WeaponInfo.ForFirepower(5));
            Assert.AreEqual(Weapon.Canon, WeaponInfo.ForFirepower(13));
            Assert.AreEqual(Weapon.Tank, WeaponInfo.ForFirepower(33));
            Assert.AreEqual(Weapon.Tank, WeaponInfo.ForFirepower(GameConfig.MaxFirepower));
            Assert.AreEqual(Weapon.LancePierre, new GameState(Roster.Hero("rambeau")) { Firepower = 8 }.Weapon);
        }

        [Test] public void LaResistanceSuitLeRangReel()
        {
            int Hp(string id) => Roster.Politicians.First(p => p.Id == id).Hp;
            Assert.Less(Hp("prefet"), Hp("depute_base"));
            Assert.Less(Hp("depute_base"), Hp("attal"));
            Assert.AreEqual(Hp("attal"), Hp("filippetti"));
            Assert.Greater(Roster.Bosses.First(b => b.Id == "macron").Hp, Hp("attal"));
            Assert.Greater(Roster.Bosses.First(b => b.Id == "musk").Hp, Roster.Bosses.First(b => b.Id == "macron").Hp);
        }

        [Test] public void LesChefsDEtatPrecedentLesHyperInfluents()
        {
            var tiers = Roster.Bosses.Select(b => b.Tier).ToList();
            Assert.Less(tiers.LastIndexOf(Tier.ChefEtat), tiers.IndexOf(Tier.HyperInfluent));
            Assert.AreEqual(Tier.ChefEtat, Levels.Palais.BossForWave(5).Tier);
            Assert.AreEqual(Tier.ChefEtat, Levels.Palais.BossForWave(10).Tier);
            Assert.AreEqual(Tier.HyperInfluent, Levels.Palais.BossForWave(15).Tier);
            Assert.AreEqual(Roster.Bosses.Last(), Levels.Palais.BossForWave(500));
        }

        [Test] public void ChaqueNiveauALesTroisRangsEtUneEchelleDeBoss()
        {
            foreach (var level in Levels.All)
            {
                var tiers = level.Politicians.Select(p => p.Tier).Distinct().ToList();
                Assert.IsTrue(tiers.Contains(Tier.HautFonctionnaire) && tiers.Contains(Tier.Depute) && tiers.Contains(Tier.Ministre), level.Id);
                Assert.AreEqual(Tier.ChefEtat, level.Bosses[0].Tier, level.Id);
                Assert.IsTrue(level.Bosses.All(b => b.IsBoss), level.Id);
            }
        }

        [Test] public void LesLibellesDeBonusSuiventLeTheme()
        {
            Assert.AreEqual("Ristourne", Levels.Carburant.BonusLabel(BonusType.Sondage));
            Assert.AreEqual("Duty free", Levels.Aeroport.BonusLabel(BonusType.Meeting));
            Assert.AreEqual(BonusType.Sondage.Label(), Levels.Palais.BonusLabel(BonusType.Sondage));
        }

        [Test] public void LeSpawnerDUnNiveauNeProduitQueSesCibles()
        {
            var sp = new Spawner(11, Levels.Carburant);
            var ids = Levels.Carburant.Politicians.Select(p => p.Id).ToHashSet();
            var spawned = new System.Collections.Generic.List<Enemy>();
            for (int i = 0; i < 2400; i++) spawned.AddRange(sp.Advance(1f / 60f, 6, 0.5f, 0).Enemies);
            Assert.Greater(spawned.Count, 0);
            Assert.IsTrue(spawned.Where(e => !e.Type.IsBoss).All(e => ids.Contains(e.Type.Id)));
        }

        [Test] public void LesMinistresNApparaissentPasDansLesPremieresVagues()
        {
            var sp = new Spawner(3);
            var early = new System.Collections.Generic.List<Enemy>();
            for (int i = 0; i < 600; i++) early.AddRange(sp.Advance(1f / 60f, 1, 0.5f, 0).Enemies);
            Assert.Greater(early.Count, 0);
            Assert.IsTrue(early.All(e => e.Type.Tier == Tier.HautFonctionnaire));
        }
    }
}
