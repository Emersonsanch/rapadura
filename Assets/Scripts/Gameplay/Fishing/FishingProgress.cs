using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Cooking;

namespace Rapadura.Gameplay.Fishing
{
    /// <summary>One species in <see cref="FishingSaveState"/>: how many were landed and the biggest one.</summary>
    [Serializable]
    public class FishRecordEntry
    {
        public string fishId;
        public int caught;
        public int bestGrams;
    }

    /// <summary>JSON-serializable snapshot of <see cref="FishingProgress"/>.</summary>
    [Serializable]
    public class FishingSaveState
    {
        public List<FishRecordEntry> entries = new List<FishRecordEntry>();

        /// <summary>Every species was landed in this save (<see cref="FishCollectionCompletedEvent"/> already raised).</summary>
        public bool completed;
    }

    /// <summary>What <see cref="FishingProgress.RecordCatch"/> changed.</summary>
    public readonly struct FishCatchOutcome
    {
        public readonly string FishId;
        public readonly int Grams;
        public readonly bool NewSpecies;
        public readonly bool NewRecord;
        /// <summary>The record before this catch (0 for a new species).</summary>
        public readonly int PreviousBestGrams;

        public FishCatchOutcome(string fishId, int grams, bool newSpecies, bool newRecord, int previousBestGrams)
        {
            FishId = fishId;
            Grams = grams;
            NewSpecies = newSpecies;
            NewRecord = newRecord;
            PreviousBestGrams = previousBestGrams;
        }
    }

    /// <summary>Raised for every fish landed (after the collection and the record were updated).</summary>
    public readonly struct FishCaughtEvent : IGameEvent
    {
        public readonly FishCatchOutcome Outcome;

        public FishCaughtEvent(FishCatchOutcome outcome)
        {
            Outcome = outcome;
        }
    }

    /// <summary>Raised once per save, when the last species of <see cref="FishCatalog"/> is landed.</summary>
    public readonly struct FishCollectionCompletedEvent : IGameEvent
    {
    }

    /// <summary>
    /// "Caderno de pesca": which species were landed in this save, how many and the biggest of each ("maior peixe").
    /// Plain C# (EditMode-testable), built by GameManager, registered with SaveManager under <see cref="Key"/> and with
    /// NewSessionReset like every per-save system; a save from before fishing has no entry and loads with an empty
    /// notebook.
    /// <list type="bullet">
    /// <item>Every new species is fed to <see cref="AchievementManager"/> (<see cref="AchievementConditionType.FishSpeciesCaught"/>,
    /// distinct ids: "Mestre das Águas" and "O Peixe-Dourado"); achievements are per profile, so after each load the
    /// save's species are fed again (distinct, no double counting).</item>
    /// <item>The first catch of a species teaches the dishes it is the key ingredient of
    /// (<see cref="MealCatalog.TaughtByCatch"/>, <see cref="MealLearnSource.Fishing"/>); re-checked after each load.</item>
    /// </list>
    /// </summary>
    public class FishingProgress : IManager, ISaveable, IResettable
    {
        public const string Key = "Fishing";

        private readonly Dictionary<string, FishRecordEntry> _records = new Dictionary<string, FishRecordEntry>(StringComparer.Ordinal);
        private bool _subscribed;

        public string SaveKey => Key;

        /// <summary>Explicit achievement sink (tests); null = the registered one.</summary>
        public AchievementManager Achievements { get; set; }

        /// <summary>Explicit kitchen (tests); null = the registered one (none: no dish is taught).</summary>
        public MealService Meals { get; set; }

        /// <summary>The whole notebook was filled in this save.</summary>
        public bool Completed { get; private set; }

