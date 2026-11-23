namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Amirani's chain at night (Docs/Mapas100.md 52: "Noite: a corrente brilha onde a luz já subiu; neve forte"; "Falta: o brilho noturno"),
    /// pure, on top of <see cref="NartesChainRules"/>. The highest ring the cousin ever reached stays in the save. At night (the game clock) the
    /// links up to that ring glow: between two lit rings the glow holds the hero and the gusts push at half; above the last lit ring the
    /// mountain's snow is strong and they push harder. By day nothing changes.
    /// <code>
    /// day: push ×1 · night, between two lit rings (ring below the hero &lt; best ring ever): ×0.5 · night, above them: ×1.3 (4.55 m/s, still
    /// under the 5.5 m/s run: walking against it holds) · the best ring is saved when a new one is reached (an event, not a tick)
    /// </code>
    /// Decision (2026-11-23): the night rewards the climb already done by day: the known stretch is easier, the new stretch is the real test.
    /// </summary>
    public static class ChainNightGlowRules
    {
        public const float GlowFactor = 0.5f;
        public const float SnowFactor = 1.3f;

        /// <summary>The save mark with the best ring ever (stored as ring + 1; no mark = never climbed).</summary>
        public const string BestRingKey = "nartes.chain.best_ring";

        /// <summary>The save mark of the cousin's line on the first night climb along a lit stretch (set once, on that event).</summary>
        public const string FirstGlowKey = "nartes.chain.first_glow";

        /// <summary>The best ring kept in a mark (−1 when the chain was never climbed).</summary>
        public static int BestFromMark(long mark) => mark > 0L ? (int)(mark - 1L) : -1;

        public static long MarkOf(int ring) => ring + 1L;

        /// <summary>Whether ring <paramref name="ring"/> glows tonight.</summary>
        public static bool Lit(int ring, int bestEver, bool night) => night && ring >= 0 && ring <= bestEver;

        /// <summary>Whether a hero whose ring below is <paramref name="safeAnchor"/> walks between two lit rings.</summary>
        public static bool OnLitStretch(int safeAnchor, int bestEver) => safeAnchor >= 0 && safeAnchor < bestEver;

        /// <summary>The factor on the gust's push.</summary>
        public static float PushFactor(bool night, int safeAnchor, int bestEver)
        {
            if (!night)
            {
                return 1f;
            }

            return OnLitStretch(safeAnchor, bestEver) ? GlowFactor : SnowFactor;
        }

        /// <summary>Whether <paramref name="reached"/> beats the saved best (the moment to write the mark).</summary>
        public static bool IsNewBest(int savedBest, int reached) => reached > savedBest;
    }
}
