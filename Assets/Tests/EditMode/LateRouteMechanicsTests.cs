using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The late route mechanics (Docs/Mapas100.md 28, 62, 63, 73, 88, 97; <see cref="LateRouteMechanicsDriver"/>): the walks along the bottom of
    /// Kirta's reef (<see cref="KirtaReefWalkRules"/>), the tunnels that change of the Galerias dos Mamutes (<see cref="MammothGalleryRules"/>), the
    /// floes of the Mar das Auroras (<see cref="AuroraFloeRules"/>), the sledge of the Areal Escondido (<see cref="HiddenSandsSledgeRules"/>), the
    /// dark of the Cova da Treva (<see cref="DarkPitLightRules"/>) and the lanterns between the barques (<see cref="SunShipLanternRules"/>): pure
    /// rules, the places the layouts declare for them (stations on the roads, inside the driver's bounds) and their texts.
    /// </summary>
    public class LateRouteMechanicsTests
    {
        /// <summary>The hero's walking speed (m/s).</summary>
        private const float WalkSpeed = 5.5f;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            MapLayouts.ClearCache();
            OverworldLayouts.ClearCache();
        }

        // ------------------------------------------------------------------
        // Bolhas de Ar (28)
        // ------------------------------------------------------------------

        [Test]
        public void Reef_TheShellOpensTheLagoon_TheBubbleTheAbyss_AndTheSharksCloseItAtNight()
        {
            ReefDive north = OverworldLayouts.KirtaReefDives[0];
            ReefDive south = OverworldLayouts.KirtaReefDives[1];
            Assert.IsFalse(north.Abyss);
            Assert.IsTrue(south.Abyss);

            float bare = KirtaReefWalkRules.BreathSeconds(false, false);
            float shell = KirtaReefWalkRules.BreathSeconds(true, false);
            float bubble = KirtaReefWalkRules.BreathSeconds(false, true);
            Assert.AreEqual(bubble, KirtaReefWalkRules.BreathSeconds(true, true), "the bubble outlasts the shell");
            Assert.Less(bare, shell);
            Assert.Less(shell, bubble);

            Assert.Greater(KirtaReefWalkRules.BreathNeeded(north.Course), bare, "bare lungs do not reach the islets");
            Assert.LessOrEqual(KirtaReefWalkRules.BreathNeeded(north.Course), shell, "the shell does");
            Assert.Greater(KirtaReefWalkRules.BreathNeeded(south.Course), shell, "the abyss asks for more than the shell");
            Assert.LessOrEqual(KirtaReefWalkRules.BreathNeeded(south.Course), bubble, "the bubble reaches the great lagoon");

            Assert.AreEqual(ReefDiveCheck.NoAir, KirtaReefWalkRules.Check(north, bare, false, true));
            Assert.AreEqual(ReefDiveCheck.Dive, KirtaReefWalkRules.Check(north, shell, false, true));
            Assert.AreEqual(ReefDiveCheck.Dive, KirtaReefWalkRules.Check(north, shell, true, true), "no sharks over the lagoon");
            Assert.AreEqual(ReefDiveCheck.NoAir, KirtaReefWalkRules.Check(south, shell, false, true));
            Assert.AreEqual(ReefDiveCheck.Dive, KirtaReefWalkRules.Check(south, bubble, false, true));
            Assert.AreEqual(ReefDiveCheck.Sharks, KirtaReefWalkRules.Check(south, bubble, true, true));
            Assert.AreEqual(ReefDiveCheck.ZoneBlocked, KirtaReefWalkRules.Check(north, shell, false, false));
        }

        [Test]
        public void Reef_ABreathAtTheStartAndOneAtEveryBubble_ForwardAndBack()
        {
            Vector2[] course = { Vector2.zero, new Vector2(10f, 0f), new Vector2(30f, 0f), new Vector2(40f, 0f) };
            Assert.AreEqual(20f, KirtaReefWalkRules.LongestStretch(course), 1e-4f);
            Assert.AreEqual(1, KirtaReefWalkRules.BreathsTaken(course, 0f, false));
            Assert.AreEqual(1, KirtaReefWalkRules.BreathsTaken(course, 9f, false));
            Assert.AreEqual(2, KirtaReefWalkRules.BreathsTaken(course, 10f, false));
            Assert.AreEqual(3, KirtaReefWalkRules.BreathsTaken(course, 35f, false));
            Assert.AreEqual(3, KirtaReefWalkRules.BreathsTaken(course, 40f, false), "the far station is not a bubble");
            Assert.AreEqual(1, KirtaReefWalkRules.BreathsTaken(course, 9f, true));
            Assert.AreEqual(2, KirtaReefWalkRules.BreathsTaken(course, 10f, true), "back from 40: the bubble at 30");
            Assert.AreEqual(3, KirtaReefWalkRules.BreathsTaken(course, 30f, true));

            Assert.AreEqual(0, KirtaReefWalkRules.StationAt(OverworldLayouts.KirtaReefDives, OverworldLayouts.KirtaReefDives[0].Start));
            Assert.AreEqual(1, KirtaReefWalkRules.StationAt(OverworldLayouts.KirtaReefDives, OverworldLayouts.KirtaReefDives[0].End));
            Assert.AreEqual(3, KirtaReefWalkRules.StationAt(OverworldLayouts.KirtaReefDives, OverworldLayouts.KirtaReefDives[1].End));
            Assert.AreEqual(-1, KirtaReefWalkRules.StationAt(OverworldLayouts.KirtaReefDives, new Vector2(-5000f, 0f)));
        }

        [Test]
        public void Reef_TheRideWalksTheBottom_NoDeeperThanTheDive()
        {
            float water = OverworldRelief.WaterY;
            Assert.AreEqual(water + 3f, MechanicsRide.BottomOrSurface(water + 3f, water, KirtaReefWalkRules.DiveDepth), "dry ground: the ground");
            Assert.AreEqual(water, MechanicsRide.BottomOrSurface(water - 5f, water, 0f), "no depth: the surface");
            Assert.AreEqual(water - 1f, MechanicsRide.BottomOrSurface(water - 1f, water, KirtaReefWalkRules.DiveDepth), "a shallow bottom");
            Assert.AreEqual(water - KirtaReefWalkRules.DiveDepth, MechanicsRide.BottomOrSurface(water - 9f, water, KirtaReefWalkRules.DiveDepth), "never deeper");
        }

        [Test]
        public void Reef_WalksLeaveKirtasGround_AndComeUpOnItsTrails()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.CostaId);
            Assert.IsNotNull(world);
            foreach (ReefDive dive in OverworldLayouts.KirtaReefDives)
            {
                Assert.AreEqual(OverworldLayouts.KirtaMapId, OverworldRules.ZoneAt(world, dive.Start)?.MapIdOrOwner, dive.Id + ": leaves from Kirta's atoll");
                Assert.LessOrEqual(Vector2.Distance(dive.Start, OverworldLayouts.KirtaOffset), OverworldLayouts.RaceCityGround, dive.Id + ": on the town's ground");
                Assert.Greater(OverworldRules.SurfaceAt(world, dive.Start), OverworldRelief.WaterY, dive.Id + ": a dry station");
                Assert.IsTrue(MapLayoutRules.OnPath(world.World, dive.End, 0.5f), dive.Id + ": comes up on a trail");
                foreach (Vector2 p in dive.Course)
                {
                    Assert.IsTrue(LateRouteMechanicsDriver.ReefBounds.Contains(p), $"{dive.Id} {p}: in the driver's bounds");
                }

                float length = RidePath.Length(dive.Course);
                for (float d = 0f; d <= length; d += 5f)
                {
                    Assert.IsNotNull(OverworldRules.ZoneAt(world, RidePath.PointAt(dive.Course, d)), $"{dive.Id} {d} m: in an area");
                }

                Assert.Greater(Vector2.Distance(dive.Start, dive.End), 2f * KirtaReefWalkRules.StationReach, "the two ends are two stations");
            }
        }

        // ------------------------------------------------------------------
        // Túneis que mudam (62)
        // ------------------------------------------------------------------

        [Test]
        public void Gallery_EveryWeekTwoTunnelsBetweenRoads_TwoMouthsCaved_TheSameForEveryone()
        {
            var seen = new HashSet<string>();
            for (long week = 2900; week < 3000; week++)
            {
                int[] tunnels = MammothGalleryRules.TunnelsFor(week);
                CollectionAssert.AreEqual(tunnels, MammothGalleryRules.TunnelsFor(week), "the same seed, the same tunnels");
                Assert.AreEqual(MammothGalleryRules.MouthCount, tunnels.Length);
                Assert.AreEqual(2, tunnels.Count(t => t < 0), "two mouths caved in");
                for (int m = 0; m < tunnels.Length; m++)
                {
                    if (tunnels[m] < 0)
                    {
                        continue;
                    }

                    Assert.AreEqual(m, tunnels[tunnels[m]], "a tunnel runs both ways");
                    Assert.AreNotEqual(MammothGalleryRules.RoadOf(m), MammothGalleryRules.RoadOf(tunnels[m]), "never two mouths of one road");
                    Assert.AreEqual(GalleryMouthCheck.Walk, MammothGalleryRules.Check(tunnels, m, true));
                    Assert.AreEqual(GalleryMouthCheck.ZoneBlocked, MammothGalleryRules.Check(tunnels, m, false));
                }

                int caved = System.Array.IndexOf(tunnels, -1);
                Assert.AreEqual(GalleryMouthCheck.CavedIn, MammothGalleryRules.Check(tunnels, caved, true));
                seen.Add(string.Join(",", tunnels));
            }

            Assert.GreaterOrEqual(seen.Count, 6, "the galleries change from week to week");
        }

        [Test]
        public void Gallery_TheWeekTurnsOnMondayMidnightUtc()
        {
            const double monday = 4d * 86400d; // 1970-01-05, a Monday
            Assert.AreEqual(0L, MammothGalleryRules.WeekAt(monday - 1d));
            Assert.AreEqual(1L, MammothGalleryRules.WeekAt(monday));
            Assert.AreEqual(1L, MammothGalleryRules.WeekAt(monday + 7d * 86400d - 1d));
            Assert.AreEqual(10d, MammothGalleryRules.SecondsToNextWeek(monday - 10d), 1e-6);
            Assert.AreEqual(MammothGalleryRules.WeekSeconds, MammothGalleryRules.SecondsToNextWeek(monday), 1e-6);
            double now = 1_800_000_000d;
            Assert.AreEqual(MammothGalleryRules.WeekAt(now) + 1, MammothGalleryRules.WeekAt(now + MammothGalleryRules.SecondsToNextWeek(now)));
        }

        [Test]
        public void Gallery_MouthsOnTheRoadsOutOfTheBoca_InTheGalleries()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.MarGeladoRegionId);
            Assert.IsNotNull(world);
            Vector2[] mouths = OverworldLayouts.MammothGalleryMouths;
            Assert.AreEqual(MammothGalleryRules.MouthCount, mouths.Length);
            for (int i = 0; i < mouths.Length; i++)
            {
                Assert.IsTrue(MapLayoutRules.OnPath(world.World, mouths[i], 0.5f), $"mouth {i} {mouths[i]}: on a road");
                Assert.AreEqual(OverworldLayouts.GaleriasMamutesMapId, OverworldRules.ZoneAt(world, mouths[i])?.MapIdOrOwner, $"mouth {i}: in 62");
                Assert.IsTrue(LateRouteMechanicsDriver.GalleryBounds.Contains(mouths[i]), $"mouth {i}: in the driver's bounds");
                Assert.AreEqual(i, MammothGalleryRules.MouthAt(mouths, mouths[i]));
                for (int j = i + 1; j < mouths.Length; j++)
                {
                    Assert.Greater(Vector2.Distance(mouths[i], mouths[j]), 2f * MammothGalleryRules.StationReach, $"mouths {i} and {j} apart");
                }
            }

            Assert.AreEqual(LateRouteMechanicsTexts.RoadSouth, LateRouteMechanicsDriver.RoadKey(0));
            Assert.AreEqual(LateRouteMechanicsTexts.RoadEast, LateRouteMechanicsDriver.RoadKey(1));
            Assert.AreEqual(LateRouteMechanicsTexts.RoadNorth, LateRouteMechanicsDriver.RoadKey(2));
        }

        // ------------------------------------------------------------------
        // Pular placas (63)
        // ------------------------------------------------------------------

        [Test]
        public void Floes_TheAuroraDancesFiftySecondsInFiveMinutes_AndTheFloesStandInLine()
        {
            double start = 1_800_000_000d - 1_800_000_000d % AuroraFloeRules.DanceEvery;
            Assert.IsTrue(AuroraFloeRules.Dancing(start));
            Assert.IsTrue(AuroraFloeRules.Dancing(start + 49d));
            Assert.IsFalse(AuroraFloeRules.Dancing(start + 50d));
            Assert.AreEqual(0f, AuroraFloeRules.SecondsToDance(start + 10d));
            Assert.AreEqual(40f, AuroraFloeRules.DanceLeft(start + 10d), 1e-3f);
            Assert.AreEqual(200f, AuroraFloeRules.SecondsToDance(start + 100d), 1e-3f);
            Assert.AreEqual(0f, AuroraFloeRules.DanceLeft(start + 100d));

            float most = 0f;
            for (int floe = 1; floe <= OverworldLayouts.AuroraFloeCount; floe++)
            {
                Assert.AreEqual(0f, AuroraFloeRules.Drift(floe, start + 20d, false), "in line while it dances");
                for (double t = start + 50d; t < start + 300d; t += 3d)
                {
                    float drift = AuroraFloeRules.Drift(floe, t, false);
                    Assert.LessOrEqual(Mathf.Abs(drift), AuroraFloeRules.DriftAmplitude + 1e-4f);
                    Assert.AreEqual(0f, AuroraFloeRules.Drift(floe, t, true), "still all night");
                    most = Mathf.Max(most, Mathf.Abs(drift));
                }
            }

            Assert.Greater(most, 3f, "by day the chain is broken");
            Assert.AreEqual(FloeCheck.Hop, AuroraFloeRules.Check(start + 5d, false, true));
            Assert.AreEqual(FloeCheck.Drifting, AuroraFloeRules.Check(start + 100d, false, true));
            Assert.AreEqual(FloeCheck.Hop, AuroraFloeRules.Check(start + 100d, true, true), "at night they stand still");
            Assert.AreEqual(FloeCheck.ZoneBlocked, AuroraFloeRules.Check(start + 5d, false, false));
        }

        [Test]
        public void Floes_ChainFromTheNorthRoadToTheLighthouseRoad_AHopWithinADance_ShorterThanTheWayRound()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.MarGeladoRegionId);
            Vector2[] chain = OverworldLayouts.AuroraFloeChain;
            Assert.AreEqual(OverworldLayouts.AuroraFloeCount + 2, chain.Length);
            Assert.IsTrue(MapLayoutRules.OnPath(world.World, chain[0], 0.5f), "the west end on the north road");
            Assert.IsTrue(MapLayoutRules.OnPath(world.World, chain[chain.Length - 1], 0.5f), "the east end on the lighthouse road");
            Assert.AreEqual(0, AuroraFloeRules.EndAt(chain, chain[0]));
            Assert.AreEqual(1, AuroraFloeRules.EndAt(chain, chain[chain.Length - 1]));
            Assert.AreEqual(-1, AuroraFloeRules.EndAt(chain, chain[chain.Length / 2]));
            foreach (Vector2 p in chain)
            {
                Assert.IsTrue(LateRouteMechanicsDriver.FloeBounds.Contains(p), p + ": in the driver's bounds");
                string map = OverworldRules.ZoneAt(world, p)?.MapIdOrOwner;
                Assert.IsNotNull(map, p + ": in an area");
            }

            float hop = RidePath.Length(chain);
            Assert.Less(hop / AuroraFloeRules.HopSpeed, (float)AuroraFloeRules.DanceSeconds, "a hop begun with the dance ends within it");
            float round = RidePath.Length(new[] { chain[0], new Vector2(40f, 960f), new Vector2(240f, 1040f), chain[chain.Length - 1] });
            Assert.Less(hop, round * 0.75f, "the floes beat the road round by the sea fork");
            for (int i = 0; i + 1 < chain.Length; i++)
            {
                Assert.Less(Vector2.Distance(chain[i], chain[i + 1]), 45f, "one hop a floe");
            }
        }

        // ------------------------------------------------------------------
        // Puxar o trenó (73)
        // ------------------------------------------------------------------

        [Test]
        public void Sledge_AloneItCrawls_HeavesAndAlliesPull_TheSerpentRuns()
        {
            Assert.AreEqual(HiddenSandsSledgeRules.SoloSpeed, HiddenSandsSledgeRules.SpeedFor(false, 0, 0));
            Assert.Less(HiddenSandsSledgeRules.SoloSpeed, WalkSpeed, "alone, slower than a walk");
            Assert.Greater(HiddenSandsSledgeRules.SpeedFor(false, 0, 2), HiddenSandsSledgeRules.SpeedFor(false, 0, 1));
            Assert.Greater(HiddenSandsSledgeRules.SpeedFor(false, 1, 0), HiddenSandsSledgeRules.SpeedFor(false, 0, 0));
            Assert.AreEqual(HiddenSandsSledgeRules.SpeedFor(false, HiddenSandsSledgeRules.MaxAllies, 0), HiddenSandsSledgeRules.SpeedFor(false, 10, 0), "at most three allies");
            Assert.AreEqual(HiddenSandsSledgeRules.MaxHauledSpeed, HiddenSandsSledgeRules.SpeedFor(false, 3, 10), "capped");
            Assert.AreEqual(HiddenSandsSledgeRules.SerpentSpeed, HiddenSandsSledgeRules.SpeedFor(true, 0, 0));
            Assert.Greater(HiddenSandsSledgeRules.SerpentSpeed, HiddenSandsSledgeRules.MaxHauledSpeed);

            Assert.IsTrue(HiddenSandsSledgeRules.DoorGlows(100f, 95f));
            Assert.IsFalse(HiddenSandsSledgeRules.DoorGlows(100f, 80f));
            Assert.IsTrue(HiddenSandsSledgeRules.Pays(20000L, 20001L));
            Assert.IsFalse(HiddenSandsSledgeRules.Pays(20001L, 20001L), "once a day");
            Assert.IsTrue(HiddenSandsSledgeRules.Pays(0L, 20001L), "never paid");
        }

        [Test]
        public void Sledge_RunsDownTheTrenosRoad_ThroughThreeDoors()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.RioRegionId);
            Assert.IsNotNull(world);
            Vector2[] course = OverworldLayouts.HiddenSandsSledgeCourse;
            float length = RidePath.Length(course);
            for (float d = 0f; d <= length; d += 2f)
            {
                Vector2 p = RidePath.PointAt(course, d);
                Assert.IsTrue(MapLayoutRules.OnPath(world.World, p, 0.5f), $"{p}: on the sledge road");
                Assert.IsTrue(LateRouteMechanicsDriver.SledgeBounds.Contains(p), $"{p}: in the driver's bounds");
            }

            Assert.AreEqual(OverworldLayouts.ArealEscondidoMapId, OverworldRules.ZoneAt(world, course[course.Length - 1])?.MapIdOrOwner, "it ends in 73");
            Assert.AreEqual(0, HiddenSandsSledgeRules.StationAt(course, course[0]));
            Assert.AreEqual(1, HiddenSandsSledgeRules.StationAt(course, course[course.Length - 1]));
            Assert.AreEqual(3, OverworldLayouts.HiddenSandsDoorsAlong.Length);
            float last = 0f;
            foreach (float door in OverworldLayouts.HiddenSandsDoorsAlong)
            {
                Assert.Greater(door, last + HiddenSandsSledgeRules.DoorGlow, "the doors glow one at a time");
                Assert.Less(door, length - HiddenSandsSledgeRules.DoorGlow, "on the road");
                last = door;
            }

            Assert.Less(length / HiddenSandsSledgeRules.SoloSpeed, 180f, "alone it is slow, not endless");
        }

        // ------------------------------------------------------------------
        // Escuro (88)
        // ------------------------------------------------------------------

        [Test]
        public void Dark_ThirtyPercentLessDamageOutOfTheLight()
        {
            Assert.AreEqual(1f, DarkPitLightRules.DamageTakenMultiplier(true));
            Assert.AreEqual(0.7f, DarkPitLightRules.DamageTakenMultiplier(false), 1e-5f);
            Assert.AreEqual(DarkPitLightRules.DamageTakenMultiplier(false), DamageCalculator.GetDefenseMultiplier(0f, DarkPitLightRules.DarkDefense, null), 1e-5f,
                "the dark's Defense on the beast is exactly the −30%");

            Assert.AreEqual(DarkPitLightRules.MariaRadius, DarkPitLightRules.HeroLightRadius(true, false));
            Assert.AreEqual(DarkPitLightRules.LampRadius, DarkPitLightRules.HeroLightRadius(false, true));
            Assert.AreEqual(0f, DarkPitLightRules.HeroLightRadius(false, false), "no lamp, no light");
            Assert.Greater(DarkPitLightRules.MariaRadius, DarkPitLightRules.LampRadius);

            Vector2 hero = Vector2.zero;
            Vector2 post = new Vector2(1000f, 0f);
            var stands = new[] { new Vector2(0f, 100f) };
            double now = 1_800_000_000d;
            var unlit = new[] { 0d };
            var lit = new[] { DarkPitLightRules.LightStand(now) };
            float lamp = DarkPitLightRules.HeroLightRadius(false, true);
            Assert.IsTrue(DarkPitLightRules.Lit(new Vector2(4f, 0f), hero, lamp, stands, unlit, post, 0d, now), "in the lamp's circle");
            Assert.IsFalse(DarkPitLightRules.Lit(new Vector2(8f, 0f), hero, lamp, stands, unlit, post, 0d, now), "out of it");
            Assert.IsTrue(DarkPitLightRules.Lit(new Vector2(8f, 0f), hero, DarkPitLightRules.HeroLightRadius(true, false), stands, unlit, post, 0d, now), "Maria's light");
            Assert.IsFalse(DarkPitLightRules.Lit(new Vector2(3f, 0f), hero, 0f, stands, unlit, post, 0d, now), "no light at all");
            Assert.IsFalse(DarkPitLightRules.Lit(new Vector2(0f, 90f), hero, lamp, stands, unlit, post, 0d, now), "an unlit stand");
            Assert.IsTrue(DarkPitLightRules.Lit(new Vector2(0f, 90f), hero, lamp, stands, lit, post, 0d, now), "a burning stand");
            Assert.IsFalse(DarkPitLightRules.Lit(new Vector2(0f, 90f), hero, lamp, stands, lit, post, 0d, now + DarkPitLightRules.StandBurnSeconds + 1d), "burnt out");
            Assert.IsTrue(DarkPitLightRules.Lit(new Vector2(1010f, 0f), hero, lamp, stands, unlit, post, 0d, now), "round the post");
            Assert.IsTrue(DarkPitLightRules.Lit(new Vector2(50f, 50f), hero, lamp, stands, unlit, post, now + 1d, now), "a Holy hit lights it");

            Assert.AreEqual(0, DarkPitLightRules.StandAt(stands, new Vector2(1f, 99f)));
            Assert.AreEqual(-1, DarkPitLightRules.StandAt(stands, new Vector2(0f, 90f)));
            Assert.AreNotEqual(DarkPitLightRules.StandKey(0), DarkPitLightRules.StandKey(1));
        }

        [Test]
        public void Dark_StandsOnTheRoadsOfThePitAndTheTunnels()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.ProfundoRegionId);
            Assert.IsNotNull(world);
            foreach (Rect area in OverworldLayouts.DarkPitAreas)
            {
                Assert.AreEqual(OverworldLayouts.CovaTrevaMapId, OverworldRules.ZoneAt(world, area.center)?.MapIdOrOwner, area + ": map 88");
            }

            Vector2[] stands = OverworldLayouts.DarkPitStands;
            for (int i = 0; i < stands.Length; i++)
            {
                Assert.IsTrue(MapLayoutRules.OnPath(world.World, stands[i], 0.5f), $"stand {i} {stands[i]}: on a road");
                Assert.IsTrue(LateRouteMechanicsDriver.InDarkPit(stands[i]), $"stand {i}: in the dark");
                Assert.AreEqual(i, DarkPitLightRules.StandAt(stands, stands[i]));
            }

            Assert.IsTrue(LateRouteMechanicsDriver.InDarkPit(OverworldLayouts.LampadaTrevaOffset), "the Lâmpada 5 post stands in the pit");
            Assert.IsFalse(LateRouteMechanicsDriver.InDarkPit(new Vector2(0f, 0f)));
        }

        // ------------------------------------------------------------------
        // Carregar a luz (97)
        // ------------------------------------------------------------------

        [Test]
        public void Lanterns_TheFlameBurnsAndShakes_RidesPutItOut_FivePayTheVoyage()
        {
            Assert.AreEqual(90f, SunShipLanternRules.Burn(100f, 10f, 0), 1e-4f);
            Assert.AreEqual(100f - 1f - SunShipLanternRules.BlowSeconds * 2f, SunShipLanternRules.Burn(100f, 1f, 2), 1e-4f);
            Assert.AreEqual(0f, SunShipLanternRules.Burn(5f, 10f, 3), "never below zero");
            Assert.AreEqual(LanternStep.Burning, SunShipLanternRules.Step(10f, 0.2f));
            Assert.AreEqual(LanternStep.BurntOut, SunShipLanternRules.Step(0f, 0.2f));
            Assert.AreEqual(LanternStep.Carried, SunShipLanternRules.Step(10f, 30f), "a teleport");
            Assert.AreEqual(LanternStep.Carried, SunShipLanternRules.Step(10f, 0.2f, true), "a ride");
            Assert.AreEqual(LanternStep.Burning, SunShipLanternRules.Step(10f, 1.5f), "a mount's stride is a walk");

            Assert.AreEqual(SunShipLanternRules.GoldPerLantern, SunShipLanternRules.GoldFor(0));
            Assert.AreEqual(SunShipLanternRules.GoldPerLantern + SunShipLanternRules.VoyageGold, SunShipLanternRules.GoldFor(SunShipLanternRules.LanternsPerVoyage - 1));
            Assert.AreEqual(0, SunShipLanternRules.GoldFor(SunShipLanternRules.LanternsPerVoyage));
            Assert.IsTrue(SunShipLanternRules.CanSet(4));
            Assert.IsFalse(SunShipLanternRules.CanSet(5));

            long day = 20833L;
            long mark = SunShipLanternRules.StoreSet(day, 3);
            Assert.AreEqual(3, SunShipLanternRules.SetOn(mark, day));
            Assert.AreEqual(0, SunShipLanternRules.SetOn(mark, day + 1), "a new day starts at zero");
            Assert.AreEqual(0, SunShipLanternRules.SetOn(0L, day));
        }

        [Test]
        public void Lanterns_RackOnTheMoonsDeck_StandOnTheSuns_TheWalkWithinTheFlame()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.CeuRegionId);
            Assert.IsNotNull(world);
            Vector2 rack = OverworldLayouts.MoonLanternRack;
            Vector2 stand = OverworldLayouts.SunLanternStand;
            Assert.IsTrue(MapLayoutRules.OnPath(world.World, rack, 0.5f), "the rack on the trunk road");
            Assert.IsTrue(MapLayoutRules.OnPath(world.World, stand, 0.5f), "the stand on the Pérgola's lane");
            Assert.AreEqual(OverworldLayouts.BarcaDaLuaMapId, OverworldRules.ZoneAt(world, rack)?.MapIdOrOwner, "the rack on the Moon Ship (93)");
            Assert.AreEqual(OverworldLayouts.BarcaDoSolMapId, OverworldRules.ZoneAt(world, stand)?.MapIdOrOwner, "the stand on the Sun Ship (97)");
            Assert.IsTrue(LateRouteMechanicsDriver.LanternBounds.Contains(rack));
            Assert.IsTrue(LateRouteMechanicsDriver.LanternBounds.Contains(stand));

            // Up the trunk to the Pérgola's row, then east along its lane.
            float walk = RidePath.Length(new[] { rack, new Vector2(rack.x, stand.y), stand });
            Assert.Less(walk / WalkSpeed, SunShipLanternRules.FlameSeconds * 0.75f, "a walk with time for a few fights");
            Assert.IsTrue(SunShipLanternRules.AtStation(rack, rack + new Vector2(3f, 0f)));
            Assert.IsFalse(SunShipLanternRules.AtStation(rack, stand));
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Texts_EveryKeyInBothLanguages_NoFolksyNames()
        {
            var keys = new HashSet<string>();
            foreach (var entry in LateRouteMechanicsTexts.Entries)
            {
                Assert.IsTrue(keys.Add(entry.Key), "duplicate key " + entry.Key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), entry.Key);
                StringAssert.DoesNotContain("Dona ", entry.PtBR);
                StringAssert.DoesNotContain("Seu ", entry.PtBR);
            }

            foreach (FieldInfo field in typeof(LateRouteMechanicsTexts).GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.IsLiteral && f.FieldType == typeof(string)))
            {
                Assert.IsTrue(keys.Contains((string)field.GetRawConstantValue()), field.Name + " has an entry");
            }

            Assert.AreEqual("As placas se afastam: a aurora dança em 3:05", LateRouteMechanicsTexts.Format(null, LateRouteMechanicsTexts.FloeDrifting, 3, 5));
            StringAssert.EndsWith("Os túneis mudam em 2 d 7 h", LateRouteMechanicsTexts.Format(null, LateRouteMechanicsTexts.TunnelCaved, 2, 7));
            Assert.AreEqual("A lanterna está no convés do Sol: 360 de ouro (5/5 hoje)", LateRouteMechanicsTexts.Format(null, LateRouteMechanicsTexts.LanternSet, 360, 5));
        }
    }
}
