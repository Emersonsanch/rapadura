using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Building;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Skills;
using Rapadura.Save;
using Rapadura.UI.CharacterSelect;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the world-state slices added in save schema 2 — selected hero
    /// (<see cref="CharacterSelectionState"/>), placed structures (<see cref="BuildingManager"/>),
    /// recipe unlocks (<see cref="CraftingManager"/>) and defeated bosses
    /// (<see cref="DefeatedEnemyRegistry"/>) — plus restore ordering, the SkillManager fallback for
    /// character skills, and compatibility with older saves that lack all of the new keys.
    /// </summary>
    public class WorldStateSaveTests
    {
        private const int TestSlot = 90021;
        private const int LegacySlot = 90022;

        private readonly List<Object> _toDestroy = new List<Object>();

        [Serializable]
        private class DummyState
        {
            public int value;
        }

        /// <summary>Default-order saveable that records when it was restored relative to the hero.</summary>
        private class OrderProbe : ISaveable
        {
            private readonly List<string> _log;

            public OrderProbe(List<string> log)
            {
                _log = log;
            }

            public string SaveKey => "probe";
            public int Value;

            public object CaptureState() => new DummyState { value = Value };

            public void RestoreState(object state)
            {
                Value = ((DummyState)state).value;
                _log.Add("probe");
            }
        }

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            DeleteSlotFiles(TestSlot);
            DeleteSlotFiles(LegacySlot);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            DeleteSlotFiles(TestSlot);
            DeleteSlotFiles(LegacySlot);
            CharacterSkillSet.Invalidate();

            foreach (Object obj in _toDestroy)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _toDestroy.Clear();
        }

        // ---------------------------------------------------------------
        // Selected character
        // ---------------------------------------------------------------

        [Test]
        public void SelectedCharacter_SaveThenLoad_RestoresHeroAndPublishesEvent()
        {
            SaveManager manager = CreateManager();
            var selection = new CharacterSelectionState();
            manager.Register(selection);
            selection.Select(CharacterId.Icaro);
            manager.Save(TestSlot);

            selection.Clear();
            CharacterId? published = null;
            EventBus.Subscribe<CharacterSelectionRestoredEvent>(e => published = e.CharacterId);

            Assert.IsTrue(manager.Load(TestSlot));
            Assert.AreEqual(CharacterId.Icaro, selection.Selected);
            Assert.AreEqual(CharacterId.Icaro, published);
        }

        [Test]
        public void SelectedCharacter_SavedBeforePicking_RestoresNoSelectionAndNoEvent()
        {
            SaveManager manager = CreateManager();
            var selection = new CharacterSelectionState();
            manager.Register(selection);
            manager.Save(TestSlot);

            selection.Select(CharacterId.Maria);
            bool published = false;
            EventBus.Subscribe<CharacterSelectionRestoredEvent>(_ => published = true);

            Assert.IsTrue(manager.Load(TestSlot));
            Assert.IsFalse(selection.HasSelection);
            Assert.IsFalse(published);
        }

        [Test]
        public void SelectedCharacter_UnknownSavedId_KeepsCurrentSelection()
        {
            var selection = new CharacterSelectionState();
            selection.Select(CharacterId.Lavine);

            selection.RestoreState(new CharacterSelectionSaveState { hasSelection = true, characterId = "NotAHero" });
            Assert.AreEqual(CharacterId.Lavine, selection.Selected);

            selection.RestoreState(new CharacterSelectionSaveState { hasSelection = true, characterId = "42" });
            Assert.AreEqual(CharacterId.Lavine, selection.Selected);
        }

        [Test]
        public void SelectedCharacter_RestoresBeforeOtherSaveables_EvenIfRegisteredLater()
        {
            SaveManager manager = CreateManager();
            var log = new List<string>();
            var probe = new OrderProbe(log) { Value = 3 };
            var selection = new CharacterSelectionState();

            manager.Register(probe);      // written first in the file...
            manager.Register(selection);  // ...hero written after it.
            selection.Select(CharacterId.Joaquim);
            manager.Save(TestSlot);

            EventBus.Subscribe<CharacterSelectionRestoredEvent>(_ => log.Add("hero"));
            Assert.IsTrue(manager.Load(TestSlot));

            CollectionAssert.AreEqual(new[] { "hero", "probe" }, log);
        }

        [Test]
        public void TryReadSavedHero_ReadsHeroFromPeekedSlot_WithoutApplyingIt()
        {
            SaveManager manager = CreateManager();
            var selection = new CharacterSelectionState();
            manager.Register(selection);
            selection.Select(CharacterId.Maithe);
            manager.Save(TestSlot);
            selection.Clear();

            SaveData peeked = manager.PeekSlot(TestSlot);

            Assert.IsTrue(CharacterSelectController.TryReadSavedHero(peeked, out CharacterId hero, out DateTime savedAt));
            Assert.AreEqual(CharacterId.Maithe, hero);
            Assert.Greater(savedAt, DateTime.MinValue);
            Assert.IsFalse(selection.HasSelection, "Peeking must not restore anything.");
        }

        // ---------------------------------------------------------------
        // SkillManager — character skills resolve even before a hero is applied
        // ---------------------------------------------------------------

        [Test]
        public void SkillManagerRestore_CharacterSkillNotRegistered_IsResolvedFromCharacterSkillSet()
        {
            SkillDefinition turret = CreateSkill("skill_test_icaro_turret", "Torreta Automática");
            CharacterSkillSet.SetForTests(CharacterId.Icaro, new[] { turret });

            var go = new GameObject("WorldStateSaveTestsCaster");
            _toDestroy.Add(go);
            SkillManager skillManager = go.AddComponent<SkillManager>();
            Assert.IsFalse(skillManager.IsSkillRegistered(turret.SkillId));

            skillManager.RestoreState(new SkillManagerSaveState
            {
                skills = new[] { new LearnedSkillSaveEntry { skillId = turret.SkillId, level = 2, equippedSlot = 0 } },
                availableSkillPoints = 1
            });

            SkillInstance restored = skillManager.LearnedSkills.SingleOrDefault();
            Assert.IsNotNull(restored);
            Assert.AreSame(turret, restored.Definition);
            Assert.AreEqual(2, restored.Level);
            Assert.AreSame(restored, skillManager.EquippedSlots[0]);
            Assert.IsTrue(skillManager.IsSkillRegistered(turret.SkillId));
        }

        // ---------------------------------------------------------------
        // Building / crafting / defeated bosses
        // ---------------------------------------------------------------

        [Test]
        public void BuildingManager_SaveThenLoad_RestoresPlacedStructures()
        {
            SaveManager manager = CreateManager();
            var building = new BuildingManager();
            manager.Register(building);
            building.RestoreState(new BuildingManager.BuildingManagerSaveState
            {
                structures = new[]
                {
                    new PlacedStructure { instanceId = "a1", structureId = "hut", gridX = 3, gridY = -2, rotationSteps = 1, currentLevel = 2 }
                }
            });
            manager.Save(TestSlot);

            var fresh = new BuildingManager();
            var freshManager = CreateManager();
            freshManager.Register(fresh);
            Assert.IsTrue(freshManager.Load(TestSlot));

            Assert.AreEqual(1, fresh.PlacedStructures.Count);
            PlacedStructure structure = fresh.PlacedStructures[0];
            Assert.AreEqual("a1", structure.instanceId);
            Assert.AreEqual("hut", structure.structureId);
            Assert.AreEqual(3, structure.gridX);
            Assert.AreEqual(-2, structure.gridY);
            Assert.AreEqual(1, structure.rotationSteps);
            Assert.AreEqual(2, structure.currentLevel);
        }

        [Test]
        public void CraftingManager_SaveThenLoad_RestoresUnlockedRecipes()
        {
            SaveManager manager = CreateManager();
            var crafting = new CraftingManager();
            manager.Register(crafting);
            crafting.RestoreState(new CraftingManagerSaveState { unlockedRecipeIds = new[] { "recipe_test_unlocked" } });
            manager.Save(TestSlot);

            var fresh = new CraftingManager();
            fresh.Initialize();
            Assert.IsFalse(fresh.IsRecipeKnown("recipe_test_unlocked"));

            SaveManager freshManager = CreateManager();
            freshManager.Register(fresh);
            Assert.IsTrue(freshManager.Load(TestSlot));

            Assert.IsTrue(fresh.IsRecipeKnown("recipe_test_unlocked"));
        }

        [Test]
        public void DefeatedEnemyRegistry_SaveThenLoad_RestoresDefeatedBosses()
        {
            SaveManager manager = CreateManager();
            var registry = new DefeatedEnemyRegistry();
            manager.Register(registry);
            Assert.IsTrue(registry.MarkDefeated("Boss_Test"));
            Assert.IsFalse(registry.MarkDefeated("Boss_Test"), "Second mark is a no-op.");
            manager.Save(TestSlot);

            var fresh = new DefeatedEnemyRegistry();
            SaveManager freshManager = CreateManager();
            freshManager.Register(fresh);
            Assert.IsTrue(freshManager.Load(TestSlot));

            Assert.IsTrue(fresh.IsDefeated("Boss_Test"));
            Assert.IsFalse(fresh.IsDefeated("Boss_Other"));
        }

        [Test]
        public void DefeatedEnemyRegistry_RecordsOnlyBossDeaths()
        {
            var registry = new DefeatedEnemyRegistry();
            registry.Initialize();

            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            definition.name = "Boss_FromEvent";
            _toDestroy.Add(definition);

            var enemy = new GameObject("WorldStateSaveTestsEnemy");
            _toDestroy.Add(enemy);
            EnemyController controller = enemy.AddComponent<EnemyController>();
            var so = new SerializedObject(controller);
            so.FindProperty("_definition").objectReferenceValue = definition;
            so.ApplyModifiedPropertiesWithoutUndo();

            EventBus.Publish(new EnemyDiedEvent(enemy, wasBoss: false));
            Assert.IsFalse(registry.IsDefeated(definition));

            EventBus.Publish(new EnemyDiedEvent(enemy, wasBoss: true));
            Assert.IsTrue(registry.IsDefeated(definition));

            registry.Shutdown();
        }

        // ---------------------------------------------------------------
        // Old saves (schema 0/1) — none of the new keys exist
        // ---------------------------------------------------------------

        [TestCase("1")]
        [TestCase("0")]
        [TestCase("")]
        public void LegacySave_WithoutNewKeys_LoadsAndLeavesNewSystemsUntouched(string legacyVersion)
        {
            WriteLegacySave(LegacySlot, legacyVersion, value: 11);

            SaveManager manager = CreateManager();
            var log = new List<string>();
            var probe = new OrderProbe(log);
            var selection = new CharacterSelectionState();
            var building = new BuildingManager();
            var crafting = new CraftingManager();
            var defeated = new DefeatedEnemyRegistry();

            selection.Select(CharacterId.Maria);
            building.RestoreState(new BuildingManager.BuildingManagerSaveState
            {
                structures = new[] { new PlacedStructure { instanceId = "keep", structureId = "wall" } }
            });
            crafting.RestoreState(new CraftingManagerSaveState { unlockedRecipeIds = new[] { "recipe_keep" } });
            defeated.MarkDefeated("Boss_Keep");

            manager.Register(probe);
            manager.Register(selection);
            manager.Register(building);
            manager.Register(crafting);
            manager.Register(defeated);

            bool published = false;
            EventBus.Subscribe<CharacterSelectionRestoredEvent>(_ => published = true);

            Assert.IsTrue(manager.Load(LegacySlot));

            Assert.AreEqual(11, probe.Value, "Existing entries still restore.");
            Assert.AreEqual(CharacterId.Maria, selection.Selected);
            Assert.IsFalse(published);
            Assert.AreEqual(1, building.PlacedStructures.Count);
            Assert.IsTrue(crafting.IsRecipeKnown("recipe_keep"));
            Assert.IsTrue(defeated.IsDefeated("Boss_Keep"));
        }

        [Test]
        public void LegacySave_PeekedForHero_ReportsNoSavedHero()
        {
            WriteLegacySave(LegacySlot, "1", value: 1);
            SaveManager manager = CreateManager();

            SaveData peeked = manager.PeekSlot(LegacySlot);

            Assert.IsNotNull(peeked);
            Assert.AreEqual(SaveManager.CurrentSaveVersion.ToString(), peeked.saveVersion);
            Assert.IsFalse(CharacterSelectController.TryReadSavedHero(peeked, out _, out _));
        }

        [Test]
        public void CurrentSaveVersion_IsAtLeastTwo()
        {
            Assert.GreaterOrEqual(SaveManager.CurrentSaveVersion, 2);
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private static SaveManager CreateManager()
        {
            var manager = new SaveManager();
            manager.Initialize();
            return manager;
        }

        private static string SlotPath(int slot) => Path.Combine(Application.persistentDataPath, "Saves", $"slot_{slot}.json");

        private static void WriteLegacySave(int slot, string version, int value)
        {
            var legacy = new SaveData { saveVersion = version, createdAtIso8601 = "2026-01-01T00:00:00.0000000Z", lastSavedAtIso8601 = "2026-01-01T00:00:00.0000000Z" };
            legacy.entries.Add(new SaveEntry
            {
                key = "probe",
                typeName = typeof(DummyState).AssemblyQualifiedName,
                json = JsonUtility.ToJson(new DummyState { value = value })
            });

            Directory.CreateDirectory(Path.GetDirectoryName(SlotPath(slot)));
            File.WriteAllText(SlotPath(slot), JsonUtility.ToJson(legacy, prettyPrint: true));
        }

        private SkillDefinition CreateSkill(string id, string displayName)
        {
            var skill = ScriptableObject.CreateInstance<SkillDefinition>();
            var so = new SerializedObject(skill);
            so.FindProperty("_skillId").stringValue = id;
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_skillType").enumValueIndex = (int)SkillType.Active;
            so.FindProperty("_maxLevel").intValue = 10;
            so.ApplyModifiedPropertiesWithoutUndo();
            _toDestroy.Add(skill);
            return skill;
        }

        private static void DeleteSlotFiles(int slot)
        {
            string main = SlotPath(slot);

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
