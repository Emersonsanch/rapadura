using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Shop;
using Rapadura.UI.HUD;
using Rapadura.UI.Menus;
using Rapadura.UI.Shop;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the pure logic behind the HUD's screen buttons/shortcuts
    /// (<see cref="ScreenExclusivityCoordinator"/>), the skill-slot name fallback
    /// (<see cref="SkillSlotDisplayMath.FormatSlotName"/>) and the game's startup language.
    /// No UIDocument involved, same reasoning as <see cref="HudBarMathTests"/>.
    /// </summary>
    public class HudScreenNavigationTests
    {
        private sealed class FakeScreen
        {
            public bool Open;
            public int OpenCalls;
            public int CloseCalls;
        }

        private static FakeScreen Register(ScreenExclusivityCoordinator coordinator, string id)
        {
            var screen = new FakeScreen();
            coordinator.Register(id,
                () => screen.Open,
                () => { screen.Open = true; screen.OpenCalls++; },
                () => { screen.Open = false; screen.CloseCalls++; });
            return screen;
        }

        [Test]
        public void Open_ClosesEveryOtherOpenScreen()
        {
            var coordinator = new ScreenExclusivityCoordinator();
            FakeScreen quests = Register(coordinator, "quests");
            FakeScreen inventory = Register(coordinator, "inventory");
            FakeScreen skills = Register(coordinator, "skills");

            coordinator.Open("quests");
            coordinator.Open("inventory");

            Assert.IsFalse(quests.Open);
            Assert.IsTrue(inventory.Open);
            Assert.IsFalse(skills.Open);
            Assert.AreEqual(0, skills.CloseCalls, "Already-closed screens must not get a redundant Close().");
        }

        [Test]
        public void Toggle_OpenScreen_ClosesItWithoutTouchingOthers()
        {
            var coordinator = new ScreenExclusivityCoordinator();
            FakeScreen quests = Register(coordinator, "quests");
            FakeScreen inventory = Register(coordinator, "inventory");

            Assert.IsTrue(coordinator.Toggle("quests"));
            Assert.IsFalse(coordinator.Toggle("quests"));

            Assert.IsFalse(quests.Open);
            Assert.AreEqual(1, quests.CloseCalls);
            Assert.AreEqual(0, inventory.OpenCalls);
            Assert.IsFalse(coordinator.IsAnyOpen);
        }

        [Test]
        public void Toggle_ClosedScreen_OpensItExclusively()
        {
            var coordinator = new ScreenExclusivityCoordinator();
            FakeScreen quests = Register(coordinator, "quests");
            FakeScreen crafting = Register(coordinator, "crafting");
            crafting.Open = true; // e.g. opened by a workbench, not through the coordinator

            Assert.IsTrue(coordinator.Toggle("quests"));

            Assert.IsTrue(quests.Open);
            Assert.IsFalse(crafting.Open);
        }

        [Test]
        public void ScreenClosedByItsOwnButton_IsSeenAsClosed()
        {
            var coordinator = new ScreenExclusivityCoordinator();
            FakeScreen quests = Register(coordinator, "quests");

            coordinator.Open("quests");
            quests.Open = false; // its own Fechar button

            Assert.IsTrue(coordinator.Toggle("quests"), "Next toggle should reopen, not try to close.");
            Assert.AreEqual(2, quests.OpenCalls);
        }

        [Test]
        public void CloseAll_ReturnsClosedCount_AndHonoursException()
        {
            var coordinator = new ScreenExclusivityCoordinator();
            FakeScreen a = Register(coordinator, "a");
            FakeScreen b = Register(coordinator, "b");
            FakeScreen c = Register(coordinator, "c");
            a.Open = b.Open = c.Open = true;

            Assert.AreEqual(2, coordinator.CloseAll(exceptId: "b"));
            Assert.IsTrue(b.Open);
            Assert.AreEqual(1, coordinator.CloseAll());
            Assert.AreEqual(0, coordinator.CloseAll());
        }

        [Test]
        public void UnknownId_IsIgnored()
        {
            var coordinator = new ScreenExclusivityCoordinator();
            FakeScreen a = Register(coordinator, "a");
            a.Open = true;

            Assert.IsFalse(coordinator.Open("missing"));
            Assert.IsFalse(coordinator.Toggle("missing"));
            Assert.IsTrue(a.Open, "Unknown ids must not close the open screen.");
        }

        [Test]
        public void Register_SameIdTwice_ReplacesEntry()
        {
            var coordinator = new ScreenExclusivityCoordinator();
            Register(coordinator, "a");
            FakeScreen replacement = Register(coordinator, "a");

            coordinator.Open("a");

            Assert.AreEqual(1, coordinator.Count);
            Assert.IsTrue(replacement.Open);
        }

        [Test]
        public void HudScreenIds_AreUnique()
        {
            var ids = new HashSet<string>
            {
                HudController.SkillTreeScreenId,
                HudController.InventoryScreenId,
                HudController.QuestLogScreenId,
                HudController.AchievementsScreenId,
                HudController.CraftingScreenId,
                HudController.ShopScreenId,
                HudController.CharacterSheetScreenId,
            };

            Assert.AreEqual(7, ids.Count);
        }

        // ---------------------------------------------------------------
        // Open with a caller-supplied open (crafting bancada with its station)
        // ---------------------------------------------------------------

        [Test]
        public void OpenWithOverride_RunsOverrideInsteadOfRegisteredOpen_AndClosesOthers()
        {
            var coordinator = new ScreenExclusivityCoordinator();
            FakeScreen crafting = Register(coordinator, HudController.CraftingScreenId);
            FakeScreen shop = Register(coordinator, HudController.ShopScreenId);
            shop.Open = true;
            int overrideCalls = 0;

            Assert.IsTrue(coordinator.Open(HudController.CraftingScreenId, () => { crafting.Open = true; overrideCalls++; }));

            Assert.AreEqual(1, overrideCalls);
            Assert.AreEqual(0, crafting.OpenCalls, "The registered (parameterless) open must not run when an override is given.");
            Assert.IsTrue(crafting.Open);
            Assert.IsFalse(shop.Open);
        }

        [Test]
        public void OpenWithOverride_RunsEvenWhenAlreadyOpen()
        {
            var coordinator = new ScreenExclusivityCoordinator();
            FakeScreen crafting = Register(coordinator, "crafting");
            crafting.Open = true; // opened by another bancada
            int overrideCalls = 0;

            coordinator.Open("crafting", () => overrideCalls++);

            Assert.AreEqual(1, overrideCalls, "A second station must re-open the window with its own context.");
            Assert.AreEqual(0, crafting.CloseCalls);
        }

        [Test]
        public void OpenWithOverride_UnknownId_DoesNotRunOverride()
        {
            var coordinator = new ScreenExclusivityCoordinator();
            bool ran = false;

            Assert.IsFalse(coordinator.Open("missing", () => ran = true));
            Assert.IsFalse(ran, "Callers fall back to opening directly, so the override must not run twice.");
        }

        [Test]
        public void OpenWithNullOverride_BehavesLikePlainOpen()
        {
            var coordinator = new ScreenExclusivityCoordinator();
            FakeScreen a = Register(coordinator, "a");

            coordinator.Open("a", null);
            coordinator.Open("a", null);

            Assert.IsTrue(a.Open);
            Assert.AreEqual(1, a.OpenCalls);
        }

        // ---------------------------------------------------------------
        // Shop public open/close
        // ---------------------------------------------------------------

        [Test]
        public void ShopUI_OpenWithoutActiveSession_StaysClosed_AndCloseIsSafe()
        {
            var go = new GameObject("ShopUITest");

            try
            {
                var shopUI = go.AddComponent<ShopUIController>();
                shopUI.SetShopManager(new ShopManager());

                Assert.IsFalse(shopUI.IsOpen);
                Assert.IsFalse(shopUI.Open(), "A shop can only be shown for a session an NPC started.");
                Assert.IsFalse(shopUI.IsOpen);

                Assert.DoesNotThrow(shopUI.Close);
                Assert.IsFalse(shopUI.IsOpen);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        // ---------------------------------------------------------------
        // Settings rebind rows for the HUD shortcuts
        // ---------------------------------------------------------------

        [Test]
        public void RebindRows_IncludeHudShortcutActions()
        {
            var actions = new HashSet<string>();
            foreach (RebindRowDefinition row in SettingsMenuTexts.Rows)
            {
                actions.Add(row.ActionName);
            }

            foreach (string expected in new[]
                     {
                         HudController.QuestLogActionName,
                         HudController.InventoryActionName,
                         HudController.SkillTreeActionName,
                         HudController.CharacterSheetActionName,
                         HudController.PauseActionName,
                     })
            {
                Assert.IsTrue(actions.Contains(expected), $"Missing Settings rebind row for {expected}");
            }
        }

        [Test]
        public void RebindRows_OnlyPauseKeyboardIsLocked()
        {
            foreach (RebindRowDefinition row in SettingsMenuTexts.Rows)
            {
                Assert.AreEqual(row.ActionName == HudController.PauseActionName, row.KeyboardLocked, row.ActionName);
            }
        }

        [Test]
        public void RebindRows_HudShortcutLabels_HaveEnglishAndPortugueseTexts()
        {
            foreach (RebindRowDefinition row in SettingsMenuTexts.Rows)
            {
                LocalizationEntry entry = SettingsMenuTexts.Entries.Find(e => e.Key == row.LabelKey);
                Assert.IsNotNull(entry, row.LabelKey);
                Assert.IsFalse(string.IsNullOrEmpty(entry.En), row.LabelKey);
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), row.LabelKey);
            }
        }

        [Test]
        public void PlayerControls_EveryRebindRowActionExists_AndPauseKeyboardIsTheCancelKey()
        {
            string json = System.IO.File.ReadAllText(System.IO.Path.Combine("Assets", "InputActions", "PlayerControls.inputactions"));
            var asset = UnityEngine.InputSystem.InputActionAsset.FromJson(json);

            try
            {
                foreach (RebindRowDefinition row in SettingsMenuTexts.Rows)
                {
                    Assert.IsNotNull(asset.FindAction(row.ActionName, throwIfNotFound: false), $"Row '{row.ActionName}' has no action.");
                }

                // The keyboard Pause row is locked because its key IS the rebind cancel key.
                UnityEngine.InputSystem.InputAction pause = asset.FindAction(HudController.PauseActionName);
                bool pauseOnCancelKey = false;
                foreach (UnityEngine.InputSystem.InputBinding binding in pause.bindings)
                {
                    pauseOnCancelKey |= binding.path == Rapadura.Core.Accessibility.InputRebindManager.DefaultCancelPath;
                }

                Assert.IsTrue(pauseOnCancelKey, "Pause's keyboard binding should be the rebind cancel key (Esc).");
            }
            finally
            {
                Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void PlayerControls_DefinesHudShortcutActions()
        {
            string json = System.IO.File.ReadAllText(System.IO.Path.Combine("Assets", "InputActions", "PlayerControls.inputactions"));
            InputActionAssetHasActions(json,
                HudController.QuestLogActionName,
                HudController.InventoryActionName,
                HudController.SkillTreeActionName,
                HudController.CharacterSheetActionName,
                HudController.PauseActionName);
        }

        private static void InputActionAssetHasActions(string json, params string[] names)
        {
            var asset = UnityEngine.InputSystem.InputActionAsset.FromJson(json);

            try
            {
                foreach (string name in names)
                {
                    Assert.IsNotNull(asset.FindAction(name, throwIfNotFound: false), $"Missing action '{name}'.");
                }
            }
            finally
            {
                Object.DestroyImmediate(asset);
            }
        }

        // ---------------------------------------------------------------
        // Skill slot name (no mid-word wraps)
        // ---------------------------------------------------------------

        [TestCase("Disparo Explosivo", 8, "Disparo\nExplosi.")]
        [TestCase("Chuva de Flechas", 8, "Chuva\nFlechas")]
        [TestCase("Golpe", 8, "Golpe")]
        [TestCase("Golpe do Martelo Sagrado", 8, "Golpe\nMartelo")]
        [TestCase("Disparo Explosivo", 4, "Dis.\nExp.")]
        [TestCase("  ", 8, "")]
        [TestCase(null, 8, "")]
        public void FormatSlotName_SplitsOnWholeWords(string name, int maxChars, string expected)
        {
            Assert.AreEqual(expected, SkillSlotDisplayMath.FormatSlotName(name, maxChars));
        }

        [Test]
        public void FormatSlotName_NoLineExceedsBudget()
        {
            string formatted = SkillSlotDisplayMath.FormatSlotName("Tempestade Eletromagnética Devastadora", 6);

            foreach (string line in formatted.Split('\n'))
            {
                Assert.LessOrEqual(line.Length, 6, line);
            }

            Assert.LessOrEqual(formatted.Split('\n').Length, SkillSlotDisplayMath.SlotNameMaxLines);
        }

        [Test]
        public void SlotNameCharsPerLine_ShrinksWithFontScale_ButNotBelowMinimum()
        {
            Assert.AreEqual(SkillSlotDisplayMath.SlotNameBaseCharsPerLine, SkillSlotDisplayMath.SlotNameCharsPerLine(1f));
            Assert.AreEqual(SkillSlotDisplayMath.SlotNameBaseCharsPerLine, SkillSlotDisplayMath.SlotNameCharsPerLine(0.75f));
            Assert.AreEqual(4, SkillSlotDisplayMath.SlotNameCharsPerLine(2f));
            Assert.AreEqual(SkillSlotDisplayMath.SlotNameMinCharsPerLine, SkillSlotDisplayMath.SlotNameCharsPerLine(10f));
        }

        // ---------------------------------------------------------------
        // Startup language (pt-BR is the primary language)
        // ---------------------------------------------------------------

        [TestCase(SystemLanguage.English, LanguageCode.en)]
        [TestCase(SystemLanguage.Portuguese, LanguageCode.ptBR)]
        [TestCase(SystemLanguage.Spanish, LanguageCode.ptBR)]
        [TestCase(SystemLanguage.Unknown, LanguageCode.ptBR)]
        public void ResolveStartupLanguage_DefaultsToPtBR_UnlessDeviceIsEnglish(SystemLanguage system, LanguageCode expected)
        {
            Assert.AreEqual(expected, LocalizationManager.ResolveStartupLanguage(system));
        }
    }
}
