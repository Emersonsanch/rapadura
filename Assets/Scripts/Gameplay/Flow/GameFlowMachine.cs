using System;
using System.Collections.Generic;

namespace Rapadura.Gameplay.Flow
{
    /// <summary>Top-level phase of a play session (see <see cref="GameFlowMachine"/>).</summary>
    public enum GameFlowState
    {
        /// <summary>App just started; nothing decided yet.</summary>
        Boot = 0,
        /// <summary>Title screen (MainMenuController in WorldMap.unity).</summary>
        MainMenu = 1,
        /// <summary>Picking a hero for a new game (CharacterSelectController in WorldMap.unity).</summary>
        CharacterSelect = 2,
        /// <summary>Walking the world map with a hero.</summary>
        World = 3,
        /// <summary>Fighting an encounter in Main.unity.</summary>
        Encounter = 4,
    }

    /// <summary>What Main.unity's character select screen must do when it is enabled (see <see cref="GameFlowMachine.DecideCombatSceneSelect"/>).</summary>
    public enum CharacterSelectDecision
    {
        /// <summary>A hero is already chosen: re-apply it to this scene's player and close without showing.</summary>
        ReapplyAndSkip = 0,
        /// <summary>No hero and no encounter (Main.unity opened directly in the Editor): show the picker, gameplay paused.</summary>
        Show = 1,
        /// <summary>No hero but an encounter is pending — impossible through the normal flow; never show, bail out to the map.</summary>
        AbortEncounter = 2,
    }

    /// <summary>
    /// Pure state machine for the session flow:
    /// <c>Boot → MainMenu → (Novo jogo) CharacterSelect → World ⇄ Encounter</c>, or
    /// <c>MainMenu → (Continuar) World</c> when the loaded save has a hero (CharacterSelect when it
    /// doesn't, e.g. a pre-v2 save). Each transition method returns whether the state changed;
    /// invalid requests are ignored. Driven by <see cref="GameFlowService"/>, kept Unity-free for
    /// EditMode tests.
    /// </summary>
    public class GameFlowMachine
    {
        public const int NoSlot = int.MinValue;

        public GameFlowState State { get; private set; } = GameFlowState.Boot;

        /// <summary>True while the hero picker was opened by "Novo jogo" (cleared on any other entry).</summary>
        public bool IsNewGameSelection { get; private set; }

        /// <summary>
        /// Whether the hero picker may offer its "Continuar com &lt;herói&gt;" shortcut: not when the
        /// player just chose "Novo jogo" (that would silently load the old save they meant to replace).
        /// </summary>
        public bool OffersSavedContinueInSelect => !(State == GameFlowState.CharacterSelect && IsNewGameSelection);

        /// <summary>
        /// The world map's front-end (re)entered. From Boot this opens the title screen; on the map
        /// without a hero (e.g. a bailed-out encounter) it forces the hero picker. Otherwise unchanged.
        /// </summary>
        public bool EnterFrontEnd(bool hasHero)
        {
            if (State == GameFlowState.Boot)
            {
                return Set(GameFlowState.MainMenu);
            }

            if ((State == GameFlowState.World || State == GameFlowState.Encounter) && !hasHero)
            {
                return Set(GameFlowState.CharacterSelect);
            }

            if (State == GameFlowState.Encounter)
            {
                // Back on the map but the encounter-ended event was missed (scene reloaded by hand).
                return Set(GameFlowState.World);
            }

            return false;
        }

        /// <summary>"Novo jogo" on the title screen.</summary>
        public bool StartNewGame()
        {
            if (State != GameFlowState.MainMenu || !Set(GameFlowState.CharacterSelect))
            {
                return false;
            }

            IsNewGameSelection = true;
            return true;
        }

        /// <summary>"Continuar" loaded a save: straight to the map if it had a hero, else pick one.</summary>
        public bool CompleteContinue(bool hasHero)
        {
            if (State != GameFlowState.MainMenu)
            {
                return false;
            }

            return Set(hasHero ? GameFlowState.World : GameFlowState.CharacterSelect);
        }

        /// <summary>
        /// A hero was confirmed (or restored by the select screen's own "Continuar com..." button).
        /// From Boot this is the Editor shortcut of opening Main.unity directly and picking there.
        /// </summary>
        public bool HeroChosen()
        {
            return (State == GameFlowState.CharacterSelect || State == GameFlowState.Boot) && Set(GameFlowState.World);
        }

        /// <summary>Back out of the hero picker to the title screen.</summary>
        public bool CancelCharacterSelect()
        {
            return State == GameFlowState.CharacterSelect && Set(GameFlowState.MainMenu);
        }

        public bool EncounterStarted()
        {
            return State != GameFlowState.Encounter && Set(GameFlowState.Encounter);
        }

        public bool EncounterEnded()
        {
            return State == GameFlowState.Encounter && Set(GameFlowState.World);
        }

        /// <summary>Back to the title screen from anywhere (e.g. pause menu "Menu principal").</summary>
        public bool ReturnToTitle()
        {
            return Set(GameFlowState.MainMenu);
        }

        /// <summary>Whether the world map's own controls (token movement, random encounters) may run.</summary>
        public static bool IsWorldInteractive(GameFlowState state)
        {
            return state == GameFlowState.World;
        }

        /// <summary>The rule behind Main.unity's character select screen: never show it during an encounter.</summary>
        public static CharacterSelectDecision DecideCombatSceneSelect(bool hasHero, bool encounterPending)
        {
            if (hasHero)
            {
                return CharacterSelectDecision.ReapplyAndSkip;
            }

            return encounterPending ? CharacterSelectDecision.AbortEncounter : CharacterSelectDecision.Show;
        }

        /// <summary>Most recently saved slot among <paramref name="candidates"/> ("Continuar"), or <see cref="NoSlot"/>.</summary>
        public static int PickNewestSlot(IEnumerable<(int slot, DateTime savedAtUtc)> candidates)
        {
            int best = NoSlot;
            DateTime newest = DateTime.MinValue;

            if (candidates == null)
            {
                return best;
            }

            foreach ((int slot, DateTime savedAtUtc) in candidates)
            {
                if (best == NoSlot || savedAtUtc > newest)
                {
                    best = slot;
                    newest = savedAtUtc;
                }
            }

            return best;
        }

        private bool Set(GameFlowState next)
        {
            if (State == next)
            {
                return false;
            }

            State = next;
            IsNewGameSelection = false;
            return true;
        }
    }
}
