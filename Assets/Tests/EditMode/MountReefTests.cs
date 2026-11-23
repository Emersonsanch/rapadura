using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Mounts.Visuals;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map.Mechanics;

namespace Rapadura.Tests.EditMode
{
    /// <summary>
    /// Kirta's aquatic mount (<see cref="MountReefCatalog"/>, Docs/Mounts.md §3.15, ficha 28 "Apito de Concha para montarias aquáticas"): the
    /// Hipocampo de Kirta, its Apito de Concha on Danel's counter, the Uncommon budget, the swimmer's gates, Canto do Recife's numbers and the
    /// five cousins' lines when it comes home.
    /// </summary>
    public class MountReefTests
    {
        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            ServiceLocator.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            ServiceLocator.Clear();
        }

        [Test]
        public void TheHippocamp_IsUncommon_OnBudget_Aquatic_AndInTheCaderno()
        {
            MountSpec spec = MountReefCatalog.Specs.Single();
            Assert.AreEqual(MountReefCatalog.KirtaHippocamp, spec.MountId);
            MountDefinition definition = MountCatalog.Get(spec.MountId);
            Assert.IsNotNull(definition);
            Assert.AreEqual(ItemRarity.Uncommon, spec.Rarity);
            Assert.AreEqual(MountRules.BudgetFor(ItemRarity.Uncommon), definition.BudgetPoints, 1e-3f, "12 points (§2.4)");
            Assert.That(spec.Speed, Is.InRange(MountRules.MinSpeedMultiplier, MountRules.MaxSpeedMultiplier));
            Assert.AreEqual(MountType.Aquatico, spec.Type, "montaria aquática (ficha 28)");
            Assert.AreEqual(MountAbilityKind.ReefSong, definition.Ability.Kind);
            Assert.AreEqual(1, MountCatalog.Specs.Count(s => s.AbilityKind == MountAbilityKind.ReefSong), "one effect per mount");
            Assert.AreEqual(1, MountCatalog.Specs.Count(s => s.Recipe == MountVisualRecipe.KirtaHippocamp), "one model per species");
            CollectionAssert.Contains(MountCatalog.MountIds.ToList(), spec.MountId);
            Assert.AreEqual(MountCatalog.Specs.Count, MountCatalog.Specs.Select(s => s.MountId).Distinct().Count());

            MountCodexEntry entry = MountCodex.Entries.Single(e => e.MountId == spec.MountId);
            Assert.AreEqual(ItemRarity.Uncommon, entry.Rarity);
            Assert.IsTrue(entry.IsAvailable);
            Assert.AreEqual(MountCatalog.Specs.Count, MountCodex.Entries.Count(e => e.IsAvailable));
            Assert.IsTrue(EncumbranceRules.MountBaseCapacity.ContainsKey(spec.MountId));
        }

        [Test]
        public void TheConchWhistle_IsSoldByDanel_InTheUncommonBand_WithItsAsset_AndRedeemsTheHippocamp()
        {
            Assert.AreEqual("item_whistle_mount_kirta_hippocamp", MountReefCatalog.ConchWhistle);
            CollectionAssert.AreEqual(new[] { MountReefCatalog.ConchWhistle }, MountReefCatalog.ShopWhistles.ToArray());
            MountSpec spec = MountCatalog.FindByWhistle(MountReefCatalog.ConchWhistle);
            Assert.IsNotNull(spec);
            Assert.AreEqual(MountReefCatalog.KirtaHippocamp, spec.MountId);
            Assert.That(spec.ShopPrice, Is.InRange(140, 150), "§5.1 Uncommon band");

            CityShopEntry sale = CityShopCatalog.EntryOf(CityShopCatalog.MergulhoDanel, MountReefCatalog.ConchWhistle);
            Assert.IsNotNull(sale, "Danel sells the Apito de Concha");
            Assert.AreEqual(spec.ShopPrice, sale.Price);
            Assert.IsFalse(sale.Rotating, "a fixed line: the counter never turns it away");
            Assert.AreEqual(1, CityShopCatalog.Shops.Count(s => s.Entries.Any(e => e.ItemId == MountReefCatalog.ConchWhistle)), "only in Kirta");

            ItemDefinition item = ItemDatabase.GetById(MountReefCatalog.ConchWhistle);
            Assert.IsNotNull(item, "asset missing");
            Assert.AreEqual(spec.ShopPrice, item.Value);
            Assert.AreEqual("Apito de Concha", item.DisplayName);
            Assert.IsNotNull(item.Icon, "generated icon (tools/icons/generate_item_icons.py)");

            int n = 0;
            var collection = new MountCollection { GuidFactory = () => "g" + (++n), UtcNow = () => 0L };
            collection.Initialize();
            try
            {
                Assert.IsNotNull(collection.RedeemWhistle(MountReefCatalog.ConchWhistle));
                Assert.IsTrue(collection.Owns(MountReefCatalog.KirtaHippocamp));
            }
            finally
            {
                collection.Shutdown();
            }
        }

