using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Core.Audio
{
    /// <summary>
    /// The subset of combat happenings that can trigger a sound. Deliberately separate from
    /// <c>Rapadura.Gameplay.Combat.CombatEvents</c> types themselves, so this asset stays a simple
    /// enum-to-cue table instead of coupling audio data to combat event structs.
    /// </summary>
    public enum CombatAudioTrigger
    {
        DamageApplied = 0,
        CriticalHit = 1,
        TargetDied = 2,
    }

    /// <summary>
    /// Maps a <see cref="CombatAudioTrigger"/> to a cue id in an <see cref="AudioCueDatabase"/>.
    /// AudioManager subscribes to the real combat events (DamageAppliedEvent, CombatTargetDiedEvent)
    /// and uses this table to decide which cue id to play — combat code never references audio at all.
    /// </summary>
    [CreateAssetMenu(fileName = "CombatAudioCueMap", menuName = "Rapadura/Audio/Combat Audio Cue Map", order = 1)]
    public class CombatAudioCueMap : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public CombatAudioTrigger Trigger;
            public string CueId;
        }

        [SerializeField] private List<Entry> _entries = new List<Entry>();

        private Dictionary<CombatAudioTrigger, string> _lookup;

        private void BuildLookupIfNeeded()
        {
            if (_lookup != null)
            {
                return;
            }

            _lookup = new Dictionary<CombatAudioTrigger, string>();

            foreach (Entry entry in _entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.CueId))
                {
                    continue;
                }

                _lookup[entry.Trigger] = entry.CueId;
            }
        }

        public bool TryGetCueId(CombatAudioTrigger trigger, out string cueId)
        {
            BuildLookupIfNeeded();
            return _lookup.TryGetValue(trigger, out cueId);
        }

        /// <summary>Adds or replaces a mapping. Used by editor tooling and tests.</summary>
        public void SetMapping(CombatAudioTrigger trigger, string cueId)
        {
            int existingIndex = _entries.FindIndex(e => e.Trigger == trigger);

            if (existingIndex >= 0)
            {
                _entries[existingIndex].CueId = cueId;
            }
            else
            {
                _entries.Add(new Entry { Trigger = trigger, CueId = cueId });
            }

            _lookup = null;
        }

        private void OnValidate()
        {
            _lookup = null;
        }
    }
}
