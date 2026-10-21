using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Localization;
using Rapadura.Core.Managers;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Building;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using Rapadura.UI.Menus;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="NewSessionReset"/> and the pause menu's return-to-title flow:
    /// a session A (quest, item, region, hero, boss, clock, recipe, structure, dialogue flag, weather)
    /// followed by "Novo jogo" or by loading another save must not leak anything into the next
    /// session — except achievements, which are per-profile lifetime progress. Uses the real
    /// <see cref="GameManager"/> so the registration audit in BuildManagers is what is tested.
    /// </summary>
    public class NewSessionResetTests
    {
        private const int SlotB = 90031;
        private const int LegacySlot = 90032;
        private const string SideQuestId = "quest_test_session_side";
        private const string BossId = "enemy_test_session_boss";
        private const string UnlockedRecipeId = "recipe_test_session_unlocked";

        private readonly List<Object> _created = new List<Object>();
        private GameObject _gameManagerObject;
        private GameManager _gm;

        private sealed class FakeSceneLoader : ISceneLoader
        {
            public readonly List<string> Loaded = new List<string>();
            public void LoadScene(string sceneName) => Loaded.Add(sceneName);
        }

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
            EventBus.Clear();
            ServiceLocator.Clear();

            if (GameManager.Instance != null)
            {
                Object.DestroyImmediate(GameManager.Instance.gameObject);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (_gm != null)
            {
                // Awake never ran in EditMode, so OnDestroy won't shut the managers down for us.
                var managers = (List<IManager>)typeof(GameManager)
                    .GetField("_managers", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(_gm);

                foreach (IManager manager in managers)
                {
                    manager.Shutdown();
                }
            }

            if (_gameManagerObject != null)
            {
                Object.DestroyImmediate(_gameManagerObject);
            }

            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
            _gm = null;
            EventBus.Clear();
            ServiceLocator.Clear();
            TestSaveRoot.Exit();
        }

        // ---------------------------------------------------------------
        // Registration audit
        // ---------------------------------------------------------------

        [Test]
        public void GameManager_RegistersEveryResettableManager_AndNotAchievements()
        {
            BuildGameManager();

            var registered = new HashSet<IResettable>(_gm.NewSessionReset.Resettables);
            var missing = new List<string>();

            foreach (PropertyInfo property in typeof(GameManager).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (property.GetValue(_gm) is IResettable resettable && !registered.Contains(resettable))
                {
                    missing.Add(property.Name);
                }
            }

            CollectionAssert.IsEmpty(missing, "Per-save managers not registered with NewSessionReset.");
            Assert.IsNotInstanceOf<IResettable>(_gm.AchievementManager, "Achievements are per-profile, never reset.");
            Assert.AreSame(_gm.NewSessionReset, _gm.SaveManager.SessionReset);

            IList<IResettable> order = _gm.NewSessionReset.Resettables.ToList();
            Assert.Less(order.IndexOf(_gm.QuestManager), order.IndexOf(_gm.DailyQuestService), "Dailies restart on an already-reset QuestManager.");
        }

        // ---------------------------------------------------------------
        // Novo jogo
        // ---------------------------------------------------------------

        [Test]
        public void NewGame_AfterSessionA_ResetsEverything_ExceptAchievements()
        {
            BuildGameManager();
            Assert.AreEqual(GameFlowState.MainMenu, _gm.GameFlowService.EnterFrontEnd());

            PlaySessionA(out long achievementQuestStat);

            Assert.IsTrue(_gm.GameFlowService.StartNewGame());

            AssertFreshSession();
            Assert.IsFalse(_gm.SaveManager.HasCarriedState("Inventory"), "The previous hero's inventory must not reach the new one.");
            Assert.AreEqual(achievementQuestStat, _gm.AchievementManager.GetStat(AchievementConditionType.QuestsCompleted), "Achievements are lifetime progress.");
            Assert.AreEqual(GameFlowState.CharacterSelect, _gm.GameFlowService.State);
        }

        [Test]
        public void NewGame_NewPlayerReceivesNoCarriedInventory()
        {
            BuildGameManager();
            _gm.GameFlowService.EnterFrontEnd();
            PlaySessionA(out _);

            _gm.GameFlowService.StartNewGame();

            InventoryManager fresh = CreateInventory("SessionB_Player");
            _gm.SaveManager.Register(fresh);

            Assert.AreEqual(0, _gm.SaveManager.RestoreCarriedState());
            Assert.AreEqual(0, fresh.GetTotalCount("item_test_session_token"));
        }

        [Test]
        public void ResetAll_PublishesEvent_AndIsolatesFailures()
        {
            var reset = new NewSessionReset();
            var log = new List<string>();
            reset.Register(new Probe("a", log));
            reset.Register(new ThrowingProbe());
            reset.Register(new Probe("b", log));

            int published = 0;
            Action<NewSessionResetEvent> handler = _ => published++;
            EventBus.Subscribe(handler);

            try
            {
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
                reset.ResetAll();
            }
            finally
            {
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
                EventBus.Unsubscribe(handler);
            }

            CollectionAssert.AreEqual(new[] { "a", "b" }, log);
            Assert.AreEqual(1, published);
        }

        // ---------------------------------------------------------------
        // Loading another save
        // ---------------------------------------------------------------

        [Test]
        public void LoadingSaveB_AfterSessionA_RestoresB_WithoutLeftoversFromA()
        {
            BuildGameManager();
            _gm.GameFlowService.EnterFrontEnd();

            // Save B: a fresh game with Joaquim, still on day 1 in the forest.
            _gm.CharacterSelectionState.Select(CharacterId.Joaquim);
            _gm.SaveManager.Save(SlotB);

            PlaySessionA(out long achievementQuestStat);

            Assert.IsTrue(_gm.SaveManager.Load(SlotB));

            Assert.AreEqual(CharacterId.Joaquim, _gm.CharacterSelectionState.Selected);
            AssertWorldFresh();
            Assert.IsFalse(_gm.SaveManager.HasCarriedState("Inventory"), "Save B had no player: A's carried inventory must be gone.");
            Assert.AreEqual(achievementQuestStat, _gm.AchievementManager.GetStat(AchievementConditionType.QuestsCompleted),
                "Loading a save with fewer achievement stats must not take lifetime progress away.");
        }

        [Test]
        public void LoadingLegacySave_MissingKeys_DoesNotKeepSessionAState()
        {
            // A pre-v2 save: only quests and the world map were saved back then.
            var legacyWriter = new SaveManager();
            legacyWriter.Initialize();
            legacyWriter.Register(new QuestManager());
            legacyWriter.Register(new WorldMapProgress());
            legacyWriter.Save(LegacySlot);
            Assume.That(File.Exists(TestSaveRoot.SlotPath(LegacySlot)));

            BuildGameManager();
            _gm.GameFlowService.EnterFrontEnd();
            PlaySessionA(out _);

            Assert.IsTrue(_gm.SaveManager.Load(LegacySlot));

            // Keys absent from the file (hero, bosses, clock, weather, recipes, structures) used to keep A's values.
            AssertFreshSession();
        }

        [Test]
        public void Continue_WithLegacySaveWithoutHero_OpensHeroPicker_InsteadOfKeepingA()
        {
            var legacyWriter = new SaveManager();
            legacyWriter.Initialize();
            legacyWriter.Register(new WorldMapProgress());
            legacyWriter.Save(SaveManager.AutoSaveSlot);

            BuildGameManager();
            _gm.GameFlowService.EnterFrontEnd();
            _gm.CharacterSelectionState.Select(CharacterId.Maithe);

            Assert.IsTrue(_gm.GameFlowService.Continue(new[] { SaveManager.AutoSaveSlot }));

            Assert.IsFalse(_gm.CharacterSelectionState.HasSelection);
            Assert.AreEqual(GameFlowState.CharacterSelect, _gm.GameFlowService.State);
        }

        // ---------------------------------------------------------------
        // Achievements: lifetime merge
        // ---------------------------------------------------------------

        [Test]
        public void AchievementRestore_MergesInsteadOfReplacing()
        {
            var kill = ScriptableObject.CreateInstance<AchievementDefinition>();
            _created.Add(kill);
            kill.ConfigureForTests("kill1", AchievementConditionType.EnemiesKilled, 1);

            var manager = new AchievementManager(new[] { kill });
            manager.Initialize();
            object empty = manager.CaptureProfileState();

            EventBus.Publish(new Rapadura.Gameplay.Enemies.EnemyDiedEvent(null, false));
            EventBus.Publish(new Rapadura.Gameplay.Enemies.EnemyDiedEvent(null, false));
            Assume.That(manager.IsUnlocked("kill1"));

            manager.RestoreState(empty);

            Assert.IsTrue(manager.IsUnlocked("kill1"));
            Assert.AreEqual(2, manager.GetStat(AchievementConditionType.EnemiesKilled));
            manager.Shutdown();
        }

        // ---------------------------------------------------------------
        // Pause menu "Menu principal"
        // ---------------------------------------------------------------

        [Test]
        public void ReturnToTitle_SaveAndQuit_AutoSavesAbandonsEncounterAndLoadsWorldMap()
        {
            var save = new SaveManager();
            save.Initialize();
            var selection = new CharacterSelectionState();
            save.Register(selection);
            var loader = new FakeSceneLoader();
            var transitions = new SceneTransitionService(new WorldMapProgress(), loader);
            var flow = new GameFlowService(selection, save, new NewSessionReset(), transitions);
            flow.Initialize();

            try
            {
                flow.EnterFrontEnd();
                flow.StartNewGame();
                selection.Select(CharacterId.Lavine);
                Assume.That(flow.State == GameFlowState.World);
                Assume.That(transitions.BeginEncounter(MakePayload()));
                Assume.That(flow.State == GameFlowState.Encounter);

                Assert.IsTrue(ReturnToTitleAction.Execute(ReturnToTitleChoice.SaveAndQuit, save, flow));

                Assert.IsTrue(save.SlotExists(SaveManager.AutoSaveSlot));
                Assert.AreEqual(GameFlowState.MainMenu, flow.State);
                Assert.IsFalse(transitions.HasPendingEncounter);
                Assert.IsNull(transitions.LastOutcome);
                Assert.AreEqual(SceneTransitionService.DefaultWorldMapSceneName, loader.Loaded.Last());

                // Back on the title, "Continuar" finds the save just written.
                Assert.IsTrue(flow.CanContinue(new[] { SaveManager.AutoSaveSlot }));
            }
            finally
            {
                flow.Shutdown();
            }
        }

        [Test]
        public void ReturnToTitle_QuitWithoutSaving_WritesNothing_AndCancelDoesNothing()
        {
            var save = new SaveManager();
            save.Initialize();
            var selection = new CharacterSelectionState();
            var loader = new FakeSceneLoader();
            var flow = new GameFlowService(selection, save, null, new SceneTransitionService(new WorldMapProgress(), loader));
            flow.Initialize();

            try
            {
                flow.EnterFrontEnd();
                flow.StartNewGame();
                selection.Select(CharacterId.Icaro);
                Assume.That(flow.State == GameFlowState.World);

                Assert.IsFalse(ReturnToTitleAction.Execute(ReturnToTitleChoice.Cancel, save, flow));
                Assert.AreEqual(GameFlowState.World, flow.State);
                CollectionAssert.IsEmpty(loader.Loaded);

                Assert.IsTrue(ReturnToTitleAction.Execute(ReturnToTitleChoice.QuitWithoutSaving, save, flow));
                Assert.IsFalse(save.SlotExists(SaveManager.AutoSaveSlot));
                Assert.AreEqual(GameFlowState.MainMenu, flow.State);
                CollectionAssert.AreEqual(new[] { SceneTransitionService.DefaultWorldMapSceneName }, loader.Loaded);

                Assert.IsFalse(flow.ReturnToTitle(), "Already on the title: no second scene load.");
                Assert.AreEqual(1, loader.Loaded.Count);
            }
            finally
            {
                flow.Shutdown();
            }
        }

        [Test]
        public void ReturnToTitleTexts_AreLocalized_AndDoNotShadowCoreKeys()
        {
            var coreKeys = new HashSet<string>(LocalizationManager.DefaultEntries.All.Select(e => e.Key));

            foreach (LocalizationEntry entry in ReturnToTitleTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), entry.Key);
                Assert.IsFalse(coreKeys.Contains(entry.Key), $"{entry.Key} shadows a core key.");
            }

            Assert.AreEqual("Voltar ao menu?", ReturnToTitleTexts.Get(null, ReturnToTitleTexts.Title));

            var manager = new LocalizationManager();
            manager.Initialize();
            manager.SetLanguage(LanguageCode.en);
            Assert.AreEqual("Quit without saving", ReturnToTitleTexts.Get(manager, ReturnToTitleTexts.QuitWithoutSaving));
        }

        [Test]
        public void ReturnToTitleDialog_IsLoadableFromResources()
        {
            var asset = Resources.Load<UnityEngine.UIElements.VisualTreeAsset>(PauseMenuController.ReturnToTitleDialogResourcePath);
            Assert.IsNotNull(asset);

            UnityEngine.UIElements.TemplateContainer tree = asset.Instantiate();
            Assert.IsNotNull(tree.Q("return-title-dialog"));
            Assert.IsNotNull(tree.Q<UnityEngine.UIElements.Button>("return-title-save-button"));
            Assert.IsNotNull(tree.Q<UnityEngine.UIElements.Button>("return-title-quit-button"));
            Assert.IsNotNull(tree.Q<UnityEngine.UIElements.Button>("return-title-cancel-button"));
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private void BuildGameManager()
        {
            _gameManagerObject = new GameObject("NewSessionResetTests_GameManager");
            _gm = _gameManagerObject.AddComponent<GameManager>();
            _gm.ForceInitializeForTests();
            Assume.That(_gm.NewSessionReset, Is.Not.Null);
        }

        /// <summary>Session A: everything a run can change in the persistent managers.</summary>
        private void PlaySessionA(out long achievementQuestStat)
        {
            _gm.CharacterSelectionState.Select(CharacterId.Maithe);

            Assert.IsTrue(_gm.QuestManager.StartQuest(CreateQuest(SideQuestId)));
            EventBus.Publish(new QuestCompletedEvent("quest_test_session_done")); // achievement stat only

            InventoryManager inventory = CreateInventory("SessionA_Player");
            _gm.SaveManager.Register(inventory);
            Assert.AreEqual(0, inventory.AddItem(CreateItem("item_test_session_token"), 3));
            _gm.SaveManager.Unregister(inventory); // Main.unity unloads → carried state

            _gm.WorldMapProgress.Unlock(BiomeType.Desert);
            _gm.WorldMapProgress.SetCurrentRegion(BiomeType.Desert);
            _gm.WorldMapProgress.RecordVictory(BiomeType.Desert);
            _gm.DefeatedEnemyRegistry.MarkDefeated(BossId);
            _gm.TimeOfDayManager.AdvanceHours(20f);
            _gm.TimeOfDayManager.AdvanceHours(10f);
            _gm.CraftingManager.RestoreState(new CraftingManagerSaveState { unlockedRecipeIds = new[] { UnlockedRecipeId } });
            _gm.BuildingManager.RestoreState(new BuildingManager.BuildingManagerSaveState
            {
                structures = new[] { new PlacedStructure { instanceId = "s1", structureId = "structure_test_session" } }
            });
            _gm.DialogueManager.SetFlag("flag_test_session_met");
            _gm.WeatherSystem.ForceWeather(WeatherType.Rain, instant: true);

            // Sanity: session A really changed things.
            Assume.That(_gm.QuestManager.IsActive(SideQuestId));
            Assume.That(_gm.SaveManager.HasCarriedState("Inventory"));
            Assume.That(_gm.TimeOfDayManager.Day, Is.EqualTo(2));
            Assume.That(_gm.CraftingManager.IsRecipeKnown(UnlockedRecipeId));

            achievementQuestStat = _gm.AchievementManager.GetStat(AchievementConditionType.QuestsCompleted);
            Assume.That(achievementQuestStat, Is.GreaterThan(0));
        }

        private void AssertFreshSession()
        {
            Assert.IsFalse(_gm.CharacterSelectionState.HasSelection, "hero");
            AssertWorldFresh();
        }

        private void AssertWorldFresh()
        {
            Assert.IsFalse(_gm.QuestManager.IsActive(SideQuestId), "quest");
            Assert.IsFalse(_gm.QuestManager.IsCompleted(SideQuestId), "quest");

            CollectionAssert.AreEquivalent(new[] { BiomeType.Forest }, _gm.WorldMapProgress.UnlockedRegions, "regions");
            Assert.AreEqual(BiomeType.Forest, _gm.WorldMapProgress.CurrentRegion);
            Assert.AreEqual(0, _gm.WorldMapProgress.GetVictories(BiomeType.Desert));

            Assert.IsFalse(_gm.DefeatedEnemyRegistry.IsDefeated(BossId), "boss");
            Assert.AreEqual(1, _gm.TimeOfDayManager.Day, "day");
            Assert.AreEqual(TimeOfDayManager.DefaultStartHour, _gm.TimeOfDayManager.CurrentHour, 0.001f, "hour");
            Assert.IsFalse(_gm.CraftingManager.IsRecipeKnown(UnlockedRecipeId), "recipe");
            CollectionAssert.IsEmpty(_gm.BuildingManager.PlacedStructures, "structures");
            Assert.IsFalse(_gm.DialogueManager.HasFlag("flag_test_session_met"), "dialogue flag");
            Assert.AreEqual(WeatherType.Clear, _gm.WeatherSystem.CurrentWeather, "weather");
            Assert.AreEqual(BiomeType.Forest, _gm.WeatherSystem.CurrentBiome, "weather biome");

            // Today's dailies are running again on the fresh quest log.
            foreach (string dailyId in _gm.DailyQuestService.TodayQuestIds)
            {
                Assert.IsTrue(_gm.QuestManager.IsActive(dailyId) || _gm.QuestManager.IsCompleted(dailyId), dailyId);
            }
        }

        private QuestDefinition CreateQuest(string questId)
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();
            _created.Add(quest);
            quest.SetDataForTests(questId, questId + ".title", questId + ".description", QuestType.Side,
                new List<QuestObjective>
                {
                    new QuestObjective { ObjectiveId = "do_it", Type = QuestObjectiveType.CustomFlag, TargetId = "flag_" + questId, RequiredAmount = 1 }
                });
            return quest;
        }

        private ItemDefinition CreateItem(string itemId)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            _created.Add(item);
            var serialized = new SerializedObject(item);
            serialized.FindProperty("_itemId").stringValue = itemId;
            serialized.FindProperty("_displayName").stringValue = itemId;
            serialized.FindProperty("_type").enumValueIndex = (int)ItemType.Material;
            serialized.FindProperty("_maxStack").intValue = 99;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        private InventoryManager CreateInventory(string name)
        {
            var go = new GameObject(name);
            _created.Add(go);
            return go.AddComponent<InventoryManager>();
        }

        private EncounterPayload MakePayload()
        {
            var enemy = ScriptableObject.CreateInstance<Rapadura.Gameplay.Enemies.EnemyDefinition>();
            enemy.name = "enemy_test_session_wolf";
            _created.Add(enemy);
            return EncounterPayload.Create(BiomeType.Forest, new[] { enemy }, seed: 1);
        }

        private sealed class Probe : IResettable
        {
            private readonly string _name;
            private readonly List<string> _log;

            public Probe(string name, List<string> log)
            {
                _name = name;
                _log = log;
            }

            public void ResetForNewSession() => _log.Add(_name);
        }

        private sealed class ThrowingProbe : IResettable
        {
            public void ResetForNewSession() => throw new InvalidOperationException("boom");
        }
    }
}
