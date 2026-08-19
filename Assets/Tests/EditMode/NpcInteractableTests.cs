using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.World;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="NpcInteractable"/>: verifies it correctly starts a dialogue
    /// via <see cref="DialogueManager"/> or opens a shop via <see cref="ShopManager"/> depending on
    /// which definition is assigned (dialogue takes priority when both are set), that it resolves
    /// the buyer inventory from a "Player"-tagged GameObject, and that it degrades gracefully
    /// (logs a warning, does not throw) when a required manager isn't registered with the
    /// <see cref="ServiceLocator"/>. Mirrors the setup style of ShopManagerTests/CheckpointManagerTests.
    /// </summary>
    public class NpcInteractableTests
    {
        private GameObject _npcGo;
        private NpcInteractable _npc;

        private GameObject _playerGo;
        private InventoryManager _playerInventory;

        private DialogueManager _dialogueManager;
        private ShopManager _shopManager;

        private ItemDefinition _gold;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();

            _npcGo = new GameObject("Npc");
            _npc = _npcGo.AddComponent<NpcInteractable>();

            _playerGo = new GameObject("Player");
            _playerGo.tag = "Player";
            _playerInventory = _playerGo.AddComponent<InventoryManager>();

            _gold = ScriptableObject.CreateInstance<ItemDefinition>();
            var serialized = new SerializedObject(_gold);
            serialized.FindProperty("_itemId").stringValue = "item_gold";
            serialized.FindProperty("_displayName").stringValue = "item_gold";
            serialized.FindProperty("_type").enumValueIndex = (int)ItemType.Material;
            serialized.FindProperty("_maxStack").intValue = 999;
            serialized.FindProperty("_value").intValue = 1;
            serialized.FindProperty("_weight").floatValue = 0f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var map = new Dictionary<string, ItemDefinition> { { "item_gold", _gold } };
            typeof(ItemDatabase)
                .GetField("_itemsById", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .SetValue(null, map);
        }

        [TearDown]
        public void TearDown()
        {
            if (_dialogueManager != null)
            {
                _dialogueManager.Shutdown();
            }

            if (_shopManager != null)
            {
                _shopManager.Shutdown();
            }

            ServiceLocator.Clear();
            ItemDatabase.Invalidate();

            Object.DestroyImmediate(_npcGo);
            Object.DestroyImmediate(_playerGo);
            Object.DestroyImmediate(_gold);
        }

        private static DialogueDefinition BuildDialogue()
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();
            var nodes = new List<DialogueNode>
            {
                new DialogueNode { Id = "start", SpeakerId = "npc_test", TextKey = "dialogue.test.start" }
            };
            dialogue.SetDataForTests("dialogue_npc_test", "start", nodes);
            return dialogue;
        }

        private static ShopDefinition BuildShop()
        {
            var shop = ScriptableObject.CreateInstance<ShopDefinition>();
            SetPrivateField(shop, "_shopId", "shop_npc_test");
            SetPrivateField(shop, "_displayNameKey", "shop.test.name");
            SetPrivateField(shop, "_currencyItemId", "item_gold");
            SetPrivateField(shop, "_sellBackRate", 0.5f);
            SetPrivateField(shop, "_catalog", new List<ShopInventoryItem>());
            return shop;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            System.Reflection.FieldInfo field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(field, $"expected private field '{fieldName}' on {target.GetType().Name}");
            field.SetValue(target, value);
        }

        private void RegisterDialogueManager()
        {
            _dialogueManager = new DialogueManager();
            _dialogueManager.Initialize();
            ServiceLocator.Register(_dialogueManager);
        }

        private void RegisterShopManager()
        {
            _shopManager = new ShopManager();
            _shopManager.Initialize();
            ServiceLocator.Register(_shopManager);
        }

        [Test]
        public void Interact_WithDialogueAssignedAndManagerRegistered_StartsDialogue()
        {
            RegisterDialogueManager();
            DialogueDefinition dialogue = BuildDialogue();
            _npc.SetInteractionSourceForTests(dialogue, null);

            _npc.Interact();

            Assert.IsTrue(_dialogueManager.IsActive);
            Assert.AreEqual(dialogue, _dialogueManager.ActiveDialogue);
        }

        [Test]
        public void Interact_WithShopAssignedAndManagersRegistered_OpensShop()
        {
            RegisterShopManager();
            ShopDefinition shop = BuildShop();
            _npc.SetInteractionSourceForTests(null, shop);

            _npc.Interact();

            Assert.IsTrue(_shopManager.IsOpen);
            Assert.AreEqual(shop, _shopManager.ActiveShop);
            Assert.AreEqual(_playerInventory, _shopManager.BuyerInventory);
        }

        [Test]
        public void Interact_WithBothAssigned_DialogueTakesPriorityOverShop()
        {
            RegisterDialogueManager();
            RegisterShopManager();
            DialogueDefinition dialogue = BuildDialogue();
            ShopDefinition shop = BuildShop();
            _npc.SetInteractionSourceForTests(dialogue, shop);

            _npc.Interact();

            Assert.IsTrue(_dialogueManager.IsActive);
            Assert.IsFalse(_shopManager.IsOpen);
        }

        [Test]
        public void Interact_WithDialogueAssigned_ButNoDialogueManagerRegistered_DoesNotThrow()
        {
            DialogueDefinition dialogue = BuildDialogue();
            _npc.SetInteractionSourceForTests(dialogue, null);

            Assert.DoesNotThrow(() => _npc.Interact());
        }

        [Test]
        public void Interact_WithShopAssigned_ButNoShopManagerRegistered_DoesNotThrow()
        {
            ShopDefinition shop = BuildShop();
            _npc.SetInteractionSourceForTests(null, shop);

            Assert.DoesNotThrow(() => _npc.Interact());
        }

        [Test]
        public void Interact_WithShopAssigned_ButNoPlayerInScene_DoesNotThrowAndDoesNotOpenShop()
        {
            RegisterShopManager();
            Object.DestroyImmediate(_playerGo);
            _playerGo = null;

            ShopDefinition shop = BuildShop();
            _npc.SetInteractionSourceForTests(null, shop);

            Assert.DoesNotThrow(() => _npc.Interact());
            Assert.IsFalse(_shopManager.IsOpen);
        }

        [Test]
        public void Interact_WithNeitherAssigned_DoesNotThrow()
        {
            _npc.SetInteractionSourceForTests(null, null);

            Assert.DoesNotThrow(() => _npc.Interact());
        }

        [Test]
        public void OnTriggerEnter_WithPlayerCollider_CallsInteractAndSetsPlayerInRange()
        {
            RegisterDialogueManager();
            DialogueDefinition dialogue = BuildDialogue();
            _npc.SetInteractionSourceForTests(dialogue, null);

            var collider = _playerGo.AddComponent<BoxCollider>();
            _npc.SendMessage("OnTriggerEnter", collider, SendMessageOptions.DontRequireReceiver);

            Assert.IsTrue(_npc.PlayerInRange);
            Assert.IsTrue(_dialogueManager.IsActive);
        }

        [Test]
        public void OnTriggerExit_WithPlayerCollider_ClearsPlayerInRange()
        {
            var collider = _playerGo.AddComponent<BoxCollider>();
            _npc.SendMessage("OnTriggerEnter", collider, SendMessageOptions.DontRequireReceiver);
            Assert.IsTrue(_npc.PlayerInRange);

            _npc.SendMessage("OnTriggerExit", collider, SendMessageOptions.DontRequireReceiver);

            Assert.IsFalse(_npc.PlayerInRange);
        }
    }
}
