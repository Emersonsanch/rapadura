using System;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Pure numbers of the hero level curve up to <see cref="MaxLevel"/> (Docs/Progressao500.md §1–2). No Unity: the
    /// future server (FASE 12) validates levels and points with these same calls.
    /// <list type="bullet">
    /// <item>Levels 1..<see cref="CampaignLevels"/>: <c>round(100 × 1.10^(n−1))</c> — unchanged, the designer table
    /// (<c>Docs/Balance/progression.csv</c> / <see cref="ProgressionCurve"/>) is the source of truth there.</item>
    /// <item>Above: <c>round(xp(50) × (n/50)³ × W(n))</c>, <c>W = 1</c> up to <see cref="WallStartLevel"/> and
    /// <c>1 + ((n − 300)/200)²</c> after (the late "wall": 400 = ×1.25, 500 = ×2).</item>
    /// </list>
    /// XP inside one level always fits an <c>int</c> (max 21 344 000 at level 500); anything summed over levels
    /// (total XP, rankings) must be <c>long</c> — the total to reach 500 is 1 853 225 538.
    /// </summary>
    public static class LevelCurveMath
    {
        /// <summary>Hero level cap ("teto"). At this level the bar fills once and then stays full (or feeds the master bar).</summary>
        public const int MaxLevel = 500;

        /// <summary>Last level of the campaign curve (exponential, designer table). The cubic starts after it.</summary>
        public const int CampaignLevels = 50;

        /// <summary>Content scope being built now ("até 100", Docs/Progressao500.md §5): the budgets in tools/balance cover 1..this.</summary>
        public const int ContentScopeLevel = 100;

        /// <summary>From this level the late wall <c>W(n)</c> multiplies the cubic.</summary>
        public const int WallStartLevel = 300;

        /// <summary>Width of the wall: at <see cref="WallStartLevel"/> + this, <c>W = 2</c>.</summary>
        public const double WallWidth = 200.0;

        /// <summary>Shipped curve parameters (Resources/Balance/ProgressionCurve.asset), for server-side use without the asset.</summary>
        public const int ShippedBaseExperience = 100;
        public const double ShippedExperienceGrowth = 1.10;

        /// <summary>Above <see cref="CampaignLevels"/> one skill point every this many levels (55, 60, …): skills cap at 5 well before 50.</summary>
        public const int HighLevelSkillPointEvery = 5;

        public static int ClampLevel(int level) => Math.Max(1, Math.Min(MaxLevel, level));

        /// <summary>The late wall factor <c>W(n)</c>: 1 up to <see cref="WallStartLevel"/>, then <c>1 + ((n − 300)/200)²</c>.</summary>
        public static double Wall(int level)
        {
            if (level <= WallStartLevel)
            {
                return 1.0;
            }

            double d = (level - WallStartLevel) / WallWidth;
            return 1.0 + d * d;
        }

        /// <summary>
        /// XP from <paramref name="level"/> to the next for levels above <see cref="CampaignLevels"/>, anchored on the XP of
        /// level 50 (<paramref name="anchorExperience"/>; 10 672 with the shipped curve). Levels at or below 50 return the
        /// anchor (callers use the table there). Clamped to <c>int.MaxValue</c> for absurd anchors.
        /// </summary>
        public static int HighLevelExperienceToNext(int level, int anchorExperience)
        {
            if (level <= CampaignLevels)
            {
                return Math.Max(1, anchorExperience);
            }

            double ratio = Math.Min(level, MaxLevel) / (double)CampaignLevels;
            double value = Math.Max(1, anchorExperience) * (ratio * ratio * ratio) * Wall(Math.Min(level, MaxLevel));
            return (int)Math.Min(int.MaxValue, Math.Floor(value + 0.5));
        }

        /// <summary>The exponential part of the formula, <c>round(base × growth^(n−1))</c> (double precision; equals the shipped table for 1..50).</summary>
        public static int ExponentialExperienceToNext(int level, int baseExperience, double growth)
        {
            double value = baseExperience * Math.Pow(growth, Math.Max(1, level) - 1);
            return (int)Math.Min(int.MaxValue, Math.Floor(value + 0.5));
        }

        /// <summary>The shipped curve, whole range (exponential up to 50, cubic + wall after). Server-side twin of the asset.</summary>
        public static int ShippedExperienceToNext(int level)
        {
            level = ClampLevel(level);
            int anchor = ExponentialExperienceToNext(CampaignLevels, ShippedBaseExperience, ShippedExperienceGrowth);
            return level <= CampaignLevels
                ? ExponentialExperienceToNext(level, ShippedBaseExperience, ShippedExperienceGrowth)
                : HighLevelExperienceToNext(level, anchor);
        }

        /// <summary>Total XP to reach <paramref name="level"/> from level 1 with 0 XP on the shipped curve (long: passes 1.8 billion).</summary>
        public static long ShippedTotalExperienceToReach(int level)
        {
            long total = 0;
            int target = ClampLevel(level);
            for (int l = 1; l < target; l++)
            {
                total += ShippedExperienceToNext(l);
            }

            return total;
        }

        /// <summary>Attribute points granted when REACHING <paramref name="level"/>: the same every level (2), up to the cap.</summary>
        public static int AttributePointsForReaching(int level, int perLevel) => level <= 1 || level > MaxLevel ? 0 : perLevel;

        /// <summary>Skill points granted when REACHING <paramref name="level"/>: <paramref name="perLevel"/> up to 50, then 1 every 5 levels.</summary>
        public static int SkillPointsForReaching(int level, int perLevel)
        {
            if (level <= 1 || level > MaxLevel)
            {
                return 0;
            }

            if (level <= CampaignLevels)
            {
                return perLevel;
            }

            return level % HighLevelSkillPointEvery == 0 ? 1 : 0;
        }
    }
}
