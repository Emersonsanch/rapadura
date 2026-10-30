using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using UnityEngine;

namespace Rapadura.Gameplay.Achievements
{
    /// <summary>JSON-serializable snapshot of <see cref="AchievementManager"/> (parallel arrays: JsonUtility has no dictionaries).</summary>
    [Serializable]
    public class AchievementSaveState
    {
        public int[] counterTypes = Array.Empty<int>();
        public long[] counterValues = Array.Empty<long>();

        /// <summary>"&lt;conditionTypeInt&gt;:&lt;id&gt;" for every id observed per condition.</summary>
        public string[] observedIds = Array.Empty<string>();

        public string[] unlockedIds = Array.Empty<string>();
        public string[] unlockedAtIso8601 = Array.Empty<string>();
    }

    /// <summary>Root of the per-profile file (<see cref="AchievementManager.ProfileFileName"/>), next to the save slots.</summary>
    [Serializable]
    public class AchievementProfileData
    {
        public int profileVersion = 1;

        /// <summary>True once achievements found in pre-profile save slots were merged in (done once, see <see cref="AchievementManager"/>).</summary>
        public bool legacySlotsImported;

        public AchievementSaveState achievements = new AchievementSaveState();
    }

    /// <summary>
    /// Tracks lifetime gameplay stats and unlocks <see cref="AchievementDefinition"/>s ("Conquistas").
    /// Purely event-driven — it subscribes to existing EventBus events in <see cref="Initialize"/> and
    /// never polls:
    /// <c>EnemyDiedEvent</c> (kills/bosses), <c>QuestCompletedEvent</c>, <c>PlayerExperienceChangedEvent</c>
    /// (level), <c>RecipeCraftedEvent</c>, <c>SkillCastEvent</c>/<c>ComboCompletedEvent</c> (player only),
    /// <c>WorldMapRegionUnlockedEvent</c> and <c>ItemSoldEvent</c> (gold earned).
    ///
    /// Achievements are evaluated against the recorded stats rather than counting per achievement, so
    /// an achievement added in a later build unlocks retroactively from progress already on the save.
    /// Each unlock publishes <see cref="AchievementUnlockedEvent"/> exactly once.
    ///
    /// Persistence is per profile, not per save slot: progress lives in
    /// <see cref="ProfileFilePath"/> (SaveManager.SaveFolderPath/profile.json, so tests' SaveRootOverride
    /// isolates it), loaded in <see cref="Initialize"/> and written on every unlock, on every slot save
    /// (<see cref="GameSavingEvent"/>), on app pause/quit (GameManager calls <see cref="SaveProfile"/>)
    /// and on <see cref="Shutdown"/>. Older builds stored achievements inside each slot under
    /// <see cref="Key"/>: the first time a profile is created, every existing slot's entry is merged
    /// into it once, and the manager stays registered with SaveManager as an <see cref="ISaveable"/>
    /// whose <see cref="CaptureState"/> writes nothing and whose <see cref="RestoreState"/> still merges
    /// a legacy slot that is loaded later. Definitions come from <c>Resources/Achievements</c> unless
    /// explicitly supplied (tests) — that constructor also leaves <see cref="PersistsProfile"/> off.
    /// </summary>
    public class AchievementManager : IManager, ISaveable
    {
        public const string Key = "Achievements";
        public const string ResourcesFolder = "Achievements";
        public const string PlayerTag = "Player";
        public const string ProfileFileName = "profile.json";

        private const string LegacySlotSearchPattern = "slot_*.json";
        private const string BackupFileExtension = ".bak";
        private const string TempFileExtension = ".tmp";

        private readonly List<AchievementDefinition> _definitions = new List<AchievementDefinition>();
        private readonly Dictionary<AchievementConditionType, long> _counters = new Dictionary<AchievementConditionType, long>();
        private readonly Dictionary<AchievementConditionType, HashSet<string>> _observedIds = new Dictionary<AchievementConditionType, HashSet<string>>();
        private readonly Dictionary<string, string> _unlocked = new Dictionary<string, string>();
        private readonly bool _loadFromResources;
        private bool _definitionsLoaded;
        private bool _subscribed;
        private bool _profileLoaded;
        private bool _suppressProfileWrites;

