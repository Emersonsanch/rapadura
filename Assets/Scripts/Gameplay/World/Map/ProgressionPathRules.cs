using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map.Overworld;

namespace Rapadura.Gameplay.World.Map
{
    /// <summary>Where an item (or a mount kind) can be had: a city counter, a quest, or the Grifo da Coluna's quest.</summary>
    public enum ProgressionSourceKind
    {
        /// <summary>A line of a city shop (<see cref="CityShopCatalog"/>), fixed or rotating, maybe behind a <see cref="ShopUnlock"/>.</summary>
        Shop = 0,

        /// <summary>A region side quest of a hub city (<see cref="RegionQuestCatalog"/>).</summary>
        RegionQuest = 1,

        /// <summary>A city side quest (<see cref="CityQuestCatalog"/>).</summary>
        CityQuest = 2,

        /// <summary>A step of As Cartas da Estrada (<see cref="LateStoryCatalog"/>).</summary>
        LateStory = 3,

        /// <summary>"Asas da Coluna" (<see cref="ColumnGriffinQuest"/>): the first flying mount, from any critter, from its level on.</summary>
        MountQuest = 4,

        /// <summary>The drop of a monster of the zones (its own loot item or its biome's table): opens once that monster was met.</summary>
        Loot = 5,

        /// <summary>A world boss of <c>RegionBossRules.AllSpawns</c> (its map open, the hero at its level): its drop, and the kill itself.</summary>
        WorldBoss = 6,

        /// <summary>A crafting recipe: opens at its level once every ingredient is had.</summary>
        Recipe = 7,

        /// <summary>A step of the mid story (<c>MidStoryCatalog</c>, levels 20–100), read by reflection: counted when it exists.</summary>
        MidStory = 8,
    }

    /// <summary>
    /// One real source of an item of the game (plain data, read-only): what it gives (<see cref="ItemId"/>, and the mount kind
    /// when it is a whistle or the griffin), where (<see cref="CityId"/>, null = anywhere) and what it waits for (shop gate,
    /// level, previous quest, zone to reach, monsters to beat).
    /// </summary>
    public sealed class ProgressionSource
    {
        public readonly ProgressionSourceKind Kind;

        /// <summary>The item given (a whistle for a mount bought at a counter; null for the griffin).</summary>
        public readonly string ItemId;

        /// <summary>The mount kind the source gives (whistle or griffin), else null.</summary>
        public readonly MountType? Mount;

        /// <summary>The city of the counter or the quest giver (<see cref="CityCatalog"/> id), null = anywhere.</summary>
        public readonly string CityId;

        /// <summary>Shop id or quest id.</summary>
        public readonly string SourceId;

        /// <summary>Hero level from which the source opens (1 = always).</summary>
        public readonly int MinLevel;

        /// <summary>Gate of a shop line (<see cref="ShopUnlock.None"/> for the quests).</summary>
        public readonly ShopUnlock Unlock;

        /// <summary>Quest that must be done first, or null.</summary>
        public readonly string PrerequisiteQuestId;

        /// <summary>Overworld zone a quest sends the cousins to (<see cref="LateStoryStep.ZoneId"/>), or null.</summary>
        public readonly string ZoneId;

        /// <summary>Enemy ids a quest asks to beat (its <see cref="QuestObjectiveType.KillEnemyType"/> objectives).</summary>
        public readonly IReadOnlyList<string> KillTargets;

        /// <summary>True for a rotating shop line (on the counter only on some days or weeks: still a real source).</summary>
        public readonly bool Rotating;

        /// <summary>Catalog map that must be open first (a world boss's map), or null.</summary>
        public readonly string MapId;

        /// <summary>Items that must all be had first (a recipe's ingredients).</summary>
        public readonly IReadOnlyList<string> Ingredients;

        public ProgressionSource(ProgressionSourceKind kind, string itemId, MountType? mount, string cityId, string sourceId, int minLevel = 1,
            ShopUnlock unlock = default, string prerequisiteQuestId = null, string zoneId = null, IReadOnlyList<string> killTargets = null, bool rotating = false,
            string mapId = null, IReadOnlyList<string> ingredients = null)
        {
            MapId = string.IsNullOrEmpty(mapId) ? null : mapId;
            Ingredients = ingredients ?? Array.Empty<string>();
            Kind = kind;
            ItemId = itemId;
            Mount = mount;
            CityId = cityId;
            SourceId = sourceId;
            MinLevel = Math.Max(1, minLevel);
            Unlock = unlock;
            PrerequisiteQuestId = string.IsNullOrEmpty(prerequisiteQuestId) ? null : prerequisiteQuestId;
            ZoneId = string.IsNullOrEmpty(zoneId) ? null : zoneId;
            KillTargets = killTargets ?? Array.Empty<string>();
            Rotating = rotating;
        }

        /// <summary>A quest (completes once, then its prerequisite chain moves on).</summary>
        public bool IsQuest => Kind != ProgressionSourceKind.Shop && Kind != ProgressionSourceKind.Loot && Kind != ProgressionSourceKind.WorldBoss
            && Kind != ProgressionSourceKind.Recipe;

