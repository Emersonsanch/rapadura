using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.ClassEvolution
{
    /// <summary>What a graduation skill does (Docs/ClassEvolution.md §2).</summary>
    public enum GraduationSkillKind
    {
        /// <summary>Support buff for the caster and every ally around (<see cref="PartySupportRules"/>).</summary>
        GroupBuff = 0,
        /// <summary>Ordinary active skill: damage/stun/heal, optional debuff on whoever it hits.</summary>
        Active = 1
    }

    /// <summary>Low-poly accessory shape built on the existing model by <see cref="ClassEvolutionVisuals"/>.</summary>
    public enum AccessoryShape
    {
        Scarf = 0,
        Hat = 1,
        Sash = 2,
        Feather = 3,
        Ribbon = 4,
        Pouch = 5,
        Halo = 6,
        /// <summary>G4: a short cloak on the back (feathers, rings, roots, forge apron, moonlight).</summary>
        Mantle = 7,
        /// <summary>G5: a thin ring of light at the feet, the first step of the Column of Glory.</summary>
        ColumnRing = 8
    }

    /// <summary>
    /// One skill a graduation hands out, as plain data (server-friendly). Built into a real <see cref="SkillDefinition"/>
    /// by <see cref="GraduationSkillFactory"/>; parsed by <c>tools/balance/power_stack.py</c> (keep each <c>Skill(...)</c>
    /// call on one line, positional, in this order).
    /// </summary>
    public sealed class GraduationSkillSpec
    {
        public readonly string SkillId;
        public readonly CharacterId Character;
        public readonly int Tier;
        public readonly GraduationSkillKind Kind;
        public readonly float CooldownSeconds;
        public readonly float ManaCost;
        public readonly float CastTimeSeconds;
        public readonly float BaseDamage;
        public readonly float DamagePerLevel;
        public readonly float AreaRadius;
        public readonly float StunSeconds;
        public readonly float BaseHealing;
        /// <summary>GroupBuff: the stat buffed on the caster and allies. Active: the stat debuffed on targets (value 0 = none).</summary>
        public readonly StatType EffectStat;
        public readonly ModifierApplication EffectApplication;
        public readonly float EffectValue;
        public readonly float EffectDurationSeconds;

        public GraduationSkillSpec(string skillId, CharacterId character, int tier, GraduationSkillKind kind, float cooldownSeconds,
            float manaCost, float castTimeSeconds, float baseDamage, float damagePerLevel, float areaRadius, float stunSeconds,
            float baseHealing, StatType effectStat, ModifierApplication effectApplication, float effectValue, float effectDurationSeconds)
        {
            SkillId = skillId;
            Character = character;
            Tier = tier;
            Kind = kind;
            CooldownSeconds = cooldownSeconds;
            ManaCost = manaCost;
            CastTimeSeconds = castTimeSeconds;
            BaseDamage = baseDamage;
            DamagePerLevel = damagePerLevel;
            AreaRadius = areaRadius;
            StunSeconds = stunSeconds;
            BaseHealing = baseHealing;
            EffectStat = effectStat;
            EffectApplication = effectApplication;
            EffectValue = effectValue;
            EffectDurationSeconds = effectDurationSeconds;
        }

        public bool HasEffect => EffectValue != 0f && EffectDurationSeconds > 0f;

        /// <summary>Fraction of the time the effect is up when recast on cooldown (what the power model uses).</summary>
        public float EffectUptime => CooldownSeconds <= 0f ? 1f : Math.Min(1f, EffectDurationSeconds / CooldownSeconds);
    }

    /// <summary>
    /// One graduation ("graduação", like MU Online's 2nd/3rd class): tier 1..5 of one cousin, unlocked by a story mission
    /// handed out by a family member (tiers 1, 2 and 4) or Vó Edna (tier 3 in person, tier 5 over Vô Jorge's radio).
    /// Tiers 1-3 count kills in a campaign region; tiers 4-5 (Docs/Progressao500.md §3) count kills of creatures of at least
    /// <see cref="MissionMinCreatureLevel"/>, anywhere, and name the place of the story (<see cref="MissionMapId"/>).
    /// </summary>
    public sealed class GraduationSpec
    {
        public readonly CharacterId Character;
        public readonly int Tier;
        public readonly int MinLevel;
        /// <summary>Who gives and receives the mission: a <see cref="CharacterId"/> name in lower case, or "vo_edna".</summary>
        public readonly string MentorId;
        /// <summary>Campaign region the kills must happen in (tiers 1-3); null for the high tiers.</summary>
        public readonly BiomeType? MissionRegion;
        /// <summary>Map of Docs/Mapas100.md where the story happens (G4: the capital of the cousin's element; G5: the Sky Realms). 0 = none.</summary>
        public readonly int MissionMapId;
        /// <summary>High tiers: lowest creature level that counts (0 = the region rule).</summary>
        public readonly int MissionMinCreatureLevel;
        public readonly int MissionKills;
        public readonly string SkillId;
        public readonly string AccessoryId;
        public readonly AccessoryShape Accessory;
        /// <summary>Tint colour of this tier (linear 0..1, no UnityEngine type so the server can use it).</summary>
        public readonly float TintR, TintG, TintB;

        public GraduationSpec(CharacterId character, int tier, int minLevel, string mentorId, BiomeType? missionRegion, int missionKills,
            string skillId, string accessoryId, AccessoryShape accessory, float tintR, float tintG, float tintB, int missionMapId = 0,
            int missionMinCreatureLevel = 0)
        {
            Character = character;
            Tier = tier;
            MinLevel = minLevel;
            MentorId = mentorId;
            MissionRegion = missionRegion;
            MissionMapId = missionMapId;
            MissionMinCreatureLevel = missionMinCreatureLevel;
            MissionKills = missionKills;
            SkillId = skillId;
            AccessoryId = accessoryId;
            Accessory = accessory;
            TintR = tintR;
            TintG = tintG;
            TintB = tintB;
        }

        /// <summary>"grad_maithe_1".</summary>
        public string Id => ClassEvolutionCatalog.GraduationId(Character, Tier);
        public string KeyBase => "class.grad." + Id;
        public string TitleKey => KeyBase + ".title";
        public string MissionTitleKey => KeyBase + ".mission";
        public string OfferKey => KeyBase + ".offer";
        public string ProgressKey => KeyBase + ".progress";
        public string TurnInKey => KeyBase + ".turn_in";
        public string ReplyKey => KeyBase + ".reply";
        public string AccessoryKey => "class.accessory." + AccessoryId;

        /// <summary>True when kills count by creature level (G4/G5) instead of by campaign region.</summary>
        public bool CountsByCreatureLevel => MissionMinCreatureLevel > 0;

        /// <summary>True when a kill (region, creature level) counts for this mission.</summary>
        public bool KillCounts(BiomeType? region, int creatureLevel) =>
            CountsByCreatureLevel ? creatureLevel >= MissionMinCreatureLevel : region != null && MissionRegion == region.Value;

        /// <summary>Graduations whose <see cref="SkillId"/> is a passive (<see cref="GraduationUpgradeSpec"/>), not a skill.</summary>
        public bool IsPassive => ClassEvolutionCatalog.GetUpgrade(SkillId) != null;

        /// <summary>How strongly the tier tint is blended over the model (honest: a light wash, never a repaint).</summary>
        public float TintStrength => ClassEvolutionCatalog.TintStrengthForTier(Tier);
    }

    /// <summary>The class combo of one cousin: three of their skills in order, the last one their tier-2 skill.</summary>
    public sealed class ClassComboSpec
    {
        public readonly string ComboId;
        public readonly CharacterId Character;
        public readonly IReadOnlyList<string> SkillIds;
        public readonly float WindowSeconds;
        public readonly ComboBonusType BonusType;
        public readonly float BonusValue;
        public readonly float BonusDurationSeconds;

        public ClassComboSpec(string comboId, CharacterId character, string[] skillIds, float windowSeconds, ComboBonusType bonusType,
            float bonusValue, float bonusDurationSeconds)
        {
            ComboId = comboId;
            Character = character;
            SkillIds = skillIds;
            WindowSeconds = windowSeconds;
            BonusType = bonusType;
            BonusValue = bonusValue;
            BonusDurationSeconds = bonusDurationSeconds;
        }

        /// <summary>The graduation tier that unlocks the combo (it ends with the tier-2 skill).</summary>
        public int RequiredTier => ClassEvolutionCatalog.ComboTier;
    }

    /// <summary>
    /// The passive of G4 "do Elemento" (Docs/Progressao500.md §3.2): the cousin's tier-1 group buff reaches
    /// <see cref="GroupBuffRadius"/> instead of 10 m and the tier-2 skill gains a secondary control effect (a short stun or a
    /// slow), never more damage. Applied by <see cref="ClassEvolutionCatalog.EffectiveSkill"/>. Keep each <c>Upgrade(...)</c>
    /// on one line, positional (tools/balance/power_stack.py reads it).
    /// </summary>
    public sealed class GraduationUpgradeSpec
    {
        public readonly string UpgradeId;
        public readonly CharacterId Character;
        public readonly int Tier;
        public readonly float GroupBuffRadius;
        public readonly string TargetSkillId;
        public readonly float ExtraStunSeconds;
        /// <summary>Move-speed debuff added when the target skill has no effect of its own (negative, e.g. -0.3).</summary>
        public readonly float SlowValue;
        public readonly float SlowDurationSeconds;

        public GraduationUpgradeSpec(string upgradeId, CharacterId character, int tier, float groupBuffRadius, string targetSkillId,
            float extraStunSeconds, float slowValue, float slowDurationSeconds)
        {
            UpgradeId = upgradeId;
            Character = character;
            Tier = tier;
            GroupBuffRadius = groupBuffRadius;
            TargetSkillId = targetSkillId;
            ExtraStunSeconds = extraStunSeconds;
            SlowValue = slowValue;
            SlowDurationSeconds = slowDurationSeconds;
        }

        public string NameKey => "class.passive." + UpgradeId + ".name";
        public string DescriptionKey => "class.passive." + UpgradeId + ".description";
    }

    /// <summary>
    /// All graduations, their skills and the class combos (Docs/ClassEvolution.md). Pure data: no Unity objects, so the
    /// future server (FASE 12) validates the same table. Numbers are budgeted by <c>tools/balance/power_stack.py</c>.
    /// </summary>
    public static class ClassEvolutionCatalog
    {
        /// <summary>Highest graduation (G5 "da Coluna"). The campaign ones are 1-3; G4/G5 come with the 500 cap.</summary>
        public const int MaxTier = 5;
        /// <summary>The last graduation of the campaign (Vó Edna, level 25): the Master gate of the level-50 era.</summary>
        public const int CampaignMaxTier = 3;
        /// <summary>Hero level of G4 "do Elemento" and G5 "da Coluna" (Docs/Progressao500.md §3.1).</summary>
        public const int Tier4Level = 150;
        public const int Tier5Level = 350;
        /// <summary>Lowest creature level that counts for the G4 / G5 missions (any map; the story names the place).</summary>
        public const int Tier4CreatureLevel = 120;
        public const int Tier5CreatureLevel = 320;
        /// <summary>Tier whose passive widens the group buff and adds control to the tier-2 skill (<see cref="Upgrades"/>).</summary>
        public const int UpgradeTier = 4;
        public const int ComboTier = 2;
        public const string VoEdna = "vo_edna";

        public static string GraduationId(CharacterId character, int tier) =>
            "grad_" + character.ToString().ToLowerInvariant() + "_" + tier;

        public static float TintStrengthForTier(int tier) =>
            tier <= 0 ? 0f : tier == 1 ? 0.15f : tier == 2 ? 0.25f : tier == 3 ? 0.35f : tier == 4 ? 0.38f : 0.4f;

        private static GraduationSkillSpec Skill(string id, CharacterId hero, int tier, GraduationSkillKind kind, float cooldown, float mana,
            float cast, float damage, float perLevel, float area, float stun, float heal, StatType stat, ModifierApplication application,
            float value, float duration) =>
            new GraduationSkillSpec(id, hero, tier, kind, cooldown, mana, cast, damage, perLevel, area, stun, heal, stat, application, value, duration);

        private static GraduationSpec Grad(CharacterId hero, int tier, int minLevel, string mentor, BiomeType region, int kills, string skillId,
            string accessoryId, AccessoryShape shape, float r, float g, float b) =>
            new GraduationSpec(hero, tier, minLevel, mentor, region, kills, skillId, accessoryId, shape, r, g, b);

        /// <summary>G4/G5: the place is a map of Docs/Mapas100.md and kills count by creature level.</summary>
        private static GraduationSpec Grad(CharacterId hero, int tier, int minLevel, string mentor, int mapId, int minCreatureLevel, int kills,
            string skillId, string accessoryId, AccessoryShape shape, float r, float g, float b) =>
            new GraduationSpec(hero, tier, minLevel, mentor, null, kills, skillId, accessoryId, shape, r, g, b, mapId, minCreatureLevel);

        private static GraduationUpgradeSpec Upgrade(string id, CharacterId hero, int tier, float radius, string targetSkillId, float stun,
            float slow, float slowDuration) =>
            new GraduationUpgradeSpec(id, hero, tier, radius, targetSkillId, stun, slow, slowDuration);

        private static ClassComboSpec Combo(string id, CharacterId hero, string[] skills, float window, ComboBonusType type, float value, float duration) =>
            new ClassComboSpec(id, hero, skills, window, type, value, duration);

        /// <summary>
        /// Tier 1 = group support buff (level 8), tier 2 = class skill + combo (level 14), tier 3 = mastery skill (level 25),
        /// tier 5 = 2nd mastery skill (level 350, cooldown 30 s, no more per second than the tier 3 one). Tier 4 is a passive
        /// (<see cref="Upgrades"/>).
        /// </summary>
        public static readonly IReadOnlyList<GraduationSkillSpec> Skills = new[]
        {
            Skill("skill_grad_maithe_olhar_do_carcara", CharacterId.Maithe, 1, GraduationSkillKind.GroupBuff, 45f, 20f, 0.3f, 0f, 0f, 10f, 0f, 0f, StatType.CriticalChance, ModifierApplication.Flat, 0.02f, 15f),
            Skill("skill_grad_maithe_flecha_da_ventania", CharacterId.Maithe, 2, GraduationSkillKind.Active, 10f, 18f, 0.3f, 14f, 3f, 0f, 0f, 0f, StatType.MoveSpeed, ModifierApplication.PercentAdditive, -0.25f, 3f),
            Skill("skill_grad_maithe_redemoinho_do_sertao", CharacterId.Maithe, 3, GraduationSkillKind.Active, 22f, 35f, 0.5f, 18f, 4f, 4f, 0.8f, 0f, StatType.MoveSpeed, ModifierApplication.PercentAdditive, 0f, 0f),
            Skill("skill_grad_maithe_sopro_da_coluna", CharacterId.Maithe, 5, GraduationSkillKind.Active, 30f, 45f, 0.5f, 24f, 5.4f, 4f, 1f, 0f, StatType.MoveSpeed, ModifierApplication.PercentAdditive, 0f, 0f),

            Skill("skill_grad_maria_bencao_do_terreiro", CharacterId.Maria, 1, GraduationSkillKind.GroupBuff, 40f, 25f, 0.3f, 0f, 0f, 10f, 0f, 0f, StatType.HealthRegen, ModifierApplication.Flat, 1f, 12f),
            Skill("skill_grad_maria_candeia_que_acende", CharacterId.Maria, 2, GraduationSkillKind.Active, 12f, 22f, 0.3f, 8f, 2f, 3f, 0f, 40f, StatType.MoveSpeed, ModifierApplication.PercentAdditive, 0f, 0f),
            Skill("skill_grad_maria_sol_do_meio_dia", CharacterId.Maria, 3, GraduationSkillKind.Active, 22f, 35f, 0.5f, 14f, 3f, 5f, 0.6f, 30f, StatType.MoveSpeed, ModifierApplication.PercentAdditive, 0f, 0f),
            Skill("skill_grad_maria_primeira_luz", CharacterId.Maria, 5, GraduationSkillKind.Active, 30f, 45f, 0.5f, 19f, 4f, 5f, 0.8f, 40f, StatType.MoveSpeed, ModifierApplication.PercentAdditive, 0f, 0f),

            Skill("skill_grad_joaquim_raiz_que_segura", CharacterId.Joaquim, 1, GraduationSkillKind.GroupBuff, 40f, 20f, 0.3f, 0f, 0f, 10f, 0f, 0f, StatType.Defense, ModifierApplication.PercentAdditive, 0.04f, 12f),
            Skill("skill_grad_joaquim_pancada_de_mandacaru", CharacterId.Joaquim, 2, GraduationSkillKind.Active, 12f, 18f, 0.4f, 10f, 2f, 2f, 0.8f, 0f, StatType.MoveSpeed, ModifierApplication.PercentAdditive, 0f, 0f),
            Skill("skill_grad_joaquim_cerca_de_aroeira", CharacterId.Joaquim, 3, GraduationSkillKind.Active, 22f, 30f, 0.5f, 14f, 3f, 5f, 1f, 0f, StatType.MoveSpeed, ModifierApplication.PercentAdditive, -0.3f, 4f),
            Skill("skill_grad_joaquim_rio_de_luz", CharacterId.Joaquim, 5, GraduationSkillKind.Active, 30f, 40f, 0.5f, 19f, 4f, 5f, 1.3f, 0f, StatType.MoveSpeed, ModifierApplication.PercentAdditive, -0.3f, 5f),

            Skill("skill_grad_icaro_engenhoca_de_reforco", CharacterId.Icaro, 1, GraduationSkillKind.GroupBuff, 50f, 25f, 0.3f, 0f, 0f, 10f, 0f, 0f, StatType.CooldownReduction, ModifierApplication.Flat, 0.04f, 15f),
            Skill("skill_grad_icaro_bobina_de_cata_vento", CharacterId.Icaro, 2, GraduationSkillKind.Active, 12f, 22f, 0.4f, 12f, 3f, 2f, 0.5f, 0f, StatType.MoveSpeed, ModifierApplication.PercentAdditive, 0f, 0f),
            Skill("skill_grad_icaro_radio_turbinado", CharacterId.Icaro, 3, GraduationSkillKind.Active, 22f, 35f, 0.5f, 18f, 4f, 5f, 0f, 0f, StatType.MoveSpeed, ModifierApplication.PercentAdditive, -0.3f, 4f),
            Skill("skill_grad_icaro_roda_das_doze_jarras", CharacterId.Icaro, 5, GraduationSkillKind.Active, 30f, 45f, 0.5f, 24f, 5.4f, 5f, 0f, 0f, StatType.MoveSpeed, ModifierApplication.PercentAdditive, -0.3f, 5f),

            Skill("skill_grad_lavine_lua_guardia", CharacterId.Lavine, 1, GraduationSkillKind.GroupBuff, 45f, 20f, 0.3f, 0f, 0f, 10f, 0f, 0f, StatType.ManaRegen, ModifierApplication.PercentAdditive, 0.1f, 15f),
            Skill("skill_grad_lavine_fita_vermelha", CharacterId.Lavine, 2, GraduationSkillKind.Active, 10f, 18f, 0.4f, 14f, 3f, 0f, 0f, 0f, StatType.MoveSpeed, ModifierApplication.PercentAdditive, -0.5f, 2f),
            Skill("skill_grad_lavine_sombra_macia", CharacterId.Lavine, 3, GraduationSkillKind.Active, 22f, 35f, 0.5f, 18f, 4f, 4f, 0.6f, 0f, StatType.MoveSpeed, ModifierApplication.PercentAdditive, 0f, 0f),
            Skill("skill_grad_lavine_noite_que_se_cala", CharacterId.Lavine, 5, GraduationSkillKind.Active, 30f, 45f, 0.5f, 24f, 5.4f, 4f, 0.8f, 0f, StatType.MoveSpeed, ModifierApplication.PercentAdditive, 0f, 0f),
        };

        public static readonly IReadOnlyList<GraduationSpec> Graduations = new[]
        {
            Grad(CharacterId.Maithe, 1, 8, "icaro", BiomeType.Desert, 15, "skill_grad_maithe_olhar_do_carcara", "pena_de_carcara", AccessoryShape.Feather, 0.55f, 0.40f, 0.25f),
            Grad(CharacterId.Maithe, 2, 14, "maria", BiomeType.Caves, 20, "skill_grad_maithe_flecha_da_ventania", "lenco_da_ventania", AccessoryShape.Scarf, 0.35f, 0.65f, 0.55f),
            Grad(CharacterId.Maithe, 3, 25, VoEdna, BiomeType.Mountain, 30, "skill_grad_maithe_redemoinho_do_sertao", "chapeu_de_couro", AccessoryShape.Hat, 0.60f, 0.42f, 0.22f),
            Grad(CharacterId.Maithe, 4, Tier4Level, "lavine", 54, Tier4CreatureLevel, 40, "passive_grad_maithe_vento_mais_longe", "manto_de_penas_de_farnkesh", AccessoryShape.Mantle, 0.70f, 0.85f, 0.95f),
            Grad(CharacterId.Maithe, 5, Tier5Level, VoEdna, 91, Tier5CreatureLevel, 50, "skill_grad_maithe_sopro_da_coluna", "anel_do_vento_da_coluna", AccessoryShape.ColumnRing, 0.85f, 0.95f, 1.00f),

            Grad(CharacterId.Maria, 1, 8, "joaquim", BiomeType.Desert, 15, "skill_grad_maria_bencao_do_terreiro", "fita_de_romaria", AccessoryShape.Ribbon, 0.95f, 0.80f, 0.35f),
            Grad(CharacterId.Maria, 2, 14, "lavine", BiomeType.Caves, 20, "skill_grad_maria_candeia_que_acende", "xale_da_aurora", AccessoryShape.Scarf, 1.00f, 0.70f, 0.55f),
            Grad(CharacterId.Maria, 3, 25, VoEdna, BiomeType.Mountain, 30, "skill_grad_maria_sol_do_meio_dia", "resplendor_de_candeia", AccessoryShape.Halo, 1.00f, 0.90f, 0.50f),
            Grad(CharacterId.Maria, 4, Tier4Level, "icaro", 41, Tier4CreatureLevel, 40, "passive_grad_maria_lampada_dos_nove_aneis", "manto_dos_nove_aneis", AccessoryShape.Mantle, 1.00f, 0.85f, 0.45f),
            Grad(CharacterId.Maria, 5, Tier5Level, VoEdna, 91, Tier5CreatureLevel, 50, "skill_grad_maria_primeira_luz", "anel_da_aurora_da_coluna", AccessoryShape.ColumnRing, 1.00f, 0.95f, 0.75f),

            Grad(CharacterId.Joaquim, 1, 8, "maria", BiomeType.Desert, 15, "skill_grad_joaquim_raiz_que_segura", "faixa_de_raiz", AccessoryShape.Sash, 0.45f, 0.32f, 0.20f),
            Grad(CharacterId.Joaquim, 2, 14, "lavine", BiomeType.Caves, 20, "skill_grad_joaquim_pancada_de_mandacaru", "bornal_de_couro", AccessoryShape.Pouch, 0.40f, 0.55f, 0.30f),
            Grad(CharacterId.Joaquim, 3, 25, VoEdna, BiomeType.Mountain, 30, "skill_grad_joaquim_cerca_de_aroeira", "chapeu_de_aroeira", AccessoryShape.Hat, 0.50f, 0.36f, 0.22f),
            Grad(CharacterId.Joaquim, 4, Tier4Level, "maithe", 31, Tier4CreatureLevel, 40, "passive_grad_joaquim_raizes_de_jhilpur", "manto_de_raizes_de_jhilpur", AccessoryShape.Mantle, 0.30f, 0.55f, 0.50f),
            Grad(CharacterId.Joaquim, 5, Tier5Level, VoEdna, 91, Tier5CreatureLevel, 50, "skill_grad_joaquim_rio_de_luz", "anel_do_rio_da_coluna", AccessoryShape.ColumnRing, 0.55f, 0.85f, 1.00f),

            Grad(CharacterId.Icaro, 1, 8, "maithe", BiomeType.Desert, 15, "skill_grad_icaro_engenhoca_de_reforco", "oculos_de_solda", AccessoryShape.Halo, 0.75f, 0.60f, 0.30f),
            Grad(CharacterId.Icaro, 2, 14, "joaquim", BiomeType.Caves, 20, "skill_grad_icaro_bobina_de_cata_vento", "bornal_de_ferramentas", AccessoryShape.Pouch, 0.45f, 0.55f, 0.75f),
            Grad(CharacterId.Icaro, 3, 25, VoEdna, BiomeType.Mountain, 30, "skill_grad_icaro_radio_turbinado", "antena_do_radio", AccessoryShape.Feather, 0.85f, 0.65f, 0.30f),
            Grad(CharacterId.Icaro, 4, Tier4Level, "maria", 65, Tier4CreatureLevel, 40, "passive_grad_icaro_fole_de_hinokura", "avental_de_forja_de_hinokura", AccessoryShape.Mantle, 0.85f, 0.40f, 0.20f),
            Grad(CharacterId.Icaro, 5, Tier5Level, VoEdna, 91, Tier5CreatureLevel, 50, "skill_grad_icaro_roda_das_doze_jarras", "anel_do_engenho_da_coluna", AccessoryShape.ColumnRing, 1.00f, 0.75f, 0.40f),

            Grad(CharacterId.Lavine, 1, 8, "maithe", BiomeType.Desert, 15, "skill_grad_lavine_lua_guardia", "laco_rubro", AccessoryShape.Ribbon, 0.80f, 0.20f, 0.25f),
            Grad(CharacterId.Lavine, 2, 14, "maria", BiomeType.Caves, 20, "skill_grad_lavine_fita_vermelha", "lenco_de_sombra", AccessoryShape.Scarf, 0.45f, 0.30f, 0.60f),
            Grad(CharacterId.Lavine, 3, 25, VoEdna, BiomeType.Mountain, 30, "skill_grad_lavine_sombra_macia", "coroa_de_lua", AccessoryShape.Halo, 0.70f, 0.75f, 0.95f),
            Grad(CharacterId.Lavine, 4, Tier4Level, "joaquim", 59, Tier4CreatureLevel, 40, "passive_grad_lavine_luar_do_porto", "manto_da_lua_branca", AccessoryShape.Mantle, 0.85f, 0.88f, 1.00f),
            Grad(CharacterId.Lavine, 5, Tier5Level, VoEdna, 91, Tier5CreatureLevel, 50, "skill_grad_lavine_noite_que_se_cala", "anel_do_silencio_da_coluna", AccessoryShape.ColumnRing, 0.75f, 0.70f, 0.95f),
        };

        public static readonly IReadOnlyList<ClassComboSpec> Combos = new[]
        {
            Combo("combo_class_maithe", CharacterId.Maithe, new[] { "skill_maithe_flecha_precisa", "skill_maithe_chuva_de_flechas", "skill_grad_maithe_flecha_da_ventania" }, 3f, ComboBonusType.DamageMultiplier, 0.04f, 5f),
            Combo("combo_class_maria", CharacterId.Maria, new[] { "skill_maria_purificacao", "skill_maria_cura_divina", "skill_grad_maria_candeia_que_acende" }, 3f, ComboBonusType.CostReduction, 0.1f, 6f),
            Combo("combo_class_joaquim", CharacterId.Joaquim, new[] { "skill_joaquim_provocacao_heroica", "skill_joaquim_terremoto", "skill_grad_joaquim_pancada_de_mandacaru" }, 3f, ComboBonusType.CostReduction, 0.1f, 6f),
            Combo("combo_class_icaro", CharacterId.Icaro, new[] { "skill_icaro_torreta_automatica", "skill_icaro_granada_eletrica", "skill_grad_icaro_bobina_de_cata_vento" }, 3f, ComboBonusType.DamageMultiplier, 0.03f, 6f),
            Combo("combo_class_lavine", CharacterId.Lavine, new[] { "skill_lavine_orbe_sombria", "skill_lavine_correntes_das_trevas", "skill_grad_lavine_fita_vermelha" }, 3f, ComboBonusType.DamageMultiplier, 0.04f, 5f),
        };

        /// <summary>G4 passives: tier-1 group buff 10 m -> 14 m and a control effect on the tier-2 skill (no damage).</summary>
        public static readonly IReadOnlyList<GraduationUpgradeSpec> Upgrades = new[]
        {
            Upgrade("passive_grad_maithe_vento_mais_longe", CharacterId.Maithe, 4, 14f, "skill_grad_maithe_flecha_da_ventania", 0.4f, 0f, 0f),
            Upgrade("passive_grad_maria_lampada_dos_nove_aneis", CharacterId.Maria, 4, 14f, "skill_grad_maria_candeia_que_acende", 0f, -0.3f, 3f),
            Upgrade("passive_grad_joaquim_raizes_de_jhilpur", CharacterId.Joaquim, 4, 14f, "skill_grad_joaquim_pancada_de_mandacaru", 0f, -0.3f, 3f),
            Upgrade("passive_grad_icaro_fole_de_hinokura", CharacterId.Icaro, 4, 14f, "skill_grad_icaro_bobina_de_cata_vento", 0f, -0.25f, 3f),
            Upgrade("passive_grad_lavine_luar_do_porto", CharacterId.Lavine, 4, 14f, "skill_grad_lavine_fita_vermelha", 0.4f, 0f, 0f),
        };

        /// <summary>True when the tier has a real graduation (mission, skill or passive, look): 1..<see cref="MaxTier"/>.</summary>
        public static bool IsTierImplemented(int tier) => tier >= 1 && tier <= MaxTier;

        /// <summary>Hero level a graduation tier opens at (8 / 14 / 25 / 150 / 350), or -1 for an unknown tier.</summary>
        public static int MilestoneLevel(int tier)
        {
            int level = -1;
            foreach (GraduationSpec spec in Graduations)
            {
                if (spec.Tier == tier && (level < 0 || spec.MinLevel < level))
                {
                    level = spec.MinLevel;
                }
            }

            return level;
        }

        public static GraduationUpgradeSpec GetUpgrade(string upgradeId)
        {
            foreach (GraduationUpgradeSpec spec in Upgrades)
            {
                if (spec.UpgradeId == upgradeId)
                {
                    return spec;
                }
            }

            return null;
        }

        /// <summary>The G4 passive of a cousin (null when none).</summary>
        public static GraduationUpgradeSpec GetUpgrade(CharacterId character)
        {
            foreach (GraduationUpgradeSpec spec in Upgrades)
            {
                if (spec.Character == character)
                {
                    return spec;
                }
            }

            return null;
        }

        /// <summary>
        /// The skill as a cousin at <paramref name="ownerTier"/> casts it: from G4 on the tier-1 group buff reaches the
        /// passive's radius and the tier-2 skill gains its stun or slow (never more damage). Same instance when nothing changes. Pure.
        /// </summary>
        public static GraduationSkillSpec EffectiveSkill(GraduationSkillSpec spec, int ownerTier)
        {
            GraduationUpgradeSpec up = spec != null ? GetUpgrade(spec.Character) : null;
            if (up == null || ownerTier < up.Tier)
            {
                return spec;
            }

            if (spec.Kind == GraduationSkillKind.GroupBuff && spec.Tier == 1)
            {
                return up.GroupBuffRadius <= spec.AreaRadius ? spec : new GraduationSkillSpec(spec.SkillId, spec.Character, spec.Tier, spec.Kind,
                    spec.CooldownSeconds, spec.ManaCost, spec.CastTimeSeconds, spec.BaseDamage, spec.DamagePerLevel, up.GroupBuffRadius,
                    spec.StunSeconds, spec.BaseHealing, spec.EffectStat, spec.EffectApplication, spec.EffectValue, spec.EffectDurationSeconds);
            }

            if (spec.SkillId != up.TargetSkillId)
            {
                return spec;
            }

            bool addSlow = !spec.HasEffect && up.SlowValue < 0f && up.SlowDurationSeconds > 0f;
            return new GraduationSkillSpec(spec.SkillId, spec.Character, spec.Tier, spec.Kind, spec.CooldownSeconds, spec.ManaCost,
                spec.CastTimeSeconds, spec.BaseDamage, spec.DamagePerLevel, spec.AreaRadius, spec.StunSeconds + Math.Max(0f, up.ExtraStunSeconds),
                spec.BaseHealing, addSlow ? StatType.MoveSpeed : spec.EffectStat,
                addSlow ? ModifierApplication.PercentAdditive : spec.EffectApplication, addSlow ? up.SlowValue : spec.EffectValue,
                addSlow ? up.SlowDurationSeconds : spec.EffectDurationSeconds);
        }

        public static GraduationSpec GetGraduation(CharacterId character, int tier)
        {
            foreach (GraduationSpec spec in Graduations)
            {
                if (spec.Character == character && spec.Tier == tier)
                {
                    return spec;
                }
            }

            return null;
        }

        public static GraduationSkillSpec GetSkill(string skillId)
        {
            foreach (GraduationSkillSpec spec in Skills)
            {
                if (spec.SkillId == skillId)
                {
                    return spec;
                }
            }

            return null;
        }

        public static ClassComboSpec GetCombo(CharacterId character)
        {
            foreach (ClassComboSpec spec in Combos)
            {
                if (spec.Character == character)
                {
                    return spec;
                }
            }

            return null;
        }

        /// <summary>Graduation skills a cousin owns at <paramref name="tier"/> (every tier up to and including it).</summary>
        public static IEnumerable<GraduationSkillSpec> SkillsUpTo(CharacterId character, int tier)
        {
            foreach (GraduationSkillSpec spec in Skills)
            {
                if (spec.Character == character && spec.Tier <= tier)
                {
                    yield return spec;
                }
            }
        }

        /// <summary>The base class title (tier 0): the existing <c>character.&lt;id&gt;.class</c> key ("Caçadora", "Guardião"...).</summary>
        public static string BaseTitleKey(CharacterId character) => Rapadura.Gameplay.Content.ContentText.CharacterClassKey(character);

        /// <summary>Title key a cousin wears at <paramref name="tier"/> (0 = base class).</summary>
        public static string TitleKey(CharacterId character, int tier)
        {
            GraduationSpec spec = tier > 0 ? GetGraduation(character, Math.Min(tier, MaxTier)) : null;
            return spec != null ? spec.TitleKey : BaseTitleKey(character);
        }

        /// <summary>"class.mentor.vo_edna".</summary>
        public static string MentorNameKey(string mentorId) => "class.mentor." + mentorId;
    }
}
