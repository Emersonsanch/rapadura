using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Quests;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the quest-giver layer of <see cref="DialogueManager"/>: data-driven
    /// <see cref="DialogueCondition"/>s / <see cref="DialogueRedirect"/>s / <see cref="DialogueAction"/>s
    /// evaluated through <see cref="QuestDialogueContext"/> against a real <see cref="QuestManager"/>
    /// (no ServiceLocator, no Scene), plus a sanity pass over the authored quest-giver assets in
    /// Resources/Dialogue. Same construction approach as DialogueManagerTests/QuestManagerTests.
    /// </summary>
    public class DialogueQuestActionsTests
    {
        private const string QuestId = "quest_test_giver";
        private const string PrereqQuestId = "quest_test_prereq";

        private DialogueManager _dialogue;
        private QuestManager _quests;
        private Dictionary<string, QuestDefinition> _definitions;
        private string _activeCharacter;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();

            _quests = new QuestManager();
            _quests.Initialize();

            _definitions = new Dictionary<string, QuestDefinition>
            {
                { QuestId, BuildQuest(QuestId, new List<string>(),
                    Objective("kill_wolves", QuestObjectiveType.KillEnemyType, "wolf"),
                    Objective("decode", QuestObjectiveType.CustomFlag, "test_decoded")) },
                { PrereqQuestId, BuildQuest(PrereqQuestId, new List<string> { QuestId },
                    Objective("flag", QuestObjectiveType.CustomFlag, "prereq_flag")) }
            };

            _activeCharacter = null;

            _dialogue = new DialogueManager
            {
                Context = new QuestDialogueContext(
                    () => _quests,
                    id => _definitions.TryGetValue(id, out QuestDefinition d) ? d : null,
                    () => _activeCharacter)
            };
            _dialogue.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _dialogue.Shutdown();
            _quests.Shutdown();
            EventBus.Clear();

            foreach (QuestDefinition definition in _definitions.Values)
            {
                Object.DestroyImmediate(definition);
            }
        }

        // ------------------------------------------------------------------
        // Builders
        // ------------------------------------------------------------------

        private static QuestObjective Objective(string id, QuestObjectiveType type, string target)
        {
            return new QuestObjective { ObjectiveId = id, Type = type, TargetId = target, RequiredAmount = 1 };
        }

        private static QuestDefinition BuildQuest(string id, List<string> prerequisites, params QuestObjective[] objectives)
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();
            quest.SetDataForTests(id, id + ".title", id + ".desc", QuestType.MainStory, objectives.ToList(), prerequisites);
            return quest;
        }

        private static DialogueCondition QuestIs(string questId, DialogueQuestState state, bool negate = false)
        {
            return new DialogueCondition { Type = DialogueConditionType.QuestState, Key = questId, QuestState = state, Negate = negate };
        }

        private static DialogueNode Line(string id, params DialogueChoice[] choices)
        {
            return new DialogueNode { Id = id, SpeakerId = "npc_test", TextKey = "dialogue.test." + id, Choices = choices.ToList() };
        }

        private static DialogueDefinition Build(string dialogueId, params DialogueNode[] nodes)
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();
            dialogue.SetDataForTests(dialogueId, nodes[0].Id, nodes.ToList());
            return dialogue;
        }

        /// <summary>A minimal quest-giver hub: completed / active / available / locked, plus an accept choice.</summary>
        private DialogueDefinition BuildGiver(string questId, string dialogueId = "dialogue_test_giver")
        {
            DialogueNode hub = Line("hub");
            hub.Redirects = new List<DialogueRedirect>
            {
                new DialogueRedirect { TargetNodeId = "completed", Conditions = { QuestIs(questId, DialogueQuestState.Completed) } },
                new DialogueRedirect { TargetNodeId = "active", Conditions = { QuestIs(questId, DialogueQuestState.Active) } },
                new DialogueRedirect { TargetNodeId = "offer", Conditions = { QuestIs(questId, DialogueQuestState.Available) } },
                new DialogueRedirect { TargetNodeId = "locked" }
            };

            DialogueNode offer = Line("offer",
                new DialogueChoice
                {
                    TextKey = "dialogue.test.accept",
                    NextNodeId = "accepted",
                    Actions = { new DialogueAction { Type = DialogueActionType.StartQuest, Key = questId } }
                },
                new DialogueChoice { TextKey = "dialogue.test.decline", NextNodeId = "" });

            return Build(dialogueId, hub, offer, Line("accepted"), Line("active"), Line("completed"), Line("locked"));
        }

        // ------------------------------------------------------------------
        // Actions
        // ------------------------------------------------------------------

        [Test]
        public void StartQuestAction_OnChoice_StartsQuestAndPublishesQuestStartedEvent()
        {
            string started = null;
            EventBus.Subscribe<QuestStartedEvent>(evt => started = evt.QuestId);

            _dialogue.StartDialogue(BuildGiver(QuestId));
            Assert.AreEqual("offer", _dialogue.CurrentNode.Id);

            _dialogue.SelectChoice(0);

            Assert.IsTrue(_quests.IsActive(QuestId));
            Assert.AreEqual(QuestId, started);
            Assert.AreEqual("accepted", _dialogue.CurrentNode.Id);
        }

        [Test]
        public void DeclineChoice_DoesNotStartQuest()
        {
            _dialogue.StartDialogue(BuildGiver(QuestId));

            _dialogue.SelectChoice(1);

            Assert.IsFalse(_quests.IsActive(QuestId));
            Assert.IsFalse(_dialogue.IsActive);
        }

        [Test]
        public void SetQuestFlagAction_ProgressesCustomFlagObjective_AndCompletesQuest()
        {
            _quests.StartQuest(_definitions[QuestId]);
            _quests.UpdateObjectiveProgress(QuestObjectiveType.KillEnemyType, "wolf");

            DialogueDefinition turnIn = Build("dialogue_test_turn_in",
                Line("root", new DialogueChoice
                {
                    TextKey = "dialogue.test.decode",
                    NextNodeId = "",
                    Actions = { new DialogueAction { Type = DialogueActionType.SetQuestFlag, Key = "test_decoded" } }
                }));

            _dialogue.StartDialogue(turnIn);
            _dialogue.SelectChoice(0);

            Assert.IsTrue(_quests.IsCompleted(QuestId));
        }

        [Test]
        public void CompleteQuestAction_IsForwardedToContext()
        {
            var fake = new FakeContext();
            _dialogue.Context = fake;

            _dialogue.StartDialogue(Build("dialogue_test_complete",
                Line("root", new DialogueChoice
                {
                    TextKey = "dialogue.test.hand_in",
                    NextNodeId = "",
                    Actions = { new DialogueAction { Type = DialogueActionType.CompleteQuest, Key = "quest_x" } }
                })));
            _dialogue.SelectChoice(0);

            CollectionAssert.AreEqual(new[] { "quest_x" }, fake.Completed);
        }

        [Test]
        public void FlagActions_SetAndClearDialogueFlags()
        {
            _dialogue.RunActions(new List<DialogueAction> { new DialogueAction { Type = DialogueActionType.SetFlag, Key = "met_npc" } });
            Assert.IsTrue(_dialogue.HasFlag("met_npc"));

            _dialogue.RunActions(new List<DialogueAction> { new DialogueAction { Type = DialogueActionType.ClearFlag, Key = "met_npc" } });
            Assert.IsFalse(_dialogue.HasFlag("met_npc"));
        }

        [Test]
        public void OnEnterActions_RunWhenNodeIsShown()
        {
            DialogueNode root = Line("root");
            root.AutoAdvanceToNodeId = "gift";
            DialogueNode gift = Line("gift");
            gift.OnEnterActions = new List<DialogueAction> { new DialogueAction { Type = DialogueActionType.StartQuest, Key = QuestId } };

            _dialogue.StartDialogue(Build("dialogue_test_enter", root, gift));
            Assert.IsFalse(_quests.IsActive(QuestId));

            _dialogue.AdvanceCurrentLine();

            Assert.IsTrue(_quests.IsActive(QuestId));
        }

        [Test]
        public void StartQuestAction_WithUnmetPrerequisites_DoesNothing()
        {
            _dialogue.RunActions(new List<DialogueAction> { new DialogueAction { Type = DialogueActionType.StartQuest, Key = PrereqQuestId } });

            Assert.IsFalse(_quests.IsActive(PrereqQuestId));
        }

        [Test]
        public void StartQuestAction_WithUnknownQuestId_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _dialogue.RunActions(new List<DialogueAction>
            {
                new DialogueAction { Type = DialogueActionType.StartQuest, Key = "quest_does_not_exist" }
            }));
        }

        // ------------------------------------------------------------------
        // Quest-state branching
        // ------------------------------------------------------------------

        [Test]
        public void Hub_RoutesByQuestState_AvailableActiveCompleted()
        {
            DialogueDefinition giver = BuildGiver(QuestId);

            _dialogue.StartDialogue(giver);
            Assert.AreEqual("offer", _dialogue.CurrentNode.Id);
            _dialogue.EndDialogue();

            _quests.StartQuest(_definitions[QuestId]);
            _dialogue.StartDialogue(giver);
            Assert.AreEqual("active", _dialogue.CurrentNode.Id);
            _dialogue.EndDialogue();

            _quests.UpdateObjectiveProgress(QuestObjectiveType.KillEnemyType, "wolf");
            _quests.SetCustomFlag("test_decoded");
            _dialogue.StartDialogue(giver);
            Assert.AreEqual("completed", _dialogue.CurrentNode.Id);
        }

        [Test]
        public void Hub_WithUnmetPrerequisites_FallsBackToLockedNode_ThenOffersOnceMet()
        {
            DialogueDefinition giver = BuildGiver(PrereqQuestId);

            _dialogue.StartDialogue(giver);
            Assert.AreEqual("locked", _dialogue.CurrentNode.Id);
            _dialogue.EndDialogue();

            _quests.StartQuest(_definitions[QuestId]);
            _quests.UpdateObjectiveProgress(QuestObjectiveType.KillEnemyType, "wolf");
            _quests.SetCustomFlag("test_decoded");

            _dialogue.StartDialogue(giver);
            Assert.AreEqual("offer", _dialogue.CurrentNode.Id);
        }

        [Test]
        public void QuestDialogueContext_ReportsStates()
        {
            IDialogueContext context = _dialogue.Context;

            Assert.AreEqual(DialogueQuestState.Available, context.GetQuestState(QuestId));
            Assert.AreEqual(DialogueQuestState.NotStarted, context.GetQuestState(PrereqQuestId));

            _quests.StartQuest(_definitions[QuestId]);
            Assert.AreEqual(DialogueQuestState.Active, context.GetQuestState(QuestId));

            _quests.UpdateObjectiveProgress(QuestObjectiveType.KillEnemyType, "wolf");
            Assert.IsTrue(context.IsQuestObjectiveComplete(QuestId, "kill_wolves"));
            Assert.IsFalse(context.IsQuestObjectiveComplete(QuestId, "decode"));

            _quests.SetCustomFlag("test_decoded");
            Assert.AreEqual(DialogueQuestState.Completed, context.GetQuestState(QuestId));
            Assert.IsTrue(context.IsQuestObjectiveComplete(QuestId, "decode"));
        }

        [Test]
        public void MatchesQuestState_RefinedStatesMatchTheirParents()
        {
            Assert.IsTrue(DialogueCondition.MatchesQuestState(DialogueQuestState.NotStarted, DialogueQuestState.Available));
            Assert.IsTrue(DialogueCondition.MatchesQuestState(DialogueQuestState.Active, DialogueQuestState.ObjectivesComplete));
            Assert.IsFalse(DialogueCondition.MatchesQuestState(DialogueQuestState.Available, DialogueQuestState.NotStarted));
            Assert.IsFalse(DialogueCondition.MatchesQuestState(DialogueQuestState.ObjectivesComplete, DialogueQuestState.Active));
            Assert.IsFalse(DialogueCondition.MatchesQuestState(DialogueQuestState.Completed, DialogueQuestState.Active));
        }

        [Test]
        public void ObjectiveCompleteCondition_GatesTurnInChoice()
        {
            _quests.StartQuest(_definitions[QuestId]);

            DialogueDefinition dialogue = Build("dialogue_test_gate",
                Line("root",
                    new DialogueChoice
                    {
                        TextKey = "dialogue.test.turn_in",
                        Conditions = { new DialogueCondition { Type = DialogueConditionType.QuestObjectiveComplete, Key = QuestId, Value = "kill_wolves" } }
                    },
                    new DialogueChoice { TextKey = "dialogue.test.bye" }));

            _dialogue.StartDialogue(dialogue);
            Assert.AreEqual(1, _dialogue.AvailableChoices.Count);
            _dialogue.EndDialogue();

            _quests.UpdateObjectiveProgress(QuestObjectiveType.KillEnemyType, "wolf");

            _dialogue.StartDialogue(dialogue);
            Assert.AreEqual(2, _dialogue.AvailableChoices.Count);
            Assert.AreEqual("dialogue.test.turn_in", _dialogue.AvailableChoices[0].TextKey);
        }

        [Test]
        public void ChoiceConditions_AreAndedWithLegacyConditionFlag()
        {
            _quests.StartQuest(_definitions[QuestId]);

            var choice = new DialogueChoice
            {
                TextKey = "dialogue.test.both",
                ConditionFlag = "has_key",
                Conditions = { QuestIs(QuestId, DialogueQuestState.Active) }
            };
            DialogueDefinition dialogue = Build("dialogue_test_and", Line("root", choice));

            _dialogue.StartDialogue(dialogue);
            Assert.AreEqual(0, _dialogue.AvailableChoices.Count);
            _dialogue.EndDialogue();

            _dialogue.SetFlag("has_key");
            _dialogue.StartDialogue(dialogue);
            Assert.AreEqual(1, _dialogue.AvailableChoices.Count);
        }

        [Test]
        public void TalkToNpcObjective_ProgressesWhenGiverDialogueEndsAfterAccepting()
        {
            var talkQuest = BuildQuest("quest_test_talk", new List<string>(),
                Objective("talk", QuestObjectiveType.TalkToNpc, "npc_test_elder"),
                Objective("kill", QuestObjectiveType.KillEnemyType, "wolf"));
            _definitions.Add(talkQuest.QuestId, talkQuest);

            _dialogue.StartDialogue(BuildGiver(talkQuest.QuestId, "dialogue_test_elder"));
            _dialogue.SelectChoice(0); // accept -> "accepted" (terminal)
            _dialogue.AdvanceCurrentLine(); // ends -> DialogueEndedEvent

            Assert.IsTrue(_quests.GetProgress(talkQuest.QuestId).Objectives.First(o => o.ObjectiveId == "talk").IsComplete);
        }

        // ------------------------------------------------------------------
        // Per-hero personalization
        // ------------------------------------------------------------------

        private static DialogueDefinition BuildPersonalized()
        {
            DialogueNode root = Line("generic");
            root.Redirects = new List<DialogueRedirect>
            {
                new DialogueRedirect { TargetNodeId = "for_maithe", Conditions = { new DialogueCondition { Type = DialogueConditionType.ActiveCharacter, Key = "Maithe" } } },
                new DialogueRedirect { TargetNodeId = "for_icaro", Conditions = { new DialogueCondition { Type = DialogueConditionType.ActiveCharacter, Key = "Icaro" } } }
            };

            return Build("dialogue_test_personal", root, Line("for_maithe"), Line("for_icaro"));
        }

        [Test]
        public void ActiveCharacterRedirect_PicksHeroSpecificNode()
        {
            _activeCharacter = "Icaro";

            _dialogue.StartDialogue(BuildPersonalized());

            Assert.AreEqual("for_icaro", _dialogue.CurrentNode.Id);
        }

        [Test]
        public void ActiveCharacterRedirect_IsCaseInsensitive()
        {
            _activeCharacter = "maithe";

            _dialogue.StartDialogue(BuildPersonalized());

            Assert.AreEqual("for_maithe", _dialogue.CurrentNode.Id);
        }

        [Test]
        public void ActiveCharacterRedirect_WithNoSelectionOrOtherHero_ShowsGenericNode()
        {
            _dialogue.StartDialogue(BuildPersonalized());
            Assert.AreEqual("generic", _dialogue.CurrentNode.Id);
            _dialogue.EndDialogue();

            _activeCharacter = "Lavine";
            _dialogue.StartDialogue(BuildPersonalized());
            Assert.AreEqual("generic", _dialogue.CurrentNode.Id);
        }

        [Test]
        public void NegatedActiveCharacterCondition_HidesChoiceForThatHero()
        {
            var choice = new DialogueChoice
            {
                TextKey = "dialogue.test.ask_about_joaquim",
                Conditions = { new DialogueCondition { Type = DialogueConditionType.ActiveCharacter, Key = "Joaquim", Negate = true } }
            };
            DialogueDefinition dialogue = Build("dialogue_test_negate", Line("root", choice, new DialogueChoice { TextKey = "dialogue.test.bye" }));

            _activeCharacter = "Joaquim";
            _dialogue.StartDialogue(dialogue);
            Assert.AreEqual(1, _dialogue.AvailableChoices.Count);
            _dialogue.EndDialogue();

            _activeCharacter = "Maria";
            _dialogue.StartDialogue(dialogue);
            Assert.AreEqual(2, _dialogue.AvailableChoices.Count);
        }

        [Test]
        public void RedirectCycle_StopsInsteadOfHanging()
        {
            DialogueNode a = Line("a");
            DialogueNode b = Line("b");
            a.Redirects = new List<DialogueRedirect> { new DialogueRedirect { TargetNodeId = "b" } };
            b.Redirects = new List<DialogueRedirect> { new DialogueRedirect { TargetNodeId = "a" } };

            Assert.DoesNotThrow(() => _dialogue.StartDialogue(Build("dialogue_test_cycle", a, b)));
            Assert.IsTrue(_dialogue.IsActive);
        }

        [Test]
        public void RedirectToMissingNode_IsIgnored()
        {
            DialogueNode root = Line("root");
            root.Redirects = new List<DialogueRedirect> { new DialogueRedirect { TargetNodeId = "missing" } };

            _dialogue.StartDialogue(Build("dialogue_test_missing_redirect", root));

            Assert.AreEqual("root", _dialogue.CurrentNode.Id);
        }

        // ------------------------------------------------------------------
        // Authored content (Resources/Dialogue)
        // ------------------------------------------------------------------

        private static readonly string[] QuestGiverDialogues =
        {
            "dialogue_village_survivor",
            "dialogue_windtrail_scholar",
            "dialogue_hermit_mystic",
            "dialogue_temple_acolyte",
            "dialogue_ruins_archivist",
            "dialogue_maria_gathering_point"
        };

        [Test]
        public void AuthoredQuestGivers_StartEveryQuestInResources()
        {
            var startedQuestIds = new HashSet<string>();

            foreach (string id in QuestGiverDialogues)
            {
                var dialogue = Resources.Load<DialogueDefinition>("Dialogue/" + id);
                Assert.IsNotNull(dialogue, id);

                foreach (DialogueAction action in AllActions(dialogue).Where(a => a.Type == DialogueActionType.StartQuest))
                {
                    startedQuestIds.Add(action.Key);
                }
            }

            foreach (QuestDefinition quest in Resources.LoadAll<QuestDefinition>("Quests"))
            {
                CollectionAssert.Contains(startedQuestIds, quest.QuestId, $"No quest-giver dialogue starts '{quest.QuestId}'.");
                Assert.IsNotNull(Resources.Load<QuestDefinition>("Quests/" + quest.QuestId), $"QuestDialogueContext can't resolve '{quest.QuestId}' by id.");
            }
        }

        [Test]
        public void AuthoredQuestGivers_HaveValidLinksAndLocalizedText()
        {
            var localization = new LocalizationManager();
            localization.Initialize();
            QuestGiverTexts.EnsureLoaded(localization);

            foreach (string id in QuestGiverDialogues)
            {
                var dialogue = Resources.Load<DialogueDefinition>("Dialogue/" + id);
                Assert.IsNotNull(dialogue.GetStartNode(), id);

                foreach (DialogueNode node in dialogue.Nodes)
                {
                    foreach (DialogueRedirect redirect in node.Redirects)
                    {
                        Assert.IsNotNull(dialogue.GetNode(redirect.TargetNodeId), $"{id}/{node.Id} -> {redirect.TargetNodeId}");
                    }

                    foreach (DialogueChoice choice in node.Choices)
                    {
                        Assert.IsTrue(string.IsNullOrEmpty(choice.NextNodeId) || dialogue.GetNode(choice.NextNodeId) != null, $"{id}/{node.Id} -> {choice.NextNodeId}");
                        Assert.IsTrue(localization.HasKey(choice.TextKey), $"{id}/{node.Id}: missing key '{choice.TextKey}'");
                    }

                    if (!string.IsNullOrEmpty(node.TextKey))
                    {
                        Assert.IsTrue(localization.HasKey(node.TextKey), $"{id}/{node.Id}: missing key '{node.TextKey}'");
                    }
                }
            }

            localization.Shutdown();
        }

        private static IEnumerable<DialogueAction> AllActions(DialogueDefinition dialogue)
        {
            foreach (DialogueNode node in dialogue.Nodes)
            {
                foreach (DialogueAction action in node.OnEnterActions)
                {
                    yield return action;
                }

                foreach (DialogueChoice choice in node.Choices)
                {
                    foreach (DialogueAction action in choice.Actions)
                    {
                        yield return action;
                    }
                }
            }
        }

        private class FakeContext : IDialogueContext
        {
            public readonly List<string> Completed = new List<string>();

            public string ActiveCharacterId => null;
            public DialogueQuestState GetQuestState(string questId) => DialogueQuestState.NotStarted;
            public bool IsQuestObjectiveComplete(string questId, string objectiveId) => false;
            public bool StartQuest(string questId) => false;

            public bool CompleteQuest(string questId)
            {
                Completed.Add(questId);
                return true;
            }

            public void SetQuestFlag(string flagName)
            {
            }
        }
    }
}
