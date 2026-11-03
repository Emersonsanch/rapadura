using System.Collections;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.Dungeon
{
    /// <summary>
    /// Marks one enemy as a guardian of the ossuary (added by <see cref="DungeonDirector"/> right after it spawns):
    /// <list type="bullet">
    /// <item>its loot comes from the catacombs' table (<see cref="IEnemyLootSource"/>, read by EnemyLootGranter);</item>
    /// <item><see cref="IsReanimated"/> — relit by the Bone Sorcerer: 40% of its max health, a dimmed look, and the golden rule
    /// of <see cref="DungeonRules.KillExperience"/>: <b>0 XP and no drop</b> when it falls again (EnemyRewardGranter reads
    /// <see cref="IKillExperienceScaler"/> before every other multiplier, so nothing can turn the 0 back into XP);</item>
    /// <item>the room it guards and whether it carries the room's bone key.</item>
    /// </list>
    /// </summary>
    [DisallowMultipleComponent]
    public class DungeonEnemyTag : MonoBehaviour, IKillExperienceScaler, IEnemyLootSource
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        /// <summary>Tint of a relit guardian: a dim, cool ivory (kid-friendly "sleepy" look, nothing gory).</summary>
        public static readonly Color ReanimatedTint = new Color(0.55f, 0.55f, 0.72f, 1f);

        private bool _healthApplied;

        public string EnemyId { get; private set; }
        public int Room { get; private set; }
        public bool IsReanimated { get; private set; }
        public bool CarriesKey { get; set; }
        public BiomeDefinition Table { get; private set; }

        public bool SuppressLoot => !DungeonRules.DropsLoot(IsReanimated);
        public BiomeDefinition LootTable => Table;

        public int ScaleKillExperience(int baseExperience) => DungeonRules.KillExperience(baseExperience, IsReanimated);

        public static DungeonEnemyTag Attach(GameObject enemy, string enemyId, int room, BiomeDefinition lootTable, bool reanimated = false)
        {
            if (enemy == null)
            {
                return null;
            }

            DungeonEnemyTag tag = enemy.GetComponent<DungeonEnemyTag>() ?? enemy.AddComponent<DungeonEnemyTag>();
            tag.EnemyId = enemyId;
            tag.Room = room;
            tag.Table = lootTable;
            tag.IsReanimated = reanimated;
            if (reanimated)
            {
                tag.CarriesKey = false;
            }

            return tag;
        }

        private IEnumerator Start()
        {
            if (!IsReanimated)
            {
                yield break;
            }

            // EnemyController.Start re-applies the definition's full health on the first frame: relight after it.
            yield return null;
            ApplyReanimatedHealth();
            Dim();
        }

        /// <summary>Sets the relit guardian to <see cref="DungeonRules.RaiseHealthFraction"/> of its max health (idempotent). Public for tests.</summary>
        public void ApplyReanimatedHealth()
        {
            if (_healthApplied || !IsReanimated)
            {
                return;
            }

            _healthApplied = true;
            EnemyController controller = GetComponent<EnemyController>();
            Health health = controller != null && controller.Health != null ? controller.Health : GetComponent<Health>();
            if (health != null)
            {
                health.ConfigureMaxHealth(Mathf.Max(1f, health.MaxHealth * DungeonRules.RaiseHealthFraction));
            }
        }

        private void Dim()
        {
            var block = new MaterialPropertyBlock();
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.GetPropertyBlock(block);
                block.SetColor(BaseColorId, ReanimatedTint);
                block.SetColor(ColorId, ReanimatedTint);
                renderer.SetPropertyBlock(block);
            }
        }
    }
}
