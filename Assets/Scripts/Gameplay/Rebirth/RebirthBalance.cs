using System;
using Rapadura.Gameplay.Player;

namespace Rapadura.Gameplay.Rebirth
{
    /// <summary>
    /// Balance model of the reset bonus: the power of a hero at level L with R effective resets against the same hero with
    /// none. Uses the curves of Progressao500 §2 (vida <c>100 + 8·(L−1)</c>, <c>damage_mult = 1 + 0,02·(L−1)</c>, 2 attribute
    /// points per level) and the per-point effects of <see cref="AttributeSet"/> (Vitalidade +10 de vida plana, Poder Arcano
    /// +1% de dano). The reference hero splits every point half into Poder Arcano and half into Vitalidade, and power is the
    /// geometric mean of damage and life (<c>√(dano × vida)</c>), the same "kill speed × survival" the balance tools measure.
    ///
    /// Equipment, skills and graduations are left out on purpose: they are the same for both heroes, so leaving them out
    /// only makes the bonus look <i>bigger</i> than it is (a pessimistic check). Band (tested): at any level, 1 reset adds
    /// less than 3%, 10 resets (one cycle) at most 15% and the 60-reset cap at most <see cref="MaxPowerRatio"/> (30%).
    /// </summary>
    public static class RebirthBalance
    {
        public const float MaxPowerRatio = 1.30f;
        public const float OneCyclePowerRatio = 1.15f;
        public const float OneResetPowerRatio = 1.03f;

        public const float BaseHealth = 100f;
        public const float HealthPerLevel = 8f;
        public const float DamageMultPerLevel = 0.02f;
        public const int AttributePointsPerLevel = 2;

        /// <summary>Attribute points a hero has at <paramref name="level"/> from levelling alone (2 per level after the first).</summary>
        public static int LevelPoints(int level) => AttributePointsPerLevel * (LevelCurveMath.ClampLevel(level) - 1);

        /// <summary>Power of the reference hero at <paramref name="level"/> with <paramref name="extraPoints"/> more attribute points.</summary>
        public static double Power(int level, int extraPoints)
        {
            int l = LevelCurveMath.ClampLevel(level);
            int points = LevelPoints(l) + Math.Max(0, extraPoints);
            int arcane = points / 2;
            int vitality = points - arcane;
            double damage = (1d + DamageMultPerLevel * (l - 1)) * (1d + AttributeSet.ArcanePowerAttackDamagePerPoint * arcane);
            double health = BaseHealth + HealthPerLevel * (l - 1) + AttributeSet.VitalityHealthPerPoint * vitality;
            return Math.Sqrt(damage * health);
        }

        /// <summary>Power of a hero at <paramref name="level"/> with <paramref name="effectiveResets"/> resets ÷ the same hero with none.</summary>
        public static double PowerRatio(int level, int effectiveResets)
        {
            int bonus = RebirthRules.UnlockedBonusPoints(RebirthRules.TotalBonusPoints(effectiveResets), level);
            return Power(level, bonus) / Power(level, 0);
        }
    }
}
