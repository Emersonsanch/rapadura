using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Cities;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Areas
{
    /// <summary>
    /// The open-map spawner of one <see cref="MapSpawnZone"/> (pipeline step 4–5): keeps up to <see cref="MapSpawnZone.MaxAlive"/>
    /// monsters in the clearing, rolled from the zone's table for the current period (day, dawn, dusk, night: night
    /// variants and night-only species through <see cref="NightVariantTag"/>), each at a level of the zone's band
    /// (<see cref="MapMonsterInfo"/>), respawning <see cref="MapSpawnZone.RespawnSeconds"/> after a death. Reuses the
    /// arena's enemy prefab and the same spawn steps as <see cref="EncounterArenaController"/> (ResetForSpawn, visual swap,
    /// <see cref="EnemySpawnedEvent"/>); combat, XP and loot are the usual real-time loop. Patrols a ring of waypoints in
    /// its clearing; never spawns next to the hero, in the river or in a safe zone, and holds the aggro of any monster that
    /// wanders into one.
    /// </summary>
    public sealed class MapMonsterSpawner : MonoBehaviour
    {
        private const string PlayerTag = "Player";
        private const float TickSeconds = 0.5f;
        private const float FillIntervalSeconds = 0.4f;
        private const float MinPlayerDistance = 9f;
        private const float CorpseSeconds = 9f;
        private const int Waypoints = 4;

        /// <summary>Off in tests that build a map without monsters.</summary>
        public static bool SpawningEnabled { get; set; } = true;

        private readonly List<GameObject> _alive = new List<GameObject>();
        private readonly List<(GameObject Enemy, float At)> _corpses = new List<(GameObject, float)>();
        private Transform[] _waypoints;
        private MapLayout _layout;
        private GameObject _prefab;
        private System.Random _random;
        private float _nextTick;
        private float _nextSpawnAt;
        private Transform _player;

        public MapSpawnZone Zone { get; private set; }
        public int AliveCount => _alive.Count;
        public int SpawnedTotal { get; private set; }

        public void Configure(MapLayout layout, MapSpawnZone zone, GameObject enemyPrefab, int seed)
        {
            _layout = layout;
            Zone = zone;
            _prefab = enemyPrefab;
            _random = new System.Random(seed);
            BuildWaypoints();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
        }

        private void BuildWaypoints()
        {
            _waypoints = new Transform[Waypoints];
            for (int i = 0; i < Waypoints; i++)
            {
                float a = i * Mathf.PI * 2f / Waypoints + 0.4f;
                var wp = new GameObject("Waypoint_" + i).transform;
                wp.SetParent(transform, false);
                wp.localPosition = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * (Zone.Radius * 0.6f);
                _waypoints[i] = wp;
            }
        }

        private void Update()
        {
            if (Time.time < _nextTick || Zone == null)
            {
                return;
            }

            _nextTick = Time.time + TickSeconds;
            ReleaseCorpses();
            _alive.RemoveAll(enemy => enemy == null);

            MapModeController mode = MapModeController.Current;
            if (mode == null || !mode.IsReady)
            {
                return;
            }

            if (_player == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
                _player = player != null ? player.transform : null;
            }

            GuardAlive(mode.HeroLevel);

            if (SpawningEnabled && _prefab != null && _alive.Count < Zone.MaxAlive && Time.time >= _nextSpawnAt)
            {
                if (TrySpawnOne(CurrentPhase()) != null)
                {
                    _nextSpawnAt = Time.time + FillIntervalSeconds;
                }
                else
                {
                    _nextSpawnAt = Time.time + 2f;
                }
            }
        }

        private static TimeOfDayPhase CurrentPhase()
        {
            return ServiceLocator.TryGet(out TimeOfDayManager time) && time != null ? time.Phase : TimeOfDayPhase.Day;
        }

        /// <summary>Spawns one monster from the table of <paramref name="phase"/> (null when nothing could be placed). Public for tests.</summary>
        public GameObject TrySpawnOne(TimeOfDayPhase phase)
        {
            IReadOnlyList<PeriodSpawnEntry> entries = Zone.EntriesFor(phase);
            PeriodSpawnEntry entry = PeriodSpawnTable.RollEntry(entries, _random, AllowedNow);
            if (entry == null)
            {
                return null;
            }

            EnemyDefinition definition = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{entry.EnemyId}");
            if (definition == null || definition.IsBoss)
            {
                return null;
            }

            if (!TryPickPoint(out Vector2 ground))
            {
                return null;
            }

            float footHeight = EnemyVisualSwapper.GetFootHeight(_prefab.transform) * _prefab.transform.localScale.y;
            Vector3 position = EncounterSpawnLayout.StandOnGround(new Vector3(ground.x, 0f, ground.y), footHeight);
            Quaternion rotation = Quaternion.Euler(0f, (float)_random.NextDouble() * 360f, 0f);
            GameObject instance = Instantiate(_prefab, position, rotation);
            instance.name = $"{_prefab.name}_{definition.name}_{Zone.Id}_{SpawnedTotal}";

            EnemyController controller = instance.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.ResetForSpawn(definition, position, _waypoints);
            }

            EnemyVisualSwapper.Apply(instance, definition);

            NightSpecies species = entry.Form == NightForm.Species ? NightVariantRules.FindSpecies(entry.SpeciesId) : null;
            var pick = new PeriodSpawnPick(definition, entry.Form, species, _layout != null ? _layout.Biome : (Rapadura.Gameplay.World.Biomes.BiomeType?)null);
            if (pick.Form != NightForm.Normal)
            {
                NightVariantTag.Attach(instance, pick.Look);
            }

            int level = MapAccessRules.RollMonsterLevel(Zone.MinLevel, Zone.MaxLevel, _random.NextDouble());
            // The level-500 curve: above the archetype level the creature's stats scale (identity for today's bands 1–5).
            EnemyLevelScaler.Attach(instance, level);
            instance.AddComponent<MapMonsterInfo>().Configure(level, _layout != null ? _layout.MapId : null, Zone.Id, species != null ? species.Id : null);

            _alive.Add(instance);
            SpawnedTotal++;
            EventBus.Publish(new EnemySpawnedEvent(instance));
            return instance;
        }

        /// <summary>A species never exceeds its <see cref="NightSpecies.MaxPerWave"/> alive in one clearing.</summary>
        private bool AllowedNow(PeriodSpawnEntry entry)
        {
            if (entry.Form != NightForm.Species)
            {
                return true;
            }

            NightSpecies species = NightVariantRules.FindSpecies(entry.SpeciesId);
            if (species == null)
            {
                return false;
            }

            int count = 0;
            foreach (GameObject enemy in _alive)
            {
                if (enemy != null && enemy.TryGetComponent(out MapMonsterInfo info) && info.SpeciesId == species.Id)
                {
                    count++;
                }
            }

            return count < species.MaxPerWave;
        }

        private bool TryPickPoint(out Vector2 point)
        {
            for (int attempt = 0; attempt < 8; attempt++)
            {
                double angle = _random.NextDouble() * Mathf.PI * 2f;
                float radius = Zone.Radius * 0.85f * Mathf.Sqrt((float)_random.NextDouble());
                point = Zone.Center + new Vector2(Mathf.Cos((float)angle), Mathf.Sin((float)angle)) * radius;
                if (_layout != null && (MapLayoutRules.InRiver(_layout, point, 0.8f) || !_layout.Bounds.Contains(point)))
                {
                    continue;
                }

                var world = new Vector3(point.x, 0.5f, point.y);
                if (CitySafeZones.IsSafeAt(world))
                {
                    continue;
                }

                if (_player != null)
                {
                    Vector3 offset = _player.position - world;
                    offset.y = 0f;
                    if (offset.sqrMagnitude < MinPlayerDistance * MinPlayerDistance)
                    {
                        continue;
                    }
                }

                return true;
            }

            point = Zone.Center;
            return false;
        }

        /// <summary>Monsters in a safe zone stop chasing; the night watchers only watch heroes below their level.</summary>
        private void GuardAlive(int heroLevel)
        {
            bool passive = !string.IsNullOrEmpty(Zone.PassiveSpeciesId) && heroLevel < Zone.PassiveBelowHeroLevel;
            foreach (GameObject enemy in _alive)
            {
                if (enemy == null || !enemy.TryGetComponent(out EnemyController controller))
                {
                    continue;
                }

                bool watcher = passive && enemy.TryGetComponent(out MapMonsterInfo info) && info.SpeciesId == Zone.PassiveSpeciesId;
                if (watcher || CitySafeZones.IsSafeAt(enemy.transform.position))
                {
                    controller.HoldAggro(TickSeconds * 3f);
                }
            }
        }

        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            if (evt.Enemy == null || !_alive.Remove(evt.Enemy))
            {
                return;
            }

            _corpses.Add((evt.Enemy, Time.time + CorpseSeconds));
            _nextSpawnAt = Mathf.Max(_nextSpawnAt, Time.time + Mathf.Max(1f, Zone.RespawnSeconds));
        }

        private void ReleaseCorpses()
        {
            for (int i = _corpses.Count - 1; i >= 0; i--)
            {
                if (_corpses[i].Enemy == null)
                {
                    _corpses.RemoveAt(i);
                    continue;
                }

                if (Time.time >= _corpses[i].At)
                {
                    Destroy(_corpses[i].Enemy);
                    _corpses.RemoveAt(i);
                }
            }
        }
    }
}
