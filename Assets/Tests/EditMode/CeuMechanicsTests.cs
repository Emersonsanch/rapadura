using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The Reinos do Céu mechanics (Docs/Mapas100.md 91–100): the Barca da Lua by night (<see cref="CeuMoonBarqueRules"/>), holding the sky (<see cref="CeuSkyHoldRules"/>),
    /// sitting on the porch (<see cref="CeuPorchRules"/>) and the places the layout declares for them.
    /// </summary>
    public class CeuMechanicsTests
    {
        private const float Step = 1f / 60f;

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.CeuRegionId);

        // ------------------------------------------------------------------
        // The Barca da Lua
        // ------------------------------------------------------------------

        [Test]
        public void Barque_BoardsOnlyAtNight_AndOnlyWhereTheHeroMayGo()
        {
            Assert.AreEqual(CeuBarqueCheck.Board, CeuMoonBarqueRules.Check(true, true));
            Assert.AreEqual(CeuBarqueCheck.ZoneBlocked, CeuMoonBarqueRules.Check(true, false));
            Assert.AreEqual(CeuBarqueCheck.NotNight, CeuMoonBarqueRules.Check(false, true));
            Assert.AreEqual(CeuBarqueCheck.NotNight, CeuMoonBarqueRules.Check(false, false));
        }

        [TestCase(12f, 8)]
        [TestCase(19.5f, 1)]
        [TestCase(20f, 0)]
        [TestCase(21f, 23)]
        public void Barque_CountsTheHoursToNight(float hour, int hours)
        {
            Assert.AreEqual(hours, CeuMoonBarqueRules.HoursUntilNight(hour, 20f));
        }

        [Test]
        public void Barque_Reach_IsAStepAway()
        {
            Assert.IsTrue(CeuMoonBarqueRules.WithinReach(Vector2.zero, new Vector2(CeuMoonBarqueRules.Reach, 0f)));
            Assert.IsFalse(CeuMoonBarqueRules.WithinReach(Vector2.zero, new Vector2(CeuMoonBarqueRules.Reach + 0.1f, 0f)));
        }

        // ------------------------------------------------------------------
        // Holding the sky
        // ------------------------------------------------------------------

        private static CeuSkyEvent[] RunHold(CeuSkyHoldSession session, float seconds, bool inReach, long now = 1000L)
        {
            int steps = Mathf.CeilToInt(seconds / Step);
            return Enumerable.Range(0, steps).Select(_ => session.Tick(Step, inReach, now)).Where(e => e != CeuSkyEvent.None).ToArray();
        }

        [Test]
        public void Hold_InReach_SealsAfterTheHold_WithThreeWarnedDrips()
        {
            var session = new CeuSkyHoldSession();
            Assert.IsTrue(session.TryRaise(2, 1000L));
            CeuSkyEvent[] events = RunHold(session, CeuSkyHoldRules.HoldSeconds + 0.1f, true);
            CollectionAssert.AreEqual(
                new[] { CeuSkyEvent.DripWarning, CeuSkyEvent.Drip, CeuSkyEvent.DripWarning, CeuSkyEvent.Drip, CeuSkyEvent.DripWarning, CeuSkyEvent.Drip, CeuSkyEvent.Sealed },
                events);
            Assert.AreEqual(-1, session.ActiveCrack);
            Assert.IsTrue(session.IsSealed(2, 1000L));
            Assert.AreEqual(1, session.SealedCount(1000L));
        }

        [Test]
        public void Hold_WalkingAway_RunsThePillarDown_AndNeverPinsTheHero()
        {
            var session = new CeuSkyHoldSession();
            session.TryRaise(0, 0L);
            RunHold(session, 4f, true);
            Assert.AreEqual(0, RunHold(session, 1f, false).Length, "still up after a step out");
            Assert.Greater(session.Progress, 0f);
            CollectionAssert.AreEqual(new[] { CeuSkyEvent.Faded }, RunHold(session, 2f, false));
            Assert.AreEqual(-1, session.ActiveCrack);
            Assert.IsFalse(session.IsSealed(0, 0L));
        }

        [Test]
        public void Hold_StepsOutOfTheDripClock()
        {
            var session = new CeuSkyHoldSession();
            session.TryRaise(0, 0L);
            RunHold(session, 5f, true);
            Assert.IsTrue(session.DripImminent);
            RunHold(session, 0.5f, false);
            Assert.IsFalse(session.DripImminent);
        }

        [Test]
        public void Hold_Sealed_ReopensOnTheClock_AndOnlyOnePillarAtATime()
        {
            var session = new CeuSkyHoldSession();
            session.TryRaise(1, 1000L);
            Assert.IsFalse(session.TryRaise(3, 1000L), "second pillar");
            RunHold(session, CeuSkyHoldRules.HoldSeconds + 0.1f, true);
            Assert.IsFalse(session.TryRaise(1, 1001L), "sealed crack");
            Assert.IsTrue(session.IsSealed(1, 1000L + CeuSkyHoldRules.SealSeconds - 1L));
            Assert.IsFalse(session.IsSealed(1, 1000L + CeuSkyHoldRules.SealSeconds));
            Assert.IsTrue(session.TryRaise(1, 1000L + CeuSkyHoldRules.SealSeconds));
            Assert.IsFalse(session.TryRaise(-1, 0L));
        }

        [TestCase(100f, 100f, 5f)]
        [TestCase(32f, 100f, 2f)]
        [TestCase(30f, 100f, 0f)]
        [TestCase(10f, 100f, 0f)]
        [TestCase(0f, 100f, 0f)]
        public void Hold_DripNeverTakesTheHeroBelowTheFloor(float current, float max, float expected)
        {
            Assert.AreEqual(expected, CeuSkyHoldRules.DripAmount(current, max), 1e-3f);
        }

        [Test]
        public void Hold_RepeatedDrips_CannotKill()
        {
            float health = 100f;
            for (int i = 0; i < 200; i++)
            {
                health -= CeuSkyHoldRules.DripAmount(health, 100f);
            }

            Assert.AreEqual(100f * CeuSkyHoldRules.DripFloor, health, 1e-3f);
        }

        [Test]
        public void Hold_Seal_MendsAndBites_ButLeavesBosses()
        {
            Assert.AreEqual(30f, CeuSkyHoldRules.SealHeal(100f), 1e-3f);
            Assert.AreEqual(15f, CeuSkyHoldRules.SealBite(100f, false), 1e-3f);
            Assert.AreEqual(0f, CeuSkyHoldRules.SealBite(100f, true), 1e-3f);
        }

        [Test]
        public void Hold_NearestCrack_PicksTheClosestInReach()
        {
            var points = new[] { Vector2.zero, new Vector2(10f, 0f) };
            Assert.AreEqual(1, CeuSkyHoldRules.NearestCrack(points, new Vector2(6f, 0f)));
            Assert.AreEqual(-1, CeuSkyHoldRules.NearestCrack(points, new Vector2(-7.5f, 0f)));
        }

        // ------------------------------------------------------------------
        // The porch
        // ------------------------------------------------------------------

        [TestCase(0L, 0)]
        [TestCase(86399L, 0)]
        [TestCase(86400L, 1)]
        [TestCase(86400L * 5L, 0)]
        [TestCase(-1L, 4)]
        public void Porch_OneStoryADay(long unix, int story)
        {
            Assert.AreEqual(story, CeuPorchRules.StoryIndex(unix));
        }

        [Test]
        public void Porch_TellsEveryLine_ThenEnds()
        {
            var session = new CeuPorchSession();
            session.Begin(2, false);
            int lines = 0;
            CeuPorchEvent last = CeuPorchEvent.None;
            for (int i = 0; i < 26 * 60 && session.IsSitting; i++)
            {
                CeuPorchEvent happened = session.Tick(Step, true, false);
                if (happened == CeuPorchEvent.NextLine)
                {
                    lines++;
                }

                if (happened != CeuPorchEvent.None)
                {
                    last = happened;
                }
            }

            Assert.AreEqual(CeuPorchRules.LinesPerStory - 1, lines);
            Assert.AreEqual(CeuPorchEvent.Finished, last);
            Assert.IsFalse(session.IsSitting);
        }

        [Test]
        public void Porch_AtNight_AddsTheClosingLine()
        {
            Assert.AreEqual(CeuPorchRules.LinesPerStory + 1, CeuPorchRules.LineCount(true));
            var session = new CeuPorchSession();
            session.Begin(0, true);
            for (int i = 0; i < CeuPorchRules.LinesPerStory; i++)
            {
                Assert.AreEqual(CeuPorchEvent.NextLine, session.Tick(CeuPorchRules.LineSeconds, true, false));
            }

            Assert.AreEqual(CeuPorchRules.LinesPerStory, session.Line);
            Assert.AreEqual(CeuPorchEvent.Finished, session.Tick(CeuPorchRules.LineSeconds, true, false));
        }

        [Test]
        public void Porch_WalkingOffOrBeingHurt_EndsTheSitting()
        {
            var session = new CeuPorchSession();
            session.Begin(0, false);
            Assert.AreEqual(CeuPorchEvent.Interrupted, session.Tick(Step, false, false));
            Assert.IsFalse(session.IsSitting);
            session.Begin(0, false);
            Assert.AreEqual(CeuPorchEvent.Interrupted, session.Tick(Step, true, true));
            Assert.AreEqual(CeuPorchEvent.None, session.Tick(Step, true, false));
        }

        [Test]
        public void Porch_Rest_StopsAtFullHealth()
        {
            Assert.AreEqual(3f, CeuPorchRules.RestAmount(50f, 100f, 1f), 1e-3f);
            Assert.AreEqual(1f, CeuPorchRules.RestAmount(99f, 100f, 1f), 1e-3f);
            Assert.AreEqual(0f, CeuPorchRules.RestAmount(100f, 100f, 1f), 1e-3f);
        }

        // ------------------------------------------------------------------
        // The layout
        // ------------------------------------------------------------------

        [Test]
        public void Layout_BarqueEnds_AreOnTheRoads_InTheirMaps()
        {
            Vector2 pier = OverworldLayouts.MoonBarquePier;
            Vector2 berth = OverworldLayouts.MoonBarqueBerth;
            Assert.AreEqual(OverworldLayouts.CaisDaColunaMapId, OverworldRules.ZoneAt(World, pier).MapIdOrOwner);
            Assert.AreEqual(OverworldLayouts.BarcaDaLuaMapId, OverworldRules.ZoneAt(World, berth).MapIdOrOwner);
            foreach (Vector2 end in new[] { pier, berth })
            {
                Assert.IsTrue(MapLayoutRules.OnPath(World.World, end, 2f), end.ToString());
                Assert.IsFalse(MapLayoutRules.InRiver(World.World, end, 2f), end.ToString());
                Assert.IsNull(OverworldRules.SettlementAt(World, end), end.ToString());
            }
        }

        [Test]
        public void Layout_Cracks_AreFive_InTheLookout_AndApart()
        {
            Assert.AreEqual(CeuSkyHoldRules.Cracks, OverworldLayouts.SkyCracks.Length);
            Assert.AreEqual(CeuSkyHoldRules.Cracks, OverworldLayouts.SkyCrackCount);
            Assert.AreEqual(CeuSkyHoldRules.Cracks, CeuMechanicsTexts.SealLines.Length);
            for (int i = 0; i < OverworldLayouts.SkyCracks.Length; i++)
            {
                Vector2 crack = OverworldLayouts.SkyCracks[i];
                Assert.AreEqual(OverworldLayouts.MiranteCeuMapId, OverworldRules.ZoneAt(World, crack).MapIdOrOwner, "crack " + i);
                Assert.IsNull(OverworldRules.SettlementAt(World, crack), "crack " + i);
                for (int j = 0; j < i; j++)
                {
                    Assert.Greater(Vector2.Distance(crack, OverworldLayouts.SkyCracks[j]), 2f * CeuSkyHoldRules.Reach, "cracks " + j + " and " + i);
                }
            }
        }

        [Test]
        public void Layout_Chair_IsOnThePorch()
        {
            Assert.AreEqual(OverworldLayouts.VarandaMapId, OverworldRules.ZoneAt(World, OverworldLayouts.RockingChairPosition).MapIdOrOwner);
            Assert.IsTrue(OverworldLayouts.CeuBounds.Contains(OverworldLayouts.RockingChairPosition));
        }

        [Test]
        public void Layout_Anchor_MapsTheRegionOntoTheStitchedWorld()
        {
            OverworldZone alone = World.Zone(OverworldLayouts.CeuAnchorZoneId);
            Assert.IsNotNull(alone);
            Assert.AreEqual(Vector2.zero, alone.Origin);
            OverworldZone stitched = OverworldLayouts.Get(OverworldLayouts.WorldId).Zone(OverworldLayouts.CeuAnchorZoneId);
            Assert.IsNotNull(stitched, "the anchor zone id must survive the stitching");
            Vector2 southWest = stitched.Origin + new Vector2(OverworldLayouts.CeuBounds.xMin, OverworldLayouts.CeuBounds.yMin);
            Assert.AreEqual(1400f, southWest.x, 1e-2f);
            Assert.AreEqual(8600f, southWest.y, 1e-2f);
        }

        [Test]
        public void Texts_HaveBothLanguages_AndEveryLineExists()
        {
            var keys = CeuMechanicsTexts.Entries.Select(entry => entry.Key).ToList();
            Assert.AreEqual(keys.Count, keys.Distinct().Count(), "duplicate key");
            for (int story = 0; story < CeuPorchRules.Stories; story++)
            {
                for (int line = 0; line < CeuPorchRules.LinesPerStory; line++)
                {
                    CollectionAssert.Contains(keys, CeuMechanicsTexts.StoryLine(story, line));
                }
            }

            foreach (var entry in CeuMechanicsTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), entry.Key);
                Assert.AreEqual(entry.En.Contains("{0}"), entry.PtBR.Contains("{0}"), entry.Key);
            }
        }
    }
}
