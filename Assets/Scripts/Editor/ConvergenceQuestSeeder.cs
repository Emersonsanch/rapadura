using System.Collections.Generic;
using System.IO;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Quests;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// One-shot editor tool that generates the single "convergence" <see cref="QuestDefinition"/>
    /// asset — the moment the five heroes' personal stories (seeded by
    /// <see cref="CharacterQuestSeeder"/>/<see cref="CharacterQuestSeeder2"/>) converge into Maria's
    /// vision that only a group can restore Rapadura's balance (see Maria.cs / Docs/GDD.md). The quest
    /// gates on all five personal quests via <see cref="QuestDefinition.PrerequisiteQuestIds"/>, so it
    /// only becomes available once the player has gathered every hero's story. Under
    /// Assets/Resources/Quests. Mirrors <c>CharacterQuestSeeder</c>: safe to re-run, an existing asset
    /// with a matching quest id is updated in place. Run via Rapadura > Seed Convergence Quest.
    ///
    /// Enemy/item ids referenced here come from <c>EnemyDatabaseSeeder</c>/<c>ItemDatabaseSeeder</c>
    /// (run those first so ItemDatabase.GetById resolves reward items).
    ///
    /// Localization keys referenced here (quest.convergence_*) ARE added to LocalizationManager's
    /// DefaultEntries by this change — see the block appended in
    /// Assets/Scripts/Core/Localization/LocalizationManager.cs.
    /// </summary>
    public static class ConvergenceQuestSeeder
    {
        private const string QuestFolder = "Assets/Resources/Quests";

        [MenuItem("Rapadura/Seed Convergence Quest")]
        public static void SeedQuest()
        {
            if (!Directory.Exists(QuestFolder))
            {
                Directory.CreateDirectory(QuestFolder);
            }

            CreateOrUpdateQuest(BuildConvergenceQuest());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ConvergenceQuestSeeder] 1 convergence quest created/updated under " + QuestFolder);
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
                    Debug.LogWarning($"[ConvergenceQuestSeeder] Reward item not found for quest '{seed.QuestId}' — run 'Rapadura > Seed Item Database' first.");
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
        /// "The Gathering" (MainStory). Requires the five personal quests to be complete —
        /// quest_joaquim_echoes_of_home, quest_maithe_windtrail, quest_lavine_redtide,
        /// quest_maria_fading_light and quest_icaro_ancient_gears — and stages the first time all
        /// five heroes stand together: Maria's vision brought them to a shared meeting point, they
        /// choose (via <c>dialogue_convergence_first_meeting</c>, see
        /// <see cref="ConvergenceDialogueSeeder"/>) to face Rapadura's growing threat as a group, and
        /// prove it by putting down a stronger foe together — the mountain warlord seeded by
        /// EnemyDatabaseSeeder.
        /// </summary>
        private static QuestDefinition BuildConvergenceQuest()
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();

            var prerequisiteQuestIds = new List<string>
            {
                "quest_joaquim_echoes_of_home",
                "quest_maithe_windtrail",
                "quest_lavine_redtide",
                "quest_maria_fading_light",
                "quest_icaro_ancient_gears"
            };

            var objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    ObjectiveId = "meet_maria_at_the_gathering",
                    Type = QuestObjectiveType.TalkToNpc,
                    TargetId = "npc_maria_gathering_point",
                    RequiredAmount = 1,
                    DescriptionKey = "quest.convergence.obj_meet_maria_at_the_gathering"
                },
                new QuestObjective
                {
                    ObjectiveId = "five_heroes_unite",
                    Type = QuestObjectiveType.CustomFlag,
                    TargetId = "convergence_five_heroes_agreed",
                    RequiredAmount = 1,
                    DescriptionKey = "quest.convergence.obj_five_heroes_unite"
                },
                new QuestObjective
                {
                    ObjectiveId = "face_the_mountain_warlord",
                    Type = QuestObjectiveType.KillEnemyType,
                    TargetId = "enemy_mountain_warlord",
                    RequiredAmount = 1,
                    DescriptionKey = "quest.convergence.obj_face_the_mountain_warlord"
                }
            };

            var rewardItems = new List<QuestItemReward> { RewardItem("item_mana_potion", 5) };

            quest.SetDataForTests(
                "quest_convergence_the_gathering",
                "quest.convergence.title",
                "quest.convergence.description",
                QuestType.MainStory,
                objectives,
                prerequisiteQuestIds: prerequisiteQuestIds,
                rewardExperience: 1200,
                rewardItems: rewardItems);

            return quest;
        }
    }
}
