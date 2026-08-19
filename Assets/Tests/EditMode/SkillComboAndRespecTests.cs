using System.Threading;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Skills;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the Fase 4 combo system (<see cref="ComboDefinition"/>/<see cref="ComboTracker"/>)
    /// and the skill point respec API added to <see cref="SkillManager"/>
    /// (<see cref="SkillManager.RespecSkill"/>/<see cref="SkillManager.ResetSkillPoints"/>).
    /// SkillDefinition/ComboDefinition are ScriptableObjects whose fields are private+[SerializeField],
    /// so tests populate them the same way the project's own editor seeders do: via SerializedObject.
    /// </summary>
    public class SkillComboAndRespecTests
    {
        private GameObject _go;
        private BuffController _buffController;
        private SkillManager _skillManager;
        private ComboTracker _comboTracker;

        private SkillDefinition _skillA;
        private SkillDefinition _skillB;
        private SkillDefinition _skillC;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();

            _go = new GameObject("SkillComboTestCaster");
            _buffController = _go.AddComponent<BuffController>();
            _skillManager = _go.AddComponent<SkillManager>();
            _comboTracker = _go.AddComponent<ComboTracker>();

            _skillA = CreateSkill("skill_a", SkillType.Active, damage: 10f);
            _skillB = CreateSkill("skill_b", SkillType.Active, damage: 5f);
            _skillC = CreateSkill("skill_c", SkillType.Active, damage: 1f);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            Object.DestroyImmediate(_go);
            Object.DestroyImmediate(_skillA);
            Object.DestroyImmediate(_skillB);
            Object.DestroyImmediate(_skillC);
        }

        // ---------------------------------------------------------------
        // Combos
        // ---------------------------------------------------------------

        [Test]
        public void Combo_CastInOrderWithinWindow_GrantsDamageBuff()
        {
            ComboDefinition combo = CreateCombo("combo_ab", new[] { _skillA, _skillB }, windowSeconds: 5f,
                bonusType: ComboBonusType.DamageMultiplier, bonusValue: 0.5f, bonusDuration: 5f);
            SetCombos(_comboTracker, combo);

            _comboTracker.HandleSkillCast(new SkillCastEvent(_go, _skillA, Vector3.zero));
            _comboTracker.HandleSkillCast(new SkillCastEvent(_go, _skillB, Vector3.zero));

            float modified = _buffController.GetModifiedValue(StatType.AttackDamage, 10f);
            Assert.AreEqual(15f, modified, 0.001f, "Completing the combo should grant a +50% AttackDamage buff.");

            Object.DestroyImmediate(combo);
        }

        [Test]
        public void Combo_PublishesComboCompletedEvent()
        {
            ComboDefinition combo = CreateCombo("combo_ab", new[] { _skillA, _skillB }, windowSeconds: 5f,
                bonusType: ComboBonusType.DamageMultiplier, bonusValue: 0.5f, bonusDuration: 5f);
            SetCombos(_comboTracker, combo);

            ComboDefinition completed = null;
            EventBus.Subscribe<ComboCompletedEvent>(evt => completed = evt.Combo);

            _comboTracker.HandleSkillCast(new SkillCastEvent(_go, _skillA, Vector3.zero));
            _comboTracker.HandleSkillCast(new SkillCastEvent(_go, _skillB, Vector3.zero));

            Assert.AreEqual(combo, completed);

            Object.DestroyImmediate(combo);
        }

        [Test]
        public void Combo_WrongOrder_DoesNotComplete()
        {
            ComboDefinition combo = CreateCombo("combo_ab", new[] { _skillA, _skillB }, windowSeconds: 5f,
                bonusType: ComboBonusType.DamageMultiplier, bonusValue: 0.5f, bonusDuration: 5f);
            SetCombos(_comboTracker, combo);

            // B then A is the wrong order for an A->B combo.
            _comboTracker.HandleSkillCast(new SkillCastEvent(_go, _skillB, Vector3.zero));
            _comboTracker.HandleSkillCast(new SkillCastEvent(_go, _skillA, Vector3.zero));

            float modified = _buffController.GetModifiedValue(StatType.AttackDamage, 10f);
            Assert.AreEqual(10f, modified, 0.001f);

            Object.DestroyImmediate(combo);
        }

        [Test]
        public void Combo_OutsideTimeWindow_DoesNotComplete()
        {
            ComboDefinition combo = CreateCombo("combo_ab_tight", new[] { _skillA, _skillB }, windowSeconds: 0f,
                bonusType: ComboBonusType.DamageMultiplier, bonusValue: 0.5f, bonusDuration: 5f);
            SetCombos(_comboTracker, combo);

            _comboTracker.HandleSkillCast(new SkillCastEvent(_go, _skillA, Vector3.zero));
            // Real time must move forward for Time.time to advance past a zero-second window.
            Thread.Sleep(50);
            _comboTracker.HandleSkillCast(new SkillCastEvent(_go, _skillB, Vector3.zero));

            float modified = _buffController.GetModifiedValue(StatType.AttackDamage, 10f);
            Assert.AreEqual(10f, modified, 0.001f, "Casting outside the combo window should not grant the bonus.");

            Object.DestroyImmediate(combo);
        }

        [Test]
        public void Combo_UnlockSkillBonus_LearnsSkillForFree()
        {
            ComboDefinition combo = CreateCombo("combo_unlock", new[] { _skillA, _skillB }, windowSeconds: 5f,
                bonusType: ComboBonusType.UnlockSkill, bonusValue: 0f, bonusDuration: 0f, unlockSkill: _skillC);
            SetCombos(_comboTracker, combo);

            Assert.IsFalse(_skillManager.IsSkillLearned(_skillC.SkillId));
            int pointsBefore = _skillManager.AvailableSkillPoints;

            _comboTracker.HandleSkillCast(new SkillCastEvent(_go, _skillA, Vector3.zero));
            _comboTracker.HandleSkillCast(new SkillCastEvent(_go, _skillB, Vector3.zero));

            Assert.IsTrue(_skillManager.IsSkillLearned(_skillC.SkillId));
            Assert.AreEqual(pointsBefore, _skillManager.AvailableSkillPoints, "Combo-granted skills must not cost a skill point.");

            Object.DestroyImmediate(combo);
        }

        // ---------------------------------------------------------------
        // Respec
        // ---------------------------------------------------------------

        [Test]
        public void RespecSkill_RefundsSpentPoints_AndUnlearnsSkill()
        {
            _skillManager.GrantSkillPoints(2);
            Assert.IsTrue(_skillManager.LearnSkillWithPoint(_skillA, casterLevel: 1));
            Assert.IsTrue(_skillManager.LevelUpSkillWithPoint(_skillA.SkillId));
            Assert.AreEqual(0, _skillManager.AvailableSkillPoints);
            Assert.AreEqual(2, _skillManager.GetLearnedSkill(_skillA.SkillId).Level);

            bool result = _skillManager.RespecSkill(_skillA.SkillId);

            Assert.IsTrue(result);
            Assert.IsFalse(_skillManager.IsSkillLearned(_skillA.SkillId));
            Assert.AreEqual(2, _skillManager.AvailableSkillPoints, "Both the initial learn point and the level-up point should be refunded.");
        }

        [Test]
        public void RespecSkill_UnknownSkill_ReturnsFalse()
        {
            Assert.IsFalse(_skillManager.RespecSkill("not_learned"));
        }

        [Test]
        public void RespecSkill_PassiveSkill_RemovesItsBuffEffect()
        {
            SkillDefinition passive = CreateSkillWithBuff("skill_passive", StatType.MoveSpeed, ModifierApplication.Flat, 3f);
            _skillManager.GrantSkillPoints(1);

            _skillManager.LearnSkillWithPoint(passive, casterLevel: 1);
            Assert.AreEqual(8f, _buffController.GetModifiedValue(StatType.MoveSpeed, 5f), 0.001f, "Learning a passive should apply its buff immediately.");

            bool result = _skillManager.RespecSkill(passive.SkillId);

            Assert.IsTrue(result);
            Assert.AreEqual(5f, _buffController.GetModifiedValue(StatType.MoveSpeed, 5f), 0.001f, "Respeccing a passive should remove the buff it granted.");

            Object.DestroyImmediate(passive);
        }

        [Test]
        public void RespecSkill_BlockedWhileADependentIsStillLearned()
        {
            SkillDefinition dependent = CreateSkillRequiring("skill_dependent", _skillA);
            _skillManager.GrantSkillPoints(2);
            _skillManager.LearnSkillWithPoint(_skillA, casterLevel: 1);
            _skillManager.LearnSkillWithPoint(dependent, casterLevel: 1);

            Assert.IsFalse(_skillManager.RespecSkill(_skillA.SkillId), "Should refuse to respec a skill another learned skill still requires.");

            Assert.IsTrue(_skillManager.RespecSkill(dependent.SkillId));
            Assert.IsTrue(_skillManager.RespecSkill(_skillA.SkillId), "Once the dependent is gone, respeccing the prerequisite should succeed.");

            Object.DestroyImmediate(dependent);
        }

        [Test]
        public void ResetSkillPoints_RefundsEverythingRegardlessOfDependencyOrder()
        {
            SkillDefinition dependent = CreateSkillRequiring("skill_dependent2", _skillA);
            _skillManager.GrantSkillPoints(2);
            _skillManager.LearnSkillWithPoint(_skillA, casterLevel: 1);
            _skillManager.LearnSkillWithPoint(dependent, casterLevel: 1);
            Assert.AreEqual(0, _skillManager.AvailableSkillPoints);

            int refunded = _skillManager.ResetSkillPoints();

            Assert.AreEqual(2, refunded);
            Assert.AreEqual(2, _skillManager.AvailableSkillPoints);
            Assert.IsFalse(_skillManager.IsSkillLearned(_skillA.SkillId));
            Assert.IsFalse(_skillManager.IsSkillLearned(dependent.SkillId));

            Object.DestroyImmediate(dependent);
        }

        [Test]
        public void ResetSkillPoints_WithNothingLearned_ReturnsZero()
        {
            Assert.AreEqual(0, _skillManager.ResetSkillPoints());
        }

        // ---------------------------------------------------------------
        // Test helpers
        // ---------------------------------------------------------------

        private static SkillDefinition CreateSkill(string id, SkillType type, float damage = 0f)
        {
            var skill = ScriptableObject.CreateInstance<SkillDefinition>();
            var so = new SerializedObject(skill);
            so.FindProperty("_skillId").stringValue = id;
            so.FindProperty("_displayName").stringValue = id;
            so.FindProperty("_skillType").enumValueIndex = (int)type;
            so.FindProperty("_cooldownSeconds").floatValue = 0f;
            so.FindProperty("_manaCost").floatValue = 0f;
            so.FindProperty("_baseDamage").floatValue = damage;
            so.FindProperty("_maxLevel").intValue = 10;
            so.ApplyModifiedPropertiesWithoutUndo();
            return skill;
        }

        private static SkillDefinition CreateSkillWithBuff(string id, StatType stat, ModifierApplication application, float value)
        {
            SkillDefinition skill = CreateSkill(id, SkillType.Passive);
            var so = new SerializedObject(skill);
            SerializedProperty buffs = so.FindProperty("_buffs");
            buffs.arraySize = 1;
            SerializedProperty buff = buffs.GetArrayElementAtIndex(0);
            buff.FindPropertyRelative("displayName").stringValue = "TestBuff";
            buff.FindPropertyRelative("affectedStat").enumValueIndex = (int)stat;
            buff.FindPropertyRelative("application").enumValueIndex = (int)application;
            buff.FindPropertyRelative("value").floatValue = value;
            buff.FindPropertyRelative("durationSeconds").floatValue = 0f; // permanent, undone only by explicit respec removal
            so.ApplyModifiedPropertiesWithoutUndo();
            return skill;
        }

        private static SkillDefinition CreateSkillRequiring(string id, SkillDefinition required)
        {
            SkillDefinition skill = CreateSkill(id, SkillType.Active);
            var so = new SerializedObject(skill);
            SerializedProperty requirement = so.FindProperty("_requirement");
            SerializedProperty requiredSkills = requirement.FindPropertyRelative("requiredSkills");
            requiredSkills.arraySize = 1;
            requiredSkills.GetArrayElementAtIndex(0).objectReferenceValue = required;
            requirement.FindPropertyRelative("requiredSkillLevel").intValue = 1;
            so.ApplyModifiedPropertiesWithoutUndo();
            return skill;
        }

        private static ComboDefinition CreateCombo(string id, SkillDefinition[] sequence, float windowSeconds,
            ComboBonusType bonusType, float bonusValue, float bonusDuration, SkillDefinition unlockSkill = null)
        {
            var combo = ScriptableObject.CreateInstance<ComboDefinition>();
            var so = new SerializedObject(combo);
            so.FindProperty("_comboId").stringValue = id;
            so.FindProperty("_displayName").stringValue = id;
            so.FindProperty("_windowSeconds").floatValue = windowSeconds;
            so.FindProperty("_bonusType").enumValueIndex = (int)bonusType;
            so.FindProperty("_bonusValue").floatValue = bonusValue;
            so.FindProperty("_bonusDurationSeconds").floatValue = bonusDuration;
            so.FindProperty("_unlockSkill").objectReferenceValue = unlockSkill;

            SerializedProperty sequenceProp = so.FindProperty("_sequence");
            sequenceProp.arraySize = sequence.Length;
            for (int i = 0; i < sequence.Length; i++)
            {
                sequenceProp.GetArrayElementAtIndex(i).objectReferenceValue = sequence[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            return combo;
        }

        private static void SetCombos(ComboTracker tracker, params ComboDefinition[] combos)
        {
            var so = new SerializedObject(tracker);
            SerializedProperty combosProp = so.FindProperty("_combos");
            combosProp.arraySize = combos.Length;
            for (int i = 0; i < combos.Length; i++)
            {
                combosProp.GetArrayElementAtIndex(i).objectReferenceValue = combos[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
