using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.Tutorial;
using Rapadura.Gameplay.World;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.Common;
using Rapadura.UI.Menus;
using Rapadura.UI.Tutorial;
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
    /// During a world-map encounter (<see cref="SceneTransitionService.HasPendingEncounter"/>) this
    /// screen owns the defeat: it claims <see cref="SceneTransitionService.DeferDefeatResolution"/> so
    /// the arena doesn't run its own timed return underneath, and "Continuar" (or the timeout) revives
    /// the player and ends the encounter as <see cref="EncounterOutcome.Defeat"/> — back to the world
    /// map — instead of a checkpoint respawn inside the arena. A load from here also leaves the arena.
    /// Pausing goes through <see cref="GameplayPause"/> like every other modal screen.
    ///
    /// If this component is absent or disabled, CheckpointManager keeps its original instant respawn.
    ///
    /// Death drop (9%, <see cref="DeathDropRules"/>): each death shown here rolls once; on success one eligible backpack
    /// item goes to Vó Zefa's "Achados e Perdidos" (<see cref="DeathDropService"/>, saved with the village chest) and the
    /// panel says "Na pressa, você deixou cair: …" ("death-drop"). No drop without this screen (instant respawn).
    ///
    /// The first-death tutorial tip (<see cref="TutorialSteps.Death"/>) is shown inside this panel
    /// ("death-tip") rather than as the floating hint bubble, which steps aside while a modal holds a
    /// pause; closing the screen counts as having read it.
    ///
    /// Text goes through <see cref="UiTexts"/> (<c>ui.death.*</c>; static UXML via
    /// <see cref="UiDocumentLocalizer"/>). The serialized title/message/encounter strings are only used
    /// when changed from their pt-BR defaults in the Inspector; otherwise the localized key wins.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class DeathScreenController : MonoBehaviour
    {
        private const string PlayerTag = "Player";
        private const string DefaultTitle = "Você caiu...";
        private const string DefaultMessage = "O sertão não perdoa, mas a jornada continua.";
        private const string DefaultEncounterContinueText = "Voltar ao mapa";

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
        [Tooltip("Custom title. Left at the default, the localized ui.death.title is shown instead.")]
        [SerializeField] private string _title = DefaultTitle;
        [Tooltip("Custom message. Left at the default, the localized ui.death.message is shown instead.")]
        [SerializeField] private string _message = DefaultMessage;
        [Tooltip("'Continuar' label while dying in a world-map encounter (it returns to the map).")]
        [SerializeField] private string _encounterContinueText = DefaultEncounterContinueText;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private Label _titleLabel;
        private Label _messageLabel;
        private Label _countdownLabel;
        private Button _continueButton;
        private Button _loadButton;
        private VisualElement _tipBox;
        private Label _tipTitleLabel;
        private Label _tipBodyLabel;
        private bool _tipShown;
        private Label _dropLabel;
        private DeathDropResult _lastDrop;
        /// <summary>"−N XP" + protection line (built in code under the drop label; see <see cref="DeathPenaltyService"/>).</summary>
        private Label _penaltyLabel;
        private DeathPenaltyResult _lastPenalty;
        private bool _penaltyApplied;
        private static readonly System.Random DropRandom = new System.Random();

        private DeathScreenFlow _flow;
        private CheckpointManager _checkpointManager;
        private SceneTransitionService _transition;
        private readonly UiDocumentLocalizer _localizer = new UiDocumentLocalizer();
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
            _localizer.Attach(_uiDocument.rootVisualElement, OnLanguageChanged);
            ClaimCheckpointManager();
            ClaimEncounterDefeat();
            ApplyVisibility(false);
        }

        private void Start()
        {
            // Same Awake/OnEnable ordering caveat as ShopUIController.OnShopOpened: GameManager may
            // not have registered CheckpointManager yet when OnEnable ran. Every scene object's
            // Awake has run by the time any Start runs, so retrying here is enough.
            ClaimCheckpointManager();
            ClaimEncounterDefeat();
            _localizer.Refresh();
        }

        private void OnDisable()
        {
            _localizer.Detach();
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

            if (_transition != null)
            {
                _transition.DeferDefeatResolution = false;
                _transition = null;
            }

            GameplayPause.Release(this);
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
            _tipBox = rootVisualElement.Q<VisualElement>("death-tip");
            _tipTitleLabel = rootVisualElement.Q<Label>("death-tip-title");
            _tipBodyLabel = rootVisualElement.Q<Label>("death-tip-body");
            _dropLabel = rootVisualElement.Q<Label>("death-drop");
            _penaltyLabel = EnsurePenaltyLabel(rootVisualElement, _dropLabel);
        }

        /// <summary>The "death-penalty" label, created next to "death-drop" (same look) when the UXML has none.</summary>
        private static Label EnsurePenaltyLabel(VisualElement root, Label dropLabel)
        {
            Label label = root.Q<Label>("death-penalty");
            if (label != null || dropLabel == null || dropLabel.parent == null)
            {
                return label;
            }

            label = new Label { name = "death-penalty" };
            label.AddToClassList("death-drop");
            label.style.display = DisplayStyle.None;
            dropLabel.parent.Insert(dropLabel.parent.IndexOf(dropLabel), label);
            return label;
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

        private void ClaimEncounterDefeat()
        {
            if (_transition != null)
            {
                return;
            }

            if (ServiceLocator.TryGet(out _transition))
            {
                _transition.DeferDefeatResolution = true;
            }
        }

        private bool IsInEncounter => _transition != null && _transition.HasPendingEncounter;

        private void OnPlayerDied(PlayerDiedEvent gameEvent)
        {
            ClaimCheckpointManager();
            ClaimEncounterDefeat();

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

            _localizer.Refresh();
            _tipShown = ShouldShowDeathTip();
            _penaltyApplied = TryApplyDeathPenalty(out _lastPenalty);
            _lastDrop = _lastPenalty.Drop;
            ApplyTexts();
            if (_loadButton != null) _loadButton.style.display = _saveLoadMenu != null ? DisplayStyle.Flex : DisplayStyle.None;

            PauseGameplay();
            ApplyVisibility(true);
            RefreshWidgets();
        }

        /// <summary>Title, message and the "Continuar"/"Voltar ao mapa" label for the current language and context.</summary>
        private void ApplyTexts()
        {
            LocalizationManager localization = _localizer.Manager;
            if (_titleLabel != null) _titleLabel.text = Localized(localization, _title, DefaultTitle, UiTexts.DeathTitle);
            if (_messageLabel != null) _messageLabel.text = Localized(localization, _message, DefaultMessage, UiTexts.DeathMessage);
            if (_continueButton != null)
            {
                _continueButton.text = IsInEncounter
                    ? Localized(localization, _encounterContinueText, DefaultEncounterContinueText, UiTexts.DeathReturnToMap)
                    : UiTexts.Get(localization, UiTexts.DeathContinue);
            }

            ApplyDeathTip(localization);
            ApplyDeathDrop(localization);
            ApplyDeathPenaltyText(localization);
            _lastCountdownSecond = int.MinValue;
        }

        /// <summary>
        /// The death penalty on the player (<see cref="DeathPenaltyService"/>): a Patuá or the Bênção da Romaria is used up,
        /// the XP loss is removed (never de-levels) and, unprotected, the 9% roll may move one item to the village chest's
        /// "Achados e Perdidos". No loss below level 3 nor in a first-campaign boss challenge.
        /// </summary>
        private bool TryApplyDeathPenalty(out DeathPenaltyResult result)
        {
            result = default;
            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            if (player == null)
            {
                return false;
            }

            InventoryManager bag = player.GetComponent<InventoryManager>();
            PlayerStats stats = player.GetComponent<PlayerStats>();
            bool boss = _transition != null && _transition.PendingEncounter != null && _transition.PendingEncounter.IsBossChallenge;
            var context = stats != null
                ? new DeathPenaltyContext(stats.Level, stats.CurrentExperience, stats.GetExperienceToNextLevel(), boss, NewGamePlusProgress.ResolveCurrentCycle())
                : new DeathPenaltyContext(1, 0, 0, boss);

            result = DeathPenaltyService.Apply(context, bag, VillageChestStorage.ResolveShared().LostAndFound,
                DeathProtectionState.ResolveShared(), DropRandom);
            int removed = stats != null ? stats.RemoveExperience(result.ExperienceLost) : 0;
            result = new DeathPenaltyResult(removed, result.Protection, result.Drop);
            return true;
        }

        private void ApplyDeathPenaltyText(LocalizationManager localization)
        {
            if (_penaltyLabel == null)
            {
                return;
            }

            _penaltyLabel.style.display = _penaltyApplied ? DisplayStyle.Flex : DisplayStyle.None;
            if (!_penaltyApplied)
            {
                return;
            }

            string text = _lastPenalty.ExperienceLost > 0
                ? DeathProtectionTexts.Format(localization, DeathProtectionTexts.DeathXpLost, _lastPenalty.ExperienceLost)
                : DeathProtectionTexts.Get(localization, DeathProtectionTexts.DeathNoXpLost);
            if (_lastPenalty.Protection == DeathProtection.Patua)
            {
                text += "\n" + DeathProtectionTexts.Get(localization, DeathProtectionTexts.DeathProtectedPatua);
            }
            else if (_lastPenalty.Protection == DeathProtection.Blessing)
            {
                text += "\n" + DeathProtectionTexts.Get(localization, DeathProtectionTexts.DeathProtectedBlessing);
            }

            _penaltyLabel.text = text;
        }

        private void ApplyDeathDrop(LocalizationManager localization)
        {
            if (_dropLabel == null)
            {
                return;
            }

            _dropLabel.style.display = _lastDrop.Dropped ? DisplayStyle.Flex : DisplayStyle.None;
            if (!_lastDrop.Dropped)
            {
                return;
            }

            _dropLabel.text = UiTexts.Format(localization, UiTexts.DeathDropped, DescribeDrop(_lastDrop.Item))
                + "\n" + UiTexts.Get(localization, UiTexts.DeathDroppedHint);
        }

        /// <summary>"Espada de Ferro" / "3 Poção de Vida": the shown name (generated name for a rolled item), quantity above one.</summary>
        public static string DescribeDrop(InventorySlotData item)
        {
            if (item == null || item.IsEmpty)
            {
                return string.Empty;
            }

            ItemDefinition definition = ItemDatabase.GetById(item.itemId);
            string name = definition != null ? GeneratedItemName.Compose(definition, item, ContentText.CurrentManager) : item.itemId;
            return item.quantity > 1 ? item.quantity + " " + name : name;
        }

        /// <summary>
        /// Queues the first-death tip (idempotent: the manager's own PlayerDiedEvent handler may not
        /// have run yet) and reports whether it is still unread — false when already done or hints are off.
        /// </summary>
        private static bool ShouldShowDeathTip()
        {
            if (!ServiceLocator.TryGet(out TutorialManager tutorial) || !tutorial.HintsEnabled)
            {
                return false;
            }

            tutorial.Trigger(TutorialSteps.Death);
            return tutorial.IsPending(TutorialSteps.Death);
        }

        private void ApplyDeathTip(LocalizationManager localization)
        {
            if (_tipBox == null)
            {
                return;
            }

            _tipBox.style.display = _tipShown ? DisplayStyle.Flex : DisplayStyle.None;
            if (!_tipShown)
            {
                return;
            }

            if (_tipTitleLabel != null) _tipTitleLabel.text = TutorialTexts.Get(localization, TutorialSteps.TitleKey(TutorialSteps.Death));
            if (_tipBodyLabel != null) _tipBodyLabel.text = TutorialTexts.Get(localization, TutorialSteps.BodyKey(TutorialSteps.Death));
        }

        /// <summary>The screen closed with the tip on it: it was read (marks the step done, saves the profile).</summary>
        private void AcknowledgeDeathTip()
        {
            if (!_tipShown)
            {
                return;
            }

            _tipShown = false;
            if (_tipBox != null)
            {
                _tipBox.style.display = DisplayStyle.None;
            }

            if (ServiceLocator.TryGet(out TutorialManager tutorial))
            {
                tutorial.Acknowledge(TutorialSteps.Death);
            }
        }

        /// <summary>The Inspector override when it was changed from the pt-BR default, else the key's text.</summary>
        private static string Localized(LocalizationManager localization, string serialized, string defaultText, string key)
        {
            return string.IsNullOrEmpty(serialized) || serialized == defaultText ? UiTexts.Get(localization, key) : serialized;
        }

        private void OnLanguageChanged()
        {
            if (_flow == null || !_flow.IsActive)
            {
                return;
            }

            ApplyTexts();
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
            if (_controlsTimeScale)
            {
                GameplayPause.Enforce();
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
                    _countdownLabel.text = second >= 0 ? UiTexts.Format(_localizer.Manager, UiTexts.DeathCountdown, second) : string.Empty;
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
            AcknowledgeDeathTip();
            ResumeGameplay();

            // PlayerStats.RestoreState sets IsDead from the saved health; a save taken at 0 HP would
            // otherwise leave the player dead with no screen up — fall back to a checkpoint respawn.
            PlayerStats stats = FindPlayerStats();

            if (IsInEncounter)
            {
                // The arena stopped resolving on its own (defeat was deferred to this screen) — a load
                // from here leaves the arena the same way "Continuar" does.
                ReturnToMapAsDefeat(stats);
                return;
            }

            if (stats != null && stats.IsDead)
            {
                RespawnViaCheckpoint(stats);
            }
        }

        private void FinishWithRespawn()
        {
            ApplyVisibility(false);
            AcknowledgeDeathTip();
            ResumeGameplay();

            if (IsInEncounter)
            {
                ReturnToMapAsDefeat(FindPlayerStats());
                return;
            }

            RespawnViaCheckpoint(FindPlayerStats());
        }

        /// <summary>
        /// Encounter defeat: revive first (CompleteEncounter auto-saves, and the player state carried
        /// into the next encounter must not be a corpse), then hand the result back to the world map.
        /// </summary>
        private void ReturnToMapAsDefeat(PlayerStats stats)
        {
            if (stats != null && stats.IsDead)
            {
                stats.Revive();
            }

            // Keep the arena frozen while the world map loads (async): a revived player must not
            // be hit — and die again — by enemies still alive in it. Released on scene unload (OnDisable).
            GameplayPause.Acquire(this);
            _transition.CompleteEncounter(EncounterOutcome.Defeat);
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

        /// <summary>Freezes gameplay through <see cref="GameplayPause"/> (which publishes <see cref="GamePausedEvent"/>) and blocks player input.</summary>
        private void PauseGameplay()
        {
            if (_controlsTimeScale)
            {
                GameplayPause.Acquire(this);
            }
            else
            {
                EventBus.Publish(new GamePausedEvent());
            }

            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            _pausedPlayerInput = player != null ? player.GetComponent<PlayerInput>() : null;
            _pausedPlayerInput?.DeactivateInput();
        }

        private void ResumeGameplay()
        {
            if (_pausedPlayerInput != null)
            {
                _pausedPlayerInput.ActivateInput();
                _pausedPlayerInput = null;
            }

            if (_controlsTimeScale)
            {
                GameplayPause.Release(this);
            }
            else
            {
                EventBus.Publish(new GameResumedEvent());
            }
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
