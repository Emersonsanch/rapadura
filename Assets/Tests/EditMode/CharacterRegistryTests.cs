using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Tests
{
    public class CharacterRegistryTests
    {
        [Test]
        public void GetAll_ReturnsExactlyFiveCharacters()
        {
            Assert.AreEqual(5, CharacterRegistry.GetAll().Count());
        }

        [TestCase(CharacterId.Joaquim)]
        [TestCase(CharacterId.Maria)]
        [TestCase(CharacterId.Maithe)]
        [TestCase(CharacterId.Icaro)]
        [TestCase(CharacterId.Lavine)]
        public void Get_ReturnsCharacterWithMatchingId(CharacterId id)
        {
            PlayableCharacter character = CharacterRegistry.Get(id);

            Assert.AreEqual(id, character.Id);
        }

        [Test]
        public void EveryCharacter_HasFiveAbilities()
        {
            foreach (PlayableCharacter character in CharacterRegistry.GetAll())
            {
                Assert.AreEqual(5, character.AbilityNames.Length, $"{character.DisplayName} should have 4 active abilities + 1 ultimate.");
            }
        }

        [Test]
        public void EveryCharacter_HasNonEmptyLoreAndMetadata()
        {
            foreach (PlayableCharacter character in CharacterRegistry.GetAll())
            {
                Assert.IsNotEmpty(character.DisplayName);
                Assert.IsNotEmpty(character.ClassName);
                Assert.IsNotEmpty(character.Role);
                Assert.IsNotEmpty(character.PrimaryAttribute);
                Assert.IsNotEmpty(character.Lore);
            }
        }
    }
}
