using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="PlayerStats"/>'s invulnerability frames, mirroring
    /// <c>HealthTests</c>'s coverage of the equivalent behavior on <c>Health</c>.
    /// </summary>
    public class PlayerStatsTests
    {
        private GameObject _go;
        private PlayerStats _stats;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            _go = new GameObject("PlayerStatsTestTarget");
            _stats = _go.AddComponent<PlayerStats>();
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
            float before = _stats.CurrentHealth;

            _stats.ApplyDamage(10f);

            Assert.Less(_stats.CurrentHealth, before);
        }

        [Test]
        public void ApplyDamage_GrantsInvulnerability()
        {
            _stats.ApplyDamage(1f);

            Assert.IsTrue(_stats.IsInvulnerable);
        }

        [Test]
        public void ApplyDamage_WhileInvulnerable_IsIgnored()
        {
            _stats.ApplyDamage(1f);
            float healthAfterFirstHit = _stats.CurrentHealth;

            // Second hit lands immediately after, while still inside the i-frame window.
            _stats.ApplyDamage(1f);

            Assert.AreEqual(healthAfterFirstHit, _stats.CurrentHealth);
        }

        [Test]
        public void ClearInvulnerability_AllowsImmediateFollowUpDamage()
        {
            _stats.ApplyDamage(1f);
            _stats.ClearInvulnerability();
            float healthAfterFirstHit = _stats.CurrentHealth;

            _stats.ApplyDamage(1f);

            Assert.Less(_stats.CurrentHealth, healthAfterFirstHit);
        }

        [Test]
        public void GrantInvulnerability_DoesNotShortenAnAlreadyLongerWindow()
        {
            _stats.GrantInvulnerability(5f);
            _stats.GrantInvulnerability(0.1f);

            Assert.IsTrue(_stats.IsInvulnerable);
        }

        [Test]
        public void ApplyDamage_LethalAmount_StillKillsTargetDespiteIFrames()
        {
            _stats.ApplyDamage(100000f);

            Assert.IsTrue(_stats.IsDead);
            Assert.IsFalse(_stats.IsAlive);
        }

        [Test]
        public void ApplyDamage_ToDeadTarget_DoesNothing()
        {
            _stats.ApplyDamage(100000f);
            _stats.ClearInvulnerability();

            _stats.ApplyDamage(10f);

            Assert.AreEqual(0f, _stats.CurrentHealth);
        }

        [Test]
        public void ICombatTarget_ApplyDamage_AlsoGrantsInvulnerability()
        {
            var target = (Rapadura.Gameplay.Combat.ICombatTarget)_stats;

            target.ApplyDamage(1f, Rapadura.Gameplay.Combat.ElementType.Physical, null);

            Assert.IsTrue(_stats.IsInvulnerable);
        }

        [Test]
        public void ConsumeMana_PublishesPlayerManaChangedEvent()
        {
            PlayerManaChangedEvent? received = null;
            EventBus.Subscribe<PlayerManaChangedEvent>(evt => received = evt);

            _stats.ConsumeMana(10f);

            Assert.IsTrue(received.HasValue);
            Assert.AreEqual(_stats.CurrentMana, received.Value.CurrentMana);
            Assert.AreEqual(_stats.MaxMana, received.Value.MaxMana);
        }

        [Test]
        public void RestoreMana_PublishesPlayerManaChangedEvent_WithUpdatedValue()
        {
            _stats.ConsumeMana(20f);

            PlayerManaChangedEvent? received = null;
            EventBus.Subscribe<PlayerManaChangedEvent>(evt => received = evt);

            _stats.RestoreMana(5f);

            Assert.IsTrue(received.HasValue);
            Assert.AreEqual(_stats.CurrentMana, received.Value.CurrentMana);
        }
    }
}
