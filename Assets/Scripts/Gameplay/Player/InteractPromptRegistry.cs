namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Static "is there an [E] point in reach right now?" flag for the HUD's touch "Usar" button.
    /// Whoever shows an [E] prompt calls <see cref="Ping"/> (every frame while in reach, or once per
    /// hint with a hold as long as its hint cooldown); the HUD reads <see cref="IsAvailable"/> and
    /// highlights the button. No driver reports yet (the [E] prompts are floating texts spawned by each
    /// mechanics driver), so the HUD keeps the button visible but dimmed in touch mode until one does.
    /// Times are unscaled seconds, passed in so the rule stays EditMode-testable.
    /// </summary>
    public static class InteractPromptRegistry
    {
        /// <summary>Default hold of one <see cref="Ping"/>: a little over a frame-by-frame reporter's gap.</summary>
        public const float DefaultHoldSeconds = 0.25f;

        private static float _availableUntil = float.NegativeInfinity;

        /// <summary>An [E] point is in reach at <paramref name="now"/>, for <paramref name="holdSeconds"/> more.</summary>
        public static void Ping(float now, float holdSeconds = DefaultHoldSeconds)
        {
            float until = now + (holdSeconds > 0f ? holdSeconds : DefaultHoldSeconds);
            if (until > _availableUntil)
            {
                _availableUntil = until;
            }
        }

        /// <summary>Whether some [E] point reported itself in reach recently enough.</summary>
        public static bool IsAvailable(float now) => now <= _availableUntil;

        /// <summary>Forgets every report (scene change, tests).</summary>
        public static void Clear() => _availableUntil = float.NegativeInfinity;
    }
}
