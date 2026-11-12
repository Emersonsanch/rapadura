using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>How cold the hero is, as a band of the meter (the driver speaks when it goes up).</summary>
    public enum GlacierColdBand
    {
        Warm,
        Chilly,
        Frozen,
    }

    /// <summary>
    /// The cold of the Geleira do Teto do Mundo (Docs/Mapas100.md 38: "frio: medidor de frio fora dos abrigos (lentidão leve quando cheio);
    /// fogueiras e chá de karkadé zeram"; "Noite: … frio dobra"), pure and server-runnable. On the glacier and the serac field (the map's two
    /// zones), off the Cabana do Iaque, the meter fills 1 per second (full in 100 s), twice as fast at night. At a herders' fire (7 m) or in the
    /// Cabana it is zero at once; out of the map it falls 10 per second. With Karkadé Tea active it fills at half speed (the tea is cooked in a
    /// town and lasts a while, so a zeroing sip could never happen up here: the same choice as the heat of the Caldeira). Warning at 60; full,
    /// the hero walks 15 % slower until a fire or the way down. It never hurts.
    /// <code>
    /// meter 0–100 · +1/s on the glacier (night ×2, tea ×0.5) · fire 7 m or the Cabana → 0 · off the map −10/s · 60 warns · 100: speed × 0.85
    /// </code>
    /// </summary>
    public static class GlacierColdRules
    {
        public const float MaxCold = 100f;
        public const float GainPerSecond = 1f;
        public const float NightFactor = 2f;
        public const float TeaFactor = 0.5f;
        public const float WarmPerSecond = 10f;
        public const float FireRadius = 7f;
        public const float ChillyAt = 60f;
        public const float FrozenMoveFactor = 0.85f;

        /// <summary>The PercentAdditive value of the frozen slow on MoveSpeed (−0.15).</summary>
        public static float FrozenMoveBonus => FrozenMoveFactor - 1f;

        /// <summary>The meter after <paramref name="seconds"/>.</summary>
        public static float Step(float cold, bool onGlacier, bool atShelter, bool night, bool tea, float seconds)
        {
            if (atShelter)
            {
                return 0f;
            }

            if (!onGlacier)
            {
                return Mathf.Max(0f, cold - WarmPerSecond * seconds);
            }

            float gain = GainPerSecond * (night ? NightFactor : 1f) * (tea ? TeaFactor : 1f);
            return Mathf.Clamp(cold + gain * seconds, 0f, MaxCold);
        }

        public static GlacierColdBand BandOf(float cold) => cold >= MaxCold ? GlacierColdBand.Frozen : cold >= ChillyAt ? GlacierColdBand.Chilly : GlacierColdBand.Warm;

        /// <summary>Whether <paramref name="p"/> is by one of the <paramref name="fires"/>.</summary>
        public static bool AtFire(IReadOnlyList<Vector2> fires, Vector2 p)
        {
            for (int i = 0; fires != null && i < fires.Count; i++)
            {
                if ((fires[i] - p).sqrMagnitude <= FireRadius * FireRadius)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
