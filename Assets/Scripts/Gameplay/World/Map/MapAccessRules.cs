using System;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map
{
    /// <summary>Why the Barcas de Luz may or may not take the hero to a map, in the order the player should hear it.</summary>
    public enum MapAccessCheck
    {
        Ok = 0,

        /// <summary>Already on that map.</summary>
        Here = 1,

        /// <summary>A trip is under way / an encounter or an event holds the hero.</summary>
        Busy = 2,

        /// <summary>The map is in the catalog but not built in this version.</summary>
        NotBuilt = 3,

        /// <summary>Below the warp level (the walk level may already allow going on foot).</summary>
        LevelTooLow = 4,

        /// <summary>The special gate (wings, lamp, saddle...) is missing.</summary>
        MissingRequirement = 5,

        NotEnoughGold = 6,

        /// <summary>A dock of the continuous region the hero is in, never reached on foot (the barca only links visited docks).</summary>
        DockNotVisited = 7,
    }

    /// <summary>
    /// The monster level delta of Docs/MetodologiaMapasERacas.md §4, for one monster against the hero: +25% critical
    /// chance from +10 levels, −40% hit rate and ×1.15 damage taken from +20 rising linearly to ×1.3 (the cap) at +30.
    /// Numbers from <see cref="Rapadura.Gameplay.Combat.LevelDangerRules"/>, which the combat applies ("Perigoso", regra B).
    /// </summary>
    public readonly struct MonsterLevelPenalty
    {
        /// <summary>Monster level − hero level (negative = the hero is above).</summary>
        public readonly int Delta;

        /// <summary>Added to the monster's critical chance against the hero (0..1).</summary>
        public readonly float MonsterCritChanceBonus;

        /// <summary>Multiplier on the hero's hit rate against the monster (1 = unchanged).</summary>
        public readonly float PlayerHitRateMultiplier;

        /// <summary>Multiplier on the damage the hero takes from the monster.</summary>
        public readonly float DamageTakenMultiplier;

        public MonsterLevelPenalty(int delta, float critBonus, float hitRate, float damageTaken)
        {
            Delta = delta;
            MonsterCritChanceBonus = critBonus;
            PlayerHitRateMultiplier = hitRate;
            DamageTakenMultiplier = damageTaken;
        }

        public bool IsNone => MonsterCritChanceBonus <= 0f && PlayerHitRateMultiplier >= 1f && DamageTakenMultiplier <= 1f;

        /// <summary>+30 levels: "quase hit-kill".</summary>
        public bool IsLethal => Delta >= MapAccessRules.LethalDelta;
    }

    /// <summary>
    /// MU-Online-style map access (Docs/MetodologiaMapasERacas.md Part I), pure C# so the online server runs the same
    /// file: walk level (cross the border on foot), warp level (the Barcas de Luz, window M), the fare
    /// <c>50 + min(warpLevel, 57) × 15</c> (at most 905) doubled when the backpack is overloaded, the hero bonuses (Ícaro −5 in ruins, Maithe −5
    /// in mountains, never below 1) and the monster level delta. Failure reasons are <see cref="MapTexts"/> keys, never
    /// sentences, so the rules never localize. EditMode-tested (MapAccessRulesTests).
    /// </summary>
    public static class MapAccessRules
    {
        public const int BaseWarpFee = 50;
        public const int WarpFeePerLevel = 15;

        /// <summary>
        /// The fare follows the warp level only up to here (Docs/Balanco100a500.md §5.3, sugestão 1): gold per hour is flat
        /// (~930–1 130/h at every level), so above it the fare would cost more than an hour of farming (7 535, ~8 h, at map 100).
        /// </summary>
        public const int WarpFeeLevelCap = 57;

        /// <summary>The dearest fare without the overload: <see cref="BaseWarpFee"/> + <see cref="WarpFeeLevelCap"/> × <see cref="WarpFeePerLevel"/> = 905.</summary>
        public const int MaxWarpFee = BaseWarpFee + WarpFeeLevelCap * WarpFeePerLevel;
        public const int OverloadFareMultiplier = 2;

        /// <summary>Levels Ícaro (ruins) and Maithe (mountains) are spared.</summary>
        public const int HeroTerrainBonus = 5;

        // The monster level delta ("Perigoso", regra B) lives in Gameplay/Combat/LevelDangerRules (applied in combat).
        public const int CritDelta = Rapadura.Gameplay.Combat.LevelDangerRules.CritDelta;
        public const int HitRateDelta = Rapadura.Gameplay.Combat.LevelDangerRules.HitRateDelta;
        public const int LethalDelta = Rapadura.Gameplay.Combat.LevelDangerRules.LethalDelta;
        public const float CritChanceBonus = Rapadura.Gameplay.Combat.LevelDangerRules.CritChanceBonus;
        public const float HitRatePenalty = Rapadura.Gameplay.Combat.LevelDangerRules.HitRatePenalty;
        public const float HeavyDamageTaken = Rapadura.Gameplay.Combat.LevelDangerRules.HeavyDamageTaken;
        public const float LethalDamageTaken = Rapadura.Gameplay.Combat.LevelDangerRules.LethalDamageTaken;

        /// <summary>The level actually asked of <paramref name="hero"/> for <paramref name="requiredLevel"/> on a <paramref name="regionType"/> map.</summary>
        public static int EffectiveLevelRequirement(int requiredLevel, CharacterId? hero, string regionType)
        {
            int required = Math.Max(1, requiredLevel);
            if (hero.HasValue && HasTerrainBonus(hero.Value, regionType))
            {
                required = Math.Max(1, required - HeroTerrainBonus);
            }

            return required;
        }

        /// <summary>Ícaro decodes the old doors (ruins and engines); Maithe walks light on rough ground (mountains, rope bridges).</summary>
        public static bool HasTerrainBonus(CharacterId hero, string regionType)
        {
            return (hero == CharacterId.Icaro && regionType == MapRegionTypes.Ruins)
                || (hero == CharacterId.Maithe && regionType == MapRegionTypes.Mountain);
        }

        /// <summary>May the hero cross into the map on foot (border, bridge, portal)?</summary>
        public static bool CanWalkIntoMap(int playerLevel, int walkLevel, CharacterId heroId, string regionType)
        {
            return playerLevel >= EffectiveLevelRequirement(walkLevel, heroId, regionType);
        }

        /// <summary>As above for a catalog map and maybe no selected cousin (the map played on its own).</summary>
        public static bool CanWalkIntoMap(int playerLevel, MapDefinition map, CharacterId? heroId)
        {
            return map != null && playerLevel >= EffectiveLevelRequirement(map.WalkLevelRequirement, heroId, map.RegionType);
        }

        /// <summary>
        /// Level and gold for the Barcas de Luz. <paramref name="failureReason"/> is empty when it may go, else
        /// <see cref="MapTexts.ReasonLevel"/> or <see cref="MapTexts.ReasonGold"/>.
        /// </summary>
        public static bool CanWarpToMap(int playerLevel, int warpLevel, int playerGold, int cost, out string failureReason)
        {
            if (playerLevel < Math.Max(1, warpLevel))
            {
                failureReason = MapTexts.ReasonLevel;
                return false;
            }

            if (playerGold < Math.Max(0, cost))
            {
                failureReason = MapTexts.ReasonGold;
                return false;
            }

            failureReason = string.Empty;
            return true;
        }

        /// <summary>The fare: <see cref="BaseWarpFee"/> + min(warpLevel, <see cref="WarpFeeLevelCap"/>) × <see cref="WarpFeePerLevel"/>, doubled when overloaded.</summary>
        public static int CalculateWarpCost(int warpLevel, bool isOverloaded) => CalculateWarpCost(BaseWarpFee, warpLevel, isOverloaded);

        /// <summary>The fare with an explicit base: 0 base = the free tutorial map (no per-level part either).</summary>
        public static int CalculateWarpCost(int baseFee, int warpLevel, bool isOverloaded)
        {
            if (baseFee <= 0)
            {
                return 0;
            }

            int cost = baseFee + FareLevel(warpLevel) * WarpFeePerLevel;
            return isOverloaded ? cost * OverloadFareMultiplier : cost;
        }

        /// <summary>The warp level the fare counts: at least 1, at most <see cref="WarpFeeLevelCap"/>.</summary>
        public static int FareLevel(int warpLevel) => Math.Min(WarpFeeLevelCap, Math.Max(1, warpLevel));

        public static int CalculateWarpCost(MapDefinition map, bool isOverloaded)
        {
            return map == null ? 0 : CalculateWarpCost(map.BaseWarpGoldFee, map.WarpLevelRequirement, isOverloaded);
        }

        /// <summary>The backpack counts as overloaded above 100% of the capacity (Docs/MetodologiaMapasERacas.md §1.3).</summary>
        public static bool IsOverloaded(float loadRatio) => loadRatio > 1f;

        /// <summary>
        /// Everything that must hold before the barca leaves for <paramref name="map"/>, in order: here, busy, built, level
        /// (warp, with the hero bonus), the special gate (<paramref name="hasRequirement"/>; null = no gate is ever met), gold.
        /// </summary>
        public static MapAccessCheck CheckWarp(MapDefinition map, int playerLevel, CharacterId? hero, int gold, bool isOverloaded,
            string currentMapId, bool busy, Func<string, bool> hasRequirement)
        {
            if (map == null)
            {
                return MapAccessCheck.NotBuilt;
            }

            if (map.Id == currentMapId)
            {
                return MapAccessCheck.Here;
            }

            if (busy)
            {
                return MapAccessCheck.Busy;
            }

            if (!map.IsBuilt)
            {
                return MapAccessCheck.NotBuilt;
            }

            if (playerLevel < EffectiveLevelRequirement(map.WarpLevelRequirement, hero, map.RegionType))
            {
                return MapAccessCheck.LevelTooLow;
            }

            if (!MeetsRequirement(map.SpecialRequirementTag, hasRequirement))
            {
                return MapAccessCheck.MissingRequirement;
            }

            return gold < CalculateWarpCost(map, isOverloaded) ? MapAccessCheck.NotEnoughGold : MapAccessCheck.Ok;
        }

        /// <summary>Walking across a border: level (walk, hero bonus) and the special gate; no gold.</summary>
        public static MapAccessCheck CheckWalk(MapDefinition map, int playerLevel, CharacterId? hero, Func<string, bool> hasRequirement)
        {
            if (map == null || !map.IsBuilt)
            {
                return MapAccessCheck.NotBuilt;
            }

            if (!CanWalkIntoMap(playerLevel, map, hero))
            {
                return MapAccessCheck.LevelTooLow;
            }

            return MeetsRequirement(map.SpecialRequirementTag, hasRequirement) ? MapAccessCheck.Ok : MapAccessCheck.MissingRequirement;
        }

        public static bool MeetsRequirement(string tag, Func<string, bool> hasRequirement)
        {
            return string.IsNullOrEmpty(tag) || (hasRequirement != null && hasRequirement(tag));
        }

        /// <summary>Docs/MetodologiaMapasERacas.md §4 (<see cref="Rapadura.Gameplay.Combat.LevelDangerRules"/>): what a monster <paramref name="monsterLevel"/> does to a hero of <paramref name="playerLevel"/>.</summary>
        public static MonsterLevelPenalty GetMonsterLevelDeltaPenalty(int playerLevel, int monsterLevel)
        {
            int delta = Rapadura.Gameplay.Combat.LevelDangerRules.Delta(playerLevel, monsterLevel);
            return new MonsterLevelPenalty(delta,
                Rapadura.Gameplay.Combat.LevelDangerRules.CreatureCritChanceBonus(delta),
                Rapadura.Gameplay.Combat.LevelDangerRules.HeroHitRateMultiplier(delta),
                Rapadura.Gameplay.Combat.LevelDangerRules.CreatureDamageMultiplier(delta));
        }

        /// <summary>A level inside a map's band from a 0..1 roll (spawners): min..max inclusive.</summary>
        public static int RollMonsterLevel(int min, int max, double roll01)
        {
            int lo = Math.Max(1, Math.Min(min, max));
            int hi = Math.Max(lo, Math.Max(min, max));
            int level = lo + (int)Math.Floor(Math.Max(0d, Math.Min(0.999999d, roll01)) * (hi - lo + 1));
            return Math.Min(hi, level);
        }
    }
}
