using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Quests
{
    /// <summary>
    /// Localization entries for the daily quests under <c>Resources/Quests/Daily</c>: title,
    /// description, one line per objective and a "Diário" journal note (<c>quest.*.journal</c>, the
    /// key <c>QuestLogPresenter.GetJournalKey</c> derives from the title key). Same runtime-merge
    /// pattern as <c>QuestLogTexts</c>/<c>QuestGiverTexts</c> so the shared
    /// <c>LocalizationManager.DefaultEntries</c> stays untouched; <see cref="DailyQuestService"/>
    /// merges these on Initialize, and a CSV/LocalizationTable defining the same keys always wins.
    ///
    /// Small chores for the five cousins (Maithe 14, Maria 13, Joaquim 12, Ícaro 11, Lavine 9 —
    /// Maithe/Ícaro and Maria/Joaquim are siblings, Lavine is everyone's younger cousin).
    /// </summary>
    public static class DailyQuestTexts
    {
        public static readonly List<LocalizationEntry> All = new List<LocalizationEntry>
        {
            // quest_daily_maithe_wolf_patrol
            new LocalizationEntry("quest.daily_maithe_wolf_patrol.title", "Morning Patrol", "Ronda da Manhã"),
            new LocalizationEntry("quest.daily_maithe_wolf_patrol.description",
                "Wolves were howling near the orchard again. Maithe walks the trail before the little ones wake up.",
                "Os lobos voltaram a uivar perto do pomar. A Maithe faz a ronda na trilha antes dos pequenos acordarem."),
            new LocalizationEntry("quest.daily_maithe_wolf_patrol.obj_hunt_wolves", "Drive off tugai wolves", "Afaste os lobos do tugai"),
            new LocalizationEntry("quest.daily_maithe_wolf_patrol.journal",
                "Ícaro thinks patrols are boring. He's right, and that's the point: a boring morning means everyone got home safe.",
                "O Ícaro acha ronda uma chatice. Ele tem razão, e é essa a ideia: manhã chata quer dizer que todo mundo voltou pra casa inteiro."),

            // quest_daily_joaquim_spider_sweep
            new LocalizationEntry("quest.daily_joaquim_spider_sweep.title", "Web Sweep", "Varredura de Teias"),
            new LocalizationEntry("quest.daily_joaquim_spider_sweep.description",
                "Spiders keep weaving across the path to the creek. Joaquim raises his shield and clears the way.",
                "As aranhas não param de tecer teias no caminho do riacho. O Joaquim ergue o escudo e abre passagem."),
            new LocalizationEntry("quest.daily_joaquim_spider_sweep.obj_clear_spiders", "Clear out dimmed weaver spiders", "Espante as aranhas-tecelãs apagadas"),
            new LocalizationEntry("quest.daily_joaquim_spider_sweep.journal",
                "Lavine is scared of spiders but won't admit it. So I go first. Every day, if I have to.",
                "A Lavine morre de medo de aranha, mas não admite. Então eu vou na frente. Todo dia, se precisar."),

            // quest_daily_maria_herb_basket
            new LocalizationEntry("quest.daily_maria_herb_basket.title", "Maria's Herb Basket", "A Cesta de Ervas da Maria"),
            new LocalizationEntry("quest.daily_maria_herb_basket.description",
                "Somebody always comes back scraped. Maria refills her basket and brews a fresh potion for the family.",
                "Sempre tem alguém voltando ralado. A Maria enche a cesta de ervas e prepara uma poção fresquinha pra família."),
            new LocalizationEntry("quest.daily_maria_herb_basket.obj_pick_herbs", "Pick herbs", "Colha ervas"),
            new LocalizationEntry("quest.daily_maria_herb_basket.obj_brew_potion", "Have a health potion ready", "Tenha uma poção de vida pronta"),
            new LocalizationEntry("quest.daily_maria_herb_basket.journal",
                "Joaquim says my potions taste like old socks. He still drinks every drop.",
                "O Joaquim diz que minhas poções têm gosto de meia velha. Mesmo assim ele bebe até a última gota."),

            // quest_daily_icaro_ore_tinkering
            new LocalizationEntry("quest.daily_icaro_ore_tinkering.title", "Spare Parts", "Peças de Reposição"),
            new LocalizationEntry("quest.daily_icaro_ore_tinkering.description",
                "Ícaro's newest engine ate all his metal. He needs fresh ore and one good ingot before Maithe notices the mess.",
                "A nova invenção do Ícaro devorou todo o metal dele. Ele precisa de minério e de um bom lingote antes que a Maithe veja a bagunça."),
            new LocalizationEntry("quest.daily_icaro_ore_tinkering.obj_dig_ore", "Gather iron ore", "Junte minério de ferro"),
            new LocalizationEntry("quest.daily_icaro_ore_tinkering.obj_smelt_ingot", "Have an iron ingot", "Tenha um lingote de ferro"),
            new LocalizationEntry("quest.daily_icaro_ore_tinkering.journal",
                "If it works today, I'll show my sister. If it explodes, I'll show Lavine — she always laughs.",
                "Se funcionar hoje, mostro pra minha irmã. Se explodir, mostro pra Lavine — ela sempre ri."),

            // quest_daily_lavine_bat_chase
            new LocalizationEntry("quest.daily_lavine_bat_chase.title", "Lavine and the Bats", "A Lavine e os Morcegos"),
            new LocalizationEntry("quest.daily_lavine_bat_chase.description",
                "A bat swarm keeps stealing fruit from the drying racks. Lavine wants to prove the cave is no place to hide from her.",
                "Um enxame de morcegos vive roubando as frutas do varal de secar. A Lavine quer provar que caverna nenhuma esconde deles."),
            new LocalizationEntry("quest.daily_lavine_bat_chase.obj_shoo_bats", "Scatter the bat swarms", "Espante os enxames de morcegos"),
            new LocalizationEntry("quest.daily_lavine_bat_chase.journal",
                "Maria made me promise to stay near the entrance. I did! Mostly.",
                "A Maria me fez prometer ficar perto da entrada. E eu fiquei! Quase sempre."),

            // quest_daily_family_campfire
            new LocalizationEntry("quest.daily_family_campfire.title", "Campfire Night", "Noite de Fogueira"),
            new LocalizationEntry("quest.daily_family_campfire.description",
                "The cousins want a campfire tonight with Grandpa Jorge's stories and roasted chestnuts. Someone has to bring wood and fiber for the torches.",
                "Os primos querem fogueira hoje à noite, com as histórias do Vô Jorge e castanha assada. Alguém tem que trazer lenha e fibra pras tochas."),
            new LocalizationEntry("quest.daily_family_campfire.obj_gather_wood", "Gather wood", "Junte lenha"),
            new LocalizationEntry("quest.daily_family_campfire.obj_gather_fiber", "Gather fiber", "Junte fibra"),
            new LocalizationEntry("quest.daily_family_campfire.journal",
                "Maithe tells the scary stories, Joaquim pretends not to flinch, Maria hums, Ícaro pokes the fire and Lavine falls asleep first. Every time.",
                "A Maithe conta as histórias de medo, o Joaquim finge que não se assusta, a Maria cantarola, o Ícaro cutuca o fogo e a Lavine é a primeira a dormir. Toda vez."),

            // quest_daily_joaquim_scorpion_watch
            new LocalizationEntry("quest.daily_joaquim_scorpion_watch.title", "Scorpion Watch", "Vigia dos Escorpiões"),
            new LocalizationEntry("quest.daily_joaquim_scorpion_watch.description",
                "Scorpions crawl out of the dunes when the sun gets hot. Joaquim keeps them away from the travelers' road.",
                "Os escorpiões saem das dunas quando o sol esquenta. O Joaquim mantém todos longe da estrada dos viajantes."),
            new LocalizationEntry("quest.daily_joaquim_scorpion_watch.obj_sting_watch", "Defeat Flaming Sands scorpions", "Derrote escorpiões das Areias Flamejantes"),
            new LocalizationEntry("quest.daily_joaquim_scorpion_watch.journal",
                "Maria packed me water and a lecture. I drank the water.",
                "A Maria me deu água e um sermão pra levar. A água eu bebi."),

            // quest_daily_maria_visit_survivor
            new LocalizationEntry("quest.daily_maria_visit_survivor.title", "A Warm Meal", "Um Prato Quente"),
            new LocalizationEntry("quest.daily_maria_visit_survivor.description",
                "The village survivor still eats alone. Maria brings a hot meal and some company.",
                "O sobrevivente da vila ainda come sozinho. A Maria leva um prato quente e um pouco de companhia."),
            new LocalizationEntry("quest.daily_maria_visit_survivor.obj_bring_meal", "Bring cooked meat", "Leve carne assada"),
            new LocalizationEntry("quest.daily_maria_visit_survivor.obj_visit_survivor", "Visit the village survivor", "Visite o sobrevivente da vila"),
            new LocalizationEntry("quest.daily_maria_visit_survivor.journal",
                "He asked about my little brother again. I told him Joaquim is fine, and taller than me now. I hate that.",
                "Ele perguntou do meu irmão de novo. Contei que o Joaquim está bem, e mais alto que eu agora. Odeio isso."),
        };

        /// <summary>Merges <see cref="All"/> into <paramref name="localization"/>, skipping keys it already has (external tables take precedence). Safe to call repeatedly.</summary>
        public static void EnsureLoaded(LocalizationManager localization)
        {
            if (localization == null)
            {
                return;
            }

            localization.LoadEntries(All.Where(entry => !localization.HasKey(entry.Key)));
        }
    }
}
