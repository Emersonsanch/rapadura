using UnityEngine;

namespace Rapadura.UI.Death
{
    /// <summary>States of the Death screen flow driven by <see cref="DeathScreenFlow"/>.</summary>
    public enum DeathScreenState
    {
        /// <summary>Screen not shown; gameplay running normally.</summary>
        Hidden,
        /// <summary>Player just died; overlay is fading in and buttons are not yet accepted.</summary>
        FadingIn,
        /// <summary>Fade finished; waiting for "Continuar"/"Carregar jogo" (or the auto-continue timeout).</summary>
        AwaitingChoice,
        /// <summary>The Save/Load screen was opened from the Death screen; auto-continue is paused.</summary>
        LoadMenuOpen
    }

    /// <summary>
    /// Pure (no Unity lifecycle) state machine behind the Death screen, so the "when does the
    /// respawn actually happen" rules are unit-testable without a UIDocument — same split as
    /// <c>SaveSlotPresenter</c> vs <c>SaveLoadMenuController</c>.
    ///
    /// Rules: <see cref="Begin"/> starts a fade of <see cref="FadeDuration"/> seconds during which
    /// input is ignored (touch-first: avoids a stray tap from the last fight skipping the screen).
    /// Once in <see cref="DeathScreenState.AwaitingChoice"/>, <see cref="TryContinue"/> or the
    /// optional auto-continue timeout (<see cref="AutoContinueSeconds"/> &gt; 0) ends the flow and
    /// tells the caller to respawn. The timeout is paused while the Load menu is open.
    /// </summary>
    public class DeathScreenFlow
    {
        private float _fadeElapsed;
        private float _choiceElapsed;

        public DeathScreenFlow(float fadeDuration, float autoContinueSeconds)
        {
            FadeDuration = Mathf.Max(0f, fadeDuration);
            AutoContinueSeconds = Mathf.Max(0f, autoContinueSeconds);
        }

        public float FadeDuration { get; }

        /// <summary>Seconds after the fade before "Continuar" fires on its own. 0 disables the timeout.</summary>
        public float AutoContinueSeconds { get; }

        public DeathScreenState State { get; private set; } = DeathScreenState.Hidden;

        public bool IsActive => State != DeathScreenState.Hidden;

        public bool CanChoose => State == DeathScreenState.AwaitingChoice;

        /// <summary>0..1 overlay opacity (1 once the fade completes, 0 while hidden).</summary>
        public float FadeAlpha
        {
            get
            {
                if (State == DeathScreenState.Hidden)
                {
                    return 0f;
                }

                if (State != DeathScreenState.FadingIn || FadeDuration <= 0f)
                {
                    return 1f;
                }

                return Mathf.Clamp01(_fadeElapsed / FadeDuration);
            }
        }

        /// <summary>Seconds left before auto-continue, or -1 when the timeout is disabled/not running.</summary>
        public float AutoContinueRemaining
        {
            get
            {
                if (AutoContinueSeconds <= 0f || State != DeathScreenState.AwaitingChoice)
                {
                    return -1f;
                }

                return Mathf.Max(0f, AutoContinueSeconds - _choiceElapsed);
            }
        }

        /// <summary>Starts the flow. Returns false (no-op) if it is already active, e.g. a duplicate death event.</summary>
        public bool Begin()
        {
            if (IsActive)
            {
                return false;
            }

            _fadeElapsed = 0f;
            _choiceElapsed = 0f;
            State = FadeDuration > 0f ? DeathScreenState.FadingIn : DeathScreenState.AwaitingChoice;
            return true;
        }

        /// <summary>
        /// Advances timers by <paramref name="deltaTime"/> (pass unscaled time — gameplay is paused
        /// while the screen is shown). Returns true exactly when the auto-continue timeout fires,
        /// at which point the flow is already <see cref="DeathScreenState.Hidden"/> and the caller
        /// must respawn the player.
        /// </summary>
        public bool Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return false;
            }

            if (State == DeathScreenState.FadingIn)
            {
                _fadeElapsed += deltaTime;

                if (_fadeElapsed >= FadeDuration)
                {
                    State = DeathScreenState.AwaitingChoice;
                }

                return false;
            }

            if (State == DeathScreenState.AwaitingChoice && AutoContinueSeconds > 0f)
            {
                _choiceElapsed += deltaTime;

                if (_choiceElapsed >= AutoContinueSeconds)
                {
                    State = DeathScreenState.Hidden;
                    return true;
                }
            }

            return false;
        }

        /// <summary>"Continuar" pressed. Returns true (and hides) only once the fade has finished.</summary>
        public bool TryContinue()
        {
            if (State != DeathScreenState.AwaitingChoice)
            {
                return false;
            }

            State = DeathScreenState.Hidden;
            return true;
        }

        /// <summary>"Carregar jogo" pressed. Returns true if the Load menu should open.</summary>
        public bool TryOpenLoadMenu()
        {
            if (State != DeathScreenState.AwaitingChoice)
            {
                return false;
            }

            State = DeathScreenState.LoadMenuOpen;
            return true;
        }

        /// <summary>The Load menu was closed without loading; back to waiting (timeout restarts).</summary>
        public void CloseLoadMenu()
        {
            if (State != DeathScreenState.LoadMenuOpen)
            {
                return;
            }

            _choiceElapsed = 0f;
            State = DeathScreenState.AwaitingChoice;
        }

        /// <summary>A save was loaded while the screen was up. Returns true if the flow was active and is now finished.</summary>
        public bool CompleteViaLoad()
        {
            if (!IsActive)
            {
                return false;
            }

            State = DeathScreenState.Hidden;
            return true;
        }
    }
}
