using System.Collections.Generic;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.Night
{
    /// <summary>What a spawned enemy is at night: itself, its night variant, or a night species built on its model.</summary>
    public enum NightForm
    {
        Normal = 0,

        /// <summary>"Lobo da Mata (noturno)": darker, glowing eyes, a bit bigger, tougher, better rewards.</summary>
        Variant = 1,

        /// <summary>A night-only creature (<see cref="NightSpecies"/>): its own definition and model, with the species' stat multipliers.</summary>
        Species = 2,
    }

    /// <summary>
    /// A night-only creature. Since the night-models pass every species has its own <c>EnemyDefinition</c>
    /// (<see cref="BaseEnemyId"/>, e.g. <c>enemy_forest_werewolf</c>) and procedural model: the definition holds the
    /// home region's common baseline (so loot table, XP tier, bestiary page and quest kill are its own) and drives the
    /// AI/attack; this adds the name and the stat/reward multipliers of Docs/DayNight.md (×5 health for the
    /// Lobisomem, …), a <see cref="Tint"/> (white = none) and <see cref="EyeColor"/> (clear = the model's own glowing
    /// eyes, no extra spheres). <see cref="HomeRegion"/> is where its definition lives; it may also show up in
    /// another region's table as a visitor, scaled to that region (<see cref="NightVariantRules.Resolve(NightForm, NightSpecies, BiomeType?)"/>).
    /// </summary>
    public sealed class NightSpecies
    {
        public readonly string Id;
        public readonly string BaseEnemyId;
        public readonly string NameKey;
        public readonly string NameEn;
        public readonly string NamePtBR;
        public readonly Color Tint;
        public readonly Color EyeColor;
        public readonly float ScaleMultiplier;
        public readonly float HealthMultiplier;
        public readonly float DamageBonus;
        public readonly float AggroRadiusMultiplier;
        public readonly float ExperienceMultiplier;
        public readonly int ExtraLootRolls;

        /// <summary>Most of this species one reinforcement wave may hold (a mini-boss comes alone).</summary>
        public readonly int MaxPerWave;

        /// <summary>Region whose biome table holds its definition (weight 0: never on the day map) — loot and XP tier.</summary>
        public readonly BiomeType HomeRegion;

        /// <summary>Herd size: one roll of the species brings <see cref="PackMin"/>..<see cref="PackMax"/> of them (1..1 = alone).</summary>
        public readonly int PackMin;
        public readonly int PackMax;

        /// <summary>May also come at dusk (fireflies); otherwise night only.</summary>
        public readonly bool ComesAtDusk;

        public NightSpecies(string id, string baseEnemyId, string nameEn, string namePtBR, Color tint, Color eyeColor,
            float scaleMultiplier, float healthMultiplier, float damageBonus, float aggroRadiusMultiplier,
            float experienceMultiplier, int extraLootRolls, int maxPerWave,
            BiomeType homeRegion = BiomeType.Forest, int packMin = 1, int packMax = 1, bool comesAtDusk = false)
        {
            Id = id;
            BaseEnemyId = baseEnemyId;
            NameKey = "enemy.night." + id + ".name";
            NameEn = nameEn;
            NamePtBR = namePtBR;
            Tint = tint;
            EyeColor = eyeColor;
            ScaleMultiplier = scaleMultiplier;
            HealthMultiplier = healthMultiplier;
            DamageBonus = damageBonus;
            AggroRadiusMultiplier = aggroRadiusMultiplier;
            ExperienceMultiplier = experienceMultiplier;
            ExtraLootRolls = extraLootRolls;
            MaxPerWave = Mathf.Max(1, maxPerWave);
            HomeRegion = homeRegion;
            PackMin = Mathf.Clamp(packMin, 1, MaxPerWave);
            PackMax = Mathf.Clamp(packMax, PackMin, MaxPerWave);
            ComesAtDusk = comesAtDusk;
        }

        /// <summary>True for a herd species (more than one per roll).</summary>
        public bool IsPack => PackMax > 1;
    }

    /// <summary>The look and numbers one spawned enemy ends up with (resolved by <see cref="NightVariantRules.Resolve"/>).</summary>
    public readonly struct NightLook
    {
        public readonly NightForm Form;
        public readonly NightSpecies Species;
        public readonly Color Tint;
        public readonly Color EyeColor;
        public readonly float ScaleMultiplier;
        public readonly float HealthMultiplier;
        public readonly float DamageBonus;
        public readonly float AggroRadiusMultiplier;
        public readonly float AggroHoldMultiplier;
        public readonly float ExperienceMultiplier;
        public readonly int ExtraLootRolls;

        public NightLook(NightForm form, NightSpecies species, Color tint, Color eyeColor, float scaleMultiplier,
            float healthMultiplier, float damageBonus, float aggroRadiusMultiplier, float aggroHoldMultiplier,
            float experienceMultiplier, int extraLootRolls)
        {
            Form = form;
            Species = species;
            Tint = tint;
            EyeColor = eyeColor;
            ScaleMultiplier = scaleMultiplier;
            HealthMultiplier = healthMultiplier;
            DamageBonus = damageBonus;
            AggroRadiusMultiplier = aggroRadiusMultiplier;
            AggroHoldMultiplier = aggroHoldMultiplier;
            ExperienceMultiplier = experienceMultiplier;
            ExtraLootRolls = extraLootRolls;
        }

        public bool IsNormal => Form == NightForm.Normal;
    }

    /// <summary>
    /// Pure rules of the night variants and the playable night species (EditMode-tested in <c>NightVariantRulesTests</c>).
    /// A variant is the same enemy at night: tinted <see cref="VariantTint"/> (darker, moonlit blue), two glowing
    /// <see cref="VariantEyeColor"/> eyes (unlit, so they read in the dark as a warning), <see cref="VariantScale"/>
    /// bigger and the stats/rewards of <see cref="NightDangerRules.VariantStats"/>; its name gets the "(noturno)"
    /// suffix. Applied in the arena by <see cref="NightVariantTag"/>, like the road event's elite.
    /// </summary>
    public static class NightVariantRules
    {
        public const float VariantScale = 1.1f;

        /// <summary>Colour multiplier on the variant's materials: darker, a little blue (moonlight), never black.</summary>
        public static readonly Color VariantTint = new Color(0.55f, 0.58f, 0.78f, 1f);

        /// <summary>Warm yellow-green "eyes in the dark": the same hue for every variant so kids learn the signal.</summary>
        public static readonly Color VariantEyeColor = new Color(1f, 0.92f, 0.35f, 1f);

        /// <summary>Red eyes mark the stronger night species (and the mini-boss).</summary>
        public static readonly Color SpeciesEyeColor = new Color(1f, 0.3f, 0.22f, 1f);

        /// <summary>Lowest a tint channel may go, so no recolour turns a model into a silhouette.</summary>
        public const float MinTintChannel = 0.35f;

        public const string SpeciesLobisomem = "lobisomem_do_sertao";
        public const string SpeciesMorcegoVampiro = "morcego_vampiro_gigante";
        public const string SpeciesRasgaMortalha = "coruja_rasga_mortalha";
        public const string SpeciesVagaLume = "vaga_lume_de_fogo";
        public const string SpeciesQueixada = "queixada_da_caipora";
        public const string SpeciesMaeDaLua = "mae_da_lua";

        public const string WerewolfEnemyId = "enemy_forest_werewolf";
        public const string VampireBatEnemyId = "enemy_cave_vampire_bat";
        public const string ShroudOwlEnemyId = "enemy_forest_shroud_owl";
        public const string FireflySwarmEnemyId = "enemy_desert_firefly_swarm";
        public const string PeccaryEnemyId = "enemy_forest_peccary";
        public const string PotooEnemyId = "enemy_mountain_potoo";

        /// <summary>No recolour (the species has its own model and palette).</summary>
        public static readonly Color NoTint = Color.white;

        /// <summary>No extra eye spheres: the species model has its own glowing eyes (emissive palette cells).</summary>
        public static readonly Color ModelEyes = Color.clear;

        /// <summary>
        /// Lobisomem-do-sertão: the rare Forest night mini-boss, its own shaggy model that rears up to strike. Not a
        /// region boss (no IsBoss, not unique, no banner): just a big scary-but-fair night fight. ×5 health, +60% damage.
        /// </summary>
        public static readonly NightSpecies Lobisomem = new NightSpecies(
            SpeciesLobisomem, WerewolfEnemyId, "Sertão Werewolf", "Lobisomem-do-sertão",
            NoTint, ModelEyes,
            scaleMultiplier: 1f, healthMultiplier: 5f, damageBonus: 0.6f, aggroRadiusMultiplier: 1.6f,
            experienceMultiplier: 5f, extraLootRolls: 3, maxPerWave: 1, homeRegion: BiomeType.Forest);

        /// <summary>Morcego-vampiro gigante: Caves night species, one big maroon bat (its own model).</summary>
        public static readonly NightSpecies MorcegoVampiro = new NightSpecies(
            SpeciesMorcegoVampiro, VampireBatEnemyId, "Giant Vampire Bat", "Morcego-vampiro gigante",
            NoTint, ModelEyes,
            scaleMultiplier: 1f, healthMultiplier: 1.8f, damageBonus: 0.3f, aggroRadiusMultiplier: 1.5f,
            experienceMultiplier: 1.8f, extraLootRolls: 1, maxPerWave: 2, homeRegion: BiomeType.Caves);

        /// <summary>Coruja-rasga-mortalha: Forest owl that swoops; also visits the Mountain at night (scaled to it).</summary>
        public static readonly NightSpecies RasgaMortalha = new NightSpecies(
            SpeciesRasgaMortalha, ShroudOwlEnemyId, "Shroud-tearer Owl", "Coruja-rasga-mortalha",
            NoTint, ModelEyes,
            scaleMultiplier: 1f, healthMultiplier: 0.8f, damageBonus: 0.2f, aggroRadiusMultiplier: 1.5f,
            experienceMultiplier: 1.5f, extraLootRolls: 1, maxPerWave: 2, homeRegion: BiomeType.Forest);

        /// <summary>
        /// Enxame de vaga-lumes-de-fogo: Desert swarm (dusk and night), also over the Forest at night. Three fireflies'
        /// worth of health in one enemy (3 × 0.35), weak bites (×0.4).
        /// </summary>
        public static readonly NightSpecies VagaLume = new NightSpecies(
            SpeciesVagaLume, FireflySwarmEnemyId, "Fire-firefly Swarm", "Enxame de vaga-lumes-de-fogo",
            NoTint, ModelEyes,
            scaleMultiplier: 1f, healthMultiplier: 1.05f, damageBonus: -0.6f, aggroRadiusMultiplier: 1.3f,
            experienceMultiplier: 1.3f, extraLootRolls: 1, maxPerWave: 1, homeRegion: BiomeType.Desert, comesAtDusk: true);

        /// <summary>Porcos-do-mato da Caipora (queixadas): Forest herd of 2–3 fast chargers.</summary>
        public static readonly NightSpecies Queixada = new NightSpecies(
            SpeciesQueixada, PeccaryEnemyId, "Caipora's Peccary", "Queixada da Caipora",
            NoTint, ModelEyes,
            scaleMultiplier: 1f, healthMultiplier: 1.3f, damageBonus: 0.3f, aggroRadiusMultiplier: 1.4f,
            experienceMultiplier: 1.3f, extraLootRolls: 0, maxPerWave: 3, homeRegion: BiomeType.Forest, packMin: 2, packMax: 3);

        /// <summary>Mãe-da-lua (urutau): rare Mountain ambusher on its branch; frail, notices you late, pays well.</summary>
        public static readonly NightSpecies MaeDaLua = new NightSpecies(
            SpeciesMaeDaLua, PotooEnemyId, "Moon-mother Potoo", "Mãe-da-lua",
            NoTint, ModelEyes,
            scaleMultiplier: 1f, healthMultiplier: 0.6f, damageBonus: -0.3f, aggroRadiusMultiplier: 1f,
            experienceMultiplier: 2f, extraLootRolls: 1, maxPerWave: 1, homeRegion: BiomeType.Mountain);

        public static readonly IReadOnlyList<NightSpecies> PlayableSpecies = new[] { Lobisomem, MorcegoVampiro, RasgaMortalha, VagaLume, Queixada, MaeDaLua };

        /// <summary>
        /// The home region's common enemy (Docs/DayNight.md §2.3): health, damage per hit and kill XP — the baseline every
        /// species definition copies and the yardstick a visiting species is scaled by.
        /// </summary>
        public static Vector3 RegionBaseline(BiomeType region)
        {
            switch (region)
            {
                case BiomeType.Desert: return new Vector3(110f, 11f, 18f);
                case BiomeType.Mountain: return new Vector3(140f, 13f, 28f);
                case BiomeType.Caves: return new Vector3(170f, 7f, 40f);
                default: return new Vector3(70f, 7f, 10f);
            }
        }

        /// <summary>The species whose own definition is <paramref name="enemyId"/>, or null.</summary>
        public static NightSpecies FindByEnemyId(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId))
            {
                return null;
            }

            foreach (NightSpecies species in PlayableSpecies)
            {
                if (species.BaseEnemyId == enemyId)
                {
                    return species;
                }
            }

            return null;
        }

        public static NightSpecies FindSpecies(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            foreach (NightSpecies species in PlayableSpecies)
            {
                if (species.Id == id)
                {
                    return species;
                }
            }

            return null;
        }

        /// <summary>Look + numbers for <paramref name="form"/> (a <paramref name="species"/> is required for <see cref="NightForm.Species"/>; without one it falls back to a variant).</summary>
        public static NightLook Resolve(NightForm form, NightSpecies species = null) => Resolve(form, species, null);

        /// <summary>
        /// Same, in <paramref name="region"/>: a species visiting a region other than its <see cref="NightSpecies.HomeRegion"/>
        /// (the owl in the Mountain, the fireflies over the Forest) is scaled by the ratio of the two regions' common
        /// baselines (<see cref="RegionBaseline"/>: health, damage, XP), so it is as tough there as it is at home.
        /// </summary>
        public static NightLook Resolve(NightForm form, NightSpecies species, BiomeType? region)
        {
            NightDangerProfile stats = NightDangerRules.VariantStats;
            if (form == NightForm.Species && species != null)
            {
                float health = species.HealthMultiplier;
                float damage = species.DamageBonus;
                float xp = species.ExperienceMultiplier;
                if (region.HasValue && region.Value != species.HomeRegion)
                {
                    Vector3 home = RegionBaseline(species.HomeRegion);
                    Vector3 there = RegionBaseline(region.Value);
                    health *= there.x / home.x;
                    damage = (1f + damage) * (there.y / home.y) - 1f;
                    xp *= there.z / home.z;
                }

                return new NightLook(NightForm.Species, species, ClampTint(species.Tint), species.EyeColor,
                    species.ScaleMultiplier, health, damage, species.AggroRadiusMultiplier,
                    stats.AggroHoldMultiplier, xp, species.ExtraLootRolls);
            }

            if (form == NightForm.Normal)
            {
                return new NightLook(NightForm.Normal, null, Color.white, Color.clear, 1f, 1f, 0f, 1f, 1f, 1f, 0);
            }

            return new NightLook(NightForm.Variant, null, ClampTint(VariantTint), VariantEyeColor, VariantScale,
                stats.HealthMultiplier, stats.DamageBonus, stats.AggroRadiusMultiplier, stats.AggroHoldMultiplier,
                stats.ExperienceMultiplier, stats.ExtraLootRolls);
        }

        /// <summary>Tint with every channel at least <see cref="MinTintChannel"/> and opaque.</summary>
        public static Color ClampTint(Color tint)
        {
            return new Color(Mathf.Clamp(tint.r, MinTintChannel, 1f), Mathf.Clamp(tint.g, MinTintChannel, 1f), Mathf.Clamp(tint.b, MinTintChannel, 1f), 1f);
        }

        /// <summary>Extra XP on top of the normal kill XP: base × (multiplier − 1), rounded, never negative.</summary>
        public static int BonusExperience(int baseExperience, NightLook look)
        {
            return Mathf.Max(0, Mathf.RoundToInt(Mathf.Max(0, baseExperience) * (look.ExperienceMultiplier - 1f)));
        }

        /// <summary>
        /// Name shown for the spawned enemy: the base name for a normal one, "{base} (noturno)" for a variant
        /// (<paramref name="variantFormat"/> = localized "{0} (noturno)"), the species name for a species.
        /// </summary>
        public static string DisplayName(string baseName, NightLook look, string variantFormat, string speciesName)
        {
            switch (look.Form)
            {
                case NightForm.Variant:
                    return string.Format(string.IsNullOrEmpty(variantFormat) ? "{0} (noturno)" : variantFormat, baseName ?? string.Empty);
                case NightForm.Species:
                    return string.IsNullOrEmpty(speciesName) ? baseName ?? string.Empty : speciesName;
                default:
                    return baseName ?? string.Empty;
            }
        }
    }
}
