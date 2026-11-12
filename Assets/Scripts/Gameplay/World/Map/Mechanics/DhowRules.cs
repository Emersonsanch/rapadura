namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The small dhows of the Mangues de Liyongo (Docs/Mapas100.md 30: "canais e dhows: barcos pequenos de aluguel (2 ouro) andam rápido nos
    /// canais; a maré fecha canais"), pure and server-runnable. The boatmen keep a dhow at each end of the Canal de Liyongo, at its west mouth in
    /// the shallow mangrove and at its east end by Hedammu's trail; [E] at a landing (from 4 m) pays 2 gold nuggets and sails the whole canal
    /// (~730 m) at 14 m/s (~52 s). When the reef's tide is low (<c>CostaTideRules</c>, the same sea) the canal runs dry and the dhows wait.
    /// <code>
    /// 2 landings, reach 4 m (<see cref="LimiarBoatRules.LandingAt"/>) · 2 gold · 14 m/s, 0.6 m over the water · no sailing at low tide · 3 s between rides
    /// </code>
    /// </summary>
    public static class DhowRules
    {
        public const float Speed = 14f;
        public const float Hover = 0.6f;
        public const int Fare = 2;
        public const float CooldownSeconds = 3f;

        /// <summary>Whether the canal carries a dhow in this phase of the tide.</summary>
        public static bool CanalOpen(CostaTidePhase tide) => tide != CostaTidePhase.Low;

        /// <summary>Whether a dhow sails: at a landing, the canal open, past the rest, and the fare in the purse.</summary>
        public static bool CanSail(int landing, CostaTidePhase tide, float sinceRide, int gold)
        {
            return landing >= 0 && CanalOpen(tide) && sinceRide >= CooldownSeconds && gold >= Fare;
        }
    }
}
