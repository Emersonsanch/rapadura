using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The rafts of the Campos de Juncos (Docs/Mapas100.md 72, <see cref="RioRaftRules"/>): reeds, bundles and the raft's life from the time
    /// marks, what [E] at a landing does, the course, the hippo bites and the splash, and the canals, landings and clumps the layout declares.
    /// </summary>
    public class RioRaftMechanicsTests
    {
        private const long T = 1_700_000_000L;

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.RioRegionId);

        [Test]
        public void TaweretsHippo_SwimsTheCanal_WithTheHeroStillMounted()
        {
            Assert.IsTrue(RioRaftRules.SwimsOnHippo(RioRaftCheck.Ride, hippoMount: true, raftLive: false), "no raft: the hippo swims");
            Assert.IsFalse(RioRaftRules.SwimsOnHippo(RioRaftCheck.Ride, hippoMount: true, raftLive: true), "a live raft: the hero sails it");
            Assert.IsFalse(RioRaftRules.SwimsOnHippo(RioRaftCheck.Ride, hippoMount: false, raftLive: true));
            Assert.IsFalse(RioRaftRules.SwimsOnHippo(RioRaftCheck.Resting, hippoMount: true, raftLive: false));
            Assert.IsFalse(RioRaftRules.SwimsOnHippo(RioRaftCheck.ZoneBlocked, hippoMount: true, raftLive: false));

            Assert.IsFalse(MechanicsRide.DismountsHero(keepMount: true), "the hippo's own swim keeps the hero on it");
            Assert.IsTrue(MechanicsRide.DismountsHero(keepMount: false), "every other ride (raft, updraft, cart, river) gets the hero off");

            var ride = new MechanicsRide();
            Assert.IsFalse(ride.Begin(null, World, new[] { Vector2.zero, Vector2.one }, 1f, 0f, 0f, keepMount: true), "no hero: nothing changes");
            Assert.IsFalse(ride.Active);
            Assert.IsFalse(ride.KeepsMount);
            ride.End();
            Assert.AreEqual(RideStep.Idle, ride.Tick(0.1f));
        }

        [Test]
        public void Reeds_RegrowAfterFiveMinutes()
        {
            Assert.IsTrue(RioRaftRules.ReedReady(0L, T), "never cut");
            Assert.IsFalse(RioRaftRules.ReedReady(T, T + 299L));
            Assert.IsTrue(RioRaftRules.ReedReady(T, T + RioRaftRules.ReedRegrowSeconds));
            Assert.AreEqual(240L, RioRaftRules.ReedSecondsLeft(T, T + 60L));
            Assert.AreEqual(0L, RioRaftRules.ReedSecondsLeft(0L, T));
        }

        [Test]
        public void Bundles_AreTheClumpsCutSinceTheLastRaft_AtMostThree()
        {
            Assert.AreEqual(0, RioRaftRules.Bundles(new long[8], 0L));
            Assert.AreEqual(2, RioRaftRules.Bundles(new[] { T, 0L, T + 5L, 0L }, 0L));
            Assert.AreEqual(3, RioRaftRules.Bundles(new[] { T, T + 1L, T + 2L, T + 3L, T + 4L }, 0L), "capped at one raft");
            Assert.AreEqual(1, RioRaftRules.Bundles(new[] { T, T + 1L, T + 2L, T + 30L }, T + 10L), "tying the raft spent the earlier ones");
            Assert.AreEqual(0, RioRaftRules.Bundles(new[] { T, T + 1L, T + 2L }, T + 2L), "a bundle cut in the second the raft was tied is spent");
            Assert.AreEqual(0, RioRaftRules.Bundles(null, 0L));
        }

        [Test]
        public void Raft_KeepsForTwoMinutes()
        {
            Assert.IsFalse(RioRaftRules.RaftLive(0L, T));
            Assert.IsTrue(RioRaftRules.RaftLive(T, T));
            Assert.IsTrue(RioRaftRules.RaftLive(T, T + 119L));
            Assert.IsFalse(RioRaftRules.RaftLive(T, T + RioRaftRules.RaftLifeSeconds));
            Assert.IsFalse(RioRaftRules.RaftLive(T, T - 5L), "a mark from the future (clock moved back) is no raft");
        }

        [Test]
        public void Check_Zone_ThenRest_ThenRaftOrHippo_ThenReeds()
        {
            Assert.AreEqual(RioRaftCheck.ZoneBlocked, RioRaftRules.Check(false, true, true, 3, 99f));
            Assert.AreEqual(RioRaftCheck.Resting, RioRaftRules.Check(true, true, false, 3, 1f));
            Assert.AreEqual(RioRaftCheck.Ride, RioRaftRules.Check(true, true, false, 0, 99f));
            Assert.AreEqual(RioRaftCheck.Ride, RioRaftRules.Check(true, false, true, 0, 99f), "Taweret's hippo needs no raft");
            Assert.AreEqual(RioRaftCheck.Build, RioRaftRules.Check(true, false, false, 3, 99f));
            Assert.AreEqual(RioRaftCheck.NeedReeds, RioRaftRules.Check(true, false, false, 2, 99f));
        }

        [Test]
        public void Bites_EveryFiveSeconds_TwoPercent_ThreeAtNight_NeverUnderThirty()
        {
            Assert.AreEqual(0, RioRaftRules.BitesDue(4.9f));
            Assert.AreEqual(1, RioRaftRules.BitesDue(5f));
            Assert.AreEqual(12, RioRaftRules.BitesDue(62f));
            Assert.AreEqual(2f, RioRaftRules.BiteAmount(100f, 100f, false), 1e-4f);
            Assert.AreEqual(3f, RioRaftRules.BiteAmount(100f, 100f, true), 1e-4f);
            Assert.AreEqual(1f, RioRaftRules.BiteAmount(31f, 100f, true), 1e-4f);
            Assert.AreEqual(0f, RioRaftRules.BiteAmount(30f, 100f, false));
            Assert.AreEqual(60f, RioRaftRules.SplashAmount(1000f, false), 1e-3f);
            Assert.AreEqual(10f, RioRaftRules.SplashAmount(1000f, true), 1e-3f);
        }

        [Test]
        public void SaveKeys_AreDistinct()
        {
            var keys = new HashSet<string> { RioRaftRules.BuiltKey };
            for (int i = 0; i < OverworldLayouts.RioReedClumps.Length; i++)
            {
                Assert.IsTrue(keys.Add(RioRaftRules.ReedKey(i)));
            }

            var progress = new WorldMapProgress();
            progress.SetTimeMark(RioRaftRules.ReedKey(2), T);
            Assert.AreEqual(T, progress.GetTimeMark(RioRaftRules.ReedKey(2)));
            Assert.AreEqual(0L, progress.GetTimeMark(RioRaftRules.BuiltKey));
        }

        [Test]
        public void Course_RunsTheWholeCanal_ToTheFarLanding()
        {
            RioRaftCanal[] canals = OverworldLayouts.RioRaftCanals;
            for (int landing = 0; landing < canals.Length * 2; landing++)
            {
                Vector2[] course = RioRaftRules.Course(canals, landing);
                int far = RioRaftRules.FarLanding(landing);
                Assert.AreEqual(landing / 2, far / 2);
                Assert.AreNotEqual(landing, far);
                Assert.AreEqual(RioRaftRules.LandingPoint(canals, landing), course[0]);
                Assert.AreEqual(RioRaftRules.LandingPoint(canals, far), course[course.Length - 1]);
                Assert.AreEqual(landing, RioRaftRules.LandingAt(canals, course[0]));
                Assert.AreEqual(far, RioRaftRules.LandingAt(canals, course[course.Length - 1]));
                Assert.That(RidePath.Length(course), Is.InRange(860f, 870f), "landing " + landing);
                Assert.That(RioRaftRules.RideSeconds(canals, landing), Is.InRange(60f, 64f), "landing " + landing);
            }

            Assert.AreEqual(-1, RioRaftRules.LandingAt(canals, OverworldLayouts.CasaDeJuncoOffset));
        }

        // ------------------------------------------------------------------
        // The layout
        // ------------------------------------------------------------------

        [Test]
        public void Layout_TheCanalsAreTheFieldsCanals_LandingsOnDryBankInTheReedFields()
        {
            RioRaftCanal[] canals = OverworldLayouts.RioRaftCanals;
            Assert.AreEqual(2, canals.Length);
            Assert.AreSame(OverworldLayouts.CanalSekhetSul, canals[0].Water);
            Assert.AreSame(OverworldLayouts.CanalSekhetNorte, canals[1].Water);
            foreach (RioRaftCanal canal in canals)
            {
                foreach (Vector2 landing in new[] { canal.WestLanding, canal.EastLanding })
                {
                    OverworldZone zone = OverworldRules.ZoneAt(World, landing);
                    Assert.IsNotNull(zone, canal.Id);
                    Assert.AreEqual(OverworldLayouts.CamposJuncosMapId, zone.MapId ?? zone.OwnerMapId, canal.Id + " landing outside the Campos de Juncos");
                    Assert.IsFalse(MapLayoutRules.InRiver(World.World, landing, 1f), canal.Id + " landing in the water");
                    Assert.IsTrue(RioRaftMechanicsDriver.FieldsBounds.Contains(landing), canal.Id);
                }

                foreach (Vector2 p in canal.Water)
                {
                    Assert.IsTrue(MapLayoutRules.InRiver(World.World, p, 0f), canal.Id + " course leaves the canal at " + p);
                }
            }
        }

        [Test]
        public void Layout_EightClumps_OnTheBanks_NotInTheWater_ApartFromEachOther()
        {
            Vector2[] clumps = OverworldLayouts.RioReedClumps;
            Assert.AreEqual(8, clumps.Length);
            for (int i = 0; i < clumps.Length; i++)
            {
                Vector2 c = clumps[i];
                Assert.IsFalse(MapLayoutRules.InRiver(World.World, c, 1f), "clump " + i + " in the water");
                Assert.IsTrue(RioRaftMechanicsDriver.FieldsBounds.Contains(c), "clump " + i);
                Assert.AreEqual(i, RioRaftRules.ReedAt(clumps, c));
                Assert.AreEqual(-1, RioRaftRules.LandingAt(OverworldLayouts.RioRaftCanals, c), "clump " + i + " on a landing");
                for (int j = i + 1; j < clumps.Length; j++)
                {
                    Assert.Greater(Vector2.Distance(c, clumps[j]), RioRaftRules.ReedReach * 2f, "clumps " + i + "/" + j);
                }

                float nearest = float.MaxValue;
                foreach (RioRaftCanal canal in OverworldLayouts.RioRaftCanals)
                {
                    for (int k = 0; k + 1 < canal.Water.Length; k++)
                    {
                        nearest = Mathf.Min(nearest, MapLayoutRules.DistanceToSegment(c, canal.Water[k], canal.Water[k + 1]));
                    }
                }

                Assert.Less(nearest, 10f, "clump " + i + " is not on a canal bank");
            }
        }
    }
}
