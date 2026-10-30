using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Daily;
using Rapadura.UI.Common;
using Rapadura.UI.HUD;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.Daily
{
    /// <summary>Loads Resources/UI/DailyRewards.uss (cached) and attaches it with the theme / reduced-motion classes.</summary>
    public static class DailyStyles
    {
        public const string ResourcePath = "UI/DailyRewards";
        public const string ThemeClass = "rap-theme";

        private static StyleSheet _sheet;

        public static StyleSheet Sheet
        {
            get
            {
                if (_sheet == null)
                {
                    _sheet = Resources.Load<StyleSheet>(ResourcePath);
                }

                return _sheet;
            }
        }

        public static void ApplyTo(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            StyleSheet sheet = Sheet;
            if (sheet != null && !root.styleSheets.Contains(sheet))
            {
                root.styleSheets.Add(sheet);
            }

            root.AddToClassList(ThemeClass);
            UiMotionPreferences.ApplyTo(root, UiMotionPreferences.IsReducedMotion());
        }
    }

    /// <summary>
    /// Feeds <see cref="DailyStaminaService"/> every frame. Self-bootstrapping (no scene edit), persistent across scenes.
    /// A frame counts as play (<see cref="DailyStaminaRules.ClassifyTick"/>) only in a gameplay scene (one with the arena
    /// <see cref="HudController"/> or the <see cref="WorldMapHudController"/>; the title is rest), not paused
    /// (<see cref="GameplayPause"/>), with the app focused and input (keys, mouse, touch, gamepad) in the last 3 minutes.
    /// Frames longer than <see cref="MaxFrameSeconds"/> (app suspended, debugger) and coming back from the background go
    /// through the clock-guarded offline path (<see cref="DailyStaminaService.ResumeAfterAway"/>).
    /// </summary>
    [DisallowMultipleComponent]
    public class DailyStaminaDriver : MonoBehaviour
    {
        public const float MaxFrameSeconds = 5f;

        private static DailyStaminaDriver _instance;

        private bool _inGameplay;
        private bool _focused = true;
        private float _lastInputTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null)
            {
                return;
            }

            var host = new GameObject(nameof(DailyStaminaDriver));
            DontDestroyOnLoad(host);
            _instance = host.AddComponent<DailyStaminaDriver>();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            _lastInputTime = Time.realtimeSinceStartup;
            RefreshScene();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => RefreshScene();

        private void RefreshScene()
        {
            _inGameplay = FindFirstObjectByType<HudController>(FindObjectsInactive.Exclude) != null
                || FindFirstObjectByType<WorldMapHudController>(FindObjectsInactive.Exclude) != null;
        }

        private void Update()
        {
            if (!ServiceLocator.TryGet(out DailyStaminaService service) || service == null)
            {
                return;
            }

            float now = Time.realtimeSinceStartup;
            if (HadInputThisFrame())
            {
                _lastInputTime = now;
            }

            float delta = Time.unscaledDeltaTime;
            if (delta > MaxFrameSeconds)
            {
                service.ResumeAfterAway();
                return;
            }

            StaminaTickKind kind = DailyStaminaRules.ClassifyTick(_inGameplay, GameplayPause.IsPaused, _focused, now - _lastInputTime);
            service.Tick(delta, kind);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            _focused = hasFocus;
        }

        private void OnApplicationPause(bool paused)
        {
            if (!ServiceLocator.TryGet(out DailyStaminaService service) || service == null)
            {
                return;
            }

            if (paused)
            {
                service.Suspend();
            }
            else
            {
                service.ResumeAfterAway();
                _lastInputTime = Time.realtimeSinceStartup;
            }
        }

        private void OnApplicationQuit()
        {
            if (ServiceLocator.TryGet(out DailyStaminaService service))
            {
                service?.Suspend();
            }
        }

        private static bool HadInputThisFrame()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.anyKey.isPressed)
            {
                return true;
            }

            Mouse mouse = Mouse.current;
            if (mouse != null && (mouse.leftButton.isPressed || mouse.rightButton.isPressed || mouse.delta.ReadValue().sqrMagnitude > 0.01f
                                  || mouse.scroll.ReadValue().sqrMagnitude > 0.01f))
            {
                return true;
            }

            Touchscreen touch = Touchscreen.current;
            if (touch != null && touch.primaryTouch.press.isPressed)
            {
                return true;
            }

            Gamepad pad = Gamepad.current;
            if (pad != null)
            {
                if (pad.leftStick.ReadValue().sqrMagnitude > 0.04f || pad.rightStick.ReadValue().sqrMagnitude > 0.04f)
                {
                    return true;
                }

                foreach (InputControl control in pad.allControls)
                {
                    if (control is UnityEngine.InputSystem.Controls.ButtonControl button && button.isPressed)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    /// <summary>
    /// The "Fôlego" meter on the world map HUD (under the hero's level, <c>wm-hero-block</c>) and the arena HUD (under the
    /// vitals, <c>hud-vitals-panel</c>), plus the reminder toast. Self-bootstrapping: after every scene load it joins the
    /// <see cref="HudController"/> / <see cref="WorldMapHudController"/> GameObject (same pattern as <c>DuoStrikeHud</c>).
    /// The meter shows the tier ("Fôlego cheio +30%" / "Fôlego normal" / "Cansado −50%") and a bar of what is left of the
    /// 5 hours; hovering or tapping it opens the tooltip (time left in the tier and how rest refills it). Reminders
    /// (<see cref="StaminaReminderEvent"/>: "Hora de descansar…" and the parental play-time one) show as a toast for
    /// <see cref="ToastSeconds"/> real seconds. Look: Resources/UI/DailyRewards.uss.
    /// </summary>
    [DisallowMultipleComponent]
    public class DailyStaminaHud : MonoBehaviour
    {
        public const string MeterName = "daily-stamina";
        public const string MeterCaptionName = "daily-stamina-caption";
        public const string MeterTrackName = "daily-stamina-track";
        public const string MeterFillName = "daily-stamina-fill";
        public const string TooltipName = "daily-stamina-tooltip";
        public const string ToastName = "daily-stamina-toast";

        public const string FullClass = "daily-stamina--full";
        public const string NormalClass = "daily-stamina--normal";
        public const string TiredClass = "daily-stamina--tired";

        public const float ToastSeconds = 7f;
        private const float RefreshInterval = 0.5f;

        private const string ArenaRootName = "hud-root";
        private const string ArenaParentName = "hud-vitals-panel";
        private const string MapRootName = "wm-hud-root";
        private const string MapParentName = "wm-hero-block";

        private UIDocument _document;
        private bool _isMap;
        private VisualElement _meter;
        private Label _caption;
        private VisualElement _fill;
        private Label _tooltip;
        private Label _toast;
        private bool _tooltipPinned;
        private float _toastUntil;
        private float _nextRefresh;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            AttachToHuds();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => AttachToHuds();

        private static void AttachToHuds()
        {
            HudController hud = FindFirstObjectByType<HudController>(FindObjectsInactive.Include);
            if (hud != null && hud.GetComponent<DailyStaminaHud>() == null)
            {
                hud.gameObject.AddComponent<DailyStaminaHud>();
            }

            WorldMapHudController map = FindFirstObjectByType<WorldMapHudController>(FindObjectsInactive.Include);
            if (map != null && map.GetComponent<DailyStaminaHud>() == null)
            {
                map.gameObject.AddComponent<DailyStaminaHud>();
            }
        }

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            _isMap = GetComponent<WorldMapHudController>() != null;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<StaminaReminderEvent>(OnReminder);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<StaminaReminderEvent>(OnReminder);
            _meter?.RemoveFromHierarchy();
            _toast?.RemoveFromHierarchy();
            _meter = null;
            _toast = null;
        }

        // ------------------------------------------------------------------
        // Pure rules (tested)
        // ------------------------------------------------------------------

        public static string TierClass(StaminaTier tier)
        {
            switch (tier)
            {
                case StaminaTier.Full:
                    return FullClass;
                case StaminaTier.Tired:
                    return TiredClass;
                default:
                    return NormalClass;
            }
        }

        // ------------------------------------------------------------------
        // Frame
        // ------------------------------------------------------------------

        private void Update()
        {
            if (_toast != null && _toastUntil > 0f && Time.unscaledTime >= _toastUntil)
            {
                _toastUntil = 0f;
                _toast.style.display = DisplayStyle.None;
            }

            if (Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + RefreshInterval;
            if (!EnsureBuilt() || !ServiceLocator.TryGet(out DailyStaminaService service) || service == null || !service.IsInitialized)
            {
                if (_meter != null)
                {
                    _meter.style.display = DisplayStyle.None;
                }

                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            StaminaTier tier = service.Tier;
            _meter.style.display = DisplayStyle.Flex;
            _caption.text = DailyTexts.TierCaption(localization, tier);
            _fill.style.width = Length.Percent(Mathf.Clamp01(service.Fill) * 100f);
            foreach (StaminaTier t in new[] { StaminaTier.Full, StaminaTier.Normal, StaminaTier.Tired })
            {
                _meter.EnableInClassList(TierClass(t), t == tier);
            }

            _tooltip.text = DailyTexts.Tooltip(localization, tier, service.SecondsToNextTier);
        }

        private void OnReminder(StaminaReminderEvent evt)
        {
            if (!EnsureBuilt())
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            _toast.text = DailyTexts.ReminderText(localization, evt.Kind, evt.SessionMinutes);
            _toast.style.display = DisplayStyle.Flex;
            _toastUntil = Time.unscaledTime + ToastSeconds;
            UiAccessibilityApplier.RequestRefresh();
        }

        // ------------------------------------------------------------------
        // View
        // ------------------------------------------------------------------

        private bool EnsureBuilt()
        {
            if (_document == null || _document.rootVisualElement == null)
            {
                return false;
            }

            VisualElement root = _document.rootVisualElement.Q<VisualElement>(_isMap ? MapRootName : ArenaRootName) ?? _document.rootVisualElement;
            VisualElement parent = root.Q<VisualElement>(_isMap ? MapParentName : ArenaParentName) ?? root;
            if (_meter != null && _meter.parent == parent && _toast != null && _toast.parent == root)
            {
                return true;
            }

            _meter?.RemoveFromHierarchy();
            _toast?.RemoveFromHierarchy();
            _meter = CreateMeter();
            _caption = _meter.Q<Label>(MeterCaptionName);
            _fill = _meter.Q<VisualElement>(MeterFillName);
            _tooltip = _meter.Q<Label>(TooltipName);
            _toast = CreateToast();
            _meter.RegisterCallback<PointerEnterEvent>(_ => ShowTooltip(true));
            _meter.RegisterCallback<PointerLeaveEvent>(_ => ShowTooltip(_tooltipPinned));
            _meter.RegisterCallback<PointerDownEvent>(_ =>
            {
                _tooltipPinned = !_tooltipPinned;
                ShowTooltip(_tooltipPinned);
            });
            parent.Add(_meter);
            root.Add(_toast);
            UiAccessibilityApplier.RequestRefresh();
            return true;
        }

        private void ShowTooltip(bool show)
        {
            if (_tooltip != null)
            {
                _tooltip.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        /// <summary>The meter (caption + bar + hidden tooltip), themed via <see cref="DailyStyles"/>. EditMode-testable.</summary>
        public static VisualElement CreateMeter()
        {
            var meter = new VisualElement { name = MeterName, pickingMode = PickingMode.Position };
            meter.AddToClassList(MeterName);
            DailyStyles.ApplyTo(meter);

            var caption = new Label { name = MeterCaptionName, pickingMode = PickingMode.Ignore };
            caption.AddToClassList(MeterCaptionName);
            var track = new VisualElement { name = MeterTrackName, pickingMode = PickingMode.Ignore };
            track.AddToClassList(MeterTrackName);
            var fill = new VisualElement { name = MeterFillName, pickingMode = PickingMode.Ignore };
            fill.AddToClassList(MeterFillName);
            track.Add(fill);
            var tooltip = new Label { name = TooltipName, pickingMode = PickingMode.Ignore };
            tooltip.AddToClassList(TooltipName);
            tooltip.style.display = DisplayStyle.None;

            meter.Add(caption);
            meter.Add(track);
            meter.Add(tooltip);
            return meter;
        }

        /// <summary>The reminder toast (hidden), themed via <see cref="DailyStyles"/>.</summary>
        public static Label CreateToast()
        {
            var toast = new Label { name = ToastName, pickingMode = PickingMode.Ignore };
            toast.AddToClassList(ToastName);
            DailyStyles.ApplyTo(toast);
            toast.style.display = DisplayStyle.None;
            return toast;
        }
    }
}
