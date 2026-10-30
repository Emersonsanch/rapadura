using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Skills;

namespace Rapadura.Gameplay.Cooking
{
    /// <summary>Why a dish can or can't be cooked right now (<see cref="CookingRules.Validate"/>).</summary>
    public enum CookCheck
    {
        Ok,
        UnknownRecipe,
        NotLearned,
        MissingIngredients,
        NotEnoughProvisions,
    }

    /// <summary>What eating a dish did to the meal already active (<see cref="CookingRules.Serve"/>).</summary>
    public enum ServeOutcome
    {
        /// <summary>No meal was active.</summary>
        Started,
        /// <summary>The same dish again: back to its full encounter count (never more).</summary>
        Refreshed,
        /// <summary>A different dish: the old one is gone (only one meal at a time).</summary>
        Replaced,
    }

    /// <summary>The meal currently in the hero's belly: which dish and how many encounters it has left.</summary>
    [Serializable]
    public struct ActiveMeal
    {
        public string recipeId;
        public int encountersRemaining;

        public ActiveMeal(string recipeId, int encountersRemaining)
        {
            this.recipeId = recipeId;
            this.encountersRemaining = encountersRemaining;
        }

        public static readonly ActiveMeal None = new ActiveMeal(null, 0);

        public bool IsActive => !string.IsNullOrEmpty(recipeId) && encountersRemaining > 0;
    }

    /// <summary>
    /// The rules of Vó Zefa's stove, pure and Unity-free (tested by <c>CookingTests</c>):
    /// <list type="bullet">
    /// <item><b>Validation</b>: the dish must exist and be learned; every ingredient and the provisions must be in the
    /// bag (checked before anything is taken, so a failed cook never eats half the ingredients).</item>
    /// <item><b>Consumption</b>: ingredients, then provisions, each removed in full.</item>
    /// <item><b>Stacking</b>: one meal at a time. Same dish → back to its full count (not added); another dish →
    /// replaces it. Meal effects live under their own <see cref="EffectSourceKey"/>/<see cref="BurstSourceKey"/> on the
    /// BuffController, so they stack with skills, attributes and equipment (different sources) but never with
    /// another meal.</item>
    /// <item><b>Duration</b>: counted in encounters, not real time — every resolved encounter (victory, defeat or
    /// flight) takes one; at zero the meal is over.</item>
    /// </list>
    /// </summary>
    public static class CookingRules
    {
        /// <summary>BuffController source of a meal's whole-encounter effects (favourite bonus included).</summary>
        public const string EffectSourceKey = "meal/effects";
        /// <summary>BuffController source of a meal's opening burst (applied once per encounter).</summary>
        public const string BurstSourceKey = "meal/burst";

        /// <param name="isLearned">Whether the recipe is known (<see cref="MealService.IsKnown"/>).</param>
        /// <param name="countInBag">Units of an item id in the bag.</param>
        public static CookCheck Validate(MealRecipeSpec recipe, bool isLearned, Func<string, int> countInBag)
        {
            if (recipe == null)
            {
                return CookCheck.UnknownRecipe;
            }

            if (!isLearned)
            {
                return CookCheck.NotLearned;
            }

            foreach (MealIngredientSpec ingredient in recipe.Ingredients)
            {
                if (Count(countInBag, ingredient.ItemId) < ingredient.Quantity)
                {
                    return CookCheck.MissingIngredients;
                }
            }

            if (recipe.ProvisionsCost > 0 && Count(countInBag, MealCatalog.ProvisionsItemId) < recipe.ProvisionsCost)
            {
                return CookCheck.NotEnoughProvisions;
            }

            return CookCheck.Ok;
        }

        /// <summary>
        /// Takes the ingredients and provisions of <paramref name="recipe"/> through <paramref name="removeFromBag"/>
        /// (item id, quantity → whether it was removed). Call only after <see cref="Validate"/> said Ok; returns false
        /// if any removal failed (the bag changed in between).
        /// </summary>
        public static bool ConsumeIngredients(MealRecipeSpec recipe, Func<string, int, bool> removeFromBag)
        {
            if (recipe == null || removeFromBag == null)
            {
                return false;
            }

            bool all = true;
            foreach (MealIngredientSpec ingredient in recipe.Ingredients)
            {
                if (ingredient.Quantity > 0)
                {
                    all &= removeFromBag(ingredient.ItemId, ingredient.Quantity);
                }
            }

            if (recipe.ProvisionsCost > 0)
            {
                all &= removeFromBag(MealCatalog.ProvisionsItemId, recipe.ProvisionsCost);
            }

            return all;
        }

