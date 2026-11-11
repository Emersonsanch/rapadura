using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The four mechanics on the road every player walks in Regions 1 and 4 (Docs/Mapas100.md): the mud of the Brejo do Lodo Escuro (03,
    /// <see cref="ZarafshanMireRules"/>), the webs of the Bosque das Amoreiras (04, <see cref="ZarafshanWebRules"/>), the boat of the Lago do Limiar
    /// (19, <see cref="LimiarBoatRules"/>) and the fear of the dark of the Salão de Tages (20, <see cref="TagesDarkRules"/>): pure rules, and the
    /// places the layouts declare for them on good ground.
    /// </summary>
    public class EarlyRouteMechanicsTests
    {
        private const long T = 1_700_000_000L;

        // ------------------------------------------------------------------
        // 03 · Brejo do Lodo Escuro
        // ------------------------------------------------------------------

        [Test]
        public void Mire_SlowsOffTheWalkways_NotOnThePostOrInACleanPool()
        {
            Assert.AreEqual(0.7f, ZarafshanMireRules.SpeedFactor(true, false, false, false), 1e-6f);
            Assert.AreEqual(1f, ZarafshanMireRules.SpeedFactor(true, true, false, false), "on a walkway");
            Assert.AreEqual(1f, ZarafshanMireRules.SpeedFactor(true, false, true, false), "on the post's safe ground");
            Assert.AreEqual(1f, ZarafshanMireRules.SpeedFactor(true, false, false, true), "in a clean pool");
            Assert.AreEqual(1f, ZarafshanMireRules.SpeedFactor(false, false, false, false), "outside the marsh");
            Assert.AreEqual(-0.3f, ZarafshanMireRules.MoveBonus, 1e-6f);

            var paths = new List<MapPath> { new MapPath(Vector2.zero, new Vector2(20f, 0f), 3f) };
            var bridges = new List<Vector2> { new Vector2(0f, -30f) };
            Assert.IsTrue(ZarafshanMireRules.OnWalkway(paths, bridges, new Vector2(10f, 2f)), "half the width + 0.5 m");
            Assert.IsFalse(ZarafshanMireRules.OnWalkway(paths, bridges, new Vector2(10f, 2.1f)));
            Assert.IsTrue(ZarafshanMireRules.OnWalkway(paths, bridges, new Vector2(4f, -30f)), "by a bridge");
            Assert.IsFalse(ZarafshanMireRules.OnWalkway(paths, bridges, new Vector2(6f, -30f)));
        }

        [Test]
        public void Mire_JoaquimsLivingWaterCleansFor20Seconds_SixPoolsAtMost()
        {
            Assert.IsTrue(ZarafshanMireRules.Cleans(CharacterId.Joaquim));
            Assert.IsFalse(ZarafshanMireRules.Cleans(CharacterId.Maria));
            Assert.IsFalse(ZarafshanMireRules.Cleans(null));

            var pools = new List<ZarafshanCleanPool>();
            ZarafshanMireRules.AddPool(pools, Vector2.zero, 100f);
            Assert.IsTrue(ZarafshanMireRules.Cleaned(pools, new Vector2(5.9f, 0f), 119.9f));
            Assert.IsFalse(ZarafshanMireRules.Cleaned(pools, new Vector2(6.1f, 0f), 101f), "6 m round the point");
            Assert.IsFalse(ZarafshanMireRules.Cleaned(pools, Vector2.zero, 120f), "dried after 20 s");

            for (int i = 0; i < 8; i++)
            {
                ZarafshanMireRules.AddPool(pools, new Vector2(i * 20f, 0f), 101f + i);
            }

            Assert.AreEqual(ZarafshanMireRules.MaxPools, pools.Count);
            Assert.IsFalse(ZarafshanMireRules.Cleaned(pools, Vector2.zero, 110f), "the oldest pool went first");
            Assert.IsTrue(ZarafshanMireRules.Cleaned(pools, new Vector2(140f, 0f), 110f));
        }

        [Test]
        public void Mire_IsMap03_ItsTrailsAreWalkways_ItsClearingsAreMud()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.ZarafshanId);
            Rect mire = OverworldLayouts.BrejoMireBounds;
            Assert.AreEqual(MapLayouts.BrejoId, OverworldRules.ZoneAt(world, mire.center)?.MapId);
            var bridges = new List<Vector2>();
            foreach (MapRiver river in world.World.Rivers)
            {
                foreach (MapCrossing crossing in river.Crossings)
                {
                    bridges.Add(crossing.At);
                }
            }

            Vector2 o = OverworldLayouts.BrejoOffset;
            Assert.IsTrue(ZarafshanMireRules.OnWalkway(world.World.Paths, bridges, o + new Vector2(20f, -60f)), "the log walkway south from the post");
            Assert.IsTrue(ZarafshanMireRules.OnWalkway(world.World.Paths, bridges, o + new Vector2(20f, -39.2f)), "the bridge over the north channel");
            Assert.IsFalse(ZarafshanMireRules.OnWalkway(world.World.Paths, bridges, o + new Vector2(-82f, -65f)), "the west mud clearing");
            Assert.IsFalse(ZarafshanMireRules.OnWalkway(world.World.Paths, bridges, o + new Vector2(55f, -15f)), "the north reed bed");
            Assert.IsNotNull(OverworldRules.SettlementAt(world, o), "the Ponte do Aryk keeps its ground dry");
        }

        // ------------------------------------------------------------------
        // 04 · Bosque das Amoreiras de Seda
        // ------------------------------------------------------------------

        [Test]
        public void Webs_HoldOnceThenRespin_ASkillCutsThem()
        {
            var web = new ZarafshanWeb("w", Vector2.zero, new Vector2(8.5f, 0f));
            var webs = new[] { web };
            Assert.AreEqual(0, ZarafshanWebRules.CatchAt(webs, new[] { 0L }, new Vector2(4f, 0.6f), T));
            Assert.AreEqual(-1, ZarafshanWebRules.CatchAt(webs, new[] { 0L }, new Vector2(4f, 0.8f), T), "0.7 m of the thread");
            Assert.AreEqual(-1, ZarafshanWebRules.CatchAt(webs, new[] { T }, new Vector2(4f, 0f), T + 44L), "torn: no second hold");
            Assert.AreEqual(0, ZarafshanWebRules.CatchAt(webs, new[] { T }, new Vector2(4f, 0f), T + 45L), "spun again after 45 s");
            Assert.AreEqual(1.5f, ZarafshanWebRules.HoldSeconds);

            Assert.IsTrue(ZarafshanWebRules.Cuts(web, new Vector2(4f, 4.9f), 3f, new Vector2(4f, -20f)), "area 3 m + 2 m");
            Assert.IsFalse(ZarafshanWebRules.Cuts(web, new Vector2(4f, 5.1f), 3f, new Vector2(4f, -20f)));
            Assert.IsTrue(ZarafshanWebRules.Cuts(web, new Vector2(40f, 40f), 0f, new Vector2(4f, -2.9f)), "cast from beside it");
            Assert.AreEqual("zarafshan.web.so_fileiras", ZarafshanWebRules.BrokenKey("so_fileiras"));
        }

        [Test]
        public void Webs_InMap04_OffTrailsYardsAndTheDryPost()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.ZarafshanId);
            ZarafshanWeb[] webs = OverworldLayouts.AmoreirasWebs;
            Assert.AreEqual(8, webs.Length);
            var ids = new HashSet<string>();
            foreach (ZarafshanWeb web in webs)
            {
                Assert.IsTrue(ids.Add(web.Id), web.Id);
                Assert.AreEqual(8.5f, Vector2.Distance(web.A, web.B), 1e-3f, web.Id);
                foreach (Vector2 p in new[] { web.A, web.B, (web.A + web.B) * 0.5f })
                {
                    Assert.IsTrue(OverworldLayouts.AmoreirasGroveBounds.Contains(p), web.Id);
                    Assert.AreEqual(MapLayouts.AmoreirasId, OverworldRules.ZoneAt(world, p)?.MapId, web.Id + " outside map 04");
                    Assert.IsFalse(MapLayoutRules.OnPath(world.World, p, 10f), web.Id + " across a trail");
                    Assert.IsFalse(MapLayoutRules.InRiver(world.World, p, 2f), web.Id + " in the water");
                    Assert.IsNull(OverworldRules.SettlementAt(world, p), web.Id + " on the post's ground");
                }
            }
        }

        // ------------------------------------------------------------------
        // 19 · Lago do Limiar
        // ------------------------------------------------------------------

        [Test]
        public void Boat_SailsFromEitherLanding_AfterTheRest_ShorterThanTheShore()
        {
            Vector2[] course = OverworldLayouts.LimiarBoatCourse;
            Assert.AreEqual(0, LimiarBoatRules.LandingAt(course, course[0] + new Vector2(3.9f, 0f)));
            Assert.AreEqual(1, LimiarBoatRules.LandingAt(course, course[course.Length - 1]));
            Assert.AreEqual(-1, LimiarBoatRules.LandingAt(course, course[0] + new Vector2(4.1f, 0f)));
            Assert.IsFalse(LimiarBoatRules.CanSail(0, 2.9f));
            Assert.IsTrue(LimiarBoatRules.CanSail(1, 3f));
            Assert.IsFalse(LimiarBoatRules.CanSail(-1, 99f));

            Vector2[] back = LimiarBoatRules.Course(course, 1);
            Assert.AreEqual(course[course.Length - 1], back[0]);
            Assert.AreEqual(course[0], back[back.Length - 1]);
            float length = RidePath.Length(course);
            Assert.AreEqual(43f, length, 1.5f);
            Assert.AreEqual(7.2f, LimiarBoatRules.RideSeconds(course), 0.3f);
        }

        [Test]
        public void Boat_LandingsOnDryGroundOffTheTrails_TheMiddleOverTheLake()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.LimiarId);
            Vector2[] course = OverworldLayouts.LimiarBoatCourse;
            foreach (Vector2 landing in new[] { course[0], course[course.Length - 1] })
            {
                Assert.IsFalse(MapLayoutRules.InRiver(world.World, landing, 2f), landing + " in the water");
                Assert.IsTrue(MapLayoutRules.InRiver(world.World, landing, 8f), landing + " too far from the shore");
                Assert.IsFalse(MapLayoutRules.OnPath(world.World, landing, LimiarBoatRules.LandingReach), landing + " on a trail");
                Assert.IsNull(OverworldRules.SettlementAt(world, landing), landing + " in the Vila");
                Assert.AreEqual(MapLayouts.LagoLimiarId, OverworldRules.ZoneAt(world, landing)?.MapId);
                Assert.IsTrue(OverworldLayouts.LimiarLakeBounds.Contains(landing));
            }

            for (int i = 1; i < course.Length - 1; i++)
            {
                Assert.IsTrue(MapLayoutRules.InRiver(world.World, course[i], 0f), course[i] + " is not over the lake");
            }
        }

        // ------------------------------------------------------------------
        // 20 · Salão de Tages
        // ------------------------------------------------------------------

        [Test]
        public void Dark_LampsBurnFiveMinutes_BeastsBoldInTheDark_CalmInTheLight()
        {
            Assert.IsFalse(TagesDarkRules.Burning(0L, T), "a lamp never lit is out");
            Assert.IsTrue(TagesDarkRules.Burning(T, T + 299L));
            Assert.IsFalse(TagesDarkRules.Burning(T, T + 300L), "the hall's sigh blows it out");
            Assert.AreEqual(1L, TagesDarkRules.SecondsLeft(T, T + 299L));
            Assert.IsTrue(TagesDarkRules.CanLight(0L, T));
            Assert.IsFalse(TagesDarkRules.CanLight(T, T + 10L));
            Assert.AreEqual("limiar.tages.lamp.colunata", TagesDarkRules.LitKey("colunata"));

            var lamps = new[] { new TagesLamp("a", Vector2.zero), new TagesLamp("b", new Vector2(30f, 0f)) };
            var lit = new[] { T, 0L };
            Assert.AreEqual(TagesBeastMood.Calm, TagesDarkRules.MoodAt(lamps, lit, new Vector2(10f, 0f), T + 1L));
            Assert.AreEqual(TagesBeastMood.Bold, TagesDarkRules.MoodAt(lamps, lit, new Vector2(20f, 0f), T + 1L), "the nearest lamp decides");
            Assert.AreEqual(TagesBeastMood.None, TagesDarkRules.MoodAt(lamps, lit, new Vector2(15f, 17f), T + 1L), "16 m of a lamp");
            Assert.AreEqual(1.25f, TagesDarkRules.DamageFactor(TagesBeastMood.Bold));
            Assert.AreEqual(0.85f, TagesDarkRules.DamageFactor(TagesBeastMood.Calm));
            Assert.AreEqual(1f, TagesDarkRules.DamageFactor(TagesBeastMood.None));
            Assert.AreEqual(1.15f, TagesDarkRules.SpeedFactor(TagesBeastMood.Bold));
            Assert.AreEqual(1f, TagesDarkRules.SpeedFactor(TagesBeastMood.Calm));
            Assert.AreEqual(1, TagesDarkRules.BurningCount(lit, T + 1L));

            Assert.AreEqual(0, TagesDarkRules.LampAt(lamps, new Vector2(2.9f, 0f), TagesDarkRules.ReachFor(false)));
            Assert.AreEqual(-1, TagesDarkRules.LampAt(lamps, new Vector2(5f, 0f), TagesDarkRules.ReachFor(false)));
            Assert.AreEqual(0, TagesDarkRules.LampAt(lamps, new Vector2(5f, 0f), TagesDarkRules.ReachFor(true)), "Maria lights from 6 m");
        }

        [Test]
        public void Dark_SixLampsInMap20_OffTrailsAndWater_EachClearingHasOne()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.LimiarId);
            TagesLamp[] lamps = OverworldLayouts.TagesLamps;
            Assert.AreEqual(6, lamps.Length);
            for (int i = 0; i < lamps.Length; i++)
            {
                Vector2 p = lamps[i].At;
                Assert.AreEqual(MapLayouts.TagesId, OverworldRules.ZoneAt(world, p)?.MapId, lamps[i].Id + " outside map 20");
                Assert.IsTrue(OverworldLayouts.TagesHallBounds.Contains(p), lamps[i].Id);
                Assert.IsFalse(MapLayoutRules.OnPath(world.World, p, 0f), lamps[i].Id + " on a trail");
                Assert.IsFalse(MapLayoutRules.InRiver(world.World, p, 2f), lamps[i].Id + " in the water");
                Assert.IsNull(OverworldRules.SettlementAt(world, p), lamps[i].Id + " on the post's ground");
                Assert.AreEqual(i, TagesDarkRules.LampAt(lamps, p, TagesDarkRules.LightBearerReach), lamps[i].Id + " too close to another lamp");
            }

            Vector2 o = OverworldLayouts.TagesOffset;
            foreach (Vector2 clearing in new[] { new Vector2(-50f, -55f), new Vector2(68f, -66f), new Vector2(84f, 62f) })
            {
                Assert.GreaterOrEqual(TagesDarkRules.LampAt(lamps, o + clearing, TagesDarkRules.BeastReach), 0, "no lamp by the clearing " + clearing);
            }

            foreach (LimiarLamp lamp in OverworldLayouts.LimiarLamps)
            {
                Assert.Less(TagesDarkRules.LampAt(lamps, lamp.Position, TagesDarkRules.LightBearerReach + 4f), 0, lamp.Id + " shares a niche with a hall lamp");
            }
        }
    }
}
