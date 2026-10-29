using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Rapadura.Core.Debug;
using Rapadura.Core.Logging;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the debug overlay's rolling log: <see cref="LogRingBuffer"/> (bounded,
    /// overwrite-oldest, thread-safe), the <see cref="GameLogger.MessageLogged"/> hook and
    /// <see cref="DebugLogHistory"/> line formatting.
    /// </summary>
    public class LogRingBufferTests
    {
        private static readonly DateTime Noon = new DateTime(2026, 10, 29, 12, 34, 56);

        private static LogEntry Entry(string message, LogLevel level = LogLevel.Info)
        {
            return new LogEntry(level, Noon, "Test", message);
        }

        [Test]
        public void Constructor_RejectsNonPositiveCapacity()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new LogRingBuffer(0));
            Assert.AreEqual(LogRingBuffer.DefaultCapacity, new LogRingBuffer().Capacity);
        }

        [Test]
        public void CopyNewest_BelowCapacity_ReturnsAllOldestFirst()
        {
            var ring = new LogRingBuffer(5);
            ring.Add(Entry("a"));
            ring.Add(Entry("b"));
            ring.Add(Entry("c"));

            var copy = new List<LogEntry>();
            Assert.AreEqual(3, ring.CopyNewest(copy, 10));
            CollectionAssert.AreEqual(new[] { "a", "b", "c" }, copy.ConvertAll(e => e.Message));
        }

        [Test]
        public void Add_PastCapacity_DropsOldest()
        {
            var ring = new LogRingBuffer(3);
            for (int i = 0; i < 7; i++)
            {
                ring.Add(Entry(i.ToString()));
            }

            Assert.AreEqual(3, ring.Count);
            var copy = new List<LogEntry>();
            ring.CopyNewest(copy, 3);
            CollectionAssert.AreEqual(new[] { "4", "5", "6" }, copy.ConvertAll(e => e.Message));
        }

        [Test]
        public void CopyNewest_LimitsToMaxCount_KeepsNewest_AndClearsDestination()
        {
            var ring = new LogRingBuffer(4);
            for (int i = 0; i < 6; i++)
            {
                ring.Add(Entry(i.ToString()));
            }

            var copy = new List<LogEntry> { Entry("stale") };
            Assert.AreEqual(2, ring.CopyNewest(copy, 2));
            CollectionAssert.AreEqual(new[] { "4", "5" }, copy.ConvertAll(e => e.Message));
            Assert.AreEqual(0, ring.CopyNewest(copy, -1));
            Assert.AreEqual(0, copy.Count);
        }

        [Test]
        public void Version_ChangesOnAddAndClear_KeepsLevelAndTimestamp()
        {
            var ring = new LogRingBuffer(2);
            long v0 = ring.Version;
            ring.Add(Entry("x", LogLevel.Warning));
            Assert.Greater(ring.Version, v0);

            var copy = new List<LogEntry>();
            ring.CopyNewest(copy, 1);
            Assert.AreEqual(LogLevel.Warning, copy[0].Level);
            Assert.AreEqual(Noon, copy[0].Timestamp);

            long v1 = ring.Version;
            ring.Clear();
            Assert.Greater(ring.Version, v1);
            Assert.AreEqual(0, ring.Count);
        }

        [Test]
        public void ConcurrentWriters_NeverExceedCapacity_AndLoseNoVersionTicks()
        {
            var ring = new LogRingBuffer(50);
            const int perThread = 2000;
            var threads = new Thread[4];
            for (int t = 0; t < threads.Length; t++)
            {
                threads[t] = new Thread(() =>
                {
                    for (int i = 0; i < perThread; i++)
                    {
                        ring.Add(LogLevel.Debug, "T", "m");
                    }
                });
                threads[t].Start();
            }

            var copy = new List<LogEntry>();
            for (int i = 0; i < 200; i++)
            {
                ring.CopyNewest(copy, 50); // reader on this thread while writers run
                Assert.LessOrEqual(copy.Count, 50);
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            Assert.AreEqual(50, ring.Count);
            Assert.AreEqual(threads.Length * perThread, ring.Version);
        }

        [Test]
        public void GameLogger_MessageLogged_FiresWithLevelAndRespectsMinimumLevel()
        {
            var seen = new List<string>();
            Action<LogLevel, string, string> handler = (level, category, message) => seen.Add(level + "|" + category + "|" + message);
            LogLevel previousMinimum = GameLogger.MinimumLevel;
            GameLogger.MessageLogged += handler;
            try
            {
                GameLogger.MinimumLevel = LogLevel.Warning;
                GameLogger.Info("Cat", "filtered out");
                GameLogger.Warning("Cat", "kept");
            }
            finally
            {
                GameLogger.MessageLogged -= handler;
                GameLogger.MinimumLevel = previousMinimum;
            }

            CollectionAssert.AreEqual(new[] { "Warning|Cat|kept" }, seen);
        }

        [Test]
        public void GameLogger_ThrowingSubscriber_DoesNotBreakCaller()
        {
            Action<LogLevel, string, string> handler = (level, category, message) => throw new InvalidOperationException("boom");
            GameLogger.MessageLogged += handler;
            try
            {
                Assert.DoesNotThrow(() => GameLogger.Info("Cat", "hello"));
            }
            finally
            {
                GameLogger.MessageLogged -= handler;
            }

        }

        [Test]
        public void AppendLines_FormatsTimestampLevelCategory_FirstLineOnly_Truncated()
        {
            var entries = new List<LogEntry>
            {
                new LogEntry(LogLevel.Warning, Noon, "Save", "slot 1 missing"),
                new LogEntry(LogLevel.Error, Noon, null, "first\r\nsecond"),
                new LogEntry(LogLevel.Debug, Noon, "Long", new string('x', 20)),
            };

            var builder = new StringBuilder();
            DebugLogHistory.AppendLines(builder, entries, 10);

            Assert.AreEqual(
                "12:34:56 W [Save] slot 1 mis...\n" +
                "12:34:56 E [?] first\n" +
                "12:34:56 D [Long] xxxxxxxxxx...",
                builder.ToString());
        }
    }
}
