using System;
using System.Collections.Generic;
using Rapadura.Gameplay.EventRooms;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.WorldEvents
{
    /// <summary>What one "Arca Resgatada +N" (id: <c>CangacoChest</c>) holds (before the bag decides what fits).</summary>
    public readonly struct CangacoChest
    {
        /// <summary>+1 … +5 (0 = no chest).</summary>
        public readonly int Tier;

        public readonly int Experience;
        public readonly int Gold;
        public readonly int BlessingStones;
        public readonly int SoulStones;

        /// <summary>A house item (the region's group) goes to the Depósito.</summary>
        public readonly bool HouseItem;

        /// <summary>Mount that joins the cousins (null: none this time).</summary>
        public readonly string MountId;

        public CangacoChest(int tier, int experience, int gold, int blessingStones, int soulStones, bool houseItem, string mountId)
        {
            Tier = tier;
            Experience = experience;
            Gold = gold;
            BlessingStones = blessingStones;
            SoulStones = soulStones;
            HouseItem = houseItem;
            MountId = mountId;
        }

        public bool IsEmpty => Tier <= 0;
    }

    /// <summary>
    /// "Invasão de Āz" (Docs/WorldEvents.md §4; MU Online's Golden Invasion + Box of Kundun, Tibia's raids): every
    /// 4 real hours the bando of Āz (Greed, the arconte captain; always female) takes one region for 30 minutes. The bando are
    /// the region's own animals wearing her enchanted little tin crowns (code-built, <see cref="WorldEventRegalia"/>): four
    /// waves, then the captain (Āz). Participation points pay one "Arca Resgatada +1…+5" per window (what Āz hid, given back). Pure and EditMode-tested.
    /// Online (FASE 12): the waves become shared on the region map; points stay per player (kills to the killer, wave and
    /// Capitão bonuses to everyone present) and the server rolls every chest with the same seed.
    /// </summary>
    public static class InvasionRules
    {
        /// <summary>02h, 06h, 10h, 14h, 18h and 22h (Brasília), 30 minutes each.</summary>
        public static readonly RealTimeSchedule Schedule = new RealTimeSchedule(240, 120, 30);

        /// <summary>Region of each window, in turn (6 windows a day: the rotation shifts every day).</summary>
        public static readonly IReadOnlyList<BiomeType> Rotation = new[] { BiomeType.Forest, BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves };

        public const int MinLevel = 3;

        public static RealTimeWindow WindowAt(DateTime utc) => WorldEventCalendar.WindowAt(Schedule, utc);

        public static BiomeType RegionFor(long windowIndex)
        {
            long i = windowIndex % Rotation.Count;
            return Rotation[(int)(i < 0 ? i + Rotation.Count : i)];
        }

        // ------------------------------------------------------------------
        // Fight
        // ------------------------------------------------------------------

        public const float FightSeconds = 240f;

        /// <summary>Wave sizes ("batedores", "bando", "bando grande", "o bando todo"), then the captain (Āz).</summary>
        public static readonly IReadOnlyList<int> WaveSizes = new[] { 3, 4, 5, 6 };

        public const float WaveIntervalSeconds = 25f;
        public const int MaxAlive = 6;

        public static int WaveCount => WaveSizes.Count;

        public static int TotalBandits
        {
            get
            {
                int total = 0;
                foreach (int size in WaveSizes)
                {
                    total += size;
                }

                return total;
            }
        }

        /// <summary>A bandit: the region's common in a hat, a bit tougher (health/XP ×1,2, +10% damage).</summary>
        public static readonly EventRoomEnemyScale BanditScale = new EventRoomEnemyScale(1.2f, 0.1f, 1.2f, 1.05f);

        /// <summary>Āz, the bando's captain: the region world boss's day base, ×6 health/XP, +35% damage, ×1,5 size.</summary>
        public static readonly EventRoomEnemyScale CaptainScale = new EventRoomEnemyScale(6f, 0.35f, 6f, 1.5f);

        /// <summary>Whether the next wave comes now (same rule as the event rooms: field almost clear or the interval passed).</summary>
        public static bool ShouldSpawnWave(int wavesSpawned, int alive, float sinceLastWave)
        {
            return wavesSpawned < WaveCount && EventRoomRules.ShouldSpawnWave(alive, sinceLastWave, WaveIntervalSeconds, MaxAlive);
        }

        /// <summary>The Capitão comes once every wave has been sent and the field is clear.</summary>
        public static bool ShouldSpawnCaptain(int wavesSpawned, int alive, bool captainSpawned)
        {
            return !captainSpawned && wavesSpawned >= WaveCount && alive <= 0;
        }

        public static int WaveSize(int waveIndex) => WaveSizes[Math.Max(0, Math.Min(waveIndex, WaveSizes.Count - 1))];

        // ------------------------------------------------------------------
        // Participation and chest tier
        // ------------------------------------------------------------------

        public const int KillPoints = 10;
        public const int CaptainPoints = 80;

        /// <summary>Defeating the captain (Āz) "frees the region".</summary>
        public const int ClearBonus = 40;

        /// <summary>At the clear: 1 point per 2 whole seconds left.</summary>
        public const int SecondsPerTimePoint = 2;

        /// <summary>Points for +1, +2, +3, +4, +5. +4 needs the captain (Āz); +5 needs her with about a minute to spare.</summary>
        public static readonly IReadOnlyList<int> TierThresholds = new[] { 30, 90, 160, 260, 330 };

        public const int MaxTier = 5;

        public static int Points(int kills, bool captainDefeated, float secondsLeft)
        {
            int points = Math.Max(0, kills) * KillPoints;
            if (captainDefeated)
            {
                points += CaptainPoints + ClearBonus + (int)Math.Floor(Math.Max(0f, secondsLeft)) / SecondsPerTimePoint;
            }

            return points;
        }

        /// <summary>Chest tier for <paramref name="points"/> (0 = none).</summary>
        public static int TierFor(int points)
        {
            int tier = 0;
            for (int i = 0; i < TierThresholds.Count; i++)
            {
                if (points >= TierThresholds[i])
                {
                    tier = i + 1;
                }
            }

            return tier;
        }

        // ------------------------------------------------------------------
        // Chest contents (seeded: no client RNG)
        // ------------------------------------------------------------------

        /// <summary>Per tier (index 0 = no chest): XP in commons of the region, gold at the Floresta (+50% per region tier), stones, house and mount chances.</summary>
        public static readonly IReadOnlyList<int> ChestExperienceCommons = new[] { 0, 3, 6, 10, 15, 22 };
        public static readonly IReadOnlyList<int> ChestGold = new[] { 0, 6, 12, 20, 32, 45 };
        public static readonly IReadOnlyList<int> ChestBlessing = new[] { 0, 0, 1, 1, 2, 2 };
        public static readonly IReadOnlyList<int> ChestSoul = new[] { 0, 0, 0, 0, 0, 1 };
        public static readonly IReadOnlyList<float> ChestHouseChance = new[] { 0f, 0f, 0.1f, 0.25f, 0.4f, 0.6f };
        public static readonly IReadOnlyList<float> ChestMountChance = new[] { 0f, 0f, 0f, 0f, 0.005f, 0.02f };

        /// <summary>The bando's horse: the Cavalo Pampa (only if the cousins don't have one yet).</summary>
        public const string ChestMountId = MountWaveCatalog.PintoHorse;

        /// <summary>Kill XP of the region's common (Floresta 10 … Cavernas 40), the chest's XP unit.</summary>
        public static int CommonExperience(BiomeType region)
        {
            int tier = Math.Max(0, Math.Min((int)region, EventRoomRules.TierExperience.Count - 1));
            return EventRoomRules.TierExperience[tier];
        }

        /// <summary>
        /// The "Arca Resgatada +<paramref name="tier"/>" of one window for one player. <paramref name="seed"/> =
        /// <see cref="WorldEventCalendar.RewardSeed"/>; two draws in a fixed order (house item, then mount).
        /// <paramref name="alreadyOwnsMount"/>: the Cavalo Pampa is not given twice (that roll pays nothing extra).
        /// </summary>
        public static CangacoChest ChestFor(int tier, BiomeType region, int seed, bool alreadyOwnsMount = false, Func<string, bool> mountAvailable = null)
        {
            int t = Math.Max(0, Math.Min(tier, MaxTier));
            if (t == 0)
            {
                return new CangacoChest(0, 0, 0, 0, 0, false, null);
            }

            int regionTier = Math.Max(0, (int)region);
            var random = new Random(seed);
            bool house = random.NextDouble() < ChestHouseChance[t];
            double mountRoll = random.NextDouble();
            string mount = !alreadyOwnsMount && (mountAvailable ?? MountDropRules.IsPlayable)(ChestMountId) && mountRoll < ChestMountChance[t]
                ? ChestMountId
                : null;

            int experience = ChestExperienceCommons[t] * CommonExperience(region);
            int gold = (int)Math.Round(ChestGold[t] * (1f + 0.5f * regionTier));
            return new CangacoChest(t, experience, gold, ChestBlessing[t], ChestSoul[t], house, mount);
        }
    }
}
