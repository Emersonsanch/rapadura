using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Dressing;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.World.Cities
{
    /// <summary>
    /// The cities on the world map (WorldMap.unity), self-bootstrapping on the scene's <see cref="WorldMapController"/>
    /// (no scene edit):
    /// <list type="bullet">
    /// <item>Zona segura: installs <see cref="WorldMapController.SafeZoneProbe"/>, so while the token is within
    /// <see cref="SafeZoneRules.MapCityRadius"/> of a region node no random encounter or road event happens, and keeps
    /// <see cref="CityUnderToken"/> up to date for the HUD's "Você está em zona segura".</item>
    /// <item>Visits: the token idle on a node means the cousins are in that city; the first time it becomes a stop of
    /// the Barcas de Luz (<see cref="CityProgress.MarkVisited"/>, <see cref="NewCityVisited"/>).</item>
    /// <item>Landmark: once the <see cref="WorldMapDressing"/> is built, adds the city limit ring with its gates and the
    /// town houses (<see cref="WorldMapCityDressing"/>) under the dressing, with its palette material.</item>
    /// </list>
    /// Main.unity has no controller, so nothing of this exists in the arena.
    /// </summary>
    [DisallowMultipleComponent]
    public class WorldMapCityPresence : MonoBehaviour
    {
        public const string DressingRootName = "Cities";

        private WorldMapController _controller;
        private Func<Vector3, bool> _probe;
        private bool _dressed;
        private GameObject _dressingRoot;
        private readonly List<Mesh> _meshes = new List<Mesh>();

        /// <summary>The live presence (null outside the world map).</summary>
        public static WorldMapCityPresence Current { get; private set; }

        /// <summary>Raised the first time the cousins reach a city (not for the starting one).</summary>
        public event Action<CitySpec> NewCityVisited;

        /// <summary>The city whose safe zone the token is in, or null.</summary>
        public CitySpec CityUnderToken { get; private set; }

        public bool TokenInSafeZone => CityUnderToken != null;

        public WorldMapController Controller => _controller;

        public bool IsDressed => _dressed;

        // ------------------------------------------------------------------
        // Bootstrap
        // ------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Attach();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Attach();

        private static void Attach()
        {
            WorldMapController controller = FindFirstObjectByType<WorldMapController>();
            if (controller != null && controller.GetComponent<WorldMapCityPresence>() == null)
            {
                controller.gameObject.AddComponent<WorldMapCityPresence>();
            }
        }

        private void Awake()
        {
            _controller = GetComponent<WorldMapController>();
            _probe = IsInSafeZone;
        }

        private void OnEnable()
        {
            Current = this;
            if (_controller != null)
            {
                _controller.SafeZoneProbe = _probe;
            }
        }

        private void OnDisable()
        {
            if (_controller != null && _controller.SafeZoneProbe == _probe)
            {
                _controller.SafeZoneProbe = null;
            }

            if (Current == this)
            {
                Current = null;
            }
        }

        private void OnDestroy()
        {
            foreach (Mesh mesh in _meshes)
            {
                if (mesh != null)
                {
                    Destroy(mesh);
                }
            }

            _meshes.Clear();
        }

        private void Update()
        {
            if (_controller == null)
            {
                return;
            }

            Transform token = _controller.PlayerToken;
            CityUnderToken = token != null ? CityAt(token.position) : CityFor(_controller.CurrentNode);

            if (_controller.CanExplore && _controller.CurrentNode != null)
            {
                CitySpec city = CityCatalog.ForRegion(_controller.CurrentNode.Region);
                if (city != null && CityProgress.ResolveShared().MarkVisited(city.Region))
                {
                    NewCityVisited?.Invoke(city);
                }
            }

            if (!_dressed)
            {
                TryDress();
            }
        }

        // ------------------------------------------------------------------
        // Safe zone
        // ------------------------------------------------------------------

        /// <summary>Whether <paramref name="nodePosition"/>'s city (flat distance, radius <see cref="SafeZoneRules.MapCityRadius"/>) contains <paramref name="position"/>.</summary>
        public static ZoneKind ZoneAround(Vector3 position, Vector3 nodePosition)
        {
            float dx = position.x - nodePosition.x;
            float dz = position.z - nodePosition.z;
            return SafeZoneRules.ZoneAtDistance(Mathf.Sqrt(dx * dx + dz * dz), SafeZoneRules.MapCityRadius);
        }

        /// <summary>The city whose limit contains <paramref name="position"/>, or null (wilds, roads).</summary>
        public CitySpec CityAt(Vector3 position)
        {
            if (_controller == null || _controller.Nodes == null)
            {
                return null;
            }

            foreach (WorldMapRegionNode node in _controller.Nodes)
            {
                if (node != null && SafeZoneRules.IsSafe(ZoneAround(position, node.Position)))
                {
                    CitySpec city = CityCatalog.ForRegion(node.Region);
                    if (city != null)
                    {
                        return city;
                    }
                }
            }

            return null;
        }

        public bool IsInSafeZone(Vector3 position) => CityAt(position) != null;

        private static CitySpec CityFor(WorldMapRegionNode node) => node != null ? CityCatalog.ForRegion(node.Region) : null;

        // ------------------------------------------------------------------
        // Dressing
        // ------------------------------------------------------------------

        private void TryDress()
        {
            WorldMapDressing dressing = FindAnyObjectByType<WorldMapDressing>();
            if (dressing == null || !dressing.IsBuilt || dressing.Terrain == null || dressing.PaletteMaterial == null)
            {
                return;
            }

            _dressed = true;
            WorldMapTerrain terrain = dressing.Terrain;
            _dressingRoot = new GameObject(DressingRootName);
            _dressingRoot.transform.SetParent(dressing.transform, false);

            var builder = new WorldMapMeshBuilder();
            for (int i = 0; i < terrain.Sites.Count; i++)
            {
                WorldMapSite site = terrain.Sites[i];
                if (!CityCatalog.HasCity(site.Region))
                {
                    continue;
                }

                builder.Clear();
                WorldMapCityDressing.AppendSafeRing(builder, SafeZoneRules.MapCityRadius, WorldMapCityDressing.RoadAngles(terrain, i));
                Spawn("CityLimit_" + site.Region, builder, new Vector3(site.Position.x, terrain.Height(site.Position), site.Position.y), 0f, dressing.PaletteMaterial);

                Vector2 landmark = WorldMapDressingLayout.LandmarkPosition(terrain, i);
                Vector2 spot = WorldMapCityDressing.TownSpot(terrain, i, landmark);
                builder.Clear();
                WorldMapCityDressing.AppendTown(builder, site.Region);
                // Houses face the node (and so, roughly, the camera side of the plaza).
                Vector2 toNode = site.Position - spot;
                float yaw = Mathf.Atan2(toNode.x, toNode.y) * Mathf.Rad2Deg + 180f;
                Spawn("Town_" + site.Region, builder, new Vector3(spot.x, terrain.Height(spot) - 0.03f, spot.y), yaw, dressing.PaletteMaterial);
            }
        }

        private void Spawn(string objectName, WorldMapMeshBuilder builder, Vector3 localPosition, float yaw, Material material)
        {
            if (builder.IsEmpty)
            {
                return;
            }

            Mesh mesh = builder.ToMesh("WorldMap" + objectName);
            _meshes.Add(mesh);
            var go = new GameObject(objectName);
            go.transform.SetParent(_dressingRoot.transform, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        }
    }
}
