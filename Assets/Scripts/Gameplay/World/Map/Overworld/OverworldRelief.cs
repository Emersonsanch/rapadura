using System.Collections.Generic;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>How an overworld terrain rises (<see cref="OverworldTerrainStyle.Shape"/>).</summary>
    public enum ReliefShape
    {
        /// <summary>Long soft swells (fields, meadows, steppe).</summary>
        Rolling = 0,
        /// <summary>Rounded hills with flat valley floors between them.</summary>
        Hills = 1,
        /// <summary>Stepped plateaus (the Altos; the garden's terraces).</summary>
        Terraced = 2,
        /// <summary>A high ground cut by winding gorges (the Ravina).</summary>
        Gorges = 3,
        /// <summary>Low ground with scattered tells (the buried ruins).</summary>
        Mounds = 4,

        // Região 2 — Areias Flamejantes (OverworldRelief.Desert.cs): 20–29.
        /// <summary>Crested dunes: long windward backs, steep slip faces, sinuous crests.</summary>
        Dunes = 20,
        /// <summary>Flat-topped sandstone mesas with sheer sides over a sand floor.</summary>
        Mesas = 21,
        /// <summary>A high layered plateau cut by narrow winding canyons (the badlands).</summary>
        Canyons = 22,

        // Região 5 — Costa das Monções (OverworldRelief.Coast.cs): 30–39.
        /// <summary>Sea cliffs: a stepped laterite headland broken by sheer bluffs and coves.</summary>
        Cliffs = 30,
        /// <summary>Flat sandbanks and islets a little over the water line, with low coral humps.</summary>
        Islets = 31,

        // Região 6 — Delta de Jhilpur (OverworldRelief.Delta.cs): 40–49.
        /// <summary>Bog hummocks: round tussock-and-root islands over flat mud, close together.</summary>
        Hummocks = 40,
        /// <summary>Paddy levees: long low dikes on a flat floor (the trails run on the mean).</summary>
        Levees = 41,

        // Região 7 — Teto do Mundo (OverworldRelief.Pamir.cs): 50–59.
        /// <summary>Moraines: long bent ridges of gravel side by side, boulder humps on them.</summary>
        Moraines = 50,
        /// <summary>A glacier tongue: a broad smooth swell of ice cut by shallow crevasse grooves.</summary>
        IceTongue = 51,
        /// <summary>Seracs: blocks of ice stepped and broken, sharp-edged humps over a high floor.</summary>
        SeracField = 52,
        /// <summary>Peaks: sharp ridged mountains (ridged noise), the highest ground of the world.</summary>
        Peaks = 53,

        // Região 8 — Planície de Mingcheng (OverworldRelief.Mingcheng.cs): 60–69.
        /// <summary>Karst: sheer limestone towers on a flat floor (the trails and clearings keep to the floor).</summary>
        KarstTowers = 60,
        /// <summary>The great wall's crest: one long ridge following the wall's line across the region, foothills below it.</summary>
        WallRidge = 61,

        // Região 9 — Estepe dos Nartes (OverworldRelief.Nartes.cs): 70–79.
        /// <summary>Kurgans: round burial mounds standing on a flat steppe (the trails and clearings keep to the floor between them).</summary>
        Kurgans = 70,
        /// <summary>The Caucasus massif: tilted rock strata in long hogback ridges with sharp crests, the highest ground of the steppe.</summary>
        Massif = 71,

        // Região 10 — Picos de Farnkesh (OverworldRelief.Farnkesh.cs): 80–89.
        /// <summary>Sandstone pinnacles: sheer round towers with domed tops standing out of a low floor (the trails and clearings keep to the floor).</summary>
        Pinnacles = 80,
        /// <summary>Cloud isles: flat-topped stone islands with sheer sides over a low cloud floor (the trails cross on the floor).</summary>
        CloudIsles = 81,
        /// <summary>A narrow gorge: two high walls along the area, a level floor between them.</summary>
        NarrowGorge = 82,

        // Região 11 — Mar Gelado (OverworldRelief.MarGelado.cs): 90–99.
        /// <summary>Ice floes: flat plates of sea ice, each a little higher or lower than the next, narrow leads between them.</summary>
        Floes = 90,
        /// <summary>Icebergs: tall angular bergs with flat tops standing out of a flat frozen sea (the trails and clearings keep to the sea).</summary>
        Icebergs = 91,
        /// <summary>Pressure ridges: long sharp ridges of crushed ice in lines across a flat floor.</summary>
        PressureRidges = 92,
        /// <summary>Fjord walls: a wide frozen channel winding between high stepped walls of dark rock.</summary>
        FjordWalls = 93,
        /// <summary>Ice vaults: round domes of ice and frozen earth with lanes (the galleries) between them.</summary>
        IceVaults = 94,

        // Região 12 — Arquipélago de Hinokura (OverworldRelief.Hinokura.cs): 100–109.
        /// <summary>A volcano's cone round a fixed centre, rising in three rings to a crater rim with a dip inside (the trails climb it).</summary>
        VolcanoCone = 100,
        /// <summary>Lava flows: rough lobes and ropy ridges of black lava over a low floor.</summary>
        LavaFlows = 101,
        /// <summary>A glass reef: sharp ridges and shards of obsidian standing out of a low floor.</summary>
        GlassReef = 102,
        /// <summary>Curved ramparts: long curved walls in rings round a fixed centre, gaps between them.</summary>
        CurvedRamparts = 103,
        /// <summary>A caldera: a high rim of cold stone in a ring round a fixed centre, a low floor inside (the lava lake).</summary>
        CalderaRim = 104,

        // Região 13 — Rio das Doze Horas (OverworldRelief.Rio.cs): 110–119.
        /// <summary>Raised rectangular beds with furrows between them.</summary>
        ReedBedGrid = 110,
        /// <summary>Diagonal zigzag ridges of dark sand with lanes between them.</summary>
        ZigzagDunes = 111,
        /// <summary>Concentric rings of scales round a fixed centre, a gap in each ring that turns from ring to ring.</summary>
        CoilRings = 112,
        /// <summary>A serpent's body: one long sinuous ridge segmented like scales, over a low sandbank.</summary>
        ApepBody = 113,

        // Região 14 — Margens da Mistura (OverworldRelief.Mistura.cs): 120–129.
        /// <summary>A flat dial with twelve low radial ridges round a fixed centre (the Desert of the Clock).</summary>
        GnomonDial = 120,
        /// <summary>Concentric rings of root ridges round a fixed centre, low in the middle (Hobabish's cedar rings).</summary>
        CedarRings = 121,
        /// <summary>One broad hump along the giant's shoulder (a high spine across the area).</summary>
        GiantShoulder = 122,
        /// <summary>Parallel folds of stone skin.</summary>
        SkinFolds = 123,
        /// <summary>A rough cone of diorite rising round a fixed centre (Ullikummi).</summary>
        DioriteRise = 124,
    }

    /// <summary>What the relief knows about one point besides its height (ground colours and scatter read it).</summary>
    public struct ReliefInfo
    {
        public float Height;

        /// <summary>The terrain drawn there (nearest zone through the border warp, like <see cref="OverworldTerrains.TerrainAt"/>).</summary>
        public OverworldTerrain Terrain;

        /// <summary>Distance (m) from the nearest river's water edge (negative in the water; large when no river is near).</summary>
        public float RiverEdge;

        /// <summary>The nearest river runs in a carved bed (wide rivers and ponds) instead of on the surface.</summary>
        public bool RiverCarved;

        /// <summary>Distance (m) from the nearest trail's edge (large when none is near).</summary>
        public float TrailEdge;

        /// <summary>Distance from the nearest spawn clearing's centre in clearing radii (large when none is near).</summary>
        public float Clearing;

        /// <summary>How much of the zone relief is left (0 on the valley floors, towns and gates; 1 away from them).</summary>
        public float Free;
    }

    /// <summary>
    /// The relief of a continuous region (Docs/Mapas100.md §0.10, "Relevo"): a pure, deterministic height for every point,
    /// so the ground mesh of each chunk, its collider, the props, the spawners and the hero all agree, chunk borders share
    /// their vertices (no seams), and an online server can rebuild it. Built once per layout (<see cref="For"/>):
    /// <list type="bullet">
    /// <item>each zone rises after its terrain (<see cref="OverworldTerrainStyle.Relief"/> and <see cref="ReliefShape"/>),
    /// blended across zone borders through the same warp as the ground colours;</item>
    /// <item>trails are levelled across their width (their centre line follows only the smooth part of the relief), spawn
    /// clearings and landmarks sit on small plateaus;</item>
    /// <item>the settlements, their safe zones and the gates stand at exactly y = 0 (the town kit assumes it), rivers run in
    /// valleys whose floor is y = 0, wide rivers and ponds carve a bed below the water line (<see cref="WaterY"/>);</item>
    /// <item>outside the walls the backdrop rises into hills.</item>
    /// </list>
    /// </summary>
    public sealed partial class OverworldRelief
    {
        /// <summary>Side (m) of a ground cell: the relief is sampled on this world-aligned grid and drawn as facets.</summary>
        public const float Cell = 3f;

        /// <summary>Water line of the carved rivers and ponds.</summary>
        public const float WaterY = -0.3f;

        /// <summary>Rivers at least this wide (m) carve a bed; narrower ones (the aryk, the brook) run on the valley floor.</summary>
        public const float CarveMinWidth = 8f;

        public const float BedDepth = 1.25f;
        public const float FordDepth = 0.5f;

        /// <summary>Flat valley floor (m) beyond a river's half width, then the valley side up to the full relief.</summary>
        public const float ValleyFloor = 7f;
        public const float ValleySide = 50f;

        /// <summary>A trail is level across its half width plus this (m), then blends into the relief over <see cref="TrailRamp"/>.</summary>
        public const float TrailFlat = 0.6f;
        public const float TrailRamp = 5.5f;

        /// <summary>Settlements: flat (y = 0) this far (m) around their town ground, then the ramp up to the relief.</summary>
        public const float TownMargin = 6f;
        public const float TownRamp = 40f;

        public const float GateFlat = 10f;
        public const float GateRamp = 90f;

        /// <summary>Width (m) over which the relief of two zones blends.</summary>
        public const float ZoneBlend = 70f;

        private const float WarpScale = 34f;

        /// <summary>Bound (m) of <see cref="OverworldTerrains.Warp"/> × <see cref="WarpScale"/> (|warp| &lt; 1.35 √2 · 1.1).</summary>
        private const float MaxWarp = 80f;
        private const float BucketSize = 32f;
        private const float DetailWavelength = 16f;

        private struct Segment
        {
            public Vector2 A;
            public Vector2 B;
            public float Half;
            public float LA;
            public float LB;
            public bool Carved;
        }

        private struct Spot
        {
            public Vector2 C;
            public float Inner;
            public float Ramp;
            public float Height;
            public float Radius;
            public bool IsClearing;
        }

        private struct Flat
        {
            public Rect Rect;
            public float Inner;
            public float Ramp;
        }

        private readonly OverworldLayout _layout;
        private readonly Rect _bounds;
        private readonly Rect[] _holes;
        private readonly Rect[] _zoneRects;
        private readonly OverworldTerrainStyle[] _zoneStyles;
        private readonly Vector2[] _zoneOrigins;
        private readonly Segment[] _trails;
        private readonly Segment[] _rivers;
        private readonly Spot[] _spots;
        private readonly Flat[] _flats;
        private readonly (Vector2 At, float Half)[] _fords;

        private readonly Rect _bucketArea;
        private readonly int _nx;
        private readonly int _nz;
        private readonly int[][] _trailCells;
        private readonly int[][] _riverCells;
        private readonly int[][] _spotCells;
        private readonly int[][] _flatCells;
        private readonly int[][] _zoneCells;
        private bool _spotsReady;

        private static readonly int[] None = new int[0];

        /// <summary>The relief of <paramref name="layout"/> (built on first use, then cached on the layout).</summary>
        public static OverworldRelief For(OverworldLayout layout)
        {
            if (layout.ReliefCache == null)
            {
                layout.ReliefCache = new OverworldRelief(layout);
            }

            return layout.ReliefCache;
        }

        private OverworldRelief(OverworldLayout layout)
        {
            _layout = layout;
            _bounds = layout.Bounds;
            _holes = new List<Rect>(layout.Holes).ToArray();
            _zoneRects = new Rect[layout.Zones.Count];
            _zoneStyles = new OverworldTerrainStyle[layout.Zones.Count];
            _zoneOrigins = new Vector2[layout.Zones.Count];
            for (int i = 0; i < layout.Zones.Count; i++)
            {
                _zoneRects[i] = layout.Zones[i].Bounds;
                _zoneStyles[i] = OverworldTerrains.For(layout.Zones[i].Terrain);
                _zoneOrigins[i] = layout.Zones[i].Origin;
            }

            // Buckets cover the walls plus the backdrop ring and the warp.
            _bucketArea = CityGeometry.Inflate(_bounds, layout.ChunkSize * 2f + 64f);
            _nx = Mathf.CeilToInt(_bucketArea.width / BucketSize);
            _nz = Mathf.CeilToInt(_bucketArea.height / BucketSize);

            // The zones that can weigh at a point of each bucket (blend width plus the largest warp): the stitched world has
            // ~80 zones, a point only ever reads the few around it.
            _zoneCells = Bucket(_zoneRects.Length, i => CityGeometry.Inflate(_zoneRects[i], ZoneBlend + MaxWarp + 1f));

            var trails = new List<Segment>();
            foreach (MapPath path in layout.World.Paths)
            {
                trails.Add(new Segment { A = path.A, B = path.B, Half = path.Width * 0.5f, LA = Smooth(path.A), LB = Smooth(path.B) });
            }

            _trails = trails.ToArray();

            var rivers = new List<Segment>();
            var fords = new List<(Vector2, float)>();
            foreach (MapRiver river in layout.World.Rivers)
            {
                bool carved = river.Width >= CarveMinWidth;
                for (int i = 0; i + 1 < river.Points.Count; i++)
                {
                    rivers.Add(new Segment { A = river.Points[i], B = river.Points[i + 1], Half = river.Width * 0.5f, Carved = carved });
                }

                if (carved)
                {
                    foreach (MapCrossing crossing in river.Crossings)
                    {
                        if (!crossing.IsBridge)
                        {
                            fords.Add((crossing.At, crossing.Width * 0.5f));
                        }
                    }
                }
            }

            _rivers = rivers.ToArray();
            _fords = fords.ToArray();

            var flats = new List<Flat>();
            foreach (OverworldSettlement settlement in layout.Settlements)
            {
                flats.Add(new Flat { Rect = settlement.GroundRect, Inner = TownMargin, Ramp = TownRamp });
                flats.Add(new Flat { Rect = settlement.SafeZone, Inner = 2f, Ramp = TownRamp });
            }

            foreach (MapExitSpec exit in layout.World.Exits)
            {
                flats.Add(new Flat { Rect = exit.Trigger, Inner = GateFlat, Ramp = GateRamp });
                flats.Add(new Flat { Rect = new Rect(exit.Arrival, Vector2.zero), Inner = 4f, Ramp = GateRamp });
            }

            // The passes of the stitched world, where the gates between two regions stood: still at y = 0 on both sides.
            foreach ((Rect rect, float inner) in layout.OpenFlats)
            {
                flats.Add(new Flat { Rect = rect, Inner = inner, Ramp = GateRamp });
            }

            _flats = flats.ToArray();

            var spots = new List<Spot>();
            foreach (MapSpawnZone zone in layout.World.Zones)
            {
                spots.Add(new Spot { C = zone.Center, Inner = zone.Radius * 0.95f + 1f, Ramp = 12f, Radius = zone.Radius, IsClearing = true });
            }

            foreach (MapLandmark landmark in layout.World.Landmarks)
            {
                float s = landmark.Scale <= 0f ? 1f : landmark.Scale;
                float inner;
                switch (landmark.Kind)
                {
                    case MapLandmarkKind.StoneRing: inner = 7.5f * s + 3f; break;
                    case MapLandmarkKind.WaterMill: inner = 5f; break;
                    case MapLandmarkKind.MelonBed: inner = 4f; break;
                    case MapLandmarkKind.Campfire: inner = 3f * s; break;
                    default: inner = 1.5f; break;
                }

                spots.Add(new Spot { C = landmark.Position, Inner = inner, Ramp = 8f, Radius = inner });
            }

            _spots = spots.ToArray();

            _trailCells = Bucket(_trails.Length, i => SegmentBox(_trails[i], _trails[i].Half + TrailFlat + TrailRamp));
            _riverCells = Bucket(_rivers.Length, i => SegmentBox(_rivers[i], _rivers[i].Half + ValleyFloor + ValleySide));
            _flatCells = Bucket(_flats.Length, i => CityGeometry.Inflate(_flats[i].Rect, _flats[i].Inner + _flats[i].Ramp + 1f));
            _spotCells = Bucket(_spots.Length, i => new Rect(_spots[i].C - Vector2.one * (_spots[i].Inner + _spots[i].Ramp + 1f),
                Vector2.one * 2f * (_spots[i].Inner + _spots[i].Ramp + 1f)));

            // Each plateau sits at the height its centre would have without plateaus.
            for (int i = 0; i < _spots.Length; i++)
            {
                _spots[i].Height = PrePlateau(_spots[i].C, out _, out _);
            }

            _spotsReady = true;
        }

        // ------------------------------------------------------------------
        // Public queries
        // ------------------------------------------------------------------

        /// <summary>The height of the ground at <paramref name="p"/> (pure, continuous everywhere).</summary>
        public float HeightAt(Vector2 p)
        {
            var info = new ReliefInfo();
            return Evaluate(p, ref info, false);
        }

        /// <summary>The height and what surrounds <paramref name="p"/> (terrain, river, trail, clearing).</summary>
        public ReliefInfo Sample(Vector2 p)
        {
            var info = new ReliefInfo();
            info.Height = Evaluate(p, ref info, true);
            return info;
        }

        /// <summary>
        /// The height of the drawn (and collided) ground at <paramref name="p"/>: the facet of the <see cref="Cell"/> grid
        /// under it, interpolated like the mesh. Use it to stand things on the ground.
        /// </summary>
        public float SurfaceAt(Vector2 p)
        {
            float gx = (p.x - _bounds.xMin) / Cell;
            float gz = (p.y - _bounds.yMin) / Cell;
            int i = Mathf.FloorToInt(gx);
            int j = Mathf.FloorToInt(gz);
            float h00 = HeightAt(Vertex(i, j));
            float h10 = HeightAt(Vertex(i + 1, j));
            float h01 = HeightAt(Vertex(i, j + 1));
            float h11 = HeightAt(Vertex(i + 1, j + 1));
            return Facet(h00, h10, h01, h11, gx - i, gz - j, i, j);
        }

        /// <summary>World position of grid vertex (<paramref name="i"/>, <paramref name="j"/>).</summary>
        public Vector2 Vertex(int i, int j) => new Vector2(_bounds.xMin + i * Cell, _bounds.yMin + j * Cell);

        /// <summary>True when cell (<paramref name="i"/>, <paramref name="j"/>) is split along its 00–11 diagonal (alternating, a diamond pattern).</summary>
        public static bool MainDiagonal(int i, int j) => ((i + j) & 1) == 0;

        /// <summary>The height inside one cell, on the same two triangles the mesh draws.</summary>
        public static float Facet(float h00, float h10, float h01, float h11, float fx, float fz, int i, int j)
        {
            if (MainDiagonal(i, j))
            {
                return fx >= fz ? h00 + (h10 - h00) * fx + (h11 - h10) * fz : h00 + (h11 - h01) * fx + (h01 - h00) * fz;
            }

            return fx + fz <= 1f ? h00 + (h10 - h00) * fx + (h01 - h00) * fz : h11 + (h01 - h11) * (1f - fx) + (h10 - h11) * (1f - fz);
        }

        /// <summary>The flat rectangles (town ground, safe zones, gates) where the relief is exactly 0 (tests, tools).</summary>
        public IEnumerable<Rect> FlatCores()
        {
            foreach (Flat flat in _flats)
            {
                yield return CityGeometry.Inflate(flat.Rect, flat.Inner);
            }
        }

        // ------------------------------------------------------------------
        // Evaluation
        // ------------------------------------------------------------------

        private float Evaluate(Vector2 p, ref ReliefInfo info, bool wantInfo)
        {
            float h = PrePlateau(p, out OverworldTerrain terrain, out float trailEdge);

            // Plateaus: spawn clearings and landmarks.
            float clearing = 1e4f;
            int cell = CellOf(p);
            if (_spotsReady && cell >= 0)
            {
                int[] spots = _spotCells[cell];
                float sum = 0f;
                float weighted = 0f;
                float max = 0f;
                for (int k = 0; k < spots.Length; k++)
                {
                    ref Spot spot = ref _spots[spots[k]];
                    float reach = spot.Inner + spot.Ramp;
                    float dx = p.x - spot.C.x;
                    float dz = p.y - spot.C.y;
                    float d2 = dx * dx + dz * dz;
                    if (d2 >= reach * reach)
                    {
                        continue;
                    }

                    float d = Mathf.Sqrt(d2);
                    if (spot.IsClearing)
                    {
                        clearing = Mathf.Min(clearing, d / spot.Radius);
                    }

                    float w = 1f - SmoothStep(spot.Inner, spot.Inner + spot.Ramp, d);
                    if (w <= 0f)
                    {
                        continue;
                    }

                    sum += w;
                    weighted += w * spot.Height;
                    max = Mathf.Max(max, w);
                }

                if (sum > 0f)
                {
                    h = Mathf.Lerp(h, weighted / sum, max);
                }
            }

            // River valleys (floor at 0) and the flat ground of the towns and gates.
            float riverEdge = 1e4f;
            bool carved = false;
            float valley = 1f;
            float carve = 0f;
            if (cell >= 0)
            {
                int[] rivers = _riverCells[cell];
                for (int k = 0; k < rivers.Length; k++)
                {
                    ref Segment s = ref _rivers[rivers[k]];
                    float reach = s.Half + ValleyFloor + ValleySide;
                    float d2 = SegmentDistanceSq(p, s.A, s.B, out _);
                    if (d2 >= reach * reach)
                    {
                        continue;
                    }

                    float edge = Mathf.Sqrt(d2) - s.Half;
                    if (edge < riverEdge)
                    {
                        riverEdge = edge;
                        carved = s.Carved;
                    }

                    valley = Mathf.Min(valley, SmoothStep(ValleyFloor, ValleyFloor + ValleySide, edge));
                    if (s.Carved && edge < 1.2f)
                    {
                        carve = Mathf.Max(carve, 1f - SmoothStep(-1f, 1.2f, edge));
                    }
                }
            }

            float free = valley * FlatFactor(p, cell);
            h *= free;

            if (carve > 0f)
            {
                float ford = 0f;
                foreach ((Vector2 at, float half) in _fords)
                {
                    float d = Vector2.Distance(p, at);
                    if (d < half + 3f)
                    {
                        ford = Mathf.Max(ford, 1f - SmoothStep(half, half + 3f, d));
                    }
                }

                h -= carve * Mathf.Lerp(BedDepth, FordDepth, ford);
            }

            // The backdrop beyond the walls rises into hills.
            float outside = OutsideDistance(p);
            if (outside > 0f)
            {
                h += SmoothStep(4f, 70f, outside) * (9f + 7f * OverworldNoise.Perlin(p.x / 60f + 3.3f, p.y / 60f + 8.1f));
            }

            if (wantInfo)
            {
                info.Terrain = terrain;
                info.RiverEdge = riverEdge;
                info.RiverCarved = carved;
                info.TrailEdge = trailEdge;
                info.Clearing = clearing;
                info.Free = free;
            }

            return h;
        }

        /// <summary>Zone relief with the trails levelled (no plateaus, valleys or flats yet).</summary>
        private float PrePlateau(Vector2 p, out OverworldTerrain terrain, out float trailEdge)
        {
            Zones(p, out float smooth, out float shaped, out float detail, out terrain);
            float h = shaped + detail;
            trailEdge = 1e4f;
            int cell = CellOf(p);
            if (cell < 0)
            {
                return h;
            }

            int[] trails = _trailCells[cell];
            float sum = 0f;
            float weighted = 0f;
            float max = 0f;
            for (int k = 0; k < trails.Length; k++)
            {
                ref Segment s = ref _trails[trails[k]];
                float reach = s.Half + TrailFlat + TrailRamp;
                float d2 = SegmentDistanceSq(p, s.A, s.B, out float t);
                if (d2 >= reach * reach)
                {
                    continue;
                }

                float d = Mathf.Sqrt(d2);
                trailEdge = Mathf.Min(trailEdge, d - s.Half);
                float w = 1f - SmoothStep(s.Half + TrailFlat, s.Half + TrailFlat + TrailRamp, d);
                if (w <= 0f)
                {
                    continue;
                }

                sum += w;
                weighted += w * (s.LA + (s.LB - s.LA) * t);
                max = Mathf.Max(max, w);
            }

            return sum > 0f ? Mathf.Lerp(h, weighted / sum, max) : h;
        }

        /// <summary>The smooth part of the relief only (trail centre lines follow it).</summary>
        private float Smooth(Vector2 p)
        {
            Zones(p, out float smooth, out _, out _, out _);
            return smooth;
        }

        private void Zones(Vector2 p, out float smooth, out float shaped, out float detail, out OverworldTerrain terrain)
        {
            Vector2 wp = p + OverworldTerrains.Warp(p) * WarpScale;
            float sw = 0f;
            float sl = 0f;
            float st = 0f;
            float sd = 0f;
            float best = float.MaxValue;
            int bestIndex = 0;
            int cell = CellOf(p);
            int[] near = cell >= 0 ? _zoneCells[cell] : null;
            int count = near != null ? near.Length : _zoneRects.Length;
            for (int k = 0; k < count; k++)
            {
                int i = near != null ? near[k] : k;
                Rect r = _zoneRects[i];
                float dx = Mathf.Max(Mathf.Max(r.xMin - wp.x, wp.x - r.xMax), 0f);
                float dz = Mathf.Max(Mathf.Max(r.yMin - wp.y, wp.y - r.yMax), 0f);
                float d = dx == 0f && dz == 0f ? 0f : Mathf.Sqrt(dx * dx + dz * dz);
                if (d < best)
                {
                    best = d;
                    bestIndex = i;
                }

                float w = 1f - d / ZoneBlend;
                if (w <= 0f)
                {
                    continue;
                }

                w *= w;
                Shape(_zoneStyles[i], p - _zoneOrigins[i], out float l, out float t, out float dd);
                sw += w;
                sl += w * l;
                st += w * t;
                sd += w * dd;
            }

            // Every zone that can weigh is in the bucket (in ascending order: the same sums as a full pass); the nearest zone
            // for the terrain is only sure when it is within the blend width, else (far backdrop) look at them all.
            if (near != null && best > ZoneBlend)
            {
                best = float.MaxValue;
                bestIndex = 0;
                for (int i = 0; i < _zoneRects.Length; i++)
                {
                    Rect r = _zoneRects[i];
                    float dx = Mathf.Max(Mathf.Max(r.xMin - wp.x, wp.x - r.xMax), 0f);
                    float dz = Mathf.Max(Mathf.Max(r.yMin - wp.y, wp.y - r.yMax), 0f);
                    float d = dx == 0f && dz == 0f ? 0f : Mathf.Sqrt(dx * dx + dz * dz);
                    if (d < best)
                    {
                        best = d;
                        bestIndex = i;
                    }
                }
            }

            // Inside the walls one zone always weighs 1; beyond them the relief fades out with the weights.
            float inv = 1f / Mathf.Max(sw, 1f);
            smooth = sl * inv;
            shaped = st * inv;
            detail = sd * inv;
            terrain = _zoneRects.Length > 0 ? _zoneStyles[bestIndex].Terrain : OverworldTerrain.Meadow;
        }

        private static void Shape(OverworldTerrainStyle style, Vector2 p, out float smooth, out float shaped, out float detail)
        {
            float a = style.Relief;
            if (a <= 0f)
            {
                smooth = shaped = detail = 0f;
                return;
            }

            float f = 1f / style.ReliefScale;
            float ox = 13.7f * ((int)style.Terrain + 1);
            float oz = 5.3f * ((int)style.Terrain + 1);
            float x = p.x * f + ox;
            float z = p.y * f + oz;
            switch (style.Shape)
            {
                case ReliefShape.Hills:
                {
                    float u = SmoothStep(0f, 1f, 0.5f + 0.95f * OverworldNoise.Fbm(x, z, 3));
                    smooth = shaped = a * u;
                    break;
                }

                case ReliefShape.Terraced:
                {
                    float u = Mathf.Clamp01(0.5f + 0.9f * OverworldNoise.Fbm(x, z, 2));
                    smooth = a * u;
                    shaped = a * Terrace(u, style.TerraceSteps, 0.5f);
                    break;
                }

                case ReliefShape.Gorges:
                {
                    float m = OverworldNoise.Fbm(x * 0.5f + 3f, z * 0.5f + 1f, 2);
                    float n = OverworldNoise.Signed(x, z) + 0.35f * OverworldNoise.Signed(x * 2.1f + 5f, z * 2.1f + 3f);
                    float g = n * n / (n * n + 0.1f);
                    smooth = a * (0.8f + 0.12f * m);
                    shaped = a * (0.12f + 0.78f * g + 0.1f * (0.5f + 0.5f * m));
                    break;
                }

                case ReliefShape.Mounds:
                {
                    smooth = a * (0.3f + 0.25f * OverworldNoise.Fbm(x, z, 2));
                    float b = OverworldNoise.Perlin(x * 4f + 7f, z * 4f + 3f);
                    shaped = smooth + a * 0.7f * SmoothStep(0.58f, 0.82f, b);
                    break;
                }

                default:
                    if (DesertShape(style, x, z, a, out smooth, out shaped) || CoastShape(style, x, z, a, out smooth, out shaped)
                        || DeltaShape(style, x, z, a, out smooth, out shaped) || PamirShape(style, x, z, a, out smooth, out shaped)
                        || MingchengShape(style, x, z, a, out smooth, out shaped) || NartesShape(style, x, z, a, out smooth, out shaped)
                        || FarnkeshShape(style, x, z, a, out smooth, out shaped) || MarGeladoShape(style, x, z, a, out smooth, out shaped)
                        || HinokuraShape(style, x, z, a, out smooth, out shaped) || RioShape(style, x, z, a, out smooth, out shaped)
                        || MisturaShape(style, x, z, a, out smooth, out shaped))
                    {
                        break; // Região 2: dunes, mesas, canyons (.Desert.cs); Região 5: cliffs, islets (.Coast.cs); Região 6: hummocks, levees (.Delta.cs);
                               // Região 7: moraines, ice tongues, seracs, peaks (.Pamir.cs); Região 8: karst towers, the wall's crest (.Mingcheng.cs);
                               // Região 9: kurgans, the Caucasus massif (.Nartes.cs); Região 10: pinnacles, cloud isles, the gorge (.Farnkesh.cs);
                               // Região 11: floes, icebergs, pressure ridges, fjords, ice vaults (.MarGelado.cs);
                               // Região 12: the volcano's cone, lava flows, the glass reef, curved ramparts, the caldera (.Hinokura.cs);
                               // Região 13: reed beds, zigzag dunes, Mehen's rings, Apep's body (.Rio.cs);
                               // Região 14: the gnomon dial, cedar rings, the giant's shoulder, folds, diorite (.Mistura.cs).
                    }

                    smooth = shaped = a * SmoothStep(0f, 1f, 0.5f + 0.85f * OverworldNoise.Fbm(x, z, 2));
                    break;
            }

            detail = style.ReliefDetail * OverworldNoise.Signed(p.x / DetailWavelength + oz, p.y / DetailWavelength + ox);
        }

        /// <summary>Stepped plateaus: <paramref name="steps"/> levels, the risers taking <paramref name="riser"/> of each step.</summary>
        public static float Terrace(float u, int steps, float riser)
        {
            if (steps <= 1)
            {
                return u;
            }

            float t = u * steps;
            float k = Mathf.Floor(t);
            float f = t - k;
            float half = riser * 0.5f;
            float r = SmoothStep(0.5f - half, 0.5f + half, f);
            return Mathf.Min(1f, (k + r) / steps);
        }

        private float FlatFactor(Vector2 p, int cell)
        {
            if (cell < 0)
            {
                return 1f;
            }

            float f = 1f;
            int[] flats = _flatCells[cell];
            for (int i = 0; i < flats.Length; i++)
            {
                ref Flat flat = ref _flats[flats[i]];
                Rect r = flat.Rect;
                float dx = Mathf.Max(Mathf.Max(r.xMin - p.x, p.x - r.xMax), 0f);
                float dz = Mathf.Max(Mathf.Max(r.yMin - p.y, p.y - r.yMax), 0f);
                float reach = flat.Inner + flat.Ramp;
                if (dx >= reach || dz >= reach)
                {
                    continue;
                }

                float d = Mathf.Sqrt(dx * dx + dz * dz);
                f = Mathf.Min(f, SmoothStep(flat.Inner, reach, d));
                if (f <= 0f)
                {
                    return 0f;
                }
            }

            return f;
        }

        private float OutsideDistance(Vector2 p)
        {
            float dx = Mathf.Max(Mathf.Max(_bounds.xMin - p.x, p.x - _bounds.xMax), 0f);
            float dz = Mathf.Max(Mathf.Max(_bounds.yMin - p.y, p.y - _bounds.yMax), 0f);
            if (dx == 0f && dz == 0f)
            {
                // Inside the bounds: beyond the walls only in a hole of the stitched world (no region there).
                return _holes.Length == 0 ? 0f : Mathf.Max(0f, -_layout.WallDistance(p));
            }

            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        /// <summary>Squared distance from <paramref name="p"/> to segment a–b, and where along it (0..1) the nearest point is.</summary>
        private static float SegmentDistanceSq(Vector2 p, Vector2 a, Vector2 b, out float t)
        {
            float abx = b.x - a.x;
            float abz = b.y - a.y;
            float apx = p.x - a.x;
            float apz = p.y - a.y;
            float len2 = abx * abx + abz * abz;
            t = len2 > 1e-6f ? (apx * abx + apz * abz) / len2 : 0f;
            t = t < 0f ? 0f : t > 1f ? 1f : t;
            float dx = apx - abx * t;
            float dz = apz - abz * t;
            return dx * dx + dz * dz;
        }

        public static float SmoothStep(float a, float b, float x)
        {
            float t = Mathf.Clamp01((x - a) / (b - a));
            return t * t * (3f - 2f * t);
        }

        // ------------------------------------------------------------------
        // Buckets
        // ------------------------------------------------------------------

        private int CellOf(Vector2 p)
        {
            int x = (int)((p.x - _bucketArea.xMin) / BucketSize);
            int z = (int)((p.y - _bucketArea.yMin) / BucketSize);
            if (p.x < _bucketArea.xMin || p.y < _bucketArea.yMin || x >= _nx || z >= _nz)
            {
                return -1;
            }

            return z * _nx + x;
        }

        private static Rect SegmentBox(Segment s, float pad)
        {
            return Rect.MinMaxRect(Mathf.Min(s.A.x, s.B.x) - pad, Mathf.Min(s.A.y, s.B.y) - pad, Mathf.Max(s.A.x, s.B.x) + pad, Mathf.Max(s.A.y, s.B.y) + pad);
        }

        private int[][] Bucket(int count, System.Func<int, Rect> box)
        {
            var lists = new List<int>[_nx * _nz];
            for (int i = 0; i < count; i++)
            {
                Rect r = box(i);
                int x0 = Mathf.Clamp((int)Mathf.Floor((r.xMin - _bucketArea.xMin) / BucketSize), 0, _nx - 1);
                int x1 = Mathf.Clamp((int)Mathf.Floor((r.xMax - _bucketArea.xMin) / BucketSize), 0, _nx - 1);
                int z0 = Mathf.Clamp((int)Mathf.Floor((r.yMin - _bucketArea.yMin) / BucketSize), 0, _nz - 1);
                int z1 = Mathf.Clamp((int)Mathf.Floor((r.yMax - _bucketArea.yMin) / BucketSize), 0, _nz - 1);
                for (int z = z0; z <= z1; z++)
                {
                    for (int x = x0; x <= x1; x++)
                    {
                        int c = z * _nx + x;
                        (lists[c] ??= new List<int>()).Add(i);
                    }
                }
            }

            var cells = new int[lists.Length][];
            for (int c = 0; c < lists.Length; c++)
            {
                cells[c] = lists[c] != null ? lists[c].ToArray() : None;
            }

            return cells;
        }
    }
}
