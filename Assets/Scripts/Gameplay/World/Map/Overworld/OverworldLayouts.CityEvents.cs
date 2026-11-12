using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The places of the three capital events (Docs/Mapas100.md 31, 36 and 41; <see cref="Mechanics.CityEventsMechanicsDriver"/>), each in its
    /// region's own coordinates (the driver adds the region's origin). The three capitals stand at their region's (0, 0): Jhilpur
    /// (<see cref="JhilpurOffset"/>), Sangkent (<see cref="SangkentOffset"/>) and Mingcheng (<see cref="MingchengOffset"/>), each on flat ground of
    /// ±30 m with its safe zone at ±26 m (nothing is hurt inside it, so every fight of these events is outside it).
    /// </summary>
    public static partial class OverworldLayouts
    {
        // ------------------------------------------------------------------
        // Maré de Lodo (31, Jhilpur)
        // ------------------------------------------------------------------

        /// <summary>
        /// Where the Filhos do Lodo rise, on the lake shore south of Jhilpur (the lake's water begins at y = −65): west, middle and east of the ghats,
        /// 30 m below the town's south face.
        /// </summary>
        public static readonly Vector2[] JhilpurTideShore =
        {
            new Vector2(-40f, -60f), new Vector2(0f, -62f), new Vector2(40f, -60f),
        };

        /// <summary>Where each shore's Filhos climb to: the foot of the brick ghats, just outside the safe zone (one per shore, same order).</summary>
        public static readonly Vector2[] JhilpurGhatFoot =
        {
            new Vector2(-22f, -30f), new Vector2(0f, -30f), new Vector2(22f, -30f),
        };

        /// <summary>The band at the foot of the ghats: a Filho standing in it silts the Raiz-Mãe (<see cref="JhilpurMudTideRules.Silting"/>).</summary>
        public static readonly Rect JhilpurGhatBand = Rect.MinMaxRect(-34f, -40f, 34f, -26f);

        // ------------------------------------------------------------------
        // Avalanche (36, Sangkent)
        // ------------------------------------------------------------------

        /// <summary>
        /// The three snow lanes above Sangkent's lower terraces, west to east, in the order they slide (<see cref="SangkentAvalancheRules"/>): each runs
        /// 176 m from the high terraces down to the town's upper edge (y = 34), 16 m wide, with its fence of juniper stakes 16 m above the foot, off the
        /// stone ring of the island gate (−80, 80; the west lane's edge is 15 m from it) and clear of the north road (the east lane's top is 5 m west
        /// of it) and of the chutes of map 37 (x ≥ 350).
        /// </summary>
        public static readonly SangkentTerraceLane[] SangkentTerraceLanes =
        {
            new SangkentTerraceLane(new TetoChuteLane(new Vector2(-55f, 210f), new Vector2(-55f, 34f), 16f), new Vector2(-55f, 50f)),
            new SangkentTerraceLane(new TetoChuteLane(new Vector2(10f, 210f), new Vector2(10f, 34f), 16f), new Vector2(10f, 50f)),
            new SangkentTerraceLane(new TetoChuteLane(new Vector2(70f, 210f), new Vector2(70f, 34f), 16f), new Vector2(70f, 50f)),
        };

        // ------------------------------------------------------------------
        // Festa das Lanternas (41, Mingcheng)
        // ------------------------------------------------------------------

        /// <summary>
        /// The five riddle lanterns: on the West, East and South streets (between the plaza and the canals) and on the west and south roads just
        /// outside the town.
        /// </summary>
        public static readonly Vector2[] MingchengRiddleLanterns =
        {
            new Vector2(-12f, 0f), new Vector2(11f, 0f), new Vector2(0f, -11f), new Vector2(-36f, 0f), new Vector2(0f, -36f),
        };

        /// <summary>
        /// The four answer lanterns on the south half of the plaza (the Torre do Tambor e do Sino fills the north half), in
        /// <see cref="LanternAnswer"/> order: Sun, Moon, Carp, Bell.
        /// </summary>
        public static readonly Vector2[] MingchengAnswerLanterns =
        {
            new Vector2(-4f, -1f), new Vector2(-1.5f, -4.5f), new Vector2(1.5f, -4.5f), new Vector2(4f, -1f),
        };
    }
}
