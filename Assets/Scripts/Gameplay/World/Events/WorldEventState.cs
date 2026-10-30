using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.World.Events
{
    /// <summary>JSON-serializable snapshot of <see cref="WorldEventState"/> (JsonUtility has no dictionaries, so seen counts are parallel lists).</summary>
    [Serializable]
    public class WorldEventSaveState
    {
        public int tripsUntilEligible;
        public int tripsWithoutEvent;
        public string lastEventId;
        public List<string> seenIds = new List<string>();
        public List<int> seenCounts = new List<int>();
        public int pendingExperience;
        public float pendingHealFraction;
        public List<StatModifierDefinition> pendingBuffs = new List<StatModifierDefinition>();
    }

    /// <summary>
    /// Per-save state of the world map's road events: the trip cooldown and "pity" counter of
    /// <see cref="WorldEventDirector"/> (they must outlive the WorldMap scene, which reloads after every fight),
    /// the last event (no repeats in a row), how often each event was seen, and the boons waiting for the next
    /// fight — the map has no live Player, so XP, heals and buffs from an event land when Main.unity's hero
    /// exists (<c>WorldEventArenaHooks</c>). A GameManager-built manager registered with SaveManager under
    /// <see cref="Key"/> and with NewSessionReset; a save written before events existed loads a fresh state.
    ///
    /// <see cref="PendingEliteSeed"/> (the ambush being fought) is deliberately not saved: an encounter is never
    /// resumed from a save either.
    /// </summary>
    public class WorldEventState : IManager, ISaveable, IResettable
    {
        public const string Key = "WorldEvents";

        /// <summary>BuffController source of every event buff (one fight long: the Player dies with Main.unity).</summary>
        public const string BuffSource = "world_event";

        private readonly Dictionary<string, int> _seen = new Dictionary<string, int>();
        private readonly List<StatModifierDefinition> _pendingBuffs = new List<StatModifierDefinition>();

        private static WorldEventState _standalone;

        public string SaveKey => Key;

        /// <summary>Trips left before an event may happen again (the cooldown).</summary>
        public int TripsUntilEligible { get; set; }

        /// <summary>Eligible trips in a row that rolled no event (raises the next trip's chance).</summary>
        public int TripsWithoutEvent { get; set; }

        public string LastEventId { get; private set; }

        public int PendingExperience { get; private set; }
        public float PendingHealFraction { get; private set; }
        public IReadOnlyList<StatModifierDefinition> PendingBuffs => _pendingBuffs;
        public bool HasPendingBoons => PendingExperience > 0 || PendingHealFraction > 0f || _pendingBuffs.Count > 0;

        /// <summary>Seed of the elite ambush payload handed to the arena (null when none is being fought). Not saved.</summary>
        public int? PendingEliteSeed { get; set; }

        /// <summary>
        /// The GameManager's state (ServiceLocator); without one (a scene played on its own) a process-wide
        /// stand-alone state, which is not saved.
        /// </summary>
        public static WorldEventState ResolveShared()
        {
            if (ServiceLocator.TryGet(out WorldEventState state))
            {
                return state;
            }

            return _standalone ??= new WorldEventState();
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
            TripsWithoutEvent = 0;
            LastEventId = null;
            _seen.Clear();
            ClearPendingBoons();
            PendingEliteSeed = null;
        }

        public int TimesSeen(string eventId) => eventId != null && _seen.TryGetValue(eventId, out int count) ? count : 0;

        /// <summary>Records that <paramref name="eventId"/> popped (last event + seen count).</summary>
        public void RecordSeen(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return;
            }

            LastEventId = eventId;
            _seen[eventId] = TimesSeen(eventId) + 1;
        }

        public void AddPendingExperience(int amount)
        {
            PendingExperience += Mathf.Max(0, amount);
        }

        /// <summary>Heals stack up to a full heal.</summary>
        public void AddPendingHeal(float fraction)
        {
            PendingHealFraction = Mathf.Clamp01(PendingHealFraction + Mathf.Max(0f, fraction));
        }

        /// <summary>
        /// Queues a buff for the next fight. Same stat + application as one already queued replaces it (the
        /// BuffController would replace it anyway, same source), so two campfires don't stack.
        /// </summary>
        public void AddPendingBuff(StatType stat, ModifierApplication application, float value)
        {
            _pendingBuffs.RemoveAll(b => b.affectedStat == stat && b.application == application);
            _pendingBuffs.Add(new StatModifierDefinition
            {
                displayName = BuffSource,
                affectedStat = stat,
                application = application,
                value = value,
                durationSeconds = 0f,
                isDebuff = false,
            });
        }

        public void ClearPendingBoons()
        {
            PendingExperience = 0;
            PendingHealFraction = 0f;
            _pendingBuffs.Clear();
        }

        public object CaptureState()
        {
            var state = new WorldEventSaveState
            {
                tripsUntilEligible = TripsUntilEligible,
                tripsWithoutEvent = TripsWithoutEvent,
                lastEventId = LastEventId,
                pendingExperience = PendingExperience,
                pendingHealFraction = PendingHealFraction,
            };

            foreach (KeyValuePair<string, int> pair in _seen)
            {
                state.seenIds.Add(pair.Key);
                state.seenCounts.Add(pair.Value);
            }

            foreach (StatModifierDefinition buff in _pendingBuffs)
            {
                state.pendingBuffs.Add(buff);
            }

            return state;
        }

        public void RestoreState(object state)
        {
            if (!(state is WorldEventSaveState saved))
            {
                return;
            }

            ResetForNewSession();
            TripsUntilEligible = Mathf.Max(0, saved.tripsUntilEligible);
            TripsWithoutEvent = Mathf.Max(0, saved.tripsWithoutEvent);
            LastEventId = string.IsNullOrEmpty(saved.lastEventId) ? null : saved.lastEventId;
            PendingExperience = Mathf.Max(0, saved.pendingExperience);
            PendingHealFraction = Mathf.Clamp01(saved.pendingHealFraction);

            if (saved.seenIds != null && saved.seenCounts != null)
            {
                int count = Math.Min(saved.seenIds.Count, saved.seenCounts.Count);
                for (int i = 0; i < count; i++)
                {
                    if (!string.IsNullOrEmpty(saved.seenIds[i]))
                    {
                        _seen[saved.seenIds[i]] = Math.Max(0, saved.seenCounts[i]);
                    }
                }
            }

            if (saved.pendingBuffs != null)
            {
                foreach (StatModifierDefinition buff in saved.pendingBuffs)
                {
                    if (buff != null)
                    {
                        AddPendingBuff(buff.affectedStat, buff.application, buff.value);
                    }
                }
            }
        }
    }
}