        public string SaveKey => Key;

        /// <summary>Where the profile lives right now (honours <see cref="SaveManager.SaveRootOverride"/>).</summary>
        public static string ProfileFilePath => Path.Combine(SaveManager.SaveFolderPath, ProfileFileName);

        /// <summary>
        /// Whether this manager reads/writes <see cref="ProfileFilePath"/>. On for the runtime
        /// (Resources) constructor, off for the custom-catalog one so unit tests never touch a
        /// profile unless they opt in (under TestSaveRoot).
        /// </summary>
        public bool PersistsProfile { get; set; }

        /// <summary>Every known definition, ordered by sort order then id.</summary>
        public IReadOnlyList<AchievementDefinition> Definitions
        {
            get
            {
                EnsureDefinitionsLoaded();
                return _definitions;
            }
        }

        public int UnlockedCount => _unlocked.Count;
        public int TotalCount => Definitions.Count;

        /// <summary>Loads definitions from <c>Resources/Achievements</c> (runtime default).</summary>
        public AchievementManager()
        {
            _loadFromResources = true;
            PersistsProfile = true;
        }

        /// <summary>Uses only the supplied definitions (tests / custom catalogs).</summary>
        public AchievementManager(IEnumerable<AchievementDefinition> definitions)
        {
            _loadFromResources = false;
            _definitionsLoaded = true;

            if (definitions != null)
            {
                foreach (AchievementDefinition definition in definitions)
                {
                    AddDefinition(definition);
                }

                SortDefinitions();
            }
        }

        public void Initialize()
        {
            EnsureDefinitionsLoaded();

            if (PersistsProfile && !_profileLoaded)
            {
                LoadProfile();
            }

            if (_subscribed)
            {
                return;
            }

            _subscribed = true;
            EventBus.Subscribe<GameSavingEvent>(OnGameSaving);
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Subscribe<QuestCompletedEvent>(OnQuestCompleted);
            EventBus.Subscribe<PlayerExperienceChangedEvent>(OnExperienceChanged);
            EventBus.Subscribe<RecipeCraftedEvent>(OnRecipeCrafted);
            EventBus.Subscribe<SkillCastEvent>(OnSkillCast);
            EventBus.Subscribe<ComboCompletedEvent>(OnComboCompleted);
            EventBus.Subscribe<WorldMapRegionUnlockedEvent>(OnRegionUnlocked);
            EventBus.Subscribe<ItemSoldEvent>(OnItemSold);
            EventBus.Subscribe<CampaignCompletedEvent>(OnCampaignCompleted);
            EventBus.Subscribe<NewGamePlusCycleCompletedEvent>(OnNewGamePlusCycleCompleted);
            EventBus.Subscribe<BestiaryCompletedEvent>(OnBestiaryCompleted);
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
                EventBus.Unsubscribe<GameSavingEvent>(OnGameSaving);
                EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
                EventBus.Unsubscribe<QuestCompletedEvent>(OnQuestCompleted);
                EventBus.Unsubscribe<PlayerExperienceChangedEvent>(OnExperienceChanged);
                EventBus.Unsubscribe<RecipeCraftedEvent>(OnRecipeCrafted);
                EventBus.Unsubscribe<SkillCastEvent>(OnSkillCast);
                EventBus.Unsubscribe<ComboCompletedEvent>(OnComboCompleted);
                EventBus.Unsubscribe<WorldMapRegionUnlockedEvent>(OnRegionUnlocked);
                EventBus.Unsubscribe<ItemSoldEvent>(OnItemSold);
                EventBus.Unsubscribe<CampaignCompletedEvent>(OnCampaignCompleted);
                EventBus.Unsubscribe<NewGamePlusCycleCompletedEvent>(OnNewGamePlusCycleCompleted);
                EventBus.Unsubscribe<BestiaryCompletedEvent>(OnBestiaryCompleted);
            }

            ClearProgress();
        }

