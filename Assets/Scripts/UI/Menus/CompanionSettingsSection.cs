using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Pets;
using UnityEngine.UIElements;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// "Ari (companheiro nos encontros)" toggle, built in code and appended to the Acessibilidade
    /// panel (same approach as <see cref="TouchControlsSettingsSection"/>, so SettingsMenuView.uxml stays
    /// untouched; reuses the panel's <c>settings-field</c> class). Writes
    /// <see cref="CompanionPreferences.CarameloEnabled"/> (default on); the dog leaves or comes back live.
    /// </summary>
    public sealed class CompanionSettingsSection
    {
        public const string ToggleName = "companion-caramelo-toggle";

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
                _toggle = new Toggle(CarameloTexts.GetFallback(CarameloTexts.SettingsToggle)) { name = ToggleName, focusable = true };
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

            _toggle.label = CarameloTexts.Get(_localization, CarameloTexts.SettingsToggle);
            _toggle.SetValueWithoutNotify(CompanionPreferences.CarameloEnabled);
        }

        private static void OnToggleChanged(ChangeEvent<bool> evt)
        {
            CompanionPreferences.CarameloEnabled = evt.newValue;
        }
    }
}
