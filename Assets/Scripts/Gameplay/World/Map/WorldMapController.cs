using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Logging;
using Rapadura.Core.Systems;
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

        [Header("Region boss challenge")]
        [Tooltip("Regular enemies rolled alongside the region boss when \"Enfrentar o chefe\" starts.")]
        [SerializeField] [Min(0)] private int _bossMinionsMin = RegionBossProgress.DefaultMinMinions;
        [SerializeField] [Min(0)] private int _bossMinionsMax = RegionBossProgress.DefaultMaxMinions;

        [Header("Input")]
        [SerializeField] private Camera _camera;
        [SerializeField] private LayerMask _nodeMask = ~0;
        [SerializeField] private float _maxRayDistance = 500f;

        private readonly Dictionary<BiomeType, WorldMapRegionNode> _nodesByRegion = new Dictionary<BiomeType, WorldMapRegionNode>();
        private WorldMapGraph _graph;
        private RandomEncounterSystem _encounters;
        private RegionBossProgress _bosses;
        private System.Random _challengeRandom;
        private WorldMapProgress _progress;
        private SceneTransitionService _transition;
        private WorldMapRegionNode _currentNode;
        private WorldMapRegionNode _targetNode;
        private bool _encounterInProgress;

        public WorldMapProgress Progress => _progress;
        public WorldMapGraph Graph => _graph;
        public RandomEncounterSystem Encounters => _encounters;

        /// <summary>Region boss rules for this map (null before Start). HUD API: CanChallengeBoss/BossFor/IsRegionCleared.</summary>
        public RegionBossProgress RegionBosses => _bosses;

        /// <summary>"Enfrentar o chefe" can be pressed right now: offered for the current region and the token is idle.</summary>
        public bool CanChallengeCurrentBoss =>
            _bosses != null && _currentNode != null && !_encounterInProgress && !IsTraveling && _bosses.CanChallengeBoss(_currentNode.Region);
        public WorldMapRegionNode CurrentNode => _currentNode;
        public bool IsTraveling => _targetNode != null;
        public IReadOnlyList<WorldMapRegionNode> Nodes => _nodes;

        /// <summary>
        /// Optional screen-space filter for map clicks/taps (bottom-left origin, like <c>Mouse.position</c>/<c>Touch.position</c>):
        /// return true to swallow the click, e.g. when it lands on the world map HUD (WorldMap.unity
        /// has no EventSystem, so UI Toolkit hits are not seen by <c>IsPointerOverGameObject</c>).
        /// </summary>
        public System.Func<Vector2, bool> PointerBlocker { get; set; }

        /// <summary>
        /// When set, every map seeds its encounter/boss-minion RNGs with this instead of the clock (or
        /// the inspector's fixed seed): reproducible rolls for PlayMode tests. Null (default) in the game.
        /// </summary>
        public static int? SeedOverride { get; set; }

        private void Awake()
        {
            _graph = BuildGraph(_nodes, _nodesByRegion);
            int seed = SeedOverride ?? (_useFixedSeed ? _seed : System.Environment.TickCount);
            _encounters = new RandomEncounterSystem(_encounterSettings, seed) { Enabled = _encountersEnabled };
            _challengeRandom = new System.Random(unchecked(seed * 31 + 7));
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
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

            // Region bosses are only fought through "Enfrentar o chefe", and defeated unique bosses
            // are never rolled again (no registry = standalone scene: only the region-boss filter).
            ServiceLocator.TryGet(out DefeatedEnemyRegistry defeatedEnemies);
            _bosses = new RegionBossProgress(_graph, _progress, BiomeRegistry.Get, defeatedEnemies);
            _encounters.EnemyFilter = _bosses.CanRollInNormalEncounter;

            SyncWithProgress();
        }

        /// <summary>
        /// Puts the per-visit state (current node, token, node visuals, unlocks, boss button) back in
        /// line with <see cref="WorldMapProgress"/>. Run by Start, and again on every
        /// <see cref="GameLoadedEvent"/>: a save loaded from the pause menu while on the map
        /// (SaveLoadMenuController in WorldMap.unity) restores WorldMapProgress in place, and without
        /// this the token, <see cref="CurrentNode"/> (which Explore/boss challenge use) and the
        /// locked/unlocked visuals kept the previous session's region until the scene reloaded.
        /// </summary>
        public void SyncWithProgress()
        {
            // Before Start (no progress yet) or while the combat scene is loading: nothing to sync.
            if (_progress == null || _bosses == null || _encounterInProgress)
            {
                return;
            }

            _targetNode = null;

            // Old saves: a boss beaten before the challenge existed counts as cleared.
            _bosses.SyncClearedFromDefeatedBosses();
            _graph.ApplyUnlocks(_progress, _bosses.IsUnlockGateSatisfied);

            if (!_nodesByRegion.TryGetValue(_progress.CurrentRegion, out _currentNode))
            {
                _nodesByRegion.TryGetValue(_progress.StartingRegion, out _currentNode);
            }

            if (_currentNode != null && _playerToken != null)
            {
                _playerToken.position = TokenPositionFor(_currentNode);
            }

            RefreshNodeVisuals();
            PublishBossChallengeState();
        }

        private void OnGameLoaded(GameLoadedEvent evt) => SyncWithProgress();

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

            // Tapping the region the token is already on explores it: at the start only Forest is
            // unlocked, so without this there is nowhere to walk and no encounter can ever happen.
            if (node == _currentNode)
            {
                return TryExplore();
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
            // A modal screen (pause menu, quest log...) holds GameplayPause: never walk behind it.
            if (_encounterInProgress || IsTraveling || GameplayPause.IsPaused)
            {
                return;
            }

            if (!TryGetPressThisFrame(out Vector2 screenPosition))
            {
                return;
            }

            if (PointerBlocker != null && PointerBlocker(screenPosition))
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

            Ray ray = _camera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, _maxRayDistance, _nodeMask, QueryTriggerInteraction.Collide))
            {
                TryTravelTo(hit.collider.GetComponentInParent<WorldMapRegionNode>());
            }
        }

        /// <summary>
        /// A tap (Touchscreen primary touch) or left click that started this frame, in screen space
        /// (bottom-left origin for both devices). Touch is checked first: this is a touch-first game,
        /// and on devices with both, the touch already reports its own position.
        /// </summary>
        private static bool TryGetPressThisFrame(out Vector2 screenPosition)
        {
            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
            {
                screenPosition = touchscreen.primaryTouch.position.ReadValue();
                return true;
            }

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                screenPosition = mouse.position.ReadValue();
                return true;
            }

            screenPosition = default;
            return false;
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
            PublishBossChallengeState();

            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.AutoSave();
            }
        }

        /// <summary>True when "Explorar" can start a fight in the current region (idle token, no encounter pending).</summary>
        public bool CanExplore =>
            _currentNode != null && _encounters != null && !_encounterInProgress && !IsTraveling;

        /// <summary>
        /// "Explorar": rolls a normal encounter in the current region right away (same enemy table and
        /// boss filter as random encounters while walking). False when not possible or nothing rolls.
        /// </summary>
        public bool TryExplore()
        {
            if (!CanExplore || GameplayPause.IsPaused)
            {
                return false;
            }

            EncounterPayload payload = _encounters.BuildPayload(BiomeRegistry.Get(_currentNode.Region));
            if (payload == null)
            {
                return false;
            }

            TriggerEncounter(_currentNode, payload);
            return true;
        }

        /// <summary>
        /// "Enfrentar o chefe": starts the current region's boss challenge (boss + optional minions,
        /// <see cref="EncounterPayload.isBossChallenge"/>) through the same encounter hand-off as a
        /// random roll. False when not offered (<see cref="CanChallengeCurrentBoss"/>).
        /// </summary>
        public bool TryChallengeBoss()
        {
            if (!CanChallengeCurrentBoss || GameplayPause.IsPaused)
            {
                return false;
            }

            EncounterPayload payload = _bosses.BuildChallengePayload(_currentNode.Region, _challengeRandom, _bossMinionsMin, _bossMinionsMax);
            if (payload == null)
            {
                return false;
            }

            TriggerEncounter(_currentNode, payload);
            PublishBossChallengeState();
            return true;
        }

        private void PublishBossChallengeState()
        {
            if (_bosses == null || _currentNode == null)
            {
                return;
            }

            BiomeType region = _currentNode.Region;
            EventBus.Publish(new RegionBossChallengeStateChangedEvent(region, _bosses.BossFor(region), CanChallengeCurrentBoss));
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
