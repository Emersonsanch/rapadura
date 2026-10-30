using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.HUD;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.Mounts
{
    /// <summary>
    /// Base of the two mount HUDs: injects a <see cref="MountHudView"/> under the HUD document's root element
    /// (<see cref="RootName"/>), refreshes it from the rider every <see cref="RefreshInterval"/> and shows the
    /// <see cref="MountNotices"/> lines. Re-injects when the HUD document rebuilds its tree.
    /// </summary>
    public abstract class MountHudBase : MonoBehaviour
    {
        protected const float RefreshInterval = 0.1f;

        private UIDocument _document;
        private MountHudView _view;
        private float _nextRefresh;

        protected abstract string RootName { get; }
        protected abstract bool OnMap { get; }
        protected abstract IMountRider Rider { get; }
        protected virtual bool HudVisible => true;

        public MountHudView View => _view;

        protected virtual void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        protected virtual void OnEnable()
        {
            MountNotices.Raised += OnNotice;
            EventBus.Subscribe<MountGrantedEvent>(OnGranted);
            EventBus.Subscribe<FirstSaddleCompletedEvent>(OnFirstSaddle);
        }

        protected virtual void OnDisable()
        {
            MountNotices.Raised -= OnNotice;
            EventBus.Unsubscribe<MountGrantedEvent>(OnGranted);
            EventBus.Unsubscribe<FirstSaddleCompletedEvent>(OnFirstSaddle);
            IMountRider rider = Rider;
            if (rider != null)
            {
                rider.TouchMountHeld = false;
            }

            _view?.Root.RemoveFromHierarchy();
            _view = null;
        }

        private void Update()
        {
            if (!EnsureBuilt())
            {
                return;
            }

            _view.TickNotice();
            if (Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + RefreshInterval;
            _view.Refresh(Rider, HudVisible);
        }

        private void OnNotice(string text)
        {
            if (EnsureBuilt())
            {
                _view.ShowNotice(text);
            }
        }

        /// <summary>Called once the view is (re)injected under <paramref name="root"/>.</summary>
        protected virtual void OnViewBuilt(MountHudView view, VisualElement root)
        {
        }

        private void OnGranted(MountGrantedEvent evt)
        {
            if (evt.Source == MountCollection.SourceQuest && evt.MountId == MountCatalog.Pony)
            {
                return; // the Primeira Sela scene says it
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            string key = evt.Source == MountCollection.SourceDrop ? MountWaveTexts.DropToast : MountTexts.GrantedToast;
            OnNotice(MountTexts.Format(localization, key, MountTexts.MountName(localization, MountCatalog.Get(evt.MountId))));
        }

        /// <summary>"Primeira Sela": Vó Zefa hands over the pony, Lavine names him, Maithe pretends not to care, then the how-to.</summary>
        private void OnFirstSaddle(FirstSaddleCompletedEvent evt)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            foreach (string key in new[] { MountTexts.SceneLine1, MountTexts.SceneLine2, MountTexts.SceneLine3, MountTexts.SceneLine4 })
            {
                OnNotice(MountTexts.Get(localization, key));
            }
        }

        private bool EnsureBuilt()
        {
            if (_document == null || _document.rootVisualElement == null)
            {
                return false;
            }

            VisualElement root = _document.rootVisualElement.Q<VisualElement>(RootName) ?? _document.rootVisualElement;
            if (_view != null && _view.Root.parent == root)
            {
                return true;
            }

            _view?.Root.RemoveFromHierarchy();
            _view = new MountHudView(OnMap, MountHudView.ReducedMotion());
            root.Add(_view.Root);
            OnViewBuilt(_view, root);
            return true;
        }
    }

    /// <summary>
    /// Arena HUD of the mounts. Self-bootstrapping, no scene edit (same pattern as <c>DuoStrikeHud</c>): joins the
    /// <see cref="HudController"/>'s GameObject after every scene load and injects into <c>hud-root</c>.
    /// </summary>
    [DisallowMultipleComponent]
    public class MountHud : MountHudBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Attach();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Attach();

        private static void Attach()
        {
            HudController hud = FindFirstObjectByType<HudController>(FindObjectsInactive.Include);
            if (hud != null && hud.GetComponent<MountHud>() == null)
            {
                hud.gameObject.AddComponent<MountHud>();
            }
        }

        protected override string RootName => "hud-root";
        protected override bool OnMap => false;
        protected override IMountRider Rider => MountController.Active;
    }

    /// <summary>
    /// World map HUD of the mounts: joins the <see cref="WorldMapHudController"/>'s GameObject after every scene load
    /// and injects into <c>wm-hud-root</c> (so the HUD's pointer blocker covers the buttons). Hidden while the map HUD is.
    /// </summary>
    [DisallowMultipleComponent]
    public class WorldMapMountHud : MountHudBase
    {
        private WorldMapHudController _hud;
        private MountStablePanel _panel;
        private bool _screenRegistered;

        public MountStablePanel Panel => _panel;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Attach();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Attach();

        private static void Attach()
        {
            WorldMapHudController hud = FindFirstObjectByType<WorldMapHudController>(FindObjectsInactive.Include);
            if (hud != null && hud.GetComponent<WorldMapMountHud>() == null)
            {
                hud.gameObject.AddComponent<WorldMapMountHud>();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _hud = GetComponent<WorldMapHudController>();
        }

        protected override string RootName => "wm-hud-root";
        protected override bool OnMap => true;
        protected override IMountRider Rider => WorldMapMountRider.Current;
        protected override bool HudVisible => (_hud == null || _hud.IsVisible) && (_panel == null || !_panel.IsOpen);

        protected override void OnDisable()
        {
            _panel?.Close();
            _panel?.Root.RemoveFromHierarchy();
            _panel = null;
            base.OnDisable();
        }

        protected override void OnViewBuilt(MountHudView view, VisualElement root)
        {
            _panel?.Close();
            _panel?.Root.RemoveFromHierarchy();
            _panel = new MountStablePanel(MountHudView.ReducedMotion());
            root.Add(_panel.Root);
            view.StableRequested += OpenPanel;
            if (_hud != null && !_screenRegistered)
            {
                // Joins the HUD's screens: one at a time, Esc closes it before the pause menu.
                _screenRegistered = true;
                _hud.RegisterScreen(MountStablePanel.ScreenId, () => _panel != null && _panel.IsOpen, OpenPanel, () => _panel?.Close());
            }
        }

        private void OpenPanel()
        {
            if (_panel != null && !_panel.IsOpen && !Rapadura.Core.Systems.GameplayPause.IsPaused)
            {
                _panel.Open(FindFirstObjectByType<WorldMapController>());
            }
        }
    }
}
