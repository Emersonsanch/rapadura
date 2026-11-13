using System;
using Rapadura.Core.Events;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The Rio das Doze Horas is always night (Docs/Mapas100.md 71: "sempre noite aqui (o rio é a noite)"; the region's beasts already use their
    /// night groups), pure. Inside the region the sky is held towards midnight (<c>EnvironmentOverrides.NightHold</c>) whatever the clock says;
    /// the hold eases in and out over 2 s at the border so the sky never snaps.
    /// </summary>
    public static class RioEverNightRules
    {
        /// <summary>How fast the hold moves (per second): 0 to 1 in 2 s.</summary>
        public const float EasePerSecond = 0.5f;

        /// <summary>The hold after <paramref name="deltaTime"/> moving towards 1 inside the region and towards 0 outside.</summary>
        public static float Step(float hold, bool inside, float deltaTime) =>
            Mathf.MoveTowards(Mathf.Clamp01(hold), inside ? 1f : 0f, EasePerSecond * Mathf.Max(0f, deltaTime));
    }

    /// <summary>
    /// The low gravity of the Cais da Coluna (Docs/Mapas100.md 91: "pouca gravidade: pulo 1,5× mais alto; quedas lentas"), pure. Gravity is
    /// scaled by 2/3 on the whole of map 91 (the quay and its two piers, region coordinates x −1200…1200, z −360…240): the same take-off speed
    /// then climbs 1.5× as high and every fall is slower.
    /// </summary>
    public static class CeuLowGravityRules
    {
        public const float JumpHeightFactor = 1.5f;

        /// <summary>The gravity multiplier: a jump of h with the same take-off speed reaches h / scale.</summary>
        public const float GravityScale = 1f / JumpHeightFactor;

        /// <summary>Map 91 in the region's own coordinates (its five areas).</summary>
        public static readonly Rect Map91 = Rect.MinMaxRect(-1200f, -360f, 1200f, 240f);

        public static bool InLowGravity(Vector2 local) => Map91.Contains(local);

        public static float ScaleAt(Vector2 local) => InLowGravity(local) ? GravityScale : 1f;

        /// <summary>The jump height reached with the take-off speed of <paramref name="baseHeight"/> under gravity × <paramref name="scale"/>.</summary>
        public static float JumpHeight(float baseHeight, float scale) => scale <= 0f ? baseHeight : baseHeight / scale;
    }

    /// <summary>A Tide of Darkness of the Fortaleza da Última Lâmpada ended near the hero (<c>ProfundoMechanicsDriver</c>), with the rings that held.</summary>
    public readonly struct ProfundoTideEndedEvent : IGameEvent
    {
        public readonly int Rings;

        public ProfundoTideEndedEvent(int rings)
        {
            Rings = rings;
        }
    }

    /// <summary>
    /// The pay of the Maré de Treva (Docs/Mapas100.md 83: "a sombra sobe; manter as lâmpadas da muralha acesas"), pure: the tide itself (every
    /// 240 s, stronger at the clock's night) is <see cref="ProfundoLampRules"/>; the first tide of each real hour that ends with rings still
    /// holding is paid by Pamour: 20 gold per ring held, all three: +1 Pedra-Faísca. A tide that fell (no ring) pays nothing and leaves the hour
    /// open for the next one.
    /// </summary>
    public static class ProfundoTideRewardRules
    {
        public const int GoldPerRing = 20;
        public const int AllRingsStones = 1;

        /// <summary>Save key (<see cref="WorldMapProgress"/> time mark): the last real hour paid, as <see cref="PaidMark"/>.</summary>
        public const string PaidKey = "city_event.mare_de_treva.paid";

        /// <summary>The real hour (UTC hours since 1970) of <paramref name="utc"/>.</summary>
        public static long HourIndex(DateTime utc) => (long)Math.Floor((utc - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalHours);

        public static long PaidMark(long hourIndex) => hourIndex + 1L;

        public static bool IsPaid(long mark, long hourIndex) => mark == PaidMark(hourIndex);

        public static bool ShouldPay(int rings, long mark, long hourIndex) => rings > 0 && !IsPaid(mark, hourIndex);

        public static int GoldFor(int rings) => Mathf.Clamp(rings, 0, ProfundoLampRules.RingCount) * GoldPerRing;

        public static int StonesFor(int rings) => rings >= ProfundoLampRules.RingCount ? AllRingsStones : 0;
    }
}