        public override string ToString()
        {
            string where = CityId ?? "qualquer lugar";
            switch (Kind)
            {
                case ProgressionSourceKind.Shop:
                    string gate = Unlock.Kind == ShopUnlockKind.RegionCleared ? $", depois do chefe de {Unlock.Region}"
                        : Unlock.Kind == ShopUnlockKind.RegionUnlocked ? $", com {Unlock.Region} aberta"
                        : Unlock.Kind == ShopUnlockKind.Reputation ? $", freguesia {Unlock.Level}" : string.Empty;
                    return $"loja {SourceId} em {where}{(Rotating ? " (rotativa)" : string.Empty)}{gate}";
                case ProgressionSourceKind.RegionQuest: return $"missão de região {SourceId} em {where}";
                case ProgressionSourceKind.CityQuest: return $"missão de cidade {SourceId} em {where}";
                case ProgressionSourceKind.LateStory: return $"Cartas da Estrada {SourceId} em {where} (nv {MinLevel}, zona {ZoneId})";
                case ProgressionSourceKind.MidStory: return $"história do meio {SourceId} em {where} (nv {MinLevel}, zona {ZoneId})";
                case ProgressionSourceKind.Loot: return $"loot de {string.Join("/", KillTargets)}";
                case ProgressionSourceKind.WorldBoss: return $"chefe do mundo {SourceId} no {MapId} (nv {MinLevel})";
                case ProgressionSourceKind.Recipe: return $"receita {SourceId} (nv {MinLevel}, com {string.Join(", ", Ingredients)})";
                default: return $"missão {SourceId} (nv {MinLevel}, {where})";
            }
        }
    }

    /// <summary>
    /// Every real source of the items of the game, gathered from the content catalogs (pure, read-only): the counters of the
    /// cities (<see cref="CityCatalog"/> → <see cref="CityShopCatalog"/>), the region quests of the hubs, the city quests, As
    /// Cartas da Estrada and the Grifo da Coluna's quest. Used by <see cref="ProgressionPathRules"/> and the tests.
    /// </summary>
    public static class ProgressionItemSources
    {
        private static List<ProgressionSource> _all;

        /// <summary>All sources, in catalog order (built once).</summary>
        public static IReadOnlyList<ProgressionSource> All => _all ??= Build();

        /// <summary>The sources that give <paramref name="itemId"/>.</summary>
        public static IEnumerable<ProgressionSource> Of(string itemId) => All.Where(s => s.ItemId == itemId);

        /// <summary>The sources that give a mount of kind <paramref name="type"/>.</summary>
        public static IEnumerable<ProgressionSource> OfMount(MountType type) => All.Where(s => s.Mount == type);

        public static void ClearCache() => _all = null;

        private static List<ProgressionSource> Build()
        {
            var list = new List<ProgressionSource>();
            foreach (CitySpec city in CityCatalog.AllCities)
            {
                var shops = new List<string>();
                if (city.ShopIds != null)
                {
                    shops.AddRange(city.ShopIds);
                }

                foreach (CityResidentSpec resident in city.Residents)
                {
                    if (!string.IsNullOrEmpty(resident.ShopId))
                    {
                        shops.Add(resident.ShopId);
                    }
                }

                foreach (string shopId in shops.Distinct())
                {
                    CityShopSpec shop = CityShopCatalog.Get(shopId);
                    if (shop == null)
                    {
                        continue;
                    }

                    foreach (CityShopEntry entry in shop.Entries)
                    {
                        if (!entry.IsBagItem || string.IsNullOrEmpty(entry.ItemId))
                        {
                            continue;
                        }

                        MountType? mount = MountCatalog.FindByWhistle(entry.ItemId)?.Type;
                        list.Add(new ProgressionSource(ProgressionSourceKind.Shop, entry.ItemId, mount, city.CityId, shopId, 1, entry.Unlock, rotating: entry.Rotating));
                    }
                }
            }

            foreach (RegionQuestSpec quest in RegionQuestCatalog.Quests)
            {
                string hub = CityCatalog.ForRegion(quest.Region)?.CityId;
                AddQuest(list, ProgressionSourceKind.RegionQuest, quest, hub, 1, null);
            }

            foreach (CityQuestSpec quest in CityQuestCatalog.Quests)
            {
                AddQuest(list, ProgressionSourceKind.CityQuest, quest.Quest, quest.CityId, 1, null);
            }

            foreach (LateStoryStep step in LateStoryCatalog.Steps)
            {
                AddQuest(list, ProgressionSourceKind.LateStory, step.Quest, step.CityId, step.MinLevel, step.ZoneId);
            }

            foreach (LateStoryStep step in MidStorySteps())
            {
                AddQuest(list, ProgressionSourceKind.MidStory, step.Quest, step.CityId, step.MinLevel, step.ZoneId);
            }

            list.Add(new ProgressionSource(ProgressionSourceKind.MountQuest, null, MountType.Voador, null, ColumnGriffinQuest.QuestId, ColumnGriffinQuest.StartLevel));
            return list;
        }

        /// <summary>
        /// The steps of <c>Rapadura.Gameplay.Quests.MidStoryCatalog.Steps</c> when that catalog exists in this build (read by
        /// reflection, so nothing breaks while it is being written or if it goes away); empty otherwise.
        /// </summary>
        public static IReadOnlyList<LateStoryStep> MidStorySteps()
        {
            const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
            Type type = typeof(LateStoryCatalog).Assembly.GetType("Rapadura.Gameplay.Quests.MidStoryCatalog");
            object value = type?.GetField("Steps", flags)?.GetValue(null) ?? type?.GetProperty("Steps", flags)?.GetValue(null);
            if (value is IEnumerable<LateStoryStep> steps)
            {
                return steps.Where(s => s != null && s.Quest != null).ToList();
            }

            return Array.Empty<LateStoryStep>();
        }

        /// <summary>Drops of the zone monsters: (enemy id, item id) pairs, each a <see cref="ProgressionSourceKind.Loot"/> source.</summary>
        public static List<ProgressionSource> LootSources(IEnumerable<(string EnemyId, string ItemId)> drops)
        {
            var list = new List<ProgressionSource>();
            foreach ((string enemyId, string itemId) in drops ?? Enumerable.Empty<(string, string)>())
            {
                if (string.IsNullOrEmpty(enemyId) || string.IsNullOrEmpty(itemId))
                {
                    continue;
                }

                list.Add(new ProgressionSource(ProgressionSourceKind.Loot, itemId, MountCatalog.FindByWhistle(itemId)?.Type, null, enemyId, killTargets: new[] { enemyId }));
            }

            return list;
        }

