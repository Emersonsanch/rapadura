using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.UI.HUD;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.WorldMap
{
    /// <summary>One line of a <see cref="CityServicePanel"/>: a title, a detail line and a button (no button when <see cref="Button"/> is empty).</summary>
    public readonly struct CityServiceRow
    {
        public readonly string Title;
        public readonly string Detail;
        public readonly string Button;
        public readonly bool Enabled;

        /// <summary>Highlighted (the circle lit now, the shelf to receive).</summary>
        public readonly bool Picked;

        public CityServiceRow(string title, string detail, string button, bool enabled, bool picked = false)
        {
            Title = title;
            Detail = detail;
            Button = button;
            Enabled = enabled;
            Picked = picked;
        }
    }

    /// <summary>
    /// What a city service shows on a <see cref="CityServicePanel"/>. The scene side (a mechanics driver) implements it on top of the
    /// service's pure rules; the panel only draws it and forwards the clicks, so online (Fase 12) the same screen can show the server's answer.
    /// </summary>
    public interface ICityServiceScreen
    {
        string Title(LocalizationManager localization);

        /// <summary>The keeper's line on top (may be empty).</summary>
        string Intro(LocalizationManager localization);

        /// <summary>One line under the intro (gold owned, pages filled...; may be empty).</summary>
        string Summary(LocalizationManager localization);

        IReadOnlyList<CityServiceRow> Rows(LocalizationManager localization);

        /// <summary>The button of row <paramref name="row"/> was pressed: do it and answer with the status line.</summary>
        string Pick(int row, LocalizationManager localization);

        /// <summary>The close button's label.</summary>
        string CloseLabel(LocalizationManager localization);
    }

    /// <summary>
    /// The small screen of a city keeper's service in the seamless world (Djedi's circles of light, Setne's shelf of the Caderno), opened by
    /// [E] at the service point. Self-bootstrapping like the travel window: one DontDestroyOnLoad host with its own UIDocument (PanelSettings
    /// borrowed from the gameplay HUD), view built in code with the hunting board's classes (<c>Resources/UI/Hunting.uss</c> under a
    /// <c>rap-theme</c> root: the Rapadura palette). Holds a <see cref="GameplayPause"/> while up; Esc / B / "Fechar" close it, a scene load
    /// closes it. No state of its own: every row comes from the <see cref="ICityServiceScreen"/>, rebuilt after each pick.
    /// </summary>
    public sealed class CityServicePanel : MonoBehaviour
    {
        public const float SortingOrder = 30f;
        public const string StyleResourcePath = "UI/Hunting";
        public const string OverlayName = "city-service-overlay";
        public const string RowPrefix = "city-service-row-";
        public const string CloseName = "city-service-close";

        private static CityServicePanel _instance;

        private UIDocument _document;
        private VisualElement _overlay;
        private Label _title;
        private Label _intro;
        private Label _summary;
        private ScrollView _list;
        private Label _status;
        private Button _close;
        private ICityServiceScreen _screen;
        private bool _open;
        private int _openedFrame;

        /// <summary>Whether a service screen is up (the drivers ignore [E] meanwhile).</summary>
        public static bool IsOpen => _instance != null && _instance._open;

        /// <summary>Opens <paramref name="screen"/>; false when there is no HUD to borrow the panel settings from.</summary>
        public static bool Open(ICityServiceScreen screen)
        {
            if (screen == null)
            {
                return false;
            }

            if (_instance == null)
            {
                var host = new GameObject(nameof(CityServicePanel));
                host.AddComponent<UIDocument>();
                host.AddComponent<CityServicePanel>();
            }

            return _instance != null && _instance.Show(screen);
        }

        public static void Close()
        {
            if (_instance != null)
            {
                _instance.Hide();
            }
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

            _document = GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            _document.sortingOrder = SortingOrder;
        }

        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Hide();
        }

        private void OnDestroy()
        {
            GameplayPause.Release(this);
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Hide();
            _overlay = null;
        }

        private void Update()
        {
            if (!_open || Time.frameCount == _openedFrame)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            Gamepad pad = Gamepad.current;
            if ((keyboard != null && keyboard.escapeKey.wasPressedThisFrame) || (pad != null && pad.buttonEast.wasPressedThisFrame))
            {
                Hide();
            }
        }

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        private bool Show(ICityServiceScreen screen)
        {
            if (!EnsureOverlay())
            {
                return false;
            }

            _screen = screen;
            _open = true;
            _openedFrame = Time.frameCount;
            _status.text = string.Empty;
            _overlay.style.display = DisplayStyle.Flex;
            GameplayPause.Acquire(this);
            Rebuild();
            return true;
        }

        private void Hide()
        {
            if (_overlay != null)
            {
                _overlay.style.display = DisplayStyle.None;
            }

            _open = false;
            _screen = null;
            GameplayPause.Release(this);
        }

        private bool EnsureOverlay()
        {
            if (_document == null)
            {
                return false;
            }

            if (_document.panelSettings == null)
            {
                PanelSettings donor = FindPanelSettings();
                if (donor == null)
                {
                    return false;
                }

                _document.panelSettings = donor;
                _document.sortingOrder = SortingOrder;
            }

            VisualElement root = _document.rootVisualElement;
            if (root == null)
            {
                return false;
            }

            if (_overlay != null && _overlay.parent == root)
            {
                return true;
            }

            root.pickingMode = PickingMode.Ignore;
            _overlay = new VisualElement { name = OverlayName };
            _overlay.AddToClassList("rap-theme");
            StyleSheet sheet = Resources.Load<StyleSheet>(StyleResourcePath);
            if (sheet != null)
            {
                _overlay.styleSheets.Add(sheet);
            }

            _overlay.style.position = Position.Absolute;
            _overlay.style.left = 0;
            _overlay.style.right = 0;
            _overlay.style.top = 0;
            _overlay.style.bottom = 0;
            _overlay.style.alignItems = Align.Center;
            _overlay.style.justifyContent = Justify.Center;
            _overlay.style.backgroundColor = new Color(0f, 0f, 0f, 0.55f);
            _overlay.style.display = DisplayStyle.None;

            var panel = new VisualElement { name = "city-service-panel" };
            panel.AddToClassList("rap-panel");
            panel.AddToClassList("hunting-panel");
            _overlay.Add(panel);

            _title = AddLabel(panel, "hunting-section-title");
            _title.style.fontSize = 18;
            _intro = AddLabel(panel, "hunting-intro");
            _summary = AddLabel(panel, "hunting-line");
            _list = new ScrollView(ScrollViewMode.Vertical) { name = "city-service-list" };
            _list.AddToClassList("hunting-scroll");
            panel.Add(_list);
            _status = AddLabel(panel, "hunting-status");

            _close = new Button(Hide) { name = CloseName, focusable = true };
            _close.AddToClassList("rap-button");
            _close.AddToClassList("rap-button--primary");
            _close.style.marginTop = 6;
            panel.Add(_close);

            root.Add(_overlay);
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
                if (document != _document && document.panelSettings != null)
                {
                    return document.panelSettings;
                }
            }

            return null;
        }

        private static Label AddLabel(VisualElement parent, string className)
        {
            var label = new Label();
            label.AddToClassList(className);
            parent.Add(label);
            return label;
        }

        private void Rebuild()
        {
            if (!_open || _screen == null)
            {
                return;
            }

            LocalizationManager localization = Localization;
            _title.text = _screen.Title(localization);
            SetLine(_intro, _screen.Intro(localization));
            SetLine(_summary, _screen.Summary(localization));
            _close.text = _screen.CloseLabel(localization);

            _list.Clear();
            Button first = null;
            IReadOnlyList<CityServiceRow> rows = _screen.Rows(localization);
            for (int i = 0; i < rows.Count; i++)
            {
                CityServiceRow data = rows[i];
                var row = new VisualElement { name = RowPrefix + i };
                row.AddToClassList("hunting-row");
                if (data.Picked)
                {
                    row.AddToClassList("hunting-row--picked");
                }

                var body = new VisualElement();
                body.style.flexGrow = 1;
                body.style.flexShrink = 1;
                body.style.minWidth = 200;
                Label name = AddLabel(body, "hunting-creature");
                name.text = data.Title;
                if (!string.IsNullOrEmpty(data.Detail))
                {
                    Label detail = AddLabel(body, "hunting-subtle");
                    detail.text = data.Detail;
                }

                row.Add(body);
                if (!string.IsNullOrEmpty(data.Button))
                {
                    int index = i;
                    var button = new Button(() => OnPick(index)) { name = RowPrefix + i + "-go", focusable = true, text = data.Button };
                    button.AddToClassList("rap-button");
                    button.AddToClassList("hunting-button");
                    button.SetEnabled(data.Enabled);
                    row.Add(button);
                    if (first == null && data.Enabled)
                    {
                        first = button;
                    }
                }

                _list.Add(row);
            }

            (first ?? _close).Focus();
        }

        private static void SetLine(Label label, string text)
        {
            label.text = text ?? string.Empty;
            label.style.display = string.IsNullOrEmpty(text) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void OnPick(int row)
        {
            if (!_open || _screen == null)
            {
                return;
            }

            _status.text = _screen.Pick(row, Localization) ?? string.Empty;
            Rebuild();
        }
    }
}
