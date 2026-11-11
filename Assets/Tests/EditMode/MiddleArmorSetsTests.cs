using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Sets;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The five armour sets of Regions 5–9 (Docs/Items.md), on the road of the middle story: each one drops from its region's
    /// own beasts, which really walk that region at its levels; the pieces sit between the Marfim (Ossuário, level 35) and the
    /// sets of Regions 13–16, and the two-piece bonuses never give more than the far ones do. Pure rules only.
    /// </summary>
    public class MiddleArmorSetsTests
    {
        private static readonly (ArmorSetDefinition Set, string RegionId, int Low, int High, string NamePt, string NameEn)[] Sets =
        {
            (ArmorSetRules.Coral, OverworldLayouts.CostaId, 45, 72, "Coral de Kilwa", "Kilwa Coral"),
            (ArmorSetRules.Lotus, OverworldLayouts.DeltaId, 60, 88, "Seda de Lótus", "Lotus Silk"),
            (ArmorSetRules.Yak, OverworldLayouts.TetoId, 70, 98, "Lã de Iaque", "Yak Wool"),
            (ArmorSetRules.Bronze, OverworldLayouts.MingchengId, 90, 140, "Vasos de Bronze", "Bronze Vessels"),
            (ArmorSetRules.Kurgan, OverworldLayouts.NartesId, 100, 150, "Ouro dos Kurgans", "Kurgan Gold"),
        };

        private static ItemDefinition Load(string id) => Resources.Load<ItemDefinition>("Items/Sets/" + id);

        private static ItemDefinition LoadBone(string id) => Resources.Load<ItemDefinition>("Items/Dungeon/" + id);

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            MapLayouts.ClearCache();
            OverworldLayouts.ClearCache();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() => MapLayouts.ClearCache();

        [Test]
        public void TheMiddleSets_AreTheFiveOfRegions5To9_InRegionOrder_AndPartOfAll()
        {
            CollectionAssert.AreEqual(Sets.Select(s => s.Set).ToList(), ArmorSetRules.Middle.ToList());
            CollectionAssert.IsSubsetOf(ArmorSetRules.Middle.ToList(), ArmorSetRules.All.ToList());
            CollectionAssert.IsSubsetOf(ArmorSetRules.Far.ToList(), ArmorSetRules.All.ToList());
            Assert.AreEqual(ArmorSetRules.All.Count, ArmorSetRules.All.Select(s => s.Id).Distinct().Count());
        }

        [Test]
        public void EveryBeast_WalksItsRegion_AtTheRegionsLevels()
        {
            foreach ((ArmorSetDefinition set, string regionId, int low, int high, _, _) in Sets)
            {
                OverworldLayout world = OverworldLayouts.Get(regionId);
                Assert.IsNotNull(world, regionId);
                foreach (string beast in set.Beasts)
                {
                    List<MapSpawnZone> spawns = world.World.Zones.Where(z => z.AllEntries().Any(e => e != null && e.EnemyId == beast)).ToList();
                    Assert.IsNotEmpty(spawns, $"{set.Id}: {beast} spawns in {regionId}");
                    Assert.IsTrue(spawns.Any(z => z.MaxLevel >= low && z.MinLevel <= high), $"{set.Id}: {beast} stands within {low}–{high}");
                }
            }
        }

        [Test]
        public void TheSource_IsReachable_AtTheRegionsLevels_AndCheaperThanTheFarSets()
        {
            foreach ((ArmorSetDefinition set, string regionId, int low, int high, _, _) in Sets)
            {
                OverworldLayout world = OverworldLayouts.Get(regionId);
                int firstLevel = world.World.Zones.Where(z => z.AllEntries().Any(e => e != null && set.Beasts.Contains(e.EnemyId))).Min(z => z.MinLevel);
                Assert.That(firstLevel, Is.InRange(low - 5, high), $"{set.Id}: the first beast that drops it stands at the region's levels");
                Assert.AreEqual(ArmorSetRules.MiddleDropChance, set.DropChance, set.Id);
            }

            foreach (ArmorSetDefinition far in ArmorSetRules.Far)
            {
                Assert.AreEqual(ArmorSetRules.DropChance, far.DropChance, far.Id);
            }

            // Fewer own beasts in Regions 5–9: one kill in 100, against one in 200 up north.
            Assert.Greater(ArmorSetRules.MiddleDropChance, ArmorSetRules.DropChance);
            Assert.AreEqual(ArmorSetRules.LotusChestId, ArmorSetRules.RollDrop("enemy_forest_spear_heron", 0.0099, 0.2));
            Assert.AreEqual(ArmorSetRules.KurganHelmId, ArmorSetRules.RollDrop("enemy_mountain_felt_swan", 0.0, 0.7));
            Assert.IsNull(ArmorSetRules.RollDrop("enemy_forest_spear_heron", ArmorSetRules.MiddleDropChance, 0.0), "the chance is exclusive");
            Assert.IsNull(ArmorSetRules.RollDrop("enemy_forest_brick_lion", 0.006, 0.0), "the far sets keep 1 in 200");
        }

        [Test]
        public void Pieces_SitBetweenTheMarfimAndTheFarSets()
        {
            float boneChest = LoadBone("armor_bone_chest").ArmorDefense;
            float boneHelm = LoadBone("armor_bone_helm").ArmorDefense;
            float firstFarChest = Load(ArmorSetRules.Far[0].ChestItemId).ArmorDefense;
            float firstFarHelm = Load(ArmorSetRules.Far[0].HelmItemId).ArmorDefense;
            float previousChest = boneChest;
            float previousHelm = boneHelm;
            foreach (ArmorSetDefinition set in ArmorSetRules.Middle)
            {
                ItemDefinition chest = Load(set.ChestItemId);
                ItemDefinition helm = Load(set.HelmItemId);
                Assert.GreaterOrEqual(chest.ArmorDefense, previousChest, set.Id);
                Assert.GreaterOrEqual(helm.ArmorDefense, previousHelm, set.Id);
                Assert.Less(chest.ArmorDefense, firstFarChest, set.Id);
                Assert.Less(helm.ArmorDefense, firstFarHelm, set.Id);
                Assert.AreEqual(ItemRarity.Rare, chest.Rarity, set.Id);
                Assert.AreEqual(ItemRarity.Rare, helm.Rarity, set.Id);
                Assert.Less(chest.Value, Load(ArmorSetRules.Far[0].ChestItemId).Value, set.Id);
                Assert.Less(helm.Value, chest.Value, set.Id);
                previousChest = chest.ArmorDefense;
                previousHelm = helm.ArmorDefense;
            }
        }

        [Test]
        public void Attributes_NeverPassWhatTheFarSetsOrTheMarfimGive()
        {
            var ceiling = new Dictionary<(StatType, ModifierApplication), float>();
            IEnumerable<ItemDefinition> reference = ArmorSetRules.Far.SelectMany(s => new[] { Load(s.ChestItemId), Load(s.HelmItemId) })
                .Concat(new[] { LoadBone("armor_bone_chest"), LoadBone("armor_bone_helm") });
            foreach (ItemDefinition item in reference)
            {
                foreach (StatModifierDefinition mod in item.EquipStatModifiers)
                {
                    (StatType, ModifierApplication) key = (mod.affectedStat, mod.application);
                    ceiling[key] = Mathf.Max(ceiling.TryGetValue(key, out float max) ? max : 0f, mod.value);
                }
            }

            foreach (ArmorSetDefinition set in ArmorSetRules.Middle)
            {
                foreach (string id in new[] { set.ChestItemId, set.HelmItemId })
                {
                    ItemDefinition item = Load(id);
                    Assert.AreEqual(1, item.EquipStatModifiers.Length, id);
                    StatModifierDefinition mod = item.EquipStatModifiers[0];
                    Assert.IsTrue(ceiling.ContainsKey((mod.affectedStat, mod.application)), $"{id}: an attribute the reference sets also give");
                    Assert.LessOrEqual(mod.value, ceiling[(mod.affectedStat, mod.application)], id);
                    Assert.AreEqual(0f, mod.durationSeconds, id);
                }
            }
        }

        [Test]
        public void Bonuses_NeedTheirOwnSet_AndStayUnderTheFarOnes()
        {
            Assert.IsTrue(ArmorSetRules.ClosingCoralProcs(ArmorSetRules.Coral, 0.149));
            Assert.IsFalse(ArmorSetRules.ClosingCoralProcs(ArmorSetRules.Coral, 0.15));
            Assert.IsFalse(ArmorSetRules.ClosingCoralProcs(ArmorSetRules.BlueBrick, 0.0));
            Assert.AreEqual(30f, ArmorSetRules.ClosingCoralHeal(100f), 1e-3f);
            Assert.AreEqual(0f, ArmorSetRules.ClosingCoralHeal(0f));

            Assert.IsTrue(ArmorSetRules.LotusSapProcs(ArmorSetRules.Lotus, 0.199));
            Assert.IsFalse(ArmorSetRules.LotusSapProcs(ArmorSetRules.Lotus, 0.2));
            Assert.IsFalse(ArmorSetRules.LotusSapProcs(ArmorSetRules.Papyrus, 0.0));
            Assert.AreEqual(10f, ArmorSetRules.LotusSapMana(200f), 1e-3f);
            Assert.IsFalse(ArmorSetBonuses.TryLotusSap(null, ArmorSetRules.Lotus, 0.0));

            Assert.IsTrue(ArmorSetRules.PamirBreathProcs(ArmorSetRules.Yak, true, 0.199));
            Assert.IsFalse(ArmorSetRules.PamirBreathProcs(ArmorSetRules.Yak, false, 0.0), "only criticals");
            Assert.IsFalse(ArmorSetRules.PamirBreathProcs(ArmorSetRules.Seal, true, 0.0));
            Assert.AreEqual(30f, ArmorSetRules.PamirBreathHeal(1000f), 1e-3f);

            Assert.IsTrue(ArmorSetRules.BronzeGongProcs(ArmorSetRules.Bronze, 0.099));
            Assert.IsFalse(ArmorSetRules.BronzeGongProcs(ArmorSetRules.Bronze, 0.1));
            Assert.IsFalse(ArmorSetRules.BronzeGongProcs(ArmorSetRules.BlueBrick, 0.0));
            Assert.AreEqual(20f, ArmorSetRules.BronzeGongDamage(100f), 1e-3f);
            Assert.AreEqual(1f, ArmorSetRules.BronzeGongDamage(1f), 1e-3f, "never below 1");

            Assert.IsTrue(ArmorSetRules.NartGallopProcs(ArmorSetRules.Kurgan, true, 0.099));
            Assert.IsFalse(ArmorSetRules.NartGallopProcs(ArmorSetRules.Kurgan, false, 0.0), "only criticals");
            Assert.IsFalse(ArmorSetRules.NartGallopProcs(ArmorSetRules.Light, true, 0.0));
            Assert.AreEqual(30f, ArmorSetRules.NartGallopDamage(100f), 1e-3f);

            // Expected value per trigger never above the far bonus of the same kind (Docs/Balanco100a500.md: proportional to the band).
            Assert.Less(ArmorSetRules.ClosingCoralChance * ArmorSetRules.ClosingCoralHealFraction, ArmorSetRules.GlazedReflectChance * ArmorSetRules.GlazedReflectFraction);
            Assert.LessOrEqual(ArmorSetRules.LotusSapChance * ArmorSetRules.LotusSapManaFraction, ArmorSetRules.BargeHourChance * ArmorSetRules.BargeHourHealFraction);
            Assert.Less(ArmorSetRules.PamirBreathChance * ArmorSetRules.PamirBreathHealFraction, ArmorSetRules.ReopenedSealChance * ArmorSetRules.ReopenedSealManaFraction);
            Assert.LessOrEqual(ArmorSetRules.BronzeGongChance * ArmorSetRules.BronzeGongDamageFraction * ArmorSetRules.BronzeGongMaxTargets,
                ArmorSetRules.GlazedReflectChance * ArmorSetRules.GlazedReflectFraction);
            Assert.Less(ArmorSetRules.NartGallopChance * ArmorSetRules.NartGallopDamageFraction * ArmorSetRules.NartGallopMaxTargets,
                ArmorSetRules.ColumnRayChance * ArmorSetRules.ColumnRayDamageFraction * ArmorSetRules.ColumnRayMaxTargets);
            Assert.LessOrEqual(ArmorSetRules.BronzeGongRadius, ArmorSetRules.ColumnRayRadius);
            Assert.LessOrEqual(ArmorSetRules.NartGallopRadius, ArmorSetRules.ColumnRayRadius);
        }

        [Test]
        public void Texts_NameTheSetAndItsBonus_InBothLanguages()
        {
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> table = ContentTexts.Entries.ToDictionary(e => e.Key);
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> bonuses = ArmorSetTexts.Entries.ToDictionary(e => e.Key);
            string[] bonusKeys = { ArmorSetTexts.ClosingCoral, ArmorSetTexts.LotusSap, ArmorSetTexts.PamirBreath, ArmorSetTexts.BronzeGong, ArmorSetTexts.NartGallop };
            for (int i = 0; i < Sets.Length; i++)
            {
                (ArmorSetDefinition set, _, _, _, string namePt, string nameEn) = Sets[i];
                Rapadura.Core.Localization.LocalizationEntry bonus = bonuses[bonusKeys[i]];
                foreach (string id in new[] { set.ChestItemId, set.HelmItemId })
                {
                    Rapadura.Core.Localization.LocalizationEntry description = table[ContentText.ItemDescriptionKey(id)];
                    StringAssert.Contains("Conjunto " + namePt + " (2 peças): " + bonus.PtBR.TrimEnd('!') + ".", description.PtBR, id);
                    StringAssert.Contains(nameEn + " set (2 pieces): " + bonus.En.TrimEnd('!') + ".", description.En, id);
                    Assert.AreNotEqual(table[ContentText.ItemNameKey(id)].PtBR, table[ContentText.ItemNameKey(id)].En, id);
                    StringAssert.DoesNotStartWith("Móveis", namePt, "the furniture sets are the ones called Móveis");
                }
            }
        }
    }
}
