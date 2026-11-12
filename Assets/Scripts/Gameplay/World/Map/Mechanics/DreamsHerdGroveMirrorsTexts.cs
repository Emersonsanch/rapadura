using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the dreams of the Fortim (49), the herding of the prairie (51), the slow grove (50) and the mirror clock of
    /// the Torres Brancas (42). Same self-contained pattern as <c>OverworldTexts</c>.
    /// </summary>
    public static class DreamsHerdGroveMirrorsTexts
    {
        public const string Dream = "ui.route3.dream";
        public const string DreamPrompt = "ui.route3.dream_prompt";
        public const string DreamFound = "ui.route3.dream_found";
        public const string HorsePrompt = "ui.route3.horse_prompt";
        public const string HorseFollows = "ui.route3.horse_follows";
        public const string HorseDelivered = "ui.route3.horse_delivered";
        public const string HorsesLost = "ui.route3.horses_lost";
        public const string GroveSlow = "ui.route3.grove_slow";
        public const string GroveLeft = "ui.route3.grove_left";
        public const string MirrorPrompt = "ui.route3.mirror_prompt";
        public const string MirrorLit = "ui.route3.mirror_lit";
        public const string MirrorVault = "ui.route3.mirror_vault";
        public const string MirrorTaken = "ui.route3.mirror_taken";
        public const string BagFull = "ui.route3.bag_full";

        /// <summary>Compass points, 0 north … 7 north-west.</summary>
        public static readonly string[] CompassKeys =
        {
            "ui.route3.dir_n", "ui.route3.dir_ne", "ui.route3.dir_e", "ui.route3.dir_se", "ui.route3.dir_s", "ui.route3.dir_sw", "ui.route3.dir_w", "ui.route3.dir_nw",
        };

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(Dream, "Virogdad: \"Last night I dreamt of a chest {0} m {1} of the fort, under a sleeping stone\"", "Virogdad: \"Esta noite sonhei com um baú a {0} m {1} do fortim, debaixo de uma pedra que dorme\""),
            new LocalizationEntry(DreamPrompt, "[E] Open the dream's chest", "[E] Abrir o baú do sonho"),
            new LocalizationEntry(DreamFound, "The dream was true: {0} gold nuggets", "O sonho era verdade: {0} pepitas de ouro"),
            new LocalizationEntry(HorsePrompt, "[E] Lead the loose horse", "[E] Levar o cavalo solto"),
            new LocalizationEntry(HorseFollows, "The horse follows you: take it to the Poço da Taça", "O cavalo te segue: leve até o Poço da Taça"),
            new LocalizationEntry(HorseDelivered, "Horse in the corral: +{0} herding point, {1} gold nuggets", "Cavalo no cercado: +{0} ponto de pastoreio, {1} pepitas de ouro"),
            new LocalizationEntry(HorsesLost, "The horses went back to graze", "Os cavalos voltaram a pastar"),
            new LocalizationEntry(GroveSlow, "Among the tree-folk time walks slowly", "Entre o povo-tronco o tempo anda devagar"),
            new LocalizationEntry(GroveLeft, "Time walks at its own pace again", "O tempo volta ao passo de sempre"),
            new LocalizationEntry(MirrorPrompt, "[E] Turn the mirror", "[E] Girar o espelho"),
            new LocalizationEntry(MirrorLit, "The light reaches {0} of 4 mirrors", "A luz chega a {0} de 4 espelhos"),
            new LocalizationEntry(MirrorVault, "The light goes all the way round: the spire's vault opens! {0}", "A luz dá a volta inteira: a sala da torre abre! {0}"),
            new LocalizationEntry(MirrorTaken, "The vault is empty until tomorrow", "A sala está vazia até amanhã"),
            new LocalizationEntry(BagFull, "Your bag is full", "Sua bolsa está cheia"),
            new LocalizationEntry(CompassKeys[0], "north", "ao norte"),
            new LocalizationEntry(CompassKeys[1], "north-east", "a nordeste"),
            new LocalizationEntry(CompassKeys[2], "east", "a leste"),
            new LocalizationEntry(CompassKeys[3], "south-east", "a sudeste"),
            new LocalizationEntry(CompassKeys[4], "south", "ao sul"),
            new LocalizationEntry(CompassKeys[5], "south-west", "a sudoeste"),
            new LocalizationEntry(CompassKeys[6], "west", "a oeste"),
            new LocalizationEntry(CompassKeys[7], "north-west", "a noroeste"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(Dream))
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
