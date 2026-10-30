using System.Collections;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// "City mode" of Main.unity (Docs/Cities.md §5): when the scene loads with a pending city visit
    /// (<see cref="SceneTransitionService.BeginCityVisit"/>, the hub's "Andar pela cidade"), this hides the arena's
    /// scenery and the sandbox (enemies, test NPCs), builds the walkable town of that city (<see cref="CityTownView"/>),
    /// walks the hero in by the road from the map, makes the plaza the respawn point, switches weather and ambience to
    /// the region and leaves the day/night cycle running. The gates and the Cais da Barca send the hero back
    /// (<see cref="Leave"/>). Self-bootstrapping on scene load like <c>EventRoomDirector</c>: no scene edit. Without a
    /// pending visit nothing happens, so encounters and the sandbox are untouched.
    /// </summary>
    public sealed class CityModeController : MonoBehaviour
    {
        private const string LogCategory = "CityMode";
        private const string PlayerTag = "Player";

        /// <summary>Main.unity roots that are the arena (hidden in city mode; the scene unloads when leaving, so nothing to restore).</summary>
        public static readonly string[] HiddenSceneRoots = { "Sandbox", "Ground", "GroundDetail", "NaturalTrees", "Rocks", "Rock1", "Rock2" };

        public static bool AutoCreate = true;

        public static CityModeController Current { get; private set; }

        private bool _leaving;

        public string CityId { get; private set; }
        public CityLayout Layout { get; private set; }
        public CityTownView Town { get; private set; }

        /// <summary>True once the town is built and the hero stands at the arrival point.</summary>
        public bool IsReady { get; private set; }

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
            if (!AutoCreate || !scene.IsValid() || !scene.isLoaded || Current != null)
            {
                return;
            }

            if (!ServiceLocator.TryGet(out SceneTransitionService transition) || !transition.HasPendingCityVisit || scene.name != transition.CombatSceneName)
            {
                return;
            }

            var host = new GameObject("[CityMode]");
            SceneManager.MoveGameObjectToScene(host, scene);
            host.AddComponent<CityModeController>().CityId = transition.PendingCityVisit;
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
            CitySpec city = CityCatalog.Get(CityId);
            if (city == null)
            {
                GameLogger.Error(LogCategory, $"Unknown city '{CityId}'; back to the map.");
                Leave(CityExitKind.ToMap);
                yield break;
            }

            HideArena();

            bool mascate = CityShopService.ShopsOpenIn(city, CityShopService.Now, ServiceLocator.TryGet(out WorldMapProgress map) ? map : null)
                .Contains(CityShopCatalog.MascateBento);
            Layout = CityTownRecipes.LayoutFor(CityId, mascate);
            Town = CityTownView.Build(Layout, null, CityId + (mascate ? "+mascate" : string.Empty));
            SceneManager.MoveGameObjectToScene(Town.gameObject, gameObject.scene);

            SyncEnvironment(city);

            if (ServiceLocator.TryGet(out CheckpointManager checkpoints))
            {
                checkpoints.ActivateCheckpoint(Town.Respawn);
            }

            // The carried state (bag, level) is restored one frame after load; place the hero after it, twice to beat
            // any spawn logic of the scene's own Start.
            PlaceHero();
            yield return null;
            PlaceHero();
            IsReady = true;

            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            if (player != null)
            {
                ServiceLocator.TryGet(out LocalizationManager localization);
                string name = CityTexts.CityName(localization, city);
                CombatFxManager.SpawnLootText(player.transform.position + Vector3.up * 2.4f, string.Format(CityWalkTexts.Get(localization, CityWalkTexts.Arrived), name));
            }

            GameLogger.Info(LogCategory, $"Walkable {CityId}: {Layout.Lots.Count} lots, {Town.RendererCount} renderers, {Town.ColliderCount} colliders, {Town.TriangleCount} triangles.");
        }

        private void HideArena()
        {
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            {
                if (System.Array.IndexOf(HiddenSceneRoots, root.name) >= 0)
                {
                    root.SetActive(false);
                }
            }
        }

        private static void SyncEnvironment(CitySpec city)
        {
            if (ServiceLocator.TryGet(out WeatherSystem weather) && weather != null && weather.CurrentBiome != city.Region)
            {
                weather.SetBiome(city.Region);
                weather.ForceWeather(weather.CurrentWeather, instant: true);
            }

            AmbientVfx ambient = FindAnyObjectByType<AmbientVfx>();
            if (ambient != null && ambient.Biome != city.Region)
            {
                ambient.SetBiome(city.Region);
            }
        }

        /// <summary>Stands the hero on the road in from the map, facing the plaza.</summary>
        public void PlaceHero()
        {
            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            if (player == null || Layout == null)
            {
                return;
            }

            PlaceAt(player, Layout.Arrival, Layout.ArrivalFacing);
        }

        /// <summary>Teleports the hero onto the town ground at <paramref name="ground"/> (tests use it to start walks at the plaza).</summary>
        public static void PlaceAt(GameObject player, Vector2 ground, Vector2 facing)
        {
            var position = new Vector3(ground.x, 0.05f, ground.y);
            Quaternion rotation = facing.sqrMagnitude > 0.001f ? Quaternion.LookRotation(new Vector3(facing.x, 0f, facing.y)) : player.transform.rotation;
            if (player.TryGetComponent(out PlayerMotor motor))
            {
                motor.Teleport(position, rotation);
                motor.SnapToGround(~0);
            }
            else
            {
                player.transform.SetPositionAndRotation(position, rotation);
            }

            if (player.TryGetComponent(out PlayerClickToMove click))
            {
                click.ClearDestination();
            }
        }

        /// <summary>A gate or the Cais da Barca: back to the map (with the Barcas de Luz window for the dock).</summary>
        public static void Leave(CityExitKind kind)
        {
            CityModeController current = Current;
            if (current != null)
            {
                if (current._leaving)
                {
                    return;
                }

                current._leaving = true;
            }

            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            if (player != null)
            {
                ServiceLocator.TryGet(out LocalizationManager localization);
                string key = kind == CityExitKind.ShipDock ? CityWalkTexts.DockBoarding : CityWalkTexts.Leaving;
                CombatFxManager.SpawnLootText(player.transform.position + Vector3.up * 2.4f, CityWalkTexts.Get(localization, key));
            }

            if (ServiceLocator.TryGet(out SceneTransitionService transition))
            {
                transition.EndCityVisit(openFastTravel: kind == CityExitKind.ShipDock);
            }
        }
    }
}
