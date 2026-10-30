using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Interfaces;

namespace Rapadura.Gameplay.World.Chatter
{
    /// <summary>JSON-serializable snapshot of <see cref="RoadChatterState"/>.</summary>
    [Serializable]
    public class RoadChatterSaveState
    {
        public int tripsUntilEligible;
        public List<string> seenIds = new List<string>();

        /// <summary>Heard ids, oldest-heard first (for repeats once everything was heard). Older saves lack it.</summary>
        public List<string> heardOrder = new List<string>();
    }

    /// <summary>
    /// Per-save state of "Conversas entre primos na estrada": which exchanges were already heard and in what order (a
    /// "Nova Jornada+" keeps them and adds its own; once every eligible one was heard they repeat, oldest-heard first) and the trip cooldown. It must outlive the WorldMap
    /// scene, which reloads after every fight, so it is a GameManager-built manager registered with SaveManager under
    /// <see cref="Key"/> and with NewSessionReset, like <c>WorldEventState</c>; a save written before the feature loads
    /// a fresh state.
    /// </summary>
    public class RoadChatterState : IManager, ISaveable, IResettable
    {
        public const string Key = "RoadChatter";

        private readonly HashSet<string> _seen = new HashSet<string>();

        // Oldest-heard first; an id moves to the end each time it is heard again.
        private readonly List<string> _heardOrder = new List<string>();

        private static RoadChatterState _standalone;

        public string SaveKey => Key;

        /// <summary>Trips left before a conversation may happen again.</summary>
        public int TripsUntilEligible { get; set; }

        public IReadOnlyCollection<string> SeenIds => _seen;

        /// <summary>The GameManager's state (ServiceLocator); without one, a process-wide stand-alone state (not saved).</summary>
        public static RoadChatterState ResolveShared()
        {
            if (ServiceLocator.TryGet(out RoadChatterState state))
            {
                return state;
            }

            return _standalone ??= new RoadChatterState();
        }

        public void Initialize()
        {
        }

        public void Shutdown()
        {
        }

        public void ResetForNewSession()
        {
            TripsUntilEligible = 0;
            _seen.Clear();
            _heardOrder.Clear();
        }

        public bool HasSeen(string id) => id != null && _seen.Contains(id);

        /// <summary>Ids in the order they were (last) heard, oldest first.</summary>
        public IReadOnlyList<string> HeardOrder => _heardOrder;

        /// <summary>How long ago <paramref name="id"/> was last heard: 0 = the oldest; -1 = never heard.</summary>
        public int HeardRank(string id) => id != null ? _heardOrder.IndexOf(id) : -1;

        /// <summary>Marks <paramref name="id"/> as heard (again): it becomes the most recently heard.</summary>
        public void MarkSeen(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _seen.Add(id);
                _heardOrder.Remove(id);
                _heardOrder.Add(id);
            }
        }

        public object CaptureState()
        {
            var state = new RoadChatterSaveState { tripsUntilEligible = TripsUntilEligible };
            state.seenIds.AddRange(_seen);
            state.seenIds.Sort(StringComparer.Ordinal);
            state.heardOrder.AddRange(_heardOrder);
            return state;
        }

        public void RestoreState(object state)
        {
            if (!(state is RoadChatterSaveState saved))
            {
                return;
            }

            ResetForNewSession();
            TripsUntilEligible = Math.Max(0, saved.tripsUntilEligible);
            if (saved.seenIds == null)
            {
                return;
            }

            foreach (string id in saved.seenIds)
            {
                MarkSeen(id);
            }

            // Replaying the saved order moves those ids to the end; seen ids missing from it (older saves) count as oldest.
            if (saved.heardOrder == null)
            {
                return;
            }

            foreach (string id in saved.heardOrder)
            {
                if (HasSeen(id))
                {
                    MarkSeen(id);
                }
            }
        }
    }
}
