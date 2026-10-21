using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Rapadura.Gameplay.Quests;

namespace Rapadura.UI.Quests
{
    /// <summary>Display-ready state of one objective row (text already localized, progress already formatted).</summary>
    public class QuestObjectiveViewModel
    {
        public string ObjectiveId;
        public string Text;
        public string ProgressText;
        public float Fraction;
        public bool IsComplete;
    }

    /// <summary>Display-ready state of one quest, consumed by <see cref="QuestLogController"/> and <see cref="QuestTrackerController"/>.</summary>
    public class QuestEntryViewModel
    {
        public string QuestId;
        public QuestType QuestType;
        public string Title;
        public string Description;
        public string Journal;
        public string TypeLabel;
        public string RewardText;
        public bool IsCompleted;
        public List<QuestObjectiveViewModel> Objectives = new List<QuestObjectiveViewModel>();

        public int CompletedObjectiveCount => Objectives.Count(o => o.IsComplete);

        /// <summary>First objective not yet done — what the compact tracker highlights. Null once every objective is complete.</summary>
        public QuestObjectiveViewModel NextObjective => Objectives.FirstOrDefault(o => !o.IsComplete);
    }

    /// <summary>
    /// Pure view-model/formatting rules behind the Quest Log and Quest Tracker, kept free of
    /// <c>UIDocument</c>/<c>VisualElement</c> so they can be exercised in EditMode tests (same split
    /// as <c>SaveSlotPresenter</c>/<c>HudBarMath</c>). Text lookup is injected as a
    /// <c>Func&lt;string, string&gt;</c> so tests don't need a live <c>LocalizationManager</c>.
    /// </summary>
    public static class QuestLogPresenter
    {
        public const string TypeMainKey = "ui.quest.type.main";
        public const string TypeSideKey = "ui.quest.type.side";
        public const string TypeDailyKey = "ui.quest.type.daily";
        public const string RewardXpKey = "ui.quest.reward_xp";

        /// <summary>
        /// Matches a "(0/6)"-style counter baked into the end of an objective's localized text.
        /// Several seeded <c>quest.*.obj_*</c> strings were authored with a static "(0/N)" suffix,
        /// which would contradict the live progress the UI shows next to it ("3/6").
        /// </summary>
        private static readonly Regex BakedProgressSuffix = new Regex(@"\s*\(\s*\d+\s*/\s*\d+\s*\)\s*$", RegexOptions.Compiled);

        /// <summary>"3/5" — current is clamped to [0, required], required to at least 1 (same floor as <see cref="QuestObjective.IsComplete"/>).</summary>
        public static string FormatProgress(int current, int required)
        {
            int safeRequired = Math.Max(1, required);
            int safeCurrent = Math.Max(0, Math.Min(current, safeRequired));
            return $"{safeCurrent}/{safeRequired}";
        }

        /// <summary>0..1 fill for a progress bar, using the same clamping as <see cref="FormatProgress"/>.</summary>
        public static float GetFraction(int current, int required)
        {
            int safeRequired = Math.Max(1, required);
            int safeCurrent = Math.Max(0, Math.Min(current, safeRequired));
            return (float)safeCurrent / safeRequired;
        }

        /// <summary>Removes a trailing static "(0/N)" counter from authored objective text (see <see cref="BakedProgressSuffix"/>).</summary>
        public static string StripBakedProgress(string text)
        {
            return string.IsNullOrEmpty(text) ? string.Empty : BakedProgressSuffix.Replace(text, string.Empty);
        }

        /// <summary>
        /// Journal (flavor) key derived from the quest's title key: "quest.maithe_windtrail.title" →
        /// "quest.maithe_windtrail.journal". Keeps journal entries next to the quest's other keys
        /// without adding a new field to <see cref="QuestDefinition"/>.
        /// </summary>
        public static string GetJournalKey(string titleKey)
        {
            const string TitleSuffix = ".title";

            if (string.IsNullOrEmpty(titleKey) || !titleKey.EndsWith(TitleSuffix, StringComparison.Ordinal))
            {
                return null;
            }

            return titleKey.Substring(0, titleKey.Length - TitleSuffix.Length) + ".journal";
        }

        public static string GetTypeLabelKey(QuestType type)
        {
            switch (type)
            {
                case QuestType.MainStory: return TypeMainKey;
                case QuestType.Daily: return TypeDailyKey;
                default: return TypeSideKey;
            }
        }

