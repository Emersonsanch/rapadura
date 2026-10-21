using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Player;
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

        /// <summary>
        /// When true, <see cref="PlayerDiedEvent"/> no longer respawns the player instantly — some
        /// other system (the Death screen, <c>Rapadura.UI.Death.DeathScreenController</c>) owns the
        /// "when" and calls <see cref="RespawnPlayer"/> itself after the player confirms (or a
        /// timeout elapses). Defaults to false so scenes/tests without a Death screen keep the
        /// original instant-respawn contract unchanged.
        /// </summary>
        public bool DeferRespawn { get; set; }

        private void OnPlayerDied(PlayerDiedEvent gameEvent)
        {
            if (DeferRespawn)
            {
                return;
            }

            RespawnPlayer();
        }

        /// <summary>
        /// Moves the Player-tagged object to the active checkpoint (if any) and revives it. Called
        /// automatically on <see cref="PlayerDiedEvent"/> unless <see cref="DeferRespawn"/> is set,
        /// in which case the Death screen calls it when the player picks "Continuar".
        /// </summary>
        public void RespawnPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);

            if (player == null)
            {
                return;
            }

            if (_activeCheckpoint != null)
            {
                player.transform.SetPositionAndRotation(_activeCheckpoint.Position, _activeCheckpoint.Rotation);
            }

            // Reviving was missing entirely before this fix: PlayerStats.Die() set IsDead permanently
            // true with nothing anywhere to clear it, so a scene with no SpawnPoint configured (like
            // Main.unity's current test setup — confirmed zero SpawnPoint instances in the scene)
            // soft-locked the run forever on the player's first death, checkpoint or not. Reviving
            // always (not only when a checkpoint exists) means dying is at worst "reset in place"
            // instead of "the run is over with no way to continue."
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            playerStats?.Revive();
        }
    }
}
