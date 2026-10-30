using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Common
{
    /// <summary>
    /// Generic, reusable tooltip overlay for UI Toolkit screens, following the same pattern as
    /// <c>HudController</c>/<c>SkillTreeController</c>: a single <see cref="UIDocument"/>, elements
    /// resolved once, pure math (<see cref="TooltipPositioning"/>) and pure text formatting
    /// (<see cref="TooltipContentBuilder"/>) factored out for testability.
    ///
    /// Any Menu/HUD controller can drop this component next to its own <see cref="UIDocument"/> and
    /// either call <see cref="Show(string, VisualElement)"/>/<see cref="Show(TooltipContent, VisualElement)"/>/
    /// <see cref="Hide"/> directly, or — the normal path — bind elements with
    /// <see cref="TooltipTrigger.Bind"/>, which handles mouse hover, touch long-press and
    /// keyboard/gamepad focus. The Inventory, Shop and Skill Tree screens add this component at
    /// runtime if it's missing (<see cref="GetOrAdd"/>), so the tooltip works even before the
    /// Inspector is wired; assign <c>TooltipView.uxml</c> to <see cref="_tooltipViewAsset"/> to get the
    /// designed look (otherwise an inline-styled fallback with the same layout is built in code).
    ///
    /// Two placement modes:
    /// - Called with <c>anchorElement == null</c>: the tooltip follows the mouse cursor (typical for
    ///   pointer hover), repositioned on every <see cref="MouseMoveEvent"/>.
    /// - Called with an <paramref name="anchorElement"/>: the tooltip anchors just below/above that
    ///   element (touch long-press and keyboard/gamepad focus, where there is no cursor to follow).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class TooltipController : MonoBehaviour
    {
        [Header("Template")]
        [Tooltip("TooltipView.uxml. If left empty, an inline-styled VisualElement + Labels is built in code.")]
        [SerializeField] private VisualTreeAsset _tooltipViewAsset;

        [Header("Placement")]
        [SerializeField] private Vector2 _cursorOffset = new Vector2(16f, 16f);
        [SerializeField] private float _anchorSpacing = 8f;

        private UIDocument _uiDocument;
        private VisualElement _panelRoot;
        private VisualElement _tooltipRoot;
        private Label _label;
        private Label _titleLabel;
        private Label _subtitleLabel;
        private Label _statsLabel;
        private Label _descriptionLabel;

        private VisualElement _anchorElement;
        private bool _isVisible;

        public bool IsVisible => _isVisible;

        /// <summary>
        /// Returns the <see cref="TooltipController"/> on <paramref name="host"/>, adding one if the
        /// scene doesn't have it yet. Lets each screen controller use tooltips without extra scene wiring.
        /// </summary>
        public static TooltipController GetOrAdd(GameObject host)
        {
            if (host == null)
            {
                return null;
            }

            TooltipController controller = host.GetComponent<TooltipController>();
            return controller != null ? controller : host.AddComponent<TooltipController>();
        }

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
            _tooltipRoot?.RemoveFromHierarchy();
            _tooltipRoot = null;
            _label = null;
            _titleLabel = null;
            _subtitleLabel = null;
            _statsLabel = null;
            _descriptionLabel = null;
            _isVisible = false;
        }

        /// <summary>
        /// (Re)builds the tooltip element if it doesn't exist or got detached. UIDocument rebuilds
        /// its rootVisualElement content on its own OnEnable (order vs. this component isn't
        /// guaranteed — see the CharacterSelectController note in TODO.md), which can drop an element
        /// added too early; checking <c>panel == null</c> on every Show() makes that harmless.
        /// </summary>
        private bool EnsureTooltipElement()
        {
            if (_tooltipRoot != null && _tooltipRoot.panel != null)
            {
                return true;
            }

            BuildTooltipElement();
            return _tooltipRoot != null;
        }

        private void BuildTooltipElement()
        {
            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();
            }

            _panelRoot = _uiDocument != null ? _uiDocument.rootVisualElement : null;

            if (_panelRoot == null)
            {
                return;
            }

            _tooltipRoot?.RemoveFromHierarchy();

            if (_tooltipViewAsset != null)
            {
                VisualElement instance = _tooltipViewAsset.Instantiate();
                // TemplateContainer wraps the uxml root; unwrap so "tooltip-root" is the element we style/move.
                _tooltipRoot = instance.Q<VisualElement>("tooltip-root") ?? instance;

                // The UXML's <Style src="TooltipView.uss"/> is attached to the TemplateContainer, not to
                // "tooltip-root" — moving the child out on its own would silently drop the stylesheet
                // (same "screen rendered without its USS" bug class fixed for 9 screens in TODO.md).
                if (_tooltipRoot != instance)
                {
                    for (int i = 0; i < instance.styleSheets.count; i++)
                    {
                        _tooltipRoot.styleSheets.Add(instance.styleSheets[i]);
                    }
                }
            }
            else
            {
                _tooltipRoot = new VisualElement { name = "tooltip-root" };
                _tooltipRoot.AddToClassList("tooltip-root");
                ApplyFallbackStyle(_tooltipRoot);
            }

            _titleLabel = EnsureLabel("tooltip-title", "tooltip-title", 14, FontStyle.Bold);
            _subtitleLabel = EnsureLabel("tooltip-subtitle", "tooltip-subtitle", 11, FontStyle.Italic);
            _statsLabel = EnsureLabel("tooltip-stats", "tooltip-stats", 12, FontStyle.Normal);
            _descriptionLabel = EnsureLabel("tooltip-description", "tooltip-description", 11, FontStyle.Normal);
            _label = EnsureLabel("tooltip-label", "tooltip-label", 12, FontStyle.Normal);

            _tooltipRoot.style.position = Position.Absolute;
            _tooltipRoot.pickingMode = PickingMode.Ignore;

            _panelRoot.Add(_tooltipRoot);
            ApplyVisibility(false);
        }

        private Label EnsureLabel(string name, string className, int fallbackFontSize, FontStyle fallbackFontStyle)
        {
            Label label = _tooltipRoot.Q<Label>(name);

            if (label == null)
            {
                label = new Label { name = name };
                label.AddToClassList(className);

                if (_tooltipViewAsset == null)
                {
                    label.style.fontSize = fallbackFontSize;
                    label.style.unityFontStyleAndWeight = fallbackFontStyle;
                    label.style.color = new Color(0.92f, 0.92f, 0.94f);
                    label.style.whiteSpace = WhiteSpace.Normal;
                    label.style.marginBottom = 2;
                }

                _tooltipRoot.Add(label);
            }

            label.pickingMode = PickingMode.Ignore;
            return label;
        }

        private static void ApplyFallbackStyle(VisualElement root)
        {
            // Mirrors TooltipView.uss so the code-built fallback doesn't render as an unstyled box.
            root.style.maxWidth = 280;
            root.style.backgroundColor = new Color(12f / 255f, 14f / 255f, 20f / 255f, 0.94f);
            root.style.borderTopLeftRadius = 6;
            root.style.borderTopRightRadius = 6;
            root.style.borderBottomLeftRadius = 6;
            root.style.borderBottomRightRadius = 6;
            Color border = new Color(1f, 1f, 1f, 0.15f);
            root.style.borderTopWidth = 1;
            root.style.borderBottomWidth = 1;
            root.style.borderLeftWidth = 1;
            root.style.borderRightWidth = 1;
            root.style.borderTopColor = border;
            root.style.borderBottomColor = border;
            root.style.borderLeftColor = border;
            root.style.borderRightColor = border;
            root.style.paddingTop = 6;
            root.style.paddingBottom = 6;
            root.style.paddingLeft = 10;
            root.style.paddingRight = 10;
        }

        /// <summary>Shows the tooltip with a single line of <paramref name="text"/>. See class docs for anchor vs mouse-follow modes.</summary>
        public void Show(string text, VisualElement anchorElement = null)
        {
            if (!EnsureTooltipElement())
            {
                return;
            }

            SetLabel(_titleLabel, null);
            SetLabel(_subtitleLabel, null);
            SetLabel(_statsLabel, null);
            SetLabel(_descriptionLabel, null);
            SetLabel(_label, text);
            ShowInternal(anchorElement);
        }

        /// <summary>Shows structured item/skill content (title, rarity/subtitle, stat lines, description).</summary>
        public void Show(TooltipContent content, VisualElement anchorElement = null)
        {
            if (content == null || !EnsureTooltipElement())
            {
                return;
            }

            SetLabel(_label, null);
            SetLabel(_titleLabel, content.Title);
            SetLabel(_subtitleLabel, content.Subtitle);
            SetLabel(_statsLabel, JoinStatLines(content));
            SetLabel(_descriptionLabel, content.Description);

            if (_titleLabel != null)
            {
                if (content.Rarity.HasValue)
                {
                    ColorblindMode mode = ServiceLocator.TryGet(out AccessibilitySettings accessibility)
                        ? accessibility.ActiveColorblindMode
                        : ColorblindMode.None;
                    _titleLabel.style.color = ColorblindPaletteMap.GetRarityColor(content.Rarity.Value, mode);
                }
                else
                {
                    _titleLabel.style.color = StyleKeyword.Null;
                }

                ApplyRefineGlow(_titleLabel, content.RefineGlowTier);
            }

            ShowInternal(anchorElement);
        }

        /// <summary>
        /// Refine glow of an item title (UI only: no item has a world model to light up): a golden text shadow that
        /// widens at +7 / +9 / +11 / +13 / +15 (<see cref="Rapadura.Gameplay.Refining.RefineRules.GlowTier"/>).
        /// </summary>
        public static void ApplyRefineGlow(VisualElement element, int tier)
        {
            if (element == null)
            {
                return;
            }

            if (tier <= 0)
            {
                element.style.textShadow = StyleKeyword.Null;
                return;
            }

            float alpha = Mathf.Clamp01(0.35f + 0.13f * tier);
            element.style.textShadow = new TextShadow
            {
                offset = Vector2.zero,
                blurRadius = 2f + 2f * tier,
                color = new Color(1f, 0.82f, 0.35f, alpha),
            };
        }

        /// <summary>
        /// Resolver for <see cref="TooltipContentBuilder"/>: <see cref="LocalizationManager.Get"/> when
        /// one is registered (with the tooltip keys merged in), otherwise null (English fallback).
        /// </summary>
        public static System.Func<string, string> GetLocalizer()
        {
            if (!ServiceLocator.TryGet(out LocalizationManager localization))
            {
                return null;
            }

            TooltipLocalization.EnsureRegistered(localization);
            return localization.Get;
        }

        private void ShowInternal(VisualElement anchorElement)
        {
            _anchorElement = anchorElement;
            ApplyVisibility(true);
            _tooltipRoot.BringToFront();

            if (_anchorElement != null)
            {
                UnregisterMouseTracking();
                RepositionToAnchor();
                // Size is only known after the next layout pass; re-anchor once it has run.
                _tooltipRoot.schedule.Execute(RepositionToAnchor);
            }
            else
            {
                RegisterMouseTracking();
            }
        }

        private static void SetLabel(Label label, string text)
        {
            if (label == null)
            {
                return;
            }

            label.text = text ?? string.Empty;
            label.style.display = string.IsNullOrEmpty(text) ? DisplayStyle.None : DisplayStyle.Flex;
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

            // Inline as well as the USS class, so visibility is correct even without TooltipView.uss.
            _tooltipRoot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
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
            if (_anchorElement == null || !_isVisible)
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

        /// <summary>Green of the "Excelente" lines (readable on the dark tooltip panel).</summary>
        public const string OptionLineColor = "#6FDB6F";

        /// <summary>Stat lines joined by newlines; the MU-style option lines are painted green (rich text).</summary>
        public static string JoinStatLines(TooltipContent content)
        {
            if (content == null)
            {
                return string.Empty;
            }

            var lines = new System.Collections.Generic.List<string>(content.StatLines.Count);
            foreach (string line in content.StatLines)
            {
                lines.Add(content.OptionLines.Contains(line) ? "<color=" + OptionLineColor + ">" + line + "</color>" : line);
            }

            return string.Join("\n", lines);
        }
    }
}
