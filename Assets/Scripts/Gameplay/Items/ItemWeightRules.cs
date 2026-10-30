using System;
using Rapadura.Gameplay.Inventory;

namespace Rapadura.Gameplay.Items
{
    /// <summary>
    /// How much one unit of an item weighs (Docs/Balance/README.md, "Peso e carga"). Plain C#, shared with a future
    /// server. Rules, in order:
    /// <list type="number">
    /// <item>Coins and gold (<see cref="ItemType.Currency"/>, <see cref="ItemCategory.Currency"/>,
    /// <see cref="ItemCategory.Gold"/>) weigh nothing.</item>
    /// <item>An authored <see cref="ItemDefinition.Weight"/> above 0 wins (the designer's number).</item>
    /// <item>Otherwise (0 or unset) the suggestion: grid cells (<see cref="ItemFootprint"/>, MU size) × the category's
    /// density (<see cref="DensityPerCell"/>), never below <see cref="MinUnitWeight"/>. So every item has a weight.</item>
    /// </list>
    /// Stacks weigh per unit (<see cref="StackWeight"/>). Weights are in "peso" units (≈ kg; a sword is 3).
    /// </summary>
    public static class ItemWeightRules
    {
        /// <summary>The lightest a non-currency item can be (a rune, a note, a whistle).</summary>
        public const float MinUnitWeight = 0.05f;

        /// <summary>True for what never weighs: coins and gold.</summary>
        public static bool IsWeightless(ItemType type, ItemCategory category)
        {
            return type == ItemType.Currency || category == ItemCategory.Currency || category == ItemCategory.Gold;
        }

        /// <summary>Weight of one grid cell of an item of <paramref name="category"/> (its "material").</summary>
        public static float DensityPerCell(ItemCategory category)
        {
            switch (category)
            {
                case ItemCategory.Wood: return 0.5f;
                case ItemCategory.Stone: return 1f;
                case ItemCategory.Metal: return 1.2f;
                case ItemCategory.Herb: return 0.2f;
                case ItemCategory.Fiber: return 0.1f;
                case ItemCategory.Food: return 0.3f;
                case ItemCategory.Potion: return 0.4f;
                case ItemCategory.MeleeWeapon: return 1f;      // 1×3 → 3 (espada de ferro)
                case ItemCategory.RangedWeapon: return 0.2f;   // 2×4 → 1.6 (arco)
                case ItemCategory.MagicWeapon: return 0.5f;    // 1×4 → 2 (cajado)
                case ItemCategory.Head: return 0.25f;          // 2×2 → 1
                case ItemCategory.Chest: return 0.4f;          // 2×3 → 2.4
                case ItemCategory.Legs: return 0.4f;           // 2×2 → 1.6
                case ItemCategory.Boots: return 0.25f;         // 2×2 → 1
                case ItemCategory.Accessory: return 0.1f;      // anel/amuleto 0.1; asas/capa 2×2 → 0.4
                case ItemCategory.GatheringTool: return 0.85f; // 1×3 → 2.5 (machado, picareta)
                case ItemCategory.BuildingTool: return 1f;     // 1×2 → 2 (martelo)
                case ItemCategory.QuestItem: return 0.05f;
                case ItemCategory.Blueprint: return 0.05f;
                case ItemCategory.Currency:
                case ItemCategory.Gold: return 0f;
                default: return 0.1f;                          // Miscellaneous
            }
        }

        /// <summary>Suggested unit weight from the category and the grid footprint (cells = width × height).</summary>
        public static float SuggestedUnitWeight(ItemType type, ItemCategory category, int cells)
        {
            if (IsWeightless(type, category))
            {
                return 0f;
            }

            return Math.Max(MinUnitWeight, Math.Max(1, cells) * DensityPerCell(category));
        }

        /// <summary>Unit weight from raw data (see the class rules).</summary>
        public static float UnitWeight(ItemType type, ItemCategory category, float authoredWeight, int cells)
        {
            if (IsWeightless(type, category))
            {
                return 0f;
            }

            return authoredWeight > 0f ? authoredWeight : SuggestedUnitWeight(type, category, cells);
        }

        /// <summary>Unit weight of <paramref name="item"/>; 0 for null (unknown ids weigh nothing).</summary>
        public static float UnitWeight(ItemDefinition item)
        {
            if (item == null)
            {
                return 0f;
            }

            GridSize size = ItemFootprint.For(item);
            return UnitWeight(item.Type, item.Category, item.Weight, size.Width * size.Height);
        }

        /// <summary>Weight of <paramref name="quantity"/> units (stacks weigh per unit).</summary>
        public static float StackWeight(ItemDefinition item, int quantity)
        {
            return quantity <= 0 ? 0f : UnitWeight(item) * quantity;
        }
    }
}
