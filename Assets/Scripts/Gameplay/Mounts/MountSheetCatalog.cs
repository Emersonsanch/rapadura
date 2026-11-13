using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// Six more mounts of the map sheets (Docs/Mapas100.md §6, Docs/Mounts.md §3.12): one of its own for each of the Regions 1–4
    /// (the Garça do Brejo, map 03; the Lobo de Guerra Ocre, 11; the Eland do Capim Alto, 15; the Proteu-Lanterna do Limiar,
    /// 19/22) and two more for the Regions 5 and 7 (the Auroque de Aratta, 26; the Camelo-Bactriano Branco, 40). Each exactly on
    /// its rarity's budget (§2.4), merged into <see cref="MountCatalog.Specs"/> after <see cref="MountMidRegionCatalog"/>. Same
    /// rules as §3.9/§3.10: the Uncommon one is a whistle sold in the region's city (<see cref="ShopWhistles"/>, §5.1); the Rare
    /// ones are never sold and come from the second city quest of their region (<see cref="MountSourceHooks"/>). None flies,
    /// none is aquatic (the boat and mud gates of the maps stay with the turtle and the frog).
    /// </summary>
    public static class MountSheetCatalog
    {
        public const string MarshHeron = "mount_marsh_heron";
        public const string OchreWarWolf = "mount_ochre_war_wolf";
        public const string TallGrassEland = "mount_eland";
        public const string LanternOlm = "mount_lantern_olm";
        public const string ArattaAurochs = "mount_aratta_aurochs";
        public const string WhiteBactrian = "mount_white_bactrian";

        /// <summary>The Uncommon one (§5.1): sold for gold at the Ervaria da Dácil, in the Copas de Garajonay (Region 1).</summary>
        public static readonly IReadOnlyList<string> ShopWhistles = new[] { MountWaveCatalog.WhistleOf(MarshHeron) };

        public static readonly IReadOnlyList<MountSpec> Specs = new List<MountSpec>
        {
            // ---------------- Incomum (loja da cidade da região) ----------------
            // Região 1, Bosque do Zarafshan (Brejo do Lodo Escuro, 03). Budget 12: 1.55 (5) + 1% (1) + 125 health (1) + 150 stamina (5).
            // A long-legged wader: light, quick, good breath, frail.
            new MountSpec
            {
                MountId = MarshHeron, Rarity = ItemRarity.Uncommon, Region = BiomeType.Forest,
                Speed = 1.55f, Defense = 0.01f, Health = 125f, Stamina = 150f, Diet = MountDiet.Carnivora,
                WhistleItemId = MountWaveCatalog.WhistleOf(MarshHeron), ShopPrice = 140, Recipe = MountVisualRecipe.MarshHeron, SeatHeight = 1.05f,
                AbilityId = "passo_de_pernalta", AbilityKind = MountAbilityKind.ShallowsWade, Cooldown = 30f, Duration = 10f, Radius = 6f,
            },

            // ---------------- Raras (missão da cidade da região) ----------------
            // Região 2, Areias Flamejantes (Ermos do Tambor Rubro, 11). Budget 16: 1.65 (7) + 4% (4) + 150 health (2) + 130 stamina (3).
            // The orcs' war wolf: fast and bold.
            new MountSpec
            {
                MountId = OchreWarWolf, Rarity = ItemRarity.Rare, Region = BiomeType.Desert, IsWarMount = true,
                Speed = 1.65f, Defense = 0.04f, Health = 150f, Stamina = 130f, Diet = MountDiet.Carnivora, Favorite = CharacterId.Icaro,
                WhistleItemId = MountWaveCatalog.WhistleOf(OchreWarWolf), Recipe = MountVisualRecipe.OchreWarWolf, SeatHeight = 0.95f,
                AbilityId = "uivo_do_tambor_rubro", AbilityKind = MountAbilityKind.DrumHowl, Cooldown = 35f, Duration = 2f, Radius = 8f,
                Magnitude = 0.25f, BossMultiplier = 0f,
            },
            // Região 3, Serra de Kaggen (Abrigos Pintados, 15). Budget 16: 1.60 (6) + 3% (3) + 175 health (3) + 140 stamina (4).
            // The big antelope of the painted shelters: tall, steady, a long jumper.
            new MountSpec
            {
                MountId = TallGrassEland, Rarity = ItemRarity.Rare, Size = MountSize.Grande, Region = BiomeType.Mountain,
                Speed = 1.60f, Defense = 0.03f, Health = 175f, Stamina = 140f, Diet = MountDiet.Herbivora, Favorite = CharacterId.Maithe,
                WhistleItemId = MountWaveCatalog.WhistleOf(TallGrassEland), Recipe = MountVisualRecipe.TallGrassEland, SeatHeight = 1.15f,
                AbilityId = "salto_do_capim_alto", AbilityKind = MountAbilityKind.LongBound, Cooldown = 20f, Duration = 0.7f, Distance = 14f,
                Radius = 6f, Magnitude = 2f, BossMultiplier = 0f,
            },
            // Região 4, Grutas do Limiar (Lago do Limiar 19, Mercado dos Túneis 22). Budget 16: 1.40 (2) + 5% (5) + 200 health (4)
            // + 150 stamina (5). A giant blind cave salamander: slow, tough, never out of breath.
            new MountSpec
            {
                MountId = LanternOlm, Rarity = ItemRarity.Rare, Region = BiomeType.Caves,
                Speed = 1.40f, Defense = 0.05f, Health = 200f, Stamina = 150f, Diet = MountDiet.Carnivora, Favorite = CharacterId.Lavine,
                WhistleItemId = MountWaveCatalog.WhistleOf(LanternOlm), Recipe = MountVisualRecipe.LanternOlm, SeatHeight = 0.7f,
                AbilityId = "cuspe_de_luz", AbilityKind = MountAbilityKind.GlowSpit, Cooldown = 30f, Duration = 4f, Distance = 10f,
                Radius = 3f, Magnitude = 0.25f, BossMultiplier = 0.5f,
            },
            // Região 5, Costa das Monções (Savana dos Auroques, 26). Budget 16: 1.50 (4) + 7% (7) + 200 health (4) + 110 stamina (1).
            // The aurochs of the Aratta course: heavy, armoured, short of breath.
            new MountSpec
            {
                MountId = ArattaAurochs, Rarity = ItemRarity.Rare, Size = MountSize.Grande, Region = BiomeType.Desert, IsWarMount = true,
                Speed = 1.50f, Defense = 0.07f, Health = 200f, Stamina = 110f, Diet = MountDiet.Herbivora, Favorite = CharacterId.Joaquim,
                WhistleItemId = MountWaveCatalog.WhistleOf(ArattaAurochs), Recipe = MountVisualRecipe.ArattaAurochs, SeatHeight = 1.15f,
                AbilityId = "pisao_de_aratta", AbilityKind = MountAbilityKind.AurochsStomp, Cooldown = 25f, Duration = 3f, Radius = 6f,
                Magnitude = 0.4f, BossMultiplier = 0.5f,
            },
            // Região 7, Teto do Mundo (Passo de Wakhan, 40). Budget 16: 1.40 (2) + 4% (4) + 200 health (4) + 160 stamina (6).
            // The white two-humped camel of the passes: slow, patient, the longest breath of the Rare ones on the ground.
            new MountSpec
            {
                MountId = WhiteBactrian, Rarity = ItemRarity.Rare, Size = MountSize.Grande, Region = BiomeType.Mountain,
                Speed = 1.40f, Defense = 0.04f, Health = 200f, Stamina = 160f, Diet = MountDiet.Herbivora,
                WhistleItemId = MountWaveCatalog.WhistleOf(WhiteBactrian), Recipe = MountVisualRecipe.WhiteBactrian, SeatHeight = 1.3f,
                AbilityId = "couro_de_tempestade", AbilityKind = MountAbilityKind.StormHide, Cooldown = 40f, Duration = 10f, Radius = 6f,
            },
        };
    }
}