        /// <summary>Eats <paramref name="recipe"/> on top of <paramref name="current"/>: the new active meal, and what happened to the old one.</summary>
        public static ActiveMeal Serve(ActiveMeal current, MealRecipeSpec recipe, out ServeOutcome outcome)
        {
            if (!current.IsActive)
            {
                outcome = ServeOutcome.Started;
            }
            else if (current.recipeId == recipe.RecipeId)
            {
                outcome = ServeOutcome.Refreshed;
            }
            else
            {
                outcome = ServeOutcome.Replaced;
            }

            // Refreshing never adds up: cooking the same dish twice in a row is wasted food, not a longer meal.
            return new ActiveMeal(recipe.RecipeId, Math.Max(1, recipe.Encounters));
        }

        /// <summary>One encounter resolved with the meal in effect: one fewer left, <see cref="ActiveMeal.None"/> when it runs out.</summary>
        public static ActiveMeal CountEncounter(ActiveMeal current, out bool expired)
        {
            expired = false;
            if (!current.IsActive)
            {
                return ActiveMeal.None;
            }

            int left = current.encountersRemaining - 1;
            if (left <= 0)
            {
                expired = true;
                return ActiveMeal.None;
            }

            return new ActiveMeal(current.recipeId, left);
        }

        /// <summary>
        /// A meal read back from a save: unknown dish or nothing left → none; more encounters than the dish gives
        /// (a later rebalance) → clamped to its full count.
        /// </summary>
        public static ActiveMeal Sanitize(ActiveMeal saved)
        {
            MealRecipeSpec recipe = MealCatalog.Get(saved.recipeId);
            if (recipe == null || saved.encountersRemaining <= 0)
            {
                return ActiveMeal.None;
            }

            return new ActiveMeal(saved.recipeId, Math.Min(saved.encountersRemaining, Math.Max(1, recipe.Encounters)));
        }

        /// <summary>Whether <paramref name="hero"/> gets the dish's favourite bonus.</summary>
        public static bool IsFavourite(MealRecipeSpec recipe, CharacterId? hero)
        {
            return recipe != null && hero.HasValue && recipe.FavouriteOf == hero;
        }

        /// <summary>Effects held for the whole encounter (the dish's, plus the favourite bonus unless it is a burst).</summary>
        public static List<StatModifierDefinition> BuildEncounterEffects(MealRecipeSpec recipe, CharacterId? hero)
        {
            var effects = new List<StatModifierDefinition>();
            if (recipe == null)
            {
                return effects;
            }

            effects.AddRange(recipe.Effects);
            if (IsFavourite(recipe, hero) && !recipe.FavouriteBonusIsBurst)
            {
                effects.AddRange(recipe.FavouriteBonus);
            }

            return effects;
        }

        /// <summary>Effects applied once as the fight starts, for their own duration (Rapadura, plus Ícaro's bonus).</summary>
        public static List<StatModifierDefinition> BuildOpeningBurst(MealRecipeSpec recipe, CharacterId? hero)
        {
            var effects = new List<StatModifierDefinition>();
            if (recipe == null)
            {
                return effects;
            }

            effects.AddRange(recipe.OpeningBurst);
            if (IsFavourite(recipe, hero) && recipe.FavouriteBonusIsBurst)
            {
                effects.AddRange(recipe.FavouriteBonus);
            }

            return effects;
        }

        /// <summary>
        /// Whether a recipe is known: a starter, learned (quest/lesson, saved) or taught by a quest already completed
        /// (so a save from before the stove existed knows what its finished quests teach).
        /// </summary>
        public static bool IsKnown(MealRecipeSpec recipe, ICollection<string> learned, Func<string, bool> isQuestCompleted)
        {
            if (recipe == null)
            {
                return false;
            }

            if (recipe.Starter || (learned != null && learned.Contains(recipe.RecipeId)))
            {
                return true;
            }

            return !string.IsNullOrEmpty(recipe.UnlockQuestId) && isQuestCompleted != null && isQuestCompleted(recipe.UnlockQuestId);
        }

        /// <summary>
        /// Whether Vó Zefa teaches <paramref name="recipe"/> at the stove to <paramref name="hero"/>: it is that cousin's
        /// favourite, not known yet, and its region is open (<paramref name="isRegionUnlocked"/>).
        /// </summary>
        public static bool ShouldTeachFavourite(MealRecipeSpec recipe, CharacterId? hero, bool alreadyKnown, Func<World.Biomes.BiomeType, bool> isRegionUnlocked)
        {
            if (!IsFavourite(recipe, hero) || alreadyKnown)
            {
                return false;
            }

            return !recipe.LessonRegion.HasValue || (isRegionUnlocked != null && isRegionUnlocked(recipe.LessonRegion.Value));
        }

        private static int Count(Func<string, int> countInBag, string itemId)
        {
            return countInBag != null ? countInBag(itemId) : 0;
        }
    }
}
