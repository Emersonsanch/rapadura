using System.Collections.Generic;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>The four compass arms a town's streets can leave the plaza by (x = east, z = north on the ground plane).</summary>
    public enum CityArm
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3,
    }

    /// <summary>What stands on a lot (drives the building model and the interaction).</summary>
    public enum CityLotKind
    {
        House = 0,
        /// <summary>A shop front: porch, sign, the keeper beside the door.</summary>
        Shop = 1,
        /// <summary>Open-fronted shed (ferraria, marcenaria): roof on posts, the work at the back.</summary>
        Workshop = 2,
        /// <summary>The lamp house at the head of the plaza (Ramrat's guardian lamp): pishtaq portal, dome, lamp tower.</summary>
        LampHouse = 3,
        /// <summary>The stable (Srosh's): long building with stall doors, paddock beside it.</summary>
        Stable = 4,
        /// <summary>Bazar da Nana: market stalls (barracas) instead of a building.</summary>
        Market = 5,
        /// <summary>Cais da Barca (world bible §3.16): a short pier over a groove of light; its trigger opens the Barcas de Luz on the map.</summary>
        ShipDock = 6,
        /// <summary>Vó Edna's house: the deep aivan on carved columns, the tandir oven, the vine trellis and the orchard.</summary>
        Grandma = 7,
    }

    /// <summary>Small things on the ground. Each has its own simple collider (or none) in <see cref="CityLayout.Solids"/>.</summary>
    public enum CityPropKind
    {
        Well = 0,
        Bench = 1,
        LampPost = 2,
        Tree = 3,
        Stall = 4,
        FenceRun = 5,
        Bush = 6,
        FlowerBed = 7,
        Crates = 8,
        HayBale = 9,
        /// <summary>The Relógio de Zurvan: the sundial of the plaza.</summary>
        Sundial = 10,
        Gate = 11,
        Rock = 12,
        GrassTuft = 13,
        /// <summary>Tandir (clay oven).</summary>
        Tandir = 14,
        /// <summary>Vine trellis (latada): four posts and a roof of vines; walkable underneath.</summary>
        Trellis = 15,
        /// <summary>Copper and clay jugs (cântaros).</summary>
        Jugs = 16,
        /// <summary>A tall carved stela (Aksum style, Litema's plaza).</summary>
        Stela = 17,
    }

    public enum CityExitKind
    {
        /// <summary>Back to the world map (the road out of town).</summary>
        ToMap = 0,
        /// <summary>The Cais da Barca: back to the map with the Barcas de Luz (fast travel) window open.</summary>
        ShipDock = 1,
    }

    /// <summary>A collider footprint, the single source for both the real colliders and the layout rules.</summary>
    public readonly struct CitySolid
    {
        public readonly bool IsCapsule;

        /// <summary>Box: axis-aligned footprint. Capsule: centre in <see cref="Rect.center"/>.</summary>
        public readonly Rect Footprint;

        public readonly float Radius;
        public readonly float Height;
        public readonly string Owner;

        private CitySolid(bool capsule, Rect footprint, float radius, float height, string owner)
        {
            IsCapsule = capsule;
            Footprint = footprint;
            Radius = radius;
            Height = height;
            Owner = owner;
        }

        public static CitySolid Box(Rect footprint, float height, string owner) => new CitySolid(false, footprint, 0f, height, owner);

        public static CitySolid Capsule(Vector2 center, float radius, float height, string owner) =>
            new CitySolid(true, new Rect(center.x - radius, center.y - radius, radius * 2f, radius * 2f), radius, height, owner);

        public Vector2 Center => Footprint.center;

        /// <summary>Ground distance from <paramref name="point"/> to this solid (0 inside).</summary>
        public float DistanceTo(Vector2 point)
        {
            if (IsCapsule)
            {
                return Mathf.Max(0f, Vector2.Distance(point, Footprint.center) - Radius);
            }

            return CityGeometry.DistanceToRect(point, Footprint);
        }
    }

    /// <summary>One building plot (or market / cart stop) of a town.</summary>
    public sealed class CityLot
    {
        public string Id;
        public CityLotKind Kind;

        /// <summary><c>CityCatalog</c> resident id of the keeper (null = nobody stands here).</summary>
        public string ResidentId;

        /// <summary>Shop opened by the keeper (<c>CityShopCatalog</c> id), or null.</summary>
        public string ShopId;

        /// <summary>The street this lot fronts (null for plaza / free lots).</summary>
        public CityArm? Arm;

        /// <summary>Distance of the door along <see cref="Arm"/> from the plaza centre.</summary>
        public float DoorAlong;

        /// <summary>Centre of the whole footprint (porch included).</summary>
        public Vector2 Center;

        /// <summary>Unit direction the front (door) faces.</summary>
        public Vector2 Front;

        /// <summary>Frontage (along the street) and total depth (porch + walls).</summary>
        public float Width;
        public float Depth;
        public float PorchDepth;

        /// <summary>World AABB of everything the lot occupies (porch included).</summary>
        public Rect Footprint;

        /// <summary>Middle of the door on the facade wall.</summary>
        public Vector2 Door;

        /// <summary>Where a visitor stands to use the door / talk to the keeper (on the porch edge / sidewalk).</summary>
        public Vector2 DoorApproach;

        /// <summary>Where the keeper stands (beside the door, never in front of it).</summary>
        public Vector2 StandPoint;

        public float InteractRadius = 2.4f;

        public TownBuildingSpec Building;

        /// <summary>Yaw (degrees) that turns the kit's south-facing (−z) front to <see cref="Front"/>.</summary>
        public float Yaw => CityGeometry.YawFacing(Front);

        public bool HasKeeper => !string.IsNullOrEmpty(ResidentId) || !string.IsNullOrEmpty(ShopId);
    }

    public sealed class CityStreet
    {
        public CityArm Arm;
        public float Length;
        public float Width;
        public float SidewalkWidth;

        /// <summary>Road + both sidewalks, from the plaza centre to the end of the arm.</summary>
        public Rect Bounds;

        /// <summary>The carriageway only.</summary>
        public Rect Road;

        public Vector2 Direction => CityGeometry.DirectionOf(Arm);
        public Vector2 End => Direction * Length;
    }

    public sealed class CityProp
    {
        public CityPropKind Kind;
        public Vector2 Position;
        public float Yaw;
        public float Scale = 1f;
        public int Variant;

        /// <summary>Fence runs: the other end (the run goes from <see cref="Position"/> to here).</summary>
        public Vector2 End;
    }

    public sealed class CityExit
    {
        public string Id;
        public CityExitKind Kind;

        /// <summary>Trigger box on the ground.</summary>
        public Rect Trigger;

        /// <summary>Point a walker heads for to use it (inside <see cref="Trigger"/>).</summary>
        public Vector2 Target;
    }

    /// <summary>
    /// A generated walkable town: plaza, streets, lots (with doors, keepers and approach points), props, exits, the
    /// respawn and arrival points and the safe-zone box. Plain data on the ground plane (x, z as a Vector2), built
    /// deterministically by <see cref="CityLayoutGenerator"/> from a <see cref="CityTownRecipe"/>; the scene view
    /// (<see cref="CityTownView"/>) and the EditMode rules (<see cref="CityLayoutRules"/>) both read it.
    /// </summary>
    public sealed class CityLayout
    {
        public string CityId;
        public BiomeType Region;
        public TownStyle Style;

        public Vector2 PlazaCenter;
        public float PlazaRadius;

        public readonly List<CityStreet> Streets = new List<CityStreet>();
        public readonly List<CityLot> Lots = new List<CityLot>();
        public readonly List<CityProp> Props = new List<CityProp>();
        public readonly List<CityExit> Exits = new List<CityExit>();

        /// <summary>Where a defeated hero wakes up (the plaza, Docs/Cities.md §3.8).</summary>
        public Vector2 Respawn;

        /// <summary>Where the hero appears when walking in from the map, and the direction they face.</summary>
        public Vector2 Arrival;
        public Vector2 ArrivalFacing = Vector2.up;

        /// <summary>The safe-zone trigger (SafeZoneRules: ZoneKind.City inside).</summary>
        public Rect SafeZone;

        /// <summary>Invisible walls keep the walker inside this box.</summary>
        public Rect WalkBounds;

        /// <summary>Half size of the painted ground (backdrop forest beyond the walk bounds).</summary>
        public float GroundHalfExtent = 90f;

        private List<CitySolid> _solids;

        public CityLot Lot(string id) => Lots.Find(lot => lot.Id == id);

        public CityLot LotOfResident(string residentId) => Lots.Find(lot => lot.ResidentId == residentId);

        public CityLot LotOfShop(string shopId) => Lots.Find(lot => lot.ShopId == shopId);

        public CityStreet Street(CityArm arm) => Streets.Find(street => street.Arm == arm);

        /// <summary>Every collider footprint of the town (cached; buildings, posts, trunks, props).</summary>
        public IReadOnlyList<CitySolid> Solids => _solids ?? (_solids = CitySolids.Build(this));

        /// <summary>Forget the cached solids (after editing the layout in a test).</summary>
        public void Invalidate() => _solids = null;

        /// <summary>
        /// Walking route from the plaza to <paramref name="lot"/>'s door: plaza centre, the point on the street axis
        /// opposite the door (street lots), then the door approach. Used by the PlayMode walk and as a hint for NPC AI.
        /// </summary>
        public List<Vector2> RouteToDoor(CityLot lot)
        {
            var route = new List<Vector2> { PlazaCenter };
            if (lot == null)
            {
                return route;
            }

            if (lot.Arm.HasValue)
            {
                route.Add(PlazaCenter + CityGeometry.DirectionOf(lot.Arm.Value) * lot.DoorAlong);
            }

            route.Add(lot.DoorApproach);
            return route;
        }
    }

    /// <summary>Ground-plane geometry helpers (pure).</summary>
    public static class CityGeometry
    {
        public static Vector2 DirectionOf(CityArm arm)
        {
            switch (arm)
            {
                case CityArm.North: return Vector2.up;
                case CityArm.East: return Vector2.right;
                case CityArm.South: return Vector2.down;
                default: return Vector2.left;
            }
        }

        /// <summary>Yaw that maps the kit's local −z (front) onto <paramref name="front"/> (x, z).</summary>
        public static float YawFacing(Vector2 front)
        {
            if (front.sqrMagnitude < 1e-6f)
            {
                return 0f;
            }

            float yaw = Mathf.Atan2(-front.x, -front.y) * Mathf.Rad2Deg;
            return yaw < 0f ? yaw + 360f : yaw;
        }

        /// <summary>Rotates a local ground offset (x right, y = local z) by <paramref name="yawDegrees"/> like Unity's Y rotation.</summary>
        public static Vector2 Rotate(Vector2 local, float yawDegrees)
        {
            float r = yawDegrees * Mathf.Deg2Rad;
            float c = Mathf.Cos(r);
            float s = Mathf.Sin(r);
            return new Vector2(local.x * c + local.y * s, -local.x * s + local.y * c);
        }

        public static Rect RectFromCenter(Vector2 center, Vector2 size) => new Rect(center - size * 0.5f, size);

        public static Rect RectFromMinMax(Vector2 min, Vector2 max) => Rect.MinMaxRect(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y),
            Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));

        /// <summary>AABB of a <paramref name="size"/> rectangle centred on <paramref name="center"/> turned by <paramref name="yaw"/>.</summary>
        public static Rect RotatedBounds(Vector2 center, Vector2 size, float yaw)
        {
            float r = yaw * Mathf.Deg2Rad;
            float c = Mathf.Abs(Mathf.Cos(r));
            float s = Mathf.Abs(Mathf.Sin(r));
            var half = new Vector2(size.x * 0.5f * c + size.y * 0.5f * s, size.x * 0.5f * s + size.y * 0.5f * c);
            return new Rect(center - half, half * 2f);
        }

        public static float DistanceToRect(Vector2 point, Rect rect)
        {
            float dx = Mathf.Max(rect.xMin - point.x, 0f, point.x - rect.xMax);
            float dy = Mathf.Max(rect.yMin - point.y, 0f, point.y - rect.yMax);
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>Strict overlap (touching edges do not count), shrunk by <paramref name="tolerance"/>.</summary>
        public static bool Overlaps(Rect a, Rect b, float tolerance = 0.001f)
        {
            return a.xMin < b.xMax - tolerance && b.xMin < a.xMax - tolerance && a.yMin < b.yMax - tolerance && b.yMin < a.yMax - tolerance;
        }

        public static bool RectOverlapsCircle(Rect rect, Vector2 center, float radius)
        {
            return DistanceToRect(center, rect) < radius;
        }

        public static bool Contains(Rect outer, Rect inner)
        {
            return inner.xMin >= outer.xMin && inner.xMax <= outer.xMax && inner.yMin >= outer.yMin && inner.yMax <= outer.yMax;
        }

        public static Rect Inflate(Rect rect, float amount) => new Rect(rect.x - amount, rect.y - amount, rect.width + amount * 2f, rect.height + amount * 2f);
    }
}
