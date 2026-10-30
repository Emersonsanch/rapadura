using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Skills;

namespace Rapadura.Gameplay.Items.Options
{
    /// <summary>One stat bonus of an item option (same shape as a rolled affix: stat + application + value).</summary>
    public readonly struct OptionModifier
    {
        public readonly StatType Stat;
        public readonly ModifierApplication Application;
        public readonly float Value;

        public OptionModifier(StatType stat, ModifierApplication application, float value)
        {
            Stat = stat;
            Application = application;
            Value = value;
        }
    }

    /// <summary>
    /// The green "Excelente" lines (MU Online's excellent options). Bits of <c>InventorySlotData.excellentMask</c>:
    /// weapon lines in the low byte, armour lines in the second. Append only (saves store the mask as an int).
    /// </summary>
    [Flags]
    public enum ExcellentOption
    {
        None = 0,
        WeaponDamage = 1 << 0,
        WeaponSpeed = 1 << 1,
        WeaponCritical = 1 << 2,
        ArmourHealth = 1 << 8,
        ArmourDefense = 1 << 9,
        ArmourThorns = 1 << 10,
    }

    /// <summary>One excellent line: its bit, whether it rolls on weapons (else armour), its bonus and text key.</summary>
    public readonly struct ExcellentLine
    {
        public readonly ExcellentOption Option;
        public readonly bool Weapon;
        public readonly OptionModifier Modifier;

        public ExcellentLine(ExcellentOption option, bool weapon, OptionModifier modifier)
        {
            Option = option;
            Weapon = weapon;
            Modifier = modifier;
        }

        public string TextKey => "ui.item_option.excellent." + Option.ToString().ToLowerInvariant();
    }

    /// <summary>A bonus tier of an ancient set, active from <see cref="Pieces"/> equipped pieces (tiers add up).</summary>
    public readonly struct AncientTier
    {
        public readonly int Pieces;
        public readonly OptionModifier Modifier;

        public AncientTier(int pieces, OptionModifier modifier)
        {
            Pieces = pieces;
            Modifier = modifier;
        }
    }

    /// <summary>An ancient set (MU's Ancient items, our names): one per cousin, bonus by pieces worn.</summary>
    public sealed class AncientSet
    {
        public readonly string Id;
        public readonly CharacterId Cousin;
        public readonly AncientTier[] Tiers;

        public AncientSet(string id, CharacterId cousin, AncientTier[] tiers)
        {
            Id = id;
            Cousin = cousin;
            Tiers = tiers;
        }

        public string NameKey => "ui.item_option.set." + Id + ".name";
    }

    /// <summary>A socket engraving: what it costs at the ferreiro (material × quantity + gold) and what it gives.</summary>
    public sealed class Engraving
    {
        public readonly string Id;
        /// <summary>True = goes only in weapons; false = only in armour.</summary>
        public readonly bool Weapon;
        public readonly string MaterialId;
        public readonly int MaterialQuantity;
        public readonly int Gold;
        public readonly OptionModifier Modifier;

        public Engraving(string id, bool weapon, string materialId, int materialQuantity, int gold, OptionModifier modifier)
        {
            Id = id;
            Weapon = weapon;
            MaterialId = materialId;
            MaterialQuantity = materialQuantity;
            Gold = gold;
            Modifier = modifier;
        }

        public string NameKey => "ui.item_option.engraving." + Id + ".name";
    }

    /// <summary>
    /// Data of the MU-style item options (FASE 16, Docs/Balance/README.md "Opções de item"): excellent lines, luck, the
    /// five ancient sets, socket engravings and the drop chances rolled by the loot. Kept as literal tables so
    /// <c>tools/balance/power_stack.py</c> parses the numbers (keep the <c>Mod(StatType.X, ModifierApplication.Y, 0.02f)</c>
    /// shape). Small on purpose: every bonus is a fraction of a refine +9 weapon (§20), so the stack stays in budget.
    /// </summary>
    public static class ItemOptionCatalog
    {
        private static OptionModifier Mod(StatType stat, ModifierApplication application, float value) => new OptionModifier(stat, application, value);

        public static readonly ExcellentLine[] ExcellentLines =
        {
            new ExcellentLine(ExcellentOption.WeaponDamage, true, Mod(StatType.AttackDamage, ModifierApplication.PercentAdditive, 0.015f)),
            new ExcellentLine(ExcellentOption.WeaponSpeed, true, Mod(StatType.AttackSpeed, ModifierApplication.PercentAdditive, 0.015f)),
            new ExcellentLine(ExcellentOption.WeaponCritical, true, Mod(StatType.CriticalChance, ModifierApplication.PercentAdditive, 0.015f)),
            new ExcellentLine(ExcellentOption.ArmourHealth, false, Mod(StatType.MaxHealth, ModifierApplication.PercentAdditive, 0.02f)),
            new ExcellentLine(ExcellentOption.ArmourDefense, false, Mod(StatType.Defense, ModifierApplication.PercentAdditive, 0.015f)),
            new ExcellentLine(ExcellentOption.ArmourThorns, false, Mod(StatType.Thorns, ModifierApplication.PercentAdditive, 0.01f)),
        };

