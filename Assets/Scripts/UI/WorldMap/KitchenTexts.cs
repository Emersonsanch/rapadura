using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// Localization keys (en + pt-BR) for "Cozinha da Vó Zefa": the village hub's "Fogão a lenha"
    /// (<see cref="VillageKitchenController"/>), the dishes of <see cref="Rapadura.Gameplay.Cooking.MealCatalog"/>
    /// (<c>meal.&lt;dish&gt;.*</c>), Vó Zefa's lines per cousin (<c>ui.kitchen.grandma.*</c>) and the HUD's meal badge
    /// (<see cref="MealHudBadge"/>). Same self-contained pattern as <see cref="VillageChestTexts"/>: merged into the
    /// live <see cref="LocalizationManager"/> on first use (keys it already has win), pt-BR fallback without one.
    /// </summary>
    public static class KitchenTexts
    {
        public const string HubButton = "ui.kitchen.hub_button";
        public const string Title = "ui.kitchen.title";
        public const string Subtitle = "ui.kitchen.subtitle";
        /// <summary>{0} = dish, {1} = encounters left.</summary>
        public const string ActiveMeal = "ui.kitchen.active_meal";
        /// <summary>{0} = dish.</summary>
        public const string ActiveMealLast = "ui.kitchen.active_meal_last";
        public const string NoMeal = "ui.kitchen.no_meal";
        public const string Cook = "ui.kitchen.cook";
        public const string CookAgain = "ui.kitchen.cook_again";
        public const string Unknown = "ui.kitchen.unknown";
        /// <summary>{0} = quest title.</summary>
        public const string LearnByQuest = "ui.kitchen.learn_by_quest";
        /// <summary>{0} = cousin.</summary>
        public const string LearnByLesson = "ui.kitchen.learn_by_lesson";
        /// <summary>{0} = fish (dishes taught at "Pesca no açude").</summary>
        public const string LearnByCatch = "ui.kitchen.learn_by_catch";
        /// <summary>{0} = item name, {1} = in the bag, {2} = needed.</summary>
        public const string Ingredient = "ui.kitchen.ingredient";
        /// <summary>{0} = gold needed, {1} = gold in the bag.</summary>
        public const string Provisions = "ui.kitchen.provisions";
        /// <summary>{0} = encounters.</summary>
        public const string Lasts = "ui.kitchen.lasts";
        /// <summary>{0} = cousin, {1} = bonus text.</summary>
        public const string FavouriteOf = "ui.kitchen.favourite_of";
        /// <summary>{0} = dish, {1} = encounters.</summary>
        public const string StatusStarted = "ui.kitchen.status_started";
        /// <summary>{0} = dish, {1} = encounters.</summary>
        public const string StatusRefreshed = "ui.kitchen.status_refreshed";
        /// <summary>{0} = new dish, {1} = dish it replaced.</summary>
        public const string StatusReplaced = "ui.kitchen.status_replaced";
        public const string StatusMissing = "ui.kitchen.status_missing";
        public const string StatusProvisions = "ui.kitchen.status_provisions";
        public const string StatusNotLearned = "ui.kitchen.status_not_learned";
        /// <summary>{0} = dish.</summary>
        public const string StatusLearned = "ui.kitchen.status_learned";
        public const string Close = "ui.kitchen.close";
        /// <summary>{0} = dish, {1} = encounters left (HUD badge).</summary>
        public const string HudBadge = "ui.kitchen.hud_badge";
        /// <summary>{0} = dish (HUD badge, last encounter).</summary>
        public const string HudBadgeLast = "ui.kitchen.hud_badge_last";
        /// <summary>Vó Zefa's line when nobody is selected.</summary>
        public const string GrandmaGreetingAny = "ui.kitchen.grandma.greeting_any";

        /// <summary>"ui.kitchen.grandma.greeting_maithe": Vó Zefa's line when <paramref name="hero"/> opens the stove.</summary>
        public static string GreetingKey(CharacterId hero) => "ui.kitchen.grandma.greeting_" + HeroSegment(hero);

        /// <summary>"ui.kitchen.grandma.lesson_maria": Vó Zefa teaching <paramref name="hero"/> their favourite.</summary>
        public static string LessonKey(CharacterId hero) => "ui.kitchen.grandma.lesson_" + HeroSegment(hero);

        /// <summary>
        /// "ui.kitchen.grandma.after_desert_icaro" (or "ui.kitchen.grandma.after_desert" without a cousin): Vó Zefa
        /// remembering the region the cousins just came back from (<c>StoryBeats.LastRegionCleared</c>).
        /// </summary>
        public static string AfterRegionKey(BiomeType region, CharacterId? hero)
        {
            string key = "ui.kitchen.grandma.after_" + region.ToString().ToLowerInvariant();
            return hero.HasValue ? key + "_" + HeroSegment(hero.Value) : key;
        }

        private static string HeroSegment(CharacterId hero) => hero.ToString().ToLowerInvariant();

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(HubButton, "Wood stove", "Fogão a lenha"),
            new LocalizationEntry(Title, "Grandma Zefa's wood stove", "Fogão a lenha da Vó Zefa"),
            new LocalizationEntry(Subtitle, "One meal at a time. It lasts a few fights, not a few hours.", "Uma refeição por vez. Vale por algumas lutas, não por algumas horas."),
            new LocalizationEntry(ActiveMeal, "In your belly: {0} · {1} fights left", "Na barriga: {0} · faltam {1} lutas"),
            new LocalizationEntry(ActiveMealLast, "In your belly: {0} · last fight", "Na barriga: {0} · última luta"),
            new LocalizationEntry(NoMeal, "Empty belly. Grandma does not approve.", "Barriga vazia. A Vó não aprova."),
            new LocalizationEntry(Cook, "Cook", "Cozinhar"),
            new LocalizationEntry(CookAgain, "Cook again (refresh)", "Cozinhar de novo (renova)"),
            new LocalizationEntry(Unknown, "Unknown recipe", "Receita desconhecida"),
            new LocalizationEntry(LearnByQuest, "Learn it: quest \"{0}\"", "Aprenda: missão \"{0}\""),
            new LocalizationEntry(LearnByLesson, "or Grandma teaches {0}", "ou a Vó ensina pra {0}"),
            new LocalizationEntry(LearnByCatch, "Learn it by fishing: {0} (map → Fish)", "Aprenda pescando: {0} (mapa → Pescar)"),
            new LocalizationEntry(Ingredient, "{0} {1}/{2}", "{0} {1}/{2}"),
            new LocalizationEntry(Provisions, "Groceries from the shop: {0} gold (you have {1})", "Mantimentos da venda: {0} de ouro (tem {1})"),
            new LocalizationEntry(Lasts, "Lasts {0} fights", "Dura {0} lutas"),
            new LocalizationEntry(FavouriteOf, "{0}'s favourite: {1}", "Favorito de {0}: {1}"),
            new LocalizationEntry(StatusStarted, "{0} served! Good for the next {1} fights.", "{0} servido! Vale pelas próximas {1} lutas."),
            new LocalizationEntry(StatusRefreshed, "Another {0}: belly full again ({1} fights).", "Mais {0}: barriga cheia de novo ({1} lutas)."),
            new LocalizationEntry(StatusReplaced, "{0} instead of {1}. One meal at a time, child!", "{0} no lugar de {1}. Uma refeição por vez, criança!"),
            new LocalizationEntry(StatusMissing, "Something is missing from the backpack.", "Tá faltando ingrediente na mochila."),
            new LocalizationEntry(StatusProvisions, "Not enough gold for the groceries.", "Não tem ouro pros mantimentos da venda."),
            new LocalizationEntry(StatusNotLearned, "Grandma hasn't taught this one yet.", "Essa a Vó ainda não ensinou."),
            new LocalizationEntry(StatusLearned, "New recipe: {0}!", "Receita nova: {0}!"),
            new LocalizationEntry(Close, "Close", "Fechar"),
            new LocalizationEntry(HudBadge, "{0} · {1}", "{0} · {1}"),
            new LocalizationEntry(HudBadgeLast, "{0} · last", "{0} · última"),

            // -- Vó Zefa at the stove, per cousin (birth order Maithe > Maria > Joaquim > Ícaro > Lavine) --
            new LocalizationEntry(GrandmaGreetingAny, "Sit down, the stove is already hot. Who's hungry?", "Senta, que o fogão já tá quente. Quem tá com fome?"),
            new LocalizationEntry("ui.kitchen.grandma.greeting_maithe", "Maithe, my eldest granddaughter. The coffee is already going through the cloth — strong, the way you like it.", "Maithe, minha neta mais velha. O café já tá passando no pano — forte, do jeito que tu gosta."),
            new LocalizationEntry("ui.kitchen.grandma.greeting_maria", "Maria, light of my eyes. Come help me tie the husks; you have your mother's hands.", "Maria, luz dos meus olhos. Vem me ajudar a amarrar a palha, que tu tem a mão da tua mãe."),
            new LocalizationEntry("ui.kitchen.grandma.greeting_joaquim", "Joaquim, leave that shield at the door. Nobody protects anybody on an empty stomach.", "Joaquim, deixa esse escudo na porta. Ninguém protege ninguém de barriga vazia."),
            new LocalizationEntry("ui.kitchen.grandma.greeting_icaro", "Ícaro, get your gadgets off my stove! ...Fine, but only if it boils the cane faster.", "Ícaro, tira essas engenhoca do meu fogão! ...Tá bom, mas só se ferver a cana mais rápido."),
            new LocalizationEntry("ui.kitchen.grandma.greeting_lavine", "Lavine, my little one. Come closer to the fire — the shadows don't come near Grandma's stove.", "Lavine, minha caçula. Chega perto do fogo, que sombra não se achega no fogão da Vó."),
            // Vó Zefa remembers the region the cousins just came back from (latest interlude / ending seen).
            new LocalizationEntry("ui.kitchen.grandma.after_forest", "So the Alpha Wolf calmed down? Sit — a tame beast deserves a pamonha, and a brave grandchild deserves two.", "Então o Lobo Alfa sossegou? Senta, que bicho manso merece pamonha e neto valente merece duas."),
            new LocalizationEntry("ui.kitchen.grandma.after_desert", "You've got sand up to your ears! Drink some water first — Seu Bento told me everything over the radio.", "Chegou com areia até na orelha! Bebe água primeiro, que o Seu Bento me contou tudo pelo rádio."),
            new LocalizationEntry("ui.kitchen.grandma.after_mountain", "That mountain cold only goes away with cloth-strained coffee. Mestre Tonho says you made the bell ring again!", "Esse frio de serra só sai com café coado. O Mestre Tonho disse que vocês fizeram o sino tocar!"),
            new LocalizationEntry("ui.kitchen.grandma.after_caves", "See the Crystal? Now eat — no light in the world fills a belly.", "Tá vendo o Cristal? Agora come, que luz nenhuma enche barriga."),
            new LocalizationEntry("ui.kitchen.grandma.after_forest_maithe", "Maithe, did you count your cousins? You did, I know. Now sit and let Grandma count the spoonfuls.", "Maithe, contou os primos? Contou, eu sei. Agora senta e deixa a Vó contar as colheradas."),
            new LocalizationEntry("ui.kitchen.grandma.after_desert_maithe", "My eldest, Ícaro told me on the radio you found the trail in the sand. Strong coffee for sharp eyes.", "Minha mais velha, o Ícaro me disse pelo rádio que foi tu que achou o rastro na areia. Café forte pra quem tem olho bom."),
            new LocalizationEntry("ui.kitchen.grandma.after_mountain_maithe", "Maithe, the mountain wind is tricky, but you brought everyone back. Sit — today Grandma does the looking after.", "Maithe, o vento da serra é traiçoeiro, mas tu voltou com todo mundo. Senta, que hoje quem cuida é a Vó."),
            new LocalizationEntry("ui.kitchen.grandma.after_caves_maithe", "Maithe, put that backpack down, dear. The Crystal is lit, and you don't have to keep watch over anything today.", "Maithe, deixa essa mochila no chão, minha filha. O Cristal tá aceso, e tu não precisa vigiar mais nada hoje."),
            new LocalizationEntry("ui.kitchen.grandma.after_forest_maria", "Maria, you were right: the Wolf wasn't bad. Come — a kind heart gets hungry too.", "Maria, tu tinha razão: o Lobo não era mau. Vem, que coração bom também sente fome."),
            new LocalizationEntry("ui.kitchen.grandma.after_desert_maria", "Maria, I prayed for you every night the radio crackled. Come tie the husks with me and tell me about the desert.", "Maria, rezei por vocês cada noite que o rádio chiava. Vem amarrar a palha comigo e me conta do deserto."),
            new LocalizationEntry("ui.kitchen.grandma.after_mountain_maria", "Maria, your little sun medal came back brighter than it left. Sit by the fire — the mountain chills you to the bone.", "Maria, tua medalhinha voltou mais brilhante do que foi. Senta perto do fogo, que a serra gela os ossos."),
            new LocalizationEntry("ui.kitchen.grandma.after_caves_maria", "Maria, my light. You lit up the dark slowly, the way you light a wood stove. Now eat.", "Maria, minha luz. Tu acendeu o escuro devagarinho, do jeito que se acende fogão a lenha. Agora come."),
            new LocalizationEntry("ui.kitchen.grandma.after_forest_joaquim", "Joaquim, so the Alpha Wolf respected your shield? Well, Grandma respects your plate: sit and have seconds.", "Joaquim, o Lobo Alfa respeitou o teu escudo, foi? Pois a Vó respeita o teu prato: senta e repete."),
            new LocalizationEntry("ui.kitchen.grandma.after_desert_joaquim", "Joaquim, you brought the whole desert in your boots! Leave them at the door and come — the baião is still warm.", "Joaquim, tu trouxe o deserto inteiro na bota! Tira na porta e vem, que o baião tá quentinho."),
            new LocalizationEntry("ui.kitchen.grandma.after_mountain_joaquim", "Joaquim, Mestre Tonho told me you stepped aside at the right moment. That's courage too, my grandson.", "Joaquim, o Mestre Tonho me contou que tu saiu da frente na hora certa. Isso é coragem também, meu neto."),
            new LocalizationEntry("ui.kitchen.grandma.after_caves_joaquim", "Joaquim, lean that shield on the wall and rest. The Crystal is lit and the village is safe — today you only guard your plate.", "Joaquim, encosta esse escudo e descansa. O Cristal tá aceso e a vila tá segura — hoje tu só protege o teu prato."),
            new LocalizationEntry("ui.kitchen.grandma.after_forest_icaro", "Ícaro, that little radio of yours is a wonder: I heard the Wolf howl from my stove! Sit — inventors eat too.", "Ícaro, esse teu radinho é uma beleza: ouvi o uivo do Lobo daqui do fogão! Senta, que inventor também come."),
            new LocalizationEntry("ui.kitchen.grandma.after_desert_icaro", "Ícaro, Seu Bento said you read the ancients' panel like a recipe. I wrote every word in the Notebook.", "Ícaro, o Seu Bento disse que tu leu o painel dos antigos como quem lê receita. Anotei tudinho no Caderno."),
            new LocalizationEntry("ui.kitchen.grandma.after_mountain_icaro", "Ícaro, so the mountain golems were built by someone too? Well, the best invention in this house is still rapadura. Taste it.", "Ícaro, os golens da serra também foram inventados, foi? Pois a melhor invenção dessa casa ainda é a rapadura. Prova."),
            new LocalizationEntry("ui.kitchen.grandma.after_caves_icaro", "Ícaro, put your gadgets away for a minute. Today Grandma just wants to see you eating and laughing with your cousins.", "Ícaro, guarda as engenhoca um minutinho. Hoje a Vó só quer ver tu comendo e rindo com os primos."),
            new LocalizationEntry("ui.kitchen.grandma.after_forest_lavine", "Lavine, my youngest, was the Forest night cold? Come to the fire — the ribbon on your wrist says you're ours.", "Lavine, minha caçula, a noite da Floresta foi fria? Chega no fogo, que a fita no teu pulso diz que tu é nossa."),
            new LocalizationEntry("ui.kitchen.grandma.after_desert_lavine", "Lavine, you came back as serious as a grown-up. But the queijo coalho still squeaks for you, you know?", "Lavine, tu voltou séria que nem gente grande. Mas o queijo coalho ainda chia pra tu, viu?"),
            new LocalizationEntry("ui.kitchen.grandma.after_mountain_lavine", "Lavine, you fell asleep on Maithe's shoulder by the campfire, didn't you? She told me quietly on the radio. Come here — Grandma's fire is even warmer.", "Lavine, dormiu no ombro da Maithe na fogueira, foi? Ela me contou baixinho no rádio. Vem cá, que o fogo da Vó é mais quentinho."),
            new LocalizationEntry("ui.kitchen.grandma.after_caves_lavine", "Lavine, my brave youngest. You showed the dark that a shadow can be soft too. Come sit on Grandma's lap.", "Lavine, minha caçula valente. Tu mostrou pro escuro que sombra também pode ser macia. Senta no colo da Vó."),
            new LocalizationEntry("ui.kitchen.grandma.lesson_maria", "Maria, it's time you learned the pamonha: husk well tied, a pinch of salt and a lot of patience.", "Maria, tá na hora de tu aprender a pamonha: palha bem amarrada, uma pitada de sal e muita paciência."),
            new LocalizationEntry("ui.kitchen.grandma.lesson_joaquim", "Joaquim, come learn the baião de dois. Rice and beans together, like you and your sister.", "Joaquim, vem aprender o baião de dois. Arroz e feijão junto, que nem tu e tua irmã."),
            new LocalizationEntry("ui.kitchen.grandma.lesson_icaro", "Ícaro, this is how rapadura is made: stir the tacho and don't stop, not even to invent anything.", "Ícaro, rapadura se faz assim: mexe o tacho e não para, nem pra inventar nada."),
            new LocalizationEntry("ui.kitchen.grandma.lesson_lavine", "Lavine, hold the skewer like this. The queijo coalho squeaks at first, then it turns golden — like you.", "Lavine, segura o espeto assim. O queijo coalho chia no começo, depois fica douradinho — que nem tu."),

            // -- Dishes (Rapadura.Gameplay.Cooking.MealCatalog) --
            new LocalizationEntry("meal.cuscuz.name", "Cuscuz", "Cuscuz"),
            new LocalizationEntry("meal.cuscuz.effect", "+30% stamina regeneration", "+30% de regeneração de vigor"),
            new LocalizationEntry("meal.cafe_coado.name", "Strained coffee", "Café coado"),
            new LocalizationEntry("meal.cafe_coado.effect", "+6% cooldown reduction", "+6% de recarga das habilidades"),
            new LocalizationEntry("meal.cafe_coado.favourite", "+3% critical chance. \"Maithe never misses with coffee in her.\"", "+3% de chance de crítico. \"Com café no bucho, a Maithe não erra.\""),
            new LocalizationEntry("meal.pamonha.name", "Pamonha", "Pamonha"),
            new LocalizationEntry("meal.pamonha.effect", "+10% max health", "+10% de vida máxima"),
            new LocalizationEntry("meal.pamonha.favourite", "+1 health per second. \"Maria's comes out the softest.\"", "+1 de vida por segundo. \"A da Maria sai a mais macia.\""),
            new LocalizationEntry("meal.rapadura.name", "Rapadura", "Rapadura"),
            new LocalizationEntry("meal.rapadura.effect", "+12% attack speed for the first 20 s of each fight", "+12% de velocidade de ataque nos primeiros 20 s de cada luta"),
            new LocalizationEntry("meal.rapadura.favourite", "+8% move speed during the rush. \"Ícaro on sugar is a hazard.\"", "+8% de velocidade de movimento no embalo. \"Ícaro com açúcar é um perigo.\""),
            new LocalizationEntry("meal.baiao_de_dois.name", "Baião de dois", "Baião de dois"),
            new LocalizationEntry("meal.baiao_de_dois.effect", "+10% defense", "+10% de defesa"),
            new LocalizationEntry("meal.baiao_de_dois.favourite", "+5% max health. \"Joaquim always asks for seconds.\"", "+5% de vida máxima. \"O Joaquim sempre repete.\""),
            new LocalizationEntry("meal.queijo_coalho.name", "Grilled queijo coalho", "Queijo coalho assado"),
            new LocalizationEntry("meal.queijo_coalho.effect", "Returns 8% of melee damage taken", "Devolve 8% do dano corpo a corpo recebido"),
            new LocalizationEntry("meal.queijo_coalho.favourite", "+20% mana regeneration. \"Lavine eats it straight off the skewer.\"", "+20% de regeneração de mana. \"A Lavine come direto do espeto.\""),
            new LocalizationEntry("meal.tilapia_na_brasa.name", "Coal-grilled tilapia", "Tilápia na brasa"),
            new LocalizationEntry("meal.tilapia_na_brasa.effect", "+25% mana regeneration", "+25% de regeneração de mana"),
            new LocalizationEntry("meal.caldo_de_piaba.name", "Piaba broth", "Caldo de piaba"),
            new LocalizationEntry("meal.caldo_de_piaba.effect", "+1.5 health per second", "+1,5 de vida por segundo"),
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
    }
}
