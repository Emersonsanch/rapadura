using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.World.Map
{
    /// <summary>
    /// Region boss challenge rules ("Bosses mundiais"), plain C# so EditMode tests cover them and
    /// the world map HUD can query them without touching scene objects:
    /// <list type="bullet">
    /// <item>Each region's boss is the first <see cref="EnemyDefinition.IsBoss"/> entry of its
    /// biome spawn table (<see cref="BossFor"/>): alpha wolf, ruin guardian, warlord, cave dweller.</item>
    /// <item>Random encounters never roll a region boss (<see cref="CanRollInNormalEncounter"/>, used
    /// as <see cref="RandomEncounterSystem.EnemyFilter"/>; it also keeps the old
    /// <see cref="DefeatedEnemyRegistry.CanSpawn"/> gate).</item>
    /// <item>After <see cref="WorldMapGraph.GetVictoriesToChallengeBoss"/> victories in the region the
    /// map offers "Enfrentar o chefe" (<see cref="CanChallengeBoss"/>); the fight is built by
    /// <see cref="BuildChallengePayload"/> (boss + 0-2 minions, <see cref="EncounterPayload.isBossChallenge"/>).</item>
    /// <item>Winning it clears the region (<see cref="WorldMapProgress.MarkRegionCleared"/>, done by
    /// <see cref="SceneTransitionService.CompleteEncounter"/>), and only a cleared region lets
    /// <see cref="WorldMapGraph.ApplyUnlocks(WorldMapProgress, Predicate{BiomeType})"/> open the next
    /// one (<see cref="IsUnlockGateSatisfied"/>). The victory threshold still applies on top.</item>
    /// </list>
    /// Backward compatibility: a boss already in <see cref="DefeatedEnemyRegistry"/> (old saves, or a
    /// boss beaten in a pre-challenge random roll) counts as cleared, a region without a boss in its
    /// table is gated by victories only, and regions already unlocked are never re-locked.
    /// </summary>
    public class RegionBossProgress
    {
        public const int DefaultMinMinions = 0;
        public const int DefaultMaxMinions = 2;

        private readonly WorldMapGraph _graph;
        private readonly WorldMapProgress _progress;
        private readonly Func<BiomeType, BiomeDefinition> _biomeLookup;
        private readonly DefeatedEnemyRegistry _defeated;

        public WorldMapProgress Progress => _progress;
        public WorldMapGraph Graph => _graph;

        /// <param name="biomeLookup">Region → biome asset; defaults to <see cref="BiomeRegistry.Get"/>.</param>
        /// <param name="defeated">Optional: lets already-defeated bosses count as cleared and keeps them out of every roll.</param>
        public RegionBossProgress(WorldMapGraph graph, WorldMapProgress progress, Func<BiomeType, BiomeDefinition> biomeLookup = null, DefeatedEnemyRegistry defeated = null)
        {
            _graph = graph ?? WorldMapGraph.CreateDefault();
            _progress = progress;
            _biomeLookup = biomeLookup ?? BiomeRegistry.Get;
            _defeated = defeated;
        }

        /// <summary>The region's boss: the first boss entry (with weight &gt; 0) of its biome spawn table, or null.</summary>
        public EnemyDefinition BossFor(BiomeType region)
        {
            BiomeDefinition biome = _biomeLookup(region);
            if (biome == null || biome.Enemies == null)
            {
                return null;
            }

            foreach (BiomeEnemySpawnEntry entry in biome.Enemies)
            {
                if (entry != null && entry.Enemy != null && entry.Enemy.IsBoss && entry.Weight > 0)
                {
                    return entry.Enemy;
                }
            }

            return null;
        }

        public bool HasBoss(BiomeType region) => BossFor(region) != null;

        /// <summary>True if <paramref name="enemy"/> is some region's challenge boss (so random rolls skip it).</summary>
        public bool IsRegionBoss(EnemyDefinition enemy)
        {
            if (enemy == null || !enemy.IsBoss)
            {
                return false;
            }

            foreach (BiomeType region in (BiomeType[])Enum.GetValues(typeof(BiomeType)))
            {
                if (BossFor(region) == enemy)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsBossDefeated(BiomeType region)
        {
            EnemyDefinition boss = BossFor(region);
            return boss != null && _defeated != null && _defeated.IsDefeated(boss);
        }

        /// <summary>The region's boss challenge was won (persisted flag) or its boss is already in the defeated registry.</summary>
        public bool IsRegionCleared(BiomeType region)
        {
            return (_progress != null && _progress.IsRegionCleared(region)) || IsBossDefeated(region);
        }

        public int VictoriesToChallenge(BiomeType region) => _graph.GetVictoriesToChallengeBoss(region);

        /// <summary>"Enfrentar o chefe" is offered: region unlocked, has a living boss, not cleared, enough victories won there.</summary>
        public bool CanChallengeBoss(BiomeType region)
        {
            return _progress != null
                   && _progress.IsUnlocked(region)
                   && HasBoss(region)
                   && !IsRegionCleared(region)
                   && _progress.GetVictories(region) >= VictoriesToChallenge(region);
        }

        /// <summary>
        /// Gate for <see cref="WorldMapGraph.ApplyUnlocks(WorldMapProgress, Predicate{BiomeType})"/>:
        /// a required region with a boss must be cleared; one without a boss only needs its victories.
        /// </summary>
        public bool IsUnlockGateSatisfied(BiomeType requiredRegion)
        {
            return !HasBoss(requiredRegion) || IsRegionCleared(requiredRegion);
        }

        /// <summary>Random encounter filter: no region bosses (they are fought only via the challenge) and no defeated unique enemies.</summary>
        public bool CanRollInNormalEncounter(EnemyDefinition enemy)
        {
            if (enemy == null)
            {
                return false;
            }

            if (_defeated != null && !_defeated.CanSpawn(enemy))
            {
                return false;
            }

            return !IsRegionBoss(enemy);
        }

        /// <summary>
        /// Copies registry kills into the persisted cleared flags (legacy saves whose boss was beaten
        /// in a random roll), so the save carries the region's state on its own from now on.
        /// Returns the regions newly marked.
        /// </summary>
        public List<BiomeType> SyncClearedFromDefeatedBosses()
        {
            var marked = new List<BiomeType>();
            if (_progress == null)
            {
                return marked;
            }

            foreach (BiomeType region in (BiomeType[])Enum.GetValues(typeof(BiomeType)))
            {
                if (IsBossDefeated(region) && _progress.MarkRegionCleared(region))
                {
                    marked.Add(region);
                }
            }

            return marked;
        }

        /// <summary>
        /// Builds the boss challenge for <paramref name="region"/>: its boss first, then
        /// <paramref name="minMinions"/>..<paramref name="maxMinions"/> regular enemies rolled from the
        /// same table (never another boss), flagged <see cref="EncounterPayload.isBossChallenge"/>.
        /// Null when the challenge is not offered (<see cref="CanChallengeBoss"/>).
        /// </summary>
        public EncounterPayload BuildChallengePayload(BiomeType region, System.Random random, int minMinions = DefaultMinMinions, int maxMinions = DefaultMaxMinions)
        {
            if (!CanChallengeBoss(region))
            {
                return null;
            }

            random ??= new System.Random();
            EnemyDefinition boss = BossFor(region);
            var enemies = new List<EnemyDefinition> { boss };

            BiomeDefinition biome = _biomeLookup(region);
            minMinions = Math.Max(0, minMinions);
            maxMinions = Math.Max(minMinions, maxMinions);
            int minions = random.Next(minMinions, maxMinions + 1);

            for (int i = 0; i < minions && biome != null; i++)
            {
                EnemyDefinition minion = biome.RollEnemy(random, IsMinionCandidate);
                if (minion != null)
                {
                    enemies.Add(minion);
                }
            }

            return EncounterPayload.Create(region, enemies, random.Next(), isBossChallenge: true);
        }

        private bool IsMinionCandidate(EnemyDefinition enemy)
        {
            return enemy != null && !enemy.IsBoss && CanRollInNormalEncounter(enemy);
        }
    }
}
