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

        /// <summary>
        /// The rising currents of the Cova do Vento (86, <c>ProfundoUpdraftRules</c>): four vents 30 m round the Lâmpada 3 (outside its 18 m of flat
        /// ground, clear of its roads west, east and south-south-east), each up to a landing on the outer ring, 160–200 m off the roads: north-west and
        /// south-west on the Plataformas Soltas, north-east and south-east on the Correntes Ascendentes.
        /// </summary>
        public static ProfundoUpdraft[] ProfundoUpdrafts => _profundoUpdrafts ??= new[]
        {
            new ProfundoUpdraft("corrente_noroeste", LampadaVentoOffset + new Vector2(-15f, 26f), new Vector2(-260f, 1150f)),
            new ProfundoUpdraft("corrente_nordeste", LampadaVentoOffset + new Vector2(15f, 26f), new Vector2(500f, 1150f)),
            new ProfundoUpdraft("corrente_sudoeste", LampadaVentoOffset + new Vector2(-21f, -21f), new Vector2(-200f, 760f)),
            new ProfundoUpdraft("corrente_sudeste", LampadaVentoOffset + new Vector2(21f, -21f), new Vector2(470f, 780f)),
        };

        private static ProfundoUpdraft[] _profundoUpdrafts;

        /// <summary>
        /// The deep stretches of the Cova do Lodo (87, <c>ProfundoSluiceRules</c>): one band of 56 m across the whole column in the Salões Alagados
        /// (z −480, on the road from the Lâmpada 4 to the Cova da Treva) and one in the Comportas (z 480, on the road north to the Túneis Cegos);
        /// each lever stands on the Lâmpada 4 side, 195 m east of the road and clear of the pools of black water.
        /// </summary>
        public static readonly ProfundoSluice[] ProfundoSluices =
        {
            new ProfundoSluice("saloes_alagados", Rect.MinMaxRect(840f, -508f, 1320f, -452f), new Vector2(1275f, -436f)),
            new ProfundoSluice("comportas", Rect.MinMaxRect(840f, 452f, 1320f, 508f), new Vector2(1275f, 436f)),
        };

        /// <summary>
        /// The cages of sparks of the Ponte do Novelo (89, <c>NoveloBridgeRules</c>): one at the pylon of each of flights 0–8 (alternate sides of the
        /// causeway, 7 m off the road on its crest); freeing cage k lights flight k + 1.
        /// </summary>
        public static Vector2 NoveloCage(int cage) => NoveloPylonPosition(cage);

        private static Vector2 GateSpot(float degrees)
        {
            float a = degrees * Mathf.Deg2Rad;
            return FortalezaUltimaLampadaOffset + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * ProfundoGateSpotRadius;
        }
    }
}
