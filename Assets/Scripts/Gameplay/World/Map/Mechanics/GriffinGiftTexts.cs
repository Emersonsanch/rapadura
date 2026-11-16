using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the old griffin's presents (48, <see cref="GriffinGiftRules"/>): the prompt, each cousin's line on taking their
    /// piece of gold, and the line for a cousin who already took theirs. Same self-contained pattern as <see cref="QochoOasisTexts"/>.
    /// </summary>
    public static class GriffinGiftTexts
    {
        public const string Prompt = "ui.griffin_gift.prompt";
        public const string Taken = "ui.griffin_gift.taken";
        public const string BagFull = "ui.griffin_gift.bag_full";
        public const string Received = "ui.griffin_gift.received";
        public const string LineMaithe = "ui.griffin_gift.maithe";
        public const string LineIcaro = "ui.griffin_gift.icaro";
        public const string LineJoaquim = "ui.griffin_gift.joaquim";
        public const string LineMaria = "ui.griffin_gift.maria";
        public const string LineLavine = "ui.griffin_gift.lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(Prompt, "[E] Take the piece of gold the old griffin left for you", "[E] Pegar a pecinha de ouro que o grifo velho deixou para você"),
            new LocalizationEntry(Taken, "Your piece is already with you: the others wait for your cousins", "A sua pecinha já está com você: as outras esperam os seus primos"),
            new LocalizationEntry(BagFull, "Your bag is full: the gift waits on the hoard", "Mochila cheia: o presente espera no tesouro"),
            new LocalizationEntry(Received, "The griffin's gift: {0} × {1}", "Presente do grifo: {0} × {1}"),
            new LocalizationEntry(LineMaithe, "Maithe: A gold panther, curled up asleep. The wind sings through it.", "Maithe: Uma pantera de ouro, enrolada dormindo. O vento canta por dentro dela."),
            new LocalizationEntry(LineIcaro, "Ícaro: A comb with two riders on it. Look at the hinges on these tiny horses!", "Ícaro: Um pente com dois cavaleiros em cima. Olha a articulação desses cavalinhos!"),
            new LocalizationEntry(LineJoaquim, "Joaquim: A gold stag lying down. It remembered who to give it to after all.", "Joaquim: Um cervo de ouro deitado. No fim ele lembrou para quem guardava."),
            new LocalizationEntry(LineMaria, "Maria: A little bell for a horse! Vô Jorge has to hear this.", "Maria: Um sininho de cavalo! O Vô Jorge precisa ouvir isso."),
            new LocalizationEntry(LineLavine, "Lavine: The cup of the sky, small enough for me. I told you I wasn't little.", "Lavine: A taça do céu, do meu tamanho. Eu disse que não era pequena."),
        };

        /// <summary>The cousin's line on taking their piece of gold.</summary>
        public static string LineFor(CharacterId hero)
        {
            switch (hero)
            {
                case CharacterId.Maithe: return LineMaithe;
                case CharacterId.Icaro: return LineIcaro;
                case CharacterId.Joaquim: return LineJoaquim;
                case CharacterId.Maria: return LineMaria;
                default: return LineLavine;
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
