using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Common
{
    /// <summary>
    /// Small, stateless framework for one-shot visual feedback ("flash"/pulse) on any
    /// <see cref="VisualElement"/> — clicked buttons, damage flashes on the HUD, invalid-input
    /// shakes, etc. Any controller can call <see cref="Pulse"/> without owning a
    /// MonoBehaviour/Update loop; it rides UI Toolkit's own
    /// <see cref="VisualElement.schedule"/> (the same mechanism transitions in
    /// HudView.uss/TooltipView.uss already rely on for width/opacity), so it works from EditMode
    /// tests too as long as the element belongs to a live panel.
    ///
    /// Implementation note: rather than driving <c>experimental.animation</c> tweens (which need a
    /// panel update loop to tick and are awkward to unit test), this flips
    /// <c>element.style.backgroundColor</c> to <paramref name="color"/> immediately and lets a USS
    /// transition (declared once, inline, via <see cref="EnsureFlashTransition"/>) ease it back to
    /// its original value — the same "toggle a class/value, let USS transition it" pattern used by
    /// <c>.hud-bar-fill</c>'s <c>width</c> transition in HudView.uss.
    /// </summary>
    public static class UiFeedbackUtility
    {
        private const float DefaultDuration = 0.25f;

        /// <summary>
        /// Flashes <paramref name="element"/>'s background to <paramref name="color"/> then eases it
        /// back to its previous background color over <paramref name="duration"/> seconds.
        /// Safe to call repeatedly (e.g. rapid clicks) — each call restarts the fade from the flash
        /// color rather than stacking timers.
        /// </summary>
        public static void Pulse(VisualElement element, Color color, float duration = DefaultDuration)
        {
            if (element == null)
            {
                return;
            }

            duration = Mathf.Max(0.01f, duration);

            StyleColor originalBackground = element.style.backgroundColor;

            // Jump to the flash color instantly (no transition on the way in), then re-enable the
            // transition and set the target back to the original color one frame later so USS
            // eases *that* change — mirrors the "set final value, let USS animate toward it" idiom
            // used by .hud-bar-fill's width transition in HudView.uss.
            ClearFlashTransition(element);
            element.style.backgroundColor = color;

            element.schedule.Execute(() =>
            {
                EnsureFlashTransition(element, duration);
                element.style.backgroundColor = originalBackground;
            }).ExecuteLater(1);
        }

        /// <summary>
        /// Briefly scales <paramref name="element"/> up then back to 1x — a generic "pop" for
        /// clicked buttons/skill nodes. Uses <c>experimental.animation</c> directly since transform
        /// scale has no USS transition-friendly single value to toggle here.
        /// </summary>
        public static void PulseScale(VisualElement element, float scaleAmount = 1.15f, float duration = DefaultDuration)
        {
            if (element == null)
            {
                return;
            }

            duration = Mathf.Max(0.01f, duration);

            element.experimental.animation.Start(1f, scaleAmount, (int)(duration * 500f), (e, v) =>
            {
                e.transform.scale = new Vector3(v, v, 1f);
            }).OnCompleted(() =>
            {
                element.experimental.animation.Start(scaleAmount, 1f, (int)(duration * 500f), (e, v) =>
                {
                    e.transform.scale = new Vector3(v, v, 1f);
                });
            });
        }

        /// <summary>
        /// Registers the inline USS transition for background-color once per element so repeated
        /// <see cref="Pulse"/> calls don't keep re-allocating style lists.
        /// </summary>
        private static void EnsureFlashTransition(VisualElement element, float duration)
        {
            element.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("background-color") };
            element.style.transitionDuration = new List<TimeValue> { new TimeValue(duration, TimeUnit.Second) };
            element.style.transitionTimingFunction = new List<EasingFunction> { new EasingFunction(EasingMode.EaseOut) };
        }

        /// <summary>Removes any transition so the initial flash color applies instantly, not eased in.</summary>
        private static void ClearFlashTransition(VisualElement element)
        {
            element.style.transitionProperty = new List<StylePropertyName>();
        }
    }
}
