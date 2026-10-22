using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Rapadura.Core.Debug
{
    /// <summary>
    /// Everything the debug overlay shows, sampled once per refresh by
    /// <see cref="DebugOverlayController"/>. Plain data so <see cref="DebugOverlayPresenter"/> can be
    /// unit-tested without a scene. "Unknown" values: negative numbers / null strings.
    /// </summary>
    public struct DebugOverlaySnapshot
    {
        public float Fps;
        public float FrameMilliseconds;
        public float PeakFrameMilliseconds;

        /// <summary>Bytes allocated by the GC in the last frame ("GC Allocated In Frame"); negative = recorder unavailable.</summary>
        public long GcAllocatedInFrameBytes;

        /// <summary>Profiler.GetTotalAllocatedMemoryLong; negative = unknown.</summary>
        public long TotalAllocatedBytes;

        /// <summary>Profiler.GetMonoUsedSizeLong (managed heap in use); negative = unknown.</summary>
        public long ManagedUsedBytes;

        /// <summary>Alive enemies currently enabled; negative = unknown.</summary>
        public int ActiveEnemies;

        /// <summary>GameFlowService state name; null = no flow service.</summary>
        public string FlowState;

        /// <summary>Names of the GameplayPause owners (may be null/empty).</summary>
        public IReadOnlyList<string> PauseOwners;

        /// <summary>The pause menu is pausing (tracked outside the owner list).</summary>
        public bool ExternalPause;

        /// <summary>Current biome name; null = no weather system.</summary>
        public string Biome;

        /// <summary>Current weather name; null = no weather system.</summary>
        public string Weather;

        /// <summary>In-game hour 0-24; negative = no time-of-day manager.</summary>
        public float Hour;

        public int Day;
    }

    /// <summary>
    /// Pure text formatting for the debug overlay (one compact line per topic, invariant culture so
    /// the numbers never depend on the device locale). Kept out of the MonoBehaviour for EditMode tests.
    /// </summary>
    public static class DebugOverlayPresenter
    {
        public const string Unknown = "—";

        /// <summary>Builds the overlay text into <paramref name="builder"/> (cleared first) and returns it as a string.</summary>
        public static string Format(in DebugOverlaySnapshot snapshot, StringBuilder builder = null)
        {
            builder ??= new StringBuilder(256);
            builder.Clear();

            builder.Append("FPS ").Append(snapshot.Fps.ToString("0.0", CultureInfo.InvariantCulture))
                .Append("  ").Append(snapshot.FrameMilliseconds.ToString("0.0", CultureInfo.InvariantCulture)).Append(" ms")
                .Append(" (pico ").Append(snapshot.PeakFrameMilliseconds.ToString("0.0", CultureInfo.InvariantCulture)).Append(" ms)")
                .Append('\n');

            builder.Append("GC/frame ")
                .Append(snapshot.GcAllocatedInFrameBytes < 0 ? Unknown : FormatBytes(snapshot.GcAllocatedInFrameBytes))
                .Append('\n');

            builder.Append("Mem ").Append(snapshot.TotalAllocatedBytes < 0 ? Unknown : FormatBytes(snapshot.TotalAllocatedBytes))
                .Append(" | managed ").Append(snapshot.ManagedUsedBytes < 0 ? Unknown : FormatBytes(snapshot.ManagedUsedBytes))
                .Append('\n');

            builder.Append("Inimigos ")
                .Append(snapshot.ActiveEnemies < 0 ? Unknown : snapshot.ActiveEnemies.ToString(CultureInfo.InvariantCulture))
                .Append('\n');

            builder.Append("Fluxo ").Append(string.IsNullOrEmpty(snapshot.FlowState) ? Unknown : snapshot.FlowState).Append('\n');

            builder.Append("Pausa ").Append(FormatPause(snapshot.PauseOwners, snapshot.ExternalPause)).Append('\n');

            builder.Append(string.IsNullOrEmpty(snapshot.Biome) ? Unknown : snapshot.Biome)
                .Append(" | ").Append(snapshot.Hour < 0f ? Unknown : FormatClock(snapshot.Hour))
                .Append(snapshot.Hour < 0f ? string.Empty : " d" + snapshot.Day.ToString(CultureInfo.InvariantCulture))
                .Append(" | ").Append(string.IsNullOrEmpty(snapshot.Weather) ? Unknown : snapshot.Weather);

            return builder.ToString();
        }

        /// <summary>"512 B", "1.5 KB", "12.0 MB", "1.25 GB".</summary>
        public static string FormatBytes(long bytes)
        {
            if (bytes < 0)
            {
                return Unknown;
            }

            const double kilo = 1024.0;
            if (bytes < 1024)
            {
                return bytes.ToString(CultureInfo.InvariantCulture) + " B";
            }

            if (bytes < 1024 * 1024)
            {
                return (bytes / kilo).ToString("0.0", CultureInfo.InvariantCulture) + " KB";
            }

            if (bytes < 1024L * 1024 * 1024)
            {
                return (bytes / (kilo * kilo)).ToString("0.0", CultureInfo.InvariantCulture) + " MB";
            }

            return (bytes / (kilo * kilo * kilo)).ToString("0.00", CultureInfo.InvariantCulture) + " GB";
        }

        /// <summary>In-game hour (0-24, fractional) as "HH:MM"; wraps 24 → 00:00.</summary>
        public static string FormatClock(float hour)
        {
            int totalMinutes = (int)System.Math.Floor(hour * 60.0 + 0.0001);
            totalMinutes = ((totalMinutes % (24 * 60)) + 24 * 60) % (24 * 60);
            int h = totalMinutes / 60;
            int m = totalMinutes % 60;
            return h.ToString("00", CultureInfo.InvariantCulture) + ":" + m.ToString("00", CultureInfo.InvariantCulture);
        }

        /// <summary>"livre", "menu" (pause menu only), "(2) Owner1, Owner2", "(1) Owner1 + menu".</summary>
        public static string FormatPause(IReadOnlyList<string> owners, bool externalPause)
        {
            int count = owners?.Count ?? 0;
            if (count == 0)
            {
                return externalPause ? "menu" : "livre";
            }

            var builder = new StringBuilder();
            builder.Append('(').Append(count.ToString(CultureInfo.InvariantCulture)).Append(") ");
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(string.IsNullOrEmpty(owners[i]) ? "?" : owners[i]);
            }

            if (externalPause)
            {
                builder.Append(" + menu");
            }

            return builder.ToString();
        }
    }
}
