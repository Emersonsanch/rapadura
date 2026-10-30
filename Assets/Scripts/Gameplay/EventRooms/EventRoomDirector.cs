using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.EventRooms
{
    /// <summary>Everything the results card shows (filled by <see cref="EventRoomDirector"/> when a run ends).</summary>
    public sealed class EventRoomResult
    {
        public EventRoomKind Kind;
        public bool Cleared;
        public bool Died;
        public int Score;
        public int Kills;
        public int BestCombo;
        public EventRoomTier Tier;
        public EventRoomReward Reward;
        public int BestScore;
        public bool NewBest;
        public bool BagOverflow;
        public int BlessingStonesGranted;
        public int SoulStonesGranted;
        public string CousinLine;
    }

    /// <summary>
    /// Runs a timed event room inside Main.unity. Self-bootstrapping like <c>NightArenaDirector</c> (no scene edit):
    /// after a scene load whose pending encounter is an event room (<see cref="EncounterPayload.IsEventRoom"/>) a host
    /// object is created in that scene and dies with it. The <see cref="EncounterArenaController"/> sees the same flag,
    /// dresses the floor, places the hero and steps aside (directed mode); this director then
    /// <list type="bullet">
    /// <item>spawns waves through <see cref="EncounterArenaController.SpawnDirected"/> from the payload's pool (every
    /// region's regular enemies) scaled to the hero's level bracket (<see cref="EventRoomEnemyScaler"/>);</item>
    /// <item>keeps the timer (scaled time: pauses with the game), the <see cref="EventRoomScore"/> and, in the Torre,
    /// the phases (<see cref="EventRoomRules.TorrePhaseFor"/>) with its props (<see cref="TorreProps"/>);</item>
    /// <item>ends on the timer, on delivery (Torre) or on death: tier → rewards (XP, gold, refining stones when they
    /// exist, through the bag's shared fit rules) → <see cref="EventRoomProgress.RecordRun"/> → results card, then
    /// <see cref="EncounterArenaController.ResolveDirected"/> (on death the arena / death screen resolve as usual).</item>
    /// </list>
    /// The HUD (<c>EventRoomHud</c>) polls <see cref="Current"/>.
    /// </summary>
    public class EventRoomDirector : MonoBehaviour
    {
        public const string HostName = "EventRoomDirector";
        private const string PlayerTag = "Player";
        private const string GoldItemId = "item_gold";

        /// <summary>Real seconds the results card stays before returning to the map.</summary>
        public const float ResultsSeconds = 5f;

        /// <summary>Real seconds to wait for the arena to enter directed mode before giving up.</summary>
        private const float ArenaWaitSeconds = 6f;

        private static readonly List<GameObject> Buffer = new List<GameObject>();

        private readonly HashSet<GameObject> _alive = new HashSet<GameObject>();
        private readonly EventRoomScore _score = new EventRoomScore();

        private EncounterPayload _payload;
        private EncounterArenaController _arena;
        private System.Random _random;
        private List<EnemyDefinition> _pool = new List<EnemyDefinition>();
        private EventRoomBracket _bracket;
        private Transform _player;

        private float _waitTimer;
        private float _graceLeft;
        private bool _started;
        private bool _setupDone;
        private bool _finished;
        private float _elapsed;
        private float _lastWaveAt;
        private int _wavesSpawned;
        private float _resultsLeft = -1f;
        private EncounterOutcome _pendingOutcome;

        // Torre
        private int _bridgeKills;
        private bool _gateBroken;
        private bool _guardianDefeated;
        private bool _moendaPicked;
        private bool _moendaDelivered;
        private GameObject _guardian;
        private GameObject _keeper;
        private GameObject _barrier;
        private EventRoomGate _gate;
        private GameObject _moenda;
        private GameObject _carried;
        private Vector3 _towerDirection = Vector3.forward;

        private string _banner;
        private float _bannerLeft;

        public static EventRoomDirector Current { get; private set; }

        public EventRoomKind Kind => _payload != null ? _payload.eventRoom : EventRoomKind.None;
        public bool IsRunning => _started && !_finished;
        public bool HasStarted => _started;
        public bool IsFinished => _finished;
        public float TimeLeft => Mathf.Max(0f, EventRoomRules.DurationSeconds(Kind) - _elapsed);
        public EventRoomScore Score => _score;
        public float Elapsed => _elapsed;
        public EventRoomBracket Bracket => _bracket;
        public TorrePhase Phase => EventRoomRules.TorrePhaseFor(new TorreProgress(_bridgeKills, _gateBroken, _guardianDefeated, _moendaPicked, _moendaDelivered));
        public int BridgeKills => _bridgeKills;
        public EventRoomGate Gate => _gate;
        public EventRoomResult Result { get; private set; }
        public string Banner => _bannerLeft > 0f ? _banner : null;
        public int AliveEnemies => _alive.Count;

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
                || !transition.PendingEncounter.IsEventRoom || FindFirstObjectByType<EventRoomDirector>() != null)
            {
                return;
            }

            var host = new GameObject(HostName);
            SceneManager.MoveGameObjectToScene(host, scene);
            host.AddComponent<EventRoomDirector>().Begin(transition.PendingEncounter);
        }

        public void Begin(EncounterPayload payload)
        {
            _payload = payload;
            _random = new System.Random(payload != null ? payload.seed : 0);
            _pool = payload != null ? payload.ResolveEnemies() : new List<EnemyDefinition>();
            _bracket = EventRoomRules.BracketFor(payload != null ? payload.eventRoomLevel : 1);
            Current = this;
            if (ServiceLocator.TryGet(out LocalizationManager localization))
            {
                EventRoomTexts.EnsureRegistered(localization);
            }
        }

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

        private void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }

            if (_gate != null)
            {
                _gate.Broken -= OnGateBroken;
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
                    _arena.ResolveDirected(_pendingOutcome);
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

            // The arena resolved on its own (the hero walked out of the flee radius): the run ends with what it has.
            if (_arena == null || !_arena.IsEncounterActive)
            {
                Finish(died: false, resolveArena: false);
                return;
            }

            _elapsed += Time.deltaTime;
            if (Kind == EventRoomKind.Torre)
            {
                TickTorre();
            }
            else
            {
                TickTerreiro();
            }

            if (!_finished && TimeLeft <= 0f)
            {
                Finish(died: false, resolveArena: true);
            }
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
                    GameLogger.Warning(HostName, "No arena in directed mode; leaving the event room.");
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
                SetupRoom();
                ShowBanner(CousinLine(EventRoomMoment.Start), 4.5f);
                return;
            }

            _graceLeft -= Time.deltaTime;
            if (_graceLeft > 0f)
            {
                return;
            }

            _started = true;
            _elapsed = 0f;
            _lastWaveAt = -999f;
        }

        private void SetupRoom()
        {
            if (Kind != EventRoomKind.Torre || _player == null)
            {
                return;
            }

            Vector3 origin = _player.position;
            Vector3 toCenter = _arena.ArenaCenter - origin;
            toCenter.y = 0f;
            _towerDirection = toCenter.sqrMagnitude > 4f ? toCenter.normalized : Flat(_player.forward);
            Vector3 right = Vector3.Cross(Vector3.up, _towerDirection);

            _keeper = TorreProps.BuildKeeper(Ground(origin - right * 2.2f - _towerDirection * 0.5f), origin + _towerDirection * 3f);
            _barrier = TorreProps.BuildBarrier(Ground(PointAlong(origin, TorreProps.BridgeDistance)), origin);
            _gate = TorreProps.BuildGate(Ground(PointAlong(origin, TorreProps.GateDistance)), origin, EventRoomRules.GateHealth(_bracket));
            _gate.Broken += OnGateBroken;
            foreach (GameObject prop in new[] { _keeper, _barrier, _gate.gameObject })
            {
                SceneManager.MoveGameObjectToScene(prop, gameObject.scene);
            }
        }

        private Vector3 PointAlong(Vector3 origin, float distance)
        {
            Vector3 point = origin + _towerDirection * distance;
            Vector3 center = _arena.ArenaCenter;
            float radius = _arena.SpawnArenaRadius > 0f ? _arena.SpawnArenaRadius - 1f : 0f;
            Vector3 offset = point - center;
            offset.y = 0f;
            if (radius > 0f && offset.magnitude > radius)
            {
                point = center + offset.normalized * radius;
                point.y = origin.y;
            }

            return point;
        }

        private Vector3 Ground(Vector3 point)
        {
            Vector3 from = point + Vector3.up * 20f;
            float best = float.NegativeInfinity;
            foreach (RaycastHit hit in Physics.RaycastAll(from, Vector3.down, 40f, _arena.GroundMask, QueryTriggerInteraction.Ignore))
            {
                if (_player != null && hit.collider.transform.IsChildOf(_player))
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

        private static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude > 0.0001f ? v.normalized : Vector3.forward;
        }

        // ------------------------------------------------------------------
        // Terreiro
        // ------------------------------------------------------------------

        private void TickTerreiro()
        {
            PruneDead();
            if (EventRoomRules.ShouldSpawnWave(_alive.Count, _elapsed - _lastWaveAt, EventRoomRules.TerreiroWaveIntervalSeconds, EventRoomRules.TerreiroMaxAlive))
            {
                int size = EventRoomRules.FitWave(EventRoomRules.TerreiroWaveSize(_wavesSpawned), _alive.Count, EventRoomRules.TerreiroMaxAlive);
                SpawnWave(size);
            }
        }

        // ------------------------------------------------------------------
        // Torre
        // ------------------------------------------------------------------

        private void TickTorre()
        {
            PruneDead();
            switch (Phase)
            {
                case TorrePhase.OpenBridge:
                    if (EventRoomRules.ShouldSpawnWave(_alive.Count, _elapsed - _lastWaveAt, EventRoomRules.TorreWaveIntervalSeconds, EventRoomRules.TorreMaxAlive))
                    {
                        SpawnWave(EventRoomRules.FitWave(EventRoomRules.TorreWaveSize(_wavesSpawned), _alive.Count, EventRoomRules.TorreMaxAlive));
                    }

                    break;

                case TorrePhase.BreakGate:
                case TorrePhase.Guardian:
                    if (_elapsed - _lastWaveAt >= EventRoomRules.TorreTrickleIntervalSeconds)
                    {
                        SpawnWave(EventRoomRules.FitWave(EventRoomRules.TorreTrickleSize, _alive.Count, EventRoomRules.TorreTrickleMaxAlive));
                    }

                    break;

                case TorrePhase.PickUpMoenda:
                    if (_player != null && _moenda != null && FlatDistance(_player.position, _moenda.transform.position) <= EventRoomRules.PickUpRadius)
                    {
                        _moendaPicked = true;
                        Destroy(_moenda);
                        _moenda = null;
                        _carried = TorreProps.BuildCarried(_player);
                        ShowBanner(Text(EventRoomTexts.HudMoendaTaken), 3f);
                        SpawnWave(EventRoomRules.FitWave(EventRoomRules.TorreAmbushSize, _alive.Count, EventRoomRules.TorreMaxAlive));
                    }

                    break;

                case TorrePhase.Deliver:
                    if (_player != null && _keeper != null && FlatDistance(_player.position, _keeper.transform.position) <= EventRoomRules.DeliverRadius)
                    {
                        _moendaDelivered = true;
                        if (_carried != null)
                        {
                            Destroy(_carried);
                        }

                        _score.AddBonus(EventRoomRules.TorreDeliveryBonus + EventRoomRules.TorreTimeBonus(TimeLeft));
                        Finish(died: false, resolveArena: true);
                    }

                    break;
            }

            if (_gate != null)
            {
                _gate.AcceptsDamage = EventRoomRules.GateAcceptsDamage(Phase);
            }
        }

        private void OnGateBroken()
        {
            if (_gateBroken || _finished)
            {
                return;
            }

            _gateBroken = true;
            _score.AddBonus(EventRoomRules.TorreGateBonus);
            ShowBanner(Text(EventRoomTexts.HudGateBroken) + " " + Text(EventRoomTexts.HudGuardianAppears), 3f);
            SpawnGuardian();
        }

        private void SpawnGuardian()
        {
            EnemyDefinition definition = _pool.Find(enemy => enemy != null && enemy.name == EventRoomRules.GuardianEnemyId);
            if (definition == null && _pool.Count > 0)
            {
                definition = _pool[_pool.Count - 1];
            }

            GameObject guardian = definition != null && _arena != null ? _arena.SpawnDirected(definition, 0, 1, 1f) : null;
            if (guardian == null)
            {
                // Nothing to fight: skip straight to the Moenda, in front of the old gate.
                _guardianDefeated = true;
                DropMoenda(_gate != null ? _gate.transform.position : (_player != null ? _player.position : transform.position));
                return;
            }

            EventRoomEnemyScaler.Attach(guardian, EventRoomRules.GuardianScale(EventRoomEntry.NativeTier(definition, _bracket), _bracket));
            _guardian = guardian;
            _alive.Add(guardian);
            ServiceLocator.TryGet(out LocalizationManager localization);
            CombatFxManager.SpawnLootWarningText(guardian.transform.position + Vector3.up * 3f, EventRoomTexts.Get(localization, EventRoomTexts.GuardianName));
        }

        private void DropMoenda(Vector3 position)
        {
            _moenda = TorreProps.BuildMoenda(Ground(position));
            SceneManager.MoveGameObjectToScene(_moenda, gameObject.scene);
        }

        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }

        // ------------------------------------------------------------------
        // Spawning and kills
        // ------------------------------------------------------------------

        private void SpawnWave(int size)
        {
            _lastWaveAt = _elapsed;
            if (size <= 0 || _pool.Count == 0 || _arena == null)
            {
                return;
            }

            int spawned = 0;
            for (int i = 0; i < size; i++)
            {
                EnemyDefinition definition = _pool[_random.Next(_pool.Count)];
                GameObject enemy = _arena.SpawnDirected(definition, i, size, 0.6f);
                if (enemy == null)
                {
                    continue;
                }

                EventRoomEnemyScaler.Attach(enemy, EventRoomRules.EnemyScale(EventRoomEntry.NativeTier(definition, _bracket), _bracket));
                _alive.Add(enemy);
                spawned++;
            }

            if (spawned > 0)
            {
                EventBus.Publish(new WaveStartedEvent(_wavesSpawned, spawned));
                _wavesSpawned++;
            }
        }

        private void PruneDead()
        {
            Buffer.Clear();
            foreach (GameObject enemy in _alive)
            {
                if (enemy == null)
                {
                    Buffer.Add(enemy);
                }
            }

            foreach (GameObject gone in Buffer)
            {
                _alive.Remove(gone);
            }
        }

        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            if (evt.Enemy == null || !_alive.Remove(evt.Enemy) || !IsRunning)
            {
                return;
            }

            bool isGuardian = evt.Enemy == _guardian;
            int points = _score.RegisterKill(_elapsed, isGuardian ? EventRoomRules.GuardianKillPoints : EventRoomRules.KillPoints);
            CombatFxManager.SpawnLootText(evt.Enemy.transform.position + Vector3.up * 2.6f, "+" + points);

            if (Kind != EventRoomKind.Torre)
            {
                return;
            }

            if (isGuardian)
            {
                _guardianDefeated = true;
                DropMoenda(evt.Enemy.transform.position);
                ShowBanner(Text(EventRoomTexts.HudGoalPickUp), 3f);
                return;
            }

            if (Phase == TorrePhase.OpenBridge)
            {
                _bridgeKills++;
                if (_bridgeKills >= EventRoomRules.TorreKillsToOpenBridge)
                {
                    _score.AddBonus(EventRoomRules.TorreBridgeBonus);
                    TorreProps.SinkBarrier(_barrier);
                    ShowBanner(Text(EventRoomTexts.HudBridgeOpen) + " " + Text(EventRoomTexts.HudGoalGate), 3f);
                }
            }
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            if (IsRunning)
            {
                // The arena / death screen own the Defeat; the run still pays what its score earned.
                Finish(died: true, resolveArena: false);
            }
        }

        // ------------------------------------------------------------------
        // End
        // ------------------------------------------------------------------

        private void Finish(bool died, bool resolveArena)
        {
            if (_finished)
            {
                return;
            }

            _finished = true;
            EventRoomKind kind = Kind;
            bool survivedToTimer = !died && kind == EventRoomKind.Terreiro && TimeLeft <= 0f;
            bool cleared = EventRoomRules.IsCleared(kind, survivedToTimer, _moendaDelivered);
            if (kind == EventRoomKind.Terreiro && survivedToTimer)
            {
                _score.AddBonus(EventRoomRules.TerreiroSurvivalBonus);
            }

            EventRoomTier tier = EventRoomRules.TierFor(_score.Total, kind, cleared);
            EventRoomReward reward = EventRoomRules.RewardFor(tier, _bracket);
            var result = new EventRoomResult
            {
                Kind = kind,
                Cleared = cleared,
                Died = died,
                Score = _score.Total,
                Kills = _score.Kills,
                BestCombo = _score.BestCombo,
                Tier = tier,
                Reward = reward,
            };

            GrantRewards(result);
            EventRoomRunOutcome outcome = EventRoomProgress.ResolveShared().RecordRun(kind, _score.Total, tier, cleared);
            result.BestScore = outcome.BestScore;
            result.NewBest = outcome.NewBest;
            result.CousinLine = CousinLine(cleared ? EventRoomMoment.Win : EventRoomMoment.Lose);
            Result = result;

            // Calm the field while the card is up.
            foreach (GameObject enemy in _alive)
            {
                if (enemy != null && enemy.TryGetComponent(out EnemyController controller))
                {
                    controller.HoldAggro(60f);
                }
            }

            if (_gate != null)
            {
                _gate.AcceptsDamage = false;
            }

            GameLogger.Info(HostName, $"{kind} finished: score {_score.Total}, tier {tier}, cleared {cleared}, died {died}.");
            if (resolveArena)
            {
                _pendingOutcome = cleared ? EncounterOutcome.Victory : EncounterOutcome.Fled;
                _resultsLeft = ResultsSeconds;
            }
        }

        private void GrantRewards(EventRoomResult result)
        {
            EventRoomReward reward = result.Reward;
            if (reward.IsEmpty)
            {
                return;
            }

            GameObject player = _player != null ? _player.gameObject : GameObject.FindGameObjectWithTag(PlayerTag);
            if (player == null)
            {
                return;
            }

            if (reward.Experience > 0 && player.TryGetComponent(out PlayerStats stats))
            {
                stats.AddExperience(reward.Experience);
            }

            if (!player.TryGetComponent(out InventoryManager inventory))
            {
                return;
            }

            result.BagOverflow |= Grant(inventory, GoldItemId, reward.Gold, out _);
            result.BagOverflow |= Grant(inventory, EventRoomRules.BlessingStoneItemId, reward.BlessingStones, out result.BlessingStonesGranted);
            result.BagOverflow |= Grant(inventory, EventRoomRules.SoulStoneItemId, reward.SoulStones, out result.SoulStonesGranted);
        }

        /// <summary>Adds through the shared fit rules; true when part of it did not fit. Missing items (refining not in yet) are skipped.</summary>
        private static bool Grant(InventoryManager inventory, string itemId, int amount, out int granted)
        {
            granted = 0;
            if (amount <= 0 || !ItemDatabase.TryGetById(itemId, out ItemDefinition item) || item == null)
            {
                return false;
            }

            LootGrantResult result = LootGrantRules.Grant(inventory, new LootDrop(item, amount), null);
            granted = result.Granted;
            return result.Lost > 0;
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

        private static string Text(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            return EventRoomTexts.Get(localization, key);
        }

        private string CousinLine(EventRoomMoment moment)
        {
            CharacterId hero = ServiceLocator.TryGet(out CharacterSelectionState selection) && selection.Selected.HasValue
                ? selection.Selected.Value
                : CharacterId.Maithe;
            return Text(EventRoomTexts.LineKey(Kind, moment, hero));
        }

        /// <summary>The current objective line for the HUD.</summary>
        public string GoalText(LocalizationManager localization)
        {
            if (Kind != EventRoomKind.Torre)
            {
                return EventRoomTexts.Get(localization, EventRoomTexts.HudGoalTerreiro);
            }

            switch (Phase)
            {
                case TorrePhase.OpenBridge:
                    return EventRoomTexts.Format(localization, EventRoomTexts.HudGoalBridge, _bridgeKills, EventRoomRules.TorreKillsToOpenBridge);
                case TorrePhase.BreakGate:
                    return EventRoomTexts.Get(localization, EventRoomTexts.HudGoalGate);
                case TorrePhase.Guardian:
                    return EventRoomTexts.Get(localization, EventRoomTexts.HudGoalGuardian);
                case TorrePhase.PickUpMoenda:
                    return EventRoomTexts.Get(localization, EventRoomTexts.HudGoalPickUp);
                default:
                    return EventRoomTexts.Get(localization, EventRoomTexts.HudGoalDeliver);
            }
        }
    }
}
