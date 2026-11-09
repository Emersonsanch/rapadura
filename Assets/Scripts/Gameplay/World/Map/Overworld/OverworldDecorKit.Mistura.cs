using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 14 — Margens da Mistura: the pieces of the Margins (low-poly, the shared palette; each built round the origin and placed by the
    /// chunk builder): glazed lions, brick kilns and stacks, the fallen gnomons and hourglass pillars of the Desert of the Clock, bitumen jars,
    /// the resin lanterns and giant cedars of Hobabish, diorite shards, the dream's standing tablets and felled trunks; and the ground detail
    /// stamped by the hundred (brick shards, dial marks, bubbles of pitch, resin drops, diorite and tablet chips).
    /// </summary>
    public static partial class OverworldDecorKit
    {
        /// <summary>A Mistura piece (Região 14, 300–319).</summary>
        public static bool IsMisturaDecor(OverworldDecor decor)
        {
            int i = (int)decor;
            return i >= 300 && i < 320;
        }

        public static readonly TownStamp BrickShardStamp = TownMeshBuilder.Record(b => BrickShard(b, 1f));
        public static readonly TownStamp ClockRippleStamp = TownMeshBuilder.Record(b => ClockRipple(b, 1f));
        public static readonly TownStamp TarBubbleStamp = TownMeshBuilder.Record(b => TarBubble(b, 1f));
        public static readonly TownStamp ResinDripStamp = TownMeshBuilder.Record(b => ResinDrip(b, 1f));
        public static readonly TownStamp DioriteChipStamp = TownMeshBuilder.Record(b => DioriteChip(b, 1f));
        public static readonly TownStamp TabletChipStamp = TownMeshBuilder.Record(b => TabletChip(b, 1f));

        /// <summary>A lion of glazed blue brick, seated on a low plinth.</summary>
        public static void GlazedLionStatue(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.3f, 0f) * s, new Vector3(1.8f, 0.6f, 1f) * s, TownSwatch.BakedBrick, variant * 11f);
            b.Blob(new Vector3(-0.1f, 1.05f, 0f) * s, new Vector3(1.3f, 0.9f, 0.7f) * s, TownSwatch.GlazedBlue);
            b.Blob(new Vector3(0.65f, 1.45f, 0f) * s, new Vector3(0.65f, 0.6f, 0.6f) * s, TownSwatch.GlazedBlue);
            b.Blob(new Vector3(0.72f, 1.5f, 0f) * s, new Vector3(0.75f, 0.72f, 0.5f) * s, TownSwatch.ElamGold);
            b.Log(new Vector3(-0.8f, 0.9f, 0f) * s, new Vector3(-1.1f, 1.5f, 0f) * s, 0.09f * s, TownSwatch.GlazedBlue, 4);
        }

        /// <summary>A round kiln of baked brick with a glowing mouth.</summary>
        public static void BrickKiln(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 1.5f * s, 1.05f * s, 2.1f * s, 8, TownSwatch.BakedBrick, TownSwatch.DirtDark, variant * 0.3f);
            b.Box(new Vector3(0f, 0.55f, -1.32f) * s, new Vector3(0.8f, 0.9f, 0.2f) * s, TownSwatch.Ember);
            b.Cylinder(new Vector3(0f, 2.1f, 0f) * s, 0.5f * s, 0.32f * s, 1.1f * s, 6, TownSwatch.Brick, TownSwatch.Bitumen);
        }

        /// <summary>A stack of mud bricks left to dry.</summary>
        public static void BrickStack(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.28f, 0f) * s, new Vector3(1.6f, 0.56f, 1f) * s, TownSwatch.BakedBrick, variant * 7f);
            b.Box(new Vector3(0.1f, 0.78f, 0f) * s, new Vector3(1.3f, 0.44f, 0.86f) * s, TownSwatch.AdobeOchre, variant * 17f);
            if ((variant & 1) == 0)
            {
                b.Box(new Vector3(-0.1f, 1.15f, 0f) * s, new Vector3(0.9f, 0.3f, 0.6f) * s, TownSwatch.Brick, variant * 31f);
            }
        }

        /// <summary>One of the twelve great gnomons, lying on the sand.</summary>
        public static void FallenGnomon(TownMeshBuilder b, float s, int variant)
        {
            var from = new Vector3(-3.2f, 0.45f, 0f) * s;
            var to = new Vector3(3.4f, 0.75f, 0.15f) * s;
            b.Log(from, to, 0.7f * s, TownSwatch.Stone, 6);
            b.Cylinder(new Vector3(3.4f, 0.1f, 0.15f) * s, 0.8f * s, 0.15f * s, 1.5f * s, 4, TownSwatch.ElamGold, null, 0.78f);
            b.Blob(new Vector3(-3.4f, 0.4f, 0f) * s, new Vector3(1.6f, 0.9f, 1.4f) * s, TownSwatch.Sandstone);
            b.Box(new Vector3(0.5f, 0.05f, 1.6f) * s, new Vector3(3.6f, 0.08f, 0.4f) * s, (variant & 1) == 0 ? TownSwatch.Violet : TownSwatch.Indigo);
        }

        /// <summary>A stone pillar in the shape of an hourglass.</summary>
        public static void HourglassPillar(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 0.9f * s, 0.25f * s, 1.5f * s, 8, TownSwatch.Stone, null, variant * 0.3f);
            b.Cylinder(new Vector3(0f, 1.5f, 0f) * s, 0.25f * s, 0.9f * s, 1.5f * s, 8, TownSwatch.Stone, TownSwatch.ElamGold, variant * 0.3f);
            b.Cylinder(new Vector3(0f, 1.1f, 0f) * s, 0.42f * s, 0.2f * s, 0.75f * s, 6, TownSwatch.DustSky);
        }

        /// <summary>A sealed jar of bitumen, a band of reed round its belly.</summary>
        public static void BitumenJar(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 0.55f * s, 0.85f * s, 0.75f * s, 7, TownSwatch.AdobeOchre, null, variant * 0.5f);
            b.Cylinder(new Vector3(0f, 0.75f, 0f) * s, 0.85f * s, 0.32f * s, 0.75f * s, 7, TownSwatch.AdobeOchre, TownSwatch.Bitumen, variant * 0.5f);
            b.Ring(new Vector3(0f, 0.72f, 0f) * s, 0.84f * s, 0.92f * s, 7, TownSwatch.MarshReed, TownSwatch.Reed);
        }

        /// <summary>A lantern of cedar resin on a short post.</summary>
        public static void ResinLantern(TownMeshBuilder b, float s, int variant)
        {
            b.Log(Vector3.zero, new Vector3(0f, 2.1f, 0f) * s, 0.13f * s, TownSwatch.Trunk, 5);
            b.Cylinder(new Vector3(0f, 2.05f, 0f) * s, 0.42f * s, 0.34f * s, 0.65f * s, 6, TownSwatch.ZigguratFlame, TownSwatch.ElamGold, variant * 0.4f);
            b.Cylinder(new Vector3(0f, 2.7f, 0f) * s, 0.52f * s, 0.06f * s, 0.4f * s, 6, TownSwatch.Pine);
        }

        /// <summary>A giant cedar: a thick trunk and three tiers of flat crowns.</summary>
        public static void GiantCedar(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 0.75f * s, 0.42f * s, 6.8f * s, 6, TownSwatch.Trunk, null, variant * 0.7f);
            b.Blob(new Vector3(0f, 7f, 0f) * s, new Vector3(4.8f, 1.1f, 4.4f) * s, TownSwatch.Pine);
            b.Blob(new Vector3(0.3f, 8.6f, 0.1f) * s, new Vector3(3.6f, 0.95f, 3.3f) * s, (variant & 1) == 0 ? TownSwatch.Pine : TownSwatch.LeafDark);
            b.Blob(new Vector3(-0.2f, 9.9f, -0.1f) * s, new Vector3(2.2f, 0.8f, 2f) * s, TownSwatch.LeafDark);
            b.Blob(new Vector3(0f, 0.2f, 0f) * s, new Vector3(2.2f, 0.45f, 2f) * s, TownSwatch.Trunk);
        }

        /// <summary>A shard of dark diorite, its crest pale.</summary>
        public static void DioriteShard(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 1.2f * s, 0.15f * s, (2.6f + 0.4f * (variant & 3)) * s, 5, TownSwatch.Basalt, TownSwatch.Granite, variant * 0.6f);
            b.Cylinder(new Vector3(0.8f, 0f, 0.3f) * s, 0.6f * s, 0.08f * s, 1.6f * s, 4, TownSwatch.StoneDark, null, variant * 0.9f);
        }

        /// <summary>A giant stone tablet standing in the ground, lines of signs on its face.</summary>
        public static void TabletStone(TownMeshBuilder b, float s, int variant)
        {
            float tilt = (variant & 3) * 0.08f;
            b.Box(new Vector3(0f, 2f, 0f) * s, new Vector3(2.4f, 4.2f, 0.5f) * s, TownSwatch.CoralDim, tilt * 40f, TownSwatch.Stone);
            for (int i = 0; i < 4; i++)
            {
                b.Box(new Vector3(0f, 1f + i * 0.85f, -0.27f) * s, new Vector3(1.8f, 0.14f, 0.03f) * s, TownSwatch.ColdWater);
            }
        }

        /// <summary>A felled trunk from the dream's garden: a log with its stump behind it.</summary>
        public static void CutTrunk(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 0.75f * s, 0.7f * s, 0.65f * s, 7, TownSwatch.GroveBark, TownSwatch.Straw, variant * 0.5f);
            b.Log(new Vector3(0.8f, 0.5f, -0.2f) * s, new Vector3(4.2f, 0.42f, -1.4f) * s, 0.48f * s, TownSwatch.GroveBark, 7, TownSwatch.Straw);
            b.Log(new Vector3(2.6f, 0.75f, -0.9f) * s, new Vector3(3.1f, 1.7f, -1.5f) * s, 0.08f * s, TownSwatch.LeafDark, 4);
        }

        /// <summary>A broken baked brick (ground detail).</summary>
        public static void BrickShard(TownMeshBuilder b, float s)
        {
            b.Quad(new Vector3(-0.4f, 0.03f, -0.22f) * s, new Vector3(-0.4f, 0.03f, 0.22f) * s, new Vector3(0.45f, 0.03f, 0.28f) * s, new Vector3(0.4f, 0.03f, -0.18f) * s, TownSwatch.BakedBrick);
        }

        /// <summary>A pale line of the hour dial marked on the sand (ground detail).</summary>
        public static void ClockRipple(TownMeshBuilder b, float s)
        {
            b.Quad(new Vector3(-1.8f, 0.03f, -0.06f) * s, new Vector3(-1.8f, 0.03f, 0.06f) * s, new Vector3(1.8f, 0.03f, 0.06f) * s, new Vector3(1.8f, 0.03f, -0.06f) * s, TownSwatch.DustSky);
            b.Quad(new Vector3(-0.9f, 0.03f, 0.3f) * s, new Vector3(-0.9f, 0.03f, 0.42f) * s, new Vector3(0.9f, 0.03f, 0.42f) * s, new Vector3(0.9f, 0.03f, 0.3f) * s, TownSwatch.Violet);
        }

        /// <summary>A bubble of pitch ringed with dark mud (ground detail).</summary>
        public static void TarBubble(TownMeshBuilder b, float s)
        {
            b.Disc(new Vector3(0f, 0.03f, 0f) * s, 0.55f * s, 7, TownSwatch.Black);
            b.Blob(new Vector3(0f, 0.1f, 0f) * s, new Vector3(0.32f, 0.16f, 0.32f) * s, TownSwatch.Bitumen);
        }

        /// <summary>A drop of glowing resin on a chip of bark (ground detail).</summary>
        public static void ResinDrip(TownMeshBuilder b, float s)
        {
            b.Disc(new Vector3(0f, 0.03f, 0f) * s, 0.42f * s, 6, TownSwatch.Trunk);
            b.Blob(new Vector3(0f, 0.11f, 0f) * s, new Vector3(0.22f, 0.16f, 0.22f) * s, TownSwatch.ZigguratFlame);
        }

        /// <summary>A chip of diorite (ground detail).</summary>
        public static void DioriteChip(TownMeshBuilder b, float s)
        {
            b.Quad(new Vector3(-0.5f, 0.03f, -0.2f) * s, new Vector3(-0.3f, 0.03f, 0.3f) * s, new Vector3(0.5f, 0.03f, 0.22f) * s, new Vector3(0.4f, 0.03f, -0.26f) * s, TownSwatch.Basalt);
        }

        /// <summary>A chip of inscribed tablet (ground detail).</summary>
        public static void TabletChip(TownMeshBuilder b, float s)
        {
            b.Quad(new Vector3(-0.5f, 0.03f, -0.3f) * s, new Vector3(-0.5f, 0.03f, 0.3f) * s, new Vector3(0.5f, 0.03f, 0.3f) * s, new Vector3(0.5f, 0.03f, -0.3f) * s, TownSwatch.CoralDim);
            b.Quad(new Vector3(-0.3f, 0.04f, -0.1f) * s, new Vector3(-0.3f, 0.04f, 0.1f) * s, new Vector3(0.3f, 0.04f, 0.1f) * s, new Vector3(0.3f, 0.04f, -0.1f) * s, TownSwatch.ColdWater);
        }
    }
}
