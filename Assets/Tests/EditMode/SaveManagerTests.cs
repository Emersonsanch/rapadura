using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rapadura.Core.Interfaces;
using Rapadura.Save;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rapadura.Tests
{
    /// <summary>
    /// Tests for SaveManager's versioning/migration, corruption-protection and save-root override
    /// behavior. Every test runs against a throwaway folder (<see cref="TestSaveRoot"/>) so it can't
    /// collide with real player save data, the auto-save slot included.
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

        private static string SaveFolderPath => SaveManager.SaveFolderPath;
        private static string SlotPath(int slot) => TestSaveRoot.SlotPath(slot);
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
            TestSaveRoot.Enter();
            DeleteSlotFiles(TestSlot);
            DeleteSlotFiles(SaveManager.AutoSaveSlot);
        }

        [TearDown]
        public void TearDown()
        {
            DeleteSlotFiles(TestSlot);
            DeleteSlotFiles(SaveManager.AutoSaveSlot);
            TestSaveRoot.Exit();
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

        [Test]
        public void Save_OverExistingSave_KeepsPreviousContentInBackup()
        {
            SaveManager manager = CreateManager();
            var saveable = new DummySaveable { Value = 1 };
            manager.Register(saveable);

            manager.Save(TestSlot);
            string firstJson = File.ReadAllText(SlotPath(TestSlot));

            saveable.Value = 2;
            manager.Save(TestSlot);

            Assert.AreEqual(firstJson, File.ReadAllText(BackupPath(TestSlot)), "The .bak should hold exactly the save that was replaced.");
            Assert.IsFalse(File.Exists(TempPath(TestSlot)), "The temp file must be swapped in, not left behind.");

            saveable.Value = 0;
            Assert.IsTrue(manager.Load(TestSlot));
            Assert.AreEqual(2, saveable.Value, "The main file should hold the newest save.");
        }

        [Test]
        public void Save_OverCorruptedMainFile_DoesNotOverwriteGoodBackup()
        {
            SaveManager manager = CreateManager();
            var saveable = new DummySaveable { Value = 5 };
            manager.Register(saveable);

            manager.Save(TestSlot);
            manager.Save(TestSlot); // .bak now holds a valid save with 5
            File.WriteAllText(SlotPath(TestSlot), "{ broken");

            saveable.Value = 6;
            manager.Save(TestSlot);

            SaveData backup = JsonUtility.FromJson<SaveData>(File.ReadAllText(BackupPath(TestSlot)));
            Assert.IsNotNull(backup, "A corrupted main file must never be rotated into the backup.");
            Assert.IsTrue(SaveManager.TryGetEntryState(backup, "dummy", out DummyState state));
            Assert.AreEqual(5, state.value);

            saveable.Value = 0;
            Assert.IsTrue(manager.Load(TestSlot));
            Assert.AreEqual(6, saveable.Value);
        }

        [Test]
        public void Load_CorruptedMainFile_LogsWarningAndUsesBackup()
        {
            SaveManager manager = CreateManager();
            var saveable = new DummySaveable { Value = 31 };
            manager.Register(saveable);
            manager.Save(TestSlot);
            manager.Save(TestSlot);
            File.WriteAllText(SlotPath(TestSlot), "not json at all");

            saveable.Value = 0;
            LogAssert.Expect(LogType.Warning, new Regex("restored from backup"));
            bool loaded = manager.Load(TestSlot);

            Assert.IsTrue(loaded);
            Assert.AreEqual(31, saveable.Value);
        }

        // ---- Save root override --------------------------------------------------

        [Test]
        public void SaveRootOverride_RoutesSlotFilesIntoOverrideFolder()
        {
            Assert.AreEqual(TestSaveRoot.Current, SaveManager.SaveFolderPath);

            SaveManager manager = CreateManager();
            manager.Save(TestSlot);
            manager.AutoSave();

            Assert.IsTrue(File.Exists(Path.Combine(TestSaveRoot.Current, $"slot_{TestSlot}.json")));
            Assert.IsTrue(File.Exists(Path.Combine(TestSaveRoot.Current, $"slot_{SaveManager.AutoSaveSlot}.json")));
            Assert.IsTrue(manager.HasAutoSave());
            Assert.IsNotNull(manager.PeekSlot(TestSlot));
        }

        [Test]
        public void SaveRootOverride_FolderMissing_SaveCreatesIt()
        {
            string nested = Path.Combine(TestSaveRoot.Current, "not", "yet", "created");
            SaveManager.SaveRootOverride = nested;

            var manager = new SaveManager(); // no Initialize: Save must create the folder itself
            manager.Save(TestSlot);

            Assert.IsTrue(File.Exists(Path.Combine(nested, $"slot_{TestSlot}.json")));
        }

        [Test]
        public void DefaultSaveFolderPath_IsPersistentDataPathSaves()
        {
            Assert.AreEqual(Path.Combine(Application.persistentDataPath, "Saves"), SaveManager.DefaultSaveFolderPath);
        }

        [Test]
        public void SaveFolderPath_WithoutOverride_IsDefaultPath()
        {
            string previous = SaveManager.SaveRootOverride;

            try
            {
                SaveManager.SaveRootOverride = null;
                Assert.AreEqual(SaveManager.DefaultSaveFolderPath, SaveManager.SaveFolderPath);
                Assert.AreEqual(Path.Combine(SaveManager.DefaultSaveFolderPath, "slot_3.json"), SaveManager.GetSlotFilePath(3));

                SaveManager.SaveRootOverride = string.Empty;
                Assert.AreEqual(SaveManager.DefaultSaveFolderPath, SaveManager.SaveFolderPath, "An empty override means 'no override'.");
            }
            finally
            {
                SaveManager.SaveRootOverride = previous;
            }
        }

        [Test]
        public void TestSaveRoot_Exit_RestoresPreviousOverrideAndDeletesFolder()
        {
            string folder = TestSaveRoot.Current;
            CreateManager().Save(TestSlot);

            TestSaveRoot.Exit();

            Assert.IsFalse(Directory.Exists(folder), "Exit should delete everything the test wrote.");
            Assert.AreEqual(SaveManager.DefaultSaveFolderPath, SaveManager.SaveFolderPath);

            TestSaveRoot.Enter(); // TearDown expects an active root
        }
    }
}
