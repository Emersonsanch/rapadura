using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the beasts of the late continuous worlds (Docs/Mapas100.md 25–40: Costa das Monções,
    /// Delta de Jhilpur, Teto do Mundo) that the overworld layouts used to play with stand-ins: lore in Vó Edna's voice and
    /// one line per cousin, en + pt-BR. Kept apart like <see cref="OverworldBestiaryTexts"/>; <see cref="BestiaryTexts"/>
    /// appends these to its <see cref="BestiaryTexts.Entries"/>.
    /// </summary>
    public static class LateRegionBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_forest_mangrove_crocodile", "enemy_forest_grass_lion", "enemy_forest_dust_rhino",
            "enemy_forest_thief_monkey", "enemy_forest_thief_otter", "enemy_forest_dimmed_python", "enemy_forest_dimmed_troll",
            "enemy_mountain_brown_bear", "enemy_mountain_wild_yak", "enemy_mountain_irbis", "enemy_mountain_almas",
            "enemy_mountain_giant_marmot",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Costa das Monções (listed with the Living Water) --
            Page(entries, "enemy_forest_mangrove_crocodile",
                "The crocodiles of the mangroves are older than the dhows, older than the coral town. They used to lie in the warm mud with their mouths open, letting little birds clean their teeth. Then the Mire came up the channels and filled their bellies with cold, and now they swallow whatever comes near: sparks, nets, a whole oar. Only their eyes stick out of the water, so look for two bumps that don't float away. They are slow to turn: go round to the side, never in front of that mouth.",
                "Os crocodilos do mangue são mais velhos que os dhows, mais velhos que a cidade de coral. Eles ficavam deitados na lama morna de boca aberta, deixando os passarinhos limparem os dentes. Aí o Lodo subiu pelos canais e encheu a barriga deles de frio, e agora engolem tudo o que chega perto: faísca, rede, um remo inteiro. Só os olhos ficam pra fora d'água, então procurem dois calombos que não saem boiando. Eles demoram pra virar: deem a volta pelo lado, nunca na frente daquela boca.",
                ("Two bumps on the water. I aim between them.", "Dois calombos na água. Eu miro no meio deles."),
                ("Nobody puts a hand near that mouth. Nobody. Not even to help.", "Ninguém põe a mão perto daquela boca. Ninguém. Nem pra ajudar."),
                ("It bit my shield and kept it a while. I asked nicely and it gave it back.", "Ele mordeu o meu escudo e ficou com ele um tempo. Pedi com educação e ele devolveu."),
                ("Eighty teeth and it can't chew. It just swallows. Terrible engineering.", "Oitenta dentes e não sabe mastigar. Só engole. Péssima engenharia."),
                ("I thought it was a log. I sat on it. It was not a log.", "Eu achei que era um tronco. Sentei nele. Não era um tronco."));

            Page(entries, "enemy_forest_grass_lion",
                "In the Aurochs Savanna the lions sleep all day under the acacias and only hunt when the grass is cool. These ones never sleep anymore. The Devouring Fire passed through the dry grass and stayed in their eyes, and now they run through the burned strips hungry for the heat. They are fast and come from the side, the mane low in the grass. Stand on the strips the bull-folk burned: the grass there is short and you see them coming.",
                "Na Savana dos Auroques os leões dormem o dia inteiro debaixo das acácias e só caçam quando o capim esfria. Esses aí não dormem mais. O Fogo Devorador passou pelo capim seco e ficou nos olhos deles, e agora correm pelas faixas queimadas com fome de calor. São ligeiros e vêm de lado, a juba baixinha no capim. Fiquem nas faixas que o povo-touro queimou: ali o capim é curto e dá pra ver eles chegando.",
                ("He hides in the grass. I'll burn the grass... no. I'll wait for the mane.", "Ele se esconde no capim. Vou queimar o capim... não. Vou esperar a juba aparecer."),
                ("Poor thing, he hasn't slept in weeks. After this, a long nap.", "Coitado, ele não dorme faz semanas. Depois disso, um cochilo bem comprido."),
                ("He roared at me. I roared back. Mine was worse, but louder.", "Ele rugiu pra mim. Eu rugi de volta. O meu foi pior, mas mais alto."),
                ("A mane is a scarf you can't take off. In this heat? No wonder he's angry.", "Juba é um cachecol que não sai. Nesse calor? Não admira que ele esteja bravo."),
                ("Can I keep him if he gets better? Vó says no. Vó always says no.", "Posso ficar com ele se ele melhorar? A Vó diz que não. A Vó sempre diz que não."));

            Page(entries, "enemy_forest_dust_rhino",
                "The dust rhino rolls in the red earth of the savanna until it is the colour of the road. Enmerkar of Aratta says they were the gentlest beasts of the plain, eating thorns and minding their own business. Then a crack of trapped light opened in their backs, and it hurts, so they charge at anything that moves. When the head goes down, it is coming: step aside at the last moment and let it run past.",
                "O rinoceronte-de-pó rola na terra vermelha da savana até ficar da cor da estrada. O Enmerkar de Aratta diz que eles eram os bichos mais mansos da planície, comendo espinho e cuidando da própria vida. Aí abriu nas costas deles uma rachadura de luz presa, e dói, então eles correm em cima de tudo o que se mexe. Quando a cabeça abaixa, ele vem: saiam de lado no último instante e deixem ele passar direto.",
                ("Head down means run. Then it's my turn.", "Cabeça baixa quer dizer corre. Depois é a minha vez."),
                ("That crack on its back hurts. When it's free, I'll put ointment on it.", "Essa rachadura nas costas dói. Quando ele soltar, eu passo pomada."),
                ("I stood my ground. Then I stood somewhere else, very fast.", "Eu fiquei firme. Depois fiquei em outro lugar, bem rápido."),
                ("Two tonnes, top speed fifty, turning radius terrible. Dodge left.", "Duas toneladas, cinquenta por hora, faz curva muito mal. Desvia pra esquerda."),
                ("It's like a rock that's angry. Rocks shouldn't be angry.", "Parece uma pedra brava. Pedra não devia ficar brava."));

            // -- Delta de Jhilpur (listed with the Living Water) --
            Page(entries, "enemy_forest_thief_monkey",
                "The monkeys of the root bridges are grey with black faces, and Salmaios of Jhilpur says they used to help the bridge-growers, carrying vines from one side of the gorge to the other. When Āz the greedy passed by, she showed them her shiny things, and now they steal bangles, bells and coins and run off along the bridges laughing. They don't want to hurt you, they want your pockets. Hold on to your things and they will lose interest.",
                "Os macacos das pontes de raiz são cinzentos de cara preta, e o Salmaios de Jhilpur diz que eles ajudavam quem fazia as pontes crescerem, levando cipó de um lado da garganta pro outro. Quando a Āz, aquela gulosa, passou por lá, mostrou as coisas brilhantes dela, e agora eles roubam pulseira, sininho e moeda e saem correndo pelas pontes dando risada. Eles não querem machucar ninguém, querem os bolsos de vocês. Segurem as coisas e eles perdem o interesse.",
                ("He took my bowstring wax. That's war.", "Ele levou a cera da minha corda. Agora é guerra."),
                ("He has a bangle on his tail! Let's find out whose it is and give it back.", "Ele tá com uma pulseira no rabo! Vamos descobrir de quem é e devolver."),
                ("He jumped on my head. Then on my shield. Then he was gone.", "Ele pulou na minha cabeça. Depois no meu escudo. Depois sumiu."),
                ("A tail that grabs things. If I had one of those I'd never drop a tool.", "Um rabo que segura coisas. Se eu tivesse um desses nunca derrubava ferramenta."),
                ("He stole my candy and ate it in front of me. That's the worst kind of thief.", "Ele roubou a minha bala e comeu na minha frente. Esse é o pior tipo de ladrão."));

            Page(entries, "enemy_forest_thief_otter",
                "The otters of Jhilpur's banks are the best fishers of the delta, better than the boatmen, and the boatmen don't mind: the otters always left them half. Now the Mire has made them greedy. They steal the lantern-lit fish right out of the baskets, the glowing ones, and they won't share anymore. They are quick in the water and quick on the mud, and they run when they are hurt. Let them go: they come back when they're hungry.",
                "As lontras das margens de Jhilpur são as melhores pescadoras do delta, melhores que os barqueiros, e os barqueiros nem ligam: as lontras sempre deixavam metade pra eles. Agora o Lodo deixou elas gulosas. Roubam os peixes iluminados de dentro dos cestos, aqueles que brilham, e não dividem mais. São ligeiras na água e ligeiras na lama, e fogem quando se machucam. Deixem ir: elas voltam quando ficam com fome.",
                ("It's holding a fish that glows. Easy target in the dark.", "Ela tá segurando um peixe que brilha. Alvo fácil no escuro."),
                ("Look at those whiskers. How can something so cute steal?", "Olha esses bigodinhos. Como uma coisa tão fofa pode roubar?"),
                ("It slipped right through my legs. Twice.", "Ela passou no meio das minhas pernas. Duas vezes."),
                ("Waterproof fur, built-in paddles. I'd like the diagram, please.", "Pelo à prova d'água, remo embutido. Eu queria o desenho técnico, por favor."),
                ("She splashed me on purpose. I saw her smile.", "Ela me molhou de propósito. Eu vi ela sorrindo."));

            Page(entries, "enemy_forest_dimmed_python",
                "Pythons don't bite to kill, they hug. In the rain forest of the gorges they used to coil around the roots and keep them warm at night, and the bridges grew strong. The shadow got into one, then another, and now they hug anything that walks on the bridges, tight and cold. Watch the head: it pulls back before it strikes, then wraps. If it catches someone, everyone hits it at once and it lets go.",
                "Píton não morde pra matar, ela abraça. Na floresta de chuva das gargantas elas se enrolavam nas raízes e esquentavam elas de noite, e as pontes cresciam fortes. A sombra entrou numa, depois em outra, e agora elas abraçam tudo o que anda nas pontes, apertado e frio. Olhem a cabeça: ela recua antes do bote, depois enrola. Se pegar alguém, todo mundo bate junto e ela solta.",
                ("Long body, small head. I aim at the small part.", "Corpo comprido, cabeça pequena. Eu miro na parte pequena."),
                ("If she hugs you, breathe out and don't panic. I'm coming.", "Se ela te abraçar, solta o ar e não entra em pânico. Eu tô indo."),
                ("She hugged me. I hugged back harder. She let go.", "Ela me abraçou. Eu abracei mais forte. Ela soltou."),
                ("No legs and still climbs trees. Physics is crying.", "Sem perna nenhuma e ainda sobe em árvore. A física tá chorando."),
                ("I don't like hugs from things without arms.", "Eu não gosto de abraço de coisa que não tem braço."));

            Page(entries, "enemy_forest_dimmed_troll",
                "The trolls of Tarchon are big, slow, kind fishermen who weave baskets and sing to the will-o'-wisps. Some of them went out into the swamp to catch a wisp for their lamps and came back without remembering the way home: the light in the cage on their backs is a trapped spark, and it whispers. They hit hard and don't run. When you beat one, the cage opens, the spark flies off, and he blinks and asks where Tarchon is. Point the way.",
                "Os trols de Tarchon são pescadores grandes, lentos e bondosos, que trançam cesto e cantam pros fogos-fátuos. Alguns saíram pro pântano pra pegar um fogo-fátuo pras lamparinas e voltaram sem lembrar o caminho de casa: a luz na gaiola das costas deles é uma faísca presa, e ela sussurra. Batem forte e não fogem. Quando a gente vence um, a gaiola abre, a faísca sai voando, e ele pisca e pergunta onde fica Tarchon. Mostrem o caminho.",
                ("Aim for the cage. The spark wants out as much as we do.", "Mirem na gaiola. A faísca quer sair tanto quanto a gente."),
                ("He's someone's neighbour. Larthia is waiting for him. Gently.", "Ele é vizinho de alguém. A Larthia tá esperando ele. Com jeito."),
                ("He's bigger than me. He's slower than me. I like those odds.", "Ele é maior que eu. Ele é mais lento que eu. Gosto dessa conta."),
                ("A wicker cage holding pure light. Who made the latch? I need to know.", "Uma gaiola de vime segurando luz pura. Quem fez a trava? Preciso saber."),
                ("He has grass growing on his head. I want grass on my head.", "Ele tem mato crescendo na cabeça. Eu quero mato na minha cabeça."));

            // -- Teto do Mundo (listed with the mountains) --
            Page(entries, "enemy_mountain_brown_bear",
                "The brown bears of the Pamir come down to the apricot valley every summer to eat the fallen fruit, and the people of Sangkent leave the low branches for them. This year the cold of the shadow got into their winter sleep, and they woke up too early, grumpy and hungry, with a frost on their backs that never melts. They are strong and slow to anger, and very slow to calm down. Don't run from a bear: back away, together, and only fight if it comes.",
                "Os ursos-pardos do Pamir descem pro vale dos damasqueiros todo verão pra comer a fruta caída, e o povo de Sangkent deixa os galhos de baixo pra eles. Este ano o frio da sombra entrou no sono de inverno deles, e eles acordaram cedo demais, emburrados e com fome, com uma geada nas costas que nunca derrete. São fortes e demoram pra ficar bravos, e demoram muito mais pra acalmar. Não corram de urso: andem pra trás, juntos, e só briguem se ele vier.",
                ("Back away slowly. If he comes, I won't miss.", "Andem pra trás devagar. Se ele vier, eu não erro."),
                ("He woke up too early. I'd be grumpy too.", "Ele acordou cedo demais. Eu também ia ficar de mau humor."),
                ("He stood up. He's taller than me standing up. That's rude.", "Ele ficou em pé. Em pé ele é mais alto que eu. Que falta de educação."),
                ("Frost that never melts on a warm animal. That's not weather, that's the shadow.", "Geada que não derrete num bicho quente. Isso não é tempo, é a sombra."),
                ("Can we give him apricots? Bears like apricots. I'll throw from far.", "A gente pode dar damasco pra ele? Urso gosta de damasco. Eu jogo de longe."));

            Page(entries, "enemy_mountain_wild_yak",
                "The wild yak is the tame yak's big cousin, black and shaggy, with a skirt of hair that sweeps the snow and horns as wide as a door. The herders of the Pamir say a wild yak never attacks first. These ones do: the shadow froze the warmth in their hearts, and they charge the caravans on the glacier as if the caravans were wolves. When the head drops, the horns are coming. Get behind a rock, and hit the flank when it passes.",
                "O iaque selvagem é o primo grandão do iaque manso, preto e peludo, com uma saia de pelo que varre a neve e chifres da largura de uma porta. Os pastores do Pamir dizem que iaque selvagem nunca ataca primeiro. Esses atacam: a sombra congelou o calor do coração deles, e eles correm em cima das caravanas na geleira como se as caravanas fossem lobos. Quando a cabeça abaixa, os chifres vêm. Fiquem atrás de uma pedra e batam no flanco quando ele passar.",
                ("All that hair and a tiny spot to aim at. Challenge accepted.", "Esse pelo todo e um pontinho pra mirar. Desafio aceito."),
                ("His heart is cold. We'll warm it up again, I promise.", "O coração dele tá gelado. A gente esquenta de novo, prometo."),
                ("Horns as wide as a door. I'm as wide as half a door. Hmm.", "Chifre da largura de uma porta. Eu sou da largura de meia porta. Hum."),
                ("That hair skirt is insulation. I'm writing this down for the winter clothes.", "Essa saia de pelo é isolamento térmico. Vou anotar pras roupas de inverno."),
                ("When he's good again, I'm riding him. Tell Vó later.", "Quando ele ficar bom, eu vou montar nele. Contem pra Vó depois."));

            Page(entries, "enemy_mountain_irbis",
                "The irbis is the snow leopard, the ghost of the high cliffs: pale grey with dark rosettes, a tail as long as its body that it wraps around its nose to sleep. You almost never see one. These ones you will see, because the shadow made them bold and hungry, and they hunt the caravans in the Wakhan Pass. They jump from above and fight fast. Look up on the cliffs, and keep your backs together.",
                "O irbis é o leopardo-das-neves, o fantasma dos penhascos altos: cinza-claro de rosetas escuras, com um rabo do tamanho do corpo que ele enrola no focinho pra dormir. Quase ninguém vê um. Esses vocês vão ver, porque a sombra deixou eles atrevidos e famintos, e eles caçam as caravanas no Passo de Wakhan. Pulam de cima e brigam ligeiro. Olhem pra cima nos penhascos, e fiquem de costas uns pros outros.",
                ("He hides on the rocks. I see the tail first. Always the tail.", "Ele se esconde nas pedras. Eu vejo o rabo primeiro. Sempre o rabo."),
                ("So beautiful. Let's free him quickly so he can go back to hiding.", "Tão bonito. Vamos soltar ele rápido pra ele voltar a se esconder."),
                ("He jumped on my shield from above. Shields go up too, cat.", "Ele pulou no meu escudo de cima. Escudo também sobe, gato."),
                ("That tail is a counterweight. He balances like a crane on a building site.", "Esse rabo é contrapeso. Ele se equilibra igual guindaste de obra."),
                ("He's a giant kitty. A giant, angry kitty. I'm going to pet him after.", "Ele é um gatinho gigante. Um gatinho gigante e bravo. Depois eu vou fazer carinho."));

            Page(entries, "enemy_mountain_almas",
                "The almas is the hairy wild man of the mountains, the one the caravan people tell stories about: tall, stooped, with feet bigger than a bucket, so shy he hides behind the seracs when anyone passes. When the shadow came to the glacier, it frightened the almas, and frightened people do strange things. Now they throw snowballs packed hard as stone at anyone who comes close, and run away when they are hurt. Be patient and gentle: an almas that isn't afraid becomes a friend.",
                "O almas é o homem selvagem peludo das montanhas, aquele das histórias do povo das caravanas: alto, curvado, com pé maior que um balde, tão tímido que se esconde atrás dos seracs quando alguém passa. Quando a sombra chegou na geleira, ela assustou os almas, e gente assustada faz coisa estranha. Agora eles jogam bola de neve socada, dura feito pedra, em quem chega perto, e fogem quando se machucam. Paciência e jeito: almas que não tem medo vira amigo.",
                ("He throws better than most archers I know. Don't tell them.", "Ele arremessa melhor que muito arqueiro que eu conheço. Não contem pra eles."),
                ("He's scared. Talk softly. Let me go first.", "Ele tá com medo. Falem baixinho. Deixa eu ir primeiro."),
                ("A snowball hit my shield and the shield went numb. How?", "Uma bola de neve acertou o meu escudo e o escudo ficou dormente. Como?"),
                ("Those footprints! We'll be able to track him from a kilometre away.", "Essas pegadas! Dá pra seguir ele a um quilômetro de distância."),
                ("He's like a big hairy uncle. A big hairy uncle who throws snow.", "Ele parece um tio grandão e peludo. Um tio grandão e peludo que joga neve."));

            Page(entries, "enemy_mountain_giant_marmot",
                "The marmots of the Pamir whistle from their holes to warn each other when an eagle passes, and the shepherds of Sangkent use the whistles as a clock. These grew big as dogs when the shadow passed through the terraces, and greedy too: they dig into the caravans' bags with those orange teeth and fight over the apricots. They come in families and run when they are hurt. Listen for the whistle: when it stops, they're coming.",
                "As marmotas do Pamir assobiam da toca pra avisar umas às outras quando passa uma águia, e os pastores de Sangkent usam os assobios como relógio. Essas ficaram do tamanho de um cachorro quando a sombra passou pelos terraços, e gulosas também: cavam a sacola das caravanas com aqueles dentes cor de laranja e brigam pelos damascos. Vêm em família e fogem quando se machucam. Escutem o assobio: quando ele para, elas estão vindo.",
                ("They whistle before they come. Thank you for the warning.", "Elas assobiam antes de vir. Obrigada pelo aviso."),
                ("They're just chubby and hungry. Like Joaquim.", "Elas só estão gordinhas e com fome. Igual ao Joaquim."),
                ("Hey! I heard that, Maria.", "Ei! Eu ouvi isso, Maria."),
                ("Teeth that never stop growing. They have to chew to survive. Fascinating.", "Dente que nunca para de crescer. Elas têm que roer pra sobreviver. Fascinante."),
                ("I whistled and three of them came. I'm not whistling anymore.", "Eu assobiei e vieram três. Não assobio mais."));
        }

        /// <summary>Lore + one comment per cousin (Maithe, Maria, Joaquim, Ícaro, Lavine, in that order).</summary>
        private static void Page(
            List<LocalizationEntry> entries, string enemyId, string loreEn, string lorePt,
            (string en, string pt) maithe, (string en, string pt) maria, (string en, string pt) joaquim,
            (string en, string pt) icaro, (string en, string pt) lavine)
        {
            entries.Add(new LocalizationEntry(BestiaryCatalog.LoreKey(enemyId), loreEn, lorePt));
            (string en, string pt)[] comments = { maithe, maria, joaquim, icaro, lavine };
            for (int i = 0; i < BestiaryTexts.Cousins.Length; i++)
            {
                entries.Add(new LocalizationEntry(BestiaryCatalog.CommentKey(enemyId, BestiaryTexts.Cousins[i]), comments[i].en, comments[i].pt));
            }
        }
    }
}
