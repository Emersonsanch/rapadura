using NUnit.Framework;
using Rapadura.Core.Accessibility;
using Rapadura.Core.Analytics;
using Rapadura.Core.Audio;
using Rapadura.Core.DI;
using Rapadura.Core.Managers;
using Rapadura.Gameplay.Building;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Shop;
using Rapadura.Save;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests verifying that <see cref="GameManager"/> constructs and registers
    /// <see cref="CraftingManager"/>, <see cref="BuildingManager"/> and <see cref="AudioManager"/>
    /// on bootstrap, alongside the pre-existing SaveManager/CheckpointManager/LocalizationManager.
    /// </summary>
    public class GameManagerTests
    {
        private GameObject _go;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();

            // Defensive against leaked state from any earlier test (in this run or another test
            // class) that constructed a GameManager and didn't clean it up before this one starts:
            // SingletonBehaviour<GameManager>.Awake() bails out immediately whenever a stale
            // Instance is still set and isn't this GameObject, silently skipping BuildManagers()
            // and leaving every manager property null — which read as this class's own bug until
            // traced back to singleton leakage, not anything specific to GameManagerTests itself.
            if (GameManager.Instance != null)
            {
                Object.DestroyImmediate(GameManager.Instance.gameObject);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
            {
                Object.DestroyImmediate(_go);
            }

            ServiceLocator.Clear();
        }

        [Test]
        public void Awake_RegistersCraftingBuildingAndAudioManagers()
        {
            _go = new GameObject("GameManagerTestTarget");
            GameManager gameManager = _go.AddComponent<GameManager>();
            // AddComponent doesn't call Awake synchronously outside Play Mode; force it so
            // BuildManagers()/InitializeManagers() actually run before the assertions below.
            gameManager.ForceInitializeForTests();

            Assert.IsNotNull(gameManager.CraftingManager);
            Assert.IsNotNull(gameManager.BuildingManager);
            Assert.IsNotNull(gameManager.AudioManager);

            Assert.IsTrue(ServiceLocator.IsRegistered<CraftingManager>());
            Assert.IsTrue(ServiceLocator.IsRegistered<BuildingManager>());
            Assert.IsTrue(ServiceLocator.IsRegistered<AudioManager>());

            Assert.AreSame(gameManager.CraftingManager, ServiceLocator.Get<CraftingManager>());
            Assert.AreSame(gameManager.BuildingManager, ServiceLocator.Get<BuildingManager>());
            Assert.AreSame(gameManager.AudioManager, ServiceLocator.Get<AudioManager>());
        }

        [Test]
        public void Awake_RegistersAccessibilitySettingsAndAnalyticsManager()
        {
            _go = new GameObject("GameManagerTestTarget");
            GameManager gameManager = _go.AddComponent<GameManager>();
            // AddComponent doesn't call Awake synchronously outside Play Mode; force it so
            // BuildManagers()/InitializeManagers() actually run before the assertions below.
            gameManager.ForceInitializeForTests();

            Assert.IsNotNull(gameManager.AccessibilitySettings);
            Assert.IsNotNull(gameManager.AnalyticsManager);

            Assert.IsTrue(ServiceLocator.IsRegistered<AccessibilitySettings>());
            Assert.IsTrue(ServiceLocator.IsRegistered<AnalyticsManager>());

            Assert.AreSame(gameManager.AccessibilitySettings, ServiceLocator.Get<AccessibilitySettings>());
            Assert.AreSame(gameManager.AnalyticsManager, ServiceLocator.Get<AnalyticsManager>());
        }

        [Test]
        public void Awake_RegistersDialogueManager()
        {
            _go = new GameObject("GameManagerTestTarget");
            GameManager gameManager = _go.AddComponent<GameManager>();
            // AddComponent doesn't call Awake synchronously outside Play Mode; force it so
            // BuildManagers()/InitializeManagers() actually run before the assertions below.
            gameManager.ForceInitializeForTests();

            Assert.IsNotNull(gameManager.DialogueManager);
            Assert.IsTrue(ServiceLocator.IsRegistered<DialogueManager>());
            Assert.AreSame(gameManager.DialogueManager, ServiceLocator.Get<DialogueManager>());
        }

        [Test]
        public void Awake_RegistersQuestManagerAndWithSaveManager()
        {
            _go = new GameObject("GameManagerTestTarget");
            GameManager gameManager = _go.AddComponent<GameManager>();
            // AddComponent doesn't call Awake synchronously outside Play Mode; force it so
            // BuildManagers()/InitializeManagers() actually run before the assertions below.
            gameManager.ForceInitializeForTests();

            Assert.IsNotNull(gameManager.QuestManager);
            Assert.IsTrue(ServiceLocator.IsRegistered<QuestManager>());
            Assert.AreSame(gameManager.QuestManager, ServiceLocator.Get<QuestManager>());

            // QuestManager implements ISaveable and must also be registered with SaveManager so
            // its state is captured/restored on save/load, same as InventoryManager/PlayerStats.
            object capturedState = gameManager.QuestManager.CaptureState();
            Assert.IsInstanceOf<QuestSaveState>(capturedState);

            var saveablesField = typeof(SaveManager).GetField("_saveables",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var saveables = (System.Collections.IDictionary)saveablesField.GetValue(gameManager.SaveManager);

            Assert.IsTrue(saveables.Contains(gameManager.QuestManager.SaveKey));
            Assert.AreSame(gameManager.QuestManager, saveables[gameManager.QuestManager.SaveKey]);
        }

        [Test]
        public void Awake_RegistersShopManager()
        {
            _go = new GameObject("GameManagerTestTarget");
            GameManager gameManager = _go.AddComponent<GameManager>();
            // AddComponent doesn't call Awake synchronously outside Play Mode; force it so
            // BuildManagers()/InitializeManagers() actually run before the assertions below.
            gameManager.ForceInitializeForTests();

            Assert.IsNotNull(gameManager.ShopManager);
            Assert.IsTrue(ServiceLocator.IsRegistered<ShopManager>());
            Assert.AreSame(gameManager.ShopManager, ServiceLocator.Get<ShopManager>());
        }
    }
}
