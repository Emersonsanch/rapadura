using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The MU-style maps (Docs/Mapas100.md, Docs/MetodologiaMapasERacas.md Part I–II): access rules (walk/warp level, hero
    /// bonuses, the Barcas de Luz fare, the order of the checks, the monster level delta), the 42-map catalog against the
    /// doc's table, the two built layouts (01 Pomar da Vó Edna, 02 Tugai do Rio Dourado) and the rows of the travel window
    /// (M), including that a fresh level-1 hero can actually reach both built maps.
    /// </summary>
    public class MapAccessRulesTests
    {
        /// <summary>The same map, not built (03 is playable since the Zarafshan maps; its gate and fare still read the same).</summary>
        private static MapDefinition Unbuilt(int mapId)
        {
            MapDefinition m = MapCatalog.Get(mapId);
            return new MapDefinition(m.MapId, m.Id, m.Region, m.RegionType, m.Size, m.MonsterLevelMin, m.MonsterLevelMax, m.WalkLevelRequirement,
                m.WarpLevelRequirement, m.BaseWarpGoldFee, m.SpecialRequirementTag, m.ReturnCityId, m.Settlement, m.SettlementId, isBuilt: false);
        }

        // ------------------------------------------------------------------
        // MapAccessRules
        // ------------------------------------------------------------------

        [Test]
        public void WarpCost_IsBasePlusLevelTimesFifteen_DoubledWhenOverloaded_FreeForTheTutorialMap()
        {
            Assert.AreEqual(50 + 5 * 15, MapAccessRules.CalculateWarpCost(5, false));
            Assert.AreEqual(2 * (50 + 5 * 15), MapAccessRules.CalculateWarpCost(5, true));
            Assert.AreEqual(50 + 15, MapAccessRules.CalculateWarpCost(0, false), "level clamps to 1");
            Assert.AreEqual(0, MapAccessRules.CalculateWarpCost(0, 30, true), "0 base = free, overloaded or not");
            Assert.AreEqual(0, MapAccessRules.CalculateWarpCost(MapCatalog.Get(MapCatalog.PomarId), true));
            Assert.AreEqual(125, MapAccessRules.CalculateWarpCost(MapCatalog.Get(MapCatalog.TugaiId), false));
            Assert.AreEqual(250, MapAccessRules.CalculateWarpCost(MapCatalog.Get(MapCatalog.TugaiId), true));
            Assert.AreEqual(0, MapAccessRules.CalculateWarpCost((MapDefinition)null, false));
        }

        [Test]
        public void Overloaded_OnlyAboveFullCapacity()
        {
            Assert.IsFalse(MapAccessRules.IsOverloaded(0.5f));
            Assert.IsFalse(MapAccessRules.IsOverloaded(1f));
            Assert.IsTrue(MapAccessRules.IsOverloaded(1.01f));
        }

        [Test]
        public void CanWarp_ChecksLevelBeforeGold_AndNamesTheReason()
        {
            Assert.IsFalse(MapAccessRules.CanWarpToMap(4, 5, 0, 125, out string reason));
            Assert.AreEqual(MapTexts.ReasonLevel, reason);
            Assert.IsFalse(MapAccessRules.CanWarpToMap(5, 5, 124, 125, out reason));
            Assert.AreEqual(MapTexts.ReasonGold, reason);
            Assert.IsTrue(MapAccessRules.CanWarpToMap(5, 5, 125, 125, out reason));
            Assert.AreEqual(string.Empty, reason);
        }

        [Test]
        public void HeroBonus_IcaroInRuins_MaitheInMountains_FiveLevels_NeverBelowOne()
        {
            Assert.AreEqual(15, MapAccessRules.EffectiveLevelRequirement(20, CharacterId.Icaro, MapRegionTypes.Ruins));
            Assert.AreEqual(20, MapAccessRules.EffectiveLevelRequirement(20, CharacterId.Icaro, MapRegionTypes.Mountain));
            Assert.AreEqual(15, MapAccessRules.EffectiveLevelRequirement(20, CharacterId.Maithe, MapRegionTypes.Mountain));
            Assert.AreEqual(20, MapAccessRules.EffectiveLevelRequirement(20, CharacterId.Maithe, MapRegionTypes.Ruins));
            Assert.AreEqual(20, MapAccessRules.EffectiveLevelRequirement(20, CharacterId.Joaquim, MapRegionTypes.Ruins));
            Assert.AreEqual(20, MapAccessRules.EffectiveLevelRequirement(20, null, MapRegionTypes.Ruins));
            Assert.AreEqual(1, MapAccessRules.EffectiveLevelRequirement(3, CharacterId.Icaro, MapRegionTypes.Ruins));
            Assert.AreEqual(1, MapAccessRules.EffectiveLevelRequirement(0, null, MapRegionTypes.Forest));

            // Map 08 (Ruínas dos Artífices, walk 6): Ícaro may walk in at level 1, Lavine only at 6.
            MapDefinition ruins = MapCatalog.Get(8);
            Assert.IsTrue(MapAccessRules.CanWalkIntoMap(1, ruins, CharacterId.Icaro));
            Assert.IsFalse(MapAccessRules.CanWalkIntoMap(5, ruins, CharacterId.Lavine));
            Assert.IsTrue(MapAccessRules.CanWalkIntoMap(6, ruins, CharacterId.Lavine));
            Assert.IsFalse(MapAccessRules.CanWalkIntoMap(99, (MapDefinition)null, null));
        }

        [Test]
        public void CheckWarp_FollowsTheOrder_HereBusyBuiltLevelGateGold()
        {
            MapDefinition tugai = MapCatalog.Get(MapCatalog.TugaiId);
            Assert.AreEqual(MapAccessCheck.NotBuilt, MapAccessRules.CheckWarp(null, 99, null, 9999, false, null, false, null));
            Assert.AreEqual(MapAccessCheck.Here, MapAccessRules.CheckWarp(tugai, 1, null, 0, false, MapCatalog.TugaiId, true, null));
            Assert.AreEqual(MapAccessCheck.Busy, MapAccessRules.CheckWarp(tugai, 1, null, 0, false, null, true, null));
            Assert.AreEqual(MapAccessCheck.LevelTooLow, MapAccessRules.CheckWarp(tugai, 4, null, 9999, false, null, false, null));
            Assert.AreEqual(MapAccessCheck.NotEnoughGold, MapAccessRules.CheckWarp(tugai, 5, null, 124, false, null, false, null));
            Assert.AreEqual(MapAccessCheck.NotEnoughGold, MapAccessRules.CheckWarp(tugai, 5, null, 249, true, null, false, null), "overloaded fare");
            Assert.AreEqual(MapAccessCheck.Ok, MapAccessRules.CheckWarp(tugai, 5, null, 125, false, null, false, null));

            // Not built comes before level; the gate before gold.
            MapDefinition brejo = Unbuilt(3);
            Assert.AreEqual(MapAccessCheck.NotBuilt, MapAccessRules.CheckWarp(brejo, 1, null, 0, false, null, false, null));
            var gated = new MapDefinition(99, "map_099_test", MapRegion.Zarafshan, MapRegionTypes.Swamp, MapSize.M, 16, 22, 14, 18,
                MapAccessRules.BaseWarpFee, MapRequirementTags.LeatherBoots, CityCatalog.VilaZefa, MapSettlementKind.Outpost, "x", isBuilt: true);
            Assert.AreEqual(MapAccessCheck.MissingRequirement, MapAccessRules.CheckWarp(gated, 18, null, 0, false, null, false, null));
            Assert.AreEqual(MapAccessCheck.MissingRequirement, MapAccessRules.CheckWarp(gated, 18, null, 0, false, null, false, tag => false));
            Assert.AreEqual(MapAccessCheck.NotEnoughGold, MapAccessRules.CheckWarp(gated, 18, null, 0, false, null, false, tag => tag == MapRequirementTags.LeatherBoots));
            Assert.AreEqual(MapAccessCheck.Ok, MapAccessRules.CheckWarp(gated, 18, null, 9999, false, null, false, tag => true));
        }

        [Test]
        public void CheckWalk_UsesTheWalkLevel_AndTheGate_NoGold()
        {
            MapDefinition tugai = MapCatalog.Get(MapCatalog.TugaiId);
            Assert.AreEqual(MapAccessCheck.LevelTooLow, MapAccessRules.CheckWalk(tugai, 1, null, null));
            Assert.AreEqual(MapAccessCheck.Ok, MapAccessRules.CheckWalk(tugai, 2, null, null), "on foot from level 2, the barca only from 5");
            Assert.AreEqual(MapAccessCheck.NotBuilt, MapAccessRules.CheckWalk(Unbuilt(3), 99, null, null));
            Assert.AreEqual(MapAccessCheck.NotBuilt, MapAccessRules.CheckWalk(null, 99, null, null));
            Assert.IsTrue(MapAccessRules.MeetsRequirement(MapRequirementTags.None, null));
            Assert.IsFalse(MapAccessRules.MeetsRequirement(MapRequirementTags.SunSeal, null));
        }

        [Test]
        public void MonsterLevelDelta_CritFromTen_HitAndDamageFromTwenty_LethalFromThirty()
        {
            MonsterLevelPenalty same = MapAccessRules.GetMonsterLevelDeltaPenalty(10, 10);
            Assert.IsTrue(same.IsNone);
            Assert.IsTrue(MapAccessRules.GetMonsterLevelDeltaPenalty(30, 5).IsNone, "monsters below the hero never get a bonus");

            MonsterLevelPenalty ten = MapAccessRules.GetMonsterLevelDeltaPenalty(10, 20);
            Assert.AreEqual(10, ten.Delta);
            Assert.AreEqual(0.25f, ten.MonsterCritChanceBonus, 1e-6f);
            Assert.AreEqual(1f, ten.PlayerHitRateMultiplier);
            Assert.AreEqual(1f, ten.DamageTakenMultiplier);
            Assert.IsFalse(ten.IsLethal);

            MonsterLevelPenalty twenty = MapAccessRules.GetMonsterLevelDeltaPenalty(10, 30);
            Assert.AreEqual(0.6f, twenty.PlayerHitRateMultiplier, 1e-6f);
            Assert.AreEqual(1.5f, twenty.DamageTakenMultiplier, 1e-6f);

            MonsterLevelPenalty thirty = MapAccessRules.GetMonsterLevelDeltaPenalty(10, 40);
            Assert.AreEqual(3f, thirty.DamageTakenMultiplier, 1e-6f);
            Assert.IsTrue(thirty.IsLethal);
        }

        [Test]
        public void RollMonsterLevel_StaysInTheBand_Inclusive()
        {
            Assert.AreEqual(2, MapAccessRules.RollMonsterLevel(2, 5, 0d));
            Assert.AreEqual(5, MapAccessRules.RollMonsterLevel(2, 5, 1d));
            Assert.AreEqual(5, MapAccessRules.RollMonsterLevel(2, 5, 0.99d));
            Assert.AreEqual(3, MapAccessRules.RollMonsterLevel(2, 5, 0.3d));
            Assert.AreEqual(4, MapAccessRules.RollMonsterLevel(5, 3, 0.5d), "swapped bounds");
            Assert.AreEqual(1, MapAccessRules.RollMonsterLevel(0, 0, 0.5d), "never below 1");
            var seen = new HashSet<int>();
            var random = new System.Random(7);
            for (int i = 0; i < 400; i++)
            {
                int level = MapAccessRules.RollMonsterLevel(1, 3, random.NextDouble());
                Assert.That(level, Is.InRange(1, 3));
                seen.Add(level);
            }

            Assert.AreEqual(3, seen.Count, "every level of the band shows up");
        }

        // ------------------------------------------------------------------
        // MapCatalog
        // ------------------------------------------------------------------

        [Test]
        public void Catalog_Has42MapsAndRegions9To16_UniqueIdsAndNumbers_InOrder()
        {
            // The 42 maps of the "até 100" scope (01–42), Região 9's six (47–52), Região 10's six (53–58), Região 11's six (59–64), Região 12's six
            // (65–70), Região 13's six (71–76), Região 14's six (77–82), Região 15's eight (83–90) and Região 16's ten (91–100), ☆, walked as their continuous regions.
            Assert.AreEqual(96, MapCatalog.Maps.Count);
            Assert.AreEqual(MapCatalog.Maps.Count, MapCatalog.Maps.Select(m => m.Id).Distinct().Count(), "unique ids");
            Assert.AreEqual(MapCatalog.Maps.Count, MapCatalog.Maps.Select(m => m.MapId).Distinct().Count(), "unique numbers");
            for (int i = 0; i < MapCatalog.Maps.Count; i++)
            {
                MapDefinition map = MapCatalog.Maps[i];
                Assert.AreEqual(i < 42 ? i + 1 : i + 5, map.MapId);
                StringAssert.IsMatch(@"^map_\d{3}_[a-z_]+$", map.Id);
                StringAssert.StartsWith($"map_{map.MapId:000}_", map.Id);
                Assert.AreSame(map, MapCatalog.Get(map.Id));
                Assert.AreSame(map, MapCatalog.Get(map.MapId));
            }

            Assert.IsNull(MapCatalog.Get("map_999_nada"));
            Assert.IsNull(MapCatalog.Get(null));
        }

        [Test]
        public void Catalog_Levels_AreCoherent()
        {
            foreach (MapDefinition map in MapCatalog.Maps)
            {
                Assert.LessOrEqual(map.MonsterLevelMin, map.MonsterLevelMax, map.Id);
                Assert.LessOrEqual(map.WalkLevelRequirement, map.WarpLevelRequirement, $"{map.Id}: on foot never asks more than the barca");
                Assert.LessOrEqual(map.WalkLevelRequirement, map.MonsterLevelMin, $"{map.Id}: the walk level never exceeds the weakest monster");
                Assert.LessOrEqual(map.WarpLevelRequirement, map.MonsterLevelMax + 2, $"{map.Id}: the barca level stays near the band");
                Assert.That(map.BaseWarpGoldFee, Is.EqualTo(0).Or.EqualTo(MapAccessRules.BaseWarpFee), map.Id);
                Assert.IsTrue(map.SpecialRequirementTag.Length == 0 || MapRequirementTags.All.Contains(map.SpecialRequirementTag), $"{map.Id}: unknown gate");
                Assert.Contains(map.ReturnCityId, MapCatalog.ReturnCityIds.ToList(), map.Id);
                Assert.Contains(map.Region, MapCatalog.Regions.ToList(), map.Id);
                Assert.IsFalse(string.IsNullOrEmpty(map.SettlementId), map.Id);
            }

            // Only the tutorial map is free; maps 01–02 are built (more maps join the built list as their layouts land).
            Assert.AreEqual(new[] { MapCatalog.PomarId }, MapCatalog.Maps.Where(m => m.BaseWarpGoldFee == 0).Select(m => m.Id).ToArray());
            CollectionAssert.IsSubsetOf(new[] { MapCatalog.PomarId, MapCatalog.TugaiId }, MapCatalog.Built.Select(m => m.Id).ToArray());
            // A map that hosts a city is a respawn point; its return city is a city.
            Assert.IsTrue(MapCatalog.Get(MapCatalog.PomarId).HasCity);
            Assert.IsFalse(MapCatalog.Get(MapCatalog.TugaiId).HasCity);
        }

        [Test]
        public void Catalog_MatchesTheMapas100Table_BandsNamesRegionsAndReturnCities()
        {
            string path = Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? string.Empty, "Docs", "Mapas100.md");
            if (!File.Exists(path))
            {
                Assert.Ignore("Docs/Mapas100.md not found");
            }

            var rows = new Dictionary<int, Match>();
            var row = new Regex(@"^\|\s*(\d{2,3})\s*\|\s*([^|]+?)\s*\|[^|]*\|\s*([^|]+?)\s*\|\s*(\d+)[–-](\d+)\s*\|[^|]*\|\s*([^|]+?)\s*\|\s*(★|☆)", RegexOptions.Multiline);
            foreach (Match match in row.Matches(File.ReadAllText(path)))
            {
                rows[int.Parse(match.Groups[1].Value)] = match;
            }

            foreach (MapDefinition map in MapCatalog.Maps)
            {
                Assert.IsTrue(rows.TryGetValue(map.MapId, out Match doc), $"map {map.MapId:00} missing from Mapas100.md §1");
                Assert.AreEqual(map.MapId <= 42 ? "★" : "☆", doc.Groups[7].Value, $"{map.Id}: in the 'até 100' scope (Regiões 9 to 16 are ☆)");
                Assert.AreEqual(int.Parse(doc.Groups[4].Value), map.MonsterLevelMin, $"{map.Id}: band min");
                Assert.AreEqual(int.Parse(doc.Groups[5].Value), map.MonsterLevelMax, $"{map.Id}: band max");
                Assert.AreEqual(doc.Groups[2].Value, MapTexts.MapName(null, map), $"{map.Id}: pt-BR name");
                Assert.AreEqual(doc.Groups[3].Value, MapTexts.RegionName(null, map.Region), $"{map.Id}: region");
                Assert.AreEqual(doc.Groups[6].Value, MapTexts.Get(null, MapTexts.ReturnCityKey(map.ReturnCityId)), $"{map.Id}: return city");
            }
        }

        [Test]
        public void Texts_EveryMapRegionSettlementCityAndGate_HasPtBrAndEnglish()
        {
            var keys = new List<string>();
            foreach (MapDefinition map in MapCatalog.Maps)
            {
                keys.Add(map.MapNameKey);
                keys.Add(map.SettlementNameKey);
                keys.Add(MapTexts.ReturnCityKey(map.ReturnCityId));
            }

            keys.AddRange(MapCatalog.Regions.Select(MapTexts.RegionKey));
            keys.AddRange(MapRequirementTags.All.Select(MapTexts.RequirementKey));
            keys.AddRange(MapRequirementTags.All.Select(MapTexts.RequirementHintKey));
            keys.AddRange(MapCatalog.ReturnCityIds.Select(MapTexts.ReturnCityKey));
            var entries = MapTexts.Entries.ToDictionary(e => e.Key);
            Assert.AreEqual(MapTexts.Entries.Count, entries.Count, "duplicate text keys");
            foreach (string key in keys.Distinct())
            {
                Assert.IsTrue(entries.TryGetValue(key, out var entry), $"missing text {key}");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), key);
            }

            Assert.AreEqual("Barcas de Luz · Mover", MapTexts.Get(null, MapTexts.WindowTitle));
        }

        [Test]
        public void RespawnCity_IsTheMapsReturnCity_OrTheVila()
        {
            Assert.AreEqual(CityCatalog.VilaZefa, MapCatalog.ReturnCityOf(MapCatalog.PomarId));
            Assert.AreEqual(CityCatalog.VilaZefa, MapCatalog.ReturnCityOf(MapCatalog.TugaiId));
            Assert.AreEqual(CityCatalog.VilaZefa, MapCatalog.RespawnCityFor(MapCatalog.TugaiId).CityId);
            Assert.AreEqual(CityCatalog.AreiaBranca, MapCatalog.RespawnCityFor("map_007_caravanas").CityId);
            Assert.IsNotNull(MapCatalog.RespawnCityFor("map_041_mingcheng"), "a planned city falls back to an existing one");
        }

        // ------------------------------------------------------------------
        // MapLayoutRules + the two built layouts
        // ------------------------------------------------------------------

        [Test]
        public void Geometry_DistancesToSegmentsAndPolylines()
        {
            Assert.AreEqual(0f, MapLayoutRules.DistanceToSegment(new Vector2(5f, 0f), Vector2.zero, new Vector2(10f, 0f)), 1e-5f);
            Assert.AreEqual(3f, MapLayoutRules.DistanceToSegment(new Vector2(5f, 3f), Vector2.zero, new Vector2(10f, 0f)), 1e-5f);
            Assert.AreEqual(5f, MapLayoutRules.DistanceToSegment(new Vector2(13f, 4f), Vector2.zero, new Vector2(10f, 0f)), 1e-5f, "past the end: to the end point");
            Assert.AreEqual(2f, MapLayoutRules.DistanceToSegment(new Vector2(0f, 2f), Vector2.zero, Vector2.zero), 1e-5f, "degenerate segment");
            var line = new List<Vector2> { Vector2.zero, new Vector2(10f, 0f), new Vector2(10f, 10f) };
            Assert.AreEqual(1f, MapLayoutRules.DistanceToPolyline(new Vector2(11f, 5f), line), 1e-5f);
        }

        [Test]
        public void BuiltLayouts_AreValid_AndOnlyForBuiltMaps()
        {
            // Built ⇔ playable: its own hand-made layout OR a zone of a continuous region (09–12, 15–18, 21–24 exist only there).
            foreach (MapDefinition map in MapCatalog.Maps)
            {
                bool ownLayout = MapLayouts.Has(map.Id);
                bool overworldZone = OverworldLayouts.ForMap(map.Id) != null;
                Assert.AreEqual(map.IsBuilt, ownLayout || overworldZone, $"{map.Id}: built ⇔ (own layout or overworld zone)");
                Assert.AreEqual(map.IsBuilt, MapLayouts.IsPlayable(map.Id), $"{map.Id}: IsPlayable mirrors the flag");
                Assert.AreEqual(!ownLayout && overworldZone, MapLayouts.IsOverworldOnly(map.Id), map.Id);
            }

            MapLayouts.ClearCache();
            foreach (MapDefinition map in MapCatalog.Built.Where(m => MapLayouts.Has(m.Id)))
            {
                MapLayout layout = MapLayouts.For(map.Id);
                Assert.IsNotNull(layout, map.Id);
                Assert.AreSame(layout, MapLayouts.For(map.Id), "cached");
                List<string> problems = MapLayoutRules.Validate(layout);
                Assert.IsEmpty(problems, string.Join("\n", problems));
            }

            Assert.IsNull(MapLayouts.For("map_042_torres_brancas"));
            Assert.IsNull(MapLayouts.For(null));
        }

        [Test]
        public void Validate_FlagsABandOutsideTheCatalogBand()
        {
            MapLayouts.ClearCache();
            MapLayout layout = MapLayouts.For(MapCatalog.PomarId);
            MapSpawnZone zone = layout.Zones[0];
            int max = zone.MaxLevel;
            try
            {
                zone.MaxLevel = 9;
                Assert.IsTrue(MapLayoutRules.Validate(layout).Any(p => p.Contains("level band")));
            }
            finally
            {
                zone.MaxLevel = max;
                MapLayouts.ClearCache();
            }

            Assert.IsNotEmpty(MapLayoutRules.Validate(null));
        }

        [Test]
        public void BuiltLayouts_RiversBlockExceptAtCrossings_AndPropsKeepOffTrails()
        {
            MapLayouts.ClearCache();
            MapLayout tugai = MapLayouts.For(MapCatalog.TugaiId);
            Assert.IsNotEmpty(tugai.Rivers, "the golden river");
            foreach (MapRiver river in tugai.Rivers)
            {
                Assert.IsNotEmpty(river.Crossings, "the river can be crossed");
                foreach (MapCrossing crossing in river.Crossings)
                {
                    Assert.IsTrue(MapLayoutRules.OnCrossing(river, crossing.At));
                    foreach ((Vector2 center, float length, float yaw) in MapLayoutRules.RiverBlockers(river))
                    {
                        Assert.IsFalse(MapLayoutRules.OnCrossing(river, center), $"blocker on the crossing at {crossing.At}");
                    }
                }
            }

            foreach (MapLayout layout in new[] { MapLayouts.For(MapCatalog.PomarId), tugai })
            {
                Assert.IsNotEmpty(layout.WildProps, layout.MapId);
                foreach (MapPath path in layout.Paths)
                {
                    Vector2 middle = (path.A + path.B) * 0.5f;
                    Assert.IsFalse(MapLayoutRules.IsFreeForProp(layout, middle, 0.5f), $"{layout.MapId}: a trail is never free for a prop");
                }
            }
        }

        [Test]
        public void BuiltLayouts_SpawnOnlyExistingNonBossMonsters_InEveryPeriod()
        {
            MapLayouts.ClearCache();
            foreach (MapDefinition map in MapCatalog.Built.Where(m => MapLayouts.Has(m.Id)))
            {
                foreach (MapSpawnZone zone in MapLayouts.For(map.Id).Zones)
                {
                    foreach (var entry in zone.AllEntries())
                    {
                        Assert.IsFalse(string.IsNullOrEmpty(entry.EnemyId), $"{map.Id}/{zone.Id}");
                        EnemyDefinition definition = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{entry.EnemyId}");
                        Assert.IsNotNull(definition, $"{map.Id}/{zone.Id}: enemy '{entry.EnemyId}' not in Resources/Enemies");
                        Assert.IsFalse(definition.IsBoss, $"{map.Id}/{zone.Id}: the open map never spawns a boss ({entry.EnemyId})");
                    }
                }
            }
        }

        // ------------------------------------------------------------------
        // Reachability: a fresh hero gets to 01 and 02 through the game's flow
        // ------------------------------------------------------------------

        [Test]
        public void FreshHero_TakesTheFreeBarcaToThePomar_ThenWalksIntoTheTugaiAtLevelTwo()
        {
            MapLayouts.ClearCache();
            MapDefinition pomar = MapCatalog.Get(MapCatalog.PomarId);
            MapDefinition tugai = MapCatalog.Get(MapCatalog.TugaiId);

            // Island map, level 1, no gold: the window's Pomar row is open and free.
            MapTravelRow pomarRow = FastTravelWindowController.BuildMapRow(pomar, 1, CharacterId.Lavine, 0, false, null, false, null, null);
            Assert.IsTrue(pomarRow.CanTravel);
            Assert.AreEqual(0, pomarRow.Cost);

            // The Pomar and the Tugai are neighbouring areas of ONE continuous map (Docs/Mapas100.md §0.10): an open passage
            // (no scene change), walkable from level 2 (the soft barrier pushes a level-1 hero back), the barca from 5.
            OverworldLayouts.ClearCache();
            OverworldLayout world = OverworldLayouts.ForMap(pomar.Id);
            Assert.IsNotNull(world, "the Pomar is part of the Zarafshan overworld");
            Assert.AreSame(world, OverworldLayouts.ForMap(tugai.Id), "the Tugai is in the same overworld");
            OverworldZone pomarZone = world.Zone("pomar");
            OverworldZone tugaiZone = world.Zone("tugai_margem");
            Assert.IsTrue(OverworldRules.AreNeighbours(pomarZone, tugaiZone), "Pomar ↔ Tugai: open passage");
            Assert.IsFalse(world.World.Exits.Exists(e => e.Kind == MapExitKind.Map && world.ContainsMap(e.TargetMapId)), "no scene-change exit inside the region");
            Assert.AreEqual(MapAccessCheck.LevelTooLow, OverworldRules.CheckEnter(tugaiZone, 1, CharacterId.Lavine, null));
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(tugaiZone, 2, CharacterId.Lavine, null));
            Assert.AreEqual(MapAccessCheck.Ok, OverworldRules.CheckEnter(pomarZone, 1, CharacterId.Lavine, null));

            // The overworld has the Vila's dock (arrival by barca), the Tugai's dock and the portal back to the island map.
            Assert.IsTrue(world.SettlementOf(pomar.Id).HasDock && world.SettlementOf(tugai.Id).HasDock, "Cais da Barca in the Vila and the Moinho");
            Assert.AreEqual(world.SettlementOf(tugai.Id).DockArrival, world.ArrivalFor(tugai.Id, null, out _), "by barca: the Moinho's dock");
            Assert.IsTrue(world.World.Exits.Exists(e => e.Kind == MapExitKind.WorldMap), "back to the island map");

            // The standalone layouts (server/legacy) keep their docks and their way back to the island map.
            MapLayout pomarLayout = MapLayouts.For(pomar.Id);
            MapLayout tugaiLayout = MapLayouts.For(tugai.Id);
            foreach (MapLayout layout in new[] { pomarLayout, tugaiLayout })
            {
                Assert.IsTrue(layout.Exits.Exists(e => e.Kind == MapExitKind.Dock), $"{layout.MapId}: Cais da Barca");
                Assert.IsTrue(layout.Exits.Exists(e => e.Kind == MapExitKind.WorldMap), $"{layout.MapId}: back to the island map");
            }

            // The Pomar hosts the Vila (respawn at the plaza); a defeat in the Tugai wakes the hero in the Vila too.
            Assert.IsTrue(pomarLayout.SettlementIsReturnCity);
            Assert.AreEqual(CityCatalog.VilaZefa, MapCatalog.RespawnCityFor(tugai.Id).CityId);

            // The bands of 01–02 fit a fresh hero: nothing on them is "dangerous" for level 1 by the warning rule.
            Assert.IsTrue(MapAccessRules.GetMonsterLevelDeltaPenalty(1, pomar.MonsterLevelMax).IsNone);
            Assert.IsTrue(MapAccessRules.GetMonsterLevelDeltaPenalty(1, tugai.MonsterLevelMax).IsNone);
        }

        // ------------------------------------------------------------------
        // FastTravelWindowController rows (pure)
        // ------------------------------------------------------------------

        [Test]
        public void WindowRow_OpenFreeMap_SaysMover()
        {
            MapTravelRow row = FastTravelWindowController.BuildMapRow(MapCatalog.Get(MapCatalog.PomarId), 1, null, 0, false, null, false, null, null);
            Assert.AreEqual(MapAccessCheck.Ok, row.Check);
            Assert.IsFalse(row.IsLocked);
            Assert.AreEqual("Mover", row.ButtonText);
            Assert.AreEqual("01 · Pomar da Vó Edna", row.Name);
            Assert.AreEqual("Barca: grátis · a pé desde o nível 1", row.Requirement);
            StringAssert.StartsWith("Nível 1", row.Meta);
            StringAssert.EndsWith("Vila da Vó Edna", row.Meta);
            Assert.AreEqual(string.Empty, row.Status);
        }

        [Test]
        public void WindowRow_LockedByLevel_ShowsTheMuLine_AndTheWalkHint()
        {
            MapTravelRow row = FastTravelWindowController.BuildMapRow(MapCatalog.Get(MapCatalog.TugaiId), 2, null, 9999, false, null, false, null, null);
            Assert.AreEqual(MapAccessCheck.LevelTooLow, row.Check);
            Assert.IsTrue(row.IsLocked);
            Assert.IsFalse(row.CanTravel);
            Assert.AreEqual(125, row.Cost);
            Assert.AreEqual("Exige Nível 5 (ou a pé nível 2) - Barca: 125 ouro", row.Requirement);
            Assert.AreEqual("Nível 5 para a barca", row.Status);
            Assert.AreEqual("Você pode tentar ir a pé pelo mapa vizinho a partir do nível 2.", row.Hint);
            Assert.AreEqual("Mover · 125 ouro", row.ButtonText);
        }

        [Test]
        public void WindowRow_GoldOverloadHereAndNotBuilt()
        {
            MapDefinition tugai = MapCatalog.Get(MapCatalog.TugaiId);
            MapTravelRow poor = FastTravelWindowController.BuildMapRow(tugai, 5, null, 124, false, null, false, null, null);
            Assert.AreEqual(MapAccessCheck.NotEnoughGold, poor.Check);
            Assert.IsFalse(poor.IsLocked, "short of gold is not a padlock");
            Assert.AreEqual("Ouro insuficiente (passagem: 125)", poor.Status);

            MapTravelRow heavy = FastTravelWindowController.BuildMapRow(tugai, 5, null, 200, true, null, false, null, null);
            Assert.AreEqual(250, heavy.Cost);
            Assert.AreEqual(MapAccessCheck.NotEnoughGold, heavy.Check);

            MapTravelRow ok = FastTravelWindowController.BuildMapRow(tugai, 5, null, 125, false, null, false, null, null);
            Assert.IsTrue(ok.CanTravel);
            Assert.AreEqual("Barca: 125 ouro · a pé desde o nível 2", ok.Requirement);

            MapTravelRow here = FastTravelWindowController.BuildMapRow(tugai, 5, null, 999, false, MapCatalog.TugaiId, false, null, null);
            Assert.AreEqual(MapAccessCheck.Here, here.Check);
            Assert.AreEqual("Vocês estão aqui", here.Status);

            MapTravelRow closed = FastTravelWindowController.BuildMapRow(Unbuilt(3), 1, null, 0, false, null, false, null, null);
            Assert.AreEqual(MapAccessCheck.NotBuilt, closed.Check);
            Assert.IsTrue(closed.IsLocked);
            StringAssert.StartsWith("Exige Nível 18 (ou a pé nível 14) - Barca: 320 ouro", closed.Requirement);
            StringAssert.EndsWith("Exige: Botas de couro", closed.Requirement);
        }

        [Test]
        public void WindowRow_HeroBonus_LowersTheShownLevels()
        {
            // Map 08 (Ruins, walk 6 / warp 10): Ícaro sees 5 / 1.
            MapDefinition ruins = MapCatalog.Get(8);
            MapTravelRow icaro = FastTravelWindowController.BuildMapRow(ruins, 1, CharacterId.Icaro, 0, false, null, false, null, null);
            StringAssert.StartsWith("Exige Nível 5 (ou a pé nível 1)", icaro.Requirement);
            MapTravelRow maria = FastTravelWindowController.BuildMapRow(ruins, 1, CharacterId.Maria, 0, false, null, false, null, null);
            StringAssert.StartsWith("Exige Nível 10 (ou a pé nível 6)", maria.Requirement);
        }
    }
}
