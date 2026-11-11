using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Dungeon;
using Rapadura.Gameplay.Enemies;
using Rapadura.UI.HUD;
using Rapadura.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Dungeon
{
    /// <summary>
    /// In-dungeon HUD of the Ossuário dos Artífices: dungeon and room name, the bone keys in the bag, the current objective
    /// (<see cref="DungeonDirector.GoalText"/>), the Colosso's health bar once it is awake, and the director's banner
    /// (cousin lines, room names, "Prisão de Costelas!"...). Self-bootstrapping like <c>EventRoomHud</c>: one
    /// DontDestroyOnLoad host whose UIDocument borrows the HUD's PanelSettings; it polls <see cref="DungeonDirector.Current"/>
    /// a few times per second (real time) and hides whenever no dungeon is running. Picking-mode Ignore throughout.
    /// </summary>
    public class DungeonHud : MonoBehaviour
    {
        public const float SortingOrder = 560f;
        public const string LayerName = "dg-layer";
        private const float RefreshInterval = 0.1f;

        private static DungeonHud _instance;

        private UIDocument _document;
        private VisualElement _layer;
        private Label _title;
        private Label _room;
        private Label _keys;
        private Label _goal;
        private VisualElement _bossBar;
        private VisualElement _bossFill;
        private Label _bossName;
        private VisualElement _bannerRoot;
        private Label _banner;
        private float _refresh;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null || FindFirstObjectByType<DungeonHud>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(DungeonHud));
            host.AddComponent<UIDocument>();
            host.AddComponent<DungeonHud>();
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

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnDisable()
        {
            _layer = null;
        }

        // ------------------------------------------------------------------
        // Pure helpers (tested)
        // ------------------------------------------------------------------

        /// <summary>"Chaves de osso: 1".</summary>
        public static string KeysLine(LocalizationManager localization, int keys)
        {
            return DungeonTexts.Format(localization, DungeonTexts.HudKeys, Mathf.Max(0, keys));
        }

        /// <summary>Fill (0..1) of the boss bar; 0 without a living boss.</summary>
        public static float BossFraction(float current, float max)
        {
            return max > 0f ? Mathf.Clamp01(current / max) : 0f;
        }

        // ------------------------------------------------------------------
        // Loop
        // ------------------------------------------------------------------

        private void Update()
        {
            _refresh += Time.unscaledDeltaTime;
            if (_refresh < RefreshInterval)
            {
                return;
            }

            _refresh = 0f;
            DungeonDirector director = DungeonDirector.Current;
            if (director == null || director.Hall == null)
            {
                if (_layer != null)
                {
                    _layer.style.display = DisplayStyle.None;
                }

                return;
            }

            if (!EnsureBound())
            {
                return;
            }

            // Sorts at 560, above every window: a modal (Barcas de Luz, Mochila, pause...) hides it, banner and all.
            if (MessageCards.ShouldHold(MessageCards.IsModalOpen))
            {
                _layer.style.display = DisplayStyle.None;
                return;
            }

            _layer.style.display = DisplayStyle.Flex;
            ServiceLocator.TryGet(out LocalizationManager localization);
            Draw(director, localization);
        }

        private void Draw(DungeonDirector director, LocalizationManager localization)
        {
            _title.text = DungeonTexts.Get(localization, DungeonTexts.DungeonName);
            _room.text = DungeonTexts.Get(localization, DungeonTexts.RoomNameKey(director.HeroRoom));
            _keys.text = KeysLine(localization, director.KeysHeld);
            _goal.text = director.GoalText(localization);

            GameObject boss = director.Boss;
            Health health = null;
            if (boss != null && boss.TryGetComponent(out EnemyController controller))
            {
                health = controller.Health;
            }

            bool showBoss = health != null && !health.IsDead && !director.IsCleared;
            _bossBar.style.display = showBoss ? DisplayStyle.Flex : DisplayStyle.None;
            if (showBoss)
            {
                _bossName.text = DungeonTexts.Get(localization, DungeonTexts.EnemyNameKey(DungeonRules.BoneColossusId));
                _bossFill.style.width = Length.Percent(BossFraction(health.CurrentHealth, health.MaxHealth) * 100f);
            }

            string banner = director.Banner;
            _bannerRoot.style.display = string.IsNullOrEmpty(banner) ? DisplayStyle.None : DisplayStyle.Flex;
            _banner.text = banner ?? string.Empty;
        }

        // ------------------------------------------------------------------
        // View
        // ------------------------------------------------------------------

        private bool EnsureBound()
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

            if (_layer != null && _layer.parent == root)
            {
                return true;
            }

            root.pickingMode = PickingMode.Ignore;
            _layer = CreateView();
            Bind(_layer);
            root.Add(_layer);
            return true;
        }

        /// <summary>The whole tree (hidden parts included), picking-mode Ignore throughout. EditMode-testable.</summary>
        public static VisualElement CreateView()
        {
            var layer = new VisualElement { name = LayerName, pickingMode = PickingMode.Ignore };
            layer.AddToClassList("dg-layer");
            DungeonStyles.ApplyTo(layer);

            var hud = Element("dg-hud");
            var card = Element("dg-hud-card");
            card.Add(Text("dg-hud-title"));
            card.Add(Text("dg-hud-room"));
            card.Add(Text("dg-hud-keys"));
            card.Add(Text("dg-hud-goal"));
            hud.Add(card);
            layer.Add(hud);

            var boss = Element("dg-boss");
            boss.Add(Text("dg-boss-name"));
            var bar = Element("dg-boss-bar");
            bar.Add(Element("dg-boss-fill"));
            boss.Add(bar);
            boss.style.display = DisplayStyle.None;
            layer.Add(boss);

            var banner = Element("dg-banner");
            banner.Add(Text("dg-banner-text"));
            banner.style.display = DisplayStyle.None;
            layer.Add(banner);
            return layer;
        }

        private void Bind(VisualElement layer)
        {
            _title = layer.Q<Label>("dg-hud-title");
            _room = layer.Q<Label>("dg-hud-room");
            _keys = layer.Q<Label>("dg-hud-keys");
            _goal = layer.Q<Label>("dg-hud-goal");
            _bossBar = layer.Q("dg-boss");
            _bossName = layer.Q<Label>("dg-boss-name");
            _bossFill = layer.Q("dg-boss-fill");
            _bannerRoot = layer.Q("dg-banner");
            _banner = layer.Q<Label>("dg-banner-text");
        }

        private static VisualElement Element(string name)
        {
            var element = new VisualElement { name = name, pickingMode = PickingMode.Ignore };
            element.AddToClassList(name);
            return element;
        }

        private static Label Text(string name)
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
                if (document != _document && document.panelSettings != null)
                {
                    return document.panelSettings;
                }
            }

            return null;
        }
    }
}
