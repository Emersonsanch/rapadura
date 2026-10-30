using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// Localization (en + pt-BR) of the mounts (phase M1): the three starters and their abilities, the HUD captions and
    /// refusals, the Estrebaria do Srosh (shop id <c>shop_haras_vila</c>) and the "Primeira Sela" mission with its little scene
    /// (Vó Edna, Srosh the Stablekeeper, Lavine naming the pony, Maithe pretending not to care). Same self-contained pattern as <c>DuoTexts</c>: merged into the live
    /// <see cref="LocalizationManager"/> on first use (keys it already has win), pt-BR fallback otherwise.
    /// </summary>
    public static class MountTexts
    {
        public const string HudMount = "ui.mount.hud.mount";
        public const string HudWhistling = "ui.mount.hud.whistling";
        public const string HudDismount = "ui.mount.hud.dismount";
        public const string HudHoldHint = "ui.mount.hud.hold_hint";
        public const string HudGallop = "ui.mount.hud.gallop";
        public const string HudTrot = "ui.mount.hud.trot";
        public const string HudAbilityReady = "ui.mount.hud.ability_ready";
        public const string HudAbilityWait = "ui.mount.hud.ability_wait";
        public const string HudStamina = "ui.mount.hud.stamina";

        public const string RefusedNoMount = "ui.mount.refused.no_mount";
        public const string RefusedTired = "ui.mount.refused.tired";
        public const string RefusedInCombat = "ui.mount.refused.in_combat";
        public const string RefusedRecall = "ui.mount.refused.recall";
        public const string RefusedNotHere = "ui.mount.refused.not_here";
        public const string RefusedArena = "ui.mount.refused.arena";
        public const string WhistleBroken = "ui.mount.whistle_broken";
        public const string TiredToast = "ui.mount.tired_toast";
        public const string GrantedToast = "ui.mount.granted_toast";
        public const string AbilityNoSecrets = "ui.mount.ability.no_secrets";
        public const string AbilitySecretsFound = "ui.mount.ability.secrets_found";
        public const string AbilityScared = "ui.mount.ability.scared";
        public const string SafeZoneNoAbility = "ui.mount.safe_zone_no_ability";
        public const string AbilityBrayMap = "ui.mount.ability.bray_map";
        public const string AbilityDashMap = "ui.mount.ability.dash_map";

        public const string HarasShopName = "city.shop.shop_haras_vila.name";

        // ---- M2: stable panel, care, codex, saddlery ----
        public const string StableButton = "ui.mount.stable.button";
        public const string StableTitle = "ui.mount.stable.title";
        public const string TabMounts = "ui.mount.stable.tab_mounts";
        public const string TabCodex = "ui.mount.stable.tab_codex";
        public const string TabSaddlery = "ui.mount.stable.tab_saddlery";
        public const string LevelLine = "ui.mount.stable.level";
        public const string LevelMax = "ui.mount.stable.level_max";

        /// <summary>Growth with the rider (Docs/Progressao500.md §6.1): {0} = cap for this hero, {1} = hero level, {2} = Brilho, {3} = max Brilho.</summary>
        public const string GrowthLine = "ui.mount.stable.growth";

        /// <summary>Saved level above the rider's cap: {0} = the level it works at.</summary>
        public const string WorksAtLine = "ui.mount.stable.works_at";

        /// <summary>Despertar reached (§6.4): {0} = I..IV.</summary>
        public const string AwakeningLine = "ui.mount.stable.awakening";
        public const string HeartsLine = "ui.mount.stable.hearts";
        public const string SatietyLine = "ui.mount.stable.satiety";
        public const string HungryTag = "ui.mount.stable.hungry";
        public const string HealthLine = "ui.mount.stable.health";
        public const string TiredLine = "ui.mount.stable.tired";
        public const string ActiveTag = "ui.mount.stable.active";
        public const string SetActive = "ui.mount.stable.set_active";
        public const string Cuddle = "ui.mount.stable.cuddle";
        public const string Feed = "ui.mount.stable.feed";
        public const string StallsLine = "ui.mount.stable.stalls";
        public const string NoHouse = "ui.mount.stable.no_house";
        public const string SaddleLine = "ui.mount.stable.saddle";
        public const string BlanketLine = "ui.mount.stable.blanket";
        public const string NoneCosmetic = "ui.mount.stable.none";
        public const string Change = "ui.mount.stable.change";
        public const string Buy = "ui.mount.stable.buy";
        public const string Owned = "ui.mount.stable.owned";
        public const string SaddleryClosed = "ui.mount.stable.saddlery_closed";
        public const string CodexCount = "ui.mount.stable.codex_count";
        public const string CodexUnknown = "ui.mount.stable.codex_unknown";
        public const string CodexSoon = "ui.mount.stable.codex_soon";
        public const string Close = "ui.mount.stable.close";
        public const string Empty = "ui.mount.stable.empty";
        public const string Fed = "ui.mount.fed";
        public const string CantEat = "ui.mount.cant_eat";
        public const string NoFood = "ui.mount.no_food";
        public const string UnknownDish = "ui.mount.unknown_dish";
        public const string LevelUp = "ui.mount.level_up";
        public const string Cuddled = "ui.mount.cuddled";
        public const string CuddleWait = "ui.mount.cuddle_wait";
        public const string Hungry = "ui.mount.hungry";
        public const string Bought = "ui.mount.bought";
        public const string NotEnoughGold = "ui.mount.not_enough_gold";

        public static string RiderLineKey(Rapadura.Gameplay.Characters.CharacterId hero) => "mount.rider_line." + hero.ToString().ToLowerInvariant();

        public const string QuestTitle = "quest.mount_first_saddle.title";
        public const string QuestDescription = "quest.mount_first_saddle.description";
        public const string QuestObjective = "quest.mount_first_saddle.objective.visit_haras";
        public const string SceneLine1 = "mount.first_saddle.line1";
        public const string SceneLine2 = "mount.first_saddle.line2";
        public const string SceneLine3 = "mount.first_saddle.line3";
        public const string SceneLine4 = "mount.first_saddle.line4";

        private static readonly LocalizationEntry[] UiEntries =
        {
            new LocalizationEntry(HudMount, "Mount", "Montaria"),
            new LocalizationEntry(HudWhistling, "Whistling…", "Assobiando…"),
            new LocalizationEntry(HudDismount, "Get off", "Descer"),
            new LocalizationEntry(HudHoldHint, "Hold to whistle", "Segure para assobiar"),
            new LocalizationEntry(HudGallop, "Gallop", "Galope"),
            new LocalizationEntry(HudTrot, "Trot (safe zone)", "Trote (zona segura)"),
            new LocalizationEntry(HudAbilityReady, "{0}", "{0}"),
            new LocalizationEntry(HudAbilityWait, "{0} {1}s", "{0} {1}s"),
            new LocalizationEntry(HudStamina, "Stamina", "Fôlego"),

            new LocalizationEntry(RefusedNoMount, "You have no mount yet. Visit Srosh's Stable!", "Vocês ainda não têm montaria. Passem na Estrebaria do Srosh!"),
            new LocalizationEntry(RefusedTired, "{0} is resting at home ({1} more fights).", "{0} está descansando em casa (mais {1} lutas)."),
            new LocalizationEntry(RefusedInCombat, "Too much commotion! Wait 5 s without fighting.", "Muita confusão! Espere 5 s sem lutar."),
            new LocalizationEntry(RefusedRecall, "Wait {0}s to whistle again.", "Espere {0}s para assobiar de novo."),
            new LocalizationEntry(RefusedNotHere, "No riding here.", "Aqui não dá para montar."),
            new LocalizationEntry(RefusedArena, "Fights start on foot. Your mount waits at the edge.", "A luta começa a pé. A montaria espera na beirada."),
            new LocalizationEntry(WhistleBroken, "The whistle broke off!", "O assobio parou no meio!"),
            new LocalizationEntry(TiredToast, "{0} got tired and trotted home to rest. Nobody got hurt!", "{0} cansou e voltou trotando para casa descansar. Ninguém se machucou!"),
            new LocalizationEntry(GrantedToast, "{0} is yours now!", "{0} agora é de vocês!"),
            new LocalizationEntry(AbilityNoSecrets, "The Alabai sniffed around… nothing here.", "O Alabai farejou tudo… nada por aqui."),
            new LocalizationEntry(AbilitySecretsFound, "The Alabai smells something hidden nearby! ({0})", "O Alabai sentiu cheiro de coisa escondida por perto! ({0})"),
            new LocalizationEntry(AbilityScared, "Hee-haw! {0} critters ran off.", "Ió-ió! {0} bichos saíram correndo."),
            new LocalizationEntry(SafeZoneNoAbility, "No tricks inside the town.", "Na cidade não tem gracinha."),
            new LocalizationEntry(AbilityBrayMap, "Hee-haw! The whole road heard the donkey.", "Ió-ió! A estrada inteira ouviu o asno."),
            new LocalizationEntry(AbilityDashMap, "Off we go!", "Lá vamos nós!"),

            new LocalizationEntry(HarasShopName, "Srosh's Stable", "Estrebaria do Srosh"),

            new LocalizationEntry(StableButton, "Mounts", "Montarias"),
            new LocalizationEntry(StableTitle, "Stable and Mount Notebook", "Estábulo e Caderno de Montarias"),
            new LocalizationEntry(TabMounts, "Our mounts", "Nossas montarias"),
            new LocalizationEntry(TabCodex, "Notebook", "Caderno"),
            new LocalizationEntry(TabSaddlery, "Saddlery", "Selaria"),
            new LocalizationEntry(LevelLine, "Level {0} · {1}/{2} XP", "Nível {0} · {1}/{2} XP"),
            new LocalizationEntry(LevelMax, "Level {0} (max)", "Nível {0} (máximo)"),
            new LocalizationEntry(GrowthLine, "Grows up to level {0} with this hero (lv {1}) · Glow {2}/{3}", "Cresce até o nível {0} com este herói (nv {1}) · Brilho {2}/{3}"),
            new LocalizationEntry(WorksAtLine, "Works at level {0} with this cousin", "Funciona no nível {0} com este primo"),
            new LocalizationEntry(AwakeningLine, "Awakening {0}", "Despertar {0}"),
            new LocalizationEntry(HeartsLine, "Affection: {0}/5", "Afeição: {0}/5"),
            new LocalizationEntry(SatietyLine, "Fed {0}%", "Saciedade {0}%"),
            new LocalizationEntry(HungryTag, "(hungry: no gallop)", "(com fome: sem galope)"),
            new LocalizationEntry(HealthLine, "Health {0}/{1}", "Vida {0}/{1}"),
            new LocalizationEntry(TiredLine, "Resting: {0} more fights", "Descansando: mais {0} lutas"),
            new LocalizationEntry(ActiveTag, "Active mount", "Montaria ativa"),
            new LocalizationEntry(SetActive, "Ride this one", "Usar esta"),
            new LocalizationEntry(Cuddle, "Pet", "Fazer carinho"),
            new LocalizationEntry(Feed, "Give {0}", "Dar {0}"),
            new LocalizationEntry(StallsLine, "Stalls at home: {0} · the rest stay at Srosh's Stable", "Baias na casa: {0} · as outras ficam na Estrebaria"),
            new LocalizationEntry(NoHouse, "No house yet: they all stay at Srosh's Stable.", "Sem casa ainda: todas ficam na Estrebaria do Srosh."),
            new LocalizationEntry(SaddleLine, "Saddle: {0}", "Sela: {0}"),
            new LocalizationEntry(BlanketLine, "Blanket: {0}", "Manta: {0}"),
            new LocalizationEntry(NoneCosmetic, "plain", "simples"),
            new LocalizationEntry(Change, "Change", "Trocar"),
            new LocalizationEntry(Buy, "Buy ({0} gold)", "Comprar ({0} ouro)"),
            new LocalizationEntry(Owned, "Yours", "Já é sua"),
            new LocalizationEntry(SaddleryClosed, "The saddlery is at Srosh's Stable.", "A Selaria fica na Estrebaria do Srosh."),
            new LocalizationEntry(CodexCount, "{0}/{1} mounts", "{0}/{1} montarias"),
            new LocalizationEntry(CodexUnknown, "???", "???"),
            new LocalizationEntry(CodexSoon, "(coming in a later update)", "(chega numa próxima versão)"),
            new LocalizationEntry(Close, "Close", "Fechar"),
            new LocalizationEntry(Empty, "No mount yet.", "Nenhuma montaria ainda."),
            new LocalizationEntry(Fed, "{0} ate {1}! (+{2} fed)", "{0} comeu {1}! (+{2} de saciedade)"),
            new LocalizationEntry(CantEat, "{0} won't eat that.", "{0} não come isso."),
            new LocalizationEntry(NoFood, "Not enough in the backpack for {0}.", "Falta coisa na mochila para {0}."),
            new LocalizationEntry(UnknownDish, "Vó Edna hasn't taught {0} yet.", "A Vó Edna ainda não ensinou {0}."),
            new LocalizationEntry(LevelUp, "{0} reached level {1}!", "{0} subiu para o nível {1}!"),
            new LocalizationEntry(Cuddled, "{0} loved the petting!", "{0} adorou o carinho!"),
            new LocalizationEntry(CuddleWait, "Let {0} breathe a little.", "Deixa {0} respirar um pouquinho."),
            new LocalizationEntry(Hungry, "{0} is hungry: no gallop until it eats.", "{0} está com fome: sem galope até comer."),
            new LocalizationEntry(Bought, "Bought {0}!", "Comprou {0}!"),
            new LocalizationEntry(NotEnoughGold, "Not enough gold.", "O ouro não dá."),

            new LocalizationEntry("mount.rider_line.maithe", "Maithe: Come on, let's see how fast you are.", "Maithe: Bora, quero ver se você é rápido mesmo."),
            new LocalizationEntry("mount.rider_line.maria", "Maria: Easy now. We take care of each other.", "Maria: Calma. A gente cuida um do outro."),
            new LocalizationEntry("mount.rider_line.joaquim", "Joaquim: Steady, friend. Nobody falls behind.", "Joaquim: Firme, amigo. Ninguém fica pra trás."),
            new LocalizationEntry("mount.rider_line.icaro", "Ícaro: Engine on! Well… legs on. See? Science.", "Ícaro: Motor ligado! Quer dizer… pernas ligadas. Viu? Ciência."),
            new LocalizationEntry("mount.rider_line.lavine", "Lavine: Let's go, let's go, let's gooo!", "Lavine: Vamos, vamos, vamooos!"),

            new LocalizationEntry("mount.food.capim.name", "a hay bundle (2 fibre)", "feixe de capim (2 fibras)"),
            new LocalizationEntry("mount.food.racao.name", "stable feed", "Ração da Estrebaria"),
            new LocalizationEntry("mount.food.erva.name", "fresh herb", "erva fresca"),
            new LocalizationEntry("mount.food.carne.name", "roast meat", "carne assada"),
            new LocalizationEntry("mount.food.cuscuz.name", "tandir bread", "nan do tandir"),
            new LocalizationEntry("mount.food.rapadura.name", "golden brick", "torrão dourado"),
            new LocalizationEntry("mount.food.baiao.name", "Grandma's plov", "plov da Vó"),

            new LocalizationEntry("mount.cosmetic.saddle_tropeiro.name", "Caravan rawhide saddle", "Sela de couro cru de caravana"),
            new LocalizationEntry("mount.cosmetic.saddle_chita.name", "Embroidered suzani saddle", "Sela bordada de suzani"),
            new LocalizationEntry("mount.cosmetic.saddle_vaqueiro_estrelas.name", "Steppe saddle with stars", "Sela da estepe com estrelas"),
            new LocalizationEntry("mount.cosmetic.blanket_chita_florida.name", "Flowery silk blanket", "Manta de seda florida"),
            new LocalizationEntry("mount.cosmetic.blanket_xadrez_quadrilha.name", "Litema striped blanket", "Manta listrada de Litema"),
            new LocalizationEntry("mount.cosmetic.blanket_la_pico.name", "Kaggen Range wool blanket", "Manta de lã da Serra de Kaggen"),

            new LocalizationEntry(QuestTitle, "First Saddle", "Primeira Sela"),
            new LocalizationEntry(QuestDescription,
                "After Kök-Böri, Vó Edna wants to take the cousins to the stable of Srosh the Stablekeeper, Vô Jorge's old road friend.",
                "Depois do Kök-Böri, a Vó Edna quer levar os primos à estrebaria do Srosh, o Tratador, velho amigo de estrada do Vô Jorge."),
            new LocalizationEntry(QuestObjective, "Visit Srosh's Stable", "Visitar a Estrebaria do Srosh"),
            new LocalizationEntry(SceneLine1, "Vó Edna: Srosh kept this one for you five. Vô Jorge would have loved it.",
                "Vó Edna: O Srosh guardou esse aqui pra vocês cinco. O Vô Jorge ia adorar."),
            new LocalizationEntry(SceneLine2, "Lavine: Look at his white feet! His name is Socks!",
                "Lavine: Olha os pezinhos brancos dele! O nome dele é Meia-Branca!"),
            new LocalizationEntry(SceneLine3, "Maithe: Whatever. A pony is a pony… Can I ride him first?",
                "Maithe: Tanto faz. Pônei é pônei… Posso montar primeiro?"),
            new LocalizationEntry(SceneLine4, "Hold the Mount button (H) for a moment to whistle for him.",
                "Segure o botão Montaria (H) um pouquinho para assobiar e chamar."),
        };

        // id → (en name, pt name, en desc, pt desc, en source, pt source)
        private static readonly (string Id, string NameEn, string NamePt, string DescEn, string DescPt, string SourceEn, string SourcePt)[] Mounts =
        {
            ("mount_pony", "Steppe Pony", "Pônei das Estepes",
                "A small, steady bay pony of the steppes, with white socks. Lavine named him Socks.",
                "Um pônei baio das estepes, pequeno e firme, de meias brancas. Foi a Lavine que deu o nome: Meia-Branca.",
                "Gift from Vó Edna (First Saddle) or Srosh's Stable.", "Presente da Vó Edna (Primeira Sela) ou na Estrebaria do Srosh."),
            ("mount_big_dog", "Fluffy Alabai", "Alabai Fofão",
                "A big, fluffy alabai, the shepherd dog of the caravans, and Ari's big nephew. Fast, cheerful, and Ari still bosses him around.",
                "Um alabai grandão e fofo, o cão pastor das caravanas, sobrinho do Ari. Rápido, alegre, e o Ari ainda manda nele.",
                "Srosh's Stable.", "Estrebaria do Srosh."),
            ("mount_pack_donkey", "Caravan Donkey", "Asno de Caravana",
                "A sturdy grey caravan donkey with a bell on its neck. Slow, but nothing tires him out.",
                "Um asno de caravana cinza e parrudo, de sininho no pescoço. Devagar, mas nada cansa ele.",
                "Srosh's Stable (later: Qocho Caravanserai).", "Estrebaria do Srosh (depois: Caravançará de Qocho)."),
        };

        private static readonly (string Id, string NameEn, string NamePt, string DescEn, string DescPt)[] Abilities =
        {
            ("arrancada", "Sprint Off", "Arrancada", "Bursts 6 m forward in 0.3 s.", "Dispara 6 m para a frente em 0,3 s."),
            ("faro_de_tesouro", "Treasure Nose", "Faro de Tesouro",
                "Sniffs out hidden things up to 20 m away for 10 s.", "Fareja coisas escondidas a até 20 m por 10 s."),
            ("zurro_espanta_bicho", "Critter-Scaring Bray", "Zurro Espanta-Bicho",
                "Common critters within 6 m run away for 3 s (bosses ignore it).", "Bichos comuns a até 6 m fogem por 3 s (chefe ignora)."),
        };

        public static readonly List<LocalizationEntry> Entries = BuildEntries();

        private static List<LocalizationEntry> BuildEntries()
        {
            var entries = new List<LocalizationEntry>(UiEntries);
            entries.AddRange(MountCodex.TextEntries());
            entries.AddRange(MountWaveTexts.Entries); // M3 wave 1: the 18 walkers, their abilities, whistles, drops and ribbons.
            foreach (var m in Mounts)
            {
                entries.Add(new LocalizationEntry("mount." + m.Id + ".name", m.NameEn, m.NamePt));
                entries.Add(new LocalizationEntry("mount." + m.Id + ".desc", m.DescEn, m.DescPt));
                entries.Add(new LocalizationEntry("mount." + m.Id + ".source", m.SourceEn, m.SourcePt));
            }

            // The Haras's items (Resources/Items): pt-BR = the asset text, like RefineTexts / TachoTexts do for theirs.
            entries.Add(new LocalizationEntry(Content.ContentText.ItemNameKey(MountCareRules.FeedItemId), "Stable Feed", "Ração da Estrebaria"));
            entries.Add(new LocalizationEntry(Content.ContentText.ItemDescriptionKey(MountCareRules.FeedItemId),
                "Srosh's barley-and-grass feed. Every mount eats it: +40 fullness.", "Ração de cevada e capim do Srosh. Toda montaria come: +40 de saciedade."));
            AddWhistle(entries, MountCatalog.PonyWhistle, "Steppe Pony Whistle", "Apito do Pônei das Estepes", "the Steppe Pony", "o Pônei das Estepes");
            AddWhistle(entries, MountCatalog.BigDogWhistle, "Fluffy Alabai Whistle", "Apito do Alabai Fofão", "the Fluffy Alabai", "o Alabai Fofão");
            AddWhistle(entries, MountCatalog.PackDonkeyWhistle, "Caravan Donkey Whistle", "Apito do Asno de Caravana", "the Caravan Donkey", "o Asno de Caravana");

            entries.Add(new LocalizationEntry("mount.mount_pony.favorite_line", "Socks, run, Maithe is watching!", "Meia-Branca, corre que a Maithe tá olhando!"));
            foreach (var a in Abilities)
            {
                entries.Add(new LocalizationEntry("mount.ability." + a.Id + ".name", a.NameEn, a.NamePt));
                entries.Add(new LocalizationEntry("mount.ability." + a.Id + ".desc", a.DescEn, a.DescPt));
            }

            return entries;
        }

        private static void AddWhistle(List<LocalizationEntry> entries, string itemId, string nameEn, string namePt, string mountEn, string mountPt)
        {
            entries.Add(new LocalizationEntry(Content.ContentText.ItemNameKey(itemId), nameEn, namePt));
            entries.Add(new LocalizationEntry(Content.ContentText.ItemDescriptionKey(itemId),
                $"Reed whistle from Srosh's Stable. Whoever buys it takes {mountEn} home.",
                $"Apito de junco da Estrebaria do Srosh. Quem compra leva {mountPt} para casa."));
        }

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(HudMount)))
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
                var map = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    map[entry.Key] = entry.PtBR;
                }

                _fallbackByKey = map;
            }

            return key != null && _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }

        public static string MountName(LocalizationManager manager, MountDefinition definition)
        {
            return definition != null ? Get(manager, definition.DisplayNameKey) : string.Empty;
        }

        public static string AbilityName(LocalizationManager manager, MountDefinition definition)
        {
            return definition != null && definition.Ability != null ? Get(manager, definition.Ability.NameKey) : string.Empty;
        }
    }
}
