using NUnit.Framework;
using Rapadura.UI.Common;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="TooltipPositioning"/> — pure math, no UIDocument/panel
    /// required, same rationale as <c>HudBarMathTests</c>.
    /// </summary>
    public class TooltipPositioningTests
    {
        [Test]
        public void ComputeFollowMousePosition_AppliesOffsetWhenFarFromEdges()
        {
            Vector2 result = TooltipPositioning.ComputeFollowMousePosition(
                mousePosition: new Vector2(100f, 100f),
                tooltipSize: new Vector2(50f, 20f),
                panelSize: new Vector2(1920f, 1080f),
                cursorOffset: new Vector2(16f, 16f));

            Assert.AreEqual(116f, result.x, 0.001f);
            Assert.AreEqual(116f, result.y, 0.001f);
        }

        [Test]
        public void ComputeFollowMousePosition_ClampsToRightEdge()
        {
            Vector2 result = TooltipPositioning.ComputeFollowMousePosition(
                mousePosition: new Vector2(1910f, 100f),
                tooltipSize: new Vector2(50f, 20f),
                panelSize: new Vector2(1920f, 1080f),
                cursorOffset: new Vector2(16f, 16f));

            Assert.AreEqual(1870f, result.x, 0.001f); // 1920 - 50
        }

        [Test]
        public void ComputeFollowMousePosition_ClampsToBottomEdge()
        {
            Vector2 result = TooltipPositioning.ComputeFollowMousePosition(
                mousePosition: new Vector2(100f, 1070f),
                tooltipSize: new Vector2(50f, 20f),
                panelSize: new Vector2(1920f, 1080f),
                cursorOffset: new Vector2(16f, 16f));

            Assert.AreEqual(1060f, result.y, 0.001f); // 1080 - 20
        }

        [Test]
        public void ComputeFollowMousePosition_NeverGoesNegative()
        {
            Vector2 result = TooltipPositioning.ComputeFollowMousePosition(
                mousePosition: new Vector2(0f, 0f),
                tooltipSize: new Vector2(300f, 300f),
                panelSize: new Vector2(200f, 200f),
                cursorOffset: new Vector2(-50f, -50f));

            Assert.GreaterOrEqual(result.x, 0f);
            Assert.GreaterOrEqual(result.y, 0f);
        }

        [Test]
        public void ComputeAnchoredPosition_PlacesBelowAnchorWhenItFits()
        {
            Rect anchor = new Rect(100f, 100f, 80f, 30f);

            Vector2 result = TooltipPositioning.ComputeAnchoredPosition(
                anchorWorldBound: anchor,
                tooltipSize: new Vector2(60f, 20f),
                panelSize: new Vector2(1920f, 1080f),
                spacing: 8f);

            Assert.AreEqual(138f, result.y, 0.001f); // 100 + 30 + 8
            Assert.AreEqual(110f, result.x, 0.001f); // centered: 100 + (80-60)/2
        }

        [Test]
        public void ComputeAnchoredPosition_FlipsAboveWhenBelowOverflowsPanel()
        {
            Rect anchor = new Rect(100f, 1050f, 80f, 30f);

            Vector2 result = TooltipPositioning.ComputeAnchoredPosition(
                anchorWorldBound: anchor,
                tooltipSize: new Vector2(60f, 20f),
                panelSize: new Vector2(1920f, 1080f),
                spacing: 8f);

            // Below would be 1050 + 30 + 8 + 20 = 1108 > 1080, so it should flip above.
            Assert.AreEqual(1022f, result.y, 0.001f); // 1050 - 8 - 20
        }

        [Test]
        public void ComputeAnchoredPosition_ClampsHorizontallyWhenAnchorNearEdge()
        {
            Rect anchor = new Rect(1900f, 100f, 80f, 30f);

            Vector2 result = TooltipPositioning.ComputeAnchoredPosition(
                anchorWorldBound: anchor,
                tooltipSize: new Vector2(60f, 20f),
                panelSize: new Vector2(1920f, 1080f),
                spacing: 8f);

            Assert.AreEqual(1860f, result.x, 0.001f); // 1920 - 60
        }

        [Test]
        public void ClampToPanel_ReturnsZeroWhenTooltipLargerThanPanel()
        {
            Vector2 result = TooltipPositioning.ClampToPanel(
                desiredPosition: new Vector2(500f, 500f),
                tooltipSize: new Vector2(400f, 400f),
                panelSize: new Vector2(300f, 300f));

            Assert.AreEqual(0f, result.x, 0.001f);
            Assert.AreEqual(0f, result.y, 0.001f);
        }
    }
}
