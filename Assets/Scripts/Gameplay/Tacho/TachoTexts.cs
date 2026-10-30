using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Content;

namespace Rapadura.Gameplay.Tacho
{
    /// <summary>
    /// en + pt-BR text of the Uasamonga da Vó Edna (the old Tacho Encantado, FASE 16): the wing and fruit items (<c>item.&lt;id&gt;.name/.description</c>,
    /// the pt-BR column equal to the asset text), the window of the Vila's hub (<c>ui.tacho.*</c>), Vó Edna's line per cousin
    /// and the wing tooltip line. Same self-contained pattern as <c>RefineTexts</c>: merged into the live
    /// <see cref="LocalizationManager"/> on first use (keys it already has win), pt-BR fallback without one.
    /// </summary>
    public static class TachoTexts
    {
        public const string HubButton = "ui.tacho.hub_button";
        public const string Title = "ui.tacho.title";
        public const string Subtitle = "ui.tacho.subtitle";
        public const string GrandmaAny = "ui.tacho.grandma.greeting_any";
        /// <summary>{0} Mistura, {1} Alma Viva, {2} Água Viva, {3} Faísca, {4} ouro.</summary>
        public const string Owned = "ui.tacho.owned";
        public const string WingsHeader = "ui.tacho.header.wings";
        public const string UpgradeHeader = "ui.tacho.header.upgrade";
        public const string ProtectionHeader = "ui.tacho.header.protection";
        public const string FruitsHeader = "ui.tacho.header.fruits";
        /// <summary>{0} piece name.</summary>
        public const string MainPiece = "ui.tacho.main_piece";
        public const string MainNone = "ui.tacho.main_none";
        public const string MainPrevious = "ui.tacho.main_previous";
        public const string MainNext = "ui.tacho.main_next";
        public const string Equipped = "ui.tacho.equipped";
        /// <summary>{0} wing name, {1} cousin's style.</summary>
        public const string WingRecipe = "ui.tacho.recipe.wings";
        /// <summary>{0} wing name, {1} from, {2} to.</summary>
        public const string UpgradeRecipe = "ui.tacho.recipe.upgrade";
        public const string ProtectionRecipe = "ui.tacho.recipe.protection";
        /// <summary>{0} from, {1} to.</summary>
        public const string ProtectionStep = "ui.tacho.protection.step";
        /// <summary>{0} damage %, {1} defense %, {2} speed %.</summary>
        public const string WingStats = "ui.tacho.wing_stats";
        /// <summary>{0} absorb %.</summary>
        public const string WingAbsorb = "ui.tacho.wing_absorb";
        public const string LegendaryNote = "ui.tacho.legendary_note";
        public const string NoWingsToUpgrade = "ui.tacho.no_wings";
        /// <summary>{0} chance %.</summary>
        public const string Chance = "ui.tacho.chance";
        /// <summary>{0} list of materials.</summary>
        public const string Cost = "ui.tacho.cost";
        /// <summary>{0} quantity, {1} item name.</summary>
        public const string Ingredient = "ui.tacho.ingredient";
        /// <summary>{0} gold.</summary>
        public const string GoldPart = "ui.tacho.gold";
        /// <summary>{0} main item name.</summary>
        public const string FailKeepsMain = "ui.tacho.fail_keeps_main";
        public const string FailMaterials = "ui.tacho.fail_materials";
        public const string FruitGrowthEffect = "ui.tacho.fruit.growth";
        public const string FruitResetEffect = "ui.tacho.fruit.reset";
        public const string CombineButton = "ui.tacho.combine_button";
        /// <summary>{0} recipe, {1} chance %, {2} cost + failure line.</summary>
        public const string Confirm = "ui.tacho.confirm";
        public const string ConfirmYes = "ui.tacho.confirm_yes";
        public const string ConfirmNo = "ui.tacho.confirm_no";
        /// <summary>{0} item made.</summary>
        public const string ResultMade = "ui.tacho.result_made";
        public const string ResultToChest = "ui.tacho.result_to_chest";
        /// <summary>{0} piece (with its new +N).</summary>
        public const string ResultProtected = "ui.tacho.result_protected";
        /// <summary>{0} main item.</summary>
        public const string ResultFailedKept = "ui.tacho.result_failed_kept";
        public const string ResultFailed = "ui.tacho.result_failed";
        public const string NeedMain = "ui.tacho.need_main";
        /// <summary>{0} minimum refine level.</summary>
        public const string NeedRefine = "ui.tacho.need_refine";
        /// <summary>{0} item name.</summary>
        public const string MissingMaterial = "ui.tacho.missing_material";
        /// <summary>{0} gold needed.</summary>
        public const string MissingGold = "ui.tacho.missing_gold";
        public const string MaxLevel = "ui.tacho.max_level";
        public const string NotNeeded = "ui.tacho.not_needed";
        public const string Close = "ui.tacho.close";
        /// <summary>{0} absorb % (tooltip of a level 2+ wing).</summary>
        public const string TooltipAbsorb = "ui.tacho.tooltip.absorb";

