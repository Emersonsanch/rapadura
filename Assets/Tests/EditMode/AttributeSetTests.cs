using NUnit.Framework;
using Rapadura.Core.EventBus;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the Fase 3 attribute system: <see cref="AttributeSet"/>'s point economy
    /// and derived-stat formulas, and <see cref="PlayableCharacter.ApplyPassive"/> for the
    /// characters wired to it.
    /// </summary>
    public class AttributeSetTests
    {
        private GameObject _go;
        private PlayerStats _stats;
        private BuffController _buffController;
        private AttributeSet _attributeSet;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            _go = new GameObject("AttributeSetTestTarget");
            _stats = _go.AddComponent<PlayerStats>();
            _buffController = _go.AddComponent<BuffController>();
            _attributeSet = _go.AddComponent<AttributeSet>();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            Object.DestroyImmediate(_go);
        }

        // ---------------------------------------------------------------
        // Point economy
        // ---------------------------------------------------------------

        [Test]
        public void GrantAttributePoints_IncreasesAvailablePoints()
        {
            _attributeSet.GrantAttributePoints(3);

            Assert.AreEqual(3, _attributeSet.AvailableAttributePoints);
        }

        [Test]
        public void TrySpendPoint_WithAvailablePoints_AllocatesAndConsumes()
        {
            _attributeSet.GrantAttributePoints(2);

            bool result = _attributeSet.TrySpendPoint(AttributeType.Vitality);

            Assert.IsTrue(result);
            Assert.AreEqual(1, _attributeSet.AvailableAttributePoints);
            Assert.AreEqual(1, _attributeSet.GetAllocatedPoints(AttributeType.Vitality));
        }

        [Test]
        public void TrySpendPoint_WithoutAvailablePoints_Fails()
        {
            bool result = _attributeSet.TrySpendPoint(AttributeType.Vitality);

            Assert.IsFalse(result);
            Assert.AreEqual(0, _attributeSet.GetAllocatedPoints(AttributeType.Vitality));
        }

        [Test]
        public void LevelUp_ViaExperienceEvent_GrantsAttributePoints()
        {
            // AttributeSet subscribes to PlayerExperienceChangedEvent the same way SkillManager does,
            // so leveling up through PlayerStats should grant attribute points automatically.
            _stats.AddExperience(_stats.GetExperienceToNextLevel());

            Assert.AreEqual(2, _attributeSet.AvailableAttributePoints, "Default is 2 attribute points per level.");
        }

        // ---------------------------------------------------------------
        // Derived stat formulas
        // ---------------------------------------------------------------

        [Test]
        public void SpendingVitality_IncreasesMaxHealthByFormula()
        {
            _attributeSet.GrantAttributePoints(3);
            _attributeSet.TrySpendPoint(AttributeType.Vitality, 3);

            float modified = _buffController.GetModifiedValue(StatType.MaxHealth, _stats.MaxHealth);

            Assert.AreEqual(_stats.MaxHealth + 3 * AttributeSet.VitalityHealthPerPoint, modified, 0.001f);
        }

        [Test]
        public void SpendingSpirit_IncreasesMaxManaByFormula()
        {
            _attributeSet.GrantAttributePoints(2);
            _attributeSet.TrySpendPoint(AttributeType.Spirit, 2);

            float modified = _buffController.GetModifiedValue(StatType.MaxMana, _stats.MaxMana);

            Assert.AreEqual(_stats.MaxMana + 2 * AttributeSet.SpiritManaPerPoint, modified, 0.001f);
        }

        [Test]
        public void SpendingDexterity_IncreasesCriticalChanceByFormula()
        {
            _attributeSet.GrantAttributePoints(4);
            _attributeSet.TrySpendPoint(AttributeType.Dexterity, 4);

            float modified = _buffController.GetModifiedValue(StatType.CriticalChance, 0f);

            Assert.AreEqual(4 * AttributeSet.DexterityCriticalChancePerPoint, modified, 0.0001f);
        }

        [Test]
        public void SpendingIntelligence_IncreasesCooldownReductionByFormula()
        {
            _attributeSet.GrantAttributePoints(1);
            _attributeSet.TrySpendPoint(AttributeType.Intelligence);

            float modified = _buffController.GetModifiedValue(StatType.CooldownReduction, 0f);

            Assert.AreEqual(AttributeSet.IntelligenceCooldownReductionPerPoint, modified, 0.0001f);
        }

        [Test]
        public void SpendingArcanePower_IncreasesAttackDamageByFormula()
        {
            _attributeSet.GrantAttributePoints(1);
            _attributeSet.TrySpendPoint(AttributeType.ArcanePower);

            float modified = _buffController.GetModifiedValue(StatType.AttackDamage, 10f);

            Assert.AreEqual(10f * (1f + AttributeSet.ArcanePowerAttackDamagePerPoint), modified, 0.001f);
        }

        [Test]
        public void ApplyCharacterPassiveBonus_AddsToTotalAndRecomputesDerivedStat()
        {
            _attributeSet.ApplyCharacterPassiveBonus(AttributeType.Vitality, 5);

            Assert.AreEqual(5, _attributeSet.GetTotal(AttributeType.Vitality));
            float modified = _buffController.GetModifiedValue(StatType.MaxHealth, _stats.MaxHealth);
            Assert.AreEqual(_stats.MaxHealth + 5 * AttributeSet.VitalityHealthPerPoint, modified, 0.001f);
        }

        [Test]
        public void TryParsePrimaryAttribute_MapsEveryLoreLabel()
        {
            Assert.IsTrue(AttributeSet.TryParsePrimaryAttribute("Vitalidade", out AttributeType vitality));
            Assert.AreEqual(AttributeType.Vitality, vitality);

            Assert.IsTrue(AttributeSet.TryParsePrimaryAttribute("Espírito", out AttributeType spirit));
            Assert.AreEqual(AttributeType.Spirit, spirit);

            Assert.IsTrue(AttributeSet.TryParsePrimaryAttribute("Destreza", out AttributeType dexterity));
            Assert.AreEqual(AttributeType.Dexterity, dexterity);

            Assert.IsTrue(AttributeSet.TryParsePrimaryAttribute("Inteligência", out AttributeType intelligence));
            Assert.AreEqual(AttributeType.Intelligence, intelligence);

            Assert.IsTrue(AttributeSet.TryParsePrimaryAttribute("Poder Arcano", out AttributeType arcanePower));
            Assert.AreEqual(AttributeType.ArcanePower, arcanePower);

            Assert.IsFalse(AttributeSet.TryParsePrimaryAttribute("Sorte", out _));
        }

        // ---------------------------------------------------------------
        // Character passives (PlayableCharacter.ApplyPassive)
        // ---------------------------------------------------------------

        [Test]
        public void Joaquim_ApplyPassive_GrantsVitalityBonusAndHealsToNewMax()
        {
            PlayableCharacter joaquim = CharacterRegistry.Get(CharacterId.Joaquim);

            joaquim.ApplyPassive(_stats);

            Assert.AreEqual(AttributeSet.CharacterPassiveBonusPoints, _attributeSet.GetTotal(AttributeType.Vitality));
            float modifiedMaxHealth = _buffController.GetModifiedValue(StatType.MaxHealth, _stats.MaxHealth);
            Assert.AreEqual(_stats.MaxHealth + AttributeSet.CharacterPassiveBonusPoints * AttributeSet.VitalityHealthPerPoint, modifiedMaxHealth, 0.001f);
            Assert.AreEqual(_stats.MaxHealth, _stats.CurrentHealth, 0.001f, "Passive should heal the character back to full base health.");
        }

        [Test]
        public void Maithe_ApplyPassive_GrantsDexterityBonusAndRestoresStamina()
        {
            PlayableCharacter maithe = CharacterRegistry.Get(CharacterId.Maithe);
            _stats.ApplyDamage(1f);
            _stats.ClearInvulnerability();

            maithe.ApplyPassive(_stats);

            Assert.AreEqual(AttributeSet.CharacterPassiveBonusPoints, _attributeSet.GetTotal(AttributeType.Dexterity));
            float modifiedCrit = _buffController.GetModifiedValue(StatType.CriticalChance, 0f);
            Assert.AreEqual(AttributeSet.CharacterPassiveBonusPoints * AttributeSet.DexterityCriticalChancePerPoint, modifiedCrit, 0.0001f);
            Assert.AreEqual(_stats.MaxStamina, _stats.CurrentStamina, 0.001f);
        }

        [Test]
        public void Lavine_ApplyPassive_GrantsArcanePowerBonus()
        {
            PlayableCharacter lavine = CharacterRegistry.Get(CharacterId.Lavine);

            lavine.ApplyPassive(_stats);

            Assert.AreEqual(AttributeSet.CharacterPassiveBonusPoints, _attributeSet.GetTotal(AttributeType.ArcanePower));
            float modifiedDamage = _buffController.GetModifiedValue(StatType.AttackDamage, 10f);
            Assert.AreEqual(10f * (1f + AttributeSet.CharacterPassiveBonusPoints * AttributeSet.ArcanePowerAttackDamagePerPoint), modifiedDamage, 0.001f);
        }

        [Test]
        public void ApplyPassive_WithoutAttributeSetComponent_DoesNotThrow()
        {
            var bare = new GameObject("BarePlayerStats");
            PlayerStats bareStats = bare.AddComponent<PlayerStats>();
            PlayableCharacter icaro = CharacterRegistry.Get(CharacterId.Icaro);

            Assert.DoesNotThrow(() => icaro.ApplyPassive(bareStats));

            Object.DestroyImmediate(bare);
        }
    }
}
