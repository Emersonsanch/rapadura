using System;
using System.Collections.Generic;
using System.Linq;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>When a placed plant was last watered (Housing.md §12.2 <c>plants[]</c>), in in-game days.</summary>
    [Serializable]
    public class HousePlantTimer
    {
        public int instanceId;
        public int lastWateredDay;

        public HousePlantTimer()
        {
        }

        public HousePlantTimer(int instanceId, int lastWateredDay)
        {
            this.instanceId = instanceId;
            this.lastWateredDay = lastWateredDay;
        }
    }

    public enum HousePlantMood
    {
        /// <summary>Watered recently.</summary>
        Happy,
        /// <summary>The last happy day: it asks for water, still full comfort.</summary>
        Thirsty,
        /// <summary>Drooping: half comfort until watered. Never dies, never disappears.</summary>
        Wilted,
    }

    /// <summary>
    /// "Plantas que pedem água" (Housing.md §11.9): happy up to <see cref="HappyDays"/> in-game days without water,
    /// then they wilt (droop, half comfort) — gently, they never die nor vanish. One watering brings everything back at
    /// once; rain waters the yard. Unity-free.
    /// </summary>
    public static class HousePlantRules
    {
        /// <summary>"Felizes até 3 dias sem água".</summary>
        public const int HappyDays = 3;

        public static bool IsPlant(HouseItemDefinition item) =>
            item != null && (item.Category == HouseItemCategory.Plant || item.Function == HouseItemFunction.Planter);

        public static HousePlantMood MoodOf(int lastWateredDay, int today)
        {
            int days = Math.Max(0, today - lastWateredDay);
            if (days > HappyDays) return HousePlantMood.Wilted;
            if (days == HappyDays) return HousePlantMood.Thirsty;
            return HousePlantMood.Happy;
        }

        /// <summary>
        /// Keeps one timer per placed plant: new plants start watered <paramref name="today"/>, plants put away lose
        /// their timer, unknown ids are dropped. Returns the synced list (input untouched).
        /// </summary>
        public static List<HousePlantTimer> Sync(IEnumerable<HousePlantTimer> timers, IEnumerable<HousePlacedData> placed,
            Func<string, HouseItemDefinition> resolve, int today)
        {
            var known = new Dictionary<int, int>();
            foreach (HousePlantTimer timer in timers ?? Enumerable.Empty<HousePlantTimer>())
            {
                if (timer != null && timer.instanceId > 0 && !known.ContainsKey(timer.instanceId)) known[timer.instanceId] = timer.lastWateredDay;
            }

            var result = new List<HousePlantTimer>();
            foreach (HousePlacedData piece in placed ?? Enumerable.Empty<HousePlacedData>())
            {
                if (piece == null || resolve == null || !IsPlant(resolve(piece.itemId))) continue;
                // A clock that went back (new game day counter) never leaves a plant "watered in the future".
                int last = known.TryGetValue(piece.instanceId, out int day) ? Math.Min(day, today) : today;
                result.Add(new HousePlantTimer(piece.instanceId, last));
            }

            return result;
        }

        /// <summary>Waters every plant (or only those <paramref name="filter"/> accepts). Returns how many were not happy.</summary>
        public static int Water(List<HousePlantTimer> timers, int today, Func<int, bool> filter = null)
        {
            int perked = 0;
            if (timers == null) return 0;
            foreach (HousePlantTimer timer in timers)
            {
                if (filter != null && !filter(timer.instanceId)) continue;
                if (MoodOf(timer.lastWateredDay, today) != HousePlantMood.Happy) perked++;
                timer.lastWateredDay = today;
            }

            return perked;
        }
    }
}
