using System;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>What the valley's wind (the four vanes of the fort) does to one gust of O Senhor do Cata-Vento (<see cref="VaneValleyWindRules.EffectOn"/>).</summary>
    public enum VaneValleyEffect
    {
        /// <summary>No wind to speak of, or it blows across the gust: the gust is as it always was.</summary>
        None = 0,

        /// <summary>The valley blows against the gust and breaks it: nobody is hit.</summary>
        Broken = 1,

        /// <summary>The valley blows with the gust and carries it further (<see cref="VaneValleyWindRules.CarriedReachFactor"/>).</summary>
        Carried = 2,
    }

    /// <summary>
    /// The four vanes of the Forte dos Ventos changing the wind of O Senhor do Cata-Vento's fight (Docs/Mapas100.md 39: "girar os 4 cata-ventos
    /// muda o vento do mapa"; "Falta: os 4 cata-ventos do mapa mudando o vento"), pure and server-runnable. The valley's wind is
    /// <see cref="WindVaneRules.Wind"/> of the vanes as the save keeps them (the same for the walk and for the fight). The boss still turns his
    /// own wind a quarter each gust (north, east, south, west); when the valley blows at least half strength:
    /// <code>
    /// valley wind ≥ 0.5 (three vanes one way, or all four) · gust against it (within 60° of the opposite): broken, nobody is hit
    /// gust with it (within 60°): carried, reach 16 m → 20 m · across it, or the valley under 0.5: the gust as always
    /// </code>
    /// Decision (2026-11-23): the vanes are a preparation, not a switch: with them aligned one gust in four breaks and one in four carries
    /// further, so the cousin who turned them knows which side to fear; left as the boss left them (one each way) nothing changes.
    /// </summary>
    public static class VaneValleyWindRules
    {
        public const float MinStrength = 0.5f;
        public const float AlongDot = 0.5f;
        public const float CarriedReachFactor = 1.25f;

        /// <summary>The save mark of the first gust the valley broke (the cousin's line, once).</summary>
        public const string FirstBrokenKey = "teto.vane.boss_broken";

        /// <summary>The valley's wind from the vanes' save marks (<paramref name="markOf"/> reads a time mark; null = never turned).</summary>
        public static Vector2 ValleyWind(Func<string, long> markOf)
        {
            var states = new int[WindVaneRules.VaneCount];
            for (int i = 0; i < states.Length; i++)
            {
                long mark = markOf != null ? markOf(WindVaneRules.StateKey(i)) : 0L;
                states[i] = WindVaneRules.StateFrom(mark, i);
            }

            return WindVaneRules.Wind(states);
        }

        /// <summary>What a valley wind of <paramref name="valleyWind"/> does to a gust blowing toward <paramref name="gustToward"/>.</summary>
        public static VaneValleyEffect EffectOn(Vector2 gustToward, Vector2 valleyWind)
        {
            float strength = valleyWind.magnitude;
            if (strength < MinStrength - 1e-4f || gustToward.sqrMagnitude < 1e-6f)
            {
                return VaneValleyEffect.None;
            }

            float dot = Vector2.Dot(gustToward.normalized, valleyWind / strength);
            if (dot <= -AlongDot)
            {
                return VaneValleyEffect.Broken;
            }

            return dot >= AlongDot ? VaneValleyEffect.Carried : VaneValleyEffect.None;
        }

        /// <summary>The factor on the gust's reach: 0 broken, <see cref="CarriedReachFactor"/> carried, 1 otherwise.</summary>
        public static float ReachFactor(VaneValleyEffect effect)
        {
            switch (effect)
            {
                case VaneValleyEffect.Broken: return 0f;
                case VaneValleyEffect.Carried: return CarriedReachFactor;
                default: return 1f;
            }
        }

        /// <summary>
        /// Does the gust (from the boss at <paramref name="vane"/>, blowing toward <paramref name="gustToward"/>, base reach <paramref name="reach"/>
        /// and lee <paramref name="lee"/>) reach a hero at <paramref name="hero"/> under <paramref name="effect"/>? With <see cref="VaneValleyEffect.None"/>
        /// it is exactly the boss's own rule: within reach and downwind (or no more than the lee upwind of his line).
        /// </summary>
        public static bool GustHits(VaneValleyEffect effect, Vector2 gustToward, Vector2 vane, Vector2 hero, float reach, float lee)
        {
            float r = reach * ReachFactor(effect);
            if (r <= 0f)
            {
                return false;
            }

            Vector2 offset = hero - vane;
            if (offset.sqrMagnitude > r * r)
            {
                return false;
            }

            return Vector2.Dot(offset, gustToward) >= -lee;
        }
    }
}
