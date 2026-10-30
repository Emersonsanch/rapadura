using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Mounts;

namespace Rapadura.Tests
{
    /// <summary>
    /// Mounts that grow with the hero (Docs/Progressao500.md §6, Docs/Mounts.md §4.1.1): level cap by the rider's level,
    /// Brilho (stored XP), combat XP per kill and the Despertar I–IV gates.
    /// </summary>
    public class MountGrowthTests
    {
        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            ServiceLocator.Clear();
            MountCollection.RiderLevelOverride = null;
        }

        [TearDown]
        public void TearDown()
        {
            MountCollection.RiderLevelOverride = null;
            EventBus.Clear();
            ServiceLocator.Clear();
        }

        [Test]
        public void LevelCap_IsTwoSqrtOfTheRider_ClampedTo20_ForAnyLevel()
        {
            int[] heroes = { 1, 4, 9, 15, 16, 25, 36, 49, 64, 81, 99, 100, 250, 500, 5000 };
            int[] caps = { 2, 4, 6, 7, 8, 10, 12, 14, 16, 18, 19, 20, 20, 20, 20 };
            for (int i = 0; i < heroes.Length; i++)
            {
                Assert.AreEqual(caps[i], MountGrowthRules.LevelCapFor(heroes[i]), "hero " + heroes[i]);
            }

            Assert.AreEqual(2, MountGrowthRules.LevelCapFor(0), "unknown/zero level counts as 1");
            Assert.AreEqual(100, MountGrowthRules.HeroLevelForCap(20));
            Assert.AreEqual(1, MountGrowthRules.HeroLevelForCap(2));
            Assert.AreEqual(9, MountGrowthRules.HeroLevelForCap(6));
            for (int h = 1; h <= 500; h++)
            {
                Assert.LessOrEqual(MountGrowthRules.LevelCapFor(h), MountGrowthRules.LevelCapFor(h + 1), "never falls");
            }
        }

        [Test]
        public void EffectiveLevel_IsTheLowerOfLevelAndCap_AndTheSavedLevelIsKept()
        {
            Assert.AreEqual(6, MountGrowthRules.EffectiveLevel(18, 10));
            Assert.AreEqual(5, MountGrowthRules.EffectiveLevel(5, 100));
            Assert.AreEqual(20, MountGrowthRules.EffectiveLevel(20, 300));

            int level = 18, xp = 40, glow = 0;
            Assert.AreEqual(0, MountGrowthRules.AddXp(ref level, ref xp, ref glow, 70, 10));
            Assert.AreEqual(18, level, "a low-level cousin never lowers the mount");
            Assert.AreEqual(70, glow, "everything earned above the cap is stored");
        }

        [Test]
        public void AddXp_StoresBrilhoAtTheCap_AndTheBrilhoGoesInFirstWhenTheCapOpens()
        {
            int level = 1, xp = 0, glow = 0;
            Assert.AreEqual(1, MountGrowthRules.AddXp(ref level, ref xp, ref glow, 300, 1), "hero 1: cap 2");
            Assert.AreEqual(2, level);
            Assert.AreEqual(0, xp);
            Assert.AreEqual(220, glow, "300 − 80 of level 1");

            // Hero 9 (cap 6): 220 of Brilho = 100 (2→3) + 120 (3→4).
            Assert.AreEqual(2, MountGrowthRules.AddXp(ref level, ref xp, ref glow, 0, 9));
            Assert.AreEqual(4, level);
            Assert.AreEqual(0, xp);
            Assert.AreEqual(0, glow);

            // Brilho first, then the new XP: 30 stored + 50 new, below the 140 of level 4.
            glow = 30;
            Assert.AreEqual(0, MountGrowthRules.AddXp(ref level, ref xp, ref glow, 50, 9));
            Assert.AreEqual(80, xp);
            Assert.AreEqual(0, glow);

            int maxLevel = 20, maxXp = 0, maxGlow = MountGrowthRules.MaxGlow - 10;
            MountGrowthRules.AddXp(ref maxLevel, ref maxXp, ref maxGlow, 500, 100);
            Assert.AreEqual(20, maxLevel);
            Assert.AreEqual(MountGrowthRules.MaxGlow, maxGlow, "Brilho caps at 5 000");
            Assert.IsTrue(MountGrowthRules.IsGlowFull(maxGlow));
        }

        [Test]
        public void CombatXp_IsTwoPlusOnePercentOfTheKill_CappedAt30()
        {
            Assert.AreEqual(2, MountGrowthRules.CombatXp(0));
            Assert.AreEqual(2, MountGrowthRules.CombatXp(10));
            Assert.AreEqual(2, MountGrowthRules.CombatXp(99));
            Assert.AreEqual(3, MountGrowthRules.CombatXp(128));
            Assert.AreEqual(30, MountGrowthRules.CombatXp(2800));
            Assert.AreEqual(30, MountGrowthRules.CombatXp(1000000), "a level-300 kill never fills the bar");
            Assert.AreEqual(2, MountGrowthRules.CombatXp(-5));
        }

