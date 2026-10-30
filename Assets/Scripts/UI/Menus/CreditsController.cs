using System;
using Rapadura.Core.Systems;
using Rapadura.Core.Versioning;
using Rapadura.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// Drives the Créditos screen, opened from <see cref="MainMenuController"/> and rolled after the
    /// campaign epilogue by <c>StoryPanelController</c> (which only calls <see cref="Show"/> and listens to
    /// <see cref="Closed"/>). Same UI Toolkit pattern as SettingsMenuController: one <see cref="UIDocument"/>
    /// (CreditsView.uxml, which pulls in the shared theme and CreditsView.uss via &lt;Style&gt;), elements
    /// resolved once, hidden on enable until <see cref="Show"/> is called. Every heading and line comes
    /// from <see cref="CreditsContent"/> as a <c>ui.credits.*</c> key (en + pt-BR), rebuilt on a language switch.
    ///
    /// The roll: after <see cref="AutoScrollDelaySeconds"/> the list scrolls itself at
    /// <see cref="AutoScrollSpeed"/> (real time, the game is paused meanwhile) until the closing line; a
    /// wheel, drag or gamepad up/down hands the scroll back to the player. With "Reduzir Movimento" /
    /// "Reduzir Flashes" (<see cref="Rapadura.Core.Accessibility.AccessibilitySettings.SuppressUiMotion"/>)
    /// nothing moves on its own. Closes on the "Voltar" button, a tap/click anywhere that is not a drag
    /// (ignored for the first <see cref="TapGuardSeconds"/>, so the tap that ended the epilogue can't skip
    /// it), or the UI cancel action (Esc / gamepad B / Android back), then raises <see cref="Closed"/>.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class CreditsController : MonoBehaviour
    {
        /// <summary>Seconds (real time) the roll waits at the top before it starts scrolling.</summary>
        public const float AutoScrollDelaySeconds = 1.5f;

        /// <summary>Auto-scroll speed in panel pixels per real second.</summary>
        public const float AutoScrollSpeed = 32f;

        /// <summary>A tap closes the credits only after they have been open this long (real seconds).</summary>
        public const float TapGuardSeconds = 0.6f;

        /// <summary>Pointer travel (px) above which a press is a drag (scroll), not a tap (close).</summary>
        public const float TapSlopPixels = 12f;

        /// <summary>Pixels scrolled per gamepad / keyboard up-down step.</summary>
        public const float NavigationScrollStep = 80f;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private ScrollView _scroll;
        private VisualElement _content;
        private Label _versionLabel;
        private Button _backButton;
        private readonly UiDocumentLocalizer _localizer = new UiDocumentLocalizer();

        private float _shownAt;
        private float _autoScrollDelay;
        private int _pressPointerId = -1;
        private Vector2 _pressPosition;
        private bool _pressDragged;

        public bool IsOpen { get; private set; }

        /// <summary>True while the roll is (or is about to start) scrolling itself.</summary>
        public bool IsAutoScrolling { get; private set; }

        /// <summary>Raised after the screen hides itself (button, tap or cancel), not on OnDisable.</summary>
        public event Action Closed;

        /// <summary>Pure rule: the credits only scroll themselves with full UI motion.</summary>
        public static bool ShouldAutoScroll(bool reducedMotion) => !reducedMotion;

        /// <summary>One auto-scroll frame: <paramref name="current"/> moved by <paramref name="speed"/> × <paramref name="deltaTime"/>, clamped to [0, <paramref name="max"/>].</summary>
        public static float NextAutoScrollOffset(float current, float max, float deltaTime, float speed)
        {
            return Mathf.Clamp(current + Mathf.Max(0f, deltaTime) * Mathf.Max(0f, speed), 0f, Mathf.Max(0f, max));
        }

        /// <summary>Pure rule: a press/release closes the credits when it did not drag and the guard time has passed.</summary>
        public static bool IsTapToClose(float travelPixels, float secondsOpen) =>
            travelPixels <= TapSlopPixels && secondsOpen >= TapGuardSeconds;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            BindElements();
            RegisterCallbacks();
            _localizer.Attach(_uiDocument.rootVisualElement, BuildContent);
            BuildContent();
            SetVisible(false);
        }

        private void OnDisable()
        {
            _localizer.Detach();
            UnregisterCallbacks();
            IsAutoScrolling = false;
            GameplayPause.Release(this);
        }

        private void BindElements()
        {
            VisualElement rootVisualElement = _uiDocument.rootVisualElement;

            _root = rootVisualElement.Q<VisualElement>("credits-root");
            _scroll = rootVisualElement.Q<ScrollView>("credits-scroll");
            _content = rootVisualElement.Q<VisualElement>("credits-content");
            _versionLabel = rootVisualElement.Q<Label>("credits-version");
            _backButton = rootVisualElement.Q<Button>("credits-back-button");
        }

        private void RegisterCallbacks()
        {
            if (_backButton != null) _backButton.clicked += Hide;
            if (_root == null)
            {
                return;
            }

            _root.RegisterCallback<NavigationCancelEvent>(OnNavigationCancel);
            _root.RegisterCallback<NavigationMoveEvent>(OnNavigationMove);
            _root.RegisterCallback<WheelEvent>(OnWheel, TrickleDown.TrickleDown);
            _root.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            _root.RegisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
            _root.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
        }

        private void UnregisterCallbacks()
        {
            if (_backButton != null) _backButton.clicked -= Hide;
            if (_root == null)
            {
                return;
            }

            _root.UnregisterCallback<NavigationCancelEvent>(OnNavigationCancel);
            _root.UnregisterCallback<NavigationMoveEvent>(OnNavigationMove);
            _root.UnregisterCallback<WheelEvent>(OnWheel, TrickleDown.TrickleDown);
            _root.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            _root.UnregisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
            _root.UnregisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
        }

        private void BuildContent()
        {
            if (_versionLabel != null)
            {
                _versionLabel.text = $"Rapadura {BuildVersion.DisplayLabel}";
            }

            if (_content == null)
            {
                return;
            }

            _content.Clear();

            foreach (CreditsSection section in CreditsContent.Sections)
            {
                var sectionElement = new VisualElement();
                sectionElement.AddToClassList("credits-section");
                if (section.TitleKey == UiTexts.CreditsSectionClosing)
                {
                    sectionElement.AddToClassList("credits-section--closing");
                }

                var title = new Label(section.GetTitle(_localizer.Manager));
                title.AddToClassList("credits-section-title");
                sectionElement.Add(title);

                for (int i = 0; i < section.LineKeys.Count; i++)
                {
                    var lineLabel = new Label(section.GetLine(_localizer.Manager, i));
                    lineLabel.AddToClassList("credits-line");
                    sectionElement.Add(lineLabel);
                }

                _content.Add(sectionElement);
            }
        }

        private void Update()
        {
            if (!IsOpen || !IsAutoScrolling || _scroll == null)
            {
                return;
            }

            float deltaTime = Time.unscaledDeltaTime;
            if (_autoScrollDelay > 0f)
            {
                _autoScrollDelay -= deltaTime;
                return;
            }

            float max = _scroll.verticalScroller.highValue;
            if (float.IsNaN(max) || max <= 0f)
            {
                // Layout not resolved yet (or everything already fits): nothing to roll.
                if (_scroll.contentContainer.layout.height > 0f && !float.IsNaN(max))
                {
                    IsAutoScrolling = false;
                }

                return;
            }

            float next = NextAutoScrollOffset(_scroll.scrollOffset.y, max, deltaTime, AutoScrollSpeed);
            _scroll.scrollOffset = new Vector2(_scroll.scrollOffset.x, next);
            if (next >= max)
            {
                IsAutoScrolling = false;
            }
        }

        /// <summary>The player took over the scroll: the roll stops for good (until the next <see cref="Show"/>).</summary>
        private void StopAutoScroll() => IsAutoScrolling = false;

        private void OnNavigationCancel(NavigationCancelEvent evt)
        {
            if (!IsOpen)
            {
                return;
            }

            evt.StopPropagation();
            Hide();
        }

        private void OnNavigationMove(NavigationMoveEvent evt)
        {
            if (!IsOpen || _scroll == null)
            {
                return;
            }

            float step = evt.direction == NavigationMoveEvent.Direction.Up ? -NavigationScrollStep
                : evt.direction == NavigationMoveEvent.Direction.Down ? NavigationScrollStep : 0f;
            if (step == 0f)
            {
                return;
            }

            StopAutoScroll();
            float max = Mathf.Max(0f, _scroll.verticalScroller.highValue);
            _scroll.scrollOffset = new Vector2(_scroll.scrollOffset.x, Mathf.Clamp(_scroll.scrollOffset.y + step, 0f, max));
            evt.StopPropagation();
        }

        private void OnWheel(WheelEvent evt) => StopAutoScroll();

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!IsOpen || IsOnScrollbar(evt.target as VisualElement))
            {
                _pressPointerId = -1;
                return;
            }

            _pressPointerId = evt.pointerId;
            _pressPosition = evt.position;
            _pressDragged = false;
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (evt.pointerId != _pressPointerId || _pressDragged)
            {
                return;
            }

            if (Vector2.Distance(_pressPosition, evt.position) > TapSlopPixels)
            {
                _pressDragged = true;
                StopAutoScroll();
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!IsOpen || evt.pointerId != _pressPointerId)
            {
                return;
            }

            _pressPointerId = -1;
            float travel = _pressDragged ? float.MaxValue : Vector2.Distance(_pressPosition, evt.position);
            if (IsTapToClose(travel, Time.unscaledTime - _shownAt))
            {
                Hide();
            }
        }

        private bool IsOnScrollbar(VisualElement target)
        {
            Scroller scroller = _scroll?.verticalScroller;
            for (VisualElement element = target; element != null; element = element.parent)
            {
                if (element == scroller)
                {
                    return true;
                }
            }

            return false;
        }

        public void Show()
        {
            _localizer.Refresh();
            BuildContent();

            bool reducedMotion = UiMotionPreferences.IsReducedMotion();
            UiMotionPreferences.ApplyTo(_root, reducedMotion);
            IsAutoScrolling = ShouldAutoScroll(reducedMotion);
            _autoScrollDelay = AutoScrollDelaySeconds;
            _shownAt = Time.unscaledTime;
            _pressPointerId = -1;

            SetVisible(true);
            if (_scroll != null) _scroll.scrollOffset = Vector2.zero;
            _backButton?.Focus();
        }

        public void Hide()
        {
            bool wasOpen = IsOpen;
            IsAutoScrolling = false;
            _pressPointerId = -1;
            SetVisible(false);

            if (wasOpen)
            {
                Closed?.Invoke();
            }
        }

        private void SetVisible(bool visible)
        {
            IsOpen = visible;

            if (_root != null)
            {
                _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (visible)
            {
                GameplayPause.Acquire(this);
            }
            else
            {
                GameplayPause.Release(this);
            }
        }
    }
}
