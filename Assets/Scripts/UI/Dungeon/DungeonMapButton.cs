using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Dungeon;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.Dungeon
{
    /// <summary>
    /// The way into the Ossuário dos Artífices from the world map, self-bootstrapping like <c>EventRoomMapButton</c> (no
    /// scene edit): added to the <see cref="WorldMapHudController"/>'s GameObject after every scene load, it puts one
    /// "Entrar: Ossuário dos Artífices" button in the HUD's <c>wm-action-row</c> while the token stands in the dungeon's
    /// region (<see cref="DungeonRules.Region"/>, the Grutas do Limiar). Click: checks the hero level (carried PlayerStats,
    /// <see cref="DungeonRules.MinLevel"/>) and starts the run through <see cref="WorldMapController.TryStartEventRoom"/>
    /// with <see cref="DungeonDirector.BuildPayload"/> (a directed encounter, like the event rooms). No entry item: the
    /// keys are found inside. A blocked entry shows a short message over the row.
    /// </summary>
    [DisallowMultipleComponent]
    public class DungeonMapButton : MonoBehaviour
    {
        public const string ButtonName = "wm-dungeon-ossuario";
        public const string MessageName = "dg-map-message";
        private const float RefreshInterval = 0.25f;
        private const float MessageSeconds = 3.5f;
        private const string PlayerStatsSaveKey = "PlayerStats";

        private WorldMapHudController _hud;
        private UIDocument _hudDocument;
        private WorldMapController _worldMap;
        private Button _button;
        private Label _message;
        private float _messageLeft;
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
            if (hud != null && hud.GetComponent<DungeonMapButton>() == null)
            {
                hud.gameObject.AddComponent<DungeonMapButton>();
            }
        }

        private void Awake()
        {
            _hud = GetComponent<WorldMapHudController>();
            _hudDocument = GetComponent<UIDocument>();
        }

        private void OnDisable()
        {
            _button?.RemoveFromHierarchy();
            _button = null;
            _message?.RemoveFromHierarchy();
            _message = null;
        }

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        // ------------------------------------------------------------------
        // Pure helpers (tested)
        // ------------------------------------------------------------------

        /// <summary>Whether the button shows: the token stands in the dungeon's region.</summary>
        public static bool ShowsIn(BiomeType? region) => region == DungeonRules.Region;

        /// <summary>The button's text ("Entrar: Ossuário dos Artífices").</summary>
        public static string ButtonText(LocalizationManager localization)
        {
            return DungeonTexts.Format(localization, DungeonTexts.MapEnter, DungeonTexts.Get(localization, DungeonTexts.DungeonName));
        }

        /// <summary>Why the hero cannot go in now (empty when they can).</summary>
        public static string BlockMessage(LocalizationManager localization, int heroLevel, BiomeType? region)
        {
            if (DungeonRules.CanEnter(heroLevel, region) || region != DungeonRules.Region)
            {
                return string.Empty;
            }

            return DungeonTexts.Format(localization, DungeonTexts.MapNeedLevel, DungeonTexts.Get(localization, DungeonTexts.DungeonName), DungeonRules.MinLevel);
        }

        // ------------------------------------------------------------------
        // View
        // ------------------------------------------------------------------

        private void Update()
        {
            if (_messageLeft > 0f)
            {
                _messageLeft -= Time.unscaledDeltaTime;
            }

            if (Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + RefreshInterval;
            Refresh();
        }

        private void Refresh()
        {
            if (!EnsureView())
            {
                return;
            }

            if (_worldMap == null)
            {
                _worldMap = FindFirstObjectByType<WorldMapController>();
            }

            bool hudVisible = _hud != null && _hud.IsVisible;
            ServiceLocator.TryGet(out DialogueManager dialogue);
            bool canAct = hudVisible && _worldMap != null && _worldMap.CanExplore && !GameplayPause.IsPaused && (dialogue == null || !dialogue.IsActive);
            bool show = canAct && ShowsIn(_worldMap.CurrentRegion);
            _button.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (show)
            {
                string text = ButtonText(Localization);
                if (_button.text != text)
                {
                    _button.text = text;
                }
            }

            _message.style.display = hudVisible && _messageLeft > 0f ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private bool EnsureView()
        {
            if (_hudDocument == null || _hudDocument.rootVisualElement == null)
            {
                return false;
            }

            VisualElement root = _hudDocument.rootVisualElement;
            VisualElement row = root.Q<VisualElement>(RegionNpcTalkButton.ActionRowName);
            if (row == null)
            {
                return false;
            }

            if (_message == null || _message.panel == null)
            {
                _message?.RemoveFromHierarchy();
                _message = new Label { name = MessageName, pickingMode = PickingMode.Ignore };
                _message.AddToClassList("dg-map-message");
                DungeonStyles.ApplyTo(_message);
                _message.style.display = DisplayStyle.None;
                root.Add(_message);
            }

            if (_button == null || _button.parent != row)
            {
                _button?.RemoveFromHierarchy();
                _button = new Button(OnClicked) { name = ButtonName, focusable = true };
                // Same look as the "Conversar" button next to it.
                _button.AddToClassList("wm-explore-button");
                _button.AddToClassList(RegionNpcTalkButton.ButtonClass);
                _button.AddToClassList("dg-map-button");
                DungeonStyles.ApplyTo(_button);
                _button.style.marginLeft = 8f;
                _button.style.display = DisplayStyle.None;
                row.Add(_button);
            }

            return true;
        }

        private void ShowMessage(string text)
        {
            if (_message == null || string.IsNullOrEmpty(text))
            {
                return;
            }

            _message.text = text;
            _messageLeft = MessageSeconds;
            _message.style.display = DisplayStyle.Flex;
        }

        // ------------------------------------------------------------------
        // Entry
        // ------------------------------------------------------------------

        private void OnClicked()
        {
            if (GameplayPause.IsPaused || (ServiceLocator.TryGet(out DialogueManager dialogue) && dialogue.IsActive))
            {
                return;
            }

            if (_worldMap == null)
            {
                _worldMap = FindFirstObjectByType<WorldMapController>();
            }

            if (_worldMap == null || !_worldMap.CanExplore)
            {
                return;
            }

            BiomeType? region = _worldMap.CurrentRegion;
            int level = HeroLevel();
            if (!DungeonRules.CanEnter(level, region))
            {
                ShowMessage(BlockMessage(Localization, level, region));
                return;
            }

            EncounterPayload payload = DungeonDirector.BuildPayload(System.Environment.TickCount);
            if (payload == null || !_worldMap.TryStartEventRoom(payload))
            {
                Debug.LogWarning("[DungeonMapButton] The ossuary could not start (missing enemy assets or the map is busy).");
            }
        }

        private static int HeroLevel()
        {
            string json = null;
            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.TryGetCarriedJson(PlayerStatsSaveKey, out json);
            }

            return WorldMapHudPresenter.ParseLevel(json);
        }
    }

    /// <summary>Loads Resources/UI/Dungeon.uss (cached) and attaches it with the theme class.</summary>
    public static class DungeonStyles
    {
        public const string ResourcePath = "UI/Dungeon";
        public const string ThemeClass = "rap-theme";

        private static StyleSheet _sheet;

        public static StyleSheet Sheet
        {
            get
            {
                if (_sheet == null)
                {
                    _sheet = Resources.Load<StyleSheet>(ResourcePath);
                }

                return _sheet;
            }
        }

        public static void ApplyTo(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            StyleSheet sheet = Sheet;
            if (sheet != null && !root.styleSheets.Contains(sheet))
            {
                root.styleSheets.Add(sheet);
            }

            root.AddToClassList(ThemeClass);
        }
    }
}
