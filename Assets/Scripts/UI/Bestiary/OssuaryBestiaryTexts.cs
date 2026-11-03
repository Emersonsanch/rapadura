using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Dungeon;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the five guardians of the Ossuário dos Artífices (Docs/Dungeons.md): lore in Vó Edna's voice
    /// (the Artificers' volunteer guards who woke up without their memory: not evil, only dimmed; beaten fairly, the spark goes up
    /// to the Column of Glory) and one line per cousin, en + pt-BR. Kept apart like <see cref="RangedBestiaryTexts"/>;
    /// <see cref="BestiaryTexts"/> appends these to its <see cref="BestiaryTexts.Entries"/>.
    /// </summary>
    public static class OssuaryBestiaryTexts
    {
        public static void AddPages(List<LocalizationEntry> entries)
        {
            Page(entries, DungeonRules.FloatingSkullId,
                "In the niches of the Ossuary there are skulls that never stopped keeping watch. A little of the guard's spark stayed stuck inside, and that green light keeps them floating, chattering their teeth like someone who is cold. When somebody passes, they wake with a fright and dive to bite, then run away, ashamed. Grandma says they're like sleepy dogs barking at the door: more noise than harm. They are very fast, so don't chase them: plant your feet, wait for the dive and answer right when they come close.",
                "Nos nichos do Ossuário tem caveiras que nunca pararam de vigiar. Um pouquinho da faísca do guarda ficou presa lá dentro, e é essa luz verde que deixa elas flutuando, batendo os dentes feito quem tá com frio. Quando alguém passa, elas acordam assustadas e mergulham pra morder, depois fogem envergonhadas. A Vó diz que são que nem cachorro sonolento latindo no portão: mais barulho que estrago. São muito ligeiras, então não corram atrás: firmem o pé, esperem o mergulho e respondam bem na hora que ela chegar perto.",
                ("Small, fast and glowing. A hard target. I like hard targets.", "Pequena, ligeira e brilhando. Alvo difícil. Eu gosto de alvo difícil."),
                ("It bit and ran off ashamed... I think it just wanted company.", "Ela mordeu e fugiu com vergonha... Acho que só queria companhia."),
                ("It went \"Buu!\" at me. I didn't jump. Lavine jumped.", "Ela fez \"Buu!\" pra mim. Eu não pulei. A Lavine pulou."),
                ("What keeps it in the air? No wings, no gears... Just light? I need to know.", "O que segura ela no ar? Sem asa, sem engrenagem... Só luz? Eu preciso saber."),
                ("I did NOT jump. I was stretching. Very fast.", "Eu NÃO pulei. Eu tava me alongando. Bem rápido."));

            Page(entries, DungeonRules.SkeletonWarriorId,
                "The Artificers asked for volunteers to guard the Seal, and these warriors said yes. To last forever, they had their bones lined with copper, and so they stayed, green with age, with their round shields of bone. When the Crystal weakened they woke up without remembering who they were, only that they must not let anybody pass. Every little while one of them lifts the shield and holds it firm: hitting it then is no use, and arrows even bounce back. Wait for the shield to come down, then go. Beaten fairly, Grandma says, they remember their names for an instant and smile.",
                "Os Artífices pediram voluntários pra guardar o Selo, e esses guerreiros disseram sim. Pra durar pra sempre, mandaram forrar os ossos de cobre, e assim ficaram, verdinhos de tão velhos, com seus escudos redondos de osso. Quando o Cristal enfraqueceu, eles acordaram sem lembrar quem eram, só que não podem deixar ninguém passar. De tempo em tempo um deles levanta o escudo e segura firme: bater nessa hora não adianta, e flecha até volta. Esperem o escudo baixar e aí vão. Vencidos com lealdade, diz a Vó, eles lembram o próprio nome por um instante e sorriem.",
                ("When the shield goes up, I lower the bow. Patience is an arrow too.", "Quando o escudo sobe, eu baixo o arco. Paciência também é flecha."),
                ("They volunteered to protect everybody... Just like Joaquim. Be careful, brother.", "Eles se ofereceram pra proteger todo mundo... Igual o Joaquim. Cuidado, mano."),
                ("A shield against a shield. Let's see who gets tired first. Not me.", "Escudo contra escudo. Quero ver quem cansa primeiro. Não sou eu."),
                ("Copper-lined bones! That's the best idea I've ever seen. On a skeleton.", "Osso forrado de cobre! É a melhor ideia que eu já vi. Num esqueleto."),
                ("I waited for the shield to come down, just like Grandma said. Then BAM.", "Eu esperei o escudo baixar, igualzinho a Vó falou. Aí, PÁ."));

            Page(entries, DungeonRules.SkeletonArcherId,
                "At the bottom of the Pit, the Ossuary's archers still keep their posts, wrapped in burial hoods so old they fall apart. Their arrows are made of ribs and carry a purple rune that leaves the legs heavy for a little while. They don't like anyone close: if you come running, they step back and nock another arrow. Grandma's advice is to zigzag in, never in a straight line, and to stick close to them. Up close, a bow is just a stick.",
                "Lá no fundo do Fosso, os arqueiros do Ossuário ainda guardam seus postos, enrolados em capuzes de mortalha tão velhos que se desmancham. As flechas são feitas de costela e levam uma runa roxa que deixa a perna pesada por um tempinho. Eles não gostam de ninguém perto: se a gente chega correndo, recuam e encaixam outra flecha. O conselho da Vó é chegar em zigue-zague, nunca em linha reta, e grudar neles. De perto, arco é só um graveto.",
                ("Their arrows are made of ribs. Mine are better. I made them with Grandpa Jorge.", "As flechas deles são de costela. As minhas são melhores. Fiz com o Vô Jorge."),
                ("If a purple arrow hits you, come to me before you keep running.", "Se a flecha roxa pegar, venham até mim antes de sair correndo."),
                ("Zigzag. Shield up. Zigzag. Got him.", "Zigue-zague. Escudo na frente. Zigue-zague. Peguei."),
                ("The rune on the arrow slows you down... If I put it in my drone, could it speed things up instead?", "A runa da flecha deixa a gente lento... Se eu botar no drone, será que deixa rápido?"),
                ("He backs away from me. That means he takes me seriously.", "Ele recua de mim. Quer dizer que ele me leva a sério."));

            Page(entries, DungeonRules.BoneSorcererId,
                "The Bone Sorcerer was the Ossuary's caretaker, the one who woke the guards when it was time to change shifts. He still has the old habit: when a copper warrior falls, he raises the staff, the violet orb shines, and he lights the bones up again, half asleep. He also throws spectral spheres from far away. Grandma says to always go for him first: while the caretaker is awake, the guards keep getting up. Watch the purple smoke: it means he is calling somebody back.",
                "O Feiticeiro das Ossadas era o zelador do Ossuário, o que acordava os guardas quando era hora de trocar o turno. Ele ainda tem a mania antiga: quando um guerreiro de cobre cai, levanta o cajado, a esfera roxa brilha, e ele acende os ossos de novo, meio dormindo. Também atira umas esferas espectrais de longe. A Vó diz pra ir sempre nele primeiro: enquanto o zelador estiver acordado, os guardas não param de levantar. Fiquem de olho na fumaça roxa: é ele chamando alguém de volta.",
                ("The caretaker first. Always the one giving the orders.", "O zelador primeiro. Sempre quem dá as ordens."),
                ("He brings the fallen back... I heal too. But I ask first.", "Ele traz os caídos de volta... Eu também curo. Mas eu pergunto antes."),
                ("I knocked that warrior down twice. Two times! Stop raising him!", "Eu derrubei aquele guerreiro duas vezes. Duas! Para de levantar ele!"),
                ("That orb is a battery of light. Maithe, can I keep the orb? Just the orb?", "Aquela esfera é uma bateria de luz. Maithe, posso ficar com a esfera? Só a esfera?"),
                ("Purple smoke means \"go now\". I went. I was first.", "Fumaça roxa quer dizer \"vai agora\". Eu fui. Fui a primeira."));

            Page(entries, DungeonRules.BoneColossusId,
                "At the very bottom of the Ossuary, the Artificers' guards who had nobody left to guard joined together so as not to be alone, skull upon skull, around a single heart of green light. That's how the Colossus of Skulls was born: huge, slow and strong, with a femur for a club. When it hits the ground the floor trembles all around; then it starts throwing skulls that bounce off the walls, and at the end it opens its own ribs and tries to trap you in a cage of bones. Grandma says: back away from the blow, jump the tremor, break the cage fast. And when it falls, look up: a lot of little sparks go up together.",
                "Bem no fundo do Ossuário, os guardas dos Artífices que não tinham mais o que guardar se juntaram pra não ficarem sozinhos, caveira sobre caveira, em volta de um coração só de luz verde. Assim nasceu o Colosso de Crânios: enorme, lento e forte, com um fêmur de clava. Quando ele bate no chão, o chão treme em volta; depois começa a jogar caveiras que quicam nas paredes, e no fim abre as próprias costelas e tenta prender a gente numa jaula de ossos. A Vó diz: afasta do golpe, pula o tremor, quebra a jaula ligeiro. E quando ele cair, olhem pra cima: sobe um monte de faisquinha junta.",
                ("I counted the skulls while I aimed. I lost count. I didn't lose my aim.", "Eu contei as caveiras enquanto mirava. Perdi a conta. A mira, não."),
                ("So many sparks going up at once... They're finally resting.", "Tanta faísca subindo de uma vez... Agora eles descansam."),
                ("It trapped me in a cage. I broke the cage. With my face. It worked.", "Ele me prendeu numa jaula. Eu quebrei a jaula. Com a cara. Deu certo."),
                ("A structure of skulls with a central energy core. It's a machine. A sad machine.", "Uma estrutura de caveiras com núcleo de energia central. É uma máquina. Uma máquina triste."),
                ("It's the biggest thing I've ever beaten. Write that down in the notebook, Grandma. In pen.", "É a coisa mais grande que eu já venci. Escreve no caderno, Vó. De caneta."));
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