        public int CaughtSpeciesCount
        {
            get
            {
                int count = 0;
                foreach (FishSpec fish in FishCatalog.Fish)
                {
                    if (HasCaught(fish.Id))
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public int TotalSpecies => FishCatalog.Fish.Count;

        public static FishingProgress ResolveShared()
        {
            if (ServiceLocator.TryGet(out FishingProgress progress))
            {
                return progress;
            }

            if (_standalone == null)
            {
                _standalone = new FishingProgress();
                _standalone.Initialize();
            }

            return _standalone;
        }

        private static FishingProgress _standalone;

        public void Initialize()
        {
            if (ServiceLocator.TryGet(out LocalizationManager localization))
            {
                FishingTexts.EnsureRegistered(localization);
            }

            if (_subscribed)
            {
                return;
            }

            _subscribed = true;
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
        }

        public void Shutdown()
        {
            if (!_subscribed)
            {
                return;
            }

            _subscribed = false;
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
        }

        public void ResetForNewSession()
        {
            _records.Clear();
            Completed = false;
        }

        public bool HasCaught(string fishId) => fishId != null && _records.ContainsKey(fishId);

        public int GetCaughtCount(string fishId) => fishId != null && _records.TryGetValue(fishId, out FishRecordEntry entry) ? entry.caught : 0;

        /// <summary>Biggest one landed, in grams (0 = never caught).</summary>
        public int GetBestGrams(string fishId) => fishId != null && _records.TryGetValue(fishId, out FishRecordEntry entry) ? entry.bestGrams : 0;

        /// <summary>Records a landed fish. Unknown species (not in <see cref="FishCatalog"/>) are ignored.</summary>
        public FishCatchOutcome RecordCatch(string fishId, int grams)
        {
            if (FishCatalog.Get(fishId) == null)
            {
                return default;
            }

            grams = Math.Max(1, grams);
            bool newSpecies = !_records.TryGetValue(fishId, out FishRecordEntry entry);
            if (newSpecies)
            {
                entry = new FishRecordEntry { fishId = fishId };
                _records[fishId] = entry;
            }

            int previous = entry.bestGrams;
            bool newRecord = grams > previous;
            entry.caught = entry.caught >= int.MaxValue ? int.MaxValue : entry.caught + 1;
            if (newRecord)
            {
                entry.bestGrams = grams;
            }

            var outcome = new FishCatchOutcome(fishId, grams, newSpecies, newRecord, previous);
            if (newSpecies)
            {
                FeedAchievement(fishId);
                TeachDishes(fishId);
            }

            EventBus.Publish(new FishCaughtEvent(outcome));
            CheckCompleted();
            return outcome;
        }

        /// <summary>Raises <see cref="FishCollectionCompletedEvent"/> the first time every species is in. True only that time.</summary>
        public bool CheckCompleted()
        {
            if (Completed || CaughtSpeciesCount < TotalSpecies || TotalSpecies == 0)
            {
                return false;
            }

            Completed = true;
            EventBus.Publish(new FishCollectionCompletedEvent());
            return true;
        }

        private AchievementManager ResolveAchievements()
        {
            if (Achievements != null)
            {
                return Achievements;
            }

            return ServiceLocator.TryGet(out AchievementManager achievements) ? achievements : null;
        }

        private MealService ResolveMeals()
        {
            if (Meals != null)
            {
                return Meals;
            }

            return ServiceLocator.TryGet(out MealService meals) ? meals : null;
        }

        private void FeedAchievement(string fishId)
        {
            ResolveAchievements()?.RecordProgress(AchievementConditionType.FishSpeciesCaught, 1, fishId, distinct: true);
        }

        private void TeachDishes(string fishId)
        {
            MealService meals = ResolveMeals();
            if (meals == null)
            {
                return;
            }

            foreach (MealRecipeSpec recipe in MealCatalog.TaughtByCatch(fishId))
            {
                meals.Learn(recipe.RecipeId, MealLearnSource.Fishing);
            }
        }

        private void OnGameLoaded(GameLoadedEvent _)
        {
            foreach (string fishId in new List<string>(_records.Keys))
            {
                FeedAchievement(fishId);
                TeachDishes(fishId);
            }

            CheckCompleted();
        }

        public object CaptureState()
        {
            var state = new FishingSaveState { completed = Completed };
            var ids = new List<string>(_records.Keys);
            ids.Sort(StringComparer.Ordinal);
            foreach (string id in ids)
            {
                FishRecordEntry entry = _records[id];
                state.entries.Add(new FishRecordEntry { fishId = id, caught = entry.caught, bestGrams = entry.bestGrams });
            }

            return state;
        }

        public void RestoreState(object state)
        {
            if (state is not FishingSaveState saved)
            {
                return;
            }

            _records.Clear();
            Completed = saved.completed;
            if (saved.entries == null)
            {
                return;
            }

            foreach (FishRecordEntry entry in saved.entries)
            {
                // Species removed in a later version are dropped instead of lingering in the save.
                if (entry == null || FishCatalog.Get(entry.fishId) == null)
                {
                    continue;
                }

                // Hand-edited / corrupted counts still mean "caught at least once".
                int caught = Math.Max(1, entry.caught);
                int best = Math.Max(1, entry.bestGrams);
                if (_records.TryGetValue(entry.fishId, out FishRecordEntry existing))
                {
                    existing.caught = Math.Max(existing.caught, caught);
                    existing.bestGrams = Math.Max(existing.bestGrams, best);
                }
                else
                {
                    _records[entry.fishId] = new FishRecordEntry { fishId = entry.fishId, caught = caught, bestGrams = best };
                }
            }
        }
    }
}
