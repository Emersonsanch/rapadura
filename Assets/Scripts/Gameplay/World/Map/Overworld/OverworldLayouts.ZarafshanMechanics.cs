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

        /// <summary>
        /// The marsh of the Brejo do Lodo Escuro (03, <c>ZarafshanMireRules</c>): the bounds of its embedded map (−100…100 × −170…30 round the
        /// map's own origin), where the mud slows the hero off the log walkways.
        /// </summary>
        public static Rect BrejoMireBounds => new Rect(BrejoOffset + new Vector2(-100f, -170f), new Vector2(200f, 200f));

        /// <summary>The mulberry grove of the Bosque das Amoreiras (04, <c>ZarafshanWebRules</c>): the bounds of its embedded map (−100…100 both ways).</summary>
        public static Rect AmoreirasGroveBounds => new Rect(AmoreirasOffset + new Vector2(-100f, -100f), new Vector2(200f, 200f));

        /// <summary>
        /// The eight webs of the Amoreiras (04): each between two neighbouring trees of a row (the rows are planted every 8.5 m along x and 10 m
        /// along z from the block's corner), two per block, beside the clearings of the spiders, the cocoons, the drying lines and the great
        /// loom, at least 15 m from every trail and yard. Built on first use: the map's offset lives in another part of this partial class.
        /// </summary>
        public static ZarafshanWeb[] AmoreirasWebs
        {
            get
            {
                if (_amoreirasWebs == null)
                {
                    Vector2 o = AmoreirasOffset;
                    _amoreirasWebs = new[]
                    {
                        new ZarafshanWeb("so_fileiras", o + new Vector2(-61f, -45f), o + new Vector2(-52.5f, -45f)),
                        new ZarafshanWeb("so_fundo", o + new Vector2(-35.5f, -75f), o + new Vector2(-27f, -75f)),
                        new ZarafshanWeb("se_varais", o + new Vector2(65f, -35f), o + new Vector2(73.5f, -35f)),
                        new ZarafshanWeb("se_fundo", o + new Vector2(22.5f, -55f), o + new Vector2(31f, -55f)),
                        new ZarafshanWeb("no_casulos", o + new Vector2(-44f, 38f), o + new Vector2(-35.5f, 38f)),
                        new ZarafshanWeb("no_oeste", o + new Vector2(-86.5f, 58f), o + new Vector2(-78f, 58f)),
                        new ZarafshanWeb("ne_tear", o + new Vector2(48f, 68f), o + new Vector2(56.5f, 68f)),
                        new ZarafshanWeb("ne_leste", o + new Vector2(73.5f, 18f), o + new Vector2(82f, 18f)),
                    };
                }

                return _amoreirasWebs;
            }
        }

        private static ZarafshanWeb[] _amoreirasWebs;
    }
}
