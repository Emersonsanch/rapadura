using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Content;

namespace Rapadura.Gameplay.Crafting.Advanced
{
    /// <summary>
    /// en + pt-BR text of the advanced crafting (FASE 16): creature parts and runes (<c>item.&lt;id&gt;.name/.description</c>,
    /// pt-BR = the asset text), the material qualities, the enchantments of Vô Jorge, the fusion forge of Adamas and the
    /// result/check lines (<c>ui.craft.*</c>). Same self-contained pattern as <c>RefineTexts</c>: merged into the live
    /// <see cref="LocalizationManager"/> on first use (keys it already has win), pt-BR fallback without one.
    /// </summary>
    public static class CraftingTexts
    {
        public const string Title = "ui.craft.title";
        public const string Subtitle = "ui.craft.subtitle";
        /// <summary>{0} tier.</summary>
        public const string Tier = "ui.craft.tier";
        /// <summary>{0} quality name.</summary>
        public const string QualityLine = "ui.craft.quality_line";
        public const string UseTrophy = "ui.craft.use_trophy";
        /// <summary>{0} item, {1} rarity.</summary>
        public const string CraftedGear = "ui.craft.result.gear";
        /// <summary>{0} quantity, {1} item.</summary>
        public const string CraftedRunes = "ui.craft.result.runes";
        public const string Locked = "ui.craft.check.locked";
        public const string WrongCrafter = "ui.craft.check.wrong_crafter";
        public const string MissingIngredients = "ui.craft.check.missing_ingredients";
        public const string MissingGold = "ui.craft.check.missing_gold";
        public const string MissingTrophy = "ui.craft.check.missing_trophy";
        public const string NoRoom = "ui.craft.check.no_room";

        public const string ImbueTitle = "ui.craft.imbue.title";
        public const string ImbueGrandpa = "ui.craft.imbue.grandpa";
        /// <summary>{0} enchantment name, {1} fights left.</summary>
        public const string ImbueActive = "ui.craft.imbue.active";
        public const string ImbueNone = "ui.craft.imbue.none";
        public const string ImbuePaused = "ui.craft.imbue.paused";
        /// <summary>{0} enchantment name.</summary>
        public const string ImbueExpired = "ui.craft.imbue.expired";
        public const string MissingParts = "ui.craft.check.missing_parts";

        public const string FusionTitle = "ui.craft.fusion.title";
        public const string FusionKeeper = "ui.craft.fusion.keeper";
        /// <summary>{0} pieces, {1} chance %.</summary>
        public const string FusionChance = "ui.craft.fusion.chance";
        /// <summary>{0} item, {1} new rarity.</summary>
        public const string FusionSuccess = "ui.craft.fusion.success";
        /// <summary>{0} item, {1} iron.</summary>
        public const string FusionFailed = "ui.craft.fusion.failed";
        public const string FusionWrongCount = "ui.craft.fusion.wrong_count";
        public const string FusionNotSame = "ui.craft.fusion.not_same";
        public const string FusionAtCap = "ui.craft.fusion.at_cap";
        public const string FusionHasOptions = "ui.craft.fusion.has_options";

        public static string QualityKey(MaterialQuality quality) => "ui.craft.quality." + quality.ToString().ToLowerInvariant();

        private static LocalizationEntry ItemName(string id, string en, string pt) => new LocalizationEntry(ContentText.ItemNameKey(id), en, pt);
        private static LocalizationEntry ItemDesc(string id, string en, string pt) => new LocalizationEntry(ContentText.ItemDescriptionKey(id), en, pt);

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            // -- Creature parts (Resources/Items; pt-BR = the asset's text) --
            ItemName(CreatureParts.WolfFang, "Wolf Fang", "Presa de Lobo"),
            ItemDesc(CreatureParts.WolfFang, "A sharp fang from a forest wolf. Vô Jorge rubs it on the blade for a keener edge.",
                "Presa afiada de lobo da mata. Vô Jorge esfrega na lâmina para dar mais corte."),
            ItemName(CreatureParts.SpiderSilk, "Spider Silk", "Seda de Aranha"),
            ItemDesc(CreatureParts.SpiderSilk, "Strong, sticky thread from the forest spider. Makes a weapon quicker and becomes rune paper.",
                "Fio forte e grudento da aranha da floresta. Deixa a arma mais ligeira e vira papel de runa."),
            ItemName(CreatureParts.ScorpionStinger, "Scorpion Stinger", "Ferrão de Escorpião"),
            ItemDesc(CreatureParts.ScorpionStinger, "A dune stinger, still venomous. Helps a weapon find the creature's weak spot.",
                "Ferrão das dunas, ainda com veneno. Ajuda a arma a achar o ponto fraco do bicho."),
            ItemName(CreatureParts.GolemShard, "Golem Shard", "Lasca de Golem"),
            ItemDesc(CreatureParts.GolemShard, "A chip of living stone from the hills. Hardens leather and blade.",
                "Lasca de pedra viva da serra. Endurece o couro e a lâmina."),
            ItemName(CreatureParts.BatWing, "Bat Wing", "Asa de Morcego"),
            ItemDesc(CreatureParts.BatWing, "A thin wing from the caves. They say it lets a fighter drink a little of their strength back.",
                "Asa fina das cavernas. Dizem que faz quem luta beber um pouco da própria força de volta."),
            ItemName(CreatureParts.BeastTrophy, "Beast Trophy", "Troféu da Fera"),
            ItemDesc(CreatureParts.BeastTrophy, "A piece of a defeated boss. Raises a recipe's quality or the fusion forge's chance.",
                "Pedaço de um chefe derrotado. Melhora a qualidade de uma receita ou a chance da forja de fusão."),

