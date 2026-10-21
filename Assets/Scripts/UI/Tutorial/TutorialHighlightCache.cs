using UnityEngine;

namespace Rapadura.UI.Tutorial
{
    /// <summary>
    /// Pure bookkeeping behind <see cref="TutorialHintController"/>'s HUD highlight, split out so it
    /// is EditMode-testable without a panel:
    /// <list type="bullet">
    /// <item>"Was the HUD document already looked up in this scene?" — WorldMap.unity has no
    /// <c>HudController</c>, and a failed <c>FindFirstObjectByType</c> must not repeat every frame for
    /// the whole 20 s a hint stays up. Reset on sceneLoaded (<see cref="InvalidateScene"/>) and when a
    /// new hint appears (<see cref="RequestLookup"/>, cheap and covers a HUD that came up late).</item>
    /// <item>The last highlight rect written to the overlay's style, so the inline styles are only
    /// rewritten when the target's worldBound actually moves (<see cref="TryApply"/>).</item>
    /// <item>The HUD → WorldMap HUD element-name mapping (<see cref="WorldMapElementName"/>).</item>
    /// </list>
    /// </summary>
    public sealed class TutorialHighlightCache
    {
        /// <summary>Prefix WorldMapHudView.uxml puts on its equivalents of Main's HUD buttons ("quests-button" → "wm-quests-button").</summary>
        public const string WorldMapPrefix = "wm-";

        private bool _hudLookedUp;
        private bool _visible;
        private Rect _lastRect;

        /// <summary>True when the HUD document should be searched for (once per scene / per new hint).</summary>
        public bool NeedsHudLookup => !_hudLookedUp;

        /// <summary>Whether the overlay is currently shown (as last written by <see cref="TryApply"/>/<see cref="TryHide"/>).</summary>
        public bool IsVisible => _visible;

        /// <summary>Record that the scene was searched (found or not); no further search until invalidated.</summary>
        public void MarkHudLookedUp() => _hudLookedUp = true;

        /// <summary>New scene: forget both the lookup result and the applied rect.</summary>
        public void InvalidateScene()
        {
            _hudLookedUp = false;
            ForgetRect();
        }

        /// <summary>Allow one more lookup (e.g. a new hint was shown) without touching the applied rect.</summary>
        public void RequestLookup() => _hudLookedUp = false;

        /// <summary>Forces the next <see cref="TryApply"/> to write styles again (the overlay was hidden elsewhere).</summary>
        public void ForgetRect()
        {
            _visible = false;
            _lastRect = default;
        }

        /// <summary>True (and remembers <paramref name="rect"/>) when the overlay must be (re)positioned/shown.</summary>
        public bool TryApply(Rect rect)
        {
            if (_visible && _lastRect == rect)
            {
                return false;
            }

            _visible = true;
            _lastRect = rect;
            return true;
        }

        /// <summary>True when the overlay was visible and must now be hidden.</summary>
        public bool TryHide()
        {
            if (!_visible)
            {
                return false;
            }

            ForgetRect();
            return true;
        }

        /// <summary>
        /// Name of the WorldMap HUD's equivalent of a Main HUD element, or null when there is none to
        /// try. Only the "wm-" prefixed variant is looked up on the map, so a hint whose element has no
        /// map counterpart (e.g. "character-sheet-button") just shows without a highlight there.
        /// </summary>
        public static string WorldMapElementName(string hudElementName)
        {
            if (string.IsNullOrEmpty(hudElementName))
            {
                return null;
            }

            return hudElementName.StartsWith(WorldMapPrefix) ? hudElementName : WorldMapPrefix + hudElementName;
        }
    }
}
