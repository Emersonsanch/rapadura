using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Managers;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.UI.CharacterSelect;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Rapadura.Tests.PlayMode
{
    /// <summary>
    /// Shared plumbing for the PlayMode smoke tests: loads the real build scenes through
    /// <see cref="SceneManager"/>, polls with real-time timeouts (the character select screen and
    /// the pause menu set <see cref="Time.timeScale"/> to 0, so scaled waits would hang), and fully
    /// tears the session down between tests — GameManager is DontDestroyOnLoad, so without
    /// destroying it every test after the first would inherit the previous one's hero, quests and
    /// world map progress.
    ///
    /// The real save folder is snapshotted in SetUp and restored in TearDown: encounters and world
    /// map travel auto-save (slot -1) through the real <c>SaveManager</c>, and a smoke run must never
    /// clobber the developer's own auto-save.
    /// </summary>
    public abstract class SmokeTestBase
    {
        public const string MainSceneName = "Main";
        public const string WorldMapSceneName = "WorldMap";

        protected const float SceneLoadTimeoutSeconds = 60f;
        protected const float DefaultTimeoutSeconds = 15f;

        private static int _emptySceneCounter;

        private readonly Dictionary<string, byte[]> _saveSnapshot = new Dictionary<string, byte[]>();
        private bool _saveFolderExisted;

        protected static string SaveFolderPath => Path.Combine(Application.persistentDataPath, "Saves");

        [UnitySetUp]
        public IEnumerator BaseSetUp()
        {
            SnapshotSaveFolder();
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator BaseTearDown()
        {
            // 1) Swap to an empty scene first, so no scene object is left calling ServiceLocator in
            //    Update after the GameManager (and with it every registration) is gone.
            Scene empty = SceneManager.CreateScene("SmokeTestEmpty_" + (++_emptySceneCounter));
            SceneManager.SetActiveScene(empty);

            var toUnload = new List<Scene>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene != empty && scene.isLoaded)
                {
                    toUnload.Add(scene);
                }
            }

            foreach (Scene scene in toUnload)
            {
                AsyncOperation op = SceneManager.UnloadSceneAsync(scene);
                while (op != null && !op.isDone)
                {
                    yield return null;
                }
            }

            // 2) Kill the persistent bootstrap so the next test builds a fresh session.
            foreach (GameManager manager in UnityEngine.Object.FindObjectsByType<GameManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                UnityEngine.Object.Destroy(manager.gameObject);
            }

            yield return null;
            yield return null;

            ServiceLocator.Clear();
            GameplayPause.Reset();
            Time.timeScale = 1f;

            RestoreSaveFolder();
        }

        // ------------------------------------------------------------------
        // Scene helpers
        // ------------------------------------------------------------------

        protected static IEnumerator LoadScene(string sceneName)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            Assert.IsNotNull(op, $"Scene '{sceneName}' could not be loaded — is it still in Build Settings?");

            float deadline = Time.realtimeSinceStartup + SceneLoadTimeoutSeconds;
            while (!op.isDone && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.IsTrue(op.isDone, $"Timed out loading scene '{sceneName}'.");
            Assert.AreEqual(sceneName, SceneManager.GetActiveScene().name);

            // Let Awake/OnEnable/Start and a couple of Updates run (late service resolution,
            // SaveManager registration retries, HUD binding...).
            yield return WaitFrames(5);
        }

        protected static IEnumerator WaitFrames(int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return null;
            }
        }

        /// <summary>Polls <paramref name="condition"/> every frame (real time, immune to timeScale 0) and fails with <paramref name="what"/> on timeout.</summary>
        protected static IEnumerator WaitUntilOrFail(Func<bool> condition, string what, float timeoutSeconds = DefaultTimeoutSeconds)
        {
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!condition() && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.IsTrue(condition(), $"Timed out after {timeoutSeconds:0.#}s waiting for: {what}");
        }

        protected static T FindInScene<T>() where T : UnityEngine.Object
        {
            return UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
        }

        protected static T[] FindAllInScene<T>() where T : UnityEngine.Object
        {
            return UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        protected static GameObject FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Assert.IsNotNull(player, "No active GameObject tagged 'Player' in the scene.");
            return player;
        }

        protected static TService GetService<TService>()
        {
            Assert.IsTrue(ServiceLocator.TryGet(out TService service), $"{typeof(TService).Name} is not registered in the ServiceLocator.");
            return service;
        }

        // ------------------------------------------------------------------
        // Hero selection
        // ------------------------------------------------------------------

        /// <summary>
        /// Picks a hero through public API only: records it in <see cref="CharacterSelectionState"/>
        /// and re-enables the scene's <see cref="CharacterSelectController"/> (when present), which by
        /// design re-applies an already-chosen hero to the fresh player (passive + signature skills)
        /// and closes itself — the same path Main.unity takes when re-entered from the world map.
        /// </summary>
        protected static IEnumerator SelectHero(CharacterId id)
        {
            CharacterSelectionState state = GetService<CharacterSelectionState>();
            state.Select(id);

            CharacterSelectController controller = FindInScene<CharacterSelectController>();
            Assert.IsNotNull(controller, "Main.unity has no CharacterSelectController — nothing applies the chosen hero to the player.");

            controller.gameObject.SetActive(false);
            controller.gameObject.SetActive(true);

            // OnEnable hides the screen and defers its own SetActive(false) to the next Update.
            yield return WaitFrames(3);

            Assert.AreEqual(1f, Time.timeScale, "Choosing a hero must unfreeze time (character select leaves timeScale at 0).");
        }

        // ------------------------------------------------------------------
        // Save folder isolation
        // ------------------------------------------------------------------

        private void SnapshotSaveFolder()
        {
            _saveSnapshot.Clear();
            _saveFolderExisted = Directory.Exists(SaveFolderPath);

            if (!_saveFolderExisted)
            {
                return;
            }

            foreach (string file in Directory.GetFiles(SaveFolderPath))
            {
                _saveSnapshot[file] = File.ReadAllBytes(file);
            }
        }

        private void RestoreSaveFolder()
        {
            try
            {
                if (Directory.Exists(SaveFolderPath))
                {
                    foreach (string file in Directory.GetFiles(SaveFolderPath))
                    {
                        if (!_saveSnapshot.ContainsKey(file))
                        {
                            File.Delete(file);
                        }
                    }
                }

                if (_saveFolderExisted)
                {
                    Directory.CreateDirectory(SaveFolderPath);
                    foreach (KeyValuePair<string, byte[]> pair in _saveSnapshot)
                    {
                        File.WriteAllBytes(pair.Key, pair.Value);
                    }
                }
            }
            catch (IOException e)
            {
                Debug.LogWarning($"[SmokeTestBase] Could not fully restore the save folder: {e.Message}");
            }
        }
    }
}
