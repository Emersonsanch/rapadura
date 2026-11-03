using System;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>Kill-XP rank of a creature (Docs/Progressao500.md §1.2): multiplier on the common kill XP of its level.</summary>
    public enum EnemyRank
    {
        Common = 0,
        Elite = 1,
        MapBoss = 2,
        WorldBoss = 3
    }

    /// <summary>
    /// Creature level and the stat scaling that comes with it (Docs/Progressao500.md §4.1). Every creature has a level (its
    /// map's band); its health and damage come from its archetype (the campaign numbers, measured at
    /// <see cref="ArchetypeLevel"/>) times the ratio between the reference hero at its level and at level 15:
    /// <code>
    /// health(L) = archetype × Rd(L) / Rd(15)     Rd(L) = (1 + 0.02(L−1)) × (1 + 0.01(L−1))   reference hero damage
    /// damage(L) = archetype × Rh(L) / Rh(15)     Rh(L) = (1 + 0.08(L−1)) × (1 + 0.005(L−1))  reference hero toughness
    /// </code>
    /// so the time targets (commons 4–7 s, bosses 15–22 s, the boss kills a parked hero in 12–40 s) hold at any level.
    /// At or below 15 the multipliers are exactly 1: the four campaign regions (bands 3 / 7 / 10 / 15) keep today's numbers.
    /// Pure (no Unity): the encounter spawner and the future server call the same functions. Budgeted by
    /// <c>tools/balance/level_scaling.py</c> (levels 15–100).
    /// </summary>
    public static class EnemyLevelScaling
    {
        /// <summary>Level the archetype numbers (today's enemy assets) are balanced for.</summary>
        public const int ArchetypeLevel = 15;
        public const int MinLevel = 1;
        public const int MaxLevel = Rapadura.Gameplay.Player.LevelCurveMath.MaxLevel;

        /// <summary>Rd(L): the curve's damage_mult growth × attributes/gear growth of the reference hero.</summary>
        public const double HeroDamageGrowthPerLevel = 0.02;
        public const double HeroOffenseGrowthPerLevel = 0.01;
        /// <summary>Rh(L): the curve's health growth × Vitality/defence growth of the reference hero.</summary>
        public const double HeroHealthGrowthPerLevel = 0.08;
        public const double HeroToughnessGrowthPerLevel = 0.005;

        /// <summary>Band level of each campaign region (the reference hero of its boss, README §10): 3 / 7 / 10 / 15.</summary>
        public const int ForestBandLevel = 3;
        public const int DesertBandLevel = 7;
        public const int MountainBandLevel = 10;
        public const int CavesBandLevel = 15;

        /// <summary>Kill XP of a common at the same level (§1.2): campaign fit up to 50, power law after.</summary>
        public const double LowKillXpScale = 10.0;
        public const double LowKillXpExponent = 0.9;
        public const double HighKillXpAtCampaignCap = 128.0;
        public const double HighKillXpExponent = 2.4;

        public static int ClampLevel(int level) => Math.Max(MinLevel, Math.Min(MaxLevel, level));

        /// <summary>The map band of a campaign region (today: the region's boss level).</summary>
        public static int RegionBandLevel(BiomeType region)
        {
            switch (region)
            {
                case BiomeType.Forest: return ForestBandLevel;
                case BiomeType.Desert: return DesertBandLevel;
                case BiomeType.Mountain: return MountainBandLevel;
                default: return CavesBandLevel;
            }
        }

        /// <summary>The level a spawned creature fights at: its definition's own level when set (&gt; 0), else the band.</summary>
        public static int ResolveLevel(int definitionLevel, int bandLevel) =>
            ClampLevel(definitionLevel > 0 ? definitionLevel : Math.Max(MinLevel, bandLevel));

        /// <summary>Damage output of the reference hero at <paramref name="level"/> (relative units).</summary>
        public static double ReferenceHeroDamage(int level)
        {
            int steps = ClampLevel(level) - 1;
            return (1.0 + HeroDamageGrowthPerLevel * steps) * (1.0 + HeroOffenseGrowthPerLevel * steps);
        }

        /// <summary>Effective health of the reference hero at <paramref name="level"/> (relative units).</summary>
        public static double ReferenceHeroToughness(int level)
        {
            int steps = ClampLevel(level) - 1;
            return (1.0 + HeroHealthGrowthPerLevel * steps) * (1.0 + HeroToughnessGrowthPerLevel * steps);
        }

        /// <summary>Creature health multiplier at <paramref name="level"/>: 1 at or below <see cref="ArchetypeLevel"/>.</summary>
        public static float HealthMultiplier(int level) =>
            level <= ArchetypeLevel ? 1f : (float)(ReferenceHeroDamage(level) / ReferenceHeroDamage(ArchetypeLevel));

        /// <summary>Creature damage multiplier at <paramref name="level"/>: 1 at or below <see cref="ArchetypeLevel"/>.</summary>
        public static float DamageMultiplier(int level) =>
            level <= ArchetypeLevel ? 1f : (float)(ReferenceHeroToughness(level) / ReferenceHeroToughness(ArchetypeLevel));

        public static float HealthFor(float archetypeHealth, int level) => archetypeHealth * HealthMultiplier(level);

        public static float DamageFor(float archetypeDamage, int level) => archetypeDamage * DamageMultiplier(level);

        /// <summary>True when the level changes nothing (every campaign band today).</summary>
        public static bool IsIdentity(int level) => level <= ArchetypeLevel;

        /// <summary>Kill XP of a creature of <paramref name="level"/> and <paramref name="rank"/> (§1.2). Not wired yet: today's
        /// assets keep their own XP; new maps use this.</summary>
        public static int KillExperience(int level, EnemyRank rank = EnemyRank.Common)
        {
            level = ClampLevel(level);
            double common = level <= Rapadura.Gameplay.Player.LevelCurveMath.CampaignLevels
                ? LowKillXpScale * Math.Pow((level + 1) / 3.0, LowKillXpExponent)
                : HighKillXpAtCampaignCap * Math.Pow(level / (double)Rapadura.Gameplay.Player.LevelCurveMath.CampaignLevels, HighKillXpExponent);
            return (int)Math.Floor(common * RankMultiplier(rank) + 0.5);
        }

        public static int RankMultiplier(EnemyRank rank)
        {
            switch (rank)
            {
                case EnemyRank.Elite: return 3;
                case EnemyRank.MapBoss: return 9;
                case EnemyRank.WorldBoss: return 30;
                default: return 1;
            }
        }

        /// <summary>
        /// MU-style XP by level difference (§1.2): 100% up to 10 levels below (or any level above), −5% per level from 11 to
        /// 20 below (50% at 20), 10% beyond. Not wired yet (it would cut today's campaign XP for over-levelled heroes).
        /// </summary>
        public static float LevelDifferenceXpMultiplier(int heroLevel, int creatureLevel)
        {
            int below = heroLevel - creatureLevel;
            if (below <= 10)
            {
                return 1f;
            }

            return below <= 20 ? 1f - 0.05f * (below - 10) : 0.1f;
        }

        /// <summary>More than 15 levels above the hero: "Perigoso" (the creature hits ×1.25, the hero ×0.8).</summary>
        public static bool IsDangerous(int heroLevel, int creatureLevel) => creatureLevel - heroLevel > 15;
    }
}
