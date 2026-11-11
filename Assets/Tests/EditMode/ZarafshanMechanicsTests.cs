using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>The mechanics of Region 1 (Docs/Mapas100.md 01–02): the daily harvest of the Pomar, the mending fords of the Tugai, and the places the layout declares for them.</summary>
    public class ZarafshanMechanicsTests
    {
        private static OverworldLayout Region => OverworldLayouts.Get(OverworldLayouts.ZarafshanId);

        private static readonly ZarafshanHarvestSpot[] TestSpots =
        {
            new ZarafshanHarvestSpot("a", Vector2.zero), new ZarafshanHarvestSpot("b", new Vector2(6f, 0f)),
        };

        // ------------------------------------------------------------------
        // Harvest
        // ------------------------------------------------------------------

        [Test]
        public void Harvest_Reach_FindsTheClosestInRange()
        {
            Assert.AreEqual(1, ZarafshanHarvestRules.SpotAt(new Vector2(4f, 0f), TestSpots));
            Assert.AreEqual(0, ZarafshanHarvestRules.SpotAt(new Vector2(0f, ZarafshanHarvestRules.ReachRadius), TestSpots));
            Assert.AreEqual(-1, ZarafshanHarvestRules.SpotAt(new Vector2(0f, 20f), TestSpots));
            Assert.AreEqual(-1, ZarafshanHarvestRules.SpotAt(Vector2.zero, null));
        }

        [Test]
        public void Harvest_Ripe_OncePerGameDay_OldSaveIsRipe()
        {
            Assert.IsTrue(ZarafshanHarvestRules.IsRipe(0, 1), "a save with no mark");
            Assert.IsFalse(ZarafshanHarvestRules.IsRipe(3, 3));
            Assert.IsTrue(ZarafshanHarvestRules.IsRipe(3, 4));
        }

        [Test]
        public void Harvest_Mark_SurvivesTheSave_InsideOneDay()
        {
            string mark = ZarafshanHarvestRules.MarkKey("a");
            var progress = new WorldMapProgress();
            progress.SetDailyMark(mark, 5);

            var reloaded = new WorldMapProgress();
            reloaded.RestoreState(progress.CaptureState());

            Assert.IsFalse(ZarafshanHarvestRules.IsRipe(reloaded.GetDailyMark(mark), 5));
            Assert.IsTrue(ZarafshanHarvestRules.IsRipe(reloaded.GetDailyMark(mark), 6));
        }

        [Test]
        public void Harvest_Items_AreRealIngredients_NeverTheAttributeFruits()
        {
            foreach (string id in ZarafshanHarvestRules.Items)
            {
                Assert.IsNotNull(Resources.Load<ScriptableObject>("Items/" + id), id);
                Assert.AreNotEqual("item_fruta_mandacaru", id);
                Assert.AreNotEqual("item_fruta_umbu", id);
            }

            Assert.AreNotEqual(ZarafshanHarvestRules.ItemFor(0, 1), ZarafshanHarvestRules.ItemFor(0, 2), "the tree changes what it gives");
        }

        [Test]
        public void Harvest_Lines_HeroOrGrandma_AllTranslated()
        {
            Assert.IsTrue(ZarafshanHarvestRules.GrandmaSpeaks(0, 2));
            Assert.IsFalse(ZarafshanHarvestRules.GrandmaSpeaks(1, 2));
            Assert.AreEqual(ZarafshanMechanicsTexts.LineLavine, ZarafshanMechanicsTexts.HarvestLine(CharacterId.Lavine, false, 1));
            Assert.AreEqual(ZarafshanMechanicsTexts.LineGrandmaA, ZarafshanMechanicsTexts.HarvestLine(CharacterId.Lavine, true, 2));
            Assert.AreEqual(ZarafshanMechanicsTexts.LineGrandmaB, ZarafshanMechanicsTexts.HarvestLine(null, false, 1), "no hero known: the Vó speaks");

            var keys = new HashSet<string>();
            foreach (var entry in ZarafshanMechanicsTexts.Entries)
            {
                Assert.IsTrue(keys.Add(entry.Key), entry.Key);
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), entry.Key);
            }
        }

        // ------------------------------------------------------------------
        // Fords
        // ------------------------------------------------------------------

        [Test]
        public void Ford_Reach_FindsTheFord()
        {
            var fords = new[] { Vector2.zero, new Vector2(50f, 0f) };
            Assert.AreEqual(0, ZarafshanFordRules.FordAt(new Vector2(ZarafshanFordRules.Radius, 0f), fords));
            Assert.AreEqual(-1, ZarafshanFordRules.FordAt(new Vector2(20f, 0f), fords));
            Assert.AreEqual(-1, ZarafshanFordRules.FordAt(Vector2.zero, null));
        }

        [Test]
        public void Ford_Heals_UpToTheCeiling_NeverLowers()
        {
            Assert.AreEqual(3f, ZarafshanFordRules.HealOver(10f, 100f, 1f), 1e-4f);
            Assert.AreEqual(1f, ZarafshanFordRules.HealOver(69f, 100f, 1f), 1e-4f, "only up to the ceiling");
            Assert.AreEqual(0f, ZarafshanFordRules.HealOver(70f, 100f, 1f), 1e-4f);
            Assert.AreEqual(0f, ZarafshanFordRules.HealOver(95f, 100f, 1f), 1e-4f, "above the ceiling: nothing, never a loss");
            Assert.AreEqual(0f, ZarafshanFordRules.HealOver(0f, 0f, 1f), 1e-4f);
            Assert.AreEqual(0f, ZarafshanFordRules.HealOver(10f, 100f, 0f), 1e-4f);
            Assert.IsTrue(ZarafshanFordRules.BelowCeiling(69f, 100f));
            Assert.IsFalse(ZarafshanFordRules.BelowCeiling(70f, 100f));

            float health = 10f;
            for (int i = 0; i < 1000; i++)
            {
                health += ZarafshanFordRules.HealOver(health, 100f, 1f);
            }

            Assert.AreEqual(70f, health, 1e-3f);
        }

        // ------------------------------------------------------------------
        // Layout
        // ------------------------------------------------------------------

        [Test]
        public void Layout_Trees_InThePomar_OnALane_OutOfTheVila()
        {
            OverworldLayout w = Region;
            var ids = new HashSet<string>();
            foreach (ZarafshanHarvestSpot spot in OverworldLayouts.ZarafshanHarvestSpots)
            {
                Assert.IsTrue(ids.Add(spot.Id), spot.Id);
                Assert.IsTrue(OverworldLayouts.ZarafshanBounds.Contains(spot.Position), spot.Id);
                Assert.AreEqual(OverworldLayouts.ZarafshanAnchorZoneId, OverworldRules.ZoneAt(w, spot.Position)?.Id, spot.Id);
                Assert.IsNull(OverworldRules.SettlementAt(w, spot.Position), spot.Id + " would answer [E] inside a town");

                foreach (MapRiver river in w.World.Rivers)
                {
                    Assert.Greater(MapLayoutRules.DistanceToPolyline(spot.Position, river.Points), river.Width * 0.5f + 1f, spot.Id + " stands in water");
                }

                float nearest = float.MaxValue;
                foreach (MapPath path in w.World.Paths)
                {
                    nearest = Mathf.Min(nearest, MapLayoutRules.DistanceToSegment(spot.Position, path.A, path.B));
                }

                Assert.Less(nearest, ZarafshanHarvestRules.ReachRadius, spot.Id + " cannot be reached from a lane");
            }

            Assert.GreaterOrEqual(ids.Count, 3);
        }

        [Test]
        public void Layout_Fords_AreRealWadingCrossings_InTheTugai()
        {
            OverworldLayout w = Region;
            var wading = new List<Vector2>();
            foreach (MapRiver river in w.World.Rivers)
            {
                foreach (MapCrossing crossing in river.Crossings)
                {
                    if (!crossing.IsBridge)
                    {
                        wading.Add(crossing.At);
                    }
                }
            }

            foreach (Vector2 ford in OverworldLayouts.ZarafshanFords)
            {
                Assert.IsTrue(wading.Exists(c => Vector2.Distance(c, ford) < 2f), "no ford at " + ford);
                Assert.AreEqual("tugai_margem", OverworldRules.ZoneAt(w, ford)?.Id, ford.ToString());
                Assert.IsNull(OverworldRules.SettlementAt(w, ford), ford.ToString());
            }
        }

        [Test]
        public void Layout_Anchor_IsAtTheRegionOrigin_InTheStitchedWorld()
        {
            Assert.IsNotNull(Region.Zone(OverworldLayouts.ZarafshanAnchorZoneId));
            OverworldZone stitched = OverworldLayouts.Get(OverworldLayouts.WorldId).Zone(OverworldLayouts.ZarafshanAnchorZoneId);
            Assert.IsNotNull(stitched);
            Assert.AreEqual(OverworldLayouts.ZarafshanWorldOffset, stitched.Origin);
        }
    }
}
