using Rapadura.Audio;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Pets;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// "Fazer carinho" on the world map, in the village region (Vó Zefa's farm, where Caramelo lives
    /// between trips). Self-bootstrapping like <see cref="VillageHubButton"/>, no scene edit: added to the
    /// <see cref="WorldMapHudController"/>'s GameObject after every scene load, it injects one button into
    /// the HUD's <c>wm-action-row</c> and a small themed note under <c>wm-hud-root</c> (picking ignored).
    /// <list type="bullet">
    /// <item>Shown while the HUD is visible, the token is idle in the village, nothing holds GameplayPause, no
    /// conversation runs, and <see cref="CompanionPreferences.ShouldOfferCuddle"/> (option on, prologue seen).
    /// Until Vó Zefa introduced him it reads "Caramelo (!)".</item>
    /// <item>Click: <see cref="CarameloCuddle.Cuddle"/>: the first time Vó Zefa's introduction (the village beat
    /// explaining why he goes with the cousins), then the current cousin's lines in turn; plus the tiny buff
    /// note (+5% speed in the next encounter) and his happy <c>pet.yip</c>.</item>
    /// </list>
    /// </summary>
    [DisallowMultipleComponent]
    public class CarameloCuddleButton : MonoBehaviour
    {
        public const string ButtonName = "wm-caramelo-button";
        public const string NoteName = "wm-caramelo-note";
        public const float NoteSeconds = 5f;

        private const float RefreshInterval = 0.2f;

        private WorldMapHudController _hud;
        private UIDocument _hudDocument;
        private WorldMapController _worldMap;
        private Button _button;
        private VisualElement _note;
        private Label _noteLabel;
        private float _noteHideAt = -1f;
        private float _nextRefresh;

        // ------------------------------------------------------------------
        // Bootstrap
        // ------------------------------------------------------------------

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
            if (hud != null && hud.GetComponent<CarameloCuddleButton>() == null)
            {
                hud.gameObject.AddComponent<CarameloCuddleButton>();
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

            _note?.RemoveFromHierarchy();
            _note = null;
        }

        private void Update()
        {
            if (_note != null && _noteHideAt > 0f && (Time.unscaledTime >= _noteHideAt || GameplayPause.IsPaused))
            {
                _note.style.display = DisplayStyle.None;
                _noteHideAt = -1f;
            }

            if (Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + RefreshInterval;
            Refresh();
        }

        // ------------------------------------------------------------------
        // Pure rule (tested)
        // ------------------------------------------------------------------

        /// <summary>Whether the "Fazer carinho" button should be on screen.</summary>
        public static bool ShouldShow(bool hudVisible, bool tokenIdle, bool paused, bool dialogueActive, BiomeType? currentRegion, bool offerCuddle)
        {
            return offerCuddle && hudVisible && tokenIdle && !paused && !dialogueActive && VillageNpcCatalog.IsVillageRegion(currentRegion);
        }

        // ------------------------------------------------------------------
        // Runtime
        // ------------------------------------------------------------------

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        private static bool PrologueSeen()
        {
            return ServiceLocator.TryGet(out StoryProgress story) && story != null
                   && (story.HasSeen(StoryBeat.Prologue) || story.CampaignComplete);
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

            ServiceLocator.TryGet(out DialogueManager dialogue);
            BiomeType? region = _worldMap != null && _worldMap.CurrentNode != null ? _worldMap.CurrentNode.Region : (BiomeType?)null;
            bool show = ShouldShow(_hud != null && _hud.IsVisible, _worldMap != null && _worldMap.CanExplore, GameplayPause.IsPaused,
                dialogue != null && dialogue.IsActive, region,
                CompanionPreferences.ShouldOfferCuddle(CompanionPreferences.CarameloEnabled, PrologueSeen()));
            _button.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (!show)
            {
                return;
            }

            string label = CarameloTexts.Get(Localization, CompanionPreferences.Introduced ? CarameloTexts.CuddleButton : CarameloTexts.CuddleButtonNew);
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

            // First time, or the HUD document rebuilt its tree (re-enable): (re)inject.
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

            ServiceLocator.TryGet(out CharacterSelectionState selection);
            bool introduced = CompanionPreferences.Introduced;
            BiomeType? cameFrom = ServiceLocator.TryGet(out StoryProgress story) && story != null ? StoryBeats.LastRegionCleared(story.HasSeen) : null;
            string lineKey = CarameloCuddle.Cuddle(selection != null ? selection.Selected : null, introduced, cameFrom);
            if (!introduced)
            {
                CompanionPreferences.Introduced = true;
            }

            LocalizationManager localization = Localization;
            ShowNote(CarameloTexts.Get(localization, lineKey) + "\n" + CarameloTexts.Get(localization, CarameloTexts.BuffNote));

            if (ServiceLocator.TryGet(out SfxEventRouter router) && router != null && router.IsInitialized)
            {
                router.PlayCue(CarameloCompanion.YipCue);
            }
        }

        private void ShowNote(string text)
        {
            if (!EnsureNote())
            {
                return;
            }

            _noteLabel.text = text;
            _note.style.display = DisplayStyle.Flex;
            _noteHideAt = Time.unscaledTime + NoteSeconds;
        }

        private bool EnsureNote()
        {
            if (_hudDocument == null || _hudDocument.rootVisualElement == null)
            {
                return false;
            }

            VisualElement root = _hudDocument.rootVisualElement.Q<VisualElement>("wm-hud-root") ?? _hudDocument.rootVisualElement;
            if (_note != null && _note.parent == root)
            {
                return true;
            }

            _note?.RemoveFromHierarchy();

            // Themed like the HUD's panels (rap-panel under the HUD root's rap-theme); only the placement is inline.
            _note = new VisualElement { name = NoteName, pickingMode = PickingMode.Ignore };
            _note.AddToClassList("rap-panel");
            _note.style.position = Position.Absolute;
            _note.style.left = new Length(18f, LengthUnit.Percent);
            _note.style.right = new Length(18f, LengthUnit.Percent);
            _note.style.bottom = new Length(22f, LengthUnit.Percent);
            _note.style.display = DisplayStyle.None;

            _noteLabel = new Label { pickingMode = PickingMode.Ignore };
            _noteLabel.AddToClassList("wm-backpack-hint");
            _noteLabel.style.whiteSpace = WhiteSpace.Normal;
            _noteLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _note.Add(_noteLabel);

            root.Add(_note);
            return true;
        }
    }
}
