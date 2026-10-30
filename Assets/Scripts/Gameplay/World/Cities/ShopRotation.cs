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
            var result = new List<CityShopEntry>();
            if (shop == null || shop.Entries == null)
            {
                return result;
            }

            var pool = new List<int>();
            for (int i = 0; i < shop.Entries.Count; i++)
            {
                CityShopEntry entry = shop.Entries[i];
                if (entry != null && entry.Rotating && IsUnlocked(entry.Unlock, isMet))
                {
                    pool.Add(i);
                }
            }

            // Seeded Fisher-Yates on the pool, first N win.
            var random = new Random(Seed(shop.ShopId, period));
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            var drawn = new HashSet<int>();
            for (int i = 0; i < Math.Min(Math.Max(0, shop.RotatingSlots), pool.Count); i++)
            {
                drawn.Add(pool[i]);
            }

            for (int i = 0; i < shop.Entries.Count; i++)
            {
                CityShopEntry entry = shop.Entries[i];
                if (entry == null)
                {
                    continue;
                }

                if (entry.Rotating ? drawn.Contains(i) : IsUnlocked(entry.Unlock, isMet))
                {
                    result.Add(entry);
                }
            }

            return result;
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
