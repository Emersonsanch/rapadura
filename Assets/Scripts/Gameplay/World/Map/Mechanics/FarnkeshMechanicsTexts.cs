using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the Picos de Farnkesh mechanics. Same self-contained pattern as <c>OverworldTexts</c>.</summary>
    public static class FarnkeshMechanicsTexts
    {
        public const string GustWarning = "ui.farnkesh.gust_warning";
        public const string GustHit = "ui.farnkesh.gust_hit";
        public const string BasketUp = "ui.farnkesh.basket_up";
        public const string BasketDown = "ui.farnkesh.basket_down";
        public const string GlideReady = "ui.farnkesh.glide_ready";
        public const string NeedsWings = "ui.farnkesh.needs_wings";
        public const string CurrentResting = "ui.farnkesh.current_resting";
        public const string RouteBlocked = "ui.farnkesh.route_blocked";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(GustWarning, "The wind is coming: stop and hold on!", "O vento vem aí: pare e segure!"),
            new LocalizationEntry(GustHit, "Gust!", "Rajada!"),
            new LocalizationEntry(BasketUp, "[E] Ride the rope basket up", "[E] Subir no cesto de corda"),
            new LocalizationEntry(BasketDown, "[E] Ride the rope basket down", "[E] Descer no cesto de corda"),
            new LocalizationEntry(GlideReady, "[E] Glide on the current", "[E] Planar na corrente"),
            new LocalizationEntry(NeedsWings, "Only wings or a flying mount ride this current", "Só asas ou montaria voadora pegam esta corrente"),
            new LocalizationEntry(CurrentResting, "The current is resting: it returns next hour", "A corrente descansa: volta na próxima hora"),
            new LocalizationEntry(RouteBlocked, "The way at the other end is closed to you", "O caminho do outro lado ainda está fechado para você"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(GustWarning))
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
