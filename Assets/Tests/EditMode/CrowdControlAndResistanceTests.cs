using NUnit.Framework;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the two remaining FASE 2 "Efeitos" items:
    /// stun (crowd control, via <see cref="BuffController"/>) and elemental
    /// resistance/immunity (via <see cref="ElementResistance"/> + <see cref="DamageCalculator"/>).
    ///
    /// Note: full <see cref="PlayerController"/> construction is intentionally NOT exercised here —
    /// PlayerController requires PlayerInputHandler, which requires a Unity <c>PlayerInput</c>
    /// component wired to a real InputActionAsset; AddComponent-ing that chain in EditMode without
    /// an asset assigned throws in Awake. Instead these tests cover the actual decision surface:
    /// BuffController.IsStunned (what PlayerController.Update/FixedUpdate gate on before ticking
    /// the state machine) and the resistance math itself.
    /// </summary>
    public class CrowdControlAndResistanceTests
    {
        private GameObject _go;

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
            {
                Object.DestroyImmediate(_go);
            }
        }

        // ---------------------------------------------------------------
        // Stun (BuffController)
        // ---------------------------------------------------------------

        [Test]
        public void BuffController_InitiallyNotStunned()
        {
            _go = new GameObject("BuffControllerTarget");
            BuffController buffs = _go.AddComponent<BuffController>();

            Assert.IsFalse(buffs.IsStunned);
        }

        [Test]
        public void ApplyStun_SetsIsStunnedTrue()
        {
            _go = new GameObject("BuffControllerTarget");
            BuffController buffs = _go.AddComponent<BuffController>();

            buffs.ApplyStun(2f);

            Assert.IsTrue(buffs.IsStunned);
            Assert.AreEqual(2f, buffs.StunTimeRemaining);
        }

        [Test]
        public void ApplyStun_WithZeroOrNegativeDuration_DoesNotStun()
        {
            _go = new GameObject("BuffControllerTarget");
            BuffController buffs = _go.AddComponent<BuffController>();

            buffs.ApplyStun(0f);
            Assert.IsFalse(buffs.IsStunned);

            buffs.ApplyStun(-1f);
            Assert.IsFalse(buffs.IsStunned);
        }

        [Test]
        public void ApplyStun_ShorterDuration_DoesNotShortenExistingStun()
        {
            _go = new GameObject("BuffControllerTarget");
            BuffController buffs = _go.AddComponent<BuffController>();

            buffs.ApplyStun(5f);
            buffs.ApplyStun(1f); // weaker stun should not reduce the remaining duration

            Assert.AreEqual(5f, buffs.StunTimeRemaining);
        }

        [Test]
        public void ClearStun_EndsStunImmediately()
        {
            _go = new GameObject("BuffControllerTarget");
            BuffController buffs = _go.AddComponent<BuffController>();

            buffs.ApplyStun(3f);
            buffs.ClearStun();

            Assert.IsFalse(buffs.IsStunned);
            Assert.AreEqual(0f, buffs.StunTimeRemaining);
        }

        // ---------------------------------------------------------------
        // Elemental resistance / immunity (ElementResistance + DamageCalculator)
        // ---------------------------------------------------------------

        [Test]
        public void ElementResistance_UnconfiguredElement_ReturnsZero()
        {
            _go = new GameObject("ResistanceTarget");
            ElementResistance resistance = _go.AddComponent<ElementResistance>();

            Assert.AreEqual(0f, resistance.GetResistance(ElementType.Fire));
        }

        [Test]
        public void ElementResistance_SetResistance_IsReadBack()
        {
            _go = new GameObject("ResistanceTarget");
            ElementResistance resistance = _go.AddComponent<ElementResistance>();

            resistance.SetResistance(ElementType.Fire, 0.5f);

            Assert.AreEqual(0.5f, resistance.GetResistance(ElementType.Fire));
        }

        [Test]
        public void ElementResistance_FullResistance_IsImmune()
        {
            _go = new GameObject("ResistanceTarget");
            ElementResistance resistance = _go.AddComponent<ElementResistance>();

            resistance.SetResistance(ElementType.Ice, 1f);

            Assert.IsTrue(resistance.IsImmuneTo(ElementType.Ice));
            Assert.IsFalse(resistance.IsImmuneTo(ElementType.Fire));
        }

        [Test]
        public void DamageCalculator_PartialResistance_ReducesDamage()
        {
            _go = new GameObject("ResistanceTarget");
            ElementResistance resistance = _go.AddComponent<ElementResistance>();
            resistance.SetResistance(ElementType.Fire, 0.5f);

            DamageInfo info = DamageInfo.Simple(100f, ElementType.Fire, null);

            float withResistance = DamageCalculator.ComputeDamage(info, defense: 0f, config: null, resistance: resistance);
            float without = DamageCalculator.ComputeDamage(info, defense: 0f, config: null);

            Assert.Less(withResistance, without);
            Assert.AreEqual(50f, withResistance);
        }

        [Test]
        public void DamageCalculator_FullResistance_DealsZeroDamage_BypassingMinimumDamage()
        {
            _go = new GameObject("ResistanceTarget");
            ElementResistance resistance = _go.AddComponent<ElementResistance>();
            resistance.SetResistance(ElementType.Ice, 1f);

            DamageInfo info = DamageInfo.Simple(9999f, ElementType.Ice, null);

            float result = DamageCalculator.ComputeDamage(info, defense: 0f, config: null, resistance: resistance);

            Assert.AreEqual(0f, result);
        }

        [Test]
        public void DamageCalculator_Weakness_NegativeResistance_IncreasesDamage()
        {
            _go = new GameObject("ResistanceTarget");
            ElementResistance resistance = _go.AddComponent<ElementResistance>();
            resistance.SetResistance(ElementType.Lightning, -0.5f);

            DamageInfo info = DamageInfo.Simple(100f, ElementType.Lightning, null);

            float result = DamageCalculator.ComputeDamage(info, defense: 0f, config: null, resistance: resistance);

            Assert.AreEqual(150f, result);
        }

        [Test]
        public void Health_WithFullResistance_TakesNoDamage()
        {
            _go = new GameObject("HealthResistanceTarget");
            ElementResistance resistance = _go.AddComponent<ElementResistance>();
            resistance.SetResistance(ElementType.Fire, 1f);
            Health health = _go.AddComponent<Health>();
            float before = health.CurrentHealth;

            health.ApplyDamage(50f, ElementType.Fire, null);

            Assert.AreEqual(before, health.CurrentHealth);
        }

        [Test]
        public void Health_WithoutResistanceComponent_StillTakesDamage()
        {
            _go = new GameObject("HealthNoResistanceTarget");
            Health health = _go.AddComponent<Health>();
            float before = health.CurrentHealth;

            health.ApplyDamage(10f, ElementType.Fire, null);

            Assert.Less(health.CurrentHealth, before);
        }

        [Test]
        public void PlayerStats_WithFullResistance_TakesNoDamage()
        {
            _go = new GameObject("PlayerStatsResistanceTarget");
            ElementResistance resistance = _go.AddComponent<ElementResistance>();
            resistance.SetResistance(ElementType.Poison, 1f);
            PlayerStats stats = _go.AddComponent<PlayerStats>();
            float before = stats.CurrentHealth;

            ((ICombatTarget)stats).ApplyDamage(25f, ElementType.Poison, null);

            Assert.AreEqual(before, stats.CurrentHealth);
        }

        [Test]
        public void PlayerStats_WithPartialResistance_ReducesDamage()
        {
            _go = new GameObject("PlayerStatsPartialResistanceTarget");
            ElementResistance resistance = _go.AddComponent<ElementResistance>();
            resistance.SetResistance(ElementType.Poison, 0.5f);
            PlayerStats stats = _go.AddComponent<PlayerStats>();
            float before = stats.CurrentHealth;

            ((ICombatTarget)stats).ApplyDamage(20f, ElementType.Poison, null);

            Assert.AreEqual(before - 10f, stats.CurrentHealth);
        }
    }
}
