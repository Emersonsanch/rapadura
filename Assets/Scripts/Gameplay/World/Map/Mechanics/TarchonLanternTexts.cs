using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of Despertar o vizinho and the swamp night of the Palafitas de Tarchon (33, <see cref="TarchonLanternRules"/>):
    /// the mast's lantern, the five dimmed trols (Arnth, Velia, Laris, Ravnthu and Fasti, Etruscan names of the inscriptions of Tarquinia and
    /// Cerveteri) waking with their own line, one line per cousin on the day's first wake, and Thanchvil's purse for the five. Same
    /// self-contained pattern as <see cref="QochoOasisTexts"/>.
    /// </summary>
    public static class TarchonLanternTexts
    {
        public const string MastPrompt = "ui.tarchon_lantern.mast_prompt";
        public const string LanternLit = "ui.tarchon_lantern.lit";
        public const string LanternOut = "ui.tarchon_lantern.out";
        public const string NeighbourPrompt = "ui.tarchon_lantern.neighbour_prompt";
        public const string NoLantern = "ui.tarchon_lantern.no_lantern";
        public const string AlreadyAwake = "ui.tarchon_lantern.already_awake";
        public const string Woke = "ui.tarchon_lantern.woke";
        public const string AllAwake = "ui.tarchon_lantern.all_awake";
        public const string NightMist = "ui.tarchon_lantern.night_mist";
        public const string LanternSight = "ui.tarchon_lantern.lantern_sight";

        public const string Arnth = "ui.tarchon_lantern.arnth";
        public const string Velia = "ui.tarchon_lantern.velia";
        public const string Laris = "ui.tarchon_lantern.laris";
        public const string Ravnthu = "ui.tarchon_lantern.ravnthu";
        public const string Fasti = "ui.tarchon_lantern.fasti";

        public const string LineMaithe = "ui.tarchon_lantern.maithe";
        public const string LineIcaro = "ui.tarchon_lantern.icaro";
        public const string LineJoaquim = "ui.tarchon_lantern.joaquim";
        public const string LineMaria = "ui.tarchon_lantern.maria";
        public const string LineLavine = "ui.tarchon_lantern.lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(MastPrompt, "[E] Light a lantern at the Mast of the Marsh Lights", "[E] Acender uma lanterna no Mastro dos Fogos-Fátuos"),
            new LocalizationEntry(LanternLit, "A lit lantern in your hands: take it to a dimmed neighbour before it fades", "Uma lanterna acesa nas mãos: leve-a a um vizinho apagado antes que ela se apague"),
            new LocalizationEntry(LanternOut, "The lantern went out. Light another one at the mast", "A lanterna se apagou. Acenda outra no mastro"),
            new LocalizationEntry(NeighbourPrompt, "[E] Hand the lit lantern to the dimmed neighbour", "[E] Entregar a lanterna acesa ao vizinho apagado"),
            new LocalizationEntry(NoLantern, "This trol stares at nothing. A lit lantern from the mast might wake him", "Este trol olha para o nada. Uma lanterna acesa do mastro pode acordá-lo"),
            new LocalizationEntry(AlreadyAwake, "Awake today, and smiling", "Acordado hoje, e sorrindo"),
            new LocalizationEntry(Woke, "A neighbour wakes ({0}/{1} today)", "Um vizinho acorda ({0}/{1} hoje)"),
            new LocalizationEntry(AllAwake, "Thanchvil: All five awake on one day! The stilts sound like a feast. Take this, for the boots you wore out on our planks.", "Thanchvil: Os cinco acordados no mesmo dia! As palafitas parecem dia de festa. Leve isto, pelas botas que você gastou nas nossas tábuas."),
            new LocalizationEntry(NightMist, "Night over the swamp: the lilac mist rises and you can't see far. A lit lantern clears it", "Noite no pântano: a névoa lilás sobe e não se vê longe. Uma lanterna acesa clareia"),
            new LocalizationEntry(LanternSight, "The lantern pushes the mist back", "A lanterna empurra a névoa para longe"),

            new LocalizationEntry(Arnth, "Arnth: ...I was counting wisps. How many nights? My nets must be rotten by now.", "Arnth: ...eu estava contando fogos-fátuos. Quantas noites? Minhas redes já devem ter apodrecido."),
            new LocalizationEntry(Velia, "Velia: Oh! My basket is still half woven. Thank you, bringer of light.", "Velia: Ah! Meu cesto ainda está pela metade. Obrigada, você que trouxe a luz."),
            new LocalizationEntry(Laris, "Laris: The wisps called me, but they never answered. Your lantern did.", "Laris: Os fogos-fátuos me chamavam, mas nunca respondiam. A sua lanterna respondeu."),
            new LocalizationEntry(Ravnthu, "Ravnthu: I had forgotten the slow song of the stilts. Listen, it is coming back.", "Ravnthu: Eu tinha esquecido a canção lenta das palafitas. Escuta, ela está voltando."),
            new LocalizationEntry(Fasti, "Fasti: Warm! It's warm! Is it morning already?", "Fasti: Quentinha! Está quentinha! Já é de manhã?"),

            new LocalizationEntry(LineMaithe, "Maithe: Vó Edna does this too: a lamp in the window so nobody gets lost.", "Maithe: A Vó Edna faz isso também: uma lamparina na janela para ninguém se perder."),
            new LocalizationEntry(LineIcaro, "Ícaro: They weren't sick, just stuck staring. A real light breaks the loop.", "Ícaro: Eles não estavam doentes, só presos olhando. Uma luz de verdade quebra o laço."),
            new LocalizationEntry(LineJoaquim, "Joaquim: Vô Jorge woke me like that on cold mornings, with a lamp and a joke.", "Joaquim: O Vô Jorge me acordava assim nas manhãs frias, com uma lamparina e uma piada."),
            new LocalizationEntry(LineMaria, "Maria: Wake up, sleepyhead! There's a whole swamp out here!", "Maria: Acorda, dorminhoco! Tem um pântano inteiro aqui fora!"),
            new LocalizationEntry(LineLavine, "Lavine: Vó Edna says a neighbour who forgets needs someone who remembers for them.", "Lavine: A Vó Edna diz que vizinho que esquece precisa de alguém que lembre por ele."),
        };

        /// <summary>The woken neighbour's own line (by the ids of <see cref="Overworld.OverworldLayouts.TarchonNeighbours"/>).</summary>
        public static string NeighbourLine(string id)
        {
            switch (id)
            {
                case "arnth": return Arnth;
                case "velia": return Velia;
                case "laris": return Laris;
                case "ravnthu": return Ravnthu;
                default: return Fasti;
            }
        }

        /// <summary>The cousin's line on the day's first wake (Joaquim's when no cousin is known).</summary>
        public static string LineFor(CharacterId? hero)
        {
            switch (hero)
            {
                case CharacterId.Maithe: return LineMaithe;
                case CharacterId.Icaro: return LineIcaro;
                case CharacterId.Maria: return LineMaria;
                case CharacterId.Lavine: return LineLavine;
                default: return LineJoaquim;
            }
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(MastPrompt))
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
