using System;
using System.Collections.Generic;
using System.Globalization;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Logging;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;

namespace Rapadura.Gameplay.Flow
{
    /// <summary>Raised whenever <see cref="GameFlowService.State"/> changes.</summary>
    public readonly struct GameFlowStateChangedEvent : IGameEvent
    {
        public readonly GameFlowState Previous;
        public readonly GameFlowState Current;

        public GameFlowStateChangedEvent(GameFlowState previous, GameFlowState current)
        {
            Previous = previous;
            Current = current;
        }
    }

    /// <summary>
    /// Owns the session flow (title → hero → world map ⇄ encounters). Built by GameManager, so it
    /// survives the WorldMap/Main scene swaps like every other manager; the scene side is
    /// <c>GameFrontEndController</c> in WorldMap.unity, which shows the title/hero screens for the
    /// current <see cref="State"/> and calls <see cref="StartNewGame"/>/<see cref="Continue"/>.
    /// Encounter and hero transitions are event-driven (<see cref="EncounterStartedEvent"/>,
    /// <see cref="EncounterEndedEvent"/>, <see cref="CharacterSelectedEvent"/>,
    /// <see cref="CharacterSelectionRestoredEvent"/>), so nothing else needs a reference to it.
    /// Rules live in the pure <see cref="GameFlowMachine"/>.
    /// </summary>
    public class GameFlowService : IManager
    {
        private const string LogCategory = "GameFlow";

        /// <summary>
        /// Slots "Continuar" considers — the auto-save plus every manual slot the Save/Load screen
        /// offers (0..<see cref="SaveManager.ManualSlotCount"/>-1); the most recently saved one wins.
        /// </summary>
        public static readonly int[] DefaultContinueSlots = BuildSlotList(includeAutoSave: true);

        /// <summary>The manual slots only (what the title screen's "Carregar" lists).</summary>
        public static readonly int[] ManualSlots = BuildSlotList(includeAutoSave: false);

        private readonly GameFlowMachine _machine = new GameFlowMachine();
        private readonly CharacterSelectionState _selection;
        private readonly SaveManager _saveManager;
        private readonly NewSessionReset _sessionReset;
        private readonly SceneTransitionService _transitions;

        public GameFlowState State => _machine.State;

        public bool HasHero => _selection != null && _selection.HasSelection;

        /// <summary>False while the hero picker was opened by "Novo jogo" (see <see cref="GameFlowMachine.OffersSavedContinueInSelect"/>).</summary>
        public bool OffersSavedContinueInSelect => _machine.OffersSavedContinueInSelect;

        /// <param name="sessionReset">Run on "Novo jogo" (null = only the hero and the carried player state are cleared).</param>
        /// <param name="transitions">Used by <see cref="ReturnToTitle"/> to drop the pending encounter and load the world map (null = state change only, e.g. tests).</param>
        public GameFlowService(CharacterSelectionState selection, SaveManager saveManager, NewSessionReset sessionReset = null, SceneTransitionService transitions = null)
        {
            _selection = selection;
            _saveManager = saveManager;
            _sessionReset = sessionReset;
            _transitions = transitions;
        }

        public void Initialize()
        {
            EventBus.Subscribe<EncounterStartedEvent>(OnEncounterStarted);
            EventBus.Subscribe<EncounterEndedEvent>(OnEncounterEnded);
            EventBus.Subscribe<CharacterSelectedEvent>(OnCharacterSelected);
            EventBus.Subscribe<CharacterSelectionRestoredEvent>(OnCharacterRestored);
        }

        public void Shutdown()
        {
            EventBus.Unsubscribe<EncounterStartedEvent>(OnEncounterStarted);
            EventBus.Unsubscribe<EncounterEndedEvent>(OnEncounterEnded);
            EventBus.Unsubscribe<CharacterSelectedEvent>(OnCharacterSelected);
            EventBus.Unsubscribe<CharacterSelectionRestoredEvent>(OnCharacterRestored);
        }

        /// <summary>Called by the world map's front-end on scene start; returns the state to present.</summary>
        public GameFlowState EnterFrontEnd()
        {
            Apply(() => _machine.EnterFrontEnd(HasHero));
            return State;
        }

        /// <summary>
        /// "Novo jogo": every per-save system back to new-game state (<see cref="NewSessionReset"/> —
        /// quests, map, hero, bosses, clock, carried player state...; achievements are kept, they are
        /// per-profile), then open the hero picker.
        /// </summary>
        public bool StartNewGame()
        {
            if (State != GameFlowState.MainMenu)
            {
                return false;
            }

            if (_sessionReset != null)
            {
                _sessionReset.ResetAll();
            }

            // Also covered by the reset when both are registered with it; kept for callers without one.
            _selection?.Clear();
            _saveManager?.ClearCarriedState();
            return Apply(_machine.StartNewGame);
        }

