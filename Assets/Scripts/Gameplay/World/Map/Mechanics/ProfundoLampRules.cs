using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>The Tide of Darkness' moment (<see cref="ProfundoLampRules.PhaseAt"/>).</summary>
    public enum ProfundoTidePhase
    {
        Calm = 0,
        /// <summary>The shadow is coming: time to get under the lamps.</summary>
        Warning = 1,
        /// <summary>The shadow is up: it snuffs lamps and bites whoever is out of the light.</summary>
        Rising = 2,
    }

    /// <summary>
    /// The lamps of the network, pure (no scene, ready for a server): each is lit or out, and standing by one tends it until it catches.
    /// The rings count from the inside: a ring holds with 3 of its 4 lamps lit and only while every ring inside it holds.
    /// </summary>
    public sealed class ProfundoLampNetwork
    {
        private readonly bool[] _lit;
        private int _tended = -1;
        private float _progress;

        /// <summary>A network of <paramref name="count"/> lamps with the innermost ring burning (the garrison keeps that one).</summary>
        public ProfundoLampNetwork(int count)
        {
            _lit = new bool[count];
            for (int i = 0; i < count && i < ProfundoLampRules.LampsPerRing; i++)
            {
                _lit[i] = true;
            }
        }

        public int Count => _lit.Length;

        public bool IsLit(int index) => _lit[index];

        /// <summary>How far the lamp being tended is (0–1), for the feedback.</summary>
        public float Progress => _tended >= 0 ? Mathf.Clamp01(_progress / ProfundoLampRules.KindleSeconds) : 0f;

        public int LitCount
        {
            get
            {
                int lit = 0;
                foreach (bool lamp in _lit)
                {
                    lit += lamp ? 1 : 0;
                }

                return lit;
            }
        }

        /// <summary>Tends lamp <paramref name="here"/> (−1: nobody at a lamp, which drops any tending). True on the frame the lamp catches.</summary>
        public bool Tend(int here, float deltaTime)
        {
            if (here < 0 || here >= _lit.Length || _lit[here])
            {
                _tended = -1;
                _progress = 0f;
                return false;
            }

            if (here != _tended)
            {
                _tended = here;
                _progress = 0f;
            }

            _progress += deltaTime;
            if (_progress < ProfundoLampRules.KindleSeconds)
            {
                return false;
            }

            _lit[here] = true;
            _tended = -1;
            _progress = 0f;
            return true;
        }

        /// <summary>The tide puts out the outermost lamp still burning; false when none is.</summary>
        public bool SnuffOutermost()
        {
            for (int i = _lit.Length - 1; i >= 0; i--)
            {
                if (_lit[i])
                {
                    _lit[i] = false;
                    return true;
                }
            }

            return false;
        }

        /// <summary>How many rings hold, counted from the inside (0–3).</summary>
        public int RingsHeld
        {
            get
            {
                int rings = 0;
                for (int ring = 0; ring < ProfundoLampRules.RingCount; ring++)
                {
                    int lit = 0;
                    for (int i = 0; i < ProfundoLampRules.LampsPerRing; i++)
                    {
                        int index = ring * ProfundoLampRules.LampsPerRing + i;
                        lit += index < _lit.Length && _lit[index] ? 1 : 0;
                    }

                    if (lit < ProfundoLampRules.RingQuorum)
                    {
                        break;
                    }

                    rings++;
                }

                return rings;
            }
        }
    }

    /// <summary>
    /// The lamp network and the Tide of Darkness of the Fortaleza da Última Lâmpada (Docs/Mapas100.md, map 83: "lâmpadas acesas pelos jogadores
    /// aumentam a zona segura da fortaleza (até 3 anéis); apagadas, ela encolhe"; "a sombra sobe; manter as lâmpadas da muralha acesas"; "à noite do
    /// relógio a Maré de Treva é mais forte"), pure. The plaza is always safe, the tide always warns first and it can never kill.
    /// <code>
    /// 12 lamps, 3 rings of 4 · tend a lamp 2 s within 3.5 m to light it · a ring holds with 3 of 4 lit, inner rings first
    /// safe radius from the plaza: 0 rings 35 m · 1 ring 80 m · 2 rings 130 m · 3 rings 190 m · a lit lamp also shelters 10 m round it
    /// tide (240 s): calm 180 s · warning 20 s · rising 40 s · reach 320 m from the plaza (all of it map 83)
    /// rising: one lamp out every 8 s (5 a tide), every 5 s at night (8 a tide), outermost first
    /// bite out of the light: 1 tick per 2 s, 2 % of max life (3 % at night), down to 30 % of max life at most; the first tick only warns
    /// the tide ends with at least 1 ring held: the Mother Lamp gives back 20 % of max life
    /// </code>
    /// </summary>
    public static class ProfundoLampRules
    {
        public const int RingCount = 3;
        public const int LampsPerRing = 4;
        public const int LampCount = RingCount * LampsPerRing;
        public const int RingQuorum = 3;
        public const float KindleSeconds = 2f;
        public const float KindleReach = 3.5f;
        public const float LampShelter = 10f;
        public const float TideReach = 320f;

        public const float CalmSeconds = 180f;
        public const float WarningSeconds = 20f;
        public const float RisingSeconds = 40f;
        public const float CycleSeconds = CalmSeconds + WarningSeconds + RisingSeconds;

        public const float SnuffSeconds = 8f;
        public const float NightSnuffSeconds = 5f;

        public const float BiteTickSeconds = 2f;
        public const float BiteFraction = 0.02f;
        public const float NightBiteFraction = 0.03f;
        public const float BiteFloorFraction = 0.3f;
        public const float HeldHealFraction = 0.2f;

        /// <summary>The safe radius (m from the plaza) for 0, 1, 2 and 3 rings held; each one reaches past the lamps of its ring.</summary>
        private static readonly float[] SafeRadii = { 35f, 80f, 130f, 190f };

        public static float SafeRadius(int ringsHeld) => SafeRadii[Mathf.Clamp(ringsHeld, 0, RingCount)];

        public static float NoRingsRadius => SafeRadii[0];

        public static int TideIndex(float time) => Mathf.FloorToInt(time / CycleSeconds);

        private static float CycleTime(float time) => Mathf.Repeat(time, CycleSeconds);

        public static ProfundoTidePhase PhaseAt(float time)
        {
            float t = CycleTime(time);
            return t < CalmSeconds ? ProfundoTidePhase.Calm : t < CalmSeconds + WarningSeconds ? ProfundoTidePhase.Warning : ProfundoTidePhase.Rising;
        }

        /// <summary>Seconds since the phase at <paramref name="time"/> began.</summary>
        public static float ElapsedInPhase(float time)
        {
            float t = CycleTime(time);
            return t < CalmSeconds ? t : t < CalmSeconds + WarningSeconds ? t - CalmSeconds : t - CalmSeconds - WarningSeconds;
        }

        /// <summary>How many lamps the tide has put out after <paramref name="elapsed"/> seconds of rising.</summary>
        public static int SnuffsDue(float elapsed, bool night) => Mathf.FloorToInt(Mathf.Max(0f, elapsed) / (night ? NightSnuffSeconds : SnuffSeconds));

        /// <summary>The bite of one tide tick.</summary>
        public static float TideBite(float currentHealth, float maxHealth, bool night) => ProfundoHazardRules.Bite(currentHealth, maxHealth, night ? NightBiteFraction : BiteFraction, BiteFloorFraction);

        public static float BiteFractionAt(bool night) => night ? NightBiteFraction : BiteFraction;

        /// <summary>The lamp within kindling reach of <paramref name="p"/> (the closest), or −1.</summary>
        public static int LampAt(IReadOnlyList<Vector2> lamps, Vector2 p)
        {
            int best = -1;
            float bestDistance = KindleReach * KindleReach;
            for (int i = 0; i < lamps.Count; i++)
            {
                float d = (lamps[i] - p).sqrMagnitude;
                if (d <= bestDistance)
                {
                    bestDistance = d;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>Whether a lit lamp shelters <paramref name="p"/>.</summary>
        public static bool ShelteredByLamp(IReadOnlyList<Vector2> lamps, ProfundoLampNetwork network, Vector2 p)
        {
            for (int i = 0; i < lamps.Count; i++)
            {
                if (network.IsLit(i) && (lamps[i] - p).sqrMagnitude <= LampShelter * LampShelter)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Whether the rising tide can bite at <paramref name="p"/> (region coordinates, <paramref name="plaza"/> the fortress' centre).</summary>
        public static bool Exposed(IReadOnlyList<Vector2> lamps, ProfundoLampNetwork network, Vector2 plaza, Vector2 p)
        {
            float distance = Vector2.Distance(plaza, p);
            return distance <= TideReach && distance > SafeRadius(network.RingsHeld) && !ShelteredByLamp(lamps, network, p);
        }

        /// <summary>The front of the shadow closing in during the warning: from the reach down to the safe radius.</summary>
        public static float FrontRadius(float elapsedInWarning, float safeRadius) => Mathf.Lerp(TideReach, safeRadius, Mathf.Clamp01(elapsedInWarning / WarningSeconds));

        /// <summary>Whether the wall held when the tide ended.</summary>
        public static bool Held(int ringsHeld) => ringsHeld >= 1;
    }
}
