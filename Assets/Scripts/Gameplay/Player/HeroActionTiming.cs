using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Shared timing contract between the hero action clips (baked into the AnimatorControllers by
    /// the editor tool <c>Rapadura/Animation/Rebuild Hero Animators</c>) and the gameplay code that
    /// has to agree with them: the basic attack lands its damage <see cref="StrikeDelay"/> seconds
    /// after the swing starts, and the rebuilder speeds each clip up so its visual peak (fastest
    /// hand for attacks, the release for casts) falls exactly there. Pure, EditMode-tested.
    /// </summary>
    public static class HeroActionTiming
    {
        /// <summary>Seconds from attack input to the hit (keeps mobile input feeling immediate while still showing a windup).</summary>
        public const float StrikeDelay = 0.22f;

        /// <summary>Seconds from cast input to the visual release.</summary>
        public const float CastReleaseDelay = 0.25f;

        /// <summary>Playback speed bounds for one-shot action clips (mocap is 30 fps real-time; combat needs it snappier).</summary>
        public const float MinActionSpeed = 1f;
        public const float MaxActionSpeed = 6f;

        /// <summary>How long (s) of a clip to keep after its peak before blending back, at playback 1.</summary>
        public const float RecoverSeconds = 0.45f;

        /// <summary>
        /// Playback speed that puts a clip's measured peak at <paramref name="targetDelay"/> seconds
        /// of real time, clamped to [<see cref="MinActionSpeed"/>, <see cref="MaxActionSpeed"/>].
        /// A peak at or before the target (or unmeasured) plays at 1.
        /// </summary>
        public static float PlaybackSpeedForPeak(float peakTime, float targetDelay)
        {
            if (peakTime <= 0f || targetDelay <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp(peakTime / targetDelay, MinActionSpeed, MaxActionSpeed);
        }

        /// <summary>Real-time seconds until the peak once the clip plays at <paramref name="speed"/>.</summary>
        public static float RealPeakTime(float peakTime, float speed)
        {
            return speed > 0f ? peakTime / speed : peakTime;
        }

        /// <summary>
        /// Normalised exit time (0..1) of a one-shot clip: just after the peak plus a short recovery,
        /// so the hero does not finish a two-second mocap follow-through for every 0.5 s attack.
        /// </summary>
        public static float ExitTime(float clipLength, float peakTime, float speed)
        {
            if (clipLength <= 0f)
            {
                return 1f;
            }

            float keep = peakTime + RecoverSeconds * Mathf.Max(1f, speed);
            return Mathf.Clamp(keep / clipLength, 0.2f, 1f);
        }

        /// <summary>
        /// Index of the sample where a tracked point moves fastest (the "strike" of an attack or the
        /// "release" of a cast), from positions sampled at a fixed <paramref name="interval"/>.
        /// Ignores the first and last 10% of the clip, where mocap actors settle into/out of the move.
        /// Returns -1 when there are too few samples.
        /// </summary>
        public static int PeakSpeedSample(Vector3[] positions, float interval)
        {
            if (positions == null || positions.Length < 3 || interval <= 0f)
            {
                return -1;
            }

            int start = Mathf.Max(1, Mathf.RoundToInt(positions.Length * 0.1f));
            int end = Mathf.Min(positions.Length - 1, Mathf.RoundToInt(positions.Length * 0.9f));
            int best = -1;
            float bestSpeed = -1f;
            for (int i = start; i <= end; i++)
            {
                float speed = (positions[i] - positions[i - 1]).magnitude / interval;
                if (speed > bestSpeed)
                {
                    bestSpeed = speed;
                    best = i;
                }
            }

            return best;
        }
    }
}
