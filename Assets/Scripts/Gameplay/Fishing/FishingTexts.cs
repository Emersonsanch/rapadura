using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Content;

namespace Rapadura.Gameplay.Fishing
{
    /// <summary>
    /// Localization keys (en + pt-BR) for "Pesca no açude": the panel (<c>fishing.ui.*</c>), the spots
    /// (<c>fishing.spot.*</c>), the species' item names/descriptions (<c>item.item_fish_*.name/.description</c>,
    /// pt-BR = the asset text, as in <c>ContentTexts</c>) and each cousin's reaction per rarity / when it gets away
    /// (<c>fishing.reaction.*</c>, <c>fishing.escape.*</c>). Same self-contained pattern as <c>KitchenTexts</c>: merged
    /// into the live <see cref="LocalizationManager"/> on first use (keys it already has win; <see cref="FishingProgress"/>
    /// merges at boot so the inventory shows fish names in English too), pt-BR fallback without one.
    /// </summary>
    public static class FishingTexts
    {
        public const string Button = "fishing.ui.button";
        /// <summary>{0} = phase, {1} = weather.</summary>
        public const string Conditions = "fishing.ui.conditions";
        public const string ConditionsSheltered = "fishing.ui.conditions_sheltered";
        public const string HintReady = "fishing.ui.hint_ready";
        public const string HintCasting = "fishing.ui.hint_casting";
        public const string HintWaiting = "fishing.ui.hint_waiting";
        public const string HintNibble = "fishing.ui.hint_nibble";
        public const string HintTremble = "fishing.ui.hint_tremble";
        public const string HintBite = "fishing.ui.hint_bite";
        public const string HintReel = "fishing.ui.hint_reel";
        public const string HintSurge = "fishing.ui.hint_surge";
        public const string ActionCast = "fishing.ui.action_cast";
        public const string ActionCasting = "fishing.ui.action_casting";
        public const string ActionHook = "fishing.ui.action_hook";
        public const string ActionReel = "fishing.ui.action_reel";
        public const string ActionAgain = "fishing.ui.action_again";
        public const string Tension = "fishing.ui.tension";
        public const string Progress = "fishing.ui.progress";
        public const string ResultTooEarly = "fishing.ui.result_too_early";
        public const string ResultTooLate = "fishing.ui.result_too_late";
        public const string ResultSnapped = "fishing.ui.result_snapped";
        public const string ResultEscaped = "fishing.ui.result_escaped";
        /// <summary>{0} = fish, {1} = rarity, {2} = weight.</summary>
        public const string ResultLanded = "fishing.ui.result_landed";
        public const string NewSpecies = "fishing.ui.new_species";
        /// <summary>{0} = previous record.</summary>
        public const string NewRecord = "fishing.ui.new_record";
        /// <summary>{0} = record.</summary>
        public const string RecordToBeat = "fishing.ui.record_to_beat";
        public const string AddedToBag = "fishing.ui.added_to_bag";
        public const string BagFull = "fishing.ui.bag_full";
        public const string CollectionComplete = "fishing.ui.collection_complete";
        /// <summary>{0} = caught species, {1} = total.</summary>
        public const string CollectionButton = "fishing.ui.collection_button";
        public const string CollectionTitle = "fishing.ui.collection_title";
        public const string BackToWater = "fishing.ui.back_to_water";
        /// <summary>{0} = fish, {1} = rarity, {2} = record, {3} = times caught.</summary>
        public const string CollectionRow = "fishing.ui.collection_row";
        /// <summary>{0} = rarity.</summary>
        public const string CollectionUnknown = "fishing.ui.collection_unknown";
        /// <summary>{0} = spots.</summary>
        public const string CollectionWhere = "fishing.ui.collection_where";
        public const string RelaxedToggle = "fishing.ui.relaxed_toggle";
        public const string RelaxedByMotion = "fishing.ui.relaxed_by_motion";
        public const string Close = "fishing.ui.close";
        /// <summary>{0} = number.</summary>
        public const string WeightKg = "fishing.ui.weight_kg";
        /// <summary>{0} = number.</summary>
        public const string WeightG = "fishing.ui.weight_g";
        /// <summary>"," in pt-BR, "." in English.</summary>
        public const string DecimalSeparator = "fishing.ui.decimal_separator";

