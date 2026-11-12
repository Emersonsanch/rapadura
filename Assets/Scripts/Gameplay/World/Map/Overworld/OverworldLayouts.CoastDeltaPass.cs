using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The places of five mechanics on the main road, each in its region's own coordinates: the pyramids and chapels of Meroë (27,
    /// <c>SunShadowRules</c>), the flooded houses of Kilwa (29, <c>FloodedHouseRules</c>) and the dhows of the Canal de Liyongo (30,
    /// <c>DhowRules</c>) in the Costa das Monções; the forgotten root bridges of the gorges (32, <c>RootBridgeRules</c>) in the Delta; Ammo's
    /// caravan road through the Passo de Wakhan (40, <c>CaravanEscortRules</c>) in the Teto do Mundo. The bounds are the maps' zones.
    /// </summary>
    public static partial class OverworldLayouts
    {
        /// <summary>The Pirâmides Altas de Meroé (27): its zone.</summary>
        public static readonly Rect MeroeShadowBounds = Rect.MinMaxRect(250f, 700f, 750f, 1200f);

        /// <summary>The apexes of the three great pyramids (27), in a half circle north of the Pátio dos Relevos (the light's order: 09, 12, 15 h).</summary>
        public static readonly Vector2[] MeroePyramids = { new Vector2(380f, 1040f), new Vector2(500f, 1110f), new Vector2(620f, 1040f) };

        /// <summary>The square half-side (m) of a great pyramid's base, drawn round its apex.</summary>
        public const float MeroePyramidHalfBase = 12f;

        /// <summary>The height (m) of a great pyramid, drawn.</summary>
        public const float MeroePyramidHeight = 24f;

        /// <summary>Kilwa Afundada (29): its zone (the drowned streets).</summary>
        public static readonly Rect KilwaHousesBounds = Rect.MinMaxRect(1250f, 250f, 1800f, 700f);

        /// <summary>The doors of the four flooded houses (29), one in each quarter between the drowned canals.</summary>
        public static readonly Vector2[] KilwaHouseDoors = { new Vector2(1300f, 450f), new Vector2(1500f, 640f), new Vector2(1720f, 450f), new Vector2(1500f, 300f) };

        /// <summary>The Mangue Raso and the Mangues de Liyongo (30): where the Canal de Liyongo runs.</summary>
        public static readonly Rect LiyongoDhowBounds = Rect.MinMaxRect(750f, -1200f, 1800f, -250f);

        /// <summary>The dhows' course (30): the Canal de Liyongo from its west mouth to its east end (the canal's own points).</summary>
        public static readonly Vector2[] LiyongoDhowCourse =
        {
            new Vector2(1050f, -1000f), new Vector2(1250f, -930f), new Vector2(1500f, -900f), new Vector2(1770f, -960f),
        };

        /// <summary>The Pontes de Raiz Viva and the Gargantas Esquecidas (32): their zones.</summary>
        public static readonly Rect RootBridgeBounds = Rect.MinMaxRect(-1200f, -840f, -720f, 120f);

        /// <summary>The three forgotten bridges (32), on the three gorge rivers away from the road's bridges.</summary>
        public static readonly Vector2[] RootBridges = { new Vector2(-1142f, -234f), new Vector2(-840f, -555f), new Vector2(-868f, -795f) };

        /// <summary>The root chest at the far (south) side of each forgotten bridge.</summary>
        public static readonly Vector2[] RootBridgeChests = { new Vector2(-1142f, -248f), new Vector2(-840f, -569f), new Vector2(-868f, -809f) };

        /// <summary>The Vale do Wakhan, the Alto do Wakhan and the Passo de Wakhan (40): where the escort may run.</summary>
        public static readonly Rect[] WakhanEscortAreas =
        {
            Rect.MinMaxRect(-240f, 840f, 240f, 1320f),
            Rect.MinMaxRect(-720f, 1320f, -240f, 1800f),
            Rect.MinMaxRect(-240f, 1320f, 240f, 1800f),
        };

        /// <summary>Ammo's caravan road (40): up the Wakhan valley, through the Caravançará de Pedra, west across the high Wakhan (the road's own points).</summary>
        public static readonly Vector2[] WakhanCaravanRoad =
        {
            new Vector2(30f, 1100f), new Vector2(40f, 1300f), new Vector2(0f, 1542f), new Vector2(-18f, 1560f), new Vector2(-420f, 1520f),
        };

        /// <summary>Whether <paramref name="local"/> (Teto coordinates) is in the pass's escort areas.</summary>
        public static bool InWakhanEscort(Vector2 local)
        {
            foreach (Rect area in WakhanEscortAreas)
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
