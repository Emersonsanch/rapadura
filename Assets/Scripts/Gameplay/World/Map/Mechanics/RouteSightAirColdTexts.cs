using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the fog of Garajonay (05), the still air of the Mercado dos Túneis (22) and the cold of the glacier (38).
    /// Same self-contained pattern as <c>OverworldTexts</c>.
    /// </summary>
    public static class RouteSightAirColdTexts
    {
        public const string FogRising = "ui.route.fog_rising";
        public const string FogUp = "ui.route.fog_up";
        public const string FogAbove = "ui.route.fog_above";
        public const string FogCrossed = "ui.route.fog_crossed";
        public const string FogCollector = "ui.route.fog_collector";
        public const string FogCollectorFull = "ui.route.fog_collector_full";
        public const string StillAir = "ui.route.still_air";
        public const string StillAirBottle = "ui.route.still_air_bottle";
        public const string ColdChilly = "ui.route.cold_chilly";
        public const string ColdFrozen = "ui.route.cold_frozen";
        public const string ColdWarmed = "ui.route.cold_warmed";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(FogRising, "The fog is rising from the streams: climb a platform or stay close", "A névoa está subindo dos córregos: suba numa plataforma ou fique perto"),
            new LocalizationEntry(FogUp, "Fog! You can't see past 8 m down here", "Névoa! Aqui embaixo não dá para ver além de 8 m"),
            new LocalizationEntry(FogAbove, "Above the fog: the canopy sees far", "Acima da névoa: das copas se vê longe"),
            new LocalizationEntry(FogCrossed, "Fog crossed: the laurels left a drop for the Mist Collector", "Névoa atravessada: os louros deixaram uma gota para o Coletor de Névoa"),
            new LocalizationEntry(FogCollector, "The Mist Collector gives you {0} ×{1}", "O Coletor de Névoa te dá {0} ×{1}"),
            new LocalizationEntry(FogCollectorFull, "The Mist Collector keeps your drops: your bag is full", "O Coletor de Névoa guarda suas gotas: a bolsa está cheia"),
            new LocalizationEntry(StillAir, "Still air far from the pipes: your breath won't come back. A Bottle of Pure Air helps", "Ar parado longe dos tubos: o fôlego não volta. Uma Garrafa de Ar Sereno resolve"),
            new LocalizationEntry(StillAirBottle, "The bottle's air: you breathe easy", "O ar da garrafa: dá para respirar"),
            new LocalizationEntry(ColdChilly, "The glacier's cold is getting in: find a fire", "O frio da geleira está entrando: procure uma fogueira"),
            new LocalizationEntry(ColdFrozen, "Frozen stiff: you walk slower until you warm up", "Congelado: você anda mais devagar até se esquentar"),
            new LocalizationEntry(ColdWarmed, "Warm again by the fire", "Quentinho de novo na fogueira"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(FogRising))
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
