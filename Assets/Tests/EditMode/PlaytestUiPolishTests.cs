using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rapadura.Core.Accessibility;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.UI.CharacterSelect;
using Rapadura.UI.Menus;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.Tests
{
    /// <summary>
    /// Playtest round 4 UI polish: localized quality / binding names in Settings, the hero roster's
    /// selected state, and the USS fixes (tab focus underline, opaque tutorial hint below the HUD top
    /// bar, opaque return-to-title backdrop) checked straight from the stylesheets.
    /// </summary>
    public class PlaytestUiPolishTests
    {
        private readonly List<GameObject> _created = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject go in _created)
            {
                Object.DestroyImmediate(go);
            }

            _created.Clear();
        }

        // ---------------------------------------------------------------
        // Settings: quality + binding names
        // ---------------------------------------------------------------

        [Test]
        public void QualityName_LocalizesUnityLevelNames_UnknownKeptAsIs()
        {
            Assert.AreEqual("Média", SettingsMenuTexts.QualityName(null, "Medium"));
            Assert.AreEqual("Muito Baixa", SettingsMenuTexts.QualityName(null, "Very Low"));
            Assert.AreEqual("Medium", SettingsMenuTexts.QualityName(English(), "Medium"));
            Assert.AreEqual("Custom Potato", SettingsMenuTexts.QualityName(null, "Custom Potato"));
        }

        [Test]
        public void Slug_LowercasesAndCollapsesSeparators()
        {
            Assert.AreEqual("d_pad", SettingsMenuTexts.Slug("D-Pad"));
            Assert.AreEqual("left_stick_press", SettingsMenuTexts.Slug(" Left Stick  Press "));
            Assert.AreEqual(string.Empty, SettingsMenuTexts.Slug(null));
        }

        [Test]
        public void BindingDisplay_TranslatesWholeNamesThenPathParts()
        {
            string Translate(string name) => SettingsMenuTexts.BindingName(null, name);

            Assert.AreEqual("Analógico Esq. (L3)", InputBindingUtility.LocalizeDisplay("Left Stick Press", Translate));
            Assert.AreEqual("Direcional/Cima", InputBindingUtility.LocalizeDisplay("D-Pad/Up", Translate));
            Assert.AreEqual("W", InputBindingUtility.LocalizeDisplay("W", Translate), "single keys stay as shown");
            Assert.AreEqual("F5/Cima", InputBindingUtility.LocalizeDisplay("F5/Up", Translate), "unknown parts are kept");
            Assert.AreEqual("Left Stick Press", InputBindingUtility.LocalizeDisplay("Left Stick Press", null));
            Assert.AreEqual("Left Stick Press", InputBindingUtility.LocalizeDisplay("Left Stick Press", n => SettingsMenuTexts.BindingName(English(), n)));
        }

        // ---------------------------------------------------------------
        // Character select
        // ---------------------------------------------------------------

        [Test]
        public void RosterSelection_MarksOnlyTheShownHero()
        {
            var buttons = new Dictionary<CharacterId, Button>
            {
                { CharacterId.Maithe, new Button() },
                { CharacterId.Joaquim, new Button() },
            };

            CharacterSelectController.ApplyRosterSelection(buttons, CharacterId.Joaquim);
            Assert.IsTrue(buttons[CharacterId.Joaquim].ClassListContains(CharacterSelectController.RosterSelectedClass));
            Assert.IsFalse(buttons[CharacterId.Maithe].ClassListContains(CharacterSelectController.RosterSelectedClass));

            CharacterSelectController.ApplyRosterSelection(buttons, CharacterId.Maithe);
            Assert.IsFalse(buttons[CharacterId.Joaquim].ClassListContains(CharacterSelectController.RosterSelectedClass));
            Assert.IsTrue(buttons[CharacterId.Maithe].ClassListContains(CharacterSelectController.RosterSelectedClass));

            CharacterSelectController.ApplyRosterSelection(buttons, null);
            Assert.IsFalse(buttons[CharacterId.Maithe].ClassListContains(CharacterSelectController.RosterSelectedClass));

            StringAssert.Contains("." + CharacterSelectController.RosterSelectedClass, ReadUss("UI/CharacterSelect/CharacterSelectView.uss"));
        }

        // ---------------------------------------------------------------
        // Stylesheets
        // ---------------------------------------------------------------

        [Test]
        public void SettingsTabFocus_IsNotAnUnderline()
        {
            string focus = Rule(ReadUss("UI/Menus/SettingsMenuView.uss"), ".settings-tab-button:focus");

            StringAssert.DoesNotContain("border-bottom-color", focus, "only the active tab is underlined");
            StringAssert.Contains("border-bottom-width: 0", focus);
        }

        [Test]
        public void TutorialHint_IsOpaque_AndSitsBelowTheHudTopBar()
        {
            string uss = ReadUss("Resources/UI/TutorialHintView.uss");

            StringAssert.DoesNotContain("rgba", BackgroundOf(Rule(uss, ".tutorial-hint")), "map labels showed through at 0.95");
            Match padding = Regex.Match(Rule(uss, ".tutorial-hint-root"), @"padding-top:\s*(\d+)px");
            Assert.IsTrue(padding.Success);
            Assert.GreaterOrEqual(int.Parse(padding.Groups[1].Value), 150, "the HUD / map top bar is ~100-140px tall");
        }

        [Test]
        public void ReturnToTitleBackdrop_IsOpaque()
        {
            string dialog = Rule(ReadUss("Resources/UI/ReturnToTitleDialog.uss"), ".return-title-dialog");

            StringAssert.DoesNotContain("rgba", BackgroundOf(dialog), "the pause panel must not show through");
        }

        private static LocalizationManager English()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            manager.SetLanguage(LanguageCode.en);
            return manager;
        }

        private static string ReadUss(string assetsRelativePath)
        {
            return File.ReadAllText(Path.Combine(Application.dataPath, assetsRelativePath)).Replace("\r\n", "\n");
        }

        /// <summary>Body of the first rule whose selector list is exactly <paramref name="selector"/> (comments stripped).</summary>
        private static string Rule(string uss, string selector)
        {
            string stripped = Regex.Replace(uss, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            Match match = Regex.Match(stripped, @"(?m)^" + Regex.Escape(selector) + @"\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(match.Success, $"rule {selector} not found");
            return match.Groups["body"].Value;
        }

        private static string BackgroundOf(string ruleBody)
        {
            Match match = Regex.Match(ruleBody, @"background-color:\s*(?<value>[^;]+);");
            Assert.IsTrue(match.Success, "no background-color");
            return match.Groups["value"].Value;
        }
    }
}
