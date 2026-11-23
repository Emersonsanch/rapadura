using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>What a tick of <see cref="CeuPorchSession"/> brought.</summary>
    public enum CeuPorchEvent
    {
        None = 0,
        NextLine = 1,
        Finished = 2,
        /// <summary>The hero walked off or was hurt.</summary>
        Interrupted = 3,
    }

    /// <summary>
    /// "Sentar e contar" (Docs/Mapas100.md, map 100), pure: beside Vô Jorge's empty rocking chair the family tells one story a day (the server's UTC day picks it). The
    /// controls are never taken: walking away or being hurt simply ends the sitting, and while it lasts the hero rests.
    /// <code>
    /// 5 stories of 4 lines, 6 s a line (24 s), plus one closing line at night · start within 4 m, stay within 5 m
    /// rest: 3% of max health per second (about 72% over a story)
    /// kept (2026-11-23, "guardar a história do dia no save"): a sitting cut short is saved with its line, and sitting again the same UTC day picks the
    /// story up there; each story told to the end is kept in a mask of five, and the fifth one heard brings Vô Jorge's memory once
    /// </code>
    /// </summary>
    public static class CeuPorchRules
    {
        public const int Stories = 5;
        public const int LinesPerStory = 4;
        public const float LineSeconds = 6f;
        public const float SeatReach = 4f;
        public const float StayReach = 5f;
        public const float RestFractionPerSecond = 0.03f;
        public const long SecondsPerDay = 86400;

        /// <summary>The story (0–4) of the UTC day of <paramref name="unixSeconds"/>.</summary>
        public static int StoryIndex(long unixSeconds)
        {
            long day = unixSeconds / SecondsPerDay;
            if (unixSeconds % SecondsPerDay != 0 && unixSeconds < 0)
            {
                day--;
            }

            long story = day % Stories;
            return (int)(story < 0 ? story + Stories : story);
        }

        public static int LineCount(bool isNight) => LinesPerStory + (isNight ? 1 : 0);

        /// <summary>The health <paramref name="seconds"/> of sitting give back, never past the max.</summary>
        public static float RestAmount(float current, float max, float seconds)
        {
            if (max <= 0f || current >= max)
            {
                return 0f;
            }

            return Mathf.Min(max - current, max * RestFractionPerSecond * Mathf.Max(0f, seconds));
        }

        public const string ResumeMarkKey = "ceu.porch.resume";
        public const string HeardMarkKey = "ceu.porch.heard";
        public const string AllHeardMarkKey = "ceu.porch.all_heard";
        public const int AllHeardMask = (1 << Stories) - 1;

        /// <summary>The UTC day number of <paramref name="unixSeconds"/> (floored, also before 1970).</summary>
        public static long UtcDay(long unixSeconds)
        {
            long day = unixSeconds / SecondsPerDay;
            if (unixSeconds % SecondsPerDay != 0 && unixSeconds < 0)
            {
                day--;
            }

            return day;
        }

        /// <summary>The saved mark of a sitting cut short on <paramref name="utcDay"/> at <paramref name="line"/> (never 0, so 0 means "nothing kept").</summary>
        public static long EncodeResume(long utcDay, int line) => utcDay * (LinesPerStory + 1) + Mathf.Clamp(line, 0, LinesPerStory) + 1;

        /// <summary>The line to start from: the one kept from the same UTC day, or 0 (another day, nothing kept, or the story was already over).</summary>
        public static int ResumeLine(long mark, long utcDay)
        {
            if (mark <= 0L)
            {
                return 0;
            }

            long raw = mark - 1L;
            long day = raw / (LinesPerStory + 1);
            int line = (int)(raw % (LinesPerStory + 1));
            return day == utcDay && line > 0 && line < LinesPerStory ? line : 0;
        }

        public static int WithStory(int mask, int story) => story >= 0 && story < Stories ? mask | (1 << story) : mask;

        public static bool Heard(int mask, int story) => story >= 0 && story < Stories && (mask & (1 << story)) != 0;

        public static int HeardCount(int mask)
        {
            int count = 0;
            for (int i = 0; i < Stories; i++)
            {
                count += Heard(mask, i) ? 1 : 0;
            }

            return count;
        }

        public static bool AllHeard(int mask) => (mask & AllHeardMask) == AllHeardMask;

        public static bool WithinSeat(Vector2 chair, Vector2 hero) => (chair - hero).sqrMagnitude <= SeatReach * SeatReach;

        public static bool WithinStay(Vector2 chair, Vector2 hero) => (chair - hero).sqrMagnitude <= StayReach * StayReach;
    }

    /// <summary>One sitting on the porch: which story, which line. No Unity object: it can run on the server.</summary>
    public sealed class CeuPorchSession
    {
        private float _clock;
        private int _lineCount;

        public bool IsSitting { get; private set; }

        public int Story { get; private set; }

        /// <summary>The line being told; past the story's own lines it is the night's closing line.</summary>
        public int Line { get; private set; }

        public void Begin(int story, bool isNight) => Begin(story, isNight, 0);

        /// <summary>Sits down at <paramref name="startLine"/> of the story (a kept sitting picks up where it stopped).</summary>
        public void Begin(int story, bool isNight, int startLine)
        {
            IsSitting = true;
            Story = story;
            Line = Mathf.Clamp(startLine, 0, CeuPorchRules.LinesPerStory - 1);
            _clock = 0f;
            _lineCount = CeuPorchRules.LineCount(isNight);
        }

        public void Cancel() => IsSitting = false;

        public CeuPorchEvent Tick(float deltaTime, bool stayed, bool hurt)
        {
            if (!IsSitting)
            {
                return CeuPorchEvent.None;
            }

            if (!stayed || hurt)
            {
                IsSitting = false;
                return CeuPorchEvent.Interrupted;
            }

            _clock += deltaTime;
            if (_clock < CeuPorchRules.LineSeconds)
            {
                return CeuPorchEvent.None;
            }

            _clock -= CeuPorchRules.LineSeconds;
            Line++;
            if (Line < _lineCount)
            {
                return CeuPorchEvent.NextLine;
            }

            IsSitting = false;
            return CeuPorchEvent.Finished;
        }
    }
}