        // ------------------------------------------------------------------
        // Definitions
        // ------------------------------------------------------------------

        private void EnsureDefinitionsLoaded()
        {
            if (_definitionsLoaded)
            {
                return;
            }

            _definitionsLoaded = true;

            if (_loadFromResources)
            {
                foreach (AchievementDefinition definition in Resources.LoadAll<AchievementDefinition>(ResourcesFolder))
                {
                    AddDefinition(definition);
                }
            }

            SortDefinitions();
        }

        /// <summary>Adds a definition at runtime (ignored if null, id-less or already known) and evaluates it immediately.</summary>
        public bool RegisterDefinition(AchievementDefinition definition)
        {
            EnsureDefinitionsLoaded();

            if (!AddDefinition(definition))
            {
                return false;
            }

            SortDefinitions();
            Evaluate(definition.ConditionType);
            return true;
        }

        private bool AddDefinition(AchievementDefinition definition)
        {
            if (definition == null || string.IsNullOrEmpty(definition.AchievementId) || GetDefinition(definition.AchievementId) != null)
            {
                return false;
            }

            _definitions.Add(definition);
            return true;
        }

        private void SortDefinitions()
        {
            _definitions.Sort((a, b) =>
            {
                int byOrder = a.SortOrder.CompareTo(b.SortOrder);
                return byOrder != 0 ? byOrder : string.CompareOrdinal(a.AchievementId, b.AchievementId);
            });
        }

        public AchievementDefinition GetDefinition(string achievementId)
        {
            return string.IsNullOrEmpty(achievementId) ? null : _definitions.FirstOrDefault(d => d.AchievementId == achievementId);
        }

        // ------------------------------------------------------------------
        // Queries
        // ------------------------------------------------------------------

        public bool IsUnlocked(string achievementId) => !string.IsNullOrEmpty(achievementId) && _unlocked.ContainsKey(achievementId);

        /// <summary>UTC unlock time, or null when still locked.</summary>
        public DateTime? GetUnlockedAt(string achievementId)
        {
            if (!string.IsNullOrEmpty(achievementId) && _unlocked.TryGetValue(achievementId, out string iso) &&
                DateTime.TryParse(iso, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime parsed))
            {
                return parsed;
            }

            return null;
        }

        /// <summary>Lifetime value of a stat (kills, gold earned, highest level...).</summary>
        public long GetStat(AchievementConditionType type) => _counters.TryGetValue(type, out long value) ? value : 0;

        public bool HasObserved(AchievementConditionType type, string id) =>
            !string.IsNullOrEmpty(id) && _observedIds.TryGetValue(type, out HashSet<string> ids) && ids.Contains(id);

        /// <summary>Current progress towards <paramref name="definition"/>, clamped to [0, TargetValue].</summary>
        public int GetProgress(AchievementDefinition definition)
        {
            if (definition == null)
            {
                return 0;
            }

            if (IsUnlocked(definition.AchievementId))
            {
                return definition.TargetValue;
            }

            long raw = definition.UsesTargetIds
                ? definition.TargetIds.Count(id => HasObserved(definition.ConditionType, id))
                : GetStat(definition.ConditionType);

            return (int)Math.Max(0, Math.Min(raw, definition.TargetValue));
        }

        /// <summary>0..1 progress fraction, for list progress bars.</summary>
        public float GetProgressFraction(AchievementDefinition definition)
        {
            return definition == null ? 0f : Mathf.Clamp01(GetProgress(definition) / (float)definition.TargetValue);
        }

        // ------------------------------------------------------------------
        // Recording
        // ------------------------------------------------------------------

        /// <summary>
        /// Adds <paramref name="amount"/> to a stat and records <paramref name="id"/> (if any) as observed.
        /// With <paramref name="distinct"/>, the counter only grows the first time that id is seen
        /// (e.g. a region unlocking twice still counts once). Public so other systems/tests can feed
        /// progress that has no EventBus event yet.
        /// </summary>
        public void RecordProgress(AchievementConditionType type, long amount, string id = null, bool distinct = false)
        {
            bool isNewId = ObserveId(type, id);

            if (amount > 0 && (!distinct || isNewId || string.IsNullOrEmpty(id)))
            {
                _counters[type] = GetStat(type) + amount;
            }

            Evaluate(type);
        }

