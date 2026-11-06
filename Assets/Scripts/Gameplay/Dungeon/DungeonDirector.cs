using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.Dungeon
{
    /// <summary>
    /// Runs the Ossuário dos Artífices inside Main.unity. Self-bootstrapping like <c>EventRoomDirector</c> (no scene edit): after
    /// a scene load whose pending encounter is a dungeon (<see cref="EncounterPayload.IsDungeon"/>) a host is created in that
    /// scene and dies with it. The <see cref="EncounterArenaController"/> sees the payload as directed, dresses the floor as the
    /// caves and places the hero; this director then
    /// <list type="bullet">
    /// <item>lays the hall out from the hero toward the arena centre (<see cref="DungeonLayout"/>, <see cref="DungeonProps"/>),
    /// hiding the arena's own rocks inside it, with its gates, sarcophagi, bone wall and chest restored from
    /// <see cref="DungeonProgress"/>;</item>
    /// <item>wakes each room's guardians when the hero gets there (a room behind a closed gate waits for the gate), through
    /// <see cref="EncounterArenaController.SpawnDirected"/>, then moves them to their spots, tags them
    /// (<see cref="DungeonEnemyTag"/>: catacomb loot, room, key carrier) and adds their abilities;</item>
    /// <item>hands out the room keys (the Vestíbulo's first warrior and the Galeria's sorcerer carry one while the gate ahead
    /// is shut), keeps the fallen warriors for the sorcerer's "Erguer as Ossadas", and spawns sarcophagus ambushes;</item>
    /// <item>ends on the Colosso's death: <see cref="DungeonProgress.RecordClear"/>, a cousin line, then
    /// <see cref="EncounterArenaController.ResolveDirected"/> (Victory). Dying or walking out resolve through the arena as usual,
    /// keeping the gates already opened.</item>
    /// </list>
    /// The HUD (<c>DungeonHud</c>) polls <see cref="Current"/>.
    /// </summary>
    public class DungeonDirector : MonoBehaviour
    {
        public const string HostName = "DungeonDirector";
        private const string PlayerTag = "Player";
        private const float ArenaWaitSeconds = 6f;
        public const float ResultsSeconds = 5f;
        private const float WakeMargin = 3f;

        private readonly HashSet<GameObject> _alive = new HashSet<GameObject>();
        private readonly List<(int room, Vector3 position)> _fallen = new List<(int, Vector3)>();
        private readonly bool[] _roomAwake = new bool[3];
        private readonly Dictionary<string, EnemyDefinition> _enemies = new Dictionary<string, EnemyDefinition>();

        private EncounterPayload _payload;
        private EncounterArenaController _arena;
        private System.Random _random;
        private DungeonProgress _progress;
        private BiomeDefinition _lootTable;
        private Transform _player;
        private DungeonHall _hall;
        private GameObject _root;
        private LockedDungeonGate _galleryGate;
        private LockedDungeonGate _colossusGate;
        private GameObject _boss;

        private float _waitTimer;
        private float _graceLeft;
        private bool _setupDone;
        private bool _started;
        private bool _finished;
        private bool _cleared;
        private float _resultsLeft = -1f;
        private string _banner;
        private float _bannerLeft;
        private int _keysHeld;
        private float _keysRefresh;

        public static DungeonDirector Current { get; private set; }

        public string DungeonId => _payload != null ? _payload.dungeonId : null;
        public bool IsRunning => _started && !_finished;
        public bool IsCleared => _cleared;
        public DungeonHall Hall => _hall;
        public int AliveEnemies => _alive.Count;
        public int KeysHeld => _keysHeld;
        public string Banner => _bannerLeft > 0f ? _banner : null;
        public GameObject Boss => _boss;

        /// <summary>The room the hero stands in (0 Vestíbulo, 1 Galeria, 2 Câmara do Colosso).</summary>
        public int HeroRoom => _hall != null && _player != null ? _hall.Layout.RoomAt(_hall.ToLocal(_player.position).y) : 0;

        // ------------------------------------------------------------------
        // Bootstrap
        // ------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!ServiceLocator.TryGet(out SceneTransitionService transition) || transition.PendingEncounter == null
                || !transition.PendingEncounter.IsDungeon || FindFirstObjectByType<DungeonDirector>() != null)
            {
                return;
            }

            var host = new GameObject(HostName);
            SceneManager.MoveGameObjectToScene(host, scene);
            host.AddComponent<DungeonDirector>().Begin(transition.PendingEncounter);
        }

        /// <summary>The payload the world-map button hands to the encounter hop (regular enemies as the ids, for logs/music).</summary>
        public static EncounterPayload BuildPayload(int seed)
        {
            var regulars = new List<EnemyDefinition>();
            foreach (string id in DungeonRules.EnemyIds)
            {
                EnemyDefinition enemy = Resources.Load<EnemyDefinition>(DungeonRules.EnemyResourcePath(id));
                if (enemy != null && !enemy.IsBoss)
                {
                    regulars.Add(enemy);
                }
            }

            if (regulars.Count == 0)
            {
                return null;
            }

            EncounterPayload payload = EncounterPayload.Create(DungeonRules.Region, regulars, seed);
            payload.dungeonId = DungeonRules.OssuaryId;
            return payload;
        }

        public void Begin(EncounterPayload payload)
        {
            _payload = payload;
            _random = new System.Random(payload != null ? payload.seed : 0);
            _progress = DungeonProgress.ResolveShared();
            _progress.RecordVisit();
            _lootTable = DungeonLoot.CatacombsTable;
            foreach (string id in DungeonRules.EnemyIds)
            {
                EnemyDefinition enemy = Resources.Load<EnemyDefinition>(DungeonRules.EnemyResourcePath(id));
                if (enemy != null)
                {
                    _enemies[id] = enemy;
                }
            }

            Current = this;
            if (ServiceLocator.TryGet(out LocalizationManager localization))
            {
                DungeonTexts.EnsureRegistered(localization);
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
        }

        private void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }

            if (_galleryGate != null)
            {
                _galleryGate.Opened -= OnGateOpened;
            }

            if (_colossusGate != null)
            {
                _colossusGate.Opened -= OnGateOpened;
            }
        }

        // ------------------------------------------------------------------
        // Loop
        // ------------------------------------------------------------------

        private void Update()
        {
            if (_payload == null)
            {
                return;
            }

            if (_bannerLeft > 0f)
            {
                _bannerLeft -= Time.unscaledDeltaTime;
            }

            if (_resultsLeft >= 0f)
            {
                _resultsLeft -= Time.unscaledDeltaTime;
                if (_resultsLeft < 0f && _arena != null && _arena.IsEncounterActive)
                {
                    _arena.ResolveDirected(EncounterOutcome.Victory);
                }

                return;
            }

            if (!_started)
            {
                TryStart();
                return;
            }

            if (_finished)
            {
                return;
            }

            if (_arena == null || !_arena.IsEncounterActive)
            {
                _finished = true;
                return;
            }

            RefreshKeys();
            PruneDead();
            WakeRooms();
        }

        private void TryStart()
        {
            if (_arena == null)
            {
                _arena = FindFirstObjectByType<EncounterArenaController>();
            }

            if (_arena == null || !_arena.IsDirected)
            {
                _waitTimer += Time.unscaledDeltaTime;
                if (_waitTimer > ArenaWaitSeconds)
                {
                    GameLogger.Warning(HostName, "No arena in directed mode; leaving the dungeon.");
                    _payload = null;
                    if (ServiceLocator.TryGet(out SceneTransitionService transition))
                    {
                        transition.CompleteEncounter(EncounterOutcome.Fled);
                    }
                }

                return;
            }

            if (!_setupDone)
            {
                _setupDone = true;
                GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
                _player = player != null ? player.transform : null;
                _graceLeft = Mathf.Max(0.01f, _arena.IntroGraceSeconds);
                BuildDungeon();
                ShowBanner(CousinLine(false), 4.5f);
                return;
            }

            _graceLeft -= Time.deltaTime;
            if (_graceLeft > 0f)
            {
                return;
            }

            _started = true;
        }

        // ------------------------------------------------------------------
        // Building
        // ------------------------------------------------------------------

        private void BuildDungeon()
        {
            Vector3 origin = _player != null ? _player.position : _arena.ArenaCenter;
            Vector3 toCenter = _arena.ArenaCenter - origin;
            toCenter.y = 0f;
            Vector3 forward = toCenter.sqrMagnitude > 4f ? toCenter.normalized : (_player != null ? _player.forward : Vector3.forward);
            float safeRadius = (_arena.SpawnArenaRadius > 0f ? _arena.SpawnArenaRadius : 18f) + 8f;
            var layout = new DungeonLayout(DungeonLayout.FitLength(toCenter.magnitude, safeRadius));
            _hall = new DungeonHall(layout, Ground(origin), forward);

            _root = new GameObject("Ossuario");
            SceneManager.MoveGameObjectToScene(_root, gameObject.scene);
            HideArenaPropsInside();

            GameObject hallProps = DungeonProps.BuildHall(_hall, Ground);
            hallProps.transform.SetParent(_root.transform, true);

            _galleryGate = DungeonProps.BuildGate(_hall, layout.GalleryGateZ, DungeonRules.GateGalleryId, Ground);
            _colossusGate = DungeonProps.BuildGate(_hall, layout.ColossusGateZ, DungeonRules.GateColossusId, Ground);
            foreach (LockedDungeonGate gate in new[] { _galleryGate, _colossusGate })
            {
                gate.transform.SetParent(_root.transform, true);
                gate.Opened += OnGateOpened;
            }

            IReadOnlyList<DungeonSpot> sarcophagi = layout.Sarcophagi;
            for (int i = 0; i < sarcophagi.Count && i < DungeonRules.SarcophagusIds.Count; i++)
            {
                SarcophagusInteractable sarcophagus = DungeonProps.BuildSarcophagus(_hall, sarcophagi[i], DungeonRules.SarcophagusIds[i], Ground);
                sarcophagus.transform.SetParent(_root.transform, true);
                sarcophagus.Random = _random;
                sarcophagus.AmbushRequested = SpawnAmbush;
            }

            BreakableBoneWall wall = DungeonProps.BuildBoneWall(_hall, layout.BoneWall, DungeonRules.BoneWallId, Ground);
            wall.transform.SetParent(_root.transform, true);
            SarcophagusInteractable chest = DungeonProps.BuildLegendaryChest(_hall, layout.LegendaryChest, DungeonRules.LegendaryChestId, DungeonRules.BoneChestItemId, Ground);
            chest.transform.SetParent(_root.transform, true);
            chest.Random = _random;
        }

        /// <summary>The caves dressing scatters rocks and stalagmites over the arena: the ones inside the hall would block it.</summary>
        private void HideArenaPropsInside()
        {
            foreach (Transform t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if (t == null || !t.gameObject.activeInHierarchy)
                {
                    continue;
                }

                string n = t.name;
                if (!n.StartsWith("ArenaProp_", System.StringComparison.Ordinal) && !n.StartsWith("ArenaSecret_", System.StringComparison.Ordinal))
                {
                    continue;
                }

                Vector2 local = _hall.ToLocal(t.position);
                if (local.y > -1.5f && local.y < _hall.Layout.Length + 1.5f && Mathf.Abs(local.x) < DungeonLayout.BossHalfWidth + DungeonLayout.NicheDepth + 1.5f)
                {
                    t.gameObject.SetActive(false);
                }
            }
        }

        private Vector3 Ground(Vector3 point)
        {
            Vector3 from = point + Vector3.up * 20f;
            float best = float.NegativeInfinity;
            LayerMask mask = _arena != null ? _arena.GroundMask : (LayerMask)~0;
            foreach (RaycastHit hit in Physics.RaycastAll(from, Vector3.down, 40f, mask, QueryTriggerInteraction.Ignore))
            {
                Transform t = hit.collider.transform;
                if ((_player != null && t.IsChildOf(_player)) || (_root != null && t.IsChildOf(_root.transform))
                    || hit.collider.GetComponentInParent<EnemyController>() != null)
                {
                    continue;
                }

                if (hit.point.y <= point.y + 2.5f && hit.point.y > best)
                {
                    best = hit.point.y;
                }
            }

            if (!float.IsNegativeInfinity(best))
            {
                point.y = best;
            }

            return point;
        }

        // ------------------------------------------------------------------
        // Rooms and guardians
        // ------------------------------------------------------------------

        /// <summary>Who guards each room, by spawn spot (<see cref="DungeonLayout.SpawnSpots"/>).</summary>
        public static readonly IReadOnlyList<string>[] RoomGuardians =
        {
            new[] { DungeonRules.SkeletonWarriorId, DungeonRules.SkeletonWarriorId, DungeonRules.FloatingSkullId, DungeonRules.FloatingSkullId },
            new[] { DungeonRules.SkeletonArcherId, DungeonRules.SkeletonArcherId, DungeonRules.SkeletonWarriorId, DungeonRules.SkeletonWarriorId, DungeonRules.BoneSorcererId },
            new[] { DungeonRules.BoneColossusId },
        };

        /// <summary>Which guardian (index in <see cref="RoomGuardians"/>) carries the key of the gate ahead of its room.</summary>
        public static readonly int[] KeyCarrierIndex = { 0, 4, -1 };

        /// <summary>Whether room <paramref name="room"/> may wake: the hero is close and every gate before it is open (pure, tested).</summary>
        public static bool ShouldWake(int room, float heroZ, DungeonLayout layout, bool galleryOpen, bool colossusOpen)
        {
            if (room == 0)
            {
                return true;
            }

            bool gatesOpen = room == 1 ? galleryOpen : galleryOpen && colossusOpen;
            return gatesOpen && heroZ >= layout.RoomStart(room) - WakeMargin;
        }

        private void WakeRooms()
        {
            float heroZ = _hall != null && _player != null ? _hall.ToLocal(_player.position).y : 0f;
            for (int room = 0; room < _roomAwake.Length; room++)
            {
                if (_roomAwake[room] || !ShouldWake(room, heroZ, _hall.Layout, _progress.IsGateOpen(DungeonRules.GateGalleryId), _progress.IsGateOpen(DungeonRules.GateColossusId)))
                {
                    continue;
                }

                _roomAwake[room] = true;
                WakeRoom(room);
            }
        }

        private void WakeRoom(int room)
        {
            IReadOnlyList<DungeonSpot> spots = _hall.Layout.SpawnSpots(room);
            IReadOnlyList<string> guardians = RoomGuardians[room];
            string gateAhead = room == 0 ? DungeonRules.GateGalleryId : room == 1 ? DungeonRules.GateColossusId : null;
            for (int i = 0; i < guardians.Count && i < spots.Count; i++)
            {
                GameObject enemy = SpawnGuardian(guardians[i], _hall.ToWorld(spots[i]), room, i, guardians.Count, room == 0 ? 0.6f : 0.9f, false);
                if (enemy != null && i == KeyCarrierIndex[room] && gateAhead != null && !_progress.IsGateOpen(gateAhead))
                {
                    enemy.GetComponent<DungeonEnemyTag>().CarriesKey = true;
                }
            }

            ShowBanner(DungeonFx.Text(DungeonTexts.RoomNameKey(room)), 2.5f);
            if (room == 2 && _boss != null && _enemies.TryGetValue(DungeonRules.BoneColossusId, out EnemyDefinition boss))
            {
                EventBus.Publish(new BossEncounterIntroEvent(_payload, boss));
                ShowBanner(DungeonFx.Text(DungeonTexts.ColossusAwakes), 3f);
            }
        }

        private GameObject SpawnGuardian(string enemyId, Vector3 position, int room, int index, int count, float hold, bool reanimated)
        {
            if (_arena == null || !_enemies.TryGetValue(enemyId, out EnemyDefinition definition))
            {
                return null;
            }

            GameObject enemy = _arena.SpawnDirected(definition, index, count, hold);
            if (enemy == null)
            {
                return null;
            }

            Teleport(enemy, _hall.Clamp(position, 0.7f));
            DungeonEnemyTag.Attach(enemy, enemyId, room, _lootTable, reanimated);
            // Level 35–45 (map 24b): directed spawns get no scaler from the arena, so the ossuary puts it on itself.
            EnemyLevelScaler.Attach(enemy, EnemyLevelScaling.ResolveLevel(definition.Level, DungeonRules.EnemyLevel(enemyId)));
            DungeonEnemyBehaviour behaviour = AddBehaviour(enemy, enemyId);
            behaviour?.Configure(_hall);
            if (behaviour is BoneSorcererBehaviour sorcerer)
            {
                sorcerer.Bind(this);
            }

            if (enemyId == DungeonRules.BoneColossusId)
            {
                _boss = enemy;
            }

            _alive.Add(enemy);
            return enemy;
        }

        /// <summary>The ability component of each ossuary enemy (bound to its brain: shooters take over the strike).</summary>
        public static DungeonEnemyBehaviour AddBehaviour(GameObject enemy, string enemyId)
        {
            DungeonEnemyBehaviour behaviour;
            switch (enemyId)
            {
                case DungeonRules.FloatingSkullId: behaviour = enemy.AddComponent<FloatingSkullBehaviour>(); break;
                case DungeonRules.SkeletonWarriorId: behaviour = enemy.AddComponent<SkeletonGuardBehaviour>(); break;
                case DungeonRules.SkeletonArcherId: behaviour = enemy.AddComponent<SkeletonArcherBehaviour>(); break;
                case DungeonRules.BoneSorcererId: behaviour = enemy.AddComponent<BoneSorcererBehaviour>(); break;
                case DungeonRules.BoneColossusId: behaviour = enemy.AddComponent<BoneColossusBehaviour>(); break;
                default: return null;
            }

            behaviour.BindBrain(enemy.GetComponent<EnemyController>());
            return behaviour;
        }

        private void Teleport(GameObject enemy, Vector3 target)
        {
            // Keep the pivot lift the arena gave it above its own ground.
            float lift = enemy.transform.position.y - Ground(enemy.transform.position).y;
            target = Ground(target);
            target.y += Mathf.Max(0f, lift);
            var body = enemy.GetComponent<CharacterController>();
            bool wasEnabled = body != null && body.enabled;
            if (body != null)
            {
                body.enabled = false;
            }

            enemy.transform.position = target;
            if (body != null)
            {
                body.enabled = wasEnabled;
            }
        }

        private void SpawnAmbush(Vector3 position, int count)
        {
            if (!IsRunning)
            {
                return;
            }

            int room = _hall.Layout.RoomAt(_hall.ToLocal(position).y);
            for (int i = 0; i < count; i++)
            {
                Vector3 offset = _hall.Right * ((i - (count - 1) * 0.5f) * 1.2f);
                SpawnGuardian(DungeonRules.FloatingSkullId, position + offset, room, i, count, 0.3f, false);
            }
        }

        private void OnGateOpened(LockedDungeonGate gate)
        {
            DungeonFx.Say(gate.transform.position + Vector3.up * 3.5f, DungeonFx.Text(DungeonTexts.GateOpened));
        }

        private void RefreshKeys()
        {
            _keysRefresh -= Time.unscaledDeltaTime;
            if (_keysRefresh > 0f)
            {
                return;
            }

            _keysRefresh = 0.25f;
            _keysHeld = _player != null && _player.TryGetComponent(out InventoryManager bag) ? bag.GetTotalCount(DungeonRules.BoneKeyItemId) : 0;
        }

        private void PruneDead()
        {
            _alive.RemoveWhere(enemy => enemy == null);
        }

        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            if (evt.Enemy == null || !_alive.Remove(evt.Enemy) || _finished)
            {
                return;
            }

            DungeonEnemyTag tag = evt.Enemy.GetComponent<DungeonEnemyTag>();
            if (tag == null)
            {
                return;
            }

            if (tag.EnemyId == DungeonRules.SkeletonWarriorId && !tag.IsReanimated)
            {
                _fallen.Add((tag.Room, evt.Enemy.transform.position));
            }

            if (tag.CarriesKey)
            {
                GrantKey(evt.Enemy);
            }

            if (evt.Enemy == _boss)
            {
                Clear();
            }
        }

        private void GrantKey(GameObject from)
        {
            if (_player == null || !_player.TryGetComponent(out InventoryManager bag) || !ItemDatabase.TryGetById(DungeonRules.BoneKeyItemId, out ItemDefinition key) || key == null)
            {
                return;
            }

            DungeonLoot.GrantToHero(bag, new[] { new LootDrop(key, 1) }, from);
            ShowBanner(DungeonFx.Text(DungeonTexts.KeyFound), 2.5f);
        }

        // ------------------------------------------------------------------
        // "Erguer as Ossadas"
        // ------------------------------------------------------------------

        /// <summary>Whether a fallen (never relit) warrior of the sorcerer's room lies within reach.</summary>
        public bool HasFallenToRaise(Vector3 sorcerer)
        {
            return DungeonRules.PickRaiseTargets(sorcerer, FallenOf(RoomOf(sorcerer), out _)).Count > 0;
        }

        /// <summary>Relights up to two fallen warriors near the sorcerer (flagged reanimated: 40% health, 0 XP, no drop).</summary>
        public int TryRaiseFallen(BoneSorcererBehaviour sorcerer, Vector3 at)
        {
            if (!IsRunning || sorcerer == null)
            {
                return 0;
            }

            List<Vector3> corpses = FallenOf(RoomOf(at), out List<int> indices);
            List<int> picked = DungeonRules.PickRaiseTargets(at, corpses);
            var raisedIndices = new List<int>();
            for (int i = 0; i < picked.Count; i++)
            {
                Vector3 p = corpses[picked[i]];
                DungeonPuff.Spawn(p + Vector3.up * 0.6f, DungeonProps.SmokePurple, 1.6f, 0.8f);
                if (SpawnGuardian(DungeonRules.SkeletonWarriorId, p, RoomOf(p), i, picked.Count, 0.8f, true) != null)
                {
                    raisedIndices.Add(indices[picked[i]]);
                }
            }

            raisedIndices.Sort();
            for (int i = raisedIndices.Count - 1; i >= 0; i--)
            {
                _fallen.RemoveAt(raisedIndices[i]);
            }

            return raisedIndices.Count;
        }

        private int RoomOf(Vector3 world) => _hall != null ? _hall.Layout.RoomAt(_hall.ToLocal(world).y) : 0;

        private List<Vector3> FallenOf(int room, out List<int> indices)
        {
            var positions = new List<Vector3>();
            indices = new List<int>();
            for (int i = 0; i < _fallen.Count; i++)
            {
                if (_fallen[i].room == room)
                {
                    positions.Add(_fallen[i].position);
                    indices.Add(i);
                }
            }

            return positions;
        }

        // ------------------------------------------------------------------
        // End
        // ------------------------------------------------------------------

        private void Clear()
        {
            if (_cleared)
            {
                return;
            }

            _cleared = true;
            _finished = true;
            _progress.RecordClear(DungeonId ?? DungeonRules.OssuaryId);
            foreach (GameObject enemy in _alive)
            {
                if (enemy != null && enemy.TryGetComponent(out EnemyController controller))
                {
                    controller.HoldAggro(60f);
                }
            }

            ShowBanner(DungeonFx.Text(DungeonTexts.Cleared) + "  “" + CousinLine(true) + "”", ResultsSeconds);
            GameLogger.Info(HostName, $"Ossuary cleared (clear #{_progress.Clears}).");
            _resultsLeft = ResultsSeconds;
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        private void ShowBanner(string text, float seconds)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            _banner = text;
            _bannerLeft = seconds;
        }

        private static string CousinLine(bool cleared)
        {
            CharacterId hero = ServiceLocator.TryGet(out CharacterSelectionState selection) && selection.Selected.HasValue
                ? selection.Selected.Value
                : CharacterId.Maithe;
            return DungeonFx.Text(DungeonTexts.LineKey(cleared, hero));
        }

        /// <summary>The current objective line for the HUD.</summary>
        public string GoalText(LocalizationManager localization)
        {
            return DungeonTexts.Get(localization, GoalKey(_cleared, _roomAwake[2], HeroRoom, CountAliveIn(HeroRoom),
                _progress != null && _progress.IsGateOpen(DungeonRules.GateGalleryId), _progress != null && _progress.IsGateOpen(DungeonRules.GateColossusId)));
        }

        /// <summary>Which goal line fits the run's state (pure, tested).</summary>
        public static string GoalKey(bool cleared, bool bossAwake, int heroRoom, int aliveInHeroRoom, bool galleryOpen, bool colossusOpen)
        {
            if (cleared)
            {
                return DungeonTexts.GoalCleared;
            }

            if (bossAwake || heroRoom >= 2)
            {
                return DungeonTexts.GoalBoss;
            }

            bool gateAheadOpen = heroRoom == 0 ? galleryOpen : colossusOpen;
            if (aliveInHeroRoom > 0)
            {
                return heroRoom == 0 ? DungeonTexts.GoalVestibule : DungeonTexts.GoalGallery;
            }

            return gateAheadOpen ? DungeonTexts.GoalGallery : DungeonTexts.GoalGate;
        }

        private int CountAliveIn(int room)
        {
            int count = 0;
            foreach (GameObject enemy in _alive)
            {
                if (enemy != null && enemy.TryGetComponent(out DungeonEnemyTag tag) && tag.Room == room)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
