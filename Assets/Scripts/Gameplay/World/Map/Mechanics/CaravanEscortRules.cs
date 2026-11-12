using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>What the escorted caravan is doing (<see cref="CaravanEscortRules.Step"/>).</summary>
    public enum CaravanState
    {
        Walking = 0,

        /// <summary>The hero fell behind: the caravan waits.</summary>
        Waiting = 1,

        /// <summary>An ambush at the bend: the caravan stops until the beasts fall or give up.</summary>
        Ambush = 2,

        /// <summary>Snow coming down the slope: the caravan stops for it.</summary>
        Avalanche = 3,

        Arrived = 4,
    }

    /// <summary>
    /// The escort of Ammo's caravan through the Passo de Wakhan (Docs/Mapas100.md 40: "escolta da caravana: evento de 20 min levando a caravana
    /// do Ammo de ponta a ponta (avalanches, emboscadas); recompensa grande"; "Noite: o passo fecha para caravanas"), pure and server-runnable.
    /// [E] by the caravan at the foot of the Wakhan valley (by day only) starts it: the camels walk the caravan road (~1.1 km: up the valley,
    /// through the Caravançará de Pedra, across the high Wakhan) at 3 m/s, about six minutes and more with the stops (shorter than the sheet's
    /// 20 min, for one hero). They walk only while the hero keeps within 40 m. At three bends an ambush: the beasts round it (70 m, bosses never)
    /// rush the caravan, which stands until none of them is alive within 30 m or 45 s pass. At two slopes an avalanche: 3 s of warning and the
    /// snow buries 8 m round the head of the caravan for 6 s (the hero caught in it loses 8 % of max health, never under 30 %). Falling, leaving
    /// the pass or the night ends the escort; arriving pays 120 gold nuggets, once per UTC day.
    /// <code>
    /// 3 m/s · hero ≤ 40 m · ambushes at 30 %, 55 %, 80 % of the road: beasts within 70 m, hold ≤ 45 s, clear = none alive within 30 m
    /// avalanches at 42 %, 68 %: 3 s warning + 6 s, 8 m round the head, 8 % hp (floor 30 %) · 120 gold, once per UTC day · day only
    /// </code>
    /// </summary>
    public static class CaravanEscortRules
    {
        public const float Speed = 3f;
        public const float LeashRange = 40f;
        public const float AmbushRange = 70f;
        public const float AmbushClearRange = 30f;
        public const float AmbushMaxSeconds = 45f;
        public const float AvalancheWarning = 3f;
        public const float AvalancheSeconds = 6f;
        public const float AvalancheRadius = 8f;
        public const float AvalancheHurtFraction = 0.08f;
        public const float AvalancheFloorFraction = 0.3f;
        public const int Purse = 120;
        public const float StartReach = 5f;

        /// <summary>The Teto driver's daily mark of the escort's reward (the UTC day it was last paid).</summary>
        public const string RewardKey = "teto.wakhan.caravan";

        public static readonly float[] AmbushAt = { 0.3f, 0.55f, 0.8f };
        public static readonly float[] AvalancheAt = { 0.42f, 0.68f };

        /// <summary>Whether caravans may cross the pass (day only).</summary>
        public static bool PassOpen(float daylightFactor) => daylightFactor >= 0.5f;

        /// <summary>Whether the caravan walks on: the hero close and no stop under way.</summary>
        public static bool Walks(float heroDistance, bool stopped) => !stopped && heroDistance <= LeashRange;

        /// <summary>The index of the stop (in <paramref name="marks"/>) the caravan reaches going from <paramref name="before"/> to <paramref name="after"/> (fractions of the road), or −1.</summary>
        public static int StopCrossed(float[] marks, float before, float after)
        {
            for (int i = 0; marks != null && i < marks.Length; i++)
            {
                if (before < marks[i] && after >= marks[i])
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>Whether an ambush is over: no ambusher alive near the caravan, or the time is up.</summary>
        public static bool AmbushOver(int aliveNear, float seconds) => aliveNear <= 0 || seconds >= AmbushMaxSeconds;

        /// <summary>Health the avalanche takes from a hero <paramref name="distance"/> m from the caravan's head: never under the floor.</summary>
        public static float AvalancheHurt(float distance, float current, float max)
        {
            if (distance > AvalancheRadius)
            {
                return 0f;
            }

            float room = current - max * AvalancheFloorFraction;
            return room <= 0f ? 0f : Mathf.Min(room, max * AvalancheHurtFraction);
        }

        public static bool Paid(long paidDay, long today) => paidDay == today;
    }
}
