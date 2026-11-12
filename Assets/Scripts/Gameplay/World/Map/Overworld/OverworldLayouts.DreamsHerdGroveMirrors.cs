using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The places of four mechanics on the main road, each in its region's own coordinates: the dream chests of the Fortim dos
    /// Gigantes-da-Colina (49, <c>NartesDreamRules</c>), the loose horses of the Pradaria dos Mil Cavalos (51, <c>PrairieHerdRules</c>), the slow
    /// grove of the Arvoredo de Haripushpa (50, <c>GroveSlowTimeRules</c>), all three in the Estepe dos Nartes, and the mirrors round the Torres
    /// Brancas (42, <c>MirrorClockRules</c>, Mingcheng). The bounds are the maps' zones.
    /// </summary>
    public static partial class OverworldLayouts
    {
        /// <summary>The Fortim dos Gigantes-da-Colina (49): its two zones, the Vale das Tábuas de Sonho and the fort's hills.</summary>
        public static readonly Rect[] FortimDreamAreas =
        {
            Rect.MinMaxRect(-1200f, 360f, -720f, 840f),
            Rect.MinMaxRect(-720f, 840f, -240f, 1320f),
        };

        /// <summary>The eight hiding places of the dream chests (49), spread over the vale and the hills, away from the fort.</summary>
        public static readonly Vector2[] FortimDreamSpots =
        {
            new Vector2(-1100f, 450f), new Vector2(-900f, 520f), new Vector2(-1060f, 760f), new Vector2(-800f, 700f),
            new Vector2(-650f, 900f), new Vector2(-300f, 950f), new Vector2(-660f, 1250f), new Vector2(-320f, 1250f),
        };

        /// <summary>The Pradaria dos Mil Cavalos (51): its zone.</summary>
        public static readonly Rect PrairieHerdBounds = Rect.MinMaxRect(720f, 840f, 1200f, 1320f);

        /// <summary>The grazing spots of the five loose horses (51), round the Poço da Taça.</summary>
        public static readonly Vector2[] PrairieHorseSpots =
        {
            new Vector2(800f, 900f), new Vector2(1120f, 920f), new Vector2(790f, 1250f), new Vector2(1130f, 1260f), new Vector2(960f, 1290f),
        };

        /// <summary>The Arvoredo de Haripushpa (50): its zone, where the clock runs slow.</summary>
        public static readonly Rect SlowGroveBounds = Rect.MinMaxRect(-1200f, 1320f, -720f, 1800f);

        /// <summary>The Torres Brancas de Amanirenas (42): its zone.</summary>
        public static readonly Rect MirrorClockBounds = Rect.MinMaxRect(720f, 360f, 1200f, 840f);

        /// <summary>The four mirrors round the white city (42), in the light's order: north, east, south, west, 70 m out from the spire.</summary>
        public static readonly Vector2[] MirrorClockMirrors =
        {
            new Vector2(960f, 670f), new Vector2(1030f, 600f), new Vector2(960f, 530f), new Vector2(890f, 600f),
        };
    }
}