        /// <summary>Whether any of <paramref name="slots"/> (default <see cref="DefaultContinueSlots"/>) holds a save.</summary>
        public bool CanContinue(int[] slots = null)
        {
            return FindContinueSlot(slots) != GameFlowMachine.NoSlot;
        }

        /// <summary>
        /// "Continuar": loads the newest save among <paramref name="slots"/>, then goes to the world
        /// map (or the hero picker if that save has no hero). False if nothing could be loaded.
        /// </summary>
        public bool Continue(int[] slots = null)
        {
            if (State != GameFlowState.MainMenu || _saveManager == null)
            {
                return false;
            }

            int slot = FindContinueSlot(slots);

            if (slot == GameFlowMachine.NoSlot || !_saveManager.Load(slot))
            {
                GameLogger.Warning(LogCategory, "Continuar: no loadable save found.");
                return false;
            }

            return Apply(() => _machine.CompleteContinue(HasHero));
        }

        /// <summary>Title screen "Carregar": loads exactly <paramref name="slot"/>, then the same transition as <see cref="Continue"/>.</summary>
        public bool LoadFromTitle(int slot) => Continue(new[] { slot });

        public bool CancelCharacterSelect() => Apply(_machine.CancelCharacterSelect);

        /// <summary>
        /// Pause menu "Menu principal": abandons the current encounter (if any), switches to
        /// <see cref="GameFlowState.MainMenu"/> and loads the world map, whose GameFrontEndController
        /// shows the title screen for that state. Saving first is the caller's choice. State is not
        /// reset here — "Novo jogo" and every save load reset it (see <see cref="NewSessionReset"/>),
        /// and those are the only ways out of the title screen. False when already on the title.
        /// </summary>
        public bool ReturnToTitle()
        {
            if (State == GameFlowState.MainMenu)
            {
                return false;
            }

            _transitions?.AbandonPendingEncounter();

            if (!Apply(_machine.ReturnToTitle))
            {
                return false;
            }

            _transitions?.ReturnToWorldMap();
            return true;
        }

        private int FindContinueSlot(int[] slots) => FindNewestSaveSlot(_saveManager, slots);

        /// <summary>
        /// The most recently saved readable slot among <paramref name="slots"/> (default
        /// <see cref="DefaultContinueSlots"/>), by each file's <c>lastSavedAtIso8601</c>; a slot whose
        /// file (and backup) can't be read is skipped. <see cref="GameFlowMachine.NoSlot"/> when none.
        /// Shared by "Continuar" and the title screen's button states (MainMenuPresenter).
        /// </summary>
        public static int FindNewestSaveSlot(SaveManager saveManager, IEnumerable<int> slots = null)
        {
            if (saveManager == null)
            {
                return GameFlowMachine.NoSlot;
            }

            var candidates = new List<(int, DateTime)>();

            foreach (int slot in slots ?? DefaultContinueSlots)
            {
                if (!saveManager.SlotExists(slot))
                {
                    continue;
                }

                SaveData data = saveManager.PeekSlot(slot);

                if (data == null)
                {
                    continue;
                }

                DateTime.TryParse(data.lastSavedAtIso8601, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime savedAt);
                candidates.Add((slot, savedAt));
            }

            return GameFlowMachine.PickNewestSlot(candidates);
        }

        private static int[] BuildSlotList(bool includeAutoSave)
        {
            var slots = new List<int>(SaveManager.ManualSlotCount + 1);

            if (includeAutoSave)
            {
                slots.Add(SaveManager.AutoSaveSlot);
            }

            for (int i = 0; i < SaveManager.ManualSlotCount; i++)
            {
                slots.Add(i);
            }

            return slots.ToArray();
        }

        private void OnEncounterStarted(EncounterStartedEvent evt) => Apply(_machine.EncounterStarted);

        private void OnEncounterEnded(EncounterEndedEvent evt) => Apply(_machine.EncounterEnded);

        private void OnCharacterSelected(CharacterSelectedEvent evt) => Apply(_machine.HeroChosen);

        // A restore during Continue (MainMenu) is resolved by Continue itself; one while the picker is
        // open (its own "Continuar com <herói>" button, also in Main.unity opened directly = Boot)
        // counts as choosing that hero.
        private void OnCharacterRestored(CharacterSelectionRestoredEvent evt)
        {
            if (State == GameFlowState.CharacterSelect || State == GameFlowState.Boot)
            {
                Apply(_machine.HeroChosen);
            }
        }

        private bool Apply(Func<bool> transition)
        {
            GameFlowState previous = _machine.State;

            if (!transition())
            {
                return false;
            }

            GameLogger.Info(LogCategory, $"{previous} -> {_machine.State}");
            EventBus.Publish(new GameFlowStateChangedEvent(previous, _machine.State));
            return true;
        }
    }
}
