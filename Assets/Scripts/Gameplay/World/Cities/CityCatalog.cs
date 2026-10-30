using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.World.Cities
{
    /// <summary>
    /// A resident of a city the cousins can chat with in the city hub: a name and one line per cousin (the
    /// <c>city.resident.&lt;id&gt;.*</c> keys of <c>CityTexts</c>). Shopkeepers are residents too (<see cref="ShopId"/>).
    /// </summary>
    public sealed class CityResidentSpec
    {
        public readonly string ResidentId;

        /// <summary>The shop this resident keeps (<see cref="CityShopCatalog"/>), or null.</summary>
        public readonly string ShopId;

        private readonly string _nameKey;

        public CityResidentSpec(string residentId, string shopId = null, string nameKey = null)
        {
            ResidentId = residentId;
            ShopId = shopId;
            _nameKey = nameKey;
        }

        /// <summary>Own key, or a shared one (Dona Cida, Mestre Tonho... are also region quest-givers: same name).</summary>
        public string NameKey => _nameKey ?? KeyBase + ".name";

        public string KeyBase => "city.resident." + ResidentId;

        public string LineKey(CharacterId hero) => KeyBase + ".line." + hero.ToString().ToLowerInvariant();

        /// <summary>Line when no cousin is selected (map played on its own).</summary>
        public string AnyLineKey => KeyBase + ".line.any";
    }

    /// <summary>One region's city: name, flavour, residents, shops and which village-only services it offers.</summary>
    public sealed class CitySpec
    {
        public readonly string CityId;
        public readonly BiomeType Region;
        public readonly IReadOnlyList<CityResidentSpec> Residents;

        /// <summary>Shops with a counter in this city (<see cref="CityShopCatalog"/> ids, or a <c>Resources/Shops</c> asset id).</summary>
        public readonly IReadOnlyList<string> ShopIds;

        /// <summary>
        /// Baú da Vó Zefa and Fogão a lenha: only in her village (they are her chest and her stove; the carro de boi
        /// brings the cousins back cheaply). See Docs/Cities.md.
        /// </summary>
        public readonly bool HasGrandmaServices;

        /// <summary>The personal-story villagers of <c>VillageNpcCatalog</c> (dialogue graphs) live here.</summary>
        public readonly bool HasStoryVillagers;

        public CitySpec(string cityId, BiomeType region, IReadOnlyList<CityResidentSpec> residents, IReadOnlyList<string> shopIds,
            bool hasGrandmaServices = false, bool hasStoryVillagers = false)
        {
            CityId = cityId;
            Region = region;
            Residents = residents;
            ShopIds = shopIds;
            HasGrandmaServices = hasGrandmaServices;
            HasStoryVillagers = hasStoryVillagers;
        }

        public string KeyBase => "city." + CityId;
        public string NameKey => KeyBase + ".name";

        /// <summary>Short label of the HUD button ("Vila", "Areia Branca"...).</summary>
        public string ButtonKey => KeyBase + ".button";
        public string FlavourKey => KeyBase + ".flavour";
    }

    /// <summary>
    /// Every region has a city (TODO.md 12.3a). The Floresta keeps the Vila da Vó Zefa (story villagers, the old
    /// general store, Baú and Fogão); the Deserto, the Montanha and the Cavernas get their own town, residents and
    /// local shop. Content as code, like <c>RegionQuestCatalog</c>/<c>MealCatalog</c>; texts in <c>CityTexts</c>.
    /// </summary>
    public static class CityCatalog
    {
        public const string VilaZefa = "vila_zefa";
        public const string AreiaBranca = "areia_branca";
        public const string SerraAlta = "serra_alta";
        public const string Lamparinas = "lamparinas";

        public static readonly IReadOnlyList<CitySpec> Cities = new List<CitySpec>
        {
            new CitySpec(VilaZefa, BiomeType.Forest,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("cida", CityShopCatalog.ArmarinhoCida, "dialogue.forest_rezadeira.name"),
                    new CityResidentSpec("lazaro", CityShopCatalog.MerceariaLazaro),
                    new CityResidentSpec("nenem", CityShopCatalog.FeiraLivre),
                    // Seu Tonico of the Haras (Docs/Mounts.md §3), Vô Jorge's old road friend: keeps the mount whistles.
                    // Not Seu Zé Caçador (Hunting): two different people. Was "ze_haras" before 2026-10-30; resident ids
                    // are not saved anywhere (only localization keys), so the rename needs no migration.
                    new CityResidentSpec("tonico_haras", CityShopCatalog.HarasVila),
                    // The new Vila shops (Docs/Cities.md §4): blacksmith, carpenter and seamstress.
                    new CityResidentSpec("severino", CityShopCatalog.FerrariaSeverino),
                    new CityResidentSpec("damiao", CityShopCatalog.MarcenariaDamiao),
                    new CityResidentSpec("fatima", CityShopCatalog.CostureiraFatima),
                },
                new List<string> { CityShopCatalog.GeneralStore, CityShopCatalog.MerceariaLazaro, CityShopCatalog.ArmarinhoCida, CityShopCatalog.FeiraLivre, CityShopCatalog.HarasVila,
                    CityShopCatalog.FerrariaSeverino, CityShopCatalog.MarcenariaDamiao, CityShopCatalog.CostureiraFatima },
                hasGrandmaServices: true, hasStoryVillagers: true),

            new CitySpec(AreiaBranca, BiomeType.Desert,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("vava", CityShopCatalog.SelariaVava),
                    new CityResidentSpec("socorro"),
                    new CityResidentSpec("tiao"),
                },
                new List<string> { CityShopCatalog.SelariaVava }),

            new CitySpec(SerraAlta, BiomeType.Mountain,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("tonho", CityShopCatalog.ForjaTonho, "dialogue.mountain_miner.name"),
                    new CityResidentSpec("graca"),
                    new CityResidentSpec("quinzinho"),
                },
                new List<string> { CityShopCatalog.ForjaTonho }),

            new CitySpec(Lamparinas, BiomeType.Caves,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("benedita", CityShopCatalog.LamparinasBenedita, "dialogue.caves_lampkeeper.name"),
                    new CityResidentSpec("candinho"),
                    new CityResidentSpec("fia"),
                    new CityResidentSpec("zezinho"),
                },
                new List<string> { CityShopCatalog.LamparinasBenedita }),
        };

        /// <summary>Seu Bento, the peddler: not a resident of one city, he is wherever his mule is today (<see cref="ShopRotation.MascateRegion"/>).</summary>
        public static readonly CityResidentSpec Mascate = new CityResidentSpec("bento", CityShopCatalog.MascateBento, "dialogue.desert_tropeiro.name");

        public static CitySpec Get(string cityId) => Cities.FirstOrDefault(c => c.CityId == cityId);

        public static CitySpec ForRegion(BiomeType? region)
        {
            return region.HasValue ? Cities.FirstOrDefault(c => c.Region == region.Value) : null;
        }

        public static bool HasCity(BiomeType region) => ForRegion(region) != null;

        /// <summary>Every resident, the roaming peddler included.</summary>
        public static IEnumerable<CityResidentSpec> AllResidents()
        {
            foreach (CitySpec city in Cities)
            {
                foreach (CityResidentSpec resident in city.Residents)
                {
                    yield return resident;
                }
            }

            yield return Mascate;
        }
    }
}
