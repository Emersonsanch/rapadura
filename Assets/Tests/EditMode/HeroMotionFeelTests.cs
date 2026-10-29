using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>Weight/personality layers of the hero animation (<see cref="MotionFeelMath"/>, <see cref="HeroMotionProfile"/>).</summary>
    public class HeroMotionFeelTests
    {
        private const float Dt = 1f / 60f;

        // ------------------------------------------------------------------
        // Springs
        // ------------------------------------------------------------------

        [Test]
        public void Spring_ConvergesToTarget()
        {
            float x = 0f, v = 0f;
            for (int i = 0; i < 600; i++)
            {
                MotionFeelMath.StepSpring(ref x, ref v, 1f, 3f, 0.5f, Dt);
            }

            Assert.AreEqual(1f, x, 1e-3f);
            Assert.AreEqual(0f, v, 1e-2f);
        }

        [Test]
        public void Spring_UnderdampedOvershoots_CriticalDoesNot()
        {
            float x = 0f, v = 0f, peak = 0f;
            for (int i = 0; i < 300; i++)
            {
                peak = Mathf.Max(peak, MotionFeelMath.StepSpring(ref x, ref v, 1f, 3f, 0.4f, Dt));
            }

            Assert.Greater(peak, 1.05f, "follow-through: a loose spring swings past the target");

            x = 0f; v = 0f; peak = 0f;
            for (int i = 0; i < 300; i++)
            {
                peak = Mathf.Max(peak, MotionFeelMath.StepSpring(ref x, ref v, 1f, 3f, 1f, Dt));
            }

            Assert.LessOrEqual(peak, 1.001f, "critically damped never overshoots");
        }

        [Test]
        public void Spring_StaysStableOnHugeFrames()
        {
            float x = 0f, v = 0f;
            for (int i = 0; i < 20; i++)
            {
                MotionFeelMath.StepSpring(ref x, ref v, 1f, 8f, 0.3f, 0.5f);
                Assert.IsFalse(float.IsNaN(x) || float.IsInfinity(x));
                Assert.Less(Mathf.Abs(x), 3f, "a hitch must never explode the spring");
            }
        }

        [Test]
        public void Spring_IsFrameRateIndependent()
        {
            float a = 0f, va = 0f, b = 0f, vb = 0f;
            for (int i = 0; i < 30; i++) MotionFeelMath.StepSpring(ref a, ref va, 1f, 2.5f, 0.6f, 1f / 30f);
            for (int i = 0; i < 120; i++) MotionFeelMath.StepSpring(ref b, ref vb, 1f, 2.5f, 0.6f, 1f / 120f);
            Assert.AreEqual(a, b, 0.02f);
        }

        [Test]
        public void Spring_ExternalAccelerationLagsByGainOverOmegaSquared()
        {
            float omega = 2f * Mathf.PI * 2f;
            float x = 0f, v = 0f;
            for (int i = 0; i < 900; i++)
            {
                MotionFeelMath.StepSpring(ref x, ref v, 0f, 2f, 0.7f, Dt, -3f * omega * omega);
            }

            Assert.AreEqual(-3f, x, 0.01f, "steady lag = drive / ω²");
        }

        [Test]
        public void Spring_PausedTimeFreezes()
        {
            float x = 0.3f, v = 2f;
            MotionFeelMath.StepSpring(ref x, ref v, 1f, 3f, 0.5f, 0f);
            Assert.AreEqual(0.3f, x);
            Assert.AreEqual(2f, v);
        }

        // ------------------------------------------------------------------
        // Turn braking
        // ------------------------------------------------------------------

        [Test]
        public void TurnBrake_OnlyTriggersOnSharpTurnsAtSpeed()
        {
            Assert.IsFalse(MotionFeelMath.ShouldTurnBrake(60f, 110f, 5f, 1f), "gentle arc: no brake");
            Assert.IsTrue(MotionFeelMath.ShouldTurnBrake(130f, 110f, 5f, 1f));
            Assert.IsFalse(MotionFeelMath.ShouldTurnBrake(130f, 110f, 0.5f, 1f), "nearly stopped: just pivot");
        }

        [Test]
        public void TurnBrakeFactor_EasesFromOneToSpeedKept()
        {
            Assert.AreEqual(1f, MotionFeelMath.TurnBrakeFactor(100f, 110f, 150f, 0.5f));
            float mid = MotionFeelMath.TurnBrakeFactor(130f, 110f, 150f, 0.5f);
            Assert.Less(mid, 1f);
            Assert.Greater(mid, 0.5f);
            Assert.AreEqual(0.5f, MotionFeelMath.TurnBrakeFactor(150f, 110f, 150f, 0.5f), 1e-4f);
            Assert.AreEqual(1f, MotionFeelMath.TurnBrakeFactor(170f, 180f, 150f, 0.5f), "brake angle past reversal = disabled");
        }

        // ------------------------------------------------------------------
        // Squash & stretch
        // ------------------------------------------------------------------

        [Test]
        public void SquashScale_PreservesVolume_AndIsCapped()
        {
            foreach (float s in new[] { -0.06f, -0.03f, 0f, 0.02f, 0.06f })
            {
                Vector3 scale = MotionFeelMath.SquashScale(s);
                Assert.AreEqual(1f, scale.x * scale.y * scale.z, 1e-5f, $"volume at {s}");
                Assert.AreEqual(1f + s, scale.y, 1e-5f);
            }

            Assert.AreEqual(1f + MotionFeelMath.MaxSquash, MotionFeelMath.SquashScale(0.5f).y, 1e-5f, "subtle: never more than 6 %");
            Assert.AreEqual(1f - MotionFeelMath.MaxSquash, MotionFeelMath.SquashScale(-0.5f).y, 1e-5f);
        }

        [Test]
        public void LandingSquash_GrowsWithFallSpeed_AndIsCapped()
        {
            AnimationCurve curve = HeroMotionProfile.Default.landingSquashByFallSpeed;
            float soft = MotionFeelMath.LandingSquash(1.5f, curve, 0.06f);
            float jump = MotionFeelMath.LandingSquash(-6.6f, curve, 0.06f);
            float hard = MotionFeelMath.LandingSquash(20f, curve, 0.06f);
            Assert.AreEqual(0f, soft, 1e-4f, "stepping off a kerb doesn't squash");
            Assert.Greater(jump, 0f);
            Assert.Greater(hard, jump);
            Assert.LessOrEqual(hard, MotionFeelMath.MaxSquash + 1e-6f);
            Assert.LessOrEqual(MotionFeelMath.LandingSquash(20f, null, 1f), MotionFeelMath.MaxSquash + 1e-6f);
        }

        // ------------------------------------------------------------------
        // Attack timeline
        // ------------------------------------------------------------------

        [Test]
        public void AttackCurveX_HitsTheImpactExactlyOnTheStrikeDelay()
        {
            float delay = HeroActionTiming.StrikeDelay;
            Assert.AreEqual(0f, MotionFeelMath.AttackCurveX(0f, delay, 0.3f), 1e-5f);
            Assert.AreEqual(0.5f, MotionFeelMath.AttackCurveX(delay, delay, 0.3f), 1e-5f);
            Assert.AreEqual(0.75f, MotionFeelMath.AttackCurveX(delay + 0.15f, delay, 0.3f), 1e-5f);
            Assert.AreEqual(-1f, MotionFeelMath.AttackCurveX(delay + 0.31f, delay, 0.3f));
            Assert.AreEqual(-1f, MotionFeelMath.AttackCurveX(-0.1f, delay, 0.3f));
            Assert.AreEqual(delay + 0.3f, MotionFeelMath.AttackLength(delay, 0.3f), 1e-5f);
        }

        [Test]
        public void DefaultAttackArm_AnticipatesThenChopsThroughTheImpact()
        {
            AnimationCurve arm = MotionFeelMath.DefaultAttackArmCurve();
            float cocked = arm.Evaluate(0.4f);
            float impact = arm.Evaluate(0.5f);
            Assert.AreEqual(1f, cocked, 1e-3f, "wound up just before the hit");
            Assert.Less(impact, cocked - 0.4f, "the chop happens over the last part of the anticipation");
            Assert.Greater(arm.Evaluate(0.2f), 0f, "visible windup from the start");
            Assert.AreEqual(0f, arm.Evaluate(1f), 1e-3f, "recovers to rest");
        }

        [Test]
        public void AttackTwistAndLunge_WindBackThenFollowThrough()
        {
            Assert.Greater(MotionFeelMath.AttackTwistAt(0.4f), 0.9f);
            Assert.Less(MotionFeelMath.AttackTwistAt(0.56f), -0.9f, "follow-through past centre after the impact");
            Assert.AreEqual(0f, MotionFeelMath.AttackTwistAt(1f));
            Assert.Less(MotionFeelMath.AttackLungeAt(0.3f), 0f, "anticipation: pull back first");
            Assert.Greater(MotionFeelMath.AttackLungeAt(0.55f), 0.9f, "push peaks at the impact");
            Assert.AreEqual(0f, MotionFeelMath.AttackLungeAt(1f));
        }

        // ------------------------------------------------------------------
        // Idle glance
        // ------------------------------------------------------------------

        [Test]
        public void IdleGlance_NeverInTheFirstInterval_BoundedAndDeterministic()
        {
            Assert.AreEqual(0f, MotionFeelMath.IdleGlanceYaw(2f, 5f, 1f, 40f, 7));

            bool glanced = false;
            for (float t = 0f; t < 60f; t += 0.05f)
            {
                float yaw = MotionFeelMath.IdleGlanceYaw(t, 5f, 1f, 40f, 7);
                Assert.LessOrEqual(Mathf.Abs(yaw), 40f + 1e-4f);
                Assert.AreEqual(yaw, MotionFeelMath.IdleGlanceYaw(t, 5f, 1f, 40f, 7), "deterministic");
                glanced |= Mathf.Abs(yaw) > 10f;
            }

            Assert.IsTrue(glanced, "a minute standing still includes glances");
            Assert.AreEqual(0f, MotionFeelMath.IdleGlanceYaw(30f, 0f, 1f, 40f, 7), "interval 0 disables");
        }

        // ------------------------------------------------------------------
        // Gait / foot IK
        // ------------------------------------------------------------------

        [Test]
        public void StridePhase_SignFollowsTheLeadingFoot()
        {
            Vector3 fwd = Vector3.forward;
            Assert.Greater(MotionFeelMath.StridePhase(new Vector3(-0.1f, 0f, 0.3f), new Vector3(0.1f, 0f, -0.3f), fwd, 0.6f), 0.9f);
            Assert.Less(MotionFeelMath.StridePhase(new Vector3(-0.1f, 0f, -0.3f), new Vector3(0.1f, 0f, 0.3f), fwd, 0.6f), -0.9f);
            Assert.AreEqual(0f, MotionFeelMath.StridePhase(Vector3.left * 0.1f, Vector3.right * 0.1f, fwd, 0.6f), 1e-5f);
        }

        [Test]
        public void FootGroundOffset_FollowsGroundAndClamps()
        {
            Assert.AreEqual(0.1f, MotionFeelMath.FootGroundOffset(true, 1.1f, 1f, 0.35f), 1e-5f);
            Assert.AreEqual(-0.35f, MotionFeelMath.FootGroundOffset(true, 0f, 1f, 0.35f), 1e-5f);
            Assert.AreEqual(0f, MotionFeelMath.FootGroundOffset(false, 5f, 1f, 0.35f));
        }

        [Test]
        public void PelvisOffset_DropsForTheLowerFoot_NeverLifts()
        {
            Assert.AreEqual(-0.12f, MotionFeelMath.PelvisOffset(0.05f, -0.12f), 1e-5f);
            Assert.AreEqual(0f, MotionFeelMath.PelvisOffset(0.05f, 0.2f), "lifting the hips would over-extend the legs");
        }

        [Test]
        public void FootLock_ReleasesOnDriftOrTwist()
        {
            Assert.IsFalse(MotionFeelMath.ShouldReleaseFootLock(Vector3.zero, new Vector3(0.05f, 0.3f, 0f), 10f, 0.16f, 40f), "height doesn't count as drift");
            Assert.IsTrue(MotionFeelMath.ShouldReleaseFootLock(Vector3.zero, new Vector3(0.2f, 0f, 0f), 0f, 0.16f, 40f));
            Assert.IsTrue(MotionFeelMath.ShouldReleaseFootLock(Vector3.zero, Vector3.zero, -45f, 0.16f, 40f));
            Assert.AreEqual(0.06f, MotionFeelMath.StepArc(0.5f, 0.06f), 1e-5f);
            Assert.AreEqual(0f, MotionFeelMath.StepArc(1f, 0.06f));
        }

        // ------------------------------------------------------------------
        // Animator parameters
        // ------------------------------------------------------------------

        [Test]
        public void AnimatorVerticalSpeed_HoldsTheTouchdownBriefly()
        {
            Assert.AreEqual(3f, MotionFeelMath.AnimatorVerticalSpeed(false, 3f, -1f, 0f, 0.12f));
            Assert.AreEqual(-6.6f, MotionFeelMath.AnimatorVerticalSpeed(true, -2f, -6.6f, 0.05f, 0.12f), "landing frame reads the impact");
            Assert.AreEqual(0f, MotionFeelMath.AnimatorVerticalSpeed(true, -2f, -6.6f, 0.2f, 0.12f), "the stick force is not falling");
        }

        [Test]
        public void SignedTurnAngle_PositiveToTheRight()
        {
            Assert.AreEqual(90f, MotionFeelMath.SignedTurnAngle(Vector3.forward, Vector3.right), 1e-3f);
            Assert.AreEqual(-90f, MotionFeelMath.SignedTurnAngle(Vector3.forward, Vector3.left), 1e-3f);
            Assert.AreEqual(0f, MotionFeelMath.SignedTurnAngle(Vector3.forward, Vector3.zero));
        }

        // ------------------------------------------------------------------
        // Profiles
        // ------------------------------------------------------------------

        [Test]
        public void Profiles_HeavyJoaquimHasMoreInertiaThanLightMaithe()
        {
            HeroMotionProfile joaquim = HeroMotionProfile.For(CharacterId.Joaquim);
            HeroMotionProfile maithe = HeroMotionProfile.For(CharacterId.Maithe);

            Assert.Greater(joaquim.weight, maithe.weight);
            Assert.Greater(joaquim.accelerationTime, maithe.accelerationTime);
            Assert.Greater(joaquim.decelerationTime, maithe.decelerationTime);
            Assert.Less(joaquim.turnSpeedMoving, maithe.turnSpeedMoving);
            Assert.Less(joaquim.springFrequency, maithe.springFrequency, "heavier = looser, slower overlap");
            Assert.Greater(joaquim.landingDip, maithe.landingDip);
            Assert.Less(joaquim.flinchScale, maithe.flinchScale);
        }

        [Test]
        public void Profiles_EveryHeroHasItsOwnTiming()
        {
            var ids = (CharacterId[])System.Enum.GetValues(typeof(CharacterId));
            for (int i = 0; i < ids.Length; i++)
            {
                HeroMotionProfile a = HeroMotionProfile.For(ids[i]);
                Assert.AreNotEqual("Default", a.label, $"{ids[i]} has a profile");
                Assert.IsNotNull(a.attackArmCurve);
                Assert.Greater(a.attackRecoverTime, 0f);
                for (int j = i + 1; j < ids.Length; j++)
                {
                    HeroMotionProfile b = HeroMotionProfile.For(ids[j]);
                    bool differs = !Mathf.Approximately(a.breathRate, b.breathRate) || !Mathf.Approximately(a.weightShiftPeriod, b.weightShiftPeriod)
                                   || !Mathf.Approximately(a.glanceInterval, b.glanceInterval);
                    Assert.IsTrue(differs, $"{ids[i]} and {ids[j]} idle with the same rhythm");
                }
            }

            Assert.Less(HeroMotionProfile.For(CharacterId.Lavine).glanceInterval, HeroMotionProfile.For(CharacterId.Maria).glanceInterval, "restless Lavine looks around more than calm Maria");
        }

        [Test]
        public void Profiles_StayWithinSubtleSquashAndRespondFaster()
        {
            foreach (CharacterId id in System.Enum.GetValues(typeof(CharacterId)))
            {
                HeroMotionProfile p = HeroMotionProfile.For(id);
                Assert.LessOrEqual(p.landingSquash, MotionFeelMath.MaxSquash);
                Assert.LessOrEqual(p.takeoffSquash, MotionFeelMath.MaxSquash);
                Assert.LessOrEqual(p.accelerationTime, 0.3f, $"{id}: weight must not make the controls sluggish");
                Assert.Less(p.turnBrakeAngle, 150f, $"{id}: turn braking kicks in before the motor's reversal snap");
            }
        }

        [Test]
        public void Profile_CloneIsDeep()
        {
            HeroMotionProfile source = HeroMotionProfile.For(CharacterId.Icaro);
            HeroMotionProfile copy = source.Clone();
            copy.weight = 9f;
            copy.attackArmCurve.MoveKey(0, new Keyframe(0f, 5f));
            Assert.AreNotEqual(9f, source.weight);
            Assert.AreEqual(0f, source.attackArmCurve.keys[0].value, 1e-5f);
        }

        [Test]
        public void Motor_AcceptsProfile_AndBrakesSharpTurns()
        {
            var go = new GameObject("MotorProfileTest");
            try
            {
                PlayerMotor motor = go.AddComponent<PlayerMotor>();
                motor.ApplyMotionProfile(HeroMotionProfile.For(CharacterId.Joaquim));
                motor.ApplyMotionProfile(null); // ignored, no throw
                Assert.AreEqual(0f, motor.TurnBrake);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
