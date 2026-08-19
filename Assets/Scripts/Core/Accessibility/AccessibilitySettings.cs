using System;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Logging;
using UnityEngine;

namespace Rapadura.Core.Accessibility
{
    /// <summary>
    /// Top-level manager for accessibility preferences: font scale, high contrast, active
    /// colorblind palette, screen-shake/flash reduction multipliers. Follows the same pattern as
    /// <see cref="Rapadura.Core.Localization.LocalizationManager"/> / <see cref="Rapadura.Core.Audio.AudioManager"/>:
    /// a plain C# class implementing <see cref="IManager"/>, meant to be constructed and
    /// registered with the <see cref="Rapadura.Core.DI.ServiceLocator"/> by
    /// <c>GameManager.BuildManagers()</c>.
    ///
    /// NOTE: this class is intentionally NOT wired into GameManager.cs by this change (per task
    /// instructions, GameManager.cs must not be edited here). A follow-up edit needs to add,
    /// mirroring the LocalizationManager/AudioManager wiring already there:
    ///   AccessibilitySettings = new AccessibilitySettings();
    ///   RegisterManager(AccessibilitySettings);
    /// inside <c>GameManager.BuildManagers()</c>, plus a public property.
    ///
    /// PERSISTENCE: values are persisted via <see cref="PlayerPrefs"/>, the same mechanism
    /// <see cref="Rapadura.Core.Audio.AudioManager"/> uses for per-category volume
    /// (see its <c>PlayerPrefsKeyPrefix</c> / <c>SetCategoryVolume</c>) — keys are namespaced
    /// under "Accessibility." to avoid collisions with Audio's "Audio.Volume." keys.
    ///
    /// INTEGRATION POINTS THAT ARE NOT WIRED BY THIS CLASS (see TODO.md Fase 6 note / final
    /// report for exact call sites this change does not touch):
    ///   - Font scale / high contrast: consumed by UI Toolkit panels (HudController, menu
    ///     controllers) which should multiply their base font size by <see cref="FontScale"/>
    ///     and toggle a "high-contrast" USS class when <see cref="HighContrast"/> is true.
    ///   - Colorblind palette: consumed via <see cref="ColorblindPaletteMap"/> using
    ///     <see cref="ColorblindMode"/> below.
    ///   - Screen shake: any caller of <c>PlayerCamera.Shake(duration, magnitude)</c> (directly,
    ///     or indirectly via <c>CombatCameraShakeRelay</c>/<c>CameraShakeRequestedEvent</c>) should
    ///     multiply the magnitude (and optionally duration) by <see cref="ScreenShakeIntensity"/>
    ///     before calling. PlayerCamera.cs/CombatCameraShakeRelay.cs are intentionally not edited
    ///     by this change.
    ///   - Flash/effects reduction: VFX/post-processing code that triggers screen flashes should
    ///     check <see cref="ReduceFlashEffects"/> and skip/soften the effect.
    /// </summary>
    public class AccessibilitySettings : IManager
    {
        private const string LogCategory = "Accessibility";

        private const string FontScaleKey = "Accessibility.FontScale";
        private const string HighContrastKey = "Accessibility.HighContrast";
        private const string ColorblindModeKey = "Accessibility.ColorblindMode";
        private const string ScreenShakeIntensityKey = "Accessibility.ScreenShakeIntensity";
        private const string ReduceFlashEffectsKey = "Accessibility.ReduceFlashEffects";

        public const float MinFontScale = 0.75f;
        public const float MaxFontScale = 2f;
        private const float DefaultFontScale = 1f;
        private const bool DefaultHighContrast = false;
        private const ColorblindMode DefaultColorblindMode = ColorblindMode.None;
        private const float DefaultScreenShakeIntensity = 1f;
        private const bool DefaultReduceFlashEffects = false;

        public bool IsInitialized { get; private set; }

        public float FontScale { get; private set; } = DefaultFontScale;
        public bool HighContrast { get; private set; } = DefaultHighContrast;
        public ColorblindMode ActiveColorblindMode { get; private set; } = DefaultColorblindMode;

        /// <summary>0..1 multiplier callers of PlayerCamera.Shake should apply to magnitude. 0 = no shake, 1 = full shake.</summary>
        public float ScreenShakeIntensity { get; private set; } = DefaultScreenShakeIntensity;

        public bool ReduceFlashEffects { get; private set; } = DefaultReduceFlashEffects;

        /// <summary>Raised whenever any setting changes, so UI/gameplay can react (e.g. re-apply font scale to open panels).</summary>
        public event Action SettingsChanged;

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            FontScale = Mathf.Clamp(PlayerPrefs.GetFloat(FontScaleKey, DefaultFontScale), MinFontScale, MaxFontScale);
            HighContrast = PlayerPrefs.GetInt(HighContrastKey, DefaultHighContrast ? 1 : 0) != 0;
            ActiveColorblindMode = ParseColorblindMode(PlayerPrefs.GetInt(ColorblindModeKey, (int)DefaultColorblindMode));
            ScreenShakeIntensity = Mathf.Clamp01(PlayerPrefs.GetFloat(ScreenShakeIntensityKey, DefaultScreenShakeIntensity));
            ReduceFlashEffects = PlayerPrefs.GetInt(ReduceFlashEffectsKey, DefaultReduceFlashEffects ? 1 : 0) != 0;

            IsInitialized = true;
            GameLogger.Info(LogCategory, $"AccessibilitySettings initialized (fontScale={FontScale}, highContrast={HighContrast}, colorblind={ActiveColorblindMode}, shake={ScreenShakeIntensity}, reduceFlash={ReduceFlashEffects}).");
        }

        public void Shutdown()
        {
            IsInitialized = false;
        }

        public void SetFontScale(float scale)
        {
            FontScale = Mathf.Clamp(scale, MinFontScale, MaxFontScale);
            PlayerPrefs.SetFloat(FontScaleKey, FontScale);
            RaiseChanged();
        }

        public void SetHighContrast(bool enabled)
        {
            HighContrast = enabled;
            PlayerPrefs.SetInt(HighContrastKey, enabled ? 1 : 0);
            RaiseChanged();
        }

        public void SetColorblindMode(ColorblindMode mode)
        {
            ActiveColorblindMode = mode;
            PlayerPrefs.SetInt(ColorblindModeKey, (int)mode);
            RaiseChanged();
        }

        public void SetScreenShakeIntensity(float intensity)
        {
            ScreenShakeIntensity = Mathf.Clamp01(intensity);
            PlayerPrefs.SetFloat(ScreenShakeIntensityKey, ScreenShakeIntensity);
            RaiseChanged();
        }

        public void SetReduceFlashEffects(bool enabled)
        {
            ReduceFlashEffects = enabled;
            PlayerPrefs.SetInt(ReduceFlashEffectsKey, enabled ? 1 : 0);
            RaiseChanged();
        }

        /// <summary>Convenience helper for callers about to invoke PlayerCamera.Shake: returns the
        /// magnitude already scaled by ScreenShakeIntensity (0 when a caller wants shake fully off).</summary>
        public float ScaleShakeMagnitude(float rawMagnitude)
        {
            return rawMagnitude * ScreenShakeIntensity;
        }

        private static ColorblindMode ParseColorblindMode(int rawValue)
        {
            return Enum.IsDefined(typeof(ColorblindMode), rawValue) ? (ColorblindMode)rawValue : ColorblindMode.None;
        }

        private void RaiseChanged()
        {
            SettingsChanged?.Invoke();
        }
    }
}
