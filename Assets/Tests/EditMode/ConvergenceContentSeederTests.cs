using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Quests;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the convergence content authored in
    /// <c>Assets/Scripts/Editor/ConvergenceQuestSeeder.cs</c> and
    /// <c>Assets/Scripts/Editor/ConvergenceDialogueSeeder.cs</c> — the quest/dialogue where the five
    /// heroes' personal stories converge on Maria's vision (see Maria.cs / Docs/GDD.md).
    ///
    /// Those seeders are Editor-only (<c>UnityEditor.AssetDatabase</c> calls, [MenuItem]) and live
    /// outside this assembly's references, so this suite does NOT invoke them. Instead it rebuilds
    /// the same data (ids/objectives/nodes, kept in lockstep with the seeders by hand) via the same
    /// plain-C# <c>SetDataForTests</c> construction path the seeders themselves use internally,
    /// mirroring <c>CharacterContentSeederTests</c>/<c>CharacterContentSeeder2Tests</c>.
    /// </summary>
    public class ConvergenceContentSeederTests
    {
        // ------------------------------------------------------------------
        // Quest structure (mirrors ConvergenceQuestSeeder.cs)
        // ------------------------------------------------------------------

        [Test]
        public void ConvergenceQuest_HasExpectedIdTypeAndObjectives()
        {
            QuestDefinition quest = BuildConvergenceQuest();

            Assert.AreEqual("quest_convergence_the_gathering", quest.QuestId);
            Assert.AreEqual(QuestType.MainStory, quest.QuestType);
            Assert.AreEqual(3, quest.Objectives.Count);

            Assert.AreEqual(QuestObjectiveType.TalkToNpc, quest.Objectives[0].Type);
            Assert.AreEqual("npc_maria_gathering_point", quest.Objectives[0].TargetId);
            Assert.AreEqual(1, quest.Objectives[0].RequiredAmount);

            Assert.AreEqual(QuestObjectiveType.CustomFlag, quest.Objectives[1].Type);
            Assert.AreEqual("convergence_five_heroes_agreed", quest.Objectives[1].TargetId);

            Assert.AreEqual(QuestObjectiveType.KillEnemyType, quest.Objectives[2].Type);
            Assert.AreEqual("enemy_mountain_warlord", quest.Objectives[2].TargetId);
            Assert.AreEqual(1, quest.Objectives[2].RequiredAmount);

            Assert.AreEqual(1200, quest.RewardExperience);
        }

        [Test]
        public void ConvergenceQuest_RequiresAllFivePersonalQuestsAsPrerequisites()
        {
            QuestDefinition quest = BuildConvergenceQuest();

            var expected = new[]
            {
                "quest_joaquim_echoes_of_home",
                "quest_maithe_windtrail",
                "quest_lavine_redtide",
                "quest_maria_fading_light",
                "quest_icaro_ancient_gears"
            };

            Assert.AreEqual(expected.Length, quest.PrerequisiteQuestIds.Count);
            foreach (string id in expected)
            {
                Assert.IsTrue(quest.PrerequisiteQuestIds.Contains(id), $"Missing prerequisite '{id}'.");
            }
        }

        [Test]
        public void ConvergenceQuest_HasNonEmptyDescriptionKeysAndRewardsMoreThanPersonalQuests()
        {
            QuestDefinition quest = BuildConvergenceQuest();

            Assert.IsFalse(string.IsNullOrEmpty(quest.TitleKey));
            Assert.IsFalse(string.IsNullOrEmpty(quest.DescriptionKey));

            foreach (QuestObjective objective in quest.Objectives)
            {
                Assert.IsFalse(string.IsNullOrEmpty(objective.ObjectiveId), "Objective missing an id.");
                Assert.IsFalse(string.IsNullOrEmpty(objective.DescriptionKey), $"Objective '{objective.ObjectiveId}' missing a description key.");
                Assert.GreaterOrEqual(objective.RequiredAmount, 1);
            }

            // Highest single-quest reward among the five personal quests is 520 (Ícaro).
            Assert.Greater(quest.RewardExperience, 520, "Convergence quest reward should exceed every personal quest's reward.");
        }

        [Test]
        public void ConvergenceQuestId_DoesNotCollideWithExistingContent()
        {
            var existingIds = new[]
            {
                "quest_joaquim_echoes_of_home",
                "quest_maithe_windtrail",
                "quest_lavine_redtide",
                "quest_maria_fading_light",
                "quest_icaro_ancient_gears"
            };

            Assert.IsFalse(existingIds.Contains(BuildConvergenceQuest().QuestId));
        }

        // ------------------------------------------------------------------
        // Dialogue structure (mirrors ConvergenceDialogueSeeder.cs)
        // ------------------------------------------------------------------

        [Test]
        public void ConvergenceFirstMeetingDialogue_IntroducesAllFiveHeroesAndEndsInAChoice()
        {
            DialogueDefinition dialogue = BuildConvergenceFirstMeetingDialogue();

            Assert.AreEqual("dialogue_convergence_first_meeting", dialogue.DialogueId);
            Assert.AreEqual(8, dialogue.Nodes.Count);

            DialogueNode start = dialogue.GetStartNode();
            Assert.IsNotNull(start);
            Assert.AreEqual("maria_opens", start.Id);

            var expectedSpeakerNodeIds = new[] { "maria_opens", "joaquim_worries", "maithe_presses", "icaro_weighs_in", "lavine_doubts" };
            foreach (string id in expectedSpeakerNodeIds)
            {
                DialogueNode node = dialogue.GetNode(id);
                Assert.IsNotNull(node, $"Missing node '{id}'.");
                Assert.IsFalse(string.IsNullOrEmpty(node.SpeakerNameKey), $"Node '{id}' has no speaker.");
            }

            DialogueNode mariaAsks = dialogue.GetNode("maria_asks");
            Assert.IsNotNull(mariaAsks);
            Assert.AreEqual(2, mariaAsks.Choices.Count, "The final choice should offer unite/hesitate.");

            DialogueNode unite = dialogue.GetNode(mariaAsks.Choices[0].NextNodeId);
            Assert.IsNotNull(unite);
            Assert.IsTrue(unite.IsTerminal);
        }

        [Test]
        public void ConvergenceFirstMeetingDialogueId_DoesNotCollideWithExistingContent()
        {
            var existingIds = new[]
            {
                "dialogue_village_survivor",
                "dialogue_windtrail_scholar",
                "dialogue_hermit_mystic",
                "dialogue_temple_acolyte",
                "dialogue_ruins_archivist"
            };

            Assert.IsFalse(existingIds.Contains(BuildConvergenceFirstMeetingDialogue().DialogueId));
        }

        // ------------------------------------------------------------------
        // Localization coverage: every key referenced by the content above must be authored.
        // ------------------------------------------------------------------

        [Test]
        public void AllConvergenceLocalizationKeys_ExistInDefaultEntries()
        {
            var entriesByKey = LocalizationManager.DefaultEntries.All.ToDictionary(e => e.Key, e => e);

            QuestDefinition quest = BuildConvergenceQuest();
            DialogueDefinition dialogue = BuildConvergenceFirstMeetingDialogue();

            var referencedKeys = new List<string> { quest.TitleKey, quest.DescriptionKey };
            referencedKeys.AddRange(quest.Objectives.Select(o => o.DescriptionKey));

            foreach (DialogueNode node in dialogue.Nodes)
            {
                referencedKeys.Add(node.TextKey);
                if (!string.IsNullOrEmpty(node.SpeakerNameKey))
                {
                    referencedKeys.Add(node.SpeakerNameKey);
                }

                referencedKeys.AddRange(node.Choices.Select(c => c.TextKey));
            }

            foreach (string key in referencedKeys.Distinct())
            {
                Assert.IsTrue(entriesByKey.ContainsKey(key), $"Missing LocalizationManager.DefaultEntries entry for key '{key}'.");
                Assert.IsFalse(string.IsNullOrEmpty(entriesByKey[key].En), $"Key '{key}' has no English text.");
                Assert.IsFalse(string.IsNullOrEmpty(entriesByKey[key].PtBR), $"Key '{key}' has no pt-BR text.");
            }
        }

        // ------------------------------------------------------------------
        // Builders — kept in lockstep with ConvergenceQuestSeeder.cs / ConvergenceDialogueSeeder.cs.
        // ------------------------------------------------------------------

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
                new QuestObjective { ObjectiveId = "meet_maria_at_the_gathering", Type = QuestObjectiveType.TalkToNpc, TargetId = "npc_maria_gathering_point", RequiredAmount = 1, DescriptionKey = "quest.convergence.obj_meet_maria_at_the_gathering" },
                new QuestObjective { ObjectiveId = "five_heroes_unite", Type = QuestObjectiveType.CustomFlag, TargetId = "convergence_five_heroes_agreed", RequiredAmount = 1, DescriptionKey = "quest.convergence.obj_five_heroes_unite" },
                new QuestObjective { ObjectiveId = "face_the_mountain_warlord", Type = QuestObjectiveType.KillEnemyType, TargetId = "enemy_mountain_warlord", RequiredAmount = 1, DescriptionKey = "quest.convergence.obj_face_the_mountain_warlord" }
            };

            quest.SetDataForTests(
                "quest_convergence_the_gathering",
                "quest.convergence.title",
                "quest.convergence.description",
                QuestType.MainStory,
                objectives,
                prerequisiteQuestIds: prerequisiteQuestIds,
                rewardExperience: 1200);

            return quest;
        }

        private static DialogueDefinition BuildConvergenceFirstMeetingDialogue()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();

            var nodes = new List<DialogueNode>
            {
                new DialogueNode { Id = "maria_opens", SpeakerId = "npc_maria_gathering_point", SpeakerNameKey = "dialogue.convergence_first_meeting.speaker_maria", TextKey = "dialogue.convergence_first_meeting.maria_opens", AutoAdvanceToNodeId = "joaquim_worries" },
                new DialogueNode { Id = "joaquim_worries", SpeakerId = "npc_joaquim", SpeakerNameKey = "dialogue.convergence_first_meeting.speaker_joaquim", TextKey = "dialogue.convergence_first_meeting.joaquim_worries", AutoAdvanceToNodeId = "maithe_presses" },
                new DialogueNode { Id = "maithe_presses", SpeakerId = "npc_maithe", SpeakerNameKey = "dialogue.convergence_first_meeting.speaker_maithe", TextKey = "dialogue.convergence_first_meeting.maithe_presses", AutoAdvanceToNodeId = "icaro_weighs_in" },
                new DialogueNode { Id = "icaro_weighs_in", SpeakerId = "npc_icaro", SpeakerNameKey = "dialogue.convergence_first_meeting.speaker_icaro", TextKey = "dialogue.convergence_first_meeting.icaro_weighs_in", AutoAdvanceToNodeId = "lavine_doubts" },
                new DialogueNode { Id = "lavine_doubts", SpeakerId = "npc_lavine", SpeakerNameKey = "dialogue.convergence_first_meeting.speaker_lavine", TextKey = "dialogue.convergence_first_meeting.lavine_doubts", AutoAdvanceToNodeId = "maria_asks" },
                new DialogueNode
                {
                    Id = "maria_asks", SpeakerId = "npc_maria_gathering_point", SpeakerNameKey = "dialogue.convergence_first_meeting.speaker_maria", TextKey = "dialogue.convergence_first_meeting.maria_asks",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.convergence_first_meeting.choice_unite", NextNodeId = "group_unites" },
                        new DialogueChoice { TextKey = "dialogue.convergence_first_meeting.choice_hesitate", NextNodeId = "group_hesitates" }
                    }
                },
                new DialogueNode { Id = "group_unites", SpeakerId = "npc_maria_gathering_point", SpeakerNameKey = "dialogue.convergence_first_meeting.speaker_maria", TextKey = "dialogue.convergence_first_meeting.group_unites" },
                new DialogueNode { Id = "group_hesitates", SpeakerId = "npc_maria_gathering_point", SpeakerNameKey = "dialogue.convergence_first_meeting.speaker_maria", TextKey = "dialogue.convergence_first_meeting.group_hesitates" }
            };

            dialogue.SetDataForTests("dialogue_convergence_first_meeting", "maria_opens", nodes);
            return dialogue;
        }
    }
}
