using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map;
using UnityEngine;

namespace Rapadura.Gameplay.EventRooms
{
    /// <summary>One room in <see cref="EventRoomSaveState"/>.</summary>
    [Serializable]
    public class EventRoomRecord
    {
        public string roomId;
        public int bestScore;
        public int bestTier;
        public int runs;
        public int clears;

        /// <summary>Window index the hero last entered (one entry per opening); -1 = never.</summary>
        public long lastEnteredWindow = -1;
    }

    /// <summary>The schedule a room follows in this save (the defaults offline; online the server's).</summary>
    [Serializable]
    public class EventRoomScheduleEntry
    {
        public string roomId;
        public EventRoomSchedule schedule;
    }

    /// <summary>JSON-serializable snapshot of <see cref="EventRoomProgress"/>.</summary>
    [Serializable]
    public class EventRoomSaveState
    {
        public List<EventRoomRecord> rooms = new List<EventRoomRecord>();
        public List<EventRoomScheduleEntry> schedules = new List<EventRoomScheduleEntry>();
    }

    /// <summary>What <see cref="EventRoomProgress.RecordRun"/> changed.</summary>
    public readonly struct EventRoomRunOutcome
    {
        public readonly EventRoomKind Kind;
        public readonly int Score;
        public readonly EventRoomTier Tier;
        public readonly bool Cleared;
        public readonly bool NewBest;
        public readonly bool FirstClear;
        public readonly int BestScore;

        public EventRoomRunOutcome(EventRoomKind kind, int score, EventRoomTier tier, bool cleared, bool newBest, bool firstClear, int bestScore)
        {
            Kind = kind;
            Score = score;
            Tier = tier;
            Cleared = cleared;
            NewBest = newBest;
            FirstClear = firstClear;
            BestScore = bestScore;
        }
    }

    /// <summary>Raised after every finished room run (results recorded, rewards granted by the director).</summary>
    public readonly struct EventRoomFinishedEvent : IGameEvent
    {
        public readonly EventRoomRunOutcome Outcome;

        public EventRoomFinishedEvent(EventRoomRunOutcome outcome)
        {
            Outcome = outcome;
        }
    }

    /// <summary>
    /// Per-save state of the timed event rooms ("Terreiro das Assombrações", "Torre do Engenho Velho"): personal bests,
    /// clears, the window each room was last entered in (one entry per opening) and the schedule in force. Plain C#
    /// (EditMode-testable), built by GameManager, registered with SaveManager under <see cref="Key"/> and with
    /// NewSessionReset; a save from before the rooms has no entry and loads with the default schedule and no records.
    /// <list type="bullet">
    /// <item>Achievements (per profile, re-fed after each load): <see cref="AchievementConditionType.EventRoomCleared"/>
    /// (distinct room ids) and <see cref="AchievementConditionType.EventRoomBestScore"/> (max).</item>
    /// <item>The entry items' rare drop: every enemy killed outside a room may drop a Convite / a Chave
    /// (<see cref="EventRoomRules.RollEntryDrop"/>), straight into the hero's bag through the shared fit rules.</item>
    /// </list>
    /// </summary>
    public class EventRoomProgress : IManager, ISaveable, IResettable
    {
        public const string Key = "EventRooms";
        private const string PlayerTag = "Player";

        private readonly Dictionary<EventRoomKind, EventRoomRecord> _records = new Dictionary<EventRoomKind, EventRoomRecord>();
        private readonly Dictionary<EventRoomKind, EventRoomSchedule> _schedules = new Dictionary<EventRoomKind, EventRoomSchedule>();
        private bool _subscribed;
        private static EventRoomProgress _standalone;

        public string SaveKey => Key;

        /// <summary>Explicit achievement sink (tests); null = the registered one.</summary>
        public AchievementManager Achievements { get; set; }

        /// <summary>Randomness of the entry-item drop (tests inject a seeded one).</summary>
        public System.Random Random { get; set; } = new System.Random();

        /// <summary>Turns the entry-item drop off (tests that count loot).</summary>
        public static bool EntryDropsEnabled { get; set; } = true;

        public static readonly EventRoomKind[] Rooms = { EventRoomKind.Terreiro, EventRoomKind.Torre };

        public EventRoomProgress()
        {
            ResetForNewSession();
        }

        public static EventRoomProgress ResolveShared()
        {
            if (ServiceLocator.TryGet(out EventRoomProgress progress))
            {
                return progress;
            }

            if (_standalone == null)
            {
                _standalone = new EventRoomProgress();
            }

            return _standalone;
        }

