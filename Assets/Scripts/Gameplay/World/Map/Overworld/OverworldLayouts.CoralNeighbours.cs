using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The places of the relit coral of Kirta (28, Costa, <see cref="KirtaCoralRules"/>) and of the dimmed neighbours and the night of the
    /// Palafitas de Tarchon (33, Delta, <see cref="TarchonLanternRules"/>), in each region's own coordinates (the driver adds the anchor zone's
    /// origin). Every coral stands on a straight leg of one of the reef's own trails (OverworldLayouts.Costa), so a hero walks to it; the
    /// neighbours stand on Tarchon's plaza and streets, off the lots of the town recipe.
    /// </summary>
    public static partial class OverworldLayouts
    {
        // Built on first use, not as field initialisers: they read offsets of other partial files of this class, whose initialisers C# may run later.
        private static KirtaCoral[] s_kirtaCorals;
        private static TarchonNeighbour[] s_tarchonNeighbours;

        /// <summary>
        /// The five dimmed corals round Kirta, each with a spark caught in it: on the east road out of the town (Atol), on the west road in, on the
        /// trail south to the great lagoon, on the trail north to the coral islets (Atol) and on the road over the kelp baixio (Jardins de Alga).
        /// </summary>
        public static KirtaCoral[] KirtaCorals => s_kirtaCorals ??= new[]
        {
            new KirtaCoral("leste", Vector2.Lerp(KirtaOffset + new Vector2(RaceCityGround, 0f), new Vector2(1250f, -20f), 0.5f)),
            new KirtaCoral("oeste", Vector2.Lerp(new Vector2(760f, -30f), KirtaOffset + new Vector2(-RaceCityGround, 0f), 0.5f)),
            new KirtaCoral("sul", Vector2.Lerp(new Vector2(760f, -30f), new Vector2(800f, -300f), 0.75f)),
            new KirtaCoral("norte", Vector2.Lerp(new Vector2(650f, 0f), new Vector2(800f, 300f), 0.75f)),
            new KirtaCoral("algas", Vector2.Lerp(new Vector2(1250f, -20f), Vector2.Lerp(new Vector2(1265f, 100f), new Vector2(1500f, 60f), 0.6f), 0.5f)),
        };

        /// <summary>The Palafitas de Tarchon's own ground (33: the stilts and the Ilhas que Andam, the zones of the map), where the swamp night falls.</summary>
        public static readonly Rect TarchonNightBounds = Rect.MinMaxRect(240f, -1320f, 1200f, -840f);

        /// <summary>
        /// The five dimmed trols who stand still in Tarchon (Docs/Mapas100.md §3.3, "Despertar o vizinho"), at offsets from the plaza centre: two on
        /// the plaza, one on each street (south, east, west). Etruscan names, like the town's own (inscriptions of Tarquinia and Cerveteri).
        /// </summary>
        public static TarchonNeighbour[] TarchonNeighbours => s_tarchonNeighbours ??= new[]
        {
            new TarchonNeighbour("arnth", TarchonOffset + new Vector2(-5f, 0.8f)),
            new TarchonNeighbour("velia", TarchonOffset + new Vector2(5f, -0.8f)),
            new TarchonNeighbour("laris", TarchonOffset + new Vector2(0.8f, -10f)),
            new TarchonNeighbour("ravnthu", TarchonOffset + new Vector2(10f, 0.8f)),
            new TarchonNeighbour("fasti", TarchonOffset + new Vector2(-12f, -0.8f)),
        };
    }
}
