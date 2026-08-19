using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rapadura.Gameplay.Items.Procedural
{
    /// <summary>
    /// Static lookup that resolves an affix id back to its <see cref="ItemAffixDefinition"/>
    /// asset. Mirrors <see cref="ItemDatabase"/> exactly: affixes are authored as ScriptableObject
    /// assets under <c>Resources/Affixes</c> and loaded lazily on first access, so
    /// <see cref="ProceduralItemGenerator"/> only needs to store/roll affix ids.
    /// </summary>
    public static class AffixDatabase
    {
        private const string ResourcesFolder = "Affixes";

        private static Dictionary<string, ItemAffixDefinition> _affixesById;

        private static void EnsureLoaded()
        {
            if (_affixesById != null)
            {
                return;
            }

            _affixesById = new Dictionary<string, ItemAffixDefinition>();

            foreach (ItemAffixDefinition affix in Resources.LoadAll<ItemAffixDefinition>(ResourcesFolder))
            {
                if (affix == null || string.IsNullOrEmpty(affix.AffixId))
                {
                    continue;
                }

                _affixesById[affix.AffixId] = affix;
            }
        }

        public static ItemAffixDefinition GetById(string affixId)
        {
            EnsureLoaded();
            return _affixesById.TryGetValue(affixId, out ItemAffixDefinition affix) ? affix : null;
        }

        public static bool TryGetById(string affixId, out ItemAffixDefinition affix)
        {
            EnsureLoaded();
            return _affixesById.TryGetValue(affixId, out affix);
        }

        public static IEnumerable<ItemAffixDefinition> GetAll()
        {
            EnsureLoaded();
            return _affixesById.Values;
        }

        /// <summary>Affixes compatible with the given equip slot (i.e. unrestricted or matching it).</summary>
        public static IEnumerable<ItemAffixDefinition> GetCompatibleWith(EquipmentSlot equipSlot)
        {
            EnsureLoaded();
            return _affixesById.Values.Where(a => a.IsCompatibleWith(equipSlot));
        }

        /// <summary>Forces the database to re-scan Resources/Affixes. Call after generating/removing affixes in the editor.</summary>
        public static void Invalidate()
        {
            _affixesById = null;
        }
    }
}
