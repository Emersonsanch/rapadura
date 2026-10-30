using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.Skills;

namespace Rapadura.Gameplay.Tacho
{
    /// <summary>The five wing/cape styles of the Tacho Encantado, one per cousin (append only: ints in saves/assets).</summary>
    public enum WingStyle
    {
        /// <summary>Asas de Carcará: Maithe, the archer (sharp eyes, the sertão hawk). Leans to damage.</summary>
        Carcara = 0,
        /// <summary>Capa de Vaqueiro Encantada: Joaquim, the shield (leather cape of the cowboys). Leans to defense.</summary>
        CapaVaqueiro = 1,
        /// <summary>Asas de Beija-flor: Maria, the healer (quick and light). Leans to speed.</summary>
        BeijaFlor = 2,
        /// <summary>Asas de Asa-branca: Ícaro, the inventor (the bird of the song, flight at last). Balanced.</summary>
        AsaBranca = 3,
        /// <summary>Asas de Vaga-lume: Lavine, the youngest (little lights in the dark). Damage and speed.</summary>
        VagaLume = 4,
    }

    /// <summary>One material line of a Tacho recipe (item id + quantity).</summary>
    public readonly struct TachoIngredient
    {
        public readonly string ItemId;
        public readonly int Quantity;

        public TachoIngredient(string itemId, int quantity)
        {
            ItemId = itemId;
            Quantity = quantity;
        }
    }

    /// <summary>A style's level-1 stats, its cousin, its materials and its three palette colours (pure data).</summary>
    public sealed class WingStyleSpec
    {
        public readonly WingStyle Style;
        public readonly CharacterId Cousin;
        /// <summary>Item id without the level ("item_asas_carcara"); the items are "<c>BaseId</c>_1".."_3".</summary>
        public readonly string BaseId;
        /// <summary>AttackDamage PercentAdditive at level 1.</summary>
        public readonly float Damage;
        /// <summary>Defense PercentAdditive at level 1 (less damage taken, DamageCalculator).</summary>
        public readonly float Defense;
        /// <summary>MoveSpeed PercentAdditive at level 1.</summary>
        public readonly float MoveSpeed;
        /// <summary>Biome materials of the level-1 recipe (besides the main item, the Pedra do Caos and the gold).</summary>
        public readonly TachoIngredient[] Materials;
        /// <summary>Palette colours of the procedural mesh (<see cref="WingMeshBuilder"/>): the low-poly enemy palette.</summary>
        public readonly EnemySwatch MainSwatch;
        public readonly EnemySwatch AccentSwatch;
        public readonly EnemySwatch TipSwatch;
        /// <summary>A cape hangs from the shoulders instead of opening sideways.</summary>
        public readonly bool IsCape;

        public WingStyleSpec(WingStyle style, CharacterId cousin, string baseId, float damage, float defense, float moveSpeed,
            TachoIngredient[] materials, EnemySwatch mainSwatch, EnemySwatch accentSwatch, EnemySwatch tipSwatch, bool isCape = false)
        {
            Style = style;
            Cousin = cousin;
            BaseId = baseId;
            Damage = damage;
            Defense = defense;
            MoveSpeed = moveSpeed;
            Materials = materials;
            MainSwatch = mainSwatch;
            AccentSwatch = accentSwatch;
            TipSwatch = tipSwatch;
            IsCape = isCape;
        }
    }

    /// <summary>One combat bonus of an equipped wing (applied by InventoryManager under <c>equip_Wings/wing/*</c>).</summary>
    public readonly struct WingModifier
    {
        public readonly StatType Stat;
        public readonly ModifierApplication Application;
        public readonly float Value;
        /// <summary>The "absorção" line of a level 2+ wing (same Defense % channel, its own tooltip line).</summary>
        public readonly bool IsAbsorb;

        public WingModifier(StatType stat, ModifierApplication application, float value, bool isAbsorb = false)
        {
            Stat = stat;
            Application = application;
            Value = value;
            IsAbsorb = isAbsorb;
        }
    }

    /// <summary>
    /// The wings of the Tacho Encantado (FASE 16, Docs/Balance/README.md "Tacho Encantado e asas"): five styles × levels 1–3
    /// as items (<c>Resources/Items/item_asas_*_N</c>, slot <see cref="Rapadura.Gameplay.Items.EquipmentSlot.Wings"/>),
    /// level 4 "lendária" documented only (no item, no recipe). Pure data, parsed by <c>tools/balance/power_stack.py</c>:
    /// keep the <c>new WingStyleSpec(...)</c> lines and the two level tables in this shape.
    /// <list type="bullet">
    /// <item>Stats of level N = the style's level-1 stats × <see cref="LevelScale"/>[N].</item>
    /// <item>Level 2+ also absorb damage (<see cref="AbsorbByLevel"/>): in this game a percent Defense already means
    /// "less damage taken", so the absorption rides that channel (summed with every other percent Defense, capped at 80%
    /// by DamageCalculator) as its own modifier and tooltip line.</item>
    /// <item>Wings are bound (never drop on death) and not refinable (no base stat).</item>
    /// </list>
    /// </summary>
    public static class WingCatalog
    {
        public const int MaxCraftableLevel = 3;
        /// <summary>Level 4 "Asas Lendárias": stats documented (<see cref="LevelScale"/>[4]) but no item and no recipe yet.</summary>
        public const int LegendaryLevel = 4;

        // Multiplier of the level-1 stats, index = wing level (0 unused, 4 = legendary, documented only).
        public static readonly float[] LevelScale = { 0f, 1f, 1.25f, 1.5f, 2f };

