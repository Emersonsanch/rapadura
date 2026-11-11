using NUnit.Framework;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Defending the stranded barque on Apep's sandbank (Docs/Mapas100.md 76, <see cref="RioBarqueDefenseRules"/> and <see cref="RioBarqueDefense"/>):
    /// the run, the gnawing, Apep's pulls, the oars, the endings, the daily mark and the freed barque's sails the layout declares.
    /// </summary>
    public class RioBarqueDefenseMechanicsTests
    {
        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.RioRegionId);

        private static RioDefenseEvent Run(RioBarqueDefense defense, float seconds, float distance, float gnaw, float step = 0.1f)
        {
            var all = RioDefenseEvent.None;
            for (float t = 0f; t < seconds - 1e-4f && defense.Running; t += step)
            {
                all |= defense.Tick(step, distance, gnaw);
            }

            return all;
        }

        [Test]
        public void Gnawing_CountsSixBeastsAtMost_BossesFirst()
        {
            Assert.AreEqual(0f, RioBarqueDefenseRules.GnawRate(0, 0));
            Assert.AreEqual(4.5f, RioBarqueDefenseRules.GnawRate(3, 0), 1e-4f);
            Assert.AreEqual(9f, RioBarqueDefenseRules.GnawRate(10, 0), 1e-4f);
            Assert.AreEqual(4f + 7.5f, RioBarqueDefenseRules.GnawRate(10, 1), 1e-4f);
        }

        [Test]
        public void Start_OncePerDay_AfterTheRowersRest_WhereTheHeroMayGo()
        {
            Assert.AreEqual(RioDefenseStart.AlreadyFreed, RioBarqueDefenseRules.CheckStart(true, 999f, true));
            Assert.AreEqual(RioDefenseStart.ZoneBlocked, RioBarqueDefenseRules.CheckStart(false, 999f, false));
            Assert.AreEqual(RioDefenseStart.RowersResting, RioBarqueDefenseRules.CheckStart(false, 10f, true));
            Assert.AreEqual(RioDefenseStart.Ok, RioBarqueDefenseRules.CheckStart(false, 30f, true));
            Assert.IsTrue(RioBarqueDefenseRules.FreedToday(4, 4));
            Assert.IsFalse(RioBarqueDefenseRules.FreedToday(3, 4));
            Assert.IsFalse(RioBarqueDefenseRules.FreedToday(0, 0), "an old save (0) never counts as freed");
        }

        [Test]
        public void Defence_UnharmedAndOnTheBow_FreesTheBarqueAfterNinetySeconds()
        {
            var defense = new RioBarqueDefense();
            defense.Begin();
            RioDefenseEvent all = Run(defense, 89f, 2f, 0f);
            Assert.IsTrue(defense.Running);
            Assert.AreEqual(RioBarqueDefenseRules.HullMax, defense.Hull, 1e-3f, "every pull fell on the hero on the bow");
            Assert.IsTrue(defense.LastPullOnHero);
            Assert.AreNotEqual(RioDefenseEvent.None, all & RioDefenseEvent.Pull);
            Assert.AreNotEqual(RioDefenseEvent.None, all & RioDefenseEvent.PullWarning);
            Assert.AreNotEqual(RioDefenseEvent.None, all & RioDefenseEvent.Oars);

            all = Run(defense, 2f, 2f, 0f);
            Assert.AreNotEqual(RioDefenseEvent.None, all & RioDefenseEvent.Freed);
            Assert.IsFalse(defense.Running);
            Assert.AreEqual(1f, defense.Progress, 1e-4f);
        }

        [Test]
        public void Defence_PullsOffTheBow_TakeTheHull_FourTimesInNinetySeconds()
        {
            var defense = new RioBarqueDefense();
            defense.Begin();
            Run(defense, 95f, 10f, 0f);
            Assert.IsFalse(defense.Running);
            Assert.AreEqual(RioBarqueDefenseRules.HullMax - 4f * RioBarqueDefenseRules.PullHullDamage, defense.Hull, 1e-3f, "pulls at 20, 40, 60 and 80 s");
            Assert.IsFalse(defense.LastPullOnHero);
        }

        [Test]
        public void Defence_SixBeastsOnTheHull_SinkIt()
        {
            var defense = new RioBarqueDefense();
            defense.Begin();
            RioDefenseEvent all = Run(defense, 90f, 2f, RioBarqueDefenseRules.GnawRate(6, 0));
            Assert.AreNotEqual(RioDefenseEvent.None, all & RioDefenseEvent.Sunk, "9 a second sink 100 in about 11 s");
            Assert.AreEqual(0f, defense.Hull);
            Assert.IsFalse(defense.Running);
        }

        [Test]
        public void Defence_HeroAwayFifteenSeconds_TheRowersDropTheOars_AndNothingIsPushed()
        {
            var defense = new RioBarqueDefense();
            defense.Begin();
            RioDefenseEvent all = Run(defense, 16f, 40f, 0f);
            Assert.AreNotEqual(RioDefenseEvent.None, all & RioDefenseEvent.Abandoned);
            Assert.AreEqual(0f, defense.Pushed, 1e-4f);

            defense.Begin();
            Run(defense, 10f, 40f, 0f);
            Run(defense, 1f, 2f, 0f);
            Run(defense, 10f, 40f, 0f);
            Assert.IsTrue(defense.Running, "coming back in time resets the count");
        }

        [Test]
        public void Defence_WarnsThreeSecondsBeforeEachPull()
        {
            var defense = new RioBarqueDefense();
            defense.Begin();
            Assert.AreEqual(RioDefenseEvent.None, Run(defense, 16.5f, 2f, 0f) & RioDefenseEvent.PullWarning);
            Assert.IsFalse(defense.PullImminent);
            Assert.AreNotEqual(RioDefenseEvent.None, Run(defense, 1f, 2f, 0f) & RioDefenseEvent.PullWarning);
            Assert.IsTrue(defense.PullImminent);
        }

        [Test]
        public void Amounts_PullOnTheHeroStopsAtThirtyPercent_OarsAndLightSpareABoss()
        {
            Assert.AreEqual(4f, RioBarqueDefenseRules.PullOnHero(100f, 100f), 1e-4f);
            Assert.AreEqual(2f, RioBarqueDefenseRules.PullOnHero(32f, 100f), 1e-4f);
            Assert.AreEqual(0f, RioBarqueDefenseRules.PullOnHero(30f, 100f));
            Assert.AreEqual(40f, RioBarqueDefenseRules.OarAmount(1000f, false), 1e-3f);
            Assert.AreEqual(5f, RioBarqueDefenseRules.OarAmount(1000f, true), 1e-3f);
            Assert.AreEqual(120f, RioBarqueDefenseRules.BurstAmount(1000f, false), 1e-3f);
            Assert.AreEqual(20f, RioBarqueDefenseRules.BurstAmount(1000f, true), 1e-3f);
            Assert.AreEqual(10f, RioBarqueDefenseRules.SailSeconds(550f), 1e-4f);
        }

        // ------------------------------------------------------------------
        // The layout
        // ------------------------------------------------------------------

        [Test]
        public void Layout_TheHull_StandsOnTheSandbank_OffThePostAndTheWater()
        {
            Vector2 hull = OverworldLayouts.StrandedBarque;
            OverworldZone zone = OverworldRules.ZoneAt(World, hull);
            Assert.IsNotNull(zone);
            Assert.AreEqual(OverworldLayouts.BancoApepMapId, zone.MapId ?? zone.OwnerMapId);
            Assert.IsNull(OverworldRules.SettlementAt(World, hull), "the hull is not inside the post's safe ground");
            Assert.IsFalse(MapLayoutRules.InRiver(World.World, hull, 6f));
            Assert.Greater(Vector2.Distance(hull, OverworldLayouts.ApepArena), RioBarqueDefenseRules.GuardRadius + 20f, "Apep's ring is not on the deck");
        }

        [Test]
        public void Layout_Sails_RunTheBank_ToItsTwoEnds_InTwelveSecondsAtMost()
        {
            Assert.AreEqual(2, OverworldLayouts.FreedBarqueSails.Length);
            foreach (SkyRoute sail in OverworldLayouts.FreedBarqueSails)
            {
                Assert.Less(Vector2.Distance(sail.A, OverworldLayouts.StrandedBarque), RioBarqueDefenseRules.HullReach * 2f, sail.Id);
                Assert.LessOrEqual(RioBarqueDefenseRules.SailSeconds(Vector2.Distance(sail.A, sail.B)), 12f, sail.Id);
                Assert.Greater(Vector2.Distance(sail.A, sail.B), 500f, sail.Id + " is not worth a sail");
                foreach (Vector2 p in new[] { sail.A, sail.B })
                {
                    OverworldZone zone = OverworldRules.ZoneAt(World, p);
                    Assert.IsNotNull(zone, sail.Id);
                    Assert.AreEqual(OverworldLayouts.BancoApepMapId, zone.MapId ?? zone.OwnerMapId, sail.Id);
                    Assert.IsFalse(MapLayoutRules.InRiver(World.World, p, 3f), sail.Id);
                }

                float nearest = float.MaxValue;
                foreach (MapPath path in World.World.Paths)
                {
                    nearest = Mathf.Min(nearest, MapLayoutRules.DistanceToSegment(sail.B, path.A, path.B));
                }

                Assert.Less(nearest, 6f, sail.Id + " lands far from the bank's road");
            }
        }
    }
}
