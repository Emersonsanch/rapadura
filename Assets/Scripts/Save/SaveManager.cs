using System;
using System.Collections.Generic;
using System.IO;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using UnityEngine;

namespace Rapadura.Save
{
    /// <summary>
    /// Central manager responsible for persisting and restoring game state to local JSON files.
    /// Any system that needs to survive a save/load cycle implements <see cref="ISaveable"/> and
    /// registers itself here; SaveManager does not know or care what each system's data looks like.
    ///
    /// Files are written under Application.persistentDataPath, one file per save slot, so this
    /// works identically on device and in the editor. This is intentionally the only place that
    /// touches disk I/O for game state — a future online backend would replace/augment this class
    /// without any gameplay system needing to change.
    ///
    /// Writes are crash-safe: each save is first written to a temporary file and verified before
    /// it replaces the slot's real file, and the previous valid save is kept alongside it as a
    /// ".bak" file. If a slot's main file is ever found to be corrupted, Load transparently falls
    /// back to that backup instead of failing outright.
    /// </summary>
    public class SaveManager : IManager
    {
        /// <summary>Current save schema version. Bump this whenever SaveData's shape changes in a
        /// way older saves need migrating for, and add the corresponding step to <see cref="Migrations"/>.</summary>
        public const int CurrentSaveVersion = 1;

        /// <summary>Reserved slot number used by <see cref="AutoSave"/> / <see cref="LoadAutoSave"/> so
        /// automatic saves never collide with player-chosen manual save slots (which are &gt;= 0).</summary>
        public const int AutoSaveSlot = -1;

        private const string SaveFolderName = "Saves";
        private const string SaveFileExtension = ".json";
        private const string BackupFileExtension = ".bak";
        private const string TempFileExtension = ".tmp";

        private readonly Dictionary<string, ISaveable> _saveables = new Dictionary<string, ISaveable>();

        /// <summary>
        /// Incremental migration steps. Entry at index N mutates a <see cref="SaveData"/> in place
        /// from schema version N to N+1. <see cref="MigrateToCurrentVersion"/> walks this array
        /// starting at the save's own version until it reaches <see cref="CurrentSaveVersion"/>.
        /// </summary>
        private static readonly Action<SaveData>[] Migrations =
        {
            MigrateV0ToV1,
        };

        private string SaveFolderPath => Path.Combine(Application.persistentDataPath, SaveFolderName);

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
        }

        /// <summary>Registers a system so its state is included in future save/load operations.</summary>
        public void Register(ISaveable saveable)
        {
            _saveables[saveable.SaveKey] = saveable;
        }

        public void Unregister(ISaveable saveable)
        {
            _saveables.Remove(saveable.SaveKey);
        }

        private string GetSlotPath(int slot) => Path.Combine(SaveFolderPath, $"slot_{slot}{SaveFileExtension}");
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
                object state = pair.Value.CaptureState();

                if (state == null)
                {
                    continue;
                }

                saveData.entries.Add(new SaveEntry
                {
                    key = pair.Key,
                    typeName = state.GetType().AssemblyQualifiedName,
                    json = JsonUtility.ToJson(state)
                });
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
                File.WriteAllText(tempPath, json);

                // Verify the file we just wrote round-trips before touching the real save.
                if (TryReadSaveData(tempPath) == null)
                {
                    Debug.LogError($"[SaveManager] Failed to verify save write for slot {slot}; keeping previous save intact.");
                    SafeDelete(tempPath);
                    return;
                }

                if (File.Exists(mainPath))
                {
                    File.Copy(mainPath, backupPath, overwrite: true);
                    File.Delete(mainPath);
                }

                File.Move(tempPath, mainPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Save to slot {slot} failed: {e.Message}");
                SafeDelete(tempPath);
            }
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

            foreach (SaveEntry entry in saveData.entries)
            {
                if (!_saveables.TryGetValue(entry.key, out ISaveable saveable))
                {
                    continue;
                }

                Type stateType = Type.GetType(entry.typeName);

                if (stateType == null)
                {
                    Debug.LogWarning($"[SaveManager] Could not resolve type '{entry.typeName}' for save key '{entry.key}'. Skipping.");
                    continue;
                }

                object state = JsonUtility.FromJson(entry.json, stateType);
                saveable.RestoreState(state);
            }

            if (usedBackup)
            {
                Debug.LogWarning($"[SaveManager] Slot {slot}'s save file was missing or corrupted; restored from backup instead.");
            }

            EventBus.Publish(new GameLoadedEvent());
            return true;
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
