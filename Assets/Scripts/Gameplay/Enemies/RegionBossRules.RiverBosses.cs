using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Five map bosses of the Rio das Doze Horas and the Margens da Mistura (Docs/Mapas100.md 72, 75, 78, 80, 81): the ids, the numbers and the pure
    /// rules of their abilities (<see cref="RegionBossSignature"/> 90–94). Their spawns are in <see cref="MapSpawns"/> with the others; the fight in
    /// <see cref="RegionBossAbilities"/> (RegionBossAbilities.RiverBosses.cs).
    /// </summary>
    public static partial class RegionBossRules
    {
        public const string ReedHippoId = "enemy_mountain_reed_hippo";
        public const string LakeGuardianId = "enemy_mountain_lake_guardian";
        public const string LostInstantId = "enemy_mountain_lost_instant";
        public const string HobabishId = "enemy_mountain_hobabish";
        public const string UllikummiId = "enemy_mountain_ullikummi";

        // ---- O Hipopótamo-de-Junco Gigante (72): the ripple of his plunge ----

        public const float RippleIntervalSeconds = 10f;
        public const float RippleWindupSeconds = 2.4f;
        public const float RippleDamageFactor = 1.35f;

        /// <summary>Half the thickness of the ripple: it rolls out to the distance the hero stood at when he plunged, this much either side of it.</summary>
        public const float RippleHalfWidth = 1.6f;

        /// <summary>Beyond this the ripple has died down.</summary>
        public const float RippleReach = 16f;

        // ---- O Guardião do Lago (75): the shadow he casts when the lake flares ----

        public const float LakeShadowIntervalSeconds = 10f;
        public const float LakeShadowWindupSeconds = 2.4f;
        public const float LakeShadowDamageFactor = 1.4f;

        /// <summary>The three lobes of the Lake of Fire flare in turn: north, east, south.</summary>
        public const int LakeFlares = 3;

        /// <summary>How far his shadow falls from him, away from the flare, and how wide it opens (degrees, all of it).</summary>
        public const float LakeShadowLength = 14f;

        public const float LakeShadowDegrees = 70f;

        // ---- O Instante Perdido (78): back to the instant it stole ----

        public const float InstantIntervalSeconds = 11f;
        public const float InstantWindupSeconds = 3f;
        public const float InstantDamageFactor = 1.4f;

        /// <summary>How far round it the moment breaks, and how big the stolen instant is (the only safe place inside that).</summary>
        public const float InstantReach = 13f;

        public const float InstantSafeRadius = 2.8f;

        // ---- Hobabish, o Guardião do Cedro (80): the seven mantles of brilliance ----

        public const float MantleIntervalSeconds = 10f;
        public const float MantleWindupSeconds = 2.4f;
        public const float MantleDamageFactor = 1.3f;

        /// <summary>He throws one mantle on the hero's spot each time; the ones already on the ground stay and flare again. Seven at most, then the oldest goes back on him.</summary>
        public const int MantleCount = 7;

        public const float MantleRadius = 2.6f;

        // ---- Ullikummi, o Que Cresce (81): the stone grows each slam ----

        public const float GrowthIntervalSeconds = 10f;
        public const float GrowthWindupSeconds = 2.6f;
        public const float GrowthDamageFactor = 1.4f;

        /// <summary>How far the first slam reaches, how much more each next one (more at night, when he grows more), and how many before he cracks and starts over.</summary>
        public const float GrowthFirstReach = 5f;

        public const float GrowthStep = 2.5f;

        public const float GrowthNightStep = 3.5f;

        public const int Growths = 4;

        /// <summary>Right against his foot, under the stone that already grew, the slam passes over.</summary>
        public const float GrowthFootSafe = 2.2f;

        /// <summary>Does the ripple of O Hipopótamo-de-Junco's plunge (at <paramref name="hippo"/>, the hero <paramref name="distanceAtPlunge"/> m off when he went under) catch a hero at <paramref name="hero"/>? Closer or farther is safe; going round him is not.</summary>
        public static bool RippleHits(Vector2 hippo, float distanceAtPlunge, Vector2 hero)
        {
            float distance = Vector2.Distance(hippo, hero);
            return distance <= RippleReach && Mathf.Abs(distance - distanceAtPlunge) <= RippleHalfWidth;
        }

        /// <summary>Which lobe of the lake flares on flare number <paramref name="flare"/>: 0 north, 1 east, 2 south.</summary>
        public static int LakeFlareOf(int flare) => Mathf.Abs(flare) % LakeFlares;

        /// <summary>The flat direction the light of flare number <paramref name="flare"/> comes FROM (north, east, south of the guardian).</summary>
        public static Vector2 LakeFlareDirection(int flare)
        {
            switch (LakeFlareOf(flare))
            {
                case 0: return Vector2.up;
                case 1: return Vector2.right;
                default: return Vector2.down;
            }
        }

        /// <summary>Is a hero at <paramref name="hero"/> in the shadow O Guardião do Lago (at <paramref name="guardian"/>) casts on flare number <paramref name="flare"/>? Away from the flare, within its wedge; the lit side and aside are safe.</summary>
        public static bool LakeShadowHits(int flare, Vector2 guardian, Vector2 hero)
        {
            Vector2 offset = hero - guardian;
            float distance = offset.magnitude;
            if (distance > LakeShadowLength)
            {
                return false;
            }

            if (distance < 0.5f)
            {
                return true;
            }

            Vector2 shadow = -LakeFlareDirection(flare);
            return Vector2.Angle(shadow, offset) <= LakeShadowDegrees * 0.5f;
        }

        /// <summary>
        /// The stolen instant O Instante Perdido (at <paramref name="instant"/>) offers back: where the hero stood at its last strike (<paramref name="lastInstant"/>),
        /// drawn in so the whole of it stays inside the reach.
        /// </summary>
        public static Vector2 InstantSpot(Vector2 instant, Vector2 lastInstant)
        {
            Vector2 offset = lastInstant - instant;
            float most = InstantReach - InstantSafeRadius;
            return offset.magnitude > most ? instant + offset.normalized * most : lastInstant;
        }

        /// <summary>Does the moment break under a hero at <paramref name="hero"/>? Within reach of O Instante Perdido, anywhere but the instant it gives back.</summary>
        public static bool InstantHits(Vector2 instant, Vector2 spot, Vector2 hero) =>
            (hero - instant).sqrMagnitude <= InstantReach * InstantReach && (hero - spot).sqrMagnitude > InstantSafeRadius * InstantSafeRadius;

        /// <summary>Lays Hobabish's next mantle on <paramref name="spot"/>: the oldest goes back on him when all seven are down. Returns the mantle's number (0 first).</summary>
        public static int ThrowMantle(List<Vector2> mantles, Vector2 spot, int thrown)
        {
            if (mantles.Count >= MantleCount)
            {
                mantles.RemoveAt(0);
            }

            mantles.Add(spot);
            return Mathf.Abs(thrown) % MantleCount;
        }

        /// <summary>Does one of the mantles on the ground flare under a hero at <paramref name="hero"/>?</summary>
        public static bool MantlesHit(IReadOnlyList<Vector2> mantles, Vector2 hero)
        {
            foreach (Vector2 mantle in mantles)
            {
                if ((hero - mantle).sqrMagnitude <= MantleRadius * MantleRadius)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Which growth slam number <paramref name="slam"/> is: 0..<see cref="Growths"/> − 1, then he cracks and starts over.</summary>
        public static int GrowthOf(int slam) => Mathf.Abs(slam) % Growths;

        /// <summary>How far Ullikummi's slam of growth <paramref name="growth"/> reaches; at night he grows more.</summary>
        public static float GrowthReach(int growth, bool night) => GrowthFirstReach + GrowthOf(growth) * (night ? GrowthNightStep : GrowthStep);

        /// <summary>Does Ullikummi's slam (at <paramref name="stone"/>) reach a hero at <paramref name="hero"/>? Between his foot and the reach; hugging the foot or beyond is safe.</summary>
        public static bool GrowthHits(int growth, bool night, Vector2 stone, Vector2 hero)
        {
            float distance = Vector2.Distance(stone, hero);
            return distance > GrowthFootSafe && distance <= GrowthReach(growth, night);
        }

        private static float RiverMapBossInterval(RegionBossSignature signature)
        {
            switch (signature)
            {
                case RegionBossSignature.RippleRing: return RippleIntervalSeconds;
                case RegionBossSignature.LakeShadow: return LakeShadowIntervalSeconds;
                case RegionBossSignature.LostInstant: return InstantIntervalSeconds;
                case RegionBossSignature.SevenMantles: return MantleIntervalSeconds;
                default: return GrowthIntervalSeconds;
            }
        }

        private static float RiverMapBossWindup(RegionBossSignature signature)
        {
            switch (signature)
            {
                case RegionBossSignature.RippleRing: return RippleWindupSeconds;
                case RegionBossSignature.LakeShadow: return LakeShadowWindupSeconds;
                case RegionBossSignature.LostInstant: return InstantWindupSeconds;
                case RegionBossSignature.SevenMantles: return MantleWindupSeconds;
                default: return GrowthWindupSeconds;
            }
        }
    }
}
