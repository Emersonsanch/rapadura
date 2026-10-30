using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>One "a mount may follow the cousins after this enemy dies" line of Docs/Mounts.md §3.6.</summary>
    public sealed class MountDropEntry
    {
        public readonly string EnemyId;
        public readonly string MountId;

        /// <summary>Chance per death by day.</summary>
        public readonly float DayChance;

        /// <summary>Chance per death at night (the Lobo-Guará doubles, the Onça only shows up at night).</summary>
        public readonly float NightChance;

        public MountDropEntry(string enemyId, string mountId, float dayChance, float nightChance)
        {
            EnemyId = enemyId;
            MountId = mountId;
            DayChance = dayChance;
            NightChance = nightChance;
        }

        public MountDropEntry(string enemyId, string mountId, float chance) : this(enemyId, mountId, chance, chance)
        {
        }

        public float ChanceAt(bool isNight) => isNight ? NightChance : DayChance;
    }

    /// <summary>What a boss's Fitas do Laço buy at the Haras (§3.6): its Rare at 25, its Epic at 80 (Legendary never).</summary>
    public sealed class MountRibbonRecipe
    {
        public readonly string BossId;
        public readonly string RareMountId;

        /// <summary>Null when the boss has no Epic (Lobo Alfa, Guardião da Ruína).</summary>
        public readonly string EpicMountId;

        public MountRibbonRecipe(string bossId, string rareMountId, string epicMountId)
        {
            BossId = bossId;
            RareMountId = rareMountId;
            EpicMountId = epicMountId;
        }
    }

    /// <summary>
    /// The mount drops of Docs/Mounts.md §3.6 and the "Fitas do Laço" bad-luck protection, as pure rules (same pattern
    /// as <c>RefineStoneDrops</c>): a separate loot source with an injected <see cref="Random"/>, rolled once per death,
    /// so the server can own every roll (online: never the client). Enemy ids are the <c>EnemyDefinition</c> asset
    /// names. The whole table of the doc is here; rows whose mount is not playable yet (a later wave: Tatu, Escaravelho,
    /// Grifo, Pégaso, Fênix, Ovo de Dragão) are skipped by <see cref="Roll"/> without drawing a number.
    /// A drop does not become a bag item: the mount joins the collection at once (source <c>drop</c>) so a full
    /// backpack can never lose a 0.2% roll.
    /// </summary>
    public static class MountDropRules
    {
        public const string ForestWolf = "enemy_forest_wolf";
        public const string ForestSpider = "enemy_forest_spider";
        public const string DesertScorpion = "enemy_desert_scorpion";
        public const string StoneGolem = "enemy_mountain_stone_golem";
        public const string BatSwarm = "enemy_cave_bat_swarm";
        public const string AlphaWolf = "enemy_forest_alpha_wolf";
        public const string RuinGuardian = "enemy_desert_ruin_guardian";
        public const string MountainWarlord = "enemy_mountain_warlord";
        public const string CaveDweller = "enemy_cave_dweller";

        public const int RareRibbonCost = 25;
        public const int EpicRibbonCost = 80;

        /// <summary>Fitas do Laço a boss victory gives.</summary>
        public const int RibbonsPerBossVictory = 1;

        public static readonly IReadOnlyList<MountDropEntry> Table = new[]
        {
            // Bichos comuns.
            new MountDropEntry(ForestWolf, MountWaveCatalog.ManedWolf, 0.004f, 0.008f),
            new MountDropEntry(ForestWolf, MountWaveCatalog.LightJaguar, 0f, 0.001f),
            new MountDropEntry(ForestSpider, MountWaveCatalog.LightJaguar, 0.001f),
            new MountDropEntry(DesertScorpion, "mount_golden_scarab", 0.004f),
            new MountDropEntry(StoneGolem, MountWaveCatalog.WoollyMammoth, 0.0015f),
            new MountDropEntry(BatSwarm, "mount_giant_armadillo", 0.004f),
            new MountDropEntry(BatSwarm, MountWaveCatalog.Carbuncle, 0.001f),

            // Chefes.
            new MountDropEntry(AlphaWolf, MountWaveCatalog.Unicorn, 0.02f),
            new MountDropEntry(RuinGuardian, "mount_golden_scarab", 0.04f),
            new MountDropEntry(RuinGuardian, "mount_griffin", 0.02f),
            new MountDropEntry(RuinGuardian, "mount_dragon_hatchling", 0.003f),
            new MountDropEntry(MountainWarlord, MountWaveCatalog.SnowTiger, 0.02f),
            new MountDropEntry(MountainWarlord, MountWaveCatalog.WoollyMammoth, 0.015f),
            new MountDropEntry(MountainWarlord, "mount_pegasus", 0.008f),
            new MountDropEntry(CaveDweller, MountWaveCatalog.Carbuncle, 0.015f),
            new MountDropEntry(CaveDweller, MountWaveCatalog.Kirin, 0.008f),
            new MountDropEntry(CaveDweller, "mount_sertao_phoenix", 0.002f),
        };

        public static readonly IReadOnlyList<MountRibbonRecipe> Ribbons = new[]
        {
            new MountRibbonRecipe(AlphaWolf, MountWaveCatalog.Unicorn, null),
            new MountRibbonRecipe(RuinGuardian, "mount_griffin", null),
            new MountRibbonRecipe(MountainWarlord, MountWaveCatalog.SnowTiger, "mount_pegasus"),
            new MountRibbonRecipe(CaveDweller, MountWaveCatalog.Carbuncle, MountWaveCatalog.Kirin),
        };

        /// <summary>Default availability: the species is in <see cref="MountCatalog"/>.</summary>
        public static bool IsPlayable(string mountId) => MountCatalog.FindSpec(mountId) != null;

        public static IEnumerable<MountDropEntry> EntriesFor(string enemyId) => Table.Where(e => e.EnemyId == enemyId);

        /// <summary>
        /// Rolls the mount drops of one death of <paramref name="enemyId"/>: one number per playable row with a chance
        /// above 0, in table order (a seed reproduces the result). Returns the mount ids that dropped (usually none).
        /// </summary>
        public static List<string> Roll(string enemyId, bool isNight, Random random, Func<string, bool> available = null)
        {
            var dropped = new List<string>();
            if (string.IsNullOrEmpty(enemyId) || random == null)
            {
                return dropped;
            }

            available ??= IsPlayable;
            foreach (MountDropEntry entry in Table)
            {
                if (entry.EnemyId != enemyId || !available(entry.MountId))
                {
                    continue;
                }

                float chance = entry.ChanceAt(isNight);
                if (chance > 0f && random.NextDouble() < chance)
                {
                    dropped.Add(entry.MountId);
                }
            }

            return dropped;
        }

        /// <summary>Deaths needed on average for one drop (∞ when impossible).</summary>
        public static float ExpectedDeaths(float chance) => chance > 0f ? 1f / chance : float.PositiveInfinity;

        // ------------------------------------------------------------------
        // Fitas do Laço
        // ------------------------------------------------------------------

        public static MountRibbonRecipe RecipeFor(string bossId) => Ribbons.FirstOrDefault(r => r.BossId == bossId);

        /// <summary>Whether <paramref name="enemyId"/> gives Fitas do Laço (the four region bosses).</summary>
        public static bool GivesRibbons(string enemyId) => RecipeFor(enemyId) != null;

        /// <summary>Ribbons <paramref name="mountId"/> costs from <paramref name="bossId"/>'s recipe; -1 when it is not on it.</summary>
        public static int CostOf(string bossId, string mountId)
        {
            MountRibbonRecipe recipe = RecipeFor(bossId);
            if (recipe == null || string.IsNullOrEmpty(mountId))
            {
                return -1;
            }

            if (recipe.RareMountId == mountId)
            {
                return RareRibbonCost;
            }

            return recipe.EpicMountId == mountId ? EpicRibbonCost : -1;
        }

        /// <summary>Cost by rarity: Rare 25, Epic 80, anything else never (-1). Legendary never comes from ribbons.</summary>
        public static int CostFor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Rare: return RareRibbonCost;
                case ItemRarity.Epic: return EpicRibbonCost;
                default: return -1;
            }
        }

        /// <summary>Whether <paramref name="ribbons"/> of <paramref name="bossId"/> buy <paramref name="mountId"/> now.</summary>
        public static bool CanRedeem(string bossId, string mountId, int ribbons, Func<string, bool> available = null)
        {
            int cost = CostOf(bossId, mountId);
            return cost > 0 && ribbons >= cost && (available ?? IsPlayable)(mountId);
        }
    }
}
