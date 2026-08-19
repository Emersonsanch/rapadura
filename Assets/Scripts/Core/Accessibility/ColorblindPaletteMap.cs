using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Items;
using UnityEngine;

namespace Rapadura.Core.Accessibility
{
    /// <summary>
    /// Static data mapping reference UI colors (item rarity tiers, HUD status colors) to
    /// colorblind-safe variants for each <see cref="ColorblindMode"/>. Plain static data rather
    /// than a ScriptableObject: this project has never been opened in the Unity Editor (see
    /// TODO.md — no Library/ProjectSettings.asset yet), so no .asset instance can safely be
    /// hand-authored; a ScriptableObject class with no asset behind it is only good for code
    /// defaults anyway. If/when the project is opened in the Editor, this can be re-exposed as a
    /// ScriptableObject asset (e.g. wrap <see cref="RarityPalette"/> lookups behind a
    /// [CreateAssetMenu] class) without changing the call-site API below.
    ///
    /// Palette choices follow the standard colorblind-safe approach recommended by the Game
    /// Accessibility Guidelines / Filament Games research: keep hue shifts large enough to stay
    /// distinguishable under each deficiency rather than relying on subtle gradients, and never
    /// make color the *only* differentiator (rarity/status should also differ in icon/shape/text
    /// where possible — that is a UI-layer concern outside this data class).
    /// </summary>
    public static class ColorblindPaletteMap
    {
        /// <summary>Reference (non-colorblind) colors for each item rarity tier, matching typical RPG UI conventions.</summary>
        private static readonly Dictionary<ItemRarity, Color> ReferenceRarityColors = new Dictionary<ItemRarity, Color>
        {
            { ItemRarity.Common, new Color(0.78f, 0.78f, 0.78f) },      // grey
            { ItemRarity.Uncommon, new Color(0.20f, 0.80f, 0.25f) },    // green
            { ItemRarity.Rare, new Color(0.20f, 0.45f, 0.95f) },        // blue
            { ItemRarity.Epic, new Color(0.65f, 0.25f, 0.90f) },        // purple
            { ItemRarity.Legendary, new Color(0.95f, 0.60f, 0.10f) },   // orange
        };

        /// <summary>Protanopia/Deuteranopia-safe variants: shift green/red-adjacent hues toward blue/yellow, which both remain distinguishable.</summary>
        private static readonly Dictionary<ItemRarity, Color> RedGreenSafeRarityColors = new Dictionary<ItemRarity, Color>
        {
            { ItemRarity.Common, new Color(0.78f, 0.78f, 0.78f) },      // grey unaffected
            { ItemRarity.Uncommon, new Color(0.20f, 0.55f, 0.95f) },    // was green -> blue
            { ItemRarity.Rare, new Color(0.35f, 0.75f, 0.95f) },        // lighter cyan-blue, distinct from Uncommon
            { ItemRarity.Epic, new Color(0.85f, 0.55f, 0.95f) },        // lighter magenta/pink (purple reads muddy for protan/deutan)
            { ItemRarity.Legendary, new Color(0.98f, 0.85f, 0.15f) },   // orange -> saturated yellow
        };

        /// <summary>Tritanopia-safe variants: shift blue/yellow-adjacent hues toward red/green/cyan, which remain distinguishable for this deficiency.</summary>
        private static readonly Dictionary<ItemRarity, Color> BlueYellowSafeRarityColors = new Dictionary<ItemRarity, Color>
        {
            { ItemRarity.Common, new Color(0.78f, 0.78f, 0.78f) },
            { ItemRarity.Uncommon, new Color(0.20f, 0.80f, 0.25f) },    // green stays fine for tritanopia
            { ItemRarity.Rare, new Color(0.90f, 0.20f, 0.30f) },        // blue -> red (blue/yellow confusable for tritan)
            { ItemRarity.Epic, new Color(0.95f, 0.35f, 0.55f) },        // pink, distinct from Rare's red
            { ItemRarity.Legendary, new Color(0.15f, 0.70f, 0.70f) },   // orange -> teal
        };

        /// <summary>Returns the rarity color adjusted for the given colorblind mode (identity mapping when mode is None).</summary>
        public static Color GetRarityColor(ItemRarity rarity, ColorblindMode mode)
        {
            Dictionary<ItemRarity, Color> table = GetRarityTable(mode);
            return table.TryGetValue(rarity, out Color color) ? color : Color.white;
        }

        private static Dictionary<ItemRarity, Color> GetRarityTable(ColorblindMode mode)
        {
            switch (mode)
            {
                case ColorblindMode.Protanopia:
                case ColorblindMode.Deuteranopia:
                    return RedGreenSafeRarityColors;
                case ColorblindMode.Tritanopia:
                    return BlueYellowSafeRarityColors;
                case ColorblindMode.None:
                default:
                    return ReferenceRarityColors;
            }
        }

        /// <summary>All rarities this map has an entry for — useful for tests/UI iteration.</summary>
        public static IReadOnlyCollection<ItemRarity> KnownRarities => ReferenceRarityColors.Keys;
    }
}