        /// <summary>
        /// The world bosses (enemy id, map id, level) and their drops (null item = only the kill, which quests may ask for):
        /// each a <see cref="ProgressionSourceKind.WorldBoss"/> source, open on its map at its level.
        /// </summary>
        public static List<ProgressionSource> BossSources(IEnumerable<(string EnemyId, string MapId, int Level, string ItemId)> bosses)
        {
            var list = new List<ProgressionSource>();
            foreach ((string enemyId, string mapId, int level, string itemId) in bosses ?? Enumerable.Empty<(string, string, int, string)>())
            {
                if (string.IsNullOrEmpty(enemyId))
                {
                    continue;
                }

                string item = string.IsNullOrEmpty(itemId) ? null : itemId;
                list.Add(new ProgressionSource(ProgressionSourceKind.WorldBoss, item, item != null ? MountCatalog.FindByWhistle(item)?.Type : null, null, enemyId, level, mapId: mapId));
            }

            return list;
        }

        /// <summary>Crafting recipes (id, result item, level, ingredient item ids): each a <see cref="ProgressionSourceKind.Recipe"/> source.</summary>
        public static List<ProgressionSource> RecipeSources(IEnumerable<(string RecipeId, string ResultId, int MinLevel, IReadOnlyList<string> Ingredients)> recipes)
        {
            var list = new List<ProgressionSource>();
            foreach ((string recipeId, string resultId, int minLevel, IReadOnlyList<string> ingredients) in recipes ?? Enumerable.Empty<(string, string, int, IReadOnlyList<string>)>())
            {
                if (string.IsNullOrEmpty(resultId))
                {
                    continue;
                }

                List<string> needs = (ingredients ?? Array.Empty<string>()).Where(i => !string.IsNullOrEmpty(i)).Distinct().ToList();
                list.Add(new ProgressionSource(ProgressionSourceKind.Recipe, resultId, MountCatalog.FindByWhistle(resultId)?.Type, null, recipeId, minLevel, ingredients: needs));
            }

            return list;
        }

        private static void AddQuest(List<ProgressionSource> list, ProgressionSourceKind kind, RegionQuestSpec quest, string cityId, int minLevel, string zoneId)
        {
            List<string> kills = quest.Objectives.Where(o => o.Type == QuestObjectiveType.KillEnemyType && !string.IsNullOrEmpty(o.TargetId)).Select(o => o.TargetId).ToList();
            if (quest.Rewards.Count == 0)
            {
                // A quest with no item still moves its chain on: keep it as a source of nothing.
                list.Add(new ProgressionSource(kind, null, null, cityId, quest.QuestId, minLevel, default, quest.PrerequisiteQuestId, zoneId, kills));
                return;
            }

            foreach (RegionRewardSpec reward in quest.Rewards)
            {
                MountType? mount = MountCatalog.FindByWhistle(reward.ItemId)?.Type;
                list.Add(new ProgressionSource(kind, reward.ItemId, mount, cityId, quest.QuestId, minLevel, default, quest.PrerequisiteQuestId, zoneId, kills));
            }
        }
    }

    /// <summary>One hole of the road from map 01 to 100 found by <see cref="ProgressionPathRules.CheckMaps"/> (texts in Portuguese, for the test).</summary>
    public sealed class ProgressionMapIssue
    {
        /// <summary>Catalog map id.</summary>
        public readonly string MapId;

        /// <summary>Which check: "a" level band, "b" requirement source, "c" city/dock reach, "d" source from a later map.</summary>
        public readonly string Check;

        public readonly string Message;

        public ProgressionMapIssue(string mapId, string check, string message)
        {
            MapId = mapId;
            Check = check;
            Message = message;
        }

        public override string ToString() => $"[{MapId}] ({Check}) {Message}";
    }

    /// <summary>What <see cref="ProgressionPathRules.Simulate"/> found: when each map, city, item, mount kind and quest was first had.</summary>
    public sealed class ProgressionPathResult
    {
        /// <summary>Highest level the hero reached (<see cref="ProgressionPathRules.MaxLevel"/> when the road goes to the end).</summary>
        public int FinalLevel;

        /// <summary>Highest monster level in the zones the hero could reach at the end.</summary>
        public int HuntCap;

        /// <summary>True when the hero stopped below the cap: no zone left with monsters near enough to level.</summary>
        public bool Stuck;

        /// <summary>Catalog map id → hero level the map first opened at (missing = never).</summary>
        public readonly Dictionary<string, int> MapOpenLevel = new Dictionary<string, int>();

        /// <summary>City id → hero level the city was first reached at on foot.</summary>
        public readonly Dictionary<string, int> CityLevel = new Dictionary<string, int>();

        /// <summary>City id → the overworld zone its plaza stands in (every city of <see cref="CityCatalog"/> placed in the world).</summary>
        public readonly Dictionary<string, OverworldZone> CityZone = new Dictionary<string, OverworldZone>();

        /// <summary>Item id → hero level it was first had at, and the source that gave it.</summary>
        public readonly Dictionary<string, int> ItemLevel = new Dictionary<string, int>();
        public readonly Dictionary<string, ProgressionSource> ItemSource = new Dictionary<string, ProgressionSource>();

        public readonly Dictionary<MountType, int> MountLevel = new Dictionary<MountType, int>();
        public readonly Dictionary<MountType, ProgressionSource> MountSource = new Dictionary<MountType, ProgressionSource>();

        /// <summary>Quest id → hero level it was done at.</summary>
        public readonly Dictionary<string, int> QuestLevel = new Dictionary<string, int>();

        /// <summary>Zone id → hero level it was first reached at.</summary>
        public readonly Dictionary<string, int> ZoneLevel = new Dictionary<string, int>();

        /// <summary>Everything the hero had at the end (to ask "why is this zone closed?").</summary>
        public MapRequirementState FinalState;

        /// <summary>The gate check of the end state (worn items tried one by one in their slot).</summary>
        public Func<string, bool> FinalLookup;

