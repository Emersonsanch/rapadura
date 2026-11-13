using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the last four map bosses of the Reinos do Céu (Docs/Mapas100.md 92, 94, 98, 99) and the lines they call out, en + pt-BR.
    /// Like the others, each is a creature gone dark that the cousins free: its last line says what it goes back to.
    /// </summary>
    public static partial class RegionBossTexts
    {
        /// <summary>The label at the downstream end of the Lost Rower's wake (<see cref="RegionBossRules.RowerWakeHits"/>).</summary>
        public const string RowerWakeKey = "region_boss.lost_rower.wake";

        /// <summary>The label over the jar that keeps turning when the Archon of the Wheel locks it (<see cref="RegionBossRules.WheelLitJar"/>).</summary>
        public const string WheelJarKey = "region_boss.wheel_archon.jar";

        /// <summary>The label on the step below the Last Archon of the Step, where it is safe (<see cref="RegionBossRules.HeldStepHits"/>).</summary>
        public const string StepBelowKey = "region_boss.last_step_archon.below";

        /// <summary>How wide the Great Crack opens this drip (<see cref="RegionBossRules.CrackHalfWidthOf"/>), called out as it warns.</summary>
        public static string CrackWidthKey(int drip) => $"region_boss.great_crack.width{System.Math.Abs(drip) % RegionBossRules.CrackHalfWidths.Length}";

        private static void AddSkyMapBossPages(List<LocalizationEntry> entries)
        {
            // -- Rio de Luz (92) --
            Page(entries, RegionBossRules.LostRowerId,
                "The barcas of light don't row themselves, children. Rowers made of light pull the oars, all the way from the Cais up the River of Light, carrying the light that was rescued down below back home. One of them stopped to look at the shadow in the water, and the shadow made him forget the way. Now the Lost Rower rows in circles and won't let anyone pass. When he pulls on his oar, the wake runs from where you stand down the river, with the current, the way the river always goes. Step upstream, against the current, or out to the side, and the wake goes by. Free him, and he remembers the way and rows back to his barca.",
                "As barcas de luz não remam sozinhas, crianças. Remadores feitos de luz puxam os remos, desde o Cais até o Rio de Luz acima, levando pra casa a luz que foi resgatada lá embaixo. Um deles parou pra olhar a sombra na água, e a sombra fez ele esquecer o caminho. Agora o Remador Perdido rema em círculos e não deixa ninguém passar. Quando ele puxa o remo, a esteira corre de onde vocês estão rio abaixo, com a correnteza, pro lado que o rio sempre vai. Deem um passo rio acima, contra a correnteza, ou pro lado, e a esteira passa. Libertem ele, e ele lembra o caminho e volta remando pra barca dele.",
                ("When he pulls the oar, I go upstream. Then I hit him.", "Quando ele puxar o remo, eu vou rio acima. Depois eu bato."),
                ("Forgetting the way home is the worst kind of lost.", "Esquecer o caminho de casa é o pior jeito de estar perdido."),
                ("A rower who forgot how rivers work. Relatable, honestly.", "Um remador que esqueceu como rio funciona. Me identifico, sinceramente."),
                ("The wake always goes with the current. East. Never against it.", "A esteira vai sempre com a correnteza. Pro leste. Nunca contra."),
                ("Can I row too? I'll go straight! Well, almost straight!", "Posso remar também? Eu vou reto! Bom, quase reto!"));
            Line(entries, RegionBossRules.LostRowerId, RegionBossPhase.Storm, "The Lost Rower splashes drops of light!", "O Remador Perdido espirra gotas de luz!");
            Line(entries, RegionBossRules.LostRowerId, RegionBossPhase.LastStand, "He's rowing harder and harder!", "Ele está remando cada vez mais forte!");
            Blow(entries, RegionBossRules.LostRowerId, "The oar comes down!", "Lá vem o remo!");
            Signature(entries, RegionBossRules.LostRowerId, "He pulls the oar! Go upstream or aside!", "Ele puxa o remo! Vai rio acima ou pro lado!");
            entries.Add(new LocalizationEntry(RowerWakeKey, "Wake!", "Esteira!"));
            Withdraw(entries, RegionBossRules.LostRowerId, "The Lost Rower stops, looks up the river as if he were seeing it for the first time, and rows away upstream, steady and sure, towards the barcas.", "O Remador Perdido para, olha o rio acima como se estivesse vendo pela primeira vez, e vai remando rio acima, firme e certo, na direção das barcas.");

            // -- Roda Celeste das Doze Jarras (94) --
            Page(entries, RegionBossRules.WheelArchonId,
                "Up in the sky there is a great wheel with twelve jars, children, and as it turns the jars fill with the light that was set free and lift it up towards the Moon. The old books say so. An archon sat on the hub and locked it so it would not turn, because a wheel that does not turn lifts nothing. When the Archon of the Wheel locks it, every jar round him spills, all but one: the jar you were standing by, which keeps turning and comes round a quarter of the wheel. Go where your jar went, or come right in to the hub under him, which never turns. Free him, and the wheel turns again, and the light goes up.",
                "Lá no céu tem uma roda enorme com doze jarras, crianças, e conforme ela gira as jarras se enchem com a luz que foi libertada e levam ela pra cima, pra Lua. Os livros antigos dizem assim. Um arconte sentou no eixo e travou a roda pra ela não girar, porque roda que não gira não leva nada. Quando o Arconte da Roda trava, todas as jarras em volta dele derramam, menos uma: a jarra do lado de onde vocês estavam, que continua girando e dá um quarto de volta. Vão pra onde a jarra de vocês foi, ou entrem bem no eixo debaixo dele, que nunca gira. Libertem ele, e a roda volta a girar, e a luz sobe.",
                ("Follow the lit jar, or get right in on the hub. Then hit.", "Segue a jarra acesa, ou entra bem no eixo. Depois bate."),
                ("Maybe he just wanted something to stay still for once.", "Vai ver ele só queria que alguma coisa ficasse parada uma vez."),
                ("Twelve jars and the one that's safe keeps moving. Of course it does.", "Doze jarras e a única segura fica andando. Claro que fica."),
                ("A quarter turn, clockwise, every time. Your jar is always three over.", "Um quarto de volta, no sentido do relógio, toda vez. A sua jarra está sempre três pra frente."),
                ("It's a merry-go-round! A merry-go-round for light! I want a ride!", "É um carrossel! Um carrossel de luz! Eu quero andar!"));
            Line(entries, RegionBossRules.WheelArchonId, RegionBossPhase.Storm, "The Archon throws bronze from the wheel!", "O Arconte atira bronze da roda!");
            Line(entries, RegionBossRules.WheelArchonId, RegionBossPhase.LastStand, "The whole wheel is creaking!", "A roda inteira está rangendo!");
            Blow(entries, RegionBossRules.WheelArchonId, "A spoke comes down!", "Lá vem um raio da roda!");
            Signature(entries, RegionBossRules.WheelArchonId, "He's locking the wheel! Go to the jar that turns, or to the hub!", "Ele está travando a roda! Vai pra jarra que gira, ou pro eixo!");
            entries.Add(new LocalizationEntry(WheelJarKey, "Your jar!", "Sua jarra!"));
            Withdraw(entries, RegionBossRules.WheelArchonId, "The Archon of the Wheel lets go of the hub with a groan. The wheel shudders, then turns, slowly at first, and the twelve jars rise one after another, full of light.", "O Arconte da Roda solta o eixo com um gemido. A roda estremece, depois gira, devagar no começo, e as doze jarras sobem uma atrás da outra, cheias de luz.");

            // -- Pés da Coluna da Glória (98) --
            Page(entries, RegionBossRules.LastStepArchonId,
                "The Column of Glory is the road of light, children: everything good that was rescued climbs it, step by step, all the way home. At its foot, on the steps, sits the last archon of all, and he sat down right there so that nobody could go up. He doesn't chase anyone down. He only pushes away whoever is on his step or above it, towards the Column. When he plants his feet, go down a step, below him, and he lets you be. Free him, and he gets up, dusts himself off, and goes down the steps, away from the Column, and the sparks climb again.",
                "A Coluna da Glória é a estrada da luz, crianças: tudo de bom que foi resgatado sobe por ela, degrau por degrau, até chegar em casa. No pé dela, nos degraus, senta o último arconte de todos, e ele sentou bem ali pra ninguém conseguir subir. Ele não corre atrás de ninguém pra baixo. Só empurra quem está no degrau dele ou acima, na direção da Coluna. Quando ele firmar os pés, desçam um degrau, abaixo dele, e ele deixa vocês em paz. Libertem ele, e ele levanta, sacode a poeira e desce os degraus, pra longe da Coluna, e as faíscas voltam a subir.",
                ("When he plants his feet, I step down. Then I come back up and hit.", "Quando ele firmar os pés, eu desço um degrau. Depois subo de volta e bato."),
                ("The last one left. Sitting there alone so nobody else gets to leave.", "O último que sobrou. Sentado ali sozinho pra ninguém mais ir embora."),
                ("We came all this way up just so he can tell us to go down. Classic.", "A gente subiu isso tudo só pra ele mandar a gente descer. Clássico."),
                ("Above him is hit, on his step is hit. Two meters below and you're clear.", "Acima dele apanha, no degrau dele apanha. Dois metros abaixo e está livre."),
                ("Mister Archon, can you scoot over? Please? Pretty please?", "Senhor Arconte, pode chegar pro lado? Por favor? Por favorzinho?"));
            Line(entries, RegionBossRules.LastStepArchonId, RegionBossPhase.Storm, "The Last Archon throws chunks of the step!", "O Último Arconte atira pedaços do degrau!");
            Line(entries, RegionBossRules.LastStepArchonId, RegionBossPhase.LastStand, "He's holding on to the step with everything he has!", "Ele está agarrado ao degrau com tudo o que tem!");
            Blow(entries, RegionBossRules.LastStepArchonId, "He stamps on the step!", "Ele pisa forte no degrau!");
            Signature(entries, RegionBossRules.LastStepArchonId, "He's planting his feet! Go down a step, below him!", "Ele está firmando os pés! Desce um degrau, abaixo dele!");
            entries.Add(new LocalizationEntry(StepBelowKey, "Down here!", "Aqui embaixo!"));
            Withdraw(entries, RegionBossRules.LastStepArchonId, "The Last Archon of the Step gets up at last, slowly, and goes down the steps one by one without looking back. Behind him, the sparks start climbing the Column again.", "O Último Arconte do Degrau enfim se levanta, devagar, e desce os degraus um por um sem olhar pra trás. Atrás dele, as faíscas voltam a subir a Coluna.");

            // -- Mirante do que Segura o Céu (99) --
            Page(entries, RegionBossRules.GreatCrackId,
                "From the Lookout you can see the one who holds up the sky, children: a huge figure of light, far away, with the sky on his hands. He doesn't talk and he doesn't fight. He just holds it. But the sky up here has cracks, and through the biggest of them the shadow drips down like tar. That is the Great Crack. It drips along its whole length, and every drip it opens wider: narrow, then wider, then widest, and then it closes up and starts again. Keep to the side of the crack, further out each time. Seal it, and the sky up here is whole again, and the one who holds it can rest his arms a little.",
                "Do Mirante dá pra ver quem segura o céu, crianças: uma figura enorme de luz, bem longe, com o céu nas mãos. Ele não fala e não luta. Só segura. Mas o céu aqui em cima tem rachaduras, e pela maior delas a sombra escorre igual piche. Essa é a Rachadura Grande. Ela pinga pelo comprimento todo, e a cada pingo abre mais: estreita, depois mais larga, depois larguíssima, e aí fecha e começa de novo. Fiquem do lado da rachadura, mais longe a cada vez. Selem ela, e o céu aqui em cima fica inteiro de novo, e quem segura pode descansar um pouquinho os braços.",
                ("Get off the crack's line. Further every drip. Then back in to hit.", "Sai da linha da rachadura. Mais longe a cada pingo. Depois volta pra bater."),
                ("Someone has been holding all of this up the whole time. Let's help him.", "Alguém está segurando tudo isso esse tempo todo. Vamos ajudar ele."),
                ("A hole in the sky. Sure. Why not. Totally normal Tuesday.", "Um buraco no céu. Claro. Por que não. Uma terça-feira normal."),
                ("Narrow, wider, widest, then it closes. Count the drips.", "Estreita, mais larga, larguíssima, depois fecha. Conta os pingos."),
                ("If the sky has a crack, can we see what's on the other side? Just a peek?", "Se o céu tem uma rachadura, dá pra ver o que tem do outro lado? Só uma espiadinha?"));
            Line(entries, RegionBossRules.GreatCrackId, RegionBossPhase.Storm, "The Great Crack spits drops of shadow!", "A Rachadura Grande cospe gotas de sombra!");
            Line(entries, RegionBossRules.GreatCrackId, RegionBossPhase.LastStand, "The whole sky is creaking open!", "O céu inteiro está rangendo e se abrindo!");
            Blow(entries, RegionBossRules.GreatCrackId, "A gush of shadow falls!", "Cai um jorro de sombra!");
            Signature(entries, RegionBossRules.GreatCrackId, "The crack is dripping! Get off its line!", "A rachadura vai pingar! Sai da linha dela!");
            entries.Add(new LocalizationEntry(CrackWidthKey(0), "The crack drips! Narrow: a step aside!", "A rachadura pinga! Estreita: um passo pro lado!"));
            entries.Add(new LocalizationEntry(CrackWidthKey(1), "The crack opens wider! Two steps aside!", "A rachadura abre mais! Dois passos pro lado!"));
            entries.Add(new LocalizationEntry(CrackWidthKey(2), "The crack opens all the way! Far aside!", "A rachadura abre toda! Bem pro lado!"));
            Withdraw(entries, RegionBossRules.GreatCrackId, "The Great Crack shrinks to a thread and closes with a sound like a door shutting softly. Far away, the one who holds up the sky shifts his hands, and the sky up here is whole.", "A Rachadura Grande encolhe até virar um fio e fecha com um barulho de porta se fechando devagar. Lá longe, quem segura o céu ajeita as mãos, e o céu aqui em cima fica inteiro.");
        }
    }
}
