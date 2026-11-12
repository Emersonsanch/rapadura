namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The slow time of the Arvoredo de Haripushpa (Docs/Mapas100.md 50, §3.3: "tempo lento: no arvoredo o relógio do jogo anda pela metade …
    /// só enquanto o primo está lá"), pure. In the grove of the tree-folk the game clock (TimeOfDayManager) runs at half its pace: a day lasts
    /// twice as long while the hero walks among them, and the pace comes back the moment the hero leaves. Only the clock: the hero, the beasts
    /// and every other timer keep their speed.
    /// <code>
    /// in the grove: clock × 0.5 · out: as it was
    /// </code>
    /// </summary>
    public static class GroveSlowTimeRules
    {
        public const float ClockFactor = 0.5f;

        /// <summary>The clock's pace in the grove, from the pace it had outside.</summary>
        public static float SlowedScale(float outsideScale) => outsideScale * ClockFactor;

        /// <summary>
        /// The pace to put back on leaving: the outside pace, unless something else changed the clock while the hero was in the grove (then
        /// that change stays).
        /// </summary>
        public static float RestoredScale(float current, float slowed, float outside) => System.Math.Abs(current - slowed) < 1e-5f ? outside : current;
    }
}