        public bool IsOpen(string mapId) => mapId != null && MapOpenLevel.ContainsKey(mapId);
    }

    /// <summary>
    /// A hero walking the whole game with pure rules only (EditMode-tested, ProgressionPathTests): from the Vila, level by level up
    /// to <see cref="MaxLevel"/>, on foot through the zones of the stitched world (<see cref="OverworldRules.CheckEnter"/>: walk level
    /// and special gate, open passages of <see cref="OverworldRules.AreNeighbours"/>), buying at the counters of the cities he walked
    /// into, doing the quests of those cities (previous quest done, level, zone reached, monsters met in a reached zone) and
    /// levelling only on the monsters of the zones he reached (up to <see cref="DefaultLevelsAboveHunt"/> above the strongest of
    /// them). The Barcas de Luz only link docks already visited on foot, so they open nothing new. No cousin's gift is counted
    /// (hero = null by default), gold and prices are not counted (farmable), the Tacho's wings are not counted (the Grifo da Coluna
    /// already flies).
    /// </summary>
    public static class ProgressionPathRules
    {
        public const int MaxLevel = 500;

        /// <summary>How many levels above the strongest monster he can reach the hero still levels to ("alguns níveis").</summary>
        public const int DefaultLevelsAboveHunt = 5;

        /// <summary>
        /// Walks the game. <paramref name="slotOf"/>: equipment slot of an item id (to wear boots, chest, wings for the worn gates;
        /// null = nothing is worn); <paramref name="bossLevelOf"/>: level of the boss that clears a world-map region (the
        /// <see cref="ShopUnlockKind.RegionCleared"/> lines; null or 0 = beaten as soon as the region is reached).
        /// </summary>
        public static ProgressionPathResult Simulate(OverworldLayout world, Func<string, EquipmentSlot> slotOf, Func<BiomeType, int> bossLevelOf,
            CharacterId? hero = null, int levelsAboveHunt = DefaultLevelsAboveHunt, IReadOnlyList<ProgressionSource> sources = null)
        {
            if (world == null)
            {
                throw new ArgumentNullException(nameof(world));
            }

            sources ??= ProgressionItemSources.All;
            var result = new ProgressionPathResult();
            List<OverworldZone> zones = world.Zones;
            int n = zones.Count;
            var index = new Dictionary<OverworldZone, int>();
            for (int i = 0; i < n; i++)
            {
                index[zones[i]] = i;
            }

            var links = new List<int>[n];
            for (int i = 0; i < n; i++)
            {
                links[i] = new List<int>();
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    if (OverworldRules.AreNeighbours(zones[i], zones[j]))
                    {
                        links[i].Add(j);
                        links[j].Add(i);
                    }
                }
            }

            OverworldZone startZone = world.Vila != null ? OverworldRules.NearestZone(world, world.Vila.Offset) : zones.FirstOrDefault();
            int start = startZone != null ? index[startZone] : -1;

            // The cities placed in the world, by the zone their plaza stands in.
            var cityZone = new List<(string CityId, int Zone)>();
            foreach (OverworldSettlement settlement in world.Settlements)
            {
                string cityId = settlement.Recipe?.CityId;
                if (string.IsNullOrEmpty(cityId) || CityCatalog.Get(cityId) == null)
                {
                    continue;
                }

                OverworldZone zone = OverworldRules.NearestZone(world, settlement.Offset);
                if (zone != null)
                {
                    cityZone.Add((cityId, index[zone]));
                    if (!result.CityZone.ContainsKey(cityId))
                    {
                        result.CityZone[cityId] = zone;
                    }
                }
            }

            // The enemies each zone spawns (the quests' kill targets).
            var spawns = new HashSet<string>[n];
            for (int i = 0; i < n; i++)
            {
                spawns[i] = new HashSet<string>(StringComparer.Ordinal);
                foreach (OverworldFauna fauna in zones[i].Fauna)
                {
                    foreach (var entry in fauna.Day.Concat(fauna.Night).Concat(fauna.Dusk ?? Enumerable.Empty<Rapadura.Gameplay.Night.PeriodSpawnEntry>()))
                    {
                        if (entry != null && !string.IsNullOrEmpty(entry.EnemyId))
                        {
                            spawns[i].Add(entry.EnemyId);
                        }
                    }
                }
            }

            var items = new HashSet<string>(StringComparer.Ordinal);
            var mounts = new HashSet<MountType>();
            var quests = new HashSet<string>(StringComparer.Ordinal);
            var reached = new bool[n];
            var cities = new HashSet<string>(StringComparer.Ordinal);
            var regions = new HashSet<MapRegion>();
            var enemies = new HashSet<string>(StringComparer.Ordinal);
            var tagMemo = new Dictionary<string, bool>(StringComparer.Ordinal);
            MapRequirementState state = null;

            bool Meets(string tag)
            {
                if (string.IsNullOrEmpty(tag))
                {
                    return true;
                }

                if (tagMemo.TryGetValue(tag, out bool memo))
                {
                    return memo;
                }

                state ??= StateOf(hero, items, mounts, null, null);
                bool met = MapRequirementRules.IsMet(tag, state);
                if (!met && slotOf != null)
                {
                    foreach (string item in items)
                    {
                        EquipmentSlot slot = slotOf(item);
                        if (slot != EquipmentSlot.None && MapRequirementRules.IsMet(tag, StateOf(hero, items, mounts, item, slot)))
                        {
                            met = true;
                            break;
                        }
                    }
                }

                tagMemo[tag] = met;
                return met;
            }

