using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.Common;
using Rapadura.UI.HUD;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Encounter
{
    /// <summary>
    /// Short "Chefe da região — Lobo Alfa" banner shown when an arena encounter with a boss starts
    /// (<see cref="BossEncounterIntroEvent"/>, raised by <see cref="EncounterArenaController"/>). The
    /// name goes through <see cref="ContentText"/> (<c>enemy.&lt;asset&gt;.name</c>), so it follows the
    /// game language. Music is not touched here: MusicDirector already switches to the Boss context
    /// on EncounterStartedEvent when the payload has a boss.
    ///
    /// Self-bootstrapping like <c>TutorialHintController</c>: one DontDestroyOnLoad host created after
    /// the first scene loads (no scene wiring), borrowing the PanelSettings of the HUD (or any other
    /// UIDocument). The view is built in code, has picking-mode Ignore everywhere (never blocks
    /// combat input) and disappears after <see cref="_displaySeconds"/> of game time. With
    /// "Reduzir Movimento" (or "Reduzir Flashes") on it appears/disappears without the fade. The look
    /// (theme palette, plate, borders, fonts, fade durations) lives in Resources/UI/EncounterOverlays.uss,
    /// attached at runtime by <see cref="EncounterOverlayStyles"/>; code only toggles display / opacity
    /// and the fade-speed classes (<see cref="QuickFadeClass"/>, <see cref="InstantFadeClass"/>).
    ///
    /// The same banner plays the encounter intro (<see cref="EncounterIntroEvent"/>) for normal
    /// encounters: "Prepare-se!" over a 2… 1… countdown matching the grace during which the enemies
    /// hold their aggro, then "Já!" briefly. Boss encounters keep the boss banner.
    ///
    /// Timing runs on scaled time (<see cref="Time.deltaTime"/>), like the enemies' aggro hold in
    /// EnemyController, so pausing mid-countdown can't let the number reach "Já!" while the enemies
    /// still wait (or the other way round). While <see cref="GameplayPause.IsPaused"/> the banner is
    /// hidden and its clock held: its document sorts above the HUD (<see cref="SortingOrder"/>), and
    /// the pause menu / modals sort lower, so it would otherwise draw over them.
    /// </summary>
    public class BossIntroBannerController : MonoBehaviour
    {
        public const float SortingOrder = 850f;
        // Must match .boss-intro-banner transition-duration in EncounterOverlays.uss (the hide waits for the fade).
        private const float FadeSeconds = 0.35f;

        public const string RootName = "boss-intro-root";
        public const string RootClass = "boss-intro-root";
        public const string BannerName = "boss-intro-banner";
        public const string BannerClass = "boss-intro-banner";
        public const string TitleName = "boss-intro-title";
        public const string NameLabelName = "boss-intro-name";
        /// <summary>Boss stinger: the active cousin's one-line quip under the name (<see cref="CutsceneScript.BossStingerKey"/>).</summary>
        public const string StingerName = "boss-intro-stinger";

        /// <summary>Extra seconds the banner stays up when it carries a stinger line.</summary>
        public const float StingerExtraSeconds = 1.5f;
        /// <summary>Half-length fade, used by the "Prepare-se!" countdown.</summary>
        public const string QuickFadeClass = "boss-intro-banner--quick";
        /// <summary>No fade (reduced motion / flashes).</summary>
        public const string InstantFadeClass = "boss-intro-banner--instant";

        [Tooltip("Seconds the banner stays fully visible (game time; held while paused).")]
        [SerializeField, Min(0.5f)] private float _displaySeconds = 2.5f;

        private static BossIntroBannerController _instance;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _banner;
        private Label _titleLabel;
        private Label _nameLabel;
        private Label _stingerLabel;
        private float _bossVisibleRemaining;
        private bool _fadingOut;
        private bool _noMotion;
        private bool _hiddenForPause;
        private IVisualElementScheduledItem _fadeHideItem;

        /// <summary>The boss currently announced (null when hidden).</summary>
        public EnemyDefinition ShownBoss { get; private set; }

        /// <summary>True while the "Prepare-se!" intro countdown is on screen.</summary>
        public bool IsShowingPrepare { get; private set; }

        /// <summary>True while either banner (countdown through "Já!", or the boss name) is up, fade-out included.</summary>
        public bool IsShowing => IsActive(IsShowingPrepare, ShownBoss != null);

        /// <summary>
        /// Whether the live banner instance is showing anything — <c>TutorialHintController</c> holds its
        /// hint back meanwhile ("Dica: Prepare-se para lutar!" overlapped the countdown).
        /// </summary>
        public static bool IsAnyShowing => _instance != null && _instance.IsShowing;

        /// <summary>Pure rule behind <see cref="IsShowing"/>.</summary>
        public static bool IsActive(bool showingPrepare, bool showingBoss) => showingPrepare || showingBoss;

        private const float GoDisplaySeconds = 0.6f;
        // Scaled seconds left of the grace countdown (negative down to -GoDisplaySeconds while "Já!" shows).
        private float _countdownRemaining;
        private int _lastCountdownValue;

        /// <summary>
        /// Number shown by the intro countdown with <paramref name="remainingSeconds"/> left: whole
        /// seconds rounded up ("2", "1"), 0 once the grace is over (the "Já!" frame).
        /// </summary>
        public static int CountdownValue(float remainingSeconds) => remainingSeconds <= 0f ? 0 : Mathf.CeilToInt(remainingSeconds);

        /// <summary>
        /// One frame of the banner clock (EditMode-testable): held while <paramref name="paused"/>,
        /// otherwise reduced by the scaled <paramref name="deltaTime"/> (timeScale 0 / hit-stop slow it
        /// exactly like the enemies' aggro hold).
        /// </summary>
        public static float AdvanceClock(float remainingSeconds, float deltaTime, bool paused) =>
            paused ? remainingSeconds : remainingSeconds - Mathf.Max(0f, deltaTime);

        /// <summary>Pure visibility rule: an active banner is hidden while gameplay is paused (pause menu / modals sort below it).</summary>
        public static bool ShouldDisplay(bool active, bool paused) => active && !paused;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null || FindFirstObjectByType<BossIntroBannerController>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(BossIntroBannerController));
            host.AddComponent<UIDocument>();
            host.AddComponent<BossIntroBannerController>();
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
            EventBus.Subscribe<BossEncounterIntroEvent>(OnBossIntro);
            EventBus.Subscribe<EncounterIntroEvent>(OnEncounterIntro);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<BossEncounterIntroEvent>(OnBossIntro);
            EventBus.Unsubscribe<EncounterIntroEvent>(OnEncounterIntro);
            Hide();
            _root = null;
        }

        private void OnDestroy()
        {
            Rapadura.Audio.UiClickSoundBinder.Unregister(_uiDocument);
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnBossIntro(BossEncounterIntroEvent evt)
        {
            Show(evt.Boss, evt.Payload != null ? evt.Payload.biome : (BiomeType?)null);
        }

        /// <summary>
        /// The stinger text for a boss of <paramref name="region"/>: "Maithe: “…”" for the active cousin, the narrator's
        /// line without a hero, empty without a region.
        /// </summary>
        public static string StingerText(LocalizationManager localization, BiomeType? region, CharacterId? hero)
        {
            if (!region.HasValue)
            {
                return string.Empty;
            }

            string line = CutsceneTexts.Get(localization, CutsceneScript.BossStingerKey(region.Value, hero));
            if (!hero.HasValue)
            {
                return line;
            }

            string name = CutsceneTexts.Get(localization, CutsceneScript.SpeakerNameKey(CutsceneScript.SpeakerOf(hero.Value)));
            return CutsceneTexts.Format(localization, CutsceneTexts.StingerFormat, name, line);
        }

        private void OnEncounterIntro(EncounterIntroEvent evt)
        {
            if (!evt.HasBoss)
            {
                ShowPrepare(evt.GraceSeconds);
            }
        }

        /// <summary>"Prepare-se!" + countdown for <paramref name="seconds"/> of game (scaled) time, then "Já!".</summary>
        public void ShowPrepare(float seconds)
        {
            if (seconds <= 0f || !EnsureBound())
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            _titleLabel.text = BossTexts.Get(localization, BossTexts.PrepareTitle);
            ShownBoss = null;
            IsShowingPrepare = true;
            _countdownRemaining = seconds;
            _lastCountdownValue = int.MinValue;
            UpdateCountdown(localization);

            ServiceLocator.TryGet(out AccessibilitySettings accessibility);
            _noMotion = accessibility != null && accessibility.SuppressUiMotion;
            SetFade(_noMotion ? FadeMode.Instant : FadeMode.Quick);
            BeginShowing();
            _banner.style.opacity = 1f;
            UiAccessibilityApplier.RequestRefresh();
        }

        private void Update()
        {
            if (_banner == null || (!IsShowingPrepare && ShownBoss == null))
            {
                return;
            }

            bool paused = GameplayPause.IsPaused;
            ApplyPauseVisibility(paused);

            if (paused || _fadingOut)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            if (IsShowingPrepare)
            {
                _countdownRemaining = AdvanceClock(_countdownRemaining, deltaTime, paused);
                ServiceLocator.TryGet(out LocalizationManager localization);
                UpdateCountdown(localization);

                if (_countdownRemaining <= -GoDisplaySeconds)
                {
                    FadeOut(_noMotion);
                }

                return;
            }

            _bossVisibleRemaining = AdvanceClock(_bossVisibleRemaining, deltaTime, paused);
            if (_bossVisibleRemaining <= 0f)
            {
                FadeOut(_noMotion);
            }
        }

        private void ApplyPauseVisibility(bool paused)
        {
            if (paused == _hiddenForPause)
            {
                return;
            }

            _hiddenForPause = paused;
            _banner.style.display = ShouldDisplay(true, paused) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>Shared start of both banners: visible now (Update hides it again if gameplay is paused).</summary>
        private void BeginShowing()
        {
            _fadingOut = false;
            _hiddenForPause = false;
            _fadeHideItem?.Pause();
            _fadeHideItem = null;
            _banner.style.display = DisplayStyle.Flex;
        }

        private void UpdateCountdown(LocalizationManager localization)
        {
            int value = CountdownValue(_countdownRemaining);
            if (value == _lastCountdownValue)
            {
                return;
            }

            _lastCountdownValue = value;
            _stingerLabel.style.display = DisplayStyle.None;
            _nameLabel.text = value > 0 ? value.ToString(System.Globalization.CultureInfo.InvariantCulture) : BossTexts.Get(localization, BossTexts.PrepareGo);
        }

        /// <summary>Announces <paramref name="boss"/> (no-op for null or when no panel can be borrowed yet).</summary>
        public void Show(EnemyDefinition boss) => Show(boss, null);

        /// <summary>Announces <paramref name="boss"/>, with the <paramref name="region"/>'s stinger line when given.</summary>
        public void Show(EnemyDefinition boss, BiomeType? region)
        {
            if (boss == null || !EnsureBound())
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            _titleLabel.text = BossTexts.Get(localization, BossTexts.BannerTitle);
            _nameLabel.text = boss.LocalizedName(localization);
            ShownBoss = boss;
            IsShowingPrepare = false;
            _bossVisibleRemaining = Mathf.Max(0.5f, _displaySeconds);

            ServiceLocator.TryGet(out CharacterSelectionState selection);
            string stinger = StingerText(localization, region, selection != null ? selection.Selected : null);
            _stingerLabel.text = stinger;
            _stingerLabel.style.display = string.IsNullOrEmpty(stinger) ? DisplayStyle.None : DisplayStyle.Flex;
            if (!string.IsNullOrEmpty(stinger))
            {
                _bossVisibleRemaining += StingerExtraSeconds;
            }

            ServiceLocator.TryGet(out AccessibilitySettings accessibility);
            bool noMotion = accessibility != null && accessibility.SuppressUiMotion;
            _noMotion = noMotion;
            SetFade(noMotion ? FadeMode.Instant : FadeMode.Normal);

            BeginShowing();
            if (noMotion)
            {
                _banner.style.opacity = 1f;
            }
            else
            {
                // Next frame, so the opacity transition runs from the hidden state.
                _banner.style.opacity = 0f;
                _banner.schedule.Execute(() => _banner.style.opacity = 1f);
            }

            UiAccessibilityApplier.RequestRefresh();
        }

        private void FadeOut(bool noMotion)
        {
            if (_banner == null)
            {
                return;
            }

            _fadingOut = true;
            _banner.style.opacity = 0f;
            if (noMotion)
            {
                Hide();
                return;
            }

            // The fade itself is cosmetic (real time is fine); while paused Update keeps it hidden.
            _fadeHideItem?.Pause();
            _fadeHideItem = _banner.schedule.Execute(Hide);
            _fadeHideItem.ExecuteLater((long)(FadeSeconds * 1000f));
        }

        private void Hide()
        {
            ShownBoss = null;
            IsShowingPrepare = false;
            _fadingOut = false;
            _hiddenForPause = false;
            _fadeHideItem?.Pause();
            _fadeHideItem = null;

            if (_banner != null)
            {
                _banner.style.display = DisplayStyle.None;
                _banner.style.opacity = 0f;
            }
        }

        private enum FadeMode { Normal, Quick, Instant }

        /// <summary>Picks the fade length through USS classes (the durations are in EncounterOverlays.uss).</summary>
        private void SetFade(FadeMode mode)
        {
            _banner.EnableInClassList(QuickFadeClass, mode == FadeMode.Quick);
            _banner.EnableInClassList(InstantFadeClass, mode == FadeMode.Instant);
            UiMotionPreferences.ApplyTo(_root, mode == FadeMode.Instant);
        }

        /// <summary>Gives the document a panel (borrowed PanelSettings) and builds the view once. False until it can render.</summary>
        private bool EnsureBound()
        {
            if (_uiDocument == null)
            {
                return false;
            }

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

            if (_root != null && _root.parent == documentRoot)
            {
                return true;
            }

            documentRoot.pickingMode = PickingMode.Ignore;
            BuildView(documentRoot);
            return true;
        }

        private void BuildView(VisualElement documentRoot)
        {
            _root = CreateView(UiMotionPreferences.IsReducedMotion());
            _banner = _root.Q<VisualElement>(BannerName);
            _titleLabel = _root.Q<Label>(TitleName);
            _nameLabel = _root.Q<Label>(NameLabelName);
            _stingerLabel = _root.Q<Label>(StingerName);
            documentRoot.Add(_root);
        }

        /// <summary>
        /// The banner tree (root → plate → title + name), hidden, picking-mode Ignore throughout, themed via
        /// <see cref="EncounterOverlayStyles"/>. No inline look: everything visual is a USS class. EditMode-testable.
        /// </summary>
        public static VisualElement CreateView(bool reducedMotion)
        {
            var root = new VisualElement { name = RootName, pickingMode = PickingMode.Ignore };
            root.AddToClassList(RootClass);
            EncounterOverlayStyles.ApplyTo(root, reducedMotion);

            var banner = new VisualElement { name = BannerName, pickingMode = PickingMode.Ignore };
            banner.AddToClassList(BannerClass);
            banner.style.display = DisplayStyle.None;
            banner.style.opacity = 0f;

            var title = new Label { name = TitleName, pickingMode = PickingMode.Ignore };
            title.AddToClassList(TitleName);

            var nameLabel = new Label { name = NameLabelName, pickingMode = PickingMode.Ignore };
            nameLabel.AddToClassList(NameLabelName);

            var stinger = new Label { name = StingerName, pickingMode = PickingMode.Ignore };
            stinger.AddToClassList(StingerName);
            stinger.style.display = DisplayStyle.None;

            banner.Add(title);
            banner.Add(nameLabel);
            banner.Add(stinger);
            root.Add(banner);
            return root;
        }

        private PanelSettings FindPanelSettings()
        {
            HudController hud = FindFirstObjectByType<HudController>();
            UIDocument hudDocument = hud != null ? hud.GetComponent<UIDocument>() : null;
            if (hudDocument != null && hudDocument.panelSettings != null)
            {
                return hudDocument.panelSettings;
            }

            foreach (UIDocument document in FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (document != _uiDocument && document.panelSettings != null)
                {
                    return document.panelSettings;
                }
            }

            return null;
        }
    }
}
