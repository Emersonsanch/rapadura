using Rapadura.Core.Events;
using UnityEngine;

namespace Rapadura.Gameplay.Skills
{
    /// <summary>Raised whenever a caster completes a combo's full skill sequence within its time window.</summary>
    public readonly struct ComboCompletedEvent : IGameEvent
    {
        public readonly GameObject Caster;
        public readonly ComboDefinition Combo;

        public ComboCompletedEvent(GameObject caster, ComboDefinition combo)
        {
            Caster = caster;
            Combo = combo;
        }
    }
}
