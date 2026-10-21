using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Regression tests for the "dead attributes" bug (Docs/Balance/README.md §2.4): Vitalidade
    /// (MaxHealth), Espírito (MaxMana), Destreza (CriticalChance) and the <c>affix_vital</c> item affix
    /// used to push modifiers into the <see cref="BuffController"/> that no code read. These assert the
    /// gameplay-visible result (PlayerStats maxima, actual crit damage dealt), not the modifier itself.
    /// </summary>
    public class AttributeEffectsTests
    {
        private GameObject _go;
        private PlayerStats _stats;
        private BuffController _buffs;
        private AttributeSet _attributes;
        private readonly System.Collections.Generic.List<Object> _created = new System.Collections.Generic.List<Object>();

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            _go = new GameObject("AttributeEffectsTests_Player");
            _stats = _go.AddComponent<PlayerStats>();
            _buffs = _go.AddComponent<BuffController>();
            _attributes = _go.AddComponent<AttributeSet>();
            _attributes.ForceInitializeForTests();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            Object.DestroyImmediate(_go);

            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
        }

        private void Spend(AttributeType type, int points)
        {
            _attributes.GrantAttributePoints(points);
            Assert.IsTrue(_attributes.TrySpendPoint(type, points));
        }

        private static StatModifierDefinition Permanent(StatType stat, ModifierApplication application, float value)
        {
            return new StatModifierDefinition
            {
                displayName = "test_" + stat,
                affectedStat = stat,
                application = application,
                value = value,
                durationSeconds = 0f // <= 0 never expires (same as equipment bonuses)
            };
        }

        // ---------------------------------------------------------------
        // Max health / max mana
        // ---------------------------------------------------------------

        [Test]
        public void Vitality_RaisesPlayerMaxHealth_AndAFullBarFollowsTheNewMax()
        {
            float baseMax = _stats.MaxHealth;
            Assert.AreEqual(baseMax, _stats.CurrentHealth, 0.001f);

            Spend(AttributeType.Vitality, 4);

            Assert.AreEqual(baseMax + 4 * AttributeSet.VitalityHealthPerPoint, _stats.MaxHealth, 0.001f);
            Assert.AreEqual(_stats.MaxHealth, _stats.CurrentHealth, 0.001f, "A full health bar should stay full when max health rises.");
        }

        [Test]
        public void Vitality_WhileDamaged_RaisesMaxButDoesNotHeal()
        {
            _stats.ApplyDamage(30f);
            float current = _stats.CurrentHealth;

            Spend(AttributeType.Vitality, 2);

            Assert.AreEqual(current, _stats.CurrentHealth, 0.001f, "Raising max health must not act as a free heal.");
            Assert.Greater(_stats.MaxHealth, current);
        }

        [Test]
        public void Spirit_RaisesPlayerMaxMana()
        {
            float baseMax = _stats.MaxMana;

            Spend(AttributeType.Spirit, 3);

            Assert.AreEqual(baseMax + 3 * AttributeSet.SpiritManaPerPoint, _stats.MaxMana, 0.001f);
            Assert.AreEqual(_stats.MaxMana, _stats.CurrentMana, 0.001f);
        }

        [Test]
        public void LosingAMaxHealthBonus_ClampsCurrentHealthDown()
        {
            float baseMax = _stats.MaxHealth;
            _buffs.ApplyEffect(Permanent(StatType.MaxHealth, ModifierApplication.Flat, 50f), "equip_Chest");
            Assert.AreEqual(baseMax + 50f, _stats.CurrentHealth, 0.001f);

            _buffs.RemoveEffectsBySource("equip_Chest");

            Assert.AreEqual(baseMax, _stats.MaxHealth, 0.001f);
            Assert.AreEqual(baseMax, _stats.CurrentHealth, 0.001f, "Current health must never exceed the (reduced) max.");
        }

        [Test]
        public void LosingAMaxManaBonus_ClampsCurrentManaDown()
        {
            float baseMax = _stats.MaxMana;
            _buffs.ApplyEffect(Permanent(StatType.MaxMana, ModifierApplication.Flat, 40f), "equip_Head");
            Assert.AreEqual(baseMax + 40f, _stats.CurrentMana, 0.001f);

            _buffs.ClearAllEffects();

            Assert.AreEqual(baseMax, _stats.CurrentMana, 0.001f);
        }

        [Test]
        public void MaxHealthChange_PublishesHealthChangedEventWithNewMax()
        {
            float baseMax = _stats.MaxHealth;
            Assert.AreEqual(baseMax, _stats.CurrentHealth, 0.001f); // initializes vitals (no Awake in EditMode)
            PlayerHealthChangedEvent? received = null;
            EventBus.Subscribe<PlayerHealthChangedEvent>(e => received = e);

            Spend(AttributeType.Vitality, 1);

            Assert.IsTrue(received.HasValue, "The HUD health bar must hear about the new max.");
            Assert.AreEqual(baseMax + AttributeSet.VitalityHealthPerPoint, received.Value.MaxHealth, 0.001f);
        }

        [Test]
        public void AffixVital_OnEquipment_RaisesMaxHealth_AndStacksWithVitality()
        {
            ItemAffixDefinition vital = AffixDatabase.GetById("affix_vital");
            Assert.IsNotNull(vital, "Expected Resources/Affixes/affix_vital.asset.");
            Assert.AreEqual(StatType.MaxHealth, vital.AffectedStat);

            float baseMax = _stats.MaxHealth;
            Spend(AttributeType.Vitality, 2);
            // Same shape InventoryManager.ApplyEquipEffects uses for rolled affixes (source "equip_<slot>").
            _buffs.ApplyEffect(Permanent(vital.AffectedStat, vital.Application, 20f), "equip_Chest");

            // Before the fix BuffController replaced by stat regardless of source, so the item wiped
            // the Vitality bonus (and nothing read either of them anyway).
            Assert.AreEqual(baseMax + 2 * AttributeSet.VitalityHealthPerPoint + 20f, _stats.MaxHealth, 0.001f);
        }

        [Test]
        public void SameSourceReapply_StillReplacesInsteadOfStacking()
        {
            _buffs.ApplyEffect(Permanent(StatType.MaxHealth, ModifierApplication.Flat, 10f), "equip_Chest");
            _buffs.ApplyEffect(Permanent(StatType.MaxHealth, ModifierApplication.Flat, 25f), "equip_Chest");

            Assert.AreEqual(25f, _buffs.GetModifiedValue(StatType.MaxHealth, 0f), 0.001f);
        }

        // ---------------------------------------------------------------
        // Critical hits
        // ---------------------------------------------------------------

        [Test]
        public void RollCritical_RespectsChanceBoundaries()
        {
            Assert.IsFalse(OutgoingDamage.RollCritical(0f, 0f), "0% never crits.");
            Assert.IsTrue(OutgoingDamage.RollCritical(1f, 1f), "100% always crits.");
            Assert.IsTrue(OutgoingDamage.RollCritical(0.25f, 0.2499f));
            Assert.IsFalse(OutgoingDamage.RollCritical(0.25f, 0.25f));
        }

        [Test]
        public void Dexterity_RaisesCriticalChance_AndInjectedRollDecidesTheCrit()
        {
            Spend(AttributeType.Dexterity, 10); // 10 × 0.5% = 5%
            PlayerCombatBalance balance = PlayerCombatBalance.CreateForTests(10f, 0f, 1.5f);
            _created.Add(balance);

            Assert.AreEqual(0.05f, OutgoingDamage.GetCriticalChance(_buffs, balance), 0.0001f);

            float crit = OutgoingDamage.Resolve(20f, 1f, _buffs, balance, () => 0.04f, out bool isCrit);
            Assert.IsTrue(isCrit);
            Assert.AreEqual(30f, crit, 0.001f);

            float normal = OutgoingDamage.Resolve(20f, 1f, _buffs, balance, () => 0.06f, out bool isNormal);
            Assert.IsFalse(isNormal);
            Assert.AreEqual(20f, normal, 0.001f);
        }

        [Test]
        public void WithoutAnyCritSource_NeverCrits_AndNeverSamplesTheRng()
        {
            bool sampled = false;
            float damage = OutgoingDamage.Resolve(20f, 1f, _buffs, null, () => { sampled = true; return 0f; }, out bool isCrit);

            Assert.IsFalse(isCrit);
            Assert.IsFalse(sampled);
            Assert.AreEqual(20f, damage, 0.001f);
        }

        [Test]
        public void CriticalDamageModifier_RaisesTheCritMultiplier_AndPercentCritAffixesCount()
        {
            // Affixes roll CriticalChance/CriticalDamage as PercentAdditive on a 0 base (AffixDatabaseSeeder);
            // they must count as plain fractions, same as Destreza's Flat modifier.
            _buffs.ApplyEffect(Permanent(StatType.CriticalChance, ModifierApplication.PercentAdditive, 0.03f), "equip_MainHand");
            _buffs.ApplyEffect(Permanent(StatType.CriticalDamage, ModifierApplication.PercentAdditive, 0.25f), "equip_MainHand");

            Assert.AreEqual(0.03f, OutgoingDamage.GetCriticalChance(_buffs, null), 0.0001f);
            Assert.AreEqual(1.75f, OutgoingDamage.GetCriticalMultiplier(_buffs, null), 0.0001f);
        }

        [Test]
        public void ArcanePower_AndCrit_ApplyToTheBasicAttack()
        {
            PlayerBasicAttack attack = _go.AddComponent<PlayerBasicAttack>();
            float baseDamage = attack.BaseDamage;
            Spend(AttributeType.ArcanePower, 10); // +10% AttackDamage
            Spend(AttributeType.Dexterity, 200); // 100% crit

            attack.CriticalRoll = () => 0.5f;
            float damage = attack.ComputeHitDamage(out bool isCrit);

            Assert.IsTrue(isCrit);
            Assert.AreEqual(baseDamage * 1.1f * OutgoingDamage.GetCriticalMultiplier(_buffs, PlayerCombatBalance.LoadDefault()), damage, 0.01f);
        }

        [Test]
        public void SkillCast_CanCrit_AndFlagsTheDamageEventWithoutDoubleMultiplying()
        {
            SkillManager skills = _go.AddComponent<SkillManager>();
            skills.CriticalRoll = () => 0f;
            Spend(AttributeType.Dexterity, 200); // 100% crit

            var targetGo = new GameObject("AttributeEffectsTests_Target");
            _created.Add(targetGo);
            Health target = targetGo.AddComponent<Health>();
            target.ConfigureMaxHealth(100f);

            SkillDefinition bolt = CreateDamageSkill("skill_test_bolt", 20f);
            SkillInstance instance = skills.LearnSkill(bolt, casterLevel: 1, force: true);

            DamageAppliedEvent? hit = null;
            EventBus.Subscribe<DamageAppliedEvent>(e => hit = e);

            Assert.IsTrue(skills.TryCast(instance, Vector3.zero, targetGo));

            float expected = 20f * OutgoingDamage.GetCriticalMultiplier(_buffs, PlayerCombatBalance.LoadDefault());
            Assert.IsTrue(hit.HasValue);
            Assert.IsTrue(hit.Value.IsCritical);
            Assert.AreEqual(Mathf.Round(expected), hit.Value.FinalDamage, 0.001f);
            Assert.AreEqual(100f - Mathf.Round(expected), target.CurrentHealth, 0.001f);
        }

        private SkillDefinition CreateDamageSkill(string id, float damage)
        {
            var skill = ScriptableObject.CreateInstance<SkillDefinition>();
            _created.Add(skill);
            var so = new SerializedObject(skill);
            so.FindProperty("_skillId").stringValue = id;
            so.FindProperty("_displayName").stringValue = id;
            so.FindProperty("_skillType").enumValueIndex = (int)SkillType.Active;
            so.FindProperty("_cooldownSeconds").floatValue = 0f;
            so.FindProperty("_manaCost").floatValue = 0f;
            so.FindProperty("_baseDamage").floatValue = damage;
            so.ApplyModifiedPropertiesWithoutUndo();
            return skill;
        }
    }
}
