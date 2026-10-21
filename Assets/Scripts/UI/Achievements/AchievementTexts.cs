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
            Title("ach_first_blood", "First Blood", "Primeiro Sangue"),
            Description("ach_first_blood", "Defeat your first enemy.", "Derrote seu primeiro inimigo."),
            Title("ach_monster_hunter", "Backlands Hunter", "Caçador do Sertão"),
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
            Description("ach_explorer", "Unlock 3 regions on the world map.", "Desbloqueie 3 regiões no mapa-múndi."),
            Title("ach_merchant", "Shrewd Trader", "Bom de Negócio"),
            Description("ach_merchant", "Earn 1000 gold selling to merchants.", "Ganhe 1000 de ouro vendendo aos mercadores."),

            // The five heroes' personal quests.
            Title("ach_hero_maithe", "Eye of the Wind", "Olho do Vento"),
            Description("ach_hero_maithe", "Finish Maithe's personal quest — the eldest of the cousins always finds the trail.", "Conclua a missão pessoal da Maithe — a mais velha dos primos sempre acha a trilha."),
            Title("ach_hero_maria", "Light That Won't Fade", "Luz que Não se Apaga"),
            Description("ach_hero_maria", "Finish Maria's personal quest and bring the crystals back to life.", "Conclua a missão pessoal da Maria e devolva a vida aos cristais."),
            Title("ach_hero_joaquim", "Echoes of Home", "Ecos de Casa"),
            Description("ach_hero_joaquim", "Finish Joaquim's personal quest — and still be home before the lanterns are lit.", "Conclua a missão pessoal do Joaquim — e ainda volte antes de acenderem as lanternas."),
            Title("ach_hero_icaro", "Ancient Gears", "Engrenagens Antigas"),
            Description("ach_hero_icaro", "Finish Ícaro's personal quest and decipher the guardian's inscriptions.", "Conclua a missão pessoal do Ícaro e decifre as inscrições do guardião."),
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
