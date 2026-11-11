using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The wind wells of the Ninho de Mahaway (Docs/Mapas100.md 24, <see cref="LimiarWindWellRules"/>): the breath, the checks, the glide,
    /// the flock's pecks and the landing gust, and the wells the layout declares (over a chasm, on firm ground, shorter than the walk round).
    /// </summary>
    public class LimiarWindWellMechanicsTests
    {
        private const string NinhoMapId = "map_024_mahaway";

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.LimiarId);

        private static readonly LimiarWindWell TestWell = new LimiarWindWell("t", Vector2.zero, new Vector2(0f, 70f), 0f);

        [TestCase(0f, true)]
        [TestCase(7.9f, true)]
        [TestCase(8f, false)]
        [TestCase(11.9f, false)]
        [TestCase(12f, true)]
        public void Breath_BlowsEightSeconds_RestsFour(float time, bool blowing)
        {
            Assert.AreEqual(blowing, LimiarWindWellRules.IsBlowing(TestWell, time));
        }

        [Test]
        public void Breath_ThePhaseShiftsAWell_AndTheWaitIsCounted()
        {
            var shifted = new LimiarWindWell("s", Vector2.zero, Vector2.up, 6f);
            Assert.IsFalse(LimiarWindWellRules.IsBlowing(shifted, 3f), "3 s + 6 s = 9 s into the breath: resting");
            Assert.AreEqual(3f, LimiarWindWellRules.SecondsUntilBlowing(shifted, 3f), 1e-4f);
            Assert.AreEqual(0f, LimiarWindWellRules.SecondsUntilBlowing(TestWell, 1f));
        }

        [Test]
        public void Check_NeedsWings_ThenTheZones_ThenABreath_ThenTheWind()
        {
            Assert.AreEqual(LimiarWellCheck.NeedsWings, LimiarWindWellRules.Check(TestWell, 0f, false, true, 99f));
            Assert.AreEqual(LimiarWellCheck.ZoneBlocked, LimiarWindWellRules.Check(TestWell, 0f, true, false, 99f));
            Assert.AreEqual(LimiarWellCheck.Resting, LimiarWindWellRules.Check(TestWell, 0f, true, true, 1f));
            Assert.AreEqual(LimiarWellCheck.Calm, LimiarWindWellRules.Check(TestWell, 9f, true, true, 99f));
            Assert.AreEqual(LimiarWellCheck.Ok, LimiarWindWellRules.Check(TestWell, 1f, true, true, 99f));
        }

        [Test]
        public void WellAt_FindsTheNearestWithinReach()
        {
            var wells = new[] { TestWell, new LimiarWindWell("b", new Vector2(6f, 0f), Vector2.zero, 0f) };
            Assert.AreEqual(0, LimiarWindWellRules.WellAt(wells, new Vector2(2f, 0f)));
            Assert.AreEqual(1, LimiarWindWellRules.WellAt(wells, new Vector2(4f, 0f)));
            Assert.AreEqual(-1, LimiarWindWellRules.WellAt(wells, new Vector2(0f, 30f)));
        }

        [Test]
        public void Glide_TakesTheDistanceAtFourteenMetresASecond()
        {
            Assert.AreEqual(5f, LimiarWindWellRules.GlideSeconds(TestWell), 1e-4f);
            Assert.AreEqual(LimiarWindWellRules.MinGlideSeconds, LimiarWindWellRules.GlideSeconds(new LimiarWindWell("z", Vector2.zero, Vector2.one, 0f)));
        }

        [Test]
        public void Flock_PecksEveryTwoSeconds_NeverUnderThirtyPercent()
        {
            Assert.AreEqual(0, LimiarWindWellRules.PecksDue(1.9f));
            Assert.AreEqual(2, LimiarWindWellRules.PecksDue(4f));
            Assert.AreEqual(2f, LimiarWindWellRules.PeckAmount(100f, 100f, false), 1e-4f);
            Assert.AreEqual(3f, LimiarWindWellRules.PeckAmount(100f, 100f, true), 1e-4f);
            Assert.AreEqual(1f, LimiarWindWellRules.PeckAmount(31f, 100f, false), 1e-4f);
            Assert.AreEqual(0f, LimiarWindWellRules.PeckAmount(30f, 100f, true));
        }

        [Test]
        public void Gust_KnocksTheBeastsHarderThanABoss()
        {
            Assert.AreEqual(60f, LimiarWindWellRules.GustAmount(1000f, false), 1e-3f);
            Assert.AreEqual(10f, LimiarWindWellRules.GustAmount(1000f, true), 1e-3f);
            Assert.AreEqual(0f, LimiarWindWellRules.GustAmount(-5f, false));
        }

        // ------------------------------------------------------------------
        // The layout
        // ------------------------------------------------------------------

        [Test]
        public void Layout_Wells_HaveUniqueIds_AndStandInTheNinho()
        {
            var ids = new HashSet<string>();
            foreach (LimiarWindWell well in OverworldLayouts.LimiarWindWells)
            {
                Assert.IsTrue(ids.Add(well.Id), well.Id);
                foreach (Vector2 p in new[] { well.Well, well.Landing })
                {
                    Assert.IsTrue(OverworldLayouts.LimiarBounds.Contains(p), well.Id);
                    OverworldZone zone = OverworldRules.ZoneAt(World, p);
                    Assert.IsNotNull(zone, well.Id);
                    Assert.AreEqual(NinhoMapId, zone.MapId ?? zone.OwnerMapId, well.Id + " is outside the Ninho de Mahaway and its rims");
                    Assert.IsNull(OverworldRules.SettlementAt(World, p), well.Id + " in a safe zone");
                    foreach (MapRiver river in World.World.Rivers)
                    {
                        Assert.Greater(MapLayoutRules.DistanceToPolyline(p, river.Points), river.Width * 0.5f + 3f, well.Id + " stands in a chasm");
                    }
                }
            }

            Assert.GreaterOrEqual(OverworldLayouts.LimiarWindWells.Length, 6);
        }

        [Test]
        public void Layout_EveryGlide_CrossesAChasm_InAtMostSevenSeconds()
        {
            foreach (LimiarWindWell well in OverworldLayouts.LimiarWindWells)
            {
                bool crosses = false;
                for (int i = 1; i < 40 && !crosses; i++)
                {
                    Vector2 p = Vector2.Lerp(well.Well, well.Landing, i / 40f);
                    crosses = MapLayoutRules.InRiver(World.World, p, 0f);
                }

                Assert.IsTrue(crosses, well.Id + " glides over solid ground: no shortcut");
                Assert.LessOrEqual(LimiarWindWellRules.GlideSeconds(well), 7f, well.Id);
            }
        }

        [Test]
        public void Layout_TheChasmWells_SpareTheWalkToTheRopeBridge()
        {
            foreach (LimiarWindWell well in OverworldLayouts.LimiarWindWells)
            {
                if (Mathf.Abs(well.Well.x) < 300f)
                {
                    continue; // the pair over Mahaway's shaft: round its ends
                }

                Vector2 bridge = well.Well.x < 0f ? OverworldLayouts.ScarpsBridge : OverworldLayouts.BottomlessBridge;
                float glide = Vector2.Distance(well.Well, well.Landing);
                Assert.Greater(LimiarWindWellRules.WalkRound(well, bridge), glide * 3f, well.Id);
            }
        }

        [Test]
        public void Layout_EachRim_HasAWellBack()
        {
            foreach (LimiarWindWell well in OverworldLayouts.LimiarWindWells)
            {
                bool back = false;
                foreach (LimiarWindWell other in OverworldLayouts.LimiarWindWells)
                {
                    back |= Vector2.Distance(other.Well, well.Landing) < 45f && Vector2.Distance(other.Landing, well.Well) < 45f;
                }

                Assert.IsTrue(back, well.Id + " has no well back from its landing");
            }
        }
    }
}
