using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Events;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// Localization keys (en + pt-BR) of the world map road events (<see cref="WorldEventCardController"/>): the card's
    /// common texts, the effect summaries and, per <see cref="WorldEventCatalog"/> event, the keys of
    /// <see cref="WorldEventSpec"/> (title, body, two choices, two results, one line per cousin). Same self-contained
    /// pattern as <see cref="VillageHubTexts"/>: merged into the live <see cref="LocalizationManager"/> on first use
    /// (keys it already has win), pt-BR fallback when no manager is registered.
    /// </summary>
    public static class WorldEventTexts
    {
        public const string Header = "ui.world_event.header";
        public const string Continue = "ui.world_event.continue";
        public const string Fight = "ui.world_event.fight";
        public const string CannotAfford = "ui.world_event.cannot_afford";
        public const string NoEffect = "ui.world_event.no_effect";
        /// <summary>{0} = cousin's name, {1} = their line.</summary>
        public const string HeroLine = "ui.world_event.hero_line";

        /// <summary>{0} = amount, {1} = item name.</summary>
        public const string FxGain = "ui.world_event.fx.gain";
        /// <summary>{0} = amount, {1} = item name.</summary>
        public const string FxPay = "ui.world_event.fx.pay";
        public const string FxExperience = "ui.world_event.fx.xp";
        public const string FxHeal = "ui.world_event.fx.heal";
        public const string FxBuffAttack = "ui.world_event.fx.buff_attack";
        public const string FxBuffDefense = "ui.world_event.fx.buff_defense";
        public const string FxBuffRegen = "ui.world_event.fx.buff_regen";
        public const string FxBuffCrit = "ui.world_event.fx.buff_crit";
        public const string FxBuffOther = "ui.world_event.fx.buff_other";
        public const string FxSlow = "ui.world_event.fx.slow";
        public const string FxHours = "ui.world_event.fx.hours";
        public const string FxElite = "ui.world_event.fx.elite";
        public const string FxFight = "ui.world_event.fx.fight";

        /// <summary>Order of the five cousin lines passed to <c>Add</c> (birth order). Declared before <see cref="Entries"/>: static initializers run top-down.</summary>
        private static readonly CharacterId[] HeroOrder = { CharacterId.Maithe, CharacterId.Maria, CharacterId.Joaquim, CharacterId.Icaro, CharacterId.Lavine };

        public static readonly List<LocalizationEntry> Entries = BuildEntries();

        private static List<LocalizationEntry> BuildEntries()
        {
            var entries = new List<LocalizationEntry>
            {
                new LocalizationEntry(Header, "On the road", "Na estrada"),
                new LocalizationEntry(Continue, "Continue the trip", "Seguir viagem"),
                new LocalizationEntry(Fight, "Fight!", "Lutar!"),
                new LocalizationEntry(CannotAfford, "You don't have enough.", "Você não tem o suficiente."),
                new LocalizationEntry(NoEffect, "Nothing changes.", "Nada muda."),
                new LocalizationEntry(HeroLine, "{0}: \"{1}\"", "{0}: \"{1}\""),

                new LocalizationEntry(FxGain, "+{0} {1}", "+{0} {1}"),
                new LocalizationEntry(FxPay, "-{0} {1}", "-{0} {1}"),
                new LocalizationEntry(FxExperience, "+{0} XP in the next fight", "+{0} XP na próxima luta"),
                new LocalizationEntry(FxHeal, "Restores {0}% health in the next fight", "Recupera {0}% da vida na próxima luta"),
                new LocalizationEntry(FxBuffAttack, "+{0}% damage in the next fight", "+{0}% de dano na próxima luta"),
                new LocalizationEntry(FxBuffDefense, "+{0}% defense in the next fight", "+{0}% de defesa na próxima luta"),
                new LocalizationEntry(FxBuffRegen, "+{0} health/s in the next fight", "+{0} de vida/s na próxima luta"),
                new LocalizationEntry(FxBuffCrit, "+{0}% crit chance in the next fight", "+{0}% de chance de crítico na próxima luta"),
                new LocalizationEntry(FxBuffOther, "A bonus in the next fight", "Um bônus na próxima luta"),
                new LocalizationEntry(FxSlow, "Trip {0}% slower", "Viagem {0}% mais lenta"),
                new LocalizationEntry(FxHours, "+{0} h on the clock", "+{0} h no relógio"),
                new LocalizationEntry(FxElite, "Fight an elite enemy (more loot)", "Luta contra um inimigo de elite (mais loot)"),
                new LocalizationEntry(FxFight, "A fight starts", "Começa uma luta"),
            };

            Add(entries, WorldEventCatalog.Peddler,
                ("Wandering peddler", "Mascate ambulante"),
                ("A peddler with a donkey loaded with baskets waves from the roadside: \"Good, cheap potions, friend! Today only!\"",
                    "Um mascate com o jumento carregado de balaios acena da beira da estrada: \"Poção boa e barata, freguês! Só hoje!\""),
                ("Buy the potions", "Comprar as poções"),
                ("Thank him and move on", "Agradecer e seguir"),
                ("The peddler wraps the potions in a cloth and wishes you a good trip.", "O mascate embrulha as poções num pano e ainda deseja boa viagem."),
                ("\"Next time, then!\" The donkey brays goodbye.", "\"Fica pra próxima, freguês!\" O jumento zurra em despedida."),
                new[]
                {
                    ("Half the village price? I'd take it. Someone always gets hurt.", "Pela metade do preço da Vila? Eu levaria. Alguém sempre se machuca."),
                    ("Grandma Zefa would say to haggle a little more...", "A Vó Zefa ia mandar pechinchar mais um pouquinho..."),
                    ("Buy them. Maria keeps saying I throw myself into danger.", "Compra. A Maria vive dizendo que eu me jogo no perigo."),
                    ("Wonder if he sells engine parts too? My drone is creaking.", "Será que ele vende peça de motor também? Meu drone tá rangendo."),
                    ("He winked at the donkey. I saw it. I don't trust him.", "Ele piscou pro jumento. Eu vi. Não confio."),
                });

            Add(entries, WorldEventCatalog.CashewRain,
                ("Cashew rain", "Chuva de caju"),
                ("A strong wind shakes a loaded cashew tree and ripe cashews tumble onto the road, sweet and fragrant.",
                    "Um vento forte sacode um cajueiro carregado e os cajus maduros despencam pela estrada, doces e cheirosos."),
                ("Eat them right there, in the shade", "Comer ali mesmo, na sombra"),
                ("Make juice for the road", "Fazer suco para a viagem"),
                ("Cashew juice runs down your chin. Your body thanks you.", "O caju escorre pelo queixo. O corpo agradece."),
                ("The gourd is full of juice. It'll keep you going in the next fight.", "A cabaça vai cheia de suco. Vai dar fôlego na próxima briga."),
                new[]
                {
                    ("If Ícaro were here, he'd have burnt his mouth on the raw nut already.", "Se o Ícaro estivesse aqui, já tinha queimado a boca na castanha crua."),
                    ("Cashews remind me of Grandma Zefa's yard. Joaquim climbed the tree and I picked them up below.",
                        "Caju lembra o quintal da Vó Zefa. O Joaquim subia no pé e eu catava embaixo."),
                    ("I could climb this tree with my eyes shut. Maria always held the basin.", "Eu subia nesse pé de olho fechado. A Maria que segurava a bacia."),
                    ("Forty kilometres an hour, at least. You can measure it by the falling cashews!", "Vento de quarenta por hora, no mínimo. Dá pra medir pela queda dos cajus!"),
                    ("It rained cashews! That counts as luck, right? Say it counts.", "Choveu caju! Isso conta como sorte, né? Diz que conta."),
                });

            Add(entries, WorldEventCatalog.Pilgrims,
                ("Pilgrims on the road", "Romeiros na estrada"),
                ("A line of straw-hatted pilgrims walks by singing hymns. An old woman asks for help: her grandson twisted his ankle.",
                    "Uma fila de romeiros de chapéu de palha passa cantando benditos. Uma senhora pede ajuda: o neto torceu o pé."),
                ("Give a Health Potion", "Dar uma Poção de Vida"),
                ("Pray with them and move on", "Rezar com eles e seguir"),
                ("The boy walks steady again. The pilgrims pool some coins and share stories of the road.",
                    "O menino já pisa firme. Os romeiros juntam umas moedas e contam histórias da estrada."),
                ("The woman blesses your forehead with a sprig: \"Go with God, He'll keep you.\"", "A senhora benze sua testa com um raminho: \"Vai com Deus, que Ele te guarda.\""),
                new[]
                {
                    ("Mom always helped the pilgrims who passed our door. I do the same.", "Mamãe sempre ajudava os romeiros que passavam na porta. Eu faço igual."),
                    ("Let me look at that ankle. Light heals, but kindness helps too.", "Deixa eu ver esse pé. A luz cura, mas um carinho ajuda também."),
                    ("I'd carry the boy on my back to the next village if I had to.", "Eu levava o menino nas costas até a próxima vila, se precisasse."),
                    ("You walk all that way? I'd build you a motor cart.", "Vocês andam tudo isso a pé? Eu fazia um carrinho com motor pra vocês."),
                    ("Do you think they'd pray for me too? I was born on the Noite Rubra...", "Será que eles rezam por mim também? Eu nasci na Noite Rubra..."),
                });

            Add(entries, WorldEventCatalog.EliteAmbush,
                ("Ambush!", "Emboscada!"),
                ("Branches snap on both sides of the trail. A huge beast with glowing eyes leads the pack blocking the way.",
                    "Galhos estalam dos dois lados da trilha. Um bicho enorme, de olhos acesos, lidera o bando que fecha o caminho."),
                ("Face the pack leader", "Enfrentar o líder do bando"),
                ("Slip away through the brush", "Despistar pelo mato"),
                ("No turning back: fight!", "Não tem volta: é lutar!"),
                ("You circle through thick brush. You get away, but lose a good while.", "Você dá a volta pelo mato fechado. Escapa, mas perde um bom tempo."),
                new[]
                {
                    ("Stay behind me. I'll drop the big one before it gets close.", "Fica atrás de mim. Eu derrubo o grandão antes que ele chegue perto."),
                    ("Breathe. If it goes wrong, I'm here to heal.", "Respira. Se der errado, eu tô aqui pra curar."),
                    ("Great. I was itching to test the Stone Wall on one of these.", "Ótimo. Eu tava doido pra testar a Muralha de Pedra num desses."),
                    ("Turret up in three seconds. Let it come!", "Torreta montada em três segundos. Pode vir!"),
                    ("It's big... but I won't run. Not this time.", "Ele é grande... mas eu não vou correr. Não dessa vez."),
                });

            Add(entries, WorldEventCatalog.Campfire,
                ("Singing by the campfire", "Cantoria ao pé da fogueira"),
                ("A campfire crackles by the road. Cowhands play the viola and start a repente, calling you to join the circle.",
                    "Uma fogueira estala na beira da estrada. Vaqueiros tocam viola e puxam um repente, chamando para a roda."),
                ("Sing along until late", "Cantar junto até tarde"),
                ("Rest by the fire", "Descansar perto do fogo"),
                ("The whole circle claps for your verse. You leave with your chest puffed out.", "A roda inteira bate palma pro seu verso. Você sai de peito estufado."),
                ("The fire's warmth and a mug of coffee soothe your tired body.", "O calor do fogo e uma caneca de café acalmam o corpo cansado."),
                new[]
                {
                    ("Ícaro used to sing off-key on purpose just to make me laugh. I miss it.", "O Ícaro desafinava de propósito só pra me ver rir. Que saudade."),
                    ("Grandma Zefa sang this one to put us to sleep.", "A Vó Zefa cantava essa pra gente dormir."),
                    ("I don't sing. ...Fine, just the chorus.", "Eu não canto. ...Tá bom, só o refrão."),
                    ("Hook that viola to a lightning amp and the whole backlands will hear the party!", "Liga essa viola num amplificador de raio e o sertão inteiro escuta a festa!"),
                    ("Sometimes the night gives me a chill... but by the fire and the music, it goes away.", "Às vezes a noite me dá um friozinho na barriga... mas perto do fogo e da música, passa."),
                });

            Add(entries, WorldEventCatalog.Sandstorm,
                ("Sandstorm", "Tempestade de areia"),
                ("The sky turns clay-coloured and a wall of sand rolls across the desert, swallowing the road.",
                    "O céu fica cor de barro e uma parede de areia avança pelo deserto, engolindo a estrada."),
                ("Push on with a scarf over your face", "Seguir com o lenço no rosto"),
                ("Shelter among the rocks", "Abrigar-se entre as pedras"),
                ("Step by step, eyes half shut. The trip will take a while.", "Passo a passo, de olho semicerrado. A viagem vai demorar."),
                ("You wait out the storm, huddled behind a rock slab.", "Você espera a tempestade passar, encolhido atrás de um lajedo."),
                new[]
                {
                    ("In this wind, not even an arrow flies straight. Better take it slow.", "Com esse vento, nem flecha voa reto. Melhor ir com calma."),
                    ("Cover your nose and mouth. And drink water, okay?", "Cobre o nariz e a boca. E bebe água, viu?"),
                    ("Sand won't knock me down. Onward.", "Areia não me derruba. Bora em frente."),
                    ("Sand in the drone's gears, again! I'll have to take it all apart.", "Areia nas engrenagens do drone, de novo! Vou ter que desmontar tudo."),
                    ("Can't see a thing... Anyone there? Oh, right, it's just me.", "Não dá pra ver nada... Tem alguém aí? Ah, é, sou só eu."),
                });

            Add(entries, WorldEventCatalog.MountainFog,
                ("Mountain fog", "Neblina na serra"),
                ("A white fog rolls down the mountain, so thick the trail vanishes two steps ahead.",
                    "Uma neblina branca desce a serra, tão espessa que a trilha some a dois passos."),
                ("Wait for the fog to lift", "Esperar a neblina baixar"),
                ("Feel your way along the trail", "Tatear pela trilha"),
                ("Hours later, the sun opens up the mountain again.", "Horas depois, o sol abre a serra de novo."),
                ("Slowly, hand on the rock. In a bank, a few chunks of ore glint.", "Devagar, mão na rocha. Num barranco, uns pedaços de minério brilham."),
                new[]
                {
                    ("I don't shoot what I can't see. And I don't rush a trail I can't see.", "Eu não atiro no que não vejo. E não corro numa trilha que não vejo."),
                    ("Let's go slowly. No one gets lost if we're not in a hurry.", "Vamos devagar. Ninguém se perde se a gente não tiver pressa."),
                    ("Rock is solid, even in the dark. I can feel it underfoot.", "Pedra é firme, até no escuro. Eu sinto pelo pé."),
                    ("If I'd finished the drone's radar... it's on the list!", "Se eu tivesse terminado o radar do drone... tá na lista!"),
                    ("Mist is my friend. The Mist of Illusion, at least. This one I'm not sure about.", "Névoa é minha amiga. A Névoa da Ilusão, pelo menos. Essa aqui eu não sei."),
                });

            Add(entries, WorldEventCatalog.CaveEcho,
                ("Cave echo", "Eco na caverna"),
                ("Your footsteps echo down the gallery... and from deep inside, something answers, mimicking your voice.",
                    "Seus passos ecoam pela galeria... e, lá do fundo, alguma coisa responde imitando sua voz."),
                ("Shout back", "Gritar de volta"),
                ("Move on in silence", "Seguir em silêncio"),
                ("The echo stops. Claws scrape the stone, closer and closer.", "O eco para. Garras arranham a pedra, cada vez mais perto."),
                ("Step by careful step, you learn the cave's rhythm. Nothing will catch you off guard.", "Pé ante pé, você aprende o ritmo da caverna. Nada vai te pegar de surpresa."),
                new[]
                {
                    ("One true shot in the dark is worth ten in daylight.", "Um tiro certeiro no escuro vale mais que dez no claro."),
                    ("Let the light go first. I'm not afraid of what it shows.", "Que a luz vá na frente. Eu não tenho medo do que ela mostra."),
                    ("Want a fight? Then come on! HEY!", "Quer briga? Então vem! Ô DE CASA!"),
                    ("Half-second echo... the gallery is about eighty metres deep. Or it's a creature.", "Eco de meio segundo... a galeria tem uns oitenta metros. Ou é bicho."),
                    ("Darkness doesn't scare me, I was born in it. ...But that echo copied my voice.", "O escuro não me assusta, eu nasci nele. ...Mas esse eco imitou minha voz."),
                });

            return entries;
        }

        private static void Add(List<LocalizationEntry> entries, string eventId,
            (string En, string Pt) title, (string En, string Pt) body,
            (string En, string Pt) choiceA, (string En, string Pt) choiceB,
            (string En, string Pt) resultA, (string En, string Pt) resultB,
            (string En, string Pt)[] heroLines)
        {
            WorldEventSpec spec = WorldEventCatalog.Get(eventId);
            string keyBase = WorldEventCatalog.KeyPrefix + eventId;
            entries.Add(new LocalizationEntry(keyBase + ".title", title.En, title.Pt));
            entries.Add(new LocalizationEntry(keyBase + ".body", body.En, body.Pt));
            entries.Add(new LocalizationEntry(keyBase + ".choice_a", choiceA.En, choiceA.Pt));
            entries.Add(new LocalizationEntry(keyBase + ".choice_b", choiceB.En, choiceB.Pt));
            entries.Add(new LocalizationEntry(keyBase + ".result_a", resultA.En, resultA.Pt));
            entries.Add(new LocalizationEntry(keyBase + ".result_b", resultB.En, resultB.Pt));

            for (int i = 0; i < HeroOrder.Length && i < heroLines.Length; i++)
            {
                string key = spec != null ? spec.LineKey(HeroOrder[i]) : keyBase + ".line_" + HeroOrder[i].ToString().ToLowerInvariant();
                entries.Add(new LocalizationEntry(key, heroLines[i].En, heroLines[i].Pt));
            }
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
    }
}
