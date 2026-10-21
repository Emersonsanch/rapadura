using Rapadura.Gameplay.Items;
using UnityEngine;

namespace Rapadura.Gameplay.Shop
{
    /// <summary>
    /// A single entry in a <see cref="ShopDefinition"/>'s catalog: which item is for sale, at what
    /// price, and how much stock the shop starts with. Plain serializable class (not a ScriptableObject
    /// of its own) so it's authored inline as a list on the shop asset, mirroring how
    /// <c>RecipeIngredient</c> is authored inline on <c>RecipeDefinition</c>.
    ///
    /// Read-only at runtime: the stock the player actually sees lives in <see cref="ShopManager"/>'s
    /// per-session stock table (saved under "Shops"). This class used to decrement its own serialized
    /// field on purchase, which in the Editor permanently rewrote the shop asset on disk and in builds
    /// leaked depleted stock across saves for the whole app session.
    /// </summary>
    [System.Serializable]
    public class ShopInventoryItem
    {
        [SerializeField] private ItemDefinition _item;

        /// <summary>Price override in currency units. 0 or less means "use <see cref="ItemDefinition.Value"/>".</summary>
        [SerializeField] private int _priceOverride = 0;

        /// <summary>Authored starting / restock amount. -1 means infinite (never depletes).</summary>
        [SerializeField] private int _stock = -1;

        public ItemDefinition Item => _item;
        public bool HasInfiniteStock => _stock < 0;

        /// <summary>Authored stock (what a new game starts with and what a restock refills to). Never changes at runtime — ask <see cref="ShopManager.GetStock"/> for the live value.</summary>
        public int InitialStock => _stock;

        /// <summary>Effective buy price: the override if set, otherwise the item's base <see cref="ItemDefinition.Value"/>.</summary>
        public int Price => _priceOverride > 0 ? _priceOverride : (_item != null ? _item.Value : 0);
    }
}
