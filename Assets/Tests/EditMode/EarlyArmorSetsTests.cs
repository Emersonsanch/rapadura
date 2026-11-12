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
    /// The four armour sets of Regions 1–4, the hub regions (Docs/Items.md), in the mould of <see cref="MiddleArmorSetsTests"/>: each
    /// one drops from its region's own beasts, which really walk that region at its levels; the pieces are Uncommon, between the
    /// leather of the shops and the Marfim (Ossuário, level 35), and below every set of Regions 5–9; the two-piece bonuses never give
    /// more than the middle ones do. Pure rules only.
    /// </summary>
    public class EarlyArmorSetsTests
    {
        private static readonly (ArmorSetDefinition Set, string RegionId, int Low, int High, string NamePt, string NameEn)[] Sets =
        {
            (ArmorSetRules.Garden, OverworldLayouts.ZarafshanId, 1, 32, "Jardim de Mihrshah", "Mihrshah's Garden"),
            (ArmorSetRules.RedLeather, OverworldLayouts.AreiasId, 5, 38, "Couro Rubro", "Red Leather"),
            (ArmorSetRules.Drystone, OverworldLayouts.KaggenId, 9, 44, "Pedra Seca", "Drystone"),
            (ArmorSetRules.Wick, OverworldLayouts.LimiarId, 13, 50, "Pavio do Limiar", "Threshold Wick"),
        };

        private static ItemDefinition Load(string id) => Resources.Load<ItemDefinition>("Items/Sets/" + id);

        private static ItemDefinition LoadBone(string id) => Resources.Load<ItemDefinition>("Items/Dungeon/" + id);

        private static ItemDefinition LoadShop(string id) => Resources.Load<ItemDefinition>("Items/" + id);

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            MapLayouts.ClearCache();
            OverworldLayouts.ClearCache();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() => MapLayouts.ClearCache();

        [Test]
        public void TheEarlySets_AreTheFourOfRegions1To4_InRegionOrder_AndFirstInAll()
        {
            CollectionAssert.AreEqual(Sets.Select(s => s.Set).ToList(), ArmorSetRules.Early.ToList());
            CollectionAssert.AreEqual(ArmorSetRules.Early.ToList(), ArmorSetRules.All.Take(ArmorSetRules.Early.Count).ToList());
            Assert.AreEqual(ArmorSetRules.Early.Count + ArmorSetRules.Middle.Count + ArmorSetRules.High.Count + ArmorSetRules.Far.Count, ArmorSetRules.All.Count);
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
        public void TheSource_IsReachable_AtTheRegionsLevels_AtOneKillIn200()
        {
            foreach ((ArmorSetDefinition set, string regionId, int low, int high, _, _) in Sets)
            {
                OverworldLayout world = OverworldLayouts.Get(regionId);
                int firstLevel = world.World.Zones.Where(z => z.AllEntries().Any(e => e != null && set.Beasts.Contains(e.EnemyId))).Min(z => z.MinLevel);
                Assert.That(firstLevel, Is.InRange(low - 5, high), $"{set.Id}: the first beast that drops it stands at the region's levels");
                Assert.AreEqual(ArmorSetRules.EarlyDropChance, set.DropChance, set.Id);
            }

            // Almost everything that walks Regions 1–4 is their own: rarer per kill than the middle sets.
            Assert.Less(ArmorSetRules.EarlyDropChance, ArmorSetRules.MiddleDropChance);
            Assert.AreEqual(ArmorSetRules.GardenChestId, ArmorSetRules.RollDrop("enemy_forest_wolf", 0.0049, 0.2));
            Assert.AreEqual(ArmorSetRules.WickHelmId, ArmorSetRules.RollDrop("enemy_cave_ooze", 0.0, 0.7));
            Assert.AreEqual(ArmorSetRules.RedLeatherChestId, ArmorSetRules.RollDrop("enemy_desert_scorpion", 0.001, 0.0));
            Assert.AreEqual(ArmorSetRules.DrystoneHelmId, ArmorSetRules.RollDrop("enemy_mountain_stone_golem", 0.001, 0.99));
            Assert.IsNull(ArmorSetRules.RollDrop("enemy_forest_wolf", ArmorSetRules.EarlyDropChance, 0.0), "the chance is exclusive");
            Assert.IsNull(ArmorSetRules.RollDrop("enemy_forest_alpha_wolf", 0.0, 0.0), "Kök-Böri, a boss, is no set beast");
        }

        [Test]
        public void Pieces_AreUncommon_AboveTheLeather_BelowTheMarfimAndTheMiddleSets()
        {
            float leatherChest = LoadShop("item_leather_chest").ArmorDefense;
            float leatherHelm = LoadShop("item_leather_helmet").ArmorDefense;
            ItemDefinition boneChest = LoadBone("armor_bone_chest");
            ItemDefinition boneHelm = LoadBone("armor_bone_helm");
            ItemDefinition firstMiddleChest = Load(ArmorSetRules.Middle[0].ChestItemId);
            ItemDefinition firstMiddleHelm = Load(ArmorSetRules.Middle[0].HelmItemId);
            float previousChest = leatherChest;
            float previousHelm = leatherHelm;
            foreach (ArmorSetDefinition set in ArmorSetRules.Early)
            {
                ItemDefinition chest = Load(set.ChestItemId);
                ItemDefinition helm = Load(set.HelmItemId);
                Assert.IsNotNull(chest, set.ChestItemId);
                Assert.IsNotNull(helm, set.HelmItemId);
                Assert.Greater(chest.ArmorDefense, previousChest, set.Id);
                Assert.Greater(helm.ArmorDefense, previousHelm, set.Id);
                Assert.Less(chest.ArmorDefense, boneChest.ArmorDefense, set.Id);
                Assert.Less(helm.ArmorDefense, boneHelm.ArmorDefense, set.Id);
                Assert.Less(chest.ArmorDefense, firstMiddleChest.ArmorDefense, set.Id);
                Assert.Less(helm.ArmorDefense, firstMiddleHelm.ArmorDefense, set.Id);
                Assert.AreEqual(ItemRarity.Uncommon, chest.Rarity, set.Id);
                Assert.AreEqual(ItemRarity.Uncommon, helm.Rarity, set.Id);
                Assert.Less(chest.Value, boneChest.Value, set.Id);
                Assert.Less(helm.Value, chest.Value, set.Id);
                Assert.Less(chest.MaxDurability, firstMiddleChest.MaxDurability, set.Id);
                previousChest = chest.ArmorDefense;
                previousHelm = helm.ArmorDefense;
            }
        }

        [Test]
        public void Attributes_StayUnderWhatTheMiddleSetsGive()
        {
            var floor = new Dictionary<(StatType, ModifierApplication), float>();
            foreach (ArmorSetDefinition set in ArmorSetRules.Middle)
            {
                foreach (StatModifierDefinition mod in new[] { Load(set.ChestItemId), Load(set.HelmItemId) }.SelectMany(i => i.EquipStatModifiers))
                {
                    (StatType, ModifierApplication) key = (mod.affectedStat, mod.application);
                    floor[key] = Mathf.Min(floor.TryGetValue(key, out float min) ? min : float.MaxValue, mod.value);
                }
            }

            foreach (ArmorSetDefinition set in ArmorSetRules.Early)
            {
                foreach (string id in new[] { set.ChestItemId, set.HelmItemId })
                {
                    ItemDefinition item = Load(id);
                    Assert.AreEqual(1, item.EquipStatModifiers.Length, id);
                    StatModifierDefinition mod = item.EquipStatModifiers[0];
                    Assert.IsTrue(floor.ContainsKey((mod.affectedStat, mod.application)), $"{id}: an attribute the middle sets also give");
                    Assert.Less(mod.value, floor[(mod.affectedStat, mod.application)], id);
                    Assert.Greater(mod.value, 0f, id);
                    Assert.AreEqual(0f, mod.durationSeconds, id);
                }
            }
        }

        [Test]
        public void Bonuses_NeedTheirOwnSet_AndStayUnderTheMiddleOnes()
        {
            Assert.IsTrue(ArmorSetRules.GardenDewProcs(ArmorSetRules.Garden, 0.149));
            Assert.IsFalse(ArmorSetRules.GardenDewProcs(ArmorSetRules.Garden, 0.15));
            Assert.IsFalse(ArmorSetRules.GardenDewProcs(ArmorSetRules.Papyrus, 0.0));
            Assert.AreEqual(30f, ArmorSetRules.GardenDewHeal(1000f), 1e-3f);
            Assert.AreEqual(0f, ArmorSetRules.GardenDewHeal(0f));
            Assert.IsFalse(ArmorSetBonuses.TryGardenDew(null, ArmorSetRules.Garden, 0.0));

            Assert.IsTrue(ArmorSetRules.BurningSandProcs(ArmorSetRules.RedLeather, 0.099));
            Assert.IsFalse(ArmorSetRules.BurningSandProcs(ArmorSetRules.RedLeather, 0.1));
            Assert.IsFalse(ArmorSetRules.BurningSandProcs(ArmorSetRules.BlueBrick, 0.0));
            Assert.AreEqual(20f, ArmorSetRules.BurningSandDamage(100f), 1e-3f);
            Assert.AreEqual(1f, ArmorSetRules.BurningSandDamage(1f), 1e-3f, "never below 1");
            Assert.AreEqual(0f, ArmorSetRules.BurningSandDamage(0f));

            Assert.IsTrue(ArmorSetRules.KaggenGustProcs(ArmorSetRules.Drystone, true, 0.099));
            Assert.IsFalse(ArmorSetRules.KaggenGustProcs(ArmorSetRules.Drystone, false, 0.0), "only criticals");
            Assert.IsFalse(ArmorSetRules.KaggenGustProcs(ArmorSetRules.Kurgan, true, 0.0));
            Assert.AreEqual(20f, ArmorSetRules.KaggenGustDamage(100f), 1e-3f);
            Assert.AreEqual(1f, ArmorSetRules.KaggenGustDamage(1f), 1e-3f, "never below 1");

            Assert.IsTrue(ArmorSetRules.LitWickProcs(ArmorSetRules.Wick, 0.149));
            Assert.IsFalse(ArmorSetRules.LitWickProcs(ArmorSetRules.Wick, 0.15));
            Assert.IsFalse(ArmorSetRules.LitWickProcs(ArmorSetRules.Lotus, 0.0));
            Assert.AreEqual(8f, ArmorSetRules.LitWickMana(200f), 1e-3f);
            Assert.IsFalse(ArmorSetBonuses.TryLitWick(null, ArmorSetRules.Wick, 0.0));

            // A middle set's bonus never fires for an early set, and the other way round.
            Assert.IsFalse(ArmorSetRules.LotusSapProcs(ArmorSetRules.Wick, 0.0));
            Assert.IsFalse(ArmorSetRules.ClosingCoralProcs(ArmorSetRules.RedLeather, 0.0));
            Assert.IsFalse(ArmorSetRules.NartGallopProcs(ArmorSetRules.Drystone, true, 0.0));

            // Expected value per trigger under the middle bonus of the same kind (and the heal under every middle heal).
            double gardenDew = ArmorSetRules.GardenDewChance * ArmorSetRules.GardenDewHealFraction;
            Assert.Less(gardenDew, ArmorSetRules.LotusSapChance * ArmorSetRules.LotusSapManaFraction, "a kill");
            Assert.Less(gardenDew, ArmorSetRules.PamirBreathChance * ArmorSetRules.PamirBreathHealFraction);
            Assert.Less(gardenDew, ArmorSetRules.BargeHourChance * ArmorSetRules.BargeHourHealFraction);
            Assert.Less(ArmorSetRules.BurningSandChance * ArmorSetRules.BurningSandFraction, ArmorSetRules.ClosingCoralChance * ArmorSetRules.ClosingCoralHealFraction);
            Assert.Less(ArmorSetRules.BurningSandChance * ArmorSetRules.BurningSandFraction,
                ArmorSetRules.BronzeGongChance * ArmorSetRules.BronzeGongDamageFraction * ArmorSetRules.BronzeGongMaxTargets);
            Assert.Less(ArmorSetRules.KaggenGustChance * ArmorSetRules.KaggenGustDamageFraction * ArmorSetRules.KaggenGustMaxTargets,
                ArmorSetRules.NartGallopChance * ArmorSetRules.NartGallopDamageFraction * ArmorSetRules.NartGallopMaxTargets);
            Assert.Less(ArmorSetRules.KaggenGustRadius, ArmorSetRules.NartGallopRadius);
            Assert.Less(ArmorSetRules.LitWickChance * ArmorSetRules.LitWickManaFraction, ArmorSetRules.LotusSapChance * ArmorSetRules.LotusSapManaFraction);
        }

        [Test]
        public void Texts_NameTheSetAndItsBonus_InBothLanguages()
        {
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> table = ContentTexts.Entries.ToDictionary(e => e.Key);
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> bonuses = ArmorSetTexts.Entries.ToDictionary(e => e.Key);
            string[] bonusKeys = { ArmorSetTexts.GardenDew, ArmorSetTexts.BurningSand, ArmorSetTexts.KaggenGust, ArmorSetTexts.LitWick };
            for (int i = 0; i < Sets.Length; i++)
            {
                (ArmorSetDefinition set, _, _, _, string namePt, string nameEn) = Sets[i];
                Rapadura.Core.Localization.LocalizationEntry bonus = bonuses[bonusKeys[i]];
                foreach (string id in new[] { set.ChestItemId, set.HelmItemId })
                {
                    ItemDefinition item = Load(id);
                    Rapadura.Core.Localization.LocalizationEntry name = table[ContentText.ItemNameKey(id)];
                    Rapadura.Core.Localization.LocalizationEntry description = table[ContentText.ItemDescriptionKey(id)];
                    Assert.AreEqual(item.DisplayName, name.PtBR, id);
                    Assert.AreEqual(item.Description, description.PtBR, id);
                    StringAssert.Contains("Conjunto " + namePt + " (2 peças): " + bonus.PtBR.TrimEnd('!') + ".", description.PtBR, id);
                    StringAssert.Contains(nameEn + " set (2 pieces): " + bonus.En.TrimEnd('!') + ".", description.En, id);
                    Assert.AreNotEqual(name.PtBR, name.En, id);
                    StringAssert.DoesNotStartWith("Móveis", namePt, "the furniture sets are the ones called Móveis");
                }
            }
        }
    }
}
