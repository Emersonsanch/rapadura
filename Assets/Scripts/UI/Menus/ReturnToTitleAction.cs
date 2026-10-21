using Rapadura.Gameplay.Flow;
using Rapadura.Save;

namespace Rapadura.UI.Menus
{
    /// <summary>Buttons of the pause menu's "Voltar ao menu?" dialog.</summary>
    public enum ReturnToTitleChoice
    {
        Cancel = 0,
        /// <summary>Auto-save first (the slot "Continuar" prefers when newest), then leave.</summary>
        SaveAndQuit = 1,
        QuitWithoutSaving = 2,
    }

    /// <summary>
    /// What a <see cref="ReturnToTitleChoice"/> does, kept out of <see cref="PauseMenuController"/> so
    /// it is testable without a UIDocument: optionally auto-saves, then
    /// <see cref="GameFlowService.ReturnToTitle"/> (abandon the encounter, MainMenu state, load the
    /// world map where GameFrontEndController shows the title screen).
    /// </summary>
    public static class ReturnToTitleAction
    {
        /// <summary>True when the session was left (the flow switched to the title screen).</summary>
        public static bool Execute(ReturnToTitleChoice choice, SaveManager saveManager, GameFlowService flow)
        {
            if (choice == ReturnToTitleChoice.Cancel)
            {
                return false;
            }

            if (choice == ReturnToTitleChoice.SaveAndQuit)
            {
                saveManager?.AutoSave();
            }

            return flow != null && flow.ReturnToTitle();
        }
    }
}
