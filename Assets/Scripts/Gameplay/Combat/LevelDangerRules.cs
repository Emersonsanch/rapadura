using System;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>
    /// "Perigoso" — regra B (Docs/MetodologiaMapasERacas.md §4, Docs/Progressao500.md §1.2): what a creature far above the
    /// hero does in combat. <c>delta = creature level − hero level</c>:
    /// <code>
    /// delta ≥ 10   the creature gets +25% critical chance against the hero (crit = ×1.5)
    /// delta ≥ 20   the hero's hit chance ×0.6 (−40%: "Errou!") and the creature's damage ×1.5
    /// 20..30       the creature's damage grows linearly ×1.5 → ×3
    /// delta ≥ 30   the creature's damage ×3 ("quase hit-kill")
    /// </code>
    /// Below +10 (and any creature at or below the hero's level) nothing changes. Only creatures with their own level
    /// (<c>ICreatureLevelSource</c>: the open maps) are judged; campaign encounters never are. Pure C# (no Unity): the
    /// online server runs the same file; the rolls are injected (uniform 0..1). Wired by <see cref="LevelDangerCombat"/>
    /// (PlayerStats for the creature's hit, Health for the hero's). EditMode-tested (LevelDangerRulesTests).
    /// </summary>
    public static class LevelDangerRules
    {
        /// <summary>From this many levels above the hero the creature crits more (and the "Perigoso" warning shows).</summary>
        public const int CritDelta = 10;

        /// <summary>From this many levels above the hero the hero misses more and the creature hits harder.</summary>
        public const int HitRateDelta = 20;

        /// <summary>From this many levels above the hero the creature's damage is at its cap (near one-shot).</summary>
        public const int LethalDelta = 30;

        /// <summary>Added to the creature's critical chance (creatures have none of their own today).</summary>
        public const float CritChanceBonus = 0.25f;

        /// <summary>Damage multiplier of the creature's critical hit (same as the default <see cref="DamageCalculator"/> crit).</summary>
        public const float CreatureCriticalMultiplier = 1.5f;

        /// <summary>Share of the hero's hits lost from <see cref="HitRateDelta"/> on (hit chance ×0.6).</summary>
        public const float HitRatePenalty = 0.4f;

        /// <summary>Creature damage multiplier at <see cref="HitRateDelta"/>.</summary>
        public const float HeavyDamageTaken = 1.5f;

        /// <summary>Creature damage multiplier at and beyond <see cref="LethalDelta"/>.</summary>
        public const float LethalDamageTaken = 3f;

        /// <summary>Creature level − hero level (negative = the hero is above).</summary>
        public static int Delta(int heroLevel, int creatureLevel) => creatureLevel - heroLevel;

        /// <summary>True from <see cref="CritDelta"/> levels above: the "Perigoso" warning and the first effect of the rule.</summary>
        public static bool IsDangerous(int delta) => delta >= CritDelta;

        public static bool IsLethal(int delta) => delta >= LethalDelta;

        /// <summary>Added to the creature's critical chance: <see cref="CritChanceBonus"/> from <see cref="CritDelta"/>, else 0.</summary>
        public static float CreatureCritChanceBonus(int delta) => delta >= CritDelta ? CritChanceBonus : 0f;

        /// <summary>Multiplier on the hero's hit chance against the creature: 0.6 from <see cref="HitRateDelta"/>, else 1.</summary>
        public static float HeroHitRateMultiplier(int delta) => delta >= HitRateDelta ? 1f - HitRatePenalty : 1f;

        /// <summary>Multiplier on the creature's damage to the hero: 1 below +20, ×1.5 at +20 rising linearly to ×3 at +30, ×3 beyond.</summary>
        public static float CreatureDamageMultiplier(int delta)
        {
            if (delta < HitRateDelta)
            {
                return 1f;
            }

            if (delta >= LethalDelta)
            {
                return LethalDamageTaken;
            }

            float t = (delta - HitRateDelta) / (float)(LethalDelta - HitRateDelta);
            return HeavyDamageTaken + (LethalDamageTaken - HeavyDamageTaken) * t;
        }

        /// <summary>True when the rule changes nothing at <paramref name="delta"/> (below +10).</summary>
        public static bool IsNone(int delta) => delta < CritDelta;

        /// <summary>
        /// The creature's hit on the hero, before the hero's defense: <paramref name="amount"/> × <see cref="CreatureDamageMultiplier"/>,
        /// then ×<see cref="CreatureCriticalMultiplier"/> when <paramref name="critRoll01"/> &lt; the crit bonus. Unchanged below +10.
        /// </summary>
        public static float ResolveCreatureHit(float amount, int heroLevel, int creatureLevel, float critRoll01, out bool isCritical)
        {
            isCritical = false;
            int delta = Delta(heroLevel, creatureLevel);
            if (amount <= 0f || IsNone(delta))
            {
                return Math.Max(0f, amount);
            }

            float damage = amount * CreatureDamageMultiplier(delta);
            float chance = CreatureCritChanceBonus(delta);
            isCritical = chance > 0f && critRoll01 < chance;
            return isCritical ? damage * CreatureCriticalMultiplier : damage;
        }

        /// <summary>True when the hero's hit lands: always below +20, else when <paramref name="hitRoll01"/> &lt; 0.6.</summary>
        public static bool HeroHitLands(int heroLevel, int creatureLevel, float hitRoll01)
        {
            float chance = HeroHitRateMultiplier(Delta(heroLevel, creatureLevel));
            return chance >= 1f || hitRoll01 < chance;
        }
    }
}
