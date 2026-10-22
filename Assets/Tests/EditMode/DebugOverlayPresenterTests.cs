using System.Text;
using NUnit.Framework;
using Rapadura.Core.Debug;
using UnityEngine.InputSystem;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the debug overlay's pure parts: <see cref="DebugOverlayPresenter"/> text
    /// formatting, the frame-time additions to <see cref="DebugOverlayModel"/>, and the
    /// <see cref="DebugOverlayToggleGesture"/> key/three-finger-tap rules.
    /// </summary>
    public class DebugOverlayPresenterTests
    {
        private static DebugOverlaySnapshot FullSnapshot()
        {
            return new DebugOverlaySnapshot
            {
                Fps = 59.64f,
                FrameMilliseconds = 16.77f,
                PeakFrameMilliseconds = 33.3f,
                GcAllocatedInFrameBytes = 1536,
                TotalAllocatedBytes = 512L * 1024 * 1024,
                ManagedUsedBytes = 40L * 1024 * 1024,
                ActiveEnemies = 3,
                FlowState = "Encounter",
                PauseOwners = new[] { "DialogueController", "ShopController" },
                ExternalPause = false,
                Biome = "Forest",
                Weather = "Rain",
                Hour = 8.5f,
                Day = 2,
            };
        }

        // ------------------------------------------------------------------
        // Presenter.
        // ------------------------------------------------------------------

        [Test]
        public void Format_FullSnapshot_ProducesOneLinePerTopic()
        {
            string text = DebugOverlayPresenter.Format(FullSnapshot());

            string[] lines = text.Split('\n');
            Assert.AreEqual(7, lines.Length);
            Assert.AreEqual("FPS 59.6  16.8 ms (pico 33.3 ms)", lines[0]);
            Assert.AreEqual("GC/frame 1.5 KB", lines[1]);
            Assert.AreEqual("Mem 512.0 MB | managed 40.0 MB", lines[2]);
            Assert.AreEqual("Inimigos 3", lines[3]);
            Assert.AreEqual("Fluxo Encounter", lines[4]);
            Assert.AreEqual("Pausa (2) DialogueController, ShopController", lines[5]);
            Assert.AreEqual("Forest | 08:30 d2 | Rain", lines[6]);
        }

        [Test]
        public void Format_UnknownValues_ShowPlaceholders()
        {
            var snapshot = new DebugOverlaySnapshot
            {
                GcAllocatedInFrameBytes = -1,
                TotalAllocatedBytes = -1,
                ManagedUsedBytes = -1,
                ActiveEnemies = -1,
                Hour = -1f,
            };

            string[] lines = DebugOverlayPresenter.Format(snapshot).Split('\n');
            string dash = DebugOverlayPresenter.Unknown;

            Assert.AreEqual("GC/frame " + dash, lines[1]);
            Assert.AreEqual("Mem " + dash + " | managed " + dash, lines[2]);
            Assert.AreEqual("Inimigos " + dash, lines[3]);
            Assert.AreEqual("Fluxo " + dash, lines[4]);
            Assert.AreEqual("Pausa livre", lines[5]);
            Assert.AreEqual(dash + " | " + dash + " | " + dash, lines[6]);
        }

        [Test]
        public void Format_ReusesAndClearsTheGivenBuilder()
        {
            var builder = new StringBuilder("stale text");

            string first = DebugOverlayPresenter.Format(FullSnapshot(), builder);
            string second = DebugOverlayPresenter.Format(FullSnapshot(), builder);

            Assert.AreEqual(first, second);
            Assert.IsFalse(first.Contains("stale"));
        }

        [TestCase(0L, "0 B")]
        [TestCase(1023L, "1023 B")]
        [TestCase(1024L, "1.0 KB")]
        [TestCase(1536L, "1.5 KB")]
        [TestCase(10L * 1024 * 1024, "10.0 MB")]
        [TestCase(3L * 1024 * 1024 * 1024 / 2, "1.50 GB")]
        public void FormatBytes_PicksTheRightUnit(long bytes, string expected)
        {
            Assert.AreEqual(expected, DebugOverlayPresenter.FormatBytes(bytes));
        }

        [TestCase(0f, "00:00")]
        [TestCase(8.5f, "08:30")]
        [TestCase(13.25f, "13:15")]
        [TestCase(23.99f, "23:59")]
        [TestCase(24f, "00:00")]
        public void FormatClock_FormatsHoursAndMinutes(float hour, string expected)
        {
            Assert.AreEqual(expected, DebugOverlayPresenter.FormatClock(hour));
        }

        [Test]
        public void FormatPause_CoversFreeMenuAndOwners()
        {
            Assert.AreEqual("livre", DebugOverlayPresenter.FormatPause(null, false));
            Assert.AreEqual("menu", DebugOverlayPresenter.FormatPause(new string[0], true));
            Assert.AreEqual("(1) InventoryController + menu", DebugOverlayPresenter.FormatPause(new[] { "InventoryController" }, true));
            Assert.AreEqual("(2) ?, ShopController", DebugOverlayPresenter.FormatPause(new[] { null, "ShopController" }, false));
        }

        // ------------------------------------------------------------------
        // Model frame time.
        // ------------------------------------------------------------------

        [Test]
        public void Model_FrameTimeAndPeak_TrackTheSlowestFrame()
        {
            var model = new DebugOverlayModel();
            Assert.AreEqual(0f, model.SmoothedFrameMilliseconds);

            model.RegisterFrame(1f / 60f);
            model.RegisterFrame(0.05f);
            model.RegisterFrame(1f / 60f);

            Assert.Greater(model.SmoothedFrameMilliseconds, 16f);
            Assert.AreEqual(50f, model.ConsumePeakFrameMilliseconds(), 0.01f);
            Assert.AreEqual(0f, model.PeakFrameMilliseconds, "Consuming starts a new peak window.");
        }

        // ------------------------------------------------------------------
        // Toggle gesture.
        // ------------------------------------------------------------------

        [Test]
        public void ToggleKeys_AreF1AndBackquoteOnly()
        {
            Assert.IsTrue(DebugOverlayToggleGesture.IsToggleKey(Key.F1));
            Assert.IsTrue(DebugOverlayToggleGesture.IsToggleKey(Key.Backquote));
            Assert.IsFalse(DebugOverlayToggleGesture.IsToggleKey(Key.F2));
            Assert.IsFalse(DebugOverlayToggleGesture.IsToggleKey(Key.Escape));
        }

        [Test]
        public void QuickThreeFingerTap_TogglesWhenTheLastFingerLifts()
        {
            var gesture = new DebugOverlayToggleGesture();

            Assert.IsFalse(gesture.Step(1, 0.00f));
            Assert.IsFalse(gesture.Step(3, 0.05f));
            Assert.IsFalse(gesture.Step(2, 0.15f), "Still fingers down: not yet.");
            Assert.IsTrue(gesture.Step(0, 0.20f));
            Assert.IsFalse(gesture.IsTracking);
        }

        [Test]
        public void ThreeFingerHold_LongerThanTheTapWindow_DoesNotToggle()
        {
            var gesture = new DebugOverlayToggleGesture();

            gesture.Step(3, 0f);
            gesture.Step(3, 0.4f);

            Assert.IsFalse(gesture.Step(0, DebugOverlayToggleGesture.MaxTapSeconds + 0.1f));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        public void TapWithOtherFingerCounts_DoesNotToggle(int fingers)
        {
            var gesture = new DebugOverlayToggleGesture();

            gesture.Step(fingers, 0f);

            Assert.IsFalse(gesture.Step(0, 0.1f));
        }

        [Test]
        public void NoFingers_NeverToggles_AndEachTapCountsOnce()
        {
            var gesture = new DebugOverlayToggleGesture();
            Assert.IsFalse(gesture.Step(0, 0f));

            gesture.Step(3, 1f);
            Assert.IsTrue(gesture.Step(0, 1.1f));
            Assert.IsFalse(gesture.Step(0, 1.2f), "The same tap must not toggle twice.");

            gesture.Step(3, 2f);
            Assert.IsTrue(gesture.Step(0, 2.2f), "A second tap toggles again.");
        }

        [Test]
        public void Reset_DropsAGestureInProgress()
        {
            var gesture = new DebugOverlayToggleGesture();
            gesture.Step(3, 0f);

            gesture.Reset();

            Assert.IsFalse(gesture.IsTracking);
            Assert.IsFalse(gesture.Step(0, 0.1f));
        }
    }
}