        /// <summary>"ui.tacho.grandma.greeting_joaquim": Vó Edna's line when <paramref name="hero"/> opens the Tacho.</summary>
        public static string GreetingKey(CharacterId hero) => "ui.tacho.grandma.greeting_" + hero.ToString().ToLowerInvariant();

        /// <summary>"ui.tacho.cousin.maithe": "estilo da Maithe" on a wing recipe.</summary>
        public static string CousinKey(CharacterId hero) => "ui.tacho.cousin." + hero.ToString().ToLowerInvariant();

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            // -- Items (Resources/Items; pt-BR = the asset's text) --
            new LocalizationEntry(ContentText.ItemNameKey("item_asas_carcara_1"), "Huma Wings", "Asas de Huma"),
            new LocalizationEntry(ContentText.ItemDescriptionKey("item_asas_carcara_1"),
                "Sand-coloured wings of firm feathers, made in the Uasamonga. The tale says the Huma bird never lands and its shadow brings luck: Maithe loves them.",
                "Asas cor de areia de penas firmes, feitas na Uasamonga. Diz o conto que a Huma nunca pousa e que a sombra dela dá sorte: a Maithe adora."),
            new LocalizationEntry(ContentText.ItemNameKey("item_asas_carcara_2"), "Huma Wings II", "Asas de Huma II"),
            new LocalizationEntry(ContentText.ItemDescriptionKey("item_asas_carcara_2"),
                "Sand-coloured wings of firm feathers, made in the Uasamonga. The tale says the Huma bird never lands and its shadow brings luck: Maithe loves them. Level 2: stronger, and it already soaks up a little damage.",
                "Asas cor de areia de penas firmes, feitas na Uasamonga. Diz o conto que a Huma nunca pousa e que a sombra dela dá sorte: a Maithe adora. Nível 2: mais fortes e já absorvem um pouco do dano."),
            new LocalizationEntry(ContentText.ItemNameKey("item_asas_carcara_3"), "Huma Wings III", "Asas de Huma III"),
            new LocalizationEntry(ContentText.ItemDescriptionKey("item_asas_carcara_3"),
                "Sand-coloured wings of firm feathers, made in the Uasamonga. The tale says the Huma bird never lands and its shadow brings luck: Maithe loves them. Level 3: they shine and soak up even more damage.",
                "Asas cor de areia de penas firmes, feitas na Uasamonga. Diz o conto que a Huma nunca pousa e que a sombra dela dá sorte: a Maithe adora. Nível 3: reluzem e absorvem ainda mais dano."),
            new LocalizationEntry(ContentText.ItemNameKey("item_capa_vaqueiro_1"), "Nart Felt Cloak", "Capa de Feltro dos Nartes"),
            new LocalizationEntry(ContentText.ItemDescriptionKey("item_capa_vaqueiro_1"),
                "A thick felt cloak with a golden buckle that flutters on its own. It guards like the cloaks of the Nart heroes in the tales: Joaquim approves.",
                "Capa de feltro grosso com fivela dourada que esvoaça sozinha. Protege como a capa dos heróis nartes dos contos: o Joaquim aprova."),
            new LocalizationEntry(ContentText.ItemNameKey("item_capa_vaqueiro_2"), "Nart Felt Cloak II", "Capa de Feltro dos Nartes II"),
            new LocalizationEntry(ContentText.ItemDescriptionKey("item_capa_vaqueiro_2"),
                "A thick felt cloak with a golden buckle that flutters on its own. It guards like the cloaks of the Nart heroes in the tales: Joaquim approves. Level 2: stronger, and it already soaks up a little damage.",
                "Capa de feltro grosso com fivela dourada que esvoaça sozinha. Protege como a capa dos heróis nartes dos contos: o Joaquim aprova. Nível 2: mais fortes e já absorvem um pouco do dano."),
            new LocalizationEntry(ContentText.ItemNameKey("item_capa_vaqueiro_3"), "Nart Felt Cloak III", "Capa de Feltro dos Nartes III"),
            new LocalizationEntry(ContentText.ItemDescriptionKey("item_capa_vaqueiro_3"),
                "A thick felt cloak with a golden buckle that flutters on its own. It guards like the cloaks of the Nart heroes in the tales: Joaquim approves. Level 3: they shine and soak up even more damage.",
                "Capa de feltro grosso com fivela dourada que esvoaça sozinha. Protege como a capa dos heróis nartes dos contos: o Joaquim aprova. Nível 3: reluzem e absorvem ainda mais dano."),
            new LocalizationEntry(ContentText.ItemNameKey("item_asas_beija_flor_1"), "Aralez Wings", "Asas de Aralez"),
            new LocalizationEntry(ContentText.ItemDescriptionKey("item_asas_beija_flor_1"),
                "Soft little green wings, like the aralez's, the winged dog of the tales who licked the heroes' scrapes. Maria says they smell like mint.",
                "Asinhas verdes e macias, como as do aralez, o cão alado dos contos que lambia o machucado dos heróis. A Maria diz que elas cheiram a hortelã."),
            new LocalizationEntry(ContentText.ItemNameKey("item_asas_beija_flor_2"), "Aralez Wings II", "Asas de Aralez II"),
            new LocalizationEntry(ContentText.ItemDescriptionKey("item_asas_beija_flor_2"),
                "Soft little green wings, like the aralez's, the winged dog of the tales who licked the heroes' scrapes. Maria says they smell like mint. Level 2: stronger, and it already soaks up a little damage.",
                "Asinhas verdes e macias, como as do aralez, o cão alado dos contos que lambia o machucado dos heróis. A Maria diz que elas cheiram a hortelã. Nível 2: mais fortes e já absorvem um pouco do dano."),
            new LocalizationEntry(ContentText.ItemNameKey("item_asas_beija_flor_3"), "Aralez Wings III", "Asas de Aralez III"),
            new LocalizationEntry(ContentText.ItemDescriptionKey("item_asas_beija_flor_3"),
                "Soft little green wings, like the aralez's, the winged dog of the tales who licked the heroes' scrapes. Maria says they smell like mint. Level 3: they shine and soak up even more damage.",
                "Asinhas verdes e macias, como as do aralez, o cão alado dos contos que lambia o machucado dos heróis. A Maria diz que elas cheiram a hortelã. Nível 3: reluzem e absorvem ainda mais dano."),
            new LocalizationEntry(ContentText.ItemNameKey("item_asas_asa_branca_1"), "Kurdalægon's Bronze Wings", "Asas de Bronze de Kurdalægon"),
            new LocalizationEntry(ContentText.ItemDescriptionKey("item_asas_asa_branca_1"),
                "Wings of bright metal plates, like the ones the sky-smith of the tales used to forge. Ícaro swears that this time he really flies.",
                "Asas de placas de metal claro, como as que o ferreiro do céu dos contos forjava. O Ícaro jura que dessa vez ele voa de verdade."),
            new LocalizationEntry(ContentText.ItemNameKey("item_asas_asa_branca_2"), "Kurdalægon's Bronze Wings II", "Asas de Bronze de Kurdalægon II"),
            new LocalizationEntry(ContentText.ItemDescriptionKey("item_asas_asa_branca_2"),
                "Wings of bright metal plates, like the ones the sky-smith of the tales used to forge. Ícaro swears that this time he really flies. Level 2: stronger, and it already soaks up a little damage.",
                "Asas de placas de metal claro, como as que o ferreiro do céu dos contos forjava. O Ícaro jura que dessa vez ele voa de verdade. Nível 2: mais fortes e já absorvem um pouco do dano."),
            new LocalizationEntry(ContentText.ItemNameKey("item_asas_asa_branca_3"), "Kurdalægon's Bronze Wings III", "Asas de Bronze de Kurdalægon III"),
            new LocalizationEntry(ContentText.ItemDescriptionKey("item_asas_asa_branca_3"),
                "Wings of bright metal plates, like the ones the sky-smith of the tales used to forge. Ícaro swears that this time he really flies. Level 3: they shine and soak up even more damage.",
                "Asas de placas de metal claro, como as que o ferreiro do céu dos contos forjava. O Ícaro jura que dessa vez ele voa de verdade. Nível 3: reluzem e absorvem ainda mais dano."),
            new LocalizationEntry(ContentText.ItemNameKey("item_asas_vaga_lume_1"), "Firefly Wings", "Asas de Vaga-lume"),
            new LocalizationEntry(ContentText.ItemDescriptionKey("item_asas_vaga_lume_1"),
                "Dark little wings with tips that glow like sparks. Lavine is not afraid of the night anymore.",
                "Asinhas escuras com pontas que brilham feito faísca. A Lavine não tem mais medo da noite."),
            new LocalizationEntry(ContentText.ItemNameKey("item_asas_vaga_lume_2"), "Firefly Wings II", "Asas de Vaga-lume II"),
            new LocalizationEntry(ContentText.ItemDescriptionKey("item_asas_vaga_lume_2"),
                "Dark little wings with tips that glow like sparks. Lavine is not afraid of the night anymore. Level 2: stronger, and it already soaks up a little damage.",
                "Asinhas escuras com pontas que brilham feito faísca. A Lavine não tem mais medo da noite. Nível 2: mais fortes e já absorvem um pouco do dano."),
            new LocalizationEntry(ContentText.ItemNameKey("item_asas_vaga_lume_3"), "Firefly Wings III", "Asas de Vaga-lume III"),
            new LocalizationEntry(ContentText.ItemDescriptionKey("item_asas_vaga_lume_3"),
                "Dark little wings with tips that glow like sparks. Lavine is not afraid of the night anymore. Level 3: they shine and soak up even more damage.",
                "Asinhas escuras com pontas que brilham feito faísca. A Lavine não tem mais medo da noite. Nível 3: reluzem e absorvem ainda mais dano."),
            new LocalizationEntry(ContentText.ItemNameKey("item_fruta_mandacaru"), "Light Melon", "Melão de Luz"),
            new LocalizationEntry(ContentText.ItemDescriptionKey("item_fruta_mandacaru"),
                "A slice of melon simmered in the Uasamonga. Grandpa Jorge used to say melons keep a lot of light. Eat it during an adventure: +1 attribute point to spend (up to 2 per journey).",
                "Fatia de melão cozida na Uasamonga. O Vô Jorge dizia que melão guarda muita luz. Coma numa aventura: +1 ponto de atributo para distribuir (até 2 por jornada)."),
            new LocalizationEntry(ContentText.ItemNameKey("item_fruta_umbu"), "Pomegranate of Second Chances", "Romã do Recomeço"),
            new LocalizationEntry(ContentText.ItemDescriptionKey("item_fruta_umbu"),
                "A tangy pomegranate from the Uasamonga. Eat it during an adventure: 1 point comes back from your highest attribute so you can place it again.",
                "Romã azedinha da Uasamonga. Coma numa aventura: 1 ponto volta do seu atributo mais alto para você distribuir de novo."),

