using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.NPC.Visuals;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The Salões de Bronze de Napirasu as a walkable race city (Docs/Cities.md, Docs/Mapas100.md §3.3 and map 17): its
    /// <see cref="CitySpec"/> among the race cities of the Serra de Kaggen, the five dwarves of the ficha at their doors with
    /// their own look (short, broad, braided beards), the RuneHall style (granite fronts, bronze doors and pillars, the cold
    /// Grande Forja, lava pits, chain lifts, the rock wall), the whole town inside the place the Kaggen overworld keeps for
    /// it, Puzur's Portões de Runa with the gear of maps 17–18, the two city quests and the respawn of 17–18.
    /// </summary>
    public class CityBronzeHallsTests
    {
        private static readonly string[] Cast = { "napirasu", "untash", "puzur", "shilhak", "kidin" };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        /// <summary>The overworld's place for the city (OverworldLayouts.Kaggen: safe zone x −20…21, z −19…20, flat ground ±24).</summary>
        private static readonly Rect OverworldSafe = Rect.MinMaxRect(-20f, -19f, 21f, 20f);

        [TearDown]
        public void TearDown() => CityTownRecipes.ClearCache();

        private static CityResidentSpec Resident(string id) => CityCatalog.AllResidents().Single(r => r.ResidentId == id);

        // ------------------------------------------------------------------
        // The CitySpec
        // ------------------------------------------------------------------

        [Test]
        public void BronzeHalls_HaveACitySpec_AsARaceCityOfTheMountain_SameIdAsTheirMapAndRecipe()
        {
            CitySpec halls = CityCatalog.Get(CityCatalog.SaloesDeBronze);
            Assert.IsNotNull(halls, "the Salões de Bronze have a CitySpec");
            Assert.AreEqual(BiomeType.Mountain, halls.Region);
            Assert.AreEqual(MapCatalog.SaloesDeBronze, halls.CityId, "the map's return city id");
            Assert.AreEqual(CityTownRecipes.BronzeHallsCityId, halls.CityId, "the recipe's city id");
            Assert.IsTrue(CityCatalog.IsRaceCity(halls));
            Assert.IsFalse(CityCatalog.IsHubCity(halls));
            Assert.IsFalse(CityCatalog.Cities.Contains(halls), "the hub list keeps one city per region");
            Assert.AreEqual(CityCatalog.SerraAlta, CityCatalog.ForRegion(BiomeType.Mountain).CityId, "the Mountain hub is still Litema");
            Assert.IsFalse(halls.HasGrandmaServices);
            Assert.IsFalse(halls.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(halls.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(halls.CityId));
        }

        [Test]
        public void MapsOfTheBronzeHalls_RespawnInTheirPlaza()
        {
            Assert.AreEqual(CityCatalog.SaloesDeBronze, MapCatalog.RespawnCityFor("map_017_napirasu").CityId);
            Assert.AreEqual(CityCatalog.SaloesDeBronze, MapCatalog.RespawnCityFor("map_018_mazans").CityId);
            Assert.AreEqual(CityCatalog.SerraAlta, MapCatalog.RespawnCityFor("map_015_abrigos").CityId, "13–16 still wake in Litema");

            var cities = new CityProgress();
            CitySpec halls = CityCatalog.Get(CityCatalog.SaloesDeBronze);
            Assert.AreSame(halls, cities.ApplyDefeatIn(halls), "a defeat on 17/18 wakes the cousins in the halls' plaza");
            Assert.AreSame(halls, cities.ConsumeRespawnNotice());
        }

        // ------------------------------------------------------------------
        // The town and its style
        // ------------------------------------------------------------------

        [Test]
        public void BronzeHalls_AreCutIntoTheRock_WithBronzePillarsTheColdForgeLavaPitsAndLifts()
        {
            CityTownRecipe recipe = CityTownRecipes.BronzeHalls();
            Assert.AreSame(TownStyles.RuneHall, recipe.Style);
            Assert.AreEqual(TownArchitecture.RuneHall, TownStyles.RuneHall.Architecture);
            Assert.AreEqual(BiomeType.Mountain, TownStyles.RuneHall.Region);
            Assert.AreEqual(TownRoofKind.Rock, TownStyles.RuneHall.HouseRoof);

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.SaloesDeBronze);
            Assert.AreSame(TownStyles.RuneHall, layout.Style, "the hand-picked style reaches the layout");
            Assert.AreEqual(1, recipe.PlazaProps.Count(p => p.Kind == CityPropKind.GreatForge), "the Grande Forja, gone cold");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.BronzePillar), 6);
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.LavaPit), 2);
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.ChainLift), 1);
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.RockWall), 3, "the mountain closes the north and the east");
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.RockWall));
            Assert.IsTrue(layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).All(l => l.Building.RuneHall && l.Building.Roof == TownRoofKind.Rock),
                "every hall is a front cut into the granite");
            Assert.AreEqual(TownShopDressing.Runes, recipe.Lots.Single(l => l.ResidentId == "puzur").Spec.Dressing);
            Assert.AreEqual(TownShopDressing.OldMiner, recipe.Lots.Single(l => l.ResidentId == "shilhak").Spec.Dressing);
            Assert.AreEqual(TownShopDressing.Forja, recipe.Lots.Single(l => l.ResidentId == "napirasu").Spec.Dressing);

            // Other towns keep their own style (rune halls only by the dwarves' style or a hand spec).
            Assert.IsFalse(CityTownRecipes.LayoutFor(CityCatalog.SerraAlta).Lots.Any(l => l.Building != null && l.Building.RuneHall));
            Assert.IsFalse(CityTownRecipes.LayoutFor(CityCatalog.Lamparinas).Lots.Any(l => l.Building != null && (l.Building.RuneHall || l.Building.TunnelStall)));
        }

        [Test]
        public void BronzeHalls_FitThePlaceTheKaggenOverworldKeeps_AtGroundLevel()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.SaloesDeBronze);
            foreach (CitySolid solid in layout.Solids)
            {
                Assert.IsTrue(CityGeometry.Contains(OverworldSafe, solid.Footprint), $"{solid.Owner} at {solid.Footprint} leaves the place the overworld keeps");
            }

            foreach (CityLot lot in layout.Lots)
            {
                Assert.IsTrue(CityGeometry.Contains(OverworldSafe, lot.Footprint), lot.Id);
            }

            foreach (CityStreet street in layout.Streets)
            {
                Assert.IsTrue(OverworldSafe.Contains(street.End), street.Arm.ToString());
            }

            foreach (CityExit exit in layout.Exits.Where(e => e.Kind == CityExitKind.ToMap))
            {
                Assert.IsTrue(OverworldSafe.Contains(exit.Target), exit.Id);
            }

            Assert.IsTrue(CityGeometry.Contains(OverworldSafe, layout.WalkBounds), "the walk box inside the overworld's safe zone");
            CollectionAssert.AreEquivalent(new[] { CityArm.West, CityArm.East, CityArm.South }, layout.Streets.Select(s => s.Arm).ToArray(), "same three roads out as before");
            Assert.AreEqual(3, layout.Exits.Count(e => e.Kind == CityExitKind.ToMap));

            // The overworld's trails come in at local (±24, 0): the West and East roads run straight to them.
            Assert.AreEqual(0f, layout.Street(CityArm.West).End.y, 1e-4f);
            Assert.AreEqual(0f, layout.Street(CityArm.East).End.y, 1e-4f);
        }

        [Test]
        public void TownPalette_AndNpcPalette_CoverEverySwatch()
        {
            Assert.AreEqual(TownPalette.Columns * TownPalette.Rows, TownPalette.Count);
            Assert.AreEqual(TownPalette.Count, Enum.GetValues(typeof(TownSwatch)).Length);
            Assert.AreEqual(NpcPalette.Columns * NpcPalette.Rows, NpcPalette.Count);
            Assert.AreEqual(NpcPalette.Count, Enum.GetValues(typeof(NpcSwatch)).Length);
            Assert.Greater(TownPalette.GlowOf(TownSwatch.RuneLight), 0f, "the runes glow at night");
        }

        // ------------------------------------------------------------------
        // The cast
        // ------------------------------------------------------------------

        [Test]
        public void BronzeHalls_Residents_AreTheFichasDwarves_WithALookOfTheirOwn()
        {
            CitySpec halls = CityCatalog.Get(CityCatalog.SaloesDeBronze);
            CollectionAssert.AreEquivalent(Cast, halls.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.Less(look.Height, 0.86f, $"{id}: a dwarf is short");
                Assert.AreEqual(BiomeType.Mountain, look.Region);
                Assert.IsFalse(look.Tusks || look.ElfEars || look.Snout, $"{id}: no orc, elf or kobold features");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
            }

            Assert.IsTrue(Cast.Where(id => id != "napirasu").All(id => NpcRecipes.Find(id).Beard != NpcBeard.None), "the dwarf men wear their beards");
            Assert.AreEqual(NpcBeard.LongBraided, NpcRecipes.Find("untash").Beard, "braided, with bronze beads");
            Assert.AreEqual(CityShopCatalog.PortoesRuna, halls.Residents.Single(r => r.ResidentId == "puzur").ShopId, "Puzur keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.PortoesRuna }, halls.ShopIds.ToArray());
        }

        [Test]
        public void BronzeHalls_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotStartWith("Dona ", pt);
                StringAssert.Contains(char.ToUpperInvariant(id[0]) + id.Substring(1), pt, $"{id}: named after the ficha ({pt})");
            }

            Assert.AreEqual("Napirasu, a Mestra Fundidora", CityTexts.ResidentName(null, Resident("napirasu")));
            Assert.AreEqual("Puzur, o Guardião das Runas", CityTexts.ResidentName(null, Resident("puzur")));
        }

        [Test]
        public void BronzeHalls_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            foreach (bool mascate in new[] { false, true })
            {
                List<string> problems = CityLayoutRules.Validate(CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.SaloesDeBronze, mascate)));
                Assert.IsEmpty(problems, (mascate ? "+caravaneer\n" : string.Empty) + string.Join("\n", problems));
            }

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.SaloesDeBronze);
            var grid = CityLayoutRules.WalkGrid.Build(layout, CityLayoutRules.MinCorridor * 0.5f);
            Assert.IsTrue(grid.Flood(layout.PlazaCenter));
            foreach (string id in Cast)
            {
                CityLot lot = layout.LotOfResident(id);
                Assert.IsNotNull(lot, $"{id} has no door to stand at");
                Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, lot.StandPoint), CityLayoutRules.WalkerRadius, $"{id} stands inside a collider");
                Assert.LessOrEqual(Vector2.Distance(lot.StandPoint, lot.DoorApproach), lot.InteractRadius, $"{id} out of reach");
                Assert.IsTrue(grid.IsReached(lot.DoorApproach), $"nobody can walk up to {id}");
                Assert.IsTrue(lot.Front.x < -0.5f || lot.Front.y < -0.5f, $"{id}'s door turns its back to the camera");
                Assert.IsTrue(layout.SafeZone.Contains(lot.StandPoint));
            }

            Assert.AreEqual("salao_da_forja", layout.LotOfResident("napirasu").Id, "Napirasu in the hall of the forge at the plaza head");
            Assert.AreEqual("portoes_de_runa", layout.LotOfShop(CityShopCatalog.PortoesRuna).Id);
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "the Cais da Barca");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn at the plaza");
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, layout.Respawn), CityLayoutRules.WalkerRadius, "nothing stands on the respawn");
        }

        // ------------------------------------------------------------------
        // Puzur's rune gates
        // ------------------------------------------------------------------

        [Test]
        public void PortoesDeRuna_SellThePickaxeTorchOf17_AndTheGingerTeaOf18_AtBalancedPrices_AndTheCounterTurnsWeekly()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.PortoesRuna);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Mountain, shop.HomeRegion);
            Assert.AreEqual(ShopTheme.Speciality, shop.Theme);
            Assert.AreEqual(ShopRotationPeriod.Weekly, shop.Period);

            // Map 17 asks for a pickaxe and a torch, map 18 (Cume dos Mazans, return city the Salões) for blizzard resistance.
            string pickaxe = MapRequirementRules.PrimaryItemOf(MapCatalog.Maps.Single(m => m.MapId == 17).SpecialRequirementTag);
            string tea = MapRequirementRules.PrimaryItemOf(MapCatalog.Maps.Single(m => m.MapId == 18).SpecialRequirementTag);
            Assert.AreEqual(MapRequirementItems.Pickaxe, pickaxe);
            Assert.AreEqual(MapRequirementItems.GingerTea, tea);
            var gates = new[] { pickaxe, MapRequirementItems.OilLamp, tea };
            foreach (string itemId in gates)
            {
                CityShopEntry gate = CityShopCatalog.EntryOf(shop.ShopId, itemId);
                Assert.IsNotNull(gate, $"Puzur sells {itemId}");
                Assert.IsFalse(gate.Rotating, $"{itemId} is always on the counter");
                Assert.IsFalse(gate.Unlock.IsGated, $"{itemId} from day one");
            }

            foreach (CityShopEntry entry in shop.Entries.Where(e => e.IsBagItem))
            {
                ItemDefinition item = ItemDatabase.GetById(entry.ItemId);
                Assert.IsNotNull(item, entry.ItemId);
                Assert.That(entry.Price, Is.InRange(Mathf.FloorToInt(item.Value * 0.8f), Mathf.CeilToInt(item.Value * 1.25f)), entry.ItemId);
            }

            Assert.IsTrue(shop.Entries.Count(e => e.ItemId.StartsWith("item_runa_")) >= 4, "the bronze runes of the ficha");
            Assert.Greater(shop.Entries.Count(e => e.Rotating && e.Pool == ShopPool.Main), shop.RotatingSlots, "more lines than slots: the counter turns");
            long week = ShopRotation.PeriodIndex(new DateTime(2026, 11, 3), ShopRotationPeriod.Weekly);
            var counters = new HashSet<string>();
            for (int w = 0; w < 8; w++)
            {
                List<string> stock = ShopRotation.PickStock(shop, week + w, null).Select(e => e.ItemId).ToList();
                CollectionAssert.AreEqual(stock, ShopRotation.PickStock(shop, week + w, null).Select(e => e.ItemId).ToList(), "same week, same counter");
                foreach (string itemId in gates)
                {
                    Assert.Contains(itemId, stock);
                }

                counters.Add(string.Join(",", stock));
            }

            Assert.Greater(counters.Count, 1, "the counter changes from week to week");
        }

        [Test]
        public void BronzeHalls_OpenTheRuneGates_AndNeverTheCaravaneer()
        {
            CitySpec halls = CityCatalog.Get(CityCatalog.SaloesDeBronze);
            for (int d = 0; d < 28; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(halls, new DateTime(2026, 11, 3).AddDays(d), null);
                Assert.Contains(CityShopCatalog.PortoesRuna, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
            }
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void BronzeHalls_HaveTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec halls = CityCatalog.Get(CityCatalog.SaloesDeBronze);
            var keys = new List<string> { halls.NameKey, halls.ButtonKey, halls.FlavourKey, CityShopCatalog.Get(CityShopCatalog.PortoesRuna).NameKey };
            foreach (string id in Cast)
            {
                CityResidentSpec resident = Resident(id);
                keys.Add(resident.NameKey);
                keys.Add(resident.AnyLineKey);
                keys.AddRange(Heroes.Select(resident.LineKey));
                Assert.AreEqual(Heroes.Length, Heroes.Select(h => CityTexts.ResidentLine(null, resident, h)).Distinct().Count(), $"{id} repeats a line");
            }

            foreach (string key in keys)
            {
                Assert.IsTrue(manager.HasKey(key), $"missing key {key}");
                Assert.IsTrue(byKey.TryGetValue(key, out LocalizationEntry entry), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), $"no pt-BR for {key}");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), $"no English for {key}");
                if (key.Contains(".line."))
                {
                    Assert.AreNotEqual(entry.En, entry.PtBR, $"{key} is not translated");
                }
            }

            foreach (string id in Cast)
            {
                foreach (CharacterId hero in Heroes)
                {
                    string line = CityTexts.ResidentLine(null, Resident(id), hero);
                    string name = hero == CharacterId.Icaro ? "Ícaro" : hero.ToString();
                    StringAssert.Contains(name, line, $"{id} → {hero}");
                }
            }

            string all = string.Join(" ", Cast.SelectMany(id => Heroes.Select(h => CityTexts.ResidentLine(null, Resident(id), h))));
            StringAssert.Contains("Jorge", all);
            StringAssert.Contains("Edna", all);
            foreach (string folk in new[] { "Saci", "Curupira", "Seu ", "Dona " })
            {
                StringAssert.DoesNotContain(folk, all);
            }
        }

        // ------------------------------------------------------------------
        // City quests
        // ------------------------------------------------------------------

        [Test]
        public void CityQuests_AreGivenByTheHallsResidents_WithTheGingerTeaAsTheLastReward()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.QuestsIn(CityCatalog.SaloesDeBronze).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "untash", "shilhak" }, quests.Select(q => q.GiverResidentId).ToArray());
            foreach (CityQuestSpec spec in quests)
            {
                foreach (RegionObjectiveSpec objective in spec.Quest.Objectives)
                {
                    EnemyDefinition enemy = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{objective.TargetId}");
                    Assert.IsNotNull(enemy, objective.TargetId);
                    Assert.IsFalse(enemy.IsBoss, objective.TargetId);
                }

                QuestDefinition quest = CityQuestCatalog.GetQuest(spec.QuestId);
                Assert.IsNotNull(quest);
                Assert.IsTrue(quest.RewardItems.All(r => r.Item != null), $"{spec.QuestId}: rewards resolve");
            }

            Assert.IsTrue(quests[1].Quest.Rewards.Any(r => r.ItemId == MapRequirementItems.GingerTea), "Shilhak pays with the ginger tea (map 18)");
            Assert.AreEqual(CityQuestCatalog.Quests.Count, CityQuestCatalog.Quests.Select(q => q.QuestId).Distinct().Count());
        }

        [Test]
        public void CityQuests_UntashOffersFirst_ShilhakOnlyAfterHisQuest_OnTheQuestManager()
        {
            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Func<string, bool> isActive = active.Contains;
            Func<string, bool> isDone = done.Contains;

            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("untash", isActive, isDone, out CityQuestSpec builder));
            Assert.AreEqual(CityQuestCatalog.SaloesGolems, builder.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("shilhak", isActive, isDone, out _), "the chain starts with the builder");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("kidin", isActive, isDone, out _), "Kidin gives no quest");

            done.Add(CityQuestCatalog.SaloesGolems);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("shilhak", isActive, isDone, out CityQuestSpec miner));
            Assert.AreEqual(CityQuestCatalog.SaloesBats, miner.QuestId);

            var quests = new QuestManager();
            quests.Initialize();
            CityQuestCatalog.RegisterAll(quests);
            CityQuestSpec golems = CityQuestCatalog.Quests.Single(q => q.QuestId == CityQuestCatalog.SaloesGolems);
            Assert.AreEqual(golems.OfferKey, CityQuestCatalog.Talk(quests, "untash"));
            Assert.IsTrue(quests.IsActive(CityQuestCatalog.SaloesGolems));
            Assert.AreEqual(golems.ActiveKey, CityQuestCatalog.Talk(quests, "untash"));
            Assert.IsNull(CityQuestCatalog.Talk(quests, "shilhak"), "his quest waits for the builder's");
            quests.Shutdown();
        }
    }
}
