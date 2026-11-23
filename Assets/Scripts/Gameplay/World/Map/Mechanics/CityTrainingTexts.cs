using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the services with a screen of their own (<see cref="MesektetServiceRules"/>): Djedi's circles of light and
    /// Setne's shelf of the Caderno (Cais de Mesektet), plus the cheaper repair of the Forja de Tambor (Tambor Rubro,
    /// <c>RepairDiscountRules</c>). Same self-contained pattern as <see cref="CityServiceTexts"/>.
    /// </summary>
    public static class CityTrainingTexts
    {
        private const string P = "ui.city_training.";

        public const string Close = P + "close";
        public const string BagFull = P + "bag_full";

        public const string LessonPrompt = P + "lesson.prompt";
        public const string LessonTitle = P + "lesson.title";
        public const string LessonIntro = P + "lesson.intro";
        /// <summary>{0} gold.</summary>
        public const string LessonButtonGold = P + "lesson.button_gold";
        public const string LessonButtonFree = P + "lesson.button_free";
        /// <summary>{0} lesson, {1} minutes.</summary>
        public const string LessonActive = P + "lesson.active";
        public const string LessonNone = P + "lesson.none";
        /// <summary>{0} lesson.</summary>
        public const string LessonDone = P + "lesson.done";
        public const string LessonCombat = P + "lesson.combat";
        /// <summary>{0} gold.</summary>
        public const string LessonGoldMissing = P + "lesson.gold_missing";
        public const string LessonEnd = P + "lesson.end";

        public const string ShelfPrompt = P + "shelf.prompt";
        public const string ShelfTitle = P + "shelf.title";
        public const string ShelfIntro = P + "shelf.intro";
        /// <summary>{0} pages filled, {1} pages in the Caderno.</summary>
        public const string ShelfPages = P + "shelf.pages";
        /// <summary>{0} pages.</summary>
        public const string ShelfRow = P + "shelf.row";
        public const string ShelfClaimButton = P + "shelf.claim";
        public const string ShelfClaimed = P + "shelf.claimed";
        /// <summary>{0} pages to go.</summary>
        public const string ShelfMissing = P + "shelf.missing";
        public const string ShelfWaitTurn = P + "shelf.wait_turn";
        /// <summary>{0} the reward list.</summary>
        public const string ShelfReceived = P + "shelf.received";
        public const string ShelfNothing = P + "shelf.nothing";
        /// <summary>{0} amount of gold.</summary>
        public const string RewardGold = P + "reward.gold";
        /// <summary>{0} item name, {1} amount.</summary>
        public const string RewardItem = P + "reward.item";

        /// <summary>{0} pieces, {1} gold, {2} percent.</summary>
        public const string ShopRepairRow = P + "repair.shop_row";
        public const string ShopRepairNothing = P + "repair.shop_nothing";
        /// <summary>{0} percent.</summary>
        public const string RepairDiscount = P + "repair.discount";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static string LessonName(DjediLesson lesson) => P + "lesson.name." + (int)lesson;

        public static string LessonEffect(DjediLesson lesson) => P + "lesson.effect." + (int)lesson;

        public static string LessonLine(CharacterId hero) => P + "lesson.line." + Hero(hero);

        public static string ShelfLine(CharacterId hero) => P + "shelf.line." + Hero(hero);

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= Build();

        private static List<LocalizationEntry> Build()
        {
            var e = new List<LocalizationEntry>
            {
                new LocalizationEntry(Close, "Close", "Fechar"),
                new LocalizationEntry(BagFull, "The backpack is full: make room and come back", "A mochila está cheia: abram espaço e voltem"),

                // ---- Djedi's circles of light (Cais de Mesektet, East street) ----
                new LocalizationEntry(LessonPrompt, "[E] Djedi's circle of light: lessons of magic", "[E] Círculo de luz do Djedi: lições de magia"),
                new LocalizationEntry(LessonTitle, "Djedi's circles of light", "Círculos de luz do Djedi"),
                new LocalizationEntry(LessonIntro,
                    "Djedi: \"Sit inside the circle and breathe slowly. One circle at a time: a new one puts the old one out. The first lesson of the day is on me; I have a hundred and ten years and time to spare.\"",
                    "Djedi: \"Sentem dentro do círculo e respirem devagar. Um círculo de cada vez: o novo apaga o velho. A primeira lição do dia é por minha conta; tenho cento e dez anos e tempo de sobra.\""),
                new LocalizationEntry(LessonButtonGold, "Learn ({0} gold)", "Aprender ({0} de ouro)"),
                new LocalizationEntry(LessonButtonFree, "Learn (free today)", "Aprender (grátis hoje)"),
                new LocalizationEntry(LessonActive, "Lit now: {0} ({1} min left)", "Aceso agora: {0} (faltam {1} min)"),
                new LocalizationEntry(LessonNone, "No circle lit around you", "Nenhum círculo aceso em volta de vocês"),
                new LocalizationEntry(LessonDone, "{0}: the circle lights up around you", "{0}: o círculo acende em volta de vocês"),
                new LocalizationEntry(LessonCombat, "Not in the middle of a fight: Djedi teaches sitting down", "No meio da briga não: o Djedi ensina sentado"),
                new LocalizationEntry(LessonGoldMissing, "Not enough gold: the lesson costs {0}", "Falta ouro: a lição custa {0}"),
                new LocalizationEntry(LessonEnd, "Djedi's circle of light went out", "O círculo de luz do Djedi apagou"),
                new LocalizationEntry(LessonName(DjediLesson.Lamp), "Circle of the Lamp", "Círculo da Lamparina"),
                new LocalizationEntry(LessonName(DjediLesson.Hours), "Circle of the Hours", "Círculo das Horas"),
                new LocalizationEntry(LessonName(DjediLesson.Scribe), "Circle of the Scribe", "Círculo do Escriba"),
                new LocalizationEntry(LessonEffect(DjediLesson.Lamp), "+20% mana regeneration for 15 min", "+20% de recuperação de mana por 15 min"),
                new LocalizationEntry(LessonEffect(DjediLesson.Hours), "Skills come back 5% sooner for 15 min", "Habilidades voltam 5% mais rápido por 15 min"),
                new LocalizationEntry(LessonEffect(DjediLesson.Scribe), "Skills cost 8% less mana for 15 min", "Habilidades gastam 8% menos mana por 15 min"),
                new LocalizationEntry(LessonLine(CharacterId.Joaquim), "Djedi: \"Slower, Joaquim. The light is not a shield you swing; it is one you hold.\"",
                    "Djedi: \"Mais devagar, Joaquim. A luz não é escudo que se bate; é escudo que se segura.\""),
                new LocalizationEntry(LessonLine(CharacterId.Maria), "Djedi: \"You light it like your grandma did, Maria. First try, and without looking.\"",
                    "Djedi: \"Você acende igual à sua avó, Maria. De primeira, e sem olhar.\""),
                new LocalizationEntry(LessonLine(CharacterId.Maithe), "Djedi: \"The eldest counts the breaths for the others. One, two... good. Now you sit too.\"",
                    "Djedi: \"A mais velha conta as respirações para os outros. Um, dois... isso. Agora senta você também.\""),
                new LocalizationEntry(LessonLine(CharacterId.Icaro), "Djedi: \"No, Ícaro, there is no gear inside the circle. I checked. Twice.\"",
                    "Djedi: \"Não, Ícaro, não tem engrenagem dentro do círculo. Eu conferi. Duas vezes.\""),
                new LocalizationEntry(LessonLine(CharacterId.Lavine), "Djedi: \"Blue again! A hundred and ten years, and every day you show me something new, Lavine.\"",
                    "Djedi: \"Azul de novo! Cento e dez anos, e todo dia você me mostra uma coisa nova, Lavine.\""),

                // ---- Setne's shelf of the Caderno (Cais de Mesektet, South street) ----
                new LocalizationEntry(ShelfPrompt, "[E] Setne's shelf: the pages of the Caderno", "[E] Estante do Setne: as páginas do Caderno"),
                new LocalizationEntry(ShelfTitle, "Setne's shelf: the Caderno", "Estante do Setne: o Caderno"),
                new LocalizationEntry(ShelfIntro,
                    "Setne: \"Every page you fill in your grandma's Caderno, I copy onto papyrus for the shelf. Every few pages, the shelf pays you back. Copies, not the book: the book stays with you.\"",
                    "Setne: \"Cada página que vocês preenchem no Caderno da avó, eu copio num papiro para a estante. A cada tantas páginas, a estante paga de volta. Cópias, não o livro: o livro fica com vocês.\""),
                new LocalizationEntry(ShelfPages, "Pages filled: {0} of {1}", "Páginas preenchidas: {0} de {1}"),
                new LocalizationEntry(ShelfRow, "{0} pages", "{0} páginas"),
                new LocalizationEntry(ShelfClaimButton, "Receive", "Receber"),
                new LocalizationEntry(ShelfClaimed, "Received", "Recebido"),
                new LocalizationEntry(ShelfMissing, "{0} pages to go", "Faltam {0} páginas"),
                new LocalizationEntry(ShelfWaitTurn, "Receive the shelf below first", "Recebam a prateleira de baixo antes"),
                new LocalizationEntry(ShelfReceived, "Setne hands you: {0}", "O Setne entrega: {0}"),
                new LocalizationEntry(ShelfNothing, "Nothing new on the shelf: fill more pages of the Caderno", "Nada novo na estante: preencham mais páginas do Caderno"),
                new LocalizationEntry(RewardGold, "{0} gold", "{0} de ouro"),
                new LocalizationEntry(RewardItem, "{0} ×{1}", "{0} ×{1}"),
                new LocalizationEntry(ShelfLine(CharacterId.Joaquim), "Setne: \"You draw the beasts bigger than they are, Joaquim. I copy them that way. It's a better book.\"",
                    "Setne: \"Você desenha os bichos maiores do que são, Joaquim. Eu copio assim mesmo. Fica um livro melhor.\""),
                new LocalizationEntry(ShelfLine(CharacterId.Maria), "Setne: \"Your notes are neat, Maria. Too neat: write something silly in the margin, like your grandpa.\"",
                    "Setne: \"Suas anotações são certinhas, Maria. Certinhas demais: escreve uma bobagem na margem, igual ao seu avô.\""),
                new LocalizationEntry(ShelfLine(CharacterId.Maithe), "Setne: \"The eldest keeps the count. I keep the copies. Between us, nothing gets lost.\"",
                    "Setne: \"A mais velha guarda a conta. Eu guardo as cópias. Entre nós dois, nada se perde.\""),
                new LocalizationEntry(ShelfLine(CharacterId.Icaro), "Setne: \"Yes, Ícaro, you can read the scroll of how the barque floats. After you return the last one.\"",
                    "Setne: \"Sim, Ícaro, pode ler o rolo de como a barca boia. Depois de devolver o último.\""),
                new LocalizationEntry(ShelfLine(CharacterId.Lavine), "Setne: \"Grandpa Jorge wrote a rice recipe on the back of my oldest papyrus. I keep it on the top shelf.\"",
                    "Setne: \"O Vô Jorge escreveu uma receita de arroz no verso do meu papiro mais antigo. Eu guardo na prateleira de cima.\""),

                // ---- The Forja de Tambor's cheaper repair (Tambor Rubro) ----
                new LocalizationEntry(ShopRepairRow, "Repair all: {0} pieces ({1} gold, {2}% cheaper here)", "Consertar tudo: {0} peças ({1} de ouro, {2}% mais barato aqui)"),
                new LocalizationEntry(ShopRepairNothing, "Repair: everything is in good shape", "Conserto: tudo inteirinho"),
                new LocalizationEntry(RepairDiscount, " · {0}% cheaper at this forge", " · {0}% mais barato nesta forja"),
            };
            return e;
        }

        /// <summary>The ferreiro window's "repair all" suffix for a forge discount (empty without one).</summary>
        public static string RepairDiscountSuffix(LocalizationManager manager, float discount)
        {
            int percent = global::Rapadura.Gameplay.Items.Durability.RepairDiscountRules.Percent(discount);
            return percent > 0 ? Format(manager, RepairDiscount, percent) : string.Empty;
        }

        public static string Format(LocalizationManager manager, string key, params object[] args) => string.Format(Get(manager, key), args);

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

        private static string Hero(CharacterId hero) => hero.ToString().ToLowerInvariant();
    }
}
