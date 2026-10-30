namespace Rapadura.Gameplay.Dialogue
{
    /// <summary>
    /// Everything outside the dialogue graph that <see cref="DialogueCondition"/>s read and
    /// <see cref="DialogueAction"/>s change: quest state and the hero being played. Kept behind an
    /// interface so <see cref="DialogueManager"/> stays testable without a GameManager/ServiceLocator
    /// (tests inject a fake or a <see cref="QuestDialogueContext"/> built around their own
    /// QuestManager). The runtime default is <see cref="QuestDialogueContext"/>.
    /// </summary>
    public interface IDialogueContext
    {
        DialogueQuestState GetQuestState(string questId);

        bool IsQuestObjectiveComplete(string questId, string objectiveId);

        bool StartQuest(string questId);

        bool CompleteQuest(string questId);

        void SetQuestFlag(string flagName);

        /// <summary>The selected hero's <c>CharacterId</c> name (e.g. "Maithe"), or null if none was chosen yet.</summary>
        string ActiveCharacterId { get; }
    }

    /// <summary>
    /// Optional extension of <see cref="IDialogueContext"/> for <see cref="DialogueConditionType.StoryBeatSeen"/>
    /// (kept apart so existing contexts and test fakes don't have to implement it; without it the condition is false).
    /// </summary>
    public interface IStoryDialogueContext
    {
        /// <summary>Whether <paramref name="beat"/> was seen in the current journey.</summary>
        bool HasSeenStoryBeat(Rapadura.Gameplay.Story.StoryBeat beat);
    }
}
