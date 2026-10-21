using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.UI.Common;
using UnityEngine.UIElements;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// "Joystick virtual" toggle, built in code and appended to the Controles panel (same approach as
    /// <see cref="HapticsSettingsSection"/>, so SettingsMenuView.uxml stays untouched; reuses the
    /// panel's <c>settings-field</c> class). Writes <see cref="TouchControlsPreferences.VirtualJoystickEnabled"/>
    /// (default off — tap-to-move stays the primary touch control). Without a touchscreen the label
    /// says the option is touch-only, but it stays editable so it's already set when the game runs
    /// on a phone/tablet.
    /// </summary>
    public sealed class TouchControlsSettingsSection
    {
        public const string ToggleName = "virtual-joystick-toggle";

        private Toggle _toggle;
        private LocalizationManager _localization;

        public Toggle Toggle => _toggle;

        /// <summary>Appends the toggle to <paramref name="panel"/> (once, as its last child) and syncs it. No-op for a null panel.</summary>
        public void Attach(VisualElement panel)
        {
            if (panel == null)
            {
                return;
            }

            if (_toggle == null)
            {
                _toggle = new Toggle(TouchControlsTexts.GetFallback(TouchControlsTexts.SettingsVirtualJoystick)) { name = ToggleName, focusable = true };
                _toggle.AddToClassList("settings-field");
                _toggle.RegisterValueChangedCallback(OnToggleChanged);
            }

            if (_toggle.parent != panel)
            {
                panel.Add(_toggle);
            }

            Refresh();
        }

        public void Detach()
        {
            _toggle?.RemoveFromHierarchy();
        }

        /// <summary>Re-reads the preference and the label (menu open, language change).</summary>
        public void Refresh()
        {
            if (_toggle == null)
            {
                return;
            }

            if (_localization == null)
            {
                ServiceLocator.TryGet(out _localization);
            }

            _toggle.label = TouchControlsTexts.Get(_localization, TouchControlsPreferences.HasTouchscreen
                ? TouchControlsTexts.SettingsVirtualJoystick
                : TouchControlsTexts.SettingsVirtualJoystickTouchOnly);
            _toggle.SetValueWithoutNotify(TouchControlsPreferences.VirtualJoystickEnabled);
        }

        private void OnToggleChanged(ChangeEvent<bool> evt)
        {
            TouchControlsPreferences.VirtualJoystickEnabled = evt.newValue;
        }
    }
}
