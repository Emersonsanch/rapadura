using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using UnityEngine.UIElements;

namespace Rapadura.UI.Common
{
    /// <summary>
    /// Localizes the static text of a UXML tree by convention, so a screen doesn't need a
    /// <c>Q&lt;T&gt;()</c> + assignment per label:
    /// <code>&lt;ui:Button text="Salvar" class="pause-menu-button loc" view-data-key="ui.pause.save" /&gt;</code>
    /// Every element with the <see cref="LocClass"/> USS class gets the text of the key stored in its
    /// <c>view-data-key</c> (UI Toolkit's own per-element string id; view-data persistence is
    /// editor-window only, so it's inert at runtime). <see cref="TextElement"/>s (Label, Button)
    /// get <c>text</c>, fields (Toggle, Slider, DropdownField...) get <c>label</c>.
    ///
    /// The UXML text stays in readable pt-BR: it's what the screen shows when there's no
    /// <see cref="LocalizationManager"/> (test scenes) or the key is unknown — nothing is overwritten
    /// with "[key]". One instance per controller: <see cref="Attach"/> once, <see cref="Refresh"/> on
    /// Show (the manager may be registered only after OnEnable), <see cref="Detach"/> on disable.
    /// While attached it re-applies on <see cref="LocalizationManager.LanguageChanged"/> and then
    /// invokes the controller's callback so it can rebuild the text it builds in code.
    /// </summary>
    public sealed class UiDocumentLocalizer
    {
        /// <summary>USS class marking an element whose text comes from its <c>view-data-key</c>.</summary>
        public const string LocClass = "loc";

        private VisualElement _root;
        private Action _onLanguageChanged;
        private Func<LocalizationManager, string, string> _resolver;
        private LocalizationManager _manager;
        private LocalizationManager _subscribed;

        /// <summary>The manager in use (resolved lazily from the <see cref="ServiceLocator"/>), or null.</summary>
        public LocalizationManager Manager => _manager;

        /// <summary>
        /// Starts localizing <paramref name="root"/>. <paramref name="onLanguageChanged"/> runs after each
        /// re-apply caused by a language switch. <paramref name="resolver"/> defaults to
        /// <see cref="UiTexts.Get"/>; pass another helper's Get to use its keys/fallbacks too.
        /// </summary>
        public void Attach(VisualElement root, Action onLanguageChanged = null, Func<LocalizationManager, string, string> resolver = null)
        {
            _root = root;
            _onLanguageChanged = onLanguageChanged;
            _resolver = resolver;
            Refresh();
        }

        /// <summary>Optional explicit injection (tests/bootstrap); otherwise resolved from the ServiceLocator.</summary>
        public void SetManager(LocalizationManager manager)
        {
            _manager = manager;
            Refresh();
        }

        /// <summary>Resolves the manager if still missing, (re)subscribes and re-applies the tree.</summary>
        public void Refresh()
        {
            if (_manager == null)
            {
                ServiceLocator.TryGet(out _manager);
            }

            Subscribe();
            Apply(_root, _manager, _resolver);
        }

        public void Detach()
        {
            Unsubscribe();
            _root = null;
            _onLanguageChanged = null;
        }

        /// <summary>
        /// Applies localized text to every <see cref="LocClass"/> element under <paramref name="root"/>.
        /// No-op without a manager (the UXML pt-BR text stays). Returns how many elements were updated.
        /// </summary>
        public static int Apply(VisualElement root, LocalizationManager manager, Func<LocalizationManager, string, string> resolver = null)
        {
            if (root == null || manager == null)
            {
                return 0;
            }

            UiTexts.EnsureRegistered(manager);
            int updated = 0;
            root.Query<VisualElement>(className: LocClass).ForEach(element =>
            {
                string key = element.viewDataKey;
                if (string.IsNullOrEmpty(key))
                {
                    return;
                }

                string text = resolver != null ? resolver(manager, key) : UiTexts.Get(manager, key);
                if (string.IsNullOrEmpty(text) || text == key)
                {
                    return;
                }

                if (SetElementText(element, text))
                {
                    updated++;
                }
            });

            return updated;
        }

        private static bool SetElementText(VisualElement element, string text)
        {
            switch (element)
            {
                case TextElement textElement:
                    textElement.text = text;
                    return true;
                case Toggle toggle:
                    toggle.label = text;
                    return true;
                case Slider slider:
                    slider.label = text;
                    return true;
                case SliderInt sliderInt:
                    sliderInt.label = text;
                    return true;
                case DropdownField dropdown:
                    dropdown.label = text;
                    return true;
                case TextField textField:
                    textField.label = text;
                    return true;
                case Foldout foldout:
                    foldout.text = text;
                    return true;
                default:
                    return false;
            }
        }

        private void Subscribe()
        {
            if (_subscribed == _manager)
            {
                return;
            }

            Unsubscribe();
            _subscribed = _manager;
            if (_subscribed != null)
            {
                _subscribed.LanguageChanged += OnLanguageChanged;
            }
        }

        private void Unsubscribe()
        {
            if (_subscribed != null)
            {
                _subscribed.LanguageChanged -= OnLanguageChanged;
                _subscribed = null;
            }
        }

        private void OnLanguageChanged(LanguageCode language)
        {
            Apply(_root, _manager, _resolver);
            _onLanguageChanged?.Invoke();
        }
    }
}
