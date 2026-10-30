using Rapadura.Gameplay.Seasons;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// Debug toggle of the "Sistema de temporadas" (Docs/Seasons.md): force the Festa Junina outside June
    /// (<see cref="SeasonCalendar.DebugForceFestaJunina"/>). Kept in EditorPrefs and re-applied after every domain
    /// reload (so it holds when entering Play), plus a play-mode helper that hands out prendas. Never used by the game.
    /// </summary>
    public static class SeasonDebugMenu
    {
        public const string ForcePrefKey = "Rapadura.Debug.ForceFestaJunina";
        private const string ForceMenu = "Rapadura/Debug/Festa Junina: forçar (fora de junho)";

        [InitializeOnLoadMethod]
        private static void Apply()
        {
            SeasonCalendar.DebugForceFestaJunina = EditorPrefs.GetBool(ForcePrefKey, false);
        }

        [MenuItem(ForceMenu)]
        private static void ToggleForce()
        {
            bool value = !EditorPrefs.GetBool(ForcePrefKey, false);
            EditorPrefs.SetBool(ForcePrefKey, value);
            SeasonCalendar.DebugForceFestaJunina = value;
            Debug.Log("[SeasonDebugMenu] Festa Junina forced: " + value);
        }

        [MenuItem(ForceMenu, true)]
        private static bool ToggleForceValidate()
        {
            Menu.SetChecked(ForceMenu, EditorPrefs.GetBool(ForcePrefKey, false));
            return true;
        }

        [MenuItem("Rapadura/Debug/Festa Junina: +50 prendas (play mode)")]
        private static void GivePrendas()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[SeasonDebugMenu] Enter play mode first.");
                return;
            }

            FestaJuninaProgress progress = FestaJuninaProgress.ResolveShared();
            progress.Earn(50);
            Debug.Log("[SeasonDebugMenu] Prendas: " + progress.Prendas);
        }
    }
}
