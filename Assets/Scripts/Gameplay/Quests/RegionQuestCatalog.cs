using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.Quests
{
    /// <summary>One objective of a <see cref="RegionQuestSpec"/> (same fields as <see cref="QuestObjective"/>, minus runtime progress).</summary>
    public sealed class RegionObjectiveSpec
    {
        public readonly string ObjectiveId;
        public readonly QuestObjectiveType Type;
        public readonly string TargetId;
        public readonly int RequiredAmount;

        public RegionObjectiveSpec(string objectiveId, QuestObjectiveType type, string targetId, int requiredAmount)
        {
            ObjectiveId = objectiveId;
            Type = type;
            TargetId = targetId;
            RequiredAmount = requiredAmount;
        }
    }

    /// <summary>One item reward of a <see cref="RegionQuestSpec"/>, by <c>ItemDefinition.ItemId</c>.</summary>
    public sealed class RegionRewardSpec
    {
        public readonly string ItemId;
        public readonly int Quantity;

        public RegionRewardSpec(string itemId, int quantity)
        {
            ItemId = itemId;
            Quantity = quantity;
        }
    }

    /// <summary>A region side quest, as plain data. Built into a <see cref="QuestDefinition"/> by <see cref="RegionQuestCatalog"/>.</summary>
    public sealed class RegionQuestSpec
    {
        public readonly string QuestId;
        public readonly BiomeType Region;
        public readonly string PrerequisiteQuestId;
        public readonly int RewardExperience;
        public readonly IReadOnlyList<RegionObjectiveSpec> Objectives;
        public readonly IReadOnlyList<RegionRewardSpec> Rewards;

        public RegionQuestSpec(string questId, BiomeType region, string prerequisiteQuestId, int rewardExperience,
            RegionObjectiveSpec[] objectives, RegionRewardSpec[] rewards)
        {
            QuestId = questId;
            Region = region;
            PrerequisiteQuestId = prerequisiteQuestId;
            RewardExperience = rewardExperience;
            Objectives = objectives;
            Rewards = rewards;
        }

        /// <summary>"quest_region_forest_howling_trail" → "quest.region_forest_howling_trail" (same shape as the personal quests' keys).</summary>
        public string KeyBase => RegionQuestCatalog.QuestKeyBase(QuestId);
        public string TitleKey => KeyBase + ".title";
        public string DescriptionKey => KeyBase + ".description";
        public string JournalKey => KeyBase + ".journal";
        public string ObjectiveKey(string objectiveId) => KeyBase + ".obj_" + objectiveId;
    }

    /// <summary>
    /// The quest-giver of one world-map region: who they are (<see cref="NpcKey"/> → <c>npc_*</c>/<c>dialogue_*</c>
    /// ids and <c>dialogue.&lt;NpcKey&gt;.*</c> text keys), which two side quests they hand out, and which hero gets
    /// the special boss warning / the special "accept" line in their conversation.
    /// </summary>
    public sealed class RegionNpcSpec
    {
        public readonly BiomeType Region;
        public readonly string NpcKey;
        public readonly string FirstQuestId;
        public readonly string SecondQuestId;
        public readonly CharacterId BossHintHero;
        public readonly CharacterId AcceptHero;

        public RegionNpcSpec(BiomeType region, string npcKey, string firstQuestId, string secondQuestId, CharacterId bossHintHero, CharacterId acceptHero)
        {
            Region = region;
            NpcKey = npcKey;
            FirstQuestId = firstQuestId;
            SecondQuestId = secondQuestId;
            BossHintHero = bossHintHero;
            AcceptHero = acceptHero;
        }

        public string NpcId => "npc_" + NpcKey;
        public string DialogueId => "dialogue_" + NpcKey;
        public string KeyBase => "dialogue." + NpcKey;
        public string NameKey => KeyBase + ".name";
    }

    /// <summary>
    /// The region side quests (2 per world-map region, Floresta → Deserto → Montanha → Cavernas) and their
    /// quest-givers. Defined in code, not as <c>Resources/Quests</c> assets, so no seeder/Editor step is
    /// needed: <see cref="GetQuest"/> builds each <see cref="QuestDefinition"/> once (same
    /// <see cref="QuestDefinition.SetDataForTests"/> path the seeders use) and the rest of the quest stack
    /// finds them through the fallbacks in <c>QuestDialogueContext</c> and <c>QuestLogDataSource</c>.
    ///
    /// Content rules (checked by <c>RegionQuestContentTests</c>), learned from the quest bugs in TODO.md:
    /// <list type="bullet">
    /// <item>Kill targets are <c>EnemyDefinition</c> asset names (<c>enemy_*</c>, what <c>QuestManager</c> matches)
    /// of <b>non-boss</b> enemies in that region's spawn table — a boss is fought once, so a quest taken after
    /// the boss fell could never finish.</item>
    /// <item>Collect targets are items those enemies actually drop: their <c>EnemyDefinition.LootItem</c>
    /// (<c>item_gold</c>) or the region's biome loot table (<c>EnemyLootRoller</c>; checked for every
    /// quest by <c>EnemyLootTableTests</c>).</item>
    /// <item>No TalkToNpc/CustomFlag objectives: quests auto-complete on their last objective, and only in
    /// Main.unity (the arena) are the hero's inventory/stats live to receive the reward. Every objective
    /// here progresses in combat, so rewards always land.</item>
    /// <item>The second quest of a region requires the first; the giver is only reachable while the token
    /// stands in that (unlocked) region, so a region's quests open together with the region.</item>
    /// </list>
    /// </summary>
    public static class RegionQuestCatalog
    {
        public const string QuestIdPrefix = "quest_";

        public const string ForestFirst = "quest_region_forest_howling_trail";
        public const string ForestSecond = "quest_region_forest_spider_webs";
        public const string DesertFirst = "quest_region_desert_stinger_road";
        public const string DesertSecond = "quest_region_desert_crossing_reserve";
        public const string MountainFirst = "quest_region_mountain_walking_stones";
        public const string MountainSecond = "quest_region_mountain_mine_pass";
        public const string CavesFirst = "quest_region_caves_dark_lamps";
        public const string CavesSecond = "quest_region_caves_last_lantern";

        public static readonly IReadOnlyList<RegionQuestSpec> Quests = new List<RegionQuestSpec>
        {
            // -- Floresta (Dona Cida) --
            new RegionQuestSpec(ForestFirst, BiomeType.Forest, null, 150,
                new[] { new RegionObjectiveSpec("hunt_scrub_wolves", QuestObjectiveType.KillEnemyType, "enemy_forest_wolf", 5) },
                new[] { new RegionRewardSpec("item_health_potion", 2) }),
            new RegionQuestSpec(ForestSecond, BiomeType.Forest, ForestFirst, 200,
                new[] { new RegionObjectiveSpec("clear_forest_spiders", QuestObjectiveType.KillEnemyType, "enemy_forest_spider", 4) },
                new[] { new RegionRewardSpec("item_leather_helmet", 1) }),

            // -- Deserto (Seu Bento) --
            new RegionQuestSpec(DesertFirst, BiomeType.Desert, null, 260,
                new[] { new RegionObjectiveSpec("chase_dune_scorpions", QuestObjectiveType.KillEnemyType, "enemy_desert_scorpion", 5) },
                new[] { new RegionRewardSpec("item_health_potion", 2), new RegionRewardSpec("item_mana_potion", 1) }),
            new RegionQuestSpec(DesertSecond, BiomeType.Desert, DesertFirst, 320,
                new[]
                {
                    new RegionObjectiveSpec("guard_the_caravan", QuestObjectiveType.KillEnemyType, "enemy_desert_scorpion", 3),
                    new RegionObjectiveSpec("save_crossing_gold", QuestObjectiveType.CollectItem, "item_gold", 12)
                },
                new[] { new RegionRewardSpec("item_leather_boots", 1) }),

            // -- Montanha (Mestre Tonho) --
            new RegionQuestSpec(MountainFirst, BiomeType.Mountain, null, 380,
                new[] { new RegionObjectiveSpec("topple_stone_golems", QuestObjectiveType.KillEnemyType, "enemy_mountain_stone_golem", 4) },
                new[] { new RegionRewardSpec("item_mana_potion", 2), new RegionRewardSpec("item_gold", 15) }),
            new RegionQuestSpec(MountainSecond, BiomeType.Mountain, MountainFirst, 460,
                new[] { new RegionObjectiveSpec("open_the_mine_pass", QuestObjectiveType.KillEnemyType, "enemy_mountain_stone_golem", 6) },
                new[] { new RegionRewardSpec("item_iron_sword", 1) }),

            // -- Cavernas (Benedita) --
            new RegionQuestSpec(CavesFirst, BiomeType.Caves, null, 520,
                new[] { new RegionObjectiveSpec("scatter_bat_swarms", QuestObjectiveType.KillEnemyType, "enemy_cave_bat_swarm", 6) },
                new[] { new RegionRewardSpec("item_health_potion", 2), new RegionRewardSpec("item_mana_potion", 2) }),
            new RegionQuestSpec(CavesSecond, BiomeType.Caves, CavesFirst, 620,
                new[] { new RegionObjectiveSpec("light_the_deep_tunnel", QuestObjectiveType.KillEnemyType, "enemy_cave_bat_swarm", 8) },
                new[] { new RegionRewardSpec("item_leather_chest", 1) }),
        };

        public static readonly IReadOnlyList<RegionNpcSpec> Npcs = new List<RegionNpcSpec>
        {
            new RegionNpcSpec(BiomeType.Forest, "forest_rezadeira", ForestFirst, ForestSecond, CharacterId.Maithe, CharacterId.Lavine),
            new RegionNpcSpec(BiomeType.Desert, "desert_tropeiro", DesertFirst, DesertSecond, CharacterId.Icaro, CharacterId.Icaro),
            new RegionNpcSpec(BiomeType.Mountain, "mountain_miner", MountainFirst, MountainSecond, CharacterId.Joaquim, CharacterId.Joaquim),
            new RegionNpcSpec(BiomeType.Caves, "caves_lampkeeper", CavesFirst, CavesSecond, CharacterId.Lavine, CharacterId.Maria),
        };

        private static readonly Dictionary<string, QuestDefinition> Built = new Dictionary<string, QuestDefinition>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Built.Clear();

        public static string QuestKeyBase(string questId)
        {
            string suffix = questId != null && questId.StartsWith(QuestIdPrefix, StringComparison.Ordinal) ? questId.Substring(QuestIdPrefix.Length) : questId;
            return "quest." + suffix;
        }

        public static bool IsRegionQuest(string questId) => FindSpec(questId) != null;

        public static RegionQuestSpec FindSpec(string questId)
        {
            return string.IsNullOrEmpty(questId) ? null : Quests.FirstOrDefault(q => q.QuestId == questId);
        }

        public static RegionNpcSpec NpcFor(BiomeType region) => Npcs.FirstOrDefault(n => n.Region == region);

        public static IEnumerable<RegionQuestSpec> QuestsFor(BiomeType region) => Quests.Where(q => q.Region == region);

        /// <summary>The built definition of a region quest (cached), or null if <paramref name="questId"/> isn't one.</summary>
        public static QuestDefinition GetQuest(string questId)
        {
            RegionQuestSpec spec = FindSpec(questId);
            if (spec == null)
            {
                return null;
            }

            if (Built.TryGetValue(questId, out QuestDefinition cached) && cached != null)
            {
                return cached;
            }

            QuestDefinition definition = Build(spec, ItemDatabase.GetById);
            Built[questId] = definition;
            return definition;
        }

        /// <summary>Every region quest definition, in catalog order.</summary>
        public static IEnumerable<QuestDefinition> GetAllQuests() => Quests.Select(q => GetQuest(q.QuestId));

        /// <summary>Registers every region quest with <paramref name="quests"/> (rewards of a quest restored from a save need it).</summary>
        public static void RegisterAll(QuestManager quests)
        {
            if (quests == null)
            {
                return;
            }

            foreach (QuestDefinition definition in GetAllQuests())
            {
                quests.RegisterDefinition(definition);
            }
        }

        /// <summary>Builds a fresh (uncached) definition; <paramref name="itemLookup"/> resolves reward item ids.</summary>
        public static QuestDefinition Build(RegionQuestSpec spec, Func<string, ItemDefinition> itemLookup)
        {
            var definition = ScriptableObject.CreateInstance<QuestDefinition>();
            definition.name = spec.QuestId;
            // Lives only in memory: keep it across scene loads' UnloadUnusedAssets.
            definition.hideFlags = HideFlags.DontUnloadUnusedAsset;

            List<QuestObjective> objectives = spec.Objectives.Select(o => new QuestObjective
            {
                ObjectiveId = o.ObjectiveId,
                Type = o.Type,
                TargetId = o.TargetId,
                RequiredAmount = o.RequiredAmount,
                DescriptionKey = spec.ObjectiveKey(o.ObjectiveId)
            }).ToList();

            List<QuestItemReward> rewards = spec.Rewards
                .Select(r => new QuestItemReward { Item = itemLookup?.Invoke(r.ItemId), Quantity = r.Quantity })
                .ToList();

            definition.SetDataForTests(
                spec.QuestId,
                spec.TitleKey,
                spec.DescriptionKey,
                QuestType.Side,
                objectives,
                string.IsNullOrEmpty(spec.PrerequisiteQuestId) ? new List<string>() : new List<string> { spec.PrerequisiteQuestId },
                spec.RewardExperience,
                rewards);

            return definition;
        }
    }
}
