using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of the Cais de Mesektet, the quay town of the Rio das Doze Horas (Docs/Mapas100.md map 71, style
    /// <c>PapyrusQuay</c>): the halls of the quay (<see cref="PapyrusHallHouse"/>: a raft of bound papyrus logs on pilings over
    /// the dark water, sandstone walls with a gold band and a cavetto cornice, a front of papyrus-bundle columns with bell
    /// capitals holding a flat roof of reed mats that overhangs as a porch, stars painted under the mats), the Cais da Barca on
    /// the plaza with the Mesektet moored at it, the piers of the hours (channels of dark water crossed by plank bridges, with
    /// mooring posts and hour-lamps), the freestanding papyrus columns with an oil lamp, the thickets of papyrus, the reed
    /// skiffs drawn up on the bank and the gate of two papyrus columns under a cornice; Khunanup's reed market front. The river
    /// towns of Kemet and their papyrus boats are form only: no temple, no figure of a god. Same rules as the rest of the kit:
    /// flat colours from <see cref="TownPalette"/> (row 23 holds the river night, the gold of Kemet, papyrus, the quay
    /// sandstone, the dark water, the reed mats, linen and the starlight that glows), low-poly, deterministic.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Radius of the <see cref="CityPropKind.NightBarque"/>'s landing and moored barque (× its scale): its collider.</summary>
        public const float NightBarqueRadius = 1.8f;

        /// <summary>Radius of a <see cref="CityPropKind.PapyrusColumn"/>'s drum (× its scale): its collider.</summary>
        public const float PapyrusColumnRadius = 0.55f;

        /// <summary>Radius of a <see cref="CityPropKind.PapyrusThicket"/> (× its scale): its collider.</summary>
        public const float PapyrusThicketRadius = 0.9f;

        /// <summary>Radius of a <see cref="CityPropKind.ReedSkiff"/> (× its scale): its collider.</summary>
        public const float ReedSkiffRadius = 0.9f;

        /// <summary>Half the width of the dark water of a <see cref="CityPropKind.HourCanal"/>.</summary>
        public const float HourCanalHalfWidth = 0.85f;

        /// <summary>Height of the raft of papyrus logs a <see cref="TownBuildingSpec.PapyrusHall"/> stands on.</summary>
        private const float RaftTop = 0.4f;

        // ------------------------------------------------------------------
        // The hall of the quay
        // ------------------------------------------------------------------

        /// <summary>
        /// A <see cref="TownBuildingSpec.PapyrusHall"/> (local frame, front −z): pilings in the dark water and a raft of bound
        /// papyrus logs, sandstone walls (<see cref="TownBuildingSpec.WallColor"/>) with a gold band (<see cref="TownBuildingSpec.AccentColor"/>)
        /// and a cavetto cornice (<see cref="TownBuildingSpec.TrimColor"/>), a door with a gold lintel, windows of lit linen; a
        /// flat roof of reed mats (<see cref="TownBuildingSpec.RoofColor"/>) on beams with stars painted under it; in front the
        /// porch: the mat roof carried forward on papyrus-bundle columns with bell capitals (at
        /// <see cref="TownBuildingBuilder.PorchPostsX"/>, the colliders use the same list), an oil lamp and the gold plaque.
        /// </summary>
        public static void PapyrusHallHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            float eave = Mathf.Max(top, RaftTop + 2.4f);
            float wx = hw - 0.2f;
            float zw = zFacade + 0.15f, zb = zBack - 0.15f;

            // The pilings and the raft of bound papyrus logs, running front to back, tied with gold-dyed cords.
            b.BoxMinMax(new Vector3(-hw, 0f, zFront), new Vector3(hw, 0.05f, zBack), TownSwatch.NightWater);
            foreach (float px in new[] { -hw + 0.25f, hw - 0.25f })
            {
                foreach (float pz in new[] { zFacade + 0.2f, zBack - 0.2f })
                {
                    b.Cylinder(new Vector3(px, 0f, pz), 0.12f, 0.12f, RaftTop, 5, TownSwatch.WoodDark);
                }
            }

            int logs = Mathf.Clamp(Mathf.RoundToInt(hw * 2f / 0.5f), 6, 18);
            for (int i = 0; i < logs; i++)
            {
                float x = Mathf.Lerp(-hw + 0.22f, hw - 0.22f, i / (logs - 1f));
                b.Log(new Vector3(x, RaftTop - 0.12f, zFront - 0.05f), new Vector3(x, RaftTop - 0.12f, zBack + 0.05f), 0.13f, TownSwatch.MesektetMat, 5);
            }

            foreach (float z in new[] { zFront + 0.25f, (zFacade + zBack) * 0.5f, zBack - 0.25f })
            {
                b.Box(new Vector3(0f, RaftTop - 0.02f, z), new Vector3(hw * 2f + 0.05f, 0.06f, 0.1f), TownSwatch.KemetGold, 0f);
            }

            // The sandstone walls, a gold band and a dado of papyrus green.
            b.BoxMinMax(new Vector3(-wx, RaftTop, zw), new Vector3(wx, eave, zb), spec.WallColor, spec.WallColor);
            b.Box(new Vector3(0f, RaftTop + 0.25f, zw - 0.03f), new Vector3(wx * 2f, 0.3f, 0.05f), TownSwatch.PapyrusGreen, 0f);
            b.Box(new Vector3(0f, eave - 0.32f, zw - 0.04f), new Vector3(wx * 2f, 0.14f, 0.06f), spec.AccentColor, 0f);

            // The cavetto cornice: a torus roll and a flared gorge on the front and the sides.
            b.Log(new Vector3(-wx - 0.05f, eave - 0.12f, zw - 0.08f), new Vector3(wx + 0.05f, eave - 0.12f, zw - 0.08f), 0.06f, spec.AccentColor, 4);
            b.Slab(new Vector3(-wx - 0.1f, eave - 0.06f, zw - 0.1f), new Vector3(-wx - 0.3f, eave + 0.35f, zw - 0.35f),
                new Vector3(wx + 0.3f, eave + 0.35f, zw - 0.35f), new Vector3(wx + 0.1f, eave - 0.06f, zw - 0.1f), 0.08f, spec.TrimColor, spec.TrimColor);
            foreach (float side in new[] { -1f, 1f })
            {
                b.Box(new Vector3(side * (wx + 0.08f), eave + 0.12f, (zw + zb) * 0.5f), new Vector3(0.18f, 0.4f, zb - zw), spec.TrimColor, 0f);
            }

            // The door with its gold lintel and the windows of lit linen.
            float doorX = Mathf.Clamp(spec.DoorOffset * spec.Width, -wx + 0.8f, wx - 0.8f);
            float door = spec.WideDoor ? 0.75f : 0.55f;
            float doorTop = RaftTop + 2.0f;
            b.BoxMinMax(new Vector3(doorX - door - 0.1f, RaftTop, zw - 0.07f), new Vector3(doorX + door + 0.1f, doorTop + 0.1f, zw - 0.02f), spec.JoineryColor);
            b.Box(new Vector3(doorX, (RaftTop + doorTop) * 0.5f, zw - 0.09f), new Vector3(door * 2f - 0.1f, doorTop - RaftTop - 0.05f, 0.03f), TownSwatch.RiverNight, 0f);
            b.Box(new Vector3(doorX, doorTop + 0.18f, zw - 0.1f), new Vector3(door * 2f + 0.5f, 0.2f, 0.08f), spec.AccentColor, 0f);

            int windows = Mathf.Clamp(spec.FrontWindows, 0, 2);
            for (int i = 0; i < windows; i++)
            {
                float side = i == 0 ? -1f : 1f;
                float x = Mathf.Clamp(doorX + side * (door + 1.0f), -wx + 0.45f, wx - 0.45f);
                if (Mathf.Abs(x - doorX) < door + 0.45f)
                {
                    continue;
                }

                float y = eave - 1.05f;
                b.Box(new Vector3(x, y, zw - 0.05f), new Vector3(0.6f, 0.5f, 0.05f), spec.JoineryColor, 0f);
                b.Box(new Vector3(x, y, zw - 0.08f), new Vector3(0.46f, 0.36f, 0.03f), TownSwatch.WindowLit, 0f);
                b.Box(new Vector3(x, y + 0.32f, zw - 0.08f), new Vector3(0.7f, 0.08f, 0.06f), spec.AccentColor, 0f);
            }

            // The flat roof of reed mats on the wall top, the beams showing and the stars painted under the overhang.
            float roofY = eave + 0.38f;
            b.BoxMinMax(new Vector3(-hw - 0.15f, roofY, zFront - 0.15f), new Vector3(hw + 0.15f, roofY + 0.12f, zBack + 0.15f), spec.RoofColor, spec.RoofColor);
            int beams = Mathf.Clamp(Mathf.RoundToInt(hw * 2f / 1.2f), 3, 8);
            for (int i = 0; i < beams; i++)
            {
                float x = Mathf.Lerp(-hw, hw, i / (beams - 1f));
                b.Box(new Vector3(x, roofY - 0.06f, (zFront + zBack) * 0.5f), new Vector3(0.12f, 0.12f, zBack - zFront + 0.3f), spec.JoineryColor, 0f);
            }

            b.Box(new Vector3(0f, roofY + 0.2f, zBack - 0.1f), new Vector3(hw * 2f, 0.16f, 0.12f), spec.TrimColor, 0f);

            if (spec.PorchDepth > 0.01f)
            {
                // The porch: a deck of planks, papyrus-bundle columns with bell capitals, the architrave, the starry ceiling.
                float deck = CitySolids.DeckHeight;
                b.BoxMinMax(new Vector3(-hw, 0f, zFront), new Vector3(hw, deck, zFacade), TownSwatch.WoodDark, TownSwatch.MesektetMat);
                float z = zFront + TownBuildingBuilder.PostInset;
                float archY = roofY - 0.22f;
                foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
                {
                    BundleColumn(b, new Vector3(x, deck, z), archY - deck, 0.15f);
                }

                b.Box(new Vector3(0f, archY + 0.05f, z), new Vector3(hw * 2f, 0.2f, 0.24f), spec.TrimColor, 0f, spec.AccentColor);
                for (int i = 0; i < 5; i++)
                {
                    float sx = Mathf.Lerp(-hw + 0.5f, hw - 0.5f, i / 4f);
                    float sz = Mathf.Lerp(zFront + 0.3f, zFacade - 0.1f, (i % 2) * 0.6f + 0.2f);
                    b.Box(new Vector3(sx, roofY - 0.02f, sz), new Vector3(0.12f, 0.02f, 0.12f), TownSwatch.StarGlow, 45f);
                }

                float lampX = Mathf.Clamp(doorX + (doorX >= 0f ? -1f : 1f) * (door + 0.9f), -hw + 0.4f, hw - 0.4f);
                OilLampStand(b, new Vector3(lampX, deck, (zFront + zFacade) * 0.5f), 0.8f);

                if (spec.Sign)
                {
                    var at = new Vector3(doorX, archY - 0.25f, z - 0.14f);
                    b.Box(at, new Vector3(1.1f, 0.3f, 0.05f), spec.AccentColor, 0f);
                    b.Box(at + new Vector3(0f, 0f, -0.03f), new Vector3(0.94f, 0.2f, 0.02f), spec.SignColor, 0f);
                }
            }
        }

        /// <summary>A column of bound papyrus stems from <paramref name="foot"/>, <paramref name="height"/> tall: a round base, the bundled shaft tied with gold bands and a bell capital.</summary>
        private static void BundleColumn(TownMeshBuilder b, Vector3 foot, float height, float r)
        {
            b.Cylinder(foot, r * 1.5f, r * 1.4f, 0.12f, 6, TownSwatch.QuaySandstone, TownSwatch.QuaySandstone);
            float capital = Mathf.Min(0.45f, height * 0.18f);
            b.Cylinder(foot + Vector3.up * 0.12f, r * 1.1f, r * 0.9f, height - 0.12f - capital, 6, TownSwatch.PapyrusGreen);
            foreach (float t in new[] { 0.3f, 0.55f })
            {
                b.Cylinder(foot + Vector3.up * (0.12f + (height - capital) * t), r * 1.05f, r * 1.05f, 0.06f, 6, TownSwatch.KemetGold);
            }

            b.Cylinder(foot + Vector3.up * (height - capital), r * 0.9f, r * 2.1f, capital, 6, TownSwatch.PapyrusGreen, TownSwatch.KemetGold);
        }

        /// <summary>A standing oil lamp of <paramref name="s"/> on <paramref name="foot"/>: a reed stand, a bronze bowl and its flame of starlight.</summary>
        private static void OilLampStand(TownMeshBuilder b, Vector3 foot, float s)
        {
            b.Cylinder(foot, 0.18f * s, 0.12f * s, 0.1f * s, 5, TownSwatch.QuaySandstone, TownSwatch.QuaySandstone);
            b.Cylinder(foot + Vector3.up * 0.1f * s, 0.05f * s, 0.05f * s, 0.8f * s, 5, TownSwatch.WoodDark);
            b.Cylinder(foot + Vector3.up * 0.9f * s, 0.08f * s, 0.2f * s, 0.12f * s, 6, TownSwatch.KemetGold, TownSwatch.KemetGold);
            b.Blob(foot + Vector3.up * 1.1f * s, new Vector3(0.14f, 0.2f, 0.14f) * s, TownSwatch.StarGlow);
        }

        // ------------------------------------------------------------------
        // The gate
        // ------------------------------------------------------------------

        /// <summary>
        /// The gate of the Cais de Mesektet at a street's end: two tall papyrus-bundle columns on sandstone drums, a lintel of
        /// sandstone with a gold band of painted stars and a cavetto cornice, an oil lamp hanging in the middle (the gateways of
        /// the river towns as form, not a temple pylon).
        /// </summary>
        public static void PapyrusGate(TownMeshBuilder b, Vector3 a, Vector3 c)
        {
            Vector3 span = c - a;
            float w = new Vector2(span.x, span.z).magnitude;
            if (w < 0.5f)
            {
                return;
            }

            b.With(a, YawAlong(new Vector2(span.x, span.z) / w), () =>
            {
                const float colTop = 3.5f;
                foreach (float x in new[] { 0f, w })
                {
                    b.Cylinder(new Vector3(x, 0f, 0f), 0.55f, 0.5f, 0.35f, 6, TownSwatch.QuaySandstone, TownSwatch.QuaySandstone);
                    BundleColumn(b, new Vector3(x, 0.35f, 0f), colTop - 0.35f, 0.22f);
                }

                b.Box(new Vector3(w * 0.5f, colTop + 0.25f, 0f), new Vector3(w + 1.2f, 0.5f, 0.6f), TownSwatch.QuaySandstone, 0f);
                b.Box(new Vector3(w * 0.5f, colTop + 0.25f, -0.31f), new Vector3(w + 0.8f, 0.16f, 0.02f), TownSwatch.KemetGold, 0f);
                for (int i = 0; i < 7; i++)
                {
                    float x = Mathf.Lerp(-0.2f, w + 0.2f, i / 6f);
                    b.Box(new Vector3(x, colTop + 0.25f, -0.33f), new Vector3(0.1f, 0.1f, 0.02f), TownSwatch.StarGlow, 45f);
                }

                b.Slab(new Vector3(-0.6f, colTop + 0.5f, -0.3f), new Vector3(-0.8f, colTop + 0.85f, -0.55f),
                    new Vector3(w + 0.8f, colTop + 0.85f, -0.55f), new Vector3(w + 0.6f, colTop + 0.5f, -0.3f), 0.08f, TownSwatch.KemetGold, TownSwatch.QuaySandstone);
                b.Box(new Vector3(w * 0.5f, colTop + 0.62f, 0.05f), new Vector3(w + 1.4f, 0.25f, 0.7f), TownSwatch.QuaySandstone, 0f, TownSwatch.MesektetMat);

                float mid = w * 0.5f;
                b.Log(new Vector3(mid, colTop, 0f), new Vector3(mid, colTop - 0.5f, 0f), 0.015f, TownSwatch.KemetGold, 3);
                b.Cylinder(new Vector3(mid, colTop - 0.75f, 0f), 0.12f, 0.24f, 0.2f, 6, TownSwatch.KemetGold, TownSwatch.KemetGold);
                b.Blob(new Vector3(mid, colTop - 0.48f, 0f), new Vector3(0.16f, 0.2f, 0.16f), TownSwatch.StarGlow);
            });
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>
        /// The Cais da Barca on the plaza: a round landing of sandstone with a gold rim and a ring of dark water inside, and the
        /// Mesektet moored across it: a long papyrus hull whose prow and stern curl up into umbels, a cabin of reed mats on
        /// gilded posts in the middle, the steering oar at the stern and two oil lamps of starlight at the bow.
        /// </summary>
        private static void NightBarque(TownMeshBuilder b, float s)
        {
            float r = NightBarqueRadius * s;
            b.Cylinder(Vector3.zero, r, r * 0.95f, 0.3f * s, 12, TownSwatch.QuaySandstone, TownSwatch.QuaySandstone);
            b.Ring(new Vector3(0f, 0.31f * s, 0f), r * 0.82f, r * 0.95f, 12, TownSwatch.KemetGold);
            b.Disc(new Vector3(0f, 0.3f * s, 0f), r * 0.82f, 12, TownSwatch.NightWater);

            // The papyrus hull, east–west, in three bundles; the prow and stern curl up.
            float half = r * 0.95f;
            float hullY = 0.55f * s;
            foreach (float z in new[] { -0.28f * s, 0f, 0.28f * s })
            {
                b.Log(new Vector3(-half * 0.7f, hullY, z * 0.9f), new Vector3(half * 0.7f, hullY, z * 0.9f), 0.2f * s, TownSwatch.MesektetMat, 6);
            }

            foreach (float sx in new[] { -1f, 1f })
            {
                var p0 = new Vector3(sx * half * 0.7f, hullY, 0f);
                var p1 = new Vector3(sx * half * 1.0f, hullY + 0.5f * s, 0f);
                var p2 = new Vector3(sx * half * 1.08f, hullY + 1.25f * s, 0f);
                b.Log(p0, p1, 0.24f * s, TownSwatch.MesektetMat, 6);
                b.Log(p1, p2, 0.16f * s, TownSwatch.MesektetMat, 6);
                b.Cylinder(p2, 0.1f * s, 0.32f * s, 0.25f * s, 6, TownSwatch.KemetGold, TownSwatch.PapyrusGreen);
                b.Cylinder(p0 + Vector3.up * 0.05f * s, 0.24f * s, 0.24f * s, 0.06f * s, 6, TownSwatch.KemetGold);
            }

            // The cabin of reed mats on four gilded posts.
            float cabinY = hullY + 0.2f * s;
            foreach (float px in new[] { -0.55f * s, 0.55f * s })
            {
                foreach (float pz in new[] { -0.3f * s, 0.3f * s })
                {
                    b.Box(new Vector3(px, cabinY + 0.6f * s, pz), new Vector3(0.08f, 1.2f, 0.08f) * s, TownSwatch.KemetGold, 0f);
                }
            }

            b.Box(new Vector3(0f, cabinY + 1.25f * s, 0f), new Vector3(1.35f, 0.1f, 0.8f) * s, TownSwatch.MesektetMat, 0f, TownSwatch.RiverNight);
            b.Box(new Vector3(0f, cabinY + 0.5f * s, 0.3f * s), new Vector3(1.1f, 0.9f, 0.04f) * s, TownSwatch.NileLinen, 0f);
            b.Box(new Vector3(0f, cabinY + 1.32f * s, 0f), new Vector3(0.14f, 0.04f, 0.14f) * s, TownSwatch.StarGlow, 45f);

            // The steering oar at the stern (west) and the two lamps at the bow (east).
            b.Log(new Vector3(-half * 0.8f, hullY + 1.1f * s, 0.2f * s), new Vector3(-half * 1.05f, 0.35f * s, 0.45f * s), 0.05f * s, TownSwatch.WoodDark, 4);
            b.Box(new Vector3(-half * 1.05f, 0.4f * s, 0.45f * s), new Vector3(0.1f, 0.35f, 0.3f) * s, TownSwatch.WoodDark, 0f);
            foreach (float z in new[] { -0.3f * s, 0.3f * s })
            {
                OilLampStand(b, new Vector3(half * 0.55f, hullY + 0.15f * s, z), 0.6f * s);
            }
        }

        /// <summary>
        /// A pier of the hours from the origin to <paramref name="d"/>: a channel of dark water <see cref="HourCanalHalfWidth"/>
        /// each side between sandstone lips, mooring posts with gold caps near the ends and, in the middle, a bridge of dark
        /// planks on reed bundles with an hour-lamp on each side. Walkable: the planks stay under the walker's step.
        /// </summary>
        private static void HourCanal(TownMeshBuilder b, Vector2 d)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            float w = HourCanalHalfWidth;
            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                b.Flat(0f, -w, length, w, 0.02f, TownSwatch.NightWater);
                foreach (float z in new[] { -w - 0.12f, w + 0.12f })
                {
                    b.Box(new Vector3(length * 0.5f, 0.06f, z), new Vector3(length, 0.12f, 0.24f), TownSwatch.QuaySandstone, 0f, TownSwatch.KemetGold);
                }

                // Reflections of the stars on the water.
                int glints = Mathf.Max(2, Mathf.RoundToInt(length / 1.5f));
                for (int i = 0; i < glints; i++)
                {
                    float x = length * (i + 0.5f) / glints;
                    if (Mathf.Abs(x - length * 0.5f) < 1.3f)
                    {
                        continue;
                    }

                    b.Box(new Vector3(x, 0.03f, (i % 2 == 0 ? -0.4f : 0.35f) * w), new Vector3(0.1f, 0.01f, 0.1f), TownSwatch.StarGlow, 45f);
                }

                // The plank bridge on bundles of reed.
                float xm = length * 0.5f;
                foreach (float x in new[] { xm - 0.85f, xm + 0.85f })
                {
                    b.Log(new Vector3(x, 0.05f, -w - 0.2f), new Vector3(x, 0.05f, w + 0.2f), 0.1f, TownSwatch.MesektetMat, 5);
                }

                for (int i = -2; i <= 2; i++)
                {
                    b.Box(new Vector3(xm + i * 0.4f, 0.15f, 0f), new Vector3(0.36f, 0.06f, w * 2f + 0.55f), TownSwatch.WoodDark, 0f, i == 0 ? TownSwatch.KemetGold : TownSwatch.WoodDark);
                }

                foreach (float z in new[] { -w - 0.3f, w + 0.3f })
                {
                    OilLampStand(b, new Vector3(xm, 0.1f, z), 0.7f);
                }

                // The mooring posts near each end.
                foreach (float x in new[] { Mathf.Min(0.5f, length * 0.12f), Mathf.Max(length - 0.5f, length * 0.88f) })
                {
                    foreach (float z in new[] { -w - 0.12f, w + 0.12f })
                    {
                        b.Cylinder(new Vector3(x, 0f, z), 0.1f, 0.09f, 0.75f, 5, TownSwatch.WoodDark, TownSwatch.KemetGold);
                    }
                }
            });
        }

        /// <summary>
        /// A freestanding papyrus column on a sandstone drum with a bell capital painted green and gold and an oil lamp of
        /// starlight on top (Variant 1: taller, with a band of painted stars on the drum).
        /// </summary>
        private static void PapyrusColumn(TownMeshBuilder b, float s, int variant)
        {
            bool tall = (variant % 2) == 1;
            float r = PapyrusColumnRadius * s;
            float h = (tall ? 3.2f : 2.4f) * s;
            b.Cylinder(Vector3.zero, r, r * 0.92f, 0.4f * s, 8, TownSwatch.QuaySandstone, TownSwatch.QuaySandstone);
            if (tall)
            {
                b.Cylinder(new Vector3(0f, 0.18f * s, 0f), r * 0.97f, r * 0.97f, 0.1f * s, 8, TownSwatch.RiverNight);
                for (int i = 0; i < 6; i++)
                {
                    float a = i * 60f * Mathf.Deg2Rad;
                    b.Box(new Vector3(Mathf.Cos(a) * r * 0.98f, 0.23f * s, Mathf.Sin(a) * r * 0.98f), new Vector3(0.06f, 0.06f, 0.06f) * s, TownSwatch.StarGlow, 45f);
                }
            }

            BundleColumn(b, new Vector3(0f, 0.4f * s, 0f), h, 0.2f * s);
            b.Box(new Vector3(0f, 0.4f * s + h + 0.05f * s, 0f), new Vector3(0.75f, 0.1f, 0.75f) * s, TownSwatch.QuaySandstone, 0f);
            OilLampStand(b, new Vector3(0f, 0.5f * s + h, 0f), 0.4f * s);
        }

        /// <summary>
        /// A thicket of papyrus in a bed of dark mud: tall stems leaning out with umbel heads (Variant 1: larger, with a pool of
        /// lotus pads at its foot).
        /// </summary>
        private static void PapyrusThicket(TownMeshBuilder b, float s, int variant)
        {
            bool large = (variant % 2) == 1;
            float r = PapyrusThicketRadius * s;
            b.Crown(new Vector3(0f, 0.08f * s, 0f), new Vector3(r * 2f, 0.2f * s, r * 1.8f), TownSwatch.DirtDark, 15f);
            int stems = large ? 11 : 7;
            for (int i = 0; i < stems; i++)
            {
                float a = (i * 137.5f) * Mathf.Deg2Rad;
                float d = r * (0.2f + 0.6f * ((i * 7) % stems) / stems);
                var foot = new Vector3(Mathf.Cos(a) * d, 0.1f * s, Mathf.Sin(a) * d);
                float h = (large ? 2.6f : 2.0f) * s * (0.8f + 0.25f * ((i * 3) % 4) / 3f);
                var tip = foot + new Vector3(Mathf.Cos(a) * 0.35f * s, h, Mathf.Sin(a) * 0.35f * s);
                b.Log(foot, tip, 0.035f * s, TownSwatch.PapyrusGreen, 3);
                b.Cylinder(tip - Vector3.up * 0.05f * s, 0.03f * s, 0.28f * s, 0.22f * s, 5, TownSwatch.PapyrusGreen, TownSwatch.LeafDark);
            }

            if (large)
            {
                b.Disc(new Vector3(r * 0.9f, 0.04f * s, -r * 0.7f), 0.55f * s, 8, TownSwatch.NightWater);
                foreach (Vector3 pad in new[] { new Vector3(0.75f, 0.05f, -0.6f), new Vector3(1.05f, 0.05f, -0.85f) })
                {
                    b.Disc(new Vector3(pad.x * r, pad.y * s, pad.z * r), 0.16f * s, 6, TownSwatch.PapyrusGreen);
                }

                b.Blob(new Vector3(0.95f * r, 0.12f * s, -0.7f * r), new Vector3(0.1f, 0.1f, 0.1f) * s, TownSwatch.NileLinen);
            }
        }

        /// <summary>
        /// A skiff of bound papyrus drawn up on the bank: two bundles tied with gold cords, prow and stern curled up, a paddle
        /// across it (Variant 1: up on two trestles with a net hung to dry beside it).
        /// </summary>
        private static void ReedSkiff(TownMeshBuilder b, float s, int variant)
        {
            bool trestles = (variant % 2) == 1;
            float y = (trestles ? 0.7f : 0.2f) * s;
            float half = ReedSkiffRadius * s;
            if (trestles)
            {
                foreach (float x in new[] { -half * 0.5f, half * 0.5f })
                {
                    b.Log(new Vector3(x, 0f, -0.3f * s), new Vector3(x, y - 0.1f * s, 0f), 0.04f * s, TownSwatch.WoodDark, 4);
                    b.Log(new Vector3(x, 0f, 0.3f * s), new Vector3(x, y - 0.1f * s, 0f), 0.04f * s, TownSwatch.WoodDark, 4);
                }

                b.Log(new Vector3(-half * 0.3f, 0f, 0.65f * s), new Vector3(-half * 0.3f, 1.3f * s, 0.65f * s), 0.03f * s, TownSwatch.WoodDark, 4);
                b.Log(new Vector3(half * 0.3f, 0f, 0.65f * s), new Vector3(half * 0.3f, 1.3f * s, 0.65f * s), 0.03f * s, TownSwatch.WoodDark, 4);
                b.Box(new Vector3(0f, 0.95f * s, 0.65f * s), new Vector3(half * 0.6f, 0.65f * s, 0.02f), TownSwatch.NetTwine, 0f);
            }

            foreach (float z in new[] { -0.13f * s, 0.13f * s })
            {
                b.Log(new Vector3(-half * 0.65f, y, z), new Vector3(half * 0.65f, y, z), 0.13f * s, TownSwatch.MesektetMat, 5);
            }

            foreach (float sx in new[] { -1f, 1f })
            {
                b.Log(new Vector3(sx * half * 0.65f, y, 0f), new Vector3(sx * half, y + 0.45f * s, 0f), 0.14f * s, TownSwatch.MesektetMat, 5);
                b.Cylinder(new Vector3(sx * half * 0.4f, y - 0.14f * s, 0f), 0.2f * s, 0.2f * s, 0.05f * s, 6, TownSwatch.KemetGold);
            }

            b.Log(new Vector3(-half * 0.4f, y + 0.16f * s, -0.35f * s), new Vector3(half * 0.4f, y + 0.16f * s, 0.3f * s), 0.03f * s, TownSwatch.WoodDark, 4);
            b.Box(new Vector3(half * 0.45f, y + 0.16f * s, 0.34f * s), new Vector3(0.25f, 0.03f, 0.12f) * s, TownSwatch.WoodDark, 40f);
        }

        // ------------------------------------------------------------------
        // Dressing
        // ------------------------------------------------------------------

        /// <summary>Khunanup's front: a reed mat with baskets of grain, two tall jars, a balance scale, rolled papyri, an oar leaning on the wall and an oil lamp.</summary>
        private static void PapyrusQuayDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            b.Box(new Vector3(x, y + 0.02f, z - 0.45f), new Vector3(2.4f, 0.04f, 0.9f), TownSwatch.MesektetMat, 0f);
            foreach (float dx in new[] { -0.85f, -0.35f })
            {
                b.Cylinder(new Vector3(x + dx, y + 0.04f, z - 0.45f), 0.2f, 0.24f, 0.3f, 6, TownSwatch.MesektetMat, TownSwatch.GoldGrass);
            }

            foreach (float dx in new[] { 0.25f, 0.6f })
            {
                b.Cylinder(new Vector3(x + dx, y + 0.04f, z - 0.3f), 0.12f, 0.2f, 0.4f, 6, TownSwatch.QuaySandstone);
                b.Cylinder(new Vector3(x + dx, y + 0.44f, z - 0.3f), 0.2f, 0.08f, 0.3f, 6, TownSwatch.QuaySandstone, TownSwatch.RiverNight);
            }

            // The balance scale.
            var scale = new Vector3(x + 1.0f, y + 0.04f, z - 0.55f);
            b.Log(scale, scale + Vector3.up * 0.7f, 0.025f, TownSwatch.KemetGold, 4);
            b.Log(scale + new Vector3(-0.3f, 0.7f, 0f), scale + new Vector3(0.3f, 0.7f, 0f), 0.02f, TownSwatch.KemetGold, 4);
            foreach (float sx in new[] { -0.3f, 0.3f })
            {
                b.Cylinder(scale + new Vector3(sx, 0.42f, 0f), 0.1f, 0.12f, 0.04f, 6, TownSwatch.KemetGold, TownSwatch.KemetGold);
            }

            // The rolled papyri on the mat, the oar on the wall and the lamp.
            for (int i = 0; i < 3; i++)
            {
                b.Log(new Vector3(x - 0.3f + i * 0.12f, y + 0.08f, z - 0.75f), new Vector3(x - 0.3f + i * 0.12f, y + 0.08f, z - 0.45f), 0.04f, TownSwatch.NileLinen, 5);
            }

            b.Log(new Vector3(x - 1.5f, y, zFacade - 0.1f), new Vector3(x - 1.35f, y + 1.8f, zFacade - 0.1f), 0.03f, TownSwatch.WoodDark, 4);
            b.Box(new Vector3(x - 1.48f, y + 0.25f, zFacade - 0.1f), new Vector3(0.18f, 0.45f, 0.03f), TownSwatch.WoodDark, 0f);
            b.Blob(new Vector3(x + 1.5f, y + 1.1f, zFacade - 0.08f), new Vector3(0.16f, 0.2f, 0.16f), TownSwatch.StarGlow);
        }
    }
}
