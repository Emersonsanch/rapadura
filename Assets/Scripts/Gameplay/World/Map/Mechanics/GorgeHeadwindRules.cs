using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The headwind of the Garganta dos Ays (Docs/Mapas100.md 57: "andar contra o vento é ×0,7; atrás das pedras-abrigo, ×1"; "Noite: o vento para
    /// pela metade"), pure and server-runnable. Between the gusts of map 55 (<see cref="WindGustRules"/>, which still blow both ways) a steady
    /// wind runs out of the gorge along <c>OverworldLayouts.GorgeWindAxis</c>, towards the Escarpa dos Ventos where the Rei dos Ays waits.
    /// <code>
    /// walking within 60° of straight into the wind: speed ×0.7 (×0.85 at night) · any other way: ×1
    /// within 6 m of a pedra-abrigo: ×1 whatever the way
    /// </code>
    /// Decision (2026-11-16): walking with the wind gives nothing (the sheet only names the headwind), and the steady wind does not change
    /// side with the gusts, so the way in (west, up the gorge) is the hard way and the way back is plain walking.
    /// </summary>
    public static class GorgeHeadwindRules
    {
        public const float AgainstFactor = 0.7f;
        public const float NightAgainstFactor = 0.85f;

        /// <summary>Cosine of the widest angle off straight into the wind that still counts as against it (60°).</summary>
        public const float AgainstDot = 0.5f;

        /// <summary>Metres round a pedra-abrigo where the wind does not reach.</summary>
        public const float ShelterRadius = 6f;

        /// <summary>The steady wind (unit), blowing along <paramref name="axis"/> out of the gorge.</summary>
        public static Vector2 Wind(Vector2 axis) => axis.sqrMagnitude < 1e-6f ? Vector2.zero : axis.normalized;

        /// <summary>Whether a hero moving along <paramref name="move"/> walks into <paramref name="wind"/>.</summary>
        public static bool Against(Vector2 move, Vector2 wind)
        {
            if (move.sqrMagnitude < 1e-4f || wind.sqrMagnitude < 1e-6f)
            {
                return false;
            }

            return Vector2.Dot(move.normalized, wind.normalized) <= -AgainstDot;
        }

        /// <summary>The nearest pedra-abrigo within <see cref="ShelterRadius"/> of <paramref name="p"/>, or −1.</summary>
        public static int ShelterAt(IReadOnlyList<Vector2> stones, Vector2 p)
        {
            int best = -1;
            float bestSqr = ShelterRadius * ShelterRadius;
            for (int i = 0; stones != null && i < stones.Count; i++)
            {
                float sqr = (stones[i] - p).sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>The factor on the speed of a hero moving along <paramref name="move"/> in the gorge.</summary>
        public static float SpeedFactor(Vector2 move, Vector2 wind, bool sheltered, bool night)
        {
            if (sheltered || !Against(move, wind))
            {
                return 1f;
            }

            return night ? NightAgainstFactor : AgainstFactor;
        }
    }
}
