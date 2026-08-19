using System.Collections.Generic;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Logging;

namespace Rapadura.Core.Localization
{
    /// <summary>
    /// Top-level manager for text localization. Follows the same pattern as
    /// <see cref="Rapadura.Save.SaveManager"/> / <see cref="Rapadura.Gameplay.World.CheckpointManager"/>:
    /// plain C# class implementing <see cref="IManager"/>, constructed and registered with the
    /// <see cref="Rapadura.Core.DI.ServiceLocator"/> by <c>GameManager.BuildManagers()</c>.
    ///
    /// NOTE: this class is intentionally NOT wired into GameManager.cs by this change (see
    /// TODO.md task instructions) — a follow-up edit needs to add:
    ///   LocalizationManager = new LocalizationManager();
    ///   RegisterManager(LocalizationManager);
    /// inside <c>GameManager.BuildManagers()</c>, plus a public property, mirroring SaveManager.
    ///
    /// Text is fully externalized: entries come from a <see cref="LocalizationTable"/>
    /// ScriptableObject and/or a CSV export (see <see cref="LocalizationCsv"/>) rather than
    /// being hardcoded in call sites. Lookup always falls back to English when the current
    /// language is missing a translation for a key, and to the key itself (wrapped in
    /// brackets) when the key doesn't exist at all, so missing text is obvious instead of
    /// silently blank in builds.
    /// </summary>
    public class LocalizationManager : IManager
    {
        private const string LogCategory = "Localization";

        private readonly Dictionary<string, LocalizationEntry> _entries = new Dictionary<string, LocalizationEntry>();

        public LanguageCode CurrentLanguage { get; private set; } = LanguageCode.en;

        /// <summary>Raised after <see cref="SetLanguage"/> changes the active language, so UI can refresh its text.</summary>
        public event System.Action<LanguageCode> LanguageChanged;

        public void Initialize()
        {
            if (_entries.Count == 0)
            {
                LoadEntries(DefaultEntries.All);
            }

            GameLogger.Info(LogCategory, $"LocalizationManager initialized with {_entries.Count} keys, language={CurrentLanguage}.");
        }

        public void Shutdown()
        {
            _entries.Clear();
        }

        /// <summary>Loads/merges entries from a table or CSV parse result. Later calls overwrite matching keys,
        /// so this can be called again to hot-reload a translation update.</summary>
        public void LoadEntries(IEnumerable<LocalizationEntry> entries)
        {
            foreach (LocalizationEntry entry in entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.Key))
                {
                    continue;
                }

