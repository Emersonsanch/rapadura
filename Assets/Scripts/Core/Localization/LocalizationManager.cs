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
            };
        }
    }
}