        /// <summary>"450 XP · Arco de Caça x1" — XP first, then each item; zero/negative entries are skipped. Empty when there's nothing to grant.</summary>
        public static string FormatRewards(int experience, IEnumerable<(string name, int quantity)> items, Func<string, string> localize)
        {
            var parts = new List<string>();

            if (experience > 0)
            {
                string xpFormat = localize != null ? localize(RewardXpKey) : null;
                parts.Add(SafeFormat(xpFormat, "{0} XP", experience));
            }

            if (items != null)
            {
                foreach ((string name, int quantity) in items)
                {
                    if (string.IsNullOrEmpty(name) || quantity <= 0)
                    {
                        continue;
                    }

                    parts.Add(quantity > 1 ? $"{name} x{quantity}" : name);
                }
            }

            return string.Join(" · ", parts);
        }

        /// <summary>
        /// Builds one quest's view-model. <paramref name="progress"/> is the live runtime clone from
        /// <see cref="QuestManager.GetProgress"/> (null for completed/not-started quests). Completed
        /// quests show every objective as done, since QuestManager drops their runtime progress.
        /// </summary>
        public static QuestEntryViewModel BuildEntry(
            QuestDefinition definition,
            QuestProgress progress,
            bool isCompleted,
            Func<string, string> localize,
            Func<string, bool> hasKey = null)
        {
            if (definition == null)
            {
                return null;
            }

            Func<string, string> text = localize ?? (key => key);

            var entry = new QuestEntryViewModel
            {
                QuestId = definition.QuestId,
                QuestType = definition.QuestType,
                Title = text(definition.TitleKey),
                Description = text(definition.DescriptionKey),
                TypeLabel = text(GetTypeLabelKey(definition.QuestType)),
                IsCompleted = isCompleted,
                RewardText = FormatRewards(
                    definition.RewardExperience,
                    definition.RewardItems
                        .Where(r => r != null && r.Item != null)
                        .Select(r => (r.Item.DisplayName, r.Quantity)),
                    text)
            };

            string journalKey = GetJournalKey(definition.TitleKey);
            if (journalKey != null && (hasKey == null || hasKey(journalKey)))
            {
                entry.Journal = text(journalKey);
            }

            foreach (QuestObjective sourceObjective in definition.Objectives)
            {
                if (sourceObjective == null)
                {
                    continue;
                }

                QuestObjective live = progress?.Objectives.FirstOrDefault(o => o.ObjectiveId == sourceObjective.ObjectiveId);
                int required = Math.Max(1, sourceObjective.RequiredAmount);
                int current = isCompleted ? required : live?.CurrentAmount ?? 0;

                entry.Objectives.Add(new QuestObjectiveViewModel
                {
                    ObjectiveId = sourceObjective.ObjectiveId,
                    Text = StripBakedProgress(text(sourceObjective.DescriptionKey)),
                    ProgressText = FormatProgress(current, required),
                    Fraction = GetFraction(current, required),
                    IsComplete = current >= required
                });
            }

            return entry;
        }

        /// <summary>Main story first, then side, then daily; ties broken by title so the list order is stable between refreshes.</summary>
        public static List<QuestEntryViewModel> Sort(IEnumerable<QuestEntryViewModel> entries)
        {
            return entries
                .Where(e => e != null)
                .OrderBy(e => (int)e.QuestType)
                .ThenBy(e => e.Title, StringComparer.CurrentCulture)
                .ToList();
        }

        /// <summary>
        /// Which quest the compact tracker should follow: the player's explicit choice while it is
        /// still active, otherwise the first active quest in display order (so the tracker is never
        /// blank while there's something to do), otherwise null (tracker hides).
        /// </summary>
        public static string ResolveTrackedQuestId(string preferredQuestId, IReadOnlyList<QuestEntryViewModel> activeEntries)
        {
            if (activeEntries == null || activeEntries.Count == 0)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(preferredQuestId) && activeEntries.Any(e => e.QuestId == preferredQuestId))
            {
                return preferredQuestId;
            }

            return activeEntries[0].QuestId;
        }

        private static string SafeFormat(string format, string fallbackFormat, object arg)
        {
            // A missing key comes back as "[key]" from LocalizationManager — no "{0}" in it, so fall
            // back rather than showing the raw key. Malformed translator formats also fall back.
            if (string.IsNullOrEmpty(format) || !format.Contains("{0}"))
            {
                return string.Format(fallbackFormat, arg);
            }

            try
            {
                return string.Format(format, arg);
            }
            catch (FormatException)
            {
                return string.Format(fallbackFormat, arg);
            }
        }
    }
}
