using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// Localization (en + pt-BR) of the mounts (phase M1): the three starters and their abilities, the HUD captions and
    /// refusals, the Haras da Vila and the "Primeira Sela" mission with its little scene (Vó Zefa, Seu Zé, Lavine naming
    /// the pony, Maithe pretending not to care). Same self-contained pattern as <c>DuoTexts</c>: merged into the live
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

            new LocalizationEntry(RefusedNoMount, "You have no mount yet. Visit the Haras da Vila!", "Vocês ainda não têm montaria. Passem no Haras da Vila!"),
            new LocalizationEntry(RefusedTired, "{0} is resting at home ({1} more fights).", "{0} está descansando em casa (mais {1} lutas)."),
            new LocalizationEntry(RefusedInCombat, "Too much commotion! Wait 5 s without fighting.", "Muita confusão! Espere 5 s sem lutar."),
            new LocalizationEntry(RefusedRecall, "Wait {0}s to whistle again.", "Espere {0}s para assobiar de novo."),
            new LocalizationEntry(RefusedNotHere, "No riding here.", "Aqui não dá para montar."),
            new LocalizationEntry(RefusedArena, "Fights start on foot. Your mount waits at the edge.", "A luta começa a pé. A montaria espera na beirada."),
            new LocalizationEntry(WhistleBroken, "The whistle broke off!", "O assobio parou no meio!"),
            new LocalizationEntry(TiredToast, "{0} got tired and trotted home to rest. Nobody got hurt!", "{0} cansou e voltou trotando para casa descansar. Ninguém se machucou!"),
            new LocalizationEntry(GrantedToast, "{0} is yours now!", "{0} agora é de vocês!"),
            new LocalizationEntry(AbilityNoSecrets, "Paçoca sniffed around… nothing here.", "O Paçoca farejou tudo… nada por aqui."),
            new LocalizationEntry(AbilitySecretsFound, "Paçoca smells something hidden nearby! ({0})", "O Paçoca sentiu cheiro de coisa escondida por perto! ({0})"),
            new LocalizationEntry(AbilityScared, "Hee-haw! {0} critters ran off.", "Ió-ió! {0} bichos saíram correndo."),
            new LocalizationEntry(SafeZoneNoAbility, "No tricks inside the town.", "Na cidade não tem gracinha."),
            new LocalizationEntry(AbilityBrayMap, "Hee-haw! The whole road heard the donkey.", "Ió-ió! A estrada inteira ouviu o jegue."),
            new LocalizationEntry(AbilityDashMap, "Off we go!", "Lá vamos nós!"),

            new LocalizationEntry(HarasShopName, "Haras da Vila (Seu Zé)", "Haras da Vila (Seu Zé)"),

            new LocalizationEntry(StableButton, "Mounts", "Montarias"),
            new LocalizationEntry(StableTitle, "Stable and Mount Notebook", "Estábulo e Caderno de Montarias"),
            new LocalizationEntry(TabMounts, "Our mounts", "Nossas montarias"),
            new LocalizationEntry(TabCodex, "Notebook", "Caderno"),
            new LocalizationEntry(TabSaddlery, "Saddlery", "Selaria"),
            new LocalizationEntry(LevelLine, "Level {0} · {1}/{2} XP", "Nível {0} · {1}/{2} XP"),
            new LocalizationEntry(LevelMax, "Level {0} (max)", "Nível {0} (máximo)"),
            new LocalizationEntry(HeartsLine, "Affection: {0}/5", "Afeição: {0}/5"),
            new LocalizationEntry(SatietyLine, "Fed {0}%", "Saciedade {0}%"),
            new LocalizationEntry(HungryTag, "(hungry: no gallop)", "(com fome: sem galope)"),
            new LocalizationEntry(HealthLine, "Health {0}/{1}", "Vida {0}/{1}"),
            new LocalizationEntry(TiredLine, "Resting: {0} more fights", "Descansando: mais {0} lutas"),
            new LocalizationEntry(ActiveTag, "Active mount", "Montaria ativa"),
            new LocalizationEntry(SetActive, "Ride this one", "Usar esta"),
            new LocalizationEntry(Cuddle, "Pet", "Fazer carinho"),
            new LocalizationEntry(Feed, "Give {0}", "Dar {0}"),
            new LocalizationEntry(StallsLine, "Stalls at home: {0} · the rest stay at the Haras", "Baias na casa: {0} · as outras ficam no Haras"),
            new LocalizationEntry(NoHouse, "No house yet: they all stay at Seu Zé's Haras.", "Sem casa ainda: todas ficam no Haras do Seu Zé."),
            new LocalizationEntry(SaddleLine, "Saddle: {0}", "Sela: {0}"),
            new LocalizationEntry(BlanketLine, "Blanket: {0}", "Manta: {0}"),
            new LocalizationEntry(NoneCosmetic, "plain", "simples"),
            new LocalizationEntry(Change, "Change", "Trocar"),
            new LocalizationEntry(Buy, "Buy ({0} gold)", "Comprar ({0} ouro)"),
            new LocalizationEntry(Owned, "Yours", "Já é sua"),
            new LocalizationEntry(SaddleryClosed, "The saddlery is at the Haras da Vila.", "A Selaria fica no Haras da Vila."),
            new LocalizationEntry(CodexCount, "{0}/{1} mounts", "{0}/{1} montarias"),
            new LocalizationEntry(CodexUnknown, "???", "???"),
            new LocalizationEntry(CodexSoon, "(coming in a later update)", "(chega numa próxima versão)"),
            new LocalizationEntry(Close, "Close", "Fechar"),
            new LocalizationEntry(Empty, "No mount yet.", "Nenhuma montaria ainda."),
            new LocalizationEntry(Fed, "{0} ate {1}! (+{2} fed)", "{0} comeu {1}! (+{2} de saciedade)"),
            new LocalizationEntry(CantEat, "{0} won't eat that.", "{0} não come isso."),
            new LocalizationEntry(NoFood, "Not enough in the backpack for {0}.", "Falta coisa na mochila para {0}."),
            new LocalizationEntry(UnknownDish, "Vó Zefa hasn't taught {0} yet.", "A Vó Zefa ainda não ensinou {0}."),
            new LocalizationEntry(LevelUp, "{0} reached level {1}!", "{0} subiu para o nível {1}!"),
            new LocalizationEntry(Cuddled, "{0} loved the petting!", "{0} adorou o carinho!"),
            new LocalizationEntry(CuddleWait, "Let {0} breathe a little.", "Deixa {0} respirar um pouquinho."),
            new LocalizationEntry(Hungry, "{0} is hungry: no gallop until it eats.", "{0} está com fome: sem galope até comer."),
            new LocalizationEntry(Bought, "Bought {0}!", "Comprou {0}!"),
            new LocalizationEntry(NotEnoughGold, "Not enough gold.", "O ouro não dá."),

            new LocalizationEntry("mount.rider_line.maithe", "Maithe: Come on, let's see how fast you are.", "Maithe: Bora, quero ver se você é rápido mesmo."),
            new LocalizationEntry("mount.rider_line.maria", "Maria: Easy now. We take care of each other.", "Maria: Calma. A gente cuida um do outro."),
            new LocalizationEntry("mount.rider_line.joaquim", "Joaquim: Steady, friend. Nobody falls behind.", "Joaquim: Firme, amigo. Ninguém fica pra trás."),
            new LocalizationEntry("mount.rider_line.icaro", "Ícaro: Engine on! Well… legs on.", "Ícaro: Motor ligado! Quer dizer… pernas ligadas."),
            new LocalizationEntry("mount.rider_line.lavine", "Lavine: Let's go, let's go, let's gooo!", "Lavine: Vamos, vamos, vamooos!"),

            new LocalizationEntry("mount.food.capim.name", "a hay bundle (2 fibre)", "feixe de capim (2 fibras)"),
            new LocalizationEntry("mount.food.racao.name", "Haras feed", "Ração do Haras"),
            new LocalizationEntry("mount.food.erva.name", "fresh herb", "erva fresca"),
            new LocalizationEntry("mount.food.carne.name", "roast meat", "carne assada"),
            new LocalizationEntry("mount.food.cuscuz.name", "Vó's cuscuz", "cuscuz da Vó"),
            new LocalizationEntry("mount.food.rapadura.name", "rapadura", "rapadura"),
            new LocalizationEntry("mount.food.baiao.name", "baião de dois", "baião de dois"),

            new LocalizationEntry("mount.cosmetic.saddle_tropeiro.name", "Drover's rawhide saddle", "Sela de couro cru do tropeiro"),
            new LocalizationEntry("mount.cosmetic.saddle_chita.name", "Embroidered chita saddle", "Sela bordada de chita"),
            new LocalizationEntry("mount.cosmetic.saddle_vaqueiro_estrelas.name", "Cowboy saddle with stars", "Sela de vaqueiro com estrelas"),
            new LocalizationEntry("mount.cosmetic.blanket_chita_florida.name", "Flowery chita blanket", "Manta de chita florida"),
            new LocalizationEntry("mount.cosmetic.blanket_xadrez_quadrilha.name", "Quadrilha plaid blanket", "Manta xadrez de quadrilha"),
            new LocalizationEntry("mount.cosmetic.blanket_la_pico.name", "Pico wool blanket", "Manta de lã do Pico Nevado"),

            new LocalizationEntry(QuestTitle, "First Saddle", "Primeira Sela"),
            new LocalizationEntry(QuestDescription,
                "After the Lobo Alfa, Vó Zefa wants to take the cousins to the Haras of Seu Zé, Vô Chico's old road friend.",
                "Depois do Lobo Alfa, a Vó Zefa quer levar os primos ao Haras do Seu Zé, velho amigo de estrada do Vô Chico."),
            new LocalizationEntry(QuestObjective, "Visit the Haras da Vila", "Visitar o Haras da Vila"),
            new LocalizationEntry(SceneLine1, "Vó Zefa: Seu Zé kept this one for you five. Vô Chico would have loved it.",
                "Vó Zefa: O Seu Zé guardou esse aqui pra vocês cinco. O Vô Chico ia adorar."),
            new LocalizationEntry(SceneLine2, "Lavine: Look at his white feet! His name is Pé-de-Pano!",
                "Lavine: Olha os pezinhos brancos dele! O nome dele é Pé-de-Pano!"),
            new LocalizationEntry(SceneLine3, "Maithe: Whatever. A pony is a pony… Can I ride him first?",
                "Maithe: Tanto faz. Pônei é pônei… Posso montar primeiro?"),
            new LocalizationEntry(SceneLine4, "Hold the Mount button (H) for a moment to whistle for him.",
                "Segure o botão Montaria (H) um pouquinho para assobiar e chamar."),
        };

        // id → (en name, pt name, en desc, pt desc, en source, pt source)
        private static readonly (string Id, string NameEn, string NamePt, string DescEn, string DescPt, string SourceEn, string SourcePt)[] Mounts =
        {
            ("mount_pony", "Pé-de-Pano Pony", "Pônei Pé-de-Pano",
                "A small, steady bay pony with white socks. Lavine named him.", "Um pônei baio pequeno e firme, de meias brancas. Foi a Lavine que deu o nome.",
                "Gift from Vó Zefa (First Saddle) or Haras da Vila.", "Presente da Vó Zefa (Primeira Sela) ou no Haras da Vila."),
            ("mount_big_dog", "Paçoca the Big Dog", "Cachorrão Paçoca",
                "Caramelo's big nephew. Fast, cheerful, and Caramelo still bosses him around.",
                "O sobrinho grandalhão do Caramelo. Rápido, alegre, e o Caramelo ainda manda nele.",
                "Haras da Vila.", "Haras da Vila."),
            ("mount_pack_donkey", "Pack Donkey", "Jegue Carregador",
                "A sturdy grey donkey with a cowbell. Slow, but nothing tires him out.",
                "Um jegue cinza parrudo, de chocalho no pescoço. Devagar, mas nada cansa ele.",
                "Haras da Vila (later: Feira de Bichos do Oásis).", "Haras da Vila (depois: Feira de Bichos do Oásis)."),
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
            foreach (var m in Mounts)
            {
                entries.Add(new LocalizationEntry("mount." + m.Id + ".name", m.NameEn, m.NamePt));
                entries.Add(new LocalizationEntry("mount." + m.Id + ".desc", m.DescEn, m.DescPt));
                entries.Add(new LocalizationEntry("mount." + m.Id + ".source", m.SourceEn, m.SourcePt));
            }

            // The Haras's items (Resources/Items): pt-BR = the asset text, like RefineTexts / TachoTexts do for theirs.
            entries.Add(new LocalizationEntry(Content.ContentText.ItemNameKey(MountCareRules.FeedItemId), "Haras Feed", "Ração do Haras"));
            entries.Add(new LocalizationEntry(Content.ContentText.ItemDescriptionKey(MountCareRules.FeedItemId),
                "Seu Zé's corn-and-grass feed. Every mount eats it: +40 fullness.", "Ração de milho e capim do Seu Zé. Toda montaria come: +40 de saciedade."));
            AddWhistle(entries, MountCatalog.PonyWhistle, "Pé-de-Pano Pony Whistle", "Apito do Pônei Pé-de-Pano", "Pé-de-Pano Pony", "Pônei Pé-de-Pano");
            AddWhistle(entries, MountCatalog.BigDogWhistle, "Paçoca the Big Dog Whistle", "Apito do Cachorrão Paçoca", "Paçoca the Big Dog", "Cachorrão Paçoca");
            AddWhistle(entries, MountCatalog.PackDonkeyWhistle, "Pack Donkey Whistle", "Apito do Jegue Carregador", "the Pack Donkey", "Jegue Carregador");

            entries.Add(new LocalizationEntry("mount.mount_pony.favorite_line", "Pé-de-Pano, run, Maithe is watching!", "Pé-de-Pano, corre que a Maithe tá olhando!"));
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
                $"Bamboo whistle from Seu Zé's Haras. Whoever buys it takes {mountEn} home.",
                $"Apito de taquara do Haras do Seu Zé. Quem compra leva o {mountPt} para casa."));
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
