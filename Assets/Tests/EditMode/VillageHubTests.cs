using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Save;
using Rapadura.UI.WorldMap;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// World-map village hub (<see cref="VillageHubButton"/>): when the "Vila" button shows, that the Main.unity
    /// villagers' dialogues are reachable from it, and that shopping/crafting on the map (where only the
    /// carried inventory exists) persists into the next encounter's Player, into save files and across a
    /// save/load (<see cref="CarriedInventorySession"/>).
    /// </summary>
    public class VillageHubTests
    {
        private const int TestSlot = 90097;

        private readonly List<Object> _created = new List<Object>();
        private SaveManager _save;

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
            EventBus.Clear();
            ItemDatabase.Invalidate();
            _save = new SaveManager();
            _save.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
            ItemDatabase.Invalidate();
            TestSaveRoot.Exit();
        }

        // ------------------------------------------------------------------
        // Button / panel rules
        // ------------------------------------------------------------------

        [Test]
        public void Button_ShowsInEveryRegionCity_WhenTheMapIsIdle()
        {
            // Every region has a city (TODO 12.3a): the same hub opens there with that city's services.
            foreach (BiomeType region in new[] { BiomeType.Forest, BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves })
            {
                Assert.IsTrue(VillageHubButton.ShouldShow(true, true, false, false, region), region.ToString());
            }

            Assert.IsFalse(VillageHubButton.ShouldShow(true, true, false, false, null), "no current node");
            Assert.IsFalse(VillageHubButton.ShouldShow(false, true, false, false, BiomeType.Forest), "HUD hidden");
            Assert.IsFalse(VillageHubButton.ShouldShow(true, false, false, false, BiomeType.Forest), "token travelling / encounter");
            Assert.IsFalse(VillageHubButton.ShouldShow(true, true, true, false, BiomeType.Forest), "paused (panel, pause menu...)");
            Assert.IsFalse(VillageHubButton.ShouldShow(true, true, false, true, BiomeType.Forest), "in a conversation");
        }

        [Test]
        public void VillageRegion_IsTheForest_WhereTheStoryPutsTheVillage()
        {
            Assert.AreEqual(BiomeType.Forest, VillageNpcCatalog.VillageRegion);
            Assert.IsTrue(VillageNpcCatalog.IsVillageRegion(BiomeType.Forest));
            Assert.IsFalse(VillageNpcCatalog.IsVillageRegion(null));
        }

        [Test]
        public void ReturnToHub_OnlyWhenNothingElseTookOver()
        {
            Assert.IsTrue(VillageHubButton.ShouldReturnToHub(true, false, false, false, true));
            Assert.IsFalse(VillageHubButton.ShouldReturnToHub(false, false, false, false, true), "not started from the panel");
            Assert.IsFalse(VillageHubButton.ShouldReturnToHub(true, true, false, false, true), "window still open");
            Assert.IsFalse(VillageHubButton.ShouldReturnToHub(true, false, true, false, true), "conversation still running");
            Assert.IsFalse(VillageHubButton.ShouldReturnToHub(true, false, false, true, true), "pause menu took over");
            Assert.IsFalse(VillageHubButton.ShouldReturnToHub(true, false, false, false, false), "HUD hidden");
        }

        [Test]
        public void InventorySession_EndsWhenNoServiceWindowUsesIt()
        {
            Assert.IsTrue(VillageHubButton.ShouldEndInventorySession(true, false, false));
            Assert.IsFalse(VillageHubButton.ShouldEndInventorySession(true, true, false));
            Assert.IsFalse(VillageHubButton.ShouldEndInventorySession(true, false, true));
            Assert.IsFalse(VillageHubButton.ShouldEndInventorySession(false, false, false));
        }

        [Test]
        public void News_WhenAQuestCanBeTakenOrHandedIn()
        {
            Assert.IsTrue(VillageNpcCatalog.HasNews(DialogueQuestState.Available));
            Assert.IsTrue(VillageNpcCatalog.HasNews(DialogueQuestState.ObjectivesComplete));
            Assert.IsFalse(VillageNpcCatalog.HasNews(DialogueQuestState.NotStarted));
            Assert.IsFalse(VillageNpcCatalog.HasNews(DialogueQuestState.Active));
            Assert.IsFalse(VillageNpcCatalog.HasNews(DialogueQuestState.Completed));
        }

        [Test]
        public void NpcLabel_NamesTheVillagerAndTheCousin()
        {
            VillageNpcSpec survivor = VillageNpcCatalog.Find("village_survivor");
            Assert.AreEqual("Sobrevivente da Vila · Joaquim", VillageHubButton.BuildNpcLabel(survivor, false, null));
            Assert.AreEqual("Sobrevivente da Vila · Joaquim (!)", VillageHubButton.BuildNpcLabel(survivor, true, null));
            StringAssert.StartsWith("Ponto de encontro", VillageHubButton.BuildNpcLabel(VillageNpcCatalog.Find("maria_gathering_point"), false, null));
        }

        [Test]
        public void ServiceViews_AreLoadableFromResources()
        {
            Assert.IsNotNull(Resources.Load<VisualTreeAsset>(VillageHubButton.ShopViewResourcePath));
            Assert.IsNotNull(Resources.Load<VisualTreeAsset>(VillageHubButton.CraftingViewResourcePath));
            Assert.IsNotNull(Resources.Load<ShopDefinition>(VillageHubButton.ShopResourcePath));
            Assert.IsNotNull(Resources.Load<VisualTreeAsset>(RegionNpcTalkButton.DialogueViewResourcePath));
        }

        // ------------------------------------------------------------------
        // Villagers
        // ------------------------------------------------------------------

        [Test]
        public void Villagers_OnePersonalStoryPerCousin_PlusTheGatheringPoint()
        {
            Assert.AreEqual(6, VillageNpcCatalog.Npcs.Count);
            Assert.AreEqual(VillageNpcCatalog.Npcs.Count, VillageNpcCatalog.Npcs.Select(n => n.NpcKey).Distinct().Count());

            var personal = VillageNpcCatalog.Npcs.Where(n => n.NpcKey != "maria_gathering_point").Select(n => n.Hero).ToList();
            CollectionAssert.AreEquivalent(System.Enum.GetValues(typeof(Rapadura.Gameplay.Characters.CharacterId)), personal);

            foreach (VillageNpcSpec npc in VillageNpcCatalog.Npcs)
            {
                Assert.IsNotNull(Resources.Load<QuestDefinition>("Quests/" + npc.QuestId), npc.QuestId);
                Assert.IsNotNull(VillageHubTexts.GetFallback(npc.NameKey), $"{npc.NpcKey}: name key {npc.NameKey}");
            }
        }

        [Test]
        public void Villagers_AreTheDialoguesOfMainScenesNpcs()
        {
            string main = File.ReadAllText("Assets/Scenes/Main.unity");
            foreach (VillageNpcSpec npc in VillageNpcCatalog.Npcs)
            {
                string guid = AssetDatabase.AssetPathToGUID("Assets/Resources/" + npc.DialogueResourcePath + ".asset");
                Assert.IsNotEmpty(guid, npc.DialogueResourcePath);
                StringAssert.Contains("_dialogue: {fileID: 11400000, guid: " + guid, main, $"{npc.NpcKey} is not an NpcInteractable dialogue of Main.unity");
            }
        }

        [Test]
        public void Villagers_DialoguesStartThroughTheDialogueManager()
        {
            var dialogue = new DialogueManager();
            dialogue.Initialize();
            try
            {
                foreach (VillageNpcSpec npc in VillageNpcCatalog.Npcs)
                {
                    DialogueDefinition definition = VillageNpcCatalog.LoadDialogue(npc);
                    Assert.IsNotNull(definition, npc.DialogueResourcePath);
                    Assert.AreEqual(npc.DialogueId, definition.DialogueId);

                    dialogue.StartDialogue(definition);
                    Assert.IsTrue(dialogue.IsActive, $"{npc.DialogueId} did not start");
                    Assert.IsNotNull(dialogue.CurrentNode);
                    dialogue.EndDialogue();
                    Assert.IsFalse(dialogue.IsActive);
                }
            }
            finally
            {
                dialogue.Shutdown();
            }
        }

        [Test]
        public void GatheringPoint_CarriesTheConvergenceAgreement()
        {
            DialogueDefinition gathering = VillageNpcCatalog.LoadDialogue(VillageNpcCatalog.Find("maria_gathering_point"));
            bool sets = gathering.Nodes.Any(node =>
                node.OnEnterActions.Any(a => a?.Key == "convergence_five_heroes_agreed") ||
                node.Choices.Any(c => c.Actions.Any(a => a?.Key == "convergence_five_heroes_agreed")));
            Assert.IsTrue(sets, "the convergence quest's CustomFlag is set from the gathering point");
        }

        // ------------------------------------------------------------------
        // Inventory on the map
        // ------------------------------------------------------------------

        /// <summary>The previous encounter's Player: holds <paramref name="items"/>, then unloads (its inventory becomes carried).</summary>
        private void CarryInventory(params (string ItemId, int Quantity)[] items)
        {
            var go = new GameObject("PreviousScenePlayer");
            _created.Add(go);
            var inventory = go.AddComponent<InventoryManager>();
            _save.Register(inventory);
            foreach ((string itemId, int quantity) in items)
            {
                Assert.AreEqual(0, inventory.AddItem(Item(itemId), quantity), itemId);
            }

            _save.Unregister(inventory);
            Assert.IsTrue(_save.HasCarriedState(inventory.SaveKey), "precondition: inventory carried");
        }

        private static ItemDefinition Item(string itemId)
        {
            ItemDefinition item = ItemDatabase.GetById(itemId);
            Assert.IsNotNull(item, itemId);
            return item;
        }

        /// <summary>Next encounter: a fresh Player registers and GameManager restores the carry into it.</summary>
        private InventoryManager NextEncounterPlayer(SaveManager save)
        {
            var go = new GameObject("NextScenePlayer");
            _created.Add(go);
            var inventory = go.AddComponent<InventoryManager>();
            save.Register(inventory);
            save.RestoreCarriedState();
            return inventory;
        }

        private static (ShopDefinition Shop, int Index) GeneralStoreFiniteEntry()
        {
            var shop = Resources.Load<ShopDefinition>(VillageHubButton.ShopResourcePath);
            Assert.IsNotNull(shop);
            for (int i = 0; i < shop.Catalog.Count; i++)
            {
                ShopInventoryItem entry = shop.Catalog[i];
                if (entry?.Item != null && !entry.HasInfiniteStock && entry.InitialStock > 0)
                {
                    return (shop, i);
                }
            }

            Assert.Inconclusive("The general store has no finite-stock entry.");
            return (shop, -1);
        }

        [Test]
        public void Session_StartsWithTheCarriedInventory_AndConsumesTheCarry()
        {
            CarryInventory(("item_gold", 40), ("item_herb", 3));
            var session = new CarriedInventorySession(_save);

            InventoryManager live = session.Open();

            Assert.AreEqual(40, live.GetTotalCount("item_gold"));
            Assert.AreEqual(3, live.GetTotalCount("item_herb"));
            Assert.IsFalse(_save.HasCarriedState(live.SaveKey), "the live inventory owns the state while open");
            Assert.AreSame(live, session.Open(), "opening twice reuses the live inventory");

            session.Close();
            Assert.IsFalse(session.IsOpen);
            Assert.IsTrue(_save.HasCarriedState("Inventory"), "closing hands it back to the carry");
        }

        [Test]
        public void BuyingOnTheMap_ReachesTheNextEncounter()
        {
            (ShopDefinition shop, int index) = GeneralStoreFiniteEntry();
            ShopInventoryItem entry = shop.Catalog[index];
            CarryInventory((shop.CurrencyItemId, entry.Price + 7));

            var shops = new ShopManager();
            shops.Initialize();
            var session = new CarriedInventorySession(_save);
            try
            {
                shops.OpenShop(shop, session.Open());
                Assert.IsTrue(shops.BuyItem(index, 1), "buy on the map");
                shops.CloseShop();
            }
            finally
            {
                session.Close();
                shops.Shutdown();
            }

            InventoryManager player = NextEncounterPlayer(_save);
            Assert.AreEqual(1, player.GetTotalCount(entry.Item.ItemId), "bought item");
            Assert.AreEqual(7, player.GetTotalCount(shop.CurrencyItemId), "change");
        }

        [Test]
        public void CraftingOnTheMap_ReachesTheNextEncounter()
        {
            var recipe = Resources.Load<RecipeDefinition>("Recipes/recipe_health_potion");
            Assert.IsNotNull(recipe);
            CarryInventory(recipe.Ingredients.Select(i => (i.item.ItemId, i.quantity)).ToArray());

            var crafting = new CraftingManager();
            crafting.Initialize();
            crafting.UnlockRecipe(recipe, 99);
            var session = new CarriedInventorySession(_save);
            try
            {
                Assert.IsTrue(crafting.TryCraft(recipe, session.Open(), VillageHubButton.Station), "craft at the village workbench");
            }
            finally
            {
                session.Close();
                crafting.Shutdown();
            }

            InventoryManager player = NextEncounterPlayer(_save);
            Assert.AreEqual(recipe.ResultQuantity, player.GetTotalCount(recipe.ResultItem.ItemId));
            foreach (RecipeIngredient ingredient in recipe.Ingredients)
            {
                if (ingredient.item != recipe.ResultItem)
                {
                    Assert.AreEqual(0, player.GetTotalCount(ingredient.item.ItemId), ingredient.item.ItemId + " consumed");
                }
            }
        }

        [Test]
        public void SaveOnTheMap_KeepsPurchasesAndShopStock_AcrossALoad()
        {
            (ShopDefinition shop, int index) = GeneralStoreFiniteEntry();
            ShopInventoryItem entry = shop.Catalog[index];
            CarryInventory((shop.CurrencyItemId, entry.Price * 2));

            var shops = new ShopManager();
            shops.Initialize();
            _save.Register(shops);
            var session = new CarriedInventorySession(_save);
            try
            {
                shops.OpenShop(shop, session.Open());
                Assert.IsTrue(shops.BuyItem(index, 1));

                // Saved while the shop is still open: the live inventory is a registered saveable.
                _save.Save(TestSlot);
                SaveData whileOpen = _save.PeekSlot(TestSlot);
                StringAssert.Contains(entry.Item.ItemId, whileOpen.entries.Single(e => e.key == "Inventory").json);

                shops.CloseShop();
            }
            finally
            {
                session.Close();
            }

            // Saved after closing: the inventory is back in the carry and still written to the file.
            _save.Save(TestSlot);
            shops.Shutdown();
            SaveData file = _save.PeekSlot(TestSlot);
            Assert.AreEqual(1, file.entries.Count(e => e.key == "Inventory"));
            StringAssert.Contains(entry.Item.ItemId, file.entries.Single(e => e.key == "Inventory").json);
            Assert.IsTrue(file.entries.Any(e => e.key == ShopManager.Key), "shop stock saved");

            // Load on the map (fresh session): stock restored, the inventory carried to the next live owner.
            var loaded = new SaveManager();
            loaded.Initialize();
            var loadedShops = new ShopManager();
            loadedShops.Initialize();
            loaded.Register(loadedShops);
            try
            {
                Assert.IsTrue(loaded.Load(TestSlot));
                Assert.AreEqual(entry.InitialStock - 1, loadedShops.GetStock(shop, index), "stock after load");

                var mapSession = new CarriedInventorySession(loaded);
                InventoryManager live = mapSession.Open();
                Assert.AreEqual(1, live.GetTotalCount(entry.Item.ItemId), "purchase after load, on the map");
                Assert.AreEqual(entry.Price, live.GetTotalCount(shop.CurrencyItemId));
                mapSession.Close();

                Assert.AreEqual(1, NextEncounterPlayer(loaded).GetTotalCount(entry.Item.ItemId), "purchase after load, next encounter");
            }
            finally
            {
                loadedShops.Shutdown();
            }
        }
    }
}
