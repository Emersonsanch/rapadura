using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Dungeon
{
    /// <summary>
    /// The set bonus of the Armadura de Marfim dos Artífices (<see cref="DungeonRules.BoneSetItemIds"/>): with both pieces
    /// equipped (and not broken), every critical hit the hero lands on an enemy has 15% chance to throw a bone shard
    /// ("Toque Espectral") that hits up to three other enemies within 4 m for 35% of the critical's damage. Listens to
    /// <see cref="DamageAppliedEvent"/> (critical flag + source) like the combat FX do; the shards are plain hits, never
    /// critical, so they cannot chain. The rule is pure (<see cref="DungeonRules.SpectralTouchProcs"/>); this class only wires it.
    /// </summary>
    public static class SpectralTouchSetBonus
    {
        private static readonly System.Random Random = new System.Random();
        private static readonly Collider[] Hits = new Collider[24];

        /// <summary>Roll override for tests (null = the shared random).</summary>
        public static System.Func<double> RollOverride { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void EnsureSubscribed()
        {
            EventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
            EventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
        }

        /// <summary>Distinct bone-set pieces worn (broken pieces do not count).</summary>
        public static int PiecesWorn(InventoryManager inventory)
        {
            if (inventory == null)
            {
                return 0;
            }

            var ids = new List<string>();
            foreach (KeyValuePair<EquipmentSlot, InventorySlotData> pair in inventory.Equipped)
            {
                string id = pair.Value?.itemId;
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                ItemDefinition item = ItemDatabase.GetById(id);
                if (!Rapadura.Gameplay.Items.Durability.DurabilityRules.IsBroken(item, pair.Value))
                {
                    ids.Add(id);
                }
            }

            return DungeonRules.CountBoneSetPieces(ids);
        }

        private static void OnDamageApplied(DamageAppliedEvent evt)
        {
            if (!evt.IsCritical || evt.Source == null || evt.Target == null || evt.FinalDamage <= 0f
                || evt.Target.GetComponent<EnemyController>() == null || evt.Source.GetComponentInParent<EnemyController>() != null)
            {
                return;
            }

            InventoryManager inventory = evt.Source.GetComponentInParent<InventoryManager>();
            int pieces = PiecesWorn(inventory);
            double roll = RollOverride != null ? RollOverride() : Random.NextDouble();
            if (!DungeonRules.SpectralTouchProcs(pieces, true, roll))
            {
                return;
            }

            float damage = DungeonRules.SpectralShardDamage(evt.FinalDamage);
            Vector3 center = evt.Target.transform.position;
            int count = Physics.OverlapSphereNonAlloc(center, DungeonRules.SpectralShardRadius, Hits, ~0, QueryTriggerInteraction.Ignore);
            var struck = new HashSet<EnemyController>();
            for (int i = 0; i < count && struck.Count < DungeonRules.SpectralShardMaxTargets; i++)
            {
                EnemyController enemy = Hits[i] != null ? Hits[i].GetComponentInParent<EnemyController>() : null;
                if (enemy == null || enemy.gameObject == evt.Target || struck.Contains(enemy) || enemy.Health == null || enemy.Health.IsDead)
                {
                    continue;
                }

                struck.Add(enemy);
                enemy.Health.ApplyDamage(damage, ElementType.Dark, evt.Source);
            }

            DungeonFx.Say(center + Vector3.up * 2.4f, DungeonFx.Text(DungeonTexts.SpectralTouch));
            if (Application.isPlaying)
            {
                DungeonPuff.Pebbles(center + Vector3.up * 1.2f, DungeonProps.Ivory, 4);
            }
        }
    }
}
