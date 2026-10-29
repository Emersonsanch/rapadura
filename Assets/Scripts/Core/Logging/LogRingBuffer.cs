using System;
using System.Collections.Generic;

namespace Rapadura.Core.Logging
{
    /// <summary>One captured log line (level, wall-clock timestamp, category and text). Plain value, no allocation to copy.</summary>
    public readonly struct LogEntry
    {
        public readonly LogLevel Level;
        public readonly DateTime Timestamp;
        public readonly string Category;
        public readonly string Message;

        public LogEntry(LogLevel level, DateTime timestamp, string category, string message)
        {
            Level = level;
            Timestamp = timestamp;
            Category = category;
            Message = message;
        }
    }

    /// <summary>
    /// Fixed-capacity, overwrite-oldest buffer of <see cref="LogEntry"/> values (the debug overlay's
    /// rolling log). Preallocated once: adding never allocates. Every member takes a private lock, so
    /// the main thread can read while <c>Application.logMessageReceivedThreaded</c> or a worker thread
    /// calling <see cref="GameLogger"/> writes.
    /// </summary>
    public sealed class LogRingBuffer
    {
        public const int DefaultCapacity = 50;

        private readonly LogEntry[] _entries;
        private readonly object _gate = new object();
        private int _start;
        private int _count;
        private long _version;

        public LogRingBuffer(int capacity = DefaultCapacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be positive.");
            }

            _entries = new LogEntry[capacity];
        }

        public int Capacity => _entries.Length;

        public int Count
        {
            get
            {
                lock (_gate)
                {
                    return _count;
                }
            }
        }

        /// <summary>Increments on every <see cref="Add(in LogEntry)"/>/<see cref="Clear"/>: readers rebuild their text only when it changed.</summary>
        public long Version
        {
            get
            {
                lock (_gate)
                {
                    return _version;
                }
            }
        }

        public void Add(in LogEntry entry)
        {
            lock (_gate)
            {
                int capacity = _entries.Length;
                if (_count < capacity)
                {
                    _entries[(_start + _count) % capacity] = entry;
                    _count++;
                }
                else
                {
                    _entries[_start] = entry;
                    _start = (_start + 1) % capacity;
                }

                _version++;
            }
        }

        public void Add(LogLevel level, string category, string message)
        {
            Add(new LogEntry(level, DateTime.Now, category, message));
        }

        public void Clear()
        {
            lock (_gate)
            {
                Array.Clear(_entries, 0, _entries.Length);
                _start = 0;
                _count = 0;
                _version++;
            }
        }

        /// <summary>
        /// Clears <paramref name="destination"/> and fills it with the newest <paramref name="maxCount"/>
        /// entries, oldest first. Returns how many were copied.
        /// </summary>
        public int CopyNewest(List<LogEntry> destination, int maxCount)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            destination.Clear();
            lock (_gate)
            {
                int take = Math.Min(Math.Max(maxCount, 0), _count);
                int capacity = _entries.Length;
                for (int i = _count - take; i < _count; i++)
                {
                    destination.Add(_entries[(_start + i) % capacity]);
                }

                return take;
            }
        }
    }
}