        public static string RarityKey(FishRarity rarity) => "fishing.rarity." + rarity.ToString().ToLowerInvariant();

        /// <summary>"fishing.reaction.maithe.rare"; "any" when no cousin is selected.</summary>
        public static string ReactionKey(CharacterId? hero, FishRarity rarity) => "fishing.reaction." + HeroSegment(hero) + "." + rarity.ToString().ToLowerInvariant();

        /// <summary>"fishing.escape.joaquim": the cousin's line when a hooked fish gets away (snapped or escaped).</summary>
        public static string EscapeKey(CharacterId? hero) => "fishing.escape." + HeroSegment(hero);

        private static string HeroSegment(CharacterId? hero) => hero.HasValue ? hero.Value.ToString().ToLowerInvariant() : "any";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(Button, "Fish", "Pescar"),
            new LocalizationEntry(Conditions, "Now: {0} · {1}", "Agora: {0} · {1}"),
            new LocalizationEntry(ConditionsSheltered, "Now: {0} · under the rock, no weather", "Agora: {0} · debaixo da pedra, sem tempo ruim"),
            new LocalizationEntry(HintReady, "Tap Cast to throw the line.", "Toque em Lançar pra jogar a linha."),
            new LocalizationEntry(HintCasting, "The line flies...", "A linha voa..."),
            new LocalizationEntry(HintWaiting, "Watch the float... Hook it when it sinks!", "Olho na boia... Fisgue quando ela afundar!"),
            new LocalizationEntry(HintNibble, "A nibble... not yet!", "Beliscou... ainda não!"),
            new LocalizationEntry(HintTremble, "The float is trembling... get ready!", "A boia tá tremendo... prepara!"),
            new LocalizationEntry(HintBite, "IT SANK! Hook it!", "AFUNDOU! Fisga!"),
            new LocalizationEntry(HintReel, "Hold to reel, let go when the line gets too tight.", "Segure pra puxar, solte quando a linha esticar demais."),
            new LocalizationEntry(HintSurge, "It's pulling! Let the line go!", "Tranco! Solta a linha!"),
            new LocalizationEntry(ActionCast, "Cast", "Lançar"),
            new LocalizationEntry(ActionCasting, "Casting...", "Lançando..."),
            new LocalizationEntry(ActionHook, "Hook!", "Fisgar!"),
            new LocalizationEntry(ActionReel, "Hold: reel in", "Segure: puxar"),
            new LocalizationEntry(ActionAgain, "Cast again", "Lançar de novo"),
            new LocalizationEntry(Tension, "Line tension", "Tensão da linha"),
            new LocalizationEntry(Progress, "Line reeled in", "Linha recolhida"),
            new LocalizationEntry(ResultTooEarly, "Too early — that was just a nibble. The fish got scared.", "Puxou cedo demais — era só uma beliscada. O peixe se assustou."),
            new LocalizationEntry(ResultTooLate, "Too slow! The fish took the bait.", "Demorou! O peixe levou a isca."),
            new LocalizationEntry(ResultSnapped, "The line snapped! Let go when the tension reaches the red.", "A linha arrebentou! Solte quando a tensão chegar no vermelho."),
            new LocalizationEntry(ResultEscaped, "The fish spat the hook and got away.", "O peixe cuspiu o anzol e escapou."),
            new LocalizationEntry(ResultLanded, "Caught: {0} ({1}) · {2}", "Pegou: {0} ({1}) · {2}"),
            new LocalizationEntry(NewSpecies, "New species in the notebook!", "Espécie nova no caderno!"),
            new LocalizationEntry(NewRecord, "New record! (before: {0})", "Novo recorde! (antes: {0})"),
            new LocalizationEntry(RecordToBeat, "Record: {0}", "Recorde: {0}"),
            new LocalizationEntry(AddedToBag, "Into the backpack it goes.", "Foi pra mochila."),
            new LocalizationEntry(BagFull, "Backpack too heavy: the fish went back into the water (the record stays in the notebook).", "Mochila pesada demais: o peixe voltou pra água (o recorde fica no caderno)."),
            new LocalizationEntry(CollectionComplete, "Notebook complete! Grandpa Chico would be proud.", "Caderno completo! O Vô Chico ia ficar orgulhoso."),
            new LocalizationEntry(CollectionButton, "Fishing notebook ({0}/{1})", "Caderno de pesca ({0}/{1})"),
            new LocalizationEntry(CollectionTitle, "Fishing notebook · biggest fish of each kind", "Caderno de pesca · o maior peixe de cada espécie"),
            new LocalizationEntry(BackToWater, "Back to the water", "Voltar pra água"),
            new LocalizationEntry(CollectionRow, "{0} ({1}) · record {2} · {3} caught", "{0} ({1}) · recorde {2} · {3} pescados"),
            new LocalizationEntry(CollectionUnknown, "??? ({0}) · not caught yet", "??? ({0}) · ainda não pescado"),
            new LocalizationEntry(CollectionWhere, "Where: {0}", "Onde: {0}"),
            new LocalizationEntry(RelaxedToggle, "Relaxed fishing (wider timing)", "Pesca tranquila (janela maior)"),
            new LocalizationEntry(RelaxedByMotion, "\"Reduce Motion\" is on: wider timing already.", "\"Reduzir Movimento\" ligado: janela maior já ativa."),
            new LocalizationEntry(Close, "Close", "Fechar"),
            new LocalizationEntry(WeightKg, "{0} kg", "{0} kg"),
            new LocalizationEntry(WeightG, "{0} g", "{0} g"),
            new LocalizationEntry(DecimalSeparator, ".", ","),

