#if UNITY_EDITOR || RAPADURA_TOOLS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>
    /// Reads the item tables of <c>Docs/Housing/CatalogoCasa.md</c> and turns them into <see cref="HouseItemDefinition"/>s,
    /// then into the generated C# table <c>HouseCatalogData.g.cs</c> (menu "Rapadura ▸ Housing ▸ Import House Catalog").
    /// Unity-free and deterministic: the same markdown always yields byte-identical code, so re-importing is a no-op
    /// (a test regenerates and compares with the committed file). Editor / tools only.
    /// <para>The markdown has no footprint, function or surface columns; they are inferred here from the "Encaixe"
    /// column, the category and the first word of the name (tables below), and can be tuned in one place.</para>
    /// </summary>
    public static class HouseCatalogImport
    {
        public const string CatalogMarkdownPath = "Docs/Housing/CatalogoCasa.md";
        /// <summary>Optional "id&lt;TAB&gt;English name" overrides; without it English falls back to the pt-BR name (TODO: translate).</summary>
        public const string EnglishNamesPath = "Docs/Housing/CatalogoCasa.en.tsv";
        public const string GeneratedCodePath = "Assets/Scripts/Gameplay/Housing/Catalog/HouseCatalogData.g.cs";

        /// <summary>One raw table row.</summary>
        public sealed class Row
        {
            public string Id;
            public string Name;
            public string Rarity;
            public string Origin;
            public string Price;
            public string Comfort;
            public string Set;
            public string Slot;
            public int Line;
        }

        // ------------------------------------------------------------------
        // Parsing
        // ------------------------------------------------------------------

        public static List<Row> ParseRows(string markdown)
        {
            var rows = new List<Row>();
            string[] lines = (markdown ?? string.Empty).Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (!line.StartsWith("| `casa_", StringComparison.Ordinal))
                {
                    continue;
                }

                string[] cells = line.Trim('|').Split('|').Select(c => c.Trim()).ToArray();
                if (cells.Length != 8)
                {
                    throw new FormatException($"CatalogoCasa.md line {i + 1}: expected 8 columns, found {cells.Length}.");
                }

                rows.Add(new Row
                {
                    Id = cells[0].Trim('`'),
                    Name = cells[1],
                    Rarity = cells[2],
                    Origin = cells[3],
                    Price = cells[4],
                    Comfort = cells[5],
                    Set = cells[6],
                    Slot = cells[7],
                    Line = i + 1,
                });
            }

            return rows;
        }

        public static List<HouseItemDefinition> Parse(string markdown) => ParseRows(markdown).Select(Build).ToList();

        public static Dictionary<string, string> ParseEnglishNames(string tsv)
        {
            var names = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string raw in (tsv ?? string.Empty).Replace("\r\n", "\n").Split('\n'))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) continue;
                int tab = line.IndexOf('\t');
                if (tab <= 0) continue;
                string id = line.Substring(0, tab).Trim();
                string name = line.Substring(tab + 1).Trim();
                if (id.Length > 0 && name.Length > 0) names[id] = name;
            }

            return names;
        }

        public static HouseItemDefinition Build(Row row)
        {
            HouseItemCategory category = CategoryOf(row.Id, row.Line);
            ItemRarity rarity = RarityOf(row.Rarity, row.Line);
            HouseItemSource source = SourceOf(row.Origin, row.Line, out HouseSeason season);
            source.Label = row.Origin;
            (int basePrice, int eventPrice) = PriceOf(row.Price, row.Line);
            int comfort = int.Parse(row.Comfort, NumberStyles.Integer, CultureInfo.InvariantCulture);
            string firstWord = FirstWord(row.Name);

            HousePlacement placement = PlacementOf(row.Slot, category, firstWord, row.Name, row.Line);
            HouseLayer layer = HousePlacementRules.DefaultLayer(new HouseItemDefinition { _placement = placement });
            (int fx, int fy) = FootprintOf(layer, category, firstWord, row.Name);
            int rotation = layer == HouseLayer.Wall ? 0 : layer == HouseLayer.Surface ? 45 : 90;
            HouseItemFunction function = FunctionOf(row.Id, layer, firstWord, out int functionParam);
            int surfaceSlots = SurfaceSlotsOf(layer, firstWord);

            bool accountBound = source.Type == HouseItemSourceType.Achievement;
            bool keepsake = source.Type == HouseItemSourceType.HouseQuest || category == HouseItemCategory.Trophy;
            bool sellable = !accountBound && !keepsake && rarity != ItemRarity.Legendary;
            bool tradeable = !accountBound && source.Type != HouseItemSourceType.HouseQuest;
            int minLevel = MinLevelOf(firstWord);

            return new HouseItemDefinition(row.Id, row.Name, category, rarity, placement, fx, fy, rotation, surfaceSlots, function,
                functionParam, comfort, SetIdOf(row.Set), basePrice, eventPrice, sellable, tradeable, accountBound, minLevel, season, source);
        }

        // ------------------------------------------------------------------
        // Columns
        // ------------------------------------------------------------------

        private static readonly Dictionary<string, HouseItemCategory> Categories = new Dictionary<string, HouseItemCategory>
        {
            { "sala", HouseItemCategory.LivingRoom }, { "quarto", HouseItemCategory.Bedroom }, { "cozinha", HouseItemCategory.Kitchen },
            { "banheiro", HouseItemCategory.Bathroom }, { "quintal", HouseItemCategory.Yard }, { "curral", HouseItemCategory.Corral },
            { "luz", HouseItemCategory.Lighting }, { "parede", HouseItemCategory.WallDecor }, { "tapete", HouseItemCategory.Rug },
            { "cortina", HouseItemCategory.Curtain }, { "planta", HouseItemCategory.Plant }, { "musica", HouseItemCategory.Music },
            { "brinquedo", HouseItemCategory.Toy }, { "regional", HouseItemCategory.Regional }, { "trofeu", HouseItemCategory.Trophy },
            { "sazonal", HouseItemCategory.Seasonal }, { "familia", HouseItemCategory.Family }, { "util", HouseItemCategory.Utility },
        };

        /// <summary>Catalog section of a category (the id's middle part).</summary>
        public static string CategoryToken(HouseItemCategory category) => Categories.First(pair => pair.Value == category).Key;

        private static HouseItemCategory CategoryOf(string id, int line)
        {
            string[] parts = id.Split('_');
            if (parts.Length == 3 && parts[0] == "casa" && Categories.TryGetValue(parts[1], out HouseItemCategory category) && parts[2].Length == 3 && parts[2].All(char.IsDigit))
            {
                return category;
            }

            throw new FormatException($"CatalogoCasa.md line {line}: malformed id '{id}'.");
        }

        private static ItemRarity RarityOf(string text, int line)
        {
            switch (text)
            {
                case "Comum": return ItemRarity.Common;
                case "Incomum": return ItemRarity.Uncommon;
                case "Raro": return ItemRarity.Rare;
                case "Épico": return ItemRarity.Epic;
                case "Lendário": return ItemRarity.Legendary;
                default: throw new FormatException($"CatalogoCasa.md line {line}: unknown rarity '{text}'.");
            }
        }

        private static (int Gold, int Event) PriceOf(string text, int line)
        {
            if (text == "—") return (0, 0);
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int gold)) return (gold, 0);
            // Event currency: "— (40 oferendas)" (the Bema offerings); the old "— (40 prendas)" still parses.
            foreach (string suffix in EventPriceSuffixes)
            {
                if (!text.StartsWith("— (", StringComparison.Ordinal) || !text.EndsWith(suffix, StringComparison.Ordinal)) continue;
                string number = text.Substring(3, text.Length - 3 - suffix.Length);
                if (int.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out int offerings)) return (0, offerings);
            }

            throw new FormatException($"CatalogoCasa.md line {line}: unknown price '{text}'.");
        }

        private static readonly string[] EventPriceSuffixes = { " oferendas)", " prendas)" };

        /// <summary>
        /// Set names of the Dawn Isle re-theme (WorldBible_Maniqueismo.md §3.20) → the set ids, which never change. Names not
        /// listed here (kept ones like "Oficina do Ícaro", and the old names) fall back to "set_" + slug.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> SetIds = new Dictionary<string, string>
        {
            { "Iwan da Vó Edna", "set_varanda_vo_zefa" }, { "Caravana do Vô Jorge", "set_tropeiro_vo_chico" },
            { "Moinho do Pomar", "set_casa_farinha" }, { "Cantinho do Ari", "set_cantinho_caramelo" },
            { "Tugai do Zarafshan", "set_mata_fechada" }, { "Areias Flamejantes", "set_duna_dourada" },
            { "Picos de Litema", "set_pico_nevado" }, { "Tufo Rosa das Lamparinas", "set_cristal_cavernas" },
            { "Bazar da Nana", "set_feira_livre" }, { "Iluminuras e Papel Recortado", "set_cordel_xilogravura" },
            { "Horta dos Melões", "set_horta_caprichada" }, { "Bema", "set_arraia" },
            { "Noite de Yalda", "set_natal_sertanejo" }, { "Festa das Lanternas", "set_frevo_maracatu" },
        };

        public static string SetIdOf(string text)
        {
            if (text == "—" || string.IsNullOrEmpty(text)) return string.Empty;
            return SetIds.TryGetValue(text, out string id) ? id : "set_" + Slug(text);
        }

        private static HousePlacement PlacementOf(string slot, HouseItemCategory category, string firstWord, string name, int line)
        {
            switch (slot)
            {
                case "Piso":
                    if (category == HouseItemCategory.Rug || RugWords.Contains(firstWord)) return HousePlacement.Rug;
                    // Benches and log furniture may also stand outside (§2.3).
                    if (OutdoorFloorWords.Contains(firstWord) || name.IndexOf(" de tora", StringComparison.Ordinal) >= 0)
                        return HousePlacement.Floor | HousePlacement.Yard;
                    return HousePlacement.Floor;
                case "Parede": return HousePlacement.Wall;
                case "Teto": return HousePlacement.Ceiling;
                case "Superfície": return HousePlacement.Surface;
                case "Quintal": return HousePlacement.Yard;
                default: throw new FormatException($"CatalogoCasa.md line {line}: unknown slot '{slot}'.");
            }
        }

        // ------------------------------------------------------------------
        // Sources
        // ------------------------------------------------------------------

        public const string GeneralStoreShopId = "shop_general_store";

        public static readonly IReadOnlyDictionary<string, string> EnemyIds = new Dictionary<string, string>
        {
            { "Lobo da Mata", "enemy_forest_wolf" }, { "Aranha da Floresta", "enemy_forest_spider" },
            { "Lobo Alfa (chefe)", "enemy_forest_alpha_wolf" }, { "Escorpião das Dunas", "enemy_desert_scorpion" },
            { "Guardião da Ruína (chefe)", "enemy_desert_ruin_guardian" }, { "Golem de Pedra", "enemy_mountain_stone_golem" },
            { "Senhor da Guerra da Montanha (chefe)", "enemy_mountain_warlord" }, { "Enxame de Morcegos", "enemy_cave_bat_swarm" },
            { "Habitante Ancestral (chefe)", "enemy_cave_dweller" },
            // Dawn Isle names (WorldBible_Maniqueismo.md §3.2–3.3); the old names above stay so older tables still resolve.
            { "Lobo do Tugai", "enemy_forest_wolf" }, { "Aranha-Tecelã Apagada", "enemy_forest_spider" },
            { "Kök-Böri, o Lobo Celeste (chefe)", "enemy_forest_alpha_wolf" }, { "Escorpião das Areias Flamejantes", "enemy_desert_scorpion" },
            { "O Grande Ushabti (chefe)", "enemy_desert_ruin_guardian" }, { "Ushabti de Pedra", "enemy_mountain_stone_golem" },
            { "Hahya, o Gigante Guerreiro (chefe)", "enemy_mountain_warlord" }, { "Enxame de Morcegos do Limiar", "enemy_cave_bat_swarm" },
            { "Tages, o Habitante Ancestral (chefe)", "enemy_cave_dweller" },
        };

        public static readonly IReadOnlyDictionary<string, string> RegionQuestIds = new Dictionary<string, string>
        {
            { "Uivos na Trilha", "quest_region_forest_howling_trail" }, { "Teias no Roçado", "quest_region_forest_spider_webs" },
            { "Teias na Horta de Ervas", "quest_region_forest_spider_webs" },
            { "Ferrões no Caminho", "quest_region_desert_stinger_road" }, { "Reserva pra Travessia", "quest_region_desert_crossing_reserve" },
            { "Pedras que Andam", "quest_region_mountain_walking_stones" }, { "A Passagem da Mina", "quest_region_mountain_mine_pass" },
            { "Lamparinas Apagadas", "quest_region_caves_dark_lamps" }, { "A Última Lamparina", "quest_region_caves_last_lantern" },
        };

        public static readonly IReadOnlyDictionary<string, string> DailyQuestIds = new Dictionary<string, string>
        {
            { "Um Prato Quente", "quest_daily_maria_visit_survivor" }, { "Noite de Fogueira", "quest_daily_family_campfire" },
            { "A Cesta de Ervas da Maria", "quest_daily_maria_herb_basket" }, { "A Lavine e os Morcegos", "quest_daily_lavine_bat_chase" },
        };

        /// <summary>pt-BR achievement titles (AchievementTexts) → achievement ids.</summary>
        public static readonly IReadOnlyDictionary<string, string> AchievementIds = new Dictionary<string, string>
        {
            { "Primeiro Sangue", "ach_first_blood" }, { "Caçador do Sertão", "ach_monster_hunter" }, { "Gigante Tombado", "ach_boss_slayer" },
            { "Mão Amiga", "ach_first_quest" }, { "Calejado", "ach_level_10" }, { "Feito à Mão", "ach_first_craft" },
            { "Mestre Artesão", "ach_master_crafter" }, { "Dom Aprimorado", "ach_skill_adept" }, { "Em Sintonia", "ach_first_combo" },
            { "Desbravador", "ach_explorer" }, { "Bom de Negócio", "ach_merchant" }, { "Olho do Vento", "ach_hero_maithe" },
            { "Luz que Não se Apaga", "ach_hero_maria" }, { "Ecos de Casa", "ach_hero_joaquim" }, { "Engrenagens Antigas", "ach_hero_icaro" },
            { "Coragem da Caçula", "ach_hero_lavine" }, { "Vento e Engrenagem", "ach_siblings_wind_gears" }, { "Escudo e Luz", "ach_siblings_shield_light" },
            { "Prima de Todos", "ach_cousin_of_all" }, { "Juntos, ou Nada Feito", "ach_family_reunited" }, { "A Luz Volta para Casa", "ach_campaign_complete" },
            { "Acender de Novo", "ach_new_journey_plus" }, { "Caderno da Vó, de Capa a Capa", "ach_bestiary_complete" },
            // Dawn Isle titles (the old ones above stay).
            { "Primeira Faísca", "ach_first_blood" }, { "Caçador da Ilha da Aurora", "ach_monster_hunter" },
            { "Lembranças do Vô Jorge", "ach_relics_all" },
        };

        public static readonly IReadOnlyDictionary<string, (string Id, HouseSeason Season)> Events = new Dictionary<string, (string, HouseSeason)>
        {
            { "Festa Junina", ("event_festa_junina", HouseSeason.Junina) },
            { "Natal Sertanejo", ("event_natal_sertanejo", HouseSeason.Natal) },
            { "Carnaval", ("event_carnaval", HouseSeason.Carnaval) },
            // Dawn Isle festivals (§3.15, §3.20): same event ids and HouseSeason values, new names.
            { "Bema", ("event_festa_junina", HouseSeason.Junina) },
            { "Noite de Yalda", ("event_natal_sertanejo", HouseSeason.Natal) },
            { "Festa das Lanternas", ("event_carnaval", HouseSeason.Carnaval) },
        };

        public static readonly IReadOnlyDictionary<string, string> CraftStations = new Dictionary<string, string>
        {
            { "Bancada", "workbench" }, { "Forja", "forge" },
        };

        private static readonly HashSet<string> Honorifics = new HashSet<string> { "mestre", "dona", "seu", "comadre" };

        /// <summary>
        /// Dawn Isle shop names (CityTexts / CityShopTexts, WorldBible_Maniqueismo.md §3.1) → the shop ids, which never change.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> ShopIds = new Dictionary<string, string>
        {
            { "Armazém Geral", GeneralStoreShopId },
            { "Mercearia do Zaku", "shop_mercearia_lazaro" }, { "Bazar de Linhas da Mariam", "shop_armarinho_cida" },
            { "Bazar da Nana", "shop_feira_livre" }, { "Viveiro da Nana", "shop_viveiro_nenem" },
            { "Selaria do Vandak", "shop_selaria_vava" }, { "Forja do Mestre Mihr", "shop_forja_tonho" },
            { "Lamparinas da Parthenos", "shop_lamparinas_benedita" }, { "Caravana do Ammo", "shop_mascate_bento" },
            { "Estrebaria do Srosh", "shop_haras_vila" }, { "Ferraria do Mestre Adamas", "shop_ferraria_severino" },
            { "Marcenaria do Mestre Omóforo", "shop_marcenaria_damiao" }, { "Costureira Mestra Narisah", "shop_costureira_fatima" },
            { "Casa de Música do Mestre Xroshtag", "shop_casa_musica_bene" }, { "Olaria da Náfsha", "shop_olaria_rosa" },
        };

        /// <summary>
        /// Shop id: the Dawn Isle names map through <see cref="ShopIds"/>; any other name follows the old city-shop convention
        /// (<c>CityShopCatalog</c>: "Armarinho da Dona Cida" → <c>shop_armarinho_cida</c>), so the old labels still resolve.
        /// </summary>
        public static string ShopIdOf(string shopName)
        {
            if (shopName != null && ShopIds.TryGetValue(shopName, out string id)) return id;
            return "shop_" + string.Join("_", Slug(shopName).Split('_').Where(w => !Honorifics.Contains(w)));
        }

        /// <summary>"Segredo do mapa (…)" places renamed in the re-theme → their unchanged secret ids.</summary>
        public static readonly IReadOnlyDictionary<string, string> SecretIds = new Dictionary<string, string>
        {
            { "moinho do pomar do Vô Jorge", "secret_casa_farinha_vo_chico" },
        };

        public static string HouseQuestIdOf(string title) => "house_quest_" + Slug(title);

        private static HouseItemSource SourceOf(string origin, int line, out HouseSeason season)
        {
            season = HouseSeason.None;
            string backtick = BacktickId(origin);

            if (TryAfter(origin, "Loja: ", out string shop)) return new HouseItemSource(HouseItemSourceType.Shop, ShopIdOf(shop), 1);
            if (TryAfter(origin, "Drop: ", out string enemy) && EnemyIds.TryGetValue(enemy, out string enemyId))
                return new HouseItemSource(HouseItemSourceType.Drop, enemyId, 3);
            if (TryAfter(origin, "Missão da Casa: ", out string houseQuest)) return new HouseItemSource(HouseItemSourceType.HouseQuest, HouseQuestIdOf(houseQuest), 1);
            if (origin.StartsWith("Missão", StringComparison.Ordinal))
            {
                if (backtick != null) return new HouseItemSource(HouseItemSourceType.Quest, backtick, 1);
                if (TryAfter(origin, "Missão: ", out string title) && RegionQuestIds.TryGetValue(title, out string questId))
                    return new HouseItemSource(HouseItemSourceType.Quest, questId, 1);
            }

            if (TryAfter(origin, "Diária: ", out string daily) && DailyQuestIds.TryGetValue(daily, out string dailyId))
                return new HouseItemSource(HouseItemSourceType.DailyQuest, dailyId, 1);
            if (TryAfter(origin, "Conquista: ", out string achievement))
            {
                if (backtick != null) return new HouseItemSource(HouseItemSourceType.Achievement, backtick, 1);
                if (AchievementIds.TryGetValue(achievement, out string achievementId)) return new HouseItemSource(HouseItemSourceType.Achievement, achievementId, 1);
            }

            if (TryAfter(origin, "Evento: ", out string evt) && Events.TryGetValue(evt, out (string Id, HouseSeason Season) found))
            {
                season = found.Season;
                return new HouseItemSource(HouseItemSourceType.Event, found.Id, 1);
            }

            if (TryAfter(origin, "Crafting: ", out string station) && CraftStations.TryGetValue(station, out string stationId))
                return new HouseItemSource(HouseItemSourceType.Craft, stationId, 1);
            if (origin.StartsWith("Segredo do mapa", StringComparison.Ordinal))
            {
                int open = origin.IndexOf('(');
                string where = open >= 0 ? origin.Substring(open + 1).TrimEnd(')') : "mapa";
                return new HouseItemSource(HouseItemSourceType.Secret, SecretIds.TryGetValue(where, out string secretId) ? secretId : "secret_" + Slug(where), 1);
            }

            throw new FormatException($"CatalogoCasa.md line {line}: unknown origin '{origin}'.");
        }

        private static bool TryAfter(string text, string prefix, out string rest)
        {
            if (text.StartsWith(prefix, StringComparison.Ordinal))
            {
                rest = text.Substring(prefix.Length).Trim();
                return true;
            }

            rest = null;
            return false;
        }

        private static string BacktickId(string text)
        {
            int start = text.IndexOf('`');
            int end = start >= 0 ? text.IndexOf('`', start + 1) : -1;
            return end > start ? text.Substring(start + 1, end - start - 1) : null;
        }

        // ------------------------------------------------------------------
        // Inference (footprint, function, surface spots)
        // ------------------------------------------------------------------

        private static readonly HashSet<string> RugWords = new HashSet<string> { "Tapete", "Passadeira", "Esteira", "Capacho" };
        private static readonly HashSet<string> OutdoorFloorWords = new HashSet<string> { "Banco", "Tora", "Tronco" };
        private static readonly HashSet<string> BedWords = new HashSet<string> { "Cama", "Cama-forte", "Cama-oficina", "Beliche", "Catre", "Colchão" };
        private static readonly HashSet<string> BigYardWords = new HashSet<string> { "Chafariz", "Carro", "Curral", "Palco", "Barquinha" };

        private static readonly Dictionary<string, (int, int)> FloorSizes = new Dictionary<string, (int, int)>
        {
            { "Cama", (4, 3) }, { "Cama-forte", (4, 3) }, { "Cama-oficina", (4, 3) }, { "Beliche", (4, 2) }, { "Catre", (4, 2) }, { "Colchão", (4, 2) },
            { "Berço", (3, 2) }, { "Caminha", (2, 2) }, { "Mesa", (3, 2) }, { "Mesinha", (1, 1) }, { "Criado-mudo", (1, 1) },
            { "Cadeira", (1, 1) }, { "Banquinho", (1, 1) }, { "Mocho", (1, 1) }, { "Tamborete", (1, 1) }, { "Pufe", (1, 1) },
            { "Poltrona", (2, 2) }, { "Sofá", (4, 2) }, { "Namoradeira", (3, 2) }, { "Banco", (3, 1) }, { "Baú-banco", (3, 1) },
            { "Baú", (2, 1) }, { "Arca", (2, 1) }, { "Baú-correio", (2, 1) }, { "Caixote", (2, 1) },
            { "Guarda-roupa", (3, 1) }, { "Cristaleira", (3, 1) }, { "Estante", (3, 1) }, { "Penteadeira", (3, 1) }, { "Cômoda", (3, 1) },
            { "Aparador", (3, 1) }, { "Gaveteiro", (2, 1) }, { "Escrivaninha", (3, 2) }, { "Balcão", (4, 1) }, { "Bancada", (4, 2) },
            { "Fogão", (3, 2) }, { "Fogareiro", (1, 1) }, { "Forno", (2, 2) }, { "Lareira", (3, 2) }, { "Geladeira", (2, 2) }, { "Pia", (2, 1) },
            { "Tear", (3, 2) }, { "Banheira", (4, 2) }, { "Tina", (2, 2) }, { "Biombo", (3, 1) }, { "Vitrine", (3, 1) }, { "Pedestal", (2, 2) },
            { "Estátua", (2, 2) }, { "Televisão", (2, 1) }, { "Tora", (2, 1) }, { "Tronco", (2, 1) }, { "Cesto", (1, 1) }, { "Jarro", (1, 1) },
            { "Talha", (1, 1) }, { "Pilão", (1, 1) }, { "Tigela", (1, 1) }, { "Penico", (1, 1) }, { "Bola", (1, 1) }, { "Bolinha", (1, 1) },
            { "Árvore", (2, 2) }, { "Ipê-roxo", (2, 2) }, { "Mandacaru", (2, 2) }, { "Costela-de-adão", (2, 2) },
            // Dawn Isle first words (same sizes as the words they replaced).
            { "Tandir", (3, 2) }, { "Saxaul", (2, 2) }, { "Romãzeira-anã", (2, 2) }, { "Choupo-turanga", (2, 2) },
        };

        private static readonly Dictionary<string, (int, int)> RugSizes = new Dictionary<string, (int, int)>
        {
            { "Tapete", (4, 3) }, { "Passadeira", (6, 2) }, { "Esteira", (4, 2) }, { "Capacho", (2, 1) },
        };

        private static readonly Dictionary<string, (int, int)> WallSizes = new Dictionary<string, (int, int)>
        {
            { "Cortina", (2, 3) }, { "Persiana", (2, 2) }, { "Rede", (4, 1) }, { "Quadro", (2, 2) }, { "Retrato", (1, 2) }, { "Prateleira", (3, 1) },
            { "Estante", (3, 3) }, { "Espelho", (2, 3) }, { "Placa", (2, 1) }, { "Relógio", (1, 2) }, { "Chuveiro", (1, 2) }, { "Painel", (3, 2) },
            { "Tapeçaria", (3, 2) }, { "Mapa", (3, 2) }, { "Mural", (4, 2) }, { "Estandarte", (2, 3) }, { "Bandeira", (2, 2) }, { "Oratório", (2, 2) },
            { "Medalha", (1, 1) }, { "Meia", (1, 1) }, { "Colher", (1, 1) }, { "Fita", (1, 1) }, { "Cabide", (1, 1) }, { "Arandela", (1, 1) },
            { "Tocha", (1, 1) }, { "Carta", (1, 1) }, { "Pegada", (1, 1) }, { "Chapéu", (1, 1) }, { "Máscara", (1, 1) }, { "Cabeça", (2, 2) },
            { "Nicho", (2, 2) },
        };

        private static readonly Dictionary<string, (int, int)> CeilingSizes = new Dictionary<string, (int, int)>
        {
            { "Lustre", (2, 2) }, { "Varal", (6, 1) }, { "Bandeirinhas", (6, 1) }, { "Pisca-pisca", (6, 1) }, { "Serpentinas", (6, 1) },
            { "Mosquiteiro", (4, 3) }, { "Sombrinhas", (3, 2) }, { "Cordão", (6, 1) },
        };

        private static readonly Dictionary<string, (int, int)> YardSizes = new Dictionary<string, (int, int)>
        {
            { "Curral", (4, 3) }, { "Palco", (4, 3) }, { "Barraca", (3, 2) }, { "Galinheiro", (2, 2) }, { "Chiqueirinho", (2, 2) }, { "Horta", (3, 2) },
            { "Canteiro", (2, 1) }, { "Caramanchão", (3, 3) }, { "Pergolado", (3, 3) }, { "Tenda", (3, 3) }, { "Laguinho", (2, 2) }, { "Chafariz", (2, 2) },
            { "Carro", (3, 2) }, { "Carroça", (3, 2) }, { "Poço", (2, 2) }, { "Cisterna", (2, 2) }, { "Pé", (2, 2) }, { "Árvore", (2, 2) },
            { "Coqueiro", (2, 2) }, { "Bananeira", (2, 2) }, { "Forja", (2, 2) }, { "Forno", (2, 1) }, { "Cerca", (4, 1) }, { "Caminho", (1, 3) },
            { "Varal", (3, 1) }, { "Rede", (3, 1) }, { "Balanço", (2, 1) }, { "Mesa", (2, 2) }, { "Banco", (2, 1) }, { "Baia", (2, 2) },
            { "Manjedoura", (2, 1) }, { "Cocho", (2, 1) }, { "Tanque", (2, 1) }, { "Mastro", (1, 1) }, { "Porteira", (2, 1) },
            { "Figueira", (2, 2) }, { "Tamareira", (2, 2) }, { "Portão", (2, 1) }, { "Barquinha", (3, 2) },
        };

        private static (int, int) FootprintOf(HouseLayer layer, HouseItemCategory category, string firstWord, string name)
        {
            switch (layer)
            {
                case HouseLayer.Rug: return RugSizes.TryGetValue(firstWord, out (int, int) rug) ? rug : (3, 2);
                case HouseLayer.Wall: return WallSizes.TryGetValue(firstWord, out (int, int) wall) ? wall : (2, 2);
                case HouseLayer.Ceiling: return CeilingSizes.TryGetValue(firstWord, out (int, int) ceiling) ? ceiling : (1, 1);
                case HouseLayer.Surface: return (1, 1);
                case HouseLayer.Yard: return YardSizes.TryGetValue(firstWord, out (int, int) yard) ? yard : (1, 1);
                default:
                    if (FloorSizes.TryGetValue(firstWord, out (int, int) floor)) return floor;
                    return category == HouseItemCategory.Plant ? (1, 1) : (2, 2);
            }
        }

        private static readonly Dictionary<string, (HouseItemFunction, int)> FunctionsById = new Dictionary<string, (HouseItemFunction, int)>
        {
            { "casa_util_001", (HouseItemFunction.Chest, 0) }, { "casa_util_002", (HouseItemFunction.Chest, 10) },
            { "casa_util_003", (HouseItemFunction.Chest, 15) }, { "casa_util_004", (HouseItemFunction.Chest, 20) },
            { "casa_util_005", (HouseItemFunction.VillageChestLink, 0) }, { "casa_util_006", (HouseItemFunction.Wardrobe, 3) },
            { "casa_util_007", (HouseItemFunction.Wardrobe, 6) }, { "casa_util_011", (HouseItemFunction.Bench, 0) },
            { "casa_util_012", (HouseItemFunction.Bench, 1) }, { "casa_cozinha_001", (HouseItemFunction.Stove, 0) },
            { "casa_util_013", (HouseItemFunction.Forge, 0) }, { "casa_util_014", (HouseItemFunction.Alchemy, 0) },
            { "casa_util_015", (HouseItemFunction.Loom, 0) }, { "casa_util_016", (HouseItemFunction.DyeTub, 0) },
            { "casa_util_019", (HouseItemFunction.BestiaryShelf, 0) }, { "casa_util_020", (HouseItemFunction.RelicCase, 0) },
            { "casa_util_021", (HouseItemFunction.TrophyStand, 0) }, { "casa_util_022", (HouseItemFunction.TrophyStand, 0) },
            { "casa_util_023", (HouseItemFunction.GuestBook, 0) }, { "casa_util_024", (HouseItemFunction.Mailbox, 0) },
            { "casa_util_026", (HouseItemFunction.Bell, 0) }, { "casa_util_027", (HouseItemFunction.NoticeBoard, 0) },
            { "casa_util_031", (HouseItemFunction.Helper, 0) }, { "casa_curral_001", (HouseItemFunction.PetBed, 0) },
            { "casa_curral_002", (HouseItemFunction.PetBed, 0) }, { "casa_quintal_001", (HouseItemFunction.Well, 0) },
            { "casa_quintal_037", (HouseItemFunction.TrainingDummy, 0) },
        };

        private static HouseItemFunction FunctionOf(string id, HouseLayer layer, string firstWord, out int param)
        {
            param = 0;
            if (FunctionsById.TryGetValue(id, out (HouseItemFunction Function, int Param) known))
            {
                param = known.Param;
                return known.Function;
            }

            if (layer == HouseLayer.Floor && BedWords.Contains(firstWord)) return HouseItemFunction.Bed;
            if (layer == HouseLayer.Yard && (firstWord == "Canteiro" || firstWord == "Horta")) return HouseItemFunction.Planter;
            return HouseItemFunction.None;
        }

        private static readonly Dictionary<string, int> FloorSurfaceSlots = new Dictionary<string, int>
        {
            { "Mesa", 4 }, { "Mesinha", 1 }, { "Criado-mudo", 1 }, { "Estante", 9 }, { "Aparador", 3 }, { "Cômoda", 3 }, { "Penteadeira", 3 },
            { "Balcão", 4 }, { "Escrivaninha", 3 }, { "Bancada", 3 }, { "Cristaleira", 3 }, { "Gaveteiro", 2 }, { "Vitrine", 3 },
        };

        private static readonly Dictionary<string, int> WallSurfaceSlots = new Dictionary<string, int>
        {
            { "Prateleira", 3 }, { "Estante", 6 }, { "Oratório", 1 }, { "Nicho", 1 },
        };

        private static int SurfaceSlotsOf(HouseLayer layer, string firstWord)
        {
            if (layer == HouseLayer.Floor && FloorSurfaceSlots.TryGetValue(firstWord, out int floor)) return floor;
            if (layer == HouseLayer.Wall && WallSurfaceSlots.TryGetValue(firstWord, out int wall)) return wall;
            if (layer == HouseLayer.Yard && firstWord == "Mesa") return 4;
            return 0;
        }

        /// <summary>"Itens grandes (chafariz, barquinha de luz) pedem N3+" (§12.1).</summary>
        private static int MinLevelOf(string firstWord) => BigYardWords.Contains(firstWord) ? 3 : 1;

        public static string FirstWord(string name)
        {
            string trimmed = (name ?? string.Empty).Trim();
            int space = trimmed.IndexOf(' ');
            return (space < 0 ? trimmed : trimmed.Substring(0, space)).TrimEnd(',', '.');
        }

        private static readonly HashSet<string> StopWords = new HashSet<string> { "de", "da", "do", "das", "dos", "e", "na", "no", "nas", "nos", "pra", "a", "o", "as", "os" };

        /// <summary>"Varanda da Vó Zefa" → "varanda_vo_zefa" (no accents, no connectors).</summary>
        public static string Slug(string text)
        {
            string decomposed = (text ?? string.Empty).Normalize(NormalizationForm.FormD);
            var plain = new StringBuilder();
            foreach (char c in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                plain.Append(char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : ' ');
            }

            return string.Join("_", plain.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Where(w => !StopWords.Contains(w)));
        }

        // ------------------------------------------------------------------
        // Code generation
        // ------------------------------------------------------------------

        /// <summary>The whole generated file, "\n" line endings, deterministic.</summary>
        public static string GenerateCode(IReadOnlyList<HouseItemDefinition> items, IReadOnlyDictionary<string, string> englishNames = null)
        {
            var sb = new StringBuilder();
            sb.Append("// <auto-generated>\n");
            sb.Append("// Generated by \"Rapadura > Housing > Import House Catalog\" (HouseCatalogImporter) from ").Append(CatalogMarkdownPath).Append(".\n");
            sb.Append("// Do not edit by hand: change the markdown (or the inference tables in HouseCatalogImport.cs) and re-import.\n");
            sb.Append("// </auto-generated>\n");
            sb.Append("using System.Collections.Generic;\n");
            sb.Append("using Rapadura.Gameplay.Items;\n\n");
            sb.Append("namespace Rapadura.Gameplay.Housing\n{\n");
            sb.Append("    public static partial class HouseCatalog\n    {\n");
            sb.Append("        public const int GeneratedItemCount = ").Append(items.Count.ToString(CultureInfo.InvariantCulture)).Append(";\n\n");
            sb.Append("        // id, pt-BR name, category, rarity, placement, footprint x, y, rotation step, surface slots, function, function param,\n");
            sb.Append("        // comfort, set, gold price, event price, sellable, tradeable, account bound, min house level, season, sources\n");
            sb.Append("        private static HouseItemDefinition[] BuildItems()\n        {\n            return new[]\n            {\n");
            foreach (HouseItemDefinition item in items)
            {
                sb.Append("                new HouseItemDefinition(")
                    .Append(Quote(item.ItemId)).Append(", ")
                    .Append(Quote(item.NamePtBR)).Append(", ")
                    .Append("HouseItemCategory.").Append(item.Category).Append(", ")
                    .Append("ItemRarity.").Append(item.Rarity).Append(", ")
                    .Append(PlacementCode(item.Placement)).Append(", ")
                    .Append(Int(item.FootprintX)).Append(", ").Append(Int(item.FootprintY)).Append(", ")
                    .Append(Int(item.RotationStep)).Append(", ").Append(Int(item.SurfaceSlots)).Append(", ")
                    .Append("HouseItemFunction.").Append(item.Function).Append(", ").Append(Int(item.FunctionParam)).Append(", ")
                    .Append(Int(item.Comfort)).Append(", ").Append(Quote(item.SetId)).Append(", ")
                    .Append(Int(item.BasePrice)).Append(", ").Append(Int(item.EventPrice)).Append(", ")
                    .Append(Bool(item.Sellable)).Append(", ").Append(Bool(item.Tradeable)).Append(", ").Append(Bool(item.AccountBound)).Append(", ")
                    .Append(Int(item.MinHouseLevel)).Append(", ").Append("HouseSeason.").Append(item.Seasonal);
                foreach (HouseItemSource source in item.Sources)
                {
                    sb.Append(", S(HouseItemSourceType.").Append(source.Type).Append(", ").Append(Quote(source.RefId)).Append(", ").Append(Int(source.Weight)).Append(", ").Append(Quote(source.Label)).Append(')');
                }

                sb.Append("),\n");
            }

            sb.Append("            };\n        }\n\n");
            sb.Append("        /// <summary>English names from ").Append(EnglishNamesPath).Append(" (missing ids fall back to the pt-BR name).</summary>\n");
            sb.Append("        private static readonly Dictionary<string, string> GeneratedEnglishNames = new Dictionary<string, string>\n        {\n");
            if (englishNames != null)
            {
                foreach (KeyValuePair<string, string> pair in englishNames.OrderBy(p => p.Key, StringComparer.Ordinal))
                {
                    sb.Append("            { ").Append(Quote(pair.Key)).Append(", ").Append(Quote(pair.Value)).Append(" },\n");
                }
            }

            sb.Append("        };\n    }\n}\n");
            return sb.ToString();
        }

        private static string PlacementCode(HousePlacement placement)
        {
            var parts = new List<string>();
            foreach (HousePlacement flag in new[] { HousePlacement.Floor, HousePlacement.Wall, HousePlacement.Ceiling, HousePlacement.Surface, HousePlacement.Yard, HousePlacement.Rug })
            {
                if ((placement & flag) != 0) parts.Add("HousePlacement." + flag);
            }

            return parts.Count == 0 ? "HousePlacement.None" : string.Join(" | ", parts);
        }

        private static string Int(int value) => value.ToString(CultureInfo.InvariantCulture);
        private static string Bool(bool value) => value ? "true" : "false";

        private static string Quote(string text)
        {
            var sb = new StringBuilder("\"");
            foreach (char c in text ?? string.Empty)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    default: sb.Append(c); break;
                }
            }

            return sb.Append('"').ToString();
        }
    }
}
#endif
