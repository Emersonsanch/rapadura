using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Chatter;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// "Conversas entre primos na estrada": a small, non-blocking bubble on the world map HUD while the token travels —
    /// the active cousin and another one on the battery radio (or remembered), two lines (<see cref="RoadChatterCatalog"/>).
    /// Self-bootstrapping like <see cref="CarameloCuddleButton"/> (added to the <see cref="WorldMapHudController"/>'s
    /// GameObject after every scene load, note injected under <c>wm-hud-root</c>, picking ignored), no scene edit.
    /// <list type="bullet">
    /// <item>Each trip is rolled once (<see cref="RoadChatterRules.RollTrip"/>: cooldown in <see cref="RoadChatterState"/>,
    /// saved); a hit pops the bubble early in the trip (<see cref="RoadChatterSettings.TriggerProgress"/>, before any road
    /// event can pop at 0.3+), from <see cref="WorldMapController.TravelProgressed"/>.</item>
    /// <item>Never with a road event card, a story panel, a menu (all hold GameplayPause) or a conversation open, nor while
    /// a story beat waits (<see cref="RoadChatterRules.CanShow"/>); it hides as soon as any of them shows up.</item>
    /// <item>Which exchange: <see cref="RoadChatterRules.Pick"/> (stage from the interludes seen, Nova Jornada+ cycle,
    /// unheard first; once all were heard, repeats oldest-heard first with the same cooldown).</item>
    /// </list>
    /// </summary>
    [DisallowMultipleComponent]
    public class RoadChatterBubble : MonoBehaviour
    {
        public const string NoteName = "wm-road-chatter";
        public const string HeaderClass = "wm-road-chatter-header";
        public const string LineClass = "wm-road-chatter-line";

        /// <summary>When set, seeds the trip/pick rolls (reproducible PlayMode tests). Null in the game.</summary>
        public static int? SeedOverride { get; set; }

        private readonly RoadChatterSettings _settings = new RoadChatterSettings();

        private WorldMapHudController _hud;
        private UIDocument _hudDocument;
        private WorldMapController _worldMap;
        private WorldMapController _subscribedMap;
        private WorldEventCardController _eventCard;
        private System.Random _random;

        private bool _tripActive;
        private WorldMapRegionNode _tripTarget;
        private bool _pendingThisTrip;

        private VisualElement _note;
        private Label _headerLabel;
        private Label _firstLabel;
        private Label _secondLabel;
        private float _hideAt = -1f;

        // ------------------------------------------------------------------
        // Bootstrap
        // ------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            AttachToHud();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => AttachToHud();

        private static void AttachToHud()
        {
            WorldMapHudController hud = FindFirstObjectByType<WorldMapHudController>(FindObjectsInactive.Include);
            if (hud != null && hud.GetComponent<RoadChatterBubble>() == null)
            {
                hud.gameObject.AddComponent<RoadChatterBubble>();
            }
        }

        private void Awake()
        {
            _hud = GetComponent<WorldMapHudController>();
            _hudDocument = GetComponent<UIDocument>();
            int seed = SeedOverride ?? WorldMapController.SeedOverride ?? System.Environment.TickCount;
            _random = new System.Random(unchecked(seed * 31 + 7));
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
            if (_subscribedMap != null)
            {
                _subscribedMap.TravelProgressed -= OnTravelProgressed;
                _subscribedMap = null;
            }

            _note?.RemoveFromHierarchy();
            _note = null;
        }

        private void OnGameLoaded(GameLoadedEvent evt)
        {
            // A bag opened mid-trip (road event card) restores carried state: not a load, the trip goes on.
            if (evt.FromCarriedState)
            {
                return;
            }

            // A save loaded from the pause menu resets the token: this trip is gone.
            _tripActive = false;
            _pendingThisTrip = false;
            Hide();
        }

        private void Update()
        {
            EnsureSubscribed();

            if (_worldMap != null && !_worldMap.IsTraveling)
            {
                _tripActive = false;
                _pendingThisTrip = false;
            }

            if (_note != null && _hideAt > 0f && (Time.unscaledTime >= _hideAt || MustHide()))
            {
                Hide();
            }
        }

        private void EnsureSubscribed()
        {
            if (_worldMap == null)
            {
                _worldMap = FindFirstObjectByType<WorldMapController>();
            }

            if (_worldMap != null && _subscribedMap != _worldMap)
            {
                if (_subscribedMap != null)
                {
                    _subscribedMap.TravelProgressed -= OnTravelProgressed;
                }

                _worldMap.TravelProgressed += OnTravelProgressed;
                _subscribedMap = _worldMap;
            }

            if (_eventCard == null)
            {
                _eventCard = GetComponent<WorldEventCardController>();
            }
        }

        // ------------------------------------------------------------------
        // Trip rolls
        // ------------------------------------------------------------------

        private void OnTravelProgressed(float progress, BiomeType region)
        {
            if (!_tripActive || _tripTarget != _worldMap.TargetNode)
            {
                _tripActive = true;
                _tripTarget = _worldMap.TargetNode;
                _pendingThisTrip = RoadChatterRules.RollTrip(RoadChatterState.ResolveShared(), _settings, Allowed(), _random);
            }

            if (!_pendingThisTrip || progress < _settings.TriggerProgress)
            {
                return;
            }

            _pendingThisTrip = false;
            if (!Allowed())
            {
                return;
            }

            ServiceLocator.TryGet(out CharacterSelectionState selection);
            CharacterId? hero = selection != null ? selection.Selected : null;
            RoadChatterState state = RoadChatterState.ResolveShared();
            RoadChatterSpec spec = RoadChatterRules.Pick(RoadChatterCatalog.All, hero, CurrentStage(), NewGamePlusProgress.ResolveCurrentCycle(),
                state.HasSeen, _random, state.HeardRank);
            if (spec == null || !hero.HasValue)
            {
                return;
            }

            RoadChatterRules.MarkShown(state, spec, _settings);
            Show(spec, hero.Value);
        }

        private static RoadChatterStage CurrentStage()
        {
            if (!ServiceLocator.TryGet(out StoryProgress story) || story == null)
            {
                return RoadChatterStage.Forest;
            }

            return RoadChatterRules.StageFrom(story.HasSeen(StoryBeat.InterludeForest), story.HasSeen(StoryBeat.InterludeDesert),
                story.HasSeen(StoryBeat.InterludeMountain), story.HasSeen(StoryBeat.Ending));
        }

        private bool Allowed()
        {
            ServiceLocator.TryGet(out StoryProgress story);
            ServiceLocator.TryGet(out DialogueManager dialogue);
            ServiceLocator.TryGet(out CharacterSelectionState selection);
            return RoadChatterRules.CanShow(
                _worldMap != null && _worldMap.IsTraveling,
                selection != null && selection.Selected.HasValue,
                story == null || story.HasSeen(StoryBeat.Prologue),
                story != null && story.GetPendingBeat() != StoryBeat.None,
                _hud != null && _hud.IsVisible,
                GameplayPause.IsPaused,
                dialogue != null && dialogue.IsActive,
                _eventCard != null && _eventCard.IsOpen);
        }

        private bool MustHide()
        {
            ServiceLocator.TryGet(out DialogueManager dialogue);
            return GameplayPause.IsPaused || (dialogue != null && dialogue.IsActive) || (_eventCard != null && _eventCard.IsOpen)
                   || (_hud != null && !_hud.IsVisible);
        }

        // ------------------------------------------------------------------
        // Pure text helpers (tested)
        // ------------------------------------------------------------------

        /// <summary>"Radinho de pilha" / "Lembrança da estrada".</summary>
        public static string BuildHeader(RoadChatterSpec spec, LocalizationManager localization)
        {
            if (spec == null)
            {
                return string.Empty;
            }

            return RoadChatterTexts.Get(localization, spec.Kind == RoadChatterKind.Memory ? RoadChatterTexts.HeaderMemory : RoadChatterTexts.HeaderRadio);
        }

        /// <summary>
        /// Line <paramref name="index"/> (0/1) as "Ícaro: ..." for the active cousin, "Maithe (no rádio): ..." /
        /// "Maithe (lembrança): ..." for the other one.
        /// </summary>
        public static string BuildLine(RoadChatterSpec spec, int index, CharacterId active, LocalizationManager localization)
        {
            if (spec == null)
            {
                return string.Empty;
            }

            CharacterId speaker = spec.Speaker(index);
            string name = WorldMapHudPresenter.HeroName(localization, speaker);
            if (speaker != active)
            {
                name = RoadChatterTexts.Format(localization, spec.Kind == RoadChatterKind.Memory ? RoadChatterTexts.Remembered : RoadChatterTexts.OnTheRadio, name);
            }

            return RoadChatterTexts.Format(localization, RoadChatterTexts.Line, name, RoadChatterTexts.Get(localization, spec.LineKey(index)));
        }

        // ------------------------------------------------------------------
        // Bubble
        // ------------------------------------------------------------------

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        private void Show(RoadChatterSpec spec, CharacterId active)
        {
            if (!EnsureNote())
            {
                return;
            }

            LocalizationManager localization = Localization;
            _headerLabel.text = BuildHeader(spec, localization);
            _firstLabel.text = BuildLine(spec, 0, active, localization);
            _secondLabel.text = BuildLine(spec, 1, active, localization);
            _note.style.display = DisplayStyle.Flex;
            _hideAt = Time.unscaledTime + _settings.ShowSeconds;
        }

        private void Hide()
        {
            if (_note != null)
            {
                _note.style.display = DisplayStyle.None;
            }

            _hideAt = -1f;
        }

        private bool EnsureNote()
        {
            if (_hudDocument == null || _hudDocument.rootVisualElement == null)
            {
                return false;
            }

            VisualElement root = _hudDocument.rootVisualElement.Q<VisualElement>("wm-hud-root") ?? _hudDocument.rootVisualElement;
            if (_note != null && _note.parent == root)
            {
                return true;
            }

            _note?.RemoveFromHierarchy();

            // Themed like Caramelo's note (rap-panel under the HUD root's rap-theme), but near the top so the two never overlap.
            _note = new VisualElement { name = NoteName, pickingMode = PickingMode.Ignore };
            _note.AddToClassList("rap-panel");
            _note.style.position = Position.Absolute;
            _note.style.left = new Length(22f, LengthUnit.Percent);
            _note.style.right = new Length(22f, LengthUnit.Percent);
            _note.style.top = new Length(14f, LengthUnit.Percent);
            _note.style.display = DisplayStyle.None;

            _headerLabel = NewLabel(HeaderClass, TextAnchor.MiddleCenter);
            _headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _firstLabel = NewLabel(LineClass, TextAnchor.MiddleLeft);
            _secondLabel = NewLabel(LineClass, TextAnchor.MiddleLeft);
            _note.Add(_headerLabel);
            _note.Add(_firstLabel);
            _note.Add(_secondLabel);

            root.Add(_note);
            return true;
        }

        private static Label NewLabel(string className, TextAnchor anchor)
        {
            var label = new Label { pickingMode = PickingMode.Ignore };
            label.AddToClassList("wm-backpack-hint");
            label.AddToClassList(className);
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.unityTextAlign = anchor;
            return label;
        }
    }
}
