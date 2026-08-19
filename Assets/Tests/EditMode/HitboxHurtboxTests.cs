using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the Hitbox/Hurtbox pair. Physics triggers don't fire outside of a
    /// running scene, so these drive <see cref="Hitbox.ProcessTriggerEnter"/> directly — the same
    /// method <c>OnTriggerEnter</c> delegates to — to exercise the exact hit-resolution logic.
    /// </summary>
    public class HitboxHurtboxTests
    {
        private GameObject _attacker;
        private GameObject _target;
        private Hitbox _hitbox;
        private Hurtbox _hurtbox;
        private Health _health;
        private Collider _targetCollider;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();

            _attacker = new GameObject("Attacker");
            _attacker.AddComponent<BoxCollider>().isTrigger = true;
            _hitbox = _attacker.AddComponent<Hitbox>();
            _hitbox.Damage = 10f;

            _target = new GameObject("Target");
            _targetCollider = _target.AddComponent<BoxCollider>();
            _health = _target.AddComponent<Health>();
            _hurtbox = _target.AddComponent<Hurtbox>();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            Object.DestroyImmediate(_attacker);
            Object.DestroyImmediate(_target);
        }

        [Test]
        public void ProcessTriggerEnter_WithHurtbox_DealsDamageToOwner()
        {
            float before = _health.CurrentHealth;

            bool handled = _hitbox.ProcessTriggerEnter(_targetCollider);

            Assert.IsTrue(handled);
            Assert.Less(_health.CurrentHealth, before);
        }

        [Test]
        public void ProcessTriggerEnter_WithoutHurtbox_ReturnsFalseAndDealsNoDamage()
        {
            var plain = new GameObject("NoHurtbox");
            Collider plainCollider = plain.AddComponent<BoxCollider>();

            bool handled = _hitbox.ProcessTriggerEnter(plainCollider);

            Assert.IsFalse(handled);
            Object.DestroyImmediate(plain);
        }

        [Test]
        public void ProcessTriggerEnter_SameTargetTwiceInOneActivation_OnlyHitsOnce()
        {
            _hitbox.ProcessTriggerEnter(_targetCollider);
            float afterFirstHit = _health.CurrentHealth;

            _hitbox.ProcessTriggerEnter(_targetCollider);

            Assert.AreEqual(afterFirstHit, _health.CurrentHealth);
        }

        [Test]
        public void ResetHitTargets_AllowsHittingSameTargetAgain()
        {
            _hitbox.ProcessTriggerEnter(_targetCollider);
            float afterFirstHit = _health.CurrentHealth;
            _health.ClearInvulnerability(); // clear i-frames so the second hit isn't swallowed by Health itself

            _hitbox.ResetHitTargets();
            _hitbox.ProcessTriggerEnter(_targetCollider);

            Assert.Less(_health.CurrentHealth, afterFirstHit);
        }

        [Test]
        public void ProcessTriggerEnter_PublishesDamageAppliedEvent()
        {
            bool received = false;
            EventBus.Subscribe<DamageAppliedEvent>(_ => received = true);

            _hitbox.ProcessTriggerEnter(_targetCollider);

            Assert.IsTrue(received);
        }
    }
}
