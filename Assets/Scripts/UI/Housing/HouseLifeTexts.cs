using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Housing;

namespace Rapadura.UI.Housing
{
    /// <summary>
    /// Localization keys (en + pt-BR) of H4 "Melhorar a casa" and H6 "Vida na casa" (<see cref="HouseLifePanel"/>,
    /// <see cref="HouseScreenController"/>): upgrade requirements, the family's moving-day lines, comfort tiers,
    /// "Descansado", the 21 sets and their guest-book titles, plants, the guest book and its visitors, the "Missões da
    /// Casa" 2–6 and the displays (trophies, relics, Caderno shelf, Caramelo, training dummy). Same self-contained
    /// pattern as <see cref="HouseTexts"/>: merged into the live manager on first use, pt-BR fallback without one.
    /// </summary>
    public static class HouseLifeTexts
    {
        public const string UpgradeButton = "ui.house_life.upgrade_button";
        public const string LifeButton = "ui.house_life.life_button";
        public const string Close = "ui.house_life.close";
        public const string UpgradeTitle = "ui.house_life.upgrade_title";
        /// <summary>{0} = current level name, {1} = next level name.</summary>
        public const string UpgradeNext = "ui.house_life.upgrade_next";
        public const string UpgradeMax = "ui.house_life.upgrade_max";
        public const string UpgradeNoHouse = "ui.house_life.upgrade_no_house";
        /// <summary>{0} = level needed, {1} = hero level.</summary>
        public const string ReqLevel = "ui.house_life.req_level";
        /// <summary>{0} = boss name.</summary>
        public const string ReqBoss = "ui.house_life.req_boss";
        /// <summary>{0} = item name, {1} = have, {2} = need.</summary>
        public const string ReqItem = "ui.house_life.req_item";
        /// <summary>{0} = keepsake name.</summary>
        public const string ReqKeepsake = "ui.house_life.req_keepsake";
        public const string RowMet = "ui.house_life.row_met";
        public const string RowMissing = "ui.house_life.row_missing";
        public const string UpgradeDo = "ui.house_life.upgrade_do";
        public const string UpgradeProvisional = "ui.house_life.upgrade_provisional";
        public const string UpgradeConfirmTitle = "ui.house_life.upgrade_confirm_title";
        /// <summary>{0} = level name, {1} = cost list.</summary>
        public const string UpgradeConfirmText = "ui.house_life.upgrade_confirm_text";
        public const string UpgradeConfirmYes = "ui.house_life.upgrade_confirm_yes";
        public const string UpgradeConfirmNo = "ui.house_life.upgrade_confirm_no";
        public const string UpgradeDoneTitle = "ui.house_life.upgrade_done_title";
        /// <summary>{0} = level name.</summary>
        public const string UpgradeDoneText = "ui.house_life.upgrade_done_text";
        /// <summary>{0} = pieces moved to the depot.</summary>
        public const string UpgradeMoved = "ui.house_life.upgrade_moved";
        public const string UpgradeNothingMoved = "ui.house_life.upgrade_nothing_moved";
        public const string UpgradeBlocked = "ui.house_life.upgrade_blocked";
        public const string UpgradePaymentFailed = "ui.house_life.upgrade_payment_failed";
        public const string UpgradeOk = "ui.house_life.upgrade_ok";
        public const string ScenePrefix = "ui.house_life.scene.";

