using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.UI.Common;
using Rapadura.UI.Encounter;
using Rapadura.UI.Menus;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.Tests
{
    /// <summary>
    /// The credits roll (fully localized lines, CMU attribution, the closing on Vó Edna's porch, auto-scroll
    /// and tap-to-close rules) and the code-built arena overlays (boss banner, "Mochila cheia" toast) moved
    /// from inline colours to the theme through Resources/UI/EncounterOverlays.uss.
    /// </summary>
    public class CreditsRollTests
    {
        private const string OverlayUss = "Resources/UI/EncounterOverlays.uss";

        // ---------------------------------------------------------------
        // Credits content
        // ---------------------------------------------------------------

        [Test]
        public void CreditsKeys_ExistInBothLanguages()
        {
            LocalizationManager en = English();
            var problems = new List<string>();

            foreach (string key in CreditsContent.AllKeys())
            {
                StringAssert.StartsWith("ui.credits.", key);
                string pt = UiTexts.GetFallback(key);
                if (string.IsNullOrEmpty(pt))
                {
                    problems.Add($"{key}: no pt-BR text");
                }

                string english = UiTexts.Get(en, key);
                if (string.IsNullOrEmpty(english) || english == key)
                {
                    problems.Add($"{key}: no en text");
                }
            }

            CollectionAssert.IsEmpty(problems, string.Join("\n", problems));
        }

        [Test]
        public void EveryCreditsEntry_IsShownOnTheScreen()
        {
            var shown = new HashSet<string>(CreditsContent.AllKeys());
            foreach (LocalizationEntry entry in UiTexts.Entries.Where(e => e.Key.StartsWith("ui.credits.", System.StringComparison.Ordinal)))
            {
                Assert.IsTrue(shown.Contains(entry.Key), $"{entry.Key} is defined but CreditsContent never shows it");
            }
        }

        [Test]
        public void CreditsLines_SwitchLanguage_AndFallBackToPtBR()
        {
            LocalizationManager en = English();
            CreditsSection team = CreditsContent.TeamSection;

            Assert.AreEqual("Um jogo do Engenho Estúdio", team.GetLine(null, 0));
            Assert.AreEqual("A game by Engenho Estúdio", team.GetLine(en, 0));
            Assert.AreEqual(team.Lines[0], team.GetLine(null, 0), "Lines holds the pt-BR text");

            foreach (CreditsSection section in CreditsContent.Sections)
            {
                Assert.AreEqual(section.LineKeys.Count, section.Lines.Count);
                Assert.IsNotEmpty(section.LineKeys, $"{section.TitleKey} has no lines");
                for (int i = 0; i < section.LineKeys.Count; i++)
                {
                    StringAssert.StartsWith(UiTexts.CreditsLinePrefix, section.LineKeys[i]);
                    Assert.AreEqual(UiTexts.GetFallback(section.LineKeys[i]), section.GetLine(null, i));
                }
            }
        }

        [Test]
        public void Credits_CmuAttributionLine_IsPresent()
        {
            CollectionAssert.Contains(CreditsContent.ThirdPartySection.LineKeys, UiTexts.CreditsLineCmuAttribution);
            int index = CreditsContent.ThirdPartySection.LineKeys.ToList().IndexOf(UiTexts.CreditsLineCmuAttribution);

            Assert.AreEqual("Motion capture data from mocap.cs.cmu.edu (NSF EIA-0196217)", CreditsContent.CmuAttribution);
            Assert.AreEqual(CreditsContent.CmuAttribution, CreditsContent.ThirdPartySection.GetLine(English(), index), "the wording CMU asks for (Docs/Mocap.md)");

            string pt = CreditsContent.ThirdPartySection.GetLine(null, index);
            StringAssert.Contains("mocap.cs.cmu.edu", pt);
            StringAssert.Contains("NSF EIA-0196217", pt);

            string all = AllPtBR();
            StringAssert.Contains("CMU Graphics Lab Motion Capture Database", all);
            StringAssert.Contains("Bruce Hahne", all);

            // The licence doc and the mocap doc ask for the same sentence.
            StringAssert.Contains(CreditsContent.CmuAttribution, File.ReadAllText("Docs/ThirdPartyLicenses.md"));
            StringAssert.Contains(CreditsContent.CmuAttribution, File.ReadAllText("Docs/Mocap.md"));
        }

        [Test]
        public void Credits_CoverStudioCousinsToolsAndAssets()
        {
            string all = AllPtBR();
            foreach (string expected in new[]
                     {
                         "Engenho Estúdio", "Unity", "URP", "UI Toolkit", "Meshy", "Feito com Claude Code",
                         "Unity Companion License", "Apache License 2.0", "Fonte", "Texturas",
                     })
            {
                StringAssert.Contains(expected, all);
            }

            StringAssert.Contains("Made with Claude Code", AllIn(English()));

            CreditsSection cast = CreditsContent.CastSection;
            Assert.AreEqual(CreditsContent.HeroNames.Length, cast.Lines.Count, "one line per cousin");
            for (int i = 0; i < CreditsContent.HeroNames.Length; i++)
            {
                StringAssert.StartsWith(CreditsContent.HeroNames[i] + " — ", cast.Lines[i], "cousins in birth order, credited as their character");
            }
        }

        [Test]
        public void Credits_EndOnVoZefasPorch()
        {
            CreditsSection last = CreditsContent.Sections[CreditsContent.Sections.Count - 1];
            Assert.AreEqual(UiTexts.CreditsSectionClosing, last.TitleKey);
            StringAssert.Contains("varanda da Vó Edna", last.Lines[last.Lines.Count - 1]);
            StringAssert.Contains("Grandma Edna's porch", last.GetLine(English(), last.Lines.Count - 1));
        }

        [Test]
        public void CreditsView_HasTheLocalizedSkipHint()
        {
            string uxml = File.ReadAllText("Assets/UI/Menus/CreditsView.uxml");
            StringAssert.IsMatch("name=\"credits-skip-hint\"[^>]*class=\"[^\"]*\\bloc\\b[^\"]*\"[^>]*view-data-key=\"" + Regex.Escape(UiTexts.CreditsSkipHint) + "\"", uxml);
            // Stable names StoryPanelController / MainMenuController rely on.
            foreach (string name in new[] { "credits-root", "credits-scroll", "credits-content", "credits-back-button" })
            {
                StringAssert.Contains($"name=\"{name}\"", uxml);
            }
        }

        // ---------------------------------------------------------------
        // Roll behaviour (pure rules)
        // ---------------------------------------------------------------

        [Test]
        public void AutoScroll_OnlyWithFullMotion()
        {
            Assert.IsTrue(CreditsController.ShouldAutoScroll(reducedMotion: false));
            Assert.IsFalse(CreditsController.ShouldAutoScroll(reducedMotion: true), "Reduzir Movimento / Flashes: nothing scrolls by itself");
        }

        [Test]
        public void AutoScroll_AdvancesAndClampsToTheEnd()
        {
            Assert.AreEqual(32f, CreditsController.NextAutoScrollOffset(0f, 500f, 1f, 32f), 1e-4f);
            Assert.AreEqual(500f, CreditsController.NextAutoScrollOffset(490f, 500f, 1f, 32f), 1e-4f);
            Assert.AreEqual(10f, CreditsController.NextAutoScrollOffset(10f, 500f, -1f, 32f), 1e-4f, "negative delta never scrolls back");
            Assert.AreEqual(0f, CreditsController.NextAutoScrollOffset(0f, 0f, 1f, 32f), 1e-4f, "everything fits: stays at the top");
        }

        [Test]
        public void TapToClose_IgnoresDragsAndTheTapThatEndedTheEpilogue()
        {
            Assert.IsTrue(CreditsController.IsTapToClose(2f, 1f));
            Assert.IsFalse(CreditsController.IsTapToClose(CreditsController.TapSlopPixels + 1f, 5f), "a drag scrolls");
            Assert.IsFalse(CreditsController.IsTapToClose(0f, CreditsController.TapGuardSeconds * 0.5f), "too soon after opening");
        }

        // ---------------------------------------------------------------
        // Arena overlays on the theme
        // ---------------------------------------------------------------

        [Test]
        public void OverlaySheet_LoadsFromResources_AndImportsTheTheme()
        {
            Assert.IsNotNull(EncounterOverlayStyles.Sheet, $"Resources/{EncounterOverlayStyles.ResourcePath}.uss is missing");
            StringAssert.Contains("@import url(\"/Assets/UI/Common/RapaduraTheme.uss\")", Read(OverlayUss));
        }

        [Test]
        public void BossBanner_Root_GetsTheThemeClasses()
        {
            VisualElement root = BossIntroBannerController.CreateView(reducedMotion: false);

            AssertThemedRoot(root, BossIntroBannerController.RootClass);
            Assert.IsFalse(root.ClassListContains(UiMotionPreferences.ReducedMotionClass));

            VisualElement banner = root.Q<VisualElement>(BossIntroBannerController.BannerName);
            Assert.IsNotNull(banner);
            Assert.IsTrue(banner.ClassListContains(BossIntroBannerController.BannerClass));
            AssertNoInlineLook(banner);
            Assert.AreEqual(DisplayStyle.None, banner.style.display.value, "hidden until an encounter starts");

            foreach (string name in new[] { BossIntroBannerController.TitleName, BossIntroBannerController.NameLabelName })
            {
                Label label = root.Q<Label>(name);
                Assert.IsNotNull(label, name);
                Assert.IsTrue(label.ClassListContains(name), $"{name} has no USS class");
                Assert.AreEqual(PickingMode.Ignore, label.pickingMode, "never blocks combat input");
                AssertNoInlineLook(label);
            }

            Assert.IsTrue(BossIntroBannerController.CreateView(reducedMotion: true).ClassListContains(UiMotionPreferences.ReducedMotionClass));
        }

        [Test]
        public void BagFullToast_Root_GetsTheThemeClasses()
        {
            VisualElement root = BagFullToastController.CreateView(reducedMotion: false);

            AssertThemedRoot(root, BagFullToastController.RootName);

            VisualElement toast = root.Q<VisualElement>(BagFullToastController.ToastName);
            Assert.IsNotNull(toast);
            Assert.IsTrue(toast.ClassListContains(BagFullToastController.ToastName));
            AssertNoInlineLook(toast);

            foreach (string name in new[] { BagFullToastController.TitleName, BagFullToastController.LostName, BagFullToastController.HintName })
            {
                Label label = root.Q<Label>(name);
                Assert.IsNotNull(label, name);
                Assert.IsTrue(label.ClassListContains(name), $"{name} has no USS class");
                Assert.AreEqual(PickingMode.Ignore, label.pickingMode);
                AssertNoInlineLook(label);
            }
        }

        [Test]
        public void OverlayControllers_HaveNoInlineColours()
        {
            foreach (string path in new[] { "Assets/Scripts/UI/Encounter/BossIntroBannerController.cs", "Assets/Scripts/UI/Encounter/BagFullToastController.cs" })
            {
                string source = File.ReadAllText(path);
                StringAssert.DoesNotContain("new Color(", source, path);
                StringAssert.DoesNotMatch(@"style\.(backgroundColor|color|border\w*Color|fontSize|transitionDuration)\s*=", source, path);
            }
        }

        [Test]
        public void OverlayClasses_UsedInCode_AreStyled()
        {
            string uss = Read(OverlayUss);
            foreach (string cls in new[]
                     {
                         BossIntroBannerController.RootClass, BossIntroBannerController.BannerClass, BossIntroBannerController.TitleName,
                         BossIntroBannerController.NameLabelName, BossIntroBannerController.QuickFadeClass, BossIntroBannerController.InstantFadeClass,
                         BagFullToastController.RootName, BagFullToastController.ToastName, BagFullToastController.TitleName,
                         BagFullToastController.LostName, BagFullToastController.HintName,
                     })
            {
                StringAssert.IsMatch(@"(?m)^\." + Regex.Escape(cls) + @"\s*[,{]", uss, $".{cls} is not styled");
            }

            // Reduced motion (and "Reduzir Flashes", via SuppressUiMotion) drops the banner fade.
            StringAssert.IsMatch(@"\.rap-reduced-motion \.boss-intro-banner\s*\{[^}]*transition-duration:\s*0s", uss);
        }

        [Test]
        public void OverlayPlates_StayNearlyOpaque_OverSandAndSnow_AndRootsStayClear()
        {
            string uss = Read(OverlayUss);
            Dictionary<string, string> palette = ThemePalette();

            foreach (string selector in new[] { ".boss-intro-banner", ".bag-full-toast" })
            {
                string value = Regex.Match(Rule(uss, selector), @"background-color:\s*(?<v>[^;]+);").Groups["v"].Value.Trim();
                Assert.IsNotEmpty(value, $"{selector} has no background");
                Match variable = Regex.Match(value, @"^var\((?<name>--rap-[a-z-]+)\)$");
                if (variable.Success)
                {
                    Assert.IsTrue(palette.TryGetValue(variable.Groups["name"].Value, out value), $"{selector} uses an undeclared variable");
                }

                Assert.GreaterOrEqual(Alpha(value), 0.85f, $"{selector} would wash out over sand / snow");
            }

            StringAssert.DoesNotContain("background-color", Rule(uss, ".boss-intro-root"));
            StringAssert.DoesNotContain("background-color", Rule(uss, ".bag-full-root"));
        }

        // ---------------------------------------------------------------

        private static void AssertThemedRoot(VisualElement root, string rootClass)
        {
            Assert.IsTrue(root.ClassListContains(EncounterOverlayStyles.ThemeClass), "root has no rap-theme (the palette lives on it)");
            Assert.IsTrue(root.ClassListContains(rootClass));
            Assert.IsNotNull(EncounterOverlayStyles.Sheet);
            Assert.IsTrue(root.styleSheets.Contains(EncounterOverlayStyles.Sheet), "overlay sheet not attached to the root");
            Assert.AreEqual(PickingMode.Ignore, root.pickingMode);
            AssertNoInlineLook(root);
        }

        private static void AssertNoInlineLook(VisualElement element)
        {
            AssertNotInline(element.style.backgroundColor.keyword, $"{element.name}: inline background");
            AssertNotInline(element.style.color.keyword, $"{element.name}: inline colour");
            AssertNotInline(element.style.borderTopColor.keyword, $"{element.name}: inline border");
            AssertNotInline(element.style.fontSize.keyword, $"{element.name}: inline font size");
            AssertNotInline(element.style.position.keyword, $"{element.name}: inline layout");
        }

        // UI Toolkit: an inline style that was never assigned reads as StyleKeyword.Null; an assigned value reads as Undefined.
        private static void AssertNotInline(StyleKeyword keyword, string message)
        {
            Assert.AreEqual(StyleKeyword.Null, keyword, message);
        }

        private static LocalizationManager English()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            manager.SetLanguage(LanguageCode.en);
            return manager;
        }

        private static string AllPtBR() => AllIn(null);

        private static string AllIn(LocalizationManager manager)
        {
            var lines = new List<string>();
            foreach (CreditsSection section in CreditsContent.Sections)
            {
                lines.Add(section.GetTitle(manager));
                for (int i = 0; i < section.LineKeys.Count; i++)
                {
                    lines.Add(section.GetLine(manager, i));
                }
            }

            return string.Join("\n", lines);
        }

        private static string Read(string assetsRelativePath)
        {
            return File.ReadAllText(Path.Combine(Application.dataPath, assetsRelativePath)).Replace("\r\n", "\n");
        }

        private static Dictionary<string, string> ThemePalette()
        {
            return Regex.Matches(Read("UI/Common/RapaduraTheme.uss"), @"(?<name>--rap-[a-z-]+)\s*:\s*(?<value>[^;]+);")
                .Cast<Match>()
                .GroupBy(m => m.Groups["name"].Value)
                .ToDictionary(g => g.Key, g => g.First().Groups["value"].Value.Trim());
        }

        private static string Rule(string uss, string selector)
        {
            string stripped = Regex.Replace(uss, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            Match match = Regex.Match(stripped, @"(?m)^" + Regex.Escape(selector) + @"\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(match.Success, $"rule {selector} not found");
            return match.Groups["body"].Value;
        }

        private static float Alpha(string colour)
        {
            Match rgba = Regex.Match(colour, @"rgba\([^,]+,[^,]+,[^,]+,\s*(?<a>[0-9.]+)\)");
            return rgba.Success ? float.Parse(rgba.Groups["a"].Value, CultureInfo.InvariantCulture) : 1f;
        }
    }
}
