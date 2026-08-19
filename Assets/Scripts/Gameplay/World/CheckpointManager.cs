using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Save;
using UnityEngine;

namespace Rapadura.Gameplay.World
{
    /// <summary>
    /// Tracks the most recently activated <see cref="SpawnPoint"/> and moves the player back
    /// to it when <see cref="PlayerDiedEvent"/> fires. Registered with the ServiceLocator by
    /// GameManager alongside the other top-level managers.
    /// </summary>
    public class CheckpointManager : IManager
    {
        private const string PlayerTag = "Player";

        private SpawnPoint _activeCheckpoint;

        public void Initialize()
        {
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        public void Shutdown()
        {
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        /// <summary>Called by a SpawnPoint trigger when the player walks into it.</summary>
        public void ActivateCheckpoint(SpawnPoint spawnPoint)
        {
            _activeCheckpoint = spawnPoint;

            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.AutoSave();
            }
        }

        /// <summary>Registers the scene's initial spawn point without requiring the player to walk into it first.</summary>
        public void SetInitialCheckpoint(SpawnPoint spawnPoint)
        {
            if (_activeCheckpoint == null)
            {
                _activeCheckpoint = spawnPoint;
            }
        }

        private void OnPlayerDied(PlayerDiedEvent gameEvent)
        {
            if (_activeCheckpoint == null)
            {
                return;
            }

            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);

            if (player == null)
            {
                return;
            }

            // TODO(Fase 2): trocar por uma tela de morte/fade + delay real via corrotina quando
            // o combate e a UI de HUD existirem. Por enquanto o respawn é instantâneo.
            player.transform.SetPositionAndRotation(_activeCheckpoint.Position, _activeCheckpoint.Rotation);
        }
    }
}
