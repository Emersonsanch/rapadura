using System;
using System.Collections;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.UI.Achievements;
using Rapadura.UI.Crafting;
using Rapadura.UI.Inventory;
using Rapadura.UI.Menus;
using Rapadura.UI.Quests;
using Rapadura.UI.SkillTree;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Rapadura.Tests.PlayMode
{
    /// <summary>
    /// Opens and closes every screen controller the HUD/pause menu can reach in Main.unity through
    /// its public Open/Show/Close/Hide API and checks it is actually on screen in between (its
    /// panel root and every ancestor resolve to a visible display, on an active UIDocument), not
    /// just that an IsOpen flag flipped. One test case per screen so a missing/broken screen
    /// doesn't hide the others. Any exception or error log fails the case.
    /// </summary>
    [Category("Smoke")]
    public class UiScreensSmokeTests : SmokeTestBase
    {
        private sealed class ScreenHandle
        {
            public MonoBehaviour Controller;
            public string RootName;
            public Action Open;
            public Action Close;
            public Func<bool> IsOpen;
        }

        private static ScreenHandle Resolve(string screenId)
        {
            switch (screenId)
            {
                case "inventory":
                {
                    InventoryUIController c = FindInScene<InventoryUIController>();
                    return c == null ? null : new ScreenHandle { Controller = c, RootName = "inventory-root", Open = c.Show, Close = c.Hide, IsOpen = () => c.IsVisible };
                }
                case "questlog":
                {
                    QuestLogController c = FindInScene<QuestLogController>();
                    return c == null ? null : new ScreenHandle { Controller = c, RootName = "quest-log-root", Open = c.Open, Close = c.Hide, IsOpen = () => c.IsOpen };
                }
                case "crafting":
                {
                    CraftingUIController c = FindInScene<CraftingUIController>();
                    return c == null ? null : new ScreenHandle { Controller = c, RootName = "crafting-root", Open = c.Open, Close = c.Close, IsOpen = () => c.IsOpen };
                }
                case "skilltree":
                {
                    SkillTreeController c = FindInScene<SkillTreeController>();
                    return c == null ? null : new ScreenHandle { Controller = c, RootName = "skill-tree-root", Open = c.Show, Close = c.Hide, IsOpen = () => c.IsVisible };
                }
                case "settings":
                {
                    SettingsMenuController c = FindInScene<SettingsMenuController>();
                    return c == null ? null : new ScreenHandle { Controller = c, RootName = "settings-menu-root", Open = c.Show, Close = c.Hide, IsOpen = () => c.IsOpen };
                }
                case "achievements":
                {
                    AchievementListController c = FindInScene<AchievementListController>();
                    return c == null ? null : new ScreenHandle { Controller = c, RootName = "achievement-list-root", Open = c.Open, Close = c.Hide, IsOpen = () => c.IsOpen };
                }
                case "pause":
                {
                    PauseMenuController c = FindInScene<PauseMenuController>();
                    return c == null ? null : new ScreenHandle { Controller = c, RootName = "pause-menu-root", Open = c.Show, Close = c.Hide, IsOpen = () => c.IsOpen };
                }
                case "saveload":
                {
                    SaveLoadMenuController c = FindInScene<SaveLoadMenuController>();
                    return c == null ? null : new ScreenHandle { Controller = c, RootName = "save-load-menu-root", Open = () => c.Show(SaveLoadMenuController.ScreenMode.Load), Close = c.Hide, IsOpen = () => c.IsOpen };
                }
                default:
                    throw new ArgumentException($"Unknown screen id '{screenId}'.", nameof(screenId));
            }
        }

        [UnityTest]
        [Timeout(120000)]
        public IEnumerator Screen_OpensVisibly_AndCloses(
            [Values("inventory", "questlog", "crafting", "skilltree", "settings", "achievements", "pause", "saveload")] string screenId)
        {
            yield return LoadScene(MainSceneName);
            yield return SelectHero(CharacterId.Joaquim);

            ScreenHandle screen = Resolve(screenId);
            Assert.IsNotNull(screen, $"No controller for screen '{screenId}' exists in Main.unity — the HUD/pause button for it can only fail silently.");

            UIDocument document = screen.Controller.GetComponent<UIDocument>();
            Assert.IsNotNull(document, $"'{screenId}' controller has no UIDocument.");

            screen.Open();
            yield return WaitFrames(3);

            Assert.IsTrue(screen.IsOpen(), $"'{screenId}' does not report open after Open().");
            Assert.IsTrue(screen.Controller.gameObject.activeInHierarchy, $"'{screenId}' GameObject is inactive — opening it shows nothing.");
            Assert.IsTrue(document.enabled, $"'{screenId}' UIDocument is disabled.");
            Assert.IsNotNull(document.rootVisualElement, $"'{screenId}' UIDocument has no visual tree.");

            VisualElement root = document.rootVisualElement.Q<VisualElement>(screen.RootName);
            Assert.IsNotNull(root, $"'{screenId}' UXML has no '{screen.RootName}' element.");
            AssertVisible(root, screenId);

            screen.Close();
            yield return WaitFrames(3);

            Assert.IsFalse(screen.IsOpen(), $"'{screenId}' still reports open after closing.");
            Assert.AreEqual(DisplayStyle.None, root.resolvedStyle.display, $"'{screenId}' panel is still displayed after closing.");

            // Unexpected Error/Exception logs already fail the test (LogAssert default).
        }

        private static void AssertVisible(VisualElement element, string screenId)
        {
            for (VisualElement e = element; e != null; e = e.parent)
            {
                Assert.AreNotEqual(DisplayStyle.None, e.resolvedStyle.display,
                    $"'{screenId}' is open but '{e.name}' (or an ancestor) resolves to display:none.");
                Assert.AreNotEqual(Visibility.Hidden, e.resolvedStyle.visibility,
                    $"'{screenId}' is open but '{e.name}' (or an ancestor) is visibility:hidden.");
            }
        }
    }
}
