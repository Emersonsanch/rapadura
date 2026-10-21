using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Achievements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Achievements
{
    /// <summary>
    /// Drives the Achievements ("Conquistas") screen (AchievementListView.uxml/.uss): a header with an
    /// "x/y desbloqueadas" summary and a scrollable list of every achievement with icon, title,
    /// description and a progress bar ("37/100"). Hidden achievements show as "???" until unlocked.
    ///
    /// Same single-<see cref="UIDocument"/> pattern and <see cref="Open"/>/<see cref="Toggle"/> contract
    /// as <c>QuestLogController</c>, so a HUD/pause-menu button only needs to call one of those.
    /// Purely reactive: listens to <see cref="AchievementUnlockedEvent"/>, <see cref="GameLoadedEvent"/>
    /// and <see cref="LocalizationManager.LanguageChanged"/>; while hidden it does nothing and always
    /// rebuilds (recomputing live progress) on the next <see cref="Show"/>.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class AchievementListController : MonoBehaviour
    {
        private UIDocument _uiDocument;
        private VisualElement _root;
        private Label _titleLabel;
        private Label _summaryLabel;
        private Button _closeButton;
        private VisualElement _listContainer;
        private Label _emptyLabel;

        private AchievementManager _manager;
        private LocalizationManager _localization;
        private bool _isOpen;

        public bool IsOpen => _isOpen;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            VisualElement rootVisualElement = _uiDocument.rootVisualElement;
            _root = rootVisualElement.Q<VisualElement>("achievement-list-root");
            _titleLabel = rootVisualElement.Q<Label>("achievement-list-title");
            _summaryLabel = rootVisualElement.Q<Label>("achievement-list-summary");
            _closeButton = rootVisualElement.Q<Button>("achievement-list-close-button");
            _listContainer = rootVisualElement.Q<VisualElement>("achievement-list");
            _emptyLabel = rootVisualElement.Q<Label>("achievement-list-empty-label");

            if (_closeButton != null) _closeButton.clicked += Hide;

            EventBus.Subscribe<AchievementUnlockedEvent>(OnAchievementUnlocked);
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
            ResolveServices();
            Hide();
        }

        private void Start()
        {
            ResolveServices();
        }

        private void OnDisable()
        {
            if (_closeButton != null) _closeButton.clicked -= Hide;

            EventBus.Unsubscribe<AchievementUnlockedEvent>(OnAchievementUnlocked);
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);

            if (_localization != null)
            {
                _localization.LanguageChanged -= OnLanguageChanged;
                _localization = null;
            }

            GameplayPause.Release(this);
        }

        /// <summary>Managers may register after this UI's OnEnable, so this is retried from Start/Show.</summary>
        private void ResolveServices()
        {
            if (_manager == null)
            {
                ServiceLocator.TryGet(out _manager);
            }

            if (_localization == null && ServiceLocator.TryGet(out _localization))
            {
                AchievementTexts.EnsureLoaded(_localization);
                _localization.LanguageChanged += OnLanguageChanged;
            }
        }

        // ------------------------------------------------------------------
        // Open / close
        // ------------------------------------------------------------------

        /// <summary>Entry point for a HUD/pause-menu "Conquistas" button.</summary>
        public void Open() => Show();

        public void Show()
        {
            _isOpen = true;

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

        private void OnAchievementUnlocked(AchievementUnlockedEvent evt) => MarkDirty();
        private void OnGameLoaded(GameLoadedEvent evt) => MarkDirty();
        private void OnLanguageChanged(LanguageCode language) => MarkDirty();

        /// <summary>Hidden screens rebuild on the next Show anyway, so only refresh while open.</summary>
        private void MarkDirty()
        {
            if (_isOpen)
            {
                Refresh();
            }
        }

        // ------------------------------------------------------------------
        // Rendering
        // ------------------------------------------------------------------

        private string Localize(string key) => AchievementTexts.Localize(_localization, key);

        private void Refresh()
        {
            // Progress (kills, gold...) changes without an unlock event, so always rebuild when shown.
            ResolveServices();

            if (_titleLabel != null) _titleLabel.text = Localize(AchievementTexts.TitleKey);
            if (_closeButton != null) _closeButton.text = Localize(AchievementTexts.CloseKey);
            if (_summaryLabel != null) _summaryLabel.text = AchievementListPresenter.FormatSummary(_manager, Localize);

            if (_listContainer == null)
            {
                return;
            }

            _listContainer.Clear();

            var entries = AchievementListPresenter.BuildAll(_manager, Localize);

            foreach (AchievementEntryViewModel entry in entries)
            {
                _listContainer.Add(CreateRow(entry));
            }

            if (_emptyLabel != null)
            {
                _emptyLabel.text = Localize(AchievementTexts.EmptyKey);
                _emptyLabel.style.display = entries.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private static VisualElement CreateRow(AchievementEntryViewModel entry)
        {
            var row = new VisualElement { name = $"achievement-item-{entry.AchievementId}" };
            row.AddToClassList("achievement-item");
            row.EnableInClassList("achievement-item--unlocked", entry.IsUnlocked);
            row.EnableInClassList("achievement-item--masked", entry.IsMasked);

            var icon = new VisualElement();
            icon.AddToClassList("achievement-item-icon");
            if (entry.Icon != null)
            {
                icon.style.backgroundImage = new StyleBackground(entry.Icon);
            }
            else
            {
                icon.AddToClassList("achievement-item-icon--placeholder");
            }

            row.Add(icon);

            var body = new VisualElement();
            body.AddToClassList("achievement-item-body");

            var header = new VisualElement();
            header.AddToClassList("achievement-item-header");

            var title = new Label(entry.Title);
            title.AddToClassList("achievement-item-title");
            header.Add(title);

            var progress = new Label(entry.ProgressText);
            progress.AddToClassList("achievement-item-progress");
            header.Add(progress);

            body.Add(header);

            var description = new Label(entry.Description);
            description.AddToClassList("achievement-item-description");
            body.Add(description);

            var barTrack = new VisualElement();
            barTrack.AddToClassList("achievement-item-bar-track");
            var barFill = new VisualElement();
            barFill.AddToClassList("achievement-item-bar-fill");
            barFill.style.width = Length.Percent(entry.Fraction * 100f);
            barTrack.Add(barFill);
            body.Add(barTrack);

            row.Add(body);
            return row;
        }
    }
}
