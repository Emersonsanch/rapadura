using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    public enum AreiasStormPhase
    {
        Calm = 0,
        /// <summary>The dust wall is coming: time to run for a shelter. Never hurts.</summary>
        Warning = 1,
        /// <summary>The storm: outside a shelter the sand wears the hero down, down to the floor and never further.</summary>
        Storm = 2,
    }

    /// <summary>
    /// The sandstorm of the Areias Flamejantes (Docs/Mapas100.md 07), the same for everyone: a pure function of the clock (a server
    /// computes the same storm), no MonoBehaviour. Every 10 minutes: 9 min 05 s of calm, 15 s of warning and 40 s of storm.
    /// Numbers: the storm takes 1,5% of max health per second outside a shelter, so it can never finish a hero (the floor is 35%
    /// of max health) and a hero who ignores a whole storm loses 60%. A shelter covers 14 m round it.
    /// </summary>
    public static class AreiasStormRules
    {
        public const float CycleSeconds = 600f;
        public const float WarningSeconds = 15f;
        public const float StormSeconds = 40f;

        public const float ShelterRadius = 14f;
        public const float HurtFractionPerSecond = 0.015f;
        public const float HealthFloorFraction = 0.35f;

        /// <summary>How far the hero sees in the storm (Docs/Mapas100.md 07: "visão 6 m"): past it the beasts are lost in the sand (bosses show).</summary>
        public const float StormSightRange = 6f;

        /// <summary>The second of the cycle the warning starts.</summary>
        public const float WarningStart = CycleSeconds - StormSeconds - WarningSeconds;

        public static float SecondOfCycle(double unixSeconds)
        {
            double t = unixSeconds % CycleSeconds;
            return (float)(t < 0d ? t + CycleSeconds : t);
        }

        /// <summary>Which storm the clock is in (it changes at the end of each cycle).</summary>
        public static long StormIndex(double unixSeconds) => (long)System.Math.Floor(unixSeconds / CycleSeconds);

        public static AreiasStormPhase PhaseAt(double unixSeconds)
        {
            float t = SecondOfCycle(unixSeconds);
            return t < WarningStart ? AreiasStormPhase.Calm : t < WarningStart + WarningSeconds ? AreiasStormPhase.Warning : AreiasStormPhase.Storm;
        }

        /// <summary>
        /// The hero's sight under the storm: <see cref="StormSightRange"/> while it blows (a shelter breaks the sand, not the dark of it), no cut
        /// in the calm or the warning, in a town or a free zone, or when the hero is down (<see cref="BeastSightRules.NoLimit"/>).
        /// </summary>
        public static float SightRange(AreiasStormPhase phase, bool exempt, bool alive)
        {
            return phase == AreiasStormPhase.Storm && !exempt && alive ? StormSightRange : BeastSightRules.NoLimit;
        }

        public static bool IsSheltered(Vector2 local, IReadOnlyList<Vector2> shelters)
        {
            return NearestShelter(local, shelters, out float distance) >= 0 && distance <= ShelterRadius;
        }

        /// <summary>Index of the closest shelter (-1 when there is none) and its distance.</summary>
        public static int NearestShelter(Vector2 local, IReadOnlyList<Vector2> shelters, out float distance)
        {
            int best = -1;
            float bestSqr = float.MaxValue;
            for (int i = 0; shelters != null && i < shelters.Count; i++)
            {
                float sqr = (shelters[i] - local).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = i;
                }
            }

            distance = best >= 0 ? Mathf.Sqrt(bestSqr) : float.PositiveInfinity;
            return best;
        }

        /// <summary>The wear of <paramref name="deltaSeconds"/> of storm: never brings health under the floor, so it never kills.</summary>
        public static float WearOver(float currentHealth, float maxHealth, float deltaSeconds)
        {
            float room = currentHealth - maxHealth * HealthFloorFraction;
            return room <= 0f || deltaSeconds <= 0f ? 0f : Mathf.Min(room, maxHealth * HurtFractionPerSecond * deltaSeconds);
        }
    }
}
