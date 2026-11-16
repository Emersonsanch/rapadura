using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the marked trunks of the Copas de Garajonay (05). Same self-contained pattern as <see cref="ZarafshanGardenTexts"/>.</summary>
    public static class GarajonayTrunkTexts
    {
        public const string Prompt = "ui.garajonay_trunk.prompt";
        public const string NeedsLizard = "ui.garajonay_trunk.needs_lizard";
        public const string Resting = "ui.garajonay_trunk.resting";
        public const string Climbs = "ui.garajonay_trunk.climbs";
        public const string Landed = "ui.garajonay_trunk.landed";
        public const string LineJoaquim = "ui.garajonay_trunk.line.joaquim";
        public const string LineMaria = "ui.garajonay_trunk.line.maria";
        public const string LineMaithe = "ui.garajonay_trunk.line.maithe";
        public const string LineIcaro = "ui.garajonay_trunk.line.icaro";
        public const string LineLavine = "ui.garajonay_trunk.line.lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(Prompt, "[E] Climb the marked trunk and cut across the canopy", "[E] Subir o tronco marcado e cortar caminho pelas copas"),
            new LocalizationEntry(NeedsLizard, "Claw marks up the bark: only a Laurel Lizard climbs here", "Marcas de garra na casca: só o Lagarto-do-Louro sobe aqui"),
            new LocalizationEntry(Resting, "The lizard catches its breath", "O lagarto recupera o fôlego"),
            new LocalizationEntry(Climbs, "Up the trunk and along the branches!", "Tronco acima e pelos galhos!"),
            new LocalizationEntry(Landed, "Down the other trunk, across the forest", "Desceu pelo outro tronco, do outro lado da mata"),
            new LocalizationEntry(LineJoaquim, "Joaquim: \"I didn't look down. Not once.\"", "Joaquim: \"Eu não olhei para baixo. Nenhuma vez.\""),
            new LocalizationEntry(LineMaria, "Maria: \"Dácil was right: it never misses a branch.\"", "Maria: \"A Dácil tinha razão: ele não erra um galho.\""),
            new LocalizationEntry(LineMaithe, "Maithe: \"Again! Can we go again?\"", "Maithe: \"De novo! Dá para ir de novo?\""),
            new LocalizationEntry(LineIcaro, "Ícaro: \"From up there the fog looks like milk.\"", "Ícaro: \"Lá de cima a névoa parece leite.\""),
            new LocalizationEntry(LineLavine, "Lavine: \"The laurels drink the fog. I saw it from the top.\"", "Lavine: \"Os louros bebem a névoa. Eu vi lá de cima.\""),
        };

        /// <summary>The cousin's line on coming down.</summary>
        public static string LineFor(CharacterId? hero)
        {
            switch (hero)
            {
                case CharacterId.Maria: return LineMaria;
                case CharacterId.Maithe: return LineMaithe;
                case CharacterId.Icaro: return LineIcaro;
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
