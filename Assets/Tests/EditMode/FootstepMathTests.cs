using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>Foot plants, blend-tree phase alignment, footstep firing (<see cref="FootstepMath"/>) and the turn follow-through drive.</summary>
    public class FootstepMathTests
    {
        private const int Samples = 90;

        /// <summary>
        /// One synthetic in-place foot over a loop of <paramref name="strides"/> strides: stance 60 % (slides back
        /// from +0.3 to -0.3 on the ground), swing 40 % (arcs forward, up to 0.1 m). The heel strikes at
        /// <paramref name="plant"/> (+ k / strides), shifted by <paramref name="landingLag"/> of the height arc.
        /// </summary>
        private static Vector3[] Foot(float plant, int strides = 1, float landingLag = 0f)
        {
            var p = new Vector3[Samples + 1];
            for (int i = 0; i <= Samples; i++)
            {
                float t = (float)i / Samples;
                float u = Mathf.Repeat((t - plant) * strides, 1f);
                float z = u < 0.6f ? Mathf.Lerp(0.3f, -0.3f, u / 0.6f) : Mathf.Lerp(-0.3f, 0.3f, (u - 0.6f) / 0.4f);
                float h = Mathf.Repeat(u - landingLag, 1f);
                float y = h < 0.6f ? 0f : 0.1f * Mathf.Sin(Mathf.PI * (h - 0.6f) / 0.4f);
                p[i] = new Vector3(0.1f, 0.08f + y, z);
            }

            return p;
        }

        private static float CircularDistance(float a, float b)
        {
            float d = Mathf.Abs(Mathf.Repeat(a - b, 1f));
            return Mathf.Min(d, 1f - d);
        }

        [Test]
        public void PlantPhase_FindsTheHeelStrike_OfEachFoot()
        {
            Assert.Less(CircularDistance(FootstepMath.PlantPhase(Foot(0.37f)), 0.37f), 0.05f);
            Assert.Less(CircularDistance(FootstepMath.PlantPhase(Foot(0.87f)), 0.87f), 0.05f, "right foot, half a cycle later");
            Assert.Less(CircularDistance(FootstepMath.PlantPhase(Foot(0.02f)), 0.02f), 0.05f, "plant right at the loop point");
        }

        [Test]
        public void PlantPhase_WaitsForTheFootToComeDown()
        {
            // Most forward at 0.2, but the foot is still in the air for a few % more.
            float phase = FootstepMath.PlantPhase(Foot(0.2f, 1, landingLag: 0.06f));
            Assert.Greater(phase, 0.2f + 1f / Samples);
            Assert.Less(phase, 0.3f);
        }

        [Test]
        public void PlantPhase_RejectsBadInput()
        {
            Assert.AreEqual(-1f, FootstepMath.PlantPhase(null));
            Assert.AreEqual(-1f, FootstepMath.PlantPhase(new Vector3[3]));
        }

        [Test]
        public void StrideCount_CountsStancesPerLoop()
        {
            Assert.AreEqual(1, FootstepMath.StrideCount(Foot(0.3f)));
            Assert.AreEqual(2, FootstepMath.StrideCount(Foot(0.3f, 2)));
            Assert.AreEqual(1, FootstepMath.StrideCount(new Vector3[Samples + 1]), "flat (idle) = 1");
        }

        [Test]
        public void AlignmentOffset_PutsTheChildPlantsOnTheReference()
        {
            Assert.AreEqual(0.25f, FootstepMath.AlignmentOffset(0.1f, 0.6f, 0.35f, 0.85f), 1e-4f);
            Assert.AreEqual(0.2f, FootstepMath.AlignmentOffset(0.9f, 0.4f, 0.1f, 0.6f), 1e-4f, "wraps around the loop");
            Assert.AreEqual(0f, FootstepMath.AlignmentOffset(0.3f, 0.8f, 0.3f, 0.8f), 1e-4f, "already aligned");
            Assert.AreEqual(0.1f, FootstepMath.AlignmentOffset(0.2f, -1f, 0.3f, -1f), 1e-4f, "one foot only");
            Assert.AreEqual(0f, FootstepMath.AlignmentOffset(-1f, -1f, 0.3f, 0.8f), "nothing to align");

            // Uneven child (R plant 0.04 late): the mean splits the error between both feet.
            float offset = FootstepMath.AlignmentOffset(0.1f, 0.6f, 0.3f, 0.84f);
            Assert.AreEqual(0.22f, offset, 1e-3f);
        }

        [Test]
        public void AlignedChild_PlantsWithTheWalk_ThroughTheBlend()
        {
            // Walk plants at 0.12/0.62; Run loops from a different frame (0.53/0.03).
            float[] walk = { FootstepMath.PlantPhase(Foot(0.12f)), FootstepMath.PlantPhase(Foot(0.62f)) };
            float[] run = { FootstepMath.PlantPhase(Foot(0.53f)), FootstepMath.PlantPhase(Foot(0.03f)) };
            float offset = FootstepMath.AlignmentOffset(walk[0], walk[1], run[0], run[1]);

            // In the blend tree the run child plays at (state time + offset): its plants land on the walk's.
            Assert.Less(CircularDistance(walk[0] + offset, run[0]), 0.02f);
            Assert.Less(CircularDistance(walk[1] + offset, run[1]), 0.02f);
        }

        [Test]
        public void CrossedPhase_FiresOncePerPlant()
        {
            Assert.IsTrue(FootstepMath.CrossedPhase(0.2f, 0.3f, 0.25f));
            Assert.IsFalse(FootstepMath.CrossedPhase(0.3f, 0.4f, 0.25f));
            Assert.IsTrue(FootstepMath.CrossedPhase(0.95f, 1.05f, 0.02f), "across the loop point");
            Assert.IsTrue(FootstepMath.CrossedPhase(1.95f, 2.05f, 0.98f), "later loops");
            Assert.IsTrue(FootstepMath.CrossedPhase(0.2f, 0.25f, 0.25f), "inclusive end");
            Assert.IsFalse(FootstepMath.CrossedPhase(0.25f, 0.3f, 0.25f), "exclusive start: no double step");
            Assert.IsFalse(FootstepMath.CrossedPhase(0.4f, 0.1f, 0.25f), "state restarted");
            Assert.IsFalse(FootstepMath.CrossedPhase(0.2f, 0.3f, -1f), "no phase baked");
            Assert.IsTrue(FootstepMath.CrossedPhase(0.55f, 0.65f, 0.1f, 2), "two strides per loop: plants at 0.1 and 0.6");
            Assert.IsFalse(FootstepMath.CrossedPhase(0.55f, 0.65f, 0.1f, 1));
        }

        [Test]
        public void CrossedPhase_CountsEveryStep_AtAnyFrameRate()
        {
            foreach (float step in new[] { 0.004f, 0.03f, 0.11f })
            {
                int left = 0;
                int right = 0;
                float previous = 0f;
                for (float t = step; t <= 10f + 1e-4f; t += step)
                {
                    if (FootstepMath.CrossedPhase(previous, t, 0.12f)) left++;
                    if (FootstepMath.CrossedPhase(previous, t, 0.62f)) right++;
                    previous = t;
                }

                Assert.AreEqual(10, left, $"dt {step}");
                Assert.AreEqual(10, right, $"dt {step}");
            }
        }

        [Test]
        public void RigidFootfall_EveryPi_Alternating()
        {
            Assert.AreEqual(1, FootstepMath.RigidFootfall(3.0f, 3.2f));
            Assert.AreEqual(2, FootstepMath.RigidFootfall(6.2f, 6.4f));
            Assert.AreEqual(-1, FootstepMath.RigidFootfall(0.1f, 0.2f));
            Assert.AreEqual(-1, FootstepMath.RigidFootfall(3.2f, 3.0f), "easing back never steps");
        }

        [Test]
        public void StrideExtremum_StrikesAtEachExtreme_Alternating()
        {
            var tracker = new FootstepMath.StrideTracker();
            const float dt = 1f / 60f;
            const float hz = 1.5f; // stride cycles per second
            int last = -1;
            int strikes = 0;
            for (float t = 0f; t < 2f; t += dt)
            {
                float spread = 0.3f * Mathf.Sin(2f * Mathf.PI * hz * t);
                int foot = FootstepMath.StrideExtremum(ref tracker, spread);
                if (foot < 0)
                {
                    continue;
                }

                Assert.AreNotEqual(last, foot, $"feet alternate (t {t:0.00})");
                // Left strikes just after a maximum (sin > 0), right just after a minimum.
                Assert.AreEqual(foot == 0, spread > 0f, $"t {t:0.00}");
                last = foot;
                strikes++;
            }

            Assert.That(strikes, Is.InRange(5, 6), "two strikes per stride over 3 strides");
        }

        [Test]
        public void StrideExtremum_IgnoresJitterWhenStanding()
        {
            var tracker = new FootstepMath.StrideTracker();
            for (int i = 0; i < 300; i++)
            {
                float jitter = 0.004f * Mathf.Sin(i * 1.7f);
                Assert.AreEqual(-1, FootstepMath.StrideExtremum(ref tracker, jitter));
            }
        }

        [Test]
        public void Intensity_GrowsWithSpeedAndWeight()
        {
            Assert.Less(FootstepMath.Intensity(2.5f, 5.5f, 1f), FootstepMath.Intensity(5.5f, 5.5f, 1f));
            Assert.Less(FootstepMath.Intensity(5.5f, 5.5f, 0.75f), FootstepMath.Intensity(5.5f, 5.5f, 1.35f));
            Assert.That(FootstepMath.Intensity(9f, 5.5f, 2f), Is.InRange(0f, 1f));
        }

        [Test]
        public void TurnLag_TrailsTheTurnStart_AndOvershootsTheStop()
        {
            Assert.AreEqual(0f, MotionFeelMath.TurnLagDrive(5000f, 0f));
            Assert.Less(MotionFeelMath.TurnLagDrive(5000f, 0.5f), 0f, "opposes the yaw acceleration");
            Assert.AreEqual(-MotionFeelMath.MaxTurnLagYawAcceleration, MotionFeelMath.TurnLagDrive(1e9f, 1f), 1e-2f, "snap turns are clamped");
            Assert.AreEqual(0f, MotionFeelMath.TurnLagDrive(float.NaN, 1f));

            const float dt = 1f / 60f;
            float lag = 0f, velocity = 0f, rate = 0f, minDuringStart = 0f, maxAfterStop = 0f;
            for (float t = 0f; t < 1.5f; t += dt)
            {
                float target = t < 0.6f ? 600f : 0f; // turn at 600°/s for 0.6 s
                float previous = rate;
                rate = Mathf.MoveTowards(rate, target, 6000f * dt);
                float accel = (rate - previous) / dt;
                MotionFeelMath.StepSpring(ref lag, ref velocity, 0f, 2.8f, 0.55f, dt, MotionFeelMath.TurnLagDrive(accel, 0.35f));
                if (t < 0.3f) minDuringStart = Mathf.Min(minDuringStart, lag);
                if (t > 0.6f) maxAfterStop = Mathf.Max(maxAfterStop, lag);
            }

            Assert.Less(minDuringStart, -0.5f, "the head trails a turn to the right");
            Assert.Greater(maxAfterStop, 0.5f, "and swings past when the turn stops");
            Assert.AreEqual(0f, lag, 0.2f, "settles");
        }

        [Test]
        public void Profiles_HaveTheOverlapLayerOn_HeavierLagsMore()
        {
            foreach (CharacterId id in new[] { CharacterId.Joaquim, CharacterId.Maria, CharacterId.Maithe, CharacterId.Icaro, CharacterId.Lavine })
            {
                HeroMotionProfile profile = HeroMotionProfile.For(id);
                Assert.IsTrue(profile.proceduralOverlap, id.ToString());
                Assert.That(profile.turnFollowThrough, Is.InRange(0.05f, 1f), id.ToString());
                Assert.Greater(profile.turnLagMaxDegrees, 0f, id.ToString());
                Assert.AreEqual(profile.proceduralOverlap, profile.Clone().proceduralOverlap);
            }

            Assert.Greater(HeroMotionProfile.For(CharacterId.Joaquim).turnFollowThrough, HeroMotionProfile.For(CharacterId.Maithe).turnFollowThrough);
        }
    }
}
