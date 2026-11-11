using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the cart of the Calhas de Pedra (37). Same self-contained pattern as <see cref="RioRaftTexts"/>.</summary>
    public static class TetoCartTexts
    {
        public const string StationRide = "ui.teto_cart.station_ride";
        public const string StationRecord = "ui.teto_cart.station_record";
        public const string StationAvalanche = "ui.teto_cart.station_avalanche";
        public const string StationBlocked = "ui.teto_cart.station_blocked";
        public const string StationResting = "ui.teto_cart.station_resting";
        public const string Started = "ui.teto_cart.started";
        public const string Spark = "ui.teto_cart.spark";
        public const string Knock = "ui.teto_cart.knock";
        public const string Finished = "ui.teto_cart.finished";
        public const string NewRecord = "ui.teto_cart.new_record";
        public const string Paid = "ui.teto_cart.paid";
        public const string BagFull = "ui.teto_cart.bag_full";
        public const string Broken = "ui.teto_cart.broken";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(StationRide, "[E] Ride the Artificers' cart down the chutes (steer left and right for the sparks)",
                "[E] Descer as calhas no carrinho dos Artífices (vire para os lados para pegar as faíscas)"),
            new LocalizationEntry(StationRecord, "Your best descent: {0} s", "Sua melhor descida: {0} s"),
            new LocalizationEntry(StationAvalanche, "The bell will ring over the chute: the cart waits for the snow to pass ({0} s)",
                "O sino vai tocar sobre a calha: o carrinho espera a neve passar ({0} s)"),
            new LocalizationEntry(StationBlocked, "The chutes run through slopes still closed to you", "As calhas passam por encostas ainda fechadas para você"),
            new LocalizationEntry(StationResting, "The Artificers are pulling the next cart up", "Os Artífices estão subindo o próximo carrinho"),
            new LocalizationEntry(Started, "The cart tips over the edge and the chute takes it!", "O carrinho passa da borda e a calha o leva!"),
            new LocalizationEntry(Spark, "Spark! ({0}/{1})", "Faísca! ({0}/{1})"),
            new LocalizationEntry(Knock, "The cart knocks a beast off the chute", "O carrinho derruba um bicho da calha"),
            new LocalizationEntry(Finished, "Down in {0} s with {1} spark(s): score {2} s", "Desceu em {0} s com {1} faísca(s): marca {2} s"),
            new LocalizationEntry(NewRecord, "A new best descent!", "Nova melhor descida!"),
            new LocalizationEntry(Paid, "The Artificers pay {0} nuggets for today's sparks", "Os Artífices pagam {0} pepitas pelas faíscas de hoje"),
            new LocalizationEntry(BagFull, "Your bag is full: the nuggets wait for your next descent today", "Mochila cheia: as pepitas esperam a próxima descida de hoje"),
            new LocalizationEntry(Broken, "You jump off the cart", "Você salta do carrinho"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(StationRide))
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
