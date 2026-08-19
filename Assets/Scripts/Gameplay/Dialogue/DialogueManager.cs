using System.Collections.Generic;
using Rapadura.Core.Audio;
using Rapadura.Core.DI;
using Rapadura.Core.EventBus;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Logging;

namespace Rapadura.Gameplay.Dialogue
{
    /// <summary>
    /// Top-level dialogue system: plain C# class implementing <see cref="IManager"/>, same shape as
    /// every other manager in this project (SaveManager, LocalizationManager, CraftingManager...).
    /// Walks a <see cref="DialogueDefinition"/> node-by-node, exposes the current line/choices for
    /// UI to render, and publishes <see cref="DialogueStartedEvent"/>/<see cref="DialogueLineChangedEvent"/>/
    /// <see cref="DialogueEndedEvent"/> on the shared <see cref="EventBus"/> so any system (quests,
    /// analytics, UI) can react without a direct reference.
    ///
    /// NOTE: this class is intentionally NOT wired into GameManager.cs by this change (per task
    /// instructions, GameManager.cs is not to be edited here) — a follow-up edit needs to add,
    /// inside <c>GameManager.BuildManagers()</c>:
    ///   DialogueManager = new DialogueManager();
    ///   RegisterManager(DialogueManager);
    /// plus a public <c>DialogueManager</c> property, mirroring LocalizationManager/CraftingManager.
    ///
    /// Ducking: on <see cref="StartDialogue"/>/<see cref="EndDialogue"/> this looks up
    /// <see cref="AudioManager"/> via <see cref="ServiceLocator.TryGet{TService}"/> (never a hard
    /// reference) and calls its existing public <c>DuckMusic(amount, duration)</c> API — that API
    /// already exists in AudioManager.cs and is not modified here.
    /// </summary>
    public class DialogueManager : IManager
    {
        private const string LogCategory = "Dialogue";

        /// <summary>Fraction to cut music volume by while a dialogue is on screen.</summary>
        private const float DialogueDuckAmount = 0.6f;

        /// <summary>Duration passed to DuckMusic when a dialogue starts; refreshed on every line change
        /// so the duck holds for as long as the dialogue is open (see <see cref="RefreshDuck"/>).</summary>
        private const float DialogueDuckHoldDuration = 3f;

        private readonly Dictionary<string, bool> _flags = new Dictionary<string, bool>();

        public DialogueDefinition ActiveDialogue { get; private set; }
        public DialogueNode CurrentNode { get; private set; }
        public bool IsActive => ActiveDialogue != null && CurrentNode != null;

        /// <summary>Choices currently offered, already filtered by <see cref="DialogueChoice.ConditionFlag"/>.</summary>
        public IReadOnlyList<DialogueChoice> AvailableChoices { get; private set; } = new List<DialogueChoice>();

        public void Initialize()
        {
            GameLogger.Info(LogCategory, "DialogueManager initialized.");
        }

        public void Shutdown()
        {
            if (IsActive)
            {
                EndDialogue();
            }

            _flags.Clear();
        }

        // ------------------------------------------------------------------
        // Flags (minimal condition support for DialogueChoice.ConditionFlag).
        // ------------------------------------------------------------------

        public void SetFlag(string key, bool value = true)
        {
            if (!string.IsNullOrEmpty(key))
            {
                _flags[key] = value;
            }
        }

        public bool HasFlag(string key)
        {
            return !string.IsNullOrEmpty(key) && _flags.TryGetValue(key, out bool value) && value;
        }

        // ------------------------------------------------------------------
        // Navigation.
        // ------------------------------------------------------------------

        /// <summary>Starts a dialogue tree from its configured start node. No-ops (with a warning) if one is already active.</summary>
        public void StartDialogue(DialogueDefinition dialogue)
        {
            if (dialogue == null)
            {
                GameLogger.Warning(LogCategory, "StartDialogue called with a null DialogueDefinition.");
                return;
            }

            if (IsActive)
            {
                GameLogger.Warning(LogCategory, $"StartDialogue('{dialogue.DialogueId}') ignored, dialogue '{ActiveDialogue.DialogueId}' is already active.");
                return;
            }

            DialogueNode startNode = dialogue.GetStartNode();

            if (startNode == null)
            {
                GameLogger.Warning(LogCategory, $"Dialogue '{dialogue.DialogueId}' has no valid start node ('{dialogue.StartNodeId}').");
                return;
            }

            ActiveDialogue = dialogue;
            SetCurrentNode(startNode);

            DuckMusicForDialogue();

            EventBus.Publish(new DialogueStartedEvent(dialogue.DialogueId, startNode.Id));
            EventBus.Publish(new DialogueLineChangedEvent(dialogue.DialogueId, startNode.Id, startNode.SpeakerId));
        }

