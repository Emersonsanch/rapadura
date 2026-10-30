using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Pure math for foot plants (heel strikes): finding them in a sampled locomotion clip (editor bake,
    /// <c>Rapadura/Animation/Rebuild Hero Animators</c>), aligning the phases of the blend-tree children so
    /// both feet hit the ground together while Walk/Jog/Run cross-fade, and firing footsteps at runtime
    /// (<see cref="PlayerFootsteps"/>) from the baked phases, from the animated feet, or from the rigid
    /// step phase. No Unity objects, EditMode-tested (<c>FootstepMathTests</c>).
    /// </summary>
    public static class FootstepMath
    {
        /// <summary>Share of a foot's height range (above its lowest point) still counted as "on the ground".</summary>
        public const float StanceHeightFraction = 0.3f;

        /// <summary>
        /// Normalized time (0..1) of a foot's plant in one loop of a clip, from per-sample foot positions in
        /// the model's space with the hips' horizontal position removed (+Z = forward, Y = up), sampled at a
        /// fixed interval over the whole clip (the last sample is the loop point, the same pose as the first).
        /// The plant is the first grounded sample of the stance that holds the foot's most forward point (an
        /// in-place clip strikes the heel in front of the hips, then the foot slides back under the body).
        /// Returns -1 for unusable input.
        /// </summary>
        public static float PlantPhase(Vector3[] footRelativeToHips)
        {
            if (footRelativeToHips == null || footRelativeToHips.Length < 5)
            {
                return -1f;
            }

            int count = footRelativeToHips.Length - 1; // the last sample repeats the first (loop)
            int front = 0;
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            for (int i = 0; i < count; i++)
            {
                Vector3 p = footRelativeToHips[i];
                if (p.z > footRelativeToHips[front].z)
                {
                    front = i;
                }

                minY = Mathf.Min(minY, p.y);
                maxY = Mathf.Max(maxY, p.y);
            }

            float range = maxY - minY;
            if (range < 1e-4f)
            {
                return (float)front / count;
            }

            float limit = minY + range * StanceHeightFraction;
            int plant = front;
            if (footRelativeToHips[front].y <= limit)
            {
                // Already down at its most forward point: walk back to where this stance began.
                for (int k = 1; k < count / 2; k++)
                {
                    int previous = Wrap(front - k, count);
                    if (footRelativeToHips[previous].y > limit)
                    {
                        break;
                    }

                    plant = previous;
                }
            }
            else
            {
                // Still coming down: the plant is the first grounded sample after it.
                for (int k = 1; k < count; k++)
                {
                    int next = Wrap(front + k, count);
                    if (footRelativeToHips[next].y <= limit)
                    {
                        plant = next;
                        break;
                    }
                }
            }

            return (float)plant / count;
        }

        /// <summary>
        /// How many strides (left+right step pairs) a looping clip holds: the number of separate stances of
        /// one foot (runs of grounded samples, see <see cref="StanceHeightFraction"/>), at least 1.
        /// </summary>
        public static int StrideCount(Vector3[] footRelativeToHips)
        {
            if (footRelativeToHips == null || footRelativeToHips.Length < 5)
            {
                return 1;
            }

            int count = footRelativeToHips.Length - 1;
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            for (int i = 0; i < count; i++)
            {
                minY = Mathf.Min(minY, footRelativeToHips[i].y);
                maxY = Mathf.Max(maxY, footRelativeToHips[i].y);
            }

            float range = maxY - minY;
            if (range < 1e-4f)
            {
                return 1;
            }

            // A little hysteresis so mocap jitter at the threshold doesn't split one stance in two.
            float down = minY + range * StanceHeightFraction;
            float up = minY + range * (StanceHeightFraction + 0.15f);
            int start = 0;
            for (int i = 0; i < count; i++)
            {
                if (footRelativeToHips[i].y > up)
                {
                    start = i;
                    break;
                }
            }

            int stances = 0;
            bool grounded = false;
            for (int k = 0; k < count; k++)
            {
                float y = footRelativeToHips[Wrap(start + k, count)].y;
                if (!grounded && y <= down)
                {
                    grounded = true;
                    stances++;
                }
                else if (grounded && y > up)
                {
                    grounded = false;
                }
            }

            return Mathf.Max(1, stances);
        }

        /// <summary>
        /// Blend-tree <c>cycleOffset</c> (0..1) for a child whose feet plant at (<paramref name="childLeft"/>,
        /// <paramref name="childRight"/>) so they land when the reference child's (Walk) do, at
        /// (<paramref name="referenceLeft"/>, <paramref name="referenceRight"/>): the circular mean of both
        /// feet's phase differences. A missing phase (&lt; 0) is ignored; none = 0 (no offset).
        /// </summary>
        public static float AlignmentOffset(float referenceLeft, float referenceRight, float childLeft, float childRight)
        {
            float sx = 0f;
            float sy = 0f;
            int used = 0;
            if (referenceLeft >= 0f && childLeft >= 0f)
            {
                Accumulate(childLeft - referenceLeft, ref sx, ref sy);
                used++;
            }

            if (referenceRight >= 0f && childRight >= 0f)
            {
                Accumulate(childRight - referenceRight, ref sx, ref sy);
                used++;
            }

            if (used == 0 || (Mathf.Abs(sx) < 1e-6f && Mathf.Abs(sy) < 1e-6f))
            {
                return 0f;
            }

            float mean = Mathf.Atan2(sy, sx) / (2f * Mathf.PI);
            return Wrap01(mean);
        }

        /// <summary>
        /// True when an increasing normalized time went past a plant at <paramref name="phase"/> (0..1, once per
        /// loop, or <paramref name="strides"/> evenly spaced times per loop) between <paramref name="previous"/>
        /// (exclusive) and <paramref name="current"/> (inclusive). Time going backwards (state restarted) or a
        /// negative phase never fires; a frame spanning several plants fires once.
        /// </summary>
        public static bool CrossedPhase(float previous, float current, float phase, int strides = 1)
        {
            if (phase < 0f || current <= previous || float.IsNaN(previous) || float.IsNaN(current))
            {
                return false;
            }

            int n = Mathf.Max(1, strides);
            float a = previous * n;
            float b = current * n;
            float p = Wrap01(phase * n);
            // Plants happen at k + p: is there an integer k with a < k + p <= b?
            return Mathf.Floor(b - p) > Mathf.Floor(a - p) || (b - a) >= 1f;
        }

        /// <summary>
        /// Rigid (rig-less) step phase in radians (one footfall every π, like <c>sin(phase)</c> touching 0): the
        /// footfall index crossed between two phases, or -1. Even = left, odd = right.
        /// </summary>
        public static int RigidFootfall(float previousPhase, float currentPhase)
        {
            if (currentPhase <= previousPhase)
            {
                return -1;
            }

            int a = Mathf.FloorToInt(previousPhase / Mathf.PI);
            int b = Mathf.FloorToInt(currentPhase / Mathf.PI);
            return b > a ? b : -1;
        }

        /// <summary>
        /// Runtime plant detection from the live (animated) feet when no phases were baked: tracks the signed
        /// stride spread (left foot ahead = +, along the facing) and reports a heel strike when the spread turns
        /// back from an extreme by more than <paramref name="hysteresis"/> (fraction of the recent stride): a
        /// maximum is the left foot striking out in front, a minimum the right. Returns 0 = left, 1 = right,
        /// -1 = none. <paramref name="state"/> keeps the tracking between frames.
        /// </summary>
        public static int StrideExtremum(ref StrideTracker state, float spread, float hysteresis = 0.15f, float minStride = 0.05f)
        {
            if (!state.Initialized)
            {
                state = new StrideTracker { Initialized = true, Extreme = spread, Amplitude = minStride, Rising = spread >= 0f };
                return -1;
            }

            state.Amplitude = Mathf.Max(minStride, Mathf.Max(Mathf.Abs(spread), state.Amplitude * 0.995f));
            float margin = state.Amplitude * Mathf.Max(0.01f, hysteresis);

            if (state.Rising)
            {
                if (spread > state.Extreme)
                {
                    state.Extreme = spread;
                }
                else if (state.Extreme - spread > margin)
                {
                    bool strike = state.Extreme > margin;
                    state.Rising = false;
                    state.Extreme = spread;
                    return strike ? 0 : -1;
                }
            }
            else
            {
                if (spread < state.Extreme)
                {
                    state.Extreme = spread;
                }
                else if (spread - state.Extreme > margin)
                {
                    bool strike = state.Extreme < -margin;
                    state.Rising = true;
                    state.Extreme = spread;
                    return strike ? 1 : -1;
                }
            }

            return -1;
        }

        /// <summary>Footstep loudness/dust (0..1): soft when walking, full at run speed, heavier heroes a bit louder.</summary>
        public static float Intensity(float speed, float runSpeed, float weight)
        {
            float run = runSpeed > 0.01f ? Mathf.Clamp01(speed / runSpeed) : 0f;
            return Mathf.Clamp01((0.25f + 0.75f * run) * Mathf.Lerp(0.85f, 1.15f, Mathf.InverseLerp(0.5f, 1.5f, weight)));
        }

        /// <summary>State kept by <see cref="StrideExtremum"/>.</summary>
        public struct StrideTracker
        {
            public bool Initialized;
            public bool Rising;
            public float Extreme;
            public float Amplitude;
        }

        private static void Accumulate(float delta, ref float sx, ref float sy)
        {
            float angle = Wrap01(delta) * 2f * Mathf.PI;
            sx += Mathf.Cos(angle);
            sy += Mathf.Sin(angle);
        }

        private static int Wrap(int index, int count)
        {
            int m = index % count;
            return m < 0 ? m + count : m;
        }

        private static float Wrap01(float value)
        {
            float w = value - Mathf.Floor(value);
            return w >= 1f ? 0f : w;
        }
    }
}
