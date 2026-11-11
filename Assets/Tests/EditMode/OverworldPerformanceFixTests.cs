using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The certain fixes of Docs/PerformanceOverworld.md: the hero's requirement state captured at most once a second (or on an
    /// inventory / mount / hero event), the ground under fixed beacons sampled once, the spawner wake test reading the clearings
    /// of the chunk index instead of the whole world (same clearings, same order), and the <see cref="MechanicsDriverHub"/>
    /// keeping on only the driver of the region the hero is in (two at a border) plus any busy one.
    /// </summary>
    public class OverworldPerformanceFixTests
    {
        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.WorldId);

        // ------------------------------------------------------------------
        // HeroRequirementCache
        // ------------------------------------------------------------------

        [Test]
        public void HeroCache_DoesNotRecaptureWithinTheSameSecond()
        {
            var cache = new HeroRequirementCache();
            var source = new object();
            int captures = 0;
            MapRequirementState Capture(object _)
            {
                captures++;
                return new MapRequirementState(CharacterId.Maithe);
            }

            MapRequirementState first = cache.Get(10f, source, Capture);
            Assert.AreSame(first, cache.Get(10f, source, Capture));
            Assert.AreSame(first, cache.Get(10.5f, source, Capture));
            Assert.AreSame(first, cache.Get(10.99f, source, Capture));
            Assert.AreEqual(1, captures);
            Assert.AreEqual(1, cache.Captures);

            Assert.AreNotSame(first, cache.Get(11f, source, Capture), "a second later it is captured again");
            Assert.AreEqual(2, captures);
        }

        [Test]
        public void HeroCache_RecapturesWhenInvalidatedOrTheBagChanges()
        {
            var cache = new HeroRequirementCache();
            int captures = 0;
            MapRequirementState Capture(object _)
            {
                captures++;
                return new MapRequirementState();
            }

            var bag = new object();
            cache.Get(0f, bag, Capture);
            cache.Invalidate();
            cache.Get(0.1f, bag, Capture);
            Assert.AreEqual(2, captures, "invalidated");

            cache.Get(0.2f, new object(), Capture);
            Assert.AreEqual(3, captures, "another bag");

            cache.Get(0.1f, cache, Capture);
            cache.Get(0.05f, cache, Capture);
            Assert.AreEqual(5, captures, "a clock that went back (new session) captures again");
        }

        [Test]
        public void HeroCache_InventoryMountAndHeroEventsMarkItStale_OnlyWhileAttached()
        {
            var cache = new HeroRequirementCache();
            int captures = 0;
            MapRequirementState Capture(object _)
            {
                captures++;
                return new MapRequirementState();
            }

            var bag = new object();
            cache.Attach();
            try
            {
                cache.Get(0f, bag, Capture);
                EventBus.Publish(new InventoryChangedEvent());
                cache.Get(0.1f, bag, Capture);
                EventBus.Publish(new CharacterSelectedEvent(CharacterId.Icaro));
                cache.Get(0.2f, bag, Capture);
                EventBus.Publish(new Rapadura.Gameplay.Mounts.MountCollectionChangedEvent());
                cache.Get(0.3f, bag, Capture);
                Assert.AreEqual(4, captures);
            }
            finally
            {
                cache.Detach();
            }

            cache.Get(0.4f, bag, Capture);
            Assert.AreEqual(5, captures, "detaching drops the state");
            EventBus.Publish(new InventoryChangedEvent());
            cache.Get(0.5f, bag, Capture);
            Assert.AreEqual(5, captures, "a detached cache no longer listens");
        }

        [Test]
        public void HeroCache_LookupFollowsTheCapturedState()
        {
            var cache = new HeroRequirementCache();
            var withCloak = new MapRequirementState().Carry(MapRequirementItems.StormCloak);
            cache.Get(0f, cache, _ => withCloak);
            System.Func<string, bool> lookup = cache.Lookup;
            Assert.AreSame(lookup, cache.Lookup, "built once per capture");
            Assert.AreEqual(MapRequirementRules.IsMet(MapRequirementTags.StormProtection, withCloak), lookup(MapRequirementTags.StormProtection));

            cache.Get(5f, cache, _ => new MapRequirementState());
            Assert.IsFalse(cache.Lookup(MapRequirementTags.StormProtection));
        }

        // ------------------------------------------------------------------
        // StaticGroundCache
        // ------------------------------------------------------------------

        [Test]
        public void GroundCache_SamplesEachFixedPointOnce_WithTheSameHeight()
        {
            OverworldLayout layout = OverworldLayouts.Get(OverworldLayouts.ZarafshanId);
            var cache = new StaticGroundCache();
            Vector2 a = layout.Bounds.center + new Vector2(13.7f, -41.2f);
            Vector2 b = layout.Bounds.center + new Vector2(-220f, 95f);

            float ya = cache.GroundAt(layout, a);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(ya, cache.GroundAt(layout, a));
            }

            float yb = cache.GroundAt(layout, b);
            Assert.AreEqual(2, cache.Samples);
            Assert.AreEqual(Mathf.Max(0f, OverworldRules.SurfaceAt(layout, a)), ya);
            Assert.AreEqual(Mathf.Max(0f, OverworldRules.SurfaceAt(layout, b)), yb);
        }

        // ------------------------------------------------------------------
        // Spawners by the chunk index
        // ------------------------------------------------------------------

        [Test]
        public void SpawnsNear_WakesTheSameClearingsAsTheWholeWorld_InTheSameOrder()
        {
            OverworldLayout world = World;
            List<MapSpawnZone> zones = world.World.Zones;
            Assume.That(zones.Count, Is.GreaterThan(100));
            var near = new List<int>();
            var heroes = new List<Vector2>();

            // Just inside the wake edge of every clearing, in a direction of its own, plus points all over the world.
            var random = new System.Random(4217);
            foreach (MapSpawnZone zone in zones)
            {
                float angle = (float)(random.NextDouble() * Mathf.PI * 2f);
                heroes.Add(zone.Center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (zone.Radius + OverworldRules.SpawnerActiveRadius - 0.05f));
            }

            for (int i = 0; i < 400; i++)
            {
                heroes.Add(new Vector2(Mathf.Lerp(world.Bounds.xMin, world.Bounds.xMax, (float)random.NextDouble()),
                    Mathf.Lerp(world.Bounds.yMin, world.Bounds.yMax, (float)random.NextDouble())));
            }

            int woken = 0;
            foreach (Vector2 hero in heroes)
            {
                List<int> full = Enumerable.Range(0, zones.Count).Where(i => OverworldRules.SpawnerShouldRun(zones[i], hero, false)).ToList();
                OverworldChunkBuilder.SpawnsNear(world, hero, OverworldView.SpawnWakeQueryRadius, near);
                List<int> indexed = near.Where(i => OverworldRules.SpawnerShouldRun(zones[i], hero, false)).ToList();
                CollectionAssert.AreEqual(full, indexed, "hero at " + hero);
                woken += full.Count;
            }

            Assert.Greater(woken, zones.Count, "the edge points wake their clearing");
        }

        [Test]
        public void SpawnsNear_IsSortedWithoutRepeats_AndFarFewerThanTheWorld()
        {
            OverworldLayout world = World;
            var near = new List<int>();
            OverworldZone pomar = world.Zone(OverworldLayouts.ZarafshanAnchorZoneId);
            OverworldChunkBuilder.SpawnsNear(world, pomar.Origin, OverworldView.SpawnWakeQueryRadius, near);
            for (int i = 1; i < near.Count; i++)
            {
                Assert.Less(near[i - 1], near[i]);
            }

            Assert.Less(near.Count, world.World.Zones.Count / 10);
        }

        // ------------------------------------------------------------------
        // MechanicsDriverHub
        // ------------------------------------------------------------------

        /// <summary>Each driver of the overworld root and the (bounds, anchor) pairs its Start builds its areas from.</summary>
        private static readonly (string Driver, (Rect Bounds, string Anchor)[] Areas)[] Drivers =
        {
            ("Zarafshan", new[] { (OverworldLayouts.ZarafshanBounds, OverworldLayouts.ZarafshanAnchorZoneId) }),
            ("Areias", new[] { (OverworldLayouts.AreiasBounds, OverworldLayouts.AreiasAnchorZoneId) }),
            ("Kaggen", new[] { (OverworldLayouts.KaggenBounds, OverworldLayouts.KaggenAnchorZoneId) }),
            ("Limiar", new[] { (OverworldLayouts.LimiarBounds, OverworldLayouts.LimiarAnchorZoneId) }),
            ("CostaDeltaTeto", new[]
            {
                (OverworldLayouts.CostaBounds, OverworldLayouts.CostaAnchorZoneId), (OverworldLayouts.DeltaBounds, OverworldLayouts.DeltaAnchorZoneId),
                (OverworldLayouts.TetoBounds, OverworldLayouts.TetoAnchorZoneId),
            }),
            ("Mingcheng", new[] { (OverworldLayouts.MingchengBounds, OverworldLayouts.MingchengAnchorZoneId) }),
            ("Nartes", new[] { (OverworldLayouts.NartesBounds, OverworldLayouts.NartesAnchorZoneId) }),
            ("Farnkesh", new[] { (OverworldLayouts.FarnkeshBounds, OverworldLayouts.FarnkeshAnchorZoneId) }),
            ("MarGelado", new[] { (OverworldLayouts.MarGeladoBounds, OverworldLayouts.MarGeladoAnchorZoneId) }),
            ("Hinokura", new[] { (OverworldLayouts.HinokuraBounds, OverworldLayouts.HinokuraAnchorZoneId) }),
            ("Rio", new[] { (OverworldLayouts.RioBounds, OverworldLayouts.RioAnchorZoneId) }),
            ("Mistura", new[] { (OverworldLayouts.MisturaBounds, OverworldLayouts.MisturaAnchorZoneId) }),
            ("Profundo", new[] { (OverworldLayouts.ProfundoBounds, OverworldLayouts.ProfundoAnchorZoneId) }),
            ("Ceu", new[] { (OverworldLayouts.CeuBounds, OverworldLayouts.CeuAnchorZoneId) }),
        };

        private static Rect[] WorldAreasOf((Rect Bounds, string Anchor)[] areas, OverworldLayout world)
        {
            return areas.Select(a =>
            {
                OverworldZone anchor = world.Zone(a.Anchor);
                Assert.IsNotNull(anchor, a.Anchor);
                return MechanicsDriverHub.WorldArea(a.Bounds, anchor.Origin);
            }).ToArray();
        }

        [Test]
        public void Hub_ShouldRun_InsideOrWithinTheMargin_OrWhileBusy()
        {
            Rect[] areas = { Rect.MinMaxRect(0f, 0f, 100f, 100f) };
            Assert.IsTrue(MechanicsDriverHub.ShouldRun(areas, new Vector2(50f, 50f), MechanicsDriverHub.Margin, false));
            Assert.IsTrue(MechanicsDriverHub.ShouldRun(areas, new Vector2(-31f, 50f), MechanicsDriverHub.Margin, false));
            Assert.IsTrue(MechanicsDriverHub.ShouldRun(areas, new Vector2(131f, 131f), MechanicsDriverHub.Margin, false));
            Assert.IsFalse(MechanicsDriverHub.ShouldRun(areas, new Vector2(-33f, 50f), MechanicsDriverHub.Margin, false));
            Assert.IsTrue(MechanicsDriverHub.ShouldRun(areas, new Vector2(5000f, 5000f), MechanicsDriverHub.Margin, true), "a ride keeps it on");
            Assert.IsFalse(MechanicsDriverHub.ShouldRun(null, new Vector2(50f, 50f), MechanicsDriverHub.Margin, false));
        }

        [Test]
        public void Hub_PicksTheDriverOfTheRegionUnderTheHero_AndHardlyAnyOther()
        {
            OverworldLayout world = World;
            Rect[][] areas = Drivers.Select(d => WorldAreasOf(d.Areas, world)).ToArray();
            for (int d = 0; d < Drivers.Length; d++)
            {
                foreach (Rect area in areas[d])
                {
                    var running = new List<string>();
                    for (int other = 0; other < Drivers.Length; other++)
                    {
                        if (MechanicsDriverHub.ShouldRun(areas[other], area.center, MechanicsDriverHub.Margin, false))
                        {
                            running.Add(Drivers[other].Driver);
                        }
                    }

                    CollectionAssert.Contains(running, Drivers[d].Driver, "hero in the middle of " + Drivers[d].Driver);
                    Assert.LessOrEqual(running.Count, 2, "hero in the middle of " + Drivers[d].Driver + ": " + string.Join(", ", running));
                }
            }
        }

        [Test]
        public void Hub_EveryPointRunsTheDriversWhoseRegionHoldsIt()
        {
            OverworldLayout world = World;
            Rect[][] areas = Drivers.Select(d => WorldAreasOf(d.Areas, world)).ToArray();
            var random = new System.Random(77);
            for (int i = 0; i < 3000; i++)
            {
                var hero = new Vector2(Mathf.Lerp(world.Bounds.xMin, world.Bounds.xMax, (float)random.NextDouble()),
                    Mathf.Lerp(world.Bounds.yMin, world.Bounds.yMax, (float)random.NextDouble()));
                int running = 0;
                for (int d = 0; d < Drivers.Length; d++)
                {
                    bool inside = areas[d].Any(a => a.Contains(hero));
                    bool run = MechanicsDriverHub.ShouldRun(areas[d], hero, MechanicsDriverHub.Margin, false);
                    Assert.IsTrue(!inside || run, Drivers[d].Driver + " must run with the hero inside it at " + hero);
                    running += run ? 1 : 0;
                }

                Assert.Less(running, Drivers.Length / 2, "far fewer than all fourteen, at " + hero);
            }
        }

        [Test]
        public void Hub_SwitchesDriversByPosition_AndLeavesUnreadyOnesAlone()
        {
            // A MonoBehaviour from this editor-only test assembly cannot be added to a GameObject, so the hub's per-driver
            // decision is checked through its pure Decide (what Refresh applies to each driver).
            Rect[] west = { Rect.MinMaxRect(0f, 0f, 1000f, 1000f) };
            Rect[] east = { Rect.MinMaxRect(1000f, 0f, 2000f, 1000f) };

            Assert.AreEqual(true, MechanicsDriverHub.Decide(west, new Vector2(500f, 500f), false));
            Assert.AreEqual(false, MechanicsDriverHub.Decide(east, new Vector2(500f, 500f), false));
            Assert.IsNull(MechanicsDriverHub.Decide(null, new Vector2(500f, 500f), false), "no areas: left as it was");
            Assert.IsNull(MechanicsDriverHub.Decide(null, new Vector2(500f, 500f), true), "no areas, even busy: left as it was");

            Assert.IsTrue(MechanicsDriverHub.Decide(west, new Vector2(990f, 500f), false) == true
                && MechanicsDriverHub.Decide(east, new Vector2(990f, 500f), false) == true, "both at the border");

            Assert.AreEqual(false, MechanicsDriverHub.Decide(west, new Vector2(1500f, 500f), false));
            Assert.AreEqual(true, MechanicsDriverHub.Decide(east, new Vector2(1500f, 500f), false));
            Assert.AreEqual(true, MechanicsDriverHub.Decide(west, new Vector2(1500f, 500f), true), "a busy driver stays on");
        }
    }
}
