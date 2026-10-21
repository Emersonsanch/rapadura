using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Localization;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Environment;
using UnityEngine;

namespace Rapadura.Gameplay.Quests
{
    /// <summary>Plain-C# JSON-serializable snapshot persisted by <see cref="DailyQuestService"/>.</summary>
    [Serializable]
    public class DailyQuestSaveState
    {
        /// <summary>Local calendar day ("yyyy-MM-dd") of the rotation in effect. Also the high-water mark used to ignore clock rollbacks.</summary>
        public string DayKey;

        /// <summary>Quest ids picked for <see cref="DayKey"/>, in pick order.</summary>
        public List<string> TodayQuestIds = new List<string>();
    }

    /// <summary>
    /// Daily quest rotation: plain C# <see cref="IManager"/> + <see cref="ISaveable"/>, same shape
    /// as <see cref="QuestManager"/>. Once per local calendar day it
    ///   1. abandons yesterday's unfinished dailies and forgets the finished ones
    ///      (<see cref="QuestManager.ResetDailyQuest"/>), so they can be done again;
    ///   2. picks <see cref="DailyCount"/> quests from the pool (every <see cref="QuestType.Daily"/>
    ///      asset under <c>Resources/Quests/Daily</c>) with a PRNG seeded by the date — the same day
    ///      always gives the same picks, on every device, regardless of Resources load order;
    ///   3. starts them through the normal <see cref="QuestManager.StartQuest"/> path, so objective
    ///      progress, rewards and the quest log ("Diária" badge) work exactly like any other quest.
    ///
    /// Rewards are only ever granted by QuestManager.CompleteQuest, and a completed daily stays in
    /// QuestManager's completed set (blocking a second StartQuest) until the rotation moves to a
    /// strictly later day. The saved <see cref="DailyQuestSaveState.DayKey"/> is a high-water mark:
    /// if the system clock moves backwards, the rotation simply stays on the newer day, so nothing
    /// is reset and no reward can be earned twice.
    ///
    /// Day changes are detected on <see cref="Initialize"/>, after a load (<see cref="RestoreState"/>
    /// and <see cref="GameLoadedEvent"/>), on every in-game hour tick (<see cref="TimeOfDayHourChangedEvent"/>)
    /// and whenever someone calls <see cref="Refresh"/> — no MonoBehaviour/scene wiring needed.
    ///
    /// Restores after QuestManager (<see cref="IRestoreOrder"/>) because a rollover mutates QuestManager's state.
    /// </summary>
    public class DailyQuestService : IManager, ISaveable, IRestoreOrder
    {
        private const string LogCategory = "Quests";

        public const string ResourcesFolder = "Quests/Daily";
        public const int DefaultDailyCount = 3;

        /// <summary>After every default-order saveable — specifically after QuestManager ("Quests").</summary>
        public const int RestoreOrderAfterQuests = 100;

        private const string DayKeyFormat = "yyyy-MM-dd";

        private readonly QuestManager _quests;
        private readonly IDailyClock _clock;
        private readonly Func<IEnumerable<QuestDefinition>> _poolProvider;
        private readonly int _dailyCount;

        private readonly List<QuestDefinition> _pool = new List<QuestDefinition>();
        private readonly Dictionary<string, QuestDefinition> _poolById = new Dictionary<string, QuestDefinition>();
        private readonly List<string> _todayQuestIds = new List<string>();
        private string _dayKey;

        /// <param name="quests">Quest manager the dailies are started on.</param>
        /// <param name="clock">Null = <see cref="SystemDailyClock"/>.</param>
        /// <param name="poolProvider">Null = <c>Resources.LoadAll&lt;QuestDefinition&gt;("Quests/Daily")</c>. Non-Daily entries are ignored.</param>
        /// <param name="dailyCount">How many dailies per day (clamped to the pool size).</param>
        public DailyQuestService(
            QuestManager quests,
            IDailyClock clock = null,
            Func<IEnumerable<QuestDefinition>> poolProvider = null,
            int dailyCount = DefaultDailyCount)
        {
            _quests = quests;
            _clock = clock ?? SystemDailyClock.Instance;
            _poolProvider = poolProvider ?? LoadPoolFromResources;
            _dailyCount = Math.Max(0, dailyCount);
        }

        public string SaveKey => "DailyQuests";

        public int RestoreOrder => RestoreOrderAfterQuests;

        public int DailyCount => _dailyCount;

        /// <summary>Day ("yyyy-MM-dd") of the current rotation, or null before the first <see cref="Refresh"/>.</summary>
        public string CurrentDayKey => _dayKey;

        public IReadOnlyList<string> TodayQuestIds => _todayQuestIds;

        /// <summary>Every Daily quest that can be picked, sorted by id.</summary>
        public IReadOnlyList<QuestDefinition> Pool => _pool;

        public bool IsDailyQuest(string questId) => !string.IsNullOrEmpty(questId) && _poolById.ContainsKey(questId);

        public void Initialize()
        {
            LoadPool();

            if (ServiceLocator.TryGet(out LocalizationManager localization))
            {
                DailyQuestTexts.EnsureLoaded(localization);
            }

            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
            EventBus.Subscribe<TimeOfDayHourChangedEvent>(OnHourChanged);

            Refresh();

            GameLogger.Info(LogCategory, $"DailyQuestService initialized: {_pool.Count} dailies in pool, day {_dayKey}.");
        }

        public void Shutdown()
        {
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
            EventBus.Unsubscribe<TimeOfDayHourChangedEvent>(OnHourChanged);

            _pool.Clear();
            _poolById.Clear();
            _todayQuestIds.Clear();
            _dayKey = null;
        }

        // ------------------------------------------------------------------
        // Rotation
        // ------------------------------------------------------------------

