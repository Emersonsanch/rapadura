using System;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.World.Cities
{
    /// <summary>What kind of ground an actor stands on, as far as fighting goes.</summary>
    public enum ZoneKind
    {
        /// <summary>Roads, wilds, arenas: normal combat.</summary>
        Wilds = 0,

        /// <summary>Inside a city: nobody fights (decision of 2026-10-29, TODO 12.3a).</summary>
        City = 1,
    }

    /// <summary>Who is acting, for <see cref="SafeZoneRules"/>.</summary>
    public enum ZoneActor
    {
        Player = 0,
        Monster = 1,

        /// <summary>Residents, shopkeepers: never attack and are never attacked.</summary>
        Npc = 2,
    }

    /// <summary>
    /// The "zona segura" rules of a city, as plain C# with no Unity dependency (only the <see cref="BiomeType"/> enum),
    /// so the online server can compile and run the very same file. Decision of the user (TODO.md 12.3a): every region
    /// has a city and inside a city there is no combat at all — monsters neither attack nor come in (they stop at the
    /// border and turn back), players attack neither monsters, nor other players, nor themselves (damaging skills are
    /// off; walking, talking, trading, shopping and the house stay on).
    /// <para>
    /// Offline today (world map, <c>WorldMapCityPresence</c>): the city of a region is the circle of
    /// <see cref="MapCityRadius"/> around its map node; while the hero token is inside it no random encounter
    /// (<see cref="CanStartRandomEncounter"/>) and no road event (<see cref="CanTriggerRoadEvent"/>) starts, the HUD
    /// says "Você está em zona segura", and every defeat sends the hero back to the plaza of the city of the region it
    /// happened in (<see cref="RespawnRegion"/>, Docs/Progressao500.md §7). The encounter arena (Main.unity) has no city: everything there is
    /// <see cref="ZoneKind.Wilds"/>, where every rule answers exactly what combat already does, so nothing changes.
    /// </para>
    /// <para>
    /// Walkable 3D cities (Docs/Cities.md): each city gets a trigger volume; the zone of an actor is read from it.
    /// Damage code asks <see cref="CanAttack"/> before a hit and <see cref="CanTakeDamage"/> before applying it,
    /// skill casting asks <see cref="CanUseDamagingSkill"/>, enemy AI asks <see cref="CanEnemyPursueInto"/> for the next
    /// step of its path and drops its target when <see cref="ShouldDropTarget"/> says so, and entering a city clears
    /// debuffs (<see cref="ShouldClearDebuffsOnEntry"/>). <see cref="CanEnterCity"/> is the hook for the anti-abuse
    /// "PZ lock" once PvP exists.
    /// </para>
    /// </summary>
    public static class SafeZoneRules
    {
        /// <summary>Radius of a city around its world-map node, in map units (= the flat plaza the dressing carves around each node).</summary>
        public const float MapCityRadius = 1.5f;

        /// <summary>Seconds after attacking another player during which the attacker cannot step into a city (online, PvP only).</summary>
        public const float PlayerCombatTagSeconds = 10f;

        public static bool IsSafe(ZoneKind zone) => zone == ZoneKind.City;

        /// <summary>City when within <paramref name="cityRadius"/> of the city's centre (inclusive), wilds otherwise.</summary>
        public static ZoneKind ZoneAtDistance(float distanceToCityCentre, float cityRadius)
        {
            return distanceToCityCentre >= 0f && distanceToCityCentre <= cityRadius ? ZoneKind.City : ZoneKind.Wilds;
        }

        /// <summary>
        /// Whether <paramref name="attacker"/> may hit <paramref name="target"/>. Never when either one is in a city, never
        /// for or against an NPC, never monster against monster. Player against another player only when
        /// <paramref name="pvpAllowed"/> (PvP areas / consented duels, online). A player hurting themself (self-damaging
        /// skill costs) is allowed in the wilds only.
        /// </summary>
        public static bool CanAttack(ZoneActor attacker, ZoneKind attackerZone, ZoneActor target, ZoneKind targetZone,
            bool targetIsSelf = false, bool pvpAllowed = false)
        {
            if (attacker == ZoneActor.Npc || target == ZoneActor.Npc)
            {
                return false;
            }

            if (IsSafe(attackerZone) || IsSafe(targetZone))
            {
                return false;
            }

            if (targetIsSelf)
            {
                return attacker == ZoneActor.Player;
            }

            if (attacker == ZoneActor.Monster)
            {
                return target == ZoneActor.Player;
            }

            return target == ZoneActor.Monster || pvpAllowed;
        }

        /// <summary>Whether damage may be applied to <paramref name="actor"/> standing in <paramref name="zone"/> (no source can hurt anyone in a city).</summary>
        public static bool CanTakeDamage(ZoneActor actor, ZoneKind zone)
        {
            return actor != ZoneActor.Npc && !IsSafe(zone);
        }

        /// <summary>Damaging skills and basic attacks are disabled in a city; movement and non-damaging actions are not.</summary>
        public static bool CanUseDamagingSkill(ZoneKind zone) => !IsSafe(zone);

        /// <summary>Whether a monster may step into <paramref name="destination"/> (chasing or wandering). Monsters stop at the city border.</summary>
        public static bool CanEnemyPursueInto(ZoneKind destination) => !IsSafe(destination);

        /// <summary>A monster whose target is now in a city drops it (and walks back), so a city is never a place to tank from.</summary>
        public static bool ShouldDropTarget(ZoneKind targetZone) => IsSafe(targetZone);

        /// <summary>Random encounters are rolled only outside cities.</summary>
        public static bool CanStartRandomEncounter(ZoneKind heroZone) => !IsSafe(heroZone);

        /// <summary>Road events (world-map cards, ambushes) only happen outside cities.</summary>
        public static bool CanTriggerRoadEvent(ZoneKind heroZone) => !IsSafe(heroZone);

        /// <summary>Debuffs (poison, slow, stun) stop when stepping from the wilds into a city.</summary>
        public static bool ShouldClearDebuffsOnEntry(ZoneKind from, ZoneKind to) => !IsSafe(from) && IsSafe(to);

        /// <summary>
        /// Tibia-style "PZ lock" for PvP: a player who attacked another player in the last
        /// <see cref="PlayerCombatTagSeconds"/> cannot step into a city to hide. Fighting monsters never locks it
        /// (they simply lose the target at the border). Offline there is no PvP, so this is always true.
        /// </summary>
        public static bool CanEnterCity(double secondsSinceAttackingAPlayer)
        {
            return secondsSinceAttackingAPlayer < 0d || secondsSinceAttackingAPlayer >= PlayerCombatTagSeconds;
        }

        /// <summary>
        /// Where a defeated hero wakes up while the world is the 4 regions of today (Docs/Progressao500.md §7): the city of
        /// the region the defeat happened in, or <paramref name="fallback"/> (the starting region) when that region has no
        /// city. Every defeat goes through this (arena, event room, walkable city, open map): a hero never revives where
        /// they fell nor at an arena checkpoint, and an outpost (safe zone without a city) is never a respawn point.
        /// </summary>
        public static BiomeType RespawnRegion(BiomeType defeatRegion, Func<BiomeType, bool> hasCity, BiomeType fallback)
        {
            return hasCity == null || hasCity(defeatRegion) ? defeatRegion : fallback;
        }

        /// <summary>
        /// The general form for the 100 maps (Docs/Mapas100.md): each map names its "cidade de retorno"
        /// (<paramref name="returnCityOf"/>, map id → city id); a map without one (or an unknown map) wakes the hero in
        /// <paramref name="fallbackCity"/> (the starting city). Pure, so the online server picks the very same city.
        /// </summary>
        public static string RespawnCity(string mapId, Func<string, string> returnCityOf, string fallbackCity)
        {
            string city = string.IsNullOrEmpty(mapId) || returnCityOf == null ? null : returnCityOf(mapId);
            return string.IsNullOrEmpty(city) ? fallbackCity : city;
        }

        /// <summary>Only cities are respawn points: an outpost (safe zone without a city: rest, sell, repair) never is.</summary>
        public static bool IsRespawnPoint(bool isCity, bool isOutpost) => isCity && !isOutpost;
    }
}
