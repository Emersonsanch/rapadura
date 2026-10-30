using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Housing.Visuals;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Quests;
using Rapadura.Save;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Rapadura.UI.Housing
{
    /// <summary>
    /// Inside "Minha casa" (Housing.md §2.4, §3.4, §5): the 3D house (<see cref="HouseInteriorView"/>, own camera,
    /// cutaway) with a code-built overlay on the map HUD's document (<c>Resources/UI/House.uss</c>), plus the
    /// "Um Cantinho Seu" card the first time.
    /// <list type="bullet">
    /// <item>View: tap/click the chest (Baú da casa, the village chest window on the house's own storage) or a
    /// bed ("Dormir até amanhã?" → clock to 7:00, auto save); turn the camera 90°, look at the yard.</item>
    /// <item>"Arrumar a casa": layer tabs (Piso, Tapete, Parede, Teto, Superfície, Quintal) filter the Depósito; pick
    /// an entry or a placed piece, the ghost follows the pointer (green fits / red does not, with the reason),
    /// rotate, mirror (walls), confirm, put away. PC: mouse moves, Q/E turn, click places, Del puts away, right
    /// click cancels, wheel zooms. Touch: one finger drags, two fingers twist, big ✓ / ↺ buttons.</item>
    /// <item>All validity comes from the pure <see cref="HousePlacementRules"/> through <see cref="HouseLayout"/>.</item>
    /// <item>While inside: GameplayPause held, the map camera and the rest of the HUD hidden; Esc / the HUD's back
    /// closes the chest first, then leaves the house.</item>
    /// </list>
    /// </summary>
    [DisallowMultipleComponent]
    public class HouseScreenController : MonoBehaviour
    {
        public const string OverlayName = "wm-house-overlay";
        public const string StyleResourcePath = "UI/House";
        public const string ChestViewResourcePath = "UI/VillageChestView";
        public const int ServiceSortingOrder = 14;
        private const float TapSlopPixels = 14f;
        private const float TwistStepDegrees = 40f;

        private static readonly HouseLayer[] Tabs = { HouseLayer.Floor, HouseLayer.Rug, HouseLayer.Wall, HouseLayer.Ceiling, HouseLayer.Surface, HouseLayer.Yard };

        private sealed class Carry
        {
            public string ItemId;
            public int FromInstance;
            public HousePlacedData Target = new HousePlacedData();
            public bool HasTarget;
            public HousePlacementResult Result = HousePlacementResult.OutOfBounds;
        }

        private UIDocument _hudDocument;
        private VisualElement _overlay;
        private readonly List<(VisualElement Element, Visibility Visibility)> _hiddenHud = new List<(VisualElement, Visibility)>();

        private Label _title, _counts, _status;
        private VisualElement _toolbar;
        private Button _arrangeButton, _chestButton, _sleepButton, _turnLeftButton, _turnRightButton, _yardButton, _leaveButton;
        private Button _upgradeButton, _lifeButton;
        private HouseLifePanel _lifePanel;
        // H3 workbench (HouseWorkbenchPanel.cs).
        private HouseWorkbenchPanel _workbench;
        // H5 wardrobe (WardrobePanel.cs).
        private WardrobePanel _wardrobe;
        private TrainingDummyPanel _dummy;
        private HouseLifeDecor _decor;
        private VisualElement _arrangePanel;
        private Label _depotHeader, _depotEmpty, _hint;
        private readonly List<Button> _tabButtons = new List<Button>();
        private ScrollView _depotList;
        private Button _confirmButton, _rotateButton, _mirrorButton, _storeButton, _cancelButton;
        private VisualElement _card;
        private Label _cardTitle, _cardText, _cardReaction;
        private Button _cardYes, _cardNo;
        private System.Action _cardYesAction;

        private HouseService _house;
        private HouseInteriorView _view;
        private Camera _mapCamera;
        private bool _inside;
        private bool _arranging;
        private HouseLayer _tab = HouseLayer.Floor;
        private Carry _carry;
        private int _selected;
        private bool _dirtyView;
        private float _nextTextRefresh;

        private VillageChestUIController _chestUI;
        private CarriedInventorySession _session;
        private bool _chestOpenedHere;

        // Pointer state (UI Toolkit pointer events on the overlay surface).
        private readonly Dictionary<int, Vector2> _pointers = new Dictionary<int, Vector2>();
        private int _primaryPointer = -1;
        private Vector2 _pressPosition;
        private bool _dragged;
        private float _twistAngle;
        private float _twistAccum;
        private bool _lastPointerTouch;

        public bool IsInside => _inside;
        public bool IsCardOpen => _card != null && _card.style.display == DisplayStyle.Flex;
        public bool IsOpen => _inside || IsCardOpen;
        public bool IsArranging => _arranging;
        public HouseInteriorView View => _view;
        public HouseLifePanel LifePanel => _lifePanel;

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;
        private bool ChestOpen => _chestUI != null && _chestUI.IsOpen;

        private void Awake()
        {
            _hudDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<HouseChangedEvent>(OnHouseChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<HouseChangedEvent>(OnHouseChanged);
            Exit();
            CloseCard();
            _overlay?.RemoveFromHierarchy();
            _overlay = null;
            if (_view != null)
            {
                Destroy(_view.gameObject);
                _view = null;
            }
        }

        // ------------------------------------------------------------------
        // Pure rules (tested)
        // ------------------------------------------------------------------

        /// <summary>The origin cell that centres a <paramref name="width"/> × <paramref name="height"/> footprint on the pointed cell.</summary>
        public static Vector2Int CenteredOrigin(Vector2Int pointed, int width, int height)
        {
            return new Vector2Int(pointed.x - (width - 1) / 2, pointed.y - (height - 1) / 2);
        }

        /// <summary>The layer a carried item uses under the active tab: the tab's if the item allows it, else its own.</summary>
        public static HouseLayer LayerFor(HouseItemDefinition item, HouseLayer tab)
        {
            return item != null && item.Allows(tab) ? tab : HousePlacementRules.DefaultLayer(item);
        }

        /// <summary>A mouse click places right away; a tap only moves the ghost (✓ places), so a finger never drops a piece by accident.</summary>
        public static bool ClickConfirms(bool touch) => !touch;

        // ------------------------------------------------------------------
        // Enter / leave
        // ------------------------------------------------------------------

        /// <summary>"Minha casa": the Vó Edna card the first time (quest beat), else straight inside.</summary>
        public void OpenFromButton()
        {
            _house = HouseService.ResolveShared();
            if (!EnsureOverlay())
            {
                return;
            }

            if (!_house.HasHouse)
            {
                ShowQuestCard();
                return;
            }

            Enter();
        }

        private void ShowQuestCard()
        {
            LocalizationManager l = Localization;
            CharacterId? hero = ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;
            ShowCard(HouseTexts.Get(l, HouseTexts.QuestTitle), HouseTexts.Get(l, HouseTexts.QuestGrandma), HouseTexts.Get(l, HouseTexts.ReactionKey(hero)),
                HouseTexts.Get(l, HouseTexts.QuestAccept), HouseTexts.Get(l, HouseTexts.QuestLater), () =>
                {
                    _house.GrantHouse();
                    Enter();
                });
        }

        public void Enter()
        {
            _house ??= HouseService.ResolveShared();
            if (_inside || !_house.HasHouse || !EnsureOverlay())
            {
                return;
            }

            if (_view == null)
            {
                _view = HouseInteriorView.Create(_house);
            }

            if (_decor == null)
            {
                _decor = HouseLifeDecor.Attach(_view);
            }

            _inside = true;
            if (_view.BuiltLevel != _house.Level)
            {
                _view.RebuildHull();
            }

            _view.RebuildFurniture();
            _view.Show(true);
            _view.SetYardFocus(false);

            _mapCamera = Camera.main;
            if (_mapCamera != null && _mapCamera != _view.ViewCamera)
            {
                _mapCamera.enabled = false;
            }

            HideHud(true);
            _overlay.style.display = DisplayStyle.Flex;
            SetArranging(false);
            GameplayPause.Acquire(this);

            SetStatus(HouseTexts.Get(Localization, HouseTexts.HintView), false);
            if (_house.MarkEntered())
            {
                SetStatus(HouseTexts.Get(Localization, HouseTexts.QuestDone), false);
            }
            else
            {
                ComeHome();
            }

            RefreshTexts();
            _arrangeButton?.Focus();
        }

        public void Exit()
        {
            if (ChestOpen && _chestOpenedHere)
            {
                _chestUI.Close();
            }

            EndInventorySession();
            _chestOpenedHere = false;

            if (!_inside)
            {
                return;
            }

            _inside = false;
            CancelCarry();
            SetArranging(false);
            CloseCard();
            _lifePanel?.Hide();
            _workbench?.Hide();
            _wardrobe?.Hide();
            _dummy?.Hide();
            if (_view != null)
            {
                _view.Show(false);
            }

            if (_mapCamera != null)
            {
                _mapCamera.enabled = true;
                _mapCamera = null;
            }

            if (_overlay != null)
            {
                _overlay.style.display = DisplayStyle.None;
            }

            HideHud(false);
            GameplayPause.Release(this);
        }

        /// <summary>The HUD's back (Esc / Start): closes the house chest first, else the card, else leaves.</summary>
        public void HandleBack()
        {
            if (ChestOpen && _chestOpenedHere)
            {
                _chestUI.Close();
                return;
            }

            if (IsCardOpen && _inside)
            {
                CloseCard();
                return;
            }

            if (_lifePanel != null && _lifePanel.IsVisible)
            {
                _lifePanel.Hide();
                return;
            }

            if (_dummy != null && _dummy.IsVisible)
            {
                _dummy.Hide();
                return;
            }

            if (_wardrobe != null && _wardrobe.IsVisible)
            {
                _wardrobe.Hide();
                return;
            }

            if (_workbench != null && _workbench.IsVisible)
            {
                _workbench.Hide();
                return;
            }

            if (IsCardOpen)
            {
                CloseCard();
                return;
            }

            Exit();
        }

        private void HideHud(bool hide)
        {
            if (hide)
            {
                _hiddenHud.Clear();
                VisualElement root = _overlay?.parent;
                if (root == null) return;
                foreach (VisualElement child in root.Children())
                {
                    if (child == _overlay) continue;
                    _hiddenHud.Add((child, child.style.visibility.value));
                    child.style.visibility = Visibility.Hidden;
                }
            }
            else
            {
                foreach ((VisualElement element, Visibility visibility) in _hiddenHud)
                {
                    if (element != null) element.style.visibility = visibility;
                }

                _hiddenHud.Clear();
            }
        }

        // ------------------------------------------------------------------
        // Overlay
        // ------------------------------------------------------------------

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
            _tabButtons.Clear();

            _overlay = new VisualElement { name = OverlayName, focusable = false };
            _overlay.AddToClassList("house-overlay");
            _overlay.style.display = DisplayStyle.None;
            _overlay.pickingMode = PickingMode.Position;
            var sheet = Resources.Load<StyleSheet>(StyleResourcePath);
            if (sheet != null)
            {
                _overlay.styleSheets.Add(sheet);
            }

            var top = new VisualElement { name = "wm-house-topbar" };
            top.AddToClassList("house-topbar");
            _title = AddLabel(top, "house-title");
            _counts = AddLabel(top, "house-counts");
            _status = AddLabel(top, "house-status");
            _overlay.Add(top);

            _toolbar = new VisualElement { name = "wm-house-toolbar" };
            _toolbar.AddToClassList("house-toolbar");
            _arrangeButton = AddButton(_toolbar, "wm-house-arrange-button", "house-tool", () => SetArranging(!_arranging));
            _chestButton = AddButton(_toolbar, "wm-house-chest-button", "house-tool", OpenHouseChest);
            _sleepButton = AddButton(_toolbar, "wm-house-sleep-button", "house-tool", AskSleep);
            _turnLeftButton = AddButton(_toolbar, "wm-house-turn-left-button", "house-tool", () => _view?.TurnCamera(-1));
            _turnRightButton = AddButton(_toolbar, "wm-house-turn-right-button", "house-tool", () => _view?.TurnCamera(1));
            _yardButton = AddButton(_toolbar, "wm-house-yard-button", "house-tool", ToggleYard);
            _upgradeButton = AddButton(_toolbar, "wm-house-upgrade-button", "house-tool", () => ToggleLifePanel(HouseLifePanel.Mode.Upgrade));
            _lifeButton = AddButton(_toolbar, "wm-house-life-button", "house-tool", () => ToggleLifePanel(HouseLifePanel.Mode.Life));
            _leaveButton = AddButton(_toolbar, "wm-house-leave-button", "house-tool", Exit);
            _leaveButton.AddToClassList("rap-button--primary");
            _overlay.Add(_toolbar);

            _arrangePanel = new VisualElement { name = "wm-house-arrange-panel" };
            _arrangePanel.AddToClassList("house-arrange");
            _depotHeader = AddLabel(_arrangePanel, "house-section");
            var tabs = new VisualElement();
            tabs.AddToClassList("house-tabs");
            foreach (HouseLayer layer in Tabs)
            {
                HouseLayer captured = layer;
                Button tab = AddButton(tabs, "wm-house-tab-" + layer.ToString().ToLowerInvariant(), "house-tab", () => SetTab(captured));
                _tabButtons.Add(tab);
            }

            _arrangePanel.Add(tabs);
            _depotList = new ScrollView(ScrollViewMode.Vertical) { name = "wm-house-depot" };
            _depotList.AddToClassList("house-depot");
            _arrangePanel.Add(_depotList);
            _depotEmpty = AddLabel(_arrangePanel, "house-depot-empty");
            var actions = new VisualElement();
            actions.AddToClassList("house-actions");
            _confirmButton = AddButton(actions, "wm-house-confirm-button", "house-action", Confirm);
            _confirmButton.AddToClassList("house-confirm");
            _rotateButton = AddButton(actions, "wm-house-rotate-button", "house-action", () => RotateCarry(1));
            _mirrorButton = AddButton(actions, "wm-house-mirror-button", "house-action", MirrorCarry);
            _storeButton = AddButton(actions, "wm-house-store-button", "house-action", StoreSelectedOrCarried);
            _cancelButton = AddButton(actions, "wm-house-cancel-button", "house-action", CancelCarry);
            _arrangePanel.Add(actions);
            _hint = AddLabel(_arrangePanel, "house-hint");
            _overlay.Add(_arrangePanel);

            _workbench = new HouseWorkbenchPanel(_overlay, () => _house, LifeWallet, () => Localization, SetStatus);
            _wardrobe = new WardrobePanel(_overlay, Rapadura.Gameplay.Wardrobe.WardrobeService.ResolveShared, () => Localization, SetStatus);
            _dummy = new TrainingDummyPanel(_overlay, () => Localization, SetStatus, null);
            _lifePanel = new HouseLifePanel(_overlay, () => _house, LifeWallet, RomariaOratoryHub.CarriedLevel, () => Localization,
                (title, text, reaction, yes, no, onYes) => ShowCard(title, text, reaction, yes, no, onYes), SetStatus, OnLifePanelClosed);

            _card = new VisualElement { name = "wm-house-card" };
            _card.AddToClassList("house-card");
            _card.style.display = DisplayStyle.None;
            var cardPanel = new VisualElement();
            cardPanel.AddToClassList("rap-panel");
            cardPanel.AddToClassList("house-card-panel");
            _cardTitle = AddLabel(cardPanel, "house-card-title");
            _cardText = AddLabel(cardPanel, "house-card-text");
            _cardReaction = AddLabel(cardPanel, "house-card-text");
            var cardButtons = new VisualElement();
            cardButtons.AddToClassList("house-card-buttons");
            _cardNo = AddButton(cardButtons, "wm-house-card-no", "house-action", CloseCard);
            _cardYes = AddButton(cardButtons, "wm-house-card-yes", "house-action", OnCardYes);
            _cardYes.AddToClassList("rap-button--primary");
            cardPanel.Add(cardButtons);
            _card.Add(cardPanel);
            root.Add(_card);

            _overlay.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _overlay.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _overlay.RegisterCallback<PointerUpEvent>(OnPointerUp);
            _overlay.RegisterCallback<PointerCancelEvent>(evt => ForgetPointer(evt.pointerId));
            _overlay.RegisterCallback<WheelEvent>(evt => { if (evt.target == _overlay) _view?.Zoom(Mathf.Sign(evt.delta.y) * -0.08f); });

            root.Add(_overlay);
            _card.BringToFront();
            Rapadura.UI.Common.UiAccessibilityApplier.RequestRefresh();
            return true;
        }

        private static Label AddLabel(VisualElement parent, string className)
        {
            var label = new Label();
            label.AddToClassList(className);
            parent.Add(label);
            return label;
        }

        private static Button AddButton(VisualElement parent, string name, string className, System.Action onClick)
        {
            var button = new Button(onClick) { name = name, focusable = true };
            button.AddToClassList("rap-button");
            button.AddToClassList(className);
            parent.Add(button);
            return button;
        }

        private void ShowCard(string title, string text, string reaction, string yes, string no, System.Action onYes)
        {
            if (!EnsureOverlay()) return;
            _cardTitle.text = title;
            _cardText.text = text;
            _cardReaction.text = reaction ?? string.Empty;
            _cardReaction.style.display = string.IsNullOrEmpty(reaction) ? DisplayStyle.None : DisplayStyle.Flex;
            _cardYes.text = yes;
            _cardNo.text = no;
            _cardYesAction = onYes;
            _card.style.display = DisplayStyle.Flex;
            _card.BringToFront();
            if (!_inside) GameplayPause.Acquire(_card);
            _cardYes.Focus();
        }

        private void CloseCard()
        {
            if (_card == null) return;
            _card.style.display = DisplayStyle.None;
            _cardYesAction = null;
            GameplayPause.Release(_card);
        }

        private void OnCardYes()
        {
            System.Action action = _cardYesAction;
            CloseCard();
            action?.Invoke();
        }

        // ------------------------------------------------------------------
        // Update
        // ------------------------------------------------------------------

        private void Update()
        {
            if (!_inside)
            {
                return;
            }

            UpdateChestWindow();

            if (_dirtyView && _view != null)
            {
                _dirtyView = false;
                if (_view.BuiltLevel != _house.Level)
                {
                    CancelCarry();
                    _view.RebuildHull();
                }

                _view.RebuildFurniture();
                if (_lifePanel != null && _lifePanel.IsVisible) _lifePanel.Refresh();
                if (_carry != null && _carry.FromInstance > 0) _view.SetHiddenInstance(_carry.FromInstance);
                _view.SetSelectedInstance(_selected);
                RebuildDepotList();
            }

            if (Time.unscaledTime >= _nextTextRefresh)
            {
                _nextTextRefresh = Time.unscaledTime + 0.5f;
                RefreshTexts();
            }

            if (ChestOpen || IsCardOpen || Rapadura.UI.Bestiary.BestiaryController.IsOpenShared)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && _arranging)
            {
                if (keyboard.qKey.wasPressedThisFrame) RotateCarry(-1);
                if (keyboard.eKey.wasPressedThisFrame) RotateCarry(1);
                if (keyboard.deleteKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame) StoreSelectedOrCarried();
                if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame) Confirm();
                if (keyboard.fKey.wasPressedThisFrame) MirrorCarry();
            }

            if (keyboard != null && !_arranging)
            {
                if (keyboard.qKey.wasPressedThisFrame) _view?.TurnCamera(-1);
                if (keyboard.eKey.wasPressedThisFrame) _view?.TurnCamera(1);
            }
        }

        private void OnHouseChanged(HouseChangedEvent evt)
        {
            _dirtyView = true;
        }

        private void RefreshTexts()
        {
            if (_overlay == null || _house == null) return;
            LocalizationManager l = Localization;
            HouseLayout layout = _house.Layout;
            HouseLevelSpec spec = _house.Spec;
            SetText(_title, HouseTexts.Format(l, HouseTexts.Title, HouseTexts.Get(l, spec.NameKey)));
            SetText(_counts, HouseTexts.Format(l, HouseTexts.Counts, layout.InteriorCount, spec.InteriorItemLimit, layout.YardCount, spec.YardItemLimit, layout.DepotEntryCount));
            SetText(_arrangeButton, HouseTexts.Get(l, _arranging ? HouseTexts.ArrangeDone : HouseTexts.Arrange));
            SetText(_chestButton, HouseTexts.Get(l, HouseTexts.ChestButton));
            SetText(_sleepButton, HouseTexts.Get(l, HouseTexts.SleepButton));
            SetText(_turnLeftButton, HouseTexts.Get(l, HouseTexts.TurnLeft));
            SetText(_turnRightButton, HouseTexts.Get(l, HouseTexts.TurnRight));
            SetText(_yardButton, HouseTexts.Get(l, _view != null && _view.YardFocus ? HouseTexts.ShowInside : HouseTexts.ShowYard));
            SetText(_leaveButton, HouseTexts.Get(l, HouseTexts.Leave));
            SetText(_upgradeButton, HouseLifeTexts.Get(l, HouseLifeTexts.UpgradeButton));
            SetText(_lifeButton, HouseLifeTexts.Get(l, HouseLifeTexts.LifeButton));
            _upgradeButton?.EnableInClassList("house-tool--active", _lifePanel != null && _lifePanel.Current == HouseLifePanel.Mode.Upgrade);
            _lifeButton?.EnableInClassList("house-tool--active", _lifePanel != null && _lifePanel.Current == HouseLifePanel.Mode.Life);
            SetText(_depotHeader, HouseTexts.Get(l, HouseTexts.Depot));
            SetText(_depotEmpty, HouseTexts.Get(l, HouseTexts.DepotEmpty));
            SetText(_confirmButton, HouseTexts.Get(l, HouseTexts.Confirm));
            SetText(_rotateButton, HouseTexts.Get(l, HouseTexts.Rotate));
            SetText(_mirrorButton, HouseTexts.Get(l, HouseTexts.Mirror));
            SetText(_storeButton, HouseTexts.Get(l, HouseTexts.Store));
            SetText(_cancelButton, HouseTexts.Get(l, HouseTexts.Cancel));
            SetText(_hint, HouseTexts.Get(l, _lastPointerTouch || Application.isMobilePlatform ? HouseTexts.HintTouch : HouseTexts.HintPc));
            for (int i = 0; i < _tabButtons.Count; i++)
            {
                SetText(_tabButtons[i], HouseTexts.Get(l, HouseTexts.LayerKey(Tabs[i])));
                _tabButtons[i].EnableInClassList("house-tab--active", Tabs[i] == _tab);
            }

            _arrangeButton.EnableInClassList("house-tool--active", _arranging);
            _chestButton.SetEnabled(layout.HasPlaced(HouseItemFunction.Chest));
            _sleepButton.SetEnabled(layout.HasPlaced(HouseItemFunction.Bed));
            bool carrying = _carry != null;
            HouseItemDefinition carried = carrying ? layout.Resolve(_carry.ItemId) : null;
            _confirmButton.SetEnabled(carrying && _carry.HasTarget && _carry.Result == HousePlacementResult.Ok);
            _rotateButton.SetEnabled(carrying && HousePlacementRules.RotationPositions(carried) > 1 && _carry.Target.layer != HouseLayer.Wall);
            _mirrorButton.SetEnabled(carrying && _carry.Target.layer == HouseLayer.Wall);
            _storeButton.SetEnabled((carrying && _carry.FromInstance > 0) || (!carrying && _selected > 0));
            _cancelButton.SetEnabled(carrying || _selected > 0);
        }

        private static void SetText(TextElement element, string text)
        {
            if (element != null && element.text != text) element.text = text;
        }

        private void SetStatus(string text, bool warning)
        {
            if (_status == null) return;
            _status.text = text ?? string.Empty;
            _status.EnableInClassList("house-status--warning", warning);
        }

        // ------------------------------------------------------------------
        // Arrange mode
        // ------------------------------------------------------------------

        private void SetArranging(bool arranging)
        {
            _arranging = arranging && _inside;
            if (_arranging) _lifePanel?.Hide();
            if (_arranging) _workbench?.Hide();
            if (_arranging) _wardrobe?.Hide();
            if (_arranging) _dummy?.Hide();
            if (!_arranging)
            {
                CancelCarry();
                Select(0);
            }

            if (_arrangePanel != null)
            {
                _arrangePanel.style.display = _arranging ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_view != null)
            {
                _view.ShowGrid(_arranging ? _tab : (HouseLayer?)null, HouseWallSide.North);
                _view.SetYardFocus(_arranging && _tab == HouseLayer.Yard);
            }

            if (_arranging)
            {
                RebuildDepotList();
                SetStatus(HouseTexts.Get(Localization, _lastPointerTouch || Application.isMobilePlatform ? HouseTexts.HintTouch : HouseTexts.HintPc), false);
            }

            RefreshTexts();
        }

        private void SetTab(HouseLayer layer)
        {
            _tab = layer;
            if (_carry != null)
            {
                HouseItemDefinition item = _house.Layout.Resolve(_carry.ItemId);
                if (item != null && item.Allows(layer))
                {
                    _carry.Target.layer = layer;
                    _carry.Target.rotation = 0;
                    _carry.HasTarget = false;
                    _view?.HideGhost();
                }
                else
                {
                    CancelCarry();
                }
            }

            _view?.SetYardFocus(layer == HouseLayer.Yard);
            _view?.ShowGrid(layer, HouseWallSide.North);
            RebuildDepotList();
            RefreshTexts();
        }

        private void ToggleYard()
        {
            if (_view == null) return;
            if (_arranging)
            {
                SetTab(_view.YardFocus ? HouseLayer.Floor : HouseLayer.Yard);
                return;
            }

            _view.SetYardFocus(!_view.YardFocus);
            RefreshTexts();
        }

        private void RebuildDepotList()
        {
            if (_depotList == null || _house == null) return;
            LocalizationManager l = Localization;
            _depotList.Clear();
            int shown = 0;
            foreach (HouseDepotEntry entry in _house.Layout.Depot)
            {
                HouseItemDefinition item = _house.Layout.Resolve(entry.itemId);
                if (item == null || !item.Allows(_tab)) continue;
                string itemId = entry.itemId;
                var button = new Button(() => PickFromDepot(itemId)) { name = "wm-house-depot-" + itemId, focusable = true };
                button.AddToClassList("rap-button");
                button.AddToClassList("house-depot-item");
                button.EnableInClassList("house-depot-item--selected", _carry != null && _carry.FromInstance == 0 && _carry.ItemId == itemId);
                button.text = HouseTexts.Format(l, HouseTexts.DepotEntry, HouseItemTexts.Name(l, item), entry.quantity);
                button.tooltip = HouseItemTexts.Get(l, item.DescKey);
                _depotList.Add(button);
                shown++;
            }

            _depotEmpty.style.display = shown == 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void PickFromDepot(string itemId)
        {
            HouseItemDefinition item = _house.Layout.Resolve(itemId);
            if (item == null) return;
            CancelCarry();
            Select(0);
            _carry = new Carry { ItemId = itemId };
            _carry.Target.itemId = itemId;
            _carry.Target.layer = LayerFor(item, _tab);
            SetStatus(HouseTexts.Format(Localization, HouseTexts.StatusCarrying, HouseItemTexts.Name(Localization, item)), false);
            RebuildDepotList();
            RefreshTexts();
        }

        private void PickInstance(int instanceId)
        {
            HousePlacedData placed = _house.Layout.Find(instanceId);
            if (placed == null) return;
            CancelCarry();
            _carry = new Carry { ItemId = placed.itemId, FromInstance = instanceId, Target = placed.Clone(), HasTarget = true, Result = HousePlacementResult.Ok };
            Select(instanceId);
            _view?.SetHiddenInstance(instanceId);
            UpdateGhost();
            SetStatus(HouseTexts.Format(Localization, HouseTexts.StatusCarrying, HouseItemTexts.Name(Localization, placed.itemId)), false);
            RefreshTexts();
        }

        private void Select(int instanceId)
        {
            _selected = instanceId;
            _view?.SetSelectedInstance(instanceId);
        }

        private void CancelCarry()
        {
            _carry = null;
            if (_view != null)
            {
                _view.HideGhost();
                _view.SetHiddenInstance(0);
            }

            RebuildDepotList();
        }

        private void RotateCarry(int direction)
        {
            if (_carry == null || _carry.Target.layer == HouseLayer.Wall) return;
            HouseItemDefinition item = _house.Layout.Resolve(_carry.ItemId);
            _carry.Target.rotation = HousePlacementRules.Rotate(item, _carry.Target.rotation, direction);
            Revalidate();
        }

        private void MirrorCarry()
        {
            if (_carry == null || _carry.Target.layer != HouseLayer.Wall) return;
            _carry.Target.mirrored = !_carry.Target.mirrored;
            Revalidate();
        }

        private void Confirm()
        {
            if (_carry == null || !_carry.HasTarget) return;
            HousePlacementRequest request = HousePlacementRequest.From(_carry.Target);
            request.ItemId = _carry.ItemId;
            HousePlacementResult result;
            if (_carry.FromInstance > 0)
            {
                result = _house.Layout.Move(_carry.FromInstance, request);
            }
            else
            {
                result = _house.Layout.Place(request, out _);
            }

            LocalizationManager l = Localization;
            if (result != HousePlacementResult.Ok)
            {
                SetStatus(HouseTexts.Get(l, HouseTexts.ResultKey(result)), true);
                return;
            }

            string name = HouseItemTexts.Name(l, _carry.ItemId);
            bool fromDepot = _carry.FromInstance == 0;
            string itemId = _carry.ItemId;
            CancelCarry();
            Select(0);
            SetStatus(HouseTexts.Format(l, HouseTexts.StatusPlaced, name), false);

            // Several copies in the depot: keep placing the next one.
            if (fromDepot && _house.Layout.DepotCount(itemId) > 0 && !_lastPointerTouch)
            {
                PickFromDepot(itemId);
            }

            RefreshTexts();
        }

        private void StoreSelectedOrCarried()
        {
            int instanceId = _carry != null ? _carry.FromInstance : _selected;
            if (instanceId <= 0)
            {
                CancelCarry();
                return;
            }

            HousePlacedData placed = _house.Layout.Find(instanceId);
            string name = placed != null ? HouseItemTexts.Name(Localization, placed.itemId) : string.Empty;
            if (!_house.TryStoreInDepot(instanceId))
            {
                SetStatus(HouseTexts.Get(Localization, HouseTexts.StatusChestInUse), true);
                return;
            }

            CancelCarry();
            Select(0);
            SetStatus(HouseTexts.Format(Localization, HouseTexts.StatusStored, name), false);
            RefreshTexts();
        }

        /// <summary>Moves the carried piece's target to what is under <paramref name="screen"/> and re-validates.</summary>
        private void AimCarry(Vector2 screen)
        {
            if (_carry == null || _view == null) return;
            HouseItemDefinition item = _house.Layout.Resolve(_carry.ItemId);
            HousePlacedData target = _carry.Target;
            bool hasTarget = false;
            switch (target.layer)
            {
                case HouseLayer.Wall:
                    if (_view.ScreenToWallCell(screen, out HouseWallSide side, out Vector2Int wallCell))
                    {
                        (int w, int h) = HousePlacementRules.BaseFootprint(item, HouseLayer.Wall);
                        Vector2Int origin = CenteredOrigin(wallCell, w, h);
                        if (side != target.wall) _view.ShowGrid(HouseLayer.Wall, side);
                        target.wall = side;
                        target.x = origin.x;
                        target.y = origin.y;
                        hasTarget = true;
                    }

                    break;
                case HouseLayer.Surface:
                {
                    int picked = _view.PickInstance(screen);
                    HousePlacedData pickedData = _house.Layout.Find(picked);
                    if (pickedData != null && pickedData.layer == HouseLayer.Surface) pickedData = _house.Layout.Find(pickedData.parentInstanceId);
                    HouseItemDefinition parentItem = pickedData != null ? _house.Layout.Resolve(pickedData.itemId) : null;
                    if (pickedData != null && parentItem != null && parentItem.SurfaceSlots > 0 && pickedData.instanceId != _carry.FromInstance)
                    {
                        int slot = _view.NearestSurfaceSlot(pickedData.instanceId, screen, _carry.FromInstance);
                        if (slot >= 0)
                        {
                            target.parentInstanceId = pickedData.instanceId;
                            target.slot = slot;
                            hasTarget = true;
                        }
                    }

                    break;
                }
                case HouseLayer.Yard:
                    if (_view.ScreenToYardCell(screen, out Vector2Int yardCell))
                    {
                        CellRect rect = HousePlacementRules.Footprint(item, HouseLayer.Yard, 0, 0, target.rotation);
                        Vector2Int origin = CenteredOrigin(yardCell, rect.Width, rect.Height);
                        target.x = origin.x;
                        target.y = origin.y;
                        hasTarget = true;
                    }

                    break;
                default:
                    if (_view.ScreenToInteriorCell(screen, out Vector2Int cell))
                    {
                        CellRect rect = HousePlacementRules.Footprint(item, target.layer, 0, 0, target.rotation);
                        Vector2Int origin = CenteredOrigin(cell, rect.Width, rect.Height);
                        target.x = origin.x;
                        target.y = origin.y;
                        hasTarget = true;
                    }

                    break;
            }

            if (!hasTarget)
            {
                return;
            }

            _carry.HasTarget = true;
            Revalidate();
        }

        private void Revalidate()
        {
            if (_carry == null) return;
            HousePlacementRequest request = HousePlacementRequest.From(_carry.Target);
            request.ItemId = _carry.ItemId;
            _carry.Result = !_carry.HasTarget ? HousePlacementResult.OutOfBounds
                : _carry.FromInstance > 0 ? _house.Layout.CanMove(_carry.FromInstance, request) : _house.Layout.CanPlace(request);
            UpdateGhost();
            if (_carry.HasTarget)
            {
                LocalizationManager l = Localization;
                SetStatus(_carry.Result == HousePlacementResult.Ok
                    ? HouseTexts.Format(l, HouseTexts.StatusCarrying, HouseItemTexts.Name(l, _carry.ItemId))
                    : HouseTexts.Get(l, HouseTexts.ResultKey(_carry.Result)), _carry.Result != HousePlacementResult.Ok);
            }

            RefreshTexts();
        }

        private void UpdateGhost()
        {
            if (_view == null || _carry == null || !_carry.HasTarget)
            {
                _view?.HideGhost();
                return;
            }

            _view.ShowGhost(_house.Layout.Resolve(_carry.ItemId), _carry.Target, _carry.Result == HousePlacementResult.Ok);
        }

        // ------------------------------------------------------------------
        // Pointer (mouse + touch through UI Toolkit)
        // ------------------------------------------------------------------

        private Vector2 PanelToScreen(Vector3 panelPosition)
        {
            IPanel panel = _overlay?.panel;
            Rect layout = panel != null ? panel.visualTree.layout : new Rect(0f, 0f, Screen.width, Screen.height);
            float sx = layout.width > 0f ? Screen.width / layout.width : 1f;
            float sy = layout.height > 0f ? Screen.height / layout.height : 1f;
            return new Vector2(panelPosition.x * sx, Screen.height - panelPosition.y * sy);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.target != _overlay || !_inside || ChestOpen || IsCardOpen) return;
            Vector2 screen = PanelToScreen(evt.position);
            _lastPointerTouch = evt.pointerType == UnityEngine.UIElements.PointerType.touch || evt.pointerType == UnityEngine.UIElements.PointerType.pen;
            _pointers[evt.pointerId] = screen;

            if (_pointers.Count == 2)
            {
                _twistAngle = TwoPointerAngle();
                _twistAccum = 0f;
                _dragged = true; // a twist is never a tap
                return;
            }

            if (evt.button == 1 && !_lastPointerTouch)
            {
                CancelCarry();
                Select(0);
                RefreshTexts();
                return;
            }

            _primaryPointer = evt.pointerId;
            _pressPosition = screen;
            _dragged = false;
            _overlay.CapturePointer(evt.pointerId);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_inside || ChestOpen || IsCardOpen) return;
            Vector2 screen = PanelToScreen(evt.position);

            if (_pointers.ContainsKey(evt.pointerId))
            {
                _pointers[evt.pointerId] = screen;
            }

            if (_pointers.Count >= 2)
            {
                float angle = TwoPointerAngle();
                _twistAccum += Mathf.DeltaAngle(_twistAngle, angle);
                _twistAngle = angle;
                if (Mathf.Abs(_twistAccum) >= TwistStepDegrees)
                {
                    RotateCarry(_twistAccum > 0f ? -1 : 1);
                    _twistAccum = 0f;
                }

                return;
            }

            if (evt.pointerId == _primaryPointer && (screen - _pressPosition).sqrMagnitude > TapSlopPixels * TapSlopPixels)
            {
                _dragged = true;
            }

            // Mouse hover (no button) or a dragging finger moves the ghost.
            bool hovering = evt.target == _overlay && evt.pointerType == UnityEngine.UIElements.PointerType.mouse;
            if (_arranging && _carry != null && (hovering || evt.pointerId == _primaryPointer))
            {
                AimCarry(screen);
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            bool wasPrimary = evt.pointerId == _primaryPointer;
            Vector2 screen = PanelToScreen(evt.position);
            ForgetPointer(evt.pointerId);
            if (!wasPrimary || !_inside || ChestOpen || IsCardOpen) return;
            if (_overlay.HasPointerCapture(evt.pointerId)) _overlay.ReleasePointer(evt.pointerId);

            bool touch = evt.pointerType == UnityEngine.UIElements.PointerType.touch || evt.pointerType == UnityEngine.UIElements.PointerType.pen;
            if (_dragged)
            {
                return;
            }

            if (!_arranging)
            {
                UseInstance(_view != null ? _view.PickInstance(screen) : 0);
                return;
            }

            if (_carry == null)
            {
                int picked = _view != null ? _view.PickInstance(screen) : 0;
                if (picked > 0) PickInstance(picked);
                return;
            }

            AimCarry(screen);
            if (ClickConfirms(touch) && _carry != null && _carry.Result == HousePlacementResult.Ok)
            {
                Confirm();
            }
        }

        private void ForgetPointer(int pointerId)
        {
            _pointers.Remove(pointerId);
            if (pointerId == _primaryPointer && _pointers.Count == 0)
            {
                _primaryPointer = -1;
            }
        }

        private float TwoPointerAngle()
        {
            Vector2 a = default, b = default;
            int i = 0;
            foreach (Vector2 p in _pointers.Values)
            {
                if (i == 0) a = p;
                else if (i == 1) b = p;
                i++;
            }

            Vector2 d = b - a;
            return Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
        }

        // ------------------------------------------------------------------
        // Furniture that does something
        // ------------------------------------------------------------------

        private void UseInstance(int instanceId)
        {
            HousePlacedData placed = _house.Layout.Find(instanceId);
            HouseItemDefinition item = placed != null ? _house.Layout.Resolve(placed.itemId) : null;
            if (item == null) return;
            LocalizationManager l = Localization;
            switch (item.Function)
            {
                case HouseItemFunction.Chest: OpenHouseChest(); return;
                case HouseItemFunction.VillageChestLink: OpenChestWindow(VillageChestStorage.ResolveShared(), null, null); return;
                case HouseItemFunction.Bed: AskSleep(); return;
                case HouseItemFunction.Wardrobe:
                    _lifePanel?.Hide();
                    _workbench?.Hide();
                    _dummy?.Hide();
                    _wardrobe?.Show();
                    return;
                case HouseItemFunction.Bench:
                case HouseItemFunction.Forge:
                case HouseItemFunction.Stove:
                    _lifePanel?.Hide();
                    _dummy?.Hide();
                    _wardrobe?.Hide();
                    _workbench?.Show(item.Function);
                    return;
                case HouseItemFunction.GuestBook: ShowLifePanel(HouseLifePanel.Mode.Life); return;
                case HouseItemFunction.PetBed: SetStatus(HouseLifeTexts.Get(l, HouseLifeTexts.PetNap), false); return;
                case HouseItemFunction.TrophyStand:
                    SetStatus(HouseLifeTexts.Format(l, HouseLifeTexts.TrophyStatus, _house.BossesDefeated, HouseUpgradeRules.BossIds.Length), false);
                    return;
                case HouseItemFunction.RelicCase: ShowRelics(); return;
                case HouseItemFunction.BestiaryShelf: OpenNotebook(); return;
                case HouseItemFunction.TrainingDummy: ShowTrainingDummy(); return;
                default:
                    if (HousePlantRules.IsPlant(item))
                    {
                        SetStatus(HouseLifeTexts.Format(l, HouseLifeTexts.PlantStatus, HouseItemTexts.Name(l, item), HouseLifeTexts.Get(l, HouseLifeTexts.MoodKey(_house.PlantMood(instanceId)))), false);
                        return;
                    }

                    SetStatus(HouseTexts.Format(l, HouseTexts.StatusItem, HouseItemTexts.Name(l, item)), false);
                    return;
            }
        }

        private void AskSleep()
        {
            LocalizationManager l = Localization;
            if (!_house.Layout.HasPlaced(HouseItemFunction.Bed))
            {
                SetStatus(HouseTexts.Get(l, HouseTexts.StatusNoBed), true);
                return;
            }

            ShowCard(HouseTexts.Get(l, HouseTexts.SleepButton), HouseTexts.Get(l, HouseTexts.SleepQuestion), null,
                HouseTexts.Get(l, HouseTexts.SleepYes), HouseTexts.Get(l, HouseTexts.SleepNo), Sleep);
        }

        private void Sleep()
        {
            int day = 0;
            if (ServiceLocator.TryGet(out TimeOfDayManager clock))
            {
                clock.AdvanceHours(HouseRules.HoursUntilMorning(clock.CurrentHour));
                day = clock.Day;
            }

            _house.RecordSleep(day);
            bool rested = _house.RestInBed();
            if (ServiceLocator.TryGet(out SaveManager saves))
            {
                saves.AutoSave();
            }

            LocalizationManager l = Localization;
            SetStatus(HouseTexts.Get(l, HouseTexts.Slept) + " " + (rested
                ? HouseLifeTexts.Format(l, HouseLifeTexts.SleptRested, _house.Rested.percent, _house.Rested.fightsLeft)
                : HouseLifeTexts.Get(l, HouseLifeTexts.SleptNoCharge)), false);
        }

        // ------------------------------------------------------------------
        // H4 / H6 panel, displays, homecoming
        // ------------------------------------------------------------------

        private void ToggleLifePanel(HouseLifePanel.Mode mode)
        {
            if (_lifePanel == null) return;
            if (_lifePanel.Current == mode)
            {
                _lifePanel.Hide();
            }
            else
            {
                ShowLifePanel(mode);
            }

            RefreshTexts();
        }

        private void ShowLifePanel(HouseLifePanel.Mode mode)
        {
            if (_lifePanel == null) return;
            if (_arranging) SetArranging(false);
            _workbench?.Hide();
            _wardrobe?.Hide();
            _dummy?.Hide();
            _lifePanel.Show(mode);
            RefreshTexts();
        }

        /// <summary>
        /// The straw training dummy (Docs/Housing.md §11.8): the active cousin tests attacks and sees damage per hit, crits
        /// and DPS (<see cref="Rapadura.Gameplay.Combat.TrainingDummyRules"/>). No XP, gold or loot: the city is a safe zone.
        /// </summary>
        private void ShowTrainingDummy()
        {
            if (_dummy == null) return;
            if (_arranging) SetArranging(false);
            _lifePanel?.Hide();
            _workbench?.Hide();
            _dummy.Show();
            RefreshTexts();
        }

        /// <summary>The bag (a short carried-inventory session while the panel is open) plus the Baú da casa.</summary>
        private IHouseWallet LifeWallet()
        {
            if (_house == null) return null;
            InventoryManager bag = BeginInventorySession();
            return new HouseInventoryWallet(bag, _house.Chest);
        }

        private void OnLifePanelClosed()
        {
            if (!ChestOpen) EndInventorySession();
            RefreshTexts();
        }

        /// <summary>Coming home (not the first time): rain on the yard, maybe a visit, quests that completed.</summary>
        private void ComeHome()
        {
            LocalizationManager l = Localization;
            string cousin = ServiceLocator.TryGet(out CharacterSelectionState selection) && selection.Selected.HasValue ? selection.Selected.Value.ToString() : null;
            bool raining = ServiceLocator.TryGet(out WeatherSystem weather) && weather.CurrentWeather == WeatherType.Rain;
            HouseHomecoming home = _house.ComeHome(cousin, raining);
            var lines = new List<string>();
            if (home.Visit != null)
            {
                lines.Add(HouseLifeTexts.Format(l, HouseLifeTexts.VisitStatus, HouseLifeTexts.Get(l, HouseLifeTexts.VisitorKey(home.Visit.visitorId))));
                lines.Add(HouseLifePanel.GuestLine(home.Visit, l));
            }

            if (home.PlantsWateredByRain > 0) lines.Add(HouseLifeTexts.Format(l, HouseLifeTexts.Rain, home.PlantsWateredByRain));
            foreach (string questId in home.CompletedQuests)
            {
                HouseQuestDefinition quest = HouseQuestChain.Get(questId);
                if (quest != null) lines.Add(HouseLifeTexts.Format(l, HouseLifeTexts.QuestDoneStatus, HouseLifeTexts.Get(l, quest.NameKey)));
            }

            if (lines.Count > 0) SetStatus(string.Join(" ", lines), false);
        }

        private void ShowRelics()
        {
            LocalizationManager l = Localization;
            Rapadura.Gameplay.Secrets.SecretsProgress secrets = Rapadura.Gameplay.Secrets.SecretsProgress.ResolveShared();
            var names = new List<string>();
            foreach (Rapadura.Gameplay.Secrets.RelicDefinition relic in Rapadura.Gameplay.Secrets.RelicCatalog.All)
            {
                bool found = secrets != null && secrets.HasRelic(relic.Id);
                names.Add((found ? "+ " : "- ") + (found ? Rapadura.Gameplay.Secrets.SecretTexts.Get(l, relic.NameKey) : HouseLifeTexts.Get(l, HouseLifeTexts.RelicMissing)));
            }

            int owned = secrets != null ? secrets.OwnedRelicCount : 0;
            ShowCard(HouseLifeTexts.Get(l, HouseLifeTexts.RelicTitle), HouseLifeTexts.Format(l, HouseLifeTexts.RelicStatus, owned, Rapadura.Gameplay.Secrets.RelicCatalog.Count),
                string.Join("\n", names), HouseLifeTexts.Get(l, HouseLifeTexts.Ok), HouseLifeTexts.Get(l, HouseLifeTexts.Close), null);
        }

        private void OpenNotebook()
        {
            LocalizationManager l = Localization;
            bool opened = Rapadura.UI.Bestiary.BestiaryController.OpenShared();
            SetStatus(HouseLifeTexts.Get(l, opened ? HouseLifeTexts.ShelfOpening : HouseLifeTexts.ShelfUnavailable), !opened);
        }

        private void OpenHouseChest()
        {
            if (!_house.Layout.HasPlaced(HouseItemFunction.Chest))
            {
                SetStatus(HouseTexts.Get(Localization, HouseTexts.StatusNoChest), true);
                return;
            }

            LocalizationManager l = Localization;
            OpenChestWindow(_house.Chest, HouseTexts.Get(l, HouseTexts.ChestTitle), HouseTexts.Get(l, HouseTexts.ChestSubtitle));
        }

        private string _chestTitle;
        private string _chestSubtitle;

        private void OpenChestWindow(VillageChestStorage chest, string title, string subtitle)
        {
            _chestUI = _chestUI != null ? _chestUI : EnsureServiceView<VillageChestUIController>(ChestViewResourcePath);
            if (_chestUI == null || chest == null)
            {
                Debug.LogWarning($"[HouseScreenController] Cannot open the chest (Resources/{ChestViewResourcePath}.uxml missing).");
                return;
            }

            ServiceLocator.TryGet(out QuestManager quests);
            _chestTitle = title;
            _chestSubtitle = subtitle;
            _chestUI.Open(BeginInventorySession(), chest, quests != null ? quests.GetOutstandingCollectAmount : (System.Func<string, int, int>)null);
            _chestOpenedHere = _chestUI.IsOpen;
            if (_chestOpenedHere && _overlay != null)
            {
                _overlay.style.display = DisplayStyle.None;
            }
        }

        private void UpdateChestWindow()
        {
            if (!_chestOpenedHere)
            {
                return;
            }

            if (ChestOpen)
            {
                // The chest window is the village one: give it the house chest's title while it shows our storage.
                UIDocument chestDocument = _chestUI.GetComponent<UIDocument>();
                VisualElement root = chestDocument != null ? chestDocument.rootVisualElement : null;
                if (root != null && _chestTitle != null)
                {
                    SetText(root.Q<Label>("village-chest-title"), _chestTitle);
                    SetText(root.Q<Label>("village-chest-subtitle"), _chestSubtitle);
                }

                return;
            }

            _chestOpenedHere = false;
            EndInventorySession();
            if (_inside && _overlay != null)
            {
                _overlay.style.display = DisplayStyle.Flex;
                _chestButton?.Focus();
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
            if (_session != null && _session.IsOpen)
            {
                _session.Close();
            }
        }

        /// <summary>Same as the village hub: the scene's window if there is one, else built from Resources on the HUD's panel.</summary>
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

            var host = new GameObject("WorldMap" + typeof(T).Name);
            host.SetActive(false);
            host.transform.SetParent(transform.parent, false);
            var document = host.AddComponent<UIDocument>();
            document.panelSettings = _hudDocument.panelSettings;
            document.visualTreeAsset = view;
            document.sortingOrder = ServiceSortingOrder;
            var controller = host.AddComponent<T>();
            host.SetActive(true);
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