        [Test]
        public void ASwimmer_RidesThroughTheReefTideAndTheBog()
        {
            bool aquatic = MountCatalog.FindSpec(MountReefCatalog.KirtaHippocamp).Type == MountType.Aquatico;
            Assert.IsFalse(CostaTideRules.Drenches(CostaTidePhase.High, aquatic), "the reef tide does not drench it");
            Assert.IsFalse(DeltaIslandRules.BogSlows(true, false, aquatic), "the bog does not slow it");
        }

        [Test]
        public void ReefSong_HealsLessThanTheRareAura_AndLightsTheCoral()
        {
            MountSpec spec = MountCatalog.FindSpec(MountReefCatalog.KirtaHippocamp);
            Assert.AreEqual(0.09f, MountReefRules.TotalHeal(spec), 1e-4f, "1.5%/s for 6 s");
            MountSpec aura = MountCatalog.Specs.Single(s => s.AbilityKind == MountAbilityKind.HealAura);
            Assert.Less(spec.Magnitude, aura.Magnitude, "the Uncommon song heals less per second than the aura");
            Assert.Greater(MountReefRules.SongLampRadius, spec.Radius, "the light opens wider than the heal");
            Assert.AreEqual(0f, MountReefRules.TotalHeal(null));
            Assert.Greater(MountReefRules.SongLampColor.r, MountReefRules.SongLampColor.b, "coral, not sea blue");
        }

        [Test]
        public void ArrivalLines_OneForEachCousin_BothLanguages_RememberingTheGrandparents()
        {
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> byKey = MountTexts.Entries.ToDictionary(e => e.Key);
            var keys = new HashSet<string>();
            foreach (CharacterId cousin in System.Enum.GetValues(typeof(CharacterId)))
            {
                string key = MountReefRules.ArrivalLineFor(MountReefCatalog.KirtaHippocamp, cousin);
                Assert.AreEqual(MountReefRules.ArrivalLineKey(cousin), key);
                Assert.IsTrue(keys.Add(key), "one line per cousin");
                Assert.IsTrue(byKey.ContainsKey(key), key);
                Assert.IsFalse(string.IsNullOrEmpty(byKey[key].En), key);
                string pt = byKey[key].PtBR;
                Assert.IsTrue(pt.Contains("Vó Edna") || pt.Contains("Vô Jorge"), key + ": " + pt);
                Assert.IsFalse(pt.Contains("Dona ") || pt.Contains("Seu "), key);
            }

            Assert.IsNull(MountReefRules.ArrivalLineFor(MountReefCatalog.KirtaHippocamp, null), "nobody chosen: no line");
            Assert.IsNull(MountReefRules.ArrivalLineFor(MountCatalog.Pony, CharacterId.Maria), "only the hippocamp");
        }

        [Test]
        public void Texts_BothLanguages_AndTheModel()
        {
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> byKey = MountTexts.Entries.ToDictionary(e => e.Key);
            MountDefinition d = MountCatalog.Get(MountReefCatalog.KirtaHippocamp);
            string whistleName = Rapadura.Gameplay.Content.ContentText.ItemNameKey(MountReefCatalog.ConchWhistle);
            string whistleDesc = Rapadura.Gameplay.Content.ContentText.ItemDescriptionKey(MountReefCatalog.ConchWhistle);
            foreach (string key in new[] { d.DisplayNameKey, d.DescriptionKey, d.LoreKey, d.SourceHintKey, d.Ability.NameKey, d.Ability.DescriptionKey,
                         MountReefTexts.ToastSong, whistleName, whistleDesc })
            {
                Assert.IsTrue(byKey.ContainsKey(key), key);
                Assert.IsFalse(string.IsNullOrEmpty(byKey[key].En), key);
                Assert.IsFalse(string.IsNullOrEmpty(byKey[key].PtBR), key);
            }

            Assert.AreEqual("Hipocampo de Kirta", byKey[d.DisplayNameKey].PtBR);
            Assert.AreEqual("Apito de Concha", byKey[whistleName].PtBR);
            Assert.AreEqual(MountTexts.Entries.Count, MountTexts.Entries.Select(e => e.Key).Distinct().Count(), "no duplicate keys");

            QuadrupedShape shape = MountRecipe.ShapeFor(MountVisualRecipe.KirtaHippocamp);
            Assert.Greater(shape.TailLength, shape.BodyLength * 0.9f, "the fish tail");
            Assert.AreEqual(EnemySwatch.Pink, shape.Mane, "coral fins for a mane");
            Assert.IsNotNull(MountModels.CreateBlueprint(MountVisualRecipe.KirtaHippocamp));
        }
    }
}
