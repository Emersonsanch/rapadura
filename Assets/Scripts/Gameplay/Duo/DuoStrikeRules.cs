using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Duo
{
    /// <summary>
    /// "Golpe em dupla": pure tuning and rules (no Unity objects, EditMode-tested in <c>DuoStrikeTests</c>; the
    /// numbers are explained in Docs/Balance/README.md, "Golpe em dupla").
    /// <code>
    /// carga  += min(0,25, dano causado / vida máx. do alvo × (chefe ? 2 : 0,5))   // 50% de um chefe, ou 2 comuns inteiros
    /// carga  += min(0,25, dano sofrido / vida máx. do herói × 1,25)                 // perder 80% da vida enche
    /// dano    = OutgoingDamage.Resolve(60 × combo.DamageMultiplier, damage_mult(nível), buffs do herói, crítico)
    /// stun    = combo.StunSeconds × (chefe ? 0,5 : 1)                               // StunRules, via BuffController.ApplyStun
    /// empurrão= combo.KnockbackForce × (chefe ? 0,35 : 1)
    /// </code>
    /// One use per encounter (<see cref="MaxUsesPerEncounter"/>), armed <see cref="ArmDelaySeconds"/> after the
    /// encounter starts, with <see cref="CooldownSeconds"/> between uses for modes that allow more than one.
    /// </summary>
    public static class DuoStrikeRules
    {
        /// <summary>Full meter.</summary>
        public const float MaxCharge = 1f;

        /// <summary>Base damage before the combo multiplier: ~1,5–2 active skills, ~60% of an Ultimate (skills.csv lv1: 24–40 / 100–150).</summary>
        public const float BaseDamage = 60f;

        /// <summary>Charge per "enemy max health" dealt to a common enemy: dealing 200% of one common's health fills the meter.</summary>
        public const float CommonDealtFactor = 0.5f;

        /// <summary>Charge per "boss max health" dealt to a boss: half the boss's health fills the meter.</summary>
        public const float BossDealtFactor = 2f;

        /// <summary>Charge per "hero max health" taken: losing 80% of the hero's health fills the meter.</summary>
        public const float TakenFactor = 1.25f;

        /// <summary>No single hit (a big crit, a boss slam) gives more than a quarter of the meter.</summary>
        public const float MaxChargePerHit = 0.25f;

        /// <summary>The duo strike is a once-per-fight moment, not a rotation skill.</summary>
        public const int MaxUsesPerEncounter = 1;

        /// <summary>Seconds between two uses when <see cref="MaxUsesPerEncounter"/> &gt; 1 (NG+/online party variants).</summary>
        public const float CooldownSeconds = 45f;

        /// <summary>Seconds after the encounter starts before it can fire (covers the "Prepare-se!" grace: enemies hold aggro for ~2 s).</summary>
        public const float ArmDelaySeconds = 2.5f;

        /// <summary>Bosses barely budge (same anti-lock idea as <see cref="StunRules.BossStunDurationMultiplier"/>).</summary>
        public const float BossKnockbackMultiplier = 0.35f;

        /// <summary>Seconds from the call until the combined move lands (the partner appears, then strikes).</summary>
        public const float ImpactDelaySeconds = 0.45f;

        /// <summary>Total seconds the partner stays on screen.</summary>
        public const float StrikeDurationSeconds = 1.5f;

        /// <summary>Range around the hero in which the strike looks for the enemy to centre on.</summary>
        public const float TargetSearchRange = 12f;

        // ------------------------------------------------------------------
        // Charge
        // ------------------------------------------------------------------

        /// <summary>Charge earned by dealing <paramref name="damage"/> to an enemy with <paramref name="targetMaxHealth"/>.</summary>
        public static float ChargeFromDealt(float damage, float targetMaxHealth, bool isBoss)
        {
            if (damage <= 0f || targetMaxHealth <= 0f)
            {
                return 0f;
            }

            float factor = isBoss ? BossDealtFactor : CommonDealtFactor;
            return Mathf.Min(MaxChargePerHit, damage / targetMaxHealth * factor);
        }

        /// <summary>Charge earned by the hero taking <paramref name="damage"/> with <paramref name="heroMaxHealth"/>.</summary>
        public static float ChargeFromTaken(float damage, float heroMaxHealth)
        {
            if (damage <= 0f || heroMaxHealth <= 0f)
            {
                return 0f;
            }

            return Mathf.Min(MaxChargePerHit, damage / heroMaxHealth * TakenFactor);
        }

        /// <summary><paramref name="current"/> + <paramref name="gain"/>, clamped to [0, <see cref="MaxCharge"/>].</summary>
        public static float AddCharge(float current, float gain)
        {
            float charge = Mathf.Clamp(current + Mathf.Max(0f, gain), 0f, MaxCharge);
            // Ten hits of 0.1 sum to 0.99999994 in float: snap so "full" means full.
            return charge >= MaxCharge - ChargeEpsilon ? MaxCharge : charge;
        }

        private const float ChargeEpsilon = 1e-4f;

        // ------------------------------------------------------------------
        // Power
        // ------------------------------------------------------------------

        /// <summary>Damage before level/buffs/crit: <see cref="BaseDamage"/> × the combo multiplier (0 for a pure support combo).</summary>
        public static float BaseDamageFor(DuoComboSpec combo)
        {
            return combo == null ? 0f : BaseDamage * Mathf.Max(0f, combo.DamageMultiplier);
        }

        /// <summary>
        /// Same attacker pipeline as skills (<see cref="OutgoingDamage.Resolve"/>): player-level damage_mult, the hero's
        /// AttackDamage buffs (Poder Arcano, meals, combos) and one crit roll for the whole strike.
        /// </summary>
        public static float ResolveDamage(DuoComboSpec combo, float levelMultiplier, BuffController heroBuffs, PlayerCombatBalance balance,
            Func<float> random, out bool isCritical)
        {
            return OutgoingDamage.Resolve(BaseDamageFor(combo), levelMultiplier, heroBuffs, balance, random, out isCritical);
        }

        /// <summary>Healing for the hero: the combo's fraction of max health.</summary>
        public static float HealingFor(DuoComboSpec combo, float heroMaxHealth)
        {
            return combo == null || heroMaxHealth <= 0f ? 0f : Mathf.Max(0f, combo.HealFraction) * heroMaxHealth;
        }

        /// <summary>Knockback impulse for one target (bosses × <see cref="BossKnockbackMultiplier"/>).</summary>
        public static float KnockbackFor(float force, bool isBoss)
        {
            return force <= 0f ? 0f : force * (isBoss ? BossKnockbackMultiplier : 1f);
        }

        /// <summary>Expected stun on a target (the real call goes through <see cref="BuffController.ApplyStun"/>, which applies the same scaling).</summary>
        public static float StunFor(DuoComboSpec combo, bool isBoss)
        {
            return combo == null ? 0f : StunRules.ScaleDuration(combo.StunSeconds, StunRules.MultiplierFor(isBoss));
        }

        // ------------------------------------------------------------------
        // Pairs
        // ------------------------------------------------------------------

        /// <summary>Birth order (0 = eldest): Maithe &gt; Maria &gt; Joaquim &gt; Ícaro &gt; Lavine.</summary>
        public static int BirthOrder(CharacterId id)
        {
            switch (id)
            {
                case CharacterId.Maithe: return 0;
                case CharacterId.Maria: return 1;
                case CharacterId.Joaquim: return 2;
                case CharacterId.Icaro: return 3;
                default: return 4;
            }
        }

        /// <summary>The two cousins of a pair, eldest first — the same key whichever of them calls the other.</summary>
        public static (CharacterId Elder, CharacterId Younger) Normalize(CharacterId a, CharacterId b)
        {
            return BirthOrder(a) <= BirthOrder(b) ? (a, b) : (b, a);
        }

        /// <summary>Order-independent id of a pair ("Maithe+Icaro").</summary>
        public static string PairKey(CharacterId a, CharacterId b)
        {
            (CharacterId elder, CharacterId younger) = Normalize(a, b);
            return elder + "+" + younger;
        }

        /// <summary>Maithe+Ícaro and Joaquim+Maria are siblings ("laço de irmãos"); Lavine is cousin to all.</summary>
        public static bool AreSiblings(CharacterId a, CharacterId b)
        {
            return (a == CharacterId.Maithe && b == CharacterId.Icaro) || (a == CharacterId.Icaro && b == CharacterId.Maithe)
                || (a == CharacterId.Joaquim && b == CharacterId.Maria) || (a == CharacterId.Maria && b == CharacterId.Joaquim);
        }

        /// <summary>
        /// Partner picked when the player never chose one: the sibling (Maithe↔Ícaro, Joaquim↔Maria); Lavine calls
        /// Maria, whose Purificação is "the light she calms her cousin with" (GDD).
        /// </summary>
        public static CharacterId DefaultPartnerFor(CharacterId hero)
        {
            switch (hero)
            {
                case CharacterId.Maithe: return CharacterId.Icaro;
                case CharacterId.Icaro: return CharacterId.Maithe;
                case CharacterId.Joaquim: return CharacterId.Maria;
                case CharacterId.Maria: return CharacterId.Joaquim;
                default: return CharacterId.Maria;
            }
        }

        /// <summary>Cousins the hero can call, in birth order: everyone unlocked except the hero themselves.</summary>
        public static List<CharacterId> AvailablePartners(CharacterId hero, Func<CharacterId, bool> isUnlocked)
        {
            var result = new List<CharacterId>();
            foreach (CharacterId id in ByBirthOrder)
            {
                if (id != hero && (isUnlocked == null || isUnlocked(id)))
                {
                    result.Add(id);
                }
            }

            return result;
        }

        /// <summary>Next partner after <paramref name="current"/> in <see cref="AvailablePartners"/> (wraps; the first one when current isn't in the list).</summary>
        public static CharacterId? NextPartner(CharacterId hero, CharacterId? current, Func<CharacterId, bool> isUnlocked)
        {
            List<CharacterId> available = AvailablePartners(hero, isUnlocked);
            if (available.Count == 0)
            {
                return null;
            }

            int index = current.HasValue ? available.IndexOf(current.Value) : -1;
            return available[(index + 1) % available.Count];
        }

        /// <summary>The five cousins, eldest first.</summary>
        public static readonly CharacterId[] ByBirthOrder =
        {
            CharacterId.Maithe, CharacterId.Maria, CharacterId.Joaquim, CharacterId.Icaro, CharacterId.Lavine
        };
    }

    /// <summary>Why the duo strike can or can't fire right now (HUD label, tests).</summary>
    public enum DuoStrikeReadiness
    {
        /// <summary>Full meter, armed, partner available: press to call.</summary>
        Ready,
        /// <summary>Meter still filling.</summary>
        Charging,
        /// <summary>Encounter just started (<see cref="DuoStrikeRules.ArmDelaySeconds"/>).</summary>
        Arming,
        /// <summary>Between two uses (<see cref="DuoStrikeRules.CooldownSeconds"/>).</summary>
        Cooldown,
        /// <summary>Used up for this encounter.</summary>
        Spent,
        /// <summary>The strike is playing.</summary>
        Executing
    }

    /// <summary>
    /// Per-encounter state of the meter (pure C#; <see cref="DuoStrikeController"/> owns one and feeds it the combat
    /// events). Charge never grows while the strike plays (its own damage doesn't refill it) or once it is spent.
    /// </summary>
    public class DuoStrikeMeter
    {
        public float Charge { get; private set; }
        public int UsesThisEncounter { get; private set; }
        public float CooldownRemaining { get; private set; }
        public float ArmRemaining { get; private set; }
        public bool IsExecuting { get; private set; }

        public int MaxUses { get; }

        public DuoStrikeMeter(int maxUses = DuoStrikeRules.MaxUsesPerEncounter)
        {
            MaxUses = Mathf.Max(1, maxUses);
            BeginEncounter();
        }

        public bool IsSpent => UsesThisEncounter >= MaxUses;

        public bool IsFull => Charge >= DuoStrikeRules.MaxCharge;

        public DuoStrikeReadiness Readiness
        {
            get
            {
                if (IsExecuting) return DuoStrikeReadiness.Executing;
                if (IsSpent) return DuoStrikeReadiness.Spent;
                if (ArmRemaining > 0f) return DuoStrikeReadiness.Arming;
                if (CooldownRemaining > 0f) return DuoStrikeReadiness.Cooldown;
                return IsFull ? DuoStrikeReadiness.Ready : DuoStrikeReadiness.Charging;
            }
        }

        /// <summary>New encounter: empty meter, no uses, armed after <see cref="DuoStrikeRules.ArmDelaySeconds"/>.</summary>
        public void BeginEncounter()
        {
            Charge = 0f;
            UsesThisEncounter = 0;
            CooldownRemaining = 0f;
            ArmRemaining = DuoStrikeRules.ArmDelaySeconds;
            IsExecuting = false;
        }

        /// <summary>Game-time step (the controller skips it while paused).</summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            ArmRemaining = Mathf.Max(0f, ArmRemaining - deltaTime);
            CooldownRemaining = Mathf.Max(0f, CooldownRemaining - deltaTime);
        }

        private bool CanGain => !IsExecuting && !IsSpent;

        public void AddDealt(float damage, float targetMaxHealth, bool isBoss)
        {
            if (CanGain)
            {
                Charge = DuoStrikeRules.AddCharge(Charge, DuoStrikeRules.ChargeFromDealt(damage, targetMaxHealth, isBoss));
            }
        }

        public void AddTaken(float damage, float heroMaxHealth)
        {
            if (CanGain)
            {
                Charge = DuoStrikeRules.AddCharge(Charge, DuoStrikeRules.ChargeFromTaken(damage, heroMaxHealth));
            }
        }

        /// <summary>Starts a strike when <see cref="Readiness"/> is Ready: empties the meter, counts the use, starts the cooldown.</summary>
        public bool TryBegin()
        {
            if (Readiness != DuoStrikeReadiness.Ready)
            {
                return false;
            }

            Charge = 0f;
            UsesThisEncounter++;
            CooldownRemaining = DuoStrikeRules.CooldownSeconds;
            IsExecuting = true;
            return true;
        }

        /// <summary>The strike finished playing (or was interrupted).</summary>
        public void EndExecution()
        {
            IsExecuting = false;
        }

#if UNITY_EDITOR
        /// <summary>Editor/test hook: fills the meter and skips the arm delay.</summary>
        public void ForceReadyForTests()
        {
            Charge = DuoStrikeRules.MaxCharge;
            ArmRemaining = 0f;
        }
#endif
    }
}
