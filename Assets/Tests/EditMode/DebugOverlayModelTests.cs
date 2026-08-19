using NUnit.Framework;
using Rapadura.Core.Debug;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the debug overlay's pure FPS smoothing/formatting logic
    /// (<see cref="DebugOverlayModel"/>), deliberately without touching
    /// <see cref="Rapadura.Core.Debug.DebugOverlayController"/> or any UIDocument/scene object.
    /// </summary>
    public class DebugOverlayModelTests
    {
        [Test]
        public void RegisterFrame_FirstSample_SetsSmoothedFpsDirectly()
        {
            var model = new DebugOverlayModel();

            float result = model.RegisterFrame(1f / 60f);

            Assert.AreEqual(60f, result, 0.01f);
            Assert.AreEqual(60f, model.SmoothedFps, 0.01f);
        }

        [Test]
        public void RegisterFrame_SecondSample_MovesTowardsNewValueGradually()
        {
            var model = new DebugOverlayModel();
            model.RegisterFrame(1f / 60f); // 60 fps

            float result = model.RegisterFrame(1f / 30f); // 30 fps sample

            Assert.Less(result, 60f);
            Assert.Greater(result, 30f);
        }

        [Test]
        public void RegisterFrame_WithZeroOrNegativeDeltaTime_IsIgnored()
        {
            var model = new DebugOverlayModel();
            model.RegisterFrame(1f / 60f);
            float before = model.SmoothedFps;

            model.RegisterFrame(0f);
            model.RegisterFrame(-1f);

            Assert.AreEqual(before, model.SmoothedFps, 0.001f);
        }

        [Test]
        public void Reset_ClearsSmoothedFpsBackToZero()
        {
            var model = new DebugOverlayModel();
            model.RegisterFrame(1f / 60f);

            model.Reset();

            Assert.AreEqual(0f, model.SmoothedFps);
        }

        [Test]
        public void FormatFps_ProducesExpectedText()
        {
            var model = new DebugOverlayModel();
            model.RegisterFrame(1f / 60f);

            string text = model.FormatFps();

            Assert.AreEqual("FPS: 60.0", text);
        }

        [Test]
        public void FormatMemory_ConvertsBytesToMegabytes()
        {
            string text = DebugOverlayModel.FormatMemory(10L * 1024 * 1024);

            Assert.AreEqual("Mem: 10.0 MB", text);
        }

        [Test]
        public void FormatMemory_WithZeroBytes_FormatsAsZero()
        {
            string text = DebugOverlayModel.FormatMemory(0L);

            Assert.AreEqual("Mem: 0.0 MB", text);
        }
    }
}
