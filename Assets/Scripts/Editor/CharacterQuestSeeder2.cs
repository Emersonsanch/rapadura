using System.Collections.Generic;
using System.IO;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Quests;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// Second wave of the one-shot editor tool started by <see cref="CharacterQuestSeeder"/>: generates
    /// personal-story <see cref="QuestDefinition"/> assets for Maria and Ícaro (see
    /// <c>Assets/Scripts/Gameplay/Characters/{Maria,Icaro}.cs</c> and Docs/GDD.md for their lore),
    /// under Assets/Resources/Quests. Same conventions as <c>CharacterQuestSeeder</c>: safe to re-run,
    /// existing assets with a matching quest id are updated in place instead of duplicated. Kept as a
    /// separate file/menu item (Rapadura > Seed Character Quests (Maria &amp; Ícaro)) rather than
    /// folded into the first-wave seeder so neither file needs touching to review/revert the other.
    ///
    /// Enemy/item ids referenced here come from <c>EnemyDatabaseSeeder</c>/<c>ItemDatabaseSeeder</c>
    /// (run those first so ItemDatabase.GetById resolves reward items; a missing item just logs a
    /// warning and leaves that reward slot's Item reference null, same as CharacterQuestSeeder/ShopSeeder).
    ///
    /// Localization keys referenced here (quest.maria_*/quest.icaro_*) ARE added to
    /// LocalizationManager's DefaultEntries by this change — see the block appended in
    /// Assets/Scripts/Core/Localization/LocalizationManager.cs. Ids are chosen to not collide with the
    /// first-wave quest.joaquim_*/quest.maithe_*/quest.lavine_* keys.
    /// </summary>
    public static class CharacterQuestSeeder2
    {
        private const string QuestFolder = "Assets/Resources/Quests";

        [MenuItem("Rapadura/Seed Character Quests (Maria & Ícaro)")]
        public static void SeedQuests()
        {
            if (!Directory.Exists(QuestFolder))
            {
                Directory.CreateDirectory(QuestFolder);
            }

            CreateOrUpdateQuest(BuildMariaQuest());
            CreateOrUpdateQuest(BuildIcaroQuest());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CharacterQuestSeeder2] 2 character quests created/updated under " + QuestFolder);
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
                    Debug.LogWarning($"[CharacterQuestSeeder2] Reward item not found for quest '{seed.QuestId}' — run 'Rapadura > Seed Item Database' first.");
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
        /// Maria — "O Brilho Que Se Apaga" (MainStory). Maria's vision showed the sacred crystals
        /// losing their light (Maria.cs / Docs/GDD.md). This quest sends her to the mountain caves
        /// where a shard of the Templo Solar's crystal network has gone dark: she cleanses corrupted
        /// creatures drawn to the fading light, gathers a still-glowing crystal fragment to study, and
        /// speaks with a fellow acolyte at the temple about what the corruption means for the group of
        /// heroes she is trying to gather.
        /// </summary>
        private static QuestDefinition BuildMariaQuest()
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();

            var objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    ObjectiveId = "cleanse_corrupted_wildlife",
                    Type = QuestObjectiveType.KillEnemyType,
                    TargetId = "enemy_cave_bat_swarm",
                    RequiredAmount = 5,
                    DescriptionKey = "quest.maria_fading_light.obj_cleanse_corrupted_wildlife"
                },
                new QuestObjective
                {
                    ObjectiveId = "gather_crystal_shard",
                    Type = QuestObjectiveType.CollectItem,
                    TargetId = "item_iron_ore",
                    RequiredAmount = 2,
                    DescriptionKey = "quest.maria_fading_light.obj_gather_crystal_shard"
                },
                new QuestObjective
                {
                    ObjectiveId = "talk_to_temple_acolyte",
                    Type = QuestObjectiveType.TalkToNpc,
                    TargetId = "npc_temple_acolyte",
                    RequiredAmount = 1,
                    DescriptionKey = "quest.maria_fading_light.obj_talk_to_temple_acolyte"
                }
            };

            var rewardItems = new List<QuestItemReward> { RewardItem("item_mana_potion", 3) };

            quest.SetDataForTests(
                "quest_maria_fading_light",
                "quest.maria_fading_light.title",
                "quest.maria_fading_light.description",
                QuestType.MainStory,
                objectives,
                rewardExperience: 500,
                rewardItems: rewardItems);

            return quest;
        }

        /// <summary>
        /// Ícaro — "Engrenagens do Passado" (MainStory). Ícaro found fragments of a lost civilization
        /// and built gear that rivals magic (Icaro.cs / Docs/GDD.md); he is chasing further tech
        /// secrets that could save or destroy Rapadura. This quest sends him into desert ruins where a
        /// guardian construct still patrols, has him put it down, recover an intact tech fragment from
        /// the wreckage, and decode its inscriptions back at his workshop.
        /// </summary>
        private static QuestDefinition BuildIcaroQuest()
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();

            var objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    ObjectiveId = "defeat_ruin_guardian",
                    Type = QuestObjectiveType.KillEnemyType,
                    TargetId = "enemy_desert_ruin_guardian",
                    RequiredAmount = 1,
                    DescriptionKey = "quest.icaro_ancient_gears.obj_defeat_ruin_guardian"
                },
                new QuestObjective
                {
                    ObjectiveId = "recover_tech_fragment",
                    Type = QuestObjectiveType.CollectItem,
                    TargetId = "item_iron",
                    RequiredAmount = 1,
                    DescriptionKey = "quest.icaro_ancient_gears.obj_recover_tech_fragment"
                },
                new QuestObjective
                {
                    ObjectiveId = "decode_fragment",
                    Type = QuestObjectiveType.CustomFlag,
                    TargetId = "icaro_decoded_fragment",
                    RequiredAmount = 1,
                    DescriptionKey = "quest.icaro_ancient_gears.obj_decode_fragment"
                }
            };

            var rewardItems = new List<QuestItemReward> { RewardItem("item_pickaxe", 1) };

            quest.SetDataForTests(
                "quest_icaro_ancient_gears",
                "quest.icaro_ancient_gears.title",
                "quest.icaro_ancient_gears.description",
                QuestType.MainStory,
                objectives,
                rewardExperience: 520,
                rewardItems: rewardItems);

            return quest;
        }
    }
}
