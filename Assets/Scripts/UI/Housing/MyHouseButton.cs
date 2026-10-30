using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.Housing
{
    /// <summary>
    /// "Minha casa" on the world map (Housing.md §1: the button shows when the token stands idle at the city node,
    /// like "Enfrentar o chefe"). A small, separate entry next to the village hub's "Vila" button — it does not
    /// touch the hub — self-bootstrapping like <see cref="VillageHubButton"/>, no scene edit:
    /// <list type="bullet">
    /// <item>Added to the <see cref="WorldMapHudController"/>'s GameObject after every scene load, with a
    /// <see cref="HouseScreenController"/>; injects one button into the HUD's <c>wm-action-row</c>.</item>
    /// <item>Shown while the HUD is visible, the token is idle in the Vila (<see cref="VillageNpcCatalog.VillageRegion"/>),
    /// nothing holds GameplayPause and the house is offered (<see cref="HouseRules.IsHouseOffered"/>: owned, or the
    /// quest beat of 3 Forest victories reached). "(!)" while "Um Cantinho Seu" has news.</item>
    /// <item>Joins the HUD's screens (<see cref="WorldMapHudController.RegisterScreen"/>), so Esc closes the house.</item>
    /// </list>
    /// </summary>
    [DisallowMultipleComponent]
    public class MyHouseButton : MonoBehaviour
    {
        public const string ButtonName = "wm-house-button";
        public const string ScreenId = "house";
        private const float RefreshInterval = 0.25f;

        private WorldMapHudController _hud;
        private UIDocument _hudDocument;
        private WorldMapController _worldMap;
        private HouseScreenController _screen;
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
            if (hud != null && hud.GetComponent<MyHouseButton>() == null)
            {
                hud.gameObject.AddComponent<MyHouseButton>();
            }
        }

        private void Awake()
        {
            _hud = GetComponent<WorldMapHudController>();
            _hudDocument = GetComponent<UIDocument>();
            _screen = GetComponent<HouseScreenController>();
            if (_screen == null)
            {
                _screen = gameObject.AddComponent<HouseScreenController>();
            }
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

        /// <summary>Whether "Minha casa" should be on screen (pure, tested).</summary>
        public static bool ShouldShow(bool hudVisible, bool tokenIdle, bool paused, bool dialogueActive, BiomeType? currentRegion, bool houseOffered)
        {
            return hudVisible && tokenIdle && !paused && !dialogueActive && houseOffered && VillageNpcCatalog.IsVillageRegion(currentRegion);
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + RefreshInterval;
            if (!_screenRegistered && _hud != null)
            {
                _hud.RegisterScreen(ScreenId, () => _screen != null && _screen.IsOpen, () => _screen?.OpenFromButton(), () => _screen?.HandleBack());
                _screenRegistered = true;
            }

            Refresh();
        }

        private void Refresh()
        {
            if (!EnsureButton())
            {
                return;
            }

            if (_worldMap == null)
            {
                _worldMap = FindFirstObjectByType<WorldMapController>();
            }

            HouseService house = HouseService.ResolveShared();
            bool dialogueActive = ServiceLocator.TryGet(out DialogueManager dialogue) && dialogue.IsActive;
            BiomeType? region = _worldMap != null && _worldMap.CurrentNode != null ? _worldMap.CurrentNode.Region : (BiomeType?)null;
            bool show = ShouldShow(_hud != null && _hud.IsVisible, _worldMap != null && _worldMap.CanExplore, GameplayPause.IsPaused, dialogueActive, region, house.IsOffered)
                && (_screen == null || !_screen.IsOpen);
            _button.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (!show)
            {
                return;
            }

            LocalizationManager localization = ServiceLocator.TryGet(out LocalizationManager l) ? l : null;
            string label = HouseTexts.Get(localization, house.HasQuestNews ? HouseTexts.ButtonNews : HouseTexts.Button);
            if (_button.text != label)
            {
                _button.text = label;
            }
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
            _button.AddToClassList(ButtonName);
            var sheet = Resources.Load<StyleSheet>(HouseScreenController.StyleResourcePath);
            if (sheet != null)
            {
                _button.styleSheets.Add(sheet);
            }

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

            _screen?.OpenFromButton();
        }
    }
}
