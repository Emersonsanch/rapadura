using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The waking stones of the Campo das Pedras-Dragão (Docs/Mapas100.md 21: "pedras que acordam: tocar uma vishapakar acende a fonte dela
    /// (cura), mas acorda um vishap jovem"), pure and server-runnable. [E] at one of the twelve vishapakar (from 4 m) lights its spring for 60 s:
    /// 8 m round the stone the hero heals 2 % of max health per second. Touching it also wakes the nearest beast within 60 m (bosses never): it
    /// comes straight for the hero and stays awake 60 s, hitting 20 % harder and moving 15 % faster. Each stone wakes once per game day (the day
    /// it was touched stays in the save).
    /// <code>
    /// 12 stones · [E] 4 m · spring 8 m, 60 s, heal 2 %/s · wakes the nearest beast within 60 m (no boss): aggro, damage × 1.2, speed × 1.15, 60 s
    /// once per stone per game day
    /// </code>
    /// </summary>
    public static class VishapStoneRules
    {
        public const int StoneCount = 12;
        public const float TouchReach = 4f;
        public const float SpringRadius = 8f;
        public const float SpringSeconds = 60f;
        public const float HealFractionPerSecond = 0.02f;
        public const float WakeRadius = 60f;
        public const float AwakeSeconds = 60f;
        public const float AwakeDamageFactor = 1.2f;
        public const float AwakeSpeedFactor = 1.15f;

        private const string KeyPrefix = "limiar.vishapakar.";

        private static string[] s_keys;

        /// <summary>The save key of stone <paramref name="stone"/> (a daily mark: the game day it was last touched), built once.</summary>
        public static string TouchKey(int stone)
        {
            if (s_keys == null)
            {
                var keys = new string[StoneCount];
                for (int i = 0; i < keys.Length; i++)
                {
                    keys[i] = KeyPrefix + i;
                }

                s_keys = keys;
            }

            return stone >= 0 && stone < s_keys.Length ? s_keys[stone] : KeyPrefix + stone;
        }

        /// <summary>Whether a stone last touched on <paramref name="lastDay"/> (0 = never) may be touched on <paramref name="day"/>.</summary>
        public static bool Asleep(int lastDay, int day) => day > lastDay;

        /// <summary>The stone within <see cref="TouchReach"/> of <paramref name="p"/> (the nearest), or −1.</summary>
        public static int StoneAt(IReadOnlyList<Vector2> stones, Vector2 p)
        {
            int best = -1;
            float bestSq = TouchReach * TouchReach;
            for (int i = 0; stones != null && i < stones.Count; i++)
            {
                float sq = (stones[i] - p).sqrMagnitude;
                if (sq <= bestSq)
                {
                    best = i;
                    bestSq = sq;
                }
            }

            return best;
        }

        /// <summary>Whether a spring lit at <paramref name="litAt"/> still flows at <paramref name="now"/> (seconds; NaN or −∞ = never lit).</summary>
        public static bool Flowing(float litAt, float now) => !float.IsNaN(litAt) && now >= litAt && now - litAt < SpringSeconds;

        /// <summary>Health the spring gives over <paramref name="seconds"/> to a hero <paramref name="distance"/> m from its stone.</summary>
        public static float HealAmount(float distance, float maxHealth, float seconds)
        {
            return distance <= SpringRadius && seconds > 0f ? maxHealth * HealFractionPerSecond * seconds : 0f;
        }

        /// <summary>Whether touching a stone may wake a beast <paramref name="distance"/> m from it.</summary>
        public static bool CanWake(bool isBoss, float distance) => !isBoss && distance <= WakeRadius;
    }
}
