using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.Save;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.WorldMap
{
    /// <summary>One map row of the Barcas de Luz window (pure, tested).</summary>
    public readonly struct MapTravelRow
    {
        public readonly MapDefinition Map;
        public readonly MapAccessCheck Check;
        public readonly int Cost;
        public readonly string Name;
        public readonly string Meta;
        public readonly string Requirement;
        public readonly string Status;
        public readonly string ButtonText;
        public readonly string Hint;

        public MapTravelRow(MapDefinition map, MapAccessCheck check, int cost, string name, string meta, string requirement, string status,
            string buttonText, string hint)
        {
            Map = map;
            Check = check;
            Cost = cost;
            Name = name;
            Meta = meta;
            Requirement = requirement;
            Status = status;
            ButtonText = buttonText;
            Hint = hint;
        }

        public bool CanTravel => Check == MapAccessCheck.Ok;

        /// <summary>Greyed with the padlock: closed by level, gate or build.</summary>
        public bool IsLocked => Check == MapAccessCheck.LevelTooLow || Check == MapAccessCheck.MissingRequirement || Check == MapAccessCheck.NotBuilt
            || Check == MapAccessCheck.DockNotVisited;
    }

    /// <summary>What the window travels with: the bag that pays and where a city trip / a map trip goes.</summary>
    public sealed class FastTravelWindowRequest
    {
        public InventoryManager Bag;

        /// <summary>The island map (city rows: the Barca between towns). Null in a playable map / city walk: maps only.</summary>
        public WorldMapController Map;

        /// <summary>A town was paid for: destination region and gold.</summary>
        public Action<BiomeType, int> OnCityTravel;

        /// <summary>A map was paid for: map id and gold. Null = <see cref="FastTravelWindowController.TravelToMap"/>.</summary>
        public Action<string, int> OnMapTravel;

        /// <summary>The window closed without a trip.</summary>
        public Action OnClosed;
    }

    /// <summary>
    /// "Barcas de Luz · Mover", the one travel window (TODO.md "FastTravelWindowController", MU's window M): the towns of
    /// the Barca (<see cref="CityFastTravelPanel.BuildRow"/>, <see cref="FastTravelRules"/>) and the maps of
    /// <see cref="MapCatalog"/> (<see cref="MapAccessRules"/>), grouped by region. Locked maps are greyed with a padlock and
    /// the line "Exige Nível {warp} (ou a pé nível {walk}) - Barca: {cost} ouro"; clicking one says from which level it
    /// can be walked into. "Mover" pays the fare from the bag (<see cref="InventoryManager.RemoveById"/>) and travels
    /// (<see cref="SceneTransitionService.TravelToMap"/>).
    /// <list type="bullet">
    /// <item>Opened by <b>M</b>, by the compass button (injected into the island-map HUD cluster; floating top-right in a
    /// playable map or a city walk) and by the Cais da Barca (the city hub delegates to it, a map's dock calls
    /// <see cref="MapModeController.DockReached"/>). Never during an encounter.</item>
    /// <item>Self-bootstrapping like the bestiary: one DontDestroyOnLoad host with its own UIDocument (borrowed
    /// PanelSettings), view in <c>Resources/UI/FastTravelWindow.uxml</c>; holds a <see cref="GameplayPause"/> while up.</item>
    /// </list>
    /// </summary>
    public class FastTravelWindowController : MonoBehaviour
    {
        public const float SortingOrder = 31f;
        public const string ViewResourcePath = "UI/FastTravelWindow";
        public const string MapButtonName = "wm-travel-compass-button";
        public const string MapGoPrefix = "ftw-go-";
        public const string MapRowPrefix = "ftw-row-";
        public const string CurrencyItemId = CityFastTravelPanel.CurrencyItemId;

        private const float HudScanInterval = 0.25f;

        private static FastTravelWindowController _instance;

        private UIDocument _document;
        private VisualTreeAsset _viewAsset;
        private VisualElement _layer;
        private VisualElement _root;
        private Button _compass;
        private Label _title;
        private Label _subtitle;
        private Label _hero;
        private Label _gold;
        private Label _overloaded;
        private ScrollView _list;
        private Label _status;
        private Button _close;

        private FastTravelWindowRequest _request;
        private CarriedInventorySession _ownSession;
        private bool _open;
        private int _releasePauseAtFrame = -1;
        private float _nextHudScan;
        private Button _mapButton;
        private WorldMapHudController _hud;

        public static bool IsOpenShared => _instance != null && _instance._open;

        /// <summary>Opens the window for <paramref name="request"/> (null = the default context of the scene). False if it cannot render.</summary>
        public static bool OpenShared(FastTravelWindowRequest request = null) => _instance != null && _instance.Open(request);

        public static void CloseShared()
        {
            if (_instance != null)
            {
                _instance.Close();
            }
        }

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        // ------------------------------------------------------------------
        // Pure
        // ------------------------------------------------------------------

        /// <summary>
        /// The row of <paramref name="map"/> for a hero of <paramref name="level"/> with <paramref name="gold"/>.
        /// <paramref name="isUnvisitedStop"/> (<see cref="MapModeController.IsUnvisitedStop(OverworldLayout,string)"/>): a dock of
        /// the region the hero walks in that was never reached on foot is offered closed (<see cref="MapAccessCheck.DockNotVisited"/>).
        /// </summary>
        public static MapTravelRow BuildMapRow(MapDefinition map, int level, CharacterId? hero, int gold, bool overloaded, string currentMapId, bool busy,
            Func<string, bool> hasRequirement, LocalizationManager localization, Func<string, bool> isUnvisitedStop = null)
        {
            MapAccessCheck check = MapAccessRules.CheckWarp(map, level, hero, gold, overloaded, currentMapId, busy, hasRequirement);
            if ((check == MapAccessCheck.Ok || check == MapAccessCheck.NotEnoughGold) && isUnvisitedStop != null && isUnvisitedStop(map.Id))
            {
                check = MapAccessCheck.DockNotVisited;
            }

            int cost = MapAccessRules.CalculateWarpCost(map, overloaded);
            int warp = MapAccessRules.EffectiveLevelRequirement(map.WarpLevelRequirement, hero, map.RegionType);
            int walk = MapAccessRules.EffectiveLevelRequirement(map.WalkLevelRequirement, hero, map.RegionType);

            string requirement = check == MapAccessCheck.LevelTooLow || check == MapAccessCheck.NotBuilt
                ? (cost > 0 ? MapTexts.Format(localization, MapTexts.RequirementLine, warp, walk, cost) : MapTexts.Format(localization, MapTexts.RequirementLineFree, warp, walk))
                : (cost > 0 ? MapTexts.Format(localization, MapTexts.OpenLine, cost, walk) : MapTexts.Format(localization, MapTexts.OpenLineFree, walk));
            if (map.HasSpecialRequirement)
            {
                requirement += " · " + MapTexts.Format(localization, MapTexts.StatusRequirement, MapTexts.RequirementName(localization, map.SpecialRequirementTag));
            }

            string status;
            switch (check)
            {
                case MapAccessCheck.Here: status = MapTexts.Get(localization, MapTexts.StatusHere); break;
                case MapAccessCheck.Busy: status = MapTexts.Get(localization, MapTexts.StatusBusy); break;
                case MapAccessCheck.NotBuilt: status = MapTexts.Get(localization, MapTexts.StatusNotBuilt); break;
                case MapAccessCheck.LevelTooLow: status = MapTexts.Format(localization, MapTexts.StatusLevel, warp); break;
                case MapAccessCheck.MissingRequirement: status = MapTexts.Format(localization, MapTexts.StatusRequirement, MapTexts.RequirementName(localization, map.SpecialRequirementTag)); break;
                case MapAccessCheck.NotEnoughGold: status = MapTexts.Format(localization, MapTexts.StatusNoGold, cost); break;
                case MapAccessCheck.DockNotVisited: status = OverworldTexts.Get(localization, OverworldTexts.DockNotVisited); break;
                default: status = string.Empty; break;
            }

            string meta = MapTexts.Format(localization, MapTexts.RowMeta, map.MonsterLevelMin, map.MonsterLevelMax, MapTexts.SettlementLine(localization, map));
            string button = cost > 0 ? MapTexts.Format(localization, MapTexts.Go, cost) : MapTexts.Get(localization, MapTexts.GoFree);
            string hint = check == MapAccessCheck.LevelTooLow ? MapTexts.Format(localization, MapTexts.LevelHint, walk)
                : check == MapAccessCheck.MissingRequirement ? status + " · " + MapTexts.RequirementHint(localization, map.SpecialRequirementTag)
                : status;
            return new MapTravelRow(map, check, cost, MapTexts.NumberedName(localization, map), meta, requirement, status, button, hint);
        }

        /// <summary>The default map trip: the Barca leaves for <paramref name="mapId"/> (Main.unity in map mode, arrival at its dock).</summary>
        public static bool TravelToMap(string mapId)
        {
            // Inside a continuous region (Docs/Mapas100.md §0.10) the barca moves the hero to that dock in the same scene.
            MapModeController mode = MapModeController.Current;
            if (mode != null && mode.TryTravelWithin(mapId))
            {
                return true;
            }

            return ServiceLocator.TryGet(out SceneTransitionService transition) && transition.TravelToMap(mapId);
        }

        /// <summary>The stops the barca refuses right now: the unvisited docks of the region the hero walks in (null elsewhere).</summary>
        public static Func<string, bool> UnvisitedStopLookup()
        {
            MapModeController mode = MapModeController.Current;
            return mode != null && mode.Overworld != null ? mode.IsUnvisitedStop : (Func<string, bool>)null;
        }

        /// <summary>The backpack is above 100% of its capacity right now (the fare doubles).</summary>
        public static bool IsOverloadedNow() => EncumbranceTracker.HasCurrent && MapAccessRules.IsOverloaded(EncumbranceTracker.Current.Ratio);

        /// <summary>The hero level: the scene's Player, else the carried state (island map).</summary>
        public static int HeroLevelNow()
        {
            MapModeController mode = MapModeController.Current;
            if (mode != null)
            {
                return mode.HeroLevel;
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Rapadura.Gameplay.Player.PlayerStats stats = player != null ? player.GetComponent<Rapadura.Gameplay.Player.PlayerStats>() : null;
            return stats != null ? Mathf.Max(1, stats.Level) : MapModeController.CarriedHeroLevel();
        }

        /// <summary>The window may open now: no encounter in course, and on the island map or in a map / city walk.</summary>
        public static bool CanOpenHere()
        {
            if (!ServiceLocator.TryGet(out SceneTransitionService transition) || transition.HasPendingEncounter)
            {
                return false;
            }

            return MapModeController.Current != null || CityModeController.Current != null || FindFirstObjectByType<WorldMapController>() != null;
        }

        // ------------------------------------------------------------------
        // Lifecycle
        // ------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null || FindFirstObjectByType<FastTravelWindowController>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(FastTravelWindowController));
            host.AddComponent<UIDocument>();
            host.AddComponent<FastTravelWindowController>();
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

            _document = GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            _document.sortingOrder = SortingOrder;
            Rapadura.Audio.UiClickSoundBinder.Register(_document);
        }

        private void OnEnable()
        {
            _layer = null;
            SceneManager.sceneLoaded += OnSceneLoaded;
            MapModeController.DockReached += OnDockReached;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            MapModeController.DockReached -= OnDockReached;
            CloseInternal(false);
            GameplayPause.Release(this);
            RemoveMapButton();
            _layer = null;
        }

        private void OnDestroy()
        {
            GameplayPause.Release(this);
            Rapadura.Audio.UiClickSoundBinder.Unregister(_document);
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CloseInternal(false);
            GameplayPause.Release(this);
            _releasePauseAtFrame = -1;
            RemoveMapButton();
            _hud = null;
        }

        private void Update()
        {
            if (_releasePauseAtFrame >= 0 && Time.frameCount >= _releasePauseAtFrame)
            {
                _releasePauseAtFrame = -1;
                GameplayPause.Release(this);
            }

            Keyboard keyboard = Keyboard.current;
            if (_open)
            {
                Gamepad pad = Gamepad.current;
                if ((keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.mKey.wasPressedThisFrame)) || (pad != null && pad.buttonEast.wasPressedThisFrame))
                {
                    Close();
                }
            }
            else if (keyboard != null && keyboard.mKey.wasPressedThisFrame && !GameplayPause.IsPausedByOthers(this))
            {
                OpenFromShortcut();
            }

            if (Time.unscaledTime >= _nextHudScan)
            {
                _nextHudScan = Time.unscaledTime + HudScanInterval;
                RefreshCompass();
            }
        }

        private void OnDockReached()
        {
            if (!_open)
            {
                OpenFromShortcut();
            }
        }

        /// <summary>M / compass / dock: on the island map through the city hub (it owns the bag and the town trip), else the scene default.</summary>
        public void OpenFromShortcut()
        {
            if (!CanOpenHere())
            {
                return;
            }

            VillageHubButton hub = MapModeController.Current == null && CityModeController.Current == null ? FindFirstObjectByType<VillageHubButton>() : null;
            if (hub != null && hub.OpenTravelWindow())
            {
                return;
            }

            Open(null);
        }

        // ------------------------------------------------------------------
        // Open / close
        // ------------------------------------------------------------------

        public bool IsOpen => _open;

        public bool Open(FastTravelWindowRequest request)
        {
            if (_open || !CanOpenHere() || !EnsureBound())
            {
                return _open;
            }

            _request = request ?? DefaultRequest();
            if (_request.Bag == null)
            {
                CloseOwnSession();
                return false;
            }

            _open = true;
            _releasePauseAtFrame = -1;
            GameplayPause.Acquire(this);
            _status.text = string.Empty;
            Render();
            SetRootVisible(true);
            return true;
        }

        public void Close() => CloseInternal(true);

        private void CloseInternal(bool notify)
        {
            if (!_open)
            {
                return;
            }

            _open = false;
            SetRootVisible(false);
            Action closed = _request != null ? _request.OnClosed : null;
            _request = null;
            CloseOwnSession();
            _releasePauseAtFrame = Time.frameCount + 1;
            if (notify)
            {
                closed?.Invoke();
            }
        }

        /// <summary>The scene's own context: the Player's bag in Main.unity, a carried bag on the island map; maps only.</summary>
        private FastTravelWindowRequest DefaultRequest()
        {
            InventoryManager bag = null;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                bag = player.GetComponentInChildren<InventoryManager>();
            }

            if (bag == null && MapModeController.Current == null && CityModeController.Current == null)
            {
                ServiceLocator.TryGet(out SaveManager saves);
                _ownSession = new CarriedInventorySession(saves);
                bag = _ownSession.Open(transform);
            }

            if (bag == null)
            {
                bag = FindFirstObjectByType<InventoryManager>();
            }

            return new FastTravelWindowRequest { Bag = bag };
        }

        private void CloseOwnSession()
        {
            if (_ownSession != null)
            {
                _ownSession.Close();
                _ownSession = null;
            }
        }

        // ------------------------------------------------------------------
        // Binding
        // ------------------------------------------------------------------

        private bool EnsureBound()
        {
            if (_document.panelSettings == null)
            {
                PanelSettings donor = null;
                foreach (UIDocument document in FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (document != _document && document.panelSettings != null)
                    {
                        donor = document.panelSettings;
                        break;
                    }
                }

                if (donor == null)
                {
                    return false;
                }

                _document.panelSettings = donor;
                _document.sortingOrder = SortingOrder;
            }

            VisualElement documentRoot = _document.rootVisualElement;
            if (documentRoot == null)
            {
                return false;
            }

            if (_layer != null && _layer.parent != null)
            {
                return true;
            }

            _viewAsset ??= Resources.Load<VisualTreeAsset>(ViewResourcePath);
            if (_viewAsset == null)
            {
                Debug.LogWarning($"[FastTravelWindowController] Missing Resources/{ViewResourcePath}.uxml; travel window disabled.");
                enabled = false;
                return false;
            }

            documentRoot.pickingMode = PickingMode.Ignore;
            _layer = documentRoot.Q<VisualElement>("ftw-layer");
            if (_layer == null)
            {
                _viewAsset.CloneTree(documentRoot);
                _layer = documentRoot.Q<VisualElement>("ftw-layer");
            }

            if (_layer == null)
            {
                return false;
            }

            for (VisualElement element = _layer; element != null && element != documentRoot; element = element.parent)
            {
                element.pickingMode = PickingMode.Ignore;
            }

            _root = _layer.Q<VisualElement>("ftw-root");
            _compass = _layer.Q<Button>("ftw-compass");
            _title = _layer.Q<Label>("ftw-title");
            _subtitle = _layer.Q<Label>("ftw-subtitle");
            _hero = _layer.Q<Label>("ftw-hero");
            _gold = _layer.Q<Label>("ftw-gold");
            _overloaded = _layer.Q<Label>("ftw-overloaded");
            _list = _layer.Q<ScrollView>("ftw-list");
            _status = _layer.Q<Label>("ftw-status");
            _close = _layer.Q<Button>("ftw-close");
            _close.clicked -= Close;
            _close.clicked += Close;
            _compass.clicked -= OnCompassClicked;
            _compass.clicked += OnCompassClicked;
            _root.RegisterCallback<NavigationCancelEvent>(evt =>
            {
                if (_open)
                {
                    evt.StopPropagation();
                    Close();
                }
            });
            SetRootVisible(false);
            return true;
        }

        private void SetRootVisible(bool visible)
        {
            if (_root != null)
            {
                _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                _root.pickingMode = visible ? PickingMode.Position : PickingMode.Ignore;
            }
        }

        // ------------------------------------------------------------------
        // Rendering
        // ------------------------------------------------------------------

        private void Render()
        {
            LocalizationManager localization = Localization;
            InventoryManager bag = _request.Bag;
            int gold = bag != null ? bag.GetTotalCount(CurrencyItemId) : 0;
            int level = HeroLevelNow();
            CharacterId? hero = ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;
            bool overloaded = IsOverloadedNow();
            WorldMapController map = _request.Map;
            bool busy = map != null && CityFastTravelPanel.IsEventActive(map);
            string here = MapModeController.Current != null ? MapModeController.Current.MapId : null;

            _title.text = MapTexts.Get(localization, MapTexts.WindowTitle);
            _subtitle.text = MapTexts.Get(localization, MapTexts.WindowSubtitle);
            _hero.text = hero.HasValue
                ? MapTexts.Format(localization, MapTexts.HeroLine, CharacterRegistry.Get(hero.Value).DisplayName, level)
                : MapTexts.Format(localization, MapTexts.HeroLineNoName, level);
            _gold.text = MapTexts.Format(localization, MapTexts.Gold, gold);
            _overloaded.text = MapTexts.Get(localization, MapTexts.Overloaded);
            _overloaded.style.display = overloaded ? DisplayStyle.Flex : DisplayStyle.None;
            _close.text = MapTexts.Get(localization, MapTexts.Close);

            _list.Clear();
            Button first = null;
            Func<string, bool> hasRequirement = MapRequirementHeroState.LookupFor(bag);
            Func<string, bool> unvisitedStop = UnvisitedStopLookup();
            foreach (MapRegion region in MapCatalog.Regions)
            {
                var header = new Label(MapTexts.RegionName(localization, region));
                header.AddToClassList("ftw-region-header");
                _list.Add(header);

                BiomeType? biome = MapCatalog.LegacyBiomeOf(region);
                CitySpec city = map != null ? CityCatalog.ForRegion(biome) : null;
                if (city != null)
                {
                    Button go = AddCityRow(city, map, gold, localization);
                    first ??= go;
                }

                foreach (MapDefinition def in MapCatalog.InRegion(region))
                {
                    MapTravelRow row = BuildMapRow(def, level, hero, gold, overloaded, here, busy, hasRequirement, localization, unvisitedStop);
                    Button go = AddMapRow(row);
                    first ??= go;
                }
            }

            (first ?? _close).Focus();
        }

        private Button AddCityRow(CitySpec city, WorldMapController map, int gold, LocalizationManager localization)
        {
            BiomeType from = map.CurrentNode != null ? map.CurrentNode.Region : BiomeType.Forest;
            bool inCity = map.CanExplore && WorldMapCityPresence.Current != null ? WorldMapCityPresence.Current.TokenInSafeZone : map.CanExplore;
            FastTravelRow row = CityFastTravelPanel.BuildRow(city, from, inCity, CityProgress.ResolveShared().IsVisited,
                map.Progress != null ? map.Progress.IsUnlocked : (Func<BiomeType, bool>)null,
                map.Graph != null ? map.Graph.GetNeighbours : (Func<BiomeType, IEnumerable<BiomeType>>)null, gold, localization,
                CityFastTravelPanel.BossClearedLookup(map), CityFastTravelPanel.IsEventActive(map));

            var element = new VisualElement { name = "wm-fast-travel-row-" + city.CityId };
            element.AddToClassList("ftw-row");
            element.AddToClassList("ftw-row--city");
            element.EnableInClassList("ftw-row--here", row.Check == FastTravelCheck.SameCity);
            element.EnableInClassList("ftw-row--locked", row.Check == FastTravelCheck.NotVisited || row.Check == FastTravelCheck.Locked || row.Check == FastTravelCheck.BossNotDefeated);
            var lockIcon = new VisualElement();
            lockIcon.AddToClassList("ftw-row__lock");
            element.Add(lockIcon);
            var body = new VisualElement();
            body.AddToClassList("ftw-row__body");
            element.Add(body);
            body.Add(NewLabel(MapTexts.Format(localization, MapTexts.KindCity, row.Name), "ftw-row__name"));
            body.Add(NewLabel(row.Status, "ftw-row__meta"));
            _list.Add(element);

            if (row.Check == FastTravelCheck.SameCity)
            {
                return null;
            }

            BiomeType destination = city.Region;
            var go = new Button(() => OnCityClicked(destination)) { name = CityFastTravelPanel.GoButtonPrefix + city.CityId, focusable = true, text = row.ButtonText };
            go.AddToClassList("rap-button");
            go.AddToClassList("ftw-go");
            go.SetEnabled(row.CanTravel);
            element.Add(go);
            return row.CanTravel ? go : null;
        }

        private Button AddMapRow(MapTravelRow row)
        {
            var element = new VisualElement { name = MapRowPrefix + row.Map.Id };
            element.AddToClassList("ftw-row");
            element.EnableInClassList("ftw-row--locked", row.IsLocked);
            element.EnableInClassList("ftw-row--here", row.Check == MapAccessCheck.Here);
            var lockIcon = new VisualElement();
            lockIcon.AddToClassList("ftw-row__lock");
            element.Add(lockIcon);
            var body = new VisualElement();
            body.AddToClassList("ftw-row__body");
            element.Add(body);
            body.Add(NewLabel(row.Name, "ftw-row__name"));
            body.Add(NewLabel(row.Meta, "ftw-row__meta"));
            body.Add(NewLabel(string.IsNullOrEmpty(row.Status) ? row.Requirement : row.Requirement + " · " + row.Status, "ftw-row__req"));
            string hint = row.Hint;
            element.RegisterCallback<ClickEvent>(_ =>
            {
                if (!string.IsNullOrEmpty(hint))
                {
                    _status.text = hint;
                }
            });
            _list.Add(element);

            if (row.Check == MapAccessCheck.Here)
            {
                return null;
            }

            string mapId = row.Map.Id;
            var go = new Button(() => OnMapClicked(mapId)) { name = MapGoPrefix + mapId, focusable = true, text = row.ButtonText };
            go.AddToClassList("rap-button");
            go.AddToClassList("ftw-go");
            go.SetEnabled(row.CanTravel);
            element.Add(go);
            return row.CanTravel ? go : null;
        }

        private static Label NewLabel(string text, string className)
        {
            var label = new Label(text);
            label.AddToClassList(className);
            return label;
        }

        // ------------------------------------------------------------------
        // Trips
        // ------------------------------------------------------------------

        private void OnMapClicked(string mapId)
        {
            if (!_open || _request == null || _request.Bag == null)
            {
                return;
            }

            MapDefinition map = MapCatalog.Get(mapId);
            InventoryManager bag = _request.Bag;
            CharacterId? hero = ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;
            bool busy = _request.Map != null && CityFastTravelPanel.IsEventActive(_request.Map);
            string here = MapModeController.Current != null ? MapModeController.Current.MapId : null;
            // Same region, dock never reached on foot: the barca only links docks already visited (DockNotVisited, a padlock).
            MapTravelRow row = BuildMapRow(map, HeroLevelNow(), hero, bag.GetTotalCount(CurrencyItemId), IsOverloadedNow(), here, busy,
                MapRequirementHeroState.LookupFor(bag), Localization, UnvisitedStopLookup());

            if (!row.CanTravel || (row.Cost > 0 && !bag.RemoveById(CurrencyItemId, row.Cost)))
            {
                _status.text = row.Hint;
                Render();
                return;
            }

            Action<string, int> onMap = _request.OnMapTravel;
            _request.OnClosed = null;
            CloseInternal(false);
            if (onMap != null)
            {
                onMap(mapId, row.Cost);
            }
            else
            {
                TravelToMap(mapId);
            }
        }

        private void OnCityClicked(BiomeType destination)
        {
            if (!_open || _request == null || _request.Bag == null || _request.Map == null)
            {
                return;
            }

            WorldMapController map = _request.Map;
            CitySpec city = CityCatalog.ForRegion(destination);
            BiomeType from = map.CurrentNode != null ? map.CurrentNode.Region : destination;
            InventoryManager bag = _request.Bag;
            FastTravelRow row = CityFastTravelPanel.BuildRow(city, from, map.CanExplore, CityProgress.ResolveShared().IsVisited,
                map.Progress != null ? map.Progress.IsUnlocked : (Func<BiomeType, bool>)null,
                map.Graph != null ? map.Graph.GetNeighbours : (Func<BiomeType, IEnumerable<BiomeType>>)null, bag.GetTotalCount(CurrencyItemId), Localization,
                CityFastTravelPanel.BossClearedLookup(map), CityFastTravelPanel.IsEventActive(map));
            if (!row.CanTravel || (row.Cost > 0 && !bag.RemoveById(CurrencyItemId, row.Cost)))
            {
                _status.text = row.Status;
                Render();
                return;
            }

            Action<BiomeType, int> onCity = _request.OnCityTravel;
            _request.OnClosed = null;
            CloseInternal(false);
            onCity?.Invoke(destination, row.Cost);
        }

        // ------------------------------------------------------------------
        // Compass buttons
        // ------------------------------------------------------------------

        private void OnCompassClicked()
        {
            if (GameplayPause.IsPausedByOthers(this) && !_open)
            {
                return;
            }

            if (_open)
            {
                Close();
            }
            else
            {
                OpenFromShortcut();
            }
        }

        private void RefreshCompass()
        {
            string label = MapTexts.Get(Localization, MapTexts.CompassButton);

            // Floating compass: a playable map or a city walk (Main.unity has no island-map HUD).
            bool floating = (MapModeController.Current != null && MapModeController.Current.IsReady) || (CityModeController.Current != null && CityModeController.Current.IsReady);
            if (floating && _layer == null)
            {
                EnsureBound();
            }

            if (_compass != null)
            {
                _compass.text = label;
                _compass.style.display = floating && !_open ? DisplayStyle.Flex : DisplayStyle.None;
                _compass.pickingMode = floating && !_open ? PickingMode.Position : PickingMode.Ignore;
            }

            // Island map: a button in the HUD cluster, before "Menu".
            if (_hud == null)
            {
                _hud = FindFirstObjectByType<WorldMapHudController>(FindObjectsInactive.Include);
            }

            UIDocument hudDocument = _hud != null ? _hud.GetComponent<UIDocument>() : null;
            VisualElement cluster = hudDocument != null && hudDocument.rootVisualElement != null ? hudDocument.rootVisualElement.Q<VisualElement>("wm-button-cluster") : null;
            if (cluster == null)
            {
                return;
            }

            if (_mapButton == null || _mapButton.parent != cluster)
            {
                RemoveMapButton();
                _mapButton = new Button(OnCompassClicked) { name = MapButtonName, focusable = true };
                _mapButton.AddToClassList("wm-menu-button");
                VisualElement menu = cluster.Q<Button>("wm-menu-button");
                int index = menu != null ? cluster.IndexOf(menu) : -1;
                if (index >= 0)
                {
                    cluster.Insert(index, _mapButton);
                }
                else
                {
                    cluster.Add(_mapButton);
                }
            }

            if (_mapButton.text != label)
            {
                _mapButton.text = label;
            }

            _mapButton.style.display = _hud.IsVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void RemoveMapButton()
        {
            if (_mapButton != null)
            {
                _mapButton.RemoveFromHierarchy();
                _mapButton = null;
            }
        }
    }
}
