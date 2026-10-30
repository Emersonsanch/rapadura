using System;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// Mounts that grow with the hero (Docs/Progressao500.md §6, Docs/Mounts.md §4.1.1). Pure: the online server runs the
    /// very same numbers.
    /// <list type="bullet">
    /// <item><b>Level cap by rider</b>: <c>clamp(floor(2·√h), 1, 20)</c>, h = level of the cousin riding (any h: 1 → 2,
    /// 25 → 10, 100+ → 20). A mount keeps its saved level; above the rider's cap it works at the cap
    /// (<see cref="EffectiveLevel"/>) and loses nothing.</item>
    /// <item><b>Brilho</b> (glow, 0..<see cref="MaxGlow"/>): XP earned at the cap is stored; when the rider grows and the
    /// cap opens, the Brilho goes in first. At 20 a full Brilho is a requirement of the Despertar.</item>
    /// <item><b>Combat XP</b>: <c>2 + 1%</c> of the kill XP, at most <see cref="MaxCombatXpPerKill"/> per kill.</item>
    /// <item><b>Despertar I–IV</b> at hero 100 / 200 / 300 / 500: look and ability only (never speed, defence, damage).</item>
    /// </list>
    /// </summary>
    public static class MountGrowthRules
    {
        /// <summary>Most Brilho a mount stores (the "XP guardado").</summary>
        public const int MaxGlow = 5000;

        public const int BaseCombatXp = 2;
        public const float CombatXpShareOfKill = 0.01f;

        /// <summary>Kill XP per point of combat XP (1%).</summary>
        private const int CombatXpPercentOfKill = 100;
        public const int MaxCombatXpPerKill = 30;

        /// <summary>Last Despertar (IV, Forma de Luz).</summary>
        public const int MaxAwakening = 4;

        /// <summary>Hero level each Despertar asks (I..IV).</summary>
        public static readonly int[] AwakeningHeroLevels = { 100, 200, 300, 500 };

        /// <summary>Hearts of affection each Despertar asks (I..IV).</summary>
        public static readonly int[] AwakeningHearts = { 3, 4, 5, 5 };

        /// <summary>Kilometres ridden with the mount for Despertar IV.</summary>
        public const float AwakeningIvKm = 1000f;

        // ---------------- Level cap ----------------

        /// <summary><c>clamp(floor(2·√heroLevel), 1, 20)</c>; works for any hero level (1..500 and beyond).</summary>
        public static int LevelCapFor(int heroLevel)
        {
            int h = Math.Max(1, heroLevel);
            int cap = (int)Math.Floor(2.0 * Math.Sqrt(h) + 1e-9);
            return Math.Max(1, Math.Min(MountRules.MaxLevel, cap));
        }

        /// <summary>The level the mount works at under a rider of <paramref name="heroLevel"/>: <c>min(level, cap)</c>.</summary>
        public static int EffectiveLevel(int mountLevel, int heroLevel)
        {
            int level = Math.Max(1, Math.Min(MountRules.MaxLevel, mountLevel));
            return Math.Min(level, LevelCapFor(heroLevel));
        }

        /// <summary>Lowest hero level whose cap reaches <paramref name="mountLevel"/> (<c>ceil((L/2)²)</c>; 20 → 100).</summary>
        public static int HeroLevelForCap(int mountLevel)
        {
            int level = Math.Max(1, Math.Min(MountRules.MaxLevel, mountLevel));
            int h = 1;
            while (LevelCapFor(h) < level)
            {
                h++;
            }

            return h;
        }

        // ---------------- XP ----------------

        /// <summary>Mount XP for a kill of <paramref name="killXp"/> with the hero: <c>2 + floor(1%)</c>, at most 30.</summary>
        public static int CombatXp(int killXp)
        {
            // Integer percent: 0.01f is not exact in binary, so (2800 * 0.01f) floors to 27.
            int share = Math.Max(0, killXp) / CombatXpPercentOfKill;
            return Math.Min(MaxCombatXpPerKill, BaseCombatXp + share);
        }

        /// <summary>Brilho after storing <paramref name="amount"/> more (clamped to 0..<see cref="MaxGlow"/>).</summary>
        public static int GlowAfter(int glow, int amount) => Math.Max(0, Math.Min(MaxGlow, glow + Math.Max(0, amount)));

        public static bool IsGlowFull(int glow) => glow >= MaxGlow;

        /// <summary>
        /// Adds <paramref name="amount"/> XP to a mount ridden by a hero of <paramref name="heroLevel"/>: the stored Brilho
        /// goes in first while the cap allows, then the new XP; whatever the cap holds back becomes Brilho. A saved level
        /// above the cap is never lowered (it simply stores everything). Returns levels gained.
        /// </summary>
        public static int AddXp(ref int level, ref int xp, ref int glow, int amount, int heroLevel)
        {
            level = Math.Max(1, Math.Min(MountRules.MaxLevel, level));
            xp = Math.Max(0, xp);
            glow = Math.Max(0, Math.Min(MaxGlow, glow));
            amount = Math.Max(0, amount);
            int cap = LevelCapFor(heroLevel);

            if (level >= cap)
            {
                if (level >= MountRules.MaxLevel)
                {
                    xp = 0;
                }

                glow = GlowAfter(glow, amount);
                return 0;
            }

            // The cap is open: the Brilho goes in first, then the new XP.
            int pool = xp + glow + amount;
            glow = 0;
            int gained = 0;
            while (level < cap && pool >= MountCareRules.XpToNext(level))
            {
                pool -= MountCareRules.XpToNext(level);
                level++;
                gained++;
            }

            if (level >= cap)
            {
                // Bar at the cap: the rest waits as Brilho (a level-20 mount has no bar).
                xp = 0;
                glow = GlowAfter(0, pool);
            }
            else
            {
                xp = pool;
            }

            return gained;
        }

        // ---------------- Despertar ----------------

        /// <summary>Hero level Despertar <paramref name="stage"/> (1..4) asks; int.MaxValue past IV.</summary>
        public static int HeroLevelForAwakening(int stage)
        {
            return stage >= 1 && stage <= MaxAwakening ? AwakeningHeroLevels[stage - 1] : int.MaxValue;
        }

        /// <summary>
        /// Whether the next Despertar (<paramref name="awakening"/> + 1) is open: hero level (100/200/300/500), mount at
        /// 20, hearts (3/4/5/5), a full Brilho for I and II, 1 000 km ridden for IV, and the materials and the Srosh's short
        /// quest (<paramref name="hasMaterials"/>, checked by the stable).
        /// </summary>
        public static bool CanAwaken(int awakening, int heroLevel, int mountLevel, int glow, int hearts, float kmRidden, bool hasMaterials)
        {
            int next = awakening + 1;
            if (awakening < 0 || next > MaxAwakening || !hasMaterials)
            {
                return false;
            }

            if (heroLevel < AwakeningHeroLevels[next - 1] || mountLevel < MountRules.MaxLevel || hearts < AwakeningHearts[next - 1])
            {
                return false;
            }

            if (next <= 2 && !IsGlowFull(glow))
            {
                return false;
            }

            return next != 4 || kmRidden >= AwakeningIvKm;
        }

        /// <summary>Despertar I and II spend the full Brilho; III and IV keep it.</summary>
        public static bool AwakeningSpendsGlow(int stage) => stage == 1 || stage == 2;

        /// <summary>"+1 grau" of the ability from Despertar I: 1 before I, ×1.15 for distance/duration after it.</summary>
        public static float AbilityGradeScale(int awakening) => awakening >= 1 ? 1.15f : 1f;

        /// <summary>Extra radius (m) of radius abilities from Despertar I.</summary>
        public static float AbilityRadiusBonus(int awakening) => awakening >= 1 ? 1f : 0f;

        /// <summary>Despertar II adds a utility side effect to the ability.</summary>
        public static bool HasSecondaryEffect(int awakening) => awakening >= 2;

        /// <summary>Despertar III: the ability's cooldown ×0.9 and the out-of-combat group passive.</summary>
        public static float AbilityCooldownFactor(int awakening) => awakening >= 3 ? 0.9f : 1f;

        /// <summary>Despertar IV: two charges of the ability (same total cooldown).</summary>
        public static int AbilityCharges(int awakening) => awakening >= 4 ? 2 : 1;
    }
}
