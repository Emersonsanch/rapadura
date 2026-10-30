using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
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
    /// The city hub on the world map: every region has a city (<see cref="CityCatalog"/>) and they all open this same
    /// panel with that city's residents and services. In the Floresta it is still the "Vila" of Vó Zefa: the village
    /// services of Main.unity (personal-story villagers, Maria's gathering point, the shop and the workbench) are
    /// switched off during encounters, the only time that scene is loaded, so they are offered here instead.
    /// Self-bootstrapping like <see cref="RegionNpcTalkButton"/>, no scene edit:
    /// <list type="bullet">
    /// <item>Added to the <see cref="WorldMapHudController"/>'s GameObject after every scene load; injects a
    /// button into the HUD's <c>wm-action-row</c> and a modal panel under <c>wm-hud-root</c>, so the HUD's
    /// pointer blocker covers both. The panel and the service windows join the HUD's screens
    /// (<see cref="WorldMapHudController.RegisterScreen"/>): one at a time, Esc closes them.</item>
    /// <item>Button shown while the HUD is visible, the token is idle in a region with a city and nothing holds
    /// GameplayPause; labelled with the city ("Vila", "Areia Branca"...), "(!)" when a story villager has a quest to give
    /// or hand in.</item>
    /// <item>Conversar: in the Vila, the same <c>Resources/Dialogue</c> graphs as Main.unity's NPCs, through
    /// <see cref="DialogueManager"/> (rewards of a quest finished here wait in
    /// <see cref="Rapadura.Gameplay.Quests.QuestManager.DeliverPendingRewards"/>); in every city, the residents'
    /// line for the cousin in play (<see cref="CityTexts.ResidentSpeech"/>), shown on the panel.</item>
    /// <item>Lojas: the city's shops (<see cref="CitySpec.ShopIds"/>, plus Seu Bento's on the day his mule is there),
    /// resolved by <see cref="CityShopService"/> into the <see cref="ShopManager"/>'s definitions (today's rotating
    /// counter); Bancada: <see cref="CraftingManager"/>. Their windows are created on first use from
    /// <c>Resources/UI/Village*View.uxml</c> (instances of the Main.unity views), working on a live
    /// <see cref="CarriedInventorySession"/> that hands the inventory back to the carry as soon as the windows close.</item>
    /// <item>Baú da Vó Zefa and Fogão a lenha (<see cref="VillageKitchenController"/>): only in her village
    /// (<see cref="CitySpec.HasGrandmaServices"/>).</item>
    /// <item>Carro de boi (<see cref="CityFastTravelPanel"/>): to another city already visited, for a little gold.</item>
    /// <item>Closing a conversation or service window returns to the panel, unless something else (pause
    /// menu) took over meanwhile.</item>
    /// </list>
    /// </summary>
    [DisallowMultipleComponent]
    public class VillageHubButton : MonoBehaviour
    {
        public const string ButtonName = "wm-village-button";
        /// <summary>The hub's "Andar pela cidade" option (walkable town).</summary>
        public const string WalkInButtonName = "wm-village-walk-in-button";
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
        /// <summary>"Carro de boi" window (<see cref="CityFastTravelPanel"/>).</summary>
        public const string FastTravelScreenId = "village_fast_travel";
        /// <summary>"Ferreiro: refinar" window (<see cref="VillageRefinePanel"/>).</summary>
        public const string RefineScreenId = "village_refine";
        /// <summary>"Tacho Encantado da Vó" window (<see cref="VillageTachoPanel"/>), only in her village.</summary>
        public const string TachoScreenId = "village_tacho";
        public const string ShopViewResourcePath = "UI/VillageShopView";
        public const string CraftingViewResourcePath = "UI/VillageCraftingView";
        /// <summary>"Baú da Vó Zefa" window (<see cref="VillageChestUIController"/>).</summary>
        public const string ChestViewResourcePath = "UI/VillageChestView";
        /// <summary>The Vila's old general store (still the asset it always was).</summary>
        public const string ShopResourcePath = "Shops/shop_general_store";
        public const string ShopButtonPrefix = "wm-village-shop-";
        public const string ResidentButtonPrefix = "wm-city-resident-";
        public const string SpeechName = "wm-city-speech";
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
        private Label _speechLabel;
        private Label _servicesHeader;
        private Label _peddlerLabel;
        private VisualElement _npcGrid;
        private VisualElement _servicesGrid;
        private Button _craftingButton;
        private Button _chestButton;
        private Button _kitchenButton;
        private Button _fastTravelButton;
        private Button _refineButton;
        private Button _tachoButton;
        /// <summary>"Oratório": the Bênção da Romaria (<see cref="RomariaOratoryHub"/>); the keeper answers on the speech line.</summary>
        private Button _oratoryButton;
        /// <summary>"Andar pela cidade": loads the walkable town (Docs/Cities.md §5, SceneTransitionService.BeginCityVisit).</summary>
        private Button _walkInButton;
        private string _oratorySpeech;
        private Button _closeButton;
        private readonly List<Button> _npcButtons = new List<Button>();
        private readonly List<VillageNpcSpec> _npcSpecs = new List<VillageNpcSpec>();
        private readonly List<(Button Button, CityResidentSpec Resident)> _residentButtons = new List<(Button, CityResidentSpec)>();
        private readonly List<(Button Button, string ShopId)> _shopButtons = new List<(Button, string)>();
        private CitySpec _city;
        private string _contentSignature;
        private CityResidentSpec _speaking;
        private string _lastShopId;
        private bool _hubOpen;
        private bool _returnToHub;
        private bool _screensRegistered;
        private float _nextRefresh;

        private ShopUIController _shopUI;
        private CraftingUIController _craftingUI;
        private VillageChestUIController _chestUI;
        private VillageKitchenController _kitchenUI;
        private CityFastTravelPanel _fastTravelUI;
        private VillageRefinePanel _refineUI;
        private VillageTachoPanel _tachoUI;
        private CarriedInventorySession _session;
        private readonly QuestDialogueContext _questContext = new QuestDialogueContext();

        public bool IsHubOpen => _hubOpen;

        /// <summary>The city the panel shows (null before it was opened).</summary>
        public CitySpec CurrentCity => _city;

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
                ClearCityContent();
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

        /// <summary>Whether the city button should be on screen (every region with a city, <see cref="CityCatalog.HasCity"/>).</summary>
        public static bool ShouldShow(bool hudVisible, bool tokenIdle, bool paused, bool dialogueActive, BiomeType? currentRegion)
        {
            return hudVisible && tokenIdle && !paused && !dialogueActive && currentRegion.HasValue && CityCatalog.HasCity(currentRegion.Value);
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

        /// <summary>Label of the HUD button: the city's short name ("Vila" in the Floresta), with "(!)" on news.</summary>
        public static string BuildButtonLabel(CitySpec city, bool hasNews, LocalizationManager localization)
        {
            if (city == null)
            {
                return VillageHubTexts.Get(localization, hasNews ? VillageHubTexts.ButtonNews : VillageHubTexts.Button);
            }

            string label = CityTexts.Get(localization, city.ButtonKey);
            return hasNews ? CityTexts.Format(localization, CityTexts.ButtonNews, label) : label;
        }

        /// <summary>The story villagers shown in <paramref name="city"/> (only in the Vila).</summary>
        public static IReadOnlyList<VillageNpcSpec> StoryVillagersOf(CitySpec city)
        {
            return city != null && city.HasStoryVillagers ? VillageNpcCatalog.Npcs : (IReadOnlyList<VillageNpcSpec>)new List<VillageNpcSpec>();
        }

        /// <summary>Residents to chat with in <paramref name="city"/>: its own, plus Seu Bento when his shop is open there.</summary>
        public static List<CityResidentSpec> ResidentsOf(CitySpec city, IReadOnlyList<string> openShops)
        {
            var residents = new List<CityResidentSpec>();
            if (city == null)
            {
                return residents;
            }

            residents.AddRange(city.Residents);
            if (openShops != null && openShops.Contains(CityShopCatalog.MascateBento))
            {
                residents.Add(CityCatalog.Mascate);
            }

            return residents;
        }

        // ------------------------------------------------------------------
        // Runtime
        // ------------------------------------------------------------------

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        private bool ShopOpen => _shopUI != null && _shopUI.IsOpen;
        private bool CraftingOpen => _craftingUI != null && _craftingUI.IsOpen;
        private bool ChestOpen => _chestUI != null && _chestUI.IsOpen;
        private bool KitchenOpen => _kitchenUI != null && _kitchenUI.IsOpen;
        private bool FastTravelOpen => _fastTravelUI != null && _fastTravelUI.IsOpen;
        private bool RefineOpen => _refineUI != null && _refineUI.IsOpen;
        private bool TachoOpen => _tachoUI != null && _tachoUI.IsOpen;

        private BiomeType? CurrentRegion => _worldMap != null && _worldMap.CurrentNode != null ? _worldMap.CurrentNode.Region : (BiomeType?)null;

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
            _hud.RegisterScreen(FastTravelScreenId, () => FastTravelOpen, OpenFastTravel, () => { if (_fastTravelUI != null) _fastTravelUI.Close(); });
            _hud.RegisterScreen(RefineScreenId, () => RefineOpen, OpenRefine, () => { if (_refineUI != null) _refineUI.Close(); });
            _hud.RegisterScreen(TachoScreenId, () => TachoOpen, OpenTacho, () => { if (_tachoUI != null) _tachoUI.Close(); });
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

            if (ShouldEndInventorySession(_session != null && _session.IsOpen, ShopOpen, CraftingOpen, ChestOpen || KitchenOpen || FastTravelOpen || RefineOpen || TachoOpen))
            {
                EndInventorySession();
            }

            if (_hubOpen && !hudVisible)
            {
                CloseHub();
            }

            ConsumeFastTravelRequest(hudVisible);

            // Decided once the conversation/window is over: back to the panel, or (something else took
            // over) forget it.
            bool serviceOpen = ShopOpen || CraftingOpen || ChestOpen || KitchenOpen || FastTravelOpen || RefineOpen || TachoOpen;
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

            BiomeType? region = CurrentRegion;
            bool show = ShouldShow(hudVisible, _worldMap != null && _worldMap.CanExplore, GameplayPause.IsPaused, dialogueActive, region);
            _button.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (!show)
            {
                return;
            }

            CitySpec city = CityCatalog.ForRegion(region);
            string label = BuildButtonLabel(city, AnyNews(city), Localization);
            if (_button.text != label)
            {
                _button.text = label;
            }
        }

        private bool AnyNews(CitySpec city)
        {
            foreach (VillageNpcSpec npc in StoryVillagersOf(city))
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
            CitySpec city = CityCatalog.ForRegion(CurrentRegion) ?? CityCatalog.ForRegion(VillageNpcCatalog.VillageRegion);
            if (_hubOpen || city == null || !EnsureOverlay())
            {
                return;
            }

            if (city != _city)
            {
                _speaking = null;
                _oratorySpeech = null;
            }

            _city = city;
            CityTexts.EnsureRegistered(Localization);
            RebuildCityContent();
            _hubOpen = true;
            RefreshHubTexts();
            _overlay.style.display = DisplayStyle.Flex;
            GameplayPause.Acquire(this);
            Button first = _npcButtons.Count > 0 ? _npcButtons[0] : _residentButtons.Count > 0 ? _residentButtons[0].Button : _closeButton;
            first?.Focus();
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
            ClearCityContent();

            // Same Rapadura-theme look as the HUD's Mochila summary (WorldMapHudView.uss, under the
            // HUD root's rap-theme), a bit wider for two columns; .wm-village-* rules add the section
            // headers and the themed option buttons, CityHub.uss the resident speech line.
            _overlay = new VisualElement { name = OverlayName };
            _overlay.AddToClassList("wm-backpack-overlay");
            _overlay.style.display = DisplayStyle.None;
            var sheet = Resources.Load<StyleSheet>(CityFastTravelPanel.StyleResourcePath);
            if (sheet != null)
            {
                _overlay.styleSheets.Add(sheet);
            }

            var panel = new VisualElement { name = "wm-village-panel" };
            panel.AddToClassList("rap-panel");
            panel.AddToClassList("wm-backpack-panel");
            panel.AddToClassList("wm-village-panel");
            _overlay.Add(panel);

            var scroll = new ScrollView(ScrollViewMode.Vertical) { name = "wm-village-scroll" };
            scroll.style.flexShrink = 1f;
            panel.Add(scroll);

            _titleLabel = AddLabel(scroll, "wm-backpack-title");
            _subtitleLabel = AddLabel(scroll, "wm-backpack-hint");
            _peddlerLabel = AddLabel(scroll, "city-peddler");
            _talkHeader = AddLabel(scroll, "wm-backpack-hero");
            _talkHeader.AddToClassList(VillageSectionClass);
            _npcGrid = AddGrid(scroll);
            _speechLabel = AddLabel(scroll, "city-speech");
            _speechLabel.name = SpeechName;

            _servicesHeader = AddLabel(scroll, "wm-backpack-hero");
            _servicesHeader.AddToClassList(VillageSectionClass);
            _servicesGrid = AddGrid(scroll);
            _craftingButton = NewOptionButton("wm-village-crafting-button", OnCraftingClicked);
            _chestButton = NewOptionButton("wm-village-chest-button", OnChestClicked);
            _kitchenButton = NewOptionButton("wm-village-kitchen-button", OnKitchenClicked);
            _fastTravelButton = NewOptionButton("wm-village-fast-travel-button", OnFastTravelClicked);
            _refineButton = NewOptionButton("wm-village-refine-button", OnRefineClicked);
            _tachoButton = NewOptionButton("wm-village-tacho-button", OnTachoClicked);
            _oratoryButton = NewOptionButton("wm-village-oratory-button", OnOratoryClicked);
            _walkInButton = NewOptionButton(WalkInButtonName, OnWalkInClicked);

            _closeButton = new Button(CloseHub) { name = "wm-village-close-button", focusable = true };
            _closeButton.AddToClassList("rap-button");
            _closeButton.AddToClassList("rap-button--primary");
            _closeButton.AddToClassList("wm-backpack-close");
            _closeButton.style.marginTop = 8f;
            panel.Add(_closeButton);

            root.Add(_overlay);
            return true;
        }

        /// <summary>(Re)fills the residents and services grids for <see cref="_city"/> and today's open shops.</summary>
        private void RebuildCityContent()
        {
            if (_npcGrid == null || _servicesGrid == null || _city == null)
            {
                return;
            }

            List<string> shops = CityShopService.ShopsOpenIn(_city, CityShopService.Now, _worldMap != null ? _worldMap.Progress : null);
            string signature = _city.CityId + "|" + string.Join(",", shops);
            if (signature == _contentSignature)
            {
                return;
            }

            _contentSignature = signature;
            _npcGrid.Clear();
            _npcButtons.Clear();
            _npcSpecs.Clear();
            _residentButtons.Clear();
            _servicesGrid.Clear();
            _shopButtons.Clear();

            foreach (VillageNpcSpec npc in StoryVillagersOf(_city))
            {
                VillageNpcSpec captured = npc;
                Button npcButton = NewOptionButton("wm-village-npc-" + npc.NpcKey, () => OnNpcClicked(captured));
                _npcGrid.Add(npcButton);
                _npcButtons.Add(npcButton);
                _npcSpecs.Add(npc);
            }

            foreach (CityResidentSpec resident in ResidentsOf(_city, shops))
            {
                CityResidentSpec captured = resident;
                Button residentButton = NewOptionButton(ResidentButtonPrefix + resident.ResidentId, () => OnResidentClicked(captured));
                _npcGrid.Add(residentButton);
                _residentButtons.Add((residentButton, resident));
            }

            foreach (string shopId in shops)
            {
                string captured = shopId;
                Button shopButton = NewOptionButton(ShopButtonPrefix + shopId, () => OnShopClicked(captured));
                _servicesGrid.Add(shopButton);
                _shopButtons.Add((shopButton, shopId));
            }

            _servicesGrid.Add(_walkInButton);
            _servicesGrid.Add(_craftingButton);
            _servicesGrid.Add(_refineButton);
            if (_city.HasGrandmaServices)
            {
                _servicesGrid.Add(_chestButton);
                _servicesGrid.Add(_kitchenButton);
                _servicesGrid.Add(_tachoButton);
            }

            if (Rapadura.Gameplay.Inventory.RomariaOratory.ForCity(_city.CityId) != null)
            {
                _servicesGrid.Add(_oratoryButton);
            }

            _servicesGrid.Add(_fastTravelButton);
        }

        private void ClearCityContent()
        {
            _npcButtons.Clear();
            _npcSpecs.Clear();
            _residentButtons.Clear();
            _shopButtons.Clear();
            _contentSignature = null;
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

        private static Button NewOptionButton(string name, System.Action onClick)
        {
            var button = new Button(onClick) { name = name, focusable = true };
            button.AddToClassList("rap-button");
            button.AddToClassList(VillageOptionClass);
            return button;
        }

        private static CharacterId? Hero => ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;

        private void RefreshHubTexts()
        {
            if (_overlay == null || _city == null)
            {
                return;
            }

            LocalizationManager localization = Localization;
            SetText(_titleLabel, CityTexts.CityName(localization, _city));
            SetText(_subtitleLabel, CityTexts.Get(localization, _city.FlavourKey));
            SetText(_talkHeader, VillageHubTexts.Get(localization, VillageHubTexts.TalkHeader));
            SetText(_servicesHeader, VillageHubTexts.Get(localization, VillageHubTexts.ServicesHeader));
            SetText(_craftingButton, VillageHubTexts.Get(localization, VillageHubTexts.Crafting));
            SetText(_chestButton, VillageChestTexts.Get(localization, VillageChestTexts.HubButton));
            SetText(_kitchenButton, KitchenTexts.Get(localization, KitchenTexts.HubButton));
            SetText(_fastTravelButton, CityTexts.Get(localization, CityTexts.FastTravel));
            SetText(_refineButton, Rapadura.Gameplay.Refining.RefineTexts.Get(localization, Rapadura.Gameplay.Refining.RefineTexts.HubButton));
            SetText(_tachoButton, Rapadura.Gameplay.Tacho.TachoTexts.Get(localization, Rapadura.Gameplay.Tacho.TachoTexts.HubButton));
            SetText(_oratoryButton, RomariaOratoryHub.ButtonText(localization, _city));
            SetText(_closeButton, VillageHubTexts.Get(localization, VillageHubTexts.Close));
            SetText(_walkInButton, Rapadura.Gameplay.World.Cities.Town.CityWalkTexts.Get(localization, Rapadura.Gameplay.World.Cities.Town.CityWalkTexts.WalkIn));

            bool peddler = false;
            foreach ((Button button, string shopId) in _shopButtons)
            {
                SetText(button, CityTexts.ShopName(localization, shopId));
                peddler |= shopId == CityShopCatalog.MascateBento;
            }

            SetText(_peddlerLabel, peddler ? CityTexts.Get(localization, CityTexts.PeddlerToday) : string.Empty);
            _peddlerLabel.style.display = peddler ? DisplayStyle.Flex : DisplayStyle.None;

            for (int i = 0; i < _npcButtons.Count && i < _npcSpecs.Count; i++)
            {
                VillageNpcSpec npc = _npcSpecs[i];
                bool news = VillageNpcCatalog.HasNews(_questContext.GetQuestState(npc.QuestId));
                SetText(_npcButtons[i], BuildNpcLabel(npc, news, localization));
            }

            foreach ((Button button, CityResidentSpec resident) in _residentButtons)
            {
                SetText(button, CityTexts.ResidentName(localization, resident));
            }

            string speech = _oratorySpeech ?? (_speaking != null ? CityTexts.ResidentSpeech(localization, _speaking, Hero) : string.Empty);
            SetText(_speechLabel, speech);
            _speechLabel.style.display = !string.IsNullOrEmpty(speech) ? DisplayStyle.Flex : DisplayStyle.None;
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

        /// <summary>A resident's line for the cousin in play, on the panel (no dialogue graph).</summary>
        private void OnResidentClicked(CityResidentSpec resident)
        {
            _speaking = resident;
            _oratorySpeech = null;
            RefreshHubTexts();
        }

        // ------------------------------------------------------------------
        // Lojas / Bancada
        // ------------------------------------------------------------------

        private void OnShopClicked(string shopId)
        {
            OpenShop(shopId);
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

        /// <summary>Screen "open" action: the shop used last in this city, else the city's first.</summary>
        private void OpenShop()
        {
            CitySpec city = _city ?? CityCatalog.ForRegion(CurrentRegion);
            string shopId = _lastShopId;
            if (city == null || shopId == null || (!city.ShopIds.Contains(shopId) && shopId != CityShopCatalog.MascateBento))
            {
                shopId = city != null && city.ShopIds.Count > 0 ? city.ShopIds[0] : CityShopCatalog.GeneralStore;
            }

            OpenShop(shopId);
        }

        private void OpenShop(string shopId)
        {
            if (!ServiceLocator.TryGet(out ShopManager shops))
            {
                return;
            }

            CityTexts.EnsureRegistered(Localization);
            if (shops.IsOpen)
            {
                shops.CloseShop();
            }

            ShopDefinition shop = CityShopService.Resolve(shopId, CityProgress.ResolveShared(), shops, _worldMap != null ? _worldMap.Progress : null);
            _shopUI = _shopUI != null ? _shopUI : EnsureServiceView<ShopUIController>(ShopViewResourcePath);
            if (shop == null || _shopUI == null)
            {
                Debug.LogWarning($"[VillageHubButton] Cannot open shop '{shopId}' (definition or Resources/{ShopViewResourcePath}.uxml missing).");
                return;
            }

            InventoryManager inventory = BeginInventorySession();
            _lastShopId = shopId;
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

        // ------------------------------------------------------------------
        // Carro de boi
        // ------------------------------------------------------------------

        /// <summary>"Andar pela cidade": hands the visit to the scene service (Main.unity in city mode) and closes the panel.</summary>
        private void OnWalkInClicked()
        {
            if (_city == null || !ServiceLocator.TryGet(out SceneTransitionService transition) || _worldMap == null || !_worldMap.CanExplore)
            {
                return;
            }

            CloseHub();
            _returnToHub = false;
            EndInventorySession();
            transition.BeginCityVisit(_city.CityId);
        }

        /// <summary>Back from the Cais da Barca of a walkable city: open the carro de boi / barcas window once the map is idle.</summary>
        private void ConsumeFastTravelRequest(bool hudVisible)
        {
            if (!hudVisible || GameplayPause.IsPaused || _worldMap == null || !_worldMap.CanExplore
                || !ServiceLocator.TryGet(out SceneTransitionService transition) || !transition.OpenFastTravelOnMapReturn)
            {
                return;
            }

            transition.OpenFastTravelOnMapReturn = false;
            OpenFastTravel();
        }

        private void OnFastTravelClicked()
        {
            OpenFastTravel();
            if (FastTravelOpen)
            {
                CloseHub();
                _returnToHub = true;
            }
        }

        // ------------------------------------------------------------------
        // Ferreiro: refinar (Pedras do Sertão)
        // ------------------------------------------------------------------

        /// <summary>Oratório: buys the Bênção da Romaria from the carried bag; the keeper's answer replaces the speech line.</summary>
        private void OnOratoryClicked()
        {
            InventoryManager bag = BeginInventorySession();
            _oratorySpeech = RomariaOratoryHub.Buy(_city, bag, Localization);
            EndInventorySession();
            _speaking = null;
            RefreshHubTexts();
        }

        private void OnRefineClicked()
        {
            OpenRefine();
            if (RefineOpen)
            {
                CloseHub();
                _returnToHub = true;
            }
        }

        private void OpenRefine()
        {
            // Built in code on this HUD's document (no UXML), like the carro de boi.
            _refineUI = _refineUI != null ? _refineUI : (GetComponent<VillageRefinePanel>() ?? gameObject.AddComponent<VillageRefinePanel>());
            _refineUI.Open(BeginInventorySession());
        }

        // ------------------------------------------------------------------
        // Tacho Encantado da Vó (wings, Proteção do Caos, Frutas do Sertão)
        // ------------------------------------------------------------------

        private void OnTachoClicked()
        {
            OpenTacho();
            if (TachoOpen)
            {
                CloseHub();
                _returnToHub = true;
            }
        }

        private void OpenTacho()
        {
            // Built in code on this HUD's document (no UXML), like the forge window.
            _tachoUI = _tachoUI != null ? _tachoUI : (GetComponent<VillageTachoPanel>() ?? gameObject.AddComponent<VillageTachoPanel>());
            _tachoUI.Open(BeginInventorySession());
        }

        private void OpenFastTravel()
        {
            if (_worldMap == null)
            {
                return;
            }

            _fastTravelUI = _fastTravelUI != null ? _fastTravelUI : (GetComponent<CityFastTravelPanel>() ?? gameObject.AddComponent<CityFastTravelPanel>());
            _fastTravelUI.Open(BeginInventorySession(), _worldMap, OnFastTravelPaid);
        }

        /// <summary>
        /// The cart was paid: hand the bag back, then play the short trip (<see cref="FastTravelTransition"/>) and, with the
        /// screen covered, move the token to the destination's city and save.
        /// </summary>
        private void OnFastTravelPaid(BiomeType destination, int gold)
        {
            EndInventorySession();
            FastTravelTransition transition = GetComponent<FastTravelTransition>() ?? gameObject.AddComponent<FastTravelTransition>();
            LocalizationManager localization = Localization;
            string line = CityTexts.Format(localization, CityTexts.FastTravelOnTheWay, CityTexts.CityName(localization, CityCatalog.ForRegion(destination)));
            transition.Play(line, () => ArriveByCart(destination, gold));
        }

        private void ArriveByCart(BiomeType destination, int gold)
        {
            if (_worldMap == null || _worldMap.Progress == null || !_worldMap.Progress.SetCurrentRegion(destination))
            {
                return;
            }

            _worldMap.SyncWithProgress();
            CityProgress.ResolveShared().MarkVisited(destination);
            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.AutoSave();
            }

            CitySafeZoneHud notice = GetComponent<CitySafeZoneHud>();
            if (notice != null)
            {
                LocalizationManager localization = Localization;
                notice.ShowNotice(CityTexts.Format(localization, CityTexts.FastTravelDone, CityTexts.CityName(localization, CityCatalog.ForRegion(destination)), gold));
            }
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
            if (!CraftingOpen && !ChestOpen && !KitchenOpen && !FastTravelOpen && !RefineOpen && !TachoOpen)
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
