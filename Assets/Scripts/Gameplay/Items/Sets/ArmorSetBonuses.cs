using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items.Durability;
using Rapadura.Gameplay.Items.Options;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Items.Sets
{
    /// <summary>
    /// Wires <see cref="ArmorSetRules"/> to combat through the same events the combat FX listen to (like the Armadura de Marfim's
    /// <c>SpectralTouchSetBonus</c>): the nine two-piece bonuses, and the rare drop of a piece when a region's own beast dies. The
    /// drop lives here, not in the enemy's single <c>_lootItem</c> (the gold of every beast). Reflected hits and rays are plain,
    /// non-critical hits dealt by the hero, so no bonus can chain.
    /// </summary>
    public static class ArmorSetBonuses
    {
        private const string PlayerTag = "Player";

        private static readonly System.Random Random = new System.Random();
        private static readonly Collider[] Hits = new Collider[24];

        /// <summary>Roll override for tests (null = the shared random).</summary>
        public static System.Func<double> RollOverride { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void EnsureSubscribed()
        {
            EventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
            EventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
            EventBus.Unsubscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
            EventBus.Subscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
        }

        /// <summary>The set completely worn by <paramref name="inventory"/> (broken pieces do not count), or null.</summary>
        public static ArmorSetDefinition WornSet(InventoryManager inventory)
        {
            if (inventory == null)
            {
                return null;
            }

            var ids = new List<string>();
            foreach (KeyValuePair<EquipmentSlot, InventorySlotData> pair in inventory.Equipped)
            {
                string id = pair.Value?.itemId;
                if (!string.IsNullOrEmpty(id) && !DurabilityRules.IsBroken(ItemDatabase.GetById(id), pair.Value))
                {
                    ids.Add(id);
                }
            }

            return ArmorSetRules.CompleteSet(ids);
        }

        private static double NextRoll() => RollOverride != null ? RollOverride() : Random.NextDouble();

        private static void Say(GameObject over, string key)
        {
            CombatFxManager.SpawnLootText(over.transform.position + Vector3.up * 2.4f, ArmorSetTexts.Get(ContentText.CurrentManager, key));
        }

        private static void OnDamageApplied(DamageAppliedEvent evt)
        {
            if (evt.Source == null || evt.Target == null || evt.FinalDamage <= 0f)
            {
                return;
            }

            bool sourceIsEnemy = evt.Source.GetComponentInParent<EnemyController>() != null;
            if (sourceIsEnemy)
            {
                OnHeroStruck(evt);
            }
            else if (evt.IsCritical && evt.Target.GetComponent<EnemyController>() != null)
            {
                OnHeroCritical(evt);
            }
        }

        private static void OnHeroStruck(DamageAppliedEvent evt)
        {
            InventoryManager inventory = evt.Target.GetComponentInParent<InventoryManager>();
            ArmorSetDefinition worn = WornSet(inventory);
            if (worn == null)
            {
                return;
            }

            double roll = NextRoll();
            if (ArmorSetRules.ClosingCoralProcs(worn, roll))
            {
                PlayerStats stats = inventory.GetComponentInParent<PlayerStats>();
                if (stats != null && stats.IsAlive)
                {
                    stats.Heal(ArmorSetRules.ClosingCoralHeal(evt.FinalDamage));
                    Say(evt.Target, ArmorSetTexts.ClosingCoral);
                }

                return;
            }

            if (ArmorSetRules.BronzeGongProcs(worn, roll))
            {
                StrikeAround(evt.Target.transform.position, null, evt.Target, ArmorSetRules.BronzeGongDamage(evt.FinalDamage),
                    ArmorSetRules.BronzeGongRadius, ArmorSetRules.BronzeGongMaxTargets, ElementType.Earth);
                Say(evt.Target, ArmorSetTexts.BronzeGong);
                return;
            }

            if (!ArmorSetRules.GlazedReflectProcs(worn, roll))
            {
                return;
            }

            EnemyController attacker = evt.Source.GetComponentInParent<EnemyController>();
            if (attacker.Health == null || attacker.Health.IsDead)
            {
                return;
            }

            attacker.Health.ApplyDamage(ArmorSetRules.GlazedReflectDamage(evt.FinalDamage), ElementType.Earth, evt.Target);
            Say(evt.Target, ArmorSetTexts.GlazedReflect);
        }

        private static void OnHeroCritical(DamageAppliedEvent evt)
        {
            InventoryManager inventory = evt.Source.GetComponentInParent<InventoryManager>();
            ArmorSetDefinition worn = WornSet(inventory);
            if (worn == null)
            {
                return;
            }

            double roll = NextRoll();
            if (ArmorSetRules.ReopenedSealProcs(worn, true, roll))
            {
                PlayerStats stats = inventory.GetComponentInParent<PlayerStats>();
                if (stats != null)
                {
                    stats.RestoreMana(ArmorSetRules.ReopenedSealMana(stats.MaxMana));
                    Say(evt.Target, ArmorSetTexts.ReopenedSeal);
                }
            }
            else if (ArmorSetRules.ColumnRayProcs(worn, true, roll))
            {
                StrikeAround(evt.Target.transform.position, evt.Target, evt.Source, ArmorSetRules.ColumnRayDamage(evt.FinalDamage),
                    ArmorSetRules.ColumnRayRadius, ArmorSetRules.ColumnRayMaxTargets, ElementType.Holy);
                Say(evt.Target, ArmorSetTexts.ColumnRay);
            }
            else if (ArmorSetRules.PamirBreathProcs(worn, true, roll))
            {
                PlayerStats stats = inventory.GetComponentInParent<PlayerStats>();
                if (stats != null && stats.IsAlive)
                {
                    stats.Heal(ArmorSetRules.PamirBreathHeal(stats.MaxHealth));
                    Say(evt.Target, ArmorSetTexts.PamirBreath);
                }
            }
            else if (ArmorSetRules.NartGallopProcs(worn, true, roll))
            {
                StrikeAround(evt.Target.transform.position, evt.Target, evt.Source, ArmorSetRules.NartGallopDamage(evt.FinalDamage),
                    ArmorSetRules.NartGallopRadius, ArmorSetRules.NartGallopMaxTargets, ElementType.Physical);
                Say(evt.Target, ArmorSetTexts.NartGallop);
            }
        }

        /// <summary>Plain hits from <paramref name="hero"/> on up to <paramref name="maxTargets"/> living enemies within <paramref name="radius"/> of <paramref name="center"/>, <paramref name="skip"/> left out.</summary>
        private static void StrikeAround(Vector3 center, GameObject skip, GameObject hero, float damage, float radius, int maxTargets, ElementType element)
        {
            int count = Physics.OverlapSphereNonAlloc(center, radius, Hits, ~0, QueryTriggerInteraction.Ignore);
            var struck = new HashSet<EnemyController>();
            for (int i = 0; i < count && struck.Count < maxTargets; i++)
            {
                EnemyController enemy = Hits[i] != null ? Hits[i].GetComponentInParent<EnemyController>() : null;
                if (enemy == null || enemy.gameObject == skip || struck.Contains(enemy) || enemy.Health == null || enemy.Health.IsDead)
                {
                    continue;
                }

                struck.Add(enemy);
                enemy.Health.ApplyDamage(damage, element, hero);
            }
        }

        private static void OnCombatTargetDied(CombatTargetDiedEvent evt)
        {
            if (evt.Target == null)
            {
                return;
            }

            EnemyController enemy = evt.Target.GetComponent<EnemyController>();
            if (enemy == null || enemy.Definition == null)
            {
                return;
            }

            InventoryManager inventory = ResolveInventory(evt.Killer);
            if (inventory == null)
            {
                return;
            }

            ArmorSetDefinition worn = WornSet(inventory);
            double roll = NextRoll();
            TryBargeHourHeal(inventory, worn, roll);
            TryLotusSap(inventory, worn, roll);
            TryDropPiece(evt.Target, enemy, inventory);
        }

        /// <summary>The Seiva do Lótus bonus: gives the hero mana back when <paramref name="roll"/> procs. True if it restored.</summary>
        public static bool TryLotusSap(InventoryManager inventory, ArmorSetDefinition worn, double roll)
        {
            if (inventory == null || !ArmorSetRules.LotusSapProcs(worn, roll))
            {
                return false;
            }

            PlayerStats stats = inventory.GetComponentInParent<PlayerStats>();
            if (stats == null || !stats.IsAlive)
            {
                return false;
            }

            stats.RestoreMana(ArmorSetRules.LotusSapMana(stats.MaxMana));
            Say(inventory.gameObject, ArmorSetTexts.LotusSap);
            return true;
        }

        /// <summary>The Hora da Barca bonus: heals the hero (PlayerStats, the hero has no Health) when <paramref name="roll"/> procs. True if it healed.</summary>
        public static bool TryBargeHourHeal(InventoryManager inventory, ArmorSetDefinition worn, double roll)
        {
            if (inventory == null || !ArmorSetRules.BargeHourProcs(worn, roll))
            {
                return false;
            }

            PlayerStats stats = inventory.GetComponentInParent<PlayerStats>();
            if (stats == null || !stats.IsAlive)
            {
                return false;
            }

            stats.Heal(ArmorSetRules.BargeHourHeal(stats.MaxHealth));
            Say(inventory.gameObject, ArmorSetTexts.BargeHour);
            return true;
        }

        private static void TryDropPiece(GameObject corpse, EnemyController enemy, InventoryManager inventory)
        {
            IEnemyLootSource lootSource = enemy.GetComponent<IEnemyLootSource>();
            if (lootSource != null && lootSource.SuppressLoot)
            {
                return;
            }

            string itemId = ArmorSetRules.RollDrop(enemy.Definition.name, NextRoll(), NextRoll());
            ItemDefinition item = itemId != null ? ItemDatabase.GetById(itemId) : null;
            if (item == null)
            {
                return;
            }

            // Same rule as every equipment drop: a creature far below the hero gives no gear (Docs/Progressao500.md §1.2).
            PlayerStats stats = inventory.GetComponentInParent<PlayerStats>();
            if (enemy.TryGetComponent(out ICreatureLevelSource leveled) && !EnemyLevelScaling.DropsEquipment(stats != null ? stats.Level : 1, leveled.CreatureLevel))
            {
                return;
            }

            int seed = Random.Next();
            GeneratedItemInstance piece = ProceduralItemGenerator.Generate(item, EnemyLootRoller.RollEquipmentRarity(item, false, Random), seed);
            piece.Options = ItemOptionRules.Roll(item, false, ItemOptionRules.OptionSeed(seed));

            var drop = new LootDrop(piece);
            LootGrantResult result = LootGrantRules.Grant(inventory, drop, null);
            Vector3 labelPosition = corpse.transform.position + Vector3.up * 2.8f;
            if (result.Granted > 0)
            {
                CombatFxManager.SpawnLootText(labelPosition, "+ " + piece.GeneratedName);
                EventBus.Publish(new EnemyLootDroppedEvent(corpse, inventory.gameObject, new List<LootDrop> { drop }));
            }

            if (result.Lost > 0 || result.KeptOverWeight > 0)
            {
                EventBus.Publish(new LootOverflowEvent(labelPosition, new List<LootGrantResult> { result }));
            }
        }

        private static InventoryManager ResolveInventory(GameObject killer)
        {
            InventoryManager inventory = killer != null ? killer.GetComponentInParent<InventoryManager>() : null;
            if (inventory != null)
            {
                return inventory;
            }

            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            return player != null ? player.GetComponent<InventoryManager>() : null;
        }
    }
}
