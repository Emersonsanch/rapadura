using System;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using UnityEngine;

namespace Rapadura.Gameplay.Characters
{
    /// <summary>JSON-serializable snapshot of <see cref="CharacterSelectionState"/>. The id is stored by name (not enum index) so reordering <see cref="CharacterId"/> can never silently swap heroes in old saves.</summary>
    [Serializable]
    public class CharacterSelectionSaveState
    {
        public bool hasSelection;
        public string characterId;
    }

    /// <summary>
    /// Raised when a save is loaded that contains a chosen hero — whoever owns the player visual/
    /// passive/skill set (today <c>CharacterSelectController</c>) re-applies that hero and skips the
    /// selection screen. Published from <see cref="CharacterSelectionState.RestoreState"/>, which
    /// restores first (<see cref="RestoreOrders.First"/>), so listeners run before
    /// PlayerStats/AttributeSet/SkillManager restore their own saved values on top.
    /// </summary>
    public readonly struct CharacterSelectionRestoredEvent : IGameEvent
    {
        public readonly CharacterId CharacterId;

        public CharacterSelectionRestoredEvent(CharacterId characterId)
        {
            CharacterId = characterId;
        }
    }

    /// <summary>
    /// Raised by <see cref="CharacterSelectionState.Select"/> when the player confirms a hero on the
    /// character select screen (not on a save restore — that is <see cref="CharacterSelectionRestoredEvent"/>).
    /// Lets the game flow leave the select screen and hero-specific UI (skill tree, quest tracker)
    /// switch to the new hero without holding a reference to the select screen.
    /// </summary>
    public readonly struct CharacterSelectedEvent : IGameEvent
    {
        public readonly CharacterId CharacterId;

        public CharacterSelectedEvent(CharacterId characterId)
        {
            CharacterId = characterId;
        }
    }

    /// <summary>
    /// Which hero the player picked. A top-level manager built by GameManager (so it survives the
    /// WorldMap/Main scene swaps) and registered with SaveManager under <see cref="SaveKey"/>. Before
    /// this existed the choice lived only in the character select screen's local state, so loading a
    /// save brought back stats/skills/inventory but not the hero they belonged to.
    /// </summary>
    public class CharacterSelectionState : IManager, ISaveable, IRestoreOrder
    {
        public const string Key = "SelectedCharacter";

        public string SaveKey => Key;

        /// <summary>Restores before every other saveable — see <see cref="CharacterSelectionRestoredEvent"/>.</summary>
        public int RestoreOrder => RestoreOrders.First;

        public CharacterId? Selected { get; private set; }

        public bool HasSelection => Selected.HasValue;

        public void Initialize()
        {
        }

        public void Shutdown()
        {
            Selected = null;
        }

        /// <summary>Records the hero chosen on the character select screen.</summary>
        public void Select(CharacterId id)
        {
            Selected = id;
            EventBus.Publish(new CharacterSelectedEvent(id));
        }

        /// <summary>Back to "no hero chosen" (e.g. a future "Novo Jogo").</summary>
        public void Clear()
        {
            Selected = null;
        }

        public object CaptureState()
        {
            return new CharacterSelectionSaveState
            {
                hasSelection = Selected.HasValue,
                characterId = Selected.HasValue ? Selected.Value.ToString() : string.Empty
            };
        }

        public void RestoreState(object state)
        {
            if (state is not CharacterSelectionSaveState saved)
            {
                return;
            }

            if (!saved.hasSelection)
            {
                Selected = null;
                return;
            }

            if (!TryParse(saved.characterId, out CharacterId id))
            {
                Debug.LogWarning($"[CharacterSelectionState] Unknown character id '{saved.characterId}' in save data. Keeping current selection.");
                return;
            }

            Selected = id;
            EventBus.Publish(new CharacterSelectionRestoredEvent(id));
        }

        /// <summary>Parses a saved id by name (case-insensitive), rejecting numeric strings that don't map to a defined hero.</summary>
        public static bool TryParse(string value, out CharacterId id)
        {
            return Enum.TryParse(value, true, out id) && Enum.IsDefined(typeof(CharacterId), id);
        }
    }
}