        public const string LifeTitle = "ui.house_life.life_title";
        /// <summary>{0} = comfort points, {1} = tier name.</summary>
        public const string ComfortLine = "ui.house_life.comfort_line";
        /// <summary>{0} = items, {1} = sets.</summary>
        public const string ComfortParts = "ui.house_life.comfort_parts";
        /// <summary>{0} = points missing, {1} = next tier name.</summary>
        public const string ComfortNext = "ui.house_life.comfort_next";
        public const string ComfortTop = "ui.house_life.comfort_top";
        public const string TierPrefix = "ui.house_life.tier.";
        /// <summary>{0} = percent, {1} = fights left.</summary>
        public const string RestedOn = "ui.house_life.rested_on";
        public const string RestedOff = "ui.house_life.rested_off";
        /// <summary>{0} = nights left today, {1} = per day.</summary>
        public const string RestCharges = "ui.house_life.rest_charges";
        /// <summary>{0} = percent, {1} = fights.</summary>
        public const string SleptRested = "ui.house_life.slept_rested";
        public const string SleptNoCharge = "ui.house_life.slept_no_charge";
        public const string SetsHeader = "ui.house_life.sets_header";
        public const string SetsNone = "ui.house_life.sets_none";
        /// <summary>{0} = set name, {1} = pieces placed, {2} = total, {3} = stage text.</summary>
        public const string SetRow = "ui.house_life.set_row";
        public const string SetStage0 = "ui.house_life.set_stage_0";
        public const string SetStage1 = "ui.house_life.set_stage_1";
        public const string SetStage2 = "ui.house_life.set_stage_2";
        public const string SetStage3 = "ui.house_life.set_stage_3";
        /// <summary>{0} = title.</summary>
        public const string HouseTitles = "ui.house_life.house_titles";
        public const string PerkSoon = "ui.house_life.perk_soon";
        public const string PlantsHeader = "ui.house_life.plants_header";
        public const string PlantsNone = "ui.house_life.plants_none";
        public const string PlantsHappy = "ui.house_life.plants_happy";
        /// <summary>{0} = plants that want water.</summary>
        public const string PlantsThirsty = "ui.house_life.plants_thirsty";
        public const string WaterButton = "ui.house_life.water_button";
        public const string Watered = "ui.house_life.watered";
        public const string WateredHappy = "ui.house_life.watered_happy";
        /// <summary>{0} = yard plants watered.</summary>
        public const string Rain = "ui.house_life.rain";
        public const string MoodHappy = "ui.house_life.mood_happy";
        public const string MoodThirsty = "ui.house_life.mood_thirsty";
        public const string MoodWilted = "ui.house_life.mood_wilted";
        /// <summary>{0} = plant name, {1} = mood.</summary>
        public const string PlantStatus = "ui.house_life.plant_status";

        public const string QuestsHeader = "ui.house_life.quests_header";
        public const string QuestLocked = "ui.house_life.quest_locked";
        public const string QuestActive = "ui.house_life.quest_active";
        public const string QuestReady = "ui.house_life.quest_ready";
        public const string QuestCompleted = "ui.house_life.quest_completed";
        public const string QuestDeliver = "ui.house_life.quest_deliver";
        /// <summary>{0} = quest name.</summary>
        public const string QuestDoneStatus = "ui.house_life.quest_done_status";
        public const string WardrobeOwed = "ui.house_life.wardrobe_owed";

        public const string GuestBookHeader = "ui.house_life.guest_book_header";
        public const string GuestBookEmpty = "ui.house_life.guest_book_empty";
        /// <summary>{0} = day, {1} = visitor, {2} = line.</summary>
        public const string GuestEntry = "ui.house_life.guest_entry";
        /// <summary>{0} = visitor.</summary>
        public const string VisitStatus = "ui.house_life.visit_status";
        /// <summary>{0} = signed cousins, {1} = needed.</summary>
        public const string CousinsSigned = "ui.house_life.cousins_signed";

        /// <summary>{0} = bosses beaten, {1} = total.</summary>
        public const string TrophyStatus = "ui.house_life.trophy_status";
        public const string RelicTitle = "ui.house_life.relic_title";
        /// <summary>{0} = found, {1} = total.</summary>
        public const string RelicStatus = "ui.house_life.relic_status";
        public const string RelicMissing = "ui.house_life.relic_missing";
        public const string ShelfOpening = "ui.house_life.shelf_opening";
        public const string ShelfUnavailable = "ui.house_life.shelf_unavailable";
        public const string PetNap = "ui.house_life.pet_nap";
        public const string Dummy = "ui.house_life.dummy";
        public const string Ok = "ui.house_life.ok";

