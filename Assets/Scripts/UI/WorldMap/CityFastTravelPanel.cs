using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.WorldMap
{
    /// <summary>One destination row of the carro de boi window (pure, tested).</summary>
    public readonly struct FastTravelRow
    {
        public readonly CitySpec City;
        public readonly FastTravelCheck Check;
        public readonly int Roads;
        public readonly int Cost;
        public readonly string Name;
        public readonly string Status;
        public readonly string ButtonText;

        public FastTravelRow(CitySpec city, FastTravelCheck check, int roads, int cost, string name, string status, string buttonText)
        {
            City = city;
            Check = check;
            Roads = roads;
            Cost = cost;
            Name = name;
            Status = status;
            ButtonText = buttonText;
        }

        public bool CanTravel => Check == FastTravelCheck.Ok;
    }

    /// <summary>
    /// "Carro de boi" of the city hub (<see cref="VillageHubButton"/> → Serviços): one row per city with where it is, what
    /// the trip costs and why it cannot go yet (<see cref="FastTravelRules"/>). Built in code on the map HUD's document
    /// like the Fogão a lenha, with <c>Resources/UI/CityHub.uss</c>; pays from the hub's live
    /// <see cref="CarriedInventorySession"/> bag and hands the destination back to the hub, which moves the token.
    /// </summary>
    [DisallowMultipleComponent]
    public class CityFastTravelPanel : MonoBehaviour
    {
        public const string OverlayName = "wm-fast-travel-overlay";
        public const string StyleResourcePath = "UI/CityHub";
        public const string GoButtonPrefix = "wm-fast-travel-go-";
        public const string CurrencyItemId = "item_gold";

        private UIDocument _hudDocument;
        private VisualElement _overlay;
        private Label _titleLabel;
        private Label _subtitleLabel;
        private Label _goldLabel;
        private ScrollView _list;
        private Label _statusLabel;
        private Button _closeButton;

        private InventoryManager _bag;
        private WorldMapController _map;
        private Action<BiomeType, int> _onTravel;
        private bool _open;

        public bool IsOpen => _open;

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        // ------------------------------------------------------------------
        // Pure
        // ------------------------------------------------------------------

        /// <summary>The row for travelling from <paramref name="from"/> to <paramref name="city"/>.</summary>
        public static FastTravelRow BuildRow(CitySpec city, BiomeType from, bool tokenIdleInCity, Func<BiomeType, bool> isVisited,
            Func<BiomeType, bool> isUnlocked, Func<BiomeType, IEnumerable<BiomeType>> neighbours, int gold, LocalizationManager localization)
        {
            return BuildRow(city, from, tokenIdleInCity, isVisited, isUnlocked, neighbours, gold, localization, null, false);
        }

        /// <summary>As above with the viagem rápida gates: the destination's boss beaten (<paramref name="isBossCleared"/>) and no event going on.</summary>
        public static FastTravelRow BuildRow(CitySpec city, BiomeType from, bool tokenIdleInCity, Func<BiomeType, bool> isVisited,
            Func<BiomeType, bool> isUnlocked, Func<BiomeType, IEnumerable<BiomeType>> neighbours, int gold, LocalizationManager localization,
            Func<BiomeType, bool> isBossCleared, bool eventActive)
        {
            int roads = FastTravelRules.Roads(neighbours, from, city.Region);
            int cost = FastTravelRules.Cost(roads);
            FastTravelCheck check = FastTravelRules.Check(from, city.Region, tokenIdleInCity, isVisited, isUnlocked, roads, gold, isBossCleared, eventActive);

            string status;
            switch (check)
            {
                case FastTravelCheck.SameCity: status = CityTexts.Get(localization, CityTexts.FastTravelHere); break;
                case FastTravelCheck.NotVisited: status = CityTexts.Get(localization, CityTexts.FastTravelNotVisited); break;
                case FastTravelCheck.Locked: status = CityTexts.Get(localization, CityTexts.FastTravelLocked); break;
                case FastTravelCheck.NoRoute: status = CityTexts.Get(localization, CityTexts.FastTravelNoRoute); break;
                case FastTravelCheck.NotEnoughGold: status = CityTexts.Format(localization, CityTexts.FastTravelNoGold, cost); break;
                case FastTravelCheck.NotInCity: status = CityTexts.Get(localization, CityTexts.FastTravelNotInCity); break;
                case FastTravelCheck.BossNotDefeated: status = CityTexts.Get(localization, CityTexts.FastTravelBossLocked); break;
                case FastTravelCheck.EventActive: status = CityTexts.Get(localization, CityTexts.FastTravelEventActive); break;
                default: status = CityTexts.Format(localization, CityTexts.FastTravelRoads, roads); break;
            }

            return new FastTravelRow(city, check, roads, cost, CityTexts.CityName(localization, city), status,
                CityTexts.Format(localization, CityTexts.FastTravelGo, cost));
        }

        /// <summary>The region's boss was beaten (persisted flag or defeated registry); null without a map.</summary>
        public static Func<BiomeType, bool> BossClearedLookup(WorldMapController map)
        {
            return map != null && map.RegionBosses != null ? map.RegionBosses.IsRegionCleared : (Func<BiomeType, bool>)null;
        }

        /// <summary>A road event card is up, an encounter is pending or the token is walking: no cart now.</summary>
        public static bool IsEventActive(WorldMapController map)
        {
            WorldEventCardController card = FindFirstObjectByType<WorldEventCardController>();
            bool pending = ServiceLocator.TryGet(out SceneTransitionService transition) && transition.HasPendingEncounter;
            return FastTravelRules.IsEventActive(card != null && card.IsOpen, pending, map != null && map.IsTraveling);
        }

        // ------------------------------------------------------------------
        // Window
        // ------------------------------------------------------------------

        private void Awake()
        {
            _hudDocument = GetComponent<UIDocument>();
        }

        private void OnDisable()
        {
            Close();
            _overlay?.RemoveFromHierarchy();
            _overlay = null;
        }

        /// <summary>Opens the window paying from <paramref name="bag"/>; <paramref name="onTravel"/> gets the destination and the gold paid.</summary>
        public void Open(InventoryManager bag, WorldMapController map, Action<BiomeType, int> onTravel)
        {
            if (_open || bag == null || map == null || !EnsureOverlay())
            {
                return;
            }

            _bag = bag;
            _map = map;
            _onTravel = onTravel;
            _open = true;
            _statusLabel.text = string.Empty;
            _overlay.style.display = DisplayStyle.Flex;
            GameplayPause.Acquire(this);
            Rebuild();
        }

        public void Close()
        {
            if (_overlay != null)
            {
                _overlay.style.display = DisplayStyle.None;
            }

            _open = false;
            _bag = null;
            _onTravel = null;
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
            _overlay = new VisualElement { name = OverlayName };
            _overlay.AddToClassList("wm-backpack-overlay");
            _overlay.style.display = DisplayStyle.None;
            var sheet = Resources.Load<StyleSheet>(StyleResourcePath);
            if (sheet != null)
            {
                _overlay.styleSheets.Add(sheet);
            }

            var panel = new VisualElement { name = "wm-fast-travel-panel" };
            panel.AddToClassList("rap-panel");
            panel.AddToClassList("wm-backpack-panel");
            panel.AddToClassList("fast-travel-panel");
            _overlay.Add(panel);

            _titleLabel = AddLabel(panel, "wm-backpack-title");
            _subtitleLabel = AddLabel(panel, "wm-backpack-hint");
            _goldLabel = AddLabel(panel, "fast-travel-gold");
            _list = new ScrollView(ScrollViewMode.Vertical) { name = "wm-fast-travel-list" };
            _list.AddToClassList("fast-travel-list");
            panel.Add(_list);
            _statusLabel = AddLabel(panel, "fast-travel-status");

            _closeButton = new Button(Close) { name = "wm-fast-travel-close-button", focusable = true };
            _closeButton.AddToClassList("rap-button");
            _closeButton.AddToClassList("rap-button--primary");
            _closeButton.AddToClassList("wm-backpack-close");
            panel.Add(_closeButton);

            root.Add(_overlay);
            return true;
        }

        private static Label AddLabel(VisualElement parent, string className)
        {
            var label = new Label();
            label.AddToClassList(className);
            parent.Add(label);
            return label;
        }

        private void Rebuild()
        {
            LocalizationManager localization = Localization;
            CityProgress cities = CityProgress.ResolveShared();
            WorldMapProgress progress = _map != null ? _map.Progress : null;
            WorldMapGraph graph = _map != null ? _map.Graph : null;
            BiomeType from = _map != null && _map.CurrentNode != null ? _map.CurrentNode.Region : BiomeType.Forest;
            bool inCity = _map != null && _map.CanExplore && WorldMapCityPresence.Current != null
                ? WorldMapCityPresence.Current.TokenInSafeZone
                : _map != null && _map.CanExplore;
            int gold = _bag != null ? _bag.GetTotalCount(CurrencyItemId) : 0;

            _titleLabel.text = CityTexts.Get(localization, CityTexts.FastTravelTitle);
            _subtitleLabel.text = CityTexts.Get(localization, CityTexts.FastTravelSubtitle);
            _goldLabel.text = CityTexts.Format(localization, CityTexts.FastTravelGold, gold);
            _closeButton.text = CityTexts.Get(localization, CityTexts.FastTravelClose);

            Func<BiomeType, bool> bossCleared = BossClearedLookup(_map);
            bool eventActive = IsEventActive(_map);
            _list.Clear();
            Button first = null;
            foreach (CitySpec city in CityCatalog.Cities)
            {
                FastTravelRow row = BuildRow(city, from, inCity, cities.IsVisited,
                    progress != null ? progress.IsUnlocked : (Func<BiomeType, bool>)null,
                    graph != null ? graph.GetNeighbours : (Func<BiomeType, IEnumerable<BiomeType>>)null, gold, localization,
                    bossCleared, eventActive);

                var element = new VisualElement { name = "wm-fast-travel-row-" + city.CityId };
                element.AddToClassList("fast-travel-row");
                element.EnableInClassList("fast-travel-row--here", row.Check == FastTravelCheck.SameCity);
                element.EnableInClassList("fast-travel-row--closed", row.Check == FastTravelCheck.NotVisited || row.Check == FastTravelCheck.Locked
                    || row.Check == FastTravelCheck.BossNotDefeated);

                var body = new VisualElement();
                body.AddToClassList("fast-travel-row__body");
                element.Add(body);
                body.Add(NewLabel(row.Name, "fast-travel-row__name"));
                body.Add(NewLabel(row.Status, "fast-travel-row__status"));

                if (row.Check != FastTravelCheck.SameCity)
                {
                    BiomeType destination = city.Region;
                    var go = new Button(() => OnTravelClicked(destination)) { name = GoButtonPrefix + city.CityId, focusable = true, text = row.ButtonText };
                    go.AddToClassList("rap-button");
                    go.AddToClassList("fast-travel-go");
                    go.SetEnabled(row.CanTravel);
                    element.Add(go);
                    if (first == null && row.CanTravel)
                    {
                        first = go;
                    }
                }

                _list.Add(element);
            }

            (first ?? _closeButton).Focus();
        }

        private static Label NewLabel(string text, string className)
        {
            var label = new Label(text);
            label.AddToClassList(className);
            return label;
        }

        private void OnTravelClicked(BiomeType destination)
        {
            if (!_open || _bag == null || _map == null)
            {
                return;
            }

            CitySpec city = CityCatalog.ForRegion(destination);
            BiomeType from = _map.CurrentNode != null ? _map.CurrentNode.Region : destination;
            int gold = _bag.GetTotalCount(CurrencyItemId);
            FastTravelRow row = BuildRow(city, from, _map.CanExplore, CityProgress.ResolveShared().IsVisited,
                _map.Progress != null ? _map.Progress.IsUnlocked : (Func<BiomeType, bool>)null,
                _map.Graph != null ? _map.Graph.GetNeighbours : (Func<BiomeType, IEnumerable<BiomeType>>)null, gold, Localization,
                BossClearedLookup(_map), IsEventActive(_map));
            if (!row.CanTravel || (row.Cost > 0 && !_bag.RemoveById(CurrencyItemId, row.Cost)))
            {
                _statusLabel.text = row.Status;
                Rebuild();
                return;
            }

            Action<BiomeType, int> onTravel = _onTravel;
            Close();
            onTravel?.Invoke(destination, row.Cost);
        }
    }
}
