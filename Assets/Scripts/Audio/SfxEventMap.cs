using System;
using System.Collections.Generic;
using Rapadura.Core.Audio;
using UnityEngine;

namespace Rapadura.Audio
{
    /// <summary>
    /// Data table behind the event-driven audio: which cue id each <see cref="SfxTrigger"/> (+ variant)
    /// plays, how each cue behaves (clip, mix category, volume, cooldown, voice cap, pause rules) and
    /// which music plays per <see cref="MusicContext"/>. Cues/music without an assigned clip fall back
    /// to a placeholder baked by <see cref="ProceduralSfxSynth"/>, so the game has sound before any real
    /// asset exists — assigning a clip in the inspector replaces the placeholder with no code change.
    /// The shipped asset lives at Resources/Audio/SfxEventMap; <see cref="CreateDefault"/> builds the
    /// same table in code when it is missing (tests, fresh checkouts). See Docs/Audio.md.
    /// </summary>
    [CreateAssetMenu(fileName = "SfxEventMap", menuName = "Rapadura/Audio/Sfx Event Map", order = 2)]
    public partial class SfxEventMap : ScriptableObject
    {
        public const string ResourcePath = "Audio/SfxEventMap";

        [Serializable]
        public class Binding
        {
            public SfxTrigger trigger;
            [Tooltip("Lower-case qualifier (element, biome, weather, phase...). Empty = fallback for the trigger.")]
            public string variant = string.Empty;
            [Tooltip("Cue id in the Cues list. Empty = deliberately silent.")]
            public string cueId = string.Empty;
        }

        [Serializable]
        public class Cue
        {
            public string id = string.Empty;
            [Tooltip("Real sound. When empty the procedural placeholder below is used.")]
            public AudioClip clip;
            public AudioCategory category = AudioCategory.Sfx;
            [Range(0f, 1f)] public float volume = 1f;
            [Tooltip("Minimum seconds between two plays of this cue (anti-spam).")]
            public float cooldown = 0.05f;
            [Tooltip("Max simultaneous voices of this cue.")]
            public int maxVoices = 2;
            [Tooltip("Suppressed while GameplayPause is active (combat sounds). UI/progression cues leave this off.")]
            public bool pausable = true;
            [Tooltip("Played as a 3D sound at the event position when one is known.")]
            public bool spatial;
            [Tooltip("Looping ambience bed (weather) instead of a one-shot.")]
            public bool loop;
            public bool ducksMusic;
            public ProceduralSfxPreset preset = ProceduralSfxPreset.Blip;
            public float frequency = 440f;
        }

        [Serializable]
        public class MusicEntry
        {
            public MusicContext context;
            public string variant = string.Empty;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 0.35f;
            [Header("Procedural placeholder")]
            public float rootFrequency = 220f;
            public bool minor;
            public float bpm = 90f;
        }

        [SerializeField] private int _maxVoices = 12;
        [SerializeField] private float _musicCrossfadeSeconds = 1.5f;
        [Tooltip("Bake placeholder sounds for cues without a clip.")]
        [SerializeField] private bool _proceduralSfx = true;
        [Tooltip("Bake placeholder music loops (drone + arpeggio) for contexts without a clip. Off = silence.")]
        [SerializeField] private bool _proceduralMusic = true;
        [SerializeField] private List<Binding> _bindings = new List<Binding>();
        [SerializeField] private List<Cue> _cues = new List<Cue>();
        [SerializeField] private List<MusicEntry> _music = new List<MusicEntry>();

        private Dictionary<string, Cue> _cueLookup;

        public int MaxVoices => Mathf.Max(1, _maxVoices);
        public float MusicCrossfadeSeconds => Mathf.Max(0f, _musicCrossfadeSeconds);
        public bool ProceduralSfx => _proceduralSfx;
        public bool ProceduralMusic => _proceduralMusic;
        public IReadOnlyList<Binding> Bindings => _bindings;
        public IReadOnlyList<Cue> Cues => _cues;
        public IReadOnlyList<MusicEntry> Music => _music;

        /// <summary>Exact (trigger, variant) lookup. A found binding with an empty cue id means "silent on purpose".</summary>
        public bool TryResolve(SfxTrigger trigger, string variant, out string cueId)
        {
            string key = Normalize(variant);

            foreach (Binding binding in _bindings)
            {
                if (binding != null && binding.trigger == trigger && Normalize(binding.variant) == key)
                {
                    cueId = binding.cueId ?? string.Empty;
                    return true;
                }
            }

            cueId = null;
            return false;
        }

        /// <summary>Tries each variant in order, then the trigger's empty-variant fallback. Returns null when nothing matches.</summary>
        public string ResolveFirst(SfxTrigger trigger, params string[] variants)
        {
            if (variants != null)
            {
                foreach (string variant in variants)
                {
                    if (!string.IsNullOrEmpty(variant) && TryResolve(trigger, variant, out string cueId))
                    {
                        return cueId;
                    }
                }
            }

            return TryResolve(trigger, string.Empty, out string fallback) ? fallback : null;
        }

        public bool TryGetCue(string id, out Cue cue)
        {
            if (_cueLookup == null)
            {
                _cueLookup = new Dictionary<string, Cue>(StringComparer.Ordinal);
                foreach (Cue entry in _cues)
                {
                    if (entry != null && !string.IsNullOrEmpty(entry.id))
                    {
                        _cueLookup[entry.id] = entry;
                    }
                }
            }

            cue = null;
            return !string.IsNullOrEmpty(id) && _cueLookup.TryGetValue(id, out cue);
        }

        /// <summary>(context, variant) then (context, "").</summary>
        public bool TryGetMusic(MusicContext context, string variant, out MusicEntry entry)
        {
            string key = Normalize(variant);
            entry = null;

            foreach (MusicEntry candidate in _music)
            {
                if (candidate == null || candidate.context != context)
                {
                    continue;
                }

                string candidateKey = Normalize(candidate.variant);
                if (candidateKey == key)
                {
                    entry = candidate;
                    return true;
                }

                if (candidateKey.Length == 0 && entry == null)
                {
                    entry = candidate;
                }
            }

            return entry != null;
        }

        public void AddBinding(SfxTrigger trigger, string variant, string cueId)
        {
            _bindings.Add(new Binding { trigger = trigger, variant = Normalize(variant), cueId = cueId ?? string.Empty });
        }

        public void AddCue(Cue cue)
        {
            if (cue == null || string.IsNullOrEmpty(cue.id))
            {
                return;
            }

            _cues.RemoveAll(c => c != null && c.id == cue.id);
            _cues.Add(cue);
            _cueLookup = null;
        }

        public void AddMusic(MusicEntry entry)
        {
            if (entry != null)
            {
                _music.Add(entry);
            }
        }

        /// <summary>Test/tooling hooks.</summary>
        public void Configure(int maxVoices, bool proceduralSfx, bool proceduralMusic)
        {
            _maxVoices = maxVoices;
            _proceduralSfx = proceduralSfx;
            _proceduralMusic = proceduralMusic;
        }

        /// <summary>Resources/Audio/SfxEventMap, or the in-code default table when the asset is missing.</summary>
        public static SfxEventMap LoadOrDefault()
        {
            SfxEventMap asset = Resources.Load<SfxEventMap>(ResourcePath);
            return asset != null ? asset : CreateDefault();
        }

        public static string Normalize(string variant) => string.IsNullOrEmpty(variant) ? string.Empty : variant.Trim().ToLowerInvariant();

        private void OnValidate()
        {
            _cueLookup = null;
        }
    }
}
