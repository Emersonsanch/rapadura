using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Cities;
using UnityEngine;

namespace Rapadura.Gameplay.Wardrobe
{
    /// <summary>
    /// The wardrobe side of the city shops' outfit lines (<see cref="CityShopEntryKind.Outfit"/>, e.g. the 38 looks of the
    /// Costureira Dona Fátima): <see cref="CityShopService.OutfitResolver"/> asks <see cref="Voucher"/> for a counter item
    /// (a runtime <see cref="ItemDefinition"/> with the entry's id, 1x1, weightless, never saved), and the purchase is
    /// turned into the unlock by <see cref="WardrobeService"/> (the voucher leaves the bag at once). A look already in the
    /// Coleção resolves to null, so it leaves the counter; dyes are consumables and can always be bought.
    /// </summary>
    public static class WardrobeShopBridge
    {
        private static readonly Dictionary<string, ItemDefinition> Vouchers = new Dictionary<string, ItemDefinition>(StringComparer.Ordinal);

        /// <summary>Is <paramref name="itemId"/> an outfit line of <paramref name="shopId"/>'s counter?</summary>
        public static bool IsOutfitLine(string shopId, string itemId)
        {
            CityShopEntry entry = CityShopCatalog.EntryOf(shopId, itemId);
            return entry != null && entry.Kind == CityShopEntryKind.Outfit && WardrobeCatalog.Contains(itemId);
        }

        /// <summary>Should the line be sold to <paramref name="wardrobe"/> (pure part of <see cref="Voucher"/>)?</summary>
        public static bool IsSellable(string entryId, WardrobeService wardrobe)
        {
            WardrobeEntry entry = WardrobeCatalog.Get(entryId);
            if (entry == null) return false;
            return entry.IsDye || wardrobe == null || !wardrobe.Book.IsUnlocked(entryId);
        }

        public static ItemDefinition Voucher(string entryId, WardrobeService wardrobe)
        {
            if (!IsSellable(entryId, wardrobe)) return null;
            if (Vouchers.TryGetValue(entryId, out ItemDefinition cached) && cached != null) return cached;

            WardrobeEntry entry = WardrobeCatalog.Get(entryId);
            var json = new VoucherJson
            {
                _itemId = entry.Id,
                _displayName = entry.NamePtBR,
                _description = WardrobeTexts.Describe(entry, true),
                _category = (int)ItemCategory.Miscellaneous,
                _rarity = (int)entry.Rarity,
                _value = Math.Max(1, entry.Price),
                _maxStack = entry.IsDye ? 20 : 1,
                _weight = 0f,
                _gridWidth = 1,
                _gridHeight = 1,
            };

            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(json), item);
            item.name = entry.Id;
            item.hideFlags = HideFlags.DontSave;
            Vouchers[entryId] = item;
            return item;
        }

        [Serializable]
        private struct VoucherJson
        {
            public string _itemId;
            public string _displayName;
            public string _description;
            public int _category;
            public int _rarity;
            public int _value;
            public int _maxStack;
            public float _weight;
            public int _gridWidth;
            public int _gridHeight;
        }
    }
}
