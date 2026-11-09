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
    /// Hinokura, a Ilha-Forja, as a walkable capital (Docs/Cities.md, Docs/Mapas100.md map 65): its <see cref="CitySpec"/> among
    /// the late-region cities, the four people of the forge isle named after the ficha (Akane, Kurogane, Sango, Hikari) at
    /// their doors, the LacquerPalace style (halls of red lacquer on curved coral walls, the Forja de Hinokura, channels of tamed
    /// lava with sluices and black stone bridges, stone lanterns, pruned pines, lava rocks, the gate of red posts under a tiled
    /// roof), the whole town inside a circle of radius ~22.5 m at y = 0 with its three roads out (W, E, S), the Ferro Preto do
    /// Kurogane with the gear the maps of the archipelago will ask for (never the life or chaos stones) and the two city
    /// quests; maps 65–70 (in <see cref="MapCatalog"/>, zones of OverworldLayouts.Hinokura) return to it and wake the cousins on
    /// its plaza (their gates: MapRequirementTests.GatesOfRegionTwelve_…).
    /// </summary>
    public class CityHinokuraTests
    {
        private static readonly string[] Cast = { "akane", "kurogane", "sango", "hikari" };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        private static readonly Dictionary<string, string> FichaNames = new Dictionary<string, string>
        {
            { "akane", "Mestra Akane" }, { "kurogane", "Kurogane" }, { "sango", "Sango" }, { "hikari", "Hikari" },
        };

        [TearDown]
        public void TearDown() => CityTownRecipes.ClearCache();

        private static CityResidentSpec Resident(string id) => CityCatalog.AllResidents().Single(r => r.ResidentId == id);

        private static float Reach(CitySolid solid) => solid.IsCapsule ? solid.Center.magnitude + solid.Radius : Reach(solid.Footprint);

        private static float Reach(Rect r) => new[] { new Vector2(r.xMin, r.yMin), new Vector2(r.xMin, r.yMax), new Vector2(r.xMax, r.yMin), new Vector2(r.xMax, r.yMax) }.Max(p => p.magnitude);

        // ------------------------------------------------------------------
        // The CitySpec
        // ------------------------------------------------------------------

        [Test]
        public void Hinokura_HasACitySpec_AsALateRegionCity_SameIdAsItsRecipe()
        {
            CitySpec hinokura = CityCatalog.Get(CityCatalog.Hinokura);
            Assert.IsNotNull(hinokura, "Hinokura has a CitySpec");
            Assert.AreEqual(BiomeType.Forest, hinokura.Region, "the archipelago borrows the Forest (always unlocked)");
            Assert.AreEqual("hinokura", hinokura.CityId, "the return city id maps 65–70 will use");
            Assert.AreEqual(CityTownRecipes.HinokuraCityId, hinokura.CityId, "the recipe's city id");
            Assert.AreEqual(MapCatalog.Hinokura, hinokura.CityId, "MapCatalog's id");
            CollectionAssert.Contains(MapCatalog.ReturnCityIds.ToArray(), hinokura.CityId);

            Assert.IsTrue(CityCatalog.IsLateRegionCity(hinokura), "a capital of the late regions, as Farnkesh and the Porto");
            Assert.IsFalse(CityCatalog.IsRaceCity(hinokura));
            Assert.IsFalse(CityCatalog.IsHubCity(hinokura));
            Assert.IsFalse(hinokura.HasGrandmaServices);
            Assert.IsFalse(hinokura.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(hinokura.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(hinokura.CityId));

            var cities = new CityProgress();
            Assert.AreSame(hinokura, cities.ApplyDefeatIn(hinokura), "a defeat on the isles wakes the cousins by the forge");
            Assert.AreSame(hinokura, cities.ConsumeRespawnNotice());
        }

        [Test]
        public void MapsOfTheArchipelago_RespawnInItsPlaza()
        {
            List<MapDefinition> maps = MapCatalog.Maps.Where(m => m.ReturnCityId == CityCatalog.Hinokura).ToList();
            CollectionAssert.AreEquivalent(new[] { 65, 66, 67, 68, 69, 70 }, maps.Select(m => m.MapId).ToArray(), "Hinokura is the return city of 65–70 (Docs/Mapas100.md)");
            foreach (MapDefinition map in maps)
            {
                Assert.AreEqual(CityCatalog.Hinokura, MapCatalog.RespawnCityFor(map.Id).CityId, map.Id);
            }

            Assert.AreEqual(MapSettlementKind.Capital, MapCatalog.Get("map_065_hinokura").Settlement);
            Assert.IsNotNull(CityCatalog.Get(SafeZoneRules.RespawnCity("map_065_hinokura", _ => CityCatalog.Hinokura, CityCatalog.VilaZefa)));
        }

        // ------------------------------------------------------------------
        // The town and its style
        // ------------------------------------------------------------------

        [Test]
        public void Hinokura_IsALacquerPalace_WithTheForgeLavaChannelsLanternsPinesAndRocks()
        {
            CityTownRecipe recipe = CityTownRecipes.Hinokura();
            Assert.AreSame(TownStyles.LacquerPalace, recipe.Style);
            Assert.AreEqual(TownArchitecture.LacquerPalace, TownStyles.LacquerPalace.Architecture);

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Hinokura);
            Assert.AreSame(TownStyles.LacquerPalace, layout.Style);
            CityProp forge = recipe.PlazaProps.Single(p => p.Kind == CityPropKind.IsleForge);
            Assert.Less(forge.Position.magnitude, layout.PlazaRadius, "the Forja de Hinokura stands on the plaza");
            Assert.Greater(forge.Position.magnitude, TownKit.IsleForgeRadius + CityLayoutRules.WalkerRadius, "...but not on the respawn");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.StoneLantern), 4, "stone lanterns");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.StoneLantern && p.Variant == 1), 1, "a tall lantern with a tiled cap");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.PrunedPine), 3, "pines pruned on lava rock");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.LavaRock), 6, "rocks of the volcano");
            List<CityProp> channels = recipe.PlazaProps.Where(p => p.Kind == CityPropKind.LavaSluice).ToList();
            Assert.GreaterOrEqual(channels.Count, 2, "channels of tamed lava with their sluices");
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.LavaSluice));
            Assert.IsFalse(TownKit.IsSegment(CityPropKind.IsleForge));
            Assert.IsNull(CitySolids.PropSolid(channels[0], layout.WalkBounds), "the black stone bridges are walkable");
            foreach (CityProp channel in channels)
            {
                Assert.IsTrue(layout.Streets.Any(s => s.Road.Contains((channel.Position + channel.End) * 0.5f)), $"the bridge at {channel.Position} carries a street");
            }

            Assert.IsTrue(CitySolids.PropSolid(forge, layout.WalkBounds).HasValue, "the forge's hearth stops the walker");
            List<CityLot> houses = layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).ToList();
            Assert.IsTrue(houses.All(l => l.Building.LacquerHall && !l.Building.MoonLonghouse && !l.Building.SnowDome && !l.Building.RingWallHall),
                "every house is a hall of red lacquer (not Mingcheng's ring halls)");
            Assert.AreEqual(TownShopDressing.BlackIronForge, recipe.Lots.Single(l => l.ResidentId == "kurogane").Spec.Dressing);
            Assert.IsFalse(layout.Streets.Any(s => s.Arm == CityArm.North), "no street to the north: the palace courts and the volcano");
            Assert.AreEqual(TownSwatch.VolcanicStone, TownStyles.LacquerPalace.Ground, "the isle is black volcanic stone");
            Assert.Greater(TownPalette.GlowOf(TownSwatch.LavaGlow), 0.5f, "the tamed lava glows");
            Assert.AreEqual(0f, TownPalette.GlowOf(TownSwatch.PalaceLacquer), "the lacquer does not glow");
            Assert.AreEqual(TownPalette.Columns * TownPalette.Rows, TownPalette.Count);
            Assert.AreEqual(NpcPalette.Columns * NpcPalette.Rows, NpcPalette.Count);

            foreach (CityLot lot in houses)
            {
                var b = new TownMeshBuilder();
                TownBuildingBuilder.Build(b, lot.Building);
                Assert.Greater(b.TriangleCount, 40, $"{lot.Id} built (almost) nothing");
                Assert.Less(b.TriangleCount, 6000, $"{lot.Id} is over the per-building budget");
            }

            foreach (CityProp prop in recipe.PlazaProps.Where(p => p.Kind >= CityPropKind.IsleForge && p.Kind <= CityPropKind.LavaRock))
            {
                var b = new TownMeshBuilder();
                TownKit.Prop(b, prop, TownStyles.LacquerPalace);
                Assert.Greater(b.TriangleCount, 10, $"{prop.Kind} built (almost) nothing");
                Assert.Less(b.TriangleCount, 4000, $"{prop.Kind} is over budget");
            }

            var gate = new TownMeshBuilder();
            TownKit.Gate(gate, new Vector3(-2f, 0f, -19f), new Vector3(2f, 0f, -19f), TownStyles.LacquerPalace);
            Assert.Greater(gate.TriangleCount, 40, "the gate of red posts under a tiled roof");
        }

        [Test]
        public void Hinokura_FitsACircleOfRadius22_AtGroundLevel_WithThreeRoadsOut()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Hinokura);
            float radius = CityTownRecipes.HinokuraSolidRadius;
            Assert.LessOrEqual(radius, 24f, "the overworld will keep a circle of ~24 m");
            foreach (CitySolid solid in layout.Solids)
            {
                Assert.LessOrEqual(Reach(solid), radius, $"{solid.Owner} at {solid.Footprint} leaves the circle");
            }

            foreach (CityLot lot in layout.Lots)
            {
                Assert.LessOrEqual(Reach(lot.Footprint), radius, lot.Id);
            }

            CollectionAssert.AreEquivalent(new[] { CityArm.West, CityArm.East, CityArm.South }, layout.Streets.Select(s => s.Arm).ToArray());
            List<CityExit> exits = layout.Exits.Where(e => e.Kind == CityExitKind.ToMap).ToList();
            Assert.AreEqual(3, exits.Count);
            foreach (CityExit exit in exits)
            {
                Assert.LessOrEqual(exit.Target.magnitude, radius, exit.Id);
                Assert.IsTrue(layout.WalkBounds.Contains(exit.Target), exit.Id);
            }

            Assert.AreEqual(CityTownRecipes.HinokuraHalfExtent, layout.WalkBounds.xMax, 0.001f);
            Assert.AreEqual(CityArm.South, CityTownRecipes.Hinokura().EntryArm, "the road in from the stone harbour");
        }

        // ------------------------------------------------------------------
        // The cast
        // ------------------------------------------------------------------

        [Test]
        public void Hinokura_Residents_AreTheFichas_PeopleOfTheForgeIsleWithALookOfTheirOwn()
        {
            CitySpec hinokura = CityCatalog.Get(CityCatalog.Hinokura);
            CollectionAssert.AreEquivalent(Cast, hinokura.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            var swatches = new[] { NpcSwatch.HinoLacquer, NpcSwatch.VolcanicStone, NpcSwatch.CoralWhite, NpcSwatch.Pumice, NpcSwatch.ObsidianBlack, NpcSwatch.HinoTile };
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.IsFalse(look.HorseBody || look.ElfEars || look.Horns || look.Gills || look.Tusks || look.Twigs || look.Satyr || look.Snout, $"{id}: human");
                Assert.IsTrue(swatches.Contains(look.Main) || swatches.Contains(look.LayerColor), $"{id}: dressed in the colours of the forge isle");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
                Assert.IsFalse(look.Beard == NpcBeard.LongBraided && (look.Layers & NpcLayer.Cape) != 0, $"{id}: a long braided beard and a cape blow the budget");
                Assert.LessOrEqual(NpcModelBuilder.CountTriangles(look), 1500, $"{id} is over the mobile budget");
            }

            Assert.Greater(NpcRecipes.Find("kurogane").Height, NpcRecipes.Named.Where(r => Cast.Contains(r.Id) && r.Id != "kurogane").Max(r => r.Height), "Kurogane is the tallest of the isle");
            Assert.AreEqual(CityShopCatalog.FerroPretoKurogane, hinokura.Residents.Single(r => r.ResidentId == "kurogane").ShopId, "Kurogane keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.FerroPretoKurogane }, hinokura.ShopIds.ToArray());
        }

        [Test]
        public void Hinokura_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotContain("Dona", pt);
                StringAssert.StartsWith(FichaNames[id], pt, $"{id}: named after the ficha ({pt})");
            }

            Assert.AreEqual("Mestra Akane, a Mestra da Laca", CityTexts.ResidentName(null, Resident("akane")));
            Assert.AreEqual("Kurogane, o Ferreiro do Ferro Preto", CityTexts.ResidentName(null, Resident("kurogane")));
        }

        [Test]
        public void Hinokura_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            foreach (bool mascate in new[] { false, true })
            {
                List<string> problems = CityLayoutRules.Validate(CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.Hinokura, mascate)));
                Assert.IsEmpty(problems, (mascate ? "+caravaneer\n" : string.Empty) + string.Join("\n", problems));
            }

            // Ammo's stall (PlazaRadius · (0.575, −0.675)): nothing solid in front of his counter.
            CityTownRecipe recipe = CityTownRecipes.Hinokura();
            var stall = new Vector2(recipe.PlazaRadius * 0.575f, -recipe.PlazaRadius * 0.675f);
            CityLayout withAmmo = CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.Hinokura, true));
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(withAmmo, stall + Vector2.up * 1.4f), CityLayoutRules.WalkerRadius, "the cousins can step up to Ammo's counter");

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Hinokura);
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

            Assert.AreEqual("ferro_preto_do_kurogane", layout.LotOfShop(CityShopCatalog.FerroPretoKurogane).Id);
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "the stone harbour");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn on the plaza");
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, layout.Respawn), CityLayoutRules.WalkerRadius, "nothing stands on the respawn");
        }

        // ------------------------------------------------------------------
        // The Ferro Preto do Kurogane
        // ------------------------------------------------------------------

        [Test]
        public void FerroPretoDoKurogane_SellsTheGearOfTheArchipelago_AtBalancedPrices_TurnsDaily_NeverTheLifeOrChaosStones()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.FerroPretoKurogane);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Forest, shop.HomeRegion);
            Assert.AreEqual(ShopRotationPeriod.Daily, shop.Period);

            // The gear the maps 65–70 will ask for (reef and hot channels, the volcano's walls, fumaroles, red bridges, heat),
            // fixed from the first day. When the maps enter MapCatalog, MapRequirementTests checks their tags against this counter.
            var gear = new[]
            {
                MapRequirementItems.DhowPassage, MapRequirementItems.ReedBoat, MapRequirementItems.ClimbingCrampons, MapRequirementItems.ClimbingRope,
                MapRequirementItems.Theriac, MapRequirementItems.AntiVenomAmulet, MapRequirementItems.BalancePole, MapRequirementItems.Waterskin,
            };
            foreach (string fixedItem in gear)
            {
                CityShopEntry entry = CityShopCatalog.EntryOf(shop.ShopId, fixedItem);
                Assert.IsNotNull(entry, $"Kurogane sells {fixedItem}");
                Assert.IsFalse(entry.Rotating, $"{fixedItem} is always on the counter");
                Assert.IsFalse(entry.Unlock.IsGated, $"{fixedItem} from the first day");
            }

            Assert.IsNull(CityShopCatalog.EntryOf(shop.ShopId, "item_pedra_vida"), "the life stone is never sold");
            Assert.IsNull(CityShopCatalog.EntryOf(shop.ShopId, "item_pedra_caos"), "the chaos stone is never sold");

            foreach (CityShopEntry entry in shop.Entries.Where(e => e.IsBagItem))
            {
                ItemDefinition item = ItemDatabase.GetById(entry.ItemId);
                Assert.IsNotNull(item, entry.ItemId);
                Assert.That(entry.Price, Is.InRange(Mathf.FloorToInt(item.Value * 0.8f), Mathf.CeilToInt(item.Value * 1.25f)), entry.ItemId);
            }

            Assert.Greater(shop.Entries.Count(e => e.Rotating && e.Pool == ShopPool.Main), shop.RotatingSlots, "more lines than slots: the counter turns");
            long day = ShopRotation.DayIndex(new DateTime(2026, 11, 6));
            var counters = new HashSet<string>();
            for (int d = 0; d < 14; d++)
            {
                List<string> today = ShopRotation.PickStock(shop, day + d, null).Select(e => e.ItemId).ToList();
                Assert.Contains(MapRequirementItems.ReedBoat, today);
                Assert.Contains(MapRequirementItems.ClimbingCrampons, today);
                counters.Add(string.Join(",", today));
            }

            Assert.Greater(counters.Count, 1, "the counter changes from day to day");

            CitySpec hinokura = CityCatalog.Get(CityCatalog.Hinokura);
            for (int d = 0; d < 28; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(hinokura, new DateTime(2026, 11, 6).AddDays(d), null);
                Assert.Contains(CityShopCatalog.FerroPretoKurogane, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
            }
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Hinokura_HasTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec hinokura = CityCatalog.Get(CityCatalog.Hinokura);
            var keys = new List<string> { hinokura.NameKey, hinokura.ButtonKey, hinokura.FlavourKey, CityShopCatalog.Get(CityShopCatalog.FerroPretoKurogane).NameKey };
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
            StringAssert.Contains("lava", all, "the tamed lava");
            StringAssert.Contains("laca", all, "the red lacquer");
            StringAssert.Contains("Fogo Vivo", CityTexts.ResidentLine(null, Resident("hikari"), CharacterId.Icaro), "Ícaro's element");
            StringAssert.Contains("Forja de Hinokura", CityTexts.ResidentLine(null, Resident("kurogane"), CharacterId.Icaro), "Ícaro's G4 is here");
            foreach (string folk in new[] { "Saci", "Curupira", "Iara", "Seu ", "Dona " })
            {
                StringAssert.DoesNotContain(folk, all);
            }
        }

        // ------------------------------------------------------------------
        // City quests
        // ------------------------------------------------------------------

        [Test]
        public void CityQuests_AreGivenByAkaneAndSango_OnExistingEnemies_WithAReedBoatAsTheLastReward()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.QuestsIn(CityCatalog.Hinokura).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "akane", "sango" }, quests.Select(q => q.GiverResidentId).ToArray());
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
                Assert.IsFalse(spec.Quest.Rewards.Any(r => r.ItemId == "item_pedra_vida" || r.ItemId == "item_pedra_caos"), spec.QuestId);
                foreach (string suffix in new[] { ".title", ".description", ".journal", ".offer", ".active" })
                {
                    Assert.IsNotNull(CityQuestTexts.GetFallback(spec.KeyBase + suffix), spec.KeyBase + suffix);
                }
            }

            Assert.IsTrue(quests[1].Quest.Rewards.Any(r => r.ItemId == MapRequirementItems.ReedBoat), "Sango pays with a reed boat (the reef, 68)");
            Assert.AreEqual(CityQuestCatalog.Quests.Count, CityQuestCatalog.Quests.Select(q => q.QuestId).Distinct().Count());

            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("akane", active.Contains, done.Contains, out CityQuestSpec crabs));
            Assert.AreEqual(CityQuestCatalog.HinokuraCrabs, crabs.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("sango", active.Contains, done.Contains, out _), "the chain starts with the lacquer master");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("kurogane", active.Contains, done.Contains, out _), "Kurogane gives no quest");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("hikari", active.Contains, done.Contains, out _), "Hikari gives no quest");
            done.Add(CityQuestCatalog.HinokuraCrabs);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("sango", active.Contains, done.Contains, out CityQuestSpec gulls));
            Assert.AreEqual(CityQuestCatalog.HinokuraEmberGulls, gulls.QuestId);
        }
    }
}