            // -- Tacho window (Vila hub → Serviços) --
            new LocalizationEntry(HubButton, "Grandma's Uasamonga", "Uasamonga da Vó"),
            new LocalizationEntry(Title, "Grandma Edna's Uasamonga", "Uasamonga da Vó Edna"),
            new LocalizationEntry(Subtitle, "Mix gear, stones and gold in the Uasamonga, the copper cup that knows who deserves it. If the Uasamonga says no, the materials go away, but your piece stays just as it was.",
                "Misture equipamento, pedras e ouro na Uasamonga, a taça de cobre que sabe quem merece. Se a Uasamonga não quiser, os materiais vão embora, mas a sua peça fica do jeitinho que estava."),
            new LocalizationEntry(GrandmaAny, "Stir slowly, child. The Uasamonga likes patience and a pinch of luck.",
                "Mexe devagar, criança. A Uasamonga gosta de paciência e de uma pitada de sorte."),
            new LocalizationEntry("ui.tacho.grandma.greeting_maithe", "Maithe, my eldest. Huma feathers for you: the tale says the Huma never lands, and its shadow brings luck.",
                "Maithe, minha mais velha. Pena de Huma pra você: diz o conto que ela nunca pousa e que a sombra dela dá sorte."),
            new LocalizationEntry("ui.tacho.grandma.greeting_maria", "Maria, aralez wings: quick to reach whoever needs looking after.",
                "Maria, asa de aralez: ligeira pra chegar em quem precisa de cuidado."),
            new LocalizationEntry("ui.tacho.grandma.greeting_joaquim", "Joaquim, your Grandpa Jorge's felt cloak had this same buckle. Stand tall in it.",
                "Joaquim, a capa de feltro do Vô Jorge tinha essa mesma fivela. Veste com o peito estufado."),
            new LocalizationEntry("ui.tacho.grandma.greeting_icaro", "Ícaro, bronze wings, yes. Gears inside the Uasamonga, no. ...Well, maybe one.",
                "Ícaro, asa de bronze, sim. Engrenagem dentro da Uasamonga, não. ...Tá, só uma."),
            new LocalizationEntry("ui.tacho.grandma.greeting_lavine", "Lavine, my little one. Firefly wings: you carry your own light now.",
                "Lavine, minha caçulinha. Asa de vaga-lume: agora você leva a sua própria luz."),
            new LocalizationEntry("ui.tacho.cousin.maithe", "Maithe's style", "estilo da Maithe"),
            new LocalizationEntry("ui.tacho.cousin.maria", "Maria's style", "estilo da Maria"),
            new LocalizationEntry("ui.tacho.cousin.joaquim", "Joaquim's style", "estilo do Joaquim"),
            new LocalizationEntry("ui.tacho.cousin.icaro", "Ícaro's style", "estilo do Ícaro"),
            new LocalizationEntry("ui.tacho.cousin.lavine", "Lavine's style", "estilo da Lavine"),
            new LocalizationEntry(Owned, "In the backpack: Mixture {0} · Living Soul {1} · Living Water {2} · Spark {3} · Gold {4}",
                "Na mochila: Mistura {0} · Alma Viva {1} · Água Viva {2} · Faísca {3} · Ouro {4}"),
            new LocalizationEntry(WingsHeader, "Wings (level 1)", "Asas (nível 1)"),
            new LocalizationEntry(UpgradeHeader, "Strengthen wings (levels 2 and 3)", "Fortalecer asas (nível 2 e 3)"),
            new LocalizationEntry(ProtectionHeader, "Mixture Protection: refine without losing a level", "Proteção da Mistura: refino sem perder nível"),
            new LocalizationEntry(FruitsHeader, "Light Fruits", "Frutas de Luz"),
            new LocalizationEntry(MainPiece, "Chosen piece: {0}", "Peça escolhida: {0}"),
            new LocalizationEntry(MainNone, "No weapon or armour in the backpack. Wings need a piece refined to +4.",
                "Nenhuma arma ou armadura na mochila. Asas precisam de uma peça refinada em +4."),
            new LocalizationEntry(MainPrevious, "Previous", "Anterior"),
            new LocalizationEntry(MainNext, "Next", "Próxima"),
            new LocalizationEntry(Equipped, "(equipped)", "(equipado)"),
            new LocalizationEntry(WingRecipe, "{0} · {1}", "{0} · {1}"),
            new LocalizationEntry(UpgradeRecipe, "{0}: level {1} → {2}", "{0}: nível {1} → {2}"),
            new LocalizationEntry(ProtectionRecipe, "Mixture Protection", "Proteção da Mistura"),
            new LocalizationEntry(ProtectionStep, "+{0} → +{1}; if it fails, it stays at +{0}", "+{0} → +{1}; se falhar, fica em +{0}"),
            new LocalizationEntry(WingStats, "+{0}% damage · +{1}% defense · +{2}% speed", "+{0}% de dano · +{1}% de defesa · +{2}% de velocidade"),
            new LocalizationEntry(WingAbsorb, " · absorbs {0}% of the damage", " · absorve {0}% do dano"),
            new LocalizationEntry(LegendaryNote, "Level 4 (legendary): only in Grandma's stories, for now.",
                "Nível 4 (lendária): só nas histórias da Vó, por enquanto."),
            new LocalizationEntry(NoWingsToUpgrade, "No wings to strengthen yet. Make a pair first!", "Nenhuma asa para fortalecer ainda. Faça um par primeiro!"),
            new LocalizationEntry(Chance, "Chance {0}%", "Chance de {0}%"),
            new LocalizationEntry(Cost, "Cost: {0}", "Custo: {0}"),
            new LocalizationEntry(Ingredient, "{0} {1}", "{0} {1}"),
            new LocalizationEntry(GoldPart, "{0} gold", "{0} de ouro"),
            new LocalizationEntry(FailKeepsMain, "If it fails: the materials are used up, {0} stays with you.",
                "Se falhar: os materiais se gastam, {0} fica com você."),
            new LocalizationEntry(FailMaterials, "If it fails: the materials are used up.", "Se falhar: os materiais se gastam."),
            new LocalizationEntry(FruitGrowthEffect, "Eat it in an adventure: +1 attribute point (up to 2 per journey).",
                "Coma numa aventura: +1 ponto de atributo (até 2 por jornada)."),
            new LocalizationEntry(FruitResetEffect, "Eat it in an adventure: 1 point comes back from your highest attribute.",
                "Coma numa aventura: 1 ponto volta do seu atributo mais alto."),
            new LocalizationEntry(CombineButton, "Stir", "Misturar"),
            new LocalizationEntry(Confirm, "{0}? Chance {1}%. {2}", "{0}? Chance de {1}%. {2}"),
            new LocalizationEntry(ConfirmYes, "Yes, stir!", "Sim, mexer!"),
            new LocalizationEntry(ConfirmNo, "Not now", "Agora não"),
            new LocalizationEntry(ResultMade, "It worked! {0} came out of the Uasamonga!", "Deu certo! {0} saiu da Uasamonga!"),
            new LocalizationEntry(ResultToChest, " (The backpack was full: it went to Grandma's chest.)", " (A mochila estava cheia: foi para o Baú da Vó.)"),
            new LocalizationEntry(ResultProtected, "It worked! Now it's {0}.", "Deu certo! Agora é {0}."),
            new LocalizationEntry(ResultFailedKept, "The Uasamonga said no... the materials went away, but {0} is still yours, just as it was.",
                "A Uasamonga não quis... os materiais foram embora, mas {0} continua com você, do jeitinho que estava."),
            new LocalizationEntry(ResultFailed, "The Uasamonga said no... the materials went away. Try again another day!",
                "A Uasamonga não quis... os materiais foram embora. Tenta de novo outro dia!"),
            new LocalizationEntry(NeedMain, "Choose a piece above first.", "Escolha uma peça lá em cima primeiro."),
            new LocalizationEntry(NeedRefine, "The piece must be refined to +{0} or more (Blacksmith).", "A peça precisa estar refinada em +{0} ou mais (Ferreiro)."),
            new LocalizationEntry(MissingMaterial, "Missing in the backpack: {0}.", "Falta na mochila: {0}."),
            new LocalizationEntry(MissingGold, "Not enough gold: it costs {0}.", "Falta ouro: custa {0}."),
            new LocalizationEntry(MaxLevel, "Already at the top.", "Já está no máximo."),
            new LocalizationEntry(NotNeeded, "Here a normal refine never loses a level: use the Blacksmith.",
                "Aqui o refino normal não perde nível: use o Ferreiro."),
            new LocalizationEntry(Close, "Close", "Fechar"),

            // -- Tooltip (TooltipContentBuilder) --
            new LocalizationEntry(TooltipAbsorb, "Absorbs {0}% of the damage", "Absorve {0}% do dano"),
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

        /// <summary>Localized text via <paramref name="manager"/> when it knows the key, else the pt-BR fallback, else the key.</summary>
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

        /// <summary><see cref="Get"/> + <see cref="string.Format(string, object[])"/>, tolerant of malformed translations.</summary>
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

        /// <summary>An item's name in the current language: its <c>item.&lt;id&gt;.name</c> key, else a helper's pt-BR, else the asset's.</summary>
        public static string ItemName(LocalizationManager manager, string itemId)
        {
            string key = ContentText.ItemNameKey(itemId);
            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            string fallback = GetFallback(key) ?? Rapadura.Gameplay.Refining.RefineTexts.GetFallback(key);
            if (fallback != null)
            {
                return fallback;
            }

            Rapadura.Gameplay.Items.ItemDefinition item = Rapadura.Gameplay.Items.ItemDatabase.GetById(itemId);
            return item != null ? item.DisplayName : itemId;
        }
    }
}
