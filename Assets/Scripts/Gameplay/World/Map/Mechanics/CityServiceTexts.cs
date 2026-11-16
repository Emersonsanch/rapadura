using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the small city services (<see cref="CityServiceRules"/>): Puzur's free rune (Salões de Bronze), the Lira
    /// do Touro (Aratta) and the Têmpera do Tambor (Tambor Rubro). Same self-contained pattern as <c>MazanThinAirTexts</c>.
    /// </summary>
    public static class CityServiceTexts
    {
        private const string P = "ui.city_service.";

        public const string RunePrompt = P + "rune.prompt";
        public const string RuneTaken = P + "rune.taken";
        public const string RuneReceived = P + "rune.received";
        public const string BagFull = P + "bag_full";

        public const string LyrePrompt = P + "lyre.prompt";
        public const string LyreCombat = P + "lyre.combat";
        public const string LyrePlaying = P + "lyre.playing";
        public const string LyreAgain = P + "lyre.again";
        public const string LyreEnd = P + "lyre.end";

        public const string TemperPrompt = P + "temper.prompt";
        public const string TemperWait = P + "temper.wait";
        public const string TemperTaken = P + "temper.taken";
        public const string TemperCombat = P + "temper.combat";
        public const string TemperDone = P + "temper.done";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static string SongName(int song) => P + "lyre.song." + song;

        public static string RuneLine(CharacterId hero) => P + "rune.line." + Hero(hero);

        public static string SongLine(CharacterId hero) => P + "lyre.line." + Hero(hero);

        public static string TemperLine(CharacterId hero) => P + "temper.line." + Hero(hero);

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= Build();

        private static List<LocalizationEntry> Build()
        {
            var e = new List<LocalizationEntry>
            {
                // ---- Puzur's rune pillar (Salões de Bronze) ----
                new LocalizationEntry(RunePrompt, "[E] Puzur's rune pillar: today's rune, engraved free", "[E] Pilar de runa do Puzur: a runa do dia, gravada de graça"),
                new LocalizationEntry(RuneTaken, "Puzur already engraved today's rune for you. Come back tomorrow", "O Puzur já gravou a runa de hoje para vocês. Voltem amanhã"),
                new LocalizationEntry(RuneReceived, "Received: {0} ×{1}", "Recebido: {0} ×{1}"),
                new LocalizationEntry(BagFull, "The backpack is full", "A mochila está cheia"),
                new LocalizationEntry(RuneLine(CharacterId.Joaquim), "Puzur: \"One line at a time, Joaquim. A rune cut in a hurry holds nothing.\"",
                    "Puzur: \"Uma linha por vez, Joaquim. Runa gravada com pressa não segura nada.\""),
                new LocalizationEntry(RuneLine(CharacterId.Maria), "Puzur: \"Touch the bronze before it cools, Maria. Your light sets the line.\"",
                    "Puzur: \"Encosta no bronze antes de esfriar, Maria. A sua luz firma a linha.\""),
                new LocalizationEntry(RuneLine(CharacterId.Maithe), "Puzur: \"For the eldest, the first of the day. Don't tell Untash.\"",
                    "Puzur: \"Para a mais velha, a primeira do dia. Não conta para o Untash.\""),
                new LocalizationEntry(RuneLine(CharacterId.Icaro), "Puzur: \"You watched every stroke, Ícaro. Tomorrow you hold the chisel.\"",
                    "Puzur: \"Você olhou cada traço, Ícaro. Amanhã quem segura o cinzel é você.\""),
                new LocalizationEntry(RuneLine(CharacterId.Lavine), "Puzur: \"Grandma Edna got one of these every winter. Grandpa Jorge, every other.\"",
                    "Puzur: \"A Vó Edna ganhava uma destas todo inverno. O Vô Jorge, um sim, um não.\""),

                // ---- The Lira do Touro (Aratta) ----
                new LocalizationEntry(LyrePrompt, "[E] Lira do Touro: play the song of the day", "[E] Lira do Touro: tocar a canção do dia"),
                new LocalizationEntry(LyreCombat, "The lyre doesn't play in the middle of a fight", "A lira não toca no meio da briga"),
                new LocalizationEntry(LyrePlaying, "New song learned: {0}. Light feet for 5 min (on foot, out of combat)",
                    "Canção nova aprendida: {0}. Pé leve por 5 min (a pé, fora de combate)"),
                new LocalizationEntry(LyreAgain, "{0} again: light feet for 5 more min", "{0} de novo: pé leve por mais 5 min"),
                new LocalizationEntry(LyreEnd, "The song of the lyre faded", "A canção da lira acabou"),
                new LocalizationEntry(SongName(0), "The Song of Enmerkar and the Lord of Aratta", "A Canção de Enmerkar e o Senhor de Aratta"),
                new LocalizationEntry(SongName(1), "Lugalbanda in the Mountain Cave", "Lugalbanda na Gruta da Montanha"),
                new LocalizationEntry(SongName(2), "The Hymn of Shulgi the Runner", "O Hino de Shulgi, o Corredor"),
                new LocalizationEntry(SongName(3), "The Reed and the Mat", "O Junco e a Esteira"),
                new LocalizationEntry(SongName(4), "The Bull Who Carried the Light", "O Touro que Carregou a Luz"),
                new LocalizationEntry(SongLine(CharacterId.Joaquim), "Joaquim: \"I'm not singing. ...I'm humming. It's different.\"",
                    "Joaquim: \"Eu não estou cantando. ...Estou cantarolando. É diferente.\""),
                new LocalizationEntry(SongLine(CharacterId.Maria), "Maria: \"Joaquim, you're humming it too. I heard you.\"",
                    "Maria: \"Joaquim, você está cantarolando também. Eu ouvi.\""),
                new LocalizationEntry(SongLine(CharacterId.Maithe), "Maithe: \"Grandpa Jorge would already be dancing with Grandma Edna.\"",
                    "Maithe: \"O Vô Jorge já estaria dançando com a Vó Edna.\""),
                new LocalizationEntry(SongLine(CharacterId.Icaro), "Ícaro: \"Four beats and a pause. I'll walk to it, it's faster.\"",
                    "Ícaro: \"Quatro batidas e uma pausa. Vou andar no ritmo, é mais rápido.\""),
                new LocalizationEntry(SongLine(CharacterId.Lavine), "Lavine: \"I'll teach this one to Grandma Edna. She'll teach it back better.\"",
                    "Lavine: \"Vou ensinar esta para a Vó Edna. Ela vai me ensinar de volta melhor.\""),

                // ---- The Têmpera do Tambor (Tambor Rubro) ----
                new LocalizationEntry(TemperPrompt, "[E] Têmpera do Tambor: Tushratta tempers the weapon to the beat",
                    "[E] Têmpera do Tambor: o Tushratta tempera a arma no ritmo"),
                new LocalizationEntry(TemperWait, "Tushratta tempers only while the drums play. Wait for the next beat",
                    "O Tushratta só tempera enquanto os tambores tocam. Espera o próximo toque"),
                new LocalizationEntry(TemperTaken, "Your weapon was already tempered today. Come back tomorrow", "A arma já foi temperada hoje. Volta amanhã"),
                new LocalizationEntry(TemperCombat, "No tempering in the middle of a fight", "Nada de têmpera no meio da briga"),
                new LocalizationEntry(TemperDone, "Weapon tempered: +8% damage for 10 min", "Arma temperada: +8% de dano por 10 min"),
                new LocalizationEntry(TemperLine(CharacterId.Joaquim), "Tushratta: \"Hold the shield up while I beat the blade, guardian. Both get stronger.\"",
                    "Tushratta: \"Segura o escudo no alto enquanto eu bato a lâmina, guardião. Os dois ficam mais fortes.\""),
                new LocalizationEntry(TemperLine(CharacterId.Maria), "Tushratta: \"The Living Fire knows you, Maria. It burned brighter just now.\"",
                    "Tushratta: \"O Fogo Vivo conhece você, Maria. Ele acendeu mais forte agora.\""),
                new LocalizationEntry(TemperLine(CharacterId.Maithe), "Tushratta: \"Arrowheads tempered on the third beat. The eldest's beat.\"",
                    "Tushratta: \"Pontas temperadas na terceira batida. A batida da mais velha.\""),
                new LocalizationEntry(TemperLine(CharacterId.Icaro), "Tushratta: \"Yes, Ícaro, the beat sets the hammer. No, you can't take the drum home.\"",
                    "Tushratta: \"Sim, Ícaro, a batida acerta o martelo. Não, você não pode levar o tambor para casa.\""),
                new LocalizationEntry(TemperLine(CharacterId.Lavine), "Tushratta: \"Grandpa Jorge's old knife got this same temper. He still cuts bread with it.\"",
                    "Tushratta: \"A faca velha do Vô Jorge levou esta mesma têmpera. Ele corta pão com ela até hoje.\""),
            };
            return e;
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(RunePrompt))
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

        private static string Hero(CharacterId hero) => hero.ToString().ToLowerInvariant();
    }
}