        [Test]
        public void Despertar_AsksHeroLevelHeartsBrilhoAndKm_AndGivesAbilityOnly()
        {
            int full = MountGrowthRules.MaxGlow;
            Assert.IsTrue(MountGrowthRules.CanAwaken(0, 100, 20, full, 3, 0f, true), "I");
            Assert.IsFalse(MountGrowthRules.CanAwaken(0, 99, 20, full, 3, 0f, true), "hero 100");
            Assert.IsFalse(MountGrowthRules.CanAwaken(0, 100, 19, full, 3, 0f, true), "mount 20");
            Assert.IsFalse(MountGrowthRules.CanAwaken(0, 100, 20, full - 1, 3, 0f, true), "full Brilho");
            Assert.IsFalse(MountGrowthRules.CanAwaken(0, 100, 20, full, 2, 0f, true), "3 hearts");
            Assert.IsFalse(MountGrowthRules.CanAwaken(0, 100, 20, full, 3, 0f, false), "materials and the Srosh's quest");
            Assert.IsTrue(MountGrowthRules.CanAwaken(1, 200, 20, full, 4, 0f, true), "II");
            Assert.IsTrue(MountGrowthRules.CanAwaken(2, 300, 20, 0, 5, 0f, true), "III keeps no Brilho gate");
            Assert.IsFalse(MountGrowthRules.CanAwaken(3, 500, 20, 0, 5, 999f, true), "IV: 1 000 km");
            Assert.IsTrue(MountGrowthRules.CanAwaken(3, 500, 20, 0, 5, 1000f, true), "IV");
            Assert.IsFalse(MountGrowthRules.CanAwaken(4, 5000, 20, full, 5, 5000f, true), "nothing past IV");

            Assert.AreEqual(1f, MountGrowthRules.AbilityGradeScale(0));
            Assert.AreEqual(1.15f, MountGrowthRules.AbilityGradeScale(1), 1e-6f);
            Assert.IsTrue(MountGrowthRules.HasSecondaryEffect(2));
            Assert.AreEqual(0.9f, MountGrowthRules.AbilityCooldownFactor(3), 1e-6f);
            Assert.AreEqual(2, MountGrowthRules.AbilityCharges(4));
            Assert.AreEqual(1, MountGrowthRules.AbilityCharges(3));
        }

        [Test]
        public void Collection_WorksAtTheRidersCap_ReleasesBrilho_AndAwakens()
        {
            int heroLevel = 1;
            MountCollection.RiderLevelOverride = () => heroLevel;
            int n = 0;
            var collection = new MountCollection { GuidFactory = () => "g" + (++n), UtcNow = () => 0L };
            collection.Initialize();
            try
            {
                OwnedMount pony = collection.Grant(MountCatalog.Pony, MountCollection.SourceShop);
                MountDefinition definition = MountCatalog.Get(MountCatalog.Pony);

                collection.AddXp(pony, 300);
                Assert.AreEqual(2, pony.level);
                Assert.AreEqual(220, pony.glow);

                pony.level = 18;
                Assert.AreEqual(2, MountCollection.EffectiveLevelOf(pony), "level 18 works at the cap 2 of a level-1 rider");
                Assert.AreEqual(MountRules.MaxHealthAt(definition.BaseHealth, 2), MountCollection.MaxHealthOf(pony), 1e-3f);
                pony.level = 2;

                EventBus.Publish(new PlayerExperienceChangedEvent(0, 100, 1));
                heroLevel = 9;
                EventBus.Publish(new PlayerExperienceChangedEvent(0, 200, 9));
                Assert.AreEqual(4, pony.level, "the rider grew: the Brilho went in");
                Assert.AreEqual(0, pony.glow);

                heroLevel = 100;
                pony.level = 20;
                pony.glow = MountGrowthRules.MaxGlow;
                pony.affection = 300;
                Assert.IsFalse(collection.TryAwaken(pony, hasMaterials: false));
                Assert.IsTrue(collection.TryAwaken(pony, hasMaterials: true));
                Assert.AreEqual(1, pony.awakening);
                Assert.AreEqual(0, pony.glow, "Despertar I spends the full Brilho");
                Assert.IsFalse(collection.TryAwaken(pony, true), "II asks hero 200");

                var restored = new MountCollection();
                pony.glow = 99999;
                pony.awakening = 9;
                restored.RestoreState(collection.CaptureState());
                OwnedMount copy = restored.Find(pony.guid);
                Assert.AreEqual(MountGrowthRules.MaxGlow, copy.glow);
                Assert.AreEqual(MountGrowthRules.MaxAwakening, copy.awakening);
            }
            finally
            {
                collection.Shutdown();
            }
        }

        [Test]
        public void MountGrowthRules_HasNoUnityDependency()
        {
            string source = System.IO.File.ReadAllText("Assets/Scripts/Gameplay/Mounts/MountGrowthRules.cs");
            StringAssert.DoesNotContain("UnityEngine", source);
        }
    }
}
