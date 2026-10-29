using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rapadura.Core.Accessibility;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.UI.CharacterSelect;
using Rapadura.UI.Common;
using Rapadura.UI.Menus;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.Tests
{
    /// <summary>
    /// Title screen + hero select restyle: the hero family line (<see cref="HeroFamily"/>), the default
    /// highlighted hero, the skill/age/medal texts of the hero select, the reduced-motion rule and the
    /// gradient helper, plus the shared theme wiring checked straight from the UXML/USS.
    /// </summary>
    public class FrontEndThemeTests
    {
        private const string ReduceFlashKey = "Accessibility.ReduceFlashEffects";
        private const string ReduceMotionKey = "Accessibility.ReduceMotion";

        // ---------------------------------------------------------------
        // Family
        // ---------------------------------------------------------------

        [Test]
        public void BirthOrder_MatchesTheRosterAges_EldestFirst()
        {
            CollectionAssert.AreEqual(
                new[] { CharacterId.Maithe, CharacterId.Maria, CharacterId.Joaquim, CharacterId.Icaro, CharacterId.Lavine },
                HeroFamily.BirthOrder.ToArray());

            for (int i = 1; i < HeroFamily.BirthOrder.Count; i++)
            {
                Assert.Greater(CharacterRegistry.Get(HeroFamily.BirthOrder[i - 1]).Age, CharacterRegistry.Get(HeroFamily.BirthOrder[i]).Age);
            }

            Assert.AreEqual(1, HeroFamily.BirthRank(CharacterId.Maithe));
            Assert.AreEqual(5, HeroFamily.BirthRank(CharacterId.Lavine));
        }

        [Test]
        public void Siblings_AreMaitheIcaro_AndMariaJoaquim_LavineIsTheCousin()
        {
            Assert.IsTrue(HeroFamily.TryGetSibling(CharacterId.Maithe, out CharacterId s1));
            Assert.AreEqual(CharacterId.Icaro, s1);
            Assert.IsTrue(HeroFamily.TryGetSibling(CharacterId.Icaro, out CharacterId s2));
            Assert.AreEqual(CharacterId.Maithe, s2);
            Assert.IsTrue(HeroFamily.TryGetSibling(CharacterId.Maria, out CharacterId s3));
            Assert.AreEqual(CharacterId.Joaquim, s3);
            Assert.IsTrue(HeroFamily.TryGetSibling(CharacterId.Joaquim, out CharacterId s4));
            Assert.AreEqual(CharacterId.Maria, s4);
            Assert.IsFalse(HeroFamily.TryGetSibling(CharacterId.Lavine, out _));
        }

        [Test]
        public void FamilyLine_PtBR_UsesGenderAndBirthRank()
        {
            Assert.AreEqual("Irmã do Ícaro · a mais velha", HeroFamily.BuildFamilyLine(CharacterId.Maithe, null));
            Assert.AreEqual("Irmã do Joaquim · a segunda mais velha", HeroFamily.BuildFamilyLine(CharacterId.Maria, null));
            Assert.AreEqual("Irmão da Maria · o do meio", HeroFamily.BuildFamilyLine(CharacterId.Joaquim, null));
            Assert.AreEqual("Irmão da Maithe · o penúltimo", HeroFamily.BuildFamilyLine(CharacterId.Icaro, null));
            Assert.AreEqual("Prima de todos · a caçula", HeroFamily.BuildFamilyLine(CharacterId.Lavine, null));
        }

        [Test]
        public void FamilyLine_English()
        {
            LocalizationManager en = English();

            Assert.AreEqual("Ícaro's sister · the eldest", HeroFamily.BuildFamilyLine(CharacterId.Maithe, en));
            Assert.AreEqual("Maria's brother · the middle one", HeroFamily.BuildFamilyLine(CharacterId.Joaquim, en));
            Assert.AreEqual("Cousin to everyone · the youngest", HeroFamily.BuildFamilyLine(CharacterId.Lavine, en));
        }

        [Test]
        public void FamilyKeys_AllResolveInBothLanguages()
        {
            LocalizationManager en = English();

            foreach (string key in HeroFamily.AllKeys())
            {
                Assert.IsNotNull(UiTexts.GetFallback(key), $"{key} has no pt-BR text");
                Assert.AreNotEqual(key, UiTexts.Get(en, key), $"{key} has no en text");
            }
        }

        // ---------------------------------------------------------------
        // Hero select texts / default highlight
        // ---------------------------------------------------------------

        [Test]
        public void DefaultHighlight_KeepsCurrent_ElseSavedHero_ElseFirst()
        {
            var roster = new[] { CharacterId.Joaquim, CharacterId.Maria, CharacterId.Maithe };

            Assert.AreEqual(CharacterId.Joaquim, CharacterSelectController.ChooseDefaultHighlight(roster, null, null), "never open on an empty preview");
            Assert.AreEqual(CharacterId.Maithe, CharacterSelectController.ChooseDefaultHighlight(roster, null, CharacterId.Maithe));
            Assert.AreEqual(CharacterId.Maria, CharacterSelectController.ChooseDefaultHighlight(roster, CharacterId.Maria, CharacterId.Maithe));
            Assert.AreEqual(CharacterId.Joaquim, CharacterSelectController.ChooseDefaultHighlight(roster, CharacterId.Lavine, CharacterId.Icaro), "ids outside the roster are ignored");
            Assert.IsNull(CharacterSelectController.ChooseDefaultHighlight(new CharacterId[0], null, CharacterId.Maithe));
            Assert.IsNull(CharacterSelectController.ChooseDefaultHighlight(null, null, null));
        }

        [Test]
        public void SkillLines_AreTheFiveAbilities_UltimateTagged()
        {
            PlayableCharacter maithe = CharacterRegistry.Get(CharacterId.Maithe);

            List<string> lines = CharacterSelectController.BuildSkillLines(maithe, null, null);

            Assert.AreEqual(5, lines.Count);
            Assert.AreEqual("Flecha Precisa", lines[0]);
            Assert.AreEqual("Tempestade de Mil Flechas (Ultimate)", lines[4]);
            CollectionAssert.IsEmpty(CharacterSelectController.BuildSkillLines(null, null, null));
        }

        [Test]
        public void SkillLines_UseTheSkillAssetsWhenPresent()
        {
            PlayableCharacter maithe = CharacterRegistry.Get(CharacterId.Maithe);

            List<string> lines = CharacterSelectController.BuildSkillLines(maithe, CharacterSkillSet.GetSkills(CharacterId.Maithe), null);

            Assert.AreEqual(5, lines.Count);
            Assert.AreEqual("Flecha Precisa", lines[0], "pt-BR asset name");
        }

        [Test]
        public void AgeText_BothLanguages()
        {
            PlayableCharacter maithe = CharacterRegistry.Get(CharacterId.Maithe);

            Assert.AreEqual("14 anos", CharacterSelectController.BuildAgeText(maithe, null));
            Assert.AreEqual("14 years old", CharacterSelectController.BuildAgeText(maithe, English()));
            Assert.AreEqual(string.Empty, CharacterSelectController.BuildAgeText(null, null));
        }

        [Test]
        public void Medal_ShowsTheInitial_ReadableOnLightAndDarkAccents()
        {
            Assert.AreEqual("Í", CharacterSelectController.BuildMedalText(CharacterRegistry.Get(CharacterId.Icaro), null));
            Assert.AreEqual(string.Empty, CharacterSelectController.BuildMedalText(null, null));

            Color onMaria = CharacterSelectController.MedalTextColor(CharacterRegistry.Get(CharacterId.Maria).AccentColor);
            Color onLavine = CharacterSelectController.MedalTextColor(CharacterRegistry.Get(CharacterId.Lavine).AccentColor);
            Assert.Less(onMaria.grayscale, 0.5f, "Maria's pale gold needs dark text");
            Assert.Greater(onLavine.grayscale, 0.5f, "Lavine's purple needs light text");
        }

        // ---------------------------------------------------------------
        // Motion / gradient helpers
        // ---------------------------------------------------------------

        [Test]
        public void ReducedMotion_FollowsReduzirMovimento_AndReduzirFlashes()
        {
            bool hadKey = PlayerPrefs.HasKey(ReduceFlashKey);
            int previous = PlayerPrefs.GetInt(ReduceFlashKey, 0);
            bool hadMotionKey = PlayerPrefs.HasKey(ReduceMotionKey);
            int previousMotion = PlayerPrefs.GetInt(ReduceMotionKey, 0);
            try
            {
                Assert.IsFalse(UiMotionPreferences.IsReducedMotion(null));

                var settings = new AccessibilitySettings();
                settings.Initialize();
                settings.SetReduceFlashEffects(false);
                settings.SetReduceMotion(true);
                Assert.IsTrue(UiMotionPreferences.IsReducedMotion(settings), "Reduzir Movimento");
                settings.SetReduceMotion(false);
                settings.SetReduceFlashEffects(true);
                Assert.IsTrue(UiMotionPreferences.IsReducedMotion(settings), "Reduzir Flashes still calms the UI");
                settings.SetReduceFlashEffects(false);
                Assert.IsFalse(UiMotionPreferences.IsReducedMotion(settings));

                var root = new VisualElement();
                UiMotionPreferences.ApplyTo(root, true);
                Assert.IsTrue(root.ClassListContains(UiMotionPreferences.ReducedMotionClass));
                UiMotionPreferences.ApplyTo(root, false);
                Assert.IsFalse(root.ClassListContains(UiMotionPreferences.ReducedMotionClass));
            }
            finally
            {
                if (hadKey) PlayerPrefs.SetInt(ReduceFlashKey, previous);
                else PlayerPrefs.DeleteKey(ReduceFlashKey);
                if (hadMotionKey) PlayerPrefs.SetInt(ReduceMotionKey, previousMotion);
                else PlayerPrefs.DeleteKey(ReduceMotionKey);
            }
        }

        [Test]
        public void TitleEntrance_SkippedWithReducedMotion_ButtonsStaggered()
        {
            Assert.IsTrue(TitleScreenDecor.ShouldPlayEntrance(false, true));
            Assert.IsFalse(TitleScreenDecor.ShouldPlayEntrance(true, true));
            Assert.IsFalse(TitleScreenDecor.ShouldPlayEntrance(false, false), "no panel = nothing would remove the class");
            Assert.Greater(TitleScreenDecor.ButtonEntranceDelay(1), TitleScreenDecor.ButtonEntranceDelay(0));
            Assert.Less(TitleScreenDecor.ButtonEntranceDelay(5), 1f, "the whole entrance stays short");
        }

        [Test]
        public void TitleEntrance_WithoutPanel_LeavesTheMenuVisible()
        {
            var document = new VisualElement();
            var screen = new VisualElement();
            document.Add(screen);
            var decor = new TitleScreenDecor();

            decor.Attach(document, screen, reducedMotion: false);

            Assert.IsFalse(screen.ClassListContains(TitleScreenDecor.EnteringClass));
            decor.Detach();
        }

        [Test]
        public void Clouds_WrapAcrossTheSky()
        {
            Assert.AreEqual(-30f, TitleScreenDecor.CloudOffsetPercent(0f, 0f), 0.001f);
            float later = TitleScreenDecor.CloudOffsetPercent(0f, 10f);
            Assert.Greater(later, -30f);
            for (float t = 0f; t < 400f; t += 7.3f)
            {
                float x = TitleScreenDecor.CloudOffsetPercent(0.5f, t);
                Assert.GreaterOrEqual(x, -30f);
                Assert.Less(x, 110f);
            }
        }

        [Test]
        public void Gradient_TopIsFirstStop_BottomIsLast()
        {
            var stops = new[] { Color.red, Color.green, Color.blue };

            Assert.AreEqual(Color.red, UiGradientTexture.Evaluate(stops, 0f));
            Assert.AreEqual(Color.green, UiGradientTexture.Evaluate(stops, 0.5f));
            Assert.AreEqual(Color.blue, UiGradientTexture.Evaluate(stops, 1f));
            Assert.AreEqual(Color.white, UiGradientTexture.Evaluate(new[] { Color.white }, 0.7f));
            Assert.IsNull(UiGradientTexture.CreateVertical(null));

            Texture2D texture = UiGradientTexture.CreateVertical(stops, 16);
            try
            {
                Assert.AreEqual(16, texture.height);
                AssertColor(Color.red, texture.GetPixel(0, 15), "top row");
                AssertColor(Color.blue, texture.GetPixel(0, 0), "bottom row");
            }
            finally
            {
                UiGradientTexture.Release(texture);
            }
        }

        // ---------------------------------------------------------------
        // UXML / USS wiring
        // ---------------------------------------------------------------

        [Test]
        public void TitleScreen_KeepsEveryButtonIdAndKey_AndUsesTheSharedTheme()
        {
            string uxml = Read("UI/Menus/MainMenuView.uxml");

            foreach ((string name, string key) in new[]
                     {
                         ("new-game-button", UiTexts.MenuNewGame), ("continue-button", UiTexts.MenuContinue),
                         ("load-button", UiTexts.MenuLoad), ("settings-button", UiTexts.MenuSettings),
                         ("credits-button", UiTexts.MenuCredits), ("quit-button", UiTexts.MenuQuit),
                     })
            {
                StringAssert.IsMatch($"name=\"{name}\"[^>]*class=\"main-menu-button loc rap-button[^\"]*\"[^>]*view-data-key=\"{key}\"", uxml);
            }

            StringAssert.Contains("/Assets/UI/Common/RapaduraTheme.uss", uxml);
            StringAssert.Contains("name=\"" + TitleScreenDecor.SkyElementName + "\"", uxml);
            StringAssert.Contains("view-data-key=\"" + UiTexts.MenuTagline + "\"", uxml);
            StringAssert.Contains("." + TitleScreenDecor.EnteringClass, Read("UI/Menus/MainMenuView.uss"));
        }

        [Test]
        public void HeroSelect_UsesTheSharedTheme_AndHasTheNewDetailElements()
        {
            string uxml = Read("UI/CharacterSelect/CharacterSelectView.uxml");

            StringAssert.Contains("/Assets/UI/Common/RapaduraTheme.uss", uxml);
            foreach (string name in new[] { "detail-preview", "detail-family", "detail-age", "detail-skills", "confirm-button", "roster-container" })
            {
                StringAssert.Contains($"name=\"{name}\"", uxml);
            }

            string uss = Read("UI/CharacterSelect/CharacterSelectView.uss");
            StringAssert.Contains("." + CharacterSelectController.SkillClass, uss);
            StringAssert.Contains("." + CharacterSelectController.UltimateSkillClass, uss);
        }

        [Test]
        public void SharedTheme_StylesEveryButtonState_AndReducedMotion()
        {
            string uss = Read("UI/Common/RapaduraTheme.uss");

            foreach (string selector in new[] { ".rap-theme", ".rap-button", ".rap-button:hover", ".rap-button:focus", ".rap-button:active", ".rap-button:disabled", ".rap-button--primary" })
            {
                StringAssert.IsMatch(@"(?m)^" + Regex.Escape(selector) + @"\s*[,{]", uss, $"{selector} missing");
            }

            StringAssert.Contains("." + UiMotionPreferences.ReducedMotionClass + " .rap-button", uss);
        }

        private static void AssertColor(Color expected, Color actual, string message)
        {
            Assert.AreEqual(expected.r, actual.r, 0.02f, message);
            Assert.AreEqual(expected.g, actual.g, 0.02f, message);
            Assert.AreEqual(expected.b, actual.b, 0.02f, message);
        }

        private static string Read(string assetsRelativePath)
        {
            return File.ReadAllText(Path.Combine(Application.dataPath, assetsRelativePath)).Replace("\r\n", "\n");
        }

        private static LocalizationManager English()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            manager.SetLanguage(LanguageCode.en);
            return manager;
        }
    }
}
