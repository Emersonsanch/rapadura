using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Tutorial;
using Rapadura.UI.Common;
using Rapadura.UI.HUD;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Tutorial
{
    /// <summary>
    /// Shows <see cref="TutorialManager.CurrentHint"/> as a small non-blocking bubble
    /// (Resources/UI/TutorialHintView.uxml/.uss) with an "Entendi!" button, plus an optional static
    /// outline around the HUD element the hint talks about (<see cref="TutorialStepDefinition.HighlightElement"/>,
    /// looked up by name in the scene's <see cref="HudController"/> document). Everything but the
    /// button has picking-mode Ignore, so the game stays playable under it. A step flagged
    /// <see cref="TutorialStepDefinition.PausesGameplay"/> would hold a <see cref="GameplayPause"/>
    /// until acknowledged (no built-in step uses it); every other hint acknowledges itself after
    /// <c>_autoDismissSeconds</c> of real time.
    ///
    /// Self-bootstrapping like <see cref="UiAccessibilityApplier"/>: one DontDestroyOnLoad host is
    /// created after the first scene loads (no scene wiring), borrows the PanelSettings of any other
    /// UIDocument in the scene and draws above it (<see cref="SortingOrder"/>). Because it is a regular
    /// active UIDocument, UiAccessibilityApplier scales its fonts / applies high contrast like any
    /// screen. With "Reduzir Flashes" on, the bubble appears without fade/slide; the highlight is
    /// never animated in any mode. The hint queue lives in the manager, so a hint survives the
    /// WorldMap ⇄ Main scene swap.
    /// </summary>
    public class TutorialHintController : MonoBehaviour
    {
        public const float SortingOrder = 900f;
        private const string ViewResourcePath = "UI/TutorialHintView";
        private const string VisibleClass = "tutorial-hint--visible";
        private const string NoMotionClass = "tutorial-hint--no-motion";
        private const float HighlightPadding = 6f;

        [Tooltip("Seconds a non-pausing hint stays up before it counts as read (real time). 0 = until \"Entendi\".")]
        [SerializeField, Min(0f)] private float _autoDismissSeconds = 20f;

        private static TutorialHintController _instance;

        private readonly UiDocumentLocalizer _localizer = new UiDocumentLocalizer();
        private UIDocument _uiDocument;
        private VisualTreeAsset _viewAsset;
        private VisualElement _root;
        private VisualElement _hint;
        private VisualElement _highlight;
        private Label _titleLabel;
        private Label _bodyLabel;
        private Button _okButton;

        private TutorialManager _manager;
        private LocalizationManager _localization;
        private AccessibilitySettings _accessibility;
        private UIDocument _hudDocument;
        private TutorialStepDefinition _shown;
        private float _shownAt;
        private bool _dirty = true;

        /// <summary>The step on screen right now (null when hidden).</summary>
        public TutorialStepDefinition ShownStep => _shown;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null || FindFirstObjectByType<TutorialHintController>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(TutorialHintController));
            host.AddComponent<UIDocument>();
            host.AddComponent<TutorialHintController>();
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
            _dirty = true;
        }

        private void OnDisable()
        {
            Hide();
            SetManager(null);
            _localizer.Detach();
            _root = null;
        }

        private void OnDestroy()
        {
            GameplayPause.Release(this);
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void Update()
        {
            ServiceLocator.TryGet(out TutorialManager manager);
            if (manager != _manager)
            {
                SetManager(manager);
            }

            if (!EnsureBound())
            {
                return;
            }

            if (_dirty)
            {
                _dirty = false;
                Render();
            }

            if (_shown == null)
            {
                return;
            }

            if (!_shown.PausesGameplay && _autoDismissSeconds > 0f && Time.unscaledTime - _shownAt >= _autoDismissSeconds)
            {
                Acknowledge();
                return;
            }

            UpdateHighlight();
        }

        private void SetManager(TutorialManager manager)
        {
            if (_manager != null)
            {
                _manager.CurrentHintChanged -= OnHintChanged;
            }

            _manager = manager;

            if (_manager != null)
            {
                _manager.CurrentHintChanged += OnHintChanged;
            }

            _dirty = true;
        }

        private void OnHintChanged() => _dirty = true;

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

            if (_root != null && _root.parent == documentRoot)
            {
                return true;
            }

            if (_viewAsset == null)
            {
                _viewAsset = Resources.Load<VisualTreeAsset>(ViewResourcePath);
                if (_viewAsset == null)
                {
                    Debug.LogWarning($"[TutorialHintController] Missing Resources/{ViewResourcePath}.uxml; hints disabled.");
                    enabled = false;
                    return false;
                }
            }

            documentRoot.pickingMode = PickingMode.Ignore;
            _root = documentRoot.Q<VisualElement>("tutorial-hint-root");
            if (_root == null)
            {
                _viewAsset.CloneTree(documentRoot);
                _root = documentRoot.Q<VisualElement>("tutorial-hint-root");
            }

            if (_root == null)
            {
                return false;
            }

            // CloneTree wraps the view in a TemplateContainer: it must not catch input either.
            for (VisualElement element = _root; element != null && element != documentRoot; element = element.parent)
            {
                element.pickingMode = PickingMode.Ignore;
            }

            _hint = _root.Q<VisualElement>("tutorial-hint");
            _highlight = _root.Q<VisualElement>("tutorial-highlight");
            _titleLabel = _root.Q<Label>("tutorial-hint-title");
            _bodyLabel = _root.Q<Label>("tutorial-hint-body");
            _okButton = _root.Q<Button>("tutorial-hint-ok");
            if (_okButton != null)
            {
                _okButton.clicked -= Acknowledge;
                _okButton.clicked += Acknowledge;
            }

            _localizer.Attach(_root, () => _dirty = true, TutorialTexts.Get);
            _dirty = true;
            return true;
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

        private void Render()
        {
            TutorialStepDefinition hint = _manager != null ? _manager.CurrentHint : null;
            if (hint == null)
            {
                Hide();
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

            if (_titleLabel != null) _titleLabel.text = TutorialTexts.Get(_localization, hint.TitleKey);
            if (_bodyLabel != null) _bodyLabel.text = TutorialTexts.Get(_localization, hint.BodyKey);

            bool isNew = hint != _shown;
            _shown = hint;
            if (isNew)
            {
                _shownAt = Time.unscaledTime;
            }

            if (_accessibility == null)
            {
                ServiceLocator.TryGet(out _accessibility);
            }

            bool noMotion = _accessibility != null && _accessibility.ReduceFlashEffects;
            if (_hint != null)
            {
                _hint.EnableInClassList(NoMotionClass, noMotion);
                _hint.style.display = DisplayStyle.Flex;
                if (noMotion)
                {
                    _hint.AddToClassList(VisibleClass);
                }
                else
                {
                    // Next frame, so the opacity/translate transition runs from the hidden state.
                    _hint.schedule.Execute(() => _hint.AddToClassList(VisibleClass));
                }
            }

            if (hint.PausesGameplay)
            {
                GameplayPause.Acquire(this);
                _okButton?.Focus();
            }
            else
            {
                GameplayPause.Release(this);
            }

            UiAccessibilityApplier.RequestRefresh();
            UpdateHighlight();
        }

        private void Hide()
        {
            _shown = null;
            GameplayPause.Release(this);

            if (_hint != null)
            {
                _hint.RemoveFromClassList(VisibleClass);
                _hint.style.display = DisplayStyle.None;
            }

            if (_highlight != null)
            {
                _highlight.style.display = DisplayStyle.None;
            }
        }

        private void Acknowledge()
        {
            TutorialStepDefinition shown = _shown;
            if (shown == null)
            {
                return;
            }

            Hide();
            _manager?.Acknowledge(shown.Id);
            _dirty = true;
        }

        private void UpdateHighlight()
        {
            if (_highlight == null)
            {
                return;
            }

            VisualElement target = FindHighlightTarget(_shown != null ? _shown.HighlightElement : null);
            if (target == null)
            {
                _highlight.style.display = DisplayStyle.None;
                return;
            }

            Rect bounds = target.worldBound;
            Vector2 topLeft = _root.WorldToLocal(bounds.position);
            _highlight.style.left = topLeft.x - HighlightPadding;
            _highlight.style.top = topLeft.y - HighlightPadding;
            _highlight.style.width = bounds.width + HighlightPadding * 2f;
            _highlight.style.height = bounds.height + HighlightPadding * 2f;
            _highlight.style.display = DisplayStyle.Flex;
        }

        private VisualElement FindHighlightTarget(string elementName)
        {
            if (string.IsNullOrEmpty(elementName))
            {
                return null;
            }

            if (_hudDocument == null)
            {
                HudController hud = FindFirstObjectByType<HudController>();
                _hudDocument = hud != null ? hud.GetComponent<UIDocument>() : null;
            }

            if (_hudDocument == null || !_hudDocument.isActiveAndEnabled || _hudDocument.rootVisualElement == null)
            {
                return null;
            }

            VisualElement target = _hudDocument.rootVisualElement.Q<VisualElement>(elementName);
            if (target == null || target.panel == null || target.resolvedStyle.display == DisplayStyle.None ||
                target.worldBound.width <= 0f || float.IsNaN(target.worldBound.width))
            {
                return null;
            }

            return target;
        }
    }
}