            // -- Runes --
            ItemName(RuneItems.Healing, "Healing Rune", "Runa da Cura"),
            ItemDesc(RuneItems.Healing, "A little cordel card with a healing verse, drawn by Lavine or Maria. Reading it heals 60 health.",
                "Cartinha de cordel com um verso de cura, desenhada por Lavine ou Maria. Ao ler, cura 60 de vida."),
            ItemName(RuneItems.Clarity, "Clarity Rune", "Runa da Clareza"),
            ItemDesc(RuneItems.Clarity, "A cordel verse that clears the head. Reading it restores 60 mana.",
                "Verso de cordel que clareia a cabeça. Ao ler, devolve 60 de mana."),
            ItemName(RuneItems.Hide, "Tough Hide Rune", "Runa do Couro Duro"),
            ItemDesc(RuneItems.Hide, "A verse that toughens the skin like tanned leather: +15% defense for 15 seconds.",
                "Verso que endurece a pele feito couro curtido: +15% de defesa por 15 segundos."),
            ItemName(RuneItems.Wind, "Wind Rune", "Runa do Vento"),
            ItemDesc(RuneItems.Wind, "A verse that calls the sertão wind at your back: +20% speed for 12 seconds.",
                "Verso que chama o vento do sertão nas costas: +20% de velocidade por 12 segundos."),
            ItemName(RuneItems.GreatHealing, "Great Healing Rune", "Runa da Cura Grande"),
            ItemDesc(RuneItems.GreatHealing, "A whole healing cordel, sealed with a bat wing. Reading it heals 120 health.",
                "Cordel inteiro de cura, com asa de morcego no lacre. Ao ler, cura 120 de vida."),

            // -- Crafting bench --
            new LocalizationEntry(Title, "Advanced crafting", "Crafting avançado"),
            new LocalizationEntry(Subtitle, "Creature parts make better gear. The further the journey, the more recipes open up.",
                "Parte de bicho faz equipamento melhor. Quanto mais longe a viagem, mais receitas se abrem."),
            new LocalizationEntry(Tier, "Crafting tier {0}/4", "Nível de ofício {0}/4"),
            new LocalizationEntry(QualityLine, "Material quality: {0}", "Qualidade do material: {0}"),
            new LocalizationEntry(QualityKey(MaterialQuality.Rustic), "Rustic", "Rústico"),
            new LocalizationEntry(QualityKey(MaterialQuality.Good), "Good", "Bom"),
            new LocalizationEntry(QualityKey(MaterialQuality.Fine), "Fine", "Fino"),
            new LocalizationEntry(QualityKey(MaterialQuality.Superb), "Superb", "Primoroso"),
            new LocalizationEntry(UseTrophy, "Add a Beast Trophy (+1 quality)", "Pôr um Troféu da Fera (+1 de qualidade)"),
            new LocalizationEntry(CraftedGear, "Made: {0} ({1}).", "Feito: {0} ({1})."),
            new LocalizationEntry(CraftedRunes, "Drawn: {0}× {1}.", "Desenhado: {0}× {1}."),
            new LocalizationEntry(Locked, "Locked: travel further to learn this one.", "Bloqueada: viaje mais longe para aprender essa."),
            new LocalizationEntry(WrongCrafter, "Only Lavine and Maria know how to draw runes.", "Só Lavine e Maria sabem desenhar runas."),
            new LocalizationEntry(MissingIngredients, "Missing materials in the backpack.", "Faltam materiais na mochila."),
            new LocalizationEntry(MissingGold, "Not enough gold.", "Falta ouro."),
            new LocalizationEntry(MissingTrophy, "No Beast Trophy in the backpack.", "Não tem Troféu da Fera na mochila."),
            new LocalizationEntry(NoRoom, "No room in the backpack for the result.", "Não cabe o resultado na mochila."),

