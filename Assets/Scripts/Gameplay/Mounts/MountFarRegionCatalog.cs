using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// The mounts named by the map sheets of Regions 10–16 (Docs/Mapas100.md §6, Docs/Mounts.md §3.9): one species for each of
    /// the maps 53, 59, 65, 67, 71, 77, 80 and 82, each exactly on its rarity's budget (§2.4). Merged into
    /// <see cref="MountCatalog.Specs"/> after the flyers. Uncommon ones are whistles sold in the region's city
    /// (<see cref="ShopWhistles"/>, §5.1); Rare and Epic ones are never sold and come from the region's second city quest
    /// (<see cref="MountSourceHooks"/>). Only the Íbis Real is a <see cref="MountType.Voador"/>, the only one that answers the
    /// "Asas" gates of the maps.
    /// </summary>
    public static class MountFarRegionCatalog
    {
        public const string GordiasGoat = "mount_gordias_goat";
        public const string AuroraReindeer = "mount_aurora_reindeer";
        public const string LacquerLion = "mount_lacquer_lion";
        public const string VolcanoGoat = "mount_volcano_goat";
        public const string MesektetIbis = "mount_mesektet_ibis";
        public const string Mushhushshu = "mount_mushhushshu";
        public const string TaigaElk = "mount_taiga_elk";
        public const string HillRam = "mount_hill_ram";

        /// <summary>The Uncommon ones (§5.1): sold for gold in Hinokura (Cabra) and Dur-Jarras (Alce, Carneiro).</summary>
        public static readonly IReadOnlyList<string> ShopWhistles = new[]
        {
            MountWaveCatalog.WhistleOf(VolcanoGoat), MountWaveCatalog.WhistleOf(TaigaElk), MountWaveCatalog.WhistleOf(HillRam),
        };

        public static readonly IReadOnlyList<MountSpec> Specs = new List<MountSpec>
        {
            // ---------------- Incomuns (loja da cidade da região) ----------------
            // Budget 12: 1.45 (3) + 3% (3) + 150 health (2) + 140 stamina (4).
            new MountSpec
            {
                MountId = VolcanoGoat, Rarity = ItemRarity.Uncommon, Size = MountSize.Pequeno, Region = BiomeType.Mountain,
                Speed = 1.45f, Defense = 0.03f, Health = 150f, Stamina = 140f, Diet = MountDiet.Herbivora,
                WhistleItemId = MountWaveCatalog.WhistleOf(VolcanoGoat), ShopPrice = 145, Recipe = MountVisualRecipe.VolcanoGoat, SeatHeight = 0.85f,
                AbilityId = "cascos_de_pedra_pomes", AbilityKind = MountAbilityKind.WeatherImmunity, Cooldown = 40f, Duration = 8f,
            },
            // Budget 12: 1.55 (5) + 2% (2) + 175 health (3) + 120 stamina (2).
            new MountSpec
            {
                MountId = TaigaElk, Rarity = ItemRarity.Uncommon, Region = BiomeType.Forest,
                Speed = 1.55f, Defense = 0.02f, Health = 175f, Stamina = 120f, Diet = MountDiet.Herbivora,
                WhistleItemId = MountWaveCatalog.WhistleOf(TaigaElk), ShopPrice = 150, Recipe = MountVisualRecipe.TaigaElk, SeatHeight = 1.2f,
                AbilityId = "passo_do_cedral", AbilityKind = MountAbilityKind.ThicketStride, Cooldown = 30f, Duration = 8f,
            },
            // Budget 12: 1.35 (1) + 6% (6) + 175 health (3) + 120 stamina (2).
            new MountSpec
            {
                MountId = HillRam, Rarity = ItemRarity.Uncommon, Region = BiomeType.Desert,
                Speed = 1.35f, Defense = 0.06f, Health = 175f, Stamina = 120f, Diet = MountDiet.Herbivora,
                WhistleItemId = MountWaveCatalog.WhistleOf(HillRam), ShopPrice = 150, Recipe = MountVisualRecipe.HillRam, SeatHeight = 1.1f,
                AbilityId = "cabecada_de_pedra", AbilityKind = MountAbilityKind.BoulderCharge, Cooldown = 22f, Duration = 0.45f, Distance = 9f,
                Radius = 2.5f, BossMultiplier = 0f,
            },

            // ---------------- Raras (missão da cidade da região) ----------------
            // Budget 16: 1.60 (6) + 4% (4) + 150 health (2) + 140 stamina (4).
            new MountSpec
            {
                MountId = GordiasGoat, Rarity = ItemRarity.Rare, Region = BiomeType.Forest,
                Speed = 1.60f, Defense = 0.04f, Health = 150f, Stamina = 140f, Diet = MountDiet.Herbivora, Favorite = CharacterId.Icaro,
                WhistleItemId = MountWaveCatalog.WhistleOf(GordiasGoat), Recipe = MountVisualRecipe.GordiasGoat, SeatHeight = 0.95f,
                AbilityId = "no_de_gordias", AbilityKind = MountAbilityKind.Untie, Cooldown = 18f, Distance = 14f,
            },
            // Budget 16: 1.70 (8) + 1% (1) + 100 health (0) + 170 stamina (7). The flyer of the group: long glide, thin body.
            new MountSpec
            {
                MountId = MesektetIbis, Type = MountType.Voador, Rarity = ItemRarity.Rare, Region = BiomeType.Desert,
                Speed = 1.70f, Defense = 0.01f, Health = 100f, Stamina = 170f, Diet = MountDiet.Carnivora,
                WhistleItemId = MountWaveCatalog.WhistleOf(MesektetIbis), Recipe = MountVisualRecipe.MesektetIbis, SeatHeight = 1.1f,
                AbilityId = "planar_de_mesektet", AbilityKind = MountAbilityKind.Glide, Cooldown = 20f, Duration = 1.4f, Distance = 16f,
            },

            // ---------------- Épicas (missão da cidade da região) ----------------
            // Budget 21: 1.75 (9) + 3% (3) + 175 health (3) + 160 stamina (6).
            new MountSpec
            {
                MountId = AuroraReindeer, Rarity = ItemRarity.Epic, Region = BiomeType.Mountain,
                Speed = 1.75f, Defense = 0.03f, Health = 175f, Stamina = 160f, Diet = MountDiet.Herbivora, Favorite = CharacterId.Maithe,
                WhistleItemId = MountWaveCatalog.WhistleOf(AuroraReindeer), Recipe = MountVisualRecipe.AuroraReindeer, SeatHeight = 1.15f,
                AbilityId = "trote_da_aurora", AbilityKind = MountAbilityKind.AuroraRun, Cooldown = 45f, Duration = 10f,
            },
            // Budget 21: 1.50 (4) + 9% (9) + 200 health (4) + 140 stamina (4).
            new MountSpec
            {
                MountId = LacquerLion, Rarity = ItemRarity.Epic, Region = BiomeType.Mountain,
                Speed = 1.50f, Defense = 0.09f, Health = 200f, Stamina = 140f, IsWarMount = true, Diet = MountDiet.Encantada,
                WhistleItemId = MountWaveCatalog.WhistleOf(LacquerLion), Recipe = MountVisualRecipe.LacquerLion, SeatHeight = 0.95f,
                AbilityId = "olho_da_guarda", AbilityKind = MountAbilityKind.Reveal, Cooldown = 30f, Radius = 22f,
            },
            // Budget 21: 1.60 (6) + 7% (7) + 200 health (4) + 140 stamina (4).
            new MountSpec
            {
                MountId = Mushhushshu, Rarity = ItemRarity.Epic, Region = BiomeType.Desert,
                Speed = 1.60f, Defense = 0.07f, Health = 200f, Stamina = 140f, IsWarMount = true, Diet = MountDiet.Carnivora | MountDiet.Encantada,
                WhistleItemId = MountWaveCatalog.WhistleOf(Mushhushshu), Recipe = MountVisualRecipe.Mushhushshu, SeatHeight = 1.0f,
                AbilityId = "rugido_do_portao", AbilityKind = MountAbilityKind.Dread, Cooldown = 40f, Duration = 4f, Radius = 8f, Magnitude = 0.15f,
                BossMultiplier = 0f,
            },
        };
    }
}
