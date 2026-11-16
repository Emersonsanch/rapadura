using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the shadow clock of the Torres Brancas (42, <see cref="ShadowClockRules"/>): the prompt, the reading, the night,
    /// and one line per cousin the first time they read it. Same self-contained pattern as <see cref="QochoOasisTexts"/>.
    /// </summary>
    public static class ShadowClockTexts
    {
        /// <summary>The save mark of the first reading (a time mark, set once).</summary>
        public const string FirstReadKey = "mingcheng_shadow_clock_read";

        public const string Prompt = "ui.shadow_clock.prompt";
        public const string Reading = "ui.shadow_clock.reading";
        public const string Night = "ui.shadow_clock.night";
        public const string FaceNorth = "ui.shadow_clock.face_north";
        public const string FaceEast = "ui.shadow_clock.face_east";
        public const string FaceSouth = "ui.shadow_clock.face_south";
        public const string FaceWest = "ui.shadow_clock.face_west";
        public const string LineMaithe = "ui.shadow_clock.maithe";
        public const string LineIcaro = "ui.shadow_clock.icaro";
        public const string LineJoaquim = "ui.shadow_clock.joaquim";
        public const string LineMaria = "ui.shadow_clock.maria";
        public const string LineLavine = "ui.shadow_clock.lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(Prompt, "[E] Read the shadow clock (the freed Little Eclipse)", "[E] Ler o relógio de sombra (o Pequeno Eclipse libertado)"),
            new LocalizationEntry(Reading, "{0}h · the light turns in {1} h · the north mirror must face {2}",
                "{0}h · a luz muda em {1} h · o espelho do norte deve olhar para o {2}"),
            new LocalizationEntry(Night, "Night: the shadow clock sleeps with the sun", "Noite: o relógio de sombra dorme com o sol"),
            new LocalizationEntry(FaceNorth, "north", "norte"),
            new LocalizationEntry(FaceEast, "east", "leste"),
            new LocalizationEntry(FaceSouth, "south", "sul"),
            new LocalizationEntry(FaceWest, "west", "oeste"),
            new LocalizationEntry(LineMaithe, "Maithe: It used to steal the light. Now it counts it for us.", "Maithe: Ele roubava a luz. Agora conta ela para a gente."),
            new LocalizationEntry(LineIcaro, "Ícaro: A clock with no gears. Annoyingly elegant.", "Ícaro: Um relógio sem engrenagem. Irritantemente elegante."),
            new LocalizationEntry(LineJoaquim, "Joaquim: Even a shadow can be useful, if it stands in the right place.", "Joaquim: Até uma sombra serve, se ficar no lugar certo."),
            new LocalizationEntry(LineMaria, "Maria: I'm taller than the clock's shadow! For now.", "Maria: Eu sou maior que a sombra do relógio! Por enquanto."),
            new LocalizationEntry(LineLavine, "Lavine: Vó Edna tells the time by the sun on the porch. Same thing!", "Lavine: A Vó Edna vê a hora pelo sol na varanda. É igualzinho!"),
        };

        /// <summary>The name of mirror facing <paramref name="state"/> (0 north, 1 east, 2 south, 3 west).</summary>
        public static string FaceKey(int state)
        {
            switch (((state % 4) + 4) % 4)
            {
                case 0: return FaceNorth;
                case 1: return FaceEast;
                case 2: return FaceSouth;
                default: return FaceWest;
            }
        }

        /// <summary>The cousin's line the first time they read the clock (Joaquim's when no cousin is known).</summary>
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
