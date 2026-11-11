using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The Arquipélago de Hinokura mechanics (Docs/Mapas100.md 65, 67 and 70): the sluices, the Tremor and the Glutão
    /// (<see cref="HinokuraForgeRules"/>), the heat meter (<see cref="HinokuraHeatRules"/>), the eruption
    /// (<see cref="HinokuraEruptionRules"/>) and the places the layout declares for them.
    /// </summary>
    public class HinokuraMechanicsTests
    {
        private const long Period = HinokuraForgeRules.TremorPeriodSeconds;

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.HinokuraRegionId);

        // ------------------------------------------------------------------
        // Forges and Tremor
        // ------------------------------------------------------------------

        [Test]
        public void Tremor_WarnsThenJams_OnTheClock()
        {
            Assert.AreEqual(Period, HinokuraForgeRules.SecondsToNextTremor(0L));
            Assert.AreEqual(1L, HinokuraForgeRules.SecondsToNextTremor(Period - 1L));
            Assert.IsFalse(HinokuraForgeRules.IsTremorWarning(Period - 31L));
            Assert.IsTrue(HinokuraForgeRules.IsTremorWarning(Period - 30L));
            Assert.IsTrue(HinokuraForgeRules.IsJammed(Period));
            Assert.IsTrue(HinokuraForgeRules.IsJammed(Period + 19L));
            Assert.IsFalse(HinokuraForgeRules.IsJammed(Period + 20L));
        }

        [Test]
        public void Tremor_StrikesTheForgesInRotation_EvenBeforeTheEpoch()
        {
            Assert.AreEqual(0, HinokuraForgeRules.StruckForge(0L));
            Assert.AreEqual(1, HinokuraForgeRules.StruckForge(1L));
            Assert.AreEqual(2, HinokuraForgeRules.StruckForge(2L));
            Assert.AreEqual(0, HinokuraForgeRules.StruckForge(3L));
            Assert.AreEqual(2, HinokuraForgeRules.StruckForge(-1L));
            Assert.AreEqual(-1L, HinokuraForgeRules.TremorCycle(-1L));
        }

        [Test]
        public void Forge_BurnsTwentyMinutes_WhenNoTremorStrikesIt()
        {
            // Forge 0 is struck by the Tremors of cycles 0, 3, 6: lit at the end of cycle 0, its next strike is 3 × 720 = 2160, after the burn ends.
            const long litAt = 100L;
            Assert.AreEqual(3L * Period, HinokuraForgeRules.NextStrikeAfter(0, litAt));
            Assert.IsTrue(HinokuraForgeRules.IsLit(0, litAt, litAt + HinokuraForgeRules.BurnSeconds - 1L));
            Assert.IsFalse(HinokuraForgeRules.IsLit(0, litAt, litAt + HinokuraForgeRules.BurnSeconds));
        }

        [Test]
        public void Forge_GoesOut_AtItsOwnTremorOnly()
        {
            // Forge 1 is struck at 720, forge 2 at 1440.
            Assert.IsFalse(HinokuraForgeRules.IsLit(1, 100L, Period));
            Assert.IsTrue(HinokuraForgeRules.IsLit(1, 100L, Period - 1L));
            Assert.IsTrue(HinokuraForgeRules.IsLit(2, 100L, Period), "the tremor of cycle 1 spares forge 2");
            Assert.IsFalse(HinokuraForgeRules.IsLit(2, 100L, 2L * Period));
        }

        [Test]
        public void Forge_NeverLit_IsOut_AndCountsAdd()
        {
            Assert.IsFalse(HinokuraForgeRules.IsLit(0, HinokuraForgeRules.NeverLit, 100L));
            long[] litAt = { 100L, HinokuraForgeRules.NeverLit, 100L };
            Assert.AreEqual(2, HinokuraForgeRules.LitCount(litAt, 200L));
        }

        [Test]
        public void Sluice_Opens_ButNotLit_NorJammed()
        {
            Assert.AreEqual(HinokuraSluiceCheck.Open, HinokuraForgeRules.Check(0, HinokuraForgeRules.NeverLit, 100L));
            Assert.AreEqual(HinokuraSluiceCheck.AlreadyLit, HinokuraForgeRules.Check(0, 100L, 200L));
            Assert.AreEqual(HinokuraSluiceCheck.Jammed, HinokuraForgeRules.Check(1, 100L, Period + 5L), "struck by this tremor, then jammed");
            Assert.AreEqual(HinokuraSluiceCheck.Open, HinokuraForgeRules.Check(1, 100L, Period + 20L), "a struck forge can be lit again after the jam");
        }

        [Test]
        public void Glutton_RecoilsPerForge_ButTheLastBlowStaysWithTheHero()
        {
            Assert.AreEqual(0f, HinokuraForgeRules.PulseDamage(1000f, 1000f, 0), 1e-3f);
            Assert.AreEqual(15f, HinokuraForgeRules.PulseDamage(1000f, 1000f, 1), 1e-3f);
            Assert.AreEqual(45f, HinokuraForgeRules.PulseDamage(1000f, 1000f, 3), 1e-3f);
            Assert.AreEqual(10f, HinokuraForgeRules.PulseDamage(410f, 1000f, 3), 1e-3f);
            Assert.AreEqual(0f, HinokuraForgeRules.PulseDamage(400f, 1000f, 3), 1e-3f);
            Assert.AreEqual(0f, HinokuraForgeRules.StaggerSeconds(0), 1e-3f);
            Assert.AreEqual(1.5f, HinokuraForgeRules.StaggerSeconds(3), 1e-3f);
        }

        // ------------------------------------------------------------------
        // Heat
        // ------------------------------------------------------------------

        [Test]
        public void Heat_RisesNearTheLava_FadesWithDistance_AndFallsAway()
        {
            Assert.AreEqual(3f, HinokuraHeatRules.Step(0f, 0f, false, false, 1f), 1e-3f);
            Assert.AreEqual(1.5f, HinokuraHeatRules.Step(0f, 45f, false, false, 1f), 1e-3f);
            Assert.AreEqual(3f, HinokuraHeatRules.Step(0f, -10f, false, false, 1f), 1e-3f, "inside the lake counts as the edge");
            Assert.AreEqual(88f, HinokuraHeatRules.Step(100f, 90f, false, false, 1f), 1e-3f);
            Assert.AreEqual(0f, HinokuraHeatRules.Step(5f, 200f, false, false, 1f), 1e-3f);
            Assert.AreEqual(100f, HinokuraHeatRules.Step(99f, 0f, false, false, 1f), 1e-3f, "the meter stops at full");
        }

        [Test]
        public void Heat_ColdStoneZeroes_AndTheTeaHalves()
        {
            Assert.AreEqual(0f, HinokuraHeatRules.Step(100f, 0f, true, false, 0.016f), 1e-3f);
            Assert.AreEqual(1.5f, HinokuraHeatRules.Step(0f, 0f, false, true, 1f), 1e-3f);
        }

        [TestCase(49.9f, HinokuraHeatBand.Cool)]
        [TestCase(50f, HinokuraHeatBand.Warm)]
        [TestCase(80f, HinokuraHeatBand.Danger)]
        [TestCase(100f, HinokuraHeatBand.Scorching)]
        public void Heat_BandsWarnBeforeItHurts(float heat, HinokuraHeatBand band)
        {
            Assert.AreEqual(band, HinokuraHeatRules.BandOf(heat));
        }

        [Test]
        public void Heat_HurtsOnlyWhenFull_AndNeverKills()
        {
            Assert.AreEqual(0f, HinokuraHeatRules.BurnAmount(99.9f, 1000f, 1000f, 1f), 1e-3f);
            Assert.AreEqual(15f, HinokuraHeatRules.BurnAmount(100f, 1000f, 1000f, 1f), 1e-3f);
            Assert.AreEqual(5f, HinokuraHeatRules.BurnAmount(100f, 205f, 1000f, 1f), 1e-3f);
            Assert.AreEqual(0f, HinokuraHeatRules.BurnAmount(100f, 200f, 1000f, 1f), 1e-3f);
        }

        // ------------------------------------------------------------------
        // Eruption
        // ------------------------------------------------------------------

        [TestCase(0L, HinokuraEruptionPhase.Erupting)]
        [TestCase(19L, HinokuraEruptionPhase.Erupting)]
        [TestCase(20L, HinokuraEruptionPhase.Calm)]
        [TestCase(569L, HinokuraEruptionPhase.Calm)]
        [TestCase(570L, HinokuraEruptionPhase.Warning)]
        [TestCase(599L, HinokuraEruptionPhase.Warning)]
        [TestCase(600L, HinokuraEruptionPhase.Erupting)]
        [TestCase(-1L, HinokuraEruptionPhase.Warning)]
        public void Eruption_FollowsTheClock(long unix, HinokuraEruptionPhase phase)
        {
            Assert.AreEqual(phase, HinokuraEruptionRules.PhaseAt(unix));
        }

        [Test]
        public void Eruption_WarnedRoute_IsTheOneThatErupts_AndRotates()
        {
            Assert.AreEqual(HinokuraEruptionRules.RouteAt(600L), HinokuraEruptionRules.RouteAt(570L));
            Assert.AreEqual(HinokuraEruptionRules.RouteAt(600L), HinokuraEruptionRules.RouteAt(25L), "known from the calm on");
            Assert.AreNotEqual(HinokuraEruptionRules.RouteAt(0L), HinokuraEruptionRules.RouteAt(600L));
            Assert.AreEqual(HinokuraEruptionRules.RouteAt(0L), HinokuraEruptionRules.RouteAt(1800L));
            Assert.AreEqual(HinokuraEruptionRules.RouteAt(600L), HinokuraEruptionRules.RouteAt(-1200L), "stays in range before the epoch");
        }

        [Test]
        public void Eruption_CountsDown()
        {
            Assert.AreEqual(30L, HinokuraEruptionRules.SecondsToChange(570L));
            Assert.AreEqual(5L, HinokuraEruptionRules.SecondsToChange(15L));
            Assert.AreEqual(550L, HinokuraEruptionRules.SecondsToChange(20L));
        }

        [Test]
        public void Eruption_HurtsOnlyInTheRoute_WhileItRuns_AndNeverKills()
        {
            Assert.AreEqual(40f, HinokuraEruptionRules.DamageAmount(HinokuraEruptionPhase.Erupting, true, 1000f, 1000f, 1f), 1e-3f);
            Assert.AreEqual(0f, HinokuraEruptionRules.DamageAmount(HinokuraEruptionPhase.Erupting, false, 1000f, 1000f, 1f), 1e-3f);
            Assert.AreEqual(0f, HinokuraEruptionRules.DamageAmount(HinokuraEruptionPhase.Warning, true, 1000f, 1000f, 1f), 1e-3f);
            Assert.AreEqual(10f, HinokuraEruptionRules.DamageAmount(HinokuraEruptionPhase.Erupting, true, 160f, 1000f, 1f), 1e-3f);
            Assert.AreEqual(0f, HinokuraEruptionRules.DamageAmount(HinokuraEruptionPhase.Erupting, true, 150f, 1000f, 1f), 1e-3f);
        }

        // ------------------------------------------------------------------
        // Layout
        // ------------------------------------------------------------------

        [Test]
        public void Layout_Bounds_AreTheRegions_AndTheAnchorMapsOntoTheWorld()
        {
            Assert.AreEqual(World.Bounds, OverworldLayouts.HinokuraBounds);
            OverworldZone alone = World.Zone(OverworldLayouts.HinokuraAnchorZoneId);
            Assert.IsNotNull(alone);
            Assert.AreEqual(Vector2.zero, alone.Origin);
            OverworldZone stitched = OverworldLayouts.Get(OverworldLayouts.WorldId).Zone(OverworldLayouts.HinokuraAnchorZoneId);
            Assert.IsNotNull(stitched, "the anchor zone id must survive the stitching");
            Assert.AreEqual(OverworldLayouts.HinokuraWorldOffset, stitched.Origin);
        }

        [Test]
        public void Layout_Sluices_AreThree_Dry_BesideTheRoads_InTheirMaps()
        {
            string[] maps = { OverworldLayouts.HinokuraMapId, OverworldLayouts.HinokuraMapId, OverworldLayouts.NoveFornalhasMapId };
            Assert.AreEqual(HinokuraForgeRules.Forges, OverworldLayouts.ForgeSluices.Length);
            Assert.AreEqual(HinokuraForgeRules.Forges, HinokuraMechanicsTexts.ForgeNames.Length);
            for (int i = 0; i < OverworldLayouts.ForgeSluices.Length; i++)
            {
                Vector2 sluice = OverworldLayouts.ForgeSluices[i];
                Assert.AreEqual(maps[i], OverworldRules.ZoneAt(World, sluice).MapIdOrOwner, "sluice " + i);
                Assert.IsFalse(MapLayoutRules.InRiver(World.World, sluice, 2f), "sluice " + i + " in the lava");
                Assert.IsTrue(MapLayoutRules.OnPath(World.World, sluice, HinokuraForgeRules.SluiceReach + 6f), "sluice " + i + " far from the road");
                Assert.IsNull(OverworldRules.SettlementAt(World, sluice), "sluice " + i);
            }
        }

        [Test]
        public void Layout_Lake_IsTheLastLavaPool()
        {
            Assert.AreEqual(OverworldLayouts.LavaPools.Last(), OverworldLayouts.CaldeiraLake[0]);
        }

        [Test]
        public void Layout_ColdStones_LieInTheHeat_Dry_InTheCalderaMap()
        {
            Assert.GreaterOrEqual(OverworldLayouts.CaldeiraColdStones.Length, 3);
            foreach (Vector2 stone in OverworldLayouts.CaldeiraColdStones)
            {
                float distance = RioFireRules.DistanceToFire(OverworldLayouts.CaldeiraLake, stone);
                Assert.Greater(distance, 2f, stone.ToString());
                Assert.Less(distance, HinokuraHeatRules.HeatRange, stone.ToString());
                Assert.AreEqual(OverworldLayouts.CaldeiraMapId, OverworldRules.ZoneAt(World, stone).MapIdOrOwner, stone.ToString());
                Assert.IsFalse(MapLayoutRules.InRiver(World.World, stone, 2f), stone.ToString());
            }
        }

        [Test]
        public void Layout_RoadToTheCaldera_MeetsAColdStone()
        {
            Vector2 roadEnd = new Vector2(-1320f, 1390f);
            float nearest = OverworldLayouts.CaldeiraColdStones.Min(stone => Vector2.Distance(stone, roadEnd));
            Assert.Less(nearest, HinokuraHeatRules.ColdStoneRadius + 5f);
        }

        [Test]
        public void Layout_Glutton_Arena_IsOutOfTheHeat()
        {
            Assert.Greater(RioFireRules.DistanceToFire(OverworldLayouts.CaldeiraLake, OverworldLayouts.GlutaoArena), HinokuraHeatRules.HeatRange);
        }

        [Test]
        public void Layout_EruptionRoutes_AreOnEcheyde_AwayFromThePosts_AndLeaveAWayOut()
        {
            Assert.AreEqual(HinokuraEruptionRules.Routes, OverworldLayouts.EruptionRoutes.Length);
            foreach ((Vector2 a, Vector2 b, float width) in OverworldLayouts.EruptionRoutes)
            {
                Assert.AreEqual(14f, width, 1e-3f);
                foreach (Vector2 p in new[] { a, b, (a + b) * 0.5f })
                {
                    Assert.AreEqual(OverworldLayouts.MonteEcheydeMapId, OverworldRules.ZoneAt(World, p).MapIdOrOwner, p.ToString());
                    Assert.IsNull(OverworldRules.SettlementAt(World, p), p.ToString());
                    Assert.Less(Vector2.Distance(p, OverworldLayouts.EcheydeCenter), 260f, p.ToString());
                }
            }

            for (int i = 0; i < OverworldLayouts.EruptionRoutes.Length; i++)
            {
                Assert.IsFalse(HinokuraEruptionRules.InRoute(OverworldLayouts.EruptionRoutes, i, OverworldLayouts.AbrigoPastoresOffset), "the shepherds' shelter stays safe");
            }
        }

        [Test]
        public void Texts_HaveBothLanguages_AndPlaceholdersMatch()
        {
            var keys = HinokuraMechanicsTexts.Entries.Select(entry => entry.Key).ToList();
            Assert.AreEqual(keys.Count, keys.Distinct().Count(), "duplicate key");
            foreach (var entry in HinokuraMechanicsTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), entry.Key);
                Assert.AreEqual(entry.En.Contains("{0}"), entry.PtBR.Contains("{0}"), entry.Key);
                Assert.AreEqual(entry.En.Contains("{1}"), entry.PtBR.Contains("{1}"), entry.Key);
            }
        }
    }
}
