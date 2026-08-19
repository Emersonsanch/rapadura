using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Common
{
    /// <summary>
    /// Generic, reusable tooltip overlay for UI Toolkit screens, following the same pattern as
    /// <c>HudController</c>/<c>SkillTreeController</c>: a single <see cref="UIDocument"/>, elements
    /// resolved once, pure math (<see cref="TooltipPositioning"/>) factored out for testability.
    ///
    /// Any Menu/HUD controller can drop this component next to its own <see cref="UIDocument"/> and
    /// call <see cref="Show(string, VisualElement)"/>/<see cref="Hide"/> — e.g. from a button's
    /// <c>PointerEnterEvent</c>/<c>FocusInEvent</c> handler — instead of building a bespoke tooltip
    /// per screen. <see cref="TooltipView.uxml"/>/<c>.uss</c> provide the default look; assign a
    /// different <see cref="VisualTreeAsset"/> to restyle without touching this script.
    ///
    /// Two modes:
    /// - Called with <c>anchorElement == null</c>: the tooltip follows the mouse cursor (typical for
    ///   pointer hover), repositioned on every <see cref="MouseMoveEvent"/>.
    /// - Called with an <paramref name="anchorElement"/>: the tooltip anchors just below/above that
    ///   element (typical for keyboard/gamepad focus, where there is no mouse position to follow —
    ///   see TODO.md Fase 6 "Navegação 100% por controle/teclado").
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class TooltipController : MonoBehaviour
    {
        [Header("Template")]
        [Tooltip("TooltipView.uxml. If left empty, a minimal VisualElement + Label is built in code.")]
        [SerializeField] private VisualTreeAsset _tooltipViewAsset;

        [Header("Placement")]
        [SerializeField] private Vector2 _cursorOffset = new Vector2(16f, 16f);
        [SerializeField] private float _anchorSpacing = 8f;

        private UIDocument _uiDocument;
        private VisualElement _panelRoot;
        private VisualElement _tooltipRoot;
        private Label _label;

        private VisualElement _anchorElement;
        private bool _isVisible;

        public bool IsVisible => _isVisible;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            BuildTooltipElement();
        }

        private void OnDisable()
        {
            UnregisterMouseTracking();

            if (_tooltipRoot != null && _panelRoot != null)
            {
                _panelRoot.Remove(_tooltipRoot);
            }

            _tooltipRoot = null;
            _label = null;
        }

        private void BuildTooltipElement()
        {
            _panelRoot = _uiDocument.rootVisualElement;

            if (_panelRoot == null)
            {
                return;
            }

            if (_tooltipViewAsset != null)
            {
                VisualElement instance = _tooltipViewAsset.Instantiate();
                // TemplateContainer wraps the uxml root; unwrap so "tooltip-root" is the element we style/move.
                _tooltipRoot = instance.Q<VisualElement>("tooltip-root") ?? instance;
            }
            else
            {
                _tooltipRoot = new VisualElement { name = "tooltip-root" };
                _tooltipRoot.AddToClassList("tooltip-root");
            }

            _label = _tooltipRoot.Q<Label>("tooltip-label");

            if (_label == null)
            {
                _label = new Label { name = "tooltip-label" };
                _label.AddToClassList("tooltip-label");
                _tooltipRoot.Add(_label);
            }

            _tooltipRoot.style.position = Position.Absolute;
            _tooltipRoot.pickingMode = PickingMode.Ignore;

            _panelRoot.Add(_tooltipRoot);
            ApplyVisibility(false);
        }

        /// <summary>Shows the tooltip with <paramref name="text"/>. See class docs for anchor vs mouse-follow modes.</summary>
        public void Show(string text, VisualElement anchorElement = null)
        {
            if (_tooltipRoot == null || _label == null)
            {
                return;
            }

            _label.text = text;
            _anchorElement = anchorElement;
            ApplyVisibility(true);

            if (_anchorElement != null)
            {
                UnregisterMouseTracking();
                RepositionToAnchor();
            }
            else
            {
                RegisterMouseTracking();
            }
        }

        public void Hide()
        {
            _anchorElement = null;
            UnregisterMouseTracking();
            ApplyVisibility(false);
        }

        private void ApplyVisibility(bool visible)
        {
            _isVisible = visible;

            if (_tooltipRoot == null)
            {
                return;
            }

            if (visible)
            {
                _tooltipRoot.AddToClassList("tooltip-root--visible");
            }
            else
            {
                _tooltipRoot.RemoveFromClassList("tooltip-root--visible");
            }
        }

        private void RegisterMouseTracking()
        {
            _panelRoot?.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        private void UnregisterMouseTracking()
        {
            _panelRoot?.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!_isVisible || _anchorElement != null)
            {
                return;
            }

            RepositionToMouse(evt.mousePosition);
        }

        private void RepositionToMouse(Vector2 mousePosition)
        {
            Vector2 panelSize = GetPanelSize();
            Vector2 tooltipSize = GetTooltipSize();

            Vector2 position = TooltipPositioning.ComputeFollowMousePosition(mousePosition, tooltipSize, panelSize, _cursorOffset);
            ApplyPosition(position);
        }

        private void RepositionToAnchor()
        {
            if (_anchorElement == null)
            {
                return;
            }

            Vector2 panelSize = GetPanelSize();
            Vector2 tooltipSize = GetTooltipSize();

            Vector2 position = TooltipPositioning.ComputeAnchoredPosition(_anchorElement.worldBound, tooltipSize, panelSize, _anchorSpacing);
            ApplyPosition(position);
        }

        private void ApplyPosition(Vector2 position)
        {
            if (_tooltipRoot == null)
            {
                return;
            }

            _tooltipRoot.style.left = position.x;
            _tooltipRoot.style.top = position.y;
        }

        private Vector2 GetPanelSize()
        {
            if (_panelRoot == null)
            {
                return Vector2.zero;
            }

            Rect layout = _panelRoot.layout;
            return new Vector2(layout.width, layout.height);
        }

        private Vector2 GetTooltipSize()
        {
            if (_tooltipRoot == null)
            {
                return Vector2.zero;
            }

            Rect layout = _tooltipRoot.layout;

            // Before the first layout pass (or while `display: none`), width/height read as NaN;
            // fall back to a small default so the clamp math below never propagates NaN.
            float width = float.IsNaN(layout.width) ? 160f : layout.width;
            float height = float.IsNaN(layout.height) ? 32f : layout.height;

            return new Vector2(width, height);
        }
    }
}
