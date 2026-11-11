using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the Grutas do Limiar lamps. Same self-contained pattern as <c>OverworldTexts</c>.</summary>
    public static class LimiarMechanicsTexts
    {
        public const string LightPrompt = "ui.limiar.light_prompt";
        public const string Lit = "ui.limiar.lit";
        public const string BackPrompt = "ui.limiar.back_prompt";
        public const string DeepPrompt = "ui.limiar.deep_prompt";
        public const string Resting = "ui.limiar.resting";
        public const string Closed = "ui.limiar.closed";
        public const string WellReady = "ui.limiar.well_ready";
        public const string WellNeedsWings = "ui.limiar.well_needs_wings";
        public const string WellBlocked = "ui.limiar.well_blocked";
        public const string WellResting = "ui.limiar.well_resting";
        public const string WellCalm = "ui.limiar.well_calm";
        public const string WellLift = "ui.limiar.well_lift";
        public const string WellPecked = "ui.limiar.well_pecked";
        public const string WellGust = "ui.limiar.well_gust";
        public const string WellLanded = "ui.limiar.well_landed";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(LightPrompt, "[E] Light the lamp", "[E] Acender a lâmpada"),
            new LocalizationEntry(Lit, "The lamp stays lit: it will remember your way", "A lâmpada fica acesa: ela vai lembrar do seu caminho"),
            new LocalizationEntry(BackPrompt, "[E] Follow the lamps back to the Vila", "[E] Seguir as lâmpadas de volta à Vila"),
            new LocalizationEntry(DeepPrompt, "[E] Follow the lamps to the deepest one lit", "[E] Seguir as lâmpadas até a mais funda acesa"),
            new LocalizationEntry(Resting, "The lamps are catching their breath", "As lâmpadas estão tomando fôlego"),
            new LocalizationEntry(Closed, "The way to that lamp is still closed to you", "O caminho até aquela lâmpada ainda está fechado para você"),
            new LocalizationEntry(WellReady, "[E] Open your wings over the wind well", "[E] Abrir as asas sobre o poço de vento"),
            new LocalizationEntry(WellNeedsWings, "The wind rises here, but only wings can ride it. The long way is the rope bridge",
                "O vento sobe aqui, mas só asas pegam a corrente. O caminho longo é a ponte de corda"),
            new LocalizationEntry(WellBlocked, "The far rim is still closed to you", "A outra borda ainda está fechada para você"),
            new LocalizationEntry(WellResting, "Catch your breath before the next glide", "Tome fôlego antes de planar de novo"),
            new LocalizationEntry(WellCalm, "The well is holding its breath: wind again in {0} s", "O poço prendeu o fôlego: vento de novo em {0} s"),
            new LocalizationEntry(WellLift, "The updraft lifts you over the chasm!", "A corrente te ergue por cima do abismo!"),
            new LocalizationEntry(WellPecked, "Mahaway's flock pecks at whoever hangs in the air!", "A revoada de Mahaway bica quem fica no ar!"),
            new LocalizationEntry(WellGust, "The gust of your landing knocks the beasts back!", "A rajada do pouso empurra os bichos!"),
            new LocalizationEntry(WellLanded, "You land on the far rim", "Você pousa na outra borda"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(LightPrompt))
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
