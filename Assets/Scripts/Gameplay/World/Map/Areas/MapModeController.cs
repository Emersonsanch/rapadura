using System;
using System.Collections;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.World.Map.Areas
{
    /// <summary>
    /// "Map mode" of Main.unity (Docs/Mapas100.md, TODO.md "Pipeline dos 100 Mapas"): when the scene loads with a pending
    /// map visit (<see cref="SceneTransitionService.TravelToMap"/>: "Mover" in the Barcas de Luz window, or walking across a
    /// border), this hides the arena's scenery and the sandbox (like <see cref="CityModeController"/>), builds the
    /// playable map (<see cref="MapAreaView"/>: settlement, greybox, wild props, exits, spawners), switches weather and
    /// ambience to the map's region, stands the hero at the dock (or at the border they walked in by) and resolves the
    /// map's exits. A defeat wakes the hero in the map's return city (<see cref="RespawnHere"/> when the map hosts it,
    /// else the Death screen loads that city, Docs/Progressao500.md §7). Self-bootstrapping on scene load: no scene edit;
    /// without a pending map visit nothing happens, so encounters, city walks and the sandbox are untouched.
    /// </summary>
    public sealed class MapModeController : MonoBehaviour
    {
        private const string LogCategory = "MapMode";
        private const string PlayerTag = "Player";
        private const string CarriedStatsKey = "PlayerStats";

        public static bool AutoCreate = true;

        public static MapModeController Current { get; private set; }

        /// <summary>The hero reached the Cais da Barca of the map: the travel window (UI) opens.</summary>
        public static event Action DockReached;

        private bool _leaving;

        public string MapId { get; private set; }
        public string ArrivingFrom { get; private set; }
        public MapDefinition Definition { get; private set; }
        public MapLayout Layout { get; private set; }
        public MapAreaView Area { get; private set; }

        /// <summary>
        /// The continuous region the map belongs to (Docs/Mapas100.md §0.10: Região 1 is one overworld), or null when the map
        /// is its own scene. While set, <see cref="MapId"/> follows the area the hero walks in.
        /// </summary>
        public OverworldLayout Overworld { get; private set; }

        public OverworldView OverworldArea { get; private set; }

        /// <summary>The town of the map's return city built in the scene (the map's own settlement, or the region's Vila).</summary>
        public CityTownView Town => Area != null ? Area.Town : OverworldArea != null ? OverworldArea.Vila : null;

        /// <summary>True once the map is built and the hero stands at the arrival point.</summary>
        public bool IsReady { get; private set; }

        /// <summary>The map hosts its own return city (01: the Vila): a defeat wakes the hero at this plaza.</summary>
        public bool CanRespawnHere => Layout != null && Layout.SettlementIsReturnCity && Town != null;

        public int HeroLevel
        {
            get
            {
                GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
                PlayerStats stats = player != null ? player.GetComponent<PlayerStats>() : null;
                return stats != null ? Mathf.Max(1, stats.Level) : CarriedHeroLevel();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Ensure(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Ensure(scene);

        public static void Ensure(Scene scene)
        {
            if (!AutoCreate || !scene.IsValid() || !scene.isLoaded || Current != null || !ServiceLocator.TryGet(out SceneTransitionService transition))
            {
                return;
            }

            if (scene.name == transition.WorldMapSceneName)
            {
                // Back on the island map some other way (menu, a loaded save): the map visit is over.
                transition.ClearMapVisit();
                return;
            }

            if (!transition.HasPendingMapVisit || transition.HasPendingEncounter || transition.HasPendingCityVisit || scene.name != transition.CombatSceneName)
            {
                return;
            }

            var host = new GameObject("[MapMode]");
            SceneManager.MoveGameObjectToScene(host, scene);
            MapModeController controller = host.AddComponent<MapModeController>();
            controller.MapId = transition.PendingMapVisit;
            controller.ArrivingFrom = transition.PendingMapArrivalFrom;
        }

        private void Awake()
        {
            Current = this;
        }

        private void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
        }

        private IEnumerator Start()
        {
            Definition = MapCatalog.Get(MapId);
            Overworld = OverworldLayouts.ForMap(MapId);
            Layout = Overworld != null ? Overworld.World : MapLayouts.For(MapId);
            if (Definition == null || Layout == null)
            {
                GameLogger.Error(LogCategory, $"Map '{MapId}' has no built layout; back to the island map.");
                LeaveToWorldMap();
                yield break;
            }

            HideArena();
            if (Overworld != null)
            {
                OverworldArea = OverworldView.Build(Overworld, ResolveEnemyPrefab());
                SceneManager.MoveGameObjectToScene(OverworldArea.gameObject, gameObject.scene);
                OverworldArea.ZoneChanged += OnOverworldZoneChanged;
            }
            else
            {
                Area = MapAreaView.Build(Layout, ResolveEnemyPrefab());
                SceneManager.MoveGameObjectToScene(Area.gameObject, gameObject.scene);
            }

            SyncEnvironment(Layout.Biome);
            if (CanRespawnHere && ServiceLocator.TryGet(out CheckpointManager checkpoints))
            {
                checkpoints.SetCityRespawn(Town.Respawn);
            }

            if (ServiceLocator.TryGet(out WorldMapProgress progress) && Definition.LegacyBiome.HasValue)
            {
                // The island-map token (and the "current region" a defeat counts in) follows the barca.
                progress.SetCurrentRegion(Definition.LegacyBiome.Value);
                if (Definition.HasCity)
                {
                    CityProgress.ResolveShared().MarkVisited(Definition.LegacyBiome.Value);
                }
            }

            // The carried state (bag, level) is restored one frame after load: place the hero after it, twice.
            PlaceHero();
            yield return null;
            PlaceHero();
            IsReady = true;
            if (Overworld != null)
            {
                GameLogger.Info(LogCategory, $"Overworld {Overworld.Id} ({Overworld.Bounds.width:0}x{Overworld.Bounds.height:0} m, {Overworld.Zones.Count} areas, "
                    + $"{Overworld.World.Zones.Count} spawn clearings): {OverworldArea.LoadedChunkCount} chunks around the hero.");
                yield break;
            }

            AnnounceArrival();
            GameLogger.Info(LogCategory, $"Map {MapId}: {Area.WildPropCount} wild props, {Area.RiverBlockerCount} river blockers, "
                + $"{Area.Spawners.Count} spawners, solid pass: {Area.SolidReport}.");
        }

        private GameObject ResolveEnemyPrefab()
        {
            EncounterArenaController arena = FindAnyObjectByType<EncounterArenaController>(FindObjectsInactive.Include);
            if (arena == null || arena.EnemyPrefab == null)
            {
                GameLogger.Warning(LogCategory, "No EncounterArenaController enemy prefab in the scene: the map has no monsters.");
                return null;
            }

            return arena.EnemyPrefab;
        }

        private void HideArena()
        {
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            {
                if (Array.IndexOf(CityModeController.HiddenSceneRoots, root.name) >= 0)
                {
                    root.SetActive(false);
                }
            }
        }

        private static void SyncEnvironment(BiomeType biome)
        {
            if (ServiceLocator.TryGet(out WeatherSystem weather) && weather != null && weather.CurrentBiome != biome)
            {
                weather.SetBiome(biome);
                weather.ForceWeather(weather.CurrentWeather, instant: true);
            }

            AmbientVfx ambient = FindAnyObjectByType<AmbientVfx>();
            if (ambient != null && ambient.Biome != biome)
            {
                ambient.SetBiome(biome);
            }
        }

        public void PlaceHero()
        {
            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            if (player == null || Layout == null)
            {
                return;
            }

            Vector2 facing;
            Vector2 at = Overworld != null ? Overworld.ArrivalFor(MapId, ArrivingFrom, out facing) : Layout.ArrivalFrom(ArrivingFrom, out facing);
            PlaceInWorld(player, at, facing);
        }

        /// <summary>Stands the hero at <paramref name="at"/>; in an overworld the chunks around are built first and the zone re-read.</summary>
        private void PlaceInWorld(GameObject player, Vector2 at, Vector2 facing)
        {
            if (OverworldArea != null)
            {
                OverworldArea.WarmUp(at);
                OverworldArea.ResetTracking(at);
            }

            CityModeController.PlaceAt(player, at, facing);
        }

        private void OnOverworldZoneChanged(OverworldZone zone)
        {
            string mapId = zone != null ? zone.MapIdOrOwner : null;
            MapDefinition map = MapCatalog.Get(mapId);
            if (map == null)
            {
                return;
            }

            MapId = mapId;
            Definition = map;
        }

        /// <summary>
        /// The Barcas de Luz inside a continuous region: the hero is carried to the dock of <paramref name="mapId"/> in the same
        /// scene (no reload). False when the map is not in this overworld (the usual scene trip then happens).
        /// </summary>
        public bool TryTravelWithin(string mapId)
        {
            if (Overworld == null || !IsReady || _leaving || !Overworld.ContainsMap(mapId))
            {
                return false;
            }

            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            if (player == null)
            {
                return false;
            }

            Vector2 at = Overworld.ArrivalFor(mapId, null, out Vector2 facing);
            PlaceInWorld(player, at, facing);
            ServiceLocator.TryGet(out LocalizationManager localization);
            MapDefinition map = MapCatalog.Get(mapId);
            string name = Overworld.SettlementOf(mapId) != null ? MapTexts.SettlementName(localization, map) : MapTexts.MapName(localization, map);
            CombatFxManager.SpawnLootText(player.transform.position + Vector3.up * 3.1f, OverworldTexts.Format(localization, OverworldTexts.BarcaArrived, name));
            return true;
        }

        /// <summary>The map is a dock of this overworld the hero has not reached on foot yet (the barca refuses it).</summary>
        public bool IsUnvisitedStop(string mapId)
        {
            return Overworld != null && Overworld.ContainsMap(mapId) && !OverworldProgress.IsVisited(mapId);
        }

        private void AnnounceArrival()
        {
            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            if (player == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            string line = MapTexts.Format(localization, MapTexts.Arrived, MapTexts.MapName(localization, Definition), Definition.MonsterLevelMin, Definition.MonsterLevelMax);
            CombatFxManager.SpawnLootText(player.transform.position + Vector3.up * 2.4f, line);
            if (MapAccessRules.GetMonsterLevelDeltaPenalty(HeroLevel, Definition.MonsterLevelMax).MonsterCritChanceBonus > 0f)
            {
                CombatFxManager.SpawnLootWarningText(player.transform.position + Vector3.up * 3.1f, MapTexts.Get(localization, MapTexts.DangerWarning));
            }
        }

        /// <summary>A defeat on a map that hosts its return city: the hero wakes at the plaza. True when placed.</summary>
        public bool RespawnHere()
        {
            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            if (player == null || !CanRespawnHere)
            {
                return false;
            }

            if (Overworld != null)
            {
                // The region's return city for the map the hero fell on (Copas de Garajonay for 05-06), else the Vila.
                string cityId = MapCatalog.ReturnCityOf(MapId);
                OverworldSettlement home = Overworld.Settlements.Find(s => s.IsReturnCity && s.Id == cityId) ?? Overworld.Vila;
                PlaceInWorld(player, home.Town.Respawn + home.Offset, Vector2.down);
                return true;
            }

            CityModeController.PlaceAt(player, Layout.Town.Respawn, Vector2.down);
            return true;
        }

        // ------------------------------------------------------------------
        // Exits
        // ------------------------------------------------------------------

        /// <summary>Called by a <see cref="MapExitTrigger"/> the hero walked into.</summary>
        public static void HandleExit(MapExitTrigger exit)
        {
            MapModeController current = Current;
            if (current == null || !current.IsReady || current._leaving || exit == null || exit.Spec == null)
            {
                return;
            }

            current.OnExit(exit);
        }

        private void OnExit(MapExitTrigger exit)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            Vector3 above = player != null ? player.transform.position + Vector3.up * 2.4f : Vector3.zero;
            MapExitSpec spec = exit.Spec;
            switch (spec.Kind)
            {
                case MapExitKind.Dock:
                    exit.Rearm();
                    if (player != null)
                    {
                        CombatFxManager.SpawnLootText(above, MapTexts.Get(localization, MapTexts.DockCallout));
                    }

                    DockReached?.Invoke();
                    return;

                case MapExitKind.WorldMap:
                    if (player != null)
                    {
                        CombatFxManager.SpawnLootText(above, MapTexts.Get(localization, MapTexts.ExitToWorldMap));
                    }

                    LeaveToWorldMap();
                    return;

                case MapExitKind.Map:
                    MapDefinition target = MapCatalog.Get(spec.TargetMapId);
                    CharacterId? hero = ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;
                    Rapadura.Gameplay.Inventory.InventoryManager bag = player != null ? player.GetComponentInChildren<Rapadura.Gameplay.Inventory.InventoryManager>() : null;
                    MapAccessCheck check = MapAccessRules.CheckWalk(target, HeroLevel, hero, MapRequirementHeroState.LookupFor(bag));
                    if (check == MapAccessCheck.Ok && MapLayouts.Has(spec.TargetMapId))
                    {
                        WalkTo(spec.TargetMapId);
                        return;
                    }

                    string line = check == MapAccessCheck.LevelTooLow
                        ? MapTexts.Format(localization, MapTexts.WalkBlocked, MapAccessRules.EffectiveLevelRequirement(target.WalkLevelRequirement, hero, target.RegionType))
                        : check == MapAccessCheck.MissingRequirement
                            ? MapTexts.Format(localization, MapTexts.StatusRequirement, MapTexts.RequirementName(localization, target.SpecialRequirementTag))
                            : MapTexts.Get(localization, MapTexts.ExitClosed);
                    if (player != null)
                    {
                        CombatFxManager.SpawnLootWarningText(above, line);
                        CityModeController.PlaceAt(player, spec.Arrival, spec.ArrivalFacing);
                    }

                    exit.Rearm();
                    return;
            }
        }

        /// <summary>On foot into <paramref name="mapId"/> (the walk level was checked): arrival at its border with this map.</summary>
        public void WalkTo(string mapId)
        {
            if (_leaving || !ServiceLocator.TryGet(out SceneTransitionService transition))
            {
                return;
            }

            _leaving = true;
            if (!transition.TravelToMap(mapId, MapId))
            {
                _leaving = false;
            }
        }

        /// <summary>Back to the island map (an exit, or an unknown map).</summary>
        public void LeaveToWorldMap()
        {
            if (_leaving)
            {
                return;
            }

            _leaving = true;
            if (ServiceLocator.TryGet(out SceneTransitionService transition) && !transition.EndMapVisit())
            {
                transition.ReturnToWorldMap();
            }
        }

        // ------------------------------------------------------------------
        // Hero level outside a scene Player (window opened on the island map)
        // ------------------------------------------------------------------

        /// <summary>The hero level carried between scenes (no Player in the scene: the island map).</summary>
        public static int CarriedHeroLevel()
        {
            if (!ServiceLocator.TryGet(out SaveManager saves) || !saves.TryGetCarriedJson(CarriedStatsKey, out string json) || string.IsNullOrEmpty(json))
            {
                return 1;
            }

            try
            {
                PlayerStatsSaveState state = JsonUtility.FromJson<PlayerStatsSaveState>(json);
                return state != null ? Mathf.Max(1, state.level) : 1;
            }
            catch (ArgumentException)
            {
                return 1;
            }
        }
    }
}
