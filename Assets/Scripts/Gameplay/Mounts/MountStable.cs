using System.Linq;
using Rapadura.Gameplay.Housing;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>What the house offers the mounts right now (Docs/Mounts.md §7).</summary>
    public readonly struct MountStableInfo
    {
        public readonly bool HasHouse;
        public readonly int HouseLevel;
        public readonly int Stalls;
        public readonly bool FeedsPerFight;
        public readonly bool WatersDaily;

        public MountStableInfo(bool hasHouse, int houseLevel, int stalls, bool feedsPerFight, bool watersDaily)
        {
            HasHouse = hasHouse;
            HouseLevel = houseLevel;
            Stalls = stalls;
            FeedsPerFight = feedsPerFight;
            WatersDaily = watersDaily;
        }
    }

    /// <summary>
    /// The stable hook into the house (Fase 15): read-only, through <see cref="HouseService"/>'s public API, so the
    /// housing code needs no change. Stalls follow the house level (<see cref="MountCareRules.StallsFor"/>); the corral
    /// items of the yard work as §7 says (Curral de aroeira +1 stall, Cocho/Manjedoura +10 satiety per fight, Bebedouro
    /// +1 affection a day). Offline, the mounts in stalls are the first owned ones after the active mount; the rest wait at
    /// the Haras (free). <see cref="Override"/> lets tests (or a future stable module) answer instead.
    /// </summary>
    public static class MountStable
    {
        /// <summary>Optional replacement of the house lookup (tests, or the housing module once it has a real stable).</summary>
        public static System.Func<MountStableInfo> Override { get; set; }

        public static MountStableInfo Current()
        {
            if (Override != null)
            {
                return Override();
            }

            HouseService house = HouseService.ResolveShared();
            if (house == null || !house.HasHouse)
            {
                return new MountStableInfo(false, 0, 0, false, false);
            }

            var placed = house.Layout != null ? house.Layout.Placed.Select(p => p.itemId).ToList() : new System.Collections.Generic.List<string>();
            bool corral = placed.Contains(MountCareRules.StableYardCorral);
            bool feeds = placed.Contains(MountCareRules.StableTrough) || placed.Contains(MountCareRules.StableManger);
            bool waters = placed.Contains(MountCareRules.StableDrinker);
            return new MountStableInfo(true, house.Level, MountCareRules.StallsFor(house.Level, true, corral), feeds, waters);
        }
    }
}
