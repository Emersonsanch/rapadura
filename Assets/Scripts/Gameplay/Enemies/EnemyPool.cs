using System.Collections.Generic;
using Rapadura.Core.Pooling;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Scene-facing enemy pool: recycles deactivated enemy GameObjects during waves instead of
    /// Instantiate/Destroy. The bookkeeping (inactive queue, active set, prewarm, double-release
    /// guard) is <see cref="GenericObjectPool{T}"/> from Core/Pooling — this class used to carry its own
    /// copy of the same Queue/HashSet logic. What stays here is the enemy-specific part:
    ///
    ///   - <see cref="Spawn"/> runs the full "fresh spawn" reset on every reuse:
    ///     <see cref="EnemyController.ResetForSpawn"/> (definition, health back to max, AI state, aggro
    ///     hold cleared, invulnerability cleared) plus <see cref="EnemyVisualSwapper.Apply"/>.
    ///   - <see cref="Get"/> undoes what <see cref="EnemyDeathVfx"/> did to a corpse: colliders it
    ///     disabled go back to the state they had when the instance was created, and the faded
    ///     renderer property blocks are cleared.
    ///   - Cleanup: every instance this pool ever created is destroyed in <see cref="OnDestroy"/>
    ///     (scene unload), so instances parented elsewhere (custom container, DontDestroyOnLoad) never
    ///     leak into the next scene. <see cref="Clear"/> does the same on demand.
    ///
    /// The public surface (<see cref="Configure"/>, <see cref="Prewarm"/>, <see cref="Get"/>,
    /// <see cref="Release"/>, <see cref="ReleaseAll"/>, counts, <see cref="Prefab"/>) is unchanged.
    /// </summary>
    public class EnemyPool : MonoBehaviour
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private int _prewarmCount;
        [SerializeField] private Transform _container;

        private GenericObjectPool<Transform> _pool;

        /// <summary>Every instance created by this pool, with the collider enabled-states it was born with.</summary>
        private readonly Dictionary<GameObject, ColliderDefaults> _created = new Dictionary<GameObject, ColliderDefaults>();

        public int InactiveCount => _pool != null ? _pool.InactiveCount : 0;
        public int ActiveCount => _pool != null ? _pool.ActiveCount : 0;

        /// <summary>Instances this pool created since the last <see cref="Clear"/> (active + parked).</summary>
        public int CreatedCount => _created.Count;

        public GameObject Prefab => _prefab;

        public void Configure(GameObject prefab, Transform container = null)
        {
            if (prefab != _prefab && _pool != null)
            {
                // Pooled instances of the old prefab must not be handed out for the new one.
                Clear();
            }

            _prefab = prefab;
            _container = container != null ? container : transform;
        }

        private void Awake()
        {
            if (_container == null)
            {
                _container = transform;
            }
        }

        private void Start()
        {
            Prewarm(_prewarmCount);
        }

        private void OnDestroy()
        {
            Clear();
        }

        public void Prewarm(int count)
        {
            if (_prefab == null || count <= 0)
            {
                return;
            }

            EnsurePool().Prewarm(count);
        }

        /// <summary>
        /// Takes an instance out of the pool (or creates one), restores its pooled-corpse state and
        /// activates it at the given pose. Does not touch AI/health/visuals — use <see cref="Spawn"/> for
        /// a full fresh-spawn reset. Returns null when no prefab is configured.
        /// </summary>
        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            if (_prefab == null)
            {
                return null;
            }

            GenericObjectPool<Transform> pool = EnsurePool();
            Transform instance = pool.Get();

            // Something outside the pool may have destroyed a parked instance; skip those.
            while (instance == null && pool.InactiveCount > 0)
            {
                instance = pool.Get();
            }

            if (instance == null)
            {
                instance = pool.Get();
            }

            if (instance == null)
            {
                return null;
            }

            GameObject go = instance.gameObject;
            RestorePooledState(go);
            instance.SetPositionAndRotation(position, rotation);
            go.SetActive(true);
            return go;
        }

        /// <summary>
        /// <see cref="Get"/> plus the full per-spawn reset every reused enemy needs:
        /// <see cref="EnemyController.ResetForSpawn"/> (health, AI state, aggro hold) and, when
        /// <paramref name="applyVisuals"/>, <see cref="EnemyVisualSwapper.Apply"/> for <paramref name="definition"/>.
        /// </summary>
        public GameObject Spawn(EnemyDefinition definition, Vector3 position, Quaternion rotation, Transform[] waypoints = null, bool applyVisuals = true)
        {
            GameObject instance = Get(position, rotation);
            if (instance == null)
            {
                return null;
            }

            EnemyController controller = instance.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.ResetForSpawn(definition, position, waypoints);
            }

            if (applyVisuals)
            {
                EnemyVisualSwapper.Apply(instance, definition);
            }

            return instance;
        }

        public void Release(GameObject instance)
        {
            if (instance == null || _pool == null)
            {
                return;
            }

            _pool.Release(instance.transform);
        }

        public void ReleaseAll()
        {
            _pool?.ReleaseAll();
        }

        /// <summary>
        /// Destroys every instance this pool created (active or parked) and forgets them. Runs
        /// automatically when the pool is destroyed (scene unload); safe to call repeatedly.
        /// </summary>
        public void Clear()
        {
            foreach (GameObject instance in _created.Keys)
            {
                if (instance == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(instance);
                }
                else
                {
                    DestroyImmediate(instance);
                }
            }

            _created.Clear();
            _pool = null;
        }

        private GenericObjectPool<Transform> EnsurePool()
        {
            if (_pool == null)
            {
                if (_container == null)
                {
                    _container = transform;
                }

                _pool = new GenericObjectPool<Transform>(CreateInstance, onRelease: Park);
            }

            return _pool;
        }

        private Transform CreateInstance()
        {
            if (_prefab == null)
            {
                return null;
            }

            GameObject instance = Instantiate(_prefab, _container);
            _created[instance] = ColliderDefaults.Capture(instance);
            return instance.transform;
        }

        private void Park(Transform instance)
        {
            if (instance == null)
            {
                return;
            }

            instance.gameObject.SetActive(false);
            if (_container != null && instance.parent != _container)
            {
                instance.SetParent(_container, worldPositionStays: false);
            }
        }

        /// <summary>Undoes <see cref="EnemyDeathVfx"/> on a reused corpse (colliders off, renderers faded out).</summary>
        private void RestorePooledState(GameObject instance)
        {
            if (_created.TryGetValue(instance, out ColliderDefaults defaults))
            {
                defaults.Restore();
            }

            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(includeInactive: true))
            {
                if (renderer.HasPropertyBlock())
                {
                    renderer.SetPropertyBlock(null);
                }
            }
        }

        /// <summary>Collider enabled-states captured right after Instantiate (i.e. the prefab's authored state).</summary>
        private readonly struct ColliderDefaults
        {
            private readonly Collider[] _colliders;
            private readonly bool[] _enabled;

            private ColliderDefaults(Collider[] colliders, bool[] enabled)
            {
                _colliders = colliders;
                _enabled = enabled;
            }

            public static ColliderDefaults Capture(GameObject instance)
            {
                Collider[] colliders = instance.GetComponentsInChildren<Collider>(includeInactive: true);
                var enabled = new bool[colliders.Length];
                for (int i = 0; i < colliders.Length; i++)
                {
                    enabled[i] = colliders[i].enabled;
                }

                return new ColliderDefaults(colliders, enabled);
            }

            public void Restore()
            {
                if (_colliders == null)
                {
                    return;
                }

                for (int i = 0; i < _colliders.Length; i++)
                {
                    if (_colliders[i] != null)
                    {
                        _colliders[i].enabled = _enabled[i];
                    }
                }
            }
        }
    }
}