            new LocalizationEntry(RarityKey(FishRarity.Common), "common", "comum"),
            new LocalizationEntry(RarityKey(FishRarity.Uncommon), "uncommon", "incomum"),
            new LocalizationEntry(RarityKey(FishRarity.Rare), "rare", "raro"),
            new LocalizationEntry(RarityKey(FishRarity.Legendary), "legendary", "lendário"),

            // -- Spots (FishCatalog.Spots) --
            new LocalizationEntry("fishing.spot.acude.name", "Grandpa Chico's pond", "Açude do Vô Chico"),
            new LocalizationEntry("fishing.spot.acude.intro", "The pond Grandpa Chico dug with his own hands. They say the Golden Fish still lives at the bottom.", "O açude que o Vô Chico cavou com as próprias mãos. Dizem que o Peixe-Dourado ainda mora lá no fundo."),
            new LocalizationEntry("fishing.spot.riacho.name", "Mountain stream", "Riacho da Serra"),
            new LocalizationEntry("fishing.spot.riacho.intro", "Icy water running down the mountain. Fish here are quick and bite fast.", "Água gelada descendo a serra. Aqui o peixe é ligeiro e morde rápido."),
            new LocalizationEntry("fishing.spot.lagosubterraneo.name", "Underground lake", "Lago Subterrâneo"),
            new LocalizationEntry("fishing.spot.lagosubterraneo.intro", "A dark lake under the rock. No rain reaches here — only the glow of a few little fish.", "Um lago escuro debaixo da pedra. Chuva nenhuma chega aqui — só o brilho de uns peixinhos."),

