using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;

namespace Rapadura.Gameplay.PvP
{
    /// <summary>What a region allows as open (non-consented) PvP.</summary>
    public enum PvpAreaKind
    {
        /// <summary>Regions 1–4 (the starting ones): no open PvP at all, ever. Duel and arena still work (they are consented).</summary>
        Protected = 0,

        /// <summary>Regions 5–13, 15, 16: open PvP between players at <see cref="PvpAreaRules.OpenPvpMinLevel"/>+, but killing there stains the killer (<see cref="PkRules"/>).</summary>
        Frontier = 1,

        /// <summary>The "Terras de Disputa" (Região 14, Margens da Mistura; bíblia §3.11): open PvP with no stain.</summary>
        DisputeLand = 2,
    }

    /// <summary>
    /// Where open PvP is allowed (TODO.md 12.3 "áreas PvP marcadas; nada de PvP forçado nas regiões iniciais"), by region
    /// (<see cref="MapCatalog.Regions"/>) and level. Pure and deterministic, so the server answers the same.
    /// <list type="bullet">
    /// <item><b>Protected</b>: Bosque do Zarafshan, Areias Flamejantes, Serra de Kaggen, Grutas do Limiar (Regiões 1–4,
    /// monsters up to ~level 50, the whole first act and the family story) — never open PvP.</item>
    /// <item><b>Frontier</b>: from the Costa das Monções (Região 5, monsters from level 45) on, both players need level
    /// <see cref="OpenPvpMinLevel"/> or more, so a newcomer passing through is never a target and never a hunter.</item>
    /// <item><b>DisputeLand</b>: the Margens da Mistura (Região 14, levels ~300), the marked "Terras de Disputa": open PvP
    /// with no outlaw stain.</item>
    /// <item>Cities are always safe (<see cref="SafeZoneRules"/>), whatever the region.</item>
    /// </list>
    /// </summary>
    public static class PvpAreaRules
    {
        /// <summary>Both attacker and target need this level for open PvP (above the whole first act, ~level 50, and Região 5's start).</summary>
        public const int OpenPvpMinLevel = 80;

        public static PvpAreaKind AreaOf(MapRegion region)
        {
            switch (region)
            {
                case MapRegion.Zarafshan:
                case MapRegion.AreiasFlamejantes:
                case MapRegion.SerraDeKaggen:
                case MapRegion.GrutasDoLimiar:
                    return PvpAreaKind.Protected;
                case MapRegion.MargensDaMistura:
                    return PvpAreaKind.DisputeLand;
                default:
                    return PvpAreaKind.Frontier;
            }
        }

        /// <summary>The area kind of a map (<see cref="MapCatalog"/> id); an unknown map is treated as protected.</summary>
        public static PvpAreaKind AreaOfMap(string mapId)
        {
            MapDefinition map = MapCatalog.Get(mapId);
            return map == null ? PvpAreaKind.Protected : AreaOf(map.Region);
        }

        public static bool IsProtected(MapRegion region) => AreaOf(region) == PvpAreaKind.Protected;

        /// <summary>Whether a player of <paramref name="level"/> standing in <paramref name="zone"/> of <paramref name="region"/> takes part in open PvP.</summary>
        public static bool OpenPvpAllowed(MapRegion region, ZoneKind zone, int level)
        {
            return !SafeZoneRules.IsSafe(zone) && !IsProtected(region) && level >= OpenPvpMinLevel;
        }

        /// <summary>Open PvP between two players: both must qualify where each one stands.</summary>
        public static bool OpenPvpBetween(PvpCombatant attacker, PvpCombatant target)
        {
            return OpenPvpAllowed(attacker.Region, attacker.Zone, attacker.Level) && OpenPvpAllowed(target.Region, target.Zone, target.Level);
        }

        /// <summary>Guild war (declared by the guild front) counts outside cities and outside the protected regions; no level gate here.</summary>
        public static bool GuildWarAllowedAt(MapRegion region, ZoneKind zone) => !SafeZoneRules.IsSafe(zone) && !IsProtected(region);
    }
}
