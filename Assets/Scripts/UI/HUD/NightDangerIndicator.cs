using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Night;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.UIElements;
using UIElements = UnityEngine.UIElements;

namespace Rapadura.UI.HUD
{
    /// <summary>
    /// Small HUD pill "Noite — os bichos estão mais bravos" shown while the clock is in <see cref="TimeOfDayPhase.Night"/>
    /// (when <see cref="NightDangerRules"/> sends more, bigger, faster waves and night variants). Self-bootstrapping like
    /// <c>BagFullToastController</c>: one DontDestroyOnLoad host, PanelSettings borrowed from the HUD, view built in
    /// code with picking-mode Ignore (never blocks input). Visible only with a gameplay HUD on screen (the combat
    /// <see cref="HudController"/> or a visible <see cref="WorldMapHudController"/>) and not paused. Polls a few times
    /// per second in real time; the text follows the language.
    /// </summary>
    public class NightDangerIndicator : MonoBehaviour
    {
        public const float SortingOrder = 600f;
        public const string RootName = "night-danger-root";
        public const string PillName = "night-danger-pill";
        public const string LabelName = "night-danger-label";
        private const float RefreshInterval = 0.25f;

        private static NightDangerIndicator _instance;

        private UIElements.UIDocument _uiDocument;
        private UIElements.VisualElement _root;
        private UIElements.VisualElement _pill;
        private UIElements.Label _label;
        private float _timer = RefreshInterval;

        public bool IsShowing => _pill != null && _pill.style.display == UIElements.DisplayStyle.Flex;

        /// <summary>Pure visibility rule (tested): night, a gameplay HUD on screen, not paused.</summary>
        public static bool ShouldShow(TimeOfDayPhase phase, bool gameplayHudVisible, bool paused)
        {
            return NightDangerRules.IsNight(phase) && gameplayHudVisible && !paused;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null || FindFirstObjectByType<NightDangerIndicator>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(NightDangerIndicator));
            host.AddComponent<UIElements.UIDocument>();
            host.AddComponent<NightDangerIndicator>();
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

            _uiDocument = GetComponent<UIElements.UIDocument>();
            if (_uiDocument == null)
            {
                _uiDocument = gameObject.AddComponent<UIElements.UIDocument>();
            }

            _uiDocument.sortingOrder = SortingOrder;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnDisable()
        {
            _root = null;
            _pill = null;
            _label = null;
        }

        private void Update()
        {
            _timer += Time.unscaledDeltaTime;
            if (_timer < RefreshInterval)
            {
                return;
            }

            _timer = 0f;
            TimeOfDayPhase phase = ServiceLocator.TryGet(out TimeOfDayManager time) && time != null ? time.Phase : TimeOfDayPhase.Day;
            bool show = ShouldShow(phase, GameplayHudVisible(), GameplayPause.IsPaused);
            if (!show)
            {
                if (_pill != null)
                {
                    _pill.style.display = UIElements.DisplayStyle.None;
                }

                return;
            }

            if (!EnsureBound())
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            _label.text = NightTexts.Get(localization, NightTexts.HudNight);
            _pill.style.display = UIElements.DisplayStyle.Flex;
        }

        private static bool GameplayHudVisible()
        {
            HudController hud = FindFirstObjectByType<HudController>();
            if (hud != null && hud.isActiveAndEnabled)
            {
                return true;
            }

            WorldMapHudController mapHud = FindFirstObjectByType<WorldMapHudController>();
            return mapHud != null && mapHud.isActiveAndEnabled && mapHud.IsVisible;
        }

        private bool EnsureBound()
        {
            if (_uiDocument == null)
            {
                return false;
            }

            if (_uiDocument.panelSettings == null)
            {
                UIElements.PanelSettings donor = FindPanelSettings();
                if (donor == null)
                {
                    return false;
                }

                _uiDocument.panelSettings = donor;
                _uiDocument.sortingOrder = SortingOrder;
            }

            UIElements.VisualElement documentRoot = _uiDocument.rootVisualElement;
            if (documentRoot == null)
            {
                return false;
            }

            if (_root != null && _root.parent == documentRoot)
            {
                return true;
            }

            documentRoot.pickingMode = UIElements.PickingMode.Ignore;
            _root = CreateView();
            _pill = _root.Q<UIElements.VisualElement>(PillName);
            _label = _root.Q<UIElements.Label>(LabelName);
            documentRoot.Add(_root);
            return true;
        }

        /// <summary>The indicator tree (root → pill → label), hidden, picking-mode Ignore throughout. EditMode-testable.</summary>
        public static UIElements.VisualElement CreateView()
        {
            var root = new UIElements.VisualElement { name = RootName, pickingMode = UIElements.PickingMode.Ignore };
            root.style.position = UIElements.Position.Absolute;
            root.style.left = 0;
            root.style.right = 0;
            root.style.top = 64;
            root.style.alignItems = UIElements.Align.Center;

            var pill = new UIElements.VisualElement { name = PillName, pickingMode = UIElements.PickingMode.Ignore };
            pill.style.display = UIElements.DisplayStyle.None;
            pill.style.backgroundColor = new Color(0.08f, 0.1f, 0.22f, 0.82f);
            pill.style.paddingLeft = 14;
            pill.style.paddingRight = 14;
            pill.style.paddingTop = 5;
            pill.style.paddingBottom = 5;
            pill.style.borderTopLeftRadius = 14;
            pill.style.borderTopRightRadius = 14;
            pill.style.borderBottomLeftRadius = 14;
            pill.style.borderBottomRightRadius = 14;
            pill.style.borderLeftWidth = 2;
            pill.style.borderRightWidth = 2;
            pill.style.borderTopWidth = 2;
            pill.style.borderBottomWidth = 2;
            Color border = new Color(1f, 0.85f, 0.4f, 0.9f);
            pill.style.borderLeftColor = border;
            pill.style.borderRightColor = border;
            pill.style.borderTopColor = border;
            pill.style.borderBottomColor = border;

            var label = new UIElements.Label { name = LabelName, pickingMode = UIElements.PickingMode.Ignore };
            label.style.color = new Color(1f, 0.93f, 0.7f, 1f);
            label.style.fontSize = 18;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            pill.Add(label);
            root.Add(pill);
            return root;
        }

        private UIElements.PanelSettings FindPanelSettings()
        {
            HudController hud = FindFirstObjectByType<HudController>();
            UIElements.UIDocument hudDocument = hud != null ? hud.GetComponent<UIElements.UIDocument>() : null;
            if (hudDocument != null && hudDocument.panelSettings != null)
            {
                return hudDocument.panelSettings;
            }

            foreach (UIElements.UIDocument document in FindObjectsByType<UIElements.UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None))
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
