namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// "The wind turns", "the channels change": which of a capital event's places a step of the window falls on (the Vendaval of Farnkesh,
    /// <see cref="FarnkeshGaleRules"/>; the Grande Tremor of Hinokura, <see cref="HinokuraGreatTremorRules"/>). Pure: the same window gives the
    /// same order to everyone (a server only needs the window index), a new window gives another, and two steps in a row never fall on the same
    /// place, so the hero always has to run.
    /// </summary>
    public static class CityEventTurns
    {
        /// <summary>The place (0…<paramref name="places"/>−1) step <paramref name="step"/> of window <paramref name="windowIndex"/> falls on.</summary>
        public static int SideOf(long windowIndex, int step, int places)
        {
            if (places <= 1)
            {
                return 0;
            }

            long side = Mod(windowIndex, places);
            for (int i = 0; i < step; i++)
            {
                // A turn of 1 or of places − 1 (never 0): with three places, "the next one" or "the one after".
                long turn = Mod(windowIndex + i, 2) == 0L ? 1L : places - 1L;
                side = Mod(side + turn, places);
            }

            return (int)side;
        }

        private static long Mod(long value, long by)
        {
            long r = value % by;
            return r < 0 ? r + by : r;
        }
    }
}
