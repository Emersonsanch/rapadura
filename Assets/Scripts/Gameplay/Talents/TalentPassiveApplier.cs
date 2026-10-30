using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.Talents
{
    /// <summary>
    /// Re-applies the playing cousin's trained levels and wheel (<see cref="TalentProgress.ApplyToPlayer"/>) two frames after
    /// every scene load, after GameManager's carried-state restore (same timing as <c>RelicPassiveApplier</c>). Character
    /// selection and save loads are handled by <see cref="TalentProgress"/> itself. Self-bootstrapping, no scene wiring.
    /// </summary>
    public class TalentPassiveApplier : MonoBehaviour
    {
        private static TalentPassiveApplier _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null)
            {
                return;
            }

            var host = new GameObject(nameof(TalentPassiveApplier));
            DontDestroyOnLoad(host);
            _instance = host.AddComponent<TalentPassiveApplier>();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            StartCoroutine(ApplyAfterSceneStart());
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => StartCoroutine(ApplyAfterSceneStart());

        private static IEnumerator ApplyAfterSceneStart()
        {
            yield return null;
            yield return null;
            TalentProgress.ResolveShared().ApplyToPlayer();
        }
    }
}