            int level = 1;
            int huntCap = 0;
            while (true)
            {
                bool changed = true;
                while (changed)
                {
                    changed = false;

                    // On foot from the Vila through every open passage the hero may cross now.
                    if (start >= 0 && !reached[start])
                    {
                        Reach(start);
                    }

                    var queue = new Queue<int>();
                    for (int i = 0; i < n; i++)
                    {
                        if (reached[i])
                        {
                            queue.Enqueue(i);
                        }
                    }

                    while (queue.Count > 0)
                    {
                        int z = queue.Dequeue();
                        foreach (int next in links[z])
                        {
                            if (!reached[next] && OverworldRules.CheckEnter(zones[next], level, hero, Meets) == MapAccessCheck.Ok)
                            {
                                Reach(next);
                                queue.Enqueue(next);
                            }
                        }
                    }

                    foreach ((string cityId, int zone) in cityZone)
                    {
                        if (reached[zone] && cities.Add(cityId))
                        {
                            result.CityLevel[cityId] = level;
                        }
                    }

                    foreach (ProgressionSource source in sources)
                    {
                        if (source.IsQuest && quests.Contains(source.SourceId) && !Gives(source))
                        {
                            continue;
                        }

                        if (!IsOpen(source))
                        {
                            continue;
                        }

                        // A world boss beaten counts as met for the quests that ask for it.
                        if (source.Kind == ProgressionSourceKind.WorldBoss && enemies.Add(source.SourceId))
                        {
                            changed = true;
                        }

                        if (source.IsQuest && quests.Add(source.SourceId))
                        {
                            result.QuestLevel[source.SourceId] = level;
                            changed = true;
                        }

                        if (!string.IsNullOrEmpty(source.ItemId) && items.Add(source.ItemId))
                        {
                            result.ItemLevel[source.ItemId] = level;
                            result.ItemSource[source.ItemId] = source;
                            changed = true;
                        }

                        if (source.Mount.HasValue && mounts.Add(source.Mount.Value))
                        {
                            result.MountLevel[source.Mount.Value] = level;
                            result.MountSource[source.Mount.Value] = source;
                            changed = true;
                        }
                    }

                    if (changed)
                    {
                        state = null;
                        tagMemo.Clear();
                    }
                }

                huntCap = 0;
                for (int i = 0; i < n; i++)
                {
                    if (reached[i] && zones[i].Fauna.Count > 0)
                    {
                        huntCap = Math.Max(huntCap, zones[i].MaxLevel);
                    }
                }

                if (level >= MaxLevel)
                {
                    break;
                }

                if (level + 1 > huntCap + levelsAboveHunt)
                {
                    result.Stuck = true;
                    break;
                }

                level++;
            }

            result.FinalLevel = level;
            result.HuntCap = huntCap;
            result.FinalState = StateOf(hero, items, mounts, null, null);
            var finalItems = new HashSet<string>(items);
            var finalMounts = new HashSet<MountType>(mounts);
            result.FinalLookup = tag =>
            {
                if (MapRequirementRules.IsMet(tag, StateOf(hero, finalItems, finalMounts, null, null)))
                {
                    return true;
                }

                return slotOf != null && finalItems.Any(i => slotOf(i) != EquipmentSlot.None && MapRequirementRules.IsMet(tag, StateOf(hero, finalItems, finalMounts, i, slotOf(i))));
            };
            return result;

            void Reach(int z)
            {
                reached[z] = true;
                OverworldZone zone = zones[z];
                result.ZoneLevel[zone.Id] = level;
                string mapId = zone.MapIdOrOwner;
                MapDefinition map = MapCatalog.Get(mapId);
                if (map != null)
                {
                    regions.Add(map.Region);
                }

                // A map opens on its own zones (MapId), or on the in-between areas it owns when it has none.
                if (!string.IsNullOrEmpty(zone.MapId) || !zones.Exists(o => o.MapId == mapId))
                {
                    if (!string.IsNullOrEmpty(mapId) && !result.MapOpenLevel.ContainsKey(mapId))
                    {
                        result.MapOpenLevel[mapId] = level;
                    }
                }

                enemies.UnionWith(spawns[z]);
            }

            bool Gives(ProgressionSource source) =>
                (!string.IsNullOrEmpty(source.ItemId) && !items.Contains(source.ItemId)) || (source.Mount.HasValue && !mounts.Contains(source.Mount.Value));

            bool IsOpen(ProgressionSource source)
            {
                if (level < source.MinLevel)
                {
                    return false;
                }

                if (source.CityId != null && !cities.Contains(source.CityId))
                {
                    return false;
                }

                if (source.PrerequisiteQuestId != null && !quests.Contains(source.PrerequisiteQuestId))
                {
                    return false;
                }

                if (source.ZoneId != null && !result.ZoneLevel.ContainsKey(source.ZoneId))
                {
                    return false;
                }

                foreach (string target in source.KillTargets)
                {
                    if (!enemies.Contains(target))
                    {
                        return false;
                    }
                }

                if (source.MapId != null && !result.MapOpenLevel.ContainsKey(source.MapId))
                {
                    return false;
                }

                foreach (string ingredient in source.Ingredients)
                {
                    if (!items.Contains(ingredient))
                    {
                        return false;
                    }
                }

                switch (source.Unlock.Kind)
                {
                    case ShopUnlockKind.RegionUnlocked:
                        return regions.Contains(MapCatalog.RegionOf(source.Unlock.Region));
                    case ShopUnlockKind.RegionCleared:
                        return regions.Contains(MapCatalog.RegionOf(source.Unlock.Region)) && level >= Math.Max(1, bossLevelOf?.Invoke(source.Unlock.Region) ?? 1);
                    default:
                        return true;
                }
            }
        }

        /// <summary>The 100 maps in level order: walk level, then the monster band, then the number.</summary>
        public static List<MapDefinition> MapsInLevelOrder() =>
            MapCatalog.Maps.OrderBy(m => m.WalkLevelRequirement).ThenBy(m => m.MonsterLevelMin).ThenBy(m => m.MapId).ToList();

