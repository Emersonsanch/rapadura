using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Audio;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.UI.HUD;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The area the hero is in (Docs/Mapas100.md §0.10, "Área atual"): the pure <see cref="OverworldAreaTracker"/> with
    /// hysteresis and its <see cref="AreaChangedEvent"/>, the pt-BR / en names of every area, the climate and music of every
    /// kind of ground (<see cref="OverworldAreaAmbience"/>, <see cref="AreaMusicRules"/>) and the letreiro's texts
    /// (<see cref="AreaBannerController"/>).
    /// </summary>
    public class OverworldAreaTests
    {
        private static readonly string[] RegionIds = { OverworldLayouts.ZarafshanId, OverworldLayouts.AreiasId, OverworldLayouts.KaggenId, OverworldLayouts.LimiarId };

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.WorldId);

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            MapLayouts.ClearCache();
            OverworldLayouts.ClearCache();
        }

        // ------------------------------------------------------------------
        // Tracker with hysteresis
        // ------------------------------------------------------------------

        [Test]
        public void Tracker_SwitchesOnlyPastTheHysteresis_AndDoesNotFlickerOnTheBorder()
        {
            OverworldLayout layout = OverworldLayouts.Get(OverworldLayouts.ZarafshanId);
            (OverworldZone a, OverworldZone b, Vector2 edge, Vector2 normal) = FindBorder(layout, samePart: true);
            var tracker = new OverworldAreaTracker(layout);
            var raised = new List<AreaChangedEvent>();
            tracker.AreaChanged += raised.Add;

            Assert.IsTrue(tracker.Update(edge - normal * 10f, out AreaChangedEvent first), "The first reading sets the area.");
            Assert.AreSame(a, first.Current);
            Assert.IsNull(first.Previous);
            Assert.IsTrue(first.Teleported, "The first reading after a reset counts as a teleport.");

            // Wobbling across the border inside the hysteresis never switches.
            float h = OverworldAreaTracker.DefaultHysteresis;
            foreach (float d in new[] { -1f, 1f, -0.5f, h * 0.5f, 0.2f, h - 0.5f, -2f, 1f })
            {
                Assert.IsFalse(tracker.Update(edge + normal * d, out _), $"Switched at {d} m from the border (hysteresis {h} m).");
                Assert.AreSame(a, tracker.Zone);
            }

            Assert.IsTrue(tracker.Update(edge + normal * (h + 2f), out AreaChangedEvent walked), "Past the hysteresis the area switches.");
            Assert.AreSame(a, walked.Previous);
            Assert.AreSame(b, walked.Current);
            Assert.IsTrue(walked.ZoneChanged);
            Assert.IsFalse(walked.Teleported);

            // Back over the border a little: still b.
            Assert.IsFalse(tracker.Update(edge - normal * 2f, out _));
            Assert.AreSame(b, tracker.Zone);

            // A teleport (arrival, barca, respawn) reads the position as is.
            tracker.Reset();
            Assert.IsTrue(tracker.Update(edge - normal * 2f, out AreaChangedEvent teleported));
            Assert.AreSame(a, teleported.Current);
            Assert.IsTrue(teleported.Teleported);
            Assert.AreEqual(3, raised.Count, "The C# event fires once per change.");
        }

        [Test]
        public void Tracker_InTheStitchedWorld_CarriesTheRegionOnTheEvent()
        {
            OverworldLayout world = World;
            (OverworldZone a, OverworldZone b, Vector2 edge, Vector2 normal) = FindBorder(world, samePart: false);
            var tracker = new OverworldAreaTracker(world);
            tracker.Update(edge - normal * 10f, out AreaChangedEvent first);
            Assert.AreSame(world.PartAt(edge - normal * 10f), first.CurrentPart);

            Assert.IsFalse(tracker.Update(edge + normal * 1f, out _), "The region border also has hysteresis.");
            Assert.IsTrue(tracker.Update(edge + normal * 12f, out AreaChangedEvent crossed));
            Assert.IsTrue(crossed.RegionChanged, $"{a} -> {b} crosses two regions.");
            Assert.AreSame(b, crossed.Current);
            Assert.AreSame(world.PartAt(edge + normal * 12f), crossed.CurrentPart);
        }

        [Test]
        public void Tracker_EnteringATown_IsAnAreaChange_LeavingItHasHysteresis()
        {
            OverworldLayout layout = OverworldLayouts.Get(OverworldLayouts.ZarafshanId);
            OverworldSettlement vila = layout.Vila;
            var tracker = new OverworldAreaTracker(layout);
            Rect safe = vila.SafeZone;
            float x = safe.center.x;
            Vector2 outside = new Vector2(x, safe.yMin - 20f);
            Assume.That(OverworldRules.ZoneAt(layout, outside), Is.Not.Null);
            tracker.Update(outside, out _);
            Assert.IsNull(tracker.Settlement);

            Assert.IsTrue(tracker.Update(new Vector2(x, safe.yMin + 1f), out AreaChangedEvent entered), "Stepping over the town's edge enters it.");
            Assert.IsTrue(entered.EnteredSettlement);
            Assert.AreSame(vila, entered.CurrentSettlement);
            Assert.IsTrue(AreaBannerController.ShouldAnnounce(entered), "Stepping into a town gets the letreiro.");

            Assert.IsFalse(tracker.Update(new Vector2(x, safe.yMin - 2f), out _), "Two steps back out are inside the hysteresis.");
            Assert.AreSame(vila, tracker.Settlement);
            Assert.IsTrue(tracker.Update(new Vector2(x, safe.yMin - OverworldAreaTracker.DefaultHysteresis - 2f), out AreaChangedEvent left));
            Assert.IsNull(left.CurrentSettlement);
            Assert.AreSame(vila, left.PreviousSettlement);
        }

        [Test]
        public void ShouldAnnounce_NewAreaOrTown_NotLeavingATownIntoTheSameArea()
        {
            OverworldLayout layout = OverworldLayouts.Get(OverworldLayouts.ZarafshanId);
            OverworldZone pomar = layout.Zone("pomar");
            OverworldZone campos = layout.Zone("campos_do_aryk");
            Assert.IsNotNull(pomar);
            Assert.IsNotNull(campos);
            Assert.IsTrue(AreaBannerController.ShouldAnnounce(new AreaChangedEvent(layout.Id, null, pomar, null, null, null, null, true)));
            Assert.IsTrue(AreaBannerController.ShouldAnnounce(new AreaChangedEvent(layout.Id, pomar, campos, null, null, null, null, false)));
            Assert.IsTrue(AreaBannerController.ShouldAnnounce(new AreaChangedEvent(layout.Id, pomar, pomar, null, null, null, layout.Vila, false)));
            Assert.IsFalse(AreaBannerController.ShouldAnnounce(new AreaChangedEvent(layout.Id, pomar, pomar, null, null, layout.Vila, null, false)));
            Assert.IsFalse(AreaBannerController.ShouldAnnounce(new AreaChangedEvent(layout.Id, pomar, null, null, null, null, null, false)));
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void EveryArea_HasItsNameInPtBrAndEn()
        {
            Dictionary<string, LocalizationEntry> entries = OverworldTexts.Entries.ToDictionary(e => e.Key);
            var problems = new List<string>();
            foreach (OverworldLayout layout in RegionIds.Select(OverworldLayouts.Get).Append(World))
            {
                Assert.IsNotNull(layout);
                foreach (OverworldZone zone in layout.Zones)
                {
                    if (string.IsNullOrWhiteSpace(zone.NamePt) || string.IsNullOrWhiteSpace(zone.NameEn))
                    {
                        problems.Add($"{layout.Id}/{zone.Id}: no own name");
                    }

                    if (!entries.TryGetValue(zone.NameKey, out LocalizationEntry entry) || string.IsNullOrWhiteSpace(entry.PtBR) || string.IsNullOrWhiteSpace(entry.En)
                        || entry.PtBR == zone.NameKey || entry.En == zone.NameKey)
                    {
                        problems.Add($"{layout.Id}/{zone.Id}: {zone.NameKey} missing in OverworldTexts");
                    }
                }
            }

            CollectionAssert.IsEmpty(problems, string.Join("\n", problems));
        }

        [Test]
        public void LetreiroTexts_ExistInBothLanguages_AndFormatTheBand()
        {
            foreach (string key in new[] { OverworldTexts.AreaBand, OverworldTexts.AreaSafe, OverworldTexts.AreaMissing, OverworldTexts.AreaLabel })
            {
                LocalizationEntry entry = OverworldTexts.Entries.FirstOrDefault(e => e.Key == key);
                Assert.IsNotNull(entry, key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), key);
            }

            OverworldZone zone = World.Zones.First(z => z.MaxLevel > z.MinLevel);
            Assert.AreEqual($"Nv {zone.MinLevel}–{zone.MaxLevel}", AreaBannerController.BandText(null, zone), "pt-BR fallback without a manager.");
            StringAssert.StartsWith(OverworldTexts.ZoneName(null, zone), AreaBannerController.LabelFor(null, zone, null));
        }

        [Test]
        public void Warning_NamesTheMissingGear_AndTheDanger_ButNotInTown()
        {
            OverworldZone gated = World.Zones.FirstOrDefault(z => !string.IsNullOrEmpty(z.RequirementTag));
            Assert.IsNotNull(gated, "Some area of the world has a special gate.");
            string missing = AreaBannerController.WarningText(null, gated, null, 999, hasGear: false);
            StringAssert.Contains(MapTexts.RequirementName(null, gated.RequirementTag), missing);
            Assert.AreEqual(string.Empty, AreaBannerController.WarningText(null, gated, null, 999, hasGear: true));

            OverworldZone high = World.Zones.OrderByDescending(z => z.MaxLevel).First();
            StringAssert.Contains(MapTexts.Get(null, MapTexts.DangerWarning), AreaBannerController.WarningText(null, high, null, 1, hasGear: true));
            Assert.AreEqual(string.Empty, AreaBannerController.WarningText(null, high, World.Vila, 1, hasGear: false), "No warning inside a town.");
        }

        // ------------------------------------------------------------------
        // Climate and music
        // ------------------------------------------------------------------

        [Test]
        public void EveryTerrain_HasAClimate_AndEveryAreaUsesOne()
        {
            foreach (OverworldTerrain terrain in Enum.GetValues(typeof(OverworldTerrain)))
            {
                Assert.IsTrue(OverworldAreaAmbience.Covers(terrain), $"{terrain} has no climate line in OverworldAreaAmbience.");
            }

            foreach (OverworldZone zone in World.Zones)
            {
                Assert.IsTrue(OverworldAreaAmbience.Covers(zone.Terrain), zone.Id);
            }

            Assert.AreEqual(WeatherType.Snow, OverworldAreaAmbience.ClimateOf(OverworldTerrain.Summit).Weather);
            Assert.AreEqual(WeatherType.Sandstorm, OverworldAreaAmbience.ClimateOf(OverworldTerrain.Erg).Weather);
            Assert.AreEqual(WeatherType.Fog, OverworldAreaAmbience.ClimateOf(OverworldTerrain.Marsh).Weather);
            Assert.IsNull(OverworldAreaAmbience.ClimateOf(OverworldTerrain.Orchard).Weather, "Calm ground lets the biome roll its weather.");
        }

        [Test]
        public void Mood_TownThenDangerThenGround()
        {
            OverworldLayout layout = OverworldLayouts.Get(OverworldLayouts.ZarafshanId);
            OverworldZone pomar = layout.Zone("pomar");
            OverworldZone high = layout.Zones.OrderByDescending(z => z.MaxLevel).First();
            Assert.AreEqual(AreaMood.Town, OverworldAreaAmbience.MoodOf(high, layout.Vila, 1));
            Assert.AreEqual(AreaMood.Danger, OverworldAreaAmbience.MoodOf(high, null, 1));
            Assert.AreNotEqual(AreaMood.Danger, OverworldAreaAmbience.MoodOf(high, null, high.MaxLevel));
            Assert.AreEqual(AreaMood.Field, OverworldAreaAmbience.MoodOf(pomar, null, 1));
        }

        [Test]
        public void EveryMoodAndTime_PicksAnExistingTrack()
        {
            SfxEventMap defaults = SfxEventMap.CreateDefault();
            SfxEventMap asset = Resources.Load<SfxEventMap>("Audio/SfxEventMap");
            try
            {
                foreach (AreaMood mood in Enum.GetValues(typeof(AreaMood)))
                {
                    foreach (bool night in new[] { false, true })
                    {
                        foreach (BiomeType biome in new[] { BiomeType.Forest, BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves })
                        {
                            AreaMusicRules.Pick(mood, night, biome, out MusicContext context, out string variant);
                            Assert.AreNotEqual(MusicContext.None, context);
                            Assert.IsTrue(defaults.TryGetMusic(context, variant, out SfxEventMap.MusicEntry entry), $"{mood}/{night}/{biome}: no default track {context}/{variant}");
                            Assert.AreEqual(SfxEventMap.Normalize(variant), SfxEventMap.Normalize(entry.variant), $"{mood}/{night}/{biome}: {context}/{variant} falls back to another variant");
                            if (asset != null)
                            {
                                Assert.IsTrue(asset.TryGetMusic(context, variant, out _), $"{mood}/{night}/{biome}: the SfxEventMap asset has no {context}/{variant}");
                            }
                        }
                    }
                }

                AreaMusicRules.Pick(AreaMood.Town, false, BiomeType.Forest, out MusicContext town, out _);
                AreaMusicRules.Pick(AreaMood.Field, false, BiomeType.Forest, out MusicContext fieldDay, out _);
                AreaMusicRules.Pick(AreaMood.Field, true, BiomeType.Forest, out MusicContext fieldNight, out _);
                AreaMusicRules.Pick(AreaMood.Danger, false, BiomeType.Caves, out MusicContext danger, out string dangerVariant);
                Assert.AreEqual(MusicContext.Title, town);
                Assert.AreEqual(MusicContext.WorldMap, fieldDay);
                Assert.AreEqual(MusicContext.Story, fieldNight);
                Assert.AreEqual(MusicContext.Encounter, danger);
                Assert.AreEqual("caves", dangerVariant);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(defaults);
            }
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// Two neighbouring zones (open passage) and the middle of their shared edge with the normal from a to b, away from any
        /// town, with 14 m of each zone on both sides; in the same region or across two regions of the stitched world.
        /// </summary>
        private static (OverworldZone A, OverworldZone B, Vector2 Edge, Vector2 Normal) FindBorder(OverworldLayout layout, bool samePart)
        {
            foreach (OverworldZone a in layout.Zones)
            {
                foreach (OverworldZone b in OverworldRules.Neighbours(layout, a))
                {
                    if (!TryEdge(a.Bounds, b.Bounds, out Vector2 edge, out Vector2 normal))
                    {
                        continue;
                    }

                    Vector2 inA = edge - normal * 14f;
                    Vector2 inB = edge + normal * 14f;
                    bool samePartHere = layout.PartAt(inA) == layout.PartAt(inB);
                    if (samePartHere != samePart || OverworldRules.ZoneAt(layout, inA) != a || OverworldRules.ZoneAt(layout, inB) != b
                        || OverworldRules.DistanceToSafety(layout, edge) < 30f)
                    {
                        continue;
                    }

                    return (a, b, edge, normal);
                }
            }

            Assert.Fail($"{layout.Id}: no border between two areas fits the test.");
            return default;
        }

        private static bool TryEdge(Rect a, Rect b, out Vector2 edge, out Vector2 normal)
        {
            edge = default;
            normal = default;
            float yMin = Mathf.Max(a.yMin, b.yMin), yMax = Mathf.Min(a.yMax, b.yMax);
            float xMin = Mathf.Max(a.xMin, b.xMin), xMax = Mathf.Min(a.xMax, b.xMax);
            if (Mathf.Approximately(a.xMax, b.xMin) && yMax - yMin > 30f)
            {
                edge = new Vector2(a.xMax, (yMin + yMax) * 0.5f);
                normal = Vector2.right;
                return true;
            }

            if (Mathf.Approximately(a.xMin, b.xMax) && yMax - yMin > 30f)
            {
                edge = new Vector2(a.xMin, (yMin + yMax) * 0.5f);
                normal = Vector2.left;
                return true;
            }

            if (Mathf.Approximately(a.yMax, b.yMin) && xMax - xMin > 30f)
            {
                edge = new Vector2((xMin + xMax) * 0.5f, a.yMax);
                normal = Vector2.up;
                return true;
            }

            if (Mathf.Approximately(a.yMin, b.yMax) && xMax - xMin > 30f)
            {
                edge = new Vector2((xMin + xMax) * 0.5f, a.yMin);
                normal = Vector2.down;
                return true;
            }

            return false;
        }
    }
}
