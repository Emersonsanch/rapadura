using System;
using System.Collections.Generic;
using System.IO;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Systems;
using UnityEngine;

namespace Rapadura.Save
{
    /// <summary>
    /// Central manager responsible for persisting and restoring game state to local JSON files.
    /// Any system that needs to survive a save/load cycle implements <see cref="ISaveable"/> and
    /// registers itself here; SaveManager does not know or care what each system's data looks like.
    ///
    /// Files are written under Application.persistentDataPath/Saves (or <see cref="SaveRootOverride"/>
    /// when tests redirect it), one file per save slot, so this
    /// works identically on device and in the editor. This is intentionally the only place that
    /// touches disk I/O for game state — a future online backend would replace/augment this class
    /// without any gameplay system needing to change.
    ///
    /// Writes are crash-safe: each save is first written to a temporary file and verified before
    /// it replaces the slot's real file, and the previous valid save is kept alongside it as a
    /// ".bak" file. If a slot's main file is ever found to be corrupted, Load transparently falls
    /// back to that backup instead of failing outright.
    /// </summary>
    public class SaveManager : IManager, IResettable
    {
        /// <summary>Current save schema version. Bump this whenever SaveData's shape changes in a
        /// way older saves need migrating for, and add the corresponding step to <see cref="Migrations"/>.</summary>
        public const int CurrentSaveVersion = 2;

        /// <summary>Reserved slot number used by <see cref="AutoSave"/> / <see cref="LoadAutoSave"/> so
        /// automatic saves never collide with player-chosen manual save slots (which are &gt;= 0).</summary>
        public const int AutoSaveSlot = -1;

        private const string SaveFolderName = "Saves";
        private const string SaveFileExtension = ".json";
        private const string BackupFileExtension = ".bak";
        private const string TempFileExtension = ".tmp";

        private readonly Dictionary<string, ISaveable> _saveables = new Dictionary<string, ISaveable>();

        /// <summary>
        /// State of saveables that are not registered right now, keyed by save key — the "carried"
        /// session state. Player-scoped saveables (PlayerStats, InventoryManager, SkillManager, ...)
        /// live on the Player in Main.unity and unregister when it unloads, and a save is often loaded
        /// in WorldMap.unity where no Player exists. Without this, a "Continuar" from the title screen
        /// silently dropped the player's level/inventory/skills, every encounter started with a fresh
        /// level-1 player, and any save written on the world map lost the player entries entirely.
        /// Filled by <see cref="Load"/> (entries nobody registered for) and <see cref="Unregister"/>
        /// (last captured state), written back by every save, and applied to the next scene's fresh
        /// saveables by <see cref="RestoreCarriedState"/> (GameManager calls it after each scene load).
        /// </summary>
        private readonly Dictionary<string, SaveEntry> _carried = new Dictionary<string, SaveEntry>();

        /// <summary>
        /// Incremental migration steps. Entry at index N mutates a <see cref="SaveData"/> in place
        /// from schema version N to N+1. <see cref="MigrateToCurrentVersion"/> walks this array
        /// starting at the save's own version until it reaches <see cref="CurrentSaveVersion"/>.
        /// </summary>
        private static readonly Action<SaveData>[] Migrations =
        {
            MigrateV0ToV1,
            MigrateV1ToV2,
        };

        /// <summary>
        /// Optional replacement for the save folder. Null/empty (the default) means
        /// <see cref="DefaultSaveFolderPath"/>. Tests point this at a temp folder so they never touch
        /// the developer's real saves (including the auto-save slot -1); gameplay code never sets it.
        /// </summary>
        public static string SaveRootOverride { get; set; }

        /// <summary>Where saves live in a real session: Application.persistentDataPath/Saves.</summary>
        public static string DefaultSaveFolderPath => Path.Combine(Application.persistentDataPath, SaveFolderName);

        /// <summary>The folder every slot is read from / written to right now (honours <see cref="SaveRootOverride"/>).</summary>
        public static string SaveFolderPath => string.IsNullOrEmpty(SaveRootOverride) ? DefaultSaveFolderPath : SaveRootOverride;

        /// <summary>Full path of a slot's main file under the current <see cref="SaveFolderPath"/> (".bak"/".tmp" sit next to it).</summary>
        public static string GetSlotFilePath(int slot) => Path.Combine(SaveFolderPath, $"slot_{slot}{SaveFileExtension}");

