using System;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Optional per-creature reward modifiers, installed by the system that owns the rule (today the hunting features:
    /// "Presa do dia" and "Bicho destaque do dia", <c>HuntingProgress</c>) and cleared on its shutdown. With nothing
    /// installed the kill XP (<see cref="EnemyRewardGranter"/>) and the loot (<see cref="EnemyLootGranter"/>) are
    /// exactly as before. A throwing hook is ignored.
    /// </summary>
    public static class EnemyRewardHooks
    {
        /// <summary>Kill XP multiplier for a defeated enemy (1 = unchanged).</summary>
        public static Func<EnemyDefinition, float> ExperienceMultiplier;

        /// <summary>Chance of extra whole loot rolls for a defeated enemy (0 = none; 1.25 = one sure + 25% for a second).</summary>
        public static Func<EnemyDefinition, float> ExtraLootChance;

        public static float ResolveExperienceMultiplier(EnemyDefinition enemy)
        {
            Func<EnemyDefinition, float> hook = ExperienceMultiplier;
            if (hook == null || enemy == null)
            {
                return 1f;
            }

            try
            {
                float value = hook(enemy);
                return float.IsNaN(value) || value < 0f ? 1f : value;
            }
            catch (Exception)
            {
                return 1f;
            }
        }

        public static float ResolveExtraLootChance(EnemyDefinition enemy)
        {
            Func<EnemyDefinition, float> hook = ExtraLootChance;
            if (hook == null || enemy == null)
            {
                return 0f;
            }

            try
            {
                float value = hook(enemy);
                return float.IsNaN(value) || value < 0f ? 0f : value;
            }
            catch (Exception)
            {
                return 0f;
            }
        }

        /// <summary>Scales a kill's XP by <see cref="ResolveExperienceMultiplier"/> (rounded, never negative).</summary>
        public static int ScaleExperience(EnemyDefinition enemy, int experience)
        {
            float multiplier = ResolveExperienceMultiplier(enemy);
            return multiplier == 1f ? experience : Math.Max(0, (int)Math.Round(experience * multiplier, MidpointRounding.AwayFromZero));
        }

        public static void ClearAll()
        {
            ExperienceMultiplier = null;
            ExtraLootChance = null;
        }
    }
}
