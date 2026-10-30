using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.WorldEvents
{
    /// <summary>One world boss in <see cref="WorldEventSaveState"/>.</summary>
    [Serializable]
    public class WorldBossRecord
    {
        public string bossId;
        public int defeats;
        public int nightDefeats;

        /// <summary>Window whose first defeat was already paid (one reward per window); -1 = never.</summary>
        public long lastRewardedWindow = -1;
    }

    /// <summary>JSON-serializable snapshot of <see cref="WorldEventProgress"/>.</summary>
    [Serializable]
    public class WorldEventSaveState
    {
        public List<WorldBossRecord> bosses = new List<WorldBossRecord>();
        public long lastChestWindow = -1;
        public int bestChestTier;
        public int invasionsDefended;

        /// <summary>Chests won per tier: index 0 = +1 … index 4 = +5.</summary>
        public List<int> chestsByTier = new List<int>();
    }

    /// <summary>
    /// Per-save state of the scheduled world events (Docs/WorldEvents.md): world-boss defeats and the window each boss
    /// last paid in, and the Baú do Cangaço record (one chest per invasion window). Plain C# (EditMode-testable), built
    /// by GameManager, registered with SaveManager under <see cref="Key"/> and with NewSessionReset; a save from before
    /// the events has no entry and loads empty. The calendar itself is not saved: it comes from the real clock
    /// (<see cref="WorldEventCalendar"/>), so it is the same for everyone.
    /// </summary>
    public class WorldEventProgress : IManager, ISaveable, IResettable
    {
        /// <summary>Not "WorldEvents": that key belongs to the road events (<c>WorldEventState</c>).</summary>
        public const string Key = "ScheduledWorldEvents";

        private readonly Dictionary<string, WorldBossRecord> _bosses = new Dictionary<string, WorldBossRecord>(StringComparer.Ordinal);
        private readonly int[] _chests = new int[InvasionRules.MaxTier];
        private long _lastChestWindow = -1;
        private int _bestChestTier;
        private int _invasionsDefended;
        private static WorldEventProgress _standalone;

        public string SaveKey => Key;

        public WorldEventProgress()
        {
            ResetForNewSession();
        }

        public static WorldEventProgress ResolveShared()
        {
            if (ServiceLocator.TryGet(out WorldEventProgress progress))
            {
                return progress;
            }

            return _standalone ??= new WorldEventProgress();
        }

        public void Initialize()
        {
            if (ServiceLocator.TryGet(out LocalizationManager localization))
            {
                WorldBossTexts.EnsureRegistered(localization);
            }
        }

        public void Shutdown()
        {
        }

        public void ResetForNewSession()
        {
            _bosses.Clear();
            foreach (WorldBossDefinition boss in WorldBossRules.All)
            {
                _bosses[boss.Id] = new WorldBossRecord { bossId = boss.Id };
            }

            Array.Clear(_chests, 0, _chests.Length);
            _lastChestWindow = -1;
            _bestChestTier = 0;
            _invasionsDefended = 0;
        }

        // ------------------------------------------------------------------
        // World bosses
        // ------------------------------------------------------------------

        public int Defeats(string bossId) => Record(bossId)?.defeats ?? 0;

        public int NightDefeats(string bossId) => Record(bossId)?.nightDefeats ?? 0;

        public long LastRewardedWindow(string bossId) => Record(bossId)?.lastRewardedWindow ?? -1;

        /// <summary>Open, and its first defeat not paid yet (the boss can be fought again after a loss in the same window).</summary>
        public bool CanFightBoss(string bossId, RealTimeWindow window) => Record(bossId) != null && window.IsOpen && window.Index != LastRewardedWindow(bossId);

        /// <summary>Records a defeat. True when it is the first of <paramref name="windowIndex"/> (the one that pays).</summary>
        public bool RecordBossDefeat(string bossId, long windowIndex, bool night)
        {
            WorldBossRecord record = Record(bossId);
            if (record == null)
            {
                return false;
            }

            record.defeats++;
            if (night)
            {
                record.nightDefeats++;
            }

            if (record.lastRewardedWindow == windowIndex)
            {
                return false;
            }

            record.lastRewardedWindow = windowIndex;
            return true;
        }

        private WorldBossRecord Record(string bossId) => bossId != null && _bosses.TryGetValue(bossId, out WorldBossRecord record) ? record : null;

        // ------------------------------------------------------------------
        // Invasions
        // ------------------------------------------------------------------

        public long LastChestWindow => _lastChestWindow;
        public int BestChestTier => _bestChestTier;
        public int InvasionsDefended => _invasionsDefended;

        /// <summary>Chests won of tier +<paramref name="tier"/> (1–5).</summary>
        public int Chests(int tier) => tier >= 1 && tier <= _chests.Length ? _chests[tier - 1] : 0;

        /// <summary>Open, and no chest earned in this window yet (a run that earned nothing can be tried again).</summary>
        public bool CanDefend(RealTimeWindow window) => window.IsOpen && window.Index != _lastChestWindow;

        /// <summary>Records a finished defence. True when it paid a chest (tier ≥ 1 and the first chest of the window).</summary>
        public bool RecordInvasion(long windowIndex, int tier, bool captainDefeated)
        {
            if (captainDefeated)
            {
                _invasionsDefended++;
            }

            if (tier <= 0 || windowIndex == _lastChestWindow)
            {
                return false;
            }

            tier = Math.Min(tier, InvasionRules.MaxTier);
            _lastChestWindow = windowIndex;
            _chests[tier - 1]++;
            _bestChestTier = Math.Max(_bestChestTier, tier);
            return true;
        }

        // ------------------------------------------------------------------
        // Player id of the reward seeds
        // ------------------------------------------------------------------

        /// <summary>"hero:maithe" — offline the playing cousin; online the account id replaces it (server side).</summary>
        public static string PlayerId()
        {
            CharacterId hero = ServiceLocator.TryGet(out CharacterSelectionState selection) && selection.Selected.HasValue
                ? selection.Selected.Value
                : CharacterId.Maithe;
            return "hero:" + hero.ToString().ToLowerInvariant();
        }

        // ------------------------------------------------------------------
        // Save
        // ------------------------------------------------------------------

        public object CaptureState()
        {
            var state = new WorldEventSaveState
            {
                lastChestWindow = _lastChestWindow,
                bestChestTier = _bestChestTier,
                invasionsDefended = _invasionsDefended,
                chestsByTier = new List<int>(_chests),
            };

            foreach (WorldBossDefinition boss in WorldBossRules.All)
            {
                WorldBossRecord record = Record(boss.Id);
                state.bosses.Add(new WorldBossRecord
                {
                    bossId = boss.Id,
                    defeats = record.defeats,
                    nightDefeats = record.nightDefeats,
                    lastRewardedWindow = record.lastRewardedWindow,
                });
            }

            return state;
        }

        public void RestoreState(object state)
        {
            if (state is not WorldEventSaveState saved)
            {
                return;
            }

            ResetForNewSession();
            if (saved.bosses != null)
            {
                foreach (WorldBossRecord entry in saved.bosses)
                {
                    WorldBossRecord record = entry != null ? Record(entry.bossId) : null;
                    if (record == null)
                    {
                        continue;
                    }

                    record.defeats = Math.Max(0, entry.defeats);
                    record.nightDefeats = Math.Max(0, Math.Min(entry.nightDefeats, record.defeats));
                    record.lastRewardedWindow = Math.Max(-1, entry.lastRewardedWindow);
                }
            }

            _lastChestWindow = Math.Max(-1, saved.lastChestWindow);
            _bestChestTier = Math.Max(0, Math.Min(saved.bestChestTier, InvasionRules.MaxTier))