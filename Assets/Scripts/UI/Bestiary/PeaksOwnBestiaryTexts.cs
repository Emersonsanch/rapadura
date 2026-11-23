using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the five roles of the Planície de Mingcheng (Região 8, Docs/Mapas100.md 42 and 46) and of the Ermidas
    /// Suspensas of the Picos de Farnkesh (Região 10, 56) that the overworld still played with another beast after the twenty-seventh pass
    /// (bible §3.2.1, twenty-eighth pass): lore in Grandma Edna's voice and one line per cousin, en + pt-BR. Kept apart like
    /// <see cref="WestOwn2BestiaryTexts"/>; <see cref="BestiaryTexts"/> appends these to its <see cref="BestiaryTexts.Entries"/>. Never gore
    /// (bible §4.3).
    /// </summary>
    public static class PeaksOwnBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_mountain_dimmed_echo", "enemy_desert_cracked_bell", "enemy_forest_taotie_servant", "enemy_forest_qiongqi_cub",
            "enemy_forest_lime_guardian",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Ermidas Suspensas (56) --
            Page(entries, "enemy_mountain_dimmed_echo",
                "The old teachers said the Light calls, and every spark that hears it answers, and that is how the sparks find their way home: the Call goes out, the Answer comes back. On the cliff of the hermitages the shadow caught the answers before the calls, and now they answer anything, the wrong way round. Run, and an Echo runs at you; strike, and it strikes back with your own blow. They are born of noise: walk slowly between the ladders, and they stay asleep in the stone. When one flattens and the sound in its mouth flares, step out of the ring.",
                "Os mestres antigos diziam que a Luz chama, e toda faísca que escuta responde, e é assim que as faíscas acham o caminho de casa: o Chamado vai, a Resposta volta. No paredão das ermidas a sombra pegou as respostas antes dos chamados, e agora elas respondem qualquer coisa, do avesso. Corram, e um Eco corre pra cima de vocês; batam, e ele devolve o golpe de vocês mesmos. Eles nascem do barulho: andem devagar entre as escadas, que eles continuam dormindo na pedra. Quando um achatar e o som da boca dele brilhar, saiam do anel.",
                ("It gives your blow back. Fewer blows, fewer echoes.", "Ele devolve o teu golpe. Menos golpe, menos eco."),
                ("It's only answering. Nobody ever called it by its name.", "Ele só está respondendo. Ninguém nunca chamou ele pelo nome."),
                ("I said \"hello\" and it hit me. Rude.", "Eu disse \"oi\" e ele me bateu. Mal-educado."),
                ("An answer without a call is just noise. Be quiet a little, and you can hear the real Call again.", "Resposta sem chamado é só barulho. Fica quietinho um pouco, que dá pra ouvir o Chamado de verdade de novo."),
                ("Shhh! Everybody tiptoe! Tiptoe!", "Psiu! Todo mundo na ponta do pé! Na ponta do pé!"));

            Page(entries, "enemy_desert_cracked_bell",
                "Every hermitage on the cliff had a little bronze bell, and the hermits rang it once at dusk, so the others knew they were well, and once more at dawn. When the great bell of the pinnacle would not stop, the little ones cracked trying to answer it. The shadow got in at the cracks. Now they hang loose on their frayed ropes and throw their broken note at whoever climbs. They tip back before they ring: step aside then, and come in close, where the note goes over your head.",
                "Cada ermida do paredão tinha um sininho de bronze, e os ermitões tocavam uma vez no fim da tarde, pros outros saberem que estava tudo bem, e outra vez de manhãzinha. Quando o sino grande do pináculo não parou mais, os pequenos racharam tentando responder. A sombra entrou pelas rachaduras. Agora eles ficam soltos nas cordas puídas e jogam a nota quebrada em quem sobe. Eles tombam pra trás antes de tocar: aí vocês dão um passo pro lado e chegam perto, que a nota passa por cima da cabeça.",
                ("It tips back before it rings. Step aside, then close in.", "Ele tomba pra trás antes de tocar. Passo pro lado e chega perto."),
                ("It cracked trying to answer a friend. That's the saddest way to break.", "Ele rachou tentando responder um amigo. É o jeito mais triste de quebrar."),
                ("Ding. Dong. Ow. Mostly ow.", "Dim. Dom. Ai. Mais ai."),
                ("A bell rings to say \"I'm here, I'm well\". Stop the big bell, and these remember what they were for.", "Sino toca pra dizer \"estou aqui, estou bem\". Para o sino grande, que esses lembram pra que serviam."),
                ("Can we fix it with glue? Grandma fixes everything with glue.", "Dá pra consertar com cola? A vó conserta tudo com cola."));

            // -- Cidadela dos Quatro Gulosos (46) --
            Page(entries, "enemy_forest_taotie_servant",
                "In the old rites the bronze vessels held the offerings, and the face cast on their bellies, the taotie, was a face that eats and is never full. When Taotie woke in the citadel, the vessels got up on their three legs and went to serve it, and they gather up whatever falls, coins, food, the light out of a lamp, to carry to their master. They are heavy and slow and hit hard with their handles: let one swing, and hit it while it is still turning back.",
                "Nos ritos antigos os vasos de bronze guardavam as oferendas, e a cara fundida na barriga deles, o taotie, era uma cara que come e nunca enche. Quando o Taotie acordou na cidadela, os vasos levantaram nas três pernas e foram servir ele, e catam tudo o que cai, moeda, comida, a luz de dentro de uma lamparina, pra levar pro dono. São pesados e lentos e batem forte com as alças: deixem um errar o golpe e batam enquanto ele ainda está virando de volta.",
                ("It swings slow. Let it miss, then hit.", "Ele bate devagar. Deixa errar e depois bate."),
                ("It was made to hold gifts. It still wants to be full of something good.", "Ele foi feito pra guardar presente. Ainda quer ficar cheio de coisa boa."),
                ("A pot with legs. I've seen everything now.", "Uma panela com perna. Agora eu já vi de tudo."),
                ("Taotie is a hunger with no bottom. That's Āz, the greed of the Darkness, wearing a bronze face.", "O Taotie é uma fome sem fundo. É a Āz, a cobiça da Treva, com cara de bronze."),
                ("Don't let it take my snacks! Those are MY offerings!", "Não deixa ele pegar meu lanche! Essas são as MINHAS oferendas!"));

            Page(entries, "enemy_forest_qiongqi_cub",
                "The old book of the mountains and seas draws Qiongqi as a tiger with wings, and with quills like a hedgehog's, and it says Qiongqi eats the honest and brings presents to the quarrelsome: it turns right into wrong. In the citadel its cubs run ahead of it in little packs. They are small and light, their wings too young to lift them, but they spread them when they pounce. They come fast and flee when they are hurt: do not chase them far from the others.",
                "O livro velho das montanhas e dos mares desenha o Qiongqi como um tigre com asas, e com espinho de ouriço, e diz que o Qiongqi come quem é honesto e leva presente pra quem é briguento: ele vira o certo em errado. Na cidadela os filhotes dele correm na frente em bandinhos. São pequenos e leves, as asas ainda novas demais pra levantar eles, mas eles abrem as asas quando dão o bote. Chegam rápido e fogem quando se machucam: não corram atrás deles pra longe dos outros.",
                ("They run when they're hurt. Don't chase them off alone.", "Eles fogem quando se machucam. Não corre atrás deles sozinho."),
                ("They're only cubs. They learned it wrong from a bad teacher.", "Eles são só filhotes. Aprenderam errado com um professor ruim."),
                ("A kitten with wings AND spikes. Nature, why.", "Um gatinho com asa E espinho. Natureza, por quê."),
                ("Qiongqi swaps right and wrong. The way to beat it is to keep yours where it belongs, even when your feet go the other way.", "O Qiongqi troca o certo e o errado. O jeito de vencer é deixar o teu no lugar, mesmo quando os pés vão pro outro lado."),
                ("I want one! A little one! It can sleep in my bed!", "Eu quero um! Um pequenininho! Ele pode dormir na minha cama!"));

            // -- Torres Brancas e Colinas dos Espelhos (42) --
            Page(entries, "enemy_forest_lime_guardian",
                "Queen Amanirenas had the guards of her white spires plastered with lime, so they would shine at the gates like the towers themselves, and she gave each one the bow of Ta-Seti, the Land of the Bow, where her people came from. The plaster cracked in the long sun, and the shadow got in under it. Now they keep the spires from the very elves who built them. They are slow and strong, and the bow is only for show: the shadow took the arrows. When both arms go up, step back.",
                "A rainha Amanirenas mandou rebocar com cal os guardas das torres brancas, pra eles brilharem nos portões que nem as próprias torres, e deu pra cada um o arco de Ta-Seti, a Terra do Arco, de onde o povo dela veio. O reboco rachou no sol comprido, e a sombra entrou por baixo. Agora eles guardam as torres dos próprios elfos que construíram elas. São lentos e fortes, e o arco é só enfeite: a sombra levou as flechas. Quando os dois braços subirem, deem um passo pra trás.",
                ("Both arms up means it's coming down. Step back.", "Os dois braços pra cima quer dizer que vai descer. Passo pra trás."),
                ("It still stands at the gate. It just forgot who it's guarding.", "Ele ainda fica no portão. Só esqueceu quem está guardando."),
                ("A bow with no arrows. I know the feeling. I always forget mine.", "Arco sem flecha. Eu conheço essa. Eu sempre esqueço as minhas."),
                ("Lime is white because it was burned clean. Something burned clean can shine again, even with a crack in it.", "A cal é branca porque foi queimada até ficar limpa. O que foi limpo no fogo pode brilhar de novo, mesmo rachado."),
                ("It's like a snowman, but grumpy and it doesn't melt!", "Parece um boneco de neve, só que bravo e que não derrete!"));
        }

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
