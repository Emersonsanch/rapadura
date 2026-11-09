using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 16 — Reinos do Céu: the relief shapes of the sky (Docs/Mapas100.md §0.10, maps 91–100), pure and continuous:
    /// <list type="bullet">
    /// <item><see cref="ReliefShape.DeckTiers"/>: the three levels of the deck of a Barca, rising towards the keel line of its row (moon deck, sun deck);</item>
    /// <item><see cref="ReliefShape.WheelRim"/>: the raised rim of the Celestial Wheel round <see cref="OverworldLayouts.WheelCenter"/>;</item>
    /// <item><see cref="ReliefShape.OrbitRidges"/>: low round ridges of the orbits of the seven spheres round <see cref="OverworldLayouts.IlhotaParadaOffset"/>;</item>
    /// <item><see cref="ReliefShape.ColumnRings"/>: rings of firm-cloud steps rising inwards to <see cref="OverworldLayouts.GloryColumnCenter"/>;</item>
    /// <item><see cref="ReliefShape.SkyFissures"/>: long raised lips of the cracks in the sky at the Lookout.</item>
    /// </list>
    /// Every raised part stays in <c>smooth</c> where a road has to ride it (the trails follow only the smooth part of the relief). Towns, posts, gates and trails flatten them like anywhere else.
    /// </summary>
    public sealed partial class OverworldRelief
    {
        /// <summary>Half the width (m) of a deck of a Barca, from its keel line to its rail.</summary>
        public const float DeckHalfWidth = 250f;

        /// <summary>Radius (m) of the rim of the Celestial Wheel, and how far (m) it falls off on each side.</summary>
        public const float WheelRimRadius = 150f;
        public const float WheelRimFalloff = 70f;

        /// <summary>Radius (m) over which the rings of the Coluna climb to its foot.</summary>
        public const float ColumnRingsReach = 380f;

        /// <summary>The Reinos do Céu shapes; false for the other ones (the caller then draws them).</summary>
        private static bool CeuShape(OverworldTerrainStyle style, float x, float z, float a, out float smooth, out float shaped)
        {
            switch (style.Shape)
            {
                case ReliefShape.DeckTiers:
                {
                    Vector2 p = LocalOf(style, x, z);
                    float keel = OverworldTerrains.IsSunDeck(style.Terrain) ? OverworldLayouts.SunDeckKeelZ : OverworldLayouts.MoonDeckKeelZ;
                    float u = SmoothStep(0f, 1f, Mathf.Clamp01(1f - Mathf.Abs(p.y - keel) / DeckHalfWidth));
                    smooth = a * u;
                    shaped = a * Terrace(u, 3, 0.45f);
                    return true;
                }

                case ReliefShape.WheelRim:
                {
                    Vector2 p = LocalOf(style, x, z);
                    float r = (p - OverworldLayouts.WheelCenter).magnitude;
                    float rim = SmoothStep(WheelRimFalloff, 0f, Mathf.Abs(r - WheelRimRadius));
                    smooth = shaped = a * (0.05f + 0.95f * rim); // The roads across the rim ride it.
                    return true;
                }

                case ReliefShape.OrbitRidges:
                {
                    Vector2 p = LocalOf(style, x, z);
                    float r = (p - OverworldLayouts.IlhotaParadaOffset).magnitude;
                    float wave = Mathf.Abs(Mathf.Cos(r * Mathf.PI / 120f));
                    float ridge = SmoothStep(0.55f, 0.98f, wave);
                    smooth = shaped = a * (0.1f + 0.9f * ridge * 0.6f); // Low and round: the roads go over them.
                    return true;
                }

                case ReliefShape.ColumnRings:
                {
                    Vector2 p = LocalOf(style, x, z);
                    float r = (p - OverworldLayouts.GloryColumnCenter).magnitude;
                    float u = Mathf.Clamp01(1f - r / ColumnRingsReach);
                    smooth = a * u;
                    shaped = a * Terrace(u, 6, 0.5f);
                    return true;
                }

                case ReliefShape.SkyFissures:
                {
                    Vector2 p = LocalOf(style, x, z);
                    float wave = Mathf.Abs(Mathf.Sin(p.x * 0.033f + 0.021f * p.y + 0.8f * OverworldNoise.Perlin(x * 0.6f + 5.1f, z * 0.6f + 8.2f)));
                    float lip = SmoothStep(0.8f, 0.98f, wave);
                    smooth = a * 0.1f;
                    shaped = a * (0.1f + 0.9f * lip);
                    return true;
                }

                default:
                    smooth = shaped = 0f;
                    return false;
            }
        }
    }
}
