using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.Talents;
using Rapadura.UI.Common;
using Rapadura.UI.Talents;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.CharacterSheet
{
    /// <summary>
    /// Ficha do personagem (Resources/UI/CharacterSheetView.uxml/.uss): hero name/class, level + XP,
    /// unspent attribute points, one row per attribute with its total, the talent bonus, a preview of
    /// what +1 changes ("+10 Vida máx.") and a 56px "+" button, the derived stats (max HP/mana/stamina,
    /// crit chance/multiplier, cooldown reduction, skill damage) and the equipped items. Every number
    /// comes from <see cref="CharacterSheetPresenter"/>, which reads the same code paths the game uses.
    ///
    /// Spending only goes through <see cref="AttributeSet.TrySpendPoint"/>. There is no
    /// "Redistribuir": <see cref="AttributeSet"/> has no respec API, and the sheet doesn't invent one.
    ///
    /// Same modal contract as <c>QuestLogController</c>: <see cref="Open"/>/<see cref="Hide"/>/
    /// <see cref="IsOpen"/>, holds a <see cref="GameplayPause"/> while open, gamepad/keyboard through
    /// <see cref="FocusNavigator"/> (D-pad between the "+" buttons, A spends, B closes), and the HUD
    /// registers it in its <c>ScreenExclusivityCoordinator</c> ("Ficha" button + C shortcut).
    ///
    /// No scene wiring needed: <c>HudController</c> creates it next to itself (own GameObject +
    /// UIDocument sharing the HUD's PanelSettings, <see cref="DefaultSortingOrder"/>) when the scene
    /// doesn't already have one. If the UIDocument has no Source Asset, the view is loaded from
    /// Resources. Player components are resolved lazily (the player may spawn after the HUD).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class CharacterSheetController : MonoBehaviour
    {
        public const string ViewResourcePath = "UI/CharacterSheetView";

        /// <summary>Same layer as the other full-screen windows: above the HUD (0), below tooltips/tutorial hints.</summary>
        public const float DefaultSortingOrder = 10f;

        [Header("Optional References (resolved from the scene when empty)")]
        [SerializeField] private PlayerStats _playerStats;
        [SerializeField] private AttributeSet _attributes;
        [SerializeField] private InventoryManager _inventory;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private Button _closeButton;
        private Label _heroNameLabel;
        private Label _heroClassLabel;
        private Label _levelLabel;
        private VisualElement _xpFill;
        private Label _xpLabel;
        private Label _pointsLabel;
        private VisualElement _attributeList;
        private VisualElement _derivedList;
        private VisualElement _equipmentList;
        private VisualElement _trainingList;
        private Label _trainingHeader;
        private Button _wheelButton;
        private TalentWheelView _wheel;

        private readonly Dictionary<AttributeType, AttributeRow> _rows = new Dictionary<AttributeType, AttributeRow>();
        private readonly UiDocumentLocalizer _localizer = new UiDocumentLocalizer();
        private FocusNavigator _focusNavigator;
        private BuffController _buffs;
        private PlayerCombatBalance _combatBalance;
        private bool _combatBalanceLookupDone;
        private AttributeSet _subscribedAttributes;
        private bool _isOpen;

        private sealed class AttributeRow
        {
            public VisualElement Root;
            public Label Name;
            public Label PrimaryTag;
            public Label Preview;
            public Label Value;
            public Label Talent;
            public Button Spend;
        }

        public bool IsOpen => _isOpen;

        private LocalizationManager Localization => _localizer.Manager;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            // Created at runtime, possibly after UiClickSoundBinder's per-scene scans.
            Rapadura.Audio.UiClickSoundBinder.Register(_uiDocument);

            if (_uiDocument.visualTreeAsset == null)
            {
                VisualTreeAsset view = Resources.Load<VisualTreeAsset>(ViewResourcePath);

                if (view == null)
                {
                    Debug.LogWarning($"[CharacterSheetController] Missing Resources/{ViewResourcePath}.uxml; character sheet disabled.");
                    enabled = false;
                    return;
                }

                _uiDocument.visualTreeAsset = view;
            }
        }

        private void OnEnable()
        {
            BindElements();

            if (_closeButton != null)
            {
                _closeButton.clicked += Hide;
            }

            EventBus.Subscribe<PlayerExperienceChangedEvent>(OnExperienceChanged);
            EventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
            EventBus.Subscribe<ProficiencyLevelUpEvent>(OnProficiencyLevelUp);
            EventBus.Subscribe<TalentWheelChangedEvent>(OnTalentWheelChanged);

            _localizer.Attach(_root, OnLanguageChanged, CharacterSheetTexts.Get);
            // D-pad between the "+" buttons and Fechar, A spends, B closes — see FocusNavigator.
            _focusNavigator = FocusNavigator.Attach(_root, () => IsOpen, OnCancel, () => FirstSpendableButton() ?? _closeButton);
            Hide();
        }

        private void Start()
        {
            // LocalizationManager may only be registered by now (GameManager.Awake ordering).
            _localizer.Refresh();
        }

        private void OnDisable()
        {
            _focusNavigator?.Detach();
            _focusNavigator = null;
            _localizer.Detach();

            if (_closeButton != null)
            {
                _closeButton.clicked -= Hide;
            }

            EventBus.Unsubscribe<PlayerExperienceChangedEvent>(OnExperienceChanged);
            EventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
            EventBus.Unsubscribe<ProficiencyLevelUpEvent>(OnProficiencyLevelUp);
            EventBus.Unsubscribe<TalentWheelChangedEvent>(OnTalentWheelChanged);
            SubscribeAttributes(null);

            _isOpen = false;
            GameplayPause.Release(this);
        }

        private void OnDestroy()
        {
            GameplayPause.Release(this);
            Rapadura.Audio.UiClickSoundBinder.Unregister(_uiDocument);
        }

        private void BindElements()
        {
            VisualElement documentRoot = _uiDocument.rootVisualElement;

            _root = documentRoot.Q<VisualElement>("character-sheet-root");
            _closeButton = documentRoot.Q<Button>("character-sheet-close-button");
            _heroNameLabel = documentRoot.Q<Label>("character-sheet-hero-name");
            _heroClassLabel = documentRoot.Q<Label>("character-sheet-hero-class");
            _levelLabel = documentRoot.Q<Label>("character-sheet-level");
            _xpFill = documentRoot.Q<VisualElement>("character-sheet-xp-fill");
            _xpLabel = documentRoot.Q<Label>("character-sheet-xp-label");
            _pointsLabel = documentRoot.Q<Label>("character-sheet-points");
            _attributeList = documentRoot.Q<VisualElement>("character-sheet-attribute-list");
            _derivedList = documentRoot.Q<VisualElement>("character-sheet-derived-list");
            _equipmentList = documentRoot.Q<VisualElement>("character-sheet-equipment-list");

            // The document's own root must not swallow touches while the sheet is hidden (HUD under it).
            documentRoot.pickingMode = PickingMode.Ignore;
            BuildAttributeRows();
            BuildTalentElements(documentRoot);
        }

        // ------------------------------------------------------------------
        // Open / close
        // ------------------------------------------------------------------

        public void Open() => Show();

        public void Show()
        {
            if (_root == null)
            {
                return;
            }

            _isOpen = true;
            _root.style.display = DisplayStyle.Flex;
            GameplayPause.Acquire(this);
            _localizer.Refresh();
            Refresh();
            _focusNavigator?.FocusInitial();
        }

        public void Hide()
        {
            _wheel?.Close();
            _focusNavigator?.ReleaseFocus();
            _isOpen = false;

            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }

            GameplayPause.Release(this);
        }

        public void Toggle()
        {
            if (_isOpen)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        // ------------------------------------------------------------------
        // Player resolution
        // ------------------------------------------------------------------

        /// <summary>Serialized refs first, then the player's siblings, then a scene search (player may spawn late).</summary>
        private void ResolvePlayer()
        {
            if (_playerStats == null)
            {
                _playerStats = _attributes != null ? _attributes.GetComponent<PlayerStats>() : FindFirstObjectByType<PlayerStats>();
            }

            if (_attributes == null)
            {
                _attributes = _playerStats != null ? _playerStats.GetComponent<AttributeSet>() : null;
                if (_attributes == null)
                {
                    _attributes = FindFirstObjectByType<AttributeSet>();
                }
            }

            if (_buffs == null && _playerStats != null)
            {
                _buffs = _playerStats.GetComponent<BuffController>();
            }

            if (_inventory == null)
            {
                _inventory = _playerStats != null ? _playerStats.GetComponent<InventoryManager>() : null;
                if (_inventory == null)
                {
                    _inventory = FindFirstObjectByType<InventoryManager>();
                }
            }

            if (!_combatBalanceLookupDone)
            {
                _combatBalanceLookupDone = true;
                _combatBalance = PlayerCombatBalance.LoadDefault();
            }

            SubscribeAttributes(_attributes);
        }

        private void SubscribeAttributes(AttributeSet attributes)
        {
            if (_subscribedAttributes == attributes)
            {
                return;
            }

            if (_subscribedAttributes != null)
            {
                _subscribedAttributes.OnAttributesChanged -= OnAttributesChanged;
            }

            _subscribedAttributes = attributes;

            if (_subscribedAttributes != null)
            {
                _subscribedAttributes.OnAttributesChanged += OnAttributesChanged;
            }
        }

        // ------------------------------------------------------------------
        // Events
        // ------------------------------------------------------------------

        private void OnAttributesChanged() => RefreshIfOpen();
        private void OnExperienceChanged(PlayerExperienceChangedEvent evt) => RefreshIfOpen();
        private void OnInventoryChanged(InventoryChangedEvent evt) => RefreshIfOpen();
        private void OnGameLoaded(GameLoadedEvent evt) => RefreshIfOpen();
        private void OnProficiencyLevelUp(ProficiencyLevelUpEvent evt) => RefreshIfOpen();
        private void OnTalentWheelChanged(TalentWheelChangedEvent evt) => RefreshIfOpen();

        private void OnLanguageChanged()
        {
            ApplyRowStaticTexts();
            RefreshIfOpen();
        }

        private void RefreshIfOpen()
        {
            if (_isOpen)
            {
                Refresh();
            }
        }

        private void OnSpendClicked(AttributeType type)
        {
            if (!CharacterSheetPresenter.TrySpend(_attributes, type))
            {
                return;
            }

            // OnAttributesChanged already refreshed. Spending the last point disables every "+":
            // move gamepad focus somewhere still usable instead of leaving it on a disabled button.
            if (!CharacterSheetPresenter.CanSpend(_attributes.AvailableAttributePoints) && FocusNavigator.NavigationInputActive)
            {
                _closeButton?.Focus();
            }
        }

        // ------------------------------------------------------------------
        // Rendering
        // ------------------------------------------------------------------

        /// <summary>Rows are built once and only updated afterwards, so gamepad focus stays on the "+" being pressed.</summary>
        private void BuildAttributeRows()
        {
            _rows.Clear();

            if (_attributeList == null)
            {
                return;
            }

            _attributeList.Clear();

            foreach (AttributeType type in CharacterSheetPresenter.AttributeOrder)
            {
                var row = new AttributeRow { Root = new VisualElement { name = "character-sheet-attribute-" + TooltipLocalization.ToSnakeCase(type.ToString()) } };
                row.Root.AddToClassList("character-sheet-attribute-row");

                var text = new VisualElement();
                text.AddToClassList("character-sheet-attribute-text");
                text.pickingMode = PickingMode.Ignore;

                var nameLine = new VisualElement();
                nameLine.AddToClassList("character-sheet-attribute-name-line");
                nameLine.pickingMode = PickingMode.Ignore;

                row.Name = NewLabel("character-sheet-attribute-name");
                row.PrimaryTag = NewLabel("character-sheet-attribute-primary-tag");
                nameLine.Add(row.Name);
                nameLine.Add(row.PrimaryTag);
                text.Add(nameLine);

                row.Preview = NewLabel("character-sheet-attribute-preview");
                text.Add(row.Preview);
                row.Root.Add(text);

                row.Value = NewLabel("character-sheet-attribute-value");
                row.Talent = NewLabel("character-sheet-attribute-talent");
                row.Root.Add(row.Value);
                row.Root.Add(row.Talent);

                AttributeType captured = type;
                row.Spend = new Button(() => OnSpendClicked(captured)) { name = row.Root.name + "-spend" };
                row.Spend.AddToClassList("character-sheet-spend-button");
                row.Root.Add(row.Spend);

                _attributeList.Add(row.Root);
                _rows[type] = row;
            }

            ApplyRowStaticTexts();
        }

        private static Label NewLabel(string className)
        {
            var label = new Label();
            label.AddToClassList(className);
            label.pickingMode = PickingMode.Ignore;
            return label;
        }

        private void ApplyRowStaticTexts()
        {
            LocalizationManager manager = Localization;

            foreach (KeyValuePair<AttributeType, AttributeRow> pair in _rows)
            {
                string attributeName = CharacterSheetPresenter.GetAttributeName(pair.Key, manager);
                pair.Value.Name.text = attributeName;
                pair.Value.PrimaryTag.text = CharacterSheetTexts.Get(manager, CharacterSheetTexts.PrimaryTag);
                pair.Value.Preview.text = CharacterSheetPresenter.FormatPreview(pair.Key, manager);
                pair.Value.Spend.text = CharacterSheetTexts.Get(manager, CharacterSheetTexts.SpendButton);
                pair.Value.Spend.tooltip = CharacterSheetTexts.Format(manager, CharacterSheetTexts.SpendButtonA11y, attributeName);
            }
        }

        private void Refresh()
        {
            ResolvePlayer();
            LocalizationManager manager = Localization;

            ApplyRowStaticTexts();
            RefreshHero(manager);
            RefreshAttributes(manager);
            RefreshDerived(manager);
            RefreshEquipment(manager);
            RefreshTalents(manager);
        }

        private PlayableCharacter CurrentHero()
        {
            if (ServiceLocator.TryGet(out CharacterSelectionState selection) && selection.Selected.HasValue)
            {
                return CharacterRegistry.Get(selection.Selected.Value);
            }

            return null;
        }

        private void RefreshHero(LocalizationManager manager)
        {
            PlayableCharacter hero = CurrentHero();

            if (_heroNameLabel != null)
            {
                _heroNameLabel.text = hero != null ? hero.LocalizedName(manager) : CharacterSheetTexts.Get(manager, CharacterSheetTexts.NoHero);
            }

            if (_heroClassLabel != null)
            {
                _heroClassLabel.text = hero != null ? hero.LocalizedClassName(manager) : string.Empty;
            }

            int level = _playerStats != null ? _playerStats.Level : 1;
            int xp = _playerStats != null ? _playerStats.CurrentExperience : 0;
            int toNext = _playerStats != null ? _playerStats.GetExperienceToNextLevel() : 0;

            if (_levelLabel != null)
            {
                _levelLabel.text = CharacterSheetTexts.Format(manager, CharacterSheetTexts.Level, level);
            }

            if (_xpFill != null)
            {
                _xpFill.style.width = Length.Percent(CharacterSheetPresenter.ExperienceFraction(xp, toNext) * 100f);
            }

            if (_xpLabel != null)
            {
                _xpLabel.text = CharacterSheetTexts.Format(manager, CharacterSheetTexts.Experience, xp, toNext);
            }
        }

        private void RefreshAttributes(LocalizationManager manager)
        {
            int points = _attributes != null ? _attributes.AvailableAttributePoints : 0;
            bool canSpend = CharacterSheetPresenter.CanSpend(points);

            if (_pointsLabel != null)
            {
                _pointsLabel.text = canSpend
                    ? CharacterSheetTexts.Format(manager, CharacterSheetTexts.PointsAvailable, points)
                    : CharacterSheetTexts.Get(manager, CharacterSheetTexts.NoPoints);
                _pointsLabel.EnableInClassList("character-sheet-points--available", canSpend);
            }

            AttributeType? primary = null;
            PlayableCharacter hero = CurrentHero();
            if (hero != null && AttributeSet.TryParsePrimaryAttribute(hero.PrimaryAttribute, out AttributeType parsed))
            {
                primary = parsed;
            }

            foreach (KeyValuePair<AttributeType, AttributeRow> pair in _rows)
            {
                AttributeRow row = pair.Value;
                int total = _attributes != null ? _attributes.GetTotal(pair.Key) : 0;
                int talent = _attributes != null ? _attributes.GetPassiveBonus(pair.Key) : 0;
                bool isPrimary = primary == pair.Key;

                row.Value.text = CharacterSheetPresenter.FormatAttributeValue(total);
                row.Talent.text = talent > 0 ? CharacterSheetTexts.Format(manager, CharacterSheetTexts.TalentBonus, talent) : string.Empty;
                row.Talent.style.display = talent > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                row.PrimaryTag.style.display = isPrimary ? DisplayStyle.Flex : DisplayStyle.None;
                row.Root.EnableInClassList("character-sheet-attribute-row--primary", isPrimary);
                row.Spend.SetEnabled(canSpend && _attributes != null);
            }
        }

        private void RefreshDerived(LocalizationManager manager)
        {
            if (_derivedList == null)
            {
                return;
            }

            CharacterDerivedStats stats = CharacterSheetPresenter.BuildDerivedStats(_playerStats, _buffs, _combatBalance);
            _derivedList.Clear();
            AddStatRow(manager, CharacterSheetTexts.StatMaxHealth, CharacterSheetPresenter.FormatFlat(stats.MaxHealth));
            AddStatRow(manager, CharacterSheetTexts.StatMaxMana, CharacterSheetPresenter.FormatFlat(stats.MaxMana));
            AddStatRow(manager, CharacterSheetTexts.StatMaxStamina, CharacterSheetPresenter.FormatFlat(stats.MaxStamina));
            AddStatRow(manager, CharacterSheetTexts.StatCritChance, CharacterSheetPresenter.FormatPercent(stats.CriticalChance, manager));
            AddStatRow(manager, CharacterSheetTexts.StatCritMultiplier, CharacterSheetPresenter.FormatMultiplier(stats.CriticalMultiplier, manager));
            AddStatRow(manager, CharacterSheetTexts.StatCooldownReduction, CharacterSheetPresenter.FormatPercent(stats.CooldownReduction, manager));
            AddStatRow(manager, CharacterSheetTexts.StatSkillDamage, "+" + CharacterSheetPresenter.FormatPercent(stats.SkillDamageBonus, manager));
            // Gear-only stats: shown once an affix grants them, so the base sheet stays short.
            if (Mathf.Abs(stats.AttackSpeedBonus) > 0.0001f)
            {
                string sign = stats.AttackSpeedBonus > 0f ? "+" : string.Empty;
                AddStatRow(manager, CharacterSheetTexts.StatAttackSpeed, sign + CharacterSheetPresenter.FormatPercent(stats.AttackSpeedBonus, manager));
            }

            if (stats.Thorns > 0.0001f)
            {
                AddStatRow(manager, CharacterSheetTexts.StatThorns, CharacterSheetPresenter.FormatPercent(stats.Thorns, manager));
            }
        }

        private void AddStatRow(LocalizationManager manager, string labelKey, string value)
        {
            var row = new VisualElement();
            row.AddToClassList("character-sheet-stat-row");
            row.pickingMode = PickingMode.Ignore;

            Label name = NewLabel("character-sheet-stat-name");
            name.text = CharacterSheetTexts.Get(manager, labelKey);
            Label valueLabel = NewLabel("character-sheet-stat-value");
            valueLabel.text = value;

            row.Add(name);
            row.Add(valueLabel);
            _derivedList.Add(row);
        }

        private void RefreshEquipment(LocalizationManager manager)
        {
            if (_equipmentList == null)
            {
                return;
            }

            _equipmentList.Clear();
            bool any = false;

            if (_inventory != null)
            {
                foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
                {
                    if (slot == EquipmentSlot.None || !_inventory.Equipped.TryGetValue(slot, out InventorySlotData data) || data == null || data.IsEmpty)
                    {
                        continue;
                    }

                    ItemDefinition item = ItemDatabase.GetById(data.itemId);
                    string itemName = GeneratedItemName.Compose(item, data, manager);
                    if (string.IsNullOrEmpty(itemName))
                    {
                        continue;
                    }

                    Label row = NewLabel("character-sheet-equipment-row");
                    row.text = $"{SlotName(slot, manager)}: {itemName}";
                    _equipmentList.Add(row);
                    any = true;
                }
            }

            if (!any)
            {
                Label empty = NewLabel("character-sheet-equipment-empty");
                empty.text = CharacterSheetTexts.Get(manager, CharacterSheetTexts.EquipmentEmpty);
                _equipmentList.Add(empty);
            }
        }

        private static string SlotName(EquipmentSlot slot, LocalizationManager manager)
        {
            string key = "ui.tooltip.slot." + TooltipLocalization.ToSnakeCase(slot.ToString());

            if (manager != null)
            {
                TooltipLocalization.EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            return TooltipLocalization.GetFallback(key) ?? slot.ToString();
        }

        // ------------------------------------------------------------------
        // Treino + Roda de Talentos (Docs/Balance/README.md §27)
        // ------------------------------------------------------------------

        /// <summary>B/Esc: closes the wheel first (back to the sheet), then the sheet.</summary>
        private void OnCancel()
        {
            if (_wheel != null && _wheel.IsOpen)
            {
                _wheel.Close();
                _wheelButton?.Focus();
                return;
            }

            Hide();
        }

        /// <summary>The "Roda de Talentos" button under the attributes and the "Treino" rows under the status (built in code).</summary>
        private void BuildTalentElements(VisualElement documentRoot)
        {
            if (_root == null)
            {
                return;
            }

            StyleSheet sheet = Resources.Load<StyleSheet>(TalentWheelView.StyleResourcePath);
            if (sheet != null && !_root.styleSheets.Contains(sheet))
            {
                _root.styleSheets.Add(sheet);
            }

            if (_attributeList?.parent != null && _wheelButton == null)
            {
                _wheelButton = new Button(OpenWheel) { name = "character-sheet-talent-wheel-button" };
                _wheelButton.AddToClassList("talent-open-button");
                _wheelButton.AddToClassList("rap-button");
                VisualElement column = _attributeList.parent;
                column.Insert(column.IndexOf(_attributeList) + 1, _wheelButton);
            }

            if (_derivedList?.parent != null && _trainingList == null)
            {
                VisualElement column = _derivedList.parent;
                _trainingHeader = NewLabel("character-sheet-section-header");
                _trainingHeader.AddToClassList("character-sheet-section-header--spaced");
                _trainingList = new VisualElement { name = "character-sheet-training-list" };
                _trainingList.pickingMode = PickingMode.Ignore;
                int at = column.IndexOf(_derivedList) + 1;
                column.Insert(at, _trainingHeader);
                column.Insert(at + 1, _trainingList);
            }

            VisualElement window = documentRoot.Q<VisualElement>("character-sheet-window");
            _wheel ??= new TalentWheelView(_root, window, () => Localization, () => (_playerStats, _buffs, _inventory));
        }

        private void OpenWheel()
        {
            ResolvePlayer();
            _wheel?.Open();
        }

        private void RefreshTalents(LocalizationManager manager)
        {
            if (_wheelButton != null)
            {
                _wheelButton.text = TalentTexts.Get(manager, TalentTexts.OpenButton);
            }

            if (_trainingHeader != null)
            {
                _trainingHeader.text = TalentTexts.Get(manager, TalentTexts.TrainingHeader);
            }

            if (_trainingList != null)
            {
                _trainingList.Clear();
                if (TalentProgress.TryActiveHero(out CharacterId hero))
                {
                    foreach (ProficiencySpec spec in ProficiencyRules.Specs)
                    {
                        Label row = NewLabel("talent-training-row");
                        row.text = TalentWheelPresenter.TrainingText(TalentProgress.ResolveShared().Proficiency(hero, spec.Kind), manager);
                        _trainingList.Add(row);
                    }
                }
            }

            if (_wheel != null && _wheel.IsOpen)
            {
                _wheel.Refresh();
            }
        }

        private Button FirstSpendableButton()
        {
            foreach (AttributeType type in CharacterSheetPresenter.AttributeOrder)
            {
                if (_rows.TryGetValue(type, out AttributeRow row) && row.Spend.enabledSelf)
                {
                    return row.Spend;
                }
            }

            return null;
        }
    }
}
