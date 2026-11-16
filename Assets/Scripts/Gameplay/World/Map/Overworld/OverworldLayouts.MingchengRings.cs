using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The places of the ring gates of Mingcheng (41, <see cref="MingchengRingGateMechanicsDriver"/>, <see cref="MingchengRingGateRules"/>), in the
    /// region's own coordinates: the 4th ring round the plaza and its three gates where the city's roads leave (the same vertices as
    /// <c>OverworldLayouts.Mingcheng.cs</c>: the west road towards the west fork, the east road towards the canal's east bridge, the spirit way
    /// towards the lions' fork). Nothing is added to the layout: the ring stands on open plain, the stations on the roads.
    /// </summary>
    public static partial class OverworldLayouts
    {
        private static RingGate[] s_mingchengRingGates;

        /// <summary>The centre of the rings: Mingcheng's plaza.</summary>
        public static Vector2 MingchengRingCenter => MingchengOffset;

        /// <summary>The gates of the 4th ring, west, east and south (<see cref="MingchengRingGateRules.GateOnRoad"/>).</summary>
        public static RingGate[] MingchengRingGates => s_mingchengRingGates ??= new[]
        {
            MingchengRingGateRules.GateOnRoad("oeste", MingchengOffset + new Vector2(-MingchengGround, 0f), new Vector2(-260f, 20f), MingchengRingCenter),
            MingchengRingGateRules.GateOnRoad("leste", MingchengOffset + new Vector2(MingchengGround, 0f), new Vector2(200f, 40f), MingchengRingCenter),
            MingchengRingGateRules.GateOnRoad("sul", MingchengOffset + new Vector2(0f, -MingchengGround), new Vector2(-10f, -150f), MingchengRingCenter),
        };
    }
}
