using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Player;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>
    /// The hero's numbers that the straw training dummy reads (Docs/Housing.md §11.8). Built from the live hero when
    /// there is one, else from the carried save (level, attributes, skill levels); see the UI's loadout builder.
    /// Same inputs <see cref="OutgoingDamage.Resolve"/> takes, as plain numbers so the rule runs anywhere (server too).
    /// </summary>
    public sealed class TrainingLoadout
    {
        /// <summary>ProgressionCurve damage multiplier at the hero's level (1 by default).</summary>
        public float LevelMultiplier = 1f;

        /// <summary>Σ Flat AttackDamage modifiers (added before the percent).</summary>
        public float FlatAttackDamage;

        /// <summary>Σ PercentAdditive AttackDamage modifiers (0.05 = +5%: Poder Arcano, affixes, buffs).</summary>
        public float AttackDamagePercent;

        /// <summary>Critical chance 0..1 (balance base + Destreza + affixes).</summary>
        public float CriticalChance;

        /// <summary>Damage multiplier of a critical hit (balance base + CriticalDamage), never below 1.</summary>
        public float CriticalMultiplier = PlayerCombatBalance.DefaultCriticalMultiplier;

        /// <summary>The dummy's armor. Straw: 0, so the numbers are the hero's own.</summary>
        public float DummyDefense;
    }

    /// <summary>One attack to try on the dummy: the basic attack or a skill, its base damage and seconds between uses.</summary>
    public readonly struct TrainingMove
    {
        public readonly string Id;
        public readonly float BaseDamage;
        public readonly float IntervalSeconds;
        public readonly bool IsBasicAttack;

        public TrainingMove(string id, float baseDamage, float intervalSeconds, bool isBasicAttack)
        {
            Id = id ?? string.Empty;
            BaseDamage = Math.Max(0f, baseDamage);
            IntervalSeconds = Math.Max(TrainingDummyRules.MinimumIntervalSeconds, intervalSeconds);
            IsBasicAttack = isBasicAttack;
        }

        /// <summary>
        /// A skill used again as soon as it can: the longer of its cooldown and its cast + recovery (mana is not spent on
        /// the dummy, so it never runs dry).
        /// </summary>
        public static TrainingMove ForSkill(string skillId, float damage, float cooldownSeconds, float castTimeSeconds, float recoverySeconds)
        {
            float interval = Math.Max(cooldownSeconds, Math.Max(0f, castTimeSeconds) + Math.Max(0f, recoverySeconds));
            return new TrainingMove(skillId, damage, interval, false);
        }
    }

    /// <summary>One hit on the dummy: when it landed in the session, how much, crit or not.</summary>
    public readonly struct TrainingHit
    {
        public readonly float Time;
        public readonly int Damage;
        public readonly bool Critical;

        public TrainingHit(float time, int damage, bool critical)
        {
            Time = time;
            Damage = damage;
            Critical = critical;
        }
    }

    /// <summary>What a training session showed: every hit, per-hit numbers, crits and damage per second.</summary>
    public sealed class TrainingSessionResult
    {
        public TrainingMove Move { get; }
        public float Seconds { get; }
        public IReadOnlyList<TrainingHit> Hits { get; }
        public int HitCount => Hits.Count;
        public int TotalDamage { get; }
        public int MinHit { get; }
        public int MaxHit { get; }
        public int CriticalHits { get; }

        /// <summary>Damage of a non-critical hit (what every hit would be without crits).</summary>
        public int NormalHit { get; }

        /// <summary>Damage of a critical hit.</summary>
        public int CriticalHit { get; }

        public float AverageHit => HitCount == 0 ? 0f : TotalDamage / (float)HitCount;
        public float CriticalRate => HitCount == 0 ? 0f : CriticalHits / (float)HitCount;
        public float DamagePerSecond => Seconds <= 0f ? 0f : TotalDamage / Seconds;

        /// <summary>The dummy never pays out: no XP (nor gold nor loot). It is for testing powers only.</summary>
        public int ExperienceGranted => TrainingDummyRules.ExperienceGranted;

        public TrainingSessionResult(TrainingMove move, float seconds, IReadOnlyList<TrainingHit> hits, int normalHit, int criticalHit)
        {
            Move = move;
            Seconds = seconds;
            Hits = hits ?? Array.Empty<TrainingHit>();
            NormalHit = normalHit;
            CriticalHit = criticalHit;
            int min = int.MaxValue, max = 0, total = 0, crits = 0;
            foreach (TrainingHit hit in Hits)
            {
                total += hit.Damage;
                min = Math.Min(min, hit.Damage);
                max = Math.Max(max, hit.Damage);
                if (hit.Critical) crits++;
            }

            TotalDamage = total;
            MinHit = Hits.Count == 0 ? 0 : min;
            MaxHit = max;
            CriticalHits = crits;
        }
    }

    /// <summary>
    /// "Boneco de treino de palha" (casa_quintal_037, Docs/Housing.md §11.8): the active cousin tries an attack on the
    /// dummy for <see cref="SessionSeconds"/> and sees damage per hit, crits and damage per second. Pure and seeded.
    /// <list type="bullet">
    /// <item>The real formulas: <see cref="OutgoingDamage"/>'s attacker pipeline (level multiplier, AttackDamage flat then
    /// percent, <see cref="OutgoingDamage.RollCritical"/>, crit multiplier) and <see cref="DamageCalculator"/>'s target
    /// side (armor curve, rounding, minimum damage) against a straw dummy with no armor.</item>
    /// <item><b>No XP, no gold, no loot, no durability, no mana</b> (<see cref="ExperienceGranted"/> = 0): the city is a
    /// safe zone and this is not a fight. It only shows numbers. Online (FASE 12) the server can run the same session.</item>
    /// </list>
    /// </summary>
    public static class TrainingDummyRules
    {
        /// <summary>How long a training session lasts (the DPS window).</summary>
        public const float SessionSeconds = 10f;

        /// <summary>Fastest an attack can repeat on the dummy (keeps a zero cooldown from looping forever).</summary>
        public const float MinimumIntervalSeconds = 0.1f;

        /// <summary>Same floor as <see cref="DamageBalanceConfig.MinimumDamage"/>'s default: a hit is never 0.</summary>
        public const float MinimumDamage = 1f;

        /// <summary>The dummy gives no XP. Ever.</summary>
        public const int ExperienceGranted = 0;

        /// <summary>The attacker side of one hit (<see cref="OutgoingDamage.Resolve"/> with plain numbers).</summary>
        public static float ResolveOutgoing(float baseDamage, TrainingLoadout loadout, bool critical)
        {
            if (baseDamage <= 0f || loadout == null)
            {
                return 0f;
            }

            float damage = baseDamage * Math.Max(0f, loadout.LevelMultiplier);
            damage = (damage + loadout.FlatAttackDamage) * (1f + loadout.AttackDamagePercent);
            if (critical)
            {
                damage *= Math.Max(1f, loadout.CriticalMultiplier);
            }

            return Math.Max(0f, damage);
        }

        /// <summary>The number the dummy shows for one hit (target side: armor, rounding, minimum).</summary>
        public static int ResolveHit(float baseDamage, TrainingLoadout loadout, bool critical)
        {
            if (baseDamage <= 0f || loadout == null)
            {
                return 0;
            }

            float amount = ResolveOutgoing(baseDamage, loadout, critical)
                * DamageCalculator.GetDefenseMultiplier(loadout.DummyDefense, 0f, null);
            return (int)Math.Max(MinimumDamage, Math.Round(amount));
        }

        /// <summary>
        /// Uses <paramref name="move"/> as often as it can for <paramref name="seconds"/> (first hit at 0 s, then every
        /// <see cref="TrainingMove.IntervalSeconds"/>), rolling each crit from <paramref name="random"/> (null = no crits).
        /// </summary>
        public static TrainingSessionResult Simulate(TrainingMove move, TrainingLoadout loadout, Random random, float seconds = SessionSeconds)
        {
            loadout ??= new TrainingLoadout();
            seconds = Math.Max(0f, seconds);
            var hits = new List<TrainingHit>();
            int normal = ResolveHit(move.BaseDamage, loadout, false);
            int critical = ResolveHit(move.BaseDamage, loadout, true);
            float chance = Math.Max(0f, Math.Min(1f, loadout.CriticalChance));

            if (move.BaseDamage > 0f)
            {
                float interval = Math.Max(MinimumIntervalSeconds, move.IntervalSeconds);
                for (int i = 0; ; i++)
                {
                    float time = i * interval;
                    if (time >= seconds)
                    {
                        break;
                    }

                    bool crit = chance > 0f && random != null && OutgoingDamage.RollCritical(chance, (float)random.NextDouble());
                    hits.Add(new TrainingHit(time, crit ? critical : normal, crit));
                }
            }

            return new TrainingSessionResult(move, seconds, hits, normal, critical);
        }

        /// <summary>
        /// The loadout from the hero's attributes alone (the map has no live hero): Destreza adds crit chance, Poder
        /// Arcano adds attack percent (<see cref="AttributeSet"/>'s per-point values).
        /// </summary>
        public static TrainingLoadout FromAttributes(float levelMultiplier, int dexterity, int arcanePower, float baseCriticalChance,
            float baseCriticalMultiplier)
        {
            return new TrainingLoadout
            {
                LevelMultiplier = levelMultiplier,
                AttackDamagePercent = Math.Max(0, arcanePower) * AttributeSet.ArcanePowerAttackDamagePerPoint,
                CriticalChance = Math.Max(0f, Math.Min(1f, baseCriticalChance + Math.Max(0, dexterity) * AttributeSet.DexterityCriticalChancePerPoint)),
                CriticalMultiplier = Math.Max(1f, baseCriticalMultiplier),
            };
        }
    }
}