        public static string SceneKey(int level) => ScenePrefix + level;
        public static string TierKey(int tier) => TierPrefix + HouseRestedRules.ClampTier(tier);
        public static string SetNameKey(HouseSetDefinition set) => set.NameKey;
        public static string SetTitleKey(HouseSetDefinition set) => set.TitleKey;
        public static string StageKey(int stage) => stage >= 3 ? SetStage3 : stage == 2 ? SetStage2 : stage == 1 ? SetStage1 : SetStage0;
        public static string VisitorKey(string visitorId) => "ui.house_life.visitor." + (visitorId ?? string.Empty).ToLowerInvariant();
        public static string VisitLineKey(string visitorId, int line) => VisitorKey(visitorId) + ".line_" + line;
        public static string BossKey(string bossId) => "ui.house_life.boss." + bossId;
        public static string MaterialKey(string itemId) => "ui.house_life.item." + itemId;

        public static string MoodKey(HousePlantMood mood) => mood == HousePlantMood.Wilted ? MoodWilted : mood == HousePlantMood.Thirsty ? MoodThirsty : MoodHappy;

        public static string QuestStateKey(HouseQuestState state)
        {
            switch (state)
            {
                case HouseQuestState.Active: return QuestActive;
                case HouseQuestState.Ready: return QuestReady;
                case HouseQuestState.Completed: return QuestCompleted;
                default: return QuestLocked;
            }
        }

        public static readonly List<LocalizationEntry> Entries = BuildEntries();

