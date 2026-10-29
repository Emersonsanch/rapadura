using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Enemies;
using Rapadura.UI.HUD;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Encounter
{
    /// <summary>
    /// "Mochila cheia" feedback for <see cref="LootOverflowEvent"/> (raised by <see cref="EnemyLootGranter"/> when a
    /// kill's loot did not fit, or quest items went in past the weight limit): a warning-coloured "Sem espaço: 2
    /// Minério de Ferro" floats over the corpse (<see cref="CombatFxManager.SpawnLootWarningText"/>) and a small toast
    /// at the bottom of the screen says what was left behind and how to make room. Before this, a full bag only wrote
    /// a warning to the log.
    ///
    /// Self-bootstrapping like <see cref="BossIntroBannerController"/>: one DontDestroyOnLoad host, PanelSettings
    /// borrowed from the HUD, view built in code with picking-mode Ignore (never blocks input). A new overflow while
    /// the toast is up merges into it and restarts its timer. Real time (the toast is information, not gameplay);
    /// hidden while <see cref="GameplayPause.IsPaused"/> because it sorts above the pause menu.
    /// </summary>
    public class BagFullToastController : MonoBehaviour
    {
        public const float SortingOrder = 840f;

        [Tooltip("Seconds the toast stays up after the last overflow (real time).")]
        [SerializeField, Min(0.5f)] private float _displaySeconds = 3.5f;

        private static BagFullToastController _instance;

        private readonly Dictionary<string, int> _lostByName = new Dictionary<string, int>();
        private bool _keptQuestItems;
        private float _remaining;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _toast;
        private Label _titleLabel;
        private Label _lostLabel;
        private Label _hintLabel;

        /// <summary>True while the toast is on screen (or waiting out a pause).</summary>
        public bool IsShowing => _remaining > 0f;

        /// <summary>
        /// "2 Minério de Ferro" / "Espada de Ferro": the quantity only when above one. Pure, EditMode-tested.
        /// </summary>
        public static string DescribeAmount(string name, int quantity) => quantity > 1 ? quantity + " " + name : name;

        /// <summary>Joins lost entries in arrival order: "2 Minério de Ferro, Espada de Ferro".</summary>
        public static string JoinLost(IEnumerable<KeyValuePair<string, int>> lost) =>
            string.Join(", ", lost.Where(p => p.Value > 0).Select(p => DescribeAmount(p.Key, p.Value)));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null || FindFirstObjectByType<BagFullToastController>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(BagFullToastController));
            host.AddComponent<UIDocument>();
            host.AddComponent<BagFullToastController>();
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
            EventBus.Subscribe<LootOverflowEvent>(OnLootOverflow);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LootOverflowEvent>(OnLootOverflow);
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

        private void OnLootOverflow(LootOverflowEvent evt)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);

            bool anyLost = false;
            int line = 0;
            foreach (LootGrantResult result in evt.Results)
            {
                if (result.KeptOverWeight > 0)
                {
                    _keptQuestItems = true;
                }

                if (result.Lost <= 0 || result.Drop.Item == null)
                {
                    continue;
                }

                anyLost = true;
                string name = result.Drop.IsGenerated ? result.Drop.Generated.GeneratedName : result.Drop.Item.LocalizedName(localization);
                _lostByName.TryGetValue(name, out int count);
                _lostByName[name] = count + result.Lost;

                CombatFxManager.SpawnLootWarningText(evt.Position + Vector3.up * (0.35f * line++),
                    BossTexts.Format(localization, BossTexts.LootNoRoom, DescribeAmount(name, result.Lost)));
            }

            if (anyLost || _keptQuestItems)
            {
                Show(localization);
            }
        }

        private void Show(LocalizationManager localization)
        {
            if (!EnsureBound())
            {
                return;
            }

            _titleLabel.text = BossTexts.Get(localization, BossTexts.BagFullTitle);

            string lost = JoinLost(_lostByName);
            var lines = new List<string>();
            if (!string.IsNullOrEmpty(lost))
            {
                lines.Add(BossTexts.Format(localization, BossTexts.BagFullLost, lost));
            }

            if (_keptQuestItems)
            {
                lines.Add(BossTexts.Get(localization, BossTexts.BagFullKeptQuest));
            }

            _lostLabel.text = string.Join("\n", lines);
            _hintLabel.text = BossTexts.Get(localization, BossTexts.BagFullHint);

            _remaining = Mathf.Max(0.5f, _displaySeconds);
            _toast.style.display = GameplayPause.IsPaused ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void Update()
        {
            if (_remaining <= 0f || _toast == null)
            {
                return;
            }

            bool paused = GameplayPause.IsPaused;
            _toast.style.display = paused ? DisplayStyle.None : DisplayStyle.Flex;
            if (paused)
            {
                return;
            }

            _remaining -= Time.unscaledDeltaTime;
            if (_remaining <= 0f)
            {
                Hide();
            }
        }

        private void Hide()
        {
            _remaining = 0f;
            _lostByName.Clear();
            _keptQuestItems = false;
            if (_toast != null)
            {
                _toast.style.display = DisplayStyle.None;
            }
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
            _root = new VisualElement { name = "bag-full-root", pickingMode = PickingMode.Ignore };
            _root.style.position = Position.Absolute;
            _root.style.left = 0;
            _root.style.right = 0;
            // Above the skill bar / joystick row, clear of the boss banner (26% from the top).
            _root.style.bottom = Length.Percent(24);
            _root.style.alignItems = Align.Center;

            Color warning = CombatFxManager.LootWarningColor;
            _toast = new VisualElement { name = "bag-full-toast", pickingMode = PickingMode.Ignore };
            _toast.AddToClassList("bag-full-toast");
            _toast.style.paddingLeft = 22;
            _toast.style.paddingRight = 22;
            _toast.style.paddingTop = 8;
            _toast.style.paddingBottom = 10;
            _toast.style.maxWidth = Length.Percent(80);
            _toast.style.alignItems = Align.Center;
            _toast.style.backgroundColor = new Color(0.10f, 0.04f, 0.03f, 0.95f);
            _toast.style.borderTopWidth = 2;
            _toast.style.borderBottomWidth = 2;
            _toast.style.borderLeftWidth = 2;
            _toast.style.borderRightWidth = 2;
            _toast.style.borderTopColor = warning;
            _toast.style.borderBottomColor = warning;
            _toast.style.borderLeftColor = warning;
            _toast.style.borderRightColor = warning;
            _toast.style.borderTopLeftRadius = 8;
            _toast.style.borderTopRightRadius = 8;
            _toast.style.borderBottomLeftRadius = 8;
            _toast.style.borderBottomRightRadius = 8;
            _toast.style.display = DisplayStyle.None;

            _titleLabel = MakeLabel("bag-full-title", 24, warning, FontStyle.Bold);
            _lostLabel = MakeLabel("bag-full-lost", 17, new Color(0.98f, 0.94f, 0.86f, 1f), FontStyle.Normal);
            _hintLabel = MakeLabel("bag-full-hint", 14, new Color(0.85f, 0.80f, 0.72f, 1f), FontStyle.Italic);

            _toast.Add(_titleLabel);
            _toast.Add(_lostLabel);
            _toast.Add(_hintLabel);
            _root.Add(_toast);
            documentRoot.Add(_root);
        }

        private static Label MakeLabel(string name, int fontSize, Color color, FontStyle style)
        {
            var label = new Label { name = name, pickingMode = PickingMode.Ignore };
            label.style.fontSize = fontSize;
            label.style.color = color;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.unityFontStyleAndWeight = style;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.textShadow = new TextShadow
            {
                offset = new Vector2(1.5f, 1.5f),
                blurRadius = 2f,
                color = new Color(0f, 0f, 0f, 0.9f),
            };
            return label;
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
