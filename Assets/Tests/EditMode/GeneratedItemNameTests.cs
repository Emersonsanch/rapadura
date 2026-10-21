using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.UI.CharacterSelect;
using Rapadura.UI.Common;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="GeneratedItemName"/> (procedural item names composed per
    /// language at display time, with the stored pt-BR <c>generatedName</c> as the legacy fallback)
    /// and for the pure text rules <see cref="CharacterSelectController"/> re-applies on a language
    /// switch. Affixes use the real <c>affix.*</c> keys from the central table, the base item the
    /// real <c>item.item_iron_sword.name</c> entry from <c>ContentTexts</c>.
    /// </summary>
    public class GeneratedItemNameTests
    {
        private readonly List<Object> _created = new List<Object>();
        private Dictionary<string, ItemAffixDefinition> _affixes;
        private ItemDefinition _sword;
        private LocalizationManager _manager;

        [SetUp]
        public void SetUp()
        {
            _sword = Track(ScriptableObject.CreateInstance<ItemDefinition>());
            SetPrivateField(_sword, "_itemId", "item_iron_sword");
            SetPrivateField(_sword, "_displayName", "Espada de Ferro");
            SetPrivateField(_sword, "_equipSlot", EquipmentSlot.MainHand);

            _affixes = new Dictionary<string, ItemAffixDefinition>
            {
                { "affix_sharp", CreateAffix("affix_sharp", "Afiada", AffixSlotKind.Prefix) },
                { "affix_brutal", CreateAffix("affix_brutal", "Brutal", AffixSlotKind.Prefix) },
                { "affix_of_the_wind", CreateAffix("affix_of_the_wind", "do Vento", AffixSlotKind.Suffix) },
            };

            _manager = new LocalizationManager();
            _manager.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _created)
            {
                Object.DestroyImmediate(obj);
            }

            _created.Clear();
        }

        // ---------------------------------------------------------------
        // Composition per language
        // ---------------------------------------------------------------

        [Test]
        public void Compose_PrefixOnly_FollowsLanguageOrder()
        {
            InventorySlotData slot = Slot("legacy", "affix_sharp");

            Assert.AreEqual("Espada de Ferro Afiada", Compose(slot, LanguageCode.ptBR));
            Assert.AreEqual("Sharp Iron Sword", Compose(slot, LanguageCode.en));
        }

        [Test]
        public void Compose_SuffixOnly_AlwaysAfterBaseName()
        {
            InventorySlotData slot = Slot("legacy", "affix_of_the_wind");

            Assert.AreEqual("Espada de Ferro do Vento", Compose(slot, LanguageCode.ptBR));
            Assert.AreEqual("Iron Sword of the Wind", Compose(slot, LanguageCode.en));
        }

        [Test]
        public void Compose_PrefixesAndSuffix_KeepRollOrderWithinEachGroup()
        {
            // Saved roll order mixes groups on purpose: suffix first, then two prefixes.
            InventorySlotData slot = Slot("legacy", "affix_of_the_wind", "affix_sharp", "affix_brutal");

            Assert.AreEqual("Espada de Ferro Afiada Brutal do Vento", Compose(slot, LanguageCode.ptBR));
            Assert.AreEqual("Sharp Brutal Iron Sword of the Wind", Compose(slot, LanguageCode.en));
        }

        [Test]
        public void Compose_NoAffixes_ShowsLocalizedBaseName()
        {
            // A zero-affix roll (or a plain item) never shows the stored pt-BR name.
            var slot = new InventorySlotData { itemId = "item_iron_sword", quantity = 1, generatedName = "Espada de Ferro", generatedRarity = ItemRarity.Uncommon };

            Assert.AreEqual("Espada de Ferro", Compose(slot, LanguageCode.ptBR));
            Assert.AreEqual("Iron Sword", Compose(slot, LanguageCode.en));
            Assert.AreEqual("Iron Sword", Compose(null, LanguageCode.en));
        }

        [Test]
        public void Compose_WithoutManager_UsesPtBRAssetTextAndOrder()
        {
            InventorySlotData slot = Slot("legacy", "affix_sharp", "affix_of_the_wind");

            Assert.AreEqual("Espada de Ferro Afiada do Vento", GeneratedItemName.Compose(_sword, slot, null, ResolveAffix));
        }

        // ---------------------------------------------------------------
        // Legacy stored-name fallback
        // ---------------------------------------------------------------

        [Test]
        public void Compose_UnresolvableAffixId_FallsBackToStoredName()
        {
            InventorySlotData slot = Slot("Afiada Espada de Ferro Removida", "affix_sharp", "affix_removed_from_game");

            Assert.AreEqual("Afiada Espada de Ferro Removida", Compose(slot, LanguageCode.en));
            Assert.AreEqual("Afiada Espada de Ferro Removida", Compose(slot, LanguageCode.ptBR));
        }

        [Test]
        public void Compose_UnresolvableAffixWithoutStoredName_FallsBackToBaseName()
        {
            InventorySlotData slot = Slot(null, "affix_removed_from_game");

            Assert.AreEqual("Iron Sword", Compose(slot, LanguageCode.en));
        }

        [Test]
        public void Compose_UnknownBaseItem_ShowsStoredName()
        {
            var slot = new InventorySlotData { itemId = "item_gone", quantity = 1, generatedAffixIds = new[] { "affix_sharp" }, generatedName = "Coisa Afiada" };

            Assert.AreEqual("Coisa Afiada", GeneratedItemName.Compose(null, slot, _manager, ResolveAffix));
        }

        [Test]
        public void Tooltip_GeneratedSlot_TitleFollowsLanguage()
        {
            InventorySlotData slot = Slot("Afiada Espada de Ferro", "affix_sharp");

            // TooltipContentBuilder resolves the manager through the ServiceLocator (usually none in
            // EditMode = pt-BR); either way the title is the composed name, not the stored old-order one.
            TooltipContent content = TooltipContentBuilder.ForItem(_sword, slot, resolveAffix: ResolveAffix);

            Assert.AreEqual(GeneratedItemName.Compose(_sword, slot, Rapadura.Gameplay.Content.ContentText.CurrentManager, ResolveAffix), content.Title);
            Assert.AreNotEqual("Afiada Espada de Ferro", content.Title);
        }

        [Test]
        public void Generator_StoresPtBRNameInSameOrderAsComposition()
        {
            GeneratedItemInstance instance = ProceduralItemGenerator.Generate(
                _sword, ItemRarity.Rare, seed: 1, affixPool: new[] { _affixes["affix_sharp"] });

            Assume.That(instance.Affixes.Count, Is.EqualTo(1));
            InventorySlotData slot = Slot(instance.GeneratedName, instance.AffixIds);

            Assert.AreEqual(instance.GeneratedName, GeneratedItemName.Compose(_sword, slot, null, ResolveAffix),
                "a fresh pt-BR save and its composed pt-BR name agree");
        }

        // ---------------------------------------------------------------
        // Character select: text re-applied on LanguageChanged
        // ---------------------------------------------------------------

        [Test]
        public void CharacterSelect_LanguageSwitch_RebuildsHeroTexts()
        {
            PlayableCharacter maithe = CharacterRegistry.Get(CharacterId.Maithe);

            _manager.SetLanguage(LanguageCode.ptBR);
            Assert.AreEqual("Maithe", CharacterSelectController.BuildRosterButtonText(maithe, _manager));
            Assert.AreEqual("Caçadora — DPS à distância", CharacterSelectController.BuildClassRoleText(maithe, _manager));
            Assert.AreEqual(maithe.Lore, CharacterSelectController.BuildLoreText(maithe, _manager));

            _manager.SetLanguage(LanguageCode.en);
            Assert.AreEqual("Huntress — Ranged DPS", CharacterSelectController.BuildClassRoleText(maithe, _manager));
            StringAssert.StartsWith("At 14, Maithe", CharacterSelectController.BuildLoreText(maithe, _manager));

            PlayableCharacter icaro = CharacterRegistry.Get(CharacterId.Icaro);
            Assert.AreEqual("Ícaro", CharacterSelectController.BuildRosterButtonText(icaro, _manager));
        }

        [Test]
        public void CharacterSelect_NoHeroHighlighted_LoreShowsPromptInEachLanguage()
        {
            _manager.SetLanguage(LanguageCode.en);
            Assert.AreEqual("Select a character to see their story.", CharacterSelectController.BuildLoreText(null, _manager));

            _manager.SetLanguage(LanguageCode.ptBR);
            Assert.AreEqual("Selecione um personagem para ver a história.", CharacterSelectController.BuildLoreText(null, _manager));
        }

        [Test]
        public void CharacterSelect_WithoutManager_UsesPtBRContent()
        {
            PlayableCharacter lavine = CharacterRegistry.Get(CharacterId.Lavine);

            Assert.AreEqual($"{lavine.ClassName} — {lavine.Role}", CharacterSelectController.BuildClassRoleText(lavine, null));
            Assert.AreEqual(string.Empty, CharacterSelectController.BuildClassRoleText(null, null));
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private string Compose(InventorySlotData slot, LanguageCode language)
        {
            _manager.SetLanguage(language);
            return GeneratedItemName.Compose(_sword, slot, _manager, ResolveAffix);
        }

        private ItemAffixDefinition ResolveAffix(string id) =>
            _affixes.TryGetValue(id, out ItemAffixDefinition affix) ? affix : null;

        private static InventorySlotData Slot(string storedName, params string[] affixIds)
        {
            return new InventorySlotData
            {
                itemId = "item_iron_sword",
                quantity = 1,
                generatedAffixIds = affixIds,
                generatedAffixValues = new float[affixIds.Length],
                generatedName = storedName,
                generatedRarity = ItemRarity.Rare,
            };
        }

        private ItemAffixDefinition CreateAffix(string affixId, string ptBRFallback, AffixSlotKind slotKind)
        {
            var affix = Track(ScriptableObject.CreateInstance<ItemAffixDefinition>());
            SetPrivateField(affix, "_affixId", affixId);
            SetPrivateField(affix, "_nameLocalizationKey", "affix." + affixId);
            SetPrivateField(affix, "_fallbackDisplayText", ptBRFallback);
            SetPrivateField(affix, "_slotKind", slotKind);
            SetPrivateField(affix, "_requiredEquipSlot", EquipmentSlot.None);
            return affix;
        }

        private T Track<T>(T obj) where T : Object
        {
            _created.Add(obj);
            return obj;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"expected private field '{fieldName}' on {target.GetType().Name}");
            field.SetValue(target, value);
        }
    }
}
