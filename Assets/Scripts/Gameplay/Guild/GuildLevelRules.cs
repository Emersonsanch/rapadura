namespace Rapadura.Gameplay.Guild
{
    /// <summary>
    /// Guild level (1–<see cref="MaxLevel"/>) from the contribution points of all members (<see cref="GuildState.TotalPoints"/>),
    /// and the small perks it gives. No combat stat, no drop rate of rare items, nothing a bought guild could sell as
    /// power (no pay-to-win): only a little XP, a little gold found, more room in the guild and in the bank.
    /// <code>
    /// Level   1    2     3     4     5      6      7      8      9       10
    /// Points  0  1000  3000  7000  15000  30000  55000  90000  140000  200000
    /// XP      0%   0%    1%    1%    1%     2%     2%     2%     3%      3%
    /// Gold    0%   0%    0%    0%    1%     1%     1%     1%     1%      2%
    /// Members 20   25    30    35    40     45     50     55     60      65
    /// Slots   30   40    50    60    70     80     90    100    110     120
    /// </code>
    /// Points come from gold given to the guild (1 per 10, capped per member per day: <see cref="GuildBankRules.MaxGoldPointsPerDay"/>)
    /// and from what the server awards (weekly guild missions, events) through <see cref="GuildBankRules.AddContributionPoints"/>.
    /// Twenty members giving the daily cap every day reach level 10 in about 50 days; gold alone never buys it faster than the cap.
    /// </summary>
    public static class GuildLevelRules
    {
        public const int MaxLevel = 10;

        private static readonly long[] Thresholds = { 0, 1000, 3000, 7000, 15000, 30000, 55000, 90000, 140000, 200000 };

        public static int LevelFor(long points)
        {
            int level = 1;
            for (int i = 1; i < Thresholds.Length; i++)
            {
                if (points >= Thresholds[i])
                {
                    level = i + 1;
                }
            }

            return level;
        }

        /// <summary>Points needed to reach <paramref name="level"/> (1 → 0); past the max, the max's threshold.</summary>
        public static long PointsFor(int level)
        {
            if (level <= 1)
            {
                return 0;
            }

            return Thresholds[System.Math.Min(level, MaxLevel) - 1];
        }

        /// <summary>Points still missing for the next level, or 0 at <see cref="MaxLevel"/>.</summary>
        public static long PointsToNextLevel(long points)
        {
            int level = LevelFor(points);
            return level >= MaxLevel ? 0 : PointsFor(level + 1) - points;
        }

        /// <summary>Extra XP for every member, in percent: level / 3 (0 to 3%). The server adds it with the other personal multipliers.</summary>
        public static int XpBonusPercent(int level) => Clamp(level) / 3;

        /// <summary>Extra gold found, in percent: level / 5 (0 to 2%).</summary>
        public static int GoldBonusPercent(int level) => Clamp(level) / 5;

        public static int MaxMembers(int level) => 20 + 5 * (Clamp(level) - 1);

        /// <summary>Different items the bank holds (stacks of one item take one slot).</summary>
        public static int BankItemSlots(int level) => 30 + 10 * (Clamp(level) - 1);

        /// <summary>XP after the guild bonus: <paramref name="xp"/> × (1 + <see cref="XpBonusPercent"/>/100), rounded down.</summary>
        public static long ApplyXpBonus(long xp, int level) => xp <= 0 ? xp : xp + xp * XpBonusPercent(level) / 100;

        private static int Clamp(int level) => level < 1 ? 1 : level > MaxLevel ? MaxLevel : level;
    }
}