        private static List<LocalizationEntry> BuildEntries()
        {
            var list = new List<LocalizationEntry>
            {
                E(UpgradeButton, "Improve the house", "Melhorar a casa"),
                E(LifeButton, "Life at home", "Vida na casa"),
                E(Close, "Close", "Fechar"),
                E(UpgradeTitle, "Improve the house", "Melhorar a casa"),
                E(UpgradeNext, "{0} → {1}", "{0} → {1}"),
                E(UpgradeMax, "Your house is already a manor! There's nothing left to build — just to enjoy.", "Sua casa já é um casarão! Não falta mais nada pra construir — só aproveitar."),
                E(UpgradeNoHouse, "First Vó Zefa has to hand you the key.", "Primeiro a Vó Zefa precisa te entregar a chave."),
                E(ReqLevel, "Cousin at level {0} (now {1})", "Primo no nível {0} (agora {1})"),
                E(ReqBoss, "{0} set free", "{0} libertado"),
                E(ReqItem, "{0}: {1}/{2}", "{0}: {1}/{2}"),
                E(ReqKeepsake, "Keepsake in the house: {0}", "Lembrança na casa: {0}"),
                E(RowMet, "✓", "✓"),
                E(RowMissing, "✗", "✗"),
                E(UpgradeDo, "Improve", "Melhorar"),
                E(UpgradeProvisional, "Prices are provisional. Gold and materials come from your bag and the house chest. Nothing is ever lost: what doesn't fit goes to the Storage.", "Preços provisórios. Ouro e material saem da mochila e do Baú da casa. Nada se perde: o que não couber vai pro Depósito."),
                E(UpgradeConfirmTitle, "Improve the house?", "Melhorar a casa?"),
                E(UpgradeConfirmText, "The house becomes \"{0}\". It costs {1}.", "A casa vira \"{0}\". Custa {1}."),
                E(UpgradeConfirmYes, "Let's build!", "Bora construir!"),
                E(UpgradeConfirmNo, "Not yet", "Ainda não"),
                E(UpgradeDoneTitle, "A new house!", "Casa nova!"),
                E(UpgradeDoneText, "The family helped and the house is now \"{0}\"!", "A família ajudou e a casa agora é \"{0}\"!"),
                E(UpgradeMoved, "{0} piece(s) didn't fit where they were and are waiting in the Storage.", "{0} peça(s) não couberam onde estavam e estão esperando no Depósito."),
                E(UpgradeNothingMoved, "Everything stayed right where it was.", "Tudo ficou no mesmo lugar."),
                E(UpgradeBlocked, "Not yet — something is still missing (✗).", "Ainda não — falta alguma coisa (✗)."),
                E(UpgradePaymentFailed, "The gold or the materials weren't all there anymore. Nothing was spent.", "O ouro ou o material não estavam mais todos lá. Nada foi gasto."),
                E(UpgradeOk, "Everything ready to improve!", "Tudo pronto pra melhorar!"),
                E(SceneKey(2), "Joaquim carried the boards, Maria brought water for everyone and Lavine painted the porch door green.", "O Joaquim carregou as tábuas, a Maria trouxe água pra todo mundo e a Lavine pintou a porta da varanda de verde."),
                E(SceneKey(3), "Joaquim carried brick by brick, Ícaro \"optimized\" the clay mixer and Maithe climbed up to lay the roof tiles.", "O Joaquim carregou tijolo por tijolo, o Ícaro \"otimizou\" a betoneira de barro e a Maithe subiu no telhado pra pôr as telhas."),
                E(SceneKey(4), "Ícaro's mixer only exploded once, Maria served water and Lavine painted the stripe at the bottom of the walls.", "A betoneira do Ícaro só explodiu uma vez, a Maria serviu água e a Lavine pintou o barrado das paredes."),
                E(SceneKey(5), "The whole family came: Joaquim with the bricks, Ícaro with the mixer, Maithe on the roof, Maria with the water and Lavine painting the big door. Vó Zefa cried a little.", "A família inteira veio: o Joaquim com os tijolos, o Ícaro com a betoneira, a Maithe no telhado, a Maria com a água e a Lavine pintando a porta grande. A Vó Zefa chorou um pouquinho."),

                E(LifeTitle, "Life at home", "Vida na casa"),
                E(ComfortLine, "Comfort {0} — {1}", "Conforto {0} — {1}"),
                E(ComfortParts, "{0} from the pieces + {1} from the sets (each id counts at most twice).", "{0} das peças + {1} dos conjuntos (cada item conta no máximo 2 vezes)."),
                E(ComfortNext, "{0} more for \"{1}\".", "Faltam {0} pra \"{1}\"."),
                E(ComfortTop, "The best a home can be!", "Melhor que isso, só a casa da Vó!"),
                E(TierKey(0), "Tidy little house", "Casa arrumadinha"),
                E(TierKey(1), "Cosy", "Aconchegante"),
                E(TierKey(2), "Well looked after", "Caprichada"),
                E(TierKey(3), "Grandma's house", "Casa de Vó"),
                E(TierKey(4), "Dream home", "Lar dos sonhos"),
                E(RestedOn, "Rested: +{0}% XP for {1} more fight(s).", "Descansado: +{0}% de XP por mais {1} luta(s)."),
                E(RestedOff, "Not rested. Sleep in your bed at home to wake up Rested (XP only).", "Sem descanso. Durma na cama de casa pra acordar Descansado (só XP)."),
                E(RestCharges, "Nights with a bonus left today: {0}/{1}", "Noites com bônus hoje: {0}/{1}"),
                E(SleptRested, "Good morning! You woke up Rested: +{0}% XP for the next {1} fights.", "Bom dia! Acordou Descansado: +{0}% de XP nas próximas {1} lutas."),
                E(SleptNoCharge, "Good morning! Today's rest bonus was already used (twice a day).", "Bom dia! O bônus de descanso de hoje já foi usado (2 vezes por dia)."),
                E(SetsHeader, "Furniture sets", "Conjuntos"),
                E(SetsNone, "No set started yet. Three pieces of the same set already add comfort.", "Nenhum conjunto começado. Três peças do mesmo conjunto já dão conforto."),
                E(SetRow, "{0}: {1}/{2} {3}", "{0}: {1}/{2} {3}"),
                E(SetStage0, "(started)", "(começado)"),
                E(SetStage1, "(+5 comfort)", "(+5 de conforto)"),
                E(SetStage2, "(+12 comfort and an ambience)", "(+12 de conforto e efeito de ambiente)"),
                E(SetStage3, "(complete! +25 comfort)", "(completo! +25 de conforto)"),
                E(HouseTitles, "This house is known as: {0}", "Esta casa é conhecida como: {0}"),
                E(PerkSoon, "(its little gift comes soon)", "(o mimo chega em breve)"),
                E(PlantsHeader, "Plants", "Plantas"),
                E(PlantsNone, "No plants placed yet.", "Nenhuma planta colocada ainda."),
                E(PlantsHappy, "Every plant is happy.", "Todas as plantas estão felizes."),
                E(PlantsThirsty, "{0} plant(s) asking for water. They never die — they just droop.", "{0} planta(s) pedindo água. Elas nunca morrem — só ficam murchinhas."),
                E(WaterButton, "Water the plants", "Regar as plantas"),
                E(Watered, "You took Vó's watering can and watered everything. The plants perked right up!", "Você pegou o regador da Vó e regou tudo. As plantas se animaram na hora!"),
                E(WateredHappy, "The plants were already happy, but they liked the attention.", "As plantas já estavam felizes, mas gostaram do carinho."),
                E(Rain, "The rain watered {0} plant(s) in the yard.", "A chuva regou {0} planta(s) do quintal."),
                E(MoodHappy, "happy", "feliz"),
                E(MoodThirsty, "thirsty", "com sede"),
                E(MoodWilted, "drooping (water it and it comes back!)", "murchinha (rega que ela volta!)"),
                E(PlantStatus, "{0}: {1}", "{0}: {1}"),

                E(QuestsHeader, "House quests", "Missões da Casa"),
                E(QuestLocked, "(not yet)", "(ainda não)"),
                E(QuestActive, "(in progress)", "(em andamento)"),
                E(QuestReady, "(ready!)", "(pronta!)"),
                E(QuestCompleted, "(done)", "(feita)"),
                E(QuestDeliver, "Hand in", "Entregar"),
                E(QuestDoneStatus, "House quest done: {0}! The reward is in the Storage.", "Missão da Casa concluída: {0}! A recompensa está no Depósito."),
                E(WardrobeOwed, "A piece of clothing is waiting for the wardrobe (coming soon).", "Uma roupa está guardada esperando o guarda-roupa (em breve)."),

                E(GuestBookHeader, "Guest book", "Livro de visitas"),
                E(GuestBookEmpty, "Nobody has signed yet. Place the guest book and someone will drop by.", "Ninguém assinou ainda. Coloque o livro de visitas que alguém aparece."),
                E(GuestEntry, "Day {0} — {1}: \"{2}\"", "Dia {0} — {1}: \"{2}\""),
                E(VisitStatus, "{0} came to visit and signed the guest book!", "{0} veio visitar e assinou o livro de visitas!"),
                E(CousinsSigned, "Cousins who signed: {0}/{1}", "Primos que assinaram: {0}/{1}"),

                E(TrophyStatus, "Trophy wall: {0} of {1} bosses set free.", "Parede de troféus: {0} de {1} chefes libertados."),
                E(RelicTitle, "Showcase of the twelve relics", "Vitrine das doze relíquias"),
                E(RelicStatus, "{0} of {1} relics found. Each one lights up when you find it.", "{0} de {1} relíquias achadas. Cada uma acende quando é achada."),
                E(RelicMissing, "??? (still hidden somewhere)", "??? (ainda escondida por aí)"),
                E(ShelfOpening, "Opening Vó's Notebook…", "Abrindo o Caderno da Vó…"),
                E(ShelfUnavailable, "The Notebook can't open right now.", "O Caderno não abre agora."),
                E(PetNap, "Caramelo is taking a nap. Zzz… His tail still wags.", "O Caramelo está tirando uma soneca. Zzz… O rabo ainda abana."),
                E(Dummy, "A straw training dummy. For now it's just decoration — Joaquim swears he'll train tomorrow.", "Um boneco de treino de palha. Por enquanto é só enfeite — o Joaquim jura que vai treinar amanhã."),
                E(Ok, "OK", "OK"),
            };

            AddBosses(list);
            AddMaterials(list);
            AddSets(list);
            AddQuests(list);
            AddVisitors(list);
            return list;
        }