        public void Initialize()
        {
            if (!Directory.Exists(SaveFolderPath))
            {
                Directory.CreateDirectory(SaveFolderPath);
            }
        }

        public void Shutdown()
        {
            _saveables.Clear();
            _carried.Clear();
        }

        private int _restoreDepth;

        /// <summary>
        /// True while <see cref="Load"/> (session reset included) or <see cref="RestoreCarriedState"/>
        /// is applying state, i.e. before their <see cref="GameLoadedEvent"/>. Events raised meanwhile
        /// (e.g. InventoryChangedEvent from InventoryManager.RestoreState) are restores, not gameplay —
        /// the tutorial's loot hint ignores them.
        /// </summary>
        public bool IsRestoring => _restoreDepth > 0;

        /// <summary>Registers a system so its state is included in future save/load operations.</summary>
        public void Register(ISaveable saveable)
        {
            _saveables[saveable.SaveKey] = saveable;
        }

        /// <summary>
        /// Stops tracking a system. Its current state is kept as carried session state (see
        /// <see cref="_carried"/>) so it survives the scene unload and is applied again by
        /// <see cref="RestoreCarriedState"/> once a new instance registers.
        /// </summary>
        public void Unregister(ISaveable saveable)
        {
            if (saveable == null)
            {
                return;
            }

            if (_saveables.TryGetValue(saveable.SaveKey, out ISaveable registered) && ReferenceEquals(registered, saveable))
            {
                SaveEntry entry = CaptureEntry(saveable.SaveKey, saveable);

                if (entry != null)
                {
                    _carried[entry.key] = entry;
                }

                _saveables.Remove(saveable.SaveKey);
            }
        }

        /// <summary>Whether state for <paramref name="key"/> is waiting for a saveable to register (see <see cref="RestoreCarriedState"/>).</summary>
        public bool HasCarriedState(string key) => key != null && _carried.ContainsKey(key);

        /// <summary>
        /// Read-only peek at the carried JSON for <paramref name="key"/> (the world map HUD shows the
        /// absent Player's level/Mochila from it). False when nothing is carried for that key.
        /// </summary>
        public bool TryGetCarriedJson(string key, out string json)
        {
            json = key != null && _carried.TryGetValue(key, out SaveEntry entry) ? entry?.json : null;
            return !string.IsNullOrEmpty(json);
        }

        /// <summary>"Novo jogo": forget the previous run's carried player state so the new hero starts fresh.</summary>
        public void ClearCarriedState()
        {
            _carried.Clear();
        }

        /// <summary><see cref="IResettable"/>: the only per-save state SaveManager owns is the carried player state.</summary>
        public void ResetForNewSession() => ClearCarriedState();

        /// <summary>
        /// Run by <see cref="Load"/> right before a file is applied (GameManager assigns the shared
        /// one). Every persistent per-save system goes back to new-game state first, so whatever the
        /// file has no entry for (older schema, a system added later) starts fresh instead of keeping
        /// the previous session's values. Null (tests, standalone use) keeps the old merge behavior.
        /// </summary>
        public NewSessionReset SessionReset { get; set; }

        /// <summary>
        /// Applies carried state to every saveable registered since it was carried (in
        /// <see cref="IRestoreOrder"/> order, like <see cref="Load"/>) and drops it from the carry.
        /// <see cref="ISceneLocalSaveable"/>s are skipped (their live state wins). Publishes
        /// <see cref="GameLoadedEvent"/> when anything was restored so UI refreshes. Returns how many
        /// saveables were restored.
        /// </summary>
        public int RestoreCarriedState()
        {
            if (_carried.Count == 0)
            {
                return 0;
            }

            var pending = new List<SaveEntry>();

            foreach (SaveEntry entry in _carried.Values)
            {
                if (_saveables.ContainsKey(entry.key))
                {
                    pending.Add(entry);
                }
            }

            int restored = 0;
            _restoreDepth++;

            try
            {
            foreach (SaveEntry entry in SortByRestoreOrder(pending))
            {
                ISaveable saveable = _saveables[entry.key];

                if (saveable is ISceneLocalSaveable)
                {
                    _carried.Remove(entry.key);
                    continue;
                }

                // Dropped from the carry only once applied: an entry whose restore threw stays carried,
                // so it is still written to the next save instead of being lost with the scene.
                if (TryRestoreEntry(saveable, entry))
                {
                    _carried.Remove(entry.key);
                    restored++;
                }
            }
            }
            finally
            {
                _restoreDepth--;
            }

            if (restored > 0)
            {
                EventBus.Publish(new GameLoadedEvent());
            }

            return restored;
        }

