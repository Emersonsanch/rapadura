using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of the Cais da Coluna, the quay in the sky where the Barcas de Luz moor (Docs/Mapas100.md map 91, região 16, style
    /// <c>SkyQuay</c>): pavilions of glass and gold on cloud (<see cref="SkyPavilionHouse"/>), the five fountains of the light elements, the
    /// foot of the Coluna da Glória, the Barca do Sol and the Barca da Lua alongside their piers of solid light with the Remadores on deck (silhouettes of light, they do not
    /// speak), banks of cloud and the gaps through which the 27 cities of the island show as little lights. Flat colours from
    /// <see cref="TownPalette"/> (rows 26–27), low-poly, deterministic. The Pai da Grandeza never appears (bible §0.2): no figure, no throne.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Radius of an <see cref="CityPropKind.ElementFountain"/> (× its scale): its collider.</summary>
        public const float ElementFountainRadius = 0.9f;

        /// <summary>Radius of the <see cref="CityPropKind.ColumnOfGlory"/> at its foot (× its scale): its collider.</summary>
        public const float ColumnOfGloryRadius = 1.6f;

        /// <summary>Height of the shaft of light of the Coluna da Glória.</summary>
        public const float ColumnOfGloryHeight = 30f;

        /// <summary>Half the length of a <see cref="CityPropKind.SkyBarge"/> (× its scale), along world x.</summary>
        public const float SkyBargeHalfLength = 5f;

        /// <summary>Half the beam of a <see cref="CityPropKind.SkyBarge"/> (× its scale): the radius of each collider along its keel.</summary>
        public const float SkyBargeHalfBeam = 1.8f;

        /// <summary>Distance between two colliders along the keel (shorter than the beam, so the hull has no holes).</summary>
        public const float SkyBargeColliderStep = 1.6f;

        /// <summary>Height of the deck of a barge: what stands on it is raised by this.</summary>
        public const float SkyBargeDeckHeight = 0.62f;

        /// <summary>Length of a <see cref="CityPropKind.LightPier"/> (× its scale), centred on the prop, along world x.</summary>
        public const float LightPierLength = 10f;

        /// <summary>The cities of the island seen from the sky (Docs/Mapas100.md map 91), shared between the two gaps in the cloud.</summary>
        public const int IslandCities = 27;

        /// <summary>How many little lights one <see cref="CityPropKind.IslandLightsGap"/> shows (Variant 0 and 1 add up to <see cref="IslandCities"/>).</summary>
        public static int IslandLightsIn(int variant) => (variant % 2) == 0 ? IslandCities / 2 + 1 : IslandCities / 2;

        /// <summary>The centres of the capsule colliders along the keel of a barge at <paramref name="center"/>.</summary>
        public static IEnumerable<Vector2> SkyBargeColliders(Vector2 center, float scale)
        {
            float reach = (SkyBargeHalfLength - SkyBargeHalfBeam) * scale;
            int count = Mathf.CeilToInt(reach * 2f / (SkyBargeColliderStep * scale));
            for (int i = 0; i <= count; i++)
            {
                yield return center + new Vector2(Mathf.Lerp(-reach, reach, (float)i / count), 0f);
            }
        }

        /// <summary>The colour of a light element: 0 Ar Sereno (lilac), 1 Vento Vivo (bluish white), 2 Luz (pale gold), 3 Água Viva (aqua), 4 Fogo Vivo (orange).</summary>
        public static TownSwatch LightElementSwatch(int variant)
        {
            switch ((variant % 5 + 5) % 5)
            {
                case 0: return TownSwatch.ElementAir;
                case 1: return TownSwatch.ElementWind;
                case 2: return TownSwatch.ElementLight;
                case 3: return TownSwatch.ElementWater;
                default: return TownSwatch.ElementFire;
            }
        }

        // ------------------------------------------------------------------
        // The house
        // ------------------------------------------------------------------

        /// <summary>
        /// A <see cref="TownBuildingSpec.SkyPavilion"/> (local frame, front −z): a white plinth of firm cloud, walls of glass panes
        /// (<see cref="TownBuildingSpec.WallColor"/>) between gold mullions, a gold door under a lintel of solid light, a low lilac dome with a
        /// gold ring and a little flame on top; in front the porch: a deck of cloud, gold posts (at <see cref="TownBuildingBuilder.PorchPostsX"/>, the
        /// colliders use the same list), a lilac awning and the plaque.
        /// </summary>
        public static void SkyPavilionHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            float eave = Mathf.Max(top, 2.6f);
            float wx = hw - 0.15f;
            float zw = zFacade + 0.1f, zb = zBack - 0.1f;

            b.BoxMinMax(new Vector3(-hw, 0f, zw - 0.08f), new Vector3(hw, 0.32f, zBack), spec.PlinthColor, spec.PlinthColor);
            b.BoxMinMax(new Vector3(-wx, 0.32f, zw), new Vector3(wx, eave, zb), spec.WallColor, spec.WallColor);

            float doorX = Mathf.Clamp(spec.DoorOffset * spec.Width, -wx + 0.8f, wx - 0.8f);
            float door = spec.WideDoor ? 0.75f : 0.55f;
            const float doorTop = 2.15f;

            // The gold mullions over the glass, leaving the door clear; a band of gold under the eave and over the plinth.
            int bays = Mathf.Clamp(Mathf.RoundToInt(wx * 2f / 0.9f), 4, 9);
            for (int i = 0; i < bays; i++)
            {
                float x = Mathf.Lerp(-wx + 0.05f, wx - 0.05f, (float)i / (bays - 1));
                if (Mathf.Abs(x - doorX) > door + 0.1f)
                {
                    b.Box(new Vector3(x, (0.32f + eave) * 0.5f, zw - 0.03f), new Vector3(0.08f, eave - 0.32f, 0.06f), spec.TrimColor, 0f);
                }
            }

            b.Box(new Vector3(0f, eave - 0.14f, zw - 0.04f), new Vector3(wx * 2f + 0.06f, 0.14f, 0.08f), spec.TrimColor, 0f);
            b.Box(new Vector3(0f, 0.42f, zw - 0.04f), new Vector3(wx * 2f + 0.06f, 0.08f, 0.08f), spec.TrimColor, 0f);

            foreach (float side in new[] { -1f, 1f })
            {
                for (int i = 0; i < 3; i++)
                {
                    float z = Mathf.Lerp(zw + 0.05f, zb - 0.05f, i / 2f);
                    b.Box(new Vector3(side * (wx + 0.01f), (0.32f + eave) * 0.5f, z), new Vector3(0.06f, eave - 0.32f, 0.08f), spec.TrimColor, 0f);
                }
            }

            b.BoxMinMax(new Vector3(doorX - door - 0.1f, 0.3f, zw - 0.14f), new Vector3(doorX + door + 0.1f, doorTop + 0.1f, zw - 0.02f), spec.JoineryColor);
            b.Box(new Vector3(doorX, (0.3f + doorTop) * 0.5f, zw - 0.16f), new Vector3(door * 2f - 0.1f, doorTop - 0.35f, 0.03f), TownSwatch.QuayLilac, 0f);
            b.Box(new Vector3(doorX, doorTop + 0.18f, zw - 0.17f), new Vector3(door * 2f + 0.5f, 0.2f, 0.08f), spec.AccentColor, 0f);

            // The flat roof of cloud and the low dome.
            b.BoxMinMax(new Vector3(-hw, eave, zFacade), new Vector3(hw, eave + 0.14f, zBack), spec.PlinthColor, spec.PlinthColor);
            float cz = (zFacade + zBack) * 0.5f;
            float r = Mathf.Min(hw, (zBack - zFacade) * 0.5f) * 0.82f;
            var dome = new Vector3(0f, eave + 0.14f, cz);
            b.Cylinder(dome, r, r * 0.98f, 0.1f, 12, TownSwatch.QuayGold, TownSwatch.QuayGold);
            b.Cylinder(dome + Vector3.up * 0.1f, r * 0.94f, r * 0.72f, 0.42f, 12, spec.RoofColor, spec.RoofColor);
            b.Cylinder(dome + Vector3.up * 0.52f, r * 0.72f, r * 0.3f, 0.4f, 12, spec.RoofColor, spec.RoofColor);
            b.Cylinder(dome + Vector3.up * 0.92f, r * 0.3f, 0.06f, 0.24f, 8, spec.RoofColor, TownSwatch.QuayGold);
            b.Blob(dome + Vector3.up * 1.3f, new Vector3(0.14f, 0.24f, 0.14f), TownSwatch.SolidLight);

            if (spec.PorchDepth > 0.01f)
            {
                float deck = CitySolids.DeckHeight;
                b.BoxMinMax(new Vector3(-hw, 0f, zFront), new Vector3(hw, deck, zFacade), spec.PlinthColor, TownSwatch.QuayCloudShade);
                float z = zFront + TownBuildingBuilder.PostInset;
                float beamY = Mathf.Min(eave, 2.75f);
                foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
                {
                    b.Box(new Vector3(x, deck + (beamY - deck) * 0.5f, z), new Vector3(0.18f, beamY - deck, 0.18f), TownSwatch.QuayGold, 0f);
                }

                b.Box(new Vector3(0f, beamY + 0.05f, z), new Vector3(hw * 2f, 0.12f, 0.18f), TownSwatch.QuayGold, 0f);
                b.Slab(new Vector3(-hw, beamY + 0.08f, zFront - 0.1f), new Vector3(-hw, beamY + 0.25f, zFacade),
                    new Vector3(hw, beamY + 0.25f, zFacade), new Vector3(hw, beamY + 0.08f, zFront - 0.1f), 0.06f, TownSwatch.QuayLilac, spec.TrimColor);

                if (spec.Sign)
                {
                    var at = new Vector3(doorX, beamY - 0.2f, z - 0.12f);
                    b.Box(at, new Vector3(1.1f, 0.3f, 0.05f), TownSwatch.QuayGold, 0f);
                    b.Box(at + new Vector3(0f, 0f, -0.03f), new Vector3(0.94f, 0.2f, 0.02f), spec.SignColor, 0f);
                }
            }
        }

        // ------------------------------------------------------------------
        // The gate
        // ------------------------------------------------------------------

        /// <summary>The gate of the Cais at a street's end: two slim gold pylons on bases of cloud, a lilac arch-beam and a globe of solid light hanging in the middle.</summary>
        public static void SkyQuayGate(TownMeshBuilder b, Vector3 a, Vector3 c)
        {
            Vector3 span = c - a;
            float w = new Vector2(span.x, span.z).magnitude;
            if (w < 0.5f)
            {
                return;
            }

            b.With(a, YawAlong(new Vector2(span.x, span.z) / w), () =>
            {
                const float pylon = 4.1f;
                foreach (float x in new[] { 0f, w })
                {
                    b.Cylinder(new Vector3(x, 0f, 0f), 0.6f, 0.5f, 0.3f, 8, TownSwatch.QuayCloud, TownSwatch.QuayCloudShade);
                    b.Cylinder(new Vector3(x, 0.3f, 0f), 0.26f, 0.2f, pylon - 0.3f, 8, TownSwatch.QuayGold, TownSwatch.QuayGold);
                    b.Blob(new Vector3(x, pylon + 0.12f, 0f), new Vector3(0.26f, 0.3f, 0.26f), TownSwatch.SolidLight);
                }

                b.Box(new Vector3(w * 0.5f, pylon - 0.5f, 0f), new Vector3(w, 0.32f, 0.3f), TownSwatch.QuayLilac, 0f, TownSwatch.QuayGold);
                b.Box(new Vector3(w * 0.5f, pylon - 0.26f, 0f), new Vector3(w * 0.7f, 0.16f, 0.22f), TownSwatch.QuayGold, 0f);
                b.Log(new Vector3(w * 0.5f, pylon - 0.66f, 0f), new Vector3(w * 0.5f, pylon - 1.05f, 0f), 0.02f, TownSwatch.QuayGold, 3);
                b.Blob(new Vector3(w * 0.5f, pylon - 1.25f, 0f), new Vector3(0.22f, 0.26f, 0.22f), TownSwatch.SolidLight);
            });
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>One of the five fountains of the plaza (<paramref name="variant"/> 0–4: Ar Sereno, Vento Vivo, Luz, Água Viva, Fogo Vivo): a round basin of cloud with a gold rim and a jet of the element's colour.</summary>
        private static void ElementFountain(TownMeshBuilder b, float s, int variant)
        {
            TownSwatch element = LightElementSwatch(variant);
            float r = ElementFountainRadius * s;
            b.Cylinder(Vector3.zero, r, r * 0.96f, 0.46f * s, 12, TownSwatch.QuayCloud, TownSwatch.QuayCloudShade);
            b.Cylinder(new Vector3(0f, 0.46f * s, 0f), r * 0.98f, r * 0.9f, 0.06f * s, 12, TownSwatch.QuayGold, element);
            b.Cylinder(new Vector3(0f, 0.52f * s, 0f), 0.26f * s, 0.18f * s, 0.62f * s, 8, TownSwatch.QuayGold, TownSwatch.QuayGold);
            b.Cylinder(new Vector3(0f, 1.14f * s, 0f), 0.44f * s, 0.5f * s, 0.1f * s, 8, TownSwatch.QuayGold, element);
            b.Blob(new Vector3(0f, 1.55f * s, 0f), new Vector3(0.22f, 0.4f, 0.22f) * s, element);
            for (int i = 0; i < 5; i++)
            {
                float a = i * Mathf.PI * 0.4f;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.6f * s, 0.66f * s, Mathf.Sin(a) * 0.6f * s), new Vector3(0.1f, 0.16f, 0.1f) * s, element);
            }
        }

        /// <summary>The foot of the Coluna da Glória: three steps of firm cloud, five gold rings with the colours of the elements, and the shaft of light going up out of sight. A column of light, no figure.</summary>
        private static void ColumnOfGlory(TownMeshBuilder b, float s)
        {
            float r = ColumnOfGloryRadius * s;
            for (int i = 0; i < 3; i++)
            {
                float ring = r * (1f - i * 0.2f);
                b.Cylinder(new Vector3(0f, i * 0.3f * s, 0f), ring, ring * 0.97f, 0.3f * s, 14, TownSwatch.QuayCloud, TownSwatch.QuayCloudShade);
            }

            float y = 0.9f * s;
            for (int i = 0; i < 5; i++)
            {
                float ring = r * (0.62f - i * 0.03f);
                b.Cylinder(new Vector3(0f, y + i * 0.34f * s, 0f), ring, ring, 0.14f * s, 12, LightElementSwatch(i), TownSwatch.QuayGold);
            }

            b.Cylinder(new Vector3(0f, y, 0f), r * 0.5f, r * 0.44f, ColumnOfGloryHeight * s, 12, TownSwatch.ColumnLight, TownSwatch.ColumnLight);
        }

        /// <summary>
        /// A Barca de Luz moored along its pier (long axis on world x, the gangway on the +z side): Variant 0 the Barca do Sol, gold with a triangular white sail and the bow to
        /// +x; Variant 1 the Barca da Lua, silver-lilac with a crescent at the stern and the bow to −x. Its colliders are <see cref="SkyBargeColliders"/>; the Remadores are
        /// <see cref="CityPropKind.LightRower"/> props standing on its deck.
        /// </summary>
        private static void SkyBarge(TownMeshBuilder b, float s, int variant)
        {
            bool sun = (variant % 2) == 0;
            float bow = sun ? 1f : -1f;
            TownSwatch hull = sun ? TownSwatch.QuayGold : TownSwatch.QuayLilac;
            TownSwatch trim = sun ? TownSwatch.SolidLight : TownSwatch.QuaySkyBlue;
            float hl = SkyBargeHalfLength * s;
            float hb = SkyBargeHalfBeam * s;
            float deck = SkyBargeDeckHeight * s;

            b.Box(new Vector3(0f, deck * 0.5f, 0f), new Vector3(hl * 1.4f, deck, hb * 1.8f), hull, 0f, TownSwatch.QuayCloud);
            b.Box(new Vector3(0f, deck + 0.12f * s, hb * 0.94f), new Vector3(hl * 1.4f, 0.1f * s, 0.08f * s), trim, 0f);
            b.Box(new Vector3(0f, deck + 0.12f * s, -hb * 0.94f), new Vector3(hl * 1.4f, 0.1f * s, 0.08f * s), trim, 0f);
            foreach (float end in new[] { -1f, 1f })
            {
                float lift = end == bow ? 1.5f : 1.1f;
                float x0 = end * hl * 0.7f;
                float x1 = end * hl;
                b.Quad(new Vector3(x0, 0f, -hb * 0.9f), new Vector3(x0, deck * lift, -hb * 0.9f), new Vector3(x1, deck * lift * 1.5f, 0f), new Vector3(x1, deck * 0.3f, 0f), hull);
                b.Quad(new Vector3(x1, deck * 0.3f, 0f), new Vector3(x1, deck * lift * 1.5f, 0f), new Vector3(x0, deck * lift, hb * 0.9f), new Vector3(x0, 0f, hb * 0.9f), hull);
                b.Quad(new Vector3(x0, deck * lift, -hb * 0.9f), new Vector3(x0, deck * lift, hb * 0.9f), new Vector3(x1, deck * lift * 1.5f, 0f), new Vector3(x1, deck * lift * 1.5f, 0f), TownSwatch.QuayCloud);
            }

            // The mast and the sail (the Sun's triangular dhow sail, Docs/Mapas100.md 97) or the crescent of the Moon.
            float mast = 5.2f * s;
            b.Cylinder(new Vector3(0f, deck, 0f), 0.09f * s, 0.06f * s, mast, 6, TownSwatch.QuayGold, TownSwatch.SolidLight);
            if (sun)
            {
                b.Quad(new Vector3(-2.6f * s, deck + 1.2f * s, 0.05f * s), new Vector3(0.1f * s, deck + mast - 0.1f * s, 0.05f * s), new Vector3(2.8f * s, deck + 1.2f * s, 0.05f * s), new Vector3(2.8f * s, deck + 1.2f * s, 0.05f * s), TownSwatch.QuayCloud);
                b.Quad(new Vector3(2.8f * s, deck + 1.2f * s, -0.05f * s), new Vector3(2.8f * s, deck + 1.2f * s, -0.05f * s), new Vector3(0.1f * s, deck + mast - 0.1f * s, -0.05f * s), new Vector3(-2.6f * s, deck + 1.2f * s, -0.05f * s), TownSwatch.QuayCloud);
                b.Log(new Vector3(-2.7f * s, deck + 1.15f * s, 0f), new Vector3(2.9f * s, deck + 1.15f * s, 0f), 0.05f * s, TownSwatch.QuayGold, 4);
            }
            else
            {
                float baseY = deck + mast;
                for (int i = 0; i < 7; i++)
                {
                    float a0 = Mathf.Lerp(40f, 320f, i / 7f) * Mathf.Deg2Rad;
                    float a1 = Mathf.Lerp(40f, 320f, (i + 1) / 7f) * Mathf.Deg2Rad;
                    var p0 = new Vector3(Mathf.Cos(a0) * 0.8f * s, baseY + Mathf.Sin(a0) * 0.8f * s, 0f);
                    var p1 = new Vector3(Mathf.Cos(a1) * 0.8f * s, baseY + Mathf.Sin(a1) * 0.8f * s, 0f);
                    b.Log(p0, p1, 0.1f * s, TownSwatch.SolidLight, 4);
                }
            }

            // The gangway to the pier on the +z side.
            b.BoxMinMax(new Vector3(-0.6f * s, 0.04f * s, hb * 0.9f), new Vector3(0.6f * s, 0.1f * s, hb + 1.4f * s), TownSwatch.SolidLight, TownSwatch.SolidLight);
            foreach (float x in new[] { -0.6f * s, 0.6f * s })
            {
                b.Log(new Vector3(x, deck, hb * 0.9f), new Vector3(x, 0.45f * s, hb + 1.4f * s), 0.03f * s, TownSwatch.QuayGold, 3);
            }
        }

        /// <summary>A pier of solid light laid over a street along world x: inlaid bars of light across the road and low studs along both curbs, ankle high (no collider, it is the road).</summary>
        private static void LightPier(TownMeshBuilder b, float s)
        {
            float half = LightPierLength * 0.5f * s;
            const int bars = 11;
            for (int i = 0; i < bars; i++)
            {
                float x = Mathf.Lerp(-half, half, i / (bars - 1f));
                b.Box(new Vector3(x, 0.04f, 0f), new Vector3(0.26f, 0.03f, 3.2f), TownSwatch.ColumnLight, 0f);
                foreach (float z in new[] { -1.82f, 1.82f })
                {
                    b.Cylinder(new Vector3(x, 0f, z), 0.08f, 0.06f, 0.16f, 6, TownSwatch.QuayGold, TownSwatch.SolidLight);
                }
            }
        }

        /// <summary>A soft bank of cloud, knee high (Variant 1: a bigger one): no collider, the cousins wade through it.</summary>
        private static void CloudPuff(TownMeshBuilder b, float s, int variant)
        {
            bool big = (variant % 2) == 1;
            float w = (big ? 1.6f : 1.1f) * s;
            b.Blob(new Vector3(0f, 0.3f * s, 0f), new Vector3(w, 0.5f * s, w * 0.8f), TownSwatch.QuayCloud);
            b.Blob(new Vector3(w * 0.8f, 0.22f * s, w * 0.2f), new Vector3(w * 0.6f, 0.36f * s, w * 0.5f), TownSwatch.QuayCloud);
            b.Blob(new Vector3(-w * 0.75f, 0.2f * s, -w * 0.15f), new Vector3(w * 0.55f, 0.32f * s, w * 0.5f), TownSwatch.QuayCloudShade);
        }

        /// <summary>
        /// A gap in the cloud (flat on the ground, no collider): the deep blue far below and on it the little lights of the cities of the island
        /// (<see cref="IslandLightsIn"/>; together the two gaps show the 27). A rim of cloud closes it.
        /// </summary>
        private static void IslandLightsGap(TownMeshBuilder b, float s, int variant)
        {
            float r = 3.4f * s;
            const int sides = 14;
            var center = new Vector3(0f, 0.02f, 0f);
            for (int i = 0; i < sides; i++)
            {
                float a0 = i * Mathf.PI * 2f / sides;
                float a1 = (i + 1) * Mathf.PI * 2f / sides;
                b.Triangle(center, new Vector3(Mathf.Cos(a1) * r, 0.02f, Mathf.Sin(a1) * r), new Vector3(Mathf.Cos(a0) * r, 0.02f, Mathf.Sin(a0) * r), TownSwatch.SkyBelow);
                b.Blob(new Vector3(Mathf.Cos(a0) * r, 0.1f, Mathf.Sin(a0) * r), new Vector3(0.55f, 0.2f, 0.55f) * s, TownSwatch.QuayCloud);
            }

            int lights = IslandLightsIn(variant);
            for (int i = 0; i < lights; i++)
            {
                // A deterministic scatter inside the disc (golden angle); the first light of each gap is the biggest.
                float a = (i + variant * 0.5f) * 2.399963f;
                float d = r * 0.82f * Mathf.Sqrt((i + 0.5f) / lights);
                b.Box(new Vector3(Mathf.Cos(a) * d, 0.04f, Mathf.Sin(a) * d), new Vector3(0.13f, 0.02f, 0.13f) * (i == 0 ? 1.6f : 1f), TownSwatch.IslandSpark, 0f);
            }
        }

        /// <summary>A Remador: a silhouette of pure light, a little taller than a person, holding a long oar (Variant 1: on the deck of a barge, raised by <see cref="SkyBargeDeckHeight"/>). Does not speak.</summary>
        private static void LightRower(TownMeshBuilder b, float s, int variant)
        {
            float y0 = (variant % 2) == 1 ? SkyBargeDeckHeight * s : 0f;
            b.Cylinder(new Vector3(0f, y0, 0f), 0.2f * s, 0.16f * s, 1.2f * s, 6, TownSwatch.RowerLight, TownSwatch.RowerLight);
            b.Blob(new Vector3(0f, y0 + 1.5f * s, 0f), new Vector3(0.17f, 0.22f, 0.17f) * s, TownSwatch.RowerLight);
            b.Log(new Vector3(-0.4f * s, y0 + 1.15f * s, 0.15f * s), new Vector3(0.75f * s, y0 + 0.1f * s, 0.95f * s), 0.03f * s, TownSwatch.RowerLight, 3);
            b.Log(new Vector3(0.22f * s, y0 + 1f * s, 0f), new Vector3(0.05f * s, y0 + 0.75f * s, 0.55f * s), 0.05f * s, TownSwatch.RowerLight, 3);
        }

        // ------------------------------------------------------------------
        // Dressing
        // ------------------------------------------------------------------

        /// <summary>Ammo's front: bales wrapped in the colours of the road, a pile of presents tied with five ribbons, a camel bell on a stand and a small flame.</summary>
        private static void SkyQuayDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            b.Box(new Vector3(x - 0.75f, y + 0.26f, z - 0.45f), new Vector3(1.1f, 0.5f, 0.72f), TownSwatch.ClothIndigo, 8f, TownSwatch.QuayGold);
            b.Box(new Vector3(x + 0.55f, y + 0.22f, z - 0.42f), new Vector3(0.9f, 0.42f, 0.66f), TownSwatch.Sandstone, -6f, TownSwatch.QuayCloud);
            for (int i = 0; i < 5; i++)
            {
                float dx = -0.46f + i * 0.23f;
                b.Box(new Vector3(x + dx, y + 0.62f, z - 0.45f), new Vector3(0.18f, 0.18f, 0.2f), LightElementSwatch(i), 0f, TownSwatch.QuayGold);
            }

            b.Log(new Vector3(x + 1.25f, y, z - 0.55f), new Vector3(x + 1.25f, y + 1.1f, z - 0.55f), 0.04f, TownSwatch.QuayGold, 4);
            b.Cylinder(new Vector3(x + 1.25f, y + 0.92f, z - 0.55f), 0.13f, 0.08f, 0.16f, 6, TownSwatch.QuayGold, TownSwatch.QuayGold);
            b.Blob(new Vector3(x - 1.2f, y + 0.26f, z - 0.25f), new Vector3(0.1f, 0.16f, 0.1f), TownSwatch.SolidLight);
        }
    }
}
