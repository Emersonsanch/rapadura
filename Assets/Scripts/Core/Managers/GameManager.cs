using System.Collections.Generic;
using Rapadura.Core.Accessibility;
using Rapadura.Core.Analytics;
using Rapadura.Core.Audio;
using Rapadura.Core.DI;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Localization;
using Rapadura.Core.Performance;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Building;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.World;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Core.Managers
{
    /// <summary>
    /// Application entry point. Lives on a single bootstrap GameObject placed in the first
    /// loaded scene, persists across scene loads, and is responsible for constructing every
    /// top-level manager and registering it with the <see cref="ServiceLocator"/> so gameplay
    /// systems can resolve dependencies without hard references.
    ///
    /// This is one of the few places a MonoBehaviour singleton is justified: everything else
    /// should depend on ServiceLocator, not on GameManager.Instance directly.
    /// </summary>
    public class GameManager : SingletonBehaviour<GameManager>
    {
        private readonly List<IManager> _managers = new List<IManager>();

        public SaveManager SaveManager { get; private set; }
        public CheckpointManager CheckpointManager { get; private set; }
        public LocalizationManager LocalizationManager { get; private set; }
        public CraftingManager CraftingManager { get; private set; }
        public BuildingManager BuildingManager { get; private set; }
        public AudioManager AudioManager { get; private set; }
        public AccessibilitySettings AccessibilitySettings { get; private set; }
        public AnalyticsManager AnalyticsManager { get; private set; }
        public DialogueManager DialogueManager { get; private set; }
        public QuestManager QuestManager { get; private set; }
        public ShopManager ShopManager { get; private set; }
        public WorldMapProgress WorldMapProgress { get; private set; }
        public SceneTransitionService SceneTransitionService { get; private set; }
        public FrameRateManager FrameRateManager { get; private set; }
        public CharacterSelectionState CharacterSelectionState { get; private set; }
        public DefeatedEnemyRegistry DefeatedEnemyRegistry { get; private set; }
        public AchievementManager AchievementManager { get; private set; }
        public TimeOfDayManager TimeOfDayManager { get; private set; }
        public WeatherSystem WeatherSystem { get; private set; }

        private bool _managersBuilt;

        protected override void Awake()
        {
            base.Awake();

            if (Instance != this)
            {
                return;
            }

            EnsureManagersBuilt();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <summary>
        /// Idempotent guard around <see cref="BuildManagers"/>/<see cref="InitializeManagers"/> —
        /// normally only reached from <see cref="Awake"/>, but also exposed (see
        /// <c>ForceInitializeForTests</c> below) since <c>AddComponent&lt;GameManager&gt;()</c>
        /// does not call <c>Awake</c> synchronously outside Play Mode, unlike Play Mode where it
        /// always does.
        /// </summary>
        private void EnsureManagersBuilt()
        {
            if (_managersBuilt)
            {
                return;
            }

            _managersBuilt = true;
            BuildManagers();
            InitializeManagers();
        }

#if UNITY_EDITOR
        /// <summary>Editor/test-only: forces manager construction immediately, see <see cref="EnsureManagersBuilt"/>.</summary>
        public void ForceInitializeForTests() => EnsureManagersBuilt();
#endif

        private void BuildManagers()
        {
            SaveManager = new SaveManager();
            CheckpointManager = new CheckpointManager();
            LocalizationManager = new LocalizationManager();
            CraftingManager = new CraftingManager();
            BuildingManager = new BuildingManager();
            AudioManager = new AudioManager();
            AccessibilitySettings = new AccessibilitySettings();
            AnalyticsManager = new AnalyticsManager();
            DialogueManager = new DialogueManager();
            QuestManager = new QuestManager();
            ShopManager = new ShopManager();
            WorldMapProgress = new WorldMapProgress();
            SceneTransitionService = new SceneTransitionService(WorldMapProgress);
            FrameRateManager = new FrameRateManager();
            CharacterSelectionState = new CharacterSelectionState();
            DefeatedEnemyRegistry = new DefeatedEnemyRegistry();
            AchievementManager = new AchievementManager();
            TimeOfDayManager = new TimeOfDayManager();
            WeatherSystem = new WeatherSystem();

            RegisterManager(SaveManager);
            RegisterManager(CheckpointManager);
            RegisterManager(LocalizationManager);
            RegisterManager(CraftingManager);
            RegisterManager(BuildingManager);
            RegisterManager(AudioManager);
            RegisterManager(AccessibilitySettings);
            RegisterManager(AnalyticsManager);
            RegisterManager(DialogueManager);
            RegisterManager(QuestManager);
            RegisterManager(ShopManager);
            RegisterManager(WorldMapProgress);
            RegisterManager(SceneTransitionService);
            RegisterManager(FrameRateManager);
            RegisterManager(CharacterSelectionState);
            RegisterManager(DefeatedEnemyRegistry);
            RegisterManager(AchievementManager);
            RegisterManager(TimeOfDayManager);
            RegisterManager(WeatherSystem);

            SaveManager.Register(QuestManager);
            SaveManager.Register(WorldMapProgress);

            // World state that used to be lost on load. CharacterSelectionState implements
            // IRestoreOrder (First), so the chosen hero's passive/skill set is re-applied before
            // PlayerStats/AttributeSet/SkillManager restore their saved values on top of it.
            SaveManager.Register(CharacterSelectionState);
            SaveManager.Register(BuildingManager);
            SaveManager.Register(CraftingManager);
            SaveManager.Register(DefeatedEnemyRegistry);
            SaveManager.Register(AchievementManager);
            SaveManager.Register(TimeOfDayManager);
            SaveManager.Register(WeatherSystem);

            // InventoryManager/SkillManager/ComboTracker/BuffController are MonoBehaviours living
            // directly on the scene's Player object (not IManager/ServiceLocator-registered) —
            // resolved via GetComponent by whatever needs them. QuestManager.SetRewardTargets is
            // called once the Player exists, from Start() (see RegisterQuestRewardTargets below),
            // since GameManager is constructed before any scene's player is spawned.
        }

        private void RegisterManager<TManager>(TManager manager) where TManager : IManager
        {
            ServiceLocator.Register(manager);
            _managers.Add(manager);
        }

        private void InitializeManagers()
        {
            foreach (IManager manager in _managers)
            {
                manager.Initialize();
            }
        }

        private void Start()
        {
            RegisterInitialCheckpoint();
            RegisterQuestRewardTargets();
        }

        /// <summary>
        /// GameManager persists across scene loads (WorldMap.unity ⇄ Main.unity for encounters, see
        /// SceneTransitionService), but Start only runs once — so without this, a Main.unity loaded
        /// after the world map never had its fresh Player/SpawnPoint wired into QuestManager/
        /// CheckpointManager. Both calls are idempotent, so also running on the first scene is harmless.
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (Instance != this)
            {
                return;
            }

            RegisterInitialCheckpoint();
            RegisterQuestRewardTargets();
        }

        /// <summary>
        /// Resolves the scene's Player and hands its InventoryManager/PlayerStats to
        /// <see cref="QuestManager.SetRewardTargets"/> — without this, the class-level doc comment
        /// on <see cref="Rapadura.Gameplay.Quests.QuestManager"/> explicitly says a "Scene
        /// bootstrap" is expected to call it once the player exists, but nothing anywhere in the
        /// project ever did. Result: every quest reward (items/XP) silently went nowhere, and
        /// CollectItem objectives (which read the reward inventory to check progress) could never
        /// be satisfied. Runs in Start for the same reason as RegisterInitialCheckpoint — the
        /// scene's Player doesn't exist yet when GameManager builds its managers.
        /// </summary>
        private void RegisterQuestRewardTargets()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                return;
            }

            InventoryManager inventory = player.GetComponent<InventoryManager>();
            PlayerStats stats = player.GetComponent<PlayerStats>();
            QuestManager.SetRewardTargets(inventory, stats);
        }

        /// <summary>
        /// Finds a scene <see cref="SpawnPoint"/> flagged <c>IsInitialSpawn</c> and registers it with
        /// <see cref="CheckpointManager"/> — nothing did this before, so a scene with no SpawnPoint the
        /// player has walked into yet (e.g. right after spawning, before reaching any checkpoint
        /// trigger) had no active checkpoint at all, and dying reverted to "revive in place" as the
        /// only fallback (see CheckpointManager.OnPlayerDied) instead of returning to the actual spawn.
        /// Runs in Start (not BuildManagers) because the scene's SpawnPoint objects don't exist yet
        /// when GameManager constructs its managers (see the BuildManagers ordering note above).
        /// </summary>
        private void RegisterInitialCheckpoint()
        {
            foreach (SpawnPoint spawnPoint in FindObjectsOfType<SpawnPoint>())
            {
                if (spawnPoint.IsInitialSpawn)
                {
                    CheckpointManager.SetInitialCheckpoint(spawnPoint);
                    break;
                }
            }
        }

        protected override void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (Instance == this)
            {
                foreach (IManager manager in _managers)
                {
                    manager.Shutdown();
                }

                ServiceLocator.Clear();
            }

            base.OnDestroy();
        }
    }
}
