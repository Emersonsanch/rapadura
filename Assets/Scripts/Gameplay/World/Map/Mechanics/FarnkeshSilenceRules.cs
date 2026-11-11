using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>How the hero moves this frame, for the noise of the hermitages (<see cref="FarnkeshSilenceRules.Rate"/>).</summary>
    public enum FarnkeshPace
    {
        Still = 0,

        /// <summary>Walking or crouching (under <see cref="FarnkeshSilenceRules.RunSpeed"/>).</summary>
        Walking = 1,
        Running = 2,
        Mounted = 3,
    }

    /// <summary>Where the echo is (<see cref="FarnkeshSilenceRules.EchoPhaseAt"/>).</summary>
    public enum FarnkeshEchoPhase
    {
        None = 0,

        /// <summary>The meter is full: the echo has marked the ground where the hero stood.</summary>
        Marked = 1,

        /// <summary>The echo strikes the mark (one frame's verdict).</summary>
        Strike = 2,
    }

    /// <summary>
    /// The silence of the Ermidas Suspensas (Docs/Mapas100.md 56: "correr e lutar enchem um medidor de barulho; cheio, os Ecos aparecem. Andar
    /// devagar e usar skills silenciosas (Lavine) ajuda"; "Noite: o medidor enche mais devagar"), pure. Over the cliff of the hermitages and the
    /// Pináculo do Sino (outside the post's safe ground) every step is heard: running, riding and every blow given or taken fill a meter, walking
    /// or standing empties it. When it is full an Echo answers: it marks the ground where the hero stands and strikes the mark 1.5 s later, so a
    /// hero who heard it steps aside; then the meter falls back to half. Lavine moves quietly (her gains count 60 %), and at night the hermits
    /// sleep (60 % again; both together 36 %).
    /// <code>
    /// meter 0–100 · still −15/s · walking (&lt; 3.2 m/s) −6/s · running +8/s · mounted +14/s · each blow given or taken +3
    /// Lavine ×0.6 and night ×0.6 on what fills it · full → the Echo marks the hero's spot (radius 3.5 m) and strikes 1.5 s later
    /// the strike: 12 % of max life, never under 25 % · after it the meter is 50 · warned at 60 % and 85 %
    /// running from empty: full in 12.5 s (≈ 69 m); then an Echo every 6.25 s while the hero keeps running
    /// </code>
    /// </summary>
    public static class FarnkeshSilenceRules
    {
        public const float Max = 100f;
        public const float StillRate = -15f;
        public const float WalkRate = -6f;
        public const float RunRate = 8f;
        public const float MountedRate = 14f;
        public const float HitNoise = 3f;
        public const float QuietFactor = 0.6f;
        public const float StillSpeed = 0.3f;
        public const float RunSpeed = 3.2f;
        public const float EchoRadius = 3.5f;
        public const float EchoDelay = 1.5f;
        public const float EchoFraction = 0.12f;
        public const float EchoFloorFraction = 0.25f;
        public const float AfterEcho = 50f;
        public const float FirstWarning = 60f;
        public const float SecondWarning = 85f;

        /// <summary>How the hero moves at <paramref name="speed"/> metres a second.</summary>
        public static FarnkeshPace PaceOf(float speed, bool mounted)
        {
            if (mounted && speed >= StillSpeed)
            {
                return FarnkeshPace.Mounted;
            }

            if (speed < StillSpeed)
            {
                return FarnkeshPace.Still;
            }

            return speed < RunSpeed ? FarnkeshPace.Walking : FarnkeshPace.Running;
        }

        /// <summary>The factor on what fills the meter: Lavine and the night each make it 60 %.</summary>
        public static float Quiet(bool lavine, bool night) => (lavine ? QuietFactor : 1f) * (night ? QuietFactor : 1f);

        /// <summary>Points a second the meter moves at <paramref name="pace"/> (the quiet factor only on what fills it).</summary>
        public static float Rate(FarnkeshPace pace, float quiet)
        {
            switch (pace)
            {
                case FarnkeshPace.Still: return StillRate;
                case FarnkeshPace.Walking: return WalkRate;
                case FarnkeshPace.Running: return RunRate * quiet;
                default: return MountedRate * quiet;
            }
        }

        /// <summary>The meter after <paramref name="dt"/> seconds at <paramref name="pace"/> and <paramref name="hits"/> blows, kept in 0–100.</summary>
        public static float Step(float meter, FarnkeshPace pace, int hits, float quiet, float dt) =>
            Mathf.Clamp(meter + Rate(pace, quiet) * Mathf.Max(0f, dt) + Mathf.Max(0, hits) * HitNoise * quiet, 0f, Max);

        public static bool Full(float meter) => meter >= Max;

        /// <summary>
        /// Where the Echo is <paramref name="sinceMark"/> seconds after it marked the ground (negative = no mark): marked until
        /// <see cref="EchoDelay"/>, then the strike.
        /// </summary>
        public static FarnkeshEchoPhase EchoPhaseAt(float sinceMark)
        {
            if (sinceMark < 0f)
            {
                return FarnkeshEchoPhase.None;
            }

            return sinceMark < EchoDelay ? FarnkeshEchoPhase.Marked : FarnkeshEchoPhase.Strike;
        }

        /// <summary>Whether the strike at <paramref name="mark"/> reaches the hero at <paramref name="hero"/>.</summary>
        public static bool StrikeHits(Vector2 mark, Vector2 hero) => (hero - mark).sqrMagnitude <= EchoRadius * EchoRadius;

        /// <summary>The strike: a slice of max life that never takes the hero under <see cref="EchoFloorFraction"/>.</summary>
        public static float StrikeAmount(float current, float max) => ProfundoHazardRules.Bite(current, max, EchoFraction, EchoFloorFraction);

        /// <summary>The warning the meter crossed going from <paramref name="before"/> to <paramref name="after"/>: 2 (85 %), 1 (60 %) or 0.</summary>
        public static int WarningCrossed(float before, float after)
        {
            if (before < SecondWarning && after >= SecondWarning)
            {
                return 2;
            }

            return before < FirstWarning && after >= FirstWarning ? 1 : 0;
        }
    }
}
