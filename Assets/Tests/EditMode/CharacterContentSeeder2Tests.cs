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
    /// EditMode tests for the second wave of character-story content authored in
    /// <c>Assets/Scripts/Editor/CharacterQuestSeeder2.cs</c> and
    /// <c>Assets/Scripts/Editor/CharacterDialogueSeeder2.cs</c> (Maria &amp; Ícaro).
    ///
    /// Those seeders are Editor-only (<c>UnityEditor.AssetDatabase</c> calls, [MenuItem]) and live
    /// outside this assembly's references, so this suite does NOT invoke them. Instead it rebuilds
    /// the same data (ids/objectives/nodes, kept in lockstep with the seeders by hand) via the same
    /// plain-C# <c>SetDataForTests</c> construction path the seeders themselves use internally, and
    /// asserts on the resulting <see cref="QuestDefinition"/>/<see cref="DialogueDefinition"/>
    /// structure — mirroring <c>CharacterContentSeederTests</c> for the first wave (Joaquim/Maithe/
    /// Lavine). It also checks that every localization key referenced by this content has a real
    /// entry in <see cref="LocalizationManager.DefaultEntries"/> (both en and pt-BR).
    /// </summary>
    public class CharacterContentSeeder2Tests
    {
        // ------------------------------------------------------------------
        // Quest structure (mirrors CharacterQuestSeeder2.cs)
        // ------------------------------------------------------------------

        [Test]
        public void MariaQuest_HasExpectedIdTypeAndObjectives()
        {
            QuestDefinition quest = BuildMariaQuest();

            Assert.AreEqual("quest_maria_fading_light", quest.QuestId);
            Assert.AreEqual(QuestType.MainStory, quest.QuestType);
            Assert.AreEqual(3, quest.Objectives.Count);

            Assert.AreEqual(QuestObjectiveType.KillEnemyType, quest.Objectives[0].Type);
            Assert.AreEqual("enemy_cave_bat_swarm", quest.Objectives[0].TargetId);
            Assert.AreEqual(5, quest.Objectives[0].RequiredAmount);

            Assert.AreEqual(QuestObjectiveType.CollectItem, quest.Objectives[1].Type);
            Assert.AreEqual("item_iron_ore", quest.Objectives[1].TargetId);
            Assert.AreEqual(2, quest.Objectives[1].RequiredAmount);

            Assert.AreEqual(QuestObjectiveType.TalkToNpc, quest.Objectives[2].Type);
            Assert.AreEqual("npc_temple_acolyte", quest.Objectives[2].TargetId);

            Assert.AreEqual(500, quest.RewardExperience);
        }

        [Test]
        public void IcaroQuest_HasExpectedIdTypeAndObjectives()
        {
            QuestDefinition quest = BuildIcaroQuest();

            Assert.AreEqual("quest_icaro_ancient_gears", quest.QuestId);
            Assert.AreEqual(QuestType.MainStory, quest.QuestType);
            Assert.AreEqual(3, quest.Objectives.Count);

            Assert.AreEqual(QuestObjectiveType.KillEnemyType, quest.Objectives[0].Type);
            Assert.AreEqual("enemy_desert_ruin_guardian", quest.Objectives[0].TargetId);
            Assert.AreEqual(1, quest.Objectives[0].RequiredAmount);

            Assert.AreEqual(QuestObjectiveType.CollectItem, quest.Objectives[1].Type);
            Assert.AreEqual("item_iron", quest.Objectives[1].TargetId);

            Assert.AreEqual(QuestObjectiveType.CustomFlag, quest.Objectives[2].Type);
            Assert.AreEqual("icaro_decoded_fragment", quest.Objectives[2].TargetId);

            Assert.AreEqual(520, quest.RewardExperience);
        }

        [Test]
        public void BothQuests_HaveUniqueIdsAndNonEmptyDescriptionKeys()
        {
            var quests = new List<QuestDefinition> { BuildMariaQuest(), BuildIcaroQuest() };

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

        [Test]
        public void SecondWaveQuestIds_DoNotCollideWithFirstWave()
        {
            var secondWaveIds = new[] { "quest_maria_fading_light", "quest_icaro_ancient_gears" };
            var firstWaveIds = new[] { "quest_joaquim_echoes_of_home", "quest_maithe_windtrail", "quest_lavine_redtide" };

            foreach (string id in secondWaveIds)
            {
                Assert.IsFalse(firstWaveIds.Contains(id), $"Quest id '{id}' collides with first-wave content.");
            }
        }

        // ------------------------------------------------------------------
        // Dialogue structure (mirrors CharacterDialogueSeeder2.cs)
        // ------------------------------------------------------------------

        [Test]
        public void TempleAcolyteDialogue_HasBranchingStartingAtGreeting()
        {
            DialogueDefinition dialogue = BuildTempleAcolyteDialogue();

            Assert.AreEqual("dialogue_temple_acolyte", dialogue.DialogueId);
            Assert.AreEqual(6, dialogue.Nodes.Count);

            DialogueNode start = dialogue.GetStartNode();
            Assert.IsNotNull(start);
            Assert.AreEqual("greeting", start.Id);
            Assert.AreEqual(3, start.Choices.Count, "Greeting should branch into crystals/about_maria/goodbye.");

            DialogueNode offerJoin = dialogue.GetNode("offer_join");
            Assert.IsNotNull(offerJoin);
            Assert.AreEqual(2, offerJoin.Choices.Count, "offer_join should branch into pledge/watchful.");

            DialogueNode pledge = dialogue.GetNode(offerJoin.Choices[0].NextNodeId);
            Assert.IsNotNull(pledge);
            Assert.IsTrue(pledge.IsTerminal);
        }

        [Test]
        public void RuinsArchivistDialogue_HasBranchingStartingAtGreeting()
        {
            DialogueDefinition dialogue = BuildRuinsArchivistDialogue();

            Assert.AreEqual("dialogue_ruins_archivist", dialogue.DialogueId);
            Assert.AreEqual(5, dialogue.Nodes.Count);

            DialogueNode start = dialogue.GetStartNode();
            Assert.AreEqual(3, start.Choices.Count);

            DialogueNode fragment = dialogue.GetNode("fragment");
            Assert.IsNotNull(fragment);
            Assert.AreEqual(2, fragment.Choices.Count, "fragment should branch into share/withhold.");
        }

        [Test]
        public void SecondWaveDialogueIds_DoNotCollideWithFirstWave()
        {
            var secondWaveIds = new[] { "dialogue_temple_acolyte", "dialogue_ruins_archivist" };
            var firstWaveIds = new[] { "dialogue_village_survivor", "dialogue_windtrail_scholar", "dialogue_hermit_mystic" };

            foreach (string id in secondWaveIds)
            {
                Assert.IsFalse(firstWaveIds.Contains(id), $"Dialogue id '{id}' collides with first-wave content.");
            }
        }

        // ------------------------------------------------------------------
        // Localization coverage: every key referenced by the content above must be authored.
        // ------------------------------------------------------------------

        [Test]
        public void AllSecondWaveLocalizationKeys_ExistInDefaultEntries()
        {
            var entriesByKey = LocalizationManager.DefaultEntries.All.ToDictionary(e => e.Key, e => e);

            var quests = new List<QuestDefinition> { BuildMariaQuest(), BuildIcaroQuest() };
            var dialogues = new List<DialogueDefinition> { BuildTempleAcolyteDialogue(), BuildRuinsArchivistDialogue() };

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
        // Builders — kept in lockstep with CharacterQuestSeeder2.cs / CharacterDialogueSeeder2.cs.
        // ------------------------------------------------------------------

        private static QuestDefinition BuildMariaQuest()
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();

            var objectives = new List<QuestObjective>
            {
                new QuestObjective { ObjectiveId = "cleanse_corrupted_wildlife", Type = QuestObjectiveType.KillEnemyType, TargetId = "enemy_cave_bat_swarm", RequiredAmount = 5, DescriptionKey = "quest.maria_fading_light.obj_cleanse_corrupted_wildlife" },
                new QuestObjective { ObjectiveId = "gather_crystal_shard", Type = QuestObjectiveType.CollectItem, TargetId = "item_iron_ore", RequiredAmount = 2, DescriptionKey = "quest.maria_fading_light.obj_gather_crystal_shard" },
                new QuestObjective { ObjectiveId = "talk_to_temple_acolyte", Type = QuestObjectiveType.TalkToNpc, TargetId = "npc_temple_acolyte", RequiredAmount = 1, DescriptionKey = "quest.maria_fading_light.obj_talk_to_temple_acolyte" }
            };

            quest.SetDataForTests("quest_maria_fading_light", "quest.maria_fading_light.title", "quest.maria_fading_light.description", QuestType.MainStory, objectives, rewardExperience: 500);
            return quest;
        }

        private static QuestDefinition BuildIcaroQuest()
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();

            var objectives = new List<QuestObjective>
            {
                new QuestObjective { ObjectiveId = "defeat_ruin_guardian", Type = QuestObjectiveType.KillEnemyType, TargetId = "enemy_desert_ruin_guardian", RequiredAmount = 1, DescriptionKey = "quest.icaro_ancient_gears.obj_defeat_ruin_guardian" },
                new QuestObjective { ObjectiveId = "recover_tech_fragment", Type = QuestObjectiveType.CollectItem, TargetId = "item_iron", RequiredAmount = 1, DescriptionKey = "quest.icaro_ancient_gears.obj_recover_tech_fragment" },
                new QuestObjective { ObjectiveId = "decode_fragment", Type = QuestObjectiveType.CustomFlag, TargetId = "icaro_decoded_fragment", RequiredAmount = 1, DescriptionKey = "quest.icaro_ancient_gears.obj_decode_fragment" }
            };

            quest.SetDataForTests("quest_icaro_ancient_gears", "quest.icaro_ancient_gears.title", "quest.icaro_ancient_gears.description", QuestType.MainStory, objectives, rewardExperience: 520);
            return quest;
        }

        private static DialogueDefinition BuildTempleAcolyteDialogue()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();

            var nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    Id = "greeting", SpeakerId = "npc_temple_acolyte", SpeakerNameKey = "dialogue.temple_acolyte.name", TextKey = "dialogue.temple_acolyte.greeting",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.temple_acolyte.choice_ask_crystals", NextNodeId = "crystals" },
                        new DialogueChoice { TextKey = "dialogue.temple_acolyte.choice_ask_maria", NextNodeId = "about_maria" },
                        new DialogueChoice { TextKey = "dialogue.common.choice_goodbye", NextNodeId = "" }
                    }
                },
                new DialogueNode { Id = "crystals", SpeakerId = "npc_temple_acolyte", SpeakerNameKey = "dialogue.temple_acolyte.name", TextKey = "dialogue.temple_acolyte.crystals", AutoAdvanceToNodeId = "offer_join" },
                new DialogueNode { Id = "about_maria", SpeakerId = "npc_temple_acolyte", SpeakerNameKey = "dialogue.temple_acolyte.name", TextKey = "dialogue.temple_acolyte.about_maria", AutoAdvanceToNodeId = "offer_join" },
                new DialogueNode
                {
                    Id = "offer_join", SpeakerId = "npc_temple_acolyte", SpeakerNameKey = "dialogue.temple_acolyte.name", TextKey = "dialogue.temple_acolyte.offer_join",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.temple_acolyte.choice_pledge_help", NextNodeId = "pledge" },
                        new DialogueChoice { TextKey = "dialogue.temple_acolyte.choice_stay_watchful", NextNodeId = "watchful" }
                    }
                },
                new DialogueNode { Id = "pledge", SpeakerId = "npc_temple_acolyte", SpeakerNameKey = "dialogue.temple_acolyte.name", TextKey = "dialogue.temple_acolyte.pledge" },
                new DialogueNode { Id = "watchful", SpeakerId = "npc_temple_acolyte", SpeakerNameKey = "dialogue.temple_acolyte.name", TextKey = "dialogue.temple_acolyte.watchful" }
            };

            dialogue.SetDataForTests("dialogue_temple_acolyte", "greeting", nodes);
            return dialogue;
        }

        private static DialogueDefinition BuildRuinsArchivistDialogue()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();

            var nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    Id = "greeting", SpeakerId = "npc_ruins_archivist", SpeakerNameKey = "dialogue.ruins_archivist.name", TextKey = "dialogue.ruins_archivist.greeting",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.ruins_archivist.choice_ask_fragment", NextNodeId = "fragment" },
                        new DialogueChoice { TextKey = "dialogue.ruins_archivist.choice_ask_icaro", NextNodeId = "about_icaro" },
                        new DialogueChoice { TextKey = "dialogue.common.choice_goodbye", NextNodeId = "" }
                    }
                },
                new DialogueNode
                {
                    Id = "fragment", SpeakerId = "npc_ruins_archivist", SpeakerNameKey = "dialogue.ruins_archivist.name", TextKey = "dialogue.ruins_archivist.fragment",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.ruins_archivist.choice_share_openly", NextNodeId = "share" },
                        new DialogueChoice { TextKey = "dialogue.ruins_archivist.choice_withhold_secret", NextNodeId = "withhold" }
                    }
                },
                new DialogueNode { Id = "about_icaro", SpeakerId = "npc_ruins_archivist", SpeakerNameKey = "dialogue.ruins_archivist.name", TextKey = "dialogue.ruins_archivist.about_icaro" },
                new DialogueNode { Id = "share", SpeakerId = "npc_ruins_archivist", SpeakerNameKey = "dialogue.ruins_archivist.name", TextKey = "dialogue.ruins_archivist.share" },
                new DialogueNode { Id = "withhold", SpeakerId = "npc_ruins_archivist", SpeakerNameKey = "dialogue.ruins_archivist.name", TextKey = "dialogue.ruins_archivist.withhold" }
            };

            dialogue.SetDataForTests("dialogue_ruins_archivist", "greeting", nodes);
            return dialogue;
        }
    }
}
