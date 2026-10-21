using System.IO;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Save;
using Rapadura.UI.HUD;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the HUD skill hotbar: the plain-C# <see cref="HotbarLoadout"/> (slot
    /// assignment, default fill, save round-trip), <see cref="SkillHotbar"/>'s default fill from
    /// <see cref="SkillManager"/> + casting, and the pure <see cref="SkillSlotDisplayMath"/> behind
    /// the cooldown overlay. Like <see cref="HudBarMathTests"/>, nothing here touches a real
    /// UIDocument — the HUD binding itself needs PanelSettings that only exist in the Editor scene.
    /// </summary>
    public class SkillHotbarTests
    {
        private const int TestSlot = 90011;

        private GameObject _go;
        private SkillManager _skillManager;
        private SkillHotbar _hotbar;

        private SkillDefinition _fireball;
        private SkillDefinition _dash;
        private SkillDefinition _toughness;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            DeleteSlotFiles(TestSlot);

            _go = new GameObject("SkillHotbarTestCaster");
            _skillManager = _go.AddComponent<SkillManager>();
            _hotbar = _go.AddComponent<SkillHotbar>();

            _fireball = CreateSkill("skill_fireball", SkillType.Active, cooldown: 4f);
            _dash = CreateSkill("skill_dash", SkillType.Active, cooldown: 2f);
            _toughness = CreateSkill("skill_toughness", SkillType.Passive);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            DeleteSlotFiles(TestSlot);
            Object.DestroyImmediate(_go);
            Object.DestroyImmediate(_fireball);
            Object.DestroyImmediate(_dash);
            Object.DestroyImmediate(_toughness);
        }

        // ---------------------------------------------------------------
        // HotbarLoadout — assignment
        // ---------------------------------------------------------------

        [Test]
        public void NewLoadout_HasFiveEmptySlots()
        {
            var loadout = new HotbarLoadout();

            Assert.AreEqual(5, loadout.SlotCount);
            for (int i = 0; i < loadout.SlotCount; i++)
            {
                Assert.IsTrue(loadout.IsEmpty(i));
            }
        }

        [Test]
        public void Assign_StoresSkillIdInSlot()
        {
            var loadout = new HotbarLoadout();

            Assert.IsTrue(loadout.Assign(2, "a"));

            Assert.AreEqual("a", loadout.GetSkillId(2));
            Assert.AreEqual(2, loadout.IndexOf("a"));
        }

        [Test]
        public void Assign_OutOfRange_ReturnsFalse()
        {
            var loadout = new HotbarLoadout();

            Assert.IsFalse(loadout.Assign(-1, "a"));
            Assert.IsFalse(loadout.Assign(5, "a"));
            Assert.IsNull(loadout.GetSkillId(5));
        }

        [Test]
        public void Assign_SkillAlreadyInAnotherSlot_SwapsSlots()
        {
            var loadout = new HotbarLoadout();
            loadout.Assign(0, "a");
            loadout.Assign(1, "b");

            loadout.Assign(1, "a");

            Assert.AreEqual("a", loadout.GetSkillId(1));
            Assert.AreEqual("b", loadout.GetSkillId(0), "The displaced skill should move to the slot the assigned skill came from.");
        }

        [Test]
        public void Assign_NullId_ClearsSlot()
        {
            var loadout = new HotbarLoadout();
            loadout.Assign(0, "a");

            loadout.Assign(0, null);

            Assert.IsTrue(loadout.IsEmpty(0));
        }

        [Test]
        public void Assign_RaisesOnChanged()
        {
            var loadout = new HotbarLoadout();
            int changes = 0;
            loadout.OnChanged += () => changes++;

            loadout.Assign(0, "a");

            Assert.AreEqual(1, changes);
        }

        [Test]
        public void FillEmptySlots_FillsInOrder_SkipsDuplicates_KeepsAssignedSlots()
        {
            var loadout = new HotbarLoadout();
            loadout.Assign(1, "manual");

            int filled = loadout.FillEmptySlots(new[] { "a", "manual", "b", "a", null, "c" });

            Assert.AreEqual(3, filled);
            Assert.AreEqual("a", loadout.GetSkillId(0));
            Assert.AreEqual("manual", loadout.GetSkillId(1), "Default fill must never overwrite an assigned slot.");
            Assert.AreEqual("b", loadout.GetSkillId(2));
            Assert.AreEqual("c", loadout.GetSkillId(3));
            Assert.IsTrue(loadout.IsEmpty(4));
        }

        [Test]
        public void FillEmptySlots_MoreCandidatesThanSlots_StopsWhenFull()
        {
            var loadout = new HotbarLoadout(2);

            int filled = loadout.FillEmptySlots(new[] { "a", "b", "c" });

            Assert.AreEqual(2, filled);
            Assert.AreEqual(-1, loadout.IndexOf("c"));
        }

        [Test]
        public void RemoveWhere_ClearsMatchingSlotsOnly()
        {
            var loadout = new HotbarLoadout();
            loadout.FillEmptySlots(new[] { "a", "b", "c" });

            int removed = loadout.RemoveWhere(id => id == "b");

            Assert.AreEqual(1, removed);
            Assert.AreEqual("a", loadout.GetSkillId(0));
            Assert.IsTrue(loadout.IsEmpty(1));
            Assert.AreEqual("c", loadout.GetSkillId(2));
        }

        // ---------------------------------------------------------------
        // HotbarLoadout — save round-trip
        // ---------------------------------------------------------------

        [Test]
        public void CaptureRestore_ThroughJsonUtility_RoundTripsIncludingEmptySlots()
        {
            var original = new HotbarLoadout();
            original.Assign(0, "a");
            original.Assign(3, "b");

            // Same serialization SaveManager uses for each ISaveable slice.
            string json = JsonUtility.ToJson(original.CaptureState());
            var state = JsonUtility.FromJson<HotbarLoadoutSaveState>(json);

            var restored = new HotbarLoadout();
            restored.RestoreState(state);

            Assert.AreEqual("a", restored.GetSkillId(0));
            Assert.IsTrue(restored.IsEmpty(1));
            Assert.IsTrue(restored.IsEmpty(2));
            Assert.AreEqual("b", restored.GetSkillId(3));
            Assert.IsTrue(restored.IsEmpty(4));
        }

        [Test]
        public void RestoreState_IgnoresExtraSlotsAndDuplicates()
        {
            var loadout = new HotbarLoadout(2);

            loadout.RestoreState(new HotbarLoadoutSaveState { slotSkillIds = new[] { "a", "a", "c" } });

            Assert.AreEqual("a", loadout.GetSkillId(0));
            Assert.IsTrue(loadout.IsEmpty(1), "A duplicated id should only keep its first slot.");
            Assert.AreEqual(-1, loadout.IndexOf("c"));
        }

        [Test]
        public void RestoreState_WrongType_LeavesLoadoutUntouched()
        {
            var loadout = new HotbarLoadout();
            loadout.Assign(0, "a");

            loadout.RestoreState("not a hotbar state");

            Assert.AreEqual("a", loadout.GetSkillId(0));
        }

        [Test]
        public void SaveManager_SaveThenLoad_RestoresLoadout()
        {
            var manager = new SaveManager();
            manager.Initialize();

            var loadout = new HotbarLoadout();
            manager.Register(loadout);
            loadout.Assign(0, "a");
            loadout.Assign(4, "e");

            manager.Save(TestSlot);
            loadout.ClearAll();

            Assert.IsTrue(manager.Load(TestSlot));
            Assert.AreEqual("a", loadout.GetSkillId(0));
            Assert.AreEqual("e", loadout.GetSkillId(4));
            Assert.IsTrue(loadout.IsEmpty(1));
        }

        // ---------------------------------------------------------------
        // SkillHotbar — default fill from SkillManager + casting
        // ---------------------------------------------------------------

        [Test]
        public void CollectDefaultCandidates_EquippedFirst_ThenLearned_ExcludesPassives()
        {
            _skillManager.LearnSkill(_fireball, 1, force: true);
            _skillManager.LearnSkill(_toughness, 1, force: true);
            _skillManager.LearnSkill(_dash, 1, force: true);
            _skillManager.EquipSkill(0, _dash.SkillId);

            string[] candidates = SkillHotbar.CollectDefaultCandidates(_skillManager).Distinct().ToArray();

            CollectionAssert.AreEqual(new[] { "skill_dash", "skill_fireball" }, candidates);
        }

        [Test]
        public void RefreshFromSkillManager_FillsFromLearnedSkills_AndPrunesUnlearned()
        {
            _skillManager.LearnSkill(_fireball, 1, force: true);
            _skillManager.LearnSkill(_dash, 1, force: true);

            _hotbar.RefreshFromSkillManager();

            Assert.AreEqual("skill_fireball", _hotbar.Loadout.GetSkillId(0));
            Assert.AreEqual("skill_dash", _hotbar.Loadout.GetSkillId(1));

            _skillManager.RespecSkill(_fireball.SkillId);
            _hotbar.RefreshFromSkillManager();

            Assert.AreEqual(-1, _hotbar.Loadout.IndexOf("skill_fireball"), "Unlearned skills must be pruned from the bar.");
            Assert.IsTrue(_hotbar.Loadout.IsEmpty(0));
            Assert.AreEqual("skill_dash", _hotbar.Loadout.GetSkillId(1), "Skills still learned keep their slot — pruning never reshuffles the bar.");
        }

        [Test]
        public void AssignSlot_UnlearnedSkill_IsRejected()
        {
            Assert.IsFalse(_hotbar.AssignSlot(0, _fireball.SkillId));

            _skillManager.LearnSkill(_fireball, 1, force: true);

            Assert.IsTrue(_hotbar.AssignSlot(0, _fireball.SkillId));
        }

        [Test]
        public void TryCastSlot_CastsAndStartsCooldown_SecondCastBlocked()
        {
            SkillInstance instance = _skillManager.LearnSkill(_fireball, 1, force: true);
            _hotbar.AssignSlot(0, _fireball.SkillId);

            Assert.IsTrue(_hotbar.TryCastSlot(0));
            Assert.IsTrue(instance.IsOnCooldown);
            Assert.AreEqual(4f, instance.CooldownDuration, 0.0001f);
            Assert.IsFalse(_hotbar.TryCastSlot(0), "Slot should be blocked while on cooldown.");
        }

        [Test]
        public void TryCastSlot_EmptySlot_ReturnsFalse()
        {
            Assert.IsFalse(_hotbar.TryCastSlot(0));
            Assert.IsFalse(_hotbar.TryCastSlot(99));
        }

        [Test]
        public void CanAffordSkill_ManaCostAboveCurrentMana_IsFalse_AndCastFails()
        {
            Object.DestroyImmediate(_go);
            _go = new GameObject("SkillHotbarManaTestCaster");
            _go.AddComponent<PlayerStats>();
            _skillManager = _go.AddComponent<SkillManager>();
            _hotbar = _go.AddComponent<SkillHotbar>();

            SkillDefinition expensive = CreateSkill("skill_expensive", SkillType.Active, cooldown: 1f, manaCost: 100000f);
            _skillManager.LearnSkill(expensive, 1, force: true);
            _hotbar.AssignSlot(0, expensive.SkillId);

            Assert.IsFalse(_skillManager.CanAffordSkill(expensive));
            Assert.IsFalse(_hotbar.TryCastSlot(0));

            Object.DestroyImmediate(expensive);
        }

        // ---------------------------------------------------------------
        // SkillSlotDisplayMath — cooldown overlay
        // ---------------------------------------------------------------

        [Test]
        public void CooldownFraction_JustCast_IsOne()
        {
            Assert.AreEqual(1f, SkillSlotDisplayMath.ComputeCooldownFraction(4f, 4f), 0.0001f);
        }

        [Test]
        public void CooldownFraction_Halfway_IsHalf()
        {
            Assert.AreEqual(0.5f, SkillSlotDisplayMath.ComputeCooldownFraction(2f, 4f), 0.0001f);
            Assert.AreEqual(50f, SkillSlotDisplayMath.ComputeCooldownPercent(2f, 4f), 0.0001f);
        }

        [Test]
        public void CooldownFraction_Ready_IsZero()
        {
            Assert.AreEqual(0f, SkillSlotDisplayMath.ComputeCooldownFraction(0f, 4f), 0.0001f);
        }

        [Test]
        public void CooldownFraction_ZeroDuration_IsZero()
        {
            Assert.AreEqual(0f, SkillSlotDisplayMath.ComputeCooldownFraction(1f, 0f), 0.0001f);
        }

        [Test]
        public void CooldownFraction_RemainingAboveDuration_ClampsToOne()
        {
            Assert.AreEqual(1f, SkillSlotDisplayMath.ComputeCooldownFraction(6f, 4f), 0.0001f);
        }

        [Test]
        public void CooldownFraction_WithCooldownReduction_UsesActualDuration()
        {
            SkillInstance instance = _skillManager.LearnSkill(_fireball, 1, force: true);
            instance.StartCooldown(0.5f); // 4s * (1 - 0.5) = 2s

            Assert.AreEqual(2f, instance.CooldownDuration, 0.0001f);
            Assert.AreEqual(1f, SkillSlotDisplayMath.ComputeCooldownFraction(instance.CooldownRemaining, instance.CooldownDuration), 0.0001f);

            instance.TickCooldown(1f);

            Assert.AreEqual(0.5f, SkillSlotDisplayMath.ComputeCooldownFraction(instance.CooldownRemaining, instance.CooldownDuration), 0.0001f);
        }

        [Test]
        public void FormatCooldownLabel_RoundsUpWholeSeconds_AndShowsTenthsBelowOne()
        {
            Assert.AreEqual(string.Empty, SkillSlotDisplayMath.FormatCooldownLabel(0f));
            Assert.AreEqual("3", SkillSlotDisplayMath.FormatCooldownLabel(2.1f));
            Assert.AreEqual("1", SkillSlotDisplayMath.FormatCooldownLabel(1f));
            Assert.AreEqual("1", SkillSlotDisplayMath.FormatCooldownLabel(0.95f));
            Assert.AreEqual("0.4", SkillSlotDisplayMath.FormatCooldownLabel(0.35f));
        }

        [Test]
        public void CooldownLabelKey_ChangesExactlyWhenLabelChanges()
        {
            Assert.AreEqual(SkillSlotDisplayMath.CooldownLabelKey(2.9f), SkillSlotDisplayMath.CooldownLabelKey(2.1f));
            Assert.AreNotEqual(SkillSlotDisplayMath.CooldownLabelKey(2.1f), SkillSlotDisplayMath.CooldownLabelKey(1.9f));
            Assert.AreEqual(SkillSlotDisplayMath.CooldownLabelKey(1f), SkillSlotDisplayMath.CooldownLabelKey(0.95f));
            Assert.AreEqual(0, SkillSlotDisplayMath.CooldownLabelKey(0f));
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private static SkillDefinition CreateSkill(string id, SkillType type, float cooldown = 0f, float manaCost = 0f)
        {
            var skill = ScriptableObject.CreateInstance<SkillDefinition>();
            var so = new SerializedObject(skill);
            so.FindProperty("_skillId").stringValue = id;
            so.FindProperty("_displayName").stringValue = id;
            so.FindProperty("_skillType").enumValueIndex = (int)type;
            so.FindProperty("_cooldownSeconds").floatValue = cooldown;
            so.FindProperty("_manaCost").floatValue = manaCost;
            so.FindProperty("_baseDamage").floatValue = 0f;
            so.FindProperty("_maxLevel").intValue = 10;
            so.ApplyModifiedPropertiesWithoutUndo();
            return skill;
        }

        private static void DeleteSlotFiles(int slot)
        {
            string folder = Path.Combine(Application.persistentDataPath, "Saves");
            string main = Path.Combine(folder, $"slot_{slot}.json");

            foreach (string path in new[] { main, main + ".bak", main + ".tmp" })
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
    }
}
