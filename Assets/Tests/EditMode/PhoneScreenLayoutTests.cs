using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Rapadura.Tests
{
    /// <summary>
    /// Phone audit (2026-11): every tappable control of the main screens (Mochila, Ficha, Skills, Missões, Loja, diálogo, casa,
    /// Caderno, Conquistas, Configurações, seleção de primo) is at least 48px tall in panel units, the same budget
    /// <see cref="MapTravelWindowTests"/> holds the Barcas de Luz window to. EditMode reads the USS numbers (no rendering);
    /// a fixed <c>height</c> counts, else <c>min-height</c>. Plus the overlaps the same audit found and fixed (title ribbon
    /// over "Sair", Dupla over "Girar ›", "Mapa (M)" over "Menu", the Caderno header squeezed flat, Configurações without scroll).
    /// </summary>
    public class PhoneScreenLayoutTests
    {
        private const float MinTouch = 48f;

        private static readonly object[] Targets =
        {
            new object[] { "Assets/UI/Inventory/InventoryView.uss", ".inventory-close-button" },
            new object[] { "Assets/UI/Inventory/InventoryView.uss", ".inventory-sort-button" },
            new object[] { "Assets/UI/Inventory/InventoryView.uss", ".inventory-row-button" },
            new object[] { "Assets/Resources/UI/CharacterSheetView.uss", ".character-sheet-close-button" },
            new object[] { "Assets/UI/SkillTree/SkillTreeView.uss", ".skill-tree-close-button" },
            new object[] { "Assets/UI/Quests/QuestLogView.uss", ".quest-log-close-button" },
            new object[] { "Assets/UI/Quests/QuestLogView.uss", ".quest-log-tab" },
            new object[] { "Assets/UI/Shop/ShopView.uss", ".shop-close-button" },
            new object[] { "Assets/UI/Shop/ShopView.uss", ".shop-row-button" },
            new object[] { "Assets/UI/Dialogue/DialogueView.uss", ".dialogue-choice-button" },
            new object[] { "Assets/UI/Dialogue/DialogueView.uss", ".dialogue-continue-button" },
            new object[] { "Assets/Resources/UI/House.uss", ".house-tab" },
            new object[] { "Assets/Resources/UI/House.uss", ".house-life-close" },
            new object[] { "Assets/Resources/UI/BestiaryView.uss", ".bestiary-close" },
            new object[] { "Assets/Resources/UI/BestiaryView.uss", ".bestiary-list-item" },
            new object[] { "Assets/UI/Achievements/AchievementListView.uss", ".achievement-list-close-button" },
            new object[] { "Assets/UI/Menus/SettingsMenuView.uss", ".settings-close-button" },
            new object[] { "Assets/UI/Menus/SettingsMenuView.uss", ".settings-tab-button" },
            new object[] { "Assets/UI/Menus/SettingsMenuView.uss", ".settings-rebind-button,\n.settings-rebind-reset-button" },
            new object[] { "Assets/UI/Menus/SettingsMenuView.uss", ".settings-rebind-footer-button" },
            new object[] { "Assets/UI/CharacterSelect/CharacterSelectView.uss", ".character-select__continue" },
            new object[] { "Assets/UI/HUD/HudView.uss", ".hud-camera-button" },
            new object[] { "Assets/UI/HUD/HudView.uss", ".hud-skill-button" },
            new object[] { "Assets/UI/HUD/HudView.uss", ".hud-menu-button" },
        };

        [TestCaseSource(nameof(Targets))]
        public void TouchTarget_IsAtLeast48pxTall(string ussPath, string selector)
        {
            string block = Block(File.ReadAllText(ussPath), selector);
            float height = Px(block, "height") ?? Px(block, "min-height") ?? 0f;

            Assert.GreaterOrEqual(height, MinTouch, ussPath + " " + selector);
        }

        /// <summary>Text buttons grow with the accessibility font scale instead of clipping: min-height, not a fixed height.</summary>
        [TestCase("Assets/Resources/UI/CharacterSheetView.uss", ".character-sheet-close-button")]
        [TestCase("Assets/UI/SkillTree/SkillTreeView.uss", ".skill-tree-close-button")]
        [TestCase("Assets/UI/Quests/QuestLogView.uss", ".quest-log-close-button")]
        [TestCase("Assets/UI/Dialogue/DialogueView.uss", ".dialogue-continue-button")]
        [TestCase("Assets/UI/Achievements/AchievementListView.uss", ".achievement-list-close-button")]
        [TestCase("Assets/UI/Menus/SettingsMenuView.uss", ".settings-close-button")]
        [TestCase("Assets/UI/CharacterSelect/CharacterSelectView.uss", ".character-select__continue")]
        public void TextButtons_GrowWithTheirLabel(string ussPath, string selector)
        {
            string block = Block(File.ReadAllText(ussPath), selector);
            StringAssert.IsMatch(@"(^|\s)height:\s*auto", block, selector + " keeps a fixed height (the label clips at a larger font scale)");
            Assert.GreaterOrEqual(Px(block, "min-height") ?? 0f, MinTouch, selector);
        }

        // ------------------------------------------------------------------
        // Overlaps found on a 2400x1080 phone (panel ~1433x645)
        // ------------------------------------------------------------------

        [Test]
        public void TitleDailyRibbon_StaysOffTheMenuColumn()
        {
            string block = Block(File.ReadAllText("Assets/Resources/UI/DailyRewards.uss"), ".daily-login-title-banner");
            StringAssert.DoesNotMatch(@"(^|[\s;])bottom\s*:", block, "a bottom-centre ribbon covered \"Sair\" on 20:9 phones");
            Assert.IsNotNull(Px(block, "top"));
            Assert.GreaterOrEqual(Px(block, "min-height") ?? 0f, MinTouch);
        }

        [Test]
        public void DuoButton_SitsAboveTheTouchCameraRow()
        {
            string hud = File.ReadAllText("Assets/UI/HUD/HudView.uss");
            string camera = Block(hud, ".hud-camera-cluster");
            float cameraTop = (Px(camera, "bottom") ?? 0f) + (Px(Block(hud, ".hud-camera-button"), "height") ?? 0f);
            float duoBottom = Px(Block(File.ReadAllText("Assets/Resources/UI/DuoStrike.uss"), ".duo-strike-button"), "bottom") ?? 0f;
            Assert.GreaterOrEqual(duoBottom, cameraTop, "the Dupla button covers \"Girar ›\"");
        }

        [Test]
        public void FloatingMapButton_StaysBelowTheHudMenuRow()
        {
            string hud = File.ReadAllText("Assets/UI/HUD/HudView.uss");
            float rowBottom = (Px(Block(hud, ".hud-top-right-cluster"), "top") ?? 0f) + (Px(Block(hud, ".hud-menu-button"), "height") ?? 0f);
            float compassTop = Px(Block(File.ReadAllText("Assets/Resources/UI/FastTravelWindow.uss"), ".ftw-compass"), "top") ?? 0f;
            Assert.GreaterOrEqual(compassTop, rowBottom, "\"Mapa (M)\" sits on the HUD's Menu button");
        }

        [Test]
        public void Caderno_HeaderAndTabsNeverShrink_AndTheCardStaysOpaque()
        {
            StringAssert.IsMatch(@"flex-shrink:\s*0", Block(File.ReadAllText("Assets/Resources/UI/BestiaryView.uss"), ".bestiary-header"));
            string hunting = File.ReadAllText("Assets/Resources/UI/Hunting.uss");
            StringAssert.IsMatch(@"flex-shrink:\s*0", Block(hunting, ".hunting-tabs"));
            StringAssert.Contains("var(--rap-brown-deep)", Block(hunting, ".bestiary-card.rap-panel"), "the injected Hunting.uss must not let the theme's 0.92 panel win");
        }

        [Test]
        public void Settings_ContentScrolls_UnderAFixedHeaderAndTabBar()
        {
            StringAssert.IsMatch("<ui:ScrollView name=\"settings-content\"[^>]*mode=\"Vertical\"", File.ReadAllText("Assets/UI/Menus/SettingsMenuView.uxml"));
            string uss = File.ReadAllText("Assets/UI/Menus/SettingsMenuView.uss");
            StringAssert.IsMatch(@"min-height:\s*0", Block(uss, ".settings-content"));
            StringAssert.IsMatch(@"flex-shrink:\s*0", Block(uss, ".settings-header,\n.settings-tab-bar"));
        }

        // ------------------------------------------------------------------
        // Redesigns after the audit (Skills, Loja, Caderno list, quest card vs Dupla)
        // ------------------------------------------------------------------

        /// <summary>Phone panel height at 2400x1080 (DefaultPanelSettings: 1200x800 reference, match 0.5), the tightest case.</summary>
        private const float PhonePanelHeight = 645f;

        [Test]
        public void SkillTree_CanvasIsSizedToTheTree_AndAHeroTreeFitsAPhoneWithoutScrolling()
        {
            string uss = File.ReadAllText("Assets/UI/SkillTree/SkillTreeView.uss");
            string canvas = Block(uss, ".skill-tree-canvas");
            Assert.IsNull(Px(canvas, "min-width"), "a fixed canvas width scrolled 5 nodes sideways on a phone");
            Assert.IsNull(Px(canvas, "min-height"), "a fixed canvas height scrolled 5 nodes on a phone");
            StringAssert.IsMatch("<ui:ScrollView name=\"skill-tree-scroll\"[^>]*mode=\"Vertical\"", File.ReadAllText("Assets/UI/SkillTree/SkillTreeView.uxml"));

            // A hero tree is 4 columns x 2 rows (SkillTreeController: 120px cells, 84px nodes, 24px for the "1/5" label).
            float node = Px(Block(uss, ".skill-node"), "width") ?? 0f;
            float treeHeight = 120f + node + 24f;
            float[] canvasMargins = Margins(canvas);
            float header = Px(Block(uss, ".skill-tree-close-button"), "min-height") ?? 0f;
            float detail = Px(Block(uss, ".skill-detail-panel"), "min-height") ?? 0f;
            Assert.LessOrEqual(header + 16f + canvasMargins[0] + treeHeight + canvasMargins[2] + detail, PhonePanelHeight);

            // Nodes without an icon (none has one yet) show the skill name like the HUD hotbar does.
            StringAssert.IsMatch(@"white-space:\s*nowrap", Block(uss, ".skill-node-name"));
        }

        [Test]
        public void Shop_HasTheModalBackdrop_AndHoldsThePause()
        {
            string root = Block(File.ReadAllText("Assets/UI/Shop/ShopView.uss"), ".shop-root");
            StringAssert.Contains("rgba(20, 10, 4, 0.86)", root, "same backdrop as Mochila / Missões / Conquistas");
            string controller = File.ReadAllText("Assets/Scripts/UI/Shop/ShopUIController.cs");
            StringAssert.Contains("GameplayPause.Acquire(this)", controller, "MessageCards hold their cards while GameplayPause is held");
        }

        [Test]
        public void Caderno_ListRows_StayClearOfTheScroller()
        {
            float[] margins = Margins(Block(File.ReadAllText("Assets/Resources/UI/BestiaryView.uss"), ".bestiary-list-item.rap-button"));
            // One class would tie .rap-button (margin 0 0 10px 0), which Hunting.uss re-imports deeper in the tree and so wins.
            Assert.GreaterOrEqual(margins[1], 8f, "the rows' right border sat under the list's scroll bar");
        }

        [Test]
        public void QuestCard_StaysAboveTheDuoButton_OnAPhone()
        {
            string tracker = File.ReadAllText("Assets/UI/Quests/QuestTrackerView.uss");
            string card = Block(tracker, ".quest-tracker-card");
            float cardBottom = (Px(card, "top") ?? 0f) + (Px(card, "max-height") ?? float.PositiveInfinity);
            StringAssert.IsMatch(@"overflow:\s*hidden", card);

            string duo = Block(File.ReadAllText("Assets/Resources/UI/DuoStrike.uss"), ".duo-strike-button");
            float duoTop = PhonePanelHeight - (Px(duo, "bottom") ?? 0f) - (Px(duo, "height") ?? 0f);
            Assert.LessOrEqual(cardBottom + 12f, duoTop, "a 3-objective quest card ran into the Dupla button");

            StringAssert.IsMatch(@"white-space:\s*nowrap", Block(tracker, ".quest-tracker-objective-text"), "one line per objective");
            StringAssert.IsMatch(@"white-space:\s*nowrap", Block(tracker, ".quest-tracker-title"), "one line for the title");
            StringAssert.IsMatch(@"flex-shrink:\s*0", Block(tracker, ".quest-tracker-objective"), "capped card squashed the rows onto each other");
        }

        /// <summary>top, right, bottom, left of the <c>margin</c> shorthand (1 to 4 values), overridden by any longhand.</summary>
        private static float[] Margins(string block)
        {
            var result = new float[4];
            Match m = Regex.Match(block, @"(^|[\s;])margin:\s*([^;]+);");
            if (m.Success)
            {
                string[] parts = m.Groups[2].Value.Trim().Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                float[] v = new float[parts.Length];
                for (int i = 0; i < parts.Length; i++)
                {
                    float.TryParse(parts[i].Replace("px", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out v[i]);
                }

                switch (v.Length)
                {
                    case 1: result = new[] { v[0], v[0], v[0], v[0] }; break;
                    case 2: result = new[] { v[0], v[1], v[0], v[1] }; break;
                    case 3: result = new[] { v[0], v[1], v[2], v[1] }; break;
                    default: result = new[] { v[0], v[1], v[2], v[3] }; break;
                }
            }

            string[] sides = { "margin-top", "margin-right", "margin-bottom", "margin-left" };
            for (int i = 0; i < 4; i++)
            {
                result[i] = Px(block, sides[i]) ?? result[i];
            }

            return result;
        }

        private static string Block(string uss, string selector)
        {
            string stripped = Regex.Replace(uss.Replace("\r\n", "\n"), @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            Match m = Regex.Match(stripped, "(^|\\n)" + Regex.Escape(selector) + "\\s*\\{([^}]*)\\}");
            Assert.IsTrue(m.Success, selector + " not in the USS");
            return m.Groups[2].Value;
        }

        private static float? Px(string block, string property)
        {
            Match m = Regex.Match(block, "(^|[\\s;])" + Regex.Escape(property) + ":\\s*([\\d.]+)px");
            return m.Success ? float.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture) : (float?)null;
        }
    }
}
