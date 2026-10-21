using System;
using Rapadura.Core.Systems;
using Rapadura.Core.Versioning;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// Drives the Créditos screen, opened from <see cref="MainMenuController"/>. Same UI Toolkit
    /// pattern as SettingsMenuController: one <see cref="UIDocument"/> (CreditsView.uxml, which
    /// pulls in CreditsView.uss via &lt;Style&gt;), elements resolved once, hidden on enable until
    /// <see cref="Show"/> is called. The text itself comes from <see cref="CreditsContent"/>.
    ///
    /// Closes on the "Voltar" button or the UI cancel action (Esc / gamepad B / Android back),
    /// then raises <see cref="Closed"/> so the main menu can restore focus.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class CreditsController : MonoBehaviour
    {
        private UIDocument _uiDocument;
        private VisualElement _root;
        private ScrollView _scroll;
        private VisualElement _content;
        private Label _versionLabel;
        private Button _backButton;

        public bool IsOpen { get; private set; }

        /// <summary>Raised after the screen hides itself (button or cancel), not on OnDisable.</summary>
        public event Action Closed;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            BindElements();
            RegisterCallbacks();
            BuildContent();
            SetVisible(false);
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
            GameplayPause.Release(this);
        }

        private void BindElements()
        {
            VisualElement rootVisualElement = _uiDocument.rootVisualElement;

            _root = rootVisualElement.Q<VisualElement>("credits-root");
            _scroll = rootVisualElement.Q<ScrollView>("credits-scroll");
            _content = rootVisualElement.Q<VisualElement>("credits-content");
            _versionLabel = rootVisualElement.Q<Label>("credits-version");
            _backButton = rootVisualElement.Q<Button>("credits-back-button");
        }

        private void RegisterCallbacks()
        {
            if (_backButton != null) _backButton.clicked += Hide;
            _root?.RegisterCallback<NavigationCancelEvent>(OnNavigationCancel);
        }

        private void UnregisterCallbacks()
        {
            if (_backButton != null) _backButton.clicked -= Hide;
            _root?.UnregisterCallback<NavigationCancelEvent>(OnNavigationCancel);
        }

        private void BuildContent()
        {
            if (_versionLabel != null)
            {
                _versionLabel.text = $"Rapadura {BuildVersion.DisplayLabel}";
            }

            if (_content == null)
            {
                return;
            }

            _content.Clear();

            foreach (CreditsSection section in CreditsContent.Sections)
            {
                var sectionElement = new VisualElement();
                sectionElement.AddToClassList("credits-section");

                var title = new Label(section.Title);
                title.AddToClassList("credits-section-title");
                sectionElement.Add(title);

                foreach (string line in section.Lines)
                {
                    var lineLabel = new Label(line);
                    lineLabel.AddToClassList("credits-line");
                    sectionElement.Add(lineLabel);
                }

                _content.Add(sectionElement);
            }
        }

        private void OnNavigationCancel(NavigationCancelEvent evt)
        {
            if (!IsOpen)
            {
                return;
            }

            evt.StopPropagation();
            Hide();
        }

        public void Show()
        {
            SetVisible(true);
            if (_scroll != null) _scroll.scrollOffset = Vector2.zero;
            _backButton?.Focus();
        }

        public void Hide()
        {
            bool wasOpen = IsOpen;
            SetVisible(false);

            if (wasOpen)
            {
                Closed?.Invoke();
            }
        }

        private void SetVisible(bool visible)
        {
            IsOpen = visible;

            if (_root != null)
            {
                _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (visible)
            {
                GameplayPause.Acquire(this);
            }
            else
            {
                GameplayPause.Release(this);
            }
        }
    }
}
