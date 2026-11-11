using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The Rio das Doze Horas mechanics (Docs/Mapas100.md 71–75): the barque's hours (<see cref="RioHourRules"/>), the fire that
    /// chooses (<see cref="RioFireRules"/>), the turning ring of Mehen (<see cref="RioMehenRules"/>) and the places the layout declares for them.
    /// </summary>
    public class RioMechanicsTests
    {
        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.RioRegionId);

        // ------------------------------------------------------------------
        // The hours
        // ------------------------------------------------------------------

        [TestCase(0L, 0)]
        [TestCase(179L, 0)]
        [TestCase(180L, 1)]
        [TestCase(2159L, 11)]
        [TestCase(2160L, 0)]
        [TestCase(-1L, 11)]
        public void Hours_TurnThroughTheTwelvePiers(long unix, int hour)
        {
            Assert.AreEqual(hour, RioHourRules.HourIndex(unix));
        }

        [Test]
        public void Hours_CountTheWaitToAPier()
        {
            Assert.AreEqual(0, RioHourRules.SecondsUntilOpen(0, 10L));
            Assert.AreEqual(170, RioHourRules.SecondsUntilOpen(1, 10L));
            Assert.AreEqual(170 + 180 * 10, RioHourRules.SecondsUntilOpen(11, 10L));
            Assert.AreEqual(180, RioHourRules.SecondsToNextHour(0L));
            Assert.AreEqual("3:05", RioHourRules.Clock(185));
        }

        [Test]
        public void Hours_BoardOnlyAtTheOpenPier_AndOnlyWhereTheHeroMayGo()
        {
            Assert.AreEqual(RioBarqueCheck.Board, RioHourRules.Check(1, 200L, true));
            Assert.AreEqual(RioBarqueCheck.ZoneBlocked, RioHourRules.Check(1, 200L, false));
            Assert.AreEqual(RioBarqueCheck.PierClosed, RioHourRules.Check(2, 200L, true));
        }

        [Test]
        public void Hours_EveryPost_IsServed()
        {
            var served = Enumerable.Range(0, RioHourRules.Piers).Select(RioHourRules.DestinationOf).Distinct().ToArray();
            Assert.AreEqual(RioHourRules.Destinations, served.Length);
            Assert.AreEqual(RioHourRules.Destinations, RioMechanicsTexts.Destinations.Length);
        }

        [Test]
        public void Hours_NearestWithinReach_PicksTheClosest()
        {
            var points = new[] { Vector2.zero, new Vector2(8f, 0f) };
            Assert.AreEqual(1, RioHourRules.NearestWithinReach(points, new Vector2(5f, 0f)));
            Assert.AreEqual(-1, RioHourRules.NearestWithinReach(points, new Vector2(-6.5f, 0f)));
        }

        // ------------------------------------------------------------------
        // The fire
        // ------------------------------------------------------------------

        private static readonly (Vector2 A, Vector2 B, float Width)[] Pool = { (Vector2.zero, new Vector2(10f, 0f), 20f) };

        [Test]
        public void Fire_ShoreIsTheBandRoundTheEdge()
        {
            Assert.AreEqual(-10f, RioFireRules.DistanceToFire(Pool, new Vector2(5f, 0f)), 1e-3f);
            Assert.IsTrue(RioFireRules.OnShore(Pool, new Vector2(5f, 10f + RioFireRules.ShoreBand)));
            Assert.IsFalse(RioFireRules.OnShore(Pool, new Vector2(5f, 10.1f + RioFireRules.ShoreBand)));
        }

        [Test]
        public void Fire_MendsOnlyAtPeace_AndNeverPastFull()
        {
            Assert.AreEqual(25f, RioFireRules.HealAmount(100f, 1000f, 6f, 1f), 1e-3f);
            Assert.AreEqual(0f, RioFireRules.HealAmount(100f, 1000f, 4.9f, 1f), 1e-3f);
            Assert.AreEqual(5f, RioFireRules.HealAmount(995f, 1000f, 6f, 1f), 1e-3f);
            Assert.AreEqual(0f, RioFireRules.HealAmount(1000f, 1000f, 6f, 1f), 1e-3f);
            Assert.AreEqual(0f, RioFireRules.HealAmount(0f, 1000f, 6f, 1f), 1e-3f, "the fire does not raise the dead");
        }

        [Test]
        public void Fire_BurnsBeasts_ButNeverKillsThem_NorTouchesBosses()
        {
            Assert.AreEqual(40f, RioFireRules.BurnAmount(1000f, 1000f, false), 1e-3f);
            Assert.AreEqual(0f, RioFireRules.BurnAmount(1000f, 1000f, true), 1e-3f);
            Assert.AreEqual(10f, RioFireRules.BurnAmount(210f, 1000f, false), 1e-3f);
            Assert.AreEqual(0f, RioFireRules.BurnAmount(200f, 1000f, false), 1e-3f);
        }

        // ------------------------------------------------------------------
        // The turning ring
        // ------------------------------------------------------------------

        [Test]
        public void Ring_TurnsThenHolds_OpeningAPairOfGatesEachStep()
        {
            Assert.IsFalse(RioMehenRules.IsHolding(17.9d));
            Assert.IsTrue(RioMehenRules.IsHolding(18d));
            Assert.IsTrue(RioMehenRules.IsGateOpen(0, 20d));
            Assert.IsTrue(RioMehenRules.IsGateOpen(4, 20d), "the opposite opening");
            Assert.IsFalse(RioMehenRules.IsGateOpen(1, 20d));
            Assert.IsTrue(RioMehenRules.IsGateOpen(1, 50d), "the next step moves on to the next pair");
            Assert.IsFalse(RioMehenRules.IsGateOpen(0, 10d), "shut while the ring turns");
        }

        [TestCase(0, 20d, 0f)]
        [TestCase(0, 10d, 8f)]
        [TestCase(1, 20d, 28f)]
        [TestCase(0, 30d, 108f)]
        public void Ring_CountsTheWait(int gate, double time, float seconds)
        {
            Assert.AreEqual(seconds, RioMehenRules.SecondsUntilOpen(gate, time), 1e-3f);
        }

        [Test]
        public void Ring_BearingArrivesAtTheGate_WhenItHolds()
        {
            Assert.AreEqual(0f, RioMehenRules.OpeningBearing(20d), 1e-3f);
            Assert.AreEqual(45f, RioMehenRules.OpeningBearing(50d), 1e-3f);
            Assert.AreEqual(22.5f, RioMehenRules.OpeningBearing(39d), 1e-3f);
            Assert.AreEqual(6f, RioMehenRules.CrossSeconds(96f), 1e-3f);
        }

        // ------------------------------------------------------------------
        // The layout
        // ------------------------------------------------------------------

        [Test]
        public void Layout_AnchorExists_AndTheBoundsAreTheRegions()
        {
            Assert.IsNotNull(World.Zone(OverworldLayouts.RioAnchorZoneId));
            Assert.AreEqual(World.Bounds, OverworldLayouts.RioBounds);
        }

        [Test]
        public void Layout_Piers_AreTwelve_Dry_Apart_AndBesideTheRiver()
        {
            Vector2[] piers = OverworldLayouts.HourPiers;
            Assert.AreEqual(RioHourRules.Piers, piers.Length);
            for (int i = 0; i < piers.Length; i++)
            {
                Assert.IsFalse(MapLayoutRules.InRiver(World.World, piers[i], 2f), "pier " + i + " in the water");
                Assert.IsTrue(MapLayoutRules.InRiver(World.World, piers[i], 12f), "pier " + i + " far from the river");
                Assert.IsNull(OverworldRules.SettlementAt(World, piers[i]), "pier " + i);
                Assert.IsNotNull(OverworldRules.ZoneAt(World, piers[i]), "pier " + i);
                if (i > 0)
                {
                    Assert.Greater(Vector2.Distance(piers[i - 1], piers[i]), 2f * RioHourRules.PierReach, "reach of piers " + i + " overlaps");
                }
            }
        }

        [Test]
        public void Layout_Berths_StandAtTheirPosts_Dry_InsideTheMaps()
        {
            string[] maps =
            {
                OverworldLayouts.CamposJuncosMapId, OverworldLayouts.ArealEscondidoMapId, OverworldLayouts.AneisMehenMapId,
                OverworldLayouts.LagoDeFogoMapId, OverworldLayouts.BancoApepMapId,
            };
            Assert.AreEqual(RioHourRules.Destinations, OverworldLayouts.BarqueBerths.Length);
            for (int i = 0; i < maps.Length; i++)
            {
                Vector2 berth = OverworldLayouts.BarqueBerths[i];
                Assert.AreEqual(maps[i], OverworldRules.ZoneAt(World, berth).MapIdOrOwner, "berth " + i);
                Assert.IsFalse(MapLayoutRules.InRiver(World.World, berth, 2f), "berth " + i + " in the water");
                Assert.IsNull(OverworldRules.SettlementAt(World, berth), "berth " + i);
            }

            Assert.IsFalse(MapLayoutRules.InRiver(World.World, OverworldLayouts.QuayBerth, 2f));
            Assert.AreEqual(OverworldLayouts.CaisMesektetMapId, OverworldRules.ZoneAt(World, OverworldLayouts.QuayBerth).MapIdOrOwner);
        }

        [Test]
        public void Layout_FireShore_ReachesTheIslePath_AndTheBandLiesInTheLakeMap()
        {
            // The road's last stretch to the Ilha das Palmeiras runs between the lobes: a hero on foot can stand on the shore.
            bool reachable = false;
            for (float x = 640f; x <= 800f; x += 4f)
            {
                reachable |= RioFireRules.OnShore(OverworldLayouts.LagoDeFogoPools, new Vector2(x, 1000f));
            }

            Assert.IsTrue(reachable);
            foreach ((Vector2 a, Vector2 b, float width) in OverworldLayouts.LagoDeFogoPools)
            {
                Assert.IsTrue(OverworldLayouts.RioBounds.Contains((a + b) * 0.5f), a.ToString());
                Assert.Greater(width, 0f);
            }
        }

        [Test]
        public void Layout_Gates_AreEight_CrossTheWall_InTheCoilsMap_AndOpenToWhoMayEnter()
        {
            Assert.AreEqual(RioMehenRules.Gates, OverworldLayouts.MehenGates.Length);
            Assert.AreEqual(RioMehenRules.Gates, OverworldLayouts.MehenGates.Select(g => g.Id).Distinct().Count());
            foreach (SkyRoute gate in OverworldLayouts.MehenGates)
            {
                Assert.AreEqual(OverworldLayouts.AneisMehenMapId, OverworldRules.ZoneAt(World, gate.A).MapIdOrOwner, gate.Id);
                Assert.AreEqual(OverworldLayouts.AneisMehenMapId, OverworldRules.ZoneAt(World, gate.B).MapIdOrOwner, gate.Id);
                Assert.AreEqual(OverworldRelief.MehenRingPeriod, Vector2.Distance(gate.A, gate.B), 1e-2f, gate.Id);
                Assert.IsFalse(MapLayoutRules.InRiver(World.World, gate.A, 2f), gate.Id);
                Assert.IsFalse(MapLayoutRules.InRiver(World.World, gate.B, 2f), gate.Id);
                Assert.IsNull(OverworldRules.SettlementAt(World, gate.A), gate.Id);
                Assert.IsTrue(SkyRouteRules.AllZonesOpen(World, Vector2.zero, gate, 320, CharacterId.Maithe, tag => true), gate.Id);
            }
        }

        [Test]
        public void Texts_HaveBothLanguages_AndPlaceholdersMatch()
        {
            Assert.GreaterOrEqual(RioMechanicsTexts.Entries.Count, 17);
            foreach (var entry in RioMechanicsTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), entry.Key);
                Assert.AreEqual(entry.En.Contains("{0}"), entry.PtBR.Contains("{0}"), entry.Key);
                Assert.AreEqual(entry.En.Contains("{1}"), entry.PtBR.Contains("{1}"), entry.Key);
            }
        }
    }
}
