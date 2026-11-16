using System.Collections.Generic;
using Rapadura.Gameplay.Mounts;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// One pair of marked laurel trunks of the Copas de Garajonay (<c>OverworldLayouts.GarajonayTrunks</c>), in the region's own coordinates:
    /// the Laurel Lizard climbs the trunk at one end, runs along the branches and comes down the trunk at the other end.
    /// </summary>
    public readonly struct LaurelTrunkLink
    {
        public readonly string Id;
        public readonly Vector2 A;
        public readonly Vector2 B;

        public LaurelTrunkLink(string id, Vector2 a, Vector2 b)
        {
            Id = id;
            A = a;
            B = b;
        }
    }

    /// <summary>What [E] at a marked trunk does (<see cref="GarajonayTrunkRules.Check"/>).</summary>
    public enum TrunkCheck
    {
        /// <summary>Riding the Laurel Lizard: up the trunk and across the canopy.</summary>
        Climb = 0,

        /// <summary>Any other mount, or on foot: the claw marks are only a hint.</summary>
        NeedsLizard = 1,

        /// <summary>The lizard just came down: a breath before the next trunk.</summary>
        Resting = 2,
    }

    /// <summary>
    /// The marked trunks of the Copas de Garajonay (Docs/Mapas100.md 05 §6: the Lagarto-do-Louro "sobe troncos marcados (atalho)"), pure and
    /// server-runnable. Some laurels of the forest floor carry claw marks; riding the <see cref="MountBossCatalog.LaurelLizard"/>, [E] at the foot
    /// of one climbs it and runs along the branches, over the trails and the streams, down the marked trunk at the other end. Nobody else climbs
    /// (no other mount, not on foot). The hero stays mounted the whole way, high over the forest floor (so above the fog, 05).
    /// <code>
    /// mark: reach 3 m · run 11 m/s, 10 m over the ground at the middle · 2 s of rest after each run
    /// </code>
    /// </summary>
    public static class GarajonayTrunkRules
    {
        public const float MarkReach = 3f;
        public const float RunSpeed = 11f;
        public const float CanopyArc = 10f;
        public const float RestSeconds = 2f;

        /// <summary>Whether a hero <paramref name="mounted"/> on <paramref name="mountId"/> can climb a marked trunk.</summary>
        public static bool IsClimber(bool mounted, string mountId) => mounted && mountId == MountBossCatalog.LaurelLizard;

        /// <summary>What [E] at a marked trunk does, <paramref name="sinceLastRun"/> seconds after the last run.</summary>
        public static TrunkCheck Check(bool mounted, string mountId, float sinceLastRun)
        {
            if (!IsClimber(mounted, mountId))
            {
                return TrunkCheck.NeedsLizard;
            }

            return sinceLastRun < RestSeconds ? TrunkCheck.Resting : TrunkCheck.Climb;
        }

        /// <summary>The link whose marked trunk is within <see cref="MarkReach"/> of <paramref name="p"/> (−1 for none), and whether it is the A one.</summary>
        public static int TrunkAt(IReadOnlyList<LaurelTrunkLink> links, Vector2 p, out bool atA)
        {
            atA = false;
            float reach = MarkReach * MarkReach;
            for (int i = 0; links != null && i < links.Count; i++)
            {
                if ((links[i].A - p).sqrMagnitude <= reach)
                {
                    atA = true;
                    return i;
                }

                if ((links[i].B - p).sqrMagnitude <= reach)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>The run over the canopy, from the trunk the hero stands at to the other one.</summary>
        public static Vector2[] Course(LaurelTrunkLink link, bool fromA) => fromA ? new[] { link.A, link.B } : new[] { link.B, link.A };

        /// <summary>Where the run comes down.</summary>
        public static Vector2 Landing(LaurelTrunkLink link, bool fromA) => fromA ? link.B : link.A;

        /// <summary>Seconds a run takes.</summary>
        public static float RunSeconds(LaurelTrunkLink link) => Vector2.Distance(link.A, link.B) / RunSpeed;
    }
}
