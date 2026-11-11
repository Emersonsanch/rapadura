using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Mounts.Visuals;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Map;
using UnityEngine;

namespace Rapadura.Tests.EditMode
{
    /// <summary>
    /// The first flying mount (Docs/Mounts.md §3.8, <see cref="MountSkyCatalog"/>): the Grifo da Coluna da Glória in the
    /// catalog, the map gates it opens (24 Ninho de Mahaway, 39 Forte dos Ventos), the "Asas da Coluna" quest that gives
    /// it, the visual-only flight, its model and its sounds.
    /// </summary>
    public class MountSkyTests
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

        private static MountCollection NewCollection()
        {
            int n = 0;
            return new MountCollection { GuidFactory = () => "g" + (++n), UtcNow = () => 0L, DropRandom = new System.Random(1), IsNight = () => false };
        }

        // ---------------- Catalog ----------------

        [Test]
        public void Griffin_IsAFlyer_OnTheRareBudget_WithItsOwnAbilityAndModel()
        {
            MountSpec spec = MountCatalog.FindSpec(MountSkyCatalog.ColumnGriffin);
            Assert.IsNotNull(spec, "in MountCatalog.Specs");
            Assert.AreEqual(MountType.Voador, spec.Type);
            Assert.IsTrue(MountSkyCatalog.IsFlyer(MountSkyCatalog.ColumnGriffin));
            Assert.IsFalse(MountSkyCatalog.IsFlyer(MountCatalog.Pony));
            Assert.AreEqual(2, MountCatalog.Specs.Count(s => s.Type == MountType.Voador), "the Grifo da Coluna and the Íbis Real");

            MountDefinition definition = MountCatalog.Get(MountSkyCatalog.ColumnGriffin);
            Assert.IsNotNull(definition);
            Assert.AreEqual(MountType.Voador, definition.Type);
            Assert.AreEqual(ItemRarity.Rare, definition.Rarity);
            Assert.AreEqual(MountRules.BudgetFor(ItemRarity.Rare), definition.BudgetPoints, 1e-3f);
            Assert.AreEqual(MountTradeRule.UmaVez, definition.TradeRule);
            Assert.AreEqual(MountVisualRecipe.ColumnGriffin, definition.VisualRecipe);
            Assert.IsTrue(definition.HasFavorite);
            Assert.AreEqual(CharacterId.Lavine, definition.FavoriteCharacter, "Lavine's Ar Sereno lives in the Coluna");
            Assert.AreEqual(0, spec.ShopPrice, "not sold: quest only");

            Assert.IsNotNull(definition.Ability);
            Assert.AreEqual(MountAbilityKind.ShortFlight, definition.Ability.Kind);
            Assert.AreEqual(MountSkyCatalog.ColumnFlight, definition.Ability.AbilityId);
            Assert.Greater(definition.Ability.CooldownSeconds, 0f);
            Assert.Greater(definition.Ability.Distance, 0f);
            Assert.Greater(definition.Ability.Magnitude, 0f, "fear seconds on landing");
            Assert.AreEqual(0f, definition.Ability.BossMultiplier, "bosses are not scared");
            Assert.IsFalse(definition.Ability.AllowedInSafeZone);
            Assert.AreEqual(MountFlightRules.CruiseHover + MountFlightRules.SoarPeak, definition.Ability.ServerMaxAltitude, 1e-4f);

            MountCodexEntry entry = MountCodex.Entries.Single(e => e.MountId == MountSkyCatalog.ColumnGriffin);
            Assert.AreEqual(spec.Rarity, entry.Rarity);
            Assert.IsTrue(entry.IsAvailable);
        }

