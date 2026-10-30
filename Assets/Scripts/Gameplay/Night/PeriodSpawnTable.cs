using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.Night
{
    /// <summary>
    /// One weighted entry of a region × period spawn table: which enemy asset (<see cref="EnemyId"/> = its name under
    /// Resources/Enemies), in which <see cref="NightForm"/>, and for a species which one. Same weighted-entry idea as
    /// <see cref="BiomeEnemySpawnEntry"/>/<see cref="BiomeWeatherEntry"/>: weights are relative, normalized over the table.
    /// </summary>
    [Serializable]
    public class PeriodSpawnEntry
    {
        [SerializeField] private string _enemyId;
        [SerializeField] [Min(0)] private int _weight = 1;
        [SerializeField] private NightForm _form = NightForm.Normal;
        [SerializeField] private string _speciesId;

        public string EnemyId => _enemyId;
        public int Weight => _weight;
        public NightForm Form => _form;
        public string SpeciesId => _speciesId;

        public PeriodSpawnEntry()
        {
        }

        public PeriodSpawnEntry(string enemyId, int weight, NightForm form = NightForm.Normal, string speciesId = null)
        {
            _enemyId = enemyId;
            _weight = weight;
            _form = form;
            _speciesId = speciesId;
        }

        public static PeriodSpawnEntry Normal(string enemyId, int weight) => new PeriodSpawnEntry(enemyId, weight);

        public static PeriodSpawnEntry Variant(string enemyId, int weight) => new PeriodSpawnEntry(enemyId, weight, NightForm.Variant);

        public static PeriodSpawnEntry Species(NightSpecies species, int weight) =>
            new PeriodSpawnEntry(species.BaseEnemyId, weight, NightForm.Species, species.Id);
    }

    /// <summary>A rolled spawn: the enemy asset to instantiate and how it is dressed for the period.</summary>
    public readonly struct PeriodSpawnPick
    {
        public readonly EnemyDefinition Enemy;
        public readonly NightForm Form;
        public readonly NightSpecies Species;

        /// <summary>Region of the arena (a species visiting from another home region is scaled to it); null = its home.</summary>
        public readonly BiomeType? Region;

        public PeriodSpawnPick(EnemyDefinition enemy, NightForm form, NightSpecies species, BiomeType? region = null)
        {
            Enemy = enemy;
            Form = species == null && form == NightForm.Species ? NightForm.Variant : form;
            Species = Form == NightForm.Species ? species : null;
            Region = region;
        }

        public NightLook Look => NightVariantRules.Resolve(Form, Species, Region);
    }

    /// <summary>
    /// Spawn tables per region (<see cref="BiomeType"/>) per period (<see cref="TimeOfDayPhase"/>) for the arena's
    /// reinforcement waves. Day keeps today's enemies (the regular, non-boss entries of the region's
    /// <see cref="BiomeDefinition"/>, same weights; a test keeps them in sync); dawn and dusk mix in a few night variants;
    /// night is mostly variants plus the playable night species (<see cref="NightVariantRules.PlayableSpecies"/>).
    /// Never a boss (boss fights stay as they are). Built-in data like <see cref="WeatherTable"/>'s defaults; species
    /// have their own definitions (home region's biome table, weight 0) and appear only at night (the fireflies also at
    /// dusk); a species visiting another region (owl in the Mountain, fireflies over the Forest) is scaled to it
    /// (<see cref="PeriodSpawnPick.Region"/>). Herd species (queixadas) come 2–3 at a time.
    /// </summary>
    public static class PeriodSpawnTable
    {
        public const string ForestWolf = "enemy_forest_wolf";
        public const string ForestSpider = "enemy_forest_spider";
        public const string DesertScorpion = "enemy_desert_scorpion";
        public const string MountainGolem = "enemy_mountain_stone_golem";
        public const string CaveBats = "enemy_cave_bat_swarm";

        private static readonly Dictionary<(BiomeType, TimeOfDayPhase), PeriodSpawnEntry[]> Tables =
            new Dictionary<(BiomeType, TimeOfDayPhase), PeriodSpawnEntry[]>
            {
                // -- Floresta --
                { (BiomeType.Forest, TimeOfDayPhase.Day), new[] { PeriodSpawnEntry.Normal(ForestWolf, 5), PeriodSpawnEntry.Normal(ForestSpider, 3) } },
                { (BiomeType.Forest, TimeOfDayPhase.Dawn), new[] { PeriodSpawnEntry.Normal(ForestWolf, 5), PeriodSpawnEntry.Normal(ForestSpider, 3), PeriodSpawnEntry.Variant(ForestWolf, 1) } },
                { (BiomeType.Forest, TimeOfDayPhase.Dusk), new[] { PeriodSpawnEntry.Normal(ForestWolf, 4), PeriodSpawnEntry.Normal(ForestSpider, 3), PeriodSpawnEntry.Variant(ForestWolf, 1), PeriodSpawnEntry.Variant(ForestSpider, 1) } },
                { (BiomeType.Forest, TimeOfDayPhase.Night), new[]
                {
                    PeriodSpawnEntry.Normal(ForestWolf, 2), PeriodSpawnEntry.Normal(ForestSpider, 1), PeriodSpawnEntry.Variant(ForestWolf, 4), PeriodSpawnEntry.Variant(ForestSpider, 3),
                    PeriodSpawnEntry.Species(NightVariantRules.Lobisomem, 1), PeriodSpawnEntry.Species(NightVariantRules.RasgaMortalha, 2),
                    PeriodSpawnEntry.Species(NightVariantRules.Queixada, 2), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 1),
                } },

                // -- Deserto --
                { (BiomeType.Desert, TimeOfDayPhase.Day), new[] { PeriodSpawnEntry.Normal(DesertScorpion, 5) } },
                { (BiomeType.Desert, TimeOfDayPhase.Dawn), new[] { PeriodSpawnEntry.Normal(DesertScorpion, 5), PeriodSpawnEntry.Variant(DesertScorpion, 1) } },
                { (BiomeType.Desert, TimeOfDayPhase.Dusk), new[] { PeriodSpawnEntry.Normal(DesertScorpion, 4), PeriodSpawnEntry.Variant(DesertScorpion, 1), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 1) } },
                { (BiomeType.Desert, TimeOfDayPhase.Night), new[] { PeriodSpawnEntry.Normal(DesertScorpion, 2), PeriodSpawnEntry.Variant(DesertScorpion, 5), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 3) } },

                // -- Montanha --
                { (BiomeType.Mountain, TimeOfDayPhase.Day), new[] { PeriodSpawnEntry.Normal(MountainGolem, 4) } },
                { (BiomeType.Mountain, TimeOfDayPhase.Dawn), new[] { PeriodSpawnEntry.Normal(MountainGolem, 4), PeriodSpawnEntry.Variant(MountainGolem, 1) } },
                { (BiomeType.Mountain, TimeOfDayPhase.Dusk), new[] { PeriodSpawnEntry.Normal(MountainGolem, 4), PeriodSpawnEntry.Variant(MountainGolem, 1) } },
                { (BiomeType.Mountain, TimeOfDayPhase.Night), new[]
                {
                    PeriodSpawnEntry.Normal(MountainGolem, 2), PeriodSpawnEntry.Variant(MountainGolem, 5),
                    PeriodSpawnEntry.Species(NightVariantRules.RasgaMortalha, 2), PeriodSpawnEntry.Species(NightVariantRules.MaeDaLua, 1),
                } },

                // -- Cavernas (always dark to the eye, but the bichos still follow the clock) --
                { (BiomeType.Caves, TimeOfDayPhase.Day), new[] { PeriodSpawnEntry.Normal(CaveBats, 6) } },
                { (BiomeType.Caves, TimeOfDayPhase.Dawn), new[] { PeriodSpawnEntry.Normal(CaveBats, 5), PeriodSpawnEntry.Variant(CaveBats, 1) } },
                { (BiomeType.Caves, TimeOfDayPhase.Dusk), new[] { PeriodSpawnEntry.Normal(CaveBats, 5), PeriodSpawnEntry.Variant(CaveBats, 1) } },
                { (BiomeType.Caves, TimeOfDayPhase.Night), new[] { PeriodSpawnEntry.Normal(CaveBats, 2), PeriodSpawnEntry.Variant(CaveBats, 4), PeriodSpawnEntry.Species(NightVariantRules.MorcegoVampiro, 2) } },
            };

        /// <summary>Entries for <paramref name="region"/> during <paramref name="phase"/> (empty, never null, for an unknown pair).</summary>
        public static IReadOnlyList<PeriodSpawnEntry> GetEntries(BiomeType region, TimeOfDayPhase phase)
        {
            return Tables.TryGetValue((region, phase), out PeriodSpawnEntry[] entries) ? entries : Array.Empty<PeriodSpawnEntry>();
        }

        public static int TotalWeight(IReadOnlyList<PeriodSpawnEntry> entries)
        {
            int total = 0;
            if (entries == null)
            {
                return 0;
            }

            foreach (PeriodSpawnEntry entry in entries)
            {
                total += entry != null ? Mathf.Max(0, entry.Weight) : 0;
            }

            return total;
        }

        /// <summary>Weighted pick of one entry (null when the table is empty or has no weight).</summary>
        public static PeriodSpawnEntry RollEntry(IReadOnlyList<PeriodSpawnEntry> entries, System.Random random, Predicate<PeriodSpawnEntry> allowed = null)
        {
            if (entries == null || random == null)
            {
                return null;
            }

            int total = 0;
            foreach (PeriodSpawnEntry entry in entries)
            {
                if (entry != null && (allowed == null || allowed(entry)))
                {
                    total += Mathf.Max(0, entry.Weight);
                }
            }

            if (total <= 0)
            {
                return null;
            }

            int roll = random.Next(total);
            int cumulative = 0;
            foreach (PeriodSpawnEntry entry in entries)
            {
                if (entry == null || (allowed != null && !allowed(entry)))
                {
                    continue;
                }

                cumulative += Mathf.Max(0, entry.Weight);
                if (roll < cumulative)
                {
                    return entry;
                }
            }

            return null;
        }

        /// <summary>
        /// Rolls a reinforcement wave of <paramref name="count"/> enemies for <paramref name="region"/> during
        /// <paramref name="phase"/>. Each species respects its <see cref="NightSpecies.MaxPerWave"/>; ids that don't
        /// resolve (through <paramref name="lookup"/>, default Resources/Enemies) and bosses are skipped, so the wave can
        /// come out smaller. A herd species (<see cref="NightSpecies.IsPack"/>) brings 2–3 at once and may stretch a
        /// small wave up to its <see cref="NightSpecies.PackMin"/>, never past <see cref="NightDangerRules.MaxWaveSize"/>.
        /// Deterministic for a given <paramref name="random"/>.
        /// </summary>
        public static List<PeriodSpawnPick> RollWave(BiomeType region, TimeOfDayPhase phase, int count, System.Random random,
            Func<string, EnemyDefinition> lookup = null)
        {
            var picks = new List<PeriodSpawnPick>(Mathf.Max(0, count));
            if (random == null || count <= 0)
            {
                return picks;
            }

            lookup ??= id => Resources.Load<EnemyDefinition>($"{World.Map.EncounterPayload.EnemyResourcesFolder}/{id}");
            IReadOnlyList<PeriodSpawnEntry> entries = GetEntries(region, phase);
            var speciesCount = new Dictionary<string, int>();

            bool Allowed(PeriodSpawnEntry entry)
            {
                if (entry.Form != NightForm.Species)
                {
                    return true;
                }

                NightSpecies species = NightVariantRules.FindSpecies(entry.SpeciesId);
                speciesCount.TryGetValue(entry.SpeciesId ?? string.Empty, out int already);
                // A herd only comes if the whole minimum herd still fits in the wave.
                return species != null && already < species.MaxPerWave
                    && (!species.IsPack || (already == 0 && NightDangerRules.MaxWaveSize - picks.Count >= species.PackMin));
            }

            for (int i = 0; i < count; i++)
            {
                PeriodSpawnEntry entry = RollEntry(entries, random, Allowed);
                if (entry == null)
                {
                    break;
                }

                EnemyDefinition enemy = lookup(entry.EnemyId);
                if (enemy == null || enemy.IsBoss)
                {
                    continue;
                }

                NightSpecies species = entry.Form == NightForm.Species ? NightVariantRules.FindSpecies(entry.SpeciesId) : null;
                int members = 1;
                if (species != null)
                {
                    speciesCount.TryGetValue(species.Id, out int already);
                    if (species.IsPack)
                    {
                        int wanted = random.Next(species.PackMin, species.PackMax + 1);
                        int room = Mathf.Max(count - picks.Count, Mathf.Min(species.PackMin, NightDangerRules.MaxWaveSize - picks.Count));
                        members = Mathf.Clamp(Mathf.Min(wanted, species.MaxPerWave - already), 1, Mathf.Max(1, room));
                    }

                    speciesCount[species.Id] = already + members;
                }

                for (int m = 0; m < members; m++)
                {
                    picks.Add(new PeriodSpawnPick(enemy, entry.Form, species, region));
                }

                i += members - 1;
            }

            return picks;
        }
    }
}
