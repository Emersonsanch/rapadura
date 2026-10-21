using System;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.Menus;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Rapadura.Core.Performance
{
    /// <summary>Abstraction over the Unity frame-pacing globals so <see cref="FrameRateManager"/> is testable.</summary>
    public interface IFrameRateApplier
    {
        void Apply(FrameRateSettings settings);
    }

    /// <summary>
    /// Default <see cref="IFrameRateApplier"/>. Only touches the globals in Play Mode / player builds:
    /// writing <c>QualitySettings.vSyncCount</c> from an EditMode test (GameManagerTests builds every
    /// manager) would dirty ProjectSettings/QualitySettings.asset.
    /// </summary>
    public class UnityFrameRateApplier : IFrameRateApplier
    {
        public void Apply(FrameRateSettings settings)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            QualitySettings.vSyncCount = settings.VSyncCount;
            Application.targetFrameRate = settings.TargetFrameRate;
            OnDemandRendering.renderFrameInterval = settings.RenderFrameInterval;
        }
    }

    /// <summary>
    /// Battery/thermal manager: picks frame pacing per <see cref="FrameRateContext"/> via
    /// <see cref="FrameRatePolicy"/> and pushes it to Unity whenever the context changes.
    /// Plain C# <see cref="IManager"/>, built and registered by <c>GameManager.BuildManagers()</c>
    /// like AudioManager/AccessibilitySettings.
    ///
    /// Context sources:
    ///   - Active scene: <see cref="SceneManager.sceneLoaded"/> (Main = combat, WorldMap = world map,
    ///     same names as <see cref="SceneTransitionService"/>).
    ///   - Pause: <see cref="GamePausedEvent"/>/<see cref="GameResumedEvent"/> from PauseMenuController.
    ///   - Main menu: <see cref="SetMenuOpen"/>, called by MainMenuController on show/hide.
    ///
    /// SETTINGS DATA: "60 FPS em combate" (<see cref="HighFrameRateInCombat"/>) persists via
    /// PlayerPrefs under "Performance." (same mechanism as AudioManager/AccessibilitySettings).
    /// No settings UI binds it yet — SettingsMenuController's Gráficos tab can call
    /// <see cref="SetHighFrameRateInCombat"/> and listen to <see cref="SettingsChanged"/>.
    /// </summary>
    public class FrameRateManager : IManager
    {
        private const string LogCategory = "FrameRate";
        public const string HighFrameRateInCombatKey = "Performance.HighFrameRateInCombat";
        private const bool DefaultHighFrameRateInCombat = true;

        private readonly IFrameRateApplier _applier;
        private readonly bool _isMobile;

        private string _activeSceneName;
        private bool _isPaused;
        private bool _isMenuOpen;
        private bool _hasApplied;

        public bool IsInitialized { get; private set; }
        public bool IsMobile => _isMobile;

        public string CombatSceneName { get; set; } = SceneTransitionService.DefaultCombatSceneName;
        public string WorldMapSceneName { get; set; } = SceneTransitionService.DefaultWorldMapSceneName;

        /// <summary>Player option: allow 60 fps during combat on mobile (off = 30 fps everywhere, battery saver).</summary>
        public bool HighFrameRateInCombat { get; private set; } = DefaultHighFrameRateInCombat;

        public FrameRateContext CurrentContext { get; private set; } = FrameRateContext.Menu;
        public FrameRateSettings CurrentSettings { get; private set; }

        /// <summary>Raised when <see cref="HighFrameRateInCombat"/> changes (for a future settings toggle).</summary>
        public event Action SettingsChanged;

        /// <param name="isMobile">Override platform detection (tests); defaults to <c>Application.isMobilePlatform</c>.</param>
        /// <param name="applier">Override where settings go (tests); defaults to <see cref="UnityFrameRateApplier"/>.</param>
        public FrameRateManager(bool? isMobile = null, IFrameRateApplier applier = null)
        {
            _isMobile = isMobile ?? Application.isMobilePlatform;
            _applier = applier ?? new UnityFrameRateApplier();
        }

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            HighFrameRateInCombat = PlayerPrefs.GetInt(HighFrameRateInCombatKey, DefaultHighFrameRateInCombat ? 1 : 0) != 0;
            _activeSceneName = SceneManager.GetActiveScene().name;

            SceneManager.sceneLoaded += OnSceneLoaded;
            EventBus.Subscribe<GamePausedEvent>(OnGamePaused);
            EventBus.Subscribe<GameResumedEvent>(OnGameResumed);

            IsInitialized = true;
            Refresh();
        }

        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            EventBus.Unsubscribe<GamePausedEvent>(OnGamePaused);
            EventBus.Unsubscribe<GameResumedEvent>(OnGameResumed);
            IsInitialized = false;
        }

        public void SetHighFrameRateInCombat(bool enabled)
        {
            if (HighFrameRateInCombat == enabled)
            {
                return;
            }

            HighFrameRateInCombat = enabled;
            PlayerPrefs.SetInt(HighFrameRateInCombatKey, enabled ? 1 : 0);
            SettingsChanged?.Invoke();
            Refresh();
        }

        /// <summary>Main menu (or any full-screen non-gameplay menu) open/closed.</summary>
        public void SetMenuOpen(bool open)
        {
            _isMenuOpen = open;
            Refresh();
        }

        public void SetPaused(bool paused)
        {
            _isPaused = paused;
            Refresh();
        }

        /// <summary>Exposed for tests and for callers that switch scenes without SceneManager.sceneLoaded (additive setups).</summary>
        public void NotifySceneChanged(string sceneName)
        {
            _activeSceneName = sceneName;
            // A freshly loaded scene never starts paused: PauseMenuController lives in the scene and is rebuilt.
            _isPaused = false;
            Refresh();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
            {
                NotifySceneChanged(scene.name);
            }
        }

        private void OnGamePaused(GamePausedEvent _) => SetPaused(true);

        private void OnGameResumed(GameResumedEvent _) => SetPaused(false);

        private void Refresh()
        {
            FrameRateContext context = FrameRatePolicy.ResolveContext(_activeSceneName, CombatSceneName, WorldMapSceneName, _isPaused, _isMenuOpen);
            FrameRateSettings settings = FrameRatePolicy.Resolve(context, _isMobile, HighFrameRateInCombat);

            if (_hasApplied && context == CurrentContext && settings.Equals(CurrentSettings))
            {
                return;
            }

            CurrentContext = context;
            CurrentSettings = settings;
            _hasApplied = true;
            _applier.Apply(settings);
            GameLogger.Info(LogCategory, $"Frame pacing -> {context} ({settings}, mobile={_isMobile}).");
        }
    }
}
