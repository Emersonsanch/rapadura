using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Dialogue;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Dialogue
{
    /// <summary>
    /// Drives the dialogue box (DialogueView.uxml/.uss), following the same single-<see cref="UIDocument"/>,
    /// <c>Q&lt;T&gt;()</c>-resolved-once pattern as <c>HudController</c>/<c>PauseMenuController</c>.
    /// Purely reactive: it never advances the dialogue itself, it only reflects
    /// <see cref="DialogueManager"/>'s current node/choices and forwards clicks back into it via
    /// <see cref="DialogueManager.SelectChoice"/>/<see cref="DialogueManager.AdvanceCurrentLine"/>.
    ///
    /// Speaker name and line text are resolved through <see cref="LocalizationManager.Get"/> —
    /// nothing here is hardcoded text, matching the rest of the project's localization approach.
    ///
    /// Choice buttons are variable in count (0 for a terminal/auto-advance line, N otherwise), so
    /// they're instantiated into <c>dialogue-choices-container</c> at runtime instead of being
    /// baked into the UXML, the same way a future inventory grid would generate slot elements.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class DialogueUIController : MonoBehaviour
    {
        private UIDocument _uiDocument;
        private VisualElement _root;
        private Label _speakerNameLabel;
        private Label _lineTextLabel;
        private VisualElement _choicesContainer;
        private Button _continueButton;

        private DialogueManager _dialogueManager;
        private readonly List<Button> _spawnedChoiceButtons = new List<Button>();

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            BindElements();
            ResolveDialogueManager();
            RegisterCallbacks();
            Hide();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        private void BindElements()
        {
            VisualElement rootVisualElement = _uiDocument.rootVisualElement;

            _root = rootVisualElement.Q<VisualElement>("dialogue-root");
            _speakerNameLabel = rootVisualElement.Q<Label>("dialogue-speaker-name");
            _lineTextLabel = rootVisualElement.Q<Label>("dialogue-line-text");
            _choicesContainer = rootVisualElement.Q<VisualElement>("dialogue-choices-container");
            _continueButton = rootVisualElement.Q<Button>("dialogue-continue-button");
        }

        /// <summary>
        /// Lets a bootstrap script assign the manager directly (useful in tests or before it is
        /// registered with the ServiceLocator). If never called, OnEnable falls back to resolving
        /// it from the ServiceLocator, since <see cref="DialogueManager"/> is a plain C# class
        /// (not a Unity Object) and therefore cannot be assigned via the Inspector.
        /// </summary>
        public void SetDialogueManager(DialogueManager manager)
        {
            _dialogueManager = manager;
        }

        private void ResolveDialogueManager()
        {
            if (_dialogueManager != null)
            {
                return;
            }

            ServiceLocator.TryGet(out _dialogueManager);
        }

        private void RegisterCallbacks()
        {
            EventBus.Subscribe<DialogueStartedEvent>(OnDialogueStarted);
            EventBus.Subscribe<DialogueLineChangedEvent>(OnDialogueLineChanged);
            EventBus.Subscribe<DialogueEndedEvent>(OnDialogueEnded);

            if (_continueButton != null)
            {
                _continueButton.clicked += OnContinueClicked;
            }
        }

        private void UnregisterCallbacks()
        {
            EventBus.Unsubscribe<DialogueStartedEvent>(OnDialogueStarted);
            EventBus.Unsubscribe<DialogueLineChangedEvent>(OnDialogueLineChanged);
            EventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);

            if (_continueButton != null)
            {
                _continueButton.clicked -= OnContinueClicked;
            }

            ClearChoiceButtons();
        }

        private void OnDialogueStarted(DialogueStartedEvent evt)
        {
            Show();
            RefreshCurrentLine();
        }

        private void OnDialogueLineChanged(DialogueLineChangedEvent evt)
        {
            RefreshCurrentLine();
        }

        private void OnDialogueEnded(DialogueEndedEvent evt)
        {
            Hide();
        }

        private void RefreshCurrentLine()
        {
            if (_dialogueManager == null || !_dialogueManager.IsActive)
            {
                Hide();
                return;
            }

            DialogueNode node = _dialogueManager.CurrentNode;

            if (_speakerNameLabel != null)
            {
                string speakerKey = string.IsNullOrEmpty(node.SpeakerNameKey) ? null : node.SpeakerNameKey;
                _speakerNameLabel.text = speakerKey != null ? LocalizeSafe(speakerKey) : node.SpeakerId;
            }

            if (_lineTextLabel != null)
            {
                _lineTextLabel.text = LocalizeSafe(node.TextKey);
            }

            RebuildChoiceButtons();

            bool hasChoices = _dialogueManager.AvailableChoices.Count > 0;
            bool canAutoAdvance = !hasChoices && !string.IsNullOrEmpty(node.AutoAdvanceToNodeId);

            if (_continueButton != null)
            {
                _continueButton.style.display = canAutoAdvance ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void RebuildChoiceButtons()
        {
            ClearChoiceButtons();

            if (_choicesContainer == null || _dialogueManager == null)
            {
                return;
            }

            IReadOnlyList<DialogueChoice> choices = _dialogueManager.AvailableChoices;

            for (int i = 0; i < choices.Count; i++)
            {
                int choiceIndex = i;
                var button = new Button(() => OnChoiceClicked(choiceIndex))
                {
                    text = LocalizeSafe(choices[i].TextKey),
                    focusable = true
                };
                button.AddToClassList("dialogue-choice-button");
                _choicesContainer.Add(button);
                _spawnedChoiceButtons.Add(button);
            }

            if (_spawnedChoiceButtons.Count > 0)
            {
                _spawnedChoiceButtons[0].Focus();
            }
        }

        private void ClearChoiceButtons()
        {
            if (_choicesContainer != null)
            {
                foreach (Button button in _spawnedChoiceButtons)
                {
                    _choicesContainer.Remove(button);
                }
            }

            _spawnedChoiceButtons.Clear();
        }

        private void OnChoiceClicked(int index)
        {
            _dialogueManager?.SelectChoice(index);
        }

        private void OnContinueClicked()
        {
            _dialogueManager?.AdvanceCurrentLine();
        }

        private static string LocalizeSafe(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            return ServiceLocator.TryGet(out LocalizationManager localization) ? localization.Get(key) : $"[{key}]";
        }

        private void Show()
        {
            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
            }
        }

        private void Hide()
        {
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }

            ClearChoiceButtons();
        }
    }
}
