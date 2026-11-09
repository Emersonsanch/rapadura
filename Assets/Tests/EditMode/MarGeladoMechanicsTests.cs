using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Fishing;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The Mar Gelado mechanics (Docs/Mapas100.md 59–64): the moon over the canals (<see cref="MoonCanalRules"/>), the thin ice
    /// (<see cref="ThinIceRules"/>), the ice fishing (<see cref="IceFishingSession"/>) and the places the layout declares for them.
    /// </summary>
    public class MarGeladoMechanicsTests
    {
        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.MarGeladoRegionId);

        // ------------------------------------------------------------------
        // The moon
        // ------------------------------------------------------------------

        [TestCase(1, false)]
        [TestCase(3, false)]
        [TestCase(4, true)]
        [TestCase(5, true)]
        [TestCase(6, true)]
        [TestCase(7, false)]
        [TestCase(9, false)]
        [TestCase(13, true)]
        public void Moon_FreezesTheCanals_ThreeDaysOutOfEight(int day, bool frozen)
        {
            Assert.AreEqual(frozen, MoonCanalRules.IsFrozen(day));
        }

        [Test]
        public void Moon_CountsTheDaysToTheFreeze()
        {
            Assert.AreEqual(3, MoonCanalRules.DaysUntilFrozen(1));
            Assert.AreEqual(0, MoonCanalRules.DaysUntilFrozen(5));
            Assert.AreEqual(5, MoonCanalRules.DaysUntilFrozen(7));
        }

        [Test]
        public void Crossing_FollowsTheWater_TheBoat_AndTheMount()
        {
            Assert.AreEqual(CanalCrossCheck.WalkOnIce, MoonCanalRules.Check(5, false, false, true));
            Assert.AreEqual(CanalCrossCheck.HeavyMount, MoonCanalRules.Check(5, false, true, true));
            Assert.AreEqual(CanalCrossCheck.NeedsBoat, MoonCanalRules.Check(1, false, false, true));
            Assert.AreEqual(CanalCrossCheck.ByBoat, MoonCanalRules.Check(1, true, true, true), "a boat carries anyone");
            Assert.AreEqual(CanalCrossCheck.ZoneBlocked, MoonCanalRules.Check(5, true, false, false));
            Assert.AreEqual(2f, MoonCanalRules.Duration(12f, CanalCrossCheck.WalkOnIce), 1e-3f);
            Assert.AreEqual(2f, MoonCanalRules.Duration(18f, CanalCrossCheck.ByBoat), 1e-3f);
        }

        // ------------------------------------------------------------------
        // Thin ice
        // ------------------------------------------------------------------

        [Test]
        public void ThinIce_OnlyHeavyMountsCount()
        {
            Assert.IsTrue(ThinIceRules.IsHeavyMount("mount_woolly_mammoth"));
            Assert.IsFalse(ThinIceRules.IsHeavyMount("mount_pony"));
            Assert.IsFalse(ThinIceRules.IsHeavyMount(null));
        }

        [Test]
        public void ThinIce_WarnsBeforeBreaking_FasterAtNight_AndDrains()
        {
            Assert.AreEqual(ThinIceStage.Holding, ThinIceRules.Stage(1.1f));
            Assert.AreEqual(ThinIceStage.Creaking, ThinIceRules.Stage(1.2f));
            Assert.AreEqual(ThinIceStage.Broken, ThinIceRules.Stage(4f));
            Assert.AreEqual(1f, ThinIceRules.Advance(0f, true, false, 1f), 1e-4f);
            Assert.AreEqual(1.6f, ThinIceRules.Advance(0f, true, true, 1f), 1e-4f);
            Assert.AreEqual(1f, ThinIceRules.Advance(2f, false, false, 0.5f), 1e-4f);
            Assert.AreEqual(0f, ThinIceRules.Advance(0.5f, false, false, 5f), 1e-4f);
        }

        [TestCase(1000f, 1000f, 80f)]
        [TestCase(50f, 1000f, 49f)]
        [TestCase(1f, 1000f, 0f)]
        public void ThinIce_BiteNeverKills(float current, float max, float expected)
        {
            float bite = ThinIceRules.BreakDamage(current, max);
            Assert.AreEqual(expected, bite, 1e-3f);
            Assert.GreaterOrEqual(current - bite, 1f);
        }

        // ------------------------------------------------------------------
        // Ice fishing
        // ------------------------------------------------------------------

        [Test]
        public void Fishing_PressInTheWindow_Lands_TheSameFishForTheSameSeed()
        {
            IceFishingSession a = Bitten(42);
            IceFishingSession b = Bitten(42);
            a.Press();
            b.Press();
            Assert.AreEqual(IceFishingState.Caught, a.State);
            Assert.AreEqual(a.Fish.Id, b.Fish.Id);
            Assert.AreEqual(a.Grams, b.Grams);
            Assert.Greater(a.Grams, 0);
        }

        [Test]
        public void Fishing_TooSoonScares_TooLateLoses()
        {
            var early = new IceFishingSession(1, false, false);
            early.Press();
            Assert.AreEqual(IceFishingState.ScaredOff, early.State);

            IceFishingSession late = Bitten(2);
            late.Tick(IceFishingSession.BiteWindow + 0.01f);
            Assert.AreEqual(IceFishingState.Lost, late.State);
            Assert.IsTrue(late.IsOver);
        }

        [Test]
        public void Fishing_NothingBitesBeforeTheMinimumDelay()
        {
            var session = new IceFishingSession(3, false, false);
            session.Tick(IceFishingSession.MinBiteDelay - 0.01f);
            Assert.AreEqual(IceFishingState.Waiting, session.State);
        }

        [Test]
        public void Fishing_Table_RunsFromCommonToTheGoldenFish_AllInTheCatalogue()
        {
            Assert.AreEqual(FishRarity.Common, IceFishingRules.Roll(0d, false, false).Rarity);
            Assert.AreEqual(FishRarity.Uncommon, IceFishingRules.Roll(0.8d, false, false).Rarity);
            Assert.AreEqual(FishRarity.Rare, IceFishingRules.Roll(0.97d, false, false).Rarity);
            Assert.AreEqual(FishCatalog.PeixeDourado, IceFishingRules.Roll(0.9999d, false, false).Id);
            Assert.AreEqual(FishRarity.Rare, IceFishingRules.Roll(0.94d, true, false).Rarity, "night brings the rare fish up");
            Assert.AreEqual(FishCatalog.PeixeDourado, IceFishingRules.Roll(0.99d, false, true).Id, "the Great Hole triples the golden fish");
        }

        [Test]
        public void Fishing_HoleAt_ReachesPastTheRim_NearestFirst()
        {
            var holes = new[] { new IceHole(Vector2.zero, 8f, false), new IceHole(new Vector2(20f, 0f), 8f, true) };
            Assert.AreEqual(0, IceFishingRules.HoleAt(holes, new Vector2(-(8f + IceFishingRules.Reach), 0f)));
            Assert.AreEqual(-1, IceFishingRules.HoleAt(holes, new Vector2(-(8.1f + IceFishingRules.Reach), 0f)));
            Assert.AreEqual(1, IceFishingRules.HoleAt(holes, new Vector2(11f, 0f)));
            Assert.AreEqual(-1, IceFishingRules.HoleAt(holes, new Vector2(0f, 20f)));
        }

        private static IceFishingSession Bitten(int seed)
        {
            var session = new IceFishingSession(seed, false, false);
            session.Tick(IceFishingSession.MaxBiteDelay + 0.01f);
            Assert.AreEqual(IceFishingState.Bite, session.State);
            return session;
        }

        // ------------------------------------------------------------------
        // The layout
        // ------------------------------------------------------------------

        [Test]
        public void Layout_AnchorExists_AndTheBoundsAreTheRegions()
        {
            Assert.IsNotNull(World.Zone(OverworldLayouts.MarGeladoAnchorZoneId));
            Assert.AreEqual(World.Bounds, OverworldLayouts.MarGeladoBounds);
        }

        [Test]
        public void Layout_Crossings_HaveUniqueIds_AndCrossACanal()
        {
            Assert.GreaterOrEqual(OverworldLayouts.CanalCrossings.Length, 5);
            Assert.AreEqual(OverworldLayouts.CanalCrossings.Length, OverworldLayouts.CanalCrossings.Select(r => r.Id).Distinct().Count());
            foreach (SkyRoute route in OverworldLayouts.CanalCrossings)
            {
                Assert.IsTrue(MapLayoutRules.InRiver(World.World, Vector2.Lerp(route.A, route.B, 0.5f), 0f), route.Id + " must span water");
                Assert.Less(Vector2.Distance(route.A, route.B), 26f, route.Id);
            }
        }

        [Test]
        public void Layout_Landings_AreDry_Level_ClearOfTowns_InTheCanalsZones()
        {
            foreach (SkyRoute route in OverworldLayouts.CanalCrossings)
            {
                foreach ((Vector2 landing, Vector2 prop) in new[] { (route.A, route.PropA), (route.B, route.PropB) })
                {
                    Assert.IsTrue(OverworldLayouts.MarGeladoBounds.Contains(landing), route.Id);
                    Assert.IsFalse(MapLayoutRules.InRiver(World.World, landing, 2f), route.Id + " landing in the water");
                    Assert.IsFalse(MapLayoutRules.InRiver(World.World, prop, 1f), route.Id + " lantern in the water");
                    Assert.IsNull(OverworldRules.SettlementAt(World, landing), route.Id);
                    Assert.LessOrEqual(Vector2.Distance(landing, prop), SkyRouteRules.StationRadius, route.Id);
                    float h = OverworldRules.SurfaceAt(World, landing);
                    Assert.GreaterOrEqual(h, -0.05f, route.Id + " landing below the banks");
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 near = landing + new Vector2(Mathf.Cos(i * Mathf.PI / 4f), Mathf.Sin(i * Mathf.PI / 4f)) * 2f;
                        Assert.Less(Mathf.Abs(OverworldRules.SurfaceAt(World, near) - h), 1.5f, route.Id + " " + landing);
                    }
                }
            }
        }

        [Test]
        public void Layout_EveryCrossing_IsOpenToAHeroWhoMayEnterItsZones()
        {
            foreach (SkyRoute route in OverworldLayouts.CanalCrossings)
            {
                Assert.IsTrue(SkyRouteRules.AllZonesOpen(World, Vector2.zero, route, 200, CharacterId.Maithe, tag => true), route.Id);
            }
        }

        [Test]
        public void Layout_ThinIce_LiesInTheCanalsMap_OffTheTowns_AndSomeOfItOnARoad()
        {
            foreach (Rect patch in OverworldLayouts.ThinIcePatches)
            {
                Assert.AreEqual(OverworldLayouts.CanaisGeloMapId, OverworldRules.ZoneAt(World, patch.center).MapIdOrOwner, patch.ToString());
                foreach (OverworldSettlement town in World.Settlements)
                {
                    Assert.IsFalse(patch.Overlaps(town.GroundRect), patch + " over " + town.Id);
                }
            }

            int onRoad = OverworldLayouts.ThinIcePatches.Count(p => MapLayoutRules.OnPath(World.World, p.center, 25f));
            Assert.GreaterOrEqual(onRoad, 2);
        }

        [Test]
        public void Layout_Holes_AreTheLakesWater_AndTheGreatHoleIsLast()
        {
            Assert.AreEqual(OverworldLayouts.KulHoles.Length, OverworldLayouts.IceFishingHoles.Length);
            Assert.AreEqual(1, OverworldLayouts.IceFishingHoles.Count(h => h.Great));
            Assert.IsTrue(OverworldLayouts.IceFishingHoles.Last().Great);
            foreach (IceHole hole in OverworldLayouts.IceFishingHoles)
            {
                Assert.IsTrue(MapLayoutRules.InRiver(World.World, hole.Center, 0f), hole.Center.ToString());
                Assert.IsFalse(MapLayoutRules.InRiver(World.World, hole.Center + Vector2.right * (hole.Radius + IceFishingRules.Reach), 0f), "the rim must be reachable on dry ice");
            }
        }

        [Test]
        public void Texts_HaveBothLanguages_AndPlaceholdersMatch()
        {
            Assert.GreaterOrEqual(MarGeladoMechanicsTexts.Entries.Count, 18);
            foreach (var entry in MarGeladoMechanicsTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), entry.Key);
                Assert.AreEqual(entry.En.Contains("{0}"), entry.PtBR.Contains("{0}"), entry.Key);
            }
        }
    }
}
