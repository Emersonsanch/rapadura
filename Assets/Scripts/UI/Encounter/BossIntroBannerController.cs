using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Enemies;
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
    /// "Reduzir Flashes" on it appears/disappears without the fade.
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
        private const float FadeSeconds = 0.35f;

        [Tooltip("Seconds the banner stays fully visible (game time; held while paused).")]
        [SerializeField, Min(0.5f)] private float _displaySeconds = 2.5f;

        private static BossIntroBannerController _instance;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _banner;
        private Label _titleLabel;
        private Label _nameLabel;
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
            Show(evt.Boss);
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
            _noMotion = accessibility != null && accessibility.ReduceFlashEffects;
            SetFade(_noMotion ? 0f : FadeSeconds * 0.5f);
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
            _nameLabel.text = value > 0 ? value.ToString(System.Globalization.CultureInfo.InvariantCulture) : BossTexts.Get(localization, BossTexts.PrepareGo);
        }

        /// <summary>Announces <paramref name="boss"/> (no-op for null or when no panel can be borrowed yet).</summary>
        public void Show(EnemyDefinition boss)
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

            ServiceLocator.TryGet(out AccessibilitySettings accessibility);
            bool noMotion = accessibility != null && accessibility.ReduceFlashEffects;
            _noMotion = noMotion;
            SetFade(noMotion ? 0f : FadeSeconds);

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

        private void SetFade(float seconds)
        {
            _banner.style.transitionProperty = new StyleList<StylePropertyName>(new System.Collections.Generic.List<StylePropertyName> { new StylePropertyName("opacity") });
            _banner.style.transitionDuration = new StyleList<TimeValue>(new System.Collections.Generic.List<TimeValue> { new TimeValue(seconds, TimeUnit.Second) });
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
            _root = new VisualElement { name = "boss-intro-root", pickingMode = PickingMode.Ignore };
            _root.style.position = Position.Absolute;
            _root.style.left = 0;
            _root.style.right = 0;
            // Below the compact tutorial hint strip at the top of the screen.
            _root.style.top = Length.Percent(26);
            _root.style.alignItems = Align.Center;

            _banner = new VisualElement { name = "boss-intro-banner", pickingMode = PickingMode.Ignore };
            _banner.AddToClassList("boss-intro-banner");
            _banner.style.paddingLeft = 32;
            _banner.style.paddingRight = 32;
            _banner.style.paddingTop = 10;
            _banner.style.paddingBottom = 12;
            _banner.style.maxWidth = Length.Percent(90);
            _banner.style.alignItems = Align.Center;
            _banner.style.backgroundColor = new Color(0.08f, 0.03f, 0.02f, 0.82f);
            Color border = new Color(0.85f, 0.62f, 0.22f, 1f);
            _banner.style.borderTopWidth = 2;
            _banner.style.borderBottomWidth = 2;
            _banner.style.borderTopColor = border;
            _banner.style.borderBottomColor = border;
            _banner.style.display = DisplayStyle.None;
            _banner.style.opacity = 0f;

            _titleLabel = new Label { name = "boss-intro-title", pickingMode = PickingMode.Ignore };
            _titleLabel.style.fontSize = 18;
            _titleLabel.style.color = border;
            _titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            _nameLabel = new Label { name = "boss-intro-name", pickingMode = PickingMode.Ignore };
            _nameLabel.style.fontSize = 36;
            _nameLabel.style.color = new Color(0.98f, 0.94f, 0.86f, 1f);
            _nameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _nameLabel.style.whiteSpace = WhiteSpace.Normal;

            _banner.Add(_titleLabel);
            _banner.Add(_nameLabel);
            _root.Add(_banner);
            documentRoot.Add(_root);
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
