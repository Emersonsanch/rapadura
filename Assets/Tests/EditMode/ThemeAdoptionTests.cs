using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Every in-game screen adopts the shared Rapadura theme (UI/Common/RapaduraTheme.uss): the sheet
    /// is imported before the screen's own USS, the root carries <c>rap-theme</c> (the palette lives on
    /// that class), every <c>var(--rap-*)</c> used anywhere is declared, the HUD stays near-opaque over
    /// sand / snow while its full-screen roots stay transparent, and the title-screen layout fixes hold.
    /// Read straight from the UXML/USS files, like <see cref="PlaytestUiPolishTests"/>.
    /// </summary>
    public class ThemeAdoptionTests
    {
        private const string ThemePath = "/Assets/UI/Common/RapaduraTheme.uss";

        /// <summary>Assets-relative UXML of every themed screen.</summary>
        private static readonly string[] ThemedScreens =
        {
            "UI/Menus/MainMenuView.uxml",
            "UI/CharacterSelect/CharacterSelectView.uxml",
            "UI/WorldMap/WorldMapHudView.uxml",
            "UI/HUD/HudView.uxml",
            "UI/Inventory/InventoryView.uxml",
            "UI/Common/TooltipView.uxml",
            "UI/Shop/ShopView.uxml",
            "UI/Crafting/CraftingView.uxml",
            "UI/Quests/QuestLogView.uxml",
            "UI/Quests/QuestTrackerView.uxml",
            "UI/SkillTree/SkillTreeView.uxml",
            "UI/Achievements/AchievementListView.uxml",
            "UI/Achievements/AchievementToastView.uxml",
            "UI/Menus/PauseMenuView.uxml",
            "UI/Menus/SettingsMenuView.uxml",
            "UI/Menus/SaveLoadMenuView.uxml",
            "UI/Menus/CreditsView.uxml",
            "UI/Death/DeathScreenView.uxml",
            "Resources/UI/ReturnToTitleDialog.uxml",
            "Resources/UI/CharacterSheetView.uxml",
            "Resources/UI/DialogueView.uxml",
            "Resources/UI/StoryPanelView.uxml",
            "Resources/UI/TutorialHintView.uxml",
            "Resources/UI/VillageChestView.uxml",
        };

        [Test]
        public void EveryScreen_ImportsTheThemeFirst_AndMarksItsRoot()
        {
            foreach (string path in ThemedScreens)
            {
                string uxml = Read(path);
                int theme = uxml.IndexOf(ThemePath, System.StringComparison.Ordinal);
                Assert.GreaterOrEqual(theme, 0, $"{path} does not import the theme");

                // The screen's own sheet (first <Style> that is not the theme) comes after it, so its
                // same-specificity rules win over the theme's defaults.
                Match own = Regex.Match(uxml, "<Style src=\"(?<src>[^\"]+)\"");
                while (own.Success && own.Groups["src"].Value == ThemePath)
                {
                    own = own.NextMatch();
                }

                if (own.Success)
                {
                    Assert.Less(theme, own.Index, $"{path}: the theme must be imported before {own.Groups["src"].Value}");
                }

                StringAssert.IsMatch("class=\"[^\"]*\\brap-theme\\b", uxml, $"{path} has no rap-theme root");
            }
        }

        [Test]
        public void EveryThemeVariable_UsedInUss_IsDeclaredByTheTheme()
        {
            string theme = Read("UI/Common/RapaduraTheme.uss");
            var declared = new HashSet<string>(Regex.Matches(theme, @"(--rap-[a-z-]+)\s*:").Cast<Match>().Select(m => m.Groups[1].Value));
            Assert.IsNotEmpty(declared);

            foreach (string file in UssFiles())
            {
                foreach (Match use in Regex.Matches(File.ReadAllText(file), @"var\((--[a-z-]+)\)"))
                {
                    Assert.IsTrue(declared.Contains(use.Groups[1].Value), $"{file} uses undeclared {use.Groups[1].Value}");
                }
            }
        }

        [Test]
        public void UssUsingThemeVariables_BelongsToAThemedScreen()
        {
            // A var() outside a rap-theme subtree resolves to nothing (transparent / default colour).
            var themedSheets = new HashSet<string>();
            foreach (string path in ThemedScreens)
            {
                foreach (Match style in Regex.Matches(Read(path), "<Style src=\"/Assets/(?<src>[^\"]+)\""))
                {
                    themedSheets.Add(style.Groups["src"].Value);
                }
            }

            foreach (string file in UssFiles())
            {
                string relative = file.Replace('\\', '/').Substring("Assets/".Length);
                if (relative == "UI/Common/RapaduraTheme.uss" || !File.ReadAllText(file).Contains("var(--rap-"))
                {
                    continue;
                }

                Assert.IsTrue(themedSheets.Contains(relative), $"{relative} uses theme variables but no themed UXML imports it");
            }
        }

        [Test]
        public void Hud_PanelsAndButtons_StayNearlyOpaque_OverSandAndSnow()
        {
            string hud = Read("UI/HUD/HudView.uss");
            string map = Read("UI/WorldMap/WorldMapHudView.uss");

            foreach ((string uss, string selector) in new[]
                     {
                         (hud, ".hud-vitals-panel"), (hud, ".hud-xp-panel"), (hud, ".hud-touch-button"),
                         (map, ".wm-top-bar"), (map, ".wm-menu-button"), (map, ".wm-region-label"),
                     })
            {
                Assert.GreaterOrEqual(Alpha(Background(Rule(uss, selector))), 0.85f, $"{selector} would wash out over sand / snow");
            }
        }

        [Test]
        public void FullScreenHudRoots_HaveNoBackground()
        {
            // A full-screen tint on these roots would sit over the whole 3D scene while the HUD shows.
            StringAssert.DoesNotContain("background-color", Rule(Read("UI/HUD/HudView.uss"), ".hud-root"));
            StringAssert.DoesNotContain("background-color", Rule(Read("UI/WorldMap/WorldMapHudView.uss"), ".wm-hud-root"));
            StringAssert.DoesNotContain("background-color", Rule(Read("UI/WorldMap/WorldMapHudView.uss"), ".wm-labels-layer"));
            StringAssert.DoesNotContain("background-color", Rule(Read("UI/Quests/QuestTrackerView.uss"), ".quest-tracker-root"));
        }

        [Test]
        public void VillageHub_ClassesUsedInCode_AreStyled()
        {
            string map = Read("UI/WorldMap/WorldMapHudView.uss");
            foreach (string cls in new[] { VillageHubButton.ButtonName, VillageHubButton.VillageSectionClass, VillageHubButton.VillageOptionClass, RegionNpcTalkButton.ButtonClass })
            {
                StringAssert.IsMatch(@"(?m)^\." + Regex.Escape(cls) + @"\s*[,{]", map, $".{cls} is not styled");
            }
        }

        [Test]
        public void Title_LogoFitsItsPlaque_AndHillsAreRoundedShapes()
        {
            string uss = Read("UI/Menus/MainMenuView.uss");

            float panelWidth = Px(Rule(uss, ".main-menu-panel"), "width");
            float logoSize = Px(Rule(uss, ".main-menu-title"), "font-size");
            // "Rapadura" in bold is ~4.7em wide plus letter spacing and outline; padding is 30px a side.
            Assert.LessOrEqual(logoSize * 4.9f + 20f, panelWidth - 60f, "the logo would overflow the plaque");

            foreach (string hill in new[] { ".main-menu-hill--far", ".main-menu-hill--mid", ".main-menu-hill--near" })
            {
                string body = Rule(uss, hill);
                float size = Px(body, "width");
                Assert.AreEqual(size, Px(body, "height"), 0.01f, $"{hill} must be a circle before scaling");
                Assert.AreEqual(size / 2f, Px(body, "border-radius"), 0.01f, $"{hill} must be fully round (no flat block)");
                StringAssert.Contains("scale:", body, $"{hill} is stretched into an ellipse with scale");
            }

            StringAssert.DoesNotContain("border-top-left-radius: 50%", uss);
        }

        // ---------------------------------------------------------------

        private static IEnumerable<string> UssFiles()
        {
            return Directory.GetFiles("Assets/UI", "*.uss", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles("Assets/Resources/UI", "*.uss", SearchOption.AllDirectories));
        }

        private static string Read(string assetsRelativePath)
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

        private static string Background(string body)
        {
            Match match = Regex.Match(body, @"background-color:\s*(?<value>[^;]+);");
            Assert.IsTrue(match.Success, "no background-color");
            return match.Groups["value"].Value.Trim();
        }

        private static float Alpha(string colour)
        {
            Match rgba = Regex.Match(colour, @"rgba\([^,]+,[^,]+,[^,]+,\s*(?<a>[0-9.]+)\)");
            return rgba.Success ? float.Parse(rgba.Groups["a"].Value, CultureInfo.InvariantCulture) : 1f;
        }

        private static float Px(string body, string property)
        {
            Match match = Regex.Match(body, @"(?m)^\s*" + Regex.Escape(property) + @":\s*(?<v>-?[0-9.]+)px");
            Assert.IsTrue(match.Success, $"{property} (px) not found");
            return float.Parse(match.Groups["v"].Value, CultureInfo.InvariantCulture);
        }
    }
}
