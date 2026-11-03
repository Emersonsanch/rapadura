using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Dungeon
{
    /// <summary>JSON-serializable snapshot of <see cref="DungeonProgress"/>.</summary>
    [Serializable]
    public class DungeonSaveState
    {
        public List<string> openGates = new List<string>();
        public List<string> openedSarcophagi = new List<string>();
        public List<string> brokenWalls = new List<string>();
        public List<string> takenChests = new List<string>();
        public int runs;
        public int clears;
    }

    /// <summary>Raised when the Colosso falls and the ossuary is cleared (the dungeon then re-seals for the next visit).</summary>
    public readonly struct DungeonClearedEvent : IGameEvent
    {
        public readonly string DungeonId;
        public readonly int Clears;

        public DungeonClearedEvent(string dungeonId, int clears)
        {
            DungeonId = dungeonId;
            Clears = clears;
        }
    }

    /// <summary>
    /// Per-save state of the Ossuário dos Artífices: which barred gates are open, which sarcophagi were opened, whether the
    /// bone wall is down and the niche's chest taken, plus visits and clears. Plain C# (EditMode-testable), built by
    /// GameManager, registered with SaveManager under <see cref="Key"/> and with NewSessionReset; GameManager survives the
    /// WorldMap ⇄ Main hop, so a gate opened on one visit stays open on the next (no key asked twice).
    ///
    /// Beating the Colosso (<see cref="RecordClear"/>) re-seals the ossuary: gates close, sarcophagi refill and the wall is
    /// mended, so the next visit is a fresh run (keys again, loot again) — MU/Tibia-style repeatable content. Until then
    /// every sarcophagus opens once per save. A save from before the dungeon has no entry and loads with everything sealed.
    /// </summary>
    public class DungeonProgress : IManager, ISaveable, IResettable
    {
        public const string Key = "Dungeons";

        private readonly HashSet<string> _openGates = new HashSet<string>();
        private readonly HashSet<string> _openedSarcophagi = new HashSet<string>();
        private readonly HashSet<string> _brokenWalls = new HashSet<string>();
        private readonly HashSet<string> _takenChests = new HashSet<string>();
        private static DungeonProgress _standalone;

        public string SaveKey => Key;
        public int Runs { get; private set; }
        public int Clears { get; private set; }

        public static DungeonProgress ResolveShared()
        {
            if (ServiceLocator.TryGet(out DungeonProgress progress))
            {
                return progress;
            }

            return _standalone ??= new DungeonProgress();
        }

        public void Initialize()
        {
            if (ServiceLocator.TryGet(out LocalizationManager localization))
            {
                DungeonTexts.EnsureRegistered(localization);
            }
        }

        public void Shutdown()
        {
        }

        public void ResetForNewSession()
        {
            ClearRunState();
            Runs = 0;
            Clears = 0;
        }

        private void ClearRunState()
        {
            _openGates.Clear();
            _openedSarcophagi.Clear();
            _brokenWalls.Clear();
            _takenChests.Clear();
        }

        // ------------------------------------------------------------------
        // Gates
        // ------------------------------------------------------------------

        public bool IsGateOpen(string gateId) => !string.IsNullOrEmpty(gateId) && _openGates.Contains(gateId);

        /// <summary>True when it was closed until now.</summary>
        public bool MarkGateOpen(string gateId) => !string.IsNullOrEmpty(gateId) && _openGates.Add(gateId);

        // ------------------------------------------------------------------
        // Sarcophagi, wall, chest
        // ------------------------------------------------------------------

        public bool IsSarcophagusOpened(string id) => !string.IsNullOrEmpty(id) && _openedSarcophagi.Contains(id);

        /// <summary>True only the first time (a sarcophagus opens once per save until the ossuary re-seals).</summary>
        public bool MarkSarcophagusOpened(string id) => !string.IsNullOrEmpty(id) && _openedSarcophagi.Add(id);

        public bool IsWallBroken(string id) => !string.IsNullOrEmpty(id) && _brokenWalls.Contains(id);

        public bool MarkWallBroken(string id) => !string.IsNullOrEmpty(id) && _brokenWalls.Add(id);

        public bool IsChestTaken(string id) => !string.IsNullOrEmpty(id) && _takenChests.Contains(id);

        public bool MarkChestTaken(string id) => !string.IsNullOrEmpty(id) && _takenChests.Add(id);

        // ------------------------------------------------------------------
        // Visits
        // ------------------------------------------------------------------

        public void RecordVisit()
        {
            Runs++;
        }

        /// <summary>The Colosso fell: one more clear, and the ossuary re-seals for the next visit.</summary>
        public void RecordClear(string dungeonId = DungeonRules.OssuaryId)
        {
            Clears++;
            ClearRunState();
            EventBus.Publish(new DungeonClearedEvent(dungeonId, Clears));
        }

        // ------------------------------------------------------------------
        // Save
        // ------------------------------------------------------------------

        public object CaptureState()
        {
            var state = new DungeonSaveState { runs = Runs, clears = Clears };
            state.openGates.AddRange(Sorted(_openGates));
            state.openedSarcophagi.AddRange(Sorted(_openedSarcophagi));
            state.brokenWalls.AddRange(Sorted(_brokenWalls));
            state.takenChests.AddRange(Sorted(_takenChests));
            return state;
        }

        public void RestoreState(object state)
        {
            if (state is not DungeonSaveState saved)
            {
                return;
            }

            ResetForNewSession();
            Runs = Math.Max(0, saved.runs);
            Clears = Math.Max(0, saved.clears);
            AddAll(_openGates, saved.openGates);
            AddAll(_openedSarcophagi, saved.openedSarcophagi);
            AddAll(_brokenWalls, saved.brokenWalls);
            AddAll(_takenChests, saved.takenChests);
        }

        private static List<string> Sorted(HashSet<string> set)
        {
            var list = new List<string>(set);
            list.Sort(string.CompareOrdinal);
            return list;
        }

        private static void AddAll(HashSet<string> set, List<string> values)
        {
            if (values == null)
            {
                return;
            }

            foreach (string value in values)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    set.Add(value);
                }
            }
        }
    }
}
