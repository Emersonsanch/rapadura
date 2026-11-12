using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The five maps of the road that had no mechanic of their own (Docs/Mapas100.md): the sun shadows of Meroë (27, <see cref="SunShadowRules"/>),
    /// the flooded houses of Kilwa (29, <see cref="FloodedHouseRules"/>), the dhows of Liyongo (30, <see cref="DhowRules"/>), the forgotten root
    /// bridges (32, <see cref="RootBridgeRules"/>) and the escort of Ammo's caravan (40, <see cref="CaravanEscortRules"/>): pure rules, and the
    /// places the layouts declare for them.
    /// </summary>
    public class CoastDeltaPassMechanicsTests
    {
        // ------------------------------------------------------------------
        // 27 · Pirâmides Altas de Meroé
        // ------------------------------------------------------------------

        [Test]
        public void Shadow_SwingsWestNorthEast_ShortestAtNoon()
        {
            Vector2 apex = Vector2.zero;
            Vector2 noon = SunShadowRules.TipAt(apex, 12f);
            Assert.AreEqual(0f, noon.x, 1e-3f);
            Assert.AreEqual(SunShadowRules.NoonLength, noon.y, 1e-3f, "north at noon, 18 m");
            Assert.Less(SunShadowRules.TipAt(apex, 8f).x, 0f, "west in the morning");
            Assert.Greater(SunShadowRules.TipAt(apex, 16f).x, 0f, "east in the evening");
            Assert.Greater(SunShadowRules.TipAt(apex, 8f).magnitude, noon.magnitude, "longer away from noon");

            Assert.IsTrue(SunShadowRules.Shining(6.5f));
            Assert.IsFalse(SunShadowRules.Shining(6.4f));
            Assert.IsFalse(SunShadowRules.Shining(20f), "no shadow at night");
        }

        [Test]
        public void Shadow_OpensTheChapelOnlyAtItsHour_OnceADay()
        {
            Vector2 apex = OverworldLayouts.MeroePyramids[1];
            Vector2 door = SunShadowRules.ChapelDoor(apex, 1);
            Assert.IsTrue(SunShadowRules.OnDoor(apex, 1, 12f));
            Assert.IsTrue(SunShadowRules.OnDoor(apex, 1, 12.3f), "a window round the hour");
            Assert.IsFalse(SunShadowRules.OnDoor(apex, 1, 13.5f));
            Assert.IsTrue(SunShadowRules.OnTip(apex, 12f, door + new Vector2(2f, 0f)));
            Assert.IsFalse(SunShadowRules.OnTip(apex, 12f, door + new Vector2(3f, 0f)));

            Assert.IsTrue(SunShadowRules.Opens(3f, 0, 1));
            Assert.IsFalse(SunShadowRules.Opens(2.9f, 0, 1));
            Assert.IsFalse(SunShadowRules.Opens(3f, 4, 4), "opened today");
            Assert.AreEqual(1, SunShadowRules.NearestPyramid(OverworldLayouts.MeroePyramids, door, 3f));
            Assert.AreSame(SunShadowRules.ChapelKey(2), SunShadowRules.ChapelKey(2));
        }

        [Test]
        public void Shadow_PyramidsAndChapelsLieInMeroe_OffThePatio()
        {
            Vector2[] apexes = OverworldLayouts.MeroePyramids;
            Assert.AreEqual(SunShadowRules.ChapelHours.Length, apexes.Length);
            for (int i = 0; i < apexes.Length; i++)
            {
                Vector2 door = SunShadowRules.ChapelDoor(apexes[i], i);
                Assert.IsTrue(OverworldLayouts.MeroeShadowBounds.Contains(apexes[i]) && OverworldLayouts.MeroeShadowBounds.Contains(door), $"pyramid {i}");
                Assert.Greater(Vector2.Distance(door, apexes[i]), OverworldLayouts.MeroePyramidHalfBase * 1.42f, "the door lies outside the base");
                Assert.Greater(Vector2.Distance(apexes[i], OverworldLayouts.PatioRelevosOffset), 40f, "away from the post");
            }
        }

        // ------------------------------------------------------------------
        // 29 · Kilwa Afundada
        // ------------------------------------------------------------------

        [Test]
        public void House_TheLintelShowsThreeDifferentStones_ChangingWithTheDay()
        {
            var codes = new HashSet<string>();
            for (int day = 1; day <= 24; day++)
            {
                int a = FloodedHouseRules.CodeStone(0, day, 0), b = FloodedHouseRules.CodeStone(0, day, 1), c = FloodedHouseRules.CodeStone(0, day, 2);
                Assert.That(new[] { a, b, c }, Is.Unique, $"day {day}");
                Assert.That(new[] { a, b, c }, Is.All.InRange(0, 3));
                codes.Add($"{a}{b}{c}");
            }

            Assert.Greater(codes.Count, 6, "the code changes over the days");
            Assert.AreNotEqual(FloodedHouseRules.CodeStone(0, 3, 0) * 100 + FloodedHouseRules.CodeStone(0, 3, 1) * 10 + FloodedHouseRules.CodeStone(0, 3, 2),
                FloodedHouseRules.CodeStone(1, 3, 0) * 100 + FloodedHouseRules.CodeStone(1, 3, 1) * 10 + FloodedHouseRules.CodeStone(1, 3, 2), "each house its own");
        }

        [Test]
        public void House_RightOrderOpens_WrongStoneStartsAgain()
        {
            const int house = 2, day = 7;
            int p = 0;
            p = FloodedHouseRules.Press(p, FloodedHouseRules.CodeStone(house, day, 0), house, day);
            Assert.AreEqual(1, p);
            p = FloodedHouseRules.Press(p, FloodedHouseRules.CodeStone(house, day, 1), house, day);
            Assert.AreEqual(2, p);
            int wrong = FloodedHouseRules.CodeStone(house, day, 0);
            Assert.AreEqual(0, FloodedHouseRules.Press(p, wrong, house, day), "a wrong stone");
            p = FloodedHouseRules.Press(p, FloodedHouseRules.CodeStone(house, day, 2), house, day);
            Assert.IsTrue(FloodedHouseRules.Opened(p));

            Vector2 door = OverworldLayouts.KilwaHouseDoors[0];
            Assert.AreEqual(2, FloodedHouseRules.StoneNear(door, door + new Vector2(0f, -3f)), "the moon stone stands south");
            Assert.AreEqual(-1, FloodedHouseRules.StoneNear(door, door));
            foreach (Vector2 d in OverworldLayouts.KilwaHouseDoors)
            {
                Assert.IsTrue(OverworldLayouts.KilwaHousesBounds.Contains(d), $"{d}");
            }
        }

        // ------------------------------------------------------------------
        // 30 · Mangues de Liyongo
        // ------------------------------------------------------------------

        [Test]
        public void Dhow_TwoGold_NoSailingAtLowTide()
        {
            Assert.IsTrue(DhowRules.CanSail(0, CostaTidePhase.High, 10f, 2));
            Assert.IsTrue(DhowRules.CanSail(1, CostaTidePhase.Turning, 10f, 5));
            Assert.IsFalse(DhowRules.CanSail(0, CostaTidePhase.Low, 10f, 5), "the canal runs dry");
            Assert.IsFalse(DhowRules.CanSail(0, CostaTidePhase.High, 10f, 1), "the fare");
            Assert.IsFalse(DhowRules.CanSail(-1, CostaTidePhase.High, 10f, 5));
            Assert.IsFalse(DhowRules.CanSail(0, CostaTidePhase.High, 1f, 5), "a moment between rides");

            Vector2[] course = OverworldLayouts.LiyongoDhowCourse;
            float length = RidePath.Length(course);
            Assert.That(length, Is.InRange(650f, 800f));
            Assert.That(length / DhowRules.Speed, Is.InRange(45f, 60f), "about 52 s down the canal");
            foreach (Vector2 p in course)
            {
                Assert.IsTrue(OverworldLayouts.LiyongoDhowBounds.Contains(p), $"{p}");
            }
        }

        // ------------------------------------------------------------------
        // 32 · Pontes de Raiz Viva
        // ------------------------------------------------------------------

        [Test]
        public void Bridge_JoaquimOrTheWaterskin_GrowsIn10s_OnlyWhileNear()
        {
            Assert.IsTrue(RootBridgeRules.CanWater(CharacterId.Joaquim, false));
            Assert.IsTrue(RootBridgeRules.CanWater(CharacterId.Maria, true));
            Assert.IsFalse(RootBridgeRules.CanWater(CharacterId.Maria, false));

            float g = 0f;
            for (int i = 0; i < 9; i++)
            {
                g = RootBridgeRules.Grow(g, true, 1f);
            }

            Assert.AreEqual(0.9f, g, 1e-4f);
            Assert.AreEqual(1f, RootBridgeRules.Grow(g, true, 1f), 1e-4f, "10 s");
            Assert.AreEqual(0f, RootBridgeRules.Grow(0.9f, false, 0.1f), "walking off starts again");

            Assert.IsTrue(RootBridgeRules.Grown(100f, 699f));
            Assert.IsFalse(RootBridgeRules.Grown(100f, 700f), "ten minutes");
            Assert.IsFalse(RootBridgeRules.Grown(float.NegativeInfinity, 0f));
            Assert.IsTrue(RootBridgeRules.ChestOpens(true, 0, 1));
            Assert.IsFalse(RootBridgeRules.ChestOpens(false, 0, 1));
            Assert.IsFalse(RootBridgeRules.ChestOpens(true, 1, 1));
        }

        [Test]
        public void Bridge_TheForgottenBridgesLieOnTheGorges_TheirChestsAcross()
        {
            Assert.AreEqual(OverworldLayouts.RootBridges.Length, OverworldLayouts.RootBridgeChests.Length);
            for (int i = 0; i < OverworldLayouts.RootBridges.Length; i++)
            {
                Vector2 b = OverworldLayouts.RootBridges[i];
                Assert.IsTrue(OverworldLayouts.RootBridgeBounds.Contains(b), $"{b}");
                float d = Vector2.Distance(b, OverworldLayouts.RootBridgeChests[i]);
                Assert.That(d, Is.InRange(10f, 20f), "the chest on the far bank");
                Assert.Greater(Vector2.Distance(OverworldLayouts.RootBridgeChests[i], b), RootBridgeRules.WaterReach, "watering and opening are two places");
            }
        }

        // ------------------------------------------------------------------
        // 40 · Passo de Wakhan
        // ------------------------------------------------------------------

        [Test]
        public void Caravan_WalksWithTheHero_StopsAtBendsAndSlopes()
        {
            Assert.IsTrue(CaravanEscortRules.Walks(40f, false));
            Assert.IsFalse(CaravanEscortRules.Walks(40.1f, false), "it waits for the hero");
            Assert.IsFalse(CaravanEscortRules.Walks(5f, true));

            Assert.AreEqual(0, CaravanEscortRules.StopCrossed(CaravanEscortRules.AmbushAt, 0.29f, 0.31f));
            Assert.AreEqual(-1, CaravanEscortRules.StopCrossed(CaravanEscortRules.AmbushAt, 0.31f, 0.4f));
            Assert.AreEqual(1, CaravanEscortRules.StopCrossed(CaravanEscortRules.AvalancheAt, 0.6f, 0.7f));

            Assert.IsTrue(CaravanEscortRules.AmbushOver(0, 1f));
            Assert.IsTrue(CaravanEscortRules.AmbushOver(3, 45f), "they give up");
            Assert.IsFalse(CaravanEscortRules.AmbushOver(1, 10f));

            Assert.AreEqual(8f, CaravanEscortRules.AvalancheHurt(5f, 100f, 100f), 1e-4f);
            Assert.AreEqual(0f, CaravanEscortRules.AvalancheHurt(9f, 100f, 100f), "out of the snow");
            Assert.AreEqual(2f, CaravanEscortRules.AvalancheHurt(5f, 32f, 100f), 1e-4f, "never under 30 %");
            Assert.IsTrue(CaravanEscortRules.PassOpen(1f));
            Assert.IsFalse(CaravanEscortRules.PassOpen(0.2f), "closed at night");
        }

        [Test]
        public void Caravan_TheRoadRunsThroughThePass_InAFewMinutes()
        {
            Vector2[] road = OverworldLayouts.WakhanCaravanRoad;
            foreach (Vector2 p in road)
            {
                Assert.IsTrue(OverworldLayouts.InWakhanEscort(p), $"{p}");
            }

            float minutes = RidePath.Length(road) / CaravanEscortRules.Speed / 60f;
            Assert.That(minutes, Is.InRange(4f, 8f), "a few minutes of walking, more with the stops");
            Assert.Less(Vector2.Distance(road[2], OverworldLayouts.CaravancaraPedraOffset), 25f, "through the Caravançará de Pedra");
        }
    }
}
