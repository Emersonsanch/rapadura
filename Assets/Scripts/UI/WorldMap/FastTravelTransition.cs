using System;
using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Systems;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// The short cosmetic trip of the carro de boi ("Viagem rápida"): the map HUD fades to a warm dusk with the line
    /// "O carro de boi vai rangendo pela estrada até ...", the token is placed in the destination while the screen is
    /// covered (<see cref="Play"/>'s <c>arrive</c> callback), then it fades back. Built in code on the HUD's document
    /// (no UXML, inline styles), on unscaled time, holding <see cref="GameplayPause"/> for the whole fade so nothing
    /// on the map ticks meanwhile. It never moves the token along a road, so there is no encounter roll, road event or
    /// road chatter. With "Reduzir Movimento" (or no HUD document) the arrival is instant.
    /// </summary>
    [DisallowMultipleComponent]
    public class FastTravelTransition : MonoBehaviour
    {
        public const string OverlayName = "wm-fast-travel-transition";
        public const float FadeSeconds = 0.35f;
        public const float HoldSeconds = 0.6f;
        public const float TotalSeconds = FadeSeconds * 2f + HoldSeconds;

        private UIDocument _document;
        private VisualElement _overlay;
        private Label _label;
        private Action _arrive;
        private float _elapsed;
        private bool _playing;
        private bool _arrived;

        public bool IsPlaying => _playing;

        /// <summary>Overlay opacity at <paramref name="t"/> seconds into the trip (pure, tested): fade in, hold, fade out.</summary>
        public static float Opacity(float t)
        {
            if (t <= 0f || t >= TotalSeconds)
            {
                return 0f;
            }

            if (t < FadeSeconds)
            {
                return t / FadeSeconds;
            }

            if (t <= FadeSeconds + HoldSeconds)
            {
                return 1f;
            }

            return Mathf.Clamp01((TotalSeconds - t) / FadeSeconds);
        }

        /// <summary>The moment the token is moved: the screen is fully covered.</summary>
        public static bool ShouldArrive(float t) => t >= FadeSeconds;

        private static bool ReduceMotion => ServiceLocator.TryGet(out AccessibilitySettings settings) && settings != null && settings.ReduceMotion;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        private void OnDisable()
        {
            Finish();
            _overlay?.RemoveFromHierarchy();
            _overlay = null;
        }

        /// <summary>Plays the trip with <paramref name="line"/>; <paramref name="arrive"/> runs once, mid-way (or right away without a screen).</summary>
        public void Play(string line, Action arrive)
        {
            if (_playing)
            {
                // A second trip while one plays: land the first one now.
                Finish();
            }

            if (ReduceMotion || !EnsureOverlay())
            {
                arrive?.Invoke();
                return;
            }

            _arrive = arrive;
            _arrived = false;
            _elapsed = 0f;
            _playing = true;
            _label.text = line ?? string.Empty;
            _overlay.style.opacity = 0f;
            _overlay.style.display = DisplayStyle.Flex;
            GameplayPause.Acquire(this);
        }

        private void Update()
        {
            if (!_playing)
            {
                return;
            }

            _elapsed += Time.unscaledDeltaTime;
            if (!_arrived && ShouldArrive(_elapsed))
            {
                Arrive();
            }

            if (_overlay != null)
            {
                _overlay.style.opacity = Opacity(_elapsed);
            }

            if (_elapsed >= TotalSeconds)
            {
                Finish();
            }
        }

        private void Arrive()
        {
            _arrived = true;
            Action arrive = _arrive;
            _arrive = null;
            try
            {
                arrive?.Invoke();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private void Finish()
        {
            if (!_playing)
            {
                return;
            }

            if (!_arrived)
            {
                Arrive();
            }

            _playing = false;
            if (_overlay != null)
            {
                _overlay.style.display = DisplayStyle.None;
            }

            GameplayPause.Release(this);
        }

        private bool EnsureOverlay()
        {
            if (_document == null || _document.rootVisualElement == null)
            {
                return false;
            }

            VisualElement root = _document.rootVisualElement.Q<VisualElement>("wm-hud-root") ?? _document.rootVisualElement;
            if (_overlay != null && _overlay.parent == root)
            {
                return true;
            }

            _overlay?.RemoveFromHierarchy();
            _overlay = new VisualElement { name = OverlayName, pickingMode = PickingMode.Position };
            _overlay.style.position = Position.Absolute;
            _overlay.style.left = 0f;
            _overlay.style.right = 0f;
            _overlay.style.top = 0f;
            _overlay.style.bottom = 0f;
            _overlay.style.backgroundColor = new Color(0.16f, 0.09f, 0.05f, 0.96f);
            _overlay.style.alignItems = Align.Center;
            _overlay.style.justifyContent = Justify.Center;
            _overlay.style.display = DisplayStyle.None;

            _label = new Label { name = "wm-fast-travel-transition-line" };
            _label.style.fontSize = 22f;
            _label.style.color = new Color(0.98f, 0.9f, 0.72f);
            _label.style.unityFontStyleAndWeight = FontStyle.Italic;
            _label.style.whiteSpace = WhiteSpace.Normal;
            _label.style.unityTextAlign = TextAnchor.MiddleCenter;
            _label.style.maxWidth = new Length(80f, LengthUnit.Percent);
            _overlay.Add(_label);
            root.Add(_overlay);
            return true;
        }
    }
}
