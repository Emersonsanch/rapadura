using System.Text;
using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.HUD
{
    /// <summary>
    /// MU-style area name of the continuous world (Docs/Mapas100.md §0.10, "Área atual"): on <see cref="AreaChangedEvent"/>
    /// (published by the <see cref="OverworldView"/>'s <see cref="OverworldAreaTracker"/>, with hysteresis) a letreiro fades
    /// in at the top centre for <see cref="DisplaySeconds"/>: the region, the area's name (the town's when the hero steps into
    /// one), the level band "Nv 12–16" (or "Área segura") and a warning line when the area's special gear is missing
    /// (<see cref="MapRequirementHeroState"/> / <see cref="MapAccessRules.MeetsRequirement"/>) or its monsters are far above
    /// the hero. A small permanent label under the HUD's top-right buttons keeps the current area while in map mode.
    ///
    /// Self-bootstrapping like <c>BossIntroBannerController</c> (one DontDestroyOnLoad host, PanelSettings borrowed from the
    /// HUD), picking-mode Ignore everywhere, look in <c>Resources/UI/AreaBanner.uss</c> (imports the theme; root gets
    /// <c>rap-theme</c>). Hidden while gameplay is paused; "Reduzir Movimento" drops the fade.
    /// </summary>
    public sealed class AreaBannerController : MonoBehaviour
    {
        public const string ResourcePath = "UI/AreaBanner";
        public const float SortingOrder = 840f;
        public const float DisplaySeconds = 3.2f;

        // Must match .area-banner-plate transition-duration in AreaBanner.uss (the hide waits for the fade).
        private const float FadeSeconds = 0.6f;
        private const float LabelRefreshSeconds = 0.25f;
        private const string PlayerTag = "Player";

        public const string RootName = "area-banner-root";
        public const string BannerName = "area-banner";
        public const string PlateName = "area-banner-plate";
        public const string RegionName = "area-banner-region";
        public const string NameLabelName = "area-banner-name";
        public const string BandName = "area-banner-band";
        public const string WarningName = "area-banner-warning";
        public const string LabelName = "area-label";
        public const string InstantClass = "area-banner--instant";

        private static AreaBannerController _instance;
        private static StyleSheet _sheet;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _banner;
        private VisualElement _plate;
        private Label _region;
        private Label _name;
        private Label _band;
        private Label _warning;
        private Label _label;
        private float _remaining;
        private bool _showing;
        private bool _fadingOut;
        private float _nextLabel;
        private IVisualElementScheduledItem _hideItem;

        /// <summary>True while the letreiro is up (fade-out included).</summary>
        public bool IsShowing => _showing;

        /// <summary>The name on the letreiro now (tests).</summary>
        public string ShownName => _name != null ? _name.text : string.Empty;

        /// <summary>The permanent label's text ("" when hidden).</summary>
        public string LabelText => _label != null && _label.style.display != DisplayStyle.None ? _label.text : string.Empty;

        /// <summary>
        /// Whether a change gets the letreiro (pure): a new area (or the first reading after arriving), or stepping into a town;
        /// leaving a town back into the same area does not.
        /// </summary>
        public static bool ShouldAnnounce(AreaChangedEvent change) => change.Current != null && (change.ZoneChanged || change.EnteredSettlement);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null || FindFirstObjectByType<AreaBannerController>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(AreaBannerController));
            host.AddComponent<UIDocument>();
            host.AddComponent<AreaBannerController>();
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
            EventBus.Subscribe<AreaChangedEvent>(OnAreaChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<AreaChangedEvent>(OnAreaChanged);
            Hide();
            _root = null;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnAreaChanged(AreaChangedEvent change)
        {
            if (ShouldAnnounce(change))
            {
                Show(change.Current, change.CurrentPart, change.CurrentSettlement);
            }

            _nextLabel = 0f;
        }

        /// <summary>Puts up the letreiro for <paramref name="zone"/> (no-op until a panel can be borrowed).</summary>
        public void Show(OverworldZone zone, OverworldPart part, OverworldSettlement settlement)
        {
            if (zone == null || !EnsureBound())
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            MapModeController mode = MapModeController.Current;
            MapRegion region = part != null ? part.Region : mode != null && mode.Overworld != null ? mode.Overworld.Region : MapRegion.Zarafshan;
            int level = mode != null ? mode.HeroLevel : 1;
            InventoryManager bag = FindBag();
            bool hasGear = MapAccessRules.MeetsRequirement(zone.RequirementTag, MapRequirementHeroState.LookupFor(bag));

            _region.text = MapTexts.RegionName(localization, region).ToUpperInvariant();
            _name.text = AreaName(localization, zone, settlement);
            _band.text = settlement != null ? OverworldTexts.Get(localization, OverworldTexts.AreaSafe) : BandText(localization, zone);
            string warning = WarningText(localization, zone, settlement, level, hasGear);
            _warning.text = warning;
            _warning.style.display = string.IsNullOrEmpty(warning) ? DisplayStyle.None : DisplayStyle.Flex;

            ServiceLocator.TryGet(out AccessibilitySettings accessibility);
            bool noMotion = UiMotionPreferences.IsReducedMotion(accessibility);
            _banner.EnableInClassList(InstantClass, noMotion);
            _remaining = DisplaySeconds + (string.IsNullOrEmpty(warning) ? 0f : 1f);
            _showing = true;
            _fadingOut = false;
            _hideItem?.Pause();
            _hideItem = null;
            _banner.style.display = DisplayStyle.Flex;
            if (noMotion)
            {
                _plate.style.opacity = 1f;
            }
            else
            {
                // Next frame, so the opacity transition runs from the hidden state.
                _plate.style.opacity = 0f;
                _plate.schedule.Execute(() => _plate.style.opacity = 1f);
            }
        }

        /// <summary>The area's name, or the town's when the hero stands in one.</summary>
        public static string AreaName(LocalizationManager localization, OverworldZone zone, OverworldSettlement settlement)
        {
            MapDefinition town = settlement != null ? MapCatalog.Get(settlement.MapId) : null;
            if (town != null)
            {
                string name = MapTexts.SettlementName(localization, town);
                if (!string.IsNullOrEmpty(name) && name != town.SettlementNameKey)
                {
                    return name;
                }
            }

            return OverworldTexts.ZoneName(localization, zone);
        }

        /// <summary>"Nv 12–16" / "Lv 12–16".</summary>
        public static string BandText(LocalizationManager localization, OverworldZone zone) =>
            zone == null ? string.Empty : OverworldTexts.Format(localization, OverworldTexts.AreaBand, zone.MinLevel, zone.MaxLevel);

        /// <summary>The warning line (pure): the missing special gear, then the danger warning; empty in a town or when all is well.</summary>
        public static string WarningText(LocalizationManager localization, OverworldZone zone, OverworldSettlement settlement, int heroLevel, bool hasGear)
        {
            if (zone == null || settlement != null)
            {
                return string.Empty;
            }

            var text = new StringBuilder();
            if (!hasGear && !string.IsNullOrEmpty(zone.RequirementTag))
            {
                text.Append(OverworldTexts.Format(localization, OverworldTexts.AreaMissing, MapTexts.RequirementName(localization, zone.RequirementTag)));
            }

            if (OverworldAreaAmbience.IsDangerous(zone, heroLevel))
            {
                if (text.Length > 0)
                {
                    text.Append('\n');
                }

                text.Append(MapTexts.Get(localization, MapTexts.DangerWarning));
            }

            return text.ToString();
        }

        /// <summary>The permanent label: "Pomar da Vó Edna · Nv 1–3" (the town's name and "Área segura" inside one).</summary>
        public static string LabelFor(LocalizationManager localization, OverworldZone zone, OverworldSettlement settlement)
        {
            if (zone == null)
            {
                return string.Empty;
            }

            string band = settlement != null ? OverworldTexts.Get(localization, OverworldTexts.AreaSafe) : BandText(localization, zone);
            return OverworldTexts.Format(localization, OverworldTexts.AreaLabel, AreaName(localization, zone, settlement), band);
        }

        private void Update()
        {
            if (_showing && _banner != null)
            {
                // A modal holds the letreiro (and its clock); it is the first card of the top-centre lane.
                bool paused = MessageCards.ShouldHold(MessageCards.IsModalOpen);
                _banner.style.display = paused ? DisplayStyle.None : DisplayStyle.Flex;
                MessageCards.Place(_banner, MessageCards.AreaBannerId, MessageCards.AreaBannerPriority, !paused);
                if (!paused && !_fadingOut)
                {
                    _remaining -= Time.unscaledDeltaTime;
                    if (_remaining <= 0f)
                    {
                        FadeOut();
                    }
                }
            }

            else
            {
                MessageCards.Release(_banner, MessageCards.AreaBannerId);
            }

            if (Time.unscaledTime >= _nextLabel)
            {
                _nextLabel = Time.unscaledTime + LabelRefreshSeconds;
                RefreshLabel();
            }
        }

        private void RefreshLabel()
        {
            OverworldAreaTracker tracker = MapModeController.Current != null && MapModeController.Current.OverworldArea != null
                ? MapModeController.Current.OverworldArea.AreaTracker
                : null;
            if (tracker == null || tracker.Zone == null)
            {
                if (_label != null)
                {
                    _label.style.display = DisplayStyle.None;
                }

                return;
            }

            if (!EnsureBound())
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            string text = LabelFor(localization, tracker.Zone, tracker.Settlement);
            if (_label.text != text)
            {
                _label.text = text;
            }

            _label.style.display = GameplayPause.IsPaused ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void FadeOut()
        {
            _fadingOut = true;
            _plate.style.opacity = 0f;
            if (_banner.ClassListContains(InstantClass) || UiMotionPreferences.IsReducedMotion())
            {
                Hide();
                return;
            }

            _hideItem?.Pause();
            _hideItem = _plate.schedule.Execute(Hide);
            _hideItem.ExecuteLater((long)(FadeSeconds * 1000f));
        }

        private void Hide()
        {
            _showing = false;
            _fadingOut = false;
            _hideItem?.Pause();
            _hideItem = null;
            if (_banner != null)
            {
                _banner.style.display = DisplayStyle.None;
                _plate.style.opacity = 0f;
            }
        }

        private static InventoryManager FindBag()
        {
            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            return player != null ? player.GetComponentInChildren<InventoryManager>() : null;
        }

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
            _root = CreateView(UiMotionPreferences.IsReducedMotion());
            _banner = _root.Q<VisualElement>(BannerName);
            _plate = _root.Q<VisualElement>(PlateName);
            _region = _root.Q<Label>(RegionName);
            _name = _root.Q<Label>(NameLabelName);
            _band = _root.Q<Label>(BandName);
            _warning = _root.Q<Label>(WarningName);
            _label = _root.Q<Label>(LabelName);
            documentRoot.Add(_root);
            return true;
        }

        /// <summary>The view tree (root → letreiro plate with its four lines, and the permanent label), hidden, picking-mode Ignore, themed. EditMode-testable.</summary>
        public static VisualElement CreateView(bool reducedMotion)
        {
            var root = new VisualElement { name = RootName, pickingMode = PickingMode.Ignore };
            root.AddToClassList(RootName);
            if (_sheet == null)
            {
                _sheet = Resources.Load<StyleSheet>(ResourcePath);
            }

            if (_sheet != null)
            {
                root.styleSheets.Add(_sheet);
            }

            root.AddToClassList("rap-theme");
            UiMotionPreferences.ApplyTo(root, reducedMotion);

            var banner = new VisualElement { name = BannerName, pickingMode = PickingMode.Ignore };
            banner.AddToClassList(BannerName);
            banner.style.display = DisplayStyle.None;

            var plate = new VisualElement { name = PlateName, pickingMode = PickingMode.Ignore };
            plate.AddToClassList(PlateName);
            plate.style.opacity = 0f;
            plate.Add(NewLabel(RegionName));
            plate.Add(NewLabel(NameLabelName));
            plate.Add(NewLabel(BandName));
            Label warning = NewLabel(WarningName);
            warning.style.display = DisplayStyle.None;
            plate.Add(warning);
            banner.Add(plate);
            root.Add(banner);

            Label label = NewLabel(LabelName);
            label.style.display = DisplayStyle.None;
            root.Add(label);
            return root;
        }

        private static Label NewLabel(string name)
        {
            var label = new Label { name = name, pickingMode = PickingMode.Ignore };
            label.AddToClassList(name);
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
    }
}