        [Test]
        public void Griffin_Texts_CoverNameAbilityToastAndQuest_InBothLanguages()
        {
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> byKey = MountTexts.Entries.ToDictionary(e => e.Key);
            MountDefinition d = MountCatalog.Get(MountSkyCatalog.ColumnGriffin);
            foreach (string key in new[]
                     {
                         d.DisplayNameKey, d.DescriptionKey, d.LoreKey, d.SourceHintKey, d.FavoriteLineKey, d.Ability.NameKey, d.Ability.DescriptionKey,
                         MountSkyTexts.ToastFlight, MountSkyTexts.QuestTitle, MountSkyTexts.QuestDescription, MountSkyTexts.QuestObjective,
                     })
            {
                Assert.IsTrue(byKey.ContainsKey(key), key);
                Assert.IsFalse(string.IsNullOrEmpty(byKey[key].En), key);
                Assert.IsFalse(string.IsNullOrEmpty(byKey[key].PtBR), key);
            }

            Assert.AreEqual("Grifo da Coluna da Glória", byKey[d.DisplayNameKey].PtBR);
            Assert.AreEqual("Voo da Coluna", byKey[d.Ability.NameKey].PtBR);
            StringAssert.Contains("{0}", byKey[MountSkyTexts.ToastFlight].PtBR);
        }

        // ---------------- Map gates ----------------

        [Test]
        public void OwningTheGriffin_OpensTheWingGatesOfMaps24And39()
        {
            MapDefinition roost = MapCatalog.Get(24);
            MapDefinition fort = MapCatalog.Get(39);
            Assert.AreEqual(MapRequirementTags.WingsOrDoubleJump, roost.SpecialRequirementTag, "Ninho de Mahaway");
            Assert.AreEqual(MapRequirementTags.WingsRequired, fort.SpecialRequirementTag, "Forte dos Ventos");

            MapRequirementState without = MapRequirementHeroState.Capture(null, null, CharacterId.Maria,
                new[] { new OwnedMount { guid = "a", mountId = MountCatalog.Pony } });
            Assert.IsFalse(MapRequirementRules.IsMet(roost.SpecialRequirementTag, without));
            Assert.IsFalse(MapRequirementRules.IsMet(fort.SpecialRequirementTag, without));

            MapRequirementState with = MapRequirementHeroState.Capture(null, null, CharacterId.Maria, new[]
            {
                new OwnedMount { guid = "a", mountId = MountCatalog.Pony },
                new OwnedMount { guid = "b", mountId = MountSkyCatalog.ColumnGriffin },
            });
            Assert.IsTrue(with.OwnedMountTypes.Contains(MountType.Voador));
            Assert.IsTrue(MapRequirementRules.IsMet(roost.SpecialRequirementTag, with));
            Assert.IsTrue(MapRequirementRules.IsMet(fort.SpecialRequirementTag, with));
        }

        // ---------------- Obtaining: "Asas da Coluna" ----------------

