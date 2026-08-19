using NUnit.Framework;
using Rapadura.UI.HUD;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="HudBarMath"/> — the pure fill-percentage/label logic behind
    /// the HUD bars. Deliberately does not touch <c>UIDocument</c>/<c>HudController</c>, since a
    /// real UI Toolkit panel needs <c>PanelSettings</c> that only exist once the project has been
    /// opened in the Editor (see TODO.md Fase 1 note) — the math is tested in isolation instead.
    /// </summary>
    public class HudBarMathTests
    {
        [Test]
        public void ComputeFillFraction_HalfValue_ReturnsHalf()
        {
            Assert.AreEqual(0.5f, HudBarMath.ComputeFillFraction(50f, 100f), 0.0001f);
        }

        [Test]
        public void ComputeFillFraction_FullValue_ReturnsOne()
        {
            Assert.AreEqual(1f, HudBarMath.ComputeFillFraction(100f, 100f), 0.0001f);
        }

        [Test]
        public void ComputeFillFraction_ZeroValue_ReturnsZero()
        {
            Assert.AreEqual(0f, HudBarMath.ComputeFillFraction(0f, 100f), 0.0001f);
        }

        [Test]
        public void ComputeFillFraction_CurrentAboveMax_ClampsToOne()
        {
            Assert.AreEqual(1f, HudBarMath.ComputeFillFraction(150f, 100f), 0.0001f);
        }

        [Test]
        public void ComputeFillFraction_NegativeCurrent_ClampsToZero()
        {
            Assert.AreEqual(0f, HudBarMath.ComputeFillFraction(-10f, 100f), 0.0001f);
        }

        [Test]
        public void ComputeFillFraction_MaxIsZero_ReturnsZero_DoesNotDivideByZero()
        {
            Assert.AreEqual(0f, HudBarMath.ComputeFillFraction(10f, 0f), 0.0001f);
        }

        [Test]
        public void ComputeFillFraction_MaxIsNegative_ReturnsZero()
        {
            Assert.AreEqual(0f, HudBarMath.ComputeFillFraction(10f, -5f), 0.0001f);
        }

        [Test]
        public void ComputeFillPercent_QuarterValue_Returns25()
        {
            Assert.AreEqual(25f, HudBarMath.ComputeFillPercent(25f, 100f), 0.0001f);
        }

        [Test]
        public void IsLow_BelowThreshold_ReturnsTrue()
        {
            Assert.IsTrue(HudBarMath.IsLow(10f, 100f, 0.25f));
        }

        [Test]
        public void IsLow_AtThreshold_ReturnsTrue()
        {
            Assert.IsTrue(HudBarMath.IsLow(25f, 100f, 0.25f));
        }

        [Test]
        public void IsLow_AboveThreshold_ReturnsFalse()
        {
            Assert.IsFalse(HudBarMath.IsLow(50f, 100f, 0.25f));
        }

        [Test]
        public void IsLow_DefaultThreshold_MatchesQuarterBoundary()
        {
            Assert.IsTrue(HudBarMath.IsLow(24f, 100f));
            Assert.IsFalse(HudBarMath.IsLow(26f, 100f));
        }

        [Test]
        public void FormatValueLabel_RoundsToWholeNumbers()
        {
            Assert.AreEqual("50/100", HudBarMath.FormatValueLabel(50.4f, 100f));
            Assert.AreEqual("51/100", HudBarMath.FormatValueLabel(50.6f, 100f));
        }

        [Test]
        public void FormatValueLabel_NegativeCurrent_ClampsDisplayToZero()
        {
            Assert.AreEqual("0/100", HudBarMath.FormatValueLabel(-5f, 100f));
        }
    }
}
