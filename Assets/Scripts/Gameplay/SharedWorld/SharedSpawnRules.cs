using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Night;
using UnityEngine;

namespace Rapadura.Gameplay.SharedWorld
{
    /// <summary>
    /// How long a fallen monster of a clearing takes to come back in the shared world (Network.md §18). Never renumber.
    /// </summary>
    public enum SpawnTier
    {
        /// <summary>A regular monster or its night variant: the clearing's own <c>RespawnSeconds</c> (25 s by default).</summary>
        Common = 0,

        /// <summary>A night species that is not a mini-boss (owl, Ubır, boars, sparks): several times slower.</summary>
        Elite = 1,

        /// <summary>A night mini-boss (Mardagayl, the Twig-Bird): a long, fixed wait and one at a time per region.</summary>
        Rare = 2,
    }

    /// <summary>
    /// The pure respawn agenda of the shared world (TODO.md 12.3 "mapas abertos por região, com inimigos que reaparecem"):
    /// how long each tier waits, the server-seeded variation, the density that grows with the players in the region and
    /// where a monster may appear (never on top of, nor in sight of, a player). No Unity objects, no clock of its own: the
    /// server passes its time and its seed, so every client sees the same monster at the same moment.
    /// <para>Offline today (<c>MapMonsterSpawner</c>): a clearing keeps up to <c>MaxAlive</c>, refills one every 0.4 s, waits
    /// <c>RespawnSeconds</c> after a death (no variation) and is despawned whole when the hero walks 125 m off (it refills
    /// from empty when they come back). Online the same clearings (<c>OverworldLayout.World.Zones</c>) are kept, with the
    /// numbers below; the per-clearing state is <see cref="SharedClearingSchedule"/>.</para>
    /// </summary>
    public static class SharedSpawnRules
    {
        // ------------------------------------------------------------------
        // Respawn time per tier
        // ------------------------------------------------------------------

        /// <summary>The offline default of a clearing (<c>MapSpawnZone.RespawnSeconds</c>), used when a clearing gives none.</summary>
        public const float DefaultCommonSeconds = 25f;

        /// <summary>Never faster than this, whatever the clearing or the crowd says (the offline spawner's own floor is 1 s).</summary>
        public const float MinRespawnSeconds = 8f;

        /// <summary>An elite waits this many times the clearing's common time…</summary>
        public const float EliteMultiplier = 4f;

        /// <summary>…and never less than this.</summary>
        public const float EliteMinSeconds = 90f;

        /// <summary>A rare mini-boss: 15 minutes, whatever the clearing (MU's "golden" monsters keep their own clock).</summary>
        public const float RareSeconds = 900f;

        /// <summary>Variation (±) of each tier's time, from the server seed: no two clearings tick together, no player can time a spawn to the second.</summary>
        public const float CommonJitter = 0.15f;
        public const float EliteJitter = 0.2f;
        public const float RareJitter = 0.3f;

        /// <summary>Spacing between two monsters filling the same clearing (the offline <c>FillIntervalSeconds</c>).</summary>
        public const float FillIntervalSeconds = 0.4f;

        /// <summary>A spawn that found no allowed point tries again after this (the offline spawner's 2 s).</summary>
        public const float RetrySeconds = 2f;

        // ------------------------------------------------------------------
        // Density
        // ------------------------------------------------------------------

        /// <summary>Extra share of a clearing per player in the region beyond the first.</summary>
        public const float DensityPerExtraPlayer = 0.25f;

        /// <summary>The density never passes ×2 (reached with 5 players in the region).</summary>
        public const float MaxDensity = 2f;

        /// <summary>Hard cap of monsters alive in one clearing, whatever its <c>MaxAlive</c> × density.</summary>
        public const int MaxAlivePerClearing = 10;

        // ------------------------------------------------------------------
        // Where
        // ------------------------------------------------------------------

        /// <summary>Never closer than this to any player (m): the offline <c>MinPlayerDistance</c>.</summary>
        public const float MinPlayerDistance = 9f;

        /// <summary>Preferred: out of every player's sight (m), about the top-down camera's half view on foot.</summary>
        public const float SightRadius = 24f;

        /// <summary>Candidate points a spawn tries before settling for the least-seen one.</summary>
        public const int SpawnPointAttempts = 8;

        /// <summary>The offline spawner keeps its points within 85% of the clearing's radius.</summary>
        public const float ClearingFill = 0.85f;

        // ------------------------------------------------------------------
        // Waking and sleeping
        // ------------------------------------------------------------------

