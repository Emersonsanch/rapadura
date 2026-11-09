using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the Mar Gelado mechanics. Same self-contained pattern as <see cref="FarnkeshMechanicsTexts"/>.</summary>
    public static class MarGeladoMechanicsTexts
    {
        public const string CrossOnIce = "ui.margelado.cross_on_ice";
        public const string CrossByBoat = "ui.margelado.cross_by_boat";
        public const string NeedsBoat = "ui.margelado.needs_boat";
        public const string HeavyOnCanal = "ui.margelado.heavy_on_canal";
        public const string CrossingBlocked = "ui.margelado.crossing_blocked";
        public const string ThinIceAhead = "ui.margelado.thin_ice_ahead";
        public const string IceCreaks = "ui.margelado.ice_creaks";
        public const string IceBroke = "ui.margelado.ice_broke";
        public const string FishPrompt = "ui.margelado.fish_prompt";
        public const string FishDismount = "ui.margelado.fish_dismount";
        public const string FishResting = "ui.margelado.fish_resting";
        public const string FishLineDown = "ui.margelado.fish_line_down";
        public const string FishBite = "ui.margelado.fish_bite";
        public const string FishCaught = "ui.margelado.fish_caught";
        public const string FishBagFull = "ui.margelado.fish_bag_full";
        public const string FishScared = "ui.margelado.fish_scared";
        public const string FishLost = "ui.margelado.fish_lost";
        public const string FishCancelled = "ui.margelado.fish_cancelled";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(CrossOnIce, "[E] Cross the frozen canal", "[E] Atravessar o canal congelado"),
            new LocalizationEntry(CrossByBoat, "[E] Cross by boat", "[E] Atravessar de barco"),
            new LocalizationEntry(NeedsBoat, "The canal is open, only a boat crosses. It freezes in {0} days", "O canal está aberto, só barco atravessa. Congela em {0} dias"),
            new LocalizationEntry(HeavyOnCanal, "This ice will not bear a heavy mount", "Este gelo não aguenta montaria pesada"),
            new LocalizationEntry(CrossingBlocked, "The way at the other bank is closed to you", "O caminho da outra margem ainda está fechado para você"),
            new LocalizationEntry(ThinIceAhead, "Thin ice: too weak for this mount!", "Gelo fino: fraco demais para esta montaria!"),
            new LocalizationEntry(IceCreaks, "The ice is creaking: get off it!", "O gelo está rangendo: saia daí!"),
            new LocalizationEntry(IceBroke, "The ice broke and your mount bolted!", "O gelo quebrou e a montaria fugiu!"),
            new LocalizationEntry(FishPrompt, "[E] Fish through the ice", "[E] Pescar no gelo"),
            new LocalizationEntry(FishDismount, "Get off your mount to fish", "Desça da montaria para pescar"),
            new LocalizationEntry(FishResting, "The fish here are wary: wait a little", "Os peixes daqui estão ariscos: espere um pouco"),
            new LocalizationEntry(FishLineDown, "Line down... wait for the pull", "Linha na água... espere puxar"),
            new LocalizationEntry(FishBite, "It's pulling! [E]", "Puxou! [E]"),
            new LocalizationEntry(FishCaught, "Caught: {0}!", "Pescou: {0}!"),
            new LocalizationEntry(FishBagFull, "Caught: {0}, but the bag is full", "Pescou: {0}, mas a mochila está cheia"),
            new LocalizationEntry(FishScared, "Too soon: the fish fled", "Cedo demais: o peixe fugiu"),
            new LocalizationEntry(FishLost, "Too late: the fish got away", "Tarde demais: o peixe escapou"),
            new LocalizationEntry(FishCancelled, "You wound the line back up", "Você recolheu a linha"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(CrossOnIce))
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

        public static string Format(LocalizationManager manager, string key, object argument) => string.Format(CultureInfo.InvariantCulture, Get(manager, key), argument);
    }
}
