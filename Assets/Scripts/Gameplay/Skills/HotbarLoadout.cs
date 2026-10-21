using System;
using System.Collections.Generic;
using Rapadura.Core.Interfaces;

namespace Rapadura.Gameplay.Skills
{
    /// <summary>Serializable snapshot of the hotbar slot → skill id mapping for the save system.</summary>
    [Serializable]
    public class HotbarLoadoutSaveState
    {
        /// <summary>One entry per slot; empty string = empty slot (JsonUtility can't round-trip null strings).</summary>
        public string[] slotSkillIds = Array.Empty<string>();
    }

    /// <summary>
    /// Plain C# (no MonoBehaviour) mapping of HUD hotbar slot → skill id, persisted through the
    /// existing <see cref="Rapadura.Save.SaveManager"/> as its own <see cref="ISaveable"/> slice.
    ///
    /// Stores ids only — never <see cref="SkillInstance"/>s — so it is independent of
    /// <see cref="SkillManager"/>'s own restore order on load (SaveManager applies entries in file
    /// order) and of whether a skill is currently learned; <see cref="SkillHotbar"/> resolves ids to
    /// learned instances at cast/display time and prunes ids that are no longer learned.
    /// </summary>
    public class HotbarLoadout : ISaveable
    {
        public const int DefaultSlotCount = 5;
        public const string DefaultSaveKey = "HotbarLoadout";

        private readonly string[] _slots;

        /// <summary>Raised whenever any slot's assignment changes (assign, clear, fill, prune, restore).</summary>
        public event Action OnChanged;

        public HotbarLoadout(int slotCount = DefaultSlotCount, string saveKey = DefaultSaveKey)
        {
            _slots = new string[Math.Max(1, slotCount)];
            SaveKey = string.IsNullOrEmpty(saveKey) ? DefaultSaveKey : saveKey;
        }

        public string SaveKey { get; }
        public int SlotCount => _slots.Length;

        public bool IsValidSlot(int slotIndex) => slotIndex >= 0 && slotIndex < _slots.Length;

        /// <summary>Skill id assigned to <paramref name="slotIndex"/>, or null when empty/out of range.</summary>
        public string GetSkillId(int slotIndex) => IsValidSlot(slotIndex) ? _slots[slotIndex] : null;

        public bool IsEmpty(int slotIndex) => string.IsNullOrEmpty(GetSkillId(slotIndex));

        /// <summary>Slot currently holding <paramref name="skillId"/>, or -1.</summary>
        public int IndexOf(string skillId)
        {
            if (string.IsNullOrEmpty(skillId))
            {
                return -1;
            }

            return Array.IndexOf(_slots, skillId);
        }

        /// <summary>
        /// Puts <paramref name="skillId"/> into <paramref name="slotIndex"/>. A skill can only occupy
        /// one slot: if it is already in another slot, the two slots swap contents (so dragging a
        /// skill onto an occupied slot never silently drops the skill that was there).
        /// Passing null/empty clears the slot.
        /// </summary>
        public bool Assign(int slotIndex, string skillId)
        {
            if (!IsValidSlot(slotIndex))
            {
                return false;
            }

            if (string.IsNullOrEmpty(skillId))
            {
                Clear(slotIndex);
                return true;
            }

            int existing = IndexOf(skillId);

            if (existing == slotIndex)
            {
                return true;
            }

            if (existing >= 0)
            {
                _slots[existing] = _slots[slotIndex];
            }

            _slots[slotIndex] = skillId;
            OnChanged?.Invoke();
            return true;
        }

        public void Clear(int slotIndex)
        {
            if (!IsValidSlot(slotIndex) || _slots[slotIndex] == null)
            {
                return;
            }

            _slots[slotIndex] = null;
            OnChanged?.Invoke();
        }

        public void ClearAll()
        {
            Array.Clear(_slots, 0, _slots.Length);
            OnChanged?.Invoke();
        }

        /// <summary>
        /// Default-fill: walks <paramref name="candidateSkillIds"/> in order and drops each id not
        /// already on the bar into the next empty slot. Never overwrites an assigned slot, so a
        /// player's manual layout survives. Returns how many slots were filled.
        /// </summary>
        public int FillEmptySlots(IEnumerable<string> candidateSkillIds)
        {
            if (candidateSkillIds == null)
            {
                return 0;
            }

            int filled = 0;
            int nextEmpty = NextEmptySlot(0);

            foreach (string id in candidateSkillIds)
            {
                if (nextEmpty < 0)
                {
                    break;
                }

                if (string.IsNullOrEmpty(id) || IndexOf(id) >= 0)
                {
                    continue;
                }

                _slots[nextEmpty] = id;
                filled++;
                nextEmpty = NextEmptySlot(nextEmpty + 1);
            }

            if (filled > 0)
            {
                OnChanged?.Invoke();
            }

            return filled;
        }

        /// <summary>Clears every slot whose id matches <paramref name="shouldRemove"/> (e.g. no longer learned). Returns how many were cleared.</summary>
        public int RemoveWhere(Predicate<string> shouldRemove)
        {
            if (shouldRemove == null)
            {
                return 0;
            }

            int removed = 0;

            for (int i = 0; i < _slots.Length; i++)
            {
                if (!string.IsNullOrEmpty(_slots[i]) && shouldRemove(_slots[i]))
                {
                    _slots[i] = null;
                    removed++;
                }
            }

            if (removed > 0)
            {
                OnChanged?.Invoke();
            }

            return removed;
        }

        private int NextEmptySlot(int startIndex)
        {
            for (int i = Math.Max(0, startIndex); i < _slots.Length; i++)
            {
                if (string.IsNullOrEmpty(_slots[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        // ---------------------------------------------------------------
        // Persistence
        // ---------------------------------------------------------------

        public object CaptureState()
        {
            var ids = new string[_slots.Length];

            for (int i = 0; i < _slots.Length; i++)
            {
                ids[i] = _slots[i] ?? string.Empty;
            }

            return new HotbarLoadoutSaveState { slotSkillIds = ids };
        }

        public void RestoreState(object state)
        {
            if (state is not HotbarLoadoutSaveState saveState)
            {
                return;
            }

            Array.Clear(_slots, 0, _slots.Length);
            string[] ids = saveState.slotSkillIds ?? Array.Empty<string>();

            // Slot count may differ from the save (e.g. bar widened later): extra saved slots are
            // dropped, missing ones stay empty, and a duplicated id keeps only its first slot.
            for (int i = 0; i < ids.Length && i < _slots.Length; i++)
            {
                if (!string.IsNullOrEmpty(ids[i]) && IndexOf(ids[i]) < 0)
                {
                    _slots[i] = ids[i];
                }
            }

            OnChanged?.Invoke();
        }
    }
}
