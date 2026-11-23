using System.Collections.Generic;
using Rapadura.Gameplay.Enemies;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Falling off the Pontes de Vento (Docs/Mapas100.md 55: "quem está numa ponte para e segura"; "Falta: a queda de verdade (a ponte não some)";
    /// "O Cortador de Cordas ... libertada, vira guarda das pontes"), pure and server-runnable, on top of <see cref="WindGustRules"/>. A gust that
    /// shoves a walking hero off the planks of a bridge of the Pontes de Vento (more than <see cref="DeckHalfWidth"/> m from its line) drops them
    /// into the rope nets under it: the Sisin's net throws them back onto the bridge some way behind, towards where they stepped on, for a hit
    /// that never kills. Once O Cortador de Cordas is freed he guards the bridges and catches the hero in the air: back on the planks where the
    /// gust caught them, nothing lost.
    /// <code>
    /// off the planks: more than 4 m from the bridge's line, within 1 s of a gust's push (holding on is never pushed)
    /// the net: back on the bridge 12 m behind (never behind where the hero stepped on) · −8 % of max life, never under 25 %
    /// O Cortador de Cordas freed: caught where the gust hit · no hit, no setback
    /// walking, no cloak (7.5 m of push) from the middle: falls · with the Capa de Tempestade (3.75 m): only from the edge
    /// </code>
    /// Decision (2026-11-23): the planks lie on the ground (no hole under them), so the fall is the net's throw-back, like the rings of Amirani's
    /// chain: the cost is the walk again and a slice of life, which is what makes "pare e segure" worth obeying.
    /// </summary>
    public static class WindBridgeFallRules
    {
        public const string GuardEnemyId = RegionBossRules.RopeCutterId;
        public const float DeckHalfWidth = 4f;
        public const float Setback = 12f;
        public const float BlownGraceSeconds = 1f;
        public const float FallFraction = 0.08f;
        public const float FallFloorFraction = 0.25f;

        /// <summary>The save mark of the cousin's line the first time the freed Cortador catches the hero (set once, on that event).</summary>
        public const string FirstCatchKey = "farnkesh.bridge.first_catch";

        /// <summary>The bridge line nearest to <paramref name="p"/>: its index (−1 with no lines), metres along it and distance from it.</summary>
        public static int Nearest(IReadOnlyList<Vector2[]> lines, Vector2 p, out float along, out float distance)
        {
            int best = -1;
            along = 0f;
            distance = float.MaxValue;
            if (lines == null)
            {
                return best;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i] == null || lines[i].Length < 2)
                {
                    continue;
                }

                NartesChainRules.Project(lines[i], p, out float a, out float d, out _);
                if (d < distance)
                {
                    distance = d;
                    along = a;
                    best = i;
                }
            }

            return best;
        }

        public static bool OnPlanks(float distance) => distance <= DeckHalfWidth;

        /// <summary>Whether leaving the planks now is a fall: only while a gust's push still counts (otherwise the hero just walked off).</summary>
        public static bool IsFall(float distance, float time, float blownUntil) => !OnPlanks(distance) && time < blownUntil;

        /// <summary>
        /// Metres along the bridge where the hero lands: the guard puts them back where the gust hit (<paramref name="hitAlong"/>); the net throws
        /// them <see cref="Setback"/> m back towards where they stepped on (<paramref name="entryAlong"/>), never past it.
        /// </summary>
        public static float LandingAlong(float entryAlong, float hitAlong, bool guarded)
        {
            if (guarded)
            {
                return hitAlong;
            }

            return hitAlong >= entryAlong ? Mathf.Max(entryAlong, hitAlong - Setback) : Mathf.Min(entryAlong, hitAlong + Setback);
        }

        /// <summary>The hit of the net: none when the guard catches the hero; otherwise a slice that never takes them under the floor.</summary>
        public static float FallDamage(float current, float max, bool guarded) =>
            guarded ? 0f : ProfundoHazardRules.Bite(current, max, FallFraction, FallFloorFraction);
    }
}