        // Damage absorbed (Defense PercentAdditive), index = wing level.
        public static readonly float[] AbsorbByLevel = { 0f, 0f, 0.01f, 0.02f, 0.03f };

        public static readonly IReadOnlyList<WingStyleSpec> Styles = new[]
        {
            new WingStyleSpec(WingStyle.Carcara, CharacterId.Maithe, "item_asas_carcara", 0.015f, 0.01f, 0.03f,
                new[] { new TachoIngredient("item_fiber", 6), new TachoIngredient("item_stone", 3) }, EnemySwatch.Leather, EnemySwatch.OwlFace, EnemySwatch.WolfFurDark),
            new WingStyleSpec(WingStyle.CapaVaqueiro, CharacterId.Joaquim, "item_capa_vaqueiro", 0.01f, 0.015f, 0.02f,
                new[] { new TachoIngredient("item_fiber", 8), new TachoIngredient("item_iron", 2) }, EnemySwatch.Leather, EnemySwatch.WarCape, EnemySwatch.Gold, isCape: true),
            new WingStyleSpec(WingStyle.BeijaFlor, CharacterId.Maria, "item_asas_beija_flor", 0.01f, 0.01f, 0.05f,
                new[] { new TachoIngredient("item_herb", 6), new TachoIngredient("item_fiber", 4) }, EnemySwatch.CaveMoss, EnemySwatch.Crystal, EnemySwatch.WarCape),
            new WingStyleSpec(WingStyle.AsaBranca, CharacterId.Icaro, "item_asas_asa_branca", 0.01f, 0.01f, 0.03f,
                new[] { new TachoIngredient("item_fiber", 6), new TachoIngredient("item_iron", 3) }, EnemySwatch.OwlFace, EnemySwatch.Bone, EnemySwatch.Steel),
            new WingStyleSpec(WingStyle.VagaLume, CharacterId.Lavine, "item_asas_vaga_lume", 0.015f, 0.005f, 0.04f,
                new[] { new TachoIngredient("item_herb", 5), new TachoIngredient("item_stone", 3) }, EnemySwatch.Shadow, EnemySwatch.FireflyGlow, EnemySwatch.Gold),
        };

        public static WingStyleSpec Get(WingStyle style)
        {
            foreach (WingStyleSpec spec in Styles)
            {
                if (spec.Style == style)
                {
                    return spec;
                }
            }

            return null;
        }

        /// <summary>The style suggested to <paramref name="cousin"/> first in the Tacho (any cousin can wear any wing).</summary>
        public static WingStyleSpec ForCousin(CharacterId cousin)
        {
            foreach (WingStyleSpec spec in Styles)
            {
                if (spec.Cousin == cousin)
                {
                    return spec;
                }
            }

            return Styles[0];
        }

        public static int ClampLevel(int level) => Math.Max(1, Math.Min(LegendaryLevel, level));

        /// <summary>"item_asas_carcara_2" (levels 1..3 exist as items).</summary>
        public static string ItemId(WingStyle style, int level) => Get(style).BaseId + "_" + ClampLevel(level);

        /// <summary>Every wing item id (5 styles × levels 1..3).</summary>
        public static IEnumerable<string> AllItemIds()
        {
            foreach (WingStyleSpec spec in Styles)
            {
                for (int level = 1; level <= MaxCraftableLevel; level++)
                {
                    yield return spec.BaseId + "_" + level;
                }
            }
        }

        /// <summary>True when <paramref name="itemId"/> is a wing item; gives its style and level.</summary>
        public static bool TryParse(string itemId, out WingStyle style, out int level)
        {
            style = default;
            level = 0;
            if (string.IsNullOrEmpty(itemId))
            {
                return false;
            }

            foreach (WingStyleSpec spec in Styles)
            {
                if (itemId.Length == spec.BaseId.Length + 2 && itemId.StartsWith(spec.BaseId + "_", StringComparison.Ordinal))
                {
                    int parsed = itemId[itemId.Length - 1] - '0';
                    if (parsed >= 1 && parsed <= MaxCraftableLevel)
                    {
                        style = spec.Style;
                        level = parsed;
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsWing(string itemId) => TryParse(itemId, out _, out _);

        public static float DamagePercent(WingStyle style, int level) => Get(style).Damage * LevelScale[ClampLevel(level)];

        public static float DefensePercent(WingStyle style, int level) => Get(style).Defense * LevelScale[ClampLevel(level)];

        public static float MoveSpeedPercent(WingStyle style, int level) => Get(style).MoveSpeed * LevelScale[ClampLevel(level)];

        public static float Absorb(int level) => AbsorbByLevel[ClampLevel(level)];

        /// <summary>The combat bonuses of a wing item (empty for anything else): damage, defense, speed, and absorb from level 2.</summary>
        public static List<WingModifier> ModifiersFor(string itemId)
        {
            var list = new List<WingModifier>();
            if (!TryParse(itemId, out WingStyle style, out int level))
            {
                return list;
            }

            list.Add(new WingModifier(StatType.AttackDamage, ModifierApplication.PercentAdditive, DamagePercent(style, level)));
            list.Add(new WingModifier(StatType.Defense, ModifierApplication.PercentAdditive, DefensePercent(style, level)));
            list.Add(new WingModifier(StatType.MoveSpeed, ModifierApplication.PercentAdditive, MoveSpeedPercent(style, level)));
            float absorb = Absorb(level);
            if (absorb > 0f)
            {
                list.Add(new WingModifier(StatType.Defense, ModifierApplication.PercentAdditive, absorb, isAbsorb: true));
            }

            return list;
        }
    }
}
