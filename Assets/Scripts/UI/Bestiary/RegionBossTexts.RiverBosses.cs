using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of five map bosses of the Rio das Doze Horas and the Margens da Mistura (Docs/Mapas100.md 72, 75, 78, 80, 81) and the lines they
    /// call out, en + pt-BR. Like the others, each is a creature gone dark that the cousins free: its last line says what it goes back to.
    /// </summary>
    public static partial class RegionBossTexts
    {
        /// <summary>Which lobe of the Lake of Fire flares behind the Guardian (<see cref="RegionBossRules.LakeFlareOf"/>), called out as he warns.</summary>
        public static string LakeFlareKey(int flare)
        {
            switch (RegionBossRules.LakeFlareOf(flare))
            {
                case 0: return "region_boss.lake_guardian.flare.north";
                case 1: return "region_boss.lake_guardian.flare.east";
                default: return "region_boss.lake_guardian.flare.south";
            }
        }

        /// <summary>The label at the far end of the Guardian's shadow (<see cref="RegionBossRules.LakeShadowHits"/>).</summary>
        public const string LakeShadowKey = "region_boss.lake_guardian.shadow";

        /// <summary>The label over the instant the Lost Instant gives back (<see cref="RegionBossRules.InstantSpot"/>).</summary>
        public const string InstantSpotKey = "region_boss.lost_instant.spot";

        /// <summary>Which of the seven mantles Hobabish throws (<see cref="RegionBossRules.ThrowMantle"/>), called out as he warns.</summary>
        public static string MantleKey(int mantle) => $"region_boss.hobabish.mantle{System.Math.Abs(mantle) % RegionBossRules.MantleCount + 1}";

        /// <summary>How far Ullikummi has grown for this slam (<see cref="RegionBossRules.GrowthOf"/>), called out as he warns.</summary>
        public static string GrowthKey(int growth) => $"region_boss.ullikummi.growth{RegionBossRules.GrowthOf(growth) + 1}";

        private static void AddRiverMapBossPages(List<LocalizationEntry> entries)
        {
            // -- Campos de Juncos (72) --
            Page(entries, RegionBossRules.ReedHippoId,
                "In the old tales of Kemet, children, the Reed Fields are the calm place, where the wheat grows taller than a man and the canals carry water to every bed. The hippos of the river are their gardeners: they wallow in the mud and keep the channels open. The biggest of them all went dark, and now he tramples the canals instead of keeping them, and the fields go dry. When he goes under the water, a ripple rolls out from him, and it stops exactly as far away as you were standing when he went under. Step closer to him or farther away, and it passes; walking round him in a circle won't save you. Free him, and he will dig the canals open again with those big feet.",
                "Nos contos antigos de Kemet, crianças, os Campos de Juncos são o lugar calmo, onde o trigo cresce mais alto que gente e os canais levam água pra cada canteiro. Os hipopótamos do rio são os jardineiros de lá: rolam na lama e mantêm os canais abertos. O maior de todos apagou, e agora pisoteia os canais em vez de cuidar deles, e os campos secam. Quando ele afunda na água, uma onda sai dele em roda, e para exatamente na distância em que vocês estavam quando ele afundou. Cheguem mais perto dele ou se afastem, e ela passa; ficar dando a volta nele não adianta. Libertem ele, e ele vai cavar os canais de novo com aqueles pés enormes.",
                ("When he goes under, I take one big step in or one big step out. Never sideways.", "Quando ele afundar, eu dou um passo grande pra dentro ou pra fora. Nunca pro lado."),
                ("He's the gardener. He just forgot what the garden was for.", "Ele é o jardineiro. Só esqueceu pra que servia o jardim."),
                ("A hippo the size of a house, in a swamp, at night. Living the dream.", "Um hipopótamo do tamanho de uma casa, num brejo, de noite. Vivendo o sonho."),
                ("The ripple remembers your distance, not your place. So change the distance.", "A onda lembra a sua distância, não o seu lugar. Então muda a distância."),
                ("Can he give me a ride after? On his back? In the canal? Please?", "Ele me dá uma carona depois? Nas costas? No canal? Por favor?"));
            Line(entries, RegionBossRules.ReedHippoId, RegionBossPhase.Storm, "The Giant Reed Hippo spits mud!", "O Hipopótamo-de-Junco Gigante cospe lama!");
            Line(entries, RegionBossRules.ReedHippoId, RegionBossPhase.LastStand, "He's thrashing in the canal!", "Ele está se debatendo no canal!");
            Blow(entries, RegionBossRules.ReedHippoId, "His great jaws snap shut!", "A bocarra dele fecha com tudo!");
            Signature(entries, RegionBossRules.ReedHippoId, "He's going under! Step closer or farther, not round him!", "Ele vai afundar! Chega mais perto ou se afasta, não dá a volta!");
            Withdraw(entries, RegionBossRules.ReedHippoId, "The Giant Reed Hippo blows a long breath of bubbles, climbs out of the canal and starts pushing the mud back onto the banks, slowly, the way a gardener does.", "O Hipopótamo-de-Junco Gigante solta um bafo comprido de bolhas, sai do canal e começa a empurrar a lama de volta pras margens, devagar, do jeito que um jardineiro faz.");

            // -- Lago de Fogo (75) --
            Page(entries, RegionBossRules.LakeGuardianId,
                "The Lake of Fire is a strange lake, children: its fire burns the shadow and only warms whoever carries light. The baboons of the Palm Isle keep it, and they will wave to you. In the middle of it stood a giant ushabti, a stone servant made to answer when called and to let the light pass on to the travellers. The shadow got into him, and he stopped letting it pass. Now, when one of the three lobes of the lake flares, north, east or south, he stands in the way of its light, and his shadow falls long behind him, on the other side, and the dark fire falls with it. Stand in the light, on the side of the flare, or beside him. Free him, and he steps aside and opens the way again.",
                "O Lago de Fogo é um lago estranho, crianças: o fogo dele queima a sombra e só esquenta quem carrega luz. Os babuínos da Ilha das Palmeiras tomam conta dele, e eles acenam pra vocês. No meio dele ficava um ushabti gigante, um servo de pedra feito pra responder quando chamado e deixar a luz passar até os viajantes. A sombra entrou nele, e ele parou de deixar passar. Agora, quando um dos três braços do lago acende, ao norte, a leste ou ao sul, ele fica na frente da luz, e a sombra dele cai comprida atrás dele, do outro lado, e o fogo escuro cai junto. Fiquem na luz, do lado de onde o lago acendeu, ou do lado dele. Libertem ele, e ele dá um passo pro lado e abre o caminho de novo.",
                ("Look where the lake flares. Go to that side of him. Then hit.", "Olha onde o lago acende. Vai pra esse lado dele. Depois bate."),
                ("He was made to let the light through. Standing in its way must hurt him too.", "Ele foi feito pra deixar a luz passar. Ficar na frente dela deve doer nele também."),
                ("A lake of fire that's nice to us. Sure. I'm keeping my boots on.", "Um lago de fogo que é bonzinho com a gente. Tá bom. Eu vou ficar de bota."),
                ("A shadow always falls away from the light. Read the flare, and you know where it falls.", "Uma sombra sempre cai do lado contrário da luz. Lê o fogo, e você sabe onde ela cai."),
                ("The monkeys are waving at me! Hi, monkeys! Hiii!", "Os macacos estão acenando pra mim! Oi, macacos! Oiii!"));
            Line(entries, RegionBossRules.LakeGuardianId, RegionBossPhase.Storm, "The Guardian of the Lake throws burning shards!", "O Guardião do Lago atira lascas em brasa!");
            Line(entries, RegionBossRules.LakeGuardianId, RegionBossPhase.LastStand, "The whole lake is boiling round him!", "O lago inteiro está fervendo em volta dele!");
            Blow(entries, RegionBossRules.LakeGuardianId, "His stone fist comes down!", "Lá vem o punho de pedra!");
            Signature(entries, RegionBossRules.LakeGuardianId, "The lake flares! Get out of his shadow!", "O lago acendeu! Sai da sombra dele!");
            entries.Add(new LocalizationEntry(LakeFlareKey(0), "The lake flares to the north! His shadow falls south!", "O lago acendeu ao norte! A sombra dele cai pro sul!"));
            entries.Add(new LocalizationEntry(LakeFlareKey(1), "The lake flares to the east! His shadow falls west!", "O lago acendeu a leste! A sombra dele cai pro oeste!"));
            entries.Add(new LocalizationEntry(LakeFlareKey(2), "The lake flares to the south! His shadow falls north!", "O lago acendeu ao sul! A sombra dele cai pro norte!"));
            entries.Add(new LocalizationEntry(LakeShadowKey, "Shadow!", "Sombra!"));
            Withdraw(entries, RegionBossRules.LakeGuardianId, "The Guardian of the Lake lowers his arms and steps aside, slow as a door, and the light of the lake runs past him onto the path again. The baboons on the isle shout and clap.", "O Guardião do Lago abaixa os braços e dá um passo pro lado, devagar como uma porta, e a luz do lago corre por ele de novo até o caminho. Os babuínos da ilha gritam e batem palmas.");

            // -- Deserto do Relógio (78) --
            Page(entries, RegionBossRules.LostInstantId,
                "In the Desert of the Clock, children, twelve great gnomons lie in a ring, and their shadows tell the hours of Zurvan, which is what the old Persians called endless time: not a person, just the great clock everything runs by. One day the shadow stole a moment from that clock, and the moment went wrong, and now it walks the sand as the Lost Instant. It breaks every moment round it, all but one: the instant it stole from you the time before, the spot where you stood when it last began. That spot it marks on the sand, and it is the only safe place near it. Go back to it. Give the instant back to the clock, and the clock of Dur-Jarras will tell the right time again.",
                "No Deserto do Relógio, crianças, doze gnômons enormes estão caídos em roda, e as sombras deles contam as horas de Zurvan, que é como os persas antigos chamavam o tempo sem fim: não é gente, é só o grande relógio pelo qual tudo anda. Um dia a sombra roubou um momento desse relógio, e o momento deu errado, e agora anda pela areia como o Instante Perdido. Ele quebra todos os momentos em volta dele, menos um: o instante que ele roubou de vocês da vez anterior, o lugar onde vocês estavam quando ele começou da última vez. Esse lugar ele marca na areia, e é o único lugar seguro perto dele. Voltem pra lá. Devolvam o instante ao relógio, e o relógio de Dur-Jarras vai marcar a hora certa de novo.",
                ("Every time it starts, I remember where I'm standing. Next time, I go back there.", "Toda vez que ele começa, eu guardo onde estou. Na próxima, eu volto pra lá."),
                ("Imagine being a moment nobody gets to live. No wonder it's sad.", "Imagina ser um momento que ninguém chega a viver. Não é à toa que ele é triste."),
                ("I lose a whole afternoon every day and nobody makes a boss out of it.", "Eu perco uma tarde inteira todo dia e ninguém faz um chefe disso."),
                ("The safe place is always behind you in time. Follow the mark, not your instinct.", "O lugar seguro está sempre atrás de você no tempo. Segue a marca, não o instinto."),
                ("If I go back to where I was, am I younger? A little bit? One second?", "Se eu voltar pra onde eu estava, eu fico mais nova? Um pouquinho? Um segundo?"));
            Line(entries, RegionBossRules.LostInstantId, RegionBossPhase.Storm, "The Lost Instant throws splinters of time!", "O Instante Perdido atira lascas de tempo!");
            Line(entries, RegionBossRules.LostInstantId, RegionBossPhase.LastStand, "The shadows of the gnomons are running backwards!", "As sombras dos gnômons estão andando pra trás!");
            Blow(entries, RegionBossRules.LostInstantId, "The hour strikes!", "A hora bateu!");
            Signature(entries, RegionBossRules.LostInstantId, "It's breaking the moment! Go back to the instant it marked!", "Ele vai quebrar o momento! Volta pro instante que ele marcou!");
            entries.Add(new LocalizationEntry(InstantSpotKey, "Back here!", "Volta aqui!"));
            Withdraw(entries, RegionBossRules.LostInstantId, "The Lost Instant shivers like a heat haze and slips into the shadow of the nearest gnomon. Far away, in Dur-Jarras, the clock of the ziggurat ticks once, and it is right.", "O Instante Perdido treme como o ar quente e escorrega pra dentro da sombra do gnômon mais perto. Longe dali, em Dur-Jarras, o relógio do zigurate faz tique uma vez, e está certo.");

            // -- Cedral de Hobabish (80) --
            Page(entries, RegionBossRules.HobabishId,
                "In the Book of the Giants, children, there is Hobabish, the guardian of the cedar forest on the mountain, the same one the old songs of Gilgamesh call Humbaba. Seven mantles of brilliance kept him, and he kept the cedars. The shadow put him out, and now he thinks everyone who comes is a woodcutter. He throws his mantles at you one by one, where you stand, and the ones already on the ground stay there and blaze again each time he throws a new one. Keep off every mantle on the ground, old and new. The seven lanterns of resin in the forest each take a mantle off him when lit. Calm him, promise him you won't cut a single tree, and keep your promise: he will be a guardian again.",
                "No Livro dos Gigantes, crianças, tem o Hobabish, o guardião da floresta de cedros da montanha, o mesmo que as canções antigas de Gilgamesh chamam de Humbaba. Sete mantos de brilho guardavam ele, e ele guardava os cedros. A sombra apagou ele, e agora ele acha que todo mundo que chega é lenhador. Ele joga os mantos em vocês um por um, onde vocês estão, e os que já estão no chão ficam lá e brilham de novo toda vez que ele joga um novo. Fiquem fora de todos os mantos no chão, os velhos e o novo. As sete lanternas de resina da floresta, acesas, tiram um manto dele cada uma. Acalmem ele, prometam que não vão cortar árvore nenhuma, e cumpram a promessa: ele volta a ser guardião.",
                ("The mantles stay where they fall. I keep a map of them in my head.", "Os mantos ficam onde caem. Eu guardo um mapa deles na cabeça."),
                ("He's not angry. He's scared for his trees. I'd be scared too.", "Ele não está bravo. Está com medo pelas árvores dele. Eu também estaria."),
                ("I promise not to cut a tree. I don't even own an axe. Tell him!", "Eu prometo não cortar árvore. Eu nem tenho machado. Fala pra ele!"),
                ("Every throw adds to the floor. Fight him near where you haven't been.", "Cada arremesso soma no chão. Luta com ele perto de onde você ainda não esteve."),
                ("The mantles are so shiny! Can I wear one? Just for a minute?", "Os mantos são tão brilhantes! Posso vestir um? Só um minutinho?"));
            Line(entries, RegionBossRules.HobabishId, RegionBossPhase.Storm, "Hobabish throws cones of burning resin!", "Hobabish atira pinhas de resina em brasa!");
            Line(entries, RegionBossRules.HobabishId, RegionBossPhase.LastStand, "The whole forest is roaring with him!", "A floresta inteira está rugindo com ele!");
            Blow(entries, RegionBossRules.HobabishId, "His great hand comes down like a falling cedar!", "A mãozona dele cai como um cedro tombando!");
            Signature(entries, RegionBossRules.HobabishId, "He's throwing a mantle! Keep off every mantle on the ground!", "Ele vai jogar um manto! Fica fora de todo manto no chão!");
            string[] ordinalsEn = { "first", "second", "third", "fourth", "fifth", "sixth", "seventh" };
            string[] ordinalsPt = { "primeiro", "segundo", "terceiro", "quarto", "quinto", "sexto", "sétimo" };
            for (int i = 0; i < RegionBossRules.MantleCount; i++)
            {
                entries.Add(new LocalizationEntry(MantleKey(i),
                    $"The {ordinalsEn[i]} mantle! Keep off all the mantles on the ground!",
                    $"O {ordinalsPt[i]} manto! Fica fora de todos os mantos no chão!"));
            }

            Withdraw(entries, RegionBossRules.HobabishId, "Hobabish looks at the cousins for a long time, then at the cedars, and sits down among them with a sigh that shakes the needles. The mantles settle on his shoulders again, soft now, and he lets you pass.", "Hobabish olha pros primos por muito tempo, depois pros cedros, e senta no meio deles com um suspiro que balança as agulhas. Os mantos voltam pros ombros dele, macios agora, e ele deixa vocês passarem.");

            // -- Ombro de Upelluri (81) --
            Page(entries, RegionBossRules.UllikummiId,
                "The Hurrian songs tell of Upelluri, children, the giant who carries the sky and the earth on his shoulder and never notices anything. On that shoulder a stone of diorite was set, and it grew, and grew, and its name was Ullikummi, and it wanted to grow until it covered the sky. The shadow is what makes it keep growing. Every time it slams the shoulder it has grown a little more, and its slam reaches farther: first close, then farther, then farther still, until it cracks and starts small again. At night it grows more. Right up against its foot, under the stone that already grew, the slam passes over you; and beyond its reach you are safe too. Beat it, and it stops growing and settles into a mountain at last.",
                "As canções hurritas contam do Upelluri, crianças, o gigante que carrega o céu e a terra no ombro e nunca percebe nada. Nesse ombro puseram uma pedra de diorito, e ela cresceu, e cresceu, e o nome dela era Ullikummi, e ela queria crescer até tapar o céu. É a sombra que faz ela continuar crescendo. Toda vez que bate no ombro ela cresceu mais um pouco, e a batida dela alcança mais longe: primeiro perto, depois mais longe, depois mais longe ainda, até rachar e começar pequena de novo. De noite ela cresce mais. Bem colado no pé dela, debaixo da pedra que já cresceu, a batida passa por cima; e depois do alcance vocês também estão seguros. Vençam ela, e ela para de crescer e vira montanha de vez.",
                ("Each slam reaches farther. Either I'm hugging its foot or I'm well out.", "Cada batida vai mais longe. Ou eu estou abraçando o pé dela, ou bem longe."),
                ("Growing forever, and never asking why. That sounds lonely.", "Crescer pra sempre, sem nunca perguntar por quê. Parece solitário."),
                ("We're fighting a rock. On a giant. Who doesn't know we're here. Okay.", "A gente está lutando com uma pedra. Em cima de um gigante. Que nem sabe que a gente está aqui. Tá."),
                ("Count the slams. It grows on a pattern, and the pattern starts over when it cracks.", "Conta as batidas. Ela cresce num padrão, e o padrão recomeça quando ela racha."),
                ("If it becomes a mountain, can I climb it? Can I name it? Mount Lavine!", "Se ela virar montanha, posso escalar? Posso dar nome? Monte Lavine!"));
            Line(entries, RegionBossRules.UllikummiId, RegionBossPhase.Storm, "Ullikummi throws chips of diorite!", "Ullikummi atira lascas de diorito!");
            Line(entries, RegionBossRules.UllikummiId, RegionBossPhase.LastStand, "The stone is reaching for the sky!", "A pedra está esticando até o céu!");
            Blow(entries, RegionBossRules.UllikummiId, "A pillar of diorite comes down!", "Lá vem um pilar de diorito!");
            Signature(entries, RegionBossRules.UllikummiId, "It's growing! Hug its foot or get out of its reach!", "Ela está crescendo! Abraça o pé dela ou sai do alcance!");
            entries.Add(new LocalizationEntry(GrowthKey(0), "It grows! The slam reaches close! Hug its foot or step out!", "Ela cresce! A batida alcança perto! Abraça o pé ou sai!"));
            entries.Add(new LocalizationEntry(GrowthKey(1), "It grows more! The slam reaches farther! Hug its foot or step out!", "Ela cresce mais! A batida vai mais longe! Abraça o pé ou sai!"));
            entries.Add(new LocalizationEntry(GrowthKey(2), "It keeps growing! The slam reaches far! Hug its foot or run out!", "Ela continua crescendo! A batida vai longe! Abraça o pé ou corre pra fora!"));
            entries.Add(new LocalizationEntry(GrowthKey(3), "It's huge now! The slam reaches very far! Hug its foot!", "Ela está enorme! A batida vai muito longe! Abraça o pé dela!"));
            Withdraw(entries, RegionBossRules.UllikummiId, "Ullikummi cracks from top to bottom with a sound like thunder far away, and stops. Moss starts to creep up it. Upelluri, under it all, shrugs in his sleep, and does not notice.", "Ullikummi racha de cima a baixo com um barulho de trovão longe, e para. O musgo começa a subir por ela. Upelluri, embaixo de tudo, mexe o ombro dormindo, e não percebe.");
        }
    }
}
