using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the 2026-11-23 round on maps 39, 51, 53 and 56: the valley's wind in O Senhor do Cata-Vento's gusts
    /// (<see cref="VaneValleyWindRules"/>), the Pactolo's water on O Ouro de Midas (<see cref="MidasWashRules"/>), the Baile do Vale
    /// (<see cref="SatyrDanceRules"/>), the silent skills of the hermitages (<see cref="SilentSkillRules"/>) and the rare horse of the herding
    /// (<see cref="PrairieHerdMountRules"/>), with one line per cousin for each first time. Same self-contained pattern as <see cref="PactoloGildTexts"/>.
    /// </summary>
    public static class PeaksSteppeRoundTexts
    {
        public const string VaneBroken = "ui.round_1123.vane_broken";
        public const string VaneCarried = "ui.round_1123.vane_carried";
        public const string VaneLinePrefix = "ui.round_1123.vane_line.";

        public const string WashFillPrompt = "ui.round_1123.wash_fill_prompt";
        public const string WashFilled = "ui.round_1123.wash_filled";
        public const string WashFull = "ui.round_1123.wash_full";
        public const string WashPourPrompt = "ui.round_1123.wash_pour_prompt";
        public const string WashPoured = "ui.round_1123.wash_poured";
        public const string WashTouchStopped = "ui.round_1123.wash_touch_stopped";
        public const string WashDried = "ui.round_1123.wash_dried";
        public const string WashLinePrefix = "ui.round_1123.wash_line.";

        public const string DancePrompt = "ui.round_1123.dance_prompt";
        public const string DanceDay = "ui.round_1123.dance_day";
        public const string DanceCountIn = "ui.round_1123.dance_count_in";
        public const string DanceOnBeat = "ui.round_1123.dance_on_beat";
        public const string DanceClose = "ui.round_1123.dance_close";
        public const string DanceStumble = "ui.round_1123.dance_stumble";
        public const string DanceMissed = "ui.round_1123.dance_missed";
        public const string DanceGold = "ui.round_1123.dance_gold";
        public const string DanceSilver = "ui.round_1123.dance_silver";
        public const string DanceBronze = "ui.round_1123.dance_bronze";
        public const string DanceNoMedal = "ui.round_1123.dance_no_medal";
        public const string DancePaid = "ui.round_1123.dance_paid";
        public const string DanceStopped = "ui.round_1123.dance_stopped";
        public const string DanceLinePrefix = "ui.round_1123.dance_line.";

        public const string SkillQuiet = "ui.round_1123.skill_quiet";
        public const string SkillLoud = "ui.round_1123.skill_loud";

        public const string HerdMount = "ui.round_1123.herd_mount";
        public const string HerdLinePrefix = "ui.round_1123.herd_line.";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            // 39 · the valley's wind in the vane lord's gusts
            new LocalizationEntry(VaneBroken, "The valley's wind breaks his gust!", "O vento do vale quebra a rajada dele!"),
            new LocalizationEntry(VaneCarried, "The valley's wind carries his gust: it goes further!", "O vento do vale leva a rajada: ela vai mais longe!"),
            new LocalizationEntry(VaneLinePrefix + "maithe", "Maithe: I turned the vanes this morning. The wind remembers who asked first.", "Maithe: Eu girei os cata-ventos de manhã. O vento lembra quem pediu primeiro."),
            new LocalizationEntry(VaneLinePrefix + "icaro", "Ícaro: Four vanes against one engine. Math wins.", "Ícaro: Quatro cata-ventos contra um engenho. A conta ganhou."),
            new LocalizationEntry(VaneLinePrefix + "joaquim", "Joaquim: The valley picked a side. Ours.", "Joaquim: O vale escolheu um lado. O nosso."),
            new LocalizationEntry(VaneLinePrefix + "maria", "Maria: He blew and the wind said no! Do it again, wind!", "Maria: Ele soprou e o vento disse não! Faz de novo, vento!"),
            new LocalizationEntry(VaneLinePrefix + "lavine", "Lavine: Vô Jorge says you don't fight the wind, you get it on your side.", "Lavine: O Vô Jorge fala que vento não se briga, se chama pro lado da gente."),

            // 53 · the Pactolo's water on O Ouro de Midas
            new LocalizationEntry(WashFillPrompt, "[E] Cup the Pactolo's water in your hands", "[E] Pegar a água do Pactolo nas mãos"),
            new LocalizationEntry(WashFilled, "Living Water in your hands ({0}/{1}): pour it on the golden statue's hands", "Água Viva nas mãos ({0}/{1}): derrame nas mãos da estátua de ouro"),
            new LocalizationEntry(WashFull, "Your hands are full of the Pactolo", "As mãos já estão cheias do Pactolo"),
            new LocalizationEntry(WashPourPrompt, "[E] Pour the Pactolo on the statue's hands ({0} left)", "[E] Derramar o Pactolo nas mãos da estátua (faltam {0})"),
            new LocalizationEntry(WashPoured, "The water runs down the gold: the statue stands still!", "A água escorre pelo ouro: a estátua para em pé!"),
            new LocalizationEntry(WashTouchStopped, "The touch does not answer: its hands are wet with the Pactolo", "O toque não responde: as mãos estão molhadas do Pactolo"),
            new LocalizationEntry(WashDried, "The Pactolo's water dried in your hands", "A água do Pactolo secou nas mãos"),
            new LocalizationEntry(WashLinePrefix + "maithe", "Maithe: Midas washed his hands in this river. The statue remembers.", "Maithe: O Midas lavou as mãos nesse rio. A estátua lembra."),
            new LocalizationEntry(WashLinePrefix + "icaro", "Ícaro: Gold, meet water. Water wins. Every time.", "Ícaro: Ouro, essa é a água. A água ganha. Sempre."),
            new LocalizationEntry(WashLinePrefix + "joaquim", "Joaquim: Heavy hands, light water. Midas would laugh.", "Joaquim: Mão pesada, água leve. O Midas ia rir."),
            new LocalizationEntry(WashLinePrefix + "maria", "Maria: I carried the river all the way here and didn't spill it!", "Maria: Eu trouxe o rio até aqui e não derramei nada!"),
            new LocalizationEntry(WashLinePrefix + "lavine", "Lavine: Vó Edna was right. Wash your hands and the trouble stops.", "Lavine: A Vó Edna tinha razão. Lava a mão que a confusão para."),

            // 53 · the Baile do Vale
            new LocalizationEntry(DancePrompt, "[E] Dance with Marsyas on the stone stage", "[E] Dançar com o Marsyas no palco de pedra"),
            new LocalizationEntry(DanceDay, "The Baile do Vale is at night: the satyrs are in the vines", "O Baile do Vale é à noite: os sátiros estão nas vinhas"),
            new LocalizationEntry(DanceCountIn, "Marsyas tunes the double flute... step on every note! [E]", "O Marsyas afina a flauta dupla... um passo em cada nota! [E]"),
            new LocalizationEntry(DanceOnBeat, "On the beat!", "No tempo!"),
            new LocalizationEntry(DanceClose, "Close!", "Quase!"),
            new LocalizationEntry(DanceStumble, "Stumbled!", "Tropeçou!"),
            new LocalizationEntry(DanceMissed, "A note went by", "Passou uma nota"),
            new LocalizationEntry(DanceGold, "Gold in the Baile! ({0}/{1})", "Ouro no Baile! ({0}/{1})"),
            new LocalizationEntry(DanceSilver, "Silver in the Baile ({0}/{1})", "Prata no Baile ({0}/{1})"),
            new LocalizationEntry(DanceBronze, "Bronze in the Baile ({0}/{1})", "Bronze no Baile ({0}/{1})"),
            new LocalizationEntry(DanceNoMedal, "Out of step: {0}/{1}. Marsyas plays again whenever you like", "Fora do compasso: {0}/{1}. O Marsyas toca de novo quando quiser"),
            new LocalizationEntry(DancePaid, "Marsyas pays the dancer: {0} gold nuggets", "O Marsyas paga a dança: {0} pepitas de ouro"),
            new LocalizationEntry(DanceStopped, "The flute stops: the dance is over", "A flauta parou: o baile acabou"),
            new LocalizationEntry(DanceLinePrefix + "maithe", "Maithe: The flute and the wind play the same tune. I just listened.", "Maithe: A flauta e o vento tocam a mesma música. Eu só escutei."),
            new LocalizationEntry(DanceLinePrefix + "icaro", "Ícaro: 0.7 seconds a beat at the end. I counted. And still danced.", "Ícaro: 0,7 segundo por nota no fim. Eu contei. E dancei mesmo assim."),
            new LocalizationEntry(DanceLinePrefix + "joaquim", "Joaquim: Don't tell anyone I can dance.", "Joaquim: Não conta pra ninguém que eu sei dançar."),
            new LocalizationEntry(DanceLinePrefix + "maria", "Maria: Again! Again! The satyrs are clapping!", "Maria: De novo! De novo! Os sátiros tão batendo palma!"),
            new LocalizationEntry(DanceLinePrefix + "lavine", "Lavine: Vô Jorge danced like this at the Bema. Vó Edna told me.", "Lavine: O Vô Jorge dançava assim no Bema. A Vó Edna me contou."),

            // 56 · the silent skills
            new LocalizationEntry(SkillQuiet, "A quiet skill: the cliff calms down", "Skill silenciosa: o paredão se acalma"),
            new LocalizationEntry(SkillLoud, "That one was loud! The cliff heard it", "Essa foi barulhenta! O paredão ouviu"),

            // 51 · the rare horse of the herding
            new LocalizationEntry(HerdMount, "Soslan's herders give you the heavenly horse that came with the herd: the Heavenly Horse of Ferghana!", "Os pastores de Soslan te dão o cavalo celeste que veio na manada: o Cavalo Celeste de Fergana!"),
            new LocalizationEntry(HerdLinePrefix + "maithe", "Maithe: It followed the herd but it was looking for us.", "Maithe: Ele veio com a manada, mas estava procurando a gente."),
            new LocalizationEntry(HerdLinePrefix + "icaro", "Ícaro: All that herding paid off. Worth every horse.", "Ícaro: Tanto pastoreio deu nisso. Valeu cada cavalo."),
            new LocalizationEntry(HerdLinePrefix + "joaquim", "Joaquim: A horse of light from the herd. Soslan said I earned it.", "Joaquim: Um cavalo de luz da manada. O Soslan disse que eu mereci."),
            new LocalizationEntry(HerdLinePrefix + "maria", "Maria: He's golden! Can I name him? I'm naming him.", "Maria: Ele é dourado! Posso dar nome? Vou dar nome."),
            new LocalizationEntry(HerdLinePrefix + "lavine", "Lavine: Vó Edna said a good horse finds its rider. This one found me.", "Lavine: A Vó Edna disse que cavalo bom acha o dono. Esse me achou."),
        };

        /// <summary>The cousin's line under <paramref name="prefix"/> (Joaquim's when no cousin is known).</summary>
        public static string LineFor(string prefix, CharacterId? hero)
        {
            switch (hero)
            {
                case CharacterId.Maithe: return prefix + "maithe";
                case CharacterId.Icaro: return prefix + "icaro";
                case CharacterId.Maria: return prefix + "maria";
                case CharacterId.Lavine: return prefix + "lavine";
                default: return prefix + "joaquim";
            }
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(VaneBroken))
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

        public static string Format(LocalizationManager manager, string key, params object[] args) => string.Format(Get(manager, key), args);
    }
}
