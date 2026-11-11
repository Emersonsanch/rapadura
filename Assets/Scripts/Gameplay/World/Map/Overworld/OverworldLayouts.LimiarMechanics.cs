using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>The lamp niches of the Grutas do Limiar (<c>LimiarLampRules</c>), in the region's own coordinates: the Vila's first, then the ones along the roads, from the nearest out.</summary>
    public static partial class OverworldLayouts
    {
        public const string LimiarAnchorZoneId = "lago_do_limiar";

        public static readonly Rect LimiarBounds = Rect.MinMaxRect(-1200f, -2100f, 1200f, 300f);

        public static readonly LimiarLamp[] LimiarLamps =
        {
            new LimiarLamp("vila", new Vector2(6f, -46f)),
            new LimiarLamp("boca_do_hahya", new Vector2(-320f, -64f)),
            new LimiarLamp("nicho_da_ponte", new Vector2(330f, -54f)),
            new LimiarLamp("colunata", new Vector2(-12f, -540f)),
            new LimiarLamp("campo_de_cristais", new Vector2(-484f, -450f)),
            new LimiarLamp("lago_dos_peixes", new Vector2(470f, -450f)),
            new LimiarLamp("fendas_ocidentais", new Vector2(-919f, -150f)),
            new LimiarLamp("terracos_de_tufo", new Vector2(800f, -44f)),
        };

        /// <summary>
        /// The wind wells of the Ninho de Mahaway (24, <c>LimiarWindWellRules</c>): one pair over Mahaway's shaft and one over each chasm of the
        /// bottom row, a well on each rim gliding to the other (the long way round is the rope bridge at x ±950, or round the shaft's ends).
        /// </summary>
        public static readonly LimiarWindWell[] LimiarWindWells =
        {
            new LimiarWindWell("poco_do_ninho_norte", new Vector2(-20f, -1968f), new Vector2(-20f, -2062f), 0f),
            new LimiarWindWell("poco_do_ninho_sul", new Vector2(20f, -2062f), new Vector2(20f, -1968f), 6f),
            new LimiarWindWell("poco_das_escarpas_norte", new Vector2(-800f, -1815f), new Vector2(-800f, -1880f), 3f),
            new LimiarWindWell("poco_das_escarpas_sul", new Vector2(-780f, -1885f), new Vector2(-780f, -1818f), 9f),
            new LimiarWindWell("poco_do_abismo_norte", new Vector2(820f, -1850f), new Vector2(820f, -1912f), 3f),
            new LimiarWindWell("poco_do_abismo_sul", new Vector2(800f, -1912f), new Vector2(800f, -1848f), 9f),
        };

        /// <summary>The rope bridges the wells spare (the long way round: west scarps, bottomless abyss), for the docs and the tests.</summary>
        public static readonly Vector2 ScarpsBridge = new Vector2(-950f, -1880f);
        public static readonly Vector2 BottomlessBridge = new Vector2(950f, -1840f);

        /// <summary>
        /// The boat of the Lago do Limiar (19, <c>LimiarBoatRules</c>): from the north landing under the Vila, 5 m off the shore and 26 m below the
        /// trail to the tunnels, over the dark lake (28 m wide round its centre line z ≈ −96…−102) to the south landing on the beach, 5 m off the
        /// shore, between the trail down to the Colunata and the beach's clearing. The Lago's map is embedded at the region's origin.
        /// </summary>
        public static readonly Vector2[] LimiarBoatCourse =
        {
            new Vector2(9f, -82f),
            new Vector2(6f, -92f),
            new Vector2(-4f, -103f),
            new Vector2(-14f, -118f),
        };

        /// <summary>The Lago do Limiar's embedded map (19), where the boat's driver works.</summary>
        public static readonly Rect LimiarLakeBounds = Rect.MinMaxRect(-100f, -170f, 100f, 30f);

        /// <summary>The Salão de Tages' embedded map (20, −100…100 both ways round <see cref="TagesOffset"/>), where the lamps' driver works.</summary>
        public static Rect TagesHallBounds => new Rect(TagesOffset + new Vector2(-100f, -100f), new Vector2(200f, 200f));

        /// <summary>
        /// The six lamps of the Salão de Tages (20, <c>TagesDarkRules</c>): the three lamp posts in the niches of the corridor of columns (3.6 m
        /// beside its centre line, at 14, 30 and 46 m from the stream end) and one by each clearing, 12–14 m from its centre, off the trails: the
        /// south gallery, the dark ford and the colonnade. Built on first use: the hall's offset lives in another part of this partial class.
        /// </summary>
        public static TagesLamp[] TagesLamps
        {
            get
            {
                if (_tagesLamps == null)
                {
                    Vector2 o = TagesOffset;
                    _tagesLamps = new[]
                    {
                        new TagesLamp("corredor_1", o + new Vector2(48.2f, 44.3f)),
                        new TagesLamp("corredor_2", o + new Vector2(34.6f, 52.7f)),
                        new TagesLamp("corredor_3", o + new Vector2(20.9f, 61f)),
                        new TagesLamp("galeria_sul", o + new Vector2(-50f, -41f)),
                        new TagesLamp("vau_escuro", o + new Vector2(74f, -54f)),
                        new TagesLamp("colunata", o + new Vector2(78f, 52f)),
                    };
                }

                return _tagesLamps;
            }
        }

        private static TagesLamp[] _tagesLamps;
    }
}
