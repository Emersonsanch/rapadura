using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Items.Sets
{
    /// <summary>One two-piece armour set of the late regions (Docs/Items.md) and the region beasts that drop it.</summary>
    public sealed class ArmorSetDefinition
    {
        public readonly string Id;
        public readonly string ChestItemId;
        public readonly string HelmItemId;
        public readonly IReadOnlyList<string> Beasts;

        public ArmorSetDefinition(string id, string chestItemId, string helmItemId, IReadOnlyList<string> beasts)
        {
            Id = id;
            ChestItemId = chestItemId;
            HelmItemId = helmItemId;
            Beasts = beasts;
        }
    }

    /// <summary>
    /// Pure rules of the region armour sets (same shape as the Armadura de Marfim in <c>DungeonRules</c>): which beasts drop which
    /// piece, how many pieces are worn and when each two-piece bonus fires. <see cref="ArmorSetBonuses"/> only wires them to combat.
    /// </summary>
    public static class ArmorSetRules
    {
        public const int PiecesForBonus = 2;

        /// <summary>Chance of a piece per kill of one of the region's own beasts (one kill in 200; either piece, evenly).</summary>
        public const double DropChance = 0.005;

        public const string PapyrusChestId = "armor_papiro_chest";
        public const string PapyrusHelmId = "armor_papiro_helm";
        public const string BlueBrickChestId = "armor_tijolo_chest";
        public const string BlueBrickHelmId = "armor_tijolo_helm";
        public const string SealChestId = "armor_selo_chest";
        public const string SealHelmId = "armor_selo_helm";
        public const string LightChestId = "armor_luz_chest";
        public const string LightHelmId = "armor_luz_helm";

        /// <summary>Cais do Papiro (Região 13, Rio das Doze Horas).</summary>
        public static readonly ArmorSetDefinition Papyrus = new ArmorSetDefinition("cais_do_papiro", PapyrusChestId, PapyrusHelmId, new[]
        {
            "enemy_forest_black_scorpion", "enemy_forest_creeping_dark", "enemy_forest_dark_river_crocodile", "enemy_forest_dimmed_ibis",
            "enemy_forest_mud_hippo", "enemy_forest_reed_snake", "enemy_forest_reed_snare", "enemy_forest_sand_ushabti",
            "enemy_forest_shadow_biter", "enemy_forest_shadow_locust", "enemy_forest_shadow_scarab", "enemy_forest_three_headed_serpent",
        });

        /// <summary>Tijolo Azul (Região 14, Margens da Mistura).</summary>
        public static readonly ArmorSetDefinition BlueBrick = new ArmorSetDefinition("tijolo_azul", BlueBrickChestId, BlueBrickHelmId, new[]
        {
            "enemy_forest_angry_cedar", "enemy_forest_bitumen_locust", "enemy_forest_brick_lion", "enemy_forest_broken_tablet",
            "enemy_forest_garden_shadow", "enemy_forest_glow_mantle", "enemy_forest_late_shadow", "enemy_forest_living_hourglass",
            "enemy_forest_living_tar", "enemy_forest_tar_heron", "enemy_forest_ullikummi_son", "enemy_forest_young_mushussu",
        });

        /// <summary>Selo dos Artífices (Região 15, Limiar Profundo).</summary>
        public static readonly ArmorSetDefinition Seal = new ArmorSetDefinition("selo_dos_artifices", SealChestId, SealHelmId, new[]
        {
            "enemy_forest_black_eagle", "enemy_forest_copy_shadow", "enemy_forest_dark_fish", "enemy_forest_ember_wolf",
            "enemy_forest_hot_smoke_lion", "enemy_forest_lesser_archon", "enemy_forest_lesser_shadow_dragon", "enemy_forest_mud_octopus",
            "enemy_forest_shadow_lizard", "enemy_forest_smoke_archer", "enemy_forest_smoke_giant", "enemy_forest_wind_harpy",
        });

        /// <summary>Cais de Luz (Região 16, Reinos do Céu).</summary>
        public static readonly ArmorSetDefinition Light = new ArmorSetDefinition("cais_de_luz", LightChestId, LightHelmId, new[]
        {
            "enemy_forest_az_thief", "enemy_forest_balloon_giant", "enemy_forest_chained_archon", "enemy_forest_cracked_jar",
            "enemy_forest_dark_stowaway", "enemy_forest_dimmed_cloud_bird", "enemy_forest_dimmed_light_fish", "enemy_forest_dimmed_light_gull",
            "enemy_forest_hold_shadow", "enemy_forest_living_rope", "enemy_forest_sky_stone", "enemy_forest_toy_archon",
        });

        public static readonly IReadOnlyList<ArmorSetDefinition> All = new[] { Papyrus, BlueBrick, Seal, Light };

        // "Hora da Barca" (Cais do Papiro): a kill may heal the hero.
        public const double BargeHourChance = 0.25;
        public const float BargeHourHealFraction = 0.05f;

        // "Reflexo Vidrado" (Tijolo Azul): a hit taken may bounce back at the attacker.
        public const double GlazedReflectChance = 0.20;
        public const float GlazedReflectFraction = 0.35f;

        // "Selo Reaberto" (Selo dos Artífices): a critical hit may give mana back.
        public const double ReopenedSealChance = 0.25;
        public const float ReopenedSealManaFraction = 0.08f;

        // "Raio da Coluna" (Cais de Luz): a critical hit may strike the enemies nearby.
        public const double ColumnRayChance = 0.15;
        public const float ColumnRayDamageFraction = 0.40f;
        public const float ColumnRayRadius = 5f;
        public const int ColumnRayMaxTargets = 4;

        /// <summary>The set whose own beasts include <paramref name="enemyId"/> (the asset name), or null.</summary>
        public static ArmorSetDefinition SetOfBeast(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId))
            {
                return null;
            }

            foreach (ArmorSetDefinition set in All)
            {
                foreach (string beast in set.Beasts)
                {
                    if (beast == enemyId)
                    {
                        return set;
                    }
                }
            }

            return null;
        }

        /// <summary>The item id one kill of <paramref name="enemyId"/> drops (<paramref name="roll"/>, <paramref name="pieceRoll"/> in [0,1)), or null.</summary>
        public static string RollDrop(string enemyId, double roll, double pieceRoll)
        {
            ArmorSetDefinition set = SetOfBeast(enemyId);
            if (set == null || roll >= DropChance)
            {
                return null;
            }

            return pieceRoll < 0.5 ? set.ChestItemId : set.HelmItemId;
        }

        /// <summary>Distinct pieces of <paramref name="set"/> among <paramref name="equippedItemIds"/>.</summary>
        public static int CountPieces(ArmorSetDefinition set, IEnumerable<string> equippedItemIds)
        {
            if (set == null || equippedItemIds == null)
            {
                return 0;
            }

            bool chest = false;
            bool helm = false;
            foreach (string id in equippedItemIds)
            {
                chest |= id == set.ChestItemId;
                helm |= id == set.HelmItemId;
            }

            return (chest ? 1 : 0) + (helm ? 1 : 0);
        }

        /// <summary>The set completely worn among <paramref name="equippedItemIds"/>, or null (two slots: at most one can be complete).</summary>
        public static ArmorSetDefinition CompleteSet(IEnumerable<string> equippedItemIds)
        {
            if (equippedItemIds == null)
            {
                return null;
            }

            var ids = new List<string>(equippedItemIds);
            foreach (ArmorSetDefinition set in All)
            {
                if (CountPieces(set, ids) >= PiecesForBonus)
                {
                    return set;
                }
            }

            return null;
        }

        public static bool BargeHourProcs(ArmorSetDefinition worn, double roll) => worn == Papyrus && roll < BargeHourChance;

        public static float BargeHourHeal(float maxHealth) => maxHealth > 0f ? maxHealth * BargeHourHealFraction : 0f;

        public static bool GlazedReflectProcs(ArmorSetDefinition worn, double roll) => worn == BlueBrick && roll < GlazedReflectChance;

        /// <summary>Damage bounced back: 35% of the hit taken (never below 1 for a positive hit).</summary>
        public static float GlazedReflectDamage(float damageTaken) => damageTaken > 0f ? Mathf.Max(1f, damageTaken * GlazedReflectFraction) : 0f;

        public static bool ReopenedSealProcs(ArmorSetDefinition worn, bool critical, double roll) => critical && worn == Seal && roll < ReopenedSealChance;

        public static float ReopenedSealMana(float maxMana) => maxMana > 0f ? maxMana * ReopenedSealManaFraction : 0f;

        public static bool ColumnRayProcs(ArmorSetDefinition worn, bool critical, double roll) => critical && worn == Light && roll < ColumnRayChance;

        /// <summary>Damage of each ray: 40% of the critical hit (never below 1 for a positive hit).</summary>
        public static float ColumnRayDamage(float criticalDamage) => criticalDamage > 0f ? Mathf.Max(1f, criticalDamage * ColumnRayDamageFraction) : 0f;
    }
}
