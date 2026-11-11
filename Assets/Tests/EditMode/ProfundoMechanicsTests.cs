using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The Limiar Profundo mechanics (Docs/Mapas100.md 83–85): the lamp network and the Tide of Darkness (<see cref="ProfundoLampRules"/>), the floor
    /// that lights (<see cref="ProfundoFloorRules"/>), the wind that clears the smoke (<see cref="ProfundoSmokeRules"/>), the places the layout
    /// declares for them, and the driver's one path of damage on a real PlayerStats.
    /// </summary>
    public class ProfundoMechanicsTests
    {
        private const float HeroRunSpeed = 5f;

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.ProfundoRegionId);

        private static ProfundoLampNetwork AllLit()
        {
            var network = new ProfundoLampNetwork(ProfundoLampRules.LampCount);
            for (int i = 0; i < ProfundoLampRules.LampCount; i++)
            {
                network.Tend(i, ProfundoLampRules.KindleSeconds);
            }

            return network;
        }

        // ------------------------------------------------------------------
        // The hazard's floor, on a real PlayerStats
        // ------------------------------------------------------------------

        [TestCase(1000f, 1000f, 40f)]
        [TestCase(260f, 1000f, 10f)]
        [TestCase(250f, 1000f, 0f)]
        [TestCase(100f, 1000f, 0f)]
        public void Bite_StopsAtTheFloor(float current, float max, float expected)
        {
            Assert.AreEqual(expected, ProfundoHazardRules.Bite(current, max, 0.04f, 0.25f), 1e-3f);
        }

        [Test]
        public void Driver_ApplyHazard_HurtsTheRealPlayerStats_AndStopsAtTheFloor()
        {
            var go = new GameObject("hero");
            try
            {
                PlayerStats stats = go.AddComponent<PlayerStats>();
                float max = stats.MaxHealth;
                float before = stats.CurrentHealth;
                float bite = ProfundoMechanicsDriver.ApplyHazard(stats, ProfundoFloorRules.BiteFraction, ProfundoFloorRules.BiteFloorFraction);
                Assert.Greater(bite, 0f);
                Assert.Less(stats.CurrentHealth, before, "the damage must really land on PlayerStats");

                for (int i = 0; i < 200; i++)
                {
                    stats.ClearInvulnerability();
                    ProfundoMechanicsDriver.ApplyHazard(stats, ProfundoFloorRules.BiteFraction, ProfundoFloorRules.BiteFloorFraction);
                }

                Assert.IsTrue(stats.IsAlive, "the pits wear the hero down but never finish them");
                Assert.AreEqual(max * ProfundoFloorRules.BiteFloorFraction, stats.CurrentHealth, max * 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Driver_ApplyHazard_IgnoresNoHero()
        {
            Assert.AreEqual(0f, ProfundoMechanicsDriver.ApplyHazard(null, 0.5f, 0.1f));
        }

        // ------------------------------------------------------------------
        // The lamp network
        // ------------------------------------------------------------------

        [Test]
        public void Network_StartsWithTheInnerRing()
        {
            var network = new ProfundoLampNetwork(ProfundoLampRules.LampCount);
            Assert.AreEqual(ProfundoLampRules.LampsPerRing, network.LitCount);
            Assert.AreEqual(1, network.RingsHeld);
        }

        [Test]
        public void Network_TendingTakesItsTime_AndLeavingDropsIt()
        {
            var network = new ProfundoLampNetwork(ProfundoLampRules.LampCount);
            int lamp = ProfundoLampRules.LampsPerRing;
            Assert.IsFalse(network.Tend(lamp, ProfundoLampRules.KindleSeconds - 0.1f));
            network.Tend(-1, 0.1f);
            Assert.IsFalse(network.Tend(lamp, ProfundoLampRules.KindleSeconds - 0.1f), "walking away starts it over");
            Assert.IsTrue(network.Tend(lamp, 0.2f));
            Assert.IsTrue(network.IsLit(lamp));
            Assert.IsFalse(network.Tend(lamp, 10f), "a lit lamp needs no tending");
        }

        [Test]
        public void Network_ARingNeedsThreeOfFour_AndTheRingsInsideIt()
        {
            var network = new ProfundoLampNetwork(ProfundoLampRules.LampCount);
            for (int i = 4; i < 7; i++)
            {
                network.Tend(i, ProfundoLampRules.KindleSeconds);
            }

            Assert.AreEqual(2, network.RingsHeld, "3 of 4 holds");

            var gap = new ProfundoLampNetwork(ProfundoLampRules.LampCount);
            for (int i = 8; i < 12; i++)
            {
                gap.Tend(i, ProfundoLampRules.KindleSeconds);
            }

            Assert.AreEqual(1, gap.RingsHeld, "an outer ring is nothing with a gap inside it");
        }

        [Test]
        public void Network_SafeRadiusGrowsWithTheRings_PastTheirLamps()
        {
            for (int ring = 0; ring < ProfundoLampRules.RingCount; ring++)
            {
                float radius = ProfundoLampRules.SafeRadius(ring + 1);
                Assert.Greater(radius, ProfundoLampRules.SafeRadius(ring));
                for (int i = 0; i < ProfundoLampRules.LampsPerRing; i++)
                {
                    Assert.Greater(radius, OverworldLayouts.WallLamps[ring * ProfundoLampRules.LampsPerRing + i].magnitude, "the ring's lamps stand inside what it protects");
                }
            }
        }

        [Test]
        public void Tide_SnuffsTheOutermostFirst_AndShrinksTheRings()
        {
            ProfundoLampNetwork network = AllLit();
            Assert.AreEqual(3, network.RingsHeld);
            Assert.IsTrue(network.SnuffOutermost());
            Assert.IsFalse(network.IsLit(ProfundoLampRules.LampCount - 1));
            Assert.AreEqual(3, network.RingsHeld, "3 of 4 still holds");
            network.SnuffOutermost();
            Assert.AreEqual(2, network.RingsHeld);
        }

        [Test]
        public void Tide_SnuffingAnEmptyNetwork_IsHarmless()
        {
            var network = new ProfundoLampNetwork(ProfundoLampRules.LampCount);
            for (int i = 0; i < 20; i++)
            {
                network.SnuffOutermost();
            }

            Assert.IsFalse(network.SnuffOutermost());
            Assert.AreEqual(0, network.RingsHeld);
            Assert.AreEqual(ProfundoLampRules.NoRingsRadius, ProfundoLampRules.SafeRadius(network.RingsHeld), 1e-4f);
        }

        [TestCase(0f, ProfundoTidePhase.Calm)]
        [TestCase(179.9f, ProfundoTidePhase.Calm)]
        [TestCase(180f, ProfundoTidePhase.Warning)]
        [TestCase(199.9f, ProfundoTidePhase.Warning)]
        [TestCase(200f, ProfundoTidePhase.Rising)]
        [TestCase(239.9f, ProfundoTidePhase.Rising)]
        [TestCase(240f, ProfundoTidePhase.Calm)]
        public void Tide_FollowsItsClock(float time, ProfundoTidePhase expected)
        {
            Assert.AreEqual(expected, ProfundoLampRules.PhaseAt(time));
        }

        [Test]
        public void Tide_AlwaysWarnsFirst_LongEnoughToWalkToALamp()
        {
            Assert.Greater(ProfundoLampRules.WarningSeconds, 0f);
            float spacing = ProfundoLampRules.SafeRadius(1) - ProfundoLampRules.NoRingsRadius;
            Assert.GreaterOrEqual(ProfundoLampRules.WarningSeconds * HeroRunSpeed, spacing, "the warning covers the widest step between safe rings");
        }

        [Test]
        public void Tide_IsStrongerAtNight()
        {
            Assert.AreEqual(5, ProfundoLampRules.SnuffsDue(ProfundoLampRules.RisingSeconds, false));
            Assert.AreEqual(8, ProfundoLampRules.SnuffsDue(ProfundoLampRules.RisingSeconds, true));
            Assert.Greater(ProfundoLampRules.BiteFractionAt(true), ProfundoLampRules.BiteFractionAt(false));
        }

        [Test]
        public void Tide_ThePlazaIsAlwaysSafe_LampsShelter_OutOfReachIsFree()
        {
            var network = new ProfundoLampNetwork(ProfundoLampRules.LampCount);
            for (int i = 0; i < 20; i++)
            {
                network.SnuffOutermost();
            }

            Vector2[] lamps = OverworldLayouts.WallLamps;
            Vector2 plaza = OverworldLayouts.FortalezaUltimaLampadaOffset;
            Assert.IsFalse(ProfundoLampRules.Exposed(lamps, network, plaza, new Vector2(30f, 0f)), "the city never bites");
            Assert.IsTrue(ProfundoLampRules.Exposed(lamps, network, plaza, new Vector2(200f, 0f)));
            Assert.IsFalse(ProfundoLampRules.Exposed(lamps, network, plaza, new Vector2(330f, 0f)), "past the reach");

            network.Tend(8, ProfundoLampRules.KindleSeconds);
            Assert.IsFalse(ProfundoLampRules.Exposed(lamps, network, plaza, lamps[8] + new Vector2(ProfundoLampRules.LampShelter - 1f, 0f)), "a lit lamp shelters");
        }

        [TestCase(1000f, 1000f, false, 20f)]
        [TestCase(1000f, 1000f, true, 30f)]
        [TestCase(310f, 1000f, false, 10f)]
        [TestCase(300f, 1000f, false, 0f)]
        public void Tide_BiteStopsAtTheFloor(float current, float max, bool night, float expected)
        {
            Assert.AreEqual(expected, ProfundoLampRules.TideBite(current, max, night), 1e-3f);
        }

        [Test]
        public void Tide_FrontClosesFromTheReachToTheSafeRadius()
        {
            Assert.AreEqual(ProfundoLampRules.TideReach, ProfundoLampRules.FrontRadius(0f, 80f), 1e-3f);
            Assert.AreEqual(80f, ProfundoLampRules.FrontRadius(ProfundoLampRules.WarningSeconds, 80f), 1e-3f);
        }

        [Test]
        public void Tide_HeldMeansARingStillStands()
        {
            Assert.IsTrue(ProfundoLampRules.Held(1));
            Assert.IsFalse(ProfundoLampRules.Held(0));
        }

        [Test]
        public void Tide_ASteadyGarrison_KeepsARing_ThroughADayTide()
        {
            ProfundoLampNetwork network = AllLit();
            for (int i = 0; i < ProfundoLampRules.SnuffsDue(ProfundoLampRules.RisingSeconds, false); i++)
            {
                network.SnuffOutermost();
            }

            Assert.IsTrue(ProfundoLampRules.Held(network.RingsHeld));
        }

        [Test]
        public void Lamp_IsFoundWithinReachOnly()
        {
            var lamps = new[] { new Vector2(10f, 0f) };
            Assert.AreEqual(0, ProfundoLampRules.LampAt(lamps, new Vector2(10f + ProfundoLampRules.KindleReach, 0f)));
            Assert.AreEqual(-1, ProfundoLampRules.LampAt(lamps, new Vector2(10.1f + ProfundoLampRules.KindleReach, 0f)));
        }

        // ------------------------------------------------------------------
        // The floor that lights
        // ------------------------------------------------------------------

        [TestCase(0f, ProfundoFloorPhase.Quiet)]
        [TestCase(4.9f, ProfundoFloorPhase.Quiet)]
        [TestCase(5f, ProfundoFloorPhase.Warning)]
        [TestCase(6.4f, ProfundoFloorPhase.Warning)]
        [TestCase(6.5f, ProfundoFloorPhase.Burning)]
        [TestCase(8.9f, ProfundoFloorPhase.Burning)]
        [TestCase(9f, ProfundoFloorPhase.Quiet)]
        public void Floor_FollowsItsClock(float time, ProfundoFloorPhase expected)
        {
            Assert.AreEqual(expected, ProfundoFloorRules.PhaseAt(time));
        }

        [Test]
        public void Floor_WarnsTheSheetsOneAndAHalfSeconds()
        {
            Assert.AreEqual(1.5f, ProfundoFloorRules.WarningSeconds, 1e-4f);
        }

        [Test]
        public void Floor_EveryLitTile_HasFourDarkNeighbours_InEveryPattern()
        {
            for (int pattern = 0; pattern < ProfundoFloorRules.PatternCount; pattern++)
            {
                for (int x = -40; x <= 40; x++)
                {
                    for (int y = -40; y <= 40; y++)
                    {
                        var cell = new Vector2Int(x, y);
                        if (!ProfundoFloorRules.IsLit(pattern, cell))
                        {
                            continue;
                        }

                        Assert.IsFalse(ProfundoFloorRules.IsLit(pattern, cell + Vector2Int.right), $"{pattern} {cell}");
                        Assert.IsFalse(ProfundoFloorRules.IsLit(pattern, cell + Vector2Int.left), $"{pattern} {cell}");
                        Assert.IsFalse(ProfundoFloorRules.IsLit(pattern, cell + Vector2Int.up), $"{pattern} {cell}");
                        Assert.IsFalse(ProfundoFloorRules.IsLit(pattern, cell + Vector2Int.down), $"{pattern} {cell}");
                    }
                }
            }
        }

        [Test]
        public void Floor_TheWarningIsEnoughToStepOffATile()
        {
            Assert.GreaterOrEqual(ProfundoFloorRules.WarningSeconds * HeroRunSpeed, ProfundoFloorRules.TileSize * 0.5f);
        }

        [Test]
        public void Floor_PatternsTakeTurns()
        {
            Assert.AreEqual(0, ProfundoFloorRules.PatternAt(0f));
            Assert.AreEqual(1, ProfundoFloorRules.PatternAt(ProfundoFloorRules.CycleSeconds));
            Assert.AreEqual(0, ProfundoFloorRules.PatternAt(ProfundoFloorRules.CycleSeconds * ProfundoFloorRules.PatternCount));
        }

        [Test]
        public void Floor_CellsWorkOnNegativeCoordinates()
        {
            Assert.AreEqual(new Vector2Int(-1, -1), ProfundoFloorRules.Cell(new Vector2(-0.1f, -7.9f)));
            Assert.AreEqual(new Vector2(-4f, -4f), ProfundoFloorRules.CellCenter(new Vector2Int(-1, -1)));
        }

        // ------------------------------------------------------------------
        // The wind that clears the smoke
        // ------------------------------------------------------------------

        private static readonly ProfundoSmokeBank[] OneBank = { new ProfundoSmokeBank(Vector2.zero, 20f, new Vector2(30f, 0f)) };

        [Test]
        public void Smoke_FillsItsBank_UntilItIsCleared()
        {
            Assert.AreEqual(0, ProfundoSmokeRules.SmokeAt(OneBank, new[] { 0f }, new Vector2(19f, 0f)));
            Assert.AreEqual(-1, ProfundoSmokeRules.SmokeAt(OneBank, new[] { 0f }, new Vector2(21f, 0f)));
            Assert.AreEqual(-1, ProfundoSmokeRules.SmokeAt(OneBank, new[] { 5f }, new Vector2(19f, 0f)), "clear air");
        }

        [Test]
        public void Smoke_StaysClearThirtySeconds_ThenComesBack()
        {
            Assert.AreEqual(30f, ProfundoSmokeRules.ClearSeconds, 1e-4f);
            float clear = ProfundoSmokeRules.Advance(ProfundoSmokeRules.ClearSeconds, 29f);
            Assert.IsTrue(ProfundoSmokeRules.IsClear(clear));
            Assert.IsFalse(ProfundoSmokeRules.IsClear(ProfundoSmokeRules.Advance(clear, 2f)));
        }

        [Test]
        public void Smoke_OnlySlows_NeverStops()
        {
            Assert.Greater(ProfundoSmokeRules.SpeedFactor, 0f);
            Assert.AreEqual(-0.2f, ProfundoSmokeRules.MoveBonus, 1e-4f);
        }

        [Test]
        public void Smoke_FanIsFoundWithinReach_WindByItsArea()
        {
            Assert.AreEqual(0, ProfundoSmokeRules.VentAt(OneBank, new Vector2(30f + ProfundoSmokeRules.VentReach, 0f)));
            Assert.AreEqual(-1, ProfundoSmokeRules.VentAt(OneBank, new Vector2(30.1f + ProfundoSmokeRules.VentReach, 0f)));
            Assert.IsTrue(ProfundoSmokeRules.WindReaches(OneBank[0], new Vector2(20f + ProfundoSmokeRules.WindMargin + 3f, 0f), 3f));
            Assert.IsFalse(ProfundoSmokeRules.WindReaches(OneBank[0], new Vector2(20.1f + ProfundoSmokeRules.WindMargin, 0f), 0f));
        }

        [TestCase(1000f, 1000f, 20f)]
        [TestCase(355f, 1000f, 5f)]
        [TestCase(350f, 1000f, 0f)]
        public void Smoke_BiteStopsAtTheFloor(float current, float max, float expected)
        {
            Assert.AreEqual(expected, ProfundoSmokeRules.Bite(current, max), 1e-3f);
        }

        // ------------------------------------------------------------------
        // The layout
        // ------------------------------------------------------------------

        [Test]
        public void Layout_AnchorExists_AndTheBoundsAreTheRegions()
        {
            Assert.IsNotNull(World.Zone(OverworldLayouts.ProfundoAnchorZoneId));
            Assert.AreEqual(World.Bounds, OverworldLayouts.ProfundoBounds);
        }

        [Test]
        public void Layout_InTheStitchedWorld_TheAnchorSaysWhereTheRegionSits()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.WorldId);
            OverworldZone anchor = world.Zone(OverworldLayouts.ProfundoAnchorZoneId);
            Assert.IsNotNull(anchor, "the driver finds its anchor in the world");
            Assert.AreEqual(OverworldLayouts.ProfundoWorldOffset, anchor.Origin);
            foreach (Vector2 lamp in OverworldLayouts.WallLamps)
            {
                Assert.AreEqual(OverworldLayouts.FortalezaUltimaLampadaMapId, OverworldRules.ZoneAt(world, anchor.Origin + lamp).MapIdOrOwner, lamp.ToString());
            }

            Assert.AreEqual(OverworldLayouts.ProfundoRegionId, world.PartAt(anchor.Origin + OverworldLayouts.WallLamps[0]).Id);
        }

        [Test]
        public void Layout_OtherRegions_HaveNoAnchor_SoTheDriverStaysOff()
        {
            Assert.IsNull(OverworldLayouts.Get(OverworldLayouts.MisturaRegionId).Zone(OverworldLayouts.ProfundoAnchorZoneId));
        }

        [Test]
        public void Layout_Lamps_AreTwelve_InMap83_OffRoadsAndTown()
        {
            Assert.AreEqual(ProfundoLampRules.LampCount, OverworldLayouts.WallLamps.Length);
            Assert.AreEqual(OverworldLayouts.WallLamps.Length, OverworldLayouts.WallLamps.Distinct().Count());
            foreach (Vector2 lamp in OverworldLayouts.WallLamps)
            {
                Assert.AreEqual(OverworldLayouts.FortalezaUltimaLampadaMapId, OverworldRules.ZoneAt(World, lamp).MapIdOrOwner, lamp.ToString());
                Assert.IsFalse(MapLayoutRules.OnPath(World.World, lamp, 1.5f), lamp + " on a road");
                Assert.IsNull(OverworldRules.SettlementAt(World, lamp), lamp.ToString());
            }
        }

        [Test]
        public void Layout_TheTideStaysInMap83()
        {
            foreach (float angle in Enumerable.Range(0, 36).Select(i => i * 10f * Mathf.Deg2Rad))
            {
                Vector2 rim = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * ProfundoLampRules.TideReach;
                Assert.AreEqual(OverworldLayouts.FortalezaUltimaLampadaMapId, OverworldRules.ZoneAt(World, rim).MapIdOrOwner, rim.ToString());
            }
        }

        [Test]
        public void Layout_MostLampsAndFans_StandAsPieces()
        {
            int lamps = OverworldLayouts.WallLamps.Count(p => HasPiece(p, OverworldDecor.LampPedestal));
            Assert.GreaterOrEqual(lamps, OverworldLayouts.WallLamps.Length / 2, "the lamps must be there to see (one can give way to a town wall)");
            int fans = OverworldLayouts.SmokeBanks.Count(b => HasPiece(b.Vent, OverworldDecor.ArtificerVent));
            Assert.GreaterOrEqual(fans, OverworldLayouts.SmokeBanks.Length / 2, "the fans must be there to see");
        }

        [Test]
        public void Layout_FireFloor_IsTheFloorThatLights()
        {
            Vector2 middle = OverworldLayouts.FireFloorArea.center;
            Assert.AreEqual(OverworldLayouts.CovaFogoMapId, OverworldRules.ZoneAt(World, middle).MapIdOrOwner);
            Assert.AreEqual(OverworldTerrain.GlowPatternFloor, OverworldRules.ZoneAt(World, middle).Terrain);
            foreach (OverworldSettlement town in World.Settlements)
            {
                Assert.IsFalse(OverworldLayouts.FireFloorArea.Overlaps(town.GroundRect), town.Id);
            }
        }

        [Test]
        public void Layout_Smoke_LiesInMap84_OnTheRoads_OffThePost_WithItsFanOutside()
        {
            Assert.GreaterOrEqual(OverworldLayouts.SmokeBanks.Length, 6);
            foreach (ProfundoSmokeBank bank in OverworldLayouts.SmokeBanks)
            {
                Assert.AreEqual(OverworldLayouts.CovaFumacaMapId, OverworldRules.ZoneAt(World, bank.Center).MapIdOrOwner, bank.Center.ToString());
                Assert.IsTrue(MapLayoutRules.OnPath(World.World, bank.Center, 12f), bank.Center + " off the roads");
                Assert.Greater(Vector2.Distance(bank.Vent, bank.Center), bank.Radius, "the fan stands outside the smoke");
                Assert.IsFalse(MapLayoutRules.OnPath(World.World, bank.Vent, 1.5f), bank.Vent + " on a road");
                foreach (OverworldSettlement town in World.Settlements)
                {
                    Assert.IsFalse(new Rect(bank.Center.x - bank.Radius, bank.Center.y - bank.Radius, bank.Radius * 2f, bank.Radius * 2f).Overlaps(town.GroundRect), bank.Center + " over " + town.Id);
                }
            }
        }

        [Test]
        public void Texts_HaveBothLanguages()
        {
            Assert.GreaterOrEqual(ProfundoMechanicsTexts.Entries.Count, 15);
            foreach (var entry in ProfundoMechanicsTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), entry.Key);
            }
        }

        private static bool HasPiece(Vector2 position, OverworldDecor decor)
        {
            Vector2Int chunk = OverworldRules.ChunkOf(World, position);
            return OverworldChunkBuilder.Content(World, chunk).Props.Any(p => p.Decor == decor && Vector2.Distance(p.Position, position) < 0.1f);
        }
    }
}
