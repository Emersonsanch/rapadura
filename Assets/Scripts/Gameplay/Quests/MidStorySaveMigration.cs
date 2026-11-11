using System;
using System.Collections.Generic;
using System.Linq;

namespace Rapadura.Gameplay.Quests
{
    /// <summary>
    /// Saves written before "A Lua que Não Enche" (<see cref="MidStoryCatalog"/>) existed, or before the first letter required it (Docs/HistoriaMeio_20a100.md, "Saves antigos").
    /// Pure, applied by <see cref="QuestManager.RestoreState"/> to a <see cref="QuestSaveState"/> whose <see cref="QuestSaveState.StorySchema"/> is below <see cref="Schema"/>:
    /// a hero who already took or finished any letter (<see cref="LateStoryCatalog.Steps"/>) gets every step of the moon marked done, straight into the completed list —
    /// no rewards (no XP, gold or items: nothing goes to <see cref="QuestSaveState.PendingRewardQuestIds"/>), no events — so the letters never "go back" to the moon.
    /// A hero who never took a letter does the moon as new content; Vó Edna offers its first step from its level (<see cref="NextMainStep"/>, <see cref="LateStoryCatalog.Decide"/>).
    /// </summary>
    public static class MidStorySaveMigration
    {
        /// <summary>Quest save schema with the mid story in place. Old saves read 0 (the field is absent); <see cref="QuestManager.CaptureState"/> writes this.</summary>
        public const int Schema = 1;

        /// <summary>True when any letter is running or done.</summary>
        public static bool HasTouchedLetters(Func<string, bool> isActive, Func<string, bool> isCompleted)
        {
            if (isActive == null || isCompleted == null)
            {
                return false;
            }

            return LateStoryCatalog.Steps.Any(s => isActive(s.QuestId) || isCompleted(s.QuestId));
        }

        /// <summary>The moon steps to mark done for this quest state: all not yet completed when a letter was touched, else none.</summary>
        public static IReadOnlyList<string> MoonStepsToMarkDone(Func<string, bool> isActive, Func<string, bool> isCompleted)
        {
            if (!HasTouchedLetters(isActive, isCompleted))
            {
                return Array.Empty<string>();
            }

            return MidStoryCatalog.Steps.Select(s => s.QuestId).Where(id => !isCompleted(id)).ToList();
        }

        /// <summary>
        /// Brings <paramref name="state"/> to <see cref="Schema"/> in place: the moon steps of <see cref="MoonStepsToMarkDone"/> go to the completed list (and leave the active one),
        /// pending rewards stay as they were. Returns how many steps were marked. A state already at the schema is left alone.
        /// </summary>
        public static int Upgrade(QuestSaveState state)
        {
            if (state == null || state.StorySchema >= Schema)
            {
                return 0;
            }

            state.ActiveQuests ??= new List<QuestProgress>();
            state.CompletedQuestIds ??= new List<string>();

            var active = new HashSet<string>(state.ActiveQuests.Where(p => p != null && !string.IsNullOrEmpty(p.QuestId)).Select(p => p.QuestId));
            var completed = new HashSet<string>(state.CompletedQuestIds.Where(id => !string.IsNullOrEmpty(id)));

            IReadOnlyList<string> marked = MoonStepsToMarkDone(active.Contains, completed.Contains);
            if (marked.Count > 0)
            {
                var markedSet = new HashSet<string>(marked);
                state.ActiveQuests.RemoveAll(p => p != null && markedSet.Contains(p.QuestId));
                state.CompletedQuestIds.AddRange(marked);
            }

            state.StorySchema = Schema;
            return marked.Count;
        }

        /// <summary>The first step of the whole main story (<see cref="LateStoryCatalog.WholeStory"/>) not yet done: the one running, or the one its giver offers next.</summary>
        public static LateStoryStep NextMainStep(Func<string, bool> isCompleted)
        {
            return isCompleted == null ? null : LateStoryCatalog.WholeStory.FirstOrDefault(s => !isCompleted(s.QuestId));
        }
    }
}
