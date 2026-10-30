using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The collider footprints of a town, derived from the layout (pure). <see cref="CityTownView"/> turns each one into
    /// a real BoxCollider / CapsuleCollider (all on one GameObject: few objects, never a MeshCollider on a building), and
    /// <see cref="CityLayoutRules"/> checks walkability against exactly the same list. Building colliders stop at
    /// <see cref="WallColliderHeight"/> so the isometric camera's collision probe does not pull in behind every eave.
    /// </summary>
    public static class CitySolids
    {
        public const float WallColliderHeight = 2.6f;
        public const float PostRadius = 0.13f;
        public const float PostHeight = 2.4f;

        /// <summary>Porch decks are steps (CharacterController step offset 0.3), not walls: listed apart, never blocking.</summary>
        public const float DeckHeight = 0.16f;

        public static List<CitySolid> Build(CityLayout layout)
        {
            var solids = new List<CitySolid>();
            if (layout == null)
            {
                return solids;
            }

            foreach (CityLot lot in layout.Lots)
            {
                AddLot(solids, lot);
            }

            foreach (CityProp prop in layout.Props)
            {
                if (prop.Kind == CityPropKind.Gate)
                {
                    solids.AddRange(GatePosts(prop));
                    continue;
                }

                CitySolid? solid = PropSolid(prop, layout.WalkBounds);
                if (solid.HasValue)
                {
                    solids.Add(solid.Value);
                }
            }

            return solids;
        }

        /// <summary>Porch decks (low boxes the walker steps onto).</summary>
        public static List<Rect> Decks(CityLayout layout)
        {
            var decks = new List<Rect>();
            foreach (CityLot lot in layout.Lots)
            {
                if (lot.PorchDepth <= 0.01f || lot.Kind == CityLotKind.Market || lot.Kind == CityLotKind.ShipDock || lot.Kind == CityLotKind.Stable)
                {
                    continue;
                }

                var local = new Rect(-lot.Width * 0.5f, -lot.Depth * 0.5f, lot.Width, lot.PorchDepth);
                decks.Add(LocalRectToWorld(lot, local));
            }

            return decks;
        }

        public static Rect LocalRectToWorld(CityLot lot, Rect local)
        {
            float yaw = lot.Yaw;
            Vector2 a = lot.Center + CityGeometry.Rotate(local.min, yaw);
            Vector2 b = lot.Center + CityGeometry.Rotate(local.max, yaw);
            return CityGeometry.RectFromMinMax(a, b);
        }

        private static void AddLot(List<CitySolid> solids, CityLot lot)
        {
            float yaw = lot.Yaw;
            float hw = lot.Width * 0.5f;
            float hd = lot.Depth * 0.5f;
            float facadeZ = -hd + lot.PorchDepth;
            string owner = lot.Id;

            switch (lot.Kind)
            {
                case CityLotKind.Market:
                    return;

                case CityLotKind.ShipDock:
                {
                    // The cart (with the oxen yoked in front of it) along the street, and the shelter's back posts.
                    solids.Add(CitySolid.Box(LocalRectToWorld(lot, new Rect(-hw + 0.4f, facadeZ + 0.2f, lot.Width - 0.8f, 1.7f)), 1.6f, owner));
                    foreach (float x in new[] { -hw + 0.25f, hw - 0.25f })
                    {
                        solids.Add(CitySolid.Capsule(lot.Center + CityGeometry.Rotate(new Vector2(x, hd - 0.25f), yaw), PostRadius, PostHeight, owner));
                    }

                    return;
                }

                case CityLotKind.LampHouse:
                {
                    solids.Add(CitySolid.Box(LocalRectToWorld(lot, new Rect(-hw, -hd, lot.Width, lot.Depth)), WallColliderHeight, owner));
                    Vector2 towerCenter = TownBuildingBuilder.TowerLocalCenter(lot.Width, lot.Depth);
                    float s = TownBuildingBuilder.TowerSize;
                    solids.Add(CitySolid.Box(LocalRectToWorld(lot, new Rect(towerCenter.x - s * 0.5f, towerCenter.y - s * 0.5f, s, s)), WallColliderHeight, owner));
                    return;
                }
            }

            // Walls (the porch strip in front stays walkable).
            solids.Add(CitySolid.Box(LocalRectToWorld(lot, new Rect(-hw, facadeZ, lot.Width, lot.Depth - lot.PorchDepth)), WallColliderHeight, owner));

            if (lot.PorchDepth > 0.01f && lot.Building != null)
            {
                foreach (float x in TownBuildingBuilder.PorchPostsX(lot.Building))
                {
                    Vector2 local = new Vector2(x, -hd + TownBuildingBuilder.PostInset);
                    solids.Add(CitySolid.Capsule(lot.Center + CityGeometry.Rotate(local, yaw), PostRadius, PostHeight, owner));
                }
            }
        }

        /// <summary>Collider of a prop (null = walkable decoration, or scenery outside <paramref name="walkBounds"/>).</summary>
        public static CitySolid? PropSolid(CityProp prop, Rect walkBounds)
        {
            if (!walkBounds.Contains(prop.Position) && prop.Kind != CityPropKind.Gate)
            {
                return null;
            }

            string owner = prop.Kind.ToString();
            float s = prop.Scale <= 0f ? 1f : prop.Scale;
            switch (prop.Kind)
            {
                case CityPropKind.Well: return CitySolid.Capsule(prop.Position, 1.0f * s, 1.2f, owner);
                case CityPropKind.Bench: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, new Vector2(1.7f, 0.6f) * s, prop.Yaw), 0.9f, owner);
                case CityPropKind.LampPost: return CitySolid.Capsule(prop.Position, 0.14f, 3f, owner);
                case CityPropKind.Tree: return CitySolid.Capsule(prop.Position, 0.3f * s, 3f, owner);
                case CityPropKind.Stall: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, new Vector2(2.1f, 1.2f) * s, prop.Yaw), 1.1f, owner);
                case CityPropKind.Bush: return CitySolid.Capsule(prop.Position, 0.45f * s, 1f, owner);
                case CityPropKind.Crates: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, new Vector2(1.0f, 0.9f) * s, prop.Yaw), 1f, owner);
                case CityPropKind.HayBale: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, new Vector2(1.3f, 0.8f) * s, prop.Yaw), 0.8f, owner);
                case CityPropKind.Sundial: return CitySolid.Box(CityGeometry.RectFromCenter(prop.Position, new Vector2(1.1f, 1.1f) * s), 1.2f, owner);
                case CityPropKind.Rock: return CitySolid.Capsule(prop.Position, 0.5f * s, 1f, owner);
                case CityPropKind.Tandir: return CitySolid.Capsule(prop.Position, 0.6f, 1f, owner);
                case CityPropKind.Stela: return CitySolid.Box(CityGeometry.RectFromCenter(prop.Position, new Vector2(1.6f, 1.2f) * s), 2f, owner);
                case CityPropKind.FenceRun:
                {
                    Rect run = CityGeometry.RectFromMinMax(prop.Position, prop.End);
                    return CitySolid.Box(CityGeometry.Inflate(run, 0.06f), 1f, owner);
                }

                case CityPropKind.Gate:
                    // Only the two mourões are solid; the gateway itself is open.
                    return null;
                default:
                    return null;
            }
        }

        /// <summary>The two posts of a gate (porteira) at the town's exits.</summary>
        public static IEnumerable<CitySolid> GatePosts(CityProp gate)
        {
            yield return CitySolid.Capsule(gate.Position, 0.2f, 3.2f, "GatePost");
            yield return CitySolid.Capsule(gate.End, 0.2f, 3.2f, "GatePost");
        }
    }
}
