using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The places of the late route mechanics (<see cref="Mechanics.LateRouteMechanicsDriver"/>), each in its region's own coordinates (the driver
    /// adds the anchor zone's origin): the walks along the bottom of Kirta's reef (28, Costa), the mouths of the Galerias dos Mamutes (62) and the
    /// floe chain of the Mar das Auroras (63, Mar Gelado), the sledge road of the Areal Escondido (73, Rio), the lamp stands of the Cova da Treva
    /// and the Túneis Cegos (88, Limiar Profundo) and the lantern rack and stand of the two barques (93 → 97, Céu). Every station a hero walks to is
    /// a point of one of the region's own trails (a vertex, or on a straight leg between two), except the two dive stations at Kirta's north and
    /// south exits, which stand on the town's flat ground: none moves if a trail is redrawn without them being redrawn too (the tests check).
    /// </summary>
    public static partial class OverworldLayouts
    {
        // Built on first use, not as field initialisers: they read offsets of other partial files of this class, whose initialisers C# may run later.
        private static ReefDive[] s_kirtaReefDives;
        private static Vector2[] s_mammothGalleryMouths;
        private static Vector2[] s_hiddenSandsSledgeCourse;
        private static Vector2[] s_darkPitStands;

        // ------------------------------------------------------------------
        // Bolhas de Ar (28, Costa)
        // ------------------------------------------------------------------

        /// <summary>The ford over the coral islets' channel on the trail from the reef to Kilwa (OverworldLayouts.Costa: On(i0, i1, 0.5)).</summary>
        public static Vector2 KirtaIsletsFord => Vector2.Lerp(new Vector2(1000f, 265f), new Vector2(980f, 470f), 0.5f);

        /// <summary>The trail south from the reef to the great lagoon, at its bend (850, −480) (OverworldLayouts.Costa).</summary>
        public static Vector2 KirtaLagoonTrail => new Vector2(850f, -480f);

        /// <summary>
        /// The two walks along the bottom (<see cref="KirtaReefWalkRules"/>): north from Kirta's north exit under the lagoon that walls the town in
        /// and up the islets' channel to its ford (every stretch ≤ 82 m: the Concha Respirável); south from the south exit down over the abyss to the
        /// great lagoon's trail (stretches of ≈ 120 m: the Bolha de Ar). The points between the ends are the air bubbles.
        /// </summary>
        public static ReefDive[] KirtaReefDives => s_kirtaReefDives ??= new ReefDive[]
        {
            new ReefDive("fundo_da_lagoa", new[]
            {
                KirtaOffset + new Vector2(0f, 26f), new Vector2(1000f, 46f), new Vector2(1000f, 126f), new Vector2(998f, 206f), new Vector2(995f, 286f), KirtaIsletsFord,
            }, false),
            new ReefDive("fundo_do_abismo", new[]
            {
                KirtaOffset + new Vector2(0f, -26f), new Vector2(1000f, -206f), new Vector2(990f, -326f), new Vector2(930f, -430f), KirtaLagoonTrail,
            }, true),
        };

        // ------------------------------------------------------------------
        // Túneis que mudam (62, Mar Gelado)
        // ------------------------------------------------------------------

        /// <summary>
        /// The six tunnel mouths of the Galerias dos Mamutes (<see cref="MammothGalleryRules"/>), two on each road out of the Boca do Gelo: 0–1 the
        /// south road (from the bay fork), 2–3 the east road (to the pinnacle pass), 4–5 the north road (to the black fjord). Each is on a straight
        /// leg of its road (OverworldLayouts.MarGelado).
        /// </summary>
        public static Vector2[] MammothGalleryMouths => s_mammothGalleryMouths ??= new Vector2[]
        {
            Vector2.Lerp(new Vector2(700f, 300f), new Vector2(900f, 430f), 0.75f),
            Vector2.Lerp(new Vector2(900f, 430f), BocaDoGeloOffset + new Vector2(0f, -18f), 0.6f),
            Vector2.Lerp(BocaDoGeloOffset + new Vector2(18f, 0f), new Vector2(1100f, 570f), 0.6f),
            Vector2.Lerp(new Vector2(1100f, 570f), new Vector2(1191f, 560f), 0.6f),
            Vector2.Lerp(BocaDoGeloOffset + new Vector2(0f, 18f), new Vector2(1000f, 850f), 0.3f),
            Vector2.Lerp(BocaDoGeloOffset + new Vector2(0f, 18f), new Vector2(1000f, 850f), 0.85f),
        };

        // ------------------------------------------------------------------
        // Pular placas (63, Mar Gelado)
        // ------------------------------------------------------------------

        /// <summary>How many floes lie between the two ends of the chain.</summary>
        public const int AuroraFloeCount = 9;

        /// <summary>The west end of the floe chain: the north road at (20, 1200), on the Mar das Auroras north of the Aurora Presa's ring.</summary>
        public static Vector2 AuroraFloeWest => new Vector2(20f, 1200f);

        /// <summary>The east end: the lighthouse road between the sea fork's bend (240, 1040) and the Farol de Gelo, 60% of the way.</summary>
        public static Vector2 AuroraFloeEast => Vector2.Lerp(new Vector2(240f, 1040f), FarolDeGeloOffset + new Vector2(-18f, 0f), 0.6f);

        private static Vector2[] s_auroraFloeChain;

        /// <summary>The chain in line (<see cref="AuroraFloeRules"/>): the west end, the <see cref="AuroraFloeCount"/> floes evenly between, the east end.</summary>
        public static Vector2[] AuroraFloeChain
        {
            get
            {
                if (s_auroraFloeChain == null)
                {
                    var chain = new Vector2[AuroraFloeCount + 2];
                    for (int i = 0; i < chain.Length; i++)
                    {
                        chain[i] = Vector2.Lerp(AuroraFloeWest, AuroraFloeEast, i / (float)(chain.Length - 1));
                    }

                    s_auroraFloeChain = chain;
                }

                return s_auroraFloeChain;
            }
        }

        // ------------------------------------------------------------------
        // Puxar o trenó (73, Rio)
        // ------------------------------------------------------------------

        /// <summary>
        /// The sledge road (<see cref="HiddenSandsSledgeRules"/>): the road south from the Trenó de Areia (its south exit at (−960, 102)) to its end in
        /// the Portas Fechadas at (−960, −360), from 12 m out of the post to 4 m short of the end.
        /// </summary>
        public static Vector2[] HiddenSandsSledgeCourse => s_hiddenSandsSledgeCourse ??= new Vector2[]
        {
            TrenoDeAreiaOffset + new Vector2(0f, -30f), new Vector2(-960f, -356f),
        };

        /// <summary>The three stone doors on the sledge road, a quarter, half and three quarters of the way down (metres from the Trenó).</summary>
        public static readonly float[] HiddenSandsDoorsAlong = { 0.25f * 446f, 0.5f * 446f, 0.75f * 446f };

        // ------------------------------------------------------------------
        // Escuro (88, Limiar Profundo)
        // ------------------------------------------------------------------

        /// <summary>Where the dark holds (Profundo coordinates): the Cova da Treva and the Túneis Cegos, the two zones of map 88.</summary>
        public static readonly Rect[] DarkPitAreas =
        {
            Rect.MinMaxRect(840f, -1200f, 1320f, -720f),
            Rect.MinMaxRect(840f, 720f, 1320f, 1200f),
        };

        /// <summary>
        /// The lamp stands (<see cref="DarkPitLightRules"/>): four on the roads of the Cova da Treva (two on the west road, one on the north road
        /// down from the Mire, one on the road east to the Ponte do Novelo) and two in the Túneis Cegos (the road from the Pit of Ruinous Wind, the
        /// road up from the sluices). Each is on a straight leg of its road (OverworldLayouts.Profundo).
        /// </summary>
        public static Vector2[] DarkPitStands => s_darkPitStands ??= new Vector2[]
        {
            Vector2.Lerp(new Vector2(600f, -960f), LampadaTrevaOffset + new Vector2(-18f, 0f), 0.6f),
            Vector2.Lerp(new Vector2(600f, -960f), LampadaTrevaOffset + new Vector2(-18f, 0f), 0.85f),
            Vector2.Lerp(new Vector2(1060f, -720f), LampadaTrevaOffset + new Vector2(0f, 18f), 0.4f),
            Vector2.Lerp(LampadaTrevaOffset + new Vector2(18f, 0f), new Vector2(1320f, -960f), 0.5f),
            Vector2.Lerp(new Vector2(860f, 960f), new Vector2(1080f, 960f), 0.45f),
            Vector2.Lerp(new Vector2(1060f, 720f), new Vector2(1080f, 960f), 0.5f),
        };

        // ------------------------------------------------------------------
        // Carregar a luz (97, Céu)
        // ------------------------------------------------------------------

        /// <summary>The lantern rack on the Moon's deck: on the road north out of the Cabine da Proa, 40 m from the cabin.</summary>
        public static Vector2 MoonLanternRack => CabineDaProaOffset + new Vector2(0f, 40f);

        /// <summary>The lantern stand on the Sun's deck: on the road from the Pérgola to the Cabine da Popa, 30 m short of the cabin.</summary>
        public static Vector2 SunLanternStand => CabineDaPopaOffset + new Vector2(-30f, 0f);
    }
}
