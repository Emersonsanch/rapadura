using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Pure rules of the five Fontes do Lodo of the Coração do Lodo (map 35, Docs/Mapas100.md): the black mud round the Olho do Lodo
    /// (<c>OverworldLayouts.LodoArena</c>) covers the spiral of islands, bites and drags the hero toward the eye; each fountain purified
    /// with [E] (<see cref="PurifySeconds"/> standing by it) burns the beasts round it, is a safe zone for <see cref="LitSeconds"/> and
    /// shrinks the mud by a fifth. With the five lit the mud is gone and the Lodo-que-Engole rises in the eye
    /// (<c>RegionBossRules.MireSummonReady</c>); each one that goes out during the fight makes it stronger (<c>RegionBossRules.MireFountainMultiplier</c>).
    /// At night the mud grows by itself (<see cref="MudNightGrowth"/>) and only the clean fountains give light.
    /// Lighting the five together the first time in a game day gives a Pedra da Água Viva (<see cref="RewardItemId"/>, a daily mark).
    /// </summary>
    public static class LodoFountainRules
    {
        /// <summary>The fountains to purify (the ficha's five).</summary>
        public const int Count = 5;

        /// <summary>Metres from a fountain where [E] purifies it and the purification holds.</summary>
        public const float PurifyReach = 4f;

        /// <summary>Seconds standing by the fountain from [E] until it is clean.</summary>
        public const float PurifySeconds = 3f;

        /// <summary>How long a clean fountain stays lit (the ficha: a temporary safe zone of 2 min).</summary>
        public const float LitSeconds = 120f;

        /// <summary>A lit fountain can be renewed (back to <see cref="LitSeconds"/>) once this little or less is left.</summary>
        public const float RenewWindowSeconds = 60f;

        /// <summary>Radius of the safe zone round a lit fountain: no mud bite, no drag, the beasts inside burn.</summary>
        public const float SafeRadius = 10f;

        /// <summary>Radius of the black mud round the eye with no fountain lit: it reaches the first fountain of the spiral.</summary>
        public const float MudBaseRadius = 200f;

        /// <summary>The mud grows by itself at night (the ficha's "Noite").</summary>
        public const float MudNightGrowth = 1.2f;

        /// <summary>Seconds between bites of the mud.</summary>
        public const float BiteTickSeconds = 3f;

        /// <summary>A bite of the mud, as a fraction of max life (the hazards' ceiling of 4%).</summary>
        public const float BiteFraction = 0.04f;

        /// <summary>The mud never takes the hero below this share of max life.</summary>
        public const float BiteFloorFraction = 0.3f;

        /// <summary>Metres per second the mud drags the hero toward the eye.</summary>
        public const float PullSpeed = 1.2f;

        /// <summary>Within this distance of the eye the drag stops (the stone ring).</summary>
        public const float PullStopRadius = 12f;

        /// <summary>The light of a fountain as it comes clean: this share of max life on each beast in its safe zone.</summary>
        public const float BurstFraction = 0.3f;

        /// <summary>Seconds between the pulses of a lit fountain on the beasts inside its safe zone.</summary>
        public const float PulseSeconds = 2f;

        public const float PulseFraction = 0.05f;

        /// <summary>The prize for the five lit together, once a game day: the Pedra da Água Viva (the ficha's loot).</summary>
        public const string RewardItemId = "item_pedra_vida";

        /// <summary>WorldMapProgress daily mark: the game day the prize was last given.</summary>
        public const string RewardMarkKey = "delta.lodo.fountains";

        public static bool IsLit(float litUntil, float now) => now < litUntil;

        /// <summary>How many fountains are lit at <paramref name="now"/> (null: none).</summary>
        public static int LitCount(float[] litUntil, float now)
        {
            int lit = 0;
            for (int i = 0; litUntil != null && i < litUntil.Length; i++)
            {
                if (IsLit(litUntil[i], now))
                {
                    lit++;
                }
            }

            return lit;
        }

        /// <summary>Can [E] purify a fountain lit until <paramref name="litUntil"/>? An unlit one always; a lit one only in its last <see cref="RenewWindowSeconds"/>.</summary>
        public static bool CanPurify(float litUntil, float now) => litUntil - now <= RenewWindowSeconds;

        /// <summary>When a fountain purified at <paramref name="now"/> goes out.</summary>
        public static float LitUntilAfterPurify(float now) => now + LitSeconds;

        /// <summary>How far the black mud reaches with <paramref name="lit"/> fountains lit: a fifth less per fountain, none with the five.</summary>
        public static float MudRadius(int lit, bool isNight)
        {
            int left = Count - Mathf.Clamp(lit, 0, Count);
            return MudBaseRadius * left / Count * (isNight ? MudNightGrowth : 1f);
        }

        /// <summary>The fountain within <see cref="PurifyReach"/> of <paramref name="hero"/> (the nearest), or -1.</summary>
        public static int FountainAt(Vector2[] fountains, Vector2 hero)
        {
            int best = -1;
            float bestSq = PurifyReach * PurifyReach;
            for (int i = 0; fountains != null && i < fountains.Length; i++)
            {
                float sq = (fountains[i] - hero).sqrMagnitude;
                if (sq <= bestSq)
                {
                    best = i;
                    bestSq = sq;
                }
            }

            return best;
        }

        /// <summary>Is <paramref name="point"/> in the safe zone of a lit fountain?</summary>
        public static bool InSafeZone(Vector2[] fountains, float[] litUntil, float now, Vector2 point)
        {
            for (int i = 0; fountains != null && litUntil != null && i < fountains.Length && i < litUntil.Length; i++)
            {
                if (IsLit(litUntil[i], now) && (fountains[i] - point).sqrMagnitude <= SafeRadius * SafeRadius)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Is the hero in the mud (inside its radius round <paramref name="eye"/>, outside every safe zone)?</summary>
        public static bool InMud(Vector2 eye, float mudRadius, bool inSafeZone, Vector2 hero) =>
            !inSafeZone && mudRadius > 0f && (hero - eye).sqrMagnitude <= mudRadius * mudRadius;

        /// <summary>The drag of the mud this frame: toward the eye at <see cref="PullSpeed"/>, nothing inside <see cref="PullStopRadius"/>.</summary>
        public static Vector2 Pull(Vector2 eye, Vector2 hero, float deltaTime)
        {
            Vector2 toEye = eye - hero;
            float distance = toEye.magnitude;
            if (distance <= PullStopRadius || deltaTime <= 0f)
            {
                return Vector2.zero;
            }

            return toEye / distance * Mathf.Min(PullSpeed * deltaTime, distance - PullStopRadius);
        }

        /// <summary>What a fountain's light does to a beast of <paramref name="maxHealth"/>: the burst as it comes clean, or a pulse. Bosses are not touched.</summary>
        public static float BeastStrike(float maxHealth, bool isBoss, bool burst) =>
            isBoss ? 0f : Mathf.Max(0f, maxHealth) * (burst ? BurstFraction : PulseFraction);

        /// <summary>Was the prize already given on game day <paramref name="today"/>?</summary>
        public static bool RewardedToday(int markedDay, int today) => markedDay == today;

        /// <summary>Does lighting this fountain give the prize? Only when it makes the five and the day's prize is not taken.</summary>
        public static bool GivesReward(int litBefore, int litAfter, int markedDay, int today) =>
            litBefore < Count && litAfter >= Count && !RewardedToday(markedDay, today);
    }

    /// <summary>
    /// Which Fontes do Lodo are lit right now (by <c>Time.time</c>), shared by the Delta driver that lights them and the world boss that
    /// waits for them (<c>RegionBossDirector</c>, <c>RegionBossAbilities</c>). Not saved: a lit fountain lasts two minutes; the driver
    /// clears it when it turns off (the hero left the Delta or the scene changed).
    /// </summary>
    public static class LodoFountainBoard
    {
        private static readonly float[] s_litUntil = NewBoard();

        /// <summary>The lit-until time of each fountain (read only: <see cref="Light"/> and <see cref="Clear"/> write it).</summary>
        public static float[] LitUntil => s_litUntil;

        public static int LitCount(float now) => LodoFountainRules.LitCount(s_litUntil, now);

        public static void Light(int fountain, float now)
        {
            if (fountain >= 0 && fountain < s_litUntil.Length)
            {
                s_litUntil[fountain] = LodoFountainRules.LitUntilAfterPurify(now);
            }
        }

        public static void Clear()
        {
            for (int i = 0; i < s_litUntil.Length; i++)
            {
                s_litUntil[i] = float.NegativeInfinity;
            }
        }

        private static float[] NewBoard()
        {
            var board = new float[LodoFountainRules.Count];
            for (int i = 0; i < board.Length; i++)
            {
                board[i] = float.NegativeInfinity;
            }

            return board;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay() => Clear();
    }
}
