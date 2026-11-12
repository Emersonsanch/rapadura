using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The wind of the Forte dos Ventos (Docs/Mapas100.md 39: "direção do vento: girar os 4 cata-ventos muda o vento do mapa"), pure and
    /// server-runnable. Four wind vanes stand round the fort; [E] at one (from 4 m) turns it a quarter, clockwise (north, east, south, west).
    /// The wind of the gorge and the fort is the average of the four: all four the same way blow full, opposite ones cancel (the Senhor do
    /// Cata-Vento left them one each way: no breeze at all). Walking with the wind (within 60°) the hero is up to 20 % faster, against it up to
    /// 20 % slower, in proportion to the wind's strength; across it nothing changes. The vanes stay where they were turned (in the save).
    /// <code>
    /// 4 vanes · [E] 4 m: a quarter turn · wind = mean of the 4 directions (0–1) · with: speed × (1 + 0.2 × wind), against: × (1 − 0.2 × wind)
    /// </code>
    /// </summary>
    public static class WindVaneRules
    {
        public const int VaneCount = 4;
        public const float TurnReach = 4f;
        public const float MaxSpeedChange = 0.2f;
        public const float AlongDot = 0.5f;

        private const string KeyPrefix = "teto.vane.";

        private static string[] s_keys;

        /// <summary>Where each vane points when never turned: one each way (no wind).</summary>
        public static int DefaultState(int vane) => ((vane % 4) + 4) % 4;

        /// <summary>The save key of vane <paramref name="vane"/> (a time mark holding its state + 1; 0 = never turned), built once.</summary>
        public static string StateKey(int vane)
        {
            if (s_keys == null)
            {
                var keys = new string[VaneCount];
                for (int i = 0; i < keys.Length; i++)
                {
                    keys[i] = KeyPrefix + i;
                }

                s_keys = keys;
            }

            return vane >= 0 && vane < s_keys.Length ? s_keys[vane] : KeyPrefix + vane;
        }

        /// <summary>The state read from the save's mark (0 = never turned: the default).</summary>
        public static int StateFrom(long mark, int vane) => mark > 0L ? (int)((mark - 1L) % 4L) : DefaultState(vane);

        public static long MarkOf(int state) => ((state % 4) + 4) % 4 + 1L;

        /// <summary>A quarter turn clockwise.</summary>
        public static int Turn(int state) => (((state + 1) % 4) + 4) % 4;

        /// <summary>The direction a vane in <paramref name="state"/> blows to (x/z): 0 north (+z), 1 east (+x), 2 south, 3 west.</summary>
        public static Vector2 Direction(int state)
        {
            switch (((state % 4) + 4) % 4)
            {
                case 0: return Vector2.up;
                case 1: return Vector2.right;
                case 2: return Vector2.down;
                default: return Vector2.left;
            }
        }

        /// <summary>The wind: the mean of the vanes' directions (length 0–1).</summary>
        public static Vector2 Wind(IReadOnlyList<int> states)
        {
            if (states == null || states.Count == 0)
            {
                return Vector2.zero;
            }

            Vector2 sum = Vector2.zero;
            for (int i = 0; i < states.Count; i++)
            {
                sum += Direction(states[i]);
            }

            return sum / states.Count;
        }

        /// <summary>The factor on the speed of a hero walking along <paramref name="move"/> (x/z) in <paramref name="wind"/>.</summary>
        public static float SpeedFactor(Vector2 move, Vector2 wind)
        {
            float strength = wind.magnitude;
            if (move.sqrMagnitude < 1e-4f || strength < 1e-4f)
            {
                return 1f;
            }

            float dot = Vector2.Dot(move.normalized, wind / strength);
            if (dot >= AlongDot)
            {
                return 1f + MaxSpeedChange * strength;
            }

            return dot <= -AlongDot ? 1f - MaxSpeedChange * strength : 1f;
        }

        /// <summary>The vane within <see cref="TurnReach"/> of <paramref name="p"/> (the nearest), or −1.</summary>
        public static int VaneAt(IReadOnlyList<Vector2> vanes, Vector2 p)
        {
            int best = -1;
            float bestSq = TurnReach * TurnReach;
            for (int i = 0; vanes != null && i < vanes.Count; i++)
            {
                float sq = (vanes[i] - p).sqrMagnitude;
                if (sq <= bestSq)
                {
                    best = i;
                    bestSq = sq;
                }
            }

            return best;
        }
    }
}
