using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.Core.Systems
{
    /// <summary>
    /// "Is the pointer over UI?" for gameplay input that shares the left mouse button with UI clicks
    /// (Attack, MoveClick). Covers uGUI via <see cref="EventSystem.IsPointerOverGameObject()"/> and
    /// UI Toolkit by picking every active <see cref="UIDocument"/> panel directly — scenes without an
    /// EventSystem (WorldMap.unity) never report UI Toolkit hits through the EventSystem.
    ///
    /// The documents are cached, not searched per call (hold-to-walk re-aims every 0.15 s, every
    /// press and every new touch ask): a <c>FindObjectsByType</c> scan (inactive included, so a
    /// screen enabled later is already known) runs lazily right after each sceneLoaded and again
    /// <see cref="RescanDelays"/> later for documents a controller creates a moment after the load,
    /// plus <see cref="Register"/> for self-bootstrapped DontDestroyOnLoad documents (forwarded by
    /// <c>UiClickSoundBinder.Register</c>). Each query then only walks the cached list, skipping
    /// disabled/destroyed entries — no allocation per call.
    /// </summary>
    public static class UiPointerQuery
    {
        /// <summary>Seconds after a sceneLoaded (or <see cref="InvalidateDocumentCache"/>) at which the next query rescans the scene.</summary>
        public static readonly float[] RescanDelays = { 0f, 0.5f, 2f };

        private static readonly List<UIDocument> Documents = new List<UIDocument>();
        private static readonly HashSet<UIDocument> KnownDocuments = new HashSet<UIDocument>();
        private static readonly List<float> ScanTimes = new List<float>();
        private static bool _subscribed;

        /// <summary>Number of scene scans done so far (diagnostics/tests).</summary>
        public static int ScanCount { get; private set; }

        /// <summary>Documents currently cached (destroyed ones are dropped on the next scan).</summary>
        public static int CachedDocumentCount => Documents.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Documents.Clear();
            KnownDocuments.Clear();
            ScanCount = 0;
            _subscribed = false;
            EnsureSubscribed();
        }

        private static void EnsureSubscribed()
        {
            if (_subscribed)
            {
                return;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            _subscribed = true;
            InvalidateDocumentCache();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => InvalidateDocumentCache();

        /// <summary>Schedules the scene rescans (now, and after each <see cref="RescanDelays"/>); the scans themselves run lazily on the next queries.</summary>
        public static void InvalidateDocumentCache()
        {
            float now = Time.unscaledTime;
            ScanTimes.Clear();
            foreach (float delay in RescanDelays)
            {
                ScanTimes.Add(now + delay);
            }
        }

        /// <summary>Scene rescans still scheduled (diagnostics/tests).</summary>
        public static int PendingRescanCount => ScanTimes.Count;

        /// <summary>Drops the scheduled rescans (tests that measure the steady-state query path).</summary>
        public static void CancelScheduledRescans() => ScanTimes.Clear();

        /// <summary>Makes <paramref name="document"/> known right away (self-bootstrapped documents created outside the scans). Null-safe, idempotent.</summary>
        public static void Register(UIDocument document)
        {
            if (document != null && KnownDocuments.Add(document))
            {
                Documents.Add(document);
            }
        }

        public static void Unregister(UIDocument document)
        {
            if (document != null && KnownDocuments.Remove(document))
            {
                Documents.Remove(document);
            }
        }

        public static bool IsCached(UIDocument document) => document != null && KnownDocuments.Contains(document);

        /// <param name="screenPosition">Screen space, bottom-left origin (like <c>Mouse.position</c>).</param>
        public static bool IsPointerOverUi(Vector2 screenPosition)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }

            RescanIfDue();

            for (int i = 0; i < Documents.Count; i++)
            {
                UIDocument document = Documents[i];
                if (document == null || !document.isActiveAndEnabled)
                {
                    continue;
                }

                VisualElement root = document.rootVisualElement;
                IPanel panel = root?.panel;
                if (panel == null)
                {
                    continue;
                }

                Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(screenPosition.x, Screen.height - screenPosition.y));
                if (IsBlockingElement(panel.Pick(panelPosition), root))
                {
                    return true;
                }
            }

            return false;
        }

        private static void RescanIfDue()
        {
            // EditMode / statics never initialised by SubsystemRegistration: behave like a fresh load
            // (keeps documents registered before the first query).
            EnsureSubscribed();

            float now = Time.unscaledTime;
            bool due = false;
            for (int i = ScanTimes.Count - 1; i >= 0; i--)
            {
                if (now >= ScanTimes[i])
                {
                    ScanTimes.RemoveAt(i);
                    due = true;
                }
            }

            if (due)
            {
                ScanScene();
            }
        }

        /// <summary>Adds every UIDocument in the loaded scenes (inactive included) and drops destroyed ones. Allocates; runs only on the scheduled rescans.</summary>
        public static void ScanScene()
        {
            ScanCount++;

            for (int i = Documents.Count - 1; i >= 0; i--)
            {
                if (Documents[i] == null)
                {
                    Documents.RemoveAt(i);
                }
            }

            KnownDocuments.RemoveWhere(d => d == null);

            foreach (UIDocument document in Object.FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Register(document);
            }
        }

        /// <summary>
        /// True when <paramref name="picked"/> is real UI rather than a document's transparent,
        /// full-screen layout container (HUD roots stretch over the whole screen and would otherwise
        /// swallow every world click): it, or an ancestor below <paramref name="documentRoot"/>, is a
        /// focusable control (button, slot, field...) or draws a background.
        /// </summary>
        public static bool IsBlockingElement(VisualElement picked, VisualElement documentRoot)
        {
            for (VisualElement element = picked; element != null && element != documentRoot; element = element.parent)
            {
                if (element.panel != null && element == element.panel.visualTree)
                {
                    break;
                }

                // Full-screen layers (HUD root, vignette/tint overlays) are never "a button under the
                // cursor": a modal's full-screen dim already pauses gameplay, so only smaller UI blocks.
                if (CoversWholePanel(element))
                {
                    continue;
                }

                if (element is Button || element.focusable || element.canGrabFocus)
                {
                    return true;
                }

                IResolvedStyle style = element.resolvedStyle;
                if (style.backgroundColor.a > 0.01f || style.backgroundImage.texture != null || style.backgroundImage.sprite != null || style.backgroundImage.vectorImage != null || style.backgroundImage.renderTexture != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CoversWholePanel(VisualElement element)
        {
            VisualElement panelRoot = element.panel?.visualTree;
            if (panelRoot == null)
            {
                return false;
            }

            Rect bounds = element.worldBound;
            Rect panelBounds = panelRoot.worldBound;
            float panelArea = panelBounds.width * panelBounds.height;
            return panelArea > 0f && bounds.width * bounds.height >= panelArea * 0.9f;
        }
    }
}
