using Rapadura.Core.DI;
using Rapadura.Gameplay.Housing;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// Play-mode helpers to look at H4/H6 without playing the whole campaign: hand the house the display pieces (trophy
    /// wall, relic showcase, Caderno shelf, Caramelo's bed, watering can, training dummy, plants), go up a level for free
    /// (same path as an upgrade: nothing lost). Never used by the game.
    /// </summary>
    public static class HouseDebugMenu
    {
        private static readonly string[] LifeItems =
        {
            "casa_util_022", "casa_util_021", "casa_util_020", "casa_util_019", "casa_curral_002", "casa_curral_003",
            "casa_quintal_027", "casa_quintal_037", "casa_planta_001", "casa_planta_002", "casa_planta_003", "casa_quintal_002",
        };

        private static HouseService House => ServiceLocator.TryGet(out HouseService house) ? house : null;

        [MenuItem("Rapadura/Housing/Debug: Give life items (play mode)")]
        private static void GiveLifeItems()
        {
            HouseService house = House;
            if (house == null || !house.HasHouse)
            {
                Debug.LogWarning("[HouseDebugMenu] Enter play mode with a save that already has the house.");
                return;
            }

            foreach (string id in LifeItems) house.Layout.GrantToDepot(id);
            Debug.Log("[HouseDebugMenu] Life items are in the Depósito: " + string.Join(", ", LifeItems));
        }

        [MenuItem("Rapadura/Housing/Debug: Next house level for free (play mode)")]
        private static void NextLevel()
        {
            HouseService house = House;
            if (house == null || !house.HasHouse || house.Level >= HouseLevels.MaxLevel)
            {
                Debug.LogWarning("[HouseDebugMenu] No house, or already a casarão.");
                return;
            }

            int moved = house.ApplyLevel(house.Level + 1);
            house.CheckAutoQuests();
            Debug.Log($"[HouseDebugMenu] House is now N{house.Level}; {moved} piece(s) went to the Depósito.");
        }

        [MenuItem("Rapadura/Housing/Debug: Next house level for free (play mode)", true)]
        [MenuItem("Rapadura/Housing/Debug: Give life items (play mode)", true)]
        private static bool PlayingOnly() => Application.isPlaying;
    }
}
