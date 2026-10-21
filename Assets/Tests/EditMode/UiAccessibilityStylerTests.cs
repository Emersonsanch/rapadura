using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Core.Accessibility;
using Rapadura.UI.Common;
using UnityEngine.UIElements;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="UiAccessibilityStyler"/>, the pure logic behind
    /// <see cref="UiAccessibilityApplier"/>: colorblind / high-contrast USS classes on a document
    /// root and per-text-element font scaling. Runs on plain VisualElements without a panel, so
    /// the "current font size" (normally resolvedStyle.fontSize) is injected by the test.
    /// </summary>
    public class UiAccessibilityStylerTests
    {
        private readonly Dictionary<VisualElement, float> _resolvedSizes = new Dictionary<VisualElement, float>();

        [SetUp]
        public void SetUp()
        {
            _resolvedSizes.Clear();
        }

        private UiAccessibilityStyler CreateStyler()
        {
            return new UiAccessibilityStyler(element => _resolvedSizes.TryGetValue(element, out float size) ? size : 0f);
        }

        private Label AddLabel(VisualElement parent, float resolvedSize)
        {
            var label = new Label("text");
            parent.Add(label);
            _resolvedSizes[label] = resolvedSize;
            return label;
        }

        /// <summary>Simulates UI Toolkit's style pass: resolved size becomes the inline value when set.</summary>
        private void SimulateStylePass(VisualElement element, float ussSize)
        {
            StyleLength inline = element.style.fontSize;
            _resolvedSizes[element] = inline.keyword == StyleKeyword.Undefined ? inline.value.value : ussSize;
        }

        private static bool HasInlineFontSize(VisualElement element)
        {
            return element.style.fontSize.keyword == StyleKeyword.Undefined;
        }

        // ------------------------------------------------------------------
        // Pure helpers.
        // ------------------------------------------------------------------

        [Test]
        public void GetColorblindClass_NoneHasNoClass_OthersArePrefixed()
        {
            Assert.IsNull(UiAccessibilityStyler.GetColorblindClass(ColorblindMode.None));
            Assert.AreEqual("a11y-colorblind-protanopia", UiAccessibilityStyler.GetColorblindClass(ColorblindMode.Protanopia));
            Assert.AreEqual("a11y-colorblind-deuteranopia", UiAccessibilityStyler.GetColorblindClass(ColorblindMode.Deuteranopia));
            Assert.AreEqual("a11y-colorblind-tritanopia", UiAccessibilityStyler.GetColorblindClass(ColorblindMode.Tritanopia));
        }

        [Test]
        public void ScaleFontSize_RoundsToWholePixels_AndClampsScale()
        {
            Assert.AreEqual(21f, UiAccessibilityStyler.ScaleFontSize(14f, 1.5f));
            Assert.AreEqual(15f, UiAccessibilityStyler.ScaleFontSize(13f, 1.15f));
            Assert.AreEqual(14f * AccessibilitySettings.MaxFontScale, UiAccessibilityStyler.ScaleFontSize(14f, 10f));
            Assert.AreEqual(9f, UiAccessibilityStyler.ScaleFontSize(12f, 0.1f), "Scale below MinFontScale is clamped to 0.75.");
            Assert.AreEqual(1f, UiAccessibilityStyler.ScaleFontSize(0.2f, 1f), "Never below 1px.");
        }

        [Test]
        public void UiAccessibilityState_FromNullSettings_IsDefault()
        {
            UiAccessibilityState state = UiAccessibilityState.From(null);

            Assert.AreEqual(1f, state.FontScale);
            Assert.IsFalse(state.HighContrast);
            Assert.AreEqual(ColorblindMode.None, state.ColorblindMode);
        }

        // ------------------------------------------------------------------
        // Classes.
        // ------------------------------------------------------------------

        [Test]
        public void ApplyClasses_SetsRootHighContrastAndColorblindClasses()
        {
            var root = new VisualElement();

            UiAccessibilityStyler.ApplyClasses(root, new UiAccessibilityState(1.25f, true, ColorblindMode.Tritanopia));

            Assert.IsTrue(root.ClassListContains(UiAccessibilityStyler.RootClass));
            Assert.IsTrue(root.ClassListContains(UiAccessibilityStyler.HighContrastClass));
            Assert.IsTrue(root.ClassListContains(UiAccessibilityStyler.FontScaledClass));
            Assert.IsTrue(root.ClassListContains("a11y-colorblind-tritanopia"));
        }

        [Test]
        public void ApplyClasses_SwitchingMode_RemovesPreviousClasses()
        {
            var root = new VisualElement();
            UiAccessibilityStyler.ApplyClasses(root, new UiAccessibilityState(1.5f, true, ColorblindMode.Protanopia));

            UiAccessibilityStyler.ApplyClasses(root, new UiAccessibilityState(1f, false, ColorblindMode.Deuteranopia));

            Assert.IsFalse(root.ClassListContains("a11y-colorblind-protanopia"));
            Assert.IsTrue(root.ClassListContains("a11y-colorblind-deuteranopia"));
            Assert.IsFalse(root.ClassListContains(UiAccessibilityStyler.HighContrastClass));
            Assert.IsFalse(root.ClassListContains(UiAccessibilityStyler.FontScaledClass));

            UiAccessibilityStyler.ApplyClasses(root, UiAccessibilityState.Default);
            Assert.IsFalse(root.ClassListContains("a11y-colorblind-deuteranopia"));
        }

        [Test]
        public void Apply_NullRoot_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => CreateStyler().Apply(null, UiAccessibilityState.Default));
        }

        // ------------------------------------------------------------------
        // Font scale.
        // ------------------------------------------------------------------

        [Test]
        public void ApplyFontScale_ScalesEveryTextElementInTheTree()
        {
            UiAccessibilityStyler styler = CreateStyler();
            var root = new VisualElement();
            Label title = AddLabel(root, 20f);
            var nested = new VisualElement();
            root.Add(nested);
            Label body = AddLabel(nested, 12f);
            var button = new Button { text = "OK" };
            nested.Add(button);
            _resolvedSizes[button] = 14f;

            styler.ApplyFontScale(root, 1.5f);

            Assert.AreEqual(30f, title.style.fontSize.value.value);
            Assert.AreEqual(18f, body.style.fontSize.value.value);
            Assert.AreEqual(21f, button.style.fontSize.value.value);
        }

        [Test]
        public void ApplyFontScale_RepeatedApplies_DoNotCompound()
        {
            UiAccessibilityStyler styler = CreateStyler();
            var root = new VisualElement();
            Label label = AddLabel(root, 14f);

            styler.ApplyFontScale(root, 1.5f);
            SimulateStylePass(label, 14f);
            styler.ApplyFontScale(root, 1.5f);
            SimulateStylePass(label, 14f);
            styler.ApplyFontScale(root, 2f);

            Assert.AreEqual(28f, label.style.fontSize.value.value, "Scale is always applied to the original base size.");
        }

        [Test]
        public void ApplyFontScale_BackToOne_RemovesInlineSize()
        {
            UiAccessibilityStyler styler = CreateStyler();
            var root = new VisualElement();
            Label label = AddLabel(root, 14f);

            styler.ApplyFontScale(root, 1.5f);
            SimulateStylePass(label, 14f);
            styler.ApplyFontScale(root, 1f);

            Assert.IsFalse(HasInlineFontSize(label));
        }

        [Test]
        public void ApplyFontScale_BackToOne_RestoresPreExistingInlineSize()
        {
            UiAccessibilityStyler styler = CreateStyler();
            var root = new VisualElement();
            Label label = AddLabel(root, 16f);
            label.style.fontSize = 16f; // set by some controller before accessibility kicks in

            styler.ApplyFontScale(root, 2f);
            Assert.AreEqual(32f, label.style.fontSize.value.value);
            SimulateStylePass(label, 16f);

            styler.ApplyFontScale(root, 1f);

            Assert.IsTrue(HasInlineFontSize(label));
            Assert.AreEqual(16f, label.style.fontSize.value.value);
        }

        [Test]
        public void ApplyFontScale_UnitScale_LeavesUntouchedElementsAlone()
        {
            UiAccessibilityStyler styler = CreateStyler();
            var root = new VisualElement();
            Label label = AddLabel(root, 14f);

            styler.ApplyFontScale(root, 1f);

            Assert.IsFalse(HasInlineFontSize(label));
        }

        [Test]
        public void ApplyFontScale_NotYetStyledElement_IsSkippedThenPickedUpLater()
        {
            UiAccessibilityStyler styler = CreateStyler();
            var root = new VisualElement();
            Label label = AddLabel(root, 0f);

            styler.ApplyFontScale(root, 1.5f);
            Assert.IsFalse(HasInlineFontSize(label));

            _resolvedSizes[label] = 12f; // styled on a later frame (e.g. row added by a controller)
            styler.ApplyFontScale(root, 1.5f);

            Assert.AreEqual(18f, label.style.fontSize.value.value);
        }

        [Test]
        public void ApplyFontScale_ExternalSizeChange_BecomesNewBase()
        {
            UiAccessibilityStyler styler = CreateStyler();
            var root = new VisualElement();
            Label label = AddLabel(root, 14f);

            styler.ApplyFontScale(root, 1.5f);
            label.style.fontSize = 20f; // controller code changed it while scaled
            SimulateStylePass(label, 14f);

            styler.ApplyFontScale(root, 1.5f);

            Assert.AreEqual(30f, label.style.fontSize.value.value);
        }

        [Test]
        public void Apply_AppliesClassesAndFontScaleTogether()
        {
            UiAccessibilityStyler styler = CreateStyler();
            var root = new VisualElement();
            Label label = AddLabel(root, 10f);

            styler.Apply(root, new UiAccessibilityState(2f, false, ColorblindMode.Protanopia));

            Assert.IsTrue(root.ClassListContains("a11y-colorblind-protanopia"));
            Assert.AreEqual(20f, label.style.fontSize.value.value);
        }
    }
}
