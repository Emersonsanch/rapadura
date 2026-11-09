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
                case CityPropKind.KarezShaft: return CitySolid.Capsule(prop.Position, 0.85f * s, 1f, owner);
                case CityPropKind.CisternDome: return CitySolid.Capsule(prop.Position, 1.95f * s, WallColliderHeight, owner);
                case CityPropKind.Ruin:
                    switch (prop.Variant % 3)
                    {
                        case 0: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position + CityGeometry.Rotate(new Vector2(0.1f, -0.2f) * s, prop.Yaw), new Vector2(3.1f, 3.6f) * s, prop.Yaw), WallColliderHeight, owner);
                        case 1: return CitySolid.Capsule(prop.Position, 0.45f * s, WallColliderHeight, owner);
                        default: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, new Vector2(3.1f, 0.7f) * s, prop.Yaw), 1.4f, owner);
                    }

                case CityPropKind.Camel: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, new Vector2(1.0f, 2.2f) * s, prop.Yaw), 1.4f, owner);
                case CityPropKind.RockArt: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position + CityGeometry.Rotate(new Vector2(0f, 0.2f) * s, prop.Yaw), new Vector2(3.0f, 1.5f) * s, prop.Yaw), WallColliderHeight, owner);
                case CityPropKind.Cliff:
                {
                    // The mass stands behind the line Position → End (left of the direction), CliffDepth deep.
                    Vector2 d = prop.End - prop.Position;
                    if (d.sqrMagnitude < 0.01f)
                    {
                        return null;
                    }

                    Vector2 back = TownKit.SegmentBack(d.normalized) * TownKit.CliffDepth;
                    return CitySolid.Box(CityGeometry.RectFromMinMax(prop.Position, prop.End + back), WallColliderHeight, owner);
                }

                case CityPropKind.Vishapakar: return CitySolid.Capsule(prop.Position, 0.6f * s, WallColliderHeight, owner);
                case CityPropKind.Fountain: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, new Vector2(2.3f, 1.1f) * s, prop.Yaw), 1.5f, owner);
                case CityPropKind.ConicalTower: return CitySolid.Capsule(prop.Position, 1.6f * s, WallColliderHeight, owner);
                case CityPropKind.CanopyTree: return CitySolid.Capsule(prop.Position, TownKit.CanopyTrunkSolidRadius * s, WallColliderHeight, owner);
                case CityPropKind.Dhow: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, new Vector2(TownKit.DhowBeam, TownKit.DhowLength) * s, prop.Yaw), 1.6f, owner);
                case CityPropKind.CoralTower: return CitySolid.Capsule(prop.Position, TownKit.CoralTowerSolidRadius * s, WallColliderHeight, owner);
                case CityPropKind.Mangrove: return CitySolid.Capsule(prop.Position, 0.9f * s, 2.5f, owner);
                case CityPropKind.DrumTower: return CitySolid.Box(CityGeometry.RectFromCenter(prop.Position, new Vector2(TownKit.DrumTowerSize, TownKit.DrumTowerSize) * s), WallColliderHeight, owner);
                case CityPropKind.ClanTent: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, new Vector2(TownKit.ClanTentSize, TownKit.ClanTentSize) * s, prop.Yaw), 2f, owner);
                case CityPropKind.ClanPost: return CitySolid.Capsule(prop.Position, 0.3f * s, WallColliderHeight, owner);
                case CityPropKind.Bonfire: return CitySolid.Capsule(prop.Position, 0.75f * s, 1f, owner);
                case CityPropKind.WarDrum: return CitySolid.Capsule(prop.Position, 0.85f * s, 1.6f, owner);
                case CityPropKind.Palisade:
                {
                    Rect run = CityGeometry.RectFromMinMax(prop.Position, prop.End);
                    return CitySolid.Box(CityGeometry.Inflate(run, TownKit.PalisadeHalfThickness), WallColliderHeight, owner);
                }

                case CityPropKind.BronzePillar: return CitySolid.Box(CityGeometry.RectFromCenter(prop.Position, new Vector2(TownKit.BronzePillarSize, TownKit.BronzePillarSize) * s), WallColliderHeight, owner);
                case CityPropKind.GreatForge: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, TownKit.GreatForgeSize * s, prop.Yaw), WallColliderHeight, owner);
                case CityPropKind.LavaPit: return CitySolid.Capsule(prop.Position, TownKit.LavaPitRadius * s, 1f, owner);
                case CityPropKind.ChainLift: return CitySolid.Box(CityGeometry.RectFromCenter(prop.Position, new Vector2(TownKit.ChainLiftSize, TownKit.ChainLiftSize) * s), WallColliderHeight, owner);
                case CityPropKind.RockWall:
                {
                    Rect run = CityGeometry.RectFromMinMax(prop.Position, prop.End);
                    return CitySolid.Box(CityGeometry.Inflate(run, TownKit.RockWallHalfThickness), WallColliderHeight, owner);
                }

                case CityPropKind.MineCart: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, TownKit.MineCartSize * s, prop.Yaw), 1.4f, owner);
                case CityPropKind.AirPipe: return CitySolid.Capsule(prop.Position, TownKit.AirPipeRadius * s, WallColliderHeight, owner);
                case CityPropKind.NicheStall: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, TownKit.NicheStallSize * s, prop.Yaw), WallColliderHeight, owner);
                case CityPropKind.Pulley: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, TownKit.PulleySize * s, prop.Yaw), WallColliderHeight, owner);

                case CityPropKind.HornPost: return CitySolid.Capsule(prop.Position, TownKit.HornPostRadius * s, WallColliderHeight, owner);
                case CityPropKind.StoneKraal: return CitySolid.Capsule(prop.Position, (TownKit.KraalRadius + 0.3f) * s, 1.2f, owner);
                case CityPropKind.BullLyre: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, TownKit.BullLyreSize * s, prop.Yaw), WallColliderHeight, owner);
                case CityPropKind.Acacia: return CitySolid.Capsule(prop.Position, TownKit.AcaciaTrunkRadius * s, 3f, owner);
                case CityPropKind.CoralMother: return CitySolid.Capsule(prop.Position, TownKit.CoralMotherRadius * s, 2f, owner);
                case CityPropKind.CoralSpire: return CitySolid.Capsule(prop.Position, TownKit.CoralSpireRadius * s, WallColliderHeight, owner);
                case CityPropKind.KelpGarden: return CitySolid.Capsule(prop.Position, TownKit.KelpGardenRadius * s, 2f, owner);
                case CityPropKind.ReefEdge:
                {
                    Rect run = CityGeometry.RectFromMinMax(prop.Position, prop.End);
                    return CitySolid.Box(CityGeometry.Inflate(run, TownKit.ReefEdgeHalfThickness), 1.2f, owner);
                }

                case CityPropKind.MotherRoot: return CitySolid.Capsule(prop.Position, TownKit.MotherRootRadius * s, WallColliderHeight, owner);
                case CityPropKind.Banyan: return CitySolid.Capsule(prop.Position, TownKit.BanyanRadius * s, WallColliderHeight, owner);
                case CityPropKind.LotusPond: return CitySolid.Capsule(prop.Position, TownKit.LotusPondRadius * s, 1.2f, owner);
                case CityPropKind.Ghat:
                {
                    Rect run = CityGeometry.RectFromMinMax(prop.Position, prop.End);
                    return CitySolid.Box(CityGeometry.Inflate(run, TownKit.GhatHalfThickness), 1.2f, owner);
                }

                case CityPropKind.SwampEdge:
                {
                    Rect run = CityGeometry.RectFromMinMax(prop.Position, prop.End);
                    return CitySolid.Box(CityGeometry.Inflate(run, TownKit.SwampEdgeHalfThickness), 1.2f, owner);
                }

                case CityPropKind.WispMast: return CitySolid.Capsule(prop.Position, TownKit.WispMastRadius * s, WallColliderHeight, owner);
                case CityPropKind.GiantBasket: return CitySolid.Capsule(prop.Position, TownKit.GiantBasketRadius * s, 1.8f, owner);
                case CityPropKind.NetRack: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, TownKit.NetRackSize * s, prop.Yaw), 1.8f, owner);
                case CityPropKind.ReedIsland: return CitySolid.Capsule(prop.Position, TownKit.ReedIslandRadius * s, 1.2f, owner);
                case CityPropKind.TerraceWall:
                {
                    Rect run = CityGeometry.RectFromMinMax(prop.Position, prop.End);
                    return CitySolid.Box(CityGeometry.Inflate(run, TownKit.TerraceWallHalfThickness), WallColliderHeight, owner);
                }

                case CityPropKind.StoneChute: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, TownKit.ChuteLandingSize * s, prop.Yaw), 1.6f, owner);
                case CityPropKind.ChuteCart: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, TownKit.ChuteCartSize * s, prop.Yaw), 1.2f, owner);
                case CityPropKind.ApricotTree: return CitySolid.Capsule(prop.Position, TownKit.ApricotTreeRadius * s, 3f, owner);
                case CityPropKind.CaravanStone: return CitySolid.Capsule(prop.Position, TownKit.CaravanStoneRadius * s, WallColliderHeight, owner);
                case CityPropKind.RingWall:
                {
                    Rect run = CityGeometry.RectFromMinMax(prop.Position, prop.End);
                    return CitySolid.Box(CityGeometry.Inflate(run, TownKit.RingWallHalfThickness), WallColliderHeight, owner);
                }

                case CityPropKind.BellDrumTower: return CitySolid.Capsule(prop.Position, TownKit.BellDrumTowerRadius * s, WallColliderHeight, owner);
                case CityPropKind.Willow: return CitySolid.Capsule(prop.Position, TownKit.WillowRadius * s, 3f, owner);
                case CityPropKind.LanternFrame: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, TownKit.LanternFrameSize * s, prop.Yaw), 2.2f, owner);
                case CityPropKind.SpireTerrace:
                {
                    Rect run = CityGeometry.RectFromMinMax(prop.Position, prop.End);
                    return CitySolid.Box(CityGeometry.Inflate(run, TownKit.SpireTerraceHalfThickness), WallColliderHeight, owner);
                }

                case CityPropKind.MirrorClock: return CitySolid.Capsule(prop.Position, TownKit.MirrorClockRadius * s, WallColliderHeight, owner);
                case CityPropKind.MirrorPost: return CitySolid.Capsule(prop.Position, TownKit.MirrorPostRadius * s, WallColliderHeight, owner);
                case CityPropKind.SpirePyramid: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, new Vector2(TownKit.SpirePyramidSize, TownKit.SpirePyramidSize) * s, prop.Yaw), WallColliderHeight, owner);
                case CityPropKind.FeltWagon: return CitySolid.Capsule(prop.Position, TownKit.FeltWagonRadius * s, 2.5f, owner);
                case CityPropKind.HorseStandard: return CitySolid.Capsule(prop.Position, TownKit.HorseStandardRadius * s, WallColliderHeight, owner);
                case CityPropKind.RopeCorral:
                {
                    Rect run = CityGeometry.RectFromMinMax(prop.Position, prop.End);
                    return CitySolid.Box(CityGeometry.Inflate(run, TownKit.CorralHalfThickness), 1.2f, owner);
                }

                case CityPropKind.StoneOven: return CitySolid.Capsule(prop.Position, TownKit.StoneOvenRadius * s, 2f, owner);
                case CityPropKind.EarthRing:
                {
                    Rect run = CityGeometry.RectFromMinMax(prop.Position, prop.End);
                    return CitySolid.Box(CityGeometry.Inflate(run, TownKit.EarthRingHalfThickness), WallColliderHeight, owner);
                }

                case CityPropKind.GiantTable: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, TownKit.GiantTableSize * s, prop.Yaw), 2.2f, owner);
                case CityPropKind.GiantPot: return CitySolid.Capsule(prop.Position, TownKit.GiantPotRadius * s, WallColliderHeight, owner);
                case CityPropKind.SpoonBench: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, TownKit.SpoonBenchSize * s, prop.Yaw), 1.2f, owner);
                case CityPropKind.MossStone: return CitySolid.Capsule(prop.Position, TownKit.MossStoneRadius * s, WallColliderHeight, owner);
                case CityPropKind.Larch: return CitySolid.Capsule(prop.Position, TownKit.LarchRadius * s, 3f, owner);
                case CityPropKind.WindBell: return CitySolid.Capsule(prop.Position, TownKit.WindBellRadius * s, WallColliderHeight, owner);
                case CityPropKind.StonePinnacle: return CitySolid.Capsule(prop.Position, TownKit.StonePinnacleRadius * s, WallColliderHeight, owner);
                case CityPropKind.RopeBasket: return CitySolid.Capsule(prop.Position, TownKit.RopeBasketRadius * s, 2.5f, owner);
                case CityPropKind.VineRow:
                {
                    Rect run = CityGeometry.RectFromMinMax(prop.Position, prop.End);
                    return CitySolid.Box(CityGeometry.Inflate(run, TownKit.VineRowHalfThickness), 1.8f, owner);
                }

                case CityPropKind.StoneStage: return CitySolid.Capsule(prop.Position, TownKit.StoneStageRadius * s, 1.2f, owner);
                case CityPropKind.GroveOak: return CitySolid.Capsule(prop.Position, TownKit.GroveOakRadius * s, 3f, owner);
                case CityPropKind.MoonQuay: return CitySolid.Capsule(prop.Position, TownKit.MoonQuayRadius * s, WallColliderHeight, owner);
                case CityPropKind.SnowDomeHut: return CitySolid.Capsule(prop.Position, TownKit.SnowDomeHutRadius * s, 2.5f, owner);
                case CityPropKind.CrescentBoat: return CitySolid.Capsule(prop.Position, TownKit.CrescentBoatRadius * s, 1.4f, owner);
                case CityPropKind.FishRack: return CitySolid.Capsule(prop.Position, TownKit.FishRackRadius * s, 2f, owner);
                case CityPropKind.IceBlock: return CitySolid.Capsule(prop.Position, TownKit.IceBlockRadius * s, WallColliderHeight, owner);
                case CityPropKind.IsleForge: return CitySolid.Capsule(prop.Position, TownKit.IsleForgeRadius * s, WallColliderHeight, owner);
                case CityPropKind.StoneLantern: return CitySolid.Capsule(prop.Position, TownKit.StoneLanternRadius * s, 2f, owner);
                case CityPropKind.PrunedPine: return CitySolid.Capsule(prop.Position, TownKit.PrunedPineRadius * s, 3f, owner);
                case CityPropKind.LavaRock: return CitySolid.Capsule(prop.Position, TownKit.LavaRockRadius * s, WallColliderHeight, owner);
                case CityPropKind.NightBarque: return CitySolid.Capsule(prop.Position, TownKit.NightBarqueRadius * s, WallColliderHeight, owner);
                case CityPropKind.PapyrusColumn: return CitySolid.Capsule(prop.Position, TownKit.PapyrusColumnRadius * s, 3f, owner);
                case CityPropKind.PapyrusThicket: return CitySolid.Capsule(prop.Position, TownKit.PapyrusThicketRadius * s, 2f, owner);
                case CityPropKind.ReedSkiff: return CitySolid.Capsule(prop.Position, TownKit.ReedSkiffRadius * s, 1.4f, owner);
                case CityPropKind.Ziggurat: return CitySolid.Box(CityGeometry.RotatedBounds(prop.Position, TownKit.ZigguratFootprint * s, prop.Yaw), WallColliderHeight, owner);
                case CityPropKind.ZurvanClock: return CitySolid.Capsule(prop.Position, TownKit.ZurvanClockRadius * s, 2f, owner);
                case CityPropKind.DatePalm: return CitySolid.Capsule(prop.Position, TownKit.DatePalmRadius * s, 3f, owner);
                case CityPropKind.GlazedLion: return CitySolid.Capsule(prop.Position, TownKit.GlazedLionRadius * s, 2.2f, owner);
                case CityPropKind.JarStack: return CitySolid.Capsule(prop.Position, TownKit.JarStackRadius * s, 1.4f, owner);

                case CityPropKind.MeltStream:
                case CityPropKind.CloudGap:
                case CityPropKind.IceCanal:
                case CityPropKind.LavaSluice:
                case CityPropKind.HourCanal:
                case CityPropKind.BitumenCanal:
                case CityPropKind.GoldenRiver:
                case CityPropKind.CanalBridge:
                case CityPropKind.LeafBridge:
                case CityPropKind.IrrigationChannel:
                case CityPropKind.Boardwalk:
                case CityPropKind.RootBridge:
                case CityPropKind.ShellBridge:
                case CityPropKind.KarezChannel:
                case CityPropKind.RopeBridge:
                case CityPropKind.LampString:
                case CityPropKind.TrainingRing:
                case CityPropKind.MineRails:
                    // Walkable (a channel to step over) or overhead.
                    return null;
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
