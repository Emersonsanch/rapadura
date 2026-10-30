using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;

namespace Rapadura.Gameplay.Cooking
{
    /// <summary>JSON-serializable snapshot of <see cref="MealService"/>.</summary>
    [Serializable]
    public class MealSaveState
    {
        /// <summary>Recipes learned from quests or Vó Zefa's lessons (starters are implicit).</summary>
        public string[] learnedRecipeIds = Array.Empty<string>();
        public string activeRecipeId;
        public int encountersRemaining;
    }

    /// <summary>Raised when the active meal changes: cooked, refreshed, replaced, counted down, run out, loaded or reset.</summary>
    public readonly struct MealChangedEvent : IGameEvent
    {
        public readonly ActiveMeal Meal;
        /// <summary>True when the change was the meal running out after an encounter.</summary>
        public readonly bool Expired;

        public MealChangedEvent(ActiveMeal meal, bool expired)
        {
            Meal = meal;
            Expired = expired;
        }
    }

    /// <summary>How a recipe was learned (<see cref="MealRecipeLearnedEvent"/>).</summary>
    public enum MealLearnSource
    {
        Quest,
        GrandmaLesson,
    }

    /// <summary>Raised once per recipe, the moment it is learned.</summary>
    public readonly struct MealRecipeLearnedEvent : IGameEvent
    {
        public readonly string RecipeId;
        public readonly MealLearnSource Source;

        public MealRecipeLearnedEvent(string recipeId, MealLearnSource source)
        {
            RecipeId = recipeId;
            Source = source;
        }
    }

    /// <summary>
    /// "Cozinha da Vó Zefa": which dishes are known and which meal is active. Per save slot, like
    /// <see cref="VillageChestStorage"/>: a GameManager-built manager registered with <c>SaveManager</c> under
    /// <see cref="Key"/> and with <c>NewSessionReset</c>; a save from before the stove loads with the starters, what
    /// its completed quests teach and no meal.
    /// <list type="bullet">
    /// <item>Cooking (<see cref="TryCook(string, InventoryManager, out ServeOutcome)"/>) takes ingredients/provisions
    /// from the given bag and serves the meal at once (<see cref="CookingRules"/>).</item>
    /// <item>Counts down on <see cref="EncounterEndedEvent"/> (published before the post-encounter auto-save, so the
    /// count is saved with it); learns on <see cref="QuestCompletedEvent"/>.</item>
    /// <item>The effects reach the hero in the arena through <see cref="MealEffectApplier"/>.</item>
    /// </list>
    /// </summary>
    public class MealService : IManager, ISaveable, IResettable
    {
        public const string Key = "VillageKitchen";

        private readonly HashSet<string> _learned = new HashSet<string>();
        private readonly Func<string, bool> _isQuestCompleted;
        private readonly Func<BiomeType, bool> _isRegionUnlocked;
        private ActiveMeal _active = ActiveMeal.None;
        private bool _subscribed;

        /// <param name="isQuestCompleted">Defaults to the registered <see cref="QuestManager"/>.</param>
        /// <param name="isRegionUnlocked">Defaults to the registered <see cref="WorldMapProgress"/>.</param>
        public MealService(Func<string, bool> isQuestCompleted = null, Func<BiomeType, bool> isRegionUnlocked = null)
        {
            _isQuestCompleted = isQuestCompleted ?? (questId => ServiceLocator.TryGet(out QuestManager quests) && quests.IsCompleted(questId));
            _isRegionUnlocked = isRegionUnlocked ?? (region => ServiceLocator.TryGet(out WorldMapProgress progress) && progress.IsUnlocked(region));
        }

        private static MealService _standalone;

        /// <summary>The GameManager's service (ServiceLocator); without one, a process-wide stand-alone one that is not saved.</summary>
        public static MealService ResolveShared()
        {
            if (ServiceLocator.TryGet(out MealService service))
            {
                return service;
            }

            if (_standalone == null)
            {
                _standalone = new MealService();
                _standalone.Initialize();
            }

            return _standalone;
        }

        public string SaveKey => Key;

        public ActiveMeal Active => _active;

        public MealRecipeSpec ActiveRecipe => _active.IsActive ? MealCatalog.Get(_active.recipeId) : null;

        public IReadOnlyCollection<string> LearnedRecipeIds => _learned;

        public void Initialize()
        {
            if (_subscribed)
            {
                return;
            }

            EventBus.Subscribe<EncounterEndedEvent>(OnEncounterEnded);
            EventBus.Subscribe<QuestCompletedEvent>(OnQuestCompleted);
            _subscribed = true;
        }

        public void Shutdown()
        {
            EventBus.Unsubscribe<EncounterEndedEvent>(OnEncounterEnded);
            EventBus.Unsubscribe<QuestCompletedEvent>(OnQuestCompleted);
            _subscribed = false;
        }

        public bool IsKnown(string recipeId) => CookingRules.IsKnown(MealCatalog.Get(recipeId), _learned, _isQuestCompleted);

