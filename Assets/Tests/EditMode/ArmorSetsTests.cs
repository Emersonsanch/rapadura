using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Sets;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>The twenty armour sets of Regions 1–16, two in each of 13–16 (Docs/Items.md): pieces, drop rule, the two-piece bonuses and the texts.</summary>
    public class ArmorSetsTests
    {
        private static IEnumerable<ArmorSetDefinition> Sets => ArmorSetRules.All;

        private static ItemDefinition Load(string id) => Resources.Load<ItemDefinition>("Items/Sets/" + id);

        [Test]
        public void EverySet_HasAChestAndAHelm_WithIcon()
        {
            Assert.AreEqual(20, ArmorSetRules.All.Count);
            foreach (ArmorSetDefinition set in Sets)
            {
                ItemDefinition chest = Load(set.ChestItemId);
                ItemDefinition helm = Load(set.HelmItemId);
                Assert.IsNotNull(chest, set.ChestItemId);
                Assert.IsNotNull(helm, set.HelmItemId);
                Assert.AreEqual(EquipmentSlot.Chest, chest.EquipSlot, set.Id);
                Assert.AreEqual(EquipmentSlot.Head, helm.EquipSlot, set.Id);
                Assert.IsNotNull(chest.Icon, set.ChestItemId);
                Assert.IsNotNull(helm.Icon, set.HelmItemId);
                Assert.IsTrue(EnemyLootRoller.IsRolledAsEquipment(chest), set.ChestItemId);
                Assert.IsTrue(EnemyLootRoller.IsRolledAsEquipment(helm), set.HelmItemId);
            }
        }

        [Test]
        public void Defense_GrowsWithTheRegion()
        {
            float previousChest = 0f;
            float previousHelm = 0f;
            foreach (ArmorSetDefinition set in ArmorSetRules.Far)
            {
                float chest = Load(set.ChestItemId).ArmorDefense;
                float helm = Load(set.HelmItemId).ArmorDefense;
                Assert.Greater(chest, previousChest, set.Id);
                Assert.Greater(helm, previousHelm, set.Id);
                previousChest = chest;
                previousHelm = helm;
            }
        }

        [Test]
        public void EveryBeast_ExistsAndBelongsToOneSet()
        {
            var seen = new HashSet<string>();
            foreach (ArmorSetDefinition set in Sets)
            {
                // Regions 13–16 have 12 own beasts each plus the stand-in passes that landed there (up to 15); Regions 1–9 have 6–12
                // (Docs/Status_100Mapas.md); the second sets of 13–16 take the 3–6 own beasts the first one left out.
                bool ancient = ArmorSetRules.Ancient.Contains(set);
                Assert.GreaterOrEqual(set.Beasts.Count, ancient ? 3 : ArmorSetRules.Far.Contains(set) ? 12 : 6, set.Id);
                Assert.LessOrEqual(set.Beasts.Count, ancient ? 6 : ArmorSetRules.Far.Contains(set) ? 15 : 12, set.Id);
                foreach (string beast in set.Beasts)
                {
                    Assert.IsTrue(seen.Add(beast), beast + " is in two sets");
                    Assert.IsNotNull(Resources.Load<EnemyDefinition>("Enemies/" + beast), beast);
                    Assert.AreSame(set, ArmorSetRules.SetOfBeast(beast), beast);
                }
            }
        }

        [Test]
        public void RollDrop_OnlyRegionBeasts_AtTheChance()
        {
            const string beast = "enemy_forest_brick_lion";
            Assert.AreEqual(ArmorSetRules.BlueBrickChestId, ArmorSetRules.RollDrop(beast, 0.0, 0.49));
            Assert.AreEqual(ArmorSetRules.BlueBrickHelmId, ArmorSetRules.RollDrop(beast, 0.0049, 0.5));
            Assert.IsNull(ArmorSetRules.RollDrop(beast, ArmorSetRules.DropChance, 0.0), "the chance is exclusive");
            Assert.IsNull(ArmorSetRules.RollDrop("enemy_cave_dweller", 0.0, 0.0), "other beasts (here a region boss) drop nothing");
            Assert.IsNull(ArmorSetRules.RollDrop(null, 0.0, 0.0));
        }

        [Test]
        public void CountPieces_AreDistinct_AndCompleteSetNeedsBoth()
        {
            ArmorSetDefinition set = ArmorSetRules.Seal;
            Assert.AreEqual(0, ArmorSetRules.CountPieces(set, null));
            Assert.AreEqual(1, ArmorSetRules.CountPieces(set, new[] { set.ChestItemId, set.ChestItemId, null }));
            Assert.AreEqual(2, ArmorSetRules.CountPieces(set, new[] { set.HelmItemId, "item_iron_sword", set.ChestItemId }));

            Assert.IsNull(ArmorSetRules.CompleteSet(null));
            Assert.IsNull(ArmorSetRules.CompleteSet(new[] { set.ChestItemId, ArmorSetRules.LightHelmId }), "mixed pieces are no set");
            Assert.AreSame(set, ArmorSetRules.CompleteSet(new[] { set.ChestItemId, set.HelmItemId }));
            Assert.IsNull(ArmorSetBonuses.WornSet(null));
        }

        [Test]
        public void BargeHour_NeedsThePapyrusSet()
        {
            Assert.IsTrue(ArmorSetRules.BargeHourProcs(ArmorSetRules.Papyrus, 0.249));
            Assert.IsFalse(ArmorSetRules.BargeHourProcs(ArmorSetRules.Papyrus, 0.25));
            Assert.IsFalse(ArmorSetRules.BargeHourProcs(ArmorSetRules.Light, 0.0));
            Assert.IsFalse(ArmorSetRules.BargeHourProcs(null, 0.0));
            Assert.AreEqual(50f, ArmorSetRules.BargeHourHeal(1000f), 1e-3f);
            Assert.AreEqual(0f, ArmorSetRules.BargeHourHeal(0f));
        }

        [Test]
        public void GlazedReflect_NeedsTheBrickSet()
        {
            Assert.IsTrue(ArmorSetRules.GlazedReflectProcs(ArmorSetRules.BlueBrick, 0.199));
            Assert.IsFalse(ArmorSetRules.GlazedReflectProcs(ArmorSetRules.BlueBrick, 0.2));
            Assert.IsFalse(ArmorSetRules.GlazedReflectProcs(ArmorSetRules.Seal, 0.0));
            Assert.AreEqual(35f, ArmorSetRules.GlazedReflectDamage(100f), 1e-3f);
            Assert.AreEqual(1f, ArmorSetRules.GlazedReflectDamage(1f), 1e-3f, "never below 1");
            Assert.AreEqual(0f, ArmorSetRules.GlazedReflectDamage(0f));
        }

        [Test]
        public void ReopenedSeal_NeedsACritical()
        {
            Assert.IsTrue(ArmorSetRules.ReopenedSealProcs(ArmorSetRules.Seal, true, 0.249));
            Assert.IsFalse(ArmorSetRules.ReopenedSealProcs(ArmorSetRules.Seal, true, 0.25));
            Assert.IsFalse(ArmorSetRules.ReopenedSealProcs(ArmorSetRules.Seal, false, 0.0), "only criticals");
            Assert.IsFalse(ArmorSetRules.ReopenedSealProcs(ArmorSetRules.Papyrus, true, 0.0));
            Assert.AreEqual(16f, ArmorSetRules.ReopenedSealMana(200f), 1e-3f);
        }

        [Test]
        public void ColumnRay_NeedsACritical()
        {
            Assert.IsTrue(ArmorSetRules.ColumnRayProcs(ArmorSetRules.Light, true, 0.149));
            Assert.IsFalse(ArmorSetRules.ColumnRayProcs(ArmorSetRules.Light, true, 0.15));
            Assert.IsFalse(ArmorSetRules.ColumnRayProcs(ArmorSetRules.Light, false, 0.0), "only criticals");
            Assert.IsFalse(ArmorSetRules.ColumnRayProcs(ArmorSetRules.BlueBrick, true, 0.0));
            Assert.AreEqual(40f, ArmorSetRules.ColumnRayDamage(100f), 1e-3f);
            Assert.AreEqual(1f, ArmorSetRules.ColumnRayDamage(1f), 1e-3f, "never below 1");
        }

        [Test]
        public void Texts_ExistInBothLanguages_AndMatchTheAssets()
        {
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> table = ContentTexts.Entries.ToDictionary(e => e.Key);
            foreach (ArmorSetDefinition set in Sets)
            {
                foreach (string id in new[] { set.ChestItemId, set.HelmItemId })
                {
                    ItemDefinition item = Load(id);
                    Rapadura.Core.Localization.LocalizationEntry name = table[ContentText.ItemNameKey(id)];
                    Rapadura.Core.Localization.LocalizationEntry description = table[ContentText.ItemDescriptionKey(id)];
                    Assert.AreEqual(item.DisplayName, name.PtBR, id);
                    Assert.AreEqual(item.Description, description.PtBR, id);
                    Assert.IsNotEmpty(name.En, id);
                    Assert.IsNotEmpty(description.En, id);
                }
            }

            foreach (Rapadura.Core.Localization.LocalizationEntry entry in ArmorSetTexts.Entries)
            {
                Assert.IsNotEmpty(entry.En, entry.Key);
                Assert.IsNotEmpty(entry.PtBR, entry.Key);
                Assert.AreEqual(entry.PtBR, ArmorSetTexts.Get(null, entry.Key));
            }
        }
    }
}
