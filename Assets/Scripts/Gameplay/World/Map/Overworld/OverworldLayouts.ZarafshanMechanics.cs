using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>The places of the Bosque do Zarafshan mechanics, in the region's own coordinates: the trees that can be picked stand on the Pomar's lanes, outside the Vila; the fords are the river's real wading crossings in the Tugai.</summary>
    public static partial class OverworldLayouts
    {
        public const string ZarafshanAnchorZoneId = "pomar";

        public static readonly Rect ZarafshanBounds = Rect.MinMaxRect(-1000f, -1000f, 1400f, 1400f);

        public static readonly ZarafshanHarvestSpot[] ZarafshanHarvestSpots =
        {
            new ZarafshanHarvestSpot("damasqueiro_oeste", new Vector2(-150f, 100f)),
            new ZarafshanHarvestSpot("damasqueiro_leste", new Vector2(150f, 100f)),
            new ZarafshanHarvestSpot("romazeira_oeste", new Vector2(-60f, 140f)),
            new ZarafshanHarvestSpot("romazeira_leste", new Vector2(60f, 140f)),
        };

        public static readonly Vector2[] ZarafshanFords =
        {
            new Vector2(330f, 150f),
            new Vector2(460f, 88f),
            new Vector2(572f, 4f),
            new Vector2(604.5f, -0.9f),
        };
    }
}
