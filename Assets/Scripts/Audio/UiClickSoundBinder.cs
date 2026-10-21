using System.Collections.Generic;
using Rapadura.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.Audio
{
    /// <summary>
    /// Gives every UI Toolkit button/toggle/dropdown a click sound without touching each screen
    /// controller: about once a second it looks for <see cref="UIDocument"/>s whose root it has not
    /// hooked yet (screens are rebuilt when re-enabled, so roots change) and registers one bubbling
    /// <see cref="ClickEvent"/> callback on it. Clicks are forwarded to <see cref="UiSoundHooks.PlayClick"/>,
    /// whose cue cooldown also dedups a controller that already plays the sound itself.
    /// Added by <see cref="SfxEventRouter.Initialize"/> in Play mode only.
    /// </summary>
    internal class UiClickSoundBinder : MonoBehaviour
    {
        private const float ScanIntervalSeconds = 1f;
        private const int MaxAncestorDepth = 4;

        private readonly HashSet<VisualElement> _hooked = new HashSet<VisualElement>();
        private readonly List<VisualElement> _stale = new List<VisualElement>();
        private float _nextScan;

        private void Update()
        {
            if (Time.unscaledTime < _nextScan)
            {
                return;
            }

            _nextScan = Time.unscaledTime + ScanIntervalSeconds;
            Scan();
        }

        private void Scan()
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

            foreach (UIDocument document in FindObjectsByType<UIDocument>(FindObjectsSortMode.None))
            {
                VisualElement root = document != null ? document.rootVisualElement : null;
                if (root != null && root.panel != null && _hooked.Add(root))
                {
                    root.RegisterCallback<ClickEvent>(OnClick);
                }
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
        }
    }
}
