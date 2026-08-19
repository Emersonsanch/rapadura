using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rapadura.Gameplay.Items.Procedural
{
    /// <summary>
    /// Rolls a random set of affixes onto a base <see cref="ItemDefinition"/> to produce a
    /// <see cref="GeneratedItemInstance"/>, following the standard ARPG (Diablo/Path of
    /// Exile-style) procedural item model:
    /// <list type="bullet">
    /// <item>The base item (id, icon, weapon/armor stats, equip slot) never changes — only the
    /// affix roll varies.</item>
    /// <item>Higher rarity rolls more affixes (see <see cref="AffixCountRange"/>).</item>
    /// <item>Affixes are drawn without replacement from the pool of affixes compatible with the
    /// item's equip slot, weighted by <see cref="ItemAffixDefinition.RarityWeight"/> (lower
    /// weight = rarer/stronger, drawn less often).</item>
    /// <item>Each drawn affix rolls a concrete value uniformly within its [min, max] range.</item>
    /// <item>The final display name combines prefix affixes + base name + suffix affixes, e.g.
    /// "Espada Flamejante da Fúria".</item>
    /// </list>
    /// An optional seed makes generation fully deterministic (used by EditMode tests and for
    /// reproducible loot debugging); without one, generation uses a randomly-seeded RNG.
    /// </summary>
    public static class ProceduralItemGenerator
    {
        /// <summary>Inclusive [min, max] number of affixes rolled for a given rarity.</summary>
        public static readonly Dictionary<ItemRarity, (int min, int max)> AffixCountRange = new Dictionary<ItemRarity, (int min, int max)>
        {
            { ItemRarity.Common, (0, 0) },
            { ItemRarity.Uncommon, (0, 1) },
            { ItemRarity.Rare, (1, 2) },
            { ItemRarity.Epic, (2, 3) },
            { ItemRarity.Legendary, (3, 4) },
        };

        /// <summary>
        /// Generates a procedural item instance.
        /// </summary>
        /// <param name="baseItem">The base item definition to roll affixes onto.</param>
        /// <param name="rarity">Desired rarity — drives how many affixes are rolled.</param>
        /// <param name="seed">Optional seed for a deterministic roll (tests/tools). Omit for a random roll.</param>
        /// <param name="localizeName">
        /// Optional resolver for an affix's display text (e.g. <c>LocalizationManager.Get</c>). When
        /// omitted, <see cref="ItemAffixDefinition.FallbackDisplayText"/> is used directly, which
        /// keeps this generator usable without a live <c>LocalizationManager</c> (EditMode tests,
        /// editor tools).
        /// </param>
        /// <param name="affixPool">
        /// Optional explicit affix pool (mainly for tests). Defaults to
        /// <see cref="AffixDatabase.GetCompatibleWith"/> for the base item's equip slot.
        /// </param>
        public static GeneratedItemInstance Generate(
            ItemDefinition baseItem,
            ItemRarity rarity,
            int? seed = null,
            Func<string, string> localizeName = null,
            IEnumerable<ItemAffixDefinition> affixPool = null)
        {
            if (baseItem == null)
            {
                throw new ArgumentNullException(nameof(baseItem));
            }

            Random random = seed.HasValue ? new Random(seed.Value) : new Random();

            List<ItemAffixDefinition> pool = (affixPool ?? AffixDatabase.GetCompatibleWith(baseItem.EquipSlot))
                .Where(a => a != null)
                .ToList();

            int affixCount = RollAffixCount(rarity, random);
            List<RolledAffix> rolled = RollAffixes(pool, affixCount, random);

            string name = BuildName(baseItem, rolled, localizeName);

            return new GeneratedItemInstance(baseItem, rarity, rolled, name);
        }

        private static int RollAffixCount(ItemRarity rarity, Random random)
        {
            if (!AffixCountRange.TryGetValue(rarity, out (int min, int max) range))
            {
                return 0;
            }

            return range.min == range.max ? range.min : random.Next(range.min, range.max + 1);
        }

        private static List<RolledAffix> RollAffixes(List<ItemAffixDefinition> pool, int count, Random random)
        {
            var result = new List<RolledAffix>(count);
            if (count <= 0 || pool.Count == 0)
            {
                return result;
            }

            List<ItemAffixDefinition> remaining = new List<ItemAffixDefinition>(pool);
            int drawCount = Math.Min(count, remaining.Count);

            for (int i = 0; i < drawCount; i++)
            {
                ItemAffixDefinition picked = DrawWeighted(remaining, random);
                remaining.Remove(picked);

                float value = RollValue(picked, random);
                result.Add(new RolledAffix(picked, value));
            }

            return result;
        }

        private static ItemAffixDefinition DrawWeighted(List<ItemAffixDefinition> candidates, Random random)
        {
            int totalWeight = candidates.Sum(a => a.RarityWeight);
            if (totalWeight <= 0)
            {
                return candidates[random.Next(candidates.Count)];
            }

            int roll = random.Next(totalWeight);
            int cumulative = 0;
            foreach (ItemAffixDefinition candidate in candidates)
            {
                cumulative += candidate.RarityWeight;
                if (roll < cumulative)
                {
                    return candidate;
                }
            }

            return candidates[candidates.Count - 1];
        }

        private static float RollValue(ItemAffixDefinition affix, Random random)
        {
            float min = affix.MinValue;
            float max = affix.MaxValue;
            if (Math.Abs(max - min) < float.Epsilon)
            {
                return min;
            }

            double t = random.NextDouble();
            return (float)(min + t * (max - min));
        }

        private static string BuildName(ItemDefinition baseItem, List<RolledAffix> rolled, Func<string, string> localizeName)
        {
            List<string> prefixes = new List<string>();
            List<string> suffixes = new List<string>();

            foreach (RolledAffix rolledAffix in rolled)
            {
                ItemAffixDefinition affix = rolledAffix.Affix;
                string text = ResolveAffixText(affix, localizeName);
                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }

                if (affix.SlotKind == AffixSlotKind.Prefix)
                {
                    prefixes.Add(text);
                }
                else
                {
                    suffixes.Add(text);
                }
            }

            var builder = new StringBuilder();
            foreach (string prefix in prefixes)
            {
                builder.Append(prefix).Append(' ');
            }

            builder.Append(baseItem.DisplayName);

            foreach (string suffix in suffixes)
            {
                builder.Append(' ').Append(suffix);
            }

            return builder.ToString();
        }

        private static string ResolveAffixText(ItemAffixDefinition affix, Func<string, string> localizeName)
        {
            if (localizeName != null && !string.IsNullOrEmpty(affix.NameLocalizationKey))
            {
                string resolved = localizeName(affix.NameLocalizationKey);
                if (!string.IsNullOrEmpty(resolved))
                {
                    return resolved;
                }
            }

            return affix.FallbackDisplayText;
        }
    }
}
