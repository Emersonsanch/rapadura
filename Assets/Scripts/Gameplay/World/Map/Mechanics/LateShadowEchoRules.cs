using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The Sombras Atrasadas double at night in the Deserto do Relógio (Docs/Mapas100.md, map 78: "Sombra Atrasada (a sombra que chega depois
    /// do bicho)"; "Noite: os gnômons não fazem sombra; as bolhas somem; Sombras Atrasadas dobram"), pure. At night every beast that falls in the
    /// desert (not a boss, not a late shadow itself) leaves its shadow behind: it rises where the beast fell a moment later, warned by a ring.
    /// A late shadow never leaves another one, so the desert can never fill up, and there is a cap on how many wait at once. Dawn takes them all.
    /// <code>
    /// night only · desert of map 78 (region x 360–840, z −720–720) · rises 2.5 s after the fall (ring warns) · at most 4 alive
    /// level: the fallen beast's (fallback 310) · gone at dawn or when the hero is 90 m away
    /// </code>
    /// </summary>
    public static class LateShadowEchoRules
    {
        public const string LateShadowId = "enemy_forest_late_shadow";
        public const float RiseDelaySeconds = 2.5f;
        public const int MaxAlive = 4;
        public const int FallbackLevel = 310;
        public const float LeaveRange = 90f;

        /// <summary>The Desert of the Clock in region coordinates (the three zones of map 78).</summary>
        public static readonly Rect DesertArea = Rect.MinMaxRect(360f, -720f, 840f, 720f);

        /// <summary>Whether a beast that fell at <paramref name="regionPoint"/> leaves a late shadow behind.</summary>
        /// <param name="aliveOrPending">Late shadows already up or about to rise.</param>
        public static bool LeavesShadow(bool night, Vector2 regionPoint, string fallenId, bool fallenIsBoss, bool fallenIsEcho, int aliveOrPending)
        {
            if (!night || fallenIsBoss || fallenIsEcho || aliveOrPending >= MaxAlive)
            {
                return false;
            }

            return DesertArea.Contains(regionPoint);
        }

        /// <summary>The level the shadow rises at: the fallen beast's, or the desert's own when it is unknown.</summary>
        public static int ShadowLevel(int fallenLevel) => fallenLevel > 0 ? fallenLevel : FallbackLevel;

        /// <summary>Whether a waiting shadow goes away: day came, or the hero walked off.</summary>
        public static bool Fades(bool night, float heroDistance) => !night || heroDistance > LeaveRange;
    }
}
