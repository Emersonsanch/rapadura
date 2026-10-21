using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Quests;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Quests
{
    /// <summary>
    /// Drives the Quest Log screen (QuestLogView.uxml/.uss), same single-<see cref="UIDocument"/>,
    /// <c>Q&lt;T&gt;()</c>-resolved-once pattern as <c>SkillTreeController</c>/<c>ShopUIController</c>.
    /// Two tabs (Ativas / Concluídas), a quest list on the left and a detail pane on the right
    /// (description, "Diário" journal note, objectives with live "3/5" progress, rewards, and an
    /// "Acompanhar" button that picks the quest shown by <see cref="QuestTrackerController"/>).
    ///
    /// Purely reactive: never polls. It listens to <see cref="QuestStartedEvent"/>/
    /// <see cref="QuestObjectiveUpdatedEvent"/>/<see cref="QuestCompletedEvent"/>/
    /// <see cref="QuestAbandonedEvent"/> plus <see cref="GameLoadedEvent"/> (QuestManager.RestoreState
    /// publishes nothing) and <see cref="LocalizationManager.LanguageChanged"/>; while hidden it only
    /// marks itself dirty and rebuilds on the next <see cref="Show"/>.
    ///
    /// Opened via <see cref="Open"/>/<see cref="Toggle"/> — same contract the HUD uses for
    /// <c>SkillTreeController.Toggle()</c>/<c>PauseMenuController.Toggle()</c>; tapping the compact
    /// tracker also opens it (see <see cref="QuestTrackerController"/>).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class QuestLogController : MonoBehaviour
    {
        public enum Tab
        {
            Active,
            Completed
        }

        [Header("Bootstrap (optional)")]
        [Tooltip("Quests started automatically on Start (prerequisites still apply). Stopgap until NPC " +
                 "quest-givers call QuestManager.StartQuest — leave empty once they do.")]
        [SerializeField] private QuestDefinition[] _autoStartQuests;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private Label _titleLabel;
        private Button _closeButton;
        private Button _activeTabButton;
        private Button _completedTabButton;
        private VisualElement _listContainer;
        private Label _listEmptyLabel;
        private VisualElement _detailContainer;
        private Label _detailTitleLabel;
        private Label _detailTypeLabel;
        private Label _detailDescriptionLabel;
        private VisualElement _journalSection;
        private Label _journalHeaderLabel;
        private Label _journalLabel;
        private VisualElement _objectivesSection;
        private Label _objectivesHeaderLabel;
        private VisualElement _objectivesContainer;
        private VisualElement _rewardsSection;
        private Label _rewardsHeaderLabel;
        private Label _rewardsLabel;
        private Button _trackButton;

        private readonly QuestLogDataSource _data = new QuestLogDataSource();
        private LocalizationManager _subscribedLocalization;
        private List<QuestEntryViewModel> _visibleEntries = new List<QuestEntryViewModel>();
        private Tab _currentTab = Tab.Active;
        private string _selectedQuestId;
        private bool _isOpen;
        private bool _dirty = true;

        public bool IsOpen => _isOpen;
        public Tab CurrentTab => _currentTab;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            BindElements();
            RegisterCallbacks();
            Hide();
        }

        private void Start()
        {
            _data.EnsureResolved();
            SubscribeLanguage();
            AutoStartQuests();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
            GameplayPause.Release(this);
        }

        private void BindElements()
        {
            VisualElement rootVisualElement = _uiDocument.rootVisualElement;

            _root = rootVisualElement.Q<VisualElement>("quest-log-root");
            _titleLabel = rootVisualElement.Q<Label>("quest-log-title");
            _closeButton = rootVisualElement.Q<Button>("quest-log-close-button");
            _activeTabButton = rootVisualElement.Q<Button>("quest-tab-active");
            _completedTabButton = rootVisualElement.Q<Button>("quest-tab-completed");
            _listContainer = rootVisualElement.Q<VisualElement>("quest-list");
            _listEmptyLabel = rootVisualElement.Q<Label>("quest-list-empty-label");
            _detailContainer = rootVisualElement.Q<VisualElement>("quest-detail");
            _detailTitleLabel = rootVisualElement.Q<Label>("quest-detail-title");
            _detailTypeLabel = rootVisualElement.Q<Label>("quest-detail-type");
            _detailDescriptionLabel = rootVisualElement.Q<Label>("quest-detail-description");
            _journalSection = rootVisualElement.Q<VisualElement>("quest-detail-journal-section");
            _journalHeaderLabel = rootVisualElement.Q<Label>("quest-detail-journal-header");
            _journalLabel = rootVisualElement.Q<Label>("quest-detail-journal");
            _objectivesSection = rootVisualElement.Q<VisualElement>("quest-detail-objectives-section");
            _objectivesHeaderLabel = rootVisualElement.Q<Label>("quest-detail-objectives-header");
            _objectivesContainer = rootVisualElement.Q<VisualElement>("quest-detail-objectives");
            _rewardsSection = rootVisualElement.Q<VisualElement>("quest-detail-rewards-section");
            _rewardsHeaderLabel = rootVisualElement.Q<Label>("quest-detail-rewards-header");
            _rewardsLabel = rootVisualElement.Q<Label>("quest-detail-rewards");
            _trackButton = rootVisualElement.Q<Button>("quest-detail-track-button");
        }

        private void RegisterCallbacks()
        {
            if (_closeButton != null) _closeButton.clicked += Hide;
            if (_activeTabButton != null) _activeTabButton.clicked += OnActiveTabClicked;
            if (_completedTabButton != null) _completedTabButton.clicked += OnCompletedTabClicked;
            if (_trackButton != null) _trackButton.clicked += OnTrackClicked;

            EventBus.Subscribe<QuestStartedEvent>(OnQuestStarted);
            EventBus.Subscribe<QuestObjectiveUpdatedEvent>(OnQuestObjectiveUpdated);
            EventBus.Subscribe<QuestCompletedEvent>(OnQuestCompleted);
            EventBus.Subscribe<QuestAbandonedEvent>(OnQuestAbandoned);
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
            EventBus.Subscribe<QuestTrackedChangedEvent>(OnQuestTrackedChanged);

            SubscribeLanguage();
        }

        private void UnregisterCallbacks()
        {
            if (_closeButton != null) _closeButton.clicked -= Hide;
            if (_activeTabButton != null) _activeTabButton.clicked -= OnActiveTabClicked;
            if (_completedTabButton != null) _completedTabButton.clicked -= OnCompletedTabClicked;
            if (_trackButton != null) _trackButton.clicked -= OnTrackClicked;

            EventBus.Unsubscribe<QuestStartedEvent>(OnQuestStarted);
            EventBus.Unsubscribe<QuestObjectiveUpdatedEvent>(OnQuestObjectiveUpdated);
            EventBus.Unsubscribe<QuestCompletedEvent>(OnQuestCompleted);
            EventBus.Unsubscribe<QuestAbandonedEvent>(OnQuestAbandoned);
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
            EventBus.Unsubscribe<QuestTrackedChangedEvent>(OnQuestTrackedChanged);

            if (_subscribedLocalization != null)
            {
                _subscribedLocalization.LanguageChanged -= OnLanguageChanged;
                _subscribedLocalization = null;
            }
        }

        /// <summary>LocalizationManager may register after OnEnable, so this is retried from Start.</summary>
        private void SubscribeLanguage()
        {
            if (_subscribedLocalization != null)
            {
                return;
            }

            _data.EnsureResolved();
            _subscribedLocalization = _data.Localization;

            if (_subscribedLocalization != null)
            {
                _subscribedLocalization.LanguageChanged += OnLanguageChanged;
            }
        }

        private void AutoStartQuests()
        {
            if (_autoStartQuests == null || _data.QuestManager == null)
            {
                return;
            }

            foreach (QuestDefinition quest in _autoStartQuests)
            {
                if (quest != null)
                {
                    _data.QuestManager.StartQuest(quest);
                }
            }
        }

        // ------------------------------------------------------------------
        // Open / close
        // ------------------------------------------------------------------

        /// <summary>Opens the log on the Ativas tab — the entry point for a HUD "Missões" button or any other trigger.</summary>
        public void Open()
        {
            Show(Tab.Active);
        }

        public void Show() => Show(_currentTab);

        public void Show(Tab tab)
        {
            _isOpen = true;
            _currentTab = tab;

            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
            }

            GameplayPause.Acquire(this);
            Refresh();
        }

        public void Hide()
        {
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
        // Event handlers
        // ------------------------------------------------------------------

        private void OnQuestStarted(QuestStartedEvent evt) => MarkDirty();
        private void OnQuestObjectiveUpdated(QuestObjectiveUpdatedEvent evt) => MarkDirty();
        private void OnQuestCompleted(QuestCompletedEvent evt) => MarkDirty();
        private void OnQuestAbandoned(QuestAbandonedEvent evt) => MarkDirty();
        private void OnGameLoaded(GameLoadedEvent evt) => MarkDirty();
        private void OnQuestTrackedChanged(QuestTrackedChangedEvent evt) => MarkDirty();
        private void OnLanguageChanged(LanguageCode language) => MarkDirty();

        private void MarkDirty()
        {
            _dirty = true;

            if (_isOpen)
            {
                Refresh();
            }
        }

        private void OnActiveTabClicked() => SwitchTab(Tab.Active);
        private void OnCompletedTabClicked() => SwitchTab(Tab.Completed);

        private void SwitchTab(Tab tab)
        {
            if (_currentTab == tab && !_dirty)
            {
                return;
            }

            _currentTab = tab;
            _selectedQuestId = null;
            Refresh();
        }

        private void OnTrackClicked()
        {
            if (string.IsNullOrEmpty(_selectedQuestId) || _currentTab != Tab.Active)
            {
                return;
            }

            QuestTracking.SetPreferred(_selectedQuestId);
        }

        // ------------------------------------------------------------------
        // Rendering
        // ------------------------------------------------------------------

        private void Refresh()
        {
            _dirty = false;

            UpdateStaticTexts();

            _visibleEntries = _currentTab == Tab.Active ? _data.BuildActive() : _data.BuildCompleted();

            if (_visibleEntries.All(e => e.QuestId != _selectedQuestId))
            {
                _selectedQuestId = _visibleEntries.Count > 0 ? _visibleEntries[0].QuestId : null;
            }

            UpdateTabs();
            BuildList();
            UpdateDetail();
        }

        private void UpdateStaticTexts()
        {
            SetText(_titleLabel, QuestLogTexts.TitleKey);
            SetText(_closeButton, QuestLogTexts.CloseKey);
            SetText(_activeTabButton, QuestLogTexts.TabActiveKey);
            SetText(_completedTabButton, QuestLogTexts.TabCompletedKey);
            SetText(_journalHeaderLabel, QuestLogTexts.JournalKey);
            SetText(_objectivesHeaderLabel, QuestLogTexts.ObjectivesKey);
            SetText(_rewardsHeaderLabel, QuestLogTexts.RewardsKey);
        }

        private void SetText(TextElement element, string key)
        {
            if (element != null)
            {
                element.text = _data.Localize(key);
            }
        }

        private void UpdateTabs()
        {
            _activeTabButton?.EnableInClassList("quest-log-tab--selected", _currentTab == Tab.Active);
            _completedTabButton?.EnableInClassList("quest-log-tab--selected", _currentTab == Tab.Completed);
        }

        private void BuildList()
        {
            if (_listContainer == null)
            {
                return;
            }

            _listContainer.Clear();

            string trackedId = QuestLogPresenter.ResolveTrackedQuestId(QuestTracking.PreferredQuestId, _currentTab == Tab.Active ? _visibleEntries : null, _data.CurrentHero);

            foreach (QuestEntryViewModel entry in _visibleEntries)
            {
                _listContainer.Add(CreateListItem(entry, entry.QuestId == trackedId));
            }

            if (_listEmptyLabel != null)
            {
                bool isEmpty = _visibleEntries.Count == 0;
                _listEmptyLabel.style.display = isEmpty ? DisplayStyle.Flex : DisplayStyle.None;
                _listEmptyLabel.text = _data.Localize(_currentTab == Tab.Active ? QuestLogTexts.EmptyActiveKey : QuestLogTexts.EmptyCompletedKey);
            }
        }

        private VisualElement CreateListItem(QuestEntryViewModel entry, bool isTracked)
        {
            var button = new Button { name = $"quest-item-{entry.QuestId}", text = string.Empty };
            button.AddToClassList("quest-list-item");
            button.AddToClassList(GetTypeClass(entry.QuestType));
            button.EnableInClassList("quest-list-item--selected", entry.QuestId == _selectedQuestId);
            button.EnableInClassList("quest-list-item--completed", entry.IsCompleted);

            var title = new Label(entry.Title);
            title.AddToClassList("quest-list-item-title");
            title.pickingMode = PickingMode.Ignore;
            button.Add(title);

            string subtitleText = entry.IsCompleted
                ? $"{entry.TypeLabel} · {_data.Localize(QuestLogTexts.CompletedBadgeKey)}"
                : $"{entry.TypeLabel} · {QuestLogPresenter.FormatProgress(entry.CompletedObjectiveCount, entry.Objectives.Count)}";
            var subtitle = new Label(subtitleText);
            subtitle.AddToClassList("quest-list-item-subtitle");
            subtitle.pickingMode = PickingMode.Ignore;
            button.Add(subtitle);

            if (isTracked)
            {
                var tracked = new Label($"» {_data.Localize(QuestLogTexts.TrackingKey)}");
                tracked.AddToClassList("quest-list-item-tracked");
                tracked.pickingMode = PickingMode.Ignore;
                button.Add(tracked);
            }

            string questId = entry.QuestId;
            button.clicked += () => SelectQuest(questId);

            return button;
        }

        private static string GetTypeClass(QuestType type)
        {
            switch (type)
            {
                case QuestType.MainStory: return "quest-list-item--main";
                case QuestType.Daily: return "quest-list-item--daily";
                default: return "quest-list-item--side";
            }
        }

        private void SelectQuest(string questId)
        {
            _selectedQuestId = questId;
            BuildList();
            UpdateDetail();
        }

        private void UpdateDetail()
        {
            QuestEntryViewModel entry = _visibleEntries.FirstOrDefault(e => e.QuestId == _selectedQuestId);

            if (entry == null)
            {
                SetText(_detailTitleLabel, QuestLogTexts.SelectHintKey);
                SetDisplay(_detailTypeLabel, false);
                SetDisplay(_detailDescriptionLabel, false);
                SetDisplay(_journalSection, false);
                SetDisplay(_objectivesSection, false);
                SetDisplay(_rewardsSection, false);
                SetDisplay(_trackButton, false);
                return;
            }

            if (_detailTitleLabel != null) _detailTitleLabel.text = entry.Title;

            if (_detailTypeLabel != null)
            {
                _detailTypeLabel.text = entry.IsCompleted ? $"{entry.TypeLabel} · {_data.Localize(QuestLogTexts.CompletedBadgeKey)}" : entry.TypeLabel;
                SetDisplay(_detailTypeLabel, true);
            }

            if (_detailDescriptionLabel != null)
            {
                _detailDescriptionLabel.text = entry.Description;
                SetDisplay(_detailDescriptionLabel, true);
            }

            bool hasJournal = !string.IsNullOrEmpty(entry.Journal);
            SetDisplay(_journalSection, hasJournal);
            if (hasJournal && _journalLabel != null) _journalLabel.text = entry.Journal;

            BuildObjectives(entry);

            bool hasRewards = !string.IsNullOrEmpty(entry.RewardText);
            SetDisplay(_rewardsSection, hasRewards);
            if (hasRewards && _rewardsLabel != null) _rewardsLabel.text = entry.RewardText;

            UpdateTrackButton(entry);
        }

        private void BuildObjectives(QuestEntryViewModel entry)
        {
            SetDisplay(_objectivesSection, entry.Objectives.Count > 0);

            if (_objectivesContainer == null)
            {
                return;
            }

            _objectivesContainer.Clear();

            foreach (QuestObjectiveViewModel objective in entry.Objectives)
            {
                var row = new VisualElement();
                row.AddToClassList("quest-objective-row");
                row.EnableInClassList("quest-objective-row--complete", objective.IsComplete);

                var header = new VisualElement();
                header.AddToClassList("quest-objective-header");

                // Drawn box instead of a check glyph — the default runtime font isn't guaranteed to have one.
                var check = new VisualElement();
                check.AddToClassList("quest-objective-check");
                header.Add(check);

                var text = new Label(objective.Text);
                text.AddToClassList("quest-objective-text");
                header.Add(text);

                var progress = new Label(objective.ProgressText);
                progress.AddToClassList("quest-objective-progress");
                header.Add(progress);

                row.Add(header);

                var barTrack = new VisualElement();
                barTrack.AddToClassList("quest-objective-bar-track");
                var barFill = new VisualElement();
                barFill.AddToClassList("quest-objective-bar-fill");
                barFill.style.width = Length.Percent(objective.Fraction * 100f);
                barTrack.Add(barFill);
                row.Add(barTrack);

                _objectivesContainer.Add(row);
            }
        }

        private void UpdateTrackButton(QuestEntryViewModel entry)
        {
            if (_trackButton == null)
            {
                return;
            }

            bool canTrack = !entry.IsCompleted && _currentTab == Tab.Active;
            SetDisplay(_trackButton, canTrack);

            if (!canTrack)
            {
                return;
            }

            bool isTracked = QuestLogPresenter.ResolveTrackedQuestId(QuestTracking.PreferredQuestId, _visibleEntries, _data.CurrentHero) == entry.QuestId;
            _trackButton.text = _data.Localize(isTracked ? QuestLogTexts.TrackingKey : QuestLogTexts.TrackKey);
            _trackButton.EnableInClassList("quest-detail-track-button--tracking", isTracked);
            _trackButton.SetEnabled(!isTracked);
        }

        private static void SetDisplay(VisualElement element, bool visible)
        {
            if (element != null)
            {
                element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}
