using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The Falta items of maps 76–96 done on 2026-11-16: the freed barque against Apep (76), the time bubbles quickening the swings (78), fire on
    /// O Rei do Piche's tar (79), the resin lanterns taking Hobabish's mantles (80), Hahya and Ohya beside the cousins (82) and the last Arca
    /// Resgatada +5 of Āz (96): pure rules, the places the layouts give them and their texts.
    /// </summary>
    public class LateBossLinksMechanicsTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            MapLayouts.ClearCache();
            OverworldLayouts.ClearCache();
        }

        // ---------------- 76 · the freed barque against Apep ----------------

        [Test]
        public void Apep_FreedBarque_WeakensTheDrink_AndStopsTheHealing()
        {
            Assert.AreEqual(RegionBossRules.DrinkDamageFactor, ApepBarqueLinkRules.DrinkDamageFactor(false), 1e-5f);
            Assert.AreEqual(RegionBossRules.DrinkHealFraction, ApepBarqueLinkRules.DrinkHealFraction(false), 1e-5f);
            Assert.AreEqual(RegionBossRules.DrinkDamageFactor * 0.75f, ApepBarqueLinkRules.DrinkDamageFactor(true), 1e-5f);
            Assert.Less(ApepBarqueLinkRules.DrinkDamageFactor(true), 1f, "the drink hits softer than her plain blow");
            Assert.AreEqual(0f, ApepBarqueLinkRules.DrinkHealFraction(true));
            Assert.IsTrue(RioBarqueDefenseRules.FreedToday(4, 4));
            Assert.IsFalse(RioBarqueDefenseRules.FreedToday(3, 4), "yesterday's barque is stranded again");
            Assert.AreEqual("rio.barque.freed", ApepBarqueLinkRules.FreedMarkKey, "the daily mark the Rio driver writes");
        }

        // ---------------- 78 · the time bubbles quicken the swings ----------------

        [Test]
        public void TimeBubbles_ChangeTheSwings_OfHeroAndBeastAlike()
        {
            Assert.AreEqual(0.3f, MisturaTimeBubbleRules.AttackBonus(MisturaTimeBubbleRules.FastFactor), 1e-5f);
            Assert.AreEqual(-0.3f, MisturaTimeBubbleRules.AttackBonus(MisturaTimeBubbleRules.SlowFactor), 1e-5f);
            Assert.AreEqual(1f, EnemyController.AttackRecoveryRate(0f), 1e-5f);
            Assert.AreEqual(1.3f, EnemyController.AttackRecoveryRate(0.3f), 1e-5f);
            Assert.AreEqual(0.7f, EnemyController.AttackRecoveryRate(-0.3f), 1e-5f);
            Assert.AreEqual(0.5f, EnemyController.AttackRecoveryRate(-3f), 1e-5f, "never frozen");

            float fast = PlayerBasicAttack.ResolveCooldown(1f, MisturaTimeBubbleRules.AttackBonus(MisturaTimeBubbleRules.FastFactor), 0f);
            float slow = PlayerBasicAttack.ResolveCooldown(1f, MisturaTimeBubbleRules.AttackBonus(MisturaTimeBubbleRules.SlowFactor), 0f);
            Assert.AreEqual(1f / 1.3f, fast, 1e-4f);
            Assert.AreEqual(1f / 0.7f, slow, 1e-4f);
            Assert.AreEqual(1f / EnemyController.AttackRecoveryRate(0.3f), fast, 1e-4f, "a beast recovers at the hero's pace");
        }

        // ---------------- 79 · fire on the King's tar ----------------

        [Test]
        public void PitchKing_FireInTheWarning_OnTheTar_BurnsIt()
        {
            Vector2 king = new Vector2(100f, 100f);
            Assert.IsTrue(PitchKingBurnRules.Burns(king, king + Vector2.right * 5.9f, 0f, 1.0d, 0.5d, 3.1d));
            Assert.IsTrue(PitchKingBurnRules.Burns(king, king + Vector2.right * 8f, 2.5f, 1.0d, 0.5d, 3.1d), "the area of the fire counts");
            Assert.IsFalse(PitchKingBurnRules.Burns(king, king + Vector2.right * 8f, 0f, 1.0d, 0.5d, 3.1d), "off the tar");
            Assert.IsFalse(PitchKingBurnRules.Burns(king, king, 0f, 0.4d, 0.5d, 3.1d), "before the warning the tar is not spread");
            Assert.IsFalse(PitchKingBurnRules.Burns(king, king, 0f, 3.2d, 0.5d, 3.1d), "too late: it hardened");
            Assert.IsTrue(PitchKingBurnRules.FlashHits(PitchKingBurnRules.TarRadius));
            Assert.IsFalse(PitchKingBurnRules.FlashHits(PitchKingBurnRules.TarRadius + 0.1f), "burn it and step out");
            Assert.Less(PitchKingBurnRules.FlashDamageFactor, RegionBossRules.TarDamageFactor, "the flash costs less than being stuck");
        }

        [Test]
        public void PitchKing_Board_KeepsTheLastFires_AndCountsTheBurns()
        {
            PitchKingBurnBoard.Reset();
            Vector2 king = new Vector2(-30f, 40f);
            Assert.IsFalse(PitchKingBurnBoard.BurnsTar(king, 0d, 10d), "no fire yet");
            PitchKingBurnBoard.Record(king + Vector2.up * 3f, 0f, 5d);
            Assert.IsTrue(PitchKingBurnBoard.BurnsTar(king, 4d, 6d));
            Assert.AreEqual(1, PitchKingBurnBoard.Burns);
            Assert.IsFalse(PitchKingBurnBoard.BurnsTar(king, 6d, 8d), "that fire was before this warning");
            for (int i = 0; i < 4; i++)
            {
                PitchKingBurnBoard.Record(king + Vector2.right * 50f, 0f, 20d + i);
            }

            Assert.IsFalse(PitchKingBurnBoard.BurnsTar(king, 0d, 30d), "four fires elsewhere push the old one out");
            Assert.AreEqual(1, PitchKingBurnBoard.Burns);
            PitchKingBurnBoard.Reset();
        }

        // ---------------- 80 · the resin lanterns ----------------

        [Test]
        public void ResinLanterns_StandOnTheCedarRing_RoundHobabish()
        {
            Assert.AreEqual(RegionBossRules.MantleCount, ResinLanternRules.LanternCount, "one lantern for each mantle");
            var spots = new List<Vector2>();
            for (int i = 0; i < ResinLanternRules.LanternCount; i++)
            {
                Vector2 spot = OverworldLayouts.ResinLanternPosition(i);
                Assert.IsTrue(OverworldLayouts.MisturaBounds.Contains(spot), $"lantern {i} is in the region");
                Assert.AreEqual(i, ResinLanternRules.LanternNear(spots.Append(spot).ToList(), spot + Vector2.one, ResinLanternRules.Reach));
                spots.Add(spot);
            }

            Assert.Less(Vector2.Distance(OverworldLayouts.HobabishArena, OverworldLayouts.CedarRingCenter), OverworldLayouts.ResinLanternRingRadius,
                "Hobabish's ring is inside the lanterns'");
            Assert.AreEqual(-1, ResinLanternRules.LanternNear(spots, OverworldLayouts.CedarRingCenter, ResinLanternRules.Reach));
            Assert.AreEqual(ResinLanternRules.LanternCount, Enumerable.Range(0, ResinLanternRules.LanternCount).Select(ResinLanternRules.MarkKey).Distinct().Count());
        }

        [Test]
        public void ResinLanterns_EachLitToday_TakesAMantle_DownToOne()
        {
            Assert.AreEqual(7, ResinLanternRules.MantleCap(0));
            Assert.AreEqual(4, ResinLanternRules.MantleCap(3));
            Assert.AreEqual(1, ResinLanternRules.MantleCap(6));
            Assert.AreEqual(1, ResinLanternRules.MantleCap(7), "the newest mantle is always his");
            Assert.AreEqual(1, ResinLanternRules.MantleCap(12));
            Assert.AreEqual(7, ResinLanternRules.MantleCap(-2));

            int[] days = { 5, 5, 4, 0, 5, 6, 5 };
            Assert.AreEqual(4, ResinLanternRules.LitCount(days, 5), "yesterday's and tomorrow's don't burn today");
            Assert.AreEqual(0, ResinLanternRules.LitCount(days, 9));
            Assert.IsFalse(ResinLanternRules.LitToday(0, 0), "never lit");
            Assert.IsTrue(ResinLanternRules.AllLit(7));
            Assert.IsFalse(ResinLanternRules.AllLit(6));

            var mantles = new List<Vector2>();
            for (int i = 0; i < 10; i++)
            {
                RegionBossRules.ThrowMantle(mantles, new Vector2(i, 0f), i);
                ResinLanternRules.TrimMantles(mantles, ResinLanternRules.MantleCap(3));
            }

            Assert.AreEqual(4, mantles.Count);
            Assert.AreEqual(new Vector2(9f, 0f), mantles[mantles.Count - 1], "the newest stays");
            Assert.AreEqual(new Vector2(6f, 0f), mantles[0], "the oldest go back on him");

            ResinLanternBoard.Set(7);
            Assert.AreEqual(1, ResinLanternBoard.MantleCap);
            ResinLanternBoard.Set(99);
            Assert.AreEqual(ResinLanternRules.LanternCount, ResinLanternBoard.Lit);
            ResinLanternBoard.Reset();
            Assert.AreEqual(RegionBossRules.MantleCount, ResinLanternBoard.MantleCap);
        }

        // ---------------- 82 · Hahya and Ohya ----------------

        [Test]
        public void DreamGiants_StrikeOnTheirTurns_OnlyWhenFreed()
        {
            Assert.AreEqual(0, DreamGiantsRules.Allies(false, false));
            Assert.AreEqual(2, DreamGiantsRules.Allies(true, true));
            Assert.IsFalse(DreamGiantsRules.StrikesBetween(0f, 14.9f, 0f));
            Assert.IsTrue(DreamGiantsRules.StrikesBetween(14.9f, 15.1f, 0f), "Hahya at 15 s");
            Assert.IsFalse(DreamGiantsRules.StrikesBetween(14.9f, 15.1f, DreamGiantsRules.OhyaOffsetSeconds));
            Assert.IsTrue(DreamGiantsRules.StrikesBetween(22.4f, 22.6f, DreamGiantsRules.OhyaOffsetSeconds), "Ohya 7.5 s later");
            Assert.AreEqual(4, DreamGiantsRules.BlowsBy(60f, 0f));
            Assert.AreEqual(3, DreamGiantsRules.BlowsBy(60f, DreamGiantsRules.OhyaOffsetSeconds));
            Assert.AreEqual(20f, DreamGiantsRules.BlowDamage(1000f), 1e-4f);
            float perMinute = (DreamGiantsRules.BlowsBy(60f, 0f) + DreamGiantsRules.BlowsBy(60f, DreamGiantsRules.OhyaOffsetSeconds)) * DreamGiantsRules.StrikeFraction;
            Assert.Less(perMinute, 0.15f, "a help, never the whole fight");

            var hahya = Resources.Load<EnemyDefinition>("Enemies/" + DreamGiantsRules.HahyaId);
            var ohya = Resources.Load<EnemyDefinition>("Enemies/" + DreamGiantsRules.OhyaId);
            Assert.IsNotNull(hahya, "Hahya's sheet");
            Assert.IsNotNull(ohya, "Ohya's sheet");
            Assert.IsTrue(hahya.IsBoss && ohya.IsBoss, "beating a boss is what the registry keeps");
        }

        // ---------------- 96 · the last Arca Resgatada +5 ----------------

        [Test]
        public void AzChest_IsTheInvasionsPlusFive_OncePerDay_WithoutTheHorse()
        {
            Assert.IsTrue(AzRescuedChestRules.CanClaim(0L, 20400L));
            Assert.IsFalse(AzRescuedChestRules.CanClaim(20400L, 20400L), "she comes back in 5 min, the chest only tomorrow");
            Assert.IsTrue(AzRescuedChestRules.CanClaim(20400L, 20401L));

            for (long day = 20400L; day < 20430L; day++)
            {
                var chest = AzRescuedChestRules.Chest(day);
                Assert.AreEqual(5, chest.Tier);
                Assert.IsNull(chest.MountId, "the horse stays with the bando");
                Assert.Greater(chest.Gold, 0);
                Assert.Greater(chest.Experience, 0);
                Assert.AreEqual(Rapadura.Gameplay.WorldEvents.InvasionRules.ChestBlessing[5], chest.BlessingStones);
                Assert.AreEqual(Rapadura.Gameplay.WorldEvents.InvasionRules.ChestSoul[5], chest.SoulStones);
            }

            Assert.AreEqual(AzRescuedChestRules.Seed(20400L), AzRescuedChestRules.Seed(20400L), "the same chest for everyone that day");
            Assert.AreEqual(RegionBossRules.AzCaptainId, "enemy_mountain_az_captain");
        }

        // ---------------- texts ----------------

        [Test]
        public void Texts_HaveBothLanguages_AndALinePerCousinForEachMoment()
        {
            var entries = LateBossLinkTexts.Entries;
            Assert.AreEqual(entries.Count, entries.Select(e => e.Key).Distinct().Count(), "no key twice");
            foreach (var entry in entries)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), entry.Key);
                Assert.IsFalse(entry.PtBR.Contains("Dona ") || entry.PtBR.Contains("Seu "), entry.Key);
            }

            string[] names = { "Maithe:", "Ícaro:", "Joaquim:", "Maria:", "Lavine:" };
            foreach (LateBossLinkTexts.Moment moment in System.Enum.GetValues(typeof(LateBossLinkTexts.Moment)))
            {
                for (int cousin = 0; cousin < names.Length; cousin++)
                {
                    string key = LateBossLinkTexts.LineKey(moment, cousin);
                    string pt = LateBossLinkTexts.Get(null, key);
                    Assert.AreNotEqual(key, pt, key);
                    StringAssert.StartsWith(names[cousin], pt, key);
                }

                Assert.AreEqual(LateBossLinkTexts.LineKey(moment, 2), LateBossLinkTexts.LineFor(moment, null), "Joaquim when no cousin is known");
            }

            StringAssert.Contains("{0}", LateBossLinkTexts.Get(null, LateBossLinkTexts.ResinLit));
            StringAssert.Contains("{1}", LateBossLinkTexts.Get(null, LateBossLinkTexts.AzChest));
        }
    }
}
