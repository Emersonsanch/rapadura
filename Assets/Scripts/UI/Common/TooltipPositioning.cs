using UnityEngine;

namespace Rapadura.UI.Common
{
    /// <summary>
    /// Pure, UIDocument-free math used by <see cref="TooltipController"/> to place the tooltip
    /// on screen. Kept separate from the MonoBehaviour so it can be covered by EditMode tests
    /// without a real <c>UIDocument</c>/<c>PanelSettings</c> — same rationale as
    /// <see cref="Rapadura.UI.HUD.HudBarMath"/> (see TODO.md Fase 1 note on Editor-only assets).
    /// </summary>
    public static class TooltipPositioning
    {
        /// <summary>
        /// Computes the top-left position for a tooltip that follows the mouse cursor, offset by
        /// <paramref name="cursorOffset"/> and clamped so it never renders outside
        /// <paramref name="panelSize"/> (e.g. when the cursor is near the screen edge).
        /// </summary>
        public static Vector2 ComputeFollowMousePosition(Vector2 mousePosition, Vector2 tooltipSize, Vector2 panelSize, Vector2 cursorOffset)
        {
            Vector2 desired = mousePosition + cursorOffset;
            return ClampToPanel(desired, tooltipSize, panelSize);
        }

        /// <summary>
        /// Computes the top-left position for a tooltip anchored to a UI element's world bounds
        /// (e.g. a focused button). Prefers placing the tooltip below the anchor with
        /// <paramref name="spacing"/> pixels of gap; flips above it when that would overflow the
        /// bottom of the panel. Horizontally centers on the anchor, clamped to panel bounds.
        /// </summary>
        public static Vector2 ComputeAnchoredPosition(Rect anchorWorldBound, Vector2 tooltipSize, Vector2 panelSize, float spacing)
        {
            float centeredX = anchorWorldBound.x + (anchorWorldBound.width - tooltipSize.x) * 0.5f;

            float belowY = anchorWorldBound.yMax + spacing;
            float aboveY = anchorWorldBound.y - spacing - tooltipSize.y;

            bool fitsBelow = belowY + tooltipSize.y <= panelSize.y;
            float y = fitsBelow ? belowY : aboveY;

            return ClampToPanel(new Vector2(centeredX, y), tooltipSize, panelSize);
        }

        /// <summary>Clamps a desired top-left position so the tooltip rect stays fully within the panel.</summary>
        public static Vector2 ClampToPanel(Vector2 desiredPosition, Vector2 tooltipSize, Vector2 panelSize)
        {
            float maxX = Mathf.Max(0f, panelSize.x - tooltipSize.x);
            float maxY = Mathf.Max(0f, panelSize.y - tooltipSize.y);

            float x = Mathf.Clamp(desiredPosition.x, 0f, maxX);
            float y = Mathf.Clamp(desiredPosition.y, 0f, maxY);

            return new Vector2(x, y);
        }
    }
}
