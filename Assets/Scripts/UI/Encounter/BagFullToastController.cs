using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Enemies;
using Rapadura.UI.Common;
using Rapadura.UI.HUD;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Encounter
{
    /// <summary>
    /// "Mochila cheia" feedback for <see cref="LootOverflowEvent"/> (raised by <see cref="EnemyLootGranter"/> when a
    /// kill's loot did not fit on the Mochila's grid): a warning-coloured "Sem espaço: 2
    /// Minério de Ferro" floats over the corpse (<see cref="CombatFxManager.SpawnLootWarningText"/>) and a small toast
    /// at the bottom of the screen says what was left behind and how to make room. Before this, a full bag only wrote
    /// a warning to the log.
    ///
    /// Self-bootstrapping like <see cref="BossIntroBannerController"/>: one DontDestroyOnLoad host, PanelSettings
    /// borrowed from the HUD, view built in code with picking-mode Ignore (never blocks input). A new overflow while
    /// the toast is up merges into it and restarts its timer. Real time (the toast is information, not gameplay);
    /// hidden while <see cref="GameplayPause.IsPaused"/> because it sorts above the pause menu. The look (theme
    /// palette, warning border, fonts) lives in Resources/UI/EncounterOverlays.uss, attached at runtime by
    /// <see cref="EncounterOverlayStyles"/>; the toast has no animation, so reduced motion changes nothing here.
    /// </summary>
    public class BagFullToastController : MonoBehaviour
    {
        public const float SortingOrder = 840f;

        public const string RootName = "bag-full-root";
        public const string ToastName = "bag-full-toast";
        public const string TitleName = "bag-full-title";
        public const string LostName = "bag-full-lost";
        public const string HintName = "bag-full-hint";

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
            _root = CreateView(UiMotionPreferences.IsReducedMotion());
            _toast = _root.Q<VisualElement>(ToastName);
            _titleLabel = _root.Q<Label>(TitleName);
            _lostLabel = _root.Q<Label>(LostName);
            _hintLabel = _root.Q<Label>(HintName);
            documentRoot.Add(_root);
        }

        /// <summary>
        /// The toast tree (root → plate → title, lost items, hint), hidden, picking-mode Ignore throughout, themed
        /// via <see cref="EncounterOverlayStyles"/>. No inline look: everything visual is a USS class. EditMode-testable.
        /// </summary>
        public static VisualElement CreateView(bool reducedMotion)
        {
            var root = new VisualElement { name = RootName, pickingMode = PickingMode.Ignore };
            root.AddToClassList(RootName);
            EncounterOverlayStyles.ApplyTo(root, reducedMotion);

            var toast = new VisualElement { name = ToastName, pickingMode = PickingMode.Ignore };
            toast.AddToClassList(ToastName);
            toast.style.display = DisplayStyle.None;

            toast.Add(MakeLabel(TitleName));
            toast.Add(MakeLabel(LostName));
            toast.Add(MakeLabel(HintName));
            root.Add(toast);
            return root;
        }

        private static Label MakeLabel(string name)
        {
            var label = new Label { name = name, pickingMode = PickingMode.Ignore };
            label.AddToClassList(name);
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
