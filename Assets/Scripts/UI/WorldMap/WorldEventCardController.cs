using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Events;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// "Eventos aleatórios": the road event card of the world map. Self-bootstrapping like
    /// <see cref="VillageHubButton"/> (added to the <see cref="WorldMapHudController"/>'s GameObject after every scene
    /// load, overlay injected under <c>wm-hud-root</c>, so the HUD's pointer blocker covers it), no scene edit.
    /// <list type="bullet">
    /// <item>Each trip between regions is rolled once by <see cref="WorldEventDirector"/> (cooldown and pity saved in
    /// <see cref="WorldEventState"/>); a hit pops the card part-way, from <see cref="WorldMapController.TravelProgressed"/>.
    /// Never during the prologue/tutorial (<see cref="WorldEventRules.CanStartEvents"/>).</item>
    /// <item>The card holds GameplayPause (the token stops: timeScale 0) and shows the event, the current cousin's
    /// line and two choices, each with its effects spelled out; a choice the bag can't pay is disabled.</item>
    /// <item>Picking applies it (<see cref="WorldEventOutcomes.Apply"/>) on a live <see cref="CarriedInventorySession"/>
    /// (closed right after, so the carry has the result), moves the clock / slows the token, and shows the result.
    /// "Seguir viagem" resumes the trip; "Lutar!" starts the ambush/encounter through
    /// <see cref="WorldMapController.TryStartEventEncounter"/>.</item>
    /// </list>
    /// </summary>
    [DisallowMultipleComponent]
    public class WorldEventCardController : MonoBehaviour
    {
        public const string OverlayName = "wm-event-overlay";
        public const string PanelClass = "wm-event-panel";
        public const string ChoiceClass = "wm-event-choice";
        public const string ChoiceLabelClass = "wm-event-choice-label";
        public const string ChoiceFxClass = "wm-event-choice-fx";
        public const string HeroLineClass = "wm-event-hero-line";
        public const string ResultClass = "wm-event-result";
        public const string Separator = " · ";

        /// <summary>When set, seeds the trip/event rolls (reproducible PlayMode tests). Null in the game.</summary>
        public static int? SeedOverride { get; set; }

        private WorldMapHudController _hud;
        private UIDocument _hudDocument;
        private WorldMapController _worldMap;
        private WorldMapController _subscribedMap;
        private WorldEventDirector _director;

        private VisualElement _overlay;
        private Label _headerLabel;
        private Label _titleLabel;
        private Label _bodyLabel;
        private Label _heroLineLabel;
        private VisualElement _choicesRow;
        private readonly Button[] _choiceButtons = new Button[2];
        private readonly Label[] _choiceLabels = new Label[2];
        private readonly Label[] _choiceFxLabels = new Label[2];
        private Label _hintLabel;
        private VisualElement _resultBox;
        private Label _resultLabel;
        private Label _resultFxLabel;
        private Button _continueButton;

        private bool _tripActive;
        private WorldMapRegionNode _tripTarget;
        private float? _triggerAt;
        private WorldEventSpec _spec;
        private BiomeType _region;
        private WorldEventOutcome _outcome;
        private CarriedInventorySession _session;

        public bool IsOpen => _spec != null;
        public WorldEventSpec CurrentEvent => _spec;

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
            if (hud != null && hud.GetComponent<WorldEventCardController>() == null)
            {
                hud.gameObject.AddComponent<WorldEventCardController>();
            }
        }

        private void Awake()
        {
            _hud = GetComponent<WorldMapHudController>();
            _hudDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
            Unsubscribe();
            Cancel();

            if (_overlay != null)
            {
                _overlay.RemoveFromHierarchy();
                _overlay = null;
            }
        }

        private void Update()
        {
            if (_worldMap == null)
            {
                _worldMap = FindFirstObjectByType<WorldMapController>();
            }

            if (_worldMap != _subscribedMap)
            {
                Unsubscribe();
                if (_worldMap != null)
                {
                    _worldMap.TravelProgressed += OnTravelProgressed;
                    _subscribedMap = _worldMap;
                }
            }

            if (_worldMap == null)
            {
                return;
            }

            if (_worldMap.IsTraveling)
            {
                EnsureTripStarted();
            }
            else if (_tripActive)
            {
                // Arrived (or an encounter took over): the trip's luck and slowdown end with it.
                _tripActive = false;
                _triggerAt = null;
                _worldMap.TravelSpeedMultiplier = 1f;
            }

            if (IsOpen && (_hud == null || !_hud.IsVisible))
            {
                Cancel();
            }
        }

        private void Unsubscribe()
        {
            if (_subscribedMap != null)
            {
                _subscribedMap.TravelProgressed -= OnTravelProgressed;
            }

            _subscribedMap = null;
        }

        private void OnGameLoaded(GameLoadedEvent evt)
        {
            // A save loaded from the pause menu resets the token (SyncWithProgress): this trip and its card are gone.
            Cancel();
            _tripActive = false;
            _triggerAt = null;
            _director = null;
        }

        // ------------------------------------------------------------------
        // Trip rolls
        // ------------------------------------------------------------------

        private WorldEventDirector Director
        {
            get
            {
                if (_director == null)
                {
                    int seed = SeedOverride ?? WorldMapController.SeedOverride ?? System.Environment.TickCount;
                    _director = new WorldEventDirector(new WorldEventSettings(), WorldEventState.ResolveShared(), new System.Random(unchecked(seed * 17 + 3)));
                }

                return _director;
            }
        }

        private void EnsureTripStarted()
        {
            // Keyed on the destination too: a new trip started in the frame right after an arrival this
            // component had not seen yet is still a new trip.
            if (_tripActive && _worldMap != null && _tripTarget == _worldMap.TargetNode)
            {
                return;
            }

            _tripActive = true;
            _tripTarget = _worldMap != null ? _worldMap.TargetNode : null;
            if (_worldMap != null)
            {
                _worldMap.TravelSpeedMultiplier = 1f;
            }

            _triggerAt = Director.RollTrip(EventsAllowed());
        }

        private void OnTravelProgressed(float progress, BiomeType region)
        {
            EnsureTripStarted();
            if (!_triggerAt.HasValue || progress < _triggerAt.Value || IsOpen)
            {
                return;
            }

            _triggerAt = null;
            if (!EventsAllowed())
            {
                return;
            }

            TimeOfDayPhase phase = ServiceLocator.TryGet(out TimeOfDayManager time) && time != null ? time.Phase : TimeOfDayPhase.Day;
            WorldEventSpec spec = Director.PickEvent(region, phase);
            if (spec == null)
            {
                return;
            }

            Director.MarkTriggered(spec);
            Show(spec, region);
        }

        private bool EventsAllowed()
        {
            ServiceLocator.TryGet(out StoryProgress story);
            ServiceLocator.TryGet(out DialogueManager dialogue);
            return WorldEventRules.CanStartEvents(
                story == null || story.HasSeen(StoryBeat.Prologue),
                story != null && story.GetPendingBeat() != StoryBeat.None,
                TotalVictories(_worldMap != null ? _worldMap.Progress : null),
                Director.Settings.MinVictories,
                _hud != null && _hud.IsVisible,
                GameplayPause.IsPaused,
                dialogue != null && dialogue.IsActive);
        }

        public static int TotalVictories(WorldMapProgress progress)
        {
            if (progress == null)
            {
                return 0;
            }

            int total = 0;
            foreach (BiomeType region in System.Enum.GetValues(typeof(BiomeType)))
            {
                total += progress.GetVictories(region);
            }

            return total;
        }

        // ------------------------------------------------------------------
        // Pure text helpers (tested)
        // ------------------------------------------------------------------

        /// <summary>"Maria: "..."", the current cousin's reaction; empty without a hero.</summary>
        public static string BuildHeroLine(WorldEventSpec spec, CharacterId? hero, LocalizationManager localization)
        {
            if (spec == null || !hero.HasValue)
            {
                return string.Empty;
            }

            string name = WorldMapHudPresenter.HeroName(localization, hero);
            return WorldEventTexts.Format(localization, WorldEventTexts.HeroLine, name, WorldEventTexts.Get(localization, spec.LineKey(hero.Value)));
        }

        /// <summary>What a choice will do, before picking it: "-18 Ouro · +2 Poção de Vida".</summary>
        public static string DescribeChoice(WorldEventChoiceSpec choice, BiomeType region, LocalizationManager localization)
        {
            if (choice == null || choice.Effects.Count == 0)
            {
                return WorldEventTexts.Get(localization, WorldEventTexts.NoEffect);
            }

            var parts = new List<string>();
            foreach (WorldEventEffect effect in choice.Effects)
            {
                parts.Add(DescribeEffect(effect, WorldEventRules.ScaledAmount(effect, region), localization));
            }

            return string.Join(Separator, parts);
        }

        /// <summary>What a picked choice did (amounts as granted).</summary>
        public static string DescribeOutcome(WorldEventOutcome outcome, LocalizationManager localization)
        {
            if (outcome == null || outcome.Results.Count == 0)
            {
                return WorldEventTexts.Get(localization, WorldEventTexts.NoEffect);
            }

            var parts = new List<string>();
            foreach (WorldEventEffectResult result in outcome.Results)
            {
                parts.Add(DescribeEffect(result.Effect, result.Amount, localization));
            }

            return string.Join(Separator, parts);
        }

        /// <param name="amount">Item/XP amount for those kinds (already region-scaled); ignored by the others.</param>
        public static string DescribeEffect(WorldEventEffect effect, int amount, LocalizationManager localization)
        {
            if (effect == null)
            {
                return string.Empty;
            }

            switch (effect.Kind)
            {
                case WorldEventEffectKind.GainItem:
                    return WorldEventTexts.Format(localization, WorldEventTexts.FxGain, amount, ItemName(effect.ItemId, localization));
                case WorldEventEffectKind.LoseItem:
                    return WorldEventTexts.Format(localization, WorldEventTexts.FxPay, amount, ItemName(effect.ItemId, localization));
                case WorldEventEffectKind.NextFightExperience:
                    return WorldEventTexts.Format(localization, WorldEventTexts.FxExperience, amount);
                case WorldEventEffectKind.NextFightHeal:
                    return WorldEventTexts.Format(localization, WorldEventTexts.FxHeal, Mathf.RoundToInt(effect.Value * 100f));
                case WorldEventEffectKind.NextFightBuff:
                    return DescribeBuff(effect, localization);
                case WorldEventEffectKind.SlowTravel:
                    return WorldEventTexts.Format(localization, WorldEventTexts.FxSlow, Mathf.RoundToInt((1f - Mathf.Clamp(effect.Value, 0.1f, 1f)) * 100f));
                case WorldEventEffectKind.AdvanceHours:
                    return WorldEventTexts.Format(localization, WorldEventTexts.FxHours, Mathf.RoundToInt(effect.Value));
                case WorldEventEffectKind.EliteEncounter:
                    return WorldEventTexts.Get(localization, WorldEventTexts.FxElite);
                case WorldEventEffectKind.Encounter:
                    return WorldEventTexts.Get(localization, WorldEventTexts.FxFight);
                default:
                    return string.Empty;
            }
        }

        private static string DescribeBuff(WorldEventEffect effect, LocalizationManager localization)
        {
            // Percent buffs and the 0-based fractions (crit) read as "+15%"; flat regen as "+3".
            int percent = Mathf.RoundToInt(effect.Value * 100f);
            switch (effect.Stat)
            {
                case StatType.AttackDamage:
                    return WorldEventTexts.Format(localization, WorldEventTexts.FxBuffAttack, percent);
                case StatType.Defense:
                    return WorldEventTexts.Format(localization, WorldEventTexts.FxBuffDefense, percent);
                case StatType.CriticalChance:
                    return WorldEventTexts.Format(localization, WorldEventTexts.FxBuffCrit, percent);
                case StatType.HealthRegen:
                    return WorldEventTexts.Format(localization, WorldEventTexts.FxBuffRegen,
                        effect.Application == ModifierApplication.Flat ? Mathf.RoundToInt(effect.Value) : percent);
                default:
                    return WorldEventTexts.Get(localization, WorldEventTexts.FxBuffOther);
            }
        }

        private static string ItemName(string itemId, LocalizationManager localization)
        {
            ItemDefinition item = ItemDatabase.GetById(itemId);
            return item != null ? item.LocalizedName(localization) : itemId;
        }

        // ------------------------------------------------------------------
        // Card
        // ------------------------------------------------------------------

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        /// <summary>Opens the card for <paramref name="spec"/> in <paramref name="region"/> (also a debug/test entry point).</summary>
        public bool Show(WorldEventSpec spec, BiomeType region)
        {
            if (spec == null || IsOpen || !EnsureOverlay())
            {
                return false;
            }

            _spec = spec;
            _region = region;
            _outcome = null;

            InventoryManager inventory = BeginInventorySession();
            LocalizationManager localization = Localization;
            CharacterId? hero = ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;

            SetText(_headerLabel, WorldEventTexts.Get(localization, WorldEventTexts.Header));
            SetText(_titleLabel, WorldEventTexts.Get(localization, spec.TitleKey));
            SetText(_bodyLabel, WorldEventTexts.Get(localization, spec.BodyKey));
            string heroLine = BuildHeroLine(spec, hero, localization);
            SetText(_heroLineLabel, heroLine);
            _heroLineLabel.style.display = string.IsNullOrEmpty(heroLine) ? DisplayStyle.None : DisplayStyle.Flex;

            bool anyBlocked = false;
            for (int i = 0; i < 2; i++)
            {
                WorldEventChoiceSpec choice = spec.Choice(i);
                bool affordable = WorldEventRules.CanAfford(choice, inventory != null ? (System.Func<string, int>)inventory.GetTotalCount : null);
                anyBlocked |= !affordable;
                SetText(_choiceLabels[i], WorldEventTexts.Get(localization, spec.ChoiceKey(i)));
                SetText(_choiceFxLabels[i], DescribeChoice(choice, region, localization));
                _choiceButtons[i].SetEnabled(affordable);
            }

            SetText(_hintLabel, WorldEventTexts.Get(localization, WorldEventTexts.CannotAfford));
            _hintLabel.style.display = anyBlocked ? DisplayStyle.Flex : DisplayStyle.None;
            _choicesRow.style.display = DisplayStyle.Flex;
            _resultBox.style.display = DisplayStyle.None;
            _overlay.style.display = DisplayStyle.Flex;
            GameplayPause.Acquire(this);

            Button first = _choiceButtons[0].enabledSelf ? _choiceButtons[0] : _choiceButtons[1];
            first.Focus();
            return true;
        }

        private void OnChoiceClicked(int index)
        {
            if (!IsOpen || _outcome != null)
            {
                return;
            }

            WorldEventOutcome outcome = WorldEventOutcomes.Apply(_spec.Choice(index), _region, _session != null ? _session.Inventory : null, WorldEventState.ResolveShared());
            if (outcome == null)
            {
                _hintLabel.style.display = DisplayStyle.Flex;
                return;
            }

            _outcome = outcome;

            // The bag goes back to the carry now: what was bought/paid is in the next save and the next fight.
            EndInventorySession();

            if (_worldMap != null && outcome.TravelSpeedMultiplier < 1f)
            {
                _worldMap.TravelSpeedMultiplier *= outcome.TravelSpeedMultiplier;
            }

            if (outcome.HoursToAdvance > 0f && ServiceLocator.TryGet(out TimeOfDayManager time) && time != null)
            {
                time.AdvanceHours(outcome.HoursToAdvance);
            }

            LocalizationManager localization = Localization;
            SetText(_resultLabel, WorldEventTexts.Get(localization, _spec.ResultKey(index)));
            SetText(_resultFxLabel, DescribeOutcome(outcome, localization));
            bool fight = outcome.StartElite || outcome.StartEncounter;
            SetText(_continueButton, WorldEventTexts.Get(localization, fight ? WorldEventTexts.Fight : WorldEventTexts.Continue));
            _choicesRow.style.display = DisplayStyle.None;
            _hintLabel.style.display = DisplayStyle.None;
            _resultBox.style.display = DisplayStyle.Flex;
            _continueButton.Focus();
        }

        private void OnContinueClicked()
        {
            WorldEventOutcome outcome = _outcome;
            BiomeType region = _region;
            Close();

            if (outcome == null || _worldMap == null || (!outcome.StartElite && !outcome.StartEncounter))
            {
                return;
            }

            BiomeDefinition biome = BiomeRegistry.Get(region);
            WorldEventState state = WorldEventState.ResolveShared();
            EncounterPayload payload = outcome.StartElite
                ? WorldEventElite.BuildPayload(biome, Director.Random, _worldMap.Encounters != null ? _worldMap.Encounters.EnemyFilter : null)
                : _worldMap.Encounters?.BuildPayload(biome);

            if (payload == null)
            {
                return;
            }

            if (outcome.StartElite)
            {
                state.PendingEliteSeed = payload.seed;
            }

            if (!_worldMap.TryStartEventEncounter(payload) && outcome.StartElite)
            {
                state.PendingEliteSeed = null;
            }
        }

        /// <summary>Hides the card and releases the pause (the chosen outcome, if any, already applied).</summary>
        private void Close()
        {
            if (_overlay != null)
            {
                _overlay.style.display = DisplayStyle.None;
            }

            _spec = null;
            _outcome = null;
            EndInventorySession();
            GameplayPause.Release(this);
        }

        /// <summary>Gone without a choice (HUD hidden, save loaded, scene left): nothing applied, the cooldown stays.</summary>
        private void Cancel()
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                GameplayPause.Release(this);
            }
        }

        private InventoryManager BeginInventorySession()
        {
            if (_session == null)
            {
                ServiceLocator.TryGet(out SaveManager saveManager);
                _session = new CarriedInventorySession(saveManager);
            }

            return _session.Open(transform);
        }

        private void EndInventorySession()
        {
            if (_session != null && _session.IsOpen)
            {
                _session.Close();
            }
        }

        // ------------------------------------------------------------------
        // Building
        // ------------------------------------------------------------------

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

            // Same Rapadura-theme look as the HUD's Mochila summary / Vila panel (WorldMapHudView.uss);
            // .wm-event-* rules add the cousin's line and the two-line choice buttons.
            _overlay = new VisualElement { name = OverlayName };
            _overlay.AddToClassList("wm-backpack-overlay");
            _overlay.style.display = DisplayStyle.None;

            var panel = new VisualElement { name = "wm-event-panel" };
            panel.AddToClassList("rap-panel");
            panel.AddToClassList("wm-backpack-panel");
            panel.AddToClassList(PanelClass);
            _overlay.Add(panel);

            _headerLabel = AddLabel(panel, "wm-backpack-hero");
            _titleLabel = AddLabel(panel, "wm-backpack-title");
            _bodyLabel = AddLabel(panel, "wm-backpack-line");
            _heroLineLabel = AddLabel(panel, HeroLineClass);

            _choicesRow = new VisualElement { name = "wm-event-choices" };
            panel.Add(_choicesRow);
            for (int i = 0; i < 2; i++)
            {
                int captured = i;
                var button = new Button(() => OnChoiceClicked(captured)) { name = "wm-event-choice-" + (i == 0 ? "a" : "b"), focusable = true, text = string.Empty };
                button.AddToClassList("rap-button");
                button.AddToClassList(ChoiceClass);
                _choiceLabels[i] = AddLabel(button, ChoiceLabelClass);
                _choiceFxLabels[i] = AddLabel(button, ChoiceFxClass);
                _choiceButtons[i] = button;
                _choicesRow.Add(button);
            }

            _hintLabel = AddLabel(panel, "wm-backpack-hint");

            _resultBox = new VisualElement { name = "wm-event-result" };
            _resultBox.style.display = DisplayStyle.None;
            panel.Add(_resultBox);
            _resultLabel = AddLabel(_resultBox, ResultClass);
            _resultFxLabel = AddLabel(_resultBox, "wm-backpack-hint");
            _continueButton = new Button(OnContinueClicked) { name = "wm-event-continue-button", focusable = true };
            _continueButton.AddToClassList("rap-button");
            _continueButton.AddToClassList("rap-button--primary");
            _continueButton.AddToClassList("wm-backpack-close");
            _resultBox.Add(_continueButton);

            root.Add(_overlay);
            return true;
        }

        private static Label AddLabel(VisualElement parent, string className)
        {
            var label = new Label();
            label.AddToClassList(className);
            label.pickingMode = PickingMode.Ignore;
            parent.Add(label);
            return label;
        }

        private static void SetText(TextElement element, string text)
        {
            if (element != null && element.text != text)
            {
                element.text = text;
            }
        }
    }
}
