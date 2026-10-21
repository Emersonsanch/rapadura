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
                RemoveDuplicate();
                return;
            }

            _instance = (T)this;

            if (_persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>
        /// A scene loaded after the persistent instance (e.g. Main.unity's GameManager object, reached
        /// from WorldMap) may carry other per-scene components on the same object (HitStopController,
        /// EnemyRewardGranter, ...). Destroying the whole duplicate GameObject silently killed them, so
        /// only this component goes — plus any sibling whose type the persistent object already has
        /// (it was carried over with it, e.g. when Main.unity was the first scene; keeping both would
        /// double every reward). The GameObject itself goes only when nothing else is left on it.
        /// </summary>
        private void RemoveDuplicate()
        {
            var redundant = new System.Collections.Generic.List<Component>();
            bool keepsOthers = false;

            foreach (Component component in GetComponents<Component>())
            {
                if (component == null || component == this || component is Transform)
                {
                    continue;
                }

                if (_instance.GetComponent(component.GetType()) != null)
                {
                    redundant.Add(component);
                }
                else
                {
                    keepsOthers = true;
                }
            }

            if (!keepsOthers)
            {
                Destroy(gameObject);
                return;
            }

            foreach (Component component in redundant)
            {
                Destroy(component);
            }

            Destroy(this);
        }

        /// <summary>True when this component is the live singleton (false for a duplicate being removed in Awake).</summary>
        protected bool IsPrimaryInstance => _instance == this;

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
