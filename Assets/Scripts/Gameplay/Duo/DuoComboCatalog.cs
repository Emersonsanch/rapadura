using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Skills;

namespace Rapadura.Gameplay.Duo
{
    /// <summary>A buff/debuff of a combo, built into a fresh <see cref="StatModifierDefinition"/> per strike.</summary>
    public readonly struct DuoEffectSpec
    {
        public readonly StatType Stat;
        public readonly ModifierApplication Application;
        public readonly float Value;
        public readonly float DurationSeconds;

        public DuoEffectSpec(StatType stat, float value, float durationSeconds, ModifierApplication application = ModifierApplication.PercentAdditive)
        {
            Stat = stat;
            Application = application;
            Value = value;
            DurationSeconds = durationSeconds;
        }

        public StatModifierDefinition Build(string displayName, bool isDebuff)
        {
            return new StatModifierDefinition
            {
                displayName = displayName,
                affectedStat = Stat,
                application = Application,
                value = Value,
                durationSeconds = DurationSeconds,
                isDebuff = isDebuff,
                ticksOverTime = false
            };
        }
    }

    /// <summary>
    /// One duo combo: an unordered pair of cousins and an effect made only of systems that already exist — area damage
    /// (<c>OutgoingDamage</c>), stun (<c>BuffController.ApplyStun</c>/<c>StunRules</c>), healing, buffs on the hero,
    /// debuffs on the enemies and knockback (<c>KnockbackReceiver</c>). The two shout lines are spoken by
    /// <see cref="Elder"/> then <see cref="Younger"/> (birth order), whichever of them is the active hero.
    /// </summary>
    public sealed class DuoComboSpec
    {
        public string Id { get; }
        public CharacterId Elder { get; }
        public CharacterId Younger { get; }
        public ElementType Element { get; }

        /// <summary>× <see cref="DuoStrikeRules.BaseDamage"/>; 0 = no damage.</summary>
        public float DamageMultiplier { get; }

        /// <summary>Radius of the area around the strike centre.</summary>
        public float Radius { get; }

        /// <summary>Stun on every enemy hit (bosses take half, <see cref="StunRules"/>).</summary>
        public float StunSeconds { get; }

        /// <summary>Fraction of the hero's max health healed.</summary>
        public float HealFraction { get; }

        /// <summary>Impulse pushing enemies away from the centre (bosses × <see cref="DuoStrikeRules.BossKnockbackMultiplier"/>).</summary>
        public float KnockbackForce { get; }

        public IReadOnlyList<DuoEffectSpec> HeroBuffs { get; }
        public IReadOnlyList<DuoEffectSpec> EnemyDebuffs { get; }

        public DuoComboSpec(string id, CharacterId a, CharacterId b, ElementType element, float damageMultiplier, float radius,
            float stunSeconds = 0f, float healFraction = 0f, float knockbackForce = 0f,
            DuoEffectSpec[] heroBuffs = null, DuoEffectSpec[] enemyDebuffs = null)
        {
            Id = id;
            (Elder, Younger) = DuoStrikeRules.Normalize(a, b);
            Element = element;
            DamageMultiplier = damageMultiplier;
            Radius = radius;
            StunSeconds = stunSeconds;
            HealFraction = healFraction;
            KnockbackForce = knockbackForce;
            HeroBuffs = heroBuffs ?? System.Array.Empty<DuoEffectSpec>();
            EnemyDebuffs = enemyDebuffs ?? System.Array.Empty<DuoEffectSpec>();
        }

        /// <summary>"Laço de irmãos": Maithe+Ícaro, Joaquim+Maria — stronger numbers and a flashier presentation.</summary>
        public bool IsSiblingBond => DuoStrikeRules.AreSiblings(Elder, Younger);

        public bool Involves(CharacterId id) => Elder == id || Younger == id;

        /// <summary>The other cousin of the pair (the one who appears) when <paramref name="hero"/> is in it.</summary>
        public CharacterId PartnerOf(CharacterId hero) => hero == Elder ? Younger : Elder;

        /// <summary>BuffController source of this combo's buffs/debuffs (a re-use refreshes, other sources stack).</summary>
        public string SourceKey => "duo/" + Id;

        public string NameKey => "duo.combo." + Id + ".name";

        /// <summary>Shout line 0 (spoken by <see cref="Elder"/>) or 1 (<see cref="Younger"/>).</summary>
        public string LineKey(int index) => "duo.combo." + Id + ".line" + (index <= 0 ? 1 : 2);

        public CharacterId Speaker(int index) => index <= 0 ? Elder : Younger;

        /// <summary>A rough "how much the combo does" score (tests keep the sibling combos on top).</summary>
        public float PowerScore
        {
            get
            {
                float score = DamageMultiplier + StunSeconds * 0.35f + HealFraction * 2f + KnockbackForce * 0.02f;
                foreach (DuoEffectSpec buff in HeroBuffs)
                {
                    // Flat regen is HP per second: score it like healing (per ~100 HP), percent buffs per second of uptime.
                    score += buff.Application == ModifierApplication.Flat
                        ? System.Math.Abs(buff.Value) * buff.DurationSeconds * 0.02f
                        : System.Math.Abs(buff.Value) * buff.DurationSeconds * 0.06f;
                }

                foreach (DuoEffectSpec debuff in EnemyDebuffs)
                {
                    score += System.Math.Abs(debuff.Value) * debuff.DurationSeconds * 0.06f;
                }

                return score;
            }
        }
    }

