using System;

namespace Rapadura.Gameplay.World.Cities
{
    /// <summary>
    /// "Freguesia": the standing of the cousins with one shopkeeper, earned by buying at that counter (1 point per gold
    /// spent there). Pure rules, no Unity: the online server keeps the same points per account and shop. Tiers gate
    /// counter lines (<see cref="ShopUnlockKind.Reputation"/>, e.g. the Bornal de couro at Dona Cida's), the points are
    /// saved per shop in <see cref="CityProgress"/>. See Docs/Cities.md §4.
    /// </summary>
    public static class ShopReputation
    {
        /// <summary>0 = Freguês novo (new customer).</summary>
        public const int NewCustomer = 0;

        /// <summary>1 = Freguês (regular).</summary>
        public const int Regular = 1;

        /// <summary>2 = Freguês de casa (house regular).</summary>
        public const int HouseRegular = 2;

        /// <summary>3 = Compadre (family friend).</summary>
        public const int Compadre = 3;

        /// <summary>Points needed for each tier (index = tier).</summary>
        public static readonly int[] Thresholds = { 0, 50, 150, 400 };

        public static int MaxTier => Thresholds.Length - 1;

        /// <summary>Points earned by a purchase of <paramref name="totalPrice"/> gold (never negative).</summary>
        public static int PointsFor(int totalPrice) => Math.Max(0, totalPrice);

        /// <summary>The tier reached with <paramref name="points"/>.</summary>
        public static int TierFor(int points)
        {
            int tier = 0;
            for (int i = 1; i < Thresholds.Length; i++)
            {
                if (points >= Thresholds[i])
                {
                    tier = i;
                }
            }

            return tier;
        }

        /// <summary>Points still missing for the next tier (0 at the top tier).</summary>
        public static int PointsToNextTier(int points)
        {
            int tier = TierFor(points);
            return tier >= MaxTier ? 0 : Thresholds[tier + 1] - Math.Max(0, points);
        }

        /// <summary>Localization key of a tier's name (<c>CityShopTexts</c>).</summary>
        public static string TierNameKey(int tier) => "city.reputation.tier." + Math.Max(0, Math.Min(MaxTier, tier));

        /// <summary>Adds points without overflowing.</summary>
        public static int Add(int points, int gained)
        {
            long sum = (long)Math.Max(0, points) + Math.Max(0, gained);
            return sum > int.MaxValue ? int.MaxValue : (int)sum;
        }
    }
}
