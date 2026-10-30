using NUnit.Framework;
using Rapadura.Gameplay.Enemies;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Enemy steering (Docs/Animation.md §7.3): obstacle avoidance slides along the hit surface on the side
    /// closer to the goal, grows with proximity and never fires when already moving along/away from the
    /// surface; separation pushes neighbours apart without overriding the goal; pace and side preference
    /// are deterministic per seed and bounded.
    /// </summary>
    public class EnemySteeringTests
    {
        // ------------------------------------------------------------------ look-ahead

        [Test]
        public void LookAhead_GrowsWithSpeed_AndIsBounded()
        {
            float still = EnemySteering.LookAhead(0f, 10f);
            float walking = EnemySteering.LookAhead(2f, 10f);
            float sprinting = EnemySteering.LookAhead(30f, 10f);

            Assert.AreEqual(0.6f, still, 1e-5f, "minimum probe when standing");
            Assert.Greater(walking, still);
            Assert.AreEqual(2.5f, sprinting, 1e-5f, "capped");
        }

        // ------------------------------------------------------------------ avoidance

        [Test]
        public void Avoidance_NoHitNormal_OrMovingAway_KeepsDesired()
        {
            Vector3 desired = Vector3.forward;
            Assert.AreEqual(desired, EnemySteering.AvoidanceDirection(desired, Vector3.zero, 0.5f, 2f, true));
            // Wall behind us (normal pointing forward = we move away from it).
            Assert.AreEqual(desired, EnemySteering.AvoidanceDirection(desired, Vector3.forward, 0.5f, 2f, true));
            // Wall to the side we are sliding along.
            Assert.AreEqual(desired, EnemySteering.AvoidanceDirection(desired, Vector3.right, 0.5f, 2f, true));
        }

        [Test]
        public void Avoidance_SlidesAlongSurface_OnTheSideCloserToTheGoal()
        {
            // Wall facing us (normal = back), goal slightly to the right: slide right.
            Vector3 desired = new Vector3(0.3f, 0f, 1f).normalized;
            Vector3 steered = EnemySteering.AvoidanceDirection(desired, Vector3.back, 0.2f, 2f, preferRight: false);

            Assert.Greater(steered.x, desired.x, "bends further right (the goal's side), not left");
            Assert.Less(steered.z, desired.z, "less into the wall");
            Assert.AreEqual(1f, steered.magnitude, 1e-4f);
            Assert.AreEqual(0f, steered.y, 1e-6f);
        }

        [Test]
        public void Avoidance_StrongerWhenCloser()
        {
            Vector3 desired = new Vector3(0.2f, 0f, 1f).normalized;
            Vector3 far = EnemySteering.AvoidanceDirection(desired, Vector3.back, 1.8f, 2f, false);
            Vector3 near = EnemySteering.AvoidanceDirection(desired, Vector3.back, 0.1f, 2f, false);

            Assert.Greater(Vector3.Angle(desired, near), Vector3.Angle(desired, far));
            Assert.Greater(Vector3.Angle(desired, far), 0f, "heading straight in: a far hit already nudges");
            Assert.Less(near.z, 0.3f, "almost touching: nearly parallel to the wall");
        }

        [Test]
        public void Avoidance_HeadOn_PicksTheSeededSide()
        {
            Vector3 right = EnemySteering.AvoidanceDirection(Vector3.forward, Vector3.back, 0.1f, 2f, preferRight: true);
            Vector3 left = EnemySteering.AvoidanceDirection(Vector3.forward, Vector3.back, 0.1f, 2f, preferRight: false);

            Assert.Greater(right.x, 0.5f);
            Assert.Less(left.x, -0.5f);
        }

        [Test]
        public void ObstacleSlowdown_OnlyWithinLookAhead()
        {
            Assert.AreEqual(1f, EnemySteering.ObstacleSlowdown(-1f, 2f));
            Assert.AreEqual(1f, EnemySteering.ObstacleSlowdown(2.5f, 2f));
            Assert.AreEqual(0.45f, EnemySteering.ObstacleSlowdown(0f, 2f), 1e-5f);
            float half = EnemySteering.ObstacleSlowdown(1f, 2f);
            Assert.Greater(half, 0.45f);
            Assert.Less(half, 1f);
        }

        // ------------------------------------------------------------------ separation

        [Test]
        public void Separation_PushesAway_StrongerWhenCloser_NothingOutsideRange()
        {
            Vector3 acc = Vector3.zero;
            Assert.IsFalse(EnemySteering.AccumulateSeparation(ref acc, Vector3.zero, new Vector3(2f, 0f, 0f), 1f), "outside desired distance");
            Assert.AreEqual(Vector3.zero, acc);

            Assert.IsTrue(EnemySteering.AccumulateSeparation(ref acc, Vector3.zero, new Vector3(0.5f, 0f, 0f), 1f));
            Assert.Less(acc.x, 0f, "away from the neighbour");
            float mid = acc.magnitude;

            acc = Vector3.zero;
            EnemySteering.AccumulateSeparation(ref acc, Vector3.zero, new Vector3(0.1f, 0f, 0f), 1f);
            Assert.Greater(acc.magnitude, mid, "closer = stronger");
        }

        [Test]
        public void Separation_CoincidentNeighbours_StillPart()
        {
            Vector3 acc = Vector3.zero;
            Assert.IsTrue(EnemySteering.AccumulateSeparation(ref acc, Vector3.one, Vector3.one, 1f));
            Assert.Greater(acc.magnitude, 0.5f);
        }

        [Test]
        public void Combine_BendsTowardSeparation_ButNeverAbandonsTheGoal()
        {
            Vector3 goal = Vector3.forward;
            Vector3 push = Vector3.right * 100f; // absurd crowding
            Vector3 result = EnemySteering.Combine(goal, push);

            Assert.AreEqual(1f, result.magnitude, 1e-4f);
            Assert.Greater(result.x, 0f, "bent toward the push");
            Assert.Greater(result.z, 0.2f, "still progresses toward the goal (weight capped)");
            Assert.AreEqual(goal, EnemySteering.Combine(goal, Vector3.zero), "no push = goal unchanged");
            Assert.AreEqual(Vector3.right, EnemySteering.Combine(Vector3.zero, Vector3.right), "no goal = pure separation");
        }

        // ------------------------------------------------------------------ pace / side

        [Test]
        public void Pace_IsDeterministic_BoundedAndVaried()
        {
            float a = EnemySteering.PaceMultiplier(12345, 0.1f);
            Assert.AreEqual(a, EnemySteering.PaceMultiplier(12345, 0.1f));
            Assert.That(a, Is.InRange(0.9f, 1.1f));
            Assert.AreEqual(1f, EnemySteering.PaceMultiplier(12345, 0f), "no spread = exact speed");

            int distinct = 0;
            float previous = float.NaN;
            for (int seed = 1; seed <= 16; seed++)
            {
                float pace = EnemySteering.PaceMultiplier(seed * 7919, 0.1f);
                Assert.That(pace, Is.InRange(0.9f, 1.1f));
                if (!Mathf.Approximately(pace, previous))
                {
                    distinct++;
                }

                previous = pace;
            }

            Assert.Greater(distinct, 8, "a pack does not share one pace");
        }

        [Test]
        public void PrefersRight_IsStablePerSeed_AndNotOneSided()
        {
            int rights = 0;
            for (int seed = 1; seed <= 64; seed++)
            {
                bool right = EnemySteering.PrefersRight(seed * 104729);
                Assert.AreEqual(right, EnemySteering.PrefersRight(seed * 104729));
                if (right)
                {
                    rights++;
                }
            }

            Assert.That(rights, Is.InRange(16, 48), "both sides get picked");
        }
    }
}
