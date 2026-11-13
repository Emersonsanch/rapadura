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
    /// The second armour sets of Regions 13–16, the old sets of the fichas (Docs/Items.md), in the mould of <see cref="HighArmorSetsTests"/>:
    /// each one drops from the 3 own beasts its region's first set left out, which really walk that region at its levels; each piece is one
    /// defence point below the first set's piece of the same slot, with the same rarity, value and durability and the first set's two
    /// attributes swapped between the slots, so it is a side choice and never an upgrade; the bonuses sit beside the far ones. Pure rules only.
    /// </summary>
    public class AncientArmorSetsTests
    {
        private static readonly (ArmorSetDefinition Set, ArmorSetDefinition First, string RegionId, int Low, int High, string NamePt, string NameEn)[] Sets =
        {
            (ArmorSetRules.MehenScale, ArmorSetRules.Papyrus, OverworldLayouts.RioRegionId, 260, 320, "Escamas de Mehen", "Mehen Scales"),
            (ArmorSetRules.Giants, ArmorSetRules.BlueBrick, OverworldLayouts.MisturaRegionId, 300, 360, "Gigantes", "Giants"),
            (ArmorSetRules.FiveLights, ArmorSetRules.Seal, OverworldLayouts.ProfundoRegionId, 340, 440, "Cinco Luzes", "Five Lights"),
            (ArmorSetRules.GloryColumn, ArmorSetRules.Light, OverworldLayouts.CeuRegionId, 420, 500, "Coluna da Glória", "Column of Glory"),
        };

        private static ItemDefinition Load(string id) => Resources.Load<ItemDefinition>("Items/Sets/" + id);

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            MapLayouts.ClearCache();
            OverworldLayouts.ClearCache();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() => MapLayouts.ClearCache();

        [Test]
        public void TheAncientSets_AreTheFourOfRegions13To16_InRegionOrder_AfterEveryOtherSet()
        {
            CollectionAssert.AreEqual(Sets.Select(s => s.Set).ToList(), ArmorSetRules.Ancient.ToList());
            CollectionAssert.AreEqual(Sets.Select(s => s.First).ToList(), ArmorSetRules.Far.ToList());
            List<ArmorSetDefinition> all = ArmorSetRules.All.ToList();
            CollectionAssert.AreEqual(ArmorSetRules.Ancient.ToList(), all.Skip(all.Count - ArmorSetRules.Ancient.Count).ToList());
            Assert.AreEqual(all.Count, all.Select(s => s.Id).Distinct().Count());
        }

        [Test]
        public void EveryBeast_WalksItsRegion_AtTheRegionsLevels_AndDropsNoOtherSet()
        {
            foreach ((ArmorSetDefinition set, ArmorSetDefinition first, string regionId, int low, int high, _, _) in Sets)
            {
                Assert.That(set.Beasts.Count, Is.InRange(3, 6), set.Id);
                OverworldLayout world = OverworldLayouts.Get(regionId);
                Assert.IsNotNull(world, regionId);
                foreach (string beast in set.Beasts)
                {
                    Assert.IsFalse(first.Beasts.Contains(beast), $"{set.Id}: {beast} already drops the region's first set");
                    Assert.AreSame(set, ArmorSetRules.SetOfBeast(beast), beast);
                    List<MapSpawnZone> spawns = world.World.Zones.Where(z => z.AllEntries().Any(e => e != null && e.EnemyId == beast)).ToList();
                    Assert.IsNotEmpty(spawns, $"{set.Id}: {beast} spawns in {regionId}");
                    Assert.IsTrue(spawns.Any(z => z.MaxLevel >= low && z.MinLevel <= high), $"{set.Id}: {beast} stands within {low}–{high}");
                }
            }
        }

        [Test]
        public void TheSource_IsTheRarestOfTheDropChances_BecauseItIsThreeBeasts()
        {
            foreach ((ArmorSetDefinition set, _, _, _, _, _, _) in Sets)
            {
                Assert.AreEqual(ArmorSetRules.AncientDropChance, set.DropChance, set.Id);
            }

            Assert.Greater(ArmorSetRules.AncientDropChance, ArmorSetRules.MiddleDropChance);
            Assert.LessOrEqual(ArmorSetRules.AncientDropChance, 0.05, "still a rare piece: one kill in 50 or fewer");
            Assert.AreEqual(ArmorSetRules.MehenScaleChestId, ArmorSetRules.RollDrop("enemy_forest_child_of_apep", 0.0199, 0.2));
            Assert.AreEqual(ArmorSetRules.GiantsHelmId, ArmorSetRules.RollDrop("enemy_forest_scorpion_man", 0.0, 0.7));
            Assert.AreEqual(ArmorSetRules.FiveLightsChestId, ArmorSetRules.RollDrop("enemy_forest_great_eel", 0.001, 0.1));
            Assert.AreEqual(ArmorSetRules.GloryColumnHelmId, ArmorSetRules.RollDrop("enemy_forest_smoke_rope", 0.01, 0.9));
            Assert.IsNull(ArmorSetRules.RollDrop("enemy_forest_child_of_apep", ArmorSetRules.AncientDropChance, 0.0), "the chance is exclusive");
        }

        [Test]
        public void Pieces_AreOnePointBelowTheFirstSet_WithItsRarityValueAndDurability()
        {
            foreach ((ArmorSetDefinition set, ArmorSetDefinition first, _, _, _, _, _) in Sets)
            {
                foreach ((string id, string firstId) in new[] { (set.ChestItemId, first.ChestItemId), (set.HelmItemId, first.HelmItemId) })
                {
                    ItemDefinition item = Load(id);
                    ItemDefinition firstItem = Load(firstId);
                    Assert.IsNotNull(item, id);
                    Assert.AreEqual(firstItem.EquipSlot, item.EquipSlot, id);
                    Assert.AreEqual(firstItem.ArmorDefense - 1f, item.ArmorDefense, 1e-3f, id);
                    Assert.AreEqual(firstItem.Rarity, item.Rarity, id);
                    Assert.AreEqual(firstItem.Value, item.Value, id);
                    Assert.AreEqual(firstItem.MaxDurability, item.MaxDurability, id);
                }
            }
        }

        [Test]
        public void Attributes_AreTheFirstSetsTwo_SwappedBetweenTheSlots()
        {
            foreach ((ArmorSetDefinition set, ArmorSetDefinition first, _, _, _, _, _) in Sets)
            {
                StatModifierDefinition Mod(string id)
                {
                    ItemDefinition item = Load(id);
                    Assert.AreEqual(1, item.EquipStatModifiers.Length, id);
                    Assert.AreEqual(0f, item.EquipStatModifiers[0].durationSeconds, id);
                    return item.EquipStatModifiers[0];
                }

                StatModifierDefinition chest = Mod(set.ChestItemId);
                StatModifierDefinition helm = Mod(set.HelmItemId);
                StatModifierDefinition firstChest = Mod(first.ChestItemId);
                StatModifierDefinition firstHelm = Mod(first.HelmItemId);
                Assert.AreEqual((firstHelm.affectedStat, firstHelm.application), (chest.affectedStat, chest.application), set.Id);
                Assert.AreEqual(firstHelm.value, chest.value, 1e-5f, set.Id);
                Assert.AreEqual((firstChest.affectedStat, firstChest.application), (helm.affectedStat, helm.application), set.Id);
                Assert.AreEqual(firstChest.value, helm.value, 1e-5f, set.Id);
            }
        }

        [Test]
        public void Bonuses_NeedTheirOwnSet_AndSitBesideTheFarOnes()
        {
            Assert.IsTrue(ArmorSetRules.MehenCoilProcs(ArmorSetRules.MehenScale, 0.179));
            Assert.IsFalse(ArmorSetRules.MehenCoilProcs(ArmorSetRules.MehenScale, 0.18));
            Assert.IsFalse(ArmorSetRules.MehenCoilProcs(ArmorSetRules.Papyrus, 0.0));
            Assert.AreEqual(35f, ArmorSetRules.MehenCoilHeal(100f), 1e-3f);
            Assert.AreEqual(0f, ArmorSetRules.MehenCoilHeal(0f));

            Assert.IsTrue(ArmorSetRules.GiantsTremorProcs(ArmorSetRules.Giants, 0.149));
            Assert.IsFalse(ArmorSetRules.GiantsTremorProcs(ArmorSetRules.Giants, 0.15));
            Assert.IsFalse(ArmorSetRules.GiantsTremorProcs(ArmorSetRules.BlueBrick, 0.0));
            Assert.AreEqual(30f, ArmorSetRules.GiantsTremorDamage(100f), 1e-3f);
            Assert.AreEqual(1f, ArmorSetRules.GiantsTremorDamage(1f), 1e-3f, "never below 1");
            Assert.AreEqual(0f, ArmorSetRules.GiantsTremorDamage(0f));

            Assert.IsTrue(ArmorSetRules.ReturningLightProcs(ArmorSetRules.FiveLights, 0.199));
            Assert.IsFalse(ArmorSetRules.ReturningLightProcs(ArmorSetRules.FiveLights, 0.2));
            Assert.IsFalse(ArmorSetRules.ReturningLightProcs(ArmorSetRules.Seal, 0.0));
            Assert.AreEqual(40f, ArmorSetRules.ReturningLightHeal(1000f), 1e-3f);
            Assert.AreEqual(8f, ArmorSetRules.ReturningLightMana(200f), 1e-3f);
            Assert.IsFalse(ArmorSetBonuses.TryReturningLight(null, ArmorSetRules.FiveLights, 0.0));

            Assert.IsTrue(ArmorSetRules.ColumnRiseProcs(ArmorSetRules.GloryColumn, true, 0.199));
            Assert.IsFalse(ArmorSetRules.ColumnRiseProcs(ArmorSetRules.GloryColumn, true, 0.2));
            Assert.IsFalse(ArmorSetRules.ColumnRiseProcs(ArmorSetRules.GloryColumn, false, 0.0), "only criticals");
            Assert.IsFalse(ArmorSetRules.ColumnRiseProcs(ArmorSetRules.Light, true, 0.0));
            Assert.AreEqual(50f, ArmorSetRules.ColumnRiseHeal(1000f), 1e-3f);

            // Expected value per trigger: above the middle bonus of the same kind and no more than the far bonus it sits beside.
            double coil = ArmorSetRules.MehenCoilChance * ArmorSetRules.MehenCoilHealFraction;
            Assert.Greater(coil, ArmorSetRules.ClosingCoralChance * ArmorSetRules.ClosingCoralHealFraction);
            Assert.Less(coil, ArmorSetRules.GlazedReflectChance * ArmorSetRules.GlazedReflectFraction);

            double tremor = ArmorSetRules.GiantsTremorChance * ArmorSetRules.GiantsTremorDamageFraction * ArmorSetRules.GiantsTremorMaxTargets;
            Assert.Greater(tremor, ArmorSetRules.BronzeGongChance * ArmorSetRules.BronzeGongDamageFraction * ArmorSetRules.BronzeGongMaxTargets);
            Assert.Less(tremor, ArmorSetRules.ColumnRayChance * ArmorSetRules.ColumnRayDamageFraction * ArmorSetRules.ColumnRayMaxTargets);
            Assert.That(ArmorSetRules.GiantsTremorRadius, Is.InRange(ArmorSetRules.BronzeGongRadius, ArmorSetRules.ColumnRayRadius));

            double lightHeal = ArmorSetRules.ReturningLightChance * ArmorSetRules.ReturningLightHealFraction;
            Assert.Less(lightHeal, ArmorSetRules.BargeHourChance * ArmorSetRules.BargeHourHealFraction, "it gives mana too");
            Assert.Less(ArmorSetRules.ReturningLightChance * ArmorSetRules.ReturningLightManaFraction, ArmorSetRules.LotusSapChance * ArmorSetRules.LotusSapManaFraction);

            double rise = ArmorSetRules.ColumnRiseChance * ArmorSetRules.ColumnRiseHealFraction;
            Assert.Greater(rise, ArmorSetRules.PamirBreathChance * ArmorSetRules.PamirBreathHealFraction);
            Assert.LessOrEqual(rise, ArmorSetRules.BargeHourChance * ArmorSetRules.BargeHourHealFraction);
        }

        [Test]
        public void Texts_NameTheSetAndItsBonus_InBothLanguages()
        {
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> table = ContentTexts.Entries.ToDictionary(e => e.Key);
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> bonuses = ArmorSetTexts.Entries.ToDictionary(e => e.Key);
            string[] bonusKeys = { ArmorSetTexts.MehenCoil, ArmorSetTexts.GiantsTremor, ArmorSetTexts.ReturningLight, ArmorSetTexts.ColumnRise };
            for (int i = 0; i < Sets.Length; i++)
            {
                (ArmorSetDefinition set, _, _, _, _, string namePt, string nameEn) = Sets[i];
                Rapadura.Core.Localization.LocalizationEntry bonus = bonuses[bonusKeys[i]];
                foreach (string id in new[] { set.ChestItemId, set.HelmItemId })
                {
                    ItemDefinition item = Load(id);
                    Assert.IsNotNull(item.Icon, id);
                    Rapadura.Core.Localization.LocalizationEntry name = table[ContentText.ItemNameKey(id)];
                    Rapadura.Core.Localization.LocalizationEntry description = table[ContentText.ItemDescriptionKey(id)];
                    Assert.AreEqual(item.DisplayName, name.PtBR, id);
                    Assert.AreEqual(item.Description, description.PtBR, id);
                    StringAssert.Contains("Conjunto " + namePt + " (2 peças): " + bonus.PtBR.TrimEnd('!') + ".", description.PtBR, id);
                    StringAssert.Contains(nameEn + " set (2 pieces): " + bonus.En.TrimEnd('!') + ".", description.En, id);
                    Assert.AreNotEqual(name.PtBR, name.En, id);
                }
            }
        }
    }
}
