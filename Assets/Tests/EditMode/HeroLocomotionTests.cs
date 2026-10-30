using NUnit.Framework;
using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>Pure math behind the fluid hero locomotion/animation (<see cref="LocomotionMath"/>).</summary>
    public class HeroLocomotionTests
    {
        private const float Dt = 1f / 60f;

        // ------------------------------------------------------------------
        // Acceleration / deceleration
        // ------------------------------------------------------------------

        [Test]
        public void StepSpeed_RampsUpGradually_NoInstantStart()
        {
            float accel = LocomotionMath.AccelerationFor(5.5f, 0.16f);
            float speed = LocomotionMath.StepSpeed(0f, 5.5f, accel, 999f, Dt);

            Assert.Greater(speed, 0f);
            Assert.Less(speed, 5.5f, "one frame must not reach full speed");
        }

        [Test]
        public void StepSpeed_ReachesTargetWithinConfiguredTime_AndNeverOvershoots()
        {
            float accel = LocomotionMath.AccelerationFor(5.5f, 0.16f);
            float speed = 0f;
            float t = 0f;
            while (speed < 5.5f && t < 1f)
            {
                speed = LocomotionMath.StepSpeed(speed, 5.5f, accel, 999f, Dt);
                Assert.LessOrEqual(speed, 5.5f);
                t += Dt;
            }

            Assert.AreEqual(0.16f, t, 2f * Dt);
        }

        [Test]
        public void StepSpeed_UsesDecelerationWhenSlowing_NoInstantStop()
        {
            float decel = LocomotionMath.AccelerationFor(5.5f, 0.12f);
            float speed = LocomotionMath.StepSpeed(5.5f, 0f, 1f, decel, Dt);

            Assert.Less(speed, 5.5f);
            Assert.Greater(speed, 0f, "releasing input must brake, not stop dead");
            Assert.AreEqual(5.5f - decel * Dt, speed, 1e-4f);
        }

        [Test]
        public void AccelerationFor_CurveFactorScales()
        {
            Assert.AreEqual(2f * LocomotionMath.AccelerationFor(5f, 0.5f), LocomotionMath.AccelerationFor(5f, 0.5f, 2f), 1e-4f);
            Assert.IsTrue(float.IsPositiveInfinity(LocomotionMath.AccelerationFor(5f, 0f)));
        }

        // ------------------------------------------------------------------
        // Turning
        // ------------------------------------------------------------------

        [Test]
        public void StepFacing_RespectsAngularSpeed()
        {
            Quaternion start = Quaternion.LookRotation(Vector3.forward);
            Quaternion next = LocomotionMath.StepFacing(start, Vector3.right, 720f, 0.05f);

            Assert.AreEqual(36f, Quaternion.Angle(start, next), 0.01f);
        }

        [Test]
        public void TurnRate_IsFasterWhenStopped()
        {
            Assert.AreEqual(1000f, LocomotionMath.TurnRate(720f, 1000f, 0f), 1e-3f);
            Assert.AreEqual(720f, LocomotionMath.TurnRate(720f, 1000f, 1f), 1e-3f);
            Assert.Greater(LocomotionMath.TurnRate(720f, 1000f, 0.3f), LocomotionMath.TurnRate(720f, 1000f, 0.8f));
        }

        [Test]
        public void StepDirection_ArcsForModerateTurns_SnapsOnReversal()
        {
            Vector3 arc = LocomotionMath.StepDirection(Vector3.forward, Vector3.right, 900f, 0.05f);
            Assert.AreEqual(45f, Vector3.Angle(Vector3.forward, arc), 0.05f);
            Assert.AreEqual(1f, arc.magnitude, 1e-4f);

            Vector3 reversal = LocomotionMath.StepDirection(Vector3.forward, Vector3.back, 900f, Dt, 150f);
            Assert.AreEqual(Vector3.back, reversal);

            Assert.AreEqual(Vector3.right, LocomotionMath.StepDirection(Vector3.zero, Vector3.right * 3f, 900f, Dt));
        }

        // ------------------------------------------------------------------
        // Path smoothing / arrival
        // ------------------------------------------------------------------

        [Test]
        public void SmoothDestination_GlidesBetweenSmallReAims()
        {
            Vector3 pos = Vector3.zero;
            Vector3 old = new Vector3(0f, 0f, 5f);
            Vector3 raw = new Vector3(1f, 0f, 5f); // ~11° re-aim
            Vector3 smoothed = LocomotionMath.SmoothDestination(pos, old, raw, Dt, 0.12f, 70f);

            Assert.Greater(smoothed.x, 0f);
            Assert.Less(smoothed.x, 1f, "a hold re-aim must not kink the heading in one frame");

            for (int i = 0; i < 120; i++)
            {
                smoothed = LocomotionMath.SmoothDestination(pos, smoothed, raw, Dt, 0.12f, 70f);
            }

            Assert.AreEqual(raw.x, smoothed.x, 1e-3f, "converges on the real destination");
        }

        [Test]
        public void SmoothDestination_SnapsOnANewOrderInAnotherDirection()
        {
            Vector3 smoothed = LocomotionMath.SmoothDestination(Vector3.zero, new Vector3(0f, 0f, 5f), new Vector3(0f, 0f, -5f), Dt, 0.12f, 70f);
            Assert.AreEqual(new Vector3(0f, 0f, -5f), smoothed);

            Assert.AreEqual(new Vector3(3f, 0f, 3f), LocomotionMath.SmoothDestination(Vector3.zero, Vector3.forward, new Vector3(3f, 0f, 3f), Dt, 0f, 70f), "0 smoothing = raw");
        }

        [Test]
        public void ArrivalSpeed_BrakesNearTheDestination()
        {
            Assert.AreEqual(5.5f, LocomotionMath.ArrivalSpeed(10f, 5.5f, 10f, 0.7f), 1e-4f, "far away: full speed");
            Assert.AreEqual(Mathf.Sqrt(2f * 10f * 0.5f), LocomotionMath.ArrivalSpeed(0.5f, 5.5f, 10f, 0.7f), 1e-4f);
            Assert.AreEqual(0.7f, LocomotionMath.ArrivalSpeed(0f, 5.5f, 10f, 0.7f), 1e-4f, "never crawls below the floor");
            Assert.Less(LocomotionMath.ArrivalSpeed(0.2f, 5.5f, 10f, 0.7f), LocomotionMath.ArrivalSpeed(0.8f, 5.5f, 10f, 0.7f));
        }

        [Test]
        public void HasArrived_ToleratesASmallOvershoot()
        {
            Assert.IsTrue(LocomotionMath.HasArrived(new Vector3(0.1f, 0f, 0f), Vector3.zero, 0.15f, 0.45f));
            Assert.IsFalse(LocomotionMath.HasArrived(new Vector3(0.3f, 0f, 0f), Vector3.right, 0.15f, 0.45f), "still ahead");
            Assert.IsTrue(LocomotionMath.HasArrived(new Vector3(-0.3f, 0f, 0f), Vector3.right, 0.15f, 0.45f), "just passed it");
            Assert.IsFalse(LocomotionMath.HasArrived(new Vector3(-2f, 0f, 0f), Vector3.right, 0.15f, 0.45f), "far behind: turn around");
        }

        // ------------------------------------------------------------------
        // Animator blend / stride
        // ------------------------------------------------------------------

        [Test]
        public void SpeedToBlend_MapsIdleWalkRun()
        {
            Assert.AreEqual(0f, LocomotionMath.SpeedToBlend(0f, 2.5f, 5.5f), 1e-4f);
            Assert.AreEqual(0.25f, LocomotionMath.SpeedToBlend(1.25f, 2.5f, 5.5f), 1e-4f);
            Assert.AreEqual(0.5f, LocomotionMath.SpeedToBlend(2.5f, 2.5f, 5.5f), 1e-4f);
            Assert.AreEqual(0.75f, LocomotionMath.SpeedToBlend(4f, 2.5f, 5.5f), 1e-4f);
            Assert.AreEqual(1f, LocomotionMath.SpeedToBlend(5.5f, 2.5f, 5.5f), 1e-4f);
            Assert.AreEqual(1f, LocomotionMath.SpeedToBlend(9f, 2.5f, 5.5f), 1e-4f, "haste clamps the blend; playback speeds up instead");
        }

        [Test]
        public void StridePlayback_MatchesGroundSpeed()
        {
            // Walk clip covers 2 m/s at playback 1, the hero walks at 2.5 → play 1.25×.
            float natural = LocomotionMath.BlendNaturalSpeed(0.5f, 2f, 5f);
            Assert.AreEqual(2f, natural, 1e-4f);
            Assert.AreEqual(1.25f, LocomotionMath.StridePlaybackSpeed(2.5f, natural, 0.5f, 2f), 1e-4f);

            // Half-way into the walk (blend 0.25) the idle-blended stride is half as long.
            Assert.AreEqual(1f, LocomotionMath.BlendNaturalSpeed(0.25f, 2f, 5f), 1e-4f);
            Assert.AreEqual(3.5f, LocomotionMath.BlendNaturalSpeed(0.75f, 2f, 5f), 1e-4f);

            Assert.AreEqual(1f, LocomotionMath.StridePlaybackSpeed(0f, natural, 0.5f, 2f), 1e-4f, "standing: normal playback");
            Assert.AreEqual(2f, LocomotionMath.StridePlaybackSpeed(20f, natural, 0.5f, 2f), 1e-4f, "clamped");
        }

        [Test]
        public void StridePlayback_WithJogChild_IsPiecewise()
        {
            // Mocap locomotion: Walk 0.5 (2 m/s), Jog 0.75 (3 m/s), Run 1 (3.5 m/s).
            Assert.AreEqual(1f, LocomotionMath.BlendNaturalSpeed(0.25f, 2f, 3f, 3.5f), 1e-4f, "below walk the idle pose shortens the stride as before");
            Assert.AreEqual(2.5f, LocomotionMath.BlendNaturalSpeed(0.625f, 2f, 3f, 3.5f), 1e-4f);
            Assert.AreEqual(3f, LocomotionMath.BlendNaturalSpeed(LocomotionMath.JogBlend, 2f, 3f, 3.5f), 1e-4f);
            Assert.AreEqual(3.25f, LocomotionMath.BlendNaturalSpeed(0.875f, 2f, 3f, 3.5f), 1e-4f);
            Assert.AreEqual(LocomotionMath.BlendNaturalSpeed(0.8f, 2f, 5f), LocomotionMath.BlendNaturalSpeed(0.8f, 2f, 0f, 5f), 1e-5f, "no jog child = old walk→run line");
        }

        [Test]
        public void DampParameter_ConvergesSmoothlyWithoutOvershoot()
        {
            float value = 0f;
            float velocity = 0f;
            value = LocomotionMath.DampParameter(value, 1f, ref velocity, 0.1f, Dt);
            Assert.Greater(value, 0f);
            Assert.Less(value, 0.5f, "damped, not instant");

            for (int i = 0; i < 120; i++)
            {
                value = LocomotionMath.DampParameter(value, 1f, ref velocity, 0.1f, Dt);
                Assert.LessOrEqual(value, 1.0001f);
            }

            Assert.AreEqual(1f, value, 1e-3f);

            float paused = LocomotionMath.DampParameter(0.3f, 1f, ref velocity, 0.1f, 0f);
            Assert.AreEqual(0.3f, paused, "paused (dt 0) holds");
        }

        [Test]
        public void EstimateStanceSpeed_ReadsPlantedFootSpeed()
        {
            // Synthetic in-place gait: the foot slides back at 1.5 u/s while low, swings forward high.
            const int n = 40;
            const float interval = 0.025f; // 1 s cycle
            var foot = new Vector3[n + 1];
            float z = 0.4f;
            for (int i = 0; i <= n; i++)
            {
                bool stance = i < n / 2;
                foot[i] = new Vector3(0f, stance ? 0f : 0.2f, z);
                z += stance ? -1.5f * interval : 1.5f * interval;
            }

            Assert.AreEqual(1.5f, LocomotionMath.EstimateStanceSpeed(foot, interval), 0.05f);
            Assert.AreEqual(0f, LocomotionMath.EstimateStanceSpeed(new Vector3[1], interval));
        }

        [Test]
        public void ChooseStandingSample_PrefersFeetLowAndTogether()
        {
            var left = new[] { new Vector3(0f, 0f, 0.3f), new Vector3(0f, 0.15f, 0f), new Vector3(0f, 0f, 0.05f) };
            var right = new[] { new Vector3(0f, 0f, -0.3f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0.01f, -0.05f) };

            Assert.AreEqual(2, LocomotionMath.ChooseStandingSample(left, right));
        }

        // ------------------------------------------------------------------
        // Procedural layers
        // ------------------------------------------------------------------

        [Test]
        public void LeanAngles_LeanIntoAccelerationAndTurns()
        {
            Vector2 accelerating = LocomotionMath.LeanAngles(5f, 0f, 3f, 1f, 1f, 12f, 10f);
            Assert.Greater(accelerating.x, 0f, "leans forward when speeding up");
            Assert.AreEqual(0f, accelerating.y, 1e-4f);

            Vector2 turningRight = LocomotionMath.LeanAngles(0f, 180f, 5f, 1f, 1f, 12f, 10f);
            Assert.Greater(turningRight.y, 0f);
            Assert.LessOrEqual(turningRight.y, 10f, "clamped");

            Vector2 stillTurning = LocomotionMath.LeanAngles(0f, 180f, 0f, 1f, 1f, 12f, 10f);
            Assert.AreEqual(0f, stillTurning.y, 1e-4f, "turning in place: no centripetal lean");
        }

        [Test]
        public void Envelope_RisesHoldsAndReleases()
        {
            Assert.AreEqual(0f, LocomotionMath.Envelope(-0.1f, 0.1f, 0.2f, 0.3f));
            Assert.AreEqual(0.5f, LocomotionMath.Envelope(0.05f, 0.1f, 0.2f, 0.3f), 1e-4f);
            Assert.AreEqual(1f, LocomotionMath.Envelope(0.2f, 0.1f, 0.2f, 0.3f));
            Assert.AreEqual(0f, LocomotionMath.Envelope(0.7f, 0.1f, 0.2f, 0.3f));
            Assert.AreEqual(0.6f, LocomotionMath.EnvelopeLength(0.1f, 0.2f, 0.3f), 1e-5f);
        }

        [Test]
        public void AttackSwing_IsFrontLoadedAndReturnsToRest()
        {
            Assert.AreEqual(0f, LocomotionMath.AttackArmSwing(0f));
            Assert.AreEqual(1f, LocomotionMath.AttackArmSwing(0.12f), 1e-4f, "whipped up right after the hit event");
            Assert.Less(LocomotionMath.AttackArmSwing(0.35f), LocomotionMath.AttackArmSwing(0.12f));
            Assert.AreEqual(0f, LocomotionMath.AttackArmSwing(1f));
            Assert.Less(LocomotionMath.AttackTwist(0.35f), 0f, "shoulders whip through the strike");
        }

        [Test]
        public void StepPhaseAdvance_FollowsGroundSpeed_AndFallIsEaseIn()
        {
            Assert.AreEqual(Mathf.PI, LocomotionMath.StepPhaseAdvance(0.6f, 0.6f, 1f), 1e-4f, "one step per step length");
            Assert.AreEqual(0f, LocomotionMath.StepPhaseAdvance(2f, 0f, 1f));

            Assert.AreEqual(0.25f, LocomotionMath.FallProgress(0.3f, 0.6f), 1e-4f);
            Assert.AreEqual(1f, LocomotionMath.FallProgress(5f, 0.6f));
        }
    }
}
