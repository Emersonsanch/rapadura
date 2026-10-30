using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Housing.Visuals;
using Rapadura.Gameplay.Inventory;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// H4 "Melhorar a casa" (Housing.md §2.2, §3.2, §7.3, §13): requirements per level (hero level, boss, gold,
    /// materials, keepsakes), all-or-nothing payment, one level at a time, nothing lost on the bigger grid (pieces that
    /// no longer fit go to the Depósito), the modular hull of N2–N5 within the §3.5 budgets, the "Missões da Casa" 2–6,
    /// and the v2 save (old H1 saves still load).
    /// </summary>
    public class HouseUpgradeTests
    {
        /// <summary>A bag that only counts.</summary>
        private sealed class FakeWallet : IHouseWallet
        {
            public readonly Dictionary<string, int> Items = new Dictionary<string, int>();

            public FakeWallet With(string id, int quantity)
            {
                Items[id] = (Items.TryGetValue(id, out int have) ? have : 0) + quantity;
                return this;
            }

            public int Count(string itemId) => itemId != null && Items.TryGetValue(itemId, out int have) ? have : 0;

            public bool Remove(string itemId, int quantity)
            {
                if (Count(itemId) < quantity) return false;
                Items[itemId] -= quantity;
                return true;
            }
        }

        private readonly HashSet<string> _bosses = new HashSet<string>();
        private int _day = 1;

        private HouseService NewHouse()
        {
            var house = new HouseService(() => 3)
            {
                BossDefeatedProvider = id => _bosses.Contains(id),
                GameDayProvider = () => _day,
            };
            house.GrantHouse();
            house.MarkEntered();
            return house;
        }

        [SetUp]
        public void SetUp()
        {
            _bosses.Clear();
            _day = 1;
        }

        private static FakeWallet RichWallet() => new FakeWallet()
            .With(HouseUpgradeRules.GoldId, 9999).With(HouseUpgradeRules.WoodId, 999).With(HouseUpgradeRules.FiberId, 999)
            .With(HouseUpgradeRules.StoneId, 999).With(HouseUpgradeRules.IronId, 999).With(HouseQuestChain.CookedMeatId, 99);

        // ------------------------------------------------------------------
        // Requirements
        // ------------------------------------------------------------------

        [Test]
        public void Evaluate_N1ToN2_NeedsLevelBossGoldAndMaterials()
        {
            HouseUpgradeCheck check = HouseUpgradeRules.Evaluate(1, true, 5, _ => false, _ => 0, _ => false);
            Assert.AreEqual(2, check.ToLevel);
            Assert.AreEqual(HouseUpgradeBlock.Requirements, check.Block);
            Assert.AreEqual(new[] { HouseRequirementKind.CharacterLevel, HouseRequirementKind.Boss, HouseRequirementKind.Gold, HouseRequirementKind.Material, HouseRequirementKind.Material, HouseRequirementKind.Material },
                check.Rows.Select(r => r.Kind).ToArray());
            Assert.AreEqual(8, check.Rows[0].Need);
            Assert.AreEqual("enemy_forest_alpha_wolf", check.Rows[1].Id);
            Assert.AreEqual(150, check.Rows[2].Need);

            FakeWallet wallet = RichWallet();
            HouseUpgradeCheck ready = HouseUpgradeRules.Evaluate(1, true, 8, id => id == "enemy_forest_alpha_wolf", wallet.Count, _ => false);
            Assert.IsTrue(ready.CanUpgrade);
            Assert.IsFalse(HouseUpgradeRules.Evaluate(1, true, 7, id => true, wallet.Count, _ => false).CanUpgrade, "hero level 7 < 8");
            Assert.IsFalse(HouseUpgradeRules.Evaluate(1, true, 8, id => false, wallet.Count, _ => false).CanUpgrade, "boss still free");
        }

        [Test]
        public void Evaluate_NoHouse_AndMaxLevel_AreBlocked()
        {
            Assert.AreEqual(HouseUpgradeBlock.NoHouse, HouseUpgradeRules.Evaluate(1, false, 50, _ => true, _ => 9999, _ => true).Block);
            Assert.AreEqual(HouseUpgradeBlock.MaxLevel, HouseUpgradeRules.Evaluate(5, true, 50, _ => true, _ => 9999, _ => true).Block);
        }

        [Test]
        public void EveryLevel_GatesOnTheDocsBossAndLevel_PricesProvisional()
        {
            Assert.AreEqual(new[] { 1, 8, 18, 28, 38 }, HouseLevels.All.Select(l => l.RequiredCharacterLevel).ToArray());
            Assert.AreEqual(new[] { null, "enemy_forest_alpha_wolf", "enemy_desert_ruin_guardian", "enemy_mountain_warlord", "enemy_cave_dweller" },
                HouseLevels.All.Select(l => l.RequiredBossId).ToArray());
            Assert.AreEqual(30, HouseLevels.Get(2).UpgradeMaterials.First(m => m.ItemId == HouseUpgradeRules.WoodId).Quantity);
            Assert.AreEqual(100, HouseLevels.Get(5).UpgradeMaterials.First(m => m.ItemId == HouseUpgradeRules.StoneId).Quantity);
            CollectionAssert.AreEquivalent(HouseUpgradeRules.BossKeepsakes, HouseLevels.Get(5).RequiredKeepsakes);
            foreach (string keepsake in HouseUpgradeRules.BossKeepsakes) Assert.IsNotNull(HouseCatalog.Get(keepsake), keepsake);
        }

        [Test]
        public void N5_NeedsOneKeepsakeOfEachBoss_InTheHouse_AndNeverSpendsThem()
        {
            HouseService house = NewHouse();
            foreach (string boss in HouseUpgradeRules.BossIds) _bosses.Add(boss);
            house.ApplyLevel(4);
            FakeWallet wallet = RichWallet();
            Assert.IsFalse(house.CheckUpgrade(40, wallet).CanUpgrade, "no keepsakes yet");
            foreach (string keepsake in HouseUpgradeRules.BossKeepsakes) house.Layout.AddToDepot(keepsake);
            Assert.IsTrue(house.CheckUpgrade(40, wallet).CanUpgrade, "in the Depósito counts as in the house");
            HouseUpgradeResult result = house.TryUpgrade(40, wallet);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(5, house.Level);
            foreach (string keepsake in HouseUpgradeRules.BossKeepsakes) Assert.IsTrue(house.HasInHouse(keepsake), keepsake + " stays");
        }

        [Test]
        public void Pay_IsAllOrNothing()
        {
            var wallet = new FakeWallet().With("item_gold", 100).With("item_wood", 3);
            var costs = new[] { new HouseMaterialCost("item_gold", 50), new HouseMaterialCost("item_wood", 5) };
            Assert.IsFalse(HouseUpgradeRules.Pay(costs, wallet));
            Assert.AreEqual(100, wallet.Count("item_gold"), "nothing taken when something is missing");
            wallet.With("item_wood", 2);
            Assert.IsTrue(HouseUpgradeRules.Pay(costs, wallet));
            Assert.AreEqual(50, wallet.Count("item_gold"));
            Assert.AreEqual(0, wallet.Count("item_wood"));
            Assert.AreEqual(new[] { 7 }, HouseUpgradeRules.Merge(new[] { new HouseMaterialCost("a", 3), new HouseMaterialCost("a", 4) }).Select(c => c.Quantity).ToArray());
        }

        [Test]
        public void HouseInventoryWallet_CountsBagAndChest_SpendsTheChestWhenTheBagRunsOut()
        {
            var chest = new VillageChestStorage(10);
            chest.Store(new InventorySlotData { itemId = "item_wood", quantity = 1 }, 7);
            var wallet = new HouseInventoryWallet(null, chest);
            Assert.AreEqual(7, wallet.Count("item_wood"));
            Assert.IsFalse(wallet.Remove("item_wood", 8));
            Assert.AreEqual(7, chest.GetTotalCount("item_wood"));
            Assert.IsTrue(wallet.Remove("item_wood", 5));
            Assert.AreEqual(2, chest.GetTotalCount("item_wood"));
        }

        // ------------------------------------------------------------------
        // Upgrading: nothing lost
        // ------------------------------------------------------------------

        [Test]
        public void TryUpgrade_PaysAndGoesUpOneLevel_KeepingEveryPiece()
        {
            HouseService house = NewHouse();
            HouseLayout layout = house.Layout;
            Assert.AreEqual(HousePlacementResult.Ok, layout.Place(HousePlacementRequest.At("casa_quarto_016", HouseLayer.Floor, 6, 5, 0), out _));
            Assert.AreEqual(HousePlacementResult.Ok, layout.Place(HousePlacementRequest.At("casa_util_001", HouseLayer.Floor, 0, 7, 0), out _));
            Assert.AreEqual(HousePlacementResult.Ok, layout.Place(HousePlacementRequest.At("casa_sala_004", HouseLayer.Floor, 4, 2, 1), out int table));
            Assert.AreEqual(HousePlacementResult.Ok, layout.Place(HousePlacementRequest.OnSurface("casa_luz_001", table, 2, 5), out _));
            Assert.AreEqual(HousePlacementResult.Ok, layout.Place(HousePlacementRequest.At("casa_quintal_002", HouseLayer.Yard, 5, 2), out int garden));
            house.Chest.Store(new InventorySlotData { itemId = "test_keep", quantity = 1 }, 3);
            int before = HouseUpgradeRules.TotalItems(layout);
            int placedBefore = layout.Placed.Count;

            FakeWallet wallet = RichWallet();
            Assert.IsFalse(house.TryUpgrade(8, wallet).Success, "Lobo Alfa not beaten");
            _bosses.Add("enemy_forest_alpha_wolf");
            HouseUpgradeResult result = house.TryUpgrade(8, wallet);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.FromLevel);
            Assert.AreEqual(2, result.ToLevel);
            Assert.AreEqual(2, house.Level);
            Assert.AreEqual(14, house.Spec.InteriorWidth);
            Assert.AreEqual(9999 - 150, wallet.Count(HouseUpgradeRules.GoldId));
            Assert.AreEqual(999 - 30, wallet.Count(HouseUpgradeRules.WoodId));
            Assert.AreEqual(before, HouseUpgradeRules.TotalItems(layout), "nothing is ever lost");
            Assert.AreEqual(placedBefore, layout.Placed.Count + result.MovedToDepot);
            Assert.AreEqual(30, house.Chest.SlotCount, "N2 chest");
            Assert.AreEqual(3, house.Chest.GetTotalCount("test_keep"));
            HousePlacedData yard = layout.Find(garden);
            Assert.IsNotNull(yard);
            Assert.AreEqual(new Vector2Int(6, 4), new Vector2Int(yard.x, yard.y), "the yard grows around the house: same place relative to the porch");
            Assert.IsFalse(house.TryUpgrade(50, wallet).Success, "one level at a time: N3 needs the Guardião");
        }

        [Test]
        public void Upgrade_APieceThatNoLongerFits_GoesToTheDepot()
        {
            HouseService house = NewHouse();
            // N1's door is at columns 2–3; N2's at 4–5: a stool right inside N2's door would block it.
            Assert.AreEqual(HousePlacementResult.Ok, house.Layout.Place(HousePlacementRequest.At("casa_sala_008", HouseLayer.Floor, 4, 0, 0), out int stool));
            int stoolsInDepot = house.Layout.DepotCount("casa_sala_008");
            int total = HouseUpgradeRules.TotalItems(house.Layout);

            int moved = house.ApplyLevel(2);

            Assert.AreEqual(1, moved);
            Assert.IsNull(house.Layout.Find(stool));
            Assert.AreEqual(stoolsInDepot + 1, house.Layout.DepotCount("casa_sala_008"));
            Assert.AreEqual(total, HouseUpgradeRules.TotalItems(house.Layout));
        }

        [Test]
        public void Restore_IntoTheNextLevel_ViaShift_KeepsEveryCopy_ForEveryStep()
        {
            HouseService house = NewHouse();
            foreach (string id in HouseRules.StarterItems.Concat(HouseRules.FirstQuestRewards).Distinct())
            {
                HouseItemDefinition item = HouseCatalog.Get(id);
                HouseLayer layer = HousePlacementRules.DefaultLayer(item);
                if (layer == HouseLayer.Surface || layer == HouseLayer.Wall) continue;
                for (int x = 0; x < 9; x++)
                {
                    if (house.Layout.Place(HousePlacementRequest.At(id, layer, x, 3, 0), out _) == HousePlacementResult.Ok) break;
                }
            }

            int total = HouseUpgradeRules.TotalItems(house.Layout);
            for (int level = 2; level <= HouseLevels.MaxLevel; level++)
            {
                house.ApplyLevel(level);
                Assert.AreEqual(level, house.Level);
                Assert.AreEqual(total, HouseUpgradeRules.TotalItems(house.Layout), $"N{level}: nothing lost");
            }
        }

        [Test]
        public void ShiftForUpgrade_MovesOnlyTheYard()
        {
            var placed = new[]
            {
                new HousePlacedData { instanceId = 1, itemId = "a", layer = HouseLayer.Floor, x = 3, y = 2 },
                new HousePlacedData { instanceId = 2, itemId = "b", layer = HouseLayer.Yard, x = 5, y = 2 },
            };
            HousePlacedData[] shifted = HouseUpgradeRules.ShiftForUpgrade(placed, HouseLevels.Get(1), HouseLevels.Get(5));
            Assert.AreEqual(3, shifted[0].x);
            Assert.AreEqual(2, shifted[0].y);
            Assert.AreEqual(5 + (18 - 8) / 2, shifted[1].x);
            Assert.AreEqual(2 + (18 - 8), shifted[1].y);
            Assert.AreEqual(5, placed[1].x, "input untouched");
        }

        // ------------------------------------------------------------------
        // The modular hull
        // ------------------------------------------------------------------

        [Test]
        public void Hull_EveryLevel_StaysWithinItsBudget_AndSwapsModulesPerSocket()
        {
            int[] budgets = { 2500, 4000, 6000, 9000, 14000 };
            for (int level = 1; level <= 5; level++)
            {
                HouseHullMeshes hull = HouseHullBuilder.Build(HouseLevels.Get(level));
                try
                {
                    Assert.LessOrEqual(hull.TriangleCount, budgets[level - 1], $"N{level} hull (§3.5)");
                    Assert.Greater(hull.Base.GetIndexCount(0), 0);
                    Assert.Greater(hull.Roof.GetIndexCount(0), 0);
                    foreach (Mesh wall in hull.Walls) Assert.Greater(wall.GetIndexCount(0), 0, $"N{level} wall");
                    StringAssert.StartsWith("HouseN" + level, hull.Base.name);
                }
                finally
                {
                    Object.DestroyImmediate(hull.Base);
                    Object.DestroyImmediate(hull.Roof);
                    foreach (Mesh wall in hull.Walls) Object.DestroyImmediate(wall);
                }
            }

            foreach (HouseHullSocket socket in System.Enum.GetValues(typeof(HouseHullSocket)))
            {
                var modules = Enumerable.Range(1, 5).Select(level => HouseHullBuilder.ModuleId(socket, level)).ToList();
                if (socket == HouseHullSocket.Wing) Assert.AreEqual("ala_intima", modules[4], "the casarão's second wing");
                else if (socket != HouseHullSocket.Chimney) Assert.AreEqual(5, modules.Distinct().Count(), $"{socket}: one module per level");
            }

            Assert.AreEqual(1.5f, HouseHullBuilder.PorchDepthOf(HouseLevels.Get(1)), 1e-4f, "N1 keeps its alpendre (and its yard origin)");
            Assert.Greater(HouseHullBuilder.PorchDepthOf(HouseLevels.Get(2)), 1.5f, "N2 has a real varanda");
        }

        [Test]
        public void Openings_NeverOverlap_AndBiggerHousesHaveMoreWindows()
        {
            int previous = 0;
            foreach (HouseLevelSpec spec in HouseLevels.All)
            {
                Assert.GreaterOrEqual(spec.Openings.Count, previous);
                previous = spec.Openings.Count;
                foreach (IGrouping<HouseWallSide, HouseOpening> wall in spec.Openings.GroupBy(o => o.Side))
                {
                    var list = wall.ToList();
                    for (int i = 0; i < list.Count; i++)
                    {
                        Assert.IsTrue(list[i].WallRect.Inside(spec.WallLength(wall.Key), HouseLevelSpec.WallRows), $"N{spec.Level} {wall.Key}");
                        for (int j = i + 1; j < list.Count; j++) Assert.IsFalse(list[i].WallRect.Overlaps(list[j].WallRect), $"N{spec.Level} {wall.Key}");
                    }
                }
            }
        }

        // ------------------------------------------------------------------
        // Missões da Casa 2–6
        // ------------------------------------------------------------------

        [Test]
        public void HouseQuests_Deliveries_PayAndRewardTheDepot()
        {
            HouseService house = NewHouse();
            HouseQuestDefinition agua = HouseQuestChain.Get(HouseQuestChain.AguaFresca);
            HouseQuestDefinition fogo = HouseQuestChain.Get(HouseQuestChain.FogoNaCozinha);
            var wallet = new FakeWallet().With(HouseUpgradeRules.StoneId, 9).With(HouseUpgradeRules.WoodId, 5);
            Assert.AreEqual(HouseQuestState.Active, house.QuestState(agua, wallet));
            Assert.IsFalse(house.TryClaimQuest(agua.Id, wallet));
            wallet.With(HouseUpgradeRules.StoneId, 1);
            Assert.AreEqual(HouseQuestState.Ready, house.QuestState(agua, wallet));
            Assert.IsTrue(house.TryClaimQuest(agua.Id, wallet));
            Assert.AreEqual(0, wallet.Count(HouseUpgradeRules.StoneId));
            Assert.AreEqual(1, house.Layout.DepotCount("casa_quintal_001"), "Poço de pedra com sarilho");
            Assert.AreEqual(HouseQuestState.Completed, house.QuestState(agua, wallet));
            Assert.IsFalse(house.TryClaimQuest(agua.Id, wallet), "once");

            wallet.With(HouseQuestChain.CookedMeatId, 3).With(HouseUpgradeRules.WoodId, 8);
            Assert.AreEqual(HouseQuestState.Locked, house.QuestState(fogo, wallet), "after the Lobo Alfa");
            _bosses.Add("enemy_forest_alpha_wolf");
            Assert.IsTrue(house.TryClaimQuest(fogo.Id, wallet));
            Assert.AreEqual(1, house.Layout.DepotCount("casa_cozinha_001"));
            Assert.AreEqual(1, house.Layout.DepotCount("casa_cozinha_002"));
        }

        [Test]
        public void HouseQuests_LockedUntilTheFirstQuestIsDone()
        {
            var house = new HouseService(() => 3) { BossDefeatedProvider = _ => true };
            house.GrantHouse();
            Assert.AreEqual(HouseQuestState.Locked, house.QuestState(HouseQuestChain.Get(HouseQuestChain.AguaFresca), RichWallet()));
        }

        [Test]
        public void HouseQuests_UpgradeQuestsCompleteWithTheUpgrade_CasaCheiaNeedsFourCousins()
        {
            HouseService house = NewHouse();
            foreach (string boss in HouseUpgradeRules.BossIds) _bosses.Add(boss);
            FakeWallet wallet = RichWallet();
            Assert.IsTrue(house.TryUpgrade(50, wallet).Success);
            HouseUpgradeResult toN3 = house.TryUpgrade(50, wallet);
            CollectionAssert.Contains(toN3.CompletedQuests, HouseQuestChain.ParedeDeBarro);
            Assert.AreEqual(1, house.Layout.DepotCount("casa_quarto_006"), "Colcha de retalhos");
            CollectionAssert.Contains(house.OwedWardrobe, "roupa_comum_010", "the sweater waits for the wardrobe (H5)");
            HouseUpgradeResult toN4 = house.TryUpgrade(50, wallet);
            CollectionAssert.Contains(toN4.CompletedQuests, HouseQuestChain.TijoloPorTijolo);
            Assert.AreEqual(1, house.Layout.DepotCount("casa_util_005"), "Baú-correio");
            foreach (string keepsake in HouseUpgradeRules.BossKeepsakes) house.Layout.AddToDepot(keepsake);
            HouseUpgradeResult toN5 = house.TryUpgrade(50, wallet);
            Assert.IsTrue(toN5.Success);
            CollectionAssert.DoesNotContain(toN5.CompletedQuests, HouseQuestChain.CasaCheia, "no cousin signed yet");

            // Place the guest book and let the days go by: the cousins come and sign.
            Assert.AreEqual(HousePlacementResult.Ok, house.Layout.Place(HousePlacementRequest.At("casa_sala_004", HouseLayer.Floor, 10, 8, 0), out int table));
            Assert.AreEqual(HousePlacementResult.Ok, house.Layout.Place(HousePlacementRequest.OnSurface("casa_util_023", table, 0), out _));
            for (_day = 2; _day < 60 && !house.IsHouseQuestDone(HouseQuestChain.CasaCheia); _day++)
            {
                house.ComeHome("Maithe", false);
            }

            Assert.IsTrue(house.IsHouseQuestDone(HouseQuestChain.CasaCheia), "four cousins signed within two months of game days");
            Assert.Less(_day, 30, "and well before that, thanks to the priority for cousins who have not signed");
            Assert.AreEqual(1, house.Layout.DepotCount("casa_familia_001"), "Retrato de casamento");
        }

        // ------------------------------------------------------------------
        // Save
        // ------------------------------------------------------------------

        [Test]
        public void Save_V2_RoundTripsLevelRestPlantsGuestsAndQuests()
        {
            HouseService house = NewHouse();
            _bosses.Add("enemy_forest_alpha_wolf");
            Assert.IsTrue(house.TryUpgrade(8, RichWallet()).Success);
            Assert.AreEqual(HousePlacementResult.Ok, house.Layout.Place(HousePlacementRequest.At("casa_planta_001", HouseLayer.Ceiling, 3, 3, 0), out int fern));
            Assert.AreEqual(HousePlacementResult.Ok, house.Layout.Place(HousePlacementRequest.At("casa_sala_004", HouseLayer.Floor, 8, 6, 0), out int table));
            Assert.AreEqual(HousePlacementResult.Ok, house.Layout.Place(HousePlacementRequest.OnSurface("casa_util_023", table, 0), out _));
            house.Clock = new FixedClock(new System.DateTime(2026, 10, 30));
            Assert.IsTrue(house.RestInBed());
            house.ConsumeRestedFight();
            _day = 2;
            Assert.IsNotNull(house.TryVisit("Maria"));
            Assert.IsTrue(house.TryClaimQuest(HouseQuestChain.AguaFresca, RichWallet()));

            string json = JsonUtility.ToJson((HouseSaveState)house.CaptureState());
            var saved = JsonUtility.FromJson<HouseSaveState>(json);
            Assert.AreEqual(HouseService.SaveVersion, saved.version);
            Assert.AreEqual(2, saved.level);
            Assert.AreEqual(house.Comfort.Total, saved.comfortCache);
            Assert.AreEqual(1, saved.plants.Length);

            var loaded = new HouseService(() => 3) { GameDayProvider = () => _day, BossDefeatedProvider = id => _bosses.Contains(id), Clock = house.Clock };
            loaded.RestoreState(saved);
            Assert.AreEqual(2, loaded.Level);
            Assert.AreEqual(14, loaded.Spec.InteriorWidth);
            Assert.AreEqual(house.Rested.fightsLeft, loaded.Rested.fightsLeft);
            Assert.AreEqual(house.Rested.percent, loaded.Rested.percent);
            Assert.AreEqual(1, loaded.RestChargesLeft);
            Assert.AreEqual(1, loaded.PlantTimers.Count);
            Assert.AreEqual(fern, loaded.PlantTimers[0].instanceId);
            Assert.AreEqual(1, loaded.GuestBook.Count);
            Assert.AreEqual(2, loaded.LastVisitDay);
            Assert.IsTrue(loaded.IsHouseQuestDone(HouseQuestChain.AguaFresca));
            Assert.AreEqual(json, JsonUtility.ToJson((HouseSaveState)loaded.CaptureState()), "capture → restore → capture is stable");
        }

        [Test]
        public void Save_OldH1Json_StillLoads()
        {
            // A save written by H1: version 1, none of the v2 fields.
            const string h1 = "{\"version\":1,\"ownerAccountId\":\"local\",\"cityId\":\"vila_zefa\",\"level\":1,\"houseQuestStep\":2,\"nextInstanceId\":3," +
                "\"placed\":[{\"instanceId\":1,\"itemId\":\"casa_util_001\",\"layer\":0,\"wall\":0,\"x\":0,\"y\":7,\"rotation\":0,\"mirrored\":false,\"parentInstanceId\":0,\"slot\":0}," +
                "{\"instanceId\":2,\"itemId\":\"casa_planta_004\",\"layer\":4,\"wall\":0,\"x\":0,\"y\":0,\"rotation\":0,\"mirrored\":false,\"parentInstanceId\":9,\"slot\":0}]," +
                "\"depot\":[{\"itemId\":\"casa_sala_004\",\"quantity\":1}],\"chest\":[],\"lastSleepDay\":4}";
            var saved = JsonUtility.FromJson<HouseSaveState>(h1);
            var house = new HouseService(() => 3) { GameDayProvider = () => 7 };
            house.RestoreState(saved);
            Assert.IsTrue(house.HasHouse);
            Assert.AreEqual(1, house.Level);
            Assert.AreEqual(4, house.LastSleepDay);
            Assert.AreEqual(1, house.Layout.Placed.Count);
            Assert.AreEqual(1, house.Layout.DepotCount("casa_planta_004"), "orphan surface piece kept in the depot");
            Assert.IsFalse(house.IsRested);
            Assert.AreEqual(HouseRestedRules.ChargesPerDay, house.RestChargesLeft);
            Assert.AreEqual(0, house.GuestBook.Count);
            Assert.AreEqual(0, house.CompletedHouseQuests.Count);
            Assert.AreEqual(0, house.LastVisitDay);
            Assert.AreEqual(HouseService.SaveVersion, ((HouseSaveState)house.CaptureState()).version, "re-saved as v2");
        }

        private sealed class FixedClock : Rapadura.Gameplay.Quests.IDailyClock
        {
            public FixedClock(System.DateTime now)
            {
                Now = now;
            }

            public System.DateTime Now { get; set; }
        }
    }
}
