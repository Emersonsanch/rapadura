using Rapadura.Gameplay.Environment;
using UnityEngine;

namespace Rapadura.Gameplay.Night
{
    /// <summary>
    /// How dangerous one <see cref="TimeOfDayPhase"/> is in the encounter arena. Immutable value; one per phase in
    /// <see cref="NightDangerRules"/>. "Variant" numbers apply to the enemies dressed as night variants
    /// (<see cref="NightVariantRules"/>), the wave numbers to the arena's reinforcement waves.
    /// </summary>
    public readonly struct NightDangerProfile
    {
        /// <summary>Waves after the first one (the map's rolled group). The encounter ends when the last is cleared.</summary>
        public readonly int ReinforcementWaves;

        /// <summary>Seconds between a wave being cleared and the next one arriving.</summary>
        public readonly float WaveIntervalSeconds;

        /// <summary>Reinforcement wave size = round(first wave's regular count × this), clamped to [1, <see cref="NightDangerRules.MaxWaveSize"/>].</summary>
        public readonly float WaveSizeMultiplier;

        /// <summary>Chance that a regular enemy of the first wave (rolled on the map from the day table) comes as a night variant.</summary>
        public readonly float VariantChance;

        /// <summary>Night variant health multiplier.</summary>
        public readonly float HealthMultiplier;

        /// <summary>Night variant damage bonus (PercentAdditive AttackDamage, 0.2 = +20%).</summary>
        public readonly float DamageBonus;

        /// <summary>Night variant detection radius multiplier (it notices the hero from farther away).</summary>
        public readonly float AggroRadiusMultiplier;

        /// <summary>Multiplier on the arena's "Prepare-se!" aggro hold for night variants and reinforcement waves (lower = engages sooner).</summary>
        public readonly float AggroHoldMultiplier;

        /// <summary>Kill XP multiplier of a night variant (1.5 = its normal XP + 50% on top).</summary>
        public readonly float ExperienceMultiplier;

        /// <summary>Extra rolls of the variant's own loot (its gold + the biome table) on death.</summary>
        public readonly int ExtraLootRolls;

        public NightDangerProfile(int reinforcementWaves, float waveIntervalSeconds, float waveSizeMultiplier, float variantChance,
            float healthMultiplier, float damageBonus, float aggroRadiusMultiplier, float aggroHoldMultiplier,
            float experienceMultiplier, int extraLootRolls)
        {
            ReinforcementWaves = Mathf.Max(0, reinforcementWaves);
            WaveIntervalSeconds = Mathf.Max(0f, waveIntervalSeconds);
            WaveSizeMultiplier = Mathf.Max(0f, waveSizeMultiplier);
            VariantChance = Mathf.Clamp01(variantChance);
            HealthMultiplier = Mathf.Max(0.1f, healthMultiplier);
            DamageBonus = damageBonus;
            AggroRadiusMultiplier = Mathf.Max(0.1f, aggroRadiusMultiplier);
            AggroHoldMultiplier = Mathf.Clamp01(aggroHoldMultiplier);
            ExperienceMultiplier = Mathf.Max(1f, experienceMultiplier);
            ExtraLootRolls = Mathf.Max(0, extraLootRolls);
        }

        /// <summary>Real seconds-per-wave rate relative to the day (day interval / this interval).</summary>
        public float SpawnRateVersusDay => WaveIntervalSeconds > 0f ? NightDangerRules.Day.WaveIntervalSeconds / WaveIntervalSeconds : 1f;
    }

    /// <summary>
    /// "A noite é mais perigosa": the per-phase multipliers of the encounter arena, pure and EditMode-tested
    /// (<c>NightDangerRulesTests</c>). Read by <c>EncounterArenaController</c> (reinforcement waves, sampled when a
    /// wave is cleared, so a phase change mid-fight takes effect on the next wave) and by <see cref="NightVariantTag"/>
    /// (variant stats/rewards). Boss challenges, fights with a boss, the elite ambush's elite and the very first fight
    /// (tutorial) get no reinforcements and no variants (<see cref="AllowsReinforcements"/>).
    /// <para>
    /// Online note (shared open world, later): the same rules apply per map instance with the phase coming from the
    /// server clock instead of the local <see cref="TimeOfDayManager"/>; every number here is a pure function of the
    /// phase, so server and clients agree as long as they agree on the phase.
    /// </para>
    /// Numbers documented in Docs/Balance/README.md ("Dia e noite") and Docs/DayNight.md.
    /// </summary>
    public static class NightDangerRules
    {
        /// <summary>No reinforcement wave ever exceeds this many enemies (arena readability on a phone).</summary>
        public const int MaxWaveSize = 4;

