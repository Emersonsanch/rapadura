using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.ClassEvolution
{
    /// <summary>
    /// en + pt-BR text of the class evolution (FASE 16, Docs/ClassEvolution.md): graduation titles, the 25 story missions
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
        /// <summary>{0} master level kept from an old save, {1} level the master bar fills again.</summary>
        public const string MasterKept = "ui.class.master.kept";
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

        /// <summary>{0} mission title, {1} kills, {2} target, {3} lowest creature level, {4} place (G4/G5).</summary>
        public const string StatusInProgressLevel = "ui.class.status.in_progress_level";
        /// <summary>{0} passive name.</summary>
        public const string NewPassive = "ui.class.new_passive";

        public static string RegionKey(BiomeType region) => "ui.class.region." + region.ToString().ToLowerInvariant();

        /// <summary>Name of the place of a G4/G5 mission (a map of Docs/Mapas100.md): "ui.class.place.map_54".</summary>
        public static string MapPlaceKey(int mapId) => "ui.class.place.map_" + mapId;

        /// <summary>Where a mission happens: the campaign region (tiers 1-3) or the map of the story (G4/G5).</summary>
        public static string MissionPlaceKey(GraduationSpec spec) =>
            spec == null ? string.Empty
            : spec.MissionRegion != null ? RegionKey(spec.MissionRegion.Value)
            : MapPlaceKey(spec.MissionMapId);

        /// <summary>Heading of a master-tree line: "ui.class.master.branch.support".</summary>
        public static string BranchKey(MasterTreeBranch branch) => "ui.class.master.branch." + branch.ToString().ToLowerInvariant();

        public static readonly List<LocalizationEntry> Entries = Build();

        private static List<LocalizationEntry> Build()
        {
            var list = new List<LocalizationEntry>
            {
                new LocalizationEntry(Title, "Graduation", "Graduação"),
                new LocalizationEntry(CurrentTitle, "Title: {0}", "Título: {0}"),
                new LocalizationEntry(TierLine, "Graduation {0} of {1}", "Graduação {0} de {1}"),
                new LocalizationEntry(StatusAllDone, "Every graduation done. Now grow in the Master Level.", "Todas as graduações feitas. Agora é crescer no Nível de Mestre."),
                new LocalizationEntry(StatusInProgressLevel, "{0}: {1}/{2} creatures of level {3}+ ({4}).", "{0}: {1}/{2} bichos de nível {3}+ ({4})."),
                new LocalizationEntry(NewPassive, "New passive: {0}", "Passiva nova: {0}"),
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
                new LocalizationEntry(MasterLocked, "The Master Level opens at level {0} with the 5th graduation.", "O Nível de Mestre abre no nível {0} com a 5ª graduação."),
                new LocalizationEntry(MasterKept, "Master Level {0} (kept). The master bar fills again at level {1}.", "Nível de Mestre {0} (guardado). A barra de mestre volta a encher no nível {1}."),
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
                new LocalizationEntry(MapPlaceKey(31), "Jhilpur Delta", "Delta de Jhilpur"),
                new LocalizationEntry(MapPlaceKey(41), "Mingcheng Plain", "Planície de Mingcheng"),
                new LocalizationEntry(MapPlaceKey(54), "Farnkesh Peaks", "Picos de Farnkesh"),
                new LocalizationEntry(MapPlaceKey(59), "Frozen Sea", "Mar Gelado"),
                new LocalizationEntry(MapPlaceKey(65), "Hinokura Archipelago", "Arquipélago de Hinokura"),
                new LocalizationEntry(MapPlaceKey(91), "Sky Realms", "Reinos do Céu"),

                new LocalizationEntry(BranchKey(MasterTreeBranch.Core), "Roots (the first 30 points)", "Raiz (os 30 primeiros pontos)"),
                new LocalizationEntry(BranchKey(MasterTreeBranch.Support), "Support: for the family", "Apoio: pela família"),
                new LocalizationEntry(BranchKey(MasterTreeBranch.Utility), "Utility: for the road", "Utilidade: pela estrada"),
                new LocalizationEntry(BranchKey(MasterTreeBranch.Bond), "Bond: for the mount and for Ari", "Laço: pela montaria e pelo Ari"),

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
            Grad(list, CharacterId.Maithe, 4, "Breath of Farnkesh", "Sopro de Farnkesh", "The Stone Nests", "Os Ninhos de Pedra",
                "Cousin, the stone nests of Farnkesh are where the Living Wind is born. I say what I think: you're the only one of us who can hear it. Forty creatures of level 120 or more, and I'll keep the moon on your back.",
                "Prima, os ninhos de pedra de Farnkesh são onde o Vento Vivo nasce. Eu falo o que penso: você é a única de nós que escuta ele. Quarenta bichos de nível 120 pra cima, e eu seguro a lua nas suas costas.",
                "{0}/{1} on the {2}. The wind is calling your name, cousin.", "{0}/{1} nos {2}. O vento tá chamando o seu nome, prima.",
                "Your wind goes further now: the Falcon's Gaze reaches the whole family and the Gale Arrow pins them where they stand. I told you.",
                "Agora o seu vento vai mais longe: o Olhar do Falcão alcança a família toda e a Flecha da Ventania prende os bichos onde estão. Eu disse.",
                "You did, youngest. ...I counted you first today.", "Disse, caçula. ...Hoje eu contei você primeiro.");
            Grad(list, CharacterId.Maithe, 5, "Wind of the Column", "Vento da Coluna", "Counting Everyone", "Contar Todo Mundo",
                "My eldest, can you hear me? Grandma is on your grandfather's radio. At the Column Quay the wind blows upward and carries the freed light to the Column of Glory. Your Grandpa Jorge always said you'd be the one to count everyone up there. Fifty creatures of level 320 or more in the Sky Realms. Over.",
                "Minha mais velha, tá me ouvindo? A Vó tá no rádio do seu avô. No Cais da Coluna o vento sopra pra cima e leva a luz que se solta até a Coluna da Glória. O seu Vô Jorge sempre disse que seria você a contar todo mundo lá em cima. Cinquenta bichos de nível 320 pra cima nos Reinos do Céu. Câmbio.",
                "{0}/{1} in the {2}. Grandma is counting with you, dear. Over.", "{0}/{1} nos {2}. A Vó tá contando junto, minha querida. Câmbio.",
                "You count the cousins even in the sky. Now your wind blows upward too: a gust that carries the light home. Over.",
                "Você conta os primos até no céu. Agora o seu vento sopra pra cima também: um sopro que leva a luz pra casa. Câmbio.",
                "Five, Grandma. All five of us. ...And you on the radio makes six. Over.", "Cinco, Vó. Nós cinco. ...E a senhora no rádio, seis. Câmbio.");

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
            Grad(list, CharacterId.Maria, 4, "Lamp of Mingcheng", "Lâmpada de Mingcheng", "The Nine Rings", "Os Nove Anéis",
                "Cousin, I measured it: the nine rings of Mingcheng hold more light than every lamp in the Vila put together. Statistically your Kindling Lantern could hold the creatures, not only burn them. Forty creatures of level 120 or more. I'll take notes.",
                "Prima, eu medi: os nove anéis de Mingcheng guardam mais luz que todas as lâmpadas da Vila juntas. Estatisticamente a sua Candeia que Acende podia segurar os bichos, não só queimar. Quarenta bichos de nível 120 pra cima. Eu anoto tudo.",
                "{0}/{1} on the {2}. The data looks good, cousin. Keep shining.", "{0}/{1} na {2}. Os dados tão bons, prima. Continua brilhando.",
                "Conclusion: your lamp's warmth reaches further and the lantern slows them down. You're a scientific lamp, cousin.",
                "Conclusão: o calor da sua lâmpada vai mais longe e a candeia deixa os bichos lentos. Você é uma lâmpada científica, prima.",
                "And you're a dreamer who uses numbers, Ícaro. Thank you.", "E você é um sonhador que usa números, Ícaro. Obrigada.");
            Grad(list, CharacterId.Maria, 5, "Dawn of the Column", "Aurora da Coluna", "The First Light", "A Primeira Luz",
                "My little dreamer, it's Grandma on the radio. In the Sky Realms the dawn starts at the Column of Glory, and the light goes home through it. You've dreamt of this since your first day at the Temple. Fifty creatures of level 320 or more, and bring your cousins back with you. Over.",
                "Minha netinha sonhadora, é a Vó no rádio. Nos Reinos do Céu a aurora começa na Coluna da Glória, e é por ela que a luz volta pra casa. Você sonha com isso desde o primeiro dia de Templo. Cinquenta bichos de nível 320 pra cima, e traz os primos de volta com você. Câmbio.",
                "{0}/{1} in the {2}. The little medal is shining all the way down here, dear. Over.", "{0}/{1} nos {2}. A medalhinha tá brilhando até aqui embaixo, querida. Câmbio.",
                "You became the dawn, Maria: the light that wakes everyone else. Your Grandpa Jorge was right about the Column. Over.",
                "Você virou aurora, Maria: a luz que acorda os outros. O seu Vô Jorge tinha razão sobre a Coluna. Câmbio.",
                "I dreamt it, Grandma, and it was real. And everyone came along. Over.", "Eu sonhei, Vó, e era de verdade. E todo mundo veio junto. Câmbio.");

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
            Grad(list, CharacterId.Joaquim, 4, "Root of Jhilpur", "Raiz de Jhilpur", "The City in the Roots", "A Cidade nas Raízes",
                "Joaquim, in Jhilpur the roots hold a whole city above the water without taking a single step. You always run ahead. Forty creatures of level 120 or more out past the delta, and this time let the roots do the walking.",
                "Joaquim, em Jhilpur as raízes seguram uma cidade inteira em cima da água sem dar um passo. Você sempre corre na frente. Quarenta bichos de nível 120 pra cima além do delta, e dessa vez deixa as raízes andarem por você.",
                "{0}/{1} on the {2}. I'm counting you, Joaquim. Stay where I can see you.", "{0}/{1} no {2}. Tô contando você, Joaquim. Fica onde eu te vejo.",
                "Your roots hold the family from further away now, and your bash leaves the creatures stuck in the mud. You stood still and nobody fell.",
                "Agora as suas raízes seguram a família de mais longe, e a sua pancada deixa os bichos atolados. Você ficou parado e ninguém caiu.",
                "Standing still is harder than hitting, Maithe. ...But I liked it. A little.", "Ficar parado é mais difícil que bater, Maithe. ...Mas eu gostei. Um pouquinho.");
            Grad(list, CharacterId.Joaquim, 5, "River of the Column", "Rio da Coluna", "The River of Light", "O Rio de Luz",
                "My stubborn grandson, Grandma here, on the radio. Up in the Sky Realms there's a river of light that flows uphill, to the Column of Glory. A river doesn't run ahead: it carries everyone together. Fifty creatures of level 320 or more, then come home for plov. Over.",
                "Meu neto teimoso, é a Vó, no rádio. Lá nos Reinos do Céu tem um rio de luz que corre pra cima, até a Coluna da Glória. Rio não corre na frente: leva todo mundo junto. Cinquenta bichos de nível 320 pra cima, e depois vem pra casa que tem plov. Câmbio.",
                "{0}/{1} in the {2}. The plov is keeping warm, dear. Over.", "{0}/{1} nos {2}. O plov tá no fogo, meu querido. Câmbio.",
                "You became a river, my boy: you carry the whole family and nobody is left on the bank. Over.",
                "Você virou rio, meu querido: leva a família inteira e ninguém fica na margem. Câmbio.",
                "Can it be three plates of plov, Grandma? A river gets hungry. Over.", "Pode ser três pratos de plov, Vó? Rio dá fome. Câmbio.");

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
            Grad(list, CharacterId.Icaro, 4, "Forge of Hinokura", "Forja de Hinokura", "The Forge Isle", "A Ilha-Forja",
                "Ícaro, they say the forges of Hinokura never go out, because Living Fire is light that refuses to sleep. Build something there that warms the family, not only the machines. Forty creatures of level 120 or more. And eat something first.",
                "Ícaro, dizem que as forjas de Hinokura nunca apagam, porque o Fogo Vivo é luz que se recusa a dormir. Constrói lá alguma coisa que aqueça a família, não só as máquinas. Quarenta bichos de nível 120 pra cima. E come alguma coisa antes.",
                "{0}/{1} in the {2}. Did you eat, Ícaro? I'm asking for science.", "{0}/{1} no {2}. Você comeu, Ícaro? Tô perguntando pela ciência.",
                "Your engine reaches everyone around now, and the coil makes the creatures heavy as wet sand. Living Fire that looks after people: Grandpa Jorge would call that good science.",
                "Agora o seu engenho alcança todo mundo em volta, e a bobina deixa os bichos pesados feito areia molhada. Fogo Vivo que cuida de gente: o Vô Jorge ia chamar isso de ciência boa.",
                "Statistically you're the best cousin, Maria. Don't tell Maithe I said that.", "Estatisticamente você é a melhor prima, Maria. Não conta pra Maithe que eu falei.");
            Grad(list, CharacterId.Icaro, 5, "Engine of the Column", "Engenho da Coluna", "The Twelve-Jar Wheel", "A Roda das Doze Jarras",
                "My little inventor, it's Grandma, on your grandfather's radio. Up there turns the Wheel of the Twelve Jars, the one that lifts the light to the Column of Glory. Your Grandpa Jorge drew it on the back of this very radio, did you know? Fifty creatures of level 320 or more, and make your own wheel turn. Over.",
                "Meu neto inventor, é a Vó, no rádio do seu avô. Lá em cima gira a Roda das Doze Jarras, a que levanta a luz até a Coluna da Glória. O seu Vô Jorge desenhou ela atrás deste mesmo rádio, sabia? Cinquenta bichos de nível 320 pra cima, e faz a sua roda girar também. Câmbio.",
                "{0}/{1} in the {2}. Grandma is listening to the gears, dear. Over.", "{0}/{1} nos {2}. A Vó tá ouvindo as engrenagens, meu querido. Câmbio.",
                "You built a wheel that lifts light instead of weight. That's the engine of the Column, Ícaro. Over.",
                "Você construiu uma roda que levanta luz em vez de peso. Esse é o engenho da Coluna, Ícaro. Câmbio.",
                "Scientific conclusion, Grandma: the best part of the machine is who's inside it. Don't tell Maithe. Over.", "Conclusão científica, Vó: a melhor parte da máquina é quem tá dentro dela. Não conta pra Maithe. Câmbio.");

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
            Grad(list, CharacterId.Lavine, 4, "White Moon", "Lua Branca", "White Moon Harbor", "O Porto da Lua Branca",
                "Cousin, at the White Moon Harbor the moon shines on the ice all night long. Nobody there is afraid of the dark. I'll go first! ...Beside you. I'll go beside you. Forty creatures of level 120 or more.",
                "Prima, no Porto da Lua Branca a lua brilha no gelo a noite inteira. Lá ninguém tem medo do escuro. Eu vou na frente! ...Do lado. Eu vou do seu lado. Quarenta bichos de nível 120 pra cima.",
                "{0}/{1} on the {2}. Beside you, cousin. See? I learned.", "{0}/{1} no {2}. Do seu lado, prima. Viu? Eu aprendi.",
                "Your moon reaches the whole family now, and the ribbon holds them still for a heartbeat. You were the brave one this time, Lavine.",
                "Agora a sua lua alcança a família toda, e a fita segura os bichos parados por um instante. Dessa vez a corajosa foi você, Lavine.",
                "I'm the youngest, not the least brave. ...You did stay beside me, Joaquim.", "Eu sou a mais nova, não a menos corajosa. ...Você ficou do meu lado mesmo, Joaquim.");
            Grad(list, CharacterId.Lavine, 5, "Silence of the Column", "Silêncio da Coluna", "The Night That Falls Silent", "A Noite que se Cala",
                "My youngest, it's Grandma on the radio. In the Sky Realms the Moon Ship carries the light across the silence, and you've been its ferrywoman since the Kaggen Range. Fifty creatures of level 320 or more, and keep your ribbon on. Over.",
                "Minha caçula, é a Vó no rádio. Nos Reinos do Céu a Barca da Lua leva a luz pelo silêncio, e você é barqueira dela desde a Serra de Kaggen. Cinquenta bichos de nível 320 pra cima, e não tira a fita. Câmbio.",
                "{0}/{1} in the {2}. The moon is with you, my youngest. Over.", "{0}/{1} nos {2}. A lua tá com você, minha caçula. Câmbio.",
                "You made the dark go quiet without making it afraid. That's the silence of the Column, Lavine. Over.",
                "Você fez o escuro ficar quieto sem deixar ele com medo. Esse é o silêncio da Coluna, Lavine. Câmbio.",
                "It's soft up here, Grandma. Like your shoulder. Over.", "Aqui em cima é macio, Vó. Feito o seu ombro. Câmbio.");

            // ---------------------------------------------------------------- Accessories
            Accessory(list, "pena_de_carcara", "Falcon Feather", "Pena de Falcão");
            Accessory(list, "lenco_da_ventania", "Embroidered Gale Scarf", "Lenço Bordado da Ventania");
            Accessory(list, "chapeu_de_couro", "Grandpa Jorge's Felt Cap", "Gorro de Feltro do Vô Jorge");
            Accessory(list, "fita_de_romaria", "Sun Ribbon", "Fita do Sol");
            Accessory(list, "xale_da_aurora", "Dawn Shawl", "Xale da Aurora");
            Accessory(list, "resplendor_de_candeia", "Lantern Glow", "Resplendor de Candeia");
            Accessory(list, "faixa_de_raiz", "Root Sash", "Faixa de Raiz");
            Accessory(list, "bornal_de_couro", "Leather Satchel", "Alforje de Couro");
            Accessory(list, "chapeu_de_aroeira", "Felt Cap with a Tamarisk Sprig", "Gorro de Feltro com Raminho de Tamargueira");
            Accessory(list, "oculos_de_solda", "Welding Goggles", "Óculos de Solda");
            Accessory(list, "bornal_de_ferramentas", "Tool Satchel", "Alforje de Ferramentas");
            Accessory(list, "antena_do_radio", "Radio Antenna", "Antena do Rádio");
            Accessory(list, "laco_rubro", "Red Bow", "Laço Rubro");
            Accessory(list, "lenco_de_sombra", "Shadow Scarf", "Lenço de Sombra");
            Accessory(list, "coroa_de_lua", "Moon Circlet", "Tiara de Lua");
            Accessory(list, "manto_de_penas_de_farnkesh", "Farnkesh Feather Mantle", "Manto de Penas de Farnkesh");
            Accessory(list, "manto_dos_nove_aneis", "Mantle of the Nine Rings", "Manto dos Nove Anéis");
            Accessory(list, "manto_de_raizes_de_jhilpur", "Jhilpur Root Mantle", "Manto de Raízes de Jhilpur");
            Accessory(list, "avental_de_forja_de_hinokura", "Hinokura Forge Apron", "Avental de Forja de Hinokura");
            Accessory(list, "manto_da_lua_branca", "White Moon Mantle", "Manto da Lua Branca");
            Accessory(list, "anel_do_vento_da_coluna", "Ring of the Column Wind", "Anel do Vento da Coluna");
            Accessory(list, "anel_da_aurora_da_coluna", "Ring of the Column Dawn", "Anel da Aurora da Coluna");
            Accessory(list, "anel_do_rio_da_coluna", "Ring of the Column River", "Anel do Rio da Coluna");
            Accessory(list, "anel_do_engenho_da_coluna", "Ring of the Column Engine", "Anel do Engenho da Coluna");
            Accessory(list, "anel_do_silencio_da_coluna", "Ring of the Column Silence", "Anel do Silêncio da Coluna");

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

            Skill(list, "skill_grad_maithe_sopro_da_coluna", "Column Gust", "Sopro da Coluna",
                "A gust of Living Wind that blows upward over an area: wounds and stuns for 1 s.",
                "Um sopro de Vento Vivo que sobe numa área: fere e atordoa por 1 s.");
            Skill(list, "skill_grad_maria_primeira_luz", "First Light", "Primeira Luz",
                "The first light of the dawn over an area: wounds, stuns for 0.8 s and heals Maria for 40.",
                "A primeira luz da aurora numa área: fere, atordoa por 0,8 s e cura a Maria em 40.");
            Skill(list, "skill_grad_joaquim_rio_de_luz", "River of Light", "Rio de Luz",
                "A river of light that flows uphill around Joaquim: wounds, stuns for 1.3 s and slows creatures by 30% for 5 s.",
                "Um rio de luz que corre pra cima em volta do Joaquim: fere, atordoa por 1,3 s e deixa os bichos 30% mais lentos por 5 s.");
            Skill(list, "skill_grad_icaro_roda_das_doze_jarras", "Twelve-Jar Wheel", "Roda das Doze Jarras",
                "A wheel of twelve jars of Living Fire spins over an area: wounds and slows creatures by 30% for 5 s.",
                "Uma roda de doze jarras de Fogo Vivo gira numa área: fere e deixa os bichos 30% mais lentos por 5 s.");
            Skill(list, "skill_grad_lavine_noite_que_se_cala", "Night Falls Silent", "Noite que se Cala",
                "The night goes quiet over an area, without fear: wounds and stuns for 0.8 s.",
                "A noite se cala numa área, sem medo: fere e atordoa por 0,8 s.");

            // ---------------------------------------------------------------- G4 passives
            Passive(list, "passive_grad_maithe_vento_mais_longe", "Wind Reaches Further", "Vento Mais Longe",
                "Falcon's Gaze reaches allies up to 14 m (was 10 m) and Gale Arrow also stuns for 0.4 s.",
                "O Olhar do Falcão alcança aliados até 14 m (eram 10 m) e a Flecha da Ventania também atordoa por 0,4 s.");
            Passive(list, "passive_grad_maria_lampada_dos_nove_aneis", "Lamp of the Nine Rings", "Lâmpada dos Nove Anéis",
                "Lamp's Warmth reaches allies up to 14 m (was 10 m) and Kindling Lantern slows creatures by 30% for 3 s.",
                "O Calor da Lâmpada alcança aliados até 14 m (eram 10 m) e a Candeia que Acende deixa os bichos 30% mais lentos por 3 s.");
            Passive(list, "passive_grad_joaquim_raizes_de_jhilpur", "Roots of Jhilpur", "Raízes de Jhilpur",
                "Holding Roots reaches allies up to 14 m (was 10 m) and Rapids Bash slows creatures by 30% for 3 s.",
                "A Raiz que Segura alcança aliados até 14 m (eram 10 m) e a Pancada da Correnteza deixa os bichos 30% mais lentos por 3 s.");
            Passive(list, "passive_grad_icaro_fole_de_hinokura", "Hinokura Bellows", "Fole de Hinokura",
                "Booster Engine reaches allies up to 14 m (was 10 m) and Pinwheel Coil slows creatures by 25% for 3 s.",
                "O Engenho de Reforço alcança aliados até 14 m (eram 10 m) e a Bobina de Cata-Vento deixa os bichos 25% mais lentos por 3 s.");
            Passive(list, "passive_grad_lavine_luar_do_porto", "Harbor Moonlight", "Luar do Porto",
                "Guardian Moon reaches allies up to 14 m (was 10 m) and Red Ribbon also stuns for 0.4 s.",
                "A Lua Guardiã alcança aliados até 14 m (eram 10 m) e a Fita Vermelha também atordoa por 0,4 s.");

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
            Node(list, "luz_partilhada", "Shared Light", "Luz Partilhada", "Your graduation group buff reaches allies 0.4 m further per point (up to 2 m).", "O buff de grupo da graduação alcança aliados 0,4 m mais longe por ponto (até 2 m).");
            Node(list, "hino_do_chamado", "Hymn of the Call", "Hino do Chamado", "The group buff lasts 4% longer on allies per point (up to 20%). Needs 3 points in Shared Light.", "O buff de grupo dura 4% mais nos aliados por ponto (até 20%). Pede 3 pontos em Luz Partilhada.");
            Node(list, "agua_viva", "Living Water", "Água Viva", "+0.2 health per second per point (up to 1).", "+0,2 de vida por segundo por ponto (até 1).");
            Node(list, "manto_de_luz", "Mantle of Light", "Manto de Luz", "0.4% less damage taken per point (up to 2%). Needs 3 points in Living Water.", "0,4% menos dano recebido por ponto (até 2%). Pede 3 pontos em Água Viva.");
            Node(list, "abraco_da_familia", "Family Embrace", "Abraço da Família", "+0.5% max health per point (up to 2.5%). Needs 3 points in Mantle of Light.", "+0,5% de vida máxima por ponto (até 2,5%). Pede 3 pontos em Manto de Luz.");
            Node(list, "passo_de_peregrino", "Pilgrim's Stride", "Passo de Peregrino", "+2% max stamina per point (up to 10%).", "+2% de fôlego máximo por ponto (até 10%).");
            Node(list, "folego_do_ouvinte", "Hearer's Breath", "Fôlego do Ouvinte", "+2% stamina recovery per point (up to 10%). Needs 3 points in Pilgrim's Stride.", "+2% de recuperação de fôlego por ponto (até 10%). Pede 3 pontos em Passo de Peregrino.");
            Node(list, "taca_do_bema", "Cup of the Bema", "Taça do Bema", "+1% max mana per point (up to 5%).", "+1% de mana máxima por ponto (até 5%).");
            Node(list, "memoria_do_ouvinte", "Hearer's Memory", "Memória do Ouvinte", "+2% master XP per point (up to 10%). Needs 3 points in Cup of the Bema.", "+2% de XP de mestre por ponto (até 10%). Pede 3 pontos em Taça do Bema.");
            Node(list, "fonte_serena", "Serene Spring", "Fonte Serena", "+1% mana recovery per point (up to 5%). Needs 3 points in Hearer's Memory.", "+1% de recuperação de mana por ponto (até 5%). Pede 3 pontos em Memória do Ouvinte.");
            Node(list, "laco_da_sela", "Saddle Knot", "Laço da Sela", "+2% mount stamina per point (up to 10%).", "+2% de fôlego da montaria por ponto (até 10%).");
            Node(list, "redea_de_luz", "Rein of Light", "Rédea de Luz", "+3% mount combat XP per point (up to 15%). Needs 3 points in Saddle Knot.", "+3% de XP de combate da montaria por ponto (até 15%). Pede 3 pontos em Laço da Sela.");
            Node(list, "faro_do_ari", "Ari's Nose", "Faro do Ari", "Ari spots creatures about to attack 6% further per point (up to 30%).", "O Ari percebe bichos prestes a atacar 6% mais longe por ponto (até 30%).");
            Node(list, "ari_ligeiro", "Quick Ari", "Ari Ligeiro", "Ari fetches drops 6% further per point (up to 30%). Needs 3 points in Ari's Nose.", "O Ari busca o que cai 6% mais longe por ponto (até 30%). Pede 3 pontos em Faro do Ari.");

            return list;
        }

        private static void Passive(List<LocalizationEntry> list, string id, string nameEn, string namePt, string descEn, string descPt)
        {
            GraduationUpgradeSpec spec = ClassEvolutionCatalog.GetUpgrade(id);
            list.Add(new LocalizationEntry(spec.NameKey, nameEn, namePt));
            list.Add(new LocalizationEntry(spec.DescriptionKey, descEn, descPt));
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
