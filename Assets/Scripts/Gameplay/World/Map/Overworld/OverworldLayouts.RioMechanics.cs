using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>The stranded barque of Apep's sandbank (76, <c>RioBarqueDefenseRules</c>) and the rafts of the reed fields (72, <c>RioRaftRules</c>), in the region's own coordinates.</summary>
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

        /// <summary>
        /// The two long canals of the Campos de Juncos (72, <c>RioRaftRules</c>): the south canal (z 500) and the north canal (z 760), 840 m each
        /// from the Canteiros de Juncos to the Trigais de Luz (built on first use: the canals live in another part of this partial class); a landing on the bank 12 m past each end (west = landing 2·i, east = 2·i + 1).
        /// </summary>
        public static RioRaftCanal[] RioRaftCanals => _rioRaftCanals ??= new[]
        {
            new RioRaftCanal("canal_sul", CanalSekhetSul, new Vector2(-432f, 500f), new Vector2(432f, 500f)),
            new RioRaftCanal("canal_norte", CanalSekhetNorte, new Vector2(-432f, 760f), new Vector2(432f, 760f)),
        };

        private static RioRaftCanal[] _rioRaftCanals;

        /// <summary>
        /// The eight reed clumps cut for the rafts (72): on the banks of the two canals, 7 m off their centre line, at x ±260 and ±380 (clear of the
        /// bridges at x ±150 and 0 and of the roads to the reed beds and the wheat, 90 m and more to the south).
        /// </summary>
        public static readonly Vector2[] RioReedClumps =
        {
            new Vector2(-380f, 493f), new Vector2(-260f, 507f), new Vector2(260f, 507f), new Vector2(380f, 493f),
            new Vector2(-380f, 767f), new Vector2(-260f, 753f), new Vector2(260f, 753f), new Vector2(380f, 767f),
        };
    }
}
