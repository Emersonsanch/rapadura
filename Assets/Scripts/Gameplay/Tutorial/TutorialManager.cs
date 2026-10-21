using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Skills;
using Rapadura.Save;
using UnityEngine;

namespace Rapadura.Gameplay.Tutorial
{
    /// <summary>Root of the per-profile tutorial file (<see cref="TutorialManager.ProfileFileName"/>), next to the save slots.</summary>
    [Serializable]
    public class TutorialProfileData
    {
        public int profileVersion = 1;
        public bool hintsEnabled = true;
        public string[] completedStepIds = Array.Empty<string>();
    }

    /// <summary>
    /// Lightweight first-time onboarding ("dicas"). Purely event-driven: each
    /// <see cref="TutorialSteps"/> entry is triggered by an existing EventBus event the first time it
    /// happens (see the handlers below), queued as pending, and exposed one at a time through
    /// <see cref="CurrentHint"/> for the hint UI (<c>Rapadura.UI.Tutorial.TutorialHintController</c>),
    /// which calls <see cref="Acknowledge"/> on "Entendi" (or its auto-dismiss). A step is only marked
    /// done when acknowledged, so a hint cut short by quitting the app comes back next time.
    ///
    /// Hints never show on the title / hero screens (GameFlowState MainMenu/CharacterSelect): steps
    /// triggered there (e.g. the hero's starting skills, the daily quests a new game starts) stay
    /// pending until gameplay. <see cref="TutorialStepDefinition.Context"/> further limits a step to
    /// the world map or to encounters. GameFlowState.Boot (a scene played directly in the editor,
    /// without the front-end) counts as both.
    ///
    /// Persistence is per profile, like achievements: progress and the "Mostrar dicas" switch live in
    /// <see cref="ProfileFilePath"/> (SaveManager.SaveFolderPath/tutorial.json — its own file, since
    /// AchievementManager rewrites profile.json from its own data class), written on every change.
    /// Not an <see cref="IResettable"/>: "Novo jogo"/loading a save never brings hints back — only
    /// <see cref="ResetTutorial"/> ("Reiniciar tutorial" in Configurações) does.
    /// </summary>
    public class TutorialManager : IManager
    {
        public const string ProfileFileName = "tutorial.json";
        public const string PlayerTag = "Player";

        private const string BackupFileExtension = ".bak";
        private const string TempFileExtension = ".tmp";

        private readonly List<TutorialStepDefinition> _steps = new List<TutorialStepDefinition>();
        private readonly HashSet<string> _completed = new HashSet<string>();
        private readonly List<TutorialStepDefinition> _pending = new List<TutorialStepDefinition>();
        private GameFlowState _flowState = GameFlowState.Boot;
        private bool _enemyDefeatedThisSession;
        private bool _subscribed;
        private bool _profileLoaded;
        private TutorialStepDefinition _lastNotifiedHint;
        private float _nextCraftableCheck = float.NegativeInfinity;
        // Player's inventory for the default probe, cached until its scene unloads (Unity-null then).
        private InventoryManager _cachedInventory;

        /// <summary>Where the tutorial profile lives right now (honours <see cref="SaveManager.SaveRootOverride"/>).</summary>
        public static string ProfileFilePath => Path.Combine(SaveManager.SaveFolderPath, ProfileFileName);

        /// <summary>Whether this manager reads/writes <see cref="ProfileFilePath"/> (on for the runtime constructor, off for the custom-steps one used by tests).</summary>
        public bool PersistsProfile { get; set; }

        /// <summary>"Mostrar dicas": when off, nothing is triggered or shown (steps stay not-done, so they can still fire after turning it back on).</summary>
        public bool HintsEnabled { get; private set; } = true;

        /// <summary>
        /// "Does the player have the materials for a known recipe right now?" — checked on inventory
        /// changes for <see cref="TutorialSteps.Crafting"/>. Defaults to a ServiceLocator/Player-tag
        /// lookup; tests replace it (null disables the step).
        /// </summary>
        public Func<bool> CraftableRecipeProbe { get; set; }

