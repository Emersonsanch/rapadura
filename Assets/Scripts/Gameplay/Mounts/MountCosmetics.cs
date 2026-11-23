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

        /// <summary>Fitas de Corrida it costs at Shulgi's post in Aratta (ShulgiRaceRules); 0 for the Selaria's gold pieces.</summary>
        public readonly int RaceRibbons;

        public MountCosmeticSpec(string cosmeticId, MountCosmeticSlot slot, EnemySwatch swatch, int price, ItemRarity rarity, int raceRibbons = 0)
        {
            CosmeticId = cosmeticId;
            Slot = slot;
            Swatch = swatch;
            Price = price;
            Rarity = rarity;
            RaceRibbons = raceRibbons;
        }

        /// <summary>Sold for gold at the Selaria do Haras (the race pieces are only traded for Fitas de Corrida).</summary>
        public bool SoldForGold => RaceRibbons <= 0;

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

            // The Corrida de Shulgi (Aratta, map 26): traded only for Fitas de Corrida at Shulgi's post (ShulgiRaceRules.Offers).
            new MountCosmeticSpec("blanket_fitas_de_aratta", MountCosmeticSlot.Blanket, EnemySwatch.Crystal, 0, ItemRarity.Uncommon, raceRibbons: 6),
            new MountCosmeticSpec("saddle_junco_de_aratta", MountCosmeticSlot.Saddle, EnemySwatch.Sandstone, 0, ItemRarity.Rare, raceRibbons: 10),
        };

        /// <summary>The pieces the Selaria sells for gold.</summary>
        public static IEnumerable<MountCosmeticSpec> ForGold => All.Where(c => c.SoldForGold);

        public static MountCosmeticSpec Get(string cosmeticId)
        {
            return string.IsNullOrEmpty(cosmeticId) ? null : All.FirstOrDefault(c => c.CosmeticId == cosmeticId);
        }

        public static IEnumerable<MountCosmeticSpec> InSlot(MountCosmeticSlot slot) => All.Where(c => c.Slot == slot);
    }
}