        private static void AddBosses(List<LocalizationEntry> list)
        {
            list.Add(E(BossKey("enemy_forest_alpha_wolf"), "Alpha Wolf", "Lobo Alfa"));
            list.Add(E(BossKey("enemy_desert_ruin_guardian"), "Ruin Guardian", "Guardião da Ruína"));
            list.Add(E(BossKey("enemy_mountain_warlord"), "Mountain Warlord", "Senhor da Guerra da Montanha"));
            list.Add(E(BossKey("enemy_cave_dweller"), "Ancient Dweller", "Habitante Ancestral"));
        }

        private static void AddMaterials(List<LocalizationEntry> list)
        {
            list.Add(E(MaterialKey(HouseUpgradeRules.GoldId), "Gold", "Ouro"));
            list.Add(E(MaterialKey(HouseUpgradeRules.WoodId), "Wood", "Madeira"));
            list.Add(E(MaterialKey(HouseUpgradeRules.FiberId), "Fiber", "Fibra"));
            list.Add(E(MaterialKey(HouseUpgradeRules.StoneId), "Stone", "Pedra"));
            list.Add(E(MaterialKey(HouseUpgradeRules.IronId), "Iron ingot", "Ferro (lingote)"));
            list.Add(E(MaterialKey(HouseQuestChain.CookedMeatId), "Roast meat", "Carne assada"));
        }

