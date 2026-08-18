using NUnit.Framework;
using Rapadura.Gameplay.Enemies;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Pure-logic tests for the enemy AI decision helpers (detection radius, attack range, flee
    /// thresholds, waypoint arrival) that back Patrol/Chase/Attack/Flee state transitions. None of this
    /// depends on physics, colliders or a running scene.
    /// </summary>
    public class EnemyDetectionUtilityTests
    {
        [Test]
        public void IsWithinRadius_ReturnsTrue_WhenInsideRadius()
        {
            Assert.IsTrue(EnemyDetectionUtility.IsWithinRadius(Vector3.zero, new Vector3(3f, 0f, 0f), 5f));
        }

        [Test]
        public void IsWithinRadius_ReturnsFalse_WhenOutsideRadius()
        {
            Assert.IsFalse(EnemyDetectionUtility.IsWithinRadius(Vector3.zero, new Vector3(10f, 0f, 0f), 5f));
        }

        [Test]
        public void IsWithinRadius_ReturnsFalse_WhenRadiusIsZeroOrNegative()
        {
            Assert.IsFalse(EnemyDetectionUtility.IsWithinRadius(Vector3.zero, Vector3.zero, 0f));
            Assert.IsFalse(EnemyDetectionUtility.IsWithinRadius(Vector3.zero, Vector3.zero, -1f));
        }

        [Test]
        public void HasArrivedAtWaypoint_TrueWithinThreshold()
        {
            Assert.IsTrue(EnemyDetectionUtility.HasArrivedAtWaypoint(Vector3.zero, new Vector3(0.1f, 0f, 0f), 0.25f));
        }

        [Test]
        public void HasArrivedAtWaypoint_FalseBeyondThreshold()
        {
            Assert.IsFalse(EnemyDetectionUtility.HasArrivedAtWaypoint(Vector3.zero, new Vector3(1f, 0f, 0f), 0.25f));
        }

        [TestCase(50f, 100f, 0.2f, false)]
        [TestCase(20f, 100f, 0.2f, true)]
        [TestCase(10f, 100f, 0.2f, true)]
        public void ShouldFlee_MatchesHealthRatioThreshold(float currentHealth, float maxHealth, float threshold, bool expected)
        {
            Assert.AreEqual(expected, EnemyDetectionUtility.ShouldFlee(currentHealth, maxHealth, threshold));
        }

        [Test]
        public void ShouldFlee_FalseWhenMaxHealthIsZero()
        {
            Assert.IsFalse(EnemyDetectionUtility.ShouldFlee(0f, 0f, 0.5f));
        }

        [Test]
        public void CanRecoverFromFlee_IsInverseOfShouldFlee()
        {
            Assert.IsTrue(EnemyDetectionUtility.CanRecoverFromFlee(80f, 100f, 0.2f));
            Assert.IsFalse(EnemyDetectionUtility.CanRecoverFromFlee(10f, 100f, 0.2f));
        }

        [Test]
        public void IsFarEnoughToStopFleeing_TrueWhenBeyondSafeDistance()
        {
            Assert.IsTrue(EnemyDetectionUtility.IsFarEnoughToStopFleeing(Vector3.zero, new Vector3(20f, 0f, 0f), 10f));
        }

        [Test]
        public void IsFarEnoughToStopFleeing_FalseWhenStillClose()
        {
            Assert.IsFalse(EnemyDetectionUtility.IsFarEnoughToStopFleeing(Vector3.zero, new Vector3(5f, 0f, 0f), 10f));
        }

        [Test]
        public void IsTargetLost_TrueWhenOutsideLeashRadius()
        {
            Assert.IsTrue(EnemyDetectionUtility.IsTargetLost(Vector3.zero, new Vector3(15f, 0f, 0f), 12f));
        }

        [Test]
        public void IsTargetLost_FalseWhenWithinLeashRadius()
        {
            Assert.IsFalse(EnemyDetectionUtility.IsTargetLost(Vector3.zero, new Vector3(5f, 0f, 0f), 12f));
        }

        [Test]
        public void IsWithinAttackRange_TrueWhenCloseEnough()
        {
            Assert.IsTrue(EnemyDetectionUtility.IsWithinAttackRange(Vector3.zero, new Vector3(1f, 0f, 0f), 1.5f));
        }

        [Test]
        public void DirectionTowards_PointsFromOriginToTarget_IgnoringY()
        {
            Vector3 direction = EnemyDetectionUtility.DirectionTowards(Vector3.zero, new Vector3(0f, 5f, 10f));
            Assert.AreEqual(new Vector3(0f, 0f, 1f), direction);
        }

        [Test]
        public void DirectionTowards_ReturnsZero_WhenPointsCoincide()
        {
            Assert.AreEqual(Vector3.zero, EnemyDetectionUtility.DirectionTowards(new Vector3(1f, 0f, 1f), new Vector3(1f, 0f, 1f)));
        }

        [Test]
        public void DirectionAwayFrom_IsOppositeOfDirectionTowards()
        {
            Vector3 origin = Vector3.zero;
            Vector3 threat = new Vector3(0f, 0f, 5f);

            Vector3 towards = EnemyDetectionUtility.DirectionTowards(origin, threat);
            Vector3 away = EnemyDetectionUtility.DirectionAwayFrom(origin, threat);

            Assert.AreEqual(-towards, away);
        }

        [Test]
        public void DirectionAwayFrom_ReturnsZero_WhenPointsCoincide()
        {
            Assert.AreEqual(Vector3.zero, EnemyDetectionUtility.DirectionAwayFrom(Vector3.zero, Vector3.zero));
        }
    }
}
