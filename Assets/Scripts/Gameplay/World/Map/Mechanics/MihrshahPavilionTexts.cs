using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the pavilion, the altar's Pétala de Luz and the night of the Jardim de Mihrshah (06, <see cref="ZarafshanPavilionRules"/>).</summary>
    public static class MihrshahPavilionTexts
    {
        public const string DoorSealed = "ui.mihrshah_pavilion.door_sealed";
        public const string DoorPrompt = "ui.mihrshah_pavilion.door_prompt";
        public const string DoorOpened = "ui.mihrshah_pavilion.door_opened";
        public const string AltarPrompt = "ui.mihrshah_pavilion.altar_prompt";
        public const string AltarGave = "ui.mihrshah_pavilion.altar_gave";
        public const string AltarToday = "ui.mihrshah_pavilion.altar_today";
        public const string BagFull = "ui.mihrshah_pavilion.bag_full";
        public const string DoorShutMends = "ui.mihrshah_pavilion.door_shut_mends";
        public const string NightFalls = "ui.mihrshah_pavilion.night_falls";
        public const string PetalLight = "ui.mihrshah_pavilion.petal_light";
        public const string PetalFades = "ui.mihrshah_pavilion.petal_fades";
        public const string LineJoaquim = "ui.mihrshah_pavilion.line.joaquim";
        public const string LineMaria = "ui.mihrshah_pavilion.line.maria";
        public const string LineMaithe = "ui.mihrshah_pavilion.line.maithe";
        public const string LineIcaro = "ui.mihrshah_pavilion.line.icaro";
        public const string LineLavine = "ui.mihrshah_pavilion.line.lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(DoorSealed, "The pavilion's door is dark: {0}/4 sparks are free", "A porta do pavilhão está escura: {0}/4 faíscas livres"),
            new LocalizationEntry(DoorPrompt, "[E] Open the door of Mihrshah's pavilion", "[E] Abrir a porta do pavilhão de Mihrshah"),
            new LocalizationEntry(DoorOpened, "The door opens and the eight columns light up: {0} Petal(s) of Light fall on the altar",
                "A porta se abre e as oito colunas se acendem: {0} Pétala(s) de Luz caem no altar"),
            new LocalizationEntry(AltarPrompt, "[E] Pick today's Petal of Light from the altar", "[E] Colher a Pétala de Luz de hoje no altar"),
            new LocalizationEntry(AltarGave, "A Petal of Light, still warm from the sparks", "Uma Pétala de Luz, ainda morna das faíscas"),
            new LocalizationEntry(AltarToday, "The altar's rose gives one petal a day: come back tomorrow", "A rosa do altar dá uma pétala por dia: volte amanhã"),
            new LocalizationEntry(BagFull, "The bag is full: the petal waits on the altar", "A mochila está cheia: a pétala espera no altar"),
            new LocalizationEntry(DoorShutMends, "The four canals run, but the pavilion's door is still shut: open it on the west side",
                "Os quatro canais correm, mas a porta do pavilhão ainda está fechada: abra pelo lado oeste"),
            new LocalizationEntry(NightFalls, "Night in the Garden: away from the lit canals you cannot see far", "Noite no Jardim: longe dos canais acesos não se enxerga longe"),
            new LocalizationEntry(PetalLight, "The Petal of Light shines in your hands: the dark hides nothing for 3 minutes",
                "A Pétala de Luz brilha nas suas mãos: por 3 minutos o escuro não esconde nada"),
            new LocalizationEntry(PetalFades, "The petal's light fades", "A luz da pétala se apaga"),
            new LocalizationEntry(LineJoaquim, "Joaquim: \"Grandpa Jorge always said a garden only opens to whoever waters it. We watered it with four sparks.\"",
                "Joaquim: \"O Vô Jorge sempre disse que jardim só abre para quem rega. A gente regou com quatro faíscas.\""),
            new LocalizationEntry(LineMaria, "Maria: \"Light kept in a petal. Old Mihrshah will cry when he sees one.\"",
                "Maria: \"Luz guardada numa pétala. O velho Mihrshah vai chorar quando vir uma.\""),
            new LocalizationEntry(LineMaithe, "Maithe: \"Ícaro, don't eat them all. One a day, like Grandma Edna's apricots.\"",
                "Maithe: \"Ícaro, não come todas. Uma por dia, que nem os damascos da Vó Edna.\""),
            new LocalizationEntry(LineIcaro, "Ícaro: \"A door that opens by itself once you've done the work. I like this garden.\"",
                "Ícaro: \"Uma porta que abre sozinha depois do trabalho feito. Gostei desse jardim.\""),
            new LocalizationEntry(LineLavine, "Lavine: \"The roses aren't holding anything any more. They're giving.\"",
                "Lavine: \"As roseiras não seguram mais nada. Agora elas dão.\""),
        };

        /// <summary>The cousin's line when the door first opens.</summary>
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
                if (!manager.HasKey(DoorPrompt))
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
