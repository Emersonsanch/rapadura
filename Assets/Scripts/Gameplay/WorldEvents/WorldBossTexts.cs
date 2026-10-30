using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.WorldEvents
{
    /// <summary>When a cousin speaks about a scheduled world event.</summary>
    public enum WorldEventMoment
    {
        /// <summary>On the world map, when a boss window opens.</summary>
        BossAppears = 0,
        BossStart = 1,
        BossStartNight = 2,
        BossWin = 3,
        BossLose = 4,

        /// <summary>On the world map, when an invasion window opens.</summary>
        InvasionAppears = 5,
        InvasionStart = 6,
        InvasionWin = 7,
        InvasionLose = 8,
    }

    /// <summary>
    /// Localization keys (en + pt-BR) of the world bosses and the Invasão do Cangaço: boss names (<c>world_boss.*</c>),
    /// region phrases, the world-map pill and buttons (<c>ui.world_boss.map.*</c>), the fight HUD and results
    /// (<c>ui.world_boss.hud.*</c>, <c>ui.world_boss.result.*</c>) and one line per cousin per moment
    /// (<c>world_boss.line.*</c>). Same self-contained pattern as <c>EventRoomTexts</c>: merged into the live
    /// <see cref="LocalizationManager"/> by <see cref="WorldEventProgress.Initialize"/> (keys it already has win), pt-BR
    /// fallback without one.
    /// </summary>
    public static class WorldBossTexts
    {
        public const string WorldBossTitle = "ui.world_boss.title";
        public const string InvasionTitle = "ui.world_boss.invasion.title";
        public const string CaptainName = "world_boss.invasion.captain";
        /// <summary>{0} = the animal's name.</summary>
        public const string BanditName = "world_boss.invasion.bandit";
        /// <summary>{0} = +tier.</summary>
        public const string ChestName = "world_boss.invasion.chest";

        /// <summary>{0} = boss, {1} = "na Floresta", {2} = countdown.</summary>
        public const string MapBossOpen = "ui.world_boss.map.boss_open";
        /// <summary>{0} = boss, {1} = "na Floresta", {2} = countdown.</summary>
        public const string MapBossNext = "ui.world_boss.map.boss_next";
        /// <summary>{0} = "na Floresta", {1} = countdown.</summary>
        public const string MapInvasionOpen = "ui.world_boss.map.invasion_open";
        /// <summary>{0} = "na Floresta", {1} = countdown.</summary>
        public const string MapInvasionNext = "ui.world_boss.map.invasion_next";
        /// <summary>{0} = boss.</summary>
        public const string MapFightButton = "ui.world_boss.map.fight";
        public const string MapDefendButton = "ui.world_boss.map.defend";
        /// <summary>{0} = level.</summary>
        public const string MapNeedLevel = "ui.world_boss.map.need_level";
        public const string MapAlreadyDone = "ui.world_boss.map.already_done";
        public const string MapClosedNow = "ui.world_boss.map.closed_now";
        public const string MapNightTag = "ui.world_boss.map.night_tag";

        /// <summary>{0} = boss.</summary>
        public const string HudGoalBoss = "ui.world_boss.hud.goal_boss";
        /// <summary>{0} = wave, {1} = waves.</summary>
        public const string HudGoalWave = "ui.world_boss.hud.goal_wave";
        public const string HudGoalCaptain = "ui.world_boss.hud.goal_captain";
        /// <summary>{0} = points, {1} = chest tier so far.</summary>
        public const string HudPoints = "ui.world_boss.hud.points";
        /// <summary>{0} = boss.</summary>
        public const string HudRage = "ui.world_boss.hud.rage";
        /// <summary>{0} = boss.</summary>
        public const string HudMinions = "ui.world_boss.hud.minions";
        public const string HudCaptainArrives = "ui.world_boss.hud.captain_arrives";
        /// <summary>{0} = wave.</summary>
        public const string HudWaveIncoming = "ui.world_boss.hud.wave_incoming";

        /// <summary>{0} = boss.</summary>
        public const string ResultBossWin = "ui.world_boss.result.boss_win";
        /// <summary>{0} = boss.</summary>
        public const string ResultBossLeft = "ui.world_boss.result.boss_left";
        public const string ResultInvasionWin = "ui.world_boss.result.invasion_win";
        public const string ResultInvasionEnded = "ui.world_boss.result.invasion_ended";
        /// <summary>{0} = points.</summary>
        public const string ResultPoints = "ui.world_boss.result.points";
        public const string ResultNoChest = "ui.world_boss.result.no_chest";
        public const string ResultAlreadyPaid = "ui.world_boss.result.already_paid";
        /// <summary>{0} = XP, {1} = gold.</summary>
        public const string ResultRewards = "ui.world_boss.result.rewards";
        /// <summary>{0} = amount, {1} = stone name.</summary>
        public const string ResultStones = "ui.world_boss.result.stones";
        /// <summary>{0} = house item name.</summary>
        public const string ResultHouseItem = "ui.world_boss.result.house_item";
        /// <summary>{0} = mount name.</summary>
        public const string ResultMount = "ui.world_boss.result.mount";
        public const string ResultBagFull = "ui.world_boss.result.bag_full";

        public static readonly List<LocalizationEntry> Entries = BuildEntries();

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        public static string RegionKey(BiomeType region) => "ui.world_boss.region_in." + RegionId(region);

        public static string RegionId(BiomeType region)
        {
            switch (region)
            {
                case BiomeType.Desert: return "desert";
                case BiomeType.Mountain: return "mountain";
                case BiomeType.Caves: return "caves";
                default: return "forest";
            }
        }

        public static string LineKey(WorldEventMoment moment, CharacterId hero)
        {
            return $"world_boss.line.{MomentId(moment)}.{hero.ToString().ToLowerInvariant()}";
        }

        private static string MomentId(WorldEventMoment moment)
        {
            switch (moment)
            {
                case WorldEventMoment.BossAppears: return "boss_appears";
                case WorldEventMoment.BossStart: return "boss_start";
                case WorldEventMoment.BossStartNight: return "boss_start_night";
                case WorldEventMoment.BossWin: return "boss_win";
                case WorldEventMoment.BossLose: return "boss_lose";
                case WorldEventMoment.InvasionAppears: return "invasion_appears";
                case WorldEventMoment.InvasionStart: return "invasion_start";
                case WorldEventMoment.InvasionWin: return "invasion_win";
                default: return "invasion_lose";
            }
        }

        private static List<LocalizationEntry> BuildEntries()
        {
            var entries = new List<LocalizationEntry>
            {
                new LocalizationEntry(WorldBossTitle, "World boss", "Chefe do mundo"),
                new LocalizationEntry(InvasionTitle, "Cangaço Invasion", "Invasão do Cangaço"),
                new LocalizationEntry(CaptainName, "Az, captain of the gang", "Az, capitão do bando"),
                new LocalizationEntry(BanditName, "{0} of the gang", "{0} do bando"),
                new LocalizationEntry(ChestName, "Cangaço Chest +{0}", "Baú do Cangaço +{0}"),

                new LocalizationEntry(RegionKey(BiomeType.Forest), "in the Forest", "na Floresta"),
                new LocalizationEntry(RegionKey(BiomeType.Desert), "in the Desert", "no Deserto"),
                new LocalizationEntry(RegionKey(BiomeType.Mountain), "on the Mountain", "na Montanha"),
                new LocalizationEntry(RegionKey(BiomeType.Caves), "in the Caves", "nas Cavernas"),

                new LocalizationEntry(MapBossOpen, "World boss: {0} is {1}! Leaves in {2}", "Chefe do mundo: {0} está {1}! Vai embora em {2}"),
                new LocalizationEntry(MapBossNext, "Next world boss: {0} {1}, in {2}", "Próximo chefe do mundo: {0} {1}, em {2}"),
                new LocalizationEntry(MapInvasionOpen, "The Cangaço gang took over {0}! Ends in {1}", "O bando do Cangaço tomou conta {0}! Acaba em {1}"),
                new LocalizationEntry(MapInvasionNext, "Next Cangaço Invasion: {0}, in {1}", "Próxima Invasão do Cangaço: {0}, em {1}"),
                new LocalizationEntry(MapFightButton, "Face: {0}", "Enfrentar: {0}"),
                new LocalizationEntry(MapDefendButton, "Defend from the Cangaço", "Defender do Cangaço"),
                new LocalizationEntry(MapNeedLevel, "You need level {0} for this one.", "Precisa do nível {0} para esse."),
                new LocalizationEntry(MapAlreadyDone, "You already won this time. Come back next time!", "Vocês já venceram desta vez. Voltem na próxima!"),
                new LocalizationEntry(MapClosedNow, "It's gone already. Watch the next countdown!", "Já foi embora. Fiquem de olho na próxima contagem!"),
                new LocalizationEntry(MapNightTag, "(night form)", "(forma da noite)"),

                new LocalizationEntry(HudGoalBoss, "Beat {0} before it leaves!", "Vençam {0} antes que vá embora!"),
                new LocalizationEntry(HudGoalWave, "Gang wave {0} of {1}", "Onda {0} de {1} do bando"),
                new LocalizationEntry(HudGoalCaptain, "Beat Az, the gang's captain!", "Derrotem Az, o capitão do bando!"),
                new LocalizationEntry(HudPoints, "Points: {0} · Chest +{1}", "Pontos: {0} · Baú +{1}"),
                new LocalizationEntry(HudRage, "{0} got furious and called for help!", "{0} ficou furioso e chamou ajuda!"),
                new LocalizationEntry(HudMinions, "{0} called its friends!", "{0} chamou os amigos!"),
                new LocalizationEntry(HudCaptainArrives, "Az, the gang's captain, is here!", "Az, o capitão do bando, chegou!"),
                new LocalizationEntry(HudWaveIncoming, "Here comes gang wave {0}!", "Lá vem a onda {0} do bando!"),

                new LocalizationEntry(ResultBossWin, "{0} defeated!", "{0} vencido!"),
                new LocalizationEntry(ResultBossLeft, "{0} went away...", "{0} foi embora..."),
                new LocalizationEntry(ResultInvasionWin, "The region is free from the Cangaço!", "A região está livre do Cangaço!"),
                new LocalizationEntry(ResultInvasionEnded, "The invasion is over", "A invasão acabou"),
                new LocalizationEntry(ResultPoints, "Points: {0}", "Pontos: {0}"),
                new LocalizationEntry(ResultNoChest, "No chest this time. Defeat more of the gang!", "Sem baú desta vez. Derrotem mais gente do bando!"),
                new LocalizationEntry(ResultAlreadyPaid, "This time's prize was already received.", "O prêmio desta vez já foi recebido."),
                new LocalizationEntry(ResultRewards, "+{0} XP · +{1} gold", "+{0} XP · +{1} de ouro"),
                new LocalizationEntry(ResultStones, "+{0} {1}", "+{0} {1}"),
                new LocalizationEntry(ResultHouseItem, "A delivery for the house: {0}", "Encomenda para a casa: {0}"),
                new LocalizationEntry(ResultMount, "{0} wants to come with you!", "{0} quer ir com vocês!"),
                new LocalizationEntry(ResultBagFull, "The backpack was full: part of the prize stayed behind.", "A mochila estava cheia: parte do prêmio ficou para trás."),
            };

            foreach (WorldBossDefinition boss in WorldBossRules.All)
            {
                entries.Add(new LocalizationEntry(boss.NameKey(false), boss.DayNameEn, boss.DayNamePt));
                entries.Add(new LocalizationEntry(boss.NameKey(true), boss.NightNameEn, boss.NightNamePt));
            }

            AddLines(entries, WorldEventMoment.BossAppears, new[]
            {
                (CharacterId.Maithe, "Did you hear that roar? A world boss showed up. Let's check the map!", "Ouviram esse rugido? Apareceu um chefe do mundo. Bora olhar o mapa!"),
                (CharacterId.Icaro, "My boss detector is beeping! It's working... I think.", "Meu detector de chefe está apitando! Está funcionando... eu acho."),
                (CharacterId.Joaquim, "The ground is trembling. Something big came to visit.", "O chão está tremendo. Coisa grande veio visitar."),
                (CharacterId.Maria, "Vó Edna always says: when the birds go quiet, something huge is near.", "A Vó Edna sempre diz: quando os passarinhos calam, tem coisa enorme por perto."),
                (CharacterId.Lavine, "A giant! A real giant! Can we go see? Please?", "Um gigante! Um gigante de verdade! Podemos ir ver? Por favor?"),
            });
            AddLines(entries, WorldEventMoment.BossStart, new[]
            {
                (CharacterId.Maithe, "It's huge, but huge means easy to hit. Stay behind me!", "É enorme, mas grande assim é fácil de acertar. Fiquem atrás de mim!"),
                (CharacterId.Icaro, "Big monster, big plan: we dodge, we hit, we dodge again!", "Bicho grande, plano grande: desvia, bate e desvia de novo!"),
                (CharacterId.Joaquim, "Roots ready. Maria, watch my back!", "Raízes prontas. Maria, cuida das minhas costas!"),
                (CharacterId.Maria, "Nobody gets hurt today. I've got the healing ready!", "Ninguém se machuca hoje. Estou com a cura pronta!"),
                (CharacterId.Lavine, "Wow, it has a crown! Is it the king of the bichos?", "Uau, ele tem coroa! É o rei dos bichos?"),
            });
            AddLines(entries, WorldEventMoment.BossStartNight, new[]
            {
                (CharacterId.Maithe, "At night it's even bigger. Ícaro, light the lamp!", "De noite ele fica ainda maior. Ícaro, acende a lamparina!"),
                (CharacterId.Icaro, "Glowing eyes in the dark... my sister goes first, right?", "Olhos brilhando no escuro... minha irmã vai na frente, né?"),
                (CharacterId.Joaquim, "Vô Jorge faced worse on moonlit nights. So can we.", "O Vô Jorge enfrentou coisa pior em noite de lua. A gente também consegue."),
                (CharacterId.Maria, "Stay close, everyone. In the dark, we hold hands.", "Fiquem pertinho. No escuro, a gente se dá as mãos."),
                (CharacterId.Lavine, "I'm not scared! ...Just a tiny bit. Okay, let's go!", "Eu não tô com medo! ...Só um tiquinho. Tá, vamos!"),
            });
            AddLines(entries, WorldEventMoment.BossWin, new[]
            {
                (CharacterId.Maithe, "Got it! Ícaro, tell everyone your sister did it!", "Conseguimos! Ícaro, conta pra todo mundo que foi a sua irmã!"),
                (CharacterId.Icaro, "Plan worked! I'll write it down before I forget.", "O plano funcionou! Vou anotar antes que eu esqueça."),
                (CharacterId.Joaquim, "The crown fell off. The forest is calm again.", "A coroa caiu. A mata ficou calma de novo."),
                (CharacterId.Maria, "Everyone's fine! Joaquim, you can stop flexing now.", "Todo mundo bem! Joaquim, pode parar de fazer muque."),
                (CharacterId.Lavine, "We beat the king! Vó Edna will make cake, right?", "A gente venceu o rei! A Vó Edna vai fazer bolo, né?"),
            });
            AddLines(entries, WorldEventMoment.BossLose, new[]
            {
                (CharacterId.Maithe, "It got away. Next time I'll aim better.", "Ele escapou. Da próxima vez eu miro melhor."),
                (CharacterId.Icaro, "Back to the workshop. I need a bigger invention.", "De volta pra oficina. Preciso de uma invenção maior."),
                (CharacterId.Joaquim, "A guardian learns from every fall. We'll be back.", "Guardião aprende com cada queda. A gente volta."),
                (CharacterId.Maria, "Let's rest, eat something and try again. Together.", "Vamos descansar, comer alguma coisa e tentar de novo. Juntos."),
                (CharacterId.Lavine, "It was too big today. Tomorrow I'll be bigger!", "Ele estava grande demais hoje. Amanhã eu vou estar maior!"),
            });
            AddLines(entries, WorldEventMoment.InvasionAppears, new[]
            {
                (CharacterId.Maithe, "Leather hats on the road! The Cangaço gang is back.", "Chapéu de couro na estrada! O bando do Cangaço voltou."),
                (CharacterId.Icaro, "They enchanted the bichos with hats. That's actually clever... and very rude!", "Eles enfeitiçaram os bichos com chapéu. Até que é esperto... e muito mal-educado!"),
                (CharacterId.Joaquim, "Az and his gang again. Let's protect the people there.", "O Az e o bando dele de novo. Vamos proteger o povo de lá."),
                (CharacterId.Maria, "Vô Jorge told stories about this gang. Now it's our turn.", "O Vô Jorge contava histórias desse bando. Agora é a nossa vez."),
                (CharacterId.Lavine, "If we win, there's a treasure chest! I want the +5!", "Se a gente ganhar, tem baú de tesouro! Eu quero o +5!"),
            });
            AddLines(entries, WorldEventMoment.InvasionStart, new[]
            {
                (CharacterId.Maithe, "Knock the hats off and the bichos go back to normal. Go!", "Derruba o chapéu que o bicho volta ao normal. Vai!"),
                (CharacterId.Icaro, "Wave after wave... good thing I brought snacks.", "Onda atrás de onda... ainda bem que eu trouxe merenda."),
                (CharacterId.Joaquim, "Maria, behind me. Nobody passes the roots!", "Maria, atrás de mim. Ninguém passa pelas raízes!"),
                (CharacterId.Maria, "My brother in front, me healing. Like always!", "Meu irmão na frente e eu curando. Como sempre!"),
                (CharacterId.Lavine, "Hey, gang! This region has owners: us!", "Ei, bando! Essa região tem dono: a gente!"),
            });
            AddLines(entries, WorldEventMoment.InvasionWin, new[]
            {
                (CharacterId.Maithe, "Az ran off without his hat! Let's open the chest.", "O Az fugiu sem o chapéu! Bora abrir o baú."),
                (CharacterId.Icaro, "Chest opened! I'm keeping the hinge for an invention.", "Baú aberto! A dobradiça eu guardo pra uma invenção."),
                (CharacterId.Joaquim, "The region is free. That's what a guardian does.", "A região está livre. É isso que guardião faz."),
                (CharacterId.Maria, "Look at the bichos! Without the hats they're so sweet.", "Olha os bichos! Sem o chapéu eles são tão bonzinhos."),
                (CharacterId.Lavine, "Treasure! I'll show it to Vó Edna and Vô Jorge!", "Tesouro! Vou mostrar pra Vó Edna e pro Vô Jorge!"),
            });
            AddLines(entries, WorldEventMoment.InvasionLose, new[]
            {
                (CharacterId.Maithe, "The gang was strong today. We regroup and come back.", "O bando estava forte hoje. A gente se junta e volta."),
                (CharacterId.Icaro, "Too many hats, not enough plans. I'll fix that.", "Chapéu demais e plano de menos. Vou resolver isso."),
                (CharacterId.Joaquim, "We held the line as long as we could. Next time, longer.", "Seguramos o quanto deu. Da próxima, seguramos mais."),
                (CharacterId.Maria, "Nobody got really hurt. That already is a victory.", "Ninguém se machucou de verdade. Isso já é uma vitória."),
                (CharacterId.Lavine, "Next invasion I'll bring my biggest spell!", "Na próxima invasão eu trago o meu feitiço maior!"),
            });

            return entries;
        }

        private static void AddLines(List<LocalizationEntry> entries, WorldEventMoment moment, (CharacterId hero, string en, string pt)[] lines)
        {
            foreach ((CharacterId hero, string en, string pt) in lines)
            {
                entries.Add(new LocalizationEntry(LineKey(moment, hero), en, pt));
            }
        }

        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(WorldBossTitle)))
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

        /// <summary>The boss's name in its window's form.</summary>
        public static string BossName(LocalizationManager manager, WorldBossDefinition boss, bool night) => boss != null ? Get(manager, boss.NameKey(night)) : string.Empty;

        public static string RegionIn(LocalizationManager manager, BiomeType region) => Get(manager, RegionKey(region));
    }
}
