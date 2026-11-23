using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the second 2026-11-23 round on maps 49, 52, 55, 56 and 70: the falls off the Pontes de Vento and the freed
    /// Cortador de Cordas catching them (<see cref="WindBridgeFallRules"/>), the stilled bell's silence (<see cref="HermitageStillBellRules"/>), the
    /// good dream once the Pesadelo da Tábua is beaten (<see cref="GoodDreamRules"/>), Amirani's chain glowing at night
    /// (<see cref="ChainNightGlowRules"/>) and the caldera's night lava and the Glutão's own heat (<see cref="CalderaFireRules"/>), with one line
    /// per cousin for each first time. Same self-contained pattern as <see cref="PeaksSteppeRoundTexts"/>.
    /// </summary>
    public static class PeaksSteppeRound2Texts
    {
        public const string BridgeFall = "ui.round2_1123.bridge_fall";
        public const string BridgeCaught = "ui.round2_1123.bridge_caught";
        public const string BridgeLinePrefix = "ui.round2_1123.bridge_line.";

        public const string BellStilled = "ui.round2_1123.bell_stilled";
        public const string BellLinePrefix = "ui.round2_1123.bell_line.";

        public const string GoodDream = "ui.round2_1123.good_dream";
        public const string GoodDreamFound = "ui.round2_1123.good_dream_found";
        public const string DreamLinePrefix = "ui.round2_1123.dream_line.";

        public const string ChainNightGlow = "ui.round2_1123.chain_night_glow";
        public const string ChainNightDark = "ui.round2_1123.chain_night_dark";
        public const string ChainSnow = "ui.round2_1123.chain_snow";
        public const string ChainLinePrefix = "ui.round2_1123.chain_line.";

        public const string LavaRises = "ui.round2_1123.lava_rises";
        public const string GluttonHeat = "ui.round2_1123.glutton_heat";
        public const string GluttonHeatForges = "ui.round2_1123.glutton_heat_forges";
        public const string GluttonLinePrefix = "ui.round2_1123.glutton_line.";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            // 55 · falling off the wind bridges, and the freed Rope Cutter
            new LocalizationEntry(BridgeFall, "Blown off the planks! The Sisin's net throws you back onto the bridge", "A rajada te jogou das tábuas! A rede do Sisin te devolve para a ponte"),
            new LocalizationEntry(BridgeCaught, "The Rope Cutter catches you in the air and sets you back on the bridge", "O Cortador de Cordas te pega no ar e te põe de volta na ponte"),
            new LocalizationEntry(BridgeLinePrefix + "maithe", "Maithe: He used to cut the ropes. Now he holds them. The wind changed his mind.", "Maithe: Ele cortava as cordas. Agora segura. O vento mudou a cabeça dele."),
            new LocalizationEntry(BridgeLinePrefix + "icaro", "Ícaro: A bird as a safety net. Best engineering on the whole mountain.", "Ícaro: Um pássaro de rede de segurança. Melhor engenharia da montanha inteira."),
            new LocalizationEntry(BridgeLinePrefix + "joaquim", "Joaquim: I'd have held on. But thanks, bird.", "Joaquim: Eu ia segurar. Mas valeu, pássaro."),
            new LocalizationEntry(BridgeLinePrefix + "maria", "Maria: He caught me! Again! No, wait, don't let me fall again!", "Maria: Ele me pegou! De novo! Não, espera, não me deixa cair de novo!"),
            new LocalizationEntry(BridgeLinePrefix + "lavine", "Lavine: Vó Edna says whoever stops cutting starts mending. He's mending.", "Lavine: A Vó Edna fala que quem para de cortar começa a remendar. Ele tá remendando."),

            // 56 · the stilled bell
            new LocalizationEntry(BellStilled, "The bell has stopped: the cliff is quieter now", "O sino parou: o paredão está mais quieto agora"),
            new LocalizationEntry(BellLinePrefix + "maithe", "Maithe: Now you can hear the wind between the hermitages. It was there all along.", "Maithe: Agora dá pra ouvir o vento entre as ermidas. Ele sempre esteve aqui."),
            new LocalizationEntry(BellLinePrefix + "icaro", "Ícaro: No more ringing. My ears are thanking me in two languages.", "Ícaro: Acabou o badalo. Meus ouvidos agradecem em duas línguas."),
            new LocalizationEntry(BellLinePrefix + "joaquim", "Joaquim: Quiet. I like it quiet.", "Joaquim: Silêncio. Eu gosto de silêncio."),
            new LocalizationEntry(BellLinePrefix + "maria", "Maria: Shhh! I'm being so quiet! Is this quiet? This is quiet.", "Maria: Shhh! Eu tô quietinha! Isso é quieto? Isso é quieto."),
            new LocalizationEntry(BellLinePrefix + "lavine", "Lavine: Vô Jorge says silence is a sound too. The hermits are listening to it.", "Lavine: O Vô Jorge fala que silêncio também é som. Os ermitões tão escutando ele."),

            // 49 · the good dream
            new LocalizationEntry(GoodDream, "Virogdad: \"The nightmare is gone and the dream came clear: a chest {0} m {1} of the fort, under a stone that laughs\"", "Virogdad: \"O pesadelo foi embora e o sonho veio claro: um baú a {0} m {1} do fortim, debaixo de uma pedra que ri\""),
            new LocalizationEntry(GoodDreamFound, "The good dream's chest: {0} gold nuggets", "O baú do sonho bom: {0} pepitas de ouro"),
            new LocalizationEntry(DreamLinePrefix + "maithe", "Maithe: The giants sleep better now. You can tell by the dreams.", "Maithe: Os gigantes dormem melhor agora. Dá pra ver pelos sonhos."),
            new LocalizationEntry(DreamLinePrefix + "icaro", "Ícaro: Twenty-five metres of precision. Virogdad upgraded.", "Ícaro: Vinte e cinco metros de precisão. O Virogdad melhorou de versão."),
            new LocalizationEntry(DreamLinePrefix + "joaquim", "Joaquim: A good dream and a heavier chest. Fair trade for one nightmare.", "Joaquim: Sonho bom e baú mais pesado. Troca justa por um pesadelo."),
            new LocalizationEntry(DreamLinePrefix + "maria", "Maria: I want a giant's dream too! A good one, with a chest!", "Maria: Eu quero um sonho de gigante também! Um bom, com baú!"),
            new LocalizationEntry(DreamLinePrefix + "lavine", "Lavine: Vó Edna tells bad dreams to the morning so they leave. The giants learned it.", "Lavine: A Vó Edna conta sonho ruim pra manhã pra ele ir embora. Os gigantes aprenderam."),

            // 52 · Amirani's chain at night
            new LocalizationEntry(ChainNightGlow, "The chain glows up to ring {0}: where the light already climbed, the gusts are weak", "A corrente brilha até a argola {0}: onde a luz já subiu, as rajadas são fracas"),
            new LocalizationEntry(ChainNightDark, "The chain is dark tonight: climb it by day and the light climbs with you", "A corrente está escura esta noite: suba de dia que a luz sobe junto"),
            new LocalizationEntry(ChainSnow, "Strong snow above the lit rings!", "Neve forte acima das argolas acesas!"),
            new LocalizationEntry(ChainLinePrefix + "maithe", "Maithe: The links remember our steps. They light up for us.", "Maithe: Os elos lembram dos nossos passos. Eles acendem pra gente."),
            new LocalizationEntry(ChainLinePrefix + "icaro", "Ícaro: Glowing iron that slows the wind. I'm writing that down.", "Ícaro: Ferro que brilha e segura o vento. Vou anotar isso."),
            new LocalizationEntry(ChainLinePrefix + "joaquim", "Joaquim: Lit up to where I got. Tomorrow, further.", "Joaquim: Aceso até onde eu cheguei. Amanhã, mais alto."),
            new LocalizationEntry(ChainLinePrefix + "maria", "Maria: It's like a necklace of stars! A really, really big necklace!", "Maria: Parece um colar de estrelas! Um colar muito, muito grande!"),
            new LocalizationEntry(ChainLinePrefix + "lavine", "Lavine: Vô Jorge says light that climbed doesn't come back down. Look.", "Lavine: O Vô Jorge fala que luz que subiu não desce mais. Olha aí."),

            // 70 · the night lava and the Glutão's heat
            new LocalizationEntry(LavaRises, "Night: the lava creeps up the shore. The heat starts further out", "Noite: a lava sobe pela margem. O calor começa mais longe"),
            new LocalizationEntry(GluttonHeat, "The Glutão is fire: near him the heat climbs. Step out to cool!", "O Glutão é fogo: perto dele o calor sobe. Saia para esfriar!"),
            new LocalizationEntry(GluttonHeatForges, "{0} lit forge(s) in Hinokura: the Glutão burns weaker", "{0} forja(s) acesa(s) em Hinokura: o Glutão queima mais fraco"),
            new LocalizationEntry(GluttonLinePrefix + "maithe", "Maithe: He eats fire and gives heat back. Not to us, though.", "Maithe: Ele come fogo e devolve calor. Só que não pra gente."),
            new LocalizationEntry(GluttonLinePrefix + "icaro", "Ícaro: Every lit forge is fire he can't eat. Light them all, then come back.", "Ícaro: Cada forja acesa é fogo que ele não come. Acende todas e volta."),
            new LocalizationEntry(GluttonLinePrefix + "joaquim", "Joaquim: Hit, step out, cool down, hit again. Like the forge.", "Joaquim: Bate, sai, esfria, bate de novo. Igual na forja."),
            new LocalizationEntry(GluttonLinePrefix + "maria", "Maria: He's so hot my hair is curling! More than it already was!", "Maria: Ele é tão quente que meu cabelo tá enrolando! Mais do que já era!"),
            new LocalizationEntry(GluttonLinePrefix + "lavine", "Lavine: Vó Edna never lets the stove eat the whole wood. You have to leave some for tomorrow.", "Lavine: A Vó Edna nunca deixa o fogão comer a lenha toda. Tem que sobrar pra amanhã."),
        };

        /// <summary>The cousin's line under <paramref name="prefix"/> (Joaquim's when no cousin is known).</summary>
        public static string LineFor(string prefix, CharacterId? hero) => PeaksSteppeRoundTexts.LineFor(prefix, hero);

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(BridgeFall))
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
