using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.EventRooms
{
    /// <summary>When a cousin speaks in a room.</summary>
    public enum EventRoomMoment
    {
        Start = 0,
        Win = 1,
        Lose = 2,
    }

    /// <summary>
    /// Localization keys (en + pt-BR) of the event rooms: names, the world-map pill and button (<c>ui.event_room.map.*</c>),
    /// the in-room HUD and results (<c>ui.event_room.hud.*</c>, <c>ui.event_room.result.*</c>), the Torre's props, one line
    /// per cousin at the start and at the end of each room (<c>event_room.line.*</c>), the entry items
    /// (<c>item.item_convite_terreiro.*</c>, <c>item.item_chave_engenho.*</c>; pt-BR = the asset text) (the three achievements' texts live in
    /// <c>AchievementTexts</c>). Same self-contained pattern as <c>FishingTexts</c>: merged into the live
    /// <see cref="LocalizationManager"/> by <see cref="EventRoomProgress.Initialize"/> (keys it already has win), pt-BR
    /// fallback without one.
    /// </summary>
    public static class EventRoomTexts
    {
        public const string TerreiroName = "ui.event_room.terreiro.name";
        public const string TorreName = "ui.event_room.torre.name";

        /// <summary>{0} = room, {1} = closing hour.</summary>
        public const string MapOpen = "ui.event_room.map.open";
        /// <summary>{0} = room, {1} = opening hour.</summary>
        public const string MapClosed = "ui.event_room.map.closed";
        /// <summary>{0} = room, {1} = next opening hour.</summary>
        public const string MapEntered = "ui.event_room.map.entered";
        /// <summary>{0} = room.</summary>
        public const string MapEnterButton = "ui.event_room.map.enter";
        /// <summary>{0} = entry item.</summary>
        public const string MapNeedItem = "ui.event_room.map.need_item";
        /// <summary>{0} = level.</summary>
        public const string MapNeedLevel = "ui.event_room.map.need_level";
        public const string MapAlreadyEntered = "ui.event_room.map.already_entered";
        public const string MapClosedNow = "ui.event_room.map.closed_now";

        /// <summary>{0} = m:ss.</summary>
        public const string HudTimer = "ui.event_room.hud.timer";
        /// <summary>{0} = score.</summary>
        public const string HudScore = "ui.event_room.hud.score";
        /// <summary>{0} = combo count, {1} = multiplier (×1.3).</summary>
        public const string HudCombo = "ui.event_room.hud.combo";
        public const string HudGoalTerreiro = "ui.event_room.hud.goal_terreiro";
        /// <summary>{0} = kills, {1} = needed.</summary>
        public const string HudGoalBridge = "ui.event_room.hud.goal_bridge";
        public const string HudGoalGate = "ui.event_room.hud.goal_gate";
        public const string HudGoalGuardian = "ui.event_room.hud.goal_guardian";
        public const string HudGoalPickUp = "ui.event_room.hud.goal_pick_up";
        public const string HudGoalDeliver = "ui.event_room.hud.goal_deliver";
        public const string HudBridgeOpen = "ui.event_room.hud.bridge_open";
        public const string HudGateBroken = "ui.event_room.hud.gate_broken";
        public const string HudGuardianAppears = "ui.event_room.hud.guardian_appears";
        public const string HudMoendaTaken = "ui.event_room.hud.moenda_taken";

        public const string ResultTitleCleared = "ui.event_room.result.title_cleared";
        public const string ResultTitleEnded = "ui.event_room.result.title_ended";
        /// <summary>{0} = score.</summary>
        public const string ResultScore = "ui.event_room.result.score";
        /// <summary>{0} = tier.</summary>
        public const string ResultTier = "ui.event_room.result.tier";
        /// <summary>{0} = personal best.</summary>
        public const string ResultBest = "ui.event_room.result.best";
        public const string ResultNewBest = "ui.event_room.result.new_best";
        /// <summary>{0} = kills, {1} = best combo.</summary>
        public const string ResultStats = "ui.event_room.result.stats";
        /// <summary>{0} = XP, {1} = gold.</summary>
        public const string ResultRewards = "ui.event_room.result.rewards";
        /// <summary>{0} = amount, {1} = stone name.</summary>
        public const string ResultStones = "ui.event_room.result.stones";
        public const string ResultNoReward = "ui.event_room.result.no_reward";
        public const string ResultBagFull = "ui.event_room.result.bag_full";

        public const string TierBronze = "ui.event_room.tier.bronze";
        public const string TierPrata = "ui.event_room.tier.prata";
        public const string TierOuro = "ui.event_room.tier.ouro";
        public const string TierMestre = "ui.event_room.tier.mestre";
        public const string TierNone = "ui.event_room.tier.none";

        public const string GuardianName = "event_room.torre.guardian";
        public const string KeeperName = "event_room.torre.keeper";
        public const string MoendaName = "event_room.torre.moenda";
        public const string GateName = "event_room.torre.gate";
        public const string StoneBlessing = "event_room.stone.blessing";
        public const string StoneSoul = "event_room.stone.soul";

        public const string TicketNameKey = "item.item_convite_terreiro.name";
        public const string TicketDescriptionKey = "item.item_convite_terreiro.description";
        public const string KeyNameKey = "item.item_chave_engenho.name";
        public const string KeyDescriptionKey = "item.item_chave_engenho.description";

        /// <summary>pt-BR names/descriptions of the entry items, also the asset text (kept in sync by EventRoomTests).</summary>
        public const string TicketNamePt = "Convite do Terreiro";
        public const string TicketDescriptionPt = "Convite pintado à mão para o Terreiro das Assombrações. Vale uma entrada quando o terreiro abre.";
        public const string KeyNamePt = "Chave do Engenho";
        public const string KeyDescriptionPt = "Chave de ferro velho da Torre do Engenho Velho. Abre a torre uma vez, na hora em que ela abre.";

        public static readonly List<LocalizationEntry> Entries = BuildEntries();

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        public static string RoomNameKey(EventRoomKind kind) => kind == EventRoomKind.Torre ? TorreName : TerreiroName;

        public static string TierKey(EventRoomTier tier)
        {
            switch (tier)
            {
                case EventRoomTier.Bronze: return TierBronze;
                case EventRoomTier.Prata: return TierPrata;
                case EventRoomTier.Ouro: return TierOuro;
                case EventRoomTier.Mestre: return TierMestre;
                default: return TierNone;
            }
        }

        public static string LineKey(EventRoomKind kind, EventRoomMoment moment, CharacterId hero)
        {
            string room = kind == EventRoomKind.Torre ? "torre" : "terreiro";
            string when = moment == EventRoomMoment.Start ? "start" : moment == EventRoomMoment.Win ? "win" : "lose";
            return $"event_room.line.{room}.{when}.{hero.ToString().ToLowerInvariant()}";
        }

        private static List<LocalizationEntry> BuildEntries()
        {
            var entries = new List<LocalizationEntry>
            {
                new LocalizationEntry(TerreiroName, "Spook Yard", "Terreiro das Assombrações"),
                new LocalizationEntry(TorreName, "Old Mill Tower", "Torre do Engenho Velho"),

                new LocalizationEntry(MapOpen, "{0}: OPEN until {1}!", "{0}: ABERTO até {1}!"),
                new LocalizationEntry(MapClosed, "{0}: opens at {1}", "{0}: abre às {1}"),
                new LocalizationEntry(MapEntered, "{0}: done for now, next at {1}", "{0}: já foi desta vez, próxima às {1}"),
                new LocalizationEntry(MapEnterButton, "Enter: {0}", "Entrar: {0}"),
                new LocalizationEntry(MapNeedItem, "You need a {0} to enter.", "Você precisa de um {0} para entrar."),
                new LocalizationEntry(MapNeedLevel, "Come back at level {0}.", "Volte no nível {0}."),
                new LocalizationEntry(MapAlreadyEntered, "You already went in this time. Wait for the next opening!", "Você já entrou desta vez. Espere a próxima abertura!"),
                new LocalizationEntry(MapClosedNow, "It just closed. Wait for the next opening!", "Acabou de fechar. Espere a próxima abertura!"),

                new LocalizationEntry(HudTimer, "{0}", "{0}"),
                new LocalizationEntry(HudScore, "Points: {0}", "Pontos: {0}"),
                new LocalizationEntry(HudCombo, "Combo {0} ({1})", "Combo {0} ({1})"),
                new LocalizationEntry(HudGoalTerreiro, "Hold out until the end!", "Aguente até o fim!"),
                new LocalizationEntry(HudGoalBridge, "Defeat creatures to lower the bridge: {0}/{1}", "Derrote bichos para baixar a ponte: {0}/{1}"),
                new LocalizationEntry(HudGoalGate, "Break the old mill gate!", "Quebre o portão do engenho!"),
                new LocalizationEntry(HudGoalGuardian, "Beat the Tower Guardian!", "Vença o Guardião da Torre!"),
                new LocalizationEntry(HudGoalPickUp, "Grab the Golden Cane Press!", "Pegue a Moenda de Ouro!"),
                new LocalizationEntry(HudGoalDeliver, "Take the Golden Cane Press to Mr. Malaquias!", "Leve a Moenda de Ouro até Seu Malaquias!"),
                new LocalizationEntry(HudBridgeOpen, "The bridge is down!", "A ponte baixou!"),
                new LocalizationEntry(HudGateBroken, "The gate broke!", "O portão quebrou!"),
                new LocalizationEntry(HudGuardianAppears, "The Tower Guardian woke up!", "O Guardião da Torre acordou!"),
                new LocalizationEntry(HudMoendaTaken, "Got it! Now run to the entrance!", "Peguei! Agora corre pra entrada!"),

                new LocalizationEntry(ResultTitleCleared, "{0}: cleared!", "{0}: vencido!"),
                new LocalizationEntry(ResultTitleEnded, "{0}: over", "{0}: fim"),
                new LocalizationEntry(ResultScore, "Points: {0}", "Pontos: {0}"),
                new LocalizationEntry(ResultTier, "Rank: {0}", "Medalha: {0}"),
                new LocalizationEntry(ResultBest, "Personal best: {0}", "Melhor marca: {0}"),
                new LocalizationEntry(ResultNewBest, "New personal best!", "Nova melhor marca!"),
                new LocalizationEntry(ResultStats, "{0} defeated · best combo {1}", "{0} derrotados · melhor combo {1}"),
                new LocalizationEntry(ResultRewards, "+{0} XP · +{1} gold", "+{0} XP · +{1} ouro"),
                new LocalizationEntry(ResultStones, "+{0} {1}", "+{0} {1}"),
                new LocalizationEntry(ResultNoReward, "No medal this time. Try again at the next opening!", "Sem medalha desta vez. Tente de novo na próxima abertura!"),
                new LocalizationEntry(ResultBagFull, "The backpack got full: some of it stayed behind.", "A mochila encheu: uma parte ficou para trás."),

                new LocalizationEntry(TierBronze, "Bronze", "Bronze"),
                new LocalizationEntry(TierPrata, "Silver", "Prata"),
                new LocalizationEntry(TierOuro, "Gold", "Ouro"),
                new LocalizationEntry(TierMestre, "Master", "Mestre"),
                new LocalizationEntry(TierNone, "None", "Nenhuma"),

                new LocalizationEntry(GuardianName, "Tower Guardian", "Guardião da Torre"),
                new LocalizationEntry(KeeperName, "Mr. Malaquias, the mill keeper", "Seu Malaquias, o vigia do engenho"),
                new LocalizationEntry(MoendaName, "Golden Cane Press", "Moenda de Ouro"),
                new LocalizationEntry(GateName, "Old Mill Gate", "Portão do Engenho"),
                new LocalizationEntry(StoneBlessing, "Blessing Stone", "Pedra da Bênção"),
                new LocalizationEntry(StoneSoul, "Soul Stone", "Pedra da Alma"),

                new LocalizationEntry(TicketNameKey, "Spook Yard Invitation", TicketNamePt),
                new LocalizationEntry(TicketDescriptionKey, "A hand-painted invitation to the Spook Yard. Good for one entry while the yard is open.", TicketDescriptionPt),
                new LocalizationEntry(KeyNameKey, "Old Mill Key", KeyNamePt),
                new LocalizationEntry(KeyDescriptionKey, "An old iron key to the Old Mill Tower. Opens the tower once, while it is open.", KeyDescriptionPt),

            };

            AddLines(entries, EventRoomKind.Terreiro, EventRoomMoment.Start, new[]
            {
                (CharacterId.Maithe, "Eye of the Wind open. Bring on the spooks, I can see them all!", "Olho do Vento aberto. Pode vir assombração, eu vejo todas!"),
                (CharacterId.Icaro, "I did the math: no spook can handle a good gadget!", "Calculei tudo: assombração não aguenta engenhoca boa!"),
                (CharacterId.Joaquim, "Stay behind my shield. The yard is ours today!", "Fiquem atrás do meu escudo. O Terreiro hoje é nosso!"),
                (CharacterId.Maria, "One blessing for courage and one for luck. Let's go!", "Uma bênção pra coragem e outra pra sorte. Vamos!"),
                (CharacterId.Lavine, "Spooks? I've got magic tickles for them!", "Assombração? Eu tenho é cócegas de magia pra elas!"),
            });
            AddLines(entries, EventRoomKind.Terreiro, EventRoomMoment.Win, new[]
            {
                (CharacterId.Maithe, "Nothing got past my aim. Ícaro will be jealous!", "Nada passou pela minha mira. O Ícaro vai ficar com inveja!"),
                (CharacterId.Icaro, "It worked! I wrote every combo down in my invention notebook.", "Funcionou! Anotei cada combo no meu caderno de invenções."),
                (CharacterId.Joaquim, "Nobody knocked the Root Shield down. Maria will be proud!", "Ninguém derrubou o Escudo das Raízes. A Maria vai ficar orgulhosa!"),
                (CharacterId.Maria, "Everyone in one piece! Grandma Zefa will bake a cake.", "Todo mundo inteiro! A Vó Zefa vai fazer um bolo pra comemorar."),
                (CharacterId.Lavine, "I did so many combos! Did you see? Did you?!", "Fiz um monte de combo! Vocês viram? Viram?!"),
            });
            AddLines(entries, EventRoomKind.Terreiro, EventRoomMoment.Lose, new[]
            {
                (CharacterId.Maithe, "Too many at once... next time I'll aim faster.", "Foram muitas de uma vez... Da próxima eu miro mais rápido."),
                (CharacterId.Icaro, "Hmm... I need a new gadget. One that handles more creatures!", "Hum... preciso de uma engenhoca nova. Uma que aguente mais bicho!"),
                (CharacterId.Joaquim, "I fell, but I fell standing tall. The shield comes back stronger.", "Caí, mas caí firme. O escudo volta mais forte."),
                (CharacterId.Maria, "Breathe... we patch up and come back.", "Respira... a gente cuida dos machucados e volta."),
                (CharacterId.Lavine, "Too many spooks... but I wasn't scared. Almost.", "Ai, foi assombração demais... mas eu não fiquei com medo. Quase."),
            });
            AddLines(entries, EventRoomKind.Torre, EventRoomMoment.Start, new[]
            {
                (CharacterId.Maithe, "The cane press is up top. Bridge, gate, guardian: one arrow at a time.", "A moenda está lá no alto. Ponte, portão, guardião: uma flecha de cada vez."),
                (CharacterId.Icaro, "An old mill gate? Leave it to me, I know where it creaks.", "Um portão de engenho velho? Deixa comigo, eu sei onde ele range."),
                (CharacterId.Joaquim, "A thick gate? My shield is thicker.", "Portão grosso? Meu escudo é mais."),
                (CharacterId.Maria, "The tower is dark, but the blessing's light goes first.", "A torre é escura, mas a luz da bênção vai na frente."),
                (CharacterId.Lavine, "I'm small, but my magic climbs towers super fast!", "Eu sou pequena, mas a minha magia sobe torre rapidinho!"),
            });
            AddLines(entries, EventRoomKind.Torre, EventRoomMoment.Win, new[]
            {
                (CharacterId.Maithe, "Cane press delivered, Mr. Malaquias! The mill will sing again.", "Moenda entregue, Seu Malaquias! O engenho vai cantar de novo."),
                (CharacterId.Icaro, "A golden cane press! Could we build a motorized rapadura maker?", "Moenda de Ouro! Será que dá pra fazer uma rapadura motorizada?"),
                (CharacterId.Joaquim, "Guarded and delivered. Guardian mission complete!", "Guardada e entregue. Missão de guardião cumprida!"),
                (CharacterId.Maria, "It shines like the sun! Mr. Malaquias even teared up.", "Ela brilha que nem sol! Seu Malaquias até se emocionou."),
                (CharacterId.Lavine, "I delivered it! Can I have a piece of rapadura now?", "Entreguei a moenda! Posso ganhar um pedaço de rapadura agora?"),
            });
            AddLines(entries, EventRoomKind.Torre, EventRoomMoment.Lose, new[]
            {
                (CharacterId.Maithe, "Time flew faster than my arrows. Let's try again.", "O tempo voou mais que as minhas flechas. Vamos tentar de novo."),
                (CharacterId.Icaro, "The hourglass won this time. I'll invent a slower clock!", "A ampulheta ganhou dessa vez. Vou inventar um relógio mais devagar!"),
                (CharacterId.Joaquim, "We got close. A real guardian never gives up.", "Chegamos perto. Guardião de verdade não desiste."),
                (CharacterId.Maria, "No time left, but nobody really got hurt. That's what matters.", "Não deu tempo, mas ninguém se machucou de verdade. É isso que importa."),
                (CharacterId.Lavine, "The tower won today. Tomorrow I'm back with new magic!", "A torre ganhou hoje. Amanhã eu volto com magia nova!"),
            });

            return entries;
        }

        private static void AddLines(List<LocalizationEntry> entries, EventRoomKind kind, EventRoomMoment moment, (CharacterId hero, string en, string pt)[] lines)
        {
            foreach ((CharacterId hero, string en, string pt) in lines)
            {
                entries.Add(new LocalizationEntry(LineKey(kind, moment, hero), en, pt));
            }
        }

        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(TerreiroName)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

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
                var table = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    table[entry.Key] = entry.PtBR;
                }

                _fallbackByKey = table;
            }

            return key != null && _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }
    }
}
