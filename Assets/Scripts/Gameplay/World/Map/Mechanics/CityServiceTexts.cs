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

        public const string RacePrompt = P + "race.prompt";
        public const string RaceCombat = P + "race.combat";
        public const string RaceCount = P + "race.count";
        public const string RaceGo = P + "race.go";
        public const string RaceGate = P + "race.gate";
        public const string RaceMedal = P + "race.medal";
        public const string RaceNoMedal = P + "race.no_medal";
        public const string RaceTimeOut = P + "race.time_out";
        public const string RaceAbandon = P + "race.abandon";

        public const string RibbonPrompt = P + "ribbon.prompt";
        public const string RibbonAllOwned = P + "ribbon.all_owned";
        public const string RibbonNeed = P + "ribbon.need";
        public const string RibbonTraded = P + "ribbon.traded";

        public const string ForgePrompt = P + "forge.prompt";
        public const string ForgeAllLit = P + "forge.all_lit";
        public const string ForgeGiftTaken = P + "forge.gift_taken";
        public const string ForgeGift = P + "forge.gift";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static string SongName(int song) => P + "lyre.song." + song;

        public static string RuneLine(CharacterId hero) => P + "rune.line." + Hero(hero);

        public static string SongLine(CharacterId hero) => P + "lyre.line." + Hero(hero);

        public static string TemperLine(CharacterId hero) => P + "temper.line." + Hero(hero);

        public static string RaceLine(CharacterId hero) => P + "race.line." + Hero(hero);

        public static string MedalName(NartesRaceMedal medal) => P + "race.medal_name." + medal.ToString().ToLowerInvariant();

        public static string RibbonLine(CharacterId hero) => P + "ribbon.line." + Hero(hero);

        /// <summary>What the next stage of the Grande Forja still waits for (<see cref="GreatForgeStage.Id"/>).</summary>
        public static string ForgeNeed(string stageId) => P + "forge.need." + stageId;

        /// <summary>The stage just lit and the service it gives the city.</summary>
        public static string ForgeLit(string stageId) => P + "forge.lit." + stageId;

        public static string ForgeLine(CharacterId hero) => P + "forge.line." + Hero(hero);

        public static string ForgeDoneLine(CharacterId hero) => P + "forge.done_line." + Hero(hero);

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

                // ---- The Corrida de Shulgi (Aratta) ----
                new LocalizationEntry(RacePrompt, "[E] Shulgi's race: run the course of the savanna for Fitas de Corrida",
                    "[E] Corrida de Shulgi: correr o percurso da savana valendo Fitas de Corrida"),
                new LocalizationEntry(RaceCombat, "Shulgi doesn't start a race in the middle of a fight",
                    "O Shulgi não dá a largada no meio da briga"),
                new LocalizationEntry(RaceCount, "{0}…",
                    "{0}…"),
                new LocalizationEntry(RaceGo, "Go! Follow the blue ribbons",
                    "Já! Sigam as fitas azuis"),
                new LocalizationEntry(RaceGate, "Ribbon {0}/{1} · {2} s",
                    "Fita {0}/{1} · {2} s"),
                new LocalizationEntry(RaceMedal, "{0} in {1} s: {2} Fitas de Corrida (you have {3})",
                    "{0} em {1} s: {2} Fitas de Corrida (vocês têm {3})"),
                new LocalizationEntry(RaceNoMedal, "Finished in {0} s. Shulgi says: faster, and the ribbon is yours",
                    "Chegaram em {0} s. O Shulgi diz: mais rápido, e a fita é de vocês"),
                new LocalizationEntry(RaceTimeOut, "Time's up: the race is over",
                    "Acabou o tempo: a corrida terminou"),
                new LocalizationEntry(RaceAbandon, "The race was left behind",
                    "A corrida ficou para trás"),
                new LocalizationEntry(MedalName(NartesRaceMedal.Gold), "Gold ribbon",
                    "Fita de ouro"),
                new LocalizationEntry(MedalName(NartesRaceMedal.Silver), "Silver ribbon",
                    "Fita de prata"),
                new LocalizationEntry(MedalName(NartesRaceMedal.Bronze), "Bronze ribbon",
                    "Fita de bronze"),
                new LocalizationEntry(RaceLine(CharacterId.Joaquim), "Shulgi: \"Nippur to Ur in a day, Joaquim. Today, just the savanna. Three… two…\"",
                    "Shulgi: \"De Nippur a Ur num dia, Joaquim. Hoje, só a savana. Três… dois…\""),
                new LocalizationEntry(RaceLine(CharacterId.Maria), "Shulgi: \"Run with the light at your back, Maria. The grass bows to it.\"",
                    "Shulgi: \"Corre com a luz nas costas, Maria. O capim se curva para ela.\""),
                new LocalizationEntry(RaceLine(CharacterId.Maithe), "Shulgi: \"The eldest sets the pace. Don't look back, they'll catch up.\"",
                    "Shulgi: \"A mais velha dita o ritmo. Não olha para trás, eles alcançam.\""),
                new LocalizationEntry(RaceLine(CharacterId.Icaro), "Shulgi: \"Yes, Ícaro, the shortest line between ribbons is straight. Now run it.\"",
                    "Shulgi: \"Sim, Ícaro, a linha mais curta entre as fitas é reta. Agora corre nela.\""),
                new LocalizationEntry(RaceLine(CharacterId.Lavine), "Shulgi: \"Grandpa Jorge ran this course once, they say. He stopped to talk to every aurochs.\"",
                    "Shulgi: \"Dizem que o Vô Jorge correu este percurso uma vez. Parou para conversar com cada auroque.\""),

                // ---- Shulgi's ribbon post (Aratta) ----
                new LocalizationEntry(RibbonPrompt, "[E] Shulgi's ribbon post: trade Fitas de Corrida",
                    "[E] Poste de fitas do Shulgi: trocar Fitas de Corrida"),
                new LocalizationEntry(RibbonAllOwned, "Shulgi: \"You have everything I trade. Keep the ribbons: {0} of them, for the stories.\"",
                    "Shulgi: \"Vocês já têm tudo o que eu troco. Guardem as fitas: {0}, para as histórias.\""),
                new LocalizationEntry(RibbonNeed, "{0} costs {1} Fitas de Corrida. You have {2}",
                    "{0} custa {1} Fitas de Corrida. Vocês têm {2}"),
                new LocalizationEntry(RibbonTraded, "Traded: {0} for {1} Fitas de Corrida ({2} left)",
                    "Trocado: {0} por {1} Fitas de Corrida (sobram {2})"),
                new LocalizationEntry(RibbonLine(CharacterId.Joaquim), "Joaquim: \"I ran for this. Every ribbon of it.\"",
                    "Joaquim: \"Eu corri por isso. Cada fita.\""),
                new LocalizationEntry(RibbonLine(CharacterId.Maria), "Maria: \"Shulgi tied the knot himself. It won't come loose.\"",
                    "Maria: \"O Shulgi deu o nó ele mesmo. Não solta.\""),
                new LocalizationEntry(RibbonLine(CharacterId.Maithe), "Maithe: \"Grandma Edna will want to see this one.\"",
                    "Maithe: \"A Vó Edna vai querer ver essa.\""),
                new LocalizationEntry(RibbonLine(CharacterId.Icaro), "Ícaro: \"Twenty-five ribbons for an aurochs. I counted twice.\"",
                    "Ícaro: \"Vinte e cinco fitas por um auroque. Eu contei duas vezes.\""),
                new LocalizationEntry(RibbonLine(CharacterId.Lavine), "Lavine: \"Blue like the sky of the savanna. Grandpa Jorge's favourite colour.\"",
                    "Lavine: \"Azul como o céu da savana. A cor preferida do Vô Jorge.\""),

                // ---- The Grande Forja (Salões de Bronze) ----
                new LocalizationEntry(ForgePrompt, "[E] The Grande Forja: Napirasu at the forge mouth",
                    "[E] A Grande Forja: a Napirasu na boca da forja"),
                new LocalizationEntry(ForgeAllLit, "Napirasu: \"The five are lit. Go and refine something worth the fire.\"",
                    "Napirasu: \"As cinco estão acesas. Vão refinar alguma coisa que valha o fogo.\""),
                new LocalizationEntry(ForgeGiftTaken, "Napirasu: \"Today's stones are in your bag already. The fire rests at night too.\"",
                    "Napirasu: \"As pedras de hoje já estão na mochila. O fogo também descansa de noite.\""),
                new LocalizationEntry(ForgeGift, "The forge's gift: {0} ×{1}",
                    "O presente da forja: {0} ×{1}"),
                new LocalizationEntry(ForgeNeed("fole"), "Napirasu: \"The Bellows is dark: the shadow took the engine that blew the forge. Beat the Dark Bellows in the lower halls and it will blow again.\"",
                    "Napirasu: \"O fole está apagado: a sombra tomou o engenho que soprava a forja. Vençam o Fole Apagado nos salões de baixo e ele volta a soprar.\""),
                new LocalizationEntry(ForgeNeed("brasa"), "Napirasu: \"The embers need the stairs clear (Untash's slag golems) and 10 iron ore and 10 wood to catch.\"",
                    "Napirasu: \"A brasa precisa da escada livre (os golens de escória do Untash) e de 10 minérios de ferro e 10 madeiras para pegar.\""),
                new LocalizationEntry(ForgeNeed("bigorna"), "Napirasu: \"The anvil waits for Shilhak's seam to be free of the ember bats, and for 6 iron and 10 stone.\"",
                    "Napirasu: \"A bigorna espera o veio do Shilhak livre dos morcegos de brasa, e 6 ferros e 10 pedras.\""),
                new LocalizationEntry(ForgeNeed("runas"), "Napirasu: \"The runes of the forge: bring one of each of Puzur's free runes, Clarity, Leather, Healing and Wind.\"",
                    "Napirasu: \"As runas da forja: tragam uma de cada runa de graça do Puzur, Clareza, Couro, Cura e Vento.\""),
                new LocalizationEntry(ForgeNeed("cristal"), "Napirasu: \"The Crystal of the Light stays dark while Hahya, the Giant Warrior, holds the summit. Beat him, and bring 3 Blessing Stones and 5 iron.\"",
                    "Napirasu: \"O Cristal da Luz fica escuro enquanto o Hahya, o Gigante Guerreiro, segura o cume. Vençam ele, e tragam 3 Pedras da Bênção e 5 ferros.\""),
                new LocalizationEntry(ForgeLit("fole"), "Stage 1 of 5, the Bellows, blows again: every refine that can fail gets +2% chance",
                    "Etapa 1 de 5, o Fole, sopra de novo: todo refino que pode falhar ganha +2% de chance"),
                new LocalizationEntry(ForgeLit("brasa"), "Stage 2 of 5, the Embers, catch: the forge gives a Blessing Stone a day",
                    "Etapa 2 de 5, a Brasa, pegou: a forja dá uma Pedra da Bênção por dia"),
                new LocalizationEntry(ForgeLit("bigorna"), "Stage 3 of 5, the Anvil, rings: Puzur now engraves two runes a day",
                    "Etapa 3 de 5, a Bigorna, tine: o Puzur agora grava duas runas por dia"),
                new LocalizationEntry(ForgeLit("runas"), "Stage 4 of 5, the Runes, light up one line at a time: refines get +4% chance",
                    "Etapa 4 de 5, as Runas, acendem uma linha por vez: o refino ganha +4% de chance"),
                new LocalizationEntry(ForgeLit("cristal"), "Stage 5 of 5: the Crystal of the Light shines in the claw. The Grande Forja is lit: refines +5%, two Blessing Stones a day",
                    "Etapa 5 de 5: o Cristal da Luz brilha na garra. A Grande Forja está acesa: refino +5%, duas Pedras da Bênção por dia"),
                new LocalizationEntry(ForgeLine(CharacterId.Joaquim), "Napirasu: \"Strong arms on the bellows, Joaquim. My grandfather's were like that.\"",
                    "Napirasu: \"Braço forte no fole, Joaquim. O do meu avô era assim.\""),
                new LocalizationEntry(ForgeLine(CharacterId.Maria), "Napirasu: \"Hold your hand near the crystal, Maria. It remembers the Light.\"",
                    "Napirasu: \"Chega a mão perto do cristal, Maria. Ele lembra da Luz.\""),
                new LocalizationEntry(ForgeLine(CharacterId.Maithe), "Napirasu: \"The eldest feeds the fire first. That is the custom of the halls.\"",
                    "Napirasu: \"A mais velha alimenta o fogo primeiro. É o costume dos salões.\""),
                new LocalizationEntry(ForgeLine(CharacterId.Icaro), "Napirasu: \"You counted the strokes of the bellows, Ícaro? Then you know why it caught.\"",
                    "Napirasu: \"Você contou as sopradas do fole, Ícaro? Então sabe por que pegou.\""),
                new LocalizationEntry(ForgeLine(CharacterId.Lavine), "Napirasu: \"Grandma Edna sent apricot sweets the winter the forge went out. Tell her it is warming up.\"",
                    "Napirasu: \"A Vó Edna mandou doce de damasco no inverno em que a forja apagou. Conta para ela que está esquentando.\""),
                new LocalizationEntry(ForgeDoneLine(CharacterId.Joaquim), "Joaquim: \"It's hot in here. Finally.\"",
                    "Joaquim: \"Está quente aqui. Finalmente.\""),
                new LocalizationEntry(ForgeDoneLine(CharacterId.Maria), "Maria: \"The Light was here all along, just asleep.\"",
                    "Maria: \"A Luz estava aqui o tempo todo, só dormindo.\""),
                new LocalizationEntry(ForgeDoneLine(CharacterId.Maithe), "Maithe: \"Grandpa Jorge would want a new hoe from this fire.\"",
                    "Maithe: \"O Vô Jorge ia querer uma enxada nova deste fogo.\""),
                new LocalizationEntry(ForgeDoneLine(CharacterId.Icaro), "Ícaro: \"Five stages, five of us. That's not a coincidence.\"",
                    "Ícaro: \"Cinco etapas, cinco de nós. Isso não é coincidência.\""),
                new LocalizationEntry(ForgeDoneLine(CharacterId.Lavine), "Lavine: \"Napirasu is crying. Or it's the smoke. I'll say it's the smoke.\"",
                    "Lavine: \"A Napirasu está chorando. Ou é a fumaça. Vou dizer que é a fumaça.\""),
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
