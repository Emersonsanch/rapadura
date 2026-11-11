using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>The places of the Planície de Mingcheng mechanics, in the region's own coordinates (the driver adds the zone's origin).</summary>
    public static partial class OverworldLayouts
    {
        /// <summary>The region's rectangle: where <see cref="Mechanics.MingchengMechanicsDriver"/> runs.</summary>
        public static readonly Rect MingchengBounds = Rect.MinMaxRect(-1200f, -600f, 1200f, 1800f);

        /// <summary>A zone of the region: its <see cref="OverworldZone.Origin"/> says where the region sits in the stitched world.</summary>
        public const string MingchengAnchorZoneId = "muralha_das_mil_torres";

        /// <summary>The Arrozais Espelhados (map 44), where the water shows the reflection.</summary>
        public static readonly Rect MirrorPaddiesArea = Rect.MinMaxRect(-1200f, -120f, -720f, 360f);

        /// <summary>
        /// The twelve towers of map 45 with a stair and a beacon, west to east: multiples of 120 m, the grid the chunk builder stands the beacon
        /// towers on, one run of neighbours so the corridor of light chains from end to end.
        /// </summary>
        public static readonly float[] GreatWallTowerXs = { -720f, -600f, -480f, -360f, -240f, -120f, 0f, 120f, 240f, 360f, 480f, 600f };

        /// <summary>The still pools of the paddies (map 44).</summary>
        public static readonly MingchengPool[] MirrorPools =
        {
            new MingchengPool(new Vector2(-1080f, 10f), 7f), new MingchengPool(new Vector2(-800f, -50f), 7f), new MingchengPool(new Vector2(-1120f, 190f), 7f),
            new MingchengPool(new Vector2(-800f, 210f), 7f), new MingchengPool(new Vector2(-900f, -20f), 7f), new MingchengPool(new Vector2(-1060f, 290f), 7f),
        };

        /// <summary>One reflected trail per pool, ending in its chest.</summary>
        public static readonly MingchengReflectedTrail[] ReflectedTrails =
        {
            new MingchengReflectedTrail(0, new Vector2(-1080f, 10f), new Vector2(-1090f, -10f), new Vector2(-1065f, -25f)),
            new MingchengReflectedTrail(1, new Vector2(-800f, -50f), new Vector2(-780f, -70f), new Vector2(-760f, -78f)),
            new MingchengReflectedTrail(2, new Vector2(-1120f, 190f), new Vector2(-1145f, 210f), new Vector2(-1145f, 230f)),
            new MingchengReflectedTrail(3, new Vector2(-800f, 210f), new Vector2(-780f, 235f), new Vector2(-790f, 255f)),
            new MingchengReflectedTrail(4, new Vector2(-900f, -20f), new Vector2(-915f, -45f), new Vector2(-895f, -60f)),
            new MingchengReflectedTrail(5, new Vector2(-1060f, 290f), new Vector2(-1040f, 315f), new Vector2(-1020f, 320f)),
        };
    }
}
