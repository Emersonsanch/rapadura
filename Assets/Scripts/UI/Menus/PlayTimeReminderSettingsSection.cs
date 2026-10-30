using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Daily;
using UnityEngine.UIElements;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// Parental "Lembrete de tempo de jogo (para os pais)" dropdown, built in code and appended to the Acessibilidade
    /// panel (same approach as <see cref="HapticsSettingsSection"/>; reuses <c>settings-field</c>). Choices:
    /// Desligado / a cada 30, 45, 60, 90, 120 min (<see cref="DailyStaminaRules.PlayTimeReminderChoices"/>); writes
    /// <see cref="DailyStaminaService.SetPlayTimeReminderMinutes"/> (per device). The reminder is a gentle toast on the
    /// map/arena HUD counting active play only; it never stops the game.
    /// </summary>
    public sealed class PlayTimeReminderSettingsSection
    {
        public const string FieldName = "playtime-reminder-dropdown";

        private DropdownField _field;
        private DailyStaminaService _service;
        private LocalizationManager _localization;

        public DropdownField Field => _field;

        /// <summary>Optional explicit injection (tests); otherwise resolved from the ServiceLocator on Refresh.</summary>
        public DailyStaminaService Service
        {
            get => _service;
            set => _service = value;
        }

        public void Attach(VisualElement panel)
        {
            if (panel == null)
            {
                return;
            }

            if (_field == null)
            {
                _field = new DropdownField(DailyTexts.GetFallback(DailyTexts.SettingsPlayTimeReminder)) { name = FieldName, focusable = true };
                _field.AddToClassList("settings-field");
                _field.RegisterValueChangedCallback(OnChanged);
            }

            if (_field.parent != panel)
            {
                panel.Add(_field);
            }

            Refresh();
        }

        public void Detach()
        {
            _field?.RemoveFromHierarchy();
        }

        public void Refresh()
        {
            if (_field == null)
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

            _field.label = DailyTexts.Get(_localization, DailyTexts.SettingsPlayTimeReminder);
            List<string> labels = DailyStaminaRules.PlayTimeReminderChoices.Select(m => DailyTexts.ReminderChoiceLabel(_localization, m)).ToList();
            _field.choices = labels;
            int current = _service != null ? _service.PlayTimeReminderMinutes : 0;
            int index = System.Array.IndexOf(DailyStaminaRules.PlayTimeReminderChoices, current);
            _field.SetValueWithoutNotify(labels[index < 0 ? 0 : index]);
            _field.SetEnabled(_service != null);
        }

        private void OnChanged(ChangeEvent<string> evt)
        {
            int index = _field.choices.IndexOf(evt.newValue);
            if (_service != null && index >= 0 && index < DailyStaminaRules.PlayTimeReminderChoices.Length)
            {
                _service.SetPlayTimeReminderMinutes(DailyStaminaRules.PlayTimeReminderChoices[index]);
            }
        }
    }
}
