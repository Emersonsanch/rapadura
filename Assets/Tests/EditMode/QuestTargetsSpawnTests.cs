using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Every beast a quest asks to beat still lives somewhere the cousins can walk to. The passes of own beasts (EarlyOwn, NorthOwn, DeepOwn,
    /// WestOwn...) keep taking borrowed beasts out of the clearings and trails, and a quest that still asks for a beast that left its place can never
    /// finish. This reads what the continuous world really draws (<see cref="OverworldLayouts.Get"/> of every region: the clearings and trail groups of
    /// <see cref="OverworldLayout.World"/>, every period), the maps that are still their own scene (<see cref="MapLayouts.For"/>) and the bosses the
    /// <see cref="RegionBossRules.DirectorSpawns"/> keeps, and checks the kill targets of the city quests, the old region quests, both story chains and
    /// the house chain (its quests, its levels and the forge recipes that wait for a boss): each spawns somewhere; a quest with a place (a map, a city's maps, a zone) finds its beast in that place's region or the next one
    /// along the road; a quest of a city's own map finds it on one of the city's maps; a story step or a road quest finds it in its own zone.
    /// </summary>
    public class QuestTargetsSpawnTests
    {
        /// <summary>One place a beast is drawn: its region, the catalog map (or owner) and the overworld zone of the clearing.</summary>
        public readonly struct SpawnSite
        {
            public readonly MapRegion Region;
            public readonly string MapId;
            public readonly string ZoneId;
            public readonly bool IsBoss;

            public SpawnSite(MapRegion region, string mapId, string zoneId, bool isBoss)
            {
                Region = region;
                MapId = mapId ?? string.Empty;
                ZoneId = zoneId ?? string.Empty;
                IsBoss = isBoss;
            }

            public override string ToString() => $"{Region}/{MapId}/{ZoneId}{(IsBoss ? " (boss)" : string.Empty)}";
        }

        /// <summary>One kill target of a quest and the place the quest sends the cousins to (any of them may be empty).</summary>
        public sealed class KillTarget
        {
            public string Source;
            public string QuestId;
            public string ObjectiveId;
            public string EnemyId;

            /// <summary>The region of the quest (null: the quest has no place in the world).</summary>
            public MapRegion? Region;

            /// <summary>The maps the beast should live on (a story step's map, a road quest's map, a city's maps); empty when the quest has none.</summary>
            public string[] Maps = new string[0];

            /// <summary>The overworld zone of a story step or a road quest.</summary>
            public string ZoneId;

            public override string ToString() => $"{Source} {QuestId}/{ObjectiveId} → {EnemyId}";
        }

        private static Dictionary<string, List<SpawnSite>> _sites;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            MapLayouts.ClearCache();
            OverworldLayouts.ClearCache();
            _sites = null;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() => MapLayouts.ClearCache();

        /// <summary>Every enemy id the world draws, and where.</summary>
        public static Dictionary<string, List<SpawnSite>> Sites()
        {
            if (_sites != null)
            {
                return _sites;
            }

            var sites = new Dictionary<string, List<SpawnSite>>();
            void Add(string enemyId, SpawnSite site)
            {
                if (string.IsNullOrEmpty(enemyId))
                {
                    return;
                }

                if (!sites.TryGetValue(enemyId, out List<SpawnSite> list))
                {
                    list = new List<SpawnSite>();
                    sites[enemyId] = list;
                }

                list.Add(site);
            }

            foreach (string regionId in OverworldLayouts.RegionLayoutIds)
            {
                OverworldLayout layout = OverworldLayouts.Get(regionId);
                if (layout?.World == null)
                {
                    continue;
                }

                foreach (MapSpawnZone clearing in layout.World.Zones)
                {
                    layout.ZoneOfSpawn.TryGetValue(clearing.Id, out OverworldZone owner);
                    string mapId = owner?.MapIdOrOwner;
                    MapDefinition map = string.IsNullOrEmpty(mapId) ? null : MapCatalog.Get(mapId);
                    var site = new SpawnSite(map != null ? map.Region : layout.Region, mapId, owner?.Id, false);
                    foreach (PeriodSpawnEntry entry in clearing.AllEntries())
                    {
                        Add(entry.EnemyId, site);
                    }
                }
            }

            // The maps still played as their own scene (not embedded in a continuous region).
            foreach (MapDefinition map in MapCatalog.Maps)
            {
                if (!MapLayouts.Has(map.Id) || OverworldLayouts.ForMap(map.Id) != null)
                {
                    continue;
                }

                MapLayout own = MapLayouts.For(map.Id);
                if (own == null)
                {
                    continue;
                }

                foreach (MapSpawnZone clearing in own.Zones)
                {
                    foreach (PeriodSpawnEntry entry in clearing.AllEntries())
                    {
                        Add(entry.EnemyId, new SpawnSite(map.Region, map.Id, clearing.Id, false));
                    }
                }
            }

            foreach (RegionBossSpawn boss in RegionBossRules.DirectorSpawns)
            {
                MapDefinition map = MapCatalog.Get(boss.MapId);
                Add(boss.EnemyId, new SpawnSite(map != null ? map.Region : MapRegion.Zarafshan, boss.MapId, boss.AreaId, true));
            }

            _sites = sites;
            return sites;
        }

        private static string[] CityMaps(string cityId) => MapCatalog.Maps.Where(m => m.ReturnCityId == cityId).Select(m => m.Id).ToArray();

        private static MapRegion? RegionOfMaps(string[] maps)
        {
            foreach (string id in maps)
            {
                MapDefinition map = MapCatalog.Get(id);
                if (map != null)
                {
                    return map.Region;
                }
            }

            return null;
        }

        /// <summary>The old region quests (one giver per world-map biome) live in the first four regions.</summary>
        private static MapRegion? RegionOfBiome(BiomeType biome)
        {
            switch (biome)
            {
                case BiomeType.Forest: return MapRegion.Zarafshan;
                case BiomeType.Desert: return MapRegion.AreiasFlamejantes;
                case BiomeType.Mountain: return MapRegion.SerraDeKaggen;
                case BiomeType.Caves: return MapRegion.GrutasDoLimiar;
                default: return null;
            }
        }

        /// <summary>Every kill target of the quest catalogs written in code.</summary>
        public static List<KillTarget> Targets()
        {
            var targets = new List<KillTarget>();

            foreach (CityQuestSpec spec in CityQuestCatalog.Quests)
            {
                string[] maps = spec.IsOnTheRoad && !string.IsNullOrEmpty(spec.MapId) ? new[] { spec.MapId } : CityMaps(spec.CityId);
                foreach (RegionObjectiveSpec objective in spec.Quest.Objectives.Where(o => o.Type == QuestObjectiveType.KillEnemyType))
                {
                    targets.Add(new KillTarget
                    {
                        Source = spec.IsOnTheRoad ? "city/road" : "city",
                        QuestId = spec.QuestId, ObjectiveId = objective.ObjectiveId, EnemyId = objective.TargetId,
                        Region = RegionOfMaps(maps), Maps = maps, ZoneId = spec.IsOnTheRoad ? spec.ZoneId : null,
                    });
                }
            }

            foreach (RegionQuestSpec spec in RegionQuestCatalog.Quests)
            {
                foreach (RegionObjectiveSpec objective in spec.Objectives.Where(o => o.Type == QuestObjectiveType.KillEnemyType))
                {
                    targets.Add(new KillTarget
                    {
                        Source = "region", QuestId = spec.QuestId, ObjectiveId = objective.ObjectiveId, EnemyId = objective.TargetId,
                        Region = RegionOfBiome(spec.Region),
                    });
                }
            }

            foreach (LateStoryStep step in LateStoryCatalog.WholeStory)
            {
                foreach (RegionObjectiveSpec objective in step.Quest.Objectives.Where(o => o.Type == QuestObjectiveType.KillEnemyType))
                {
                    bool seal = objective.ObjectiveId == MidStoryCatalog.SealObjectiveId;
                    targets.Add(new KillTarget
                    {
                        Source = "story", QuestId = step.QuestId, ObjectiveId = objective.ObjectiveId, EnemyId = objective.TargetId,
                        Region = RegionOfMaps(new[] { step.MapId }), Maps = new[] { step.MapId }, ZoneId = seal ? null : step.ZoneId,
                    });
                }
            }

            foreach (HouseQuestDefinition house in HouseQuestChain.All)
            {
                if (!string.IsNullOrEmpty(house.RequiredBossId))
                {
                    targets.Add(new KillTarget { Source = "house", QuestId = house.Id, ObjectiveId = "required_boss", EnemyId = house.RequiredBossId });
                }
            }

            // The house levels and the forge recipes that wait for a boss (the trophies of the world bosses).
            foreach (HouseLevelSpec level in HouseLevels.All)
            {
                targets.Add(new KillTarget { Source = "house", QuestId = "house_level_" + level.Level, ObjectiveId = "required_boss", EnemyId = level.RequiredBossId });
            }

            foreach (HouseCraftRecipe recipe in HouseCraftRules.All)
            {
                targets.Add(new KillTarget { Source = "house", QuestId = recipe.Id, ObjectiveId = "required_boss", EnemyId = recipe.RequiredBossId });
            }

            return targets.Where(t => !string.IsNullOrEmpty(t.EnemyId)).ToList();
        }

        /// <summary>The bosses of the campaign arena (Main.unity, not the continuous world): fought once, credited when already faced.</summary>
        private static readonly HashSet<string> CampaignBosses = new HashSet<string>(HouseUpgradeRules.BossIds) // the four region bosses of the house levels
        {
            MidStoryCatalog.SealEnemyId, // Tages, the seal of the Grutas
            "enemy_forest_alpha_wolf", // the Lobo Alfa of the house chain
        };

        private static List<SpawnSite> SitesOf(string enemyId) => Sites().TryGetValue(enemyId, out List<SpawnSite> list) ? list : new List<SpawnSite>();

        private static bool Near(MapRegion a, MapRegion b) => Mathf.Abs((int)a - (int)b) <= 1;

        /// <summary>A readable list of the broken targets (the message of a failing test, and the audit's printout).</summary>
        public static List<string> Problems(out List<string> notes)
        {
            var problems = new List<string>();
            notes = new List<string>();
            foreach (KillTarget target in Targets())
            {
                List<SpawnSite> where = SitesOf(target.EnemyId);
                if (where.Count == 0)
                {
                    if (!CampaignBosses.Contains(target.EnemyId))
                    {
                        problems.Add($"{target}: spawns nowhere in the world");
                    }

                    continue;
                }

                string seen = string.Join(", ", where.Select(s => s.Region.ToString()).Distinct());
                if (target.Region.HasValue && !where.Any(s => Near(s.Region, target.Region.Value)))
                {
                    problems.Add($"{target}: not in {target.Region} or a region next to it (only {seen})");
                    continue;
                }

                if (target.Maps.Length > 0 && !where.Any(s => target.Maps.Contains(s.MapId)))
                {
                    string message = $"{target}: not on {string.Join("/", target.Maps)} (only {seen})";
                    if (target.Source == "city" || target.Source == "city/road")
                    {
                        problems.Add(message);
                    }
                    else
                    {
                        notes.Add(message);
                    }

                    continue;
                }

                if (!string.IsNullOrEmpty(target.ZoneId) && !where.Any(s => s.ZoneId == target.ZoneId))
                {
                    problems.Add($"{target}: not drawn in zone {target.ZoneId}");
                }
            }

            return problems;
        }

        [Test]
        public void EveryQuestKillTarget_SpawnsInTheWorld_WhereTheQuestSendsTheCousins()
        {
            List<string> problems = Problems(out List<string> notes);
            foreach (string note in notes)
            {
                TestContext.WriteLine("note: " + note);
            }

            Assert.IsEmpty(problems, "quests asking for a beast that does not live where they send the cousins:\n" + string.Join("\n", problems));
        }

        [Test]
        public void TheAudit_ReadsEveryCatalog_AndTheWholeWorld()
        {
            List<KillTarget> targets = Targets();
            Assert.Greater(targets.Count(t => t.Source == "city"), 40, "the city quests of the cities' own maps");
            Assert.Greater(targets.Count(t => t.Source == "city/road"), 30, "the city quests on the road");
            Assert.Greater(targets.Count(t => t.Source == "story"), 30, "both story chains");
            Assert.Greater(targets.Count(t => t.Source == "region"), 5, "the old region quests");
            Assert.Greater(Sites().Count, 200, "the whole world draws a few hundred beasts");
            Assert.IsTrue(SitesOf(RegionBossRules.ApepId).Any(s => s.IsBoss), "the bosses of the director count as spawns");
        }

        [Test]
        public void TheUltimaLampadaUshabtiQuest_HuntsABeastOfTheFortress()
        {
            CityQuestSpec spec = CityQuestCatalog.FindSpec(CityQuestCatalog.UltimaLampadaUshabti);
            Assert.IsNotNull(spec);
            string[] maps = CityMaps(spec.CityId);
            foreach (RegionObjectiveSpec objective in spec.Quest.Objectives)
            {
                Assert.IsTrue(SitesOf(objective.TargetId).Any(s => maps.Contains(s.MapId)), $"{objective.TargetId} lives on a map of the Fortaleza");
            }
        }

        /// <summary>The quests kept as Resources assets (the cousins' own quests and the dailies), needs the editor's asset database.</summary>
        [Test]
        public void QuestAssets_KillTargets_SpawnSomewhere()
        {
            QuestDefinition[] assets = Resources.LoadAll<QuestDefinition>("Quests");
            var problems = new List<string>();
            foreach (QuestDefinition quest in assets)
            {
                foreach (QuestObjective objective in quest.Objectives.Where(o => o.Type == QuestObjectiveType.KillEnemyType && !string.IsNullOrEmpty(o.TargetId)))
                {
                    if (!CampaignBosses.Contains(objective.TargetId) && SitesOf(objective.TargetId).Count == 0)
                    {
                        problems.Add($"{quest.name}/{objective.ObjectiveId} → {objective.TargetId}");
                    }
                }
            }

            Assert.IsEmpty(problems, "quest assets asking for a beast that spawns nowhere:\n" + string.Join("\n", problems));
        }
    }
}
