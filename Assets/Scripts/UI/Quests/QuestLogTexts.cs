using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using UnityEngine;

namespace Rapadura.UI.Quests
{
    /// <summary>
    /// Localization entries owned by the Quest Log/Tracker: UI chrome (<c>ui.quest.*</c>) plus one
    /// short "Diário" journal note per quest (<c>quest.*.journal</c>, key derived by
    /// <see cref="QuestLogPresenter.GetJournalKey"/>). Kept in the UI layer instead of
    /// <c>LocalizationManager.DefaultEntries</c> so this screen doesn't need to edit that shared file;
    /// merged at runtime by <see cref="EnsureLoaded"/>, which only adds keys that aren't already
    /// registered — a CSV/LocalizationTable that defines the same keys always wins.
    ///
    /// Journal notes are written for the post-2026-08-24 roster (Maithe 14, Maria 13, Joaquim 12,
    /// Ícaro 11, Lavine 9) and lean on the canonical family tree from Docs/GDD.md: Maithe and Ícaro
    /// are siblings, Joaquim and Maria are siblings, Lavine is everyone's younger cousin. The quest
    /// descriptions themselves (seeded by Character/ConvergenceQuestSeeder) are left untouched.
    /// </summary>
    public static class QuestLogTexts
    {
        public const string TitleKey = "ui.quest.title";
        public const string TabActiveKey = "ui.quest.tab_active";
        public const string TabCompletedKey = "ui.quest.tab_completed";
        public const string CloseKey = "ui.quest.close";
        public const string ObjectivesKey = "ui.quest.objectives";
        public const string RewardsKey = "ui.quest.rewards";
        public const string JournalKey = "ui.quest.journal";
        public const string TrackKey = "ui.quest.track";
        public const string TrackingKey = "ui.quest.tracking";
        public const string EmptyActiveKey = "ui.quest.empty_active";
        public const string EmptyCompletedKey = "ui.quest.empty_completed";
        public const string SelectHintKey = "ui.quest.select_hint";
        public const string CompletedBadgeKey = "ui.quest.completed_badge";
        public const string QuestCompletedToastKey = "ui.quest.toast_completed";
        public const string OpenLogHintKey = "ui.quest.open_log_hint";

