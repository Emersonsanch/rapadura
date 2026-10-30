#if UNITY_EDITOR || RAPADURA_TOOLS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Wardrobe
{
    /// <summary>
    /// Reads the tables of <c>Docs/Housing/CatalogoRoupas.md</c> (cousin sets, common clothes, accessories, dyes, weapon
    /// skins) into <see cref="WardrobeEntry"/>s and then into the generated table <c>WardrobeCatalogData.g.cs</c> (menu
    /// "Rapadura ▸ Housing ▸ Import Wardrobe Catalog"). Unity-free and deterministic like <c>HouseCatalogImport</c>, whose
    /// shop/enemy/quest/achievement name tables it reuses so both catalogs point at the same ids. Editor / tools only.
    /// </summary>
    public static class WardrobeCatalogImport
    {
        public const string CatalogMarkdownPath = "Docs/Housing/CatalogoRoupas.md";
        public const string EnglishNamesPath = "Docs/Housing/CatalogoRoupas.en.tsv";
        public const string GeneratedCodePath = "Assets/Scripts/Gameplay/Wardrobe/WardrobeCatalogData.g.cs";

        private static readonly (string Token, CharacterId Id)[] Cousins =
        {
            ("maithe", CharacterId.Maithe), ("maria", CharacterId.Maria), ("joaquim", CharacterId.Joaquim), ("icaro", CharacterId.Icaro), ("lavine", CharacterId.Lavine),
        };

        public static readonly IReadOnlyDictionary<CharacterId, string> PersonalQuestIds = new Dictionary<CharacterId, string>
        {
            { CharacterId.Maithe, "quest_maithe_windtrail" }, { CharacterId.Maria, "quest_maria_fading_light" },
            { CharacterId.Joaquim, "quest_joaquim_echoes_of_home" }, { CharacterId.Icaro, "quest_icaro_ancient_gears" },
            { CharacterId.Lavine, "quest_lavine_redtide" },
        };

        /// <summary>Dailies named only in the clothing catalog (the others come from <c>HouseCatalogImport.DailyQuestIds</c>).</summary>
        public static readonly IReadOnlyDictionary<string, string> ExtraDailyQuestIds = new Dictionary<string, string>
        {
            { "Ronda da Manhã", "quest_daily_maithe_wolf_patrol" }, { "Vigia dos Escorpiões", "quest_daily_joaquim_scorpion_watch" },
            { "Peças de Reposição", "quest_daily_icaro_ore_tinkering" },
        };

        public const string SecondForestQuestId = "quest_region_forest_spider_webs";

        /// <summary>RGB of each dye colour named in the catalog (the art reference for the tint, §9.4).</summary>
        public static readonly IReadOnlyDictionary<string, string> DyeColors = new Dictionary<string, string>
        {
            { "Azul", "2F5DA8" }, { "Vermelho-alaranjado", "D9542B" }, { "Amarelo", "E8B923" }, { "Preto-azulado", "1E2A3A" },
            { "Terracota", "B5562F" }, { "Cinza-carvão", "3A3A3C" }, { "Marrom", "7A4B2A" }, { "Vermelho intenso", "B3122E" },
            { "Verde-folha", "4C9A2A" }, { "Dourado-palha", "C9A44A" }, { "Rosa", "D9708F" }, { "Branco", "F2EFE6" },
            { "Ocre", "C2862F" }, { "Dourado-areia", "D8B26A" }, { "Azul-cristal", "5FC6E8" }, { "Verde-musgo", "5B6B2E" },
            { "Azul-celeste", "7FB7E6" }, { "Rosa-choque", "E8338B" }, { "Cobre", "B8733A" }, { "Lilás", "A98BD6" },
            { "Cinza-pedra", "8A8A84" }, { "Preto", "1A1A1A" }, { "Estampa de chita (multicor)", "E04F7A" },
            { "Vermelho-escuro com brilho", "7A0F26" }, { "Dourado brilhante", "F2C230" },
        };

        public static List<WardrobeEntry> Parse(string markdown)
        {
            var entries = new List<WardrobeEntry>();
            string[] lines = (markdown ?? string.Empty).Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (!line.StartsWith("| `", StringComparison.Ordinal)) continue;
                string[] cells = line.Trim('|').Split('|').Select(c => c.Trim()).ToArray();
                string id = cells[0].Trim('`');
                entries.Add(Build(id, cells, i + 1));
            }

            return entries;
        }

        private static WardrobeEntry Build(string id, string[] c, int line)
        {
            if (id.StartsWith("roupa_comum_", StringComparison.Ordinal))
            {
                Expect(c, 6, line);
                return Make(id, c[1], WardrobeEntryKind.Piece, SlotOf(c[2], line), null, c[3], string.Empty, "qualquer", c[4], c[5], null, line);
            }

            if (id.StartsWith("roupa_", StringComparison.Ordinal))
            {
                Expect(c, 8, line);
                return Make(id, c[1], WardrobeEntryKind.Piece, SlotOf(c[2], line), CousinOfId(id, line), c[3], c[7], c[4], c[5], c[6], null, line);
            }

            if (id.StartsWith("acess_", StringComparison.Ordinal))
            {
                Expect(c, 7, line);
                return Make(id, c[1], WardrobeEntryKind.Accessory, SlotOf(c[3], line), null, c[4], string.Empty, "qualquer", c[5], c[6], null, line);
            }

            if (id.StartsWith("tinta_", StringComparison.Ordinal))
            {
                Expect(c, 6, line);
                return Make(id, c[1], WardrobeEntryKind.Dye, AppearanceSlot.Chest, null, c[3], string.Empty, "qualquer", c[4], c[5], c[2], line);
            }

            if (id.StartsWith("visual_arma_", StringComparison.Ordinal))
            {
                Expect(c, 6, line);
                return Make(id, c[1], WardrobeEntryKind.WeaponSkin, AppearanceSlot.Weapon, CousinOfId(id, line), c[3], string.Empty, "qualquer", c[4], c[5], null, line);
            }

            throw new FormatException($"CatalogoRoupas.md line {line}: unknown id '{id}'.");
        }

        private static void Expect(string[] cells, int count, int line)
        {
            if (cells.Length != count) throw new FormatException($"CatalogoRoupas.md line {line}: expected {count} columns, found {cells.Length}.");
        }

        private static WardrobeEntry Make(string id, string name, WardrobeEntryKind kind, AppearanceSlot slot, CharacterId? owner, string rarity,
            string set, string level, string origin, string price, string color, int line)
        {
            (int min, int max) = LevelOf(level, line);
            (int gold, int prendas) = PriceOf(price, line);
            (WardrobeSourceType type, string refId) = SourceOf(origin, line);
            string colorHex = string.Empty;
            if (color != null && !DyeColors.TryGetValue(color, out colorHex)) throw new FormatException($"CatalogoRoupas.md line {line}: unknown colour '{color}'.");
            string setName = set == "—" ? string.Empty : set ?? string.Empty;
            return new WardrobeEntry(id, name, kind, slot, owner, RarityOf(rarity, line), setName, SetIdOf(setName), min, max, type, refId, origin,
                gold, prendas, color ?? string.Empty, colorHex ?? string.Empty);
        }

        public static string SetIdOf(string setName) => string.IsNullOrEmpty(setName) ? string.Empty : "conjunto_" + HouseCatalogImport.Slug(setName);

        private static CharacterId CousinOfId(string id, int line)
        {
            foreach ((string token, CharacterId cousin) in Cousins)
            {
                if (id.Contains("_" + token + "_")) return cousin;
            }

            throw new FormatException($"CatalogoRoupas.md line {line}: no cousin in id '{id}'.");
        }

        private static AppearanceSlot SlotOf(string text, int line)
        {
            switch (text)
            {
                case "Cabeça": return AppearanceSlot.Head;
                case "Peito": return AppearanceSlot.Chest;
                case "Pernas": return AppearanceSlot.Legs;
                case "Pés": return AppearanceSlot.Boots;
                case "Acessório": return AppearanceSlot.Accessory;
                case "Rosto/Pescoço": return AppearanceSlot.FaceNeck;
                case "Costas": return AppearanceSlot.Back;
                case "Caramelo": return AppearanceSlot.Caramelo;
                default: throw new FormatException($"CatalogoRoupas.md line {line}: unknown slot '{text}'.");
            }
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
                default: throw new FormatException($"CatalogoRoupas.md line {line}: unknown rarity '{text}'.");
            }
        }

        private static (int, int) LevelOf(string text, int line)
        {
            if (text == "qualquer" || string.IsNullOrEmpty(text)) return (0, 0);
            string[] parts = text.Split('–', '-');
            if (parts.Length == 2 && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int min)
                                  && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int max)) return (min, max);
            throw new FormatException($"CatalogoRoupas.md line {line}: unknown level '{text}'.");
        }

        private static (int, int) PriceOf(string text, int line)
        {
            if (text == "—") return (0, 0);
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int gold)) return (gold, 0);
            if (text.StartsWith("— (", StringComparison.Ordinal) && text.EndsWith(" prendas)", StringComparison.Ordinal)
                && int.TryParse(text.Substring(3, text.Length - 3 - " prendas)".Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out int prendas)) return (0, prendas);
            throw new FormatException($"CatalogoRoupas.md line {line}: unknown price '{text}'.");
        }

        private static string StripParen(string text)
        {
            int open = text.IndexOf(" (", StringComparison.Ordinal);
            return open >= 0 ? text.Substring(0, open).Trim() : text.Trim();
        }

        private static (WardrobeSourceType, string) SourceOf(string origin, int line)
        {
            if (origin == "Desbloqueado desde o começo") return (WardrobeSourceType.Starter, string.Empty);
            if (TryAfter(origin, "Loja: ", out string shop)) return (WardrobeSourceType.Shop, HouseCatalogImport.ShopIdOf(shop));
            if (TryAfter(origin, "Drop: ", out string enemy))
            {
                if (enemy.StartsWith("inimigos comuns", StringComparison.Ordinal)) return (WardrobeSourceType.Drop, WardrobeEntry.CommonDropRef);
                if (HouseCatalogImport.EnemyIds.TryGetValue(enemy, out string enemyId)) return (WardrobeSourceType.Drop, enemyId);
            }

            if (TryAfter(origin, "Missão da Casa: ", out string houseQuest)) return (WardrobeSourceType.HouseQuest, HouseCatalogImport.HouseQuestIdOf(houseQuest));
            if (origin.StartsWith("Missão pessoal d", StringComparison.Ordinal))
            {
                foreach ((string token, CharacterId cousin) in Cousins)
                {
                    if (HouseCatalogImport.Slug(origin).EndsWith(token, StringComparison.Ordinal)) return (WardrobeSourceType.Quest, PersonalQuestIds[cousin]);
                }
            }

            if (origin.StartsWith("Missão regional da Floresta", StringComparison.Ordinal)) return (WardrobeSourceType.Quest, SecondForestQuestId);
            if (TryAfter(origin, "Missão: ", out string title) && HouseCatalogImport.RegionQuestIds.TryGetValue(title, out string questId))
                return (WardrobeSourceType.Quest, questId);
            if (TryAfter(origin, "Diária: ", out string daily))
            {
                if (HouseCatalogImport.DailyQuestIds.TryGetValue(daily, out string dailyId)) return (WardrobeSourceType.DailyQuest, dailyId);
                if (ExtraDailyQuestIds.TryGetValue(daily, out dailyId)) return (WardrobeSourceType.DailyQuest, dailyId);
            }

            if (TryAfter(origin, "Conquista: ", out string achievement) && HouseCatalogImport.AchievementIds.TryGetValue(StripParen(achievement), out string achievementId))
                return (WardrobeSourceType.Achievement, achievementId);
            if (TryAfter(origin, "Evento: ", out string evt) && HouseCatalogImport.Events.TryGetValue(StripParen(evt), out (string Id, HouseSeason Season) found))
                return (WardrobeSourceType.Event, found.Id);
            if (TryAfter(origin, "Crafting: ", out string station))
            {
                if (station == "Bancada") return (WardrobeSourceType.Craft, "workbench");
                if (station == "Forja") return (WardrobeSourceType.Craft, "forge");
                if (station == "Fogão a lenha") return (WardrobeSourceType.Craft, "stove");
                if (station.StartsWith("Tear de mestre", StringComparison.Ordinal)) return (WardrobeSourceType.Craft, "master_loom");
                if (station.StartsWith("Tear", StringComparison.Ordinal)) return (WardrobeSourceType.Craft, "loom");
                if (station.StartsWith("Forja (", StringComparison.Ordinal)) return (WardrobeSourceType.Craft, "forge_boss");
            }

            if (origin.StartsWith("Segredo do mapa", StringComparison.Ordinal))
            {
                int start = origin.IndexOf('`');
                int end = start >= 0 ? origin.IndexOf('`', start + 1) : -1;
                if (end > start) return (WardrobeSourceType.Secret, origin.Substring(start + 1, end - start - 1));
            }

            throw new FormatException($"CatalogoRoupas.md line {line}: unknown origin '{origin}'.");
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

        // ------------------------------------------------------------------
        // Code generation
        // ------------------------------------------------------------------

        public static string GenerateCode(IReadOnlyList<WardrobeEntry> entries, IReadOnlyDictionary<string, string> englishNames = null)
        {
            var sb = new StringBuilder();
            sb.Append("// <auto-generated>\n");
            sb.Append("// Generated by \"Rapadura > Housing > Import Wardrobe Catalog\" (WardrobeCatalogImporter) from ").Append(CatalogMarkdownPath).Append(".\n");
            sb.Append("// Do not edit by hand: change the markdown (or the tables in WardrobeCatalogImport.cs) and re-import.\n");
            sb.Append("// </auto-generated>\n");
            sb.Append("using System.Collections.Generic;\n");
            sb.Append("using Rapadura.Gameplay.Characters;\n");
            sb.Append("using Rapadura.Gameplay.Items;\n\n");
            sb.Append("namespace Rapadura.Gameplay.Wardrobe\n{\n");
            sb.Append("    public static partial class WardrobeCatalog\n    {\n");
            sb.Append("        public const int GeneratedEntryCount = ").Append(entries.Count.ToString(CultureInfo.InvariantCulture)).Append(";\n\n");
            sb.Append("        // id, pt-BR name, kind, slot, owner, rarity, set name, set id, level min, max, source, source ref, origin label,\n");
            sb.Append("        // gold price, event price, colour name, colour hex\n");
            sb.Append("        private static WardrobeEntry[] BuildEntries()\n        {\n            return new[]\n            {\n");
            foreach (WardrobeEntry e in entries)
            {
                sb.Append("                new WardrobeEntry(")
                    .Append(Quote(e.Id)).Append(", ").Append(Quote(e.NamePtBR)).Append(", ")
                    .Append("WardrobeEntryKind.").Append(e.Kind).Append(", ")
                    .Append("AppearanceSlot.").Append(e.Slot).Append(", ")
                    .Append(e.Owner.HasValue ? "CharacterId." + e.Owner.Value : "null").Append(", ")
                    .Append("ItemRarity.").Append(e.Rarity).Append(", ")
                    .Append(Quote(e.SetName)).Append(", ").Append(Quote(e.SetId)).Append(", ")
                    .Append(Int(e.LevelMin)).Append(", ").Append(Int(e.LevelMax)).Append(", ")
                    .Append("WardrobeSourceType.").Append(e.Source).Append(", ").Append(Quote(e.SourceRef)).Append(", ").Append(Quote(e.SourceLabel)).Append(", ")
                    .Append(Int(e.Price)).Append(", ").Append(Int(e.EventPrice)).Append(", ")
                    .Append(Quote(e.ColorName)).Append(", ").Append(Quote(e.ColorHex))
                    .Append("),\n");
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

        private static string Int(int value) => value.ToString(CultureInfo.InvariantCulture);

        private static string Quote(string text)
        {
            var sb = new StringBuilder("\"");
            foreach (char ch in text ?? string.Empty)
            {
                switch (ch)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    default: sb.Append(ch); break;
                }
            }

            return sb.Append('"').ToString();
        }
    }
}
#endif
