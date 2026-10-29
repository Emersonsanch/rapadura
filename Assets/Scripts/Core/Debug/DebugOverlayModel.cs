using System.Globalization;

namespace Rapadura.Core.Debug
{
    /// <summary>
    /// Pure, engine-independent state/formatting for the debug overlay (Fase 10 → Qualidade).
    /// Kept separate from <see cref="DebugOverlayController"/> (the MonoBehaviour/UI Toolkit
    /// binding) so the FPS smoothing and text formatting can be unit-tested in EditMode without
    /// a live scene, a UIDocument, or a running Update loop.
    ///
    /// The full multi-line overlay text (frame time, GC/frame, enemies, flow, pause, world) is built by
    /// <see cref="DebugOverlayPresenter"/> from a <see cref="DebugOverlaySnapshot"/>.
    ///
    /// The rolling log of recent GameLogger messages lives in <see cref="DebugLogHistory"/> (fed by
    /// <c>GameLogger.MessageLogged</c>) and is drawn under the stats by the controller.
    /// </summary>
    public class DebugOverlayModel
    {
        private const float SmoothingFactor = 0.1f;

        private float _smoothedFps;
        private bool _hasSample;
        private float _peakDeltaSeconds;

        /// <summary>Current exponentially-smoothed FPS estimate.</summary>
        public float SmoothedFps => _smoothedFps;

        /// <summary>Frame time implied by <see cref="SmoothedFps"/>, in milliseconds (0 before the first sample).</summary>
        public float SmoothedFrameMilliseconds => _smoothedFps > 0f ? 1000f / _smoothedFps : 0f;

        /// <summary>Longest frame seen since the last <see cref="ConsumePeakFrameMilliseconds"/>, in milliseconds.</summary>
        public float PeakFrameMilliseconds => _peakDeltaSeconds * 1000f;

        /// <summary>Returns <see cref="PeakFrameMilliseconds"/> and starts a new peak window (call once per overlay refresh).</summary>
        public float ConsumePeakFrameMilliseconds()
        {
            float peak = PeakFrameMilliseconds;
            _peakDeltaSeconds = 0f;
            return peak;
        }

        /// <summary>
        /// Feeds one frame's delta time into the smoothing average. Call once per frame with
        /// <c>Time.unscaledDeltaTime</c>. Returns the updated smoothed FPS.
        /// </summary>
        public float RegisterFrame(float deltaTimeSeconds)
        {
            if (deltaTimeSeconds <= 0f)
            {
                return _smoothedFps;
            }

            float instantFps = 1f / deltaTimeSeconds;
            if (deltaTimeSeconds > _peakDeltaSeconds)
            {
                _peakDeltaSeconds = deltaTimeSeconds;
            }

            if (!_hasSample)
            {
                _smoothedFps = instantFps;
                _hasSample = true;
            }
            else
            {
                _smoothedFps += (instantFps - _smoothedFps) * SmoothingFactor;
            }

            return _smoothedFps;
        }

        /// <summary>Resets the smoothing state (e.g. on overlay re-enable).</summary>
        public void Reset()
        {
            _smoothedFps = 0f;
            _hasSample = false;
            _peakDeltaSeconds = 0f;
        }

        /// <summary>Formats the current smoothed FPS as display text, e.g. "FPS: 59.6".</summary>
        public string FormatFps()
        {
            return "FPS: " + _smoothedFps.ToString("0.0", CultureInfo.InvariantCulture);
        }

        /// <summary>Formats a managed-heap byte count (e.g. from <c>GC.GetTotalMemory</c>) as human-readable MB text.</summary>
        public static string FormatMemory(long totalAllocatedBytes)
        {
            double megabytes = totalAllocatedBytes / (1024.0 * 1024.0);
            return "Mem: " + megabytes.ToString("0.0", CultureInfo.InvariantCulture) + " MB";
        }
    }
}
