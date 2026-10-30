using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The ground of a town as one mesh: a tiled field in the region's two ground colours, the streets (packed earth
    /// with stone edges, continuing out of town past each gate), raised paved sidewalks with curbs, the aryk channel along
    /// the streets with little plank bridges at the doors (Zarafshan, Qocho), the plaza (rings of brick and paving with a
    /// girih star at its centre, the respawn spot), swept earth under the lots and the yards. Visual only: the walkable
    /// ground is one flat BoxCollider at y = 0 (<see cref="CityTownView"/>).
    /// </summary>
    public static class TownGroundBuilder
    {
        public const float SidewalkHeight = 0.05f;
        public const float FieldCell = 6f;

        public static void Build(TownMeshBuilder b, CityLayout layout)
        {
            TownStyle style = layout.Style;
            Field(b, layout, style);
            foreach (CityLot lot in layout.Lots)
            {
                LotGround(b, lot, style);
            }

            foreach (CityStreet street in layout.Streets)
            {
                Street(b, layout, street, style);
            }

            Plaza(b, layout, style);
        }

        private static void Field(TownMeshBuilder b, CityLayout layout, TownStyle style)
        {
            float e = layout.GroundHalfExtent;
            int cells = Mathf.CeilToInt(e * 2f / FieldCell);
            for (int ix = 0; ix < cells; ix++)
            {
                for (int iz = 0; iz < cells; iz++)
                {
                    float x0 = -e + ix * FieldCell;
                    float z0 = -e + iz * FieldCell;
                    string key = layout.CityId + ix + "_" + iz;
                    TownSwatch a = TownStyle.Hash01(key, 1) < 0.3f ? style.GroundAlt : style.Ground;
                    TownSwatch c = TownStyle.Hash01(key, 2) < 0.3f ? style.GroundAlt : style.Ground;
                    var p00 = new Vector3(x0, 0f, z0);
                    var p01 = new Vector3(x0, 0f, z0 + FieldCell);
                    var p11 = new Vector3(x0 + FieldCell, 0f, z0 + FieldCell);
                    var p10 = new Vector3(x0 + FieldCell, 0f, z0);
                    b.Triangle(p00, p01, p11, a);
                    b.Triangle(p00, p11, p10, c);
                }
            }
        }

        private static void LotGround(TownMeshBuilder b, CityLot lot, TownStyle style)
        {
            if (lot.Kind == CityLotKind.Market)
            {
                return;
            }

            Rect r = CityGeometry.Inflate(lot.Footprint, 0.3f);
            b.Flat(r.xMin, r.yMin, r.xMax, r.yMax, 0.016f, TownSwatch.Dirt);
            // The yard behind the house: a little darker.
            Rect yard = CitySolids.LocalRectToWorld(lot, new Rect(-lot.Width * 0.5f, lot.Depth * 0.5f, lot.Width, 4.2f));
            if (lot.Kind != CityLotKind.LampHouse && lot.Kind != CityLotKind.ShipDock)
            {
                b.Flat(yard.xMin, yard.yMin, yard.xMax, yard.yMax, 0.01f, style.GroundAlt);
            }
        }

        private static void Street(TownMeshBuilder b, CityLayout layout, CityStreet street, TownStyle style)
        {
            Vector2 d = street.Direction;
            var n = new Vector2(-d.y, d.x);
            float halfRoad = street.Width * 0.5f;
            float halfStreet = halfRoad + street.SidewalkWidth;
            bool exit = layout.Exits.Exists(x => x.Id == "exit_" + street.Arm.ToString().ToLowerInvariant());
            float roadEnd = exit ? layout.GroundHalfExtent : street.Length + 1f;

            // Carriageway and its stone edges.
            Strip(b, d, n, 0f, roadEnd, -halfRoad, halfRoad, 0.022f, style.Street);
            Strip(b, d, n, 0f, roadEnd, -halfRoad - 0.05f, -halfRoad + 0.22f, 0.03f, style.StreetEdge);
            Strip(b, d, n, 0f, roadEnd, halfRoad - 0.22f, halfRoad + 0.05f, 0.03f, style.StreetEdge);
            // Worn wheel ruts.
            Strip(b, d, n, layout.PlazaRadius, roadEnd, -0.9f, -0.6f, 0.026f, TownSwatch.DirtDark);
            Strip(b, d, n, layout.PlazaRadius, roadEnd, 0.6f, 0.9f, 0.026f, TownSwatch.DirtDark);

            // Sidewalks, tiled, with curb faces; from the plaza edge to the end of the street.
            float from = layout.PlazaRadius + 0.25f;
            int tile = 0;
            for (float s = from; s < street.Length; s += 1.2f)
            {
                float s1 = Mathf.Min(street.Length, s + 1.2f);
                TownSwatch c = (tile++ & 1) == 0 ? style.Sidewalk : style.PlazaAlt == TownSwatch.Brick ? TownSwatch.PavingDark : style.PlazaAlt;
                Strip(b, d, n, s, s1, halfRoad, halfStreet, SidewalkHeight, c);
                Strip(b, d, n, s, s1, -halfStreet, -halfRoad, SidewalkHeight, c);
            }

            CurbFace(b, d, n, from, street.Length, halfRoad, -1f);
            CurbFace(b, d, n, from, street.Length, -halfRoad, 1f);

            if (style.Aryk)
            {
                // The aryk runs just inside the road edge on the shop side (+n), with plank bridges at the doors.
                float a0 = halfRoad - 0.5f;
                float a1 = halfRoad - 0.18f;
                Strip(b, d, n, from, street.Length, a0 - 0.08f, a0, 0.045f, TownSwatch.Stone);
                Strip(b, d, n, from, street.Length, a1, a1 + 0.08f, 0.045f, TownSwatch.Stone);
                Strip(b, d, n, from, street.Length, a0, a1, 0.034f, TownSwatch.Water);
                foreach (CityLot lot in layout.Lots)
                {
                    if (lot.Arm != street.Arm || Vector2.Dot(lot.Center, n) <= 0f)
                    {
                        continue;
                    }

                    float along = lot.DoorAlong;
                    Strip(b, d, n, along - 0.55f, along + 0.55f, a0 - 0.12f, a1 + 0.12f, 0.06f, TownSwatch.WoodLight);
                }
            }
        }

        /// <summary>An upward rectangle in street coordinates (along <paramref name="d"/>, across <paramref name="n"/>).</summary>
        private static void Strip(TownMeshBuilder b, Vector2 d, Vector2 n, float s0, float s1, float t0, float t1, float y, TownSwatch swatch)
        {
            Vector2 a = d * s0 + n * t0;
            Vector2 c = d * s1 + n * t1;
            Rect r = CityGeometry.RectFromMinMax(a, c);
            b.Flat(r.xMin, r.yMin, r.xMax, r.yMax, y, swatch);
        }

        private static void CurbFace(TownMeshBuilder b, Vector2 d, Vector2 n, float s0, float s1, float t, float facingSign)
        {
            Vector2 a = d * s0 + n * t;
            Vector2 c = d * s1 + n * t;
            var facing = new Vector3(n.x * facingSign, 0f, n.y * facingSign);
            b.QuadFacing(new Vector3(a.x, 0f, a.y), new Vector3(a.x, SidewalkHeight, a.y), new Vector3(c.x, SidewalkHeight, c.y), new Vector3(c.x, 0f, c.y), facing, TownSwatch.Curb);
        }

        private static void Plaza(TownMeshBuilder b, CityLayout layout, TownStyle style)
        {
            Vector3 c = new Vector3(layout.PlazaCenter.x, 0f, layout.PlazaCenter.y);
            float r = layout.PlazaRadius;
            const int sides = 32;
            b.Disc(c + Vector3.up * 0.036f, r, sides, style.Plaza);
            b.Ring(c + Vector3.up * 0.042f, r * 0.55f, r * 0.62f, sides, style.PlazaAlt);
            b.Ring(c + Vector3.up * 0.042f, r * 0.86f, r * 0.9f, sides, style.PlazaAlt, style.Plaza);
            b.Ring(c + Vector3.up * 0.05f, r, r + 0.25f, sides, TownSwatch.Curb);

            // Girih star at the centre (the respawn spot): two squares turned 45 degrees, accent and lime.
            Star(b, c + Vector3.up * 0.046f, 1.3f, 0f, style.Accent);
            Star(b, c + Vector3.up * 0.05f, 1.3f, 45f, style.Accent);
            b.Disc(c + Vector3.up * 0.054f, 0.55f, 8, TownSwatch.LimeWhite, 0.5f);

            // Paved path from the plaza to the lamp house.
            CityLot lamp = layout.Lots.Find(l => l.Kind == CityLotKind.LampHouse);
            if (lamp != null)
            {
                Vector2 from = layout.PlazaCenter + (lamp.Door - layout.PlazaCenter).normalized * (r - 0.4f);
                Rect path = CityGeometry.Inflate(CityGeometry.RectFromMinMax(from, lamp.Door), 0f);
                path = new Rect(path.x - 1.3f, path.y, path.width + 2.6f, path.height);
                b.Flat(path.xMin, path.yMin, path.xMax, path.yMax, 0.04f, style.PlazaAlt);
            }
        }

        private static void Star(TownMeshBuilder b, Vector3 c, float r, float rotation, TownSwatch swatch)
        {
            var points = new List<Vector3>(4);
            for (int i = 0; i < 4; i++)
            {
                float a = (rotation + 45f + i * 90f) * Mathf.Deg2Rad;
                points.Add(c + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * r);
            }

            b.QuadFacing(points[0], points[1], points[2], points[3], Vector3.up, swatch);
        }
    }

    /// <summary>
    /// Session cache of the town meshes (buildings by <see cref="TownBuildingSpec.CacheKey"/>, villagers by resident,
    /// whole-town chunks by city): equal buildings share one mesh, re-entering a city rebuilds nothing. Never saved.
    /// </summary>
    public static class TownMeshCache
    {
        private static readonly Dictionary<string, Mesh> Meshes = new Dictionary<string, Mesh>();

        public static int Count => Meshes.Count;

        public static Mesh Get(string key, System.Action<TownMeshBuilder> build)
        {
            if (Meshes.TryGetValue(key, out Mesh mesh) && mesh != null)
            {
                return mesh;
            }

            var b = new TownMeshBuilder();
            build(b);
            mesh = b.ToMesh(key);
            mesh.hideFlags = HideFlags.DontSave;
            mesh.UploadMeshData(false);
            Meshes[key] = mesh;
            return mesh;
        }

        public static void Clear()
        {
            foreach (Mesh mesh in Meshes.Values)
            {
                if (mesh == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Object.Destroy(mesh);
                }
                else
                {
                    Object.DestroyImmediate(mesh);
                }
            }

            Meshes.Clear();
        }
    }
}
