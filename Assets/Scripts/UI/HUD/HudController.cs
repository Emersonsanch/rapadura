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
        [SerializeField] private PlayerInputHandler _playerInput;

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
        private Button _runButton;
        private Button _jumpButton;

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

        private void Start()
        {
            // OnEnable can run before PlayerStats.Awake() has set its Current*=Max* defaults
            // (Unity doesn't guarantee cross-object Awake/OnEnable ordering for objects spawned
            // at runtime), which left the bars reading 0/0 (invisible fill) until the first
            // stat-change event. Re-reading here, once every object's Awake has definitely run,
            // fixes that first-frame empty-bar bug without needing explicit script execution order.
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

            _runButton = rootVisualElement.Q<Button>("run-button");
            _jumpButton = rootVisualElement.Q<Button>("jump-button");
        }

        private void RegisterCallbacks()
        {
            EventBus.Subscribe<PlayerHealthChangedEvent>(OnHealthChanged);
            EventBus.Subscribe<PlayerStaminaChangedEvent>(OnStaminaChanged);
            EventBus.Subscribe<PlayerManaChangedEvent>(OnManaChanged);
            EventBus.Subscribe<PlayerExperienceChangedEvent>(OnExperienceChanged);

            if (_runButton != null)
            {
                // Not the Button's Click event on purpose — Click only fires on release, but Run
                // needs to stay held for as long as the finger/pointer stays down (a "sprint"
                // toggle, not a one-shot action), so PointerDown/Up + PointerLeave (finger sliding
                // off the button without a clean release) drive it directly instead.
                _runButton.RegisterCallback<PointerDownEvent>(OnRunButtonPressed);
                _runButton.RegisterCallback<PointerUpEvent>(OnRunButtonPointerUp);
                _runButton.RegisterCallback<PointerLeaveEvent>(OnRunButtonPointerLeave);
            }

            if (_jumpButton != null)
            {
                // One-shot like the physical Jump key, so PointerDown (not Click, which waits for
                // release) — same responsiveness reasoning as the run button.
                _jumpButton.RegisterCallback<PointerDownEvent>(OnJumpButtonPressed);
            }
        }

        private void UnregisterCallbacks()
        {
            EventBus.Unsubscribe<PlayerHealthChangedEvent>(OnHealthChanged);
            EventBus.Unsubscribe<PlayerStaminaChangedEvent>(OnStaminaChanged);
            EventBus.Unsubscribe<PlayerManaChangedEvent>(OnManaChanged);
            EventBus.Unsubscribe<PlayerExperienceChangedEvent>(OnExperienceChanged);

            if (_runButton != null)
            {
                _runButton.UnregisterCallback<PointerDownEvent>(OnRunButtonPressed);
                _runButton.UnregisterCallback<PointerUpEvent>(OnRunButtonPointerUp);
                _runButton.UnregisterCallback<PointerLeaveEvent>(OnRunButtonPointerLeave);
            }

            if (_jumpButton != null)
            {
                _jumpButton.UnregisterCallback<PointerDownEvent>(OnJumpButtonPressed);
            }

            if (_playerInput != null)
            {
                _playerInput.TouchRunHeld = false;
            }
        }

        private void OnJumpButtonPressed(PointerDownEvent evt)
        {
            if (_playerInput != null)
            {
                _playerInput.TouchJumpRequested = true;
            }
        }

        private void OnRunButtonPressed(PointerDownEvent evt)
        {
            if (_playerInput != null)
            {
                _playerInput.TouchRunHeld = true;
            }

            _runButton?.AddToClassList("hud-touch-button--active");
        }

        private void OnRunButtonPointerUp(PointerUpEvent evt) => ReleaseRunButton();
        private void OnRunButtonPointerLeave(PointerLeaveEvent evt) => ReleaseRunButton();

        private void ReleaseRunButton()
        {
            if (_playerInput != null)
            {
                _playerInput.TouchRunHeld = false;
            }

            _runButton?.RemoveFromClassList("hud-touch-button--active");
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

            Vector4 safeInset = ComputeSafeAreaInset();

            float left = safeInset.x;
            float top = safeInset.y;
            float right = safeInset.z;
            float bottom = safeInset.w;

            switch (_anchorPreset)
            {
                case HudAnchorPreset.TopLeft:
                case HudAnchorPreset.Custom:
                    left += _anchorOffset.x;
                    top += _anchorOffset.y;
                    break;
                case HudAnchorPreset.TopRight:
                    right += _anchorOffset.x;
                    top += _anchorOffset.y;
                    break;
                case HudAnchorPreset.BottomLeft:
                    left += _anchorOffset.x;
                    bottom += _anchorOffset.y;
                    break;
                case HudAnchorPreset.BottomRight:
                    right += _anchorOffset.x;
                    bottom += _anchorOffset.y;
                    break;
            }

            // All four sides are always set (not just the two the preset "anchors" to) — every
            // corner of the HUD (vitals top-left, XP bar bottom-left, Run/Jump bottom-right) is
            // positioned absolute relative to hud-root's own box, so every edge needs the safe-area
            // inset, not just whichever two sides the corner preset happens to touch. This also
            // means hud-root's box is now fully defined by its 4 offsets, so the USS width:100%/
            // height:100% no longer applies (Yoga resolves size from left+right / top+bottom instead).
            _root.style.left = left;
            _root.style.top = top;
            _root.style.right = right;
            _root.style.bottom = bottom;
        }

        /// <summary>
        /// Converts <see cref="Screen.safeArea"/> (device screen pixels) into panel-space insets
        /// (left, top, right, bottom) so HUD content stays clear of notches/camera cutouts/rounded
        /// corners/home-indicator bars on real phones. In the Editor (and any device reporting a
        /// full-screen safe area, i.e. no notch) this always returns zero insets — genuinely
        /// exercising the non-zero path needs a real notched device, which this environment
        /// doesn't have; the math mirrors Unity's documented safe-area pattern.
        /// </summary>
        private Vector4 ComputeSafeAreaInset()
        {
            if (_root == null || _root.panel == null)
            {
                return Vector4.zero;
            }

            float pixelsPerPoint = _root.panel.scaledPixelsPerPoint;

            if (pixelsPerPoint <= 0f)
            {
                return Vector4.zero;
            }

            Rect safeArea = Screen.safeArea;

            float left = safeArea.xMin / pixelsPerPoint;
            float bottom = safeArea.yMin / pixelsPerPoint;
            float right = (Screen.width - safeArea.xMax) / pixelsPerPoint;
            float top = (Screen.height - safeArea.yMax) / pixelsPerPoint;

            return new Vector4(left, top, right, bottom);
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
