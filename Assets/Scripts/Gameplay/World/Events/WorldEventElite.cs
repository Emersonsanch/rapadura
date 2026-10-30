using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using UnityEngine;

namespace Rapadura.Gameplay.World.Events
{
    /// <summary>
    /// "Emboscada de elite": the ambush payload and how tough/rewarding its elite is. The elite is a regular
    /// (never boss) enemy of the region's spawn table, first in the payload, made tougher in the arena by
    /// <see cref="WorldEventArenaHooks"/> (health, damage, size), which also pays the bonus when it dies. Numbers
    /// are in Docs/Balance/README.md ("Eventos aleatórios do mapa").
    /// </summary>
    public static class WorldEventElite
    {
        public const float HealthMultiplier = 2f;

        /// <summary>PercentAdditive AttackDamage on the elite's BuffController (+35%).</summary>
        public const float AttackDamageBonus = 0.35f;

        public const float ScaleMultiplier = 1.25f;

        /// <summary>Regular enemies of the region that come with the elite.</summary>
        public const int Minions = 1;

        /// <summary>Gold when the elite dies (≈ 5 fights of gold drops; not region-scaled, like the drops).</summary>
        public const int BonusGold = 10;

        /// <summary>XP on top of the elite's own kill XP (× <see cref="WorldEventRules.RegionRewardMultiplier"/>: two more kills' worth).</summary>
        public const int BonusExperience = 20;

        /// <summary>Extra rolls of the elite's own loot (its gold + the biome table), on top of its normal drop.</summary>
        public const int ExtraLootRolls = 2;

        public const string BuffSource = "world_event_elite";

        /// <summary>
        /// Rolls the ambush in <paramref name="biome"/>: the elite (a non-boss enemy that passes <paramref name="filter"/>)
        /// followed by <see cref="Minions"/> regular enemies. Null when the region has no eligible enemy.
        /// </summary>
        public static EncounterPayload BuildPayload(BiomeDefinition biome, System.Random random, Predicate<EnemyDefinition> filter = null)
        {
            if (biome == null || random == null)
            {
                return null;
            }

            bool Allowed(EnemyDefinition enemy) => enemy != null && !enemy.IsBoss && (filter == null || filter(enemy));

            EnemyDefinition elite = biome.RollEnemy(random, Allowed);
            if (elite == null)
            {
                return null;
            }

            var enemies = new List<EnemyDefinition> { elite };
            for (int i = 0; i < Minions; i++)
            {
                EnemyDefinition minion = biome.RollEnemy(random, Allowed);
                if (minion != null)
                {
                    enemies.Add(minion);
                }
            }

            return EncounterPayload.Create(biome.Type, enemies, random.Next());
        }

        public static int ScaledExperience(BiomeType region) => Mathf.RoundToInt(BonusExperience * WorldEventRules.RegionRewardMultiplier(region));
    }
}
