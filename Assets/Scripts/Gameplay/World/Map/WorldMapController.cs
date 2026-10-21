using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Save;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Rapadura.Gameplay.World.Map
{
    /// <summary>
    /// Scene driver for WorldMap.unity (top-down overworld, classic-FF style). Clicking a
    /// <see cref="WorldMapRegionNode"/> walks the player token to it if the region is unlocked and
    /// connected to the current one; while walking, distance/time is fed to a
    /// <see cref="RandomEncounterSystem"/> using the biome of the node the token is closest to, and a
    /// triggered encounter is handed to <see cref="SceneTransitionService.BeginEncounter"/>.
    ///
    /// State that must outlive the scene (current/unlocked regions, victories) lives in the
    /// GameManager-built <see cref="WorldMapProgress"/>; this component only holds per-visit state.
    /// Because a new <see cref="RandomEncounterSystem"/> is built on every scene load, the grace
    /// period naturally applies right after returning from combat.
    /// </summary>
    public class WorldMapController : MonoBehaviour
    {
        private const string LogCategory = "WorldMap";

        [Header("Regions")]
        [SerializeField] private WorldMapRegionNode[] _nodes = new WorldMapRegionNode[0];

        [Header("Player token")]
        [SerializeField] private Transform _playerToken;
        [SerializeField] [Min(0.1f)] private float _tokenSpeed = 4f;
        [Tooltip("Vertical offset applied to the token above each node's position.")]
        [SerializeField] private float _tokenHeightOffset = 0.5f;

        [Header("Encounters")]
        [SerializeField] private bool _encountersEnabled = true;
        [SerializeField] private RandomEncounterSettings _encounterSettings = new RandomEncounterSettings();
        [Tooltip("Use a fixed RNG seed (reproducible encounter sequence, handy for debugging). Otherwise seeded from the clock.")]
        [SerializeField] private bool _useFixedSeed;
        [SerializeField] private int _seed = 12345;

        [Header("Input")]
        [SerializeField] private Camera _camera;
        [SerializeField] private LayerMask _nodeMask = ~0;
        [SerializeField] private float _maxRayDistance = 500f;

        private readonly Dictionary<BiomeType, WorldMapRegionNode> _nodesByRegion = new Dictionary<BiomeType, WorldMapRegionNode>();
        private WorldMapGraph _graph;
        private RandomEncounterSystem _encounters;
        private WorldMapProgress _progress;
        private SceneTransitionService _transition;
        private WorldMapRegionNode _currentNode;
        private WorldMapRegionNode _targetNode;
        private bool _encounterInProgress;

        public WorldMapProgress Progress => _progress;
        public WorldMapGraph Graph => _graph;
        public RandomEncounterSystem Encounters => _encounters;
        public WorldMapRegionNode CurrentNode => _currentNode;
        public bool IsTraveling => _targetNode != null;

        private void Awake()
        {
            _graph = BuildGraph(_nodes, _nodesByRegion);
            int seed = _useFixedSeed ? _seed : System.Environment.TickCount;
            _encounters = new RandomEncounterSystem(_encounterSettings, seed) { Enabled = _encountersEnabled };
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _progress))
            {
                // Lets WorldMap.unity be played on its own without a GameManager; nothing persists.
                GameLogger.Warning(LogCategory, "No WorldMapProgress registered (missing GameManager?); using a temporary, unsaved one.");
                _progress = new WorldMapProgress();
            }

            if (!ServiceLocator.TryGet(out _transition))
            {
                GameLogger.Warning(LogCategory, "No SceneTransitionService registered; encounters will be rolled but cannot load the combat scene.");
            }

            // Defeated unique bosses are never rolled again (no registry = standalone scene, no filtering).
            if (ServiceLocator.TryGet(out DefeatedEnemyRegistry defeatedEnemies))
            {
                _encounters.EnemyFilter = defeatedEnemies.CanSpawn;
            }

            _graph.ApplyUnlocks(_progress);

            if (!_nodesByRegion.TryGetValue(_progress.CurrentRegion, out _currentNode))
            {
                _nodesByRegion.TryGetValue(_progress.StartingRegion, out _currentNode);
            }

            if (_currentNode != null && _playerToken != null)
            {
                _playerToken.position = TokenPositionFor(_currentNode);
            }

            RefreshNodeVisuals();
        }

        private void Update()
        {
            HandleClick();
            TickTravel(Time.deltaTime);
        }

        /// <summary>
        /// Builds a <see cref="WorldMapGraph"/> from scene nodes: every neighbour link becomes a
        /// bidirectional connection and every node with an unlock rule registers it. Falls back to
        /// <see cref="WorldMapGraph.CreateDefault"/> when no nodes are given.
        /// </summary>
        public static WorldMapGraph BuildGraph(IEnumerable<WorldMapRegionNode> nodes, Dictionary<BiomeType, WorldMapRegionNode> nodesByRegion = null)
        {
            var graph = new WorldMapGraph();
            bool any = false;

            if (nodes != null)
            {
                foreach (WorldMapRegionNode node in nodes)
                {
                    if (node == null)
                    {
                        continue;
                    }

                    any = true;
                    if (nodesByRegion != null)
                    {
                        nodesByRegion[node.Region] = node;
                    }

                    if (node.HasUnlockRule)
                    {
                        graph.SetUnlockRule(node.Region, node.UnlockRequiresVictoriesIn, node.VictoriesRequired);
                    }

                    if (node.Neighbours == null)
                    {
                        continue;
                    }

                    foreach (WorldMapRegionNode neighbour in node.Neighbours)
                    {
                        if (neighbour != null)
                        {
                            graph.AddConnection(node.Region, neighbour.Region);
                        }
                    }
                }
            }

            return any ? graph : WorldMapGraph.CreateDefault();
        }

        /// <summary>Starts walking the token to <paramref name="node"/>. Returns false if not travelable (locked, not connected, already there or mid-travel).</summary>
        public bool TryTravelTo(WorldMapRegionNode node)
        {
            if (node == null || _progress == null || _encounterInProgress || IsTraveling || _currentNode == null)
            {
                return false;
            }

            if (!_graph.CanTravel(_progress, node.Region))
            {
                return false;
            }

            _targetNode = node;
            return true;
        }

        private void HandleClick()
        {
            if (_encounterInProgress || IsTraveling)
            {
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_camera == null)
            {
                return;
            }

            Ray ray = _camera.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, _maxRayDistance, _nodeMask, QueryTriggerInteraction.Collide))
            {
                TryTravelTo(hit.collider.GetComponentInParent<WorldMapRegionNode>());
            }
        }

        private void TickTravel(float deltaTime)
        {
            if (!IsTraveling || _playerToken == null)
            {
                return;
            }

            Vector3 before = _playerToken.position;
            Vector3 destination = TokenPositionFor(_targetNode);
            _playerToken.position = Vector3.MoveTowards(before, destination, _tokenSpeed * deltaTime);

            Vector3 flatDirection = destination - before;
            flatDirection.y = 0f;
            if (flatDirection.sqrMagnitude > 0.0001f)
            {
                _playerToken.rotation = Quaternion.LookRotation(flatDirection);
            }

            if ((_playerToken.position - destination).sqrMagnitude <= 0.0001f)
            {
                ArriveAt(_targetNode);
                return;
            }

            float moved = Vector3.Distance(before, _playerToken.position);
            WorldMapRegionNode underToken = NearestOf(_currentNode, _targetNode, _playerToken.position);
            BiomeDefinition biome = BiomeRegistry.Get(underToken.Region);

            if (_encounters.Advance(moved, deltaTime, biome, out EncounterPayload payload))
            {
                TriggerEncounter(underToken, payload);
            }
        }

        private void ArriveAt(WorldMapRegionNode node)
        {
            _targetNode = null;
            _currentNode = node;
            _progress.SetCurrentRegion(node.Region);
            RefreshNodeVisuals();

            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.AutoSave();
            }
        }

        private void TriggerEncounter(WorldMapRegionNode regionNode, EncounterPayload payload)
        {
            // Settle on the region the fight happened in, so returning from combat puts the token there.
            _targetNode = null;
            _currentNode = regionNode;
            _progress.SetCurrentRegion(regionNode.Region);
            RefreshNodeVisuals();

            EventBus.Publish(new EncounterTriggeredEvent(payload));

            if (_transition != null && _transition.BeginEncounter(payload))
            {
                _encounterInProgress = true;
                return;
            }

            if (_playerToken != null)
            {
                _playerToken.position = TokenPositionFor(regionNode);
            }

            GameLogger.Info(LogCategory, $"Encounter rolled in {payload.biome} ({string.Join(", ", payload.enemyIds)}) but no combat scene transition is available.");
        }

        private void RefreshNodeVisuals()
        {
            foreach (WorldMapRegionNode node in _nodesByRegion.Values)
            {
                node.RefreshVisuals(_progress != null && _progress.IsUnlocked(node.Region), node == _currentNode);
            }
        }

        private Vector3 TokenPositionFor(WorldMapRegionNode node)
        {
            return node.Position + Vector3.up * _tokenHeightOffset;
        }

        private static WorldMapRegionNode NearestOf(WorldMapRegionNode a, WorldMapRegionNode b, Vector3 position)
        {
            if (a == null)
            {
                return b;
            }

            if (b == null)
            {
                return a;
            }

            return (a.Position - position).sqrMagnitude <= (b.Position - position).sqrMagnitude ? a : b;
        }
    }
}
