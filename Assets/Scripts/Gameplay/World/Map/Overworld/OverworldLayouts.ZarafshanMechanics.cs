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

        /// <summary>
        /// The centre of the chahar bagh of the Jardim de Mihrshah (06): the pavilion of eight columns, where the two canals cross (the garden
        /// is the map's settlement recipe, 80 m east of the Portão do Jardim).
        /// </summary>
        public static Vector2 MihrshahGardenCentre => MihrshahOffset + new Vector2(80f, 0f);

        /// <summary>
        /// The four trapped sparks of the garden (06, <c>ZarafshanGardenRules</c>): each cage on the clearing of its quadrant (the spawn zones
        /// 30 m out on both axes, where the quadrant's beasts stand) and the arm of the dry canal it fills, from 2 m inside the wall to 9 m short
        /// of the pavilion: north-west fills the north arm, north-east the east, south-west the west, south-east the south. Built on first use:
        /// the garden's offset lives in another part of this partial class.
        /// </summary>
        public static ZarafshanGardenSpark[] MihrshahSparks
        {
            get
            {
                if (_mihrshahSparks == null)
                {
                    Vector2 c = MihrshahGardenCentre;
                    _mihrshahSparks = new[]
                    {
                        new ZarafshanGardenSpark("noroeste", c + new Vector2(-30f, 30f), c + new Vector2(0f, 9f), c + new Vector2(0f, 56f)),
                        new ZarafshanGardenSpark("nordeste", c + new Vector2(30f, 30f), c + new Vector2(9f, 0f), c + new Vector2(56f, 0f)),
                        new ZarafshanGardenSpark("sudoeste", c + new Vector2(-30f, -30f), c + new Vector2(-56f, 0f), c + new Vector2(-9f, 0f)),
                        new ZarafshanGardenSpark("sudeste", c + new Vector2(30f, -30f), c + new Vector2(0f, -56f), c + new Vector2(0f, -9f)),
                    };
                }

                return _mihrshahSparks;
            }
        }

        private static ZarafshanGardenSpark[] _mihrshahSparks;
    }
}
