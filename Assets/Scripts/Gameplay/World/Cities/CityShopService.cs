using System;
using System.Collections.Generic;
using System.Text;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Gameplay.World.Cities
{
    /// <summary>
    /// Turns a city shop id into the <see cref="ShopDefinition"/> the existing <see cref="ShopManager"/>/ShopUIController
    /// work with: a themed shop (<see cref="CityShopCatalog"/>) becomes a runtime definition holding today's counter
    /// (<see cref="ShopRotation.PickStock"/>, campaign gates read from <see cref="WorldMapProgress"/>); any other id is
    /// loaded from <c>Resources/Shops</c> (the old general store). When a shop's period moves on
    /// (<see cref="CityProgress.ResolveShopPeriod"/>), its live stock in <see cref="ShopManager"/> is refilled
    /// (<see cref="ShopManager.RestockShop"/>), so sold-out lines come back with the new counter. Runtime definitions
    /// are cached per shop and counter, and replaced (destroyed) when the counter changes.
    /// </summary>
    public static class CityShopService
    {
        private static readonly Dictionary<string, (string Signature, ShopDefinition Shop)> Cache =
            new Dictionary<string, (string, ShopDefinition)>(StringComparer.Ordinal);

        /// <summary>Clock used for the rotation (tests override it; null = <see cref="DateTime.Now"/>).</summary>
        public static Func<DateTime> ClockOverride { get; set; }

        public static DateTime Now => ClockOverride != null ? ClockOverride() : DateTime.Now;

        /// <summary>
        /// The wardrobe's item for a <see cref="CityShopEntryKind.Outfit"/> line (CatalogoRoupas.md id), or null while it
        /// has no data: outfit lines then stay off the counter. Set by the wardrobe when it exists.
        /// </summary>
        public static Func<string, ItemDefinition> OutfitResolver { get; set; }

        /// <summary>Whether the cousins have the house (house items go to its depot). Null = <see cref="HouseService.ResolveShared"/>.</summary>
        public static Func<bool> HasHouseOverride { get; set; }

        private static readonly Dictionary<string, ItemDefinition> HouseVouchers = new Dictionary<string, ItemDefinition>(StringComparer.Ordinal);

        /// <summary>Campaign gate check against <paramref name="map"/> (null map = everything unlocked).</summary>
        public static Func<ShopUnlock, bool> UnlockCheck(WorldMapProgress map)
        {
            if (map == null)
            {
                return null;
            }

            return unlock =>
            {
                switch (unlock.Kind)
                {
                    case ShopUnlockKind.RegionUnlocked: return map.IsUnlocked(unlock.Region);
                    case ShopUnlockKind.RegionCleared: return map.IsRegionCleared(unlock.Region);
                    default: return true;
                }
            };
        }

        /// <summary>
        /// Every gate of <paramref name="shopId"/>'s lines: campaign (<see cref="UnlockCheck(WorldMapProgress)"/>, null map =
        /// open) and the shop's freguesia tier in <paramref name="cities"/> (null = tier 0).
        /// </summary>
        public static Func<ShopUnlock, bool> GateCheck(WorldMapProgress map, CityProgress cities, string shopId)
        {
            Func<ShopUnlock, bool> campaign = UnlockCheck(map);
            return unlock =>
            {
                if (unlock.Kind == ShopUnlockKind.Reputation)
                {
                    return (cities != null ? cities.ReputationTier(shopId) : 0) >= unlock.Level;
                }

                return campaign == null || campaign(unlock);
            };
        }

        /// <summary>
        /// Whether a line can be sold right now (pure, given the facts): house items need the house, outfits need wardrobe
        /// data, the Bornal de couro is sold once.
        /// </summary>
        public static bool IsAvailable(CityShopEntry entry, bool hasHouse, bool canBuyBornal, Func<string, bool> outfitExists)
        {
            if (entry == null)
            {
                return false;
            }

            if (entry.ItemId == CityShopCatalog.BornalItemId && !canBuyBornal)
            {
                return false;
            }

            switch (entry.Kind)
            {
                case CityShopEntryKind.HouseItem: return hasHouse;
                case CityShopEntryKind.Outfit: return outfitExists != null && outfitExists(entry.ItemId);
                default: return true;
            }
        }

        /// <summary><see cref="IsAvailable"/> against the live house, <paramref name="cities"/> and <see cref="OutfitResolver"/>.</summary>
        public static Func<CityShopEntry, bool> AvailabilityCheck(CityProgress cities)
        {
            bool hasHouse = HasHouseOverride != null ? HasHouseOverride() : HouseService.ResolveShared().HasHouse;
            bool canBuyBornal = cities == null || cities.CanBuyBornal;
            Func<string, ItemDefinition> outfits = OutfitResolver;
            Func<string, bool> outfitExists = null;
            if (outfits != null)
            {
                outfitExists = id => outfits(id) != null;
            }

            return entry => IsAvailable(entry, hasHouse, canBuyBornal, outfitExists);
        }

        /// <summary>The item a counter line hands to <see cref="ShopManager"/> (a bag item, a house-item voucher or an outfit), or null.</summary>
        public static ItemDefinition ResolveItem(CityShopEntry entry)
        {
            if (entry == null)
            {
                return null;
            }

            switch (entry.Kind)
            {
                case CityShopEntryKind.HouseItem: return HouseVoucher(entry.ItemId);
                case CityShopEntryKind.Outfit: return OutfitResolver?.Invoke(entry.ItemId);
                default: return ItemDatabase.GetById(entry.ItemId);
            }
        }

        /// <summary>
        /// A runtime <see cref="ItemDefinition"/> standing for house item <paramref name="houseItemId"/> on a counter (same
        /// id, 1x1, not stackable): the shop UI and <see cref="ShopManager"/> need an item, and the purchase moves it on to
        /// the house depot (<see cref="CityShopPurchases"/>). Null for an unknown id. Cached; never saved.
        /// </summary>
        public static ItemDefinition HouseVoucher(string houseItemId)
        {
            HouseItemDefinition house = HouseCatalog.Get(houseItemId);
            if (house == null)
            {
                return null;
            }

            if (HouseVouchers.TryGetValue(houseItemId, out ItemDefinition cached) && cached != null)
            {
                return cached;
            }

            var json = new VoucherJson
            {
                _itemId = house.ItemId,
                _displayName = house.NamePtBR,
                _description = HouseItemTexts.Describe(house, true),
                _category = (int)ItemCategory.Miscellaneous,
                _rarity = (int)house.Rarity,
                _value = house.BasePrice,
                _maxStack = 1,
                _weight = 0f,
                _gridWidth = 1,
                _gridHeight = 1,
            };

            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(json), item);
            item.name = house.ItemId;
            item.hideFlags = HideFlags.DontSave;
            HouseVouchers[houseItemId] = item;
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

        /// <summary>Regions where Seu Bento can be today: the ones open on the map (all, without a map).</summary>
        public static List<BiomeType> MascateCandidates(WorldMapProgress map)
        {
            var candidates = new List<BiomeType>();
            foreach (CitySpec city in CityCatalog.Cities)
            {
                if (map == null || map.IsUnlocked(city.Region))
                {
                    candidates.Add(city.Region);
                }
            }

            return candidates;
        }

        /// <summary>Region of the peddler on <paramref name="date"/>.</summary>
        public static BiomeType? MascateRegion(DateTime date, WorldMapProgress map)
        {
            return ShopRotation.MascateRegion(ShopRotation.DayIndex(date), MascateCandidates(map));
        }

        /// <summary>The shops with a counter in <paramref name="city"/> today: its own, plus the peddler when his mule is there.</summary>
        public static List<string> ShopsOpenIn(CitySpec city, DateTime date, WorldMapProgress map)
        {
            var shops = new List<string>();
            if (city == null)
            {
                return shops;
            }

            shops.AddRange(city.ShopIds);
            if (MascateRegion(date, map) == city.Region)
            {
                shops.Add(CityShopCatalog.MascateBento);
            }

            return shops;
        }

        /// <summary>Today's counter of a themed shop (entries only; no Unity objects), recording the period in <paramref name="cities"/>.</summary>
        public static List<CityShopEntry> CounterFor(CityShopSpec spec, DateTime date, CityProgress cities, ShopManager shops,
            Func<ShopUnlock, bool> isMet, out long period)
        {
            return CounterFor(spec, date, cities, shops, isMet, null, out period);
        }

        /// <summary>Today's counter, leaving out lines <paramref name="isAvailable"/> refuses (null = all) before the draw.</summary>
        public static List<CityShopEntry> CounterFor(CityShopSpec spec, DateTime date, CityProgress cities, ShopManager shops,
            Func<ShopUnlock, bool> isMet, Func<CityShopEntry, bool> isAvailable, out long period)
        {
            long current = ShopRotation.PeriodIndex(date, spec.Period);
            bool advanced = false;
            period = cities != null ? cities.ResolveShopPeriod(spec.ShopId, current, out advanced) : current;
            if (advanced)
            {
                shops?.RestockShop(spec.ShopId);
            }

            return ShopRotation.PickStock(spec, period, isMet, isAvailable);
        }

        /// <summary>The definition to open for <paramref name="shopId"/> right now, or null when it cannot be built.</summary>
        public static ShopDefinition Resolve(string shopId, CityProgress cities, ShopManager shops, WorldMapProgress map)
        {
            CityShopSpec spec = CityShopCatalog.Get(shopId);
            if (spec == null)
            {
                return string.IsNullOrEmpty(shopId) ? null : Resources.Load<ShopDefinition>(CityShopCatalog.AssetFolder + shopId);
            }

            List<CityShopEntry> counter = CounterFor(spec, Now, cities, shops, GateCheck(map, cities, shopId), AvailabilityCheck(cities), out long period);
            var signature = new StringBuilder().Append(period);
            var catalog = new List<ShopInventoryItem>(counter.Count);
            foreach (CityShopEntry entry in counter)
            {
                ItemDefinition item = ResolveItem(entry);
                if (item == null)
                {
                    continue;
                }

                catalog.Add(new ShopInventoryItem(item, entry.Price, entry.Stock));
                signature.Append('|').Append(entry.ItemId);
            }

            string key = signature.ToString();
            if (Cache.TryGetValue(shopId, out (string Signature, ShopDefinition Shop) cached) && cached.Shop != null && cached.Signature == key)
            {
                return cached.Shop;
            }

            // The old counter may still be open in ShopManager; let the caller close it first (it replaces the shop).
            if (cached.Shop != null && (shops == null || shops.ActiveShop != cached.Shop))
            {
                DestroyShop(cached.Shop);
            }

            ShopDefinition shop = ShopDefinition.CreateRuntime(shopId, spec.NameKey, catalog, spec.SellBackRate);
            Cache[shopId] = (key, shop);
            return shop;
        }

        /// <summary>Drops every cached runtime shop (tests).</summary>
        public static void ClearCache()
        {
            foreach ((string _, ShopDefinition shop) in Cache.Values)
            {
                DestroyShop(shop);
            }

            Cache.Clear();
        }

        private static void DestroyShop(ShopDefinition shop)
        {
            if (shop == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(shop);
            }
            else
            {
                Object.DestroyImmediate(shop);
            }
        }
    }
}
