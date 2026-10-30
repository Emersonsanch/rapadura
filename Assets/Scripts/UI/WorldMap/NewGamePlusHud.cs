using System.Globalization;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// "Nova Jornada+" on the world map. Self-bootstrapping like <see cref="VillageHubButton"/>, no scene edit:
    /// <list type="bullet">
    /// <item>Added to the <see cref="WorldMapHudController"/>'s GameObject after every scene load.</item>
    /// <item>Badge: a small "Jornada+N" label under the hero's level (<c>wm-hero-block</c>) while the save is on
    /// a Nova Jornada+ (<see cref="NewGamePlusProgress.Cycle"/> 1+).</item>
    /// <item>Offer: a "Nova Jornada+" button in the HUD's <c>wm-action-row</c>, shown in the village region (Vó
    /// Zefa's) once the campaign is complete and its ending was shown (<see cref="NewGamePlusRules.CanOffer"/>).
    /// It opens Vó Zefa's confirmation panel under <c>wm-hud-root</c> (one of the HUD's screens: Esc closes it):
    /// her line for the current hero, what is kept, what starts over and the next cycle's enemy multipliers.</item>
    /// <item>"Partir de novo": <see cref="NewGamePlusProgress.BeginNextCycle"/>, auto-save, then the world map is
    /// reloaded so the token, regions and bosses start fresh; the new-journey prologue plays there
    /// (<c>StoryPanelController</c>). The hero's level/gear/gold ride along in SaveManager's carried state.</item>
    /// </list>
    /// </summary>
    public class NewGamePlusHud : MonoBehaviour
    {
        public const string ButtonName = "wm-ngplus-button";
        public const string BadgeName = "wm-ngplus-badge";
        public const string OverlayName = "wm-ngplus-overlay";
        public const string ScreenId = "new_journey";

        private const string HeroBlockName = "wm-hero-block";
        private const float RefreshInterval = 0.25f;
        private static readonly Color BadgeColor = new Color(1f, 0.78f, 0.3f);

        private WorldMapHudController _hud;
        private UIDocument _hudDocument;
        private WorldMapController _worldMap;
        private Button _button;
        private Label _badge;
        private VisualElement _overlay;
        private Label _titleLabel;
        private Label _speechLabel;
        private Label _keepsLabel;
        private Label _resetsLabel;
        private Button _startButton;
        private Button _cancelButton;
        private bool _open;
        private bool _screenRegistered;
        private bool _starting;
        private float _nextRefresh;

        public bool IsOpen => _open;

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
            if (hud != null && hud.GetComponent<NewGamePlusHud>() == null)
            {
                hud.gameObject.AddComponent<NewGamePlusHud>();
            }
        }

        private void Awake()
        {
            _hud = GetComponent<WorldMapHudController>();
            _hudDocument = GetComponent<UIDocument>();
        }

        private void OnDisable()
        {
            Close();

            if (_button != null)
            {
                _button.clicked -= OnButtonClicked;
                _button.RemoveFromHierarchy();
                _button = null;
            }

            _badge?.RemoveFromHierarchy();
            _badge = null;
            _overlay?.RemoveFromHierarchy();
            _overlay = null;
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
                _hud.RegisterScreen(ScreenId, () => _open, Open, Close);
                _screenRegistered = true;
            }

            Refresh();
        }

        // ------------------------------------------------------------------
        // Pure rules (tested)
        // ------------------------------------------------------------------

        /// <summary>Whether the "Nova Jornada+" button should be on screen (same gating as the "Vila" button, plus the offer).</summary>
        public static bool ShouldShowOffer(bool canBegin, bool hudVisible, bool tokenIdle, bool paused, bool dialogueActive, BiomeType? currentRegion)
        {
            return canBegin && VillageHubButton.ShouldShow(hudVisible, tokenIdle, paused, dialogueActive, currentRegion);
        }

        /// <summary>"Jornada+2" for a Nova Jornada+ cycle; null on the first campaign (no badge).</summary>
        public static string BadgeText(LocalizationManager localization, int cycle)
        {
            int clamped = NewGamePlusRules.ClampCycle(cycle);
            return clamped > 0 ? NewGamePlusTexts.Format(localization, NewGamePlusTexts.Badge, clamped) : null;
        }

        /// <summary>The "what changes" line for the cycle about to start (multipliers like "1,8" in pt-BR, "1.8" in English).</summary>
        public static string ResetsText(LocalizationManager localization, int nextCycle)
        {
            bool english = localization != null && localization.CurrentLanguage == LanguageCode.en;
            return NewGamePlusTexts.Format(localization, NewGamePlusTexts.Resets,
                Multiplier(NewGamePlusRules.HealthMultiplier(nextCycle), english),
                Multiplier(NewGamePlusRules.DamageMultiplier(nextCycle), english),
                Multiplier(NewGamePlusRules.ExperienceMultiplier(nextCycle), english));
        }

        // No pt-BR CultureInfo lookup (not every player build ships culture data): invariant + decimal comma.
        private static string Multiplier(float value, bool english)
        {
            string text = value.ToString("0.#", CultureInfo.InvariantCulture);
            return english ? text : text.Replace('.', ',');
        }

        /// <summary>
        /// Shows <paramref name="cycle"/>'s badge as a small label inside <paramref name="host"/> (created on first use,
        /// hidden on the first campaign). Shared with the title screen's "Continuar" (MainMenuController), where
        /// <paramref name="corner"/> pins it to the button's top-right corner instead of flowing under the text.
        /// </summary>
        public static Label ApplyBadge(VisualElement host, Label badge, int cycle, LocalizationManager localization, bool corner = false)
        {
            string text = BadgeText(localization, cycle);
            if (host == null)
            {
                return badge;
            }

            if (badge == null || badge.parent != host)
            {
                badge?.RemoveFromHierarchy();
                badge = new Label { name = BadgeName, pickingMode = PickingMode.Ignore };
                badge.AddToClassList(BadgeName);
                badge.style.color = BadgeColor;
                badge.style.fontSize = 12f;
                badge.style.unityFontStyleAndWeight = FontStyle.Bold;
                if (corner)
                {
                    badge.style.position = Position.Absolute;
                    badge.style.top = 2f;
                    badge.style.right = 8f;
                    badge.style.fontSize = 11f;
                }

                host.Add(badge);
            }

            badge.style.display = text != null ? DisplayStyle.Flex : DisplayStyle.None;
            if (text != null && badge.text != text)
            {
                badge.text = text;
            }

            return badge;
        }

        // ------------------------------------------------------------------
        // Runtime
        // ------------------------------------------------------------------

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        private static NewGamePlusProgress Progress => ServiceLocator.TryGet(out NewGamePlusProgress progress) ? progress : null;

        private void Refresh()
        {
            if (_hudDocument == null || _hudDocument.rootVisualElement == null)
            {
                return;
            }

            NewGamePlusProgress progress = Progress;
            VisualElement root = _hudDocument.rootVisualElement;
            _badge = ApplyBadge(root.Q<VisualElement>(HeroBlockName), _badge, progress != null ? progress.Cycle : 0, Localization);

            if (_worldMap == null)
            {
                _worldMap = FindFirstObjectByType<WorldMapController>();
            }

            bool hudVisible = _hud != null && _hud.IsVisible;
            if (_open && (!hudVisible || progress == null || !progress.CanBeginNextCycle))
            {
                Close();
            }

            if (!EnsureButton(root))
            {
                return;
            }

            ServiceLocator.TryGet(out DialogueManager dialogue);
            BiomeType? region = _worldMap != null && _worldMap.CurrentNode != null ? _worldMap.CurrentNode.Region : (BiomeType?)null;
            bool show = !_starting && ShouldShowOffer(progress != null && progress.CanBeginNextCycle, hudVisible,
                _worldMap != null && _worldMap.CanExplore, GameplayPause.IsPaused, dialogue != null && dialogue.IsActive, region);
            _button.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;

            string label = NewGamePlusTexts.Get(Localization, NewGamePlusTexts.Button);
            if (_button.text != label)
            {
                _button.text = label;
            }
        }

        private bool EnsureButton(VisualElement root)
        {
            VisualElement row = root.Q<VisualElement>(RegionNpcTalkButton.ActionRowName);
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
                _button.clicked -= OnButtonClicked;
                _button.RemoveFromHierarchy();
            }

            _button = new Button { name = ButtonName, focusable = true };
            _button.AddToClassList("wm-explore-button");
            // Same themed colours as the "Vila" button (.wm-village-button in WorldMapHudView.uss).
            _button.AddToClassList(VillageHubButton.ButtonName);
            _button.style.marginLeft = 8f;
            _button.style.display = DisplayStyle.None;
            _button.clicked += OnButtonClicked;
            row.Add(_button);
            return true;
        }

        private void OnButtonClicked()
        {
            if (GameplayPause.IsPaused || (ServiceLocator.TryGet(out DialogueManager dialogue) && dialogue.IsActive))
            {
                return;
            }

            Open();
        }

        // ------------------------------------------------------------------
        // Confirmation panel
        // ------------------------------------------------------------------

        public void Open()
        {
            NewGamePlusProgress progress = Progress;
            if (_open || _starting || progress == null || !progress.CanBeginNextCycle || !EnsureOverlay())
            {
                return;
            }

            _open = true;
            RefreshTexts(progress);
            _overlay.style.display = DisplayStyle.Flex;
            GameplayPause.Acquire(this);
            _cancelButton?.Focus();
        }

        public void Close()
        {
            if (_overlay != null)
            {
                _overlay.style.display = DisplayStyle.None;
            }

            _open = false;
            GameplayPause.Release(this);
        }

        private bool EnsureOverlay()
        {
            if (_hudDocument == null || _hudDocument.rootVisualElement == null)
            {
                return false;
            }

            VisualElement root = _hudDocument.rootVisualElement.Q<VisualElement>("wm-hud-root") ?? _hudDocument.rootVisualElement;
            if (_overlay != null && _overlay.parent == root)
            {
                return true;
            }

            _overlay?.RemoveFromHierarchy();

            // Same Rapadura-theme panel as the village hub (WorldMapHudView.uss, under the HUD root's rap-theme).
            _overlay = new VisualElement { name = OverlayName };
            _overlay.AddToClassList("wm-backpack-overlay");
            _overlay.style.display = DisplayStyle.None;

            var panel = new VisualElement { name = "wm-ngplus-panel" };
            panel.AddToClassList("rap-panel");
            panel.AddToClassList("wm-backpack-panel");
            _overlay.Add(panel);

            _titleLabel = AddLabel(panel, "wm-backpack-title");
            _speechLabel = AddLabel(panel, "wm-backpack-hint");
            _keepsLabel = AddLabel(panel, "wm-backpack-hero");
            _resetsLabel = AddLabel(panel, "wm-backpack-hero");

            var buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.style.justifyContent = Justify.SpaceBetween;
            buttons.style.marginTop = 8f;
            panel.Add(buttons);

            _cancelButton = new Button(Close) { name = "wm-ngplus-cancel-button", focusable = true };
            _cancelButton.AddToClassList("rap-button");
            _cancelButton.AddToClassList("wm-backpack-close");
            buttons.Add(_cancelButton);

            _startButton = new Button(OnStartClicked) { name = "wm-ngplus-start-button", focusable = true };
            _startButton.AddToClassList("rap-button");
            _startButton.AddToClassList("rap-button--primary");
            _startButton.AddToClassList("wm-backpack-close");
            buttons.Add(_startButton);

            root.Add(_overlay);
            return true;
        }

        private static Label AddLabel(VisualElement parent, string className)
        {
            var label = new Label();
            label.AddToClassList(className);
            label.style.whiteSpace = WhiteSpace.Normal;
            parent.Add(label);
            return label;
        }

        private void RefreshTexts(NewGamePlusProgress progress)
        {
            LocalizationManager localization = Localization;
            ServiceLocator.TryGet(out CharacterSelectionState selection);
            CharacterId? hero = selection != null ? selection.Selected : null;
            int next = progress.NextCycle;

            _titleLabel.text = NewGamePlusTexts.Format(localization, NewGamePlusTexts.Title, next);
            _speechLabel.text = NewGamePlusTexts.Get(localization, NewGamePlusTexts.ZefaKey(hero));
            _keepsLabel.text = NewGamePlusTexts.Get(localization, NewGamePlusTexts.Keeps);
            _resetsLabel.text = next == progress.Cycle
                ? NewGamePlusTexts.Get(localization, NewGamePlusTexts.MaxCycle)
                : ResetsText(localization, next);
            _startButton.text = NewGamePlusTexts.Get(localization, NewGamePlusTexts.Start);
            _cancelButton.text = NewGamePlusTexts.Get(localization, NewGamePlusTexts.Cancel);
        }

        private void OnStartClicked()
        {
            NewGamePlusProgress progress = Progress;
            if (_starting || progress == null || !progress.BeginNextCycle())
            {
                Close();
                return;
            }

            _starting = true;
            Close();

            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.AutoSave();
            }

            // Fresh map: token back in the Floresta, regions locked, bosses back (WorldMapController reads it all on Start).
            if (ServiceLocator.TryGet(out SceneTransitionService transitions))
            {
                transitions.ReturnToWorldMap();
            }
            else
            {
                _starting = false;
                _worldMap?.SyncWithProgress();
            }
        }
    }
}
