using System.Collections.Generic;
using Rapadura.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// Look-only layer of the title screen (MainMenuView.uxml), kept out of
    /// <see cref="MainMenuController"/> so the menu's behaviour stays where it is:
    /// <list type="bullet">
    /// <item>paints the illustrated "fim de tarde no engenho" backdrop's sky gradient (USS has no
    /// gradients; see <see cref="UiGradientTexture"/>);</item>
    /// <item>plays a short entrance — logo and buttons fade/slide in, buttons staggered — by adding
    /// <see cref="EnteringClass"/> and removing it a frame later so the USS transitions run;</item>
    /// <item>drifts the clouds slowly across the sky.</item>
    /// </list>
    /// With "Reduzir Movimento" or "Reduzir Flashes" on (<see cref="UiMotionPreferences"/>) there is no entrance and no drift:
    /// the screen appears in its final state and stays still. The rules are the public statics, so
    /// EditMode tests can check them without a panel.
    /// </summary>
    public sealed class TitleScreenDecor
    {
        /// <summary>On the root while the entrance is pending: logo/buttons start transparent and offset.</summary>
        public const string EnteringClass = "main-menu--entering";

        public const string SkyElementName = "main-menu-sky";
        public const string CloudClass = "main-menu-cloud";

        /// <summary>Seconds between one button's entrance and the next.</summary>
        public const float ButtonStaggerSeconds = 0.06f;

        /// <summary>Logo first, then buttons after this delay.</summary>
        public const float ButtonsStartDelaySeconds = 0.18f;

        /// <summary>Cloud speed, as a fraction of the sky width per second (a full crossing takes ~100 s).</summary>
        public const float CloudSpeedPerSecond = 0.01f;

        private const long DriftIntervalMs = 50;

        /// <summary>USS class of the menu buttons whose entrance is staggered.</summary>
        public const string ButtonClass = "main-menu-button";

        /// <summary>Warm late-afternoon sky over the sugar-cane fields, top to bottom.</summary>
        public static readonly Color[] SkyStops =
        {
            new Color32(58, 40, 78, 255),   // dusk violet
            new Color32(150, 72, 62, 255),  // clay rose
            new Color32(232, 132, 56, 255), // caramel
            new Color32(250, 200, 110, 255) // gold at the horizon
        };

        private VisualElement _root;
        private Texture2D _skyTexture;
        private readonly List<VisualElement> _clouds = new List<VisualElement>();
        private readonly List<float> _cloudPhase = new List<float>();
        private IVisualElementScheduledItem _driftItem;
        private IVisualElementScheduledItem _enterItem;
        private bool _reducedMotion;
        private float _lastDriftTime;

        public bool ReducedMotion => _reducedMotion;

        /// <summary>Delay before the <paramref name="index"/>-th menu button slides in.</summary>
        public static float ButtonEntranceDelay(int index) => ButtonsStartDelaySeconds + Mathf.Max(0, index) * ButtonStaggerSeconds;

        /// <summary>Whether to play the entrance: never with reduced motion, and only on a live panel (else nothing would remove the class).</summary>
        public static bool ShouldPlayEntrance(bool reducedMotion, bool attachedToPanel) => !reducedMotion && attachedToPanel;

        /// <summary>
        /// Horizontal cloud offset in percent of the sky width for a cloud starting at <paramref name="phase"/>
        /// (0..1), after <paramref name="seconds"/>; wraps from 110% back to -30% so clouds re-enter from the left.
        /// </summary>
        public static float CloudOffsetPercent(float phase, float seconds)
        {
            const float start = -30f;
            const float span = 140f;
            float travelled = Mathf.Repeat(phase + seconds * CloudSpeedPerSecond * (100f / span), 1f);
            return start + travelled * span;
        }

        public void Attach(VisualElement documentRoot, VisualElement screenRoot, bool reducedMotion)
        {
            Detach();

            _root = screenRoot ?? documentRoot;
            if (_root == null)
            {
                return;
            }

            _reducedMotion = UiMotionPreferences.ApplyTo(_root, reducedMotion);

            VisualElement sky = documentRoot?.Q<VisualElement>(SkyElementName);
            _skyTexture = UiGradientTexture.ApplyVertical(sky, SkyStops);

            _clouds.Clear();
            _cloudPhase.Clear();
            documentRoot?.Query<VisualElement>(className: CloudClass).ForEach(cloud =>
            {
                _cloudPhase.Add(_clouds.Count * 0.37f % 1f);
                _clouds.Add(cloud);
            });

            ApplyButtonDelays(documentRoot);
            PositionClouds(0f);

            if (!_reducedMotion && _clouds.Count > 0)
            {
                _lastDriftTime = Time.unscaledTime;
                float elapsed = 0f;
                _driftItem = _root.schedule.Execute(() =>
                {
                    float now = Time.unscaledTime;
                    elapsed += Mathf.Clamp(now - _lastDriftTime, 0f, 0.25f);
                    _lastDriftTime = now;
                    PositionClouds(elapsed);
                }).Every(DriftIntervalMs);
            }

            PlayEntrance();
        }

        /// <summary>
        /// Re-reads the reduced-motion choice (AccessibilitySettings can register after OnEnable, or be
        /// changed in Configurações while the title is up): switching it on stops the drift and
        /// finishes the entrance at once. Switching it off only takes effect on the next Attach.
        /// </summary>
        public void RefreshMotion(bool reducedMotion)
        {
            if (_root == null || reducedMotion == _reducedMotion)
            {
                return;
            }

            _reducedMotion = UiMotionPreferences.ApplyTo(_root, reducedMotion);
            if (_reducedMotion)
            {
                _driftItem?.Pause();
                _enterItem?.Pause();
                _root.RemoveFromClassList(EnteringClass);
            }
        }

        /// <summary>Replays the entrance (the title shown again after returning from the game).</summary>
        public void PlayEntrance()
        {
            if (_root == null)
            {
                return;
            }

            _enterItem?.Pause();
            if (!ShouldPlayEntrance(_reducedMotion, _root.panel != null))
            {
                _root.RemoveFromClassList(EnteringClass);
                return;
            }

            _root.AddToClassList(EnteringClass);
            // Removed on a later frame: the start state must be resolved once for the transition to run.
            _enterItem = _root.schedule.Execute(() => _root?.RemoveFromClassList(EnteringClass)).StartingIn(40);
        }

        public void Detach()
        {
            _driftItem?.Pause();
            _driftItem = null;
            _enterItem?.Pause();
            _enterItem = null;
            _root?.RemoveFromClassList(EnteringClass);
            _clouds.Clear();
            _cloudPhase.Clear();
            UiGradientTexture.Release(_skyTexture);
            _skyTexture = null;
            _root = null;
        }

        private void PositionClouds(float seconds)
        {
            for (int i = 0; i < _clouds.Count; i++)
            {
                _clouds[i].style.left = Length.Percent(CloudOffsetPercent(_cloudPhase[i], seconds));
            }
        }

        private static void ApplyButtonDelays(VisualElement documentRoot)
        {
            int index = 0;
            documentRoot?.Query<Button>(className: ButtonClass).ForEach(button =>
            {
                // Same order as .main-menu-button's transition-property: only opacity (the entrance,
                // last) is delayed, so hover/focus/press feedback stays instant.
                float delay = ButtonEntranceDelay(index++);
                button.style.transitionDelay = new List<TimeValue>
                {
                    new TimeValue(0f), new TimeValue(0f), new TimeValue(0f), new TimeValue(0f), new TimeValue(0f),
                    new TimeValue(delay),
                };
            });
        }
    }
}
