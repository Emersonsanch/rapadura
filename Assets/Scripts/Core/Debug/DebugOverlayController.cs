using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.Core.Debug
{
    /// <summary>
    /// MonoBehaviour/UI Toolkit binding for the Fase 10 debug overlay. Displays smoothed FPS and
    /// current managed-heap memory usage, refreshed a few times a second (not every frame, to
    /// avoid layout-rebuild cost defeating the point of a *performance* overlay — see the mobile
    /// battery/thermal note in TODO.md Fase 0).
    ///
    /// All FPS smoothing/formatting logic lives in <see cref="DebugOverlayModel"/> so it can be
    /// unit-tested without a UIDocument. This class only wires that model to UI Toolkit elements
    /// looked up from <c>Assets/UI/Debug/DebugOverlayView.uxml</c> by name.
    ///
    /// See <see cref="DebugOverlayModel"/>'s doc comment for why the "rolling log of the last N
    /// GameLogger messages" part of the original ask is NOT implemented here: GameLogger has no
    /// new-message hook and was left unedited by design.
    ///
    /// Not registered anywhere automatically — attach this to a GameObject with a
    /// <see cref="UIDocument"/> pointing at DebugOverlayView.uxml (Editor work, out of scope for
    /// this change since no Scene/Prefab exists yet; see TODO.md's Unity Editor blocker note).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class DebugOverlayController : MonoBehaviour
    {
        private const float RefreshIntervalSeconds = 0.25f;

        [SerializeField] private bool _startVisible = true;

        private readonly DebugOverlayModel _model = new DebugOverlayModel();

        private UIDocument _document;
        private Label _fpsLabel;
        private Label _memoryLabel;
        private VisualElement _root;
        private float _timeSinceRefresh;

        public DebugOverlayModel Model => _model;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            _model.Reset();
            _timeSinceRefresh = 0f;

            if (_document != null && _document.rootVisualElement != null)
            {
                _root = _document.rootVisualElement.Q<VisualElement>("debug-overlay-root");
                _fpsLabel = _document.rootVisualElement.Q<Label>("debug-overlay-fps-label");
                _memoryLabel = _document.rootVisualElement.Q<Label>("debug-overlay-memory-label");
                SetVisible(_startVisible);
            }
        }

        private void Update()
        {
            _model.RegisterFrame(Time.unscaledDeltaTime);
            _timeSinceRefresh += Time.unscaledDeltaTime;

            if (_timeSinceRefresh < RefreshIntervalSeconds)
            {
                return;
            }

            _timeSinceRefresh = 0f;
            RefreshLabels();
        }

        private void RefreshLabels()
        {
            if (_fpsLabel != null)
            {
                _fpsLabel.text = _model.FormatFps();
            }

            if (_memoryLabel != null)
            {
                _memoryLabel.text = DebugOverlayModel.FormatMemory(System.GC.GetTotalMemory(forceFullCollection: false));
            }
        }

        /// <summary>Toggles overlay visibility, e.g. bound to a debug hotkey elsewhere.</summary>
        public void SetVisible(bool visible)
        {
            if (_root != null)
            {
                _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public void Toggle()
        {
            if (_root == null)
            {
                return;
            }

            bool currentlyVisible = _root.style.display != DisplayStyle.None;
            SetVisible(!currentlyVisible);
        }
    }
}
