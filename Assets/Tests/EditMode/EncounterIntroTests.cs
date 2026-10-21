using System.Reflection;
using NUnit.Framework;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.States;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.Encounter;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Encounter start: enemies are placed on an arc 6-9 m in front of the player, inside the arena
    /// (<see cref="EncounterSpawnLayout"/>), and hold their aggro during the "Prepare-se!" grace
    /// (<see cref="EnemyController.HoldAggro"/>) — no chase, no attack — while damage to them still lands.
    /// </summary>
    public class EncounterIntroTests
    {
        private GameObject _enemyGameObject;
        private GameObject _targetGameObject;
        private EnemyController _controller;
        private Health _health;

        [SetUp]
        public void SetUp()
        {
            _enemyGameObject = new GameObject("Enemy");
            _health = _enemyGameObject.AddComponent<Health>();
            _controller = _enemyGameObject.AddComponent<EnemyController>();
            _targetGameObject = new GameObject("Target");
            _controller.Target = _targetGameObject.transform;
        }

        [TearDown]
        public void TearDown()
        {
            if (_enemyGameObject != null) Object.DestroyImmediate(_enemyGameObject);
            if (_targetGameObject != null) Object.DestroyImmediate(_targetGameObject);
        }

        // ------------------------------------------------------------------
        // Spawn ring
        // ------------------------------------------------------------------

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        public void RingPoints_AreWithinRadii_AndInFrontOfThePlayer(int count)
        {
            Vector3 player = new Vector3(2f, 0.5f, -3f);
            Vector3 forward = new Vector3(1f, 0f, 1f).normalized;

            for (int i = 0; i < count; i++)
            {
                Vector3 point = EncounterSpawnLayout.RingPoint(player, forward, i, count, 6f, 9f, 120f);
                float distance = EncounterSpawnLayout.FlatDistance(point, player);

                Assert.GreaterOrEqual(distance, 6f - 0.001f, $"enemy {i}/{count} too close");
                Assert.LessOrEqual(distance, 9f + 0.001f, $"enemy {i}/{count} too far");
                Vector3 toPoint = (point - player);
                toPoint.y = 0f;
                Assert.GreaterOrEqual(Vector3.Angle(forward, toPoint), 0f);
                Assert.LessOrEqual(Vector3.Angle(forward, toPoint), 60f + 0.01f, $"enemy {i}/{count} outside the 120° arc");
                Assert.AreEqual(player.y, point.y, 0.0001f, "the ring keeps the player's height (ground snap is the controller's)");
            }
        }

        [Test]
        public void RingPoints_SingleEnemy_StandsStraightAheadMidway()
        {
            Vector3 point = EncounterSpawnLayout.RingPoint(Vector3.zero, Vector3.forward, 0, 1, 6f, 9f, 120f);

            Assert.AreEqual(0f, point.x, 0.0001f);
            Assert.AreEqual(7.5f, point.z, 0.0001f);
        }

        [Test]
        public void RingPoints_ArePairwiseApart()
        {
            const int count = 4;
            var points = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                points[i] = EncounterSpawnLayout.RingPoint(Vector3.zero, Vector3.forward, i, count, 6f, 9f, 120f);
            }

            for (int a = 0; a < count; a++)
            {
                for (int b = a + 1; b < count; b++)
                {
                    Assert.Greater(EncounterSpawnLayout.FlatDistance(points[a], points[b]), 2f, $"enemies {a} and {b} overlap");
                }
            }
        }

        [Test]
        public void RingPoints_ZeroForward_FallsBackToWorldForward_AndClampsIndex()
        {
            Vector3 point = EncounterSpawnLayout.RingPoint(Vector3.zero, Vector3.up, 0, 1, 6f, 6f, 90f);
            Assert.AreEqual(6f, point.z, 0.0001f);

            Vector3 clamped = EncounterSpawnLayout.RingPoint(Vector3.zero, Vector3.forward, 99, 3, 6f, 9f, 120f);
            Vector3 last = EncounterSpawnLayout.RingPoint(Vector3.zero, Vector3.forward, 2, 3, 6f, 9f, 120f);
            Assert.AreEqual(last, clamped);
        }

        [Test]
        public void ClampToArena_PullsOutsidePointsBackOnTheCircle()
        {
            Vector3 center = new Vector3(10f, 0f, 0f);

            Vector3 inside = new Vector3(12f, 1f, 3f);
            Assert.AreEqual(inside, EncounterSpawnLayout.ClampToArena(inside, center, 18f));

            Vector3 outside = new Vector3(40f, 2f, 0f);
            Vector3 clamped = EncounterSpawnLayout.ClampToArena(outside, center, 18f);
            Assert.AreEqual(18f, EncounterSpawnLayout.FlatDistance(clamped, center), 0.001f);
            Assert.AreEqual(2f, clamped.y, 0.0001f, "Y untouched");

            Assert.AreEqual(outside, EncounterSpawnLayout.ClampToArena(outside, center, 0f), "0 disables the clamp");
        }

        [Test]
        public void ArenaDefaults_SpawnFarEnoughFromThePlayer()
        {
            Assert.GreaterOrEqual(EncounterSpawnLayout.DefaultMinRadius, 6f);
            Assert.LessOrEqual(EncounterSpawnLayout.DefaultMaxRadius, 9f);
        }

        // ------------------------------------------------------------------
        // Grace period (aggro hold)
        // ------------------------------------------------------------------

        private static EnemyDefinition CreateDefinition(float attackRange = 5f, float detectionRadius = 20f, float attackDamage = 15f)
        {
            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            SetPrivateField(definition, "_maxHealth", 100f);
            SetPrivateField(definition, "_detectionRadius", detectionRadius);
            SetPrivateField(definition, "_loseTargetRadius", 50f);
            SetPrivateField(definition, "_attackRange", attackRange);
            SetPrivateField(definition, "_attackCooldown", 1f);
            SetPrivateField(definition, "_attackDamage", attackDamage);
            SetPrivateField(definition, "_fleeHealthThreshold", 0f);
            SetPrivateField(definition, "_chaseSpeed", 4f);
            return definition;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Expected private field '{fieldName}' on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        [Test]
        public void HoldAggro_EnemyNeitherChasesNorAttacks_UntilTheGraceEnds()
        {
            _controller.ResetForSpawn(CreateDefinition(), Vector3.zero);
            Health targetHealth = _targetGameObject.AddComponent<Health>();
            _targetGameObject.transform.position = new Vector3(1f, 0f, 0f); // inside attack range
            float before = targetHealth.CurrentHealth;

            _controller.HoldAggro(1.75f);
            for (int i = 0; i < 17; i++)
            {
                _controller.TickBrain(0.1f);
            }

            Assert.IsTrue(_controller.IsHoldingAggro);
            Assert.IsTrue(_controller.StateMachine.IsInState<EnemyPatrolState>(), "no chase while holding");
            Assert.AreEqual(before, targetHealth.CurrentHealth, "no attack during the grace");
            Assert.AreEqual(Vector3.zero, _enemyGameObject.transform.position, "no movement during the grace");

            // Grace runs out, then the normal Patrol -> Chase -> Attack chain resumes.
            _controller.TickBrain(0.1f);
            Assert.IsFalse(_controller.IsHoldingAggro);
            for (int i = 0; i < 3; i++)
            {
                _controller.TickBrain(0.1f);
            }

            Assert.Less(targetHealth.CurrentHealth, before, "attacks resume after the grace");
        }

        [Test]
        public void HoldAggro_DoesNotBlockDamageToTheEnemy()
        {
            _controller.ResetForSpawn(CreateDefinition(), Vector3.zero);
            _controller.HoldAggro(5f);

            _health.ApplyDamage(1_000_000f, ElementType.Physical, _targetGameObject);

            Assert.IsTrue(_health.IsDead, "the PlayMode smoke test kills enemies right after spawn");
        }

        [Test]
        public void HoldAggro_NeverShortensALongerHold_AndResetForSpawnClearsIt()
        {
            _controller.ResetForSpawn(CreateDefinition(), Vector3.zero);
            _controller.HoldAggro(2f);
            _controller.HoldAggro(0.5f);
            Assert.AreEqual(2f, _controller.AggroHoldRemaining, 0.0001f);

            _controller.ReleaseAggro();
            Assert.IsFalse(_controller.IsHoldingAggro);

            _controller.HoldAggro(2f);
            _controller.ResetForSpawn(CreateDefinition(), Vector3.zero);
            Assert.IsFalse(_controller.IsHoldingAggro, "a pooled re-spawn starts without a stale hold");
        }

        [Test]
        public void HoldAggro_FacesTheTargetWhileWaiting()
        {
            _controller.ResetForSpawn(CreateDefinition(), Vector3.zero);
            _enemyGameObject.transform.rotation = Quaternion.LookRotation(Vector3.back);
            _targetGameObject.transform.position = new Vector3(0f, 0f, 8f);

            _controller.HoldAggro(5f);
            for (int i = 0; i < 20; i++)
            {
                _controller.TickBrain(0.1f);
            }

            Assert.Less(Vector3.Angle(_enemyGameObject.transform.forward, Vector3.forward), 1f);
        }

        [Test]
        public void IntroCountdown_RoundsUpWholeSeconds_ThenZero()
        {
            Assert.AreEqual(2, BossIntroBannerController.CountdownValue(1.75f));
            Assert.AreEqual(1, BossIntroBannerController.CountdownValue(0.4f));
            Assert.AreEqual(0, BossIntroBannerController.CountdownValue(0f));
            Assert.AreEqual(0, BossIntroBannerController.CountdownValue(-1f));
        }
    }
}
