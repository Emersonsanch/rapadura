using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Durability;
using Rapadura.Gameplay.Refining;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The services earlier waves left out because they needed a shop or a screen: the Forja de Tambor's cheaper repair (map 11,
    /// <see cref="RepairDiscountRules"/> + <see cref="RepairService"/>), Djedi's circles of light and Setne's shelf of the Caderno (map 71,
    /// <see cref="MesektetServiceRules"/>): the rules, the live repair on a bag, where the two service points land, and the texts.
    /// </summary>
    public class CityTrainingServicesTests
    {
        private static readonly CharacterId[] Heroes = (CharacterId[])System.Enum.GetValues(typeof(CharacterId));

        // ------------------------------------------------------------------ repair discount

        [Test]
        public void RepairDiscount_OnlyTheForjaDeTambor_TwentyPercent_RoundedUp_NeverFree()
        {
            Assert.AreEqual(CityShopCatalog.ForjaTambor, RepairDiscountRules.ForjaTamborShopId);
            Assert.AreEqual(CityCatalog.TamborRubro, RepairDiscountRules.TamborRubroCityId);
            Assert.AreEqual(0.2f, RepairDiscountRules.ForShop(CityShopCatalog.ForjaTambor), 1e-6f, "ficha 11: 20 % cheaper");
            Assert.AreEqual(0.2f, RepairDiscountRules.ForCity(CityCatalog.TamborRubro), 1e-6f);
            Assert.IsTrue(RepairDiscountRules.OffersRepair(CityShopCatalog.ForjaTambor));
            Assert.IsFalse(RepairDiscountRules.OffersRepair(CityShopCatalog.TrocasKhunanup));
            Assert.AreEqual(0f, RepairDiscountRules.ForCity(CityCatalog.CaisDeMesektet));
            Assert.AreEqual(0f, RepairDiscountRules.ForShop(null));

            Assert.AreEqual(80, RepairDiscountRules.Apply(100, 0.2f));
            Assert.AreEqual(11, RepairDiscountRules.Apply(13, 0.2f), "ceil(10.4)");
            Assert.AreEqual(1, RepairDiscountRules.Apply(1, 0.2f), "at least 1 gold");
            Assert.AreEqual(0, RepairDiscountRules.Apply(0, 0.2f), "nothing to repair stays free");
            Assert.AreEqual(13, RepairDiscountRules.Apply(13, 0f));
            Assert.AreEqual(10, RepairDiscountRules.Apply(100, 5f), "capped at 90 %");
            Assert.AreEqual(20, RepairDiscountRules.Percent(0.2f));
        }

        private GameObject _owner;
        private InventoryManager _bag;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            ItemDatabase.Invalidate();
            _owner = new GameObject("RepairDiscountBag");
            BuffController buffs = _owner.AddComponent<BuffController>();
            _bag = _owner.AddComponent<InventoryManager>();
            typeof(InventoryManager).GetField("_buffController", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_bag, buffs);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            Object.DestroyImmediate(_owner);
        }

        [Test]
        public void RepairService_WithTheForgeDiscount_EachPieceAndRepairAllAreCheaper()
        {
            foreach (string id in new[] { "item_iron_sword", "item_leather_chest" })
            {
                _bag.AddItem(ItemDatabase.GetById(id), 1);
                int index = _bag.Slots.Select((s, i) => (s, i)).First(p => p.s.itemId == id).i;
                Assert.IsTrue(_bag.EquipFromSlot(index), id);
            }

            // Nearly worn out, so the 20 % shows through the rounding.
            _bag.Equipped[EquipmentSlot.MainHand].currentDurability = 5f;
            _bag.Equipped[EquipmentSlot.Chest].currentDurability = 5f;
            RefineTarget sword = RefineTarget.Equipped(EquipmentSlot.MainHand);
            int full = RepairService.Quote(_bag, sword).Cost;
            int cheap = RepairService.Quote(_bag, sword, RepairDiscountRules.TamborRubroDiscount).Cost;
            Assert.AreEqual(RepairDiscountRules.Apply(full, 0.2f), cheap);
            Assert.Less(cheap, full);

            int total = RepairService.RepairAllCost(_bag, out int pieces);
            int discounted = RepairService.RepairAllCost(_bag, out int samePieces, 0.2f);
            Assert.AreEqual(2, pieces);
            Assert.AreEqual(pieces, samePieces);
            Assert.Less(discounted, total);

            _bag.AddItem(ItemDatabase.GetById(RefineItems.Gold), discounted);
            Assert.IsFalse(RepairService.TryRepairAll(_bag, out _, out _, out RepairCheck check), "full price: not enough");
            Assert.AreEqual(RepairCheck.NotEnoughGold, check);
            Assert.IsTrue(RepairService.TryRepairAll(_bag, out int paid, out int repaired, out check, 0.2f));
            Assert.AreEqual(discounted, paid);
            Assert.AreEqual(2, repaired);
            Assert.AreEqual(0, _bag.GetTotalCount(RefineItems.Gold));
            Assert.AreEqual(0, RepairService.ListWorn(_bag).Count);
        }

        // ------------------------------------------------------------------ Djedi

        [Test]
        public void Djedi_ThreeCircles_FirstLessonOfTheDayFree_ThenGold_NeverInAFight()
        {
            CollectionAssert.AreEqual(new[] { DjediLesson.Lamp, DjediLesson.Hours, DjediLesson.Scribe }, MesektetServiceRules.Lessons);
            Assert.AreEqual(StatType.ManaRegen, MesektetServiceRules.LessonStat(DjediLesson.Lamp));
            Assert.AreEqual(ModifierApplication.PercentAdditive, MesektetServiceRules.LessonApplication(DjediLesson.Lamp));
            Assert.AreEqual(StatType.CooldownReduction, MesektetServiceRules.LessonStat(DjediLesson.Hours));
            Assert.AreEqual(ModifierApplication.Flat, MesektetServiceRules.LessonApplication(DjediLesson.Hours));
            Assert.AreEqual(StatType.ResourceCostReduction, MesektetServiceRules.LessonStat(DjediLesson.Scribe));
            foreach (DjediLesson lesson in MesektetServiceRules.Lessons)
            {
                Assert.That(MesektetServiceRules.LessonValue(lesson), Is.InRange(0.01f, 0.25f), lesson + ": a nudge, not a class");
            }

            const long today = 20415L;
            Assert.AreEqual(0, MesektetServiceRules.LessonCost(0L, today), "never taken: free");
            Assert.AreEqual(0, MesektetServiceRules.LessonCost(today - 1L, today), "yesterday's free one: today's is free again");
            Assert.AreEqual(MesektetServiceRules.LessonGold, MesektetServiceRules.LessonCost(today, today));
            Assert.IsTrue(MesektetServiceRules.LessonIsFree(0L, today));

            Assert.AreEqual(MesektetServiceCheck.Ready, MesektetServiceRules.CheckLesson(false, 0, 0));
            Assert.AreEqual(MesektetServiceCheck.NotEnoughGold, MesektetServiceRules.CheckLesson(false, 120, 119));
            Assert.AreEqual(MesektetServiceCheck.Ready, MesektetServiceRules.CheckLesson(false, 120, 120));
            Assert.AreEqual(MesektetServiceCheck.InCombat, MesektetServiceRules.CheckLesson(true, 0, 999));

            Assert.AreEqual(900f, MesektetServiceRules.LessonSeconds, "15 min");
            Assert.AreEqual(15, MesektetServiceRules.MinutesLeft(900f));
            Assert.AreEqual(1, MesektetServiceRules.MinutesLeft(0.5f));
            Assert.AreEqual(0, MesektetServiceRules.MinutesLeft(0f));
        }

        // ------------------------------------------------------------------ Setne

        [Test]
        public void Setne_ShelvesPayOnceEach_InOrder_AndGrowWithThePages()
        {
            Assert.AreEqual(8, MesektetServiceRules.ShelfCount);
            for (int shelf = 1; shelf < MesektetServiceRules.ShelfCount; shelf++)
            {
                Assert.Greater(MesektetServiceRules.ShelfPages[shelf], MesektetServiceRules.ShelfPages[shelf - 1]);
                Assert.Greater(MesektetServiceRules.ShelfGold(shelf), MesektetServiceRules.ShelfGold(shelf - 1));
                Assert.GreaterOrEqual(MesektetServiceRules.ShelfBlessingStones(shelf), MesektetServiceRules.ShelfBlessingStones(shelf - 1));
            }

            Assert.AreEqual(75, MesektetServiceRules.ShelfGold(0), "25 pages × 3 gold");
            Assert.AreEqual(0, MesektetServiceRules.ShelfSoulStones(2), "no Pedra da Alma below 100 pages");
            Assert.AreEqual(1, MesektetServiceRules.ShelfSoulStones(3));
            Assert.AreEqual(1, MesektetServiceRules.ShelfLifeStones(MesektetServiceRules.ShelfCount - 1), "a Pedra da Vida on the last shelf");
            Assert.AreEqual(1, Enumerable.Range(0, MesektetServiceRules.ShelfCount).Sum(MesektetServiceRules.ShelfLifeStones), "only one, ever");
            Assert.AreEqual(0, MesektetServiceRules.ShelfGold(-1));
            Assert.AreEqual(0, MesektetServiceRules.ShelfGold(99));

            Assert.AreEqual(0, MesektetServiceRules.ShelvesReached(24));
            Assert.AreEqual(1, MesektetServiceRules.ShelvesReached(25));
            Assert.AreEqual(4, MesektetServiceRules.ShelvesReached(149));
            Assert.AreEqual(8, MesektetServiceRules.ShelvesReached(999));
            Assert.AreEqual(3, MesektetServiceRules.Claimable(120, 1));
            Assert.AreEqual(0, MesektetServiceRules.Claimable(120, 9));

            Assert.AreEqual(MesektetServiceCheck.Ready, MesektetServiceRules.CheckShelf(0, 30, 0));
            Assert.AreEqual(MesektetServiceCheck.WaitTurn, MesektetServiceRules.CheckShelf(1, 60, 0), "the lower shelf first");
            Assert.AreEqual(MesektetServiceCheck.NothingToClaim, MesektetServiceRules.CheckShelf(0, 30, 1), "already received");
            Assert.AreEqual(MesektetServiceCheck.NothingToClaim, MesektetServiceRules.CheckShelf(1, 49, 1), "not reached");
            Assert.AreEqual(MesektetServiceCheck.NothingToClaim, MesektetServiceRules.CheckShelf(8, 999, 8), "no ninth shelf");
            Assert.AreEqual(1, MesektetServiceRules.PagesToGo(1, 49));
            Assert.AreEqual(0, MesektetServiceRules.PagesToGo(1, 60));

            foreach (string id in new[] { RefineItems.BlessingStone, RefineItems.SoulStone, RefineItems.LifeStone, RefineItems.Gold })
            {
                Assert.IsNotNull(ItemDatabase.GetById(id), id);
            }
        }

        // ------------------------------------------------------------------ the world

        [Test]
        public void World_DjediAndSetneServicePoints_StandAtTheirDoors_InsideTheQuay()
        {
            CityLayout town = CityTownRecipes.LayoutFor(CityTownRecipes.MesektetCityId);
            OverworldLayout layout = OverworldLayouts.ForMap(OverworldLayouts.CaisMesektetMapId);
            Assert.IsNotNull(layout);
            OverworldZone anchor = layout.Zone(OverworldLayouts.RioAnchorZoneId);
            Assert.IsNotNull(anchor);
            OverworldSettlement city = layout.Settlements.FirstOrDefault(s => s.Recipe != null && s.Recipe.CityId == CityTownRecipes.MesektetCityId);
            Assert.IsNotNull(city);

            var points = new Dictionary<string, Vector2>();
            foreach (string resident in new[] { "djedi", "setne" })
            {
                CityLot lot = town.LotOfResident(resident);
                Assert.IsNotNull(lot, resident);
                Vector2 local = MesektetServicesMechanicsDriver.DoorOf(resident);
                Assert.Less(Vector2.Distance(local, lot.DoorApproach), 0.01f, resident + ": in front of the door");
                Assert.LessOrEqual(Vector2.Distance(local, lot.StandPoint), lot.InteractRadius + MesektetServiceRules.LessonReach, resident + ": beside the keeper");
                Vector2 world = anchor.Origin + OverworldLayouts.CaisMesektetOffset + local;
                Assert.Less(Vector2.Distance(city.Offset + local, world), 0.01f, resident + ": the driver's point is the town's point");
                Assert.AreSame(city, OverworldRules.SettlementAt(layout, world), resident + ": inside the quay's safe zone");
                points[resident] = world;
            }

            Assert.Greater(Vector2.Distance(points["djedi"], points["setne"]), MesektetServiceRules.LessonReach + MesektetServiceRules.ShelfReach,
                "the two points never overlap");
        }

        // ------------------------------------------------------------------ texts

        [Test]
        public void Texts_BothLanguages_EveryCousin_EveryCircle()
        {
            var keys = new HashSet<string>(CityTrainingTexts.Entries.Select(e => e.Key));
            Assert.AreEqual(CityTrainingTexts.Entries.Count, keys.Count, "no key twice");
            foreach (var entry in CityTrainingTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), entry.Key + " en");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), entry.Key + " pt");
                StringAssert.DoesNotContain("Dona ", entry.PtBR, entry.Key);
                StringAssert.DoesNotContain("Seu ", entry.PtBR, entry.Key);
            }

            foreach (CharacterId hero in Heroes)
            {
                Assert.IsTrue(keys.Contains(CityTrainingTexts.LessonLine(hero)), "lesson line " + hero);
                Assert.IsTrue(keys.Contains(CityTrainingTexts.ShelfLine(hero)), "shelf line " + hero);
            }

            foreach (DjediLesson lesson in MesektetServiceRules.Lessons)
            {
                Assert.IsTrue(keys.Contains(CityTrainingTexts.LessonName(lesson)), "name " + lesson);
                Assert.IsTrue(keys.Contains(CityTrainingTexts.LessonEffect(lesson)), "effect " + lesson);
            }

            Assert.AreEqual("Consertar tudo: 3 peças (40 de ouro, 20% mais barato aqui)",
                CityTrainingTexts.Format(null, CityTrainingTexts.ShopRepairRow, 3, 40, 20));
            Assert.AreEqual(" · 20% mais barato nesta forja", CityTrainingTexts.RepairDiscountSuffix(null, 0.2f));
            Assert.AreEqual(string.Empty, CityTrainingTexts.RepairDiscountSuffix(null, 0f));
        }
    }
}
