using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the silence of the Ermidas Suspensas (56). Same self-contained pattern as <see cref="RioRaftTexts"/>.</summary>
    public static class FarnkeshSilenceTexts
    {
        public const string Enter = "ui.farnkesh_silence.enter";
        public const string EnterLavine = "ui.farnkesh_silence.enter_lavine";
        public const string WarnHalf = "ui.farnkesh_silence.warn_half";
        public const string WarnNear = "ui.farnkesh_silence.warn_near";
        public const string EchoMarks = "ui.farnkesh_silence.echo_marks";
        public const string EchoHits = "ui.farnkesh_silence.echo_hits";
        public const string EchoMissed = "ui.farnkesh_silence.echo_missed";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(Enter, "The hermitages listen: running and fighting wake the Echoes; walk and the noise fades",
                "As ermidas escutam: correr e lutar acordam os Ecos; ande e o barulho some"),
            new LocalizationEntry(EnterLavine, "Lavine: \"Shh. I know how to walk without being heard.\"", "Lavine: \"Psiu. Eu sei andar sem ninguém ouvir.\""),
            new LocalizationEntry(WarnHalf, "The cliff is listening to your steps", "O paredão está ouvindo seus passos"),
            new LocalizationEntry(WarnNear, "Too much noise: an Echo is about to answer", "Barulho demais: um Eco vai responder"),
            new LocalizationEntry(EchoMarks, "An Echo answers! Step out of the ring", "Um Eco responde! Saia do círculo"),
            new LocalizationEntry(EchoHits, "The Echo throws your own noise back at you", "O Eco devolve o seu próprio barulho"),
            new LocalizationEntry(EchoMissed, "The Echo strikes the empty stone and fades", "O Eco bate na pedra vazia e some"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(Enter))
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
