using System;
using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.World.Map
{
    /// <summary>JSON-serializable snapshot of <see cref="WorldMapProgress"/> (JsonUtility has no dictionaries, so victories are parallel lists).</summary>
    [Serializable]
    public class WorldMapSaveState
    {
        public int currentRegion;
        public List<int> unlockedRegions = new List<int>();
        public List<int> victoryRegions = new List<int>();
        public List<int> victoryCounts = new List<int>();
    }

    /// <summary>
    /// Persistent world map state: which region the player is in, which regions are unlocked and
    /// how many encounters were won per region (drives <see cref="WorldMapGraph"/> unlock rules).
    /// A top-level manager built by GameManager (so it outlives WorldMap/Main scene swaps) and
    /// registered with SaveManager as an <see cref="ISaveable"/> under <see cref="SaveKey"/>.
    /// </summary>
    public class WorldMapProgress : IManager, ISaveable
    {
        private readonly BiomeType _startingRegion;
        private readonly HashSet<BiomeType> _unlocked = new HashSet<BiomeType>();
        private readonly Dictionary<BiomeType, int> _victories = new Dictionary<BiomeType, int>();

        public string SaveKey => "WorldMap";

        public BiomeType StartingRegion => _startingRegion;
        public BiomeType CurrentRegion { get; private set; }
        public IReadOnlyCollection<BiomeType> UnlockedRegions => _unlocked;

        public WorldMapProgress(BiomeType startingRegion = BiomeType.Forest)
        {
            _startingRegion = startingRegion;
            ResetToNewGame();
        }

        public void Initialize()
        {
        }

        public void Shutdown()
        {
        }

        /// <summary>Back to a fresh game: only the starting region unlocked, standing on it, no victories.</summary>
        public void ResetToNewGame()
        {
            _unlocked.Clear();
            _victories.Clear();
            _unlocked.Add(_startingRegion);
            CurrentRegion = _startingRegion;
        }

        public bool IsUnlocked(BiomeType region) => _unlocked.Contains(region);

        public int GetVictories(BiomeType region) => _victories.TryGetValue(region, out int count) ? count : 0;

        /// <summary>Unlocks a region. Returns true (and publishes <see cref="WorldMapRegionUnlockedEvent"/>) only if it was locked before.</summary>
        public bool Unlock(BiomeType region)
        {
            if (!_unlocked.Add(region))
            {
                return false;
            }

            EventBus.Publish(new WorldMapRegionUnlockedEvent(region));
            return true;
        }

        /// <summary>Moves the player to an unlocked region. Returns false (no change) for a locked region; publishes <see cref="WorldMapRegionChangedEvent"/> on an actual change.</summary>
        public bool SetCurrentRegion(BiomeType region)
        {
            if (!IsUnlocked(region))
            {
                return false;
            }

            if (region == CurrentRegion)
            {
                return true;
            }

            BiomeType previous = CurrentRegion;
            CurrentRegion = region;
            EventBus.Publish(new WorldMapRegionChangedEvent(previous, region));
            return true;
        }

        public void RecordVictory(BiomeType region)
        {
            _victories[region] = GetVictories(region) + 1;
        }

        public object CaptureState()
        {
            var state = new WorldMapSaveState { currentRegion = (int)CurrentRegion };

            foreach (BiomeType region in _unlocked)
            {
                state.unlockedRegions.Add((int)region);
            }

            foreach (KeyValuePair<BiomeType, int> pair in _victories)
            {
                state.victoryRegions.Add((int)pair.Key);
                state.victoryCounts.Add(pair.Value);
            }

            return state;
        }

        public void RestoreState(object state)
        {
            if (!(state is WorldMapSaveState saved))
            {
                return;
            }

            _unlocked.Clear();
            _victories.Clear();

            // The starting region is always reachable, even if an old/hand-edited save omits it.
            _unlocked.Add(_startingRegion);

            if (saved.unlockedRegions != null)
            {
                foreach (int region in saved.unlockedRegions)
                {
                    if (Enum.IsDefined(typeof(BiomeType), region))
                    {
                        _unlocked.Add((BiomeType)region);
                    }
                }
            }

            if (saved.victoryRegions != null && saved.victoryCounts != null)
            {
                int count = Math.Min(saved.victoryRegions.Count, saved.victoryCounts.Count);
                for (int i = 0; i < count; i++)
                {
                    int region = saved.victoryRegions[i];
                    if (Enum.IsDefined(typeof(BiomeType), region))
                    {
                        _victories[(BiomeType)region] = Math.Max(0, saved.victoryCounts[i]);
                    }
                }
            }

            BiomeType current = (BiomeType)saved.currentRegion;
            CurrentRegion = Enum.IsDefined(typeof(BiomeType), saved.currentRegion) && _unlocked.Contains(current)
                ? current
                : _startingRegion;
        }
    }
}
