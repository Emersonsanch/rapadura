using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Refining;

namespace Rapadura.Gameplay.Items.Options
{
    /// <summary>What a dropped piece rolled (pure data; online the server rolls it and sends it).</summary>
    public readonly struct ItemOptionRoll
    {
        public readonly int ExcellentMask;
        public readonly bool Luck;
        public readonly string AncientSetId;
        public readonly int SocketCount;

        public ItemOptionRoll(int excellentMask, bool luck, string ancientSetId, int socketCount)
        {
            ExcellentMask = excellentMask;
            Luck = luck;
            AncientSetId = ancientSetId;
            SocketCount = socketCount;
        }

        public bool IsEmpty => ExcellentMask == 0 && !Luck && string.IsNullOrEmpty(AncientSetId) && SocketCount == 0;

        /// <summary>Writes this roll on a slot (sockets start empty: "").</summary>
        public void ApplyTo(InventorySlotData data)
        {
            if (data == null)
            {
                return;
            }

            data.excellentMask = ExcellentMask;
            data.luck = Luck;
            data.ancientSetId = string.IsNullOrEmpty(AncientSetId) ? null : AncientSetId;
            data.sockets = SocketCount > 0 ? new string[SocketCount] : null;
            if (data.sockets != null)
            {
                for (int i = 0; i < data.sockets.Length; i++)
                {
                    data.sockets[i] = string.Empty;
                }
            }
        }
    }

    /// <summary>
    /// Pure rules of the MU-style item options (FASE 16; data in <see cref="ItemOptionCatalog"/>), unit-tested in
    /// <c>ItemOptionTests</c>. Only pieces that can be refined (weapons with damage, armour with defense) get options.
    /// <list type="bullet">
    /// <item><b>Excelente</b> (green lines): 1 line off a common enemy, 1–2 off a boss, from the weapon or armour lines.</item>
    /// <item><b>Com Sorte</b>: a little critical chance, and +5 points on refines that can fail (<see cref="RefineRules.LuckBonus"/>).</item>
    /// <item><b>Conjunto antigo</b>: bosses only, never excellent too; bonus tiers at 2/3/4 pieces worn (they add up).</item>
    /// <item><b>Engaste</b>: 1–2 empty sockets; the ferreiro engraves a material in each (<see cref="EngraveService"/>).</item>
    /// <item>A broken piece (durability 0) gives nothing, options included, and does not count for its set.</item>
    /// </list>
    /// Rolls use a seeded SplitMix64 stream (same on every platform), derived from the piece's own seed, so the loot
    /// roller draws no extra numbers and old seeds reproduce the same drops.
    /// </summary>
    public static class ItemOptionRules
    {
        public static bool CanHaveOptions(ItemDefinition item) => RefineRules.IsRefinable(item);

        public static bool IsWeapon(ItemDefinition item) => item != null && item.WeaponDamage > 0f;

        /// <summary>Seeded uniform [0, 1) stream (SplitMix64), the server's on line.</summary>
        public struct OptionRandom
        {
            private ulong _state;

            public OptionRandom(int seed)
            {
                _state = (ulong)(uint)seed * 0x9E3779B97F4A7C15UL + 0xD1B54A32D192ED03UL;
            }

            public double NextDouble()
            {
                unchecked
                {
                    _state += 0x9E3779B97F4A7C15UL;
                    ulong z = _state;
                    z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                    z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                    z ^= z >> 31;
                    return (z >> 11) * (1.0 / (1UL << 53));
                }
            }

            public int Next(int maxExclusive) => maxExclusive <= 1 ? 0 : Math.Min(maxExclusive - 1, (int)(NextDouble() * maxExclusive));
        }

        /// <summary>
        /// The options of one dropped <paramref name="item"/> (none when it cannot have options). Fixed order of draws:
        /// ancient (boss), excellent, lines, luck, sockets.
        /// </summary>
        public static ItemOptionRoll Roll(ItemDefinition item, bool isBoss, int seed)
        {
            if (!CanHaveOptions(item))
            {
                return default;
            }

            return Roll(IsWeapon(item), isBoss, seed);
        }

        /// <summary><see cref="Roll(ItemDefinition, bool, int)"/> by kind (weapon or armour), without the item asset.</summary>
        public static ItemOptionRoll Roll(bool weapon, bool isBoss, int seed)
        {
            var random = new OptionRandom(seed);
            string ancient = null;
            double ancientRoll = random.NextDouble();
            if (isBoss && ancientRoll < ItemOptionCatalog.AncientChanceBoss)
            {
                ancient = ItemOptionCatalog.AncientSets[random.Next(ItemOptionCatalog.AncientSets.Length)].Id;
            }

            int mask = 0;
            double excellentRoll = random.NextDouble();
            float excellentChance = isBoss ? ItemOptionCatalog.ExcellentChanceBoss : ItemOptionCatalog.ExcellentChanceCommon;
            if (ancient == null && excellentRoll < excellentChance)
            {
                var pool = new List<ExcellentLine>(ItemOptionCatalog.LinesFor(weapon));
                int lines = isBoss ? 1 + random.Next(ItemOptionCatalog.ExcellentLinesBossMax) : 1;
                for (int i = 0; i < lines && pool.Count > 0; i++)
                {
                    int pick = random.Next(pool.Count);
                    mask |= (int)pool[pick].Option;
                    pool.RemoveAt(pick);
                }
            }

            bool luck = random.NextDouble() < (isBoss ? ItemOptionCatalog.LuckChanceBoss : ItemOptionCatalog.LuckChanceCommon);

            int sockets = 0;
            if (random.NextDouble() < (isBoss ? ItemOptionCatalog.SocketChanceBoss : ItemOptionCatalog.SocketChanceCommon))
            {
                sockets = isBoss ? 1 + random.Next(ItemOptionCatalog.MaxSockets) : 1;
            }

            return new ItemOptionRoll(mask, luck, ancient, sockets);
        }