        /// <summary>A clearing runs while some player is this close to its edge (the offline <c>SpawnerActiveRadius</c>).</summary>
        public const float WakeRadius = 85f;

        /// <summary>…and keeps running until every player is past this (the offline <c>SpawnerSleepRadius</c>).</summary>
        public const float SleepRadius = 125f;

        /// <summary>Online a clearing only sleeps after this long with nobody inside <see cref="SleepRadius"/> (no flicker at the border).</summary>
        public const float SleepGraceSeconds = 60f;

        // ------------------------------------------------------------------

        /// <summary>
        /// The tier of a rolled spawn entry: night species are elites, the night mini-bosses (×2 kill XP or more:
        /// Mardagayl ×5, the Twig-Bird ×2) are rare; everything else (normal and night variant) is common.
        /// </summary>
        public static SpawnTier TierOf(NightForm form, NightSpecies species)
        {
            if (form != NightForm.Species || species == null)
            {
                return SpawnTier.Common;
            }

            return species.ExperienceMultiplier >= 2f && species.MaxPerWave <= 1 ? SpawnTier.Rare : SpawnTier.Elite;
        }

        /// <summary>The tier of a spawn entry of a clearing's table (<see cref="PeriodSpawnEntry"/>).</summary>
        public static SpawnTier TierOf(PeriodSpawnEntry entry)
        {
            if (entry == null)
            {
                return SpawnTier.Common;
            }

            return TierOf(entry.Form, entry.Form == NightForm.Species ? NightVariantRules.FindSpecies(entry.SpeciesId) : null);
        }

        /// <summary>The tier of a monster by its species id (<c>MapMonsterInfo.SpeciesId</c>; null = a common).</summary>
        public static SpawnTier TierOfSpecies(string speciesId)
        {
            NightSpecies species = NightVariantRules.FindSpecies(speciesId);
            return species == null ? SpawnTier.Common : TierOf(NightForm.Species, species);
        }

        /// <summary>The wait of <paramref name="tier"/> before variation and density, for a clearing whose common time is <paramref name="clearingSeconds"/>.</summary>
        public static float BaseSeconds(SpawnTier tier, float clearingSeconds)
        {
            float common = clearingSeconds > 0f ? clearingSeconds : DefaultCommonSeconds;
            switch (tier)
            {
                case SpawnTier.Elite: return Mathf.Max(EliteMinSeconds, common * EliteMultiplier);
                case SpawnTier.Rare: return RareSeconds;
                default: return Mathf.Max(MinRespawnSeconds, common);
            }
        }

        /// <summary>
        /// The tier a clearing slot waits on after a death of <paramref name="tier"/>: a rare frees its slot on the common
        /// time (the species itself is held back region-wide by <see cref="SharedRareTracker"/>); the others wait their own.
        /// </summary>
        public static SpawnTier SlotTier(SpawnTier tier) => tier == SpawnTier.Rare ? SpawnTier.Common : tier;

        public static float JitterOf(SpawnTier tier)
        {
            switch (tier)
            {
                case SpawnTier.Elite: return EliteJitter;
                case SpawnTier.Rare: return RareJitter;
                default: return CommonJitter;
            }
        }

        /// <summary>
        /// The density of a region with <paramref name="playersInRegion"/> players: ×1 alone (0 counts as 1), +0.25 per
        /// extra player, at most ×<see cref="MaxDensity"/>.
        /// </summary>
        public static float Density(int playersInRegion)
        {
            int extra = Math.Max(0, playersInRegion - 1);
            return Mathf.Min(MaxDensity, 1f + DensityPerExtraPlayer * extra);
        }

        /// <summary>
        /// How many a clearing with <paramref name="baseMaxAlive"/> holds with that crowd: round(base × density), never
        /// below the base, never above twice the base nor <see cref="MaxAlivePerClearing"/> (a clearing already above that
        /// cap offline keeps its own number).
        /// </summary>
        public static int TargetAlive(int baseMaxAlive, int playersInRegion)
        {
            if (baseMaxAlive <= 0)
            {
                return 0;
            }

            int scaled = Mathf.RoundToInt(baseMaxAlive * Density(playersInRegion));
            int cap = Math.Max(baseMaxAlive, Math.Min(baseMaxAlive * 2, MaxAlivePerClearing));
            return Math.Max(baseMaxAlive, Math.Min(cap, scaled));
        }

