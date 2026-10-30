using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Crafting.Advanced;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace Rapadura.Tests
{
    /// <summary>
    /// Advanced crafting (FASE 16, Docs/Balance/README.md "Crafting avançado"): tiers and material quality
    /// (<see cref="CraftTierRules"/>), creature-part drops, Vô Jorge's weapon enchantments (<see cref="ImbueRules"/>,
    /// <see cref="CraftingManager"/> save), the fusion forge (<see cref="FusionRules"/>) and the runes only Lavine and
    /// Maria draw, on a live bag through <see cref="AdvancedCraftingService"/>.
    /// </summary>
    public class AdvancedCraftingTests
    {
        // ------------------------------------------------------------------ tiers and quality

        [Test]
        public void Tier_FollowsTheDeepestRegion_AndNgPlusOpensAll()
        {
            Assert.AreEqual(1, CraftTierRules.TierFor(-1));
            Assert.AreEqual(1, CraftTierRules.TierFor(0));
            Assert.AreEqual(3, CraftTierRules.TierFor((int)BiomeType.Mountain));
            Assert.AreEqual(4, CraftTierRules.TierFor((int)BiomeType.Caves));
            Assert.AreEqual(4, CraftTierRules.TierFor(0, newGamePlusCycle: 1));
        }

        [Test]
        public void Caps_GrowWithTier_AndNeverReachEpic()
        {
            for (int tier = CraftTierRules.MinTier; tier < CraftTierRules.MaxTier; tier++)
            {
                Assert.LessOrEqual(CraftTierRules.RarityCap(tier), CraftTierRules.RarityCap(tier + 1));
            }

            Assert.Less(CraftTierRules.RarityCap(CraftTierRules.MaxTier), ItemRarity.Epic, "tools/balance/crafting_stack.py: Épico breaks the budget");
        }

        [Test]
        public void Quality_IsTheAverageRarity_PlusOneStepWithTrophy()
        {
            Assert.AreEqual(MaterialQuality.Rustic, CraftTierRules.QualityOf(new[] { (ItemRarity.Common, 5) }));
            Assert.AreEqual(MaterialQuality.Good, CraftTierRules.QualityOf(new[] { (ItemRarity.Common, 4), (ItemRarity.Uncommon, 2) }));
            Assert.AreEqual(MaterialQuality.Fine, CraftTierRules.QualityOf(new[] { (ItemRarity.Common, 4), (ItemRarity.Uncommon, 2) }, withTrophy: true));
            Assert.AreEqual(MaterialQuality.Superb, CraftTierRules.QualityOf(new[] { (ItemRarity.Rare, 3) }, withTrophy: true));
            Assert.AreEqual(MaterialQuality.Rustic, CraftTierRules.QualityOf(null));
        }

        [Test]
        public void RarityChances_SumToOne_RespectFloorAndCap()
        {
            foreach (MaterialQuality quality in Enum.GetValues(typeof(MaterialQuality)))
            {
                for (int tier = 1; tier <= 4; tier++)
                {
                    float[] chances = CraftTierRules.RarityChances(quality, tier, ItemRarity.Uncommon);
                    Assert.AreEqual(1f, chances.Sum(), 1e-4f);
                    Assert.AreEqual(0f, chances[0], "never below the floor");
                    for (int r = (int)CraftTierRules.RarityCap(tier) + 1; r < chances.Length; r++)
                    {
                        Assert.AreEqual(0f, chances[r], $"above the cap at tier {tier}");
                    }
                }
            }

            // Better materials never lower the expected rarity.
            float Expected(MaterialQuality q) => CraftTierRules.RarityChances(q, 4).Select((c, i) => c * i).Sum();
            Assert.LessOrEqual(Expected(MaterialQuality.Rustic), Expected(MaterialQuality.Good));
            Assert.LessOrEqual(Expected(MaterialQuality.Good), Expected(MaterialQuality.Fine));
            Assert.LessOrEqual(Expected(MaterialQuality.Fine), Expected(MaterialQuality.Superb));
        }

        [Test]
        public void RollRarity_IsSeeded()
        {
            ItemRarity[] a = Enumerable.Range(0, 20).Select(_ => ItemRarity.Common).ToArray();
            var r1 = new Random(7);
            var r2 = new Random(7);
            for (int i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(CraftTierRules.RollRarity(MaterialQuality.Fine, 4, r1), CraftTierRules.RollRarity(MaterialQuality.Fine, 4, r2));
            }
        }

        [Test]
        public void Recipes_AreUnlockedByTier_AndRunesOnlyByTheMagicCousins()
        {
            Assert.IsTrue(AdvancedRecipeCatalog.Unlocked(1).All(r => r.Tier == 1));
            Assert.AreEqual(AdvancedRecipeCatalog.All.Count, AdvancedRecipeCatalog.Unlocked(4).Count);
            foreach (AdvancedRecipeSpec rune in AdvancedRecipeCatalog.All.Where(r => r.Kind == AdvancedRecipeKind.Rune))
            {
                Assert.IsTrue(rune.CanBeCraftedBy(CharacterId.Lavine));
                Assert.IsTrue(rune.CanBeCraftedBy(CharacterId.Maria));
                Assert.IsFalse(rune.CanBeCraftedBy(CharacterId.Joaquim));
                Assert.IsFalse(rune.CanBeCraftedBy(null));
            }

            Assert.IsTrue(AdvancedRecipeCatalog.All.Where(r => r.Kind == AdvancedRecipeKind.Gear).All(r => r.CanBeCraftedBy(CharacterId.Icaro)));
            Assert.AreEqual(AdvancedRecipeCatalog.All.Count, AdvancedRecipeCatalog.All.Select(r => r.RecipeId).Distinct().Count());
        }

        [Test]
        public void EveryReferencedItem_ExistsInResources()
        {
            ItemDatabase.Invalidate();
            var ids = new HashSet<string>(CreatureParts.All.Concat(RuneItems.All));
            foreach (AdvancedRecipeSpec recipe in AdvancedRecipeCatalog.All)
            {
                ids.Add(recipe.ResultItemId);
                foreach (AdvancedIngredient ingredient in recipe.Ingredients)
                {
                    ids.Add(ingredient.ItemId);
                }
            }

            foreach (ImbueSpec imbue in ImbueCatalog.All)
            {
                ids.Add(imbue.PartItemId);
            }

            ids.Add(FusionRules.ConsolationItemId);
            foreach (string id in ids)
            {
                Assert.IsNotNull(ItemDatabase.GetById(id), id);
            }

            foreach (string rune in RuneItems.All)
            {
                Assert.IsTrue(ItemDatabase.GetById(rune).IsConsumable, rune);
            }
        }

        // ------------------------------------------------------------------ drops

        [Test]
        public void Drops_SpiderGivesSilk_BossesGiveThreePlusMaybeTrophy_NoRegionNothing()
        {
            Assert.AreEqual(CreatureParts.SpiderSilk, CreatureParts.PartOf("enemy_forest_spider", BiomeType.Forest));
            Assert.AreEqual(CreatureParts.WolfFang, CreatureParts.PartOf("enemy_forest_wolf", BiomeType.Forest));
            Assert.AreEqual(CreatureParts.BatWing, CreatureParts.PartOf("enemy_cave_dweller", BiomeType.Caves));
            Assert.IsNull(CreatureParts.PartOf("x", null));
            Assert.IsEmpty(CreaturePartDrops.Roll("enemy_forest_wolf", false, null, new Random(1)));

            var stub = new Dictionary<string, ItemDefinition>();
            ItemDefinition Resolve(string id)
            {
                if (!stub.TryGetValue(id, out ItemDefinition item))
                {
                    item = ScriptableObject.CreateInstance<ItemDefinition>();
                    stub[id] = item;
                }

                return item;
            }

            try
            {
                var random = new Random(3);
                int trophies = 0;
                for (int i = 0; i < 200; i++)
                {
                    var drops = CreaturePartDrops.Roll("enemy_desert_ruin_guardian", true, BiomeType.Desert, random, 0, Resolve);
                    Assert.AreEqual(stub[CreatureParts.ScorpionStinger], drops[0].Item);
                    Assert.AreEqual(CreaturePartDrops.BossParts, drops[0].Quantity);
                    trophies += drops.Count - 1;
                }

                Assert.That(trophies, Is.InRange(60, 140), "about half the bosses drop a trophy");
                Assert.AreEqual(CreaturePartDrops.CommonChance + CreaturePartDrops.NewGamePlusBonusPerCycle, CreaturePartDrops.ChanceOfPart(false, 1), 1e-6f);
                CollectionAssert.AreEqual(
                    CreaturePartDrops.Roll("enemy_forest_wolf", false, BiomeType.Forest, new Random(9), 0, Resolve).Select(d => d.Quantity),
                    CreaturePartDrops.Roll("enemy_forest_wolf", false, BiomeType.Forest, new Random(9), 0, Resolve).Select(d => d.Quantity));
            }
            finally
            {
                foreach (ItemDefinition item in stub.Values)
                {
                    Object.DestroyImmediate(item);
                }
            }
        }

        // ------------------------------------------------------------------ enchantments

        [Test]
        public void Imbues_NeverBeatTheBestRoadBlessing_AndGrowByTier()
        {
            foreach (ImbueSpec spec in ImbueCatalog.All)
            {
                Assert.That(spec.Tier, Is.InRange(1, 4));
                if (spec.Stat == Rapadura.Gameplay.Skills.StatType.AttackDamage)
                {
                    Assert.LessOrEqual(spec.Value, 0.08f, "the Cantoria's +8% is the slot's ceiling");
                }
            }
        }

        [Test]
        public void Imbue_PausesUnderRoadBlessing_CountsOnlyAppliedFights_AndExpires()
        {
            ActiveImbue active = ImbueRules.Apply(ImbueCatalog.Get(ImbueCatalog.WolfFang));
            Assert.AreEqual(ImbueRules.DurationEncounters, active.encountersRemaining);
            Assert.IsFalse(ImbueRules.AppliesThisFight(active, hasRoadBlessing: true, hasWeaponInHand: true));
            Assert.IsFalse(ImbueRules.AppliesThisFight(active, hasRoadBlessing: false, hasWeaponInHand: false));
            Assert.IsTrue(ImbueRules.AppliesThisFight(active, false, true));

            active = ImbueRules.CountEncounter(active, appliedThisFight: false, out bool expired);
            Assert.AreEqual(ImbueRules.DurationEncounters, active.encountersRemaining);
            Assert.IsFalse(expired);

            for (int i = 0; i < ImbueRules.DurationEncounters; i++)
            {
                active = ImbueRules.CountEncounter(active, true, out expired);
            }

            Assert.IsTrue(expired);
            Assert.IsFalse(active.IsActive);
            Assert.IsFalse(ImbueRules.Sanitize(new ActiveImbue("imbue_gone", 3)).IsActive);
            Assert.AreEqual(ImbueRules.DurationEncounters, ImbueRules.Sanitize(new ActiveImbue(ImbueCatalog.BatWing, 99)).encountersRemaining);
        }

        [Test]
        public void Imbue_Validate_LockedPartsGold()
        {
            ImbueSpec bat = ImbueCatalog.Get(ImbueCatalog.BatWing);
            var bag = new Dictionary<string, int> { { bat.PartItemId, 4 }, { "item_gold", 100 } };
            int Count(string id) => bag.TryGetValue(id, out int n) ? n : 0;
            Assert.AreEqual(ImbueCheck.Locked, ImbueRules.Validate(bat, 3, Count, "item_gold"));
            Assert.AreEqual(ImbueCheck.Ok, ImbueRules.Validate(bat, 4, Count, "item_gold"));
            bag[bat.PartItemId] = 3;
            Assert.AreEqual(ImbueCheck.MissingParts, ImbueRules.Validate(bat, 4, Count, "item_gold"));
            bag[bat.PartItemId] = 4;
            bag["item_gold"] = 1;
            Assert.AreEqual(ImbueCheck.MissingGold, ImbueRules.Validate(bat, 4, Count, "item_gold"));
            Assert.AreEqual(ImbueCheck.Unknown, ImbueRules.Validate(null, 4, Count, "item_gold"));
        }

        [Test]
        public void CraftingManager_SavesTheImbue_AndCountsDownOnEncounterEnd()
        {
            EventBus.Clear();
            var manager = new CraftingManager();
            manager.Initialize();
            try
            {
                manager.SetImbue(ImbueRules.Apply(ImbueCatalog.Get(ImbueCatalog.Stinger)));
                EventBus.Publish(new EncounterEndedEvent(null, default));
                Assert.AreEqual(ImbueRules.DurationEncounters, manager.Imbue.encountersRemaining, "not applied: paused");
                manager.MarkImbueApplied();
                EventBus.Publish(new EncounterEndedEvent(null, default));
                Assert.AreEqual(ImbueRules.DurationEncounters - 1, manager.Imbue.encountersRemaining);

                var state = (CraftingManagerSaveState)JsonUtility.FromJson(JsonUtility.ToJson(manager.CaptureState()), typeof(CraftingManagerSaveState));
                var loaded = new CraftingManager();
                loaded.RestoreState(state);
                Assert.AreEqual(ImbueCatalog.Stinger, loaded.Imbue.imbueId);
                Assert.AreEqual(ImbueRules.DurationEncounters - 1, loaded.Imbue.encountersRemaining);

                var old = (CraftingManagerSaveState)JsonUtility.FromJson("{\"unlockedRecipeIds\":[]}", typeof(CraftingManagerSaveState));
                loaded.RestoreState(old);
                Assert.IsFalse(loaded.Imbue.IsActive, "saves before the enchantments load with none");
                manager.ResetForNewSession();
                Assert.IsFalse(manager.Imbue.IsActive);
            }
            finally
            {
                manager.Shutdown();
                EventBus.Clear();
            }
        }

        // ------------------------------------------------------------------ fusion rules

        [Test]
        public void FusionChance_ThreeBeatsTwo_EpicIsHarder_TrophyHelps_Capped()
        {
            Assert.Greater(FusionRules.Chance(3, ItemRarity.Rare, false), FusionRules.Chance(2, ItemRarity.Rare, false));
            Assert.Less(FusionRules.Chance(2, ItemRarity.Epic, false), FusionRules.Chance(2, ItemRarity.Rare, false));
            Assert.Greater(FusionRules.Chance(2, ItemRarity.Rare, true), FusionRules.Chance(2, ItemRarity.Rare, false));
            Assert.LessOrEqual(FusionRules.Chance(3, ItemRarity.Uncommon, true), FusionRules.MaxChance);
        }

        // ------------------------------------------------------------------ service on a live bag

        private GameObject _owner;
        private InventoryManager _bag;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            ItemDatabase.Invalidate();
            _owner = new GameObject("AdvancedCraftBag");
            _bag = _owner.AddComponent<InventoryManager>();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            Object.DestroyImmediate(_owner);
        }

        private void Give(string id, int quantity) => _bag.AddItem(ItemDatabase.GetById(id), quantity);

        private int[] IndicesOf(string id) => _bag.Slots.Select((s, i) => (s, i)).Where(p => p.s.itemId == id).Select(p => p.i).ToArray();

        [Test]
        public void CraftGear_SpendsEverything_AndMakesOnePieceWithinTheCap()
        {
            AdvancedRecipeSpec recipe = AdvancedRecipeCatalog.Get("adv_leather_helmet");
            Assert.AreEqual(AdvancedCraftCheck.MissingIngredients, AdvancedCraftingService.TryCraft(recipe, _bag, 1, CharacterId.Joaquim, false, new Random(1), out _));
            Give("item_fiber", 4);
            Give(CreatureParts.WolfFang, 2);
            Give("item_gold", 10);
            Assert.AreEqual(MaterialQuality.Good, AdvancedCraftingService.QualityOf(recipe, false));

            Assert.AreEqual(AdvancedCraftCheck.Ok, AdvancedCraftingService.TryCraft(recipe, _bag, 1, CharacterId.Joaquim, false, new Random(1), out ItemRarity rarity));
            Assert.LessOrEqual(rarity, CraftTierRules.RarityCap(1));
            Assert.AreEqual(0, _bag.GetTotalCount("item_fiber"));
            Assert.AreEqual(0, _bag.GetTotalCount(CreatureParts.WolfFang));
            Assert.AreEqual(0, _bag.GetTotalCount("item_gold"));
            InventorySlotData piece = _bag.Slots.Single(s => s.itemId == "item_leather_helmet");
            Assert.AreEqual(rarity, FusionRules.EffectiveRarity(piece, ItemDatabase.GetById("item_leather_helmet")));
            Assert.IsFalse(piece.HasItemOptions, "forged pieces never carry MU options");
        }

        [Test]
        public void CraftGear_LockedBelowItsTier()
        {
            AdvancedRecipeSpec recipe = AdvancedRecipeCatalog.Get("adv_iron_sword");
            Assert.AreEqual(AdvancedCraftCheck.Locked, AdvancedCraftingService.TryCraft(recipe, _bag, 2, CharacterId.Joaquim, false, new Random(1), out _));
        }

        [Test]
        public void CraftRunes_OnlyLavineOrMaria_AndTheyAreUsable()
        {
            AdvancedRecipeSpec recipe = AdvancedRecipeCatalog.Get("adv_rune_healing");
            Give("item_herb", 4);
            Give(CreatureParts.SpiderSilk, 2);
            Assert.AreEqual(AdvancedCraftCheck.WrongCrafter, AdvancedCraftingService.TryCraft(recipe, _bag, 1, CharacterId.Icaro, false, new Random(1), out _));
            Assert.AreEqual(4, _bag.GetTotalCount("item_herb"), "nothing spent");
            Assert.AreEqual(AdvancedCraftCheck.Ok, AdvancedCraftingService.TryCraft(recipe, _bag, 1, CharacterId.Lavine, false, new Random(1), out _));
            Assert.AreEqual(AdvancedCraftCheck.Ok, AdvancedCraftingService.TryCraft(recipe, _bag, 1, CharacterId.Maria, false, new Random(1), out _));
            Assert.AreEqual(4, _bag.GetTotalCount(RuneItems.Healing));
            Assert.AreEqual(0, _bag.GetTotalCount("item_herb"));

            int index = IndicesOf(RuneItems.Healing)[0];
            Assert.IsTrue(_bag.UseItemInSlot(index), "a rune is a consumable anyone can read");
            Assert.AreEqual(3, _bag.GetTotalCount(RuneItems.Healing));
        }

        [Test]
        public void Fusion_Success_UpgradesTheBestPiece_KeepsRefine_MeltsTheRest()
        {
            Give("item_leather_boots", 1);
            Give("item_leather_boots", 1);
            Give("item_leather_boots", 1);
            Give("item_gold", 100);
            int[] boots = IndicesOf("item_leather_boots");
            _bag.Slots[boots[1]].refineLevel = 3;

            FusionOutcome outcome = default;
            int seed = 0;
            // The 3-piece chance is 75%: find a seed that succeeds (deterministic per seed).
            for (; seed < 50; seed++)
            {
                var probe = FusionRules.Roll(boots.Select(i => _bag.Slots[i]).ToList(), false, ItemDatabase.GetById, new Random(seed));
                if (probe.Success)
                {
                    break;
                }
            }

            Assert.AreEqual(FusionCheck.Ok, AdvancedCraftingService.TryFuse(_bag, boots, 4, false, new Random(seed), out outcome));
            Assert.IsTrue(outcome.Success);
            Assert.AreEqual(1, outcome.KeptIndex, "highest refine wins the tie");
            InventorySlotData kept = _bag.Slots.Single(s => s.itemId == "item_leather_boots");
            Assert.AreEqual(ItemRarity.Uncommon, kept.generatedRarity);
            Assert.AreEqual(3, kept.refineLevel);
            Assert.AreEqual(100 - FusionRules.GoldCost(ItemRarity.Uncommon), _bag.GetTotalCount("item_gold"));
        }

        [Test]
        public void Fusion_Failure_KeepsTheBestPiece_AndPaysConsolation()
        {
            Give("item_leather_boots", 1);
            Give("item_leather_boots", 1);
            Give("item_gold", 100);
            int[] boots = IndicesOf("item_leather_boots");
            int seed = 0;
            for (; seed < 50; seed++)
            {
                if (!FusionRules.Roll(boots.Select(i => _bag.Slots[i]).ToList(), false, ItemDatabase.GetById, new Random(seed)).Success)
                {
                    break;
                }
            }

            Assert.AreEqual(FusionCheck.Ok, AdvancedCraftingService.TryFuse(_bag, boots, 4, false, new Random(seed), out FusionOutcome outcome));
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(1, _bag.GetTotalCount("item_leather_boots"), "never lose everything");
            Assert.AreEqual(FusionRules.ConsolationPerPiece, _bag.GetTotalCount(FusionRules.ConsolationItemId));
        }

        [Test]
        public void Fusion_Refuses_WrongCount_DifferentItems_OptionsAndCap()
        {
            Give("item_leather_boots", 1);
            Give("item_leather_helmet", 1);
            Give("item_gold", 100);
            int boot = IndicesOf("item_leather_boots")[0];
            int helmet = IndicesOf("item_leather_helmet")[0];
            Assert.AreEqual(FusionCheck.WrongCount, AdvancedCraftingService.CheckFusion(_bag, new[] { boot }, 4, false));
            Assert.AreEqual(FusionCheck.NotSameItem, AdvancedCraftingService.CheckFusion(_bag, new[] { boot, helmet }, 4, false));
            Assert.AreEqual(FusionCheck.NotSameItem, AdvancedCraftingService.CheckFusion(_bag, new[] { boot, boot }, 4, false));

            Give("item_leather_boots", 1);
            int[] boots = IndicesOf("item_leather_boots");
            Assert.AreEqual(FusionCheck.MissingTrophy, AdvancedCraftingService.CheckFusion(_bag, boots, 4, true));
            _bag.Slots[boots[0]].luck = true;
            Assert.AreEqual(FusionCheck.HasOptions, AdvancedCraftingService.CheckFusion(_bag, boots, 4, false));
            _bag.Slots[boots[0]].luck = false;
            _bag.Slots[boots[0]].generatedRarity = CraftTierRules.RarityCap(1);
            Assert.AreEqual(FusionCheck.AtCap, AdvancedCraftingService.CheckFusion(_bag, boots, 1, false));
        }

        [Test]
        public void Imbue_Service_SpendsPartsAndGold_AndSetsTheManager()
        {
            EventBus.Clear();
            var manager = new CraftingManager();
            Give(CreatureParts.SpiderSilk, 4);
            Give("item_gold", 10);
            Assert.AreEqual(ImbueCheck.Ok, AdvancedCraftingService.TryImbue(manager, _bag, ImbueCatalog.SpiderSilk, 1));
            Assert.AreEqual(ImbueCatalog.SpiderSilk, manager.Imbue.imbueId);
            Assert.AreEqual(0, _bag.GetTotalCount(CreatureParts.SpiderSilk));
            Assert.AreEqual(0, _bag.GetTotalCount("item_gold"));
            Assert.AreEqual(ImbueCheck.MissingParts, AdvancedCraftingService.TryImbue(manager, _bag, ImbueCatalog.SpiderSilk, 1));
        }

        // ------------------------------------------------------------------ texts

        [Test]
        public void Texts_CoverEveryImbueQualityAndItem_InBothLanguages()
        {
            var keys = new HashSet<string>(CraftingTexts.Entries.Select(e => e.Key));
            foreach (ImbueSpec spec in ImbueCatalog.All)
            {
                Assert.IsTrue(keys.Contains(spec.NameKey), spec.NameKey);
                Assert.IsTrue(keys.Contains(spec.EffectKey), spec.EffectKey);
            }

            foreach (MaterialQuality quality in Enum.GetValues(typeof(MaterialQuality)))
            {
                Assert.IsTrue(keys.Contains(CraftingTexts.QualityKey(quality)));
            }

            ItemDatabase.Invalidate();
            foreach (string id in CreatureParts.All.Concat(RuneItems.All))
            {
                string key = Rapadura.Gameplay.Content.ContentText.ItemNameKey(id);
                Assert.AreEqual(ItemDatabase.GetById(id).DisplayName, CraftingTexts.GetFallback(key), id);
            }

            Assert.IsTrue(CraftingTexts.Entries.All(e => !string.IsNullOrEmpty(e.En) && !string.IsNullOrEmpty(e.PtBR)));
        }
    }
}
