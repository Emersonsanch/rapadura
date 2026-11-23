using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Refining;
using Rapadura.Gameplay.World.Map.Mechanics;

namespace Rapadura.Tests
{
    /// <summary>
    /// Apep beaten spits the river (Docs/Mapas100.md 76, gancho "Pedra da Mistura", 2026-11-23): the stones she spits, the midnight bonus, what stays
    /// due with a full bag and the texts (<see cref="ApepSpitRiverRules"/>, <see cref="ApepSpitRiverTexts"/>).
    /// </summary>
    public class ApepSpitRiverTests
    {
        private static readonly CharacterId[] Cousins = { CharacterId.Maithe, CharacterId.Icaro, CharacterId.Joaquim, CharacterId.Maria, CharacterId.Lavine };

        [Test]
        public void Apep_SpitsOneMixtureStone_TwoAtMidnight()
        {
            Assert.AreEqual(RefineItems.ChaosStone, ApepSpitRiverRules.StoneItemId);
            Assert.IsTrue(ApepSpitRiverRules.IsApep(RegionBossRules.ApepId));
            Assert.IsFalse(ApepSpitRiverRules.IsApep(RegionBossRules.OldKulId));
            Assert.IsFalse(ApepSpitRiverRules.IsApep(null));

            Assert.AreEqual(2, ApepSpitRiverRules.StonesFor(0f));
            Assert.AreEqual(2, ApepSpitRiverRules.StonesFor(23.2f), "wraps round midnight");
            Assert.AreEqual(2, ApepSpitRiverRules.StonesFor(1f));
            Assert.AreEqual(1, ApepSpitRiverRules.StonesFor(1.5f));
            Assert.AreEqual(1, ApepSpitRiverRules.StonesFor(21f));
            Assert.AreEqual(1, ApepSpitRiverRules.StonesFor(12f));
            Assert.GreaterOrEqual(RegionBossRules.MidnightMultiplier(ApepSpitRiverRules.MidnightBonusHours), 1.25f, "the bonus hour is her strongest");
        }

        [Test]
        public void Apep_StonesThatDoNotFit_StayDue_AndAddUp()
        {
            Assert.AreEqual(1, ApepSpitRiverRules.Owe(0, 21f));
            Assert.AreEqual(3, ApepSpitRiverRules.Owe(1, 0f));
            Assert.AreEqual(1, ApepSpitRiverRules.Owe(-4, 12f), "never below zero");
            Assert.AreNotEqual(ApepBarqueLinkRules.FreedMarkKey, ApepSpitRiverRules.OwedKey);
        }

        [Test]
        public void Apep_StoneExists_AndTheTextsHaveBothLanguages()
        {
            Assert.IsNotNull(ItemDatabase.GetById(ApepSpitRiverRules.StoneItemId));
            foreach (var entry in ApepSpitRiverTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), entry.Key);
                StringAssert.DoesNotContain("Dona ", entry.PtBR);
                StringAssert.DoesNotContain("Seu ", entry.PtBR);
            }

            foreach (CharacterId hero in Cousins)
            {
                Assert.IsNotNull(ApepSpitRiverTexts.LineFor(hero), hero.ToString());
            }

            Assert.IsNull(ApepSpitRiverTexts.LineFor(null));
            foreach (string key in new[] { ApepSpitRiverTexts.Spat, ApepSpitRiverTexts.SpatMidnight, ApepSpitRiverTexts.BagFull, ApepSpitRiverTexts.Paid })
            {
                StringAssert.Contains("{0}", ApepSpitRiverTexts.Get(null, key), key);
            }
        }
    }
}