        /// <summary>
        /// Rolls the rotation over if the clock's local date is later than the current rotation's
        /// day; otherwise just makes sure today's dailies are running (e.g. after loading a save
        /// written before dailies existed). Returns true when a new day's set was picked.
        /// </summary>
        public bool Refresh()
        {
            if (_quests == null)
            {
                return false;
            }

            string today = ToDayKey(_clock.Now);

            if (_dayKey != null && string.CompareOrdinal(today, _dayKey) <= 0)
            {
                if (string.CompareOrdinal(today, _dayKey) < 0)
                {
                    GameLogger.Debug(LogCategory, $"System clock is behind the daily rotation ({today} < {_dayKey}); keeping the current dailies.");
                }

                EnsureTodayStarted();
                return false;
            }

            Rollover(today);
            return true;
        }

        private void Rollover(string today)
        {
            // Forget every daily, not just yesterday's picks: covers quests dropped from the pool
            // and saves whose daily slice is missing/out of sync with the quest slice.
            foreach (string questId in _poolById.Keys.Concat(_todayQuestIds).Distinct().ToList())
            {
                if (_quests.IsActive(questId))
                {
                    _quests.AbandonQuest(questId);
                }

                _quests.ResetDailyQuest(questId);
            }

            _dayKey = today;
            _todayQuestIds.Clear();
            _todayQuestIds.AddRange(PickForDay(today, _pool.Select(d => d.QuestId).ToList(), _dailyCount));

            EnsureTodayStarted();

            GameLogger.Info(LogCategory, $"Daily quests for {today}: {string.Join(", ", _todayQuestIds)}.");
            EventBus.Publish(new DailyQuestsRefreshedEvent(_dayKey, _todayQuestIds.ToList()));
        }

        private void EnsureTodayStarted()
        {
            foreach (string questId in _todayQuestIds)
            {
                if (_quests.IsActive(questId) || _quests.IsCompleted(questId))
                {
                    continue;
                }

                if (_poolById.TryGetValue(questId, out QuestDefinition definition))
                {
                    _quests.StartQuest(definition);
                }
            }
        }

        /// <summary>
        /// Deterministic pick of <paramref name="count"/> distinct ids for a day: the ids are sorted
        /// ordinally, then Fisher-Yates-shuffled by a small integer PRNG seeded with yyyymmdd (not
        /// System.Random, whose sequence isn't guaranteed across runtimes).
        /// </summary>
        public static List<string> PickForDay(string dayKey, IReadOnlyList<string> questIds, int count)
        {
            var ids = (questIds ?? Array.Empty<string>())
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();

            uint state = Hash((uint)DaySeed(dayKey));

            for (int i = ids.Count - 1; i > 0; i--)
            {
                state = state * 1664525u + 1013904223u;
                int j = (int)(Hash(state) % (uint)(i + 1));
                (ids[i], ids[j]) = (ids[j], ids[i]);
            }

            return ids.Take(Math.Max(0, Math.Min(count, ids.Count))).ToList();
        }

        public static string ToDayKey(DateTime localTime)
        {
            return localTime.Date.ToString(DayKeyFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>"2026-10-21" → 20261021 (0 if unparseable).</summary>
        public static int DaySeed(string dayKey)
        {
            return DateTime.TryParseExact(dayKey, DayKeyFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime day)
                ? day.Year * 10000 + day.Month * 100 + day.Day
                : 0;
        }

        private static uint Hash(uint x)
        {
            x ^= x >> 16;
            x *= 0x7feb352du;
            x ^= x >> 15;
            x *= 0x846ca68bu;
            x ^= x >> 16;
            return x;
        }

        private void LoadPool()
        {
            _pool.Clear();
            _poolById.Clear();

            IEnumerable<QuestDefinition> source = _poolProvider() ?? Enumerable.Empty<QuestDefinition>();

            foreach (QuestDefinition definition in source.OrderBy(d => d != null ? d.QuestId : null, StringComparer.Ordinal))
            {
                if (definition == null || string.IsNullOrEmpty(definition.QuestId) || definition.QuestType != QuestType.Daily)
                {
                    continue;
                }

                if (_poolById.ContainsKey(definition.QuestId))
                {
                    continue;
                }

                _pool.Add(definition);
                _poolById[definition.QuestId] = definition;

                // RestoreState in QuestManager doesn't re-register definitions — doing it here means a
                // restored daily still grants its rewards and ResetDailyQuest recognizes it as Daily.
                _quests?.RegisterDefinition(definition);
            }
        }

        private static IEnumerable<QuestDefinition> LoadPoolFromResources()
        {
            return Resources.LoadAll<QuestDefinition>(ResourcesFolder);
        }

        private void OnGameLoaded(GameLoadedEvent evt) => Refresh();

        private void OnHourChanged(TimeOfDayHourChangedEvent evt) => Refresh();

        // ------------------------------------------------------------------
        // Persistence
        // ------------------------------------------------------------------

        public object CaptureState()
        {
            return new DailyQuestSaveState
            {
                DayKey = _dayKey,
                TodayQuestIds = _todayQuestIds.ToList()
            };
        }

        public void RestoreState(object state)
        {
            if (state is not DailyQuestSaveState saveState)
            {
                return;
            }

            _dayKey = DaySeed(saveState.DayKey) != 0 ? saveState.DayKey : null;
            _todayQuestIds.Clear();

            if (_dayKey != null && saveState.TodayQuestIds != null)
            {
                _todayQuestIds.AddRange(saveState.TodayQuestIds.Where(id => _poolById.ContainsKey(id)).Distinct());
            }

            // QuestManager restored first (RestoreOrder): roll over now if the save is from an
            // earlier day, otherwise resume today's set.
            Refresh();
        }
    }
}
