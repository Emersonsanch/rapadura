using System;
using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Cooking;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Quests;

namespace Rapadura.UI.WorldMap
{
    /// <summary>Texts and state of one dish row of the "Fogão a lenha" window (<see cref="KitchenPresenter.BuildRow"/>).</summary>
    public sealed class KitchenRecipeRow
    {
        public string RecipeId;
        public bool Known;
        public bool CanCook;
        /// <summary>This dish is the one in the hero's belly (the button says "renova").</summary>
        public bool IsActive;
        /// <summary>The hero at the stove loves this dish.</summary>
        public bool IsHeroFavourite;
        public string Title;
        public string Effect;
        /// <summary>"Madeira 1/2 · Erva 0/1" (ingredients only; provisions in <see cref="Provisions"/>).</summary>
        public string Ingredients;
        public string Provisions;
        public string Lasts;
        /// <summary>"Favorito de Maria: ..." or empty.</summary>
        public string Favourite;
        /// <summary>How to learn it (unknown dishes only), else empty.</summary>
        public string HowToLearn;
        public string ButtonText;
    }

    /// <summary>
    /// Pure text building for <see cref="VillageKitchenController"/> and <see cref="MealHudBadge"/> (tested by
    /// <c>CookingTests</c>): no UI Toolkit, inventory reads through a count function.
    /// </summary>
    public static class KitchenPresenter
    {
        public static string DishName(LocalizationManager manager, MealRecipeSpec recipe) =>
            recipe != null ? KitchenTexts.Get(manager, recipe.NameKey) : string.Empty;

        public static KitchenRecipeRow BuildRow(MealRecipeSpec recipe, bool known, Func<string, int> countInBag, ActiveMeal active, CharacterId? hero, LocalizationManager manager)
        {
            var row = new KitchenRecipeRow
            {
                RecipeId = recipe.RecipeId,
                Known = known,
                IsActive = active.IsActive && active.recipeId == recipe.RecipeId,
                IsHeroFavourite = CookingRules.IsFavourite(recipe, hero),
            };

            row.Title = known ? DishName(manager, recipe) : KitchenTexts.Get(manager, KitchenTexts.Unknown);
            row.Effect = known ? KitchenTexts.Get(manager, recipe.EffectKey) : string.Empty;
            row.Lasts = known ? KitchenTexts.Format(manager, KitchenTexts.Lasts, recipe.Encounters) : string.Empty;
            row.Ingredients = known ? IngredientsLine(recipe, countInBag, manager) : string.Empty;
            row.Provisions = known && recipe.ProvisionsCost > 0
                ? KitchenTexts.Format(manager, KitchenTexts.Provisions, recipe.ProvisionsCost, Count(countInBag, MealCatalog.ProvisionsItemId))
                : string.Empty;
            row.Favourite = known && recipe.FavouriteOf.HasValue
                ? KitchenTexts.Format(manager, KitchenTexts.FavouriteOf, WorldMapHudPresenter.HeroName(manager, recipe.FavouriteOf.Value), KitchenTexts.Get(manager, recipe.FavouriteKey))
                : string.Empty;
            row.HowToLearn = known ? string.Empty : HowToLearn(recipe, manager);
            row.CanCook = known && CookingRules.Validate(recipe, true, countInBag) == CookCheck.Ok;
            row.ButtonText = KitchenTexts.Get(manager, row.IsActive ? KitchenTexts.CookAgain : KitchenTexts.Cook);
            return row;
        }

        /// <summary>"Madeira 1/2 · Erva 0/1".</summary>
        public static string IngredientsLine(MealRecipeSpec recipe, Func<string, int> countInBag, LocalizationManager manager)
        {
            var parts = new List<string>();
            foreach (MealIngredientSpec ingredient in recipe.Ingredients)
            {
                parts.Add(KitchenTexts.Format(manager, KitchenTexts.Ingredient, ItemName(ingredient.ItemId, manager), Count(countInBag, ingredient.ItemId), ingredient.Quantity));
            }

            return string.Join(" · ", parts);
        }

        /// <summary>"Aprenda: missão "Trilha dos Uivos" ou a Vó ensina pra Maria" (whichever routes the dish has).</summary>
        public static string HowToLearn(MealRecipeSpec recipe, LocalizationManager manager)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(recipe.UnlockQuestId))
            {
                parts.Add(KitchenTexts.Format(manager, KitchenTexts.LearnByQuest, QuestTitle(recipe.UnlockQuestId, manager)));
            }

            if (recipe.FavouriteOf.HasValue)
            {
                parts.Add(KitchenTexts.Format(manager, KitchenTexts.LearnByLesson, WorldMapHudPresenter.HeroName(manager, recipe.FavouriteOf.Value)));
            }

