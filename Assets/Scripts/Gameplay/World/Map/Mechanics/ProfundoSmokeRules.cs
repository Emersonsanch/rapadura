using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>A bank of smoke of the Pit of Smoke and the fan of the Artificers that clears it, outside its rim (region coordinates).</summary>
    public readonly struct ProfundoSmokeBank
    {
        public readonly Vector2 Center;
        public readonly float Radius;
        public readonly Vector2 Vent;

        public ProfundoSmokeBank(Vector2 center, float radius, Vector2 vent)
        {
            Center = center;
            Radius = radius;
            Vent = vent;
        }
    }

    /// <summary>
    /// The wind that clears the smoke (Docs/Mapas100.md, map 84: "ligar os ventiladores (ou skills de Vento Vivo, Garrafas de Ar Sereno) limpa a
    /// fumaça por 30 s"), pure. The smoke only slows and stings, so a hero with no wind still walks through; each bank has its fan beside it,
    /// outside the smoke, and any Wind skill reaching the bank clears it as well.
    /// <code>
    /// in the smoke: hero × 0.8 · 1 tick per 1.5 s, 2 % of max life, down to 35 % of max life at most
    /// fan: 1.5 s within 4 m · a clear bank stays clear 30 s
    /// </code>
    /// </summary>
    public static class ProfundoSmokeRules
    {
        public const float SpeedFactor = 0.8f;
        public const float BiteTickSeconds = 1.5f;
        public const float BiteFraction = 0.02f;
        public const float BiteFloorFraction = 0.35f;
        public const float VentSeconds = 1.5f;
        public const float VentReach = 4f;
        public const float ClearSeconds = 30f;

        /// <summary>How far past a bank's rim (m) a Wind skill still clears it, on top of the skill's own area.</summary>
        public const float WindMargin = 2f;

        /// <summary>The move-speed modifier of the smoke (−0.2).</summary>
        public static float MoveBonus => SpeedFactor - 1f;

        public static bool IsClear(float clearLeft) => clearLeft > 0f;

        /// <summary>The seconds of clear air left after <paramref name="deltaTime"/>.</summary>
        public static float Advance(float clearLeft, float deltaTime) => Mathf.Max(0f, clearLeft - deltaTime);

        /// <summary>The bank whose smoke is at <paramref name="p"/>, or −1 (a clear bank has none).</summary>
        public static int SmokeAt(IReadOnlyList<ProfundoSmokeBank> banks, IReadOnlyList<float> clearLeft, Vector2 p)
        {
            for (int i = 0; i < banks.Count; i++)
            {
                if (!IsClear(clearLeft[i]) && (p - banks[i].Center).sqrMagnitude <= banks[i].Radius * banks[i].Radius)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>The fan within reach of <paramref name="p"/> (the closest), or −1.</summary>
        public static int VentAt(IReadOnlyList<ProfundoSmokeBank> banks, Vector2 p)
        {
            int best = -1;
            float bestDistance = VentReach * VentReach;
            for (int i = 0; i < banks.Count; i++)
            {
                float d = (banks[i].Vent - p).sqrMagnitude;
                if (d <= bestDistance)
                {
                    bestDistance = d;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>Whether a Wind skill landing at <paramref name="point"/> with its area reaches the bank.</summary>
        public static bool WindReaches(ProfundoSmokeBank bank, Vector2 point, float areaRadius) => Vector2.Distance(bank.Center, point) <= bank.Radius + Mathf.Max(0f, areaRadius) + WindMargin;

        public static float Bite(float currentHealth, float maxHealth) => ProfundoHazardRules.Bite(currentHealth, maxHealth, BiteFraction, BiteFloorFraction);
    }
}
