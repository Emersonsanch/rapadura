using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.ClassEvolution;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using Rapadura.UI.Hunting;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.ClassEvolution
{
    /// <summary>
    /// "Graduação" on the world map (Docs/ClassEvolution.md §5). Self-bootstrapping like <see cref="HuntingBoardPanel"/>
    /// (no scene edit): a button in the HUD's <c>wm-action-row</c> in the Vila, where the family gathers at Vó Edna's house,
    /// and a panel built in code (Mochila overlay classes + <c>Resources/UI/Hunting.uss</c>) with the cousin's title, the
    /// next family mission with its dialogue and "Aceitar"/"Entregar", the class combo and the master tree. The new skill,
    /// look and master bonuses reach the hero when the next region scene loads (<see cref="ClassEvolutionProgress"/> applies
    /// them on <see cref="GameLoadedEvent"/>). One of the HUD's screens (Esc closes it); holds GameplayPause; auto-saves on close.
    /// </summary>
    [DisallowMultipleComponent]
    public class ClassEvolutionPanel : MonoBehaviour
    {
        public const string ButtonName = "wm-class-button";
        public const string OverlayName = "wm-class-overlay";
        public const string ScreenId = "class_evolution";
        private const string PlayerStatsSaveKey = "PlayerStats";
        private const float RefreshInterval = 0.25f;

        private static ClassEvolutionPanel _instance;

        private WorldMapHudController _hud;
        private UIDocument _hudDocument;
        private WorldMapController _worldMap;
        private Button _button;
        private VisualElement _overlay;
        private Label _titleLabel;
        private Label _heroLabel;
        private VisualElement _missionSection;
        private VisualElement _comboSection;
        private VisualElement _masterSection;
        private Label _statusLabel;
        private Button _closeButton;
        private bool _open;
        private bool _dirty;
        private bool _screenRegistered;
        private float _nextRefresh;

        public bool IsOpen => _open;

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
            if (hud != null && hud.GetComponent<ClassEvolutionPanel>() == null)
            {
                hud.gameObject.AddComponent<ClassEvolutionPanel>();
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
            EventBus.Subscribe<GraduationMissionChangedEvent>(OnMissionChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GraduationMissionChangedEvent>(OnMissionChanged);
            Close();
            if (_button != null)
            {
                _button.clicked -= OnButtonClicked;
                _button.RemoveFromHierarchy();
                _button = null;
            }

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
            if (_open && (_hud == null || !_hud.IsVisible))
            {
                Close();
            }
        }

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        private static ClassEvolutionProgress Progress => ClassEvolutionProgress.ResolveShared();

        private static CharacterId? ActiveHero() =>
            ServiceLocator.TryGet(out CharacterSelectionState selection) && selection != null ? selection.Selected : null;

        private static int HeroLevel()
        {
            string json = null;
            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.TryGetCarriedJson(PlayerStatsSaveKey, out json);
            }

            return WorldMapHudPresenter.ParseLevel(json);
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
            bool show = ActiveHero().HasValue && HuntingBoardPanel.ShouldShowButton(_hud != null && _hud.IsVisible,
                _worldMap != null && _worldMap.CanExplore, GameplayPause.IsPaused, dialogue != null && dialogue.IsActive, CurrentRegion());
            _button.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (show)
            {
                string label = ClassEvolutionTexts.Get(Localization, ClassEvolutionTexts.Button);
                if (_button.text != label)
                {
                    _button.text = label;
                }
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

        private void OnMissionChanged(GraduationMissionChangedEvent evt)
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
            StyleSheet sheet = Resources.Load<StyleSheet>(HuntingBoardPanel.StyleResourcePath);
            if (sheet != null)
            {
                _overlay.styleSheets.Add(sheet);
            }

            var panel = new VisualElement { name = "wm-class-panel" };
            panel.AddToClassList("rap-panel");
            panel.AddToClassList("wm-backpack-panel");
            panel.AddToClassList("hunting-panel");
            _overlay.Add(panel);

            _titleLabel = AddLabel(panel, "wm-backpack-title");
            _heroLabel = AddLabel(panel, "hunting-intro");

            var scroll = new ScrollView { name = "wm-class-scroll" };
            scroll.AddToClassList("hunting-scroll");
            panel.Add(scroll);
            _missionSection = AddSection(scroll, "wm-class-mission");
            _comboSection = AddSection(scroll, "wm-class-combo");
            _masterSection = AddSection(scroll, "wm-class-master");

            _statusLabel = AddLabel(panel, "hunting-status");
            _closeButton = new Button(Close) { name = "wm-class-close-button", focusable = true };
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

        // ------------------------------------------------------------------ render

        private void Render()
        {
            ClassEvolutionProgress progress = Progress;
            if (progress == null || _overlay == null)
            {
                return;
            }

            LocalizationManager localization = Localization;
            _titleLabel.text = ClassEvolutionTexts.Get(localization, ClassEvolutionTexts.Title);
            _closeButton.text = ClassEvolutionTexts.Get(localization, ClassEvolutionTexts.Close);
            _missionSection.Clear();
            _comboSection.Clear();
            _masterSection.Clear();

            CharacterId? selected = ActiveHero();
            if (!selected.HasValue)
            {
                _heroLabel.text = ClassEvolutionTexts.Get(localization, ClassEvolutionTexts.NoHero);
                return;
            }

            CharacterId hero = selected.Value;
            int level = HeroLevel();
            GraduationRecord record = progress.Record(hero);
            _heroLabel.text = ClassEvolutionPresenter.HeroName(hero) + " — " + ClassEvolutionPresenter.TitleLine(localization, hero, record.tier)
                + "\n" + ClassEvolutionPresenter.TierLine(localization, record.tier);

            RenderMission(progress, localization, hero, record, level);
            RenderCombo(localization, hero, record);
            RenderMaster(progress, localization, hero, record, level);
        }

        private void RenderMission(ClassEvolutionProgress progress, LocalizationManager localization, CharacterId hero, GraduationRecord record, int level)
        {
            AddLabel(_missionSection, "hunting-section-title").text = ClassEvolutionTexts.Get(localization, ClassEvolutionTexts.SectionMission);
            AddLabel(_missionSection, "hunting-line").text = ClassEvolutionPresenter.StatusLine(localization, hero, record, level);
            foreach (string line in ClassEvolutionPresenter.DialogueLines(localization, hero, record, level))
            {
                AddLabel(_missionSection, "hunting-subtle").text = line;
            }

            GraduationMissionStatus status = progress.Status(hero, level);
            if (status == GraduationMissionStatus.Available)
            {
                AddButton(_missionSection, ClassEvolutionTexts.Get(localization, ClassEvolutionTexts.Accept), () =>
                {
                    if (progress.AcceptMission(hero, level))
                    {
                        _dirty = true;
                        _statusLabel.text = string.Empty;
                        Render();
                    }
                }, primary: true);
            }
            else if (status == GraduationMissionStatus.ReadyToTurnIn)
            {
                AddButton(_missionSection, ClassEvolutionTexts.Get(localization, ClassEvolutionTexts.TurnIn), () =>
                {
                    GraduationSpec done = progress.TurnIn(hero, level);
                    if (done != null)
                    {
                        _dirty = true;
                        Render();
                        _statusLabel.text = string.Join("\n", ClassEvolutionPresenter.GraduatedLines(localization, hero, done));
                    }
                }, primary: true);
            }
        }

        private void RenderCombo(LocalizationManager localization, CharacterId hero, GraduationRecord record)
        {
            AddLabel(_comboSection, "hunting-section-title").text = ClassEvolutionTexts.Get(localization, ClassEvolutionTexts.SectionCombo);
            var lines = ClassEvolutionPresenter.ComboLines(localization, hero, record.tier);
            for (int i = 0; i < lines.Count; i++)
            {
                AddLabel(_comboSection, i == 0 ? "hunting-line" : "hunting-subtle").text = lines[i];
            }
        }

        private void RenderMaster(ClassEvolutionProgress progress, LocalizationManager localization, CharacterId hero, GraduationRecord record, int level)
        {
            AddLabel(_masterSection, "hunting-section-title").text = ClassEvolutionTexts.Get(localization, ClassEvolutionTexts.SectionMaster);
            AddLabel(_masterSection, "hunting-line").text = ClassEvolutionPresenter.MasterHeader(localization, record, level);
            bool master = progress.IsMaster(hero, level);
            if (master)
            {
                AddLabel(_masterSection, "hunting-subtle").text = ClassEvolutionPresenter.MasterXpLine(localization, record);
                AddLabel(_masterSection, "hunting-subtle").text = ClassEvolutionTexts.Format(localization, ClassEvolutionTexts.MasterPoints,
                    MasterLevelRules.AvailablePoints(record));
            }

            foreach (MasterNodeSpec node in MasterLevelRules.Nodes)
            {
                var row = new VisualElement();
                row.AddToClassList("hunting-row");
                _masterSection.Add(row);
                AddLabel(row, "hunting-subtle").text = ClassEvolutionPresenter.NodeLine(localization, record, node);
                if (!master)
                {
                    continue;
                }

                MasterNodeSpec captured = node;
                MasterSpendResult can = progress.CanSpendMasterPoint(hero, level, node.NodeId);
                Button spend = AddButton(row, ClassEvolutionTexts.Get(localization, ClassEvolutionTexts.MasterSpend), () =>
                {
                    MasterSpendResult result = progress.SpendMasterPoint(hero, level, captured.NodeId);
                    if (result == MasterSpendResult.Spent)
                    {
                        _dirty = true;
                        Render();
                        _statusLabel.text = string.Empty;
                    }
                    else
                    {
                        _statusLabel.text = ClassEvolutionPresenter.SpendFailure(localization, result, captured) ?? string.Empty;
                    }
                }, enabled: can == MasterSpendResult.Spent);
                spend.tooltip = ClassEvolutionPresenter.SpendFailure(localization, can, node) ?? string.Empty;
            }

            if (master && MasterLevelRules.SpentPoints(record) > 0)
            {
                AddButton(_masterSection, ClassEvolutionTexts.Get(localization, ClassEvolutionTexts.MasterReset), () =>
                {
                    int refunded = progress.ResetMasterTree(hero);
                    _dirty = true;
                    Render();
                    _statusLabel.text = ClassEvolutionTexts.Format(localization, ClassEvolutionTexts.MasterResetDone, refunded);
                });
            }
        }
    }
}
