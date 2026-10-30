using System;
using System.Linq;
using Rapadura.UI.Achievements;
using Rapadura.UI.Crafting;
using Rapadura.UI.Inventory;
using Rapadura.UI.Menus;
using Rapadura.UI.Quests;
using Rapadura.UI.SkillTree;
using UnityEngine;

namespace Rapadura.Tests.PlayMode
{
    /// <summary>
    /// The screens the HUD/pause menu can open in Main.unity, each as an Open/Close/IsOpen handle on its
    /// controller's public API. Shared by <see cref="UiScreensSmokeTests"/> (one case per screen) and
    /// <see cref="SoakPlaythroughTests"/> (opens and closes them in rotation mid-fight).
    /// </summary>
    public static class UiScreenHandles
    {
        /// <summary>Every id <see cref="Resolve"/> knows.</summary>
        public static readonly string[] All = { "inventory", "questlog", "crafting", "skilltree", "settings", "achievements", "pause", "saveload" };

        public sealed class Handle
        {
            public MonoBehaviour Controller;
            public string RootName;
            public Action Open;
            public Action Close;
            public Func<bool> IsOpen;
        }

        public static Handle Resolve(string screenId)
        {
            switch (screenId)
            {
                case "inventory":
                {
                    InventoryUIController c = Find<InventoryUIController>();
                    return c == null ? null : new Handle { Controller = c, RootName = "inventory-root", Open = c.Show, Close = c.Hide, IsOpen = () => c.IsVisible };
                }
                case "questlog":
                {
                    QuestLogController c = Find<QuestLogController>();
                    return c == null ? null : new Handle { Controller = c, RootName = "quest-log-root", Open = c.Open, Close = c.Hide, IsOpen = () => c.IsOpen };
                }
                case "crafting":
                {
                    CraftingUIController c = Find<CraftingUIController>();
                    return c == null ? null : new Handle { Controller = c, RootName = "crafting-root", Open = c.Open, Close = c.Close, IsOpen = () => c.IsOpen };
                }
                case "skilltree":
                {
                    SkillTreeController c = Find<SkillTreeController>();
                    return c == null ? null : new Handle { Controller = c, RootName = "skill-tree-root", Open = c.Show, Close = c.Hide, IsOpen = () => c.IsVisible };
                }
                case "settings":
                {
                    SettingsMenuController c = Find<SettingsMenuController>();
                    return c == null ? null : new Handle { Controller = c, RootName = "settings-menu-root", Open = c.Show, Close = c.Hide, IsOpen = () => c.IsOpen };
                }
                case "achievements":
                {
                    AchievementListController c = Find<AchievementListController>();
                    return c == null ? null : new Handle { Controller = c, RootName = "achievement-list-root", Open = c.Open, Close = c.Hide, IsOpen = () => c.IsOpen };
                }
                case "pause":
                {
                    PauseMenuController c = Find<PauseMenuController>();
                    return c == null ? null : new Handle { Controller = c, RootName = "pause-menu-root", Open = c.Show, Close = c.Hide, IsOpen = () => c.IsOpen };
                }
                case "saveload":
                {
                    SaveLoadMenuController c = Find<SaveLoadMenuController>();
                    return c == null ? null : new Handle { Controller = c, RootName = "save-load-menu-root", Open = () => c.Show(SaveLoadMenuController.ScreenMode.Load), Close = c.Hide, IsOpen = () => c.IsOpen };
                }
                default:
                    throw new ArgumentException($"Unknown screen id '{screenId}'.", nameof(screenId));
            }
        }

        private static T Find<T>() where T : UnityEngine.Object
        {
            return UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
        }
    }
}
