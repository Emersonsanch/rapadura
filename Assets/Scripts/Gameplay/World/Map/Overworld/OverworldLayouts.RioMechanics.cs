using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>The stranded barque of Apep's sandbank (76, <c>RioBarqueDefenseRules</c>), in the region's own coordinates.</summary>
    public static partial class OverworldLayouts
    {
        /// <summary>
        /// The hull of the stranded barque, on the flat sand south-east of the Proa Encalhada, 40 m east of the road up to it: off the post's safe
        /// ground and well south of Apep's spine (the ridge of <c>ReliefShape.ApepBody</c> runs at z ≈ 1560 + 120·sin(0.0095·x + 0.4), here 1644).
        /// </summary>
        public static Vector2 StrandedBarque => ProaEncalhadaOffset + new Vector2(40f, -60f);

        /// <summary>
        /// The freed barque's two sails along the dry river: A beside the hull (west and east side), B at the far end on the road of the bank
        /// (the Corpo de Apep to the west, the Rio Seco to the east, 550–630 m out, over Apep's coils). Each B is also the way back. Built on first use: it reads the
        /// post of another part of this partial class, whose initialisers may run later.
        /// </summary>
        public static SkyRoute[] FreedBarqueSails => _freedBarqueSails ??= new[]
        {
            new SkyRoute { Id = "barca_solta_oeste", Kind = SkyRouteKind.RopeBasket, A = StrandedBarque + new Vector2(-8f, 0f), B = new Vector2(-600f, 1530f) },
            new SkyRoute { Id = "barca_solta_leste", Kind = SkyRouteKind.RopeBasket, A = StrandedBarque + new Vector2(8f, 0f), B = new Vector2(600f, 1530f) },
        };

        private static SkyRoute[] _freedBarqueSails;
    }
}
