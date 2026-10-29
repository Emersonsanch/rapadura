using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Reachability audit of every quest in the game: personal and convergence (<c>Resources/Quests</c>), daily
    /// (<c>Resources/Quests/Daily</c>, <see cref="DailyQuestService"/>) and region (<see cref="RegionQuestCatalog"/>).
    /// Born from <c>quest_joaquim_echoes_of_home</c> asking for 6 kills of <c>enemy_cave_dweller</c>, a region boss that
    /// is fought once through the boss challenge and never rolls in a random encounter
    /// (<see cref="RegionBossProgress.CanRollInNormalEncounter"/>), so the quest could never finish. Rules:
    /// <list type="bullet">
    /// <item>Kill: a common enemy in some biome's spawn table (weight &gt; 0), or a region boss
    /// (<see cref="RegionBossProgress.BossFor"/>) with RequiredAmount 1. A boss killed before the quest was taken is
    /// credited by <c>QuestManager.CreditDefeatedUniqueEnemies</c>.</item>
    /// <item>Collect: dropped by a common enemy (its LootItem or its biome loot table, <see cref="EnemyLootRoller"/>) or
    /// crafted by a default-known recipe from such drops.</item>
    /// <item>Talk: a region quest-giver on the map (<see cref="RegionQuestCatalog.Npcs"/>) or a villager in
    /// <see cref="VillageHubNpcs"/>, the allow-list of Main.unity NPCs the world-map village hub exposes. Its dialogue
    /// asset must exist.</item>
    /// <item>Custom flag: raised by a SetQuestFlag action of a reachable NPC's dialogue.</item>
    /// <item>Reach location: no trigger system exists, so none is reachable.</item>
    /// <item>Every quest can be started: dailies by the rotation, region quests by their giver, the rest by a StartQuest
    /// action in a reachable NPC's dialogue.</item>
    /// </list>
    /// </summary>
    public class QuestReachabilityTests
    {
        /// <summary>
        /// Main.unity villagers. Main.unity only loads as the encounter arena with its village switched off, so these
        /// are reachable only through the world-map village hub (<c>VillageNpcCatalog</c>, built by the village-hub
        /// work; listed here by key so this audit does not depend on that code). Each entry: npc key → reason.
        /// </summary>
        private static readonly IReadOnlyDictionary<string, string> VillageHubNpcs = new Dictionary<string, string>
        {
            { "village_survivor", "Joaquim's story villager; village hub in Floresta (dialogue_village_survivor)" },
            { "temple_acolyte", "Maria's story villager; village hub in Floresta (dialogue_temple_acolyte)" },
            { "windtrail_scholar", "Maithe's story villager; village hub in Floresta (dialogue_windtrail_scholar)" },
            { "ruins_archivist", "Ícaro's story villager; village hub in Floresta (dialogue_ruins_archivist)" },
            { "hermit_mystic", "Lavine's story villager; village hub in Floresta (dialogue_hermit_mystic)" },
            { "maria_gathering_point", "convergence gathering point; village hub in Floresta (dialogue_maria_gathering_point)" },
        };

        private const string NpcPrefix = "npc_";
        private const string DialoguePrefix = "dialogue_";

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            BiomeRegistry.Invalidate();
            ItemDatabase.Invalidate();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            BiomeRegistry.Invalidate();
            ItemDatabase.Invalidate();
        }

        [Test]
        public void EveryQuestObjective_IsReachable()
        {
            List<QuestDefinition> quests = AllQuests();
            Assert.GreaterOrEqual(quests.Count, 6 + 8 + 8, "Expected the personal/convergence, daily and region quests.");

            var context = new ReachabilityContext();
            var problems = new List<string>();

            foreach (QuestDefinition quest in quests)
            {
                Assert.IsNotEmpty(quest.Objectives, $"{quest.QuestId} has no objectives.");
                foreach (QuestObjective objective in quest.Objectives)
                {
                    if (!context.IsReachable(objective, out string why))
                    {
                        problems.Add($"{quest.QuestId}/{objective.ObjectiveId} ({objective.Type} '{objective.TargetId}' x{objective.RequiredAmount}): {why}");
                    }
                }
            }

            Assert.IsEmpty(problems, "Unreachable quest objectives:\n" + string.Join("\n", problems));
        }

        [Test]
        public void EveryQuest_CanBeStarted()
        {
            var context = new ReachabilityContext();
            var problems = new List<string>();

            foreach (QuestDefinition quest in AllQuests())
            {
                if (quest.QuestType == QuestType.Daily || RegionQuestCatalog.IsRegionQuest(quest.QuestId))
                {
                    continue;
                }

                bool started = context.Dialogues.Any(d => context.IsDialogueReachable(d.DialogueId)
                                                           && Actions(d).Any(a => a.Type == DialogueActionType.StartQuest && a.Key == quest.QuestId));
                if (!started)
                {
                    problems.Add($"{quest.QuestId}: no reachable dialogue has a StartQuest action for it.");
                }
            }

            foreach (RegionQuestSpec spec in RegionQuestCatalog.Quests)
            {
                RegionNpcSpec giver = RegionQuestCatalog.NpcFor(spec.Region);
                if (giver == null || (giver.FirstQuestId != spec.QuestId && giver.SecondQuestId != spec.QuestId))
                {
                    problems.Add($"{spec.QuestId}: no region quest-giver hands it out.");
                }
            }

            Assert.IsEmpty(problems, string.Join("\n", problems));
        }

        [Test]
        public void EveryPrerequisite_IsAKnownQuest()
        {
            List<QuestDefinition> quests = AllQuests();
            var ids = new HashSet<string>(quests.Select(q => q.QuestId));

            foreach (QuestDefinition quest in quests)
            {
                foreach (string prerequisite in quest.PrerequisiteQuestIds.Where(p => !string.IsNullOrEmpty(p)))
                {
                    Assert.IsTrue(ids.Contains(prerequisite), $"{quest.QuestId} requires unknown quest '{prerequisite}'.");
                    Assert.AreNotEqual(quest.QuestId, prerequisite, $"{quest.QuestId} requires itself.");
                }
            }
        }

        [Test]
        public void JoaquimEchoesOfHome_HuntsTheForestSpiders_NotTheCavesBoss()
        {
            QuestDefinition quest = Resources.Load<QuestDefinition>("Quests/quest_joaquim_echoes_of_home");
            Assert.IsNotNull(quest);

            QuestObjective clear = quest.Objectives.First(o => o.ObjectiveId == "clear_ruin_dwellers");
            Assert.AreEqual(QuestObjectiveType.KillEnemyType, clear.Type);
            Assert.AreEqual("enemy_forest_spider", clear.TargetId);
            Assert.AreEqual(5, clear.RequiredAmount);

            EnemyDefinition spider = Resources.Load<EnemyDefinition>("Enemies/" + clear.TargetId);
            Assert.IsNotNull(spider);
            Assert.IsFalse(spider.IsBoss);
            Assert.AreEqual(BiomeType.Forest, BiomeRegistry.FindBiomeOf(spider).Type, "The village (and the survivor) are at the Forest's edge.");
        }

        [Test]
        public void TheVillageHubAllowList_OnlyNamesExistingDialogues()
        {
            foreach (KeyValuePair<string, string> npc in VillageHubNpcs)
            {
                Assert.IsNotNull(Resources.Load<DialogueDefinition>("Dialogue/" + DialoguePrefix + npc.Key),
                    $"Allow-listed villager '{npc.Key}' ({npc.Value}) has no dialogue asset.");
            }
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>Personal + convergence + daily assets and the region catalog, one per quest id.</summary>
        private static List<QuestDefinition> AllQuests()
        {
            var byId = new Dictionary<string, QuestDefinition>();
            IEnumerable<QuestDefinition> all = Resources.LoadAll<QuestDefinition>("Quests")
                .Concat(Resources.LoadAll<QuestDefinition>(DailyQuestService.ResourcesFolder))
                .Concat(RegionQuestCatalog.GetAllQuests());

            foreach (QuestDefinition quest in all)
            {
                if (quest != null && !string.IsNullOrEmpty(quest.QuestId) && !byId.ContainsKey(quest.QuestId))
                {
                    byId[quest.QuestId] = quest;
                }
            }

            return byId.Values.ToList();
        }

        private static IEnumerable<DialogueAction> Actions(DialogueDefinition dialogue)
        {
            foreach (DialogueNode node in dialogue.Nodes.Where(n => n != null))
            {
                foreach (DialogueAction action in node.OnEnterActions ?? new List<DialogueAction>())
                {
                    yield return action;
                }

                foreach (DialogueChoice choice in node.Choices ?? new List<DialogueChoice>())
                {
                    foreach (DialogueAction action in choice.Actions ?? new List<DialogueAction>())
                    {
                        yield return action;
                    }
                }
            }
        }

        private sealed class ReachabilityContext
        {
            public readonly List<DialogueDefinition> Dialogues = Resources.LoadAll<DialogueDefinition>("Dialogue").Where(d => d != null).ToList();

            private readonly Dictionary<string, EnemyDefinition> _enemies = Resources.LoadAll<EnemyDefinition>("Enemies")
                .Where(e => e != null).GroupBy(e => e.name).ToDictionary(g => g.Key, g => g.First());

            private readonly HashSet<string> _commonSpawns = new HashSet<string>();
            private readonly HashSet<string> _regionBosses = new HashSet<string>();
            private readonly HashSet<string> _obtainableItems;
            private readonly HashSet<string> _reachableNpcKeys;

            public ReachabilityContext()
            {
                foreach (BiomeDefinition biome in BiomeRegistry.GetAll())
                {
                    foreach (BiomeEnemySpawnEntry entry in biome.Enemies.Where(e => e?.Enemy != null && e.Weight > 0 && !e.Enemy.IsBoss))
                    {
                        _commonSpawns.Add(entry.Enemy.name);
                    }
                }

                var bosses = new RegionBossProgress(WorldMapGraph.CreateDefault(), null);
                foreach (BiomeType region in (BiomeType[])System.Enum.GetValues(typeof(BiomeType)))
                {
                    EnemyDefinition boss = bosses.BossFor(region);
                    if (boss != null)
                    {
                        _regionBosses.Add(boss.name);
                    }
                }

                _obtainableItems = BuildObtainableItemIds();
                _reachableNpcKeys = new HashSet<string>(RegionQuestCatalog.Npcs.Select(n => n.NpcKey).Concat(VillageHubNpcs.Keys));
            }

            public bool IsDialogueReachable(string dialogueId)
            {
                return !string.IsNullOrEmpty(dialogueId) && dialogueId.StartsWith(DialoguePrefix)
                       && _reachableNpcKeys.Contains(dialogueId.Substring(DialoguePrefix.Length));
            }

            public bool IsReachable(QuestObjective objective, out string why)
            {
                why = null;
                switch (objective.Type)
                {
                    case QuestObjectiveType.KillEnemyType:
                        if (!_enemies.TryGetValue(objective.TargetId ?? string.Empty, out EnemyDefinition enemy))
                        {
                            why = "no such EnemyDefinition under Resources/Enemies";
                            return false;
                        }

                        if (_commonSpawns.Contains(enemy.name))
                        {
                            return true;
                        }

                        if (_regionBosses.Contains(enemy.name))
                        {
                            if (objective.RequiredAmount <= 1)
                            {
                                return true;
                            }

                            why = "a region boss is fought once (boss challenge), so it can only be asked for 1 kill";
                            return false;
                        }

                        why = "neither in a biome's normal spawn table nor a region boss";
                        return false;

                    case QuestObjectiveType.CollectItem:
                        if (ItemDatabase.GetById(objective.TargetId) == null)
                        {
                            why = "no such item";
                            return false;
                        }

                        if (_obtainableItems.Contains(objective.TargetId))
                        {
                            return true;
                        }

                        why = "no common enemy drops it and no default recipe makes it from drops";
                        return false;

                    case QuestObjectiveType.TalkToNpc:
                        string target = objective.TargetId ?? string.Empty;
                        string key = target.StartsWith(NpcPrefix) ? target.Substring(NpcPrefix.Length)
                            : target.StartsWith(DialoguePrefix) ? target.Substring(DialoguePrefix.Length) : null;
                        if (key == null || !_reachableNpcKeys.Contains(key))
                        {
                            why = "no region quest-giver or village-hub villager with that id";
                            return false;
                        }

                        bool regionNpc = RegionQuestCatalog.Npcs.Any(n => n.NpcKey == key);
                        if (!regionNpc && Dialogues.All(d => d.DialogueId != DialoguePrefix + key))
                        {
                            why = $"no Resources/Dialogue asset '{DialoguePrefix + key}'";
                            return false;
                        }

                        return true;

                    case QuestObjectiveType.CustomFlag:
                        if (Dialogues.Any(d => IsDialogueReachable(d.DialogueId)
                                               && Actions(d).Any(a => a.Type == DialogueActionType.SetQuestFlag && a.Key == objective.TargetId)))
                        {
                            return true;
                        }

                        why = "no reachable NPC dialogue raises this flag (SetQuestFlag)";
                        return false;

                    case QuestObjectiveType.ReachLocation:
                        why = "there is no location trigger system yet";
                        return false;

                    default:
                        why = "unknown objective type";
                        return false;
                }
            }

            /// <summary>Common-enemy drops (LootItem + biome table), closed over default-known recipes made from them.</summary>
            private HashSet<string> BuildObtainableItemIds()
            {
                var ids = new HashSet<string>();
                foreach (BiomeDefinition biome in BiomeRegistry.GetAll())
                {
                    List<EnemyDefinition> commons = biome.Enemies.Where(e => e?.Enemy != null && e.Weight > 0 && !e.Enemy.IsBoss).Select(e => e.Enemy).ToList();
                    if (commons.Count == 0)
                    {
                        continue;
                    }

                    foreach (EnemyDefinition enemy in commons.Where(e => e.LootItem != null && e.LootDropChance > 0f && e.LootQuantityMax > 0))
                    {
                        ids.Add(enemy.LootItem.ItemId);
                    }

                    if (biome.LootDropChance > 0f && biome.LootRollsPerKill > 0)
                    {
                        foreach (BiomeLootEntry entry in biome.Loot.Where(e => e?.Item != null && e.Weight > 0))
                        {
                            ids.Add(entry.Item.ItemId);
                        }
                    }
                }

                RecipeDefinition[] recipes = RecipeDatabase.GetAll().Where(r => r != null && r.ResultItem != null && r.KnownByDefault).ToArray();
                bool grew = true;
                while (grew)
                {
                    grew = false;
                    foreach (RecipeDefinition recipe in recipes)
                    {
                        if (!ids.Contains(recipe.ResultItem.ItemId) && recipe.Ingredients.All(i => i.item != null && ids.Contains(i.item.ItemId)))
                        {
                            ids.Add(recipe.ResultItem.ItemId);
                            grew = true;
                        }
                    }
                }

                return ids;
            }
        }
    }
}
