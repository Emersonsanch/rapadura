using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// One mount of its own for each of the Regions 5–9 (Docs/Mapas100.md §6, Docs/Mounts.md §3.10): the Costa (map 25), the
    /// Delta (33), the Teto do Mundo (36), Mingcheng (42) and the Nartes (47), each exactly on its rarity's budget (§2.4), merged
    /// into <see cref="MountCatalog.Specs"/> after <see cref="MountFarRegionCatalog"/>. Same rules as the Regions 10–16: the
    /// Uncommon ones are whistles sold in the region's city (<see cref="ShopWhistles"/>, §5.1); the Rare and Epic ones are never
    /// sold and come from the second city quest of the region (<see cref="MountSourceHooks"/>). The Dhow-Tartaruga and the
    /// Sapo-Gigante are <see cref="MountType.Aquatico"/>: they answer the boat and mud gates of the maps and ride through the
    /// reef tide and the reed bog dry.
    /// </summary>
    public static class MountMidRegionCatalog
    {
        public const string DhowTurtle = "mount_dhow_turtle";
        public const string TarchonFrog = "mount_tarchon_frog";
        public const string RoofYak = "mount_roof_yak";
        public const string WhiteLightStag = "mount_white_light_stag";
        public const string GoldenSteppeHorse = "mount_golden_steppe_horse";

        /// <summary>The Uncommon ones (§5.1): sold for gold in the Palafitas de Tarchon (Sapo) and Sangkent (Iaque).</summary>
        public static readonly IReadOnlyList<string> ShopWhistles = new[]
        {
            MountWaveCatalog.WhistleOf(TarchonFrog), MountWaveCatalog.WhistleOf(RoofYak),
        };

        public static readonly IReadOnlyList<MountSpec> Specs = new List<MountSpec>
        {
            // ---------------- Incomuns (loja da cidade da região) ----------------
            // Região 6, Delta. Budget 12: 1.50 (4) + 2% (2) + 150 health (2) + 140 stamina (4). Amphibian: floats over the bog.
            new MountSpec
            {
                MountId = TarchonFrog, Type = MountType.Aquatico, Rarity = ItemRarity.Uncommon, Region = BiomeType.Forest,
                Speed = 1.50f, Defense = 0.02f, Health = 150f, Stamina = 140f, Diet = MountDiet.Carnivora,
                WhistleItemId = MountWaveCatalog.WhistleOf(TarchonFrog), ShopPrice = 145, Recipe = MountVisualRecipe.TarchonFrog, SeatHeight = 0.75f,
                AbilityId = "salto_do_charco", AbilityKind = MountAbilityKind.BogLeap, Cooldown = 16f, Duration = 0.6f, Distance = 10f,
                Radius = 4f, Magnitude = 0.3f, BossMultiplier = 0.5f,
            },
            // Região 7, Teto do Mundo. Budget 12: 1.30 (0) + 5% (5) + 200 health (4) + 130 stamina (3). Slow, warm and a great carrier.
            new MountSpec
            {
                MountId = RoofYak, Rarity = ItemRarity.Uncommon, Size = MountSize.Grande, Region = BiomeType.Mountain,
                Speed = 1.30f, Defense = 0.05f, Health = 200f, Stamina = 130f, Diet = MountDiet.Herbivora,
                WhistleItemId = MountWaveCatalog.WhistleOf(RoofYak), ShopPrice = 150, Recipe = MountVisualRecipe.RoofYak, SeatHeight = 1.2f,
                AbilityId = "la_do_teto", AbilityKind = MountAbilityKind.WarmFleece, Cooldown = 35f, Duration = 8f, Radius = 6f, Magnitude = 0.15f,
            },

            // ---------------- Rara (missão da cidade da região) ----------------
            // Região 9, Nartes. Budget 16: 1.75 (9) + 1% (1) + 125 health (1) + 150 stamina (5). A runner, nothing else.
            new MountSpec
            {
                MountId = GoldenSteppeHorse, Rarity = ItemRarity.Rare, Region = BiomeType.Mountain,
                Speed = 1.75f, Defense = 0.01f, Health = 125f, Stamina = 150f, Diet = MountDiet.Herbivora, Favorite = CharacterId.Maria,
                WhistleItemId = MountWaveCatalog.WhistleOf(GoldenSteppeHorse), Recipe = MountVisualRecipe.GoldenSteppeHorse, SeatHeight = 1.1f,
                AbilityId = "folego_de_soslan", AbilityKind = MountAbilityKind.SecondWind, Cooldown = 45f, Radius = 8f, Magnitude = 0.3f,
            },

            // ---------------- Épicas (missão da cidade da região) ----------------
            // Região 5, Costa. Budget 21: 1.45 (3) + 8% (8) + 250 health (6) + 140 stamina (4). A green turtle with a dhow's sail.
            new MountSpec
            {
                MountId = DhowTurtle, Type = MountType.Aquatico, Rarity = ItemRarity.Epic, Size = MountSize.Grande, Region = BiomeType.Desert,
                Speed = 1.45f, Defense = 0.08f, Health = 250f, Stamina = 140f, Diet = MountDiet.Herbivora, Favorite = CharacterId.Joaquim,
                WhistleItemId = MountWaveCatalog.WhistleOf(DhowTurtle), Recipe = MountVisualRecipe.DhowTurtle, SeatHeight = 0.95f,
                AbilityId = "vela_da_moncao", AbilityKind = MountAbilityKind.MonsoonSail, Cooldown = 40f, Duration = 8f, Radius = 6f,
                BossMultiplier = 0f,
            },
            // Região 8, Mingcheng. Budget 21: 1.70 (8) + 4% (4) + 175 health (3) + 160 stamina (6).
            new MountSpec
            {
                MountId = WhiteLightStag, Rarity = ItemRarity.Epic, Region = BiomeType.Caves,
                Speed = 1.70f, Defense = 0.04f, Health = 175f, Stamina = 160f, Diet = MountDiet.Encantada, Favorite = CharacterId.Lavine,
                WhistleItemId = MountWaveCatalog.WhistleOf(WhiteLightStag), Recipe = MountVisualRecipe.WhiteLightStag, SeatHeight = 1.15f,
                AbilityId = "rastro_de_luz_branca", AbilityKind = MountAbilityKind.LightTrail, Cooldown = 40f, Duration = 8f, Radius = 1.6f,
                Magnitude = 0.02f,
            },
        };
    }
}
