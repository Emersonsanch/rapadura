using System;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Secrets
{
    /// <summary>What the world map looks like right now, as far as secret reveals care (filled by <see cref="WorldMapSecretsDirector"/>).</summary>
    public struct SecretRevealContext
    {
        /// <summary>Region of the node the token stands on.</summary>
        public BiomeType Region;
        public bool RegionUnlocked;

        /// <summary>Token idle on the node (not travelling, no encounter starting), HUD visible, nothing paused.</summary>
        public bool TokenIdle;

        /// <summary>Seconds the token has been idle on this region without interruption.</summary>
        public float IdleSeconds;

        public bool IsNight;

        /// <summary>Whether a story beat was already seen; null = none seen.</summary>
        public Func<StoryBeat, bool> HasSeenBeat;
    }

    /// <summary>
    /// The secrets' pure rules: when a world-map secret reveals itself and whether an encounter arena gets a
    /// breakable secret (and what it holds). No Unity state; EditMode-tested in SecretsTests.
    /// </summary>
    public static class SecretRules
    {
        /// <summary>Chance an arena hides a breakable secret while its biome's relic is still missing.</summary>
        public const float ArenaChanceRelicMissing = 0.35f;

        /// <summary>Chance once the biome's relic was found (the secret then holds only gold).</summary>
        public const float ArenaChanceRelicFound = 0.12f;

        /// <summary>Boss challenges never get one: the fight deserves the whole screen.</summary>
        public const bool ArenaSecretInBossFights = false;

        /// <summary>Gold inside an arena secret whose relic is already in the collection, per biome tier (Forest = 1).</summary>
        public const int ArenaGoldPerTier = 15;

        /// <summary>Hits needed to break an arena secret (any damage counts as one hit).</summary>
        public const int ArenaSecretHits = 3;

        /// <summary>
        /// True when <paramref name="secret"/> should reveal itself now. Every rule needs the token idle on the
        /// secret's own (unlocked) region; Idle also needs <see cref="WorldSecretDefinition.IdleSeconds"/> of it.
        /// </summary>
        public static bool ShouldReveal(WorldSecretDefinition secret, in SecretRevealContext context)
        {
            if (secret == null || !context.TokenIdle || !context.RegionUnlocked || context.Region != secret.Region)
            {
                return false;
            }

            switch (secret.Rule)
            {
                case SecretRevealRule.Idle:
                    return context.IdleSeconds >= secret.IdleSeconds;
                case SecretRevealRule.Night:
                    return context.IsNight;
                case SecretRevealRule.StoryBeat:
                    return secret.Beat != StoryBeat.None && context.HasSeenBeat != null && context.HasSeenBeat(secret.Beat);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Deterministic per encounter (same payload seed = same answer): does this arena hide a breakable secret?
        /// <paramref name="roll01"/> is a uniform sample in [0, 1) derived from the seed by <see cref="ArenaRoll"/>.
        /// </summary>
        public static bool ShouldSpawnArenaSecret(bool isBossChallenge, bool biomeRelicFound, float roll01)
        {
            if (isBossChallenge && !ArenaSecretInBossFights)
            {
                return false;
            }

            return roll01 < (biomeRelicFound ? ArenaChanceRelicFound : ArenaChanceRelicMissing);
        }

        /// <summary>Stable [0, 1) sample from an encounter seed (System.Random is platform-stable for a given seed).</summary>
        public static float ArenaRoll(int encounterSeed)
        {
            return (float)new Random(unchecked(encounterSeed * 7919 + 104729)).NextDouble();
        }

        /// <summary>Gold in an arena secret once its relic is already collected: deeper regions pay more.</summary>
        public static int ArenaGoldFor(BiomeType biome)
        {
            return ArenaGoldPerTier * ((int)biome + 1);
        }
    }
}
