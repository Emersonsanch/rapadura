using Rapadura.Gameplay.Enemies;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Two fires of the Caldeira do Fogo Devorador on the heat meter (Docs/Mapas100.md 70: "Noite: ... a lava sobe um pouco"; "o Glutão de Brasa,
    /// fogo sem rosto que come o próprio fogo das forjas ... Recua quando as forjas de Hinokura ficam acesas"), pure and server-runnable, on top
    /// of <see cref="HinokuraHeatRules"/>. At night the lava lake creeps up its shore, so its heat starts further out. The Glutão is fire too: next
    /// to him the meter climbs even far from the lake, where there are no cold stones, so a cousin who fights him must step out to cool; every
    /// forge lit in Hinokura is fire he cannot eat, and his heat falls with each.
    /// <code>
    /// night: the lake's edge 8 m nearer (heat from 98 m, 3/s from 8 m out)
    /// the Glutão: 5/s within 2 m of him, fading to 0 at 24 m · ×(1 − 0.25 per lit forge): 3 forges → 1.25/s
    /// both add up · Karkadé Tea halves the sum · cold stone zeroes · nothing near → falls 12/s
    /// next to him with no forge lit: full in ~20 s (then the meter's burn, never under 20 %); 24 m out it falls 12/s
    /// </code>
    /// Decision (2026-11-23): the fight was the same as any world boss' because the arena is 260 m from the lava; now the meter is the fight's
    /// rhythm (in, out, in) and lighting the forges before going is worth it twice.
    /// </summary>
    public static class CalderaFireRules
    {
        public const string GluttonEnemyId = RegionBossRules.EmberGluttonId;
        public const float NightRise = 8f;
        public const float GluttonReach = 24f;
        public const float GluttonBody = 2f;
        public const float GluttonMaxGain = 5f;
        public const float CoolPerForge = 0.25f;
        public const int Forges = 3;

        /// <summary>The save mark of the cousin's line the first time the Glutão's own heat is felt (set once, on that event).</summary>
        public const string FirstGluttonHeatKey = "hinokura.glutton.first_heat";

        /// <summary>The distance to the lava's edge the meter uses: the lake creeps <see cref="NightRise"/> m up at night.</summary>
        public static float LakeDistance(float raw, bool night) => night ? raw - NightRise : raw;

        /// <summary>Points a second the lake gives at <paramref name="distance"/> m from its edge (0 out of range).</summary>
        public static float LakeGain(float distance)
        {
            if (distance >= HinokuraHeatRules.HeatRange)
            {
                return 0f;
            }

            return HinokuraHeatRules.MaxGainPerSecond * (1f - Mathf.Clamp01(distance / HinokuraHeatRules.HeatRange));
        }

        /// <summary>Points a second the Glutão gives at <paramref name="distance"/> m with <paramref name="litForges"/> forges lit (0 out of reach).</summary>
        public static float GluttonGain(float distance, int litForges)
        {
            if (distance >= GluttonReach)
            {
                return 0f;
            }

            float fade = 1f - Mathf.Clamp01((distance - GluttonBody) / (GluttonReach - GluttonBody));
            float forges = Mathf.Clamp01(1f - CoolPerForge * Mathf.Clamp(litForges, 0, Forges));
            return GluttonMaxGain * fade * forges;
        }

        /// <summary>
        /// The meter after <paramref name="seconds"/>: zero on a cold stone; the lake's and the Glutão's gains added (halved by the tea); with neither,
        /// it falls at <see cref="HinokuraHeatRules.CoolPerSecond"/>. With no Glutão near this is <see cref="HinokuraHeatRules.Step"/> exactly.
        /// </summary>
        public static float Step(float heat, float lakeDistance, float gluttonGain, bool onColdStone, bool tea, float seconds)
        {
            if (onColdStone)
            {
                return 0f;
            }

            bool lakeNear = lakeDistance < HinokuraHeatRules.HeatRange;
            if (!lakeNear && gluttonGain <= 0f)
            {
                return Mathf.Max(0f, heat - HinokuraHeatRules.CoolPerSecond * seconds);
            }

            float gain = (LakeGain(lakeDistance) + Mathf.Max(0f, gluttonGain)) * (tea ? HinokuraHeatRules.TeaFactor : 1f);
            return Mathf.Min(HinokuraHeatRules.MaxHeat, heat + gain * seconds);
        }
    }
}
