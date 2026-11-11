using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>Where the silence of the Ermidas Suspensas is kept (56, <c>FarnkeshSilenceRules</c>), in the region's own coordinates.</summary>
    public static partial class OverworldLayouts
    {
        /// <summary>
        /// The two areas of map 56: the Ermidas Suspensas and the Pináculo do Sino (the post's safe ground inside is left out by the driver,
        /// and so is everything south of z 840, the Bosque de Pináculos, and north of 1320, the Garganta dos Ays and its gusts).
        /// </summary>
        public static readonly Rect ErmidasSilenceArea = Rect.MinMaxRect(-1200f, 840f, -240f, 1320f);
    }
}
