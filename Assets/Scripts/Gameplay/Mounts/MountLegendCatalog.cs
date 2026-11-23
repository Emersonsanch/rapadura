using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// The legendary of the map sheets (Docs/Mounts.md §3.14, Docs/Mapas100.md §6): the <b>Barquinha de Luz</b> (ficha 76, "anda sobre
    /// água e luz; flutua 1 m"), a little skiff of the Barcas de Luz (bíblia §3.16) that Apep caught in her coils on the sandbank of map 76.
    /// When Apep falls, the skiff is freed only very rarely (<see cref="MountDropRules.Table"/>, 0.3%, the top of the Legendary band of
    /// §2.1); Legendaries never come from Fitas, so there is no bad-luck protection (§3.5). Exactly on the Legendary budget (26, §2.4),
    /// <see cref="MountType.Aquatico"/> (it opens the boat and ford requirements like the other swimmers), and it floats a metre over the
    /// ground (<see cref="MountLegendRules.FloatHeight"/>, look only). Merged into <see cref="MountCatalog.Specs"/> after
    /// <see cref="MountBossCatalog"/>.
    /// </summary>
    public static class MountLegendCatalog
    {
        public const string LightSkiff = "mount_light_skiff";

        /// <summary>Which boss frees which legendary (enemy id, mount id); the drop rows live in <see cref="MountDropRules.Table"/>.</summary>
        public static readonly IReadOnlyList<(string BossId, string MountId)> BossSources = new[]
        {
            (MountDropRules.Apep, LightSkiff),
        };

        public static readonly IReadOnlyList<MountSpec> Specs = new List<MountSpec>
        {
            // ---------------- Lendária (drop do chefe do mundo, 0,3%; sem Fitas) ----------------
            // Região 13, Rio das Doze Horas (Banco de Areia de Apep, 76; Apep). Budget 26: 1.75 (9) + 6% (6) + 200 health (4) + 170 stamina (7).
            // A little skiff of the Barcas de Luz: rows itself on water and light, a metre over the ground, light and sure.
            new MountSpec
            {
                MountId = LightSkiff, Type = MountType.Aquatico, Rarity = ItemRarity.Legendary, Size = MountSize.Medio, Region = BiomeType.Desert,
                Speed = 1.75f, Defense = 0.06f, Health = 200f, Stamina = 170f, Diet = MountDiet.Encantada, Favorite = CharacterId.Maithe,
                WhistleItemId = MountWaveCatalog.WhistleOf(LightSkiff), Recipe = MountVisualRecipe.LightSkiff, SeatHeight = 0.8f,
                AbilityId = "travessia_de_luz", AbilityKind = MountAbilityKind.LightCrossing, Cooldown = 60f, Duration = 8f, Radius = 10f,
                Magnitude = 0.2f, BossMultiplier = 0f,
            },
        };
    }
}
