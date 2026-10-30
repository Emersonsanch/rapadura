using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Pets
{
    /// <summary>
    /// Localization keys (en + pt-BR) of Caramelo, Vó Zefa's caramel mutt: the settings toggle, the world
    /// map "Fazer carinho" button, Vó Zefa's introduction (the small village beat that explains why he
    /// joins the cousins), two cuddle lines per cousin, the buff note and the "Caramelo trouxe!" fetch
    /// text. Same self-contained pattern as <c>TouchControlsTexts</c>: merged into the live
    /// <see cref="LocalizationManager"/> on first use (keys it already has win), pt-BR fallback otherwise.
    /// </summary>
    public static class CarameloTexts
    {
        public const string Name = "pet.caramelo.name";
        public const string SettingsToggle = "ui.settings.accessibility.companion_caramelo";
        public const string CuddleButton = "ui.pet.caramelo.cuddle_button";
        /// <summary>Before Vó Zefa introduced him: "Caramelo (!)".</summary>
        public const string CuddleButtonNew = "ui.pet.caramelo.cuddle_button_new";
        public const string Intro = "pet.caramelo.intro";
        public const string BuffNote = "pet.caramelo.buff_note";
        public const string BuffName = "pet.caramelo.buff_name";
        public const string Fetched = "pet.caramelo.fetched";
        /// <summary>Prefix of the cuddle lines: <c>pet.caramelo.cuddle.&lt;hero&gt;.&lt;n&gt;</c> (hero in lower case, n = 0..<see cref="LinesPerHero"/>-1).</summary>
        public const string CuddlePrefix = "pet.caramelo.cuddle.";
        public const int LinesPerHero = 2;

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(Name, "Caramelo", "Caramelo"),
            new LocalizationEntry(SettingsToggle, "Caramelo (companion in encounters)", "Caramelo (companheiro nos encontros)"),
            new LocalizationEntry(CuddleButton, "Pet Caramelo", "Fazer carinho"),
            new LocalizationEntry(CuddleButtonNew, "Caramelo (!)", "Caramelo (!)"),
            new LocalizationEntry(Intro,
                "Grandma Zefa: \"This mutt showed up at the gate the day the Crystal went dark and never left. That's Caramelo. He barks before any danger, so take him with you.\"",
                "Vó Zefa: \"Esse vira-lata apareceu na porteira no dia em que o Cristal apagou e não arredou mais o pé. É o Caramelo. Ele late antes de qualquer perigo, então levem ele com vocês.\""),
            new LocalizationEntry(BuffNote, "Caramelo's cuddle: +5% speed in the next encounter", "Carinho do Caramelo: +5% de velocidade no próximo encontro"),
            new LocalizationEntry(BuffName, "Caramelo's cuddle", "Carinho do Caramelo"),
            new LocalizationEntry(Fetched, "Caramelo brought it!", "Caramelo trouxe!"),

            // Maithe: the eldest, Eye of the Wind; Ícaro's big sister.
            new LocalizationEntry(CuddlePrefix + "maithe.0",
                "Maithe: \"Good boy. You keep an eye on the wind with me, deal?\"",
                "Maithe: \"Bom garoto. Você fica de olho no vento comigo, combinado?\""),
            new LocalizationEntry(CuddlePrefix + "maithe.1",
                "Maithe: \"Ícaro tried to teach you to shake hands with science. I just used a biscuit.\"",
                "Maithe: \"O Ícaro tentou te ensinar a dar a pata com ciência. Comigo foi só um biscoito.\""),
            // Maria: the dreamer who decides; Joaquim's big sister.
            new LocalizationEntry(CuddlePrefix + "maria.0",
                "Maria: \"I dreamed about you, you know? You barked and the light came back.\"",
                "Maria: \"Sonhei com você, sabia? Você latia e a luz voltava.\""),
            new LocalizationEntry(CuddlePrefix + "maria.1",
                "Maria: \"If Joaquim runs ahead again, you go after him, okay? I trust you.\"",
                "Maria: \"Se o Joaquim correr na frente de novo, você vai atrás dele, tá? Eu confio em você.\""),
            // Joaquim: always first into danger, Shield of Roots; Maria's little brother.
            new LocalizationEntry(CuddlePrefix + "joaquim.0",
                "Joaquim: \"I go in front, you come right behind. Team of two!\"",
                "Joaquim: \"Eu vou na frente, você vem logo atrás. Time de dois!\""),
            new LocalizationEntry(CuddlePrefix + "joaquim.1",
                "Joaquim: \"Nobody messes with Caramelo. Not lizards, not bats, not even Maria when he eats her rapadura.\"",
                "Joaquim: \"Ninguém mexe com o Caramelo. Nem lagarto, nem morcego, nem a Maria quando ele come a rapadura dela.\""),
            // Ícaro: the little scientist; Maithe's little brother.
            new LocalizationEntry(CuddlePrefix + "icaro.0",
                "Ícaro: \"Tail wags per minute: a lot. Scientific conclusion: you are happy.\"",
                "Ícaro: \"Abanadas de rabo por minuto: muitas. Conclusão científica: você está feliz.\""),
            new LocalizationEntry(CuddlePrefix + "icaro.1",
                "Ícaro: \"Your nose works better than my light meter. Don't tell Maithe.\"",
                "Ícaro: \"Seu faro funciona melhor que o meu medidor de luz. Não conta pra Maithe.\""),
            // Lavine: the youngest, cousin to all, born on the Noite Rubra.
            new LocalizationEntry(CuddlePrefix + "lavine.0",
                "Lavine: \"You're family too, right, Caramelo? Just like me.\"",
                "Lavine: \"Você também é da família, né, Caramelo? Igualzinho a mim.\""),
            new LocalizationEntry(CuddlePrefix + "lavine.1",
                "Lavine: \"When the shadow gets cold, you lean on me. Thank you.\"",
                "Lavine: \"Quando a sombra esfria, você encosta em mim. Obrigada.\""),
            // Stage lines: what the cousin remembers of the region they just came back from (latest interlude seen).
            new LocalizationEntry(CuddlePrefix + "maithe.after_forest",
                "Maithe: \"You sniffed out the golden sand before I did. Are you competing with me?\"",
                "Maithe: \"Você farejou a areia dourada antes de mim. Tá competindo comigo, é?\""),
            new LocalizationEntry(CuddlePrefix + "maithe.after_desert",
                "Maithe: \"Built guardians, a cracking seal... and all you want is a belly rub. Fine, here.\"",
                "Maithe: \"Guardião construído, selo rachando... e você só quer coçar a barriga. Tá bom, toma.\""),
            new LocalizationEntry(CuddlePrefix + "maithe.after_mountain",
                "Maithe: \"Up there the wind fooled my Eye, but your nose didn't. Good boy.\"",
                "Maithe: \"Lá em cima o vento enganou o meu Olho, mas o seu nariz não. Bom garoto.\""),
            new LocalizationEntry(CuddlePrefix + "maithe.after_caves",
                "Maithe: \"Go ahead and run in front, Caramelo. Today I'm not counting anyone. ...Okay, just once.\"",
                "Maithe: \"Pode correr na frente, Caramelo. Hoje eu não conto ninguém. ...Tá, só uma vez.\""),
            new LocalizationEntry(CuddlePrefix + "maria.after_forest",
                "Maria: \"The Wolf had a dark little stone on his collar, you know? Good thing yours is just leather.\"",
                "Maria: \"O Lobo tinha uma pedrinha escura na coleira, sabia? Ainda bem que a sua é só de couro.\""),
            new LocalizationEntry(CuddlePrefix + "maria.after_desert",
                "Maria: \"The ancients left guardians to keep watch. We have you.\"",
                "Maria: \"Os antigos deixaram guardiões vigiando. A gente tem você.\""),
            new LocalizationEntry(CuddlePrefix + "maria.after_mountain",
                "Maria: \"I saved you another little piece of rapadura from the campfire. Don't tell Joaquim.\"",
                "Maria: \"Guardei mais um pedacinho de rapadura da fogueira pra você. Não conta pro Joaquim.\""),
            new LocalizationEntry(CuddlePrefix + "maria.after_caves",
                "Maria: \"In my dream there were five cousins and one caramel dog. Everybody came back.\"",
                "Maria: \"No meu sonho tinha cinco primos e um cachorro caramelo. Voltou todo mundo.\""),
            new LocalizationEntry(CuddlePrefix + "joaquim.after_forest",
                "Joaquim: \"See how the Alpha Wolf calmed down? It was the shield. And a little bit you, okay.\"",
                "Joaquim: \"Viu como o Lobo Alfa sossegou? Foi o escudo. E um pouquinho você, tá.\""),
            new LocalizationEntry(CuddlePrefix + "joaquim.after_desert",
                "Joaquim: \"You sneezed sand on me the whole trip. Team of two, but I get the side without sand.\"",
                "Joaquim: \"Você espirrou areia em mim a viagem inteira. Time de dois, mas eu fico com o lado sem areia.\""),
            new LocalizationEntry(CuddlePrefix + "joaquim.after_mountain",
                "Joaquim: \"On the mountain I learned to step aside. You already knew, right? You always come back to check we're coming.\"",
                "Joaquim: \"Na serra eu aprendi a sair da frente. Você já sabia, né? Sempre volta pra ver se a gente vem.\""),
            new LocalizationEntry(CuddlePrefix + "joaquim.after_caves",
                "Joaquim: \"Shield on the wall, belly full, dog on my lap. Now that's a victory.\"",
                "Joaquim: \"Escudo encostado, barriga cheia, cachorro no colo. Isso é que é vitória.\""),
            new LocalizationEntry(CuddlePrefix + "icaro.after_forest",
                "Ícaro: \"You barked three seconds before the Wolf showed up. I'm calling it the Caramelo radar.\"",
                "Ícaro: \"Você latiu três segundos antes do Lobo aparecer. Vou chamar isso de radar Caramelo.\""),
            new LocalizationEntry(CuddlePrefix + "icaro.after_desert",
                "Ícaro: \"The ancients' panel talked about guardians. Scientific hypothesis: you're a furry guardian.\"",
                "Ícaro: \"O painel dos antigos falava de guardiões. Hipótese científica: você é um guardião peludo.\""),
            new LocalizationEntry(CuddlePrefix + "icaro.after_mountain",
                "Ícaro: \"Experiment: rapadura in the left hand, rapadura in the right. Result: you ate both.\"",
                "Ícaro: \"Experimento: rapadura na mão esquerda, rapadura na direita. Resultado: você comeu as duas.\""),
            new LocalizationEntry(CuddlePrefix + "icaro.after_caves",
                "Ícaro: \"The light meter is maxed out. So is the tail-wag meter.\"",
                "Ícaro: \"O medidor de luz tá no máximo. E o medidor de abanadas também.\""),
            new LocalizationEntry(CuddlePrefix + "lavine.after_forest",
                "Lavine: \"The Forest night gave me the shivers. You slept leaning on me. That was on purpose, wasn't it?\"",
                "Lavine: \"A noite na Floresta deu friozinho. Você dormiu encostado em mim. Foi de propósito, né?\""),
            new LocalizationEntry(CuddlePrefix + "lavine.after_desert",
                "Lavine: \"In the sandstorm I couldn't see anything. But I heard you. That was enough.\"",
                "Lavine: \"Na tempestade de areia eu não vi nada. Mas ouvi você. Foi o suficiente.\""),
            new LocalizationEntry(CuddlePrefix + "lavine.after_mountain",
                "Lavine: \"It's really dark down in the Caves, Caramelo. We go together, okay? I hold the ribbon, you hold the courage.\"",
                "Lavine: \"Lá embaixo nas Cavernas é bem escuro, Caramelo. A gente vai junto, tá? Eu seguro a fita, você segura a coragem.\""),
            new LocalizationEntry(CuddlePrefix + "lavine.after_caves",
                "Lavine: \"You're not afraid of the dark anymore either, right, Caramelo?\"",
                "Lavine: \"Você também não tem mais medo do escuro, né, Caramelo?\""),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Key of the <paramref name="index"/>-th cuddle line of <paramref name="hero"/> (wraps around <see cref="LinesPerHero"/>).</summary>
        public static string CuddleLineKey(CharacterId hero, int index)
        {
            int n = ((index % LinesPerHero) + LinesPerHero) % LinesPerHero;
            return CuddlePrefix + hero.ToString().ToLowerInvariant() + "." + n;
        }

        /// <summary>
        /// Key of <paramref name="hero"/>'s line for the region they just came back from:
        /// <c>pet.caramelo.cuddle.&lt;hero&gt;.after_&lt;region&gt;</c> (forest/desert/mountain/caves).
        /// </summary>
        public static string StageLineKey(CharacterId hero, BiomeType cameFrom)
        {
            return CuddlePrefix + hero.ToString().ToLowerInvariant() + ".after_" + cameFrom.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// The <paramref name="index"/>-th cuddle line once the campaign moved on: the rotation gains the stage line of
        /// <paramref name="cameFrom"/> (index 0, 3, 6... → <see cref="StageLineKey"/>, the others the usual lines).
        /// Without a region (before the Forest interlude) it is <see cref="CuddleLineKey(CharacterId, int)"/>.
        /// </summary>
        public static string CuddleLineKey(CharacterId hero, int index, BiomeType? cameFrom)
        {
            if (!cameFrom.HasValue)
            {
                return CuddleLineKey(hero, index);
            }

            int cycle = LinesPerHero + 1;
            int n = ((index % cycle) + cycle) % cycle;
            return n == 0 ? StageLineKey(hero, cameFrom.Value) : CuddleLineKey(hero, n - 1);
        }

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Name)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        /// <summary>Localized text via <paramref name="manager"/> when available, else the pt-BR fallback, else the key.</summary>
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
