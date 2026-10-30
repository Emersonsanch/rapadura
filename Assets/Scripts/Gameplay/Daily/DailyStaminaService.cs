using System;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Logging;
using UnityEngine;

namespace Rapadura.Gameplay.Daily
{
    /// <summary>Raised by <see cref="DailyStaminaService"/> when the fôlego moves to another tier.</summary>
    public readonly struct StaminaTierChangedEvent : IGameEvent
    {
        public readonly StaminaTier Previous;
        public readonly StaminaTier Current;

        public StaminaTierChangedEvent(StaminaTier previous, StaminaTier current)
        {
            Previous = previous;
            Current = current;
        }
    }

    public enum StaminaReminderKind
    {
        /// <summary>"Hora de descansar, a Vó Edna chamou para o chá" (tired tier).</summary>
        Tired,

        /// <summary>The parental "Lembrete de tempo de jogo" (every N minutes, Configurações → Acessibilidade).</summary>
        PlayTime,
    }

    /// <summary>Raised by <see cref="DailyStaminaService"/> when a gentle reminder is due; the HUD shows it as a toast.</summary>
    public readonly struct StaminaReminderEvent : IGameEvent
    {
        public readonly StaminaReminderKind Kind;

        /// <summary>Whole minutes of active play this session (for the play-time text).</summary>
        public readonly int SessionMinutes;

        public StaminaReminderEvent(StaminaReminderKind kind, int sessionMinutes)
        {
            Kind = kind;
            SessionMinutes = sessionMinutes;
        }
    }

    /// <summary>Where the fôlego is kept between runs. Runtime default: PlayerPrefs (per device, like the settings).</summary>
    public interface IDailyStaminaStore
    {
        string LoadJson();

        void SaveJson(string json);

        int LoadReminderMinutes();

        void SaveReminderMinutes(int minutes);
    }

    /// <summary>PlayerPrefs-backed <see cref="IDailyStaminaStore"/>.</summary>
    public sealed class PlayerPrefsDailyStaminaStore : IDailyStaminaStore
    {
        public const string StateKey = "Daily.Folego.State";
        public const string ReminderKey = "Parental.PlayTimeReminderMinutes";

        public string LoadJson() => PlayerPrefs.GetString(StateKey, string.Empty);

        public void SaveJson(string json)
        {
            PlayerPrefs.SetString(StateKey, json ?? string.Empty);
            PlayerPrefs.Save();
        }

        public int LoadReminderMinutes() => PlayerPrefs.GetInt(ReminderKey, 0);

