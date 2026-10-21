using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Audio
{
    /// <summary>
    /// Turns the stream of <c>PlayerExperienceChangedEvent.Level</c> values into "level went up" edges.
    /// The first value seen (scene start, load) only sets the baseline; <see cref="Reset"/> on load /
    /// new session so a restored save never counts as a level-up.
    /// </summary>
    public sealed class LevelUpDetector
    {
        private int _lastLevel = -1;

        public bool Observe(int level)
        {
            bool leveledUp = _lastLevel >= 0 && level > _lastLevel;
            _lastLevel = level;
            return leveledUp;
        }

        public void Reset() => _lastLevel = -1;
    }

    /// <summary>Small shared lookups for the audio/haptics routers.</summary>
    public static class AudioEventHelpers
    {
        public const string PlayerTag = "Player";

        public static bool IsPlayer(GameObject go)
        {
            return go != null && (go.CompareTag(PlayerTag) || go.GetComponentInParent<PlayerStats>() != null);
        }

        private static InventoryManager _cachedInventory;

        /// <summary>Total item units in the player's bag (0 when no player is spawned). Used to tell pickups from other inventory changes.</summary>
        public static int CountPlayerItems()
        {
            if (_cachedInventory == null)
            {
                GameObject player = GameObject.FindWithTag(PlayerTag);
                _cachedInventory = player != null ? player.GetComponentInParent<InventoryManager>() : null;
                if (_cachedInventory == null)
                {
                    return 0;
                }
            }

            int total = 0;
            var slots = _cachedInventory.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && !string.IsNullOrEmpty(slots[i].itemId))
                {
                    total += slots[i].quantity;
                }
            }

            return total;
        }
    }
}
