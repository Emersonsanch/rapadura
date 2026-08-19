using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>Data-only per-element resistance entry, editable in the Inspector.</summary>
    [Serializable]
    public class ElementResistanceEntry
    {
        public ElementType element = ElementType.None;

        /// <summary>
        /// Fraction of incoming damage of this element mitigated, in [-1, 1].
        /// 0 = no effect, 1 (or more) = fully immune, negative = weakness (takes extra damage).
        /// </summary>
        [Range(-1f, 1f)]
        public float resistance = 0f;
    }

    /// <summary>
    /// Optional per-character/per-enemy table of elemental resistances and immunities, consulted by
    /// <see cref="DamageCalculator"/> when computing final damage. Common ARPG pattern (Diablo/PoE-style
    /// resistance percentages, capped at 100% = immune) layered on top of the existing flat-defense
    /// mitigation curve rather than replacing it — a target can have both high armor (physical mitigation)
    /// and, separately, elemental resistances/weaknesses.
    /// Attach next to <see cref="Health"/> or <c>PlayerStats</c>; entirely optional — absence means
    /// "no resistance to anything," so existing prefabs without this component are unaffected.
    /// </summary>
    public class ElementResistance : MonoBehaviour
    {
        [SerializeField] private List<ElementResistanceEntry> _resistances = new List<ElementResistanceEntry>();

        /// <summary>Returns the resistance fraction for the given element, or 0 if not configured.</summary>
        public float GetResistance(ElementType element)
        {
            for (int i = 0; i < _resistances.Count; i++)
            {
                if (_resistances[i].element == element)
                {
                    return _resistances[i].resistance;
                }
            }

            return 0f;
        }

        /// <summary>True if resistance to this element is 100% or more (damage fully negated).</summary>
        public bool IsImmuneTo(ElementType element)
        {
            return GetResistance(element) >= 1f;
        }

        /// <summary>Sets (or adds) the resistance value for an element. Used by buffs/gear that grant resistance at runtime.</summary>
        public void SetResistance(ElementType element, float resistance)
        {
            for (int i = 0; i < _resistances.Count; i++)
            {
                if (_resistances[i].element == element)
                {
                    _resistances[i].resistance = resistance;
                    return;
                }
            }

            _resistances.Add(new ElementResistanceEntry { element = element, resistance = resistance });
        }
    }
}
