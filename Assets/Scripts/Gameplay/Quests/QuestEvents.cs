using Rapadura.Core.Events;

namespace Rapadura.Gameplay.Quests
{
    /// <summary>Raised when a quest transitions from not-started to active (StartQuest succeeded).</summary>
    public readonly struct QuestStartedEvent : IGameEvent
    {
        public readonly string QuestId;

        public QuestStartedEvent(string questId)
        {
            QuestId = questId;
        }
    }

    /// <summary>Raised whenever a single objective's progress changes (increment or explicit set), including when it completes.</summary>
    public readonly struct QuestObjectiveUpdatedEvent : IGameEvent
    {
        public readonly string QuestId;
        public readonly string ObjectiveId;
        public readonly int CurrentAmount;
        public readonly int RequiredAmount;
        public readonly bool ObjectiveCompleted;

        public QuestObjectiveUpdatedEvent(string questId, string objectiveId, int currentAmount, int requiredAmount, bool objectiveCompleted)
        {
            QuestId = questId;
            ObjectiveId = objectiveId;
            CurrentAmount = currentAmount;
            RequiredAmount = requiredAmount;
            ObjectiveCompleted = objectiveCompleted;
        }
    }

    /// <summary>Raised once, when every objective of a quest is complete and rewards have been granted.</summary>
    public readonly struct QuestCompletedEvent : IGameEvent
    {
        public readonly string QuestId;

        public QuestCompletedEvent(string questId)
        {
            QuestId = questId;
        }
    }

    /// <summary>Raised when an in-progress quest is abandoned before completion.</summary>
    public readonly struct QuestAbandonedEvent : IGameEvent
    {
        public readonly string QuestId;

        public QuestAbandonedEvent(string questId)
        {
            QuestId = questId;
        }
    }
}
