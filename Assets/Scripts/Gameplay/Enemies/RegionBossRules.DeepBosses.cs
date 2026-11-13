using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// The map bosses of the pits of the Limiar Profundo and of the bridge (Docs/Mapas100.md 84, 85, 87, 88, 89; 86 is A Águia de Asas de Treva, in
    /// RegionBossRules.UpperMapBosses.cs): their ids, the numbers and the pure rules of their abilities (<see cref="RegionBossSignature"/> 95–99).
    /// Their spawns are in <see cref="MapSpawns"/> with the others; the fight in RegionBossAbilities.DeepBosses.cs.
    /// </summary>
    public static partial class RegionBossRules
    {
        // The pits of the Limiar Profundo and the bridge: Docs/Mapas100.md 84, 85, 87, 88, 89.
        public const string SmokeKingId = "enemy_mountain_smoke_king";
        public const string DevouringLionessId = "enemy_mountain_devouring_lioness";
        public const string BlackTailFishId = "enemy_mountain_black_tail_fish";
        public const string DeepDragonId = "enemy_mountain_deep_dragon";
        public const string NoveloGuardianId = "enemy_mountain_novelo_guardian";

        // ---- O Rei da Fumaça (84): the smoke copy of the hero ----

        public const float SmokeCopyIntervalSeconds = 10f;
        public const float SmokeCopyWindupSeconds = 2.2f;
        public const float SmokeCopyDamageFactor = 1.4f;

        /// <summary>How far round the king the smoke chokes; the copy never rises closer to him than <see cref="SmokeCopyMinDistance"/>.</summary>
        public const float SmokeCopyReach = 16f;

        public const float SmokeCopyMinDistance = 3f;

        // ---- A Leoa do Fogo Devorador (85): the stripes of the floor ----

        public const float LionessIntervalSeconds = 10f;
        public const float LionessWindupSeconds = 2.2f;
        public const float LionessDamageFactor = 1.4f;

        /// <summary>The width of a stripe of the floor (the ficha's stones), laid out from where she stood; it burns within this reach of her.</summary>
        public const float LionessStripe = 4f;

        public const float LionessReach = 14f;

        // ---- O Peixe de Cauda Negra (87): the tail sweeps the hero's side ----

        public const float TailIntervalSeconds = 10f;
        public const float TailWindupSeconds = 2.4f;
        public const float TailDamageFactor = 1.4f;

        /// <summary>How far the black tail reaches round the fish, on the side the hero stood.</summary>
        public const float TailReach = 9f;

        // ---- O Dragão do Fundo (88): one lamp in the dark ----

        public const float LampIntervalSeconds = 11f;
        public const float LampWindupSeconds = 2.4f;
        public const float LampDamageFactor = 1.45f;

        /// <summary>The dark falls this far round the dragon; one lamp stays lit, this far from him across the line to the hero, this wide.</summary>
        public const float DarkReach = 16f;

        public const float LampDistance = 6f;

        public const float LampRadius = 3f;

        // ---- O Guardião do Novelo (89): the waves of the Novelo, and his shelter ----

        public const float NoveloIntervalSeconds = 10f;
        public const float NoveloWindupSeconds = 2.4f;
        public const float NoveloDamageFactor = 1.4f;

        /// <summary>How far round the Guardian the wave washes; his shelter runs this deep behind him (away from the Novelo) and this wide either side.</summary>
        public const float NoveloReach = 18f;

        public const float ShelterDepth = 7f;

        public const float ShelterHalfWidth = 3f;

        /// <summary>Where the waves come from: the dark globe of the Novelo lies east of the bridge (OverworldLayouts.NoveloGlobeCenter), so they roll west.</summary>
        public static readonly Vector2 NoveloWaveFrom = Vector2.right;

        /// <summary>Where the smoke copy of the hero rises: on the hero's spot <paramref name="mark"/>, never closer to the king than <see cref="SmokeCopyMinDistance"/>.</summary>
        public static Vector2 SmokeCopySpot(Vector2 king, Vector2 mark)
        {
            Vector2 line = mark - king;
            float distance = line.magnitude;
            Vector2 along = distance > 0.01f ? line / distance : Vector2.up;
            return king + along * Mathf.Max(distance, SmokeCopyMinDistance);
        }

        /// <summary>Does the smoke of O Rei da Fumaça (at <paramref name="king"/>) choke a hero at <paramref name="hero"/>? Within his reach, nearer the copy than him; nearer him (or behind him) is safe.</summary>
        public static bool SmokeCopyHits(Vector2 king, Vector2 copy, Vector2 hero)
        {
            float toKing = (hero - king).sqrMagnitude;
            return toKing <= SmokeCopyReach * SmokeCopyReach && (hero - copy).sqrMagnitude < toKing;
        }

        /// <summary>Do the stripes A Leoa lights on roar number <paramref name="roar"/> run north–south (even roars) or east–west (odd ones)?</summary>
        public static bool LionessStripesRunNorth(int roar) => Mathf.Abs(roar) % 2 == 0;

        /// <summary>The stripe of the floor under <paramref name="p"/>, the stripes laid out from <paramref name="lioness"/>, counted across them.</summary>
        public static int LionessStripeOf(bool runNorth, Vector2 lioness, Vector2 p) =>
            Mathf.FloorToInt((runNorth ? p.x - lioness.x : p.y - lioness.y) / LionessStripe);

        /// <summary>
        /// Does the floor burn under a hero at <paramref name="hero"/>? A Leoa (at <paramref name="lioness"/>) lights the hero's stripe (where they stood,
        /// <paramref name="mark"/>) and every other one from it, within her reach; the dark stripes between are safe, one step across.
        /// </summary>
        public static bool LionessStripeBurns(bool runNorth, Vector2 lioness, Vector2 mark, Vector2 hero)
        {
            if ((hero - lioness).sqrMagnitude > LionessReach * LionessReach)
            {
                return false;
            }

            return ((LionessStripeOf(runNorth, lioness, hero) - LionessStripeOf(runNorth, lioness, mark)) & 1) == 0;
        }

        /// <summary>Does the black tail of the fish (at <paramref name="fish"/>) sweep a hero at <paramref name="hero"/>? Within its reach, on the side of the hero's spot <paramref name="mark"/>; the other side is safe.</summary>
        public static bool TailSweepHits(Vector2 fish, Vector2 mark, Vector2 hero)
        {
            Vector2 offset = hero - fish;
            if (offset.sqrMagnitude > TailReach * TailReach)
            {
                return false;
            }

            Vector2 side = mark - fish;
            if (side.sqrMagnitude < 0.01f)
            {
                side = Vector2.up;
            }

            return Vector2.Dot(offset, side) > 0f;
        }

        /// <summary>Does the lamp of breath number <paramref name="breath"/> stay lit on the dragon's left of the line to the hero (even breaths) or on his right?</summary>
        public static bool LampOnLeft(int breath) => Mathf.Abs(breath) % 2 == 0;

        /// <summary>Where the one lamp stays lit when O Dragão do Fundo (at <paramref name="dragon"/>) breathes the dark: across the line to the hero's spot <paramref name="mark"/>.</summary>
        public static Vector2 LampSpot(int breath, Vector2 dragon, Vector2 mark)
        {
            Vector2 line = mark - dragon;
            Vector2 along = line.sqrMagnitude > 0.01f ? line.normalized : Vector2.up;
            Vector2 left = new Vector2(-along.y, along.x);
            return dragon + (LampOnLeft(breath) ? left : -left) * LampDistance;
        }

        /// <summary>Does the dark of O Dragão do Fundo (at <paramref name="dragon"/>) bite a hero at <paramref name="hero"/>? Within his reach, out of the lamp's light.</summary>
        public static bool DarkBreathHits(Vector2 lamp, Vector2 dragon, Vector2 hero) =>
            (hero - dragon).sqrMagnitude <= DarkReach * DarkReach && (hero - lamp).sqrMagnitude > LampRadius * LampRadius;

        /// <summary>Is a hero at <paramref name="hero"/> in the shelter of O Guardião do Novelo (at <paramref name="guardian"/>): behind him, away from the Novelo, close enough?</summary>
        public static bool InGuardianShelter(Vector2 guardian, Vector2 hero)
        {
            Vector2 lee = -NoveloWaveFrom.normalized;
            Vector2 offset = hero - guardian;
            float behind = Vector2.Dot(offset, lee);
            return behind >= 0f && behind <= ShelterDepth && Mathf.Abs(lee.x * offset.y - lee.y * offset.x) <= ShelterHalfWidth;
        }

        /// <summary>Does the wave of the Novelo wash over a hero at <paramref name="hero"/>? Within the Guardian's reach, out of his shelter.</summary>
        public static bool NoveloWaveHits(Vector2 guardian, Vector2 hero) =>
            (hero - guardian).sqrMagnitude <= NoveloReach * NoveloReach && !InGuardianShelter(guardian, hero);

        private static float DeepMapBossInterval(RegionBossSignature signature)
        {
            switch (signature)
            {
                case RegionBossSignature.SmokeCopy: return SmokeCopyIntervalSeconds;
                case RegionBossSignature.BurningStripes: return LionessIntervalSeconds;
                case RegionBossSignature.TailSweep: return TailIntervalSeconds;
                case RegionBossSignature.LampHaven: return LampIntervalSeconds;
                default: return NoveloIntervalSeconds;
            }
        }

        private static float DeepMapBossWindup(RegionBossSignature signature)
        {
            switch (signature)
            {
                case RegionBossSignature.SmokeCopy: return SmokeCopyWindupSeconds;
                case RegionBossSignature.BurningStripes: return LionessWindupSeconds;
                case RegionBossSignature.TailSweep: return TailWindupSeconds;
                case RegionBossSignature.LampHaven: return LampWindupSeconds;
                default: return NoveloWindupSeconds;
            }
        }
    }
}
