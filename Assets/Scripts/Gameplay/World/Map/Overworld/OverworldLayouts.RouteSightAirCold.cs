using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The places of three mechanics on the main road, each in its region's own coordinates: the fog of the Copas de Garajonay (05,
    /// <c>GarajonayFogRules</c>, Zarafshan), the still air and the copper pipes of the Mercado dos Túneis (22, <c>TunnelStillAirRules</c>, Limiar)
    /// and the cold of the Geleira do Teto do Mundo with its herders' fires (38, <c>GlacierColdRules</c>, Teto). The bounds are the maps' zones.
    /// </summary>
    public static partial class OverworldLayouts
    {
        /// <summary>The Copas de Garajonay (05): its zone.</summary>
        public static readonly Rect GarajonayFogBounds = Rect.MinMaxRect(1050f, -150f, 1400f, 700f);

        /// <summary>The Mercado dos Túneis (22): its zone.</summary>
        public static readonly Rect TunnelMarketBounds = Rect.MinMaxRect(-300f, -1700f, 300f, -1250f);

        /// <summary>
        /// The kobolds' air pipes (22): one at the market and one every 100 m along each of its four galleries (the trails that leave its four
        /// streets), to the edge of the map, and one in the middle of the map boss's ring.
        /// </summary>
        public static readonly Vector2[] TunnelAirPipes =
        {
            new Vector2(0f, -1475f),
            new Vector2(0f, -1375f), new Vector2(0f, -1275f),
            new Vector2(0f, -1575f), new Vector2(0f, -1675f),
            new Vector2(-100f, -1475f), new Vector2(-200f, -1475f), new Vector2(-290f, -1475f),
            new Vector2(100f, -1475f), new Vector2(200f, -1475f), new Vector2(290f, -1475f),

            // The ring of O Engarrafador Guloso (22's map boss, EngarrafadorGulosoArena): the fight is about his bottles, not the still air.
            new Vector2(160f, -1464f),
        };

        /// <summary>The Geleira do Teto do Mundo (38): its two zones, the serac field and the glacier itself.</summary>
        public static readonly Rect[] GlacierColdAreas =
        {
            Rect.MinMaxRect(-1200f, 840f, -720f, 1320f),
            Rect.MinMaxRect(-720f, 840f, -240f, 1320f),
        };

        /// <summary>The herders' fires of the glacier (38): beside the trails to the Cabana do Iaque, to the seracs and on to the ice peaks.</summary>
        public static readonly Vector2[] GlacierFires =
        {
            new Vector2(-634f, 900f), new Vector2(-394f, 960f), new Vector2(-760f, 1106f), new Vector2(-950f, 1156f), new Vector2(-514f, 1300f),
        };

        /// <summary>Whether <paramref name="local"/> (Teto coordinates) is on the glacier or the serac field.</summary>
        public static bool OnGlacier(Vector2 local)
        {
            foreach (Rect area in GlacierColdAreas)
            {
                if (area.Contains(local))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