        public void Initialize()
        {
            if (ServiceLocator.TryGet(out LocalizationManager localization))
            {
                EventRoomTexts.EnsureRegistered(localization);
            }

            if (_subscribed)
            {
                return;
            }

            _subscribed = true;
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
            EventBus.Subscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
        }

        public void Shutdown()
        {
            if (!_subscribed)
            {
                return;
            }

            _subscribed = false;
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
            EventBus.Unsubscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
        }

        public void ResetForNewSession()
        {
            _records.Clear();
            _schedules.Clear();
            foreach (EventRoomKind kind in Rooms)
            {
                _records[kind] = new EventRoomRecord { roomId = EventRoomRules.RoomId(kind) };
                _schedules[kind] = EventRoomRules.DefaultSchedule(kind);
            }
        }

        // ------------------------------------------------------------------
        // Schedule
        // ------------------------------------------------------------------

        public EventRoomSchedule GetSchedule(EventRoomKind kind)
        {
            return _schedules.TryGetValue(kind, out EventRoomSchedule schedule) ? schedule : EventRoomRules.DefaultSchedule(kind);
        }

        /// <summary>Replaces a room's schedule (online: pushed by the server). Invalid schedules are ignored.</summary>
        public bool SetSchedule(EventRoomKind kind, EventRoomSchedule schedule)
        {
            if (!_records.ContainsKey(kind) || !schedule.IsValid)
            {
                return false;
            }

            _schedules[kind] = schedule;
            return true;
        }

        public EventRoomWindow WindowAt(EventRoomKind kind, int day, float hourOfDay) => EventRoomRules.WindowAt(GetSchedule(kind), day, hourOfDay);

        /// <summary>The window on the live clock (day 1, 8h without a TimeOfDayManager).</summary>
        public EventRoomWindow WindowNow(EventRoomKind kind)
        {
            if (ServiceLocator.TryGet(out TimeOfDayManager time) && time != null)
            {
                return WindowAt(kind, time.Day, time.CurrentHour);
            }

            return WindowAt(kind, 1, TimeOfDayManager.DefaultStartHour);
        }

        public long LastEnteredWindow(EventRoomKind kind) => Record(kind)?.lastEnteredWindow ?? -1;

        public bool CanEnter(EventRoomKind kind, EventRoomWindow window) => Record(kind) != null && EventRoomRules.CanEnterWindow(window, LastEnteredWindow(kind));

        /// <summary>Spends this opening of the room (called when the entry item is consumed).</summary>
        public void MarkEntered(EventRoomKind kind, long windowIndex)
        {
            EventRoomRecord record = Record(kind);
            if (record != null)
            {
                record.lastEnteredWindow = windowIndex;
            }
        }

        // ------------------------------------------------------------------
        // Records
        // ------------------------------------------------------------------

        public int BestScore(EventRoomKind kind) => Record(kind)?.bestScore ?? 0;

        public EventRoomTier BestTier(EventRoomKind kind) => (EventRoomTier)(Record(kind)?.bestTier ?? 0);

        public int Clears(EventRoomKind kind) => Record(kind)?.clears ?? 0;

        public int Runs(EventRoomKind kind) => Record(kind)?.runs ?? 0;

        /// <summary>Records a finished run: runs/clears, personal best, achievements. Publishes <see cref="EventRoomFinishedEvent"/>.</summary>
        public EventRoomRunOutcome RecordRun(EventRoomKind kind, int score, EventRoomTier tier, bool cleared)
        {
            EventRoomRecord record = Record(kind);
            if (record == null)
            {
                return default;
            }

            score = Math.Max(0, score);
            record.runs++;
            bool firstClear = cleared && record.clears == 0;
            if (cleared)
            {
                record.clears++;
            }

            bool newBest = score > record.bestScore;
            if (newBest)
            {
                record.bestScore = score;
            }

            record.bestTier = Math.Max(record.bestTier, (int)tier);
            FeedAchievements(kind, cleared, score);

            var outcome = new EventRoomRunOutcome(kind, score, tier, cleared, newBest, firstClear, record.bestScore);
            EventBus.Publish(new EventRoomFinishedEvent(outcome));
            return outcome;
        }

        private EventRoomRecord Record(EventRoomKind kind) => _records.TryGetValue(kind, out EventRoomRecord record) ? record : null;

        private AchievementManager ResolveAchievements()
        {
            if (Achievements != null)
            {
                return Achievements;
            }

            return ServiceLocator.TryGet(out AchievementManager achievements) ? achievements : null;
        }

