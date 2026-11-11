using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the rafts of the Reed Fields (72). Same self-contained pattern as <see cref="ProfundoPathsTexts"/>.</summary>
    public static class RioRaftTexts
    {
        public const string ReedReady = "ui.rio_raft.reed_ready";
        public const string ReedGrowing = "ui.rio_raft.reed_growing";
        public const string ReedCut = "ui.rio_raft.reed_cut";
        public const string ReedEnough = "ui.rio_raft.reed_enough";
        public const string LandingRide = "ui.rio_raft.landing_ride";
        public const string LandingBuild = "ui.rio_raft.landing_build";
        public const string LandingNeed = "ui.rio_raft.landing_need";
        public const string LandingHippo = "ui.rio_raft.landing_hippo";
        public const string LandingBlocked = "ui.rio_raft.landing_blocked";
        public const string LandingResting = "ui.rio_raft.landing_resting";
        public const string Built = "ui.rio_raft.built";
        public const string Sailing = "ui.rio_raft.sailing";
        public const string HippoSwims = "ui.rio_raft.hippo_swims";
        public const string Bitten = "ui.rio_raft.bitten";
        public const string Splash = "ui.rio_raft.splash";
        public const string Landed = "ui.rio_raft.landed";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(ReedReady, "[E] Cut a bundle of reed ({0}/{1} for a raft)", "[E] Cortar um feixe de junco ({0}/{1} para uma jangada)"),
            new LocalizationEntry(ReedGrowing, "This reed was cut: it grows back in {0} min", "Este junco foi cortado: cresce de novo em {0} min"),
            new LocalizationEntry(ReedCut, "A bundle of reed ({0}/{1})", "Um feixe de junco ({0}/{1})"),
            new LocalizationEntry(ReedEnough, "Enough reed for a raft: tie it at a landing at the end of a canal", "Junco bastante para uma jangada: amarre-a num embarcadouro na ponta de um canal"),
            new LocalizationEntry(LandingRide, "[E] Sail the raft down the long canal ({0} s left on the raft)", "[E] Descer o canal comprido de jangada (a jangada aguenta mais {0} s)"),
            new LocalizationEntry(LandingBuild, "[E] Tie a raft of reed and sail the long canal", "[E] Amarrar uma jangada de junco e descer o canal comprido"),
            new LocalizationEntry(LandingNeed, "A raft takes {1} bundles of reed (you have {0}): cut them on the banks of the canals",
                "Uma jangada leva {1} feixes de junco (você tem {0}): corte nas margens dos canais"),
            new LocalizationEntry(LandingHippo, "[E] Swim the long canal on Taweret's hippo", "[E] Nadar o canal comprido com a Hipopotinha de Taweret"),
            new LocalizationEntry(LandingBlocked, "The canal runs through fields still closed to you", "O canal passa por campos ainda fechados para você"),
            new LocalizationEntry(LandingResting, "The water is still settling at the landing", "A água ainda está assentando no embarcadouro"),
            new LocalizationEntry(Built, "You tie the reed into a raft: it holds for 2 minutes", "Você amarra o junco numa jangada: ela aguenta 2 minutos"),
            new LocalizationEntry(Sailing, "The raft slides down the canal through the fields", "A jangada desliza pelo canal entre os campos"),
            new LocalizationEntry(HippoSwims, "Taweret's hippo swims the canal; the mud hippos let her pass", "A Hipopotinha de Taweret nada o canal; os hipopótamos de lodo deixam passar"),
            new LocalizationEntry(Bitten, "A mud hippo snaps at the raft", "Um hipopótamo de lodo morde a jangada"),
            new LocalizationEntry(Splash, "The raft lands with a splash that knocks the beasts on the bank!", "A jangada encosta com um tranco que derruba os bichos da margem!"),
            new LocalizationEntry(Landed, "The raft comes to the far landing", "A jangada chega ao outro embarcadouro"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(ReedReady))
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
