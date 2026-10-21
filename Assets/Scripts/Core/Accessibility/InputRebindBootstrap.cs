using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Rapadura.Core.Accessibility
{
    /// <summary>
    /// Applies the player's saved rebinds (see <see cref="InputRebindManager.LoadBindingOverrides"/>)
    /// to every <see cref="PlayerInput"/>'s actions asset as soon as a scene finishes loading, so
    /// custom bindings work from the first frame even if the Settings screen is never opened.
    /// Self-registering via <see cref="RuntimeInitializeOnLoadMethodAttribute"/> — needs no scene
    /// wiring. Loading the same JSON twice onto an asset is idempotent, so re-running on every
    /// scene load (e.g. world -> combat transition spawning a new player) is safe.
    /// </summary>
    public static class InputRebindBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            ApplySavedOverrides();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplySavedOverrides();
        }

        /// <summary>Loads saved overrides onto each distinct actions asset used by an active <see cref="PlayerInput"/>.</summary>
        public static void ApplySavedOverrides()
        {
            var seen = new HashSet<InputActionAsset>();
            foreach (PlayerInput playerInput in PlayerInput.all)
            {
                InputActionAsset asset = playerInput != null ? playerInput.actions : null;
                if (asset != null && seen.Add(asset))
                {
                    new InputRebindManager(asset).LoadBindingOverrides();
                }
            }
        }
    }
}
