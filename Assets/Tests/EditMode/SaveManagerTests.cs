using System;
using System.IO;
using NUnit.Framework;
using Rapadura.Core.Interfaces;
using Rapadura.Save;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Tests for SaveManager's versioning/migration and corruption-protection behavior. Uses
    /// dedicated, high-numbered slots under Application.persistentDataPath so it can't collide
    /// with real player save data, and cleans up after itself in TearDown.
    /// </summary>
    public class SaveManagerTests
    {
        private const int TestSlot = 90001;

        [Serializable]
        private class DummyState
        {
            public int value;
        }

        private class DummySaveable : ISaveable
        {
            public string SaveKey => "dummy";
            public int Value;

            public object CaptureState() => new DummyState { value = Value };

            public void RestoreState(object state)
            {
                Value = ((DummyState)state).value;
            }
        }

        private static string SaveFolderPath => Path.Combine(Application.persistentDataPath, "Saves");
        private static string SlotPath(int slot) => Path.Combine(SaveFolderPath, $"slot_{slot}.json");
        private static string BackupPath(int slot) => SlotPath(slot) + ".bak";
        private static string TempPath(int slot) => SlotPath(slot) + ".tmp";

        private SaveManager CreateManager()
        {
            var manager = new SaveManager();
            manager.Initialize();
            return manager;
        }

        private static void DeleteSlotFiles(int slot)
        {
            foreach (string path in new[] { SlotPath(slot), BackupPath(slot), TempPath(slot) })
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        [SetUp]
        public void SetUp()
        {
            DeleteSlotFiles(TestSlot);
            DeleteSlotFiles(SaveManager.AutoSaveSlot);
        }

        [TearDown]
        public void TearDown()
        {
            DeleteSlotFiles(TestSlot);
            DeleteSlotFiles(SaveManager.AutoSaveSlot);
        }

        // ---- Basic round trip -------------------------------------------------

        [Test]
        public void Save_ThenLoad_RestoresRegisteredSaveableState()
        {
            SaveManager manager = CreateManager();
            var saveable = new DummySaveable { Value = 42 };
            manager.Register(saveable);

            manager.Save(TestSlot);
            saveable.Value = 0;
            bool loaded = manager.Load(TestSlot);

            Assert.IsTrue(loaded);
            Assert.AreEqual(42, saveable.Value);
        }

        [Test]
        public void Save_WritesCurrentSaveVersion()
        {
            SaveManager manager = CreateManager();
            manager.Save(TestSlot);

            string json = File.ReadAllText(SlotPath(TestSlot));
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            Assert.AreEqual(SaveManager.CurrentSaveVersion.ToString(), saveData.saveVersion);
        }

        // ---- Auto save ----------------------------------------------------------

        [Test]
        public void AutoSave_WritesToReservedSlot_SeparateFromManualSlots()
        {
            SaveManager manager = CreateManager();
            var saveable = new DummySaveable { Value = 7 };
            manager.Register(saveable);

            manager.AutoSave();

            Assert.IsTrue(manager.HasAutoSave());
            Assert.IsFalse(manager.SlotExists(TestSlot));
        }

        [Test]
        public void LoadAutoSave_RestoresAutoSavedState()
        {
            SaveManager manager = CreateManager();
            var saveable = new DummySaveable { Value = 99 };
            manager.Register(saveable);
            manager.AutoSave();
            saveable.Value = 0;

            bool loaded = manager.LoadAutoSave();

            Assert.IsTrue(loaded);
            Assert.AreEqual(99, saveable.Value);
        }

        // ---- Versioning / migration --------------------------------------------

        [Test]
        public void Load_LegacyUnversionedSave_MigratesAndRestoresState()
        {
            // Simulate a save file written before saveVersion existed / with schema 0.
            var legacyState = new DummyState { value = 123 };
            var legacyData = new SaveData
            {
                saveVersion = "0",
                createdAtIso8601 = DateTime.UtcNow.ToString("o"),
                lastSavedAtIso8601 = DateTime.UtcNow.ToString("o")
            };
            legacyData.entries.Add(new SaveEntry
            {
                key = "dummy",
                typeName = typeof(DummyState).AssemblyQualifiedName,
                json = JsonUtility.ToJson(legacyState)
            });

            Directory.CreateDirectory(SaveFolderPath);
            File.WriteAllText(SlotPath(TestSlot), JsonUtility.ToJson(legacyData));

            SaveManager manager = CreateManager();
            var saveable = new DummySaveable();
            manager.Register(saveable);

            bool loaded = manager.Load(TestSlot);

            Assert.IsTrue(loaded);
            Assert.AreEqual(123, saveable.Value);
        }

        [Test]
        public void Load_LegacySave_ThenReSave_UpgradesStoredVersionOnDisk()
        {
            var legacyData = new SaveData { saveVersion = "0" };
            Directory.CreateDirectory(SaveFolderPath);
            File.WriteAllText(SlotPath(TestSlot), JsonUtility.ToJson(legacyData));

            SaveManager manager = CreateManager();
            manager.Load(TestSlot);
            manager.Save(TestSlot);

            string json = File.ReadAllText(SlotPath(TestSlot));
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            Assert.AreEqual(SaveManager.CurrentSaveVersion.ToString(), saveData.saveVersion);
        }

        // ---- Corruption protection ----------------------------------------------

        [Test]
        public void Save_TwiceInARow_CreatesBackupOfPreviousSave()
        {
            SaveManager manager = CreateManager();

            manager.Save(TestSlot);
            Assert.IsFalse(File.Exists(BackupPath(TestSlot)), "No backup should exist after the first save.");

            manager.Save(TestSlot);
            Assert.IsTrue(File.Exists(BackupPath(TestSlot)), "A backup should be created once a slot is overwritten.");
        }

        [Test]
        public void Load_CorruptedMainFile_FallsBackToBackup()
        {
            SaveManager manager = CreateManager();
            var saveable = new DummySaveable { Value = 11 };
            manager.Register(saveable);

            manager.Save(TestSlot); // creates slot_X.json, no backup yet

            saveable.Value = 22;
            manager.Save(TestSlot); // slot_X.json now has 22, backup has 11

            // Corrupt the main file.
            File.WriteAllText(SlotPath(TestSlot), "{ not valid json ][");

            saveable.Value = 0;
            bool loaded = manager.Load(TestSlot);

            Assert.IsTrue(loaded, "Load should recover from the backup instead of failing.");
            Assert.AreEqual(11, saveable.Value, "Restored state should come from the last known-good backup.");
        }

        [Test]
        public void Load_MissingMainFile_FallsBackToBackup()
        {
            SaveManager manager = CreateManager();
            var saveable = new DummySaveable { Value = 55 };
            manager.Register(saveable);

            manager.Save(TestSlot);
            manager.Save(TestSlot); // ensures a .bak exists

            File.Delete(SlotPath(TestSlot));

            bool loaded = manager.Load(TestSlot);

            Assert.IsTrue(loaded);
            Assert.AreEqual(55, saveable.Value);
        }

        [Test]
        public void Load_NoMainOrBackup_ReturnsFalse()
        {
            SaveManager manager = CreateManager();

            bool loaded = manager.Load(TestSlot);

            Assert.IsFalse(loaded);
        }

        [Test]
        public void Save_DoesNotLeaveTempFileBehind()
        {
            SaveManager manager = CreateManager();

            manager.Save(TestSlot);

            Assert.IsFalse(File.Exists(TempPath(TestSlot)));
        }

        [Test]
        public void DeleteSlot_RemovesMainAndBackupAndTempFiles()
        {
            SaveManager manager = CreateManager();
            manager.Save(TestSlot);
            manager.Save(TestSlot);
            File.WriteAllText(TempPath(TestSlot), "leftover");

            manager.DeleteSlot(TestSlot);

            Assert.IsFalse(File.Exists(SlotPath(TestSlot)));
            Assert.IsFalse(File.Exists(BackupPath(TestSlot)));
            Assert.IsFalse(File.Exists(TempPath(TestSlot)));
        }
    }
}
