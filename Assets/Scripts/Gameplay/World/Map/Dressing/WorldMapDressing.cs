using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.World.Map.Dressing
{
    /// <summary>
    /// Dresses WorldMap.unity as a small low-poly JRPG overworld island, built once at load from the
    /// scene's <see cref="WorldMapRegionNode"/>s (same spirit as <see cref="BiomeArenaDressing"/>;
    /// added by <see cref="WorldMapDressingBootstrap"/>, no scene wiring):
    /// <list type="bullet">
    /// <item>one ground mesh (forest hills, desert dunes, mountain slopes, rocky caves; flat on
    /// roads and node plazas) coloured by a baked texture: blended biomes, dirt roads between
    /// connected nodes (the token's straight travel lines), a river with a bridge, beach, shallow
    /// water and open sea;</item>
    /// <item>per-region props laid out by <see cref="WorldMapDressingLayout"/> (seeded): the arena's
    /// tree prefabs, grass tufts and cacti (named so <see cref="EnvironmentPropAttacher"/> gives them
    /// wind sway), plus rocks, snowy rocks, peaks, stalagmites and crystals merged into one static
    /// mesh;</item>
    /// <item>a landmark per node (village, desert ruin, peak camp, cave mouth) replacing the old
    /// disc and sphere: grey fog veils it while the region is locked, a gold banner flies once it is
    /// cleared; an extra trigger collider under the node makes the landmark clickable too;</item>
    /// <item>sea out to the horizon and cloud banks past the coast instead of a hard edge.</item>
    /// </list>
    /// Cost: every procedural mesh samples one point-filtered palette texture through the same
    /// material (<see cref="WorldMapPalette"/>), so all static props are one draw call; two runtime
    /// materials in total (palette, ground); no Update, state changes come from world-map events.
    /// The authored ground plane, node discs and locked spheres are hidden (renderers only;
    /// colliders and the node/controller logic are untouched) and restored by <see cref="Restore"/>.
    /// </summary>
    public class WorldMapDressing : MonoBehaviour
    {
        private const string LogCategory = "WorldMap";
        public const int DefaultSeed = 20261029;

        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");
        private static readonly int GlossinessId = Shader.PropertyToID("_Glossiness");
        private static readonly int MetallicId = Shader.PropertyToID("_Metallic");

        [Tooltip("Layout seed: the same seed always gives the same map.")]
        [SerializeField] private int _seed = DefaultSeed;
        [SerializeField] private bool _river = true;
        [Tooltip("Multiplies every region's prop count.")]
        [SerializeField] [Range(0f, 2f)] private float _density = 1f;
        [Tooltip("Ground mesh grid spacing (world units).")]
        [SerializeField] [Min(0.25f)] private float _groundStep = 0.5f;
        [Tooltip("Width of the baked ground colour texture (height follows the island's aspect).")]
        [SerializeField] [Min(32)] private int _groundTextureWidth = 192;
        [Tooltip("Height of a scale-1 tree on the map (the hero token is ~3.6 tall).")]
        [SerializeField] [Min(0.5f)] private float _treeHeight = 2.6f;
        [SerializeField] private bool _castShadows = true;
        [Tooltip("Hide the authored placeholder visuals (ground plane, node discs, locked spheres). Colliders stay.")]
        [SerializeField] private bool _hideAuthoredMarkers = true;
        [Tooltip("Root objects whose renderer is the authored placeholder ground.")]
        [SerializeField] private string[] _authoredGroundNames = { "MapGround" };
        [Tooltip("Optional; empty = Resources/" + WorldMapDressingLibrary.ResourcePath + ".")]
        [SerializeField] private WorldMapDressingLibrary _library;

        private sealed class NodeDressing
        {
            public WorldMapRegionNode Node;
            public BiomeType Region;
            public GameObject Landmark;
            public GameObject Fog;
            public GameObject Banner;
            public Collider ClickArea;
        }

        private readonly List<NodeDressing> _nodeDressings = new List<NodeDressing>();
        private readonly List<Object> _owned = new List<Object>();
        private readonly List<Renderer> _hiddenRenderers = new List<Renderer>();
        private readonly List<GameObject> _swaying = new List<GameObject>();
        private readonly Dictionary<GameObject, float> _prefabHeights = new Dictionary<GameObject, float>();

        private WorldMapController _controller;
        private WorldMapTerrain _terrain;
        private List<WorldMapPropPlacement> _props = new List<WorldMapPropPlacement>();
        private List<WorldMapCloudPlacement> _clouds = new List<WorldMapCloudPlacement>();
        private WorldMapPropMeshes _meshes;
        private Material _paletteMaterial;
        private Material _groundMaterial;
        private Transform _root;
        private bool _built;

        public bool IsBuilt => _built;
        public WorldMapTerrain Terrain => _terrain;
        public IReadOnlyList<WorldMapPropPlacement> Props => _props;
        public IReadOnlyList<WorldMapCloudPlacement> Clouds => _clouds;
        public int SwayingPropCount => _swaying.Count;
        public int HiddenAuthoredRendererCount => _hiddenRenderers.Count;
        public Material PaletteMaterial => _paletteMaterial;
        public Material GroundMaterial => _groundMaterial;

        public int Seed
        {
            get => _seed;
            set => _seed = value;
        }

        /// <summary>Adds (once) and builds the dressing for <paramref name="controller"/>'s scene.</summary>
        public static WorldMapDressing Ensure(WorldMapController controller)
        {
            if (controller == null)
            {
                return null;
            }

            WorldMapDressing existing = FindAnyObjectByType<WorldMapDressing>();
            if (existing != null && existing.gameObject.scene == controller.gameObject.scene)
            {
                if (!existing._built)
                {
                    existing.Build(controller);
                }

                return existing;
            }

            var go = new GameObject("[WorldMapDressing]");
            if (controller.gameObject.scene.IsValid() && go.scene != controller.gameObject.scene)
            {
                SceneManager.MoveGameObjectToScene(go, controller.gameObject.scene);
            }

            var dressing = go.AddComponent<WorldMapDressing>();
            dressing.Build(controller);
            return dressing;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<WorldMapRegionUnlockedEvent>(OnRegionUnlocked);
            EventBus.Subscribe<WorldMapRegionClearedEvent>(OnRegionCleared);
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<WorldMapRegionUnlockedEvent>(OnRegionUnlocked);
            EventBus.Unsubscribe<WorldMapRegionClearedEvent>(OnRegionCleared);
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
        }

        private void Start()
        {
            // Placed in a scene by hand (instead of by the bootstrap): build for the scene's map.
            if (!_built)
            {
                WorldMapController controller = FindAnyObjectByType<WorldMapController>();
                if (controller != null)
                {
                    Build(controller);
                }
            }

            RefreshStates();
        }

        private void OnRegionUnlocked(WorldMapRegionUnlockedEvent _) => RefreshStates();

        private void OnRegionCleared(WorldMapRegionClearedEvent _) => RefreshStates();

        private void OnGameLoaded(GameLoadedEvent _) => RefreshStates();

        private void OnDestroy() => Restore();

        /// <summary>
        /// Removes everything the dressing built and shows the authored placeholders again (also run
        /// by OnDestroy; EditMode code must call it, OnDestroy does not run there). Safe to repeat.
        /// </summary>
        public void Restore()
        {
            for (int i = 0; i < _hiddenRenderers.Count; i++)
            {
                if (_hiddenRenderers[i] != null)
                {
                    _hiddenRenderers[i].enabled = true;
                }
            }

            _hiddenRenderers.Clear();

            // Click areas live under the (scene-owned) nodes.
            foreach (NodeDressing node in _nodeDressings)
            {
                if (node.ClickArea != null)
                {
                    SafeDestroy(node.ClickArea.gameObject);
                }
            }

            _nodeDressings.Clear();
            _meshes?.Dispose();
            _meshes = null;
            foreach (Object owned in _owned)
            {
                SafeDestroy(owned);
            }

            _owned.Clear();

            if (_root != null)
            {
                for (int i = _root.childCount - 1; i >= 0; i--)
                {
                    SafeDestroy(_root.GetChild(i).gameObject);
                }
            }

            _swaying.Clear();
            _props.Clear();
            _clouds.Clear();
            _prefabHeights.Clear();
            _terrain = null;
            _paletteMaterial = null;
            _groundMaterial = null;
            _built = false;
        }

        // ---------------------------------------------------------------
        // Build
        // ---------------------------------------------------------------

        public void Build(WorldMapController controller)
        {
            _controller = controller;
            WorldMapGraph graph = controller != null ? controller.Graph : null;
            Build(controller != null ? controller.Nodes : null, graph != null ? graph.AreConnected : (System.Func<BiomeType, BiomeType, bool>)null);
        }

        /// <summary>
        /// Builds everything for <paramref name="nodes"/>. Roads join every pair <paramref name="connected"/>
        /// reports (the controller's travel graph); null = the nodes' own neighbour lists.
        /// </summary>
        public void Build(IReadOnlyList<WorldMapRegionNode> nodes, System.Func<BiomeType, BiomeType, bool> connected)
        {
            if (_built)
            {
                return;
            }

            var validNodes = new List<WorldMapRegionNode>();
            if (nodes != null)
            {
                foreach (WorldMapRegionNode node in nodes)
                {
                    if (node != null)
                    {
                        validNodes.Add(node);
                    }
                }
            }

            if (validNodes.Count == 0)
            {
                GameLogger.Warning(LogCategory, "World map dressing skipped: no region nodes.");
                return;
            }

            _built = true;
            float baseY = float.PositiveInfinity;
            foreach (WorldMapRegionNode node in validNodes)
            {
                baseY = Mathf.Min(baseY, node.Position.y);
            }

            _root = transform;
            _root.SetPositionAndRotation(new Vector3(0f, baseY, 0f), Quaternion.identity);
            _root.localScale = Vector3.one;

            _terrain = CreateTerrain(validNodes, connected, _seed, _river);
            _props = WorldMapDressingLayout.BuildProps(_terrain, _seed, _density);
            _clouds = WorldMapDressingLayout.BuildClouds(_terrain, _seed);
            _meshes = new WorldMapPropMeshes(_seed);

            if (_library == null)
            {
                _library = WorldMapDressingLibrary.Load();
            }

            Material baseMaterial = ResolveBaseMaterial();
            _paletteMaterial = CreateMaterial(baseMaterial, Own(WorldMapPalette.CreateTexture()), "WorldMapPalette");
            Rect groundRect = GroundRect(_terrain);
            _groundMaterial = CreateMaterial(baseMaterial, Own(BakeGroundTexture(_terrain, groundRect, _groundTextureWidth)), "WorldMapGround");

            BuildGround(groundRect);
            BuildStaticProps(groundRect);
            BuildClouds();
            BuildSwayingProps();
            BuildLandmarks(validNodes);

            if (_hideAuthoredMarkers)
            {
                HideAuthoredMarkers(validNodes);
            }

            RefreshStates();
            GameLogger.Info(LogCategory,
                $"World map dressed: {_props.Count} props ({_swaying.Count} swaying), {_terrain.Paths.Count} roads, {_clouds.Count} clouds, seed {_seed}.");
        }

        /// <summary>Terrain for <paramref name="nodes"/> (roads between connected pairs, in node order).</summary>
        public static WorldMapTerrain CreateTerrain(IReadOnlyList<WorldMapRegionNode> nodes, System.Func<BiomeType, BiomeType, bool> connected, int seed, bool river)
        {
            var sites = new List<WorldMapSite>(nodes.Count);
            foreach (WorldMapRegionNode node in nodes)
            {
                Vector3 p = node.Position;
                sites.Add(new WorldMapSite(node.Region, new Vector2(p.x, p.z), ClickRadiusOf(node)));
            }

            var paths = new List<WorldMapPath>();
            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    bool linked = connected != null
                        ? connected(nodes[i].Region, nodes[j].Region)
                        : Links(nodes[i], nodes[j]) || Links(nodes[j], nodes[i]);
                    if (linked && nodes[i].Region != nodes[j].Region)
                    {
                        paths.Add(new WorldMapPath(i, j, sites[i].Position, sites[j].Position));
                    }
                }
            }

            return new WorldMapTerrain(sites, paths, seed, river);
        }

        private static bool Links(WorldMapRegionNode from, WorldMapRegionNode to)
        {
            if (from.Neighbours == null)
            {
                return false;
            }

            foreach (WorldMapRegionNode neighbour in from.Neighbours)
            {
                if (neighbour == to)
                {
                    return true;
                }
            }

            return false;
        }

        private static float ClickRadiusOf(WorldMapRegionNode node)
        {
            if (node.TryGetComponent(out Collider collider) && collider.enabled)
            {
                Bounds bounds = collider.bounds;
                float radius = Mathf.Max(bounds.extents.x, bounds.extents.z);
                if (radius > 0.01f)
                {
                    return radius;
                }
            }

            return WorldMapTerrain.DefaultClickRadius;
        }

        private static Rect GroundRect(WorldMapTerrain terrain)
        {
            Rect land = terrain.LandBounds;
            return Rect.MinMaxRect(land.xMin - 2f, land.yMin - 2f, land.xMax + 2f, land.yMax + 2f);
        }

        private Material ResolveBaseMaterial()
        {
            if (_library != null && _library.BaseMaterial != null)
            {
                return _library.BaseMaterial;
            }

            Renderer authored = FindAuthoredGround();
            if (authored != null && authored.sharedMaterial != null)
            {
                return authored.sharedMaterial;
            }

            // The render pipeline's default material, as BiomeArenaPropFactory's primitives use.
            GameObject probe = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Material material = probe.GetComponent<Renderer>().sharedMaterial;
            SafeDestroy(probe);
            return material;
        }

        private Material CreateMaterial(Material source, Texture texture, string materialName)
        {
            Material material = source != null ? new Material(source) : new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.name = materialName;
            material.enableInstancing = true;
            SetIfPresent(material, BaseColorId, Color.white);
            SetIfPresent(material, ColorId, Color.white);
            if (material.HasProperty(BaseMapId))
            {
                material.SetTexture(BaseMapId, texture);
                material.SetTextureScale(BaseMapId, Vector2.one);
                material.SetTextureOffset(BaseMapId, Vector2.zero);
            }

            if (material.HasProperty(MainTexId))
            {
                material.SetTexture(MainTexId, texture);
            }

            SetFloatIfPresent(material, SmoothnessId, 0.08f);
            SetFloatIfPresent(material, GlossinessId, 0.08f);
            SetFloatIfPresent(material, MetallicId, 0f);
            return Own(material);
        }

        private static void SetIfPresent(Material material, int id, Color color)
        {
            if (material.HasProperty(id))
            {
                material.SetColor(id, color);
            }
        }

        private static void SetFloatIfPresent(Material material, int id, float value)
        {
            if (material.HasProperty(id))
            {
                material.SetFloat(id, value);
            }
        }

        // ---------------------------------------------------------------
        // Ground
        // ---------------------------------------------------------------

        /// <summary>Samples <see cref="WorldMapTerrain.GroundColor"/> over <paramref name="rect"/>.</summary>
        public static Texture2D BakeGroundTexture(WorldMapTerrain terrain, Rect rect, int width)
        {
            width = Mathf.Max(8, width);
            int height = Mathf.Max(8, Mathf.RoundToInt(width * rect.height / Mathf.Max(0.01f, rect.width)));
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                float z = Mathf.Lerp(rect.yMin, rect.yMax, (y + 0.5f) / height);
                for (int x = 0; x < width; x++)
                {
                    float wx = Mathf.Lerp(rect.xMin, rect.xMax, (x + 0.5f) / width);
                    pixels[y * width + x] = terrain.GroundColor(new Vector2(wx, z));
                }
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, true)
            {
                name = "WorldMapGround",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                anisoLevel = 2,
            };
            texture.SetPixels32(pixels);
            texture.Apply(true, true);
            return texture;
        }

        private void BuildGround(Rect rect)
        {
            int columns = Mathf.Max(2, Mathf.CeilToInt(rect.width / _groundStep) + 1);
            int rows = Mathf.Max(2, Mathf.CeilToInt(rect.height / _groundStep) + 1);
            var vertices = new Vector3[columns * rows];
            var uvs = new Vector2[vertices.Length];
            for (int r = 0; r < rows; r++)
            {
                float v = (float)r / (rows - 1);
                float z = Mathf.Lerp(rect.yMin, rect.yMax, v);
                for (int c = 0; c < columns; c++)
                {
                    float u = (float)c / (columns - 1);
                    float x = Mathf.Lerp(rect.xMin, rect.xMax, u);
                    int index = r * columns + c;
                    vertices[index] = new Vector3(x, _terrain.Height(new Vector2(x, z)), z);
                    uvs[index] = new Vector2(u, v);
                }
            }

            var triangles = new int[(columns - 1) * (rows - 1) * 6];
            int t = 0;
            for (int r = 0; r < rows - 1; r++)
            {
                for (int c = 0; c < columns - 1; c++)
                {
                    int i0 = r * columns + c;
                    int i1 = i0 + 1;
                    int i2 = i0 + columns;
                    int i3 = i2 + 1;
                    triangles[t++] = i0; triangles[t++] = i2; triangles[t++] = i1;
                    triangles[t++] = i1; triangles[t++] = i2; triangles[t++] = i3;
                }
            }

            var mesh = new Mesh { name = "WorldMapGround" };
            mesh.indexFormat = vertices.Length > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.UploadMeshData(true);
            CreateMeshObject("Ground", Own(mesh), _groundMaterial, castShadows: false);
        }

        // ---------------------------------------------------------------
        // Props
        // ---------------------------------------------------------------

        private void BuildStaticProps(Rect groundRect)
        {
            var builder = new WorldMapMeshBuilder();
            bool forestPrefabs = HasPrefabs(_library != null ? _library.ForestTrees : null);
            bool mountainPrefabs = HasPrefabs(_library != null ? _library.MountainTrees : null);

            for (int i = 0; i < _props.Count; i++)
            {
                WorldMapPropPlacement prop = _props[i];
                builder.Matrix = Matrix4x4.TRS(prop.Position, Quaternion.Euler(0f, prop.Yaw, 0f), Vector3.one * prop.Scale);
                if (_meshes.AppendStatic(builder, prop.Kind, i))
                {
                    continue;
                }

                if (prop.Kind == WorldMapPropKind.Tree && !forestPrefabs)
                {
                    WorldMapPropMeshes.AppendFallbackTree(builder, false, _treeHeight);
                }
                else if (prop.Kind == WorldMapPropKind.Pine && !mountainPrefabs)
                {
                    WorldMapPropMeshes.AppendFallbackTree(builder, true, _treeHeight);
                }
            }

            builder.Matrix = Matrix4x4.identity;
            WorldMapPropMeshes.AppendRoadStones(builder, _terrain, _seed);

            if (_terrain.HasRiver)
            {
                Vector2 center = _terrain.BridgeCenter;
                Vector2 dir = _terrain.BridgeDirection;
                float yaw = Mathf.Atan2(-dir.y, dir.x) * Mathf.Rad2Deg;
                builder.Matrix = Matrix4x4.TRS(new Vector3(center.x, WorldMapTerrain.GroundLevel, center.y), Quaternion.Euler(0f, yaw, 0f), Vector3.one);
                WorldMapPropMeshes.AppendBridge(builder, (WorldMapTerrain.RiverHalfWidth + 0.7f) * 2f + 1.2f, WorldMapTerrain.PathHalfWidth * 2f + 0.1f, 0.06f);
                builder.Matrix = Matrix4x4.identity;
            }

            var outer = Rect.MinMaxRect(groundRect.xMin - 80f, groundRect.yMin - 60f, groundRect.xMax + 80f, groundRect.yMax + 90f);
            WorldMapPropMeshes.AppendSeaFrame(builder, groundRect, outer, WorldMapTerrain.SeaLevel);

            Mesh mesh = Own(builder.ToMesh("WorldMapStaticProps"));
            mesh.UploadMeshData(true);
            CreateMeshObject("StaticProps", mesh, _paletteMaterial, _castShadows);
        }

        private void BuildClouds()
        {
            if (_clouds.Count == 0)
            {
                return;
            }

            var builder = new WorldMapMeshBuilder();
            foreach (WorldMapCloudPlacement cloud in _clouds)
            {
                builder.Matrix = Matrix4x4.TRS(cloud.Position, Quaternion.Euler(0f, (cloud.Seed & 0xff) * 1.4f, 0f), Vector3.one * cloud.Scale);
                WorldMapPropMeshes.AppendCloud(builder, cloud.Seed);
            }

            Mesh mesh = Own(builder.ToMesh("WorldMapClouds"));
            mesh.UploadMeshData(true);
            CreateMeshObject("Clouds", mesh, _paletteMaterial, castShadows: false);
        }

        private void BuildSwayingProps()
        {
            var builder = new WorldMapMeshBuilder();
            WorldMapPropMeshes.AppendCactus(builder);
            Mesh cactus = Own(builder.ToMesh("WorldMapCactus"));
            builder.Clear();
            WorldMapPropMeshes.AppendGrassClump(builder);
            // Named like the scattered tufts so EnvironmentPropRules gives it grass sway.
            Mesh grass = Own(builder.ToMesh(EnvironmentPropRules.GrassTuftMeshName));

            GameObject[] forestTrees = _library != null ? _library.ForestTrees : null;
            GameObject[] mountainTrees = _library != null ? _library.MountainTrees : null;
            var parent = new GameObject("SwayingProps").transform;
            parent.SetParent(_root, false);

            for (int i = 0; i < _props.Count; i++)
            {
                WorldMapPropPlacement prop = _props[i];
                Vector3 position = _root.TransformPoint(prop.Position);
                Quaternion rotation = Quaternion.Euler(0f, prop.Yaw, 0f);
                GameObject go = null;

                switch (prop.Kind)
                {
                    case WorldMapPropKind.Tree:
                        go = SpawnTree(PickPrefab(forestTrees, i), parent, position, rotation, prop.Scale, i);
                        break;
                    case WorldMapPropKind.Pine:
                        go = SpawnTree(PickPrefab(mountainTrees, i), parent, position, rotation, prop.Scale * 0.9f, i);
                        break;
                    case WorldMapPropKind.Cactus:
                        // "ArenaProp_Cactus" prefix = shrub sway (EnvironmentPropRules).
                        go = SpawnMesh("ArenaProp_Cactus_Map" + i, cactus, parent, position, rotation, prop.Scale, _castShadows);
                        break;
                    case WorldMapPropKind.GrassClump:
                        go = SpawnMesh("GrassTuft_Map" + i, grass, parent, position, rotation, prop.Scale * 1.3f, false);
                        break;
                }

                if (go != null)
                {
                    EnvironmentPropAttacher.Attach(go);
                    _swaying.Add(go);
                }
            }
        }

        private static bool HasPrefabs(GameObject[] prefabs)
        {
            if (prefabs == null)
            {
                return false;
            }

            foreach (GameObject prefab in prefabs)
            {
                if (prefab != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static GameObject PickPrefab(GameObject[] prefabs, int index)
        {
            if (!HasPrefabs(prefabs))
            {
                return null;
            }

            for (int i = 0; i < prefabs.Length; i++)
            {
                GameObject prefab = prefabs[(index + i) % prefabs.Length];
                if (prefab != null)
                {
                    return prefab;
                }
            }

            return null;
        }

        private GameObject SpawnTree(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation, float scale, int index)
        {
            if (prefab == null)
            {
                return null; // built into the static mesh instead
            }

            GameObject tree = Instantiate(prefab, position, rotation, parent);
            // "Tree_" prefix + LODGroup = tree sway (EnvironmentPropRules).
            tree.name = prefab.name + "_Map" + index;
            float natural = NaturalHeight(prefab, tree);
            tree.transform.localScale = prefab.transform.localScale * (_treeHeight * scale / natural);

            foreach (Collider collider in tree.GetComponentsInChildren<Collider>())
            {
                collider.enabled = false; // never block node clicks
            }

            foreach (Renderer renderer in tree.GetComponentsInChildren<Renderer>())
            {
                renderer.shadowCastingMode = _castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            }

            // Map trees are small on screen: keep the cheaper last LOD instead of culling it away.
            if (tree.TryGetComponent(out LODGroup lodGroup))
            {
                LOD[] lods = lodGroup.GetLODs();
                if (lods.Length > 0)
                {
                    lods[lods.Length - 1].screenRelativeTransitionHeight = Mathf.Min(lods[lods.Length - 1].screenRelativeTransitionHeight, 0.002f);
                    lodGroup.SetLODs(lods);
                }
            }

            return tree;
        }

        private float NaturalHeight(GameObject prefab, GameObject instance)
        {
            if (_prefabHeights.TryGetValue(prefab, out float height))
            {
                return height;
            }

            height = 0f;
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }

                float scaleY = Mathf.Max(0.0001f, instance.transform.lossyScale.y);
                height = bounds.size.y / scaleY * Mathf.Max(0.0001f, prefab.transform.localScale.y);
            }

            if (height <= 0.01f)
            {
                height = 4.5f;
            }

            _prefabHeights[prefab] = height;
            return height;
        }

        private GameObject SpawnMesh(string objectName, Mesh mesh, Transform parent, Vector3 position, Quaternion rotation, float scale, bool castShadows)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(position, rotation);
            go.transform.localScale = Vector3.one * scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            ConfigureRenderer(renderer, _paletteMaterial, castShadows);
            return go;
        }

        // ---------------------------------------------------------------
        // Landmarks
        // ---------------------------------------------------------------

        private void BuildLandmarks(List<WorldMapRegionNode> nodes)
        {
            var builder = new WorldMapMeshBuilder();
            var landmarkMeshes = new Dictionary<BiomeType, Mesh>();

            builder.Clear();
            WorldMapPropMeshes.AppendClearedBanner(builder);
            Mesh banner = Own(builder.ToMesh("WorldMapClearedBanner"));

            for (int i = 0; i < nodes.Count; i++)
            {
                WorldMapRegionNode node = nodes[i];
                if (!landmarkMeshes.TryGetValue(node.Region, out Mesh landmarkMesh))
                {
                    builder.Clear();
                    _meshes.AppendLandmark(builder, node.Region);
                    landmarkMesh = Own(builder.ToMesh("WorldMapLandmark_" + node.Region));
                    landmarkMeshes[node.Region] = landmarkMesh;
                }

                builder.Clear();
                WorldMapPropMeshes.AppendLockedFog(builder, unchecked(_seed + i * 977));
                Mesh fog = Own(builder.ToMesh("WorldMapLockedFog_" + node.Region));

                Vector2 spot = WorldMapDressingLayout.LandmarkPosition(_terrain, i);
                var local = new Vector3(spot.x, _terrain.Height(spot) - 0.04f, spot.y);

                var entry = new NodeDressing { Node = node, Region = node.Region };
                entry.Landmark = CreateMeshObject("Landmark_" + node.Region, landmarkMesh, _paletteMaterial, _castShadows);
                entry.Landmark.transform.localPosition = local;

                entry.Fog = CreateMeshObject("LockedFog", fog, _paletteMaterial, false);
                entry.Fog.transform.SetParent(entry.Landmark.transform, false);

                entry.Banner = CreateMeshObject("ClearedBanner", banner, _paletteMaterial, _castShadows);
                entry.Banner.transform.SetParent(entry.Landmark.transform, false);
                entry.Banner.transform.localPosition = new Vector3(1.15f, 0f, 0.35f);

                entry.ClickArea = AddClickArea(node, _root.TransformPoint(local));
                _nodeDressings.Add(entry);
            }
        }

        /// <summary>
        /// Trigger box over the landmark, parented to the node so the controller's raycast
        /// (GetComponentInParent&lt;WorldMapRegionNode&gt;, triggers included) treats it as the node.
        /// </summary>
        private static Collider AddClickArea(WorldMapRegionNode node, Vector3 landmarkWorld)
        {
            var go = new GameObject("LandmarkClickArea");
            go.layer = node.gameObject.layer;
            go.transform.SetPositionAndRotation(landmarkWorld + Vector3.up * 1.1f, Quaternion.identity);
            go.transform.localScale = Vector3.one;
            go.transform.SetParent(node.transform, true);
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            Vector3 lossy = go.transform.lossyScale;
            box.size = new Vector3(2.6f / Mathf.Max(0.0001f, Mathf.Abs(lossy.x)), 2.2f / Mathf.Max(0.0001f, Mathf.Abs(lossy.y)), 2.4f / Mathf.Max(0.0001f, Mathf.Abs(lossy.z)));
            return box;
        }

        /// <summary>Fog over locked regions, banner over cleared ones (from <see cref="WorldMapProgress"/>; all open without one).</summary>
        public void RefreshStates()
        {
            WorldMapProgress progress = _controller != null ? _controller.Progress : null;
            if (progress == null)
            {
                ServiceLocator.TryGet(out progress);
            }

            foreach (NodeDressing node in _nodeDressings)
            {
                bool unlocked = progress == null || progress.IsUnlocked(node.Region);
                bool cleared = progress != null && progress.IsRegionCleared(node.Region);
                if (node.Fog != null)
                {
                    node.Fog.SetActive(!unlocked);
                }

                if (node.Banner != null)
                {
                    node.Banner.SetActive(cleared);
                }
            }
        }

        /// <summary>For tests/debug: whether <paramref name="region"/>'s landmark shows the locked fog / cleared banner.</summary>
        public bool TryGetLandmarkState(BiomeType region, out bool fogShown, out bool bannerShown)
        {
            foreach (NodeDressing node in _nodeDressings)
            {
                if (node.Region == region)
                {
                    fogShown = node.Fog != null && node.Fog.activeSelf;
                    bannerShown = node.Banner != null && node.Banner.activeSelf;
                    return true;
                }
            }

            fogShown = false;
            bannerShown = false;
            return false;
        }

        // ---------------------------------------------------------------
        // Authored placeholders
        // ---------------------------------------------------------------

        private void HideAuthoredMarkers(List<WorldMapRegionNode> nodes)
        {
            Renderer ground = FindAuthoredGround();
            Hide(ground);

            foreach (WorldMapRegionNode node in nodes)
            {
                // The coloured disc (the node's own renderer; its collider stays clickable).
                if (node.TryGetComponent(out Renderer disc))
                {
                    Hide(disc);
                }

                // The dark "locked" sphere: the landmark fog replaces it. RefreshVisuals keeps toggling the object.
                if (node.LockedVisual != null)
                {
                    foreach (Renderer renderer in node.LockedVisual.GetComponentsInChildren<Renderer>(true))
                    {
                        Hide(renderer);
                    }
                }
            }
        }

        private void Hide(Renderer renderer)
        {
            if (renderer != null && renderer.enabled)
            {
                renderer.enabled = false;
                _hiddenRenderers.Add(renderer);
            }
        }

        private Renderer FindAuthoredGround()
        {
            if (_authoredGroundNames == null || _authoredGroundNames.Length == 0)
            {
                return null;
            }

            Scene scene = gameObject.scene;
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return null;
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (string groundName in _authoredGroundNames)
                {
                    if (!string.IsNullOrEmpty(groundName) && root.name == groundName && root.TryGetComponent(out Renderer renderer))
                    {
                        return renderer;
                    }
                }
            }

            return null;
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private GameObject CreateMeshObject(string objectName, Mesh mesh, Material material, bool castShadows)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(_root, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            ConfigureRenderer(go.AddComponent<MeshRenderer>(), material, castShadows);
            return go;
        }

        private static void ConfigureRenderer(MeshRenderer renderer, Material material, bool castShadows)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            renderer.receiveShadows = true;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private T Own<T>(T asset) where T : Object
        {
            if (asset != null)
            {
                _owned.Add(asset);
            }

            return asset;
        }

        private static void SafeDestroy(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