        private static void AddSets(List<LocalizationEntry> list)
        {
            (string id, string enName, string ptName, string enTitle, string ptTitle)[] sets =
            {
                ("set_varanda_vo_zefa", "Vó Zefa's Porch", "Varanda da Vó Zefa", "Grandma's porch", "Varanda de Vó"),
                ("set_tropeiro_vo_chico", "Vô Chico the Drover", "Tropeiro do Vô Chico", "Drover's house", "Casa de tropeiro"),
                ("set_casa_farinha", "Flour House", "Casa de Farinha", "Flour-house folk", "Gente da casa de farinha"),
                ("set_cozinha_vo", "Vó's Kitchen", "Cozinha da Vó", "Best kitchen around", "Cozinha mais gostosa do sertão"),
                ("set_oficina_icaro", "Ícaro's Workshop", "Oficina do Ícaro", "Crazy workshop", "Oficina maluca"),
                ("set_ventania_maithe", "Maithe's Gale", "Ventania da Maithe", "Windy house", "Casa dos ventos"),
                ("set_luz_maria", "Maria's Light", "Luz da Maria", "House of light", "Casa de luz"),
                ("set_raizes_joaquim", "Joaquim's Roots", "Raízes do Joaquim", "Deep-rooted house", "Casa de raiz"),
                ("set_noite_rubra_lavine", "Lavine's Crimson Night", "Noite Rubra da Lavine", "Starry house", "Casa estrelada"),
                ("set_cantinho_caramelo", "Caramelo's Corner", "Cantinho do Caramelo", "Caramelo's palace", "Palácio do Caramelo"),
                ("set_mata_fechada", "Deep Woods", "Mata Fechada", "Forest house", "Casa da mata"),
                ("set_duna_dourada", "Golden Dune", "Duna Dourada", "Desert house", "Casa do deserto"),
                ("set_pico_nevado", "Snowy Peak", "Pico Nevado", "Mountain house", "Casa da serra"),
                ("set_cristal_cavernas", "Cave Crystal", "Cristal das Cavernas", "Crystal house", "Casa de cristal"),
                ("set_feira_livre", "Street Market", "Feira Livre", "Market stall", "Barraca de feira"),
                ("set_cordel_xilogravura", "Cordel and Woodcut", "Cordel e Xilogravura", "Cordel house", "Casa de cordel"),
                ("set_horta_caprichada", "Tidy Garden", "Horta Caprichada", "Green thumbs", "Mão boa pra planta"),
                ("set_cristal_sol", "Sun Crystal", "Cristal do Sol", "Sunlit house", "Casa do sol"),
                ("set_arraia", "Arraiá", "Arraiá", "June-party house", "Casa de São João"),
                ("set_natal_sertanejo", "Backland Christmas", "Natal Sertanejo", "Christmas house", "Casa de Natal"),
                ("set_frevo_maracatu", "Frevo and Maracatu", "Frevo e Maracatu", "Carnival house", "Casa de carnaval"),
            };

            foreach ((string id, string enName, string ptName, string enTitle, string ptTitle) in sets)
            {
                HouseSetDefinition set = HouseSetCatalog.Get(id) ?? new HouseSetDefinition(id);
                list.Add(E(set.NameKey, enName, ptName));
                list.Add(E(set.TitleKey, enTitle, ptTitle));
            }
        }

