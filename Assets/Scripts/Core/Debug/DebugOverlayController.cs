using System;
using System.Collections.Generic;
using System.Text;
using Rapadura.Core.DI;
using Rapadura.Core.Logging;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Flow;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace Rapadura.Core.Debug
{
    /// <summary>
    /// Developer performance/state overlay (Fase 10 → Qualidade). Top-left, small monospace-ish text:
    /// smoothed FPS + frame time (and the worst frame of the last refresh window), GC bytes allocated
    /// in the last frame (<see cref="ProfilerRecorder"/> "GC Allocated In Frame"), total/managed memory
    /// (<see cref="Profiler.GetTotalAllocatedMemoryLong"/>), alive enemies, GameFlow state,
    /// <see cref="GameplayPause"/> owners, and biome / in-game clock / weather.
    ///
    /// Self-bootstrapping, but ONLY in the Editor and Development builds
    /// (<c>UNITY_EDITOR || DEVELOPMENT_BUILD</c>): one DontDestroyOnLoad host created after the first
    /// scene loads, like <c>BossIntroBannerController</c>, borrowing the PanelSettings of any UIDocument
    /// in the scene. Release builds never create it. Starts hidden; toggle with F1 / backquote on a
    /// keyboard or a quick three-finger tap (<see cref="DebugOverlayToggleGesture"/>).
    ///
    /// Cost when hidden: no sampling, no recorder, no UI work — Update only polls the two toggle keys
    /// and, while a finger is on the glass, counts pressed touches (a toggle has to be noticed
    /// somewhere). The <see cref="ProfilerRecorder"/> runs only while the overlay is visible. The view
    /// is built in code (one Label, picking-mode Ignore everywhere, so it never eats combat input);
    /// <c>Assets/UI/Debug/DebugOverlayView.uxml</c> is not used by the bootstrapped overlay.
    ///
    /// All smoothing/formatting lives in <see cref="DebugOverlayModel"/> / <see cref="DebugOverlayPresenter"/>
    /// so it is unit-tested without a UIDocument.
    ///
    /// Rolling log: below the stats, the newest <see cref="LogLines"/> entries of
    /// <see cref="DebugLogHistory"/> (GameLogger messages + uncaught exceptions, captured even while the
    /// overlay is hidden). F2 shows/hides just the log section. The log text is rebuilt only on a refresh
    /// tick where the ring's <see cref="LogRingBuffer.Version"/> changed.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class DebugOverlayController : MonoBehaviour
    {
        /// <summary>Above every gameplay/menu document (HUD, modals, boss banner at 850).</summary>
        public const float SortingOrder = 10000f;

        private const float RefreshIntervalSeconds = 0.25f;
        private const string GcAllocatedInFrameCounter = "GC Allocated In Frame";

        /// <summary>How many of the newest log entries the overlay shows.</summary>
        public const int LogLines = 12;

        /// <summary>Shows/hides only the rolling-log section (the overlay itself toggles with F1 / backquote).</summary>
        public const Key LogToggleKey = Key.F2;

        [SerializeField] private bool _startVisible;

        private static DebugOverlayController _instance;

        private readonly DebugOverlayModel _model = new DebugOverlayModel();
        private readonly DebugOverlayToggleGesture _gesture = new DebugOverlayToggleGesture();
        private readonly StringBuilder _text = new StringBuilder(256);
        private readonly List<object> _pauseOwners = new List<object>();
        private readonly List<string> _pauseOwnerNames = new List<string>();
        private readonly List<LogEntry> _logEntries = new List<LogEntry>(LogLines);
        private readonly StringBuilder _logText = new StringBuilder(LogLines * 64);

        private UIDocument _document;
        private VisualElement _root;
        private Label _label;
        private Label _logLabel;
        private bool _logVisible = true;
        private long _shownLogVersion = -1;
        private ProfilerRecorder _gcRecorder;
        private float _timeSinceRefresh;
        private bool _visible;

        public DebugOverlayModel Model => _model;
        public bool IsVisible => _visible;
        public bool IsLogVisible => _logVisible;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null || FindFirstObjectByType<DebugOverlayController>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(DebugOverlayController));
            host.AddComponent<UIDocument>();
            host.AddComponent<DebugOverlayController>();
        }
