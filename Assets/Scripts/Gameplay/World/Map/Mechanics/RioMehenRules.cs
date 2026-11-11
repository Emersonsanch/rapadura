namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The turning coils of Mehen (Docs/Mapas100.md, map 74: "os anéis giram a cada 30 s; as aberturas se alinham por pouco
    /// tempo"), pure in the server's clock. One ring of light turns around the Anel Quieto over the scale wall of radius 144 m,
    /// with two openings opposite each other. Eight gates cross that wall, 45° apart (<c>OverworldLayouts.MehenGates</c>): a
    /// gate works only while an opening is held in front of it. The wall is low relief that can also be walked round, so a
    /// shut gate costs a wait or a detour, and the neighbouring gate (about 14 s on foot along the trough) opens 30 s later.
    /// <code>
    /// step 30 s: the ring turns 45° for 18 s, then holds for 12 s (the openings stand on gates step and step + 4)
    /// each gate works 12 s out of every 120 s (a wait of at most 108 s); some pair of gates works every 30 s
    /// the crossing flies at 16 m/s over the 96 m between the troughs (6 s) · arc 5 m
    /// </code>
    /// </summary>
    public static class RioMehenRules
    {
        public const int Gates = 8;
        public const float StepSeconds = 30f;
        public const float TurnSeconds = 18f;
        public const float DegreesPerStep = 45f;
        public const float CrossSpeed = 16f;
        public const float CrossArc = 5f;

        /// <summary>Openings repeat after this many steps: two openings on eight gates.</summary>
        private const int StepsPerCycle = Gates / 2;

        /// <summary>The whole steps elapsed at <paramref name="time"/> (seconds of the shared clock).</summary>
        private static long Step(double time) => (long)System.Math.Floor(time / StepSeconds);

        private static float IntoStep(double time) => (float)(time - Step(time) * (double)StepSeconds);

        /// <summary>True while the ring holds still (the last 12 s of each step).</summary>
        public static bool IsHolding(double time) => IntoStep(time) >= TurnSeconds;

        /// <summary>Gate a held opening stands on during this step, 0–3 (the other opening stands on that + 4).</summary>
        public static int ActivePair(double time)
        {
            long pair = Step(time) % StepsPerCycle;
            return (int)(pair < 0 ? pair + StepsPerCycle : pair);
        }

        public static bool IsGateOpen(int gate, double time) => IsHolding(time) && Normalize(gate) % StepsPerCycle == ActivePair(time);

        /// <summary>Seconds until <paramref name="gate"/> next works: 0 while it does.</summary>
        public static float SecondsUntilOpen(int gate, double time)
        {
            if (IsGateOpen(gate, time))
            {
                return 0f;
            }

            int wanted = Normalize(gate) % StepsPerCycle;
            int steps = (wanted - ActivePair(time) + StepsPerCycle) % StepsPerCycle;
            return steps * StepSeconds + TurnSeconds - IntoStep(time);
        }

        /// <summary>Bearing (degrees, 0–180 is enough: the other opening is opposite) of the first opening right now, for the drawing.</summary>
        public static float OpeningBearing(double time)
        {
            float turned = IntoStep(time) >= TurnSeconds ? 1f : IntoStep(time) / TurnSeconds;
            return (ActivePair(time) - 1 + turned) * DegreesPerStep;
        }

        public static float CrossSeconds(float metres) => metres / CrossSpeed;

        private static int Normalize(int gate) => ((gate % Gates) + Gates) % Gates;
    }
}