        /// <summary>
        /// Minimum real seconds between two <see cref="CraftableRecipeProbe"/> runs triggered by
        /// <see cref="InventoryChangedEvent"/> (loot bursts publish several in one frame, and the default
        /// probe scans every recipe). A <see cref="RecipeUnlockedEvent"/> always probes. 0 = every change.
        /// </summary>
        public float CraftableCheckIntervalSeconds { get; set; } = 1f;

        /// <summary>Clock for <see cref="CraftableCheckIntervalSeconds"/>; defaults to <see cref="Time.unscaledTime"/>.</summary>
        public Func<float> Clock { get; set; } = () => Time.unscaledTime;

        public IReadOnlyList<TutorialStepDefinition> Steps => _steps;

        /// <summary>Raised whenever <see cref="CurrentHint"/> changes (new hint, acknowledged, context switch, toggle, reset).</summary>
        public event Action CurrentHintChanged;

        /// <summary>Built-in <see cref="TutorialSteps.All"/>, persisted to the profile.</summary>
        public TutorialManager() : this(TutorialSteps.All)
        {
            PersistsProfile = true;
            CraftableRecipeProbe = DefaultCraftableRecipeProbe;
        }

        /// <summary>Only the supplied steps; no profile file and no crafting probe unless set (tests).</summary>
        public TutorialManager(IEnumerable<TutorialStepDefinition> steps)
        {
            if (steps != null)
            {
                foreach (TutorialStepDefinition step in steps)
                {
                    if (step != null && !string.IsNullOrEmpty(step.Id) && GetStep(step.Id) == null)
                    {
                        _steps.Add(step);
                    }
                }
            }

            _steps.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        public void Initialize()
        {
            if (PersistsProfile && !_profileLoaded)
            {
                LoadProfile();
            }

            if (ServiceLocator.TryGet(out GameFlowService flow))
            {
                _flowState = flow.State;
            }

            if (_subscribed)
            {
                return;
            }

            _subscribed = true;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            EventBus.Subscribe<GameFlowStateChangedEvent>(OnFlowStateChanged);
            EventBus.Subscribe<EnemySpawnedEvent>(OnEnemySpawned);
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Subscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
            EventBus.Subscribe<SkillLearnedEvent>(OnSkillLearned);
            EventBus.Subscribe<SkillEquippedChangedEvent>(OnSkillEquipped);
            EventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            EventBus.Subscribe<RecipeUnlockedEvent>(OnRecipeUnlocked);
            EventBus.Subscribe<QuestStartedEvent>(OnQuestStarted);
            EventBus.Subscribe<PlayerExperienceChangedEvent>(OnExperienceChanged);
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Subscribe<TimeOfDayPhaseChangedEvent>(OnPhaseChanged);
            EventBus.Subscribe<NewSessionResetEvent>(OnNewSession);
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
        }

        public void Shutdown()
        {
            if (_profileLoaded)
            {
                SaveProfile();
                _profileLoaded = false;
            }

            if (_subscribed)
            {
                _subscribed = false;
                UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
                EventBus.Unsubscribe<GameFlowStateChangedEvent>(OnFlowStateChanged);
                EventBus.Unsubscribe<EnemySpawnedEvent>(OnEnemySpawned);
                EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
                EventBus.Unsubscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
                EventBus.Unsubscribe<SkillLearnedEvent>(OnSkillLearned);
                EventBus.Unsubscribe<SkillEquippedChangedEvent>(OnSkillEquipped);
                EventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
                EventBus.Unsubscribe<RecipeUnlockedEvent>(OnRecipeUnlocked);
                EventBus.Unsubscribe<QuestStartedEvent>(OnQuestStarted);
                EventBus.Unsubscribe<PlayerExperienceChangedEvent>(OnExperienceChanged);
                EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
                EventBus.Unsubscribe<TimeOfDayPhaseChangedEvent>(OnPhaseChanged);
                EventBus.Unsubscribe<NewSessionResetEvent>(OnNewSession);
                EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
            }

            _pending.Clear();
            _completed.Clear();
            HintsEnabled = true;
            _lastNotifiedHint = null;
            _cachedInventory = null;
            _nextCraftableCheck = float.NegativeInfinity;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode) => _cachedInventory = null;

        // ------------------------------------------------------------------
        // Queries
        // ------------------------------------------------------------------

        public TutorialStepDefinition GetStep(string stepId) =>
            string.IsNullOrEmpty(stepId) ? null : _steps.FirstOrDefault(s => s.Id == stepId);

        public bool IsCompleted(string stepId) => !string.IsNullOrEmpty(stepId) && _completed.Contains(stepId);

        public bool IsPending(string stepId) => _pending.Any(s => s.Id == stepId);

        public int CompletedCount => _completed.Count;

        public IReadOnlyCollection<string> CompletedStepIds => _completed;

        /// <summary>The flow state hints are gated on (title/hero screens show nothing).</summary>
        public GameFlowState FlowState => _flowState;

        /// <summary>
        /// Pending hint to show right now (an <see cref="TutorialStepDefinition.Interrupts"/> step first, else the
        /// lowest <see cref="TutorialStepDefinition.Order"/> allowed in the current context), or null.
        /// </summary>
        public TutorialStepDefinition CurrentHint
        {
            get
            {
                if (!HintsEnabled)
                {
                    return null;
                }

                TutorialStepDefinition best = null;
                foreach (TutorialStepDefinition step in _pending)
                {
                    if (IsAllowedNow(step) && (best == null || Precedes(step, best)))
                    {
                        best = step;
                    }
                }

                return best;
            }
        }

        /// <summary>An interrupting step beats any other; otherwise the lower <see cref="TutorialStepDefinition.Order"/> wins.</summary>
        private static bool Precedes(TutorialStepDefinition step, TutorialStepDefinition other)
        {
            if (step.Interrupts != other.Interrupts)
            {
                return step.Interrupts;
            }

            return step.Order < other.Order;
        }

        private bool IsAllowedNow(TutorialStepDefinition step)
        {
            switch (_flowState)
            {
                case GameFlowState.MainMenu:
                case GameFlowState.CharacterSelect:
                    return false;
                case GameFlowState.World:
                    return step.Context != TutorialContext.Encounter;
                case GameFlowState.Encounter:
                    return step.Context != TutorialContext.World;
                default:
                    return true; // Boot: a gameplay scene played directly, without the front-end.
            }
        }

        private bool IsFrontEnd => _flowState == GameFlowState.MainMenu || _flowState == GameFlowState.CharacterSelect;

        // ------------------------------------------------------------------
        // Commands
        // ------------------------------------------------------------------

        /// <summary>
        /// Queues <paramref name="stepId"/> as a pending hint. Returns false (and does nothing) when
        /// hints are off, the step is unknown, already done or already pending — so each step fires once.
        /// </summary>
        public bool Trigger(string stepId)
        {
            if (!HintsEnabled || IsCompleted(stepId) || IsPending(stepId))
            {
                return false;
            }

            TutorialStepDefinition step = GetStep(stepId);
            if (step == null)
            {
                return false;
            }

            _pending.Add(step);
            NotifyIfHintChanged();
            return true;
        }

        /// <summary>Marks a step done ("Entendi"), removes it from the queue and saves the profile. Returns false if it was already done.</summary>
        public bool Acknowledge(string stepId)
        {
            TutorialStepDefinition step = GetStep(stepId);
            if (step == null || IsCompleted(stepId))
            {
                return false;
            }

            _pending.RemoveAll(s => s.Id == stepId);
            _completed.Add(stepId);
            SaveProfile();
            NotifyIfHintChanged();
            return true;
        }

        /// <summary>"Mostrar dicas". Turning it off drops pending hints (not marked done) and hides the current one.</summary>
        public void SetHintsEnabled(bool enabled)
        {
            if (HintsEnabled == enabled)
            {
                return;
            }

            HintsEnabled = enabled;
            if (!enabled)
            {
                _pending.Clear();
            }

            SaveProfile();
            NotifyIfHintChanged();
        }

        /// <summary>"Reiniciar tutorial": every step can fire again (also turns hints back on).</summary>
        public void ResetTutorial()
        {
            _completed.Clear();
            _pending.Clear();
            _enemyDefeatedThisSession = false;
            HintsEnabled = true;
            SaveProfile();
            NotifyIfHintChanged();
        }

        private void NotifyIfHintChanged()
        {
            TutorialStepDefinition current = CurrentHint;
            if (current == _lastNotifiedHint)
            {
                return;
            }

            _lastNotifiedHint = current;
            CurrentHintChanged?.Invoke();
        }

        // ------------------------------------------------------------------
        // Event handlers
        // ------------------------------------------------------------------

        private void OnFlowStateChanged(GameFlowStateChangedEvent evt)
        {
            _flowState = evt.Current;

            if (evt.Current == GameFlowState.World)
            {
                Trigger(TutorialSteps.WorldMap);
            }

            NotifyIfHintChanged();
        }

        private void OnEnemySpawned(EnemySpawnedEvent evt) => Trigger(TutorialSteps.Encounter);

        private void OnEnemyDied(EnemyDiedEvent evt) => _enemyDefeatedThisSession = true;

        private void OnCombatTargetDied(CombatTargetDiedEvent evt)
        {
            // Loot is granted from this same event (EnemyLootGranter); the flag must be up before the
            // InventoryChangedEvent it causes, whatever the subscription order of EnemyDiedEvent.
            if (evt.Target != null && evt.Target.GetComponent<EnemyController>() != null)
            {
                _enemyDefeatedThisSession = true;
            }
        }

        private void OnSkillLearned(SkillLearnedEvent evt)
        {
            if (evt.Skill != null)
            {
                Trigger(TutorialSteps.Skills);
            }
        }

        private void OnSkillEquipped(SkillEquippedChangedEvent evt)
        {
            if (evt.Skill != null)
            {
                Trigger(TutorialSteps.Skills);
            }
        }

        private void OnInventoryChanged(InventoryChangedEvent evt)
        {
            // Starting gear / save restores also change the inventory: only loot after a fight counts.
            if (_enemyDefeatedThisSession)
            {
                Trigger(TutorialSteps.Loot);
            }

            CheckCraftable(force: false);
        }

        private void OnRecipeUnlocked(RecipeUnlockedEvent evt) => CheckCraftable(force: true);

        private void CheckCraftable(bool force)
        {
            if (!HintsEnabled || IsFrontEnd || CraftableRecipeProbe == null ||
                IsCompleted(TutorialSteps.Crafting) || IsPending(TutorialSteps.Crafting) || GetStep(TutorialSteps.Crafting) == null)
            {
                return;
            }

            // Throttled: a skipped change is picked up by the next one after the interval (the hint
            // is "first time it happens", a second of delay is harmless).
            float now = Clock != null ? Clock() : Time.unscaledTime;
            if (!force && now < _nextCraftableCheck)
            {
                return;
            }

            _nextCraftableCheck = now + Mathf.Max(0f, CraftableCheckIntervalSeconds);

            if (CraftableRecipeProbe())
            {
                Trigger(TutorialSteps.Crafting);
            }
        }

        private void OnQuestStarted(QuestStartedEvent evt) => Trigger(TutorialSteps.Quest);

        private void OnExperienceChanged(PlayerExperienceChangedEvent evt)
        {
            // "First time at level 2+" rather than "level went up": save restores and scene swaps
            // also publish this event with the restored level, so a delta would misfire.
            if (evt.Level >= 2)
            {
                Trigger(TutorialSteps.LevelUp);
            }
        }

        private void OnPlayerDied(PlayerDiedEvent evt) => Trigger(TutorialSteps.Death);

        private void OnPhaseChanged(TimeOfDayPhaseChangedEvent evt)
        {
            // The clock may tick behind the title screen; a night hint queued there would show in daylight.
            if (evt.CurrentPhase == TimeOfDayPhase.Night && !IsFrontEnd)
            {
                Trigger(TutorialSteps.Night);
            }
        }

        private void OnNewSession(NewSessionResetEvent evt) => _enemyDefeatedThisSession = false;

        private void OnGameLoaded(GameLoadedEvent evt) => _enemyDefeatedThisSession = false;

        private bool DefaultCraftableRecipeProbe()
        {
            if (!ServiceLocator.TryGet(out CraftingManager crafting))
            {
                return false;
            }

            if (_cachedInventory == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
                _cachedInventory = player != null ? player.GetComponent<InventoryManager>() : null;
            }

            InventoryManager inventory = _cachedInventory;
            if (inventory == null)
            {
                return false;
            }

            foreach (RecipeDefinition recipe in RecipeDatabase.GetAll())
            {
                if (recipe == null || recipe.ResultItem == null || !crafting.IsRecipeKnown(recipe) || !HasAnyIngredient(recipe))
                {
                    continue;
                }

                if (crafting.HasAllIngredients(recipe, inventory))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasAnyIngredient(RecipeDefinition recipe)
        {
            foreach (RecipeIngredient ingredient in recipe.Ingredients)
            {
                if (ingredient.item != null && ingredient.quantity > 0)
                {
                    return true;
                }
            }

            return false;
        }

        // ------------------------------------------------------------------
        // Profile file
        // ------------------------------------------------------------------

        /// <summary>Reads <see cref="ProfileFilePath"/> (or its ".bak"); no file = fresh profile with hints on. No-op unless <see cref="PersistsProfile"/>.</summary>
        public void LoadProfile()
        {
            if (!PersistsProfile)
            {
                return;
            }

            _profileLoaded = true;
            string path = ProfileFilePath;
            TutorialProfileData profile = TryReadProfile(path) ?? TryReadProfile(path + BackupFileExtension);

            _completed.Clear();
            _pending.Clear();
            HintsEnabled = true;

            if (profile == null)
            {
                return;
            }

            HintsEnabled = profile.hintsEnabled;
            if (profile.completedStepIds != null)
            {
                foreach (string id in profile.completedStepIds)
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        _completed.Add(id);
                    }
                }
            }
        }

        /// <summary>Writes <see cref="ProfileFilePath"/> (temp file + ".bak", like save slots). No-op unless <see cref="PersistsProfile"/>.</summary>
        public void SaveProfile()
        {
            if (!PersistsProfile)
            {
                return;
            }

            string path = ProfileFilePath;
            string tempPath = path + TempFileExtension;
            var profile = new TutorialProfileData
            {
                hintsEnabled = HintsEnabled,
                completedStepIds = _completed.OrderBy(id => id, StringComparer.Ordinal).ToArray()
            };

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(tempPath, JsonUtility.ToJson(profile, prettyPrint: true));

                if (File.Exists(path))
                {
                    if (TryReadProfile(path) != null)
                    {
                        File.Copy(path, path + BackupFileExtension, overwrite: true);
                    }

                    File.Delete(path);
                }

                File.Move(tempPath, path);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                Debug.LogError($"[TutorialManager] Failed to write profile '{path}': {e.Message}");
            }
        }

        private static TutorialProfileData TryReadProfile(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(path);
                return string.IsNullOrWhiteSpace(json) ? null : JsonUtility.FromJson<TutorialProfileData>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[TutorialManager] Failed to read profile '{path}': {e.Message}");
                return null;
            }
        }
    }
}