        public void SaveReminderMinutes(int minutes)
        {
            PlayerPrefs.SetInt(ReminderKey, minutes);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// "Fôlego diário": plain-C# <see cref="IManager"/> registered by GameManager. Holds the
    /// <see cref="DailyStaminaState"/> (rules: <see cref="DailyStaminaRules"/>), fed every frame by
    /// <c>DailyStaminaDriver</c> (input/pause/scene → <see cref="StaminaTickKind"/>), and answers the two choke points:
    /// kill XP in <c>EnemyRewardGranter</c> (<see cref="ExperienceMultiplier"/>: the tier at the campaign pace,
    /// <see cref="DailyStaminaRules.KillXpPace"/>) and stackable loot in <c>EnemyLootGranter</c> (<see cref="Multiplier"/>,
    /// <c>StaminaLootRules</c>). Quest, event and boss-bonus rewards are not touched.
    ///
    /// Per device (PlayerPrefs via <see cref="IDailyStaminaStore"/>), deliberately not in a save slot nor reset by
    /// "Novo jogo": the fôlego is about the child's play time. Saved every <see cref="SaveIntervalSeconds"/>, on tier
    /// change, on pause/quit. The parental play-time reminder interval lives here too
    /// (<see cref="PlayTimeReminderMinutes"/>, 0 = off). Online (Fase 12) the server owns the fôlego.
    /// </summary>
    public class DailyStaminaService : IManager
    {
        private const string LogCategory = "Daily";
        public const double SaveIntervalSeconds = 30;

        private readonly IDailyStaminaStore _store;
        private readonly Func<long> _utcNow;

        private DailyStaminaState _state = new DailyStaminaState();
        private StaminaTier _tier = StaminaTier.Full;
        private long _lastWallTicks;
        private double _sinceSave;
        private double _sessionActiveSeconds;
        private double _lastPlayTimeReminderAt;
        private double _activeSinceTiredReminder;
        private int _reminderMinutes;

        /// <param name="store">Null = PlayerPrefs.</param>
        /// <param name="utcNow">Null = <see cref="DateTime.UtcNow"/> ticks.</param>
        public DailyStaminaService(IDailyStaminaStore store = null, Func<long> utcNow = null)
        {
            _store = store ?? new PlayerPrefsDailyStaminaStore();
            _utcNow = utcNow ?? (() => DateTime.UtcNow.Ticks);
        }

        public bool IsInitialized { get; private set; }

        /// <summary>A copy of the current state (tests, debug).</summary>
        public DailyStaminaState Snapshot => _state.Clone();

        public StaminaTier Tier => _tier;

        /// <summary>×1.3 / ×1 / ×0.5 for the current tier.</summary>
        public float Multiplier => DailyStaminaRules.MultiplierOf(_tier);

        public float Fill => DailyStaminaRules.Fill(_state);

        public double SecondsToNextTier => DailyStaminaRules.SecondsToNextTier(_state);

        public double SessionActiveSeconds => _sessionActiveSeconds;

        /// <summary>Parental "Lembrete de tempo de jogo", minutes (0 = off).</summary>
        public int PlayTimeReminderMinutes => _reminderMinutes;

        /// <summary>Kill-XP multiplier: the tier × <see cref="DailyStaminaRules.KillXpPace"/>.</summary>
        public float ExperienceMultiplier => DailyStaminaRules.ExperienceMultiplierOf(_tier);

        /// <summary>The live service's loot multiplier (×1.3 / ×1 / ×0.5), or ×1 outside a game session (tests, tools).</summary>
        public static float ResolveMultiplier()
        {
            return ServiceLocator.TryGet(out DailyStaminaService service) && service != null && service.IsInitialized ? service.Multiplier : 1f;
        }

        /// <summary>The live service's kill-XP multiplier (×1 / ≈×0.77 / ≈×0.38), or ×1 outside a game session.</summary>
        public static float ResolveExperienceMultiplier()
        {
            return ServiceLocator.TryGet(out DailyStaminaService service) && service != null && service.IsInitialized ? service.ExperienceMultiplier : 1f;
        }

        public void Initialize()
        {
            _state = Load() ?? new DailyStaminaState();
            _reminderMinutes = DailyStaminaRules.SanitizeReminderMinutes(_store.LoadReminderMinutes());
            ApplyOffline(_utcNow());
            _tier = DailyStaminaRules.TierOf(_state);
            IsInitialized = true;
            Save();
            GameLogger.Info(LogCategory, $"Fôlego: {_tier}, {_state.FullLeftSeconds / 60:0} min cheio + {_state.NormalLeftSeconds / 60:0} min normal.");
        }

        public void Shutdown()
        {
            if (IsInitialized)
            {
                Save();
            }

            IsInitialized = false;
        }

        public void SetPlayTimeReminderMinutes(int minutes)
        {
            _reminderMinutes = DailyStaminaRules.SanitizeReminderMinutes(minutes);
            _store.SaveReminderMinutes(_reminderMinutes);
            _lastPlayTimeReminderAt = _sessionActiveSeconds;
        }

        /// <summary>
        /// One frame: <paramref name="realDeltaSeconds"/> of monotonic time (unscaled, clamped by the driver) counted as
        /// <paramref name="kind"/>. Also watches the wall clock for date jumps.
        /// </summary>
        public void Tick(double realDeltaSeconds, StaminaTickKind kind)
        {
            if (!IsInitialized || realDeltaSeconds <= 0)
            {
                return;
            }

            long wallNow = _utcNow();
            if (_lastWallTicks > 0)
            {
                double jump = DailyStaminaRules.DetectClockJump(realDeltaSeconds, DailyStaminaRules.TicksToSeconds(wallNow - _lastWallTicks));
                if (jump > 0)
                {
                    _state.ClockSkewSeconds = DailyStaminaRules.AccumulateSkew(_state.ClockSkewSeconds, jump);
                    GameLogger.Warning(LogCategory, $"Clock jumped {jump:0} s ahead while playing; not counted as rest.");
                }
            }

            _lastWallTicks = wallNow;

            if (kind == StaminaTickKind.Active)
            {
                DailyStaminaRules.Play(_state, realDeltaSeconds);
                _sessionActiveSeconds += realDeltaSeconds;
                _activeSinceTiredReminder += realDeltaSeconds;
            }
            else
            {
                DailyStaminaRules.Rest(_state, realDeltaSeconds);
            }

            long adjusted = wallNow - DailyStaminaRules.SecondsToTicks(_state.ClockSkewSeconds);
            _state.LastSeenUtcTicks = Math.Max(_state.LastSeenUtcTicks, adjusted);

            UpdateTier();
            CheckPlayTimeReminder();

            _sinceSave += realDeltaSeconds;
            if (_sinceSave >= SaveIntervalSeconds)
            {
                Save();
            }
        }

        /// <summary>Back from the background / a long frame hitch: credits the offline rest (clock-guarded) and saves.</summary>
        public void ResumeAfterAway()
        {
            if (!IsInitialized)
            {
                return;
            }

            ApplyOffline(_utcNow());
            UpdateTier();
            Save();
        }

        /// <summary>Going to the background or quitting.</summary>
        public void Suspend()
        {
            if (IsInitialized)
            {
                Save();
            }
        }

        private void ApplyOffline(long wallNow)
        {
            OfflineRestResult offline = DailyStaminaRules.ResolveOffline(_state.LastSeenUtcTicks, _state.ClockSkewSeconds, wallNow);
            DailyStaminaRules.Rest(_state, offline.RestSeconds);
            _state.LastSeenUtcTicks = offline.NewLastSeenUtcTicks;
            _state.ClockSkewSeconds = offline.NewClockSkewSeconds;
            _lastWallTicks = wallNow;
        }

        private void UpdateTier()
        {
            StaminaTier previous = _tier;
            _tier = DailyStaminaRules.TierOf(_state);

            if (DailyStaminaRules.ShouldRemindTired(previous, _tier, _activeSinceTiredReminder))
            {
                _activeSinceTiredReminder = 0;
                EventBus.Publish(new StaminaReminderEvent(StaminaReminderKind.Tired, (int)(_sessionActiveSeconds / 60)));
            }

            if (previous != _tier)
            {
                EventBus.Publish(new StaminaTierChangedEvent(previous, _tier));
                Save();
            }
        }

        private void CheckPlayTimeReminder()
        {
            if (DailyStaminaRules.IsPlayTimeReminderDue(_sessionActiveSeconds, _lastPlayTimeReminderAt, _reminderMinutes))
            {
                _lastPlayTimeReminderAt = _sessionActiveSeconds;
                EventBus.Publish(new StaminaReminderEvent(StaminaReminderKind.PlayTime, (int)(_sessionActiveSeconds / 60)));
            }
        }

        private DailyStaminaState Load()
        {
            string json = _store.LoadJson();
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                DailyStaminaState state = JsonUtility.FromJson<DailyStaminaState>(json);
                if (state != null)
                {
                    // Re-clamp whatever came from disk.
                    state.FullLeftSeconds = Math.Max(0, Math.Min(DailyStaminaRules.FullSeconds, state.FullLeftSeconds));
                    state.NormalLeftSeconds = Math.Max(0, Math.Min(DailyStaminaRules.NormalSeconds, state.NormalLeftSeconds));
                    state.ClockSkewSeconds = Math.Max(0, state.ClockSkewSeconds);
                }

                return state;
            }
            catch (Exception e)
            {
                GameLogger.Warning(LogCategory, $"Fôlego state unreadable, starting full: {e.Message}");
                return null;
            }
        }

        private void Save()
        {
            _sinceSave = 0;
            _store.SaveJson(JsonUtility.ToJson(_state));
        }
    }
}
