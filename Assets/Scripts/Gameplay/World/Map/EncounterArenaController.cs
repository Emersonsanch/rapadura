using System.Collections;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map
{
    /// <summary>
    /// Lives in the combat scene (Main.unity). When the scene was entered from the world map with a
    /// pending <see cref="EncounterPayload"/> (see <see cref="SceneTransitionService"/>), it moves the
    /// player to the arena spawn, spawns each rolled enemy from <see cref="_enemyPrefab"/> at the
    /// configured spawn points (reusing <see cref="EnemyController.ResetForSpawn"/>, so combat is the
    /// existing real-time Hitbox/Health/SkillManager loop) and reports the outcome back:
    /// all spawned enemies dead → Victory, player dead → Defeat, player walks out of
    /// <see cref="_fleeRadius"/> (or <see cref="Flee"/> is called from UI) → Fled.
    ///
    /// Without a pending encounter (Main.unity played directly) it does nothing, so the existing
    /// standalone combat sandbox keeps working.
    ///
    /// Placement: with <see cref="_spawnOnRingAroundPlayer"/> (default) enemies stand on an arc
    /// <see cref="_spawnMinRadius"/>-<see cref="_spawnMaxRadius"/> m in front of the player
    /// (<see cref="EncounterSpawnLayout"/>), clamped inside <see cref="_spawnArenaRadius"/> and dropped
    /// onto the ground with a raycast; the scene's spawn points are the fallback (no player found).
    /// Intro: for <see cref="_introGraceSeconds"/> every spawned enemy holds its aggro
    /// (<see cref="EnemyController.HoldAggro"/>: no chase, no attacks — damage to them still lands)
    /// while <see cref="EncounterIntroEvent"/> drives the "Prepare-se!" countdown.
    ///
    /// Why enemies are Instantiated here and not taken from <see cref="EnemyPool"/>: Main.unity is
    /// loaded fresh for every encounter and unloaded on return to the map, so a pool would never get
    /// to reuse anything (it dies with the scene, same as these instances), and corpses must stay in
    /// the scene after death for <see cref="EnemyDeathVfx"/>'s sink/fade and the Victory return delay
    /// — releasing them into a pool on death would cut that short. All spawned enemies go away with
    /// the scene unload. Revisit if encounters ever chain inside one scene load.
    ///
    /// Reinforcement waves (day/night, <see cref="NightDangerRules"/>): when a wave is cleared and the phase at that
    /// moment asks for another (day 1 more, night 2 more, faster and bigger), the next wave is rolled from the
    /// region x period table (<see cref="PeriodSpawnTable"/>) after the phase's interval; the encounter is a Victory
    /// once the last wave is cleared. A phase change mid-fight therefore takes effect on the next wave. Never for boss
    /// challenges, groups with a boss, the elite ambush or the first fight (tutorial). At night/dawn/dusk some regular
    /// enemies of the first wave come as night variants (<see cref="NightVariantTag"/>).
    /// </summary>
    public class EncounterArenaController : MonoBehaviour
    {
        private const string LogCategory = "EncounterArena";
        private const string PlayerTag = "Player";

        [Header("Spawning")]
        [Tooltip("Enemy prefab with EnemyController + Health (+ Hitbox). Reconfigured per rolled EnemyDefinition; its visual is swapped by EnemyVisualSwapper (definition model > placeholder > this prefab's own).")]
        [SerializeField] private GameObject _enemyPrefab;
        [Tooltip("Where rolled enemies appear (cycled if there are more enemies than points). Falls back to this transform.")]
        [SerializeField] private Transform[] _enemySpawnPoints = new Transform[0];
        [Tooltip("Optional patrol waypoints handed to spawned enemies.")]
        [SerializeField] private Transform[] _enemyWaypoints;
        [Tooltip("Optional: where the player is placed when the encounter starts.")]
        [SerializeField] private SpawnPoint _playerSpawn;
        [Tooltip("Place enemies on an arc in front of the player (spawn points become the fallback). Off = the old fixed spawn points.")]
        [SerializeField] private bool _spawnOnRingAroundPlayer = true;
        [Tooltip("Closest an enemy spawns to the player (m).")]
        [SerializeField] [Min(1f)] private float _spawnMinRadius = EncounterSpawnLayout.DefaultMinRadius;
        [Tooltip("Farthest an enemy spawns from the player (m).")]
        [SerializeField] [Min(1f)] private float _spawnMaxRadius = EncounterSpawnLayout.DefaultMaxRadius;
        [Tooltip("Width of the arc in front of the player the enemies are spread over (degrees).")]
        [SerializeField] [Range(0f, 360f)] private float _spawnArcDegrees = EncounterSpawnLayout.DefaultArcDegrees;
        [Tooltip("Spawns are kept within this distance of the arena centre (this transform). Keep it below the flee radius. 0 = no clamp.")]
        [SerializeField] [Min(0f)] private float _spawnArenaRadius = 18f;
        [Tooltip("Minimum XZ gap between two spawned enemies; ring points squeezed together by the arena clamp are slid apart.")]
        [SerializeField] [Min(0f)] private float _spawnMinSpacing = EncounterSpawnLayout.DefaultMinSpacing;
        [Tooltip("Layers the spawn ground raycast may land on.")]
        [SerializeField] private LayerMask _groundMask = ~0;

        [Header("Intro")]
        [Tooltip("Seconds after spawning during which enemies hold their aggro (no chase/attacks) while \"Prepare-se!\" plays. 0 = fight at once.")]
        [SerializeField] [Min(0f)] private float _introGraceSeconds = 1.75f;

        [Header("Scene cleanup")]
        [Tooltip("Objects disabled while an encounter is active (e.g. the sandbox EnemySpawner or pre-placed test enemies).")]
        [SerializeField] private GameObject[] _disableDuringEncounter = new GameObject[0];
        [Tooltip("Re-dresses the arena for the encounter's biome. Empty = found in the scene, else added to this object at runtime.")]
        [SerializeField] private BiomeArenaDressing _biomeDressing;

        [Header("Reinforcement waves (day/night)")]
        [Tooltip("Send reinforcement waves after the first one is cleared (NightDangerRules: day 1, night 2). Off = one wave, as before.")]
        [SerializeField] private bool _reinforcementWaves = true;

        [Header("Resolution")]
        [Tooltip("Player farther than this from the arena center (this transform) counts as fleeing. 0 disables distance fleeing.")]
        [SerializeField] [Min(0f)] private float _fleeRadius = 30f;
        [Tooltip("Seconds to wait after the outcome before returning to the map (lets death VFX/loot/XP play out).")]
        [SerializeField] [Min(0f)] private float _returnDelaySeconds = 1.5f;

        private readonly WaveTracker _tracker = new WaveTracker();
        private readonly HashSet<GameObject> _spawned = new HashSet<GameObject>();

        /// <summary>Ground points already used by this encounter's spawns (for <see cref="_spawnMinSpacing"/>).</summary>
        private readonly List<Vector3> _spawnGroundPoints = new List<Vector3>();
        private SceneTransitionService _transition;
        private Transform _player;
        private bool _active;
        private bool _resolved;

        private bool _allowReinforcements;
        private int _wavesSpawned;
        private int _firstWaveRegularCount;
        private BiomeType _region;
        private System.Random _waveRandom;
        private bool _waveIncoming;

        public bool IsEncounterActive => _active && !_resolved;
        public int AliveEnemies => _tracker.AliveCount;
        public float IntroGraceSeconds => _introGraceSeconds;

        // ---------------------------------------------------------------
        // Directed mode (timed event rooms, EventRoomDirector)
        // ---------------------------------------------------------------

        /// <summary>True when the pending encounter is an event room: an external director spawns and resolves.</summary>
        public bool IsDirected { get; private set; }

        public Vector3 ArenaCenter => transform.position;
        public float SpawnArenaRadius => _spawnArenaRadius;
        public LayerMask GroundMask => _groundMask;

        /// <summary>
        /// Directed mode: spawns <paramref name="definition"/> exactly like a rolled enemy (ring in front of the hero,
        /// ground snap, visual swap, aggro hold). <paramref name="index"/> 0 starts a new wave's spacing. Null when not directed.
        /// </summary>
        public GameObject SpawnDirected(EnemyDefinition definition, int index, int count, float aggroHoldSeconds)
        {
            if (!IsDirected || !IsEncounterActive || definition == null || _enemyPrefab == null)
            {
                return null;
            }

            if (index == 0)
            {
                _spawnGroundPoints.Clear();
            }

            return SpawnEnemy(definition, index, Mathf.Max(1, count), aggroHoldSeconds);
        }

        /// <summary>Directed mode: ends the encounter (the usual return delay, then back to the map).</summary>
        public void ResolveDirected(EncounterOutcome outcome)
        {
            if (IsDirected)
            {
                Resolve(outcome);
            }
        }

        /// <summary>
        /// Process-wide switch on top of the Inspector's <c>_reinforcementWaves</c>: PlayMode playthroughs that clear one
        /// wave and expect the Victory at once turn it off (and back on in their TearDown).
        /// </summary>
        public static bool ReinforcementWavesEnabled { get; set; } = true;

        /// <summary>Waves spawned so far this encounter (1 after the start).</summary>
        public int WavesSpawned => _wavesSpawned;

        /// <summary>True between a cleared wave and the arrival of the next reinforcement wave.</summary>
        public bool IsWaveIncoming => _waveIncoming;

        /// <summary>Highest a ground hit may sit above the player's feet before it counts as a prop top (tree, rock).</summary>
        private const float MaxGroundStepUp = 2.5f;

        private void OnEnable()
        {
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _transition) || !_transition.HasPendingEncounter)
            {
                return;
            }

            BeginArena(_transition.PendingEncounter);
        }

        private void Update()
        {
            if (!IsEncounterActive || _fleeRadius <= 0f || _player == null)
            {
                return;
            }

            Vector3 offset = _player.position - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude > _fleeRadius * _fleeRadius)
            {
                Resolve(EncounterOutcome.Fled);
            }
        }

        /// <summary>Gives up on the current encounter (e.g. a "Fugir" HUD button).</summary>
        public void Flee()
        {
            Resolve(EncounterOutcome.Fled);
        }

        private void BeginArena(EncounterPayload payload)
        {
            foreach (GameObject toDisable in _disableDuringEncounter)
            {
                if (toDisable != null)
                {
                    toDisable.SetActive(false);
                }
            }

            ResolveBiomeDressing().Apply(payload.biome, payload.seed, transform.position);

            GameObject playerObject = GameObject.FindGameObjectWithTag(PlayerTag);
            _player = playerObject != null ? playerObject.transform : null;

            if (_player != null && _playerSpawn != null)
            {
                MovePlayer(_player, _playerSpawn);
            }

            if (_player != null && _player.TryGetComponent(out PlayerMotor motor))
            {
                // The spawn point (or the carried-over position) can sit above the re-dressed biome
                // floor (Mountain: 1.08 m) - stand the hero on it before the enemies are placed
                // relative to the player's feet.
                motor.SnapToGround(_groundMask);
            }

            if (_enemyPrefab == null)
            {
                GameLogger.Error(LogCategory, "No enemy prefab assigned; cannot spawn the encounter. Returning to the map.");
                _active = true;
                Resolve(EncounterOutcome.Fled);
                return;
            }

            // Timed event rooms (Terreiro / Torre): EventRoomDirector spawns (SpawnDirected), scores and resolves
            // (ResolveDirected). The arena has dressed the floor and placed the hero; flee/death still resolve here.
            if (payload.IsEventRoom)
            {
                IsDirected = true;
                _region = payload.biome;
                _active = true;
                if (_introGraceSeconds > 0f)
                {
                    EventBus.Publish(new EncounterIntroEvent(payload, _introGraceSeconds, false));
                }

                return;
            }

            List<EnemyDefinition> enemies = payload.ResolveEnemies();
            FilterDefeatedUniqueEnemies(enemies);

            // Day/night: reinforcement waves and night variants (NightDangerRules), decided once from the payload.
            bool groupHasBoss = enemies.Exists(enemy => enemy != null && enemy.IsBoss);
            bool eliteAmbush = IsEliteAmbush(payload);
            int victories = TotalVictories();
            TimeOfDayPhase phase = CurrentPhase();
            _region = payload.biome;
            _waveRandom = new System.Random(payload.seed);
            _allowReinforcements = _reinforcementWaves && ReinforcementWavesEnabled && NightDangerRules.AllowsReinforcements(payload.isBossChallenge, groupHasBoss, eliteAmbush, victories);
            bool allowVariants = NightDangerRules.AllowsNightVariants(payload.isBossChallenge, groupHasBoss, victories);
            _firstWaveRegularCount = enemies.FindAll(enemy => enemy != null && !enemy.IsBoss).Count;
            _wavesSpawned = 1;
            _waveIncoming = false;

            _spawnGroundPoints.Clear();
            for (int i = 0; i < enemies.Count; i++)
            {
                GameObject spawned = SpawnEnemy(enemies[i], i, enemies.Count, _introGraceSeconds);
                bool isElite = eliteAmbush && i == 0;
                if (spawned != null && allowVariants && !isElite && NightDangerRules.RollVariant(phase, _waveRandom))
                {
                    NightVariantTag.Attach(spawned, NightVariantRules.Resolve(NightForm.Variant));
                }
            }

            _active = true;
            _tracker.StartWave(0, _spawned.Count);

            if (_spawned.Count == 0)
            {
                GameLogger.Warning(LogCategory, $"Encounter in {payload.biome} resolved to zero enemies; returning to the map.");
                Resolve(EncounterOutcome.Fled);
                return;
            }

            // Boss intro banner (BossIntroBannerController). Music already switched to the Boss
            // context on EncounterStartedEvent (MusicDirector checks the payload for IsBoss).
            EnemyDefinition boss = enemies.Find(enemy => enemy != null && enemy.IsBoss);
            if (boss != null)
            {
                EventBus.Publish(new BossEncounterIntroEvent(payload, boss));
            }

            // "Prepare-se!" — the enemies were told to hold their aggro for the same grace in SpawnEnemy.
            if (_introGraceSeconds > 0f)
            {
                EventBus.Publish(new EncounterIntroEvent(payload, _introGraceSeconds, boss != null));
            }
        }

        private BiomeArenaDressing ResolveBiomeDressing()
        {
            if (_biomeDressing == null && !TryGetComponent(out _biomeDressing))
            {
                _biomeDressing = FindAnyObjectByType<BiomeArenaDressing>();
                if (_biomeDressing == null)
                {
                    _biomeDressing = gameObject.AddComponent<BiomeArenaDressing>();
                }
            }

            return _biomeDressing;
        }

        private GameObject SpawnEnemy(EnemyDefinition definition, int index, int count, float aggroHoldSeconds)
        {
            Vector3 position = GetSpawnPosition(index, count);
            Quaternion rotation = Quaternion.identity;
            if (_player != null)
            {
                Vector3 toPlayer = _player.position - position;
                toPlayer.y = 0f;
                if (toPlayer.sqrMagnitude > 0.0001f)
                {
                    rotation = Quaternion.LookRotation(toPlayer, Vector3.up);
                }
            }

            GameObject instance = Instantiate(_enemyPrefab, position, rotation);
            instance.name = $"{_enemyPrefab.name}_{definition.name}_{index}";

            EnemyController controller = instance.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.ResetForSpawn(definition, position, _enemyWaypoints);
                if (_player != null)
                {
                    controller.Target = _player;
                }

                // Force aggro once "Ja!" ends the grace: short-detection enemies (spiders/scorpions,
                // 7-8 m) spawned 8-9 m away never noticed the hero otherwise.
                controller.HoldAggro(aggroHoldSeconds, forceAggroWhenHoldEnds: true);
            }
            else
            {
                GameLogger.Warning(LogCategory, $"Enemy prefab '{_enemyPrefab.name}' has no EnemyController; '{definition.name}' will not fight back.");
            }

            // The shared prefab only provides behaviour; the look comes from the definition
            // (its model, else a per-enemy placeholder, else the prefab's own visual).
            EnemyVisualSwapper.Apply(instance, definition);

            _spawned.Add(instance);
            EventBus.Publish(new EnemySpawnedEvent(instance));
            return instance;
        }

        /// <summary>
        /// Drops unique enemies already in <see cref="DefeatedEnemyRegistry"/> (the map's rolls are
        /// filtered too, but a payload can be stale or come from JSON). No registry = no filtering.
        /// </summary>
        private static void FilterDefeatedUniqueEnemies(List<EnemyDefinition> enemies)
        {
            if (!ServiceLocator.TryGet(out DefeatedEnemyRegistry defeated))
            {
                return;
            }

            int removed = enemies.RemoveAll(defeated.ShouldSkipSpawn);
            if (removed > 0)
            {
                GameLogger.Info(LogCategory, $"Skipped {removed} already-defeated boss(es) from the encounter.");
            }
        }

        /// <summary>
        /// Root position for enemy <paramref name="index"/>: a ground point (ring around the player,
        /// raycast down; or a fixed spawn point, authored at floor level) lifted by the prefab's
        /// pivot-to-feet distance so the capsule — and the visual resting on its bottom — stands on it.
        /// </summary>
        private Vector3 GetSpawnPosition(int index, int count)
        {
            Vector3 ground;
            if (_spawnOnRingAroundPlayer && _player != null)
            {
                Vector3 ring = EncounterSpawnLayout.RingPoint(_player.position, _player.forward, index, count,
                    _spawnMinRadius, _spawnMaxRadius, _spawnArcDegrees);
                ring = EncounterSpawnLayout.ClampToArena(ring, transform.position, _spawnArenaRadius);
                ring = EncounterSpawnLayout.KeepApart(ring, _spawnGroundPoints, _spawnMinSpacing,
                    _player.position, transform.position, _spawnArenaRadius);
                ground = SnapToGround(ring, _player.position.y);
            }
            else
            {
                ground = GetFixedSpawnPosition(index);
            }

            _spawnGroundPoints.Add(ground);

            float footHeight = EnemyVisualSwapper.GetFootHeight(_enemyPrefab.transform) * _enemyPrefab.transform.localScale.y;
            return EncounterSpawnLayout.StandOnGround(ground, footHeight);
        }

        /// <summary>
        /// Drops a ring point onto the arena floor. Hits far above the player's feet are prop tops
        /// (trees, rocks) and are ignored, as are the player and other enemies (an earlier spawn's
        /// capsule is not floor — landing on it stacked enemies); no hit keeps the player's height.
        /// </summary>
        private Vector3 SnapToGround(Vector3 point, float referenceHeight)
        {
            const float castHeight = 20f;
            Vector3 from = new Vector3(point.x, referenceHeight + castHeight, point.z);
            RaycastHit[] hits = Physics.RaycastAll(from, Vector3.down, castHeight * 2f, _groundMask, QueryTriggerInteraction.Ignore);

            float bestY = float.NegativeInfinity;
            foreach (RaycastHit hit in hits)
            {
                if (!EncounterSpawnLayout.IsGroundCandidate(hit.collider, _player))
                {
                    continue;
                }

                float y = hit.point.y;
                if (y <= referenceHeight + MaxGroundStepUp && y > bestY)
                {
                    bestY = y;
                }
            }

            point.y = float.IsNegativeInfinity(bestY) ? referenceHeight : bestY;
            return point;
        }

        private Vector3 GetFixedSpawnPosition(int index)
        {
            if (_enemySpawnPoints == null || _enemySpawnPoints.Length == 0)
            {
                return transform.position;
            }

            Transform point = _enemySpawnPoints[index % _enemySpawnPoints.Length];
            return point != null ? point.position : transform.position;
        }

        private static void MovePlayer(Transform player, SpawnPoint spawn)
        {
            // CharacterController overrides transform writes while enabled, so toggle it around the teleport.
            CharacterController characterController = player.GetComponent<CharacterController>();
            bool wasEnabled = characterController != null && characterController.enabled;
            if (wasEnabled)
            {
                characterController.enabled = false;
            }

            player.SetPositionAndRotation(spawn.Position, spawn.Rotation);

            if (wasEnabled)
            {
                characterController.enabled = true;
            }
        }

        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            if (!IsEncounterActive || IsDirected || !_spawned.Contains(evt.Enemy))
            {
                return;
            }

            if (_tracker.RegisterDeath())
            {
                if (TryScheduleNextWave())
                {
                    return;
                }

                Resolve(EncounterOutcome.Victory);
            }
        }

        // ---------------------------------------------------------------
        // Reinforcement waves (day/night)
        // ---------------------------------------------------------------

        /// <summary>Queues the next wave if the phase right now asks for one (NightDangerRules). False = the encounter is won.</summary>
        private bool TryScheduleNextWave()
        {
            if (!_allowReinforcements || !isActiveAndEnabled)
            {
                return false;
            }

            TimeOfDayPhase phase = CurrentPhase();
            if (!NightDangerRules.ShouldSpawnAnotherWave(_wavesSpawned, phase))
            {
                return false;
            }

            _waveIncoming = true;
            StartCoroutine(SpawnNextWaveAfter(phase));
            return true;
        }

        private IEnumerator SpawnNextWaveAfter(TimeOfDayPhase phase)
        {
            float interval = NightDangerRules.WaveInterval(phase);
            if (interval > 0f)
            {
                // Scaled time: the wait stops with the game's pause like the fight itself.
                yield return new WaitForSeconds(interval);
            }

            _waveIncoming = false;
            if (!IsEncounterActive)
            {
                yield break;
            }

            int size = NightDangerRules.WaveSize(_firstWaveRegularCount, phase);
            List<PeriodSpawnPick> picks = PeriodSpawnTable.RollWave(_region, phase, size, _waveRandom);
            if (picks.Count == 0)
            {
                Resolve(EncounterOutcome.Victory);
                yield break;
            }

            float hold = NightDangerRules.ReinforcementAggroHold(_introGraceSeconds, phase);
            _spawnGroundPoints.Clear();
            int spawnedCount = 0;
            for (int i = 0; i < picks.Count; i++)
            {
                GameObject spawned = SpawnEnemy(picks[i].Enemy, i, picks.Count, hold);
                if (spawned == null)
                {
                    continue;
                }

                spawnedCount++;
                NightVariantTag.Attach(spawned, picks[i].Look);
            }

            if (spawnedCount == 0)
            {
                Resolve(EncounterOutcome.Victory);
                yield break;
            }

            int waveIndex = _wavesSpawned++;
            _tracker.StartWave(waveIndex, spawnedCount);
            EventBus.Publish(new WaveStartedEvent(waveIndex, spawnedCount));

            if (_player != null)
            {
                ServiceLocator.TryGet(out LocalizationManager localization);
                string key = NightDangerRules.IsNight(phase) ? NightTexts.WaveIncomingNight : NightTexts.WaveIncoming;
                CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * 2.4f, NightTexts.Get(localization, key));
            }

            GameLogger.Info(LogCategory, $"Reinforcement wave {waveIndex + 1} ({phase}) in {_region}: {spawnedCount} enemies.");
        }

        private static TimeOfDayPhase CurrentPhase()
        {
            return ServiceLocator.TryGet(out TimeOfDayManager time) && time != null ? time.Phase : TimeOfDayPhase.Day;
        }

        /// <summary>Total victories on the map (the first fight is the tutorial). No progress service = 0 (one wave, no variants).</summary>
        private static int TotalVictories()
        {
            if (!ServiceLocator.TryGet(out WorldMapProgress progress) || progress == null)
            {
                return 0;
            }

            int total = 0;
            foreach (BiomeType region in System.Enum.GetValues(typeof(BiomeType)))
            {
                total += progress.GetVictories(region);
            }

            return total;
        }

        private static bool IsEliteAmbush(EncounterPayload payload)
        {
            Rapadura.Gameplay.World.Events.WorldEventState state = Rapadura.Gameplay.World.Events.WorldEventState.ResolveShared();
            return payload != null && state != null && state.PendingEliteSeed.HasValue && state.PendingEliteSeed.Value == payload.seed;
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            if (!IsEncounterActive)
            {
                return;
            }

            if (_transition != null && _transition.DeferDefeatResolution)
            {
                // The death screen owns this moment (its "Continuar" completes the encounter as
                // Defeat). Just stop the flee check; no timed return racing the paused screen.
                _resolved = true;
                GameLogger.Info(LogCategory, "Player died; defeat handed to the death screen.");
                return;
            }

            Resolve(EncounterOutcome.Defeat);
        }

        private void Resolve(EncounterOutcome outcome)
        {
            if (_resolved)
            {
                return;
            }

            _resolved = true;
            GameLogger.Info(LogCategory, $"Encounter resolved: {outcome}.");

            // Decided now, completed after the delay: a death during that delay (death screen →
            // CompleteEncounter(Defeat)) must not undo a victory.
            _transition?.DecideOutcome(outcome);

            if (_returnDelaySeconds > 0f && isActiveAndEnabled)
            {
                StartCoroutine(CompleteAfterDelay(outcome));
            }
            else
            {
                _transition?.CompleteEncounter(outcome);
            }
        }

        private IEnumerator CompleteAfterDelay(EncounterOutcome outcome)
        {
            yield return new WaitForSeconds(_returnDelaySeconds);
            _transition?.CompleteEncounter(outcome);
        }
    }
}
