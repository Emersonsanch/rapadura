using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Where the hall stands in its tremor cycle (<see cref="TagesTremorRules.PhaseAt"/>).</summary>
    public enum TagesTremorPhase
    {
        Still = 0,

        /// <summary>The floor starts to hum: a few seconds to get to a lamp.</summary>
        Warning = 1,
    }

    /// <summary>
    /// The night tremor of the Salão de Tages (Docs/Mapas100.md 20: "Noite: igual; o salão 'suspira' e as lâmpadas tremem"), pure and
    /// server-runnable, on top of <see cref="TagesDarkRules"/>. Only at night (the game clock), every 150 s of the UTC clock (the same moment for
    /// every player), after 5 s of warning, the hall shakes: every lamp still burning takes 90 s off its light, unless a hero stands within 8 m
    /// of it holding it steady. A lamp shaken past its light goes out (and the beasts round it turn bold). Lamps that are out stay out.
    /// <code>
    /// night only · cycle 150 s · warning the last 5 s · at the tremor: a burning lamp with no hero within 8 m → lit mark − 90 s
    /// the save: only the shaken lamps' time marks, written only at the tremor (the hero must be in the hall to hear it; entering the hall never
    /// replays a tremor that already passed)
    /// </code>
    /// Decision (2026-11-23): by day the lamps burn their 5 min as before; at night the hero who wants the hall calm has to keep moving
    /// between lamps or guard one. Maria still lights from 6 m but steadies from 8 m like everyone (she is the light, not the ground).
    /// </summary>
    public static class TagesTremorRules
    {
        public const long CycleSeconds = 150L;
        public const long WarningSeconds = 5L;
        public const long ShakeSeconds = 90L;
        public const float SteadyReach = 8f;

        /// <summary>Which tremor of the clock the second <paramref name="now"/> follows (the tremor of index n happens at n × cycle).</summary>
        public static long TremorIndex(long now) => now / CycleSeconds;

        public static TagesTremorPhase PhaseAt(long now) =>
            now % CycleSeconds >= CycleSeconds - WarningSeconds ? TagesTremorPhase.Warning : TagesTremorPhase.Still;

        /// <summary>Seconds to the next tremor.</summary>
        public static long SecondsToTremor(long now) => CycleSeconds - now % CycleSeconds;

        /// <summary>Whether a tremor happened since the one of index <paramref name="lastSeen"/> (heard on entering the hall) and shakes now: night only.</summary>
        public static bool Due(long lastSeen, long now, bool night) => night && TremorIndex(now) > lastSeen;

        /// <summary>Whether a hero stands close enough to hold the lamp at <paramref name="lamp"/> steady.</summary>
        public static bool Steadied(Vector2 lamp, IReadOnlyList<Vector2> heroes)
        {
            for (int i = 0; heroes != null && i < heroes.Count; i++)
            {
                if ((heroes[i] - lamp).sqrMagnitude <= SteadyReach * SteadyReach)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The lamp's lit mark after the tremor at <paramref name="tremorAt"/>: burning and not steadied → 90 s less light.</summary>
        public static long Shaken(long litAt, long tremorAt, bool steadied)
        {
            if (steadied || !TagesDarkRules.Burning(litAt, tremorAt))
            {
                return litAt;
            }

            // Never back to 0 (0 means "never lit" in the save): a lamp shaken out keeps a mark in the past.
            return System.Math.Max(1L, litAt - ShakeSeconds);
        }
    }
}
