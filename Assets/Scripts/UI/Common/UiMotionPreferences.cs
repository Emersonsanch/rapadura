using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using UnityEngine.UIElements;

namespace Rapadura.UI.Common
{
    /// <summary>
    /// Reduced-motion rule for UI animation (entrance fades, drifting backdrop, button scale): on with
    /// the accessibility toggle "Reduzir Movimento" (<see cref="AccessibilitySettings.ReduceMotion"/>),
    /// and still with "Reduzir Flashes", which calmed UI movement before the motion option existed
    /// (<see cref="AccessibilitySettings.SuppressUiMotion"/>, shared with the story, boss and tutorial
    /// overlays). Screens that use
    /// <c>Common/RapaduraTheme.uss</c> toggle <see cref="ReducedMotionClass"/> on their root, which
    /// makes every themed transition instant.
    /// </summary>
    public static class UiMotionPreferences
    {
        /// <summary>USS class (see RapaduraTheme.uss) that zeroes the themed transitions under it.</summary>
        public const string ReducedMotionClass = "rap-reduced-motion";

        /// <summary>True when <paramref name="settings"/> asks for less motion (or fewer flashes); null settings = full motion.</summary>
        public static bool IsReducedMotion(AccessibilitySettings settings) => settings != null && settings.SuppressUiMotion;

        /// <summary><see cref="IsReducedMotion(AccessibilitySettings)"/> for the live settings in the <see cref="ServiceLocator"/>.</summary>
        public static bool IsReducedMotion()
        {
            ServiceLocator.TryGet(out AccessibilitySettings settings);
            return IsReducedMotion(settings);
        }

        /// <summary>Toggles <see cref="ReducedMotionClass"/> on <paramref name="root"/> and returns the state applied.</summary>
        public static bool ApplyTo(VisualElement root, bool reducedMotion)
        {
            root?.EnableInClassList(ReducedMotionClass, reducedMotion);
            return reducedMotion;
        }
    }
}
