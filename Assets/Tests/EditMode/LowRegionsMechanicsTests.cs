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
    /// <summary>
    /// The mechanics of Regions 2–4 (Docs/Mapas100.md 07, 13–14, 19): the sandstorm (<see cref="AreiasStormRules"/>), the winds and rope
    /// bridges (<see cref="KaggenWindRules"/>), the lamps (<see cref="LimiarLampRules"/>, <see cref="LimiarLampProgress"/>), and the places
    /// the layouts declare for them.
    /// </summary>
    public class LowRegionsMechanicsTests
    {
        // ------------------------------------------------------------------
        // Sandstorm
        // ------------------------------------------------------------------

        [TestCase(0d, AreiasStormPhase.Calm)]
        [TestCase(544.9d, AreiasStormPhase.Calm)]
        [TestCase(545d, AreiasStormPhase.Warning)]
        [TestCase(559.9d, AreiasStormPhase.Warning)]
        [TestCase(560d, AreiasStormPhase.Storm)]
        [TestCase(599.9d, AreiasStormPhase.Storm)]
        [TestCase(600d, AreiasStormPhase.Calm)]
        [TestCase(1160d, AreiasStormPhase.Storm)]
        public void Storm_Phase_FollowsTheTenMinuteCycle(double time, AreiasStormPhase expected)
        {
            Assert.AreEqual(expected, AreiasStormRules.PhaseAt(time));
        }

        [Test]
        public void Storm_Index_ChangesEachCycle()
        {
            Assert.AreEqual(0L, AreiasStormRules.StormIndex(599d));
            Assert.AreEqual(1L, AreiasStormRules.StormIndex(600d));
        }

        [Test]
        public void Storm_Wear_StopsAtTheFloor_NeverKills()
        {
            Assert.AreEqual(1.5f, AreiasStormRules.WearOver(100f, 100f, 1f), 1e-4f);
            Assert.AreEqual(1f, AreiasStormRules.WearOver(36f, 100f, 1f), 1e-4f, "only what is above the floor");
            Assert.AreEqual(0f, AreiasStormRules.WearOver(35f, 100f, 1f), 1e-4f);
            Assert.AreEqual(0f, AreiasStormRules.WearOver(10f, 100f, 1f), 1e-4f, "already under the floor: nothing more");

            float health = 100f;
            for (int i = 0; i < 1000; i++)
            {
                health -= AreiasStormRules.WearOver(health, 100f, 1f);
            }

            Assert.AreEqual(35f, health, 1e-3f);
        }

        [Test]
        public void Storm_Shelter_CoversTheRadius()
        {
            var shelters = new[] { new Vector2(10f, 0f), new Vector2(100f, 0f) };
            Assert.IsTrue(AreiasStormRules.IsSheltered(new Vector2(10f + AreiasStormRules.ShelterRadius, 0f), shelters));
            Assert.IsFalse(AreiasStormRules.IsSheltered(new Vector2(50f, 0f), shelters));
            Assert.AreEqual(0, AreiasStormRules.NearestShelter(new Vector2(0f, 0f), shelters, out float distance));
            Assert.AreEqual(10f, distance, 1e-4f);
            Assert.AreEqual(-1, AreiasStormRules.NearestShelter(Vector2.zero, null, out _));
        }

        [Test]
        public void Storm_Layout_SheltersAreInTheRegion_AndNeverFar()
        {
            OverworldLayout w = OverworldLayouts.Get(OverworldLayouts.AreiasId);
            foreach (Vector2 shelter in OverworldLayouts.AreiasStormShelters)
            {
                Assert.IsTrue(OverworldLayouts.AreiasBounds.Contains(shelter), shelter.ToString());
                Assert.IsNotNull(OverworldRules.ZoneAt(w, shelter), shelter.ToString());
            }

            foreach (string zone in OverworldLayouts.AreiasStormFreeZones)
            {
                Assert.IsNotNull(w.Zone(zone), zone);
            }

            Assert.IsNotNull(w.Zone(OverworldLayouts.AreiasAnchorZoneId));

            // The caravan road (nv 5–7) is where a new hero first meets the storm: a shelter is never far.
            float worst = 0f;
            for (float x = -700f; x <= -100f; x += 40f)
            {
                for (float z = 0f; z <= 500f; z += 40f)
                {
                    AreiasStormRules.NearestShelter(new Vector2(x, z), OverworldLayouts.AreiasStormShelters, out float distance);
                    worst = Mathf.Max(worst, distance);
                }
            }

            Assert.Less(worst, 320f);
        }

        // ------------------------------------------------------------------
        // Winds and rope bridges
        // ------------------------------------------------------------------

        [TestCase(0d, KaggenWindPhase.Calm)]
        [TestCase(19.4d, KaggenWindPhase.Calm)]
        [TestCase(19.5d, KaggenWindPhase.Warning)]
        [TestCase(22.4d, KaggenWindPhase.Warning)]
        [TestCase(22.5d, KaggenWindPhase.Blowing)]
        [TestCase(24.9d, KaggenWindPhase.Blowing)]
        [TestCase(25d, KaggenWindPhase.Calm)]
        public void Wind_Phase_FollowsTheCycle(double time, KaggenWindPhase expected)
        {
            Assert.AreEqual(expected, KaggenWindRules.PhaseAt(time));
        }

        [Test]
        public void Wind_Push_StillOrMaitheIsZero()
        {
            Assert.AreEqual(Vector2.zero, KaggenWindRules.PushVelocity(Vector2.right, true, CharacterId.Joaquim));
            Assert.AreEqual(Vector2.zero, KaggenWindRules.PushVelocity(Vector2.right, false, CharacterId.Maithe));
            Assert.AreEqual(KaggenWindRules.PushSpeed, KaggenWindRules.PushVelocity(Vector2.right, false, CharacterId.Lavine).x, 1e-4f);
            Assert.AreEqual(6f, KaggenWindRules.PushSpeed * KaggenWindRules.BlowSeconds, 1e-4f, "metres of a whole gust");
            Assert.AreEqual(Vector2.right, KaggenWindRules.Direction(0));
            Assert.AreEqual(Vector2.left, KaggenWindRules.Direction(1));
        }

        [Test]
        public void Bridge_RunningDrainsExtra_NotWalkingNotMaithe()
        {
            Assert.AreEqual(0.5f, KaggenWindRules.ExtraBridgeDrain(true, true, CharacterId.Icaro, 0.5f), 1e-4f);
            Assert.AreEqual(0f, KaggenWindRules.ExtraBridgeDrain(true, false, CharacterId.Icaro, 0.5f), 1e-4f);
            Assert.AreEqual(0f, KaggenWindRules.ExtraBridgeDrain(false, true, CharacterId.Icaro, 0.5f), 1e-4f);
            Assert.AreEqual(0f, KaggenWindRules.ExtraBridgeDrain(true, true, CharacterId.Maithe, 0.5f), 1e-4f);
        }

        [Test]
        public void Wind_Layout_BridgesAreRealCrossings_AreasInBounds()
        {
            OverworldLayout w = OverworldLayouts.Get(OverworldLayouts.KaggenId);
            var crossings = new List<Vector2>();
            foreach (MapRiver river in w.World.Rivers)
            {
                foreach (MapCrossing crossing in river.Crossings)
                {
                    if (crossing.IsBridge)
                    {
                        crossings.Add(crossing.At);
                    }
                }
            }

            foreach (Vector2 bridge in OverworldLayouts.KaggenBridges)
            {
                Assert.IsTrue(crossings.Exists(c => Vector2.Distance(c, bridge) < 2f), "no crossing at " + bridge);
                Assert.IsTrue(KaggenWindRules.OnBridge(bridge, OverworldLayouts.KaggenBridges));
            }

            foreach (Rect area in OverworldLayouts.KaggenWindAreas)
            {
                Assert.IsTrue(OverworldLayouts.KaggenBounds.Contains(area.min) && OverworldLayouts.KaggenBounds.Contains(area.max - Vector2.one), area.ToString());
            }

            Assert.IsNotNull(w.Zone(OverworldLayouts.KaggenAnchorZoneId));
        }

        // ------------------------------------------------------------------
        // Lamps
        // ------------------------------------------------------------------

        private static readonly LimiarLamp[] TestLamps =
        {
            new LimiarLamp("a", Vector2.zero), new LimiarLamp("b", new Vector2(100f, 0f)), new LimiarLamp("c", new Vector2(200f, 0f)),
        };

        [Test]
        public void Lamp_Reach_FindsTheClosestInRange()
        {
            Assert.AreEqual(1, LimiarLampRules.LampAt(new Vector2(99f, 0f), TestLamps));
            Assert.AreEqual(-1, LimiarLampRules.LampAt(new Vector2(50f, 0f), TestLamps));
        }

        [Test]
        public void Lamp_Actions_LightThenTravelToTheDeepest()
        {
            var lit = new HashSet<string>();
            Assert.IsTrue(LimiarLampRules.IsLit(LimiarLampRules.VilaLamp, TestLamps, lit), "the Vila's is always lit");
            Assert.AreEqual(LimiarLampAction.None, LimiarLampRules.ActionAt(0, TestLamps, lit), "nothing to go to yet");
            Assert.AreEqual(LimiarLampAction.Light, LimiarLampRules.ActionAt(1, TestLamps, lit));

            lit.Add("b");
            Assert.AreEqual(LimiarLampAction.ReturnToVila, LimiarLampRules.ActionAt(1, TestLamps, lit));
            Assert.AreEqual(0, LimiarLampRules.TravelTarget(1, TestLamps, lit));
            Assert.AreEqual(1, LimiarLampRules.TravelTarget(0, TestLamps, lit));

            lit.Add("c");
            Assert.AreEqual(2, LimiarLampRules.DeepestLit(TestLamps, lit));
            Assert.AreEqual(2, LimiarLampRules.TravelTarget(0, TestLamps, lit));
            Assert.AreEqual(-1, LimiarLampRules.TravelTarget(-1, TestLamps, lit));
        }

        [Test]
        public void Lamp_Travel_RestsBetweenTrips()
        {
            Assert.IsTrue(LimiarLampRules.CanTravel(1000f, float.NegativeInfinity));
            Assert.IsFalse(LimiarLampRules.CanTravel(19f, 0f));
            Assert.IsTrue(LimiarLampRules.CanTravel(20f, 0f));
        }

        [Test]
        public void Lamp_Progress_RoundTripsThroughSave_OldSaveIsEmpty()
        {
            var progress = new LimiarLampProgress();
            Assert.IsTrue(progress.Light("b"));
            Assert.IsFalse(progress.Light("b"));

            var other = new LimiarLampProgress();
            other.RestoreState(progress.CaptureState());
            Assert.IsTrue(other.Lit.Contains("b"));

            other.RestoreState(null);
            Assert.AreEqual(0, other.Lit.Count);
        }

        [Test]
        public void Lamp_Layout_OnWalkableGround_NearATrail_InZones()
        {
            OverworldLayout w = OverworldLayouts.Get(OverworldLayouts.LimiarId);
            var ids = new HashSet<string>();
            foreach (LimiarLamp lamp in OverworldLayouts.LimiarLamps)
            {
                Assert.IsTrue(ids.Add(lamp.Id), lamp.Id);
                Assert.IsTrue(OverworldLayouts.LimiarBounds.Contains(lamp.Position), lamp.Id);
                Assert.IsNotNull(OverworldRules.ZoneAt(w, lamp.Position), lamp.Id);
                Assert.IsNull(OverworldRules.SettlementAt(w, lamp.Position), lamp.Id + " would be a lamp inside a town");

                foreach (MapRiver river in w.World.Rivers)
                {
                    Assert.Greater(MapLayoutRules.DistanceToPolyline(lamp.Position, river.Points), river.Width * 0.5f + 1f, lamp.Id + " stands in water or a chasm");
                }

                float nearest = float.MaxValue;
                foreach (MapPath path in w.World.Paths)
                {
                    nearest = Mathf.Min(nearest, MapLayoutRules.DistanceToSegment(lamp.Position, path.A, path.B));
                }

                Assert.Less(nearest, 8f, lamp.Id + " is far from any trail");
            }

            Assert.IsNotNull(w.Zone(OverworldLayouts.LimiarAnchorZoneId));
        }
    }
}