        /// <summary>Raises a "highest value" stat (e.g. level) to <paramref name="value"/> if larger.</summary>
        public void RecordMax(AchievementConditionType type, long value)
        {
            if (value > GetStat(type))
            {
                _counters[type] = value;
            }

            Evaluate(type);
        }

        private bool ObserveId(AchievementConditionType type, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            if (!_observedIds.TryGetValue(type, out HashSet<string> ids))
            {
                ids = new HashSet<string>();
                _observedIds[type] = ids;
            }

            return ids.Add(id);
        }

        /// <summary>Unlocks an achievement directly (debug/cheat menus, scripted story beats). Returns true only the first time.</summary>
        public bool Unlock(string achievementId)
        {
            AchievementDefinition definition = GetDefinition(achievementId);
            return definition != null && UnlockInternal(definition, DateTime.UtcNow);
        }

        private void Evaluate(AchievementConditionType type)
        {
            EnsureDefinitionsLoaded();

            // Snapshot: an AchievementUnlockedEvent listener could register more definitions.
            foreach (AchievementDefinition definition in _definitions.ToArray())
            {
                if (definition.ConditionType == type && !IsUnlocked(definition.AchievementId) &&
                    GetProgress(definition) >= definition.TargetValue)
                {
                    UnlockInternal(definition, DateTime.UtcNow);
                }
            }
        }

        private void EvaluateAll()
        {
            foreach (AchievementConditionType type in Enum.GetValues(typeof(AchievementConditionType)))
            {
                Evaluate(type);
            }
        }

        private bool UnlockInternal(AchievementDefinition definition, DateTime unlockedAtUtc)
        {
            if (IsUnlocked(definition.AchievementId))
            {
                return false;
            }

            _unlocked[definition.AchievementId] = unlockedAtUtc.ToString("o");
            EventBus.Publish(new AchievementUnlockedEvent(definition));
            SaveProfile();
            return true;
        }

        private void ClearProgress()
        {
            _counters.Clear();
            _observedIds.Clear();
            _unlocked.Clear();
        }

        // ------------------------------------------------------------------
        // Event handlers
        // ------------------------------------------------------------------

        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            string enemyId = null;

            if (evt.Enemy != null)
            {
                EnemyController controller = evt.Enemy.GetComponent<EnemyController>();
                enemyId = controller != null ? DefeatedEnemyRegistry.GetId(controller.Definition) : null;
            }

            RecordProgress(AchievementConditionType.EnemiesKilled, 1, enemyId);

