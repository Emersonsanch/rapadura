using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Hunting;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.Hunting
{
    /// <summary>
    /// Seu Zé Caçador on the world map. Self-bootstrapping like <c>FishingSpotButton</c> (no scene edit, no change to
    /// the city hub panel): added to the <see cref="WorldMapHudController"/>'s GameObject after every scene load.
    /// <list type="bullet">
    /// <item>"Patig, o Rastreador" button in the HUD's <c>wm-action-row</c>, in the Vila (Vó Edna's region,
    /// <see cref="VillageNpcCatalog.VillageRegion"/>) with the same gating as the city button
    /// (<see cref="VillageHubButton.ShouldShow"/>). <see cref="OpenShared"/> lets the city hub open the board too.</item>
    /// <item>The board (built in code on the HUD's document, under <c>wm-hud-root</c>, with the Mochila overlay classes
    /// and <c>Resources/UI/Hunting.uss</c>): the running task (progress, reward, "Desistir") or the creatures and sizes
    /// to take one; today's three prey with their bonus, "Escolher" and the paid reroll; "Pontos de Caça", the hunter
    /// rank and the trade for a "Ponto do Caderno". One of the HUD's screens (Esc closes it); holds GameplayPause.</item>
    /// <item>"Bicho destaque do dia" chip in the HUD's region block, whenever the HUD is visible.</item>
    /// <item>The reroll's gold comes out of the carried bag through a short <see cref="CarriedInventorySession"/>
    /// (as the kitchen and the carro de boi do); an auto-save follows any change when the board closes.</item>
    /// </list>
    /// </summary>
    [DisallowMultipleComponent]
    public class HuntingBoardPanel : MonoBehaviour
    {
        public const string ButtonName = "wm-hunting-button";
        public const string OverlayName = "wm-hunting-overlay";
        public const string BoostedChipName = "wm-hunting-boosted";
        public const string ScreenId = "hunting";
        public const string StyleResourcePath = "UI/Hunting";
        public const string GoldItemId = "item_gold";

        private const float RefreshInterval = 0.25f;

        private static HuntingBoardPanel _instance;

        private WorldMapHudController _hud;
        private UIDocument _hudDocument;
        private WorldMapController _worldMap;
        private Button _button;
        private Label _boostedChip;
        private VisualElement _overlay;
        private Label _titleLabel;
        private Label _greetingLabel;
        private VisualElement _taskSection;
        private VisualElement _preySection;
        private VisualElement _pointsSection;
        private Label _statusLabel;
        private Button _closeButton;
        private bool _open;
        private bool _dirty;
        private bool _screenRegistered;
        private float _nextRefresh;

        public bool IsOpen => _open;

        /// <summary>Opens the board on the current map HUD (e.g. from the city hub). False when there is none.</summary>
        public static bool OpenShared()
        {
            if (_instance == null)
            {
                AttachToHud();
            }

            if (_instance == null)
            {
                return false;
            }

            _instance.Open();
            return _instance._open;
        }

        // ------------------------------------------------------------------ bootstrap

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
            if (hud != null && hud.GetComponent<HuntingBoardPanel>() == null)
            {
                hud.gameObject.AddComponent<HuntingBoardPanel>();
            }
        }

        private void Awake()
        {
            _instance = this;
            _hud = GetComponent<WorldMapHudController>();
            _hudDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<HuntingStateChangedEvent>(OnHuntingChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<HuntingStateChangedEvent>(OnHuntingChanged);
            Close();
            if (_button != null)
            {
                _button.clicked -= OnButtonClicked;
                _button.RemoveFromHierarchy();
                _button = null;
            }

            _boostedChip?.RemoveFromHierarchy();
            _boostedChip = null;
            _overlay?.RemoveFromHierarchy();
            _overlay = null;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + RefreshInterval;
            if (!_screenRegistered && _hud != null)
            {
                _hud.RegisterScreen(ScreenId, () => _open, Open, Close);
                _screenRegistered = true;
            }

            RefreshButton();
            RefreshBoostedChip();
            if (_open && (_hud == null || !_hud.IsVisible))
            {
                Close();
            }
        }

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        private static HuntingProgress Progress => HuntingProgress.ResolveShared();

        /// <summary>Whether Seu Zé's button is on screen: the city button's gating, in the Vila only.</summary>
        public static bool ShouldShowButton(bool hudVisible, bool tokenIdle, bool paused, bool dialogueActive, BiomeType? currentRegion)
        {
            return currentRegion == VillageNpcCatalog.VillageRegion && VillageHubButton.ShouldShow(hudVisible, tokenIdle, paused, dialogueActive, currentRegion);
        }

        private BiomeType? CurrentRegion()
        {
            if (_worldMap == null)
            {
                _worldMap = FindFirstObjectByType<WorldMapController>();
            }

            return _worldMap != null && _worldMap.CurrentNode != null ? _worldMap.CurrentNode.Region : (BiomeType?)null;
        }

        private void RefreshButton()
        {
            if (_hudDocument == null || _hudDocument.rootVisualElement == null)
            {
                return;
            }

            VisualElement row = _hudDocument.rootVisualElement.Q<VisualElement>(RegionNpcTalkButton.ActionRowName);
            if (row == null)
            {
                return;
            }

            if (_button == null || _button.parent != row)
            {
                if (_button != null)
                {
                    _button.clicked -= OnButtonClicked;
                    _button.RemoveFromHierarchy();
                }

                _button = new Button { name = ButtonName, focusable = true };
                _button.AddToClassList("wm-explore-button");
                _button.AddToClassList(RegionNpcTalkButton.ButtonClass);
                _button.style.marginLeft = 8f;
                _button.style.display = DisplayStyle.None;
                _button.clicked += OnButtonClicked;
                row.Add(_button);
            }

            ServiceLocator.TryGet(out DialogueManager dialogue);
            bool show = Progress != null && ShouldShowButton(_hud != null && _hud.IsVisible, _worldMap != null && _worldMap.CanExplore,
                GameplayPause.IsPaused, dialogue != null && dialogue.IsActive, CurrentRegion());
            _button.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (show)
            {
                string label = HuntingTexts.Get(Localization, HuntingTexts.Button);
                if (_button.text != label)
                {
                    _button.text = label;
                }
            }
        }

        private void RefreshBoostedChip()
        {
            VisualElement block = _hudDocument != null && _hudDocument.rootVisualElement != null
                ? _hudDocument.rootVisualElement.Q<VisualElement>("wm-region-block")
                : null;
            if (block == null)
            {
                return;
            }

            if (_boostedChip == null || _boostedChip.parent != block)
            {
                _boostedChip?.RemoveFromHierarchy();
                _boostedChip = new Label { name = BoostedChipName, pickingMode = PickingMode.Ignore };
                _boostedChip.AddToClassList("hunting-boosted-chip");
                StyleSheet sheet = Resources.Load<StyleSheet>(StyleResourcePath);
                if (sheet != null)
                {
                    _boostedChip.styleSheets.Add(sheet);
                }

                VisualElement actionRow = block.Q<VisualElement>(RegionNpcTalkButton.ActionRowName);
                int index = actionRow != null && actionRow.parent == block ? block.IndexOf(actionRow) : block.childCount;
                block.Insert(index, _boostedChip);
            }

            HuntingProgress progress = Progress;
            LocalizationManager localization = Localization;
            string text = progress != null ? HuntingPresenter.BoostedHudText(localization, HuntingPresenter.CreatureName(localization, progress.BoostedId)) : null;
            bool show = text != null && _hud != null && _hud.IsVisible && !string.IsNullOrEmpty(progress.BoostedId);
            _boostedChip.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (show && _boostedChip.text != text)
            {
                _boostedChip.text = text;
            }
        }

        private void OnButtonClicked()
        {
            if (GameplayPause.IsPaused || (ServiceLocator.TryGet(out DialogueManager dialogue) && dialogue.IsActive))
            {
                return;
            }

            Open();
        }

        private void OnHuntingChanged(HuntingStateChangedEvent evt)
        {
            if (_open)
            {
                Render();
            }
        }

        // ------------------------------------------------------------------ open / close

        public void Open()
        {
            if (_open || Progress == null || !EnsureOverlay())
            {
                return;
            }

            Progress.Refresh();
            _open = true;
            _dirty = false;
            _statusLabel.text = string.Empty;
            _overlay.style.display = DisplayStyle.Flex;
            GameplayPause.Acquire(this);
            Render();
            _closeButton.Focus();
        }

        public void Close()
        {
            if (_overlay != null)
            {
                _overlay.style.display = DisplayStyle.None;
            }

            if (!_open)
            {
                return;
            }

            _open = false;
            GameplayPause.Release(this);
            if (_dirty && ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.AutoSave();
            }

            _dirty = false;
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
            _overlay = new VisualElement { name = OverlayName };
            _overlay.AddToClassList("wm-backpack-overlay");
            _overlay.style.display = DisplayStyle.None;
            StyleSheet sheet = Resources.Load<StyleSheet>(StyleResourcePath);
            if (sheet != null)
            {
                _overlay.styleSheets.Add(sheet);
            }

            var panel = new VisualElement { name = "wm-hunting-panel" };
            panel.AddToClassList("rap-panel");
            panel.AddToClassList("wm-backpack-panel");
            panel.AddToClassList("hunting-panel");
            _overlay.Add(panel);

            _titleLabel = AddLabel(panel, "wm-backpack-title");
            _greetingLabel = AddLabel(panel, "hunting-intro");

            var scroll = new ScrollView { name = "wm-hunting-scroll" };
            scroll.AddToClassList("hunting-scroll");
            panel.Add(scroll);
            _taskSection = AddSection(scroll, "wm-hunting-task");
            _preySection = AddSection(scroll, "wm-hunting-prey");
            _pointsSection = AddSection(scroll, "wm-hunting-points");

            _statusLabel = AddLabel(panel, "hunting-status");
            _closeButton = new Button(Close) { name = "wm-hunting-close-button", focusable = true };
            _closeButton.AddToClassList("rap-button");
            _closeButton.AddToClassList("rap-button--primary");
            _closeButton.AddToClassList("wm-backpack-close");
            panel.Add(_closeButton);

            root.Add(_overlay);
            return true;
        }

        private static Label AddLabel(VisualElement parent, string className)
        {
            var label = new Label { pickingMode = PickingMode.Ignore };
            label.AddToClassList(className);
            parent.Add(label);
            return label;
        }

        private static VisualElement AddSection(VisualElement parent, string name)
        {
            var section = new VisualElement { name = name };
            section.AddToClassList("hunting-section");
            parent.Add(section);
            return section;
        }

        private static Button AddButton(VisualElement parent, string text, System.Action onClick, bool enabled = true, bool primary = false)
        {
            var button = new Button(onClick) { text = text, focusable = true };
            button.AddToClassList("rap-button");
            button.AddToClassList("hunting-button");
            if (primary)
            {
                button.AddToClassList("rap-button--primary");
            }

            button.SetEnabled(enabled);
            parent.Add(button);
            return button;
        }

        private static VisualElement AddRow(VisualElement parent)
        {
            var row = new VisualElement();
            row.AddToClassList("hunting-row");
            parent.Add(row);
            return row;
        }

        // ------------------------------------------------------------------ render

        private void Render()
        {
            HuntingProgress progress = Progress;
            if (progress == null || _overlay == null)
            {
                return;
            }

            LocalizationManager localization = Localization;
            CharacterId? hero = ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;
            _titleLabel.text = HuntingTexts.Get(localization, HuntingTexts.Title);
            _greetingLabel.text = HuntingPresenter.Greeting(localization, hero);
            _closeButton.text = HuntingTexts.Get(localization, HuntingTexts.Close);

            RenderTask(progress, localization);
            RenderPrey(progress, localization);
            RenderPoints(progress, localization);
        }

        private void RenderTask(HuntingProgress progress, LocalizationManager localization)
        {
            _taskSection.Clear();
            AddLabel(_taskSection, "hunting-section-title").text = HuntingTexts.Get(localization, HuntingTexts.SectionTask);

            HuntingTask task = progress.ActiveTask;
            if (task != null)
            {
                AddLabel(_taskSection, "hunting-line").text = HuntingPresenter.ActiveTaskText(localization, task, HuntingPresenter.CreatureName(localization, task.enemyId));
                AddLabel(_taskSection, "hunting-subtle").text = HuntingPresenter.RewardText(localization, task);
                AddButton(_taskSection, HuntingTexts.Get(localization, HuntingTexts.TaskCancel), () =>
                {
                    if (progress.CancelTask())
                    {
                        _dirty = true;
                    }
                });
                return;
            }

            if (!progress.CanTakeTask)
            {
                AddLabel(_taskSection, "hunting-line").text = HuntingTexts.Format(localization, HuntingTexts.TaskCap, HuntingRules.MaxTasksPerDay);
                return;
            }

            AddLabel(_taskSection, "hunting-subtle").text = HuntingTexts.Get(localization, HuntingTexts.TaskNone);
            foreach (HuntingCreature creature in progress.TaskCreatures())
            {
                VisualElement row = AddRow(_taskSection);
                AddLabel(row, "hunting-creature").text = HuntingPresenter.CreatureName(localization, creature.Id);
                foreach (HuntingTaskTier tier in new[] { HuntingTaskTier.Small, HuntingTaskTier.Medium, HuntingTaskTier.Large })
                {
                    string id = creature.Id;
                    HuntingTask preview = HuntingRules.CreateTask(creature, tier);
                    Button button = AddButton(row, HuntingPresenter.TierButton(localization, tier), () =>
                    {
                        if (progress.TakeTask(id, tier))
                        {
                            _dirty = true;
                        }
                    });
                    button.tooltip = HuntingPresenter.RewardText(localization, preview);
                }
            }
        }

        private void RenderPrey(HuntingProgress progress, LocalizationManager localization)
        {
            _preySection.Clear();
            AddLabel(_preySection, "hunting-section-title").text = HuntingTexts.Get(localization, HuntingTexts.SectionPrey);

            PreyOffer active = progress.ActivePrey;
            AddLabel(_preySection, "hunting-line").text = HuntingPresenter.PreyStatus(localization, active, progress.PreyOffers.Count,
                active != null ? HuntingPresenter.CreatureName(localization, active.enemyId) : null);

            IReadOnlyList<PreyOffer> offers = progress.PreyOffers;
            for (int i = 0; i < offers.Count; i++)
            {
                int index = i;
                bool picked = progress.PreyPickIndex == i;
                VisualElement row = AddRow(_preySection);
                row.EnableInClassList("hunting-row--picked", picked);
                AddLabel(row, "hunting-creature").text = HuntingPresenter.CreatureName(localization, offers[i].enemyId);
                AddLabel(row, "hunting-bonus").text = HuntingPresenter.PreyBonusText(localization, offers[i].bonus);
                AddButton(row, HuntingTexts.Get(localization, picked ? HuntingTexts.PreyPicked : HuntingTexts.PreyPick), () =>
                {
                    if (progress.PickPrey(index))
                    {
                        _dirty = true;
                    }
                }, enabled: !picked);
            }

            bool rerollLeft = progress.PreyRerollsToday < HuntingRules.MaxPreyRerollsPerDay;
            string rerollText = rerollLeft
                ? HuntingTexts.Format(localization, HuntingTexts.PreyReroll, HuntingRules.PreyRerollGold)
                : HuntingTexts.Get(localization, HuntingTexts.PreyRerollUsed);
            AddButton(_preySection, rerollText, () => Reroll(progress, localization), enabled: rerollLeft && offers.Count > 0);
            AddLabel(_preySection, "hunting-subtle").text = HuntingTexts.Get(localization, HuntingTexts.BossesExcluded);
        }

        private void RenderPoints(HuntingProgress progress, LocalizationManager localization)
        {
            _pointsSection.Clear();
            AddLabel(_pointsSection, "hunting-section-title").text = HuntingTexts.Get(localization, HuntingTexts.SectionPoints);
            AddLabel(_pointsSection, "hunting-line").text = HuntingTexts.Format(localization, HuntingTexts.HuntPoints, progress.HuntPointsAvailable);
            AddLabel(_pointsSection, "hunting-subtle").text = HuntingPresenter.RankText(localization, progress.HunterRank);
            AddLabel(_pointsSection, "hunting-subtle").text = HuntingTexts.Format(localization, HuntingTexts.CharmPoints, progress.CadernoPointsAvailable);
            AddButton(_pointsSection, HuntingTexts.Format(localization, HuntingTexts.TradePoints, HuntingRules.HuntPointsPerCadernoPoint), () =>
            {
                if (progress.TradeHuntPoints())
                {
                    _dirty = true;
                }
            }, enabled: progress.CanTradeHuntPoints);
        }

        private void Reroll(HuntingProgress progress, LocalizationManager localization)
        {
            ServiceLocator.TryGet(out SaveManager saveManager);
            var session = new CarriedInventorySession(saveManager);
            InventoryManager bag = session.Open(transform);
            try
            {
                int gold = bag != null ? bag.GetTotalCount(GoldItemId) : 0;
                bool done = progress.RerollPrey(gold, cost => bag != null && bag.RemoveById(GoldItemId, cost));
                if (done)
                {
                    _dirty = true;
                    _statusLabel.text = string.Empty;
                }
                else if (!progress.CanRerollPrey(gold) && progress.PreyRerollsToday < HuntingRules.MaxPreyRerollsPerDay)
                {
                    _statusLabel.text = HuntingTexts.Format(localization, HuntingTexts.PreyNoGold, HuntingRules.PreyRerollGold);
                }
            }
            finally
            {
                session.Close();
            }
        }
    }
}
