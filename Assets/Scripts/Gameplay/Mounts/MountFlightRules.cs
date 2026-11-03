using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// How a flyer (<see cref="MountType.Voador"/>) looks in the air, pure and EditMode-tested. The flight is visual only
    /// (Docs/Mounts.md §2.6): the hero's CharacterController, click-to-move, collisions and the camera keep the ground;
    /// <see cref="Visuals.MountActor"/> lifts only the model (and the rider's visual, which sits on its saddle) by
    /// <see cref="HoverHeight"/> and beats the wings by <see cref="WingAngle"/>.
    /// <list type="bullet">
    /// <item>Standing: hovers <see cref="IdleHover"/> m (the doc's "flutua 1 m acima do chão").</item>
    /// <item>Moving: rises toward <see cref="CruiseHover"/> m with the speed.</item>
    /// <item>City / house (<see cref="MountPlace.SafeZone"/>, <see cref="MountPlace.Indoors"/>): glides low, <see cref="LowHover"/> m (§9.2: no flying into safe zones).</item>
    /// <item>"Voo da Coluna" (G): an arc up to <see cref="SoarPeak"/> m over the flight (<see cref="SoarLift"/>).</item>
    /// </list>
    /// "Reduzir Movimento" halves the bob and the wing beat (<see cref="MotionScale"/>), never the height.
    /// </summary>
    public static class MountFlightRules
    {
        public const float IdleHover = 1.0f;
        public const float CruiseHover = 1.35f;
        public const float LowHover = 0.3f;
        public const float SoarPeak = 2.6f;

        /// <summary>Hover bob amplitude (m) at full motion.</summary>
        public const float BobAmplitude = 0.08f;

        /// <summary>Wing beat (degrees up/down from level) standing, cruising and soaring.</summary>
        public const float IdleFlap = 34f;
        public const float CruiseFlap = 46f;
        public const float SoarFlap = 55f;

        /// <summary>Wing beats per second standing, cruising and soaring.</summary>
        public const float IdleBeatsPerSecond = 1.4f;
        public const float CruiseBeatsPerSecond = 2.4f;
        public const float SoarBeatsPerSecond = 3.2f;

        /// <summary>The outer wing follows the inner one this fraction of a beat later (the flexible tip).</summary>
        public const float TipLag = 0.12f;

        /// <summary>Front and hind legs folded under the body in the air (degrees of pitch on the hip).</summary>
        public const float FrontLegTuck = -55f;
        public const float HindLegTuck = 60f;

        /// <summary>How fast the height eases to its target (1/s).</summary>
        public const float HeightEase = 3.5f;

        /// <summary>Motion scale for "Reduzir Movimento".</summary>
        public static float MotionScale(bool reducedMotion) => reducedMotion ? 0.5f : 1f;

        /// <summary>
        /// Height (m) of the model above the ground. <paramref name="speed01"/> 0..1 of the top ride speed;
        /// <paramref name="soar01"/> 0..1 of the "Voo da Coluna" (outside of it: anything out of range = no soar).
        /// </summary>
        public static float HoverHeight(bool flightAllowed, float speed01, float soar01 = -1f)
        {
            if (!flightAllowed)
            {
                return LowHover;
            }

            float hover = Mathf.Lerp(IdleHover, CruiseHover, Mathf.Clamp01(speed01));
            return hover + SoarLift(soar01);
        }

        /// <summary>Extra height of the soar at <paramref name="t01"/> of the flight: a smooth arc, 0 at both ends (and outside 0..1).</summary>
        public static float SoarLift(float t01)
        {
            if (t01 <= 0f || t01 >= 1f)
            {
                return 0f;
            }

            return Mathf.Sin(Mathf.PI * t01) * SoarPeak;
        }

        /// <summary>Wing beats per second.</summary>
        public static float BeatsPerSecond(float speed01, bool soaring)
        {
            return soaring ? SoarBeatsPerSecond : Mathf.Lerp(IdleBeatsPerSecond, CruiseBeatsPerSecond, Mathf.Clamp01(speed01));
        }

        /// <summary>Peak wing angle (degrees), scaled by <see cref="MotionScale"/>.</summary>
        public static float FlapAmplitude(float speed01, bool soaring, bool reducedMotion)
        {
            float amplitude = soaring ? SoarFlap : Mathf.Lerp(IdleFlap, CruiseFlap, Mathf.Clamp01(speed01));
            return amplitude * MotionScale(reducedMotion);
        }

        /// <summary>
        /// Wing angle (degrees, + = up) at <paramref name="beatPhase"/> (in beats: 1.0 = one full beat). The upstroke is
        /// a little slower than the downstroke, like a real bird; the wings rest slightly raised (a glide dihedral).
        /// </summary>
        public static float WingAngle(float beatPhase, float amplitude)
        {
            float u = beatPhase - Mathf.Floor(beatPhase);
            // Up for 60% of the beat, down for 40%.
            float shaped = u < 0.6f ? -Mathf.Cos(Mathf.PI * u / 0.6f) : Mathf.Cos(Mathf.PI * (u - 0.6f) / 0.4f);
            return 6f + shaped * amplitude;
        }

        /// <summary>Whether the downstroke started between <paramref name="previousPhase"/> and <paramref name="phase"/> (the "whoomp" of the beat).</summary>
        public static bool DownstrokeStarted(float previousPhase, float phase)
        {
            if (phase <= previousPhase)
            {
                return false;
            }

            float mark = Mathf.Floor(previousPhase) + 0.6f;
            if (mark <= previousPhase)
            {
                mark += 1f;
            }

            return phase >= mark;
        }

        /// <summary>Hover bob (m) at <paramref name="beatPhase"/>: the body rises a little on each downstroke.</summary>
        public static float Bob(float beatPhase, bool reducedMotion)
        {
            return Mathf.Sin(2f * Mathf.PI * beatPhase) * BobAmplitude * MotionScale(reducedMotion);
        }

        /// <summary>Whether a flyer may fly high in <paramref name="place"/> (cities and houses: low glide only).</summary>
        public static bool FlightAllowed(MountPlace place) => place != MountPlace.SafeZone && place != MountPlace.Indoors;

        /// <summary>"Voo da Coluna": where the flight lands (<paramref name="distance"/> m ahead on the ground plane).</summary>
        public static Vector3 FlightLanding(Vector3 from, Vector3 forward, float distance)
        {
            forward.y = 0f;
            if (forward.sqrMagnitude < 1e-6f)
            {
                forward = Vector3.forward;
            }

            return from + forward.normalized * Mathf.Max(0f, distance);
        }
    }
}
