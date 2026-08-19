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
            };
        }
    }
}
