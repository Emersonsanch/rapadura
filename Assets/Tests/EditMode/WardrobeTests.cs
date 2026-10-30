using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Wardrobe;
using Rapadura.Gameplay.World.Cities;
using Rapadura.UI.Housing;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// H5 "Guarda-roupa" (Housing.md §9): the 234-entry catalog generated from <c>CatalogoRoupas.md</c> (importer
    /// deterministic, committed table up to date, every origin valid), the Coleção (permanent unlock, starters), looks per
    /// cousin and layer (owner-only cousin sets), dyes spent one per channel and the free coconut soap, saved outfits
    /// (3 / 6), the sources (quests, achievements, relic, boss first win, loot default look, owed from the house, shop
    /// vouchers), the save round trip and the v1 visuals (tint + bone attachments, nothing about stats).
    /// </summary>
    public class WardrobeTests
    {
        private const string MaitheHat = "roupa_maithe_001";
        private const string MaitheChest = "roupa_maithe_002";
        private const string JoaquimChest = "roupa_joaquim_002";
        private const string Dye = "tinta_001";

        private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;

        private static WardrobeService NewWardrobe() => new WardrobeService
        {
            Random = new System.Random(3),
            ActiveCousinProvider = () => CharacterId.Maithe,
            RosewoodProvider = () => false,
        };

        // ------------------------------------------------------------------
        // Catalog
        // ------------------------------------------------------------------

        [Test]
        public void Catalog_Has234Entries_UniqueIds_AndOneSetPerCousinAndRarity()
        {
            Assert.AreEqual(234, WardrobeCatalog.All.Count);
            Assert.AreEqual(WardrobeCatalog.GeneratedEntryCount, WardrobeCatalog.All.Count);
            Assert.AreEqual(WardrobeCatalog.All.Count, WardrobeCatalog.All.Select(e => e.Id).Distinct().Count());
            foreach (CharacterId cousin in Enum.GetValues(typeof(CharacterId)))
            {
                Assert.AreEqual(24, WardrobeCatalog.All.Count(e => e.Kind == WardrobeEntryKind.Piece && e.Owner == cousin), cousin + ": 6 sets of 4");
                foreach (ItemRarity rarity in new[] { ItemRarity.Common, ItemRarity.Uncommon, ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary })
                {
                    foreach (AppearanceSlot slot in new[] { AppearanceSlot.Head, AppearanceSlot.Chest, AppearanceSlot.Legs, AppearanceSlot.Boots })
                    {
                        Assert.IsNotNull(WardrobeCatalog.DefaultLookFor(cousin, slot, rarity), $"{cousin} {slot} {rarity}");
                    }
                }

                Assert.AreEqual(5, WardrobeCatalog.All.Count(e => e.Kind == WardrobeEntryKind.WeaponSkin && e.Owner == cousin));
            }

            Assert.AreEqual(25, WardrobeCatalog.Dyes.Count());
            Assert.IsTrue(WardrobeCatalog.Dyes.All(d => WardrobeVisuals.TryParseHex(d.ColorHex, out _)), "every dye has a colour");
        }

        [Test]
        public void Importer_IsDeterministic_AndTheCommittedTableIsUpToDate()
        {
            string markdown = File.ReadAllText(Path.Combine(ProjectRoot, WardrobeCatalogImport.CatalogMarkdownPath), Encoding.UTF8);
            string englishPath = Path.Combine(ProjectRoot, WardrobeCatalogImport.EnglishNamesPath);
            Dictionary<string, string> english = File.Exists(englishPath) ? HouseCatalogImport.ParseEnglishNames(File.ReadAllText(englishPath, Encoding.UTF8)) : null;
            string first = WardrobeCatalogImport.GenerateCode(WardrobeCatalogImport.Parse(markdown), english);
            Assert.AreEqual(first, WardrobeCatalogImport.GenerateCode(WardrobeCatalogImport.Parse(markdown), english));
            string committed = File.ReadAllText(Path.Combine(ProjectRoot, WardrobeCatalogImport.GeneratedCodePath), Encoding.UTF8).Replace("\r\n", "\n");
            Assert.AreEqual(first, committed, "WardrobeCatalogData.g.cs is stale: run Rapadura ▸ Housing ▸ Import Wardrobe Catalog");
        }

        [Test]
        public void Names_UseTheGrandparentsEdnaAndJorge()
        {
            foreach (WardrobeEntry entry in WardrobeCatalog.All)
            {
                StringAssert.DoesNotContain("Zefa", entry.NamePtBR, entry.Id);
                StringAssert.DoesNotContain("Chico", entry.NamePtBR, entry.Id);
            }
        }

        // ------------------------------------------------------------------
        // Coleção and looks
        // ------------------------------------------------------------------

        [Test]
        public void Starters_AreUnlocked_AndUnlocksArePermanent()
        {
            var book = new WardrobeBook();
            Assert.IsTrue(book.IsUnlocked("roupa_comum_028"), "Pés descalços: from the start");
            Assert.IsFalse(book.IsUnlocked(MaitheHat));
            Assert.IsTrue(book.Grant(MaitheHat));
            Assert.IsFalse(book.Grant(MaitheHat), "already in the Coleção");
            Assert.IsFalse(book.Grant("roupa_que_nao_existe"));
            Assert.IsTrue(book.IsUnlocked(MaitheHat));
        }

        [Test]
        public void Looks_NeedUnlock_SameLayer_AndCousinSetsFitOnlyTheirOwner()
        {
            var book = new WardrobeBook();
            Assert.AreEqual(WardrobeBlock.NotUnlocked, book.SetLook(CharacterId.Maithe, AppearanceSlot.Head, MaitheHat));
            book.Grant(MaitheHat);
            Assert.AreEqual(WardrobeBlock.WrongSlot, book.SetLook(CharacterId.Maithe, AppearanceSlot.Chest, MaitheHat));
            Assert.AreEqual(WardrobeBlock.WrongCousin, book.SetLook(CharacterId.Joaquim, AppearanceSlot.Head, MaitheHat));
            Assert.AreEqual(WardrobeBlock.None, book.SetLook(CharacterId.Maithe, AppearanceSlot.Head, MaitheHat));
            Assert.AreEqual(MaitheHat, book.LookOn(CharacterId.Maithe, AppearanceSlot.Head).Id);
            Assert.IsNull(book.LookOn(CharacterId.Joaquim, AppearanceSlot.Head), "looks are per cousin");

            Assert.AreEqual(WardrobeBlock.None, book.SetLook(CharacterId.Joaquim, AppearanceSlot.Boots, "roupa_comum_028"), "common clothes fit everyone");
            book.SetHidden(CharacterId.Maithe, AppearanceSlot.Head, true);
            Assert.IsNull(book.LookOn(CharacterId.Maithe, AppearanceSlot.Head), "esconder chapéu");
        }

        [Test]
        public void Dyes_AreSpentOnePerChannel_AndTheSoapIsFree()
        {
            var book = new WardrobeBook();
            book.Grant(MaitheChest);
            Assert.AreEqual(WardrobeBlock.NoDyeLeft, book.ApplyDye(CharacterId.Maithe, AppearanceSlot.Chest, DyeChannel.Primary, Dye));
            book.Grant(Dye, 2);
            Assert.AreEqual(2, book.DyeCount(Dye));
            Assert.AreEqual(WardrobeBlock.NoLookToDye, book.ApplyDye(CharacterId.Maithe, AppearanceSlot.Chest, DyeChannel.Primary, Dye), "paint a look, not the bare model");
            Assert.AreEqual(WardrobeBlock.NotADye, book.ApplyDye(CharacterId.Maithe, AppearanceSlot.Chest, DyeChannel.Primary, MaitheChest));
            book.SetLook(CharacterId.Maithe, AppearanceSlot.Chest, MaitheChest);
            Assert.AreEqual(WardrobeBlock.None, book.ApplyDye(CharacterId.Maithe, AppearanceSlot.Chest, DyeChannel.Primary, Dye));
            Assert.AreEqual(WardrobeBlock.None, book.ApplyDye(CharacterId.Maithe, AppearanceSlot.Chest, DyeChannel.Detail, Dye));
            Assert.AreEqual(0, book.DyeCount(Dye), "one dye per channel, spent");
            Assert.AreEqual(Dye, book.Choice(CharacterId.Maithe, AppearanceSlot.Chest).dyeDetail);
            book.ClearDye(CharacterId.Maithe, AppearanceSlot.Chest, DyeChannel.Primary);
            Assert.AreEqual(string.Empty, book.Choice(CharacterId.Maithe, AppearanceSlot.Chest).dyePrimary);
            Assert.AreEqual(0, book.DyeCount(Dye), "the soap does not refund");
        }

        [Test]
        public void Outfits_SaveAndWear_ThreeSlots_OrSixWithTheRosewoodWardrobe()
        {
            var book = new WardrobeBook();
            book.Grant(MaitheHat);
            book.Grant(Dye);
            book.SetLook(CharacterId.Maithe, AppearanceSlot.Head, MaitheHat);
            book.ApplyDye(CharacterId.Maithe, AppearanceSlot.Head, DyeChannel.Primary, Dye);
            Assert.AreEqual(3, WardrobeBook.OutfitSlots(false));
            Assert.AreEqual(6, WardrobeBook.OutfitSlots(true));
            Assert.AreEqual(WardrobeBlock.OutfitSlotLocked, book.SaveOutfit(CharacterId.Maithe, 3, "x", WardrobeBook.OutfitSlots(false)));
            Assert.AreEqual(WardrobeBlock.None, book.SaveOutfit(CharacterId.Maithe, 0, "Festa", 3));

            book.ClearLook(CharacterId.Maithe, AppearanceSlot.Head);
            Assert.IsNull(book.LookOn(CharacterId.Maithe, AppearanceSlot.Head));
            Assert.AreEqual(WardrobeBlock.EmptyOutfit, book.WearOutfit(CharacterId.Maithe, 1, 3));
            Assert.AreEqual(WardrobeBlock.None, book.WearOutfit(CharacterId.Maithe, 0, 3));
            Assert.AreEqual(MaitheHat, book.LookOn(CharacterId.Maithe, AppearanceSlot.Head).Id);
            Assert.AreEqual(Dye, book.Choice(CharacterId.Maithe, AppearanceSlot.Head).dyePrimary, "the outfit keeps the colours already paid");
            Assert.AreEqual(0, book.DyeCount(Dye), "wearing it spends nothing");
        }

        [Test]
        public void Save_RoundTripsThroughJsonUtility_AndDropsUnknownIds()
        {
            var book = new WardrobeBook();
            book.Grant(MaitheHat);
            book.Grant(Dye, 3);
            book.SetLook(CharacterId.Maithe, AppearanceSlot.Head, MaitheHat);
            book.ApplyDye(CharacterId.Maithe, AppearanceSlot.Head, DyeChannel.Detail, Dye);
            book.SaveOutfit(CharacterId.Maithe, 2, "Roça", 3);
            book.Claim("quest:x");
            WardrobeSaveState state = book.CaptureState();
            state.unlocked = state.unlocked.Concat(new[] { "roupa_removida_999" }).ToArray();
            string json = JsonUtility.ToJson(state);

            var loaded = new WardrobeBook();
            loaded.RestoreState(JsonUtility.FromJson<WardrobeSaveState>(json));
            Assert.IsTrue(loaded.IsUnlocked(MaitheHat));
            Assert.IsFalse(loaded.IsUnlocked("roupa_removida_999"));
            Assert.IsTrue(loaded.IsUnlocked("roupa_comum_028"), "starters always");
            Assert.AreEqual(2, loaded.DyeCount(Dye));
            Assert.AreEqual(Dye, loaded.Choice(CharacterId.Maithe, AppearanceSlot.Head).dyeDetail);
            Assert.AreEqual("Roça", loaded.Outfit(CharacterId.Maithe, 2).name);
            Assert.IsTrue(loaded.IsClaimed("quest:x"));
        }

        // ------------------------------------------------------------------
        // Sources
        // ------------------------------------------------------------------

        [Test]
        public void QuestAchievementAndRelic_UnlockTheirEntriesOnce()
        {
            WardrobeService wardrobe = NewWardrobe();
            CollectionAssert.Contains(wardrobe.OnQuestCompleted("quest_maithe_windtrail"), "roupa_maithe_013");
            Assert.IsEmpty(wardrobe.OnQuestCompleted("quest_maithe_windtrail"));
            CollectionAssert.Contains(wardrobe.OnAchievementUnlocked("ach_hero_maithe"), "roupa_maithe_017");
            CollectionAssert.Contains(wardrobe.OnRelicCollected("relic_chapeu_couro"), "acess_002");
            CollectionAssert.Contains(wardrobe.OnQuestCompleted("quest_daily_maithe_wolf_patrol"), "roupa_maithe_012", "a daily's look, the first time");
        }

        [Test]
        public void BossFirstWin_GuaranteesALook_ThenOnlyMissingOnesCanDrop()
        {
            WardrobeService wardrobe = NewWardrobe();
            string first = wardrobe.OnEnemyDefeated("enemy_cave_dweller", true);
            Assert.IsNotNull(first, "first win always gives one");
            List<WardrobeEntry> pool = WardrobeSourceRules.DropPool("enemy_cave_dweller", true);
            Assert.IsTrue(pool.Any(e => e.Id == first));
            for (int i = 0; i < 200; i++) wardrobe.OnEnemyDefeated("enemy_cave_dweller", true);
            Assert.IsTrue(pool.Where(e => e.IsLook).All(e => wardrobe.Book.IsUnlocked(e.Id)), "the whole boss group comes in time");

            Assert.IsNull(WardrobeSourceRules.RollDrop("enemy_forest_wolf", false, false, _ => false, new System.Random(1), new WardrobeEntry[0]));
            Assert.IsTrue(WardrobeSourceRules.DropPool("enemy_desert_scorpion", false).Any(e => e.SourceRef == WardrobeEntry.CommonDropRef));
            Assert.IsFalse(WardrobeSourceRules.DropPool("enemy_cave_bat_swarm", false).Any(e => e.SourceRef == WardrobeEntry.CommonDropRef), "the shared group is Floresta/Deserto only");
        }

        [Test]
        public void LootedGear_UnlocksTheCousinsDefaultLook_WithoutTouchingStats()
        {
            WardrobeService wardrobe = NewWardrobe();
            Assert.AreEqual("roupa_maithe_010", wardrobe.OnEquipmentLooted(CharacterId.Maithe, EquipmentSlot.Chest, ItemRarity.Rare), "§9.3 example");
            Assert.IsNull(wardrobe.OnEquipmentLooted(CharacterId.Maithe, EquipmentSlot.MainHand, ItemRarity.Rare), "weapons have no default look");
            Assert.IsNull(wardrobe.OnEquipmentLooted(CharacterId.Maithe, EquipmentSlot.Chest, ItemRarity.Rare), "already in the Coleção");
            Assert.IsFalse(typeof(WardrobeBook).GetMethods().Any(m => m.GetParameters().Any(p => p.ParameterType.Namespace != null
                    && (p.ParameterType.Namespace.StartsWith("Rapadura.Gameplay.Inventory") || p.ParameterType.Namespace.StartsWith("Rapadura.Gameplay.Stats")
                        || p.ParameterType.Namespace.StartsWith("Rapadura.Gameplay.Player")))),
                "the wardrobe never sees the equipment or the stats");
        }

        [Test]
        public void HouseOwedRewards_AreClaimedByTheWardrobe()
        {
            var house = new HouseService(() => 0);
            house.OweWardrobe("roupa_comum_010");
            house.OweWardrobe("tinta_006");
            WardrobeService wardrobe = NewWardrobe();
            Assert.AreEqual(2, wardrobe.ClaimOwedFromHouse(house));
            Assert.IsTrue(wardrobe.Book.IsUnlocked("roupa_comum_010"));
            Assert.AreEqual(1, wardrobe.Book.DyeCount("tinta_006"));
            Assert.AreEqual(0, house.OwedWardrobe.Count);
            Assert.AreEqual(0, wardrobe.ClaimOwedFromHouse(house));
        }

        [Test]
        public void ShopBridge_SellsLockedLooksAndAlwaysDyes_AndABuyUnlocks()
        {
            WardrobeService wardrobe = NewWardrobe();
            CityShopEntry line = CityShopCatalog.Get(CityShopCatalog.CostureiraFatima).Entries.First(e => e.Kind == CityShopEntryKind.Outfit);
            Assert.IsTrue(WardrobeShopBridge.IsOutfitLine(CityShopCatalog.CostureiraFatima, line.ItemId));
            ItemDefinition voucher = WardrobeShopBridge.Voucher(line.ItemId, wardrobe);
            Assert.IsNotNull(voucher);
            Assert.AreEqual(line.ItemId, voucher.ItemId);
            wardrobe.OnOutfitBought(line.ItemId, 1, null);
            Assert.IsTrue(wardrobe.Book.IsUnlocked(line.ItemId));
            Assert.IsNull(WardrobeShopBridge.Voucher(line.ItemId, wardrobe), "an owned look leaves the counter");
            Assert.IsTrue(WardrobeShopBridge.IsSellable(Dye, wardrobe));
            wardrobe.OnOutfitBought(Dye, 2, null);
            Assert.AreEqual(2, wardrobe.Book.DyeCount(Dye));
        }

        [Test]
        public void EveryOutfitLineOfTheShops_IsAWardrobeEntry()
        {
            foreach (CityShopSpec shop in CityShopCatalog.Shops)
            {
                foreach (CityShopEntry entry in shop.Entries.Where(e => e.Kind == CityShopEntryKind.Outfit))
                {
                    Assert.IsTrue(WardrobeCatalog.Contains(entry.ItemId), $"{shop.ShopId}: {entry.ItemId}");
                }
            }
        }

        // ------------------------------------------------------------------
        // Visuals and UI helpers
        // ------------------------------------------------------------------

        [Test]
        public void Visuals_TintTheBody_AndAttachAHat_ThatHideRemoves()
        {
            var root = new GameObject("hero");
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.transform.SetParent(root.transform, false);
            try
            {
                var book = new WardrobeBook();
                book.Grant(MaitheHat);
                book.Grant(MaitheChest);
                book.SetLook(CharacterId.Maithe, AppearanceSlot.Head, MaitheHat);
                book.SetLook(CharacterId.Maithe, AppearanceSlot.Chest, MaitheChest);
                WardrobeVisuals.Apply(root, CharacterId.Maithe, book);
                Assert.IsTrue(root.GetComponentsInChildren<Transform>().Any(t => t.name.StartsWith(WardrobeVisuals.AttachmentPrefix + "Head")));
                Assert.IsTrue(body.GetComponent<Renderer>().HasPropertyBlock(), "body tinted with a property block");

                book.SetHidden(CharacterId.Maithe, AppearanceSlot.Head, true);
                WardrobeVisuals.Apply(root, CharacterId.Maithe, book);
                Assert.IsFalse(root.GetComponentsInChildren<Transform>().Any(t => t.name.StartsWith(WardrobeVisuals.AttachmentPrefix + "Head")));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void TintAndColours_ArePure()
        {
            Assert.IsTrue(WardrobeVisuals.TryParseHex("2F5DA8", out Color blue));
            Assert.AreEqual(0x2F / 255f, blue.r, 1e-4f);
            Assert.IsFalse(WardrobeVisuals.TryParseHex("xyz", out _));
            Color tinted = WardrobeVisuals.TintedColor(Color.white, Color.black, false);
            Assert.AreEqual(1f - WardrobeVisuals.LookTintStrength, tinted.r, 1e-4f);
            Assert.AreEqual(blue, WardrobeVisuals.ChannelColor(WardrobeCatalog.Get(MaitheChest), Dye, false), "the dye wins over the rarity colour");
        }

        [Test]
        public void PanelCycle_GoesThroughAsIsAndTheUnlockedLooks()
        {
            var options = new List<string> { "a", "b" };
            Assert.AreEqual("a", WardrobePanel.Cycle(options, null, 1));
            Assert.AreEqual("b", WardrobePanel.Cycle(options, "a", 1));
            Assert.IsNull(WardrobePanel.Cycle(options, "b", 1), "back to as-is");
            Assert.AreEqual("b", WardrobePanel.Cycle(options, null, -1));
            var book = new WardrobeBook();
            book.Grant(JoaquimChest);
            Assert.IsFalse(WardrobePanel.Options(book, CharacterId.Maithe, AppearanceSlot.Chest).Contains(JoaquimChest));
            Assert.IsTrue(WardrobePanel.Options(book, CharacterId.Joaquim, AppearanceSlot.Chest).Contains(JoaquimChest));
        }

        [Test]
        public void Texts_CoverEveryBlockAndTheToast()
        {
            foreach (WardrobeBlock block in Enum.GetValues(typeof(WardrobeBlock)))
            {
                string key = WardrobeUiTexts.BlockKey(block);
                if (key != null) Assert.IsNotNull(WardrobeUiTexts.GetFallback(key), block.ToString());
            }

            var toast = HouseGrantToastRelay.ForWardrobe(new WardrobeEntryGrantedEvent(MaitheHat, true), null);
            StringAssert.Contains("Chapéu de palha", toast.Body);
        }
    }
}
