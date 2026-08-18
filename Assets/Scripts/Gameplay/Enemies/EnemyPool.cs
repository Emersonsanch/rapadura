using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Minimal Queue-backed object pool for enemy instances. Avoids repeated Instantiate/Destroy calls
    /// during waves by recycling deactivated GameObjects. Deliberately not built on Unity's
    /// <c>UnityEngine.Pool</c> API to keep the dependency surface small and the behaviour easy to reason
    /// about/test; swapping the internals for <c>ObjectPool&lt;T&gt;</c> later is a drop-in change.
    /// </summary>
    public class EnemyPool : MonoBehaviour
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private int _prewarmCount;
        [SerializeField] private Transform _container;

        private readonly Queue<GameObject> _inactive = new Queue<GameObject>();
        private readonly HashSet<GameObject> _active = new HashSet<GameObject>();

        public int InactiveCount => _inactive.Count;
        public int ActiveCount => _active.Count;
        public GameObject Prefab => _prefab;

        public void Configure(GameObject prefab, Transform container = null)
        {
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

        public void Prewarm(int count)
        {
            if (_prefab == null)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                GameObject instance = CreateInstance();
                instance.SetActive(false);
                _inactive.Enqueue(instance);
            }
        }

        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            GameObject instance = _inactive.Count > 0 ? _inactive.Dequeue() : CreateInstance();

            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
            _active.Add(instance);
            return instance;
        }

        public void Release(GameObject instance)
        {
            if (instance == null || !_active.Remove(instance))
            {
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(_container, worldPositionStays: false);
            _inactive.Enqueue(instance);
        }

        public void ReleaseAll()
        {
            foreach (GameObject instance in _active)
            {
                if (instance == null)
                {
                    continue;
                }

                instance.SetActive(false);
                instance.transform.SetParent(_container, worldPositionStays: false);
                _inactive.Enqueue(instance);
            }

            _active.Clear();
        }

        private GameObject CreateInstance()
        {
            GameObject instance = Instantiate(_prefab, _container);
            return instance;
        }
    }
}
