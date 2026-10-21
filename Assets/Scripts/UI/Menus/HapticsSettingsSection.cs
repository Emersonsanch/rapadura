using Rapadura.Audio;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using UnityEngine.UIElements;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// "Vibração" toggle, built in code and appended to the Acessibilidade panel after the tutorial
    /// rows (same approach as <c>TutorialSettingsSection</c>, so SettingsMenuView.uxml/USS stay untouched;
    /// reuses the panel's <c>settings-field</c> class). Writes <see cref="HapticsService.SetEnabled"/>,
    /// resolved from the ServiceLocator on every <see cref="Refresh"/>; disabled without a service.
    /// On desktop/editor the label says vibration only works on phones/tablets.
    /// </summary>
    public sealed class HapticsSettingsSection
    {
        public const string ToggleName = "vibration-toggle";

        private Toggle _toggle;
        private HapticsService _service;
        private LocalizationManager _localization;

        public Toggle Toggle => _toggle;

        /// <summary>Optional explicit injection (tests); otherwise resolved from the ServiceLocator on Refresh.</summary>
        public HapticsService Service
        {
            get => _service;
            set => _service = value;
        }

        /// <summary>Appends the toggle to <paramref name="panel"/> (once, as its last child) and syncs it. No-op for a null panel.</summary>
        public void Attach(VisualElement panel)
        {
            if (panel == null)
            {
                return;
            }

            if (_toggle == null)
            {
                _toggle = new Toggle(HapticsTexts.GetFallback(HapticsTexts.SettingsVibration)) { name = ToggleName, focusable = true };
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

        /// <summary>Re-reads the service state and the label (menu open, language change).</summary>
        public void Refresh()
        {
            if (_toggle == null)
            {
                return;
            }

            if (_service == null)
            {
                ServiceLocator.TryGet(out _service);
            }

            if (_localization == null)
            {
                ServiceLocator.TryGet(out _localization);
            }

            bool supported = _service == null || _service.IsSupported;
            _toggle.label = HapticsTexts.Get(_localization, supported ? HapticsTexts.SettingsVibration : HapticsTexts.SettingsVibrationUnsupported);
            _toggle.SetValueWithoutNotify(_service == null || _service.Enabled);
            _toggle.SetEnabled(_service != null);
        }

        private void OnToggleChanged(ChangeEvent<bool> evt)
        {
            _service?.SetEnabled(evt.newValue);
        }
    }
}
