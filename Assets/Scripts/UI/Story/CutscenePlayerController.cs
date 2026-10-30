using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Cameras;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.Story
{
    /// <summary>
    /// Small code-driven cutscene player (no Timeline: com.unity.timeline is not in the project, and the world map is
    /// built in code, so there is nothing to author a Timeline against). Plays a <see cref="CutsceneDefinition"/> from
    /// <see cref="CutsceneScript"/>: takes over <see cref="Camera.main"/> and glides it between each shot's framings over
    /// the world map's region nodes (<see cref="CutsceneCameraMath"/>), draws letterbox bars, a title card, subtitles with
    /// a speaker portrait medal (hero accent colour from <see cref="CharacterRegistry"/>) and the shadow / light screen
    /// tints, hides every other UIDocument meanwhile and holds a <see cref="GameplayPause"/>. Music needs no cue of its
    /// own: <see cref="StoryPanelController.IsShowing"/> includes a playing cutscene, so <c>MusicDirector</c> keeps the
    /// Story context (the epilogue variant for the outro).
    ///
    /// Started only by <see cref="StoryPanelController"/> (intro before the prologue panels, outro after the ending
    /// panels, before the credits). Shots advance on their own after the line's reading time; tap / Space / Enter /
    /// gamepad South jumps to the next shot; "Pular" / Esc / gamepad East or Start ends it. "Reduzir Movimento" (or
    /// "Reduzir Flashes"): every shot is a still camera cut, bars and text appear without sliding or fading, tints
    /// are halved. <see cref="AutoPlay"/> off (PlayMode tests, <c>SmokeTestBase</c>) never plays one.
    ///
    /// Self-bootstrapping like <see cref="StoryPanelController"/>: one DontDestroyOnLoad host, borrowed PanelSettings,
    /// view built in code and themed by Resources/UI/Cutscene.uss.
    /// </summary>
    public class CutscenePlayerController : MonoBehaviour
    {
        /// <summary>Above the story panels (45) and tutorial bubble (40); below the death screen (50) and toasts (60).</summary>
        public const float SortingOrder = 47f;

        public const string StyleResourcePath = "UI/Cutscene";
        public const string RootName = "cutscene-root";
        public const string RootClass = "cutscene-root";
        public const string OpenClass = "cutscene-root--open";
        public const string ThemeClass = "rap-theme";
        public const string LineEnteringClass = "cutscene-line--entering";
        public const string NarratorClass = "cutscene-line--narrator";
        public const string PortraitClass = "cutscene-portrait";

        private const float AdvanceCooldownSeconds = 0.25f;

        /// <summary>When false, cutscenes are never played (PlayMode tests). Default true.</summary>
        public static bool AutoPlay = true;

        private static CutscenePlayerController _instance;

        private UIDocument _uiDocument;
        private StyleSheet _sheet;
        private VisualElement _root;
        private VisualElement _shadowTint;
        private VisualElement _lightTint;
        private Label _title;
        private Button _skipButton;
        private VisualElement _portrait;
        private Label _portraitInitial;
        private Label _speaker;
        private Label _line;
        private Label _hint;

        private CutsceneSequence _sequence;
        private Action _onDone;
        private int _renderedShot = -1;
        private float _lastAdvanceAt = -10f;
        private int _releasePauseAtFrame = -1;
        private bool _reducedMotion;
        private LocalizationManager _localization;

        private Camera _camera;
        private Vector3 _savedPosition;
        private Quaternion _savedRotation;
        private float _savedFov;
        private readonly List<Behaviour> _disabledDrivers = new List<Behaviour>();
        private readonly Dictionary<BiomeType, Vector3> _nodes = new Dictionary<BiomeType, Vector3>();
        private Vector3 _islandCentre;
        private float _mapScale = 1f;

        private readonly List<UIDocument> _hiddenDocuments = new List<UIDocument>();
        private readonly List<StyleEnum<DisplayStyle>> _hiddenDisplays = new List<StyleEnum<DisplayStyle>>();

        /// <summary>True while a cutscene is on screen.</summary>
        public static bool IsPlaying => _instance != null && _instance._sequence != null;

        /// <summary>The cutscene on screen (None when idle).</summary>
        public static CutsceneId Playing => IsPlaying ? _instance._sequence.Definition.Id : CutsceneId.None;

        /// <summary>Shot index on screen (-1 when idle).</summary>
        public static int PlayingShotIndex => IsPlaying ? _instance._sequence.Index : -1;

        /// <summary>Same as "Pular": ends the cutscene on screen (its completion callback runs). False when idle.</summary>
        public static bool SkipPlaying()
        {
            if (!IsPlaying)
            {
                return false;
            }

            _instance.Skip();
            return true;
        }

        /// <summary>
        /// Starts <paramref name="definition"/>; <paramref name="onDone"/> runs once it ends (read to the end or skipped).
        /// False — and <paramref name="onDone"/> NOT called — when nothing can play (no player instance, no panel to draw
        /// on, already playing): the caller just carries on without it.
        /// </summary>
        public static bool TryPlay(CutsceneDefinition definition, Action onDone)
        {
            if (definition == null || definition.Shots.Count == 0)
            {
                return false;
            }

            if (_instance == null)
            {
                Bootstrap();
            }

            return _instance != null && _instance.Begin(definition, onDone);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null || FindFirstObjectByType<CutscenePlayerController>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(CutscenePlayerController));
            host.AddComponent<UIDocument>();
            host.AddComponent<CutscenePlayerController>();
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

            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                _uiDocument = gameObject.AddComponent<UIDocument>();
            }

            _uiDocument.sortingOrder = SortingOrder;
            Rapadura.Audio.UiClickSoundBinder.Register(_uiDocument);
        }

        private void OnEnable()
        {
            _root = null;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Abort();
            _root = null;
        }

        private void OnDestroy()
        {
            GameplayPause.Release(this);
            Rapadura.Audio.UiClickSoundBinder.Unregister(_uiDocument);
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Abort();

        /// <summary>Drops a cutscene without running its callback (scene swap / disable): restores what it touched.</summary>
        private void Abort()
        {
            if (_sequence != null)
            {
                _sequence = null;
                _onDone = null;
                Restore();
            }

            _releasePauseAtFrame = -1;
            GameplayPause.Release(this);
        }

        private bool Begin(CutsceneDefinition definition, Action onDone)
        {
            if (_sequence != null || !EnsureBound())
            {
                return false;
            }

            ServiceLocator.TryGet(out _localization);
            _reducedMotion = UiMotionPreferences.IsReducedMotion();
            UiMotionPreferences.ApplyTo(_root, _reducedMotion);

            _onDone = onDone;
            _renderedShot = -1;
            _releasePauseAtFrame = -1;
            _lastAdvanceAt = Time.unscaledTime;
            GameplayPause.Acquire(this);

            TakeCamera();
            HideOtherDocuments();
            _root.style.display = DisplayStyle.Flex;
            _root.pickingMode = PickingMode.Position;
            _title.text = CutsceneTexts.Get(_localization, definition.TitleKey);
            _skipButton.text = CutsceneTexts.Get(_localization, CutsceneTexts.Skip);
            _hint.text = CutsceneTexts.Get(_localization, CutsceneTexts.TapHint);

            if (_reducedMotion)
            {
                _root.AddToClassList(OpenClass);
            }
            else
            {
                // Next frame, so the bars slide in from off-screen.
                _root.RemoveFromClassList(OpenClass);
                _root.schedule.Execute(() => _root?.AddToClassList(OpenClass));
            }

            _sequence = new CutsceneSequence(definition, shot => CutsceneRules.ReadingSeconds(CutsceneTexts.Get(_localization, shot.LineKey).Length));
            UiAccessibilityApplier.RequestRefresh();
            _skipButton.Focus();
            Draw();
            return true;
        }

        private void Update()
        {
            if (_releasePauseAtFrame >= 0 && Time.frameCount >= _releasePauseAtFrame)
            {
                _releasePauseAtFrame = -1;
                GameplayPause.Release(this);
            }

            if (_sequence == null)
            {
                return;
            }

            PollInput();
            if (_sequence == null)
            {
                return;
            }

            if (!_sequence.Tick(Time.unscaledDeltaTime))
            {
                Finish();
                return;
            }

            Draw();
        }

        private void PollInput()
        {
            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;

            bool skip = (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) ||
                        (gamepad != null && (gamepad.buttonEast.wasPressedThisFrame || gamepad.startButton.wasPressedThisFrame));
            if (skip)
            {
                Skip();
                return;
            }

            bool next = (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)) ||
                        (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame);
            if (next)
            {
                Next();
            }
        }

        private void Next()
        {
            if (_sequence == null || Time.unscaledTime - _lastAdvanceAt < AdvanceCooldownSeconds)
            {
                return;
            }

            _lastAdvanceAt = Time.unscaledTime;
            if (!_sequence.Next())
            {
                Finish();
                return;
            }

            Draw();
        }

        private void Skip()
        {
            if (_sequence == null)
            {
                return;
            }

            _sequence.Skip();
            Finish();
        }

        private void Finish()
        {
            Action done = _onDone;
            _onDone = null;
            _sequence = null;
            Restore();
            done?.Invoke();
            // A frame later, so the tap that ended it cannot also reach the map.
            _releasePauseAtFrame = Time.frameCount + 1;
        }

        private void Restore()
        {
            if (_root != null)
            {
                _root.RemoveFromClassList(OpenClass);
                _root.style.display = DisplayStyle.None;
                _root.pickingMode = PickingMode.Ignore;
            }

            RestoreOtherDocuments();
            ReleaseCamera();
        }

        // ------------------------------------------------------------------
        // Drawing
        // ------------------------------------------------------------------

        private void Draw()
        {
            if (_sequence == null || _sequence.IsFinished)
            {
                return;
            }

            CutsceneShot shot = _sequence.CurrentShot;
            if (_renderedShot != _sequence.Index)
            {
                _renderedShot = _sequence.Index;
                RenderLine(shot);
            }

            float tintScale = _reducedMotion ? 0.5f : 1f;
            _shadowTint.style.opacity = _sequence.Shadow * 0.55f * tintScale;
            _lightTint.style.opacity = _sequence.Light * 0.35f * tintScale;
            _title.style.display = _sequence.Index == 0 ? DisplayStyle.Flex : DisplayStyle.None;

            if (_camera != null)
            {
                CutsceneCameraMath.Pose(shot, _sequence.ShotProgress, FocusPoint, _mapScale, _reducedMotion,
                    out Vector3 position, out Quaternion rotation, out float fov);
                _camera.transform.SetPositionAndRotation(position, rotation);
                if (!_camera.orthographic)
                {
                    _camera.fieldOfView = fov;
                }
            }
        }

        private void RenderLine(CutsceneShot shot)
        {
            bool narrator = shot.Speaker == CutsceneSpeaker.Narrator;
            _portrait.style.display = narrator ? DisplayStyle.None : DisplayStyle.Flex;
            _speaker.style.display = narrator ? DisplayStyle.None : DisplayStyle.Flex;
            _line.EnableInClassList(NarratorClass, narrator);
            _line.text = CutsceneTexts.Get(_localization, shot.LineKey);

            if (!narrator)
            {
                string name = CutsceneTexts.Get(_localization, CutsceneScript.SpeakerNameKey(shot.Speaker));
                _speaker.text = name;
                _portraitInitial.text = string.IsNullOrEmpty(name) ? "?" : name.Substring(0, 1).ToUpperInvariant();

                foreach (CutsceneSpeaker speaker in (CutsceneSpeaker[])Enum.GetValues(typeof(CutsceneSpeaker)))
                {
                    _portrait.EnableInClassList(PortraitClass + "--" + CutsceneScript.SpeakerId(speaker), speaker == shot.Speaker);
                }

                CharacterId? hero = CutsceneScript.HeroOf(shot.Speaker);
                _portrait.style.backgroundColor = hero.HasValue ? new StyleColor(CharacterRegistry.Get(hero.Value).AccentColor) : new StyleColor(StyleKeyword.Null);
            }

            if (!_reducedMotion)
            {
                _line.AddToClassList(LineEnteringClass);
                _line.schedule.Execute(() => _line?.RemoveFromClassList(LineEnteringClass));
            }
        }

        // ------------------------------------------------------------------
        // Camera
        // ------------------------------------------------------------------

        private Vector3 FocusPoint(CutsceneFocus focus) =>
            CutsceneCameraMath.FocusPoint(focus, region => _nodes.TryGetValue(region, out Vector3 p) ? p : (Vector3?)null, _islandCentre);

        private void TakeCamera()
        {
            _nodes.Clear();
            _camera = Camera.main;
            if (_camera == null)
            {
                return;
            }

            var points = new List<Vector3>();
            WorldMapController map = FindFirstObjectByType<WorldMapController>();
            if (map != null && map.Nodes != null)
            {
                foreach (WorldMapRegionNode node in map.Nodes)
                {
                    if (node != null && !_nodes.ContainsKey(node.Region))
                    {
                        _nodes.Add(node.Region, node.Position);
                        points.Add(node.Position);
                    }
                }
            }

            if (points.Count == 0)
            {
                // Not on the world map (no nodes to frame): subtitles only, the camera stays put.
                _camera = null;
                return;
            }

            CutsceneCameraMath.Island(points, out _islandCentre, out float spread);
            _mapScale = CutsceneCameraMath.MapScale(spread);

            _savedPosition = _camera.transform.position;
            _savedRotation = _camera.transform.rotation;
            _savedFov = _camera.fieldOfView;
            _disabledDrivers.Clear();
            foreach (CameraController driver in _camera.GetComponentsInParent<CameraController>())
            {
                if (driver.enabled)
                {
                    driver.enabled = false;
                    _disabledDrivers.Add(driver);
                }
            }
        }

        private void ReleaseCamera()
        {
            if (_camera != null)
            {
                _camera.transform.SetPositionAndRotation(_savedPosition, _savedRotation);
                _camera.fieldOfView = _savedFov;
            }

            foreach (Behaviour driver in _disabledDrivers)
            {
                if (driver != null)
                {
                    driver.enabled = true;
                }
            }

            _disabledDrivers.Clear();
            _camera = null;
        }

        private void HideOtherDocuments()
        {
            _hiddenDocuments.Clear();
            _hiddenDisplays.Clear();
            foreach (UIDocument document in FindObjectsByType<UIDocument>(FindObjectsSortMode.None))
            {
                VisualElement root = document != _uiDocument ? document.rootVisualElement : null;
                if (root == null)
                {
                    continue;
                }

                _hiddenDocuments.Add(document);
                _hiddenDisplays.Add(root.style.display);
                root.style.display = DisplayStyle.None;
            }
        }

        private void RestoreOtherDocuments()
        {
            for (int i = 0; i < _hiddenDocuments.Count; i++)
            {
                UIDocument document = _hiddenDocuments[i];
                VisualElement root = document != null ? document.rootVisualElement : null;
                if (root != null)
                {
                    root.style.display = _hiddenDisplays[i];
                }
            }

            _hiddenDocuments.Clear();
            _hiddenDisplays.Clear();
        }

        // ------------------------------------------------------------------
        // View
        // ------------------------------------------------------------------

        private bool EnsureBound()
        {
            if (_uiDocument == null)
            {
                return false;
            }

            if (_uiDocument.panelSettings == null)
            {
                PanelSettings donor = null;
                foreach (UIDocument document in FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (document != _uiDocument && document.panelSettings != null)
                    {
                        donor = document.panelSettings;
                        break;
                    }
                }

                if (donor == null)
                {
                    return false;
                }

                _uiDocument.panelSettings = donor;
                _uiDocument.sortingOrder = SortingOrder;
            }

            VisualElement documentRoot = _uiDocument.rootVisualElement;
            if (documentRoot == null)
            {
                return false;
            }

            if (_root != null && _root.parent == documentRoot)
            {
                return true;
            }

            if (_sheet == null)
            {
                _sheet = Resources.Load<StyleSheet>(StyleResourcePath);
            }

            documentRoot.pickingMode = PickingMode.Ignore;
            _root = CreateView(_sheet, UiMotionPreferences.IsReducedMotion());
            _shadowTint = _root.Q<VisualElement>("cutscene-tint-shadow");
            _lightTint = _root.Q<VisualElement>("cutscene-tint-light");
            _title = _root.Q<Label>("cutscene-title");
            _skipButton = _root.Q<Button>("cutscene-skip");
            _portrait = _root.Q<VisualElement>("cutscene-portrait");
            _portraitInitial = _root.Q<Label>("cutscene-portrait-initial");
            _speaker = _root.Q<Label>("cutscene-speaker");
            _line = _root.Q<Label>("cutscene-line");
            _hint = _root.Q<Label>("cutscene-hint");

            _skipButton.clicked += Skip;
            _root.RegisterCallback<PointerUpEvent>(OnPointerUp);
            _root.RegisterCallback<NavigationCancelEvent>(evt =>
            {
                evt.StopPropagation();
                Skip();
            });

            documentRoot.Add(_root);
            return true;
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            for (VisualElement element = evt.target as VisualElement; element != null; element = element.parent)
            {
                if (element is Button)
                {
                    return;
                }
            }

            Next();
        }

        /// <summary>
        /// The cutscene tree (hidden): tints, top bar (title + "Pular"), bottom bar (portrait, speaker, line, hint).
        /// Everything visual is a USS class of Resources/UI/Cutscene.uss. EditMode-testable.
        /// </summary>
        public static VisualElement CreateView(StyleSheet sheet, bool reducedMotion)
        {
            var root = new VisualElement { name = RootName, pickingMode = PickingMode.Ignore };
            root.AddToClassList(RootClass);
            root.AddToClassList(ThemeClass);
            if (sheet != null)
            {
                root.styleSheets.Add(sheet);
            }

            UiMotionPreferences.ApplyTo(root, reducedMotion);
            root.style.display = DisplayStyle.None;

            root.Add(Element("cutscene-tint-shadow", "cutscene-tint", "cutscene-tint--shadow"));
            root.Add(Element("cutscene-tint-light", "cutscene-tint", "cutscene-tint--light"));

            VisualElement top = Element("cutscene-bar-top", "cutscene-bar", "cutscene-bar--top");
            top.Add(Text("cutscene-title", "cutscene-title"));
            var skip = new Button { name = "cutscene-skip" };
            skip.AddToClassList("cutscene-skip");
            top.Add(skip);
            root.Add(top);

            VisualElement bottom = Element("cutscene-bar-bottom", "cutscene-bar", "cutscene-bar--bottom");
            VisualElement portrait = Element("cutscene-portrait", PortraitClass);
            portrait.Add(Text("cutscene-portrait-initial", "cutscene-portrait-initial"));
            bottom.Add(portrait);
            VisualElement text = Element("cutscene-text", "cutscene-text");
            text.Add(Text("cutscene-speaker", "cutscene-speaker"));
            text.Add(Text("cutscene-line", "cutscene-line"));
            bottom.Add(text);
            bottom.Add(Text("cutscene-hint", "cutscene-hint"));
            root.Add(bottom);
            return root;
        }

        private static VisualElement Element(string name, params string[] classes)
        {
            var element = new VisualElement { name = name, pickingMode = PickingMode.Ignore };
            foreach (string cls in classes)
            {
                element.AddToClassList(cls);
            }

            return element;
        }

        private static Label Text(string name, string cls)
        {
            var label = new Label { name = name, pickingMode = PickingMode.Ignore };
            label.AddToClassList(cls);
            return label;
        }
    }
}
