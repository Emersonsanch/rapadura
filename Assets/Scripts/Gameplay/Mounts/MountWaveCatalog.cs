using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// Phase M3, wave 1 (Docs/Mounts.md §13 M3.1): the 18 walkers that need no new physics, with the attributes of §3
    /// (each one exactly on its rarity's §2.4 budget, EditMode-checked) and their ability parameters. Merged into
    /// <see cref="MountCatalog.Specs"/> after the three starters. The Capivara is <see cref="MountType.Aquatico"/> on
    /// paper but swims only from wave 2; until then it walks like the others.
    /// Where each one comes from: shop whistles (<see cref="ShopWhistles"/>, wired in <c>CityShopCatalog</c>), enemy and boss
    /// drops plus the Fitas do Laço (<see cref="MountDropRules"/>), missions/achievements/secret/event
    /// (<see cref="MountSourceHooks"/>).
    /// </summary>
    public static class MountWaveCatalog
    {
        public const string Capybara = "mount_capybara";
        public const string MountainGoat = "mount_mountain_goat";
        public const string Rhea = "mount_rhea";
        public const string PintoHorse = "mount_pinto_horse";
        public const string EnchantedDonkey = "mount_enchanted_donkey";
        public const string ManedWolf = "mount_maned_wolf";
        public const string CaiporaPeccary = "mount_caipora_peccary";
        public const string GiantAnteater = "mount_giant_anteater";
        public const string SnowTiger = "mount_snow_tiger";
        public const string WoollyMammoth = "mount_woolly_mammoth";
        public const string Unicorn = "mount_unicorn";
        public const string LightJaguar = "mount_light_jaguar";
        public const string Carbuncle = "mount_carbuncle";
        public const string FireRhea = "mount_fire_rhea";
        public const string Kirin = "mount_kirin";
        public const string Boitata = "mount_boitata";
        public const string BoiBumba = "mount_boi_bumba";
        public const string ElderTortoise = "mount_elder_tortoise";

        /// <summary>"Apito de …" item of a species (only the shop ones have an asset in <c>Resources/Items</c>).</summary>
        public static string WhistleOf(string mountId) => "item_whistle_" + mountId;

        /// <summary>The wave's mounts sold for gold (§3.6): Haras da Vila, the Desert saddlery, the Mountain forge, the Caves lamp shop.</summary>
        public static readonly IReadOnlyList<string> ShopWhistles = new[]
        {
            WhistleOf(Capybara), WhistleOf(MountainGoat), WhistleOf(Rhea), WhistleOf(PintoHorse), WhistleOf(GiantAnteater),
        };

        public static readonly IReadOnlyList<MountSpec> Specs = new List<MountSpec>
        {
            // ---------------- Comuns (loja) ----------------
            new MountSpec
            {
                MountId = Capybara, Type = MountType.Aquatico, Size = MountSize.Pequeno, Region = BiomeType.Forest,
                Speed = 1.35f, Defense = 0.04f, Health = 150f, Stamina = 110f, Diet = MountDiet.Herbivora,
                WhistleItemId = WhistleOf(Capybara), ShopPrice = 55, Recipe = MountVisualRecipe.Capybara, SeatHeight = 0.7f,
                AbilityId = "banho_de_lama", AbilityKind = MountAbilityKind.Cleanse, Cooldown = 35f, Duration = 3f,
            },
            new MountSpec
            {
                MountId = MountainGoat, Size = MountSize.Pequeno, Region = BiomeType.Mountain,
                Speed = 1.40f, Defense = 0.03f, Health = 150f, Stamina = 110f, Diet = MountDiet.Herbivora,
                WhistleItemId = WhistleOf(MountainGoat), ShopPrice = 70, Recipe = MountVisualRecipe.Goat, SeatHeight = 0.85f,
                AbilityId = "pulo_duplo", AbilityKind = MountAbilityKind.DoubleJump, Cooldown = 6f, Distance = 2.5f,
            },
            new MountSpec
            {
                MountId = Rhea, Region = BiomeType.Desert,
                Speed = 1.55f, Defense = 0f, Health = 100f, Stamina = 130f, Diet = MountDiet.Herbivora,
                WhistleItemId = WhistleOf(Rhea), ShopPrice = 75, Recipe = MountVisualRecipe.Rhea, SeatHeight = 1.15f,
                AbilityId = "folego_de_ema", AbilityKind = MountAbilityKind.FreeGallop, Cooldown = 30f, Duration = 8f,
            },

            // ---------------- Incomuns ----------------
            new MountSpec
            {
                MountId = PintoHorse, Rarity = ItemRarity.Uncommon, Region = BiomeType.Forest,
                Speed = 1.60f, Defense = 0.03f, Health = 150f, Stamina = 110f, IsWarMount = true, Diet = MountDiet.Herbivora,
                WhistleItemId = WhistleOf(PintoHorse), ShopPrice = 150, Recipe = MountVisualRecipe.PintoHorse, SeatHeight = 1.1f,
                AbilityId = "investida_do_vaqueiro", AbilityKind = MountAbilityKind.Knockback, Cooldown = 20f, Duration = 0.4f, Distance = 8f,
                Radius = 3f, BossMultiplier = 0f,
            },
            new MountSpec
            {
                MountId = EnchantedDonkey, Rarity = ItemRarity.Uncommon, Region = BiomeType.Desert,
                Speed = 1.40f, Defense = 0.05f, Health = 200f, Stamina = 110f, Diet = MountDiet.Herbivora | MountDiet.Encantada,
                WhistleItemId = WhistleOf(EnchantedDonkey), Recipe = MountVisualRecipe.EnchantedDonkey, SeatHeight = 1.0f,
                AbilityId = "teimosia_santa", AbilityKind = MountAbilityKind.CCImmunity, Cooldown = 40f, Duration = 5f,
            },
            new MountSpec
            {
                MountId = ManedWolf, Rarity = ItemRarity.Uncommon, Region = BiomeType.Forest,
                Speed = 1.60f, Defense = 0.02f, Health = 125f, Stamina = 130f, Diet = MountDiet.Carnivora,
                WhistleItemId = WhistleOf(ManedWolf), Recipe = MountVisualRecipe.ManedWolf, SeatHeight = 1.05f,
                AbilityId = "olhos_do_cerrado", AbilityKind = MountAbilityKind.NightVision, Cooldown = 60f, Duration = 30f, Radius = 30f,
            },
            new MountSpec
            {
                MountId = CaiporaPeccary, Rarity = ItemRarity.Uncommon, Region = BiomeType.Forest,
                Speed = 1.50f, Defense = 0.04f, Health = 150f, Stamina = 120f, Diet = MountDiet.Herbivora | MountDiet.Encantada,
                WhistleItemId = WhistleOf(CaiporaPeccary), Recipe = MountVisualRecipe.Peccary, SeatHeight = 0.8f,
                AbilityId = "trilha_da_caipora", AbilityKind = MountAbilityKind.PartySpeedTrail, Cooldown = 30f, Duration = 6f, Radius = 1.2f,
                Magnitude = 0.15f,
            },
            new MountSpec
            {
                MountId = GiantAnteater, Rarity = ItemRarity.Uncommon, Region = BiomeType.Caves,
                Speed = 1.40f, Defense = 0.05f, Health = 175f, Stamina = 120f, Diet = MountDiet.Herbivora,
                WhistleItemId = WhistleOf(GiantAnteater), ShopPrice = 140, Recipe = MountVisualRecipe.Anteater, SeatHeight = 0.9f,
                AbilityId = "lingua_comprida", AbilityKind = MountAbilityKind.RangedInteract, Cooldown = 20f, Distance = 12f,
            },

            // ---------------- Raras ----------------
            new MountSpec
            {
                MountId = SnowTiger, Rarity = ItemRarity.Rare, Region = BiomeType.Mountain,
                Speed = 1.70f, Defense = 0.03f, Health = 150f, Stamina = 130f, IsWarMount = true, Diet = MountDiet.Carnivora,
                WhistleItemId = WhistleOf(SnowTiger), Recipe = MountVisualRecipe.SnowTiger, SeatHeight = 0.9f,
                AbilityId = "rugido_do_tigre", AbilityKind = MountAbilityKind.Intimidate, Cooldown = 35f, Duration = 6f, Radius = 8f,
                Magnitude = 0.2f, BossMultiplier = 0.5f,
            },
            new MountSpec
            {
                MountId = WoollyMammoth, Rarity = ItemRarity.Rare, Size = MountSize.Grande, Region = BiomeType.Mountain,
                Speed = 1.30f, Defense = 0.10f, Health = 250f, Stamina = 100f, IsWarMount = true, Diet = MountDiet.Herbivora,
                Favorite = CharacterId.Joaquim,
                WhistleItemId = WhistleOf(WoollyMammoth), Recipe = MountVisualRecipe.Mammoth, SeatHeight = 1.85f,
                AbilityId = "pisao_do_mamute", AbilityKind = MountAbilityKind.AreaStun, Cooldown = 25f, Duration = 1.2f, Radius = 4f,
                BossMultiplier = 0.5f,
            },
            new MountSpec
            {
                MountId = Unicorn, Rarity = ItemRarity.Rare, Region = BiomeType.Forest,
                Speed = 1.55f, Defense = 0.04f, Health = 175f, Stamina = 140f, Diet = MountDiet.Encantada, Favorite = CharacterId.Maria,
                WhistleItemId = WhistleOf(Unicorn), Recipe = MountVisualRecipe.Unicorn, SeatHeight = 1.1f,
                AbilityId = "aura_curativa", AbilityKind = MountAbilityKind.HealAura, Cooldown = 45f, Duration = 6f, Radius = 6f, Magnitude = 0.02f,
            },
            new MountSpec
            {
                MountId = LightJaguar, Rarity = ItemRarity.Rare, Region = BiomeType.Forest,
                Speed = 1.75f, Defense = 0.02f, Health = 125f, Stamina = 140f, IsWarMount = true, Diet = MountDiet.Carnivora,
                WhistleItemId = WhistleOf(LightJaguar), Recipe = MountVisualRecipe.LightJaguar, SeatHeight = 0.9f,
                AbilityId = "salto_da_onca", AbilityKind = MountAbilityKind.PounceMark, Cooldown = 18f, Duration = 0.35f, Distance = 12f,
            },
            new MountSpec
            {
                MountId = Carbuncle, Rarity = ItemRarity.Rare, Region = BiomeType.Caves,
                Speed = 1.45f, Defense = 0.06f, Health = 200f, Stamina = 130f, Diet = MountDiet.Encantada,
                WhistleItemId = WhistleOf(Carbuncle), Recipe = MountVisualRecipe.Carbuncle, SeatHeight = 0.85f,
                AbilityId = "joia_acesa", AbilityKind = MountAbilityKind.Light, Cooldown = 90f, Duration = 60f, Radius = 15f,
            },
            new MountSpec
            {
                MountId = FireRhea, Rarity = ItemRarity.Rare, Region = BiomeType.Desert,
                Speed = 1.70f, Defense = 0.01f, Health = 125f, Stamina = 160f, Diet = MountDiet.Encantada,
                WhistleItemId = WhistleOf(FireRhea), Recipe = MountVisualRecipe.FireRhea, SeatHeight = 1.15f,
                AbilityId = "corrida_de_brasa", AbilityKind = MountAbilityKind.DamageTrail, Cooldown = 30f, Duration = 6f, Radius = 1f, Magnitude = 4f,
            },

            // ---------------- Épicas ----------------
            new MountSpec
            {
                MountId = Kirin, Rarity = ItemRarity.Epic, Region = BiomeType.Caves,
                Speed = 1.65f, Defense = 0.06f, Health = 200f, Stamina = 140f, Diet = MountDiet.Encantada,
                WhistleItemId = WhistleOf(Kirin), Recipe = MountVisualRecipe.Kirin, SeatHeight = 1.1f,
                AbilityId = "passo_sem_pegada", AbilityKind = MountAbilityKind.Stealth, Cooldown = 60f, Duration = 10f, Magnitude = 0.2f,
            },
            new MountSpec
            {
                MountId = Boitata, Rarity = ItemRarity.Epic, Region = BiomeType.Forest,
                Speed = 1.60f, Defense = 0.08f, Health = 225f, Stamina = 120f, IsWarMount = true, Diet = MountDiet.Encantada,
                WhistleItemId = WhistleOf(Boitata), Recipe = MountVisualRecipe.Boitata, SeatHeight = 0.85f,
                AbilityId = "anel_de_fogo_protetor", AbilityKind = MountAbilityKind.RepelRing, Cooldown = 40f, Duration = 6f, Radius = 5f,
                Magnitude = 5f, BossMultiplier = 0f,
            },
            new MountSpec
            {
                MountId = BoiBumba, Rarity = ItemRarity.Epic, Size = MountSize.Grande, Region = BiomeType.Forest,
                Speed = 1.45f, Defense = 0.10f, Health = 275f, Stamina = 110f, IsWarMount = true, Diet = MountDiet.Encantada,
                WhistleItemId = WhistleOf(BoiBumba), Recipe = MountVisualRecipe.BoiBumba, SeatHeight = 1.4f,
                AbilityId = "toada_do_boi", AbilityKind = MountAbilityKind.PartyBuff, Cooldown = 60f, Duration = 10f, Radius = 10f, Magnitude = 0.1f,
            },

            // ---------------- Lendária ----------------
            new MountSpec
            {
                MountId = ElderTortoise, Rarity = ItemRarity.Legendary, Size = MountSize.Grande, Region = BiomeType.Forest,
                Speed = 1.30f, Defense = 0.15f, Health = 375f, Stamina = 100f, IsWarMount = true, Diet = MountDiet.Herbivora,
                WhistleItemId = WhistleOf(ElderTortoise), Recipe = MountVisualRecipe.ElderTortoise, SeatHeight = 1.0f,
                AbilityId = "casco_de_pedra", AbilityKind = MountAbilityKind.Invulnerable, Cooldown = 90f, Duration = 3f,
            },
        };
    }
}
