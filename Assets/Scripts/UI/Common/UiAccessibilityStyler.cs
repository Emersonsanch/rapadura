using System;
using System.Runtime.CompilerServices;
using Rapadura.Core.Accessibility;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Common
{
    /// <summary>Immutable snapshot of the accessibility values the UI styler cares about.</summary>
    public readonly struct UiAccessibilityState
    {
        public static readonly UiAccessibilityState Default = new UiAccessibilityState(1f, false, ColorblindMode.None);

        public readonly float FontScale;
        public readonly bool HighContrast;
        public readonly ColorblindMode ColorblindMode;

        public UiAccessibilityState(float fontScale, bool highContrast, ColorblindMode colorblindMode)
        {
            FontScale = fontScale;
            HighContrast = highContrast;
            ColorblindMode = colorblindMode;
        }

        public static UiAccessibilityState From(AccessibilitySettings settings)
        {
            return settings == null
                ? Default
                : new UiAccessibilityState(settings.FontScale, settings.HighContrast, settings.ActiveColorblindMode);
        }
    }

    /// <summary>
    /// Pure (MonoBehaviour-free) logic behind <see cref="UiAccessibilityApplier"/>: applies the
    /// accessibility USS classes and font scale to one UI Toolkit tree. Kept separate so EditMode
    /// tests can exercise it on plain <see cref="VisualElement"/>s without a panel.
    ///
    /// FONT SCALE: almost every screen's USS sets an explicit <c>font-size</c> in px on its own
    /// classes, so changing the root's font-size would not cascade. Instead every
    /// <see cref="TextElement"/> (Label, Button, field text...) gets an inline font-size =
    /// base * scale, where "base" is the size it had before we touched it (remembered per element
    /// in a weak table so rebuilt/discarded elements don't leak). At scale 1 the inline value is
    /// removed, restoring the USS size exactly. If something else changes an element's size while
    /// scaled (the resolved size no longer equals what we applied), that new size becomes the base.
    ///
    /// COLORBLIND / HIGH CONTRAST: toggles USS classes on the document root
    /// (<see cref="ColorblindClassPrefix"/> + mode, <see cref="HighContrastClass"/>) that the
    /// shared Resources/UI/AccessibilityTheme.uss (or any screen's own USS) can target. Rarity
    /// colors set from code keep using <see cref="ColorblindPaletteMap"/> directly (see TooltipController).
    /// </summary>
    public class UiAccessibilityStyler
    {
        public const string RootClass = "a11y-root";
        public const string HighContrastClass = "a11y-high-contrast";
        public const string FontScaledClass = "a11y-font-scaled";
        public const string ColorblindClassPrefix = "a11y-colorblind-";

        private const float ScaleEpsilon = 0.001f;
        private const float SizeEpsilon = 0.01f;

        private sealed class FontRecord
        {
            public float BaseSize;
            public float AppliedSize;
            /// <summary>Size applied before the latest change; a resolved size equal to it is just a not-yet-restyled frame, not an external change.</summary>
            public float PreviousAppliedSize;
            /// <summary>Inline font-size the element had before we scaled it (usually keyword Null), restored at scale 1.</summary>
            public StyleLength OriginalInline;
        }

        private readonly ConditionalWeakTable<VisualElement, FontRecord> _fontRecords = new ConditionalWeakTable<VisualElement, FontRecord>();
        private readonly Func<VisualElement, float> _currentFontSizeProvider;

        /// <param name="currentFontSizeProvider">
        /// Returns an element's current font size. Defaults to <c>resolvedStyle.fontSize</c>; tests
        /// inject their own since resolved styles need a live panel.
        /// </param>
        public UiAccessibilityStyler(Func<VisualElement, float> currentFontSizeProvider = null)
        {
            _currentFontSizeProvider = currentFontSizeProvider ?? (element => element.resolvedStyle.fontSize);
        }

        /// <summary>USS class for a colorblind mode, or null for <see cref="ColorblindMode.None"/>.</summary>
        public static string GetColorblindClass(ColorblindMode mode)
        {
            return mode == ColorblindMode.None ? null : ColorblindClassPrefix + mode.ToString().ToLowerInvariant();
        }

        /// <summary>base * scale, rounded to whole pixels (min 1) so text stays crisp.</summary>
        public static float ScaleFontSize(float baseSize, float scale)
        {
            float clampedScale = Mathf.Clamp(scale, AccessibilitySettings.MinFontScale, AccessibilitySettings.MaxFontScale);
            return Mathf.Max(1f, Mathf.Round(baseSize * clampedScale));
        }

        public static bool IsUnitScale(float scale)
        {
            return Mathf.Abs(scale - 1f) < ScaleEpsilon;
        }

        /// <summary>Applies classes, optional theme stylesheet and font scale to <paramref name="root"/> and its subtree.</summary>
        public void Apply(VisualElement root, UiAccessibilityState state, StyleSheet themeStyleSheet = null)
        {
            if (root == null)
            {
                return;
            }

            if (themeStyleSheet != null && !root.styleSheets.Contains(themeStyleSheet))
            {
                root.styleSheets.Add(themeStyleSheet);
            }

            ApplyClasses(root, state);
            ApplyFontScale(root, state.FontScale);
        }

        public static void ApplyClasses(VisualElement root, UiAccessibilityState state)
        {
            if (root == null)
            {
                return;
            }

            root.AddToClassList(RootClass);
            root.EnableInClassList(HighContrastClass, state.HighContrast);
            root.EnableInClassList(FontScaledClass, !IsUnitScale(state.FontScale));

            string activeColorblindClass = GetColorblindClass(state.ColorblindMode);
            foreach (ColorblindMode mode in (ColorblindMode[])Enum.GetValues(typeof(ColorblindMode)))
            {
                string modeClass = GetColorblindClass(mode);
                if (modeClass != null)
                {
                    root.EnableInClassList(modeClass, modeClass == activeColorblindClass);
                }
            }
        }

        public void ApplyFontScale(VisualElement root, float scale)
        {
            if (root == null)
            {
                return;
            }

            bool unit = IsUnitScale(scale);
            root.Query<TextElement>().ForEach(element => ApplyFontScaleToElement(element, scale, unit));
        }

        private void ApplyFontScaleToElement(VisualElement element, float scale, bool unit)
        {
            float current = _currentFontSizeProvider(element);
            bool hasRecord = _fontRecords.TryGetValue(element, out FontRecord record);

            if (!hasRecord)
            {
                if (unit || float.IsNaN(current) || current <= 0f)
                {
                    // Nothing to undo, or not styled yet (picked up on the applier's next rescan).
                    return;
                }

                record = new FontRecord { BaseSize = current, AppliedSize = current, PreviousAppliedSize = current, OriginalInline = element.style.fontSize };
                _fontRecords.Add(element, record);
            }
            else if (!float.IsNaN(current) && current > 0f && Mathf.Abs(current - record.AppliedSize) > SizeEpsilon
                     && Mathf.Abs(current - record.BaseSize) > SizeEpsilon && Mathf.Abs(current - record.PreviousAppliedSize) > SizeEpsilon)
            {
                // Changed by something other than us (class swap, controller code): adopt as new base.
                record.BaseSize = current;
                if (!IsOurInline(element, record))
                {
                    record.OriginalInline = element.style.fontSize;
                }
            }

            if (unit)
            {
                if (IsOurInline(element, record))
                {
                    element.style.fontSize = record.OriginalInline;
                }
                SetApplied(record, record.BaseSize);
                return;
            }

            float target = ScaleFontSize(record.BaseSize, scale);
            if (Mathf.Abs(target - record.AppliedSize) > SizeEpsilon || !IsOurInline(element, record))
            {
                element.style.fontSize = target;
            }
            SetApplied(record, target);
        }

        private static void SetApplied(FontRecord record, float size)
        {
            if (Mathf.Abs(size - record.AppliedSize) > SizeEpsilon)
            {
                record.PreviousAppliedSize = record.AppliedSize;
            }
            record.AppliedSize = size;
        }

        /// <summary>True when the element's inline font-size is the value this styler last applied
        /// (keyword Undefined == an inline length is set; Null == no inline value).</summary>
        private static bool IsOurInline(VisualElement element, FontRecord record)
        {
            StyleLength inline = element.style.fontSize;
            return inline.keyword == StyleKeyword.Undefined && Mathf.Abs(inline.value.value - record.AppliedSize) <= SizeEpsilon;
        }
    }
}
