using System.Reflection;
using NUnit.Framework;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.States;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Drives EnemyController's Patrol/Chase/Attack/Flee state machine with simulated positions/health,
    /// without relying on real physics (no colliders/raycasts/NavMesh involved) — only the Transform-based
    /// movement and the EnemyDetectionUtility math the states are built on.
    /// </summary>
    public class EnemyControllerStateTests
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

        private static EnemyDefinition CreateDefinition(
            float maxHealth = 100f,
            float detectionRadius = 8f,
            float loseTargetRadius = 12f,
            float attackRange = 1.5f,
            float attackCooldown = 1f,
            float attackDamage = 8f,
            float fleeHealthThreshold = 0.2f,
            float fleeSafeDistance = 10f,
            float chaseSpeed = 4f,
            float fleeSpeed = 5f,
            float patrolSpeed = 2f,
            bool isBoss = false)
        {
            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            SetPrivateField(definition, "_maxHealth", maxHealth);
            SetPrivateField(definition, "_detectionRadius", detectionRadius);
            SetPrivateField(definition, "_loseTargetRadius", loseTargetRadius);
            SetPrivateField(definition, "_attackRange", attackRange);
            SetPrivateField(definition, "_attackCooldown", attackCooldown);
            SetPrivateField(definition, "_attackDamage", attackDamage);
            SetPrivateField(definition, "_fleeHealthThreshold", fleeHealthThreshold);
            SetPrivateField(definition, "_fleeSafeDistance", fleeSafeDistance);
            SetPrivateField(definition, "_chaseSpeed", chaseSpeed);
            SetPrivateField(definition, "_fleeSpeed", fleeSpeed);
            SetPrivateField(definition, "_patrolSpeed", patrolSpeed);
            SetPrivateField(definition, "_isBoss", isBoss);
            return definition;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Expected private field '{fieldName}' on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        [Test]
        public void Patrol_StaysPatrolling_WhenTargetOutsideDetectionRadius()
        {
            var definition = CreateDefinition(detectionRadius: 5f);
            _controller.ResetForSpawn(definition, Vector3.zero);
            _targetGameObject.transform.position = new Vector3(50f, 0f, 0f);

            _controller.StateMachine.Tick(0.1f);

            Assert.IsTrue(_controller.StateMachine.IsInState<EnemyPatrolState>());
        }

        [Test]
        public void Patrol_TransitionsToChase_WhenTargetEntersDetectionRadius()
        {
            var definition = CreateDefinition(detectionRadius: 8f);
            _controller.ResetForSpawn(definition, Vector3.zero);
            _targetGameObject.transform.position = new Vector3(3f, 0f, 0f);

            _controller.StateMachine.Tick(0.1f);

            Assert.IsTrue(_controller.StateMachine.IsInState<EnemyChaseState>());
        }

        [Test]
        public void Chase_TransitionsToAttack_WhenWithinAttackRange()
        {
            var definition = CreateDefinition(detectionRadius: 8f, attackRange: 2f);
            _controller.ResetForSpawn(definition, Vector3.zero);
            _controller.StateMachine.ChangeState<EnemyChaseState>();
            _targetGameObject.transform.position = new Vector3(1f, 0f, 0f);

            _controller.StateMachine.Tick(0.1f);

            Assert.IsTrue(_controller.StateMachine.IsInState<EnemyAttackState>());
        }

        [Test]
        public void Chase_TransitionsBackToPatrol_WhenTargetLeavesLeashRadius()
        {
            var definition = CreateDefinition(loseTargetRadius: 12f, attackRange: 1f);
            _controller.ResetForSpawn(definition, Vector3.zero);
            _controller.StateMachine.ChangeState<EnemyChaseState>();
            _targetGameObject.transform.position = new Vector3(50f, 0f, 0f);

            _controller.StateMachine.Tick(0.1f);

            Assert.IsTrue(_controller.StateMachine.IsInState<EnemyPatrolState>());
        }

        [Test]
        public void Chase_TransitionsToFlee_WhenHealthDropsBelowThreshold()
        {
            var definition = CreateDefinition(maxHealth: 100f, fleeHealthThreshold: 0.2f, attackRange: 1f, loseTargetRadius: 50f);
            _controller.ResetForSpawn(definition, Vector3.zero);
            _controller.StateMachine.ChangeState<EnemyChaseState>();
            _targetGameObject.transform.position = new Vector3(20f, 0f, 0f);

            _health.ApplyDamage(90f, ElementType.Physical, null);
            Assert.LessOrEqual(_health.CurrentHealth, 10f);

            _controller.StateMachine.Tick(0.1f);

            Assert.IsTrue(_controller.StateMachine.IsInState<EnemyFleeState>());
        }

        [Test]
        public void Attack_TransitionsBackToChase_WhenTargetLeavesAttackRange()
        {
            var definition = CreateDefinition(attackRange: 1.5f);
            _controller.ResetForSpawn(definition, Vector3.zero);
            _controller.StateMachine.ChangeState<EnemyAttackState>();
            _targetGameObject.transform.position = new Vector3(10f, 0f, 0f);

            _controller.StateMachine.Tick(0.1f);

            Assert.IsTrue(_controller.StateMachine.IsInState<EnemyChaseState>());
        }

        [Test]
        public void Attack_DamagesTarget_WhenTargetHasCombatComponent()
        {
            var definition = CreateDefinition(attackRange: 5f, attackCooldown: 1f, attackDamage: 15f);
            _controller.ResetForSpawn(definition, Vector3.zero);
            _controller.StateMachine.ChangeState<EnemyAttackState>();

            var targetHealth = _targetGameObject.AddComponent<Health>();
            _targetGameObject.transform.position = new Vector3(1f, 0f, 0f);

            float healthBefore = targetHealth.CurrentHealth;
            _controller.StateMachine.Tick(0.1f);

            Assert.Less(targetHealth.CurrentHealth, healthBefore);
        }

        [Test]
        public void Flee_TransitionsBackToPatrol_WhenHealthRecoveredAndSafeDistance()
        {
            var definition = CreateDefinition(maxHealth: 100f, fleeHealthThreshold: 0.2f, fleeSafeDistance: 10f);
            _controller.ResetForSpawn(definition, Vector3.zero);
            _controller.StateMachine.ChangeState<EnemyFleeState>();
            _targetGameObject.transform.position = new Vector3(50f, 0f, 0f);

            // Health starts at max (100) from ResetForSpawn, well above the 20% flee threshold,
            // and the target is already far beyond the safe distance.
            _controller.StateMachine.Tick(0.1f);

            Assert.IsTrue(_controller.StateMachine.IsInState<EnemyPatrolState>());
        }

        [Test]
        public void Flee_StaysFleeing_WhenStillLowHealthAndClose()
        {
            var definition = CreateDefinition(maxHealth: 100f, fleeHealthThreshold: 0.2f, fleeSafeDistance: 10f, fleeSpeed: 5f);
            _controller.ResetForSpawn(definition, Vector3.zero);
            _health.ApplyDamage(95f, ElementType.Physical, null);
            _controller.StateMachine.ChangeState<EnemyFleeState>();
            _targetGameObject.transform.position = new Vector3(2f, 0f, 0f);

            _controller.StateMachine.Tick(0.1f);

            Assert.IsTrue(_controller.StateMachine.IsInState<EnemyFleeState>());
        }

        [Test]
        public void ResetForSpawn_AppliesBossDefinitionMaxHealthToHealthComponent()
        {
            var bossDefinition = CreateDefinition(maxHealth: 500f, isBoss: true);

            _controller.ResetForSpawn(bossDefinition, Vector3.zero);

            Assert.AreEqual(500f, _health.MaxHealth);
            Assert.AreEqual(500f, _health.CurrentHealth);
            Assert.IsTrue(_controller.IsBoss);
        }
    }
}
