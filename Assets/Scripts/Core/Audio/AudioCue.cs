using System;
using UnityEngine;

namespace Rapadura.Core.Audio
{
    /// <summary>
    /// A single named sound entry: which clip to play, which mix category it belongs to,
    /// and whether it is important enough to duck the music when it plays. Authored inside
    /// an <see cref="AudioCueDatabase"/> asset so designers can wire up sounds without touching code.
    /// </summary>
    [Serializable]
    public class AudioCue
    {
        [Tooltip("Stable identifier used by PlayOneShot(cueId) and by CombatAudioCueMap.")]
        [SerializeField] private string _id;
        [SerializeField] private AudioClip _clip;
        [SerializeField] private AudioCategory _category = AudioCategory.Sfx;
        [Range(0f, 1f)]
        [SerializeField] private float _volume = 1f;
        [Tooltip("If true, playing this cue triggers a brief automatic music ducking.")]
        [SerializeField] private bool _ducksMusic;
        [SerializeField] private bool _is3D;

        public string Id => _id;
        public AudioClip Clip => _clip;
        public AudioCategory Category => _category;
        public float Volume => _volume;
        public bool DucksMusic => _ducksMusic;
        public bool Is3D => _is3D;

        public AudioCue()
        {
        }

        /// <summary>Constructor used by tests/tools to build cues without the inspector.</summary>
        public AudioCue(string id, AudioClip clip, AudioCategory category = AudioCategory.Sfx, float volume = 1f, bool ducksMusic = false, bool is3D = false)
        {
            _id = id;
            _clip = clip;
            _category = category;
            _volume = volume;
            _ducksMusic = ducksMusic;
            _is3D = is3D;
        }
    }
}
