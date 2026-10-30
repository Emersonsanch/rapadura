using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Logging;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using Rapadura.UI.Death;
using Rapadura.UI.Menus;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Rapadura.Tests.PlayMode
{
    /// <summary>
    /// Soak test for TODO "30 minutos de gameplay contínuo sem bugs críticos": an autopilot plays the real
    /// WorldMap ⇄ Main loop for a configurable stretch of *game* time (default 30 min, scaled time raised
    /// to <see cref="SoakSettings.TimeScale"/>) and fails on the first sign of rot:
    /// <list type="bullet">
    /// <item>any Error/Exception/Assert log (the Test Framework fails the frame it happens; the count is
    /// re-checked at the end in case a caller ignores failing messages);</item>
    /// <item>GameObject growth on the idle world map beyond <see cref="SoakSettings.MaxGameObjectGrowth"/>
    /// (leaked DontDestroyOnLoad objects, pools that only grow), Unity object growth (meshes, textures,
    /// materials created in code and never destroyed) beyond <see cref="SoakSettings.MaxUnityObjectGrowth"/>,
    /// and managed heap growth beyond <see cref="SoakSettings.MaxManagedGrowthMb"/> — all measured after
    /// <c>Resources.UnloadUnusedAssets</c> + a full GC, against a baseline taken after a warm-up;</item>
    /// <item>a stuck state: no progress (scene change, encounter resolved, token arrival, menu closed...)
    /// for <see cref="SoakSettings.StuckSeconds"/> real seconds.</item>
    /// </list>
    /// Each cycle on the map: sometimes travel to another unlocked region, sometimes a manual save, every
    /// <see cref="SoakSettings.RestartEvery"/> cycles a cold restart + "Carregar" from the title (persistence
    /// checked), then an encounter — the region boss whenever it is offered, else "Explorar". In the arena
    /// the autopilot spends attribute/skill points, manages the bag (equips, discards when nearly full),
    /// walks to the nearest enemy, attacks, rotates the hotbar skills, opens and closes one HUD/pause
    /// screen (<see cref="UiScreenHandles"/>), drinks a "potion" (heal) below 35% health and, every
    /// <see cref="SoakSettings.DeathEvery"/> cycles, dies on purpose and takes the death screen's
    /// "Voltar ao mapa". An enemy the autopilot cannot reach (terrain) is finished by an "assist" after
    /// <see cref="SoakSettings.AssistAfterGameSeconds"/> without damage — counted, not a failure.
    ///
    /// A short report (cycles, outcomes, level, memory/object deltas, warnings) goes to the log
    /// ("[Soak]"), the test output and Logs/SoakReport.txt — also when the run fails.
    ///
    /// [Explicit]: excluded from the normal Test Runner/CI run (game-ci runs everything not explicit);
    /// run it by name or with the "Soak" category. Knobs via environment variables of the Unity process
    /// (see <see cref="SoakSettings.FromEnvironment"/>); <see cref="Soak_QuickCheck_3Minutes"/> is the
    /// same run cut to 3 game minutes for a fast sanity pass.
    /// </summary>
    [Category("Soak")]
    [Explicit("Soak test (~10-15 min real time): run by name or with the Soak category.")]
    public class SoakPlaythroughTests : WorldMapFlowTestBase
    {
        /// <summary>Run configuration. Every field can be overridden through an environment variable.</summary>
        public sealed class SoakSettings
        {
            public float GameMinutes = 30f;
            public float TimeScale = 4f;
            public float StuckSeconds = 90f;
            public float MaxManagedGrowthMb = 64f;
            public int MaxGameObjectGrowth = 150;
            public int MaxUnityObjectGrowth = 600;
            public int WarmupCycles = 3;
            public int SampleEvery = 3;
            public int TravelEvery = 3;
            public int SaveEvery = 4;
            public int RestartEvery = 10;
            public int DeathEvery = 7;
            public float AssistAfterGameSeconds = 25f;
            public int Seed = 20261029;
            public CharacterId Hero = CharacterId.Joaquim;

            /// <summary>
            /// RAPADURA_SOAK_MINUTES, _TIMESCALE, _STUCK_SECONDS, _MAX_MANAGED_MB, _MAX_GAMEOBJECTS,
            /// _MAX_UNITY_OBJECTS, _SEED and _HERO (Joaquim/Maria/Maithe/Icaro/Lavine). Unset or unparseable = default.
            /// </summary>
            public static SoakSettings FromEnvironment(float defaultMinutes)
            {
                var settings = new SoakSettings { GameMinutes = defaultMinutes };
                settings.GameMinutes = ReadFloat("RAPADURA_SOAK_MINUTES", settings.GameMinutes);
                settings.TimeScale = Mathf.Clamp(ReadFloat("RAPADURA_SOAK_TIMESCALE", settings.TimeScale), 1f, 20f);
                settings.StuckSeconds = ReadFloat("RAPADURA_SOAK_STUCK_SECONDS", settings.StuckSeconds);
                settings.MaxManagedGrowthMb = ReadFloat("RAPADURA_SOAK_MAX_MANAGED_MB", settings.MaxManagedGrowthMb);
                settings.MaxGameObjectGrowth = (int)ReadFloat("RAPADURA_SOAK_MAX_GAMEOBJECTS", settings.MaxGameObjectGrowth);
                settings.MaxUnityObjectGrowth = (int)ReadFloat("RAPADURA_SOAK_MAX_UNITY_OBJECTS", settings.MaxUnityObjectGrowth);
                settings.Seed = (int)ReadFloat("RAPADURA_SOAK_SEED", settings.Seed);
                if (Enum.TryParse(System.Environment.GetEnvironmentVariable("RAPADURA_SOAK_HERO"), true, out CharacterId hero))
                {
                    settings.Hero = hero;
                }

                return settings;
            }

            private static float ReadFloat(string name, float fallback)
            {
                string raw = System.Environment.GetEnvironmentVariable(name);
                return !string.IsNullOrWhiteSpace(raw) && float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) && value > 0f
                    ? value
                    : fallback;
            }
        }

        /// <summary>One idle-map measurement.</summary>
        private struct Sample
        {
            public int Cycle;
            public float GameMinutes;
            public long ManagedBytes;
            public int GameObjects;
            public int UnityObjects;
            public Dictionary<string, int> ByName;
        }

        private const int SaveSlot = 1;
        private const float PotionThreshold = 0.35f;
        private const float AttackDistance = 1.6f;
        private const float SkillIntervalGameSeconds = 1.5f;
        private const float ForcedDeathAfterGameSeconds = 3f;
        private const float MenuHoldRealSeconds = 0.25f;
        private const float MenuAfterGameSeconds = 2f;

        private static readonly FieldInfo DeathFlowField = typeof(DeathScreenController).GetField("_flow", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo DeathContinueMethod = typeof(DeathScreenController).GetMethod("OnContinueClicked", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly AttributeType[] Attributes = (AttributeType[])Enum.GetValues(typeof(AttributeType));

        private readonly List<GameObject> _spawned = new List<GameObject>();
        private readonly List<string> _errors = new List<string>();
        private readonly Dictionary<string, int> _warnings = new Dictionary<string, int>();
        private readonly Dictionary<EncounterOutcome, int> _outcomes = new Dictionary<EncounterOutcome, int>();
        private readonly List<Sample> _samples = new List<Sample>();
        private Action<EnemySpawnedEvent> _onSpawned;

        private SoakSettings _settings;
        private System.Random _random;
        private float _startGameTime;
        private float _startRealTime;
        private float _lastProgressRealTime;
        private string _lastProgress;
        private int _cycles;
        private int _bossFights;
        private int _travels;
        private int _deaths;
        private int _forcedDeaths;
        private int _menus;
        private int _saves;
        private int _restarts;
        private int _assists;
        private int _potions;
        private int _skillCasts;
        private int _attributePointsSpent;
        private int _skillPointsSpent;
        private int _itemsGained;
        private int _itemsDiscarded;
        private int _equips;
        private int _menuIndex;
        private readonly HashSet<string> _menuRefusals = new HashSet<string>();
        private int _heroLevel;
        private string _failure;
        private bool _reported;

        [SetUp]
        public void SoakSetUp()
        {
            _spawned.Clear();
            _errors.Clear();
            _warnings.Clear();
            _outcomes.Clear();
            _samples.Clear();
            _menuRefusals.Clear();
            _reported = false;
            _failure = null;
            _onSpawned = e => _spawned.Add(e.Enemy);
            EventBus.Subscribe(_onSpawned);
            Application.logMessageReceivedThreaded += OnLog;
        }

        [TearDown]
        public void SoakTearDown()
        {
            Application.logMessageReceivedThreaded -= OnLog;
            EventBus.Unsubscribe(_onSpawned);
            WorldMapController.SeedOverride = null;

            if (!_reported && _settings != null)
            {
                // The run stopped early (a failed wait, an error log): still leave the report behind.
                WriteReport(_failure ?? "interrompido antes do fim (veja a falha do teste)");
            }
        }

        [UnityTest]
        [Timeout(7200000)]
        public IEnumerator Soak_30MinutesOfGameplay_NoErrorsLeaksOrStalls()
        {
            yield return RunSoak(SoakSettings.FromEnvironment(defaultMinutes: 30f));
        }

        [UnityTest]
        [Timeout(1800000)]
        public IEnumerator Soak_QuickCheck_3Minutes()
        {
            SoakSettings settings = SoakSettings.FromEnvironment(defaultMinutes: 3f);
            settings.GameMinutes = Mathf.Min(settings.GameMinutes, 3f);
            settings.WarmupCycles = 1;
            settings.SampleEvery = 1;
            settings.RestartEvery = 4;
            settings.DeathEvery = 3;
            yield return RunSoak(settings);
        }

        // ------------------------------------------------------------------
        // Run
        // ------------------------------------------------------------------

        private IEnumerator RunSoak(SoakSettings settings)
        {
            _settings = settings;
            _random = new System.Random(settings.Seed);
            WorldMapController.SeedOverride = settings.Seed;
            _startRealTime = Time.realtimeSinceStartup;
            MarkProgress("início");

            // Title → Novo jogo → hero (prologue left pending: AutoPresent is off, see SmokeTestBase).
            yield return LoadScene(WorldMapSceneName);
            MainMenuController menu = FindInScene<MainMenuController>();
            Assert.IsNotNull(menu, "WorldMap.unity has no MainMenuController (title screen).");
            menu.OnNewGameRequested.Invoke();
            yield return WaitFrames(2);
            yield return SelectHero(settings.Hero, requireCharacterSelect: false);
            _startGameTime = Time.time;

            WorldMapController map = null;
            while (GameMinutesElapsed() < settings.GameMinutes)
            {
                _cycles++;
                yield return WaitForMap(m => map = m, settings.StuckSeconds);
                MarkProgress($"mapa, ciclo {_cycles}");

                if (_cycles % settings.TravelEvery == 0)
                {
                    yield return TravelSomewhere(map);
                    yield return WaitForMap(m => map = m, settings.StuckSeconds);
                }

                if (_cycles % settings.SaveEvery == 0)
                {
                    GetService<SaveManager>().Save(SaveSlot);
                    Assert.IsTrue(File.Exists(SaveManager.GetSlotFilePath(SaveSlot)), "A manual save wrote no file.");
                    _saves++;
                }

                if (_cycles % settings.RestartEvery == 0)
                {
                    yield return RestartAndLoad();
                    yield return WaitForMap(m => map = m, settings.StuckSeconds);
                }

                bool boss = map.CanChallengeCurrentBoss;
                bool forceDeath = _cycles % settings.DeathEvery == 0;
                yield return RunEncounter(map, boss, forceDeath);

                if (_cycles >= settings.WarmupCycles && (_cycles - settings.WarmupCycles) % settings.SampleEvery == 0)
                {
                    yield return WaitForMap(m => map = m, settings.StuckSeconds);
                    yield return TakeSample();
                    CheckGrowth();
                }
            }

            yield return WaitForMap(m => map = m, settings.StuckSeconds);
            yield return TakeSample();
            CheckGrowth();
            Assert.IsEmpty(_errors, "Errors were logged during the soak run:\n" + string.Join("\n", _errors.Take(10)));

            WriteReport(null);
        }

        private IEnumerator TravelSomewhere(WorldMapController map)
        {
            List<BiomeType> options = map.Progress.UnlockedRegions.Where(r => r != map.Progress.CurrentRegion).ToList();
            if (options.Count == 0)
            {
                yield break;
            }

            BiomeType destination = options[_random.Next(options.Count)];
            yield return TravelTo(destination, _settings.TimeScale);
            _travels++;
            MarkProgress($"viagem para {destination}");
        }

        /// <summary>Manual save, the app "closed" and reopened, "Carregar" from the title: same hero, same region.</summary>
        private IEnumerator RestartAndLoad()
        {
            GetService<SaveManager>().Save(SaveSlot);
            _saves++;
            BiomeType region = GetService<WorldMapProgress>().CurrentRegion;
            CharacterId? hero = GetService<CharacterSelectionState>().Selected;

            yield return ColdRestart();

            MainMenuController menu = FindInScene<MainMenuController>();
            Assert.IsNotNull(menu, "WorldMap.unity has no title screen after a restart.");
            Assert.IsNotNull(menu.LoadSlotHandler, "The title screen's \"Carregar\" is not wired after a restart.");
            Assert.IsTrue(menu.LoadSlotHandler(SaveSlot), $"\"Carregar\" slot {SaveSlot} failed after a restart.");
            yield return WaitFrames(3);
            Assert.AreEqual(GameFlowState.World, GetService<GameFlowService>().State, "Loading after a restart did not reach the world map.");
            Assert.AreEqual(hero, GetService<CharacterSelectionState>().Selected, "The restart + load lost the hero.");
            Assert.AreEqual(region, GetService<WorldMapProgress>().CurrentRegion, "The restart + load moved the token.");
            _restarts++;
            MarkProgress("reinício + carregar");
        }

        // ------------------------------------------------------------------
        // Encounter autopilot
        // ------------------------------------------------------------------

        private IEnumerator RunEncounter(WorldMapController map, bool boss, bool forceDeath)
        {
            SceneTransitionService transition = GetService<SceneTransitionService>();
            string what = boss ? "boss challenge" : "encounter";

            _spawned.Clear();
            Assert.IsTrue(boss ? map.TryChallengeBoss() : map.TryExplore(), $"The map refused to start the {what} (cycle {_cycles}).");
            if (boss)
            {
                _bossFights++;
            }

            yield return WaitProgress(() => SceneManager.GetActiveScene().name == MainSceneName && SceneManager.GetActiveScene().isLoaded,
                $"Main.unity to load for the {what}");

            EncounterArenaController arena = null;
            yield return WaitProgress(() =>
            {
                arena = UnityEngine.Object.FindFirstObjectByType<EncounterArenaController>();
                return arena != null && arena.IsEncounterActive;
            }, $"the arena to start the {what}");
            MarkProgress($"{what} começou");

            GameObject player = FindPlayer();
            PlayerStats stats = player.GetComponent<PlayerStats>();
            PlayerInputHandler input = player.GetComponent<PlayerInputHandler>();
            PlayerController controller = player.GetComponent<PlayerController>();
            SkillHotbar hotbar = player.GetComponent<SkillHotbar>();
            InventoryManager inventory = player.GetComponent<InventoryManager>();
            DeathScreenController deathScreen = FindInScene<DeathScreenController>();
            Assert.IsNotNull(stats, "The player has no PlayerStats.");
            Assert.IsNotNull(input, "The player has no PlayerInputHandler — the autopilot cannot move.");

            SpendPoints(player, stats);
            ManageInventory(inventory);
            int itemsBefore = CountItems(inventory);

            float arenaStart = Time.time;
            float lastDamageTime = Time.time;
            float lastSkillTime = Time.time;
            float lastEnemyHealth = float.MaxValue;
            bool menuDone = false;
            bool died = false;
            int slot = 0;

            while (arena != null && arena.IsEncounterActive)
            {
                if (deathScreen != null && deathScreen.IsOpen)
                {
                    input.TouchMoveInput = Vector2.zero;
                    yield return TakeDeathScreenExit(deathScreen);
                    died = true;
                    break;
                }

                if (Time.realtimeSinceStartup - _lastProgressRealTime > _settings.StuckSeconds)
                {
                    Fail($"stuck in the {what}: no enemy damage or outcome for {_settings.StuckSeconds:0} real seconds (cycle {_cycles}).");
                }

                RaiseTimeScale(_settings.TimeScale);
                float inArena = Time.time - arenaStart;

                if (forceDeath && !died && inArena > ForcedDeathAfterGameSeconds && !stats.IsDead)
                {
                    stats.ClearInvulnerability();
                    stats.ApplyDamage(1_000_000_000f);
                    _forcedDeaths++;
                    yield return null;
                    continue;
                }

                if (!forceDeath && !stats.IsDead && stats.CurrentHealth < stats.MaxHealth * PotionThreshold)
                {
                    stats.Heal(stats.MaxHealth);
                    _potions++;
                }

                if (!menuDone && inArena > MenuAfterGameSeconds && !GameplayPause.IsPaused)
                {
                    menuDone = true;
                    input.TouchMoveInput = Vector2.zero;
                    yield return OpenAndCloseOneScreen();
                    continue;
                }

                List<EnemyController> alive = AliveEnemies();
                float enemyHealth = alive.Sum(e => e.Health.CurrentHealth);
                if (enemyHealth < lastEnemyHealth - 0.01f)
                {
                    lastDamageTime = Time.time;
                    MarkProgress($"dano no {what}");
                }

                lastEnemyHealth = enemyHealth;

                EnemyController target = alive.OrderBy(e => (e.transform.position - player.transform.position).sqrMagnitude).FirstOrDefault();
                if (target == null)
                {
                    input.TouchMoveInput = Vector2.zero;
                    yield return null;
                    continue;
                }

                if (Time.time - lastDamageTime > _settings.AssistAfterGameSeconds)
                {
                    // Out of reach (terrain, flee-radius dance): finish it so the soak keeps cycling.
                    target.Health.ClearInvulnerability();
                    target.Health.ApplyDamage(1_000_000f, ElementType.Physical, player);
                    _assists++;
                    lastDamageTime = Time.time;
                    yield return null;
                    continue;
                }

                Vector3 toTarget = target.transform.position - player.transform.position;
                toTarget.y = 0f;
                Vector3 direction = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : player.transform.forward;
                Vector2 move = CameraRelative(controller != null ? controller.CameraTransform : null, direction);
                input.TouchMoveInput = toTarget.magnitude > AttackDistance ? move : move * 0.2f;

                if (toTarget.magnitude <= AttackDistance + 0.9f)
                {
                    input.TouchAttackRequested = true;
                }

                if (hotbar != null && hotbar.SlotCount > 0 && Time.time - lastSkillTime > SkillIntervalGameSeconds)
                {
                    lastSkillTime = Time.time;
                    if (hotbar.TryCastSlot(slot))
                    {
                        _skillCasts++;
                    }

                    slot = (slot + 1) % hotbar.SlotCount;
                }

                yield return null;
            }

            if (input != null)
            {
                input.TouchMoveInput = Vector2.zero;
            }

            _itemsGained += Math.Max(0, CountItems(inventory) - itemsBefore);
            _heroLevel = stats != null ? stats.Level : _heroLevel;

            yield return WaitProgress(() => SceneManager.GetActiveScene().name == WorldMapSceneName && SceneManager.GetActiveScene().isLoaded,
                $"the return to the world map after the {what}");
            RestoreTimeScale();

            Assert.IsFalse(transition.HasPendingEncounter, $"The {what} is still pending after returning to the map.");
            Assert.IsTrue(transition.LastOutcome.HasValue, $"The {what} returned to the map without an outcome.");
            EncounterOutcome outcome = transition.LastOutcome.Value;
            _outcomes[outcome] = _outcomes.TryGetValue(outcome, out int count) ? count + 1 : 1;
            if (forceDeath)
            {
                Assert.AreEqual(EncounterOutcome.Defeat, outcome, $"Dying on purpose in the {what} did not end it as a Defeat.");
            }

            MarkProgress($"{what} terminou ({outcome})");
        }

        /// <summary>The death screen's "Voltar ao mapa" as soon as it accepts a choice (its own auto-continue is the fallback).</summary>
        private IEnumerator TakeDeathScreenExit(DeathScreenController deathScreen)
        {
            _deaths++;
            MarkProgress("morte");
            while (deathScreen != null && deathScreen.IsOpen)
            {
                if (Time.realtimeSinceStartup - _lastProgressRealTime > _settings.StuckSeconds)
                {
                    Fail($"stuck on the death screen for {_settings.StuckSeconds:0} real seconds (cycle {_cycles}).");
                }

                if (DeathFlowField?.GetValue(deathScreen) is DeathScreenFlow flow && flow.CanChoose)
                {
                    DeathContinueMethod?.Invoke(deathScreen, null);
                }

                yield return null;
            }

            MarkProgress("saiu da tela de morte");
        }

        private IEnumerator OpenAndCloseOneScreen()
        {
            for (int attempt = 0; attempt < UiScreenHandles.All.Length; attempt++)
            {
                string id = UiScreenHandles.All[_menuIndex++ % UiScreenHandles.All.Length];
                UiScreenHandles.Handle screen = UiScreenHandles.Resolve(id);
                if (screen == null)
                {
                    continue;
                }

                screen.Open();
                yield return new WaitForSecondsRealtime(MenuHoldRealSeconds);
                if (!screen.IsOpen())
                {
                    // A screen may refuse to open mid-fight by design: noted in the report, not a failure.
                    _menuRefusals.Add(id);
                }

                screen.Close();
                yield return WaitProgress(() => !screen.IsOpen(), $"'{id}' to close mid-fight");
                yield return WaitFrames(2);
                _menus++;
                MarkProgress($"menu {id}");
                yield break;
            }
        }

        private void SpendPoints(GameObject player, PlayerStats stats)
        {
            AttributeSet attributes = player.GetComponent<AttributeSet>();
            if (attributes != null)
            {
                for (int guard = 0; attributes.AvailableAttributePoints > 0 && guard < 100; guard++)
                {
                    if (attributes.TrySpendPoint(Attributes[_random.Next(Attributes.Length)]))
                    {
                        _attributePointsSpent++;
                    }
                }
            }

            SkillManager skills = player.GetComponent<SkillManager>();
            if (skills == null || !GetService<CharacterSelectionState>().Selected.HasValue)
            {
                return;
            }

            IReadOnlyList<SkillDefinition> kit = CharacterSkillSet.GetSkills(GetService<CharacterSelectionState>().Selected.Value);
            for (int guard = 0; skills.AvailableSkillPoints > 0 && guard < 20; guard++)
            {
                SkillDefinition unlearned = kit.FirstOrDefault(s => s != null && !skills.IsSkillLearned(s.SkillId));
                bool spent = unlearned != null && skills.LearnSkillWithPoint(unlearned, stats.Level);
                if (!spent)
                {
                    List<SkillInstance> learned = skills.LearnedSkills.ToList();
                    spent = learned.Count > 0 && skills.LevelUpSkillWithPoint(learned[_random.Next(learned.Count)].Definition.SkillId);
                }

                if (!spent)
                {
                    break;
                }

                _skillPointsSpent++;
            }
        }

        /// <summary>Equips one bag item now and then and discards from the back when the bag is nearly full or overweight.</summary>
        private void ManageInventory(InventoryManager inventory)
        {
            if (inventory == null)
            {
                return;
            }

            IReadOnlyList<InventorySlotData> slots = inventory.Slots;
            var filled = new List<int>();
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && !string.IsNullOrEmpty(slots[i].itemId) && slots[i].quantity > 0)
                {
                    filled.Add(i);
                }
            }

            if (filled.Count > 0 && _cycles % 2 == 0 && inventory.EquipFromSlot(filled[_random.Next(filled.Count)]))
            {
                _equips++;
            }

            // The MU-style grid is the only limit (the weight limit is gone): keep at least 4 free cells.
            int free = inventory.FreeCellCount;
            for (int i = filled.Count - 1; i >= 0 && free < 4; i--)
            {
                InventorySlotData data = inventory.Slots[filled[i]];
                if (data == null || string.IsNullOrEmpty(data.itemId))
                {
                    continue;
                }

                int quantity = data.quantity;
                if (inventory.RemoveFromSlot(filled[i], quantity))
                {
                    _itemsDiscarded += quantity;
                    free = inventory.FreeCellCount;
                }
            }
        }

        private static int CountItems(InventoryManager inventory)
        {
            if (inventory == null)
            {
                return 0;
            }

            int total = 0;
            foreach (InventorySlotData slot in inventory.Slots)
            {
                if (slot != null && !string.IsNullOrEmpty(slot.itemId))
                {
                    total += slot.quantity;
                }
            }

            return total;
        }

        private List<EnemyController> AliveEnemies()
        {
            var alive = new List<EnemyController>();
            foreach (GameObject go in _spawned)
            {
                if (go == null)
                {
                    continue;
                }

                EnemyController enemy = go.GetComponent<EnemyController>();
                if (enemy != null && enemy.Health != null && enemy.Health.IsAlive)
                {
                    alive.Add(enemy);
                }
            }

            return alive;
        }

        /// <summary>World direction → move stick (x = camera right, y = camera forward), the same space the HUD joystick writes.</summary>
        private static Vector2 CameraRelative(Transform camera, Vector3 worldDirection)
        {
            if (camera == null)
            {
                return new Vector2(worldDirection.x, worldDirection.z).normalized;
            }

            Vector3 forward = Vector3.ProjectOnPlane(camera.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(camera.right, Vector3.up).normalized;
            var move = new Vector2(Vector3.Dot(worldDirection, right), Vector3.Dot(worldDirection, forward));
            return move.sqrMagnitude > 0.0001f ? move.normalized : Vector2.up;
        }

        // ------------------------------------------------------------------
        // Leak checks
        // ------------------------------------------------------------------

        private IEnumerator TakeSample()
        {
            yield return Resources.UnloadUnusedAssets();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            yield return null;

            GameObject[] objects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var byName = new Dictionary<string, int>();
            foreach (GameObject go in objects)
            {
                string name = go.name;
                byName[name] = byName.TryGetValue(name, out int n) ? n + 1 : 1;
            }

            _samples.Add(new Sample
            {
                Cycle = _cycles,
                GameMinutes = GameMinutesElapsed(),
                ManagedBytes = GC.GetTotalMemory(false),
                GameObjects = objects.Length,
                UnityObjects = Resources.FindObjectsOfTypeAll<Mesh>().Length + Resources.FindObjectsOfTypeAll<Texture>().Length
                               + Resources.FindObjectsOfTypeAll<Material>().Length,
                ByName = byName
            });
            MarkProgress("amostra de memória");
        }

        private void CheckGrowth()
        {
            if (_samples.Count < 2)
            {
                return;
            }

            Sample baseline = _samples[0];
            Sample last = _samples[_samples.Count - 1];

            float managedMb = (last.ManagedBytes - baseline.ManagedBytes) / (1024f * 1024f);
            if (managedMb > _settings.MaxManagedGrowthMb)
            {
                Fail($"managed memory grew {managedMb:0.0} MB since the baseline (limit {_settings.MaxManagedGrowthMb:0} MB).");
            }

            int objectGrowth = last.GameObjects - baseline.GameObjects;
            if (objectGrowth > _settings.MaxGameObjectGrowth)
            {
                Fail($"{objectGrowth} more GameObjects on the idle map than at the baseline (limit {_settings.MaxGameObjectGrowth}); top: {TopGrowth(baseline, last)}.");
            }

            int unityGrowth = last.UnityObjects - baseline.UnityObjects;
            if (unityGrowth > _settings.MaxUnityObjectGrowth)
            {
                Fail($"{unityGrowth} more meshes/textures/materials alive than at the baseline (limit {_settings.MaxUnityObjectGrowth}).");
            }
        }

        private static string TopGrowth(Sample baseline, Sample last)
        {
            if (baseline.ByName == null || last.ByName == null)
            {
                return "-";
            }

            IEnumerable<string> top = last.ByName
                .Select(kv => (Name: kv.Key, Delta: kv.Value - (baseline.ByName.TryGetValue(kv.Key, out int before) ? before : 0)))
                .Where(x => x.Delta > 0)
                .OrderByDescending(x => x.Delta)
                .Take(5)
                .Select(x => $"{x.Name} +{x.Delta}");
            string joined = string.Join(", ", top);
            return joined.Length > 0 ? joined : "-";
        }

        // ------------------------------------------------------------------
        // Progress, logs, report
        // ------------------------------------------------------------------

        private void MarkProgress(string what)
        {
            _lastProgressRealTime = Time.realtimeSinceStartup;
            _lastProgress = what;
        }

        /// <summary>Waits (scaled time raised) until <paramref name="condition"/>, failing as stuck after <see cref="SoakSettings.StuckSeconds"/> real seconds.</summary>
        private IEnumerator WaitProgress(Func<bool> condition, string what)
        {
            float deadline = Time.realtimeSinceStartup + _settings.StuckSeconds;
            while (!condition())
            {
                if (Time.realtimeSinceStartup > deadline)
                {
                    Fail($"stuck: {what} did not happen within {_settings.StuckSeconds:0} real seconds (cycle {_cycles}, last progress: {_lastProgress}).");
                }

                RaiseTimeScale(_settings.TimeScale);
                yield return null;
            }

            MarkProgress(what);
        }

        private void Fail(string reason)
        {
            _failure = reason;
            WriteReport(reason);
            Assert.Fail("Soak: " + reason);
        }

        private float GameMinutesElapsed() => (Time.time - _startGameTime) / 60f;

        private void OnLog(string condition, string stackTrace, LogType type)
        {
            lock (_errors)
            {
                if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                {
                    _errors.Add($"{type}: {condition}");
                }
                else if (type == LogType.Warning)
                {
                    string key = condition.Length > 70 ? condition.Substring(0, 70) : condition;
                    _warnings[key] = _warnings.TryGetValue(key, out int n) ? n + 1 : 1;
                }
            }
        }

        private void WriteReport(string failure)
        {
            _reported = true;
            var report = new StringBuilder();
            float realMinutes = (Time.realtimeSinceStartup - _startRealTime) / 60f;
            report.Append("Relatório do teste de resistência (soak)\n");
            report.Append($"Resultado: {(failure == null ? "OK" : "FALHOU — " + failure)}\n");
            report.Append($"Tempo de jogo: {GameMinutesElapsed():0.0} min (alvo {_settings.GameMinutes:0.#}) | tempo real: {realMinutes:0.0} min | timeScale {_settings.TimeScale:0.#} | seed {_settings.Seed}\n");
            report.Append($"Ciclos: {_cycles} | encontros: {string.Join(", ", _outcomes.Select(kv => kv.Key + " " + kv.Value))} | chefes: {_bossFights} | viagens: {_travels}\n");
            report.Append($"Mortes: {_deaths} (forçadas {_forcedDeaths}) | menus abertos/fechados: {_menus} | saves: {_saves} | reinícios + carregar: {_restarts}\n");
            report.Append($"Herói: {_settings.Hero} nível {_heroLevel} | habilidades lançadas: {_skillCasts} | pontos gastos: atributos {_attributePointsSpent}, habilidades {_skillPointsSpent}\n");
            report.Append($"Itens recebidos: {_itemsGained} | descartados: {_itemsDiscarded} | equipados: {_equips} | poções: {_potions} | assistências: {_assists}\n");

            if (ServiceLocatorHas(out WorldMapProgress progress))
            {
                report.Append($"Regiões liberadas: {string.Join(", ", progress.UnlockedRegions)} | atual: {progress.CurrentRegion}\n");
            }

            if (_samples.Count > 0)
            {
                Sample baseline = _samples[0];
                Sample last = _samples[_samples.Count - 1];
                report.Append($"Memória gerenciada: {baseline.ManagedBytes / 1048576f:0.0} MB (ciclo {baseline.Cycle}) → {last.ManagedBytes / 1048576f:0.0} MB (ciclo {last.Cycle}), limite +{_settings.MaxManagedGrowthMb:0} MB\n");
                report.Append($"GameObjects no mapa: {baseline.GameObjects} → {last.GameObjects}, limite +{_settings.MaxGameObjectGrowth}; maior crescimento: {TopGrowth(baseline, last)}\n");
                report.Append($"Meshes/texturas/materiais: {baseline.UnityObjects} → {last.UnityObjects}, limite +{_settings.MaxUnityObjectGrowth}\n");
            }

            lock (_errors)
            {
                report.Append($"Erros/exceções: {_errors.Count}");
                foreach (string error in _errors.Take(5))
                {
                    report.Append("\n  ").Append(error.Length > 200 ? error.Substring(0, 200) : error);
                }

                report.Append($"\nAvisos: {_warnings.Values.Sum()}");
                foreach (KeyValuePair<string, int> warning in _warnings.OrderByDescending(kv => kv.Value).Take(5))
                {
                    report.Append($"\n  {warning.Value}x {warning.Key}");
                }
            }

            string text = report.ToString();
            GameLogger.Info("Soak", text);
            TestContext.Out.WriteLine(text);

            try
            {
                string logs = Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? ".", "Logs");
                Directory.CreateDirectory(logs);
                File.WriteAllText(Path.Combine(logs, "SoakReport.txt"), text);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                GameLogger.Warning("Soak", "Could not write Logs/SoakReport.txt: " + e.Message);
            }
        }

        private static bool ServiceLocatorHas<T>(out T service)
        {
            return Rapadura.Core.DI.ServiceLocator.TryGet(out service);
        }
    }
}
