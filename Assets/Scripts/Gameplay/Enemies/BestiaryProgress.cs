using System;
using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>One page of <see cref="BestiarySaveState"/>: an enemy id (asset name) and how many were defeated.</summary>
    [Serializable]
    public class BestiaryKillRecord
    {
        public string enemyId;
        public int kills;
    }

    /// <summary>JSON-serializable snapshot of <see cref="BestiaryProgress"/>.</summary>
    [Serializable]
    public class BestiarySaveState
    {
        public List<BestiaryKillRecord> entries = new List<BestiaryKillRecord>();

        /// <summary>Every page was filled in this save (<see cref="BestiaryCompletedEvent"/> already raised).</summary>
        public bool completed;
    }

    /// <summary>Raised the first time an enemy type is defeated in this save (its bestiary page unlocks).</summary>
    public readonly struct BestiaryEntryUnlockedEvent : IGameEvent
    {
        public readonly string EnemyId;

        public BestiaryEntryUnlockedEvent(string enemyId)
        {
            EnemyId = enemyId;
        }
    }

    /// <summary>Raised once per save, when the last page of "Caderno da Vó Zefa" unlocks (the "Caderno Completo" achievement listens to it).</summary>
    public readonly struct BestiaryCompletedEvent : IGameEvent
    {
    }

    /// <summary>
    /// "Caderno da Vó Zefa" progress: which enemy types were defeated in this save and how many times.
    /// Plain C# (EditMode-testable), built by GameManager next to <see cref="DefeatedEnemyRegistry"/>,
    /// registered with SaveManager under <see cref="Key"/> and with NewSessionReset like every per-save system.
    /// <list type="bullet">
    /// <item>Listens to <see cref="EnemyDiedEvent"/> (regular enemies and bosses) and counts the kill under
    /// <see cref="DefeatedEnemyRegistry.GetId"/> of the enemy's definition; the first one unlocks the page
    /// (<see cref="BestiaryEntryUnlockedEvent"/>).</item>
    /// <item>Once every page of <see cref="BestiaryCatalog"/> is unlocked, raises <see cref="BestiaryCompletedEvent"/>
    /// once per save (the flag is saved, so a load never raises it again).</item>
    /// <item>A save written before the bestiary existed has no "Bestiary" entry and loads with an empty
    /// notebook. Bosses never respawn once beaten, though, so on every <see cref="GameLoadedEvent"/> the
    /// bosses <see cref="DefeatedEnemyRegistry"/> already has are reconciled into the notebook (1 kill each):
    /// an old save can still complete it.</item>
    /// </list>
    /// </summary>
    public class BestiaryProgress : IManager, ISaveable, IResettable
    {
        public const string Key = "Bestiary";

        private readonly Dictionary<string, int> _kills = new Dictionary<string, int>();
        private readonly Func<IReadOnlyList<string>> _catalogIds;
        private DefeatedEnemyRegistry _defeatedBosses;
        private bool _subscribed;

        /// <param name="defeatedBosses">Source of bosses beaten before the bestiary existed (may be null).</param>
        /// <param name="catalogIds">Every page id (default: <see cref="BestiaryCatalog.Ids"/>); tests pass their own.</param>
        public BestiaryProgress(DefeatedEnemyRegistry defeatedBosses = null, Func<IReadOnlyList<string>> catalogIds = null)
        {
            _defeatedBosses = defeatedBosses;
            _catalogIds = catalogIds ?? (() => BestiaryCatalog.Ids);
        }

        public string SaveKey => Key;

        /// <summary>The whole notebook was filled in this save (stays true even if new enemies are added later).</summary>
        public bool Completed { get; private set; }

        /// <summary>Ids of the unlocked pages (any order).</summary>
        public IReadOnlyCollection<string> UnlockedIds => _kills.Keys;

        public void SetDefeatedRegistry(DefeatedEnemyRegistry registry) => _defeatedBosses = registry;

        public void Initialize()
        {
            if (_subscribed)
            {
                return;
            }

            _subscribed = true;
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
        }

        public void Shutdown()
        {
            if (!_subscribed)
            {
                return;
            }

            _subscribed = false;
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
        }

        /// <summary>New game / right before a load: an empty notebook.</summary>
        public void ResetForNewSession()
        {
            _kills.Clear();
            Completed = false;
        }

        public bool IsUnlocked(string enemyId) => enemyId != null && _kills.ContainsKey(enemyId);

        public int GetKills(string enemyId) => enemyId != null && _kills.TryGetValue(enemyId, out int kills) ? kills : 0;

        /// <summary>How many catalog pages are unlocked (ids not in the catalog, e.g. a removed enemy, don't count).</summary>
        public int UnlockedCount
        {
            get
            {
                int count = 0;
                IReadOnlyList<string> ids = _catalogIds() ?? Array.Empty<string>();
                foreach (string id in ids)
                {
                    if (IsUnlocked(id))
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public int TotalCount => (_catalogIds() ?? Array.Empty<string>()).Count;

        /// <summary>True when every catalog page is unlocked (an empty catalog is never complete).</summary>
        public bool AllUnlocked
        {
            get
            {
                int total = TotalCount;
                return total > 0 && UnlockedCount == total;
            }
        }

        /// <summary>Counts one defeat of <paramref name="enemyId"/>. Returns true only when it unlocked the page.</summary>
        public bool RecordKill(string enemyId) => AddKills(enemyId, 1);

        private bool AddKills(string enemyId, int amount)
        {
            if (string.IsNullOrEmpty(enemyId) || amount <= 0)
            {
                return false;
            }

            bool unlocked = !_kills.TryGetValue(enemyId, out int kills);
            // Saturate instead of overflowing on absurd counts.
            _kills[enemyId] = kills > int.MaxValue - amount ? int.MaxValue : kills + amount;

            if (unlocked)
            {
                EventBus.Publish(new BestiaryEntryUnlockedEvent(enemyId));
                CheckCompleted();
            }

            return unlocked;
        }

        /// <summary>Raises <see cref="BestiaryCompletedEvent"/> the first time every page is unlocked. True only that time.</summary>
        public bool CheckCompleted()
        {
            if (Completed || !AllUnlocked)
            {
                return false;
            }

            Completed = true;
            EventBus.Publish(new BestiaryCompletedEvent());
            return true;
        }

        /// <summary>Unlocks (1 kill) every boss <see cref="DefeatedEnemyRegistry"/> knows but the notebook does not (see the class notes).</summary>
        public int ReconcileDefeatedBosses()
        {
            if (_defeatedBosses == null)
            {
                return 0;
            }

            int added = 0;
            // Copy first: unlocking publishes events whose listeners could touch the registry.
            foreach (string id in new List<string>(_defeatedBosses.DefeatedIds))
            {
                if (!IsUnlocked(id) && AddKills(id, 1))
                {
                    added++;
                }
            }

            return added;
        }

        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            if (evt.Enemy == null)
            {
                return;
            }

            EnemyController controller = evt.Enemy.GetComponent<EnemyController>();
            if (controller != null)
            {
                RecordKill(DefeatedEnemyRegistry.GetId(controller.Definition));
            }
        }

        private void OnGameLoaded(GameLoadedEvent evt)
        {
            ReconcileDefeatedBosses();
            // A catalog that shrank (an enemy removed) may leave a full notebook not flagged yet.
            CheckCompleted();
        }

        public object CaptureState()
        {
            var state = new BestiarySaveState { completed = Completed };
            var ids = new List<string>(_kills.Keys);
            ids.Sort(StringComparer.Ordinal);
            foreach (string id in ids)
            {
                state.entries.Add(new BestiaryKillRecord { enemyId = id, kills = _kills[id] });
            }

            return state;
        }

        public void RestoreState(object state)
        {
            if (state is not BestiarySaveState saved)
            {
                return;
            }

            _kills.Clear();
            Completed = saved.completed;

            if (saved.entries == null)
            {
                return;
            }

            foreach (BestiaryKillRecord record in saved.entries)
            {
                if (record == null || string.IsNullOrEmpty(record.enemyId))
                {
                    continue;
                }

                // A page saved with 0 (hand-edited / corrupted) still counts as seen once.
                int kills = Math.Max(1, record.kills);
                _kills[record.enemyId] = _kills.TryGetValue(record.enemyId, out int existing) ? Math.Max(existing, kills) : kills;
            }
        }
    }
}
