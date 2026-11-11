using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The Barcas de Luz window (M, <see cref="FastTravelWindowController"/>) with the 100 maps in 16 regions: every catalog map is
    /// listed under its region, the region / level filters never leave the window empty, every region has a name (pt-BR + en)
    /// and a colour, the window opens on a sensible region (also from an overworld zone without a catalog map), every row builds
    /// without a null, and the card fits phone resolutions (touch targets, narrow mode, a sideways filter strip).
    /// </summary>
    public class MapTravelWindowTests
    {
        private const string UssPath = "Assets/Resources/UI/FastTravelWindow.uss";
        private const string UxmlPath = "Assets/Resources/UI/FastTravelWindow.uxml";
        private const string PanelSettingsPath = "Assets/UI/DefaultPanelSettings.asset";

        // ------------------------------------------------------------------
        // Every map, every region
        // ------------------------------------------------------------------

        [Test]
        public void TheAllFilter_ListsEveryCatalogMapOnce_UnderItsRegion()
        {
            var listed = new List<string>();
            foreach (MapRegion region in FastTravelWindowController.RegionsInView(null))
            {
                foreach (MapDefinition map in FastTravelWindowController.MapsInView(region, false, 1))
                {
                    Assert.AreEqual(region, map.Region, map.Id);
                    listed.Add(map.Id);
                }
            }

            Assert.AreEqual(MapCatalog.Maps.Count, listed.Count);
            Assert.AreEqual(100, listed.Count, "the catalog has the 100 maps");
            CollectionAssert.AreEquivalent(MapCatalog.Maps.Select(m => m.Id), listed);
            CollectionAssert.AllItemsAreUnique(listed);
        }

        [Test]
        public void EveryEnumRegion_IsInTheCatalog_HasMaps_AndItsOwnFilterShowsOnlyIt()
        {
            foreach (MapRegion region in System.Enum.GetValues(typeof(MapRegion)))
            {
                CollectionAssert.Contains(MapCatalog.Regions, region);
                CollectionAssert.AreEqual(new[] { region }, FastTravelWindowController.RegionsInView(region));
                List<MapDefinition> maps = FastTravelWindowController.MapsInView(region, false, 1);
                Assert.IsNotEmpty(maps, region.ToString());
                Assert.IsTrue(maps.All(m => m.Region == region), region.ToString());
            }

            Assert.AreEqual(16, MapCatalog.Regions.Count);
        }

        [Test]
        public void EveryRegion_HasANameInPtBrAndEn_AndADistinctColour()
        {
            var colours = new HashSet<Color32>();
            foreach (MapRegion region in MapCatalog.Regions)
            {
                string key = MapTexts.RegionKey(region);
                LocalizationEntry entry = MapTexts.Entries.FirstOrDefault(e => e.Key == key);
                Assert.IsNotNull(entry, key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), key + " pt-BR");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), key + " en");
                Assert.AreNotEqual(key, MapTexts.RegionName(null, region));

                Assert.IsTrue(MapRegionColors.HasColor(region), region + " has no colour");
                Assert.IsTrue(colours.Add(MapRegionColors.Of(region)), region + " shares its colour");

                int count = MapCatalog.InRegion(region).Count();
                StringAssert.StartsWith(count + " mapas", FastTravelWindowController.RegionSummary(null, region));
            }
        }

        [Test]
        public void WindowTexts_HavePtBrAndEn()
        {
            CollectionAssert.AllItemsAreUnique(MapTravelWindowTexts.Entries.Select(e => e.Key));
            foreach (LocalizationEntry entry in MapTravelWindowTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), entry.Key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), entry.Key);
                Assert.AreEqual(entry.PtBR, MapTravelWindowTexts.Get(null, entry.Key));
            }
        }

        // ------------------------------------------------------------------
        // Filters
        // ------------------------------------------------------------------

        [Test]
        public void TheLevelFilter_NeverEmptiesTheAllView_FromLevel1To500()
        {
            for (int level = 1; level <= 500; level += 3)
            {
                int shown = FastTravelWindowController.RegionsInView(null)
                    .Sum(region => FastTravelWindowController.MapsInView(region, true, level).Count);
                Assert.Greater(shown, 0, "level " + level);
            }
        }

        [Test]
        public void TheLevelFilter_KeepsTheMapTheHeroStandsOn()
        {
            MapDefinition sky = MapCatalog.Maps.Last();
            List<MapDefinition> maps = FastTravelWindowController.MapsInView(sky.Region, true, 1, sky.Id);
            CollectionAssert.Contains(maps, sky);
            Assert.IsFalse(FastTravelWindowController.FitsLevel(sky, 1));
            Assert.IsTrue(FastTravelWindowController.FitsLevel(sky, sky.MonsterLevelMin));
            Assert.IsFalse(FastTravelWindowController.FitsLevel(null, 1));
        }

        [Test]
        public void CycleRegion_WalksAllThenEveryRegion_AndWraps()
        {
            MapRegion? filter = null;
            var seen = new List<MapRegion?>();
            for (int i = 0; i < MapCatalog.Regions.Count + 1; i++)
            {
                filter = FastTravelWindowController.CycleRegion(filter, 1);
                seen.Add(filter);
            }

            Assert.IsNull(seen.Last(), "17 steps come back to Todas");
            CollectionAssert.AreEqual(MapCatalog.Regions.Select(r => (MapRegion?)r), seen.Take(MapCatalog.Regions.Count));
            Assert.AreEqual(MapCatalog.Regions.Last(), FastTravelWindowController.CycleRegion(null, -1));
            Assert.IsNull(FastTravelWindowController.CycleRegion(MapCatalog.Regions[0], -1));
        }

        [Test]
        public void DefaultRegion_IsTheMapsRegion_ThenTheOverworldZonesRegion_ThenTheHerosLevel()
        {
            foreach (MapDefinition map in MapCatalog.Maps)
            {
                Assert.AreEqual(map.Region, FastTravelWindowController.DefaultRegion(map.Id, null, 1), map.Id);
            }

            // A zone of the continuous world without a map of its own (an overworld-only id): the region it lies in, never null.
            Assert.AreEqual(MapRegion.MarGelado, FastTravelWindowController.DefaultRegion("zona_sem_mapa", MapRegion.MarGelado, 1));
            Assert.AreEqual(MapRegion.Zarafshan, FastTravelWindowController.DefaultRegion(null, null, 1));
            Assert.AreEqual(MapRegion.Zarafshan, FastTravelWindowController.DefaultRegion(string.Empty, null, 0));
            Assert.AreEqual(MapRegion.ReinosDoCeu, FastTravelWindowController.DefaultRegion(null, null, 500));
        }

        // ------------------------------------------------------------------
        // Rows
        // ------------------------------------------------------------------

        [Test]
        public void EveryCatalogMap_BuildsARow_AtLevel1And500_WithoutANull()
        {
            foreach (MapDefinition map in MapCatalog.Maps)
            {
                foreach (int level in new[] { 1, 250, 500 })
                {
                    MapTravelRow row = FastTravelWindowController.BuildMapRow(map, level, null, 0, false, null, false, _ => false, null);
                    Assert.IsFalse(string.IsNullOrEmpty(row.Name), map.Id);
                    Assert.IsFalse(string.IsNullOrEmpty(row.Meta), map.Id);
                    Assert.IsFalse(string.IsNullOrEmpty(row.Requirement), map.Id);
                    Assert.IsFalse(string.IsNullOrEmpty(row.ButtonText), map.Id);
                    StringAssert.DoesNotContain("map.", row.Name, map.Id + " has no name text");
                    if (row.Check == MapAccessCheck.MissingRequirement)
                    {
                        string hint = MapTexts.RequirementHint(null, map.SpecialRequirementTag);
                        Assert.AreNotEqual(MapTexts.RequirementHintKey(map.SpecialRequirementTag), hint, map.Id + ": no 'where to get it' hint");
                    }
                }
            }
        }

        [Test]
        public void EverySpecialRequirement_SaysWhereToGetIt()
        {
            foreach (string tag in MapCatalog.Maps.Where(m => m.HasSpecialRequirement).Select(m => m.SpecialRequirementTag).Distinct())
            {
                Assert.AreNotEqual(MapTexts.RequirementKey(tag), MapTexts.RequirementName(null, tag), tag);
                Assert.AreNotEqual(MapTexts.RequirementHintKey(tag), MapTexts.RequirementHint(null, tag), tag);
            }
        }

        // ------------------------------------------------------------------
        // Phone / PC layout budget (no rendering in EditMode: the USS numbers against the panel size)
        // ------------------------------------------------------------------

        private static readonly Vector2Int[] Screens =
        {
            new Vector2Int(1920, 1080), new Vector2Int(1280, 720), new Vector2Int(3840, 2160), new Vector2Int(1024, 768),
            new Vector2Int(2400, 1080), new Vector2Int(1080, 2400), new Vector2Int(2532, 1170), new Vector2Int(1170, 2532),
            new Vector2Int(720, 1600), new Vector2Int(1600, 720), new Vector2Int(1080, 2520), new Vector2Int(2048, 1536),
        };

        [Test]
        public void TheCardAndItsRows_FitEveryScreen_PhoneAndPc()
        {
            (Vector2 reference, float match) = ReadPanelScaling();
            string uss = File.ReadAllText(UssPath);
            float goWide = Px(uss, ".ftw-go", "min-width");
            float goNarrow = Px(uss, ".ftw-card--narrow .ftw-go", "min-width");
            float close = Px(uss, ".ftw-close", "min-width");
            float lockWidth = Px(uss, ".ftw-row__lock", "width") + Px(uss, ".ftw-row__lock", "margin-right");
            const float rootPadding = 14f * 2f;
            const float cardPadding = 18f * 2f;
            const float rowChrome = 10f * 2f + 2f * 2f + 14f; // row padding, border, a vertical scrollbar
            const float minBodyWidth = 180f;

            foreach (Vector2Int screen in Screens)
            {
                Vector2 panel = FastTravelWindowController.PanelSizeFor(screen.x, screen.y, reference, match);
                float card = Mathf.Min((panel.x - rootPadding) * 0.96f, 920f);
                float inner = card - cardPadding;
                float rowInner = inner - rowChrome;
                bool narrow = card < FastTravelWindowController.NarrowCardWidth;

                Assert.Greater(inner, close + 160f, screen + ": the title has no room beside Fechar");
                if (narrow)
                {
                    Assert.LessOrEqual(goNarrow, rowInner, screen + ": the stacked Mover button leaves the row");
                }
                else
                {
                    Assert.LessOrEqual(lockWidth + minBodyWidth + goWide + 10f, rowInner, screen + ": the row's Mover button leaves the card");
                }

                // Height: header + chips + filter strip + status must leave room to scroll at least three rows.
                float cardHeight = Mathf.Min((panel.y - rootPadding) * 0.94f, 760f) - 28f;
                float fixedParts = 90f + 44f + Px(uss, ".ftw-filters", "max-height") + 6f + 28f;
                Assert.GreaterOrEqual(cardHeight - fixedParts, 3f * Px(uss, ".ftw-row", "min-height"), screen + ": the list has no room");
            }
        }

        [Test]
        public void TouchTargets_AreAtLeast48px_AndTheFilterStripScrollsSideways()
        {
            string uss = File.ReadAllText(UssPath);
            Assert.GreaterOrEqual(Px(uss, ".ftw-filter", "min-height"), 48f);
            Assert.GreaterOrEqual(Px(uss, ".ftw-go", "min-height"), 48f);
            Assert.GreaterOrEqual(Px(uss, ".ftw-close", "min-height"), 48f);
            Assert.GreaterOrEqual(Px(uss, ".ftw-compass", "min-height"), 48f);
            StringAssert.Contains("white-space: nowrap", Block(uss, ".ftw-filter"), "chips keep one line (the strip scrolls)");

            string uxml = File.ReadAllText(UxmlPath);
            StringAssert.IsMatch("name=\"ftw-filters\"[^>]*mode=\"Horizontal\"", uxml);
            StringAssert.IsMatch("name=\"ftw-list\"[^>]*horizontal-scroller-visibility=\"Hidden\"", uxml);
            Assert.Less(uxml.IndexOf("ftw-filters"), uxml.IndexOf("name=\"ftw-list\""), "the filters sit above the list");
        }

        private static (Vector2 reference, float match) ReadPanelScaling()
        {
            string asset = File.ReadAllText(PanelSettingsPath);
            Match res = Regex.Match(asset, @"m_ReferenceResolution: \{x: ([\d.]+), y: ([\d.]+)\}");
            Match match = Regex.Match(asset, @"m_Match: ([\d.]+)");
            Assert.IsTrue(res.Success && match.Success, "DefaultPanelSettings scaling not found");
            return (new Vector2(float.Parse(res.Groups[1].Value, CultureInfo.InvariantCulture), float.Parse(res.Groups[2].Value, CultureInfo.InvariantCulture)),
                float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture));
        }

        private static string Block(string uss, string selector)
        {
            Match m = Regex.Match(uss, "(^|\\n)" + Regex.Escape(selector) + "\\s*\\{([^}]*)\\}");
            Assert.IsTrue(m.Success, selector + " not in the USS");
            return m.Groups[2].Value;
        }

        private static float Px(string uss, string selector, string property)
        {
            Match m = Regex.Match(Block(uss, selector), "(^|\\s)" + Regex.Escape(property) + ":\\s*([\\d.]+)px");
            Assert.IsTrue(m.Success, selector + " has no " + property);
            return float.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
        }
    }
}
