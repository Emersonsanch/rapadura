using System.Collections.Generic;
using Rapadura.Audio;
using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Cooking;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Fishing;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Save;
using Rapadura.UI.Common;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Rapadura.UI.Fishing
{
    /// <summary>
    /// The fishing minigame window ("Pesca no açude"), opened by <see cref="FishingSpotButton"/>. Built in code on
    /// the map HUD's document like the kitchen window: under <c>wm-hud-root</c> (a <c>rap-theme</c> root) with the
    /// Mochila overlay classes plus <c>Resources/UI/Fishing.uss</c>. Holds GameplayPause while open.
    /// <list type="bullet">
    /// <item>The game itself is <see cref="FishingSession"/> (pure); this class only reads input, ticks it with
    /// unscaled time and draws it: the water with the float (bobbing, twitching on nibbles, trembling before the bite,
    /// sunk during the bite window), the tension bar (red zone at the top) and the line-reeled bar.</item>
    /// <item>One action: the big pad (pointer down/up), Space or the gamepad's south button. Tap = cast / hook /
    /// again; hold = reel.</item>
    /// <item>Assist (<see cref="FishingPresenter.IsAssist"/>): the "Pesca tranquila" toggle (PlayerPrefs) or
    /// "Reduzir Movimento". Reduced motion also stills the float: states are shown by colour and a "!" mark.</item>
    /// <item>A landed fish goes to <see cref="FishingProgress"/> (collection, record, achievements, dishes) and to
    /// the carried backpack (a short <see cref="CarriedInventorySession"/>, as the kitchen does) if it fits.</item>
    /// <item>Each finished cast passes 15 in-game minutes, so a long session reaches dusk or night (and its fish).</item>
    /// </list>
    /// </summary>
    [DisallowMultipleComponent]
    public class FishingPanelController : MonoBehaviour
    {
        public const string OverlayName = "wm-fishing-overlay";
        public const string StyleResourcePath = "UI/Fishing";
        public const string ActionPadName = "wm-fishing-action";
        public const string RelaxedPrefKey = "Fishing.Relaxed";
        /// <summary>In-game hours each finished cast takes.</summary>
        public const float HoursPerCast = 0.25f;
        /// <summary>Seconds a result stays before the pad casts again (time to read it).</summary>
        public const float ResultLockSeconds = 0.45f;

        private UIDocument _hudDocument;
        private VisualElement _overlay;
        private Label _titleLabel;
        private Label _introLabel;
        private Label _conditionsLabel;
        private VisualElement _gameSection;
        private VisualElement _water;
        private VisualElement _float;
        private Label _floatMark;
        private VisualElement _tensionRow;
        private VisualElement _tensionFill;
        private Label _tensionLabel;
        private VisualElement _progressFill;
        private Label _progressLabel;
        private Label _hintLabel;
        private Label _resultLabel;
        private VisualElement _actionPad;
        private Label _actionLabel;
        private Toggle _relaxedToggle;
        private Label _relaxedNote;
        private Button _collectionButton;
        private VisualElement _collectionSection;
        private ScrollView _collectionList;
        private Button _closeButton;

        private FishingSpotSpec _spot;
        private FishingSession _session;
        private CharacterId? _hero;
        private bool _open;
        private bool _showingCollection;
        private bool _pointerHeld;
        private float _resultAt;
        private float _bobTime;
        private string _pendingLearned;
        private bool _subscribed;

        public bool IsOpen => _open;

        public FishingSession Session => _session;

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        private static AccessibilitySettings Accessibility => ServiceLocator.TryGet(out AccessibilitySettings settings) ? settings : null;

        public static bool RelaxedPreference
        {
            get => PlayerPrefs.GetInt(RelaxedPrefKey, 0) != 0;
            set => PlayerPrefs.SetInt(RelaxedPrefKey, value ? 1 : 0);
        }

        private static bool ReduceMotion
        {
            get
            {
                AccessibilitySettings settings = Accessibility;
                return settings != null && settings.ReduceMotion;
            }
        }

        private void Awake()
        {
            _hudDocument = GetComponent<UIDocument>();
        }

        private void OnDisable()
        {
            Close();
            _overlay?.RemoveFromHierarchy();
            _overlay = null;
        }

        // ------------------------------------------------------------------
        // Open / close
        // ------------------------------------------------------------------

        public void Open(FishingSpotSpec spot)
        {
            if (_open || spot == null || !EnsureOverlay())
            {
                return;
            }

            _spot = spot;
            _hero = ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;
            _session = NewSession();
            _showingCollection = false;
            _pointerHeld = false;
            _pendingLearned = null;
            _open = true;
            _overlay.style.display = DisplayStyle.Flex;
            UiMotionPreferences.ApplyTo(_overlay, UiMotionPreferences.IsReducedMotion());
            GameplayPause.Acquire(this);
            Subscribe();

            if (ServiceLocator.TryGet(out SfxEventRouter router) && router != null)
            {
                FishingAudio.EnsureCues(router.Map);
            }

            _resultLabel.text = string.Empty;
            RefreshStatic();
            Render();
            _actionPad.Focus();
        }

        public void Close()
        {
            if (_overlay != null)
            {
                _overlay.style.display = DisplayStyle.None;
            }

            _open = false;
            _pointerHeld = false;
            Unsubscribe();
            GameplayPause.Release(this);
        }

        private void Subscribe()
        {
            if (_subscribed)
            {
                return;
            }

            EventBus.Subscribe<MealRecipeLearnedEvent>(OnRecipeLearned);
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
            {
                return;
            }

            EventBus.Unsubscribe<MealRecipeLearnedEvent>(OnRecipeLearned);
            _subscribed = false;
        }

        private void OnRecipeLearned(MealRecipeLearnedEvent evt)
        {
            if (evt.Source == MealLearnSource.Fishing)
            {
                MealRecipeSpec recipe = MealCatalog.Get(evt.RecipeId);
                _pendingLearned = KitchenTexts.Format(Localization, KitchenTexts.StatusLearned, KitchenPresenter.DishName(Localization, recipe));
            }
        }

        private bool Assist => FishingPresenter.IsAssist(RelaxedPreference, ReduceMotion);

        private FishingSession NewSession()
        {
            TimeOfDayPhase phase = ServiceLocator.TryGet(out TimeOfDayManager clock) ? clock.Phase : TimeOfDayPhase.Day;
            WeatherType weather = ServiceLocator.TryGet(out WeatherSystem sky) ? sky.CurrentWeather : WeatherType.Clear;
            int seed = unchecked(System.Environment.TickCount * 31 + (int)_spot.Id);
            return new FishingSession(_spot, phase, weather, Assist, seed);
        }

        /// <summary>A new cast under the current sky: rebuild the session if the phase or the weather changed.</summary>
        private void RefreshConditions()
        {
            TimeOfDayPhase phase = ServiceLocator.TryGet(out TimeOfDayManager clock) ? clock.Phase : TimeOfDayPhase.Day;
            WeatherType weather = ServiceLocator.TryGet(out WeatherSystem sky) ? sky.CurrentWeather : WeatherType.Clear;
            if (_session == null || _session.TimeOfDay != phase || _session.Weather != weather)
            {
                _session = NewSession();
            }

            _session.Assist = Assist;
            _conditionsLabel.text = FishingPresenter.ConditionsLine(Localization, _spot, _session.TimeOfDay, _session.Weather);
        }

        // ------------------------------------------------------------------
        // Loop
        // ------------------------------------------------------------------

        private void Update()
        {
            if (!_open || _session == null)
            {
                return;
            }

            float dt = Time.unscaledDeltaTime;
            _bobTime += dt;

            if (!_showingCollection)
            {
                bool pressed = PressedThisFrame();
                if (pressed)
                {
                    HandlePress();
                }

                _session.Tick(dt, IsHeld());
                HandleCues();
            }

            Render();
        }

        private bool PadHasFocus()
        {
            Focusable focused = _overlay?.focusController?.focusedElement;
            return focused == null || focused == _actionPad;
        }

        private bool PressedThisFrame()
        {
            if (!PadHasFocus())
            {
                return false;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            return gamepad != null && gamepad.buttonSouth.wasPressedThisFrame;
        }

        private bool IsHeld()
        {
            if (_pointerHeld)
            {
                return true;
            }

            if (!PadHasFocus())
            {
                return false;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.spaceKey.isPressed)
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            return gamepad != null && gamepad.buttonSouth.isPressed;
        }

        private void HandlePress()
        {
            if (_session.Phase == FishingPhase.Result && Time.unscaledTime - _resultAt < ResultLockSeconds)
            {
                return;
            }

            if (_session.Phase == FishingPhase.Ready || _session.Phase == FishingPhase.Result)
            {
                RefreshConditions();
                _resultLabel.text = string.Empty;
                _resultLabel.RemoveFromClassList("fishing-result--good");
                _resultLabel.RemoveFromClassList("fishing-result--bad");
            }

            _session.Press();
            HandleCues();
        }

        private void HandleCues()
        {
            IReadOnlyList<FishingCue> cues = _session.Cues;
            for (int i = 0; i < cues.Count; i++)
            {
                FishingCue cue = cues[i];
                string sound = FishingAudio.CueFor(cue, _session.Fish);
                if (sound != null)
                {
                    FishingAudio.Play(sound);
                }

                switch (cue)
                {
                    case FishingCue.Bite:
                        PlayHaptic(HapticPattern.Light);
                        break;
                    case FishingCue.Snapped:
                        PlayHaptic(HapticPattern.Medium);
                        break;
                    case FishingCue.Landed:
                        PlayHaptic(HapticPattern.Success);
                        break;
                }

                if (cue == FishingCue.Landed || cue == FishingCue.Snapped || cue == FishingCue.Escaped || cue == FishingCue.TooEarly || cue == FishingCue.TooLate)
                {
                    OnCastFinished();
                }
            }

            if (_session.Phase == FishingPhase.Reeling && IsHeld())
            {
                FishingAudio.Play(FishingAudio.Reel);
            }
        }

        private void OnCastFinished()
        {
            _resultAt = Time.unscaledTime;
            if (ServiceLocator.TryGet(out TimeOfDayManager clock))
            {
                clock.AdvanceHours(HoursPerCast);
            }

            LocalizationManager localization = Localization;
            FishSpec fish = _session.Fish;
            bool landed = _session.Result == FishingResult.Landed && fish != null;
            FishCatchOutcome outcome = default;
            FishBagStatus bag = FishBagStatus.Added;

            if (landed)
            {
                _pendingLearned = null;
                outcome = FishingProgress.ResolveShared().RecordCatch(fish.Id, _session.LandedGrams);
                bag = PutInBackpack(fish.Id);
                if (outcome.NewRecord && !outcome.NewSpecies)
                {
                    FishingAudio.Play(FishingAudio.Record);
                }
            }

            string text = FishingPresenter.ResultText(localization, _session.Result, fish, outcome, bag, _hero);
            if (landed && !string.IsNullOrEmpty(_pendingLearned))
            {
                text += "\n" + _pendingLearned;
                _pendingLearned = null;
            }

            if (landed && FishingProgress.ResolveShared().Completed && outcome.NewSpecies)
            {
                text += "\n" + FishingTexts.Get(localization, FishingTexts.CollectionComplete);
            }

            _resultLabel.text = text;
            _resultLabel.EnableInClassList("fishing-result--good", landed);
            _resultLabel.EnableInClassList("fishing-result--bad", !landed);
            _collectionButton.text = FishingPresenter.CollectionButton(localization, FishingProgress.ResolveShared());
            RefreshConditionsLabelOnly();
        }

        private void RefreshConditionsLabelOnly()
        {
            TimeOfDayPhase phase = ServiceLocator.TryGet(out TimeOfDayManager clock) ? clock.Phase : _session.TimeOfDay;
            WeatherType weather = ServiceLocator.TryGet(out WeatherSystem sky) ? sky.CurrentWeather : _session.Weather;
            _conditionsLabel.text = FishingPresenter.ConditionsLine(Localization, _spot, phase, weather);
        }

        /// <summary>Into the carried backpack if it fits (the map has no Player: same short session as the kitchen).</summary>
        private FishBagStatus PutInBackpack(string itemId)
        {
            ItemDefinition item = ItemDatabase.GetById(itemId);
            if (item == null)
            {
                return FishBagStatus.BagFull;
            }

            InventoryManager bag = FindFirstObjectByType<InventoryManager>();
            CarriedInventorySession session = null;
            if (bag == null)
            {
                ServiceLocator.TryGet(out SaveManager saveManager);
                session = new CarriedInventorySession(saveManager);
                bag = session.Open(transform);
            }

            int leftover = bag != null ? bag.AddItem(item, 1) : 1;
            session?.Close();
            return leftover == 0 ? FishBagStatus.Added : FishBagStatus.BagFull;
        }

        private static void PlayHaptic(HapticPattern pattern)
        {
            if (ServiceLocator.TryGet(out HapticsService haptics) && haptics != null)
            {
                haptics.Play(pattern);
            }
        }

        // ------------------------------------------------------------------
        // Drawing
        // ------------------------------------------------------------------

        private void RefreshStatic()
        {
            LocalizationManager localization = Localization;
            _titleLabel.text = FishingPresenter.SpotName(localization, _spot);
            _introLabel.text = FishingTexts.Get(localization, _spot.IntroKey);
            _conditionsLabel.text = FishingPresenter.ConditionsLine(localization, _spot, _session.TimeOfDay, _session.Weather);
            _tensionLabel.text = FishingTexts.Get(localization, FishingTexts.Tension);
            _progressLabel.text = FishingTexts.Get(localization, FishingTexts.Progress);
            _relaxedToggle.text = FishingTexts.Get(localization, FishingTexts.RelaxedToggle);
            _relaxedToggle.SetValueWithoutNotify(RelaxedPreference);
            _relaxedNote.text = ReduceMotion ? FishingTexts.Get(localization, FishingTexts.RelaxedByMotion) : string.Empty;
            _relaxedNote.style.display = ReduceMotion ? DisplayStyle.Flex : DisplayStyle.None;
            _collectionButton.text = FishingPresenter.CollectionButton(localization, FishingProgress.ResolveShared());
            _closeButton.text = FishingTexts.Get(localization, FishingTexts.Close);
            _gameSection.style.display = _showingCollection ? DisplayStyle.None : DisplayStyle.Flex;
            _collectionSection.style.display = _showingCollection ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void Render()
        {
            if (_overlay == null || _session == null || _showingCollection)
            {
                return;
            }

            LocalizationManager localization = Localization;
            _hintLabel.text = FishingPresenter.Hint(localization, _session);
            _actionLabel.text = FishingPresenter.ActionLabel(localization, _session);

            bool reduced = ReduceMotion;
            bool inWater = _session.Phase == FishingPhase.Waiting || _session.Phase == FishingPhase.Reeling;
            bool trembling = _session.Phase == FishingPhase.Waiting && !_session.FloatSunk && _session.PhaseTime >= _session.Plan.TrembleAt;
            _float.style.display = inWater ? DisplayStyle.Flex : DisplayStyle.None;
            _float.EnableInClassList("fishing-float--twitch", _session.FloatTwitching && !trembling);
            _float.EnableInClassList("fishing-float--tremble", trembling);
            _float.EnableInClassList("fishing-float--sunk", _session.FloatSunk);
            _float.EnableInClassList("fishing-float--hooked", _session.Phase == FishingPhase.Reeling);
            _floatMark.text = _session.FloatSunk ? "!" : (trembling ? "?" : string.Empty);
            _water.EnableInClassList("fishing-water--bite", _session.FloatSunk);

            // Motion: a gentle bob, a twitch on nibbles, a shiver before the bite, down when sunk. Reduced motion: still,
            // the classes above carry the state (colour, size, the mark).
            float y = 0f;
            float x = 0f;
            if (!reduced && inWater)
            {
                y = Mathf.Sin(_bobTime * 2.2f) * 2f;
                if (_session.FloatTwitching && !trembling)
                {
                    y += 5f;
                }

                if (trembling)
                {
                    x = Mathf.Sin(_bobTime * 60f) * 2.5f;
                }

                if (_session.FloatSunk)
                {
                    y = 12f;
                }

                if (_session.Phase == FishingPhase.Reeling)
                {
                    x = Mathf.Sin(_bobTime * (_session.Reel.Surging ? 30f : 6f)) * (_session.Reel.Surging ? 8f : 3f);
                    y = 6f;
                }
            }

            _float.style.translate = new Translate(new Length(x, LengthUnit.Pixel), new Length(y, LengthUnit.Pixel));

            bool reeling = _session.Phase == FishingPhase.Reeling;
            _tensionRow.EnableInClassList("fishing-bars--idle", !reeling);
            float tension = reeling ? _session.Reel.Tension : 0f;
            float progress = reeling ? _session.Reel.Progress : (_session.Result == FishingResult.Landed ? 1f : 0f);
            _tensionFill.style.width = new Length(tension * 100f, LengthUnit.Percent);
            _tensionFill.EnableInClassList("fishing-tension-fill--danger", tension >= 0.8f);
            _tensionFill.EnableInClassList("fishing-tension-fill--slack", reeling && tension <= 0.12f);
            _progressFill.style.width = new Length(progress * 100f, LengthUnit.Percent);

            _actionPad.EnableInClassList("fishing-action--bite", _session.FloatSunk);
            _actionPad.EnableInClassList("fishing-action--held", reeling && IsHeld());
            _actionPad.EnableInClassList("fishing-action--wait", _session.Phase == FishingPhase.Casting);
        }

        private void ShowCollection(bool show)
        {
            _showingCollection = show;
            _pointerHeld = false;
            LocalizationManager localization = Localization;
            if (show)
            {
                _collectionList.Clear();
                foreach (FishCollectionRow row in FishingPresenter.CollectionRows(localization, FishingProgress.ResolveShared()))
                {
                    var element = new VisualElement { name = "wm-fishing-row-" + row.FishId };
                    element.AddToClassList("fishing-row");
                    element.AddToClassList(FishingPresenter.RarityClass(row.Rarity));
                    element.EnableInClassList("fishing-row--unknown", !row.Caught);
                    var title = new Label(row.Title);
                    title.AddToClassList("fishing-row-title");
                    element.Add(title);
                    var where = new Label(row.Where);
                    where.AddToClassList("fishing-row-line");
                    element.Add(where);
                    _collectionList.Add(element);
                }

                _collectionButton.text = FishingTexts.Get(localization, FishingTexts.BackToWater);
            }
            else
            {
                _collectionButton.text = FishingPresenter.CollectionButton(localization, FishingProgress.ResolveShared());
            }

            _gameSection.style.display = show ? DisplayStyle.None : DisplayStyle.Flex;
            _collectionSection.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (!show)
            {
                _actionPad.Focus();
            }
        }

        // ------------------------------------------------------------------
        // Build
        // ------------------------------------------------------------------

        private bool EnsureOverlay()
        {
            if (_hudDocument == null || _hudDocument.rootVisualElement == null)
            {
                return false;
            }

            VisualElement root = _hudDocument.rootVisualElement.Q<VisualElement>("wm-hud-root") ?? _hudDocument.rootVisualElement;
            if (_overlay != null && _overlay.parent == root)
            {
                return true;
            }

            _overlay?.RemoveFromHierarchy();

            _overlay = new VisualElement { name = OverlayName };
            _overlay.AddToClassList("wm-backpack-overlay");
            _overlay.style.display = DisplayStyle.None;
            var sheet = Resources.Load<StyleSheet>(StyleResourcePath);
            if (sheet != null)
            {
                _overlay.styleSheets.Add(sheet);
            }

            var panel = new VisualElement { name = "wm-fishing-panel" };
            panel.AddToClassList("rap-panel");
            panel.AddToClassList("wm-backpack-panel");
            panel.AddToClassList("fishing-panel");
            _overlay.Add(panel);

            _titleLabel = AddLabel(panel, "wm-backpack-title");
            _introLabel = AddLabel(panel, "fishing-intro");
            _conditionsLabel = AddLabel(panel, "fishing-conditions");

            _gameSection = new VisualElement { name = "wm-fishing-game" };
            _gameSection.AddToClassList("fishing-game");
            panel.Add(_gameSection);

            _water = new VisualElement { name = "wm-fishing-water", pickingMode = PickingMode.Ignore };
            _water.AddToClassList("fishing-water");
            _gameSection.Add(_water);

            _float = new VisualElement { name = "wm-fishing-float", pickingMode = PickingMode.Ignore };
            _float.AddToClassList("fishing-float");
            _floatMark = new Label { pickingMode = PickingMode.Ignore };
            _floatMark.AddToClassList("fishing-float-mark");
            _float.Add(_floatMark);
            _water.Add(_float);

            _hintLabel = AddLabel(_gameSection, "fishing-hint");

            _tensionRow = new VisualElement { name = "wm-fishing-bars" };
            _tensionRow.AddToClassList("fishing-bars");
            _gameSection.Add(_tensionRow);
            _tensionLabel = AddLabel(_tensionRow, "fishing-bar-label");
            _tensionFill = AddBar(_tensionRow, "wm-fishing-tension", "fishing-tension-track", "fishing-tension-fill", withDangerZone: true);
            _progressLabel = AddLabel(_tensionRow, "fishing-bar-label");
            _progressFill = AddBar(_tensionRow, "wm-fishing-progress", "fishing-progress-track", "fishing-progress-fill", withDangerZone: false);

            _actionPad = new VisualElement { name = ActionPadName, focusable = true };
            _actionPad.AddToClassList("fishing-action");
            _actionLabel = new Label { pickingMode = PickingMode.Ignore };
            _actionLabel.AddToClassList("fishing-action-label");
            _actionPad.Add(_actionLabel);
            _actionPad.RegisterCallback<PointerDownEvent>(OnPadDown);
            _actionPad.RegisterCallback<PointerUpEvent>(OnPadUp);
            _actionPad.RegisterCallback<PointerCaptureOutEvent>(_ => _pointerHeld = false);
            // Space / gamepad south are read in Update; keep the UI's own submit from double-firing.
            _actionPad.RegisterCallback<NavigationSubmitEvent>(evt => evt.StopPropagation());
            _gameSection.Add(_actionPad);

            _resultLabel = AddLabel(_gameSection, "fishing-result");

            _collectionSection = new VisualElement { name = "wm-fishing-collection" };
            _collectionSection.AddToClassList("fishing-collection");
            _collectionSection.style.display = DisplayStyle.None;
            panel.Add(_collectionSection);
            AddLabel(_collectionSection, "fishing-collection-title").text = FishingTexts.Get(Localization, FishingTexts.CollectionTitle);
            _collectionList = new ScrollView(ScrollViewMode.Vertical) { name = "wm-fishing-collection-list" };
            _collectionList.AddToClassList("fishing-collection-list");
            _collectionSection.Add(_collectionList);

            var options = new VisualElement();
            options.AddToClassList("fishing-options");
            panel.Add(options);
            _relaxedToggle = new Toggle { name = "wm-fishing-relaxed", focusable = true };
            _relaxedToggle.AddToClassList("fishing-relaxed");
            _relaxedToggle.RegisterValueChangedCallback(evt =>
            {
                RelaxedPreference = evt.newValue;
                if (_session != null && (_session.Phase == FishingPhase.Ready || _session.Phase == FishingPhase.Result))
                {
                    _session.Assist = Assist;
                }

                _actionPad.Focus();
            });
            options.Add(_relaxedToggle);
            _relaxedNote = AddLabel(options, "fishing-relaxed-note");

            var buttons = new VisualElement();
            buttons.AddToClassList("fishing-buttons");
            panel.Add(buttons);
            _collectionButton = new Button(() => ShowCollection(!_showingCollection)) { name = "wm-fishing-collection-button", focusable = true };
            _collectionButton.AddToClassList("rap-button");
            _collectionButton.AddToClassList("fishing-collection-button");
            buttons.Add(_collectionButton);
            _closeButton = new Button(Close) { name = "wm-fishing-close-button", focusable = true };
            _closeButton.AddToClassList("rap-button");
            _closeButton.AddToClassList("rap-button--primary");
            _closeButton.AddToClassList("wm-backpack-close");
            buttons.Add(_closeButton);

            root.Add(_overlay);
            return true;
        }

        private void OnPadDown(PointerDownEvent evt)
        {
            if (!_open || _showingCollection)
            {
                return;
            }

            _pointerHeld = true;
            _actionPad.CapturePointer(evt.pointerId);
            _actionPad.Focus();
            HandlePress();
            evt.StopPropagation();
        }

        private void OnPadUp(PointerUpEvent evt)
        {
            _pointerHeld = false;
            if (_actionPad.HasPointerCapture(evt.pointerId))
            {
                _actionPad.ReleasePointer(evt.pointerId);
            }

            evt.StopPropagation();
        }

        private static Label AddLabel(VisualElement parent, string className)
        {
            var label = new Label();
            label.AddToClassList(className);
            parent.Add(label);
            return label;
        }

        private static VisualElement AddBar(VisualElement parent, string name, string trackClass, string fillClass, bool withDangerZone)
        {
            var track = new VisualElement { name = name, pickingMode = PickingMode.Ignore };
            track.AddToClassList(trackClass);
            if (withDangerZone)
            {
                var danger = new VisualElement { pickingMode = PickingMode.Ignore };
                danger.AddToClassList("fishing-tension-danger");
                track.Add(danger);
            }

            var fill = new VisualElement { pickingMode = PickingMode.Ignore };
            fill.AddToClassList(fillClass);
            track.Add(fill);
            parent.Add(track);
            return fill;
        }
    }
}
