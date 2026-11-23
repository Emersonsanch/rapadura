using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the night tremor of the Salão de Tages (20, <see cref="TagesTremorRules"/>).</summary>
    public static class TagesTremorTexts
    {
        public const string Warning = "ui.tages_tremor.warning";
        public const string Shook = "ui.tages_tremor.shook";
        public const string Held = "ui.tages_tremor.held";
        public const string Quiet = "ui.tages_tremor.quiet";
        public const string LineJoaquim = "ui.tages_tremor.line.joaquim";
        public const string LineMaria = "ui.tages_tremor.line.maria";
        public const string LineMaithe = "ui.tages_tremor.line.maithe";
        public const string LineIcaro = "ui.tages_tremor.line.icaro";
        public const string LineLavine = "ui.tages_tremor.line.lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(Warning, "The floor of the hall hums: stand by a lamp to hold it steady!", "O chão do salão zumbe: fique perto de uma lâmpada para segurá-la firme!"),
            new LocalizationEntry(Shook, "The hall shakes: {0} lamp(s) flicker and lose light", "O salão treme: {0} lâmpada(s) tremem e perdem luz"),
            new LocalizationEntry(Held, "You held the lamp steady through the tremor", "Você segurou a lâmpada firme no tremor"),
            new LocalizationEntry(Quiet, "The hall shakes, but no lamp was burning to lose its light", "O salão treme, mas nenhuma lâmpada acesa tinha luz a perder"),
            new LocalizationEntry(LineJoaquim, "Joaquim: \"Easy, little flame. Grandpa Jorge holds the oil lamp like this when the bus jolts.\"",
                "Joaquim: \"Calma, chaminha. O Vô Jorge segura o lampião assim quando o ônibus balança.\""),
            new LocalizationEntry(LineMaria, "Maria: \"The hall sighs at night. It's afraid of the dark too.\"",
                "Maria: \"O salão suspira de noite. Ele também tem medo do escuro.\""),
            new LocalizationEntry(LineMaithe, "Maithe: \"Ícaro, hold the lamp, don't hold me!\"",
                "Maithe: \"Ícaro, segura a lâmpada, não a mim!\""),
            new LocalizationEntry(LineIcaro, "Ícaro: \"A whole mountain shaking and I'm worried about a little lamp. Fine.\"",
                "Ícaro: \"A montanha inteira tremendo e eu preocupado com uma lampadinha. Tá bom.\""),
            new LocalizationEntry(LineLavine, "Lavine: \"Grandma Edna would cup her hands round it. Like this.\"",
                "Lavine: \"A Vó Edna faria uma conchinha com as mãos em volta. Assim.\""),
        };

        /// <summary>The cousin's line the first time a lamp is held through a tremor in a visit.</summary>
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
                if (!manager.HasKey(Warning))
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

        public static string Format(LocalizationManager manager, string key, params object[] arguments) => string.Format(CultureInfo.InvariantCulture, Get(manager, key), arguments);
    }
}
