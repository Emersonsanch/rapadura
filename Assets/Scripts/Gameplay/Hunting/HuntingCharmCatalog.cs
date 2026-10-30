using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.Gameplay.Hunting
{
    /// <summary>What an "Amuleto do Caderno" does (values on <see cref="HuntingCharmSpec.Value"/>).</summary>
    public enum HuntingCharmEffect
    {
        /// <summary>Value less damage taken from the creatures of <see cref="HuntingCharmSpec.Archetype"/>.</summary>
        TypeProtection = 0,
        /// <summary>Value chance (0..1) to dodge a common creature's hit.</summary>
        Dodge = 1,
        /// <summary>Heals Value × max health on each common creature defeated.</summary>
        HealOnKill = 2,
        /// <summary>Stuns on common creatures last 1 + Value times as long.</summary>
        LongerStun = 3,
    }

    /// <summary>One charm: id (<c>hunting.charm.&lt;id&gt;.*</c> text keys), effect, value and cost in "Pontos do Caderno".</summary>
    public sealed class HuntingCharmSpec
    {
        public readonly string Id;
        public readonly HuntingCharmEffect Effect;
        public readonly EnemyMotionArchetype Archetype;
        public readonly float Value;
        public readonly int Cost;

        public HuntingCharmSpec(string id, HuntingCharmEffect effect, float value, int cost, EnemyMotionArchetype archetype = EnemyMotionArchetype.Auto)
        {
            Id = id;
            Effect = effect;
            Value = value;
            Cost = cost;
            Archetype = archetype;
        }

        public string NameKey => "hunting.charm." + Id + ".name";
        public string EffectKey => "hunting.charm." + Id + ".effect";
    }

    /// <summary>
    /// "Amuletos do Caderno": small charms bought with "Pontos do Caderno" (<see cref="HuntingRules.MilestonePointsFor"/>).
    /// A charm is bought once and kept; at most <see cref="MaxEquipped"/> are worn at a time ("o cordão da Vó só
    /// aguenta dois"). Every effect is against common creatures only, never bosses, and none raises the hero's damage:
    /// they protect (less damage taken, dodge, heal on a win, longer stuns), so the times-to-kill of the power budget
    /// (§14) don't move. <c>tools/balance/power_stack.py</c> (§ "Caça") reads this file and fails if a damage charm
    /// ever appears: the commons of the Cavernas sit on a breakpoint where even +2% damage breaks the budget.
    /// </summary>
    public static class HuntingCharmCatalog
    {
        public const int MaxEquipped = 2;

        /// <summary>Protection of each type charm (8% less damage from one kind of creature).</summary>
        public const float TypeProtectionValue = 0.08f;
        public const float DodgeChance = 0.03f;
        public const float HealOnKillFraction = 0.02f;
        public const float LongerStunBonus = 0.1f;

        public const int TypeCharmCost = 4;
        public const int DodgeCost = 6;
        public const int HealCost = 5;
        public const int StunCost = 5;

        public const string DenteDeOnca = "dente_de_onca";
        public const string CascaDeBesouro = "casca_de_besouro";
        public const string PenaDeCarcara = "pena_de_carcara";
        public const string PedraDeCorisco = "pedra_de_corisco";
        public const string FigaDeGuine = "figa_de_guine";
        public const string RaminhoDeArruda = "raminho_de_arruda";
        public const string NoDeVaqueiro = "no_de_vaqueiro";

        public static readonly IReadOnlyList<HuntingCharmSpec> All = new[]
        {
            new HuntingCharmSpec(DenteDeOnca, HuntingCharmEffect.TypeProtection, TypeProtectionValue, TypeCharmCost, EnemyMotionArchetype.Quadruped),
            new HuntingCharmSpec(CascaDeBesouro, HuntingCharmEffect.TypeProtection, TypeProtectionValue, TypeCharmCost, EnemyMotionArchetype.Arthropod),
            new HuntingCharmSpec(PenaDeCarcara, HuntingCharmEffect.TypeProtection, TypeProtectionValue, TypeCharmCost, EnemyMotionArchetype.Flyer),
            new HuntingCharmSpec(PedraDeCorisco, HuntingCharmEffect.TypeProtection, TypeProtectionValue, TypeCharmCost, EnemyMotionArchetype.Heavy),
            new HuntingCharmSpec(FigaDeGuine, HuntingCharmEffect.Dodge, DodgeChance, DodgeCost),
            new HuntingCharmSpec(RaminhoDeArruda, HuntingCharmEffect.HealOnKill, HealOnKillFraction, HealCost),
            new HuntingCharmSpec(NoDeVaqueiro, HuntingCharmEffect.LongerStun, LongerStunBonus, StunCost),
        };

        public static HuntingCharmSpec Get(string id) => id == null ? null : All.FirstOrDefault(c => c.Id == id);

        public static bool Contains(string id) => Get(id) != null;

        /// <summary>Total cost of the given (unlocked) charms; unknown ids cost nothing.</summary>
        public static int TotalCost(IEnumerable<string> ids) =>
            (ids ?? Enumerable.Empty<string>()).Distinct().Select(Get).Where(c => c != null).Sum(c => c.Cost);

        public static bool CanUnlock(string id, IEnumerable<string> unlocked, int availablePoints)
        {
            HuntingCharmSpec spec = Get(id);
            return spec != null && !(unlocked ?? Enumerable.Empty<string>()).Contains(id) && availablePoints >= spec.Cost;
        }

        public static bool CanEquip(string id, IEnumerable<string> unlocked, IReadOnlyCollection<string> equipped)
        {
            return Contains(id)
                && (unlocked ?? Enumerable.Empty<string>()).Contains(id)
                && (equipped == null || (!equipped.Contains(id) && equipped.Count < MaxEquipped));
        }

        /// <summary>Equipped specs (known, distinct, at most <see cref="MaxEquipped"/>, in the given order).</summary>
        public static IEnumerable<HuntingCharmSpec> Equipped(IEnumerable<string> equipped) =>
            (equipped ?? Enumerable.Empty<string>()).Distinct().Select(Get).Where(c => c != null).Take(MaxEquipped);

        /// <summary>Σ type-charm protection against <paramref name="archetype"/> (0 for an unresolved archetype).</summary>
        public static float TypeProtectionBonus(IEnumerable<string> equipped, EnemyMotionArchetype archetype)
        {
            if (archetype == EnemyMotionArchetype.Auto)
            {
                return 0f;
            }

            return Equipped(equipped).Where(c => c.Effect == HuntingCharmEffect.TypeProtection && c.Archetype == archetype).Sum(c => c.Value);
        }

        public static float Total(IEnumerable<string> equipped, HuntingCharmEffect effect) =>
            Equipped(equipped).Where(c => c.Effect == effect).Sum(c => c.Value);

        /// <summary>Dodge chance against a hit from a creature (bosses: 0).</summary>
        public static float DodgeChanceAgainst(IEnumerable<string> equipped, bool attackerIsBoss) =>
            attackerIsBoss ? 0f : Math.Min(1f, Total(equipped, HuntingCharmEffect.Dodge));

        /// <summary>A dodge happens when <paramref name="roll01"/> is below the chance (a chance of 0 never dodges).</summary>
        public static bool RollDodge(float chance, double roll01) => chance > 0f && roll01 < chance;

        /// <summary>Health healed on defeating a creature (bosses: 0).</summary>
        public static float HealOnKill(IEnumerable<string> equipped, bool victimIsBoss, float maxHealth) =>
            victimIsBoss || maxHealth <= 0f ? 0f : maxHealth * Total(equipped, HuntingCharmEffect.HealOnKill);

        /// <summary>Stun duration multiplier on a creature (bosses: 1, they keep their own anti-lock rule).</summary>
        public static float StunMultiplier(IEnumerable<string> equipped, bool targetIsBoss) =>
            targetIsBoss ? 1f : 1f + Total(equipped, HuntingCharmEffect.LongerStun);
    }
}
