using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>The places of the Costa das Monções tide, in the region's own coordinates (the driver adds the zone's origin).</summary>
    public static partial class OverworldLayouts
    {
        /// <summary>The region's rectangle: where <see cref="Mechanics.CostaDeltaTetoMechanicsDriver"/> runs the tide.</summary>
        public static readonly Rect CostaBounds = Rect.MinMaxRect(-600f, -1200f, 1800f, 1200f);

        /// <summary>A zone of the region: its <see cref="OverworldZone.Origin"/> says where the region sits in the stitched world.</summary>
        public const string CostaAnchorZoneId = "recife_de_taa_pwani";

        /// <summary>The six flats of the reef of map 25, off the east road and clear of the pools and the canal, each with a chest in its middle.</summary>
        public static readonly CostaTideFlat[] TideFlats =
        {
            new CostaTideFlat(new Vector2(370f, 130f), 30f), new CostaTideFlat(new Vector2(510f, 195f), 32f), new CostaTideFlat(new Vector2(620f, 170f), 32f),
            new CostaTideFlat(new Vector2(380f, -150f), 30f), new CostaTideFlat(new Vector2(520f, -170f), 35f), new CostaTideFlat(new Vector2(610f, -160f), 30f),
        };
    }
}
