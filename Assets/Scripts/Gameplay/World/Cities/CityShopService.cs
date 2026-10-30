using System;
using System.Collections.Generic;
using System.Text;
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
            long current = ShopRotation.PeriodIndex(date, spec.Period);
            bool advanced = false;
            period = cities != null ? cities.ResolveShopPeriod(spec.ShopId, current, out advanced) : current;
            if (advanced)
            {
                shops?.RestockShop(spec.ShopId);
            }

            return ShopRotation.PickStock(spec, period, isMet);
        }

        /// <summary>The definition to open for <paramref name="shopId"/> right now, or null when it cannot be built.</summary>
        public static ShopDefinition Resolve(string shopId, CityProgress cities, ShopManager shops, WorldMapProgress map)
        {
            CityShopSpec spec = CityShopCatalog.Get(shopId);
            if (spec == null)
            {
                return string.IsNullOrEmpty(shopId) ? null : Resources.Load<ShopDefinition>(CityShopCatalog.AssetFolder + shopId);
            }

            List<CityShopEntry> counter = CounterFor(spec, Now, cities, shops, UnlockCheck(map), out long period);
            var signature = new StringBuilder().Append(period);
            var catalog = new List<ShopInventoryItem>(counter.Count);
            foreach (CityShopEntry entry in counter)
            {
                ItemDefinition item = ItemDatabase.GetById(entry.ItemId);
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
