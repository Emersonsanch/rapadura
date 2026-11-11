using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The beacons of the great wall (Docs/Mapas100.md, map 45: "acender os faróis das torres liga um corredor de luz, velocidade +10% a pé dentro
    /// dele"; "os faróis acesos ficam" at night), pure. The state is one array of seconds left per tower, so a server can hold it as it is.
    /// <code>
    /// a beacon burns 300 s · the clock stops at night · between two neighbouring lit towers: the corridor, +10% on foot, on the deck only
    /// </code>
    /// </summary>
    public static class MingchengBeaconRules
    {
        public const float LitSeconds = 300f;
        public const float CorridorBonus = 0.1f;

        public static bool IsLit(IReadOnlyList<float> remaining, int index) => index >= 0 && index < remaining.Count && remaining[index] > 0f;

        /// <summary>Lights tower <paramref name="index"/> for its full time; false if it was already burning (lighting does not refill it).</summary>
        public static bool Light(float[] remaining, int index)
        {
            if (index < 0 || index >= remaining.Length || remaining[index] > 0f)
            {
                return false;
            }

            remaining[index] = LitSeconds;
            return true;
        }

        /// <summary>Burns the beacons down by <paramref name="deltaTime"/>; at night they stay as they are.</summary>
        public static void Advance(float[] remaining, float deltaTime, bool night)
        {
            if (night)
            {
                return;
            }

            for (int i = 0; i < remaining.Length; i++)
            {
                remaining[i] = Mathf.Max(0f, remaining[i] - deltaTime);
            }
        }

        public static int LitCount(IReadOnlyList<float> remaining)
        {
            int count = 0;
            for (int i = 0; i < remaining.Count; i++)
            {
                if (remaining[i] > 0f)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>Whether <paramref name="x"/> lies between two neighbouring towers (<paramref name="towerXs"/> ascending) that are both lit.</summary>
        public static bool InCorridor(IReadOnlyList<float> towerXs, IReadOnlyList<float> remaining, float x)
        {
            for (int i = 0; i + 1 < towerXs.Count; i++)
            {
                if (x >= towerXs[i] && x <= towerXs[i + 1])
                {
                    return remaining[i] > 0f && remaining[i + 1] > 0f;
                }
            }

            return false;
        }

        /// <summary>The speed factor of the corridor: only on the deck, in a lit stretch and on foot.</summary>
        public static float SpeedFactor(bool onDeck, bool inCorridor, bool mounted) => onDeck && inCorridor && !mounted ? 1f + CorridorBonus : 1f;
    }
}