        /// <summary>"Com Sorte": a little more critical chance (and <see cref="Refining.RefineRules.LuckBonus"/> on refines).</summary>
        public static readonly OptionModifier LuckModifier = Mod(StatType.CriticalChance, ModifierApplication.PercentAdditive, 0.005f);

        public static readonly AncientSet[] AncientSets =
        {
            new AncientSet("ancient_carcara", CharacterId.Maithe, new[]
            {
                new AncientTier(2, Mod(StatType.CriticalChance, ModifierApplication.PercentAdditive, 0.015f)),
                new AncientTier(3, Mod(StatType.AttackDamage, ModifierApplication.PercentAdditive, 0.015f)),
                new AncientTier(4, Mod(StatType.AttackSpeed, ModifierApplication.PercentAdditive, 0.015f)),
            }),
            new AncientSet("ancient_vaqueiro", CharacterId.Joaquim, new[]
            {
                new AncientTier(2, Mod(StatType.Defense, ModifierApplication.Flat, 2f)),
                new AncientTier(3, Mod(StatType.MaxHealth, ModifierApplication.PercentAdditive, 0.02f)),
                new AncientTier(4, Mod(StatType.Defense, ModifierApplication.PercentAdditive, 0.01f)),
            }),
            new AncientSet("ancient_benzedeira", CharacterId.Maria, new[]
            {
                new AncientTier(2, Mod(StatType.MaxHealth, ModifierApplication.PercentAdditive, 0.015f)),
                new AncientTier(3, Mod(StatType.CooldownReduction, ModifierApplication.PercentAdditive, 0.02f)),
                new AncientTier(4, Mod(StatType.Defense, ModifierApplication.PercentAdditive, 0.01f)),
            }),
            new AncientSet("ancient_inventor", CharacterId.Icaro, new[]
            {
                new AncientTier(2, Mod(StatType.AttackSpeed, ModifierApplication.PercentAdditive, 0.015f)),
                new AncientTier(3, Mod(StatType.CooldownReduction, ModifierApplication.PercentAdditive, 0.02f)),
                new AncientTier(4, Mod(StatType.AttackDamage, ModifierApplication.PercentAdditive, 0.015f)),
            }),
            new AncientSet("ancient_vagalume", CharacterId.Lavine, new[]
            {
                new AncientTier(2, Mod(StatType.CriticalChance, ModifierApplication.PercentAdditive, 0.015f)),
                new AncientTier(3, Mod(StatType.CriticalDamage, ModifierApplication.PercentAdditive, 0.04f)),
                new AncientTier(4, Mod(StatType.AttackDamage, ModifierApplication.PercentAdditive, 0.015f)),
            }),
        };

        /// <summary>
        /// Engravings use materials that already drop (no new item): iron, stone, herb and the Pedra do Caos. Like MU's
        /// sockets, the offensive ones go only in weapons and the defensive ones only in armour (<c>true</c> = weapon),
        /// so eight armour sockets never stack damage.
        /// </summary>
        public static readonly Engraving[] Engravings =
        {
            new Engraving("engrave_ferro", true, "item_iron", 2, 10, Mod(StatType.AttackDamage, ModifierApplication.PercentAdditive, 0.01f)),
            new Engraving("engrave_caos", true, "item_pedra_caos", 1, 25, Mod(StatType.CriticalChance, ModifierApplication.PercentAdditive, 0.015f)),
            new Engraving("engrave_pedra", false, "item_stone", 3, 10, Mod(StatType.Defense, ModifierApplication.Flat, 1f)),
            new Engraving("engrave_erva", false, "item_herb", 3, 10, Mod(StatType.MaxHealth, ModifierApplication.PercentAdditive, 0.015f)),
        };

        // -------------------------------------------------------------- drop chances (per equipment piece dropped)

        public const float ExcellentChanceCommon = 0.015f;
        public const float ExcellentChanceBoss = 0.08f;
        /// <summary>Lines on an excellent piece: always 1 off a common enemy, 1 or 2 off a boss.</summary>
        public const int ExcellentLinesBossMax = 2;
        public const float LuckChanceCommon = 0.05f;
        public const float LuckChanceBoss = 0.15f;
        public const float SocketChanceCommon = 0.04f;
        public const float SocketChanceBoss = 0.12f;
        public const int MaxSockets = 2;
        /// <summary>Ancient pieces only come from bosses (and are never excellent too, like MU).</summary>
        public const float AncientChanceBoss = 0.04f;

        public static ExcellentLine? FindLine(ExcellentOption option)
        {
            foreach (ExcellentLine line in ExcellentLines)
            {
                if (line.Option == option)
                {
                    return line;
                }
            }

            return null;
        }

        public static AncientSet FindSet(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            foreach (AncientSet set in AncientSets)
            {
                if (set.Id == id)
                {
                    return set;
                }
            }

            return null;
        }

        public static Engraving FindEngraving(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            foreach (Engraving engraving in Engravings)
            {
                if (engraving.Id == id)
                {
                    return engraving;
                }
            }

            return null;
        }

        public static IEnumerable<ExcellentLine> LinesFor(bool weapon)
        {
            foreach (ExcellentLine line in ExcellentLines)
            {
                if (line.Weapon == weapon)
                {
                    yield return line;
                }
            }
        }
    }
}
