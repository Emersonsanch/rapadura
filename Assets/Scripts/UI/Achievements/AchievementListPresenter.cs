using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Achievements;
using UnityEngine;

namespace Rapadura.UI.Achievements
{
    /// <summary>Display-ready snapshot of one achievement row (pure data, no UI Toolkit types).</summary>
    public class AchievementEntryViewModel
    {
        public string AchievementId;
        public string Title;
        public string Description;
        public Sprite Icon;
        public bool IsUnlocked;

        /// <summary>True when the achievement is hidden and still locked — title/description are masked.</summary>
        public bool IsMasked;

        public int Progress;
        public int Target;
        public float Fraction;
        public string ProgressText;
    }

    /// <summary>
    /// Pure logic behind <see cref="AchievementListController"/>/<see cref="AchievementToastController"/>:
    /// turns <see cref="AchievementManager"/> state into <see cref="AchievementEntryViewModel"/>s, masking
    /// hidden-and-locked achievements. Kept separate from the MonoBehaviours so it is EditMode-testable,
    /// same split as QuestLogPresenter.
    /// </summary>
    public static class AchievementListPresenter
    {
        public static AchievementEntryViewModel Build(AchievementDefinition definition, AchievementManager manager, Func<string, string> localize)
        {
            bool unlocked = manager != null && manager.IsUnlocked(definition.AchievementId);
            bool masked = definition.Hidden && !unlocked;
            int target = definition.TargetValue;
            int progress = manager != null ? manager.GetProgress(definition) : 0;

            return new AchievementEntryViewModel
            {
                AchievementId = definition.AchievementId,
                Title = localize(masked ? AchievementTexts.HiddenTitleKey : definition.TitleKey),
                Description = localize(masked ? AchievementTexts.HiddenDescriptionKey : definition.DescriptionKey),
                Icon = masked ? null : definition.Icon,
                IsUnlocked = unlocked,
                IsMasked = masked,
                Progress = progress,
                Target = target,
                Fraction = target > 0 ? Mathf.Clamp01(progress / (float)target) : 0f,
                ProgressText = unlocked ? localize(AchievementTexts.UnlockedBadgeKey) : FormatProgress(progress, target)
            };
        }

        /// <summary>All definitions in catalog order (sort order, then id).</summary>
        public static List<AchievementEntryViewModel> BuildAll(AchievementManager manager, Func<string, string> localize)
        {
            var result = new List<AchievementEntryViewModel>();

            if (manager == null)
            {
                return result;
            }

            foreach (AchievementDefinition definition in manager.Definitions)
            {
                result.Add(Build(definition, manager, localize));
            }

            return result;
        }

        public static string FormatProgress(int progress, int target) => $"{progress}/{target}";

        /// <summary>"3/20 desbloqueadas" header text.</summary>
        public static string FormatSummary(AchievementManager manager, Func<string, string> localize)
        {
            int unlocked = manager != null ? manager.UnlockedCount : 0;
            int total = manager != null ? manager.TotalCount : 0;
            string format = localize(AchievementTexts.SummaryKey);

            try
            {
                return string.Format(format, unlocked, total);
            }
            catch (FormatException)
            {
                return $"{unlocked}/{total}";
            }
        }
    }
}
