using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.World.Map;

namespace Rapadura.Gameplay.Characters
{
    /// <summary>The broad family of a people (Docs/MetodologiaMapasERacas.md Part III headings).</summary>
    public enum RaceFamily
    {
        Human = 0,
        Elven = 1,
        Orcish = 2,
        Awakened = 3,
        Vedic = 4,
        Fantastic = 5,
    }

    /// <summary>One of the Five Elements of Light a people keeps (or lost) — world bible.</summary>
    public enum LightElement
    {
        Light = 0,
        LivingWater = 1,
        LivingFire = 2,
        LivingWind = 3,
        SereneAir = 4,
        /// <summary>Earth and living water (the Sogdian farmers).</summary>
        Earth = 5,
    }

    /// <summary>
    /// How a people speaks in dialogue (a writing guide for lines, never an accent of a real people — Docs/Mapas100.md
    /// §3.1): the NPC dialogue of that race follows it.
    /// </summary>
    public enum DialogueStyle
    {
        WarmFamily = 0,
        Storyteller = 1,
        PausedWisdom = 2,
        SeafarerBanter = 3,
        OathBound = 4,
        Whistled = 5,
        Ceremonial = 6,
        ForgeProud = 7,
        BarteringChatter = 8,
        HospitableBoom = 9,
        TidalCalm = 10,
        SlowGentle = 11,
        Playful = 12,
        Musical = 13,
        RememberingEcho = 14,
        Riddling = 15,
    }

    /// <summary>
    /// One people of the Ilha da Aurora and beyond (Docs/MetodologiaMapasERacas.md Part III + Docs/Mapas100.md §3): data
    /// only — home map/region, element, dialogue style, cosmetic traits (tags the NPC look recipes may read) and the
    /// notable NPCs. No people is evil by nature (every one keeps sparks of the Living Light). Plain C# for the server,
    /// like <c>CityCatalog</c>; texts in <see cref="RaceTexts"/>.
    /// </summary>
    public sealed class RaceDefinition
    {
        public readonly string Id;
        public readonly RaceFamily Family;
        public readonly MapRegion HomeRegion;

        /// <summary>The map of its town (<see cref="MapCatalog"/> id), or null when it has none in the ★ scope.</summary>
        public readonly string HomeMapId;

        public readonly LightElement Element;
        public readonly DialogueStyle Dialogue;

        /// <summary>Cosmetic trait tags (<c>long_ears</c>, <c>tusks</c>, <c>tail</c>...), read by NPC look recipes.</summary>
        public readonly IReadOnlyList<string> CosmeticTraits;

        /// <summary>Names of the notable NPCs of this people (proper names, not localized).</summary>
        public readonly IReadOnlyList<string> NotableNpcs;

        /// <summary>The mythology the names of its NPCs come from (Docs/Mapas100.md §3.2), for writers.</summary>
        public readonly string NameSource;

        public RaceDefinition(string id, RaceFamily family, MapRegion homeRegion, string homeMapId, LightElement element, DialogueStyle dialogue,
            string[] cosmeticTraits, string[] notableNpcs, string nameSource)
        {
            Id = id;
            Family = family;
            HomeRegion = homeRegion;
            HomeMapId = homeMapId;
            Element = element;
            Dialogue = dialogue;
            CosmeticTraits = cosmeticTraits ?? new string[0];
            NotableNpcs = notableNpcs ?? new string[0];
            NameSource = nameSource ?? string.Empty;
        }

        public string NameKey => "race." + Id + ".name";
        public string SummaryKey => "race." + Id + ".summary";
    }

