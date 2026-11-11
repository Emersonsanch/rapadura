using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.UI.Bestiary;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>The Quatro Gulosos (Docs/Mapas100.md 46): Taotie eating the offerings on the ground, Qiongqi swapping the steps. Pure rules only.</summary>
    public class FourGluttonsAppetiteTests
    {
        [Test]
        public void TaotieSpill_StopsAtTheGroundAndFightCaps()
        {
            Assert.IsTrue(RegionBossRules.TaotieShouldSpill(0, 0));
            Assert.IsFalse(RegionBossRules.TaotieShouldSpill(RegionBossRules.TaotieMaxOfferingsOnGround, 1), "ground full");
            Assert.IsFalse(RegionBossRules.TaotieShouldSpill(0, RegionBossRules.TaotieMaxOfferingsPerFight), "fight used up");
            Assert.IsTrue(RegionBossRules.TaotieShouldSpill(RegionBossRules.TaotieMaxOfferingsOnGround - 1, RegionBossRules.TaotieMaxOfferingsPerFight - 1));
        }

        [Test]
        public void TaotieSpillSpot_LandsInTheRingRoundTheVessel()
        {
            Vector2 boss = new Vector2(10f, -4f);
            for (int i = 0; i <= 10; i++)
            {
                float t = i / 10f;
                float distance = Vector2.Distance(boss, RegionBossRules.TaotieSpillSpot(boss, t, 1f - t));
                Assert.That(distance, Is.InRange(RegionBossRules.TaotieSpillMinRadius - 1e-3f, RegionBossRules.TaotieSpillMaxRadius + 1e-3f));
            }

            Assert.AreEqual(RegionBossRules.TaotieSpillMaxRadius, Vector2.Distance(boss, RegionBossRules.TaotieSpillSpot(boss, 0.3f, 5f)), 1e-3f, "clamped");
        }

        [Test]
        public void TaotieChoose_TakesTheNearestToItsMouthInReach()
        {
            Vector2 boss = Vector2.zero;
            Vector2 hero = new Vector2(-20f, 0f);
            var offerings = new List<Vector2> { new Vector2(6f, 0f), new Vector2(0f, 3f), new Vector2(RegionBossRules.TaotieEatReach + 1f, 0f) };
            Assert.AreEqual(1, RegionBossRules.TaotieChooseOffering(offerings, boss, hero));

            var far = new List<Vector2> { new Vector2(RegionBossRules.TaotieEatReach + 1f, 0f) };
            Assert.AreEqual(-1, RegionBossRules.TaotieChooseOffering(far, boss, hero), "out of reach");
            Assert.AreEqual(-1, RegionBossRules.TaotieChooseOffering(new List<Vector2>(), boss, hero));
            Assert.AreEqual(-1, RegionBossRules.TaotieChooseOffering(null, boss, hero));

            // The one the hero stands on is being picked up: Taotie goes for the next.
            Assert.AreEqual(0, RegionBossRules.TaotieChooseOffering(offerings, boss, new Vector2(0f, 3.5f)));
        }

        [Test]
        public void TaotiePickup_ByWalkingOverIt()
        {
            Vector2 offering = new Vector2(3f, 3f);
            Assert.IsTrue(RegionBossRules.TaotieOfferingPicked(offering, offering));
            Assert.IsTrue(RegionBossRules.TaotieOfferingPicked(offering, offering + Vector2.right * (RegionBossRules.TaotieOfferingPickupRadius - 0.01f)));
            Assert.IsFalse(RegionBossRules.TaotieOfferingPicked(offering, offering + Vector2.right * (RegionBossRules.TaotieOfferingPickupRadius + 0.01f)));
        }

        [Test]
        public void TaotieReturn_IsDoubledWhatItAte_PlusWhatIsLeft()
        {
            int coins = RegionBossRules.TaotieOfferingCoins;
            Assert.AreEqual(0, RegionBossRules.TaotieDeathReturn(0, 0));
            Assert.AreEqual(2 * coins, RegionBossRules.TaotieDeathReturn(1, 0), "one eaten comes back twice");
            Assert.AreEqual(3 * 2 * coins + coins, RegionBossRules.TaotieDeathReturn(3, 1));
            Assert.AreEqual(0, RegionBossRules.TaotieDeathReturn(-2, -1));
            Assert.AreEqual(2, RegionBossRules.TaotieReturnFactor);

            // The choice is real: letting it eat is worth more than grabbing, but only if it falls.
            Assert.Greater(RegionBossRules.TaotieDeathReturn(1, 0), RegionBossRules.TaotieDeathReturn(0, 1));
            Assert.Greater(RegionBossRules.TaotieEatWindupSeconds, 2f, "time to run and grab it");
        }

        [Test]
        public void Qiongqi_SwapsOnlyOnItsAppetiteAndInTheFight()
        {
            Assert.IsTrue(RegionBossRules.QiongqiSwapsSteps(GluttonAppetite.Qiongqi, 2f));
            Assert.IsTrue(RegionBossRules.QiongqiSwapsSteps(GluttonAppetite.Qiongqi, RegionBossRules.AbilityRange));
            Assert.IsFalse(RegionBossRules.QiongqiSwapsSteps(GluttonAppetite.Qiongqi, RegionBossRules.AbilityRange + 1f));
            Assert.IsFalse(RegionBossRules.QiongqiSwapsSteps(GluttonAppetite.Taotie, 2f));
            Assert.IsFalse(RegionBossRules.QiongqiSwapsSteps(GluttonAppetite.Hundun, 2f));
            Assert.IsFalse(RegionBossRules.QiongqiSwapsSteps(GluttonAppetite.Taowu, 2f));
            Assert.That(RegionBossRules.QiongqiMirrorSeconds, Is.InRange(3f, 4f));
            Assert.LessOrEqual(RegionBossRules.QiongqiMirrorSeconds, HeroControlMirror.MaxSeconds);
        }

        [Test]
        public void Mirror_EndsByItselfAndIsCapped()
        {
            Assert.IsTrue(HeroControlMirror.IsActiveAt(10f, 13f));
            Assert.IsFalse(HeroControlMirror.IsActiveAt(13f, 13f));
            Assert.IsFalse(HeroControlMirror.IsActiveAt(10f, -1f), "off by default");

            Assert.AreEqual(13f, HeroControlMirror.EndTime(10f, -1f, 3f), 1e-4f);
            Assert.AreEqual(10f + HeroControlMirror.MaxSeconds, HeroControlMirror.EndTime(10f, -1f, 60f), 1e-4f, "capped");
            Assert.AreEqual(10f + HeroControlMirror.MaxSeconds, HeroControlMirror.EndTime(10f, 500f, 3f), 1e-4f, "a stale end time is pulled back");
            Assert.AreEqual(13.5f, HeroControlMirror.EndTime(11f, 13.5f, 1f), 1e-4f, "never shortened");
            Assert.AreEqual(10f, HeroControlMirror.EndTime(10f, -1f, -5f), 1e-4f);
        }

        [Test]
        public void Mirror_FlipsTheStickAndTheTappedPointThroughTheHero()
        {
            Assert.AreEqual(new Vector2(-1f, 0.5f), HeroControlMirror.MirrorMove(new Vector2(1f, -0.5f)));
            Assert.AreEqual(Vector2.zero, HeroControlMirror.MirrorMove(Vector2.zero));

            Vector3 hero = new Vector3(5f, 2f, 5f);
            Vector3 mirrored = HeroControlMirror.MirrorPoint(hero, new Vector3(8f, 0.5f, 1f));
            Assert.AreEqual(new Vector3(2f, 2f, 9f), mirrored);
            Assert.AreEqual(hero, HeroControlMirror.MirrorPoint(hero, hero), "a tap on the hero stays put");
            Vector3 flat = new Vector3(8f, 2f, 1f) - hero;
            Assert.AreEqual(flat.magnitude, (mirrored - hero).magnitude, 1e-4f, "same distance, other side");
        }

        [Test]
        public void FightLines_HaveBothLanguages_AndTheNumberSlot()
        {
            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            foreach (string key in RegionBossTexts.FourGluttonsFightKeys)
            {
                Assert.IsTrue(table.TryGetValue(key, out LocalizationEntry entry), key);
                Assert.IsFalse(string.IsNullOrEmpty(entry.En), key);
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), key);
            }

            foreach (string key in new[] { RegionBossTexts.TaotieEatenKey, RegionBossTexts.TaotieReturnedKey, RegionBossTexts.QiongqiMirrorKey })
            {
                StringAssert.Contains("{0}", table[key].En, key);
                StringAssert.Contains("{0}", table[key].PtBR, key);
            }
        }
    }
}
