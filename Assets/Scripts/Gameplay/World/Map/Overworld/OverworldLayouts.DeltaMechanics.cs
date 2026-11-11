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
    }
}
