using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>The state of one strip of grass of the Savana dos Auroques (<see cref="SavannaFireRules"/>).</summary>
    public enum SavannaStripState
    {
        /// <summary>The low fire runs through it now.</summary>
        Burning = 0,

        /// <summary>Burnt a moment ago: black ash, a fast trail.</summary>
        FreshAsh = 1,

        /// <summary>Still ash: a fast trail.</summary>
        Ash = 2,

        /// <summary>Old grass, waiting for its fire.</summary>
        Grass = 3,
    }

    /// <summary>
    /// The low fire of the Savana dos Auroques (Docs/Mapas100.md 26: "fogo baixo: o povo-touro queima o capim velho em faixas (Fogo Vivo que
    /// renova); as faixas queimadas viram trilhas rápidas; os bichos fogem delas"), pure and server-runnable (a function of the clock). Four
    /// long strips of grass north and south of Aratta's ground take turns, five minutes each: burning, fresh ash, ash, old grass, and round
    /// again, so two are always ash. On ash the hero walks 20 % faster; the beasts hate it (on ash or in the fire they move 30 % slower and hit
    /// 15 % softer; bosses are left alone). The fire itself never hurts: it is Living Fire, it only renews.
    /// <code>
    /// 4 strips · 300 s per state: burning → fresh ash → ash → grass · ash: hero speed × 1.2 · beasts on ash or fire: speed × 0.7, damage × 0.85
    /// </code>
    /// </summary>
    public static class SavannaFireRules
    {
        public const double StateSeconds = 300d;
        public const int States = 4;
        public const float HeroAshFactor = 1.2f;
        public const float BeastSpeedFactor = 0.7f;
        public const float BeastDamageFactor = 0.85f;

        public static long TurnIndex(double unixSeconds) => (long)System.Math.Floor(unixSeconds / StateSeconds);

        /// <summary>The state of strip <paramref name="strip"/> at <paramref name="unixSeconds"/>: the strips are one state apart.</summary>
        public static SavannaStripState StateOf(int strip, double unixSeconds)
        {
            long k = (TurnIndex(unixSeconds) + strip) % States;
            return (SavannaStripState)(k < 0 ? k + States : k);
        }

        public static bool IsAsh(SavannaStripState state) => state == SavannaStripState.FreshAsh || state == SavannaStripState.Ash;

        /// <summary>The strip <paramref name="p"/> stands in, or −1.</summary>
        public static int StripAt(IReadOnlyList<Rect> strips, Vector2 p)
        {
            for (int i = 0; strips != null && i < strips.Count; i++)
            {
                if (strips[i].Contains(p))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>The factor on the hero's speed in a strip of <paramref name="state"/> (off the town).</summary>
        public static float HeroSpeedFactor(SavannaStripState state, bool inTown) => !inTown && IsAsh(state) ? HeroAshFactor : 1f;

        /// <summary>Whether a beast in a strip of <paramref name="state"/> shies from it.</summary>
        public static bool BeastShies(SavannaStripState state, bool isBoss) => !isBoss && state != SavannaStripState.Grass;
    }
}
