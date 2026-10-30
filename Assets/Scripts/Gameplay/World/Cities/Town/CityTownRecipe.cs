using System.Collections.Generic;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>A street leaving the plaza: how long it is and what waits at its end.</summary>
    public sealed class CityArmRecipe
    {
        public CityArm Arm;
        public float Length = 34f;

        /// <summary>The porteira at the end of the street leads back to the map (null = dead end / a lot closes it).</summary>
        public bool ExitToMap = true;

        public CityArmRecipe(CityArm arm, float length, bool exitToMap)
        {
            Arm = arm;
            Length = length;
            ExitToMap = exitToMap;
        }
    }

    /// <summary>
    /// One lot of a recipe. Street lots give the street (<see cref="Arm"/>), the side of it (<see cref="Side"/>, a compass
    /// direction perpendicular to the street) and the distance from the plaza centre (<see cref="Along"/>); free lots
    /// (<see cref="Free"/>: the lamp house at the head of the plaza, the stable at a street's end) give a centre and a front.
    /// </summary>
    public sealed class CityLotRecipe
    {
        public string Id;
        public CityLotKind Kind;
        public string ResidentId;
        public string ShopId;

        public CityArm Arm;
        public CityArm Side;
        public float Along;

        public bool Free;
        public Vector2 Center;
        public Vector2 Front = Vector2.down;

        public float Width = 6.5f;
        public float Depth = 5.5f;
        public float Porch = 1.4f;

        /// <summary>Optional fixed keeper spot / visitor spot (market stalls); null = computed from the door.</summary>
        public Vector2? StandOverride;
        public Vector2? ApproachOverride;

        /// <summary>Optional: start from this spec (the generator still fills size, colours left at default and the seed).</summary>
        public TownBuildingSpec Spec;
    }

    /// <summary>
    /// The data of one walkable town: plaza, streets, lots and hand-placed plaza props, plus switches for the props the
    /// generator adds on its own (street lamps between lots, backyards with fences and fruit trees, flower beds, the
    /// backdrop forest). The other cities are just more recipes like <see cref="CityTownRecipes.VilaEdna"/>.
    /// </summary>
    public sealed class CityTownRecipe
    {
        public string CityId;
        public BiomeType Region;

        public float PlazaRadius = 8f;
        public float StreetWidth = 4f;
        public float SidewalkWidth = 1.2f;

        /// <summary>Gap between the sidewalk and the porch front.</summary>
        public float Setback = 0.3f;

        /// <summary>The walker stays inside ±this (invisible walls); the safe zone is the same box.</summary>
        public float WalkHalfExtent = 40f;

        /// <summary>The street the hero walks in by (from the map).</summary>
        public CityArm EntryArm = CityArm.South;

        public readonly List<CityArmRecipe> Arms = new List<CityArmRecipe>();
        public readonly List<CityLotRecipe> Lots = new List<CityLotRecipe>();
        public readonly List<CityProp> PlazaProps = new List<CityProp>();

        public bool AutoLamps = true;
        public bool AutoYards = true;
        public bool Backdrop = true;

        /// <summary>Point lights at night (the lamps nearest the plaza get one, mobile budget).</summary>
        public int NightLights = 5;
    }

    /// <summary>
    /// Turns a <see cref="CityTownRecipe"/> into a <see cref="CityLayout"/>. Deterministic: positions come from the
    /// recipe, colours and small variations from <see cref="TownStyle.Hash"/> of the lot id, never from a runtime RNG, so
    /// every client (and the online server) builds the very same town. Automatic props that would land on a building,
    /// a street or another solid are dropped (the rules in <see cref="CityLayoutRules"/> hold by construction).
    /// </summary>
    public static class CityLayoutGenerator
    {
        public const float LampCurbInset = 0.3f;

        public static CityLayout Generate(CityTownRecipe recipe)
        {
            var layout = new CityLayout
            {
                CityId = recipe.CityId,
                Region = recipe.Region,
                Style = TownStyles.For(recipe.Region),
                PlazaCenter = Vector2.zero,
                PlazaRadius = recipe.PlazaRadius,
            };

            float halfRoad = recipe.StreetWidth * 0.5f;
            float halfStreet = halfRoad + recipe.SidewalkWidth;

            foreach (CityArmRecipe arm in recipe.Arms)
            {
                Vector2 d = CityGeometry.DirectionOf(arm.Arm);
                Vector2 n = new Vector2(Mathf.Abs(d.y), Mathf.Abs(d.x));
                var street = new CityStreet
                {
                    Arm = arm.Arm,
                    Length = arm.Length,
                    Width = recipe.StreetWidth,
                    SidewalkWidth = recipe.SidewalkWidth,
                    Bounds = CityGeometry.RectFromMinMax(-n * halfStreet, d * arm.Length + n * halfStreet),
                    Road = CityGeometry.RectFromMinMax(-n * halfRoad, d * arm.Length + n * halfRoad),
                };
                layout.Streets.Add(street);

                if (arm.ExitToMap)
                {
                    Vector2 a = d * (arm.Length - 1.6f) - n * halfStreet;
                    Vector2 b = d * (arm.Length + 1.2f) + n * halfStreet;
                    layout.Exits.Add(new CityExit
                    {
                        Id = "exit_" + arm.Arm.ToString().ToLowerInvariant(),
                        Kind = CityExitKind.ToMap,
                        Trigger = CityGeometry.RectFromMinMax(a, b),
                        Target = d * (arm.Length - 0.4f),
                    });
                }
            }

            foreach (CityLotRecipe lotRecipe in recipe.Lots)
            {
                layout.Lots.Add(BuildLot(recipe, layout.Style, lotRecipe, halfStreet));
            }

            foreach (CityLot lot in layout.Lots)
            {
                if (lot.Kind == CityLotKind.ShipDock)
                {
                    layout.Exits.Add(new CityExit
                    {
                        Id = "cais_da_barca",
                        Kind = CityExitKind.ShipDock,
                        Trigger = CityGeometry.RectFromCenter(lot.DoorApproach, new Vector2(1.6f, 1.6f)),
                        Target = lot.DoorApproach,
                    });
                }
            }

            layout.Respawn = layout.PlazaCenter;
            CityStreet entry = layout.Street(recipe.EntryArm) ?? (layout.Streets.Count > 0 ? layout.Streets[0] : null);
            if (entry != null)
            {
                layout.Arrival = entry.Direction * (entry.Length - 4.5f);
                layout.ArrivalFacing = -entry.Direction;
            }

            float h = recipe.WalkHalfExtent;
            layout.WalkBounds = new Rect(-h, -h, h * 2f, h * 2f);
            layout.SafeZone = layout.WalkBounds;

            layout.Props.AddRange(recipe.PlazaProps);
            var occupied = new List<CitySolid>(CitySolids.Build(layout));

            if (recipe.AutoLamps)
            {
                AddStreetLamps(recipe, layout, occupied, halfRoad);
            }

            AddGates(layout, halfStreet);
            if (recipe.AutoYards)
            {
                AddYards(layout, occupied);
            }

            if (recipe.Backdrop)
            {
                AddBackdrop(layout);
            }

            layout.Invalidate();
            return layout;
        }

        // ------------------------------------------------------------------
        // Lots
        // ------------------------------------------------------------------

        private static CityLot BuildLot(CityTownRecipe recipe, TownStyle style, CityLotRecipe r, float halfStreet)
        {
            float porch = r.Kind == CityLotKind.LampHouse || r.Kind == CityLotKind.Market ? 0f : Mathf.Max(0f, r.Porch);
            float depth = r.Depth + porch;
            var lot = new CityLot
            {
                Id = r.Id,
                Kind = r.Kind,
                ResidentId = r.ResidentId,
                ShopId = r.ShopId,
                Width = r.Width,
                Depth = depth,
                PorchDepth = porch,
            };

            if (r.Free)
            {
                lot.Center = r.Center;
                lot.Front = r.Front.sqrMagnitude > 0f ? r.Front.normalized : Vector2.down;
            }
            else
            {
                Vector2 d = CityGeometry.DirectionOf(r.Arm);
                Vector2 side = CityGeometry.DirectionOf(r.Side);
                float frontLine = halfStreet + recipe.Setback;
                lot.Arm = r.Arm;
                lot.Center = d * r.Along + side * (frontLine + depth * 0.5f);
                lot.Front = -side;
            }

            float yaw = lot.Yaw;
            Vector2 localSize = new Vector2(lot.Width, lot.Depth);
            lot.Footprint = CityGeometry.RotatedBounds(lot.Center, localSize, yaw);
            if (r.Kind == CityLotKind.LampHouse)
            {
                // The bell tower stands beside the nave, on its left (seen from the plaza).
                Rect tower = CityGeometry.RotatedBounds(lot.Center + CityGeometry.Rotate(TownBuildingBuilder.TowerLocalCenter(r.Width, depth), yaw),
                    new Vector2(TownBuildingBuilder.TowerSize, TownBuildingBuilder.TowerSize), yaw);
                lot.Footprint = Union(lot.Footprint, tower);
            }

            lot.Building = BuildSpec(style, r, porch);

            // Door on the facade wall (porch depth back from the front line); the keeper beside it on the porch.
            float doorX = lot.Building.DoorOffset * lot.Width;
            float facadeZ = -depth * 0.5f + porch;
            lot.Door = lot.Center + CityGeometry.Rotate(new Vector2(doorX, facadeZ), yaw);
            float keeperSide = doorX > 0f ? -1f : 1f;
            float keeperX = doorX + keeperSide * Mathf.Min(1.05f, lot.Width * 0.5f - 0.6f);
            Vector2 stand = r.Kind == CityLotKind.Workshop
                ? new Vector2(doorX + keeperSide * 0.9f, facadeZ - 0.55f)
                : new Vector2(keeperX, facadeZ - Mathf.Clamp(porch * 0.55f, 0.6f, 0.9f));
            Vector2 approach = new Vector2(doorX, -depth * 0.5f - 0.8f);
            if (r.Kind == CityLotKind.LampHouse)
            {
                stand = new Vector2(doorX + 1.6f, facadeZ - 0.8f);
            }

            lot.StandPoint = r.StandOverride ?? lot.Center + CityGeometry.Rotate(stand, yaw);
            lot.DoorApproach = r.ApproachOverride ?? lot.Center + CityGeometry.Rotate(approach, yaw);
            if (lot.Arm.HasValue)
            {
                lot.DoorAlong = Vector2.Dot(lot.Door, CityGeometry.DirectionOf(lot.Arm.Value));
            }

            lot.InteractRadius = Vector2.Distance(lot.StandPoint, lot.DoorApproach) + 0.7f;
            return lot;
        }

        private static Rect Union(Rect a, Rect b) => Rect.MinMaxRect(Mathf.Min(a.xMin, b.xMin), Mathf.Min(a.yMin, b.yMin), Mathf.Max(a.xMax, b.xMax), Mathf.Max(a.yMax, b.yMax));

        private static TownBuildingSpec BuildSpec(TownStyle style, CityLotRecipe r, float porch)
        {
            TownBuildingSpec spec = r.Spec != null ? r.Spec.Clone() : new TownBuildingSpec();
            string key = r.Id ?? string.Empty;
            spec.Kind = r.Kind;
            spec.Width = r.Width;
            spec.Depth = r.Depth;
            spec.PorchDepth = porch;
            spec.Seed = key;

            if (r.Spec == null)
            {
                spec.WallColor = TownStyle.Pick(style.Walls, key, 1);
                spec.TrimColor = TownStyle.Pick(style.Trims, key, 2);
                spec.RoofColor = TownStyle.Pick(style.Roofs, key, 3);
                spec.JoineryColor = TownStyle.Pick(style.Joinery, key, 4);
                spec.AccentColor = style.Accent;
                spec.PlinthColor = style.Plinth;
                spec.Roof = style.HouseRoof;
                spec.WallHeight = 2.8f + TownStyle.Hash01(key, 7) * 0.5f;
                spec.DoorOffset = r.Width >= 6.5f ? (TownStyle.Hash01(key, 9) < 0.5f ? -0.18f : 0.18f) : 0f;
                spec.FrontWindows = r.Width >= 7f ? 2 : 1;
                spec.Chimney = (TownStyle.Hash(key, 10) % 4) == 0;
                spec.UpperRoom = style.HouseRoof == TownRoofKind.Flat && (TownStyle.Hash(key, 11) % 3) == 0;
                spec.StorkNest = style.Region == BiomeType.Forest && (TownStyle.Hash(key, 12) % 5) == 0;
            }
            else
            {
                // Spec given: keep what the recipe chose, fill only the plinth from the region.
                spec.PlinthColor = r.Spec.PlinthColor == TownSwatch.Stone ? style.Plinth : r.Spec.PlinthColor;
            }

            switch (r.Kind)
            {
                case CityLotKind.Shop:
                case CityLotKind.Workshop:
                    spec.Sign = true;
                    break;
                case CityLotKind.LampHouse:
                    spec.Roof = TownRoofKind.Dome;
                    spec.DoorOffset = 0f;
                    spec.Sign = false;
                    spec.Dressing = TownShopDressing.LampHouse;
                    break;
                case CityLotKind.Stable:
                    spec.WideDoor = true;
                    spec.Sign = true;
                    break;
            }

            return spec;
        }

        // ------------------------------------------------------------------
        // Automatic props
        // ------------------------------------------------------------------

        private static void AddStreetLamps(CityTownRecipe recipe, CityLayout layout, List<CitySolid> occupied, float halfRoad)
        {
            foreach (CityStreet street in layout.Streets)
            {
                Vector2 d = street.Direction;
                Vector2 n = new Vector2(-d.y, d.x);
                int index = 0;
                for (float along = layout.PlazaRadius + 4f; along < street.Length - 2f; along += 8f)
                {
                    float side = (index++ & 1) == 0 ? 1f : -1f;
                    Vector2 position = d * along + n * side * (halfRoad + LampCurbInset);
                    TryAddProp(layout, occupied, new CityProp { Kind = CityPropKind.LampPost, Position = position, Yaw = CityGeometry.YawFacing(-n * side) }, 0.9f);
                }
            }
        }

        private static void AddGates(CityLayout layout, float halfStreet)
        {
            foreach (CityExit exit in layout.Exits)
            {
                if (exit.Kind != CityExitKind.ToMap)
                {
                    continue;
                }

                CityStreet street = layout.Streets.Find(s => exit.Id == "exit_" + s.Arm.ToString().ToLowerInvariant());
                if (street == null)
                {
                    continue;
                }

                Vector2 d = street.Direction;
                Vector2 n = new Vector2(-d.y, d.x);
                Vector2 at = d * (street.Length + 1.6f);
                layout.Props.Add(new CityProp
                {
                    Kind = CityPropKind.Gate,
                    Position = at + n * (halfStreet + 0.2f),
                    End = at - n * (halfStreet + 0.2f),
                    Yaw = CityGeometry.YawFacing(-d),
                });
            }
        }

        /// <summary>Backyards: a fence around the back of each house lot, a fruit tree or two, a flower bed by the porch.</summary>
        private static void AddYards(CityLayout layout, List<CitySolid> occupied)
        {
            foreach (CityLot lot in layout.Lots)
            {
                if (lot.Kind == CityLotKind.Market || lot.Kind == CityLotKind.LampHouse || lot.Kind == CityLotKind.ShipDock)
                {
                    continue;
                }

                float yaw = lot.Yaw;
                string key = lot.Id ?? string.Empty;
                float yardDepth = 4.2f;
                float hw = lot.Width * 0.5f;
                float back = lot.Depth * 0.5f;

                // Trees in the yard.
                int trees = 1 + (int)(TownStyle.Hash(key, 20) % 2);
                for (int i = 0; i < trees; i++)
                {
                    float u = trees == 1 ? 0f : (i == 0 ? -0.5f : 0.5f);
                    var local = new Vector2(u * hw + (TownStyle.Hash01(key, 21 + i) - 0.5f) * 1.2f, back + 1.8f + TownStyle.Hash01(key, 23 + i) * 1.6f);
                    TryAddProp(layout, occupied, new CityProp
                    {
                        Kind = CityPropKind.Tree,
                        Position = lot.Center + CityGeometry.Rotate(local, yaw),
                        Yaw = TownStyle.Hash01(key, 25 + i) * 360f,
                        Scale = 0.85f + TownStyle.Hash01(key, 27 + i) * 0.4f,
                        Variant = (int)(TownStyle.Hash(key, 29 + i) % 4),
                    }, 0.4f);
                }

                // Yard fence: back and both sides, set 0.3 m off the walls.
                Vector2 bl = new Vector2(-hw + 0.1f, back + 0.3f);
                Vector2 br = new Vector2(hw - 0.1f, back + 0.3f);
                Vector2 blb = new Vector2(-hw + 0.1f, back + yardDepth);
                Vector2 brb = new Vector2(hw - 0.1f, back + yardDepth);
                TryAddFence(layout, occupied, lot.Center + CityGeometry.Rotate(blb, yaw), lot.Center + CityGeometry.Rotate(brb, yaw));
                TryAddFence(layout, occupied, lot.Center + CityGeometry.Rotate(bl, yaw), lot.Center + CityGeometry.Rotate(blb, yaw));
                TryAddFence(layout, occupied, lot.Center + CityGeometry.Rotate(br, yaw), lot.Center + CityGeometry.Rotate(brb, yaw));

                // A flower bed against the facade, on the side away from the door and the keeper.
                if (lot.Kind == CityLotKind.House || lot.Kind == CityLotKind.Grandma)
                {
                    float doorX = lot.Building.DoorOffset * lot.Width;
                    float bedX = doorX >= 0f ? -hw * 0.6f : hw * 0.6f;
                    var bed = new Vector2(bedX, -lot.Depth * 0.5f - 0.45f);
                    TryAddProp(layout, occupied, new CityProp
                    {
                        Kind = CityPropKind.FlowerBed,
                        Position = lot.Center + CityGeometry.Rotate(bed, yaw),
                        Yaw = yaw,
                        Variant = (int)(TownStyle.Hash(key, 31) % 3),
                    }, 0f);
                }
            }
        }

        private static void TryAddFence(CityLayout layout, List<CitySolid> occupied, Vector2 a, Vector2 b)
        {
            var prop = new CityProp { Kind = CityPropKind.FenceRun, Position = a, End = b };
            CitySolid? solid = CitySolids.PropSolid(prop, layout.WalkBounds);
            if (solid.HasValue && !Blocks(layout, occupied, solid.Value, 0.05f, allowLotOverlap: false))
            {
                layout.Props.Add(prop);
                occupied.Add(solid.Value);
            }
        }

        private static void TryAddProp(CityLayout layout, List<CitySolid> occupied, CityProp prop, float clearance)
        {
            CitySolid? solid = CitySolids.PropSolid(prop, layout.WalkBounds);
            if (!solid.HasValue)
            {
                // Walkable decoration (flower bed): only keep it off the streets and buildings.
                Rect spot = CityGeometry.RectFromCenter(prop.Position, new Vector2(1.4f, 0.7f));
                foreach (CityStreet street in layout.Streets)
                {
                    if (CityGeometry.Overlaps(street.Bounds, spot))
                    {
                        return;
                    }
                }

                foreach (CityLot lot in layout.Lots)
                {
                    if (lot.Kind != CityLotKind.Market && CityGeometry.Overlaps(lot.Footprint, spot))
                    {
                        return;
                    }
                }

                layout.Props.Add(prop);
                return;
            }

            if (!Blocks(layout, occupied, solid.Value, clearance, allowLotOverlap: false))
            {
                layout.Props.Add(prop);
                occupied.Add(solid.Value);
            }
        }

        /// <summary>True when <paramref name="solid"/> would sit on a building, a road, the plaza, a door path or too close to another solid.</summary>
        private static bool Blocks(CityLayout layout, List<CitySolid> occupied, CitySolid solid, float clearance, bool allowLotOverlap)
        {
            Rect area = CityGeometry.Inflate(solid.Footprint, clearance * 0.5f);
            foreach (CityStreet street in layout.Streets)
            {
                // Lamps stand on the curb: only the carriageway is off limits for them.
                Rect forbidden = solid.Owner == "LampPost" ? street.Road : street.Bounds;
                if (CityGeometry.Overlaps(forbidden, solid.Footprint))
                {
                    return true;
                }
            }

            if (solid.Owner != "LampPost" && CityGeometry.RectOverlapsCircle(solid.Footprint, layout.PlazaCenter, layout.PlazaRadius))
            {
                return true;
            }

            foreach (CityLot lot in layout.Lots)
            {
                if (!allowLotOverlap && CityGeometry.Overlaps(CityGeometry.Inflate(lot.Footprint, 0.2f), solid.Footprint))
                {
                    return true;
                }

                // Never between a door and the street.
                if (Vector2.Distance(lot.DoorApproach, solid.Center) < 1.6f + solid.Radius || Vector2.Distance(lot.StandPoint, solid.Center) < 1.2f + solid.Radius)
                {
                    return true;
                }
            }

            foreach (CityExit exit in layout.Exits)
            {
                if (CityGeometry.Overlaps(CityGeometry.Inflate(exit.Trigger, 0.5f), solid.Footprint))
                {
                    return true;
                }
            }

            foreach (CitySolid other in occupied)
            {
                if (CityGeometry.Overlaps(area, CityGeometry.Inflate(other.Footprint, clearance * 0.5f)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The forest (or the region's scenery) beyond the walk bounds: trees and low hills, no colliders.</summary>
        private static void AddBackdrop(CityLayout layout)
        {
            float inner = layout.WalkBounds.width * 0.5f + 1.5f;
            string key = layout.CityId ?? "town";
            int count = 150;
            for (int i = 0; i < count; i++)
            {
                float angle = (i + TownStyle.Hash01(key, 100 + i) * 0.8f) / count * Mathf.PI * 2f;
                float radius = inner + TownStyle.Hash01(key, 400 + i) * 26f;
                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                // Square-ish ring: push points out to the bounds' square so corners are covered too.
                float square = Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
                Vector2 position = dir / square * radius;
                if (OnRoadOut(layout, position))
                {
                    continue;
                }

                layout.Props.Add(new CityProp
                {
                    Kind = CityPropKind.Tree,
                    Position = position,
                    Yaw = TownStyle.Hash01(key, 700 + i) * 360f,
                    Scale = 1f + TownStyle.Hash01(key, 1000 + i) * 0.8f,
                    Variant = (int)(TownStyle.Hash(key, 1300 + i) % 4),
                });
            }

            for (int i = 0; i < 26; i++)
            {
                float angle = i / 26f * Mathf.PI * 2f;
                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                float square = Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
                Vector2 position = dir / square * (inner + 30f + TownStyle.Hash01(key, 1600 + i) * 12f);
                layout.Props.Add(new CityProp
                {
                    Kind = CityPropKind.Rock,
                    Position = position,
                    Scale = 9f + TownStyle.Hash01(key, 1700 + i) * 7f,
                    Variant = 9,
                });
            }
        }

        /// <summary>Keeps the backdrop off the roads leaving town, so each exit shows its road going on.</summary>
        private static bool OnRoadOut(CityLayout layout, Vector2 position)
        {
            foreach (CityStreet street in layout.Streets)
            {
                Vector2 d = street.Direction;
                float along = Vector2.Dot(position, d);
                float across = Mathf.Abs(Vector2.Dot(position, new Vector2(-d.y, d.x)));
                if (along > street.Length - 2f && across < street.Width * 0.5f + 2.5f)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
