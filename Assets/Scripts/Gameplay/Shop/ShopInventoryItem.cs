using Rapadura.Gameplay.Items;
using UnityEngine;

namespace Rapadura.Gameplay.Shop
{
    /// <summary>
    /// A single entry in a <see cref="ShopDefinition"/>'s catalog: which item is for sale, at what
    /// price, and how much stock the shop carries. Plain serializable class (not a ScriptableObject
    /// of its own) so it's authored inline as a list on the shop asset, mirroring how
    /// <c>RecipeIngredient</c> is authored inline on <c>RecipeDefinition</c>.
    /// </summary>
    [System.Serializable]
    public class ShopInventoryItem
    {
        [SerializeField] private ItemDefinition _item;

        /// <summary>Price override in currency units. 0 or less means "use <see cref="ItemDefinition.Value"/>".</summary>
        [SerializeField] private int _priceOverride = 0;

        /// <summary>How many units the shop has in stock. -1 means infinite (never depletes).</summary>
        [SerializeField] private int _stock = -1;

        public ItemDefinition Item => _item;
        public bool HasInfiniteStock => _stock < 0;
        public int Stock => _stock;

        /// <summary>Effective buy price: the override if set, otherwise the item's base <see cref="ItemDefinition.Value"/>.</summary>
        public int Price => _priceOverride > 0 ? _priceOverride : (_item != null ? _item.Value : 0);

        /// <summary>Reduces stock by <paramref name="quantity"/>. No-ops when infinite.</summary>
        public void ConsumeStock(int quantity)
        {
            if (HasInfiniteStock)
            {
                return;
            }

            _stock = Mathf.Max(0, _stock - quantity);
        }

        /// <summary>Increases stock by <paramref name="quantity"/> (e.g. when the player sells this item back). No-ops when infinite.</summary>
        public void RestockFromSale(int quantity)
        {
            if (HasInfiniteStock)
            {
                return;
            }

            _stock += quantity;
        }

        public bool HasStockFor(int quantity)
        {
            return HasInfiniteStock || _stock >= quantity;
        }
    }
}
