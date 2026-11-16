using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// Six more mounts (Docs/Mounts.md §3.13): four <b>boss mounts</b>, each one the creature a boss held, freed when the boss falls
    /// and following the cousins as a rare drop of that boss (<see cref="MountDropRules"/>, with the Fitas do Laço as bad-luck
    /// protection), and the last two map-sheet proposals of Docs/Mapas100.md §6 for the Regions 1 and 6. The boss ones: the
    /// Argali do Pamir (Aydahar, world boss of Region 7, map 40), the Cavalo de Fumaça Liberto (O Cavalo de Fumaça, map boss 47:
    /// "libertado, vira cavalo de verdade"), the Ninki Nanka Mansa (Ninki Nanka, map boss 34: "libertada") and the Javali de Brasa
    /// Viva (the Glutão de Brasa, world boss of Region 12, map 70). The map-sheet ones: the Lagarto-do-Louro (05) and the
    /// Búfalo-d'Água de Jhilpur (31), Uncommon whistles sold in their region's city (<see cref="ShopWhistles"/>, §5.1). Each exactly
    /// on its rarity's budget (§2.4), merged into <see cref="MountCatalog.Specs"/> after <see cref="MountSheetCatalog"/>. The buffalo
    /// and the Ninki Nanka are <see cref="MountType.Aquatico"/>, like the frog of the same delta; nobody flies.
    /// </summary>
    public static class MountBossCatalog
    {
        public const string LaurelLizard = "mount_laurel_lizard";
        public const string JhilpurBuffalo = "mount_jhilpur_buffalo";
        public const string PamirArgali = "mount_pamir_argali";
        public const string FreedSmokeHorse = "mount_freed_smoke_horse";
        public const string TameNinkiNanka = "mount_ninki_nanka";
        public const string LivingEmberBoar = "mount_living_ember_boar";

        /// <summary>The Uncommon ones (§5.1): the lizard at the Ervaria da Dácil (Garajonay), the buffalo at the Trocas do Nuhzadag (Jhilpur).</summary>
        public static readonly IReadOnlyList<string> ShopWhistles = new[] { MountWaveCatalog.WhistleOf(LaurelLizard), MountWaveCatalog.WhistleOf(JhilpurBuffalo) };

        /// <summary>Which boss frees which mount (enemy id, mount id), in <see cref="Specs"/> order; the drop rows live in <see cref="MountDropRules.Table"/>.</summary>
        public static readonly IReadOnlyList<(string BossId, string MountId)> BossSources = new[]
        {
            (MountDropRules.Aydahar, PamirArgali),
            (MountDropRules.SmokeStallion, FreedSmokeHorse),
            (MountDropRules.NinkiNanka, TameNinkiNanka),
            (MountDropRules.EmberGlutton, LivingEmberBoar),
        };

        public static readonly IReadOnlyList<MountSpec> Specs = new List<MountSpec>
        {
            // ---------------- Incomuns (loja da cidade da região) ----------------
            // Região 1, Bosque do Zarafshan (Copas de Garajonay, 05). Budget 12: 1.50 (4) + 2% (2) + 125 health (1) + 150 stamina (5).
            // A giant laurel-forest lizard: low, quick, good breath, frail.
            new MountSpec
            {
                MountId = LaurelLizard, Rarity = ItemRarity.Uncommon, Size = MountSize.Pequeno, Region = BiomeType.Forest,
                Speed = 1.50f, Defense = 0.02f, Health = 125f, Stamina = 150f, Diet = MountDiet.Herbivora,
                WhistleItemId = MountWaveCatalog.WhistleOf(LaurelLizard), ShopPrice = 145, Recipe = MountVisualRecipe.LaurelLizard, SeatHeight = 0.72f,
                AbilityId = "rabo_solto", AbilityKind = MountAbilityKind.TailDecoy, Cooldown = 25f, Duration = 3f, Distance = 3f, Radius = 8f,
                BossMultiplier = 0f,
            },
            // Região 6, Delta de Jhilpur (Jhilpur, 31). Budget 12: 1.35 (1) + 5% (5) + 200 health (4) + 120 stamina (2).
            // The paddy buffalo of the delta: slow, sturdy, at home in mud and water.
            new MountSpec
            {
                MountId = JhilpurBuffalo, Type = MountType.Aquatico, Rarity = ItemRarity.Uncommon, Size = MountSize.Grande, Region = BiomeType.Forest,
                Speed = 1.35f, Defense = 0.05f, Health = 200f, Stamina = 120f, Diet = MountDiet.Herbivora, Favorite = CharacterId.Joaquim,
                WhistleItemId = MountWaveCatalog.WhistleOf(JhilpurBuffalo), ShopPrice = 145, Recipe = MountVisualRecipe.JhilpurBuffalo, SeatHeight = 1.1f,
                AbilityId = "banho_de_lama", AbilityKind = MountAbilityKind.MudWallow, Cooldown = 35f, Duration = 8f, Radius = 6f, Magnitude = 0.15f,
            },

            // ---------------- Raras (drop de chefe, 2%; 25 Fitas do Laço) ----------------
            // Região 7, Teto do Mundo (Passo de Wakhan, 40; Aydahar). Budget 16: 1.55 (5) + 4% (4) + 150 health (2) + 150 stamina (5).
            // The wild sheep of the high ledges: sure-footed and watchful.
            new MountSpec
            {
                MountId = PamirArgali, Rarity = ItemRarity.Rare, Region = BiomeType.Mountain,
                Speed = 1.55f, Defense = 0.04f, Health = 150f, Stamina = 150f, Diet = MountDiet.Herbivora,
                WhistleItemId = MountWaveCatalog.WhistleOf(PamirArgali), Recipe = MountVisualRecipe.PamirArgali, SeatHeight = 1.0f,
                AbilityId = "vigia_da_escarpa", AbilityKind = MountAbilityKind.LedgeWatch, Cooldown = 35f, Duration = 8f, Radius = 8f, Magnitude = 0.10f,
            },
            // Região 9, Estepe dos Nartes (Acampamento de Soslan, 47; O Cavalo de Fumaça). Budget 16: 1.70 (8) + 2% (2) + 125 health (1)
            // + 150 stamina (5). The shadow stallion freed into a real horse: fast, light, still trailing a little smoke.
            new MountSpec
            {
                MountId = FreedSmokeHorse, Rarity = ItemRarity.Rare, Region = BiomeType.Mountain,
                Speed = 1.70f, Defense = 0.02f, Health = 125f, Stamina = 150f, Diet = MountDiet.Herbivora, Favorite = CharacterId.Maria,
                WhistleItemId = MountWaveCatalog.WhistleOf(FreedSmokeHorse), Recipe = MountVisualRecipe.FreedSmokeHorse, SeatHeight = 1.1f,
                AbilityId = "nuvem_de_fumaca", AbilityKind = MountAbilityKind.SmokeCloud, Cooldown = 35f, Duration = 5f, Radius = 5f, BossMultiplier = 0f,
            },

            // ---------------- Épicas (drop de chefe, 0,8%; 80 Fitas do Laço) ----------------
            // Região 6, Delta de Jhilpur (Charco do Ninki Nanka, 34; Ninki Nanka). Budget 21: 1.55 (5) + 6% (6) + 225 health (5)
            // + 150 stamina (5). The shy dragoness of the bog, freed: long, scaled, at home under the water.
            new MountSpec
            {
                MountId = TameNinkiNanka, Type = MountType.Aquatico, Rarity = ItemRarity.Epic, Size = MountSize.Grande, Region = BiomeType.Forest,
                Speed = 1.55f, Defense = 0.06f, Health = 225f, Stamina = 150f, Diet = MountDiet.Carnivora, Favorite = CharacterId.Lavine,
                WhistleItemId = MountWaveCatalog.WhistleOf(TameNinkiNanka), Recipe = MountVisualRecipe.TameNinkiNanka, SeatHeight = 0.95f,
                AbilityId = "mergulho_timido", AbilityKind = MountAbilityKind.ShyDive, Cooldown = 45f, Duration = 3f, Radius = 12f,
                Magnitude = 0.4f, BossMultiplier = 0.5f,
            },
            // Região 12, Hinokura (Caldeira do Fogo Devorador, 70; o Glutão de Brasa). Budget 21: 1.50 (4) + 9% (9) + 225 health (5)
            // + 130 stamina (3). The forge boar the Glutão swallowed with the living fire: heavy, armoured, warm.
            new MountSpec
            {
                MountId = LivingEmberBoar, Rarity = ItemRarity.Epic, Region = BiomeType.Mountain, IsWarMount = true,
                Speed = 1.50f, Defense = 0.09f, Health = 225f, Stamina = 130f, Diet = MountDiet.Herbivora, Favorite = CharacterId.Icaro,
                WhistleItemId = MountWaveCatalog.WhistleOf(LivingEmberBoar), Recipe = MountVisualRecipe.LivingEmberBoar, SeatHeight = 0.9f,
                AbilityId = "forja_acesa", AbilityKind = MountAbilityKind.EmberForge, Cooldown = 50f, Duration = 10f, Radius = 8f, Magnitude = 0.3f,
            },
        };
    }
}
