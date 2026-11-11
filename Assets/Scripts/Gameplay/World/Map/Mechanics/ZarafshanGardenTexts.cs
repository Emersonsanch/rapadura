using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the sparks and canals of the Jardim de Mihrshah (06). Same self-contained pattern as <see cref="RioRaftTexts"/>.</summary>
    public static class ZarafshanGardenTexts
    {
        public const string CageFree = "ui.zarafshan_garden.cage_free";
        public const string CageGuarded = "ui.zarafshan_garden.cage_guarded";
        public const string CageDone = "ui.zarafshan_garden.cage_done";
        public const string Freed = "ui.zarafshan_garden.freed";
        public const string AllFreed = "ui.zarafshan_garden.all_freed";
        public const string CanalMends = "ui.zarafshan_garden.canal_mends";
        public const string CanalWaits = "ui.zarafshan_garden.canal_waits";
        public const string PavilionMends = "ui.zarafshan_garden.pavilion_mends";
        public const string LineJoaquim = "ui.zarafshan_garden.line.joaquim";
        public const string LineMaria = "ui.zarafshan_garden.line.maria";
        public const string LineMaithe = "ui.zarafshan_garden.line.maithe";
        public const string LineIcaro = "ui.zarafshan_garden.line.icaro";
        public const string LineLavine = "ui.zarafshan_garden.line.lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(CageFree, "[E] Free the spark caught in the roses", "[E] Libertar a faísca presa nas roseiras"),
            new LocalizationEntry(CageGuarded, "A spark is caught here, but {0} beast(s) still guard it", "Uma faísca está presa aqui, mas {0} bicho(s) ainda a guardam"),
            new LocalizationEntry(CageDone, "This spark is free: its canal runs with Living Water", "Esta faísca está livre: o canal dela corre com Água Viva"),
            new LocalizationEntry(Freed, "The spark rises and Living Water fills a canal ({0}/{1})", "A faísca sobe e a Água Viva enche um canal ({0}/{1})"),
            new LocalizationEntry(AllFreed, "The four canals run again: the pavilion of Mihrshah is open", "Os quatro canais correm de novo: o pavilhão de Mihrshah está aberto"),
            new LocalizationEntry(CanalMends, "The lit water mends you while you rest beside it", "A água acesa cura você enquanto descansa ao lado dela"),
            new LocalizationEntry(CanalWaits, "The lit water mends only out of a fight", "A água acesa só cura fora da luta"),
            new LocalizationEntry(PavilionMends, "Under the pavilion the light mends body and mind", "Debaixo do pavilhão a luz cura o corpo e a cabeça"),
            new LocalizationEntry(LineJoaquim, "Joaquim: \"Old Mihrshah saw this garden lit when he was a boy. Now he can see it again.\"",
                "Joaquim: \"O velho Mihrshah viu este jardim aceso quando era menino. Agora pode ver de novo.\""),
            new LocalizationEntry(LineMaria, "Maria: \"Four sparks, four canals. Grandma would say: water that is shared does not run out.\"",
                "Maria: \"Quatro faíscas, quatro canais. A Vó diria: água repartida não acaba.\""),
            new LocalizationEntry(LineMaithe, "Maithe: \"Listen, Ícaro: the fountains are singing.\"", "Maithe: \"Escuta, Ícaro: as fontes estão cantando.\""),
            new LocalizationEntry(LineIcaro, "Ícaro: \"I'm drinking from it. If it mends, it's good.\"", "Ícaro: \"Vou beber. Se cura, é bom.\""),
            new LocalizationEntry(LineLavine, "Lavine: \"The roses were holding them. They let go when the beasts did.\"",
                "Lavine: \"As roseiras é que seguravam. Soltaram quando os bichos soltaram.\""),
        };

        /// <summary>The cousin's line when the fourth spark is freed.</summary>
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
                if (!manager.HasKey(CageFree))
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
