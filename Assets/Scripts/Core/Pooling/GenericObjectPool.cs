using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Core.Pooling
{
    /// <summary>
    /// Generic, reusable object pool for any <see cref="Component"/> type (projectiles, VFX,
    /// future enemy variants, etc.). Modeled after the same Queue/HashSet approach used by
    /// <c>Rapadura.Gameplay.Enemies.EnemyPool</c> (Fase 2), but parameterized so any system can
    /// use it without depending on enemy-specific code, and built around a <see cref="Func{T}"/>
    /// factory instead of a single serialized prefab reference — closer in spirit to Unity's own
    /// <c>UnityEngine.Pool.ObjectPool&lt;T&gt;</c> (see docs.unity3d.com/Manual/performance-reusable-code.html),
    /// which was the reference design researched for this implementation. We keep a small,
    /// dependency-free implementation rather than wrapping <c>UnityEngine.Pool.ObjectPool&lt;T&gt;</c>
    /// directly so this class stays trivially unit-testable in EditMode without a live scene.
    ///
    /// Callers own the factory (how to Instantiate/create a <typeparamref name="T"/>) and may
    /// optionally supply hooks for what happens on Get/Release, mirroring Unity's
    /// actionOnGet/actionOnRelease/actionOnDestroy callbacks.
    /// </summary>
    /// <typeparam name="T">Component type stored in the pool.</typeparam>
    public class GenericObjectPool<T> where T : Component
    {
        private readonly Func<T> _factory;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly Action<T> _onDestroy;
        private readonly int _maxSize;

        private readonly Queue<T> _inactive = new Queue<T>();
        private readonly HashSet<T> _active = new HashSet<T>();

        /// <summary>Number of instances currently sitting inactive in the pool, ready to be reused.</summary>
        public int InactiveCount => _inactive.Count;

        /// <summary>Number of instances currently checked out via <see cref="Get"/>.</summary>
        public int ActiveCount => _active.Count;

        /// <summary>Total instances ever created by this pool (active + inactive).</summary>
        public int CountAll => InactiveCount + ActiveCount;

        /// <param name="factory">Creates a brand-new instance. Called only when the pool has nothing free to reuse.</param>
        /// <param name="onGet">Optional hook invoked on the instance right before it is handed out by <see cref="Get"/> (e.g. SetActive(true)).</param>
        /// <param name="onRelease">Optional hook invoked on the instance right after it is returned via <see cref="Release"/> (e.g. SetActive(false)).</param>
        /// <param name="onDestroy">Optional hook invoked when an instance is evicted because the pool is above <paramref name="maxSize"/>.</param>
        /// <param name="maxSize">Soft cap on how many inactive instances are kept around; 0 or negative means unlimited.</param>
        public GenericObjectPool(Func<T> factory, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, int maxSize = 0)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _onGet = onGet;
            _onRelease = onRelease;
            _onDestroy = onDestroy;
            _maxSize = maxSize;
        }

        /// <summary>Creates <paramref name="count"/> instances up front and parks them inactive, avoiding first-use Instantiate spikes.</summary>
        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T instance = _factory();

                if (instance == null)
                {
                    continue;
                }

                _onRelease?.Invoke(instance);
                _inactive.Enqueue(instance);
            }
        }

        /// <summary>Returns a reused instance if one is available, otherwise creates a new one via the factory.</summary>
        public T Get()
        {
            T instance = _inactive.Count > 0 ? _inactive.Dequeue() : _factory();

            if (instance != null)
            {
                _active.Add(instance);
                _onGet?.Invoke(instance);
            }

            return instance;
        }

        /// <summary>
        /// Returns an instance to the pool. No-ops for null/unknown instances (e.g. double-release,
        /// or an instance this pool never issued) so callers don't need to track that themselves.
        /// </summary>
        public void Release(T instance)
        {
            if (instance == null || !_active.Remove(instance))
            {
                return;
            }

            _onRelease?.Invoke(instance);

            if (_maxSize > 0 && _inactive.Count >= _maxSize)
            {
                _onDestroy?.Invoke(instance);
                return;
            }

            _inactive.Enqueue(instance);
        }

        /// <summary>Forces every currently active instance back into the pool. Intended for scene teardown/tests.</summary>
        public void ReleaseAll()
        {
            foreach (T instance in _active)
            {
                if (instance == null)
                {
                    continue;
                }

                _onRelease?.Invoke(instance);
                _inactive.Enqueue(instance);
            }

            _active.Clear();
        }

        /// <summary>Empties the pool, invoking <c>onDestroy</c> for every inactive instance still held.</summary>
        public void Clear()
        {
            while (_inactive.Count > 0)
            {
                T instance = _inactive.Dequeue();
                _onDestroy?.Invoke(instance);
            }
        }
    }
}
