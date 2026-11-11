using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Dungeon;
using Rapadura.Gameplay.Environment;
using Rapadura.UI.Bestiary;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Keeps the world bosses of Regions 10–16 at their arenas. <see cref="MapMonsterSpawner"/> refuses bosses on purpose and every
    /// region test holds that its clearings have none, so they get this small director instead, built on the same spawn steps
    /// (arena prefab, ResetForSpawn, visual swap, level scaler, <see cref="MapMonsterInfo"/>, <see cref="EnemySpawnedEvent"/>): it needs nothing from the world
    /// but <see cref="MapModeController.Current"/>. A boss appears when the hero comes near, leaves (back to full health) when the hero walks far
    /// off, and returns <see cref="RegionBossRules.RespawnSeconds"/> after dying, like the clearings refill.
    /// It lives across scenes (DontDestroyOnLoad) but its timers belong to one game: a new game (<see cref="NewSessionResetEvent"/>) or a save
    /// read from disk (<see cref="GameLoadedEvent"/> not from carried state) puts every boss back at its arena, so nothing leaks between slots.
    /// </summary>
    public sealed class RegionBossDirector : MonoBehaviour
    {
        private const string PlayerTag = "Player";
        private const float TickSeconds = 1f;
        private const float GroundProbeHeight = 6f;

        private sealed class Slot
        {
            public RegionBossSpawn Spawn;
            public GameObject Instance;
            public GameObject Corpse;
            public float CorpseUntil;
            public readonly RegionBossReturns Returns = new RegionBossReturns();
        }

        /// <summary>Off in tests.</summary>
        public static bool SpawningEnabled { get; set; } = true;

        private readonly List<Slot> _slots = new List<Slot>();
        private float _nextTick;
        private Transform _player;
        private GameObject _prefab;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var host = new GameObject(nameof(RegionBossDirector));
            DontDestroyOnLoad(host);
            host.AddComponent<RegionBossDirector>();
        }

        private void Awake()
        {
            foreach (RegionBossSpawn spawn in RegionBossRules.AllSpawns)
            {
                _slots.Add(new Slot { Spawn = spawn });
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Subscribe<NewSessionResetEvent>(OnNewSession);
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Unsubscribe<NewSessionResetEvent>(OnNewSession);
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
        }

        private void OnNewSession(NewSessionResetEvent _) => ResetSlots();

        private void OnGameLoaded(GameLoadedEvent evt)
        {
            // A scene swap hands carried state back to the same game: the timers still hold. Only a save read from disk starts over.
            if (RegionBossReturns.LoadResets(evt.FromCarriedState))
            {
                ResetSlots();
            }
        }

        /// <summary>Every boss back at its arena, no respawn timer, no night to wait for.</summary>
        private void ResetSlots()
        {
            foreach (Slot slot in _slots)
            {
                Clear(slot);
                slot.CorpseUntil = 0f;
                slot.Returns.Reset();
            }

            _player = null;
            _nextTick = 0f;
        }

        private void Update()
        {
            if (Time.time < _nextTick)
            {
                return;
            }

            _nextTick = Time.time + TickSeconds;
            MapModeController mode = MapModeController.Current;
            OverworldView view = mode != null && mode.IsReady ? mode.OverworldArea : null;
            if (view == null || !SpawningEnabled || view.Layout == null)
            {
                foreach (Slot slot in _slots)
                {
                    Clear(slot);
                }

                _prefab = null;
                return;
            }

            if (_player == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
                _player = player != null ? player.transform : null;
                if (_player == null)
                {
                    return;
                }
            }

            Vector2 hero = new Vector2(_player.position.x, _player.position.z);
            foreach (Slot slot in _slots)
            {
                Tick(slot, view, hero);
            }
        }

        private void Tick(Slot slot, OverworldView view, Vector2 hero)
        {
            ReleaseCorpse(slot);
            Vector2? arena = RegionBossRules.ArenaIn(view.Layout, slot.Spawn);
            if (arena == null)
            {
                Clear(slot);
                return;
            }

            float distance = Vector2.Distance(arena.Value, hero);
            if (slot.Instance != null)
            {
                if (RegionBossRules.ShouldSleep(distance))
                {
                    Destroy(slot.Instance);
                    slot.Instance = null;
                }

                return;
            }

            // The night is read off the game clock (its day and hour), so a day spent away from the boss's world still counts.
            bool hasClock = ServiceLocator.TryGet(out TimeOfDayManager time) && time != null;
            if (!slot.Returns.CanReturn(Time.time, hasClock, hasClock ? time.Day : 0, hasClock ? time.CurrentHour : 0f,
                    hasClock && time.Phase == TimeOfDayPhase.Night, TimeOfDayManager.NightStartHour)
                || !RegionBossRules.ShouldWake(distance))
            {
                return;
            }

            SpawnBoss(slot, view, arena.Value);
        }

        private void SpawnBoss(Slot slot, OverworldView view, Vector2 arena)
        {
            if (_prefab == null)
            {
                EncounterArenaController encounter = FindAnyObjectByType<EncounterArenaController>(FindObjectsInactive.Include);
                _prefab = encounter != null ? encounter.EnemyPrefab : null;
                if (_prefab == null)
                {
                    return;
                }
            }

            EnemyDefinition definition = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{slot.Spawn.EnemyId}");
            if (definition == null)
            {
                return;
            }

            float groundY = view.GroundHeight(arena);
            // The chunk under the arena may still be building: wait for its collider rather than drop the boss through the world.
            if (!Physics.Raycast(new Vector3(arena.x, groundY + GroundProbeHeight, arena.y), Vector3.down, GroundProbeHeight * 2f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                return;
            }

            float footHeight = EnemyVisualSwapper.GetFootHeight(_prefab.transform) * _prefab.transform.localScale.y;
            Vector3 position = EncounterSpawnLayout.StandOnGround(new Vector3(arena.x, groundY, arena.y), footHeight);
            GameObject instance = Instantiate(_prefab, position, Quaternion.identity);
            instance.name = $"{_prefab.name}_{definition.name}_world_boss";

            EnemyController controller = instance.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.ResetForSpawn(definition, position);
            }

            EnemyVisualSwapper.Apply(instance, definition);
            EnemyLevelScaler.Attach(instance, slot.Spawn.Level);
            instance.AddComponent<MapMonsterInfo>().Configure(slot.Spawn.Level, slot.Spawn.MapId, slot.Spawn.AreaId, null);

            slot.Instance = instance;
            EventBus.Publish(new EnemySpawnedEvent(instance));
        }

        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            foreach (Slot slot in _slots)
            {
                if (slot.Instance != null && evt.Enemy == slot.Instance)
                {
                    slot.Corpse = slot.Instance;
                    slot.CorpseUntil = Time.time + (slot.Spawn.NeverDies ? RegionBossRules.WithdrawSeconds : RegionBossRules.CorpseSeconds);
                    slot.Instance = null;
                    slot.Returns.Withdraw(Time.time, slot.Spawn.ReturnsNextNight, CurrentNight());
                    if (slot.Spawn.NeverDies)
                    {
                        ServiceLocator.TryGet(out LocalizationManager localization);
                        DungeonFx.Warn(slot.Corpse.transform.position + Vector3.up * 4f, BestiaryTexts.Get(localization, RegionBossTexts.WithdrawKey(slot.Spawn.EnemyId)));
                    }

                    return;
                }
            }
        }

        /// <summary>The night the game clock is in or last went through (by the game's day, not by what the hero saw); 0 without a clock.</summary>
        private static int CurrentNight() =>
            ServiceLocator.TryGet(out TimeOfDayManager time) && time != null ? RegionBossRules.NightIndex(time.Day, time.CurrentHour, TimeOfDayManager.NightStartHour) : 0;

        private static void ReleaseCorpse(Slot slot)
        {
            if (slot.Corpse != null && Time.time >= slot.CorpseUntil)
            {
                Destroy(slot.Corpse);
                slot.Corpse = null;
            }
        }

        private static void Clear(Slot slot)
        {
            if (slot.Instance != null)
            {
                Destroy(slot.Instance);
                slot.Instance = null;
            }

            if (slot.Corpse != null)
            {
                Destroy(slot.Corpse);
                slot.Corpse = null;
            }
        }
    }
}
