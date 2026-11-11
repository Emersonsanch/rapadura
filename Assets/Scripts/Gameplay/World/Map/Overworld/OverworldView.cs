using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Turns an <see cref="OverworldLayout"/> into the scene of a continuous region (map mode of Main.unity,
    /// <see cref="MapModeController"/>), streamed around the hero:
    /// <list type="bullet">
    /// <item>always there: one ground slab collider and the walls of the whole region, the gates out of the region and the
    /// Cais da Barca triggers;</item>
    /// <item>streamed by distance (<see cref="TownLoadRadius"/> / <see cref="TownUnloadRadius"/>, at most one per tick): the
    /// settlements (town kit, their own colliders, safe zones, keepers and night lights; the city gates removed); the
    /// stitched world has some 25 of them, only the ones near the hero exist;</item>
    /// <item>streamed by chunk (<see cref="OverworldRules.LoadRadius"/> / <see cref="OverworldRules.UnloadRadius"/>, at most
    /// one new chunk per frame): two merged meshes per chunk (ground; props, landmarks and bridges) on the shared town
    /// material, capsule/box colliders, the river blockers and bridge decks;</item>
    /// <item>spawners woken near the hero and put to sleep (monsters removed) far from them, so only a handful run;</item>
    /// <item>the zone the hero is in (the map id the rest of the game reads; with hysteresis, <see cref="AreaTracker"/>, the
    /// <see cref="AreaChangedEvent"/> of the HUD letreiro and the area music / weather) and the soft barrier: stepping into an area above the hero's walk level (or without its special gear) pushes
    /// the hero back a step with a message, never a teleport across the map.</item>
    /// </list>
    /// </summary>
    public sealed class OverworldView : MonoBehaviour
    {
        public const string RootPrefix = "Overworld_";
        private const string PlayerTag = "Player";
        private const float StreamTick = 0.2f;
        private const float ZoneTick = 0.1f;
        private const float WarningCooldown = 2.5f;

        /// <summary>A town whose ground comes this close to the hero (m) is built...</summary>
        public const float TownLoadRadius = 170f;

        /// <summary>...and torn down past this (m).</summary>
        public const float TownUnloadRadius = 260f;

        private sealed class Chunk
        {
            public GameObject Root;
            public Mesh Ground;
            public Mesh Props;
            public Mesh Collider;
        }

        private readonly Dictionary<Vector2Int, Chunk> _chunks = new Dictionary<Vector2Int, Chunk>();
        private readonly Dictionary<string, MapMonsterSpawner> _spawners = new Dictionary<string, MapMonsterSpawner>();
        private readonly List<Vector2Int> _wanted = new List<Vector2Int>();
        private readonly List<Vector2Int> _drop = new List<Vector2Int>();
        private readonly List<string> _sleep = new List<string>();
        private readonly HashSet<Vector2Int> _pinned = new HashSet<Vector2Int>();
        private readonly List<Vector2Int> _clearingChunks = new List<Vector2Int>();
        private readonly List<Vector3> _colliderVertices = new List<Vector3>(400);
        private readonly List<int> _colliderTriangles = new List<int>(1600);
        private readonly Stack<Mesh> _meshPool = new Stack<Mesh>();
        private readonly List<MapExitTrigger> _exits = new List<MapExitTrigger>();
        private readonly List<CityTownView> _towns = new List<CityTownView>();
        private readonly Dictionary<OverworldSettlement, CityTownView> _townOf = new Dictionary<OverworldSettlement, CityTownView>();
        private readonly List<OverworldSettlement> _townDrop = new List<OverworldSettlement>();
        private readonly Dictionary<OverworldSettlement, GameObject> _townAnchors = new Dictionary<OverworldSettlement, GameObject>();
        private readonly TownMeshBuilder _builder = new TownMeshBuilder();

        private GameObject _enemyPrefab;
        private Transform _chunkRoot;
        private Transform _spawnerRoot;
        private Transform _townRoot;
        private Transform _player;
        private InventoryManager _bag;
        private float _nextStream;
        private float _nextZone;
        private float _nextWarning;
        private Vector2 _lastAllowed;
        private bool _hasLastAllowed;

        public OverworldLayout Layout { get; private set; }

        /// <summary>The town of the region the hero is in, when it is built (null while streamed out).</summary>
        public CityTownView Vila => TownOf(CurrentPart != null ? CurrentPart.Vila : Layout?.Vila);

        /// <summary>The region of the stitched world the hero is in (null for a region on its own).</summary>
        public OverworldPart CurrentPart { get; private set; }

        public int LoadedTownCount => _townOf.Count;

        public IReadOnlyList<CityTownView> Towns => _towns;
        public IReadOnlyList<MapExitTrigger> Exits => _exits;
        public OverworldZone CurrentZone { get; private set; }
        public int LoadedChunkCount => _chunks.Count;
        public int ActiveSpawnerCount => _spawners.Count;
        public int ChunksBuiltTotal { get; private set; }

        /// <summary>Raised when the hero walks into another area of the region.</summary>
        public event System.Action<OverworldZone> ZoneChanged;

        /// <summary>Raised when the hero walks (or is carried) into another region of the stitched world.</summary>
        public event System.Action<OverworldPart> RegionChanged;

        /// <summary>
        /// The hero's area with hysteresis (letreiro, area music and weather): each change is published as an
        /// <see cref="AreaChangedEvent"/> on the <see cref="EventBus"/>.
        /// </summary>
        public OverworldAreaTracker AreaTracker { get; private set; }

        public bool IsChunkLoaded(Vector2Int chunk) => _chunks.ContainsKey(chunk);

        public CityTownView TownOf(OverworldSettlement settlement)
        {
            return settlement != null && _townOf.TryGetValue(settlement, out CityTownView town) && town != null ? town : null;
        }

        public static OverworldView Build(OverworldLayout layout, GameObject enemyPrefab, Transform parent = null)
        {
            var root = new GameObject(RootPrefix + layout.Id);
            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            OverworldView view = root.AddComponent<OverworldView>();
            view.Layout = layout;
            view.AreaTracker = new OverworldAreaTracker(layout);
            view._enemyPrefab = enemyPrefab;
            view.BuildStatic();
            root.AddComponent<Mechanics.FarnkeshMechanicsDriver>();
            root.AddComponent<Mechanics.MarGeladoMechanicsDriver>();
            root.AddComponent<Mechanics.MisturaMechanicsDriver>();
            root.AddComponent<Mechanics.RioMechanicsDriver>();
            root.AddComponent<Mechanics.MingchengMechanicsDriver>();
            root.AddComponent<Mechanics.ProfundoMechanicsDriver>();
            root.AddComponent<Mechanics.CeuMechanicsDriver>();
            root.AddComponent<Mechanics.NartesMechanicsDriver>();
            root.AddComponent<Mechanics.HinokuraMechanicsDriver>();
            root.AddComponent<Mechanics.AreiasMechanicsDriver>(); root.AddComponent<Mechanics.KaggenMechanicsDriver>(); root.AddComponent<Mechanics.LimiarMechanicsDriver>();
            root.AddComponent<Mechanics.CostaDeltaTetoMechanicsDriver>();
            root.AddComponent<Mechanics.ZarafshanMechanicsDriver>();
            return view;
        }

        private void OnEnable()
        {
            // Whoever stands the hero somewhere in this region (arrival, respawn, barrier) stands them on the relief.
            CityModeController.GroundHeightAt = GroundHeight;
        }

        private void OnDisable()
        {
            if (CityModeController.GroundHeightAt != null && ReferenceEquals(CityModeController.GroundHeightAt.Target, this))
            {
                CityModeController.GroundHeightAt = null;
            }
        }

        /// <summary>Height of the drawn and collided ground at <paramref name="p"/> (<see cref="OverworldRules.SurfaceAt"/>).</summary>
        public float GroundHeight(Vector2 p) => Layout != null ? OverworldRules.SurfaceAt(Layout, p) : 0f;

        private void OnDestroy()
        {
            foreach (Chunk chunk in _chunks.Values)
            {
                DestroyMeshes(chunk);
            }

            while (_meshPool.Count > 0)
            {
                Destroy(_meshPool.Pop());
            }
        }

        // ------------------------------------------------------------------
        // Static part: ground slab, walls, towns, gates
        // ------------------------------------------------------------------

        private void BuildStatic()
        {
            Rect b = Layout.Bounds;
            var colliders = new GameObject("Ow_WorldColliders");
            colliders.transform.SetParent(transform, false);
            colliders.isStatic = true;
            // The ground itself is each chunk's relief collider; the slab is only a safety net well below the river beds.
            var slab = colliders.AddComponent<BoxCollider>();
            slab.center = new Vector3(b.center.x, -6.5f, b.center.y);
            slab.size = new Vector3(b.width + 120f, 1f, b.height + 120f);
            const float bottom = -20f;
            const float h = 80f;
            const float t = 1f;
            AddBox(colliders, new Vector3(b.center.x, bottom + h * 0.5f, b.yMax + t * 0.5f), new Vector3(b.width + t * 2f, h, t));
            AddBox(colliders, new Vector3(b.center.x, bottom + h * 0.5f, b.yMin - t * 0.5f), new Vector3(b.width + t * 2f, h, t));
            AddBox(colliders, new Vector3(b.xMax + t * 0.5f, bottom + h * 0.5f, b.center.y), new Vector3(t, h, b.height));
            AddBox(colliders, new Vector3(b.xMin - t * 0.5f, bottom + h * 0.5f, b.center.y), new Vector3(t, h, b.height));

            // The stitched world need not be a rectangle: each hole no region covers is walled off from the inside too.
            foreach (Rect hole in Layout.Holes)
            {
                AddBox(colliders, new Vector3(hole.center.x, bottom + h * 0.5f, hole.yMax - t * 0.5f), new Vector3(hole.width, h, t));
                AddBox(colliders, new Vector3(hole.center.x, bottom + h * 0.5f, hole.yMin + t * 0.5f), new Vector3(hole.width, h, t));
                AddBox(colliders, new Vector3(hole.xMax - t * 0.5f, bottom + h * 0.5f, hole.center.y), new Vector3(t, h, hole.height));
                AddBox(colliders, new Vector3(hole.xMin + t * 0.5f, bottom + h * 0.5f, hole.center.y), new Vector3(t, h, hole.height));
            }

            _townRoot = new GameObject("Ow_Towns").transform;
            _townRoot.SetParent(transform, false);

            foreach (MapExitSpec exit in Layout.World.Exits)
            {
                var go = new GameObject("MapExit_" + exit.Id);
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(exit.Trigger.center.x, GroundHeight(exit.Trigger.center), exit.Trigger.center.y);
                var trigger = go.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.center = new Vector3(0f, 1.2f, 0f);
                trigger.size = new Vector3(exit.Trigger.width, 2.4f, exit.Trigger.height);
                MapExitTrigger component = go.AddComponent<MapExitTrigger>();
                component.Configure(exit);
                _exits.Add(component);
            }

            _chunkRoot = new GameObject("Ow_Chunks").transform;
            _chunkRoot.SetParent(transform, false);
            _spawnerRoot = new GameObject("Ow_Spawners").transform;
            _spawnerRoot.SetParent(transform, false);
        }

        private void BuildTown(OverworldSettlement settlement)
        {
            var anchor = new GameObject("Ow_Town_" + settlement.Id).transform;
            anchor.SetParent(_townRoot, false);
            anchor.localPosition = new Vector3(settlement.Offset.x, 0f, settlement.Offset.y);
            CityTownView town = CityTownView.Build(settlement.Town, anchor, "ow:" + Layout.Id + ":" + settlement.Id);
            RemoveCityGates(town);
            _towns.Add(town);
            _townOf[settlement] = town;
            _townAnchors[settlement] = anchor.gameObject;
        }

        /// <summary>
        /// Builds the towns near <paramref name="at"/> (all of them with <paramref name="all"/>, else at most one, the nearest)
        /// and tears down the far ones: a town is heavy (houses, keepers, lights), only the few around the hero exist.
        /// </summary>
        private void UpdateTowns(Vector2 at, bool all)
        {
            _townDrop.Clear();
            foreach (KeyValuePair<OverworldSettlement, CityTownView> pair in _townOf)
            {
                if (CityGeometry.DistanceToRect(at, pair.Key.GroundRect) > TownUnloadRadius)
                {
                    _townDrop.Add(pair.Key);
                }
            }

            foreach (OverworldSettlement settlement in _townDrop)
            {
                _towns.Remove(_townOf[settlement]);
                _townOf.Remove(settlement);
                if (_townAnchors.TryGetValue(settlement, out GameObject anchor))
                {
                    _townAnchors.Remove(settlement);
                    Destroy(anchor);
                }
            }

            OverworldSettlement nearest = null;
            float best = TownLoadRadius;
            foreach (OverworldSettlement settlement in Layout.Settlements)
            {
                if (_townOf.ContainsKey(settlement))
                {
                    continue;
                }

                float d = CityGeometry.DistanceToRect(at, settlement.GroundRect);
                if (d > TownLoadRadius)
                {
                    continue;
                }

                if (all)
                {
                    BuildTown(settlement);
                }
                else if (d <= best)
                {
                    best = d;
                    nearest = settlement;
                }
            }

            if (nearest != null)
            {
                BuildTown(nearest);
            }
        }

        private static void RemoveCityGates(CityTownView town)
        {
            foreach (CityExitTrigger cityExit in town.Exits)
            {
                if (cityExit == null)
                {
                    continue;
                }

                foreach (Collider c in cityExit.GetComponents<Collider>())
                {
                    c.enabled = false;
                    Destroy(c);
                }

                Destroy(cityExit);
            }
        }

        private static void AddBox(GameObject go, Vector3 center, Vector3 size)
        {
            var box = go.AddComponent<BoxCollider>();
            box.center = center;
            box.size = size;
        }

        // ------------------------------------------------------------------
        // Streaming
        // ------------------------------------------------------------------

        /// <summary>Builds every chunk around <paramref name="at"/> now (arrival, respawn, barca): no pop-in under the hero.</summary>
        public void WarmUp(Vector2 at)
        {
            OverworldRules.ChunksWithin(Layout, at, OverworldRules.LoadRadius, _wanted);
            foreach (Vector2Int c in _wanted)
            {
                if (!_chunks.ContainsKey(c))
                {
                    BuildChunk(c);
                }
            }

            UnloadFar(at);
            UpdateTowns(at, true);
            UpdateSpawners(at);

            // The clearings that woke up asked for their ground: build it now, then let them spawn.
            foreach (Vector2Int c in _wanted)
            {
                if (!_chunks.ContainsKey(c))
                {
                    BuildChunk(c);
                }
            }

            UpdateSpawners(at);
        }

        private void Update()
        {
            Transform player = Player();
            if (player == null)
            {
                return;
            }

            var at = new Vector2(player.position.x, player.position.z);
            if (Time.unscaledTime >= _nextZone)
            {
                _nextZone = Time.unscaledTime + ZoneTick;
                TrackZone(player, at);
            }

            if (Time.unscaledTime >= _nextStream)
            {
                _nextStream = Time.unscaledTime + StreamTick;
                OverworldRules.ChunksWithin(Layout, at, OverworldRules.LoadRadius, _wanted);
                UnloadFar(at);
                UpdateTowns(at, false);
                UpdateSpawners(at);
            }

            // At most one new chunk per frame (nearest first): the frame never pays for the whole ring at once.
            for (int i = 0; i < _wanted.Count; i++)
            {
                if (!_chunks.ContainsKey(_wanted[i]))
                {
                    BuildChunk(_wanted[i]);
                    _wanted.RemoveAt(i);
                    break;
                }
            }
        }

        private void UnloadFar(Vector2 at)
        {
            _drop.Clear();
            foreach (KeyValuePair<Vector2Int, Chunk> pair in _chunks)
            {
                // A chunk under an awake clearing keeps its ground collider (its monsters stand on it).
                if (OverworldRules.DistanceToChunk(Layout, at, pair.Key) > OverworldRules.UnloadRadius && !_pinned.Contains(pair.Key))
                {
                    _drop.Add(pair.Key);
                }
            }

            foreach (Vector2Int c in _drop)
            {
                Chunk chunk = _chunks[c];
                _chunks.Remove(c);
                DestroyMeshes(chunk);
                Destroy(chunk.Root);
            }
        }

        private void DestroyMeshes(Chunk chunk)
        {
            foreach (Mesh mesh in new[] { chunk.Ground, chunk.Props, chunk.Collider })
            {
                if (mesh != null)
                {
                    mesh.Clear();
                    _meshPool.Push(mesh);
                }
            }

            chunk.Ground = null;
            chunk.Props = null;
            chunk.Collider = null;
        }

        private Mesh TakeMesh() => _meshPool.Count > 0 ? _meshPool.Pop() : new Mesh();

        private void BuildChunk(Vector2Int coord)
        {
            OverworldChunkContent content = OverworldChunkBuilder.Content(Layout, coord);
            var root = new GameObject("Ow_Chunk_" + coord.x + "_" + coord.y);
            root.transform.SetParent(_chunkRoot, false);
            root.isStatic = true;
            var chunk = new Chunk { Root = root };
            Material material = TownPalette.SharedMaterial;

            _builder.Clear();
            OverworldChunkBuilder.BuildGround(_builder, Layout, content);
            if (!_builder.IsEmpty)
            {
                chunk.Ground = _builder.ToMesh("ow_ground", TakeMesh());
                AddRenderer(root.transform, "Ow_Ground", chunk.Ground, material, ShadowCastingMode.Off);
            }

            _builder.Clear();
            OverworldChunkBuilder.BuildProps(_builder, content);
            if (!_builder.IsEmpty)
            {
                chunk.Props = _builder.ToMesh("ow_props", TakeMesh());
                AddRenderer(root.transform, "Ow_Props", chunk.Props, material, content.Inner ? ShadowCastingMode.On : ShadowCastingMode.Off);
            }

            if (content.Inner)
            {
                chunk.Collider = BuildGroundCollider(root, content);
                BuildColliders(root, content);
            }

            _chunks[coord] = chunk;
            ChunksBuiltTotal++;
        }

        /// <summary>The relief collider of the chunk (same facets as the drawn ground): what the hero, the monsters and the clicks stand on.</summary>
        private Mesh BuildGroundCollider(GameObject root, OverworldChunkContent content)
        {
            OverworldChunkBuilder.BuildColliderMesh(content, _colliderVertices, _colliderTriangles);
            Mesh mesh = TakeMesh();
            mesh.Clear();
            mesh.name = "ow_ground_collider";
            mesh.SetVertices(_colliderVertices);
            mesh.SetTriangles(_colliderTriangles, 0);
            mesh.RecalculateBounds();
            var go = new GameObject("Ow_GroundCollider");
            go.transform.SetParent(root.transform, false);
            go.isStatic = true;
            go.AddComponent<MeshCollider>().sharedMesh = mesh;
            return mesh;
        }

        private void BuildColliders(GameObject root, OverworldChunkContent content)
        {
            var solids = new GameObject("Ow_Solids");
            solids.transform.SetParent(root.transform, false);
            solids.isStatic = true;
            foreach (OverworldSolid solid in content.Solids)
            {
                if (solid.IsBox)
                {
                    var piece = new GameObject("Ow_Box");
                    piece.transform.SetParent(solids.transform, false);
                    piece.transform.localPosition = new Vector3(solid.Center.x, solid.Base + solid.Height * 0.5f, solid.Center.y);
                    piece.transform.localRotation = Quaternion.Euler(0f, solid.Yaw, 0f);
                    piece.AddComponent<BoxCollider>().size = new Vector3(solid.Size.x, solid.Height, solid.Size.y);
                    continue;
                }

                var capsule = solids.AddComponent<CapsuleCollider>();
                capsule.direction = 1;
                capsule.radius = solid.Radius;
                capsule.height = Mathf.Max(solid.Height, solid.Radius * 2f);
                capsule.center = new Vector3(solid.Center.x, solid.Base + capsule.height * 0.5f, solid.Center.y);
            }

            foreach ((Vector2 center, float length, float yaw, float width) in Layout.RiverPiecesIn(content.Coord))
            {
                var piece = new GameObject("Ow_RiverBlock");
                piece.transform.SetParent(solids.transform, false);
                // From under the carved bed (-2 m) to 3 m over the banks (the valley floor is y = 0).
                piece.transform.localPosition = new Vector3(center.x, 0.5f, center.y);
                piece.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
                piece.AddComponent<BoxCollider>().size = new Vector3(Mathf.Max(1f, width - 1.2f), 5f, length);
            }

            foreach ((MapRiver river, MapCrossing crossing) in content.Bridges)
            {
                Vector2 across = MapAreaView.RiverNormal(river, crossing.At);
                var bridge = new GameObject("Ow_BridgeDeck");
                bridge.transform.SetParent(solids.transform, false);
                bridge.transform.localPosition = new Vector3(crossing.At.x, Mathf.Max(0f, content.Grid.Sample(crossing.At)), crossing.At.y);
                bridge.transform.localRotation = Quaternion.Euler(0f, CityGeometry.YawFacing(across), 0f);
                float span = OverworldChunkBuilder.BridgeSpan(river);
                var deck = bridge.AddComponent<BoxCollider>();
                deck.center = new Vector3(0f, 0.09f, 0f);
                deck.size = new Vector3(crossing.Width, 0.18f, span);
                foreach (float side in new[] { -1f, 1f })
                {
                    var rail = bridge.AddComponent<BoxCollider>();
                    rail.center = new Vector3(side * (crossing.Width * 0.5f + 0.1f), 0.6f, 0f);
                    rail.size = new Vector3(0.2f, 1.2f, span - 2.6f);
                }
            }
        }

        private static void AddRenderer(Transform parent, string objectName, Mesh mesh, Material material, ShadowCastingMode shadows)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            go.isStatic = true;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = shadows;
            renderer.receiveShadows = true;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        // ------------------------------------------------------------------
        // Spawners
        // ------------------------------------------------------------------

        private void UpdateSpawners(Vector2 at)
        {
            _sleep.Clear();
            _pinned.Clear();
            foreach (KeyValuePair<string, MapMonsterSpawner> pair in _spawners)
            {
                if (pair.Value == null || !OverworldRules.SpawnerShouldRun(pair.Value.Zone, at, true))
                {
                    _sleep.Add(pair.Key);
                }
            }

            foreach (string id in _sleep)
            {
                MapMonsterSpawner spawner = _spawners[id];
                _spawners.Remove(id);
                if (spawner != null)
                {
                    spawner.DespawnAll();
                    Destroy(spawner.gameObject);
                }
            }

            foreach (KeyValuePair<string, MapMonsterSpawner> pair in _spawners)
            {
                OverworldRules.ChunksUnderClearing(Layout, pair.Value.Zone, _clearingChunks);
                foreach (Vector2Int c in _clearingChunks)
                {
                    _pinned.Add(c);
                }
            }

            foreach (MapSpawnZone zone in Layout.World.Zones)
            {
                if (_spawners.ContainsKey(zone.Id) || !OverworldRules.SpawnerShouldRun(zone, at, false))
                {
                    continue;
                }

                // Its monsters stand on the relief collider: wait until the chunks under the clearing are built (asked for here).
                OverworldRules.ChunksUnderClearing(Layout, zone, _clearingChunks);
                bool ready = true;
                foreach (Vector2Int c in _clearingChunks)
                {
                    if (!_chunks.ContainsKey(c))
                    {
                        ready = false;
                        if (!_wanted.Contains(c))
                        {
                            _wanted.Add(c);
                        }
                    }
                }

                if (!ready)
                {
                    continue;
                }

                foreach (Vector2Int c in _clearingChunks)
                {
                    _pinned.Add(c);
                }

                var go = new GameObject("Spawner_" + zone.Id);
                go.transform.SetParent(_spawnerRoot, false);
                go.transform.localPosition = new Vector3(zone.Center.x, GroundHeight(zone.Center), zone.Center.y);
                MapMonsterSpawner spawner = go.AddComponent<MapMonsterSpawner>();
                Layout.ZoneOfSpawn.TryGetValue(zone.Id, out OverworldZone owner);
                spawner.Configure(Layout.World, zone, _enemyPrefab, (int)TownStyle.Hash(zone.Id, 3), owner?.MapIdOrOwner, GroundHeight);
                _spawners[zone.Id] = spawner;
            }
        }

        // ------------------------------------------------------------------
        // Zones and the soft barrier
        // ------------------------------------------------------------------

        private Transform Player()
        {
            if (_player == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
                _player = player != null ? player.transform : null;
                _bag = player != null ? player.GetComponentInChildren<InventoryManager>() : null;
            }

            return _player;
        }

        /// <summary>Forgets the last allowed spot (after a teleport: barca, respawn, arrival).</summary>
        public void ResetTracking(Vector2 at)
        {
            _lastAllowed = at;
            _hasLastAllowed = true;
            CurrentZone = null;
            _nextZone = 0f;
            AreaTracker?.Reset();
        }

        private void TrackZone(Transform player, Vector2 at)
        {
            OverworldZone zone = OverworldRules.ZoneAt(Layout, at);
            if (zone == null)
            {
                return;
            }

            // The region is the one under the hero's feet (half-open rectangles: one per point); if the soft barrier pushes
            // them back over the border, the next tick switches back.
            OverworldPart part = Layout.PartAt(at);
            if (part != CurrentPart)
            {
                CurrentPart = part;
                RegionChanged?.Invoke(part);
            }

            if (zone != CurrentZone)
            {
                MapModeController mode = MapModeController.Current;
                int level = mode != null ? mode.HeroLevel : 1;
                CharacterId? hero = ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;
                MapAccessCheck check = OverworldRules.CheckEnter(zone, level, hero, MapRequirementHeroState.LookupFor(_bag));
                if (check != MapAccessCheck.Ok && CurrentZone != null && _hasLastAllowed)
                {
                    PushBack(player, at, zone, check, hero);
                    return;
                }

                CurrentZone = zone;
                ZoneChanged?.Invoke(zone);
            }

            _lastAllowed = at;
            _hasLastAllowed = true;
            if (AreaTracker != null && AreaTracker.Update(at, out AreaChangedEvent change))
            {
                // The letreiro (UI.HUD.AreaBannerController) announces it; music and weather follow (Audio.AreaAmbienceDirector).
                EventBus.Publish(change);
            }

            OverworldSettlement settlement = OverworldRules.SettlementAt(Layout, at);
            if (settlement != null)
            {
                OverworldProgress.MarkVisited(settlement.MapId);
            }
        }

        private void PushBack(Transform player, Vector2 at, OverworldZone zone, MapAccessCheck check, CharacterId? hero)
        {
            Vector2 away = _lastAllowed - at;
            Vector2 dir = away.sqrMagnitude > 1e-4f ? away.normalized : -(zone.Bounds.center - at).normalized;
            Vector2 back = _lastAllowed + dir * 1.5f;
            if (OverworldRules.ZoneAt(Layout, back) != CurrentZone)
            {
                back = _lastAllowed;
            }

            CityModeController.PlaceAt(player.gameObject, back, dir);
            if (Time.unscaledTime < _nextWarning)
            {
                return;
            }

            _nextWarning = Time.unscaledTime + WarningCooldown;
            ServiceLocator.TryGet(out LocalizationManager localization);
            string line = check == MapAccessCheck.MissingRequirement
                ? OverworldTexts.Format(localization, OverworldTexts.BarrierRequirement, MapTexts.RequirementName(localization, zone.RequirementTag))
                : OverworldTexts.Format(localization, OverworldTexts.BarrierLevel, MapAccessRules.EffectiveLevelRequirement(zone.WalkLevel, hero, zone.RegionType));
            CombatFxManager.SpawnLootWarningText(player.position + Vector3.up * 2.4f, OverworldTexts.ZoneName(localization, zone) + ": " + line);
        }
    }
}
