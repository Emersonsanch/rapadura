using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Seasons;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.Seasons
{
    /// <summary>
    /// "Arraiá" on the world map during the Festa Junina: a small, separate entry, self-bootstrapping like
    /// <c>FishingSpotButton</c> (no scene edit, no change to the village hub): added to the
    /// <see cref="WorldMapHudController"/>'s GameObject after every scene load, it injects one button into the HUD's
    /// <c>wm-action-row</c>, shown while <see cref="FestaJuninaPresenter.ShouldShowButton"/> (festa on, the token idle
    /// inside the Vila da Vó Edna, nothing modal). Click: opens <see cref="FestaJuninaPanel"/>; it joins the HUD's
    /// screens (<see cref="WorldMapHudController.RegisterScreen"/>), so Esc closes it.
    /// </summary>
    [DisallowMultipleComponent]
    public class FestaJuninaButton : MonoBehaviour
    {
        public const string ButtonName = "wm-festa-button";
        public const string ScreenId = "festa_junina";

        private const float RefreshInterval = 0.25f;

        private WorldMapHudController _hud;
        private UIDocument _hudDocument;
        private WorldMapController _worldMap;
        private FestaJuninaPanel _panel;
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
            if (hud != null && hud.GetComponent<FestaJuninaButton>() == null)
            {
                hud.gameObject.AddComponent<FestaJuninaButton>();
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
            bool active = SeasonCalendar.IsFestaJuninaActive;
            if (!active && _button == null)
            {
                return;
            }

            if (!EnsureButton())
            {
                return;
            }

            RegisterScreen();
            ServiceLocator.TryGet(out DialogueManager dialogue);
            BiomeType? region = CurrentRegion();
            bool inSafeZone = WorldMapCityPresence.Current == null || WorldMapCityPresence.Current.TokenInSafeZone;
            bool show = FestaJuninaPresenter.ShouldShowButton(active, _hud != null && _hud.IsVisible, _worldMap != null && _worldMap.CanExplore,
                GameplayPause.IsPaused, dialogue != null && dialogue.IsActive, region, inSafeZone);
            _button.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (show)
            {
                string label = FestaJuninaTexts.Get(Localization, FestaJuninaTexts.Button);
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
            if (!SeasonCalendar.IsFestaJuninaActive)
            {
                return;
            }

            _panel = _panel != null ? _panel : (GetComponent<FestaJuninaPanel>() ?? gameObject.AddComponent<FestaJuninaPanel>());
            _panel.Open();
        }
    }
}
