using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// The flying mounts (<see cref="MountType.Voador"/>, Docs/Mounts.md §2.6 and §3.8). Merged into
    /// <see cref="MountCatalog.Specs"/> after M3 wave 1. Owning one answers the "Asas ou montaria voadora" gates of the
    /// maps (<c>MapRequirementRules</c>: map 24 Ninho de Mahaway, <c>WingsOrDoubleJump</c>; map 39 Forte dos Ventos,
    /// <c>WingsRequired</c>), because <c>MapRequirementHeroState</c> turns every owned mount into its <see cref="MountSpec.Type"/>.
    /// <para>The flight is a look only: the model hovers and beats its wings over the hero, who keeps walking on the
    /// ground (same CharacterController, same click-to-move, same collisions; <see cref="MountFlightRules"/>).</para>
    /// </summary>
    public static class MountSkyCatalog
    {
        /// <summary>
        /// Grifo da Coluna da Glória: a griffin of light, lion body and eagle head, that guards the Coluna da Glória
        /// (Srōšahrāy, the path the freed sparks climb, Docs/Narrative.md §1.1). One of them was "put out" and fell into
        /// the Grutas do Limiar; 40 sparks freed by a cousin of level 40+ light it again (<see cref="ColumnGriffinQuest"/>).
        /// </summary>
        public const string ColumnGriffin = "mount_column_griffin";

        /// <summary>"Voo da Coluna" (G): a short flight forward, high over the critters, landing in a ring of light.</summary>
        public const string ColumnFlight = "voo_da_coluna";

        public static readonly IReadOnlyList<MountSpec> Specs = new List<MountSpec>
        {
            // ---------------- Rara (missão) ----------------
            // Budget §2.4, Rare = 16: speed 1.65 (7) + 2% (2) + 125 health (1) + 160 stamina (6).
            new MountSpec
            {
                MountId = ColumnGriffin, Type = MountType.Voador, Rarity = ItemRarity.Rare, Size = MountSize.Medio, Region = BiomeType.Caves,
                Speed = 1.65f, Defense = 0.02f, Health = 125f, Stamina = 160f, Diet = MountDiet.Encantada, Favorite = CharacterId.Lavine,
                WhistleItemId = MountWaveCatalog.WhistleOf(ColumnGriffin), Recipe = MountVisualRecipe.ColumnGriffin, SeatHeight = 0.95f,
                SeatForward = 0.02f,
                // 10 m forward in 0.8 s (walls stop it like the Arrancada); the landing scares the common critters
                // within 4 m for 2.5 s (Magnitude), bosses not at all.
                AbilityId = ColumnFlight, AbilityKind = MountAbilityKind.ShortFlight, Cooldown = 25f, Duration = 0.8f, Distance = 10f,
                Radius = 4f, Magnitude = 2.5f, BossMultiplier = 0f,
            },
        };

        /// <summary>Whether <paramref name="mountId"/> is a flyer of the catalog.</summary>
        public static bool IsFlyer(string mountId)
        {
            MountSpec spec = MountCatalog.FindSpec(mountId);
            return spec != null && spec.Type == MountType.Voador;
        }
    }
}