    /// <summary>
    /// The 10 duo combos, one per pair of cousins (see Docs/Balance/README.md, "Golpe em dupla", for the table).
    /// Lookup is by unordered pair: <c>Find(Maithe, Icaro) == Find(Icaro, Maithe)</c>.
    /// </summary>
    public static class DuoComboCatalog
    {
        public const string SkyThunderId = "maithe_icaro";
        public const string HolyBastionId = "maria_joaquim";

        public static readonly IReadOnlyList<DuoComboSpec> All = new[]
        {
            // ---- Laço de irmãos ----
            // Maithe fires, Ícaro's drone charges the arrow: the widest, hardest-hitting combo.
            new DuoComboSpec(SkyThunderId, CharacterId.Maithe, CharacterId.Icaro, ElementType.Lightning,
                damageMultiplier: 1.4f, radius: 6f, stunSeconds: 1.2f, knockbackForce: 5f,
                heroBuffs: new[] { new DuoEffectSpec(StatType.CriticalChance, 0.15f, 6f) }),
            // Joaquim raises the wall, Maria blesses it: big heal + defense + shove.
            new DuoComboSpec(HolyBastionId, CharacterId.Maria, CharacterId.Joaquim, ElementType.Holy,
                damageMultiplier: 1.0f, radius: 5.5f, healFraction: 0.30f, knockbackForce: 7f,
                heroBuffs: new[] { new DuoEffectSpec(StatType.Defense, 0.40f, 8f) }),

            // ---- Primos ----
            new DuoComboSpec("maithe_maria", CharacterId.Maithe, CharacterId.Maria, ElementType.Holy,
                damageMultiplier: 1.0f, radius: 5f, healFraction: 0.15f),
            new DuoComboSpec("maithe_joaquim", CharacterId.Maithe, CharacterId.Joaquim, ElementType.Earth,
                damageMultiplier: 1.15f, radius: 4.5f, knockbackForce: 5f,
                heroBuffs: new[] { new DuoEffectSpec(StatType.CriticalChance, 0.20f, 6f) }),
            new DuoComboSpec("maithe_lavine", CharacterId.Maithe, CharacterId.Lavine, ElementType.Dark,
                damageMultiplier: 1.2f, radius: 4.5f,
                enemyDebuffs: new[] { new DuoEffectSpec(StatType.MoveSpeed, -0.4f, 4f) }),
            new DuoComboSpec("maria_icaro", CharacterId.Maria, CharacterId.Icaro, ElementType.Lightning,
                damageMultiplier: 0.7f, radius: 4.5f, healFraction: 0.20f,
                heroBuffs: new[] { new DuoEffectSpec(StatType.HealthRegen, 3f, 6f, ModifierApplication.Flat) }),
            new DuoComboSpec("maria_lavine", CharacterId.Maria, CharacterId.Lavine, ElementType.Arcane,
                damageMultiplier: 0.9f, radius: 4.5f, stunSeconds: 1.0f, healFraction: 0.15f),
            new DuoComboSpec("joaquim_icaro", CharacterId.Joaquim, CharacterId.Icaro, ElementType.Earth,
                damageMultiplier: 1.1f, radius: 5f, stunSeconds: 1.0f, knockbackForce: 4f),
            new DuoComboSpec("joaquim_lavine", CharacterId.Joaquim, CharacterId.Lavine, ElementType.Dark,
                damageMultiplier: 0.9f, radius: 4.5f, stunSeconds: 1.4f,
                heroBuffs: new[] { new DuoEffectSpec(StatType.Defense, 0.25f, 6f) }),
            new DuoComboSpec("icaro_lavine", CharacterId.Icaro, CharacterId.Lavine, ElementType.Arcane,
                damageMultiplier: 1.3f, radius: 5f,
                heroBuffs: new[] { new DuoEffectSpec(StatType.AttackSpeed, 0.20f, 6f) }),
        };

        private static Dictionary<string, DuoComboSpec> _byPair;

        /// <summary>The combo of the pair (either order), or null for a cousin paired with themselves.</summary>
        public static DuoComboSpec Find(CharacterId a, CharacterId b)
        {
            if (a == b)
            {
                return null;
            }

            if (_byPair == null)
            {
                var map = new Dictionary<string, DuoComboSpec>();
                foreach (DuoComboSpec combo in All)
                {
                    map[DuoStrikeRules.PairKey(combo.Elder, combo.Younger)] = combo;
                }

                _byPair = map;
            }

            return _byPair.TryGetValue(DuoStrikeRules.PairKey(a, b), out DuoComboSpec spec) ? spec : null;
        }

        public static DuoComboSpec FindById(string id)
        {
            foreach (DuoComboSpec combo in All)
            {
                if (combo.Id == id)
                {
                    return combo;
                }
            }

            return null;
        }
    }
}
