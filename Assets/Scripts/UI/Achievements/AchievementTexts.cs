using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.UI.Achievements
{
    /// <summary>
    /// Localization entries owned by the Achievements feature: UI chrome (<c>ui.achievement.*</c>) plus a
    /// title/description pair per seeded achievement (<c>achievement.&lt;id&gt;.title/.description</c>,
    /// matching the keys stored in <c>Resources/Achievements/*.asset</c>). Same pattern as
    /// <c>QuestLogTexts</c>: kept out of <c>LocalizationManager.DefaultEntries</c> so this feature never
    /// edits that shared file, and merged at runtime by <see cref="EnsureLoaded"/>, which only adds
    /// missing keys — a CSV/LocalizationTable defining the same keys always wins.
    ///
    /// Story achievements follow the canonical family tree (Docs/GDD.md): Maithe and Ícaro are
    /// siblings, Joaquim and Maria are siblings, and Lavine — the youngest — is cousin to all four.
    /// </summary>
    public static class AchievementTexts
    {
        public const string TitleKey = "ui.achievement.title";
        public const string CloseKey = "ui.achievement.close";
        public const string ToastHeaderKey = "ui.achievement.toast_unlocked";
        public const string HiddenTitleKey = "ui.achievement.hidden_title";
        public const string HiddenDescriptionKey = "ui.achievement.hidden_description";
        public const string SummaryKey = "ui.achievement.summary";
        public const string UnlockedBadgeKey = "ui.achievement.unlocked_badge";
        public const string EmptyKey = "ui.achievement.empty";

        public static readonly List<LocalizationEntry> All = new List<LocalizationEntry>
        {
            new LocalizationEntry(TitleKey, "Achievements", "Conquistas"),
            new LocalizationEntry(CloseKey, "Close", "Fechar"),
            new LocalizationEntry(ToastHeaderKey, "Achievement unlocked!", "Conquista desbloqueada!"),
            new LocalizationEntry(HiddenTitleKey, "???", "???"),
            new LocalizationEntry(HiddenDescriptionKey, "A secret achievement. Keep going to find out.", "Conquista secreta. Siga a jornada para descobrir."),
            new LocalizationEntry(SummaryKey, "{0}/{1} unlocked", "{0}/{1} desbloqueadas"),
            new LocalizationEntry(UnlockedBadgeKey, "Unlocked", "Desbloqueada"),
            new LocalizationEntry(EmptyKey, "No achievements available.", "Nenhuma conquista disponível."),

            // General progression.
            Title("ach_first_blood", "First Spark", "Primeira Faísca"),
            Description("ach_first_blood", "Defeat your first enemy and set its spark free.", "Derrote seu primeiro inimigo e liberte a faísca dele."),
            Title("ach_monster_hunter", "Dawn Isle Hunter", "Caçador da Ilha da Aurora"),
            Description("ach_monster_hunter", "Defeat 100 enemies.", "Derrote 100 inimigos."),
            Title("ach_boss_slayer", "Giant Felled", "Gigante Tombado"),
            Description("ach_boss_slayer", "Defeat a boss.", "Derrote um chefe."),
            Title("ach_first_quest", "Helping Hand", "Mão Amiga"),
            Description("ach_first_quest", "Complete your first quest.", "Conclua sua primeira missão."),
            Title("ach_level_10", "Seasoned", "Calejado"),
            Description("ach_level_10", "Reach level 10.", "Alcance o nível 10."),
            Title("ach_first_craft", "Handmade", "Feito à Mão"),
            Description("ach_first_craft", "Craft your first item.", "Fabrique seu primeiro item."),
            Title("ach_master_crafter", "Master Artisan", "Mestre Artesão"),
            Description("ach_master_crafter", "Craft 50 items.", "Fabrique 50 itens."),
            Title("ach_skill_adept", "Honed Gift", "Dom Aprimorado"),
            Description("ach_skill_adept", "Cast skills 100 times.", "Use habilidades 100 vezes."),
            Title("ach_first_combo", "In Sync", "Em Sintonia"),
            Description("ach_first_combo", "Complete a skill combo.", "Complete um combo de habilidades."),
            Title("ach_explorer", "Trailblazer", "Desbravador"),
            Description("ach_explorer", "Unlock 3 regions on the island map.", "Desbloqueie 3 regiões no mapa da ilha."),
            Title("ach_merchant", "Shrewd Trader", "Bom de Negócio"),
            Description("ach_merchant", "Earn 1000 gold selling to merchants.", "Ganhe 1000 de ouro vendendo aos mercadores."),

            // The five heroes' personal quests.
            Title("ach_hero_maithe", "Eye of the Wind", "Olho do Vento"),
            Description("ach_hero_maithe", "Finish Maithe's personal quest — the eldest of the cousins always finds the trail.", "Conclua a missão pessoal da Maithe — a mais velha dos primos sempre acha a trilha."),
            Title("ach_hero_maria", "Light That Won't Fade", "Luz que Não se Apaga"),
            Description("ach_hero_maria", "Finish Maria's personal quest and light the crystals up again.", "Conclua a missão pessoal da Maria e acenda os cristais de novo."),
            Title("ach_hero_joaquim", "Echoes of Home", "Ecos de Casa"),
            Description("ach_hero_joaquim", "Finish Joaquim's personal quest — and still be home before the lanterns are lit.", "Conclua a missão pessoal do Joaquim — e ainda volte antes de acenderem as lanternas."),
            Title("ach_hero_icaro", "Ancient Gears", "Engrenagens Antigas"),
            Description("ach_hero_icaro", "Finish Ícaro's personal quest and decipher the Great Ushabti's inscriptions.", "Conclua a missão pessoal do Ícaro e decifre as inscrições do Grande Ushabti."),
            Title("ach_hero_lavine", "The Youngest's Courage", "Coragem da Caçula"),
            Description("ach_hero_lavine", "Finish Lavine's personal quest — brave on her own, at last.", "Conclua a missão pessoal da Lavine — corajosa sozinha, enfim."),

            // Family bonds.
            Title("ach_siblings_wind_gears", "Wind and Gears", "Vento e Engrenagem"),
            Description("ach_siblings_wind_gears", "Finish the personal quests of siblings Maithe and Ícaro.", "Conclua as missões pessoais dos irmãos Maithe e Ícaro."),
            Title("ach_siblings_shield_light", "Shield and Light", "Escudo e Luz"),
            Description("ach_siblings_shield_light", "Finish the personal quests of siblings Joaquim and Maria.", "Conclua as missões pessoais dos irmãos Joaquim e Maria."),
            Title("ach_cousin_of_all", "Everyone's Cousin", "Prima de Todos"),
            Description("ach_cousin_of_all", "Finish all five personal quests. Lavine always knew her cousins would make it.", "Conclua as cinco missões pessoais. A Lavine sempre soube que os primos iam conseguir."),
            Title("ach_family_reunited", "Together, or Not at All", "Juntos, ou Nada Feito"),
            Description("ach_family_reunited", "Answer Maria's call and reunite the whole family.", "Atenda ao chamado da Maria e reúna a família inteira."),

            // Campaign (StoryProgress raises CampaignCompletedEvent when the Cavernas boss falls).
            Title("ach_campaign_complete", "The Light Comes Home", "A Luz Volta para Casa"),
            Description("ach_campaign_complete", "Beat the guardian of the Threshold Caves and bring the light back to the village. Grandma Edna knew you would.", "Vença o guardião das Grutas do Limiar e devolva a luz à vila. A Vó Edna sabia que vocês iam conseguir."),

            // Nova Jornada+ (NewGamePlusProgress raises NewGamePlusCycleCompletedEvent when a NG+ campaign is finished).
            Title("ach_new_journey_plus", "Lighting It Up Again", "Acender de Novo"),
            Description("ach_new_journey_plus", "Finish New Journey+ 1: relight the Sun Crystal a second time. Grandma Edna already has the jar of Golden Brick ready.", "Conclua a Nova Jornada+ 1: acenda o Cristal do Sol pela segunda vez. A Vó Edna já deixou o pote de Torrão Dourado pronto."),

            // Bestiary (BestiaryProgress raises BestiaryCompletedEvent when the last page unlocks).
            Title("ach_bestiary_complete", "Grandma's Notebook, Cover to Cover", "Caderno da Vó, de Capa a Capa"),
            Description("ach_bestiary_complete", "Defeat every kind of creature and boss so Grandma Edna can fill in every page of her notebook.", "Vença todo tipo de bicho e chefe para a Vó Edna preencher todas as páginas do caderno."),
            Title("ach_relics_all", "Grandpa Jorge's Keepsakes", "Lembranças do Vô Jorge"),
            Description("ach_relics_all", "Find all 12 family keepsakes Grandpa Jorge hid around Dawn Isle.", "Encontre as 12 lembranças da família que o Vô Jorge escondeu pela Ilha da Aurora."),

            // Fishing (FishingProgress feeds every new species through RecordProgress).
            Title("ach_fish_all", "Master of the Waters", "Mestre das Águas"),
            Description("ach_fish_all", "Catch all 14 kinds of fish in Faro's Pool, the Tugai Stream and the Threshold Lake.", "Pesque as 14 espécies de peixe do Remanso de Faro, do Riacho do Tugai e do Lago do Limiar."),
            Title("ach_fish_golden", "Grandpa Jorge's Legend", "A Lenda do Vô Jorge"),
            Description("ach_fish_golden", "Catch the Golden Fish of Faro's Pool. It only bites at dawn or dusk.", "Pesque o Peixe-Dourado do Remanso de Faro. Ele só morde de madrugada ou ao entardecer."),

            // Night (the rare Forest night mini-boss).
            Title("ach_night_werewolf", "Full Moon, Full Courage", "Lua Cheia, Coragem Cheia"),
            Description("ach_night_werewolf", "Defeat Mardagayl, the Seven-Night Wolf, the rare night visitor of the Zarafshan Woods, and break its spell. When it howls, dodge!", "Vença o Mardagayl, o Lobo-das-Sete-Noites, raro visitante das noites do Bosque do Zarafshan, e desfaça o encanto. Quando ele uivar, esquive!"),

            // Timed event rooms (EventRoomProgress feeds clears and the best score).
            Title("ach_room_terreiro_clear", "Nobody Spooks Us", "Ninguém Assusta a Gente"),
            Description("ach_room_terreiro_clear", "Hold out in the Mazan Yard until the timer ends.", "Aguente no Pátio dos Mazans até o tempo acabar."),
            Title("ach_room_torre_clear", "The Wheel Turns Again", "A Roda Gira de Novo"),
            Description("ach_room_torre_clear", "Take the Golden Jar back to Adda the Tower Keeper in the Tower of the Twelve-Jar Wheel.", "Leve a Jarra de Ouro até Adda, o Vigia da Torre, na Torre da Roda das Doze Jarras."),
            Title("ach_room_master", "Master of the Rooms", "Mestre das Salas"),
            Description("ach_room_master", "Score 7000 points in the Mazan Yard or the Tower of the Jars.", "Faça 7000 pontos no Pátio dos Mazans ou na Torre das Jarras."),

            // Tacho Encantado (TachoService feeds wings made and the highest wing level).
            Title("ach_wings_first", "Up and Away", "Bateu Asas"),
            Description("ach_wings_first", "Make your first pair of wings in Grandma Edna's Uasamonga.", "Faça o primeiro par de asas na Uasamonga da Vó Edna."),
            Title("ach_wings_level3", "Above Dawn Isle", "Por Cima da Ilha da Aurora"),
            Description("ach_wings_level3", "Strengthen a pair of wings up to level 3 in the Uasamonga.", "Fortaleça um par de asas até o nível 3 na Uasamonga."),

            // World bosses of Regions 10–16 (EnemyDiedEvent with WasBoss; three of them only ever withdraw).
            Title("ach_boss_wind_king", "The Gale Stops", "O Vendaval Parou"),
            Description("ach_boss_wind_king", "Defeat the Wind King in the Picos de Farnkesh.", "Vença o Rei-Vendaval nos Picos de Farnkesh."),
            Title("ach_boss_old_kul", "Thin Ice, Old Kul", "Gelo Fino, Velho Kul"),
            Description("ach_boss_old_kul", "Defeat Old Kul at the bottom of the Frozen Sea.", "Vença o Velho Kul no fundo do Mar Gelado."),
            Title("ach_boss_ember_glutton", "Enough Embers", "Chega de Brasa"),
            Description("ach_boss_ember_glutton", "Defeat the Ember Glutton in the caldera of Hinokura.", "Vença o Glutão de Brasa na caldeira de Hinokura."),
            Title("ach_boss_apep", "Back Tomorrow Night", "Volta Amanhã à Noite"),
            Description("ach_boss_apep", "Make Apep withdraw from her sandbank. She drinks the river, and she will be back the next night.", "Faça Apep recuar do banco de areia. Ela bebe o rio, e volta na noite seguinte."),
            Title("ach_boss_dream", "Wide Awake", "Bem Acordados"),
            Description("ach_boss_dream", "Defeat the Dream of Destruction on the Tablets of the Dream.", "Vença o Sonho da Destruição nas Tábuas do Sonho."),
            Title("ach_boss_great_dark", "Five Faces, One Lamp", "Cinco Caras, Uma Lâmpada"),
            Description("ach_boss_great_dark", "Make the Great Dark withdraw at the Gate of the Land of Darkness, through all its forms.", "Faça a Grande Treva recuar no Portão da Terra da Treva, passando por todas as suas formas."),
            Title("ach_boss_az_captain", "The Ark Comes Up Empty", "A Arca Voltou Vazia"),
            Description("ach_boss_az_captain", "Make Āz, the Greedy Captain, withdraw in the Garden of Light.", "Faça Āz, a Capitã Cobiçosa, recuar no Jardim de Luz."),
            Title("ach_bosses_far_all", "Seven Doors Shut", "Sete Portas Fechadas"),
            Description("ach_bosses_far_all", "Face and overcome all seven world bosses of Regions 10 to 16.", "Enfrente e supere os sete chefes do mundo das Regiões 10 a 16."),

            // One particular cousin (HeroDeed: boss beaten with that hero selected).
            Title("ach_cousin_maithe_wind", "Wind Knows Wind", "Vento Conhece Vento"),
            Description("ach_cousin_maithe_wind", "As Maithe, defeat the Wind King. The storm never did move her.", "Como Maithe, vença o Rei-Vendaval. A ventania nunca conseguiu movê-la."),
            Title("ach_cousin_joaquim_apep", "Close Enough to Stay Dry", "Perto o Bastante para Não se Molhar"),
            Description("ach_cousin_joaquim_apep", "As Joaquim, drive off Apep. Only those who stand close escape the swallow, and he never was one to step back.", "Como Joaquim, afaste Apep. Só escapa do gole quem fica perto, e ele nunca foi de dar um passo atrás."),
            Title("ach_cousin_maria_dark", "Lit From Within", "Acesa por Dentro"),
            Description("ach_cousin_maria_dark", "As Maria, make the Great Dark withdraw. The light in the family was always hers to carry.", "Como Maria, faça a Grande Treva recuar. A luz da família sempre foi ela quem carregou."),
            Title("ach_cousin_icaro_dream", "Stood Exactly Still", "Parado no Lugar Certo"),
            Description("ach_cousin_icaro_dream", "As Ícaro, defeat the Dream of Destruction. He worked out where the ring doesn't fall and wrote it down.", "Como Ícaro, vença o Sonho da Destruição. Ele calculou onde o anel não cai e anotou no caderno."),
            Title("ach_cousin_lavine_az", "The Youngest Asks for Nothing", "A Caçula Não Pediu Nada"),
            Description("ach_cousin_lavine_az", "As Lavine, make Āz withdraw. Greed has nothing to offer someone who only came for her cousins.", "Como Lavine, faça Āz recuar. A cobiça não tem o que oferecer a quem só veio pelos primos."),

            // Region mechanics (RegionFeatEvent from the drivers of World/Map/Mechanics).
            Title("ach_nartes_race_gold", "First Across the Steppe", "Primeiro na Estepe"),
            Description("ach_nartes_race_gold", "Win a gold medal in the steppe race of the Nartes.", "Ganhe medalha de ouro na corrida da Estepe dos Nartes."),
            Title("ach_lavine_sky_golds", "Only the Youngest", "Só a Caçula"),
            Description("ach_lavine_sky_golds", "Lift all four Golds of Heaven in the kurgans. They burn everyone else's hands; only Lavine can pick them up.", "Levante os quatro Ouros do Céu nos kurgans. Eles queimam a mão dos outros; só a Lavine consegue pegar."),
            Title("ach_amirani_chain_top", "All the Way Up the Links", "Até a Última Argola"),
            Description("ach_amirani_chain_top", "Climb Amirani's Chain to the very top, gusts and all.", "Suba a Corrente de Amirani até o topo, com rajada e tudo."),
            Title("ach_porch_story", "Grandpa's Chair", "A Cadeira do Vô"),
            Description("ach_porch_story", "Sit in Grandpa Jorge's chair on the Porch and hear a story through to the end.", "Sente na cadeira do Vô Jorge na Varanda e ouça uma história até o fim."),
            Title("ach_sky_whole", "Holding Up the Sky", "Segurando o Céu"),
            Description("ach_sky_whole", "Seal all the cracks of the Lookout so the sky is whole again.", "Sele todas as rachaduras do Mirante até o céu ficar inteiro de novo."),

            // The whole road (AreaChangedEvent.CurrentPart).
            Title("ach_world_all_regions", "Every Road, All the Way", "Todas as Estradas, Até o Fim"),
            Description("ach_world_all_regions", "Set foot in all 16 regions of the world, from the Zarafshan Woods to the Kingdoms of the Sky.", "Pise nas 16 regiões do mundo, do Bosque do Zarafshan aos Reinos do Céu."),

            // "As Cartas da Estrada" (LateStoryCatalog).
            Title("ach_letters_complete", "The Last Letter", "A Última Carta"),
            Description("ach_letters_complete", "Follow the Letters of the Road all the way to the Porch.", "Siga as Cartas da Estrada até a Varanda."),
            Title("ach_letters_all_cousins", "Five Readings of the Same Letters", "Cinco Leituras das Mesmas Cartas"),
            Description("ach_letters_all_cousins", "Finish the Letters of the Road once with each of the five cousins. Grandpa Jorge wrote a different line for every one of them.", "Termine as Cartas da Estrada uma vez com cada um dos cinco primos. O Vô Jorge escreveu uma linha diferente para cada um."),

            // New mounts (MountGrantedEvent).
            Title("ach_mounts_far_regions", "A Stable from Far Away", "Estrebaria de Longe"),
            Description("ach_mounts_far_regions", "Own all eight mounts of the far regions, from the golden goat of the Satyr Vale to the dragon-serpent of Dur-Jarras.", "Tenha as oito montarias das regiões distantes, do bode dourado do Vale dos Sátiros ao dragão-serpente de Dur-Jarras."),
            Title("ach_column_griffin", "Wings of the Column", "Asas da Coluna"),
            Description("ach_column_griffin", "Earn the Griffin of the Column of Glory, the first mount that truly flies.", "Conquiste o Grifo da Coluna da Glória, a primeira montaria que voa de verdade."),

            // New furniture sets (ItemPurchasedEvent / HouseItemGrantedEvent).
            Title("ach_house_cais_de_luz", "A Room on the Quay of Light", "Um Cômodo no Cais de Luz"),
            Description("ach_house_cais_de_luz", "Collect all 8 pieces of the Quay of Light furniture.", "Reúna as 8 peças dos Móveis do Cais de Luz."),
            Title("ach_house_far_sets", "Furnished from Far Away", "Mobiliada de Longe"),
            Description("ach_house_far_sets", "Collect all 40 pieces of the five furniture sets from Regions 11 to 16.", "Reúna as 40 peças dos cinco conjuntos de móveis das Regiões 11 a 16."),
            Title("ach_house_far_sets_second", "Furnished to the Last Shore", "Mobiliada até a Última Margem"),
            Description("ach_house_far_sets_second", "Collect all 48 pieces of the six newer furniture sets from Regions 10 to 16: Stone Nest, Lacquer Palace, Reed Fields, Sundial, Bōlos and Moon Ship.", "Reúna as 48 peças dos seis conjuntos de móveis mais novos das Regiões 10 a 16: Ninho de Pedra, Palácio de Laca, Campo de Juncos, Relógio de Sol, Novelo e Barca da Lua."),
        };

        private static LocalizationEntry Title(string id, string en, string ptBR) => new LocalizationEntry($"achievement.{id}.title", en, ptBR);
        private static LocalizationEntry Description(string id, string en, string ptBR) => new LocalizationEntry($"achievement.{id}.description", en, ptBR);

        /// <summary>Merges <see cref="All"/> into <paramref name="localization"/>, skipping keys it already has (external tables take precedence). Safe to call repeatedly.</summary>
        public static void EnsureLoaded(LocalizationManager localization)
        {
            if (localization == null)
            {
                return;
            }

            localization.LoadEntries(All.Where(entry => !localization.HasKey(entry.Key)));
        }

        /// <summary>Fallback text when no LocalizationManager is registered (e.g. a test scene): PT-BR, the project's default UI language.</summary>
        public static string GetFallback(string key)
        {
            LocalizationEntry entry = All.FirstOrDefault(e => e.Key == key);
            return entry != null ? entry.GetText(LanguageCode.ptBR) : $"[{key}]";
        }

        /// <summary>Localizes through <paramref name="localization"/> when available, otherwise <see cref="GetFallback"/>.</summary>
        public static string Localize(LocalizationManager localization, string key)
        {
            return localization != null ? localization.Get(key) : GetFallback(key);
        }
    }
}
