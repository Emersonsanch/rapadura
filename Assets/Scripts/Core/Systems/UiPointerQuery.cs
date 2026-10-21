using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Rapadura.Core.Systems
{
    /// <summary>
    /// "Is the pointer over UI?" for gameplay input that shares the left mouse button with UI clicks
    /// (Attack, MoveClick). Covers uGUI via <see cref="EventSystem.IsPointerOverGameObject()"/> and
    /// UI Toolkit by picking every active <see cref="UIDocument"/> panel directly — scenes without an
    /// EventSystem (WorldMap.unity) never report UI Toolkit hits through the EventSystem.
    /// </summary>
    public static class UiPointerQuery
    {
        /// <param name="screenPosition">Screen space, bottom-left origin (like <c>Mouse.position</c>).</param>
        public static bool IsPointerOverUi(Vector2 screenPosition)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }

            foreach (UIDocument document in Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None))
            {
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
