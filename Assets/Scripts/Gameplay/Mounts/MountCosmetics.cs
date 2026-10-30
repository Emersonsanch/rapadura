using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>Where a cosmetic goes (§6). M2 has saddles and blankets; harness, bell and trail come later.</summary>
    public enum MountCosmeticSlot
    {
        Saddle = 0,
        Blanket = 1,
    }

    /// <summary>One saddle or blanket: appearance only (a palette colour on the model), no attribute.</summary>
    public sealed class MountCosmeticSpec
    {
        public readonly string CosmeticId;
        public readonly MountCosmeticSlot Slot;
        public readonly EnemySwatch Swatch;
        public readonly int Price;
        public readonly ItemRarity Rarity;

        public MountCosmeticSpec(string cosmeticId, MountCosmeticSlot slot, EnemySwatch swatch, int price, ItemRarity rarity)
        {
            CosmeticId = cosmeticId;
            Slot = slot;
            Swatch = swatch;
            Price = price;
            Rarity = rarity;
        }

        public string NameKey => "mount.cosmetic." + CosmeticId + ".name";
    }

    /// <summary>
    /// The first saddles and blankets (Docs/Mounts.md §6, phase M2), sold by the "Selaria do Haras" (the stable panel
    /// while the cousins are in the Vila) for gold. They fit every mount in M2 (the size rule of §6 waits for the big
    /// mounts of M3). An owned piece can be put on any mount; the colour is baked into that mount's model.
    /// </summary>
    public static class MountCosmetics
    {
        public static readonly IReadOnlyList<MountCosmeticSpec> All = new List<MountCosmeticSpec>
        {
            new MountCosmeticSpec("saddle_tropeiro", MountCosmeticSlot.Saddle, EnemySwatch.Leather, 20, ItemRarity.Common),
            new MountCosmeticSpec("saddle_chita", MountCosmeticSlot.Saddle, EnemySwatch.WarCape, 30, ItemRarity.Common),
            new MountCosmeticSpec("saddle_vaqueiro_estrelas", MountCosmeticSlot.Saddle, EnemySwatch.Gold, 150, ItemRarity.Rare),
            new MountCosmeticSpec("blanket_chita_florida", MountCosmeticSlot.Blanket, EnemySwatch.WarCape, 25, ItemRarity.Common),
            new MountCosmeticSpec("blanket_xadrez_quadrilha", MountCosmeticSlot.Blanket, EnemySwatch.CaveMoss, 25, ItemRarity.Common),
            new MountCosmeticSpec("blanket_la_pico", MountCosmeticSlot.Blanket, EnemySwatch.GolemSnow, 120, ItemRarity.Rare),
        };

        public static MountCosmeticSpec Get(string cosmeticId)
        {
            return string.IsNullOrEmpty(cosmeticId) ? null : All.FirstOrDefault(c => c.CosmeticId == cosmeticId);
        }

        public static IEnumerable<MountCosmeticSpec> InSlot(MountCosmeticSlot slot) => All.Where(c => c.Slot == slot);
    }
}