#endif

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

            _document = GetComponent<UIDocument>();
            _document.sortingOrder = SortingOrder;
        }

        private void OnEnable()
        {
            _gesture.Reset();
            SetVisible(_startVisible);
        }

        private void OnDisable()
        {
            StopRecorder();
            _visible = false;
        }

        private void OnDestroy()
        {
            StopRecorder();
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void Update()
        {
            if (PollToggle())
            {
                Toggle();
            }

            if (!_visible)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard[LogToggleKey].wasPressedThisFrame)
            {
                SetLogVisible(!_logVisible);
            }

            _model.RegisterFrame(Time.unscaledDeltaTime);
            _timeSinceRefresh += Time.unscaledDeltaTime;

            if (_timeSinceRefresh < RefreshIntervalSeconds)
            {
                return;
            }

            _timeSinceRefresh = 0f;
            Refresh();
        }

        /// <summary>Shows/hides the overlay. Showing (re)binds the view and starts the GC recorder; hiding stops both.</summary>
        public void SetVisible(bool visible)
        {
            _visible = visible;

            if (visible)
            {
                _model.Reset();
                _timeSinceRefresh = RefreshIntervalSeconds; // first text on the next frame
                StartRecorder();
            }
            else
            {
                StopRecorder();
            }

            if (EnsureBound())
            {
                _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public void Toggle()
        {
            SetVisible(!_visible);
        }

        /// <summary>Shows/hides the rolling-log section under the stats.</summary>
        public void SetLogVisible(bool visible)
        {
            _logVisible = visible;
            _shownLogVersion = -1; // rebuild on the next refresh
            if (_logLabel != null)
            {
                _logLabel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private bool PollToggle()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                foreach (Key key in DebugOverlayToggleGesture.ToggleKeys)
                {
                    if (keyboard[key].wasPressedThisFrame)
                    {
                        return true;
                    }
                }
            }

            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen == null)
            {
                return false;
            }

            // Nothing on the glass and no gesture running: one control read, no loop.
            if (!_gesture.IsTracking && !touchscreen.press.isPressed)
            {
                return false;
            }

            int fingers = 0;
            foreach (TouchControl touch in touchscreen.touches)
            {
                if (touch.press.isPressed)
                {
                    fingers++;
                }
            }

            return _gesture.Step(fingers, Time.unscaledTime);
        }

        private void Refresh()
        {
            if (!EnsureBound())
            {
                return;
            }

            if (_root.style.display != DisplayStyle.Flex)
            {
                _root.style.display = DisplayStyle.Flex;
            }

            DebugOverlaySnapshot snapshot = Sample();
            _label.text = DebugOverlayPresenter.Format(snapshot, _text);
            RefreshLog();
        }

        private void RefreshLog()
        {
            if (!_logVisible || _logLabel == null)
            {
                return;
            }

            LogRingBuffer ring = DebugLogHistory.Entries;
            long version = ring.Version;
            if (version == _shownLogVersion)
            {
                return;
            }

            _shownLogVersion = version;
            ring.CopyNewest(_logEntries, LogLines);
            _logText.Clear();
            if (_logEntries.Count == 0)
            {
                _logText.Append("log vazio");
            }
            else
            {
                DebugLogHistory.AppendLines(_logText, _logEntries);
            }

            _logLabel.text = _logText.ToString();
        }

        private DebugOverlaySnapshot Sample()
        {
            var snapshot = new DebugOverlaySnapshot
            {
                Fps = _model.SmoothedFps,
                FrameMilliseconds = _model.SmoothedFrameMilliseconds,
                PeakFrameMilliseconds = _model.ConsumePeakFrameMilliseconds(),
                GcAllocatedInFrameBytes = _gcRecorder.Valid ? _gcRecorder.LastValue : -1,
                TotalAllocatedBytes = Profiler.GetTotalAllocatedMemoryLong(),
                ManagedUsedBytes = Profiler.GetMonoUsedSizeLong(),
                ActiveEnemies = EnemyController.CountAlive(),
                ExternalPause = GameplayPause.IsExternallyPaused,
                Hour = -1f,
            };

            if (ServiceLocator.TryGet(out GameFlowService flow) && flow != null)
            {
                snapshot.FlowState = NameCache<GameFlowState>.Get(flow.State);
            }

            if (ServiceLocator.TryGet(out WeatherSystem weather) && weather != null)
            {
                snapshot.Biome = NameCache<Rapadura.Gameplay.World.Biomes.BiomeType>.Get(weather.CurrentBiome);
                snapshot.Weather = NameCache<WeatherType>.Get(weather.CurrentWeather);
            }

            if (ServiceLocator.TryGet(out TimeOfDayManager time) && time != null)
            {
                snapshot.Hour = time.CurrentHour;
                snapshot.Day = time.Day;
            }

            _pauseOwners.Clear();
            _pauseOwnerNames.Clear();
            GameplayPause.CopyOwnersTo(_pauseOwners);
            foreach (object owner in _pauseOwners)
            {
                _pauseOwnerNames.Add(owner != null ? owner.GetType().Name : null);
            }

            snapshot.PauseOwners = _pauseOwnerNames;
            return snapshot;
        }

        private void StartRecorder()
        {
            if (_gcRecorder.Valid)
            {
                return;
            }

            try
            {
                _gcRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, GcAllocatedInFrameCounter);
            }
            catch (Exception)
            {
                // Counter not available on this platform/build; the overlay shows "—" for GC/frame.
                _gcRecorder = default;
            }
        }

        private void StopRecorder()
        {
            if (_gcRecorder.Valid)
            {
                _gcRecorder.Dispose();
            }

            _gcRecorder = default;
        }

        /// <summary>Gives the document a panel (borrowed PanelSettings) and builds the view once. False until it can render.</summary>
        private bool EnsureBound()
        {
            if (_document == null)
            {
                return false;
            }

            if (_document.panelSettings == null)
            {
                PanelSettings donor = FindPanelSettings();
                if (donor == null)
                {
                    return false;
                }

                _document.panelSettings = donor;
                _document.sortingOrder = SortingOrder;
            }

            VisualElement documentRoot = _document.rootVisualElement;
            if (documentRoot == null)
            {
                return false;
            }

            if (_root != null && _root.parent == documentRoot)
            {
                return true;
            }

            documentRoot.pickingMode = PickingMode.Ignore;
            BuildView(documentRoot);
            return true;
        }

        private void BuildView(VisualElement documentRoot)
        {
            _root = new VisualElement { name = "debug-overlay-root", pickingMode = PickingMode.Ignore };
            _root.style.position = Position.Absolute;
            _root.style.left = 6;
            _root.style.top = 6;
            _root.style.paddingLeft = 6;
            _root.style.paddingRight = 6;
            _root.style.paddingTop = 3;
            _root.style.paddingBottom = 3;
            _root.style.backgroundColor = new Color(0f, 0f, 0f, 0.6f);
            _root.style.borderTopLeftRadius = 3;
            _root.style.borderTopRightRadius = 3;
            _root.style.borderBottomLeftRadius = 3;
            _root.style.borderBottomRightRadius = 3;
            _root.style.display = _visible ? DisplayStyle.Flex : DisplayStyle.None;

            _label = new Label { name = "debug-overlay-label", pickingMode = PickingMode.Ignore, text = "FPS --" };
            _label.style.fontSize = 11;
            _label.style.color = new Color(0.47f, 1f, 0.47f, 1f);
            _label.style.unityFontStyleAndWeight = FontStyle.Bold;
            _label.style.whiteSpace = WhiteSpace.NoWrap;
            _label.style.marginLeft = 0;
            _label.style.marginRight = 0;
            _label.style.marginTop = 0;
            _label.style.marginBottom = 0;
            _label.style.paddingLeft = 0;
            _label.style.paddingRight = 0;
            _label.style.paddingTop = 0;
            _label.style.paddingBottom = 0;

            _logLabel = new Label { name = "debug-overlay-log", pickingMode = PickingMode.Ignore, text = string.Empty };
            _logLabel.style.fontSize = 10;
            _logLabel.style.color = new Color(0.85f, 0.95f, 0.85f, 1f);
            _logLabel.style.whiteSpace = WhiteSpace.NoWrap;
            _logLabel.style.marginLeft = 0;
            _logLabel.style.marginRight = 0;
            _logLabel.style.marginTop = 4;
            _logLabel.style.marginBottom = 0;
            _logLabel.style.paddingLeft = 0;
            _logLabel.style.paddingRight = 0;
            _logLabel.style.paddingTop = 0;
            _logLabel.style.paddingBottom = 0;
            _logLabel.style.display = _logVisible ? DisplayStyle.Flex : DisplayStyle.None;
            _shownLogVersion = -1;

            _root.Add(_label);
            _root.Add(_logLabel);
            documentRoot.Add(_root);
        }

        private PanelSettings FindPanelSettings()
        {
            foreach (UIDocument document in FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (document != _document && document.panelSettings != null)
                {
                    return document.panelSettings;
                }
            }

            return null;
        }

        /// <summary>Enum → name without the per-refresh ToString allocation (which would show up in the GC/frame line).</summary>
        private static class NameCache<TEnum> where TEnum : struct, Enum
        {
            private static readonly Dictionary<TEnum, string> Names = new Dictionary<TEnum, string>();

            public static string Get(TEnum value)
            {
                if (!Names.TryGetValue(value, out string name))
                {
                    name = value.ToString();
                    Names[value] = name;
                }

                return name;
            }
        }
    }
}
