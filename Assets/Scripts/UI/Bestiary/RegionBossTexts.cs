using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the world bosses of Regions 5–16 (Docs/Mapas100.md 30, 35, 40, 46, 52, 58, 64, 70, 76, 82, 90, 96) and the short lines they call out in the fight, en + pt-BR.
    /// Kept apart like <see cref="CeuBestiaryTexts"/>; <see cref="BestiaryTexts"/> appends these to its entries. The Rei-Vendaval is the Vento Destruidor and the
    /// Glutão the Fogo Devorador (bible §1.3), not gods: they lose and go back, like the Dark.
    /// </summary>
    public static partial class RegionBossTexts
    {
        public static readonly string[] EnemyIds = 
        {
            RegionBossRules.WindKingId, RegionBossRules.OldKulId, RegionBossRules.EmberGluttonId,
            RegionBossRules.ApepId, RegionBossRules.DreamOfDestructionId, RegionBossRules.GreatDarkId, RegionBossRules.AzCaptainId,
            RegionBossRules.HedammuId, RegionBossRules.SwallowingMireId, RegionBossRules.AydaharId, RegionBossRules.FourGluttonsId, RegionBossRules.OhyaId,
            RegionBossRules.RopeCutterId, RegionBossRules.EndlessBellId, RegionBossRules.WalrusQueenId, RegionBossRules.GrandfatherMammothId,
            RegionBossRules.ShisaKingId, RegionBossRules.FurnaceMasterId,
            RegionBossRules.MonsoonOctopusId, RegionBossRules.SmokeAurochsId, RegionBossRules.ForgetfulBridgeId, RegionBossRules.RunawayCartId,
            RegionBossRules.GreatAlmasId, RegionBossRules.UnleapedCarpId,
            RegionBossRules.ForgottenSpinnerId, RegionBossRules.WellDrinkingRootId, RegionBossRules.GreatSmudgeId, RegionBossRules.NinkiNankaId,
            RegionBossRules.NianId, RegionBossRules.OldKurganGriffinId,
        };

        public static string PhaseKey(string enemyId, RegionBossPhase phase) => $"region_boss.{enemyId}.phase{(int)phase}";

        /// <summary>The line every boss without one of its own calls out on reaching <paramref name="phase"/>.</summary>
        public static string GenericPhaseKey(RegionBossPhase phase) => $"region_boss.phase{(int)phase}";

        /// <summary>
        /// The key to show when a boss reaches <paramref name="phase"/>: its own line (<see cref="PhaseKey"/>) when it has one, else
        /// <see cref="GenericPhaseKey"/>; null for <see cref="RegionBossPhase.Wrath"/>, the opening phase, which nobody calls out.
        /// </summary>
        public static string PhaseWarningKey(string enemyId, RegionBossPhase phase)
        {
            if (phase <= RegionBossPhase.Wrath)
            {
                return null;
            }

            string own = PhaseKey(enemyId, phase);
            return BestiaryTexts.GetFallback(own) != null ? own : GenericPhaseKey(phase);
        }

        public static string GreatBlowKey(string enemyId) => $"region_boss.{enemyId}.great_blow";

        public static string SignatureKey(string enemyId) => $"region_boss.{enemyId}.signature";

        public static string WithdrawKey(string enemyId) => $"region_boss.{enemyId}.withdraw";

        public static string FormKey(GreatDarkForm form) => $"region_boss.great_dark.form.{form.ToString().ToLowerInvariant()}";

        public static string AppetiteKey(GluttonAppetite appetite) => $"region_boss.four_gluttons.appetite.{appetite.ToString().ToLowerInvariant()}";

        // Taotie's offerings and Qiongqi's swapped steps (RegionBossAbilities); {0} is a number.
        public const string TaotieSpillKey = "region_boss.four_gluttons.taotie.spill";
        public const string TaotieEatWarningKey = "region_boss.four_gluttons.taotie.eat_warning";
        public const string TaotieEatenKey = "region_boss.four_gluttons.taotie.eaten";
        public const string TaotieMissedKey = "region_boss.four_gluttons.taotie.missed";
        public const string TaotieReturnedKey = "region_boss.four_gluttons.taotie.returned";
        public const string QiongqiMirrorKey = "region_boss.four_gluttons.qiongqi.mirror";
        public const string QiongqiMirrorEndKey = "region_boss.four_gluttons.qiongqi.mirror_end";

        /// <summary>The fight lines of Taotie's offerings and Qiongqi's swap.</summary>
        public static readonly string[] FourGluttonsFightKeys =
        {
            TaotieSpillKey, TaotieEatWarningKey, TaotieEatenKey, TaotieMissedKey, TaotieReturnedKey, QiongqiMirrorKey, QiongqiMirrorEndKey,
        };

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

            // -- Mangues de Liyongo (30) --
            Page(entries, RegionBossRules.HedammuId,
                "Hedammu is the sea serpent of the old Hurrian song, the one who ate everything and was never full, and Āz is very fond of her for that. She coils up in the Largo in the middle of the mangroves and swallows whatever drifts by: fish, boats, the little sparks of light the tide brings in. You can see them shining through her scales. When she opens that mouth she bites straight ahead, wide as a doorway, so don't stand in front of her, stand beside her. Beaten, she spits out every spark she swallowed and goes back down to the bottom, smaller and calmer. She'll be back. Appetites always come back. But a little smaller each time, and that is how it's done.",
                "Hedammu é a serpente do mar da canção hurrita antiga, a que comia tudo e nunca enchia, e Āz gosta muito dela por isso. Ela se enrola no Largo no meio dos mangues e engole o que passar: peixe, barco, as faisquinhas de luz que a maré traz. Dá pra ver elas brilhando por baixo das escamas. Quando ela abre aquela boca, morde reto pra frente, larga como uma porta, então não fiquem na frente dela, fiquem do lado. Vencida, cospe todas as faíscas que engoliu e volta pro fundo, menor e mais calma. Ela volta. Apetite sempre volta. Mas um pouco menor de cada vez, e é assim que se faz.",
                ("The mouth only goes one way. Be the other way.", "A boca só vai pra um lado. Seja o outro lado."),
                ("She's so hungry she can't even taste it. That's the saddest part.", "Ela tem tanta fome que nem sente o gosto. Essa é a parte mais triste."),
                ("Hey, chew! Nobody's going to take it from you!", "Ei, mastiga! Ninguém vai tirar de você!"),
                ("Everything goes in and nothing fills her. The sparks don't belong to her, that's why.", "Tudo entra e nada enche. As faíscas não são dela, por isso."),
                ("She ate the little lights! Spit them out, they're scared in there!", "Ela comeu as luzinhas! Cospe, elas tão com medo aí dentro!"));
            Line(entries, RegionBossRules.HedammuId, RegionBossPhase.Storm, "Hedammu spits the tide!", "Hedammu cospe a maré!");
            Line(entries, RegionBossRules.HedammuId, RegionBossPhase.LastStand, "She wants all of it now!", "Agora ela quer tudo!");
            Blow(entries, RegionBossRules.HedammuId, "Her coils come down!", "Lá vêm os anéis dela!");
            Signature(entries, RegionBossRules.HedammuId, "Hedammu opens her mouth! Get beside her!", "Hedammu abriu a boca! Pro lado dela!");
            Withdraw(entries, RegionBossRules.HedammuId, "Hedammu spits out the sparks she swallowed and sinks back to the bottom, smaller and calmer.", "Hedammu cospe as faíscas que engoliu e volta pro fundo, menor e mais calma.");

            // -- Coração do Lodo (35) --
            Page(entries, RegionBossRules.SwallowingMireId,
                "The Mire is the fourth of the five worlds of the Darkness, the murky water, and its people are the ones who swim. Down at the bottom of the delta it stands up as mud with no face and swallows: the roots, the clean water, the ground under your feet. It has no bones to break, children, so it can't be beaten for good. It draws back when the five springs are lit and the clean water pushes it away, like the Prince himself. When it goes still and the mud starts to shine round it, get out of the mud, and get further out in the end, because by then it reaches more. At night it grows by itself. Light the springs anyway.",
                "O Lodo é o quarto dos cinco mundos da Treva, a água turva, e o povo dele são os que nadam. Lá no fundo do delta ele se levanta como lama sem rosto e engole: as raízes, a água limpa, o chão debaixo dos pés. Não tem osso pra quebrar, crianças, então não se vence de vez. Ele recua quando as cinco fontes acendem e a água limpa empurra ele pra longe, como o Príncipe. Quando ele ficar parado e a lama começar a brilhar em volta, saiam da lama, e saiam mais longe no fim, que aí ele alcança mais. De noite ele cresce sozinho. Acendam as fontes mesmo assim.",
                ("When it stops moving, you start. Out of the mud, every time.", "Quando ele para de mexer, você começa. Pra fora da lama, toda vez."),
                ("It swallowed the clean water and it still looks thirsty.", "Engoliu a água limpa e ainda parece com sede."),
                ("My boots are never going to be the same. Neither is my dignity.", "Minha bota nunca mais vai ser a mesma. Nem minha dignidade."),
                ("No face, no bones, no centre. It is the shape of being stuck.", "Sem rosto, sem osso, sem centro. É o formato de estar atolado."),
                ("It took the water! Give it back, the fishes need it!", "Ele pegou a água! Devolve, os peixinhos precisam!"));
            Line(entries, RegionBossRules.SwallowingMireId, RegionBossPhase.Storm, "The mud boils!", "A lama ferve!");
            Line(entries, RegionBossRules.SwallowingMireId, RegionBossPhase.LastStand, "The Mire spreads out!", "O Lodo se espalha!");
            Blow(entries, RegionBossRules.SwallowingMireId, "The mud comes up under you!", "A lama sobe debaixo de você!");
            Signature(entries, RegionBossRules.SwallowingMireId, "The Mire swallows! Out of the mud!", "O Lodo vai engolir! Sai da lama!");
            Withdraw(entries, RegionBossRules.SwallowingMireId, "The Mire sinks back into the bottom of the delta. The clean water pushes it away, for now.", "O Lodo afunda de volta no fundo do delta. A água limpa empurra ele pra longe, por enquanto.");

            // -- Passo de Wakhan (40) --
            Page(entries, RegionBossRules.AydaharId,
                "Aydahar is the dragon of the old steppe tales, the long one with whiskers, and this one has been coiled across the Wakhan Pass for longer than anyone remembers, lying on Āz's gold without ever asking why. The scales went gold from lying on it. It isn't wicked, I don't think. It is simply guarding, the way a dog guards a door nobody uses. When it draws breath and goes quiet, it will breathe straight down the pass at you: one step to the side and the fire goes past. Set it free and it flies off, and the pass opens again for the caravans. That is the best ending a guard can have.",
                "Aydahar é o dragão dos contos antigos da estepe, o comprido de bigode, e este está enrolado no Passo de Wakhan há mais tempo do que alguém lembra, deitado no ouro de Āz sem nunca perguntar por quê. As escamas ficaram douradas de tanto deitar em cima. Não acho que seja mau. Só está guardando, como um cachorro guarda uma porta que ninguém usa. Quando ele puxar o ar e ficar quieto, vai soprar reto passo abaixo em vocês: um passo pro lado e o fogo passa. Libertem ele e ele voa embora, e o passo abre de novo pras caravanas. É o melhor fim que um guarda pode ter.",
                ("Straight lines are easy to leave. Sidestep and hit.", "Linha reta é fácil de sair. Passo pro lado e bate."),
                ("Guarding gold he never wanted. Somebody should tell him he can go.", "Guardando um ouro que ele nem queria. Alguém precisa dizer que ele pode ir."),
                ("Nice whiskers. Not as nice as mine will be.", "Belo bigode. Não tão bonito quanto o meu vai ser."),
                ("The fire travels along the pass. The pass is the problem, not the dragon.", "O fogo corre pelo passo. O problema é o passo, não o dragão."),
                ("Can I ride him after? Just a little? He looks soft!", "Posso andar nele depois? Só um pouquinho? Ele parece fofinho!"));
            Line(entries, RegionBossRules.AydaharId, RegionBossPhase.Storm, "Aydahar spits embers!", "Aydahar cospe brasa!");
            Line(entries, RegionBossRules.AydaharId, RegionBossPhase.LastStand, "The dragon uncoils!", "O dragão se desenrola!");
            Blow(entries, RegionBossRules.AydaharId, "Its tail comes down!", "Lá vem a cauda!");
            Signature(entries, RegionBossRules.AydaharId, "Aydahar draws breath! Step aside!", "Aydahar puxou o ar! Sai da frente!");
            Withdraw(entries, RegionBossRules.AydaharId, "Aydahar shakes off the gold and flies away. The pass is open.", "Aydahar sacode o ouro e voa embora. O passo está aberto.");

            // -- Cidadela dos Quatro Gulosos (46) --
            Page(entries, RegionBossRules.FourGluttonsId,
                "The old legends of the plain speak of four perils, and here they are, sharing one bronze body like four people sharing one blanket: Taotie the glutton, Hundun the chaos with no face, Qiongqi who turns right into wrong, and Taowu who never listens. Each is one appetite of Āz left on its own. One of them is in charge each week, and at night the palaces swap and the next one takes over. Taotie eats the ground you stand on, so move, and it eats the offerings the vessel spills too: grab them while you can, or let it swallow them and it gives them back double when it falls, if it falls. Hundun puts out the light except where it marked, so stay. Qiongqi is safe up close, and it swaps your steps round for a moment: left goes right, so walk the other way and don't panic, it passes. Taowu strikes all round itself, so keep away. Beaten, they go back to being figures on the old vessels. Good. That's where they belong.",
                "As lendas antigas da planície falam de quatro perigos, e aqui estão eles, dividindo um corpo de bronze como quatro pessoas dividindo um cobertor: Taotie, o guloso, Hundun, o caos sem rosto, Qiongqi, que troca o certo pelo errado, e Taowu, que nunca escuta. Cada um é um apetite de Āz que ficou sozinho. Um deles manda em cada semana, e de noite os palácios trocam e o seguinte assume. Taotie come o chão onde vocês pisam, então saiam, e come também as oferendas que o vaso derrama: peguem enquanto dá, ou deixem ele engolir, que ele devolve em dobro quando cair, se cair. Hundun apaga a luz menos onde marcou, então fiquem. Com Qiongqi, perto é seguro, e ele troca os passos de vocês por um instante: esquerda vira direita, então andem pro outro lado e sem pânico, que passa. Taowu bate em volta de si, então fiquem longe. Vencidos, voltam a ser figuras nos vasos antigos. Ótimo. Lugar deles é lá.",
                ("Listen to which one shouts. Then do what it hates.", "Escuta qual deles grita. Depois faz o que ele odeia."),
                ("Four of them in one body and every one of them alone.", "Quatro num corpo só e cada um sozinho."),
                ("Let it eat the coins! It gives them back double! ...Unless we lose. Let's not lose.", "Deixa ele comer as moedas! Devolve em dobro! ...A não ser que a gente perca. Vamos não perder."),
                ("Qiongqi swaps left and right for three seconds. Three seconds is a rule too. Count, then walk.", "Qiongqi troca esquerda e direita por três segundos. Três segundos também é regra. Conta e anda."),
                ("My feet went the wrong way! Do it again! No, don't!", "Meu pé foi pro lado errado! Faz de novo! Não, não faz!"));
            Line(entries, RegionBossRules.FourGluttonsId, RegionBossPhase.Storm, "The vessel rings!", "O vaso toca!");
            Line(entries, RegionBossRules.FourGluttonsId, RegionBossPhase.LastStand, "All four are hungry!", "Os quatro estão com fome!");
            Blow(entries, RegionBossRules.FourGluttonsId, "The bronze comes down!", "Lá vem o bronze!");
            Signature(entries, RegionBossRules.FourGluttonsId, "An appetite wakes!", "Um apetite acordou!");
            Appetite(entries, GluttonAppetite.Taotie, "Taotie eats the ground! Off the mark!", "Taotie come o chão! Sai da marca!");
            Appetite(entries, GluttonAppetite.Hundun, "Hundun puts out the light! Stay on the mark!", "Hundun apaga a luz! Fica na marca!");
            Appetite(entries, GluttonAppetite.Qiongqi, "Qiongqi will swap your steps! Come close!", "Qiongqi vai trocar seus passos! Chega perto!");
            Appetite(entries, GluttonAppetite.Taowu, "Taowu won't listen! Keep away!", "Taowu não escuta! Fica longe!");
            entries.Add(new LocalizationEntry(TaotieSpillKey, "The vessel spills an offering!", "O vaso derramou uma oferenda!"));
            entries.Add(new LocalizationEntry(TaotieEatWarningKey, "Taotie wants the offering! Grab it or let it go!", "Taotie quer a oferenda! Pega ou deixa!"));
            entries.Add(new LocalizationEntry(TaotieEatenKey, "Taotie swallowed it! Back double if it falls ({0})", "Taotie engoliu! Volta em dobro se ele cair ({0})"));
            entries.Add(new LocalizationEntry(TaotieMissedKey, "Taotie bit the air!", "Taotie mordeu o vento!"));
            entries.Add(new LocalizationEntry(TaotieReturnedKey, "Taotie gives it all back double: +{0}", "Taotie devolve tudo em dobro: +{0}"));
            entries.Add(new LocalizationEntry(QiongqiMirrorKey, "Steps swapped! Walk the other way ({0} s)", "Passos trocados! Anda pro outro lado ({0} s)"));
            entries.Add(new LocalizationEntry(QiongqiMirrorEndKey, "Your steps are yours again", "Os passos voltaram ao normal"));

            // -- Corrente de Amirani (52) --
            Page(entries, RegionBossRules.OhyaId,
                "Ohya is one of the two giant brothers of the old Book of the Giants, and Hahya, the one you met at the pass, is the other. Hahya has come back to himself. Ohya hasn't, not yet. He chained himself to the mountain so as not to fight anyone, he says, and then he fights everyone who climbs up, which is very like a brother. When he swings the chain round, it passes at its full length: get in close against his legs or stay well out of reach. Don't hate him for it. Beaten, he lets go of the chain and walks down to live with Hahya, and I'm told they argue about the cooking now. Which is the right thing for brothers to argue about.",
                "Ohya é um dos dois irmãos gigantes do antigo Livro dos Gigantes, e o Hahya, o que vocês conheceram no passo, é o outro. O Hahya voltou a si. O Ohya não, ainda não. Ele se prendeu à montanha pra não brigar com ninguém, diz ele, e depois briga com todo mundo que sobe, o que é bem coisa de irmão. Quando ele girar a corrente, ela passa no comprimento todo: cheguem bem perto das pernas dele ou fiquem bem fora do alcance. Não tenham raiva dele. Vencido, ele solta a corrente e desce pra morar com o Hahya, e me contaram que agora eles brigam por causa da comida. Que é a briga certa pra irmão ter.",
                ("The chain has a length. Either inside it or outside it, never on it.", "A corrente tem um comprimento. Ou dentro, ou fora, nunca em cima."),
                ("He tied himself up so he wouldn't hurt anyone. He just needs someone to untie him.", "Ele se amarrou pra não machucar ninguém. Só precisa de alguém que desamarre."),
                ("I fight with my sister too. Not with a chain. Usually.", "Eu também brigo com minha irmã. Não com corrente. Normalmente."),
                ("A giant who chains himself to stop fighting and fights anyway. The chain was never the point.", "Um gigante que se acorrenta pra parar de brigar e briga mesmo assim. A corrente nunca foi o ponto."),
                ("He's so big! I bet I can still beat him! I'm not small!", "Ele é muito grande! Aposto que eu ganho dele! Eu não sou pequena!"));
            Line(entries, RegionBossRules.OhyaId, RegionBossPhase.Storm, "Ohya throws the snow!", "Ohya joga a neve!");
            Line(entries, RegionBossRules.OhyaId, RegionBossPhase.LastStand, "The giant tears at the chain!", "O gigante puxa a corrente!");
            Blow(entries, RegionBossRules.OhyaId, "His fist comes down!", "Lá vem o punho!");
            Signature(entries, RegionBossRules.OhyaId, "The chain swings round! In close or far away!", "A corrente vai girar! Bem perto ou bem longe!");

            AddMapBossPages(entries);

            // -- Fallback for a boss without its own phase line --
            entries.Add(new LocalizationEntry(GenericPhaseKey(RegionBossPhase.Storm), "It's getting angrier!", "Ele ficou mais bravo!"));
            entries.Add(new LocalizationEntry(GenericPhaseKey(RegionBossPhase.LastStand), "It's fighting with everything now!", "Agora ele luta com tudo!"));
        }

        private static void Signature(List<LocalizationEntry> entries, string enemyId, string en, string pt) =>
            entries.Add(new LocalizationEntry(SignatureKey(enemyId), en, pt));

        private static void Withdraw(List<LocalizationEntry> entries, string enemyId, string en, string pt) =>
            entries.Add(new LocalizationEntry(WithdrawKey(enemyId), en, pt));

        private static void Appetite(List<LocalizationEntry> entries, GluttonAppetite appetite, string en, string pt) =>
            entries.Add(new LocalizationEntry(AppetiteKey(appetite), en, pt));

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
