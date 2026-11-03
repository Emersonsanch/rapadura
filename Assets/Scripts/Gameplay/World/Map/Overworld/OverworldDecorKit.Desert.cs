using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 2 — Areias Flamejantes: the desert's own pieces (bones, broken caravans, pyramids, karez shafts, sandstone
    /// chimneys and arches, red salt crystals, rammed-earth towers, salt crust, the crowned clans' tents). Built around the
    /// origin like the rest of the kit, low-poly on the shared palette (60–160 triangles each, the salt crust 12).
    /// </summary>
    public static partial class OverworldDecorKit
    {
        private static TownStyle _desert;

        /// <summary>Qocho's town style (the kit's desert props are drawn in it).</summary>
        public static TownStyle DesertStyle => _desert ??= TownStyles.For(BiomeType.Desert);

        /// <summary>A desert piece (Região 2, 80–99).</summary>
        public static bool IsDesertDecor(OverworldDecor decor)
        {
            int i = (int)decor;
            return i >= 80 && i < 100;
        }

        /// <summary>Draws a desert piece around the origin (the caller placed it); false when <paramref name="decor"/> is not one.</summary>
        public static bool DesertPiece(TownMeshBuilder b, OverworldDecor decor, float s, int variant)
        {
            switch (decor)
            {
                case OverworldDecor.Bones: Bones(b, s, variant); return true;
                case OverworldDecor.CaravanWreck: CaravanWreck(b, s, variant); return true;
                case OverworldDecor.Pyramid: KitProp(b, CityPropKind.Ruin, s, 0); return true;
                case OverworldDecor.LotusColumn: KitProp(b, CityPropKind.Ruin, s, 1); return true;
                case OverworldDecor.Ushabtis: KitProp(b, CityPropKind.Ruin, s, 2); return true;
                case OverworldDecor.KarezShaft: KitProp(b, CityPropKind.KarezShaft, s, 0); return true;
                case OverworldDecor.Marker: KitProp(b, CityPropKind.Stela, 0.55f * s, 0); return true;
                case OverworldDecor.Hoodoo: Hoodoo(b, s, variant); return true;
                case OverworldDecor.RockArch: RockArch(b, s); return true;
                case OverworldDecor.SaltCrystal: SaltCrystal(b, s, variant); return true;
                case OverworldDecor.TaipaTower: TaipaTower(b, s, variant); return true;
                case OverworldDecor.SaltCrust: SaltCrust(b, s, variant); return true;
                case OverworldDecor.CaravanCamp: CaravanCamp(b, s, variant); return true;
                case OverworldDecor.ClanTent: ClanTent(b, s, variant); return true;
                default: return false;
            }
        }

        private static void KitProp(TownMeshBuilder b, CityPropKind kind, float s, int variant)
        {
            TownKit.Prop(b, new CityProp { Kind = kind, Scale = s, Variant = variant }, DesertStyle);
        }

        /// <summary>A camel's spine and rib cage half buried, the skull a little apart.</summary>
        public static void Bones(TownMeshBuilder b, float s, int variant)
        {
            b.Log(new Vector3(-1.1f, 0.12f, 0f) * s, new Vector3(1.0f, 0.2f, 0.1f) * s, 0.07f * s, TownSwatch.LimeWhite, 4);
            for (int i = 0; i < 4; i++)
            {
                float x = (-0.65f + i * 0.36f) * s;
                float lean = (variant & 1) == 0 ? 0.08f : -0.05f;
                b.Log(new Vector3(x, 0.15f * s, 0.04f), new Vector3(x + lean * s, 0.78f * s, 0.55f * s), 0.045f * s, TownSwatch.LimeWhite, 4);
                b.Log(new Vector3(x, 0.15f * s, -0.04f), new Vector3(x + lean * s, 0.7f * s, -0.52f * s), 0.045f * s, TownSwatch.Plaster, 4);
            }

            var skull = new Vector3(1.55f, 0.2f, 0.45f) * s;
            b.Crown(skull, new Vector3(0.75f, 0.36f, 0.4f) * s, TownSwatch.LimeWhite, 25f);
            b.Blob(skull + new Vector3(0.12f, 0.08f, 0.1f) * s, new Vector3(0.14f, 0.1f, 0.12f) * s, TownSwatch.Black);
            b.Blob(new Vector3(0.1f, 0.03f, 0.7f) * s, new Vector3(1.4f, 0.12f, 0.6f) * s, TownSwatch.SandDark);
        }

        /// <summary>A cart on its side, one wheel off, crates and bales of silk spilled in the sand.</summary>
        public static void CaravanWreck(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.45f, 0f) * s, new Vector3(2.2f, 0.7f, 1.2f) * s, TownSwatch.WoodWeathered, 8f, TownSwatch.Beam);
            b.Box(new Vector3(0f, 0.85f, 0.62f) * s, new Vector3(2.2f, 0.5f, 0.08f) * s, TownSwatch.WoodWeathered, 8f);
            b.Log(new Vector3(-0.9f, 0.45f, -0.7f) * s, new Vector3(-0.9f, 0.45f, -0.85f) * s, 0.45f * s, TownSwatch.Walnut, 8, TownSwatch.WoodDark);
            b.Log(new Vector3(1.6f, 0.06f, 0.9f) * s, new Vector3(1.85f, 0.06f, 1.6f) * s, 0.42f * s, TownSwatch.Walnut, 8, TownSwatch.WoodDark);
            b.Log(new Vector3(1.1f, 0.2f, -0.1f) * s, new Vector3(2.6f, 0.08f, -0.5f) * s, 0.06f * s, TownSwatch.WoodDark, 4);
            b.Box(new Vector3(-1.7f, 0.25f, 0.9f) * s, new Vector3(0.6f, 0.5f, 0.6f) * s, TownSwatch.WoodLight, 30f, TownSwatch.Beam);
            b.Box(new Vector3(-1.4f, 0.22f, -1.3f) * s, new Vector3(0.9f, 0.45f, 0.55f) * s, (variant & 1) == 0 ? TownSwatch.ClothIndigo : TownSwatch.ClothRed, 70f, TownSwatch.Gold);
            b.Box(new Vector3(0.4f, 0.2f, -1.5f) * s, new Vector3(0.8f, 0.4f, 0.5f) * s, TownSwatch.Thatch, 15f, TownSwatch.LeafGold);
        }

        /// <summary>A stone chimney of layered red sandstone, narrowing up to its dark cap rock.</summary>
        public static void Hoodoo(TownMeshBuilder b, float s, int variant)
        {
            float k = 0.85f + 0.1f * (variant & 3);
            b.Cylinder(Vector3.zero, 1.1f * s, 0.85f * s, 1.5f * s * k, 6, TownSwatch.Sandstone, null, 0.2f);
            b.Cylinder(new Vector3(0f, 1.5f * s * k, 0f), 0.85f * s, 0.58f * s, 1.4f * s * k, 6, TownSwatch.Laterite, null, 0.5f);
            b.Cylinder(new Vector3(0f, 2.9f * s * k, 0f), 0.58f * s, 0.46f * s, 1.1f * s * k, 6, TownSwatch.AdobeOchre, null, 0.1f);
            b.Crown(new Vector3(0f, 4.1f * s * k, 0f), new Vector3(1.5f, 0.55f, 1.3f) * s, TownSwatch.StoneDark, 30f * variant);
        }

        /// <summary>A natural sandstone arch: two weathered legs and the span (a hero walks under it).</summary>
        public static void RockArch(TownMeshBuilder b, float s)
        {
            b.Cylinder(new Vector3(-2.3f, 0f, 0f) * s, 1.05f * s, 0.8f * s, 3.4f * s, 6, TownSwatch.Sandstone, null, 0.3f);
            b.Cylinder(new Vector3(2.3f, 0f, 0f) * s, 1.0f * s, 0.75f * s, 3.4f * s, 6, TownSwatch.Laterite, null, 0.1f);
            b.Box(new Vector3(0f, 3.75f, 0f) * s, new Vector3(6.2f, 1.1f, 1.5f) * s, TownSwatch.Sandstone, 0f, TownSwatch.AdobeOchre);
            b.Crown(new Vector3(-1.6f, 4.35f, 0f) * s, new Vector3(2.2f, 0.7f, 1.4f) * s, TownSwatch.Laterite, 10f);
            b.Crown(new Vector3(1.4f, 4.3f, 0.1f) * s, new Vector3(1.8f, 0.6f, 1.3f) * s, TownSwatch.AdobeOchre, 60f);
        }

        /// <summary>Red salt crystals (hexagonal prisms) leaning out of a white salt mound.</summary>
        public static void SaltCrystal(TownMeshBuilder b, float s, int variant)
        {
            b.Crown(new Vector3(0f, 0.15f, 0f) * s, new Vector3(1.9f, 0.5f, 1.7f) * s, TownSwatch.LimeWhite, 15f);
            for (int i = 0; i < 5; i++)
            {
                float a = (i * 72f + variant * 23f) * Mathf.Deg2Rad;
                float r = i == 0 ? 0f : 0.45f;
                float h = i == 0 ? 2.1f : 1.1f + 0.25f * ((i + variant) % 3);
                var foot = new Vector3(Mathf.Cos(a) * r, 0.2f, Mathf.Sin(a) * r) * s;
                var tip = foot + new Vector3(Mathf.Cos(a) * r * 0.9f, h, Mathf.Sin(a) * r * 0.9f) * s;
                b.Log(foot, tip, (i == 0 ? 0.24f : 0.15f) * s, (i & 1) == 0 ? TownSwatch.Sandstone : TownSwatch.ClothRed, 6, TownSwatch.Pearl);
            }
        }

        /// <summary>A watchtower of rammed earth (Qocho's sentinels): tall block, slit windows, half its parapet fallen.</summary>
        public static void TaipaTower(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.25f, 0f) * s, new Vector3(3.2f, 0.5f, 3.2f) * s, TownSwatch.AdobeOchre, 0f);
            b.Box(new Vector3(0f, 2.75f, 0f) * s, new Vector3(2.6f, 4.5f, 2.6f) * s, TownSwatch.Adobe, 0f, TownSwatch.Mud);
            b.Box(new Vector3(0f, 2.2f, -1.31f) * s, new Vector3(0.22f, 0.9f, 0.04f) * s, TownSwatch.Black, 0f);
            b.Box(new Vector3(-1.31f, 3.6f, 0f) * s, new Vector3(0.04f, 0.8f, 0.22f) * s, TownSwatch.Black, 0f);
            b.Box(new Vector3(-0.95f, 5.25f, 0f) * s, new Vector3(0.7f, 0.5f, 2.6f) * s, TownSwatch.Adobe, 0f, TownSwatch.Mud);
            if ((variant & 1) == 0)
            {
                b.Box(new Vector3(0.3f, 5.25f, 0.95f) * s, new Vector3(1.8f, 0.5f, 0.7f) * s, TownSwatch.Adobe, 0f, TownSwatch.Mud);
            }

            b.Crown(new Vector3(1.9f, 0.3f, -0.6f) * s, new Vector3(1.4f, 0.6f, 1.2f) * s, TownSwatch.AdobeOchre, 35f);
            b.Log(new Vector3(0.6f, 5.5f, -0.6f) * s, new Vector3(0.6f, 7f, -0.6f) * s, 0.04f * s, TownSwatch.WoodDark, 4);
            b.Box(new Vector3(0.95f, 6.6f, -0.6f) * s, new Vector3(0.7f, 0.45f, 0.03f) * s, TownSwatch.ClothIndigo, 0f);
        }

        /// <summary>A flat hexagon of salt crust with a raised rim (ground detail).</summary>
        public static void SaltCrust(TownMeshBuilder b, float s, int variant)
        {
            float phase = (variant % 6) / 6f;
            b.Disc(new Vector3(0f, 0.035f, 0f), 1.1f * s, 6, TownSwatch.Plaster, phase);
            b.Disc(new Vector3(0f, 0.06f, 0f), 0.85f * s, 6, (variant & 1) == 0 ? TownSwatch.LimeWhite : TownSwatch.Pearl, phase);
        }

        /// <summary>A resting caravan: two camels, bales of silk, jars and crates round a cold hearth.</summary>
        public static void CaravanCamp(TownMeshBuilder b, float s, int variant)
        {
            b.With(new Vector3(-1.6f, 0f, 0.8f) * s, 20f, () => KitProp(b, CityPropKind.Camel, s, 0));
            if ((variant & 1) == 0)
            {
                b.With(new Vector3(1.8f, 0f, 1.2f) * s, 200f, () => KitProp(b, CityPropKind.Camel, 0.9f * s, 0));
            }

            b.Box(new Vector3(0.2f, 0.25f, -1.4f) * s, new Vector3(1f, 0.5f, 0.6f) * s, TownSwatch.ClothIndigo, 10f, TownSwatch.Gold);
            b.Box(new Vector3(1.3f, 0.25f, -1.1f) * s, new Vector3(0.9f, 0.5f, 0.6f) * s, TownSwatch.ClothRed, 60f, TownSwatch.Gold);
            b.Box(new Vector3(-0.9f, 0.25f, -1.6f) * s, new Vector3(0.6f, 0.5f, 0.6f) * s, TownSwatch.WoodLight, 30f, TownSwatch.Beam);
            TownKit.Jugs(b, new Vector3(2.2f, 0f, -0.3f) * s);
            b.Ring(new Vector3(0f, 0.04f, 0f), 0.35f * s, 0.6f * s, 7, TownSwatch.Stone);
            b.Disc(new Vector3(0f, 0.03f, 0f), 0.35f * s, 7, TownSwatch.Black);
        }

        /// <summary>A clan tent of ochre-dyed leather on a pole, the war drum and a cloth banner beside it.</summary>
        public static void ClanTent(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 1.8f * s, 0f, 2.5f * s, 6, (variant & 1) == 0 ? TownSwatch.AdobeOchre : TownSwatch.Laterite, null, 0.5f);
            b.Log(new Vector3(0f, 2.3f, 0f) * s, new Vector3(0f, 3f, 0f) * s, 0.05f * s, TownSwatch.WoodDark, 4);
            b.Box(new Vector3(0f, 0.6f, -1.62f) * s, new Vector3(0.7f, 1.1f, 0.05f) * s, TownSwatch.Black, 0f);
            var drum = new Vector3(2.3f, 0f, -0.6f) * s;
            b.Cylinder(drum, 0.5f * s, 0.45f * s, 0.75f * s, 8, TownSwatch.Laterite, TownSwatch.Thatch);
            b.Log(new Vector3(-2.1f, 0f, -0.9f) * s, new Vector3(-2.1f, 2.6f, -0.9f) * s, 0.04f * s, TownSwatch.WoodDark, 4);
            b.Box(new Vector3(-1.75f, 2.25f, -0.9f) * s, new Vector3(0.7f, 0.6f, 0.03f) * s, TownSwatch.ClothYellow, 0f);
        }
    }
}
