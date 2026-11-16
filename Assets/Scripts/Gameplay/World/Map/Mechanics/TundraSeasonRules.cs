namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>The season on the tundra of Omöl (<see cref="TundraSeasonRules.SeasonOf"/>).</summary>
    public enum TundraSeason
    {
        Summer = 0,
        Autumn = 1,
        Winter = 2,
        Spring = 3,
    }

    /// <summary>
    /// The season of the Tundra de Omöl (Docs/Mapas100.md 61: "no verão a tundra vira brejo (lento), no inverno neve (rápido)"), pure in the
    /// in-game day so the online server runs the same calendar. The tundra keeps the moon of the Porto da Lua Branca's canals
    /// (<see cref="MoonCanalRules"/>): winter is exactly the three days the canals are frozen, summer the three days round the new moon.
    /// <code>
    /// moon index 7, 0, 1: summer (bog: speed ×0.8) · 2: autumn (×1) · 3, 4, 5: winter (packed snow: speed ×1.15) · 6: spring (×1)
    /// </code>
    /// The same ground is under the stone ring of O Primeiro Desajeitado, so in summer the bog makes it harder to leave the strip he falls
    /// along and in winter easier. Decision (2026-11-16): the season follows the 8-day moon (20-minute days) instead of a real-world
    /// calendar, so a player sees all four within one evening of play; the safe ground of the Tenda de Yen is never bog.
    /// </summary>
    public static class TundraSeasonRules
    {
        public const float BogFactor = 0.8f;
        public const float SnowFactor = 1.15f;

        public static TundraSeason SeasonOf(int day)
        {
            int moon = MoonCanalRules.MoonIndex(day);
            if (MoonCanalRules.IsFrozen(day))
            {
                return TundraSeason.Winter;
            }

            if (moon == 2)
            {
                return TundraSeason.Autumn;
            }

            return moon == 6 ? TundraSeason.Spring : TundraSeason.Summer;
        }

        /// <summary>The factor on the hero's speed on the tundra in <paramref name="season"/> (1 on the post's safe ground).</summary>
        public static float SpeedFactor(TundraSeason season, bool safeGround)
        {
            if (safeGround)
            {
                return 1f;
            }

            switch (season)
            {
                case TundraSeason.Summer: return BogFactor;
                case TundraSeason.Winter: return SnowFactor;
                default: return 1f;
            }
        }

        /// <summary>In-game days until the season after <paramref name="day"/>'s begins (1 on its last day).</summary>
        public static int DaysToNextSeason(int day)
        {
            TundraSeason now = SeasonOf(day);
            for (int wait = 1; wait <= MoonCanalRules.CycleDays; wait++)
            {
                if (SeasonOf(day + wait) != now)
                {
                    return wait;
                }
            }

            return MoonCanalRules.CycleDays;
        }
    }
}
