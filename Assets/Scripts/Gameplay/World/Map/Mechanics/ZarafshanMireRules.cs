using System.Collections.Generic;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>A patch of mud cleaned by Living Water (<see cref="ZarafshanMireRules"/>): where, and until when (seconds of the driver's clock).</summary>
    public readonly struct ZarafshanCleanPool
    {
        public readonly Vector2 Center;
        public readonly float Until;

        public ZarafshanCleanPool(Vector2 center, float until)
        {
            Center = center;
            Until = until;
        }
    }

    /// <summary>
    /// The mud of the Brejo do Lodo Escuro (Docs/Mapas100.md 03: "lama: fora da passarela anda a ×0,7; Água Viva (skills do Joaquim, poção) limpa
    /// poças por 20 s"), pure and server-runnable. Inside the marsh's own map, the hero wades at 0.7 of their pace anywhere off the log walkways
    /// (the trails, with half a metre of margin), off the bridges over the black channels and off the Ponte do Aryk's safe ground. Every skill Joaquim
    /// casts (the cousin of Living Water) cleans the mud round the point it lands on and round his own feet for 20 s, where the pace comes back. It
    /// only ever slows, never hurts, and nothing is saved (the pools dry in 20 s).
    /// <code>
    /// off the walkway: × 0.7 (5.5 m/s → 3.85 m/s) · walkway = within half the trail's width + 0.5 m, or 5 m of a bridge
    /// Joaquim's skill: a clean pool of 6 m round the target point and another round him, for 20 s · at most 6 pools at once (the oldest dries first)
    /// Tugai gate (north) to the Amoreiras gate (east): ~265 m on the walkways (~48 s at 5.5 m/s) or ~190 m straight through the mud (~50 s):
    /// the mud no longer pays as a shortcut, and Joaquim's pools make it one again
    /// </code>
    /// </summary>
    public static class ZarafshanMireRules
    {
        public const float MudFactor = 0.7f;
        public const float WalkwayMargin = 0.5f;
        public const float BridgeReach = 5f;
        public const float CleanRadius = 6f;
        public const float CleanSeconds = 20f;
        public const int MaxPools = 6;

        /// <summary>The PercentAdditive move bonus of the mud (−0.3).</summary>
        public static float MoveBonus => MudFactor - 1f;

        /// <summary>Whether <paramref name="p"/> is on a log walkway: on a trail (with <see cref="WalkwayMargin"/>) or by a bridge.</summary>
        public static bool OnWalkway(IReadOnlyList<MapPath> paths, IReadOnlyList<Vector2> bridges, Vector2 p)
        {
            for (int i = 0; paths != null && i < paths.Count; i++)
            {
                if (MapLayoutRules.DistanceToSegment(p, paths[i].A, paths[i].B) <= paths[i].Width * 0.5f + WalkwayMargin)
                {
                    return true;
                }
            }

            for (int i = 0; bridges != null && i < bridges.Count; i++)
            {
                if ((bridges[i] - p).sqrMagnitude <= BridgeReach * BridgeReach)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Whether a clean pool still open at <paramref name="now"/> covers <paramref name="p"/>.</summary>
        public static bool Cleaned(IReadOnlyList<ZarafshanCleanPool> pools, Vector2 p, float now)
        {
            for (int i = 0; pools != null && i < pools.Count; i++)
            {
                if (now < pools[i].Until && (pools[i].Center - p).sqrMagnitude <= CleanRadius * CleanRadius)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The hero's pace factor: 1 outside the marsh, on a walkway, on the post's safe ground or in a clean pool; <see cref="MudFactor"/>
        /// otherwise.
        /// </summary>
        public static float SpeedFactor(bool inMarsh, bool onWalkway, bool safeGround, bool cleaned) => inMarsh && !onWalkway && !safeGround && !cleaned ? MudFactor : 1f;

        /// <summary>Whether the cousin who cleans the mud is casting (Joaquim, Living Water).</summary>
        public static bool Cleans(Characters.CharacterId? hero) => hero == Characters.CharacterId.Joaquim;

        /// <summary>
        /// Opens a clean pool at <paramref name="center"/> until <paramref name="now"/> + <see cref="CleanSeconds"/>: dried pools are dropped and,
        /// past <see cref="MaxPools"/>, the oldest goes.
        /// </summary>
        public static void AddPool(List<ZarafshanCleanPool> pools, Vector2 center, float now)
        {
            pools.RemoveAll(pool => now >= pool.Until);
            while (pools.Count >= MaxPools)
            {
                pools.RemoveAt(0);
            }

            pools.Add(new ZarafshanCleanPool(center, now + CleanSeconds));
        }
    }
}
