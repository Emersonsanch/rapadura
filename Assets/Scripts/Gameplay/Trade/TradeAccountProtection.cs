using System;

namespace Rapadura.Gameplay.Trade
{
    /// <summary>How old the account is and how far its hero went (from the server's account records).</summary>
    public readonly struct TradeAccountStanding
    {
        /// <summary>Hours since the account was created.</summary>
        public readonly double AccountAgeHours;

        /// <summary>Level of the hero who is trading.</summary>
        public readonly int HeroLevel;

        public TradeAccountStanding(double accountAgeHours, int heroLevel)
        {
            AccountAgeHours = accountAgeHours;
            HeroLevel = heroLevel;
        }

        /// <summary>An account that is never limited (old and high level); the default for callers that do not care.</summary>
        public static TradeAccountStanding Veteran => new TradeAccountStanding(1_000_000d, 1000);
    }

    /// <summary>
    /// What one account traded on one UTC day (<see cref="TradeRules.DayKey"/>): kept by the server per account, rolled
    /// over by <see cref="ForDay"/>. Item counts are lines (one stack or one piece each), like the trade window.
    /// </summary>
    [Serializable]
    public struct TradeDailyTally
    {
        public string dayKey;
        public long goldReceived;
        public long goldGiven;
        public int itemsReceived;
        public int itemsGiven;

        /// <summary>This tally when it is of <paramref name="day"/>, else a fresh one for that day.</summary>
        public TradeDailyTally ForDay(string day)
        {
            return string.Equals(dayKey, day, StringComparison.Ordinal) ? this : new TradeDailyTally { dayKey = day };
        }

        /// <summary>This tally (rolled to <paramref name="day"/>) plus one trade.</summary>
        public TradeDailyTally With(string day, long goldIn, long goldOut, int itemsIn, int itemsOut)
        {
            TradeDailyTally next = ForDay(day);
            next.goldReceived += Math.Max(0L, goldIn);
            next.goldGiven += Math.Max(0L, goldOut);
            next.itemsReceived += Math.Max(0, itemsIn);
            next.itemsGiven += Math.Max(0, itemsOut);
            return next;
        }
    }

    /// <summary>
    /// Protection of new accounts against scams and gold mules (TODO.md Fase 12.3, "limite para contas novas contra
    /// golpe"). Decided 2026-11-23: an account is <b>new</b> while it is younger than <see cref="NewAccountHours"/> hours
    /// <b>or</b> its trading hero is below level <see cref="NewAccountLevel"/>. A new account may receive and give, each
    /// on its own, up to <see cref="NewAccountGoldPerDay"/> gold and <see cref="NewAccountItemsPerDay"/> item lines per
    /// UTC day. Veterans have no daily cap (only the per-trade limits of <see cref="TradeRules"/>). Pure: the server keeps
    /// the <see cref="TradeDailyTally"/>.
    /// </summary>
    public static class TradeAccountProtection
    {
        public const double NewAccountHours = 72d;
        public const int NewAccountLevel = 30;
        public const long NewAccountGoldPerDay = 20_000L;
        public const int NewAccountItemsPerDay = 20;

        public static bool IsNewAccount(TradeAccountStanding standing)
        {
            return standing.AccountAgeHours < NewAccountHours || standing.HeroLevel < NewAccountLevel;
        }

        /// <summary>
        /// Whether one more trade (<paramref name="goldIn"/>/<paramref name="goldOut"/> gold, <paramref name="itemsIn"/>/
        /// <paramref name="itemsOut"/> lines) stays within today's limits of <paramref name="standing"/>, given what
        /// <paramref name="tally"/> (already rolled to today) holds.
        /// </summary>
        public static TradeRefusal Check(TradeAccountStanding standing, TradeDailyTally tally, long goldIn, long goldOut, int itemsIn, int itemsOut)
        {
            if (!IsNewAccount(standing))
            {
                return TradeRefusal.None;
            }

            if (tally.goldReceived + Math.Max(0L, goldIn) > NewAccountGoldPerDay || tally.goldGiven + Math.Max(0L, goldOut) > NewAccountGoldPerDay)
            {
                return TradeRefusal.NewAccountGoldLimit;
            }

            if (tally.itemsReceived + Math.Max(0, itemsIn) > NewAccountItemsPerDay || tally.itemsGiven + Math.Max(0, itemsOut) > NewAccountItemsPerDay)
            {
                return TradeRefusal.NewAccountItemLimit;
            }

            return TradeRefusal.None;
        }

        /// <summary>Gold still allowed today (receiving or giving); <see cref="long.MaxValue"/> for a veteran.</summary>
        public static long GoldLeftToday(TradeAccountStanding standing, TradeDailyTally tally, bool receiving)
        {
            if (!IsNewAccount(standing))
            {
                return long.MaxValue;
            }

            return Math.Max(0L, NewAccountGoldPerDay - (receiving ? tally.goldReceived : tally.goldGiven));
        }

        /// <summary>Item lines still allowed today (receiving or giving); <see cref="int.MaxValue"/> for a veteran.</summary>
        public static int ItemsLeftToday(TradeAccountStanding standing, TradeDailyTally tally, bool receiving)
        {
            if (!IsNewAccount(standing))
            {
                return int.MaxValue;
            }

            return Math.Max(0, NewAccountItemsPerDay - (receiving ? tally.itemsReceived : tally.itemsGiven));
        }
    }
}