        public static readonly List<LocalizationEntry> All = new List<LocalizationEntry>
        {
            new LocalizationEntry(TitleKey, "Quests", "Missões"),
            new LocalizationEntry(TabActiveKey, "Active", "Ativas"),
            new LocalizationEntry(TabCompletedKey, "Completed", "Concluídas"),
            new LocalizationEntry(CloseKey, "Close", "Fechar"),
            new LocalizationEntry(ObjectivesKey, "Objectives", "Objetivos"),
            new LocalizationEntry(RewardsKey, "Rewards", "Recompensas"),
            new LocalizationEntry(JournalKey, "Journal", "Diário"),
            new LocalizationEntry(TrackKey, "Track", "Acompanhar"),
            new LocalizationEntry(TrackingKey, "Tracking", "Acompanhando"),
            new LocalizationEntry(EmptyActiveKey, "No quests in progress. Talk to people around Rapadura — someone always needs a hand.", "Nenhuma missão em andamento. Converse com o povo de Rapadura — sempre tem alguém precisando de ajuda."),
            new LocalizationEntry(EmptyCompletedKey, "No quests completed yet.", "Nenhuma missão concluída ainda."),
            new LocalizationEntry(SelectHintKey, "Select a quest", "Selecione uma missão"),
            new LocalizationEntry(CompletedBadgeKey, "Completed", "Concluída"),
            new LocalizationEntry(QuestCompletedToastKey, "Quest complete!", "Missão concluída!"),
            new LocalizationEntry(OpenLogHintKey, "Tap to open quests", "Toque para abrir as missões"),
            new LocalizationEntry(QuestLogPresenter.TypeMainKey, "Main Story", "Principal"),
            new LocalizationEntry(QuestLogPresenter.TypeSideKey, "Side", "Secundária"),
            new LocalizationEntry(QuestLogPresenter.TypeDailyKey, "Daily", "Diária"),
            new LocalizationEntry(QuestLogPresenter.RewardXpKey, "{0} XP", "{0} XP"),

            // Journal notes — one per seeded quest, in the voice of the hero (or the family).
            new LocalizationEntry("quest.joaquim_echoes.journal",
                "Maria made me promise to be home before the lanterns are lit. She never lets me forget she was born first. But this was our village too — if anyone is still out there, I'm the one with the shield. Maithe found the tracks; I'll do the rest.",
                "A Maria me fez prometer voltar antes de acenderem as lanternas. Ela nunca me deixa esquecer que nasceu primeiro. Mas essa vila era nossa também — se ainda tem alguém lá fora, quem carrega o escudo sou eu. A Maithe achou as pegadas; o resto é comigo."),
            new LocalizationEntry("quest.maithe_windtrail.journal",
                "Ícaro begged to come see the golems. I said no — somebody has to keep the workshop from catching fire. I'll bring my little brother a piece of the marked ore instead. He'll take it apart before dinner, and maybe he'll see something the Olho do Vento can't.",
                "O Ícaro implorou pra vir ver os golens. Eu disse que não — alguém precisa impedir a oficina de pegar fogo. Vou levar pro meu irmãozinho um pedaço do minério marcado. Ele vai desmontar antes do jantar, e talvez enxergue algo que nem o Olho do Vento enxerga."),
            new LocalizationEntry("quest.lavine_redtide.journal",
                "Everyone treats me like the baby of the family. Maithe walks slower so I can keep up, and Joaquim stands in front of me without even thinking. But the red whispers only talk to me. This time I have to be brave on my own — and show my cousins that my power can protect them too.",
                "Todo mundo me trata como a caçula da família. A Maithe anda mais devagar pra eu acompanhar, e o Joaquim fica na minha frente sem nem pensar. Mas os sussurros vermelhos só falam comigo. Dessa vez eu preciso ser corajosa sozinha — e mostrar pros meus primos que o meu poder também pode protegê-los."),
            new LocalizationEntry("quest.maria_fading_light.journal",
                "In my vision I saw the five of us standing together: Maithe, Joaquim, Ícaro, little Lavine and me. Joaquim pretends he isn't worried about me going into the caves alone. He is. First I need to understand why the crystals are going dark — then I'll go and bring everyone home.",
                "Na minha visão eu vi nós cinco lado a lado: a Maithe, o Joaquim, o Ícaro, a pequena Lavine e eu. O Joaquim finge que não está preocupado comigo indo sozinha às cavernas. Ele está. Primeiro eu preciso entender por que os cristais estão se apagando — depois vou buscar todo mundo."),
            new LocalizationEntry("quest.icaro_ancient_gears.journal",
                "Maithe says I take apart anything that stands still. This guardian doesn't stand still — that's the problem. If I can read those inscriptions, maybe I'll finally build something that protects my big sister for once, instead of the other way around.",
                "A Maithe diz que eu desmonto qualquer coisa que fica parada. Esse guardião não fica parado — esse é o problema. Se eu conseguir ler aquelas inscrições, talvez eu finalmente construa algo que proteja a minha irmã mais velha, pra variar, em vez do contrário."),
            new LocalizationEntry("quest.convergence.journal",
                "No strangers answered Maria's call — only family. The five cousins grew up racing through the same orchards and sharing the same scoldings. Now Maithe leads, Joaquim guards, Maria heals, Ícaro tinkers and Lavine surprises everyone. Together, or not at all.",
                "Nenhum estranho atendeu ao chamado da Maria — só a família. Os cinco primos cresceram correndo pelos mesmos pomares e levando as mesmas broncas. Agora a Maithe lidera, o Joaquim protege, a Maria cura, o Ícaro inventa e a Lavine surpreende todo mundo. Juntos, ou nada feito."),
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

        /// <summary>Fallback text when no LocalizationManager is registered (e.g. a test scene): PT-BR, the project's default UI language.</summary>
        public static string GetFallback(string key)
        {
            LocalizationEntry entry = All.FirstOrDefault(e => e.Key == key);
            return entry != null ? entry.GetText(LanguageCode.ptBR) : $"[{key}]";
        }
    }
}
