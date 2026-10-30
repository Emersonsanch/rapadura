using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Mounts;
using Rapadura.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Mounts
{
    /// <summary>
    /// The mount HUD tree, built in code (no UXML) and shared by the arena (<see cref="MountHud"/>) and the world map
    /// (<see cref="WorldMapMountHud"/>): the round "Montaria" button (touch/mouse: hold to whistle, the fill rises;
    /// tap while mounted to get off), the ability button (shown mounted, counts the cooldown down), the gallop stamina
    /// and mount health bars, and a notice line (<see cref="MountNotices"/>) shown for <see cref="NoticeSeconds"/>.
    /// Look in Resources/UI/Mounts.uss (<see cref="MountHudStyles"/>). <see cref="Refresh"/> is driven by the owner
    /// every ~0.1 s; the button presses go straight to the rider.
    /// </summary>
    public sealed class MountHudView
    {
        public const string LayerName = "mount-layer";
        public const string MapModifierClass = "mount-layer--map";
        public const string ButtonName = "mount-button";
        public const string FillName = "mount-fill";
        public const string LabelName = "mount-label";
        public const string HintName = "mount-hint";
        public const string AbilityButtonName = "mount-ability-button";
        public const string AbilityLabelName = "mount-ability-label";
        public const string BarsName = "mount-bars";
        public const string StaminaName = "mount-stamina";
        public const string HealthName = "mount-health";
        public const string NoticeName = "mount-notice";
        public const string StableButtonName = "mount-stable-button";
        public const string MountedClass = "mount-button--mounted";
        public const string EmptyClass = "mount-button--empty";
        public const string TrotClass = "mount-stamina--trot";
        public const string LoadName = "mount-load";
        public const string LoadHeavyClass = "mount-load--heavy";
        public const string LoadOverClass = "mount-load--over";
        public const float NoticeSeconds = 3.2f;

        private readonly VisualElement _layer;
        private readonly Button _button;
        private readonly VisualElement _fill;
        private readonly Label _label;
        private readonly Label _hint;
        private readonly Button _abilityButton;
        private readonly Label _abilityLabel;
        private readonly VisualElement _bars;
        private readonly VisualElement _staminaBar;
        private readonly VisualElement _staminaFill;
        private readonly Label _staminaCaption;
        private readonly VisualElement _healthFill;
        private readonly Label _loadCaption;
        private readonly VisualElement _loadBar;
        private readonly VisualElement _loadFill;
        private readonly Label _notice;
        private float _noticeHideAt = -1f;
        private readonly System.Collections.Generic.Queue<string> _pending = new System.Collections.Generic.Queue<string>();
        private IMountRider _rider;
        private readonly Button _stableButton;

        /// <summary>The map's "Montarias" button (stable + Caderno), or null in the arena.</summary>
        public Button StableButton => _stableButton;

        /// <summary>Raised when the "Montarias" button is pressed (map only).</summary>
        public event System.Action StableRequested;

        public VisualElement Root => _layer;
        public Button MountButton => _button;
        public Button AbilityButton => _abilityButton;
        public Label Notice => _notice;

        public MountHudView(bool onMap, bool reducedMotion)
        {
            _layer = new VisualElement { name = LayerName, pickingMode = PickingMode.Ignore };
            _layer.AddToClassList(LayerName);
            if (onMap)
            {
                _layer.AddToClassList(MapModifierClass);
            }

            MountHudStyles.ApplyTo(_layer, reducedMotion);

            var cluster = new VisualElement { pickingMode = PickingMode.Ignore };
            cluster.AddToClassList("mount-cluster");

            _abilityButton = new Button { name = AbilityButtonName, focusable = false };
            _abilityButton.AddToClassList(AbilityButtonName);
            _abilityLabel = Text(AbilityLabelName);
            _abilityButton.Add(_abilityLabel);
            _abilityButton.clicked += () =>
            {
                if (_rider != null)
                {
                    _rider.TouchAbilityRequested = true;
                }
            };

            _button = new Button { name = ButtonName, focusable = false };
            _button.AddToClassList(ButtonName);
            _fill = new VisualElement { name = FillName, pickingMode = PickingMode.Ignore };
            _fill.AddToClassList(FillName);
            _label = Text(LabelName);
            _hint = Text(HintName);
            _button.Add(_fill);
            _button.Add(_label);
            _button.Add(_hint);

            // Hold to whistle: the Button's own Clickable captures the pointer, so listen in the trickle-down phase.
            _button.RegisterCallback<PointerDownEvent>(_ => SetHeld(true), TrickleDown.TrickleDown);
            _button.RegisterCallback<PointerUpEvent>(_ => SetHeld(false), TrickleDown.TrickleDown);
            _button.RegisterCallback<PointerCancelEvent>(_ => SetHeld(false), TrickleDown.TrickleDown);
            _button.RegisterCallback<PointerLeaveEvent>(_ => SetHeld(false));

            cluster.Add(_abilityButton);
            cluster.Add(_button);
            if (onMap)
            {
                _stableButton = new Button { name = StableButtonName, focusable = false };
                _stableButton.AddToClassList(StableButtonName);
                _stableButton.clicked += () => StableRequested?.Invoke();
                _layer.Add(_stableButton);
            }

            _bars = new VisualElement { name = BarsName, pickingMode = PickingMode.Ignore };
            _bars.AddToClassList(BarsName);
            _staminaBar = Bar(StaminaName, out _staminaFill);
            _staminaCaption = Text("mount-bar-caption");
            VisualElement health = Bar(HealthName, out _healthFill);
            _bars.Add(_staminaCaption);
            _bars.Add(_staminaBar);
            _bars.Add(health);
            // Saddlebags (EncumbranceTracker): "Alforjes 120/210" + a bar, amber past half, red over capacity.
            _loadCaption = Text("mount-bar-caption");
            _loadBar = Bar(LoadName, out _loadFill);
            _bars.Add(_loadCaption);
            _bars.Add(_loadBar);

            _notice = Text(NoticeName);
            _notice.style.display = DisplayStyle.None;

            _layer.Add(cluster);
            _layer.Add(_bars);
            _layer.Add(_notice);
            SetVisible(false);
        }

        public void Bind(IMountRider rider)
        {
            if (!ReferenceEquals(_rider, rider) && _rider != null)
            {
                _rider.TouchMountHeld = false;
            }

            _rider = rider;
        }

        public void ShowNotice(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (_noticeHideAt > 0f)
            {
                // One at a time: the Primeira Sela scene sends several lines in a row.
                if (_pending.Count < 8)
                {
                    _pending.Enqueue(text);
                }

                return;
            }

            _notice.text = text;
            _notice.style.display = DisplayStyle.Flex;
            _noticeHideAt = Time.unscaledTime + NoticeSeconds;
        }

        public void TickNotice()
        {
            if (_noticeHideAt > 0f && Time.unscaledTime >= _noticeHideAt)
            {
                _noticeHideAt = -1f;
                _notice.style.display = DisplayStyle.None;
                if (_pending.Count > 0)
                {
                    ShowNotice(_pending.Dequeue());
                }
            }
        }

        // ------------------------------------------------------------------
        // Pure rules (tested)
        // ------------------------------------------------------------------

        /// <summary>The cluster is shown with a mount owned, nothing pausing, and where riding is possible (or while mounted).</summary>
        public static bool ShouldShow(bool hasMount, bool paused, bool hudVisible, MountPlace place, bool mounted)
        {
            return hasMount && !paused && hudVisible && (mounted || MountRules.CanRideIn(place));
        }

        /// <summary>Main button caption: "Assobiando…", "Descer" or "Montaria".</summary>
        public static string ButtonKey(bool mounted, bool whistling)
        {
            return whistling ? MountTexts.HudWhistling : mounted ? MountTexts.HudDismount : MountTexts.HudMount;
        }

        // ------------------------------------------------------------------

        /// <summary>Updates every element from the rider (null rider = hidden).</summary>
        public void Refresh(IMountRider rider, bool hudVisible)
        {
            Bind(rider);
            OwnedMount owned = rider?.ActiveMount;
            MountDefinition definition = rider?.Definition;
            MountSession session = rider?.Session;
            bool mounted = session != null && session.IsMounted;
            bool show = rider != null && ShouldShow(owned != null && definition != null, GameplayPause.IsPaused, hudVisible, rider.Place, mounted);
            SetVisible(show);
            if (!show)
            {
                if (rider != null)
                {
                    rider.TouchMountHeld = false;
                }

                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            if (_stableButton != null)
            {
                SetText(_stableButton, MountTexts.Get(localization, MountTexts.StableButton));
            }

            bool whistling = session.IsWhistling;
            SetText(_label, MountTexts.Get(localization, ButtonKey(mounted, whistling)));
            string hint = TouchControlsPreferences.HasTouchscreen ? (mounted ? string.Empty : MountTexts.Get(localization, MountTexts.HudHoldHint)) : rider.KeyHint;
            SetText(_hint, hint);
            _fill.style.height = Length.Percent(whistling ? session.WhistleProgress * 100f : 0f);
            _button.EnableInClassList(MountedClass, mounted);
            _button.EnableInClassList(EmptyClass, owned.IsTired);

            MountAbilityDefinition ability = definition.Ability;
            bool showAbility = mounted && ability != null;
            _abilityButton.style.display = showAbility ? DisplayStyle.Flex : DisplayStyle.None;
            if (showAbility)
            {
                float remaining = session.AbilityRemaining(rider.Now);
                string name = MountTexts.Get(localization, ability.NameKey);
                bool safe = rider.Place == MountPlace.SafeZone;
                SetText(_abilityLabel, remaining > 0f
                    ? MountTexts.Format(localization, MountTexts.HudAbilityWait, name, Mathf.CeilToInt(remaining))
                    : MountTexts.Format(localization, MountTexts.HudAbilityReady, name));
                _abilityButton.SetEnabled(remaining <= 0f && !safe);
            }

            _bars.style.display = mounted ? DisplayStyle.Flex : DisplayStyle.None;
            if (mounted)
            {
                bool trot = rider.Place == MountPlace.SafeZone;
                _staminaBar.EnableInClassList(TrotClass, trot || session.IsExhausted);
                _staminaFill.style.width = Length.Percent(session.StaminaFraction * 100f);
                SetText(_staminaCaption, MountTexts.Get(localization, trot ? MountTexts.HudTrot : session.IsGalloping ? MountTexts.HudGallop : MountTexts.HudStamina));
                float max = MountCollection.MaxHealthOf(owned);
                _healthFill.style.width = Length.Percent(max > 0f ? Mathf.Clamp01(MountCollection.HealthOf(owned) / max) * 100f : 0f);
                RefreshLoad(localization);
            }
        }

        private void RefreshLoad(LocalizationManager localization)
        {
            bool show = Rapadura.Gameplay.Inventory.EncumbranceTracker.HasCurrent && Rapadura.Gameplay.Inventory.EncumbranceTracker.Current.Mounted;
            DisplayStyle display = show ? DisplayStyle.Flex : DisplayStyle.None;
            _loadCaption.style.display = display;
            _loadBar.style.display = display;
            if (!show)
            {
                return;
            }

            Rapadura.Gameplay.Inventory.EncumbranceSnapshot load = Rapadura.Gameplay.Inventory.EncumbranceTracker.Current;
            SetText(_loadCaption, Rapadura.Gameplay.Inventory.EncumbranceTexts.Format(localization, Rapadura.Gameplay.Inventory.EncumbranceTexts.HudMountLoad,
                Rapadura.Gameplay.Inventory.EncumbranceTexts.Number(load.Load), Rapadura.Gameplay.Inventory.EncumbranceTexts.Number(load.Capacity)));
            _loadFill.style.width = Length.Percent(Mathf.Clamp01(load.Ratio) * 100f);
            Rapadura.Gameplay.Inventory.LoadBand band = load.Band;
            _loadBar.EnableInClassList(LoadHeavyClass, band == Rapadura.Gameplay.Inventory.LoadBand.Heavy);
            _loadBar.EnableInClassList(LoadOverClass, band == Rapadura.Gameplay.Inventory.LoadBand.Overloaded);
        }

        private void SetVisible(bool visible)
        {
            DisplayStyle display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            _button.parent.style.display = display;
            if (_stableButton != null)
            {
                _stableButton.style.display = display;
            }
            if (!visible)
            {
                _bars.style.display = DisplayStyle.None;
            }
        }

        private void SetHeld(bool held)
        {
            if (_rider != null)
            {
                _rider.TouchMountHeld = held;
            }
        }

        private static void SetText(TextElement label, string text)
        {
            if (label.text != text)
            {
                label.text = text;
            }
        }

        private static Label Text(string className)
        {
            var label = new Label { pickingMode = PickingMode.Ignore };
            label.AddToClassList(className);
            if (className == NoticeName)
            {
                label.name = NoticeName;
            }

            return label;
        }

        private static VisualElement Bar(string name, out VisualElement fill)
        {
            var bar = new VisualElement { name = name, pickingMode = PickingMode.Ignore };
            bar.AddToClassList("mount-bar");
            bar.AddToClassList(name);
            fill = new VisualElement { pickingMode = PickingMode.Ignore };
            fill.AddToClassList("mount-bar-fill");
            bar.Add(fill);
            return bar;
        }

        public static bool ReducedMotion()
        {
            ServiceLocator.TryGet(out AccessibilitySettings accessibility);
            return UiMotionPreferences.IsReducedMotion(accessibility);
        }
    }

    /// <summary>Loads Resources/UI/Mounts.uss and marks the layer with the theme class (same as <c>DuoStrikeStyles</c>).</summary>
    public static class MountHudStyles
    {
        public const string ResourcePath = "UI/Mounts";
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

        public static void ApplyTo(VisualElement root, bool reducedMotion)
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
            UiMotionPreferences.ApplyTo(root, reducedMotion);
        }
    }
}
