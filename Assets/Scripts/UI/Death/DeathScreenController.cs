using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World;
using Rapadura.UI.Menus;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Rapadura.UI.Death
{
    /// <summary>
    /// Drives the Death screen (DeathScreenView.uxml/.uss), following the same
    /// single-<see cref="UIDocument"/>/<c>Q&lt;T&gt;()</c>-once pattern as <c>PauseMenuController</c>.
    ///
    /// While enabled it sets <see cref="CheckpointManager.DeferRespawn"/>, so a
    /// <see cref="PlayerDiedEvent"/> no longer teleports/revives the player instantly; instead this
    /// screen fades in, pauses gameplay (Time.timeScale = 0 + <see cref="PlayerInput.DeactivateInput"/>
    /// + <see cref="GamePausedEvent"/>) and only calls <see cref="CheckpointManager.RespawnPlayer"/>
    /// once the player presses "Continuar" or the auto-continue timeout elapses. "Carregar jogo"
    /// opens the shared <see cref="SaveLoadMenuController"/> in Load mode; a successful load
    /// (<see cref="GameLoadedEvent"/>) ends the flow without a checkpoint respawn. The timing rules
    /// live in the pure <see cref="DeathScreenFlow"/> (unit tested separately).
    ///
    /// If this component is absent or disabled, CheckpointManager keeps its original instant respawn.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class DeathScreenController : MonoBehaviour
    {
        private const string PlayerTag = "Player";

        [Header("Panels")]
        [Tooltip("Optional: enables the 'Carregar jogo' button. Hidden when not assigned.")]
        [SerializeField] private SaveLoadMenuController _saveLoadMenu;

        [Header("Timing (unscaled seconds)")]
        [SerializeField] private float _fadeDuration = 0.8f;
        [Tooltip("Seconds after the fade before 'Continuar' fires automatically. 0 = wait for the button forever.")]
        [SerializeField] private float _autoContinueSeconds = 15f;

        [Header("Behaviour")]
        [Tooltip("Whether showing this screen sets Time.timeScale to 0 and hiding it restores it to 1.")]
        [SerializeField] private bool _controlsTimeScale = true;

        [Header("Text")]
        [SerializeField] private string _title = "Você caiu...";
        [SerializeField] private string _message = "O sertão não perdoa, mas a jornada continua.";

        private UIDocument _uiDocument;
        private VisualElement _root;
        private Label _titleLabel;
        private Label _messageLabel;
        private Label _countdownLabel;
        private Button _continueButton;
        private Button _loadButton;

        private DeathScreenFlow _flow;
        private CheckpointManager _checkpointManager;
        private PlayerInput _pausedPlayerInput;

        /// <summary>Last whole second written to the countdown label (-1 = blank) — RefreshWidgets runs every
        /// frame while the screen is up, so the label string is only rebuilt when the shown second changes.</summary>
        private int _lastCountdownSecond = int.MinValue;

        public bool IsOpen => _flow != null && _flow.IsActive;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            _flow = new DeathScreenFlow(_fadeDuration, _autoContinueSeconds);
        }

        private void OnEnable()
        {
            BindElements();
            RegisterCallbacks();
            ClaimCheckpointManager();
            ApplyVisibility(false);
        }

        private void Start()
        {
            // Same Awake/OnEnable ordering caveat as ShopUIController.OnShopOpened: GameManager may
            // not have registered CheckpointManager yet when OnEnable ran. Every scene object's
            // Awake has run by the time any Start runs, so retrying here is enough.
            ClaimCheckpointManager();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();

            if (_flow != null && _flow.IsActive)
            {
                _flow.CompleteViaLoad();
                ResumeGameplay();
            }

            if (_checkpointManager != null)
            {
                _checkpointManager.DeferRespawn = false;
                _checkpointManager = null;
            }
        }

        private void BindElements()
        {
            VisualElement rootVisualElement = _uiDocument.rootVisualElement;

            _root = rootVisualElement.Q<VisualElement>("death-screen-root");
            _titleLabel = rootVisualElement.Q<Label>("death-title");
            _messageLabel = rootVisualElement.Q<Label>("death-message");
            _countdownLabel = rootVisualElement.Q<Label>("death-countdown");
            _continueButton = rootVisualElement.Q<Button>("death-continue-button");
            _loadButton = rootVisualElement.Q<Button>("death-load-button");
        }

        private void RegisterCallbacks()
        {
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);

            if (_continueButton != null) _continueButton.clicked += OnContinueClicked;
            if (_loadButton != null) _loadButton.clicked += OnLoadClicked;
        }

        private void UnregisterCallbacks()
        {
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);

            if (_continueButton != null) _continueButton.clicked -= OnContinueClicked;
            if (_loadButton != null) _loadButton.clicked -= OnLoadClicked;
        }

        private void ClaimCheckpointManager()
        {
            if (_checkpointManager != null)
            {
                return;
            }

            if (ServiceLocator.TryGet(out _checkpointManager))
            {
                _checkpointManager.DeferRespawn = true;
            }
        }

        private void OnPlayerDied(PlayerDiedEvent gameEvent)
        {
            ClaimCheckpointManager();

            // If CheckpointManager was only claimed just now, its own handler may already have run
            // the old instant respawn this frame — nothing to show in that case.
            PlayerStats stats = FindPlayerStats();
            if (stats != null && !stats.IsDead)
            {
                return;
            }

            if (!_flow.Begin())
            {
                return;
            }

            if (_titleLabel != null) _titleLabel.text = _title;
            if (_messageLabel != null) _messageLabel.text = _message;
            if (_loadButton != null) _loadButton.style.display = _saveLoadMenu != null ? DisplayStyle.Flex : DisplayStyle.None;

            PauseGameplay();
            ApplyVisibility(true);
            RefreshWidgets();
        }

        private void Update()
        {
            if (_flow == null || !_flow.IsActive)
            {
                return;
            }

            // HitStopController restores its own "previous" timeScale after the lethal hit's
            // freeze-frame, which would otherwise un-pause gameplay underneath this screen.
            if (_controlsTimeScale && Time.timeScale != 0f)
            {
                Time.timeScale = 0f;
            }

            if (_flow.State == DeathScreenState.LoadMenuOpen)
            {
                if (IsSaveLoadMenuVisible())
                {
                    return;
                }

                // Load menu closed via its own "X" without loading anything — back to this screen.
                _flow.CloseLoadMenu();
                ApplyVisibility(true);
            }

            bool timedOut = _flow.Tick(Time.unscaledDeltaTime);

            if (timedOut)
            {
                FinishWithRespawn();
                return;
            }

            RefreshWidgets();
        }

        private void RefreshWidgets()
        {
            if (_root != null)
            {
                _root.style.opacity = _flow.FadeAlpha;
            }

            bool canChoose = _flow.CanChoose;
            _continueButton?.SetEnabled(canChoose);
            _loadButton?.SetEnabled(canChoose);

            if (_countdownLabel != null)
            {
                float remaining = _flow.AutoContinueRemaining;
                int second = remaining >= 0f ? Mathf.CeilToInt(remaining) : -1;
                if (second != _lastCountdownSecond)
                {
                    _lastCountdownSecond = second;
                    _countdownLabel.text = second >= 0 ? $"Continuando em {second}s" : string.Empty;
                }
            }
        }

        private void OnContinueClicked()
        {
            if (_flow.TryContinue())
            {
                FinishWithRespawn();
            }
        }

        private void OnLoadClicked()
        {
            if (_saveLoadMenu == null || !_flow.TryOpenLoadMenu())
            {
                return;
            }

            // Hide (but keep the flow alive) so the Save/Load panel, which shares the same
            // PanelSettings/sort order, isn't covered by this overlay.
            ApplyVisibility(false);
            _saveLoadMenu.Show(SaveLoadMenuController.ScreenMode.Load);
        }

        private void OnGameLoaded(GameLoadedEvent gameEvent)
        {
            if (!_flow.CompleteViaLoad())
            {
                return;
            }

            _saveLoadMenu?.Hide();
            ApplyVisibility(false);
            ResumeGameplay();

            // PlayerStats.RestoreState sets IsDead from the saved health; a save taken at 0 HP would
            // otherwise leave the player dead with no screen up — fall back to a checkpoint respawn.
            PlayerStats stats = FindPlayerStats();
            if (stats != null && stats.IsDead)
            {
                RespawnViaCheckpoint(stats);
            }
        }

        private void FinishWithRespawn()
        {
            ApplyVisibility(false);
            ResumeGameplay();
            RespawnViaCheckpoint(FindPlayerStats());
        }

        private void RespawnViaCheckpoint(PlayerStats fallbackStats)
        {
            if (_checkpointManager != null)
            {
                _checkpointManager.RespawnPlayer();
            }
            else
            {
                // No CheckpointManager registered at all: never leave the run soft-locked.
                fallbackStats?.Revive();
            }
        }

        private void PauseGameplay()
        {
            if (_controlsTimeScale)
            {
                Time.timeScale = 0f;
            }

            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            _pausedPlayerInput = player != null ? player.GetComponent<PlayerInput>() : null;
            _pausedPlayerInput?.DeactivateInput();

            EventBus.Publish(new GamePausedEvent());
        }

        private void ResumeGameplay()
        {
            if (_controlsTimeScale)
            {
                Time.timeScale = 1f;
            }

            if (_pausedPlayerInput != null)
            {
                _pausedPlayerInput.ActivateInput();
                _pausedPlayerInput = null;
            }

            EventBus.Publish(new GameResumedEvent());
        }

        private bool IsSaveLoadMenuVisible()
        {
            if (_saveLoadMenu == null)
            {
                return false;
            }

            UIDocument document = _saveLoadMenu.GetComponent<UIDocument>();
            VisualElement saveLoadRoot = document != null ? document.rootVisualElement?.Q<VisualElement>("save-load-menu-root") : null;
            return saveLoadRoot != null && saveLoadRoot.style.display.value == DisplayStyle.Flex;
        }

        private void ApplyVisibility(bool visible)
        {
            if (_root == null)
            {
                return;
            }

            _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

            if (!visible)
            {
                _root.style.opacity = 0f;
            }
            else
            {
                _root.style.opacity = _flow.FadeAlpha;
            }
        }

        private static PlayerStats FindPlayerStats()
        {
            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            return player != null ? player.GetComponent<PlayerStats>() : null;
        }
    }
}
