using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>The regions' saved state: it survives a save and reload, a new game clears it, and a save from before these fields loads clean.</summary>
    public class RegionMechanicsSaveTests
    {
        private const long Now = 1_800_000_000L;
        private const long Today = 20_833L;

        private static WorldMapProgress Reload(WorldMapProgress progress)
        {
            var reloaded = new WorldMapProgress();
            reloaded.RestoreState(JsonUtility.FromJson<WorldMapSaveState>(JsonUtility.ToJson(progress.CaptureState())));
            return reloaded;
        }

        [Test]
        public void TimeMarks_SurviveASaveAndAreClearedOnNewGame()
        {
            var progress = new WorldMapProgress();
            Assert.AreEqual(-5L, progress.GetTimeMark("spot", -5L));
            progress.SetTimeMark("spot", Now);

            WorldMapProgress reloaded = Reload(progress);

            Assert.AreEqual(Now, reloaded.GetTimeMark("spot"));
            reloaded.ResetToNewGame();
            Assert.AreEqual(0L, reloaded.GetTimeMark("spot"));
        }

        [TestCase(NartesMechanicsDriver.RaceDayKey)]
        [TestCase(NartesMechanicsDriver.ChainDayKey)]
        [TestCase(NartesMechanicsDriver.GoldDayKeyPrefix + "2")]
        public void NartesClaim_SurvivesASaveAndIsClearedOnNewGame(string key)
        {
            var progress = new WorldMapProgress();
            Assert.AreEqual(NartesSkyGoldRules.NeverTaken, NartesMechanicsDriver.ClaimedDay(progress, key));
            NartesMechanicsDriver.StoreClaim(progress, key, Today);

            WorldMapProgress reloaded = Reload(progress);

            Assert.AreEqual(Today, NartesMechanicsDriver.ClaimedDay(reloaded, key));
            reloaded.ResetToNewGame();
            Assert.AreEqual(NartesSkyGoldRules.NeverTaken, NartesMechanicsDriver.ClaimedDay(reloaded, key));
        }

        [Test]
        public void NartesGold_StaysCooledAfterAReloadAndComesBackTheNextDay()
        {
            string key = NartesMechanicsDriver.GoldDayKeyPrefix + "0";
            var progress = new WorldMapProgress();
            NartesMechanicsDriver.StoreClaim(progress, key, Today);

            long saved = NartesMechanicsDriver.ClaimedDay(Reload(progress), key);

            Assert.AreEqual(NartesSkyGoldCheck.Cooled, NartesSkyGoldRules.Check(CharacterId.Lavine, saved, Today));
            Assert.AreEqual(NartesSkyGoldCheck.Take, NartesSkyGoldRules.Check(CharacterId.Lavine, saved, Today + 1L));
        }

        [Test]
        public void CeuSeal_SurvivesASaveForItsTenMinutesAndIsClearedOnNewGame()
        {
            var progress = new WorldMapProgress();
            CeuMechanicsDriver.StoreSeal(progress, 3, Now);

            WorldMapProgress reloaded = Reload(progress);

            Assert.IsTrue(CeuMechanicsDriver.CrackSealedIn(reloaded, 3, Now + CeuSkyHoldRules.SealSeconds - 1L));
            Assert.IsFalse(CeuMechanicsDriver.CrackSealedIn(reloaded, 3, Now + CeuSkyHoldRules.SealSeconds));
            Assert.IsFalse(CeuMechanicsDriver.CrackSealedIn(reloaded, 2, Now));
            reloaded.ResetToNewGame();
            Assert.IsFalse(CeuMechanicsDriver.CrackSealedIn(reloaded, 3, Now));
        }

        [Test]
        public void CeuSeal_FromAClockGoneBackDoesNotLockTheCrack()
        {
            var progress = new WorldMapProgress();
            CeuMechanicsDriver.StoreSeal(progress, 0, Now);

            Assert.IsFalse(CeuMechanicsDriver.CrackSealedIn(progress, 0, Now - 86400L));
        }

        [Test]
        public void HinokuraForge_SurvivesASaveInsideItsBurnAndIsClearedOnNewGame()
        {
            long litAt = HinokuraForgeRules.TremorCycle(Now) * HinokuraForgeRules.TremorPeriodSeconds + 1L;
            int forge = HinokuraForgeRules.StruckForge(HinokuraForgeRules.TremorCycle(litAt) + 2L);
            var progress = new WorldMapProgress();
            HinokuraMechanicsDriver.StoreForge(progress, forge, litAt);

            WorldMapProgress reloaded = Reload(progress);
            var loaded = new long[HinokuraForgeRules.Forges];
            HinokuraMechanicsDriver.ReadForges(reloaded, loaded);

            Assert.AreEqual(litAt, loaded[forge]);
            Assert.AreEqual(HinokuraForgeRules.NeverLit, loaded[(forge + 1) % HinokuraForgeRules.Forges]);
            Assert.IsTrue(HinokuraForgeRules.IsLit(forge, loaded[forge], litAt + HinokuraForgeRules.BurnSeconds - 1L));
            Assert.IsFalse(HinokuraForgeRules.IsLit(forge, loaded[forge], litAt + HinokuraForgeRules.BurnSeconds));

            reloaded.ResetToNewGame();
            HinokuraMechanicsDriver.ReadForges(reloaded, loaded);
            Assert.AreEqual(HinokuraForgeRules.NeverLit, loaded[forge]);
        }

        [Test]
        public void OldSave_WithoutTheFields_LoadsEmpty()
        {
            var progress = new WorldMapProgress();
            progress.RestoreState(JsonUtility.FromJson<WorldMapSaveState>("{\"currentRegion\":0,\"unlockedRegions\":[0]}"));
            AssertNothingStored(progress);
        }

        [Test]
        public void OldSave_WithNullLists_LoadsEmpty()
        {
            var progress = new WorldMapProgress();
            progress.RestoreState(new WorldMapSaveState { timeMarkKeys = null, timeMarkValues = null, dailyMarkKeys = null, dailyMarkDays = null });
            AssertNothingStored(progress);
        }

        private static void AssertNothingStored(WorldMapProgress progress)
        {
            var loaded = new long[HinokuraForgeRules.Forges];
            HinokuraMechanicsDriver.ReadForges(progress, loaded);
            Assert.AreEqual(HinokuraForgeRules.NeverLit, loaded[0]);
            Assert.IsFalse(CeuMechanicsDriver.CrackSealedIn(progress, 0, Now));
            Assert.AreEqual(NartesSkyGoldRules.NeverTaken, NartesMechanicsDriver.ClaimedDay(progress, NartesMechanicsDriver.RaceDayKey));
        }
    }
}
