using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// A hero walking from map 01 to 100 with pure rules only (<see cref="ProgressionPathRules"/>): on foot through the stitched
    /// world, buying at the counters of the cities he walked into, doing the quests (cities, regions, Cartas da Estrada and the
    /// mid story when it exists), picking up the drops of the monsters and world bosses he met and crafting the recipes. Then each
    /// map of <see cref="MapCatalog"/>, in level order, is checked: (a) no hole in the level bands, (b) every gate has a real
    /// source, (c) the return city and the map's post/dock are reached, (d) no source of a gate depends on a later map.
    /// The holes found are listed in Docs/Status_100Mapas.md ("Buracos de progressão").
    /// </summary>
    public class ProgressionPathTests
    {
        private OverworldLayout _world;
        private List<ProgressionSource> _sources;
        private ProgressionPathResult _result;

        private static EquipmentSlot SlotOf(string itemId) => ItemDatabase.GetById(itemId)?.EquipSlot ?? EquipmentSlot.None;

        // The "depois do chefe" counter lines of Regions 1–4: the region's world boss event opens at its own level.
        private static int BossLevelOf(BiomeType biome) => WorldBossRules.For(biome)?.MinLevel ?? 1;

        [OneTimeSetUp]
        public void Walk()
        {
            _world = OverworldLayouts.StitchedWorld;
            Assert.That(_world, Is.Not.Null, "o mundo contínuo não foi montado");
            _sources = BuildSources(_world);
            _result = ProgressionPathRules.Simulate(_world, SlotOf, BossLevelOf, null, ProgressionPathRules.DefaultLevelsAboveHunt, _sources);
        }

        [Test]
        public void MapOrderCoversTheWholeCatalog()
        {
            List<MapDefinition> maps = ProgressionPathRules.MapsInLevelOrder();
            Assert.That(maps.Count, Is.EqualTo(MapCatalog.Maps.Count));
            Assert.That(maps[0].Id, Is.EqualTo(MapCatalog.PomarId), "a rota começa no mapa 01");
            for (int i = 1; i < maps.Count; i++)
            {
                Assert.That(maps[i].WalkLevelRequirement, Is.GreaterThanOrEqualTo(maps[i - 1].WalkLevelRequirement));
            }
        }

        [Test]
        public void WalkReachesLevel500AndEveryMap()
        {
            Assert.That(_result.Stuck, Is.False, $"o herói empacou no nível {_result.FinalLevel} (bicho mais forte alcançado: {_result.HuntCap})");
            Assert.That(_result.FinalLevel, Is.EqualTo(ProgressionPathRules.MaxLevel));
            List<string> closed = MapCatalog.Maps.Where(m => !_result.IsOpen(m.Id)).Select(m => m.Id).ToList();
            Assert.That(closed, Is.Empty, "mapas que o herói nunca abre: " + string.Join(", ", closed));
        }

        [Test]
        public void SourcesIncludeLootBossesAndQuests()
        {
            Assert.That(_sources.Any(s => s.Kind == ProgressionSourceKind.Shop), "sem lojas de cidade");
            Assert.That(_sources.Any(s => s.Kind == ProgressionSourceKind.CityQuest), "sem missões de cidade");
            Assert.That(_sources.Any(s => s.Kind == ProgressionSourceKind.LateStory), "sem As Cartas da Estrada");
            Assert.That(_sources.Any(s => s.Kind == ProgressionSourceKind.Loot), "sem loot de bicho");
            Assert.That(_sources.Count(s => s.Kind == ProgressionSourceKind.WorldBoss && s.ItemId == null), Is.EqualTo(RegionBossRules.AllSpawns.Count),
                "cada chefe do mundo entra como fonte (ao menos a vitória)");
        }

        /// <summary>The road itself: fails only for the real holes of the content, one line per map and what is missing.</summary>
        [Test]
        public void RoadFromMap01To100HasNoHoles()
        {
            List<ProgressionMapIssue> issues = ProgressionPathRules.CheckMaps(_world, _result, SlotOf, _sources);
            Assert.That(issues, Is.Empty,
                $"Buracos de progressão ({issues.Count}) — ver Docs/Status_100Mapas.md, \"Buracos de progressão\":\n" + string.Join("\n", issues));
        }

        /// <summary>The checker itself: with no waterskin anywhere, the Dunas (map 10, Água Fresca) must be flagged.</summary>
        [Test]
        public void CheckerFlagsAGateWithNoSource()
        {
            List<ProgressionSource> without = _sources.Where(s => s.ItemId != MapRequirementItems.Waterskin).ToList();
            ProgressionPathResult walk = ProgressionPathRules.Simulate(_world, SlotOf, BossLevelOf, null, ProgressionPathRules.DefaultLevelsAboveHunt, without);
            List<ProgressionMapIssue> issues = ProgressionPathRules.CheckMaps(_world, walk, SlotOf, without);
            Assert.That(issues.Any(i => i.MapId == "map_010_dunas" && i.Check == "b" && i.Message.Contains(MapRequirementTags.FreshWater)),
                "sem odre em lugar nenhum, o mapa 10 tinha de acusar a exigência sem fonte:\n" + string.Join("\n", issues));
        }

        /// <summary>The checker itself: a hero that never walked opens no map, and each one is flagged (a) with its id.</summary>
        [Test]
        public void CheckerFlagsMapsNeverOpened()
        {
            var empty = new ProgressionPathResult { FinalLevel = 1, FinalLookup = _ => false };
            List<ProgressionMapIssue> issues = ProgressionPathRules.CheckMaps(_world, empty, SlotOf, _sources);
            foreach (MapDefinition map in MapCatalog.Maps)
            {
                Assert.That(issues.Any(i => i.MapId == map.Id && i.Check == "a"), $"{map.Id} devia acusar que nunca abre");
            }
        }

        /// <summary>Every source the game has: the catalogs (shops, quests, mid story) plus drops, world bosses and recipes from Resources.</summary>
        private static List<ProgressionSource> BuildSources(OverworldLayout world)
        {
            var list = new List<ProgressionSource>(ProgressionItemSources.All);
            var enemies = new HashSet<string>(StringComparer.Ordinal);
            foreach (OverworldZone zone in world.Zones)
            {
                foreach (OverworldFauna fauna in zone.Fauna)
                {
                    foreach (var entry in fauna.Day.Concat(fauna.Night).Concat(fauna.Dusk ?? Enumerable.Empty<Rapadura.Gameplay.Night.PeriodSpawnEntry>()))
                    {
                        if (entry != null && !string.IsNullOrEmpty(entry.EnemyId))
                        {
                            enemies.Add(entry.EnemyId);
                        }
                    }
                }
            }

            var drops = new List<(string, string)>();
            foreach (string enemyId in enemies)
            {
                foreach (string item in DropsOf(enemyId))
                {
                    drops.Add((enemyId, item));
                }
            }

            list.AddRange(ProgressionItemSources.LootSources(drops));

            var bosses = new List<(string, string, int, string)>();
            foreach (RegionBossSpawn spawn in RegionBossRules.AllSpawns)
            {
                bosses.Add((spawn.EnemyId, spawn.MapId, spawn.Level, null));
                foreach (string item in DropsOf(spawn.EnemyId))
                {
                    bosses.Add((spawn.EnemyId, spawn.MapId, spawn.Level, item));
                }
            }

            list.AddRange(ProgressionItemSources.BossSources(bosses));

            var recipes = new List<(string, string, int, IReadOnlyList<string>)>();
            foreach (RecipeDefinition recipe in RecipeDatabase.GetAll())
            {
                if (recipe == null || recipe.ResultItem == null)
                {
                    continue;
                }

                IReadOnlyList<string> needs = (recipe.Ingredients ?? Array.Empty<RecipeIngredient>()).Where(i => i.item != null).Select(i => i.item.ItemId).ToList();
                recipes.Add((recipe.RecipeId, recipe.ResultItem.ItemId, recipe.MinimumPlayerLevel, needs));
            }

            list.AddRange(ProgressionItemSources.RecipeSources(recipes));
            return list;
        }

        /// <summary>The items an enemy can drop: its own loot item, the boss bonus drop and its biome's loot table.</summary>
        private static IEnumerable<string> DropsOf(string enemyId)
        {
            EnemyDefinition enemy = Resources.Load<EnemyDefinition>(EncounterPayload.EnemyResourcesFolder + "/" + enemyId);
            if (enemy == null)
            {
                yield break;
            }

            if (enemy.LootItem != null)
            {
                yield return enemy.LootItem.ItemId;
            }

            if (enemy.BossBonusDropItem != null)
            {
                yield return enemy.BossBonusDropItem.ItemId;
            }

            BiomeDefinition biome = BiomeRegistry.FindBiomeOf(enemy);
            if (biome == null)
            {
                yield break;
            }

            foreach (BiomeLootEntry entry in biome.Loot)
            {
                if (entry?.Item != null)
                {
                    yield return entry.Item.ItemId;
                }
            }
        }
    }
}
