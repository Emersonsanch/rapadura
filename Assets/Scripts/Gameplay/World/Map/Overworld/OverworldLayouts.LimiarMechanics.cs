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
    }
}
