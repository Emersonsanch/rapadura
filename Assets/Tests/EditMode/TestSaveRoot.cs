using System;
using System.IO;
using Rapadura.Save;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Points <see cref="SaveManager"/> at a fresh, empty temp folder so tests never read or write
    /// the developer's real saves under Application.persistentDataPath (the auto-save slot -1
    /// included). Call <see cref="Enter"/> in [SetUp] and <see cref="Exit"/> in [TearDown]; Exit
    /// restores the previous override and deletes the folder with everything a test wrote.
    /// </summary>
    public static class TestSaveRoot
    {
        private static string _previousOverride;
        private static string _current;

        /// <summary>The active temp save folder (null outside Enter/Exit).</summary>
        public static string Current => _current;

        /// <summary>Redirects saves to a new temp folder and returns its path.</summary>
        public static string Enter()
        {
            if (_current != null)
            {
                Exit();
            }

            _previousOverride = SaveManager.SaveRootOverride;
            _current = Path.Combine(Path.GetTempPath(), "RapaduraTestSaves", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_current);
            SaveManager.SaveRootOverride = _current;
            return _current;
        }

        /// <summary>Restores the previous save root and deletes the temp folder.</summary>
        public static void Exit()
        {
            if (_current == null)
            {
                return;
            }

            SaveManager.SaveRootOverride = _previousOverride;
            _previousOverride = null;

            try
            {
                if (Directory.Exists(_current))
                {
                    Directory.Delete(_current, recursive: true);
                }
            }
            catch (IOException e)
            {
                Debug.LogWarning($"[TestSaveRoot] Could not delete '{_current}': {e.Message}");
            }
            catch (UnauthorizedAccessException e)
            {
                Debug.LogWarning($"[TestSaveRoot] Could not delete '{_current}': {e.Message}");
            }

            _current = null;
        }

        /// <summary>Main file path of <paramref name="slot"/> inside the active save root.</summary>
        public static string SlotPath(int slot) => SaveManager.GetSlotFilePath(slot);
    }
}
