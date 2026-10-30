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
using Rapadura.Gameplay.Flow;
using Rapadura.Save;
using Rapadura.UI.CharacterSelect;
using Rapadura.UI.Story;
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
    /// Saves are redirected to a throwaway temp folder (<see cref="SaveManager.SaveRootOverride"/>)
    /// for each test: encounters and world map travel auto-save (slot -1) through the real
    /// <c>SaveManager</c>, and a smoke run must never touch the developer's own saves.
    /// </summary>
    public abstract class SmokeTestBase
    {
        public const string MainSceneName = "Main";
        public const string WorldMapSceneName = "WorldMap";

        protected const float SceneLoadTimeoutSeconds = 60f;
        protected const float DefaultTimeoutSeconds = 15f;

        private static int _emptySceneCounter;

        private string _previousSaveRoot;
        private string _testSaveRoot;

        /// <summary>The folder saves go to during this test (a temp folder, see <see cref="SaveManager.SaveRootOverride"/>).</summary>
        protected static string SaveFolderPath => SaveManager.SaveFolderPath;

        [UnitySetUp]
        public IEnumerator BaseSetUp()
        {
            RedirectSaveFolder();
            Time.timeScale = 1f;

            // The campaign prologue would hold GameplayPause over the first world map; smoke tests drive the map directly.
            StoryPanelController.AutoPresent = false;
            // Camera cutscenes (intro / outro) are presentation only: the flow tests never wait through them.
            CutscenePlayerController.AutoPlay = false;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator BaseTearDown()
        {
            // 1) Empty scene, every other scene unloaded, 2) the persistent bootstrap killed so the next
            //    test builds a fresh session.
            yield return UnloadSession("SmokeTestEmpty_" + (++_emptySceneCounter));

            StoryPanelController.AutoPresent = true;
            CutscenePlayerController.AutoPlay = true;

            RestoreSaveFolder();
        }

        // ------------------------------------------------------------------
        // Scene helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// Ends the running session like quitting the app, minus the save folder: swaps to a new empty scene
        /// named <paramref name="emptySceneName"/> first (so no scene object calls ServiceLocator in Update
        /// after the GameManager and every registration are gone), unloads every other scene, destroys the
        /// DontDestroyOnLoad GameManager, then clears the ServiceLocator, GameplayPause and timeScale.
        /// </summary>
        protected static IEnumerator UnloadSession(string emptySceneName)
        {
            Scene empty = SceneManager.CreateScene(emptySceneName);
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

            foreach (GameManager manager in UnityEngine.Object.FindObjectsByType<GameManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                UnityEngine.Object.Destroy(manager.gameObject);
            }

            yield return null;
            yield return null;

            ServiceLocator.Clear();
            GameplayPause.Reset();
            Time.timeScale = 1f;
        }

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
        protected static IEnumerator SelectHero(CharacterId id, bool requireCharacterSelect = true)
        {
            // On the title screen (WorldMap.unity boot) go through "Novo jogo" first, exactly like
            // the menu button; everywhere else the flow accepts a hero straight away.
            if (ServiceLocator.TryGet(out GameFlowService flow) && flow.State == GameFlowState.MainMenu)
            {
                Assert.IsTrue(flow.StartNewGame(), "GameFlowService refused StartNewGame from the title screen.");
                yield return WaitFrames(2);
            }

            CharacterSelectionState state = GetService<CharacterSelectionState>();
            state.Select(id);
            yield return null;

            CharacterSelectController controller = FindInScene<CharacterSelectController>();
            if (requireCharacterSelect)
            {
                Assert.IsNotNull(controller, "Scene has no CharacterSelectController — nothing applies the chosen hero to the player.");
            }

            if (controller != null)
            {
                controller.gameObject.SetActive(false);
                controller.gameObject.SetActive(true);
            }

            // OnEnable hides the screen and defers its own SetActive(false) to the next Update.
            yield return WaitFrames(3);

            yield return WaitUntilOrFail(() => Time.timeScale > 0f && !GameplayPause.IsPaused,
                "gameplay to unpause after choosing a hero (a front-end screen still holds GameplayPause?)", 5f);
        }

        // ------------------------------------------------------------------
        // Save folder isolation
        // ------------------------------------------------------------------

        private void RedirectSaveFolder()
        {
            _previousSaveRoot = SaveManager.SaveRootOverride;
            _testSaveRoot = Path.Combine(Path.GetTempPath(), "RapaduraSmokeSaves", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testSaveRoot);
            SaveManager.SaveRootOverride = _testSaveRoot;
        }

        private void RestoreSaveFolder()
        {
            SaveManager.SaveRootOverride = _previousSaveRoot;

            try
            {
                if (_testSaveRoot != null && Directory.Exists(_testSaveRoot))
                {
                    Directory.Delete(_testSaveRoot, recursive: true);
                }
            }
            catch (IOException e)
            {
                Debug.LogWarning($"[SmokeTestBase] Could not delete the temp save folder: {e.Message}");
            }

            _testSaveRoot = null;
        }
    }
}
