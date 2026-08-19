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
    /// EditMode tests for the character-story content authored in
    /// <c>Assets/Scripts/Editor/CharacterQuestSeeder.cs</c> and
    /// <c>Assets/Scripts/Editor/CharacterDialogueSeeder.cs</c>.
    ///
    /// Those seeders are Editor-only (<c>UnityEditor.AssetDatabase</c> calls, [MenuItem]) and live
    /// outside this assembly's references, so this suite does NOT invoke them. Instead it rebuilds
    /// the same data (ids/objectives/nodes, kept in lockstep with the seeders by hand) via the same
    /// plain-C# <c>SetDataForTests</c> construction path the seeders themselves use internally, and
    /// asserts on the resulting <see cref="QuestDefinition"/>/<see cref="DialogueDefinition"/>
    /// structure — mirroring how <c>QuestManagerTests</c>/<c>DialogueManagerTests</c> build content
    /// without AssetDatabase. It also checks that every localization key referenced by that content
    /// has a real entry in <see cref="LocalizationManager.DefaultEntries"/> (both en and pt-BR),
    /// so a typo'd/missing key would fail here instead of silently rendering as "[key]" in-game.
    /// </summary>
    public class CharacterContentSeederTests
    {
        // ------------------------------------------------------------------
        // Quest structure (mirrors CharacterQuestSeeder.cs)
        // ------------------------------------------------------------------

        [Test]
        public void JoaquimQuest_HasExpectedIdTypeAndObjectives()
        {
            QuestDefinition quest = BuildJoaquimQuest();

            Assert.AreEqual("quest_joaquim_echoes_of_home", quest.QuestId);
            Assert.AreEqual(QuestType.MainStory, quest.QuestType);
            Assert.AreEqual(3, quest.Objectives.Count);

            Assert.AreEqual(QuestObjectiveType.KillEnemyType, quest.Objectives[0].Type);
            Assert.AreEqual("enemy_cave_dweller", quest.Objectives[0].TargetId);
            Assert.AreEqual(6, quest.Objectives[0].RequiredAmount);

            Assert.AreEqual(QuestObjectiveType.CollectItem, quest.Objectives[1].Type);
            Assert.AreEqual(QuestObjectiveType.TalkToNpc, quest.Objectives[2].Type);
            Assert.AreEqual("npc_village_survivor", quest.Objectives[2].TargetId);

            Assert.AreEqual(500, quest.RewardExperience);
        }

        [Test]
        public void MaitheQuest_HasExpectedIdTypeAndObjectives()
        {
            QuestDefinition quest = BuildMaitheQuest();

            Assert.AreEqual("quest_maithe_windtrail", quest.QuestId);
            Assert.AreEqual(QuestType.Side, quest.QuestType);
            Assert.AreEqual(3, quest.Objectives.Count);

            Assert.AreEqual(QuestObjectiveType.KillEnemyType, quest.Objectives[0].Type);
            Assert.AreEqual("enemy_mountain_stone_golem", quest.Objectives[0].TargetId);

            Assert.AreEqual(QuestObjectiveType.CollectItem, quest.Objectives[1].Type);
            Assert.AreEqual("item_iron_ore", quest.Objectives[1].TargetId);

            Assert.AreEqual(QuestObjectiveType.CustomFlag, quest.Objectives[2].Type);
            Assert.AreEqual("maithe_decoded_glyphs", quest.Objectives[2].TargetId);
        }

        [Test]
        public void LavineQuest_HasExpectedIdTypeAndObjectives()
        {
            QuestDefinition quest = BuildLavineQuest();

            Assert.AreEqual("quest_lavine_redtide", quest.QuestId);
            Assert.AreEqual(QuestType.MainStory, quest.QuestType);
            Assert.AreEqual(3, quest.Objectives.Count);

            Assert.AreEqual(QuestObjectiveType.CustomFlag, quest.Objectives[0].Type);
            Assert.AreEqual("lavine_resisted_whispers", quest.Objectives[0].TargetId);

            Assert.AreEqual(QuestObjectiveType.KillEnemyType, quest.Objectives[1].Type);
            Assert.AreEqual("enemy_cave_bat_swarm", quest.Objectives[1].TargetId);
            Assert.AreEqual(8, quest.Objectives[1].RequiredAmount);

            Assert.AreEqual(QuestObjectiveType.TalkToNpc, quest.Objectives[2].Type);
            Assert.AreEqual("npc_hermit_mystic", quest.Objectives[2].TargetId);
        }

        [Test]
        public void AllThreeQuests_HaveUniqueIdsAndNonEmptyDescriptionKeys()
        {
            var quests = new List<QuestDefinition> { BuildJoaquimQuest(), BuildMaitheQuest(), BuildLavineQuest() };

            var ids = quests.Select(q => q.QuestId).ToList();
            Assert.AreEqual(ids.Distinct().Count(), ids.Count, "Quest ids must be unique.");

            foreach (QuestDefinition quest in quests)
            {
                Assert.IsFalse(string.IsNullOrEmpty(quest.TitleKey));
                Assert.IsFalse(string.IsNullOrEmpty(quest.DescriptionKey));

                foreach (QuestObjective objective in quest.Objectives)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(objective.ObjectiveId), $"Objective in {quest.QuestId} missing an id.");
                    Assert.IsFalse(string.IsNullOrEmpty(objective.DescriptionKey), $"Objective '{objective.ObjectiveId}' in {quest.QuestId} missing a description key.");
                    Assert.GreaterOrEqual(objective.RequiredAmount, 1);
                }
            }
        }

        // ------------------------------------------------------------------
        // Dialogue structure (mirrors CharacterDialogueSeeder.cs)
        // ------------------------------------------------------------------

        [Test]
        public void VillageSurvivorDialogue_HasBranchingStartingAtGreeting()
        {
            DialogueDefinition dialogue = BuildVillageSurvivorDialogue();

            Assert.AreEqual("dialogue_village_survivor", dialogue.DialogueId);
            Assert.AreEqual(6, dialogue.Nodes.Count);

            DialogueNode start = dialogue.GetStartNode();
            Assert.IsNotNull(start);
            Assert.AreEqual("greeting", start.Id);
            Assert.AreEqual(3, start.Choices.Count, "Greeting should branch into destruction/joaquim/goodbye.");

            DialogueNode offerHelp = dialogue.GetNode("offer_help");
            Assert.IsNotNull(offerHelp);
            Assert.AreEqual(2, offerHelp.Choices.Count, "offer_help should branch into accept/decline.");

            DialogueNode accept = dialogue.GetNode(offerHelp.Choices[0].NextNodeId);
            Assert.IsNotNull(accept);
            Assert.IsTrue(accept.IsTerminal);
        }

        [Test]
        public void WindTrailScholarDialogue_HasBranchingStartingAtGreeting()
        {
            DialogueDefinition dialogue = BuildWindTrailScholarDialogue();

            Assert.AreEqual("dialogue_windtrail_scholar", dialogue.DialogueId);
            Assert.AreEqual(4, dialogue.Nodes.Count);

            DialogueNode start = dialogue.GetStartNode();
            Assert.AreEqual(3, start.Choices.Count);
        }

        [Test]
        public void HermitMysticDialogue_HasBranchingStartingAtGreeting()
        {
            DialogueDefinition dialogue = BuildHermitMysticDialogue();

            Assert.AreEqual("dialogue_hermit_mystic", dialogue.DialogueId);
            Assert.AreEqual(5, dialogue.Nodes.Count);

            DialogueNode aboutLavine = dialogue.GetNode("about_lavine");
            Assert.IsNotNull(aboutLavine);
            Assert.AreEqual(2, aboutLavine.Choices.Count, "about_lavine should branch into trust/doubt.");
        }

        // ------------------------------------------------------------------
        // Localization coverage: every key referenced by the content above must be authored.
        // ------------------------------------------------------------------

        [Test]
        public void AllCharacterContentLocalizationKeys_ExistInDefaultEntries()
        {
            var entriesByKey = LocalizationManager.DefaultEntries.All.ToDictionary(e => e.Key, e => e);

            var quests = new List<QuestDefinition> { BuildJoaquimQuest(), BuildMaitheQuest(), BuildLavineQuest() };
            var dialogues = new List<DialogueDefinition>
            {
                BuildVillageSurvivorDialogue(), BuildWindTrailScholarDialogue(), BuildHermitMysticDialogue()
            };

            var referencedKeys = new List<string>();

            foreach (QuestDefinition quest in quests)
            {
                referencedKeys.Add(quest.TitleKey);
                referencedKeys.Add(quest.DescriptionKey);
                referencedKeys.AddRange(quest.Objectives.Select(o => o.DescriptionKey));
            }

            foreach (DialogueDefinition dialogue in dialogues)
            {
                foreach (DialogueNode node in dialogue.Nodes)
                {
                    referencedKeys.Add(node.TextKey);
                    if (!string.IsNullOrEmpty(node.SpeakerNameKey))
                    {
                        referencedKeys.Add(node.SpeakerNameKey);
                    }

                    referencedKeys.AddRange(node.Choices.Select(c => c.TextKey));
                }
            }

            foreach (string key in referencedKeys.Distinct())
            {
                Assert.IsTrue(entriesByKey.ContainsKey(key), $"Missing LocalizationManager.DefaultEntries entry for key '{key}'.");
                Assert.IsFalse(string.IsNullOrEmpty(entriesByKey[key].En), $"Key '{key}' has no English text.");
                Assert.IsFalse(string.IsNullOrEmpty(entriesByKey[key].PtBR), $"Key '{key}' has no pt-BR text.");
            }
        }

        // ------------------------------------------------------------------
        // Builders — kept in lockstep with CharacterQuestSeeder.cs / CharacterDialogueSeeder.cs.
        // ------------------------------------------------------------------

        private static QuestDefinition BuildJoaquimQuest()
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();

            var objectives = new List<QuestObjective>
            {
                new QuestObjective { ObjectiveId = "clear_ruin_dwellers", Type = QuestObjectiveType.KillEnemyType, TargetId = "enemy_cave_dweller", RequiredAmount = 6, DescriptionKey = "quest.joaquim_echoes.obj_clear_ruin_dwellers" },
                new QuestObjective { ObjectiveId = "recover_relic_blade", Type = QuestObjectiveType.CollectItem, TargetId = "item_iron_sword", RequiredAmount = 1, DescriptionKey = "quest.joaquim_echoes.obj_recover_relic_blade" },
                new QuestObjective { ObjectiveId = "talk_to_survivor", Type = QuestObjectiveType.TalkToNpc, TargetId = "npc_village_survivor", RequiredAmount = 1, DescriptionKey = "quest.joaquim_echoes.obj_talk_to_survivor" }
            };

            quest.SetDataForTests("quest_joaquim_echoes_of_home", "quest.joaquim_echoes.title", "quest.joaquim_echoes.description", QuestType.MainStory, objectives, rewardExperience: 500);
            return quest;
        }

        private static QuestDefinition BuildMaitheQuest()
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();

            var objectives = new List<QuestObjective>
            {
                new QuestObjective { ObjectiveId = "fell_stone_golems", Type = QuestObjectiveType.KillEnemyType, TargetId = "enemy_mountain_stone_golem", RequiredAmount = 4, DescriptionKey = "quest.maithe_windtrail.obj_fell_stone_golems" },
                new QuestObjective { ObjectiveId = "gather_marked_ore", Type = QuestObjectiveType.CollectItem, TargetId = "item_iron_ore", RequiredAmount = 3, DescriptionKey = "quest.maithe_windtrail.obj_gather_marked_ore" },
                new QuestObjective { ObjectiveId = "decode_glyphs", Type = QuestObjectiveType.CustomFlag, TargetId = "maithe_decoded_glyphs", RequiredAmount = 1, DescriptionKey = "quest.maithe_windtrail.obj_decode_glyphs" }
            };

            quest.SetDataForTests("quest_maithe_windtrail", "quest.maithe_windtrail.title", "quest.maithe_windtrail.description", QuestType.Side, objectives, rewardExperience: 450);
            return quest;
        }

        private static QuestDefinition BuildLavineQuest()
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();

            var objectives = new List<QuestObjective>
            {
                new QuestObjective { ObjectiveId = "resist_the_whispers", Type = QuestObjectiveType.CustomFlag, TargetId = "lavine_resisted_whispers", RequiredAmount = 1, DescriptionKey = "quest.lavine_redtide.obj_resist_the_whispers" },
                new QuestObjective { ObjectiveId = "cull_corrupted_swarm", Type = QuestObjectiveType.KillEnemyType, TargetId = "enemy_cave_bat_swarm", RequiredAmount = 8, DescriptionKey = "quest.lavine_redtide.obj_cull_corrupted_swarm" },
                new QuestObjective { ObjectiveId = "talk_to_hermit", Type = QuestObjectiveType.TalkToNpc, TargetId = "npc_hermit_mystic", RequiredAmount = 1, DescriptionKey = "quest.lavine_redtide.obj_talk_to_hermit" }
            };

            quest.SetDataForTests("quest_lavine_redtide", "quest.lavine_redtide.title", "quest.lavine_redtide.description", QuestType.MainStory, objectives, rewardExperience: 480);
            return quest;
        }

        private static DialogueDefinition BuildVillageSurvivorDialogue()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();

            var nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    Id = "greeting", SpeakerId = "npc_village_survivor", SpeakerNameKey = "dialogue.village_survivor.name", TextKey = "dialogue.village_survivor.greeting",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.village_survivor.choice_ask_destruction", NextNodeId = "destruction" },
                        new DialogueChoice { TextKey = "dialogue.village_survivor.choice_ask_joaquim", NextNodeId = "about_joaquim" },
                        new DialogueChoice { TextKey = "dialogue.common.choice_goodbye", NextNodeId = "" }
                    }
                },
                new DialogueNode { Id = "destruction", SpeakerId = "npc_village_survivor", SpeakerNameKey = "dialogue.village_survivor.name", TextKey = "dialogue.village_survivor.destruction", AutoAdvanceToNodeId = "offer_help" },
                new DialogueNode { Id = "about_joaquim", SpeakerId = "npc_village_survivor", SpeakerNameKey = "dialogue.village_survivor.name", TextKey = "dialogue.village_survivor.about_joaquim", AutoAdvanceToNodeId = "offer_help" },
                new DialogueNode
                {
                    Id = "offer_help", SpeakerId = "npc_village_survivor", SpeakerNameKey = "dialogue.village_survivor.name", TextKey = "dialogue.village_survivor.offer_help",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.village_survivor.choice_accept", NextNodeId = "accept" },
                        new DialogueChoice { TextKey = "dialogue.village_survivor.choice_decline", NextNodeId = "decline" }
                    }
                },
                new DialogueNode { Id = "accept", SpeakerId = "npc_village_survivor", SpeakerNameKey = "dialogue.village_survivor.name", TextKey = "dialogue.village_survivor.accept" },
                new DialogueNode { Id = "decline", SpeakerId = "npc_village_survivor", SpeakerNameKey = "dialogue.village_survivor.name", TextKey = "dialogue.village_survivor.decline" }
            };

            dialogue.SetDataForTests("dialogue_village_survivor", "greeting", nodes);
            return dialogue;
        }

        private static DialogueDefinition BuildWindTrailScholarDialogue()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();

            var nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    Id = "greeting", SpeakerId = "npc_windtrail_scholar", SpeakerNameKey = "dialogue.windtrail_scholar.name", TextKey = "dialogue.windtrail_scholar.greeting",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.windtrail_scholar.choice_ask_glyphs", NextNodeId = "glyphs" },
                        new DialogueChoice { TextKey = "dialogue.windtrail_scholar.choice_ask_maithe", NextNodeId = "about_maithe" },
                        new DialogueChoice { TextKey = "dialogue.common.choice_goodbye", NextNodeId = "" }
                    }
                },
                new DialogueNode
                {
                    Id = "glyphs", SpeakerId = "npc_windtrail_scholar", SpeakerNameKey = "dialogue.windtrail_scholar.name", TextKey = "dialogue.windtrail_scholar.glyphs",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.windtrail_scholar.choice_press_further", NextNodeId = "warning" },
                        new DialogueChoice { TextKey = "dialogue.common.choice_goodbye", NextNodeId = "" }
                    }
                },
                new DialogueNode { Id = "warning", SpeakerId = "npc_windtrail_scholar", SpeakerNameKey = "dialogue.windtrail_scholar.name", TextKey = "dialogue.windtrail_scholar.warning" },
                new DialogueNode { Id = "about_maithe", SpeakerId = "npc_windtrail_scholar", SpeakerNameKey = "dialogue.windtrail_scholar.name", TextKey = "dialogue.windtrail_scholar.about_maithe" }
            };

            dialogue.SetDataForTests("dialogue_windtrail_scholar", "greeting", nodes);
            return dialogue;
        }

        private static DialogueDefinition BuildHermitMysticDialogue()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();

            var nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    Id = "greeting", SpeakerId = "npc_hermit_mystic", SpeakerNameKey = "dialogue.hermit_mystic.name", TextKey = "dialogue.hermit_mystic.greeting",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.hermit_mystic.choice_ask_eclipse", NextNodeId = "eclipse" },
                        new DialogueChoice { TextKey = "dialogue.hermit_mystic.choice_ask_lavine", NextNodeId = "about_lavine" },
                        new DialogueChoice { TextKey = "dialogue.common.choice_goodbye", NextNodeId = "" }
                    }
                },
                new DialogueNode { Id = "eclipse", SpeakerId = "npc_hermit_mystic", SpeakerNameKey = "dialogue.hermit_mystic.name", TextKey = "dialogue.hermit_mystic.eclipse", AutoAdvanceToNodeId = "about_lavine" },
                new DialogueNode
                {
                    Id = "about_lavine", SpeakerId = "npc_hermit_mystic", SpeakerNameKey = "dialogue.hermit_mystic.name", TextKey = "dialogue.hermit_mystic.about_lavine",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.hermit_mystic.choice_defend_lavine", NextNodeId = "trust" },
                        new DialogueChoice { TextKey = "dialogue.hermit_mystic.choice_voice_doubt", NextNodeId = "doubt" }
                    }
                },
                new DialogueNode { Id = "trust", SpeakerId = "npc_hermit_mystic", SpeakerNameKey = "dialogue.hermit_mystic.name", TextKey = "dialogue.hermit_mystic.trust" },
                new DialogueNode { Id = "doubt", SpeakerId = "npc_hermit_mystic", SpeakerNameKey = "dialogue.hermit_mystic.name", TextKey = "dialogue.hermit_mystic.doubt" }
            };

            dialogue.SetDataForTests("dialogue_hermit_mystic", "greeting", nodes);
            return dialogue;
        }
    }
}
