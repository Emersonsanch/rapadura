using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Core.Audio
{
    /// <summary>
    /// Data-driven table of every sound cue in the game (combat, player, UI). Lets designers add/retune
    /// sounds — clip, category, volume, ducking — without touching code, the same "balanceável via dados
    /// externos" approach used by <see cref="Rapadura.Gameplay.Combat.DamageBalanceConfig"/> and
    /// <see cref="Rapadura.Gameplay.Skills.SkillDefinition"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioCueDatabase", menuName = "Rapadura/Audio/Audio Cue Database", order = 0)]
    public class AudioCueDatabase : ScriptableObject
    {
        [SerializeField] private List<AudioCue> _cues = new List<AudioCue>();

        private Dictionary<string, AudioCue> _lookup;

        private void BuildLookupIfNeeded()
        {
            if (_lookup != null)
            {
                return;
            }

            _lookup = new Dictionary<string, AudioCue>();

            foreach (AudioCue cue in _cues)
            {
                if (cue == null || string.IsNullOrEmpty(cue.Id))
                {
                    continue;
                }

                _lookup[cue.Id] = cue;
            }
        }

        public bool TryGetCue(string id, out AudioCue cue)
        {
            BuildLookupIfNeeded();

            if (string.IsNullOrEmpty(id))
            {
                cue = null;
                return false;
            }

            return _lookup.TryGetValue(id, out cue);
        }

        public IReadOnlyList<AudioCue> Cues => _cues;

        /// <summary>Adds or replaces a cue. Used by editor tooling and tests — not meant for hot gameplay code.</summary>
        public void AddOrReplaceCue(AudioCue cue)
        {
            if (cue == null || string.IsNullOrEmpty(cue.Id))
            {
                return;
            }

            int existingIndex = _cues.FindIndex(c => c != null && c.Id == cue.Id);

            if (existingIndex >= 0)
            {
                _cues[existingIndex] = cue;
            }
            else
            {
                _cues.Add(cue);
            }

            _lookup = null;
        }

        private void OnValidate()
        {
            _lookup = null;
        }
    }
}
