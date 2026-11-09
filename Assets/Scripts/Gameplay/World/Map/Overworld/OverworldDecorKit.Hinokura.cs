using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 12 — Arquipélago de Hinokura: the volcanic islands' pieces (low-poly, the shared palette; each built round the origin
    /// and placed by the chunk builder): lava boulders with glowing cracks, pumice rocks, stone lanterns (garden lamps, no shrine),
    /// pines pruned in cloud pads, gates of red lacquer (the palaces' double gates, form only), fumaroles, obsidian shards, ember
    /// coral, broom bushes and stone roses of Mount Echeyde, canary pines, furnace towers and slag heaps of the castle; and the
    /// ground detail stamped by the hundred: hot pools, raked gravel, ash drifts and glowing lava cracks.
    /// </summary>
    public static partial class OverworldDecorKit
    {
        /// <summary>A Hinokura piece (Região 12, 260–279).</summary>
        public static bool IsHinokuraDecor(OverworldDecor decor)
        {
            int i = (int)decor;
            return i >= 260 && i < 280;
        }

        public static readonly TownStamp HotPoolStamp = TownMeshBuilder.Record(b => HotPool(b, 1f));
        public static readonly TownStamp RakedGravelStamp = TownMeshBuilder.Record(b => RakedGravel(b, 1f));
        public static readonly TownStamp AshDriftStamp = TownMeshBuilder.Record(b => AshDrift(b, 1f));
        public static readonly TownStamp LavaCrackStamp = TownMeshBuilder.Record(b => LavaCrack(b, 1f));

        /// <summary>A boulder of black volcanic rock, two cracks of glowing lava across it.</summary>
        public static void LavaBoulder(TownMeshBuilder b, float s, int variant)
        {
            b.Crown(new Vector3(0f, 0.5f, 0f) * s, new Vector3(2.2f, 1.6f, 1.9f) * s, (variant & 1) == 0 ? TownSwatch.Basalt : TownSwatch.Black, variant * 41f);
            b.Box(new Vector3(0.1f, 0.85f, 0f) * s, new Vector3(1.9f, 0.07f, 0.12f) * s, TownSwatch.Ember, 25f + variant * 7f);
            b.Box(new Vector3(-0.3f, 0.55f, 0.3f) * s, new Vector3(0.1f, 0.6f, 1.5f) * s, TownSwatch.Ember, -15f);
            if ((variant & 2) == 0)
            {
                b.Blob(new Vector3(1.2f, 0.25f, -0.6f) * s, new Vector3(0.7f, 0.5f, 0.6f) * s, TownSwatch.StoneDark);
            }
        }

        /// <summary>A pale boulder of pumice, a smaller one beside it.</summary>
        public static void PumiceRock(TownMeshBuilder b, float s, int variant)
        {
            b.Crown(new Vector3(0f, 0.45f, 0f) * s, new Vector3(1.9f, 1.3f, 1.6f) * s, (variant & 1) == 0 ? TownSwatch.GoodSmoke : TownSwatch.Plaster, variant * 29f);
            b.Blob(new Vector3(0.9f, 0.2f, 0.7f) * s, new Vector3(0.7f, 0.5f, 0.6f) * s, TownSwatch.Sandstone);
            b.Disc(new Vector3(-0.3f, 0.9f, -0.79f) * s, 0.12f * s, 5, TownSwatch.StoneDark);
        }

        /// <summary>A stone lantern: a pumice foot, a post, a lit fire box and a wide cap of volcanic stone.</summary>
        public static void StoneLantern(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.down * 0.2f * s, 0.55f * s, 0.5f * s, 0.45f * s, 6, TownSwatch.GoodSmoke, TownSwatch.Stone);
            b.Cylinder(new Vector3(0f, 0.25f, 0f) * s, 0.2f * s, 0.18f * s, 0.9f * s, 6, TownSwatch.Stone);
            b.Box(new Vector3(0f, 1.35f, 0f) * s, new Vector3(0.6f, 0.5f, 0.6f) * s, TownSwatch.StoneDark);
            b.Box(new Vector3(0f, 1.35f, 0f) * s, new Vector3(0.36f, 0.32f, 0.62f) * s, TownSwatch.LampGlow);
            b.Cylinder(new Vector3(0f, 1.6f, 0f) * s, 0.62f * s, 0.12f * s, 0.4f * s, 6, TownSwatch.Basalt, null, 0.5f);
            if ((variant & 1) == 0)
            {
                b.Blob(new Vector3(0f, 2.05f, 0f) * s, new Vector3(0.16f, 0.16f, 0.16f) * s, TownSwatch.Basalt);
            }
        }

        /// <summary>A pine pruned in cloud pads on a block of lava rock: a bent trunk, three flat round pads of needles.</summary>
        public static void CloudPine(TownMeshBuilder b, float s, int variant)
        {
            b.Crown(new Vector3(0f, 0.2f, 0f) * s, new Vector3(1.8f, 0.9f, 1.6f) * s, TownSwatch.Basalt, variant * 17f);
            float lean = (variant & 1) == 0 ? 0.6f : -0.5f;
            var p0 = new Vector3(0f, 0.5f, 0f) * s;
            var p1 = new Vector3(lean, 2f, 0.2f) * s;
            var p2 = new Vector3(lean * 0.2f, 3.4f, -0.2f) * s;
            b.Log(p0, p1, 0.22f * s, TownSwatch.Trunk, 5);
            b.Log(p1, p2, 0.16f * s, TownSwatch.Trunk, 5);
            b.Log(p1, p1 + new Vector3(1.4f * Mathf.Sign(lean), 0.4f, 0.3f) * s, 0.1f * s, TownSwatch.Trunk, 4);
            b.Blob(p2 + new Vector3(0f, 0.3f, 0f) * s, new Vector3(1.8f, 0.55f, 1.6f) * s, TownSwatch.Pine);
            b.Blob(p1 + new Vector3(1.5f * Mathf.Sign(lean), 0.5f, 0.3f) * s, new Vector3(1.3f, 0.45f, 1.1f) * s, TownSwatch.Larch);
            b.Blob(p1 + new Vector3(-0.6f * Mathf.Sign(lean), 0.2f, -0.5f) * s, new Vector3(1.1f, 0.4f, 1f) * s, TownSwatch.Pine);
        }

        /// <summary>A gate of red lacquer: two posts on a stone step, a lintel and a little red-tiled roof.</summary>
        public static void LacquerGate(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.1f, 0f) * s, new Vector3(4.2f, 0.3f, 1.4f) * s, TownSwatch.CoralLime, 0f, TownSwatch.Pearl);
            foreach (float side in new[] { -1f, 1f })
            {
                b.Box(new Vector3(side * 1.6f, 1.55f, 0f) * s, new Vector3(0.36f, 2.7f, 0.36f) * s, TownSwatch.Lacquer);
                b.Box(new Vector3(side * 1.6f, 0.35f, 0f) * s, new Vector3(0.5f, 0.3f, 0.5f) * s, TownSwatch.Black);
            }

            b.Box(new Vector3(0f, 2.75f, 0f) * s, new Vector3(3.8f, 0.3f, 0.45f) * s, TownSwatch.Lacquer);
            b.Box(new Vector3(0f, 3.05f, 0f) * s, new Vector3(4.4f, 0.22f, 1.5f) * s, TownSwatch.ClayTile, 0f, TownSwatch.ClayTile);
            b.Box(new Vector3(0f, 3.25f, 0f) * s, new Vector3(3.6f, 0.2f, 0.9f) * s, TownSwatch.ClayTile);
            if ((variant & 1) == 0)
            {
                b.Box(new Vector3(0f, 2.35f, 0.24f) * s, new Vector3(0.9f, 0.4f, 0.05f) * s, TownSwatch.Gold);
            }
        }

        /// <summary>A fumarole: a low cone of rock crusted yellow, a column of steam over its mouth.</summary>
        public static void Fumarole(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.down * 0.2f * s, 1.5f * s, 0.45f * s, 1.3f * s, 7, TownSwatch.StoneDark, TownSwatch.ClothYellow, variant * 0.4f);
            b.Blob(new Vector3(0.6f, 0.3f, 0.4f) * s, new Vector3(0.7f, 0.35f, 0.6f) * s, TownSwatch.ClothYellow);
            b.Blob(new Vector3(0f, 1.9f, 0f) * s, new Vector3(0.9f, 0.9f, 0.9f) * s, TownSwatch.GoodSmoke);
            b.Blob(new Vector3(0.2f, 2.9f, 0.1f) * s, new Vector3(1.3f, 1f, 1.2f) * s, TownSwatch.CloudWhite);
        }

        /// <summary>Shards of black obsidian leaning out of the ground.</summary>
        public static void ObsidianShard(TownMeshBuilder b, float s, int variant)
        {
            for (int i = 0; i < 3; i++)
            {
                float a = (i / 3f + variant * 0.11f) * Mathf.PI * 2f;
                float h = 1.6f + 0.6f * ((i + variant) % 3);
                var foot = new Vector3(Mathf.Cos(a) * 0.35f, -0.2f, Mathf.Sin(a) * 0.35f) * s;
                var tip = foot + new Vector3(Mathf.Cos(a) * 0.4f, h, Mathf.Sin(a) * 0.4f) * s;
                b.Log(foot, tip, 0.28f * s, (i & 1) == 0 ? TownSwatch.Black : TownSwatch.Indigo, 3);
            }

            b.Blob(new Vector3(0f, 0.05f, 0f) * s, new Vector3(1.4f, 0.3f, 1.2f) * s, TownSwatch.HollowDark);
        }

        /// <summary>Ember coral: branches of coral glowing orange on a black foot.</summary>
        public static void EmberCoral(TownMeshBuilder b, float s, int variant)
        {
            b.Blob(new Vector3(0f, 0.1f, 0f) * s, new Vector3(1.2f, 0.4f, 1.1f) * s, TownSwatch.Black);
            for (int i = 0; i < 5; i++)
            {
                float a = (i / 5f + variant * 0.07f) * Mathf.PI * 2f;
                var foot = new Vector3(Mathf.Cos(a) * 0.2f, 0.2f, Mathf.Sin(a) * 0.2f) * s;
                var tip = foot + new Vector3(Mathf.Cos(a) * 0.5f, 0.9f + 0.3f * (i % 2), Mathf.Sin(a) * 0.5f) * s;
                b.Log(foot, tip, 0.1f * s, (i & 1) == 0 ? TownSwatch.Ember : TownSwatch.CoralLive, 4);
            }
        }

        /// <summary>A bush of yellow broom in flower: a round green mound dotted with yellow.</summary>
        public static void BroomBush(TownMeshBuilder b, float s, int variant)
        {
            b.Blob(new Vector3(0f, 0.55f, 0f) * s, new Vector3(1.8f, 1.1f, 1.6f) * s, TownSwatch.Larch);
            b.Blob(new Vector3(0.2f, 0.95f, 0.1f) * s, new Vector3(1.4f, 0.6f, 1.3f) * s, (variant & 1) == 0 ? TownSwatch.GroveFlower : TownSwatch.ClothYellow);
            b.Blob(new Vector3(-0.6f, 0.6f, -0.4f) * s, new Vector3(0.6f, 0.4f, 0.6f) * s, TownSwatch.GroveFlower);
        }

        /// <summary>A stone rose: a rosette of weathered rock leaves round a centre.</summary>
        public static void StoneRose(TownMeshBuilder b, float s, int variant)
        {
            for (int i = 0; i < 6; i++)
            {
                float yaw = i * 60f + variant * 9f;
                float a = yaw * Mathf.Deg2Rad;
                b.Box(new Vector3(Mathf.Cos(a) * 0.55f, 0.35f, Mathf.Sin(a) * 0.55f) * s, new Vector3(0.9f, 0.7f, 0.2f) * s, (i & 1) == 0 ? TownSwatch.Sandstone : TownSwatch.Laterite, -yaw + 90f);
            }

            b.Blob(new Vector3(0f, 0.55f, 0f) * s, new Vector3(0.6f, 0.6f, 0.6f) * s, TownSwatch.Laterite);
        }

        /// <summary>A canary pine: a tall red-brown trunk, layered drooping crowns of long needles.</summary>
        public static void CanaryPine(TownMeshBuilder b, float s, int variant)
        {
            float h = 6f + (variant & 3) * 0.6f;
            b.Log(new Vector3(0f, -0.3f, 0f) * s, new Vector3(0f, h, 0f) * s, 0.24f * s, TownSwatch.RootBark, 5);
            b.Cylinder(new Vector3(0f, h * 0.45f, 0f) * s, 1.8f * s, 1.2f * s, 1.4f * s, 7, TownSwatch.Pine, null, variant * 0.3f);
            b.Cylinder(new Vector3(0f, h * 0.68f, 0f) * s, 1.5f * s, 0.9f * s, 1.3f * s, 7, TownSwatch.Larch, null, variant * 0.5f);
            b.Cylinder(new Vector3(0f, h * 0.9f, 0f) * s, 1.1f * s, 0.2f * s, 1.4f * s, 6, TownSwatch.Pine);
        }

        /// <summary>A furnace tower: a round chimney of black stone, an iron band, a glowing mouth and a red cap.</summary>
        public static void FurnaceTower(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.down * 0.3f * s, 1.9f * s, 1.5f * s, 2.4f * s, 8, TownSwatch.Basalt, TownSwatch.StoneDark);
            b.Cylinder(new Vector3(0f, 2.1f, 0f) * s, 1.5f * s, 1f * s, 3.4f * s, 8, TownSwatch.StoneDark);
            b.Cylinder(new Vector3(0f, 2.3f, 0f) * s, 1.52f * s, 1.48f * s, 0.25f * s, 8, TownSwatch.Iron);
            b.Box(new Vector3(0f, 0.75f, -1.68f) * s, new Vector3(0.9f, 0.9f, 0.1f) * s, TownSwatch.Ember);
            b.Cylinder(new Vector3(0f, 5.5f, 0f) * s, 1.15f * s, 1.15f * s, 0.25f * s, 8, (variant & 1) == 0 ? TownSwatch.Lacquer : TownSwatch.ClayTile);
            b.Disc(new Vector3(0f, 5.76f, 0f) * s, 0.7f * s, 8, TownSwatch.Ember);
        }

        /// <summary>A heap of slag and cinder, a few embers in it.</summary>
        public static void SlagHeap(TownMeshBuilder b, float s, int variant)
        {
            b.Crown(new Vector3(0f, 0.2f, 0f) * s, new Vector3(2.8f, 1.4f, 2.4f) * s, (variant & 1) == 0 ? TownSwatch.PotIron : TownSwatch.ColdEmber, variant * 31f);
            b.Blob(new Vector3(1.1f, 0.2f, 0.6f) * s, new Vector3(1f, 0.5f, 0.8f) * s, TownSwatch.Black);
            b.Blob(new Vector3(-0.4f, 1.05f, -0.2f) * s, new Vector3(0.3f, 0.15f, 0.3f) * s, TownSwatch.Ember);
        }

        /// <summary>A hot pool: a disc of steaming blue water with a rim of sinter (ground detail).</summary>
        public static void HotPool(TownMeshBuilder b, float s)
        {
            b.Disc(new Vector3(0f, 0.03f, 0f) * s, 0.9f * s, 7, TownSwatch.ReefTeal);
            b.Ring(new Vector3(0f, 0.05f, 0f) * s, 0.9f * s, 1.3f * s, 7, TownSwatch.LimeWhite, TownSwatch.ClothYellow);
        }

        /// <summary>Raked gravel: three pale curved lines (ground detail).</summary>
        public static void RakedGravel(TownMeshBuilder b, float s)
        {
            for (int i = 0; i < 3; i++)
            {
                float z = -0.5f + i * 0.5f;
                b.Quad(new Vector3(-1.6f, 0.03f, z - 0.08f) * s, new Vector3(-1.6f, 0.03f, z + 0.08f) * s, new Vector3(1.6f, 0.03f, z + 0.08f) * s, new Vector3(1.6f, 0.03f, z - 0.08f) * s, TownSwatch.Plaster);
            }
        }

        /// <summary>A drift of grey ash: two low lumps (ground detail).</summary>
        public static void AshDrift(TownMeshBuilder b, float s)
        {
            b.Blob(new Vector3(0f, 0.04f, 0f) * s, new Vector3(1.6f, 0.15f, 1.1f) * s, TownSwatch.GoodSmoke);
            b.Blob(new Vector3(0.8f, 0.03f, 0.5f) * s, new Vector3(0.8f, 0.1f, 0.6f) * s, TownSwatch.Granite);
        }

        /// <summary>A crack of glowing lava kinked in two (ground detail).</summary>
        public static void LavaCrack(TownMeshBuilder b, float s)
        {
            b.Quad(new Vector3(-1.5f, 0.03f, -0.05f) * s, new Vector3(-1.5f, 0.03f, 0.06f) * s, new Vector3(0f, 0.03f, 0.32f) * s, new Vector3(0f, 0.03f, 0.16f) * s, TownSwatch.Ember);
            b.Quad(new Vector3(0f, 0.03f, 0.16f) * s, new Vector3(0f, 0.03f, 0.32f) * s, new Vector3(1.6f, 0.03f, -0.08f) * s, new Vector3(1.6f, 0.03f, -0.18f) * s, TownSwatch.Ember);
        }
    }
}
