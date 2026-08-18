using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Localization;
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

        protected override void Awake()
        {
            base.Awake();

            if (Instance != this)
            {
                return;
            }

            BuildManagers();
            InitializeManagers();
        }

        private void BuildManagers()
        {
            SaveManager = new SaveManager();
            CheckpointManager = new CheckpointManager();
            LocalizationManager = new LocalizationManager();

            RegisterManager(SaveManager);
            RegisterManager(CheckpointManager);
            RegisterManager(LocalizationManager);

            // Future managers (InventoryManager, SkillManager, AudioManager, BuildingManager, etc.)
            // are constructed and registered here in dependency order as they are implemented.
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
