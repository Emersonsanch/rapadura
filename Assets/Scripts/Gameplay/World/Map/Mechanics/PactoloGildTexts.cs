using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the gilding of the Vale dos Sátiros (53, <see cref="PactoloGildRules"/>): the gold piling on, flaking off and
    /// washed off in the Pactolo, and one line per cousin the first time the river washes them. Same self-contained pattern as <see cref="QochoOasisTexts"/>.
    /// </summary>
    public static class PactoloGildTexts
    {
        public const string Gilded = "ui.pactolo_gild.gilded";
        public const string Heavy = "ui.pactolo_gild.heavy";
        public const string Flaked = "ui.pactolo_gild.flaked";
        public const string Washed = "ui.pactolo_gild.washed";
        public const string LineMaithe = "ui.pactolo_gild.maithe";
        public const string LineIcaro = "ui.pactolo_gild.icaro";
        public const string LineJoaquim = "ui.pactolo_gild.joaquim";
        public const string LineMaria = "ui.pactolo_gild.maria";
        public const string LineLavine = "ui.pactolo_gild.lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(Gilded, "Gilded ({0}/{1}): the gold weighs on your legs", "Dourado ({0}/{1}): o ouro pesa nas pernas"),
            new LocalizationEntry(Heavy, "Covered in gold! Wash it off in the Pactolo", "Coberto de ouro! Lave no Pactolo"),
            new LocalizationEntry(Flaked, "A layer of gold flakes off", "Uma camada de ouro descasca"),
            new LocalizationEntry(Washed, "The Pactolo washes the gold off: Living Water", "O Pactolo lava o ouro: Água Viva"),
            new LocalizationEntry(LineMaithe, "Maithe: The river hums when the gold comes off. Like it was glad.", "Maithe: O rio zumbe quando o ouro sai. Como se estivesse contente."),
            new LocalizationEntry(LineIcaro, "Ícaro: Rich for five seconds. Worst five seconds of my life.", "Ícaro: Rico por cinco segundos. Os piores cinco segundos da minha vida."),
            new LocalizationEntry(LineJoaquim, "Joaquim: Midas learned it here too: gold is heavy, water is light.", "Joaquim: O Midas aprendeu aqui também: ouro pesa, água é leve."),
            new LocalizationEntry(LineMaria, "Maria: Look, the pebbles took my gold! Now they shine more.", "Maria: Olha, as pedrinhas pegaram o meu ouro! Agora brilham mais."),
            new LocalizationEntry(LineLavine, "Lavine: Vó Edna always said: wash your hands before you touch anything.", "Lavine: A Vó Edna sempre disse: lava a mão antes de pegar nas coisas."),
        };

        /// <summary>The cousin's line the first time the Pactolo washes them (Joaquim's when no cousin is known).</summary>
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
                if (!manager.HasKey(Gilded))
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
