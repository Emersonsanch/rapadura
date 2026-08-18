using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Core.Localization;

namespace Rapadura.Tests
{
    /// <summary>Plain C# tests for LocalizationManager — no MonoBehaviour/scene/asset required.</summary>
    public class LocalizationManagerTests
    {
        private LocalizationManager CreateManager()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            return manager;
        }

        [Test]
        public void Initialize_LoadsBuiltInSeedKeys()
        {
            LocalizationManager manager = CreateManager();

            Assert.IsTrue(manager.HasKey("ui.pause.title"));
            Assert.IsTrue(manager.HasKey("ui.menu.start"));
        }

        [Test]
        public void Get_DefaultLanguageIsEnglish()
        {
            LocalizationManager manager = CreateManager();

            Assert.AreEqual(LanguageCode.en, manager.CurrentLanguage);
            Assert.AreEqual("Paused", manager.Get("ui.pause.title"));
        }

        [Test]
        public void Get_AfterSettingPtBR_ReturnsPortugueseText()
        {
            LocalizationManager manager = CreateManager();

            manager.SetLanguage(LanguageCode.ptBR);

            Assert.AreEqual("Pausado", manager.Get("ui.pause.title"));
            Assert.AreEqual("Iniciar Jogo", manager.Get("ui.menu.start"));
        }

        [Test]
        public void Get_MissingTranslationForCurrentLanguage_FallsBackToEnglish()
        {
            LocalizationManager manager = CreateManager();
            manager.LoadEntries(new List<LocalizationEntry>
            {
                new LocalizationEntry("ui.test.englishOnly", "English Only", null),
            });
            manager.SetLanguage(LanguageCode.ptBR);

            Assert.AreEqual("English Only", manager.Get("ui.test.englishOnly"));
        }

        [Test]
        public void Get_UnknownKey_ReturnsBracketedKeyInstead_OfThrowing()
        {
            LocalizationManager manager = CreateManager();

            Assert.AreEqual("[does.not.exist]", manager.Get("does.not.exist"));
        }

        [Test]
        public void SetLanguage_RaisesLanguageChangedEvent()
        {
            LocalizationManager manager = CreateManager();
            LanguageCode? raised = null;
            manager.LanguageChanged += lang => raised = lang;

            manager.SetLanguage(LanguageCode.ptBR);

            Assert.AreEqual(LanguageCode.ptBR, raised);
        }

        [Test]
        public void SetLanguage_ToSameLanguage_DoesNotRaiseEvent()
        {
            LocalizationManager manager = CreateManager();
            int raisedCount = 0;
            manager.LanguageChanged += _ => raisedCount++;

            manager.SetLanguage(LanguageCode.en);

            Assert.AreEqual(0, raisedCount);
        }

        [Test]
        public void LoadEntries_OverwritesExistingKey()
        {
            LocalizationManager manager = CreateManager();

            manager.LoadEntries(new List<LocalizationEntry>
            {
                new LocalizationEntry("ui.pause.title", "Paused (v2)", "Pausado (v2)"),
            });

            Assert.AreEqual("Paused (v2)", manager.Get("ui.pause.title"));
        }

        [Test]
        public void Shutdown_ClearsEntries()
        {
            LocalizationManager manager = CreateManager();

            manager.Shutdown();

            Assert.AreEqual(0, manager.KeyCount);
            Assert.AreEqual("[ui.pause.title]", manager.Get("ui.pause.title"));
        }
    }

    /// <summary>Tests for the hand-rolled CSV parser used by the translation pipeline import.</summary>
    public class LocalizationCsvTests
    {
        [Test]
        public void Parse_SimpleCsv_ProducesExpectedEntries()
        {
            string csv = "key,en,pt-BR\n" +
                          "ui.pause.title,Paused,Pausado\n" +
                          "ui.menu.start,Start Game,Iniciar Jogo\n";

            List<LocalizationEntry> entries = LocalizationCsv.Parse(csv);

            Assert.AreEqual(2, entries.Count);
            Assert.AreEqual("ui.pause.title", entries[0].Key);
            Assert.AreEqual("Paused", entries[0].En);
            Assert.AreEqual("Pausado", entries[0].PtBR);
        }

        [Test]
        public void Parse_QuotedFieldWithEmbeddedComma_IsHandledCorrectly()
        {
            string csv = "key,en,pt-BR\n" +
                          "ui.test.comma,\"Hello, world\",\"Olá, mundo\"\n";

            List<LocalizationEntry> entries = LocalizationCsv.Parse(csv);

            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("Hello, world", entries[0].En);
            Assert.AreEqual("Olá, mundo", entries[0].PtBR);
        }

        [Test]
        public void Parse_MissingPtBRColumn_LeavesPtBRNull()
        {
            string csv = "key,en\nui.test.enOnly,English Text\n";

            List<LocalizationEntry> entries = LocalizationCsv.Parse(csv);

            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("English Text", entries[0].En);
            Assert.IsNull(entries[0].PtBR);
        }

        [Test]
        public void Parse_EmptyText_ReturnsEmptyList()
        {
            Assert.AreEqual(0, LocalizationCsv.Parse(string.Empty).Count);
            Assert.AreEqual(0, LocalizationCsv.Parse(null).Count);
        }

        [Test]
        public void Parse_MissingKeyOrEnColumn_Throws()
        {
            Assert.Throws<System.FormatException>(() => LocalizationCsv.Parse("foo,bar\n1,2\n"));
        }

        [Test]
        public void Parse_CanFeedDirectlyIntoLocalizationManager()
        {
            string csv = "key,en,pt-BR\nui.test.pipeline,Pipeline OK,Pipeline OK (pt)\n";
            var manager = new LocalizationManager();
            manager.Initialize();

            manager.LoadEntries(LocalizationCsv.Parse(csv));

            Assert.AreEqual("Pipeline OK", manager.Get("ui.test.pipeline"));
            manager.SetLanguage(LanguageCode.ptBR);
            Assert.AreEqual("Pipeline OK (pt)", manager.Get("ui.test.pipeline"));
        }
    }
}
