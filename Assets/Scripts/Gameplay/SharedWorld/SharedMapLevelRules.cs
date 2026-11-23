using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Rebirth;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Overworld;

namespace Rapadura.Gameplay.SharedWorld
{
    /// <summary>How strongly the shared world warns a player stepping into a zone. Never renumber (graver = higher).</summary>
    public enum MapLevelWarning
    {
        None = 0,

        /// <summary>Below the zone's walk level (with the cousin's terrain bonus).</summary>
        BelowWalkLevel = 1,

        /// <summary>The zone's strongest monsters are <see cref="LevelDangerRules.CritDelta"/> (10)+ levels above: "Perigoso".</summary>
        StrongMonsters = 2,

        /// <summary><see cref="LevelDangerRules.LethalDelta"/> (30)+ levels above: almost a one-hit kill.</summary>
        Lethal = 3,
    }

    /// <summary>The warning for one player and one zone.</summary>
    public readonly struct MapLevelNotice
    {
        public readonly MapLevelWarning Warning;

        /// <summary>The walk level asked of this player (cousin bonus applied).</summary>
        public readonly int RequiredLevel;

        /// <summary>The level the check read (<see cref="RebirthRules.EffectiveLevel"/>).</summary>
        public readonly int EffectiveLevel;

        /// <summary>Zone's top monster level − effective level (negative = the player is above).</summary>
        public readonly int MonsterGap;

        public MapLevelNotice(MapLevelWarning warning, int requiredLevel, int effectiveLevel, int monsterGap)
        {
            Warning = warning;
            RequiredLevel = requiredLevel;
            EffectiveLevel = effectiveLevel;
            MonsterGap = monsterGap;
        }

        /// <summary>Online the level never blocks (MU style): the warning is all there is.</summary>
        public bool Blocks => false;
    }

    /// <summary>
    /// Map level in the shared world: <b>a warning, never a wall</b>, as in MU. Offline the border still stops a hero below
    /// the walk level (<see cref="MapAccessRules.CanWalkIntoMap(int,int,CharacterId,string)"/>, untouched); online anyone
    /// may walk in and is told how dangerous it is. The check reads the effective level of a reset hero
    /// (<see cref="RebirthRules.EffectiveLevel"/>: the gear of their old level goes with them) and the same danger bands
    /// as the combat (<see cref="LevelDangerRules"/>). Texts: <see cref="SharedWorldTexts.WarningKey"/>. Pure.
    /// </summary>
    public static class SharedMapLevelRules
    {
        /// <summary>The level gate of the online world blocks nothing.</summary>
        public const bool BlocksEntry = false;

        public static MapLevelNotice Check(int level, RebirthState rebirth, int walkLevel, int zoneMaxMonsterLevel, CharacterId? hero, string regionType)
        {
            int effective = RebirthRules.EffectiveLevel(rebirth, level);
            int required = MapAccessRules.EffectiveLevelRequirement(walkLevel, hero, regionType);
            int gap = LevelDangerRules.Delta(effective, zoneMaxMonsterLevel);

            MapLevelWarning warning = MapLevelWarning.None;
            if (gap >= LevelDangerRules.LethalDelta)
            {
                warning = MapLevelWarning.Lethal;
            }
            else if (gap >= LevelDangerRules.CritDelta)
            {
                warning = MapLevelWarning.StrongMonsters;
            }
            else if (effective < required)
            {
                warning = MapLevelWarning.BelowWalkLevel;
            }

            return new MapLevelNotice(warning, required, effective, gap);
        }

        /// <summary>The same for an overworld zone (its walk level, top monster level and region type).</summary>
        public static MapLevelNotice Check(int level, RebirthState rebirth, OverworldZone zone, CharacterId? hero)
        {
            if (zone == null)
            {
                int effective = RebirthRules.EffectiveLevel(rebirth, level);
                return new MapLevelNotice(MapLevelWarning.None, 1, effective, 0);
            }

            return Check(level, rebirth, zone.WalkLevel, zone.MaxLevel, hero, zone.RegionType);
        }

        /// <summary>Tell the player on entering a new zone (not every step inside it), when there is something to say.</summary>
        public static bool ShouldAnnounce(string previousZoneId, string zoneId, MapLevelNotice notice) =>
            notice.Warning != MapLevelWarning.None && zoneId != previousZoneId;
    }
}
