using System;
using Rapadura.Core.Logging;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rapadura.Core.Accessibility
{
    /// <summary>
    /// Wraps the New Input System's real rebind APIs
    /// (<see cref="InputActionRebindingExtensions.PerformInteractiveRebinding"/> and
    /// <see cref="InputActionRebindingExtensions.SaveBindingOverridesAsJson"/> /
    /// <see cref="InputActionRebindingExtensions.LoadBindingOverridesFromJson"/>) so UI code
    /// (the future Fase 6 "Controles" settings screen — see <c>SettingsMenuController.cs</c>,
    /// which today has the controles tab as a documented placeholder) can offer interactive
    /// rebinding without touching the Input System API directly.
    ///
    /// Not an <see cref="Rapadura.Core.Interfaces.IManager"/>: it has no per-frame state of its
    /// own and does not need Initialize/Shutdown lifecycle hooks — it operates directly on
    /// whatever <see cref="InputActionAsset"/> is handed to it (the project's
    /// PlayerControls.inputactions at runtime, or a throwaway test asset in EditMode tests).
    /// Construct one with the asset in use and keep it alive as long as rebind UI needs it.
    ///
    /// PERSISTENCE: uses the same PlayerPrefs pattern as
    /// <see cref="Rapadura.Core.Audio.AudioManager"/>'s volume keys — overrides are serialized to
    /// a single JSON blob via the Input System's own SaveBindingOverridesAsJson and stored under
    /// one PlayerPrefs key per asset name, since that call already captures every rebound action
    /// in the asset.
    /// </summary>
    public class InputRebindManager
    {
        private const string LogCategory = "InputRebind";
        private const string PlayerPrefsKeyPrefix = "Accessibility.InputBindings.";

        private readonly InputActionAsset _actions;
        private InputActionRebindingExtensions.RebindingOperation _activeRebind;

        public InputRebindManager(InputActionAsset actions)
        {
            _actions = actions ?? throw new ArgumentNullException(nameof(actions));
        }

        /// <summary>True while an interactive rebind started by <see cref="PerformInteractiveRebind"/> is waiting for input.</summary>
        public bool IsRebinding => _activeRebind != null;

        /// <summary>
        /// Starts an interactive rebind for one binding of <paramref name="action"/>.
        /// <paramref name="bindingIndex"/> selects which binding to rebind when the action has
        /// several (e.g. composite parts, or one binding per control scheme) — pass 0 for a
        /// simple single-binding action. Excludes mouse/pointer motion by default so a rebind
        /// doesn't get immediately satisfied by the player's mouse jiggling.
        /// </summary>
        public void PerformInteractiveRebind(InputAction action, int bindingIndex, Action onComplete = null, Action onCancel = null)
        {
            if (action == null)
            {
                GameLogger.Warning(LogCategory, "PerformInteractiveRebind called with a null action.");
                return;
            }

            if (IsRebinding)
            {
                GameLogger.Warning(LogCategory, "PerformInteractiveRebind called while another rebind is already active; cancelling the previous one.");
                CancelActiveRebind();
            }

            action.Disable();

            _activeRebind = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("Mouse")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation =>
                {
                    CleanUpRebind(operation, action);
                    onComplete?.Invoke();
                })
                .OnCancel(operation =>
                {
                    CleanUpRebind(operation, action);
                    onCancel?.Invoke();
                })
                .Start();
        }

        /// <summary>Cancels an in-progress interactive rebind started by <see cref="PerformInteractiveRebind"/>, if any.</summary>
        public void CancelActiveRebind()
        {
            _activeRebind?.Cancel();
        }

        private void CleanUpRebind(InputActionRebindingExtensions.RebindingOperation operation, InputAction action)
        {
            operation.Dispose();
            _activeRebind = null;
            action.Enable();
        }

        /// <summary>Resets a single binding back to its original path, clearing any override.</summary>
        public void ResetBinding(InputAction action, int bindingIndex)
        {
            action?.RemoveBindingOverride(bindingIndex);
        }

        /// <summary>Resets every override on every action map in the wrapped asset.</summary>
        public void ResetAllBindings()
        {
            _actions.RemoveAllBindingOverrides();
        }

        /// <summary>
        /// Serializes all current binding overrides on the wrapped asset to PlayerPrefs, using
        /// the Input System's own <see cref="InputActionRebindingExtensions.SaveBindingOverridesAsJson"/>.
        /// </summary>
        public void SaveBindingOverrides()
        {
            string json = _actions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(PrefsKey(), json);
            PlayerPrefs.Save();
            GameLogger.Info(LogCategory, $"Saved binding overrides for '{_actions.name}'.");
        }

        /// <summary>
        /// Loads previously saved binding overrides from PlayerPrefs via
        /// <see cref="InputActionRebindingExtensions.LoadBindingOverridesFromJson"/>. No-ops
        /// silently if nothing was ever saved for this asset.
        /// </summary>
        public bool LoadBindingOverrides()
        {
            string key = PrefsKey();
            if (!PlayerPrefs.HasKey(key))
            {
                return false;
            }

            string json = PlayerPrefs.GetString(key);
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            _actions.LoadBindingOverridesFromJson(json);
            GameLogger.Info(LogCategory, $"Loaded binding overrides for '{_actions.name}'.");
            return true;
        }

        /// <summary>Human-readable current binding path for display in rebind UI (e.g. "E", "Gamepad West Button").</summary>
        public string GetBindingDisplayString(InputAction action, int bindingIndex)
        {
            return action == null ? string.Empty : action.GetBindingDisplayString(bindingIndex);
        }

        private string PrefsKey()
        {
            return PlayerPrefsKeyPrefix + _actions.name;
        }
    }
}
