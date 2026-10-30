using System;
using Rapadura.Gameplay.Cooking;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>Why a food could not be given (<see cref="MountFeeding.TryPay"/>).</summary>
    public enum MountFeedBlock
    {
        None = 0,
        Diet,
        MissingItems,
        UnknownDish,
    }

    /// <summary>
    /// Paying for a mount's food from the bag (Docs/Mounts.md §4.2): plain items (2 fibras for the capim, the Ração do
    /// Haras, an herb, roast meat) or one of Vó Edna's dishes cooked for the mount — the dish must be known
    /// (<c>MealService</c>) and its ingredients plus the "mantimentos" (gold) are paid, but it does not become the
    /// cousins' meal. Bag access goes through delegates so it is EditMode-testable and works on the map's carried bag.
    /// </summary>
    public static class MountFeeding
    {
        /// <summary>Checks without paying.</summary>
        public static MountFeedBlock Check(MountDiet diet, MountFoodSpec food, Func<string, int> countInBag, Func<string, bool> isDishKnown)
        {
            if (food == null || !MountCareRules.CanEat(diet, food))
            {
                return MountFeedBlock.Diet;
            }

            if (!food.IsDish)
            {
                return countInBag != null && countInBag(food.ItemId) >= food.ItemQuantity ? MountFeedBlock.None : MountFeedBlock.MissingItems;
            }

            MealRecipeSpec recipe = MealCatalog.Get(food.MealRecipeId);
            if (recipe == null || isDishKnown == null || !isDishKnown(food.MealRecipeId))
            {
                return MountFeedBlock.UnknownDish;
            }

            if (countInBag == null)
            {
                return MountFeedBlock.MissingItems;
            }

            int goldNeeded = recipe.ProvisionsCost;
            foreach (MealIngredientSpec ingredient in recipe.Ingredients)
            {
                int need = ingredient.Quantity + (ingredient.ItemId == MealCatalog.ProvisionsItemId ? goldNeeded : 0);
                if (ingredient.ItemId == MealCatalog.ProvisionsItemId)
                {
                    goldNeeded = 0;
                }

                if (countInBag(ingredient.ItemId) < need)
                {
                    return MountFeedBlock.MissingItems;
                }
            }

            return goldNeeded > 0 && countInBag(MealCatalog.ProvisionsItemId) < goldNeeded ? MountFeedBlock.MissingItems : MountFeedBlock.None;
        }

        /// <summary>Checks, then takes the cost out of the bag. None = paid.</summary>
        public static MountFeedBlock TryPay(MountDiet diet, MountFoodSpec food, Func<string, int> countInBag, Func<string, int, bool> removeFromBag,
            Func<string, bool> isDishKnown)
        {
            MountFeedBlock block = Check(diet, food, countInBag, isDishKnown);
            if (block != MountFeedBlock.None || removeFromBag == null)
            {
                return block == MountFeedBlock.None ? MountFeedBlock.MissingItems : block;
            }

            if (!food.IsDish)
            {
                removeFromBag(food.ItemId, food.ItemQuantity);
                return MountFeedBlock.None;
            }

            MealRecipeSpec recipe = MealCatalog.Get(food.MealRecipeId);
            foreach (MealIngredientSpec ingredient in recipe.Ingredients)
            {
                removeFromBag(ingredient.ItemId, ingredient.Quantity);
            }

            if (recipe.ProvisionsCost > 0)
            {
                removeFromBag(MealCatalog.ProvisionsItemId, recipe.ProvisionsCost);
            }

            return MountFeedBlock.None;
        }
    }
}
