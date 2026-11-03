using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Dungeon;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>What a "Dicas da Vó" line in the bestiary is about (texts in <c>BestiaryTexts</c>, one key per kind).</summary>
    public enum BestiaryTipKind
    {
        /// <summary>Quadruped (wolves): runs at you fast, sidestep the lunge.</summary>
        PackRunner = 0,
        /// <summary>Arthropod (spider, scorpion): low and quick, never stand still.</summary>
        Skitterer = 1,
        /// <summary>Flyer (bats): hovers and swoops, hit it when it dives.</summary>
        Flyer = 2,
        /// <summary>Heavy (golem, guardian, warlord, dweller): slow and strong, punish after the slam.</summary>
        Heavy = 3,
        /// <summary>Seconds of telegraph before each strike (<see cref="BestiaryTip.Value"/>).</summary>
        Telegraph = 4,
        /// <summary>Attack cooldown at or below <see cref="BestiaryCatalog.RapidAttackCooldown"/>: swings back to back.</summary>
        RapidAttacks = 5,
        /// <summary>Has a flee threshold: runs away when hurt.</summary>
        Flees = 6,
        /// <summary>Damage per hit at or above <see cref="BestiaryCatalog.HeavyHitterDamage"/>.</summary>
        HeavyHitter = 7,
        /// <summary>Boss: stuns last only <see cref="BestiaryTip.Value"/> (0..1) of the usual time (<see cref="StunRules"/>).</summary>
        StunResistant = 8,
        /// <summary>[distância]: shoots from up to <see cref="BestiaryTip.Value"/> m (<see cref="EnemyDefinition.IsRanged"/>).</summary>
        Ranged = 9,
    }

    /// <summary>One tip line; <see cref="Value"/> is the number shown in it (seconds, or a 0..1 fraction), 0 when unused.</summary>
    public readonly struct BestiaryTip
    {
        public readonly BestiaryTipKind Kind;
        public readonly float Value;

        public BestiaryTip(BestiaryTipKind kind, float value = 0f)
        {
            Kind = kind;
            Value = value;
        }
    }

    /// <summary>How often a bestiary drop shows up per defeat (from the real drop chances, see <see cref="BestiaryCatalog.ClassifyChance"/>).</summary>
    public enum BestiaryDropFrequency
    {
        Rare = 0,
        Sometimes = 1,
        Common = 2,
        Guaranteed = 3,
    }

    /// <summary>One item an enemy can drop, with its per-defeat chance (at least one, all rolls counted).</summary>
    public sealed class BestiaryDrop
    {
        public readonly ItemDefinition Item;
        public readonly float ChancePerDefeat;
        /// <summary>Rolled as a procedural piece of equipment (rarity + affixes), see <see cref="EnemyLootRoller.IsRolledAsEquipment"/>.</summary>
        public readonly bool IsEquipment;
        /// <summary>The boss's own bonus piece (<see cref="EnemyDefinition.BossBonusDropItem"/>).</summary>
        public readonly bool IsBossBonus;

        public BestiaryDrop(ItemDefinition item, float chancePerDefeat, bool isEquipment, bool isBossBonus = false)
        {
            Item = item;
            ChancePerDefeat = Mathf.Clamp01(chancePerDefeat);
            IsEquipment = isEquipment;
            IsBossBonus = isBossBonus;
        }

        public BestiaryDropFrequency Frequency => BestiaryCatalog.ClassifyChance(ChancePerDefeat);
    }

    /// <summary>
    /// A page of "Caderno da Vó Edna": everything shown about one enemy, derived from its
    /// <see cref="EnemyDefinition"/> and the loot table of its biome (nothing is typed twice by hand;
    /// only the lore and the cousins' one-liners are authored, in <c>BestiaryTexts</c>).
    /// </summary>
    public sealed class BestiaryEntry
    {
        public string Id { get; }
        public EnemyDefinition Definition { get; }
        /// <summary>The biome whose spawn table lists the enemy, or null (enemy not placed anywhere yet).</summary>
        public BiomeType? Region { get; }
        public bool IsBoss { get; }
        public EnemyMotionArchetype Archetype { get; }
        /// <summary>Multiplier on stun durations (<see cref="StunRules.MultiplierFor"/>): 1, or 0.5 for bosses.</summary>
        public float StunMultiplier { get; }
        /// <summary>Telegraph actually used before each strike (<see cref="EnemyMotionMath.ResolveWindup"/>).</summary>
        public float WindupSeconds { get; }
        public float MaxHealth { get; }
        public float AttackDamage { get; }
        public IReadOnlyList<BestiaryTip> Tips { get; }
        public IReadOnlyList<BestiaryDrop> Drops { get; }

        /// <summary>
        /// Id of the dungeon whose guardians this page is about (<c>"ossuario"</c>, <see cref="DungeonRules.OssuaryId"/>), or null
        /// for the open-world enemies. Dungeon pages carry the region the dungeon is entered from in <see cref="Region"/>, are
        /// listed after the regions under the dungeon's own header and never show up as hunting prey.
        /// </summary>
        public string DungeonId { get; }

        public bool IsDungeon => !string.IsNullOrEmpty(DungeonId);

        public BestiaryEntry(string id, EnemyDefinition definition, BiomeType? region, IReadOnlyList<BestiaryTip> tips, IReadOnlyList<BestiaryDrop> drops,
            string dungeonId = null)
        {
            Id = id;
            Definition = definition;
            Region = region;
            DungeonId = string.IsNullOrEmpty(dungeonId) ? null : dungeonId;
            IsBoss = definition != null && definition.IsBoss;
            Archetype = definition != null ? definition.MotionArchetype : EnemyMotionArchetype.Quadruped;
            StunMultiplier = StunRules.MultiplierFor(IsBoss);
            WindupSeconds = definition != null
                ? EnemyMotionMath.ResolveWindup(EnemyMotionProfile.For(Archetype).BaseWindup, definition.AttackCooldown)
                : 0f;
            MaxHealth = definition != null ? definition.MaxHealth : 0f;
            AttackDamage = definition != null ? definition.AttackDamage : 0f;
            Tips = tips ?? Array.Empty<BestiaryTip>();
            Drops = drops ?? Array.Empty<BestiaryDrop>();
        }

        public string LoreKey => BestiaryCatalog.LoreKey(Id);

        public string CommentKey(CharacterId hero) => BestiaryCatalog.CommentKey(Id, hero);
    }

    /// <summary>A dungeon whose guardians get bestiary pages (their assets live outside <c>Resources/Enemies</c>).</summary>
    public sealed class BestiaryDungeonSource
    {
        public readonly string DungeonId;
        /// <summary>Resources folder of the guardians' <see cref="EnemyDefinition"/> assets.</summary>
        public readonly string EnemyFolder;
        /// <summary>Region of the world map the dungeon is entered from.</summary>
        public readonly BiomeType Region;
        /// <summary>Resources path of the BiomeDefinition used as the dungeon's loot table (its drops are listed on the pages).</summary>
        public readonly string LootTablePath;

        public BestiaryDungeonSource(string dungeonId, string enemyFolder, BiomeType region, string lootTablePath)
        {
            DungeonId = dungeonId;
            EnemyFolder = enemyFolder;
            Region = region;
            LootTablePath = lootTablePath;
        }
    }

    /// <summary>
    /// Every page of "Caderno da Vó Edna" (the bestiary), one per <see cref="EnemyDefinition"/> under
    /// <c>Resources/Enemies</c> (bosses included), in campaign order: region (Floresta → Deserto →
    /// Montanha → Cavernas), regular enemies before the region boss, then by name; then the guardians of each
    /// dungeon in <see cref="DungeonSources"/> (Ossuário dos Artífices: <c>Resources/Dungeons/Ossuario/Enemies</c>), with
    /// the drops of the dungeon's own loot table. Pages are keyed by
    /// <see cref="DefeatedEnemyRegistry.GetId"/> (the asset name), the same id the kill events,
    /// achievements and <c>enemy.&lt;id&gt;.name</c> localization use.
    ///
    /// Tips and drops come from real data: archetype (<see cref="EnemyMotionArchetypes"/>), telegraph
    /// (<see cref="EnemyMotionMath.ResolveWindup"/>), attack cooldown/damage, flee threshold, the boss
    /// stun reduction (<see cref="StunRules"/>), the base loot chance (<see cref="BossBonusRules"/>) and
    /// the biome loot table rolled by <see cref="EnemyLootRoller"/>. Static and lazily built like
    /// <see cref="BiomeRegistry"/>; <see cref="SetForTests"/> swaps the enemy list for EditMode tests.
    /// </summary>
    public static class BestiaryCatalog
    {
        /// <summary>Attack cooldown (s) at or below which the "ataca sem parar" tip shows.</summary>
        public const float RapidAttackCooldown = 1f;

        /// <summary>Damage per hit at or above which the "bate forte" tip shows.</summary>
        public const float HeavyHitterDamage = 25f;

        /// <summary>The dungeons whose guardians have pages, in page order.</summary>
        public static readonly IReadOnlyList<BestiaryDungeonSource> DungeonSources = new[]
        {
            new BestiaryDungeonSource(DungeonRules.OssuaryId, DungeonRules.EnemyResourcesFolder, DungeonRules.Region, DungeonRules.LootTableResourcePath),
        };

        private static List<BestiaryEntry> _entries;
        private static Dictionary<string, BestiaryEntry> _byId;
        private static List<string> _ids;

        /// <summary>Localization key of a page's lore paragraph.</summary>
        public static string LoreKey(string enemyId) => string.IsNullOrEmpty(enemyId) ? null : $"bestiary.{enemyId}.lore";

        /// <summary>Localization key of <paramref name="hero"/>'s one-liner on a page (<c>bestiary.&lt;id&gt;.comment.maithe</c>...).</summary>
        public static string CommentKey(string enemyId, CharacterId hero) =>
            string.IsNullOrEmpty(enemyId) ? null : $"bestiary.{enemyId}.comment.{hero.ToString().ToLowerInvariant()}";

        public static IReadOnlyList<BestiaryEntry> Entries
        {
            get
            {
                EnsureBuilt();
                return _entries;
            }
        }

        /// <summary>Every page id, in page order.</summary>
        public static IReadOnlyList<string> Ids
        {
            get
            {
                EnsureBuilt();
                return _ids;
            }
        }

        public static int Count => Entries.Count;

        public static BestiaryEntry Get(string enemyId)
        {
            EnsureBuilt();
            return enemyId != null && _byId.TryGetValue(enemyId, out BestiaryEntry entry) ? entry : null;
        }

        public static bool Contains(string enemyId) => Get(enemyId) != null;

        /// <summary>Forces a re-scan of Resources/Enemies (and of the biomes) on next use.</summary>
        public static void Invalidate()
        {
            _entries = null;
            _byId = null;
            _ids = null;
        }

        /// <summary>EditMode hook: builds the pages from <paramref name="enemies"/> with <paramref name="biomeOf"/> (default: <see cref="BiomeRegistry.FindBiomeOf"/>) until <see cref="Invalidate"/>.</summary>
        public static void SetForTests(IEnumerable<EnemyDefinition> enemies, Func<EnemyDefinition, BiomeDefinition> biomeOf = null)
        {
            SetEntries(BuildAll(enemies, biomeOf ?? BiomeRegistry.FindBiomeOf));
        }

        private static void EnsureBuilt()
        {
            if (_entries != null)
            {
                return;
            }

            List<BestiaryEntry> entries = BuildAll(Resources.LoadAll<EnemyDefinition>(EncounterPayload.EnemyResourcesFolder), BiomeRegistry.FindBiomeOf);
            foreach (BestiaryDungeonSource dungeon in DungeonSources)
            {
                entries.AddRange(BuildDungeonPages(Resources.LoadAll<EnemyDefinition>(dungeon.EnemyFolder), dungeon,
                    Resources.Load<BiomeDefinition>(dungeon.LootTablePath)));
            }

            entries = Distinct(entries);
            entries.Sort(ComparePages);
            SetEntries(entries);
        }

        /// <summary>
        /// The pages of one dungeon's guardians: region = the dungeon's entry region, drops from its loot table
        /// (<paramref name="lootTable"/>, may be null), in page order.
        /// </summary>
        public static List<BestiaryEntry> BuildDungeonPages(IEnumerable<EnemyDefinition> enemies, BestiaryDungeonSource dungeon, BiomeDefinition lootTable)
        {
            var built = new List<BestiaryEntry>();
            if (enemies == null || dungeon == null)
            {
                return built;
            }

            var seen = new HashSet<string>();
            foreach (EnemyDefinition enemy in enemies)
            {
                string id = DefeatedEnemyRegistry.GetId(enemy);
                if (string.IsNullOrEmpty(id) || !seen.Add(id))
                {
                    continue;
                }

                var entry = new BestiaryEntry(id, enemy, dungeon.Region, null, null, dungeon.DungeonId);
                built.Add(new BestiaryEntry(id, enemy, dungeon.Region, BuildTips(entry), BuildDrops(enemy, lootTable), dungeon.DungeonId));
            }

            built.Sort(ComparePages);
            return built;
        }

        /// <summary>The source of a dungeon page (null for open-world pages).</summary>
        public static BestiaryDungeonSource DungeonSourceOf(BestiaryEntry entry)
        {
            if (entry == null || !entry.IsDungeon)
            {
                return null;
            }

            foreach (BestiaryDungeonSource dungeon in DungeonSources)
            {
                if (dungeon.DungeonId == entry.DungeonId)
                {
                    return dungeon;
                }
            }

            return null;
        }

        private static List<BestiaryEntry> Distinct(List<BestiaryEntry> entries)
        {
            var seen = new HashSet<string>();
            var unique = new List<BestiaryEntry>(entries.Count);
            foreach (BestiaryEntry entry in entries)
            {
                if (seen.Add(entry.Id))
                {
                    unique.Add(entry);
                }
            }

            return unique;
        }

        private static void SetEntries(List<BestiaryEntry> entries)
        {
            _entries = entries;
            _byId = new Dictionary<string, BestiaryEntry>();
            _ids = new List<string>();
            foreach (BestiaryEntry entry in entries)
            {
                _byId[entry.Id] = entry;
                _ids.Add(entry.Id);
            }
        }

        // ------------------------------------------------------------------
        // Pure builders (tested)
        // ------------------------------------------------------------------

        /// <summary>One page per distinct definition (first one wins on a duplicate id), in page order.</summary>
        public static List<BestiaryEntry> BuildAll(IEnumerable<EnemyDefinition> enemies, Func<EnemyDefinition, BiomeDefinition> biomeOf)
        {
            var built = new List<BestiaryEntry>();
            var seen = new HashSet<string>();
            if (enemies == null)
            {
                return built;
            }

            foreach (EnemyDefinition enemy in enemies)
            {
                string id = DefeatedEnemyRegistry.GetId(enemy);
                if (string.IsNullOrEmpty(id) || !seen.Add(id))
                {
                    continue;
                }

                built.Add(Build(enemy, biomeOf?.Invoke(enemy)));
            }

            built.Sort(ComparePages);
            return built;
        }

        /// <summary>
        /// Open-world pages by region, then the dungeon pages (by dungeon order), unplaced enemies last; inside a group regular
        /// before boss, then id (stable, language-independent).
        /// </summary>
        public static int ComparePages(BestiaryEntry a, BestiaryEntry b)
        {
            int groupA = GroupRank(a);
            int groupB = GroupRank(b);
            if (groupA != groupB)
            {
                return groupA.CompareTo(groupB);
            }

            if (a.IsBoss != b.IsBoss)
            {
                return a.IsBoss ? 1 : -1;
            }

            return string.CompareOrdinal(a.Id, b.Id);
        }

        /// <summary>Region index for open-world pages, 1000 + dungeon order for dungeon pages, int.MaxValue for unplaced ones.</summary>
        private static int GroupRank(BestiaryEntry entry)
        {
            if (entry.IsDungeon)
            {
                for (int i = 0; i < DungeonSources.Count; i++)
                {
                    if (DungeonSources[i].DungeonId == entry.DungeonId)
                    {
                        return 1000 + i;
                    }
                }

                return 1000 + DungeonSources.Count;
            }

            return entry.Region.HasValue ? (int)entry.Region.Value : int.MaxValue;
        }

        public static BestiaryEntry Build(EnemyDefinition enemy, BiomeDefinition biome)
        {
            string id = DefeatedEnemyRegistry.GetId(enemy);
            var entry = new BestiaryEntry(id, enemy, biome != null ? biome.Type : (BiomeType?)null, null, null);
            return new BestiaryEntry(id, enemy, entry.Region, BuildTips(entry), BuildDrops(enemy, biome));
        }

        /// <summary>Archetype tip first, then telegraph, then whatever the numbers call for; the boss stun line last.</summary>
        public static List<BestiaryTip> BuildTips(BestiaryEntry entry)
        {
            var tips = new List<BestiaryTip>();
            EnemyDefinition enemy = entry?.Definition;
            if (enemy == null)
            {
                return tips;
            }

            tips.Add(new BestiaryTip(ArchetypeTip(entry.Archetype)));
            tips.Add(new BestiaryTip(BestiaryTipKind.Telegraph, entry.WindupSeconds));

            if (enemy.IsRanged)
            {
                tips.Add(new BestiaryTip(BestiaryTipKind.Ranged, enemy.AttackRange));
            }

            if (enemy.AttackCooldown <= RapidAttackCooldown)
            {
                tips.Add(new BestiaryTip(BestiaryTipKind.RapidAttacks, enemy.AttackCooldown));
            }

            if (enemy.AttackDamage >= HeavyHitterDamage)
            {
                tips.Add(new BestiaryTip(BestiaryTipKind.HeavyHitter, enemy.AttackDamage));
            }

            if (enemy.FleeHealthThreshold > 0f)
            {
                tips.Add(new BestiaryTip(BestiaryTipKind.Flees, enemy.FleeHealthThreshold));
            }

            if (entry.StunMultiplier < 1f)
            {
                tips.Add(new BestiaryTip(BestiaryTipKind.StunResistant, entry.StunMultiplier));
            }

            return tips;
        }

        public static BestiaryTipKind ArchetypeTip(EnemyMotionArchetype archetype)
        {
            switch (archetype)
            {
                case EnemyMotionArchetype.Arthropod: return BestiaryTipKind.Skitterer;
                case EnemyMotionArchetype.Flyer: return BestiaryTipKind.Flyer;
                case EnemyMotionArchetype.Heavy: return BestiaryTipKind.Heavy;
                default: return BestiaryTipKind.PackRunner;
            }
        }

        /// <summary>
        /// The base loot item, every item of the biome loot table and the boss bonus piece, each with
        /// its chance of dropping at least once per defeat (same rolls as <see cref="EnemyLootRoller.Roll"/>),
        /// most likely first. Items appearing twice (e.g. in the table and as base loot) are merged.
        /// </summary>
        public static List<BestiaryDrop> BuildDrops(EnemyDefinition enemy, BiomeDefinition biome)
        {
            var chances = new Dictionary<ItemDefinition, float>();
            var order = new List<ItemDefinition>();
            var drops = new List<BestiaryDrop>();
            if (enemy == null)
            {
                return drops;
            }

            void Add(ItemDefinition item, float chance)
            {
                if (item == null || chance <= 0f)
                {
                    return;
                }

                if (chances.TryGetValue(item, out float existing))
                {
                    // Independent sources: P(at least one) = 1 - (1-a)(1-b).
                    chances[item] = 1f - (1f - existing) * (1f - chance);
                }
                else
                {
                    chances[item] = chance;
                    order.Add(item);
                }
            }

            Add(enemy.LootItem, BossBonusRules.GetEffectiveLootDropChance(enemy));

            if (biome != null)
            {
                int totalWeight = 0;
                foreach (BiomeLootEntry entry in biome.Loot)
                {
                    if (entry?.Item != null && entry.Weight > 0)
                    {
                        totalWeight += entry.Weight;
                    }
                }

                int rolls = EnemyLootRoller.GetBiomeRolls(enemy, biome);
                float rollChance = EnemyLootRoller.GetBiomeRollChance(enemy, biome);
                if (totalWeight > 0 && rolls > 0)
                {
                    foreach (BiomeLootEntry entry in biome.Loot)
                    {
                        if (entry?.Item == null || entry.Weight <= 0)
                        {
                            continue;
                        }

                        float perRoll = rollChance * entry.Weight / totalWeight;
                        Add(entry.Item, 1f - Mathf.Pow(1f - perRoll, rolls));
                    }
                }
            }

            foreach (ItemDefinition item in order)
            {
                drops.Add(new BestiaryDrop(item, chances[item], EnemyLootRoller.IsRolledAsEquipment(item)));
            }

            if (BossBonusRules.HasBonusDrop(enemy))
            {
                drops.Add(new BestiaryDrop(enemy.BossBonusDropItem, 1f, isEquipment: true, isBossBonus: true));
            }

            // Most likely first; ties keep the authored order (List.Sort is not stable, so compare indices).
            var index = new Dictionary<BestiaryDrop, int>();
            for (int i = 0; i < drops.Count; i++)
            {
                index[drops[i]] = i;
            }

            drops.Sort((a, b) =>
            {
                int byChance = b.ChancePerDefeat.CompareTo(a.ChancePerDefeat);
                return byChance != 0 ? byChance : index[a].CompareTo(index[b]);
            });
            return drops;
        }

        /// <summary>Per-defeat chance → frequency label: ≥ 99% guaranteed, ≥ 50% common, ≥ 20% sometimes, else rare.</summary>
        public static BestiaryDropFrequency ClassifyChance(float chancePerDefeat)
        {
            if (chancePerDefeat >= 0.99f) return BestiaryDropFrequency.Guaranteed;
            if (chancePerDefeat >= 0.5f) return BestiaryDropFrequency.Common;
            if (chancePerDefeat >= 0.2f) return BestiaryDropFrequency.Sometimes;
            return BestiaryDropFrequency.Rare;
        }
    }
}