        private string GetSlotPath(int slot) => GetSlotFilePath(slot);
        private string GetBackupPath(int slot) => GetSlotPath(slot) + BackupFileExtension;
        private string GetTempPath(int slot) => GetSlotPath(slot) + TempFileExtension;

        public bool SlotExists(int slot)
        {
            return File.Exists(GetSlotPath(slot));
        }

        /// <summary>Whether an auto-save exists that could be offered to the player (e.g. "continue" on death).</summary>
        public bool HasAutoSave() => SlotExists(AutoSaveSlot);

        /// <summary>Captures every registered saveable's state and writes it to the given slot (manual save).</summary>
        public void Save(int slot)
        {
            SaveInternal(slot);
        }

        /// <summary>
        /// Captures every registered saveable's state and writes it to the reserved auto-save slot.
        /// Safe to call frequently (e.g. on checkpoint, on interval, on scene transition) — it goes
        /// through the same atomic write + backup path as a manual save, so it can never corrupt an
        /// existing manual save slot, and a failed auto-save never destroys the previous auto-save.
        /// </summary>
        public void AutoSave()
        {
            SaveInternal(AutoSaveSlot);
        }

        private void SaveInternal(int slot)
        {
            EventBus.Publish(new GameSavingEvent());

            SaveData previous = TryReadSaveData(GetSlotPath(slot));

            var saveData = new SaveData
            {
                saveVersion = CurrentSaveVersion.ToString(),
                createdAtIso8601 = previous?.createdAtIso8601 ?? DateTime.UtcNow.ToString("o"),
                lastSavedAtIso8601 = DateTime.UtcNow.ToString("o")
            };

            foreach (KeyValuePair<string, ISaveable> pair in _saveables)
            {
                SaveEntry entry = CaptureEntry(pair.Key, pair.Value);

                if (entry != null)
                {
                    saveData.entries.Add(entry);
                }
            }

            // Saveables not alive right now (e.g. the Player while on the world map) keep their last
            // known state in the file instead of vanishing from it.
            foreach (KeyValuePair<string, SaveEntry> pair in _carried)
            {
                if (!_saveables.ContainsKey(pair.Key))
                {
                    saveData.entries.Add(pair.Value);
                }
            }

            string json = JsonUtility.ToJson(saveData, prettyPrint: true);
            WriteAtomically(slot, json);
        }

