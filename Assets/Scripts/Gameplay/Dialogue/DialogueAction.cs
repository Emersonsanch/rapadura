using System;

namespace Rapadura.Gameplay.Dialogue
{
    /// <summary>What a single <see cref="DialogueAction"/> does.</summary>
    public enum DialogueActionType
    {
        /// <summary>Sets <see cref="DialogueManager"/> flag <see cref="DialogueAction.Key"/>.</summary>
        SetFlag = 0,

        /// <summary>Clears <see cref="DialogueManager"/> flag <see cref="DialogueAction.Key"/>.</summary>
        ClearFlag = 1,

        /// <summary>Starts quest <see cref="DialogueAction.Key"/> (QuestManager.StartQuest; prerequisites still apply).</summary>
        StartQuest = 2,

        /// <summary>Hands in quest <see cref="DialogueAction.Key"/> (QuestManager.CompleteQuest; no-op unless every objective is done).</summary>
        CompleteQuest = 3,

        /// <summary>Raises quest custom flag <see cref="DialogueAction.Key"/> (QuestManager.SetCustomFlag), progressing any CustomFlag objective that targets it.</summary>
        SetQuestFlag = 4
    }

    /// <summary>
    /// One data-driven side effect run by <see cref="DialogueManager"/> when a
    /// <see cref="DialogueChoice"/> is picked (<see cref="DialogueChoice.Actions"/>) or a node is
    /// shown (<see cref="DialogueNode.OnEnterActions"/>). Quest actions go through the manager's
    /// <see cref="IDialogueContext"/>; the quest system then publishes its usual
    /// QuestStarted/ObjectiveUpdated/Completed events, so nothing new is added to the EventBus.
    /// </summary>
    [Serializable]
    public class DialogueAction
    {
        public DialogueActionType Type;

        /// <summary>Flag name or quest id, depending on <see cref="Type"/>.</summary>
        public string Key;
    }
}
