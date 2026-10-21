using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Common
{
    /// <summary>
    /// Applies <see cref="AccessibilitySettings"/> (font scale, high contrast, colorblind mode) to
    /// EVERY active <see cref="UIDocument"/> in the game — HUD, menus, tooltips, screens enabled
    /// later — via <see cref="UiAccessibilityStyler"/>. Re-applies immediately when
    /// <see cref="AccessibilitySettings.SettingsChanged"/> fires and otherwise rescans on a short
    /// unscaled interval, which is what picks up documents enabled later and list items rebuilt
    /// by controllers (the styler only touches elements whose size isn't already right, so a
    /// rescan of an unchanged tree is a cheap walk).
    ///
    /// Self-bootstrapping: a single DontDestroyOnLoad instance is created after the first scene
    /// loads (no scene wiring needed). A scene may also contain one explicitly (e.g. to assign a
    /// different theme stylesheet); the bootstrap then reuses it. The theme defaults to
    /// <c>Resources/UI/AccessibilityTheme.uss</c>, which holds the colorblind / high-contrast rules.
    ///
    /// Waits for <see cref="AccessibilitySettings"/> to appear in the <see cref="ServiceLocator"/>
    /// (GameManager registers it) and re-resolves it every rescan, so registration order doesn't matter.
    /// </summary>
    public class UiAccessibilityApplier : MonoBehaviour
    {
        private const string ThemeResourcePath = "UI/AccessibilityTheme";

        [SerializeField] private StyleSheet _themeStyleSheet;
        [SerializeField, Min(0.05f)] private float _rescanIntervalSeconds = 0.5f;

        private static UiAccessibilityApplier _instance;

        private readonly UiAccessibilityStyler _styler = new UiAccessibilityStyler();
        private AccessibilitySettings _settings;
        private float _nextRescanTime;
        private bool _dirty = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null || FindFirstObjectByType<UiAccessibilityApplier>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(UiAccessibilityApplier));
            host.AddComponent<UiAccessibilityApplier>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }

            if (_themeStyleSheet == null)
            {
                _themeStyleSheet = Resources.Load<StyleSheet>(ThemeResourcePath);
            }
        }

        private void OnDestroy()
        {
            Subscribe(null);
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>Forces a re-apply on the next frame (e.g. right after a screen rebuilt its contents).</summary>
        public static void RequestRefresh()
        {
            if (_instance != null)
            {
                _instance._dirty = true;
            }
        }

        private void Update()
        {
            ServiceLocator.TryGet(out AccessibilitySettings settings);
            if (settings != _settings)
            {
                Subscribe(settings);
                _dirty = true;
            }

            // At most one apply per frame: the styler compares against resolved styles, which only
            // refresh between frames.
            if (!_dirty && Time.unscaledTime < _nextRescanTime)
            {
                return;
            }

            _dirty = false;
            _nextRescanTime = Time.unscaledTime + _rescanIntervalSeconds;
            ApplyToAllDocuments();
        }

        private void Subscribe(AccessibilitySettings settings)
        {
            if (_settings != null)
            {
                _settings.SettingsChanged -= OnSettingsChanged;
            }

            _settings = settings;

            if (_settings != null)
            {
                _settings.SettingsChanged += OnSettingsChanged;
            }
        }

        private void OnSettingsChanged()
        {
            _dirty = true;
        }

        private void ApplyToAllDocuments()
        {
            UiAccessibilityState state = UiAccessibilityState.From(_settings);
            UIDocument[] documents = FindObjectsByType<UIDocument>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            foreach (UIDocument document in documents)
            {
                if (document == null || !document.isActiveAndEnabled)
                {
                    continue;
                }

                _styler.Apply(document.rootVisualElement, state, _themeStyleSheet);
            }
        }
    }
}
