using NUnit.Framework;
using Rapadura.Core.EventBus;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="Health"/>: damage/heal clamping, death, invulnerability
    /// frames and the events it publishes. GameObjects/components can be created directly in
    /// EditMode as long as the test drives their methods explicitly instead of relying on the
    /// Unity player loop.
    /// </summary>
    public class HealthTests
    {
        private GameObject _go;
        private Health _health;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            _go = new GameObject("HealthTestTarget");
            _health = _go.AddComponent<Health>();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void ApplyDamage_ReducesCurrentHealth()
        {
            float before = _health.CurrentHealth;

            _health.ApplyDamage(10f, ElementType.Physical, null);

            Assert.Less(_health.CurrentHealth, before);
        }

        [Test]
        public void ApplyDamage_LethalAmount_KillsTargetAndPublishesEvent()
        {
            bool died = false;
            EventBus.Subscribe<CombatTargetDiedEvent>(_ => died = true);

            _health.ApplyDamage(100000f, ElementType.Physical, null);

            Assert.IsTrue(_health.IsDead);
            Assert.IsFalse(_health.IsAlive);
            Assert.IsTrue(died);
        }

        [Test]
        public void ApplyDamage_PublishesDamageAppliedEvent()
        {
            DamageAppliedEvent? received = null;
            EventBus.Subscribe<DamageAppliedEvent>(evt => received = evt);

            _health.ApplyDamage(5f, ElementType.Fire, null);

            Assert.IsTrue(received.HasValue);
            Assert.AreEqual(_go, received.Value.Target);
            Assert.AreEqual(ElementType.Fire, received.Value.Element);
        }

        [Test]
        public void ApplyDamage_WhileInvulnerable_IsIgnored()
        {
            _health.ApplyDamage(1f, ElementType.Physical, null);
            float healthAfterFirstHit = _health.CurrentHealth;

            // Second hit lands immediately after, while still inside the i-frame window.
            _health.ApplyDamage(1f, ElementType.Physical, null);

            Assert.AreEqual(healthAfterFirstHit, _health.CurrentHealth);
        }

        [Test]
        public void ApplyHealing_IncreasesHealthButNeverAboveMax()
        {
            _health.ApplyDamage(20f, ElementType.Physical, null);

            _health.ApplyHealing(100000f, null);

            Assert.AreEqual(_health.MaxHealth, _health.CurrentHealth);
        }

        [Test]
        public void ApplyDamage_ToDeadTarget_DoesNothing()
        {
            _health.ApplyDamage(100000f, ElementType.Physical, null);
            _health.ClearInvulnerability(); // clear i-frames so we're testing the death gate, not invulnerability

            _health.ApplyDamage(10f, ElementType.Physical, null);

            Assert.AreEqual(0f, _health.CurrentHealth);
        }
    }
}
