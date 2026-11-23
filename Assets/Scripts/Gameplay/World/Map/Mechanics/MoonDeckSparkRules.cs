using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>What a tick of <see cref="MoonDeckSparkSession"/> brought.</summary>
    public enum MoonSparkEvent
    {
        None = 0,
        /// <summary>The spark agreed to come: it follows the hero now.</summary>
        Convinced = 1,
        /// <summary>The hero struck while talking to it: it hides for <see cref="MoonDeckSparkRules.SulkSeconds"/>.</summary>
        Sulked = 2,
        /// <summary>The hero walked off before it agreed.</summary>
        Cancelled = 3,
        /// <summary>The spark slipped back to its hiding place (the hero left the ship or fell).</summary>
        Dropped = 4,
        /// <summary>The spark reached the deck lantern and lit it.</summary>
        Delivered = 5,
    }

    /// <summary>
    /// The stubborn sparks of the hold and the moon that grows (Docs/Mapas100.md, map 93: "Faísca Teimosa (faísca que não quer ir para casa; não se bate:
    /// se convence)"; "a lua cresce: entregar faíscas enche o convés de lanternas; na lua cheia o convés vira festa"), pure. Five sparks hide in the Porão
    /// da Lua. [E] beside one starts talking; staying close without striking (no basic attack, no skill) convinces it, striking makes it sulk and hide a
    /// while. A convinced spark follows the hero up the deck to the lantern of the Cabine da Proa and lights a lantern there for the game day; each
    /// lantern makes the deck's moon grow, and with all five the moon is full and the deck holds its feast, once a game day.
    /// <code>
    /// 5 sparks · talk from 4 m, stay within 6 m for 6 s without striking · striking: sulks 20 s · one spark carried at a time
    /// carried: drops back to its hiding place if the hero leaves the Moon Ship (region z 740–1240) or falls · deliver within 12 m of the lantern
    /// each spark: one lantern for the game day (daily mark), heals 10% · 5 lit: full moon, feast once a day: full heal + 1 Runa da Clareza
    /// </code>
    /// </summary>
    public static class MoonDeckSparkRules
    {
        public const int SparkCount = 5;
        public const float TalkReach = 4f;
        public const float StayReach = 6f;
        public const float ConvinceSeconds = 6f;
        public const float SulkSeconds = 20f;
        public const float DeliverReach = 12f;
        public const float DeliverHealFraction = 0.1f;
        public const string FeastItemId = "item_runa_clareza";
        public const string FeastMarkKey = "ceu.moon_deck.feast";

        /// <summary>The lantern of the Cabine da Proa (the deck's post, region coordinates).</summary>
        public static readonly Vector2 Lantern = new Vector2(0f, 1000f);

        /// <summary>The rows of the Moon Ship (prow to stern, region coordinates): a carried spark stays only while the hero is aboard.</summary>
        public static readonly Rect ShipArea = Rect.MinMaxRect(-1200f, 740f, 1200f, 1240f);

        /// <summary>The hiding places in the Porão da Lua, beside the x = 480 road and the deck lane, away from the Clandestino Grande's ring (468, 836).</summary>
        public static readonly Vector2[] HidingPlaces =
        {
            new Vector2(494f, 780f),
            new Vector2(466f, 930f),
            new Vector2(560f, 1012f),
            new Vector2(650f, 988f),
            new Vector2(492f, 1120f),
        };

        public static string MarkKey(int spark) => "ceu.moon_spark." + spark;

        public static bool LitToday(int markDay, int today) => markDay > 0 && markDay == today;

        public static int LitCount(IReadOnlyList<int> markDays, int today)
        {
            int count = 0;
            for (int i = 0; i < markDays.Count; i++)
            {
                count += LitToday(markDays[i], today) ? 1 : 0;
            }

            return count;
        }

        public static bool IsFullMoon(int lit) => lit >= SparkCount;

        /// <summary>Whether the feast is still due today (full moon, not yet held this game day).</summary>
        public static bool FeastDue(int lit, int feastMarkDay, int today) => IsFullMoon(lit) && !LitToday(feastMarkDay, today);

        /// <summary>The nearest spark within <see cref="TalkReach"/> that can be talked to now (not lit today, not sulking), or −1.</summary>
        public static int SparkNear(IReadOnlyList<Vector2> places, Vector2 hero, IReadOnlyList<int> markDays, int today, IReadOnlyList<float> sulkUntil, float now)
        {
            int best = -1;
            float bestSqr = TalkReach * TalkReach;
            for (int i = 0; i < places.Count; i++)
            {
                float sqr = (places[i] - hero).sqrMagnitude;
                if (sqr > bestSqr || LitToday(markDays[i], today) || now < sulkUntil[i])
                {
                    continue;
                }

                bestSqr = sqr;
                best = i;
            }

            return best;
        }

        public static float DeliverHeal(float max) => Mathf.Max(0f, max) * DeliverHealFraction;
    }

    /// <summary>One hero with the sparks of the hold: which one is being talked to or carried, and which sulk until when. No Unity object: it can run on the server.</summary>
    public sealed class MoonDeckSparkSession
    {
        private readonly float[] _sulkUntil = new float[MoonDeckSparkRules.SparkCount];
        private float _talk;

        /// <summary>The spark being talked to, or −1.</summary>
        public int Talking { get; private set; } = -1;

        /// <summary>The spark following the hero, or −1.</summary>
        public int Carrying { get; private set; } = -1;

        public float TalkFraction => Mathf.Clamp01(_talk / MoonDeckSparkRules.ConvinceSeconds);

        public IReadOnlyList<float> SulkUntil => _sulkUntil;

        public bool Busy => Talking >= 0 || Carrying >= 0;

        /// <summary>Starts talking to a spark; false while another is being talked to or carried, or the index is invalid.</summary>
        public bool BeginTalk(int spark)
        {
            if (Busy || spark < 0 || spark >= _sulkUntil.Length)
            {
                return false;
            }

            Talking = spark;
            _talk = 0f;
            return true;
        }

        /// <summary>One step while talking. <paramref name="struck"/>: the hero struck (basic attack or skill) since the last step.</summary>
        public MoonSparkEvent TickTalk(float deltaTime, bool inStay, bool struck, float now)
        {
            if (Talking < 0)
            {
                return MoonSparkEvent.None;
            }

            if (struck)
            {
                _sulkUntil[Talking] = now + MoonDeckSparkRules.SulkSeconds;
                Talking = -1;
                return MoonSparkEvent.Sulked;
            }

            if (!inStay)
            {
                Talking = -1;
                return MoonSparkEvent.Cancelled;
            }

            _talk += Mathf.Max(0f, deltaTime);
            if (_talk < MoonDeckSparkRules.ConvinceSeconds)
            {
                return MoonSparkEvent.None;
            }

            Carrying = Talking;
            Talking = -1;
            return MoonSparkEvent.Convinced;
        }

        /// <summary>One step while carrying: it drops off the ship or when the hero falls, and lights the lantern on arrival.</summary>
        public MoonSparkEvent TickCarry(bool aboard, bool alive, bool atLantern)
        {
            if (Carrying < 0)
            {
                return MoonSparkEvent.None;
            }

            if (!aboard || !alive)
            {
                Carrying = -1;
                return MoonSparkEvent.Dropped;
            }

            if (!atLantern)
            {
                return MoonSparkEvent.None;
            }

            Carrying = -1;
            return MoonSparkEvent.Delivered;
        }

        public void Cancel()
        {
            Talking = -1;
            Carrying = -1;
            _talk = 0f;
        }
    }
}
