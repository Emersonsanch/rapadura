using System.Collections.Generic;
using System.IO;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Skills;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// One-shot editor tool that seeds a starter set of real <see cref="ItemAffixDefinition"/>
    /// assets under Assets/Resources/Affixes — the procedural item generation system
    /// (<c>ItemAffixDefinition</c>/<c>AffixDatabase</c>/<c>ProceduralItemGenerator</c>) was already
    /// built and tested, but nothing had actually authored any affixes for it to draw from (see
    /// TODO.md Fase "Itens" > "autoria de afixos reais em Resources/Affixes"). Mirrors
    /// <see cref="SkillDatabaseSeeder"/>/<see cref="EnemyDatabaseSeeder"/>'s pattern: safe to
    /// re-run (updates existing assets by matching affix id, doesn't duplicate).
    ///
    /// Run via Rapadura > Seed Example Affixes.
    /// </summary>
    public static class AffixDatabaseSeeder
    {
        private const string AffixFolder = "Assets/Resources/Affixes";

        private class AffixSeed
        {
            public string id;
            public string fallbackText;
            public AffixSlotKind slotKind;
            public StatType stat;
            public ModifierApplication application;
            public float min;
            public float max;
            public int weight;
            public EquipmentSlot requiredSlot;
        }

        [MenuItem("Rapadura/Seed Example Affixes")]
        public static void SeedAffixes()
        {
            if (!Directory.Exists(AffixFolder))
            {
                Directory.CreateDirectory(AffixFolder);
            }

            foreach (AffixSeed seed in BuildSeeds())
            {
                CreateOrUpdateAffix(seed);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AffixDatabase.Invalidate();
            Debug.Log("[AffixDatabaseSeeder] Example affixes created/updated under " + AffixFolder);
        }

        private static void CreateOrUpdateAffix(AffixSeed seed)
        {
            string path = Path.Combine(AffixFolder, seed.id + ".asset");
            var affix = AssetDatabase.LoadAssetAtPath<ItemAffixDefinition>(path);

            if (affix == null)
            {
                affix = ScriptableObject.CreateInstance<ItemAffixDefinition>();
                AssetDatabase.CreateAsset(affix, path);
            }

            var so = new SerializedObject(affix);
            so.FindProperty("_affixId").stringValue = seed.id;
            so.FindProperty("_nameLocalizationKey").stringValue = "affix." + seed.id;
            so.FindProperty("_fallbackDisplayText").stringValue = seed.fallbackText;
            so.FindProperty("_slotKind").enumValueIndex = (int)seed.slotKind;
            so.FindProperty("_affectedStat").enumValueIndex = (int)seed.stat;
            so.FindProperty("_application").enumValueIndex = (int)seed.application;
            so.FindProperty("_minValue").floatValue = seed.min;
            so.FindProperty("_maxValue").floatValue = seed.max;
            so.FindProperty("_rarityWeight").intValue = seed.weight;
            so.FindProperty("_requiredEquipSlot").enumValueIndex = (int)seed.requiredSlot;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(affix);
        }

        private static List<AffixSeed> BuildSeeds()
        {
            return new List<AffixSeed>
            {
                // Weapon prefixes (MainHand) — flat attack damage, common to rare tiers.
                new AffixSeed { id = "affix_sharp", fallbackText = "Afiada", slotKind = AffixSlotKind.Prefix,
                    stat = StatType.AttackDamage, application = ModifierApplication.Flat, min = 1f, max = 3f,
                    weight = 100, requiredSlot = EquipmentSlot.MainHand },
                new AffixSeed { id = "affix_brutal", fallbackText = "Brutal", slotKind = AffixSlotKind.Prefix,
                    stat = StatType.AttackDamage, application = ModifierApplication.Flat, min = 3f, max = 6f,
                    weight = 40, requiredSlot = EquipmentSlot.MainHand },
                new AffixSeed { id = "affix_devastating", fallbackText = "Devastadora", slotKind = AffixSlotKind.Prefix,
                    stat = StatType.AttackDamage, application = ModifierApplication.PercentAdditive, min = 0.1f, max = 0.2f,
                    weight = 12, requiredSlot = EquipmentSlot.MainHand },
                new AffixSeed { id = "affix_of_fury", fallbackText = "da Fúria", slotKind = AffixSlotKind.Suffix,
                    stat = StatType.CriticalChance, application = ModifierApplication.PercentAdditive, min = 0.02f, max = 0.05f,
                    weight = 50, requiredSlot = EquipmentSlot.MainHand },
                new AffixSeed { id = "affix_of_the_executioner", fallbackText = "do Executor", slotKind = AffixSlotKind.Suffix,
                    stat = StatType.CriticalDamage, application = ModifierApplication.PercentAdditive, min = 0.1f, max = 0.25f,
                    weight = 15, requiredSlot = EquipmentSlot.MainHand },
                // Weapon attack speed (StatType.AttackSpeed, fraction: 0.1 = swings 10% faster, see PlayerBasicAttack.ResolveCooldown).
                new AffixSeed { id = "affix_nimble", fallbackText = "Ágil", slotKind = AffixSlotKind.Prefix,
                    stat = StatType.AttackSpeed, application = ModifierApplication.PercentAdditive, min = 0.06f, max = 0.12f,
                    weight = 45, requiredSlot = EquipmentSlot.MainHand },
                new AffixSeed { id = "affix_of_frenzy", fallbackText = "do Frenesi", slotKind = AffixSlotKind.Suffix,
                    stat = StatType.AttackSpeed, application = ModifierApplication.PercentAdditive, min = 0.13f, max = 0.2f,
                    weight = 12, requiredSlot = EquipmentSlot.MainHand },

                // Armor prefixes (Chest/Legs/Head/Boots) — health and defense.
                new AffixSeed { id = "affix_sturdy", fallbackText = "Resistente", slotKind = AffixSlotKind.Prefix,
                    stat = StatType.Defense, application = ModifierApplication.Flat, min = 2f, max = 5f,
                    weight = 100, requiredSlot = EquipmentSlot.Chest },
                new AffixSeed { id = "affix_reinforced", fallbackText = "Reforçada", slotKind = AffixSlotKind.Prefix,
                    stat = StatType.Defense, application = ModifierApplication.Flat, min = 6f, max = 10f,
                    weight = 40, requiredSlot = EquipmentSlot.Chest },
                new AffixSeed { id = "affix_vital", fallbackText = "Vital", slotKind = AffixSlotKind.Prefix,
                    stat = StatType.MaxHealth, application = ModifierApplication.Flat, min = 10f, max = 25f,
                    weight = 60, requiredSlot = EquipmentSlot.Chest },
                new AffixSeed { id = "affix_of_vigor", fallbackText = "do Vigor", slotKind = AffixSlotKind.Suffix,
                    stat = StatType.HealthRegen, application = ModifierApplication.Flat, min = 0.5f, max = 1.5f,
                    weight = 50, requiredSlot = EquipmentSlot.Chest },
                // Armor thorns (StatType.Thorns, fraction of melee damage taken reflected, see ThornsRules).
                new AffixSeed { id = "affix_thorned", fallbackText = "Espinhosa", slotKind = AffixSlotKind.Prefix,
                    stat = StatType.Thorns, application = ModifierApplication.PercentAdditive, min = 0.05f, max = 0.1f,
                    weight = 40, requiredSlot = EquipmentSlot.Chest },
                new AffixSeed { id = "affix_of_the_mandacaru", fallbackText = "do Saxaul", slotKind = AffixSlotKind.Suffix,
                    stat = StatType.Thorns, application = ModifierApplication.PercentAdditive, min = 0.11f, max = 0.18f,
                    weight = 12, requiredSlot = EquipmentSlot.Chest },

                // Boots — move speed.
                new AffixSeed { id = "affix_swift", fallbackText = "Ligeiras", slotKind = AffixSlotKind.Prefix,
                    stat = StatType.MoveSpeed, application = ModifierApplication.PercentAdditive, min = 0.05f, max = 0.1f,
                    weight = 60, requiredSlot = EquipmentSlot.Boots },
                new AffixSeed { id = "affix_of_the_wind", fallbackText = "do Vento", slotKind = AffixSlotKind.Suffix,
                    stat = StatType.MoveSpeed, application = ModifierApplication.PercentAdditive, min = 0.11f, max = 0.18f,
                    weight = 15, requiredSlot = EquipmentSlot.Boots },

                // Accessory — mana/cooldown/cost, unrestricted (also allowed on other slots since None means "any item").
                new AffixSeed { id = "affix_arcane", fallbackText = "Arcana", slotKind = AffixSlotKind.Prefix,
                    stat = StatType.MaxMana, application = ModifierApplication.Flat, min = 8f, max = 20f,
                    weight = 60, requiredSlot = EquipmentSlot.Accessory },
                new AffixSeed { id = "affix_of_haste", fallbackText = "da Presteza", slotKind = AffixSlotKind.Suffix,
                    stat = StatType.CooldownReduction, application = ModifierApplication.PercentAdditive, min = 0.03f, max = 0.08f,
                    weight = 35, requiredSlot = EquipmentSlot.Accessory },
                new AffixSeed { id = "affix_of_the_scholar", fallbackText = "do Estudioso", slotKind = AffixSlotKind.Suffix,
                    stat = StatType.ResourceCostReduction, application = ModifierApplication.Flat, min = 0.03f, max = 0.07f,
                    weight = 35, requiredSlot = EquipmentSlot.Accessory },

                // Universal (no slot restriction) — a couple of broadly-rollable affixes so items
                // without a strong slot identity (e.g. Tool) still have something to draw from.
                new AffixSeed { id = "affix_of_stamina", fallbackText = "da Resistência", slotKind = AffixSlotKind.Suffix,
                    stat = StatType.MaxStamina, application = ModifierApplication.Flat, min = 8f, max = 18f,
                    weight = 70, requiredSlot = EquipmentSlot.None },
                new AffixSeed { id = "affix_lucky", fallbackText = "Sortuda", slotKind = AffixSlotKind.Prefix,
                    stat = StatType.CriticalChance, application = ModifierApplication.PercentAdditive, min = 0.01f, max = 0.03f,
                    weight = 70, requiredSlot = EquipmentSlot.None },
            };
        }
    }
}
