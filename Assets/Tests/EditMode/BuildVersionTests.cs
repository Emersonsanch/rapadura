using NUnit.Framework;
using Rapadura.Core.Versioning;
using Rapadura.UI.Menus;

namespace Rapadura.Tests
{
    /// <summary>EditMode tests for <see cref="BuildVersion"/> (SemVer helper over Application.version) and the credits text data.</summary>
    public class BuildVersionTests
    {
        [TestCase(null, "0.1.0")]
        [TestCase("", "0.1.0")]
        [TestCase("   ", "0.1.0")]
        [TestCase("1.0", "0.1.0")]
        [TestCase("0.1.0", "0.1.0")]
        [TestCase(" 0.2.3 ", "0.2.3")]
        [TestCase("0.2", "0.2.0")]
        [TestCase("1.1", "1.1.0")]
        [TestCase("0.3.0-beta.1", "0.3.0")]
        [TestCase("nightly", "nightly")]
        public void Resolve_NormalizesAndReplacesPlaceholder(string raw, string expected)
        {
            Assert.AreEqual(expected, BuildVersion.Resolve(raw));
        }

        [Test]
        public void TryParse_ValidAndInvalid()
        {
            Assert.IsTrue(BuildVersion.TryParse("1.2.3", out int major, out int minor, out int patch));
            Assert.AreEqual((1, 2, 3), (major, minor, patch));

            Assert.IsTrue(BuildVersion.TryParse("4.5+build7", out major, out minor, out patch));
            Assert.AreEqual((4, 5, 0), (major, minor, patch));

            Assert.IsFalse(BuildVersion.TryParse("1", out _, out _, out _));
            Assert.IsFalse(BuildVersion.TryParse("1.2.3.4", out _, out _, out _));
            Assert.IsFalse(BuildVersion.TryParse("a.b.c", out _, out _, out _));
            Assert.IsFalse(BuildVersion.TryParse("1.-2.0", out _, out _, out _));
            Assert.IsFalse(BuildVersion.TryParse(null, out _, out _, out _));
        }

        [Test]
        public void Compare_UsesSemVerPrecedence()
        {
            Assert.Less(BuildVersion.Compare("0.1.0", "0.2.0"), 0);
            Assert.Less(BuildVersion.Compare("0.9.9", "0.10.0"), 0);
            Assert.Greater(BuildVersion.Compare("1.0.0", "0.99.99"), 0);
            Assert.AreEqual(0, BuildVersion.Compare("0.1", "0.1.0"));
            Assert.Less(BuildVersion.Compare("garbage", "0.0.1"), 0);
        }

        [Test]
        public void FormatLabel_AddsDevSuffixOnlyForDevBuilds()
        {
            Assert.AreEqual("v0.1.0", BuildVersion.FormatLabel("0.1.0", false));
            Assert.AreEqual("v0.1.0 (dev)", BuildVersion.FormatLabel("0.1.0", true));
        }

        [Test]
        public void Current_IsAlwaysParseable()
        {
            Assert.IsTrue(BuildVersion.TryParse(BuildVersion.Current, out _, out _, out _), BuildVersion.Current);
        }

        [Test]
        public void Credits_MentionStudioCreatorAndAllFiveHeroes()
        {
            string all = string.Empty;
            foreach (CreditsSection section in CreditsContent.Sections)
            {
                all += section.Title + "\n" + string.Join("\n", section.Lines) + "\n";
            }

            StringAssert.Contains("Engenho Estúdio", all);
            StringAssert.Contains("Emerson Leandro Sanches", all);
            foreach (string hero in new[] { "Maithe", "Maria", "Joaquim", "Ícaro", "Lavine" })
            {
                StringAssert.Contains(hero, all);
            }

            Assert.AreEqual("Maithe, Maria, Joaquim, Ícaro e Lavine", CreditsContent.JoinHeroNames());
        }
    }
}
