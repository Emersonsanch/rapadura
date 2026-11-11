using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the world bosses of Regions 10–16 (Docs/Mapas100.md 58, 64, 70, 76, 82, 90, 96) and the short lines they call out in the fight, en + pt-BR.
    /// Kept apart like <see cref="CeuBestiaryTexts"/>; <see cref="BestiaryTexts"/> appends these to its entries. The Rei-Vendaval is the Vento Destruidor and the
    /// Glutão the Fogo Devorador (bible §1.3), not gods: they lose and go back, like the Dark.
    /// </summary>
    public static class RegionBossTexts
    {
        public static readonly string[] EnemyIds = 
        {
            RegionBossRules.WindKingId, RegionBossRules.OldKulId, RegionBossRules.EmberGluttonId,
            RegionBossRules.ApepId, RegionBossRules.DreamOfDestructionId, RegionBossRules.GreatDarkId, RegionBossRules.AzCaptainId,
        };

        public static string PhaseKey(string enemyId, RegionBossPhase phase) => $"region_boss.{enemyId}.phase{(int)phase}";

        public static string GreatBlowKey(string enemyId) => $"region_boss.{enemyId}.great_blow";

        public static string SignatureKey(string enemyId) => $"region_boss.{enemyId}.signature";

        public static string WithdrawKey(string enemyId) => $"region_boss.{enemyId}.withdraw";

        public static string FormKey(GreatDarkForm form) => $"region_boss.great_dark.form.{form.ToString().ToLowerInvariant()}";

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Ninho da Huma (58) --
            Page(entries, RegionBossRules.WindKingId,
                "The Gale King has no face, and that isn't a figure of speech, I looked. It is the Destroying Wind wearing the shape of a bird, and what it wants is the huma's nest, because the huma never lands and that drives it mad. Every time a shadow of her falls on someone they get lucky, and every time it pushes at the nest the nest simply stays. Watch the sky at night, it gets worse then. When it spreads those wings, back off, and when it goes quiet and the air goes thick around your feet, move: it comes down exactly where you were standing. You won't kill the wind, children. You'll only make it leave for a while, and it will go sulk in the caves.",
                "O Rei-Vendaval não tem rosto, e não é modo de dizer, eu olhei. É o Vento Destruidor vestido de ave, e o que ele quer é o ninho da huma, porque a huma nunca pousa e isso deixa ele doido. Toda vez que a sombra dela cai em alguém a pessoa dá sorte, e toda vez que ele empurra o ninho o ninho simplesmente fica. Cuidado à noite, que piora. Quando ele abrir as asas, afastem-se, e quando ficar quieto e o ar engrossar em volta dos pés, saiam: ele desce exatamente onde vocês estavam. Vento não se mata, crianças. Só se faz ir embora um tempo, e ele vai emburrar nas grutas.",
                ("Stop standing where it can see you stop. When the air goes thick, walk.", "Para de ficar parada onde ele te vê parar. O ar engrossou, anda."),
                ("All that hurry and no face to be sorry with. Poor thing. Hit it anyway.", "Tanta pressa e nenhum rosto pra se arrepender. Coitado. Bate mesmo assim."),
                ("It has no face! How do I know if it's laughing at my hair?", "Ele não tem cara! Como eu vou saber se tá rindo do meu cabelo?"),
                ("Wind is only air with an opinion. Unfortunately this one has a very strong opinion.", "Vento é só ar com opinião. Infelizmente este aqui tem opinião forte."),
                ("I'm going to find his face and draw a smile on it!", "Eu vou achar a cara dele e desenhar um sorriso nela!"));
            Line(entries, RegionBossRules.WindKingId, RegionBossPhase.Storm, "The wind turns!", "O vento virou!");
            Line(entries, RegionBossRules.WindKingId, RegionBossPhase.LastStand, "The Gale King comes down!", "O Rei-Vendaval desce!");
            Blow(entries, RegionBossRules.WindKingId, "Out from under it!", "Sai debaixo!");

            // -- Fundo do Velho Kul (64) --
            Page(entries, RegionBossRules.OldKulId,
                "The kul are the water spirits of the old stories of the north, and most of them only want to be left alone with their fish. This one wants to be left alone with all of the fish. He has gone grey and sour under the ice, and he keeps them shut in his chest so that nobody can say he has none. Grumpy, I'd say, more than wicked. He throws cold water at you when he's tired of hitting you, and the night makes him bolder. Beat him and he lets the fish go and sulks for a week. Then he comes back to complain about the fishing, which, frankly, he has every right to do.",
                "Os kul são os espíritos d'água das histórias antigas do norte, e a maioria só quer ficar em paz com seus peixes. Este quer ficar em paz com todos os peixes. Ficou cinzento e azedo debaixo do gelo, e guarda os bichos presos no peito pra ninguém poder dizer que ele não tem. Mais rabugento que mau, eu diria. Joga água gelada em vocês quando cansa de bater, e a noite deixa ele mais atrevido. Ganhem dele e ele solta os peixes e passa a semana emburrado. Depois volta pra reclamar da pesca, o que, sinceramente, é direito dele.",
                ("Slow arms, fast temper. Stay out of the first and let the second wear out.", "Braço lento, temperamento rápido. Fica fora do primeiro e deixa o segundo cansar."),
                ("He's keeping them so nobody can take them. I know people like that.", "Ele guarda pra ninguém tirar. Conheço gente assim."),
                ("Old man, those are not your fish, those are everybody's lunch!", "Velho, esses peixes não são seus, são o almoço de todo mundo!"),
                ("A body of water that holds a grudge. The ice is the grudge.", "Um corpo d'água guardando rancor. O gelo é o rancor."),
                ("He has fish in his tummy! Do they get to come out? They get to come out!", "Ele tem peixe na barriga! Eles vão sair? Eles vão sair!"));
            Line(entries, RegionBossRules.OldKulId, RegionBossPhase.Storm, "The Old Kul spits the cold!", "O Velho Kul cospe o frio!");
            Line(entries, RegionBossRules.OldKulId, RegionBossPhase.LastStand, "The ice groans under you!", "O gelo geme debaixo de você!");
            Blow(entries, RegionBossRules.OldKulId, "The ice is coming up!", "O gelo vai subir!");

            // -- Caldeira do Fogo Devorador (70) --
            Page(entries, RegionBossRules.EmberGluttonId,
                "Living Fire makes the forges burn and bread rise. The Devouring Fire is what's left when the same warmth forgets it is supposed to stop. It has no face either, only the mouth. It eats the forges' fire when it can reach them and its own when it can't, and it serves Āz without knowing it, because hunger doesn't need to be told what to want. Keep the fires of Hinokura lit, children, it loses its courage then. And watch the ground: when it opens up, you have about as long as it takes to say my name. Don't say my name. Run.",
                "O Fogo Vivo faz a forja arder e o pão crescer. O Fogo Devorador é o que sobra quando esse mesmo calor esquece que precisava parar. Também não tem rosto, só a boca. Come o fogo das forjas quando alcança e o próprio quando não alcança, e serve a Āz sem saber, porque a fome não precisa que lhe digam o que querer. Mantenham acesos os fogos de Hinokura, crianças, que ele perde a coragem. E olho no chão: quando abre, vocês têm mais ou menos o tempo de dizer o meu nome. Não digam meu nome. Corram.",
                ("Its arms come down slow. The ground doesn't. Mind the ground.", "Os braços descem devagar. O chão não. Olho no chão."),
                ("It's so hungry it would eat its own warmth. That's the saddest thing I've ever had to punch.", "Tem tanta fome que come o próprio calor. É a coisa mais triste que já tive que socar."),
                ("You and me both, big guy. Well, not the 'eating the forge' part.", "Eu também, grandão. Bom, tirando a parte de comer a forja."),
                ("Combustion with an appetite and nothing to burn it for. The Mixture in one sentence.", "Combustão com apetite e nada por que queimar. A Mistura numa frase."),
                ("Mouth! Mouth mouth mouth! It's all mouth!", "Boca! Boca boca boca! É só boca!"));
            Line(entries, RegionBossRules.EmberGluttonId, RegionBossPhase.Storm, "The Glutton spits embers!", "O Glutão cospe brasa!");
            Line(entries, RegionBossRules.EmberGluttonId, RegionBossPhase.LastStand, "It wants the fire under your feet!", "Ele quer o fogo debaixo dos seus pés!");
            Blow(entries, RegionBossRules.EmberGluttonId, "The ground is opening!", "O chão vai abrir!");
            // -- Banco de Areia de Apep (76) --
            Page(entries, RegionBossRules.ApepId,
                "Apep is the serpent that drinks the river every night so the barque of the hours runs aground, and every morning the barque goes on anyway. Nobody has ever killed her. You beat her, she spits the river back out, and she pulls away to sulk until the next night. Look at the clock when you go to her: near midnight she is at her strongest. When she stops to swallow, get in close, right under her chin. The ones who stay far away go down with the water. I know it feels wrong. Do it anyway.",
                "Apep é a serpente que bebe o rio toda noite pra barca das horas encalhar, e toda manhã a barca segue assim mesmo. Ninguém nunca matou ela. Vocês ganham, ela cospe o rio de volta e recua pra emburrar até a noite seguinte. Olhem o relógio antes de ir: perto da meia-noite ela está no mais forte. Quando ela parar pra engolir, cheguem perto, bem debaixo do queixo. Quem fica longe desce junto com a água. Eu sei que parece errado. Façam assim mesmo.",
                ("When she swallows, stand close. It's the only place the river isn't going.", "Quando ela engolir, fica perto. É o único lugar pra onde o rio não vai."),
                ("She drinks the whole river and she's still thirsty. Somebody should tell her it doesn't work.", "Bebe o rio inteiro e continua com sede. Alguém devia avisar que não funciona."),
                ("I've seen you eat, Lavine. This is about the same.", "Eu já te vi comendo, Lavine. É mais ou menos isso."),
                ("A river is a very long, very slow promise. She keeps breaking it and it keeps being made.", "Um rio é uma promessa comprida e devagar. Ela quebra, e ela é feita de novo."),
                ("Spit it out! Spit it out spit it out! That's not yours!", "Cospe! Cospe cospe cospe! Isso não é teu!"));
            Line(entries, RegionBossRules.ApepId, RegionBossPhase.Storm, "Apep spits the mud of the river!", "Apep cospe o lodo do rio!");
            Line(entries, RegionBossRules.ApepId, RegionBossPhase.LastStand, "The river is going down her throat!", "O rio tá descendo pela garganta dela!");
            Blow(entries, RegionBossRules.ApepId, "Out from under her coils!", "Sai de baixo das voltas dela!");
            Signature(entries, RegionBossRules.ApepId, "She's swallowing the river! Get close!", "Ela tá engolindo o rio! Chega perto!");
            Withdraw(entries, RegionBossRules.ApepId, "Apep spits the river back and slides away until the next night.", "Apep cospe o rio de volta e desliza até a noite seguinte.");

            // -- Campo das Tábuas do Sonho (82) --
            Page(entries, RegionBossRules.DreamOfDestructionId,
                "Hahya and Ohya, the giants' sons, dreamed of a tablet washed clean and a garden cut down, and they were so frightened that the fear got up and walked away on its own. That is this. It isn't a monster, children, it is a bad night that nobody finished. It knocks the sky's trees down around the spot where you were standing and, in a dream, the only safe place is the one that feels the worst: stand still and let it pass over you, or run far out. Hahya and Ohya are ashamed of it and they will tell you so. Win, and the dream ends well. That is the whole of the trick.",
                "Hahya e Ohya, os filhos dos gigantes, sonharam com uma tábua lavada e um jardim cortado, e ficaram com tanto medo que o medo levantou e saiu andando sozinho. É isso aqui. Não é monstro, crianças, é uma noite ruim que ninguém terminou. Derruba as árvores do sonho em volta do lugar em que vocês estavam e, no sonho, o único lugar seguro é o que parece pior: fiquem parados e deixem passar por cima, ou corram pra bem longe. O Hahya e o Ohya têm vergonha dele e vão dizer isso a vocês. Ganhem, e o sonho termina bem. O truque é só esse.",
                ("Don't run. I know. Don't run. Stay where it marked you.", "Não corre. Eu sei. Não corre. Fica onde ele marcou."),
                ("Somebody's fear got up and left without them. Let's walk it home.", "O medo de alguém levantou e saiu sem ele. Vamos levar ele pra casa."),
                ("I hit a nightmare once. It was a pillow. This one's worse.", "Eu já bati num pesadelo. Era um travesseiro. Este é pior."),
                ("In the dream, you keep still. Awake, you run. I've been doing it backwards my whole life.", "No sonho, fica parado. Acordado, corre. Faço ao contrário a vida inteira."),
                ("When I have a bad dream I go in Vó's bed. Does it have a Vó?", "Quando eu tenho sonho ruim eu vou pra cama da Vó. Ele tem Vó?"));
            Line(entries, RegionBossRules.DreamOfDestructionId, RegionBossPhase.Storm, "The dream throws its broken tablets!", "O sonho joga as tábuas quebradas!");
            Line(entries, RegionBossRules.DreamOfDestructionId, RegionBossPhase.LastStand, "Everything is cut down at once!", "Tudo cortado de uma vez!");
            Blow(entries, RegionBossRules.DreamOfDestructionId, "The tablet comes down!", "A tábua vai cair!");
            Signature(entries, RegionBossRules.DreamOfDestructionId, "Don't move! In the dream it's the other way round!", "Não se mexe! No sonho é ao contrário!");

            // -- Portão da Terra da Treva (90) --
            Page(entries, RegionBossRules.GreatDarkId,
                "You'll only see an outline, and I am glad of it. The Prince of Darkness has a lion's head, an eagle's wings, a dragon's body, a fish's tail and a demon's feet, and it wears them one after the other because it never could decide what it wanted to be. It can't die, children. The Five Lamps must be lit together to close the seal and send it back for a season, and that is the work of many hands, more than yours alone. But you can stand in its way until it has had enough. Lion hits hardest, the eagle shoots more, and the fish is the moment to breathe. Then go home, and light a lamp anyway.",
                "Vocês só vão ver o contorno, e fico feliz. O Príncipe da Treva tem cabeça de leão, asas de águia, corpo de dragão, cauda de peixe e pés de demônio, e usa cada um por vez porque nunca soube decidir o que queria ser. Ele não morre, crianças. As Cinco Lâmpadas têm que acender juntas pra fechar o selo e mandar ele de volta por uma temporada, e isso é trabalho de muita gente, mais do que vocês sozinhos. Mas dá pra ficar na frente até ele enjoar. O leão bate mais forte, a águia atira mais, e o peixe é a hora de respirar. Depois voltem pra casa, e acendam uma lâmpada mesmo assim.",
                ("Watch which one it is, not where it is. The shape tells you what's coming.", "Olha qual é, não onde está. A forma diz o que vem."),
                ("It can't pick what it wants to be. That part I almost feel sorry for.", "Não consegue escolher o que quer ser. Dessa parte eu quase tenho pena."),
                ("Five forms and it still can't get a decent haircut.", "Cinco formas e ainda não consegue um corte de cabelo decente."),
                ("Not one of the five is a face. Interesting thing to leave out.", "Nenhuma das cinco é um rosto. Curioso deixar isso de fora."),
                ("I'm not scared. I'm holding the lamp so you won't be scared.", "Eu não tô com medo. Tô segurando a lâmpada pra vocês não ficarem."));
            Line(entries, RegionBossRules.GreatDarkId, RegionBossPhase.Storm, "The Darkness spits fire!", "A Treva cospe fogo!");
            Line(entries, RegionBossRules.GreatDarkId, RegionBossPhase.LastStand, "It is all five at once!", "São as cinco de uma vez!");
            Blow(entries, RegionBossRules.GreatDarkId, "The dark comes down!", "A escuridão desce!");
            Form(entries, GreatDarkForm.Lion, "The lion's head!", "A cabeça de leão!");
            Form(entries, GreatDarkForm.Eagle, "The eagle's wings!", "As asas de águia!");
            Form(entries, GreatDarkForm.Dragon, "The dragon's body!", "O corpo de dragão!");
            Form(entries, GreatDarkForm.Fish, "The fish's tail!", "A cauda de peixe!");
            Form(entries, GreatDarkForm.Demon, "The demon's feet!", "Os pés de demônio!");
            Withdraw(entries, RegionBossRules.GreatDarkId, "The Great Darkness draws back behind the seal. It isn't over. It never is.", "A Grande Treva recua para trás do selo. Não acabou. Nunca acaba.");

            // -- Jardim de Luz (96) --
            Page(entries, RegionBossRules.AzCaptainId,
                "Āz is the appetite that made the whole Mixture happen, and she has decided to be a sea captain, with the hat and everything. She wants the Garden of Light in one ark and she'll carry it off to where nobody else can see it. When the Ark opens she helps herself to some of whatever you have left, so the fuller you are the more she takes. Stay light on your feet and she'll get less. She can't really die either, and I'm not sorry: beaten, she has to give the ark back and stand there being ashamed, in front of the whole garden. I'd pay to see that twice.",
                "Āz é o apetite que fez a Mistura toda acontecer, e resolveu ser capitã de navio, de chapéu e tudo. Quer o Jardim de Luz inteiro numa arca e vai carregar pra onde ninguém mais veja. Quando a Arca abre, ela se serve um pouco de tudo o que vocês ainda têm, e quanto mais cheios, mais ela pega. Ela também não morre de verdade, e não me dá pena: vencida, tem que devolver a arca e ficar ali com vergonha, na frente do jardim inteiro. Eu pagaria pra ver isso duas vezes.",
                ("Stop looking at the Ark. That's what it's for.", "Para de olhar pra Arca. É pra isso que ela serve."),
                ("She has everything and she's still counting. Poor woman.", "Tem tudo e ainda tá contando. Coitada."),
                ("Captain of what? There's no ship! That's just a hat!", "Capitã do quê? Não tem navio! É só um chapéu!"),
                ("Greed is the only sum that grows by being subtracted from. Mathematically unpleasant.", "A cobiça é a única conta que cresce quanto mais você tira. Matematicamente desagradável."),
                ("You can't have the flowers! Those are mine! I mean they're everybody's!", "Tu não pode levar as flores! São minhas! Quer dizer, são de todo mundo!"));
            Line(entries, RegionBossRules.AzCaptainId, RegionBossPhase.Storm, "Āz throws the garden's stolen light!", "Āz atira a luz roubada do jardim!");
            Line(entries, RegionBossRules.AzCaptainId, RegionBossPhase.LastStand, "The Ark wants it all!", "A Arca quer tudo!");
            Blow(entries, RegionBossRules.AzCaptainId, "Her boot comes down!", "Lá vem a bota!");
            Signature(entries, RegionBossRules.AzCaptainId, "The Ark is opening! It takes from what you have!", "A Arca abriu! Ela leva do que vocês têm!");
            Withdraw(entries, RegionBossRules.AzCaptainId, "Āz gives the ark back and stands there, ashamed, in front of the whole garden.", "Āz devolve a arca e fica ali com vergonha, na frente do jardim inteiro.");
        }

        private static void Signature(List<LocalizationEntry> entries, string enemyId, string en, string pt) =>
            entries.Add(new LocalizationEntry(SignatureKey(enemyId), en, pt));

        private static void Withdraw(List<LocalizationEntry> entries, string enemyId, string en, string pt) =>
            entries.Add(new LocalizationEntry(WithdrawKey(enemyId), en, pt));

        private static void Form(List<LocalizationEntry> entries, GreatDarkForm form, string en, string pt) =>
            entries.Add(new LocalizationEntry(FormKey(form), en, pt));

        private static void Line(List<LocalizationEntry> entries, string enemyId, RegionBossPhase phase, string en, string pt) =>
            entries.Add(new LocalizationEntry(PhaseKey(enemyId, phase), en, pt));

        private static void Blow(List<LocalizationEntry> entries, string enemyId, string en, string pt) =>
            entries.Add(new LocalizationEntry(GreatBlowKey(enemyId), en, pt));

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
