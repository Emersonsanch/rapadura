using System.Collections.Generic;
using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The places of the mechanics of the far south of the Areias Flamejantes and of the top of the Serra de Kaggen, each in its region's own
    /// coordinates: the sunbeams of the Galerias do Karez (09, <c>KarezSunbeamRules</c>), the Dunas que Cantam (10, <c>DuneSongRules</c>), the
    /// salt of the Trono de Namrael with the shade of its towers (12, <c>NamraelGlareRules</c>) and the thin air of the Cume dos Mazans (18,
    /// <c>MazanThinAirRules</c>). The bounds are the maps' zones.
    /// </summary>
    public static partial class OverworldLayouts
    {
        /// <summary>The Galerias do Karez (09): its zone.</summary>
        public static readonly Rect KarezGalleriesBounds = Rect.MinMaxRect(-400f, -1300f, 400f, -900f);

        /// <summary>The Dunas que Cantam (10): its zone.</summary>
        public static readonly Rect SingingDunesBounds = Rect.MinMaxRect(-400f, -1900f, 400f, -1300f);

        /// <summary>The crescent lake of the dunes (centre and radius of the arc, plus the water's half width): the sand does not sing in it.</summary>
        public static readonly Vector2 DunesCrescentCentre = new Vector2(0f, -1715f);
        public const float DunesCrescentRadius = 115f;
        public const float DunesCrescentHalfWidth = 10f;

        /// <summary>The Trono de Namrael (12): its zone, the salt terraces up to the throne.</summary>
        public static readonly Rect NamraelSaltBounds = Rect.MinMaxRect(400f, -1900f, 1200f, -1300f);

        /// <summary>The rammed-earth towers of Qocho's sentinels on the salt (the same six the layout raises), whose shade the glare does not reach.</summary>
        public static readonly Vector2[] NamraelShadeTowers =
        {
            new Vector2(600f, -1350f), new Vector2(1010f, -1380f), new Vector2(640f, -1660f), new Vector2(1060f, -1700f), new Vector2(930f, -1860f), new Vector2(520f, -1840f),
        };

        /// <summary>
        /// The sunbeams of the Galerias do Karez (09): every fourth shaft of the two karez lines over the galleries (the lines the layout draws
        /// from (−380, −930) to (380, −1270) every 19 m and from (−380, −1270) to (−60, −940) every 21 m). Built on first use.
        /// </summary>
        public static Vector2[] KarezSunbeams
        {
            get
            {
                if (_karezSunbeams == null)
                {
                    var beams = new List<Vector2>();
                    KarezSunbeamRules.AddBeamsAlong(beams, new Vector2(-380f, -930f), new Vector2(380f, -1270f), 19f);
                    KarezSunbeamRules.AddBeamsAlong(beams, new Vector2(-380f, -1270f), new Vector2(-60f, -940f), 21f);
                    _karezSunbeams = beams.ToArray();
                }

                return _karezSunbeams;
            }
        }

        private static Vector2[] _karezSunbeams;

        /// <summary>The top plateau of the Cume dos Mazans (18): the northern half of the summit's zone, round Mazan's ring of stones.</summary>
        public static readonly Rect MazanThinAirBounds = Rect.MinMaxRect(-1700f, 1100f, -1300f, 1500f);

        /// <summary>Whether <paramref name="local"/> (Areias coordinates) is in the water of the dunes' crescent lake (the arc 200°–340°).</summary>
        public static bool InDunesCrescent(Vector2 local)
        {
            Vector2 d = local - DunesCrescentCentre;
            float r = d.magnitude;
            if (Mathf.Abs(r - DunesCrescentRadius) > DunesCrescentHalfWidth)
            {
                return false;
            }

            float angle = Mathf.Repeat(Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, 360f);
            return angle >= 195f && angle <= 345f;
        }
    }
}
