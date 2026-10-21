using System.Collections.Generic;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.World.Map
{
    /// <summary>
    /// Plain C# description of the overworld's layout: which regions connect to which (travel is
    /// only allowed along a connection) and what unlocks each region (N victories in another
    /// region, plus — on the map — that region's boss beaten, see <see cref="RegionBossProgress"/>). <see cref="WorldMapController"/> builds one from the scene's
    /// <see cref="WorldMapRegionNode"/>s; <see cref="CreateDefault"/> mirrors the intended
    /// progression for tests and as a fallback when a scene has no nodes wired.
    /// </summary>
    public class WorldMapGraph
    {
        private readonly struct UnlockRule
        {
            public readonly BiomeType RequiredRegion;
            public readonly int VictoriesRequired;

            public UnlockRule(BiomeType requiredRegion, int victoriesRequired)
            {
                RequiredRegion = requiredRegion;
                VictoriesRequired = victoriesRequired;
            }
        }

        private readonly Dictionary<BiomeType, HashSet<BiomeType>> _connections = new Dictionary<BiomeType, HashSet<BiomeType>>();
        private readonly Dictionary<BiomeType, UnlockRule> _unlockRules = new Dictionary<BiomeType, UnlockRule>();

        /// <summary>Victories in the previous region needed to open the next one in the default progression.</summary>
        public const int DefaultVictoriesToUnlock = 3;

        /// <summary>Linear default progression Floresta → Deserto → Montanha → Cavernas, each gated by victories in the previous one.</summary>
        public static WorldMapGraph CreateDefault()
        {
            var graph = new WorldMapGraph();
            graph.AddConnection(BiomeType.Forest, BiomeType.Desert);
            graph.AddConnection(BiomeType.Desert, BiomeType.Mountain);
            graph.AddConnection(BiomeType.Mountain, BiomeType.Caves);
            graph.SetUnlockRule(BiomeType.Desert, BiomeType.Forest, DefaultVictoriesToUnlock);
            graph.SetUnlockRule(BiomeType.Mountain, BiomeType.Desert, DefaultVictoriesToUnlock);
            graph.SetUnlockRule(BiomeType.Caves, BiomeType.Mountain, DefaultVictoriesToUnlock);
            return graph;
        }

        /// <summary>Adds a bidirectional connection. Self-connections are ignored.</summary>
        public void AddConnection(BiomeType a, BiomeType b)
        {
            if (a == b)
            {
                return;
            }

            GetOrCreate(a).Add(b);
            GetOrCreate(b).Add(a);
        }

        public bool AreConnected(BiomeType a, BiomeType b)
        {
            return _connections.TryGetValue(a, out HashSet<BiomeType> neighbours) && neighbours.Contains(b);
        }

        public IEnumerable<BiomeType> GetNeighbours(BiomeType region)
        {
            return _connections.TryGetValue(region, out HashSet<BiomeType> neighbours)
                ? neighbours
                : (IEnumerable<BiomeType>)System.Array.Empty<BiomeType>();
        }

        /// <summary>
        /// <paramref name="region"/> unlocks once the player has won <paramref name="victoriesRequired"/>
        /// encounters in <paramref name="requiredRegion"/>. A region with no rule never auto-unlocks
        /// (it must be unlocked explicitly, e.g. the starting region or a quest reward).
        /// </summary>
        public void SetUnlockRule(BiomeType region, BiomeType requiredRegion, int victoriesRequired)
        {
            _unlockRules[region] = new UnlockRule(requiredRegion, victoriesRequired < 0 ? 0 : victoriesRequired);
        }

        /// <summary>Reads <paramref name="region"/>'s unlock rule (see <see cref="SetUnlockRule"/>). False when it has none.</summary>
        public bool TryGetUnlockRule(BiomeType region, out BiomeType requiredRegion, out int victoriesRequired)
        {
            if (_unlockRules.TryGetValue(region, out UnlockRule rule))
            {
                requiredRegion = rule.RequiredRegion;
                victoriesRequired = rule.VictoriesRequired;
                return true;
            }

            requiredRegion = default;
            victoriesRequired = 0;
            return false;
        }

        /// <summary>
        /// Victories in <paramref name="region"/> after which its boss challenge is offered: the lowest
        /// threshold among the rules gated by <paramref name="region"/> (the same N that used to open
        /// the next region on its own), or <see cref="DefaultVictoriesToUnlock"/> for a region that
        /// gates nothing (the last one, Cavernas).
        /// </summary>
        public int GetVictoriesToChallengeBoss(BiomeType region)
        {
            int best = -1;
            foreach (UnlockRule rule in _unlockRules.Values)
            {
                if (rule.RequiredRegion == region && (best < 0 || rule.VictoriesRequired < best))
                {
                    best = rule.VictoriesRequired;
                }
            }

            return best < 0 ? DefaultVictoriesToUnlock : best;
        }

        /// <summary>Regions whose unlock rule depends on <paramref name="region"/> (what beating its boss opens).</summary>
        public List<BiomeType> GetRegionsUnlockedBy(BiomeType region)
        {
            var result = new List<BiomeType>();
            foreach (KeyValuePair<BiomeType, UnlockRule> pair in _unlockRules)
            {
                if (pair.Value.RequiredRegion == region)
                {
                    result.Add(pair.Key);
                }
            }

            return result;
        }

        /// <summary>Unlocks every still-locked region whose rule is now satisfied (victories only). Returns the regions newly unlocked by this call.</summary>
        public List<BiomeType> ApplyUnlocks(WorldMapProgress progress)
        {
            return ApplyUnlocks(progress, null);
        }

        /// <summary>
        /// Like <see cref="ApplyUnlocks(WorldMapProgress)"/>, but a rule also needs
        /// <paramref name="requiredRegionDone"/> to accept its required region — the world map passes
        /// <see cref="RegionBossProgress.IsUnlockGateSatisfied"/>, so the next region opens only after
        /// the previous region's boss falls. Null = victories only. Already-unlocked regions are never
        /// re-locked, so saves from before the boss gate keep everything they had opened.
        /// </summary>
        public List<BiomeType> ApplyUnlocks(WorldMapProgress progress, System.Predicate<BiomeType> requiredRegionDone)
        {
            var newlyUnlocked = new List<BiomeType>();
            if (progress == null)
            {
                return newlyUnlocked;
            }

            // Repeat until stable so a rule chain satisfied all at once (e.g. a 0-victory rule
            // behind a region that just opened) resolves in a single call.
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (KeyValuePair<BiomeType, UnlockRule> pair in _unlockRules)
                {
                    if (progress.IsUnlocked(pair.Key))
                    {
                        continue;
                    }

                    UnlockRule rule = pair.Value;
                    if (progress.IsUnlocked(rule.RequiredRegion)
                        && progress.GetVictories(rule.RequiredRegion) >= rule.VictoriesRequired
                        && (requiredRegionDone == null || requiredRegionDone(rule.RequiredRegion)))
                    {
                        progress.Unlock(pair.Key);
                        newlyUnlocked.Add(pair.Key);
                        changed = true;
                    }
                }
            }

            return newlyUnlocked;
        }

        /// <summary>Travel is allowed only to an unlocked region directly connected to the current one.</summary>
        public bool CanTravel(WorldMapProgress progress, BiomeType destination)
        {
            return progress != null
                   && destination != progress.CurrentRegion
                   && progress.IsUnlocked(destination)
                   && AreConnected(progress.CurrentRegion, destination);
        }

        private HashSet<BiomeType> GetOrCreate(BiomeType region)
        {
            if (!_connections.TryGetValue(region, out HashSet<BiomeType> set))
            {
                set = new HashSet<BiomeType>();
                _connections[region] = set;
            }

            return set;
        }
    }
}
