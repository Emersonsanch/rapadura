using System.Collections.Generic;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;

namespace Rapadura.UI.Crafting
{
    /// <summary>One ingredient line of a recipe as shown in the Crafting screen: how many the player has vs needs.</summary>
    public readonly struct IngredientStatus
    {
        public readonly ItemDefinition Item;
        public readonly int Have;
        public readonly int Need;

        public IngredientStatus(ItemDefinition item, int have, int need)
        {
            Item = item;
            Have = have;
            Need = need;
        }

        public bool IsSatisfied => Have >= Need;
        public string DisplayName => Item != null ? Item.DisplayName : "?";
    }

    /// <summary>
    /// Pure presentation rules for the Crafting screen (have/need counts, labels, why the Craft
    /// button is disabled) — kept out of <see cref="CraftingUIController"/> so they're unit
    /// testable without a UIDocument, same split as <c>SaveSlotPresenter</c>/<c>SaveLoadMenuController</c>.
    /// The actual craftability check is always delegated to <see cref="CraftingManager.CanCraft"/>
    /// so the UI can never disagree with what <see cref="CraftingManager.TryCraft"/> will accept.
    /// </summary>
    public static class CraftingRecipePresenter
    {
        public const string UnknownRecipeReason = "Receita desconhecida";
        public const string MissingMaterialsReason = "Materiais insuficientes";
        public const string NoInventoryReason = "Inventário indisponível";

        /// <summary>
        /// Have/need per ingredient, skipping the same malformed entries (null item or quantity &lt;= 0)
        /// <see cref="CraftingManager.HasAllIngredients"/> skips. A null inventory reports 0 owned.
        /// </summary>
        public static List<IngredientStatus> GetIngredientStatuses(RecipeDefinition recipe, InventoryManager inventory)
        {
            var result = new List<IngredientStatus>();

            if (recipe == null || recipe.Ingredients == null)
            {
                return result;
            }

            foreach (RecipeIngredient ingredient in recipe.Ingredients)
            {
                if (ingredient.item == null || ingredient.quantity <= 0)
                {
                    continue;
                }

                int have = inventory != null ? inventory.GetTotalCount(ingredient.item.ItemId) : 0;
                result.Add(new IngredientStatus(ingredient.item, have, ingredient.quantity));
            }

            return result;
        }

        public static int CountMissingIngredients(IReadOnlyList<IngredientStatus> statuses)
        {
            int missing = 0;

            if (statuses == null)
            {
                return missing;
            }

            foreach (IngredientStatus status in statuses)
            {
                if (!status.IsSatisfied)
                {
                    missing++;
                }
            }

            return missing;
        }

        /// <summary>"have/need", e.g. "1/2".</summary>
        public static string FormatHaveNeed(int have, int need)
        {
            return $"{have}/{need}";
        }

        public static string FormatStationRequirement(CraftingStationType station)
        {
            return $"Requer: {station}";
        }

        public static string FormatResult(RecipeDefinition recipe)
        {
            if (recipe == null || recipe.ResultItem == null)
            {
                return string.Empty;
            }

            return $"Resultado: {recipe.ResultItem.DisplayName} x{recipe.ResultQuantity}";
        }

        public static string FormatSuccessMessage(RecipeDefinition recipe)
        {
            if (recipe == null || recipe.ResultItem == null)
            {
                return "Criado!";
            }

            return $"Criado: {recipe.ResultItem.DisplayName} x{recipe.ResultQuantity}!";
        }

        /// <summary>
        /// Null when <paramref name="recipe"/> can be crafted right now (Craft button enabled),
        /// otherwise a short player-facing reason for the disabled button. Order mirrors
        /// <see cref="CraftingManager.CanCraft"/>'s own checks.
        /// </summary>
        public static string GetCraftBlockReason(CraftingManager craftingManager, RecipeDefinition recipe, InventoryManager inventory, CraftingStationType nearbyStation)
        {
            if (craftingManager == null || recipe == null || recipe.ResultItem == null)
            {
                return UnknownRecipeReason;
            }

            if (inventory == null)
            {
                return NoInventoryReason;
            }

            if (!craftingManager.IsRecipeKnown(recipe))
            {
                return UnknownRecipeReason;
            }

            if (recipe.RequiresCraftingStation && recipe.RequiredStation != nearbyStation)
            {
                return FormatStationRequirement(recipe.RequiredStation);
            }

            if (!craftingManager.HasAllIngredients(recipe, inventory))
            {
                return MissingMaterialsReason;
            }

            // Belt and braces: anything CanCraft rejects that the checks above didn't explain.
            return craftingManager.CanCraft(recipe, inventory, nearbyStation) ? null : MissingMaterialsReason;
        }

        public static bool IsCraftable(CraftingManager craftingManager, RecipeDefinition recipe, InventoryManager inventory, CraftingStationType nearbyStation)
        {
            return GetCraftBlockReason(craftingManager, recipe, inventory, nearbyStation) == null;
        }
    }
}