        /// <summary>Most reinforcement waves any phase can ask for (safety cap for the arena loop).</summary>
        public const int MaxReinforcementWaves = 4;

        //                                           waves interval size  chance  hp    dmg   aggroR hold  xp    loot
        public static readonly NightDangerProfile Day = new NightDangerProfile(1, 4.0f, 1.0f, 0.00f, 1.00f, 0.00f, 1.00f, 1.0f, 1.0f, 0);
        public static readonly NightDangerProfile Dawn = new NightDangerProfile(1, 3.5f, 1.0f, 0.10f, 1.35f, 0.20f, 1.40f, 0.6f, 1.5f, 1);
        public static readonly NightDangerProfile Dusk = new NightDangerProfile(1, 3.5f, 1.0f, 0.15f, 1.35f, 0.20f, 1.40f, 0.6f, 1.5f, 1);
        public static readonly NightDangerProfile Night = new NightDangerProfile(2, 2.5f, 1.5f, 0.50f, 1.35f, 0.20f, 1.40f, 0.6f, 1.5f, 1);

        /// <summary>
        /// Stats a night variant gets wherever it spawns (a dawn/dusk straggler is the same creature as a midnight one):
        /// health, damage, aggro radius/hold, XP and loot of the <see cref="Night"/> profile.
        /// </summary>
        public static NightDangerProfile VariantStats => Night;

        public static NightDangerProfile For(TimeOfDayPhase phase)
        {
            switch (phase)
            {
                case TimeOfDayPhase.Dawn: return Dawn;
                case TimeOfDayPhase.Dusk: return Dusk;
                case TimeOfDayPhase.Night: return Night;
                default: return Day;
            }
        }

        public static bool IsNight(TimeOfDayPhase phase) => phase == TimeOfDayPhase.Night;

        /// <summary>
        /// Whether the arena may send reinforcement waves (and dress night variants): never for a region boss challenge,
        /// a group with a boss, an elite ambush, or before the first victory (the first fight is the tutorial).
        /// </summary>
        public static bool AllowsReinforcements(bool isBossChallenge, bool hasBoss, bool isEliteAmbush, int totalVictories)
        {
            return !isBossChallenge && !hasBoss && !isEliteAmbush && totalVictories >= 1;
        }

        /// <summary>
        /// Whether the first wave's regular enemies may come as night variants: not in a boss challenge or a group with a
        /// boss, not before the first victory. The elite ambush keeps its elite as is (the arena skips it), its minion may
        /// be a variant.
        /// </summary>
        public static bool AllowsNightVariants(bool isBossChallenge, bool hasBoss, int totalVictories)
        {
            return !isBossChallenge && !hasBoss && totalVictories >= 1;
        }

        /// <summary>Whether another wave comes after wave <paramref name="wavesSpawned"/> (1 = only the first so far) was cleared during <paramref name="phase"/>.</summary>
        public static bool ShouldSpawnAnotherWave(int wavesSpawned, TimeOfDayPhase phase)
        {
            int reinforcements = Mathf.Min(MaxReinforcementWaves, For(phase).ReinforcementWaves);
            return wavesSpawned >= 1 && wavesSpawned < 1 + reinforcements;
        }

        /// <summary>Enemies in a reinforcement wave during <paramref name="phase"/>, given the first wave's regular count.</summary>
        public static int WaveSize(int firstWaveRegularCount, TimeOfDayPhase phase)
        {
            int baseCount = Mathf.Max(1, firstWaveRegularCount);
            int size = Mathf.RoundToInt(baseCount * For(phase).WaveSizeMultiplier);
            return Mathf.Clamp(size, 1, MaxWaveSize);
        }

        /// <summary>Seconds from a wave being cleared to the next one during <paramref name="phase"/>.</summary>
        public static float WaveInterval(TimeOfDayPhase phase) => For(phase).WaveIntervalSeconds;

        /// <summary>Aggro hold of a reinforcement wave (the arena's intro grace × the phase's hold multiplier).</summary>
        public static float ReinforcementAggroHold(float introGraceSeconds, TimeOfDayPhase phase) =>
            Mathf.Max(0f, introGraceSeconds) * For(phase).AggroHoldMultiplier;

        /// <summary>Total waves an encounter has if the whole fight stays in <paramref name="phase"/> (for docs/tests).</summary>
        public static int TotalWaves(TimeOfDayPhase phase) => 1 + Mathf.Min(MaxReinforcementWaves, For(phase).ReinforcementWaves);

        /// <summary>Deterministic variant roll for the first wave's enemy <paramref name="index"/> (seeded by the payload).</summary>
        public static bool RollVariant(TimeOfDayPhase phase, System.Random random)
        {
            float chance = For(phase).VariantChance;
            return random != null && chance > 0f && random.NextDouble() < chance;
        }
    }
}
