using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of six more map bosses of Regions 10–16 (Docs/Mapas100.md 57, 63, 67, 74, 86, 97) and the lines they call out, en + pt-BR.
    /// Like the others, each is a creature gone dark that the cousins free: its last line says what it goes back to.
    /// </summary>
    public static partial class RegionBossTexts
    {
        /// <summary>Which way the King of the Ays' gust blows (<see cref="RegionBossRules.AysGustDirection"/>), called out as he warns: up the gorge or down it.</summary>
        public static string AysGustKey(int gust) => (gust & 1) == 0 ? "region_boss.ays_king.gust.up" : "region_boss.ays_king.gust.down";

        /// <summary>The label at the end of the route the Trapped Aurora lights (<see cref="RegionBossRules.AuroraRouteDirection"/>).</summary>
        public const string AuroraRouteKey = "region_boss.trapped_aurora.route";

        /// <summary>The label over each loop the Wrong Knot throws (<see cref="RegionBossRules.KnotLoopCentres"/>).</summary>
        public const string KnotLoopKey = "region_boss.wrong_knot.loop";

        /// <summary>Which side the Shadow of the Sail swings the boom to (<see cref="RegionBossRules.BoomSwingsStarboard"/>), called out as it warns.</summary>
        public static string BoomSideKey(bool starboard) => starboard ? "region_boss.sail_shadow.boom.starboard" : "region_boss.sail_shadow.boom.port";

        private static void AddUpperMapBossPages(List<LocalizationEntry> entries)
        {
            // -- Garganta dos Ays (57) --
            Page(entries, RegionBossRules.AysKingId,
                "In the mountains of the far west, children, the old people tell of the ays: little spirits of the bad wind that ride the dust and get into the houses through the cracks. In the Gorge of the Ays they live inside the wind itself, and you only see them when it lifts the dust in their shape. The King of the Ays is the biggest whirlwind of them all, and the shadow made him forget he was ever a breeze. When he draws breath, his gust runs straight along the gorge from him, up it one time, down it the next. Stand behind him, upwind, or step off its line to the side, and it goes by. Beaten, he turns into a soft breeze again, and the shepherds of the gorge will thank you for it.",
                "Nas montanhas do oeste distante, crianças, o povo antigo conta dos ays: espiritozinhos do vento ruim que andam na poeira e entram nas casas pelas frestas. Na Garganta dos Ays eles moram dentro do próprio vento, e só dá pra ver eles quando o vento levanta a poeira no formato deles. O Rei dos Ays é o maior redemoinho de todos, e a sombra fez ele esquecer que um dia foi brisa. Quando ele puxa o fôlego, a rajada corre reta pela garganta a partir dele, garganta acima uma vez, garganta abaixo na outra. Fiquem atrás dele, contra o vento, ou saiam da linha pro lado, e ela passa. Vencido, ele vira brisa mansa de novo, e os pastores da garganta vão agradecer.",
                ("Behind him or beside the line. Never in front of the wind.", "Atrás dele ou do lado da linha. Nunca na frente do vento."),
                ("He was a breeze once. Somebody must have shut every window on him.", "Ele já foi brisa. Alguém deve ter fechado todas as janelas na cara dele."),
                ("A king made of wind. So the crown just blows away? Every time? Sad.", "Um rei feito de vento. Então a coroa sai voando? Toda vez? Triste."),
                ("The gorge only lets the wind go two ways. Neither of them is sideways.", "A garganta só deixa o vento ir pra dois lados. Nenhum deles é pro lado."),
                ("I can see the little ones in the dust! They look like puppies! Angry puppies!", "Eu vejo os pequenininhos na poeira! Parecem filhotes! Filhotes bravos!"));
            Line(entries, RegionBossRules.AysKingId, RegionBossPhase.Storm, "The King of the Ays throws stones in the wind!", "O Rei dos Ays atira pedras no vento!");
            Line(entries, RegionBossRules.AysKingId, RegionBossPhase.LastStand, "The whole gorge is howling!", "A garganta inteira está uivando!");
            Blow(entries, RegionBossRules.AysKingId, "The whirlwind comes down!", "Lá vem o redemoinho!");
            Signature(entries, RegionBossRules.AysKingId, "He's drawing breath! Get off the line of the gorge!", "Ele está puxando o fôlego! Sai da linha da garganta!");
            entries.Add(new LocalizationEntry(AysGustKey(0), "The gust blows up the gorge! Stand behind him or aside!", "A rajada sobe a garganta! Fica atrás dele ou do lado!"));
            entries.Add(new LocalizationEntry(AysGustKey(1), "The gust blows down the gorge! Stand behind him or aside!", "A rajada desce a garganta! Fica atrás dele ou do lado!"));
            Withdraw(entries, RegionBossRules.AysKingId, "The King of the Ays spins slower and slower, drops the stones he carried, and goes out of the gorge as a breeze that smells of thyme.", "O Rei dos Ays gira cada vez mais devagar, solta as pedras que carregava e sai da garganta como uma brisa com cheiro de tomilho.");

            // -- Mar das Auroras (63) --
            Page(entries, RegionBossRules.TrappedAuroraId,
                "The aurora is light that dances in the northern sky, children, light that wants to go home. One night a piece of it fell, and the sea froze round it, and now it is stuck inside an iceberg in the middle of the Sea of Auroras. It doesn't mean to hurt anyone. It flails, the way a bird does in a closed room, and when it does, the ice cracks all round it. But even trapped it still dances, and when it dances it lights one road out across the ice, a different one each time. Get onto the lit road and the ice holds under you. Free it, and the aurora goes back up into the sky, and the whole harbour comes out to watch.",
                "A aurora é luz que dança no céu do norte, crianças, luz que quer voltar pra casa. Uma noite um pedaço dela caiu, e o mar congelou em volta, e agora ela está presa dentro de um iceberg no meio do Mar das Auroras. Ela não quer machucar ninguém. Ela se debate, igual passarinho numa sala fechada, e quando se debate o gelo racha todo em volta dela. Mas mesmo presa ela ainda dança, e quando dança acende um caminho pelo gelo, um diferente a cada vez. Subam no caminho aceso e o gelo aguenta debaixo de vocês. Libertem ela, e a aurora sobe de volta pro céu, e o porto inteiro sai pra ver.",
                ("Find the lit road, get on it, then hit her from there.", "Acha o caminho aceso, sobe nele, e bate dali."),
                ("She's not attacking. She's trying to get out. That's different.", "Ela não está atacando. Está tentando sair. É diferente."),
                ("The floor is lava, except it's ice, except it's cracking. Love this game.", "O chão é lava, só que é gelo, só que está rachando. Adoro esse jogo."),
                ("Even trapped, light still shows the way. That's what light is for.", "Mesmo presa, a luz ainda mostra o caminho. É pra isso que a luz serve."),
                ("It's so pretty! Green and purple! Can I keep a little bit in a jar?", "É muito bonita! Verde e roxa! Posso guardar um pouquinho num pote?"));
            Line(entries, RegionBossRules.TrappedAuroraId, RegionBossPhase.Storm, "The Trapped Aurora throws shards of light!", "A Aurora Presa atira lascas de luz!");
            Line(entries, RegionBossRules.TrappedAuroraId, RegionBossPhase.LastStand, "The iceberg is splitting open!", "O iceberg está se abrindo!");
            Blow(entries, RegionBossRules.TrappedAuroraId, "A wave of cold light!", "Uma onda de luz fria!");
            Signature(entries, RegionBossRules.TrappedAuroraId, "She's dancing! Get onto the lit road!", "Ela está dançando! Sobe no caminho aceso!");
            entries.Add(new LocalizationEntry(AuroraRouteKey, "This way!", "Por aqui!"));
            Withdraw(entries, RegionBossRules.TrappedAuroraId, "The iceberg splits in two and the light rises out of it, green and violet, and goes up and up until it is dancing in the sky with the rest of the aurora.", "O iceberg se parte em dois e a luz sobe de dentro dele, verde e violeta, e vai subindo até estar dançando no céu com o resto da aurora.");

            // -- Monte Echeyde (67) --
            Page(entries, RegionBossRules.GuayotaId,
                "The people of the island of the great volcano told of Guayota, a being of the deep who lived inside the mountain and one day stole the sun and shut it in there with him, and the whole world went dark until he was made to let it go. On Mount Echeyde he has done it again with a little piece of light, children: he keeps it in his chest and won't give it back. When the mountain rumbles he spits lava down the slope, and it falls on every other square of the ground, like the board of a game: the squares between stay cold, and the next time it's the other ones. Step onto a cold square. Free him, and at dawn the light comes out of the crater, and the shepherds blow their horns for it.",
                "O povo da ilha do grande vulcão contava do Guayota, um ser das profundezas que morava dentro da montanha e um dia roubou o sol e trancou lá dentro com ele, e o mundo inteiro ficou escuro até ele ser obrigado a soltar. No Monte Echeyde ele fez de novo com um pedacinho de luz, crianças: guarda no peito e não devolve. Quando a montanha ronca ele cospe lava pela encosta, e ela cai em um quadrado sim, um não do chão, igual tabuleiro de jogo: os quadrados do meio ficam frios, e na vez seguinte são os outros. Pisem num quadrado frio. Libertem ele, e de manhãzinha a luz sai pela cratera, e os pastores tocam as buzinas pra ela.",
                ("Hot square, cold square. Jump to the cold one, then hit him.", "Quadrado quente, quadrado frio. Pula pro frio, depois bate."),
                ("Keeping the light locked in the dark with you is the loneliest way to have it.", "Guardar a luz trancada no escuro com você é o jeito mais solitário de ter ela."),
                ("Hopscotch. On a volcano. With lava. Childhood ruined, thank you.", "Amarelinha. Num vulcão. Com lava. Infância arruinada, obrigado."),
                ("The pattern flips every time. Remember where you were and go to the other colour.", "O padrão vira toda vez. Lembra onde você estava e vai pra outra cor."),
                ("I'm the best at hopscotch! Watch me! ...Ow, hot, hot, hot!", "Eu sou a melhor na amarelinha! Olha! ...Ai, quente, quente, quente!"));
            Line(entries, RegionBossRules.GuayotaId, RegionBossPhase.Storm, "Guayota throws burning stones!", "Guayota atira pedras em brasa!");
            Line(entries, RegionBossRules.GuayotaId, RegionBossPhase.LastStand, "The whole mountain is rumbling!", "A montanha inteira está roncando!");
            Blow(entries, RegionBossRules.GuayotaId, "His fist of lava comes down!", "Lá vem o punho de lava!");
            Signature(entries, RegionBossRules.GuayotaId, "The crater spits! Find a cold square!", "A cratera cospe! Acha um quadrado frio!");
            Withdraw(entries, RegionBossRules.GuayotaId, "Guayota opens his chest, and the little light he kept rises out of the crater like a second sunrise. Then he sinks back into the mountain, quiet, and lets it go.", "Guayota abre o peito, e a luzinha que ele guardava sobe pela cratera como um segundo nascer do sol. Depois ele afunda de volta na montanha, quieto, e deixa ela ir.");

            // -- Anéis de Mehen (74) --
            Page(entries, RegionBossRules.WrongKnotId,
                "In the old books of Kemet, the great serpent Mehen coils round the barca of light in the hours of the night to keep the dark away from it. Its coils are a maze, and in one place they tied themselves wrong, children, and the shadow got into the knot. Now the Wrong Knot crawls through the coils looking for a way to untie itself, and it throws its loops out to either side of itself, across the line between it and you. Stay on that line, in front of it or behind it, and the loops close on nothing; step to the side and they catch you. Untie it, and the coil lies straight again round the barca, the way it should.",
                "Nos livros velhos de Kemet, a grande serpente Mehen se enrola em volta da barca de luz nas horas da noite pra manter o escuro longe. As voltas dela são um labirinto, e num lugar elas se amarraram errado, crianças, e a sombra entrou no nó. Agora o Nó Errado se arrasta pelas voltas procurando um jeito de se desamarrar, e joga as alças pros dois lados dele, atravessadas na linha entre ele e vocês. Fiquem nessa linha, na frente dele ou atrás, e as alças fecham no vazio; deem um passo pro lado e elas pegam vocês. Desamarrem ele, e a volta fica reta de novo em volta da barca, do jeito que deve ser.",
                ("Don't sidestep. Stay on his line and hit straight in.", "Não desvia pro lado. Fica na linha dele e bate de frente."),
                ("Imagine being tied up in yourself and not knowing how to get out.", "Imagina estar amarrado em si mesmo e não saber como sair."),
                ("I can't even tie my shoes properly, and now I'm untying a snake.", "Eu não sei nem amarrar o meu tênis direito, e agora vou desamarrar uma cobra."),
                ("Every instinct says step aside. The knot is counting on that.", "Todo instinto diz pra desviar. O nó conta com isso."),
                ("I know! You pull this end and that end! Like my hair in the morning!", "Eu sei! Puxa essa ponta e aquela ponta! Igual meu cabelo de manhã!"));
            Line(entries, RegionBossRules.WrongKnotId, RegionBossPhase.Storm, "The Wrong Knot spits dark!", "O Nó Errado cospe escuridão!");
            Line(entries, RegionBossRules.WrongKnotId, RegionBossPhase.LastStand, "It's pulling itself tighter!", "Ele está se apertando mais!");
            Blow(entries, RegionBossRules.WrongKnotId, "The coil lashes!", "A volta chicoteia!");
            Signature(entries, RegionBossRules.WrongKnotId, "It's throwing its loops! Stay on its line, don't step aside!", "Ele vai jogar as alças! Fica na linha dele, não desvia pro lado!");
            entries.Add(new LocalizationEntry(KnotLoopKey, "Loop!", "Alça!"));
            Withdraw(entries, RegionBossRules.WrongKnotId, "The Wrong Knot loosens with a long sigh, and the coil it was slides away straight and smooth, back round the barca of the night.", "O Nó Errado se afrouxa com um suspiro comprido, e a volta que ele era desliza embora reta e lisa, de volta em volta da barca da noite.");

            // -- Cova do Vento Destruidor (86) --
            Page(entries, RegionBossRules.DarkWingedEagleId,
                "Of the five shapes the Prince of Darkness wore, one had the wings of an eagle, children. This eagle is not him: it is a bird of the Pit of Ruinous Wind that grew those wings in the dark and keeps them, and they are too big and too heavy for it. When it swoops at you, the shadow of its wings falls in two long bars across its flight, where you stood and one wingbeat further on. Running to the side won't help, the wings are too wide. Step back, towards it, under it, and it flies right over you. Beaten, it sheds the dark wings and is just a bird again, and it flies up the shaft to the light.",
                "Das cinco formas que o Príncipe da Treva usava, uma tinha asas de águia, crianças. Esta águia não é ele: é uma ave da Cova do Vento Destruidor que ganhou aquelas asas no escuro e ficou com elas, e elas são grandes e pesadas demais pra ela. Quando ela mergulha em vocês, a sombra das asas cai em duas faixas compridas atravessadas no voo dela, onde vocês estavam e uma batida de asa mais adiante. Correr pro lado não adianta, as asas são largas demais. Recuem, na direção dela, pra debaixo dela, e ela passa direto por cima. Vencida, ela larga as asas escuras e vira ave de novo, e voa pelo poço acima até a luz.",
                ("When she dives, I step in under her. Not sideways. In.", "Quando ela mergulhar, eu entro debaixo dela. Pro lado não. Pra dentro."),
                ("Wings that aren't yours are a heavy thing to carry.", "Asas que não são suas são uma coisa pesada de carregar."),
                ("The safe place is underneath the giant eagle. Wonderful. Great. Amazing.", "O lugar seguro é debaixo da águia gigante. Maravilha. Ótimo. Incrível."),
                ("A shadow falls where the bird is going, not where it is. So stand where it is.", "A sombra cai pra onde a ave vai, não onde ela está. Então fica onde ela está."),
                ("If she gives the wings back, can I have them? I'll be careful!", "Se ela devolver as asas, eu posso ficar com elas? Eu tomo cuidado!"));
            Line(entries, RegionBossRules.DarkWingedEagleId, RegionBossPhase.Storm, "The Eagle throws dark feathers!", "A Águia atira penas de treva!");
            Line(entries, RegionBossRules.DarkWingedEagleId, RegionBossPhase.LastStand, "Her wings fill the whole pit!", "As asas dela enchem a cova inteira!");
            Blow(entries, RegionBossRules.DarkWingedEagleId, "Her talons come down!", "Lá vêm as garras!");
            Signature(entries, RegionBossRules.DarkWingedEagleId, "She's diving! Step in under her, not aside!", "Ela vai mergulhar! Entra debaixo dela, não pro lado!");
            Withdraw(entries, RegionBossRules.DarkWingedEagleId, "The dark wings fall off her like an old coat. Underneath she is a small grey eagle, and she rides the updraft out of the pit, calling.", "As asas escuras caem dela como um casaco velho. Por baixo ela é uma aguiazinha cinza, e sobe na corrente de ar pra fora da cova, chamando.");

            // -- Convés da Barca do Sol (97) --
            Page(entries, RegionBossRules.SailShadowId,
                "The Sun Ship carries the light home, children, the last stretch of the long journey, under a great sail like the boats of the eastern sea. A shadow hid under that sail, where the noon sun can't reach, and it makes the sail catch the wrong wind, so the ship stands still. When it swings the boom across the deck, it sweeps one side of the mast, then the other: starboard one time, port the next. Stand on the other side, or right up against the mast, under the boom, or out past its end. Beaten, the shadow slips over the side, the sail fills with the right wind, and the ship sails on.",
                "A Barca do Sol leva a luz pra casa, crianças, o último trecho da viagem comprida, debaixo de uma vela grande igual a dos barcos do mar do leste. Uma sombra se escondeu debaixo dessa vela, onde o sol do meio-dia não alcança, e faz a vela pegar o vento errado, então a barca fica parada. Quando ela gira a retranca pelo convés, varre um lado do mastro, depois o outro: estibordo uma vez, bombordo na outra. Fiquem do outro lado, ou bem colados no mastro, debaixo da retranca, ou depois da ponta dela. Vencida, a sombra escorrega pela borda, a vela enche com o vento certo, e a barca segue viagem.",
                ("Wrong side of the mast? Hug the mast. Then hit.", "Lado errado do mastro? Abraça o mastro. Depois bate."),
                ("Hiding in the one shade on a ship of the sun. I'd be scared too.", "Se escondendo na única sombra num barco do sol. Eu também ia ter medo."),
                ("Starboard, port. I'm going to get this wrong. I'm already getting it wrong.", "Estibordo, bombordo. Eu vou errar isso. Eu já estou errando."),
                ("A boom can only swing to one side at a time. Be on the other.", "Uma retranca só gira pra um lado de cada vez. Fica no outro."),
                ("Duck! Duck! ...That's the game, right? Duck, duck, shadow?", "Abaixa! Abaixa! ...É esse o jogo, né? Abaixa, abaixa, sombra?"));
            Line(entries, RegionBossRules.SailShadowId, RegionBossPhase.Storm, "The Shadow throws scraps of dark sail!", "A Sombra atira retalhos de vela escura!");
            Line(entries, RegionBossRules.SailShadowId, RegionBossPhase.LastStand, "The whole sail is flapping wild!", "A vela inteira está batendo doida!");
            Blow(entries, RegionBossRules.SailShadowId, "The sail cracks like a whip!", "A vela estala como um chicote!");
            Signature(entries, RegionBossRules.SailShadowId, "The boom is swinging! Other side, or hug the mast!", "A retranca vai girar! Pro outro lado, ou abraça o mastro!");
            entries.Add(new LocalizationEntry(BoomSideKey(true), "The boom swings to starboard! Go west or hug the mast!", "A retranca vai pra estibordo! Vai pro oeste ou abraça o mastro!"));
            entries.Add(new LocalizationEntry(BoomSideKey(false), "The boom swings to port! Go east or hug the mast!", "A retranca vai pra bombordo! Vai pro leste ou abraça o mastro!"));
            Withdraw(entries, RegionBossRules.SailShadowId, "The Shadow of the Sail slips over the rail into the bright air and is gone. The sail fills, the ship creaks, and the Sun Ship sails on with its light.", "A Sombra da Vela escorrega pela amurada para o ar claro e some. A vela enche, a barca range, e a Barca do Sol segue viagem com a sua luz.");
        }
    }
}
