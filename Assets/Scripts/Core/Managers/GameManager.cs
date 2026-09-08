using System.Collections.Generic;
using Rapadura.Core.Accessibility;
using Rapadura.Core.Analytics;
using Rapadura.Core.Audio;
using Rapadura.Core.DI;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Building;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.World;
using Rapadura.Save;
using UnityEngine;

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

        private bool _managersBuilt;

        protected override void Awake()
        {
            base.Awake();

            if (Instance != this)
            {
                return;
            }

            EnsureManagersBuilt();
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

            SaveManager.Register(QuestManager);

            // Future managers (InventoryManager, SkillManager, etc.) are constructed and
            // registered here in dependency order as they are implemented.
            //
            // NOTE: QuestManager.SetRewardTargets(InventoryManager, PlayerStats) still needs to be
            // called once the player's components exist (Scene bootstrap) so reward granting and
            // CollectItem checks work — those components aren't available here since GameManager
            // is constructed before any scene's player is spawned.
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
