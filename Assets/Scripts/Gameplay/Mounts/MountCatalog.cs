using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>A mount species as plain data (Docs/Mounts.md §3), turned into a <see cref="MountDefinition"/> by <see cref="MountCatalog"/>.</summary>
    public sealed class MountSpec
    {
        public string MountId;
        public MountType Type = MountType.Terrestre;
        public ItemRarity Rarity = ItemRarity.Common;
        public MountSize Size = MountSize.Medio;
        public BiomeType Region = BiomeType.Forest;
        public float Speed;
        public float Defense;
        public float Health;
        public float Stamina;
        public bool IsWarMount;
        public MountDiet Diet = MountDiet.Herbivora;
        public CharacterId? Favorite;
        public string WhistleItemId;
        public int ShopPrice;
        public MountVisualRecipe Recipe;
        public float SeatHeight;
        public float SeatForward;

        public string AbilityId;
        public MountAbilityKind AbilityKind;
        public float Cooldown;
        public float Duration;
        public float Distance;
        public float Radius;
        public float Magnitude;
        public float BossMultiplier;
    }

    /// <summary>
    /// The mounts of the game. Phase M1 has the three starters of §13 (Pônei Pé-de-Pano: balanced, Cachorrão Paçoca:
    /// fast, Jegue Carregador: sturdy), all Comum, all 8 budget points. <see cref="Get"/> prefers an authored asset at
    /// <c>Resources/Mounts/&lt;mountId&gt;</c> (baked by "Rapadura/Mounts/Bake Starter Mount Assets", or authored by hand
    /// later) and builds the definition from the spec below otherwise, cached for the session.
    /// </summary>
    public static class MountCatalog
    {
        public const string ResourcesFolder = "Mounts";

        public const string Pony = "mount_pony";
        public const string BigDog = "mount_big_dog";
        public const string PackDonkey = "mount_pack_donkey";

        public const string PonyWhistle = "item_whistle_mount_pony";
        public const string BigDogWhistle = "item_whistle_mount_big_dog";
        public const string PackDonkeyWhistle = "item_whistle_mount_pack_donkey";

        public const string Arrancada = "arrancada";
        public const string FaroDeTesouro = "faro_de_tesouro";
        public const string ZurroEspantaBicho = "zurro_espanta_bicho";

        public static readonly IReadOnlyList<MountSpec> Specs = new List<MountSpec>
        {
            new MountSpec
            {
                MountId = Pony, Size = MountSize.Pequeno, Region = BiomeType.Forest,
                Speed = 1.45f, Defense = 0.03f, Health = 125f, Stamina = 110f,
                Diet = MountDiet.Herbivora, Favorite = CharacterId.Lavine,
                WhistleItemId = PonyWhistle, ShopPrice = 40, Recipe = MountVisualRecipe.Pony, SeatHeight = 0.9f, SeatForward = 0.02f,
                AbilityId = Arrancada, AbilityKind = MountAbilityKind.Dash, Cooldown = 12f, Duration = 0.3f, Distance = 6f,
            },
            new MountSpec
            {
                MountId = BigDog, Size = MountSize.Medio, Region = BiomeType.Forest,
                Speed = 1.50f, Defense = 0.02f, Health = 125f, Stamina = 110f,
                Diet = MountDiet.Carnivora,
                WhistleItemId = BigDogWhistle, ShopPrice = 60, Recipe = MountVisualRecipe.BigDog, SeatHeight = 0.85f, SeatForward = -0.02f,
                AbilityId = FaroDeTesouro, AbilityKind = MountAbilityKind.LootMagnet, Cooldown = 40f, Duration = 10f, Radius = 8f, Magnitude = 20f,
            },
            new MountSpec
            {
                MountId = PackDonkey, Size = MountSize.Medio, Region = BiomeType.Desert,
                Speed = 1.30f, Defense = 0.04f, Health = 175f, Stamina = 110f,
                Diet = MountDiet.Herbivora,
                WhistleItemId = PackDonkeyWhistle, ShopPrice = 65, Recipe = MountVisualRecipe.Donkey, SeatHeight = 0.95f, SeatForward = 0f,
                AbilityId = ZurroEspantaBicho, AbilityKind = MountAbilityKind.Fear, Cooldown = 45f, Duration = 3f, Radius = 6f, BossMultiplier = 0f,
            },
        };

        private static readonly Dictionary<string, MountDefinition> Cache = new Dictionary<string, MountDefinition>(StringComparer.Ordinal);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Cache.Clear();

        public static IEnumerable<string> MountIds => Specs.Select(s => s.MountId);

        public static MountSpec FindSpec(string mountId) => Specs.FirstOrDefault(s => s.MountId == mountId);

        /// <summary>Mount whose whistle is <paramref name="itemId"/>, or null.</summary>
        public static MountSpec FindByWhistle(string itemId)
        {
            return string.IsNullOrEmpty(itemId) ? null : Specs.FirstOrDefault(s => s.WhistleItemId == itemId);
        }

        public static bool IsWhistle(string itemId) => FindByWhistle(itemId) != null;

        /// <summary>The definition of <paramref name="mountId"/> (authored asset first, code spec otherwise), or null.</summary>
        public static MountDefinition Get(string mountId)
        {
            if (string.IsNullOrEmpty(mountId))
            {
                return null;
            }

            if (Cache.TryGetValue(mountId, out MountDefinition cached) && cached != null)
            {
                return cached;
            }

            MountDefinition definition = Resources.Load<MountDefinition>(ResourcesFolder + "/" + mountId);
            if (definition == null)
            {
                MountSpec spec = FindSpec(mountId);
                if (spec == null)
                {
                    return null;
                }

                definition = MountDefinition.CreateRuntime(spec, CreateAbility(spec));
            }

            Cache[mountId] = definition;
            return definition;
        }

        /// <summary>Code-built ability of <paramref name="spec"/>.</summary>
        public static MountAbilityDefinition CreateAbility(MountSpec spec)
        {
            return MountAbilityDefinition.CreateRuntime(spec.AbilityId, spec.AbilityKind, spec.Cooldown, spec.Duration, spec.Distance,
                spec.Radius, spec.Magnitude, spec.BossMultiplier);
        }
    }
}
