using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Fishing;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.Fishing
{
    /// <summary>
    /// "Pescar" on the world map, in the regions with water (açude in the Floresta, riacho in the Montanha, lago
    /// subterrâneo in the Cavernas). A small, separate entry, self-bootstrapping like <see cref="CarameloCuddleButton"/>
    /// (no scene edit, no change to the village hub / city panels): added to the <see cref="WorldMapHudController"/>'s
    /// GameObject after every scene load, it injects one button into the HUD's <c>wm-action-row</c>.
    /// <list type="bullet">
    /// <item>Shown while <see cref="FishingPresenter.ShouldShowButton"/>: HUD visible, the token idle on a region with a
    /// spot (<see cref="FishCatalog.SpotFor"/>), nothing holding GameplayPause, no conversation.</item>
    /// <item>Click: opens <see cref="FishingPanelController"/> (built in code on the HUD's document) for that spot; it
    /// joins the HUD's screens (<see cref="WorldMapHudController.RegisterScreen"/>), so Esc closes it.</item>
    /// </list>
    /// </summary>
    [DisallowMultipleComponent]
    public class FishingSpotButton : MonoBehaviour
    {
        public const string ButtonName = "wm-fishing-button";
        public const string ScreenId = "fishing";

        private const float RefreshInterval = 0.2f;

        private WorldMapHudController _hud;
        private UIDocument _hudDocument;
        private WorldMapController _worldMap;
        private FishingPanelController _panel;
        private Button _button;
        private bool _screenRegistered;
        private float _nextRefresh;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            AttachToHud();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => AttachToHud();

        private static void AttachToHud()
        {
            WorldMapHudController hud = FindFirstObjectByType<WorldMapHudController>(FindObjectsInactive.Include);
            if (hud != null && hud.GetComponent<FishingSpotButton>() == null)
            {
                hud.gameObject.AddComponent<FishingSpotButton>();
            }
        }

        private void Awake()
        {
            _hud = GetComponent<WorldMapHudController>();
            _hudDocument = GetComponent<UIDocument>();
        }

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.clicked -= OnClicked;
                _button.RemoveFromHierarchy();
                _button = null;
            }
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + RefreshInterval;
            Refresh();
        }

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        private BiomeType? CurrentRegion()
        {
            if (_worldMap == null)
            {
                _worldMap = FindFirstObjectByType<WorldMapController>();
            }

            return _worldMap != null && _worldMap.CurrentNode != null ? _worldMap.CurrentNode.Region : (BiomeType?)null;
        }

        private void Refresh()
        {
            if (!EnsureButton())
            {
                return;
            }

            RegisterScreen();
            ServiceLocator.TryGet(out DialogueManager dialogue);
            bool show = FishingPresenter.ShouldShowButton(_hud != null && _hud.IsVisible, _worldMap != null && _worldMap.CanExplore,
                GameplayPause.IsPaused, dialogue != null && dialogue.IsActive, CurrentRegion());
            _button.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (show)
            {
                string label = FishingTexts.Get(Localization, FishingTexts.Button);
                if (_button.text != label)
                {
                    _button.text = label;
                }
            }
        }

        private void RegisterScreen()
        {
            if (_screenRegistered || _hud == null)
            {
                return;
            }

            _hud.RegisterScreen(ScreenId, () => _panel != null && _panel.IsOpen, OpenPanel, () => { if (_panel != null) _panel.Close(); });
            _screenRegistered = true;
        }

        private bool EnsureButton()
        {
            if (_hudDocument == null || _hudDocument.rootVisualElement == null)
            {
                return false;
            }

            VisualElement row = _hudDocument.rootVisualElement.Q<VisualElement>(RegionNpcTalkButton.ActionRowName);
            if (row == null)
            {
                return false;
            }

            if (_button != null && _button.parent == row)
            {
                return true;
            }

            if (_button != null)
            {
                _button.clicked -= OnClicked;
                _button.RemoveFromHierarchy();
            }

            _button = new Button { name = ButtonName, focusable = true };
            // Same look as the "Conversar" button next to it.
            _button.AddToClassList("wm-explore-button");
            _button.AddToClassList(RegionNpcTalkButton.ButtonClass);
            _button.style.marginLeft = 8f;
            _button.style.display = DisplayStyle.None;
            _button.clicked += OnClicked;
            row.Add(_button);
            return true;
        }

        private void OnClicked()
        {
            if (GameplayPause.IsPaused || (ServiceLocator.TryGet(out DialogueManager dialogue) && dialogue.IsActive))
            {
                return;
            }

            OpenPanel();
        }

        private void OpenPanel()
        {
            FishingSpotSpec spot = FishCatalog.SpotFor(CurrentRegion());
            if (spot == null)
            {
                return;
            }

            // Built in code on this HUD's document (no UXML), so it lives on the HUD's GameObject.
            _panel = _panel != null ? _panel : (GetComponent<FishingPanelController>() ?? gameObject.AddComponent<FishingPanelController>());
            _panel.Open(spot);
        }
    }
}
