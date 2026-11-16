using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Calling a boss back early (the Velho Kul's silver lure, Docs/Mapas100.md 64: <see cref="Rapadura.Gameplay.World.Map.Mechanics.KulLureCall"/>).
    /// The boss keeps its arena and its return rule (<see cref="RegionBossReturns.CallNow"/>): a call only shortens the wait of one that was beaten.
    /// </summary>
    public sealed partial class RegionBossDirector
    {
        private static RegionBossDirector _found;

        private static RegionBossDirector Found()
        {
            if (_found == null)
            {
                _found = FindAnyObjectByType<RegionBossDirector>();
            }

            return _found;
        }

        private Slot SlotOf(string enemyId)
        {
            foreach (Slot slot in _slots)
            {
                if (slot.Spawn.EnemyId == enemyId)
                {
                    return slot;
                }
            }

            return null;
        }

        /// <summary>Is <paramref name="enemyId"/> beaten and waiting out its return (not in the world)?</summary>
        public static bool IsAway(string enemyId)
        {
            RegionBossDirector director = Found();
            Slot slot = director != null ? director.SlotOf(enemyId) : null;
            return slot != null && slot.Instance == null && slot.Returns.Waiting(Time.time);
        }

        /// <summary>Brings <paramref name="enemyId"/> back now if it is waiting; it still appears only with the hero near its arena.</summary>
        public static bool CallBack(string enemyId)
        {
            RegionBossDirector director = Found();
            Slot slot = director != null ? director.SlotOf(enemyId) : null;
            return slot != null && slot.Instance == null && slot.Returns.CallNow(Time.time);
        }
    }
}
