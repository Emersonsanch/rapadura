using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>The places of the Teto do Mundo avalanches, in the region's own coordinates (the driver adds the zone's origin).</summary>
    public static partial class OverworldLayouts
    {
        /// <summary>The region's rectangle: where <see cref="Mechanics.CostaDeltaTetoMechanicsDriver"/> runs the avalanches.</summary>
        public static readonly Rect TetoBounds = Rect.MinMaxRect(-1200f, -600f, 1200f, 1800f);

        /// <summary>A zone of the region: its <see cref="OverworldZone.Origin"/> says where the region sits in the stitched world.</summary>
        public const string TetoAnchorZoneId = "calhas_de_pedra";

        /// <summary>
        /// The four chutes of map 37 (two in the Calhas de Pedra, two in the Calhas Geladas), running north to south and clear of the river's bridges.
        /// Roads cross them, and that is the point: the crossing is dangerous only for the seven seconds of a slide.
        /// </summary>
        public static readonly TetoChuteLane[] ChuteLanes =
        {
            new TetoChuteLane(new Vector2(600f, 340f), new Vector2(580f, -100f), 18f),
            new TetoChuteLane(new Vector2(700f, 350f), new Vector2(690f, -100f), 18f),
            new TetoChuteLane(new Vector2(350f, 830f), new Vector2(470f, 380f), 18f),
            new TetoChuteLane(new Vector2(510f, 835f), new Vector2(525f, 395f), 18f),
        };
    }
}
