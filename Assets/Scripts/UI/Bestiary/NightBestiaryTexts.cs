using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the night-only species (Docs/DayNight.md; names from Docs/WorldBible_Maniqueismo.md §3.5): lore in Vó Edna's voice and one line
    /// per cousin, en + pt-BR. Kept apart from <see cref="BestiaryTexts"/> so the night pass and the bestiary don't edit
    /// the same lines; <see cref="BestiaryTexts"/> appends these to its own <see cref="BestiaryTexts.Entries"/>, so they are
    /// merged, covered by the localization tests and looked up exactly like every other page.
    /// Tone: fairy tale for kids — a little spooky, always fair, with a way to win and a touch of humour.
    /// </summary>
    public static class NightBestiaryTexts
    {
        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Bosque do Zarafshan (Floresta) --
            Page(entries, "enemy_forest_werewolf",
                "In Grandpa Jorge's stories, a mardagayl is someone enchanted into a wolf. Here on the isle the spell lasts seven nights: an old wanderer of the tugai turns into the Mardagayl, shaggy, smelly and with one ear that never stands up. He runs on all fours and rises on his hind legs to swipe. Grandma says he isn't evil, just grumpy from missing supper, and that light undoes the spell. When he howls, step aside: the big swipe comes right after.",
                "Nas histórias do Vô Jorge, mardagayl é gente que ficou encantada em lobo. Aqui na ilha o encanto dura sete noites: um andarilho velho do tugai vira o Mardagayl, peludo, fedido e com uma orelha que nunca fica em pé. Corre de quatro e se levanta nas patas de trás pra dar a patada. A Vó diz que ele não é malvado, só ranzinza de quem perdeu a janta, e que a luz desfaz o encanto. Quando ele uivar, saia de lado: a patada grande vem logo depois.",
                ("When he stands up, he's slow to come down. That's my shot.", "Quando ele fica em pé, demora pra descer. É a minha flechada."),
                ("He smells like a wet dog... I'll heal you, but nobody hugs him!", "Ele tem cheiro de cachorro molhado... eu curo vocês, mas ninguém abraça ele!"),
                ("Big claws, big shield. Come on, hairy!", "Unha grande, escudo grande. Pode vir, cabeludo!"),
                ("His ear is bent. Asymmetric. It bothers me more than the claws.", "A orelha dele é torta. Assimétrica. Me incomoda mais que as unhas."),
                ("He howled at me and I howled back. He looked confused.", "Ele uivou pra mim e eu uivei de volta. Ele ficou confuso."));

            Page(entries, "enemy_forest_shroud_owl",
                "The Smoke Owl screeches like someone ripping a sheet in the dark, and its feathers trail a wisp of smoke when it flies. That white heart face floats between the trees like a little moon. Owls are good birds; this one is just dimmed. It swoops down fast, but the lantern light dazzles it: stay in the light and hit it when it comes down.",
                "A Coruja-de-Fumaça grita que nem alguém rasgando um lençol no escuro, e as penas soltam um fiapo de fumaça quando ela voa. Aquela cara branca de coração flutua entre as árvores feito uma luazinha. Coruja é bicho bom; essa só está apagada. Ela desce num rasante ligeiro, mas a luz da lamparina deixa ela tonta: fiquem na claridade e batam quando ela descer.",
                ("Her face shines in the dark. Easy to aim at.", "A cara dela brilha no escuro. Fácil de mirar."),
                ("That scream gave me goosebumps. Joaquim, stay close to the light.", "Esse grito me arrepiou toda. Joaquim, fica perto da luz."),
                ("I'm not scared. I just jumped because I wanted to.", "Eu não tô com medo. Eu pulei porque quis."),
                ("She turned her head all the way back! How many joints does that neck have?", "Ela virou a cabeça toda pra trás! Quantas juntas tem esse pescoço?"),
                ("She's my favourite. She's spooky AND pretty.", "Ela é a minha preferida. É assustadora E bonita."));

            Page(entries, "enemy_forest_peccary",
                "Mire boars walk in herds through the tugai, leaving dark mud prints wherever they go. They charge in a straight line, one after the other, clacking their teeth. Step out of the line and hit them from the side. And no greed: if you knock down too many too fast, the next herd gets scared and runs away.",
                "O Javali do Lodo anda em bando pelo tugai, deixando pegada de lama escura por onde passa. Eles investem em linha reta, um atrás do outro, batendo os dentes. Saiam da linha e batam de lado. E nada de ganância: se derrubar demais muito depressa, o próximo bando se assusta e foge.",
                ("They only run straight. I just have to not be in front.", "Eles só correm reto. É só eu não estar na frente."),
                ("Careful, they're in a group! Nobody gets trampled today.", "Cuidado, eles vêm em grupo! Hoje ninguém é atropelado."),
                ("Three at once? I'll hold all three!", "Três de uma vez? Eu seguro os três!"),
                ("If I put a ramp in front of them... no, Maithe, I'm not doing it.", "Se eu botar uma rampa na frente deles... não, Maithe, eu não vou fazer isso."),
                ("I heard a snort in the reeds. I didn't like it.", "Eu ouvi um ronco no meio do junco. Não gostei."));

            // -- Areias Flamejantes (Deserto) --
            Page(entries, "enemy_desert_firefly_swarm",
                "These little lights are trapped sparks: bits of light the shadow made cross and confused. They come at dusk, dancing over the sands and the edge of the woods, and pinch like a hot pepper. Pretty from far away, a nuisance up close. A good sweeping blow calms the whole swarm at once, and then the sparks rise to the sky, free.",
                "Essas luzinhas são faíscas presas: pedacinhos de luz que a sombra deixou bravos e confusos. Elas chegam no entardecer, dançando por cima das areias e da beira do bosque, e beliscam que nem pimenta. De longe é bonito, de perto é amolação. Uma boa pancada em roda acalma o enxame inteiro de uma vez, e aí as faíscas sobem pro céu, livres.",
                ("So many little lights... I'll aim at the middle of the cloud.", "Tanta luzinha... vou mirar no meio da nuvem."),
                ("They singe, but just a bit. Come here, I'll blow on it.", "Elas queimam, mas só um pouquinho. Vem cá que eu assopro."),
                ("I swung the shield around and three went up. Three!", "Girei o escudo e subiram três. Três!"),
                ("Cold light! Well, warm light. I need to study this.", "Luz fria! Quer dizer, luz quente. Preciso estudar isso."),
                ("I wanted to keep one in a jar. Grandma said a free spark has to go up.", "Eu queria guardar uma num pote. A Vó disse que faísca livre tem que subir."));

            // -- Serra de Kaggen (Montanha) --
            Page(entries, "enemy_mountain_potoo",
                "Grandpa Jorge used to tell of two brothers who made the creatures of the world: Yen, who was careful, and Omöl, who was clumsy. The twig-bird is one of Omöl's: it sits so still on a dry branch that it looks like part of the tree, and its pink beak opens enormous. At night it sings a sad song that goes down, down, down... and calls other critters. Only its big yellow eyes give it away. Find it before it sings.",
                "O Vô Jorge contava de dois irmãos que fizeram os bichos do mundo: o Yen, caprichoso, e o Omöl, trapalhão. A Ave-Graveto é das do Omöl: fica tão paradinha num galho seco que parece pedaço da árvore, e o bocão rosa abre enorme. De noite ela canta uma cantiga triste que vai descendo, descendo... e chama outros bichos. Só os olhões amarelos entregam. Achem ela antes que cante.",
                ("I found her first. Those eyes don't fool me.", "Eu achei ela primeiro. Esses olhos não me enganam."),
                ("Her song is so sad... I almost wanted to sing along.", "A cantiga dela é tão triste... quase deu vontade de cantar junto."),
                ("I thought it was a branch. I sat next to it. It was not a branch.", "Eu achei que era um galho. Sentei do lado. Não era um galho."),
                ("She flew off with the branch still in her feet. That's not aerodynamic!", "Ela voou com o galho ainda nos pés. Isso não é aerodinâmico!"),
                ("We had a staring contest. I lost.", "A gente fez um concurso de quem pisca primeiro. Eu perdi."));

            // -- Grutas do Limiar (Cavernas) --
            Page(entries, "enemy_cave_vampire_bat",
                "Ubır is the grandfather of all the cave bats: wings as wide as a hammock and two fangs he's very proud of. In the old tales an ubır is a spirit that sucks; this one drinks light. He bites and drinks a little of your light to feel stronger. Keep the lantern lit and hit him right after he dives: he's big, so he takes a while to climb back up.",
                "O Ubır é o avô de todos os morcegos das grutas: asa do tamanho de uma rede e dois dentões de que ele morre de orgulho. Nos contos antigos, o ubır é um espírito que suga; esse aqui bebe luz. Ele morde e bebe um pouquinho da luz da gente pra ficar mais forte. Deixem a lamparina acesa e batam logo depois do mergulho: ele é grande, demora pra subir de novo.",
                ("Big wings, big target. Thank you, bat.", "Asa grande, alvo grande. Obrigada, morcego."),
                ("He drinks our light? Then I'll heal faster than he drinks.", "Ele bebe a nossa luz? Então eu curo mais rápido do que ele bebe."),
                ("He tried to bite my shield. Now he has a crooked fang.", "Ele tentou morder meu escudo. Agora tem um dente torto."),
                ("Wingspan of at least two meters. Maithe, did you see? Two meters!", "Envergadura de pelo menos dois metros. Maithe, viu? Dois metros!"),
                ("He's dark like me, but I don't bite anybody. Usually.", "Ele é escuro que nem eu, mas eu não mordo ninguém. Geralmente."));
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
