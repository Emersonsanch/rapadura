using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Achievements;
using Rapadura.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Achievements
{
    /// <summary>
    /// Shows a small "Conquista desbloqueada!" toast (AchievementToastView.uxml/.uss) whenever
    /// <see cref="AchievementUnlockedEvent"/> fires. Unlocks arriving while a toast is on screen are
    /// queued and shown one after another. Timing is real time, but held (toast hidden) while a modal
    /// screen is open, see <see cref="MessageCards"/>; the whole view has picking-mode Ignore so it never
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
        private bool _fading;
        private float _remaining;

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
            _fading = false;
            MessageCards.Release(_toast, MessageCards.AchievementId);
        }

        private void OnAchievementUnlocked(AchievementUnlockedEvent evt)
        {
            if (evt.Achievement == null)
            {
                return;
            }

            _pending.Enqueue(evt.Achievement);

            if (!_isShowing && !MessageCards.ShouldHold(MessageCards.IsModalOpen))
            {
                ShowNext();
            }
        }

        /// <summary>
        /// Real-time clock of the toast. While a modal is open (Barcas de Luz, Mochila, Loja, diálogo...) the toast is
        /// hidden and its clock and queue are held: this document sorts at 60, above every window, and the toast used
        /// to slide in over the travel window's rows. In the top-centre lane it moves below the area banner.
        /// </summary>
        private void Update()
        {
            if (_toast == null)
            {
                return;
            }

            bool hold = MessageCards.ShouldHold(MessageCards.IsModalOpen);
            if (!_isShowing)
            {
                MessageCards.Release(_toast, MessageCards.AchievementId);
                if (!hold && _pending.Count > 0)
                {
                    ShowNext();
                }

                return;
            }

            if (hold)
            {
                _toast.RemoveFromClassList(VisibleClass);
                MessageCards.Release(_toast, MessageCards.AchievementId);
                return;
            }

            if (!_fading)
            {
                _toast.AddToClassList(VisibleClass);
            }

            MessageCards.Place(_toast, MessageCards.AchievementId, MessageCards.AchievementPriority, true);
            _remaining -= Time.unscaledDeltaTime;
            if (_remaining > 0f)
            {
                return;
            }

            if (!_fading)
            {
                // Slide/fade out, then the next one.
                _fading = true;
                _toast.RemoveFromClassList(VisibleClass);
                _remaining = Mathf.Max(0f, _transitionSeconds);
                return;
            }

            _fading = false;
            _isShowing = false;
            if (_pending.Count > 0)
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
            _fading = false;
            _remaining = Mathf.Max(0.5f, _displaySeconds);
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
        }
    }
}
