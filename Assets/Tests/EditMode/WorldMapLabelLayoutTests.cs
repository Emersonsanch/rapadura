using NUnit.Framework;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Playtest regression: world map region cards overlapped (Montanha's card over Deserto/Cavernas).
    /// Covers the compact-vs-full card rule and the screen-space overlap avoidance in
    /// <see cref="WorldMapHudPresenter"/>.
    /// </summary>
    public class WorldMapLabelLayoutTests
    {
        [Test]
        public void ShowFullLabel_OnlyCurrentTravelableAndAdjacentLockedGoal()
        {
            WorldMapGraph graph = WorldMapGraph.CreateDefault();
            var progress = new WorldMapProgress(BiomeType.Forest);
            progress.Unlock(BiomeType.Forest);

            Assert.IsTrue(WorldMapHudPresenter.ShowFullLabel(graph, progress, BiomeType.Forest, RegionLabelStatus.Current));
            Assert.IsTrue(WorldMapHudPresenter.ShowFullLabel(graph, progress, BiomeType.Desert, RegionLabelStatus.Travelable));
            Assert.IsTrue(WorldMapHudPresenter.ShowFullLabel(graph, progress, BiomeType.Desert, RegionLabelStatus.Locked), "Adjacent locked region = next goal, keeps its requirement.");
            Assert.IsFalse(WorldMapHudPresenter.ShowFullLabel(graph, progress, BiomeType.Mountain, RegionLabelStatus.Locked), "Far locked region collapses to a chip.");
            Assert.IsFalse(WorldMapHudPresenter.ShowFullLabel(graph, progress, BiomeType.Caves, RegionLabelStatus.Locked));
            Assert.IsFalse(WorldMapHudPresenter.ShowFullLabel(graph, progress, BiomeType.Mountain, RegionLabelStatus.None));
        }

        [Test]
        public void ResolveLabelOverlaps_LowerPriorityLabelMovesClear_HigherStays()
        {
            var rects = new[]
            {
                new Rect(100f, 100f, 160f, 60f), // Montanha (compact/low priority)
                new Rect(140f, 120f, 160f, 60f), // Deserto (current)
            };
            var priorities = new[] { 3, 0 };
            var offsets = new float[2];
            var order = new int[2];

            WorldMapHudPresenter.ResolveLabelOverlaps(rects, priorities, 2, 4f, offsets, order);

            Assert.AreEqual(0f, offsets[1], "Current region's label never moves.");
            Rect moved = rects[0];
            moved.y += offsets[0];
            Rect fixedRect = rects[1];
            Assert.IsFalse(moved.Overlaps(fixedRect), "Lower-priority label must be pushed clear.");
            Assert.Less(offsets[0], 0f, "Smaller move here is upward (label sits mostly above).");
        }

        [Test]
        public void ResolveLabelOverlaps_ThreeStackedLabels_EndUpDisjoint()
        {
            var rects = new[]
            {
                new Rect(0f, 0f, 100f, 40f),
                new Rect(10f, 10f, 100f, 40f),
                new Rect(20f, 20f, 100f, 40f),
            };
            var priorities = new[] { 1, 2, 3 };
            var offsets = new float[3];
            var order = new int[3];

            WorldMapHudPresenter.ResolveLabelOverlaps(rects, priorities, 3, 2f, offsets, order);

            for (int a = 0; a < 3; a++)
            {
                for (int b = a + 1; b < 3; b++)
                {
                    Rect ra = rects[a];
                    ra.y += offsets[a];
                    Rect rb = rects[b];
                    rb.y += offsets[b];
                    Assert.IsFalse(ra.Overlaps(rb), $"Labels {a} and {b} still overlap.");
                }
            }
        }

        [Test]
        public void ResolveLabelOverlaps_SeparateOrUnlaidOutLabels_AreUntouched()
        {
            var rects = new[]
            {
                new Rect(0f, 0f, 100f, 40f),
                new Rect(300f, 0f, 100f, 40f),
                new Rect(0f, 0f, 0f, 0f), // not laid out yet
            };
            var offsets = new float[3];

            WorldMapHudPresenter.ResolveLabelOverlaps(rects, new[] { 3, 3, 3 }, 3, 4f, offsets, new int[3]);

            CollectionAssert.AreEqual(new[] { 0f, 0f, 0f }, offsets);
        }
    }
}
