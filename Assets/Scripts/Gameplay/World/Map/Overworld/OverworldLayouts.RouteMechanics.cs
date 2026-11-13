using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The places of the route mechanics of Regions 2, 6, 7 and 8 (<see cref="Mechanics.RouteMechanicsDriver"/>), each in its region's own
    /// coordinates (the driver adds the anchor zone's origin): the escorted caravan of the Estrada das Caravanas (07), the fords that the monsoon
    /// floods round Jhilpur and the boats that cross them (31), the rope lifts and chutes of Sangkent's terraces (36), the barges of the drum on the
    /// Canal das Lanternas (41) and its four locks (43). Every station a hero walks to stands on a road (no decor on a trail) or, for the lock
    /// gates, next to a signpost that keeps the bank clear (<see cref="AddCanalLockPosts"/>); every course is built from the same vertices as
    /// the region's own roads and rivers, so it can never drift from them.
    /// </summary>
    public static partial class OverworldLayouts
    {
        /// <summary>Metres a mooring stands off the water's edge (beyond the river's half width, measured square to the river).</summary>
        public const float MooringOffWater = 9f;

        /// <summary>
        /// A station on the road that leaves <paramref name="crossing"/> (a ford or a bridge on river segment <paramref name="riverA"/>–<paramref name="riverB"/>)
        /// towards <paramref name="toward"/> (the road's next vertex), <see cref="MooringOffWater"/> metres beyond the water's edge measured square to
        /// the river, however slanted the road is.
        /// </summary>
        public static Vector2 RoadMooring(Vector2 crossing, Vector2 riverA, Vector2 riverB, float halfWidth, Vector2 toward)
        {
            Vector2 road = (toward - crossing).normalized;
            Vector2 river = (riverB - riverA).normalized;
            float square = Mathf.Abs(road.x * river.y - road.y * river.x);
            return crossing + road * ((halfWidth + MooringOffWater) / Mathf.Max(0.3f, square));
        }

        // ------------------------------------------------------------------
        // Estrada das Caravanas (07, Areias)
        // ------------------------------------------------------------------

        /// <summary>
        /// The road of Ammo's caravan (<see cref="CaravanRoadEscortRules"/>): from its camp on the caravan road below the Pomar gate, down the
        /// road, through the Caravançará do Ammo (its West and East gates at x = −216 and −184), on to Qocho's West street, ~650 m. Every point is a
        /// vertex of the caravan road (OverworldLayouts.Areias: the Pomar road and the Caravançará's embedded road) or on its last leg.
        /// </summary>
        public static readonly Vector2[] AreiasCaravanRoad =
        {
            new Vector2(-300f, 455f), new Vector2(-300f, 330f), new Vector2(-300f, 130f), new Vector2(-216f, 130f), new Vector2(-184f, 130f),
            new Vector2(-100f, 130f), new Vector2(-75f, 60f), Vector2.Lerp(new Vector2(-75f, 60f), new Vector2(-46f, 0f), 0.7f),
        };

        // ------------------------------------------------------------------
        // Monção de Jhilpur (31, Delta)
        // ------------------------------------------------------------------

        /// <summary>
        /// The six fords of the delta's two canals (<see cref="JhilpurMonsoonRules"/>), the same points as the region's <c>Crossing</c>s. The four on a
        /// road have two moorings each, on that road, west/south bank first; the two off the roads (north of Jhilpur, the middle of the Canal do Leste)
        /// have none: in the monsoon they are simply under water.
        /// </summary>
        public static DeltaFord[] DeltaFords => s_deltaFords ??= new[]
        {
            Ford("vau_de_tarchon", JhilpurCanal[6], JhilpurCanal[7], 0.6f, 13f, 12f, new Vector2(-100f, -950f), new Vector2(300f, -1060f)),
            Ford("vau_da_ilhota", JhilpurCanal[8], JhilpurCanal[9], 150f / 340f, 13f, 12f, new Vector2(-200f, -1580f), new Vector2(240f, -1570f)),
            Ford("vau_do_leste", EastCanal[1], EastCanal[2], 0.75f, 11f, 12f, new Vector2(560f, -60f), new Vector2(850f, -120f)),
            Ford("vau_das_ilhas", EastCanal[4], EastCanal[5], 0.3f, 11f, 12f, new Vector2(700f, -1070f), new Vector2(1050f, -900f)),
            Ford("vau_do_norte", JhilpurCanal[1], JhilpurCanal[2], 0.5f, 13f, 10f),
            Ford("vau_do_meio", EastCanal[2], EastCanal[3], 0.5f, 11f, 10f),
        };

        // Built on first use, not as field initialisers: they read arrays of other partial files of this class, whose initialisers C# may run later.
        private static DeltaFord[] s_deltaFords;
        private static Vector2[] s_jhilpurMonsoonRun;
        private static Vector2[] s_drumBargeCourse;
        private static CanalLock[] s_canalLocks;

        /// <summary>The root bridge of the east road over the Canal de Jhilpur (OverworldLayouts.Delta).</summary>
        public static Vector2 JhilpurRootBridge => Vector2.Lerp(JhilpurCanal[2], JhilpurCanal[3], 150f / 210f);

        /// <summary>
        /// Abiesous's boat school, on the east road between Jhilpur's East street and the root bridge, 9 m off the canal's west bank: where the
        /// rowing lesson is taken and where the monsoon run down the canal leaves from.
        /// </summary>
        public static Vector2 JhilpurBoatSchool => RoadMooring(JhilpurRootBridge, JhilpurCanal[2], JhilpurCanal[3], 13f, new Vector2(JhilpurGround, 0f));

        /// <summary>
        /// The monsoon run (rowers only, only in the monsoon): from the boat school into the canal below the root bridge, down the Canal de Jhilpur
        /// past the lake to the Tarchon ford and out on its east mooring, on Tarchon's west road, ~1.1 km. Reversed, the run back up.
        /// </summary>
        public static Vector2[] JhilpurMonsoonRun => s_jhilpurMonsoonRun ??= new[]
        {
            JhilpurBoatSchool, Vector2.Lerp(JhilpurCanal[2], JhilpurCanal[3], 165f / 210f), JhilpurCanal[3], JhilpurCanal[4], JhilpurCanal[5], JhilpurCanal[6],
            DeltaFords[0].Center, DeltaFords[0].MooringB, TarchonRunLanding,
        };

        /// <summary>
        /// Where the monsoon run lands and the run back leaves: 8 m on from the Tarchon ford's east mooring along Tarchon's west road (two stations,
        /// one button each).
        /// </summary>
        public static Vector2 TarchonRunLanding => DeltaFords[0].MooringB + (new Vector2(300f, -1060f) - DeltaFords[0].MooringB).normalized * 8f;

        private static DeltaFord Ford(string id, Vector2 a, Vector2 b, float t, float halfWidth, float crossingWidth)
        {
            return new DeltaFord(id, Vector2.Lerp(a, b, t), (b - a).normalized, halfWidth, crossingWidth * 0.5f + JhilpurMonsoonRules.FordMargin, false, Vector2.zero, Vector2.zero);
        }

        private static DeltaFord Ford(string id, Vector2 a, Vector2 b, float t, float halfWidth, float crossingWidth, Vector2 towardA, Vector2 towardB)
        {
            Vector2 center = Vector2.Lerp(a, b, t);
            return new DeltaFord(id, center, (b - a).normalized, halfWidth, crossingWidth * 0.5f + JhilpurMonsoonRules.FordMargin, true,
                RoadMooring(center, a, b, halfWidth, towardA), RoadMooring(center, a, b, halfWidth, towardB));
        }

        // ------------------------------------------------------------------
        // Os 7 terraços (36, Teto)
        // ------------------------------------------------------------------

        /// <summary>
        /// The foot of the terraces (the 1st terrace, <see cref="SangkentLiftRules"/>): on the east road 30 m out of Sangkent's East street, where the
        /// first rope lift leaves and both chutes end.
        /// </summary>
        public static readonly Vector2 SangkentTerraceFoot = Vector2.Lerp(new Vector2(SangkentGround, 0f), new Vector2(200f, 30f), 30f / 170f);

        /// <summary>The 4th terrace: the north road's bend at (60, 250), where the first lift arrives and the second leaves.</summary>
        public static readonly Vector2 SangkentTerraceMiddle = new Vector2(60f, 250f);

        /// <summary>The top of the middle chute, 8 m up the north road from the 4th terrace's lift (two stations, one button each).</summary>
        public static readonly Vector2 SangkentMiddleChuteTop = SangkentTerraceMiddle + (new Vector2(20f, 500f) - SangkentTerraceMiddle).normalized * 8f;

        /// <summary>The 7th terrace: the north road at (20, 500) in the Terraços Altos, where the second lift arrives and the long chute leaves.</summary>
        public static readonly Vector2 SangkentTerraceTop = new Vector2(20f, 500f);

        /// <summary>The two rope lifts, up only (foot → 4th, 4th → 7th) and the two chutes, down only (7th → foot, 4th → foot).</summary>
        public static readonly SangkentTerraceLine[] SangkentTerraceLines =
        {
            new SangkentTerraceLine("elevador_baixo", true, SangkentTerraceFoot, SangkentTerraceMiddle),
            new SangkentTerraceLine("elevador_alto", true, SangkentTerraceMiddle, SangkentTerraceTop),
            new SangkentTerraceLine("calha_longa", false, SangkentTerraceTop, SangkentTerraceFoot),
            new SangkentTerraceLine("calha_do_meio", false, SangkentMiddleChuteTop, SangkentTerraceFoot),
        };

        // ------------------------------------------------------------------
        // Barcas do Tambor (41) and the locks (43), Mingcheng
        // ------------------------------------------------------------------

        /// <summary>The canal's stone bridge on the east road (OverworldLayouts.Mingcheng).</summary>
        public static Vector2 LanternCanalEastBridge => Vector2.Lerp(LanternCanalLine[0], LanternCanalLine[1], 0.55f);

        /// <summary>The canal's stone bridge on the north road over to the Torres.</summary>
        public static Vector2 LanternCanalNorthBridge => Vector2.Lerp(LanternCanalLine[2], LanternCanalLine[3], 0.68f);

        /// <summary>The Cais do Tambor: on the east road just over the east bridge, on the canal's east bank.</summary>
        public static Vector2 DrumBargeSouthLanding => RoadMooring(LanternCanalEastBridge, LanternCanalLine[0], LanternCanalLine[1], 7f, new Vector2(600f, 170f));

        /// <summary>The Cais da Ponte Norte: on the road from the north bridge to the Eclusa da Lua, on the canal's east bank.</summary>
        public static Vector2 DrumBargeNorthLanding => RoadMooring(LanternCanalNorthBridge, LanternCanalLine[2], LanternCanalLine[3], 7f, new Vector2(700f, 700f));

        /// <summary>
        /// The course of the drum's barges (<see cref="DrumBargeRules"/>), south to north: out of the Cais do Tambor into the canal north of the
        /// east bridge, up the canal through the Eclusa da Lua and the second lock, out before the north bridge onto the Cais da Ponte Norte.
        /// </summary>
        public static Vector2[] DrumBargeCourse => s_drumBargeCourse ??= new[]
        {
            DrumBargeSouthLanding, Vector2.Lerp(LanternCanalLine[0], LanternCanalLine[1], 0.8f), LanternCanalLine[1], LanternCanalLine[2],
            Vector2.Lerp(LanternCanalLine[2], LanternCanalLine[3], 0.6f), DrumBargeNorthLanding,
        };

        /// <summary>Metres from the canal's centre line to a lock gate's two ends (7 m of water, then the stone of the lock).</summary>
        public const float LockEndOffset = 15f;

        /// <summary>
        /// The four locks of the Canal das Lanternas, south to north (<see cref="CanalLockRules"/>): the Eclusa da Lua itself (y ≈ 320, in its
        /// approach area), one below the north bridge (y = 500), and two north of it (y ≈ 762 and 1000), where no bridge crosses.
        /// </summary>
        public static CanalLock[] CanalLocks => s_canalLocks ??= new[]
        {
            Lock("eclusa_da_lua", LanternCanalLine[1], LanternCanalLine[2], 0.48f),
            Lock("eclusa_dos_salgueiros", LanternCanalLine[2], LanternCanalLine[3], 0.2f),
            Lock("eclusa_das_lanternas", LanternCanalLine[3], LanternCanalLine[4], 0.25f),
            Lock("eclusa_da_carpa", LanternCanalLine[4], LanternCanalLine[5], 0.25f),
        };

        private static CanalLock Lock(string id, Vector2 a, Vector2 b, float t)
        {
            Vector2 center = Vector2.Lerp(a, b, t);
            Vector2 along = (b - a).normalized;
            Vector2 east = new Vector2(along.y, -along.x);
            return new CanalLock(id, center, center - east * LockEndOffset, center + east * LockEndOffset, CanalLockRules.DistanceAlong(DrumBargeCourse, center));
        }

        /// <summary>
        /// A signpost 3 m beyond each end of every lock gate (away from the water): the posts mark the gates from afar and keep the bank under the
        /// ends clear of willows and pavilions (decor keeps 4.5 m off a landmark). Called once the region's clearings are made, so it moves none.
        /// </summary>
        private static void AddCanalLockPosts(OverworldLayout w)
        {
            foreach (CanalLock gate in CanalLocks)
            {
                Vector2 out1 = (gate.WestEnd - gate.Center).normalized * 3f;
                Vector2 out2 = (gate.EastEnd - gate.Center).normalized * 3f;
                Landmark(w, MapLandmarkKind.Signpost, gate.WestEnd.x + out1.x, gate.WestEnd.y + out1.y, 0f);
                Landmark(w, MapLandmarkKind.Signpost, gate.EastEnd.x + out2.x, gate.EastEnd.y + out2.y, 0f);
            }
        }
    }
}
