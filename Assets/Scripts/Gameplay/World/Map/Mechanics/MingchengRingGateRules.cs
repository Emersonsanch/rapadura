using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// One gate of Mingcheng's 4th ring (<c>OverworldLayouts.MingchengRingGates</c>), in the region's own coordinates: where its road crosses the
    /// ring (<see cref="Crossing"/>), the station outside it (<see cref="Outer"/>) and the street inside (<see cref="Inner"/>), both on the road,
    /// and the unit vector across the road (<see cref="Across"/>) along which the gate's leaves stand.
    /// </summary>
    public readonly struct RingGate
    {
        public readonly string Id;
        public readonly Vector2 Crossing;
        public readonly Vector2 Outer;
        public readonly Vector2 Inner;
        public readonly Vector2 Across;

        public RingGate(string id, Vector2 crossing, Vector2 outer, Vector2 inner, Vector2 across)
        {
            Id = id;
            Crossing = crossing;
            Outer = outer;
            Inner = inner;
            Across = across;
        }

        /// <summary>The walk through the gate, from the outside station to the street inside.</summary>
        public Vector2[] Course => new[] { Outer, Crossing, Inner };
    }

    /// <summary>What [E] at a gate's outside station does at night (<see cref="MingchengRingGateRules.Check"/>).</summary>
    public enum RingGateCheck
    {
        /// <summary>No curfew (the drum's day, or a night of the Festa das Lanternas): the gate stands open, walk in.</summary>
        Open = 0,

        /// <summary>A night watch: the postern is open, in for free.</summary>
        Watch = 1,

        /// <summary>Bögü's safe-conduct was bought tonight: in for free at any gate until the drum.</summary>
        Pass = 2,

        /// <summary>Shut: pay the toll to Bögü's guard and go in (the safe-conduct lasts the night).</summary>
        Toll = 3,

        /// <summary>Shut and too little gold: wait for the next watch (never more than two game hours).</summary>
        Wait = 4,
    }

    /// <summary>
    /// The gates of Mingcheng's rings (Docs/Mapas100.md 41: "portões por hora: os portões dos anéis abrem e fecham com o tambor (dia) e o sino
    /// (noite)"; "o sino fecha os anéis de fora: quem está fora fica fora até o tambor"), pure on the in-game day and hour so a server holds every
    /// player to the same curfew. Rings 1–3 are the open country round the city; the 4th ring, where the craft districts begin, is a ring of
    /// <see cref="Radius"/> m round the plaza with three gates where the west, east and south roads cross it. While the drum rules the day
    /// (<see cref="DrumBargeRules.IsDrumHour"/>, 6 h–20 h) the gates stand open and nothing stops anyone. When the bell rings at 20 h the curfew
    /// begins and the 4th ring holds whoever is outside it: crossing it anywhere is pushed back (<see cref="WallPush"/>), and only the gates let in:
    /// <list type="bullet">
    /// <item>at every night watch (22, 0, 2 and 4 h) the watch drum opens the postern for <see cref="WatchOpenHours"/> h: in for free;</item>
    /// <item>at any hour <see cref="Toll"/> gold to Bögü's guard buys the safe-conduct of the night (one save mark, written on the payment);</item>
    /// <item>on the nights of the Festa das Lanternas (<see cref="MingchengLanternFestaRules.IsOpen(int, float)"/>) the curfew is lifted until 1 h,
    /// as Chang'an lifted its night curfew for the lantern nights.</item>
    /// </list>
    /// Leaving is always free (the postern lets anyone out), the plaza is the respawn and every arrival by barque or portal lands inside, so the
    /// wait for the postern is never more than two game hours (100 real s by default) and needs no gold.
    /// <code>
    /// ring 48 m round the plaza · gates on the W, E, S roads, stations 5 m out and in · reach 3.5 m · through at 4 m/s
    /// curfew 20 h–6 h (festa nights from 1 h) · postern 22, 0, 2, 4 h for 0.5 h · toll 10 gold, the safe-conduct until the drum
    /// the wall holds only who comes from outside: a band of 6 m inside the ring line; a jump over 6 m is a teleport (the side is taken again)
    /// </code>
    /// </summary>
    public static class MingchengRingGateRules
    {
        public const float Radius = 48f;
        public const float GateHalfWidth = 4f;
        public const float GateEndOffset = 5f;
        public const float EndReach = 3.5f;
        public const float CrossSpeed = 4f;
        public const float WallBand = 6f;
        public const float WallSkin = 0.25f;
        public const float TeleportJump = 6f;
        public const float WatchOpenHours = 0.5f;
        public const int Toll = 10;

        /// <summary>Side of the ring: not known yet (just switched on, after a ride or a teleport).</summary>
        public const int Unknown = 0;

        public const int Inside = -1;
        public const int Outside = 1;

        /// <summary>The time mark of Bögü's safe-conduct: the night it was bought (+1), written only on the payment.</summary>
        public const string PassKey = "route.mingcheng_rings.pass";

        /// <summary>The night watches, when the watch drum opens the posterns.</summary>
        public static readonly int[] WatchHours = { 22, 0, 2, 4 };

        /// <summary>Whether the bell's curfew holds the 4th ring at game <paramref name="day"/>/<paramref name="hour"/>.</summary>
        public static bool Curfew(int day, float hour) => !DrumBargeRules.IsDrumHour(hour) && !MingchengLanternFestaRules.IsOpen(day, hour);

        /// <summary>The night the hour belongs to: the day whose bell began it (before the drum of 6 h it is still yesterday's night).</summary>
        public static int NightOf(int day, float hour) => hour < DrumBargeRules.DrumHour ? day - 1 : day;

        /// <summary>The safe-conduct's mark for <paramref name="night"/> (+1, so the empty mark 0 is never a night).</summary>
        public static long StorePass(int night) => night + 1L;

        /// <summary>Whether the saved safe-conduct was bought on <paramref name="night"/>.</summary>
        public static bool PassValid(long saved, int night) => saved != 0L && saved == StorePass(night);

        /// <summary>The watch whose postern is open at <paramref name="hour"/>, or −1.</summary>
        public static int OpenWatch(float hour)
        {
            foreach (int watch in WatchHours)
            {
                if (hour >= watch && hour < watch + WatchOpenHours)
                {
                    return watch;
                }
            }

            return -1;
        }

        /// <summary>The next hour that lets the hero in (a watch, or the drum of 6 h) after <paramref name="hour"/>.</summary>
        public static int NextOpening(float hour)
        {
            int best = (int)DrumBargeRules.DrumHour;
            float bestWait = Wait(hour, best);
            foreach (int watch in WatchHours)
            {
                float wait = Wait(hour, watch);
                if (wait < bestWait)
                {
                    bestWait = wait;
                    best = watch;
                }
            }

            return best;
        }

        /// <summary>Game hours from <paramref name="hour"/> until the next <paramref name="at"/> o'clock (0 &lt; wait ≤ 24).</summary>
        public static float Wait(float hour, int at)
        {
            float wait = (at - hour) % 24f;
            if (wait <= 0f)
            {
                wait += 24f;
            }

            return wait;
        }

        /// <summary>What [E] at a gate's outside station does.</summary>
        public static RingGateCheck Check(bool curfew, bool watchOpen, bool passTonight, int gold)
        {
            if (!curfew)
            {
                return RingGateCheck.Open;
            }

            if (passTonight)
            {
                return RingGateCheck.Pass;
            }

            if (watchOpen)
            {
                return RingGateCheck.Watch;
            }

            return gold >= Toll ? RingGateCheck.Toll : RingGateCheck.Wait;
        }

        /// <summary>The side of the ring round <paramref name="center"/> that <paramref name="p"/> stands on.</summary>
        public static int SideOf(Vector2 p, Vector2 center) => (p - center).sqrMagnitude < Radius * Radius ? Inside : Outside;

        /// <summary>
        /// The side the hero is held on after moving from <paramref name="last"/> to <paramref name="p"/>: under the curfew whoever came from outside
        /// stays outside while within <see cref="WallBand"/> of the ring line (the wall pushes them back); an unknown side or a jump of more than
        /// <see cref="TeleportJump"/> takes the side where the hero stands.
        /// </summary>
        public static int NextSide(int side, Vector2 last, Vector2 p, Vector2 center, bool curfew)
        {
            if (side == Unknown || (p - last).sqrMagnitude > TeleportJump * TeleportJump)
            {
                return SideOf(p, center);
            }

            if (curfew && side == Outside && (p - center).magnitude > Radius - WallBand)
            {
                return Outside;
            }

            return SideOf(p, center);
        }

        /// <summary>The shove (m) that puts a hero held <see cref="Outside"/> back over the ring line under the curfew; zero otherwise.</summary>
        public static Vector2 WallPush(int side, Vector2 p, Vector2 center, bool curfew)
        {
            if (!curfew || side != Outside)
            {
                return Vector2.zero;
            }

            Vector2 d = p - center;
            float r = d.magnitude;
            if (r >= Radius || r <= Radius - WallBand)
            {
                return Vector2.zero;
            }

            Vector2 dir = r > 1e-3f ? d / r : Vector2.right;
            return dir * (Radius + WallSkin - r);
        }

        /// <summary>The gate whose outside station is within <see cref="EndReach"/> of <paramref name="p"/>, or −1.</summary>
        public static int OuterAt(IReadOnlyList<RingGate> gates, Vector2 p)
        {
            for (int i = 0; gates != null && i < gates.Count; i++)
            {
                if ((gates[i].Outer - p).sqrMagnitude <= EndReach * EndReach)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>The gate whose crossing is within <see cref="GateHalfWidth"/> of <paramref name="p"/>, or −1.</summary>
        public static int CrossingAt(IReadOnlyList<RingGate> gates, Vector2 p)
        {
            for (int i = 0; gates != null && i < gates.Count; i++)
            {
                if ((gates[i].Crossing - p).sqrMagnitude <= GateHalfWidth * GateHalfWidth)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// The gate of a straight road from <paramref name="a"/> (inside the ring) to <paramref name="b"/> (outside it): where the road crosses the
        /// ring round <paramref name="center"/>, with its stations <see cref="GateEndOffset"/> m out and in along the road.
        /// </summary>
        public static RingGate GateOnRoad(string id, Vector2 a, Vector2 b, Vector2 center)
        {
            Vector2 dir = (b - a).normalized;
            Vector2 f = a - center;
            float half = Vector2.Dot(f, dir);
            float c = f.sqrMagnitude - Radius * Radius;
            float t = -half + Mathf.Sqrt(Mathf.Max(0f, half * half - c));
            Vector2 crossing = a + dir * t;
            return new RingGate(id, crossing, crossing + dir * GateEndOffset, crossing - dir * GateEndOffset, new Vector2(-dir.y, dir.x));
        }

        /// <summary>
        /// The wall of the ring round <paramref name="center"/> as arcs between the gates (each gap <see cref="GateHalfWidth"/> to each side of a
        /// crossing), <paramref name="pointsPerArc"/> points per arc at least, in angle order.
        /// </summary>
        public static List<Vector2[]> WallArcs(IReadOnlyList<RingGate> gates, Vector2 center, int pointsPerArc)
        {
            var arcs = new List<Vector2[]>();
            int count = gates?.Count ?? 0;
            float gap = GateHalfWidth / Radius;
            var angles = new List<float>(count);
            for (int i = 0; i < count; i++)
            {
                Vector2 d = gates[i].Crossing - center;
                angles.Add(Mathf.Atan2(d.y, d.x));
            }

            angles.Sort();
            if (count == 0)
            {
                angles.Add(0f);
                gap = 0f;
            }

            int points = Mathf.Max(2, pointsPerArc);
            for (int i = 0; i < angles.Count; i++)
            {
                float from = angles[i] + gap;
                float to = (i + 1 < angles.Count ? angles[i + 1] : angles[0] + 2f * Mathf.PI) - gap;
                var arc = new Vector2[points];
                for (int k = 0; k < points; k++)
                {
                    float angle = Mathf.Lerp(from, to, k / (float)(points - 1));
                    arc[k] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Radius;
                }

                arcs.Add(arc);
            }

            return arcs;
        }
    }
}
