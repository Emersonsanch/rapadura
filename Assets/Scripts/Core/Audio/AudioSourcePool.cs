using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Core.Audio
{
    /// <summary>
    /// Simple pool of reusable <see cref="AudioSource"/> components, so one-shot SFX never pay
    /// the cost of Instantiate/Destroy. Grows lazily up to <see cref="_maxSize"/>; once full it
    /// steals the least-recently-acquired source (a short audible cut on an already-busy source
    /// is preferable to unbounded GameObject growth).
    /// </summary>
    public class AudioSourcePool
    {
        private readonly List<AudioSource> _sources = new List<AudioSource>();
        private readonly Transform _root;
        private readonly int _maxSize;

        public int Count => _sources.Count;
        public IReadOnlyList<AudioSource> Sources => _sources;

        public AudioSourcePool(Transform root, int initialSize, int maxSize)
        {
            _root = root;
            _maxSize = Mathf.Max(1, maxSize);

            int clampedInitial = Mathf.Clamp(initialSize, 0, _maxSize);

            for (int i = 0; i < clampedInitial; i++)
            {
                CreateSource();
            }
        }

        private AudioSource CreateSource()
        {
            var go = new GameObject($"PooledAudioSource_{_sources.Count}");

            if (_root != null)
            {
                go.transform.SetParent(_root, false);
            }

            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            _sources.Add(source);
            return source;
        }

        /// <summary>Returns a source that is not currently playing, growing the pool if allowed and needed.</summary>
        public AudioSource Acquire()
        {
            foreach (AudioSource source in _sources)
            {
                if (source != null && !source.isPlaying)
                {
                    // Move to the back so round-robin reuse favors the least-recently-used source.
                    _sources.Remove(source);
                    _sources.Add(source);
                    return source;
                }
            }

            if (_sources.Count < _maxSize)
            {
                return CreateSource();
            }

            // Pool exhausted and everything is busy: steal the oldest (front of the list).
            AudioSource stolen = _sources[0];
            _sources.RemoveAt(0);
            _sources.Add(stolen);
            return stolen;
        }

        public void StopAll()
        {
            foreach (AudioSource source in _sources)
            {
                if (source != null)
                {
                    source.Stop();
                }
            }
        }
    }
}