        /// <summary>Seed of the options of a piece rolled with <paramref name="pieceSeed"/> (decorrelated from its affixes).</summary>
        public static int OptionSeed(int pieceSeed) => unchecked(pieceSeed * 31 + 0x5EED0F7);

        /// <summary>The excellent lines set in <paramref name="mask"/> that fit the piece's kind.</summary>
        public static List<ExcellentLine> ExcellentLinesOf(int mask, bool weapon)
        {
            var lines = new List<ExcellentLine>();
            foreach (ExcellentLine line in ItemOptionCatalog.ExcellentLines)
            {
                if (line.Weapon == weapon && (mask & (int)line.Option) != 0)
                {
                    lines.Add(line);
                }
            }

            return lines;
        }

        /// <summary>
        /// Every bonus the piece itself gives (excellent lines + luck + engraved sockets). Empty when it cannot have
        /// options. The ancient set bonus is not here: it depends on the whole equipment (<see cref="SetBonuses"/>).
        /// </summary>
        public static List<OptionModifier> PieceModifiers(ItemDefinition item, InventorySlotData data)
        {
            var mods = new List<OptionModifier>();
            if (data == null || !CanHaveOptions(item))
            {
                return mods;
            }

            foreach (ExcellentLine line in ExcellentLinesOf(data.excellentMask, IsWeapon(item)))
            {
                mods.Add(line.Modifier);
            }

            if (data.luck)
            {
                mods.Add(ItemOptionCatalog.LuckModifier);
            }

            if (data.sockets != null)
            {
                foreach (string socket in data.sockets)
                {
                    Engraving engraving = ItemOptionCatalog.FindEngraving(socket);
                    if (engraving != null)
                    {
                        mods.Add(engraving.Modifier);
                    }
                }
            }

            return mods;
        }

        /// <summary>How many equipped, working pieces of each ancient set (each slot counts once).</summary>
        public static Dictionary<string, int> CountSetPieces(IEnumerable<(ItemDefinition item, InventorySlotData data, bool broken)> equipped)
        {
            var counts = new Dictionary<string, int>();
            if (equipped == null)
            {
                return counts;
            }

            foreach ((ItemDefinition item, InventorySlotData data, bool broken) in equipped)
            {
                if (broken || data == null || !CanHaveOptions(item) || ItemOptionCatalog.FindSet(data.ancientSetId) == null)
                {
                    continue;
                }

                counts.TryGetValue(data.ancientSetId, out int count);
                counts[data.ancientSetId] = count + 1;
            }

            return counts;
        }

        /// <summary>Active tiers of <paramref name="set"/> with <paramref name="pieces"/> worn (tiers add up).</summary>
        public static List<OptionModifier> SetTierModifiers(AncientSet set, int pieces)
        {
            var mods = new List<OptionModifier>();
            if (set == null)
            {
                return mods;
            }

            foreach (AncientTier tier in set.Tiers)
            {
                if (pieces >= tier.Pieces)
                {
                    mods.Add(tier.Modifier);
                }
            }

            return mods;
        }

        /// <summary>Every active ancient set bonus of the whole equipment, in catalog order.</summary>
        public static List<OptionModifier> SetBonuses(IEnumerable<(ItemDefinition item, InventorySlotData data, bool broken)> equipped)
        {
            Dictionary<string, int> counts = CountSetPieces(equipped);
            var mods = new List<OptionModifier>();
            foreach (AncientSet set in ItemOptionCatalog.AncientSets)
            {
                if (counts.TryGetValue(set.Id, out int pieces))
                {
                    mods.AddRange(SetTierModifiers(set, pieces));
                }
            }

            return mods;
        }

        /// <summary>Whether <paramref name="engraving"/> fits <paramref name="item"/>: weapon engravings in weapons, armour ones in armour.</summary>
        public static bool CanEngrave(ItemDefinition item, Engraving engraving) =>
            engraving != null && CanHaveOptions(item) && engraving.Weapon == IsWeapon(item);

        /// <summary>Index of the first empty socket, or -1.</summary>
        public static int FirstEmptySocket(InventorySlotData data)
        {
            if (data?.sockets == null)
            {
                return -1;
            }

            for (int i = 0; i < data.sockets.Length; i++)
            {
                if (string.IsNullOrEmpty(data.sockets[i]))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