        private static void AddQuests(List<LocalizationEntry> list)
        {
            void Quest(string id, string enName, string ptName, string enGoal, string ptGoal)
            {
                HouseQuestDefinition quest = HouseQuestChain.Get(id);
                list.Add(E(quest.NameKey, enName, ptName));
                list.Add(E(quest.GoalKey, enGoal, ptGoal));
            }

            Quest(HouseQuestChain.AguaFresca, "Fresh Water", "Água Fresca", "Bring 10 stone and 5 wood for a stone well.", "Traga 10 pedras e 5 madeiras pra um poço de pedra.");
            Quest(HouseQuestChain.FogoNaCozinha, "Fire in the Kitchen", "Fogo na Cozinha", "After the Alpha Wolf: bring 3 roast meat and 8 wood for a wood stove.", "Depois do Lobo Alfa: traga 3 carnes assadas e 8 madeiras pra um fogão a lenha.");
            Quest(HouseQuestChain.ParedeDeBarro, "Clay Walls", "Parede de Barro", "Improve the house to rammed earth and tiles.", "Melhore a casa pra taipa e telha.");
            Quest(HouseQuestChain.TijoloPorTijolo, "Brick by Brick", "Tijolo por Tijolo", "Improve the house to brick.", "Melhore a casa pra alvenaria.");
            Quest(HouseQuestChain.CasaCheia, "Full House", "Casa Cheia", "Have the manor and four cousins sign the guest book.", "Tenha o casarão e quatro primos assinando o livro de visitas.");
        }