            // -- Species (Resources/Items/item_fish_*.asset; pt-BR = the asset text) --
            ItemName(FishCatalog.Tilapia, "Tilapia", "Tilápia"),
            ItemDescription(FishCatalog.Tilapia, "A pond fish with firm white meat. Great on the coals.", "Peixe de açude, carne branca e firme. Vai bem na brasa."),
            ItemName(FishCatalog.Piaba, "Piaba", "Piaba"),
            ItemDescription(FishCatalog.Piaba, "A tiny fish found in any water. Good for broth.", "Peixinho miúdo que vive em todo canto d'água. Bom pra caldo."),
            ItemName(FishCatalog.Traira, "Traíra", "Traíra"),
            ItemDescription(FishCatalog.Traira, "A sharp-toothed fish that hunts at dusk and at night.", "Peixe de dente afiado que caça no fim da tarde e de noite."),
            ItemName(FishCatalog.Pacu, "Pacu", "Pacu"),
            ItemDescription(FishCatalog.Pacu, "A round, strong fish that loves the midday sun.", "Peixe redondo e forte, gosta do sol do meio-dia."),
            ItemName(FishCatalog.Curimata, "Curimatã", "Curimatã"),
            ItemDescription(FishCatalog.Curimata, "Comes up to the surface when it rains.", "Sobe à flor d'água quando chove."),
            ItemName(FishCatalog.Tucunare, "Peacock bass", "Tucunaré"),
            ItemDescription(FishCatalog.Tucunare, "A brave fish with an eye on its tail. Only strikes under a clear sky.", "Peixe valente de olho no rabo. Só ataca a isca com o céu limpo."),
            ItemName(FishCatalog.PeixeDourado, "Golden Fish of the Pond", "Peixe-Dourado do Açude"),
            ItemDescription(FishCatalog.PeixeDourado, "Grandpa Chico's legend: it only shows up at dawn or dusk, and even more in the rain.", "A lenda do Vô Chico: só aparece de madrugada ou ao entardecer, e mais ainda na chuva."),
            ItemName(FishCatalog.Lambari, "Lambari", "Lambari"),
            ItemDescription(FishCatalog.Lambari, "Silvery and quick, it lives in the stream's rapids.", "Prateado e ligeiro, vive nas corredeiras do riacho."),
            ItemName(FishCatalog.Piau, "Piau", "Piau"),
            ItemDescription(FishCatalog.Piau, "A striped fish that likes the early-morning fog.", "Peixe listrado que gosta da neblina da madrugada."),
            ItemName(FishCatalog.Mandi, "Mandi", "Mandi"),
            ItemDescription(FishCatalog.Mandi, "A small catfish with a sting that comes out at night. Careful with the hook!", "Bagrinho de ferrão que sai de noite. Cuidado ao tirar do anzol!"),
            ItemName(FishCatalog.Cascudo, "Armoured catfish", "Cascudo"),
            ItemDescription(FishCatalog.Cascudo, "An armoured fish that clings to the rocks.", "Peixe de couraça que vive grudado nas pedras."),
            ItemName(FishCatalog.Surubim, "Surubim", "Surubim"),
            ItemDescription(FishCatalog.Surubim, "A huge spotted catfish. Prefers night and rain.", "Peixe-gato pintado, enorme. Prefere a noite e a chuva."),
            ItemName(FishCatalog.BagreCego, "Blind Cave Catfish", "Bagre-cego da Gruta"),
            ItemDescription(FishCatalog.BagreCego, "It has never seen the sun. It feels the bait through the ripples.", "Nunca viu a luz do sol. Sente a isca pelo tremor da água."),
            ItemName(FishCatalog.PiabaLanterna, "Lantern Piaba", "Piaba-lanterna"),
            ItemDescription(FishCatalog.PiabaLanterna, "It glows in the dark of the underground lake, brighter at night.", "Brilha no escuro do lago subterrâneo, mais forte de noite."),

            // -- Reactions per cousin and rarity (birth order Maithe > Maria > Joaquim > Ícaro > Lavine) --
            Reaction(null, FishRarity.Common, "A small one, but it counts!", "Pequeno, mas vale!"),
            Reaction(null, FishRarity.Uncommon, "That one put up a fight!", "Esse brigou bonito!"),
            Reaction(null, FishRarity.Rare, "What a fish! Grandma has to see this.", "Que peixão! A Vó tem que ver isso."),
            Reaction(null, FishRarity.Legendary, "The Golden Fish of the Pond... it's real!", "O Peixe-Dourado do Açude... ele existe!"),

            Reaction(CharacterId.Maithe, FishRarity.Common, "Small, but honest. With good aim, even a piaba counts.", "Pequeno, mas honesto. Na mira certa até piaba vale."),
            Reaction(CharacterId.Maithe, FishRarity.Uncommon, "This one fought well. Respect.", "Esse lutou bonito. Respeito."),
            Reaction(CharacterId.Maithe, FishRarity.Rare, "A huntress's patience. Ícaro, take notes!", "Paciência de caçadora. Ícaro, aprende!"),
            Reaction(CharacterId.Maithe, FishRarity.Legendary, "Grandpa Chico's Golden Fish... He said it only shows itself to those who know how to wait.", "O Peixe-Dourado do Vô Chico... Ele dizia que só aparece pra quem sabe esperar."),

            Reaction(CharacterId.Maria, FishRarity.Common, "How cute! Thank you, pond — we'll take good care of it.", "Que bonitinho! Obrigada, açude, a gente cuida bem dele."),
            Reaction(CharacterId.Maria, FishRarity.Uncommon, "Look at those colours! Grandma's going to love it.", "Olha que cor linda! A Vó vai gostar."),
            Reaction(CharacterId.Maria, FishRarity.Rare, "Goodness, what a fish! Joaquim, come see this!", "Meu Deus, que peixão! Joaquim, vem ver isso!"),
            Reaction(CharacterId.Maria, FishRarity.Legendary, "It shines like the village light. Shall we let it go someday? ...After the picture.", "Brilha igual à luz da vila. Vamos soltar um dia? ...Depois da foto."),

