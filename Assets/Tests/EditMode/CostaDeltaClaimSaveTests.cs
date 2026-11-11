using NUnit.Framework;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The reef chest of the Costa and the spark of the Delta are taken once per tide / island layout in the save (WorldMapProgress), not in the driver's
    /// memory: a save and reload, or a world rebuilt from the same progress, inside the same tide or layout cannot hand the prize out again.
    /// </summary>
    public class CostaDeltaClaimSaveTests
    {
        private const long Now = 1_800_000_000L;

        private static WorldMapProgress Reload(WorldMapProgress progress)
        {
            var reloaded = new WorldMapProgress();
            reloaded.RestoreState(JsonUtility.FromJson<WorldMapSaveState>(JsonUtility.ToJson(progress.CaptureState())));
            return reloaded;
        }

        [Test]
        public void ReefChest_TakenThenReloaded_IsSpentForTheSameTide_AndOpensOnTheNext()
        {
            long tide = CostaTideRules.TideIndex(Now);
            var progress = new WorldMapProgress();
            Assert.IsTrue(CostaTideRules.CanOpenChest(CostaDeltaTetoMechanicsDriver.ReefChestTide(progress, 1), tide, CostaTidePhase.Low));
            CostaDeltaTetoMechanicsDriver.StoreReefChest(progress, 1, tide);

            WorldMapProgress reloaded = Reload(progress);
            long saved = CostaDeltaTetoMechanicsDriver.ReefChestTide(reloaded, 1);

            Assert.AreEqual(tide, saved);
            Assert.IsFalse(CostaTideRules.CanOpenChest(saved, tide, CostaTidePhase.Low));
            Assert.IsFalse(CostaTideRules.CanOpenChest(saved, tide, CostaTidePhase.Turning));
            Assert.IsTrue(CostaTideRules.CanOpenChest(saved, tide + 1L, CostaTidePhase.Low));
            Assert.IsTrue(CostaTideRules.CanOpenChest(CostaDeltaTetoMechanicsDriver.ReefChestTide(reloaded, 0), tide, CostaTidePhase.Low), "each chest has its own mark");
        }

        [Test]
        public void ReefChest_SameProgressSeenByARebuiltWorld_IsStillSpent()
        {
            long tide = CostaTideRules.TideIndex(Now);
            var progress = new WorldMapProgress();
            CostaDeltaTetoMechanicsDriver.StoreReefChest(progress, 0, tide);

            // A rebuilt (or re-enabled) driver starts with empty memory; what it reads is the progress.
            Assert.IsFalse(CostaTideRules.CanOpenChest(CostaDeltaTetoMechanicsDriver.ReefChestTide(progress, 0), tide, CostaTidePhase.Low));
        }

        [Test]
        public void Spark_FreedThenReloaded_IsGoneForTheSameLayout_AndBackOnTheNext()
        {
            long layout = DeltaIslandRules.LayoutIndex(Now);
            var progress = new WorldMapProgress();
            Assert.IsTrue(DeltaIslandRules.CanFreeSpark(CostaDeltaTetoMechanicsDriver.SparkLayout(progress), layout));
            CostaDeltaTetoMechanicsDriver.StoreSpark(progress, layout);

            long saved = CostaDeltaTetoMechanicsDriver.SparkLayout(Reload(progress));

            Assert.AreEqual(layout, saved);
            Assert.IsFalse(DeltaIslandRules.CanFreeSpark(saved, layout));
            Assert.IsTrue(DeltaIslandRules.CanFreeSpark(saved, layout + 1L));
        }

        [Test]
        public void OldSave_WithoutTheKeys_ReadsNotTaken()
        {
            var progress = new WorldMapProgress();
            progress.RestoreState(JsonUtility.FromJson<WorldMapSaveState>("{\"currentRegion\":0,\"unlockedRegions\":[0]}"));
            long tide = CostaTideRules.TideIndex(Now);
            long layout = DeltaIslandRules.LayoutIndex(Now);

            Assert.AreEqual(CostaDeltaTetoMechanicsDriver.NeverTaken, CostaDeltaTetoMechanicsDriver.ReefChestTide(progress, 2));
            Assert.AreEqual(CostaDeltaTetoMechanicsDriver.NeverTaken, CostaDeltaTetoMechanicsDriver.SparkLayout(progress));
            Assert.IsTrue(CostaTideRules.CanOpenChest(CostaDeltaTetoMechanicsDriver.ReefChestTide(progress, 2), tide, CostaTidePhase.Low));
            Assert.IsTrue(DeltaIslandRules.CanFreeSpark(CostaDeltaTetoMechanicsDriver.SparkLayout(progress), layout));
            Assert.IsTrue(CostaTideRules.CanOpenChest(CostaDeltaTetoMechanicsDriver.ReefChestTide(null, 2), tide, CostaTidePhase.Low));
        }

        [Test]
        public void NewGame_ClearsTheMarks()
        {
            var progress = new WorldMapProgress();
            CostaDeltaTetoMechanicsDriver.StoreReefChest(progress, 0, 7L);
            CostaDeltaTetoMechanicsDriver.StoreSpark(progress, 9L);
            progress.ResetToNewGame();

            Assert.AreEqual(CostaDeltaTetoMechanicsDriver.NeverTaken, CostaDeltaTetoMechanicsDriver.ReefChestTide(progress, 0));
            Assert.AreEqual(CostaDeltaTetoMechanicsDriver.NeverTaken, CostaDeltaTetoMechanicsDriver.SparkLayout(progress));
        }
    }
}
