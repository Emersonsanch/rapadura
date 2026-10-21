using System;
using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>JSON-serializable snapshot of <see cref="DefeatedEnemyRegistry"/>.</summary>
    [Serializable]
    public class DefeatedEnemySaveState
    {
        public string[] defeatedIds = Array.Empty<string>();
    }

    /// <summary>
    /// Persistent record of unique enemies (today: every boss) the player has already defeated, so
    /// that knowledge survives save/load. Enemies have no dedicated id field, so a boss is keyed by
    /// its <see cref="EnemyDefinition"/> asset name (see <see cref="GetId"/>). Listens to
    /// <see cref="EnemyDiedEvent"/> and records any death flagged <c>WasBoss</c>; other systems
    /// (spawners, quests, map unlocks) can query <see cref="IsDefeated(string)"/>. Spawning is gated
    /// through <see cref="CanSpawn"/>: <c>EnemySpawner.SpawnBoss</c>, the world map's
    /// <c>RandomEncounterSystem</c> rolls (via <c>BiomeDefinition.RollEnemy(random, filter)</c>) and
    /// <c>EncounterArenaController</c> all skip a unique enemy that was already defeated.
    ///
    /// A plain <see cref="IManager"/> built by GameManager and registered with SaveManager under
    /// <see cref="SaveKey"/>, same shape as WorldMapProgress/QuestManager.
    /// </summary>
    public class DefeatedEnemyRegistry : IManager, ISaveable
    {
        public const string Key = "DefeatedEnemies";

        private readonly HashSet<string> _defeated = new HashSet<string>();

        public string SaveKey => Key;

        public IReadOnlyCollection<string> DefeatedIds => _defeated;

        public void Initialize()
        {
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
        }

        public void Shutdown()
        {
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            _defeated.Clear();
        }

        /// <summary>Stable id used for a definition (its asset name); null for a null definition.</summary>
        public static string GetId(EnemyDefinition definition) => definition != null ? definition.name : null;

        public bool IsDefeated(string enemyId) => !string.IsNullOrEmpty(enemyId) && _defeated.Contains(enemyId);

        public bool IsDefeated(EnemyDefinition definition) => IsDefeated(GetId(definition));

        /// <summary>
        /// True if <paramref name="definition"/> is a unique enemy (today: <see cref="EnemyDefinition.IsBoss"/>)
        /// that should not appear again because it was already defeated. Regular enemies always respawn.
        /// </summary>
        public bool ShouldSkipSpawn(EnemyDefinition definition)
        {
            return definition != null && definition.IsBoss && IsDefeated(definition);
        }

        /// <summary>
        /// Spawn gate usable as a <c>Predicate&lt;EnemyDefinition&gt;</c> (e.g. for
        /// <c>BiomeDefinition.RollEnemy(random, filter)</c>): false for null definitions and for
        /// defeated unique enemies.
        /// </summary>
        public bool CanSpawn(EnemyDefinition definition)
        {
            return definition != null && !ShouldSkipSpawn(definition);
        }

        /// <summary>Records an enemy as defeated. Returns true only the first time.</summary>
        public bool MarkDefeated(string enemyId)
        {
            return !string.IsNullOrEmpty(enemyId) && _defeated.Add(enemyId);
        }

        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            if (!evt.WasBoss || evt.Enemy == null)
            {
                return;
            }

            EnemyController controller = evt.Enemy.GetComponent<EnemyController>();

            if (controller != null)
            {
                MarkDefeated(GetId(controller.Definition));
            }
        }

        public object CaptureState()
        {
            var ids = new List<string>(_defeated);
            ids.Sort(StringComparer.Ordinal);
            return new DefeatedEnemySaveState { defeatedIds = ids.ToArray() };
        }

        public void RestoreState(object state)
        {
            if (state is not DefeatedEnemySaveState saved)
            {
                return;
            }

            _defeated.Clear();

            if (saved.defeatedIds == null)
            {
                return;
            }

            foreach (string id in saved.defeatedIds)
            {
                MarkDefeated(id);
            }
        }
    }
}
