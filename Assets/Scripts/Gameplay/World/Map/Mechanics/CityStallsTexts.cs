using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the third round of city stalls (<c>CityStallsMechanicsDriver</c>): Shayn's Banca de Troca, Miwnay's gallery fair and
    /// O Engarrafador Guloso helping at it (Mercado dos Túneis, Sogdian names of the Ancient Letters), Pughat's diver of the day on Kirta's bottom
    /// (the trapped divers carry Ugaritic names of the epic of Kirta), Kurogane's forge of the day in Hinokura and Makarios' moon counter in the
    /// Porto da Lua Branca, with one line per cousin for each. Same self-contained pattern as <see cref="KirtaTarchonTownTexts"/>.
    /// </summary>
    public static class CityStallsTexts
    {
        private const string P = "ui.city_stalls.";

        public const string Close = P + "close";
        public const string BagFull = P + "bag_full";
        public const string Gold = P + "gold";
        public const string BundleRow = P + "bundle_row";
        public const string BundlePrice = P + "bundle_price";
        public const string BundleBuy = P + "bundle_buy";
        public const string BundleTaken = P + "bundle_taken";
        public const string BundleBought = P + "bundle_bought";
        public const string NoGold = P + "no_gold";

        // ---- Banca de Troca ----
        public const string BarterPrompt = P + "barter_prompt";
        public const string BarterTitle = P + "barter_title";
        public const string BarterIntro = P + "barter_intro";
        public const string BarterSummary = P + "barter_summary";
        public const string BarterGiveRow = P + "barter_give_row";
        public const string BarterGiveDetail = P + "barter_give_detail";
        public const string BarterPick = P + "barter_pick";
        public const string BarterPicked = P + "barter_picked";
        public const string BarterTakeRow = P + "barter_take_row";
        public const string BarterTakeDetail = P + "barter_take_detail";
        public const string BarterTrade = P + "barter_trade";
        public const string BarterNothing = P + "barter_nothing";
        public const string BarterChoose = P + "barter_choose";
        public const string BarterDone = P + "barter_done";
        public const string BarterUsedUp = P + "barter_used_up";
        public const string BarterNotEnough = P + "barter_not_enough";
        public static readonly string[] TierNames = { P + "tier_loose", P + "tier_grown", P + "tier_vein", P + "tier_forged", P + "tier_mine" };
        public const string BarterMaithe = P + "barter_maithe";
        public const string BarterIcaro = P + "barter_icaro";
        public const string BarterJoaquim = P + "barter_joaquim";
        public const string BarterMaria = P + "barter_maria";
        public const string BarterLavine = P + "barter_lavine";

        // ---- Feira da Galeria ----
        public static readonly string[] GalleryNames = { P + "gallery_north", P + "gallery_east", P + "gallery_south", P + "gallery_west" };
        public const string FairToday = P + "fair_today";
        public const string FairPrompt = P + "fair_prompt";
        public const string FairTitle = P + "fair_title";
        public const string FairIntro = P + "fair_intro";
        public const string FairSummary = P + "fair_summary";
        public const string FairBought = P + "fair_bought";
        public const string BottlerRow = P + "bottler_row";
        public const string BottlerDetail = P + "bottler_detail";
        public const string BottlerTake = P + "bottler_take";
        public const string BottlerTaken = P + "bottler_taken";
        public const string BottlerAbsentRow = P + "bottler_absent_row";
        public const string BottlerAbsentDetail = P + "bottler_absent_detail";
        public const string BottlerGift = P + "bottler_gift";
        public const string BottlerMaithe = P + "bottler_maithe";
        public const string BottlerIcaro = P + "bottler_icaro";
        public const string BottlerJoaquim = P + "bottler_joaquim";
        public const string BottlerMaria = P + "bottler_maria";
        public const string BottlerLavine = P + "bottler_lavine";

        // ---- Kirta: Pughat's diver of the day ----
        public static readonly string[] WalkNames = { P + "walk_lagoon", P + "walk_abyss" };
        public const string DiverHint = P + "diver_hint";
        public const string DiverHintNight = P + "diver_hint_night";
        public const string DiverHintDone = P + "diver_hint_done";
        public const string DiverFreed = P + "diver_freed";
        public const string DiverFreedBubble = P + "diver_freed_bubble";
        public const string DiverMaithe = P + "diver_maithe";
        public const string DiverIcaro = P + "diver_icaro";
        public const string DiverJoaquim = P + "diver_joaquim";
        public const string DiverMaria = P + "diver_maria";
        public const string DiverLavine = P + "diver_lavine";

        // ---- Hinokura: the forge of the day ----
        public const string ForgeDayHintLit = P + "forge_day_hint_lit";
        public const string ForgeDayHintOut = P + "forge_day_hint_out";
        public const string ForgeDayLit = P + "forge_day_lit";
        public const string ForgeDayOut = P + "forge_day_out";
        public const string ForgeMaithe = P + "forge_maithe";
        public const string ForgeIcaro = P + "forge_icaro";
        public const string ForgeJoaquim = P + "forge_joaquim";
        public const string ForgeMaria = P + "forge_maria";
        public const string ForgeLavine = P + "forge_lavine";

        // ---- Porto da Lua Branca: Makarios' moon counter ----
        public static readonly string[] MoonNames =
        {
            P + "moon_0", P + "moon_1", P + "moon_2", P + "moon_3", P + "moon_4", P + "moon_5", P + "moon_6", P + "moon_7",
        };
        public const string MoonPrompt = P + "moon_prompt";
        public const string MoonTitle = P + "moon_title";
        public const string MoonIntro = P + "moon_intro";
        public const string MoonSummary = P + "moon_summary";
        public const string MoonMaithe = P + "moon_maithe";
        public const string MoonIcaro = P + "moon_icaro";
        public const string MoonJoaquim = P + "moon_joaquim";
        public const string MoonMaria = P + "moon_maria";
        public const string MoonLavine = P + "moon_lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(Close, "Close", "Fechar"),
            new LocalizationEntry(BagFull, "The bag is full: make room first", "A mochila está cheia: abra espaço primeiro"),
            new LocalizationEntry(Gold, "Gold: {0}", "Ouro: {0}"),
            new LocalizationEntry(BundleRow, "{1} × {0}", "{1} × {0}"),
            new LocalizationEntry(BundlePrice, "{0} gold (at the crossing: {1})", "{0} de ouro (no cruzamento: {1})"),
            new LocalizationEntry(BundleBuy, "Buy ({0})", "Comprar ({0})"),
            new LocalizationEntry(BundleTaken, "Bought today", "Já comprado hoje"),
            new LocalizationEntry(BundleBought, "Bought: {1} × {0}", "Comprado: {1} × {0}"),
            new LocalizationEntry(NoGold, "{0} gold short", "Faltam {0} de ouro"),

            new LocalizationEntry(BarterPrompt, "[E] Shayn's Barter Stall", "[E] Banca de Troca da Shayn"),
            new LocalizationEntry(BarterTitle, "Shayn's Barter Stall", "Banca de Troca da Shayn"),
            new LocalizationEntry(BarterIntro, "Shayn: No gold at my bench. Three of one thing for two of another, same kind of thing. Pick what you give, then what you take.", "Shayn: Na minha bancada não entra ouro. Três de uma coisa por duas de outra, do mesmo tipo. Escolha o que dá, depois o que leva."),
            new LocalizationEntry(BarterSummary, "Trades today: {0}/{1} · 3 for 2, same kind only", "Trocas de hoje: {0}/{1} · 3 por 2, só do mesmo tipo"),
            new LocalizationEntry(BarterGiveRow, "Give 3 × {0}", "Dar 3 × {0}"),
            new LocalizationEntry(BarterGiveDetail, "You have {0} · {1}", "Você tem {0} · {1}"),
            new LocalizationEntry(BarterPick, "Choose", "Escolher"),
            new LocalizationEntry(BarterPicked, "Chosen", "Escolhido"),
            new LocalizationEntry(BarterTakeRow, "Take 2 × {0}", "Levar 2 × {0}"),
            new LocalizationEntry(BarterTakeDetail, "For 3 × {0}", "Por 3 × {0}"),
            new LocalizationEntry(BarterTrade, "Trade", "Trocar"),
            new LocalizationEntry(BarterNothing, "You don't carry 3 of anything Shayn trades (stone, wood, fibre, herb, ore, iron, silk, fangs, stings, golem chips, bat wings)", "Você não leva 3 de nada que a Shayn troca (pedra, madeira, fibra, erva, minério, ferro, seda, presas, ferrões, lascas de golem, asas de morcego)"),
            new LocalizationEntry(BarterChoose, "Choose first what you give", "Escolha primeiro o que dar"),
            new LocalizationEntry(BarterDone, "Shayn: Done! 3 × {0} for 2 × {1}.", "Shayn: Feito! 3 × {0} por 2 × {1}."),
            new LocalizationEntry(BarterUsedUp, "Shayn: That's enough for today, my bench needs to cool down. Come back tomorrow.", "Shayn: Chega por hoje, minha bancada precisa esfriar. Voltem amanhã."),
            new LocalizationEntry(BarterNotEnough, "You need 3 × {0} to trade", "Precisa de 3 × {0} para trocar"),
            new LocalizationEntry(TierNames[0], "trades with: loose stuff of the ground", "troca com: coisas soltas do chão"),
            new LocalizationEntry(TierNames[1], "trades with: what grows", "troca com: o que cresce"),
            new LocalizationEntry(TierNames[2], "trades with: veins and beasts", "troca com: veias e bichos"),
            new LocalizationEntry(TierNames[3], "trades with: forged and sharp", "troca com: forjado e pontudo"),
            new LocalizationEntry(TierNames[4], "trades with: the deep mine", "troca com: a mina funda"),
            new LocalizationEntry(BarterMaithe, "Maithe: Vó Edna swapped eggs for thread at the fair for years. She'd like Shayn.", "Maithe: A Vó Edna trocava ovo por linha na feira por anos. Ela ia gostar da Shayn."),
            new LocalizationEntry(BarterIcaro, "Ícaro: Three for two is fair when you need the two. That's the whole trick of a barter.", "Ícaro: Três por dois é justo quando a gente precisa dos dois. Esse é o truque todo da troca."),
            new LocalizationEntry(BarterJoaquim, "Joaquim: Vô Jorge says the best deal is the one where both sides go home whistling.", "Joaquim: O Vô Jorge diz que o melhor negócio é aquele em que os dois lados voltam para casa assobiando."),
            new LocalizationEntry(BarterMaria, "Maria: No coins, only things! It's like trading stickers at school!", "Maria: Sem moeda, só coisa! Parece troca de figurinha na escola!"),
            new LocalizationEntry(BarterLavine, "Lavine: The pack is lighter and still has what we need. Good trade.", "Lavine: A mochila ficou mais leve e ainda tem o que a gente precisa. Boa troca."),

            new LocalizationEntry(GalleryNames[0], "North", "Norte"),
            new LocalizationEntry(GalleryNames[1], "East", "Leste"),
            new LocalizationEntry(GalleryNames[2], "South", "Sul"),
            new LocalizationEntry(GalleryNames[3], "West", "Oeste"),
            new LocalizationEntry(FairToday, "Today Miwnay's fair is down the {0} gallery, 70 m from the crossing (follow the amber light)", "Hoje a feira da Miwnay está na galeria {0}, a 70 m do cruzamento (siga a luz âmbar)"),
            new LocalizationEntry(FairPrompt, "[E] Fair of the {0} Gallery", "[E] Feira da Galeria {0}"),
            new LocalizationEntry(FairTitle, "Fair of the {0} Gallery", "Feira da Galeria {0}"),
            new LocalizationEntry(FairIntro, "Miwnay: The fair walks, and so do the prices! Today it's this gallery's turn: what it digs, it sells cheap.", "Miwnay: A feira anda, e os preços também! Hoje é a vez desta galeria: o que ela cava, ela vende barato."),
            new LocalizationEntry(FairSummary, "Gold: {0} · each bundle once per day of the game · tomorrow the fair moves", "Ouro: {0} · cada pacote uma vez por dia do jogo · amanhã a feira muda de galeria"),
            new LocalizationEntry(FairBought, "Miwnay: Into the bag! Tell the other galleries Miwnay's prices don't bite.", "Miwnay: Para a bolsa! Contem nas outras galerias que os preços da Miwnay não mordem."),
            new LocalizationEntry(BottlerRow, "O Engarrafador: 2 Bottles of Pure Air, free", "O Engarrafador: 2 Garrafas de Ar Sereno de graça"),
            new LocalizationEntry(BottlerDetail, "He helps at the fair now · once a day", "Agora ele ajuda na feira · uma vez por dia"),
            new LocalizationEntry(BottlerTake, "Take", "Pegar"),
            new LocalizationEntry(BottlerTaken, "Taken today", "Já pegou hoje"),
            new LocalizationEntry(BottlerAbsentRow, "An empty bench full of corks", "Uma bancada vazia cheia de rolhas"),
            new LocalizationEntry(BottlerAbsentDetail, "O Engarrafador Guloso still hoards the air in his ring down the East gallery", "O Engarrafador Guloso ainda guarda o ar no anel dele, na galeria Leste"),
            new LocalizationEntry(BottlerGift, "Engarrafador: Here. I kept all the air for myself... now it belongs to the fair. Breathe it slowly.", "Engarrafador: Tomem. Eu guardava o ar todo para mim... agora ele é da feira. Respirem devagar."),
            new LocalizationEntry(BottlerMaithe, "Maithe: He who hoarded the air now gives it away. Vó Edna would say that's the best ending a story can have.", "Maithe: Quem guardava o ar agora dá de graça. A Vó Edna diria que esse é o melhor final que uma história pode ter."),
            new LocalizationEntry(BottlerIcaro, "Ícaro: Cork, glass, a breath of Ar Sereno. Simple machine, and it saves a lung.", "Ícaro: Rolha, vidro, um sopro de Ar Sereno. Máquina simples, e salva um pulmão."),
            new LocalizationEntry(BottlerJoaquim, "Joaquim: Vô Jorge always gave a second chance. Looks like the kobolds do too.", "Joaquim: O Vô Jorge sempre dava uma segunda chance. Pelo jeito os kobolds também."),
            new LocalizationEntry(BottlerMaria, "Maria: He's not greedy anymore! Now he's the Engarrafador Generoso!", "Maria: Ele não é mais guloso! Agora é o Engarrafador Generoso!"),
            new LocalizationEntry(BottlerLavine, "Lavine: Two bottles in the pack. The long tunnels feel shorter already.", "Lavine: Duas garrafas na mochila. Os túneis compridos já parecem mais curtos."),

            new LocalizationEntry(WalkNames[0], "lagoon floor (north)", "fundo da lagoa (norte)"),
            new LocalizationEntry(WalkNames[1], "abyss floor (south)", "fundo do abismo (sul)"),
            new LocalizationEntry(DiverHint, "Pughat: The Lodo caught {0} at bubble {1} of the {2}. Walk the bottom and pass through it!", "Pughat: O Lodo prendeu {0} na bolha {1} do {2}. Desçam pelo fundo e passem por ela!"),
            new LocalizationEntry(DiverHintNight, "Pughat: {0} is stuck at a bubble of the abyss, and the sharks close the south at night. Wait for the day, then go.", "Pughat: {0} está preso numa bolha do abismo, e de noite os tubarões fecham o sul. Esperem o dia e vão."),
            new LocalizationEntry(DiverHintDone, "Pughat: {0} is home thanks to you. Tomorrow the Lodo catches someone else; it always does, and we always go.", "Pughat: {0} está em casa graças a vocês. Amanhã o Lodo prende outro; sempre prende, e a gente sempre vai."),
            new LocalizationEntry(DiverFreed, "{0} is free! +{1} gold, +1 Blessing Stone", "{0} está livre! +{1} de ouro, +1 Pedra da Bênção"),
            new LocalizationEntry(DiverFreedBubble, "{0} is free! +{1} gold, +1 Blessing Stone, +1 Bottled Air Bubble", "{0} está livre! +{1} de ouro, +1 Pedra da Bênção, +1 Bolha de Ar Engarrafada"),
            new LocalizationEntry(DiverMaithe, "Maithe: Breathe, breathe! Vó Edna taught us: first the air, then the questions.", "Maithe: Respira, respira! A Vó Edna ensinou: primeiro o ar, depois as perguntas."),
            new LocalizationEntry(DiverIcaro, "Ícaro: The mud had his foot, the bubble had his head. We just joined the two halves.", "Ícaro: O lodo pegou o pé, a bolha segurou a cabeça. A gente só juntou as duas metades."),
            new LocalizationEntry(DiverJoaquim, "Joaquim: Vô Jorge pulled a calf out of the mud once. Same thing, only wetter.", "Joaquim: O Vô Jorge tirou um bezerro do atoleiro uma vez. Mesma coisa, só que mais molhado."),
            new LocalizationEntry(DiverMaria, "Maria: Got you! Hold my hand and don't let go till the top!", "Maria: Te peguei! Segura minha mão e não solta até lá em cima!"),
            new LocalizationEntry(DiverLavine, "Lavine: Pughat can count one more home tonight. That's what the bottom walk is for.", "Lavine: A Pughat pode contar mais um em casa hoje. É para isso que serve andar no fundo."),

            new LocalizationEntry(ForgeDayHintLit, "Kurogane: Today's forge is {0}, and it burns: every refine goes 5% higher ({1} min left).", "Kurogane: A forja do dia é {0}, e ela arde: todo refino sobe 5% (faltam {1} min)."),
            new LocalizationEntry(ForgeDayHintOut, "Kurogane: Today's forge is {0}. Open its sluice: while it burns, every refine goes 5% higher.", "Kurogane: A forja do dia é {0}. Abram a comporta dela: enquanto arder, todo refino sobe 5%."),
            new LocalizationEntry(ForgeDayLit, "Today's forge burns ({0}): refining +5% while it lasts", "A forja do dia arde ({0}): refino +5% enquanto durar"),
            new LocalizationEntry(ForgeDayOut, "Today's forge went out: refining is back to normal", "A forja do dia apagou: o refino volta ao normal"),
            new LocalizationEntry(ForgeMaithe, "Maithe: Vó Edna lit the wood stove before baking, never after. Same with refining.", "Maithe: A Vó Edna acendia o fogão a lenha antes de assar, nunca depois. Refino é igual."),
            new LocalizationEntry(ForgeIcaro, "Ícaro: Lava at the right forge, on the right day. Now the hammer does the rest. Quick, before the Tremor!", "Ícaro: Lava na forja certa, no dia certo. Agora o martelo faz o resto. Rápido, antes do Tremor!"),
            new LocalizationEntry(ForgeJoaquim, "Joaquim: Vô Jorge says iron listens better when the fire is hot.", "Joaquim: O Vô Jorge diz que o ferro escuta melhor quando o fogo está quente."),
            new LocalizationEntry(ForgeMaria, "Maria: It's glowing orange! Let's refine everything we have!", "Maria: Está brilhando laranja! Vamos refinar tudo que a gente tem!"),
            new LocalizationEntry(ForgeLavine, "Lavine: Twenty minutes of good fire. Let's not waste them on the road.", "Lavine: Vinte minutos de fogo bom. Não vamos gastar na estrada."),

            new LocalizationEntry(MoonNames[0], "the new moon", "a lua nova"),
            new LocalizationEntry(MoonNames[1], "the young crescent", "a lua crescente"),
            new LocalizationEntry(MoonNames[2], "the first quarter", "o quarto crescente"),
            new LocalizationEntry(MoonNames[3], "the waxing moon", "a lua gibosa crescente"),
            new LocalizationEntry(MoonNames[4], "the full white moon", "a lua cheia branca"),
            new LocalizationEntry(MoonNames[5], "the waning moon", "a lua gibosa minguante"),
            new LocalizationEntry(MoonNames[6], "the last quarter", "o quarto minguante"),
            new LocalizationEntry(MoonNames[7], "the old moon", "a lua velha"),
            new LocalizationEntry(MoonPrompt, "[E] Makarios' Moon Counter", "[E] Balcão da Lua do Makarios"),
            new LocalizationEntry(MoonTitle, "Makarios' Moon Counter", "Balcão da Lua do Makarios"),
            new LocalizationEntry(MoonIntro, "Makarios: The ice lets through what the moon wants. Tonight it is {0}: this is what the boats brought.", "Makarios: O gelo deixa passar o que a lua quer. Hoje é {0}: foi isso que os barcos trouxeram."),
            new LocalizationEntry(MoonSummary, "Gold: {0} · each bundle once per day of the game · tomorrow: {1}", "Ouro: {0} · cada pacote uma vez por dia do jogo · amanhã: {1}"),
            new LocalizationEntry(MoonMaithe, "Maithe: Vó Edna planted by the moon. Makarios sells by it. I think they'd get along.", "Maithe: A Vó Edna plantava pela lua. O Makarios vende por ela. Acho que os dois iam se dar bem."),
            new LocalizationEntry(MoonIcaro, "Ícaro: Eight moons, sixteen bundles. I'm writing it down so we come back on the right night.", "Ícaro: Oito luas, dezesseis pacotes. Vou anotar para a gente voltar na noite certa."),
            new LocalizationEntry(MoonJoaquim, "Joaquim: Vô Jorge never went fishing without looking at the moon first. Now I see why.", "Joaquim: O Vô Jorge nunca ia pescar sem olhar a lua antes. Agora eu entendo."),
            new LocalizationEntry(MoonMaria, "Maria: The moon chooses the shop! Tomorrow it'll choose something else!", "Maria: A lua escolhe a loja! Amanhã ela escolhe outra coisa!"),
            new LocalizationEntry(MoonLavine, "Lavine: Makarios calls me the Moon Boatwoman. I'll take that as a discount.", "Lavine: O Makarios me chama de Barqueira da Lua. Vou entender isso como desconto."),
        };

        public static string BarterLine(CharacterId hero) => Pick(hero, BarterMaithe, BarterIcaro, BarterJoaquim, BarterMaria, BarterLavine);

        public static string BottlerLine(CharacterId hero) => Pick(hero, BottlerMaithe, BottlerIcaro, BottlerJoaquim, BottlerMaria, BottlerLavine);

        public static string DiverLine(CharacterId hero) => Pick(hero, DiverMaithe, DiverIcaro, DiverJoaquim, DiverMaria, DiverLavine);

        public static string ForgeLine(CharacterId hero) => Pick(hero, ForgeMaithe, ForgeIcaro, ForgeJoaquim, ForgeMaria, ForgeLavine);

        public static string MoonLine(CharacterId hero) => Pick(hero, MoonMaithe, MoonIcaro, MoonJoaquim, MoonMaria, MoonLavine);

        /// <summary>The trapped diver's display name (an id of <see cref="KirtaDiverRescueRules.DiverIds"/>): the names stay the same in both languages.</summary>
        public static string DiverName(string diverId)
        {
            switch (diverId)
            {
                case "yassib": return "Yassib";
                case "thitmanat": return "Thitmanat";
                case "ilhu": return "Ilhu";
                default: return "Pabil";
            }
        }

        private static string Pick(CharacterId hero, string maithe, string icaro, string joaquim, string maria, string lavine)
        {
            switch (hero)
            {
                case CharacterId.Maithe: return maithe;
                case CharacterId.Icaro: return icaro;
                case CharacterId.Maria: return maria;
                case CharacterId.Lavine: return lavine;
                default: return joaquim;
            }
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(Close))
                {
                    manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
                }

                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            _fallback ??= Entries.ToDictionary(entry => entry.Key, entry => entry.PtBR);
            return key != null && _fallback.TryGetValue(key, out string text) ? text : key;
        }

        public static string Format(LocalizationManager manager, string key, params object[] args) => string.Format(Get(manager, key), args);
    }
}
