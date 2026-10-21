using System.Collections.Generic;
using Rapadura.UI.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.Audio
{
    /// <summary>
    /// Gives every UI Toolkit button/toggle/dropdown a click sound without touching each screen
    /// controller: it registers one bubbling <see cref="ClickEvent"/> callback on the root of each
    /// known <see cref="UIDocument"/> and forwards clicks to <see cref="UiSoundHooks.PlayClick"/>,
    /// whose cue cooldown also dedups a controller that already plays the sound itself.
    ///
    /// Documents become known two ways, neither of which polls the whole scene forever:
    /// <list type="bullet">
    /// <item>a <c>FindObjectsByType&lt;UIDocument&gt;</c> scan (inactive included) right after each
    /// sceneLoaded plus a couple of delayed rescans (<see cref="RescanDelays"/>) for screens a
    /// controller builds a moment later;</item>
    /// <item><see cref="Register"/>, for self-bootstrapped DontDestroyOnLoad documents (character
    /// sheet, tutorial hint, boss banner, return-to-title dialog) created outside those scans.</item>
    /// </list>
    /// Known documents are re-checked cheaply every <see cref="RecheckIntervalSeconds"/> because a
    /// screen's root is rebuilt when it is re-enabled. Added by <see cref="SfxEventRouter.Initialize"/>
    /// in Play mode only.
    /// </summary>
    public class UiClickSoundBinder : MonoBehaviour
    {
        /// <summary>Seconds after a sceneLoaded at which the full scene scan runs again.</summary>
        public static readonly float[] RescanDelays = { 0f, 0.5f, 2f };

        private const float RecheckIntervalSeconds = 0.25f;
        private const int MaxAncestorDepth = 4;

        private static readonly HashSet<UIDocument> Registered = new HashSet<UIDocument>();
        private static UiClickSoundBinder _instance;

        private readonly HashSet<VisualElement> _hooked = new HashSet<VisualElement>();
        private readonly List<VisualElement> _stale = new List<VisualElement>();
        private readonly List<UIDocument> _tracked = new List<UIDocument>();
        private readonly HashSet<UIDocument> _trackedSet = new HashSet<UIDocument>();
        private readonly List<float> _scanTimes = new List<float>();
        private float _nextRecheck;

        /// <summary>
        /// Opts a document into click sounds right away (and across re-enables). Safe to call before
        /// the binder exists, more than once, or with null.
        /// </summary>
        public static void Register(UIDocument document)
        {
            if (document == null || !Registered.Add(document))
            {
                return;
            }

            if (_instance != null)
            {
                _instance.Track(document);
                _instance.Hook(document);
            }
        }

        /// <summary>Stops tracking a registered document (its current root is unhooked).</summary>
        public static void Unregister(UIDocument document)
        {
            if (document == null || !Registered.Remove(document))
            {
                return;
            }

            if (_instance != null)
            {
                _instance.Untrack(document);
            }
        }

        public static bool IsRegistered(UIDocument document) => document != null && Registered.Contains(document);

        /// <summary>Registered documents still alive (destroyed ones are dropped as a side effect).</summary>
        public static int RegisteredCount
        {
            get
            {
                Registered.RemoveWhere(d => d == null);
                return Registered.Count;
            }
        }

        private void Awake()
        {
            _instance = this;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            ScheduleScans();

            Registered.RemoveWhere(d => d == null);
            foreach (UIDocument document in Registered)
            {
                Track(document);
            }
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ScheduleScans();

        private void ScheduleScans()
        {
            float now = Time.unscaledTime;
            _scanTimes.Clear();
            foreach (float delay in RescanDelays)
            {
                _scanTimes.Add(now + delay);
            }
        }

        private void Update()
        {
            float now = Time.unscaledTime;
            bool scanned = false;
            for (int i = _scanTimes.Count - 1; i >= 0; i--)
            {
                if (now >= _scanTimes[i])
                {
                    _scanTimes.RemoveAt(i);
                    if (!scanned)
                    {
                        ScanScene();
                        scanned = true;
                    }
                }
            }

            if (scanned || now >= _nextRecheck)
            {
                _nextRecheck = now + RecheckIntervalSeconds;
                Recheck();
            }
        }

        private void ScanScene()
        {
            foreach (UIDocument document in FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Track(document);
            }
        }

        /// <summary>Drops dead roots/documents and hooks any tracked document whose root is new.</summary>
        private void Recheck()
        {
            _stale.Clear();
            foreach (VisualElement root in _hooked)
            {
                if (root == null || root.panel == null)
                {
                    _stale.Add(root);
                }
            }

            foreach (VisualElement root in _stale)
            {
                root?.UnregisterCallback<ClickEvent>(OnClick);
                _hooked.Remove(root);
            }

            for (int i = _tracked.Count - 1; i >= 0; i--)
            {
                UIDocument document = _tracked[i];
                if (document == null)
                {
                    _tracked.RemoveAt(i);
                    _trackedSet.Remove(document);
                    continue;
                }

                if (document.isActiveAndEnabled)
                {
                    Hook(document);
                }
            }
        }

        private void Track(UIDocument document)
        {
            if (document != null && _trackedSet.Add(document))
            {
                _tracked.Add(document);
            }
        }

        private void Untrack(UIDocument document)
        {
            if (!_trackedSet.Remove(document))
            {
                return;
            }

            _tracked.Remove(document);
            VisualElement root = document.rootVisualElement;
            if (root != null && _hooked.Remove(root))
            {
                root.UnregisterCallback<ClickEvent>(OnClick);
            }
        }

        private void Hook(UIDocument document)
        {
            VisualElement root = document != null ? document.rootVisualElement : null;
            if (root != null && root.panel != null && _hooked.Add(root))
            {
                root.RegisterCallback<ClickEvent>(OnClick);
            }
        }

        private static void OnClick(ClickEvent evt)
        {
            if (IsClickable(evt.target as VisualElement))
            {
                UiSoundHooks.PlayClick();
            }
        }

        internal static bool IsClickable(VisualElement element)
        {
            for (int depth = 0; element != null && depth <= MaxAncestorDepth; depth++, element = element.parent)
            {
                if (element is Button || element is Toggle || element is DropdownField)
                {
                    return element.enabledInHierarchy;
                }
            }

            return false;
        }

        private void OnDestroy()
        {
            foreach (VisualElement root in _hooked)
            {
                root?.UnregisterCallback<ClickEvent>(OnClick);
            }

            _hooked.Clear();
            _tracked.Clear();
            _trackedSet.Clear();

            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
