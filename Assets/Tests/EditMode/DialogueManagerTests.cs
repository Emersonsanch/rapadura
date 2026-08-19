using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.Dialogue;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="DialogueManager"/>: node navigation, choice selection
    /// (including conditional choices), and dialogue termination. Deliberately builds
    /// <see cref="DialogueDefinition"/> instances via <see cref="ScriptableObject.CreateInstance{T}"/>
    /// + <see cref="DialogueDefinition.SetDataForTests"/> instead of loading real assets, and never
    /// touches <see cref="UnityEngine.UIElements.UIDocument"/> — DialogueUIController is UI-only and
    /// intentionally not exercised here, matching how AudioManagerTests avoids asserting on actual
    /// sound playback.
    /// </summary>
    public class DialogueManagerTests
    {
        private DialogueManager _manager;

        [SetUp]
        public void SetUp()
        {
            _manager = new DialogueManager();
            _manager.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _manager.Shutdown();
        }

        private static DialogueDefinition BuildLinearDialogue()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();

            var nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    Id = "start",
                    SpeakerId = "npc_test",
                    TextKey = "dialogue.test.start",
                    AutoAdvanceToNodeId = "end"
                },
                new DialogueNode
                {
                    Id = "end",
                    SpeakerId = "npc_test",
                    TextKey = "dialogue.test.end"
                }
            };

            dialogue.SetDataForTests("dialogue_test_linear", "start", nodes);
            return dialogue;
        }

        private static DialogueDefinition BuildBranchingDialogue()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();

            var nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    Id = "root",
                    SpeakerId = "npc_test",
                    TextKey = "dialogue.test.root",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.test.choice_a", NextNodeId = "branch_a" },
                        new DialogueChoice { TextKey = "dialogue.test.choice_b", NextNodeId = "branch_b" },
                        new DialogueChoice { TextKey = "dialogue.test.choice_bye", NextNodeId = "" }
                    }
                },
                new DialogueNode { Id = "branch_a", SpeakerId = "npc_test", TextKey = "dialogue.test.branch_a" },
                new DialogueNode { Id = "branch_b", SpeakerId = "npc_test", TextKey = "dialogue.test.branch_b" }
            };

            dialogue.SetDataForTests("dialogue_test_branching", "root", nodes);
            return dialogue;
        }

        private static DialogueDefinition BuildConditionalDialogue()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();

            var nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    Id = "root",
                    SpeakerId = "npc_test",
                    TextKey = "dialogue.test.root",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.test.locked_choice", NextNodeId = "locked_branch", ConditionFlag = "has_key" },
                        new DialogueChoice { TextKey = "dialogue.test.only_without_flag", NextNodeId = "negated_branch", ConditionFlag = "has_key", NegateCondition = true },
                        new DialogueChoice { TextKey = "dialogue.test.choice_bye", NextNodeId = "" }
                    }
                },
                new DialogueNode { Id = "locked_branch", SpeakerId = "npc_test", TextKey = "dialogue.test.locked_branch" },
                new DialogueNode { Id = "negated_branch", SpeakerId = "npc_test", TextKey = "dialogue.test.negated_branch" }
            };

            dialogue.SetDataForTests("dialogue_test_conditional", "root", nodes);
            return dialogue;
        }

        [Test]
        public void StartDialogue_SetsCurrentNodeToStartNode()
        {
            DialogueDefinition dialogue = BuildBranchingDialogue();

            _manager.StartDialogue(dialogue);

            Assert.IsTrue(_manager.IsActive);
            Assert.AreEqual("root", _manager.CurrentNode.Id);
            Assert.AreEqual(dialogue, _manager.ActiveDialogue);
        }

        [Test]
        public void StartDialogue_WithNull_DoesNotActivate()
        {
            _manager.StartDialogue(null);

            Assert.IsFalse(_manager.IsActive);
        }

        [Test]
        public void StartDialogue_WhileAlreadyActive_IsIgnored()
        {
            DialogueDefinition first = BuildBranchingDialogue();
            DialogueDefinition second = BuildLinearDialogue();

            _manager.StartDialogue(first);
            _manager.StartDialogue(second);

            Assert.AreEqual(first, _manager.ActiveDialogue);
        }

        [Test]
        public void SelectChoice_NavigatesToTargetNode()
        {
            _manager.StartDialogue(BuildBranchingDialogue());

            _manager.SelectChoice(1); // choice_b -> branch_b

            Assert.IsTrue(_manager.IsActive);
            Assert.AreEqual("branch_b", _manager.CurrentNode.Id);
        }

        [Test]
        public void SelectChoice_WithEmptyNextNodeId_EndsDialogue()
        {
            _manager.StartDialogue(BuildBranchingDialogue());

            _manager.SelectChoice(2); // "choice_bye" -> ""

            Assert.IsFalse(_manager.IsActive);
        }

        [Test]
        public void SelectChoice_OutOfRange_IsIgnoredAndKeepsDialogueActive()
        {
            _manager.StartDialogue(BuildBranchingDialogue());

            _manager.SelectChoice(99);

            Assert.IsTrue(_manager.IsActive);
            Assert.AreEqual("root", _manager.CurrentNode.Id);
        }

        [Test]
        public void AdvanceCurrentLine_FollowsAutoAdvanceLink()
        {
            _manager.StartDialogue(BuildLinearDialogue());

            Assert.AreEqual("start", _manager.CurrentNode.Id);

            _manager.AdvanceCurrentLine();

            Assert.AreEqual("end", _manager.CurrentNode.Id);
        }

        [Test]
        public void AdvanceCurrentLine_OnTerminalNode_EndsDialogue()
        {
            _manager.StartDialogue(BuildLinearDialogue());
            _manager.AdvanceCurrentLine(); // -> "end" (terminal, no AutoAdvance)

            _manager.AdvanceCurrentLine();

            Assert.IsFalse(_manager.IsActive);
        }

        [Test]
        public void EndDialogue_ClearsActiveDialogueAndChoices()
        {
            _manager.StartDialogue(BuildBranchingDialogue());

            _manager.EndDialogue();

            Assert.IsFalse(_manager.IsActive);
            Assert.IsNull(_manager.ActiveDialogue);
            Assert.IsNull(_manager.CurrentNode);
            Assert.AreEqual(0, _manager.AvailableChoices.Count);
        }

        [Test]
        public void EndDialogue_WhenNotActive_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _manager.EndDialogue());
        }

        [Test]
        public void AvailableChoices_FiltersOutChoiceWithUnsetConditionFlag()
        {
            _manager.StartDialogue(BuildConditionalDialogue());

            // has_key flag never set: "locked_choice" (requires it) should be filtered out,
            // "only_without_flag" (requires it to be absent) and "choice_bye" should remain.
            Assert.AreEqual(2, _manager.AvailableChoices.Count);
            CollectionAssert.DoesNotContain(
                new List<string> { _manager.AvailableChoices[0].TextKey, _manager.AvailableChoices[1].TextKey },
                "dialogue.test.locked_choice");
        }

        [Test]
        public void AvailableChoices_IncludesChoiceOnceRequiredFlagIsSet()
        {
            _manager.SetFlag("has_key", true);

            _manager.StartDialogue(BuildConditionalDialogue());

            Assert.AreEqual(2, _manager.AvailableChoices.Count);
            Assert.AreEqual("dialogue.test.locked_choice", _manager.AvailableChoices[0].TextKey);
        }

        [Test]
        public void HasFlag_ReflectsSetFlag()
        {
            Assert.IsFalse(_manager.HasFlag("quest_done"));

            _manager.SetFlag("quest_done");

            Assert.IsTrue(_manager.HasFlag("quest_done"));

            _manager.SetFlag("quest_done", false);

            Assert.IsFalse(_manager.HasFlag("quest_done"));
        }

        [Test]
        public void StartDialogue_WithMissingStartNode_DoesNotActivate()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();
            dialogue.SetDataForTests("broken", "does_not_exist", new List<DialogueNode>());

            _manager.StartDialogue(dialogue);

            Assert.IsFalse(_manager.IsActive);
        }

        [Test]
        public void SelectChoice_ToMissingNodeId_EndsDialogueInsteadOfThrowing()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();
            var nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    Id = "root",
                    TextKey = "dialogue.test.root",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { TextKey = "dialogue.test.choice_broken", NextNodeId = "does_not_exist" }
                    }
                }
            };
            dialogue.SetDataForTests("dialogue_test_broken_link", "root", nodes);

            _manager.StartDialogue(dialogue);

            Assert.DoesNotThrow(() => _manager.SelectChoice(0));
            Assert.IsFalse(_manager.IsActive);
        }

        [Test]
        public void Shutdown_EndsActiveDialogue()
        {
            _manager.StartDialogue(BuildBranchingDialogue());

            _manager.Shutdown();

            Assert.IsFalse(_manager.IsActive);
        }
    }
}
