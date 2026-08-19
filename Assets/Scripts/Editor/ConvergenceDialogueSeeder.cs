using System.Collections.Generic;
using System.IO;
using Rapadura.Gameplay.Dialogue;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// One-shot editor tool that generates <c>dialogue_convergence_first_meeting</c> — the scene where
    /// all five heroes (Joaquim, Maria, Maithe, Ícaro, Lavine) stand together for the first time,
    /// answering Maria's vision that only a group can restore Rapadura's balance (see Maria.cs /
    /// Docs/GDD.md). Narratively the payoff of quest_convergence_the_gathering's
    /// "meet_maria_at_the_gathering" objective, seeded by <see cref="ConvergenceQuestSeeder"/>: the
    /// player's final choice here is what flips the "five_heroes_unite" CustomFlag that quest checks
    /// for. Under Assets/Resources/Dialogue. Mirrors <c>CharacterDialogueSeeder</c>: safe to re-run,
    /// an existing asset with a matching dialogue id is updated in place. Run via
    /// Rapadura > Seed Convergence Dialogue.
    ///
    /// Localization keys referenced here (dialogue.convergence_first_meeting.*) ARE added to
    /// LocalizationManager's DefaultEntries by this change — see the block appended in
    /// Assets/Scripts/Core/Localization/LocalizationManager.cs.
    /// </summary>
    public static class ConvergenceDialogueSeeder
    {
        private const string DialogueFolder = "Assets/Resources/Dialogue";

        [MenuItem("Rapadura/Seed Convergence Dialogue")]
        public static void SeedDialogue()
        {
            if (!Directory.Exists(DialogueFolder))
            {
                Directory.CreateDirectory(DialogueFolder);
            }

            CreateOrUpdateDialogue(BuildConvergenceFirstMeetingDialogue());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ConvergenceDialogueSeeder] 1 convergence dialogue created/updated under " + DialogueFolder);
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
        /// The five heroes' first meeting. Maria (hopeful, urgent) opens with why she gathered them;
        /// Joaquim (protective) worries about the cost; Maithe (blunt, skeptical) presses for proof
        /// before she commits; Ícaro (pragmatic, curious) treats the vision as a hypothesis worth
        /// testing; Lavine (unsure of herself) doubts she belongs in the group at all. The player's
        /// final choice as Maria seals whether the group commits together, which flips the
        /// "convergence_five_heroes_agreed" CustomFlag quest_convergence_the_gathering waits on.
        /// </summary>
        private static DialogueDefinition BuildConvergenceFirstMeetingDialogue()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();

            var nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    Id = "maria_opens",
                    SpeakerId = "npc_maria_gathering_point",
                    SpeakerNameKey = "dialogue.convergence_first_meeting.speaker_maria",
                    TextKey = "dialogue.convergence_first_meeting.maria_opens",
                    AutoAdvanceToNodeId = "joaquim_worries"
                },
                new DialogueNode
                {
                    Id = "joaquim_worries",
                    SpeakerId = "npc_joaquim",
                    SpeakerNameKey = "dialogue.convergence_first_meeting.speaker_joaquim",
                    TextKey = "dialogue.convergence_first_meeting.joaquim_worries",
                    AutoAdvanceToNodeId = "maithe_presses"
                },
                new DialogueNode
                {
                    Id = "maithe_presses",
                    SpeakerId = "npc_maithe",
                    SpeakerNameKey = "dialogue.convergence_first_meeting.speaker_maithe",
                    TextKey = "dialogue.convergence_first_meeting.maithe_presses",
                    AutoAdvanceToNodeId = "icaro_weighs_in"
                },
                new DialogueNode
                {
                    Id = "icaro_weighs_in",
                    SpeakerId = "npc_icaro",
                    SpeakerNameKey = "dialogue.convergence_first_meeting.speaker_icaro",
                    TextKey = "dialogue.convergence_first_meeting.icaro_weighs_in",
                    AutoAdvanceToNodeId = "lavine_doubts"
                },
                new DialogueNode
                {
                    Id = "lavine_doubts",
                    SpeakerId = "npc_lavine",
                    SpeakerNameKey = "dialogue.convergence_first_meeting.speaker_lavine",
                    TextKey = "dialogue.convergence_first_meeting.lavine_doubts",
                    AutoAdvanceToNodeId = "maria_asks"
                },
                new DialogueNode
                {
                    Id = "maria_asks",
                    SpeakerId = "npc_maria_gathering_point",
                    SpeakerNameKey = "dialogue.convergence_first_meeting.speaker_maria",
                    TextKey = "dialogue.convergence_first_meeting.maria_asks",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.convergence_first_meeting.choice_unite", NextNodeId = "group_unites" },
                        new DialogueChoice { TextKey = "dialogue.convergence_first_meeting.choice_hesitate", NextNodeId = "group_hesitates" }
                    }
                },
                new DialogueNode
                {
                    Id = "group_unites",
                    SpeakerId = "npc_maria_gathering_point",
                    SpeakerNameKey = "dialogue.convergence_first_meeting.speaker_maria",
                    TextKey = "dialogue.convergence_first_meeting.group_unites"
                },
                new DialogueNode
                {
                    Id = "group_hesitates",
                    SpeakerId = "npc_maria_gathering_point",
                    SpeakerNameKey = "dialogue.convergence_first_meeting.speaker_maria",
                    TextKey = "dialogue.convergence_first_meeting.group_hesitates"
                }
            };

            dialogue.SetDataForTests("dialogue_convergence_first_meeting", "maria_opens", nodes);
            return dialogue;
        }
    }
}