    /// <summary>
    /// The 25 peoples of the compendium. The Docs/MetodologiaMapasERacas.md "Nagas" are the <c>serpent_folk</c> here:
    /// Docs/Mapas100.md §3.1 rules the word out (a living sacred figure), so the people keeps its look and lore under a
    /// neutral name.
    /// </summary>
    public static class RaceCatalog
    {
        public static readonly IReadOnlyList<RaceDefinition> Races = new List<RaceDefinition>
        {
            // 1. Humans and the peoples of the road
            new RaceDefinition("sogdian", RaceFamily.Human, MapRegion.Zarafshan, MapCatalog.PomarId, LightElement.Earth, DialogueStyle.WarmFamily,
                new[] { "high_embroidered_collar", "felt_shawl", "turban" }, new[] { "Vó Edna", "Mestre Adamas", "Mariam", "Zaku" }, "sogdian"),
            new RaceDefinition("qocho_nomad", RaceFamily.Human, MapRegion.AreiasFlamejantes, "map_007_caravanas", LightElement.LivingWind, DialogueStyle.Storyteller,
                new[] { "layered_robes", "face_veil", "brass_goggles" }, new[] { "Ammo", "Vandak" }, "sogdian"),
            new RaceDefinition("kaggen_highlander", RaceFamily.Human, MapRegion.SerraDeKaggen, "map_013_rondaveis", LightElement.LivingWind, DialogueStyle.PausedWisdom,
                new[] { "wool_blanket", "conical_straw_hat" }, new[] { "Coti", "Mestre Mihr" }, "san"),
            new RaceDefinition("swahili_sailor", RaceFamily.Human, MapRegion.CostaDasMoncoes, "map_025_taa_pwani", LightElement.LivingWater, DialogueStyle.SeafarerBanter,
                new[] { "white_kanzu", "gold_embroidered_vest" }, new[] { "Capitão Liyongo", "Mwana" }, "swahili"),
            new RaceDefinition("nart_rider", RaceFamily.Human, MapRegion.PlanicieDeMingcheng, null, LightElement.LivingWind, DialogueStyle.OathBound,
                new[] { "felt_tunic", "silver_plate_belt", "turquoise_braids" }, new string[0], "ossetian"),
            new RaceDefinition("cave_folk", RaceFamily.Human, MapRegion.GrutasDoLimiar, "map_019_lago_limiar", LightElement.Light, DialogueStyle.RememberingEcho,
                new[] { "lamp_lanyard", "tuff_dust_cloak" }, new[] { "Gabryab" }, "etruscan"),
            new RaceDefinition("sangkent_monk", RaceFamily.Human, MapRegion.TetoDoMundo, "map_036_sangkent", LightElement.Light, DialogueStyle.Ceremonial,
                new[] { "quilted_robe", "prayer_beads", "yak_wool_cap" }, new string[0], "sogdian"),
            new RaceDefinition("mingcheng_imperial", RaceFamily.Human, MapRegion.PlanicieDeMingcheng, "map_041_mingcheng", LightElement.Light, DialogueStyle.Ceremonial,
                new[] { "silk_robe", "lacquer_hat" }, new string[0], "tocharian"),

            // 2. Elven peoples
            new RaceDefinition("forest_elf", RaceFamily.Elven, MapRegion.Zarafshan, "map_005_garajonay", LightElement.LivingWater, DialogueStyle.Whistled,
                new[] { "long_ears", "bark_skin", "autumn_hair", "mulberry_silk" }, new[] { "Gara", "Jonay", "Dácil", "Bencomo", "Tinguaro" }, "guanche"),
            new RaceDefinition("high_elf", RaceFamily.Elven, MapRegion.PlanicieDeMingcheng, "map_042_torres_brancas", LightElement.Light, DialogueStyle.Ceremonial,
                new[] { "long_ears", "golden_eyes", "white_silk", "gold_plates" }, new string[0], "kushite"),

            // 3. Orcish clans
            new RaceDefinition("red_drum_orc", RaceFamily.Orcish, MapRegion.AreiasFlamejantes, "map_011_tambor_rubro", LightElement.LivingFire, DialogueStyle.OathBound,
                new[] { "tusks", "terracotta_skin", "glowing_ritual_scars" }, new[] { "Kessi", "Appu", "Gurparanzah" }, "hurrian"),

            // 4. The awakened of the Threshold
            new RaceDefinition("ivory_ossuary", RaceFamily.Awakened, MapRegion.GrutasDoLimiar, null, LightElement.Light, DialogueStyle.RememberingEcho,
                new[] { "alabaster_bones", "bronze_mail", "blue_wisp_eyes" }, new string[0], "etruscan"),

            // 5. Peoples of the delta
            new RaceDefinition("jhilpuri", RaceFamily.Vedic, MapRegion.DeltaDeJhilpur, "map_031_jhilpur", LightElement.LivingWater, DialogueStyle.Storyteller,
                new[] { "saffron_cotton", "sandalwood_mark" }, new string[0], "sanskrit_epic"),
            new RaceDefinition("serpent_folk", RaceFamily.Vedic, MapRegion.DeltaDeJhilpur, null, LightElement.LivingWater, DialogueStyle.TidalCalm,
                new[] { "iridescent_scales", "serpent_tail" }, new string[0], "sanskrit_epic"),
            new RaceDefinition("vanara", RaceFamily.Vedic, MapRegion.DeltaDeJhilpur, "map_032_raiz_viva", LightElement.LivingWind, DialogueStyle.Playful,
                new[] { "prehensile_tail", "golden_fur", "bamboo_staff" }, new string[0], "sanskrit_epic"),
            new RaceDefinition("gandharva", RaceFamily.Vedic, MapRegion.PlanicieDeMingcheng, null, LightElement.SereneAir, DialogueStyle.Musical,
                new[] { "floating_silk", "sitar" }, new string[0], "sanskrit_epic"),

            // 6. Other peoples
            new RaceDefinition("napirasu_dwarf", RaceFamily.Fantastic, MapRegion.SerraDeKaggen, "map_017_napirasu", LightElement.Light, DialogueStyle.ForgeProud,
                new[] { "ringed_beard", "amianthus_apron", "smoky_quartz_goggles" }, new string[0], "elamite"),
            new RaceDefinition("tunnel_kobold", RaceFamily.Fantastic, MapRegion.GrutasDoLimiar, "map_022_mercado_tuneis", LightElement.SereneAir, DialogueStyle.BarteringChatter,
                new[] { "scales", "big_eyes", "fan_ears" }, new string[0], "sogdian"),
            new RaceDefinition("aratta_bull_folk", RaceFamily.Fantastic, MapRegion.CostaDasMoncoes, "map_026_auroques", LightElement.LivingFire, DialogueStyle.HospitableBoom,
                new[] { "curved_horns", "thick_coat", "labrys" }, new[] { "Enmerkar", "Lugalbanda" }, "sumerian"),
            new RaceDefinition("kirta_sea_folk", RaceFamily.Fantastic, MapRegion.CostaDasMoncoes, "map_028_kirta", LightElement.LivingWater, DialogueStyle.TidalCalm,
                new[] { "mother_of_pearl_skin", "gills", "webbed_fingers" }, new[] { "Kirta", "Aqhat" }, "ugaritic"),
            new RaceDefinition("tarchon_troll", RaceFamily.Fantastic, MapRegion.DeltaDeJhilpur, "map_033_tarchon", LightElement.SereneAir, DialogueStyle.SlowGentle,
                new[] { "bark_hide", "moss_growth", "three_meters_tall" }, new[] { "Tarchon", "Tages" }, "etruscan"),
            new RaceDefinition("soslan_centaur", RaceFamily.Fantastic, MapRegion.PlanicieDeMingcheng, null, LightElement.LivingWind, DialogueStyle.OathBound,
                new[] { "horse_body", "braided_mane", "bone_beads" }, new[] { "Soslan" }, "ossetian"),
            new RaceDefinition("hill_giant", RaceFamily.Fantastic, MapRegion.PlanicieDeMingcheng, null, LightElement.LivingFire, DialogueStyle.RememberingEcho,
                new[] { "giant_stature", "repentant_marks" }, new string[0], "manichaean_book_of_giants"),
            new RaceDefinition("trunk_folk", RaceFamily.Fantastic, MapRegion.PlanicieDeMingcheng, null, LightElement.LivingWater, DialogueStyle.SlowGentle,
                new[] { "bark_body", "leaf_crown" }, new string[0], "tocharian"),
            new RaceDefinition("satyr", RaceFamily.Fantastic, MapRegion.TetoDoMundo, null, LightElement.LivingWind, DialogueStyle.Riddling,
                new[] { "goat_legs", "small_horns", "reed_pipes" }, new string[0], "phrygian"),
        };

        public static RaceDefinition Get(string id) => Races.FirstOrDefault(r => r.Id == id);

        public static IEnumerable<RaceDefinition> InRegion(MapRegion region) => Races.Where(r => r.HomeRegion == region);
    }
}
