using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Secrets;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Dressing;
using Rapadura.Save;
using Rapadura.UI.Secrets;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// "Segredos e áreas ocultas": the secret/relic catalogs, the reveal rules (idle / night / story beat), investigating
    /// (reward and guarded fight), the arena secret roll and break, the relic passives on a BuffController, the map gold
    /// hand-off to the bag, per-save persistence (old saves load empty), the collection achievement, the hint placement
    /// on the island and every text key in both languages.
    /// </summary>
    public class SecretsTests
    {
        private const int TestSlot = 90121;

        private readonly List<Object> _created = new List<Object>();
        private readonly List<SecretsProgress> _secrets = new List<SecretsProgress>();

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
            EventBus.Clear();
            ServiceLocator.Clear();
            ItemDatabase.Invalidate();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (SecretsProgress secrets in _secrets)
            {
                secrets.Shutdown();
            }

            _secrets.Clear();
            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();

            EventBus.Clear();
            ServiceLocator.Clear();
            ItemDatabase.Invalidate();
            TestSaveRoot.Exit();
        }

        private SecretsProgress NewSecrets(AchievementManager achievements = null)
        {
            var secrets = new SecretsProgress { Achievements = achievements ?? new AchievementManager(new AchievementDefinition[0]) };
            secrets.Initialize();
            _secrets.Add(secrets);
            return secrets;
        }

        private static SecretRevealContext IdleOn(BiomeType region, float idleSeconds = 0f, bool night = false, StoryBeat seen = StoryBeat.None)
        {
            return new SecretRevealContext
            {
                Region = region,
                RegionUnlocked = true,
                TokenIdle = true,
                IdleSeconds = idleSeconds,
                IsNight = night,
                HasSeenBeat = beat => seen != StoryBeat.None && beat == seen,
            };
        }

        // ------------------------------------------------------------------
        // Catalogs
        // ------------------------------------------------------------------

        [Test]
        public void Catalog_TwelveUniqueRelics_TwoMapSecretsAndOneArenaRelicPerRegion()
        {
            Assert.AreEqual(12, RelicCatalog.Count);
            Assert.AreEqual(12, RelicCatalog.AllIds().Distinct().Count());
            Assert.AreEqual(8, WorldSecretCatalog.All.Count);
            Assert.AreEqual(8, WorldSecretCatalog.All.Select(s => s.Id).Distinct().Count());

            foreach (BiomeType region in new[] { BiomeType.Forest, BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves })
            {
                List<WorldSecretDefinition> secrets = WorldSecretCatalog.InRegion(region);
                Assert.AreEqual(2, secrets.Count, $"{region}: two map secrets");
                Assert.IsTrue(secrets.Any(s => s.Rule == SecretRevealRule.Idle), $"{region}: one opens to patience");
                Assert.IsTrue(secrets.Any(s => s.Rule != SecretRevealRule.Idle), $"{region}: one opens to the night or a story beat");

                RelicDefinition arena = RelicCatalog.ArenaRelicFor(region);
                Assert.IsNotNull(arena, $"{region}: arena relic");
                Assert.AreEqual(RelicSource.ArenaSecret, arena.Source);
            }

            foreach (WorldSecretDefinition secret in WorldSecretCatalog.All)
            {
                RelicDefinition relic = RelicCatalog.Get(secret.RelicId);
                Assert.IsNotNull(relic, secret.Id);
                Assert.AreEqual(RelicSource.MapSecret, relic.Source, secret.Id);
                Assert.AreEqual(secret.Region, relic.Region, secret.Id);
                Assert.AreSame(secret, WorldSecretCatalog.ForRelic(relic.Id));
                Assert.Greater(secret.BonusGold, 0);
                if (secret.Rule == SecretRevealRule.StoryBeat)
                {
                    Assert.AreNotEqual(StoryBeat.None, secret.Beat, secret.Id);
                }
            }

            // Every map relic is behind exactly one secret.
            Assert.AreEqual(8, RelicCatalog.All.Count(r => r.Source == RelicSource.MapSecret));
            Assert.IsTrue(RelicCatalog.All.All(r => r.Value > 0f));
        }

        [Test]
        public void Catalog_HeroRelicsCoverTheFiveCousins()
        {
            var owners = new HashSet<CharacterId>(RelicCatalog.All.Where(r => r.Hero.HasValue).Select(r => r.Hero.Value));
            foreach (CharacterId hero in new[] { CharacterId.Maithe, CharacterId.Maria, CharacterId.Joaquim, CharacterId.Icaro, CharacterId.Lavine })
            {
                Assert.IsTrue(owners.Contains(hero), $"{hero} has a keepsake");
            }
        }

        [Test]
        public void AchievementAsset_TargetsEveryRelic()
        {
            AchievementDefinition definition = Resources.LoadAll<AchievementDefinition>(AchievementManager.ResourcesFolder)
                .FirstOrDefault(d => d.AchievementId == "ach_relics_all");
            Assert.IsNotNull(definition, "Resources/Achievements/ach_relics_all.asset");
            Assert.AreEqual(AchievementConditionType.RelicsCollected, definition.ConditionType);
            CollectionAssert.AreEquivalent(RelicCatalog.AllIds(), definition.TargetIds);
            Assert.AreEqual("achievement.ach_relics_all.title", definition.TitleKey);
            Assert.AreEqual("achievement.ach_relics_all.description", definition.DescriptionKey);
            Assert.IsTrue(Rapadura.UI.Achievements.AchievementTexts.All.Any(e => e.Key == definition.TitleKey && !string.IsNullOrEmpty(e.En) && !string.IsNullOrEmpty(e.PtBR)));
            Assert.IsTrue(Rapadura.UI.Achievements.AchievementTexts.All.Any(e => e.Key == definition.DescriptionKey && !string.IsNullOrEmpty(e.En) && !string.IsNullOrEmpty(e.PtBR)));
        }

        // ------------------------------------------------------------------
        // Reveal rules
        // ------------------------------------------------------------------

        [Test]
        public void Reveal_Idle_NeedsTheIdleTimeOnItsOwnUnlockedRegion()
        {
            WorldSecretDefinition farinha = WorldSecretCatalog.Get(WorldSecretCatalog.CasaDeFarinha);
            Assert.AreEqual(SecretRevealRule.Idle, farinha.Rule);

            Assert.IsFalse(SecretRules.ShouldReveal(farinha, IdleOn(BiomeType.Forest, farinha.IdleSeconds - 0.1f)));
            Assert.IsTrue(SecretRules.ShouldReveal(farinha, IdleOn(BiomeType.Forest, farinha.IdleSeconds)));
            Assert.IsFalse(SecretRules.ShouldReveal(farinha, IdleOn(BiomeType.Desert, 99f)), "another region");

            SecretRevealContext walking = IdleOn(BiomeType.Forest, 99f);
            walking.TokenIdle = false;
            Assert.IsFalse(SecretRules.ShouldReveal(farinha, walking), "travelling / paused");

            SecretRevealContext locked = IdleOn(BiomeType.Forest, 99f);
            locked.RegionUnlocked = false;
            Assert.IsFalse(SecretRules.ShouldReveal(farinha, locked));
            Assert.IsFalse(SecretRules.ShouldReveal(null, IdleOn(BiomeType.Forest, 99f)));
        }

        [Test]
        public void Reveal_NightAndStoryBeatRules()
        {
            WorldSecretDefinition jatoba = WorldSecretCatalog.Get(WorldSecretCatalog.JatobaOco);
            Assert.AreEqual(SecretRevealRule.Night, jatoba.Rule);
            Assert.IsFalse(SecretRules.ShouldReveal(jatoba, IdleOn(BiomeType.Forest, 99f)), "day, however long");
            Assert.IsTrue(SecretRules.ShouldReveal(jatoba, IdleOn(BiomeType.Forest, 0f, night: true)), "night, at once");

            WorldSecretDefinition gruta = WorldSecretCatalog.Get(WorldSecretCatalog.GrutaCachoeira);
            Assert.AreEqual(SecretRevealRule.StoryBeat, gruta.Rule);
            Assert.IsFalse(SecretRules.ShouldReveal(gruta, IdleOn(BiomeType.Mountain, 99f, true)));
            Assert.IsFalse(SecretRules.ShouldReveal(gruta, IdleOn(BiomeType.Mountain, 0f, seen: StoryBeat.InterludeForest)), "the wrong beat");
            Assert.IsTrue(SecretRules.ShouldReveal(gruta, IdleOn(BiomeType.Mountain, 0f, seen: gruta.Beat)));

            SecretRevealContext noStory = IdleOn(BiomeType.Mountain);
            noStory.HasSeenBeat = null;
            Assert.IsFalse(SecretRules.ShouldReveal(gruta, noStory), "no StoryProgress = never");
        }

        [Test]
        public void RevealDue_RevealsOnlyWhatTheContextAllows_Once()
        {
            SecretsProgress secrets = NewSecrets();
            var revealed = new List<string>();
            EventBus.Subscribe<SecretRevealedEvent>(e => revealed.Add(e.SecretId));

            Assert.AreEqual(0, WorldMapSecretsDirector.RevealDue(secrets, IdleOn(BiomeType.Forest, 1f)));
            Assert.AreEqual(2, WorldMapSecretsDirector.RevealDue(secrets, IdleOn(BiomeType.Forest, 30f, night: true)));
            Assert.AreEqual(0, WorldMapSecretsDirector.RevealDue(secrets, IdleOn(BiomeType.Forest, 30f, night: true)), "already revealed");

            CollectionAssert.AreEquivalent(new[] { WorldSecretCatalog.CasaDeFarinha, WorldSecretCatalog.JatobaOco }, revealed);
            Assert.IsFalse(secrets.IsRevealed(WorldSecretCatalog.BauMandacaru));
            Assert.IsFalse(secrets.Reveal("secret_does_not_exist"));
        }

        // ------------------------------------------------------------------
        // Investigating
        // ------------------------------------------------------------------

        [Test]
        public void Claim_GrantsTheRelicAndQueuesTheGold_OnlyOnceAndOnlyWhenRevealed()
        {
            SecretsProgress secrets = NewSecrets();
            var relics = new List<RelicCollectedEvent>();
            var claims = new List<SecretClaimedEvent>();
            EventBus.Subscribe<RelicCollectedEvent>(e => relics.Add(e));
            EventBus.Subscribe<SecretClaimedEvent>(e => claims.Add(e));
            WorldSecretDefinition farinha = WorldSecretCatalog.Get(WorldSecretCatalog.CasaDeFarinha);

            Assert.IsFalse(secrets.Claim(farinha.Id), "not revealed yet");
            Assert.IsTrue(secrets.Reveal(farinha.Id));
            Assert.IsTrue(secrets.IsInvestigable(farinha.Id));
            Assert.IsTrue(secrets.Claim(farinha.Id));
            Assert.IsFalse(secrets.Claim(farinha.Id), "already investigated");

            Assert.IsTrue(secrets.HasRelic(farinha.RelicId));
            Assert.IsFalse(secrets.IsInvestigable(farinha.Id));
            Assert.AreEqual(farinha.BonusGold, secrets.PendingGold);
            Assert.AreEqual(1, relics.Count);
            Assert.AreEqual(1, relics[0].OwnedCount);
            Assert.AreEqual(1, claims.Count);
            Assert.AreEqual(farinha.BonusGold, claims[0].Gold);
        }

        [Test]
        public void GuardedSecret_IsClaimedByItsOwnVictoryOnly()
        {
            SecretsProgress secrets = NewSecrets();
            WorldSecretDefinition cacimba = WorldSecretCatalog.Get(WorldSecretCatalog.CacimbaSeca);
            Assert.AreEqual(SecretVisitKind.Encounter, cacimba.Visit);
            var payload = EncounterPayload.Create(BiomeType.Desert, new List<Rapadura.Gameplay.Enemies.EnemyDefinition>(), 7);

            Assert.IsFalse(secrets.BeginSecretEncounter(cacimba.Id, payload), "not revealed yet");
            secrets.Reveal(cacimba.Id);
            Assert.IsTrue(secrets.BeginSecretEncounter(cacimba.Id, payload));
            Assert.AreEqual(cacimba.Id, secrets.PendingEncounterSecret);

            // Another fight ending (a random roll) changes nothing.
            EventBus.Publish(new EncounterEndedEvent(EncounterPayload.Create(BiomeType.Desert, null, 8), EncounterOutcome.Victory));
            Assert.IsFalse(secrets.IsClaimed(cacimba.Id));

            // Fled: the secret stays open for another try.
            EventBus.Publish(new EncounterEndedEvent(payload, EncounterOutcome.Fled));
            Assert.IsFalse(secrets.IsClaimed(cacimba.Id));
            Assert.IsNull(secrets.PendingEncounterSecret);
            Assert.IsTrue(secrets.IsInvestigable(cacimba.Id));

            Assert.IsTrue(secrets.BeginSecretEncounter(cacimba.Id, payload));
            EventBus.Publish(new EncounterEndedEvent(payload, EncounterOutcome.Victory));
            Assert.IsTrue(secrets.IsClaimed(cacimba.Id));
            Assert.IsTrue(secrets.HasRelic(cacimba.RelicId));
        }

        // ------------------------------------------------------------------
        // Arena secrets
        // ------------------------------------------------------------------

        [Test]
        public void ArenaSpawn_RollIsDeterministic_AndBossFightsNeverGetOne()
        {
            Assert.AreEqual(SecretRules.ArenaRoll(1234), SecretRules.ArenaRoll(1234));
            for (int seed = -50; seed < 50; seed++)
            {
                float roll = SecretRules.ArenaRoll(seed * 7919);
                Assert.GreaterOrEqual(roll, 0f);
                Assert.Less(roll, 1f);
            }

            Assert.IsTrue(SecretRules.ShouldSpawnArenaSecret(false, false, SecretRules.ArenaChanceRelicMissing - 0.01f));
            Assert.IsFalse(SecretRules.ShouldSpawnArenaSecret(false, false, SecretRules.ArenaChanceRelicMissing));
            Assert.IsFalse(SecretRules.ShouldSpawnArenaSecret(false, true, SecretRules.ArenaChanceRelicFound), "rarer once the relic is home");
            Assert.IsTrue(SecretRules.ShouldSpawnArenaSecret(false, true, SecretRules.ArenaChanceRelicFound - 0.01f));
            Assert.IsFalse(SecretRules.ShouldSpawnArenaSecret(true, false, 0f), "boss challenge");

            // Over many encounters, roughly the advertised share gets one.
            int hits = Enumerable.Range(0, 2000).Count(seed => SecretRules.ShouldSpawnArenaSecret(false, false, SecretRules.ArenaRoll(seed)));
            Assert.That(hits / 2000f, Is.EqualTo(SecretRules.ArenaChanceRelicMissing).Within(0.05f));
        }

        [Test]
        public void ArenaSecret_BreaksAfterThreeHeroHits_RelicFirstThenGold()
        {
            SecretsProgress secrets = NewSecrets();
            ServiceLocator.Register(secrets);

            ArenaSecretObject first = NewArenaSecret(BiomeType.Mountain);
            Assert.IsTrue(first.IsAlive);
            first.ApplyDamage(5f, ElementType.Physical, null);
            first.ApplyDamage(5f, ElementType.Physical, null);
            Assert.IsFalse(first.IsBroken);
            first.ApplyDamage(5f, ElementType.Physical, null);
            Assert.IsTrue(first.IsBroken);
            Assert.IsFalse(first.IsAlive);
            Assert.IsTrue(secrets.HasRelic(RelicCatalog.PiaoIrmaos));
            first.ApplyDamage(5f, ElementType.Physical, null);
            Assert.AreEqual(1, secrets.ArenaSecretsBroken, "a broken secret takes no more hits");

            // The relic is home: the next one holds gold (no Player in EditMode, so it waits as pending gold).
            var broken = new List<ArenaSecretBrokenEvent>();
            EventBus.Subscribe<ArenaSecretBrokenEvent>(e => broken.Add(e));
            ArenaSecretObject second = NewArenaSecret(BiomeType.Mountain);
            var hero = new GameObject("HeroSwing");
            _created.Add(hero);
            for (int i = 0; i < SecretRules.ArenaSecretHits; i++)
            {
                second.ApplyDamage(1f, ElementType.Physical, hero);
            }

            Assert.IsTrue(second.IsBroken);
            Assert.AreEqual(1, broken.Count);
            Assert.IsNull(broken[0].RelicId);
            Assert.AreEqual(SecretRules.ArenaGoldFor(BiomeType.Mountain), broken[0].Gold);
            Assert.AreEqual(SecretRules.ArenaGoldFor(BiomeType.Mountain), secrets.PendingGold);
            Assert.AreEqual(2, secrets.ArenaSecretsBroken);
            Assert.Greater(SecretRules.ArenaGoldFor(BiomeType.Caves), SecretRules.ArenaGoldFor(BiomeType.Forest));
        }

        private ArenaSecretObject NewArenaSecret(BiomeType biome)
        {
            var go = new GameObject("ArenaSecretTest");
            _created.Add(go);
            go.AddComponent<SphereCollider>();
            var secret = go.AddComponent<ArenaSecretObject>();
            secret.Configure(biome, null);
            return secret;
        }

        [Test]
        public void ArenaSecret_CountsAnyHitButAnEnemys()
        {
            Assert.IsTrue(ArenaSecretObject.CountsAsHit(null));
            var hero = new GameObject("Hero");
            _created.Add(hero);
            Assert.IsTrue(ArenaSecretObject.CountsAsHit(hero));
        }

        // ------------------------------------------------------------------
        // Passives / gold
        // ------------------------------------------------------------------

        [Test]
        public void Passives_OnePerRelic_Idempotent_DoubledForTheOwner()
        {
            var go = new GameObject("RelicHero");
            _created.Add(go);
            var buffs = go.AddComponent<BuffController>();
            var owned = new[] { RelicCatalog.CartasVoChico, RelicCatalog.ArcoMaithe, "relic_unknown" };

            Assert.AreEqual(2, RelicPassiveApplier.ApplyPassives(buffs, owned, CharacterId.Joaquim));
            Assert.AreEqual(2, RelicPassiveApplier.ApplyPassives(buffs, owned, CharacterId.Joaquim));
            Assert.AreEqual(10f, buffs.GetFlatTotal(StatType.MaxHealth), 0.0001f, "re-applying never stacks");
            Assert.AreEqual(0.01f, buffs.GetAdditiveFractionTotal(StatType.CriticalChance), 0.0001f);

            RelicPassiveApplier.ApplyPassives(buffs, owned, CharacterId.Maithe);
            Assert.AreEqual(0.02f, buffs.GetAdditiveFractionTotal(StatType.CriticalChance), 0.0001f, "Maithe with her own bow");
            Assert.AreEqual(10f, buffs.GetFlatTotal(StatType.MaxHealth), 0.0001f, "the grandparents' relics are the same for all");

            // Other sources are left alone; an empty collection clears only the relic modifiers.
            buffs.ApplyEffect(new StatModifierDefinition { affectedStat = StatType.MaxHealth, value = 5f, durationSeconds = 0f }, "equip_Chest");
            Assert.AreEqual(0, RelicPassiveApplier.ApplyPassives(buffs, new string[0], null));
            Assert.AreEqual(5f, buffs.GetFlatTotal(StatType.MaxHealth), 0.0001f);
        }

        [Test]
        public void Passives_CooldownAndCostRelicsAreFlat_SoTheyCountOnAZeroBase()
        {
            var go = new GameObject("RelicHero");
            _created.Add(go);
            var buffs = go.AddComponent<BuffController>();
            RelicPassiveApplier.ApplyPassives(buffs, new[] { RelicCatalog.EngrenagemIcaro, RelicCatalog.RetratoPrimos }, null);
            Assert.AreEqual(0.03f, buffs.GetModifiedValue(StatType.CooldownReduction, 0f), 0.0001f);
            Assert.AreEqual(0.03f, buffs.GetModifiedValue(StatType.ResourceCostReduction, 0f), 0.0001f);
        }

        [Test]
        public void PendingGold_GoesIntoTheBag()
        {
            SecretsProgress secrets = NewSecrets();
            secrets.Reveal(WorldSecretCatalog.CasaDeFarinha);
            secrets.Claim(WorldSecretCatalog.CasaDeFarinha);
            int gold = secrets.PendingGold;
            Assert.Greater(gold, 0);

            var go = new GameObject("GoldBag");
            _created.Add(go);
            var bag = go.AddComponent<InventoryManager>();
            ItemDefinition coin = ItemDatabase.GetById(RelicPassiveApplier.GoldItemId);
            Assert.IsNotNull(coin, "item_gold_coin exists");

            Assert.AreEqual(gold, RelicPassiveApplier.DeliverPendingGold(secrets, bag, coin));
            Assert.AreEqual(gold, bag.GetTotalCount(RelicPassiveApplier.GoldItemId));
            Assert.AreEqual(0, secrets.PendingGold);
            Assert.AreEqual(0, RelicPassiveApplier.DeliverPendingGold(secrets, bag, coin), "nothing left");
            Assert.AreEqual(0, RelicPassiveApplier.DeliverPendingGold(secrets, null, coin));
        }

        // ------------------------------------------------------------------
        // Achievement
        // ------------------------------------------------------------------

        [Test]
        public void Achievement_UnlocksWithTheTwelfthDistinctRelic()
        {
            var definition = ScriptableObject.CreateInstance<AchievementDefinition>();
            _created.Add(definition);
            definition.ConfigureForTests("ach_relics_test", AchievementConditionType.RelicsCollected, 0, RelicCatalog.AllIds());
            var achievements = new AchievementManager(new[] { definition });
            SecretsProgress secrets = NewSecrets(achievements);

            string[] ids = RelicCatalog.AllIds();
            for (int i = 0; i < ids.Length - 1; i++)
            {
                Assert.IsTrue(secrets.AddRelic(ids[i]));
            }

            Assert.IsFalse(secrets.AddRelic(ids[0]), "duplicate");
            Assert.IsFalse(achievements.IsUnlocked("ach_relics_test"));
            Assert.AreEqual(11, achievements.GetProgress(definition));

            Assert.IsTrue(secrets.AddRelic(ids[ids.Length - 1]));
            Assert.IsTrue(secrets.HasAllRelics);
            Assert.IsTrue(achievements.IsUnlocked("ach_relics_test"));
        }

        // ------------------------------------------------------------------
        // Save / load
        // ------------------------------------------------------------------

        [Test]
        public void SaveLoad_RoundTripsSecretsRelicsAndGold()
        {
            var save = new SaveManager();
            save.Initialize();
            SecretsProgress secrets = NewSecrets();
            save.Register(secrets);
            secrets.Reveal(WorldSecretCatalog.CasaDeFarinha);
            secrets.Claim(WorldSecretCatalog.CasaDeFarinha);
            secrets.Reveal(WorldSecretCatalog.BauMandacaru);
            secrets.AddRelic(RelicCatalog.RetratoPrimos);
            secrets.BreakArenaSecret(BiomeType.Caves, out _);

            save.Save(TestSlot);
            Assert.IsTrue(save.PeekSlot(TestSlot).entries.Any(e => e.key == SecretsProgress.Key));

            var loadedSave = new SaveManager();
            loadedSave.Initialize();
            SecretsProgress loaded = NewSecrets();
            loadedSave.Register(loaded);
            Assert.IsTrue(loadedSave.Load(TestSlot));

            Assert.IsTrue(loaded.IsClaimed(WorldSecretCatalog.CasaDeFarinha));
            Assert.IsTrue(loaded.IsInvestigable(WorldSecretCatalog.BauMandacaru));
            Assert.IsFalse(loaded.IsRevealed(WorldSecretCatalog.JatobaOco));
            CollectionAssert.AreEqual(secrets.OwnedRelics, loaded.OwnedRelics, "same relics, same order");
            Assert.AreEqual(secrets.PendingGold, loaded.PendingGold);
            Assert.AreEqual(1, loaded.ArenaSecretsBroken);
        }

        [Test]
        public void OldSave_WithoutSecrets_LoadsEmpty_AndGarbageIsIgnored()
        {
            var save = new SaveManager();
            save.Initialize();
            save.Register(new WorldMapProgress());
            save.Save(TestSlot);
            Assert.IsFalse(save.PeekSlot(TestSlot).entries.Any(e => e.key == SecretsProgress.Key), "precondition: no secrets entry");

            // Loaded into a session that already found things (the previous run).
            var reset = new NewSessionReset();
            SecretsProgress secrets = NewSecrets();
            reset.Register(secrets);
            var loadedSave = new SaveManager { SessionReset = reset };
            loadedSave.Initialize();
            loadedSave.Register(secrets);
            loadedSave.Register(new WorldMapProgress());
            secrets.Reveal(WorldSecretCatalog.JatobaOco);
            Assert.IsTrue(secrets.Claim(WorldSecretCatalog.JatobaOco));
            Assert.AreEqual(1, secrets.OwnedRelicCount, "precondition: the previous run found a relic");

            Assert.IsTrue(loadedSave.Load(TestSlot));
            Assert.AreEqual(0, secrets.OwnedRelicCount);
            Assert.IsFalse(secrets.IsRevealed(WorldSecretCatalog.JatobaOco));
            Assert.AreEqual(0, secrets.PendingGold);

            Assert.DoesNotThrow(() => secrets.RestoreState(new SecretsSaveState { revealedSecrets = null, claimedSecrets = null, relics = null, pendingGold = -5 }));
            Assert.AreEqual(0, secrets.PendingGold);

            // Unknown ids from a later/older version are dropped; a claimed secret counts as revealed.
            secrets.RestoreState(new SecretsSaveState
            {
                claimedSecrets = new List<string> { WorldSecretCatalog.MarcoTropeiros, "secret_gone" },
                relics = new List<string> { "relic_gone", RelicCatalog.ChocalhoTeimosa, RelicCatalog.ChocalhoTeimosa },
            });
            Assert.IsTrue(secrets.IsRevealed(WorldSecretCatalog.MarcoTropeiros));
            Assert.IsTrue(secrets.IsClaimed(WorldSecretCatalog.MarcoTropeiros));
            CollectionAssert.AreEqual(new[] { RelicCatalog.ChocalhoTeimosa }, secrets.OwnedRelics);
        }

        // ------------------------------------------------------------------
        // Hints on the island
        // ------------------------------------------------------------------

        [Test]
        public void Hints_SitOnDryLandOfTheirRegion_OffTheRoads_ApartFromEachOther()
        {
            var sites = new List<WorldMapSite>
            {
                new WorldMapSite(BiomeType.Forest, new Vector2(0f, 0f)),
                new WorldMapSite(BiomeType.Desert, new Vector2(10f, 0f)),
                new WorldMapSite(BiomeType.Mountain, new Vector2(20f, 4f)),
                new WorldMapSite(BiomeType.Caves, new Vector2(30f, 0f)),
            };
            var paths = new List<WorldMapPath>
            {
                new WorldMapPath(0, 1, sites[0].Position, sites[1].Position),
                new WorldMapPath(1, 2, sites[1].Position, sites[2].Position),
                new WorldMapPath(2, 3, sites[2].Position, sites[3].Position),
            };
            var terrain = new WorldMapTerrain(sites, paths, WorldMapDressing.DefaultSeed, true);
            List<WorldMapPropPlacement> props = WorldMapDressingLayout.BuildProps(terrain, WorldMapDressing.DefaultSeed);

            var taken = new List<Vector2>();
            for (int site = 0; site < sites.Count; site++)
            {
                foreach (WorldSecretDefinition secret in WorldSecretCatalog.InRegion(sites[site].Region))
                {
                    Vector2 spot = WorldMapSecretHints.FindSpot(terrain, site, secret.PreferredAngle, taken, props);
                    Assert.IsTrue(terrain.IsDryLand(spot), $"{secret.Id} on dry land");
                    Assert.AreEqual(sites[site].Region, terrain.DominantRegion(spot), $"{secret.Id} in its region");
                    Assert.Greater(terrain.DistanceToPath(spot), WorldMapTerrain.PathHalfWidth + WorldMapSecretHints.HintRadius, $"{secret.Id} off the road");
                    Assert.Greater(Vector2.Distance(spot, sites[site].Position), sites[site].ClickRadius + WorldMapSecretHints.HintRadius, $"{secret.Id} off the node");
                    foreach (Vector2 other in taken)
                    {
                        Assert.Greater(Vector2.Distance(spot, other), WorldMapSecretHints.HintRadius * 2f - 0.01f, $"{secret.Id} apart from the other hints");
                    }

                    taken.Add(spot);
                }
            }

            Assert.AreEqual(8, taken.Count);
            Assert.AreEqual(taken[0], WorldMapSecretHints.FindSpot(terrain, 0, WorldSecretCatalog.All[0].PreferredAngle, new List<Vector2>(), props), "deterministic");
        }

        [Test]
        public void Hints_EveryKindAndTheMarkerBuildAMesh()
        {
            var builder = new WorldMapMeshBuilder();
            foreach (SecretHintKind kind in System.Enum.GetValues(typeof(SecretHintKind)))
            {
                builder.Clear();
                WorldMapSecretHints.AppendHint(builder, kind);
                Assert.Greater(builder.VertexCount, 0, kind.ToString());
            }

            builder.Clear();
            WorldMapSecretHints.AppendMarker(builder);
            Mesh marker = builder.ToMesh("MarkerTest");
            _created.Add(marker);
            Assert.Greater(marker.vertexCount, 0);
            Assert.Less(marker.bounds.min.y, 0f, "the diamond's lower half");
        }

        // ------------------------------------------------------------------
        // UI (pure parts)
        // ------------------------------------------------------------------

        [Test]
        public void Panel_FoundRelicsShowTheirStory_MissingOnesTheirClue()
        {
            SecretsProgress secrets = NewSecrets();
            secrets.AddRelic(RelicCatalog.ArcoMaithe);
            secrets.AddRelic(RelicCatalog.GaitaVoChico);

            List<RelicRowViewModel> rows = RelicCollectionPresenter.Build(secrets, key => key, hero => hero.ToString());
            Assert.AreEqual(RelicCatalog.Count, rows.Count);

            RelicRowViewModel bow = rows.Single(r => r.Id == RelicCatalog.ArcoMaithe);
            Assert.IsTrue(bow.Found);
            Assert.AreEqual(RelicCatalog.Get(RelicCatalog.ArcoMaithe).NameKey, bow.Name);
            Assert.AreEqual(RelicCatalog.Get(RelicCatalog.ArcoMaithe).LoreKey, bow.Body);
            Assert.AreEqual(SecretTexts.OwnerBonus, bow.Tag, "the key, formatted with the hero (no placeholder in a key)");

            RelicRowViewModel gaita = rows.Single(r => r.Id == RelicCatalog.GaitaVoChico);
            Assert.AreEqual(SecretTexts.FamilyTag, gaita.Tag);

            RelicRowViewModel missing = rows.Single(r => r.Id == RelicCatalog.FitaLavine);
            Assert.IsFalse(missing.Found);
            Assert.AreEqual(SecretTexts.UnknownName, missing.Name);
            Assert.AreEqual(RelicCatalog.Get(RelicCatalog.FitaLavine).ClueKey, missing.Body);
            Assert.IsEmpty(missing.Passive);

            Assert.AreEqual("2/12 encontradas", RelicCollectionPresenter.FormatCount(secrets, key => SecretTexts.Get(null, key)));

            // Real texts: the owner tag names the hero.
            List<RelicRowViewModel> localized = RelicCollectionPresenter.Build(secrets, key => SecretTexts.Get(null, key), hero => CharacterRegistry.Get(hero).DisplayName);
            Assert.AreEqual("Rende o dobro com Maithe", localized.Single(r => r.Id == RelicCatalog.ArcoMaithe).Tag);
        }

        [Test]
        public void PanelView_BuildsARowPerRelic()
        {
            SecretsProgress secrets = NewSecrets();
            secrets.AddRelic(RelicCatalog.ChapeuCouro);
            var view = new RelicCollectionView();
            view.Populate(RelicCollectionPresenter.Build(secrets, key => key, hero => hero.ToString()));

            Assert.AreEqual(RelicCatalog.Count, view.List.contentContainer.childCount);
            Assert.AreEqual(1, view.List.Query<VisualElement>(className: "relic-row").ToList().Count(r => !r.ClassListContains("relic-row--missing")));
            Assert.IsTrue(view.Overlay.ClassListContains(SecretsStyles.ThemeClass));
            Assert.IsFalse(view.IsShown);
        }

        [Test]
        public void MapButtons_ShowRuleAndLabels()
        {
            Assert.IsTrue(SecretsMapButtons.ShouldShowInvestigate(true, true, false, false));
            Assert.IsFalse(SecretsMapButtons.ShouldShowInvestigate(false, true, false, false));
            Assert.IsFalse(SecretsMapButtons.ShouldShowInvestigate(true, false, false, false));
            Assert.IsFalse(SecretsMapButtons.ShouldShowInvestigate(true, true, true, false));
            Assert.IsFalse(SecretsMapButtons.ShouldShowInvestigate(true, true, false, true));

            Assert.AreEqual("Investigar: Moinho velho do Vô",
                SecretsMapButtons.BuildInvestigateLabel(WorldSecretCatalog.Get(WorldSecretCatalog.CasaDeFarinha), null));
            Assert.AreEqual("Investigar: Poço de karez seco (tem guarda!)",
                SecretsMapButtons.BuildInvestigateLabel(WorldSecretCatalog.Get(WorldSecretCatalog.CacimbaSeca), null));

            var english = new LocalizationManager();
            english.SetLanguage(LanguageCode.en);
            Assert.AreEqual("Investigate: Dry karez well (guarded!)",
                SecretsMapButtons.BuildInvestigateLabel(WorldSecretCatalog.Get(WorldSecretCatalog.CacimbaSeca), english));

            SecretsProgress secrets = NewSecrets();
            secrets.AddRelic(RelicCatalog.PiaoIrmaos);
            Assert.AreEqual("Lembranças (1/12)", SecretsMapButtons.BuildRelicsEntryLabel(secrets, null));
        }

        [Test]
        public void Toast_Messages()
        {
            SecretToastController.Message reveal = SecretToastController.ForReveal(WorldSecretCatalog.JatobaOco, null);
            Assert.AreEqual("Um segredo!", reveal.Title);
            StringAssert.Contains("vaga-lumes", reveal.Body);

            SecretToastController.Message relic = SecretToastController.ForRelic(RelicCatalog.ArcoMaithe, 3, null);
            Assert.AreEqual("Lembrança encontrada!", relic.Title);
            StringAssert.Contains("Primeiro arco da Maithe (3/12)", relic.Body);
            StringAssert.Contains("+1% de chance de crítico", relic.Body);
            StringAssert.DoesNotContain(SecretTexts.Get(null, SecretTexts.ToastCollectionDone), relic.Body);
            StringAssert.Contains(SecretTexts.Get(null, SecretTexts.ToastCollectionDone), SecretToastController.ForRelic(RelicCatalog.ArcoMaithe, 12, null).Body);

            StringAssert.Contains("Cave", SecretToastController.ForArenaSpawn(BiomeType.Desert, null).Body);
            StringAssert.Contains("pedra rachada", SecretToastController.ForArenaSpawn(BiomeType.Forest, null).Body);

            VisualElement view = SecretToastController.CreateView();
            Assert.AreEqual(PickingMode.Ignore, view.pickingMode);
            Assert.IsNotNull(view.Q<Label>(SecretToastController.TitleName));
        }

        // ------------------------------------------------------------------
        // Localization
        // ------------------------------------------------------------------

        [Test]
        public void Localization_EveryKeyInBothLanguages()
        {
            var keys = new List<string>();
            foreach (RelicDefinition relic in RelicCatalog.All)
            {
                keys.AddRange(new[] { relic.NameKey, relic.LoreKey, relic.PassiveKey, relic.ClueKey });
            }

            foreach (WorldSecretDefinition secret in WorldSecretCatalog.All)
            {
                keys.AddRange(new[] { secret.NameKey, secret.RevealKey });
            }

            foreach (System.Reflection.FieldInfo field in typeof(SecretTexts).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            {
                if (field.IsLiteral && field.FieldType == typeof(string))
                {
                    keys.Add((string)field.GetRawConstantValue());
                }
            }

            var byKey = SecretTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.ToList());
            Assert.IsTrue(byKey.Values.All(list => list.Count == 1), "no key defined twice");
            foreach (string key in keys)
            {
                Assert.IsTrue(byKey.ContainsKey(key), $"{key} missing");
                LocalizationEntry entry = byKey[key][0];
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), $"{key} en");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), $"{key} pt-BR");
            }

            // Merged into a live manager, both languages resolve.
            var localization = new LocalizationManager();
            SecretTexts.EnsureRegistered(localization);
            localization.SetLanguage(LanguageCode.ptBR);
            Assert.AreEqual("Lembranças do Vô Jorge", localization.Get(SecretTexts.PanelTitle));
            localization.SetLanguage(LanguageCode.en);
            Assert.AreEqual("Grandpa Jorge's Keepsakes", localization.Get(SecretTexts.PanelTitle));
        }
    }
}
