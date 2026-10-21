using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Achievements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Achievements
{
    /// <summary>
    /// Shows a small "Conquista desbloqueada!" toast (AchievementToastView.uxml/.uss) whenever
    /// <see cref="AchievementUnlockedEvent"/> fires. Unlocks arriving while a toast is on screen are
    /// queued and shown one after another. Timing uses the panel scheduler (real time), so toasts
    /// still advance while the game is paused; the whole view has picking-mode Ignore so it never
    /// blocks touches/clicks on the HUD underneath.
    ///
    /// Needs only a <see cref="UIDocument"/> with AchievementToastView.uxml as its source asset; it
    /// listens to the EventBus by itself, so no other wiring is required.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class AchievementToastController : MonoBehaviour
    {
        private const string VisibleClass = "achievement-toast--visible";

        [Tooltip("Seconds each toast stays fully visible.")]
        [SerializeField] private float _displaySeconds = 3.5f;
        [Tooltip("Seconds reserved for the USS slide/fade transition between toasts.")]
        [SerializeField] private float _transitionSeconds = 0.35f;

        private readonly Queue<AchievementDefinition> _pending = new Queue<AchievementDefinition>();
        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _toast;
        private VisualElement _icon;
        private Label _headerLabel;
        private Label _titleLabel;
        private Label _descriptionLabel;
        private LocalizationManager _localization;
        private bool _isShowing;

        public bool IsShowing => _isShowing;
        public int PendingCount => _pending.Count;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            VisualElement rootVisualElement = _uiDocument.rootVisualElement;
            _root = rootVisualElement.Q<VisualElement>("achievement-toast-root");
            _toast = rootVisualElement.Q<VisualElement>("achievement-toast");
            _icon = rootVisualElement.Q<VisualElement>("achievement-toast-icon");
            _headerLabel = rootVisualElement.Q<Label>("achievement-toast-header");
            _titleLabel = rootVisualElement.Q<Label>("achievement-toast-title");
            _descriptionLabel = rootVisualElement.Q<Label>("achievement-toast-description");

            // Never intercept input, regardless of what the UXML says.
            rootVisualElement.pickingMode = PickingMode.Ignore;
            if (_root != null) _root.pickingMode = PickingMode.Ignore;

            _toast?.RemoveFromClassList(VisibleClass);
            _isShowing = false;

            EventBus.Subscribe<AchievementUnlockedEvent>(OnAchievementUnlocked);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<AchievementUnlockedEvent>(OnAchievementUnlocked);
            _pending.Clear();
            _isShowing = false;
        }

        private void OnAchievementUnlocked(AchievementUnlockedEvent evt)
        {
            if (evt.Achievement == null)
            {
                return;
            }

            _pending.Enqueue(evt.Achievement);

            if (!_isShowing)
            {
                ShowNext();
            }
        }

        private void ShowNext()
        {
            if (_pending.Count == 0 || _toast == null)
            {
                _isShowing = false;
                return;
            }

            _isShowing = true;
            AchievementDefinition achievement = _pending.Dequeue();

            if (_localization == null && ServiceLocator.TryGet(out _localization))
            {
                AchievementTexts.EnsureLoaded(_localization);
            }

            if (_headerLabel != null) _headerLabel.text = AchievementTexts.Localize(_localization, AchievementTexts.ToastHeaderKey);
            if (_titleLabel != null) _titleLabel.text = AchievementTexts.Localize(_localization, achievement.TitleKey);
            if (_descriptionLabel != null) _descriptionLabel.text = AchievementTexts.Localize(_localization, achievement.DescriptionKey);

            if (_icon != null)
            {
                bool hasIcon = achievement.Icon != null;
                _icon.style.backgroundImage = hasIcon ? new StyleBackground(achievement.Icon) : new StyleBackground(StyleKeyword.None);
                _icon.EnableInClassList("achievement-toast-icon--placeholder", !hasIcon);
            }

            _toast.AddToClassList(VisibleClass);

            long visibleMs = (long)(Mathf.Max(0.5f, _displaySeconds) * 1000f);
            long transitionMs = (long)(Mathf.Max(0f, _transitionSeconds) * 1000f);

            _toast.schedule.Execute(() =>
            {
                _toast.RemoveFromClassList(VisibleClass);
                _toast.schedule.Execute(ShowNext).ExecuteLater(transitionMs);
            }).ExecuteLater(visibleMs);
        }
    }
}
