using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Quests;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Quests
{
    /// <summary>
    /// Compact on-screen tracker for the quest the player is following (QuestTrackerView.uxml/.uss).
    /// Lives in its own <see cref="UIDocument"/> instead of inside HudView so the HUD and the
    /// quest UI can evolve independently; the root is <c>picking-mode="Ignore"</c> so only the card
    /// itself intercepts touches — tapping it opens the <see cref="QuestLogController"/>.
    ///
    /// Which quest is shown: <see cref="QuestTracking.PreferredQuestId"/> (set by the log's
    /// "Acompanhar" button) while it's still active, else the first active quest
    /// (<see cref="QuestLogPresenter.ResolveTrackedQuestId"/>) — preferring the current hero's own
    /// personal quest when it is active; hidden when nothing is active.
    /// Event-driven like the log — refreshes on quest start/progress/complete/abandon,
    /// <see cref="GameLoadedEvent"/>, <see cref="QuestTrackedChangedEvent"/> and language changes,
    /// and briefly shows a "Missão concluída!" banner on <see cref="QuestCompletedEvent"/>.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class QuestTrackerController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Opened when the player taps the tracker card. Optional.")]
        [SerializeField] private QuestLogController _questLog;

        [Header("Layout")]
        [Tooltip("Maximum objective rows shown on the compact card.")]
        [SerializeField, Range(1, 6)] private int _maxObjectives = 3;

        [Header("Feedback")]
        [SerializeField] private float _completedToastSeconds = 2.5f;
        [SerializeField] private float _progressPulseSeconds = 0.35f;

        private UIDocument _uiDocument;
        private VisualElement _card;
        private Label _titleLabel;
        private VisualElement _objectivesContainer;
        private Label _toastLabel;

        private readonly QuestLogDataSource _data = new QuestLogDataSource();
        private LocalizationManager _subscribedLocalization;
        private IVisualElementScheduledItem _toastHideItem;
        private IVisualElementScheduledItem _pulseItem;
        private string _displayedQuestId;

        /// <summary>Quest currently shown on the card (null when hidden).</summary>
        public string DisplayedQuestId => _displayedQuestId;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            BindElements();
            RegisterCallbacks();
            Refresh();
        }

        private void Start()
        {
            SubscribeLanguage();
            Refresh();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        private void BindElements()
        {
            VisualElement rootVisualElement = _uiDocument.rootVisualElement;

            _card = rootVisualElement.Q<VisualElement>("quest-tracker-card");
            _titleLabel = rootVisualElement.Q<Label>("quest-tracker-title");
            _objectivesContainer = rootVisualElement.Q<VisualElement>("quest-tracker-objectives");
            _toastLabel = rootVisualElement.Q<Label>("quest-tracker-toast");

            if (_toastLabel != null)
            {
                _toastLabel.style.display = DisplayStyle.None;
            }
        }

        private void RegisterCallbacks()
        {
            _card?.RegisterCallback<ClickEvent>(OnCardClicked);

            EventBus.Subscribe<QuestStartedEvent>(OnQuestStarted);
            EventBus.Subscribe<QuestObjectiveUpdatedEvent>(OnQuestObjectiveUpdated);
            EventBus.Subscribe<QuestCompletedEvent>(OnQuestCompleted);
            EventBus.Subscribe<QuestAbandonedEvent>(OnQuestAbandoned);
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
            EventBus.Subscribe<QuestTrackedChangedEvent>(OnQuestTrackedChanged);
            EventBus.Subscribe<CharacterSelectedEvent>(OnCharacterSelected);
            EventBus.Subscribe<CharacterSelectionRestoredEvent>(OnCharacterRestored);

            SubscribeLanguage();
        }

        private void UnregisterCallbacks()
        {
            _card?.UnregisterCallback<ClickEvent>(OnCardClicked);

            EventBus.Unsubscribe<QuestStartedEvent>(OnQuestStarted);
            EventBus.Unsubscribe<QuestObjectiveUpdatedEvent>(OnQuestObjectiveUpdated);
            EventBus.Unsubscribe<QuestCompletedEvent>(OnQuestCompleted);
            EventBus.Unsubscribe<QuestAbandonedEvent>(OnQuestAbandoned);
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
            EventBus.Unsubscribe<QuestTrackedChangedEvent>(OnQuestTrackedChanged);
            EventBus.Unsubscribe<CharacterSelectedEvent>(OnCharacterSelected);
            EventBus.Unsubscribe<CharacterSelectionRestoredEvent>(OnCharacterRestored);

            if (_subscribedLocalization != null)
            {
                _subscribedLocalization.LanguageChanged -= OnLanguageChanged;
                _subscribedLocalization = null;
            }

            _toastHideItem?.Pause();
            _pulseItem?.Pause();
        }

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

        // ------------------------------------------------------------------
        // Event handlers
        // ------------------------------------------------------------------

        private void OnQuestStarted(QuestStartedEvent evt) => Refresh();
        private void OnQuestAbandoned(QuestAbandonedEvent evt) => Refresh();
        private void OnGameLoaded(GameLoadedEvent evt) => Refresh();
        private void OnQuestTrackedChanged(QuestTrackedChangedEvent evt) => Refresh();
        private void OnCharacterSelected(CharacterSelectedEvent evt) => Refresh();
        private void OnCharacterRestored(CharacterSelectionRestoredEvent evt) => Refresh();
        private void OnLanguageChanged(LanguageCode language) => Refresh();

        private void OnQuestObjectiveUpdated(QuestObjectiveUpdatedEvent evt)
        {
            Refresh();

            if (evt.QuestId == _displayedQuestId)
            {
                Pulse();
            }
        }

        private void OnQuestCompleted(QuestCompletedEvent evt)
        {
            Refresh();
            ShowCompletedToast(evt.QuestId);
        }

        private void OnCardClicked(ClickEvent evt)
        {
            _questLog?.Open();
        }

        // ------------------------------------------------------------------
        // Rendering
        // ------------------------------------------------------------------

        public void Refresh()
        {
            if (_card == null)
            {
                return;
            }

            List<QuestEntryViewModel> active = _data.BuildActive();
            string trackedId = QuestLogPresenter.ResolveTrackedQuestId(QuestTracking.PreferredQuestId, active, _data.CurrentHero);
            QuestEntryViewModel entry = trackedId != null ? active.Find(e => e.QuestId == trackedId) : null;

            _displayedQuestId = entry?.QuestId;

            if (entry == null)
            {
                _card.style.display = DisplayStyle.None;
                return;
            }

            _card.style.display = DisplayStyle.Flex;

            if (_titleLabel != null)
            {
                _titleLabel.text = entry.Title;
            }

            BuildObjectives(entry);
        }

        private void BuildObjectives(QuestEntryViewModel entry)
        {
            if (_objectivesContainer == null)
            {
                return;
            }

            _objectivesContainer.Clear();

            // Pending objectives first so the "what do I do next" line is always visible on a small card.
            var ordered = new List<QuestObjectiveViewModel>(entry.Objectives.FindAll(o => !o.IsComplete));
            ordered.AddRange(entry.Objectives.FindAll(o => o.IsComplete));

            int shown = 0;
            foreach (QuestObjectiveViewModel objective in ordered)
            {
                if (shown >= _maxObjectives)
                {
                    break;
                }

                var row = new VisualElement { pickingMode = PickingMode.Ignore };
                row.AddToClassList("quest-tracker-objective");
                row.EnableInClassList("quest-tracker-objective--complete", objective.IsComplete);

                var text = new Label(objective.Text) { pickingMode = PickingMode.Ignore };
                text.AddToClassList("quest-tracker-objective-text");
                row.Add(text);

                var progress = new Label(objective.ProgressText) { pickingMode = PickingMode.Ignore };
                progress.AddToClassList("quest-tracker-objective-progress");
                row.Add(progress);

                _objectivesContainer.Add(row);
                shown++;
            }
        }

        private void Pulse()
        {
            if (_card == null)
            {
                return;
            }

            _card.AddToClassList("quest-tracker-card--pulse");
            _pulseItem?.Pause();
            _pulseItem = _card.schedule
                .Execute(() => _card.RemoveFromClassList("quest-tracker-card--pulse"))
                .StartingIn((long)(_progressPulseSeconds * 1000f));
        }

        private void ShowCompletedToast(string questId)
        {
            if (_toastLabel == null)
            {
                return;
            }

            QuestDefinition definition = _data.FindDefinition(questId);
            string header = _data.Localize(QuestLogTexts.QuestCompletedToastKey);
            _toastLabel.text = definition != null ? $"{header}\n{_data.Localize(definition.TitleKey)}" : header;
            _toastLabel.style.display = DisplayStyle.Flex;

            _toastHideItem?.Pause();
            _toastHideItem = _toastLabel.schedule
                .Execute(() => _toastLabel.style.display = DisplayStyle.None)
                .StartingIn((long)(_completedToastSeconds * 1000f));
        }
    }
}
