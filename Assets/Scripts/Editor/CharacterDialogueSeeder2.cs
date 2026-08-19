using System.Collections.Generic;
using System.IO;
using Rapadura.Gameplay.Dialogue;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// Second wave of the one-shot editor tool started by <see cref="CharacterDialogueSeeder"/>:
    /// generates narrative dialogue trees for Maria and Ícaro's personal stories (see
    /// <c>Assets/Scripts/Gameplay/Characters/{Maria,Icaro}.cs</c> and Docs/GDD.md), under
    /// Assets/Resources/Dialogue. Each dialogue is an NPC speaking ABOUT the hero it's tied to and is
    /// narratively linked to the matching quest seeded by <see cref="CharacterQuestSeeder2"/> (the
    /// temple acolyte's branch pays off "talk_to_temple_acolyte" in quest_maria_fading_light; the
    /// archivist's branch pays off the tech-fragment thread in quest_icaro_ancient_gears). Same
    /// conventions as <c>CharacterDialogueSeeder</c>: safe to re-run, existing assets with a matching
    /// dialogue id are updated in place. Run via Rapadura > Seed Character Dialogues (Maria &amp; Ícaro).
    ///
    /// Localization keys referenced here (dialogue.temple_acolyte.*/dialogue.ruins_archivist.*) ARE
    /// added to LocalizationManager's DefaultEntries by this change — see the block appended in
    /// Assets/Scripts/Core/Localization/LocalizationManager.cs. Ids chosen to not collide with the
    /// first-wave dialogue.village_survivor.*/dialogue.windtrail_scholar.*/dialogue.hermit_mystic.*.
    /// </summary>
    public static class CharacterDialogueSeeder2
    {
        private const string DialogueFolder = "Assets/Resources/Dialogue";

        [MenuItem("Rapadura/Seed Character Dialogues (Maria & Ícaro)")]
        public static void SeedDialogues()
        {
            if (!Directory.Exists(DialogueFolder))
            {
                Directory.CreateDirectory(DialogueFolder);
            }

            CreateOrUpdateDialogue(BuildTempleAcolyteDialogue());
            CreateOrUpdateDialogue(BuildRuinsArchivistDialogue());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CharacterDialogueSeeder2] 2 character dialogues created/updated under " + DialogueFolder);
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

        /// <summary>
        /// An acolyte of the Templo Solar, found tending the shrine. Narratively the NPC target of the
        /// "talk_to_temple_acolyte" objective in quest_maria_fading_light (<see cref="CharacterQuestSeeder2"/>)
        /// — she only opens up about the dimming crystals once Maria has already fought through the
        /// corruption drawn to them, and the branch lets the player either offer Maria's help to the
        /// wider group she's assembling or hold back, learning either the "join" or "watch and wait" tone
        /// of the acolyte's trust in that mission.
        /// </summary>
        private static DialogueDefinition BuildTempleAcolyteDialogue()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();

            var nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    Id = "greeting",
                    SpeakerId = "npc_temple_acolyte",
                    SpeakerNameKey = "dialogue.temple_acolyte.name",
                    TextKey = "dialogue.temple_acolyte.greeting",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.temple_acolyte.choice_ask_crystals", NextNodeId = "crystals" },
                        new DialogueChoice { TextKey = "dialogue.temple_acolyte.choice_ask_maria", NextNodeId = "about_maria" },
                        new DialogueChoice { TextKey = "dialogue.common.choice_goodbye", NextNodeId = "" }
                    }
                },
                new DialogueNode
                {
                    Id = "crystals",
                    SpeakerId = "npc_temple_acolyte",
                    SpeakerNameKey = "dialogue.temple_acolyte.name",
                    TextKey = "dialogue.temple_acolyte.crystals",
                    AutoAdvanceToNodeId = "offer_join"
                },
                new DialogueNode
                {
                    Id = "about_maria",
                    SpeakerId = "npc_temple_acolyte",
                    SpeakerNameKey = "dialogue.temple_acolyte.name",
                    TextKey = "dialogue.temple_acolyte.about_maria",
                    AutoAdvanceToNodeId = "offer_join"
                },
                new DialogueNode
                {
                    Id = "offer_join",
                    SpeakerId = "npc_temple_acolyte",
                    SpeakerNameKey = "dialogue.temple_acolyte.name",
                    TextKey = "dialogue.temple_acolyte.offer_join",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.temple_acolyte.choice_pledge_help", NextNodeId = "pledge" },
                        new DialogueChoice { TextKey = "dialogue.temple_acolyte.choice_stay_watchful", NextNodeId = "watchful" }
                    }
                },
                new DialogueNode
                {
                    Id = "pledge",
                    SpeakerId = "npc_temple_acolyte",
                    SpeakerNameKey = "dialogue.temple_acolyte.name",
                    TextKey = "dialogue.temple_acolyte.pledge"
                },
                new DialogueNode
                {
                    Id = "watchful",
                    SpeakerId = "npc_temple_acolyte",
                    SpeakerNameKey = "dialogue.temple_acolyte.name",
                    TextKey = "dialogue.temple_acolyte.watchful"
                }
            };

            dialogue.SetDataForTests("dialogue_temple_acolyte", "greeting", nodes);
            return dialogue;
        }

        /// <summary>
        /// An archivist camped among the desert ruins Ícaro is exploring. Narratively follows the
        /// "recover_tech_fragment"/"decode_fragment" objectives in quest_icaro_ancient_gears
        /// (<see cref="CharacterQuestSeeder2"/>) — she has studied the lost civilization for years and
        /// can read part of what Ícaro recovered, but the branch is a values fork: does the player have
        /// Ícaro share the discovery openly with her, or keep the dangerous half of it to himself?
        /// </summary>
        private static DialogueDefinition BuildRuinsArchivistDialogue()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();

            var nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    Id = "greeting",
                    SpeakerId = "npc_ruins_archivist",
                    SpeakerNameKey = "dialogue.ruins_archivist.name",
                    TextKey = "dialogue.ruins_archivist.greeting",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.ruins_archivist.choice_ask_fragment", NextNodeId = "fragment" },
                        new DialogueChoice { TextKey = "dialogue.ruins_archivist.choice_ask_icaro", NextNodeId = "about_icaro" },
                        new DialogueChoice { TextKey = "dialogue.common.choice_goodbye", NextNodeId = "" }
                    }
                },
                new DialogueNode
                {
                    Id = "fragment",
                    SpeakerId = "npc_ruins_archivist",
                    SpeakerNameKey = "dialogue.ruins_archivist.name",
                    TextKey = "dialogue.ruins_archivist.fragment",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.ruins_archivist.choice_share_openly", NextNodeId = "share" },
                        new DialogueChoice { TextKey = "dialogue.ruins_archivist.choice_withhold_secret", NextNodeId = "withhold" }
                    }
                },
                new DialogueNode
                {
                    Id = "about_icaro",
                    SpeakerId = "npc_ruins_archivist",
                    SpeakerNameKey = "dialogue.ruins_archivist.name",
                    TextKey = "dialogue.ruins_archivist.about_icaro"
                },
                new DialogueNode
                {
                    Id = "share",
                    SpeakerId = "npc_ruins_archivist",
                    SpeakerNameKey = "dialogue.ruins_archivist.name",
                    TextKey = "dialogue.ruins_archivist.share"
                },
                new DialogueNode
                {
                    Id = "withhold",
                    SpeakerId = "npc_ruins_archivist",
                    SpeakerNameKey = "dialogue.ruins_archivist.name",
                    TextKey = "dialogue.ruins_archivist.withhold"
                }
            };

            dialogue.SetDataForTests("dialogue_ruins_archivist", "greeting", nodes);
            return dialogue;
        }
    }
}
