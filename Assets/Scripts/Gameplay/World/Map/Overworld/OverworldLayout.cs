using System.Collections.Generic;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>The kind of ground of an overworld zone: ground colours, vegetation and scatter density (<see cref="OverworldTerrains"/>).</summary>
    public enum OverworldTerrain
    {
        /// <summary>Apricot and pomegranate rows around the Vila.</summary>
        Orchard = 0,
        /// <summary>Patchwork of melon, wheat and fallow plots along the aryk canals.</summary>
        Fields = 1,
        /// <summary>Dry steppe towards the Areias Flamejantes: straw grass, sand, tamarisks.</summary>
        Steppe = 2,
        /// <summary>Open meadows: green and gold grass, scattered poplars and flowers.</summary>
        Meadow = 3,
        /// <summary>The tugai: gold and green poplars and tamarisks along the river.</summary>
        Riverbank = 4,
        /// <summary>Wet floodplain: reeds, mud and tamarisks.</summary>
        Floodplain = 5,
        /// <summary>Rolling hills: earth mounds, boulders and cedars.</summary>
        Hills = 6,
        /// <summary>High stony ground: big boulders, few trees.</summary>
        HighHills = 7,
        /// <summary>Old ruins: paving, broken walls and standing stones.</summary>
        Ruins = 8,
        /// <summary>Closed wood: dense dark poplars and cedars.</summary>
        DenseWood = 9,
        /// <summary>Dark ravine of basalt with cave mouths.</summary>
        Ravine = 10,
        /// <summary>The Brejo: mud, ponds, reeds and dead trees.</summary>
        Marsh = 11,
        /// <summary>Silk mulberry rows and loom ruins.</summary>
        Mulberry = 12,
        /// <summary>The tall laurel forest of Garajonay.</summary>
        Canopy = 13,
        /// <summary>Mihrshah's walled garden: paving, cypresses, roses, dry canals.</summary>
        Garden = 14,

        // Região 2 — Areias Flamejantes (OverworldLayouts.Areias.cs, OverworldTerrains.Desert.cs): 20–39.
        /// <summary>The caravan road's dry steppe turning to sand: camel thorn, tamarisks, stone markers.</summary>
        CaravanSteppe = 20,
        /// <summary>The Oásis de Qocho: palms and green patches round the water.</summary>
        Oasis = 21,
        /// <summary>Date-palm rows watered by the karez channels.</summary>
        PalmGrove = 22,
        /// <summary>Stony desert (hamada) of the Artífices: gravel, lotus columns, ushabtis.</summary>
        StoneDesert = 23,
        /// <summary>Low rolling dunes.</summary>
        LowDunes = 24,
        /// <summary>The erg: high crested dunes.</summary>
        Erg = 25,
        /// <summary>Red sandstone canyons: chimneys and natural arches.</summary>
        RedCanyon = 26,
        /// <summary>Salt flats: white crust, brine pools.</summary>
        SaltFlat = 27,
        /// <summary>The Meroë pyramid fields: steep pyramids, chapels, tells.</summary>
        MeroeRuins = 28,
        /// <summary>Flat-topped sandstone mesas over a sand floor.</summary>
        Mesas = 29,
        /// <summary>Lines of karez shafts across the clay plain.</summary>
        KarezField = 30,
        /// <summary>A tamarisk thicket round an oasis lake.</summary>
        TamariskOasis = 31,
        /// <summary>Dark sand strewn with bones and broken caravans.</summary>
        BoneValley = 32,
        /// <summary>The Galerias do Karez (09): sunken clay ground over the galleries, shafts and wells.</summary>
        KarezGallery = 33,
        /// <summary>The Dunas que Cantam (10): the tallest dunes, the crescent lake.</summary>
        SingingDunes = 34,
        /// <summary>The Ermos do Tambor Rubro (11): layered red badlands, canyons and chimneys.</summary>
        Badlands = 35,
        /// <summary>The Trono de Namrael (12): terraces of salt and red crystal, rammed-earth towers.</summary>
        SaltThrone = 36,

        // Região 4 — Grutas do Limiar (OverworldLayouts.Limiar.cs, OverworldTerrains.Caves.cs): 40–59.
        /// <summary>The mouth of the caves towards the Hahya: juniper, tuff boulders, the first glowing moss.</summary>
        CaveMouth = 40,
        /// <summary>Black-sand shores of the underground lakes: basalt, moss, glowing mushrooms by the water.</summary>
        LakeShore = 41,
        /// <summary>A hall of stalactites and stalagmites.</summary>
        StalactiteHall = 42,
        /// <summary>A grove of giant glowing mushrooms.</summary>
        MushroomGrove = 43,
        /// <summary>A field of blue crystals on indigo rock.</summary>
        CrystalField = 44,
        /// <summary>Chasms and fissures: high basalt cut by gorges, rope bridges over the abysses.</summary>
        Chasm = 45,
        /// <summary>Stepped pink-tuff terraces.</summary>
        TuffTerraces = 46,
        /// <summary>A forest of natural stone columns.</summary>
        ColumnForest = 47,
        /// <summary>Moss meadows with springs and the vishapakar (21).</summary>
        MossMeadow = 48,
        /// <summary>The kobolds' tunnels: rails, carts, air pipes (22).</summary>
        TunnelMarket = 49,
        /// <summary>The underground canyon of the wheel of twelve jars: tuff, bronze, aqueducts (23).</summary>
        WheelCanyon = 50,
        /// <summary>Mahaway's roost: a giant shaft, nests on the ledges, rope bridges (24).</summary>
        Roost = 51,

        // Região 3 — Serra de Kaggen (OverworldLayouts.Kaggen.cs, OverworldTerrains.Mountain.cs): 60–79.
        /// <summary>Golden grass slopes round the Vila de Litema: aloes, proteas, the odd baobab, pastors' rondavels.</summary>
        GoldenSlopes = 60,
        /// <summary>Farming terraces: dry-stone retaining walls with sorghum rows on each step.</summary>
        FarmTerraces = 61,
        /// <summary>Windy aloe slopes: tree aloes, protea, granite boulders, cairns.</summary>
        AloeSlopes = 62,
        /// <summary>Baobab grassland on the lower plateau.</summary>
        BaobabGrass = 63,
        /// <summary>The high pass of Hahya: granite, snow in the hollows, twisted cedars.</summary>
        HighPass = 64,
        /// <summary>Meltwater torrents in green gullies: cedars, ferns, mossy boulders.</summary>
        Torrents = 65,
        /// <summary>Herders' pastures with round dry-stone kraals (currais) and rondavels.</summary>
        Kraals = 66,
        /// <summary>Narrow passes between rock walls (paredões).</summary>
        NarrowPass = 67,
        /// <summary>Stepped red sandstone escarpments.</summary>
        SandstoneScarp = 68,
        /// <summary>The Abrigos Pintados (15): sandstone overhangs with rock paintings.</summary>
        PaintedShelters = 69,
        /// <summary>The Muralhas de Pedra Seca (16): granite hills, curved dry-stone walls, conical towers, lichen.</summary>
        DryStone = 70,
        /// <summary>The Salões de Bronze de Napirasu (17): dark granite, bronze rune pillars, cold lava.</summary>
        BronzeHalls = 71,
        /// <summary>Snowfields under the summit: snow, granite, sparse cedars, mist.</summary>
        Snowfield = 72,
        /// <summary>The Cume dos Mazans (18): the snowy peak in the mist, giant granite blocks.</summary>
        Summit = 73,
        /// <summary>The Estela Alta plateau (18): snow-dusted steps, the shepherds' shelter.</summary>
        StelaPlateau = 74,
    }

    /// <summary>
    /// A group of monsters a spawn clearing of an overworld zone can host (Docs/Mapas100.md §0.5 archetypes): day, dusk and
    /// night tables over the existing enemy definitions, and the night watchers that only look at weak heroes.
    /// </summary>
    public sealed class OverworldFauna
    {
        public string Id;
        public string Archetypes;
        public int MaxAlive = 3;
        public float RespawnSeconds = 25f;
        public PeriodSpawnEntry[] Day = new PeriodSpawnEntry[0];
        public PeriodSpawnEntry[] Dusk;
        public PeriodSpawnEntry[] Night = new PeriodSpawnEntry[0];
        public string PassiveSpeciesId;
        public int PassiveBelowHeroLevel;
    }

    /// <summary>
    /// One area of a continuous region (Docs/Mapas100.md §0.10): a rectangle of the overworld with its level band, the level
    /// (and special gate) to set foot in it, its terrain and its monster groups. A catalog map (01–06) is one or more zones
    /// (<see cref="MapId"/>); the in-between areas (meadows, hills, closed wood…) have no catalog id and count for the map
    /// in <see cref="OwnerMapId"/> (return city, fast travel "you are here", gates).
    /// </summary>
    public sealed class OverworldZone
    {
        public string Id;

        /// <summary>The catalog map this zone is (part of), or null for an in-between area.</summary>
        public string MapId;

        /// <summary>The catalog map an in-between area counts for (equal to <see cref="MapId"/> when that is set).</summary>
        public string OwnerMapId;

        public Rect Bounds;
        public OverworldTerrain Terrain;
        public int MinLevel = 1;
        public int MaxLevel = 1;

        /// <summary>Lowest level that may walk in (hero terrain bonus applies, <see cref="MapAccessRules.EffectiveLevelRequirement"/>).</summary>
        public int WalkLevel = 1;

        public string RegionType = MapRegionTypes.Forest;

        /// <summary>Special gate (<see cref="MapRequirementTags"/>), empty = none.</summary>
        public string RequirementTag = string.Empty;

        public string NameEn;
        public string NamePt;

        /// <summary>Grid spacing of the generated spawn clearings (m): smaller = more monster groups.</summary>
        public float ClearingSpacing = 100f;

        public readonly List<OverworldFauna> Fauna = new List<OverworldFauna>();

        /// <summary>The catalog map the hero is "on" while in this zone.</summary>
        public string MapIdOrOwner => !string.IsNullOrEmpty(MapId) ? MapId : OwnerMapId;

        public string NameKey => "overworld.zone." + Id + ".name";

        public override string ToString() => Id;
    }

    /// <summary>
    /// A safe settlement placed in the overworld (the Vila, the outposts of the catalog maps): its town-kit recipe, the
    /// generated town (local coordinates, plaza at its origin) and where it sits (<see cref="Offset"/>), its safe zone and
    /// its Cais da Barca in world coordinates.
    /// </summary>
    public sealed class OverworldSettlement
    {
        public string Id;
        public string MapId;
        public Vector2 Offset;
        public CityTownRecipe Recipe;
        public CityLayout Town;

        /// <summary>Safe zone in world coordinates.</summary>
        public Rect SafeZone;

        public float GroundHalfExtent = 18f;

        /// <summary>A defeat in the region wakes the hero here (the Vila).</summary>
        public bool IsReturnCity;

        public bool HasDock;
        public Rect DockTrigger;
        public Vector2 DockArrival;
        public Vector2 DockArrivalFacing = Vector2.down;

        /// <summary>The square of town ground (world coordinates) the overworld ground leaves to the town kit.</summary>
        public Rect GroundRect => new Rect(Offset.x - GroundHalfExtent, Offset.y - GroundHalfExtent, GroundHalfExtent * 2f, GroundHalfExtent * 2f);
    }

    /// <summary>
    /// A whole region as ONE continuous map (Docs/Mapas100.md §0.10): no scene change between the Vila and the areas around
    /// it. <see cref="World"/> is a <see cref="MapLayout"/> in world coordinates holding what the shared map code reads
    /// (bounds, rivers, trails, landmarks, every spawn clearing, gates and docks, the Vila as the return city), so the
    /// spawners, the river rules and the respawn work unchanged; the zones, settlements and the chunk grid are the
    /// overworld's own. Pure data, deterministic: the online server can rebuild the same world.
    /// </summary>
    public sealed class OverworldLayout
    {
        public string Id;
        public MapRegion Region;
        public Rect Bounds;

        /// <summary>Side of one streamed chunk (m).</summary>
        public float ChunkSize = 48f;

        public MapLayout World;
        public readonly List<OverworldZone> Zones = new List<OverworldZone>();
        public readonly List<OverworldSettlement> Settlements = new List<OverworldSettlement>();

        /// <summary>Spawn clearing id → the zone it belongs to.</summary>
        public readonly Dictionary<string, OverworldZone> ZoneOfSpawn = new Dictionary<string, OverworldZone>();

        /// <summary>The region's town (respawn, start).</summary>
        public OverworldSettlement Vila;

        /// <summary>The bounds of the embedded hand-made maps (world): no generated clearings or noise scatter inside them.</summary>
        public readonly List<Rect> AuthoredAreas = new List<Rect>();

        /// <summary>The wild props of the embedded hand-made maps (world coordinates), streamed with their chunk.</summary>
        public readonly List<CityProp> FixedProps = new List<CityProp>();

        /// <summary>The relief of the region, built on first use (<see cref="OverworldRelief.For"/>).</summary>
        internal OverworldRelief ReliefCache;

        private Dictionary<Vector2Int, List<(Vector2 Center, float Length, float Yaw, float Width)>> _riverPieces;
        private Dictionary<Vector2Int, List<CityProp>> _fixedProps;

        public bool InAuthoredArea(Vector2 p)
        {
            foreach (Rect r in AuthoredAreas)
            {
                if (r.Contains(p))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The embedded props standing in <paramref name="chunk"/>.</summary>
        public IReadOnlyList<CityProp> FixedPropsIn(Vector2Int chunk)
        {
            if (_fixedProps == null)
            {
                _fixedProps = new Dictionary<Vector2Int, List<CityProp>>();
                foreach (CityProp prop in FixedProps)
                {
                    Vector2Int c = OverworldRules.ChunkOf(this, prop.Position);
                    if (!_fixedProps.TryGetValue(c, out List<CityProp> list))
                    {
                        list = new List<CityProp>();
                        _fixedProps[c] = list;
                    }

                    list.Add(prop);
                }
            }

            return _fixedProps.TryGetValue(chunk, out List<CityProp> props) ? props : (IReadOnlyList<CityProp>)System.Array.Empty<CityProp>();
        }

        public OverworldZone Zone(string id) => Zones.Find(z => z.Id == id);

        public IEnumerable<OverworldZone> ZonesOfMap(string mapId)
        {
            foreach (OverworldZone zone in Zones)
            {
                if (zone.MapId == mapId)
                {
                    yield return zone;
                }
            }
        }

        public bool ContainsMap(string mapId)
        {
            foreach (OverworldZone zone in Zones)
            {
                if (zone.MapId == mapId && !string.IsNullOrEmpty(mapId))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The settlement (and Barca stop) of a catalog map, or null.</summary>
        public OverworldSettlement SettlementOf(string mapId) => string.IsNullOrEmpty(mapId) ? null : Settlements.Find(s => s.MapId == mapId);

        /// <summary>
        /// Where a hero entering the overworld stands: at the gate towards <paramref name="fromMapId"/> when they walked in from
        /// a map outside the region, else at the dock of <paramref name="mapId"/>'s settlement, else at the Vila's dock.
        /// </summary>
        public Vector2 ArrivalFor(string mapId, string fromMapId, out Vector2 facing)
        {
            if (!string.IsNullOrEmpty(fromMapId) && !ContainsMap(fromMapId))
            {
                MapExitSpec gate = World.Exits.Find(e => e.Kind == MapExitKind.Map && e.TargetMapId == fromMapId);
                if (gate != null)
                {
                    facing = gate.ArrivalFacing;
                    return gate.Arrival;
                }
            }

            OverworldSettlement stop = SettlementOf(mapId) ?? Vila;
            if (stop != null && stop.HasDock)
            {
                facing = stop.DockArrivalFacing;
                return stop.DockArrival;
            }

            facing = World.DockArrivalFacing;
            return World.DockArrival;
        }

        /// <summary>The river blocker pieces (all rivers) bucketed by chunk, computed once.</summary>
        public IReadOnlyList<(Vector2 Center, float Length, float Yaw, float Width)> RiverPiecesIn(Vector2Int chunk)
        {
            if (_riverPieces == null)
            {
                _riverPieces = new Dictionary<Vector2Int, List<(Vector2, float, float, float)>>();
                foreach (MapRiver river in World.Rivers)
                {
                    foreach ((Vector2 center, float length, float yaw) in MapLayoutRules.RiverBlockers(river))
                    {
                        Vector2Int c = OverworldRules.ChunkOf(this, center);
                        if (!_riverPieces.TryGetValue(c, out List<(Vector2, float, float, float)> list))
                        {
                            list = new List<(Vector2, float, float, float)>();
                            _riverPieces[c] = list;
                        }

                        list.Add((center, length, yaw, river.Width));
                    }
                }
            }

            return _riverPieces.TryGetValue(chunk, out List<(Vector2, float, float, float)> pieces)
                ? pieces
                : (IReadOnlyList<(Vector2, float, float, float)>)System.Array.Empty<(Vector2, float, float, float)>();
        }
    }
}
