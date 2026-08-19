using System.Globalization;

namespace Rapadura.Core.Debug
{
    /// <summary>
    /// Pure, engine-independent state/formatting for the debug overlay (Fase 10 → Qualidade).
    /// Kept separate from <see cref="DebugOverlayController"/> (the MonoBehaviour/UI Toolkit
    /// binding) so the FPS smoothing and text formatting can be unit-tested in EditMode without
    /// a live scene, a UIDocument, or a running Update loop.
    ///
    /// KNOWN LIMITATION: this overlay shows FPS and memory only. The task also asked for a
    /// rolling log of the last N `GameLogger` messages, but `GameLogger` (Core/Logging/GameLogger.cs)
    /// exposes no event/hook fired on new messages (and was intentionally left unedited — it's
    /// out of scope here). Without such a hook there is no way to observe log messages from
    /// outside `Debug.Log` itself, short of registering an `Application.logMessageReceived`
    /// callback, which would capture *all* Unity log output rather than specifically the
    /// GameLogger-formatted category/message pairs the task asked for. Wiring GameLogger with a
    /// proper `OnMessageLogged` event (or having this overlay subscribe to
    /// `Application.logMessageReceived` directly) is the natural follow-up once that decision is made.
    /// </summary>
    public class DebugOverlayModel
    {
        private const float SmoothingFactor = 0.1f;

        private float _smoothedFps;
        private bool _hasSample;

        /// <summary>Current exponentially-smoothed FPS estimate.</summary>
        public float SmoothedFps => _smoothedFps;

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
