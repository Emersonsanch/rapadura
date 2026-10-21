using System;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.UI.Menus;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.Environment
{
    /// <summary>JSON-serializable snapshot of <see cref="TimeOfDayManager"/>.</summary>
    [Serializable]
    public class TimeOfDaySaveState
    {
        public float hour;
        public int day;
    }

    /// <summary>
    /// In-game clock for the day/night cycle. Plain C# <see cref="IManager"/> + <see cref="ISaveable"/>
    /// built by <c>GameManager.BuildManagers()</c> (so it survives WorldMap ⇄ Main scene swaps) and
    /// registered with SaveManager under <see cref="SaveKey"/>.
    ///
    /// Time advances through <see cref="Tick"/> — driven by <see cref="EnvironmentTickRunner"/> in
    /// Play Mode, directly by tests in EditMode — at <see cref="DayLengthSeconds"/> real seconds per
    /// 24 in-game hours, and stops while paused (<see cref="GamePausedEvent"/>/<see cref="GameResumedEvent"/>).
    /// Every phase boundary crossed publishes <see cref="TimeOfDayPhaseChangedEvent"/> (in order, even
    /// if one large tick crosses several), and a whole-hour change publishes
    /// <see cref="TimeOfDayHourChangedEvent"/> for a future HUD clock.
    ///
    /// Gameplay hook: <see cref="EncounterChanceMultiplier"/> (read by RandomEncounterSystem) is
    /// <see cref="NightEncounterMultiplier"/> at night, 1 otherwise.
    /// </summary>
    public class TimeOfDayManager : IManager, ISaveable, IResettable
    {
        public const float HoursPerDay = 24f;
        public const float DawnStartHour = 5f;
        public const float DayStartHour = 7f;
        public const float DuskStartHour = 18f;
        public const float NightStartHour = 20f;

        public const float DefaultDayLengthSeconds = 1200f;
        public const float DefaultStartHour = 8f;
        public const float DefaultNightEncounterMultiplier = 1.25f;

        // Upper bound on boundary hops per Tick (a full day has 4 phase boundaries + midnight).
        private const int MaxBoundaryHopsPerTick = 12;

        private readonly Action<float> _tickAction;
        private float _dayLengthSeconds;
        private readonly float _startHour;
        private bool _isInitialized;

        public string SaveKey => "TimeOfDay";

        /// <summary>Real seconds for one full 24h in-game day. Clamped to at least 1.</summary>
        public float DayLengthSeconds
        {
            get => _dayLengthSeconds;
            set => _dayLengthSeconds = Mathf.Max(1f, value);
        }

        /// <summary>Extra multiplier on clock speed (1 = normal). 0 freezes time without pausing.</summary>
        public float TimeScale { get; set; } = 1f;

        /// <summary>Encounter chance multiplier applied at night (default 1.25 = "slightly" more fights).</summary>
        public float NightEncounterMultiplier { get; set; } = DefaultNightEncounterMultiplier;

        /// <summary>Current in-game hour in [0, 24).</summary>
        public float CurrentHour { get; private set; }

        /// <summary>Days elapsed since the start of the game (starts at 1).</summary>
        public int Day { get; private set; } = 1;

        public TimeOfDayPhase Phase { get; private set; }
        public bool IsPaused { get; private set; }

        /// <summary><see cref="CurrentHour"/> mapped to [0, 1) (0 = midnight, 0.5 = noon).</summary>
        public float NormalizedTime => CurrentHour / HoursPerDay;

        /// <summary>0 at night, 1 during the day, smooth ramps through dawn and dusk. Cheap, allocation-free.</summary>
        public float DaylightFactor => GetDaylightFactor(CurrentHour);

        /// <summary>Multiplier RandomEncounterSystem applies to its per-step chance (1 outside night).</summary>
        public float EncounterChanceMultiplier => Phase == TimeOfDayPhase.Night ? Mathf.Max(0f, NightEncounterMultiplier) : 1f;

        public TimeOfDayManager(float dayLengthSeconds = DefaultDayLengthSeconds, float startHour = DefaultStartHour)
        {
            DayLengthSeconds = dayLengthSeconds;
            _startHour = WrapHour(startHour);
            CurrentHour = _startHour;
            Phase = GetPhase(CurrentHour);
            _tickAction = Tick;
        }

        /// <summary>New game / load: day 1 at the constructor's start hour (publishes the phase/hour change like a restore).</summary>
        public void ResetForNewSession()
        {
            Day = 1;
            CurrentHour = _startHour;
            UpdatePhase();
            EventBus.Publish(new TimeOfDayHourChangedEvent(Mathf.FloorToInt(CurrentHour), Day, Phase));
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;
            EventBus.Subscribe<GamePausedEvent>(OnGamePaused);
            EventBus.Subscribe<GameResumedEvent>(OnGameResumed);
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnvironmentTickRunner.Register(_tickAction);
        }

        public void Shutdown()
        {
            if (!_isInitialized)
            {
                return;
            }

            _isInitialized = false;
            EventBus.Unsubscribe<GamePausedEvent>(OnGamePaused);
            EventBus.Unsubscribe<GameResumedEvent>(OnGameResumed);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            EnvironmentTickRunner.Unregister(_tickAction);
        }

        public void SetPaused(bool paused) => IsPaused = paused;

        /// <summary>Advances the clock by <paramref name="deltaTime"/> real seconds (no-op while paused). Capped at one in-game day per call.</summary>
        public void Tick(float deltaTime)
        {
            if (IsPaused || deltaTime <= 0f || TimeScale <= 0f)
            {
                return;
            }

            float hours = Mathf.Min(HoursPerDay, deltaTime * TimeScale * HoursPerDay / _dayLengthSeconds);
            AdvanceHours(hours);
        }

        /// <summary>Advances the clock by in-game hours (ignores pause), publishing every phase boundary crossed in order.</summary>
        public void AdvanceHours(float hours)
        {
            if (hours <= 0f)
            {
                return;
            }

            int previousWholeHour = Mathf.FloorToInt(CurrentHour);
            int previousDay = Day;
            float remaining = Mathf.Min(hours, HoursPerDay);

            for (int hop = 0; hop < MaxBoundaryHopsPerTick && remaining > 0f; hop++)
            {
                float boundary = GetNextBoundary(CurrentHour);
                float toBoundary = boundary - CurrentHour;

                if (remaining < toBoundary)
                {
                    CurrentHour += remaining;
                    break;
                }

                remaining -= toBoundary;

                if (boundary >= HoursPerDay)
                {
                    CurrentHour = 0f;
                    Day++;
                }
                else
                {
                    CurrentHour = boundary;
                }

                UpdatePhase();
            }

            int wholeHour = Mathf.FloorToInt(CurrentHour);
            if (wholeHour != previousWholeHour || Day != previousDay)
            {
                EventBus.Publish(new TimeOfDayHourChangedEvent(wholeHour, Day, Phase));
            }
        }

        /// <summary>Jumps the clock to <paramref name="hour"/> on the current day (debug/sleep/cutscenes). Publishes a phase change if the phase differs.</summary>
        public void SetTime(float hour)
        {
            int previousWholeHour = Mathf.FloorToInt(CurrentHour);
            CurrentHour = WrapHour(hour);
            UpdatePhase();

            int wholeHour = Mathf.FloorToInt(CurrentHour);
            if (wholeHour != previousWholeHour)
            {
                EventBus.Publish(new TimeOfDayHourChangedEvent(wholeHour, Day, Phase));
            }
        }

        public static TimeOfDayPhase GetPhase(float hour)
        {
            hour = WrapHour(hour);

            if (hour >= NightStartHour || hour < DawnStartHour)
            {
                return TimeOfDayPhase.Night;
            }

            if (hour < DayStartHour)
            {
                return TimeOfDayPhase.Dawn;
            }

            return hour < DuskStartHour ? TimeOfDayPhase.Day : TimeOfDayPhase.Dusk;
        }

        /// <summary>0 at night, 1 by day, linear ramps across dawn/dusk.</summary>
        public static float GetDaylightFactor(float hour)
        {
            hour = WrapHour(hour);

            if (hour < DawnStartHour || hour >= NightStartHour)
            {
                return 0f;
            }

            if (hour < DayStartHour)
            {
                return (hour - DawnStartHour) / (DayStartHour - DawnStartHour);
            }

            if (hour < DuskStartHour)
            {
                return 1f;
            }

            return 1f - (hour - DuskStartHour) / (NightStartHour - DuskStartHour);
        }

        public object CaptureState()
        {
            return new TimeOfDaySaveState { hour = CurrentHour, day = Day };
        }

        public void RestoreState(object state)
        {
            if (!(state is TimeOfDaySaveState saved))
            {
                return;
            }

            Day = Mathf.Max(1, saved.day);
            CurrentHour = WrapHour(saved.hour);
            UpdatePhase();
            EventBus.Publish(new TimeOfDayHourChangedEvent(Mathf.FloorToInt(CurrentHour), Day, Phase));
        }

        private void UpdatePhase()
        {
            TimeOfDayPhase phase = GetPhase(CurrentHour);
            if (phase == Phase)
            {
                return;
            }

            TimeOfDayPhase previous = Phase;
            Phase = phase;
            EventBus.Publish(new TimeOfDayPhaseChangedEvent(previous, phase, Day));
        }

        private static float GetNextBoundary(float hour)
        {
            if (hour < DawnStartHour) return DawnStartHour;
            if (hour < DayStartHour) return DayStartHour;
            if (hour < DuskStartHour) return DuskStartHour;
            if (hour < NightStartHour) return NightStartHour;
            return HoursPerDay;
        }

        private static float WrapHour(float hour)
        {
            if (float.IsNaN(hour) || float.IsInfinity(hour))
            {
                return 0f;
            }

            hour %= HoursPerDay;
            if (hour < 0f)
            {
                hour += HoursPerDay;
            }

            return hour >= HoursPerDay ? 0f : hour;
        }

        private void OnGamePaused(GamePausedEvent _) => IsPaused = true;

        private void OnGameResumed(GameResumedEvent _) => IsPaused = false;

        // Same rule as FrameRateManager: a freshly loaded scene never starts paused (its
        // PauseMenuController is rebuilt), so a "quit to menu" from pause can't freeze the clock.
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
            {
                IsPaused = false;
            }
        }
    }
}
