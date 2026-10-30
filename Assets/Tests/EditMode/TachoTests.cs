using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Refining;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.Tacho;
using Rapadura.UI.Achievements;
using Rapadura.UI.Common;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Tacho Encantado da Vó Zefa (FASE 16, Docs/Balance/README.md "Tacho Encantado e asas"): the pure <see cref="TachoRules"/>
    /// (recipes, chances, checks, seeded rolls, failure keeps the main item), the wings (<see cref="WingCatalog"/>: items in the
    /// new "Costas/Asas" slot, stats per level, equip bonuses, save), <see cref="TachoService"/> on a live bag, the Frutas do
    /// Sertão on the <see cref="AttributeSet"/>, the procedural wing mesh / flap and the window's texts.
    /// </summary>
    public class TachoTests
    {
        private GameObject _owner;
        private InventoryManager _bag;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            ItemDatabase.Invalidate();
            _owner = new GameObject("TachoBag");
            _bag = _owner.AddComponent<InventoryManager>();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            Object.DestroyImmediate(_owner);
        }

        private static int SeedBelow(double chance) => Enumerable.Range(0, 5000).First(s => TachoRules.Roll(s) < chance);

        private static int SeedAtOrAbove(double chance) => Enumerable.Range(0, 5000).First(s => TachoRules.Roll(s) >= chance);

        private void Give(string itemId, int quantity)
        {
            ItemDefinition item = ItemDatabase.GetById(itemId);
            Assert.IsNotNull(item, itemId);
            Assert.AreEqual(0, _bag.AddItem(item, quantity), $"{itemId} x{quantity} did not fit");
        }

        private void GiveCost(TachoRecipe recipe, int mainLevel)
        {
            foreach (TachoIngredient ingredient in TachoRules.CostFor(recipe, mainLevel))
            {
                Give(ingredient.ItemId, ingredient.Quantity);
            }

            int gold = TachoRules.GoldFor(recipe, mainLevel);
            if (gold > 0)
            {
                Give(TachoRules.Gold, gold);
            }
        }

        private int IndexOf(string itemId) => _bag.Slots.Select((s, i) => (s, i)).First(p => p.s != null && p.s.itemId == itemId).i;

        private int AddSword(int refineLevel)
        {
            Give("item_iron_sword", 1);
            int index = IndexOf("item_iron_sword");
            _bag.Slots[index].refineLevel = refineLevel;
            return index;
        }

        // ------------------------------------------------------------------ catalog and items

        [Test]
        public void Recipes_FiveWings_TenUpgrades_Protection_TwoFruits_AllItemsExist()
        {
            IReadOnlyList<TachoRecipe> all = TachoRules.All;
            Assert.AreEqual(5, all.Count(r => r.Kind == TachoRecipeKind.CreateWings));
            Assert.AreEqual(10, all.Count(r => r.Kind == TachoRecipeKind.UpgradeWings), "5 styles × (1→2, 2→3)");
            Assert.AreEqual(1, all.Count(r => r.Kind == TachoRecipeKind.ProtectedRefine));
            Assert.AreEqual(1, all.Count(r => r.Kind == TachoRecipeKind.GrowthFruit));
            Assert.AreEqual(1, all.Count(r => r.Kind == TachoRecipeKind.ResetFruit));
            Assert.AreEqual(all.Count, all.Select(r => r.Id).Distinct().Count(), "unique ids");

            foreach (TachoRecipe recipe in all)
            {
                foreach (TachoIngredient ingredient in recipe.Materials)
                {
                    Assert.IsNotNull(ItemDatabase.GetById(ingredient.ItemId), $"{recipe.Id}: {ingredient.ItemId} is not in Resources/Items");
                    Assert.Greater(ingredient.Quantity, 0);
                }

                if (recipe.ResultItemId != null)
                {
                    Assert.IsNotNull(ItemDatabase.GetById(recipe.ResultItemId), $"{recipe.Id}: {recipe.ResultItemId} missing");
                }

                if (recipe.Kind == TachoRecipeKind.CreateWings || recipe.Kind == TachoRecipeKind.UpgradeWings || recipe.Kind == TachoRecipeKind.ProtectedRefine
                    || recipe.Kind == TachoRecipeKind.GrowthFruit)
                {
                    Assert.IsTrue(recipe.Materials.Any(m => m.ItemId == RefineItems.ChaosStone), $"{recipe.Id} uses the Pedra do Caos");
                }
            }
        }

        [Test]
        public void Wings_AreBoundItems_InTheNewBackSlot_NotRefinable_TwoByTwo()
        {
            Assert.AreEqual(9, (int)EquipmentSlot.Wings, "appended: saves and assets store the slot as an int");
            Assert.AreEqual(15, WingCatalog.AllItemIds().Count());
            foreach (string id in WingCatalog.AllItemIds())
            {
                ItemDefinition item = ItemDatabase.GetById(id);
                Assert.IsNotNull(item, $"{id} missing from Resources/Items");
                Assert.AreEqual(EquipmentSlot.Wings, item.EquipSlot, id);
                Assert.IsTrue(item.IsBound, $"{id}: never drops on death");
                Assert.IsFalse(item.IsStackable, id);
                Assert.IsFalse(RefineRules.IsRefinable(item), $"{id}: no base stat, upgraded in the Tacho");
                Assert.AreEqual(new GridSize(2, 2), ItemFootprint.For(item), id);
                Assert.IsTrue(WingCatalog.TryParse(id, out WingStyle style, out int level));
                Assert.AreEqual(id, WingCatalog.ItemId(style, level));
            }

            Assert.IsNull(ItemDatabase.GetById("item_asas_carcara_4"), "level 4 (lendária) is documented only");
            Assert.IsFalse(WingCatalog.IsWing("item_iron_sword"));
            Assert.IsFalse(WingCatalog.IsWing("item_asas_carcara_4"));
            Assert.AreEqual(WingStyle.Carcara, WingCatalog.ForCousin(Rapadura.Gameplay.Characters.CharacterId.Maithe).Style);
            Assert.AreEqual(WingStyle.CapaVaqueiro, WingCatalog.ForCousin(Rapadura.Gameplay.Characters.CharacterId.Joaquim).Style);
            Assert.AreEqual(5, WingCatalog.Styles.Select(s => s.Cousin).Distinct().Count(), "one style per cousin");
        }

        [Test]
        public void WingStats_GrowWithTheLevel_AbsorbFromLevelTwo_SmallBudget()
        {
            foreach (WingStyleSpec spec in WingCatalog.Styles)
            {
                for (int level = 1; level <= WingCatalog.LegendaryLevel; level++)
                {
                    float damage = WingCatalog.DamagePercent(spec.Style, level);
                    Assert.LessOrEqual(damage, 0.04f, $"{spec.Style} {level}: wings are a small bonus (power budget)");
                    Assert.LessOrEqual(WingCatalog.DefensePercent(spec.Style, level) + WingCatalog.Absorb(level), 0.07f);
                    Assert.LessOrEqual(WingCatalog.MoveSpeedPercent(spec.Style, level), 0.10f);
                    if (level > 1)
                    {
                        Assert.Greater(damage, WingCatalog.DamagePercent(spec.Style, level - 1));
                    }
                }
            }

            Assert.AreEqual(0f, WingCatalog.Absorb(1));
            Assert.Greater(WingCatalog.Absorb(2), 0f);
            Assert.Greater(WingCatalog.Absorb(3), WingCatalog.Absorb(2));
            Assert.AreEqual(3, WingCatalog.ModifiersFor("item_asas_carcara_1").Count);
            List<WingModifier> two = WingCatalog.ModifiersFor("item_asas_carcara_2");
            Assert.AreEqual(4, two.Count);
            Assert.IsTrue(two.Single(m => m.IsAbsorb).Stat == StatType.Defense);
            Assert.AreEqual(0, WingCatalog.ModifiersFor("item_iron_sword").Count);
        }

        // ------------------------------------------------------------------ rules

        [Test]
        public void Chances_WingsFrom50At4_Upgrades50And35_ProtectionIsTheRefineChance()
        {
            Assert.AreEqual(0f, TachoRules.CreateWingsChance(3));
            Assert.AreEqual(0.50f, TachoRules.CreateWingsChance(4), 1e-6f);
            Assert.AreEqual(0.55f, TachoRules.CreateWingsChance(5), 1e-6f);
            Assert.AreEqual(0.75f, TachoRules.CreateWingsChance(9), 1e-6f);
            Assert.AreEqual(0.75f, TachoRules.CreateWingsChance(15), 1e-6f, "capped");

            Assert.AreEqual(0.50f, TachoRules.ChanceFor(TachoRules.Get(TachoRules.UpgradeWingsId(WingStyle.BeijaFlor, 1)), 1), 1e-6f);
            Assert.AreEqual(0.35f, TachoRules.ChanceFor(TachoRules.Get(TachoRules.UpgradeWingsId(WingStyle.BeijaFlor, 2)), 2), 1e-6f);
            TachoRecipe protection = TachoRules.Get(TachoRules.ProtectedRefineId);
            for (int level = 7; level < 15; level++)
            {
                Assert.AreEqual(RefineRules.BaseChance(level), TachoRules.ChanceFor(protection, level), 1e-6f);
                Assert.AreEqual(RefineRules.GoldCostFor(level), TachoRules.GoldFor(protection, level));
                Assert.IsTrue(TachoRules.ProtectionMatters(level), $"+{level}");
            }

            Assert.IsFalse(TachoRules.ProtectionMatters(6), "+6 → +7 already keeps +6 on failure");
            Assert.IsFalse(TachoRules.ProtectionMatters(15));
            Assert.AreEqual(0.60f, TachoRules.ChanceFor(TachoRules.Get(TachoRules.GrowthFruitId), 0), 1e-6f);
            Assert.AreEqual(0.80f, TachoRules.ChanceFor(TachoRules.Get(TachoRules.ResetFruitId), 0), 1e-6f);
        }

        [Test]
        public void Checks_NeedTheMainItem_PlusFour_TheMaterials_AndTheGold()
        {
            TachoRecipe wings = TachoRules.Get(TachoRules.CreateWingsId(WingStyle.Carcara));
            var owned = new Dictionary<string, int>();
            int Owned(string id) => owned.TryGetValue(id, out int n) ? n : 0;

            Assert.AreEqual(TachoCheck.NoMainItem, TachoRules.Check(wings, false, 0, null, Owned));
            Assert.AreEqual(TachoCheck.MainItemTooLow, TachoRules.Check(wings, true, 3, "item_iron_sword", Owned));
            Assert.AreEqual(TachoCheck.MissingMaterial, TachoRules.Check(wings, true, 4, "item_iron_sword", Owned));
            foreach (TachoIngredient ingredient in wings.Materials)
            {
                owned[ingredient.ItemId] = ingredient.Quantity;
            }

            Assert.AreEqual(TachoCheck.NotEnoughGold, TachoRules.Check(wings, true, 4, "item_iron_sword", Owned));
            owned[TachoRules.Gold] = TachoRules.CreateWingsGold;
            Assert.AreEqual(TachoCheck.Ok, TachoRules.Check(wings, true, 4, "item_iron_sword", Owned));

            TachoRecipe up = TachoRules.Get(TachoRules.UpgradeWingsId(WingStyle.Carcara, 1));
            Assert.AreEqual(TachoCheck.NoMainItem, TachoRules.Check(up, true, 1, "item_asas_vaga_lume_1", Owned), "other style");
            Assert.AreEqual(TachoCheck.MainItemTooLow, TachoRules.Check(up, true, 2, "item_asas_carcara_2", Owned), "wrong level");
            Assert.AreEqual(TachoCheck.MaxLevel, TachoRules.Check(up, true, 3, "item_asas_carcara_3", Owned));
            Assert.IsNull(TachoRules.UpgradeFor("item_asas_carcara_3"), "level 4 is not craftable");

            TachoRecipe protection = TachoRules.Get(TachoRules.ProtectedRefineId);
            Assert.AreEqual(TachoCheck.NotNeeded, TachoRules.Check(protection, true, 5, "item_iron_sword", Owned));
            Assert.AreEqual(TachoCheck.MaxLevel, TachoRules.Check(protection, true, 15, "item_iron_sword", Owned));
            CollectionAssert.Contains(TachoRules.CostFor(protection, 8).Select(i => i.ItemId).ToList(), RefineItems.SoulStone, "the refine's own stone too");
        }

        [Test]
        public void Rolls_AreSeeded_AndAFailureNeverLowersAnything()
        {
            foreach (TachoRecipe recipe in TachoRules.All)
            {
                int level = recipe.Kind == TachoRecipeKind.UpgradeWings ? recipe.FromLevel : recipe.Kind == TachoRecipeKind.ProtectedRefine ? 10 : 6;
                for (int seed = 0; seed < 50; seed++)
                {
                    TachoOutcome a = TachoRules.Attempt(recipe, level, seed);
                    TachoOutcome b = TachoRules.Attempt(recipe, level, seed);
                    Assert.AreEqual(a.Success, b.Success, "same seed, same answer (the server's)");
                    Assert.AreEqual(a.Roll, b.Roll);
                    Assert.GreaterOrEqual(a.ToLevel, a.FromLevel, $"{recipe.Id}: never loses a level");
                    if (!a.Success)
                    {
                        Assert.IsTrue(a.MainItemKept);
                        Assert.AreEqual(a.FromLevel, a.ToLevel);
                        Assert.IsNull(a.ResultItemId);
                    }
                }
            }

            TachoRecipe protection = TachoRules.Get(TachoRules.ProtectedRefineId);
            TachoOutcome fail = TachoRules.Resolve(protection, 12, 0.99);
            Assert.IsFalse(fail.Success);
            Assert.AreEqual(12, fail.ToLevel, "Proteção do Caos: +12 stays +12 (the forge would drop to +11)");
            Assert.AreEqual(13, TachoRules.Resolve(protection, 12, 0.01).ToLevel);
            Assert.AreEqual("item_asas_carcara_1", TachoRules.Resolve(TachoRules.Get(TachoRules.CreateWingsId(WingStyle.Carcara)), 4, 0.1).ResultItemId);
        }

        // ------------------------------------------------------------------ service on a live bag

        [Test]
        public void Service_MakesWings_ConsumesThePiece_AndSpendsTheCost()
        {
            int index = AddSword(5);
            TachoRecipe recipe = TachoRules.Get(TachoRules.CreateWingsId(WingStyle.AsaBranca));
            GiveCost(recipe, 5);
            TachoPreview preview = TachoService.Preview(_bag, recipe, RefineTarget.Bag(index));
            Assert.AreEqual(TachoCheck.Ok, preview.Check);
            Assert.AreEqual(0.55f, preview.Chance, 1e-6f);

            Assert.IsTrue(TachoService.TryCombine(_bag, recipe, RefineTarget.Bag(index), SeedBelow(0.55), out TachoOutcome outcome, out _, out bool toChest));
            Assert.IsTrue(outcome.Success);
            Assert.IsFalse(toChest);
            Assert.AreEqual(0, _bag.GetTotalCount("item_iron_sword"), "the refined piece went into the cauldron");
            Assert.AreEqual(1, _bag.GetTotalCount("item_asas_asa_branca_1"));
            Assert.AreEqual(0, _bag.GetTotalCount(RefineItems.ChaosStone));
            Assert.AreEqual(0, _bag.GetTotalCount(TachoRules.Gold));
        }

        [Test]
        public void Service_FailedWings_SpendTheMaterials_ButKeepThePieceExactlyAsItWas()
        {
            int index = AddSword(4);
            TachoRecipe recipe = TachoRules.Get(TachoRules.CreateWingsId(WingStyle.Carcara));
            GiveCost(recipe, 4);
            Assert.IsTrue(TachoService.TryCombine(_bag, recipe, RefineTarget.Bag(index), SeedAtOrAbove(0.50), out TachoOutcome outcome, out _, out _));
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual("item_iron_sword", _bag.Slots[index].itemId, "kid-friendly: the main item stays");
            Assert.AreEqual(4, _bag.Slots[index].refineLevel, "and keeps its +4");
            Assert.AreEqual(0, _bag.GetTotalCount(RefineItems.ChaosStone), "the materials are spent");
            Assert.AreEqual(0, _bag.GetTotalCount("item_fiber"));
            Assert.AreEqual(0, _bag.GetTotalCount(TachoRules.Gold));

            Assert.IsFalse(TachoService.TryCombine(_bag, recipe, RefineTarget.Bag(index), 1, out _, out TachoCheck check, out _), "nothing left to spend");
            Assert.AreEqual(TachoCheck.MissingMaterial, check);
        }

        [Test]
        public void Service_UpgradesAnEquippedWing_AndReEquipsIt_WithItsBonuses()
        {
            BuffController buffs = _owner.AddComponent<BuffController>();
            typeof(InventoryManager).GetField("_buffController", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_bag, buffs);
            Give("item_asas_carcara_1", 1);
            Assert.IsTrue(_bag.EquipFromSlot(IndexOf("item_asas_carcara_1")));
            Assert.AreEqual("item_asas_carcara_1", _bag.Equipped[EquipmentSlot.Wings].itemId);
            Assert.AreEqual(0.015f, buffs.GetPercentTotal(StatType.AttackDamage), 1e-5f, "Carcará 1: +1.5% damage");
            Assert.AreEqual(0.01f, buffs.GetPercentTotal(StatType.Defense), 1e-5f);

            TachoRecipe recipe = TachoRules.Get(TachoRules.UpgradeWingsId(WingStyle.Carcara, 1));
            Assert.IsTrue(TachoService.ListUpgradableWings(_bag).Any(t => t.IsEquipped));
            GiveCost(recipe, 1);
            RefineTarget equipped = RefineTarget.Equipped(EquipmentSlot.Wings);
            Assert.IsTrue(TachoService.TryCombine(_bag, recipe, equipped, SeedBelow(0.50), out TachoOutcome outcome, out _, out _));
            Assert.IsTrue(outcome.Success);
            Assert.AreEqual(2, outcome.ToLevel);
            Assert.AreEqual("item_asas_carcara_2", _bag.Equipped[EquipmentSlot.Wings].itemId, "back on the hero at level 2");
            Assert.AreEqual(0, _bag.GetTotalCount("item_asas_carcara_1"));
            Assert.AreEqual(0.01875f, buffs.GetPercentTotal(StatType.AttackDamage), 1e-5f, "×1.25 at level 2");
            Assert.AreEqual(0.0125f + WingCatalog.Absorb(2), buffs.GetPercentTotal(StatType.Defense), 1e-5f, "defense + absorb");

            Assert.IsTrue(_bag.UnequipSlot(EquipmentSlot.Wings));
            Assert.AreEqual(0f, buffs.GetPercentTotal(StatType.AttackDamage), 1e-6f, "bonuses leave with the wing");
        }

        [Test]
        public void Service_ProtectedRefine_FailureKeepsTheLevel_SuccessRaisesIt()
        {
            int index = AddSword(10);
            TachoRecipe recipe = TachoRules.Get(TachoRules.ProtectedRefineId);
            GiveCost(recipe, 10);
            Assert.AreEqual(TachoCheck.Ok, TachoService.Preview(_bag, recipe, RefineTarget.Bag(index)).Check);
            Assert.IsTrue(TachoService.TryCombine(_bag, recipe, RefineTarget.Bag(index), SeedAtOrAbove(0.55), out TachoOutcome fail, out _, out _));
            Assert.IsFalse(fail.Success);
            Assert.AreEqual(10, _bag.Slots[index].refineLevel, "the Pedra do Caos held it at +10");
            Assert.AreEqual(0, _bag.GetTotalCount(RefineItems.SoulStone));
            Assert.AreEqual(0, _bag.GetTotalCount(RefineItems.ChaosStone));

            GiveCost(recipe, 10);
            Assert.IsTrue(TachoService.TryCombine(_bag, recipe, RefineTarget.Bag(index), SeedBelow(0.55), out TachoOutcome ok, out _, out _));
            Assert.IsTrue(ok.Success);
            Assert.AreEqual(11, _bag.Slots[index].refineLevel);
        }

        [Test]
        public void Service_FullBag_SendsTheWingToGrandmasChest_NeverLost()
        {
            var chest = new VillageChestStorage();
            int index = AddSword(6);
            TachoRecipe recipe = TachoRules.Get(TachoRules.CreateWingsId(WingStyle.VagaLume));
            GiveCost(recipe, 6);
            ItemDefinition stone = ItemDatabase.GetById("item_iron_ore");
            while (_bag.AddItem(stone, stone.MaxStack) == 0)
            {
            }

            Assert.IsTrue(TachoService.TryCombine(_bag, recipe, RefineTarget.Bag(index), SeedBelow(0.60), out TachoOutcome outcome, out _, out bool toChest, chest));
            Assert.IsTrue(outcome.Success);
            if (toChest)
            {
                Assert.IsTrue(chest.Inbox.Any(i => i.itemId == "item_asas_vaga_lume_1"), "in the chest's inbox");
            }
            else
            {
                Assert.AreEqual(1, _bag.GetTotalCount("item_asas_vaga_lume_1"), "the sword's 1×3 hole was enough? then it is in the bag");
            }
        }

        [Test]
        public void Achievements_FirstWings_AndLevelThree_AreShippedAndFed()
        {
            AchievementDefinition first = Resources.Load<AchievementDefinition>("Achievements/ach_wings_first");
            AchievementDefinition level3 = Resources.Load<AchievementDefinition>("Achievements/ach_wings_level3");
            Assert.IsNotNull(first);
            Assert.IsNotNull(level3);
            Assert.AreEqual(AchievementConditionType.WingsCrafted, first.ConditionType);
            Assert.AreEqual(1, first.TargetValue);
            Assert.AreEqual(AchievementConditionType.WingLevelReached, level3.ConditionType);
            Assert.AreEqual(3, level3.TargetValue);
            Assert.IsTrue(AchievementTexts.All.Any(e => e.Key == "achievement.ach_wings_first.title"));
            Assert.IsTrue(AchievementTexts.All.Any(e => e.Key == "achievement.ach_wings_level3.description"));
        }

        // ------------------------------------------------------------------ fruits

        [Test]
        public void Fruits_Growth_AddsAPoint_UpToTheCap_ThenStaysInTheBag()
        {
            BuffController buffs = _owner.AddComponent<BuffController>();
            AttributeSet attributes = _owner.AddComponent<AttributeSet>();
            attributes.ForceInitializeForTests();
            Give(FruitRules.GrowthFruit, FruitRules.MaxGrowthPoints + 1);

            for (int i = 0; i < FruitRules.MaxGrowthPoints; i++)
            {
                Assert.IsTrue(_bag.UseItemInSlot(IndexOf(FruitRules.GrowthFruit)));
            }

            Assert.AreEqual(FruitRules.MaxGrowthPoints, attributes.AvailableAttributePoints);
            Assert.AreEqual(FruitRules.MaxGrowthPoints, attributes.GrowthFruitPoints);
            Assert.IsFalse(_bag.UseItemInSlot(IndexOf(FruitRules.GrowthFruit)), "cap reached: not eaten");
            Assert.AreEqual(1, _bag.GetTotalCount(FruitRules.GrowthFruit));

            var state = (AttributeSetSaveState)attributes.CaptureState();
            Assert.AreEqual(FruitRules.MaxGrowthPoints, JsonUtility.FromJson<AttributeSetSaveState>(JsonUtility.ToJson(state)).growthFruitPoints);
            Assert.AreEqual(0, JsonUtility.FromJson<AttributeSetSaveState>("{\"vitality\":3}").growthFruitPoints, "old saves: none eaten");
            Assert.IsNotNull(buffs);
        }

        [Test]
        public void Fruits_Reset_TakesFromTheHighestAttribute_AndNeedsSomethingToTake()
        {
            _owner.AddComponent<BuffController>();
            AttributeSet attributes = _owner.AddComponent<AttributeSet>();
            attributes.ForceInitializeForTests();
            Give(FruitRules.ResetFruit, 2);
            Assert.IsFalse(_bag.UseItemInSlot(IndexOf(FruitRules.ResetFruit)), "nothing allocated");
            Assert.AreEqual(2, _bag.GetTotalCount(FruitRules.ResetFruit));

            attributes.GrantAttributePoints(3);
            attributes.TrySpendPoint(AttributeType.Spirit, 1);
            attributes.TrySpendPoint(AttributeType.ArcanePower, 2);
            Assert.IsTrue(_bag.UseItemInSlot(IndexOf(FruitRules.ResetFruit)));
            Assert.AreEqual(1, attributes.GetAllocatedPoints(AttributeType.ArcanePower));
            Assert.AreEqual(1, attributes.AvailableAttributePoints);
            Assert.AreEqual(1, _bag.GetTotalCount(FruitRules.ResetFruit));

            Assert.AreEqual(1, FruitRules.ResetTarget(new[] { 2, 3, 3 }), "first on ties");
            Assert.AreEqual(-1, FruitRules.ResetTarget(new[] { 0, 0 }));
        }

        [Test]
        public void Fruits_OnTheMap_WithoutAnAttributeSheet_AreNotEaten()
        {
            Give(FruitRules.GrowthFruit, 1);
            Assert.IsFalse(_bag.UseItemInSlot(IndexOf(FruitRules.GrowthFruit)));
            Assert.AreEqual(1, _bag.GetTotalCount(FruitRules.GrowthFruit));
        }

        // ------------------------------------------------------------------ visual

        [Test]
        public void WingMeshes_AreLowPoly_ForEveryStyleAndLevel_AndGrowWithTheLevel()
        {
            foreach (WingStyleSpec spec in WingCatalog.Styles)
            {
                float lastWidth = 0f;
                for (int level = 1; level <= WingCatalog.MaxCraftableLevel; level++)
                {
                    foreach (bool mirror in new[] { false, true })
                    {
                        Mesh mesh = WingMeshBuilder.Build(spec, level, mirror);
                        try
                        {
                            int triangles = mesh.triangles.Length / 3;
                            Assert.Greater(triangles, 0);
                            Assert.LessOrEqual(triangles, WingMeshBuilder.MaxTrianglesPerSide, $"{spec.Style} {level}");
                            Assert.AreEqual(mesh.vertexCount, mesh.uv.Length, "palette UVs");
                            if (!mirror)
                            {
                                Assert.GreaterOrEqual(mesh.bounds.size.x, lastWidth - 1e-4f);
                                lastWidth = mesh.bounds.size.x;
                            }
                            else
                            {
                                Assert.Less(mesh.bounds.center.x, 0f, "the left side is the mirror");
                            }
                        }
                        finally
                        {
                            Object.DestroyImmediate(mesh);
                        }
                    }
                }
            }
        }

        [Test]
        public void Flap_IdleBeatAndRunFold_ReduceMotionTurnsTheBeatOffAndHalves()
        {
            float quarter = 0.25f / WingBackVisual.IdleFrequencyHz;
            Assert.AreEqual(WingBackVisual.IdleAmplitudeDegrees, WingBackVisual.FlapTarget(quarter, 0f, false, 1f), 1e-3f);
            Assert.AreEqual(0f, WingBackVisual.FlapTarget(quarter, 0f, true, 0.5f), 1e-6f, "Reduzir Movimento: no idle beat");
            Assert.AreEqual(-WingBackVisual.RunFoldDegrees, WingBackVisual.FlapTarget(0f, 20f, false, 1f), 1e-3f, "folded at a run");
            Assert.AreEqual(-WingBackVisual.RunFoldDegrees * 0.5f, WingBackVisual.FlapTarget(0f, 20f, true, 0.5f), 1e-3f, "halved");
        }

        // ------------------------------------------------------------------ texts and tooltip

        [Test]
        public void Texts_KeysResolve_ItemNamesMatchTheAssets_AndTheTooltipShowsTheWing()
        {
            foreach (FieldInfo field in typeof(TachoTexts).GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.IsLiteral))
            {
                string key = (string)field.GetRawConstantValue();
                Assert.IsNotNull(TachoTexts.GetFallback(key), $"TachoTexts.{field.Name} ({key}) has no entry");
            }

            foreach (Rapadura.Gameplay.Characters.CharacterId hero in System.Enum.GetValues(typeof(Rapadura.Gameplay.Characters.CharacterId)))
            {
                Assert.IsNotNull(TachoTexts.GetFallback(TachoTexts.GreetingKey(hero)), hero.ToString());
                Assert.IsNotNull(TachoTexts.GetFallback(TachoTexts.CousinKey(hero)), hero.ToString());
            }

            foreach (string id in WingCatalog.AllItemIds().Concat(new[] { FruitRules.GrowthFruit, FruitRules.ResetFruit }))
            {
                Assert.AreEqual(ItemDatabase.GetById(id).DisplayName, TachoTexts.GetFallback(Rapadura.Gameplay.Content.ContentText.ItemNameKey(id)), $"{id} name drifted");
            }

            Assert.AreEqual("Costas/Asas", TooltipLocalization.Entries.First(e => e.Key == "ui.tooltip.slot.wings").PtBR);
            TooltipContent tooltip = TooltipContentBuilder.ForItem(ItemDatabase.GetById("item_asas_carcara_3"), null);
            Assert.IsTrue(tooltip.StatLines.Any(l => l.Contains("Absorve 2% do dano")), string.Join(" | ", tooltip.StatLines));
            Assert.Greater(tooltip.StatLines.Count, 3);
        }

        [Test]
        public void Window_Lines_ShowStatsCostAndTheKidFriendlyFailure()
        {
            Assert.AreEqual("75", VillageTachoPanel.Percent(0.75f));
            Assert.AreEqual("+1.5% de dano · +1% de defesa · +3% de velocidade", VillageTachoPanel.WingStatsLine("item_asas_carcara_1", null));
            StringAssert.Contains("absorve 1% do dano", VillageTachoPanel.WingStatsLine("item_asas_carcara_2", null));

            int index = AddSword(4);
            TachoRecipe recipe = TachoRules.Get(TachoRules.CreateWingsId(WingStyle.Carcara));
            TachoPreview preview = TachoService.Preview(_bag, recipe, RefineTarget.Bag(index));
            StringAssert.StartsWith("Custo: 1 Pedra do Caos", VillageTachoPanel.CostLine(preview, null));
            StringAssert.Contains("60 de ouro", VillageTachoPanel.CostLine(preview, null));
            Assert.AreEqual("Se falhar: os materiais se gastam, Espada de Ferro +4 fica com você.",
                VillageTachoPanel.FailureLine(preview, "Espada de Ferro +4", null));
            StringAssert.StartsWith("Falta na mochila:", VillageTachoPanel.CheckLine(preview, null, _bag.GetTotalCount));

            var failed = new TachoOutcome(recipe.Id, recipe.Kind, 0.5f, 0.9, false, 0, 0, null);
            StringAssert.Contains("continua com você", VillageTachoPanel.ResultLine(failed, "Espada de Ferro +4", null, false, null));
            var made = new TachoOutcome(recipe.Id, recipe.Kind, 0.5f, 0.1, true, 0, 1, "item_asas_carcara_1");
            Assert.AreEqual("Deu certo! Asas de Carcará saiu do tacho! (A mochila estava cheia: foi para o Baú da Vó.)",
                VillageTachoPanel.ResultLine(made, "Espada", "Asas de Carcará", true, null));
        }
    }
}
