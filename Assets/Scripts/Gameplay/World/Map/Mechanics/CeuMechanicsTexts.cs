using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the Reinos do Céu mechanics. Same self-contained pattern as <see cref="RioMechanicsTexts"/>.</summary>
    public static class CeuMechanicsTexts
    {
        public const string BarqueBoard = "ui.ceu.barque_board";
        public const string BarqueNotNight = "ui.ceu.barque_not_night";
        public const string BarqueBlocked = "ui.ceu.barque_blocked";
        public const string BarqueReturn = "ui.ceu.barque_return";
        public const string BarqueBoarding = "ui.ceu.barque_boarding";
        public const string BarqueArrives = "ui.ceu.barque_arrives";
        public const string CrackRaise = "ui.ceu.crack_raise";
        public const string CrackSealed = "ui.ceu.crack_sealed";
        public const string CrackHold = "ui.ceu.crack_hold";
        public const string DripWarning = "ui.ceu.drip_warning";
        public const string DripHit = "ui.ceu.drip_hit";
        public const string PillarFaded = "ui.ceu.pillar_faded";
        public const string SkyWhole = "ui.ceu.sky_whole";
        public const string PorchSit = "ui.ceu.porch_sit";
        public const string PorchEnd = "ui.ceu.porch_end";
        public const string PorchBroken = "ui.ceu.porch_broken";
        public const string PorchNightCoda = "ui.ceu.porch_night_coda";

        /// <summary>What the cousins say as each crack closes, indexed by crack.</summary>
        public static readonly string[] SealLines =
        {
            "ui.ceu.seal.maithe", "ui.ceu.seal.icaro", "ui.ceu.seal.joaquim", "ui.ceu.seal.maria", "ui.ceu.seal.lavine",
        };

        private const int StoryCount = CeuPorchRules.Stories;
        private const int LinesPerStory = CeuPorchRules.LinesPerStory;

        /// <summary>The key of line <paramref name="line"/> of story <paramref name="story"/>.</summary>
        public static string StoryLine(int story, int line) => "ui.ceu.story." + story + "." + line;

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(BarqueBoard, "[E] Board the Moon Ship", "[E] Embarcar na Barca da Lua"),
            new LocalizationEntry(BarqueNotNight, "Lavine: the Moon Ship only comes at night. {0} h to go", "Lavine: a Barca da Lua só encosta à noite. Faltam {0} h"),
            new LocalizationEntry(BarqueBlocked, "The deck of the Moon Ship is still closed to you", "O convés da Barca da Lua ainda está fechado para você"),
            new LocalizationEntry(BarqueReturn, "[E] Back down to the Moon Pier", "[E] Descer de volta ao Píer da Lua"),
            new LocalizationEntry(BarqueBoarding, "The silver ship lifts off the pier...", "A barca de prata se solta do píer..."),
            new LocalizationEntry(BarqueArrives, "The Moon Ship has come in at the pier", "A Barca da Lua encostou no píer"),
            new LocalizationEntry(CrackRaise, "[E] Raise a pillar of light and hold the sky", "[E] Erguer um pilar de luz e segurar o céu"),
            new LocalizationEntry(CrackSealed, "This crack is sealed for now", "Esta rachadura está selada por enquanto"),
            new LocalizationEntry(CrackHold, "Stay close to hold the pillar up. You can still fight", "Fique perto para sustentar o pilar. Dá para lutar"),
            new LocalizationEntry(DripWarning, "The shadow is about to drip! Stay and bear it, or step out", "A sombra vai pingar! Aguente ou saia de perto"),
            new LocalizationEntry(DripHit, "Shadow drips on you", "A sombra pingou em você"),
            new LocalizationEntry(PillarFaded, "The pillar went out. The crack is open again", "O pilar apagou. A rachadura continua aberta"),
            new LocalizationEntry(SkyWhole, "Grandma Edna, on the radio: the sky is whole. Now come have dinner", "Vó Edna, no rádio: o céu está inteiro. Agora vem jantar"),
            new LocalizationEntry(PorchSit, "[E] Sit beside Grandpa Jorge's chair", "[E] Sentar ao lado da cadeira do Vô Jorge"),
            new LocalizationEntry(PorchEnd, "The chair keeps rocking, all by itself", "A cadeira continua balançando, sozinha"),
            new LocalizationEntry(PorchBroken, "The story waits for you. Come back and sit", "A história espera. Volte e sente"),
            new LocalizationEntry(PorchNightCoda, "Far below, the lanterns of the 27 cities light up, and one spark rises slowly", "Lá embaixo, as lanternas das 27 cidades acendem, e uma faísca sobe devagar"),

            new LocalizationEntry(SealLines[0], "Maithe: it held! Breathe, we still have sky to mend", "Maithe: segurou! Respira, que ainda tem céu para remendar"),
            new LocalizationEntry(SealLines[1], "Ícaro: mending the sky. Grandpa would love this one", "Ícaro: remendar o céu. O Vô ia adorar contar essa"),
            new LocalizationEntry(SealLines[2], "Joaquim: one more. My arms hurt just from watching", "Joaquim: mais uma. Meu braço dói só de olhar"),
            new LocalizationEntry(SealLines[3], "Maria: careful with the next one, it is dripping more", "Maria: cuidado com a próxima, ela pinga mais"),
            new LocalizationEntry(SealLines[4], "Lavine: told you the moon was a shoe. The sky is just a patched sock", "Lavine: eu disse que a lua era um sapato. O céu é uma meia remendada"),

            new LocalizationEntry(StoryLine(0, 0), "Grandma Edna: he sat here and said every story starts with someone arriving", "Vó Edna: ele sentava aqui e dizia que toda história começa com alguém chegando"),
            new LocalizationEntry(StoryLine(0, 1), "Maithe: he told of the First Traveller as if he were a neighbour", "Maithe: ele falava do Primeiro Viajante como se fosse um vizinho"),
            new LocalizationEntry(StoryLine(0, 2), "Ícaro: and he always stopped at the best part, so we would come back", "Ícaro: e sempre parava na melhor parte, para a gente voltar amanhã"),
            new LocalizationEntry(StoryLine(0, 3), "Grandpa Jorge, remembered: when the light calls, we answer", "Vô Jorge, de memória: quando a luz chama, a gente responde"),

            new LocalizationEntry(StoryLine(1, 0), "Joaquim: he played the ney so out of tune that Ari howled along", "Joaquim: ele tocava a flauta ney tão desafinada que o Ari uivava junto"),
            new LocalizationEntry(StoryLine(1, 1), "Maria: Grandma laughed and pretended not to", "Maria: a Vó ria e fingia que não"),
            new LocalizationEntry(StoryLine(1, 2), "Grandma Edna: pretended nothing. It was the loveliest sound in the village", "Vó Edna: fingia nada. Era o som mais bonito da vila"),
            new LocalizationEntry(StoryLine(1, 3), "Ari lies down beside the chair and sighs", "O Ari se deita do lado da cadeira e suspira"),

            new LocalizationEntry(StoryLine(2, 0), "Lavine: Ammo says Grandpa gave the best spot at the fire to whoever came last", "Lavine: o Ammo diz que o Vô dava o melhor lugar da fogueira para quem chegava por último"),
            new LocalizationEntry(StoryLine(2, 1), "Ícaro: and he hid keepsakes across the whole map, so we would have to walk", "Ícaro: e escondeu lembranças pelo mapa inteiro, para a gente ter que andar"),
            new LocalizationEntry(StoryLine(2, 2), "Maria: we walked, Grandpa", "Maria: a gente andou, Vô"),
            new LocalizationEntry(StoryLine(2, 3), "Grandma Edna starts the hymn softly, and the family joins in", "A Vó Edna puxa o hino baixinho, e a família entra junto"),

            new LocalizationEntry(StoryLine(3, 0), "Grandma Edna: he wrote me from every oasis. About the weather and the price of salt", "Vó Edna: ele me escrevia de cada oásis. Falava do tempo e do preço do sal"),
            new LocalizationEntry(StoryLine(3, 1), "Maithe: only that, Grandma?", "Maithe: só isso, Vó?"),
            new LocalizationEntry(StoryLine(3, 2), "Grandma Edna: at the end of each one: keep the lamp lit, I am coming back", "Vó Edna: no fim de todas: guarda a lâmpada acesa, que eu volto"),
            new LocalizationEntry(StoryLine(3, 3), "Joaquim watches the chair rock by itself, and nobody says a thing", "O Joaquim olha a cadeira balançar sozinha, e ninguém diz nada"),

            new LocalizationEntry(StoryLine(4, 0), "Ícaro: he pointed at the moon and said it was a ship filling up with light", "Ícaro: ele apontava a lua e dizia que era uma barca enchendo de luz"),
            new LocalizationEntry(StoryLine(4, 1), "Lavine: and I said it was a shoe", "Lavine: e eu dizia que era um sapato"),
            new LocalizationEntry(StoryLine(4, 2), "Maria: he agreed with you, just to watch us argue", "Maria: ele concordava com você, só para ver a gente discutir"),
            new LocalizationEntry(StoryLine(4, 3), "Grandpa Jorge, remembered: every spark goes home. It takes time, but it goes", "Vô Jorge, de memória: cada faísca volta para casa. Demora, mas volta"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(BarqueBoard))
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

        /// <summary>Stories × lines, for the tests.</summary>
        public static int StoryLineCount => StoryCount * LinesPerStory;
    }
}
