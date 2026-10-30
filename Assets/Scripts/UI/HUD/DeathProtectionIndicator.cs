using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Inventory;
using Rapadura.Save;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.UIElements;
using UIElements = UnityEngine.UIElements;

namespace Rapadura.UI.HUD
{
    /// <summary>
    /// Small HUD badge (top right) showing the death protections the hero carries: "Bênção" while the Bênção da Romaria
    /// is on (<see cref="DeathProtectionState.IsBlessed"/>) and "Tumar ×N" for Tumares in the bag. Self-bootstrapping like
    /// <see cref="NightDangerIndicator"/>: one DontDestroyOnLoad host, PanelSettings borrowed from a HUD, view built in
    /// code with picking-mode Ignore. Visible only with a gameplay HUD on screen and not paused. The Patuá count reads the
    /// player's bag in combat and the carried bag on the world map.
    /// </summary>
    public class DeathProtectionIndicator : MonoBehaviour
    {
        public const float SortingOrder = 601f;
        public const string RootName = "death-protection-root";
        public const string BlessingName = "death-protection-blessing";
        public const string PatuaName = "death-protection-patua";
        private const string PlayerTag = "Player";
        private const float RefreshInterval = 0.5f;
        /// <summary>InventoryManager.SaveKey (the carried bag's entry).</summary>
        private const string CarriedInventoryKey = "Inventory";

        private static DeathProtectionIndicator _instance;

        private UIElements.UIDocument _uiDocument;
        private UIElements.VisualElement _root;
        private UIElements.Label _blessing;
        private UIElements.Label _patua;
        private float _timer = RefreshInterval;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null || FindFirstObjectByType<DeathProtectionIndicator>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(DeathProtectionIndicator));
            host.AddComponent<UIElements.UIDocument>();
            host.AddComponent<DeathProtectionIndicator>();
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

            _uiDocument = GetComponent<UIElements.UIDocument>() ?? gameObject.AddComponent<UIElements.UIDocument>();
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
            _blessing = null;
            _patua = null;
        }

        private void Update()
        {
            _timer += Time.unscaledDeltaTime;
            if (_timer < RefreshInterval)
            {
                return;
            }

            _timer = 0f;
            bool blessed = DeathProtectionState.ResolveShared().IsBlessed;
            int patuas = GameplayHudVisible() ? CountPatuas() : 0;
            bool show = (blessed || patuas > 0) && GameplayHudVisible() && !GameplayPause.IsPaused;
            if (!show)
            {
                if (_root != null)
                {
                    _root.style.display = UIElements.DisplayStyle.None;
                }

                return;
            }

            if (!EnsureBound())
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            _blessing.text = DeathProtectionTexts.Get(localization, DeathProtectionTexts.HudBlessed);
            _blessing.style.display = blessed ? UIElements.DisplayStyle.Flex : UIElements.DisplayStyle.None;
            _patua.text = DeathProtectionTexts.Format(localization, DeathProtectionTexts.HudPatua, patuas);
            _patua.style.display = patuas > 0 ? UIElements.DisplayStyle.Flex : UIElements.DisplayStyle.None;
            _root.style.display = UIElements.DisplayStyle.Flex;
        }

        /// <summary>Patuás in the live player's bag, else in the carried bag (world map).</summary>
        private static int CountPatuas()
        {
            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            InventoryManager bag = player != null ? player.GetComponent<InventoryManager>() : null;
            if (bag != null)
            {
                return bag.GetTotalCount(PatuaRules.ItemId);
            }

            if (!ServiceLocator.TryGet(out SaveManager saveManager) || !saveManager.TryGetCarriedJson(CarriedInventoryKey, out string json))
            {
                return 0;
            }

            InventorySaveState state = WorldMapHudPresenter.ParseInventory(json);
            int count = 0;
            if (state?.slots != null)
            {
                foreach (InventorySlotData slot in state.slots)
                {
                    if (slot != null && slot.itemId == PatuaRules.ItemId)
                    {
                        count += slot.quantity;
                    }
                }
            }

            return count;
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
            _blessing = _root.Q<UIElements.Label>(BlessingName);
            _patua = _root.Q<UIElements.Label>(PatuaName);
            documentRoot.Add(_root);
            return true;
        }

        /// <summary>The badge tree (root → two small labels), hidden, picking-mode Ignore throughout.</summary>
        public static UIElements.VisualElement CreateView()
        {
            var root = new UIElements.VisualElement { name = RootName, pickingMode = UIElements.PickingMode.Ignore };
            root.style.display = UIElements.DisplayStyle.None;
            root.style.position = UIElements.Position.Absolute;
            root.style.right = 12;
            root.style.top = 104;
            root.style.flexDirection = UIElements.FlexDirection.Column;
            root.style.alignItems = UIElements.Align.FlexEnd;
            root.Add(Badge(BlessingName, new Color(0.25f, 0.18f, 0.05f, 0.85f), new Color(1f, 0.85f, 0.4f, 0.95f)));
            root.Add(Badge(PatuaName, new Color(0.3f, 0.08f, 0.08f, 0.85f), new Color(1f, 0.55f, 0.45f, 0.95f)));
            return root;
        }

        private static UIElements.Label Badge(string name, Color background, Color border)
        {
            var label = new UIElements.Label { name = name, pickingMode = UIElements.PickingMode.Ignore };
            label.style.display = UIElements.DisplayStyle.None;
            label.style.backgroundColor = background;
            label.style.color = new Color(1f, 0.95f, 0.8f, 1f);
            label.style.fontSize = 15;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 4;
            label.style.paddingLeft = 10;
            label.style.paddingRight = 10;
            label.style.paddingTop = 3;
            label.style.paddingBottom = 3;
            label.style.borderTopLeftRadius = 12;
            label.style.borderTopRightRadius = 12;
            label.style.borderBottomLeftRadius = 12;
            label.style.borderBottomRightRadius = 12;
            label.style.borderLeftWidth = 2;
            label.style.borderRightWidth = 2;
            label.style.borderTopWidth = 2;
            label.style.borderBottomWidth = 2;
            label.style.borderLeftColor = border;
            label.style.borderRightColor = border;
            label.style.borderTopColor = border;
            label.style.borderBottomColor = border;
            return label;
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
