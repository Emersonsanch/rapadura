using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of the Fortaleza da Última Lâmpada, the fortress of the Limiar Profundo (Docs/Mapas100.md map 83, style
    /// <c>BasaltFort</c>): the keeps of basalt with their rows of lamp niches (<see cref="BasaltKeepHouse"/>), the arc of the
    /// rampart over the abyss with its thousand niches, the five gates of the pits (one for each dark element), the Lâmpada-Mãe Grande,
    /// the niche steles, the racks of wicks, the copper-bound oil cisterns, the gate of two basalt piers and Tehat's front. A lamp and
    /// its oil only: no altar, no figure, no gore (bible §4.3). Same rules as the rest of the kit: flat colours from
    /// <see cref="TownPalette"/> (row 25), low-poly, deterministic.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Radius of the arc of a <see cref="CityPropKind.LampRampart"/> (× its scale), centred on the plaza.</summary>
        public const float LampRampartRadius = 18f;

        /// <summary>Half the thickness of the rampart: the radius of each collider along its arc.</summary>
        public const float LampRampartHalfThickness = 1.3f;

        /// <summary>The rampart runs from this angle (degrees; 0 = east, counter-clockwise) to 180 minus it, always north of the plaza.</summary>
        public const float LampRampartStartAngle = 12f;

        /// <summary>Distance between two colliders along the arc (shorter than the thickness, so the wall has no holes).</summary>
        public const float LampRampartColliderStep = 1.8f;

        /// <summary>Height of the rampart.</summary>
        public const float LampRampartHeight = 4.6f;

        /// <summary>Radius of a <see cref="CityPropKind.MotherLamp"/>'s plinth (× its scale): its collider.</summary>
        public const float MotherLampRadius = 1.4f;

        /// <summary>Radius of a <see cref="CityPropKind.NicheStele"/> (× its scale): its collider.</summary>
        public const float NicheSteleRadius = 0.7f;

        /// <summary>Radius of a <see cref="CityPropKind.WickRack"/> (× its scale): its collider.</summary>
        public const float WickRackRadius = 0.8f;

        /// <summary>Radius of an <see cref="CityPropKind.OilCistern"/> (× its scale): its collider.</summary>
        public const float OilCisternRadius = 0.9f;

        private const int RampartChords = 13;

        /// <summary>The centres of the capsule colliders along the arc of a rampart standing at <paramref name="center"/>.</summary>
        public static IEnumerable<Vector2> LampRampartColliders(Vector2 center, float scale)
        {
            float radius = LampRampartRadius * scale;
            float sweep = (180f - LampRampartStartAngle * 2f) * Mathf.Deg2Rad;
            int count = Mathf.CeilToInt(radius * sweep / (LampRampartColliderStep * scale));
            for (int i = 0; i <= count; i++)
            {
                float a = LampRampartStartAngle * Mathf.Deg2Rad + sweep * i / count;
                yield return center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
            }
        }

        // ------------------------------------------------------------------
        // The house
        // ------------------------------------------------------------------

        /// <summary>
        /// A <see cref="TownBuildingSpec.BasaltKeep"/> (local frame, front −z): walls of dark basalt (<see cref="TownBuildingSpec.WallColor"/>)
        /// with two rows of small lit niches, a copper-framed door under a lintel of amber, deep slit windows, a flat roof behind square
        /// merlons with a copper band; in front the porch: a basalt deck, copper posts (at <see cref="TownBuildingBuilder.PorchPostsX"/>,
        /// the colliders use the same list), an awning of white cloth and the plaque.
        /// </summary>
        public static void BasaltKeepHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            float eave = Mathf.Max(top, 2.6f);
            float wx = hw - 0.15f;
            float zw = zFacade + 0.1f, zb = zBack - 0.1f;

            b.BoxMinMax(new Vector3(-hw, 0f, zw - 0.08f), new Vector3(hw, 0.32f, zBack), spec.PlinthColor, spec.PlinthColor);
            b.BoxMinMax(new Vector3(-wx, 0.32f, zw), new Vector3(wx, eave, zb), spec.WallColor, spec.WallColor);
            b.Box(new Vector3(0f, eave - 0.26f, zw - 0.05f), new Vector3(wx * 2f + 0.08f, 0.14f, 0.08f), spec.TrimColor, 0f);

            float doorX = Mathf.Clamp(spec.DoorOffset * spec.Width, -wx + 0.8f, wx - 0.8f);
            float door = spec.WideDoor ? 0.75f : 0.55f;
            const float doorTop = 2.15f;

            // The two rows of lit niches, leaving the door clear.
            int bays = Mathf.Clamp(Mathf.RoundToInt(wx * 2f / 0.75f), 4, 10);
            for (int i = 0; i < bays; i++)
            {
                float x = Mathf.Lerp(-wx + 0.32f, wx - 0.32f, (float)i / (bays - 1));
                if (Mathf.Abs(x - doorX) > door + 0.35f)
                {
                    FrontNiche(b, x, 1.15f, zw - 0.04f, spec.TrimColor);
                }

                if ((i % 2) == 0 && eave > 2.7f)
                {
                    FrontNiche(b, x, eave - 0.7f, zw - 0.04f, spec.TrimColor);
                }
            }

            b.BoxMinMax(new Vector3(doorX - door - 0.1f, 0.3f, zw - 0.14f), new Vector3(doorX + door + 0.1f, doorTop + 0.1f, zw - 0.02f), spec.JoineryColor);
            b.Box(new Vector3(doorX, (0.3f + doorTop) * 0.5f, zw - 0.16f), new Vector3(door * 2f - 0.1f, doorTop - 0.35f, 0.03f), TownSwatch.AbyssIndigo, 0f);
            b.Box(new Vector3(doorX, doorTop + 0.18f, zw - 0.17f), new Vector3(door * 2f + 0.5f, 0.2f, 0.08f), spec.AccentColor, 0f);

            int windows = Mathf.Clamp(spec.FrontWindows, 0, 2);
            for (int i = 0; i < windows; i++)
            {
                float side = i == 0 ? -1f : 1f;
                float x = Mathf.Clamp(doorX + side * (door + 1.0f), -wx + 0.4f, wx - 0.4f);
                if (Mathf.Abs(x - doorX) < door + 0.42f)
                {
                    continue;
                }

                b.Box(new Vector3(x, 1.75f, zw - 0.1f), new Vector3(0.32f, 0.72f, 0.05f), spec.JoineryColor, 0f);
                b.Box(new Vector3(x, 1.75f, zw - 0.13f), new Vector3(0.18f, 0.56f, 0.03f), TownSwatch.ThousandLamps, 0f);
            }

            // The flat roof behind square merlons.
            b.BoxMinMax(new Vector3(-hw, eave, zFacade), new Vector3(hw, eave + 0.14f, zBack), spec.RoofColor, spec.RoofColor);
            int merlons = Mathf.Clamp(Mathf.RoundToInt(hw * 2f / 0.8f), 5, 11);
            for (int i = 0; i < merlons; i++)
            {
                float x = Mathf.Lerp(-hw + 0.22f, hw - 0.22f, i / (merlons - 1f));
                b.Box(new Vector3(x, eave + 0.34f, zFacade + 0.1f), new Vector3(0.42f, 0.5f, 0.2f), spec.WallColor, 0f, spec.TrimColor);
            }

            foreach (float side in new[] { -1f, 1f })
            {
                b.Box(new Vector3(side * (hw - 0.1f), eave + 0.26f, (zFacade + zBack) * 0.5f), new Vector3(0.2f, 0.36f, zBack - zFacade), spec.WallColor, 0f, spec.TrimColor);
            }

            b.Box(new Vector3(0f, eave + 0.26f, zBack - 0.1f), new Vector3(hw * 2f, 0.36f, 0.2f), spec.WallColor, 0f, spec.TrimColor);

            if (spec.PorchDepth > 0.01f)
            {
                float deck = CitySolids.DeckHeight;
                b.BoxMinMax(new Vector3(-hw, 0f, zFront), new Vector3(hw, deck, zFacade), spec.PlinthColor, TownSwatch.BasaltDust);
                float z = zFront + TownBuildingBuilder.PostInset;
                float beamY = Mathf.Min(eave, 2.75f);
                foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
                {
                    b.Box(new Vector3(x, deck + (beamY - deck) * 0.5f, z), new Vector3(0.22f, beamY - deck, 0.22f), TownSwatch.LampCopper, 0f);
                }

                b.Box(new Vector3(0f, beamY + 0.05f, z), new Vector3(hw * 2f, 0.14f, 0.2f), spec.JoineryColor, 0f);
                b.Slab(new Vector3(-hw, beamY + 0.08f, zFront - 0.1f), new Vector3(-hw, beamY + 0.25f, zFacade),
                    new Vector3(hw, beamY + 0.25f, zFacade), new Vector3(hw, beamY + 0.08f, zFront - 0.1f), 0.06f, TownSwatch.WickLinen, spec.TrimColor);

                float lampX = Mathf.Clamp(doorX + (doorX >= 0f ? -1f : 1f) * (door + 0.9f), -hw + 0.4f, hw - 0.4f);
                HangingLamp(b, new Vector3(lampX, beamY - 0.05f, (zFront + zFacade) * 0.5f), 0.9f);

                if (spec.Sign)
                {
                    var at = new Vector3(doorX, beamY - 0.2f, z - 0.14f);
                    b.Box(at, new Vector3(1.1f, 0.3f, 0.05f), spec.AccentColor, 0f);
                    b.Box(at + new Vector3(0f, 0f, -0.03f), new Vector3(0.94f, 0.2f, 0.02f), spec.SignColor, 0f);
                }
            }
        }

        /// <summary>A small lit niche on a wall facing −z: a recess, a copper sill and the amber flame.</summary>
        private static void FrontNiche(TownMeshBuilder b, float x, float y, float z, TownSwatch sill)
        {
            b.Box(new Vector3(x, y, z - 0.01f), new Vector3(0.26f, 0.36f, 0.02f), TownSwatch.AbyssBlack, 0f);
            b.Box(new Vector3(x, y - 0.2f, z - 0.03f), new Vector3(0.32f, 0.05f, 0.06f), sill, 0f);
            b.Box(new Vector3(x, y - 0.04f, z - 0.03f), new Vector3(0.1f, 0.16f, 0.02f), TownSwatch.ThousandLamps, 0f);
        }

        /// <summary>A copper lamp hanging from <paramref name="at"/> on a short chain, with its amber flame.</summary>
        private static void HangingLamp(TownMeshBuilder b, Vector3 at, float s)
        {
            b.Log(at, at + Vector3.down * 0.4f * s, 0.018f * s, TownSwatch.LampCopper, 3);
            b.Cylinder(at + Vector3.down * 0.6f * s, 0.09f * s, 0.2f * s, 0.2f * s, 6, TownSwatch.LampCopper, TownSwatch.OilDark);
            b.Blob(at + Vector3.down * 0.34f * s, new Vector3(0.1f, 0.15f, 0.1f) * s, TownSwatch.ThousandLamps);
        }

        // ------------------------------------------------------------------
        // The gate
        // ------------------------------------------------------------------

        /// <summary>
        /// The gate of the Fortaleza at a street's end: two blunt piers of basalt with lit niches and copper bands, a heavy lintel of
        /// basalt with a band of copper studs and a copper lamp hanging in the middle.
        /// </summary>
        public static void BasaltFortGate(TownMeshBuilder b, Vector3 a, Vector3 c)
        {
            Vector3 span = c - a;
            float w = new Vector2(span.x, span.z).magnitude;
            if (w < 0.5f)
            {
                return;
            }

            b.With(a, YawAlong(new Vector2(span.x, span.z) / w), () =>
            {
                const float pierTop = 3.9f;
                foreach (float x in new[] { 0f, w })
                {
                    b.Box(new Vector3(x, pierTop * 0.5f, 0f), new Vector3(1.15f, pierTop, 1.15f), TownSwatch.LampBasalt, 0f, TownSwatch.BasaltDust);
                    b.Box(new Vector3(x, 1.0f, 0f), new Vector3(1.21f, 0.14f, 1.21f), TownSwatch.LampCopper, 0f);
                    b.Box(new Vector3(x, 2.9f, 0f), new Vector3(1.21f, 0.14f, 1.21f), TownSwatch.LampCopper, 0f);
                    foreach (float z in new[] { -0.59f, 0.59f })
                    {
                        b.Box(new Vector3(x, 1.95f, z), new Vector3(0.28f, 0.42f, 0.03f), TownSwatch.AbyssBlack, 0f);
                        b.Box(new Vector3(x, 1.9f, z * 1.03f), new Vector3(0.1f, 0.18f, 0.02f), TownSwatch.ThousandLamps, 0f);
                    }
                }

                b.Box(new Vector3(w * 0.5f, pierTop - 0.3f, 0f), new Vector3(w, 0.72f, 0.7f), TownSwatch.LampBasalt, 0f, TownSwatch.BasaltDust);
                for (int i = 0; i < 7; i++)
                {
                    float x = Mathf.Lerp(0.3f, w - 0.3f, i / 6f);
                    b.Box(new Vector3(x, pierTop - 0.3f, -0.36f), new Vector3(0.14f, 0.14f, 0.03f), TownSwatch.LampCopper, 0f);
                }

                HangingLamp(b, new Vector3(w * 0.5f, pierTop - 0.66f, 0f), 1.1f);
            });
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>
        /// The arc of the rampart north of the plaza, over the abyss, drawn in straight lengths from <see cref="LampRampartStartAngle"/>
        /// to 180 minus it (the local origin is the centre of the plaza; the inner face looks at it): dark basalt, rows of lit niches
        /// on the inner face, merlons and a copper band along the top. Its colliders are <see cref="LampRampartColliders"/>.
        /// </summary>
        private static void LampRampart(TownMeshBuilder b, float s)
        {
            float radius = LampRampartRadius * s;
            float start = LampRampartStartAngle * Mathf.Deg2Rad;
            float step = (180f - LampRampartStartAngle * 2f) * Mathf.Deg2Rad / RampartChords;
            float half = LampRampartHalfThickness * s;
            float h = LampRampartHeight * s;
            for (int i = 0; i < RampartChords; i++)
            {
                float a0 = start + step * i, a1 = a0 + step;
                var p0 = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * radius;
                var p1 = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius;
                Vector2 chord = p1 - p0;
                float length = chord.magnitude;
                int seed = i;
                b.With(new Vector3(p0.x, 0f, p0.y), YawAlong(chord / length), () =>
                {
                    // Local +x runs along the chord and local +z looks at the plaza.
                    b.Box(new Vector3(length * 0.5f, h * 0.5f, 0f), new Vector3(length + 0.25f * s, h, half * 2f), TownSwatch.LampBasalt, 0f, TownSwatch.BasaltDust);
                    b.Box(new Vector3(length * 0.5f, h - 0.3f * s, half + 0.02f), new Vector3(length + 0.25f * s, 0.14f * s, 0.05f), TownSwatch.LampCopper, 0f);
                    for (int row = 0; row < 2; row++)
                    {
                        float y = (1.1f + row * 1.4f) * s;
                        for (int k = 0; k < 3; k++)
                        {
                            float x = length * (k + (row == 0 ? 0.5f : 0.75f) - (row == 0 ? 0f : 0.25f)) / 3f;
                            b.Box(new Vector3(x, y, half + 0.02f), new Vector3(0.34f, 0.46f, 0.03f) * s, TownSwatch.AbyssBlack, 0f);
                            bool lit = ((seed + row + k) % 4) != 0;
                            b.Box(new Vector3(x, y - 0.06f * s, half + 0.05f), new Vector3(0.12f, 0.2f, 0.02f) * s, lit ? TownSwatch.ThousandLamps : TownSwatch.OilDark, 0f);
                        }
                    }

                    for (int m = 0; m < 3; m++)
                    {
                        float x = length * (m + 0.5f) / 3f;
                        b.Box(new Vector3(x, h + 0.28f * s, 0f), new Vector3(0.7f * s, 0.56f * s, half * 2f - 0.2f * s), TownSwatch.LampBasalt, 0f, TownSwatch.BasaltDust);
                    }
                });
            }
        }

        /// <summary>
        /// One of the five gates of the pits set into the rampart (front −z, looking at the plaza): a tall arch of basalt with a copper
        /// frame round a sealed leaf, the colour of its element on the lintel and in the lamps on each side (<paramref name="variant"/> 0–4:
        /// Fumaça, Fogo Devorador, Vento Destruidor, Lodo, Treva). Closed: the pits are behind it.
        /// </summary>
        private static void CoveGate(TownMeshBuilder b, float s, int variant)
        {
            TownSwatch element = ElementSwatch(variant);
            float z = -0.12f * s;
            b.Box(new Vector3(0f, 1.75f * s, 0.08f * s), new Vector3(2.5f, 3.5f, 0.2f) * s, TownSwatch.LampBasalt, 0f, TownSwatch.BasaltDust);
            b.Box(new Vector3(0f, 1.45f * s, z), new Vector3(1.5f, 2.6f, 0.05f) * s, TownSwatch.AbyssBlack, 0f);
            foreach (float side in new[] { -1f, 1f })
            {
                b.Box(new Vector3(side * 0.85f * s, 1.45f * s, z), new Vector3(0.14f, 2.75f, 0.1f) * s, TownSwatch.LampCopper, 0f);
                b.Box(new Vector3(side * 1.1f * s, 2.2f * s, z), new Vector3(0.2f, 0.34f, 0.05f) * s, element, 0f);
                b.Box(new Vector3(side * 0.4f * s, 1.45f * s, z - 0.02f * s), new Vector3(0.65f, 2.45f, 0.03f) * s, TownSwatch.OilDark, 0f);
            }

            b.Box(new Vector3(0f, 2.95f * s, z), new Vector3(2.0f, 0.42f, 0.14f) * s, TownSwatch.LampCopper, 0f);
            b.Box(new Vector3(0f, 2.95f * s, z - 0.08f * s), new Vector3(0.9f, 0.22f, 0.04f) * s, element, 0f);
        }

        /// <summary>The colour of a pit's element: 0 Fumaça, 1 Fogo Devorador, 2 Vento Destruidor, 3 Lodo, 4 Treva.</summary>
        public static TownSwatch ElementSwatch(int variant)
        {
            switch ((variant % 5 + 5) % 5)
            {
                case 0: return TownSwatch.BasaltDust;
                case 1: return TownSwatch.Ember;
                case 2: return TownSwatch.Glass;
                case 3: return TownSwatch.Kelp;
                default: return TownSwatch.AbyssIndigo;
            }
        }

        /// <summary>
        /// The Lâmpada-Mãe Grande: three stepped rings of basalt with copper rims (the three rings of the fortress' lamp network), a thick
        /// column, a great bowl of copper and the amber flame. A lamp, not an idol.
        /// </summary>
        private static void MotherLamp(TownMeshBuilder b, float s)
        {
            float r = MotherLampRadius * s;
            for (int i = 0; i < 3; i++)
            {
                float y = i * 0.28f * s;
                float ring = r * (1f - i * 0.24f);
                b.Cylinder(new Vector3(0f, y, 0f), ring, ring * 0.97f, 0.24f * s, 12, TownSwatch.LampBasalt, TownSwatch.BasaltDust);
                b.Cylinder(new Vector3(0f, y + 0.24f * s, 0f), ring * 0.99f, ring * 0.94f, 0.04f * s, 12, TownSwatch.LampCopper, TownSwatch.LampCopper);
            }

            float top = 0.84f * s;
            b.Cylinder(new Vector3(0f, top, 0f), 0.42f * s, 0.3f * s, 1.5f * s, 8, TownSwatch.LampBasalt, TownSwatch.LampCopper);
            b.Cylinder(new Vector3(0f, top + 0.9f * s, 0f), 0.36f * s, 0.34f * s, 0.1f * s, 8, TownSwatch.LampCopper, TownSwatch.LampCopper);
            float bowl = top + 1.5f * s;
            b.Cylinder(new Vector3(0f, bowl, 0f), 0.3f * s, 1.0f * s, 0.5f * s, 12, TownSwatch.LampCopper, TownSwatch.OilDark);
            b.Cylinder(new Vector3(0f, bowl + 0.5f * s, 0f), 1.0f * s, 1.05f * s, 0.08f * s, 12, TownSwatch.LampCopper, TownSwatch.OilDark);
            b.Blob(new Vector3(0f, bowl + 1.0f * s, 0f), new Vector3(0.55f, 0.95f, 0.55f) * s, TownSwatch.ThousandLamps);
            b.Blob(new Vector3(0f, bowl + 0.75f * s, 0f), new Vector3(0.28f, 0.45f, 0.28f) * s, TownSwatch.WickLinen);
        }

        /// <summary>A standing stele of basalt with rows of lit niches on its front (Variant 1: taller, with a copper cap).</summary>
        private static void NicheStele(TownMeshBuilder b, float s, int variant)
        {
            bool tall = (variant % 2) == 1;
            float h = (tall ? 3.0f : 2.2f) * s;
            float r = NicheSteleRadius * s;
            b.Box(new Vector3(0f, 0.1f * s, 0f), new Vector3(r * 2f, 0.2f * s, r * 1.5f), TownSwatch.AbyssIndigo, 0f);
            b.Box(new Vector3(0f, 0.2f * s + h * 0.5f, 0f), new Vector3(r * 1.5f, h, 0.5f * s), TownSwatch.LampBasalt, 0f, TownSwatch.BasaltDust);
            for (int row = 0; row < (tall ? 4 : 3); row++)
            {
                float y = 0.2f * s + (row + 0.6f) * 0.62f * s;
                foreach (float x in new[] { -0.26f, 0.26f })
                {
                    b.Box(new Vector3(x * s, y, -0.26f * s), new Vector3(0.24f, 0.34f, 0.03f) * s, TownSwatch.AbyssBlack, 0f);
                    b.Box(new Vector3(x * s, y - 0.04f * s, -0.28f * s), new Vector3(0.08f, 0.15f, 0.02f) * s, TownSwatch.ThousandLamps, 0f);
                }
            }

            if (tall)
            {
                b.Box(new Vector3(0f, 0.28f * s + h, 0f), new Vector3(r * 1.7f, 0.16f * s, 0.62f * s), TownSwatch.LampCopper, 0f);
            }
        }

        /// <summary>Tehat's rack: two copper posts, three rods with white wicks hanging to dry (Variant 1: a longer rack with a basket of wicks at the foot).</summary>
        private static void WickRack(TownMeshBuilder b, float s, int variant)
        {
            float half = ((variant % 2) == 1 ? 0.9f : 0.65f) * s;
            float h = 1.8f * s;
            foreach (float x in new[] { -half, half })
            {
                b.Log(new Vector3(x, 0f, 0f), new Vector3(x, h, 0f), 0.05f * s, TownSwatch.LampCopper, 4);
            }

            for (int rod = 0; rod < 3; rod++)
            {
                float y = h - 0.05f * s - rod * 0.2f * s;
                b.Log(new Vector3(-half, y, 0f), new Vector3(half, y, 0f), 0.025f * s, TownSwatch.LampBasalt, 3);
                int wicks = Mathf.RoundToInt(half * 2f / (0.2f * s));
                for (int k = 0; k < wicks; k++)
                {
                    float x = Mathf.Lerp(-half + 0.08f * s, half - 0.08f * s, wicks <= 1 ? 0.5f : (float)k / (wicks - 1));
                    float length = (0.45f + 0.12f * ((k + rod) % 3)) * s;
                    b.Box(new Vector3(x, y - length * 0.5f, 0f), new Vector3(0.03f * s, length, 0.03f * s), TownSwatch.WickLinen, 0f);
                }
            }

            if ((variant % 2) == 1)
            {
                b.Cylinder(new Vector3(0.1f * s, 0f, 0.35f * s), 0.22f * s, 0.26f * s, 0.26f * s, 6, TownSwatch.OilDark, TownSwatch.WickLinen);
            }
        }

        /// <summary>A cistern of lamp oil: a basalt drum with copper bands, a copper lid and a little lit spout (Variant 1: with a smaller cistern against it).</summary>
        private static void OilCistern(TownMeshBuilder b, float s, int variant)
        {
            float r = OilCisternRadius * s;
            b.Cylinder(Vector3.zero, r, r * 0.92f, 0.85f * s, 10, TownSwatch.LampBasalt, TownSwatch.LampCopper);
            b.Cylinder(new Vector3(0f, 0.2f * s, 0f), r * 1.02f, r * 1.02f, 0.08f * s, 10, TownSwatch.LampCopper, TownSwatch.LampCopper);
            b.Cylinder(new Vector3(0f, 0.65f * s, 0f), r * 0.98f, r * 0.98f, 0.08f * s, 10, TownSwatch.LampCopper, TownSwatch.LampCopper);
            b.Cylinder(new Vector3(0f, 0.85f * s, 0f), r * 0.9f, r * 0.72f, 0.14f * s, 10, TownSwatch.LampCopper, TownSwatch.OilDark);
            b.Blob(new Vector3(0f, 1.03f * s, 0f), new Vector3(0.1f, 0.16f, 0.1f) * s, TownSwatch.ThousandLamps);
            if ((variant % 2) == 1)
            {
                b.Cylinder(new Vector3(r * 0.8f, 0f, r * 0.2f), r * 0.32f, r * 0.3f, 0.55f * s, 8, TownSwatch.LampBasalt, TownSwatch.LampCopper);
            }
        }

        // ------------------------------------------------------------------
        // Dressing
        // ------------------------------------------------------------------

        /// <summary>Tehat's front: a basket of wicks, skeins of white thread on a low bench, jars of oil and a small lit lamp.</summary>
        private static void BasaltFortDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            b.Box(new Vector3(x, y + 0.02f, z - 0.45f), new Vector3(2.3f, 0.04f, 0.9f), TownSwatch.WickLinen, 0f);
            b.Box(new Vector3(x - 0.55f, y + 0.25f, z - 0.4f), new Vector3(1.1f, 0.06f, 0.5f), TownSwatch.LampCopper, 0f);
            foreach (float dx in new[] { -1.0f, -0.1f })
            {
                b.Box(new Vector3(x + dx, y + 0.12f, z - 0.4f), new Vector3(0.08f, 0.2f, 0.42f), TownSwatch.LampBasalt, 0f);
            }

            for (int i = 0; i < 4; i++)
            {
                float dx = -0.95f + i * 0.27f;
                b.Cylinder(new Vector3(x + dx, y + 0.3f, z - 0.4f), 0.1f, 0.09f, 0.14f, 5, TownSwatch.WickLinen, TownSwatch.WickLinen);
            }

            b.Cylinder(new Vector3(x + 0.55f, y + 0.04f, z - 0.45f), 0.2f, 0.27f, 0.3f, 6, TownSwatch.OilDark, TownSwatch.WickLinen);
            foreach (float dx in new[] { 0.95f, 1.2f })
            {
                b.Cylinder(new Vector3(x + dx, y + 0.04f, z - 0.3f), 0.1f, 0.14f, 0.3f, 6, TownSwatch.LampBasalt, TownSwatch.LampCopper);
            }

            HangingLamp(b, new Vector3(x + 1.0f, y + 1.15f, zFacade - 0.25f), 0.8f);
        }
    }
}
