using System.Collections.Generic;
using System.IO;
using Rapadura.Gameplay.Dialogue;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// One-shot editor tool that generates narrative dialogue trees giving voice to three of the
    /// heroes' personal stories (Joaquim, Maithe, Lavine — see TODO.md's roster and
    /// <c>Assets/Scripts/Gameplay/Characters/{Joaquim,Maithe,Lavine}.cs</c>), under
    /// Assets/Resources/Dialogue. Each dialogue is an NPC speaking ABOUT the hero it's tied to and
    /// is narratively linked to the matching quest seeded by <see cref="CharacterQuestSeeder"/>
    /// (e.g. the survivor's "I'll wait here" branch is the natural spot a future
    /// <c>DialogueChoice</c> -> <c>QuestManager.StartQuest</c> hookup would live, not implemented
    /// here since that wiring needs Scene/NPC objects). Mirrors <c>DialogueSeeder</c>: safe to
    /// re-run, existing assets with a matching dialogue id are updated in place. Run via
    /// Rapadura > Seed Character Dialogues.
    ///
    /// Localization keys referenced here (dialogue.*) ARE added to LocalizationManager's
    /// DefaultEntries by this change — see the block added in
    /// Assets/Scripts/Core/Localization/LocalizationManager.cs.
    /// </summary>
    public static class CharacterDialogueSeeder
    {
        private const string DialogueFolder = "Assets/Resources/Dialogue";

        [MenuItem("Rapadura/Seed Character Dialogues")]
        public static void SeedDialogues()
        {
            if (!Directory.Exists(DialogueFolder))
            {
                Directory.CreateDirectory(DialogueFolder);
            }

            CreateOrUpdateDialogue(BuildVillageSurvivorDialogue());
            CreateOrUpdateDialogue(BuildWindTrailScholarDialogue());
            CreateOrUpdateDialogue(BuildHermitMysticDialogue());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CharacterDialogueSeeder] 3 character dialogues created/updated under " + DialogueFolder);
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
        /// A survivor of Joaquim's razed village, found near its ruins. Narratively the NPC target
        /// of the "talk_to_survivor" objective in quest_joaquim_echoes_of_home
        /// (<see cref="CharacterQuestSeeder"/>) — she only has something to say about Joaquim once
        /// the ruins have been cleared, which is why her "hopeful" branch reads like the payoff of
        /// that fight rather than a cold open.
        /// </summary>
        private static DialogueDefinition BuildVillageSurvivorDialogue()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();

            var nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    Id = "greeting",
                    SpeakerId = "npc_village_survivor",
                    SpeakerNameKey = "dialogue.village_survivor.name",
                    TextKey = "dialogue.village_survivor.greeting",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.village_survivor.choice_ask_destruction", NextNodeId = "destruction" },
                        new DialogueChoice { TextKey = "dialogue.village_survivor.choice_ask_joaquim", NextNodeId = "about_joaquim" },
                        new DialogueChoice { TextKey = "dialogue.common.choice_goodbye", NextNodeId = "" }
                    }
                },
                new DialogueNode
                {
                    Id = "destruction",
                    SpeakerId = "npc_village_survivor",
                    SpeakerNameKey = "dialogue.village_survivor.name",
                    TextKey = "dialogue.village_survivor.destruction",
                    AutoAdvanceToNodeId = "offer_help"
                },
                new DialogueNode
                {
                    Id = "about_joaquim",
                    SpeakerId = "npc_village_survivor",
                    SpeakerNameKey = "dialogue.village_survivor.name",
                    TextKey = "dialogue.village_survivor.about_joaquim",
                    AutoAdvanceToNodeId = "offer_help"
                },
                new DialogueNode
                {
                    Id = "offer_help",
                    SpeakerId = "npc_village_survivor",
                    SpeakerNameKey = "dialogue.village_survivor.name",
                    TextKey = "dialogue.village_survivor.offer_help",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.village_survivor.choice_accept", NextNodeId = "accept" },
                        new DialogueChoice { TextKey = "dialogue.village_survivor.choice_decline", NextNodeId = "decline" }
                    }
                },
                new DialogueNode
                {
                    Id = "accept",
                    SpeakerId = "npc_village_survivor",
                    SpeakerNameKey = "dialogue.village_survivor.name",
                    TextKey = "dialogue.village_survivor.accept"
                },
                new DialogueNode
                {
                    Id = "decline",
                    SpeakerId = "npc_village_survivor",
                    SpeakerNameKey = "dialogue.village_survivor.name",
                    TextKey = "dialogue.village_survivor.decline"
                }
            };

            dialogue.SetDataForTests("dialogue_village_survivor", "greeting", nodes);
            return dialogue;
        }

        /// <summary>
        /// A wandering scholar camped on the mountain trail Maithe is investigating. Narratively
        /// precedes/complements the "decode_glyphs" CustomFlag objective in quest_maithe_windtrail
        /// (<see cref="CharacterQuestSeeder"/>) — the scholar can't read the glyphs himself, but
        /// recognizes them as the same mark Maithe has been chasing since she found the Olho do
        /// Vento, giving her (and the player) a reason to keep pulling that thread.
        /// </summary>
        private static DialogueDefinition BuildWindTrailScholarDialogue()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();

            var nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    Id = "greeting",
                    SpeakerId = "npc_windtrail_scholar",
                    SpeakerNameKey = "dialogue.windtrail_scholar.name",
                    TextKey = "dialogue.windtrail_scholar.greeting",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.windtrail_scholar.choice_ask_glyphs", NextNodeId = "glyphs" },
                        new DialogueChoice { TextKey = "dialogue.windtrail_scholar.choice_ask_maithe", NextNodeId = "about_maithe" },
                        new DialogueChoice { TextKey = "dialogue.common.choice_goodbye", NextNodeId = "" }
                    }
                },
                new DialogueNode
                {
                    Id = "glyphs",
                    SpeakerId = "npc_windtrail_scholar",
                    SpeakerNameKey = "dialogue.windtrail_scholar.name",
                    TextKey = "dialogue.windtrail_scholar.glyphs",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.windtrail_scholar.choice_press_further", NextNodeId = "warning" },
                        new DialogueChoice { TextKey = "dialogue.common.choice_goodbye", NextNodeId = "" }
                    }
                },
                new DialogueNode
                {
                    Id = "warning",
                    SpeakerId = "npc_windtrail_scholar",
                    SpeakerNameKey = "dialogue.windtrail_scholar.name",
                    TextKey = "dialogue.windtrail_scholar.warning"
                },
                new DialogueNode
                {
                    Id = "about_maithe",
                    SpeakerId = "npc_windtrail_scholar",
                    SpeakerNameKey = "dialogue.windtrail_scholar.name",
                    TextKey = "dialogue.windtrail_scholar.about_maithe"
                }
            };

            dialogue.SetDataForTests("dialogue_windtrail_scholar", "greeting", nodes);
            return dialogue;
        }

        /// <summary>
        /// A hermit mystic who has studied the Noite Rubra eclipse Lavine was born under.
        /// Narratively the NPC target of the "talk_to_hermit" objective in quest_lavine_redtide
        /// (<see cref="CharacterQuestSeeder"/>) — the branch is a small trust test: does the player
        /// let the hermit voice fear of Lavine, or push back on her behalf? Neither ends the
        /// dialogue badly, but they read differently, which is the point.
        /// </summary>
        private static DialogueDefinition BuildHermitMysticDialogue()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();

            var nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    Id = "greeting",
                    SpeakerId = "npc_hermit_mystic",
                    SpeakerNameKey = "dialogue.hermit_mystic.name",
                    TextKey = "dialogue.hermit_mystic.greeting",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.hermit_mystic.choice_ask_eclipse", NextNodeId = "eclipse" },
                        new DialogueChoice { TextKey = "dialogue.hermit_mystic.choice_ask_lavine", NextNodeId = "about_lavine" },
                        new DialogueChoice { TextKey = "dialogue.common.choice_goodbye", NextNodeId = "" }
                    }
                },
                new DialogueNode
                {
                    Id = "eclipse",
                    SpeakerId = "npc_hermit_mystic",
                    SpeakerNameKey = "dialogue.hermit_mystic.name",
                    TextKey = "dialogue.hermit_mystic.eclipse",
                    AutoAdvanceToNodeId = "about_lavine"
                },
                new DialogueNode
                {
                    Id = "about_lavine",
                    SpeakerId = "npc_hermit_mystic",
                    SpeakerNameKey = "dialogue.hermit_mystic.name",
                    TextKey = "dialogue.hermit_mystic.about_lavine",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.hermit_mystic.choice_defend_lavine", NextNodeId = "trust" },
                        new DialogueChoice { TextKey = "dialogue.hermit_mystic.choice_voice_doubt", NextNodeId = "doubt" }
                    }
                },
                new DialogueNode
                {
                    Id = "trust",
                    SpeakerId = "npc_hermit_mystic",
                    SpeakerNameKey = "dialogue.hermit_mystic.name",
                    TextKey = "dialogue.hermit_mystic.trust"
                },
                new DialogueNode
                {
                    Id = "doubt",
                    SpeakerId = "npc_hermit_mystic",
                    SpeakerNameKey = "dialogue.hermit_mystic.name",
                    TextKey = "dialogue.hermit_mystic.doubt"
                }
            };

            dialogue.SetDataForTests("dialogue_hermit_mystic", "greeting", nodes);
            return dialogue;
        }
    }
}