        public CookCheck Check(string recipeId, Func<string, int> countInBag)
        {
            MealRecipeSpec recipe = MealCatalog.Get(recipeId);
            return CookingRules.Validate(recipe, recipe != null && IsKnown(recipeId), countInBag);
        }

        /// <summary>Cooks and eats <paramref name="recipeId"/> with what is in <paramref name="bag"/>.</summary>
        public CookCheck TryCook(string recipeId, InventoryManager bag, out ServeOutcome outcome)
        {
            if (bag == null)
            {
                outcome = ServeOutcome.Started;
                return CookCheck.MissingIngredients;
            }

            return TryCook(recipeId, bag.GetTotalCount, bag.RemoveById, out outcome);
        }

        /// <summary>Same, on any bag (tests): <paramref name="countInBag"/> / <paramref name="removeFromBag"/> as in <see cref="CookingRules"/>.</summary>
        public CookCheck TryCook(string recipeId, Func<string, int> countInBag, Func<string, int, bool> removeFromBag, out ServeOutcome outcome)
        {
            outcome = ServeOutcome.Started;
            CookCheck check = Check(recipeId, countInBag);
            if (check != CookCheck.Ok)
            {
                return check;
            }

            MealRecipeSpec recipe = MealCatalog.Get(recipeId);
            if (!CookingRules.ConsumeIngredients(recipe, removeFromBag))
            {
                return CookCheck.MissingIngredients;
            }

            _active = CookingRules.Serve(_active, recipe, out outcome);
            EventBus.Publish(new MealChangedEvent(_active, false));
            return CookCheck.Ok;
        }

        /// <summary>
        /// At the stove with <paramref name="hero"/>: Vó Zefa teaches the cousin's favourite if
        /// <see cref="CookingRules.ShouldTeachFavourite"/> says so. Returns the dish taught, or null.
        /// </summary>
        public MealRecipeSpec TryTeachFavourite(CharacterId? hero)
        {
            if (!hero.HasValue)
            {
                return null;
            }

            MealRecipeSpec favourite = MealCatalog.FavouriteOf(hero.Value);
            if (favourite == null || !CookingRules.ShouldTeachFavourite(favourite, hero, IsKnown(favourite.RecipeId), _isRegionUnlocked))
            {
                return null;
            }

            return Learn(favourite.RecipeId, MealLearnSource.GrandmaLesson) ? favourite : null;
        }

        /// <summary>Learns a recipe (no-op for an unknown id or one already learned). Returns whether it was new.</summary>
        public bool Learn(string recipeId, MealLearnSource source)
        {
            if (MealCatalog.Get(recipeId) == null || _learned.Contains(recipeId))
            {
                return false;
            }

            bool wasKnown = IsKnown(recipeId);
            _learned.Add(recipeId);
            if (!wasKnown)
            {
                EventBus.Publish(new MealRecipeLearnedEvent(recipeId, source));
            }

            return !wasKnown;
        }

        /// <summary>Counts one resolved encounter against the active meal (what <see cref="EncounterEndedEvent"/> triggers).</summary>
        public void CountEncounter()
        {
            if (!_active.IsActive)
            {
                return;
            }

            _active = CookingRules.CountEncounter(_active, out bool expired);
            EventBus.Publish(new MealChangedEvent(_active, expired));
        }

        private void OnEncounterEnded(EncounterEndedEvent evt) => CountEncounter();

        private void OnQuestCompleted(QuestCompletedEvent evt)
        {
            MealRecipeSpec recipe = MealCatalog.TaughtByQuest(evt.QuestId);
            if (recipe != null)
            {
                // The quest manager may not count the quest as completed yet when this fires; learning it
                // explicitly keeps the event (and the saved list) independent of that order.
                Learn(recipe.RecipeId, MealLearnSource.Quest);
            }
        }

        public void ResetForNewSession()
        {
            _learned.Clear();
            _active = ActiveMeal.None;
            EventBus.Publish(new MealChangedEvent(_active, false));
        }

        public object CaptureState()
        {
            var learned = new List<string>(_learned);
            learned.Sort(StringComparer.Ordinal);
            return new MealSaveState
            {
                learnedRecipeIds = learned.ToArray(),
                activeRecipeId = _active.IsActive ? _active.recipeId : null,
                encountersRemaining = _active.IsActive ? _active.encountersRemaining : 0,
            };
        }

        public void RestoreState(object state)
        {
            if (state is not MealSaveState saved)
            {
                return;
            }

            _learned.Clear();
            if (saved.learnedRecipeIds != null)
            {
                foreach (string recipeId in saved.learnedRecipeIds)
                {
                    // Dishes removed in a later version are dropped instead of lingering in the save.
                    if (MealCatalog.Get(recipeId) != null)
                    {
                        _learned.Add(recipeId);
                    }
                }
            }

            _active = CookingRules.Sanitize(new ActiveMeal(saved.activeRecipeId, saved.encountersRemaining));
            EventBus.Publish(new MealChangedEvent(_active, false));
        }
    }
}
