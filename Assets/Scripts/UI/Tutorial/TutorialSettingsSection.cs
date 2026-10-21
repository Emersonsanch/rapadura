using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Tutorial;
using UnityEngine.UIElements;

namespace Rapadura.UI.Tutorial
{
    /// <summary>
    /// "Mostrar dicas" toggle + "Reiniciar tutorial" button, built in code and appended to a settings
    /// panel (SettingsMenuController's Acessibilidade tab), so SettingsMenuView.uxml stays untouched.
    /// Reuses the panel's own USS classes (<c>settings-field</c>, <c>settings-rebind-footer-button</c>,
    /// <c>settings-rebind-status</c>). Talks to <see cref="TutorialManager"/> through the ServiceLocator,
    /// resolved on every <see cref="Refresh"/> (the menu may enable before GameManager registers it);
    /// without a manager both controls are disabled.
    /// </summary>
    public sealed class TutorialSettingsSection
    {
        public const string ToggleName = "tutorial-hints-toggle";
        public const string ResetButtonName = "tutorial-reset-button";
        public const string StatusLabelName = "tutorial-reset-status";

        private VisualElement _container;
        private Toggle _toggle;
        private Button _resetButton;
        private Label _statusLabel;
        private TutorialManager _manager;
        private LocalizationManager _localization;

        public Toggle Toggle => _toggle;
        public Button ResetButton => _resetButton;
        public Label StatusLabel => _statusLabel;

        /// <summary>Optional explicit injection (tests); otherwise resolved from the ServiceLocator on Refresh.</summary>
        public TutorialManager Manager
        {
            get => _manager;
            set => _manager = value;
        }

        /// <summary>Appends the controls to <paramref name="panel"/> (once) and syncs them. No-op for a null panel.</summary>
        public void Attach(VisualElement panel)
        {
            if (panel == null)
            {
                return;
            }

            if (_container == null)
            {
                _container = new VisualElement { name = "tutorial-settings" };

                _toggle = new Toggle(TutorialTexts.GetFallback(TutorialTexts.SettingsShowHints)) { name = ToggleName, focusable = true };
                _toggle.AddToClassList("settings-field");
                _toggle.RegisterValueChangedCallback(OnToggleChanged);

                _resetButton = new Button(OnResetClicked) { name = ResetButtonName, text = TutorialTexts.GetFallback(TutorialTexts.SettingsReset), focusable = true };
                _resetButton.AddToClassList("settings-rebind-footer-button");

                _statusLabel = new Label(string.Empty) { name = StatusLabelName };
                _statusLabel.AddToClassList("settings-rebind-status");

                _container.Add(_toggle);
                _container.Add(_resetButton);
                _container.Add(_statusLabel);
            }

            if (_container.parent != panel)
            {
                panel.Add(_container);
            }

            Refresh();
        }

        public void Detach()
        {
            _container?.RemoveFromHierarchy();
        }

        /// <summary>Re-reads the manager state and the texts (call on menu open and on language change).</summary>
        public void Refresh()
        {
            if (_container == null)
            {
                return;
            }

            if (_manager == null)
            {
                ServiceLocator.TryGet(out _manager);
            }

            if (_localization == null)
            {
                ServiceLocator.TryGet(out _localization);
            }

            _toggle.label = TutorialTexts.Get(_localization, TutorialTexts.SettingsShowHints);
            _resetButton.text = TutorialTexts.Get(_localization, TutorialTexts.SettingsReset);
            _toggle.SetValueWithoutNotify(_manager == null || _manager.HintsEnabled);
            _toggle.SetEnabled(_manager != null);
            _resetButton.SetEnabled(_manager != null);
            _statusLabel.text = string.Empty;
        }

        private void OnToggleChanged(ChangeEvent<bool> evt)
        {
            _manager?.SetHintsEnabled(evt.newValue);
            _statusLabel.text = string.Empty;
        }

        private void OnResetClicked()
        {
            if (_manager == null)
            {
                return;
            }

            _manager.ResetTutorial();
            _toggle.SetValueWithoutNotify(_manager.HintsEnabled);
            _statusLabel.text = TutorialTexts.Get(_localization, TutorialTexts.SettingsResetDone);
        }
    }
}
