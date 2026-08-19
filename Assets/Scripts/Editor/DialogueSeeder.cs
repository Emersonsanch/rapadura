using System.Collections.Generic;
using System.IO;
using Rapadura.Gameplay.Dialogue;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// One-shot editor tool that generates a couple of example DialogueDefinition assets under
    /// Assets/Resources/Dialogue, mirroring <c>SkillDatabaseSeeder</c>: safe to re-run, existing
    /// assets with a matching dialogue id are updated in place instead of duplicated.
    /// Run via Rapadura > Seed Example Dialogues.
    ///
    /// Localization keys referenced here (dialogue.*) are NOT added to LocalizationManager's
    /// DefaultEntries by this seeder — LocalizationManager.cs is out of scope to edit for this
    /// task. Missing keys just render as "[key]" in-game (LocalizationManager's documented
    /// fallback behaviour) until real translations are authored for a LocalizationTable/CSV.
    /// </summary>
    public static class DialogueSeeder
    {
        private const string DialogueFolder = "Assets/Resources/Dialogue";

        [MenuItem("Rapadura/Seed Example Dialogues")]
        public static void SeedDialogues()
        {
            if (!Directory.Exists(DialogueFolder))
            {
                Directory.CreateDirectory(DialogueFolder);
            }

            CreateOrUpdateDialogue(BuildVillagerGreeting());
            CreateOrUpdateDialogue(BuildBlacksmithOffer());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DialogueSeeder] 2 example dialogues created/updated under " + DialogueFolder);
        }

        private static void CreateOrUpdateDialogue(DialogueDefinition seed)
        {
            string path = Path.Combine(DialogueFolder, seed.DialogueId + ".asset");
            var asset = AssetDatabase.LoadAssetAtPath<DialogueDefinition>(path);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<DialogueDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty(DialogueDefinition.DialogueIdPropertyName).stringValue = seed.DialogueId;
            so.FindProperty(DialogueDefinition.StartNodeIdPropertyName).stringValue = seed.StartNodeId;

            SerializedProperty nodesProperty = so.FindProperty(DialogueDefinition.NodesPropertyName);
            nodesProperty.arraySize = seed.Nodes.Count;

            for (int i = 0; i < seed.Nodes.Count; i++)
            {
                WriteNode(nodesProperty.GetArrayElementAtIndex(i), seed.Nodes[i]);
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        private static void WriteNode(SerializedProperty nodeProperty, DialogueNode node)
        {
            nodeProperty.FindPropertyRelative(nameof(DialogueNode.Id)).stringValue = node.Id;
            nodeProperty.FindPropertyRelative(nameof(DialogueNode.TextKey)).stringValue = node.TextKey;
            nodeProperty.FindPropertyRelative(nameof(DialogueNode.SpeakerId)).stringValue = node.SpeakerId;
            nodeProperty.FindPropertyRelative(nameof(DialogueNode.SpeakerNameKey)).stringValue = node.SpeakerNameKey;
            nodeProperty.FindPropertyRelative(nameof(DialogueNode.AutoAdvanceToNodeId)).stringValue = node.AutoAdvanceToNodeId;

            SerializedProperty choicesProperty = nodeProperty.FindPropertyRelative(nameof(DialogueNode.Choices));
            choicesProperty.arraySize = node.Choices.Count;

            for (int i = 0; i < node.Choices.Count; i++)
            {
                SerializedProperty choiceProperty = choicesProperty.GetArrayElementAtIndex(i);
                DialogueChoice choice = node.Choices[i];

                choiceProperty.FindPropertyRelative(nameof(DialogueChoice.TextKey)).stringValue = choice.TextKey;
                choiceProperty.FindPropertyRelative(nameof(DialogueChoice.NextNodeId)).stringValue = choice.NextNodeId;
                choiceProperty.FindPropertyRelative(nameof(DialogueChoice.ConditionFlag)).stringValue = choice.ConditionFlag;
                choiceProperty.FindPropertyRelative(nameof(DialogueChoice.NegateCondition)).boolValue = choice.NegateCondition;
            }
        }

        /// <summary>Simple two-line greeting with a branching farewell choice.</summary>
        private static DialogueDefinition BuildVillagerGreeting()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();

            var nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    Id = "greeting",
                    SpeakerId = "npc_villager",
                    SpeakerNameKey = "dialogue.villager.name",
                    TextKey = "dialogue.villager.greeting",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.villager.choice_ask_about_village", NextNodeId = "about_village" },
                        new DialogueChoice { TextKey = "dialogue.common.choice_goodbye", NextNodeId = "" }
                    }
                },
                new DialogueNode
                {
                    Id = "about_village",
                    SpeakerId = "npc_villager",
                    SpeakerNameKey = "dialogue.villager.name",
                    TextKey = "dialogue.villager.about_village",
                    AutoAdvanceToNodeId = "farewell"
                },
                new DialogueNode
                {
                    Id = "farewell",
                    SpeakerId = "npc_villager",
                    SpeakerNameKey = "dialogue.villager.name",
                    TextKey = "dialogue.villager.farewell"
                }
            };

            dialogue.SetDataForTests("dialogue_villager_greeting", "greeting", nodes);
            return dialogue;
        }

        /// <summary>Example with a conditional choice gated behind a flag (e.g. a quest reward already claimed).</summary>
        private static DialogueDefinition BuildBlacksmithOffer()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();

            var nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    Id = "offer",
                    SpeakerId = "npc_blacksmith",
                    SpeakerNameKey = "dialogue.blacksmith.name",
                    TextKey = "dialogue.blacksmith.offer",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.blacksmith.choice_ask_quest", NextNodeId = "quest_intro", ConditionFlag = "quest_blacksmith_completed", NegateCondition = true },
                        new DialogueChoice { TextKey = "dialogue.blacksmith.choice_ask_shop", NextNodeId = "shop_redirect" },
                        new DialogueChoice { TextKey = "dialogue.common.choice_goodbye", NextNodeId = "" }
                    }
                },
                new DialogueNode
                {
                    Id = "quest_intro",
                    SpeakerId = "npc_blacksmith",
                    SpeakerNameKey = "dialogue.blacksmith.name",
                    TextKey = "dialogue.blacksmith.quest_intro"
                },
                new DialogueNode
                {
                    Id = "shop_redirect",
                    SpeakerId = "npc_blacksmith",
                    SpeakerNameKey = "dialogue.blacksmith.name",
                    TextKey = "dialogue.blacksmith.shop_redirect"
                }
            };

            dialogue.SetDataForTests("dialogue_blacksmith_offer", "offer", nodes);
            return dialogue;
        }
    }
}
