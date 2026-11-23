using System;
using System.Collections.Generic;

namespace Rapadura.Gameplay.TimedEvents
{
    /// <summary>
    /// The role of an event enemy. The server maps each role to an enemy of the room's region (content), so the rule never
    /// names an asset. Never renumber.
    /// </summary>
    public enum TimedEnemyRole
    {
        /// <summary>"Sombra": the common one. Cost 1.</summary>
        Shade = 0,

        /// <summary>"Corredor da Treva": fast, little health. Cost 1.</summary>
        Runner = 1,

        /// <summary>"Sussurrador": ranged, snuffs lamps from afar. Cost 2.</summary>
        Caster = 2,

        /// <summary>"Bruto": slow, heavy hits. Cost 3.</summary>
        Brute = 3,

        /// <summary>"Arauto da Treva": an elite. Cost 6.</summary>
        Elite = 4,

        /// <summary>The archon that closes the event (one per final wave / last assault).</summary>
        Archon = 5,
    }

    /// <summary>How the event is going.</summary>
    public enum TimedEventOutcome
    {
        InProgress = 0,
        Victory = 1,
        Defeat = 2,
    }

    public readonly struct TimedWaveGroup
    {
        public readonly TimedEnemyRole Role;
        public readonly int Count;

        /// <summary>Which gate it comes through (Cerco: 0–2; Vigília: always 0).</summary>
        public readonly int Gate;

        public TimedWaveGroup(TimedEnemyRole role, int count, int gate)
        {
            Role = role;
            Count = count;
            Gate = gate;
        }
    }

    /// <summary>One wave: its index, the enemies' level and its groups (role order, then gate).</summary>
    public sealed class TimedWave
    {
        public readonly int Index;
        public readonly int EnemyLevel;
        public readonly List<TimedWaveGroup> Groups = new List<TimedWaveGroup>();

        public TimedWave(int index, int enemyLevel)
        {
            Index = index;
            EnemyLevel = enemyLevel;
        }

        public int Count(TimedEnemyRole role)
        {
            int total = 0;
            foreach (TimedWaveGroup group in Groups)
            {
                if (group.Role == role)
                {
                    total += group.Count;
                }
            }

            return total;
        }

        public int TotalEnemies
        {
            get
            {
                int total = 0;
                foreach (TimedWaveGroup group in Groups)
                {
                    total += group.Count;
                }

                return total;
            }
        }

        /// <summary>Budget spent (Archon not counted).</summary>
        public int Cost
        {
            get
            {
                int total = 0;
                foreach (TimedWaveGroup group in Groups)
                {
                    total += TimedWaveRules.RoleCost(group.Role) * group.Count;
                }

                return total;
            }
        }
    }

    /// <summary>
    /// Wave composition shared by the Vigília da Coluna (survival) and the Cerco da Treva (defense), pure and seeded:
    /// a budget by wave and room size, spent on the roles unlocked by that wave, with the room level from its band.
    /// Same seed, room, wave and player count → the same wave, on every machine.
    /// </summary>
    public static class TimedWaveRules
    {
        public static int RoleCost(TimedEnemyRole role)
        {
            switch (role)
            {
                case TimedEnemyRole.Shade: return 1;
                case TimedEnemyRole.Runner: return 1;
                case TimedEnemyRole.Caster: return 2;
                case TimedEnemyRole.Brute: return 3;
                case TimedEnemyRole.Elite: return 6;
                default: return 0;
            }
        }

        /// <summary>Per extra player in the room, the budget grows 35%.</summary>
        public const float BudgetPerExtraPlayer = 0.35f;

        /// <summary>Enemy level: band middle − 6, +2 per wave, never above <c>LevelCurveMath.MaxLevel</c> (500).</summary>
        public const int LevelBelowMid = 6;
        public const int LevelPerWave = 2;
        public const int MaxEnemyLevel = 500;

        public static int EnemyLevel(TimedEventRoomBand band, int waveIndex)
        {
            return Math.Max(1, Math.Min(MaxEnemyLevel, band.MidLevel - LevelBelowMid + LevelPerWave * Math.Max(0, waveIndex)));
        }

        /// <summary>Budget of a wave: (base + step × wave) × (1 + 0,35 × extra players), rounded.</summary>
        public static int Budget(int baseBudget, int stepPerWave, int waveIndex, int playerCount)
        {
            float scale = 1f + BudgetPerExtraPlayer * Math.Max(0, playerCount - 1);
            return Math.Max(1, (int)Math.Round((baseBudget + stepPerWave * Math.Max(0, waveIndex)) * scale));
        }

