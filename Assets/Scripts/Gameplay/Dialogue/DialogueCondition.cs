using System;
using System.Collections.Generic;

namespace Rapadura.Gameplay.Dialogue
{
    /// <summary>What a single <see cref="DialogueCondition"/> checks.</summary>
    public enum DialogueConditionType
    {
        /// <summary>A <see cref="DialogueManager"/> flag (<see cref="DialogueCondition.Key"/> = flag name) is set — same check as the legacy <see cref="DialogueChoice.ConditionFlag"/>.</summary>
        Flag = 0,

        /// <summary>Quest <see cref="DialogueCondition.Key"/> is in <see cref="DialogueCondition.QuestState"/>.</summary>
        QuestState = 1,

        /// <summary>Objective <see cref="DialogueCondition.Value"/> of active quest <see cref="DialogueCondition.Key"/> is complete (also true once the whole quest is completed).</summary>
        QuestObjectiveComplete = 2,

        /// <summary>The hero being played is <see cref="DialogueCondition.Key"/> (a <c>CharacterId</c> name, e.g. "Maithe"; case-insensitive).</summary>
        ActiveCharacter = 3
    }

    /// <summary>
    /// Quest state as seen by dialogue. <see cref="Available"/> and <see cref="ObjectivesComplete"/>
    /// are refinements of <see cref="NotStarted"/>/<see cref="Active"/>, so a condition asking for
    /// <see cref="NotStarted"/> also matches an available quest and <see cref="Active"/> also
    /// matches one whose objectives are all done (see <see cref="DialogueCondition.MatchesQuestState"/>).
    /// </summary>
    public enum DialogueQuestState
    {
        /// <summary>Never started (prerequisites may or may not be met).</summary>
        NotStarted = 0,

        /// <summary>Not started and every prerequisite quest is completed — i.e. StartQuest would succeed.</summary>
        Available = 1,

        /// <summary>In progress.</summary>
        Active = 2,

        /// <summary>In progress with every objective done but not yet handed in (only reachable if the quest wasn't auto-completed).</summary>
        ObjectivesComplete = 3,

        /// <summary>Completed (rewards granted).</summary>
        Completed = 4
    }

    /// <summary>
    /// One data-driven gate for a <see cref="DialogueChoice"/> or <see cref="DialogueRedirect"/>.
    /// Lists of conditions are AND-ed; <see cref="Negate"/> flips a single condition. Evaluated by
    /// <see cref="DialogueManager"/> through its <see cref="IDialogueContext"/>, so this plain data
    /// class never touches the quest/character systems itself.
    /// </summary>
    [Serializable]
    public class DialogueCondition
    {
        public DialogueConditionType Type;

        /// <summary>Flag name / quest id / CharacterId name, depending on <see cref="Type"/>.</summary>
        public string Key;

        /// <summary>Secondary operand: the objective id for <see cref="DialogueConditionType.QuestObjectiveComplete"/>. Unused otherwise.</summary>
        public string Value;

        /// <summary>Required state for <see cref="DialogueConditionType.QuestState"/>. Unused otherwise.</summary>
        public DialogueQuestState QuestState;

        public bool Negate;

        /// <summary>
        /// True if <paramref name="actual"/> satisfies a requirement of <paramref name="required"/>:
        /// NotStarted accepts Available, Active accepts ObjectivesComplete; everything else is exact.
        /// </summary>
        public static bool MatchesQuestState(DialogueQuestState required, DialogueQuestState actual)
        {
            switch (required)
            {
                case DialogueQuestState.NotStarted:
                    return actual == DialogueQuestState.NotStarted || actual == DialogueQuestState.Available;
                case DialogueQuestState.Active:
                    return actual == DialogueQuestState.Active || actual == DialogueQuestState.ObjectivesComplete;
                default:
                    return actual == required;
            }
        }
    }

    /// <summary>
    /// A conditional jump evaluated when a node is entered: the first redirect whose
    /// <see cref="Conditions"/> all pass sends the conversation straight to <see cref="TargetNodeId"/>
    /// without showing the node's own line. Lets one dialogue asset act as a quest-giver "hub"
    /// (offer → in-progress → turn-in → epilogue) and pick per-hero lines by
    /// <see cref="DialogueConditionType.ActiveCharacter"/>. An empty condition list always passes.
    /// </summary>
    [Serializable]
    public class DialogueRedirect
    {
        public List<DialogueCondition> Conditions = new List<DialogueCondition>();
        public string TargetNodeId;
    }
}
