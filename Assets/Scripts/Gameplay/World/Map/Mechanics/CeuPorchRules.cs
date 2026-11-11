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

        public void Begin(int story, bool isNight)
        {
            IsSitting = true;
            Story = story;
            Line = 0;
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
