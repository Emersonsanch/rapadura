using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Player;
using Rapadura.Save;
using UnityEngine;

namespace Rapadura.Gameplay.World
{
    /// <summary>
    /// Scene entry points and the one place a death may still be resolved inside a scene. Registered with the
    /// ServiceLocator by GameManager alongside the other top-level managers.
    /// <para>
    /// Respawn rule (Docs/Progressao500.md §7, decision of 2026-10-30): a defeat always wakes the hero at the plaza of the
    /// region's city. The Death screen (<c>Rapadura.UI.Death.DeathScreenController</c>) owns that: it sets
    /// <see cref="DeferRespawn"/> and sends the hero to the city. What is left here:
    /// <list type="bullet">
    /// <item><see cref="SpawnPoint"/>s are only <b>scene entry points</b>: the initial one places the hero, walking
    /// into another one auto-saves, and none of them changes where a death is resolved any more (no "arena
    /// checkpoints").</item>
    /// <item>The walkable city marks its plaza with <see cref="SetCityRespawn"/> (<c>CityModeController</c>): the only
    /// "death checkpoint" that remains.</item>
    /// <item><see cref="RespawnPlayer"/> (instant respawn without a Death screen: test scenes, safety net) revives at
    /// the city plaza when one is set, else at the scene's entry point, else in place.</item>
    /// </list>
    /// </para>
    /// </summary>
    public class CheckpointManager : IManager
    {
        private const string PlayerTag = "Player";

        private SpawnPoint _entryPoint;
        private SpawnPoint _cityRespawn;

        /// <summary>The scene's entry point (initial <see cref="SpawnPoint"/>), or null.</summary>
        public SpawnPoint EntryPoint => _entryPoint;

        /// <summary>The plaza of the walkable city, when the hero is in one; null otherwise.</summary>
        public SpawnPoint CityRespawn => _cityRespawn;

        /// <summary>Where <see cref="RespawnPlayer"/> puts the hero: the city plaza, else the scene entry point (null = in place).</summary>
        public SpawnPoint RespawnTarget => _cityRespawn != null ? _cityRespawn : _entryPoint;

        public void Initialize()
        {
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        public void Shutdown()
        {
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        /// <summary>
        /// Called by a SpawnPoint trigger when the player walks into it: auto-saves. It does not move the respawn (deaths
        /// go to the city); it only becomes the entry point when the scene has none yet.
        /// </summary>
        public void ActivateCheckpoint(SpawnPoint spawnPoint)
        {
            if (_entryPoint == null)
            {
                _entryPoint = spawnPoint;
            }

            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.AutoSave();
            }
        }

        /// <summary>Registers the scene's initial spawn point (its entry point) without requiring the player to walk into it first.</summary>
        public void SetInitialCheckpoint(SpawnPoint spawnPoint)
        {
            if (_entryPoint == null)
            {
                _entryPoint = spawnPoint;
            }
        }

        /// <summary>The walkable city's plaza (<c>CityModeController</c>): a death in the city wakes the hero there. Auto-saves like a checkpoint.</summary>
        public void SetCityRespawn(SpawnPoint plaza)
        {
            _cityRespawn = plaza;
            if (plaza != null && ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.AutoSave();
            }
        }

        /// <summary>
        /// When true, <see cref="PlayerDiedEvent"/> no longer respawns the player instantly — the Death screen
        /// (<c>Rapadura.UI.Death.DeathScreenController</c>) owns the "when" and the "where" (the city of the region).
        /// Defaults to false so scenes/tests without a Death screen keep the instant-respawn contract.
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
        /// Moves the Player-tagged object to <see cref="RespawnTarget"/> (if any) and revives it. Called automatically on
        /// <see cref="PlayerDiedEvent"/> unless <see cref="DeferRespawn"/> is set.
        /// </summary>
        public void RespawnPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);

            if (player == null)
            {
                return;
            }

            SpawnPoint target = RespawnTarget;
            if (target != null)
            {
                player.transform.SetPositionAndRotation(target.Position, target.Rotation);
            }

            // Reviving always (not only when a target exists) means dying is at worst "reset in place" instead of a
            // soft-lock (PlayerStats.Die() sets IsDead with nothing else to clear it).
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            playerStats?.Revive();
        }
    }
}