        /// <summary>
        /// The wait after a death, in seconds: the tier's base, ± its variation from the server seed (same inputs, same
        /// time on every machine), divided by the density for commons and elites (a crowded region refills faster, at most
        /// twice as fast); a rare keeps its own clock. Never below <see cref="MinRespawnSeconds"/>.
        /// </summary>
        /// <param name="deathIndex">How many monsters of this clearing had fallen before (a counter the clearing keeps).</param>
        public static float RespawnSeconds(SpawnTier tier, float clearingSeconds, int serverSeed, string clearingId, int deathIndex, int playersInRegion)
        {
            float seconds = BaseSeconds(tier, clearingSeconds);
            float jitter = JitterOf(tier);
            double roll = Roll01(serverSeed, clearingId, deathIndex);
            seconds *= 1f + jitter * (float)(roll * 2d - 1d);
            if (tier != SpawnTier.Rare)
            {
                seconds /= Density(playersInRegion);
            }

            return Mathf.Max(MinRespawnSeconds, seconds);
        }

        /// <summary>A deterministic 0..1 from the server seed, a key and an index (FNV-1a + a final mix: stable across .NET versions, unlike <c>string.GetHashCode</c>).</summary>
        public static double Roll01(int serverSeed, string key, int index)
        {
            return (Hash(serverSeed, key, index) & 0xFFFFFF) / (double)0x1000000;
        }

        /// <summary>A stable 32-bit hash of (seed, key, index).</summary>
        public static uint Hash(int serverSeed, string key, int index)
        {
            unchecked
            {
                uint h = 2166136261u;
                h = (h ^ (uint)serverSeed) * 16777619u;
                if (key != null)
                {
                    foreach (char c in key)
                    {
                        h = (h ^ c) * 16777619u;
                    }
                }

                h = (h ^ (uint)index) * 16777619u;
                h ^= h >> 16;
                h *= 0x85EBCA6Bu;
                h ^= h >> 13;
                h *= 0xC2B2AE35u;
                h ^= h >> 16;
                return h;
            }
        }

        // ------------------------------------------------------------------
        // Spawn point
        // ------------------------------------------------------------------

        /// <summary>Is <paramref name="point"/> at least <paramref name="radius"/> m (on the ground plane) from every player?</summary>
        public static bool ClearOfPlayers(Vector2 point, IReadOnlyList<Vector2> players, float radius)
        {
            return NearestPlayerDistance(point, players) >= radius;
        }

        /// <summary>Distance to the nearest player (∞ when nobody is given).</summary>
        public static float NearestPlayerDistance(Vector2 point, IReadOnlyList<Vector2> players)
        {
            float best = float.PositiveInfinity;
            if (players == null)
            {
                return best;
            }

            foreach (Vector2 p in players)
            {
                float d = Vector2.Distance(point, p);
                if (d < best)
                {
                    best = d;
                }
            }

            return best;
        }

        /// <summary>
        /// Where the next monster of a clearing appears: up to <see cref="SpawnPointAttempts"/> seeded points inside it
        /// (<paramref name="allowed"/> filters river, map border and safe zones, as offline). The first one out of every
        /// player's sight (<see cref="SightRadius"/>) wins; failing that, the one farthest from the players that is still
        /// outside <see cref="MinPlayerDistance"/> (a crowd standing in the clearing does not stop it for good). False — try
        /// again in <see cref="RetrySeconds"/> — when every point is within 9 m of a player or not allowed.
        /// </summary>
        public static bool TryPickSpawnPoint(Vector2 center, float radius, IReadOnlyList<Vector2> players, System.Random random,
            out Vector2 point, Func<Vector2, bool> allowed = null)
        {
            point = center;
            if (random == null)
            {
                return false;
            }

            bool found = false;
            float bestDistance = -1f;
            for (int attempt = 0; attempt < SpawnPointAttempts; attempt++)
            {
                double angle = random.NextDouble() * Math.PI * 2d;
                float r = radius * ClearingFill * Mathf.Sqrt((float)random.NextDouble());
                Vector2 candidate = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * r;
                if (allowed != null && !allowed(candidate))
                {
                    continue;
                }

                float distance = NearestPlayerDistance(candidate, players);
                if (distance < MinPlayerDistance)
                {
                    continue;
                }

                if (distance >= SightRadius)
                {
                    point = candidate;
                    return true;
                }

                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    point = candidate;
                    found = true;
                }
            }

            return found;
        }

        /// <summary>Does a clearing whose edge is <paramref name="edgeDistance"/> m from the nearest player wake (it was asleep) or keep running (it was awake)?</summary>
        public static bool InRange(float edgeDistance, bool running) => edgeDistance <= (running ? SleepRadius : WakeRadius);
    }
}
