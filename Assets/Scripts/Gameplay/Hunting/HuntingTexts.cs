using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.Hunting
{
    /// <summary>
    /// Localization keys (en + pt-BR) of the hunting features: Seu Zé Caçador's board (tasks, "Presa do dia", points),
    /// the quest-tracker line, the "Bicho destaque do dia" notices and the Caderno's "Amuletos" tab
    /// (<c>ui.hunting.*</c>), plus a name and an effect line per charm (<see cref="HuntingCharmSpec.NameKey"/>).
    /// Seu Zé greets each cousin his own way (Maithe the eldest, sharp-eyed archer; Maria the healer who keeps an eye on
    /// her brother Joaquim; Joaquim the stubborn shield; Ícaro the inventor, Maithe's little brother; Lavine the
    /// youngest cousin). Same self-contained pattern as <c>NightTexts</c>: merged into the live
    /// <see cref="LocalizationManager"/> on first use (keys it already has win), pt-BR fallback otherwise.
    /// </summary>
    public static class HuntingTexts
    {
        public const string Title = "ui.hunting.title";
        public const string Button = "ui.hunting.button";
        public const string Close = "ui.hunting.close";
        public const string GreetingNone = "ui.hunting.greeting.none";
        public const string SectionTask = "ui.hunting.section.task";
        public const string SectionPrey = "ui.hunting.section.prey";
        public const string SectionPoints = "ui.hunting.section.points";

        public const string TaskNone = "ui.hunting.task.none";
        /// <summary>{0} creature, {1} kills so far, {2} kills needed.</summary>
        public const string TaskActive = "ui.hunting.task.active";
        /// <summary>{0} tier name, {1} kills.</summary>
        public const string TaskTierButton = "ui.hunting.task.tier_button";
        public const string TierSmall = "ui.hunting.task.tier.small";
        public const string TierMedium = "ui.hunting.task.tier.medium";
        public const string TierLarge = "ui.hunting.task.tier.large";
        /// <summary>{0} XP, {1} gold, {2} hunt points.</summary>
        public const string TaskReward = "ui.hunting.task.reward";
        public const string TaskCancel = "ui.hunting.task.cancel";
        /// <summary>{0} tasks per day.</summary>
        public const string TaskCap = "ui.hunting.task.cap";
        /// <summary>{0} XP, {1} gold, {2} hunt points.</summary>
        public const string TaskCompleted = "ui.hunting.task.completed";
        public const string TrackerTitle = "ui.hunting.tracker.title";
        /// <summary>{0} creature, {1} kills so far, {2} kills needed.</summary>
        public const string TrackerLine = "ui.hunting.tracker.line";

        /// <summary>{0} percent.</summary>
        public const string PreyBonusExperience = "ui.hunting.prey.bonus.experience";
        public const string PreyBonusLoot = "ui.hunting.prey.bonus.loot";
        public const string PreyBonusProtection = "ui.hunting.prey.bonus.protection";
        public const string PreyPick = "ui.hunting.prey.pick";
        public const string PreyPicked = "ui.hunting.prey.picked";
        /// <summary>{0} creature, {1} bonus.</summary>
        public const string PreyActive = "ui.hunting.prey.active";
        public const string PreyNone = "ui.hunting.prey.none";
        public const string PreyEmpty = "ui.hunting.prey.empty";
        /// <summary>{0} gold.</summary>
        public const string PreyReroll = "ui.hunting.prey.reroll";
        public const string PreyRerollUsed = "ui.hunting.prey.reroll_used";
        /// <summary>{0} gold.</summary>
        public const string PreyNoGold = "ui.hunting.prey.no_gold";
        public const string BossesExcluded = "ui.hunting.bosses_excluded";

        /// <summary>{0} points.</summary>
        public const string HuntPoints = "ui.hunting.points.hunt";
        /// <summary>{0} hunt points per Caderno point.</summary>
        public const string TradePoints = "ui.hunting.points.trade";
        /// <summary>{0} rank name.</summary>
        public const string RankLine = "ui.hunting.rank_line";

        /// <summary>{0} creature, {1} percent.</summary>
        public const string BoostedHud = "ui.hunting.boosted.hud";
        /// <summary>{0} creature, {1} percent.</summary>
        public const string BoostedCaderno = "ui.hunting.boosted.caderno";
        public const string DodgeFloat = "ui.hunting.dodge_float";

        public const string TabPages = "ui.hunting.charms.tab_pages";
        public const string TabCharms = "ui.hunting.charms.tab_charms";
        /// <summary>{0} points available.</summary>
        public const string CharmPoints = "ui.hunting.charms.points";
        /// <summary>{0} worn, {1} max.</summary>
        public const string CharmEquipped = "ui.hunting.charms.equipped";
        public const string CharmIntro = "ui.hunting.charms.intro";
        /// <summary>{0} cost.</summary>
        public const string CharmBuy = "ui.hunting.charms.buy";
        public const string CharmEquip = "ui.hunting.charms.equip";
        public const string CharmUnequip = "ui.hunting.charms.unequip";
        public const string CharmFull = "ui.hunting.charms.full";
        /// <summary>{0} cost.</summary>
        public const string CharmLocked = "ui.hunting.charms.locked";

        public static string GreetingKey(CharacterId hero) => "ui.hunting.greeting." + hero.ToString().ToLowerInvariant();

        public static string TierKey(HuntingTaskTier tier)
        {
            switch (tier)
            {
                case HuntingTaskTier.Medium: return TierMedium;
                case HuntingTaskTier.Large: return TierLarge;
                default: return TierSmall;
            }
        }

        public static string PreyBonusKey(PreyBonusKind kind)
        {
            switch (kind)
            {
                case PreyBonusKind.Loot: return PreyBonusLoot;
                case PreyBonusKind.Protection: return PreyBonusProtection;
                default: return PreyBonusExperience;
            }
        }

        public static string RankKey(int rank) => "ui.hunting.rank." + Mathf_Clamp(rank, 0, HuntingRules.RankThresholds.Length - 1).ToString(CultureInfo.InvariantCulture);

        private static int Mathf_Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;

        public static readonly List<LocalizationEntry> Entries = BuildEntries();

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        private static List<LocalizationEntry> BuildEntries()
        {
            var entries = new List<LocalizationEntry>
            {
                new LocalizationEntry(Title, "Zé the Hunter's stall", "Barraca do Seu Zé Caçador"),
                new LocalizationEntry(Button, "Zé the Hunter", "Seu Zé Caçador"),
                new LocalizationEntry(Close, "Close", "Fechar"),
                new LocalizationEntry(GreetingNone, "\"Hunting tasks, prey of the day... pick and go, kid!\"", "\"Tarefa de caça, presa do dia... escolhe e vai, menino!\""),
                new LocalizationEntry(GreetingKey(CharacterId.Maithe), "\"Maithe! Eyes like a hawk, just like your grandpa. Pick a task; your aim does the rest.\"", "\"Maithe! Olho de gavião, igualzinha ao seu avô. Escolhe a tarefa que a pontaria faz o resto.\""),
                new LocalizationEntry(GreetingKey(CharacterId.Maria), "\"Maria, you tend to everyone... and who tends to Joaquim? Take a task and keep him close.\"", "\"Maria, você cuida de todo mundo... e quem cuida do Joaquim? Pega uma tarefa e leva ele junto.\""),
                new LocalizationEntry(GreetingKey(CharacterId.Joaquim), "\"Joaquim, stubborn as a mule! Good. A big task needs a hard head.\"", "\"Joaquim, teimoso que só mula! Ótimo. Tarefa grande pede cabeça dura.\""),
                new LocalizationEntry(GreetingKey(CharacterId.Icaro), "\"Ícaro, no contraption hunts by itself. But go on, show Maithe what you can do.\"", "\"Ícaro, engenhoca nenhuma caça sozinha. Mas vai, mostra pra Maithe do que você é capaz.\""),
                new LocalizationEntry(GreetingKey(CharacterId.Lavine), "\"Lavine! Small, but no one spots a track faster. Start with a little task, then go big.\"", "\"Lavine! Pequena, mas ninguém acha rastro mais rápido. Começa com uma tarefa pequena e depois vai na grande.\""),
                new LocalizationEntry(SectionTask, "Hunting task", "Tarefa de caça"),
                new LocalizationEntry(SectionPrey, "Prey of the day", "Presa do dia"),
                new LocalizationEntry(SectionPoints, "Points", "Pontos"),
                new LocalizationEntry(TaskNone, "Pick a creature and the size of the task.", "Escolha um bicho e o tamanho da tarefa."),
                new LocalizationEntry(TaskActive, "Defeat {0}: {1}/{2}", "Derrote {0}: {1}/{2}"),
                new LocalizationEntry(TaskTierButton, "{0} ({1})", "{0} ({1})"),
                new LocalizationEntry(TierSmall, "Small", "Pequena"),
                new LocalizationEntry(TierMedium, "Medium", "Média"),
                new LocalizationEntry(TierLarge, "Large", "Grande"),
                new LocalizationEntry(TaskReward, "Reward: {0} XP, {1} gold, {2} Hunt Points", "Recompensa: {0} de XP, {1} de ouro, {2} Pontos de Caça"),
                new LocalizationEntry(TaskCancel, "Give up the task", "Desistir da tarefa"),
                new LocalizationEntry(TaskCap, "That's enough for today: Zé gives out {0} tasks a day.", "Chega por hoje: o Seu Zé só dá {0} tarefas por dia."),
                new LocalizationEntry(TaskCompleted, "Hunting task done! +{0} XP, +{1} gold, +{2} Hunt Points", "Tarefa de caça concluída! +{0} de XP, +{1} de ouro, +{2} Pontos de Caça"),
                new LocalizationEntry(TrackerTitle, "Hunting task", "Tarefa de caça"),
                new LocalizationEntry(TrackerLine, "{0}: {1}/{2}", "{0}: {1}/{2}"),
                new LocalizationEntry(PreyBonusExperience, "+{0}% XP", "+{0}% de XP"),
                new LocalizationEntry(PreyBonusLoot, "+{0}% loot chance", "+{0}% de chance de saque"),
                new LocalizationEntry(PreyBonusProtection, "-{0}% damage taken", "-{0}% de dano recebido"),
                new LocalizationEntry(PreyPick, "Pick", "Escolher"),
                new LocalizationEntry(PreyPicked, "Picked", "Escolhida"),
                new LocalizationEntry(PreyActive, "Today's prey: {0} ({1}) until tomorrow.", "Presa de hoje: {0} ({1}) até amanhã."),
                new LocalizationEntry(PreyNone, "Pick one prey: the bonus lasts until tomorrow.", "Escolha uma presa: o bônus vale até amanhã."),
                new LocalizationEntry(PreyEmpty, "No prey today: beat a creature first.", "Sem presa hoje: vença um bicho primeiro."),
                new LocalizationEntry(PreyReroll, "Change the prey ({0} gold)", "Trocar as presas ({0} de ouro)"),
                new LocalizationEntry(PreyRerollUsed, "You already changed the prey today.", "Você já trocou as presas hoje."),
                new LocalizationEntry(PreyNoGold, "Not enough gold to change ({0}).", "Falta ouro para trocar ({0})."),
                new LocalizationEntry(BossesExcluded, "Bosses are another story: tasks, prey and charms never count against them.", "Chefe é outra conversa: tarefa, presa e amuleto não valem contra eles."),
                new LocalizationEntry(HuntPoints, "Hunt Points: {0}", "Pontos de Caça: {0}"),
                new LocalizationEntry(TradePoints, "Trade {0} Hunt Points for 1 Notebook Point", "Trocar {0} Pontos de Caça por 1 Ponto do Caderno"),
                new LocalizationEntry(RankLine, "Zé calls you: {0}.", "O Seu Zé te chama de: {0}."),
                new LocalizationEntry(RankKey(0), "Hunter's apprentice", "Aprendiz de caçador"),
                new LocalizationEntry(RankKey(1), "Woodsman", "Mateiro"),
                new LocalizationEntry(RankKey(2), "Tracker", "Rastreador"),
                new LocalizationEntry(RankKey(3), "Hunter of the Sertão", "Caçador do Sertão"),
                new LocalizationEntry(BoostedHud, "Highlight of the day: {0} (+{1}% XP and loot)", "Destaque do dia: {0} (+{1}% de XP e saque)"),
                new LocalizationEntry(BoostedCaderno, "Today's highlighted creature: {0}. +{1}% XP and loot.", "Bicho destaque de hoje: {0}. +{1}% de XP e saque."),
                new LocalizationEntry(DodgeFloat, "Dodged!", "Desviou!"),
                new LocalizationEntry(TabPages, "Pages", "Páginas"),
                new LocalizationEntry(TabCharms, "Charms", "Amuletos"),
                new LocalizationEntry(CharmPoints, "Notebook Points: {0}", "Pontos do Caderno: {0}"),
                new LocalizationEntry(CharmEquipped, "On the cord: {0}/{1}", "No cordão: {0}/{1}"),
                new LocalizationEntry(CharmIntro, "Each page earns Notebook Points at 10, 50 and 200 creatures defeated. Grandma's cord holds two charms. They never work against bosses.", "Cada página dá Pontos do Caderno com 10, 50 e 200 bichos vencidos. O cordão da Vó aguenta dois amuletos. Eles não valem contra chefes."),
                new LocalizationEntry(CharmBuy, "Get ({0})", "Comprar ({0})"),
                new LocalizationEntry(CharmEquip, "Wear", "Usar"),
                new LocalizationEntry(CharmUnequip, "Take off", "Tirar"),
                new LocalizationEntry(CharmFull, "Cord full", "Cordão cheio"),
                new LocalizationEntry(CharmLocked, "Needs {0} points", "Precisa de {0} pontos"),
            };

            foreach (HuntingCharmSpec charm in HuntingCharmCatalog.All)
            {
                (string nameEn, string namePt, string effectEn, string effectPt) = CharmText(charm.Id);
                entries.Add(new LocalizationEntry(charm.NameKey, nameEn, namePt));
                entries.Add(new LocalizationEntry(charm.EffectKey, effectEn, effectPt));
            }

            return entries;
        }

        private static (string, string, string, string) CharmText(string id)
        {
            switch (id)
            {
                case HuntingCharmCatalog.DenteDeOnca:
                    return ("Jaguar Tooth", "Dente de Onça", "8% less damage from four-legged creatures (wolves).", "8% menos dano de bichos de quatro patas (lobos).");
                case HuntingCharmCatalog.CascaDeBesouro:
                    return ("Beetle Shell", "Casca de Besouro", "8% less damage from many-legged creatures (spiders, scorpions).", "8% menos dano de bichos de muitas patas (aranhas, escorpiões).");
                case HuntingCharmCatalog.PenaDeCarcara:
                    return ("Caracara Feather", "Pena de Carcará", "8% less damage from flying creatures (bats).", "8% menos dano de bichos que voam (morcegos).");
                case HuntingCharmCatalog.PedraDeCorisco:
                    return ("Thunderstone", "Pedra de Corisco", "8% less damage from heavy creatures (cave dwellers).", "8% menos dano de bichos pesados (habitantes das cavernas).");
                case HuntingCharmCatalog.FigaDeGuine:
                    return ("Guinea Figa", "Figa de Guiné", "3% chance to dodge a creature's hit.", "3% de chance de desviar do golpe de um bicho.");
                case HuntingCharmCatalog.RaminhoDeArruda:
                    return ("Rue Sprig", "Raminho de Arruda", "Heals 2% of your health for each creature defeated.", "Cura 2% da vida a cada bicho vencido.");
                default:
                    return ("Cowboy Knot", "Nó de Vaqueiro", "Your stuns on creatures last 10% longer.", "Seus atordoamentos nos bichos duram 10% a mais.");
            }
        }

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Title)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        /// <summary>The live manager, or null outside a game session.</summary>
        public static LocalizationManager Live => ServiceLocator.TryGet(out LocalizationManager manager) ? manager : null;

        /// <summary>Localized text via <paramref name="manager"/> when available, else the pt-BR fallback, else the key (pass <see cref="Live"/> at runtime).</summary>
        public static string Get(LocalizationManager manager, string key)
        {
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
            string format = Get(manager, key);
            try
            {
                return string.Format(CultureInfo.InvariantCulture, format, args);
            }
            catch (System.FormatException)
            {
                return format;
            }
        }

        /// <summary>A 0..1 fraction as a whole percent ("25").</summary>
        public static string Percent(float fraction) => ((int)System.Math.Round(fraction * 100f)).ToString(CultureInfo.InvariantCulture);

        public static string GetFallback(string key)
        {
            if (_fallbackByKey == null)
            {
                _fallbackByKey = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    _fallbackByKey[entry.Key] = entry.PtBR;
                }
            }

            return key != null && _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }
    }
}
