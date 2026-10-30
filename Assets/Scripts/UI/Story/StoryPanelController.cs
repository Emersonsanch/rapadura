using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using Rapadura.UI.Common;
using Rapadura.UI.Menus;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.Story
{
    /// <summary>
    /// The one story-panel presenter of the campaign (Resources/UI/StoryPanelView.uxml/.uss): plays the
    /// beat <see cref="StoryProgress.GetPendingBeat"/> reports — the prologue on a new game's first
    /// world map, an interlude after each region boss, the ending after the Cavernas boss — as a
    /// sequence of short text panels (<see cref="StoryPanelSequence"/>, texts from <see cref="StoryTexts"/>,
    /// one panel focused on the hero the player chose). Tap/click anywhere, "Continuar", Space/Enter or
    /// the gamepad's south button go to the next panel; "Pular" ends the beat (it still counts as seen).
    /// Once a beat ends it is marked seen and the game auto-saves; the ending then opens the scene's
    /// <see cref="CreditsController"/>, and closing the credits leaves the player on the world map (post-game).
    ///
    /// A beat only starts on the world map (<see cref="StoryPresentationRules.CanPresent"/>): flow in
    /// World with a hero, <see cref="WorldMapController"/> running and idle, no other screen holding a
    /// <see cref="GameplayPause"/>, and after a short real-time delay so the map is seen first. While it is
    /// up it holds its own GameplayPause (no map clicks/encounters behind it; the tutorial bubble waits),
    /// released one frame after the last panel so the closing tap cannot also reach the map.
    ///
    /// Self-bootstrapping like <c>TutorialHintController</c>: one DontDestroyOnLoad host created after
    /// the first scene loads (no scene wiring), borrowing the PanelSettings of another UIDocument in the
    /// scene. Fonts/contrast follow <see cref="UiAccessibilityApplier"/>; with "Reduzir Movimento" (or "Reduzir Flashes") the text
    /// swaps instantly instead of fading in. <see cref="AutoPresent"/> lets PlayMode smoke tests keep the
    /// map free of story overlays.
    /// </summary>
    public class StoryPanelController : MonoBehaviour
    {
        /// <summary>Above the map HUD (5), windows (10-30) and tutorial bubble (40); below the death screen (50) and toasts (60).</summary>
        public const float SortingOrder = 45f;

        private const string ViewResourcePath = "UI/StoryPanelView";
        private const string EnteringClass = "story-body--entering";
        private const string NoMotionClass = "story-body--no-motion";

        /// <summary>Real seconds between two accepted "next" inputs (a double tap must not skip a panel).</summary>
        private const float AdvanceCooldownSeconds = 0.25f;

        /// <summary>When false, pending beats are never started (PlayMode smoke tests). Default true.</summary>
        public static bool AutoPresent = true;

        [Tooltip("Real seconds the world map must be ready before a pending beat starts.")]
        [SerializeField, Min(0f)] private float _startDelaySeconds = 0.6f;

        private static StoryPanelController _instance;

        private readonly UiDocumentLocalizer _localizer = new UiDocumentLocalizer();
        private UIDocument _uiDocument;
        private VisualTreeAsset _viewAsset;
        private VisualElement _root;
        private VisualElement _card;
        private Label _titleLabel;
        private Label _bodyLabel;
        private Label _progressLabel;
        private Button _skipButton;
        private Button _nextButton;

        private LocalizationManager _localization;
        private WorldMapController _map;
        private bool _mapLookedUp;
        private StoryPanelSequence _sequence;
        private CreditsController _credits;
        private float _readyTime = -1f;
        private float _lastAdvanceAt = -10f;
        private int _releasePauseAtFrame = -1;

        // Cutscenes already started this session (a quit mid-panels must not replay them in a loop), and whether one
        // of ours is on screen right now (intro before the prologue panels, outro before the credits).
        private readonly System.Collections.Generic.HashSet<CutsceneId> _cutscenesPlayed = new System.Collections.Generic.HashSet<CutsceneId>();
        private bool _waitingForCutscene;

        /// <summary>The beat on screen right now (None when idle).</summary>
        public static StoryBeat ShowingBeat => _instance != null && _instance._sequence != null ? _instance._sequence.Beat : StoryBeat.None;

        /// <summary>True while a story beat, its cutscene (<see cref="CutscenePlayerController"/>) or the credits it opened is on screen.</summary>
        public static bool IsShowing => _instance != null && (_instance._sequence != null || _instance._credits != null || _instance._waitingForCutscene);

        /// <summary>Panel index / count of the beat on screen (-1 / 0 when idle), e.g. for the PlayMode campaign test.</summary>
        public static int ShowingPanelIndex => _instance != null && _instance._sequence != null ? _instance._sequence.Index : -1;

        public static int ShowingPanelCount => _instance != null && _instance._sequence != null ? _instance._sequence.Count : 0;

        /// <summary>
        /// Same as tapping / "Continuar" / Space on the beat on screen (the double-tap cooldown applies,
        /// so call it at most every <see cref="AdvanceCooldownSeconds"/> of real time). False when no beat is up.
        /// </summary>
        public static bool AdvanceShowing()
        {
            if (_instance == null || _instance._sequence == null)
            {
                return false;
            }

            _instance.Advance();
            return true;
        }

        /// <summary>Same as "Pular" / cancel on the beat on screen: ends it, still counted as seen. False when no beat is up.</summary>
        public static bool SkipShowing()
        {
            if (_instance == null || _instance._sequence == null)
            {
                return false;
            }

            _instance.Skip();
            return true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null || FindFirstObjectByType<StoryPanelController>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(StoryPanelController));
            host.AddComponent<UIDocument>();
            host.AddComponent<StoryPanelController>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }

            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                _uiDocument = gameObject.AddComponent<UIDocument>();
            }

            _uiDocument.sortingOrder = SortingOrder;
            // Self-bootstrapped document: outside UiClickSoundBinder's per-scene scans.
            Rapadura.Audio.UiClickSoundBinder.Register(_uiDocument);
        }

        private void OnEnable()
        {
            _root = null;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            DetachCredits();
            _sequence = null;
            SetRootVisible(false);
            GameplayPause.Release(this);
            _localizer.Detach();
            _root = null;
        }

        private void OnDestroy()
        {
            GameplayPause.Release(this);
            Rapadura.Audio.UiClickSoundBinder.Unregister(_uiDocument);
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _map = null;
            _mapLookedUp = false;
            _readyTime = -1f;

            // A beat never spans a scene swap (encounters only start from an idle map).
            if (_sequence != null)
            {
                _sequence = null;
                SetRootVisible(false);
            }

            DetachCredits();
            _waitingForCutscene = false;
            GameplayPause.Release(this);
        }

        private void Update()
        {
            if (_releasePauseAtFrame >= 0 && Time.frameCount >= _releasePauseAtFrame)
            {
                _releasePauseAtFrame = -1;
                GameplayPause.Release(this);
            }

            if (_sequence != null)
            {
                PollAdvanceInput();
                return;
            }

            if (_waitingForCutscene)
            {
                if (!CutscenePlayerController.IsPlaying)
                {
                    _waitingForCutscene = false; // dropped without its callback (scene swap)
                }

                return;
            }

            if (_credits != null)
            {
                // Credits closed by a path that skipped its Closed event (disabled with the scene).
                if (!_credits.isActiveAndEnabled || !_credits.IsOpen)
                {
                    DetachCredits();
                }

                return;
            }

            TryStartPendingBeat();
        }

        private void TryStartPendingBeat()
        {
            if (!AutoPresent || !ServiceLocator.TryGet(out StoryProgress story))
            {
                _readyTime = -1f;
                return;
            }

            StoryBeat pending = story.GetPendingBeat();
            if (pending == StoryBeat.None)
            {
                _readyTime = -1f;
                return;
            }

            bool worldInteractive = ServiceLocator.TryGet(out GameFlowService flow) && GameFlowMachine.IsWorldInteractive(flow.State);
            bool hasHero = flow != null && flow.HasHero;
            WorldMapController map = FindMap();
            bool mapReady = map != null && map.isActiveAndEnabled && map.Progress != null;

            if (!StoryPresentationRules.CanPresent(pending, worldInteractive, hasHero, mapReady, mapReady && map.IsTraveling, GameplayPause.IsPausedByOthers(this)))
            {
                _readyTime = -1f;
                return;
            }

            if (_readyTime < 0f)
            {
                _readyTime = Time.unscaledTime;
            }

            if (Time.unscaledTime - _readyTime < _startDelaySeconds || !EnsureBound())
            {
                return;
            }

            ServiceLocator.TryGet(out CharacterSelectionState selection);
            CharacterId? hero = selection != null ? selection.Selected : null;
            int cycle = NewGamePlusProgress.ResolveCurrentCycle();

            // New game: the intro cutscene first, then the prologue panels (Docs/Narrative.md, "Cutscenes").
            CutsceneDefinition before = CutsceneScript.For(pending, CutscenePlacement.BeforePanels, cycle);
            if (PlayCutscene(before, () => Begin(new StoryPanelSequence(pending, hero, cycle), hero)))
            {
                _readyTime = -1f;
                return;
            }

            Begin(new StoryPanelSequence(pending, hero, cycle), hero);
        }

        /// <summary>Plays <paramref name="definition"/> once per session (see <see cref="CutsceneRules.ShouldPlay"/>), then <paramref name="then"/>. False when it does not play.</summary>
        private bool PlayCutscene(CutsceneDefinition definition, System.Action then)
        {
            if (!CutsceneRules.ShouldPlay(definition, CutscenePlayerController.AutoPlay, definition != null && _cutscenesPlayed.Contains(definition.Id)))
            {
                return false;
            }

            _cutscenesPlayed.Add(definition.Id);
            _waitingForCutscene = true;
            if (CutscenePlayerController.TryPlay(definition, () =>
                {
                    _waitingForCutscene = false;
                    then?.Invoke();
                }))
            {
                return true;
            }

            _waitingForCutscene = false;
            return false;
        }

        private WorldMapController FindMap()
        {
            if (_map == null && !_mapLookedUp)
            {
                // Once per scene: Main.unity has no map, and a failed lookup every frame is wasteful.
                _mapLookedUp = true;
                _map = FindFirstObjectByType<WorldMapController>(FindObjectsInactive.Include);
            }

            return _map;
        }

        private void Begin(StoryPanelSequence sequence, CharacterId? hero)
        {
            _readyTime = -1f;
            _releasePauseAtFrame = -1;

            if (sequence.IsFinished)
            {
                // A beat with no panels: nothing to read, just move past it.
                Complete(sequence);
                return;
            }

            _sequence = sequence;
            GameplayPause.Acquire(this);

            if (_card != null)
            {
                _card.style.borderTopColor = _card.style.borderBottomColor = _card.style.borderLeftColor = _card.style.borderRightColor =
                    hero.HasValue ? new StyleColor(CharacterRegistry.Get(hero.Value).AccentColor) : new StyleColor(StyleKeyword.Null);
            }

            SetRootVisible(true);
            _lastAdvanceAt = Time.unscaledTime;
            Render(animate: true);
            UiAccessibilityApplier.RequestRefresh();
            _nextButton?.Focus();
        }

        /// <summary>Gives the document a panel (borrowed PanelSettings) and clones the view into it once. False until it can render.</summary>
        private bool EnsureBound()
        {
            if (_uiDocument.panelSettings == null)
            {
                PanelSettings donor = FindPanelSettings();
                if (donor == null)
                {
                    return false;
                }

                _uiDocument.panelSettings = donor;
                _uiDocument.sortingOrder = SortingOrder;
            }

            VisualElement documentRoot = _uiDocument.rootVisualElement;
            if (documentRoot == null)
            {
                return false;
            }

            if (_root != null && _root.parent != null)
            {
                return true;
            }

            if (_viewAsset == null)
            {
                _viewAsset = Resources.Load<VisualTreeAsset>(ViewResourcePath);
                if (_viewAsset == null)
                {
                    Debug.LogWarning($"[StoryPanelController] Missing Resources/{ViewResourcePath}.uxml; story panels disabled.");
                    enabled = false;
                    return false;
                }
            }

            documentRoot.pickingMode = PickingMode.Ignore;
            _root = documentRoot.Q<VisualElement>("story-root");
            if (_root == null)
            {
                _viewAsset.CloneTree(documentRoot);
                _root = documentRoot.Q<VisualElement>("story-root");
            }

            if (_root == null)
            {
                return false;
            }

            // CloneTree wraps the view in a TemplateContainer: it must not catch input while hidden.
            for (VisualElement element = _root.parent; element != null && element != documentRoot; element = element.parent)
            {
                element.pickingMode = PickingMode.Ignore;
            }

            _card = _root.Q<VisualElement>("story-card");
            _titleLabel = _root.Q<Label>("story-title");
            _bodyLabel = _root.Q<Label>("story-body");
            _progressLabel = _root.Q<Label>("story-progress");
            _skipButton = _root.Q<Button>("story-skip");
            _nextButton = _root.Q<Button>("story-next");

            if (_skipButton != null)
            {
                _skipButton.clicked -= Skip;
                _skipButton.clicked += Skip;
            }

            if (_nextButton != null)
            {
                _nextButton.clicked -= OnNextClicked;
                _nextButton.clicked += OnNextClicked;
            }

            _root.UnregisterCallback<PointerUpEvent>(OnBackdropPointerUp);
            _root.RegisterCallback<PointerUpEvent>(OnBackdropPointerUp);
            _root.UnregisterCallback<NavigationCancelEvent>(OnNavigationCancel);
            _root.RegisterCallback<NavigationCancelEvent>(OnNavigationCancel);

            _localizer.Attach(_root, () => Render(animate: false), StoryTexts.Get);
            SetRootVisible(false);
            return true;
        }

        private PanelSettings FindPanelSettings()
        {
            foreach (UIDocument document in FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (document != _uiDocument && document.panelSettings != null)
                {
                    return document.panelSettings;
                }
            }

            return null;
        }

        private void Render(bool animate)
        {
            if (_sequence == null || _sequence.IsFinished)
            {
                return;
            }

            if (_localization == null && ServiceLocator.TryGet(out _localization))
            {
                _localizer.SetManager(_localization);
            }
            else
            {
                _localizer.Refresh();
            }

            if (_titleLabel != null) _titleLabel.text = StoryTexts.Get(_localization, _sequence.TitleKey);
            if (_progressLabel != null) _progressLabel.text = StoryTexts.Format(_localization, StoryTexts.Progress, _sequence.Index + 1, _sequence.Count);

            if (_nextButton != null)
            {
                string nextKey = !_sequence.IsLastPanel
                    ? StoryTexts.Next
                    : StoryPresentationRules.RollsCredits(_sequence.Beat) ? StoryTexts.ToCredits : StoryTexts.Finish;
                _nextButton.text = StoryTexts.Get(_localization, nextKey);
            }

            if (_bodyLabel == null)
            {
                return;
            }

            _bodyLabel.text = StoryTexts.Get(_localization, _sequence.CurrentKey);

            ServiceLocator.TryGet(out AccessibilitySettings accessibility);
            bool noMotion = accessibility != null && accessibility.SuppressUiMotion;
            _bodyLabel.EnableInClassList(NoMotionClass, noMotion);

            if (animate && !noMotion)
            {
                // Hidden this frame, transition back in on the next one.
                _bodyLabel.AddToClassList(EnteringClass);
                _bodyLabel.schedule.Execute(() => _bodyLabel.RemoveFromClassList(EnteringClass));
            }
            else
            {
                _bodyLabel.RemoveFromClassList(EnteringClass);
            }
        }

        private void PollAdvanceInput()
        {
            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;

            // The same press may also reach the focused "Continuar" through UI navigation: the advance
            // cooldown turns that into a single step.
            bool next = (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)) ||
                        (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame);

            if (next)
            {
                Advance();
            }
        }

        private void OnBackdropPointerUp(PointerUpEvent evt)
        {
            if (_sequence == null)
            {
                return;
            }

            // Taps on the buttons are theirs (Pular must never also count as "next").
            for (VisualElement element = evt.target as VisualElement; element != null; element = element.parent)
            {
                if (element is Button)
                {
                    return;
                }
            }

            Advance();
        }

        private void OnNavigationCancel(NavigationCancelEvent evt)
        {
            if (_sequence == null)
            {
                return;
            }

            evt.StopPropagation();
            Skip();
        }

        private void OnNextClicked() => Advance();

        private void Advance()
        {
            if (_sequence == null || Time.unscaledTime - _lastAdvanceAt < AdvanceCooldownSeconds)
            {
                return;
            }

            _lastAdvanceAt = Time.unscaledTime;
            if (_sequence.Advance())
            {
                Render(animate: true);
                return;
            }

            Finish();
        }

        private void Skip()
        {
            if (_sequence == null)
            {
                return;
            }

            _sequence.Skip();
            Finish();
        }

        private void Finish()
        {
            StoryPanelSequence finished = _sequence;
            _sequence = null;
            SetRootVisible(false);
            Complete(finished);
        }

        /// <summary>Marks the beat seen, saves, and rolls the credits after the ending; the pause goes a frame later.</summary>
        private void Complete(StoryPanelSequence finished)
        {
            if (ServiceLocator.TryGet(out StoryProgress story))
            {
                story.MarkSeen(finished.Beat);
            }

            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.AutoSave();
            }

            if (StoryPresentationRules.RollsCredits(finished.Beat))
            {
                // The outro cutscene (the light runs home, Vó Edna's porch) leads into the credits.
                CutsceneDefinition after = CutsceneScript.For(finished.Beat, CutscenePlacement.AfterPanels, NewGamePlusProgress.ResolveCurrentCycle());
                if (!PlayCutscene(after, OpenCredits))
                {
                    OpenCredits();
                }
            }

            _releasePauseAtFrame = Time.frameCount + 1;
        }

        private void OpenCredits()
        {
            CreditsController credits = FindFirstObjectByType<CreditsController>();
            if (credits == null || !credits.isActiveAndEnabled)
            {
                return;
            }

            _credits = credits;
            _credits.Closed += OnCreditsClosed;
            _credits.Show();
        }

        private void OnCreditsClosed() => DetachCredits();

        private void DetachCredits()
        {
            if (_credits != null)
            {
                _credits.Closed -= OnCreditsClosed;
                _credits = null;
            }
        }

        private void SetRootVisible(bool visible)
        {
            if (_root != null)
            {
                _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                _root.pickingMode = visible ? PickingMode.Position : PickingMode.Ignore;
            }
        }
    }
}
