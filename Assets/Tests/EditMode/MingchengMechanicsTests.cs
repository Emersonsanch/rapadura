using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The Planície de Mingcheng mechanics (Docs/Mapas100.md 44–45): the walk on the great wall (<see cref="MingchengWallRules"/>), its beacons
    /// (<see cref="MingchengBeaconRules"/>), the mirror paddies (<see cref="MingchengReflectionRules"/>) and the places the layout declares for them.
    /// </summary>
    public class MingchengMechanicsTests
    {
        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.MingchengId);

        // ------------------------------------------------------------------
        // The wall
        // ------------------------------------------------------------------

        [Test]
        public void Wall_StandsOnTheDeck_OnlyAtDeckHeightAndOverTheLine()
        {
            Vector2 p = MingchengWallRules.LineAt(100f);
            Assert.IsTrue(MingchengWallRules.OnDeck(p, 13.4f, 10f));
            Assert.IsFalse(MingchengWallRules.OnDeck(p, 10f, 10f), "on the ground under the deck");
            Assert.IsFalse(MingchengWallRules.OnDeck(p + Vector2.up * 5f, 13.4f, 10f), "off its side");
            Assert.IsFalse(MingchengWallRules.OnDeck(MingchengWallRules.LineAt(MingchengWallRules.MaxX + 20f), 13.4f, 10f), "past its end");
        }

        [Test]
        public void Wall_PieceIsKeptOnTheDeck()
        {
            Assert.AreEqual(MingchengWallRules.MinIndex, MingchengWallRules.PieceAt(-5000f));
            Assert.AreEqual(MingchengWallRules.MaxIndex, MingchengWallRules.PieceAt(5000f));
            Assert.AreEqual(2, MingchengWallRules.PieceAt(12.5f));
        }

        [TestCase(false, false, false, MingchengWallAction.None)]
        [TestCase(true, false, false, MingchengWallAction.Climb)]
        [TestCase(true, false, true, MingchengWallAction.Climb)]
        [TestCase(true, true, false, MingchengWallAction.LightBeacon)]
        [TestCase(true, true, true, MingchengWallAction.Descend)]
        public void Wall_TowerAction(bool atTower, bool onDeck, bool lit, MingchengWallAction expected)
        {
            Assert.AreEqual(expected, MingchengWallRules.ActionAt(atTower, onDeck, lit));
        }

        [Test]
        public void Wall_TowerAt_NearestWithinTheStair()
        {
            var towers = new[] { 0f, 120f };
            Assert.AreEqual(0, MingchengWallRules.TowerAt(towers, MingchengWallRules.LineAt(0f) + Vector2.down * MingchengWallRules.StairReach));
            Assert.AreEqual(1, MingchengWallRules.TowerAt(towers, MingchengWallRules.LineAt(120f)));
            Assert.AreEqual(-1, MingchengWallRules.TowerAt(towers, MingchengWallRules.LineAt(60f)));
        }

        [Test]
        public void Wall_TheWayDownLandsSouthOfTheWall_ClearOfItsBody()
        {
            foreach (float x in OverworldLayouts.GreatWallTowerXs)
            {
                Vector2 foot = MingchengWallRules.FootOf(x);
                Assert.Less(foot.y, MingchengWallRules.LineAt(x).y, x.ToString());
                Assert.AreEqual(MingchengWallRules.FootOffset, Vector2.Distance(foot, MingchengWallRules.LineAt(x)), 1e-3f);
                Assert.Greater(MingchengWallRules.FootOffset, 5.1f * 0.5f + 1f, "outside the tower's collider");
            }
        }

        [Test]
        public void Wall_FallingOff_IsAShortDrop_AndItStaysAboveAJump()
        {
            Assert.Less(MingchengWallRules.DeckHeight, 4f);
            Assert.Greater(MingchengWallRules.DeckHeight, 1.2f + 0.3f, "not reachable with a jump and a step");
        }

        // ------------------------------------------------------------------
        // The beacons
        // ------------------------------------------------------------------

        [Test]
        public void Beacon_LightsOnce_WithoutRefilling()
        {
            var beacons = new float[2];
            Assert.IsTrue(MingchengBeaconRules.Light(beacons, 0));
            Assert.AreEqual(MingchengBeaconRules.LitSeconds, beacons[0], 1e-4f);
            beacons[0] = 10f;
            Assert.IsFalse(MingchengBeaconRules.Light(beacons, 0));
            Assert.AreEqual(10f, beacons[0], 1e-4f);
        }

        [Test]
        public void Beacon_BurnsOutByDay_AndHoldsAtNight()
        {
            var beacons = new[] { 10f };
            MingchengBeaconRules.Advance(beacons, 100f, true);
            Assert.AreEqual(10f, beacons[0], 1e-4f);
            MingchengBeaconRules.Advance(beacons, 4f, false);
            Assert.AreEqual(6f, beacons[0], 1e-4f);
            MingchengBeaconRules.Advance(beacons, 1000f, false);
            Assert.AreEqual(0f, beacons[0], 1e-4f);
            Assert.AreEqual(0, MingchengBeaconRules.LitCount(beacons));
        }

        [Test]
        public void Beacon_Corridor_NeedsBothNeighbours()
        {
            var towers = new[] { 0f, 120f, 240f };
            var beacons = new[] { 5f, 5f, 0f };
            Assert.IsTrue(MingchengBeaconRules.InCorridor(towers, beacons, 60f));
            Assert.IsFalse(MingchengBeaconRules.InCorridor(towers, beacons, 180f));
            Assert.IsFalse(MingchengBeaconRules.InCorridor(towers, beacons, -10f));
        }

        [Test]
        public void Beacon_Bonus_OnFootOnTheDeckInTheCorridor()
        {
            Assert.AreEqual(1.1f, MingchengBeaconRules.SpeedFactor(true, true, false), 1e-4f);
            Assert.AreEqual(1f, MingchengBeaconRules.SpeedFactor(false, true, false), 1e-4f);
            Assert.AreEqual(1f, MingchengBeaconRules.SpeedFactor(true, false, false), 1e-4f);
            Assert.AreEqual(1f, MingchengBeaconRules.SpeedFactor(true, true, true), 1e-4f);
        }

        // ------------------------------------------------------------------
        // The reflection
        // ------------------------------------------------------------------

        [Test]
        public void Reflection_LastsTenSeconds_TwentyAtNight()
        {
            Assert.AreEqual(10f, MingchengReflectionRules.Look(default, false).Vision, 1e-4f);
            Assert.AreEqual(20f, MingchengReflectionRules.Look(default, true).Vision, 1e-4f);
        }

        [Test]
        public void Reflection_CannotBeReadAgain_UntilTheCooldownPassed()
        {
            MingchengReflectionState s = MingchengReflectionRules.Look(default, false);
            Assert.IsFalse(s.Ready);
            s = MingchengReflectionRules.Advance(s, 9f);
            Assert.IsTrue(s.Active);
            Assert.AreEqual(s.Vision, MingchengReflectionRules.Look(s, false).Vision, 1e-4f, "looking again changes nothing");
            s = MingchengReflectionRules.Advance(s, 2f);
            Assert.IsFalse(s.Active);
            Assert.IsFalse(s.Ready);
            s = MingchengReflectionRules.Advance(s, MingchengReflectionRules.CooldownSeconds);
            Assert.IsTrue(s.Ready);
        }

        [Test]
        public void Reflection_ALongFrame_EndsEverything()
        {
            Assert.IsTrue(MingchengReflectionRules.Advance(MingchengReflectionRules.Look(default, false), 1000f).Ready);
        }

        [Test]
        public void Reflection_Trail_SpeedsOnlyOnItAndOnlyDuringTheVision()
        {
            var trails = new[] { new MingchengReflectedTrail(0, Vector2.zero, new Vector2(20f, 0f)) };
            MingchengReflectionState active = MingchengReflectionRules.Look(default, false);
            Assert.AreEqual(1.25f, MingchengReflectionRules.SpeedFactor(trails, new Vector2(10f, 2f), active), 1e-4f);
            Assert.AreEqual(1f, MingchengReflectionRules.SpeedFactor(trails, new Vector2(10f, 4f), active), 1e-4f);
            Assert.AreEqual(1f, MingchengReflectionRules.SpeedFactor(trails, new Vector2(10f, 2f), default), 1e-4f);
        }

        [Test]
        public void Reflection_Chest_ExistsOnlyInTheVision_OncePerDay()
        {
            var trails = new[] { new MingchengReflectedTrail(0, Vector2.zero, new Vector2(20f, 0f)) };
            MingchengReflectionState active = MingchengReflectionRules.Look(default, false);
            Assert.AreEqual(0, MingchengReflectionRules.ChestAt(trails, new Vector2(18f, 0f), active));
            Assert.AreEqual(-1, MingchengReflectionRules.ChestAt(trails, new Vector2(18f, 0f), default));
            Assert.IsTrue(MingchengReflectionRules.CanOpen(0, 1));
            Assert.IsFalse(MingchengReflectionRules.CanOpen(3, 3));
            Assert.IsTrue(MingchengReflectionRules.CanOpen(3, 4));
        }

        [Test]
        public void Reflection_ChestItems_AreRealItemsInTurn()
        {
            foreach (string id in MingchengReflectionRules.ChestItems)
            {
                Assert.IsNotNull(Rapadura.Gameplay.Items.ItemDatabase.GetById(id), id);
            }

            Assert.AreNotEqual(MingchengReflectionRules.ChestItem(0, 1), MingchengReflectionRules.ChestItem(0, 2));
        }

        // ------------------------------------------------------------------
        // The layout
        // ------------------------------------------------------------------

        [Test]
        public void Layout_AnchorExists_AndTheBoundsAreTheRegions()
        {
            Assert.IsNotNull(World.Zone(OverworldLayouts.MingchengAnchorZoneId));
            Assert.AreEqual(World.Bounds, OverworldLayouts.MingchengBounds);
        }

        [Test]
        public void Layout_Deck_LiesOnTheWallsTerrain_FromEndToEnd()
        {
            Assert.AreEqual(OverworldTerrain.GreatWall, OverworldRules.ZoneAt(World, MingchengWallRules.LineAt(MingchengWallRules.MinX)).Terrain);
            Assert.AreEqual(OverworldTerrain.GreatWall, OverworldRules.ZoneAt(World, MingchengWallRules.LineAt(MingchengWallRules.MaxX)).Terrain);
        }

        [Test]
        public void Layout_Deck_IsNeverSteeperThanTheHeroCanWalk()
        {
            for (int i = MingchengWallRules.MinIndex; i <= MingchengWallRules.MaxIndex; i++)
            {
                float a = OverworldRules.SurfaceAt(World, MingchengWallRules.PointOf(i));
                float b = OverworldRules.SurfaceAt(World, MingchengWallRules.PointOf(i + 1));
                Assert.Less(Mathf.Abs(a - b) / MingchengWallRules.Step, 0.7f, "piece " + i);
            }
        }

        [Test]
        public void Layout_TwelveTowers_Ascending_EachStandingOnTheWall()
        {
            float[] towers = OverworldLayouts.GreatWallTowerXs;
            Assert.AreEqual(12, towers.Length, "the sheet's 12 towers");
            for (int i = 0; i < towers.Length; i++)
            {
                Assert.IsTrue(i == 0 || towers[i] > towers[i - 1]);
                Assert.GreaterOrEqual(towers[i], MingchengWallRules.MinX);
                Assert.LessOrEqual(towers[i], MingchengWallRules.MaxX);
                Vector2 at = MingchengWallRules.LineAt(towers[i]);
                OverworldChunkContent content = OverworldChunkBuilder.Content(World, OverworldRules.ChunkOf(World, at));
                Assert.IsTrue(content.Props.Any(p => p.Decor == OverworldDecor.BeaconTower && Vector2.Distance(p.Position, at) < 0.1f), "no tower at x " + towers[i]);
            }
        }

        [Test]
        public void Layout_WallColliders_StayUnderTheDeck_AndTheTowersOpenTheWalk()
        {
            foreach (float x in OverworldLayouts.GreatWallTowerXs)
            {
                Vector2 at = MingchengWallRules.LineAt(x);
                OverworldChunkContent content = OverworldChunkBuilder.Content(World, OverworldRules.ChunkOf(World, at));
                foreach (OverworldSolid solid in content.Solids.Where(s => s.IsBox && Vector2.Distance(s.Center, at) < 12f))
                {
                    Assert.Less(solid.Base + solid.Height, content.Grid.Sample(solid.Center) + MingchengWallRules.DeckHeight, "x " + x);
                }
            }
        }

        [Test]
        public void Layout_TheWallsGaps_CarryTheDeck_WithoutColliders()
        {
            int spans = 0;
            OverworldLayout w = World;
            var chunks = new System.Collections.Generic.HashSet<Vector2Int>();
            for (float x = MingchengWallRules.MinX; x <= MingchengWallRules.MaxX; x += MingchengWallRules.Step)
            {
                chunks.Add(OverworldRules.ChunkOf(w, MingchengWallRules.LineAt(x)));
            }

            foreach (Vector2Int coord in chunks)
            {
                spans += OverworldChunkBuilder.Content(w, coord).Props.Count(p => p.Decor == OverworldDecor.GreatWallRun && p.Variant == int.MinValue);
            }

            Assert.Greater(spans, 0, "the gates and the stretches beside the towers");
        }

        [Test]
        public void Layout_Pools_LieInTheMirrorPaddies_Dry_OffTheTowns()
        {
            Assert.GreaterOrEqual(OverworldLayouts.MirrorPools.Length, 6);
            foreach (MingchengPool pool in OverworldLayouts.MirrorPools)
            {
                AssertInPaddies(pool.Center);
                Assert.IsFalse(MapLayoutRules.InRiver(World.World, pool.Center, pool.Radius), pool.Center + " in the water");
            }
        }

        [Test]
        public void Layout_EachPoolHasItsTrail_StartingAtIt_AndEveryPointLiesInThePaddies()
        {
            MingchengReflectedTrail[] trails = OverworldLayouts.ReflectedTrails;
            Assert.AreEqual(OverworldLayouts.MirrorPools.Length, trails.Length);
            for (int i = 0; i < trails.Length; i++)
            {
                Assert.AreEqual(i, trails[i].Pool);
                Assert.AreEqual(OverworldLayouts.MirrorPools[i].Center, trails[i].Points[0]);
                foreach (Vector2 point in trails[i].Points)
                {
                    AssertInPaddies(point);
                }

                Assert.Greater(Vector2.Distance(trails[i].Points[0], trails[i].Chest), 30f, "a chest worth the walk");
            }
        }

        [Test]
        public void Layout_AChestIsWithinTheVision_AtHeroSpeed()
        {
            const float heroRunSpeed = 5f * (1f + MingchengReflectionRules.TrailBonus);
            foreach (MingchengReflectedTrail trail in OverworldLayouts.ReflectedTrails)
            {
                float length = 0f;
                for (int i = 0; i + 1 < trail.Points.Length; i++)
                {
                    length += Vector2.Distance(trail.Points[i], trail.Points[i + 1]);
                }

                Assert.LessOrEqual(length / heroRunSpeed, MingchengReflectionRules.VisionSeconds, trail.Chest.ToString() + " inside one day-time vision");
            }
        }

        [Test]
        public void Texts_HaveBothLanguages()
        {
            Assert.GreaterOrEqual(MingchengMechanicsTexts.Entries.Count, 13);
            foreach (var entry in MingchengMechanicsTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), entry.Key);
            }
        }

        private static void AssertInPaddies(Vector2 p)
        {
            Assert.IsTrue(OverworldLayouts.MirrorPaddiesArea.Contains(p), p.ToString());
            Assert.AreEqual(OverworldLayouts.ArrozaisMapId, OverworldRules.ZoneAt(World, p).MapIdOrOwner, p.ToString());
            Assert.IsNull(OverworldRules.SettlementAt(World, p), p.ToString());
        }
    }
}