        /// <summary>
        /// Spends <paramref name="budget"/> on the roles unlocked at <paramref name="waveIndex"/> (Runner from wave 1, Caster
        /// from 2, Brute from 3, Elite from 5; rooms 4+ weigh Casters double). Seeded pick per enemy; leftover goes to Shades.
        /// </summary>
        public static TimedWave Compose(TimedEventRoomBand band, int waveIndex, int budget, int seed, int gates, bool archon)
        {
            var wave = new TimedWave(waveIndex, EnemyLevel(band, waveIndex));
            var pool = new List<(TimedEnemyRole Role, int Weight)> { (TimedEnemyRole.Shade, 5) };
            if (waveIndex >= 1) pool.Add((TimedEnemyRole.Runner, 3));
            if (waveIndex >= 2) pool.Add((TimedEnemyRole.Caster, band.Index >= 4 ? 4 : 2));
            if (waveIndex >= 3) pool.Add((TimedEnemyRole.Brute, 2));
            if (waveIndex >= 5) pool.Add((TimedEnemyRole.Elite, 1));

            var counts = new Dictionary<(TimedEnemyRole, int), int>();
            int left = archon ? Math.Max(1, budget / 2) : budget;
            int draw = 0;
            while (left > 0)
            {
                int total = 0;
                foreach (var entry in pool)
                {
                    if (RoleCost(entry.Role) <= left)
                    {
                        total += entry.Weight;
                    }
                }

                TimedEnemyRole role = TimedEnemyRole.Shade;
                double roll = TimedEventAgenda.Roll(seed, waveIndex, draw, 1) * total;
                foreach (var entry in pool)
                {
                    if (RoleCost(entry.Role) > left)
                    {
                        continue;
                    }

                    if (roll < entry.Weight)
                    {
                        role = entry.Role;
                        break;
                    }

                    roll -= entry.Weight;
                }

                int gate = gates <= 1 ? 0 : (int)(TimedEventAgenda.Roll(seed, waveIndex, draw, 2) * gates) % gates;
                counts.TryGetValue((role, gate), out int c);
                counts[(role, gate)] = c + 1;
                left -= RoleCost(role);
                draw++;
            }

            if (archon)
            {
                int gate = gates <= 1 ? 0 : (int)(TimedEventAgenda.Roll(seed, waveIndex, 999, 3) * gates) % gates;
                counts[(TimedEnemyRole.Archon, gate)] = 1;
            }

            var keys = new List<(TimedEnemyRole Role, int Gate)>(counts.Keys);
            keys.Sort((a, b) => a.Role != b.Role ? a.Role.CompareTo(b.Role) : a.Gate.CompareTo(b.Gate));
            foreach (var key in keys)
            {
                wave.Groups.Add(new TimedWaveGroup(key.Role, counts[key], key.Gate));
            }

            return wave;
        }

        /// <summary>Next wave when the field is (almost) clear or the wave timer ran out; never after the last wave.</summary>
        public static bool ShouldStartNextWave(int wavesStarted, int waveCount, double secondsSinceWave, int enemiesAlive, double waveSeconds, int clearThreshold)
        {
            if (wavesStarted >= waveCount)
            {
                return false;
            }

            if (wavesStarted == 0)
            {
                return true;
            }

            return enemiesAlive <= clearThreshold || secondsSinceWave >= waveSeconds;
        }
    }

    /// <summary>
    /// "Vigília da Coluna" (survival, Devil Square spirit): the Coluna da Glória lifts the light of the day to the Moon and
    /// the Treva sends 8 waves to snuff the lamps; the room holds 15 minutes. The 8th wave brings the "Arconte da Vigília".
    /// Victory: the last wave cleared, or the clock runs out with someone standing. Defeat: everybody down at once.
    /// </summary>
    public static class TimedVigiliaRules
    {
        public const int WaveCount = 8;
        public const int BaseBudget = 6;
        public const int BudgetPerWave = 2;

        /// <summary>The next wave comes with ≤ 2 enemies alive or after 100 s.</summary>
        public const double WaveSeconds = 100d;
        public const int ClearThreshold = 2;

        public static TimedWave Wave(TimedEventRoomBand band, int waveIndex, int playerCount, int seed)
        {
            int budget = TimedWaveRules.Budget(BaseBudget, BudgetPerWave, waveIndex, playerCount);
            return TimedWaveRules.Compose(band, waveIndex, budget, seed, 1, waveIndex == WaveCount - 1);
        }

        public static bool ShouldStartNextWave(int wavesStarted, double secondsSinceWave, int enemiesAlive) =>
            TimedWaveRules.ShouldStartNextWave(wavesStarted, WaveCount, secondsSinceWave, enemiesAlive, WaveSeconds, ClearThreshold);

        public static TimedEventOutcome Outcome(int playersAlive, int wavesCleared, bool timeUp)
        {
            if (wavesCleared >= WaveCount)
            {
                return TimedEventOutcome.Victory;
            }

            if (playersAlive <= 0)
            {
                return TimedEventOutcome.Defeat;
            }

            return timeUp ? TimedEventOutcome.Victory : TimedEventOutcome.InProgress;
        }
    }
}
