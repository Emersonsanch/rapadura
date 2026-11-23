using System.Collections.Generic;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Where the pavilion of the Jardim de Mihrshah stands for the hero (<see cref="ZarafshanPavilionRules.StateOf"/>).</summary>
    public enum MihrshahPavilionState
    {
        /// <summary>Some of the four sparks are still caught: the door is dark.</summary>
        Sealed = 0,

        /// <summary>The four canals run, the door glows and waits for [E].</summary>
        Closed = 1,

        /// <summary>The door was opened: the pavilion mends and its altar gives the Pétala de Luz.</summary>
        Open = 2,
    }

    /// <summary>What [E] at the pavilion's altar does (<see cref="ZarafshanPavilionRules.CheckAltar"/>).</summary>
    public enum MihrshahAltarCheck
    {
        /// <summary>A Pétala de Luz for today.</summary>
        Give = 0,

        /// <summary>Today's petal was already picked.</summary>
        PickedToday = 1,

        /// <summary>The door is not open yet.</summary>
        Shut = 2,

        /// <summary>A fallen hero picks nothing.</summary>
        Dead = 3,
    }

    /// <summary>
    /// The pavilion, the Pétala de Luz and the night of the Jardim Esquecido de Mihrshah (Docs/Mapas100.md 06: "libertar as 4 faíscas ... abre o
    /// pavilhão"; "loot Pétala de Luz"; "Noite: as rosas brilham; visão boa só perto dos canais cheios"), pure and server-runnable, on top of
    /// <see cref="ZarafshanGardenRules"/>.
    /// <code>
    /// door: on the west of the eight columns, where the west canal ends (centre − 8.5 m on x), reach 3 m
    ///   Sealed until the 4 sparks are free → Closed (glows) → [E] opens it for good (a time mark, written only then) + 3 Pétalas de Luz once
    /// the open pavilion mends as before (9 m, 6 % life and mana a second, out of combat); a closed door mends nothing
    /// altar: the centre of the pavilion, reach 2.5 m, door open → 1 Pétala de Luz per game day (a daily mark, written only then)
    /// Pétala de Luz: 120 life + 60 mana (the item), and its light for 180 s: the Garden's night and the Garajonay fog hide no beast
    /// night (inside the walls, 58 m): beasts past 9 m are lost to sight, unless the hero stands within 6 m of a lit canal arm, inside the
    ///   open pavilion or in the petal's light; bosses always show
    /// </code>
    /// Decision (2026-11-23): Saklas's fight stays the world-map event; opening the door is the reward of the four quadrants, so the pavilion's
    /// mending now waits for the door (an older save with the four free only has to press [E] once). One petal a day keeps it a garden gift,
    /// not a shop: the first opening gives three so the cousin can try it at night straight away.
    /// </summary>
    public static class ZarafshanPavilionRules
    {
        public const string PetalItemId = "item_petala_luz";

        public const float DoorReach = 3f;
        public const float AltarReach = 2.5f;
        public const int FirstOpenPetals = 3;
        public const int DailyPetals = 1;

        /// <summary>The door: on the west of the pavilion's columns (8 m round the centre), at the end of the west canal arm (which stops 9 m out).</summary>
        public static readonly Vector2 DoorOffset = new Vector2(-8.5f, 0f);

        /// <summary>Half the side of the walled chahar bagh (the wall stands 58 m round the centre).</summary>
        public const float WallHalf = 58f;

        public const float NightSight = 9f;
        public const float CanalSightReach = 6f;
        public const float PetalLightSeconds = 180f;

        /// <summary>Time mark (the unix second the door was opened, &gt; 0 = open) of the pavilion's door.</summary>
        public const string DoorKey = "zarafshan.garden.pavilion";

        /// <summary>Daily mark (the game day of the last petal picked) of the altar.</summary>
        public const string AltarKey = "zarafshan.garden.altar";

        public static Vector2 DoorAt(Vector2 centre) => centre + DoorOffset;

        public static bool DoorOpen(long doorMark) => doorMark > 0L;

        public static MihrshahPavilionState StateOf(IReadOnlyList<long> freedMarks, int sparks, long doorMark)
        {
            if (!ZarafshanGardenRules.PavilionOpen(freedMarks, sparks))
            {
                return MihrshahPavilionState.Sealed;
            }

            return DoorOpen(doorMark) ? MihrshahPavilionState.Open : MihrshahPavilionState.Closed;
        }

        /// <summary>Whether [E] at <paramref name="p"/> opens the door now.</summary>
        public static bool CanOpenDoor(MihrshahPavilionState state, Vector2 centre, Vector2 p, bool alive) =>
            alive && state == MihrshahPavilionState.Closed && (p - DoorAt(centre)).sqrMagnitude <= DoorReach * DoorReach;

        /// <summary>Whether the pavilion mends the hero at <paramref name="p"/>: open and inside its ring.</summary>
        public static bool Mends(MihrshahPavilionState state, Vector2 centre, Vector2 p) =>
            state == MihrshahPavilionState.Open && ZarafshanGardenRules.InPavilion(centre, p);

        public static bool AtAltar(Vector2 centre, Vector2 p) => (p - centre).sqrMagnitude <= AltarReach * AltarReach;

        /// <summary>What [E] at the altar does on game day <paramref name="today"/> (<paramref name="lastDay"/> = the daily mark, 0 = never).</summary>
        public static MihrshahAltarCheck CheckAltar(MihrshahPavilionState state, int lastDay, int today, bool alive)
        {
            if (!alive)
            {
                return MihrshahAltarCheck.Dead;
            }

            if (state != MihrshahPavilionState.Open)
            {
                return MihrshahAltarCheck.Shut;
            }

            return lastDay > 0 && lastDay >= today ? MihrshahAltarCheck.PickedToday : MihrshahAltarCheck.Give;
        }

        /// <summary>Whether the petal eaten at <paramref name="litAt"/> (Time.time, −∞ = none) still shines at <paramref name="now"/>.</summary>
        public static bool PetalLit(float litAt, float now) => now >= litAt && now - litAt < PetalLightSeconds;

        public static bool InGarden(Vector2 centre, Vector2 p) => Mathf.Abs(p.x - centre.x) <= WallHalf && Mathf.Abs(p.y - centre.y) <= WallHalf;

        /// <summary>Whether a lit canal arm runs within <see cref="CanalSightReach"/> of <paramref name="p"/>.</summary>
        public static bool NearLitCanal(IReadOnlyList<ZarafshanGardenSpark> sparks, IReadOnlyList<long> freedMarks, Vector2 p)
        {
            for (int i = 0; sparks != null && freedMarks != null && i < sparks.Count && i < freedMarks.Count; i++)
            {
                if (ZarafshanGardenRules.IsFree(freedMarks[i])
                    && MapLayoutRules.DistanceToSegment(p, sparks[i].CanalA, sparks[i].CanalB) <= CanalSightReach)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// How far the hero sees beasts in the Garden (<see cref="BeastSightRules"/>: ≤ 0 = no cut): only at night, inside the walls, alive, away
        /// from the lit water, outside the open pavilion and without the petal's light.
        /// </summary>
        public static float SightRange(bool night, bool inGarden, bool alive, bool nearLitCanal, bool inOpenPavilion, bool petalLit)
        {
            if (!night || !inGarden || !alive || nearLitCanal || inOpenPavilion || petalLit)
            {
                return BeastSightRules.NoLimit;
            }

            return NightSight;
        }
    }

    /// <summary>
    /// When the hero last ate a Pétala de Luz (Time.time), shared by the drivers of the Bosque do Zarafshan (the Garden's night, the Garajonay
    /// fog). Local to the hero's client; nothing is saved: the light lasts minutes. Online the server keeps its own per player.
    /// </summary>
    public static class LightPetalBoard
    {
        public static float LitAt { get; private set; } = float.NegativeInfinity;

        public static void Light(float now) => LitAt = now;

        public static bool Lit(float now) => ZarafshanPavilionRules.PetalLit(LitAt, now);

        /// <summary>Also on each Play (domain reload is off: a stale time from the last session would light it again).</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Clear() => LitAt = float.NegativeInfinity;
    }
}
