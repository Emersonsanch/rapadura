using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using Rapadura.UI.Crafting;
using Rapadura.UI.Shop;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// "Vila" on the world map: the village services of Main.unity (personal-story villagers, Maria's
    /// gathering point, the shop and the workbench) are switched off during encounters, the only time that
    /// scene is loaded, so they are offered here instead. Self-bootstrapping like
    /// <see cref="RegionNpcTalkButton"/>, no scene edit:
    /// <list type="bullet">
    /// <item>Added to the <see cref="WorldMapHudController"/>'s GameObject after every scene load; injects a
    /// button into the HUD's <c>wm-action-row</c> and a modal panel under <c>wm-hud-root</c>, so the HUD's
    /// pointer blocker covers both. The panel and the two service windows join the HUD's screens
    /// (<see cref="WorldMapHudController.RegisterScreen"/>): one at a time, Esc closes them.</item>
    /// <item>Button shown while the HUD is visible, the token is idle in <see cref="VillageNpcCatalog.VillageRegion"/>
    /// and nothing holds GameplayPause; "(!)" when a villager has a quest to give or hand in.</item>
    /// <item>Conversar: the same <c>Resources/Dialogue</c> graphs as Main.unity's NPCs, through
    /// <see cref="DialogueManager"/> (rewards of a quest finished here wait in
    /// <see cref="Rapadura.Gameplay.Quests.QuestManager.DeliverPendingRewards"/>).</item>
    /// <item>Loja / Bancada: <see cref="ShopManager"/> / <see cref="CraftingManager"/> and their windows,
    /// created on first use from <c>Resources/UI/Village*View.uxml</c> (instances of the Main.unity views),
    /// working on a live <see cref="CarriedInventorySession"/> that hands the inventory back to the carry
    /// as soon as both windows are closed.</item>
    /// <item>Fogão a lenha: <see cref="VillageKitchenController"/> (built in code on this HUD's document), on the
    /// same live inventory session.</item>
    /// <item>Closing a conversation or service window returns to the panel, unless something else (pause
    /// menu) took over meanwhile.</item>
    /// </list>
    /// </summary>
    [DisallowMultipleComponent]
    public class VillageHubButton : MonoBehaviour
    {
        public const string ButtonName = "wm-village-button";
        public const string OverlayName = "wm-village-overlay";
        /// <summary>USS classes (WorldMapHudView.uss) of the hub's section headers and option buttons.</summary>
        public const string VillageSectionClass = "wm-village-section";
        public const string VillageOptionClass = "wm-village-option";
        public const string HubScreenId = "village";
        public const string ShopScreenId = "village_shop";
        public const string CraftingScreenId = "village_crafting";
        public const string ChestScreenId = "village_chest";
        /// <summary>"Fogão a lenha" window (<see cref="VillageKitchenController"/>).</summary>
        public const string KitchenScreenId = "village_kitchen";
        public const string ShopViewResourcePath = "UI/VillageShopView";
        public const string CraftingViewResourcePath = "UI/VillageCraftingView";
        /// <summary>"Baú da Vó Zefa" window (<see cref="VillageChestUIController"/>).</summary>
        public const string ChestViewResourcePath = "UI/VillageChestView";
        public const string ShopResourcePath = "Shops/shop_general_store";
        public const CraftingStationType Station = CraftingStationType.Workbench;
        public const int ServiceSortingOrder = 14;

        private const float RefreshInterval = 0.2f;

        private WorldMapHudController _hud;
        private UIDocument _hudDocument;
        private WorldMapController _worldMap;
        private Button _button;
        private VisualElement _overlay;
        private Label _titleLabel;
        private Label _subtitleLabel;
        private Label _talkHeader;
        private Label _servicesHeader;
        private Button _shopButton;
        private Button _craftingButton;
        private Button _chestButton;
        private Button _kitchenButton;
        private Button _closeButton;
        private readonly List<Button> _npcButtons = new List<Button>();
        private bool _hubOpen;
        private bool _returnToHub;
        private bool _screensRegistered;
        private float _nextRefresh;

        private ShopUIController _shopUI;
        private CraftingUIController _craftingUI;
        private VillageChestUIController _chestUI;
        private VillageKitchenController _kitchenUI;
        private CarriedInventorySession _session;
        private readonly QuestDialogueContext _questContext = new QuestDialogueContext();

        public bool IsHubOpen => _hubOpen;

        // ------------------------------------------------------------------
        // Bootstrap
        // ------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            AttachToHud();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => AttachToHud();

        private static void AttachToHud()
        {
            WorldMapHudController hud = FindFirstObjectByType<WorldMapHudController>(FindObjectsInactive.Include);
            if (hud != null && hud.GetComponent<VillageHubButton>() == null)
            {
                hud.gameObject.AddComponent<VillageHubButton>();
            }
        }

        private void Awake()
        {
            _hud = GetComponent<WorldMapHudController>();
            _hudDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<ShopClosedEvent>(OnShopClosed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ShopClosedEvent>(OnShopClosed);
            CloseHub();
            _returnToHub = false;
            EndInventorySession();

            if (_button != null)
            {
                _button.clicked -= OnButtonClicked;
                _button.RemoveFromHierarchy();
                _button = null;
            }

            if (_overlay != null)
            {
                _overlay.RemoveFromHierarchy();
                _overlay = null;
                _npcButtons.Clear();
            }
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + RefreshInterval;
            RegisterScreens();
            Refresh();
        }

        // ------------------------------------------------------------------
        // Pure rules (tested)
        // ------------------------------------------------------------------

        /// <summary>Whether the "Vila" button should be on screen.</summary>
        public static bool ShouldShow(bool hudVisible, bool tokenIdle, bool paused, bool dialogueActive, BiomeType? currentRegion)
        {
            return hudVisible && tokenIdle && !paused && !dialogueActive && VillageNpcCatalog.IsVillageRegion(currentRegion);
        }

        /// <summary>
        /// After a conversation or service window started from the panel ends: back to the panel, unless
        /// something else took over (pause menu, quest log...) or the HUD went away.
        /// </summary>
        public static bool ShouldReturnToHub(bool pending, bool serviceOpen, bool dialogueActive, bool paused, bool hudVisible)
        {
            return pending && !serviceOpen && !dialogueActive && !paused && hudVisible;
        }

        /// <summary>Whether the live map inventory can be handed back to the carry (no service window uses it).</summary>
        public static bool ShouldEndInventorySession(bool sessionOpen, bool shopOpen, bool craftingOpen)
        {
            return ShouldEndInventorySession(sessionOpen, shopOpen, craftingOpen, false);
        }

        /// <summary>Same, counting the "Baú da Vó Zefa" window, which also works on the live map inventory.</summary>
        public static bool ShouldEndInventorySession(bool sessionOpen, bool shopOpen, bool craftingOpen, bool chestOpen)
        {
            return sessionOpen && !shopOpen && !craftingOpen && !chestOpen;
        }

        /// <summary>"Sobrevivente da Vila · Joaquim" (+ " (!)"), the villager's name and whose story it is.</summary>
        public static string BuildNpcLabel(VillageNpcSpec npc, bool hasNews, LocalizationManager localization)
        {
            string name = VillageHubTexts.Get(localization, npc.NameKey);
            string hero = WorldMapHudPresenter.HeroName(localization, npc.Hero);
            return VillageHubTexts.Format(localization, hasNews ? VillageHubTexts.NpcLabelNews : VillageHubTexts.NpcLabel, name, hero);
        }

        // ------------------------------------------------------------------
        // Runtime
        // ------------------------------------------------------------------

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        private bool ShopOpen => _shopUI != null && _shopUI.IsOpen;
        private bool CraftingOpen => _craftingUI != null && _craftingUI.IsOpen;
        private bool ChestOpen => _chestUI != null && _chestUI.IsOpen;
        private bool KitchenOpen => _kitchenUI != null && _kitchenUI.IsOpen;

        private void RegisterScreens()
        {
            if (_screensRegistered || _hud == null)
            {
                return;
            }

            _hud.RegisterScreen(HubScreenId, () => _hubOpen, OpenHub, CloseHub);
            _hud.RegisterScreen(ShopScreenId, () => ShopOpen, OpenShop, () => { if (_shopUI != null) _shopUI.Close(); });
            _hud.RegisterScreen(CraftingScreenId, () => CraftingOpen, OpenCrafting, () => { if (_craftingUI != null) _craftingUI.Close(); });
            _hud.RegisterScreen(ChestScreenId, () => ChestOpen, OpenChest, () => { if (_chestUI != null) _chestUI.Close(); });
            _hud.RegisterScreen(KitchenScreenId, () => KitchenOpen, OpenKitchen, () => { if (_kitchenUI != null) _kitchenUI.Close(); });
            _screensRegistered = true;
        }

        private void Refresh()
        {
            if (_worldMap == null)
            {
                _worldMap = FindFirstObjectByType<WorldMapController>();
            }

            bool hudVisible = _hud != null && _hud.IsVisible;
            ServiceLocator.TryGet(out DialogueManager dialogue);
            bool dialogueActive = dialogue != null && dialogue.IsActive;

            if (ShouldEndInventorySession(_session != null && _session.IsOpen, ShopOpen, CraftingOpen, ChestOpen || KitchenOpen))
            {
                EndInventorySession();
            }

            if (_hubOpen && !hudVisible)
            {
                CloseHub();
            }

            // Decided once the conversation/window is over: back to the panel, or (something else took
            // over) forget it.
            bool serviceOpen = ShopOpen || CraftingOpen || ChestOpen || KitchenOpen;
            if (_returnToHub && !serviceOpen && !dialogueActive)
            {
                bool back = ShouldReturnToHub(_returnToHub, serviceOpen, dialogueActive, GameplayPause.IsPaused, hudVisible);
                _returnToHub = false;
                if (back)
                {
                    OpenHub();
                }
            }

            if (_hubOpen)
            {
                RefreshHubTexts();
            }

            if (!EnsureButton())
            {
                return;
            }

            BiomeType? region = _worldMap != null && _worldMap.CurrentNode != null ? _worldMap.CurrentNode.Region : (BiomeType?)null;
            bool show = ShouldShow(hudVisible, _worldMap != null && _worldMap.CanExplore, GameplayPause.IsPaused, dialogueActive, region);
            _button.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (!show)
            {
                return;
            }

            string label = VillageHubTexts.Get(Localization, AnyNews() ? VillageHubTexts.ButtonNews : VillageHubTexts.Button);
            if (_button.text != label)
            {
                _button.text = label;
            }
        }

        private bool AnyNews()
        {
            foreach (VillageNpcSpec npc in VillageNpcCatalog.Npcs)
            {
                if (VillageNpcCatalog.HasNews(_questContext.GetQuestState(npc.QuestId)))
                {
                    return true;
                }
            }

            return false;
        }

        private bool EnsureButton()
        {
            if (_hudDocument == null || _hudDocument.rootVisualElement == null)
            {
                return false;
            }

            VisualElement row = _hudDocument.rootVisualElement.Q<VisualElement>(RegionNpcTalkButton.ActionRowName);
            if (row == null)
            {
                return false;
            }

            if (_button != null && _button.parent == row)
            {
                return true;
            }

            // First time, or the HUD document rebuilt its tree (re-enable): (re)inject.
            if (_button != null)
            {
                _button.clicked -= OnButtonClicked;
                _button.RemoveFromHierarchy();
            }

            _button = new Button { name = ButtonName, focusable = true };
            _button.AddToClassList("wm-explore-button");
            _button.AddToClassList(ButtonName);
            // Colours come from .wm-village-button in WorldMapHudView.uss (Rapadura theme), not inline.
            _button.style.marginLeft = 8f;
            _button.style.display = DisplayStyle.None;
            _button.clicked += OnButtonClicked;
            row.Add(_button);
            return true;
        }

        private void OnButtonClicked()
        {
            if (GameplayPause.IsPaused || (ServiceLocator.TryGet(out DialogueManager dialogue) && dialogue.IsActive))
            {
                return;
            }

            OpenHub();
        }

        // ------------------------------------------------------------------
        // Panel
        // ------------------------------------------------------------------

        public void OpenHub()
        {
            if (_hubOpen || !EnsureOverlay())
            {
                return;
            }

            _hubOpen = true;
            RefreshHubTexts();
            _overlay.style.display = DisplayStyle.Flex;
            GameplayPause.Acquire(this);
            if (_npcButtons.Count > 0)
            {
                _npcButtons[0].Focus();
            }
        }

        public void CloseHub()
        {
            if (_overlay != null)
            {
                _overlay.style.display = DisplayStyle.None;
            }

            _hubOpen = false;
            GameplayPause.Release(this);
        }

        private bool EnsureOverlay()
        {
            if (_hudDocument == null || _hudDocument.rootVisualElement == null)
            {
                return false;
            }

            VisualElement root = _hudDocument.rootVisualElement.Q<VisualElement>("wm-hud-root") ?? _hudDocument.rootVisualElement;
            if (_overlay != null && _overlay.parent == root)
            {
                return true;
            }

            _overlay?.RemoveFromHierarchy();
            _npcButtons.Clear();

            // Same Rapadura-theme look as the HUD's Mochila summary (WorldMapHudView.uss, under the
            // HUD root's rap-theme), a bit wider for two columns; .wm-village-* rules add the section
            // headers and the themed option buttons.
            _overlay = new VisualElement { name = OverlayName };
            _overlay.AddToClassList("wm-backpack-overlay");
            _overlay.style.display = DisplayStyle.None;

            var panel = new VisualElement { name = "wm-village-panel" };
            panel.AddToClassList("rap-panel");
            panel.AddToClassList("wm-backpack-panel");
            panel.AddToClassList("wm-village-panel");
            _overlay.Add(panel);

            _titleLabel = AddLabel(panel, "wm-backpack-title");
            _subtitleLabel = AddLabel(panel, "wm-backpack-hint");
            _talkHeader = AddLabel(panel, "wm-backpack-hero");
            _talkHeader.AddToClassList(VillageSectionClass);

            VisualElement npcGrid = AddGrid(panel);
            foreach (VillageNpcSpec npc in VillageNpcCatalog.Npcs)
            {
                VillageNpcSpec captured = npc;
                Button npcButton = AddGridButton(npcGrid, "wm-village-npc-" + npc.NpcKey, () => OnNpcClicked(captured));
                _npcButtons.Add(npcButton);
            }

            _servicesHeader = AddLabel(panel, "wm-backpack-hero");
            _servicesHeader.AddToClassList(VillageSectionClass);
            VisualElement servicesGrid = AddGrid(panel);
            _shopButton = AddGridButton(servicesGrid, "wm-village-shop-button", OnShopClicked);
            _craftingButton = AddGridButton(servicesGrid, "wm-village-crafting-button", OnCraftingClicked);
            _chestButton = AddGridButton(servicesGrid, "wm-village-chest-button", OnChestClicked);
            _kitchenButton = AddGridButton(servicesGrid, "wm-village-kitchen-button", OnKitchenClicked);

            _closeButton = new Button(CloseHub) { name = "wm-village-close-button", focusable = true };
            _closeButton.AddToClassList("rap-button");
            _closeButton.AddToClassList("rap-button--primary");
            _closeButton.AddToClassList("wm-backpack-close");
            _closeButton.style.marginTop = 8f;
            panel.Add(_closeButton);

            root.Add(_overlay);
            return true;
        }

        private static Label AddLabel(VisualElement parent, string className)
        {
            var label = new Label();
            label.AddToClassList(className);
            parent.Add(label);
            return label;
        }

        private static VisualElement AddGrid(VisualElement parent)
        {
            var grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            grid.style.justifyContent = Justify.SpaceBetween;
            parent.Add(grid);
            return grid;
        }

        private static Button AddGridButton(VisualElement grid, string name, System.Action onClick)
        {
            var button = new Button(onClick) { name = name, focusable = true };
            button.AddToClassList("rap-button");
            button.AddToClassList(VillageOptionClass);
            grid.Add(button);
            return button;
        }

        private void RefreshHubTexts()
        {
            if (_overlay == null)
            {
                return;
            }

            LocalizationManager localization = Localization;
            SetText(_titleLabel, VillageHubTexts.Get(localization, VillageHubTexts.Title));
            SetText(_subtitleLabel, VillageHubTexts.Get(localization, VillageHubTexts.Subtitle));
            SetText(_talkHeader, VillageHubTexts.Get(localization, VillageHubTexts.TalkHeader));
            SetText(_servicesHeader, VillageHubTexts.Get(localization, VillageHubTexts.ServicesHeader));
            SetText(_shopButton, VillageHubTexts.Get(localization, VillageHubTexts.Shop));
            SetText(_craftingButton, VillageHubTexts.Get(localization, VillageHubTexts.Crafting));
            SetText(_chestButton, VillageChestTexts.Get(localization, VillageChestTexts.HubButton));
            SetText(_kitchenButton, KitchenTexts.Get(localization, KitchenTexts.HubButton));
            SetText(_closeButton, VillageHubTexts.Get(localization, VillageHubTexts.Close));

            for (int i = 0; i < _npcButtons.Count && i < VillageNpcCatalog.Npcs.Count; i++)
            {
                VillageNpcSpec npc = VillageNpcCatalog.Npcs[i];
                bool news = VillageNpcCatalog.HasNews(_questContext.GetQuestState(npc.QuestId));
                SetText(_npcButtons[i], BuildNpcLabel(npc, news, localization));
            }
        }

        private static void SetText(TextElement element, string text)
        {
            if (element != null && element.text != text)
            {
                element.text = text;
            }
        }

        // ------------------------------------------------------------------
        // Conversar
        // ------------------------------------------------------------------

        private void OnNpcClicked(VillageNpcSpec npc)
        {
            if (!ServiceLocator.TryGet(out DialogueManager dialogue) || dialogue.IsActive)
            {
                return;
            }

            DialogueDefinition definition = VillageNpcCatalog.LoadDialogue(npc);
            if (definition == null)
            {
                Debug.LogWarning($"[VillageHubButton] Missing dialogue Resources/{npc.DialogueResourcePath}.");
                return;
            }

            RegionNpcTalkButton talk = GetComponent<RegionNpcTalkButton>();
            if (talk != null)
            {
                talk.EnsureDialogueView();
            }

            CloseHub();
            _returnToHub = true;
            dialogue.StartDialogue(definition);
        }

        // ------------------------------------------------------------------
        // Loja / Bancada
        // ------------------------------------------------------------------

        private void OnShopClicked()
        {
            OpenShop();
            if (ShopOpen)
            {
                CloseHub();
                _returnToHub = true;
            }
        }

        private void OnCraftingClicked()
        {
            OpenCrafting();
            if (CraftingOpen)
            {
                CloseHub();
                _returnToHub = true;
            }
        }

        private void OpenShop()
        {
            if (!ServiceLocator.TryGet(out ShopManager shops))
            {
                return;
            }

            var shop = Resources.Load<ShopDefinition>(ShopResourcePath);
            _shopUI = _shopUI != null ? _shopUI : EnsureServiceView<ShopUIController>(ShopViewResourcePath);
            if (shop == null || _shopUI == null)
            {
                Debug.LogWarning($"[VillageHubButton] Cannot open the shop (Resources/{ShopResourcePath} or Resources/{ShopViewResourcePath}.uxml missing).");
                return;
            }

            InventoryManager inventory = BeginInventorySession();
            if (shops.IsOpen)
            {
                shops.CloseShop();
            }

            shops.OpenShop(shop, inventory);
        }

        private void OpenCrafting()
        {
            _craftingUI = _craftingUI != null ? _craftingUI : EnsureServiceView<CraftingUIController>(CraftingViewResourcePath);
            if (_craftingUI == null)
            {
                Debug.LogWarning($"[VillageHubButton] Cannot open the workbench (Resources/{CraftingViewResourcePath}.uxml missing).");
                return;
            }

            _craftingUI.Open(BeginInventorySession(), Station);
        }

        // ------------------------------------------------------------------
        // Baú
        // ------------------------------------------------------------------

        private void OnChestClicked()
        {
            OpenChest();
            if (ChestOpen)
            {
                CloseHub();
                _returnToHub = true;
            }
        }

        private void OpenChest()
        {
            _chestUI = _chestUI != null ? _chestUI : EnsureServiceView<VillageChestUIController>(ChestViewResourcePath);
            if (_chestUI == null)
            {
                Debug.LogWarning($"[VillageHubButton] Cannot open the chest (Resources/{ChestViewResourcePath}.uxml missing).");
                return;
            }

            VillageChestStorage chest = VillageChestStorage.ResolveShared();
            ServiceLocator.TryGet(out Rapadura.Gameplay.Quests.QuestManager quests);
            _chestUI.Open(BeginInventorySession(), chest, quests != null ? quests.GetOutstandingCollectAmount : (System.Func<string, int, int>)null);
        }

        // ------------------------------------------------------------------
        // Fogão a lenha
        // ------------------------------------------------------------------

        private void OnKitchenClicked()
        {
            OpenKitchen();
            if (KitchenOpen)
            {
                CloseHub();
                _returnToHub = true;
            }
        }

        private void OpenKitchen()
        {
            // Built in code on this HUD's document (no UXML), so it lives on the HUD's GameObject.
            _kitchenUI = _kitchenUI != null ? _kitchenUI : (GetComponent<VillageKitchenController>() ?? gameObject.AddComponent<VillageKitchenController>());
            _kitchenUI.Open(BeginInventorySession());
        }

        private InventoryManager BeginInventorySession()
        {
            if (_session == null)
            {
                ServiceLocator.TryGet(out SaveManager saveManager);
                _session = new CarriedInventorySession(saveManager);
            }

            return _session.Open(transform);
        }

        private void EndInventorySession()
        {
            if (_session == null || !_session.IsOpen)
            {
                return;
            }

            // The shop must not keep a buyer that is about to be destroyed.
            if (ServiceLocator.TryGet(out ShopManager shops) && shops.IsOpen && shops.BuyerInventory == _session.Inventory)
            {
                shops.CloseShop();
            }

            _session.Close();
        }

        private void OnShopClosed(ShopClosedEvent evt)
        {
            if (!CraftingOpen && !ChestOpen && !KitchenOpen)
            {
                EndInventorySession();
            }
        }

        /// <summary>Builds a service window for this scene from Resources (WorldMap.unity ships without one), or reuses the scene's.</summary>
        private T EnsureServiceView<T>(string resourcePath) where T : MonoBehaviour
        {
            T existing = FindFirstObjectByType<T>(FindObjectsInactive.Include);
            if (existing != null)
            {
                return existing;
            }

            var view = Resources.Load<VisualTreeAsset>(resourcePath);
            if (view == null || _hudDocument == null || _hudDocument.panelSettings == null)
            {
                return null;
            }

            // Built inactive so UIDocument has its tree before the controller's OnEnable binds it.
            var host = new GameObject("WorldMap" + typeof(T).Name);
            host.SetActive(false);
            host.transform.SetParent(transform.parent, false);
            var document = host.AddComponent<UIDocument>();
            document.panelSettings = _hudDocument.panelSettings;
            document.visualTreeAsset = view;
            document.sortingOrder = ServiceSortingOrder;
            var controller = host.AddComponent<T>();
            host.SetActive(true);

            // The wrapper's <ui:Instance> must not catch clicks while the window inside is hidden.
            foreach (VisualElement child in document.rootVisualElement.Children())
            {
                child.pickingMode = PickingMode.Ignore;
            }

            Rapadura.Audio.UiClickSoundBinder.Register(document);
            Rapadura.UI.Common.UiAccessibilityApplier.RequestRefresh();
            return controller;
        }
    }
}
