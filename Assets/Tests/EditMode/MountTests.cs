using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Locomotion;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Mounts.Visuals;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.Mounts;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// Montarias, phase M1 (Docs/Mounts.md §13): the pure <see cref="MountRules"/> (speed cap, buffs ignored but slows
    /// kept, cooldowns, summon only out of combat / out of arena fights, damage split, tired mount that never dies),
    /// the <see cref="MountSession"/> whistle/gallop state, the starter catalog and its budget, the collection save,
    /// the whistle redeem, "Primeira Sela", the Haras entry, the procedural models, synths and the HUD view.
    /// </summary>
    public class MountTests
    {
        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            ServiceLocator.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            ServiceLocator.Clear();
        }

        // ---------------- Speed ----------------

        [Test]
        public void Speed_FastestRide_IsCappedAt216xRun_InsideBuffClamp()
        {
            Assert.AreEqual(2.16f, MountRules.MaxRideMultiplier, 1e-4f);
            Assert.AreEqual(2.16f, MountRules.MountedSpeedMultiplier(1f, 1.80f, true, false), 1e-4f);
            // A species above the table is clamped to ×1.80.
            Assert.AreEqual(2.16f, MountRules.MountedSpeedMultiplier(1f, 3f, true, false), 1e-4f);
            Assert.LessOrEqual(MountRules.MaxRideMultiplier, MountRules.MaxMoveMultiplier);
            Assert.AreEqual(11.88f, MountRules.HeroRunSpeed * MountRules.MaxRideMultiplier, 0.01f);
        }

        [Test]
        public void Speed_BuffsAboveOneAreIgnored_SlowsApply_RootStops()
        {
            float plain = MountRules.MountedSpeedMultiplier(1f, 1.45f, false, false);
            Assert.AreEqual(1.45f, plain, 1e-4f);
            Assert.AreEqual(plain, MountRules.MountedSpeedMultiplier(1.5f, 1.45f, false, false), 1e-4f, "Passo do Vento must not stack");
            Assert.AreEqual(plain, MountRules.MountedSpeedMultiplier(2.5f, 1.45f, false, false), 1e-4f, "dash must not stack");
            Assert.AreEqual(1.45f * 0.5f, MountRules.MountedSpeedMultiplier(0.5f, 1.45f, false, false), 1e-4f, "slows still apply");
            Assert.AreEqual(MountRules.MinMoveMultiplier, MountRules.MountedSpeedMultiplier(0.1f, 1.30f, false, true), 1e-4f);
            Assert.AreEqual(0f, MountRules.MountedSpeedMultiplier(0f, 1.8f, true, false), "rooted stays rooted");
        }

        [Test]
        public void Speed_SafeZone_TrotsAtRunSpeed_NoGallop()
        {
            Assert.AreEqual(1f, MountRules.MountedSpeedMultiplier(1f, 1.5f, true, true), 1e-4f);
            Assert.IsFalse(MountRules.CanGallop(MountPlace.SafeZone, 100f, false));
            Assert.IsTrue(MountRules.CanGallop(MountPlace.WorldMap, 100f, false));
            Assert.AreEqual(5.5f * 1.1f, MountRules.ServerSpeedCap(1.5f, true, 1f, true), 1e-3f);
        }

        // ---------------- Summon ----------------

        [Test]
        public void Summon_OnlyOutOfCombat_NotInArenaFight_AfterRecall()
        {
            Assert.AreEqual(MountSummonBlock.None, MountRules.CanSummon(MountPlace.WorldMap, true, false, false, float.PositiveInfinity, 0f));
            Assert.AreEqual(MountSummonBlock.NoMount, MountRules.CanSummon(MountPlace.WorldMap, false, false, false, 99f, 0f));
            Assert.AreEqual(MountSummonBlock.AlreadyMounted, MountRules.CanSummon(MountPlace.WorldMap, true, true, false, 99f, 0f));
            Assert.AreEqual(MountSummonBlock.NotHere, MountRules.CanSummon(MountPlace.ArenaFight, true, false, false, 99f, 0f));
            Assert.AreEqual(MountSummonBlock.NotHere, MountRules.CanSummon(MountPlace.Indoors, true, false, false, 99f, 0f));
            Assert.AreEqual(MountSummonBlock.Tired, MountRules.CanSummon(MountPlace.ArenaFree, true, false, true, 99f, 0f));
            Assert.AreEqual(MountSummonBlock.InCombat, MountRules.CanSummon(MountPlace.ArenaFree, true, false, false, 4.9f, 0f));
            Assert.AreEqual(MountSummonBlock.None, MountRules.CanSummon(MountPlace.ArenaFree, true, false, false, 5f, 0f));
            Assert.AreEqual(MountSummonBlock.RecallCooldown, MountRules.CanSummon(MountPlace.SafeZone, true, false, false, 99f, 3f));
        }

        [Test]
        public void Summon_Interruptions_AndPlaces()
        {
            Assert.IsTrue(MountRules.SummonInterrupted(true, false, false, false));
            Assert.IsTrue(MountRules.SummonInterrupted(false, false, false, true));
            Assert.IsFalse(MountRules.SummonInterrupted(false, false, false, false));
            Assert.AreEqual(MountPlace.ArenaFight, MountRules.ResolvePlace(false, false, true, false));
            Assert.AreEqual(MountPlace.ArenaFree, MountRules.ResolvePlace(false, false, false, false));
            Assert.AreEqual(MountPlace.SafeZone, MountRules.ResolvePlace(true, true, false, false));
            Assert.AreEqual(MountPlace.Indoors, MountRules.ResolvePlace(true, true, false, true));
            Assert.IsTrue(MountRules.ShouldForceDismount(MountPlace.ArenaFight, false), "fights start on foot");
            Assert.IsTrue(MountRules.ShouldForceDismount(MountPlace.SafeZone, true), "combat at the city border drops the rider");
            Assert.IsFalse(MountRules.ShouldForceDismount(MountPlace.WorldMap, true));
        }

        [Test]
        public void Session_Whistle_TakesOneAndAHalfSeconds_AndReleasingCancels()
        {
            var session = new MountSession();
            Assert.AreEqual(MountSummonBlock.None, session.BeginWhistle(MountPlace.WorldMap, true, false, 99f, 0f));
            Assert.IsFalse(session.TickWhistle(1f, true));
            Assert.AreEqual(1f / 1.5f, session.WhistleProgress, 1e-3f);
            Assert.IsTrue(session.TickWhistle(0.5f, true));
            Assert.IsFalse(session.IsWhistling);

            session.BeginWhistle(MountPlace.WorldMap, true, false, 99f, 0f);
            Assert.IsFalse(session.TickWhistle(0.5f, false));
            Assert.IsFalse(session.IsWhistling, "letting go breaks the whistle");
        }

        [Test]
        public void Session_DismountStartsTheRecallCooldown()
        {
            var session = new MountSession();
            session.Mount(110f);
            Assert.IsTrue(session.IsMounted);
            session.Dismount(100f);
            Assert.AreEqual(10f, session.RecallRemaining(100f), 1e-4f);
            Assert.AreEqual(MountSummonBlock.RecallCooldown, session.BeginWhistle(MountPlace.WorldMap, true, false, 99f, 105f));
            Assert.AreEqual(MountSummonBlock.None, session.BeginWhistle(MountPlace.WorldMap, true, false, 99f, 110f));
        }

        // ---------------- Gallop ----------------

        [Test]
        public void Gallop_SpendsMountStamina_RecoversWithHysteresis()
        {
            bool exhausted = false;
            float stamina = MountRules.StepGallopStamina(100f, 100f, true, true, 1f, ref exhausted, out bool galloping);
            Assert.IsTrue(galloping);
            Assert.AreEqual(88f, stamina, 1e-3f);

            stamina = MountRules.StepGallopStamina(5f, 100f, true, true, 1f, ref exhausted, out _);
            Assert.AreEqual(0f, stamina);
            Assert.IsTrue(exhausted);

            stamina = MountRules.StepGallopStamina(stamina, 100f, true, true, 1f, ref exhausted, out galloping);
            Assert.IsFalse(galloping, "empty: trots while it refills");
            Assert.AreEqual(15f, stamina, 1e-3f);
            stamina = MountRules.StepGallopStamina(stamina, 100f, true, true, 1f, ref exhausted, out _);
            Assert.IsFalse(exhausted, "back at 25%");
            Assert.AreEqual(100f / 12f, MountRules.GallopSeconds(100f), 1e-3f);
        }

        [Test]
        public void Session_GallopOnlyWhenMountedAndAllowed()
        {
            var session = new MountSession();
            session.Mount(100f);
            session.Tick(0.5f, true, MountPlace.SafeZone);
            Assert.IsFalse(session.IsGalloping);
            Assert.AreEqual(1f, session.SpeedMultiplier(1f, 1.5f, MountPlace.SafeZone), 1e-4f);
            session.Tick(0.5f, true, MountPlace.WorldMap);
            Assert.IsTrue(session.IsGalloping);
            Assert.AreEqual(1.8f, session.SpeedMultiplier(1.4f, 1.5f, MountPlace.WorldMap), 1e-4f);
            Assert.AreEqual(94f, session.Stamina, 1e-3f);
        }

        // ---------------- Damage / tired ----------------

        [Test]
        public void Damage_HalfGoesToTheMount_NeverMoreThanItHas()
        {
            float left = MountRules.SplitDamage(40f, 0.5f, 125f, out float toHero, out float toMount);
            Assert.AreEqual(20f, toHero, 1e-4f);
            Assert.AreEqual(20f, toMount, 1e-4f);
            Assert.AreEqual(105f, left, 1e-4f);

            left = MountRules.SplitDamage(40f, 0.5f, 5f, out toHero, out toMount);
            Assert.AreEqual(5f, toMount, 1e-4f);
            Assert.AreEqual(35f, toHero, 1e-4f);
            Assert.IsTrue(MountRules.BecomesTired(left));
        }

        [Test]
        public void Collection_TiredMountRestsTwoFights_AndComesBackFull()
        {
            var collection = NewCollection();
            OwnedMount pony = collection.Grant(MountCatalog.Pony, MountCollection.SourceShop);
            Assert.AreEqual(125f, MountCollection.HealthOf(pony), 1e-3f);
            bool tired = collection.SetHealth(pony, 0f);
            Assert.IsTrue(tired);
            Assert.IsTrue(pony.IsTired);
            Assert.AreEqual(0f, MountCollection.HealthOf(pony), "never dies, just tired");

            collection.RestAfterEncounter();
            Assert.IsTrue(pony.IsTired);
            collection.RestAfterEncounter();
            Assert.IsFalse(pony.IsTired);
            Assert.AreEqual(125f, MountCollection.HealthOf(pony), 1e-3f);
        }

        // ---------------- Cooldowns / abilities ----------------

        [Test]
        public void Ability_Cooldown_AndNoAbilityInTheCity()
        {
            Assert.AreEqual(12f, MountRules.AbilityCooldownSeconds(12f, 1, 0, false), 1e-4f);
            Assert.AreEqual(12f * 0.905f, MountRules.AbilityCooldownSeconds(12f, 20, 0, false), 1e-4f);
            Assert.AreEqual(12f * 0.95f * 0.85f, MountRules.AbilityCooldownSeconds(12f, 1, 2, true), 1e-4f);

            var session = new MountSession();
            Assert.IsFalse(session.TryUseAbility(0f, MountPlace.WorldMap, 12f), "not mounted");
            session.Mount(100f);
            Assert.IsFalse(session.TryUseAbility(0f, MountPlace.SafeZone, 12f));
            Assert.IsTrue(session.TryUseAbility(0f, MountPlace.WorldMap, 12f));
            Assert.IsFalse(session.TryUseAbility(11f, MountPlace.WorldMap, 12f));
            Assert.AreEqual(1f, session.AbilityRemaining(11f), 1e-4f);
            Assert.IsTrue(session.TryUseAbility(12f, MountPlace.WorldMap, 12f));
        }

        [Test]
        public void Ability_FearIgnoresBosses_StunHalvesLikeStunRules()
        {
            Assert.AreEqual(0f, MountRules.EffectDurationOn(3f, true, 0f));
            Assert.AreEqual(3f, MountRules.EffectDurationOn(3f, false, 0f));
            Assert.AreEqual(0.6f, MountRules.EffectDurationOn(1.2f, true, 0.5f), 1e-4f);
            Assert.AreEqual(0.8f, MountRules.MountedAttackMultiplier(false), 1e-4f);
        }

        // ---------------- Catalog ----------------

        [Test]
        public void Catalog_ThreeStarters_HitTheCommonBudget()
        {
            Assert.AreEqual(3, MountCatalog.Starters.Count);
            Assert.AreEqual(3 + MountWaveCatalog.Specs.Count, MountCatalog.Specs.Count);
            foreach (string id in new[] { MountCatalog.Pony, MountCatalog.BigDog, MountCatalog.PackDonkey })
            {
                MountDefinition definition = MountCatalog.Get(id);
                Assert.IsNotNull(definition, id);
                Assert.AreEqual(ItemRarity.Common, definition.Rarity);
                Assert.AreEqual(MountRules.BudgetFor(ItemRarity.Common), definition.BudgetPoints, 1e-3f, id);
                Assert.IsNotNull(definition.Ability, id);
                Assert.IsFalse(definition.Ability.AllowedInSafeZone);
                Assert.That(definition.SpeedMultiplier, Is.InRange(MountRules.MinSpeedMultiplier, MountRules.MaxSpeedMultiplier));
                Assert.IsNotNull(ItemDatabase.GetById(definition.WhistleItemId), definition.WhistleItemId + " asset missing");
            }

            Assert.AreEqual(MountAbilityKind.Dash, MountCatalog.Get(MountCatalog.Pony).Ability.Kind);
            Assert.AreEqual(MountAbilityKind.LootMagnet, MountCatalog.Get(MountCatalog.BigDog).Ability.Kind);
            Assert.AreEqual(MountAbilityKind.Fear, MountCatalog.Get(MountCatalog.PackDonkey).Ability.Kind);
            Assert.AreEqual(0f, MountCatalog.Get(MountCatalog.PackDonkey).Ability.BossMultiplier);
        }

        [Test]
        public void HarasKeeper_IsSeuTonico_NotSeuZeCacador()
        {
            // Srosh (resident id tonico_haras, Vô Jorge's old road friend) keeps the Haras; the hunter (Hunting) is someone else.
            CityResidentSpec keeper = CityCatalog.AllResidents().Single(r => r.ShopId == CityShopCatalog.HarasVila);
            Assert.AreEqual("tonico_haras", keeper.ResidentId);
            Assert.IsFalse(CityCatalog.AllResidents().Any(r => r.ResidentId == "ze_haras"));

            LocalizationEntry name = Rapadura.UI.WorldMap.CityTexts.Entries.Single(e => e.Key == keeper.NameKey);
            Assert.AreEqual("Srosh, o Tratador", name.PtBR);
            Assert.AreEqual("Srosh the Stablekeeper", name.En);
            Assert.IsFalse(Rapadura.UI.WorldMap.CityTexts.Entries.Any(e => e.Key.StartsWith("city.resident.ze_haras", System.StringComparison.Ordinal)));
            Assert.IsFalse(MountTexts.Entries.Any(e => (e.PtBR ?? string.Empty).Contains("Seu Zé") || (e.En ?? string.Empty).Contains("Seu Zé")), "mount texts name Seu Tonico");
        }

        [Test]
        public void Haras_SellsTheThreeWhistles_InTheVila()
        {
            CityShopSpec haras = CityShopCatalog.Get(CityShopCatalog.HarasVila);
            Assert.IsNotNull(haras);
            Assert.AreEqual(FirstSaddleQuest.HarasShopId, haras.ShopId);
            CollectionAssert.Contains(CityCatalog.ForRegion(Rapadura.Gameplay.World.Biomes.BiomeType.Forest).ShopIds, CityShopCatalog.HarasVila);
            var harasMounts = MountCatalog.Starters.Concat(new[] { MountCatalog.FindSpec(MountWaveCatalog.Capybara), MountCatalog.FindSpec(MountWaveCatalog.PintoHorse) });
            foreach (MountSpec spec in harasMounts)
            {
                CityShopEntry entry = haras.Entries.FirstOrDefault(e => e.ItemId == spec.WhistleItemId);
                Assert.IsNotNull(entry, spec.MountId);
                Assert.AreEqual(spec.ShopPrice, entry.Price);
                Assert.AreEqual(ItemDatabase.GetById(spec.WhistleItemId).Value, entry.Price);
            }
        }

        // ---------------- Collection / save ----------------

        [Test]
        public void Collection_SaveRoundTrip_IsPerAccount()
        {
            var collection = NewCollection();
            collection.Grant(MountCatalog.Pony, MountCollection.SourceQuest);
            OwnedMount dog = collection.Grant(MountCatalog.BigDog, MountCollection.SourceShop);
            collection.SetActive(dog.guid);
            collection.SetHealth(dog, 30f);

            string json = JsonUtility.ToJson(collection.CaptureState());
            var restored = NewCollection();
            restored.RestoreState(JsonUtility.FromJson<MountCollectionSaveState>(json));

            Assert.AreEqual(2, restored.Count);
            Assert.AreEqual(MountCatalog.BigDog, restored.Active.mountId);
            Assert.AreEqual(30f, MountCollection.HealthOf(restored.Active), 1e-3f);
            CollectionAssert.AreEquivalent(new[] { MountCatalog.Pony, MountCatalog.BigDog }, restored.CodexSeen.ToArray());

            restored.ResetForNewSession();
            Assert.AreEqual(0, restored.Count);
            Assert.IsNull(restored.Active);
        }

        [Test]
        public void Collection_FirstMountBecomesActive_WhistleRedeems()
        {
            var collection = NewCollection();
            Assert.IsNull(collection.RedeemWhistle("item_herb"));
            OwnedMount donkey = collection.RedeemWhistle(MountCatalog.PackDonkeyWhistle);
            Assert.AreEqual(MountCatalog.PackDonkey, donkey.mountId);
            Assert.AreSame(donkey, collection.Active);
            collection.Grant(MountCatalog.Pony, "shop");
            Assert.AreSame(donkey, collection.Active, "later mounts don't steal the active slot");
        }

        // ---------------- Primeira Sela ----------------

        [Test]
        public void FirstSaddle_StartsAfterTheForest_VisitGivesThePony()
        {
            var quests = new QuestManager();
            quests.Initialize();
            ServiceLocator.Register(quests);
            var collection = NewCollection();
            FirstSaddleQuest.Install(quests, collection);
            try
            {
                var map = new WorldMapProgress();
                FirstSaddleQuest.Evaluate(map);
                Assert.IsFalse(quests.IsActive(FirstSaddleQuest.QuestId), "not before the Lobo Alfa");

                map.MarkRegionCleared(Rapadura.Gameplay.World.Biomes.BiomeType.Forest);
                FirstSaddleQuest.Evaluate(map);
                Assert.IsTrue(quests.IsActive(FirstSaddleQuest.QuestId));

                EventBus.Publish(new ShopOpenedEvent(FirstSaddleQuest.HarasShopId));
                Assert.IsTrue(quests.IsCompleted(FirstSaddleQuest.QuestId));
                Assert.AreEqual(1, collection.Count);
                Assert.AreEqual(MountCatalog.Pony, collection.Active.mountId);
                Assert.IsFalse(FirstSaddleQuest.OwesPony(true, collection.Mounts));

                FirstSaddleQuest.Evaluate(map);
                Assert.AreEqual(1, collection.Count, "given once");
            }
            finally
            {
                FirstSaddleQuest.Uninstall();
                quests.Shutdown();
            }
        }

        // ---------------- Models ----------------

        [Test]
        public void Models_AreJointedQuadrupeds_WithinBudget_AndBindToTheGaitEngine()
        {
            foreach (MountVisualRecipe recipe in System.Enum.GetValues(typeof(MountVisualRecipe)))
            {
                ProceduralEnemyBlueprint blueprint = MountModels.CreateBlueprint(recipe);
                try
                {
                    Assert.LessOrEqual(blueprint.TriangleCount, MountModels.TriangleBudget, recipe.ToString());
                    bool biped = MountRecipe.ShapeFor(recipe).Biped;
                    foreach (string leg in biped ? new[] { "HL", "HR" } : new[] { "FL", "FR", "HL", "HR" })
                    {
                        Assert.GreaterOrEqual(blueprint.IndexOf("Leg_" + leg), 0);
                        Assert.GreaterOrEqual(blueprint.IndexOf("Shin_" + leg), 0);
                        Assert.GreaterOrEqual(blueprint.IndexOf("Foot_" + leg), 0);
                    }

                    Assert.GreaterOrEqual(blueprint.IndexOf(MountParts.Seat), 0);
                    Assert.GreaterOrEqual(blueprint.IndexOf(MountParts.Head), 0);

                    var root = new GameObject("MountTest");
                    try
                    {
                        GameObject visual = Rapadura.Gameplay.Pets.Visuals.CarameloModel.Instantiate(blueprint, null, root.transform);
                        var legs = visual.AddComponent<ProceduralLegLocomotion>();
                        Assert.IsTrue(legs.Bind(root.transform, visual.transform, 0f), recipe.ToString());
                        Assert.AreEqual(biped ? 2 : 4, legs.LegCount, recipe.ToString());
                        Assert.AreEqual(biped ? LegGaitStyle.Biped : LegGaitStyle.Quadruped, legs.DetectedStyle, recipe.ToString());
                    }
                    finally
                    {
                        Object.DestroyImmediate(root);
                    }
                }
                finally
                {
                    blueprint.DestroyMeshes();
                }
            }

            Assert.Greater(MountRecipe.ShapeFor(MountVisualRecipe.Donkey).EarLength, MountRecipe.ShapeFor(MountVisualRecipe.Pony).EarLength);
            Assert.IsTrue(MountRecipe.ShapeFor(MountVisualRecipe.Donkey).Cowbell);
        }

        [Test]
        public void RiderLift_PutsTheHipsOnTheSaddle()
        {
            Assert.AreEqual(0.2f, MountRiderPose.LiftFor(0.95f, 0.75f), 1e-4f);
            Assert.AreEqual(0f, MountRiderPose.LiftFor(0.5f, 0.75f));
        }

        // ---------------- Audio ----------------

        [Test]
        public void Synths_ProduceShortNonSilentClips_AndCuesAreAddedOnce()
        {
            foreach (float[] samples in new[] { MountAudio.GenerateWhistle(), MountAudio.GenerateHoof(), MountAudio.GenerateCowbell(), MountAudio.GenerateBray() })
            {
                Assert.Greater(samples.Length, 100);
                Assert.Less(samples.Length, MountAudio.SampleRate * 2);
                float peak = samples.Max(Mathf.Abs);
                Assert.That(peak, Is.InRange(0.3f, 1f));
            }

            var map = ScriptableObject.CreateInstance<Rapadura.Audio.SfxEventMap>();
            try
            {
                int added = MountAudio.EnsureCues(map);
                Assert.AreEqual(MountAudio.BuildCues(false).Count, added);
                Assert.AreEqual(0, MountAudio.EnsureCues(map));
                Assert.IsTrue(map.TryGetCue(MountAudio.Whistle, out var whistle));
                Assert.IsNotNull(whistle.clip);
            }
            finally
            {
                Object.DestroyImmediate(map);
            }
        }

        // ---------------- Texts / HUD ----------------

        [Test]
        public void Texts_CoverCatalog_HarasAndQuest()
        {
            var keys = new HashSet<string>(MountTexts.Entries.Select(e => e.Key));
            foreach (string id in MountCatalog.MountIds)
            {
                MountDefinition definition = MountCatalog.Get(id);
                Assert.IsTrue(keys.Contains(definition.DisplayNameKey), definition.DisplayNameKey);
                Assert.IsTrue(keys.Contains(definition.Ability.NameKey), definition.Ability.NameKey);
                Assert.IsTrue(keys.Contains(definition.Ability.DescriptionKey));
            }

            Assert.IsTrue(keys.Contains(CityShopCatalog.Get(CityShopCatalog.HarasVila).NameKey));
            Assert.IsTrue(keys.Contains(FirstSaddleQuest.Definition.TitleKey));
            foreach (LocalizationEntry entry in MountTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), entry.Key);
            }
        }

        [Test]
        public void Hud_ShowsOnlyWhereRidingIsPossible_AndBuildsItsTree()
        {
            Assert.IsFalse(MountHudView.ShouldShow(false, false, true, MountPlace.WorldMap, false));
            Assert.IsFalse(MountHudView.ShouldShow(true, true, true, MountPlace.WorldMap, false));
            Assert.IsFalse(MountHudView.ShouldShow(true, false, true, MountPlace.ArenaFight, false));
            Assert.IsTrue(MountHudView.ShouldShow(true, false, true, MountPlace.SafeZone, false));
            Assert.AreEqual(MountTexts.HudDismount, MountHudView.ButtonKey(true, false));
            Assert.AreEqual(MountTexts.HudWhistling, MountHudView.ButtonKey(false, true));

            var view = new MountHudView(onMap: true, reducedMotion: true);
            Assert.IsTrue(view.Root.ClassListContains(MountHudView.MapModifierClass));
            Assert.IsNotNull(view.Root.Q<Button>(MountHudView.ButtonName));
            Assert.IsNotNull(view.Root.Q<Button>(MountHudView.AbilityButtonName));
            Assert.IsNotNull(view.Root.Q<Label>(MountHudView.NoticeName));
            view.Refresh(null, true);
            Assert.AreEqual(DisplayStyle.None, view.MountButton.parent.style.display.value);
        }

        [Test]
        public void Uss_ImportsTheThemeAndHasNoTransitions()
        {
            string uss = File.ReadAllText(Path.Combine(Application.dataPath, "Resources/UI/Mounts.uss"));
            StringAssert.Contains("@import url(\"/Assets/UI/Common/RapaduraTheme.uss\")", uss);
            StringAssert.DoesNotContain("transition", uss.Replace("No transitions", string.Empty));
        }

        // ================= M2: care, level, affection, codex, cosmetics, stable, input =================

        [Test]
        public void Hunger_FallsOnePerMinute_AndSlowsTheRide()
        {
            Assert.AreEqual(99f, MountCareRules.DecaySatiety(100f, 60f), 1e-4f);
            Assert.IsTrue(MountCareRules.IsHungry(29.9f));
            Assert.IsFalse(MountCareRules.IsHungry(30f));
            Assert.AreEqual(1.15f, MountRules.MountedSpeedMultiplier(1f, 1.5f, true, false, hungry: true), 1e-4f, "×1.15 and no gallop");
            Assert.IsFalse(MountRules.CanGallop(MountPlace.WorldMap, 100f, false, hungry: true));

            var session = new MountSession();
            session.Mount(100f);
            session.Tick(0.5f, true, MountPlace.WorldMap, hungry: true);
            Assert.IsFalse(session.IsGalloping);
        }

        [Test]
        public void Food_RespectsDiets_AndTheTable()
        {
            MountFoodSpec capim = MountCareRules.FindFood("capim");
            MountFoodSpec carne = MountCareRules.FindFood("carne");
            MountFoodSpec rapadura = MountCareRules.FindFood("rapadura");
            Assert.IsTrue(MountCareRules.CanEat(MountCatalog.Get(MountCatalog.Pony).Diet, capim));
            Assert.IsFalse(MountCareRules.CanEat(MountCatalog.Get(MountCatalog.Pony).Diet, carne));
            Assert.IsTrue(MountCareRules.CanEat(MountCatalog.Get(MountCatalog.BigDog).Diet, carne));
            Assert.IsFalse(MountCareRules.CanEat(MountCatalog.Get(MountCatalog.BigDog).Diet, capim));
            Assert.AreEqual(5, rapadura.Affection, "the favourite treat");
            Assert.AreEqual(2, capim.ItemQuantity);
            Assert.IsNotNull(ItemDatabase.GetById(MountCareRules.FeedItemId), "Ração do Haras asset");
            Assert.IsTrue(CityShopCatalog.Get(CityShopCatalog.HarasVila).Entries.Any(e => e.ItemId == MountCareRules.FeedItemId && e.Price == 3));
        }

        [Test]
        public void Level_CurveAndCap()
        {
            Assert.AreEqual(80, MountCareRules.XpToNext(1));
            Assert.AreEqual(4940, MountCareRules.TotalXpToMax());
            int level = 1, xp = 0;
            Assert.AreEqual(1, MountCareRules.AddXp(ref level, ref xp, 90));
            Assert.AreEqual(2, level);
            Assert.AreEqual(10, xp);
            Assert.AreEqual(18, MountCareRules.AddXp(ref level, ref xp, 100000));
            Assert.AreEqual(MountRules.MaxLevel, level);
            Assert.AreEqual(0, xp);
            Assert.AreEqual(0, MountCareRules.AddXp(ref level, ref xp, 50));
            Assert.AreEqual(125f * 1.95f, MountRules.MaxHealthAt(125f, 20), 1e-3f);
        }

        [Test]
        public void Affection_HeartsPerks_AndFavouriteBonus()
        {
            float fraction = 0f;
            int affection = 0;
            for (int i = 0; i < 4; i++)
            {
                affection = MountCareRules.AddAffection(affection, ref fraction, 1, isFavorite: true);
            }

            Assert.AreEqual(5, affection, "×1.25 with carry-over");
            Assert.AreEqual(3, MountCareRules.Hearts(399));
            Assert.AreEqual(5, MountCareRules.Hearts(9999));
            Assert.AreEqual(0.75f, MountCareRules.WhistleSeconds(4), 1e-4f);
            Assert.AreEqual(1.5f, MountCareRules.WhistleSeconds(3), 1e-4f);
            Assert.AreEqual(1.05f, MountCareRules.HealthMultiplier(5), 1e-4f);
            Assert.IsTrue(MountCareRules.SaysLineOnMount(0, true));
            Assert.IsTrue(MountCareRules.SaysLineOnMount(3, false));
            Assert.IsFalse(MountCareRules.SaysLineOnMount(2, false));
        }

        [Test]
        public void Favourite_LavineOnThePony_RechargesFaster()
        {
            MountDefinition pony = MountCatalog.Get(MountCatalog.Pony);
            var mount = new OwnedMount { mountId = MountCatalog.Pony };
            Assert.AreEqual(12f, MountCareRules.AbilityCooldown(pony, mount, Rapadura.Gameplay.Characters.CharacterId.Maithe), 1e-4f);
            Assert.AreEqual(12f * 0.85f - 3f, MountCareRules.AbilityCooldown(pony, mount, Rapadura.Gameplay.Characters.CharacterId.Lavine), 1e-4f);
            Assert.IsTrue(MountCareRules.IsFavorite(pony, Rapadura.Gameplay.Characters.CharacterId.Lavine));
            Assert.IsFalse(MountCareRules.IsFavorite(MountCatalog.Get(MountCatalog.BigDog), Rapadura.Gameplay.Characters.CharacterId.Lavine));
            var keys = new HashSet<string>(MountTexts.Entries.Select(e => e.Key));
            Assert.IsTrue(keys.Contains(pony.FavoriteLineKey));
            foreach (Rapadura.Gameplay.Characters.CharacterId hero in System.Enum.GetValues(typeof(Rapadura.Gameplay.Characters.CharacterId)))
            {
                Assert.IsTrue(keys.Contains(MountTexts.RiderLineKey(hero)), hero.ToString());
            }
        }

        [Test]
        public void Collection_RidingFeedingCuddling()
        {
            var collection = NewCollection();
            OwnedMount dog = collection.Grant(MountCatalog.BigDog, "shop");
            collection.AddRideDistance(dog, 1000f, false);
            Assert.AreEqual(1, dog.level);
            Assert.AreEqual(50, dog.xp);
            Assert.AreEqual(1, dog.affection, "+1 per km");

            dog.satiety = 10f;
            Assert.IsFalse(collection.Feed(dog, MountCareRules.FindFood("capim"), false), "carnivore");
            Assert.IsTrue(collection.Feed(dog, MountCareRules.FindFood("carne"), false));
            Assert.AreEqual(50f, dog.satiety, 1e-3f);
            Assert.AreEqual(1, dog.level, "50 + 15 XP is still below the 80 of level 1");
            Assert.AreEqual(65, dog.xp);
            Assert.AreEqual(2, dog.affection);
        }

        [Test]
        public void Collection_CuddleCooldown_DecayAndHungerEdge()
        {
            var collection = NewCollection();
            OwnedMount pony = collection.Grant(MountCatalog.Pony, "shop");
            Assert.IsTrue(collection.TryCuddle(pony, 0f, false));
            Assert.IsFalse(collection.TryCuddle(pony, 10f, false));
            Assert.IsTrue(collection.TryCuddle(pony, 30f, false));
            Assert.AreEqual(2, pony.affection);

            pony.satiety = 30.5f;
            Assert.IsTrue(collection.DecaySatiety(pony, 60f), "becomes hungry now");
            Assert.IsFalse(collection.DecaySatiety(pony, 60f), "announced once");
        }

        [Test]
        public void Cosmetics_OnlyOwnedCanBeWorn_AndAreSaved()
        {
            var collection = NewCollection();
            OwnedMount pony = collection.Grant(MountCatalog.Pony, "shop");
            Assert.IsFalse(collection.Equip(pony, MountCosmeticSlot.Saddle, "saddle_chita"));
            Assert.IsTrue(collection.GrantCosmetic("saddle_chita"));
            Assert.IsFalse(collection.Equip(pony, MountCosmeticSlot.Blanket, "saddle_chita"), "wrong slot");
            Assert.IsTrue(collection.Equip(pony, MountCosmeticSlot.Saddle, "saddle_chita"));

            string json = JsonUtility.ToJson(collection.CaptureState());
            var restored = NewCollection();
            restored.RestoreState(JsonUtility.FromJson<MountCollectionSaveState>(json));
            Assert.IsTrue(restored.OwnsCosmetic("saddle_chita"));
            Assert.AreEqual("saddle_chita", restored.Active.saddleId);

            ProceduralEnemyBlueprint dressed = MountModels.CreateBlueprint(MountVisualRecipe.Pony, EnemySwatch.Gold, EnemySwatch.GolemSnow);
            dressed.DestroyMeshes();
        }

        [Test]
        public void Feeding_PaysItemsOrDishes()
        {
            var bag = new Dictionary<string, int> { { "item_fiber", 3 } };
            System.Func<string, int> count = id => bag.TryGetValue(id, out int n) ? n : 0;
            System.Func<string, int, bool> remove = (id, n) => { bag[id] = count(id) - n; return true; };
            MountDiet herbivore = MountDiet.Herbivora;
            Assert.AreEqual(MountFeedBlock.None, MountFeeding.TryPay(herbivore, MountCareRules.FindFood("capim"), count, remove, _ => true));
            Assert.AreEqual(1, bag["item_fiber"]);
            Assert.AreEqual(MountFeedBlock.MissingItems, MountFeeding.Check(herbivore, MountCareRules.FindFood("capim"), count, _ => true));
            Assert.AreEqual(MountFeedBlock.Diet, MountFeeding.Check(herbivore, MountCareRules.FindFood("carne"), count, _ => true));
            Assert.AreEqual(MountFeedBlock.UnknownDish, MountFeeding.Check(herbivore, MountCareRules.FindFood("cuscuz"), count, _ => false));
        }

        [Test]
        public void Stable_StallsByHouse_AndDailyWater()
        {
            Assert.AreEqual(0, MountCareRules.StallsFor(3, false, true));
            Assert.AreEqual(2, MountCareRules.StallsFor(1, true, false));
            Assert.AreEqual(9, MountCareRules.StallsFor(5, true, true));

            var collection = NewCollection();
            long day = 7;
            collection.DayIndex = () => day;
            OwnedMount active = collection.Grant(MountCatalog.Pony, "shop");
            OwnedMount stalled = collection.Grant(MountCatalog.BigDog, "shop");
            stalled.satiety = 50f;
            var stable = new MountStableInfo(true, 1, 2, true, true);
            collection.ApplyStable(stable);
            collection.ApplyStable(stable);
            Assert.AreEqual(70f, stalled.satiety, 1e-3f, "+10 per fight");
            Assert.AreEqual(1, stalled.affection, "water once a day");
            Assert.AreEqual(100f, active.satiety, 1e-3f, "the active mount is not in a stall");
            day = 8;
            collection.ApplyStable(stable);
            Assert.AreEqual(2, stalled.affection);
        }

        [Test]
        public void Codex_ListsThe30_WithTexts()
        {
            Assert.AreEqual(30, MountCodex.Count);
            Assert.AreEqual(30, MountCodex.Entries.Select(e => e.MountId).Distinct().Count());
            var keys = new HashSet<string>(MountTexts.Entries.Select(e => e.Key));
            foreach (MountCodexEntry entry in MountCodex.Entries)
            {
                Assert.IsTrue(keys.Contains(entry.NameKey), entry.NameKey);
                Assert.IsTrue(keys.Contains(entry.HintKey), entry.HintKey);
            }

            Assert.AreEqual(MountCatalog.Specs.Count, MountCodex.Entries.Count(e => e.IsAvailable));
            foreach (MountCosmeticSpec cosmetic in MountCosmetics.All)
            {
                Assert.IsTrue(keys.Contains(cosmetic.NameKey), cosmetic.NameKey);
            }

            foreach (MountFoodSpec food in MountCareRules.Foods)
            {
                Assert.IsTrue(keys.Contains(food.NameKey), food.NameKey);
            }
        }

        [Test]
        public void Input_ActionsInTheAsset_MatchTheMapBindings_AndAreRebindable()
        {
            string json = File.ReadAllText(Path.Combine(Application.dataPath, "InputActions/PlayerControls.inputactions"));
            foreach (string id in new[] { MountInput.MountKeyboardId, MountInput.MountGamepadId, MountInput.AbilityKeyboardId, MountInput.AbilityGamepadId })
            {
                StringAssert.Contains(id, json);
            }

            StringAssert.Contains("\"name\": \"" + MountController.MountActionName + "\"", json);
            StringAssert.Contains("\"name\": \"" + MountController.AbilityActionName + "\"", json);
            Assert.IsTrue(Rapadura.UI.Menus.SettingsMenuTexts.Rows.Any(r => r.ActionName == MountController.MountActionName));
            Assert.IsTrue(Rapadura.UI.Menus.SettingsMenuTexts.Rows.Any(r => r.ActionName == MountController.AbilityActionName));

            UnityEngine.InputSystem.InputActionMap map = MountInput.CreateMap(out var mount, out var ability);
            try
            {
                Assert.AreEqual(2, mount.bindings.Count);
                Assert.AreEqual(MountInput.MountKeyboardId, mount.bindings[0].id.ToString());
                Assert.AreEqual(MountController.DefaultAbilityBinding, ability.bindings[0].path);
            }
            finally
            {
                map.Dispose();
            }
        }

        [Test]
        public void StablePanel_BuildsHidden()
        {
            var panel = new MountStablePanel(reducedMotion: true);
            Assert.IsFalse(panel.IsOpen);
            Assert.AreEqual(MountStablePanel.PanelName, panel.Root.name);
            Assert.IsFalse(MountStablePanel.IsInVila(null));
        }

        private static MountCollection NewCollection()
        {
            int n = 0;
            return new MountCollection { GuidFactory = () => "g" + (++n), UtcNow = () => 0L };
        }
    }
}