        [Test]
        public void Quest_StartRules_AndTheHook()
        {
            Assert.IsFalse(ColumnGriffinQuest.ShouldStart(ColumnGriffinQuest.StartLevel - 1, false, false, false));
            Assert.IsTrue(ColumnGriffinQuest.ShouldStart(ColumnGriffinQuest.StartLevel, false, false, false));
            Assert.IsFalse(ColumnGriffinQuest.ShouldStart(60, true, false, false), "already active");
            Assert.IsFalse(ColumnGriffinQuest.ShouldStart(60, false, true, false), "already done");
            Assert.IsFalse(ColumnGriffinQuest.ShouldStart(60, false, false, true), "griffin already owned");
            Assert.Less(ColumnGriffinQuest.StartLevel, MapCatalog.Get(24).MonsterLevelMin, "opens before the Ninho de Mahaway needs it");

            Assert.IsTrue(ColumnGriffinQuest.OwesGriffin(true, new List<OwnedMount>()));
            Assert.IsFalse(ColumnGriffinQuest.OwesGriffin(false, new List<OwnedMount>()));
            Assert.IsFalse(ColumnGriffinQuest.OwesGriffin(true, new[] { new OwnedMount { mountId = MountSkyCatalog.ColumnGriffin } }));

            MountSourceHook hook = MountSourceHooks.For(MountSkyCatalog.ColumnGriffin);
            Assert.IsNotNull(hook);
            Assert.AreEqual(MountSourceKind.Quest, hook.Kind);
            Assert.AreEqual(ColumnGriffinQuest.QuestId, hook.HookId);

            QuestDefinition definition = ColumnGriffinQuest.Build();
            try
            {
                QuestObjective objective = definition.Objectives.Single();
                Assert.AreEqual(QuestObjectiveType.KillEnemyType, objective.Type);
                Assert.IsTrue(string.IsNullOrEmpty(objective.TargetId), "any critter frees a spark");
                Assert.AreEqual(ColumnGriffinQuest.SparksRequired, objective.RequiredAmount);
            }
            finally
            {
                Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void Quest_OpensAtLevel40_FortySparksGiveTheGriffin_Once()
        {
            var quests = new QuestManager();
            quests.Initialize();
            ServiceLocator.Register(quests);
            MountCollection collection = NewCollection();
            collection.Initialize(); // installs "Primeira Sela" and "Asas da Coluna", listens to QuestCompletedEvent
            var killed = new GameObject("enemy_threshold_bat");
            try
            {
                EventBus.Publish(new PlayerExperienceChangedEvent(0, 100, ColumnGriffinQuest.StartLevel - 1));
                Assert.IsFalse(quests.IsActive(ColumnGriffinQuest.QuestId), "not before level 40");

                EventBus.Publish(new PlayerExperienceChangedEvent(0, 100, ColumnGriffinQuest.StartLevel));
                Assert.IsTrue(quests.IsActive(ColumnGriffinQuest.QuestId));

                // A real kill counts by itself (empty target), then the rest.
                EventBus.Publish(new CombatTargetDiedEvent(killed, null));
                Assert.AreEqual(1, quests.GetProgress(ColumnGriffinQuest.QuestId).Objectives[0].CurrentAmount);
                for (int i = 1; i < ColumnGriffinQuest.SparksRequired - 1; i++)
                {
                    quests.UpdateObjectiveProgress(QuestObjectiveType.KillEnemyType, "enemy_any_" + (i % 3), 1);
                }

                Assert.IsFalse(collection.Owns(MountSkyCatalog.ColumnGriffin), "one spark short");
                quests.UpdateObjectiveProgress(QuestObjectiveType.KillEnemyType, "enemy_last", 1);

                Assert.IsTrue(quests.IsCompleted(ColumnGriffinQuest.QuestId));
                Assert.IsTrue(collection.Owns(MountSkyCatalog.ColumnGriffin));
                OwnedMount griffin = collection.Mounts.Single(m => m.mountId == MountSkyCatalog.ColumnGriffin);
                Assert.AreEqual(MountCollection.SourceQuest, griffin.source);

                // Given once, and the map gate now opens with the live collection.
                ColumnGriffinQuest.Evaluate(80);
                EventBus.Publish(new PlayerExperienceChangedEvent(0, 100, 80));
                Assert.AreEqual(1, collection.Mounts.Count(m => m.mountId == MountSkyCatalog.ColumnGriffin));
                Assert.IsFalse(quests.IsActive(ColumnGriffinQuest.QuestId));
                MapRequirementState state = MapRequirementHeroState.Capture(null, null, null, collection.Mounts);
                Assert.IsTrue(MapRequirementRules.IsMet(MapRequirementTags.WingsOrDoubleJump, state));
            }
            finally
            {
                Object.DestroyImmediate(killed);
                collection.Shutdown();
                quests.Shutdown();
            }
        }

        [Test]
        public void Quest_DoneButGriffinMissing_IsRepaid()
        {
            var quests = new QuestManager();
            quests.Initialize();
            ServiceLocator.Register(quests);
            MountCollection collection = NewCollection();
            collection.Initialize();
            try
            {
                EventBus.Publish(new PlayerExperienceChangedEvent(0, 100, 45));
                quests.UpdateObjectiveProgress(QuestObjectiveType.KillEnemyType, "enemy_x", ColumnGriffinQuest.SparksRequired);
                Assert.IsTrue(quests.IsCompleted(ColumnGriffinQuest.QuestId));

                // An old save: the quest is done but the collection lost the griffin.
                collection.ResetForNewSession();
                Assert.IsFalse(collection.Owns(MountSkyCatalog.ColumnGriffin));
                ColumnGriffinQuest.Evaluate(0);
                Assert.IsTrue(collection.Owns(MountSkyCatalog.ColumnGriffin));
            }
            finally
            {
                collection.Shutdown();
                quests.Shutdown();
            }
        }

        // ---------------- Visual-only flight ----------------

        [Test]
        public void Flight_HoversAboveTheGround_LowInCities_ArcsOnTheAbility()
        {
            Assert.AreEqual(MountFlightRules.IdleHover, MountFlightRules.HoverHeight(true, 0f), 1e-4f);
            Assert.AreEqual(MountFlightRules.CruiseHover, MountFlightRules.HoverHeight(true, 1f), 1e-4f);
            Assert.Greater(MountFlightRules.HoverHeight(true, 1f), MountFlightRules.HoverHeight(true, 0f), "rises with speed");
            Assert.AreEqual(MountFlightRules.LowHover, MountFlightRules.HoverHeight(false, 1f, 0.5f), 1e-4f, "cities: low glide, no soar");
            Assert.AreEqual(MountFlightRules.IdleHover + MountFlightRules.SoarPeak, MountFlightRules.HoverHeight(true, 0f, 0.5f), 1e-4f);
            Assert.AreEqual(0f, MountFlightRules.SoarLift(0f));
            Assert.AreEqual(0f, MountFlightRules.SoarLift(1f));
            Assert.AreEqual(0f, MountFlightRules.SoarLift(-1f));

            Assert.IsFalse(MountFlightRules.FlightAllowed(MountPlace.SafeZone));
            Assert.IsFalse(MountFlightRules.FlightAllowed(MountPlace.Indoors));
            Assert.IsTrue(MountFlightRules.FlightAllowed(MountPlace.WorldMap));
            Assert.IsTrue(MountFlightRules.FlightAllowed(MountPlace.ArenaFree));

            Vector3 landing = MountFlightRules.FlightLanding(new Vector3(1f, 2f, 3f), new Vector3(0f, 5f, 2f), 10f);
            Assert.AreEqual(new Vector3(1f, 2f, 13f), landing, "flat, on the ground plane");
        }

        [Test]
        public void Flight_WingBeat_StaysInRange_SlowsWithReducedMotion_AndMarksTheDownstroke()
        {
            float amplitude = MountFlightRules.FlapAmplitude(1f, false, false);
            Assert.AreEqual(amplitude * 0.5f, MountFlightRules.FlapAmplitude(1f, false, true), 1e-4f);
            Assert.Greater(MountFlightRules.FlapAmplitude(0f, true, false), amplitude, "beats harder when soaring");
            Assert.Greater(MountFlightRules.BeatsPerSecond(1f, false), MountFlightRules.BeatsPerSecond(0f, false));

            float min = float.MaxValue;
            float max = float.MinValue;
            for (int i = 0; i <= 100; i++)
            {
                float angle = MountFlightRules.WingAngle(i / 100f, amplitude);
                min = Mathf.Min(min, angle);
                max = Mathf.Max(max, angle);
            }

            Assert.AreEqual(6f + amplitude, max, 0.5f);
            Assert.AreEqual(6f - amplitude, min, 0.5f);
            Assert.AreEqual(MountFlightRules.WingAngle(0.3f, amplitude), MountFlightRules.WingAngle(2.3f, amplitude), 1e-3f, "periodic");

            Assert.IsTrue(MountFlightRules.DownstrokeStarted(0.55f, 0.65f));
            Assert.IsFalse(MountFlightRules.DownstrokeStarted(0.65f, 0.95f));
            Assert.IsTrue(MountFlightRules.DownstrokeStarted(0.9f, 1.62f));
            Assert.IsFalse(MountFlightRules.DownstrokeStarted(0.7f, 0.7f));
            Assert.AreEqual(MountFlightRules.Bob(0.25f, false) * 0.5f, MountFlightRules.Bob(0.25f, true), 1e-5f);
        }

        // ---------------- Model and sound ----------------

        [Test]
        public void Model_HasJointedWingsOnTheBody_ABeak_AndStaysInBudget()
        {
            QuadrupedShape shape = MountRecipe.ShapeFor(MountVisualRecipe.ColumnGriffin);
            Assert.IsTrue(shape.FlightWings);
            Assert.IsTrue(shape.Beak);
            Assert.Greater(shape.WingSpan, shape.BodyLength * 0.5f);

            ProceduralEnemyBlueprint blueprint = MountModels.CreateBlueprint(MountVisualRecipe.ColumnGriffin);
            try
            {
                Assert.LessOrEqual(blueprint.TriangleCount, MountModels.TriangleBudget);
                int body = blueprint.IndexOf(MountParts.Body);
                foreach (string wing in new[] { MountParts.WingLeft, MountParts.WingRight })
                {
                    int index = blueprint.IndexOf(wing);
                    Assert.GreaterOrEqual(index, 0, wing);
                    Assert.AreEqual(body, blueprint.Parts[index].Parent, wing + " flaps with the body");
                }

                Assert.AreEqual(blueprint.IndexOf(MountParts.WingLeft), blueprint.Parts[blueprint.IndexOf(MountParts.WingTipLeft)].Parent);
                Assert.AreEqual(blueprint.IndexOf(MountParts.WingRight), blueprint.Parts[blueprint.IndexOf(MountParts.WingTipRight)].Parent);
                Assert.Less(blueprint.Parts[blueprint.IndexOf(MountParts.WingLeft)].Position.x, 0f);
                Assert.Greater(blueprint.Parts[blueprint.IndexOf(MountParts.WingRight)].Position.x, 0f);
                Assert.GreaterOrEqual(blueprint.IndexOf(MountSkyRecipes.Prefix + "Beak"), 0);
            }
            finally
            {
                blueprint.DestroyMeshes();
            }

            // The walkers never get flapping wings.
            Assert.IsFalse(MountRecipe.ShapeFor(MountVisualRecipe.Rhea).FlightWings);
            Assert.IsFalse(MountRecipe.ShapeFor(MountVisualRecipe.Pony).FlightWings);
        }

        [Test]
        public void Actor_OfTheGriffin_IsAFlyer_WithoutTheGaitEngine_AndSoars()
        {
            MountActor griffin = MountActor.Spawn(MountCatalog.Get(MountSkyCatalog.ColumnGriffin), Vector3.zero, Quaternion.identity);
            MountActor pony = MountActor.Spawn(MountCatalog.Get(MountCatalog.Pony), Vector3.zero, Quaternion.identity);
            try
            {
                Assert.IsTrue(griffin.IsFlyer);
                Assert.IsFalse(griffin.HasLegRig, "the feet are never planted: the legs fold in the air");
                Assert.IsFalse(griffin.IsSoaring);
                griffin.Soar(0.8f);
                Assert.IsTrue(griffin.IsSoaring);

                Assert.IsFalse(pony.IsFlyer);
                pony.Soar(1f);
                Assert.IsFalse(pony.IsSoaring, "walkers do not soar");
            }
            finally
            {
                Object.DestroyImmediate(griffin.gameObject);
                Object.DestroyImmediate(pony.gameObject);
            }
        }

        [Test]
        public void Sounds_WingBeatAndCry_AreShortAndAudible()
        {
            foreach (float[] samples in new[] { MountAudio.GenerateWingBeat(), MountAudio.GenerateGriffinCry() })
            {
                Assert.Greater(samples.Length, 100);
                Assert.Less(samples.Length, MountAudio.SampleRate * 2);
                Assert.That(samples.Max(Mathf.Abs), Is.InRange(0.3f, 1f));
            }

            List<Rapadura.Audio.SfxEventMap.Cue> cues = MountAudio.BuildCues(false);
            Assert.IsTrue(cues.Any(c => c.id == MountAudio.WingBeat));
            Assert.IsTrue(cues.Any(c => c.id == MountAudio.GriffinCry));
            Assert.AreEqual(MountAudio.PawStep, MountAudio.StepCueFor(MountVisualRecipe.ColumnGriffin));
        }
    }
}
