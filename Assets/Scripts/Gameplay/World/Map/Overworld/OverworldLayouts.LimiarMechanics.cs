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
    }
}
