namespace Rapadura.UI.Menus
{
    /// <summary>
    /// Pure, UIDocument-free presentation logic for a single save slot row in the Save/Load screen
    /// (<see cref="SaveLoadMenuController"/>). Kept separate from the controller — same reasoning as
    /// <c>Rapadura.UI.HUD.HudBarMath</c> — so slot-state rules (label text, which buttons should be
    /// enabled) are covered by EditMode tests without needing a real <c>UIDocument</c>.
    /// </summary>
    public static class SaveSlotPresenter
    {
        /// <summary>Visual/interaction state of one save slot.</summary>
        public enum SlotState
        {
            Empty,
            Occupied
        }

        public static SlotState GetState(bool slotExists)
        {
            return slotExists ? SlotState.Occupied : SlotState.Empty;
        }

        /// <summary>Human-readable slot name, 1-based for display ("Slot 1") regardless of the underlying 0-based index.</summary>
        public static string GetSlotTitle(int slotIndex)
        {
            return $"Slot {slotIndex + 1}";
        }

        public static string GetStatusLabel(bool slotExists)
        {
            return slotExists ? "Salvo" : "Vazio";
        }

        /// <summary>Save is always allowed — saving into an occupied slot overwrites it (with SaveManager's own backup safety net).</summary>
        public static bool CanSave(bool slotExists)
        {
            return true;
        }

        /// <summary>Load only makes sense for a slot that actually has data.</summary>
        public static bool CanLoad(bool slotExists)
        {
            return slotExists;
        }

        /// <summary>Delete only makes sense for a slot that actually has data.</summary>
        public static bool CanDelete(bool slotExists)
        {
            return slotExists;
        }

        /// <summary>Label for the slot's primary action button, which flips meaning between the Save and Load screens.</summary>
        public static string GetPrimaryActionLabel(bool isSaveMode, bool slotExists)
        {
            if (isSaveMode)
            {
                return slotExists ? "Sobrescrever" : "Salvar";
            }

            return slotExists ? "Carregar" : "Vazio";
        }

        /// <summary>Whether the slot's primary action button should be interactable, given the current screen mode.</summary>
        public static bool IsPrimaryActionEnabled(bool isSaveMode, bool slotExists)
        {
            return isSaveMode ? CanSave(slotExists) : CanLoad(slotExists);
        }
    }
}
