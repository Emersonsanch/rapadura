using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// Localization keys (en + pt-BR) of "Caderno da Vó Zefa" (<see cref="BestiaryController"/>): the
    /// window chrome (<c>ui.bestiary.*</c>), one "Dicas da Vó" line per <see cref="BestiaryTipKind"/>, the drop
    /// frequency labels, and per enemy page a lore paragraph (<see cref="BestiaryCatalog.LoreKey"/>) plus a
    /// one-liner from each of the five cousins (<see cref="BestiaryCatalog.CommentKey"/>). Enemy names come
    /// from <c>ContentText</c> (<c>enemy.&lt;asset&gt;.name</c>), region names from the core <c>biome.*</c> keys.
    ///
    /// Lore is Vó Zefa's own voice (sertão/caipira, gentle — kids read it); the comments follow the
    /// roster (Docs/GDD.md): Maithe the eldest and watchful archer, Maria the caring healer who worries about
    /// her brother Joaquim, Joaquim the stubborn shield of the family, Ícaro the inventor who wants to impress
    /// his big sister Maithe, Lavine the youngest cousin who wants to be taken seriously.
    /// Same self-contained pattern as <c>BossTexts</c>/<c>StoryTexts</c>: merged into the live
    /// <see cref="LocalizationManager"/> on first use (keys it already has win), pt-BR fallback otherwise.
    /// </summary>
    public static class BestiaryTexts
    {
        public const string Title = "ui.bestiary.title";
        public const string Subtitle = "ui.bestiary.subtitle";
        /// <summary>{0} unlocked pages, {1} total.</summary>
        public const string Progress = "ui.bestiary.progress";
        public const string Complete = "ui.bestiary.complete";
        public const string Close = "ui.bestiary.close";
        public const string LockedName = "ui.bestiary.locked_name";
        public const string LockedHint = "ui.bestiary.locked_hint";
        public const string LockedBossHint = "ui.bestiary.locked_boss_hint";
        public const string BossBadge = "ui.bestiary.boss_badge";
        public const string UnknownRegion = "ui.bestiary.unknown_region";
        /// <summary>{0} region name.</summary>
        public const string RegionLine = "ui.bestiary.region";
        /// <summary>{0} defeats.</summary>
        public const string Kills = "ui.bestiary.kills";
        /// <summary>{0} health, {1} damage per hit.</summary>
        public const string Stats = "ui.bestiary.stats";
        public const string TipsHeader = "ui.bestiary.tips_header";
        public const string DropsHeader = "ui.bestiary.drops_header";
        public const string DropsNone = "ui.bestiary.drops_none";
        /// <summary>{0} item name, {1} frequency label.</summary>
        public const string DropLine = "ui.bestiary.drop_line";
        /// <summary>{0} item name, {1} frequency label: a procedurally rolled piece of equipment.</summary>
        public const string DropEquipmentLine = "ui.bestiary.drop_equipment_line";
        /// <summary>{0} item name: the boss's own bonus piece.</summary>
        public const string DropBossBonusLine = "ui.bestiary.drop_boss_bonus_line";
        /// <summary>{0} hero name.</summary>
        public const string CommentHeader = "ui.bestiary.comment_header";
        public const string NoHeroComment = "ui.bestiary.no_hero_comment";

        public const string FrequencyRare = "ui.bestiary.frequency.rare";
        public const string FrequencySometimes = "ui.bestiary.frequency.sometimes";
        public const string FrequencyCommon = "ui.bestiary.frequency.common";
        public const string FrequencyGuaranteed = "ui.bestiary.frequency.guaranteed";

        public const string TipPackRunner = "ui.bestiary.tip.pack_runner";
        public const string TipSkitterer = "ui.bestiary.tip.skitterer";
        public const string TipFlyer = "ui.bestiary.tip.flyer";
        public const string TipHeavy = "ui.bestiary.tip.heavy";
        /// <summary>{0} seconds of telegraph ("0,4").</summary>
        public const string TipTelegraph = "ui.bestiary.tip.telegraph";
        public const string TipRapidAttacks = "ui.bestiary.tip.rapid_attacks";
        /// <summary>{0} damage per hit.</summary>
        public const string TipHeavyHitter = "ui.bestiary.tip.heavy_hitter";
        /// <summary>{0} health percentage at which it runs ("20").</summary>
        public const string TipFlees = "ui.bestiary.tip.flees";
        /// <summary>{0} percentage of the usual stun time ("50").</summary>
        public const string TipStunResistant = "ui.bestiary.tip.stun_resistant";

        /// <summary>The five cousins, in birth order (Maithe > Maria > Joaquim > Ícaro > Lavine).</summary>
        public static readonly CharacterId[] Cousins = { CharacterId.Maithe, CharacterId.Maria, CharacterId.Joaquim, CharacterId.Icaro, CharacterId.Lavine };

        public static string TipKey(BestiaryTipKind kind)
        {
            switch (kind)
            {
                case BestiaryTipKind.PackRunner: return TipPackRunner;
                case BestiaryTipKind.Skitterer: return TipSkitterer;
                case BestiaryTipKind.Flyer: return TipFlyer;
                case BestiaryTipKind.Heavy: return TipHeavy;
                case BestiaryTipKind.Telegraph: return TipTelegraph;
                case BestiaryTipKind.RapidAttacks: return TipRapidAttacks;
                case BestiaryTipKind.HeavyHitter: return TipHeavyHitter;
                case BestiaryTipKind.Flees: return TipFlees;
                case BestiaryTipKind.StunResistant: return TipStunResistant;
                default: return null;
            }
        }

        public static string FrequencyKey(BestiaryDropFrequency frequency)
        {
            switch (frequency)
            {
                case BestiaryDropFrequency.Guaranteed: return FrequencyGuaranteed;
                case BestiaryDropFrequency.Common: return FrequencyCommon;
                case BestiaryDropFrequency.Sometimes: return FrequencySometimes;
                default: return FrequencyRare;
            }
        }

        public static readonly List<LocalizationEntry> Entries = BuildEntries();

        private static List<LocalizationEntry> BuildEntries()
        {
            var entries = new List<LocalizationEntry>
            {
                new LocalizationEntry(Title, "Grandma Zefa's Notebook", "Caderno da Vó Zefa"),
                new LocalizationEntry(Subtitle, "Every creature you beat, Grandma writes down here.", "Cada bicho que vocês vencem, a Vó anota aqui."),
                new LocalizationEntry(Progress, "{0}/{1} pages written", "{0}/{1} páginas escritas"),
                new LocalizationEntry(Complete, "Notebook complete! Grandma Zefa is proud as can be.", "Caderno completo! A Vó Zefa está toda orgulhosa."),
                new LocalizationEntry(Close, "Close", "Fechar"),
                new LocalizationEntry(LockedName, "???", "???"),
                new LocalizationEntry(LockedHint, "Not faced yet. Beat one and Grandma will write it down.", "Ainda não enfrentado. Vença um e a Vó anota."),
                new LocalizationEntry(LockedBossHint, "The boss of a region. Grandma only writes it down after you win.", "O chefe de uma região. A Vó só anota depois que vocês vencerem."),
                new LocalizationEntry(BossBadge, "Boss", "Chefe"),
                new LocalizationEntry(UnknownRegion, "Unknown lands", "Terras desconhecidas"),
                new LocalizationEntry(RegionLine, "Lives in: {0}", "Vive em: {0}"),
                new LocalizationEntry(Kills, "Defeated: {0}", "Vencidos: {0}"),
                new LocalizationEntry(Stats, "Health {0} · Hit {1}", "Vida {0} · Golpe {1}"),
                new LocalizationEntry(TipsHeader, "Grandma's tips", "Dicas da Vó"),
                new LocalizationEntry(DropsHeader, "What it drops", "O que deixa cair"),
                new LocalizationEntry(DropsNone, "Nothing useful.", "Nada que preste."),
                new LocalizationEntry(DropLine, "{0} ({1})", "{0} ({1})"),
                new LocalizationEntry(DropEquipmentLine, "{0}, random gear ({1})", "{0}, equipamento sorteado ({1})"),
                new LocalizationEntry(DropBossBonusLine, "{0}, the boss's treasure (always)", "{0}, o tesouro do chefe (sempre)"),
                new LocalizationEntry(CommentHeader, "{0} says", "{0} diz"),
                new LocalizationEntry(NoHeroComment, "Pick a cousin to hear what they think.", "Escolha um primo para ouvir o que ele acha."),

                new LocalizationEntry(FrequencyRare, "rare", "raro"),
                new LocalizationEntry(FrequencySometimes, "sometimes", "às vezes"),
                new LocalizationEntry(FrequencyCommon, "often", "quase sempre"),
                new LocalizationEntry(FrequencyGuaranteed, "always", "sempre"),

                new LocalizationEntry(TipPackRunner, "Runs straight at you and lunges: step aside at the last moment.", "Vem correndo e dá o bote: pule de lado na última hora."),
                new LocalizationEntry(TipSkitterer, "Low and quick on its many legs: keep moving, don't let it circle you.", "Baixinho e ligeiro nas muitas pernas: não pare, não deixe ele rodear você."),
                new LocalizationEntry(TipFlyer, "Flies and swoops: hit it right after it dives.", "Voa e dá rasante: acerte logo depois do mergulho."),
                new LocalizationEntry(TipHeavy, "Big and slow, but strong: hit it after its slam, then back away.", "Grandão e lento, mas forte: bata depois da pancada e saia de perto."),
                new LocalizationEntry(TipTelegraph, "Gets ready for {0} s before each hit: that's your cue to dodge.", "Se prepara {0} s antes de cada golpe: é a sua deixa para esquivar."),
                new LocalizationEntry(TipRapidAttacks, "Attacks back to back: don't stand in front of it for long.", "Ataca sem parar: não fique muito tempo na frente dele."),
                new LocalizationEntry(TipHeavyHitter, "Hits hard ({0} per hit): save your dodge for the big one.", "Bate forte ({0} por golpe): guarde a esquiva para a pancada grande."),
                new LocalizationEntry(TipFlees, "Runs away below {0}% health: finish it before it gets away.", "Foge com menos de {0}% de vida: termine o serviço antes que escape."),
                new LocalizationEntry(TipStunResistant, "Stubborn boss: stuns last only {0}% of the usual time.", "Chefe teimoso: atordoamento dura só {0}% do normal."),
            };

            // -- Floresta --
            Page(entries, "enemy_forest_wolf",
                "Scrub wolves walk in gangs and howl at the moon just to show off. Alone they're more fuss than danger. Grandpa used to say a wolf respects whoever doesn't run.",
                "Lobo da mata anda em bando e uiva pra lua só pra se mostrar. Sozinho é mais zoada que perigo. Seu avô dizia que lobo respeita quem não corre.",
                ("When they spread out, they're about to jump. I can see it from here.", "Quando eles se espalham é porque vão pular. Daqui eu vejo."),
                ("Poor thing, it's just hungry... but nobody bites my brother!", "Coitado, deve ser fome... mas no meu irmão ninguém morde!"),
                ("Come on, then! The shield is right here.", "Pode vir! O escudo tá aqui."),
                ("They always lunge after the same pause. Easy to predict.", "Eles sempre pulam depois da mesma pausa. Dá pra prever."),
                ("I'm not scared of wolves. Only a tiny bit.", "Eu não tenho medo de lobo. Só um pouquinho."));

            Page(entries, "enemy_forest_spider",
                "These spiders weave webs between the trees as fine as lace from a fair. They're quick and sneaky, but a sturdy stick and a steady heart scare them right off.",
                "Essas aranhas tecem teia entre as árvores mais fina que renda de feira. São ligeiras e sonsas, mas um pau firme e um coração calmo espantam elas de vereda.",
                ("Watch the webs between the trunks. That's where they wait.", "Olhem as teias entre os troncos. É ali que elas esperam."),
                ("Eight legs and none of them stays still. Careful, Joaquim!", "Oito pernas e nenhuma fica parada. Cuidado, Joaquim!"),
                ("It runs fast but it's tiny. I'm not afraid of it.", "Corre rápido, mas é pequenininha. Não tenho medo dela não."),
                ("That silk could make a great cable for my drone...", "Essa seda dava um ótimo cabo pro meu drone..."),
                ("My shadows are faster than its legs. I timed it.", "Minhas sombras são mais rápidas que as pernas dela. Eu contei."));

            Page(entries, "enemy_forest_alpha_wolf",
                "The Alpha Wolf is the old leader of the pack, with a scar on his snout and pride in his chest. When he howls, the whole forest goes quiet. Beat him and the wolves calm down.",
                "O Lobo Alfa é o chefe velho do bando, com cicatriz no focinho e orgulho no peito. Quando ele uiva, a mata toda fica quieta. Vencendo ele, os lobos sossegam.",
                ("I tracked him three days. He knew I was there.", "Segui o rastro dele três dias. Ele sabia que eu estava lá."),
                ("He protects his pack like we protect each other. I almost feel sorry.", "Ele protege o bando como a gente se protege. Quase dá pena."),
                ("He's big. My shield is bigger.", "Ele é grande. Meu escudo é maior."),
                ("Half the stun on him! I'll need a stronger grenade.", "Metade do atordoamento nele! Vou precisar de uma granada mais forte."),
                ("I helped beat the Alpha. Write that down, Grandma! Write it big!", "Eu ajudei a vencer o Alfa. Anota aí, Vó! Bem grande!"));

            // -- Deserto --
            Page(entries, "enemy_desert_scorpion",
                "The dune scorpion hides under the hot sand with only its tail sticking out, like a sprout of trouble. Shake your boots before putting them on, my dears.",
                "O escorpião das dunas se esconde na areia quente com só o rabo de fora, feito broto de encrenca. Sacudam a botina antes de calçar, meus queridos.",
                ("Follow the tail. When it rises, it's coming.", "Fiquem de olho no rabo. Quando ele levanta, lá vem."),
                ("If it stings someone, come to me right away. No being brave about it.", "Se ele picar alguém, venham direto pra mim. Nada de bancar o valente."),
                ("The armor is tough, but so am I.", "A casca é dura, mas eu também sou."),
                ("Its shell is like natural armor plating. I want a sample.", "A casca dele é tipo uma blindagem natural. Quero uma amostra."),
                ("It's ugly. I'm saying it, nobody else wants to.", "Ele é feio. Eu falo, ninguém mais quer falar."));

            Page(entries, "enemy_desert_ruin_guardian",
                "Nobody remembers who built the Ruin Guardian. It stood watch over the old stones for so long that it forgot it could rest. Grandma thinks it just wanted someone to say: you can sleep now.",
                "Ninguém lembra quem fez o Guardião da Ruína. Ele vigiou as pedras velhas por tanto tempo que esqueceu que podia descansar. A Vó acha que ele só queria alguém pra dizer: pode dormir agora.",
                ("It turns slowly. Go around it while it turns.", "Ele vira devagar. Deem a volta enquanto ele vira."),
                ("Such an old, tired thing... I hope it rests now.", "Uma coisa tão velha e cansada... tomara que agora descanse."),
                ("It hit my shield so hard my teeth rattled. Still standing!", "Bateu no meu escudo tão forte que meus dentes tremeram. Continuo de pé!"),
                ("The gears inside are older than the village! I deciphered part of the inscriptions.", "As engrenagens dele são mais velhas que a vila! Decifrei parte das inscrições."),
                ("It was guarding the ruins alone. I know how that feels.", "Ele guardava as ruínas sozinho. Eu sei como é isso."));

            // -- Montanha --
            Page(entries, "enemy_mountain_stone_golem",
                "Stone golems are rocks that got tired of standing still. They walk heavy, boom, boom, and shake the pebbles of the trail. Strong as an ox, but slow as Sunday afternoon.",
                "Golem de pedra é rocha que cansou de ficar parada. Anda pesado, tum, tum, e faz tremer o cascalho da trilha. Forte que nem boi, mas lento que nem tarde de domingo.",
                ("Wait for the slam, then shoot. It takes forever to stand up straight.", "Esperem a pancada e atirem. Ele demora uma vida pra se endireitar."),
                ("Joaquim, don't take its punch on purpose to prove anything!", "Joaquim, não aguenta o soco dele de propósito pra provar nada!"),
                ("Rock against roots. The roots win.", "Pedra contra raiz. A raiz ganha."),
                ("What makes it move? There must be a power core in there.", "O que faz ele mexer? Deve ter um núcleo de energia aí dentro."),
                ("I'll call it Pebble. Even if it's huge.", "Vou chamar ele de Pedrinha. Mesmo sendo enorme."));

            Page(entries, "enemy_mountain_warlord",
                "The Mountain Warlord climbed so high he thought he owned the sky. He shouts orders at the wind and the wind doesn't listen. Up there, it's the humble who keep their footing.",
                "O Senhor da Guerra da Montanha subiu tão alto que achou que era dono do céu. Grita ordem pro vento e o vento nem escuta. Lá em cima, quem tem humildade é que não escorrega.",
                ("The wind up there is mine. He'll learn it.", "O vento lá de cima é meu. Ele vai aprender."),
                ("He shouts a lot. People who shout a lot are usually afraid.", "Ele grita demais. Quem grita muito geralmente tá com medo."),
                ("He said nobody could hold him. I held him.", "Ele disse que ninguém segurava ele. Eu segurei."),
                ("His armor has a weak spot at the joints. I drew it in my notebook too.", "A armadura dele tem ponto fraco nas juntas. Desenhei no meu caderno também."),
                ("He laughed at me. He's not laughing now.", "Ele riu de mim. Agora não ri mais."));

            // -- Cavernas --
            Page(entries, "enemy_cave_bat_swarm",
                "Cave bats never come alone: they come in a whirlwind, squeaking and flapping in your ears. They hate light. A lantern and a little courage go a long way.",
                "Morcego de caverna nunca vem sozinho: vem num redemoinho, chiando e batendo asa no ouvido. Eles detestam claridade. Uma lamparina e um tiquinho de coragem resolvem muito.",
                ("Too many to aim at one. I'll rain arrows on all of them.", "São muitos pra mirar num só. Vou chover flecha em todos."),
                ("My light bothers them. Stay close to me in the dark.", "A minha luz incomoda eles. Fiquem perto de mim no escuro."),
                ("They keep pulling my hair! That's not fair!", "Eles ficam puxando meu cabelo! Assim não vale!"),
                ("They find their way by echo. If I make noise, they get lost.", "Eles se guiam pelo eco. Se eu fizer barulho, eles se perdem."),
                ("They like the dark. So do I. We are not friends.", "Eles gostam do escuro. Eu também. A gente não é amigo."));

            Page(entries, "enemy_cave_dweller",
                "Deep in the caves lives the Ancient Dweller, who drank the light of the crystals to keep the cold away. It isn't wicked, it's lonely and greedy. Beating it gives the light back to the village.",
                "Lá no fundo das cavernas mora o Habitante Ancestral, que bebeu a luz dos cristais pra espantar o frio. Não é malvado, é sozinho e guloso. Vencendo ele, a luz volta pra vila.",
                ("We came all this way together. Together we go home.", "A gente veio até aqui junto. Junto a gente volta pra casa."),
                ("The crystals are shining again. I can feel it in my hands.", "Os cristais estão brilhando de novo. Eu sinto nas mãos."),
                ("Nobody fell. Not while I was standing in front.", "Ninguém caiu. Não enquanto eu estava na frente."),
                ("Maithe, did you see my drone hit it right in the crystal? Did you see?", "Maithe, viu o meu drone acertar bem no cristal? Viu?"),
                ("I used the dark to bring back the light. Funny, right?", "Eu usei o escuro pra trazer a luz de volta. Engraçado, né?"));

            return entries;
        }

        /// <summary>Lore + one comment per cousin (Maithe, Maria, Joaquim, Ícaro, Lavine, in that order).</summary>
        private static void Page(
            List<LocalizationEntry> entries, string enemyId, string loreEn, string lorePt,
            (string en, string pt) maithe, (string en, string pt) maria, (string en, string pt) joaquim,
            (string en, string pt) icaro, (string en, string pt) lavine)
        {
            entries.Add(new LocalizationEntry(BestiaryCatalog.LoreKey(enemyId), loreEn, lorePt));
            (string en, string pt)[] comments = { maithe, maria, joaquim, icaro, lavine };
            for (int i = 0; i < Cousins.Length; i++)
            {
                entries.Add(new LocalizationEntry(BestiaryCatalog.CommentKey(enemyId, Cousins[i]), comments[i].en, comments[i].pt));
            }
        }

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Title)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        /// <summary>Localized text via <paramref name="manager"/> when available, else the pt-BR fallback, else the key.</summary>
        public static string Get(LocalizationManager manager, string key)
        {
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

        /// <summary><see cref="Get"/> + <c>string.Format</c>; falls back to the pt-BR pattern if a translation's placeholders are broken.</summary>
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
            if (_fallbackByKey == null)
            {
                _fallbackByKey = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    _fallbackByKey[entry.Key] = entry.PtBR;
                }
            }

            return key != null && _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }
    }
}
