using Rapadura.Gameplay.Flow;
using Rapadura.Save;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// Pure enabled-state rules for <see cref="MainMenuController"/>'s save buttons, kept out of the
    /// UIDocument-driven controller so they are unit tested on their own (same split as
    /// <see cref="SaveSlotPresenter"/>/<see cref="SaveLoadMenuController"/>).
    /// <list type="bullet">
    /// <item>"Continuar": enabled when the auto-save or any manual slot holds a readable save
    /// (<see cref="GameFlowService.DefaultContinueSlots"/>); it loads the newest one.</item>
    /// <item>"Carregar": enabled when a Save/Load screen exists and one of the manual slots it lists
    /// (<see cref="GameFlowService.ManualSlots"/>) holds a readable save — the auto-save is not listed
    /// there, "Continuar" covers it.</item>
    /// </list>
    /// </summary>
    public static class MainMenuPresenter
    {
        /// <summary>Slot "Continuar" would load right now, or <see cref="GameFlowMachine.NoSlot"/>.</summary>
        public static int GetContinueSlot(SaveManager saveManager)
        {
            return GameFlowService.FindNewestSaveSlot(saveManager, GameFlowService.DefaultContinueSlots);
        }

        public static bool IsContinueEnabled(SaveManager saveManager)
        {
            return GetContinueSlot(saveManager) != GameFlowMachine.NoSlot;
        }

        public static bool IsLoadEnabled(SaveManager saveManager, bool hasLoadMenu)
        {
            return hasLoadMenu && GameFlowService.FindNewestSaveSlot(saveManager, GameFlowService.ManualSlots) != GameFlowMachine.NoSlot;
        }
    }
}
