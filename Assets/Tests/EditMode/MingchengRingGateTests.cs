using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// The ring gates of Mingcheng (Docs/Mapas100.md 41, <see cref="MingchengRingGateMechanicsDriver"/>): the bell's curfew and the drum
    /// (<see cref="MingchengRingGateRules"/>), the postern at the watches, Bögü's toll and safe-conduct, the lantern fence that holds only who comes
    /// from outside, the three gates on the city's roads and their texts.
    /// </summary>
    public class MingchengRingGateTests
    {
        private static readonly Vector2 Center = OverworldLayouts.MingchengRingCenter;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            MapLayouts.ClearCache();
            OverworldLayouts.ClearCache();
        }

        private static int FestaDay()
        {
            for (int day = 1; day < 40; day++)
            {
                if (MingchengLanternFestaRules.IsFestaDay(day) && !MingchengLanternFestaRules.IsFestaDay(day - 1))
                {
                    return day;
                }
            }

            Assert.Fail("no festa day");
            return 0;
        }

        private static int PlainDay()
        {
            for (int day = 2; day < 40; day++)
            {
                if (!MingchengLanternFestaRules.IsFestaDay(day) && !MingchengLanternFestaRules.IsFestaDay(day - 1))
                {
                    return day;
                }
            }

            Assert.Fail("no plain day");
            return 0;
        }

        [Test]
        public void Curfew_TheBellShutsAt20_TheDrumOpensAt6_TheFestaLiftsItUntil1()
        {
            int day = PlainDay();
            Assert.IsFalse(MingchengRingGateRules.Curfew(day, 6f), "the drum");
            Assert.IsFalse(MingchengRingGateRules.Curfew(day, 12f));
            Assert.IsFalse(MingchengRingGateRules.Curfew(day, 19.99f));
            Assert.IsTrue(MingchengRingGateRules.Curfew(day, 20f), "the bell");
            Assert.IsTrue(MingchengRingGateRules.Curfew(day, 23.5f));
            Assert.IsTrue(MingchengRingGateRules.Curfew(day, 5.9f));

            int festa = FestaDay();
            Assert.IsFalse(MingchengRingGateRules.Curfew(festa, 21f), "the lantern nights lift the curfew");
            Assert.IsFalse(MingchengRingGateRules.Curfew(festa + 1, 0.5f), "until 1 h");
            Assert.IsTrue(MingchengRingGateRules.Curfew(festa + 1, 1.5f), "then the bell's night goes on");

            Assert.AreEqual(day, MingchengRingGateRules.NightOf(day, 21f));
            Assert.AreEqual(day - 1, MingchengRingGateRules.NightOf(day, 3f), "before the drum it is still yesterday's night");
        }

        [Test]
        public void Watches_ThePosternOpensEveryTwoHours_NoWaitLongerThanTwoHours()
        {
            Assert.AreEqual(22, MingchengRingGateRules.OpenWatch(22.2f));
            Assert.AreEqual(-1, MingchengRingGateRules.OpenWatch(22.6f));
            Assert.AreEqual(0, MingchengRingGateRules.OpenWatch(0.1f));
            Assert.AreEqual(4, MingchengRingGateRules.OpenWatch(4.4f));
            Assert.AreEqual(-1, MingchengRingGateRules.OpenWatch(20.2f), "the bell itself opens nothing");

            Assert.AreEqual(22, MingchengRingGateRules.NextOpening(20.5f));
            Assert.AreEqual(0, MingchengRingGateRules.NextOpening(23f));
            Assert.AreEqual(2, MingchengRingGateRules.NextOpening(0.6f));
            Assert.AreEqual(6, MingchengRingGateRules.NextOpening(4.6f), "the drum after the last watch");

            int day = PlainDay();
            for (float hour = 20f; hour < 30f; hour += 0.05f)
            {
                float h = hour % 24f;
                if (!MingchengRingGateRules.Curfew(day, h) || MingchengRingGateRules.OpenWatch(h) >= 0)
                {
                    continue;
                }

                float wait = MingchengRingGateRules.Wait(h, MingchengRingGateRules.NextOpening(h));
                Assert.LessOrEqual(wait, 2.001f, "at " + h + " h the wait is " + wait + " h");
            }
        }

        [Test]
        public void Gate_OpenByDay_WatchAndPassFree_TollWithGold_OtherwiseWait()
        {
            const int toll = MingchengRingGateRules.Toll;
            Assert.AreEqual(RingGateCheck.Open, MingchengRingGateRules.Check(false, false, false, 0));
            Assert.AreEqual(RingGateCheck.Pass, MingchengRingGateRules.Check(true, false, true, 0));
            Assert.AreEqual(RingGateCheck.Watch, MingchengRingGateRules.Check(true, true, false, 0), "the watch needs no gold");
            Assert.AreEqual(RingGateCheck.Toll, MingchengRingGateRules.Check(true, false, false, toll));
            Assert.AreEqual(RingGateCheck.Wait, MingchengRingGateRules.Check(true, false, false, toll - 1));

            int night = 41;
            long mark = MingchengRingGateRules.StorePass(night);
            Assert.IsTrue(MingchengRingGateRules.PassValid(mark, night));
            Assert.IsFalse(MingchengRingGateRules.PassValid(mark, night + 1), "the safe-conduct ends with the drum");
            Assert.IsFalse(MingchengRingGateRules.PassValid(0L, 0), "an empty save is no safe-conduct, not even for night 0");
            Assert.IsFalse(MingchengRingGateRules.PassValid(0L, -1));
        }

        [Test]
        public void Fence_HoldsWhoComesFromOutside_NeverWhoGoesOut_ATeleportTakesTheSideAgain()
        {
            float r = MingchengRingGateRules.Radius;
            Vector2 outside = Center + new Vector2(r + 1f, 0f);
            Vector2 stepIn = Center + new Vector2(r - 0.4f, 0f);
            int side = MingchengRingGateRules.NextSide(MingchengRingGateRules.Unknown, outside, outside, Center, true);
            Assert.AreEqual(MingchengRingGateRules.Outside, side);

            side = MingchengRingGateRules.NextSide(side, outside, stepIn, Center, true);
            Assert.AreEqual(MingchengRingGateRules.Outside, side, "held outside");
            Vector2 push = MingchengRingGateRules.WallPush(side, stepIn, Center, true);
            Assert.Greater((stepIn + push - Center).magnitude, r, "shoved back over the line");

            Assert.AreEqual(Vector2.zero, MingchengRingGateRules.WallPush(side, stepIn, Center, false), "by day nothing holds");
            Assert.AreEqual(MingchengRingGateRules.Inside, MingchengRingGateRules.NextSide(MingchengRingGateRules.Outside, outside, stepIn, Center, false));

            // Going out after the bell is free.
            Vector2 inside = Center + new Vector2(r - 1f, 0f);
            side = MingchengRingGateRules.NextSide(MingchengRingGateRules.Inside, inside, outside, Center, true);
            Assert.AreEqual(MingchengRingGateRules.Outside, side);
            Assert.AreEqual(Vector2.zero, MingchengRingGateRules.WallPush(MingchengRingGateRules.Inside, inside, Center, true));

            // A respawn or a barque lands deep inside: the side is taken again, and no one is ever yanked from the plaza.
            Vector2 plaza = Center + new Vector2(3f, 2f);
            Assert.AreEqual(MingchengRingGateRules.Inside, MingchengRingGateRules.NextSide(MingchengRingGateRules.Outside, outside, plaza, Center, true));
            Assert.AreEqual(Vector2.zero, MingchengRingGateRules.WallPush(MingchengRingGateRules.Outside, plaza, Center, true));
            Assert.AreEqual(MingchengRingGateRules.Inside, MingchengRingGateRules.NextSide(MingchengRingGateRules.Unknown, plaza, plaza, Center, true));
        }

        [Test]
        public void Gates_OnTheCitysThreeRoads_InTheRings_TheFenceClearsTheTownAndTheFesta()
        {
            OverworldLayout world = OverworldLayouts.Get(OverworldLayouts.MingchengId);
            Assert.IsNotNull(world);
            RingGate[] gates = OverworldLayouts.MingchengRingGates;
            Assert.AreEqual(3, gates.Length);
            foreach (RingGate gate in gates)
            {
                Assert.AreEqual(MingchengRingGateRules.Radius, (gate.Crossing - Center).magnitude, 0.01f, gate.Id + " on the ring");
                Assert.Greater((gate.Outer - Center).magnitude, MingchengRingGateRules.Radius, gate.Id + " station outside");
                Assert.Less((gate.Inner - Center).magnitude, MingchengRingGateRules.Radius - 1f, gate.Id + " street inside");
                Assert.IsTrue(MapLayoutRules.OnPath(world.World, gate.Outer, 0.5f), gate.Id + " station on the road");
                Assert.IsTrue(MapLayoutRules.OnPath(world.World, gate.Crossing, 0.5f), gate.Id + " gate on the road");
                Assert.IsTrue(MapLayoutRules.OnPath(world.World, gate.Inner, 0.5f), gate.Id + " street on the road");
                Assert.AreEqual(OverworldLayouts.MingchengMapId, OverworldRules.ZoneAt(world, gate.Outer)?.MapIdOrOwner, gate.Id + " in Mingcheng (41)");
                Assert.IsTrue(MingchengRingGateMechanicsDriver.RingBounds.Contains(gate.Outer));
                Assert.AreEqual(2f * MingchengRingGateRules.GateEndOffset, RidePath.Length(gate.Course), 0.05f, "a short walk through");
                Assert.AreEqual(1f, gate.Across.magnitude, 1e-3f);
            }

            Assert.AreEqual(0, MingchengRingGateRules.OuterAt(gates, gates[0].Outer + new Vector2(1f, 1f)));
            Assert.AreEqual(-1, MingchengRingGateRules.OuterAt(gates, Center));
            Assert.AreEqual(2, MingchengRingGateRules.CrossingAt(gates, gates[2].Crossing));

            // The ring clears the town's flat ground and keeps the festa's lanterns inside.
            Assert.Greater(MingchengRingGateRules.Radius - MingchengRingGateRules.WallBand, OverworldLayouts.MingchengGround * Mathf.Sqrt(2f));
            foreach (Vector2 lantern in OverworldLayouts.MingchengRiddleLanterns)
            {
                Assert.AreEqual(MingchengRingGateRules.Inside, MingchengRingGateRules.SideOf(OverworldLayouts.MingchengOffset + lantern, Center));
            }

            // The fence: arcs on the ring, open at every gate.
            List<Vector2[]> arcs = MingchengRingGateRules.WallArcs(gates, Center, 24);
            Assert.AreEqual(3, arcs.Count);
            foreach (Vector2[] arc in arcs)
            {
                foreach (Vector2 p in arc)
                {
                    Assert.AreEqual(MingchengRingGateRules.Radius, (p - Center).magnitude, 0.01f);
                    foreach (RingGate gate in gates)
                    {
                        Assert.GreaterOrEqual((p - gate.Crossing).magnitude, MingchengRingGateRules.GateHalfWidth * 0.95f, "the fence leaves the gate open");
                    }
                }
            }
        }

        [Test]
        public void Texts_EveryKeyInBothLanguages_ACousinLineEach_NoFolksyNames()
        {
            var keys = new HashSet<string>();
            foreach (var entry in MingchengRingGateTexts.Entries)
            {
                Assert.IsTrue(keys.Add(entry.Key), "duplicate key " + entry.Key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), entry.Key);
                StringAssert.DoesNotContain("Dona ", entry.PtBR);
                StringAssert.DoesNotContain("Seu ", entry.PtBR);
            }

            foreach (FieldInfo field in typeof(MingchengRingGateTexts).GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.IsLiteral && f.FieldType == typeof(string)))
            {
                Assert.IsTrue(keys.Contains((string)field.GetRawConstantValue()), field.Name + " has an entry");
            }

            var lines = new HashSet<string>();
            foreach (CharacterId? hero in new CharacterId?[] { CharacterId.Joaquim, CharacterId.Maria, CharacterId.Maithe, CharacterId.Icaro, CharacterId.Lavine })
            {
                string key = MingchengRingGateTexts.BellLineFor(hero);
                Assert.IsTrue(keys.Contains(key), key);
                Assert.IsTrue(lines.Add(key), "one line per cousin");
            }

            Assert.AreEqual(MingchengRingGateTexts.BellLineFor(CharacterId.Joaquim), MingchengRingGateTexts.BellLineFor(null));
            Assert.AreEqual("Portão Oeste do 4º anel: aberto com o tambor até o sino das 20 h",
                MingchengRingGateTexts.Format(null, MingchengRingGateTexts.DayOpen, MingchengRingGateTexts.Get(null, MingchengRingGateTexts.GateWest)));
            Assert.AreEqual("O sino fechou o portão. A portinhola abre na vigia das 22 h (ou agora por 10 de ouro)",
                MingchengRingGateTexts.Format(null, MingchengRingGateTexts.WaitHint, 22, MingchengRingGateRules.Toll));
        }
    }
}
