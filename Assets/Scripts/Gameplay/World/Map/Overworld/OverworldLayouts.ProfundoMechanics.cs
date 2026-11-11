using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>The five gates of the pits of the Fortaleza da Última Lâmpada (83, <c>ProfundoGateRules</c>), in the region's own coordinates.</summary>
    public static partial class OverworldLayouts
    {
        /// <summary>Radius of the gates' plaza spots: just in front of the leaves set into the rampart (rampart 18 m, half thickness 1.3 m).</summary>
        public const float ProfundoGateSpotRadius = 14.8f;

        /// <summary>
        /// The gates, in the order of the town's <c>CoveGate</c> variants (150°, 120°, 90°, 60°, 30° round the plaza): Fumaça (84), Fogo Devorador (85),
        /// Vento Destruidor (86), Lodo (87), Treva (88). Arrivals stand clear of the steles, the oil cisterns and the lamp posts; landings are on each
        /// post's own road, 24 m out. Built on first use: it reads the posts of another part of this partial class, whose initialisers may run later.
        /// </summary>
        public static ProfundoGate[] ProfundoGates => _profundoGates ??= new[]
        {
            new ProfundoGate("fumaca", GateSpot(150f), new Vector2(-9.5f, 5.5f), LampadaFumacaOffset, LampadaFumacaOffset + new Vector2(-24f, 0f)),
            new ProfundoGate("fogo", GateSpot(120f), new Vector2(-4.5f, 7.8f), LampadaFogoOffset, LampadaFogoOffset + new Vector2(0f, 24f)),
            new ProfundoGate("vento", GateSpot(90f), new Vector2(0f, 11.5f), LampadaVentoOffset, LampadaVentoOffset + new Vector2(0f, -24f)),
            new ProfundoGate("lodo", GateSpot(60f), new Vector2(4.5f, 7.8f), LampadaLodoOffset, LampadaLodoOffset + new Vector2(-24f, 0f)),
            new ProfundoGate("treva", GateSpot(30f), new Vector2(9.5f, 5.5f), LampadaTrevaOffset, LampadaTrevaOffset + new Vector2(-24f, 0f)),
        };

        private static ProfundoGate[] _profundoGates;

        private static Vector2 GateSpot(float degrees)
        {
            float a = degrees * Mathf.Deg2Rad;
            return FortalezaUltimaLampadaOffset + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * ProfundoGateSpotRadius;
        }
    }
}
