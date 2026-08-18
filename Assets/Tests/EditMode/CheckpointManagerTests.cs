using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Gameplay.World;
using Rapadura.Save;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="CheckpointManager"/>, focused on the integration with
    /// <see cref="SaveManager"/>: activating a checkpoint must trigger an auto-save through the
    /// ServiceLocator. Uses a real SaveManager writing to a dedicated auto-save slot (there is
    /// only one reserved auto-save slot, so no fake/mock is needed) and cleans up afterwards.
    /// </summary>
    public class CheckpointManagerTests
    {
        private GameObject _spawnGo;
        private SpawnPoint _spawnPoint;
        private CheckpointManager _checkpointManager;
        private SaveManager _saveManager;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();

            _saveManager = new SaveManager();
            _saveManager.Initialize();
            ServiceLocator.Register(_saveManager);

            _checkpointManager = new CheckpointManager();
            _checkpointManager.Initialize();
            ServiceLocator.Register(_checkpointManager);

            _spawnGo = new GameObject("SpawnPoint");
            _spawnPoint = _spawnGo.AddComponent<SpawnPoint>();

            _saveManager.DeleteSlot(SaveManager.AutoSaveSlot);
        }

        [TearDown]
        public void TearDown()
        {
            _saveManager.DeleteSlot(SaveManager.AutoSaveSlot);
            _checkpointManager.Shutdown();
            Object.DestroyImmediate(_spawnGo);
            ServiceLocator.Clear();
        }

        [Test]
        public void ActivateCheckpoint_TriggersAutoSave()
        {
            Assert.IsFalse(_saveManager.HasAutoSave());

            _checkpointManager.ActivateCheckpoint(_spawnPoint);

            Assert.IsTrue(_saveManager.HasAutoSave(), "Activating a checkpoint should write an auto-save.");
        }

        [Test]
        public void ActivateCheckpoint_WithoutRegisteredSaveManager_DoesNotThrow()
        {
            ServiceLocator.Unregister<SaveManager>();

            Assert.DoesNotThrow(() => _checkpointManager.ActivateCheckpoint(_spawnPoint));
        }
    }
}
