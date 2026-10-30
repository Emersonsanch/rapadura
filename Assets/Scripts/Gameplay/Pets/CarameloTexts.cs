using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

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
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Key of the <paramref name="index"/>-th cuddle line of <paramref name="hero"/> (wraps around <see cref="LinesPerHero"/>).</summary>
        public static string CuddleLineKey(CharacterId hero, int index)
        {
            int n = ((index % LinesPerHero) + LinesPerHero) % LinesPerHero;
            return CuddlePrefix + hero.ToString().ToLowerInvariant() + "." + n;
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
