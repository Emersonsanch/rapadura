using UnityEngine;

namespace Rapadura.Core.DI
{
    /// <summary>
    /// Generic MonoBehaviour singleton base class. Use sparingly — only for true
    /// application-wide entry points (e.g. GameManager) that must survive scene loads.
    /// Regular systems should be resolved through <see cref="ServiceLocator"/> instead
    /// of inheriting from this class.
    /// </summary>
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();
                }

                return _instance;
            }
        }

        [SerializeField] private bool _persistAcrossScenes = true;

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = (T)this;

            if (_persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
