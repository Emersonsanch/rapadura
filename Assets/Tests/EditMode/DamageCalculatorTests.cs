using NUnit.Framework;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>Plain-math tests for the damage mitigation formula — no MonoBehaviour/scene required.</summary>
    public class DamageCalculatorTests
    {
        [Test]
        public void ComputeDamage_WithZeroDefense_DealsFullAmount()
        {
            DamageInfo info = DamageInfo.Simple(50f, ElementType.Physical, null);

            float result = DamageCalculator.ComputeDamage(info, defense: 0f, config: null);

            Assert.AreEqual(50f, result);
        }

        [Test]
        public void ComputeDamage_WithDefenseEqualToArmorConstant_HalvesDamage()
        {
            var config = ScriptableObject.CreateInstance<DamageBalanceConfig>();
            DamageInfo info = DamageInfo.Simple(100f, ElementType.Physical, null);

            // Default armor constant is 100, so defense of 100 should yield ~50% mitigation.
            float result = DamageCalculator.ComputeDamage(info, defense: 100f, config: config);

            Assert.AreEqual(50f, result);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void ComputeDamage_NeverGoesBelowMinimumDamage()
        {
            DamageInfo info = DamageInfo.Simple(1f, ElementType.Physical, null);

            float result = DamageCalculator.ComputeDamage(info, defense: 100000f, config: null);

            Assert.GreaterOrEqual(result, 1f);
        }

        [Test]
        public void ComputeDamage_CriticalHit_MultipliesDamage()
        {
            var normal = new DamageInfo(40f, ElementType.Physical, null, Vector3.zero, Vector3.zero, 0f, isCritical: false);
            var critical = new DamageInfo(40f, ElementType.Physical, null, Vector3.zero, Vector3.zero, 0f, isCritical: true);

            float normalResult = DamageCalculator.ComputeDamage(normal, defense: 0f, config: null);
            float criticalResult = DamageCalculator.ComputeDamage(critical, defense: 0f, config: null);

            Assert.Greater(criticalResult, normalResult);
        }
    }
}
