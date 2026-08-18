using System.Collections.Generic;

namespace Rapadura.Gameplay.Characters
{
    /// <summary>Static lookup for the fixed roster of playable characters (no runtime state).</summary>
    public static class CharacterRegistry
    {
        private static readonly Dictionary<CharacterId, PlayableCharacter> All = new Dictionary<CharacterId, PlayableCharacter>
        {
            { CharacterId.Joaquim, new Joaquim() },
            { CharacterId.Maria, new Maria() },
            { CharacterId.Maithe, new Maithe() },
            { CharacterId.Icaro, new Icaro() },
            { CharacterId.Lavine, new Lavine() },
        };

        public static PlayableCharacter Get(CharacterId id) => All[id];

        public static IEnumerable<PlayableCharacter> GetAll() => All.Values;
    }
}
