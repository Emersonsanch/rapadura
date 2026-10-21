using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Save;
using Rapadura.UI.Quests;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// Regression tests for the reviewer's batch of gameplay bugs: input acting while paused / over
    /// UI, equipment bonuses replacing each other, overlapping hit-stops leaving permanent slow-mo,
    /// null-source buffs replacing each other across sources, SaveManager not being exception-safe,
    /// PlayerStats restoring before stat bonuses (free heal on scene hops) and the followed quest
    /// surviving a load. (The duplicate-GameManager bug is covered by the PlayMode
    /// WorldMapEncounterSmokeTests.)
    /// </summary>
    public class ReviewRegressionFixesTests
    {
        private const int TestSlot = 90071;

        private readonly List<Object> _created = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
            EventBus.Clear();
            QuestTracking.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            QuestTracking.Reset();
            ItemDatabase.Invalidate();
            AffixDatabase.Invalidate();

            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
            TestSaveRoot.Exit();
        }

        // ---------------------------------------------------------------
        // Bug 2 — input while paused / over UI
        // ---------------------------------------------------------------

        [Test]
        public void GatePresses_WhilePaused_DropsEveryPress()
        {
            bool jump = true, interact = true, attack = true, moveClick = true;

            PlayerInputHandler.GatePresses(ref jump, ref interact, ref attack, ref moveClick, paused: true, pointerOverUi: false);

            Assert.IsFalse(jump);
            Assert.IsFalse(interact);
            Assert.IsFalse(attack);
            Assert.IsFalse(moveClick);
        }

        [Test]
        public void GatePresses_PointerOverUi_DropsOnlyMousePresses()
        {
            bool jump = true, interact = true, attack = true, moveClick = true;

            PlayerInputHandler.GatePresses(ref jump, ref interact, ref attack, ref moveClick, paused: false, pointerOverUi: true);

            Assert.IsTrue(jump);
            Assert.IsTrue(interact);
            Assert.IsFalse(attack, "Clicking a UI button must not swing an attack.");
            Assert.IsFalse(moveClick, "Clicking a UI button must not walk the player.");
        }

        [Test]
        public void GatePresses_InGameplay_KeepsPresses()
        {
            bool jump = true, interact = false, attack = true, moveClick = true;

            PlayerInputHandler.GatePresses(ref jump, ref interact, ref attack, ref moveClick, paused: false, pointerOverUi: false);

            Assert.IsTrue(jump);
            Assert.IsFalse(interact);
            Assert.IsTrue(attack);
            Assert.IsTrue(moveClick);
        }

        [Test]
        public void IsBlockingElement_ButtonInsideDocument_Blocks_TransparentContainerDoesNot()
        {
            var root = new VisualElement();
            var fullScreenContainer = new VisualElement();
            var label = new Label("hp");
            var button = new Button();
            var buttonLabel = new Label("Comprar");
            root.Add(fullScreenContainer);
            fullScreenContainer.Add(label);
            fullScreenContainer.Add(button);
            button.Add(buttonLabel);

            Assert.IsFalse(UiPointerQuery.IsBlockingElement(null, root));
            Assert.IsFalse(UiPointerQuery.IsBlockingElement(root, root), "The document root itself is not UI.");
            Assert.IsFalse(UiPointerQuery.IsBlockingElement(fullScreenContainer, root), "A transparent layout container must not swallow world clicks.");
            Assert.IsFalse(UiPointerQuery.IsBlockingElement(label, root));
            Assert.IsTrue(UiPointerQuery.IsBlockingElement(button, root));
            Assert.IsTrue(UiPointerQuery.IsBlockingElement(buttonLabel, root), "A click on a button's label is a click on the button.");
        }

        // ---------------------------------------------------------------
        // Bug 3 — equipment bonuses wiping each other
        // ---------------------------------------------------------------

        [Test]
        public void BuffController_SameSourceAndStat_DifferentApplication_BothApply()
        {
            BuffController buffs = Track(new GameObject("Buffs")).AddComponent<BuffController>();

            buffs.ApplyEffect(Modifier(StatType.AttackDamage, ModifierApplication.Flat, 5f), "same_source");
            buffs.ApplyEffect(Modifier(StatType.AttackDamage, ModifierApplication.PercentAdditive, 0.5f), "same_source");

            Assert.AreEqual((10f + 5f) * 1.5f, buffs.GetModifiedValue(StatType.AttackDamage, 10f), 0.001f);
        }

        [Test]
        public void EquipItem_BaseModifiersAndAffixOnSameStat_AllStack_UnequipRemovesAll_RestoreReapplies()
        {
            (InventoryManager inventory, BuffController buffs) = CreateInventoryWithBuffs();

            ItemDefinition sword = Track(CreateItem("review_sword", ItemType.Weapon, EquipmentSlot.MainHand));
            SetPrivateField(sword, "_equipStatModifiers", new[]
            {
                Modifier(StatType.AttackDamage, ModifierApplication.Flat, 5f),
                Modifier(StatType.AttackDamage, ModifierApplication.Flat, 3f),
            });
            ItemAffixDefinition affix = Track(CreateAffix("review_affix_brutal", StatType.AttackDamage));
            RegisterItems(sword);
            RegisterAffixes(affix);

            Assert.IsTrue(inventory.AddGeneratedItem(new GeneratedItemInstance(sword, ItemRarity.Epic, new[] { new RolledAffix(affix, 7f) }, "Espada")));
            Assert.IsTrue(inventory.EquipFromSlot(FindSlotIndex(inventory, sword.ItemId)));

            // 10 base + 5 + 3 (base modifiers) + 7 (affix): nothing replaced anything.
            Assert.AreEqual(25f, buffs.GetModifiedValue(StatType.AttackDamage, 10f), 0.001f);

            string json = JsonUtility.ToJson(inventory.CaptureState());
            inventory.RestoreState(JsonUtility.FromJson<InventorySaveState>(json));
            Assert.AreEqual(25f, buffs.GetModifiedValue(StatType.AttackDamage, 10f), 0.001f, "RestoreState must re-apply every base modifier and affix.");

            Assert.IsTrue(inventory.UnequipSlot(EquipmentSlot.MainHand));
            Assert.AreEqual(10f, buffs.GetModifiedValue(StatType.AttackDamage, 10f), 0.001f, "Unequip must remove every equip sub-key.");
        }

        [Test]
        public void Unequip_OneSlot_KeepsAnotherSlotsBonuses()
        {
            BuffController buffs = Track(new GameObject("Buffs")).AddComponent<BuffController>();
            buffs.ApplyEffect(Modifier(StatType.AttackDamage, ModifierApplication.Flat, 5f), InventoryManager.GetEquipBaseSourceKey(EquipmentSlot.MainHand, 0));
            buffs.ApplyEffect(Modifier(StatType.AttackDamage, ModifierApplication.Flat, 2f), InventoryManager.GetEquipAffixSourceKey(EquipmentSlot.Chest, 0));

            buffs.RemoveEffectsBySourcePrefix("equip_" + EquipmentSlot.MainHand + "/");

            Assert.AreEqual(12f, buffs.GetModifiedValue(StatType.AttackDamage, 10f), 0.001f);
        }

        // ---------------------------------------------------------------
        // Bug 4 — overlapping hit-stops
        // ---------------------------------------------------------------

        [Test]
        public void HitStopScaleState_OverlappingFreezes_RestoreTheOriginalScale()
        {
            var state = new HitStopScaleState();

            Assert.AreEqual(0.02f, state.Begin(1f, 0.02f), 0.0001f);
            // Second hit lands mid-freeze: Time.timeScale is 0.02 right now.
            Assert.AreEqual(0.05f, state.Begin(0.02f, 0.05f), 0.0001f);

            Assert.AreEqual(1f, state.End(gameplayPaused: false), 0.0001f, "Overlapping hit-stops left the game in permanent slow motion.");
            Assert.IsFalse(state.IsActive);
        }

        [Test]
        public void HitStopScaleState_EndWhilePaused_KeepsTimeFrozen()
        {
            var state = new HitStopScaleState();
            state.Begin(1f, 0.02f);

            Assert.AreEqual(0f, state.End(gameplayPaused: true));
        }

        [Test]
        public void HitStopScaleState_StartedFromZero_RestoresToOne()
        {
            var state = new HitStopScaleState();
            state.Begin(0f, 0.02f);

            Assert.AreEqual(1f, state.End(gameplayPaused: false));
        }

        // ---------------------------------------------------------------
        // Bug 5 — null-source buffs replacing each other
        // ---------------------------------------------------------------

        [Test]
        public void Consumable_And_SkillBuffs_OnSameStat_Stack()
        {
            (InventoryManager inventory, BuffController buffs) = CreateInventoryWithBuffs();

            ItemDefinition potion = Track(CreateItem("review_potion", ItemType.Consumable, EquipmentSlot.None, maxStack: 5));
            SetPrivateField(potion, "_consumeBuffs", new[] { Timed(StatType.AttackDamage, 4f) });
            RegisterItems(potion);
            inventory.AddItem(potion, 2);

            Assert.IsTrue(inventory.UseItemInSlot(FindSlotIndex(inventory, potion.ItemId)));
            buffs.ApplyEffect(Timed(StatType.AttackDamage, 6f), SkillManager.SelfBuffSourceKey("review_skill"));

            Assert.AreEqual(20f, buffs.GetModifiedValue(StatType.AttackDamage, 10f), 0.001f, "A skill buff must not replace a potion's buff on the same stat.");

            // Drinking the same potion again refreshes its own buff instead of stacking.
            Assert.IsTrue(inventory.UseItemInSlot(FindSlotIndex(inventory, potion.ItemId)));
            Assert.AreEqual(20f, buffs.GetModifiedValue(StatType.AttackDamage, 10f), 0.001f);
        }

        [Test]
        public void SkillSourceKeys_ArePerSkill_AndDebuffsDistinct()
        {
            Assert.AreEqual("skill_fire", SkillManager.SelfBuffSourceKey("fire"));
            Assert.AreEqual("skill_fire_debuff", SkillManager.DebuffSourceKey("fire"));
            Assert.AreEqual("consumable_herb", InventoryManager.GetConsumableSourceKey("herb"));
            Assert.AreNotEqual(SkillManager.SelfBuffSourceKey("a"), SkillManager.SelfBuffSourceKey("b"));
        }

        // ---------------------------------------------------------------
        // Bug 6 — SaveManager exception safety
        // ---------------------------------------------------------------

        [Test]
        public void Load_OneRestoreThrows_OthersStillRestore_AndGameLoadedIsPublished()
        {
            var manager = new SaveManager();
            manager.Initialize();
            var first = new ValueSaveable("first") { Value = 1 };
            var broken = new ValueSaveable("broken") { Value = 2 };
            var last = new ValueSaveable("last") { Value = 3 };
            manager.Register(first);
            manager.Register(broken);
            manager.Register(last);
            manager.Save(TestSlot);

            first.Value = last.Value = 0;
            broken.ThrowOnRestore = true;
            int loadedEvents = 0;
            EventBus.Subscribe<GameLoadedEvent>(_ => loadedEvents++);

            Assert.IsTrue(manager.Load(TestSlot));

            Assert.AreEqual(1, first.Value);
            Assert.AreEqual(3, last.Value, "A saveable after the throwing one was never restored.");
            Assert.AreEqual(1, loadedEvents);
        }

        [Test]
        public void RestoreCarriedState_FailedEntry_StaysCarried_SucceededOneIsDropped()
        {
            var writer = new SaveManager();
            writer.Initialize();
            writer.Register(new ValueSaveable("good") { Value = 7 });
            writer.Register(new ValueSaveable("bad") { Value = 8 });
            writer.Save(TestSlot);

            var manager = new SaveManager();
            manager.Initialize();
            Assert.IsTrue(manager.Load(TestSlot));
            Assert.IsTrue(manager.HasCarriedState("good"));
            Assert.IsTrue(manager.HasCarriedState("bad"));

            var good = new ValueSaveable("good");
            var bad = new ValueSaveable("bad") { ThrowOnRestore = true };
            manager.Register(good);
            manager.Register(bad);

            Assert.AreEqual(1, manager.RestoreCarriedState());
            Assert.AreEqual(7, good.Value);
            Assert.IsFalse(manager.HasCarriedState("good"));
            Assert.IsTrue(manager.HasCarriedState("bad"), "A carried entry was dropped although its restore failed.");
        }

        // ---------------------------------------------------------------
        // Bug 7 — PlayerStats restore order (free heal on scene hops)
        // ---------------------------------------------------------------

        [Test]
        public void RestoreOrders_PlayerStatsRestoresAfterInventoryAndDefaultSaveables()
        {
            GameObject go = Track(new GameObject("Player"));
            int statsOrder = go.AddComponent<PlayerStats>().RestoreOrder;
            int inventoryOrder = go.AddComponent<InventoryManager>().RestoreOrder;

            Assert.Less(inventoryOrder, RestoreOrders.Default, "InventoryManager clears all buffs on restore, so it must run before AttributeSet/SkillManager.");
            Assert.Greater(statsOrder, RestoreOrders.Default);
            Assert.Greater(statsOrder, inventoryOrder);
        }

        [Test]
        public void Load_PlayerStatsWrittenBeforeABonus_DoesNotHealToTheBonusMax()
        {
            GameObject go = Track(new GameObject("Player"));
            PlayerStats stats = go.AddComponent<PlayerStats>();
            BuffController buffs = go.AddComponent<BuffController>();
            var bonus = new MaxHealthBonusSaveable(buffs);
            Assert.Greater(stats.CurrentHealth, 0f); // lazy init (Awake doesn't run in EditMode) before anything is restored

            float baseMax = stats.MaxHealth;
            bonus.Apply();
            float bonusMax = stats.MaxHealth;
            Assert.Greater(bonusMax, baseMax);
            float damaged = baseMax + 10f; // above the bare max, below the bonus max
            stats.RestoreState(new PlayerStatsSaveState { currentHealth = damaged, currentMana = stats.MaxMana, currentStamina = stats.MaxStamina, level = stats.Level });

            var manager = new SaveManager();
            manager.Initialize();
            manager.Register(stats); // written first, like an entry that lands before AttributeSet/Inventory
            manager.Register(bonus);
            manager.Save(TestSlot);

            // Fresh scene: bonuses not re-applied yet.
            buffs.ClearAllEffects();
            Assert.IsTrue(manager.Load(TestSlot));

            Assert.AreEqual(bonusMax, stats.MaxHealth, 0.001f);
            Assert.AreEqual(damaged, stats.CurrentHealth, 0.001f, "Restoring PlayerStats before the stat bonuses healed the player for free.");
        }

        // ---------------------------------------------------------------
        // Bug 8 — followed quest survives a load / new session
        // ---------------------------------------------------------------

        [Test]
        public void QuestTracking_NewSessionReset_ClearsPreferredQuest()
        {
            QuestTracking.EnsureSubscribed();
            QuestTracking.SetPreferred("quest_old_run");

            new NewSessionReset().ResetAll();

            Assert.IsNull(QuestTracking.PreferredQuestId);
        }

        [Test]
        public void QuestTracking_SaveLoad_ClearsPreferredQuest()
        {
            QuestTracking.EnsureSubscribed();
            var manager = new SaveManager { SessionReset = new NewSessionReset() };
            manager.Initialize();
            manager.Register(new ValueSaveable("any") { Value = 1 });
            manager.Save(TestSlot);
            QuestTracking.SetPreferred("quest_old_run");

            Assert.IsTrue(manager.Load(TestSlot));

            Assert.IsNull(QuestTracking.PreferredQuestId);
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        [Serializable]
        private class ValueState
        {
            public int value;
        }

        private class ValueSaveable : ISaveable
        {
            public ValueSaveable(string key) => SaveKey = key;

            public string SaveKey { get; }
            public int Value { get; set; }
            public bool ThrowOnRestore { get; set; }

            public object CaptureState() => new ValueState { value = Value };

            public void RestoreState(object state)
            {
                if (ThrowOnRestore)
                {
                    throw new InvalidOperationException("boom");
                }

                Value = ((ValueState)state).value;
            }
        }

        /// <summary>Stand-in for AttributeSet/InventoryManager: re-applies a MaxHealth bonus when restored (default order).</summary>
        private class MaxHealthBonusSaveable : ISaveable
        {
            private readonly BuffController _buffs;

            public MaxHealthBonusSaveable(BuffController buffs) => _buffs = buffs;

            public string SaveKey => "review_bonus";

            public void Apply() => _buffs.ApplyEffect(Modifier(StatType.MaxHealth, ModifierApplication.Flat, 50f), "review_bonus");

            public object CaptureState() => new ValueState { value = 1 };

            public void RestoreState(object state) => Apply();
        }

        private T Track<T>(T obj) where T : Object
        {
            _created.Add(obj);
            return obj;
        }

        private (InventoryManager, BuffController) CreateInventoryWithBuffs()
        {
            GameObject owner = Track(new GameObject("InventoryOwner"));
            InventoryManager inventory = owner.AddComponent<InventoryManager>();
            BuffController buffs = owner.AddComponent<BuffController>();
            SetPrivateField(inventory, "_buffController", buffs);
            return (inventory, buffs);
        }

        private static StatModifierDefinition Modifier(StatType stat, ModifierApplication application, float value)
        {
            return new StatModifierDefinition
            {
                displayName = "review_" + stat,
                affectedStat = stat,
                application = application,
                value = value,
                durationSeconds = 0f
            };
        }

        private static StatModifierDefinition Timed(StatType stat, float value)
        {
            StatModifierDefinition definition = Modifier(stat, ModifierApplication.Flat, value);
            definition.durationSeconds = 30f;
            return definition;
        }

        private static int FindSlotIndex(InventoryManager inventory, string itemId)
        {
            var slots = inventory.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsEmpty && slots[i].itemId == itemId)
                {
                    return i;
                }
            }

            return -1;
        }

        private static ItemDefinition CreateItem(string itemId, ItemType type, EquipmentSlot equipSlot, int maxStack = 1)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            var serialized = new SerializedObject(item);

            serialized.FindProperty("_itemId").stringValue = itemId;
            serialized.FindProperty("_displayName").stringValue = itemId;
            serialized.FindProperty("_type").enumValueIndex = (int)type;
            serialized.FindProperty("_maxStack").intValue = maxStack;
            serialized.FindProperty("_equipSlot").enumValueIndex = (int)equipSlot;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return item;
        }

        private static ItemAffixDefinition CreateAffix(string affixId, StatType stat)
        {
            var affix = ScriptableObject.CreateInstance<ItemAffixDefinition>();
            var serialized = new SerializedObject(affix);

            serialized.FindProperty("_affixId").stringValue = affixId;
            serialized.FindProperty("_affectedStat").enumValueIndex = (int)stat;
            serialized.FindProperty("_application").enumValueIndex = (int)ModifierApplication.Flat;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return affix;
        }

        private static void RegisterItems(params ItemDefinition[] items)
        {
            FieldInfo field = typeof(ItemDatabase).GetField("_itemsById", BindingFlags.NonPublic | BindingFlags.Static);
            var map = new Dictionary<string, ItemDefinition>();

            foreach (ItemDefinition item in items)
            {
                map[item.ItemId] = item;
            }

            field.SetValue(null, map);
        }

        private static void RegisterAffixes(params ItemAffixDefinition[] affixes)
        {
            FieldInfo field = typeof(AffixDatabase).GetField("_affixesById", BindingFlags.NonPublic | BindingFlags.Static);
            var map = new Dictionary<string, ItemAffixDefinition>();

            foreach (ItemAffixDefinition affix in affixes)
            {
                map[affix.AffixId] = affix;
            }

            field.SetValue(null, map);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"No field '{fieldName}' on {target.GetType().Name}.");
            field.SetValue(target, value);
        }
    }
}
