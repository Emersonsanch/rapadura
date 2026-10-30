using System;
using System.Collections.Generic;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.World.Cities
{
    /// <summary>
    /// Daily/weekly rotation of the themed shops, seeded by the calendar date (pure, no Unity; the online server can run
    /// it on its own clock and every client sees the same counter). A period is a day, or a Monday-to-Sunday week,
    /// counted from <see cref="Epoch"/>; the stock of a period is the shop's fixed entries plus
    /// <see cref="CityShopSpec.RotatingSlots"/> rotating ones drawn with a seed made of the shop id and the period
    /// (stable FNV-1a hash, never <c>string.GetHashCode</c>, which differs between runtimes). Gated entries that are not
    /// unlocked yet are left out before the draw, so unlocking something never reshuffles what was already decided for
    /// shops whose pool did not change. <see cref="EffectivePeriod"/> keeps a clock set back from re-rolling an old
    /// counter (the last period seen is saved in <see cref="CityProgress"/>).
    /// </summary>
    public static class ShopRotation
    {
        /// <summary>A Monday: week periods run Monday to Sunday.</summary>
        public static readonly DateTime Epoch = new DateTime(2026, 1, 5);

        public static long DayIndex(DateTime date)
        {
            return (long)Math.Floor((date.Date - Epoch).TotalDays);
        }

        public static long PeriodIndex(DateTime date, ShopRotationPeriod period)
        {
            long day = DayIndex(date);
            return period == ShopRotationPeriod.Weekly ? FloorDiv(day, 7) : day;
        }

        /// <summary>The period to use: the clock's, unless an already seen later one (clock set back).</summary>
        public static long EffectivePeriod(long current, long? lastSeen)
        {
            return lastSeen.HasValue && lastSeen.Value > current ? lastSeen.Value : current;
        }

        public static int Seed(string key, long period)
        {
            unchecked
            {
                uint hash = 2166136261;
                if (key != null)
                {
                    foreach (char c in key)
                    {
                        hash = (hash ^ c) * 16777619;
                    }
                }

                hash = (hash ^ (uint)(period & 0xffffffff)) * 16777619;
                hash = (hash ^ (uint)(period >> 32)) * 16777619;
                return (int)(hash & 0x7fffffff);
            }
        }

        /// <summary>Whether <paramref name="unlock"/> is met (<paramref name="isMet"/> null = everything unlocked).</summary>
        public static bool IsUnlocked(ShopUnlock unlock, Func<ShopUnlock, bool> isMet)
        {
            return !unlock.IsGated || isMet == null || isMet(unlock);
        }

        /// <summary>
        /// The counter of <paramref name="shop"/> in <paramref name="period"/>: unlocked fixed entries, then the drawn
        /// rotating ones, all in catalog order.
        /// </summary>
        public static List<CityShopEntry> PickStock(CityShopSpec shop, long period, Func<ShopUnlock, bool> isMet)
        {
            return PickStock(shop, period, isMet, null);
        }

        /// <summary>
        /// The counter of <paramref name="shop"/> in <paramref name="period"/> (the shop's own period index). Each
        /// <see cref="ShopPool"/> is drawn on its own: <see cref="ShopPool.Main"/> with the shop id and the period (the
        /// original seed), <see cref="ShopPool.House"/> with "id/house" and the period, <see cref="ShopPool.Weekly"/> with
        /// "id/week" and the week (<see cref="WeekOf"/>). <paramref name="isAvailable"/> (null = all) drops lines that
        /// cannot be sold right now (no house yet, a bag already expanded, outfits without data) before the draw, like a
        /// locked gate, so they never waste a slot. Result in catalog order.
        /// </summary>
        public static List<CityShopEntry> PickStock(CityShopSpec shop, long period, Func<ShopUnlock, bool> isMet, Func<CityShopEntry, bool> isAvailable)
        {
            var result = new List<CityShopEntry>();
            if (shop == null || shop.Entries == null)
            {
                return result;
            }

            var drawn = new HashSet<int>();
            Draw(shop, ShopPool.Main, Seed(shop.ShopId, period), isMet, isAvailable, drawn);
            Draw(shop, ShopPool.House, Seed(shop.ShopId + "/house", period), isMet, isAvailable, drawn);
            Draw(shop, ShopPool.Weekly, Seed(shop.ShopId + "/week", WeekOf(period, shop.Period)), isMet, isAvailable, drawn);

            for (int i = 0; i < shop.Entries.Count; i++)
            {
                CityShopEntry entry = shop.Entries[i];
                if (entry == null)
                {
                    continue;
                }

                bool open = entry.Rotating ? drawn.Contains(i) : IsUnlocked(entry.Unlock, isMet) && (isAvailable == null || isAvailable(entry));
                if (open)
                {
                    result.Add(entry);
                }
            }

            return result;
        }

        /// <summary>The week index of a period of <paramref name="kind"/> (a daily period is a day index).</summary>
        public static long WeekOf(long period, ShopRotationPeriod kind)
        {
            return kind == ShopRotationPeriod.Weekly ? period : FloorDiv(period, 7);
        }

        private static void Draw(CityShopSpec shop, ShopPool which, int seed, Func<ShopUnlock, bool> isMet,
            Func<CityShopEntry, bool> isAvailable, HashSet<int> drawn)
        {
            int slots = Math.Max(0, shop.SlotsOf(which));
            if (slots == 0)
            {
                return;
            }

            var pool = new List<int>();
            for (int i = 0; i < shop.Entries.Count; i++)
            {
                CityShopEntry entry = shop.Entries[i];
                if (entry != null && entry.Rotating && entry.Pool == which && IsUnlocked(entry.Unlock, isMet)
                    && (isAvailable == null || isAvailable(entry)))
                {
                    pool.Add(i);
                }
            }

            // Seeded Fisher-Yates on the pool, first N win.
            var random = new Random(seed);
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            for (int i = 0; i < Math.Min(slots, pool.Count); i++)
            {
                drawn.Add(pool[i]);
            }
        }

        /// <summary>
        /// Which city Seu Bento's mule is in on day <paramref name="dayIndex"/>: one of <paramref name="candidates"/>
        /// (the regions open on the map), drawn by date. Null without candidates.
        /// </summary>
        public static BiomeType? MascateRegion(long dayIndex, IReadOnlyList<BiomeType> candidates)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return null;
            }

            var sorted = new List<BiomeType>(candidates);
            sorted.Sort();
            return sorted[Seed(CityShopCatalog.MascateBento + "/where", dayIndex) % sorted.Count];
        }

        private static long FloorDiv(long a, long b)
        {
            long q = a / b;
            return (a % b != 0 && (a < 0) != (b < 0)) ? q - 1 : q;
        }
    }
}