                _entries[entry.Key] = entry;
            }
        }

        /// <summary>Loads entries straight from a <see cref="LocalizationTable"/> ScriptableObject.</summary>
        public void LoadFromTable(LocalizationTable table)
        {
            if (table == null)
            {
                GameLogger.Warning(LogCategory, "LoadFromTable called with a null table.");
                return;
            }

            LoadEntries(table.Entries);
        }

        public void SetLanguage(LanguageCode language)
        {
            if (CurrentLanguage == language)
            {
                return;
            }

            CurrentLanguage = language;
            GameLogger.Info(LogCategory, $"Language changed to {language}.");
            LanguageChanged?.Invoke(language);
        }

        /// <summary>
        /// Returns the localized text for <paramref name="key"/> in the current language.
        /// Falls back to English if the key exists but has no translation for the current
        /// language, and to "[key]" if the key isn't registered at all (so missing strings
        /// are visibly obvious in-game rather than blank).
        /// </summary>
        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            if (!_entries.TryGetValue(key, out LocalizationEntry entry))
            {
                GameLogger.Warning(LogCategory, $"Missing localization key: '{key}'.");
                return $"[{key}]";
            }

            string text = entry.GetText(CurrentLanguage);
            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }

            if (CurrentLanguage != LanguageCode.en)
            {
                string fallback = entry.GetText(LanguageCode.en);
                if (!string.IsNullOrEmpty(fallback))
                {
                    GameLogger.Debug(LogCategory, $"Key '{key}' missing for {CurrentLanguage}, used en fallback.");
                    return fallback;
                }
            }

            GameLogger.Warning(LogCategory, $"Key '{key}' has no text for {CurrentLanguage} or en fallback.");
            return $"[{key}]";
        }

        public bool HasKey(string key)
        {
            return !string.IsNullOrEmpty(key) && _entries.ContainsKey(key);
        }

        public int KeyCount => _entries.Count;

        /// <summary>
        /// Small built-in seed set so the localization system is usable end to end before any
        /// external CSV/table asset is authored. Real content should come from
        /// StreamingAssets/Resources CSV or a LocalizationTable asset via
        /// <see cref="LoadEntries"/>/<see cref="LoadFromTable"/> — this is test/bootstrap data only.
        /// </summary>
        public static class DefaultEntries
        {
            public static readonly List<LocalizationEntry> All = new List<LocalizationEntry>
            {
                new LocalizationEntry("ui.pause.title", "Paused", "Pausado"),
                new LocalizationEntry("ui.pause.resume", "Resume", "Continuar"),
                new LocalizationEntry("ui.menu.start", "Start Game", "Iniciar Jogo"),
                new LocalizationEntry("ui.menu.settings", "Settings", "Configurações"),
                new LocalizationEntry("ui.menu.quit", "Quit", "Sair"),

                // Dialogue content for the example dialogues generated by DialogueSeeder
                // (Assets/Scripts/Editor/DialogueSeeder.cs) — that seeder deliberately does not
                // touch this file (out of scope for its own task), so these keys used to render
                // as "[key]" until authored here.
                new LocalizationEntry("dialogue.villager.name", "Villager", "Aldeão"),
                new LocalizationEntry("dialogue.villager.greeting", "Welcome, traveler! Haven't seen you around here before.", "Bem-vindo, viajante! Nunca te vi por aqui antes."),
                new LocalizationEntry("dialogue.villager.about_village", "This village has stood for generations, quiet and peaceful, ever since the old war ended.", "Esta vila existe há gerações, quieta e pacífica, desde o fim da velha guerra."),
                new LocalizationEntry("dialogue.villager.farewell", "Safe travels, friend.", "Boa viagem, amigo."),
                new LocalizationEntry("dialogue.villager.choice_ask_about_village", "Tell me about this village.", "Fale-me sobre esta vila."),
                new LocalizationEntry("dialogue.common.choice_goodbye", "Goodbye.", "Adeus."),
                new LocalizationEntry("dialogue.blacksmith.name", "Blacksmith", "Ferreiro"),
                new LocalizationEntry("dialogue.blacksmith.offer", "Need something forged, or are you here to browse my wares?", "Precisa de algo forjado, ou veio dar uma olhada nas minhas mercadorias?"),
                new LocalizationEntry("dialogue.blacksmith.choice_ask_quest", "Do you have any work for me?", "Você tem algum trabalho para mim?"),
                new LocalizationEntry("dialogue.blacksmith.choice_ask_shop", "Show me what you're selling.", "Mostre-me o que você está vendendo."),
                new LocalizationEntry("dialogue.blacksmith.quest_intro", "As a matter of fact, I do. Find me later and I'll explain what I need.", "Por acaso, tenho sim. Procure-me depois e eu explico o que preciso."),
                new LocalizationEntry("dialogue.blacksmith.shop_redirect", "Take a look at the counter — everything there is for sale.", "Dê uma olhada no balcão — tudo ali está à venda."),

                // Shop display names (ShopSeeder.cs / ShopDefinition.cs default, same out-of-scope
                // note as the dialogue keys above).
                new LocalizationEntry("shop.general_store.name", "General Store", "Loja Geral"),
                new LocalizationEntry("shop.new.name", "New Shop", "Nova Loja"),

                // Character-story content for CharacterQuestSeeder.cs / CharacterDialogueSeeder.cs
                // (Assets/Scripts/Editor/CharacterQuestSeeder.cs, CharacterDialogueSeeder.cs) —
                // Joaquim, Maithe and Lavine personal quests/dialogues, see TODO.md's roster section
                // for the source lore each of these expands on.

                // -- Joaquim: quest_joaquim_echoes_of_home ("Ecos da Vila Perdida") --
                new LocalizationEntry("quest.joaquim_echoes.title", "Echoes of Home", "Ecos da Vila Perdida"),
                new LocalizationEntry("quest.joaquim_echoes.description", "Dark creatures razed Joaquim's village long ago. The same breed now nests in its ruins — he means to clear them out, recover what he can of what was lost, and find out if anyone survived.", "Criaturas sombrias destruíram a vila de Joaquim há muito tempo. A mesma laia agora se esconde nas ruínas — ele pretende expulsá-las, recuperar o que restou do que foi perdido e descobrir se alguém sobreviveu."),
                new LocalizationEntry("quest.joaquim_echoes.obj_clear_ruin_dwellers", "Clear the creatures nesting in the village ruins (0/6)", "Expulse as criaturas que se escondem nas ruínas da vila (0/6)"),
                new LocalizationEntry("quest.joaquim_echoes.obj_recover_relic_blade", "Recover the old guardian's blade from the wreckage", "Recupere a lâmina do antigo guardião entre os escombros"),
                new LocalizationEntry("quest.joaquim_echoes.obj_talk_to_survivor", "Speak with the survivor near the ruins", "Fale com a sobrevivente perto das ruínas"),

                // -- Maithe: quest_maithe_windtrail ("Rastros do Vento") --
                new LocalizationEntry("quest.maithe_windtrail.title", "Windtrail", "Rastros do Vento"),
                new LocalizationEntry("quest.maithe_windtrail.description", "The Olho do Vento lets Maithe see threats no one else can. Stone constructs in the mountains bear a mark she's chased for years — proof someone, or something, built the monsters plaguing the realms.", "O Olho do Vento permite que Maithe enxergue ameaças que ninguém mais vê. Constructos de pedra nas montanhas carregam uma marca que ela persegue há anos — prova de que alguém, ou algo, criou os monstros que assolam os reinos."),
                new LocalizationEntry("quest.maithe_windtrail.obj_fell_stone_golems", "Fell the marked stone golems (0/4)", "Derrote os golens de pedra marcados (0/4)"),
                new LocalizationEntry("quest.maithe_windtrail.obj_gather_marked_ore", "Gather ore samples etched with the strange mark (0/3)", "Reúna amostras de minério gravadas com a marca estranha (0/3)"),
                new LocalizationEntry("quest.maithe_windtrail.obj_decode_glyphs", "Use the Olho do Vento to decode the glyphs", "Use o Olho do Vento para decifrar os símbolos"),

                // -- Lavine: quest_lavine_redtide ("A Maré Rubra") --
                new LocalizationEntry("quest.lavine_redtide.title", "The Red Tide", "A Maré Rubra"),
                new LocalizationEntry("quest.lavine_redtide.description", "Born under the Noite Rubra, Lavine carries a corruption that never fully sleeps. It flares here, drawing corrupted creatures to her — she must master it before it masters her, then seek out someone who understands the eclipse that made her what she is.", "Nascida sob a Noite Rubra, Lavine carrega uma corrupção que nunca dorme por completo. Ela desperta aqui, atraindo criaturas corrompidas — Lavine precisa dominá-la antes que a domine, e então procurar alguém que entenda o eclipse que a fez o que é."),
                new LocalizationEntry("quest.lavine_redtide.obj_resist_the_whispers", "Resist the corruption's whispers", "Resista aos sussurros da corrupção"),
                new LocalizationEntry("quest.lavine_redtide.obj_cull_corrupted_swarm", "Cull the corrupted swarm drawn to her power (0/8)", "Extermine o enxame corrompido atraído pelo poder dela (0/8)"),
                new LocalizationEntry("quest.lavine_redtide.obj_talk_to_hermit", "Speak with the hermit who studies the Noite Rubra", "Fale com o eremita que estuda a Noite Rubra"),

                // -- Dialogue: dialogue_village_survivor (tied to quest_joaquim_echoes_of_home) --
                new LocalizationEntry("dialogue.village_survivor.name", "Village Survivor", "Sobrevivente da Vila"),
                new LocalizationEntry("dialogue.village_survivor.greeting", "You're not one of them. Thank the roots... I didn't think anyone was left to find me.", "Você não é um deles. Graças às raízes... eu não achava que sobrasse alguém pra me encontrar."),
                new LocalizationEntry("dialogue.village_survivor.choice_ask_destruction", "What happened to this village?", "O que aconteceu com esta vila?"),
                new LocalizationEntry("dialogue.village_survivor.choice_ask_joaquim", "Did you know Joaquim, the guardian who grew up here?", "Você conheceu Joaquim, o guardião que cresceu aqui?"),
                new LocalizationEntry("dialogue.village_survivor.destruction", "It came at night. Shadows poured out of the ground itself and nothing we had could hold them back. Most of us didn't make it out.", "Aconteceu à noite. Sombras brotaram do próprio chão e nada do que tínhamos foi capaz de detê-las. A maioria de nós não conseguiu escapar."),
                new LocalizationEntry("dialogue.village_survivor.about_joaquim", "Joaquim was just a boy who never left anyone behind, even when he should have run. Word is he still carries a shield made from what was left of these roots.", "Joaquim era só um garoto que nunca abandonava ninguém, mesmo quando devia ter fugido. Dizem que ele ainda carrega um escudo feito do que restou destas raízes."),
                new LocalizationEntry("dialogue.village_survivor.offer_help", "If you're clearing what's left of those creatures out of the ruins, I can point you to where the old blade was kept. It might still be there.", "Se você está limpando o que sobrou daquelas criaturas nas ruínas, posso te indicar onde a antiga lâmina era guardada. Talvez ainda esteja lá."),
                new LocalizationEntry("dialogue.village_survivor.choice_accept", "Show me where.", "Mostre-me onde."),
                new LocalizationEntry("dialogue.village_survivor.choice_decline", "Not right now.", "Agora não."),
                new LocalizationEntry("dialogue.village_survivor.accept", "Follow the old well path past the fallen gate. Bring it back to Joaquim — it belongs with him, not buried in the dirt.", "Siga a trilha do velho poço além do portão caído. Leve-a de volta para Joaquim — ela pertence a ele, não enterrada na terra."),
                new LocalizationEntry("dialogue.village_survivor.decline", "It'll wait. It's waited this long already.", "Ela vai esperar. Já esperou até agora."),

                // -- Dialogue: dialogue_windtrail_scholar (tied to quest_maithe_windtrail) --
                new LocalizationEntry("dialogue.windtrail_scholar.name", "Wandering Scholar", "Estudioso Errante"),
                new LocalizationEntry("dialogue.windtrail_scholar.greeting", "Careful on this trail — the golems up there aren't natural, and neither is what's carved into them.", "Cuidado nesta trilha — os golens lá em cima não são naturais, e o que está gravado neles também não."),
                new LocalizationEntry("dialogue.windtrail_scholar.choice_ask_glyphs", "What do you mean, carved into them?", "Como assim, gravado neles?"),
                new LocalizationEntry("dialogue.windtrail_scholar.choice_ask_maithe", "I'm looking into it for Maithe.", "Estou investigando isso para a Maithe."),
                new LocalizationEntry("dialogue.windtrail_scholar.glyphs", "A mark, repeated on every golem I've studied. Not magic I recognize, and not craftsmanship I can place. Someone designed these things on purpose.", "Uma marca, repetida em cada golem que já estudei. Não é magia que eu reconheça, nem um trabalho que eu saiba identificar. Alguém projetou essas coisas de propósito."),
                new LocalizationEntry("dialogue.windtrail_scholar.choice_press_further", "Where else have you seen that mark?", "Onde mais você já viu essa marca?"),
                new LocalizationEntry("dialogue.windtrail_scholar.warning", "On worse things than golems, in worse places than this. If Maithe's chasing it, tell her to bring back proof, not just scars.", "Em coisas piores que golens, em lugares piores que este. Se a Maithe está atrás disso, diga a ela para trazer provas, não só cicatrizes."),
                new LocalizationEntry("dialogue.windtrail_scholar.about_maithe", "The huntress with the strange eye? I've heard she sees things the rest of us can't. If anyone can make sense of these marks, it's her.", "A caçadora com o olho estranho? Ouvi dizer que ela enxerga coisas que o resto de nós não consegue. Se alguém pode entender essas marcas, é ela."),

                // -- Dialogue: dialogue_hermit_mystic (tied to quest_lavine_redtide) --
                new LocalizationEntry("dialogue.hermit_mystic.name", "Hermit Mystic", "Eremita Místico"),
                new LocalizationEntry("dialogue.hermit_mystic.greeting", "I felt something stir before you arrived. A power that doesn't belong to daylight.", "Senti algo se agitar antes de você chegar. Um poder que não pertence à luz do dia."),
                new LocalizationEntry("dialogue.hermit_mystic.choice_ask_eclipse", "Tell me about the Noite Rubra.", "Fale-me sobre a Noite Rubra."),
                new LocalizationEntry("dialogue.hermit_mystic.choice_ask_lavine", "What do you know about Lavine?", "O que você sabe sobre a Lavine?"),
                new LocalizationEntry("dialogue.hermit_mystic.eclipse", "Once in a generation, the sky bleeds red and the veil between here and somewhere else grows thin. Those born under it carry a piece of that somewhere else inside them.", "Uma vez por geração, o céu sangra em vermelho e o véu entre este lugar e algum outro se torna fino. Quem nasce sob ele carrega um pedaço desse outro lugar dentro de si."),
                new LocalizationEntry("dialogue.hermit_mystic.about_lavine", "The sorceress born that night. She fights it every day — I've felt her do it, right now, close by. Most who carry that much power stop fighting long before she has.", "A feiticeira nascida naquela noite. Ela luta contra isso todos os dias — eu senti isso agora mesmo, bem perto. A maioria que carrega tanto poder para de lutar muito antes dela."),
                new LocalizationEntry("dialogue.hermit_mystic.choice_defend_lavine", "She's stronger than you're giving her credit for.", "Ela é mais forte do que você está reconhecendo."),
                new LocalizationEntry("dialogue.hermit_mystic.choice_voice_doubt", "How do you know she'll keep winning that fight?", "Como você sabe que ela vai continuar vencendo essa luta?"),
                new LocalizationEntry("dialogue.hermit_mystic.trust", "I hope you're right. Power like hers, spent protecting instead of consuming — that's rare enough to be worth believing in.", "Espero que você esteja certo. Um poder como o dela, gasto para proteger em vez de consumir — isso é raro o bastante para valer a pena acreditar."),
                new LocalizationEntry("dialogue.hermit_mystic.doubt", "I don't. No one does, not even her. That's exactly why she needs allies who don't flinch when it gets close.", "Eu não sei. Ninguém sabe, nem ela mesma. É exatamente por isso que ela precisa de aliados que não recuem quando isso chegar perto."),

                // -- Maria: quest_maria_fading_light ("O Brilho Que Se Apaga") --
                new LocalizationEntry("quest.maria_fading_light.title", "The Fading Light", "O Brilho Que Se Apaga"),
                new LocalizationEntry("quest.maria_fading_light.description", "Maria's vision showed the sacred crystals losing their light. A shard of the Templo Solar's crystal network has gone dark in the mountain caves — she must cleanse the corruption drawn to it, recover a still-glowing fragment, and bring word back to the temple.", "A visão de Maria mostrou os cristais sagrados perdendo o brilho. Um fragmento da rede de cristais do Templo Solar apagou-se nas cavernas da montanha — ela precisa purificar a corrupção atraída até ali, recuperar um fragmento ainda brilhante e levar notícias de volta ao templo."),
                new LocalizationEntry("quest.maria_fading_light.obj_cleanse_corrupted_wildlife", "Cleanse the corrupted creatures drawn to the fading light (0/5)", "Purifique as criaturas corrompidas atraídas pelo brilho que se apaga (0/5)"),
                new LocalizationEntry("quest.maria_fading_light.obj_gather_crystal_shard", "Gather still-glowing crystal fragments (0/2)", "Reúna fragmentos de cristal ainda brilhantes (0/2)"),
                new LocalizationEntry("quest.maria_fading_light.obj_talk_to_temple_acolyte", "Speak with the acolyte at the Templo Solar", "Fale com a acólita do Templo Solar"),

                // -- Ícaro: quest_icaro_ancient_gears ("Engrenagens do Passado") --
                new LocalizationEntry("quest.icaro_ancient_gears.title", "Gears of the Past", "Engrenagens do Passado"),
                new LocalizationEntry("quest.icaro_ancient_gears.description", "Ícaro traced fragments of a lost civilization to desert ruins still guarded by an old construct. He must put it down, recover an intact tech fragment from the wreckage, and decode its inscriptions — secrets that could save Rapadura, or destroy it.", "Ícaro rastreou fragmentos de uma civilização perdida até ruínas no deserto ainda guardadas por um antigo autômato. Ele precisa derrotá-lo, recuperar um fragmento tecnológico intacto dos escombros e decifrar suas inscrições — segredos que podem salvar Rapadura, ou destruí-la."),
                new LocalizationEntry("quest.icaro_ancient_gears.obj_defeat_ruin_guardian", "Defeat the ruin guardian construct (0/1)", "Derrote o autômato guardião das ruínas (0/1)"),
                new LocalizationEntry("quest.icaro_ancient_gears.obj_recover_tech_fragment", "Recover an intact tech fragment (0/1)", "Recupere um fragmento tecnológico intacto (0/1)"),
                new LocalizationEntry("quest.icaro_ancient_gears.obj_decode_fragment", "Decode the fragment's inscriptions back at the workshop", "Decifre as inscrições do fragmento na oficina"),

                // -- Dialogue: dialogue_temple_acolyte (tied to quest_maria_fading_light) --
                new LocalizationEntry("dialogue.temple_acolyte.name", "Temple Acolyte", "Acólita do Templo"),
                new LocalizationEntry("dialogue.temple_acolyte.greeting", "Maria — you came back from the caves. I can feel the crystal's light is steadier already, just from you standing here.", "Maria — você voltou das cavernas. Já sinto que a luz do cristal está mais estável, só de você estar aqui."),
                new LocalizationEntry("dialogue.temple_acolyte.choice_ask_crystals", "What's causing the crystals to lose their light?", "O que está fazendo os cristais perderem o brilho?"),
                new LocalizationEntry("dialogue.temple_acolyte.choice_ask_maria", "What do you make of my vision?", "O que você acha da minha visão?"),
                new LocalizationEntry("dialogue.temple_acolyte.crystals", "No one is certain. The elders say the light has dimmed before, long ago, but never this fast. Whatever draws those corrupted creatures to the shards is spreading.", "Ninguém tem certeza. Os anciãos dizem que a luz já diminuiu antes, há muito tempo, mas nunca tão rápido. O que quer que atraia aquelas criaturas corrompidas aos fragmentos está se espalhando."),
                new LocalizationEntry("dialogue.temple_acolyte.about_maria", "A vision like yours hasn't come to this temple in a generation. If you truly saw a group of heroes who can restore the balance, then finding them isn't just your calling — it's all of ours.", "Uma visão como a sua não vem a este templo há uma geração. Se você realmente viu um grupo de heróis capaz de restaurar o equilíbrio, encontrá-los não é só o seu chamado — é o de todos nós."),
                new LocalizationEntry("dialogue.temple_acolyte.offer_join", "The temple can spare hands and supplies, if you'll have them, for this journey you're set on.", "O templo pode ceder mãos e suprimentos, se você aceitar, para esta jornada que você decidiu seguir."),
                new LocalizationEntry("dialogue.temple_acolyte.choice_pledge_help", "I'll take whatever help the temple can offer.", "Vou aceitar qualquer ajuda que o templo puder oferecer."),
                new LocalizationEntry("dialogue.temple_acolyte.choice_stay_watchful", "Keep the temple's strength here, in case the light fades further.", "Mantenha a força do templo aqui, caso a luz continue a se apagar."),
                new LocalizationEntry("dialogue.temple_acolyte.pledge", "Then go with the Templo Solar's blessing. Every hand you gather brings the balance closer to restored.", "Então vá com a bênção do Templo Solar. Cada mão que você reunir aproxima o equilíbrio de ser restaurado."),
                new LocalizationEntry("dialogue.temple_acolyte.watchful", "Wise. Someone should watch the light while you search for those who can save it. I'll keep my eyes on the crystal.", "Sábio. Alguém precisa vigiar a luz enquanto você procura por aqueles que podem salvá-la. Vou manter meus olhos no cristal."),

                // -- Dialogue: dialogue_ruins_archivist (tied to quest_icaro_ancient_gears) --
                new LocalizationEntry("dialogue.ruins_archivist.name", "Ruins Archivist", "Arquivista das Ruínas"),
                new LocalizationEntry("dialogue.ruins_archivist.greeting", "You're the inventor everyone's talking about. Did you really put down the guardian that's stood watch here for centuries?", "Você é o inventor de quem todos falam. Você realmente derrotou o guardião que vigia este lugar há séculos?"),
                new LocalizationEntry("dialogue.ruins_archivist.choice_ask_fragment", "What can you tell me about this fragment I recovered?", "O que você pode me dizer sobre este fragmento que eu recuperei?"),
                new LocalizationEntry("dialogue.ruins_archivist.choice_ask_icaro", "What have you heard about me?", "O que você já ouviu falar sobre mim?"),
                new LocalizationEntry("dialogue.ruins_archivist.fragment", "Part of it is a schematic — power beyond anything we've built since. The rest is a warning, half-eroded. I can only make out enough to know it was written for a reason.", "Parte dele é um esquema — poder além de qualquer coisa que construímos desde então. O resto é um aviso, meio corroído. Só consigo entender o suficiente para saber que foi escrito por um motivo."),
                new LocalizationEntry("dialogue.ruins_archivist.choice_share_openly", "Study it with me. Rapadura deserves to know what we find.", "Estude isso comigo. Rapadura merece saber o que encontrarmos."),
                new LocalizationEntry("dialogue.ruins_archivist.choice_withhold_secret", "Some of this stays with me until I understand what it can do.", "Parte disso fica comigo até eu entender o que pode fazer."),
                new LocalizationEntry("dialogue.ruins_archivist.about_icaro", "That you build machines that shame most mages, and that you never stop asking what else the old world left behind. Careful — that hunger works both ways.", "Que você constrói máquinas que envergonham a maioria dos magos, e que nunca para de perguntar o que mais o mundo antigo deixou para trás. Cuidado — essa fome funciona nos dois sentidos."),
                new LocalizationEntry("dialogue.ruins_archivist.share", "Then let's get started. Whatever this technology can do, it's safer understood by more than one mind.", "Então vamos começar. Seja lá o que essa tecnologia possa fazer, é mais seguro ser compreendida por mais de uma mente."),
                new LocalizationEntry("dialogue.ruins_archivist.withhold", "I won't push. Just remember — the warning half of that schematic was written by someone who thought the same way you do now.", "Não vou insistir. Só lembre-se — a metade do esquema que é um aviso foi escrita por alguém que pensava do mesmo jeito que você pensa agora."),

                // -- Convergence: quest_convergence_the_gathering ("A Convocação") — gated on all
                // five personal quests, seeded by ConvergenceQuestSeeder.cs --
                new LocalizationEntry("quest.convergence.title", "The Gathering", "A Convocação"),
                new LocalizationEntry("quest.convergence.description", "Maria's vision was clear: only a group of heroes can restore Rapadura's balance. Joaquim, Maithe, Lavine and Ícaro have each answered her call — now they must meet, decide to stand together, and prove it against a threat none of them could face alone.", "A visão de Maria foi clara: só um grupo de heróis pode restaurar o equilíbrio de Rapadura. Joaquim, Maithe, Lavine e Ícaro responderam ao seu chamado — agora precisam se encontrar, decidir permanecer juntos e provar isso contra uma ameaça que nenhum deles enfrentaria sozinho."),
                new LocalizationEntry("quest.convergence.obj_meet_maria_at_the_gathering", "Meet Maria at the gathering point", "Encontre Maria no ponto de encontro"),
                new LocalizationEntry("quest.convergence.obj_five_heroes_unite", "Decide, together, to face what's coming", "Decidam, juntos, enfrentar o que está por vir"),
                new LocalizationEntry("quest.convergence.obj_face_the_mountain_warlord", "Defeat the mountain warlord as a group", "Derrote o senhor da guerra da montanha em grupo"),

                // -- Dialogue: dialogue_convergence_first_meeting (tied to
                // quest_convergence_the_gathering) — the five heroes' first meeting, seeded by
                // ConvergenceDialogueSeeder.cs --
                new LocalizationEntry("dialogue.convergence_first_meeting.speaker_maria", "Maria", "Maria"),
                new LocalizationEntry("dialogue.convergence_first_meeting.speaker_joaquim", "Joaquim", "Joaquim"),
                new LocalizationEntry("dialogue.convergence_first_meeting.speaker_maithe", "Maithe", "Maithe"),
                new LocalizationEntry("dialogue.convergence_first_meeting.speaker_icaro", "Ícaro", "Ícaro"),
                new LocalizationEntry("dialogue.convergence_first_meeting.speaker_lavine", "Lavine", "Lavine"),
                new LocalizationEntry("dialogue.convergence_first_meeting.maria_opens", "You all came. Feels strange calling my own family to a gathering point instead of just showing up at the door, doesn't it? But I saw this moment before I understood it — the five of us, cousins, standing where the light still holds. Rapadura's balance was never going to be restored by one hero alone. It was going to take our whole family.", "Vocês todos vieram. É estranho chamar minha própria família pra um ponto de encontro em vez de simplesmente bater na porta, não é? Mas eu vi este momento antes de entender — nós cinco, primos, parados onde a luz ainda resiste. O equilíbrio de Rapadura nunca ia ser restaurado por um único herói. Ia precisar da nossa família inteira."),
                new LocalizationEntry("dialogue.convergence_first_meeting.joaquim_worries", "Little sister, you know I'd follow you into worse than this. But five of us fighting together means five ways for it to go wrong. I've buried people who trusted the wrong plan — I won't bury family. If we do this, no one gets left behind. I'll hold that line myself if I have to.", "Maninha, você sabe que eu te seguiria pra coisa pior que essa. Mas nós cinco lutando juntos significa cinco jeitos disso dar errado. Já enterrei gente que confiou no plano errado — não vou enterrar família. Se vamos fazer isso, ninguém fica pra trás. Eu mesmo seguro essa linha se for preciso."),
                new LocalizationEntry("dialogue.convergence_first_meeting.maithe_presses", "A vision isn't proof, Maria — and don't think being the oldest here means I'll just fall in line because you asked nice. I've dragged Ícaro out of worse ideas than this since he was old enough to walk. Before any of us commits, I want to know exactly what we're walking into.", "Uma visão não é prova, Maria — e não pense que eu ser a mais velha aqui significa que vou simplesmente concordar porque você pediu com jeito. Já tirei o Ícaro de ideias piores que essa desde que ele tinha idade pra andar. Antes de qualquer um de nós se comprometer, eu quero saber exatamente no que estamos nos metendo."),
                new LocalizationEntry("dialogue.convergence_first_meeting.icaro_weighs_in", "For once my sister and I agree on something before I even open my mouth. Proof or not, it's testable: five skill sets, one shared threat, and three of us already know how the other two fight from growing up in the same house. That's not faith, that's just good odds.", "Pela primeira vez minha irmã e eu concordamos em algo antes de eu nem abrir a boca. Prova ou não, é testável: cinco conjuntos de habilidades, uma ameaça compartilhada, e três de nós já sabe como os outros dois lutam de ter crescido na mesma casa. Isso não é fé, são só boas chances."),
                new LocalizationEntry("dialogue.convergence_first_meeting.lavine_doubts", "You all keep saying 'the five of us' like it's simple because we share blood. Being the youngest cousin doesn't mean I've earned my place in this number. I've spent more nights fighting myself than fighting anything out there — I'm not sure I belong standing where you saw me, Maria.", "Vocês continuam dizendo 'nós cinco' como se fosse simples só porque somos sangue. Ser a prima mais nova não significa que eu ganhei meu lugar nesse número. Passei mais noites lutando contra mim mesma do que contra qualquer coisa lá fora — não tenho certeza se pertenço a onde você me viu, Maria."),
                new LocalizationEntry("dialogue.convergence_first_meeting.maria_asks", "I saw five, Lavine — not four cousins and one exception, and not for one second did being the youngest change that. Family doesn't get to opt out of the vision. What I need to hear now, from all of you, is whether you'll stand together when it counts, starting today.", "Eu vi cinco, Lavine — não quatro primos e uma exceção, e ser a mais nova não mudou isso nem por um segundo. Família não escolhe ficar de fora da visão. O que eu preciso ouvir agora, de todos vocês, é se vão ficar juntos quando importar, começando hoje."),
                new LocalizationEntry("dialogue.convergence_first_meeting.choice_unite", "We stand together. Whatever's coming, it finds all five of us — the whole family.", "Ficamos juntos. Seja lá o que estiver vindo, vai encontrar nós cinco — a família inteira."),
                new LocalizationEntry("dialogue.convergence_first_meeting.choice_hesitate", "Let's not swear to anything until we've fought side by side once — even family needs to prove it in the field.", "Não vamos jurar nada até termos lutado lado a lado ao menos uma vez — até família precisa provar em campo."),
                new LocalizationEntry("dialogue.convergence_first_meeting.group_unites", "Then it's settled. Rapadura's balance rests on our family now — let's go earn that vision together.", "Então está decidido. O equilíbrio de Rapadura agora depende da nossa família — vamos honrar essa visão juntos."),
                new LocalizationEntry("dialogue.convergence_first_meeting.group_hesitates", "Fair enough. Then let the fight ahead be the vow — we've shared blood our whole lives, but we'll know soon enough if we can share a battlefield too.", "Justo. Então que a luta que vem seja o juramento — compartilhamos sangue a vida inteira, mas logo vamos saber se conseguimos compartilhar um campo de batalha também."),
            };
        }
    }
}