        /// <summary>
        /// Writes <paramref name="json"/> to a temp file, verifies it can be read back, backs up the
        /// slot's current file (if any) to ".bak", then swaps the temp file into place. If verification
        /// fails, the existing save (and its backup) are left untouched.
        /// </summary>
        private void WriteAtomically(int slot, string json)
        {
            string mainPath = GetSlotPath(slot);
            string tempPath = GetTempPath(slot);
            string backupPath = GetBackupPath(slot);

            try
            {
                Directory.CreateDirectory(SaveFolderPath);
                File.WriteAllText(tempPath, json);

                // Verify the file we just wrote round-trips before touching the real save.
                if (TryReadSaveData(tempPath) == null)
                {
                    Debug.LogError($"[SaveManager] Failed to verify save write for slot {slot}; keeping previous save intact.");
                    SafeDelete(tempPath);
                    return;
                }

                if (!File.Exists(mainPath))
                {
                    File.Move(tempPath, mainPath);
                }
                else if (TryReadSaveData(mainPath) == null)
                {
                    // The current main file is already corrupt: never let it overwrite the last
                    // known-good backup. Drop it and put the fresh save in place.
                    File.Delete(mainPath);
                    File.Move(tempPath, mainPath);
                }
                else
                {
                    ReplaceWithBackup(tempPath, mainPath, backupPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Save to slot {slot} failed: {e.Message}");
                SafeDelete(tempPath);
            }
        }

        /// <summary>
        /// Swaps <paramref name="tempPath"/> into <paramref name="mainPath"/>, keeping the previous main
        /// file as <paramref name="backupPath"/>. Uses the OS's atomic File.Replace where available and
        /// falls back to copy + delete + move (still recoverable: the backup exists before the main file
        /// is removed, and Load falls back to it).
        /// </summary>
        private static void ReplaceWithBackup(string tempPath, string mainPath, string backupPath)
        {
            try
            {
                File.Replace(tempPath, mainPath, backupPath);
                return;
            }
            catch (Exception e) when (e is IOException || e is PlatformNotSupportedException || e is UnauthorizedAccessException)
            {
                if (!File.Exists(tempPath))
                {
                    throw;
                }
            }

            File.Copy(mainPath, backupPath, overwrite: true);
            File.Delete(mainPath);
            File.Move(tempPath, mainPath);
        }

        /// <summary>Reads a slot from disk (falling back to its backup if the main file is missing/corrupted),
        /// migrates it to the current schema if needed, and applies it to every currently registered saveable.</summary>
        public bool Load(int slot)
        {
            SaveData saveData = LoadValidOrBackup(slot, out bool usedBackup);

            if (saveData == null)
            {
                return false;
            }

            MigrateToCurrentVersion(saveData);

            _restoreDepth++;

            try
            {
            // The loaded file replaces the whole session, carried state included — and so does
            // anything the file doesn't mention (see SessionReset).
            SessionReset?.ResetAll();
            _carried.Clear();

            foreach (SaveEntry entry in GetEntriesInRestoreOrder(saveData))
            {
                if (!_saveables.TryGetValue(entry.key, out ISaveable saveable))
                {
                    // Nobody alive for it yet (e.g. player data loaded on the world map): carry it
                    // until its owner registers in a later scene (see RestoreCarriedState).
                    if (!string.IsNullOrEmpty(entry.key))
                    {
                        _carried[entry.key] = entry;
                    }

                    continue;
                }

                TryRestoreEntry(saveable, entry);
            }
            }
            finally
            {
                _restoreDepth--;
            }

            if (usedBackup)
            {
                Debug.LogWarning($"[SaveManager] Slot {slot}'s save file was missing or corrupted; restored from backup instead.");
            }

            EventBus.Publish(new GameLoadedEvent());
            return true;
        }

        /// <summary>
        /// Orders entries by their saveable's <see cref="IRestoreOrder.RestoreOrder"/> (lower first,
        /// <see cref="RestoreOrders.Default"/> when not implemented/unregistered). Stable, so equal
        /// orders keep the order they were written in — i.e. exactly the old behavior when nobody
        /// opts in.
        /// </summary>
        private List<SaveEntry> GetEntriesInRestoreOrder(SaveData saveData)
        {
            return SortByRestoreOrder(saveData.entries);
        }

        private List<SaveEntry> SortByRestoreOrder(List<SaveEntry> entries)
        {
            var ordered = new List<SaveEntry>(entries);
            var originalIndex = new Dictionary<SaveEntry, int>();

            for (int i = 0; i < ordered.Count; i++)
            {
                originalIndex[ordered[i]] = i;
            }

            ordered.Sort((a, b) =>
            {
                int byOrder = GetRestoreOrder(a.key).CompareTo(GetRestoreOrder(b.key));
                return byOrder != 0 ? byOrder : originalIndex[a].CompareTo(originalIndex[b]);
            });

            return ordered;
        }

        private static SaveEntry CaptureEntry(string key, ISaveable saveable)
        {
            object state;

            try
            {
                state = saveable.CaptureState();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveManager] CaptureState failed for '{key}': {e.Message}");
                return null;
            }

            if (state == null)
            {
                return null;
            }

            return new SaveEntry
            {
                key = key,
                typeName = state.GetType().AssemblyQualifiedName,
                json = JsonUtility.ToJson(state)
            };
        }

        /// <summary>
        /// Exception-safe like <see cref="CaptureEntry"/>: one broken entry (bad JSON, a RestoreState
        /// that throws) is logged and skipped instead of aborting the rest of <see cref="Load"/> /
        /// <see cref="RestoreCarriedState"/> — which, after SessionReset already wiped the session,
        /// left every later system at new-game state and never published GameLoadedEvent.
        /// </summary>
        private static bool TryRestoreEntry(ISaveable saveable, SaveEntry entry)
        {
            try
            {
                Type stateType = string.IsNullOrEmpty(entry.typeName) ? null : Type.GetType(entry.typeName);

                if (stateType == null)
                {
                    Debug.LogWarning($"[SaveManager] Could not resolve type '{entry.typeName}' for save key '{entry.key}'. Skipping.");
                    return false;
                }

                object state = JsonUtility.FromJson(entry.json, stateType);
                saveable.RestoreState(state);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveManager] RestoreState failed for '{entry.key}': {e.Message}");
                return false;
            }
        }

