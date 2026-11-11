using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>The buoys of the Rio de Luz (92, <c>CeuRiverRules</c>), in the region's own coordinates.</summary>
    public static partial class OverworldLayouts
    {
        /// <summary>
        /// The five buoys on the south bank of the Rio de Luz, west to east, 11–12 m south of its centre line (<see cref="RioDeLuz"/>, 10 m wide):
        /// the west reach (Foz Oeste), beside the west bridge's road (30 m west of it), beside the north road's bridge (18 m west of it, under
        /// the Boia de Luz), beside the east bridge's road (30 m west of it) and the east reach (Foz Leste).
        /// </summary>
        public static readonly Vector2[] RioDeLuzBuoys =
        {
            new Vector2(-1140f, 471f), new Vector2(-510f, 483f), new Vector2(-14f, 480f), new Vector2(450f, 483f), new Vector2(1140f, 491f),
        };

        /// <summary>The river row of map 92 (region coordinates): where the buoys are shown and [E] is read.</summary>
        public static readonly Rect RioDeLuzRow = Rect.MinMaxRect(-1200f, 240f, 1200f, 740f);
    }
}
