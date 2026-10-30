using System;
using System.Collections.Generic;
using Rapadura.Gameplay.EventRooms;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.WorldEvents
{
    /// <summary>Which scheduled world event an encounter is. Serialized in <c>EncounterPayload.worldEvent</c>: never renumber.</summary>
    public enum WorldEventKind
    {
        None = 0,

        /// <summary>A region's world boss ("chefe do mundo").</summary>
        WorldBoss = 1,

        /// <summary>"Invasão de Āz": themed waves + Āz, the bando's Capitã.</summary>
        Invasion = 2,
    }

    /// <summary>
    /// One region's world boss: who it is by day and by night (an existing enemy definition, so it walks with that
    /// model's gait and attacks with its AI; <see cref="WorldBossTag"/> makes it huge, crowns it and scales it), its
    /// slot in the calendar, the minimum level and its loot hooks (a rare mount, the region boss's house-item group).
    /// </summary>
    public sealed class WorldBossDefinition
    {
        public readonly string Id;
        public readonly BiomeType Region;

        /// <summary>EnemyDefinition asset of the day form / of the night form (same one when the night only recolours it).</summary>
        public readonly string DayEnemyId;
        public readonly string NightEnemyId;

        public readonly string DayNameEn;
        public readonly string DayNamePt;
        public readonly string NightNameEn;
        public readonly string NightNamePt;

        /// <summary>Minutes after server-local midnight of the first window of the day (then every <see cref="WorldBossRules.PeriodMinutes"/>).</summary>
        public readonly int OffsetMinutes;

        public readonly int MinLevel;

        /// <summary>The rare mount it may leave (Docs/Mounts.md), with its chance per first defeat of a window, by day / by night.</summary>
        public readonly string MountId;
        public readonly float MountDayChance;
        public readonly float MountNightChance;

        /// <summary>Region boss whose "Drop:" house-item group the world boss shares (<c>HouseDropRules.BossPool</c>).</summary>
        public readonly string HouseGroupBossId;

        public WorldBossDefinition(string id, BiomeType region, string dayEnemyId, string nightEnemyId,
            string dayNameEn, string dayNamePt, string nightNameEn, string nightNamePt, int offsetMinutes, int minLevel,
            string mountId, float mountDayChance, float mountNightChance, string houseGroupBossId)
        {
            Id = id;
            Region = region;
            DayEnemyId = dayEnemyId;
            NightEnemyId = nightEnemyId;
            DayNameEn = dayNameEn;
            DayNamePt = dayNamePt;
            NightNameEn = nightNameEn;
            NightNamePt = nightNamePt;
            OffsetMinutes = offsetMinutes;
            MinLevel = minLevel;
            MountId = mountId;
            MountDayChance = mountDayChance;
            MountNightChance = mountNightChance;
            HouseGroupBossId = houseGroupBossId;
        }

        public string NameKey(bool night) => "world_boss." + Id + (night ? ".night" : ".day");

        public string EnemyId(bool night) => night ? NightEnemyId : DayEnemyId;

        /// <summary>True when the night form is the same model recoloured (moonlit tint + glowing eyes) rather than another creature.</summary>
        public bool NightIsRecolour => NightEnemyId == DayEnemyId;

        public float MountChance(bool night) => night ? MountNightChance : MountDayChance;

        public RealTimeSchedule Schedule => new RealTimeSchedule(WorldBossRules.PeriodMinutes, OffsetMinutes, WorldBossRules.WindowMinutes);
    }

    /// <summary>Everything a first world-boss defeat of a window pays on top of the kill itself.</summary>
    public readonly struct WorldBossReward
    {
        public readonly int Gold;
        public readonly int BlessingStones;
        public readonly int SoulStones;

        /// <summary>A house item from the region boss's group goes to the Depósito.</summary>
        public readonly bool HouseItem;

        /// <summary>Mount that joins the cousins (null: none this time).</summary>
        public readonly string MountId;

        public WorldBossReward(int gold, int blessingStones, int soulStones, bool houseItem, string mountId)
        {
            Gold = gold;
            BlessingStones = blessingStones;
            SoulStones = soulStones;
            HouseItem = houseItem;
            MountId = mountId;
        }
    }

    /// <summary>
    /// Numbers and rules of the world bosses (Docs/WorldEvents.md §3), pure and EditMode-tested (WorldEventsTests).
    /// One boss per region, each on its own 3-hour real-time slot (30 minutes open), staggered 45 minutes apart so a
    /// boss shows up somewhere every 45 minutes; at server night (18h–6h Brasília) it comes in its night form.
    /// Online (FASE 12): the boss becomes one shared monster per region (MU Online style); the same numbers are the
    /// per-player solo baseline and the server multiplies the health by the number of fighters.
    /// </summary>
    public static class WorldBossRules
    {
        public const int PeriodMinutes = 180;
        public const int WindowMinutes = 30;

        // Kill-equivalents of the region's common enemy (the base definitions copy it: 70 / 110 / 140 / 170 HP).
        /// <summary>Health: 15 commons by day (Floresta 1.050 … Cavernas 2.550: ~2× the region boss), ×1,25 at night.</summary>
        public const float HealthCommons = 15f;
        public const float NightHealthMultiplier = 1.25f;

        /// <summary>PercentAdditive AttackDamage: +40% by day, +60% at night.</summary>
        public const float DayDamageBonus = 0.4f;
        public const float NightDamageBonus = 0.6f;

        /// <summary>Kill XP: 12 commons (the region boss pays 8), ×1,3 at night.</summary>
        public const float ExperienceCommons = 12f;
        public const float NightExperienceMultiplier = 1.3f;

        public const float DayVisualScale = 1.9f;
        public const float NightVisualScale = 2.05f;

        /// <summary>Real seconds of the fight before the boss goes away (the window may close earlier: it does not cut a fight short).</summary>
        public const float FightSeconds = 300f;

        /// <summary>The boss calls 2 of the region's commons every 22 s (first after 12 s), at most 3 alive.</summary>
        public const float FirstMinionsSeconds = 12f;
        public const float MinionIntervalSeconds = 22f;
        public const int MinionsPerCall = 2;
        public const int MaxMinionsAlive = 3;

        /// <summary>Below half health it roars once and calls 3 at once (the "fúria").</summary>
        public const float RageHealthFraction = 0.5f;
        public const int RageMinions = 3;

        public const string BossForestId = "lobo_guara_encantado";
        public const string BossDesertId = "escorpiao_rei";
        public const string BossMountainId = "gigante_da_serra";
        public const string BossCavesId = "rei_da_revoada";

        public static readonly IReadOnlyList<WorldBossDefinition> All = new[]
        {
            new WorldBossDefinition(BossForestId, BiomeType.Forest, "enemy_forest_wolf", "enemy_forest_werewolf",
                "Saklas, the Ash Wolf", "Saklas, o Lobo de Cinzas", "Saklas, the Mardagayl King", "Saklas, o Rei Mardagayl",
                offsetMinutes: 0, minLevel: 3, MountWaveCatalog.LightJaguar, 0.02f, 0.04f, MountDropRules.AlphaWolf),
            new WorldBossDefinition(BossDesertId, BiomeType.Desert, "enemy_desert_scorpion", "enemy_desert_scorpion",
                "Namrael, the Scorpion King", "Namrael, o Escorpião-Rei", "Namrael of the Full Moon", "Namrael da Lua Cheia",
                offsetMinutes: 45, minLevel: 6, MountWaveCatalog.FireRhea, 0.015f, 0.03f, MountDropRules.RuinGuardian),
            new WorldBossDefinition(BossMountainId, BiomeType.Mountain, "enemy_mountain_stone_golem", "enemy_mountain_stone_golem",
                "Mazan, Giant of the Range", "Mazan, o Gigante da Serra", "Mazan, the Moonstone Giant", "Mazan, o Gigante da Pedra-Lua",
                offsetMinutes: 90, minLevel: 9, MountWaveCatalog.SnowTiger, 0.02f, 0.04f, MountDropRules.MountainWarlord),
            new WorldBossDefinition(BossCavesId, BiomeType.Caves, "enemy_cave_bat_swarm", "enemy_cave_vampire_bat",
                "Mahaway, King of the Flock", "Mahaway, o Rei da Revoada", "Mahaway, the Great Bat of Darkness", "Mahaway, o Morcegão da Treva",
                offsetMinutes: 135, minLevel: 13, MountWaveCatalog.Kirin, 0.008f, 0.016f, MountDropRules.CaveDweller),
        };

        public static WorldBossDefinition For(BiomeType region)
        {
            foreach (WorldBossDefinition boss in All)
            {
                if (boss.Region == region)
                {
                    return boss;
                }
            }

            return All[0];
        }

        public static WorldBossDefinition Find(string id)
        {
            foreach (WorldBossDefinition boss in All)
            {
                if (boss.Id == id)
                {
                    return boss;
                }
            }

            return null;
        }

        public static RealTimeWindow WindowAt(WorldBossDefinition boss, DateTime utc) => WorldEventCalendar.WindowAt(boss.Schedule, utc);

        /// <summary>The form of window <paramref name="window"/>: decided once, at its start, so it never changes mid-fight.</summary>
        public static bool IsNightWindow(RealTimeWindow window) => WorldEventCalendar.IsNight(window.StartUtc);

        /// <summary>
        /// The boss's stats on top of its base definition (which holds one common of its region): health/XP in commons,
        /// a damage buff and its size. Reuses the event rooms' scaler (<c>EventRoomEnemyScaler</c>).
        /// </summary>
        public static EventRoomEnemyScale Scale(bool night)
        {
            return new EventRoomEnemyScale(
                HealthCommons * (night ? NightHealthMultiplier : 1f),
                night ? NightDamageBonus : DayDamageBonus,
                ExperienceCommons * (night ? NightExperienceMultiplier : 1f),
                night ? NightVisualScale : DayVisualScale);
        }

        /// <summary>Minions the boss calls now (0 = none): first after 12 s, then every 22 s, capped by the alive limit.</summary>
        public static int MinionsDue(float elapsed, float sinceLastCall, int minionsAlive)
        {
            if (elapsed < FirstMinionsSeconds || sinceLastCall < MinionIntervalSeconds)
            {
                return 0;
            }

            return Math.Max(0, Math.Min(MinionsPerCall, MaxMinionsAlive - Math.Max(0, minionsAlive)));
        }

        public static bool ShouldRage(float healthFraction, bool alreadyRaged) => !alreadyRaged && healthFraction > 0f && healthFraction <= RageHealthFraction;

        // ------------------------------------------------------------------
        // Rewards (first defeat of a window; seeded: no client RNG)
        // ------------------------------------------------------------------

        public const int BaseGold = 20;
        public const float NightGoldMultiplier = 1.25f;
        public const float HouseItemDayChance = 0.5f;
        public const float HouseItemNightChance = 0.65f;

        /// <summary>
        /// The reward of a first defeat in a window. <paramref name="seed"/> = <see cref="WorldEventCalendar.RewardSeed"/>
        /// (window + player): two draws in a fixed order (house item, then mount), so a given window always pays the same.
        /// <paramref name="mountAvailable"/> = the species is playable (default <see cref="MountDropRules.IsPlayable"/>).
        /// </summary>
        public static WorldBossReward RewardFor(WorldBossDefinition boss, bool night, int seed, Func<string, bool> mountAvailable = null)
        {
            if (boss == null)
            {
                return default;
            }

            int tier = (int)boss.Region;
            var random = new Random(seed);
            int gold = (int)Math.Round(BaseGold * (1f + 0.5f * tier) * (night ? NightGoldMultiplier : 1f));
            int blessing = 2 + (night ? 1 : 0);
            int soul = tier >= 1 ? 1 : 0;
            bool house = random.NextDouble() < (night ? HouseItemNightChance : HouseItemDayChance);

            string mount = null;
            double mountRoll = random.NextDouble();
            if (!string.IsNullOrEmpty(boss.MountId) && (mountAvailable ?? MountDropRules.IsPlayable)(boss.MountId) && mountRoll < boss.MountChance(night))
            {
                mount = boss.MountId;
            }

            return new WorldBossReward(gold, blessing, soul, house, mount);
        }
    }
}
