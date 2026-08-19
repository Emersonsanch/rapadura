using System.Collections.Generic;
using System.IO;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Quests;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// One-shot editor tool that generates personal-story <see cref="QuestDefinition"/> assets for
    /// three of the five playable heroes (Joaquim, Maithe, Lavine — see
    /// <c>Assets/Scripts/Gameplay/Characters/{Joaquim,Maithe,Lavine}.cs</c> and TODO.md's roster
    /// section for their lore), under Assets/Resources/Quests. Mirrors <c>DialogueSeeder</c>/
    /// <c>ShopSeeder</c>: safe to re-run, existing assets with a matching quest id are updated in
    /// place instead of duplicated. Run via Rapadura > Seed Character Quests.
    ///
    /// Enemy/item ids referenced here come from <c>EnemyDatabaseSeeder</c>/<c>ItemDatabaseSeeder</c>
    /// (run those first so ItemDatabase.GetById resolves reward items; a missing item just logs a
    /// warning and leaves that reward slot's Item reference null, same as ShopSeeder's pattern).
    ///
    /// Localization keys referenced here (quest.*) ARE added to LocalizationManager's
    /// DefaultEntries by this change — see the "quest.*" block added in
    /// Assets/Scripts/Core/Localization/LocalizationManager.cs.
    /// </summary>
    public static class CharacterQuestSeeder
    {
        private const string QuestFolder = "Assets/Resources/Quests";

        [MenuItem("Rapadura/Seed Character Quests")]
        public static void SeedQuests()
        {
            if (!Directory.Exists(QuestFolder))
            {
                Directory.CreateDirectory(QuestFolder);
            }

            CreateOrUpdateQuest(BuildJoaquimQuest());
            CreateOrUpdateQuest(BuildMaitheQuest());
            CreateOrUpdateQuest(BuildLavineQuest());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CharacterQuestSeeder] 3 character quests created/updated under " + QuestFolder);
        }

        private static void CreateOrUpdateQuest(QuestDefinition seed)
        {
            string path = Path.Combine(QuestFolder, seed.QuestId + ".asset");
            var asset = AssetDatabase.LoadAssetAtPath<QuestDefinition>(path);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<QuestDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty(QuestDefinition.QuestIdPropertyName).stringValue = seed.QuestId;
            so.FindProperty(QuestDefinition.TitleKeyPropertyName).stringValue = seed.TitleKey;
            so.FindProperty(QuestDefinition.DescriptionKeyPropertyName).stringValue = seed.DescriptionKey;
            so.FindProperty(QuestDefinition.QuestTypePropertyName).enumValueIndex = (int)seed.QuestType;
            so.FindProperty(QuestDefinition.RewardExperiencePropertyName).intValue = seed.RewardExperience;

            SerializedProperty prereqProperty = so.FindProperty(QuestDefinition.PrerequisiteQuestIdsPropertyName);
            prereqProperty.arraySize = seed.PrerequisiteQuestIds.Count;
            for (int i = 0; i < seed.PrerequisiteQuestIds.Count; i++)
            {
                prereqProperty.GetArrayElementAtIndex(i).stringValue = seed.PrerequisiteQuestIds[i];
            }

            SerializedProperty objectivesProperty = so.FindProperty(QuestDefinition.ObjectivesPropertyName);
            objectivesProperty.arraySize = seed.Objectives.Count;

            for (int i = 0; i < seed.Objectives.Count; i++)
            {
                WriteObjective(objectivesProperty.GetArrayElementAtIndex(i), seed.Objectives[i]);
            }

            SerializedProperty rewardItemsProperty = so.FindProperty(QuestDefinition.RewardItemsPropertyName);
            rewardItemsProperty.arraySize = seed.RewardItems.Count;

            for (int i = 0; i < seed.RewardItems.Count; i++)
            {
                QuestItemReward rewardSeed = seed.RewardItems[i];
                SerializedProperty rewardProperty = rewardItemsProperty.GetArrayElementAtIndex(i);

                ItemDefinition item = rewardSeed.Item;
                if (item == null)
                {
                    Debug.LogWarning($"[CharacterQuestSeeder] Reward item not found for quest '{seed.QuestId}' — run 'Rapadura > Seed Item Database' first.");
                }

                rewardProperty.FindPropertyRelative(nameof(QuestItemReward.Item)).objectReferenceValue = item;
                rewardProperty.FindPropertyRelative(nameof(QuestItemReward.Quantity)).intValue = rewardSeed.Quantity;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        private static void WriteObjective(SerializedProperty objectiveProperty, QuestObjective objective)
        {
            objectiveProperty.FindPropertyRelative(nameof(QuestObjective.ObjectiveId)).stringValue = objective.ObjectiveId;
            objectiveProperty.FindPropertyRelative(nameof(QuestObjective.Type)).enumValueIndex = (int)objective.Type;
            objectiveProperty.FindPropertyRelative(nameof(QuestObjective.TargetId)).stringValue = objective.TargetId;
            objectiveProperty.FindPropertyRelative(nameof(QuestObjective.RequiredAmount)).intValue = objective.RequiredAmount;
            objectiveProperty.FindPropertyRelative(nameof(QuestObjective.DescriptionKey)).stringValue = objective.DescriptionKey;
        }

        private static QuestItemReward RewardItem(string itemId, int quantity)
        {
            return new QuestItemReward { Item = ItemDatabase.GetById(itemId), Quantity = quantity };
        }

        /// <summary>
        /// Joaquim — "Ecos da Vila Perdida" (MainStory). His village was razed by dark creatures
        /// from Rapadura's depths (see Joaquim.cs / TODO.md roster); this quest sends him back to
        /// its ruins, now infested by the same breed of creature, to clear them out, recover a
        /// relic from the wreckage and find out whether anyone survived.
        /// </summary>
        private static QuestDefinition BuildJoaquimQuest()
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();

            var objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    ObjectiveId = "clear_ruin_dwellers",
                    Type = QuestObjectiveType.KillEnemyType,
                    TargetId = "enemy_cave_dweller",
                    RequiredAmount = 6,
                    DescriptionKey = "quest.joaquim_echoes.obj_clear_ruin_dwellers"
                },
                new QuestObjective
                {
                    ObjectiveId = "recover_relic_blade",
                    Type = QuestObjectiveType.CollectItem,
                    TargetId = "item_iron_sword",
                    RequiredAmount = 1,
                    DescriptionKey = "quest.joaquim_echoes.obj_recover_relic_blade"
                },
                new QuestObjective
                {
                    ObjectiveId = "talk_to_survivor",
                    Type = QuestObjectiveType.TalkToNpc,
                    TargetId = "npc_village_survivor",
                    RequiredAmount = 1,
                    DescriptionKey = "quest.joaquim_echoes.obj_talk_to_survivor"
                }
            };

            var rewardItems = new List<QuestItemReward> { RewardItem("item_leather_chest", 1) };

            quest.SetDataForTests(
                "quest_joaquim_echoes_of_home",
                "quest.joaquim_echoes.title",
                "quest.joaquim_echoes.description",
                QuestType.MainStory,
                objectives,
                rewardExperience: 500,
                rewardItems: rewardItems);

            return quest;
        }

        /// <summary>
        /// Maithe — "Rastros do Vento" (Side). Maithe carries the Olho do Vento and wants to know
        /// who created the monsters plaguing the realms (Maithe.cs / TODO.md roster). Stone
        /// constructs in the mountains bear the same strange markings she's seen before; this quest
        /// has her fight through them, gather ore samples etched with those markings, and use the
        /// Olho do Vento to decode what they mean.
        /// </summary>
        private static QuestDefinition BuildMaitheQuest()
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();

            var objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    ObjectiveId = "fell_stone_golems",
                    Type = QuestObjectiveType.KillEnemyType,
                    TargetId = "enemy_mountain_stone_golem",
                    RequiredAmount = 4,
                    DescriptionKey = "quest.maithe_windtrail.obj_fell_stone_golems"
                },
                new QuestObjective
                {
                    ObjectiveId = "gather_marked_ore",
                    Type = QuestObjectiveType.CollectItem,
                    TargetId = "item_iron_ore",
                    RequiredAmount = 3,
                    DescriptionKey = "quest.maithe_windtrail.obj_gather_marked_ore"
                },
                new QuestObjective
                {
                    ObjectiveId = "decode_glyphs",
                    Type = QuestObjectiveType.CustomFlag,
                    TargetId = "maithe_decoded_glyphs",
                    RequiredAmount = 1,
                    DescriptionKey = "quest.maithe_windtrail.obj_decode_glyphs"
                }
            };

            var rewardItems = new List<QuestItemReward> { RewardItem("item_hunting_bow", 1) };

            quest.SetDataForTests(
                "quest_maithe_windtrail",
                "quest.maithe_windtrail.title",
                "quest.maithe_windtrail.description",
                QuestType.Side,
                objectives,
                rewardExperience: 450,
                rewardItems: rewardItems);

            return quest;
        }

        /// <summary>
        /// Lavine — "A Maré Rubra" (MainStory). Born under the Noite Rubra eclipse, Lavine fights an
        /// internal corruption tied to those forbidden energies (Lavine.cs / TODO.md roster). Here
        /// her power flares and briefly slips her control, drawing a corrupted swarm to her; she has
        /// to master herself and put them down before seeking out a hermit who understands the
        /// eclipse that made her what she is.
        /// </summary>
        private static QuestDefinition BuildLavineQuest()
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();

            var objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    ObjectiveId = "resist_the_whispers",
                    Type = QuestObjectiveType.CustomFlag,
                    TargetId = "lavine_resisted_whispers",
                    RequiredAmount = 1,
                    DescriptionKey = "quest.lavine_redtide.obj_resist_the_whispers"
                },
                new QuestObjective
                {
                    ObjectiveId = "cull_corrupted_swarm",
                    Type = QuestObjectiveType.KillEnemyType,
                    TargetId = "enemy_cave_bat_swarm",
                    RequiredAmount = 8,
                    DescriptionKey = "quest.lavine_redtide.obj_cull_corrupted_swarm"
                },
                new QuestObjective
                {
                    ObjectiveId = "talk_to_hermit",
                    Type = QuestObjectiveType.TalkToNpc,
                    TargetId = "npc_hermit_mystic",
                    RequiredAmount = 1,
                    DescriptionKey = "quest.lavine_redtide.obj_talk_to_hermit"
                }
            };

            var rewardItems = new List<QuestItemReward> { RewardItem("item_mana_potion", 3) };

            quest.SetDataForTests(
                "quest_lavine_redtide",
                "quest.lavine_redtide.title",
                "quest.lavine_redtide.description",
                QuestType.MainStory,
                objectives,
                rewardExperience: 480,
                rewardItems: rewardItems);

            return quest;
        }
    }
}