            // -- Vô Jorge's weapon enchantments --
            new LocalizationEntry(ImbueTitle, "Vô Jorge's blade blessing", "Benzedura de arma do Vô Jorge"),
            new LocalizationEntry(ImbueGrandpa, "Give me that blade and the creature's part. An old cowboy knows: the bush teaches the iron.",
                "Me dá essa lâmina e a parte do bicho. Vaqueiro velho sabe: o mato ensina o ferro."),
            new LocalizationEntry(ImbueActive, "On the weapon: {0} ({1} fights left)", "Na arma: {0} (faltam {1} lutas)"),
            new LocalizationEntry(ImbueNone, "No blessing on the weapon.", "Nenhuma benzedura na arma."),
            new LocalizationEntry(ImbuePaused, "A road blessing already helps this fight: the weapon blessing waits for the next one.",
                "Uma bênção da estrada já ajuda nesta luta: a benzedura da arma espera a próxima."),
            new LocalizationEntry(ImbueExpired, "{0} wore off the weapon.", "{0} saiu da arma."),
            new LocalizationEntry(MissingParts, "Missing creature parts.", "Faltam partes de bicho."),
            new LocalizationEntry(ImbueCatalog.Get(ImbueCatalog.WolfFang).NameKey, "Fang Edge", "Fio da Presa"),
            new LocalizationEntry(ImbueCatalog.Get(ImbueCatalog.WolfFang).EffectKey, "+6% damage", "+6% de dano"),
            new LocalizationEntry(ImbueCatalog.Get(ImbueCatalog.SpiderSilk).NameKey, "Spider Quickness", "Ligeireza da Aranha"),
            new LocalizationEntry(ImbueCatalog.Get(ImbueCatalog.SpiderSilk).EffectKey, "+6% attack speed", "+6% de velocidade de ataque"),
            new LocalizationEntry(ImbueCatalog.Get(ImbueCatalog.Stinger).NameKey, "Stinger Venom", "Veneno do Ferrão"),
            new LocalizationEntry(ImbueCatalog.Get(ImbueCatalog.Stinger).EffectKey, "+4% critical chance", "+4% de chance de crítico"),
            new LocalizationEntry(ImbueCatalog.Get(ImbueCatalog.GolemShard).NameKey, "Stone Bark", "Casca de Pedra"),
            new LocalizationEntry(ImbueCatalog.Get(ImbueCatalog.GolemShard).EffectKey, "+10% defense", "+10% de defesa"),
            new LocalizationEntry(ImbueCatalog.Get(ImbueCatalog.BatWing).NameKey, "Bat Thirst", "Sede do Morcego"),
            new LocalizationEntry(ImbueCatalog.Get(ImbueCatalog.BatWing).EffectKey, "+2 health per second", "+2 de vida por segundo"),

            // -- Adamas's fusion forge --
            new LocalizationEntry(FusionTitle, "Adamas's fusion forge", "Forja de fusão de Adamas"),
            new LocalizationEntry(FusionKeeper, "Two or three of the same piece, and the fire picks the best. If it fails, the best one comes back whole.",
                "Duas ou três peças iguais, e o fogo escolhe a melhor. Se falhar, a melhor volta inteira."),
            new LocalizationEntry(FusionChance, "{0} pieces · chance {1}%", "{0} peças · chance de {1}%"),
            new LocalizationEntry(FusionSuccess, "It fused! {0} is now {1}.", "Fundiu! {0} agora é {1}."),
            new LocalizationEntry(FusionFailed, "It didn't fuse... {0} came back as it was, and the rest melted into {1} iron.",
                "Não fundiu... {0} voltou do jeito que era, e o resto derreteu em {1} de ferro."),
            new LocalizationEntry(FusionWrongCount, "Pick two or three pieces.", "Escolha duas ou três peças."),
            new LocalizationEntry(FusionNotSame, "The pieces must be the same item (from the backpack).", "As peças têm que ser o mesmo item (da mochila)."),
            new LocalizationEntry(FusionHasOptions, "Excellent, lucky, ancient or socketed pieces don't go in the fusion fire.",
                "Peça excelente, com sorte, antiga ou com engaste não vai no fogo da fusão."),
            new LocalizationEntry(FusionAtCap, "This piece is already as good as the forge can make it here.", "Essa peça já está no melhor que a forja faz por aqui."),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Entries[0].Key)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            return GetFallback(key) ?? key;
        }

        public static string Format(LocalizationManager manager, string key, params object[] args)
        {
            string pattern = Get(manager, key);
            try
            {
                return string.Format(pattern, args);
            }
            catch (System.FormatException)
            {
                return string.Format(GetFallback(key) ?? key, args);
            }
        }

        public static string GetFallback(string key)
        {
            if (key == null)
            {
                return null;
            }

            if (_fallbackByKey == null)
            {
                var table = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    table[entry.Key] = entry.PtBR;
                }

                _fallbackByKey = table;
            }

            return _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }
    }
}
