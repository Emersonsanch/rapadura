using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.UI.CharacterSheet;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Regression for the "percent on a zero base" bug (Balance §7.6): stats that are 0-based fractions
    /// (crit chance/damage, attack speed, thorns, cooldown reduction, resource cost reduction) must count
    /// Flat and PercentAdditive modifiers alike. affix_of_haste rolls CooldownReduction as PercentAdditive
    /// and SkillManager used to read GetModifiedValue(CooldownReduction, 0) = (0 + flat) × (1 + percent) → 0.
    /// </summary>
    public class ZeroBaseFractionStatTests
    {
        private GameObject _go;
        private BuffController _buffs;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("ZeroBaseFractionStatTests");
            _buffs = _go.AddComponent<BuffController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        private void Apply(StatType stat, ModifierApplication application, float value)
        {
            _buffs.ApplyEffect(new StatModifierDefinition
            {
                displayName = "test",
                affectedStat = stat,
                application = application,
                value = value,
                durationSeconds = 0f
            });
        }

        /// <summary>The production reader for each 0-based fraction stat, as gameplay consumes it.</summary>
        private float Read(StatType stat)
        {
            switch (stat)
            {
                case StatType.CooldownReduction: return SkillManager.ResolveCooldownReduction(_buffs);
                case StatType.ResourceCostReduction: return SkillManager.ResolveResourceCostReduction(_buffs);
                case StatType.CriticalChance: return OutgoingDamage.GetCriticalChance(_buffs, null);
                case StatType.CriticalDamage: return OutgoingDamage.GetCriticalMultiplier(_buffs, null) - PlayerCombatBalance.DefaultCriticalMultiplier;
                case StatType.Thorns: return ThornsRules.GetFraction(_buffs);
                case StatType.AttackSpeed: return _go.AddComponent<PlayerBasicAttack>().AttackSpeedBonus;
                default: throw new System.ArgumentOutOfRangeException(nameof(stat));
            }
        }

        private static readonly StatType[] FractionStats =
        {
            StatType.CooldownReduction, StatType.ResourceCostReduction, StatType.CriticalChance,
            StatType.CriticalDamage, StatType.Thorns, StatType.AttackSpeed
        };

        [Test]
        public void PercentAdditiveCooldownReduction_IsNotDroppedOnZeroBase()
        {
            Apply(StatType.CooldownReduction, ModifierApplication.PercentAdditive, 0.05f);

            Assert.AreEqual(0.05f, SkillManager.ResolveCooldownReduction(_buffs), 1e-5f);
        }

        [Test]
        public void CooldownReduction_FlatAndPercentSumAdditively_AndClampToOne()
        {
            Apply(StatType.CooldownReduction, ModifierApplication.Flat, 0.06f);          // Café coado
            Apply(StatType.CooldownReduction, ModifierApplication.PercentAdditive, 0.08f); // affix_of_haste (max)
            Assert.AreEqual(0.14f, SkillManager.ResolveCooldownReduction(_buffs), 1e-5f,
                "not (0 + 0.06) × 1.08 = 0.0648");

            Apply(StatType.CooldownReduction, ModifierApplication.Flat, 2f);
            Assert.AreEqual(1f, SkillManager.ResolveCooldownReduction(_buffs), 1e-5f);
        }

        [Test]
        public void CooldownReduction_NullBuffs_IsZero()
        {
            Assert.AreEqual(0f, SkillManager.ResolveCooldownReduction(null));
            Assert.AreEqual(0f, SkillManager.ResolveResourceCostReduction(null));
        }

        [Test]
        public void CharacterSheet_ShowsTheSameCooldownReductionTheSkillsUse()
        {
            Apply(StatType.CooldownReduction, ModifierApplication.PercentAdditive, 0.05f);

            CharacterDerivedStats derived = CharacterSheetPresenter.BuildDerivedStats(null, _buffs, null);

            Assert.AreEqual(SkillManager.ResolveCooldownReduction(_buffs), derived.CooldownReduction, 1e-5f);
            Assert.AreEqual(0.05f, derived.CooldownReduction, 1e-5f);
        }

        [Test]
        public void EveryZeroBaseFractionStat_CountsAPercentAdditiveModifier([ValueSource(nameof(FractionStats))] StatType stat)
        {
            Apply(stat, ModifierApplication.PercentAdditive, 0.04f);

            Assert.AreEqual(0.04f, Read(stat), 1e-5f, $"{stat}: a PercentAdditive +4% on a 0 base must still be +0.04");
        }

        [Test]
        public void EveryZeroBaseFractionStat_CountsAFlatModifier([ValueSource(nameof(FractionStats))] StatType stat)
        {
            Apply(stat, ModifierApplication.Flat, 0.04f);

            Assert.AreEqual(0.04f, Read(stat), 1e-5f, $"{stat}: a Flat +0.04 must be +0.04");
        }

        [Test]
        public void EveryShippedFractionAffix_HasAnEffectAtItsMinimumRoll()
        {
            var checkedIds = new List<string>();

            foreach (ItemAffixDefinition affix in Resources.LoadAll<ItemAffixDefinition>("Affixes"))
            {
                if (System.Array.IndexOf(FractionStats, affix.AffectedStat) < 0)
                {
                    continue;
                }

                _buffs.ClearAllEffects();
                Apply(affix.AffectedStat, affix.Application, affix.MinValue);
                Assert.Greater(Read(affix.AffectedStat), 0f, $"{affix.AffixId} ({affix.AffectedStat}, {affix.Application}) does nothing");
                checkedIds.Add(affix.AffixId);
            }

            CollectionAssert.Contains(checkedIds, "affix_of_haste");
        }
    }
}
