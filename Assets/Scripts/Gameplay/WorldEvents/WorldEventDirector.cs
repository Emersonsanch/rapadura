using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.EventRooms;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.WorldEvents
{
    /// <summary>Everything the results card shows (filled by <see cref="WorldEventDirector"/> when a fight ends).</summary>
    public sealed class WorldEventResult
    {
        public WorldEventKind Kind;
        public bool Won;
        public bool Died;
        public string BossName;
        public int Points;
        public int ChestTier;

        /// <summary>False when this window's prize had already been received (a second win pays only the kills).</summary>
        public bool Paid;

        public int Experience;
        public int Gold;
        public int BlessingStones;
        public int SoulStones;
        public string HouseItemName;
        public string MountName;
        public bool BagOverflow;
        public string CousinLine;
    }

    /// <summary>
    /// Runs a world-boss fight or an Invasão de Āz inside Main.unity, self-bootstrapping like
    /// <c>EventRoomDirector</c> (no scene edit): after a scene load whose pending encounter is a world event
    /// (<see cref="EncounterPayload.IsWorldEvent"/>), a host is created in that scene and dies with it. The arena enters
    /// directed mode (<see cref="EncounterPayload.IsDirected"/>), dresses the floor and places the hero; this director
    /// spawns through <see cref="EncounterArenaController.SpawnDirected"/> and dresses each enemy with
    /// <see cref="WorldEventEnemyTag"/> (stats via the event rooms' scaler, crown / Āz's tin crown, night recolour).
    /// <list type="bullet">
    /// <item><b>World boss</b>: the boss alone, minions every 22 s, a "fúria" at half health; win = boss down; 5 min.</item>
    /// <item><b>Invasão</b>: 4 waves of the region's commons in tin crowns, then Āz, the bando's captain; points → chest tier; 4 min.</item>
    /// </list>
    /// Rewards are paid once per real-time window (<see cref="WorldEventProgress"/>) from a seeded roll
    /// (<see cref="WorldEventCalendar.RewardSeed"/>: window + player, no client RNG), then a results card, then
    /// <see cref="EncounterArenaController.ResolveDirected"/>. The HUD (<c>WorldEventHud</c>) polls <see cref="Current"/>.
    /// </summary>
    public class WorldEventDirector : MonoBehaviour
    {
        public const string HostName = "WorldEventDirector";
        private const string PlayerTag = "Player";
        private const string GoldItemId = "item_gold";
        public const float ResultsSeconds = 6f;
        private const float ArenaWaitSeconds = 6f;

        private static readonly List<GameObject> Buffer = new List<GameObject>();

        private readonly HashSet<GameObject> _alive = new HashSet<GameObject>();

        private EncounterPayload _payload;
        private EncounterArenaController _arena;
        private System.Random _spawnRandom;
        private readonly List<EnemyDefinition> _commons = new List<EnemyDefinition>();
        private EnemyDefinition _bossDefinition;
        private WorldBossDefinition _boss;
        private Transform _player;

        private float _waitTimer;
        private float _graceLeft;
        private bool _setupDone;
        private bool _started;
        private bool _finished;
        private float _elapsed;
        private float _lastCall = -999f;
        private int _wavesSpawned;
        private int _kills;
        private bool _raged;
        private GameObject _bossObject;
        private Health _bossHealth;
        private bool _bossDefeated;
        private bool _captainSpawned;
        private bool _captainDefeated;
        private float _resultsLeft = -1f;
        private EncounterOutcome _pendingOutcome;

        private string _banner;
        private float _bannerLeft;

        public static WorldEventDirector Current { get; private set; }

        public WorldEventKind Kind => _payload != null ? _payload.worldEvent : WorldEventKind.None;
        public bool IsNight => _payload != null && _payload.worldEventNight;
        public BiomeType Region => _payload != null ? _payload.biome : BiomeType.Forest;
        public WorldBossDefinition Boss => _boss;
        public bool HasStarted => _started;
        public bool IsRunning => _started && !_finished;
        public bool IsFinished => _finished;
        public float Duration => Kind == WorldEventKind.Invasion ? InvasionRules.FightSeconds : WorldBossRules.FightSeconds;
        public float TimeLeft => Mathf.Max(0f, Duration - _elapsed);
        public int WavesSpawned => _wavesSpawned;
        public bool CaptainSpawned => _captainSpawned;
        public int Kills => _kills;
        public int Points => InvasionRules.Points(_kills, _captainDefeated, TimeLeft);
        public WorldEventResult Result { get; private set; }
        public string Banner => _bannerLeft > 0f ? _banner : null;

        /// <summary>Boss health 0–1 (1 before it is up).</summary>
        public float BossHealthFraction => _bossHealth != null && _bossHealth.MaxHealth > 0f ? Mathf.Clamp01(_bossHealth.CurrentHealth / _bossHealth.MaxHealth) : (_bossDefeated ? 0f : 1f);

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
                || !transition.PendingEncounter.IsWorldEvent || FindFirstObjectByType<WorldEventDirector>() != null)
            {
                return;
            }

            var host = new GameObject(HostName);
            SceneManager.MoveGameObjectToScene(host, scene);
            host.AddComponent<WorldEventDirector>().Begin(transition.PendingEncounter);
        }

        public void Begin(EncounterPayload payload)
        {
            _payload = payload;
            _spawnRandom = new System.Random(payload != null ? payload.seed : 0);
            _boss = WorldBossRules.For(payload != null ? payload.biome : BiomeType.Forest);
            _commons.Clear();
            _bossDefinition = null;
            string bossEnemyId = _boss.EnemyId(Kind == WorldEventKind.WorldBoss && IsNight);
            if (payload != null)
            {
                foreach (EnemyDefinition enemy in payload.ResolveEnemies())
                {
                    if (enemy == null)
                    {
                        continue;
                    }

                    if (enemy.name == bossEnemyId)
                    {
                        _bossDefinition = enemy;
                    }

                    if (IsCommon(enemy) && !_commons.Contains(enemy))
                    {
                        _commons.Add(enemy);
                    }
                }
            }

            if (_bossDefinition == null && _commons.Count > 0)
            {
                _bossDefinition = _commons[0];
            }

            Current = this;
            if (ServiceLocator.TryGet(out LocalizationManager localization))
            {
                WorldBossTexts.EnsureRegistered(localization);
            }
        }

        /// <summary>A region common the boss calls / the bando is made of: everything but a night-only boss base.</summary>
        private bool IsCommon(EnemyDefinition enemy) => enemy != null && (enemy.name != _boss.NightEnemyId || _boss.NightIsRecolour);

        /// <summary>
        /// The payload of a world event in <paramref name="region"/>: the enemy ids are the region's day-table commons
        /// (weight &gt; 0, no bosses) plus the world boss's day and night bases.
        /// </summary>
        public static EncounterPayload BuildPayload(WorldEventKind kind, BiomeType region, long windowIndex, bool night, int seed)
        {
            if (kind == WorldEventKind.None)
            {
                return null;
            }

            WorldBossDefinition boss = WorldBossRules.For(region);
            var enemies = new List<EnemyDefinition>();
            BiomeDefinition biome = BiomeRegistry.Get(region);
            if (biome != null)
            {
                foreach (BiomeEnemySpawnEntry entry in biome.Enemies)
                {
                    EnemyDefinition enemy = entry?.Enemy;
                    if (enemy != null && !enemy.IsBoss && entry.Weight > 0 && !enemies.Contains(enemy))
                    {
                        enemies.Add(enemy);
                    }
                }
            }

            foreach (string id in new[] { boss.DayEnemyId, boss.NightEnemyId })
            {
                EnemyDefinition enemy = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{id}");
                if (enemy != null && !enemies.Contains(enemy))
                {
                    enemies.Add(enemy);
                }
            }

            if (enemies.Count == 0)
            {
                return null;
            }

            EncounterPayload payload = EncounterPayload.Create(region, enemies, seed);
            payload.worldEvent = kind;
            payload.worldEventWindow = windowIndex;
            payload.worldEventNight = night;
            return payload;
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

            if (_arena == null || !_arena.IsEncounterActive)
            {
                Finish(died: false, resolveArena: false);
                return;
            }

            _elapsed += Time.deltaTime;
            PruneDead();
            if (Kind == WorldEventKind.Invasion)
            {
                TickInvasion();
            }
            else
            {
                TickBoss();
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
                    GameLogger.Warning(HostName, "No arena in directed mode; leaving the world event.");
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
                WorldEventMoment moment = Kind == WorldEventKind.Invasion
                    ? WorldEventMoment.InvasionStart
                    : (IsNight ? WorldEventMoment.BossStartNight : WorldEventMoment.BossStart);
                ShowBanner(CousinLine(moment), 4.5f);
                return;
            }

            _graceLeft -= Time.deltaTime;
            if (_graceLeft > 0f)
            {
                return;
            }

            _started = true;
            _elapsed = 0f;
            if (Kind == WorldEventKind.WorldBoss)
            {
                SpawnBoss();
            }
        }

        // ------------------------------------------------------------------
        // World boss
        // ------------------------------------------------------------------

        private string BossDisplayName()
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            return WorldBossTexts.BossName(localization, _boss, IsNight);
        }

        private void SpawnBoss()
        {
            GameObject boss = _bossDefinition != null && _arena != null ? _arena.SpawnDirected(_bossDefinition, 0, 1, 1.2f) : null;
            if (boss == null)
            {
                GameLogger.Warning(HostName, "World boss could not spawn; ending the fight.");
                Finish(died: false, resolveArena: true);
                return;
            }

            bool recolour = IsNight && _boss.NightIsRecolour;
            WorldEventEnemyTag.Attach(boss, WorldEventEnemyRole.WorldBoss, WorldBossRules.Scale(IsNight), recolour, BossDisplayName());
            _bossObject = boss;
            _bossHealth = boss.GetComponent<EnemyController>() is EnemyController controller && controller.Health != null ? controller.Health : boss.GetComponent<Health>();
            _alive.Add(boss);
        }

        private void TickBoss()
        {
            if (_bossDefeated)
            {
                return;
            }

            int minionsAlive = _alive.Count - (_bossObject != null ? 1 : 0);
            int due = WorldBossRules.MinionsDue(_elapsed, _elapsed - _lastCall, minionsAlive);
            if (due > 0)
            {
                _lastCall = _elapsed;
                SpawnCommons(due, WorldEventEnemyRole.Minion, EventRoomEnemyScale.Identity, false);
                ShowBanner(Format(WorldBossTexts.HudMinions, BossDisplayName()), 2.5f);
            }

            if (_bossHealth != null && WorldBossRules.ShouldRage(BossHealthFraction, _raged))
            {
                _raged = true;
                int fit = Mathf.Max(0, Mathf.Min(WorldBossRules.RageMinions, WorldBossRules.MaxMinionsAlive + 1 - minionsAlive));
                SpawnCommons(fit, WorldEventEnemyRole.Minion, EventRoomEnemyScale.Identity, false);
                _lastCall = _elapsed;
                ShowBanner(Format(WorldBossTexts.HudRage, BossDisplayName()), 3f);
            }
        }

        // ------------------------------------------------------------------
        // Invasion
        // ------------------------------------------------------------------

        private void TickInvasion()
        {
            if (InvasionRules.ShouldSpawnWave(_wavesSpawned, _alive.Count, _elapsed - _lastCall))
            {
                int size = EventRoomRules.FitWave(InvasionRules.WaveSize(_wavesSpawned), _alive.Count, InvasionRules.MaxAlive);
                _lastCall = _elapsed;
                _wavesSpawned++;
                SpawnCommons(size, WorldEventEnemyRole.Bandit, InvasionRules.BanditScale, false);
                ShowBanner(Format(WorldBossTexts.HudWaveIncoming, _wavesSpawned), 2.5f);
                return;
            }

            if (InvasionRules.ShouldSpawnCaptain(_wavesSpawned, _alive.Count, _captainSpawned))
            {
                _captainSpawned = true;
                EnemyDefinition definition = _commons.Find(e => e != null && e.name == _boss.DayEnemyId) ?? (_commons.Count > 0 ? _commons[0] : null);
                GameObject captain = definition != null ? _arena.SpawnDirected(definition, 0, 1, 1f) : null;
                if (captain == null)
                {
                    _captainDefeated = true;
                    Finish(died: false, resolveArena: true);
                    return;
                }

                WorldEventEnemyTag.Attach(captain, WorldEventEnemyRole.Captain, InvasionRules.CaptainScale, false, Text(WorldBossTexts.CaptainName));
                _alive.Add(captain);
                _bossObject = captain;
                _bossHealth = captain.GetComponent<EnemyController>() is EnemyController controller && controller.Health != null ? controller.Health : captain.GetComponent<Health>();
                ShowBanner(Text(WorldBossTexts.HudCaptainArrives), 3f);
            }
        }

        /// <summary>The live objective line for the HUD.</summary>
        public string GoalText(LocalizationManager localization)
        {
            if (Kind == WorldEventKind.WorldBoss)
            {
                return WorldBossTexts.Format(localization, WorldBossTexts.HudGoalBoss, WorldBossTexts.BossName(localization, _boss, IsNight));
            }

            return _captainSpawned
                ? WorldBossTexts.Get(localization, WorldBossTexts.HudGoalCaptain)
                : WorldBossTexts.Format(localization, WorldBossTexts.HudGoalWave, Mathf.Max(1, _wavesSpawned), InvasionRules.WaveCount);
        }

        // ------------------------------------------------------------------
        // Spawning and deaths
        // ------------------------------------------------------------------

        private void SpawnCommons(int count, WorldEventEnemyRole role, EventRoomEnemyScale scale, bool recolour)
        {
            if (count <= 0 || _arena == null)
            {
                return;
            }

            List<EnemyDefinition> pool = _commons;
            if (pool.Count == 0)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            int spawned = 0;
            for (int i = 0; i < count; i++)
            {
                EnemyDefinition definition = pool[_spawnRandom.Next(pool.Count)];
                GameObject enemy = _arena.SpawnDirected(definition, i, count, 0.6f);
                if (enemy == null)
                {
                    continue;
                }

                string name = role == WorldEventEnemyRole.Bandit && i == 0
                    ? WorldBossTexts.Format(localization, WorldBossTexts.BanditName, definition.LocalizedName(localization))
                    : null;
                WorldEventEnemyTag.Attach(enemy, role, scale, recolour, name);
                _alive.Add(enemy);
                spawned++;
            }

            if (spawned > 0)
            {
                EventBus.Publish(new WaveStartedEvent(_wavesSpawned, spawned));
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

            bool main = evt.Enemy == _bossObject;
            if (Kind == WorldEventKind.Invasion)
            {
                if (main)
                {
                    _captainDefeated = true;
                    CombatFxManager.SpawnLootText(evt.Enemy.transform.position + Vector3.up * 2.6f, "+" + InvasionRules.CaptainPoints);
                    Finish(died: false, resolveArena: true);
                }
                else
                {
                    _kills++;
                    CombatFxManager.SpawnLootText(evt.Enemy.transform.position + Vector3.up * 2.6f, "+" + InvasionRules.KillPoints);
                }

                return;
            }

            if (main)
            {
                _bossDefeated = true;
                Finish(died: false, resolveArena: true);
            }
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            if (IsRunning)
            {
                // The arena / death screen own the Defeat; an invasion still pays what its points earned.
                Finish(died: true, resolveArena: false);
            }
        }

        // ------------------------------------------------------------------
        // End and rewards
        // ------------------------------------------------------------------

        private void Finish(bool died, bool resolveArena)
        {
            if (_finished)
            {
                return;
            }

            _finished = true;
            WorldEventProgress progress = WorldEventProgress.ResolveShared();
            long window = _payload != null ? _payload.worldEventWindow : -1;
            var result = new WorldEventResult { Kind = Kind, Died = died, BossName = BossDisplayName() };
            int seed = WorldEventCalendar.RewardSeed(Kind == WorldEventKind.Invasion ? "invasion" : "world_boss", _boss.Id, window, WorldEventProgress.PlayerId());

            if (Kind == WorldEventKind.Invasion)
            {
                result.Won = _captainDefeated;
                result.Points = Points;
                result.ChestTier = InvasionRules.TierFor(result.Points);
                result.Paid = progress.RecordInvasion(window, result.ChestTier, _captainDefeated);
                if (result.Paid)
                {
                    bool owns = MountCollection.ResolveShared().Owns(InvasionRules.ChestMountId);
                    CangacoChest chest = InvasionRules.ChestFor(result.ChestTier, Region, seed, owns);
                    GrantChest(result, chest, seed);
                }
            }
            else
            {
                result.Won = _bossDefeated;
                if (_bossDefeated)
                {
                    result.Paid = progress.RecordBossDefeat(_boss.Id, window, IsNight);
                    if (result.Paid)
                    {
                        GrantBossReward(result, WorldBossRules.RewardFor(_boss, IsNight, seed), seed);
                    }
                }
            }

            WorldEventMoment moment = Kind == WorldEventKind.Invasion
                ? (result.Won ? WorldEventMoment.InvasionWin : WorldEventMoment.InvasionLose)
                : (result.Won ? WorldEventMoment.BossWin : WorldEventMoment.BossLose);
            result.CousinLine = CousinLine(moment);
            Result = result;

            foreach (GameObject enemy in _alive)
            {
                if (enemy != null && enemy.TryGetComponent(out EnemyController controller))
                {
                    controller.HoldAggro(60f);
                }
            }

            GameLogger.Info(HostName, $"{Kind} {_boss.Id} finished: won {result.Won}, points {result.Points}, chest +{result.ChestTier}, paid {result.Paid}, died {died}.");
            if (resolveArena)
            {
                _pendingOutcome = result.Won ? EncounterOutcome.Victory : EncounterOutcome.Fled;
                _resultsLeft = ResultsSeconds;
            }
        }

        private void GrantBossReward(WorldEventResult result, WorldBossReward reward, int seed)
        {
            result.Gold = reward.Gold;
            // The kill XP itself comes from EnemyRewardGranter (scaled by the boss's EventRoomEnemyScaler); shown here.
            result.Experience = _bossDefinition != null
                ? EventRoomRules.ScaleExperience(_bossDefinition.ExperienceReward, WorldBossRules.Scale(IsNight).ExperienceMultiplier)
                : 0;
            GameObject player = Player();
            if (player != null && player.TryGetComponent(out InventoryManager inventory))
            {
                result.BagOverflow |= Grant(inventory, GoldItemId, reward.Gold, out _);
                result.BagOverflow |= Grant(inventory, EventRoomRules.BlessingStoneItemId, reward.BlessingStones, out result.BlessingStones);
                result.BagOverflow |= Grant(inventory, EventRoomRules.SoulStoneItemId, reward.SoulStones, out result.SoulStones);
            }

            if (reward.HouseItem)
            {
                result.HouseItemName = GrantHouseItem(HouseDropRules.BossPool(_boss.HouseGroupBossId), HouseDropRules.BossRarityWeights, seed);
            }

            result.MountName = GrantMount(reward.MountId);
        }

        private void GrantChest(WorldEventResult result, CangacoChest chest, int seed)
        {
            if (chest.IsEmpty)
            {
                return;
            }

            result.Experience = chest.Experience;
            result.Gold = chest.Gold;
            GameObject player = Player();
            if (player != null)
            {
                if (chest.Experience > 0 && player.TryGetComponent(out PlayerStats stats))
                {
                    stats.AddExperience(chest.Experience);
                }

                if (player.TryGetComponent(out InventoryManager inventory))
                {
                    result.BagOverflow |= Grant(inventory, GoldItemId, chest.Gold, out _);
                    result.BagOverflow |= Grant(inventory, EventRoomRules.BlessingStoneItemId, chest.BlessingStones, out result.BlessingStones);
                    result.BagOverflow |= Grant(inventory, EventRoomRules.SoulStoneItemId, chest.SoulStones, out result.SoulStones);
                }
            }

            if (chest.HouseItem)
            {
                result.HouseItemName = GrantHouseItem(HouseDropRules.CommonPool(_boss.DayEnemyId), HouseDropRules.CommonRarityWeights, seed);
            }

            result.MountName = GrantMount(chest.MountId);
        }

        /// <summary>A house item from <paramref name="pool"/> into the Depósito (seeded after the reward's own draws). Its name, or null.</summary>
        private string GrantHouseItem(List<(HouseItemDefinition Item, int Weight)> pool, IReadOnlyList<(ItemRarity Rarity, int Weight)> rarities, int seed)
        {
            if (pool == null || pool.Count == 0)
            {
                return null;
            }

            var random = new System.Random(seed ^ 0x5bd1e995);
            HouseItemDefinition item = HouseDropRules.PickFromPool(pool, HouseDropRules.PickRarity(rarities, random), random);
            HouseService house = item != null ? HouseService.ResolveShared() : null;
            if (house == null || !house.GrantItem(item.ItemId, HouseGrantReason.BossDrop, "world_event_" + _boss.Id))
            {
                return null;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            string name = HouseItemTexts.Name(localization, item.ItemId);
            return string.IsNullOrEmpty(name) ? item.ItemId : name;
        }

        private static string GrantMount(string mountId)
        {
            if (string.IsNullOrEmpty(mountId))
            {
                return null;
            }

            OwnedMount mount = MountCollection.ResolveShared().Grant(mountId, MountCollection.SourceEvent);
            if (mount == null)
            {
                return null;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            MountDefinition definition = MountCatalog.Get(mountId);
            return definition != null ? MountTexts.MountName(localization, definition) : mountId;
        }

        private GameObject Player()
        {
            return _player != null ? _player.gameObject : GameObject.FindGameObjectWithTag(PlayerTag);
        }

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
            return WorldBossTexts.Get(localization, key);
        }

        private static string Format(string key, params object[] args)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            return WorldBossTexts.Format(localization, key, args);
        }

        public static string CousinLine(WorldEventMoment moment)
        {
            CharacterId hero = ServiceLocator.TryGet(out CharacterSelectionState selection) && selection.Selected.HasValue
                ? selection.Selected.Value
                : CharacterId.Maithe;
            return Text(WorldBossTexts.LineKey(moment, hero));
        }
    }
}
