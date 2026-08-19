namespace Rapadura.Gameplay.Quests
{
    /// <summary>
    /// The kind of condition a single <see cref="QuestObjective"/> tracks. Mirrors the same
    /// "small closed enum + data-driven definition" shape used by <c>SkillType</c>/<c>ItemType</c>
    /// elsewhere in this project — new objective kinds are added here, and
    /// <see cref="QuestManager"/> gets one new event handler per kind, never per-quest special-casing.
    /// </summary>
    public enum QuestObjectiveType
    {
        /// <summary>Kill N enemies whose <c>EnemyDefinition</c>/target id matches <see cref="QuestObjective.TargetId"/>.</summary>
        KillEnemyType = 0,

        /// <summary>Collect/hold N of an item (<see cref="QuestObjective.TargetId"/> = <c>ItemDefinition.ItemId</c>).</summary>
        CollectItem = 1,

        /// <summary>
        /// Reach a location/area identified by <see cref="QuestObjective.TargetId"/>. Placeholder:
        /// there is no world trigger/zone system yet (depends on Scenes, out of scope for this
        /// change) — <see cref="QuestManager"/> exposes <see cref="QuestManager.ReportLocationReached"/>
        /// so a future <c>QuestLocationTrigger</c> MonoBehaviour in a Scene can call it directly.
        /// </summary>
        ReachLocation = 2,

        /// <summary>Talk to an NPC identified by <see cref="QuestObjective.TargetId"/> (matched against <c>DialogueDefinition.DialogueId</c> or a speaker id).</summary>
        TalkToNpc = 3,

        /// <summary>
        /// Generic named flag, satisfied by an explicit call to <see cref="QuestManager.SetCustomFlag"/>.
        /// Escape hatch for objectives that don't fit the other types (e.g. "survive the night",
        /// "escort the NPC") without inventing a new enum value/event for every one-off quest.
        /// </summary>
        CustomFlag = 4
    }
}