        /// <summary>The special gates of a map: its own tag and those of its zones in the world, without repeats.</summary>
        public static List<string> RequirementTagsOf(OverworldLayout world, MapDefinition map)
        {
            var tags = new List<string>();
            if (map == null)
            {
                return tags;
            }

            if (map.HasSpecialRequirement)
            {
                tags.Add(map.SpecialRequirementTag);
            }

            if (world != null)
            {
                foreach (OverworldZone zone in world.Zones)
                {
                    if (zone.MapIdOrOwner == map.Id && !string.IsNullOrEmpty(zone.RequirementTag) && !tags.Contains(zone.RequirementTag))
                    {
                        tags.Add(zone.RequirementTag);
                    }
                }
            }

            return tags;
        }

        /// <summary>
        /// Is <paramref name="tag"/> met with what the walk had by hero level <paramref name="level"/> (items and mount kinds
        /// first had at that level or before; one item tried worn in its slot)?
        /// </summary>
        public static bool TagMetAt(ProgressionPathResult result, string tag, int level, Func<string, EquipmentSlot> slotOf, CharacterId? hero = null,
            string withoutItem = null, MountType? withoutMount = null)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return true;
            }

            if (result == null)
            {
                return false;
            }

            List<string> items = result.ItemLevel.Where(p => p.Value <= level && p.Key != withoutItem).Select(p => p.Key).ToList();
            List<MountType> mounts = result.MountLevel.Where(p => p.Value <= level && (!withoutMount.HasValue || p.Key != withoutMount.Value)).Select(p => p.Key).ToList();
            return MetWith(tag, items, mounts, slotOf, hero);
        }

        /// <summary>Is <paramref name="tag"/> met carrying <paramref name="items"/> and owning <paramref name="mounts"/> (one item tried worn)?</summary>
        public static bool MetWith(string tag, IReadOnlyCollection<string> items, IReadOnlyCollection<MountType> mounts, Func<string, EquipmentSlot> slotOf, CharacterId? hero = null)
        {
            items ??= Array.Empty<string>();
            mounts ??= Array.Empty<MountType>();
            if (MapRequirementRules.IsMet(tag, StateOf(hero, items, mounts, null, null)))
            {
                return true;
            }

            if (slotOf == null)
            {
                return false;
            }

            foreach (string item in items)
            {
                EquipmentSlot slot = slotOf(item);
                if (slot != EquipmentSlot.None && MapRequirementRules.IsMet(tag, StateOf(hero, items, mounts, item, slot)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>First hero level the walk meets <paramref name="tag"/> at, or 0 when it never does.</summary>
        public static int FirstLevelMeeting(ProgressionPathResult result, string tag, Func<string, EquipmentSlot> slotOf, CharacterId? hero = null)
        {
            if (result == null)
            {
                return 0;
            }

            IEnumerable<int> levels = result.ItemLevel.Values.Concat(result.MountLevel.Values).Append(1).Distinct().OrderBy(l => l);
            foreach (int level in levels)
            {
                if (TagMetAt(result, tag, level, slotOf, hero))
                {
                    return level;
                }
            }

            return 0;
        }

        /// <summary>
        /// Checks every map of <see cref="MapsInLevelOrder"/> against the walk of <see cref="Simulate"/> (pure; the test is
        /// ProgressionPathTests):
        /// (a) its walk level is reachable with the monsters of the maps before it (no hole in the bands) and the walk opened it;
        /// (b) each of its gates (<see cref="RequirementTagsOf"/>) is met by something the walk got from a real source;
        /// (c) its return city exists and was walked into, and its own post/dock stands in a zone the walk reached;
        /// (d) the source the gate is first met with does not depend on a map later in the order (its city, zone, monster or boss).
        /// Empty list = the road from 01 to 100 has no hole.
        /// </summary>
        public static List<ProgressionMapIssue> CheckMaps(OverworldLayout world, ProgressionPathResult result, Func<string, EquipmentSlot> slotOf,
            IReadOnlyList<ProgressionSource> sources = null, CharacterId? hero = null, int levelsAboveHunt = DefaultLevelsAboveHunt)
        {
            if (world == null)
            {
                throw new ArgumentNullException(nameof(world));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            sources ??= ProgressionItemSources.All;
            var issues = new List<ProgressionMapIssue>();
            List<MapDefinition> maps = MapsInLevelOrder();
            var order = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < maps.Count; i++)
            {
                order[maps[i].Id] = i;
            }

            int OrderOfMap(string mapId) => mapId != null && order.TryGetValue(mapId, out int o) ? o : -1;

            // The earliest map (in level order) where each monster lives; the world bosses at their own map.
            var enemyOrder = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (OverworldZone zone in world.Zones)
            {
                int o = OrderOfMap(zone.MapIdOrOwner);
                foreach (OverworldFauna fauna in zone.Fauna)
                {
                    foreach (var entry in fauna.Day.Concat(fauna.Night).Concat(fauna.Dusk ?? Enumerable.Empty<Rapadura.Gameplay.Night.PeriodSpawnEntry>()))
                    {
                        if (entry != null && !string.IsNullOrEmpty(entry.EnemyId) && o >= 0 && (!enemyOrder.TryGetValue(entry.EnemyId, out int had) || o < had))
                        {
                            enemyOrder[entry.EnemyId] = o;
                        }
                    }
                }
            }

            foreach (ProgressionSource boss in sources.Where(s => s.Kind == ProgressionSourceKind.WorldBoss))
            {
                int o = OrderOfMap(boss.MapId);
                if (o >= 0 && (!enemyOrder.TryGetValue(boss.SourceId, out int had) || o < had))
                {
                    enemyOrder[boss.SourceId] = o;
                }
            }

            // The map (in level order) a source depends on: its city, its zone, its monsters, its boss's map, its ingredients.
            int HomeOf(ProgressionSource source, int depth)
            {
                int home = -1;
                if (source.CityId != null)
                {
                    home = Math.Max(home, result.CityZone.TryGetValue(source.CityId, out OverworldZone cz) ? OrderOfMap(cz.MapIdOrOwner) : int.MaxValue);
                }

                if (source.ZoneId != null)
                {
                    home = Math.Max(home, OrderOfMap(world.Zone(source.ZoneId)?.MapIdOrOwner));
                }

                foreach (string target in source.KillTargets)
                {
                    home = Math.Max(home, enemyOrder.TryGetValue(target, out int eo) ? eo : int.MaxValue);
                }

                if (source.MapId != null)
                {
                    home = Math.Max(home, OrderOfMap(source.MapId));
                }

                if (depth < 4)
                {
                    foreach (string ingredient in source.Ingredients)
                    {
                        int best = sources.Where(s => s.ItemId == ingredient).Select(s => HomeOf(s, depth + 1)).DefaultIfEmpty(int.MaxValue).Min();
                        home = Math.Max(home, best);
                    }
                }

                return home;
            }

            // Hero level a source was open at in the walk (int.MaxValue = never or unknown).
            int AvailableAt(ProgressionSource source)
            {
                if (source.Kind == ProgressionSourceKind.Shop)
                {
                    return source.CityId != null && result.CityLevel.TryGetValue(source.CityId, out int cl) ? Math.Max(cl, source.MinLevel) : int.MaxValue;
                }

                if (source.IsQuest)
                {
                    return result.QuestLevel.TryGetValue(source.SourceId, out int ql) ? ql : int.MaxValue;
                }

                return source.ItemId != null && result.ItemSource.TryGetValue(source.ItemId, out ProgressionSource u) && u == source ? result.ItemLevel[source.ItemId] : int.MaxValue;
            }

            string NameOf(int o) => o >= 0 && o < maps.Count ? maps[o].Id : "um mapa que o herói não alcança";

            int prevMax = 0;
            for (int k = 0; k < maps.Count; k++)
            {
                MapDefinition map = maps[k];
                bool inWorld = world.Zones.Exists(z => z.MapIdOrOwner == map.Id);

                // (a) The bands: the maps before this one level the hero up to its walk level.
                if (k > 0 && map.WalkLevelRequirement > prevMax + levelsAboveHunt)
                {
                    issues.Add(new ProgressionMapIssue(map.Id, "a",
                        $"nível de entrada {map.WalkLevelRequirement} fora de alcance: os bichos dos mapas anteriores vão só até o nível {prevMax} (+{levelsAboveHunt})."));
                }

                prevMax = Math.Max(prevMax, map.MonsterLevelMax);
                if (!inWorld)
                {
                    issues.Add(new ProgressionMapIssue(map.Id, "c", "o mapa não tem nenhuma zona no mundo contínuo."));
                }
                else if (!result.IsOpen(map.Id))
                {
                    var why = new List<string>();
                    foreach (OverworldZone zone in world.Zones.Where(z => z.MapIdOrOwner == map.Id))
                    {
                        if (result.ZoneLevel.ContainsKey(zone.Id))
                        {
                            continue;
                        }

                        MapAccessCheck check = OverworldRules.CheckEnter(zone, result.FinalLevel, hero, result.FinalLookup);
                        why.Add(check == MapAccessCheck.Ok ? $"{zone.Id} sem passagem aberta a partir de zona alcançada" : $"{zone.Id}: {check}{(string.IsNullOrEmpty(zone.RequirementTag) ? string.Empty : " (" + zone.RequirementTag + ")")}");
                    }

                    issues.Add(new ProgressionMapIssue(map.Id, "a",
                        $"o herói nunca entra no mapa (parou no nível {result.FinalLevel}): {string.Join("; ", why)}."));
                }

                // (b) + (d) The gates.
                bool lateTag = false;
                foreach (string tag in RequirementTagsOf(world, map))
                {
                    string primary = MapRequirementRules.PrimaryItemOf(tag);
                    int first = FirstLevelMeeting(result, tag, slotOf, hero);
                    if (first == 0)
                    {
                        List<string> known = primary == null ? new List<string>() : sources.Where(s => s.ItemId == primary).Select(s => s.ToString()).ToList();
                        issues.Add(new ProgressionMapIssue(map.Id, "b",
                            $"exigência {tag} ({primary ?? "asas ou montaria voadora"}) sem fonte alcançável na rota"
                            + (known.Count == 0 ? ": nenhuma loja, loot, chefe, missão ou receita dá o item." : ": as fontes do catálogo ficam fora do caminho (" + string.Join("; ", known) + ").")));
                        continue;
                    }

                    // What the walk needed at that level: the items and mount kinds it cannot do without.
                    var needed = new List<(string Label, ProgressionSource Used, IEnumerable<ProgressionSource> Alternatives)>();
                    foreach (var item in result.ItemLevel.Where(p => p.Value <= first))
                    {
                        if (!TagMetAt(result, tag, first, slotOf, hero, withoutItem: item.Key))
                        {
                            needed.Add((item.Key, result.ItemSource.TryGetValue(item.Key, out ProgressionSource u) ? u : null, sources.Where(s => s.ItemId == item.Key)));
                        }
                    }

                    foreach (var mount in result.MountLevel.Where(p => p.Value <= first))
                    {
                        if (!TagMetAt(result, tag, first, slotOf, hero, withoutMount: mount.Key))
                        {
                            needed.Add(("montaria " + mount.Key, result.MountSource.TryGetValue(mount.Key, out ProgressionSource u) ? u : null, sources.Where(s => s.Mount == mount.Key)));
                        }
                    }

                    // Nothing alone is a must (two answers came at the same level): any one of what came at that level will do.
                    bool anyOf = needed.Count == 0;
                    if (anyOf)
                    {
                        List<string> before = result.ItemLevel.Where(p => p.Value < first).Select(p => p.Key).ToList();
                        List<MountType> mountsBefore = result.MountLevel.Where(p => p.Value < first).Select(p => p.Key).ToList();
                        foreach (var item in result.ItemLevel.Where(p => p.Value == first))
                        {
                            if (MetWith(tag, before.Append(item.Key).ToList(), mountsBefore, slotOf, hero))
                            {
                                needed.Add((item.Key, result.ItemSource.TryGetValue(item.Key, out ProgressionSource u) ? u : null, sources.Where(s => s.ItemId == item.Key)));
                            }
                        }

                        foreach (var mount in result.MountLevel.Where(p => p.Value == first))
                        {
                            if (MetWith(tag, before, mountsBefore.Append(mount.Key).ToList(), slotOf, hero))
                            {
                                needed.Add(("montaria " + mount.Key, result.MountSource.TryGetValue(mount.Key, out ProgressionSource u) ? u : null, sources.Where(s => s.Mount == mount.Key)));
                            }
                        }
                    }

                    // A giver on a map not later than this one, open by the same level, keeps the gate on the road.
                    bool OnRoad((string Label, ProgressionSource Used, IEnumerable<ProgressionSource> Alternatives) n) =>
                        n.Used == null || HomeOf(n.Used, 0) <= k || n.Alternatives.Any(a => HomeOf(a, 0) <= k && AvailableAt(a) <= first);

                    string usedText = string.Join(anyOf ? " ou " : " e ", needed.Select(n => n.Used != null ? $"{n.Label}, de {n.Used}" : n.Label));
                    var off = needed.Where(n => !OnRoad(n)).ToList();
                    bool flagged = anyOf ? needed.Count > 0 && off.Count == needed.Count : off.Count > 0;
                    if (flagged)
                    {
                        foreach (var n in off)
                        {
                            issues.Add(new ProgressionMapIssue(map.Id, "d",
                                $"exigência {tag}: {n.Label} só vem de {n.Used}, que depende de {NameOf(HomeOf(n.Used, 0))} (posterior na ordem de nível)."));
                        }
                    }

                    if (!flagged && first > map.WalkLevelRequirement + levelsAboveHunt)
                    {
                        lateTag = true;
                        issues.Add(new ProgressionMapIssue(map.Id, "b",
                            $"exigência {tag} só é atendida no nível {first} ({usedText}), {first - map.WalkLevelRequirement} níveis depois da entrada (nv {map.WalkLevelRequirement}): "
                            + (primary == null ? "nenhuma fonte antes disso." : "as fontes de " + primary + " são " + string.Join("; ", sources.Where(s => s.ItemId == primary)
                                .OrderBy(AvailableAt).Take(4).Select(s => $"{s.SourceId}{(s.CityId != null ? " em " + s.CityId : string.Empty)} ({NameOf(HomeOf(s, 0))}, aberta no nv {(AvailableAt(s) == int.MaxValue ? "nunca" : AvailableAt(s).ToString())})")) + ".")));
                    }
                }

                if (!lateTag && result.IsOpen(map.Id) && result.MapOpenLevel[map.Id] > map.WalkLevelRequirement + levelsAboveHunt)
                {
                    issues.Add(new ProgressionMapIssue(map.Id, "a",
                        $"o mapa só abre no nível {result.MapOpenLevel[map.Id]}, {result.MapOpenLevel[map.Id] - map.WalkLevelRequirement} níveis depois da entrada (nv {map.WalkLevelRequirement})."));
                }

                // (c) The return city and the map's own post/dock.
                if (string.IsNullOrEmpty(map.ReturnCityId) || CityCatalog.Get(map.ReturnCityId) == null)
                {
                    issues.Add(new ProgressionMapIssue(map.Id, "c", $"cidade de retorno '{map.ReturnCityId}' não existe no CityCatalog."));
                }
                else if (!result.CityLevel.ContainsKey(map.ReturnCityId))
                {
                    issues.Add(new ProgressionMapIssue(map.Id, "c", $"cidade de retorno {map.ReturnCityId} nunca é alcançada a pé (nem por Barca, que só liga cais já visitados)."));
                }
                else if (result.CityZone.TryGetValue(map.ReturnCityId, out OverworldZone cityZone) && OrderOfMap(cityZone.MapIdOrOwner) > k
                    && !(result.IsOpen(map.Id) && result.CityLevel[map.ReturnCityId] <= result.MapOpenLevel[map.Id]))
                {
                    issues.Add(new ProgressionMapIssue(map.Id, "c",
                        $"cidade de retorno {map.ReturnCityId} fica em {cityZone.MapIdOrOwner}, posterior na ordem de nível, e só é alcançada depois do mapa."));
                }

                List<OverworldSettlement> posts = world.Settlements.Where(st => st.MapId == map.Id).ToList();
                if (posts.Count > 0 && result.IsOpen(map.Id))
                {
                    bool any = posts.Any(st =>
                    {
                        OverworldZone z = OverworldRules.NearestZone(world, st.Offset);
                        return z != null && result.ZoneLevel.ContainsKey(z.Id);
                    });
                    if (!any)
                    {
                        issues.Add(new ProgressionMapIssue(map.Id, "c",
                            $"o posto/cais do mapa ({string.Join(", ", posts.Select(st => st.Id))}) fica numa zona que o herói nunca alcança."));
                    }
                }
            }

            if (result.Stuck)
            {
                issues.Add(new ProgressionMapIssue("-", "a", $"o herói empaca no nível {result.FinalLevel}: o bicho mais forte das zonas alcançadas é nível {result.HuntCap}."));
            }

            return issues;
        }

        /// <summary>The gear of <paramref name="items"/> carried, the mount kinds owned and maybe one item worn in its slot.</summary>
        private static MapRequirementState StateOf(CharacterId? hero, IEnumerable<string> items, IEnumerable<MountType> mounts, string worn, EquipmentSlot? slot)
        {
            var state = new MapRequirementState(hero);
            state.Carried.UnionWith(items);
            foreach (MountType mount in mounts)
            {
                state.Own(mount);
            }

            if (worn != null && slot.HasValue)
            {
                state.Wear(slot.Value, worn);
            }

            return state;
        }
    }
}
