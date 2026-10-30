using System.Collections.Generic;
using Rapadura.Audio;
using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Logging;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Cameras;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.PhotoMode;
using Rapadura.Gameplay.Player;
using Rapadura.UI.Common;
using Rapadura.UI.HUD;
using Rapadura.UI.Menus;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.PhotoMode
{
    /// <summary>
    /// "Modo foto": entered from the pause menu (encounters and world map) and from the world-map HUD
    /// button cluster. Freezes gameplay through <see cref="GameplayPause"/> (kept while the pause menu
    /// hides itself), hides every other UIDocument, takes over Camera.main with a constrained orbit
    /// camera around the hero (<see cref="PhotoCameraMath"/>: orbit/zoom/pan/roll/FOV with limits scaled
    /// to the hero's size, sphere-cast collision so the lens never goes through terrain), applies a look
    /// (<see cref="PhotoFilterPresets"/>, rendered by the URP assembly through <see cref="PhotoFilterRendering"/>),
    /// poses the hero (<see cref="PhotoPoseRig"/>, 5 poses per cousin) and saves PNGs
    /// (<see cref="PhotoCaptureService"/>) with a toast showing the path.
    ///
    /// Self-bootstrapping like <c>BagFullToastController</c>: one DontDestroyOnLoad host, PanelSettings
    /// borrowed from the HUD, view built in code and themed by Resources/UI/PhotoMode.uss. No scene or
    /// UXML edits: the entry buttons are injected into the pause menu list (before "Menu Principal") and
    /// the world-map cluster (before "Menu"), re-injected whenever those documents rebuild.
    ///
    /// Input is read straight from the devices in real time (the game is paused): mouse drag orbit,
    /// right/middle drag pan, wheel zoom, WASD/QE pan, arrows orbit, Z/C roll; touch drag orbit,
    /// pinch zoom + two-finger pan (world fingers only, <see cref="TouchGestureTracker"/>); gamepad
    /// sticks/triggers/shoulders/D-pad. Esc / B leaves — back to the pause menu one frame later when it
    /// was the entry point, so the HUD's own Esc handling of this frame can't close it again.
    /// Accessibility: "Reduzir Movimento" = no camera smoothing / pose blend, smaller wave;
    /// "Reduzir Flashes" = no shutter flash, capped bloom, less grain.
    /// </summary>
    [DefaultExecutionOrder(10000)]
    public class PhotoModeController : MonoBehaviour
    {
        public const float SortingOrder = 960f;
        public const string StyleResourcePath = "UI/PhotoMode";

        public const string PauseEntryButtonName = "photo-mode-button";
        public const string WorldMapEntryButtonName = "wm-photo-mode-button";

        public enum EntrySource
        {
            Code,
            PauseMenu,
            WorldMapHud,
        }

        [Tooltip("Camera smoothing sharpness (1/s). Ignored with \"Reduzir Movimento\".")]
        [SerializeField] private float _smoothing = 14f;
        [Tooltip("Degrees of orbit per screen width dragged.")]
        [SerializeField] private float _orbitDegreesPerScreen = 300f;
        [Tooltip("Pan speed (hero heights per second) for keys / stick.")]
        [SerializeField] private float _panSpeed = 1.2f;
        [SerializeField] private float _keyOrbitSpeed = 90f;
        [SerializeField] private float _rollSpeed = 40f;
        [Tooltip("Radius (m, × hero scale) of the collision probe toward the camera.")]
        [SerializeField] private float _collisionRadius = 0.2f;
        [SerializeField] private float _toastSeconds = 4f;

        private static PhotoModeController _instance;

        /// <summary>The live controller (null before the first scene finished loading).</summary>
        public static PhotoModeController Instance => _instance;

        /// <summary>True while the photo mode owns the camera.</summary>
        public bool IsActive { get; private set; }

        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _panel;
        private VisualElement _topBar;
        private VisualElement _flash;
        private Label _titleLabel;
        private Label _hintLabel;
        private Label _toastLabel;
        private Label _filterCaption;
        private Label _filterValue;
        private Label _poseCaption;
        private Label _poseValue;
        private VisualElement _filterRow;
        private Slider _rollSlider;
        private Slider _fovSlider;
        private Button _saveButton;
        private Button _hidePanelButton;
        private Button _recenterButton;
        private Button _exitButton;
        private Button _showPanelButton;

        // Entry points.
        private PauseMenuController _pauseMenu;
        private Button _pauseEntryButton;
        private WorldMapHudController _worldMapHud;
        private Button _mapEntryButton;
        private float _nextInjectCheck;
        private float _sceneLoadedAt;

        // Session.
        private EntrySource _source;
        private bool _reopenPauseMenu;
        private Camera _camera;
        private Vector3 _savedCameraPosition;
        private Quaternion _savedCameraRotation;
        private float _savedFov;
        private readonly List<Behaviour> _disabledCameraDrivers = new List<Behaviour>();
        private readonly List<UIDocument> _hiddenDocuments = new List<UIDocument>();
        private readonly List<StyleEnum<DisplayStyle>> _hiddenDisplays = new List<StyleEnum<DisplayStyle>>();
        private Transform _heroRoot;
        private CharacterId? _heroId;
        private float _heroHeight;
        private float _heroScale;
        private Vector3 _focusBase;
        private PhotoCameraLimits _limits;
        private PhotoCameraState _initial;
        private PhotoCameraState _target;
        private PhotoCameraState _current;
        private PhotoFilter _filter;
        private int _poseIndex;
        private PhotoPoseRig _poseRig;
        private int _savedRenderInterval = 1;
        private bool _panelVisible = true;
        private bool _capturing;
        private float _toastRemaining;
        private float _flashRemaining;

        // Pointer state.
        private bool _mouseOrbiting;
        private bool _mousePanning;
        private int _previousWorldTouches;
        private Vector2 _previousTouch0;
        private Vector2 _previousTouchMid;
        private float _previousPinchDistance;
        private readonly RaycastHit[] _hits = new RaycastHit[16];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null || FindFirstObjectByType<PhotoModeController>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(PhotoModeController));
            host.AddComponent<UIDocument>();
            host.AddComponent<PhotoModeController>();
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
        }

        private void OnEnable()
        {
            _root = null;
            SceneManager.sceneLoaded += OnSceneLoaded;
            _sceneLoadedAt = Time.unscaledTime;
            _nextInjectCheck = 0f;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (IsActive)
            {
                Exit(returnToPauseMenu: false);
            }

            FinishPauseMenuReturn(reopen: false);
            _root = null;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode != LoadSceneMode.Single)
            {
                return;
            }

            // Whatever we held belonged to the old scene (camera, hero, documents).
            if (IsActive)
            {
                Exit(returnToPauseMenu: false);
            }

            FinishPauseMenuReturn(reopen: false);
            _pauseMenu = null;
            _pauseEntryButton = null;
            _worldMapHud = null;
            _mapEntryButton = null;
            _sceneLoadedAt = Time.unscaledTime;
            _nextInjectCheck = 0f;
        }

        // ------------------------------------------------------------------
        // Entry points (injected buttons)
        // ------------------------------------------------------------------

        private void Update()
        {
            if (_reopenPauseMenu)
            {
                FinishPauseMenuReturn(reopen: true);
            }

            if (!IsActive && Time.unscaledTime >= _nextInjectCheck)
            {
                // Fast retries right after a load (documents bind a moment later), then a slow heartbeat
                // that also notices a document rebuilt by a disable/enable and the language changing.
                bool recentLoad = Time.unscaledTime - _sceneLoadedAt < 3f;
                _nextInjectCheck = Time.unscaledTime + (recentLoad ? 0.5f : 2f);
                InjectEntryButtons();
            }

            if (IsActive)
            {
                TickOverlays(Time.unscaledDeltaTime);
            }
        }

        private void InjectEntryButtons()
        {
            LocalizationManager localization = Localization;
            string label = PhotoModeTexts.Get(localization, PhotoModeTexts.Title);

            if (_pauseEntryButton == null || _pauseEntryButton.panel == null)
            {
                _pauseEntryButton = null;
                if (_pauseMenu == null) _pauseMenu = FindFirstObjectByType<PauseMenuController>();
                VisualElement list = DocumentRoot(_pauseMenu)?.Q<VisualElement>("pause-menu-button-list");
                if (list != null)
                {
                    _pauseEntryButton = InjectButton(list, PauseEntryButtonName, "main-menu-button", OnPauseEntryClicked, "pause-menu-button", "rap-button");
                }
            }

            if (_mapEntryButton == null || _mapEntryButton.panel == null)
            {
                _mapEntryButton = null;
                if (_worldMapHud == null) _worldMapHud = FindFirstObjectByType<WorldMapHudController>();
                VisualElement cluster = DocumentRoot(_worldMapHud)?.Q<VisualElement>("wm-button-cluster");
                if (cluster != null)
                {
                    _mapEntryButton = InjectButton(cluster, WorldMapEntryButtonName, "wm-menu-button", OnWorldMapEntryClicked, "wm-menu-button");
                }
            }

            if (_pauseEntryButton != null) _pauseEntryButton.text = label;
            if (_mapEntryButton != null) _mapEntryButton.text = label;
        }

        private static VisualElement DocumentRoot(Component owner)
        {
            UIDocument document = owner != null ? owner.GetComponent<UIDocument>() : null;
            return document != null ? document.rootVisualElement : null;
        }

        /// <summary>Adds (once) a button to <paramref name="container"/>, right before <paramref name="beforeName"/> when present.</summary>
        private static Button InjectButton(VisualElement container, string name, string beforeName, System.Action onClick, params string[] classes)
        {
            Button button = container.Q<Button>(name);
            if (button == null)
            {
                button = new Button { name = name, focusable = true };
                foreach (string className in classes)
                {
                    button.AddToClassList(className);
                }

                VisualElement before = container.Q<VisualElement>(beforeName);
                int index = before != null && before.parent == container ? container.IndexOf(before) : container.childCount;
                container.Insert(index, button);
            }

            button.clicked -= onClick;
            button.clicked += onClick;
            return button;
        }

        private void OnPauseEntryClicked() => Enter(EntrySource.PauseMenu);

        private void OnWorldMapEntryClicked()
        {
            // A modal (quest log, backpack, pause...) is up: the cluster button sits under it.
            if (!GameplayPause.IsPaused)
            {
                Enter(EntrySource.WorldMapHud);
            }
        }

        // ------------------------------------------------------------------
        // Enter / exit
        // ------------------------------------------------------------------

        /// <summary>Opens the photo mode. False (nothing changed) without a camera, a hero or a panel to draw on.</summary>
        public bool Enter(EntrySource source)
        {
            if (IsActive || _capturing)
            {
                return false;
            }

            Camera camera = Camera.main;
            if (camera == null || !ResolveHero(out Transform heroRoot, out Transform visual) || !EnsureBound())
            {
                GameLogger.Info(LogCategory, "Photo mode unavailable here (no camera, hero or panel).");
                return false;
            }

            // Hold the pause before the pause menu lets go of its own, so gameplay never resumes in between.
            GameplayPause.Acquire(this);
            _reopenPauseMenu = false; // a pending return to the pause menu: we still hold its pause, just drop the reopen
            _source = source;
            if (source == EntrySource.PauseMenu && _pauseMenu != null && _pauseMenu.IsOpen)
            {
                _pauseMenu.Hide();
            }

            _camera = camera;
            _savedCameraPosition = camera.transform.position;
            _savedCameraRotation = camera.transform.rotation;
            _savedFov = camera.fieldOfView;
            _disabledCameraDrivers.Clear();
            foreach (CameraController driver in camera.GetComponentsInParent<CameraController>())
            {
                if (driver.enabled)
                {
                    driver.enabled = false;
                    _disabledCameraDrivers.Add(driver);
                }
            }

            _heroRoot = heroRoot;
            _heroId = ServiceLocator.TryGet(out CharacterSelectionState selection) && selection != null ? selection.Selected : null;
            Bounds bounds = HeroBounds(heroRoot, visual);
            _heroHeight = Mathf.Max(0.5f, bounds.size.y);
            _heroScale = Mathf.Clamp(_heroHeight / PhotoCameraLimits.ReferenceHeroHeight, 0.5f, 4f);
            _focusBase = new Vector3(heroRoot.position.x, bounds.min.y + _heroHeight * 0.55f, heroRoot.position.z);
            _limits = PhotoCameraLimits.ForHeroHeight(_heroHeight);
            _initial = PhotoCameraMath.FromCamera(_savedCameraPosition, _savedCameraRotation, _focusBase, _savedFov, _limits);
            _target = _current = _initial;

            _poseRig = visual != null ? new PhotoPoseRig(visual, heroRoot, _heroHeight, bounds.min.y) : null;
            _poseRig?.Capture();
            _poseIndex = 0;
            _filter = PhotoFilter.None;

            HideOtherDocuments();
            _savedRenderInterval = OnDemandRendering.renderFrameInterval;
            _mouseOrbiting = _mousePanning = false;
            _previousWorldTouches = 0;
            _toastRemaining = 0f;
            _flashRemaining = 0f;
            _panelVisible = true;
            IsActive = true;

            ApplyFilter();
            ApplyPose(instant: false);
            RefreshTexts();
            SyncSliders();
            _root.style.display = DisplayStyle.Flex;
            ApplyPanelVisibility();
            _toastLabel.style.display = DisplayStyle.None;
            UiMotionPreferences.ApplyTo(_root, UiMotionPreferences.IsReducedMotion());
            GameLogger.Info(LogCategory, $"Photo mode opened from {source} (hero height {_heroHeight:0.00}, rig={_poseRig?.IsRigged}).");
            return true;
        }

        /// <summary>Closes the photo mode and gives camera, HUD, hero pose and pause back.</summary>
        public void Exit() => Exit(returnToPauseMenu: true);

        private void Exit(bool returnToPauseMenu)
        {
            if (!IsActive)
            {
                return;
            }

            IsActive = false;
            StopAllCoroutines();
            _capturing = false;

            _poseRig?.Restore();
            _poseRig = null;
            PhotoFilterRendering.Renderer?.Clear();

            if (_camera != null)
            {
                _camera.transform.SetPositionAndRotation(_savedCameraPosition, _savedCameraRotation);
                _camera.fieldOfView = _savedFov;
            }

            foreach (Behaviour driver in _disabledCameraDrivers)
            {
                if (driver != null) driver.enabled = true;
            }

            _disabledCameraDrivers.Clear();
            _camera = null;
            _heroRoot = null;
            RestoreOtherDocuments();
            OnDemandRendering.renderFrameInterval = _savedRenderInterval;

            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
                _root.style.visibility = Visibility.Visible; // a capture may have been cut short
            }

            if (returnToPauseMenu && _source == EntrySource.PauseMenu && _pauseMenu != null)
            {
                // Keep the pause held; the menu reopens next frame (see class docs).
                _reopenPauseMenu = true;
                return;
            }

            GameplayPause.Release(this);
        }

        private void FinishPauseMenuReturn(bool reopen)
        {
            if (!_reopenPauseMenu)
            {
                return;
            }

            _reopenPauseMenu = false;
            if (reopen && _pauseMenu != null)
            {
                _pauseMenu.Show();
            }

            if (!IsActive)
            {
                GameplayPause.Release(this);
            }
        }

        private bool ResolveHero(out Transform root, out Transform visual)
        {
            PlayerProceduralAnimator animator = FindFirstObjectByType<PlayerProceduralAnimator>();
            if (animator != null && animator.isActiveAndEnabled)
            {
                root = animator.transform;
                visual = animator.VisualRoot;
                return true;
            }

            WorldMapHeroToken token = FindFirstObjectByType<WorldMapHeroToken>();
            if (token != null && token.isActiveAndEnabled)
            {
                root = token.transform;
                visual = null;
                for (int i = 0; i < root.childCount && visual == null; i++)
                {
                    Transform child = root.GetChild(i);
                    if (child.gameObject.activeInHierarchy && child.name.StartsWith("HeroToken_", System.StringComparison.Ordinal))
                    {
                        visual = child;
                    }
                }

                return true;
            }

            GameObject player = GameObject.FindWithTag("Player");
            root = player != null ? player.transform : null;
            visual = null;
            return root != null;
        }

        private static Bounds HeroBounds(Transform root, Transform visual)
        {
            Transform source = visual != null ? visual : root;
            bool any = false;
            var bounds = new Bounds(root.position + Vector3.up * PhotoCameraLimits.ReferenceHeroHeight * 0.5f,
                new Vector3(0.5f, PhotoCameraLimits.ReferenceHeroHeight, 0.5f));

            foreach (Renderer renderer in source.GetComponentsInChildren<Renderer>())
            {
                if (!renderer.enabled || renderer is ParticleSystemRenderer || renderer is TrailRenderer || renderer is LineRenderer)
                {
                    continue;
                }

                if (!any)
                {
                    bounds = renderer.bounds;
                    any = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return bounds;
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
        // Per frame: input → camera → pose
        // ------------------------------------------------------------------

        private void LateUpdate()
        {
            if (!IsActive)
            {
                return;
            }

            if (_camera == null || _heroRoot == null)
            {
                Exit(returnToPauseMenu: false);
                return;
            }

            float deltaTime = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            bool reducedMotion = ReduceMotion;

            if (!_capturing)
            {
                ReadInput(deltaTime);
                if (!IsActive)
                {
                    return; // Esc / B closed the photo mode
                }
            }

            _target = PhotoCameraMath.Clamp(_target, _limits);
            _current = reducedMotion ? _target : PhotoCameraMath.Damp(_current, _target, _smoothing, deltaTime);

            Vector3 focus = _focusBase + _current.Pan;
            Quaternion rotation = PhotoCameraMath.Rotation(_current);
            Vector3 back = rotation * Vector3.back;
            float distance = ResolveCollision(focus, back, _current.Distance);
            Vector3 position = PhotoCameraMath.ClampToRadius(focus + back * distance, _focusBase, _limits.MaxCameraRadius);
            _camera.transform.SetPositionAndRotation(position, rotation);
            _camera.fieldOfView = _current.Fov;

            _poseRig?.Apply(Time.unscaledTime, deltaTime, reducedMotion);

            // Paused pacing renders every other frame (FrameRatePolicy); the moving photo camera needs every frame.
            OnDemandRendering.renderFrameInterval = 1;
        }

        private float ResolveCollision(Vector3 focus, Vector3 back, float desired)
        {
            float radius = _collisionRadius * _heroScale;
            int count = Physics.SphereCastNonAlloc(focus, radius, back, _hits, desired, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            bool hit = false;
            float nearest = desired;
            for (int i = 0; i < count; i++)
            {
                Collider collider = _hits[i].collider;
                // Overlaps at the start report distance 0 (focus inside something) — ignore them, and the hero itself.
                if (collider == null || _hits[i].distance <= 0f || collider.transform.IsChildOf(_heroRoot))
                {
                    continue;
                }

                if (_hits[i].distance < nearest)
                {
                    nearest = _hits[i].distance;
                    hit = true;
                }
            }

            return PhotoCameraMath.ResolveCollisionDistance(desired, hit, nearest, _limits.MinDistance * 0.5f, radius * 0.5f);
        }

        private void ReadInput(float deltaTime)
        {
            float degreesPerPixel = _orbitDegreesPerScreen / Mathf.Max(1, Screen.width);
            float panPerPixel = _current.Distance * 0.0015f;
            Quaternion view = PhotoCameraMath.Rotation(_current);

            ReadKeyboard(deltaTime);
            ReadGamepad(deltaTime);

            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                Vector2 position = mouse.position.ReadValue();
                bool overUi = (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame)
                              && UiPointerQuery.IsPointerOverUi(position);
                if (mouse.leftButton.wasPressedThisFrame) _mouseOrbiting = !overUi;
                if (mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame) _mousePanning = !overUi;
                if (!mouse.leftButton.isPressed) _mouseOrbiting = false;
                if (!mouse.rightButton.isPressed && !mouse.middleButton.isPressed) _mousePanning = false;

                Vector2 delta = mouse.delta.ReadValue();
                if (_mouseOrbiting)
                {
                    _target.Yaw += delta.x * degreesPerPixel;
                    _target.Pitch -= delta.y * degreesPerPixel;
                }

                if (_mousePanning)
                {
                    _target.Pan -= (view * Vector3.right * delta.x + view * Vector3.up * delta.y) * panPerPixel;
                }

                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f && !UiPointerQuery.IsPointerOverUi(position))
                {
                    _target.Distance = PhotoCameraMath.ZoomDistance(_target.Distance, scroll);
                }
            }

            // Touch: one world finger orbits; two pinch-zoom and pan with their midpoint. Fingers that
            // started on the panel (sliders, buttons) never count (TouchGestureTracker).
            TouchGestureTracker.Refresh();
            int touches = TouchGestureTracker.WorldTouchCount;
            Vector2 touch0 = TouchGestureTracker.WorldTouch0;
            Vector2 touch1 = TouchGestureTracker.WorldTouch1;
            if (touches == 1)
            {
                if (_previousWorldTouches == 1)
                {
                    Vector2 delta = touch0 - _previousTouch0;
                    _target.Yaw += delta.x * degreesPerPixel;
                    _target.Pitch -= delta.y * degreesPerPixel;
                }

                _previousTouch0 = touch0;
            }
            else if (touches >= 2)
            {
                Vector2 mid = (touch0 + touch1) * 0.5f;
                float pinch = Vector2.Distance(touch0, touch1);
                if (_previousWorldTouches >= 2 && _previousPinchDistance > 1f && pinch > 1f)
                {
                    _target.Distance *= _previousPinchDistance / pinch;
                    Vector2 delta = mid - _previousTouchMid;
                    _target.Pan -= (view * Vector3.right * delta.x + view * Vector3.up * delta.y) * panPerPixel;
                }

                _previousTouchMid = mid;
                _previousPinchDistance = pinch;
            }

            _previousWorldTouches = touches;
        }

        private void ReadKeyboard(float deltaTime)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                Exit();
                return;
            }

            var planar = new Vector2(Axis(keyboard.dKey, keyboard.aKey), Axis(keyboard.wKey, keyboard.sKey));
            float vertical = Axis(keyboard.eKey, keyboard.qKey);
            float speed = _panSpeed * _heroHeight * deltaTime * (keyboard.shiftKey.isPressed ? 2.5f : 1f);
            _target.Pan += PhotoCameraMath.PlanarPanDelta(_target.Yaw, planar, vertical) * speed;

            _target.Yaw += Axis(keyboard.leftArrowKey, keyboard.rightArrowKey) * _keyOrbitSpeed * deltaTime;
            _target.Pitch += Axis(keyboard.upArrowKey, keyboard.downArrowKey) * _keyOrbitSpeed * 0.6f * deltaTime;
            float roll = Axis(keyboard.zKey, keyboard.cKey);
            if (roll != 0f)
            {
                _target.Roll += roll * _rollSpeed * deltaTime;
                SyncSliders();
            }

            int step = keyboard.shiftKey.isPressed ? -1 : 1;
            if (keyboard.fKey.wasPressedThisFrame) CycleFilter(step);
            if (keyboard.gKey.wasPressedThisFrame) CyclePose(step);
            if (keyboard.rKey.wasPressedThisFrame) Recenter();
            if (keyboard.hKey.wasPressedThisFrame) TogglePanel();
            if (keyboard.pKey.wasPressedThisFrame || keyboard.f12Key.wasPressedThisFrame) SavePhoto();
        }

        private void ReadGamepad(float deltaTime)
        {
            Gamepad gamepad = Gamepad.current;
            if (gamepad == null || !IsActive)
            {
                return;
            }

            if (gamepad.buttonEast.wasPressedThisFrame || gamepad.startButton.wasPressedThisFrame)
            {
                Exit();
                return;
            }

            Vector2 move = gamepad.leftStick.ReadValue();
            _target.Pan += PhotoCameraMath.PlanarPanDelta(_target.Yaw, move, 0f) * (_panSpeed * _heroHeight * deltaTime);

            Vector2 look = gamepad.rightStick.ReadValue();
            _target.Yaw += look.x * _keyOrbitSpeed * 1.4f * deltaTime;
            _target.Pitch -= look.y * _keyOrbitSpeed * deltaTime;

            float zoom = gamepad.leftTrigger.ReadValue() - gamepad.rightTrigger.ReadValue();
            if (Mathf.Abs(zoom) > 0.05f)
            {
                _target.Distance *= Mathf.Pow(2f, zoom * deltaTime);
            }

            float roll = (gamepad.leftShoulder.isPressed ? 1f : 0f) - (gamepad.rightShoulder.isPressed ? 1f : 0f);
            if (roll != 0f)
            {
                _target.Roll += roll * _rollSpeed * deltaTime;
                SyncSliders();
            }

            if (gamepad.dpad.right.wasPressedThisFrame) CycleFilter(1);
            if (gamepad.dpad.left.wasPressedThisFrame) CycleFilter(-1);
            if (gamepad.dpad.up.wasPressedThisFrame) CyclePose(1);
            if (gamepad.dpad.down.wasPressedThisFrame) CyclePose(-1);
            if (gamepad.buttonWest.wasPressedThisFrame) Recenter();
            if (gamepad.buttonNorth.wasPressedThisFrame) TogglePanel();
            if (gamepad.buttonSouth.wasPressedThisFrame) SavePhoto();
        }

        private static float Axis(UnityEngine.InputSystem.Controls.KeyControl positive, UnityEngine.InputSystem.Controls.KeyControl negative) =>
            (positive.isPressed ? 1f : 0f) - (negative.isPressed ? 1f : 0f);

        // ------------------------------------------------------------------
        // Actions
        // ------------------------------------------------------------------

        public void CycleFilter(int step)
        {
            if (!PhotoFilterRendering.IsAvailable)
            {
                return;
            }

            _filter = PhotoFilterPresets.Cycle(_filter, step);
            ApplyFilter();
            RefreshTexts();
        }

        public void CyclePose(int step)
        {
            _poseIndex = PhotoPoseLibrary.WrapIndex(_poseIndex + step, PhotoPoseLibrary.For(_heroId).Count);
            ApplyPose(instant: false);
            RefreshTexts();
        }

        public void Recenter()
        {
            _target = _initial;
            SyncSliders();
        }

        public void TogglePanel()
        {
            _panelVisible = !_panelVisible;
            ApplyPanelVisibility();
        }

        /// <summary>Hides the photo UI for a frame, writes the PNG and shows a toast with the path.</summary>
        public void SavePhoto()
        {
            if (!IsActive || _capturing)
            {
                return;
            }

            _capturing = true;
            _root.style.visibility = Visibility.Hidden;
            BlurFocus();
            StartCoroutine(CaptureAfterHide());
        }

        private System.Collections.IEnumerator CaptureAfterHide()
        {
            // One full frame without the panel before reading the screen.
            yield return null;
            yield return PhotoCaptureService.CaptureRoutine(_heroId, OnPhotoSaved);
        }

        private void OnPhotoSaved(string path)
        {
            _capturing = false;
            if (_root == null)
            {
                return;
            }

            _root.style.visibility = Visibility.Visible;
            LocalizationManager localization = Localization;
            ShowToast(path != null
                ? PhotoModeTexts.Format(localization, PhotoModeTexts.Saved, path)
                : PhotoModeTexts.Get(localization, PhotoModeTexts.SaveFailed));

            if (path != null)
            {
                GameLogger.Info(LogCategory, "Photo saved: " + path);
                if (!ReduceFlashes)
                {
                    _flashRemaining = 0.25f;
                }
            }
        }

        private void ApplyFilter()
        {
            IPhotoFilterRenderer renderer = PhotoFilterRendering.Renderer;
            if (renderer == null || _camera == null)
            {
                return;
            }

            if (_filter == PhotoFilter.None)
            {
                renderer.Clear();
                return;
            }

            renderer.Apply(_camera, PhotoFilterPresets.ForAccessibility(PhotoFilterPresets.Get(_filter), ReduceFlashes));
        }

        private void ApplyPose(bool instant)
        {
            _poseRig?.SetPose(PhotoPoseLibrary.Get(_heroId, _poseIndex), instant || ReduceMotion);
        }

        private void ShowToast(string text)
        {
            if (_toastLabel == null)
            {
                return;
            }

            _toastLabel.text = text;
            _toastLabel.style.display = DisplayStyle.Flex;
            _toastRemaining = _toastSeconds;
        }

        private void TickOverlays(float deltaTime)
        {
            if (_toastRemaining > 0f)
            {
                _toastRemaining -= deltaTime;
                if (_toastRemaining <= 0f && _toastLabel != null)
                {
                    _toastLabel.style.display = DisplayStyle.None;
                }
            }

            if (_flash != null)
            {
                _flashRemaining = Mathf.Max(0f, _flashRemaining - deltaTime);
                _flash.style.opacity = _flashRemaining > 0f ? _flashRemaining / 0.25f * 0.8f : 0f;
            }
        }

        private void ApplyPanelVisibility()
        {
            if (_panel == null)
            {
                return;
            }

            DisplayStyle shown = _panelVisible ? DisplayStyle.Flex : DisplayStyle.None;
            _panel.style.display = shown;
            _topBar.style.display = shown;
            _showPanelButton.style.display = _panelVisible ? DisplayStyle.None : DisplayStyle.Flex;
            BlurFocus();
        }

        private void SyncSliders()
        {
            _rollSlider?.SetValueWithoutNotify(_target.Roll);
            _fovSlider?.SetValueWithoutNotify(_target.Fov);
        }

        private void BlurFocus()
        {
            // A clicked button keeps focus; gamepad South would then also "submit" it.
            _root?.focusController?.focusedElement?.Blur();
        }

        private void RefreshTexts()
        {
            if (_root == null)
            {
                return;
            }

            LocalizationManager localization = Localization;
            _titleLabel.text = PhotoModeTexts.Get(localization, PhotoModeTexts.Title);
            _hintLabel.text = PhotoModeTexts.Get(localization, HintKey());
            _filterCaption.text = PhotoModeTexts.Get(localization, PhotoModeTexts.Filter);
            _poseCaption.text = PhotoModeTexts.Get(localization, PhotoModeTexts.Pose);
            _rollSlider.label = PhotoModeTexts.Get(localization, PhotoModeTexts.Roll);
            _fovSlider.label = PhotoModeTexts.Get(localization, PhotoModeTexts.Fov);
            _saveButton.text = PhotoModeTexts.Get(localization, PhotoModeTexts.Save);
            _hidePanelButton.text = PhotoModeTexts.Get(localization, PhotoModeTexts.HidePanel);
            _showPanelButton.text = PhotoModeTexts.Get(localization, PhotoModeTexts.ShowPanel);
            _recenterButton.text = PhotoModeTexts.Get(localization, PhotoModeTexts.Recenter);
            _exitButton.text = PhotoModeTexts.Get(localization, PhotoModeTexts.Exit);

            bool filters = PhotoFilterRendering.IsAvailable;
            _filterValue.text = filters
                ? PhotoModeTexts.Get(localization, PhotoFilterPresets.LabelKey(_filter))
                : PhotoModeTexts.Get(localization, PhotoModeTexts.FiltersUnavailable);
            _filterRow.EnableInClassList("photo-row--disabled", !filters);
            _poseValue.text = PhotoModeTexts.Get(localization, PhotoPoseLibrary.Get(_heroId, _poseIndex).LabelKey);
        }

        private static string HintKey()
        {
            if (Touchscreen.current != null && Mouse.current == null) return PhotoModeTexts.HintTouch;
            if (Gamepad.current != null && Keyboard.current == null) return PhotoModeTexts.HintGamepad;
            return PhotoModeTexts.HintPointer;
        }

        // ------------------------------------------------------------------
        // View
        // ------------------------------------------------------------------

        /// <summary>Gives the document a panel (borrowed PanelSettings) and builds the view once. False until it can render.</summary>
        private bool EnsureBound()
        {
            if (_uiDocument == null)
            {
                return false;
            }

            if (_uiDocument.panelSettings == null)
            {
                PanelSettings donor = FindPanelSettings();
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

            documentRoot.pickingMode = PickingMode.Ignore;
            BuildView(documentRoot);
            UiClickSoundBinder.Register(_uiDocument);
            return true;
        }

        private void BuildView(VisualElement documentRoot)
        {
            _root = new VisualElement { name = "photo-root", pickingMode = PickingMode.Ignore };
            _root.AddToClassList("photo-root");
            _root.AddToClassList("rap-theme");
            StyleSheet sheet = Resources.Load<StyleSheet>(StyleResourcePath);
            if (sheet != null)
            {
                _root.styleSheets.Add(sheet);
            }

            _flash = Element("photo-flash");
            _flash.style.opacity = 0f;
            _root.Add(_flash);

            _topBar = Element("photo-top");
            _titleLabel = Text("photo-title");
            _hintLabel = Text("photo-hint");
            _topBar.Add(_titleLabel);
            _topBar.Add(_hintLabel);
            _root.Add(_topBar);

            _toastLabel = Text("photo-toast");
            _toastLabel.style.display = DisplayStyle.None;
            _root.Add(_toastLabel);

            _panel = Element("photo-panel", PickingMode.Position);
            _panel.AddToClassList("rap-panel");

            _filterRow = Selector(out _filterCaption, out _filterValue, () => CycleFilter(-1), () => CycleFilter(1));
            _panel.Add(_filterRow);
            _panel.Add(Selector(out _poseCaption, out _poseValue, () => CyclePose(-1), () => CyclePose(1)));

            var sliders = Element("photo-row", PickingMode.Position);
            _rollSlider = MakeSlider(-SliderLimits.MaxRoll, SliderLimits.MaxRoll, value => _target.Roll = value);
            _fovSlider = MakeSlider(SliderLimits.MinFov, SliderLimits.MaxFov, value => _target.Fov = value);
            sliders.Add(_rollSlider);
            sliders.Add(_fovSlider);
            _panel.Add(sliders);

            var actions = Element("photo-row", PickingMode.Position);
            _saveButton = MakeButton("photo-button", SavePhoto);
            _saveButton.AddToClassList("rap-button--primary");
            _hidePanelButton = MakeButton("photo-button", TogglePanel);
            _recenterButton = MakeButton("photo-button", Recenter);
            _exitButton = MakeButton("photo-button", Exit);
            actions.Add(_saveButton);
            actions.Add(_hidePanelButton);
            actions.Add(_recenterButton);
            actions.Add(_exitButton);
            _panel.Add(actions);
            _root.Add(_panel);

            _showPanelButton = MakeButton("photo-show-button", TogglePanel);
            _showPanelButton.style.display = DisplayStyle.None;
            _root.Add(_showPanelButton);

            _root.style.display = DisplayStyle.None;
            documentRoot.Add(_root);
        }

        /// <summary>Slider ranges are the same for every hero size (degrees), so the reference limits serve.</summary>
        private static readonly PhotoCameraLimits SliderLimits = PhotoCameraLimits.ForHeroHeight(PhotoCameraLimits.ReferenceHeroHeight);

        private VisualElement Selector(out Label caption, out Label value, System.Action previous, System.Action next)
        {
            var row = Element("photo-row", PickingMode.Position);
            row.AddToClassList("photo-selector");
            caption = Text("photo-caption");
            value = Text("photo-value");
            LocalizationManager localization = Localization;
            Button previousButton = MakeButton("photo-arrow", previous);
            previousButton.text = PhotoModeTexts.Get(localization, PhotoModeTexts.Previous);
            Button nextButton = MakeButton("photo-arrow", next);
            nextButton.text = PhotoModeTexts.Get(localization, PhotoModeTexts.Next);
            row.Add(caption);
            row.Add(previousButton);
            row.Add(value);
            row.Add(nextButton);
            return row;
        }

        private Slider MakeSlider(float low, float high, System.Action<float> onChanged)
        {
            var slider = new Slider(low, high) { focusable = true };
            slider.AddToClassList("photo-slider");
            slider.RegisterValueChangedCallback(evt => onChanged(evt.newValue));
            return slider;
        }

        private Button MakeButton(string className, System.Action onClick)
        {
            var button = new Button(() =>
            {
                onClick();
                BlurFocus();
            });
            button.AddToClassList("rap-button");
            button.AddToClassList(className);
            return button;
        }

        private static VisualElement Element(string className, PickingMode picking = PickingMode.Ignore)
        {
            var element = new VisualElement { pickingMode = picking };
            element.AddToClassList(className);
            return element;
        }

        private static Label Text(string className)
        {
            var label = new Label { pickingMode = PickingMode.Ignore };
            label.AddToClassList(className);
            return label;
        }

        private PanelSettings FindPanelSettings()
        {
            HudController hud = FindFirstObjectByType<HudController>();
            UIDocument hudDocument = hud != null ? hud.GetComponent<UIDocument>() : null;
            if (hudDocument != null && hudDocument.panelSettings != null)
            {
                return hudDocument.panelSettings;
            }

            foreach (UIDocument document in FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (document != _uiDocument && document.panelSettings != null)
                {
                    return document.panelSettings;
                }
            }

            return null;
        }

        // ------------------------------------------------------------------

        private const string LogCategory = "PhotoMode";

        private static LocalizationManager Localization
        {
            get
            {
                ServiceLocator.TryGet(out LocalizationManager localization);
                return localization;
            }
        }

        private static AccessibilitySettings Accessibility
        {
            get
            {
                ServiceLocator.TryGet(out AccessibilitySettings settings);
                return settings;
            }
        }

        private static bool ReduceMotion => Accessibility != null && Accessibility.ReduceMotion;

        private static bool ReduceFlashes => Accessibility != null && Accessibility.ReduceFlashEffects;
    }
}