            Reaction(CharacterId.Joaquim, FishRarity.Common, "That's a snack already. Ten more and it's lunch!", "Já dá pra um tira-gosto. Mais uns dez e vira almoço!"),
            Reaction(CharacterId.Joaquim, FishRarity.Uncommon, "This one I hold with both hands. And the shield, if needed.", "Esse aqui eu seguro com as duas mãos. E com o escudo, se precisar."),
            Reaction(CharacterId.Joaquim, FishRarity.Rare, "That's not a fish, that's an opponent! And I won!", "Isso não é peixe, é adversário! E eu venci!"),
            Reaction(CharacterId.Joaquim, FishRarity.Legendary, "NOBODY is going to believe this. Maria, you're my witness!", "NINGUÉM vai acreditar. Maria, tu é testemunha!"),

            Reaction(CharacterId.Icaro, FishRarity.Common, "Noted: the bait works. Next test: a bigger fish.", "Anotado: a isca funciona. Próximo teste: peixe maior."),
            Reaction(CharacterId.Icaro, FishRarity.Uncommon, "Tension under control, reel intact. Pure engineering!", "Tensão controlada, carretilha inteira. Engenharia pura!"),
            Reaction(CharacterId.Icaro, FishRarity.Rare, "I need a bigger scale. And a frame for the wall.", "Preciso de uma balança maior. E de um quadro na parede."),
            Reaction(CharacterId.Icaro, FishRarity.Legendary, "Scales that reflect all the light?! Maithe, lend me your notebook!", "Escama que reflete a luz toda?! Maithe, me empresta teu caderno!"),

            Reaction(CharacterId.Lavine, FishRarity.Common, "I caught it by myself. Nobody needs to help.", "Peguei sozinha. Ninguém precisa ajudar."),
            Reaction(CharacterId.Lavine, FishRarity.Uncommon, "See? I said I could. Go tell Maithe.", "Viu? Eu falei que conseguia. Conta pra Maithe."),
            Reaction(CharacterId.Lavine, FishRarity.Rare, "...Fine. You can be impressed. Just a little.", "...Tá. Pode ficar impressionado. Só um pouquinho."),
            Reaction(CharacterId.Lavine, FishRarity.Legendary, "Me. The youngest. I caught the legend. Someone write it in Grandma's notebook.", "Eu. A caçula. Peguei a lenda. Alguém escreve isso no caderno da Vó."),

            // -- When a hooked fish gets away --
            new LocalizationEntry(EscapeKey(null), "It got away... next time!", "Escapou... fica pra próxima!"),
            new LocalizationEntry(EscapeKey(CharacterId.Maithe), "Got away. Next time I read the line first.", "Escapou. Da próxima eu leio a linha antes."),
            new LocalizationEntry(EscapeKey(CharacterId.Maria), "Go in peace, little fish. But come back, okay?", "Vai com Deus, peixinho. Mas volta, viu?"),
            new LocalizationEntry(EscapeKey(CharacterId.Joaquim), "Hey! Come back, the fight's not over!", "Ei! Volta aqui que a luta não acabou!"),
            new LocalizationEntry(EscapeKey(CharacterId.Icaro), "Design flaw: the fish. Adjusting...", "Falha de projeto: o peixe. Ajustando..."),
            new LocalizationEntry(EscapeKey(CharacterId.Lavine), "I let it go. On purpose. ...Don't tell anyone.", "Eu deixei ele ir. De propósito. ...Não conta pra ninguém."),
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

        private static LocalizationEntry ItemName(string id, string en, string ptBR) => new LocalizationEntry(ContentText.ItemNameKey(id), en, ptBR);
        private static LocalizationEntry ItemDescription(string id, string en, string ptBR) => new LocalizationEntry(ContentText.ItemDescriptionKey(id), en, ptBR);
        private static LocalizationEntry Reaction(CharacterId? hero, FishRarity rarity, string en, string ptBR) => new LocalizationEntry(ReactionKey(hero, rarity), en, ptBR);
    }
}
