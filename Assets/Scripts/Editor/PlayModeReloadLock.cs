using UnityEditor;

namespace Rapadura.Editor
{
    /// <summary>
    /// Holds script recompiles and domain reloads back while the Editor is in Play Mode, so scripts saved meanwhile (by hand or by
    /// tools editing the project) compile once Play Mode ends instead of reloading the domain mid-run: a reload in Play Mode wipes
    /// non-serialized state (a click-to-move destination, a coroutine's wait) and made the PlayMode smoke tests fail at random.
    /// </summary>
    [InitializeOnLoad]
    public static class PlayModeReloadLock
    {
        private static bool _locked;

        static PlayModeReloadLock()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            switch (change)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    if (!_locked)
                    {
                        EditorApplication.LockReloadAssemblies();
                        _locked = true;
                    }

                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    if (_locked)
                    {
                        EditorApplication.UnlockReloadAssemblies();
                        _locked = false;
                    }

                    break;
            }
        }
    }
}
