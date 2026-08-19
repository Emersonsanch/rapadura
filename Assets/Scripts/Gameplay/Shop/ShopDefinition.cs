using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Shop
{
    /// <summary>
    /// Data-driven definition of a single NPC shop: an id, a localized display name, its catalog
    /// of <see cref="ShopInventoryItem"/> entries, and the rate the shop pays the player when
    /// buying items back. Authored as a ScriptableObject asset, same "data as asset" pattern as
    /// <c>DialogueDefinition</c>/<c>RecipeDefinition</c>/<c>ItemDefinition</c> elsewhere in this
    /// project, so designers can build shops entirely in the Inspector.
    ///
    /// Currency: this project has no dedicated "Currency"/gold concept in
    /// <c>InventoryManager</c>/<c>ItemDefinition</c> — money is just an item like any other,
    /// tracked as inventory stock. <see cref="ResourceItemSeeder"/> already seeds
    /// <c>item_gold</c> (ItemCategory.Gold) as the game's precious-metal resource, and it doubles
    /// as currency here (its <c>ItemDefinition.Value</c> is the gold-per-unit baseline other item
    /// prices are expressed in). <c>ItemCategory.Currency</c> exists on the enum but no item uses
    /// it yet, so <see cref="CurrencyItemId"/> defaults to "item_gold" and is overridable per-shop
    /// in case a future item adopts ItemCategory.Currency instead.
    /// </summary>
    [CreateAssetMenu(fileName = "NewShop", menuName = "Rapadura/Shop/Shop Definition", order = 0)]
    public class ShopDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _shopId = "shop_new";
        [SerializeField] private string _displayNameKey = "shop.new.name";

        [Header("Economy")]
        [Tooltip("Item id used as currency for this shop. Defaults to item_gold (see ResourceItemSeeder).")]
        [SerializeField] private string _currencyItemId = "item_gold";
        [Tooltip("Fraction of an item's sell price the shop pays the player when they sell it back (e.g. 0.5 = 50%).")]
        [Range(0f, 1f)]
        [SerializeField] private float _sellBackRate = 0.5f;

        [Header("Catalog")]
        [SerializeField] private List<ShopInventoryItem> _catalog = new List<ShopInventoryItem>();

        public string ShopId => _shopId;
        public string DisplayNameKey => _displayNameKey;
        public string CurrencyItemId => _currencyItemId;
        public float SellBackRate => _sellBackRate;
        public IReadOnlyList<ShopInventoryItem> Catalog => _catalog;
    }
}
