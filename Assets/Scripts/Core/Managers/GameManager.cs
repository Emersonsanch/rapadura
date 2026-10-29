using System.Collections.Generic;
using Rapadura.Audio;
using Rapadura.Core.Accessibility;
using Rapadura.Core.Analytics;
using Rapadura.Core.Audio;
using Rapadura.Core.DI;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Localization;
using Rapadura.Core.Performance;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Building;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.Tutorial;
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
        public SfxEventRouter SfxEventRouter { get; private set; }
        public MusicDirector MusicDirector { get; private set; }
        public HapticsService HapticsService { get; private set; }
        public AccessibilitySettings AccessibilitySettings { get; private set; }
        public AnalyticsManager AnalyticsManager { get; private set; }
        public DialogueManager DialogueManager { get; private set; }
        public QuestManager QuestManager { get; private set; }
        public DailyQuestService DailyQuestService { get; private set; }
        public ShopManager ShopManager { get; private set; }
        public VillageChestStorage VillageChest { get; private set; }
        public WorldMapProgress WorldMapProgress { get; private set; }
        public StoryProgress StoryProgress { get; private set; }
        public SceneTransitionService SceneTransitionService { get; private set; }
        public FrameRateManager FrameRateManager { get; private set; }
        public CharacterSelectionState CharacterSelectionState { get; private set; }
        public DefeatedEnemyRegistry DefeatedEnemyRegistry { get; private set; }
        public AchievementManager AchievementManager { get; private set; }
        public TimeOfDayManager TimeOfDayManager { get; private set; }
        public WeatherSystem WeatherSystem { get; private set; }
        public GameFlowService GameFlowService { get; private set; }
        public NewSessionReset NewSessionReset { get; private set; }
        public TutorialManager TutorialManager { get; private set; }

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

            // Per-frame half of GameplayPause (modal screens freeze gameplay) — lives on this
            // persistent object so no scene needs wiring for it.
            if (GetComponent<GameplayPauseDriver>() == null)
            {
                gameObject.AddComponent<GameplayPauseDriver>();
            }
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
            LocalizationManager.SetLanguage(LocalizationManager.ResolveStartupLanguage(Application.systemLanguage, LocalizationManager.LoadSavedLanguage()));
            CraftingManager = new CraftingManager();
            BuildingManager = new BuildingManager();
            AudioManager = new AudioManager();
            SfxEventRouter = new SfxEventRouter(AudioManager);
            MusicDirector = new MusicDirector(AudioManager);
            HapticsService = new HapticsService();
            AccessibilitySettings = new AccessibilitySettings();
            AnalyticsManager = new AnalyticsManager();
            DialogueManager = new DialogueManager();
            QuestManager = new QuestManager();
            DailyQuestService = new DailyQuestService(QuestManager);
            ShopManager = new ShopManager();
            VillageChest = new VillageChestStorage();
            WorldMapProgress = new WorldMapProgress();
            StoryProgress = new StoryProgress(WorldMapProgress);
            SceneTransitionService = new SceneTransitionService(WorldMapProgress);
            FrameRateManager = new FrameRateManager();
            CharacterSelectionState = new CharacterSelectionState();
            DefeatedEnemyRegistry = new DefeatedEnemyRegistry();
            AchievementManager = new AchievementManager();
            TimeOfDayManager = new TimeOfDayManager();
            WeatherSystem = new WeatherSystem();
            NewSessionReset = new NewSessionReset();
            GameFlowService = new GameFlowService(CharacterSelectionState, SaveManager, NewSessionReset, SceneTransitionService);

            // First-time hints; per profile (Saves/tutorial.json), deliberately not session-resettable.
            TutorialManager = new TutorialManager();

            RegisterManager(SaveManager);
            RegisterManager(CheckpointManager);
            RegisterManager(LocalizationManager);
            RegisterManager(CraftingManager);
            RegisterManager(BuildingManager);
            RegisterManager(AudioManager);
            RegisterManager(SfxEventRouter);
            RegisterManager(MusicDirector);
            RegisterManager(HapticsService);
            RegisterManager(AccessibilitySettings);
            RegisterManager(AnalyticsManager);
            RegisterManager(DialogueManager);
            RegisterManager(QuestManager);
            RegisterManager(DailyQuestService);
            RegisterManager(ShopManager);
            RegisterManager(VillageChest);
            RegisterManager(WorldMapProgress);
            RegisterManager(StoryProgress);
            RegisterManager(SceneTransitionService);
            RegisterManager(FrameRateManager);
            RegisterManager(CharacterSelectionState);
            RegisterManager(DefeatedEnemyRegistry);
            RegisterManager(AchievementManager);
            RegisterManager(TimeOfDayManager);
            RegisterManager(WeatherSystem);
            RegisterManager(GameFlowService);
            RegisterManager(TutorialManager);

            SaveManager.Register(QuestManager);
            SaveManager.Register(DailyQuestService);
            SaveManager.Register(WorldMapProgress);
            SaveManager.Register(StoryProgress);

            // World state that used to be lost on load. CharacterSelectionState implements
            // IRestoreOrder (First), so the chosen hero's passive/skill set is re-applied before
            // PlayerStats/AttributeSet/SkillManager restore their saved values on top of it.
            SaveManager.Register(CharacterSelectionState);
            SaveManager.Register(BuildingManager);
            SaveManager.Register(CraftingManager);
            SaveManager.Register(DefeatedEnemyRegistry);
            SaveManager.Register(ShopManager);
            // "Baú da Vó Zefa" (village hub on the map); older saves have no entry and load an empty chest.
            SaveManager.Register(VillageChest);

            // Only reads achievements out of slots written before they moved to the profile file
            // (Saves/profile.json); AchievementManager.CaptureState returns null so slots no longer carry them.
            SaveManager.Register(AchievementManager);
            SaveManager.Register(TimeOfDayManager);
            SaveManager.Register(WeatherSystem);

            RegisterSessionResettables();

            // InventoryManager/SkillManager/ComboTracker/BuffController are MonoBehaviours living
            // directly on the scene's Player object (not IManager/ServiceLocator-registered) —
            // resolved via GetComponent by whatever needs them. QuestManager.SetRewardTargets is
            // called once the Player exists, from Start() (see RegisterQuestRewardTargets below),
            // since GameManager is constructed before any scene's player is spawned.
        }

        /// <summary>
        /// Every manager holding per-save state goes back to new-game state on "Novo jogo"
        /// (GameFlowService.StartNewGame) and right before a save is applied (SaveManager.Load), so
        /// nothing leaks from the previous session. Order matters: QuestManager before
        /// DailyQuestService (which restarts today's dailies on it), WorldMapProgress before
        /// WeatherSystem. Audit of the rest of <see cref="BuildManagers"/> — intentionally NOT reset:
        /// CheckpointManager (scene SpawnPoint refs, rebuilt per scene), LocalizationManager /
        /// AudioManager / AccessibilitySettings / FrameRateManager (per-device settings),
        /// AnalyticsManager (telemetry buffer), SceneTransitionService (scene hop bookkeeping; a load
        /// mid-encounter must keep its arena — ReturnToTitle abandons the encounter explicitly),
        /// AchievementManager (per-profile lifetime progress kept in Saves/profile.json; its
        /// RestoreState only merges legacy slot data in), GameFlowService (drives the reset itself). Player-scoped state (inventory,
        /// stats, skills...) lives on the scene's Player and reaches the next scene only as
        /// SaveManager carried state, which SaveManager's own reset clears.
        /// </summary>
        private void RegisterSessionResettables()
        {
            NewSessionReset.Register(SaveManager);
            NewSessionReset.Register(CraftingManager);
            NewSessionReset.Register(BuildingManager);
            NewSessionReset.Register(DialogueManager);
            NewSessionReset.Register(QuestManager);
            NewSessionReset.Register(DailyQuestService);
            NewSessionReset.Register(ShopManager);
            NewSessionReset.Register(VillageChest);
            NewSessionReset.Register(WorldMapProgress);
            NewSessionReset.Register(StoryProgress);
            NewSessionReset.Register(CharacterSelectionState);
            NewSessionReset.Register(DefeatedEnemyRegistry);
            NewSessionReset.Register(TimeOfDayManager);
            NewSessionReset.Register(WeatherSystem);

            SaveManager.SessionReset = NewSessionReset;
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
            // A duplicate (e.g. Main.unity's, loaded after WorldMap's persistent one) only removes
            // itself — SingletonBehaviour keeps the rest of its GameObject — and never built managers.
            if (!IsPrimaryInstance)
            {
                return;
            }

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
            StartCoroutine(RestoreCarriedStateAfterSceneStart());
        }

        /// <summary>
        /// Hands the player state carried across the scene swap (see SaveManager.RestoreCarriedState —
        /// e.g. level/inventory/skills after "Continuar" on the world map, or after the previous
        /// encounter) to the fresh Player. Waits one frame so every scene object's Start — where the
        /// player components register with SaveManager, and where the hero is re-applied — has run.
        /// Gameplay that grants XP during that frame (an early kill, a quest reward) is not lost:
        /// PlayerStats re-applies XP earned while its carried state was pending on top of the restore.
        /// </summary>
        private System.Collections.IEnumerator RestoreCarriedStateAfterSceneStart()
        {
            yield return null;
            SaveManager?.RestoreCarriedState();
            // Rewards of quests completed on the world map (no Player there) land now, after the
            // restore, so the carried inventory/XP doesn't overwrite them.
            QuestManager?.DeliverPendingRewards();
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
                // No Player in this scene (WorldMap.unity): drop the previous scene's destroyed one so
                // quests completed here queue their rewards (QuestManager.DeliverPendingRewards).
                QuestManager?.SetRewardTargets(null, null);
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
            foreach (SpawnPoint spawnPoint in FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None))
            {
                if (spawnPoint.IsInitialSpawn)
                {
                    CheckpointManager.SetInitialCheckpoint(spawnPoint);
                    break;
                }
            }
        }

        /// <summary>Achievements are per-profile (Saves/profile.json): flush them when the app is backgrounded (mobile may kill it there) or quits.</summary>
        private void OnApplicationPause(bool paused)
        {
            if (paused && Instance == this)
            {
                AchievementManager?.SaveProfile();
            }
        }

        private void OnApplicationQuit()
        {
            if (Instance == this)
            {
                AchievementManager?.SaveProfile();
            }
        }

        protected override void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            // Only the instance that built the managers tears them down: a removed duplicate (Main.unity's,
            // loaded after WorldMap's) never built any, and must not Shutdown/Clear the live instance's
            // services — Instance would even re-run FindFirstObjectByType mid-teardown.
            if (_managersBuilt)
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
