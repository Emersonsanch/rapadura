using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.Story;
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

        /// <summary>
        /// "Nova Jornada+" cycle of the save "Continuar" would load (for its small badge); 0 without a save or for a
        /// save written before Nova Jornada+ existed (no "NewGamePlus" entry).
        /// </summary>
        public static int GetContinueCycle(SaveManager saveManager)
        {
            int slot = GetContinueSlot(saveManager);
            if (slot == GameFlowMachine.NoSlot)
            {
                return 0;
            }

            return SaveManager.TryGetEntryState(saveManager.PeekSlot(slot), NewGamePlusProgress.Key, out NewGamePlusSaveState state)
                ? NewGamePlusRules.ClampCycle(state.cycle)
                : 0;
        }

        public static bool IsLoadEnabled(SaveManager saveManager, bool hasLoadMenu)
        {
            return hasLoadMenu && GameFlowService.FindNewestSaveSlot(saveManager, GameFlowService.ManualSlots) != GameFlowMachine.NoSlot;
        }
    }
}