            if (evt.WasBoss)
            {
                RecordProgress(AchievementConditionType.BossesDefeated, 1, enemyId);
            }
        }

        private void OnQuestCompleted(QuestCompletedEvent evt) =>
            RecordProgress(AchievementConditionType.QuestsCompleted, 1, evt.QuestId);

        private void OnExperienceChanged(PlayerExperienceChangedEvent evt) =>
            RecordMax(AchievementConditionType.LevelReached, evt.Level);

        private void OnRecipeCrafted(RecipeCraftedEvent evt) =>
            RecordProgress(AchievementConditionType.ItemsCrafted, 1, evt.Recipe != null ? evt.Recipe.RecipeId : null);

        private void OnSkillCast(SkillCastEvent evt)
        {
            if (IsPlayer(evt.Caster))
            {
                RecordProgress(AchievementConditionType.SkillsCast, 1, evt.Skill != null ? evt.Skill.SkillId : null);
            }
        }

        private void OnComboCompleted(ComboCompletedEvent evt)
        {
            if (IsPlayer(evt.Caster))
            {
                RecordProgress(AchievementConditionType.CombosCompleted, 1, evt.Combo != null ? evt.Combo.ComboId : null);
            }
        }

        private void OnRegionUnlocked(WorldMapRegionUnlockedEvent evt) =>
            RecordProgress(AchievementConditionType.RegionsUnlocked, 1, evt.Region.ToString(), distinct: true);

        private void OnItemSold(ItemSoldEvent evt) =>
            RecordProgress(AchievementConditionType.GoldEarned, evt.TotalPayout, evt.ItemId);

        private void OnCampaignCompleted(CampaignCompletedEvent evt) =>
            RecordProgress(AchievementConditionType.CampaignCompleted, 1);

        private void OnNewGamePlusCycleCompleted(NewGamePlusCycleCompletedEvent evt) =>
            RecordMax(AchievementConditionType.NewGamePlusCompleted, evt.Cycle);

        private void OnBestiaryCompleted(BestiaryCompletedEvent evt) =>
            RecordProgress(AchievementConditionType.BestiaryCompleted, 1);

        private void OnGameSaving(GameSavingEvent evt) => SaveProfile();

        private static bool IsPlayer(GameObject caster) => caster != null && caster.CompareTag(PlayerTag);

        // ------------------------------------------------------------------
        // Profile file
        // ------------------------------------------------------------------

        /// <summary>
        /// Merges <see cref="ProfileFilePath"/> (or its ".bak" if the main file is unreadable) into the
        /// current progress. When no profile exists yet, or it predates the legacy import, every save
        /// slot's old "Achievements" entry is merged in once and the profile is written straight away.
        /// No-op unless <see cref="PersistsProfile"/>.
        /// </summary>
        public void LoadProfile()
        {
            if (!PersistsProfile)
            {
                return;
            }

            _profileLoaded = true;
            _suppressProfileWrites = true;
            bool needsWrite;

            try
            {
                string path = ProfileFilePath;
                AchievementProfileData profile = TryReadProfile(path) ?? TryReadProfile(path + BackupFileExtension);

                if (profile?.achievements != null)
                {
                    MergeState(profile.achievements);
                }

                needsWrite = profile == null || !profile.legacySlotsImported;

                if (needsWrite)
                {
                    ImportLegacySlots();
                }
            }
            finally
            {
                _suppressProfileWrites = false;
            }

            // Definitions added since the profile was written unlock from the restored stats.
            EvaluateAll();

            if (needsWrite)
            {
                SaveProfile();
            }
        }

        /// <summary>Writes the current progress to <see cref="ProfileFilePath"/> (temp file + ".bak", like save slots). No-op unless <see cref="PersistsProfile"/>.</summary>
        public void SaveProfile()
        {
            if (!PersistsProfile || _suppressProfileWrites)
            {
                return;
            }

            string path = ProfileFilePath;
            string tempPath = path + TempFileExtension;
            var profile = new AchievementProfileData { legacySlotsImported = true, achievements = CaptureProfileState() };

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(tempPath, JsonUtility.ToJson(profile, prettyPrint: true));

                if (File.Exists(path))
                {
                    // Never let a corrupted main file overwrite the last good backup.
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
                Debug.LogError($"[AchievementManager] Failed to write profile '{path}': {e.Message}");
            }
        }

        /// <summary>Merges the "Achievements" entry of every save slot in SaveManager.SaveFolderPath (older builds kept them per slot).</summary>
        private void ImportLegacySlots()
        {
            string folder = SaveManager.SaveFolderPath;

            if (!Directory.Exists(folder))
            {
                return;
            }

            foreach (string slotPath in Directory.GetFiles(folder, LegacySlotSearchPattern))
            {
                SaveData data = null;

                try
                {
                    data = JsonUtility.FromJson<SaveData>(File.ReadAllText(slotPath));
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[AchievementManager] Skipping unreadable save '{slotPath}' during legacy import: {e.Message}");
                }

                if (SaveManager.TryGetEntryState(data, Key, out AchievementSaveState legacy))
                {
                    MergeState(legacy);
                }
            }
        }

        private static AchievementProfileData TryReadProfile(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(path);
                return string.IsNullOrWhiteSpace(json) ? null : JsonUtility.FromJson<AchievementProfileData>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AchievementManager] Failed to read profile '{path}': {e.Message}");
                return null;
            }
        }

        // ------------------------------------------------------------------
        // ISaveable (legacy slots) / snapshots
        // ------------------------------------------------------------------

        /// <summary>
        /// Save slots no longer carry achievements (they live in the profile file), so this returns
        /// null and SaveManager writes no "Achievements" entry. Use <see cref="CaptureProfileState"/>
        /// for a snapshot.
        /// </summary>
        public object CaptureState() => null;

        /// <summary>Snapshot of the current lifetime progress (what the profile file stores).</summary>
        public AchievementSaveState CaptureProfileState()
        {
            var types = _counters.Keys.OrderBy(t => (int)t).ToList();
            var observed = new List<string>();

            foreach (KeyValuePair<AchievementConditionType, HashSet<string>> pair in _observedIds)
            {
                observed.AddRange(pair.Value.Select(id => $"{(int)pair.Key}:{id}"));
            }

            observed.Sort(StringComparer.Ordinal);
            var unlockedIds = _unlocked.Keys.OrderBy(id => id, StringComparer.Ordinal).ToArray();

            return new AchievementSaveState
            {
                counterTypes = types.Select(t => (int)t).ToArray(),
                counterValues = types.Select(t => _counters[t]).ToArray(),
                observedIds = observed.ToArray(),
                unlockedIds = unlockedIds,
                unlockedAtIso8601 = unlockedIds.Select(id => _unlocked[id]).ToArray()
            };
        }

        /// <summary>
        /// Merges a saved snapshot into the current progress instead of replacing it: achievements are
        /// per-profile lifetime progress, not per-save, so loading another (or an older) save never
        /// takes an unlock or a stat away. Counters keep the higher value, observed ids and unlocks are
        /// unioned (an unlock keeps its earliest known timestamp). For the same reason this manager is
        /// not an <see cref="IResettable"/>: "Novo jogo" keeps every achievement. Only legacy slots
        /// still have this entry; whatever they add is written to the profile right away.
        /// </summary>
        public void RestoreState(object state)
        {
            if (state is not AchievementSaveState saved)
            {
                return;
            }

            _suppressProfileWrites = true;

            try
            {
                MergeState(saved);

                // Achievements added after this save was written unlock now from the restored stats.
                EvaluateAll();
            }
            finally
            {
                _suppressProfileWrites = false;
            }

            SaveProfile();
        }

        private void MergeState(AchievementSaveState saved)
        {

            if (saved.counterTypes != null && saved.counterValues != null)
            {
                for (int i = 0; i < Math.Min(saved.counterTypes.Length, saved.counterValues.Length); i++)
                {
                    var type = (AchievementConditionType)saved.counterTypes[i];
                    _counters[type] = _counters.TryGetValue(type, out long current) ? Math.Max(current, saved.counterValues[i]) : saved.counterValues[i];
                }
            }

            if (saved.observedIds != null)
            {
                foreach (string entry in saved.observedIds)
                {
                    int separator = entry != null ? entry.IndexOf(':') : -1;

                    if (separator > 0 && int.TryParse(entry.Substring(0, separator), out int type))
                    {
                        ObserveId((AchievementConditionType)type, entry.Substring(separator + 1));
                    }
                }
            }

            if (saved.unlockedIds != null)
            {
                for (int i = 0; i < saved.unlockedIds.Length; i++)
                {
                    string id = saved.unlockedIds[i];

                    if (string.IsNullOrEmpty(id))
                    {
                        continue;
                    }

                    string at = saved.unlockedAtIso8601 != null && i < saved.unlockedAtIso8601.Length ? saved.unlockedAtIso8601[i] : null;

                    if (_unlocked.TryGetValue(id, out string existing) && !IsEarlier(at, existing))
                    {
                        continue;
                    }

                    _unlocked[id] = string.IsNullOrEmpty(at) ? existing ?? DateTime.UtcNow.ToString("o") : at;
                }
            }
        }

        private static bool IsEarlier(string candidateIso, string currentIso)
        {
            return DateTime.TryParse(candidateIso, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime candidate)
                && (!DateTime.TryParse(currentIso, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime current) || candidate < current);
        }
    }
}
