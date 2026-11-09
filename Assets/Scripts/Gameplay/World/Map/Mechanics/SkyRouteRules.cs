using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    public enum SkyRouteKind
    {
        /// <summary>The rope baskets of the Sisin (maps 54–57): anyone rides them.</summary>
        RopeBasket = 0,
        /// <summary>A wind current between the cloud isles (map 58): wings, canvas wings or a flying mount.</summary>
        Glide = 1,
    }

    /// <summary>
    /// One rope basket line or wind current, in the region's own coordinates: the two landings and the prop that marks each
    /// (<see cref="OverworldLayouts.SkyRoutes"/>). For a basket <see cref="A"/> is the low end; for a current it is the end
    /// the current leaves from (<see cref="SkyRouteRules.IsCurrentOpen"/>).
    /// </summary>
    public sealed class SkyRoute
    {
        public string Id;
        public SkyRouteKind Kind;
        public Vector2 A;
        public Vector2 B;
        public Vector2 PropA;
        public Vector2 PropB;

        /// <summary>Which of the three daily turns (0–2) this current rests in; unused by the baskets.</summary>
        public int CurrentGroup;

        public override string ToString() => Id;
    }

    /// <summary>Why a ride may not start (<see cref="SkyRouteRules.Check"/>).</summary>
    public enum SkyRideCheck
    {
        Ok = 0,
        NeedsWings = 1,
        CurrentResting = 2,
        ZoneBlocked = 3,
    }

    /// <summary>
    /// Rope baskets and wind currents (Docs/Mapas100.md, 54 "subir é devagar, descer planando é rápido" and 58 "planar; as
    /// correntes mudam por hora"), pure and server-runnable. The hero goes in a straight line between two landings on an arc,
    /// out of reach of the ground.
    /// <code>
    /// basket: up 9 m/s · down 9 m/s, 18 m/s with wings or a flying mount · arc 10 m
    /// current: 14 m/s · arc 5 m · rests 1 in-game hour out of 3 (leaving from A only: from B the way back never closes)
    /// </code>
    /// The hero runs at 5.5 m/s, so a basket is a shortcut that spares the climb and its monsters rather than a teleport.
    /// </summary>
    public static class SkyRouteRules
    {
        public const float StationRadius = 4f;
        public const float BasketUpSpeed = 9f;
        public const float BasketDownSpeed = 9f;
        public const float BasketGlideDownSpeed = 18f;
        public const float GlideSpeed = 14f;
        public const float BasketArc = 10f;
        public const float GlideArc = 5f;
        public const int CurrentTurns = 3;

        /// <summary>Metres between the points of a route checked against the soft barrier.</summary>
        public const float ZoneSampleStep = 8f;

        /// <summary>Speed (m/s) of a ride; <paramref name="toB"/> goes from A to B.</summary>
        public static float Speed(SkyRoute route, bool toB, bool canGlide)
        {
            if (route.Kind == SkyRouteKind.Glide)
            {
                return GlideSpeed;
            }

            return toB ? BasketUpSpeed : canGlide ? BasketGlideDownSpeed : BasketDownSpeed;
        }

        public static float Arc(SkyRoute route) => route.Kind == SkyRouteKind.Glide ? GlideArc : BasketArc;

        public static float Duration(SkyRoute route, bool toB, bool canGlide) => Vector2.Distance(route.A, route.B) / Speed(route, toB, canGlide);

        /// <summary>A current rests while the whole in-game hour modulo <see cref="CurrentTurns"/> is its group; the way back (from B) never does, so no isle strands a hero.</summary>
        public static bool IsCurrentOpen(SkyRoute route, bool toB, float hour)
        {
            if (route.Kind != SkyRouteKind.Glide || !toB)
            {
                return true;
            }

            return Mathf.FloorToInt(Mathf.Max(0f, hour)) % CurrentTurns != route.CurrentGroup;
        }

        /// <summary>Can the ride start? <paramref name="zonesOpen"/> = <see cref="AllZonesOpen"/>.</summary>
        public static SkyRideCheck Check(SkyRoute route, bool toB, float hour, bool canGlide, bool zonesOpen)
        {
            if (route.Kind == SkyRouteKind.Glide && !canGlide)
            {
                return SkyRideCheck.NeedsWings;
            }

            if (!zonesOpen)
            {
                return SkyRideCheck.ZoneBlocked;
            }

            return IsCurrentOpen(route, toB, hour) ? SkyRideCheck.Ok : SkyRideCheck.CurrentResting;
        }

        /// <summary>
        /// True when the soft barrier would let the hero through every zone under the route (it would otherwise throw them back
        /// in mid-air). <paramref name="origin"/> is where the region sits in <paramref name="layout"/> (zero alone).
        /// </summary>
        public static bool AllZonesOpen(OverworldLayout layout, Vector2 origin, SkyRoute route, int heroLevel, CharacterId? hero, Func<string, bool> hasRequirement)
        {
            float length = Vector2.Distance(route.A, route.B);
            int steps = Mathf.Max(1, Mathf.CeilToInt(length / ZoneSampleStep));
            for (int i = 0; i <= steps; i++)
            {
                OverworldZone zone = OverworldRules.ZoneAt(layout, origin + Vector2.Lerp(route.A, route.B, i / (float)steps));
                if (zone == null || OverworldRules.CheckEnter(zone, heroLevel, hero, hasRequirement) != MapAccessCheck.Ok)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Ground position of a ride at <paramref name="t"/> (0–1).</summary>
        public static Vector2 PositionAt(SkyRoute route, bool toB, float t) => toB ? Vector2.Lerp(route.A, route.B, t) : Vector2.Lerp(route.B, route.A, t);

        /// <summary>Height of a ride at <paramref name="t"/> (0–1): between the two grounds with an arc on top, touching them at both ends.</summary>
        public static float HeightAt(float fromGround, float toGround, float arc, float t)
        {
            float k = Mathf.Clamp01(t);
            return Mathf.Lerp(fromGround, toGround, k) + arc * Mathf.Sin(k * Mathf.PI);
        }

        /// <summary>
        /// The routes with a landing within <see cref="StationRadius"/> of <paramref name="p"/> (region coordinates), nearest
        /// first, into <paramref name="found"/> (route and whether it leaves towards B).
        /// </summary>
        public static void StationsAt(IReadOnlyList<SkyRoute> routes, Vector2 p, List<(SkyRoute Route, bool ToB)> found)
        {
            found.Clear();
            float max = StationRadius * StationRadius;
            foreach (SkyRoute route in routes)
            {
                if ((route.A - p).sqrMagnitude <= max)
                {
                    found.Add((route, true));
                }

                if ((route.B - p).sqrMagnitude <= max)
                {
                    found.Add((route, false));
                }
            }

            found.Sort((x, y) => ((x.ToB ? x.Route.A : x.Route.B) - p).sqrMagnitude.CompareTo(((y.ToB ? y.Route.A : y.Route.B) - p).sqrMagnitude));
        }
    }
}
