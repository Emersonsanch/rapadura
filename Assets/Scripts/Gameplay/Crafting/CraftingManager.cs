using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Inventory;

namespace Rapadura.Gameplay.Crafting
{
    /// <summary>
    /// Central runtime authority over crafting: which recipes are known, whether a recipe can
    /// currently be crafted (ingredients present, player level, nearby station) and consuming
    /// ingredients/producing the result against a given <see cref="InventoryManager"/>. Reads
    /// every per-recipe number from the data-driven <see cref="RecipeDefinition"/> asset — this
    /// class contains no per-recipe special-casing, mirroring how <c>SkillManager</c> treats
    /// <c>SkillDefinition</c>.
    ///
    /// This is a plain <see cref="IManager"/> (not a MonoBehaviour) following the same pattern as
    /// <c>CheckpointManager</c>/<c>SaveManager</c>: constructed once by GameManager and registered
    /// in the ServiceLocator. It intentionally does not hold a hard reference to any particular
    /// player's <see cref="InventoryManager"/> — every crafting operation takes the inventory to
    /// operate on as a parameter, which keeps this manager usable for NPC crafting/multiple
    /// characters and easy to unit-test without a scene.
    ///
    /// Constructed and registered in <c>GameManager.BuildManagers()</c> alongside the other
    /// top-level managers.
    /// </summary>
    public class CraftingManager : IManager
    {
        private readonly HashSet<string> _knownRecipeIds = new HashSet<string>();

        public void Initialize()
        {
            foreach (RecipeDefinition recipe in RecipeDatabase.GetAll())
            {
                if (recipe != null && recipe.KnownByDefault)
                {
                    _knownRecipeIds.Add(recipe.RecipeId);
                }
            }
        }

        public void Shutdown()
        {
            _knownRecipeIds.Clear();
        }

        // ---------------------------------------------------------------
        // Unlocking
        // ---------------------------------------------------------------

        public bool IsRecipeKnown(RecipeDefinition recipe)
        {
            return recipe != null && (recipe.KnownByDefault || _knownRecipeIds.Contains(recipe.RecipeId));
        }

        public bool IsRecipeKnown(string recipeId)
        {
            return !string.IsNullOrEmpty(recipeId) && _knownRecipeIds.Contains(recipeId);
        }

        /// <summary>Unlocks a recipe so it becomes craftable. Returns false if already known or level requirement unmet.</summary>
        public bool UnlockRecipe(RecipeDefinition recipe, int playerLevel)
        {
            if (recipe == null || IsRecipeKnown(recipe) || playerLevel < recipe.MinimumPlayerLevel)
            {
                return false;
            }

            _knownRecipeIds.Add(recipe.RecipeId);
            EventBus.Publish(new RecipeUnlockedEvent(recipe));
            return true;
        }

        // ---------------------------------------------------------------
        // Crafting
        // ---------------------------------------------------------------

        /// <summary>
        /// Checks whether <paramref name="recipe"/> can be crafted right now: recipe must be
        /// known, the inventory must hold every ingredient in the required quantity, and if the
        /// recipe requires a crafting station, <paramref name="nearbyStation"/> must match it.
        /// </summary>
        public bool CanCraft(RecipeDefinition recipe, InventoryManager inventory, CraftingStationType nearbyStation = CraftingStationType.None)
        {
            if (recipe == null || inventory == null || recipe.ResultItem == null)
            {
                return false;
            }

            if (!IsRecipeKnown(recipe))
            {
                return false;
            }

            if (recipe.RequiresCraftingStation && recipe.RequiredStation != nearbyStation)
            {
                return false;
            }

            return HasAllIngredients(recipe, inventory);
        }

        public bool HasAllIngredients(RecipeDefinition recipe, InventoryManager inventory)
        {
            if (recipe == null || inventory == null)
            {
                return false;
            }

            foreach (RecipeIngredient ingredient in recipe.Ingredients)
            {
                if (ingredient.item == null || ingredient.quantity <= 0)
                {
                    continue;
                }

                if (inventory.GetTotalCount(ingredient.item.ItemId) < ingredient.quantity)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Attempts to craft one instance of <paramref name="recipe"/> against <paramref name="inventory"/>:
        /// consumes every ingredient, then adds the result item. Ingredients are only consumed if the
        /// full craft succeeds (checked up-front), and the consumption is rolled back if, for any
        /// reason, adding the result item fails to fit (extremely unlikely, but keeps inventory state
        /// consistent rather than silently destroying materials).
        /// </summary>
        public bool TryCraft(RecipeDefinition recipe, InventoryManager inventory, CraftingStationType nearbyStation = CraftingStationType.None)
        {
            if (!CanCraft(recipe, inventory, nearbyStation))
            {
                return false;
            }

            foreach (RecipeIngredient ingredient in recipe.Ingredients)
            {
                if (ingredient.item == null || ingredient.quantity <= 0)
                {
                    continue;
                }

                inventory.RemoveById(ingredient.item.ItemId, ingredient.quantity);
            }

            int leftover = inventory.AddItem(recipe.ResultItem, recipe.ResultQuantity);

            if (leftover > 0)
            {
                // No space for the result — refund the consumed ingredients rather than lose them.
                foreach (RecipeIngredient ingredient in recipe.Ingredients)
                {
                    if (ingredient.item == null || ingredient.quantity <= 0)
                    {
                        continue;
                    }

                    inventory.AddItem(ingredient.item, ingredient.quantity);
                }

                // Whatever fraction of the result DID fit must also be removed again.
                if (leftover < recipe.ResultQuantity)
                {
                    inventory.RemoveById(recipe.ResultItem.ItemId, recipe.ResultQuantity - leftover);
                }

                return false;
            }

            EventBus.Publish(new RecipeCraftedEvent(recipe));
            return true;
        }
    }
}
