using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.ClassEvolution
{
    /// <summary>
    /// en + pt-BR text of the class evolution (FASE 16, Docs/ClassEvolution.md): graduation titles, the 15 story missions
    /// (mentor offer / progress / turn-in, hero reply), accessories, graduation skills (<c>skill.&lt;id&gt;.*</c>), class combos
    /// (<c>combo.&lt;id&gt;.*</c>), master tree and UI lines. Same self-contained pattern as the other *Texts helpers: merged into
    /// the live <see cref="LocalizationManager"/> on first use (keys it already has win), pt-BR fallback without one.
    /// </summary>
    public static class ClassEvolutionTexts
    {
        public const string Title = "ui.class.title";
        /// <summary>{0} title.</summary>
        public const string CurrentTitle = "ui.class.current_title";
        /// <summary>{0} tier, {1} max.</summary>
        public const string TierLine = "ui.class.tier";
        public const string StatusAllDone = "ui.class.status.all_done";
        /// <summary>{0} level.</summary>
        public const string StatusLevelTooLow = "ui.class.status.level_too_low";
        /// <summary>{0} mentor, {1} mission title.</summary>
        public const string StatusAvailable = "ui.class.status.available";
        /// <summary>{0} mission title, {1} kills, {2} target, {3} region.</summary>
        public const string StatusInProgress = "ui.class.status.in_progress";
        /// <summary>{0} mentor.</summary>
        public const string StatusReady = "ui.class.status.ready";
        public const string Accept = "ui.class.accept";
        public const string TurnIn = "ui.class.turn_in";
        /// <summary>{0} new title.</summary>
        public const string Graduated = "ui.class.graduated";
        /// <summary>{0} skill name.</summary>
        public const string NewSkill = "ui.class.new_skill";
        /// <summary>{0} accessory name.</summary>
        public const string NewAccessory = "ui.class.new_accessory";
        /// <summary>{0} combo name.</summary>
        public const string ComboUnlocked = "ui.class.combo_unlocked";
        /// <summary>{0} combo name.</summary>
        public const string ComboDone = "ui.class.combo_done";
        /// <summary>{0} master level.</summary>
        public const string MasterTitle = "ui.class.master.title";
        /// <summary>{0} required level.</summary>
        public const string MasterLocked = "ui.class.master.locked";
        /// <summary>{0} points.</summary>
        public const string MasterPoints = "ui.class.master.points";
        /// <summary>{0} xp, {1} xp to next.</summary>
        public const string MasterXp = "ui.class.master.xp";
        /// <summary>{0} master level.</summary>
        public const string MasterLevelUp = "ui.class.master.level_up";
        public const string MasterMax = "ui.class.master.max";
        public const string MasterReset = "ui.class.master.reset";
        /// <summary>{0} rank, {1} max rank.</summary>
        public const string MasterRank = "ui.class.master.rank";
        public const string MasterNoPoints = "ui.class.master.no_points";
        public const string MasterMaxRank = "ui.class.master.max_rank";
        /// <summary>{0} ranks needed, {1} node name.</summary>
        public const string MasterNodeLocked = "ui.class.master.node_locked";
        /// <summary>Map HUD button that opens the graduation panel (in the Vila, with the family).</summary>
        public const string Button = "ui.class.button";
        public const string Close = "ui.class.close";
        public const string SectionMission = "ui.class.section.mission";
        public const string SectionCombo = "ui.class.section.combo";
        public const string SectionMaster = "ui.class.section.master";
        public const string NoHero = "ui.class.no_hero";
        /// <summary>{0} speaker, {1} line.</summary>
        public const string SpeakerLine = "ui.class.speaker_line";
        /// <summary>{0} combo tier.</summary>
        public const string ComboLocked = "ui.class.combo_locked";
        public const string MasterSpend = "ui.class.master.spend";
        /// <summary>{0} points refunded.</summary>
        public const string MasterResetDone = "ui.class.master.reset_done";

        public static string RegionKey(BiomeType region) => "ui.class.region." + region.ToString().ToLowerInvariant();

        public static readonly List<LocalizationEntry> Entries = Build();

        private static List<LocalizationEntry> Build()
        {
            var list = new List<LocalizationEntry>
            {
                new LocalizationEntry(Title, "Graduation", "Graduação"),
                new LocalizationEntry(CurrentTitle, "Title: {0}", "Título: {0}"),
                new LocalizationEntry(TierLine, "Graduation {0} of {1}", "Graduação {0} de {1}"),
                new LocalizationEntry(StatusAllDone, "Every graduation done. Now grow in the Master Level.", "Todas as graduações feitas. Agora é crescer no Nível de Mestre."),
                new LocalizationEntry(StatusLevelTooLow, "Next graduation at level {0}.", "Próxima graduação no nível {0}."),
                new LocalizationEntry(StatusAvailable, "{0} has a mission for you: {1}.", "{0} tem uma missão pra você: {1}."),
                new LocalizationEntry(StatusInProgress, "{0}: {1}/{2} creatures in the {3}.", "{0}: {1}/{2} bichos em {3}."),
                new LocalizationEntry(StatusReady, "Mission done! Talk to {0}.", "Missão feita! Fale com {0}."),
                new LocalizationEntry(Accept, "Accept mission", "Aceitar missão"),
                new LocalizationEntry(TurnIn, "Hand in", "Entregar"),
                new LocalizationEntry(Graduated, "Graduated! Now you are {0}.", "Graduou! Agora é {0}."),
                new LocalizationEntry(NewSkill, "New skill: {0}", "Skill nova: {0}"),
                new LocalizationEntry(NewAccessory, "New look: {0}", "Visual novo: {0}"),
                new LocalizationEntry(ComboUnlocked, "Class combo unlocked: {0}", "Combo da classe liberado: {0}"),
                new LocalizationEntry(ComboDone, "Combo: {0}!", "Combo: {0}!"),
                new LocalizationEntry(MasterTitle, "Master Level {0}", "Nível de Mestre {0}"),
                new LocalizationEntry(MasterLocked, "The Master Level opens at level {0} with the 3rd graduation.", "O Nível de Mestre abre no nível {0} com a 3ª graduação."),
                new LocalizationEntry(MasterPoints, "Master points: {0}", "Pontos de mestre: {0}"),
                new LocalizationEntry(MasterXp, "Master XP: {0}/{1}", "XP de mestre: {0}/{1}"),
                new LocalizationEntry(MasterLevelUp, "Master Level {0}! +1 point.", "Nível de Mestre {0}! +1 ponto."),
                new LocalizationEntry(MasterMax, "Master Level maxed out.", "Nível de Mestre no máximo."),
                new LocalizationEntry(MasterReset, "Start over (points come back)", "Começar de novo (os pontos voltam)"),
                new LocalizationEntry(MasterRank, "{0}/{1}", "{0}/{1}"),
                new LocalizationEntry(MasterNoPoints, "No master points left.", "Sem pontos de mestre sobrando."),
                new LocalizationEntry(MasterMaxRank, "This one is already full.", "Esse já está cheio."),
                new LocalizationEntry(MasterNodeLocked, "Needs {0} points in {1}.", "Pede {0} pontos em {1}."),
                new LocalizationEntry(Button, "Graduation", "Graduação"),
                new LocalizationEntry(Close, "Close", "Fechar"),
                new LocalizationEntry(SectionMission, "Family mission", "Missão da família"),
                new LocalizationEntry(SectionCombo, "Class combo", "Combo da classe"),
                new LocalizationEntry(SectionMaster, "Master Tree", "Árvore de Mestre"),
                new LocalizationEntry(NoHero, "Pick a cousin first.", "Escolha um primo primeiro."),
                new LocalizationEntry(SpeakerLine, "{0}: \"{1}\"", "{0}: \"{1}\""),
                new LocalizationEntry(ComboLocked, "Opens with graduation {0}.", "Abre com a graduação {0}."),
                new LocalizationEntry(MasterSpend, "+1 point", "+1 ponto"),
                new LocalizationEntry(MasterResetDone, "{0} points came back.", "{0} pontos voltaram."),

                new LocalizationEntry(RegionKey(BiomeType.Forest), "Zarafshan Woods", "Bosque do Zarafshan"),
                new LocalizationEntry(RegionKey(BiomeType.Desert), "Flaming Sands", "Areias Flamejantes"),
                new LocalizationEntry(RegionKey(BiomeType.Mountain), "Kaggen Range", "Serra de Kaggen"),
                new LocalizationEntry(RegionKey(BiomeType.Caves), "Threshold Caves", "Grutas do Limiar"),

                new LocalizationEntry(ClassEvolutionCatalog.MentorNameKey("maithe"), "Maithe", "Maithe"),
                new LocalizationEntry(ClassEvolutionCatalog.MentorNameKey("maria"), "Maria", "Maria"),
                new LocalizationEntry(ClassEvolutionCatalog.MentorNameKey("joaquim"), "Joaquim", "Joaquim"),
                new LocalizationEntry(ClassEvolutionCatalog.MentorNameKey("icaro"), "Ícaro", "Ícaro"),
                new LocalizationEntry(ClassEvolutionCatalog.MentorNameKey("lavine"), "Lavine", "Lavine"),
                new LocalizationEntry(ClassEvolutionCatalog.MentorNameKey(ClassEvolutionCatalog.VoEdna), "Grandma Edna", "Vó Edna"),
            };

            // ---------------------------------------------------------------- Maithe
            Grad(list, CharacterId.Maithe, 1, "Falcon Tracker", "Rastreadora do Falcão", "The Wind Meter", "Medidor de Vento",
                "Sis, I built a wind meter! Statistically your Eye of the Wind misses less with it. Test it in the Flaming Sands: 15 creatures. Don't tell anyone I helped.",
                "Mana, montei um medidor de vento! Estatisticamente o seu Olho do Vento erra menos com ele. Testa nas Areias Flamejantes: 15 bichos. Não conta pra ninguém que eu ajudei.",
                "{0}/{1} in the {2}. The needle is spinning, Sis!", "{0}/{1} no {2}. O ponteiro tá girando, mana!",
                "See? Science. Now you read the Living Wind like a saker falcon reads the ground. Call us to your side when you aim.",
                "Viu? Ciência. Agora você lê o Vento Vivo como um falcão-sacre lê o chão. Chama a gente pro seu lado quando mirar.",
                "Fine, pest. It worked. Just this once. My wind doesn't lie.", "Tá bom, pestinha. Funcionou. Só dessa vez. Meu vento não mente.");
            Grad(list, CharacterId.Maithe, 2, "Gale Archer", "Flecheira da Ventania", "Three Months and Eleven Days", "Três Meses e Onze Dias",
                "Cousin, in the Threshold Caves I go first, with the light. You cover us from behind. Twenty creatures. Trust me?",
                "Prima, nas Grutas do Limiar quem vai na frente sou eu, com a luz. Você cobre a gente lá de trás. Vinte bichos. Confia?",
                "{0}/{1} in the {2}. Breathe, cousin. I'm right here in front.", "{0}/{1} nas {2}. Respira, prima. Eu tô aqui na frente.",
                "See how the arrow flies better when you don't have to look back? Three months older and still learning.",
                "Viu como a flecha voa melhor quando você não precisa olhar pra trás? Três meses mais velha e ainda aprendendo.",
                "Don't start with the three months. ...Thank you, Maria.", "Não começa com os três meses. ...Obrigada, Maria.");
            Grad(list, CharacterId.Maithe, 3, "Eye of Liyongo", "Olho de Liyongo", "The Apricot-Wood Bow", "O Arco de Damasqueiro",
                "My eldest, your Grandpa Jorge carved your first bow from an apricot branch in the orchard. He used to say the wind of the Kaggen Range teaches more than he did, and he told of Liyongo, the archer-poet of the coast. Go on, thirty creatures, and come back for tea.",
                "Minha mais velha, o seu Vô Jorge entalhou o seu primeiro arco de um galho do damasqueiro do pomar. Ele dizia que o vento da Serra de Kaggen ensina mais que ele, e contava de Liyongo, o arqueiro-poeta da costa. Vai lá, trinta bichos, e volta pra tomar chá.",
                "{0}/{1} on the {2}. Grandma lit a lamp for you, dear.", "{0}/{1} na {2}. A Vó acendeu uma lâmpada por você, minha querida.",
                "You came back with your grandfather's eyes. Take his felt cap; you're big enough for it now.",
                "Você voltou com o olhar do seu avô. Toma o gorro de feltro dele; você já tem tamanho pra ele.",
                "It fits, Grandma. I count the cousins and I always come back.", "Ele cabe, Vó. Eu conto os primos e volto sempre.");

            // ---------------------------------------------------------------- Maria
            Grad(list, CharacterId.Maria, 1, "Sun Healer", "Curandeira do Sol", "Who Looks After the Guardian", "Quem Cuida do Guardião",
                "Sis, I'm not tired! ...Okay, a little. In the Flaming Sands everybody gets hurt together. Can you make a light that looks after everyone at once? Fifteen creatures, I'll go first!",
                "Mana, eu não tô cansado! ...Tá, um pouquinho. Nas Areias Flamejantes todo mundo se machuca junto. Inventa uma luz que cuide de todo mundo de uma vez? Quinze bichos, eu vou na frente!",
                "{0}/{1} in the {2}. Bring it on! ...Sis, heal me here?", "{0}/{1} no {2}. Pode vir! ...Mana, cura aqui?",
                "I felt it! It hit everyone at once! You're the best healer on the whole island, Sis.",
                "Senti! Foi todo mundo de uma vez! Você é a melhor curandeira da ilha inteira, mana.",
                "Go with light, little brother. And ask to sit down when you're tired.", "Vai com luz, maninho. E pede pra sentar quando cansar.");
            Grad(list, CharacterId.Maria, 2, "Dawn Guardian", "Guardiã da Aurora", "Light and Shadow", "Luz e Sombra",
                "Cousin, the dark in the Threshold Caves doesn't scare me anymore. But your light and my shadow look pretty together. Twenty creatures. Are you coming?",
                "Prima, o escuro das Grutas do Limiar não me assusta mais. Mas a sua luz e a minha sombra juntas ficam bonitas. Vinte bichos. Você vem?",
                "{0}/{1} in the {2}. The shadow is soft today, cousin.", "{0}/{1} nas {2}. A sombra tá macia hoje, prima.",
                "Your light didn't put me out. It stayed beside me. I say what I think: it was beautiful.",
                "A sua luz não me apagou. Ficou do lado. Eu falo o que penso: foi bonito.",
                "In the dream there was light and shadow holding hands. It was you, Lavine.", "No sonho tinha luz e sombra de mãos dadas. Era você, Lavine.");
            Grad(list, CharacterId.Maria, 3, "Lantern of Glory", "Candeia da Glória", "The Little Sun Medal", "A Medalhinha de Sol",
                "My little dreamer, the medal I hung on your neck on your first day at the Temple is asking for more light. Up on the Kaggen Range the sun rises first, and your Grandpa Jorge used to say that's where the Column of Glory starts. Thirty creatures, then come back.",
                "Minha netinha sonhadora, a medalhinha que pendurei no seu pescoço no primeiro dia de Templo tá pedindo mais luz. Lá no alto da Serra de Kaggen o sol nasce primeiro, e o seu Vô Jorge dizia que é lá que começa a Coluna da Glória. Trinta bichos e volta.",
                "{0}/{1} on the {2}. There's date bread waiting for you.", "{0}/{1} na {2}. Tem pão de tâmara te esperando.",
                "Now you're a lantern, Maria: you light other people's way without going out. Your Grandpa Jorge would have loved this.",
                "Agora você é candeia, Maria: acende o caminho dos outros sem se apagar. O Vô Jorge ia adorar isso.",
                "You always knew, didn't you, Grandma?", "A senhora sempre soube, né, Vó?");

            // ---------------------------------------------------------------- Joaquim
            Grad(list, CharacterId.Joaquim, 1, "Guardian of the Roots", "Guardião das Raízes", "The Barrel Lid", "A Tampa de Barril",
                "Little brother, remember when you saved me from the geese with a barrel lid? The Shield of Roots is the same: it holds everyone, not just you. Fifteen creatures in the Flaming Sands, and let the roots hug us.",
                "Maninho, lembra quando você me protegeu dos gansos com uma tampa de barril? O Escudo das Raízes é a mesma coisa: ele segura todo mundo, não só você. Quinze bichos nas Areias Flamejantes, e deixa as raízes abraçarem a gente.",
                "{0}/{1} in the {2}. Breathe, little brother. No need to go so far ahead.", "{0}/{1} no {2}. Respira, maninho. Não precisa ir tão na frente.",
                "The roots held both of us. You've grown, little brother.", "As raízes seguraram nós dois. Você cresceu, maninho.",
                "Leave it to me, Sis! My water lets nothing through. ...And the geese had better watch out.", "Deixa comigo, mana! A minha água não deixa passar. ...E os gansos que se cuidem.");
            Grad(list, CharacterId.Joaquim, 2, "Guardian of the Tugai", "Guardião do Tugai", "Knowing When to Step Aside", "Saber Sair da Frente",
                "Cousin, you're always in front of me. In the Threshold Caves I want to walk beside you. Twenty creatures. You hit, I hold.",
                "Primo, você fica sempre na minha frente. Nas Grutas do Limiar eu quero ir do seu lado. Vinte bichos. Você bate, eu seguro.",
                "{0}/{1} in the {2}. Cousin, I said beside. Not in front.", "{0}/{1} nas {2}. Primo, eu disse do lado. Não na frente.",
                "You stepped aside and nobody got hurt. I'll say it: that's the best thing you've done.",
                "Você saiu da frente e ninguém se machucou. Eu falo: foi o melhor que você fez.",
                "Good thing you were beside me, Lavine. A tamarisk by the river protects without walking too.", "Ainda bem que você tava do lado, Lavine. A tamargueira da beira do rio também protege sem andar.");
            Grad(list, CharacterId.Joaquim, 3, "Master of the Roots", "Mestre das Raízes", "The Shield on the Wall", "O Escudo na Parede",
                "My stubborn grandson, the Shield of the Eternal Roots belonged to your great-grandfather. He said a good root is one that stays still, drinks the water and holds the whole ground. Thirty creatures on the Kaggen Range, then sit here, there's plov.",
                "Meu neto teimoso, o Escudo das Raízes Eternas era do seu bisavô. Ele dizia que raiz boa é a que fica quieta, bebe a água e segura a terra toda. Trinta bichos na Serra de Kaggen, e depois senta aqui que tem plov.",
                "{0}/{1} on the {2}. You get seconds of plov when you're back.", "{0}/{1} na {2}. Você repete o plov quando voltar.",
                "You came back in no hurry and with everyone in one piece. That's being a master, my boy.",
                "Você voltou sem pressa e com todo mundo inteiro. Isso é ser mestre, meu querido.",
                "Can I have seconds of plov, Grandma? ...Twice?", "Posso repetir o plov, Vó? ...Duas vezes?");

            // ---------------------------------------------------------------- Ícaro
            Grad(list, CharacterId.Icaro, 1, "Artificer", "Artífice", "Don't Tell Maithe", "Não Conta pra Maithe",
                "Pest, I saw the engine you're hiding. It makes everyone faster, right? Prove it in the Flaming Sands. Fifteen creatures. And I won't tell anyone. Maybe.",
                "Pestinha, eu vi o engenho que você tá escondendo. Ele deixa todo mundo mais rápido, né? Prova nas Areias Flamejantes. Quinze bichos. E eu não conto pra ninguém. Talvez.",
                "{0}/{1} in the {2}. Stay behind me while the thing heats up.", "{0}/{1} no {2}. Fica atrás de mim enquanto a coisa esquenta.",
                "It worked. Don't get cocky. ...I'm proud of you, little brother.", "Funcionou. Não fica se achando. ...Tô orgulhosa, maninho.",
                "Scientific conclusion: Sis said 'proud'. I'm writing that down.", "Conclusão científica: a mana disse \"orgulhosa\". Vou anotar.");
            Grad(list, CharacterId.Icaro, 2, "Master of Machines", "Mestre das Máquinas", "The Artificers' Warning", "O Aviso dos Artífices",
                "Cousin, you read that the Artificers left a warning. So build a machine that warns us too! I'll go first in the Threshold Caves, twenty creatures. Leave it to me!",
                "Primo, você leu que os Artífices deixaram um aviso. Então faz uma máquina que avisa a gente também! Eu vou na frente nas Grutas do Limiar, vinte bichos. Deixa comigo!",
                "{0}/{1} in the {2}. Cousin, the coil is making a noise, is that normal?", "{0}/{1} nas {2}. Primo, a bobina tá fazendo barulho, é normal?",
                "The coil spun and the creatures stopped! You really are a master of machines, cousin!",
                "A bobina girou e os bichos pararam! Você é mestre das máquinas mesmo, primo!",
                "Statistically it was a 70% chance. But it worked because you held them.", "Estatisticamente era 70% de chance. Mas deu certo porque você segurou.");
            Grad(list, CharacterId.Icaro, 3, "Heir of Kurdalægon", "Herdeiro de Kurdalægon", "Grandpa Jorge's Radio", "O Rádio do Vô Jorge",
                "My little inventor, you took the parts for the walkie-talkies from your Grandpa Jorge's radio. He used to tell of Kurdalægon, the sky smith who mended heroes. Up on the Kaggen Range the signal is better. Thirty creatures, and tell me everything over the radio.",
                "Meu neto inventor, foi do rádio do seu Vô Jorge que você tirou as peças dos radinhos. Ele contava de Kurdalægon, o ferreiro do céu que consertava heróis. Lá no alto da Serra de Kaggen o sinal pega melhor. Trinta bichos e me conta tudo pelo rádio.",
                "{0}/{1} on the {2}. Grandma is listening, dear. Over.", "{0}/{1} na {2}. A Vó tá ouvindo, meu querido. Câmbio.",
                "You made your grandfather's radio talk to the whole family again. That's good science.",
                "Você fez o rádio do seu avô falar com a família inteira de novo. Isso é ciência boa.",
                "Best invention of all, Grandma: the family. But don't tell Maithe.", "Melhor invenção de todas, Vó: a família. Mas não conta pra Maithe.");

            // ---------------------------------------------------------------- Lavine
            Grad(list, CharacterId.Lavine, 1, "Red Night Sorceress", "Feiticeira da Noite Rubra", "Butterflies in the Dark", "Friozinho na Barriga",
                "Lavine, the night in the Flaming Sands gives you butterflies, I know. Use the moon to look after us, not to hide. Fifteen creatures. I'll stay by your side.",
                "Lavine, a noite nas Areias Flamejantes dá friozinho na barriga, eu sei. Usa a lua pra cuidar da gente, não pra se esconder. Quinze bichos. Eu fico do seu lado.",
                "{0}/{1} in the {2}. Let's go, Lavine. I'm counting the cousins.", "{0}/{1} no {2}. Bora, Lavine. Eu tô contando os primos.",
                "You protected everyone with the night. Nobody here thinks you're little.", "Você protegeu todo mundo com a noite. Ninguém aqui acha que você é pequena.",
                "I'm not little. I'm the youngest. It's different. ...Thanks, cousin.", "Eu não sou pequena. Sou a mais nova. É diferente. ...Obrigada, prima.");
            Grad(list, CharacterId.Lavine, 2, "Keeper of the Soft Shadow", "Guardiã da Sombra Macia", "Shadows Can Be Soft", "A Sombra Também É Macia",
                "Lavine, your red ribbon ties the dark so it knows it's ours. Shall we show that in the Threshold Caves? Twenty creatures, together.",
                "Lavine, a sua fita vermelha amarra o escuro pra ele saber que é nosso. Vamos mostrar isso nas Grutas do Limiar? Vinte bichos, juntas.",
                "{0}/{1} in the {2}. Breathe, cousin. The shadow is yours.", "{0}/{1} nas {2}. Respira, prima. A sombra é sua.",
                "Your shadow held the creatures without hurting anyone in the family. How beautiful, cousin.",
                "A sua sombra segurou os bichos sem machucar ninguém da família. Que coisa bonita, prima.",
                "The dark doesn't scare me. It's soft when it's mine.", "O escuro não me assusta. Ele é macio quando é meu.");
            Grad(list, CharacterId.Lavine, 3, "Ferrywoman of the Moon", "Barqueira da Lua", "The Red Ribbon", "A Fita Vermelha",
                "My youngest, when you were born on the Red Night I tied this ribbon to your wrist so the dark would know you're ours. Your Grandpa Jorge said the Moon Boat gathers the light that comes free. Today you'll show the Kaggen Range. Thirty creatures, then come back to Grandma.",
                "Minha caçula, quando você nasceu na Noite Rubra eu amarrei essa fita no seu pulso pro escuro saber que você é nossa. O seu Vô Jorge dizia que a Barca da Lua junta a luz que se solta. Hoje você vai mostrar pra Serra de Kaggen. Trinta bichos e volta pra Vó.",
                "{0}/{1} on the {2}. The moon is full, my youngest.", "{0}/{1} na {2}. A lua tá cheia, minha caçula.",
                "You became the moon's ferrywoman, my youngest: you carry the light across the night for the whole family.", "Você virou barqueira da lua, minha caçula: leva a luz pela noite pra família inteira.",
                "Grandma... can I sleep on your shoulder tonight? Just tonight.", "Vó... posso dormir no seu ombro hoje? Só hoje.");

            // ---------------------------------------------------------------- Accessories
            Accessory(list, "pena_de_carcara", "Falcon Feather", "Pena de Falcão");
            Accessory(list, "lenco_da_ventania", "Embroidered Gale Scarf", "Lenço Bordado da Ventania");
            Accessory(list, "chapeu_de_couro", "Grandpa Jorge's Felt Cap", "Gorro de Feltro do Vô Jorge");
            Accessory(list, "fita_de_romaria", "Sun Ribbon", "Fita do Sol");
            Accessory(list, "xale_da_aurora", "Dawn Shawl", "Xale da Aurora");
            Accessory(list, "resplendor_de_candeia", "Lantern Glow", "Resplendor de Candeia");
            Accessory(list, "faixa_de_raiz", "Root Sash", "Faixa de Raiz");
            Accessory(list, "bornal_de_couro", "Leather Satchel", "Bornal de Couro");
            Accessory(list, "chapeu_de_aroeira", "Felt Cap with a Tamarisk Sprig", "Gorro de Feltro com Raminho de Tamargueira");
            Accessory(list, "oculos_de_solda", "Welding Goggles", "Óculos de Solda");
            Accessory(list, "bornal_de_ferramentas", "Tool Satchel", "Bornal de Ferramentas");
            Accessory(list, "antena_do_radio", "Radio Antenna", "Antena do Rádio");
            Accessory(list, "laco_rubro", "Red Bow", "Laço Rubro");
            Accessory(list, "lenco_de_sombra", "Shadow Scarf", "Lenço de Sombra");
            Accessory(list, "coroa_de_lua", "Moon Circlet", "Tiara de Lua");

            // ---------------------------------------------------------------- Graduation skills
            Skill(list, "skill_grad_maithe_olhar_do_carcara", "Falcon's Gaze", "Olhar do Falcão",
                "Maithe aims like a saker falcon and points out weak spots: she and nearby allies get +2% critical chance for 15 s.",
                "Maithe mira como o falcão-sacre e aponta os pontos fracos: ela e os aliados por perto ganham +2% de chance de crítico por 15 s.");
            Skill(list, "skill_grad_maithe_flecha_da_ventania", "Gale Arrow", "Flecha da Ventania",
                "An arrow pushed by the wind that wounds and slows the target by 25% for 3 s.",
                "Uma flecha empurrada pelo vento que fere e deixa o alvo 25% mais lento por 3 s.");
            Skill(list, "skill_grad_maithe_redemoinho_do_sertao", "Living Wind Whirlwind", "Redemoinho do Vento Vivo",
                "A whirlwind of Living Wind and arrows over an area: wounds and stuns for 0.8 s.",
                "Um redemoinho de Vento Vivo e flechas numa área: fere e atordoa por 0,8 s.");
            Skill(list, "skill_grad_maria_bencao_do_terreiro", "Lamp's Warmth", "Calor da Lâmpada",
                "Maria lights a lamp for the family: she and nearby allies recover 1 extra health per second for 12 s.",
                "Maria acende uma lâmpada pela família: ela e os aliados por perto recuperam 1 de vida a mais por segundo por 12 s.");
            Skill(list, "skill_grad_maria_candeia_que_acende", "Kindling Lantern", "Candeia que Acende",
                "A lantern of light that burns the creatures around and heals Maria for 40.",
                "Uma candeia de luz que queima os bichos em volta e cura a Maria em 40.");
            Skill(list, "skill_grad_maria_sol_do_meio_dia", "Noon Sun", "Sol do Meio-Dia",
                "The high noon sun over an area: wounds, stuns for 0.6 s and heals Maria for 30.",
                "O sol a pino numa área: fere, atordoa por 0,6 s e cura a Maria em 30.");
            Skill(list, "skill_grad_joaquim_raiz_que_segura", "Holding Roots", "Raiz que Segura",
                "Roots burst from the ground and hold the family: Joaquim and nearby allies take 4% less damage for 12 s.",
                "Raízes saem do chão e seguram a família: Joaquim e os aliados por perto recebem 4% menos dano por 12 s.");
            Skill(list, "skill_grad_joaquim_pancada_de_mandacaru", "Rapids Bash", "Pancada da Correnteza",
                "A shield bash that splashes Living Water around and stuns for 0.8 s.",
                "Uma pancada com o escudo que espirra Água Viva em volta e atordoa por 0,8 s.");
            Skill(list, "skill_grad_joaquim_cerca_de_aroeira", "Tamarisk Fence", "Cerca de Tamargueira",
                "A tamarisk fence from the riverbank sprouts around: wounds, stuns for 1 s and slows creatures by 30% for 4 s.",
                "Uma cerca de tamargueira da beira do rio brota em volta: fere, atordoa por 1 s e deixa os bichos 30% mais lentos por 4 s.");
            Skill(list, "skill_grad_icaro_engenhoca_de_reforco", "Booster Engine", "Engenho de Reforço",
                "Ícaro switches on an Engine of Light that speeds everyone up: he and nearby allies recharge skills 4% faster for 15 s.",
                "Ícaro liga um Engenho da Luz que acelera todo mundo: ele e os aliados por perto recarregam as skills 4% mais rápido por 15 s.");
            Skill(list, "skill_grad_icaro_bobina_de_cata_vento", "Pinwheel Coil", "Bobina de Cata-Vento",
                "A coil that spins with the wind and throws sparks of Living Fire over an area, stunning for 0.5 s.",
                "Uma bobina que gira com o vento e solta faíscas de Fogo Vivo numa área, atordoando por 0,5 s.");
            Skill(list, "skill_grad_icaro_radio_turbinado", "Turbo Radio", "Rádio Turbinado",
                "A deafening radio screech over an area: wounds and slows creatures by 30% for 4 s.",
                "Um chiado altíssimo do rádio numa área: fere e deixa os bichos 30% mais lentos por 4 s.");
            Skill(list, "skill_grad_lavine_lua_guardia", "Guardian Moon", "Lua Guardiã",
                "Lavine calls the moon: she and nearby allies recover mana 10% faster for 15 s.",
                "Lavine chama a lua: ela e os aliados por perto recuperam mana 10% mais rápido por 15 s.");
            Skill(list, "skill_grad_lavine_fita_vermelha", "Red Ribbon", "Fita Vermelha",
                "The red ribbon stretches and binds the target: wounds and slows it by 50% for 2 s.",
                "A fita vermelha estica e prende o alvo: fere e deixa ele 50% mais lento por 2 s.");
            Skill(list, "skill_grad_lavine_sombra_macia", "Soft Shadow", "Sombra Macia",
                "A soft shadow covers the area: wounds and stuns for 0.6 s, no fear of the dark.",
                "Uma sombra macia cobre a área: fere e atordoa por 0,6 s, sem medo do escuro.");

            // ---------------------------------------------------------------- Class combos
            Combo(list, "combo_class_maithe", "Trail, Rain and Gale", "Rastro, Chuva e Ventania",
                "Precise Arrow, Rain of Arrows and Gale Arrow in a row (up to 3 s between each): +4% damage for 5 s.",
                "Flecha Precisa, Chuva de Flechas e Flecha da Ventania em sequência (até 3 s entre cada): +4% de dano por 5 s.");
            Combo(list, "combo_class_maria", "Full Light", "Luz Completa",
                "Purification, Healing Light and Kindling Lantern in a row (up to 3 s between each): skills cost 10% less mana for 6 s.",
                "Purificação, Luz que Cura e Candeia que Acende em sequência (até 3 s entre cada): skills custam 10% menos mana por 6 s.");
            Combo(list, "combo_class_joaquim", "Wall of Water", "Muro d'Água",
                "Heroic Taunt, Tidal Quake and Rapids Bash in a row (up to 3 s between each): skills cost 10% less mana for 6 s.",
                "Provocação Heroica, Maremoto e Pancada da Correnteza em sequência (até 3 s entre cada): skills custam 10% menos mana por 6 s.");
            Combo(list, "combo_class_icaro", "Full Experiment", "Experimento Completo",
                "Ushabti Turret, Spark Grenade and Pinwheel Coil in a row (up to 3 s between each): +3% damage for 6 s.",
                "Torreta Ushabti, Granada de Faísca e Bobina de Cata-Vento em sequência (até 3 s entre cada): +3% de dano por 6 s.");
            Combo(list, "combo_class_lavine", "Red Night", "Noite Rubra",
                "Pure Air Orb, Chains of Air and Red Ribbon in a row (up to 3 s between each): +4% damage for 5 s.",
                "Orbe do Ar Sereno, Correntes de Ar e Fita Vermelha em sequência (até 3 s entre cada): +4% de dano por 5 s.");

            // ---------------------------------------------------------------- Master tree
            Node(list, "couro_curtido", "Tanned Leather", "Couro Curtido", "+1% max health per point (up to 5%).", "+1% de vida máxima por ponto (até 5%).");
            Node(list, "mao_firme", "Steady Hand", "Mão Firme", "+0.5% damage per point (up to 2.5%).", "+0,5% de dano por ponto (até 2,5%).");
            Node(list, "folego_de_vaqueiro", "Caravaneer's Breath", "Fôlego de Caravaneiro", "+2% mana recovery per point (up to 10%).", "+2% de recuperação de mana por ponto (até 10%).");
            Node(list, "pe_no_chao", "Feet on the Ground", "Pé no Chão", "0.6% less damage taken per point (up to 3%). Needs 3 points in Tanned Leather.", "0,6% menos dano recebido por ponto (até 3%). Pede 3 pontos em Couro Curtido.");
            Node(list, "olho_vivo", "Sharp Eye", "Olho Vivo", "+0.4% critical chance per point (up to 2%). Needs 3 points in Steady Hand.", "+0,4% de chance de crítico por ponto (até 2%). Pede 3 pontos em Mão Firme.");
            Node(list, "passo_ligeiro", "Quick Step", "Passo Ligeiro", "+1% move speed per point (up to 5%). Needs 3 points in Caravaneer's Breath.", "+1% de velocidade por ponto (até 5%). Pede 3 pontos em Fôlego de Caravaneiro.");

            return list;
        }

        private static void Grad(List<LocalizationEntry> list, CharacterId hero, int tier, string titleEn, string titlePt, string missionEn,
            string missionPt, string offerEn, string offerPt, string progressEn, string progressPt, string turnInEn, string turnInPt,
            string replyEn, string replyPt)
        {
            GraduationSpec spec = ClassEvolutionCatalog.GetGraduation(hero, tier);
            list.Add(new LocalizationEntry(spec.TitleKey, titleEn, titlePt));
            list.Add(new LocalizationEntry(spec.MissionTitleKey, missionEn, missionPt));
            list.Add(new LocalizationEntry(spec.OfferKey, offerEn, offerPt));
            list.Add(new LocalizationEntry(spec.ProgressKey, progressEn, progressPt));
            list.Add(new LocalizationEntry(spec.TurnInKey, turnInEn, turnInPt));
            list.Add(new LocalizationEntry(spec.ReplyKey, replyEn, replyPt));
        }

        private static void Accessory(List<LocalizationEntry> list, string id, string en, string pt) =>
            list.Add(new LocalizationEntry("class.accessory." + id, en, pt));

        private static void Skill(List<LocalizationEntry> list, string id, string nameEn, string namePt, string descEn, string descPt)
        {
            list.Add(new LocalizationEntry(ContentText.SkillNameKey(id), nameEn, namePt));
            list.Add(new LocalizationEntry(ContentText.SkillDescriptionKey(id), descEn, descPt));
        }

        private static void Combo(List<LocalizationEntry> list, string id, string nameEn, string namePt, string descEn, string descPt)
        {
            list.Add(new LocalizationEntry(ContentText.ComboNameKey(id), nameEn, namePt));
            list.Add(new LocalizationEntry(ContentText.ComboDescriptionKey(id), descEn, descPt));
        }

        private static void Node(List<LocalizationEntry> list, string id, string nameEn, string namePt, string descEn, string descPt)
        {
            MasterNodeSpec node = MasterLevelRules.GetNode(id);
            list.Add(new LocalizationEntry(node.NameKey, nameEn, namePt));
            list.Add(new LocalizationEntry(node.DescriptionKey, descEn, descPt));
        }

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Entries[0].Key)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        /// <summary>Localized text via <paramref name="manager"/> when it knows the key, else the pt-BR fallback, else the key.</summary>
        public static string Get(LocalizationManager manager, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            return GetFallback(key) ?? key;
        }

        /// <summary><see cref="Get"/> + <see cref="string.Format(string, object[])"/>, tolerant of malformed translations.</summary>
        public static string Format(LocalizationManager manager, string key, params object[] args)
        {
            string pattern = Get(manager, key);
            try
            {
                return string.Format(pattern, args);
            }
            catch (System.FormatException)
            {
                return string.Format(GetFallback(key) ?? key, args);
            }
        }

        public static string GetFallback(string key)
        {
            if (key == null)
            {
                return null;
            }

            if (_fallbackByKey == null)
            {
                var table = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    table[entry.Key] = entry.PtBR;
                }

                _fallbackByKey = table;
            }

            return _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }

        /// <summary>The title a cousin wears at <paramref name="tier"/> in the current language (tier 0 = base class).</summary>
        public static string TitleOf(LocalizationManager manager, CharacterId hero, int tier)
        {
            string key = ClassEvolutionCatalog.TitleKey(hero, tier);
            string text = Get(manager, key);
            return text == key && tier == 0 ? CharacterRegistry.Get(hero)?.ClassName ?? key : text;
        }
    }
}
