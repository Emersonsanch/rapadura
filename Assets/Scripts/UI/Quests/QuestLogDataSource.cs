using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Quests;
using UnityEngine;

namespace Rapadura.UI.Quests
{
    /// <summary>
    /// Shared read side for <see cref="QuestLogController"/> and <see cref="QuestTrackerController"/>:
    /// resolves <see cref="QuestManager"/>/<see cref="LocalizationManager"/> from the ServiceLocator
    /// (both are plain C# managers, not Inspector-assignable) and turns the quest catalog into
    /// <see cref="QuestEntryViewModel"/>s via <see cref="QuestLogPresenter"/>.
    ///
    /// The catalog comes from <c>Resources/Quests</c> (same <c>Resources.LoadAll</c> approach as
    /// <c>ItemDatabase</c>/<c>RecipeDatabase</c>) because QuestManager only exposes per-id queries,
    /// not a list of active quests. Each loaded definition is also handed to
    /// <see cref="QuestManager.RegisterDefinition"/> — harmless for already-known quests, and it
    /// means quests restored from a save (RestoreState doesn't re-register definitions) still have
    /// their definition available when they complete and grant rewards.
    /// </summary>
    public class QuestLogDataSource
    {
        public const string ResourcesFolder = "Quests";

        private readonly List<QuestDefinition> _definitions = new List<QuestDefinition>();
        private QuestManager _questManager;
        private LocalizationManager _localization;

        public QuestManager QuestManager => _questManager;
        public LocalizationManager Localization => _localization;
        public IReadOnlyList<QuestDefinition> Definitions => _definitions;

        /// <summary>Resolves managers lazily (they may register after this UI's OnEnable) and loads the catalog once.</summary>
        public bool EnsureResolved()
        {
            if (_questManager == null)
            {
                ServiceLocator.TryGet(out _questManager);
            }

            if (_localization == null && ServiceLocator.TryGet(out _localization))
            {
                QuestLogTexts.EnsureLoaded(_localization);
            }

            if (_definitions.Count == 0)
            {
                _definitions.AddRange(Resources.LoadAll<QuestDefinition>(ResourcesFolder).Where(d => d != null && !string.IsNullOrEmpty(d.QuestId)));
            }

            if (_questManager != null)
            {
                foreach (QuestDefinition definition in _definitions)
                {
                    _questManager.RegisterDefinition(definition);
                }
            }

            return _questManager != null;
        }

        public string Localize(string key)
        {
            if (_localization != null)
            {
                return _localization.Get(key);
            }

            return QuestLogTexts.GetFallback(key);
        }

        public bool HasKey(string key)
        {
            return _localization != null ? _localization.HasKey(key) : QuestLogTexts.All.Any(e => e.Key == key);
        }

        public List<QuestEntryViewModel> BuildActive()
        {
            if (!EnsureResolved())
            {
                return new List<QuestEntryViewModel>();
            }

            return QuestLogPresenter.Sort(_definitions
                .Where(d => _questManager.IsActive(d.QuestId))
                .Select(d => QuestLogPresenter.BuildEntry(d, _questManager.GetProgress(d.QuestId), false, Localize, HasKey)));
        }

        public List<QuestEntryViewModel> BuildCompleted()
        {
            if (!EnsureResolved())
            {
                return new List<QuestEntryViewModel>();
            }

            return QuestLogPresenter.Sort(_definitions
                .Where(d => _questManager.IsCompleted(d.QuestId))
                .Select(d => QuestLogPresenter.BuildEntry(d, null, true, Localize, HasKey)));
        }

        public QuestDefinition FindDefinition(string questId)
        {
            return _definitions.FirstOrDefault(d => d.QuestId == questId);
        }
    }
}