        private void FeedAchievements(EventRoomKind kind, bool cleared, int score)
        {
            AchievementManager achievements = ResolveAchievements();
            if (achievements == null)
            {
                return;
            }

            if (cleared)
            {
                achievements.RecordProgress(AchievementConditionType.EventRoomCleared, 1, EventRoomRules.RoomId(kind), distinct: true);
            }

            achievements.RecordMax(AchievementConditionType.EventRoomBestScore, score);
        }

        private void OnGameLoaded(GameLoadedEvent _)
        {
            foreach (EventRoomKind kind in Rooms)
            {
                FeedAchievements(kind, Clears(kind) > 0, BestScore(kind));
            }
        }

        // ------------------------------------------------------------------
        // Entry-item drop
        // ------------------------------------------------------------------

        private void OnCombatTargetDied(CombatTargetDiedEvent evt)
        {
            if (!EntryDropsEnabled || evt.Target == null || !Application.isPlaying)
            {
                return;
            }

            EnemyController enemy = evt.Target.GetComponent<EnemyController>();
            if (enemy == null || enemy.Definition == null)
            {
                return;
            }

            bool insideRoom = ServiceLocator.TryGet(out SceneTransitionService transition) && transition != null
                && transition.PendingEncounter != null && transition.PendingEncounter.IsEventRoom;
            GameObject player = evt.Killer != null && evt.Killer.GetComponentInParent<PlayerStats>() != null
                ? evt.Killer.GetComponentInParent<PlayerStats>().gameObject
                : GameObject.FindGameObjectWithTag(PlayerTag);
            if (player == null || !player.TryGetComponent(out InventoryManager inventory) || !player.TryGetComponent(out PlayerStats stats))
            {
                return;
            }

            EventRoomKind drop = EventRoomRules.RollEntryDrop(enemy.IsBoss, insideRoom, stats.Level, Random.NextDouble(), Random.NextDouble());
            if (drop == EventRoomKind.None)
            {
                return;
            }

            ItemDefinition item = ItemDatabase.GetById(EventRoomRules.EntryItemId(drop));
            if (item == null)
            {
                return;
            }

            LootGrantResult result = LootGrantRules.Grant(inventory, new LootDrop(item, 1), null);
            if (result.Granted > 0)
            {
                CombatFxManager.SpawnLootText(evt.Target.transform.position + Vector3.up * 2.3f, "+ " + item.LocalizedName());
            }
        }

        // ------------------------------------------------------------------
        // Save
        // ------------------------------------------------------------------

        public object CaptureState()
        {
            var state = new EventRoomSaveState();
            foreach (EventRoomKind kind in Rooms)
            {
                EventRoomRecord record = Record(kind);
                state.rooms.Add(new EventRoomRecord
                {
                    roomId = EventRoomRules.RoomId(kind),
                    bestScore = record.bestScore,
                    bestTier = record.bestTier,
                    runs = record.runs,
                    clears = record.clears,
                    lastEnteredWindow = record.lastEnteredWindow,
                });
                state.schedules.Add(new EventRoomScheduleEntry { roomId = EventRoomRules.RoomId(kind), schedule = GetSchedule(kind) });
            }

            return state;
        }

        public void RestoreState(object state)
        {
            if (state is not EventRoomSaveState saved)
            {
                return;
            }

            ResetForNewSession();
            if (saved.rooms != null)
            {
                foreach (EventRoomRecord entry in saved.rooms)
                {
                    EventRoomKind kind = entry != null ? EventRoomRules.ParseRoomId(entry.roomId) : EventRoomKind.None;
                    EventRoomRecord record = Record(kind);
                    if (record == null)
                    {
                        continue;
                    }

                    record.bestScore = Math.Max(0, entry.bestScore);
                    record.bestTier = Mathf.Clamp(entry.bestTier, 0, (int)EventRoomTier.Mestre);
                    record.runs = Math.Max(0, entry.runs);
                    record.clears = Mathf.Clamp(entry.clears, 0, record.runs > 0 ? record.runs : int.MaxValue);
                    record.lastEnteredWindow = Math.Max(-1, entry.lastEnteredWindow);
                }
            }

            if (saved.schedules != null)
            {
                foreach (EventRoomScheduleEntry entry in saved.schedules)
                {
                    if (entry != null)
                    {
                        SetSchedule(EventRoomRules.ParseRoomId(entry.roomId), entry.schedule);
                    }
                }
            }
        }
    }
}