        private int GetRestoreOrder(string key)
        {
            return key != null && _saveables.TryGetValue(key, out ISaveable saveable) && saveable is IRestoreOrder ordered
                ? ordered.RestoreOrder
                : RestoreOrders.Default;
        }

        /// <summary>
        /// Reads (and migrates) a slot without applying it to any saveable — for UI that needs to
        /// preview a save before loading it (e.g. "Continuar com &lt;herói&gt;" on character select).
        /// Falls back to the backup file like <see cref="Load"/>. Returns null if neither is readable.
        /// </summary>
        public SaveData PeekSlot(int slot)
        {
            SaveData saveData = LoadValidOrBackup(slot, out _);

            if (saveData != null)
            {
                MigrateToCurrentVersion(saveData);
            }

            return saveData;
        }

        /// <summary>Deserializes one entry of a peeked <see cref="SaveData"/> by key. False if the key is absent (e.g. an older save) or of another type.</summary>
        public static bool TryGetEntryState<TState>(SaveData saveData, string key, out TState state) where TState : class
        {
            state = null;
            SaveEntry entry = saveData?.entries?.Find(e => e.key == key);

            if (entry == null || string.IsNullOrEmpty(entry.json))
            {
                return false;
            }

            try
            {
                state = JsonUtility.FromJson<TState>(entry.json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveManager] Failed to read entry '{key}': {e.Message}");
                return false;
            }

            return state != null;
        }

        /// <summary>Loads the reserved auto-save slot, if one exists.</summary>
        public bool LoadAutoSave() => Load(AutoSaveSlot);

        public void DeleteSlot(int slot)
        {
            SafeDelete(GetSlotPath(slot));
            SafeDelete(GetBackupPath(slot));
            SafeDelete(GetTempPath(slot));
        }

        private SaveData LoadValidOrBackup(int slot, out bool usedBackup)
        {
            usedBackup = false;

            SaveData data = TryReadSaveData(GetSlotPath(slot));

            if (data != null)
            {
                return data;
            }

            data = TryReadSaveData(GetBackupPath(slot));

            if (data != null)
            {
                usedBackup = true;
            }

            return data;
        }

        /// <summary>Reads and deserializes a save file, returning null (and logging) instead of throwing
        /// if the file is missing, empty, or not valid JSON — the caller decides how to react.</summary>
        private static SaveData TryReadSaveData(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(path);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return null;
                }

                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveManager] Failed to read save at '{path}': {e.Message}");
                return null;
            }
        }

        private static void MigrateToCurrentVersion(SaveData saveData)
        {
            int version = ParseVersion(saveData.saveVersion);

            while (version < CurrentSaveVersion && version < Migrations.Length)
            {
                Migrations[version](saveData);
                version++;
            }

            saveData.saveVersion = CurrentSaveVersion.ToString();
        }

        private static int ParseVersion(string versionString)
        {
            if (string.IsNullOrEmpty(versionString))
            {
                return 0;
            }

            string majorPart = versionString.Split('.')[0];
            return int.TryParse(majorPart, out int parsed) ? Math.Max(0, parsed) : 0;
        }

        /// <summary>
        /// Migrates a pre-versioning save (schema 0 — no meaningful <c>saveVersion</c>) to schema 1.
        /// No structural change is needed yet since versioning was introduced without altering the
        /// entry format; this exists to document and exercise the migration pipeline so the first
        /// real schema change only needs a new step appended to <see cref="Migrations"/>.
        /// </summary>
        private static void MigrateV0ToV1(SaveData saveData)
        {
            // No-op placeholder: v0 and v1 share the same entry format.
        }

        /// <summary>
        /// Schema 1 -&gt; 2: adds new, purely additive world-state slices — the selected character
        /// ("SelectedCharacter"), placed structures ("BuildingManager"), unlocked recipes
        /// ("CraftingManager") and defeated bosses ("DefeatedEnemies"). Nothing to rewrite: an older
        /// save simply has no entry for them, and Load never touches a saveable whose key is absent,
        /// so those systems keep their current (new-game) state. Kept as an explicit step so the
        /// version bump is documented where the pipeline lives.
        /// </summary>
        private static void MigrateV1ToV2(SaveData saveData)
        {
            // No-op: v2 only adds optional entries.
        }

        private static void SafeDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveManager] Failed to delete '{path}': {e.Message}");
            }
        }
    }
}
