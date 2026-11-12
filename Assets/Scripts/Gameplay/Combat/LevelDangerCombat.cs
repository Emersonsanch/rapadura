using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.UI.Common;
using UnityEngine;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>
    /// Unity side of <see cref="LevelDangerRules"/> (the "Perigoso" regra B): finds the two levels on the GameObjects of a
    /// hit — the hero's <see cref="PlayerStats.Level"/> and the creature's <see cref="ICreatureLevelSource"/> (open-map
    /// monsters only; campaign encounters have none and are never touched) — and applies the rule. Called by
    /// <see cref="PlayerStats"/> (creature → hero: damage ×1.15..×1.3 and +25% crit) and <see cref="Health"/> (hero → creature:
    /// −40% hit, "Errou!"). The random source is only sampled when the rule is active, so ordinary fights stay deterministic.
    /// </summary>
    public static class LevelDangerCombat
    {
        /// <summary>Uniform 0..1 roll; tests replace it (null = Unity's RNG).</summary>
        public static Func<float> Random;

        private static float Roll() => (Random ?? OutgoingDamage.UnityRandom)();

        /// <summary>Level of the creature that dealt or takes a hit (its own component or a parent's); false without one.</summary>
        public static bool TryGetCreatureLevel(GameObject creature, out int level)
        {
            level = 0;
            if (creature == null)
            {
                return false;
            }

            ICreatureLevelSource source = creature.GetComponentInParent<ICreatureLevelSource>();
            if (source == null)
            {
                return false;
            }

            level = source.CreatureLevel;
            return level > 0;
        }

        /// <summary>The creature's hit on <paramref name="hero"/> (before defense). Unchanged unless the attacker has its own level far above.</summary>
        public static float ResolveCreatureHit(float amount, int heroLevel, GameObject attacker, out bool isCritical)
        {
            isCritical = false;
            if (amount <= 0f || !TryGetCreatureLevel(attacker, out int creatureLevel)
                || LevelDangerRules.IsNone(LevelDangerRules.Delta(heroLevel, creatureLevel)))
            {
                return amount;
            }

            return LevelDangerRules.ResolveCreatureHit(amount, heroLevel, creatureLevel, Roll(), out isCritical);
        }

        /// <summary>True when the hero's hit from <paramref name="source"/> on <paramref name="target"/> misses (creature 20+ levels above).</summary>
        public static bool HeroMisses(GameObject target, GameObject source)
        {
            if (source == null || !TryGetCreatureLevel(target, out int creatureLevel))
            {
                return false;
            }

            PlayerStats hero = source.GetComponentInParent<PlayerStats>();
            if (hero == null)
            {
                return false;
            }

            if (LevelDangerRules.HeroHitRateMultiplier(LevelDangerRules.Delta(hero.Level, creatureLevel)) >= 1f)
            {
                return false;
            }

            return !LevelDangerRules.HeroHitLands(hero.Level, creatureLevel, Roll());
        }

        /// <summary>Floating "Errou!" / "Miss!" over a creature the hero missed (play mode only).</summary>
        public static void ShowMiss(Vector3 position)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            LocalizationManager manager = ServiceLocator.TryGet(out LocalizationManager live) ? live : null;
            CombatFxManager.SpawnLootWarningText(position + Vector3.up * 2f, UiTexts.Get(manager, UiTexts.HudMiss));
        }
    }
}
