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

        /// <summary>Own key, or a shared one (Mariam, Mestre Mihr... are also region quest-givers: same name).</summary>
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
        /// Baú da Vó Edna and forno (tandir): only in her village (they are her chest and her stove; the Barcas de Luz
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

        /// <summary>Short label of the HUD button ("Vila", "Qocho"...).</summary>
        public string ButtonKey => KeyBase + ".button";
        public string FlavourKey => KeyBase + ".flavour";
    }

    /// <summary>
    /// Every region has a city (TODO.md 12.3a). The Floresta keeps the Vila da Vó Edna (story villagers, the old
    /// general store, Baú and Fogão); the Deserto, the Montanha and the Cavernas get their own town, residents and
    /// local shop. Content as code, like <c>RegionQuestCatalog</c>/<c>MealCatalog</c>; texts in <c>CityTexts</c>.
    /// </summary>
    public static class CityCatalog
    {
        public const string VilaZefa = "vila_zefa";
        public const string AreiaBranca = "areia_branca";
        public const string SerraAlta = "serra_alta";
        public const string Lamparinas = "lamparinas";

        /// <summary>
        /// Copas de Garajonay, the race city of the elfos-da-mata (map 05, Docs/Mapas100.md §3.3). Same id as
        /// <c>MapCatalog.CopasGarajonay</c> and <c>CityTownRecipes.GarajonayCityId</c> (never change it: the map uses it).
        /// </summary>
        public const string CopasGarajonay = "copas_garajonay";

        /// <summary>
        /// Taa Pwani, the Costa da Lâmpada: the city of return of the Costa das Monções (região 5, maps 25 and 30,
        /// Docs/Mapas100.md). Same id as <c>MapCatalog.TaaPwani</c> and <c>CityTownRecipes.TaaPwaniCityId</c>.
        /// </summary>
        public const string TaaPwani = "taa_pwani";

        /// <summary>
        /// Tambor Rubro, the race city of the orcs (map 11, return city of maps 11–12, Docs/Mapas100.md §3.3). Same id as
        /// <c>MapCatalog.TamborRubro</c> and <c>CityTownRecipes.TamborRubroCityId</c>.
        /// </summary>
        public const string TamborRubro = "tambor_rubro";

        public static readonly IReadOnlyList<CitySpec> Cities = new List<CitySpec>
        {
            new CitySpec(VilaZefa, BiomeType.Forest,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("cida", CityShopCatalog.ArmarinhoCida, "dialogue.forest_rezadeira.name"),
                    new CityResidentSpec("lazaro", CityShopCatalog.MerceariaLazaro),
                    new CityResidentSpec("nenem", CityShopCatalog.FeiraLivre),
                    // Srosh of the Haras (Docs/Mounts.md §3), Vô Jorge's old road friend: keeps the mount whistles.
                    // Not Patig, the Tracker (Hunting): two different people. Was "ze_haras" before 2026-10-30; resident ids
                    // are not saved anywhere (only localization keys), so the rename needs no migration.
                    new CityResidentSpec("tonico_haras", CityShopCatalog.HarasVila),
                    // The lore cast of the Vila without a counter (TODO.md "História e Arcos dos NPCs"): Vó Edna on her
                    // aivan, Ramrat at the lamp, the two musicians of the Bema and Patig the tracker on the plaza.
                    new CityResidentSpec("vo_edna"),
                    new CityResidentSpec("socorro_vila"),
                    new CityResidentSpec("xroshtag"),
                    new CityResidentSpec("padwaxtag"),
                    new CityResidentSpec("patig"),
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
                    // Náfsha the potter, in the olaria on the West street (old id "rosa", like shop_olaria_rosa).
                    new CityResidentSpec("rosa"),
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

        /// <summary>
        /// The race cities (Docs/Mapas100.md §3.3) that already have a walkable town: a second city inside a region, so they
        /// stay out of <see cref="Cities"/> (one hub city per region: the world-map hub, the region respawn, the Barcas list).
        /// <see cref="Get"/>, <see cref="AllCities"/> and <see cref="AllResidents"/> see them; a defeat on one of their maps
        /// wakes the hero in their plaza (<c>MapCatalog.RespawnCityFor</c>).
        /// </summary>
        public static readonly IReadOnlyList<CitySpec> RaceCities = new List<CitySpec>
        {
            // Copas de Garajonay (elfos-da-mata): Gara the canopy warden (missions), Jonay the archer, Dácil the herbalist
            // (the city's shop, with the rope of the canopy and the Garden Key), Bencomo the old chief, Tinguaro the tracker.
            new CitySpec(CopasGarajonay, BiomeType.Forest,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("gara"),
                    new CityResidentSpec("jonay"),
                    new CityResidentSpec("dacil", CityShopCatalog.ErvariaDacil),
                    new CityResidentSpec("bencomo"),
                    new CityResidentSpec("tinguaro"),
                },
                new List<string> { CityShopCatalog.ErvariaDacil }),

            // Tambor Rubro (orcs, names from the Hurrian tales and Hittite/Mitanni history): Gurparanzah the archer chief
            // (missions), Kessi the hunter (second mission), Appu the gentle merchant who adopts the camp's orphans,
            // Ashmunikal the matriarch of the drums (the guardian lamp on the drum tower), Tushratta the smith (the city's
            // shop: the Forja de Tambor, with the theriac of map 12).
            new CitySpec(TamborRubro, BiomeType.Desert,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("gurparanzah"),
                    new CityResidentSpec("kessi"),
                    new CityResidentSpec("appu"),
                    new CityResidentSpec("ashmunikal"),
                    new CityResidentSpec("tushratta", CityShopCatalog.ForjaTambor),
                },
                new List<string> { CityShopCatalog.ForjaTambor }),
        };

        /// <summary>
        /// The cities of the regions past the four biomes of the world map (Docs/Mapas100.md regiões 5–8) that already have a
        /// walkable town. Their region has no world-map biome yet, so their <see cref="CitySpec.Region"/> is a borrowed one (the
        /// Forest: always unlocked, so the respawn of <see cref="CityProgress.ApplyDefeatIn"/> always lands in their plaza);
        /// like the race cities they stay out of <see cref="Cities"/> (no world-map hub, no peddler, no Barcas hub list)
        /// and are reached through their maps. Their shops sell the gear of their region's map gates
        /// (Docs/MetodologiaMapasERacas.md §2.1).
        /// </summary>
        public static readonly IReadOnlyList<CitySpec> LateRegionCities = new List<CitySpec>
        {
            // Taa Pwani (Swahili sailors and merchants): Captain Liyongo the nakhoda (missions), Mwana the pearl merchant (the
            // city's shop: dhow passages and the gear of maps 25–30), Nuru the keeper of the Farol de Coral, Zawadi who braids
            // root boots (second mission), old Mbaraka who reads the Moon Ship's stars.
            new CitySpec(TaaPwani, BiomeType.Forest,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("liyongo"),
                    new CityResidentSpec("mwana", CityShopCatalog.BazarMwana),
                    new CityResidentSpec("nuru"),
                    new CityResidentSpec("zawadi"),
                    new CityResidentSpec("mbaraka"),
                },
                new List<string> { CityShopCatalog.BazarMwana }),
        };

        /// <summary>Ammo, the peddler: not a resident of one city, he is wherever his mule is today (<see cref="ShopRotation.MascateRegion"/>).</summary>
        public static readonly CityResidentSpec Mascate = new CityResidentSpec("bento", CityShopCatalog.MascateBento, "dialogue.desert_tropeiro.name");

        /// <summary>Any city with a walkable town: the region hubs, then the race cities, then the late-region cities.</summary>
        public static IEnumerable<CitySpec> AllCities => Cities.Concat(RaceCities).Concat(LateRegionCities);

        public static CitySpec Get(string cityId) => AllCities.FirstOrDefault(c => c.CityId == cityId);

        /// <summary>Whether <paramref name="city"/> is a race city (not its region's hub: no world-map hub, no peddler).</summary>
        public static bool IsRaceCity(CitySpec city) => city != null && RaceCities.Contains(city);

        /// <summary>Whether <paramref name="city"/> is a late-region city (Taa Pwani...): reached through its maps, not the world map.</summary>
        public static bool IsLateRegionCity(CitySpec city) => city != null && LateRegionCities.Contains(city);

        /// <summary>Whether <paramref name="city"/> is its region's hub (<see cref="Cities"/>): the only kind the peddler visits.</summary>
        public static bool IsHubCity(CitySpec city) => city != null && Cities.Contains(city);

        public static CitySpec ForRegion(BiomeType? region)
        {
            return region.HasValue ? Cities.FirstOrDefault(c => c.Region == region.Value) : null;
        }

        public static bool HasCity(BiomeType region) => ForRegion(region) != null;

        /// <summary>Every resident, the roaming peddler included.</summary>
        public static IEnumerable<CityResidentSpec> AllResidents()
        {
            foreach (CitySpec city in AllCities)
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
