using System;
using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.World.Map;
using UnityEngine;

namespace Rapadura.Audio
{
    /// <summary>Short vibration shapes. Timings live in <see cref="HapticsService.GetTimings"/>.</summary>
    public enum HapticPattern
    {
        /// <summary>Critical hit dealt — a quick tick.</summary>
        Light = 0,
        /// <summary>Player took damage.</summary>
        Medium = 1,
        /// <summary>Player died / boss down.</summary>
        Heavy = 2,
        /// <summary>Level up / victory — three pulses.</summary>
        Success = 3,
    }

    /// <summary>Platform vibration output; swapped for a recorder in tests.</summary>
    public interface IHapticsBackend
    {
        bool IsSupported { get; }

        /// <summary>Android-style timings in ms: [delay, on, off, on, ...].</summary>
        void Play(long[] timingsMs);
    }

    /// <summary>
    /// Android: <c>Vibrator.vibrate(long[] pattern, -1)</c> through JNI for short, shaped pulses,
    /// falling back to <see cref="Handheld.Vibrate"/>. iOS: <see cref="Handheld.Vibrate"/> only exposes
    /// one long (~0.4 s) buzz, so only patterns with a total "on" time ≥ 80 ms are forwarded there —
    /// crit ticks would feel like a phone call. Editor/desktop: no-op.
    /// </summary>
    public sealed class MobileHapticsBackend : IHapticsBackend
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        private AndroidJavaObject _vibrator;
        private bool _resolved;
#endif

        public bool IsSupported => Application.isMobilePlatform;

        public void Play(long[] timingsMs)
        {
            if (timingsMs == null || timingsMs.Length == 0 || !IsSupported)
            {
                return;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                if (!_resolved)
                {
                    _resolved = true;
                    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        _vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                    }
                }

                if (_vibrator != null)
                {
                    _vibrator.Call("vibrate", timingsMs, -1);
                    return;
                }
            }
            catch (Exception e)
            {
                GameLogger.Warning("Haptics", $"Android vibrator unavailable, using Handheld.Vibrate: {e.Message}");
                _vibrator = null;
            }

            Handheld.Vibrate();
#elif UNITY_IOS && !UNITY_EDITOR
            long on = 0;
            for (int i = 1; i < timingsMs.Length; i += 2) on += timingsMs[i];
            if (on >= 80)
            {
                Handheld.Vibrate();
            }
#endif
        }
    }

    /// <summary>
    /// Haptic feedback for the touch build ("Vibração" in Settings > Accessibility): crit dealt,
    /// damage taken, level up, death, encounter victory. Listens to the EventBus like
    /// <see cref="SfxEventRouter"/>; the on/off choice is per device (PlayerPrefs "Haptics.Enabled",
    /// same mechanism as AudioManager volumes / AccessibilitySettings). Each pattern has a minimum
    /// interval so a multi-hit combo produces one buzz, not a continuous rattle.
    /// </summary>
    public class HapticsService : IManager
    {
        public const string EnabledKey = "Haptics.Enabled";
        private const bool DefaultEnabled = true;

        private static readonly Dictionary<HapticPattern, long[]> Timings = new Dictionary<HapticPattern, long[]>
        {
            { HapticPattern.Light, new long[] { 0, 20 } },
            { HapticPattern.Medium, new long[] { 0, 45 } },
            { HapticPattern.Heavy, new long[] { 0, 160 } },
            { HapticPattern.Success, new long[] { 0, 35, 70, 35, 70, 90 } },
        };

        private static readonly Dictionary<HapticPattern, float> MinInterval = new Dictionary<HapticPattern, float>
        {
            { HapticPattern.Light, 0.15f },
            { HapticPattern.Medium, 0.3f },
            { HapticPattern.Heavy, 0.5f },
            { HapticPattern.Success, 0.8f },
        };

        private readonly IHapticsBackend _backend;
        private readonly Func<float> _clock;
        private readonly Dictionary<HapticPattern, float> _lastPlayed = new Dictionary<HapticPattern, float>();
        private readonly LevelUpDetector _levelUp = new LevelUpDetector();

        public bool IsInitialized { get; private set; }

        public bool Enabled { get; private set; } = DefaultEnabled;

        /// <summary>False on desktop/editor — the settings row stays visible but is informational there.</summary>
        public bool IsSupported => _backend != null && _backend.IsSupported;

        public HapticPattern? LastPlayed { get; private set; }

        public int PlayedCount { get; private set; }

        public event Action SettingsChanged;

        public HapticsService(IHapticsBackend backend = null, Func<float> clock = null)
        {
            _backend = backend ?? new MobileHapticsBackend();
            _clock = clock ?? (() => Time.unscaledTime);
        }

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            Enabled = PlayerPrefs.GetInt(EnabledKey, DefaultEnabled ? 1 : 0) != 0;

            EventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
            EventBus.Subscribe<PlayerExperienceChangedEvent>(OnExperienceChanged);
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Subscribe<EncounterEndedEvent>(OnEncounterEnded);
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
            IsInitialized = true;
        }

        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }

            EventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
            EventBus.Unsubscribe<PlayerExperienceChangedEvent>(OnExperienceChanged);
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Unsubscribe<EncounterEndedEvent>(OnEncounterEnded);
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
            IsInitialized = false;
        }

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            PlayerPrefs.SetInt(EnabledKey, enabled ? 1 : 0);
            SettingsChanged?.Invoke();
        }

        public static long[] GetTimings(HapticPattern pattern) => Timings.TryGetValue(pattern, out long[] t) ? t : Timings[HapticPattern.Light];

        /// <summary>Vibrates unless disabled or still inside the pattern's minimum interval. True when sent to the backend.</summary>
        public bool Play(HapticPattern pattern)
        {
            if (!Enabled || _backend == null)
            {
                return false;
            }

            float now = _clock();
            if (_lastPlayed.TryGetValue(pattern, out float last) && now - last < MinInterval[pattern])
            {
                return false;
            }

            _lastPlayed[pattern] = now;
            _backend.Play(GetTimings(pattern));
            LastPlayed = pattern;
            PlayedCount++;
            return true;
        }

        private void OnDamageApplied(DamageAppliedEvent evt)
        {
            if (AudioEventHelpers.IsPlayer(evt.Target))
            {
                Play(HapticPattern.Medium);
            }
            else if (evt.IsCritical && AudioEventHelpers.IsPlayer(evt.Source))
            {
                Play(HapticPattern.Light);
            }
        }

        private void OnExperienceChanged(PlayerExperienceChangedEvent evt)
        {
            if (_levelUp.Observe(evt.Level))
            {
                Play(HapticPattern.Success);
            }
        }

        private void OnPlayerDied(PlayerDiedEvent evt) => Play(HapticPattern.Heavy);

        private void OnEncounterEnded(EncounterEndedEvent evt)
        {
            if (evt.Outcome == EncounterOutcome.Victory)
            {
                Play(HapticPattern.Success);
            }
        }

        private void OnGameLoaded(GameLoadedEvent evt) => _levelUp.Reset();
    }
}
