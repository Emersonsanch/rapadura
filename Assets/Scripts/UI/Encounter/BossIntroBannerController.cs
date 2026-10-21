using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
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
    /// combat input) and disappears after <see cref="_displaySeconds"/> of real time. With
    /// "Reduzir Flashes" on it appears/disappears without the fade.
    /// </summary>
    public class BossIntroBannerController : MonoBehaviour
    {
        public const float SortingOrder = 850f;
        private const float FadeSeconds = 0.35f;

        [Tooltip("Seconds the banner stays fully visible (real time).")]
        [SerializeField, Min(0.5f)] private float _displaySeconds = 2.5f;

        private static BossIntroBannerController _instance;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _banner;
        private Label _titleLabel;
        private Label _nameLabel;
        private IVisualElementScheduledItem _hideItem;

        /// <summary>The boss currently announced (null when hidden).</summary>
        public EnemyDefinition ShownBoss { get; private set; }

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
        }

        private void OnEnable()
        {
            _root = null;
            EventBus.Subscribe<BossEncounterIntroEvent>(OnBossIntro);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<BossEncounterIntroEvent>(OnBossIntro);
            Hide();
            _root = null;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnBossIntro(BossEncounterIntroEvent evt)
        {
            Show(evt.Boss);
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

            ServiceLocator.TryGet(out AccessibilitySettings accessibility);
            bool noMotion = accessibility != null && accessibility.ReduceFlashEffects;
            SetFade(noMotion ? 0f : FadeSeconds);

            _banner.style.display = DisplayStyle.Flex;
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

            _hideItem?.Pause();
            long visibleMs = (long)(Mathf.Max(0.5f, _displaySeconds) * 1000f);
            _hideItem = _banner.schedule.Execute(() => FadeOut(noMotion));
            _hideItem.ExecuteLater(visibleMs);
        }

        private void FadeOut(bool noMotion)
        {
            if (_banner == null)
            {
                return;
            }

            _banner.style.opacity = 0f;
            if (noMotion)
            {
                Hide();
                return;
            }

            _banner.schedule.Execute(Hide).ExecuteLater((long)(FadeSeconds * 1000f));
        }

        private void Hide()
        {
            ShownBoss = null;
            _hideItem?.Pause();
            _hideItem = null;

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
            _root.style.top = Length.Percent(18);
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
