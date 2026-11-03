using System;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.World.Map
{
    /// <summary>The eight regions of the "até 100" scope (Docs/Mapas100.md §5, Docs/MetodologiaMapasERacas.md Part II).</summary>
    public enum MapRegion
    {
        Zarafshan = 0,
        AreiasFlamejantes = 1,
        SerraDeKaggen = 2,
        GrutasDoLimiar = 3,
        CostaDasMoncoes = 4,
        DeltaDeJhilpur = 5,
        TetoDoMundo = 6,
        PlanicieDeMingcheng = 7,
    }

    /// <summary>Map size class (Docs/Mapas100.md §0.7): P ~120 m, M ~200 m, G ~300 m, GG ~400 m on a side.</summary>
    public enum MapSize
    {
        P = 0,
        M = 1,
        G = 2,
        GG = 3,
    }

    /// <summary>What safe ground a map has (Docs/Mapas100.md §0.1): every map has one of these.</summary>
    public enum MapSettlementKind
    {
        /// <summary>A full city (respawn, shops, stable, smith, Cais da Barca).</summary>
        City = 0,

        /// <summary>A city of one of the peoples (Docs/Mapas100.md §3): a full city too.</summary>
        RaceCity = 1,

        /// <summary>A region capital (Docs/Mapas100.md §2): a full city too.</summary>
        Capital = 2,

        /// <summary>Posto avançado: small safe zone (fire/lamp, a seller, repair), never a respawn point.</summary>
        Outpost = 3,
    }

    /// <summary>
    /// Terrain tags read by the hero bonuses of <see cref="MapAccessRules"/> (Docs/MetodologiaMapasERacas.md §3): Ícaro
    /// enters "Ruins" maps 5 levels early, Maithe "Mountain" maps. Plain strings, so the online server and data files can
    /// carry them as they are.
    /// </summary>
    public static class MapRegionTypes
    {
        public const string Forest = "Forest";
        public const string Desert = "Desert";
        public const string Mountain = "Mountain";
        public const string Ruins = "Ruins";
        public const string Underground = "Underground";
        public const string Coast = "Coast";
        public const string Swamp = "Swamp";
        public const string Plain = "Plain";
    }

    /// <summary>
    /// The special gates of a map (Docs/MetodologiaMapasERacas.md §2): an item, mount or skill the hero must have to set
    /// foot on it. Empty = no gate. Texts in <see cref="MapTexts.RequirementKey"/>.
    /// </summary>
    public static class MapRequirementTags
    {
        public const string None = "";
        public const string LeatherBoots = "LeatherBoots";
        public const string ElevatedPlatforms = "ElevatedPlatforms";
        public const string GardenKey = "GardenKey";
        public const string FreshWater = "FreshWater";
        public const string PoisonProtection = "PoisonProtection";
        public const string WoolCloak = "WoolCloak";
        public const string PickaxeTorch = "PickaxeTorch";
        public const string BlizzardResistance = "BlizzardResistance";
        public const string TorchRequired = "TorchRequired";
        public const string RunicLamp = "RunicLamp";
        public const string EternalFireOil = "EternalFireOil";
        public const string WingsOrDoubleJump = "WingsOrDoubleJump";
        public const string BoatOrDock = "BoatOrDock";
        public const string ArtificerRelic = "ArtificerRelic";
        public const string BreathingShell = "BreathingShell";
        public const string AirBubble = "AirBubble";
        public const string RootBoots = "RootBoots";
        public const string RootBridgeBalance = "RootBridgeBalance";
        public const string SaddleRequired = "SaddleRequired";
        public const string AntiVenomAmulet = "AntiVenomAmulet";
        public const string PurifyingFruit = "PurifyingFruit";
        public const string ThermalClothes = "ThermalClothes";
        public const string ClimbingCrampons = "ClimbingCrampons";
        public const string WingsRequired = "WingsRequired";
        public const string StormProtection = "StormProtection";
        public const string SunSeal = "SunSeal";

        public static readonly string[] All =
        {
            LeatherBoots, ElevatedPlatforms, GardenKey, FreshWater, PoisonProtection, WoolCloak, PickaxeTorch, BlizzardResistance,
            TorchRequired, RunicLamp, EternalFireOil, WingsOrDoubleJump, BoatOrDock, ArtificerRelic, BreathingShell, AirBubble,
            RootBoots, RootBridgeBalance, SaddleRequired, AntiVenomAmulet, PurifyingFruit, ThermalClothes, ClimbingCrampons,
            WingsRequired, StormProtection, SunSeal,
        };
    }

    /// <summary>
    /// One map of the 100 (Docs/Mapas100.md, Docs/MetodologiaMapasERacas.md Part II): its three MU-style barriers (walk
    /// level, warp level, gold fee), the special gate, the monster band, the safe settlement and the "cidade de retorno"
    /// a defeat wakes the hero in. Plain immutable C# (no Unity types), so the online server reads the very same catalog
    /// (<see cref="MapCatalog"/>); <see cref="MapDefinitionAsset"/> is an optional ScriptableObject wrapper for designers.
    /// Texts: <see cref="MapTexts"/> (keys derived from <see cref="Id"/>).
    /// </summary>
    public sealed class MapDefinition
    {
        /// <summary>Number of the map (1..100).</summary>
        public readonly int MapId;

        /// <summary>Stable id, <c>map_NNN_slug</c> (Docs/Mapas100.md "Ids").</summary>
        public readonly string Id;

        public readonly MapRegion Region;

        /// <summary>Terrain tag for the hero bonuses (<see cref="MapRegionTypes"/>).</summary>
        public readonly string RegionType;

        public readonly MapSize Size;

        /// <summary>Monster level band.</summary>
        public readonly int MonsterLevelMin;
        public readonly int MonsterLevelMax;

        /// <summary>Lowest level that may cross the border on foot.</summary>
        public readonly int WalkLevelRequirement;

        /// <summary>Lowest level the Barcas de Luz (window M) takes there; always ≥ <see cref="WalkLevelRequirement"/>.</summary>
        public readonly int WarpLevelRequirement;

        /// <summary>Base of the fare formula (<see cref="MapAccessRules.CalculateWarpCost(int,int,bool)"/>): 50, or 0 for the tutorial map.</summary>
        public readonly int BaseWarpGoldFee;

        /// <summary>Special gate (<see cref="MapRequirementTags"/>), empty = none.</summary>
        public readonly string SpecialRequirementTag;

        /// <summary>City a defeat on this map wakes the hero in (<c>CityCatalog</c> id when the city exists in the build, else a planned id).</summary>
        public readonly string ReturnCityId;

        public readonly MapSettlementKind Settlement;

        /// <summary>Slug of the settlement's name (texts <c>map.settlement.&lt;slug&gt;</c>).</summary>
        public readonly string SettlementId;

        /// <summary>
        /// Built and playable in this build: a code-built layout in <c>MapLayouts</c> or a zone of a continuous region
        /// (<c>OverworldLayouts</c>), i.e. <c>MapLayouts.IsPlayable</c>; the others are listed but closed.
        /// </summary>
        public readonly bool IsBuilt;

        public MapDefinition(int mapId, string id, MapRegion region, string regionType, MapSize size, int monsterLevelMin, int monsterLevelMax,
            int walkLevel, int warpLevel, int baseWarpGoldFee, string specialRequirementTag, string returnCityId,
            MapSettlementKind settlement, string settlementId, bool isBuilt = false)
        {
            MapId = mapId;
            Id = id;
            Region = region;
            RegionType = regionType ?? string.Empty;
            Size = size;
            MonsterLevelMin = Math.Max(1, monsterLevelMin);
            MonsterLevelMax = Math.Max(MonsterLevelMin, monsterLevelMax);
            WalkLevelRequirement = Math.Max(1, walkLevel);
            WarpLevelRequirement = Math.Max(WalkLevelRequirement, warpLevel);
            BaseWarpGoldFee = Math.Max(0, baseWarpGoldFee);
            SpecialRequirementTag = specialRequirementTag ?? string.Empty;
            ReturnCityId = returnCityId;
            Settlement = settlement;
            SettlementId = settlementId;
            IsBuilt = isBuilt;
        }

        /// <summary>Localization key of the map name (<c>map.&lt;id&gt;.name</c>).</summary>
        public string MapNameKey => "map." + Id + ".name";

        public string SettlementNameKey => "map.settlement." + SettlementId + ".name";

        public bool HasSpecialRequirement => !string.IsNullOrEmpty(SpecialRequirementTag);

        /// <summary>A city of any kind (respawn point), not an outpost.</summary>
        public bool HasCity => Settlement != MapSettlementKind.Outpost;

        /// <summary>
        /// The region of today's world map the map belongs to (weather, ambience, the map token): the four first regions
        /// are the four biomes; the later ones have no world-map node yet (null).
        /// </summary>
        public BiomeType? LegacyBiome => MapCatalog.LegacyBiomeOf(Region);

        public override string ToString() => $"{MapId:00} {Id}";
    }
}
