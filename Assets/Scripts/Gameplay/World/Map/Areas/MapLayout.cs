using System.Collections.Generic;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Areas
{
    /// <summary>Where a map exit leads.</summary>
    public enum MapExitKind
    {
        /// <summary>Back to the island (world) map.</summary>
        WorldMap = 0,

        /// <summary>On foot into a neighbouring map (walk level checked).</summary>
        Map = 1,

        /// <summary>The Cais da Barca: opens the Barcas de Luz window.</summary>
        Dock = 2,
    }

    /// <summary>A way out of a playable map (ground rect on the XZ plane, x = east, y = north).</summary>
    public sealed class MapExitSpec
    {
        public string Id;
        public MapExitKind Kind;
        public Rect Trigger;

        /// <summary>For <see cref="MapExitKind.Map"/>: the neighbouring map.</summary>
        public string TargetMapId;

        /// <summary>Where a hero coming in from <see cref="TargetMapId"/> stands (just inside this exit), and the way they face.</summary>
        public Vector2 Arrival;
        public Vector2 ArrivalFacing = Vector2.up;
    }

    /// <summary>
    /// A monster spawner of a map (step 4 of the pipeline): a clearing with a level band, a population cap, a respawn
    /// timer and a spawn table per period of the day (step 5, night variant: <see cref="NightForm.Variant"/> entries and
    /// night-only species). Entries resolve through Resources/Enemies like <see cref="PeriodSpawnTable"/>.
    /// </summary>
    public sealed class MapSpawnZone
    {
        public string Id;
        public Vector2 Center;
        public float Radius = 10f;
        public int MinLevel = 1;
        public int MaxLevel = 1;
        public int MaxAlive = 3;
        public float RespawnSeconds = 25f;

        /// <summary>Archetype tags of the clearing ([rápido], [enxame]...), documentation for the balance pass.</summary>
        public string Archetypes;

        public PeriodSpawnEntry[] Day = new PeriodSpawnEntry[0];
        public PeriodSpawnEntry[] Dawn;
        public PeriodSpawnEntry[] Dusk;
        public PeriodSpawnEntry[] Night = new PeriodSpawnEntry[0];

        /// <summary>A species that only watches (never attacks) heroes below <see cref="PassiveBelowHeroLevel"/> (Mapas100 01: the owl).</summary>
        public string PassiveSpeciesId;
        public int PassiveBelowHeroLevel;

        /// <summary>A group standing on a road or trail of the stitched world (OverworldLayouts.AddTrailSpawns), not a clearing: no plateau under it.</summary>
        public bool AlongTrail;

        /// <summary>The table for <paramref name="phase"/>: dawn/dusk fall back to the day table when not given.</summary>
        public IReadOnlyList<PeriodSpawnEntry> EntriesFor(TimeOfDayPhase phase)
        {
            switch (phase)
            {
                case TimeOfDayPhase.Night: return Night ?? Day;
                case TimeOfDayPhase.Dawn: return Dawn ?? Day;
                case TimeOfDayPhase.Dusk: return Dusk ?? Day;
                default: return Day;
            }
        }

        /// <summary>Every entry of every period (validation, tests).</summary>
        public IEnumerable<PeriodSpawnEntry> AllEntries()
        {
            foreach (PeriodSpawnEntry[] table in new[] { Day, Dawn, Dusk, Night })
            {
                if (table == null)
                {
                    continue;
                }

                foreach (PeriodSpawnEntry entry in table)
                {
                    yield return entry;
                }
            }
        }
    }

    /// <summary>A crossing of a river: a log bridge (deck, walked over) or a ford (shallow, walked through).</summary>
    public sealed class MapCrossing
    {
        public Vector2 At;
        public float Width = 4f;
        public bool IsBridge;
    }

    /// <summary>A river as a polyline: the water blocks the hero except at its crossings.</summary>
    public sealed class MapRiver
    {
        public readonly List<Vector2> Points = new List<Vector2>();
        public float Width = 8f;
        public readonly List<MapCrossing> Crossings = new List<MapCrossing>();
    }

    /// <summary>A trail (≥ 3 m, Docs/Mapas100.md §0.7): drawn on the ground and kept clear of props.</summary>
    public readonly struct MapPath
    {
        public readonly Vector2 A;
        public readonly Vector2 B;
        public readonly float Width;

        public MapPath(Vector2 a, Vector2 b, float width = 3.5f)
        {
            A = a;
            B = b;
            Width = width;
        }
    }

    public enum MapLandmarkKind
    {
        /// <summary>A ring of standing stones (the boss clearing).</summary>
        StoneRing = 0,

        /// <summary>The water mill of the Moinho do Pomar.</summary>
        WaterMill = 1,

        /// <summary>A wooden signpost at a junction.</summary>
        Signpost = 2,

        /// <summary>A campfire with logs (outposts, camps).</summary>
        Campfire = 3,

        /// <summary>Melon beds of the orchard.</summary>
        MelonBed = 4,
    }

    public readonly struct MapLandmark
    {
        public readonly MapLandmarkKind Kind;
        public readonly Vector2 Position;
        public readonly float Yaw;
        public readonly float Scale;

        public MapLandmark(MapLandmarkKind kind, Vector2 position, float yaw = 0f, float scale = 1f)
        {
            Kind = kind;
            Position = position;
            Yaw = yaw;
            Scale = scale;
        }
    }

    /// <summary>
    /// The whole code-built layout of one playable map (Docs/Mapas100.md, the 6-step pipeline of TODO.md): bounds and
    /// ground (step 1, greybox: trails, rivers, crossings, clearings), the safe settlement as a town-kit recipe
    /// (step 2, <see cref="CityTownRecipe"/> → <see cref="CityLayoutGenerator"/>), the wild props whose colliders come
    /// from <see cref="SolidPropBinder"/> (step 3), the spawn zones with level bands (step 4) and their night tables
    /// (step 5), the exits, arrivals and the Cais da Barca (step 6). Pure data, deterministic: the online server can
    /// build the same map (positions, spawns) without a scene. The settlement plaza is the world origin.
    /// </summary>
    public sealed class MapLayout
    {
        public string MapId;
        public BiomeType Biome = BiomeType.Forest;

        /// <summary>Walkable rectangle of the map (invisible walls on its edges).</summary>
        public Rect Bounds;

        /// <summary>The safe settlement (a full city, or an outpost), plaza at the origin.</summary>
        public CityTownRecipe Settlement;

        /// <summary>The settlement generated from <see cref="Settlement"/> and fitted to the map (walls on the map edges, its own safe zone).</summary>
        public CityLayout Town;

        /// <summary>Half size of the settlement's own ground (its street field); the map ground covers the rest.</summary>
        public float SettlementGroundHalfExtent = 24f;

        /// <summary>The settlement's safe zone (no combat, Docs/Cities.md §3).</summary>
        public Rect SafeZone;

        /// <summary>The settlement is the map's return city (map 01: the Vila): a defeat here wakes the hero at this plaza.</summary>
        public bool SettlementIsReturnCity;

        /// <summary>Where the Barcas de Luz leave the hero (beside the dock) and the way they face.</summary>
        public Vector2 DockArrival;
        public Vector2 DockArrivalFacing = Vector2.down;

        public readonly List<MapExitSpec> Exits = new List<MapExitSpec>();
        public readonly List<MapSpawnZone> Zones = new List<MapSpawnZone>();
        public readonly List<MapRiver> Rivers = new List<MapRiver>();
        public readonly List<MapPath> Paths = new List<MapPath>();
        public readonly List<MapLandmark> Landmarks = new List<MapLandmark>();

        /// <summary>Trees, rocks and bushes of the wild (each its own object; SolidPropBinder gives the colliders).</summary>
        public readonly List<CityProp> WildProps = new List<CityProp>();

        /// <summary>Darker grass discs (the clearings) drawn on the ground.</summary>
        public readonly List<Vector2> Clearings = new List<Vector2>();

        public MapExitSpec Exit(string id) => Exits.Find(e => e.Id == id);

        public MapSpawnZone Zone(string id) => Zones.Find(z => z.Id == id);

        /// <summary>Where a hero arriving from <paramref name="fromMapId"/> stands: the exit towards that map, else the dock.</summary>
        public Vector2 ArrivalFrom(string fromMapId, out Vector2 facing)
        {
            MapExitSpec exit = string.IsNullOrEmpty(fromMapId) ? null : Exits.Find(e => e.Kind == MapExitKind.Map && e.TargetMapId == fromMapId);
            if (exit != null)
            {
                facing = exit.ArrivalFacing;
                return exit.Arrival;
            }

            facing = DockArrivalFacing;
            return DockArrival;
        }
    }
}
