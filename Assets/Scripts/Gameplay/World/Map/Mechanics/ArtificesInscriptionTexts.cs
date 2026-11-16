using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of "o painel do aviso" of the Ruínas dos Artífices (08, <see cref="ArtificesInscriptionRules"/>): the three lines
    /// of the Artificers' warning, the clue of the seal and one line per cousin when the panel is complete. Same self-contained pattern as
    /// <c>AreiasSouthTexts</c>.
    /// </summary>
    public static class ArtificesInscriptionTexts
    {
        public const string Prompt = "ui.artifices.inscription_prompt";
        public const string AlreadyRead = "ui.artifices.inscription_read";
        public const string Stela0 = "ui.artifices.inscription_0";
        public const string Stela1 = "ui.artifices.inscription_1";
        public const string Stela2 = "ui.artifices.inscription_2";
        public const string Progress = "ui.artifices.inscription_progress";
        public const string Clue = "ui.artifices.inscription_clue";
        public const string LineMaithe = "ui.artifices.inscription_maithe";
        public const string LineIcaro = "ui.artifices.inscription_icaro";
        public const string LineJoaquim = "ui.artifices.inscription_joaquim";
        public const string LineMaria = "ui.artifices.inscription_maria";
        public const string LineLavine = "ui.artifices.inscription_lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(Prompt, "[E] Read the inscription", "[E] Ler a inscrição"),
            new LocalizationEntry(AlreadyRead, "You already know this line of the warning", "Você já sabe esta linha do aviso"),
            new LocalizationEntry(Stela0, "\"We made the one who answers out of clay and light. It answers whoever calls it by the old name.\"", "\"Fizemos de barro e luz aquele que responde. Ele responde a quem o chama pelo nome antigo.\""),
            new LocalizationEntry(Stela1, "\"The dark came in through the seal on its back. Since then it answers only the dark.\"", "\"A treva entrou pelo selo das costas. Desde então ele só responde à treva.\""),
            new LocalizationEntry(Stela2, "\"When it kneels to answer, the seal shows. Break the seal and the light goes back to the clay.\"", "\"Quando ele se ajoelha para responder, o selo aparece. Quebre o selo e a luz volta para o barro.\""),
            new LocalizationEntry(Progress, "Panel of the warning: {0}/{1} read (+{2} XP)", "Painel do aviso: {0}/{1} lidas (+{2} XP)"),
            new LocalizationEntry(Clue, "Clue of the seal: the Grande Ushabti shows the seal on its back when it kneels. +{0} XP", "Pista do selo: o Grande Ushabti mostra o selo das costas quando se ajoelha. +{0} XP"),
            new LocalizationEntry(LineMaithe, "Maithe: So it isn't evil. Somebody just locked the dark inside it.", "Maithe: Então ele não é mau. Alguém só trancou a treva lá dentro."),
            new LocalizationEntry(LineIcaro, "Ícaro: Clay, light and a seal on the back... I could have built that. I'd have put the seal somewhere safer.", "Ícaro: Barro, luz e um selo nas costas... Eu podia ter feito isso. Só que punha o selo num lugar mais seguro."),
            new LocalizationEntry(LineJoaquim, "Joaquim: Vô Jorge always said: read the sign before you go in. I finally get it.", "Joaquim: O Vô Jorge sempre dizia: lê a placa antes de entrar. Agora eu entendi."),
            new LocalizationEntry(LineMaria, "Maria: Wait for it to kneel, then hit the back. Easy. I think.", "Maria: Esperar ele ajoelhar e bater nas costas. Fácil. Acho."),
            new LocalizationEntry(LineLavine, "Lavine: Vó Edna would like these people. They left a warning for whoever came after.", "Lavine: A Vó Edna ia gostar desse povo. Deixaram um aviso para quem viesse depois."),
        };

        /// <summary>The key of the inscription on stela <paramref name="index"/> (0–2).</summary>
        public static string StelaKey(int index) => index == 0 ? Stela0 : index == 1 ? Stela1 : Stela2;

        /// <summary>The cousin's line when the panel is complete (Joaquim's when no cousin is known).</summary>
        public static string LineFor(CharacterId? hero)
        {
            switch (hero)
            {
                case CharacterId.Maithe: return LineMaithe;
                case CharacterId.Icaro: return LineIcaro;
                case CharacterId.Maria: return LineMaria;
                case CharacterId.Lavine: return LineLavine;
                default: return LineJoaquim;
            }
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(Prompt))
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
    }
}
