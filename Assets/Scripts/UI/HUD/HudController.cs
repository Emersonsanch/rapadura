using Rapadura.Core.Events;
using Rapadura.Gameplay.Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.HUD
{
    /// <summary>
    /// Drives the always-on gameplay HUD (health/stamina/mana/XP bars) built with UI Toolkit
    /// (HudView.uxml/.uss), following the same pattern as <c>SkillTreeController</c>: a single
    /// <see cref="UIDocument"/>, elements resolved once via <c>Q&lt;T&gt;()</c>, and bars updated
    /// reactively from EventBus events published by <see cref="PlayerStats"/> instead of polling
    /// every frame — important on mobile where battery/thermal budget matters (see TODO.md Fase 0,
    /// "Bateria/thermal: evitar Update() caro rodando sempre a 60fps sem necessidade").
    ///
    /// Mana is driven the same way as health/stamina: <see cref="PlayerStats"/> publishes
    /// <see cref="PlayerManaChangedEvent"/> from SetMana, and this controller subscribes to it
    /// directly instead of polling every frame.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class HudController : MonoBehaviour
    {
        /// <summary>Where the HUD anchors itself within the screen/panel.</summary>
        public enum HudAnchorPreset
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            Custom
        }

        [Header("References")]
        [SerializeField] private PlayerStats _playerStats;

        [Header("Adaptive HUD")]
        [Tooltip("Uniform scale applied to the whole HUD root. Exposed here so a future " +
                 "Settings screen (TODO.md Fase 6 > Menus > Configurações) can bind an " +
                 "accessibility slider to SetHudScale() without touching this component.")]
        [SerializeField, Range(0.5f, 2f)] private float _hudScale = 1f;

        [Tooltip("Corner the HUD anchors to. 'Custom' uses _customOffset as a raw left/top " +
                 "offset instead of a corner, for free placement from a future layout editor.")]
        [SerializeField] private HudAnchorPreset _anchorPreset = HudAnchorPreset.TopLeft;

        [Tooltip("Offset from the chosen anchor (or raw left/top when preset is Custom), in pixels.")]
        [SerializeField] private Vector2 _anchorOffset = Vector2.zero;

        [Header("Low Value Warning")]
        [SerializeField, Range(0f, 1f)] private float _lowValueThreshold = 0.25f;

        private UIDocument _uiDocument;
        private VisualElement _root;

        private VisualElement _healthFill;
        private Label _healthValueLabel;
        private VisualElement _staminaFill;
        private Label _staminaValueLabel;
        private VisualElement _manaFill;
        private Label _manaValueLabel;
        private VisualElement _xpFill;
        private Label _xpValueLabel;
        private Label _levelLabel;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            BindElements();
            ApplyAdaptiveLayout();
            RegisterCallbacks();
            InitializeFromCurrentStats();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        private void BindElements()
        {
            VisualElement rootVisualElement = _uiDocument.rootVisualElement;

            _root = rootVisualElement.Q<VisualElement>("hud-root");

            _healthFill = rootVisualElement.Q<VisualElement>("health-bar-fill");
            _healthValueLabel = rootVisualElement.Q<Label>("health-bar-value-label");

            _staminaFill = rootVisualElement.Q<VisualElement>("stamina-bar-fill");
            _staminaValueLabel = rootVisualElement.Q<Label>("stamina-bar-value-label");

            _manaFill = rootVisualElement.Q<VisualElement>("mana-bar-fill");
            _manaValueLabel = rootVisualElement.Q<Label>("mana-bar-value-label");

            _xpFill = rootVisualElement.Q<VisualElement>("xp-bar-fill");
            _xpValueLabel = rootVisualElement.Q<Label>("xp-bar-value-label");
            _levelLabel = rootVisualElement.Q<Label>("level-label");
        }

        private void RegisterCallbacks()
        {
            EventBus.Subscribe<PlayerHealthChangedEvent>(OnHealthChanged);
            EventBus.Subscribe<PlayerStaminaChangedEvent>(OnStaminaChanged);
            EventBus.Subscribe<PlayerManaChangedEvent>(OnManaChanged);
            EventBus.Subscribe<PlayerExperienceChangedEvent>(OnExperienceChanged);
        }

        private void UnregisterCallbacks()
        {
            EventBus.Unsubscribe<PlayerHealthChangedEvent>(OnHealthChanged);
            EventBus.Unsubscribe<PlayerStaminaChangedEvent>(OnStaminaChanged);
            EventBus.Unsubscribe<PlayerManaChangedEvent>(OnManaChanged);
            EventBus.Unsubscribe<PlayerExperienceChangedEvent>(OnExperienceChanged);
        }

        private void InitializeFromCurrentStats()
        {
            if (_playerStats == null)
            {
                return;
            }

            UpdateHealthBar(_playerStats.CurrentHealth, _playerStats.MaxHealth);
            UpdateStaminaBar(_playerStats.CurrentStamina, _playerStats.MaxStamina);
            UpdateManaBar(_playerStats.CurrentMana, _playerStats.MaxMana);
            UpdateXpBar(_playerStats.CurrentExperience, _playerStats.GetExperienceToNextLevel(), _playerStats.Level);
        }

        private void OnHealthChanged(PlayerHealthChangedEvent evt) => UpdateHealthBar(evt.CurrentHealth, evt.MaxHealth);
        private void OnStaminaChanged(PlayerStaminaChangedEvent evt) => UpdateStaminaBar(evt.CurrentStamina, evt.MaxStamina);
        private void OnManaChanged(PlayerManaChangedEvent evt) => UpdateManaBar(evt.CurrentMana, evt.MaxMana);
        private void OnExperienceChanged(PlayerExperienceChangedEvent evt) => UpdateXpBar(evt.CurrentExperience, evt.ExperienceToNextLevel, evt.Level);

        private void UpdateHealthBar(float current, float max)
        {
            SetBarFill(_healthFill, _healthValueLabel, current, max);
        }

        private void UpdateStaminaBar(float current, float max)
        {
            SetBarFill(_staminaFill, _staminaValueLabel, current, max);
        }

        private void UpdateManaBar(float current, float max)
        {
            SetBarFill(_manaFill, _manaValueLabel, current, max);
        }

        private void UpdateXpBar(int current, int toNextLevel, int level)
        {
            SetBarFill(_xpFill, _xpValueLabel, current, toNextLevel);

            if (_levelLabel != null)
            {
                _levelLabel.text = $"Nv. {level}";
            }
        }

        private void SetBarFill(VisualElement fill, Label valueLabel, float current, float max)
        {
            if (fill == null)
            {
                return;
            }

            float percent = HudBarMath.ComputeFillPercent(current, max);
            fill.style.width = new Length(percent, LengthUnit.Percent);

            if (HudBarMath.IsLow(current, max, _lowValueThreshold))
            {
                fill.AddToClassList("hud-bar-fill--low");
            }
            else
            {
                fill.RemoveFromClassList("hud-bar-fill--low");
            }

            if (valueLabel != null)
            {
                valueLabel.text = HudBarMath.FormatValueLabel(current, max);
            }
        }

        /// <summary>
        /// Applies <see cref="_hudScale"/>/<see cref="_anchorPreset"/>/<see cref="_anchorOffset"/>
        /// to the HUD root. Public entry points below let a future Settings screen drive this at
        /// runtime (e.g. an accessibility "UI scale" slider) without needing a new component.
        /// </summary>
        private void ApplyAdaptiveLayout()
        {
            if (_root == null)
            {
                return;
            }

            _root.transform.scale = new Vector3(_hudScale, _hudScale, 1f);

            _root.style.left = StyleKeyword.Auto;
            _root.style.right = StyleKeyword.Auto;
            _root.style.top = StyleKeyword.Auto;
            _root.style.bottom = StyleKeyword.Auto;

            switch (_anchorPreset)
            {
                case HudAnchorPreset.TopLeft:
                    _root.style.left = _anchorOffset.x;
                    _root.style.top = _anchorOffset.y;
                    break;
                case HudAnchorPreset.TopRight:
                    _root.style.right = _anchorOffset.x;
                    _root.style.top = _anchorOffset.y;
                    break;
                case HudAnchorPreset.BottomLeft:
                    _root.style.left = _anchorOffset.x;
                    _root.style.bottom = _anchorOffset.y;
                    break;
                case HudAnchorPreset.BottomRight:
                    _root.style.right = _anchorOffset.x;
                    _root.style.bottom = _anchorOffset.y;
                    break;
                case HudAnchorPreset.Custom:
                    _root.style.left = _anchorOffset.x;
                    _root.style.top = _anchorOffset.y;
                    break;
            }
        }

        /// <summary>Runtime entry point for a future Settings/accessibility screen.</summary>
        public void SetHudScale(float scale)
        {
            _hudScale = Mathf.Clamp(scale, 0.5f, 2f);
            ApplyAdaptiveLayout();
        }

        /// <summary>Runtime entry point for a future Settings/accessibility screen (HUD position).</summary>
        public void SetAnchor(HudAnchorPreset preset, Vector2 offset)
        {
            _anchorPreset = preset;
            _anchorOffset = offset;
            ApplyAdaptiveLayout();
        }
    }
}
