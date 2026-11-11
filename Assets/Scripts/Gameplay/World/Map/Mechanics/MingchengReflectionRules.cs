using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>A still pool of the paddies where the reflection can be looked at (region coordinates).</summary>
    public readonly struct MingchengPool
    {
        public readonly Vector2 Center;
        public readonly float Radius;

        public MingchengPool(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }
    }

    /// <summary>A trail that only exists in the reflection, from its pool to a chest at its far end (region coordinates).</summary>
    public sealed class MingchengReflectedTrail
    {
        public readonly int Pool;
        public readonly Vector2[] Points;

        public MingchengReflectedTrail(int pool, params Vector2[] points)
        {
            Pool = pool;
            Points = points;
        }

        public Vector2 Chest => Points[Points.Length - 1];
    }

    /// <summary>The vision of the reflection: seconds it still lasts and seconds until the water can be read again.</summary>
    public readonly struct MingchengReflectionState
    {
        public readonly float Vision;
        public readonly float Cooldown;

        public MingchengReflectionState(float vision, float cooldown)
        {
            Vision = vision;
            Cooldown = cooldown;
        }

        public bool Active => Vision > 0f;

        public bool Ready => Vision <= 0f && Cooldown <= 0f;
    }

    /// <summary>
    /// The mirror world of the Arrozais Espelhados (Docs/Mapas100.md, map 44: "olhar a água (botão) mostra o reflexo, onde aparecem trilhas e baús
    /// que não existem no seco; pisar na trilha refletida funciona por 10 s"; "a lua no espelho dobra as trilhas do reflexo"), pure. Looking is
    /// free and harmless: when the vision ends the hero simply stands in the paddy again, so it can neither strand nor hurt them.
    /// <code>
    /// vision 10 s, 20 s at night · then 12 s before the water can be read again
    /// on a reflected trail while the vision lasts: +25% speed, 3 m to each side · chest: 4 m reach, once per game day each
    /// </code>
    /// </summary>
    public static class MingchengReflectionRules
    {
        public const float VisionSeconds = 10f;
        public const float NightFactor = 2f;
        public const float CooldownSeconds = 12f;
        public const float TrailHalfWidth = 3f;
        public const float TrailBonus = 0.25f;
        public const float ChestReach = 4f;
        public const float PoolMargin = 3f;

        /// <summary>The chests' prizes (existing items), one a day, in turn.</summary>
        public static readonly string[] ChestItems = { "item_health_potion", "item_mana_potion", "item_herb" };

        public static float VisionLength(bool night) => night ? VisionSeconds * NightFactor : VisionSeconds;

        /// <summary>Starts the vision; nothing changes unless the water is ready.</summary>
        public static MingchengReflectionState Look(MingchengReflectionState state, bool night) => state.Ready ? new MingchengReflectionState(VisionLength(night), CooldownSeconds) : state;

        /// <summary>The cooldown only starts counting once the vision is over.</summary>
        public static MingchengReflectionState Advance(MingchengReflectionState state, float deltaTime)
        {
            float vision = state.Vision - deltaTime;
            if (vision > 0f)
            {
                return new MingchengReflectionState(vision, state.Cooldown);
            }

            return new MingchengReflectionState(0f, Mathf.Max(0f, state.Cooldown + Mathf.Min(0f, vision)));
        }

        /// <summary>The pool the hero can look into from <paramref name="p"/>, or −1.</summary>
        public static int PoolAt(IReadOnlyList<MingchengPool> pools, Vector2 p)
        {
            for (int i = 0; i < pools.Count; i++)
            {
                float reach = pools[i].Radius + PoolMargin;
                if ((pools[i].Center - p).sqrMagnitude <= reach * reach)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>Whether <paramref name="p"/> is on the reflected trail.</summary>
        public static bool OnTrail(MingchengReflectedTrail trail, Vector2 p)
        {
            for (int i = 0; i + 1 < trail.Points.Length; i++)
            {
                Vector2 a = trail.Points[i];
                Vector2 ab = trail.Points[i + 1] - a;
                float t = ab.sqrMagnitude > 0f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
                if ((a + ab * t - p).sqrMagnitude <= TrailHalfWidth * TrailHalfWidth)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The speed factor of the reflection: only on a trail and only while the vision lasts.</summary>
        public static float SpeedFactor(IReadOnlyList<MingchengReflectedTrail> trails, Vector2 p, MingchengReflectionState state)
        {
            if (!state.Active)
            {
                return 1f;
            }

            for (int i = 0; i < trails.Count; i++)
            {
                if (OnTrail(trails[i], p))
                {
                    return 1f + TrailBonus;
                }
            }

            return 1f;
        }

        /// <summary>The chest within reach of <paramref name="p"/>, or −1; chests only exist while the vision lasts.</summary>
        public static int ChestAt(IReadOnlyList<MingchengReflectedTrail> trails, Vector2 p, MingchengReflectionState state)
        {
            if (!state.Active)
            {
                return -1;
            }

            for (int i = 0; i < trails.Count; i++)
            {
                if ((trails[i].Chest - p).sqrMagnitude <= ChestReach * ChestReach)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>A chest opens once per game day (<paramref name="lastDay"/> is the day it last opened; 0 = never).</summary>
        public static bool CanOpen(int lastDay, int day) => day > lastDay;

        /// <summary>The prize of chest <paramref name="chest"/> on <paramref name="day"/>.</summary>
        public static string ChestItem(int chest, int day) => ChestItems[Mathf.Abs(chest + day) % ChestItems.Length];
    }
}
