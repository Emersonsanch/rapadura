using Rapadura.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Encounter
{
    /// <summary>
    /// Look of the code-built arena overlays (<see cref="BossIntroBannerController"/>,
    /// <see cref="BagFullToastController"/>): their self-bootstrapped documents have no UXML, so the sheet
    /// <c>Resources/UI/EncounterOverlays.uss</c> (which <c>@import</c>s Common/RapaduraTheme.uss) is
    /// loaded at runtime and attached to the overlay root together with <see cref="ThemeClass"/>, where the
    /// theme declares its <c>--rap-*</c> palette. Colours, sizes, borders and fade durations live in that
    /// USS; the controllers only toggle display / opacity and the fade-speed classes.
    /// </summary>
    public static class EncounterOverlayStyles
    {
        /// <summary><see cref="Resources.Load(string)"/> path of the overlay sheet.</summary>
        public const string ResourcePath = "UI/EncounterOverlays";

        /// <summary>Root class that declares the theme variables (RapaduraTheme.uss).</summary>
        public const string ThemeClass = "rap-theme";

        private static StyleSheet _sheet;

        /// <summary>The overlay sheet (cached), or null when it is missing from Resources.</summary>
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

        /// <summary>Attaches the overlay sheet and the theme / reduced-motion classes to <paramref name="root"/>.</summary>
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
