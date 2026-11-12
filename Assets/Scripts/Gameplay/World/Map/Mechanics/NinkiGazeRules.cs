using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    public enum NinkiGlowPhase
    {
        Dim = 0,

        /// <summary>The calf starts to shine: time to turn away.</summary>
        Warning = 1,

        /// <summary>The calf shines: whoever looks at it is slowed.</summary>
        Glowing = 2,
    }

    /// <summary>
    /// The calves' glow of the Charco do Ninki Nanka (Docs/Mapas100.md 34: "não olhe: quando a dragoa (ou um filhote) brilha, quem está de frente
    /// para ela fica lento; virar de costas evita. Ela fica com vergonha"), pure and server-runnable. Every 12 s each Ninki Nanka calf shines:
    /// 1.2 s of warning, then 1 s of glow, each calf on its own beat. A hero within 14 m who faces it during the glow (within 60° either side of
    /// where the hero looks) is slowed 40 % for 3 s, once per glow; turning the back or the side saves, and pressed against it (under 1.5 m)
    /// there is nothing to look at. The dragon herself keeps her own blow (the map boss).
    /// <code>
    /// cycle 12 s: 9.8 dim · 1.2 warning · 1 glow · facing within 60° and 1.5–14 m → speed × 0.6 for 3 s, once per glow
    /// </code>
    /// </summary>
    public static class NinkiGazeRules
    {
        public const float CycleSeconds = 12f;
        public const float WarningSeconds = 1.2f;
        public const float GlowSeconds = 1f;
        public const float Reach = 14f;
        public const float MinReach = 1.5f;
        public const float HalfAngle = 60f;
        public const float SlowFactor = 0.6f;
        public const float SlowSeconds = 3f;

        public const float WarningStart = CycleSeconds - GlowSeconds - WarningSeconds;

        /// <summary>The calf's own beat: an offset in [0, cycle) from a stable number of it (its spawn id).</summary>
        public static float Offset(int seed) => Mathf.Repeat(Mathf.Abs(seed * 0.6180339f) * CycleSeconds, CycleSeconds);

        public static NinkiGlowPhase PhaseAt(double seconds, float offset)
        {
            double t = (seconds + offset) % CycleSeconds;
            t = t < 0d ? t + CycleSeconds : t;
            return t < WarningStart ? NinkiGlowPhase.Dim : t < WarningStart + WarningSeconds ? NinkiGlowPhase.Warning : NinkiGlowPhase.Glowing;
        }

        /// <summary>Which glow of the calf the clock is in (once-per-glow bookkeeping).</summary>
        public static long GlowIndex(double seconds, float offset) => (long)System.Math.Floor((seconds + offset) / CycleSeconds);

        /// <summary>Whether a hero at <paramref name="hero"/> looking along <paramref name="heroForward"/> (x/z) faces a calf at <paramref name="calf"/>.</summary>
        public static bool Facing(Vector2 hero, Vector2 heroForward, Vector2 calf)
        {
            Vector2 to = calf - hero;
            float distance = to.magnitude;
            if (distance < MinReach || distance > Reach || heroForward.sqrMagnitude < 1e-6f)
            {
                return false;
            }

            return Vector2.Angle(heroForward, to) <= HalfAngle;
        }

        /// <summary>The PercentAdditive value of the slow on MoveSpeed (−0.4).</summary>
        public static float SlowBonus => SlowFactor - 1f;
    }
}