        /// <summary>Selects the choice at <paramref name="index"/> from <see cref="AvailableChoices"/> and advances to its target node (or ends the dialogue if it has none).</summary>
        public void SelectChoice(int index)
        {
            if (!IsActive)
            {
                GameLogger.Warning(LogCategory, "SelectChoice called with no active dialogue.");
                return;
            }

            if (index < 0 || index >= AvailableChoices.Count)
            {
                GameLogger.Warning(LogCategory, $"SelectChoice({index}) out of range ({AvailableChoices.Count} available).");
                return;
            }

            DialogueChoice choice = AvailableChoices[index];
            AdvanceTo(choice.NextNodeId);
        }

        /// <summary>Advances a terminal-but-linear node (no choices, <see cref="DialogueNode.AutoAdvanceToNodeId"/> set) to its next node.</summary>
        public void AdvanceCurrentLine()
        {
            if (!IsActive)
            {
                return;
            }

            if (CurrentNode.Choices.Count > 0)
            {
                GameLogger.Warning(LogCategory, "AdvanceCurrentLine called on a node with choices; use SelectChoice instead.");
                return;
            }

            AdvanceTo(CurrentNode.AutoAdvanceToNodeId);
        }

        private void AdvanceTo(string nextNodeId)
        {
            if (string.IsNullOrEmpty(nextNodeId))
            {
                EndDialogue();
                return;
            }

            DialogueNode nextNode = ActiveDialogue.GetNode(nextNodeId);

            if (nextNode == null)
            {
                GameLogger.Warning(LogCategory, $"Dialogue '{ActiveDialogue.DialogueId}' references missing node '{nextNodeId}'; ending dialogue.");
                EndDialogue();
                return;
            }

            SetCurrentNode(nextNode);
            RefreshDuck();

            EventBus.Publish(new DialogueLineChangedEvent(ActiveDialogue.DialogueId, nextNode.Id, nextNode.SpeakerId));
        }

        /// <summary>Force-closes the active dialogue (e.g. player walks away, or the last node was reached).</summary>
        public void EndDialogue()
        {
            if (!IsActive)
            {
                return;
            }

            string dialogueId = ActiveDialogue.DialogueId;

            ActiveDialogue = null;
            CurrentNode = null;
            AvailableChoices = System.Array.Empty<DialogueChoice>();

            RestoreMusicAfterDialogue();

            EventBus.Publish(new DialogueEndedEvent(dialogueId));
        }

        private void SetCurrentNode(DialogueNode node)
        {
            CurrentNode = node;
            AvailableChoices = FilterAvailableChoices(node.Choices);
        }

        private List<DialogueChoice> FilterAvailableChoices(List<DialogueChoice> choices)
        {
            var result = new List<DialogueChoice>(choices.Count);

            foreach (DialogueChoice choice in choices)
            {
                if (IsChoiceAvailable(choice))
                {
                    result.Add(choice);
                }
            }

            return result;
        }

        private bool IsChoiceAvailable(DialogueChoice choice)
        {
            if (string.IsNullOrEmpty(choice.ConditionFlag))
            {
                return true;
            }

            bool flagIsSet = HasFlag(choice.ConditionFlag);
            return choice.NegateCondition ? !flagIsSet : flagIsSet;
        }

        // ------------------------------------------------------------------
        // Audio ducking (consumes AudioManager's existing public API only).
        // ------------------------------------------------------------------

        private void DuckMusicForDialogue()
        {
            if (ServiceLocator.TryGet(out AudioManager audioManager))
            {
                audioManager.DuckMusic(DialogueDuckAmount, DialogueDuckHoldDuration);
            }
        }

        /// <summary>Re-applies the duck on every line change so it doesn't fade back to full volume mid-conversation
        /// (DuckMusic's duration is a countdown, not "until told otherwise").</summary>
        private void RefreshDuck()
        {
            DuckMusicForDialogue();
        }

        private void RestoreMusicAfterDialogue()
        {
            if (ServiceLocator.TryGet(out AudioManager audioManager))
            {
                // Duck amount 0 = no cut, short duration = quick fade back to full volume.
                audioManager.DuckMusic(0f, 0.5f);
            }
        }
    }
}
