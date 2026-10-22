using NUnit.Framework;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Player.States;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the hero action timing contract (clip peak → playback speed → strike
    /// delay), the Animator parameter probe used before firing triggers, and the basic attack's
    /// delayed strike (damage lands after the windup, dropped on death).
    /// </summary>
    public class HeroActionTimingTests
    {
        [Test]
        public void PlaybackSpeed_PutsPeakAtTargetDelay_WithinBounds()
        {
            Assert.AreEqual(1f, HeroActionTiming.PlaybackSpeedForPeak(0f, 0.22f), "unmeasured clip plays at 1");
            Assert.AreEqual(1f, HeroActionTiming.PlaybackSpeedForPeak(0.1f, 0.22f), "a peak already before the target is not slowed down");
            Assert.AreEqual(0.88f / 0.22f, HeroActionTiming.PlaybackSpeedForPeak(0.88f, 0.22f), 1e-4f);
            Assert.AreEqual(HeroActionTiming.MaxActionSpeed, HeroActionTiming.PlaybackSpeedForPeak(50f, 0.22f), "never faster than the cap");

            float speed = HeroActionTiming.PlaybackSpeedForPeak(0.88f, HeroActionTiming.StrikeDelay);
            Assert.AreEqual(HeroActionTiming.StrikeDelay, HeroActionTiming.RealPeakTime(0.88f, speed), 1e-4f);
        }

        [Test]
        public void ExitTime_KeepsPeakPlusRecovery_Clamped()
        {
            float exit = HeroActionTiming.ExitTime(2f, 0.8f, 1f);
            Assert.AreEqual((0.8f + HeroActionTiming.RecoverSeconds) / 2f, exit, 1e-4f);
            Assert.AreEqual(1f, HeroActionTiming.ExitTime(0.5f, 0.8f, 1f), "cannot exit past the end");
            Assert.AreEqual(1f, HeroActionTiming.ExitTime(0f, 0.8f, 1f));
            Assert.GreaterOrEqual(HeroActionTiming.ExitTime(10f, 0.01f, 1f), 0.2f);
        }

        [Test]
        public void PeakSpeedSample_FindsTheFastestMove_IgnoringTheEdges()
        {
            var positions = new Vector3[21];
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = Vector3.zero;
            }

            positions[0] = new Vector3(5f, 0f, 0f); // huge jump at the very start: settling, ignored
            positions[12] = new Vector3(0f, 1f, 0f); // the strike (fastest move, ties resolve to the first)
            positions[13] = new Vector3(0f, 1f, 0f);

            Assert.AreEqual(12, HeroActionTiming.PeakSpeedSample(positions, 0.05f));
            Assert.AreEqual(-1, HeroActionTiming.PeakSpeedSample(new Vector3[2], 0.05f));
            Assert.AreEqual(-1, HeroActionTiming.PeakSpeedSample(null, 0.05f));
        }

        [Test]
        public void HasParameter_IsFalseWithoutAnimatorOrController()
        {
            Assert.IsFalse(PlayerAnimatorHashes.HasParameter(null, PlayerAnimatorHashes.Attack));

            var go = new GameObject("NoController");
            try
            {
                var animator = go.AddComponent<Animator>();
                Assert.IsFalse(PlayerAnimatorHashes.HasParameter(animator, PlayerAnimatorHashes.Attack));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ActionAnimator_WithoutClips_ReportsNoneAndDefaultStrikeDelay()
        {
            var go = new GameObject("Player");
            try
            {
                var actions = go.AddComponent<PlayerActionAnimator>();
                Assert.IsFalse(actions.HasClipActions);
                Assert.IsFalse(actions.HasDeathClip);
                Assert.AreEqual(HeroActionTiming.StrikeDelay, actions.AttackStrikeDelay);
                // Firing with no animator must be a silent no-op.
                actions.OnAttackPerformedForTests(new Rapadura.Core.Events.PlayerAttackPerformedEvent(go));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void BasicAttack_DamageLandsAfterTheWindup()
        {
            var go = new GameObject("Player");
            try
            {
                var attack = go.AddComponent<PlayerBasicAttack>();
                Assert.AreEqual(HeroActionTiming.StrikeDelay, attack.StrikeDelay, 1e-5f, "no clips: contract default");

                attack.PerformAttackForTests();
                Assert.IsTrue(attack.IsStrikePending, "the swing started, the hit has not landed");

                attack.TickPendingStrikeForTests(HeroActionTiming.StrikeDelay * 0.5f);
                Assert.IsTrue(attack.IsStrikePending);

                attack.TickPendingStrikeForTests(HeroActionTiming.StrikeDelay * 0.6f);
                Assert.IsFalse(attack.IsStrikePending, "the hit landed once the delay elapsed");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