            return string.Join(" ", parts);
        }

        /// <summary>"Na barriga: Cuscuz · faltam 2 lutas" / "... · última luta" / "Barriga vazia...".</summary>
        public static string ActiveMealLine(ActiveMeal meal, LocalizationManager manager)
        {
            MealRecipeSpec recipe = meal.IsActive ? MealCatalog.Get(meal.recipeId) : null;
            if (recipe == null)
            {
                return KitchenTexts.Get(manager, KitchenTexts.NoMeal);
            }

            return meal.encountersRemaining == 1
                ? KitchenTexts.Format(manager, KitchenTexts.ActiveMealLast, DishName(manager, recipe))
                : KitchenTexts.Format(manager, KitchenTexts.ActiveMeal, DishName(manager, recipe), meal.encountersRemaining);
        }

        /// <summary>HUD badge text ("Cuscuz · 2" / "Cuscuz · última"), or null with no meal (badge hidden).</summary>
        public static string HudBadgeText(ActiveMeal meal, LocalizationManager manager)
        {
            MealRecipeSpec recipe = meal.IsActive ? MealCatalog.Get(meal.recipeId) : null;
            if (recipe == null)
            {
                return null;
            }

            return meal.encountersRemaining == 1
                ? KitchenTexts.Format(manager, KitchenTexts.HudBadgeLast, DishName(manager, recipe))
                : KitchenTexts.Format(manager, KitchenTexts.HudBadge, DishName(manager, recipe), meal.encountersRemaining);
        }

        /// <summary>USS modifier class of the dish icon (<c>meal-icon--cuscuz</c>, VillageKitchen.uss).</summary>
        public static string IconClass(string recipeId)
        {
            return "meal-icon--" + (recipeId != null && recipeId.StartsWith(MealCatalog.RecipeIdPrefix) ? recipeId.Substring(MealCatalog.RecipeIdPrefix.Length) : recipeId);
        }

        /// <summary>One letter drawn on the dish icon (the font has no food glyphs).</summary>
        public static string IconGlyph(MealRecipeSpec recipe, LocalizationManager manager)
        {
            string name = DishName(manager, recipe);
            return string.IsNullOrEmpty(name) ? "?" : name.Substring(0, 1).ToUpperInvariant();
        }

        /// <summary>Vó Zefa's line when <paramref name="hero"/> opens the stove (the lesson line when she just taught them).</summary>
        public static string GrandmaLine(CharacterId? hero, MealRecipeSpec taught, LocalizationManager manager)
        {
            if (!hero.HasValue)
            {
                return KitchenTexts.Get(manager, KitchenTexts.GrandmaGreetingAny);
            }

            return KitchenTexts.Get(manager, taught != null ? KitchenTexts.LessonKey(hero.Value) : KitchenTexts.GreetingKey(hero.Value));
        }

        /// <summary>Status line after pressing "Cozinhar". <paramref name="previous"/> = the meal before cooking.</summary>
        public static string CookStatus(CookCheck check, ServeOutcome outcome, MealRecipeSpec recipe, MealRecipeSpec previous, bool favourite, LocalizationManager manager)
        {
            switch (check)
            {
                case CookCheck.MissingIngredients:
                    return KitchenTexts.Get(manager, KitchenTexts.StatusMissing);
                case CookCheck.NotEnoughProvisions:
                    return KitchenTexts.Get(manager, KitchenTexts.StatusProvisions);
                case CookCheck.NotLearned:
                case CookCheck.UnknownRecipe:
                    return KitchenTexts.Get(manager, KitchenTexts.StatusNotLearned);
            }

            string dish = DishName(manager, recipe);
            string text = outcome switch
            {
                ServeOutcome.Refreshed => KitchenTexts.Format(manager, KitchenTexts.StatusRefreshed, dish, recipe.Encounters),
                ServeOutcome.Replaced => KitchenTexts.Format(manager, KitchenTexts.StatusReplaced, dish, DishName(manager, previous)),
                _ => KitchenTexts.Format(manager, KitchenTexts.StatusStarted, dish, recipe.Encounters),
            };

            return favourite ? text + " " + KitchenTexts.Get(manager, recipe.FavouriteKey) : text;
        }

        private static string ItemName(string itemId, LocalizationManager manager)
        {
            ItemDefinition item = ItemDatabase.GetById(itemId);
            return item != null ? item.LocalizedName(manager) : itemId;
        }

        private static string QuestTitle(string questId, LocalizationManager manager)
        {
            string key = RegionQuestCatalog.QuestKeyBase(questId) + ".title";
            if (manager != null)
            {
                RegionQuestTexts.EnsureLoaded(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            return RegionQuestTexts.GetFallback(key) ?? questId;
        }

        private static int Count(Func<string, int> countInBag, string itemId) => countInBag != null ? countInBag(itemId) : 0;
    }
}
