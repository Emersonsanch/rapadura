using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// The last four map bosses of the Reinos do Céu (Docs/Mapas100.md 92, 94, 98, 99): the ids, the numbers and the pure rules of their abilities
    /// (<see cref="RegionBossSignature"/> 100–103). Their spawns are in <see cref="MapSpawns"/> with the others; the fight in
    /// <see cref="RegionBossAbilities"/> (RegionBossAbilities.SkyBosses.cs). Map 100 has no fight by its ficha, and 95 and 96 keep their world bosses.
    /// </summary>
    public static partial class RegionBossRules
    {
        public const string LostRowerId = "enemy_mountain_lost_rower";
        public const string WheelArchonId = "enemy_mountain_wheel_archon";
        public const string LastStepArchonId = "enemy_mountain_last_step_archon";
        public const string GreatCrackId = "enemy_mountain_great_crack";

        // ---- O Remador Perdido (92): the wake runs downstream ----

        public const float RowerIntervalSeconds = 10f;
        public const float RowerWindupSeconds = 2.2f;
        public const float RowerDamageFactor = 1.35f;

        /// <summary>The wake starts on the hero's spot and runs this far downstream, this wide either side of its line.</summary>
        public const float RowerWakeLength = 7f;

        public const float RowerWakeHalfWidth = 2.5f;

        /// <summary>Which way the Rio de Luz runs (east: the current is fast that way, slow against it; CeuRiverRules).</summary>
        public static readonly Vector2 RowerDownstream = Vector2.right;

        // ---- O Arconte da Roda (94): the jar that keeps turning ----

        public const float WheelIntervalSeconds = 11f;
        public const float WheelWindupSeconds = 2.4f;
        public const float WheelDamageFactor = 1.35f;

        /// <summary>The twelve jars round him, on a ring this far out, each this wide; the hub under him does not turn.</summary>
        public const int WheelJars = 12;

        public const float WheelJarRing = 7f;
        public const float WheelJarRadius = 2.2f;
        public const float WheelHubRadius = 2.5f;

        /// <summary>How far round him the jars spill.</summary>
        public const float WheelReach = 11f;

        /// <summary>The jar of the hero comes round this many jars (a quarter of the wheel, clockwise) before the lock bites.</summary>
        public const int WheelTurnJars = 3;

        // ---- O Último Arconte do Degrau (98): nobody goes up ----

        public const float StepIntervalSeconds = 10f;
        public const float StepWindupSeconds = 2.2f;
        public const float StepDamageFactor = 1.4f;

        /// <summary>The step he sits on runs this far down the slope from him; everything on it and above it, within his reach, is thrown down.</summary>
        public const float StepHalfDepth = 2f;

        public const float StepReach = 12f;

        // ---- A Rachadura Grande (99): the crack opens wider ----

        public const float CrackIntervalSeconds = 10f;
        public const float CrackWindupSeconds = 2f;
        public const float CrackDamageFactor = 1.4f;

        /// <summary>How far the crack runs either way from her, along <see cref="CrackAxis"/>.</summary>
        public const float CrackHalfLength = 12f;

        /// <summary>How wide either side of its line the crack is on each drip: wider and wider, then it closes up and starts over.</summary>
        public static readonly float[] CrackHalfWidths = { 1.5f, 3f, 4.5f };

        /// <summary>The crack runs like the lips of the Lookout's own cracks (OverworldRelief.Ceu, SkyFissures: across their wave).</summary>
        public static readonly Vector2 CrackAxis = new Vector2(-0.021f, 0.033f).normalized;

        /// <summary>Does the wake of O Remador Perdido, from the hero's spot <paramref name="mark"/> downstream, catch a hero at <paramref name="hero"/>? Upstream or aside is safe.</summary>
        public static bool RowerWakeHits(Vector2 mark, Vector2 hero)
        {
            Vector2 end = mark + RowerDownstream * RowerWakeLength;
            Vector2 line = end - mark;
            float t = Mathf.Clamp01(Vector2.Dot(hero - mark, line) / line.sqrMagnitude);
            return (hero - (mark + line * t)).sqrMagnitude <= RowerWakeHalfWidth * RowerWakeHalfWidth;
        }

        /// <summary>The jar of the wheel round <paramref name="centre"/> (0 north, clockwise) nearest <paramref name="spot"/>.</summary>
        public static int WheelJarOf(Vector2 centre, Vector2 spot)
        {
            Vector2 offset = spot - centre;
            if (offset.sqrMagnitude < 0.0001f)
            {
                return 0;
            }

            float degrees = Mathf.Repeat(Mathf.Atan2(offset.x, offset.y) * Mathf.Rad2Deg, 360f);
            return Mathf.RoundToInt(degrees / (360f / WheelJars)) % WheelJars;
        }

        /// <summary>Where jar <paramref name="jar"/> of the wheel round <paramref name="centre"/> stands (0 north, clockwise).</summary>
        public static Vector2 WheelJarPosition(Vector2 centre, int jar)
        {
            float a = (((jar % WheelJars) + WheelJars) % WheelJars) * (360f / WheelJars) * Mathf.Deg2Rad;
            return centre + new Vector2(Mathf.Sin(a), Mathf.Cos(a)) * WheelJarRing;
        }

        /// <summary>The jar that keeps turning when O Arconte da Roda (at <paramref name="centre"/>) locks the wheel: the hero's own (at <paramref name="mark"/>), a quarter on.</summary>
        public static int WheelLitJar(Vector2 centre, Vector2 mark) => (WheelJarOf(centre, mark) + WheelTurnJars) % WheelJars;

        /// <summary>Do the jars spill on a hero at <paramref name="hero"/> when O Arconte da Roda (at <paramref name="centre"/>) locks the wheel? Within his reach, but not on the hub nor in the lit jar.</summary>
        public static bool WheelLockHits(Vector2 centre, int litJar, Vector2 hero)
        {
            float d2 = (hero - centre).sqrMagnitude;
            if (d2 > WheelReach * WheelReach || d2 <= WheelHubRadius * WheelHubRadius)
            {
                return false;
            }

            return (hero - WheelJarPosition(centre, litJar)).sqrMagnitude > WheelJarRadius * WheelJarRadius;
        }

        /// <summary>Uphill from O Último Arconte do Degrau's ring: towards the Coluna da Glória (region coordinates; the offset of the stitched world cancels out).</summary>
        public static Vector2 StepUphill => (OverworldLayouts.GloryColumnCenter - OverworldLayouts.UltimoArconteArena).normalized;

        /// <summary>Does O Último Arconte do Degrau (at <paramref name="archon"/>) throw down a hero at <paramref name="hero"/>? On his step or above it, within his reach; a step below is safe.</summary>
        public static bool HeldStepHits(Vector2 archon, Vector2 uphill, Vector2 hero)
        {
            Vector2 offset = hero - archon;
            if (offset.sqrMagnitude > StepReach * StepReach)
            {
                return false;
            }

            Vector2 up = uphill.sqrMagnitude > 0.0001f ? uphill.normalized : Vector2.up;
            return Vector2.Dot(offset, up) >= -StepHalfDepth;
        }

        /// <summary>How wide either side of its line the crack of A Rachadura Grande opens on drip number <paramref name="drip"/>.</summary>
        public static float CrackHalfWidthOf(int drip) => CrackHalfWidths[Mathf.Abs(drip) % CrackHalfWidths.Length];

        /// <summary>Does drip number <paramref name="drip"/> of A Rachadura Grande (at <paramref name="crack"/>) fall on a hero at <paramref name="hero"/>? Inside the crack's band, along its axis; aside of it is safe.</summary>
        public static bool CrackDripHits(int drip, Vector2 crack, Vector2 hero)
        {
            Vector2 offset = hero - crack;
            if (Mathf.Abs(Vector2.Dot(offset, CrackAxis)) > CrackHalfLength)
            {
                return false;
            }

            return Mathf.Abs(CrackAxis.x * offset.y - CrackAxis.y * offset.x) <= CrackHalfWidthOf(drip);
        }

        private static float SkyMapBossInterval(RegionBossSignature signature)
        {
            switch (signature)
            {
                case RegionBossSignature.DownstreamWake: return RowerIntervalSeconds;
                case RegionBossSignature.TurningJars: return WheelIntervalSeconds;
                case RegionBossSignature.HeldStep: return StepIntervalSeconds;
                default: return CrackIntervalSeconds;
            }
        }

        private static float SkyMapBossWindup(RegionBossSignature signature)
        {
            switch (signature)
            {
                case RegionBossSignature.DownstreamWake: return RowerWindupSeconds;
                case RegionBossSignature.TurningJars: return WheelWindupSeconds;
                case RegionBossSignature.HeldStep: return StepWindupSeconds;
                default: return CrackWindupSeconds;
            }
        }
    }
}
