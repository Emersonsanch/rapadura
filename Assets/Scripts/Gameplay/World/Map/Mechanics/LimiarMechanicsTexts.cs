using System.Collections.Generic;
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
    }
}