        private static void AddVisitors(List<LocalizationEntry> list)
        {
            (string id, string en, string pt, string[] enLines, string[] ptLines)[] visitors =
            {
                ("Maithe", "Maithe", "Maithe",
                    new[] { "Look at that — {0}! Nice taste, cousin.", "I came running from the woods just to see the house.", "If you need a lookout on the roof, call me." },
                    new[] { "Olha só — {0}! Bom gosto, primo.", "Vim correndo da mata só pra ver a casa.", "Se precisar de vigia no telhado, me chama." }),
                ("Maria", "Maria", "Maria",
                    new[] { "I prayed a little for {0}. Things last longer that way.", "I brought herb tea. Drink it before it gets cold!", "This house smells like a hug." },
                    new[] { "Rezei um pouquinho pro(a) {0}. Assim dura mais.", "Trouxe chá de erva. Bebe antes de esfriar!", "Esta casa tem cheiro de abraço." }),
                ("Joaquim", "Joaquim", "Joaquim",
                    new[] { "{0}? I'd have made it stronger. But it's pretty.", "Anything heavy to carry? I'm here.", "Tomorrow I'll train on that dummy. For real." },
                    new[] { "{0}? Eu faria mais forte. Mas tá bonito.", "Tem coisa pesada pra carregar? Tô aqui.", "Amanhã eu treino naquele boneco. Sério." }),
                ("Icaro", "Ícaro", "Ícaro",
                    new[] { "I could put a little engine in {0}. Just a small one.", "I measured the house: it fits three more inventions.", "Don't touch the gear on the shelf, it's calibrated." },
                    new[] { "Dava pra pôr um motorzinho no(a) {0}. Só um pequeno.", "Medi a casa: cabem mais três invenções.", "Não mexe na engrenagem da estante, tá calibrada." }),
                ("Lavine", "Lavine", "Lavine",
                    new[] { "{0} looks beautiful under the moon.", "I drew the house in my notebook. With stars.", "At night the shadows here are soft. I like it." },
                    new[] { "{0} fica lindo(a) debaixo da lua.", "Desenhei a casa no meu caderno. Com estrelas.", "De noite as sombras daqui são macias. Gosto." }),
                (HouseVisitRules.VoZefa, "Vó Zefa", "Vó Zefa",
                    new[] { "Oh, {0}! Just like the one I had when I was a girl.", "Your grandpa would be proud of this house.", "I left a pot of rapadura in the kitchen. Don't tell the others." },
                    new[] { "Ai, {0}! Igualzinho ao que eu tinha quando era menina.", "Seu avô ia ter orgulho desta casa.", "Deixei um pote de rapadura na cozinha. Não conta pros outros." }),
                (HouseVisitRules.DonaCida, "Dona Cida", "Dona Cida",
                    new[] { "I blessed {0} with rue. Now it's protected.", "A house with plants is a house with good luck.", "Come by the haberdashery, I've got new lace." },
                    new[] { "Benzi o(a) {0} com arruda. Agora tá protegido(a).", "Casa com planta é casa com sorte.", "Passa no armarinho, chegou renda nova." }),
                (HouseVisitRules.SeuBento, "Seu Bento", "Seu Bento",
                    new[] { "On my travels I never saw a {0} like this one.", "I stopped my mule at the gate. She liked the yard.", "From Areia Branca to here — it was worth the trip." },
                    new[] { "Nas minhas viagens nunca vi {0} igual.", "Parei a mula na porteira. Ela gostou do quintal.", "De Areia Branca até aqui — valeu a viagem." }),
                (HouseVisitRules.MestreTonho, "Mestre Tonho", "Mestre Tonho",
                    new[] { "{0}: good work. Solid.", "These walls will stand a hundred years.", "If you need a hinge, the forge is open." },
                    new[] { "{0}: trabalho bom. Firme.", "Essas paredes aguentam cem anos.", "Se precisar de dobradiça, a forja tá aberta." }),
                (HouseVisitRules.Benedita, "Benedita", "Benedita",
                    new[] { "{0} would look even prettier with a lamp beside it.", "Your house shines from far away, like a lamp in the cave.", "I brought a candle for the window." },
                    new[] { "{0} ia ficar mais bonito(a) com uma lamparina do lado.", "Sua casa brilha de longe, que nem lamparina na caverna.", "Trouxe uma vela pra janela." }),
            };

            foreach ((string id, string en, string pt, string[] enLines, string[] ptLines) in visitors)
            {
                list.Add(E(VisitorKey(id), en, pt));
                for (int i = 0; i < HouseVisitRules.LinesPerVisitor; i++)
                {
                    list.Add(E(VisitLineKey(id, i), enLines[i], ptLines[i]));
                }
            }
        }

        private static LocalizationEntry E(string key, string en, string ptBR) => new LocalizationEntry(key, en, ptBR);

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

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
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key)) return manager.Get(key);
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
            if (key == null) return null;
            if (_fallbackByKey == null)
            {
                var table = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries) table[entry.Key] = entry.PtBR;
                _fallbackByKey = table;
            }

            return _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }
    }
}
