using System;

namespace Rapadura.Gameplay.Quests
{
    /// <summary>
    /// Plain data describing one objective inside a <see cref="QuestDefinition"/>: what kind of
    /// condition it is, which target it tracks, and how much progress is required. Authored inside
    /// the ScriptableObject as a serializable class (same "plain data class referenced from a
    /// ScriptableObject list" pattern as <c>RecipeIngredient</c>/<c>DialogueChoice</c>), and also
    /// used at runtime by <see cref="QuestManager"/> to track live progress per active quest
    /// (cloned so different players/save slots don't share mutable state with the asset).
    /// </summary>
    [Serializable]
    public class QuestObjective
    {
        public string ObjectiveId;
        public QuestObjectiveType Type;

        /// <summary>Enemy id / item id / location id / npc id / custom flag name, depending on <see cref="Type"/>.</summary>
        public string TargetId;

        /// <summary>How many units of progress complete this objective (e.g. kill 5 wolves). Minimum 1.</summary>
        public int RequiredAmount = 1;

        /// <summary>Localization key for the objective's display text (e.g. "quest.obj.kill_wolves").</summary>
        public string DescriptionKey;

        /// <summary>Current progress, 0..RequiredAmount. Only meaningful on a runtime clone tracked by QuestManager, not on the source asset.</summary>
        public int CurrentAmount;

        public bool IsComplete => CurrentAmount >= Math.Max(1, RequiredAmount);

        /// <summary>Returns a runtime copy with progress reset to zero, safe to mutate independently of the source asset/definition.</summary>
        public QuestObjective Clone()
        {
            return new QuestObjective
            {
                ObjectiveId = ObjectiveId,
                Type = Type,
                TargetId = TargetId,
                RequiredAmount = Math.Max(1, RequiredAmount),
                DescriptionKey = DescriptionKey,
                CurrentAmount = 0
            };
        }
    }
}
