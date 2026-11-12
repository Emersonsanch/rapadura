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
    /// The three armour sets of Regions 10–12 (Docs/Items.md), in the mould of <see cref="MiddleArmorSetsTests"/>: each one drops from
    /// its region's 12 own beasts, which really walk that region at its levels; the pieces sit between the last middle set (Ouro dos
    /// Kurgans) and the first far one (Cais do Papiro), and so do the two-piece bonuses. Pure rules only.
    /// </summary>
    public class HighArmorSetsTests
    {
        private static readonly (ArmorSetDefinition Set, string RegionId, int Low, int High, string NamePt, string NameEn)[] Sets =
        {
            (ArmorSetRules.WindBell, OverworldLayouts.FarnkeshRegionId, 140, 200, "Sino de Vento", "Wind Bell"),
            (ArmorSetRules.AuroraSilver, OverworldLayouts.MarGeladoRegionId, 180, 240, "Prata da Aurora", "Aurora Silver"),
            (ArmorSetRules.CelestialIron, OverworldLayouts.HinokuraRegionId, 220, 280, "Kurdalægon", "Kurdalægon"),
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
        public void TheHighSets_AreTheThreeOfRegions10To12_InRegionOrder_BetweenTheMiddleAndTheFarOnes()
        {
            CollectionAssert.AreEqual(Sets.Select(s => s.Set).ToList(), ArmorSetRules.High.ToList());
            List<ArmorSetDefinition> all = ArmorSetRules.All.ToList();
            int first = all.IndexOf(ArmorSetRules.WindBell);
            Assert.AreEqual(all.IndexOf(ArmorSetRules.Middle.Last()) + 1, first);
            Assert.AreEqual(all.IndexOf(ArmorSetRules.Far.First()) - ArmorSetRules.High.Count, first);
            Assert.AreEqual(all.Count, all.Select(s => s.Id).Distinct().Count());
        }

        [Test]
        public void EveryBeast_WalksItsRegion_AtTheRegionsLevels()
        {
            foreach ((ArmorSetDefinition set, string regionId, int low, int high, _, _) in Sets)
            {
                Assert.AreEqual(12, set.Beasts.Count, set.Id);
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
        public void TheSource_IsReachable_AtTheRegionsLevels_BetweenTheMiddleAndTheFarChance()
        {
            foreach ((ArmorSetDefinition set, string regionId, int low, int high, _, _) in Sets)
            {
                OverworldLayout world = OverworldLayouts.Get(regionId);
                int firstLevel = world.World.Zones.Where(z => z.AllEntries().Any(e => e != null && set.Beasts.Contains(e.EnemyId))).Min(z => z.MinLevel);
                Assert.That(firstLevel, Is.InRange(low - 5, high), $"{set.Id}: the first beast that drops it stands at the region's levels");
                Assert.AreEqual(ArmorSetRules.HighDropChance, set.DropChance, set.Id);
            }

            Assert.Less(ArmorSetRules.HighDropChance, ArmorSetRules.MiddleDropChance);
            Assert.Greater(ArmorSetRules.HighDropChance, ArmorSetRules.DropChance);
            Assert.AreEqual(ArmorSetRules.WindBellChestId, ArmorSetRules.RollDrop("enemy_mountain_takin", 0.0074, 0.2));
            Assert.AreEqual(ArmorSetRules.AuroraSilverHelmId, ArmorSetRules.RollDrop("enemy_mountain_polar_bear", 0.0, 0.7));
            Assert.AreEqual(ArmorSetRules.CelestialIronChestId, ArmorSetRules.RollDrop("enemy_mountain_slag_giant", 0.001, 0.1));
            Assert.IsNull(ArmorSetRules.RollDrop("enemy_mountain_takin", ArmorSetRules.HighDropChance, 0.0), "the chance is exclusive");
        }

        [Test]
        public void Pieces_AreRare_BetweenTheKurgansAndThePapyrus()
        {
            ItemDefinition lastMiddleChest = Load(ArmorSetRules.Middle.Last().ChestItemId);
            ItemDefinition lastMiddleHelm = Load(ArmorSetRules.Middle.Last().HelmItemId);
            ItemDefinition firstFarChest = Load(ArmorSetRules.Far[0].ChestItemId);
            ItemDefinition firstFarHelm = Load(ArmorSetRules.Far[0].HelmItemId);
            float previousChest = lastMiddleChest.ArmorDefense;
            float previousHelm = lastMiddleHelm.ArmorDefense;
            int previousValue = lastMiddleChest.Value;
            foreach (ArmorSetDefinition set in ArmorSetRules.High)
            {
                ItemDefinition chest = Load(set.ChestItemId);
                ItemDefinition helm = Load(set.HelmItemId);
                Assert.IsNotNull(chest, set.ChestItemId);
                Assert.IsNotNull(helm, set.HelmItemId);
                Assert.Greater(chest.ArmorDefense, previousChest, set.Id);
                Assert.Greater(helm.ArmorDefense, previousHelm, set.Id);
                Assert.Less(chest.ArmorDefense, firstFarChest.ArmorDefense, set.Id);
                Assert.Less(helm.ArmorDefense, firstFarHelm.ArmorDefense, set.Id);
                Assert.AreEqual(ItemRarity.Rare, chest.Rarity, set.Id);
                Assert.AreEqual(ItemRarity.Rare, helm.Rarity, set.Id);
                Assert.Greater(chest.Value, previousValue, set.Id);
                Assert.Less(chest.Value, firstFarChest.Value, set.Id);
                Assert.Less(helm.Value, chest.Value, set.Id);
                Assert.That(chest.MaxDurability, Is.InRange(lastMiddleChest.MaxDurability, firstFarChest.MaxDurability), set.Id);
                previousChest = chest.ArmorDefense;
                previousHelm = helm.ArmorDefense;
                previousValue = chest.Value;
            }
        }

        [Test]
        public void Attributes_NeverPassWhatTheFarSetsGive()
        {
            var ceiling = new Dictionary<(StatType, ModifierApplication), float>();
            foreach (ItemDefinition item in ArmorSetRules.Far.SelectMany(s => new[] { Load(s.ChestItemId), Load(s.HelmItemId) }))
            {
                foreach (StatModifierDefinition mod in item.EquipStatModifiers)
                {
                    (StatType, ModifierApplication) key = (mod.affectedStat, mod.application);
                    ceiling[key] = Mathf.Max(ceiling.TryGetValue(key, out float max) ? max : 0f, mod.value);
                }
            }

            foreach (ArmorSetDefinition set in ArmorSetRules.High)
            {
                foreach (string id in new[] { set.ChestItemId, set.HelmItemId })
                {
                    ItemDefinition item = Load(id);
                    Assert.AreEqual(1, item.EquipStatModifiers.Length, id);
                    StatModifierDefinition mod = item.EquipStatModifiers[0];
                    Assert.IsTrue(ceiling.ContainsKey((mod.affectedStat, mod.application)), $"{id}: an attribute the far sets also give");
                    Assert.LessOrEqual(mod.value, ceiling[(mod.affectedStat, mod.application)], id);
                    Assert.Greater(mod.value, 0f, id);
                    Assert.AreEqual(0f, mod.durationSeconds, id);
                }
            }
        }

        [Test]
        public void Bonuses_NeedTheirOwnSet_AndSitBetweenTheMiddleAndTheFarOnes()
        {
            Assert.IsTrue(ArmorSetRules.HermitageTollProcs(ArmorSetRules.WindBell, true, 0.119));
            Assert.IsFalse(ArmorSetRules.HermitageTollProcs(ArmorSetRules.WindBell, true, 0.12));
            Assert.IsFalse(ArmorSetRules.HermitageTollProcs(ArmorSetRules.WindBell, false, 0.0), "only criticals");
            Assert.IsFalse(ArmorSetRules.HermitageTollProcs(ArmorSetRules.Kurgan, true, 0.0));
            Assert.AreEqual(35f, ArmorSetRules.HermitageTollDamage(100f), 1e-3f);
            Assert.AreEqual(1f, ArmorSetRules.HermitageTollDamage(1f), 1e-3f, "never below 1");

            Assert.IsTrue(ArmorSetRules.LanternWarmthProcs(ArmorSetRules.AuroraSilver, 0.219));
            Assert.IsFalse(ArmorSetRules.LanternWarmthProcs(ArmorSetRules.AuroraSilver, 0.22));
            Assert.IsFalse(ArmorSetRules.LanternWarmthProcs(ArmorSetRules.Papyrus, 0.0));
            Assert.AreEqual(50f, ArmorSetRules.LanternWarmthHeal(1000f), 1e-3f);
            Assert.AreEqual(0f, ArmorSetRules.LanternWarmthHeal(0f));
            Assert.IsFalse(ArmorSetBonuses.TryLanternWarmth(null, ArmorSetRules.AuroraSilver, 0.0));

            Assert.IsTrue(ArmorSetRules.AnvilEmberProcs(ArmorSetRules.CelestialIron, 0.199));
            Assert.IsFalse(ArmorSetRules.AnvilEmberProcs(ArmorSetRules.CelestialIron, 0.2));
            Assert.IsFalse(ArmorSetRules.AnvilEmberProcs(ArmorSetRules.BlueBrick, 0.0));
            Assert.AreEqual(30f, ArmorSetRules.AnvilEmberDamage(100f), 1e-3f);
            Assert.AreEqual(1f, ArmorSetRules.AnvilEmberDamage(1f), 1e-3f, "never below 1");
            Assert.AreEqual(0f, ArmorSetRules.AnvilEmberDamage(0f));

            // Expected value per trigger between the middle bonus and the far bonus of the same kind.
            double toll = ArmorSetRules.HermitageTollChance * ArmorSetRules.HermitageTollDamageFraction * ArmorSetRules.HermitageTollMaxTargets;
            Assert.Greater(toll, ArmorSetRules.NartGallopChance * ArmorSetRules.NartGallopDamageFraction * ArmorSetRules.NartGallopMaxTargets);
            Assert.Less(toll, ArmorSetRules.ColumnRayChance * ArmorSetRules.ColumnRayDamageFraction * ArmorSetRules.ColumnRayMaxTargets);
            Assert.That(ArmorSetRules.HermitageTollRadius, Is.InRange(ArmorSetRules.NartGallopRadius, ArmorSetRules.ColumnRayRadius));

            double warmth = ArmorSetRules.LanternWarmthChance * ArmorSetRules.LanternWarmthHealFraction;
            Assert.Greater(warmth, ArmorSetRules.LotusSapChance * ArmorSetRules.LotusSapManaFraction);
            Assert.Less(warmth, ArmorSetRules.BargeHourChance * ArmorSetRules.BargeHourHealFraction);

            double ember = ArmorSetRules.AnvilEmberChance * ArmorSetRules.AnvilEmberFraction;
            Assert.Greater(ember, ArmorSetRules.ClosingCoralChance * ArmorSetRules.ClosingCoralHealFraction);
            Assert.Less(ember, ArmorSetRules.GlazedReflectChance * ArmorSetRules.GlazedReflectFraction);
        }

        [Test]
        public void Texts_NameTheSetAndItsBonus_InBothLanguages()
        {
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> table = ContentTexts.Entries.ToDictionary(e => e.Key);
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> bonuses = ArmorSetTexts.Entries.ToDictionary(e => e.Key);
            string[] bonusKeys = { ArmorSetTexts.HermitageToll, ArmorSetTexts.LanternWarmth, ArmorSetTexts.AnvilEmber };
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
