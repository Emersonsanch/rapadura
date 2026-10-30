using System.Collections.Generic;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Events
{
    /// <summary>One effect as it really happened (amount after region scaling; for an item, how many landed).</summary>
    public readonly struct WorldEventEffectResult
    {
        public readonly WorldEventEffect Effect;
        public readonly int Amount;

        public WorldEventEffectResult(WorldEventEffect effect, int amount)
        {
            Effect = effect;
            Amount = amount;
        }
    }

    /// <summary>What picking a choice did, plus what the map still has to do (slow the token, move the clock, start a fight).</summary>
    public sealed class WorldEventOutcome
    {
        public readonly List<WorldEventEffectResult> Results = new List<WorldEventEffectResult>();

        /// <summary>Token speed multiplier until arrival (1 = unchanged; several slows multiply).</summary>
        public float TravelSpeedMultiplier = 1f;

        public float HoursToAdvance;
        public bool StartElite;
        public bool StartEncounter;
    }

    /// <summary>
    /// Applies a <see cref="WorldEventChoiceSpec"/> to the real systems. On the map: costs and items go through the
    /// live bag (a <c>CarriedInventorySession</c> inventory: <see cref="InventoryManager.RemoveById"/> and
    /// <see cref="LootGrantRules.Grant"/>, so gold passes the weight cap like loot), XP/heal/buffs are queued in
    /// <see cref="WorldEventState"/>. In the next fight, <see cref="ApplyPendingBoons"/> hands those to the live
    /// PlayerStats/BuffController. Travel, clock and encounter effects are returned in the
    /// <see cref="WorldEventOutcome"/> for the map to carry out.
    /// </summary>
    public static class WorldEventOutcomes
    {
        /// <summary>
        /// Applies <paramref name="choice"/> in <paramref name="region"/>. Returns null (and changes nothing) when
        /// the bag can't pay its costs; without a bag (scene on its own) costs block and item gains are skipped.
        /// </summary>
        public static WorldEventOutcome Apply(WorldEventChoiceSpec choice, BiomeType region, InventoryManager inventory, WorldEventState state)
        {
            if (choice == null)
            {
                return null;
            }

            if (!WorldEventRules.CanAfford(choice, inventory != null ? (System.Func<string, int>)inventory.GetTotalCount : null))
            {
                return null;
            }

            var outcome = new WorldEventOutcome();

            // Costs first: a gain must never be what pays for itself.
            foreach (WorldEventEffect cost in choice.Costs)
            {
                if (inventory != null && inventory.RemoveById(cost.ItemId, cost.Amount))
                {
                    outcome.Results.Add(new WorldEventEffectResult(cost, cost.Amount));
                }
            }

            foreach (WorldEventEffect effect in choice.Effects)
            {
                int amount = WorldEventRules.ScaledAmount(effect, region);
                switch (effect.Kind)
                {
                    case WorldEventEffectKind.GainItem:
                        ItemDefinition item = ItemDatabase.GetById(effect.ItemId);
                        if (inventory != null && item != null && amount > 0)
                        {
                            LootGrantResult granted = LootGrantRules.Grant(inventory, new LootDrop(item, amount), null);
                            outcome.Results.Add(new WorldEventEffectResult(effect, granted.Granted));
                        }

                        break;

                    case WorldEventEffectKind.NextFightExperience:
                        state?.AddPendingExperience(amount);
                        outcome.Results.Add(new WorldEventEffectResult(effect, amount));
                        break;

                    case WorldEventEffectKind.NextFightHeal:
                        state?.AddPendingHeal(effect.Value);
                        outcome.Results.Add(new WorldEventEffectResult(effect, Mathf.RoundToInt(effect.Value * 100f)));
                        break;

                    case WorldEventEffectKind.NextFightBuff:
                        state?.AddPendingBuff(effect.Stat, effect.Application, effect.Value);
                        outcome.Results.Add(new WorldEventEffectResult(effect, 0));
                        break;

                    case WorldEventEffectKind.SlowTravel:
                        outcome.TravelSpeedMultiplier *= Mathf.Clamp(effect.Value, 0.1f, 1f);
                        outcome.Results.Add(new WorldEventEffectResult(effect, Mathf.RoundToInt((1f - Mathf.Clamp(effect.Value, 0.1f, 1f)) * 100f)));
                        break;

                    case WorldEventEffectKind.AdvanceHours:
                        outcome.HoursToAdvance += Mathf.Max(0f, effect.Value);
                        outcome.Results.Add(new WorldEventEffectResult(effect, Mathf.RoundToInt(effect.Value)));
                        break;

                    case WorldEventEffectKind.EliteEncounter:
                        outcome.StartElite = true;
                        outcome.Results.Add(new WorldEventEffectResult(effect, 0));
                        break;

                    case WorldEventEffectKind.Encounter:
                        outcome.StartEncounter = true;
                        outcome.Results.Add(new WorldEventEffectResult(effect, 0));
                        break;
                }
            }

            return outcome;
        }

        /// <summary>
        /// Gives the hero of the fight that just started what events queued: XP, then the heal (as a fraction of
        /// the max health), then the buffs (one fight long, under <see cref="WorldEventState.BuffSource"/>). Clears the
        /// queue. Call after the carried PlayerStats state is restored, or the restore overwrites health/XP.
        /// Returns false (queue kept) without PlayerStats.
        /// </summary>
        public static bool ApplyPendingBoons(WorldEventState state, PlayerStats stats, BuffController buffs)
        {
            if (state == null || stats == null || !state.HasPendingBoons)
            {
                return false;
            }

            if (state.PendingExperience > 0)
            {
                stats.AddExperience(state.PendingExperience);
            }

            if (buffs != null)
            {
                foreach (StatModifierDefinition buff in state.PendingBuffs)
                {
                    buffs.ApplyEffect(buff, WorldEventState.BuffSource + "/" + buff.affectedStat);
                }
            }

            // After the buffs: a MaxHealth/regen buff must count in what "35% of the health" means.
            if (state.PendingHealFraction > 0f)
            {
                stats.Heal(stats.MaxHealth * state.PendingHealFraction);
            }

            state.ClearPendingBoons();
            return true;
        }
    }
}
