using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>The places of the Delta de Jhilpur wandering islands, in the region's own coordinates (the driver adds the zone's origin).</summary>
    public static partial class OverworldLayouts
    {
        /// <summary>The region's rectangle: where <see cref="Mechanics.CostaDeltaTetoMechanicsDriver"/> runs the islands.</summary>
        public static readonly Rect DeltaBounds = Rect.MinMaxRect(-1200f, -1800f, 1200f, 600f);

        /// <summary>A zone of the region: its <see cref="OverworldZone.Origin"/> says where the region sits in the stitched world.</summary>
        public const string DeltaAnchorZoneId = "ilhas_que_andam";

        /// <summary>The centre of the bog of map 33 and of the ring of stations, south of the Canal do Leste, off every road.</summary>
        public static readonly Vector2 ReedBogCenter = new Vector2(975f, -1215f);

        public const float ReedBogRadius = 105f;
        public const float ReedStationRing = 85f;

        /// <summary>The eight stations the islands drift between, on a ring round the bog's centre (one every 45°).</summary>
        public static readonly Vector2[] ReedIslandStations =
        {
            ReedBogCenter + new Vector2(85f, 0f), ReedBogCenter + new Vector2(60.1f, 60.1f), ReedBogCenter + new Vector2(0f, 85f), ReedBogCenter + new Vector2(-60.1f, 60.1f),
            ReedBogCenter + new Vector2(-85f, 0f), ReedBogCenter + new Vector2(-60.1f, -60.1f), ReedBogCenter + new Vector2(0f, -85f), ReedBogCenter + new Vector2(60.1f, -60.1f),
        };

        /// <summary>
        /// The five Fontes do Lodo of map 35 (<see cref="Mechanics.LodoFountainRules"/>), a spiral down to the Olho do Lodo (<see cref="LodoArena"/>):
        /// 170, 140, 110, 80 and 50 m from the stone ring, 45° apart (from the north-east, the Raiz-Mãe's side, round by the north and west),
        /// 355 m of walking from the first to the last (71 s on foot, well inside the 2 min a fountain stays lit), all in the Olho do Lodo,
        /// clear of the mire pools (the nearest, at (−1000, −1450), is 75 m off the third).
        /// </summary>
        public static readonly Vector2[] LodoFountains =
        {
            new Vector2(-753f, -1555f), new Vector2(-864f, -1505f), new Vector2(-955f, -1545f), new Vector2(-977f, -1619f), new Vector2(-943f, -1665f),
        };
    }
}
