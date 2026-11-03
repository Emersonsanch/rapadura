using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Dungeon
{
    /// <summary>One placed thing of the ossuary, in hall space: x = lateral (+ right), z = along the hall from the entrance.</summary>
    public readonly struct DungeonSpot
    {
        public readonly float X;
        public readonly float Z;

        /// <summary>Which room it belongs to (0 Vestíbulo, 1 Galeria, 2 Câmara do Colosso).</summary>
        public readonly int Room;

        public DungeonSpot(float x, float z, int room)
        {
            X = x;
            Z = z;
            Room = room;
        }

        public Vector2 Local => new Vector2(X, Z);
    }

    /// <summary>
    /// Floor plan of the Ossuário dos Artífices, pure (EditMode-tested): a narrow hall of stone running from where the hero
    /// stands toward (and past) the arena centre, cut into three rooms by two barred gates:
    /// <list type="number">
    /// <item><b>Vestíbulo</b> — skulls and copper warriors, two sarcophagi;</item>
    /// <item><b>Galeria dos Arqueiros</b> — archers, warriors and the bone sorcerer, two sarcophagi, and on the right wall a
    /// cracked bone wall hiding the niche with the legendary chest;</item>
    /// <item><b>Câmara do Colosso</b> — the boss, in a wider round end.</item>
    /// </list>
    /// Everything scales with <see cref="Length"/> (the arena gives up to <see cref="DesiredLength"/> m inside its flee radius).
    /// </summary>
    public sealed class DungeonLayout
    {
        public const float DesiredLength = 26f;
        public const float MinLength = 16f;
        public const float HalfWidth = 3.2f;
        public const float BossHalfWidth = 5f;
        public const float WallHeight = 1.7f;
        public const float WallThickness = 0.6f;
        public const float NicheDepth = 3.4f;
        public const float NicheHalfLength = 1.6f;

        public float Length { get; }

        public DungeonLayout(float length)
        {
            Length = Mathf.Clamp(length, MinLength, DesiredLength);
        }

        /// <summary>
        /// Hall length for a hero standing <paramref name="distanceToCenter"/> m from the arena centre, walking toward it,
        /// when everything must stay within <paramref name="safeRadius"/> of the centre (the flee radius minus a margin).
        /// </summary>
        public static float FitLength(float distanceToCenter, float safeRadius)
        {
            float d = Mathf.Max(0f, distanceToCenter);
            float available = d <= safeRadius ? d + safeRadius : 2f * safeRadius;
            return Mathf.Clamp(available - 1f, MinLength, DesiredLength);
        }

        public float GalleryGateZ => Length * 0.36f;
        public float ColossusGateZ => Length * 0.68f;
        public float EndZ => Length;

        /// <summary>Where each room starts and ends along the hall.</summary>
        public float RoomStart(int room) => room <= 0 ? 0f : room == 1 ? GalleryGateZ : ColossusGateZ;

        public float RoomEnd(int room) => room <= 0 ? GalleryGateZ : room == 1 ? ColossusGateZ : EndZ;

        public float RoomCenterZ(int room) => (RoomStart(room) + RoomEnd(room)) * 0.5f;

        /// <summary>Half width of the hall at <paramref name="z"/> (the boss chamber is wider).</summary>
        public float HalfWidthAt(float z) => z >= ColossusGateZ + 0.5f ? BossHalfWidth : HalfWidth;

        /// <summary>The room a point along the hall is in.</summary>
        public int RoomAt(float z) => z < GalleryGateZ ? 0 : z < ColossusGateZ ? 1 : 2;

        /// <summary>The niche behind the bone wall opens in the right wall of the gallery, at this z.</summary>
        public float NicheZ => Length * 0.52f;

        public DungeonSpot BoneWall => new DungeonSpot(HalfWidth + WallThickness * 0.5f, NicheZ, 1);

        public DungeonSpot LegendaryChest => new DungeonSpot(HalfWidth + NicheDepth * 0.6f, NicheZ, 1);

        public DungeonSpot BossSpawn => new DungeonSpot(0f, Length * 0.86f, 2);

        /// <summary>The four sarcophagi, in <see cref="DungeonRules.SarcophagusIds"/> order, against the side walls.</summary>
        public IReadOnlyList<DungeonSpot> Sarcophagi => new[]
        {
            new DungeonSpot(-(HalfWidth - 0.75f), Length * 0.17f, 0),
            new DungeonSpot(HalfWidth - 0.75f, Length * 0.29f, 0),
            new DungeonSpot(-(HalfWidth - 0.75f), Length * 0.45f, 1),
            new DungeonSpot(-(HalfWidth - 0.75f), Length * 0.6f, 1),
        };

        /// <summary>Where each room's guardians stand when it wakes (the director picks the enemy per index).</summary>
        public IReadOnlyList<DungeonSpot> SpawnSpots(int room)
        {
            float start = RoomStart(room);
            float span = RoomEnd(room) - start;
            if (room == 0)
            {
                return new[]
                {
                    new DungeonSpot(-1.4f, start + span * 0.55f, 0),
                    new DungeonSpot(1.4f, start + span * 0.6f, 0),
                    new DungeonSpot(-0.6f, start + span * 0.85f, 0),
                    new DungeonSpot(0.8f, start + span * 0.8f, 0),
                };
            }

            if (room == 1)
            {
                return new[]
                {
                    new DungeonSpot(-1.2f, start + span * 0.25f, 1),
                    new DungeonSpot(1.2f, start + span * 0.3f, 1),
                    new DungeonSpot(-1.8f, start + span * 0.85f, 1),
                    new DungeonSpot(1.8f, start + span * 0.8f, 1),
                    new DungeonSpot(0f, start + span * 0.9f, 1),
                };
            }

            return new[] { BossSpawn };
        }

        /// <summary>Skull niches with fairy lights along both walls, every ~3 m (not at the gates nor at the bone wall).</summary>
        public IReadOnlyList<DungeonSpot> LightNiches
        {
            get
            {
                var spots = new List<DungeonSpot>();
                for (float z = 1.5f; z < Length - 1f; z += 3f)
                {
                    if (Mathf.Abs(z - GalleryGateZ) < 1f || Mathf.Abs(z - ColossusGateZ) < 1f)
                    {
                        continue;
                    }

                    float half = HalfWidthAt(z);
                    spots.Add(new DungeonSpot(-half, z, RoomAt(z)));
                    if (Mathf.Abs(z - NicheZ) > NicheHalfLength + 0.5f)
                    {
                        spots.Add(new DungeonSpot(half, z, RoomAt(z)));
                    }
                }

                return spots;
            }
        }

        /// <summary>Whether a hall-space point is inside the walkable hall (or the niche once the wall is down).</summary>
        public bool Contains(Vector2 local, bool nicheOpen)
        {
            if (local.y < 0f || local.y > Length)
            {
                return false;
            }

            if (Mathf.Abs(local.x) <= HalfWidthAt(local.y))
            {
                return true;
            }

            return nicheOpen && local.x > 0f && local.x <= HalfWidth + NicheDepth && Mathf.Abs(local.y - NicheZ) <= NicheHalfLength;
        }
    }

    /// <summary>
    /// The hall placed in the world: <see cref="Origin"/> (the entrance, on the ground where the hero starts) and
    /// <see cref="Rotation"/> (hall +z = <see cref="Forward"/>). Converts between hall space (x lateral, z along) and the world,
    /// and keeps points inside the walls (enemy retreats, ambush spawns, bouncing skulls).
    /// </summary>
    public sealed class DungeonHall
    {
        public DungeonHall(DungeonLayout layout, Vector3 origin, Vector3 forward)
        {
            Layout = layout;
            Origin = origin;
            forward.y = 0f;
            Forward = forward.sqrMagnitude > 1e-6f ? forward.normalized : Vector3.forward;
            Rotation = Quaternion.LookRotation(Forward, Vector3.up);
            Right = Vector3.Cross(Vector3.up, Forward);
        }

        public DungeonLayout Layout { get; }
        public Vector3 Origin { get; }
        public Vector3 Forward { get; }
        public Vector3 Right { get; }
        public Quaternion Rotation { get; }

        /// <summary>World point of hall-space (x, z) at the origin's height.</summary>
        public Vector3 ToWorld(float x, float z) => Origin + Right * x + Forward * z;

        public Vector3 ToWorld(DungeonSpot spot) => ToWorld(spot.X, spot.Z);

        /// <summary>Hall-space (x, z) of a world point.</summary>
        public Vector2 ToLocal(Vector3 world)
        {
            Vector3 d = world - Origin;
            return new Vector2(Vector3.Dot(d, Right), Vector3.Dot(d, Forward));
        }

        /// <summary>Lateral direction (world) of hall-space velocity components.</summary>
        public Vector3 Direction(float lateral, float along) => Right * lateral + Forward * along;

        /// <summary>Keeps a world point inside the hall (a margin from the walls), keeping its height.</summary>
        public Vector3 Clamp(Vector3 world, float margin = 0.6f)
        {
            Vector2 local = ToLocal(world);
            float z = Mathf.Clamp(local.y, 0.5f, Layout.Length - margin);
            float half = Mathf.Max(0.2f, Layout.HalfWidthAt(z) - margin);
            float x = Mathf.Clamp(local.x, -half, half);
            Vector3 clamped = ToWorld(x, z);
            clamped.y = world.y;
            return clamped;
        }
    }
}
