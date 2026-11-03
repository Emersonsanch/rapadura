using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The five guardians of the Ossuário dos Artífices (Docs/Dungeons.md), low-poly from the shared palette like the other
    /// recipes (copper/ivory bones, soul-fire eyes; the swatches of the palette's 7th row). Pivot at the feet (y = 0), facing +Z.
    /// They spawn through the dungeon's <c>DungeonDirector</c> → arena pool → <see cref="EnemyVisualSwapper"/>, which picks these
    /// models by asset id (<see cref="ProceduralEnemyModels.KindsById"/>); <c>DungeonEnemyBehaviours</c> finds the parts it moves
    /// (the warrior's shield arm) through <see cref="ProceduralEnemyRig.GetPart"/>.
    /// <list type="bullet">
    /// <item><b>Caveira Flutuante</b> (~0.75 m): an ivory skull with copper rivets, soul-fire sockets and a trail of green
    /// wisps. <c>Body</c> (the skull, bobs) → <c>Jaw</c> (chatters, bites), <c>Wisp</c> (flickers).</item>
    /// <item><b>Guerreiro Esqueleto de Cobre</b> (~1.9 m): a skeleton whose bones are old green-patinated copper, crested helm,
    /// round bone shield on the left forearm, copper sword. <c>Body</c> (pelvis) → <c>Head</c> → <c>Jaw</c>;
    /// <c>Arm_L</c> (the shield arm the guard behaviour raises), <c>Arm_R</c> → <c>Weapon</c>; jointed legs L/R.</item>
    /// <item><b>Arqueiro Esqueleto do Fosso</b> (~1.85 m): an ivory skeleton in a tattered hood with a quiver of rune arrows,
    /// a recurved longbow in the left hand. Same skeleton; <c>Weapon</c> (the bow) under <c>Arm_L</c>.</item>
    /// <item><b>Feiticeiro das Ossadas</b> (~2.2 m): a hunched skeleton in a ragged burial robe with a copper circlet and a bone
    /// staff topped by a claw holding a violet orb. <c>Arm_R</c> → <c>Weapon</c> (staff) → <c>Orb</c> (pulses).</item>
    /// <item><b>O Colosso de Crânios</b> (chefe, ~3.4 m): a giant built of stacked skulls round a soul-fire <c>Core</c>, a crown
    /// of small skulls, a femur club, rib-cage doors <c>Ribs_L</c>/<c>Ribs_R</c> over the chest and a <c>Halo</c> of loose
    /// skulls (<c>HaloSkull_0..5</c>) that is folded away (scale ~0) until the skull-throwing phase. The rig shows the phases
    /// of <c>DungeonRules.ColossusPhaseFor</c>: halo out from 70%, rib cage open and core blazing below 30%.</item>
    /// </list>
    /// </summary>
    public static class OssuaryEnemyRecipes
    {
        /// <summary>Rest scale of the colossus' skull halo: folded away until its second phase (the rig unfolds it).</summary>
        public const float HaloFoldedScale = 0.01f;

        /// <summary>Skulls in the colossus' halo.</summary>
        public const int HaloSkulls = 6;

        // ------------------------------------------------------------------
        // Shared: skulls and skeletons
        // ------------------------------------------------------------------

        /// <summary>
        /// A skull without its lower jaw, centred on <paramref name="center"/>, <paramref name="size"/> m across (cranium,
        /// face, dark sockets with a glowing spark inside, nose hole, upper teeth). Faces +Z after <paramref name="rotation"/>.
        /// </summary>
        public static void DrawSkull(EnemyMeshBuilder b, Vector3 center, float size, EnemySwatch bone, EnemySwatch eye,
            Quaternion? rotation = null, bool coarse = true, bool withJaw = false)
        {
            Matrix4x4 saved = b.Matrix;
            b.Matrix = saved * Matrix4x4.TRS(center, rotation ?? Quaternion.identity, Vector3.one * size);
            b.Blob(new Vector3(0f, 0.08f, -0.03f), new Vector3(0.9f, 0.86f, 1f), bone, coarse: coarse);
            b.Box(new Vector3(0f, -0.2f, 0.2f), new Vector3(0.6f, 0.32f, 0.44f), bone);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.17f * s, -0.03f, 0.42f), new Vector3(0.21f, 0.18f, 0.06f), EnemySwatch.Black);
                b.Box(new Vector3(0.17f * s, -0.03f, 0.45f), new Vector3(0.1f, 0.09f, 0.03f), eye);
            }

            b.Box(new Vector3(0f, -0.18f, 0.43f), new Vector3(0.08f, 0.1f, 0.04f), EnemySwatch.Black);
            b.Box(new Vector3(0f, -0.33f, 0.37f), new Vector3(0.42f, 0.06f, 0.07f), EnemySwatch.Fang);
            if (withJaw)
            {
                DrawJaw(b, new Vector3(0f, -0.34f, 0f), 1f, bone);
            }

            b.Matrix = saved;
        }

        /// <summary>A lower jaw hanging from its hinge at <paramref name="hinge"/> (skull of <paramref name="size"/> m).</summary>
        public static void DrawJaw(EnemyMeshBuilder b, Vector3 hinge, float size, EnemySwatch bone)
        {
            Matrix4x4 saved = b.Matrix;
            b.Matrix = saved * Matrix4x4.TRS(hinge, Quaternion.identity, Vector3.one * size);
            b.Box(new Vector3(0f, -0.08f, 0.2f), new Vector3(0.5f, 0.1f, 0.42f), bone);
            b.Box(new Vector3(0f, -0.01f, 0.36f), new Vector3(0.38f, 0.05f, 0.06f), EnemySwatch.Fang);
            b.Matrix = saved;
        }

        /// <summary>How a humanoid skeleton is dressed.</summary>
        private struct SkeletonLook
        {
            public EnemySwatch Bone;
            public EnemySwatch Joint;
            public EnemySwatch Eye;
            /// <summary>Uniform scale of the whole body.</summary>
            public float Scale;
            /// <summary>Forward lean of the spine (deg): the sorcerer hunches.</summary>
            public float Hunch;
        }

        private const float SkeletonHip = 0.95f;

        /// <summary>
        /// Pelvis-rooted humanoid skeleton: <c>Body</c> (pelvis pivot, spine + rib cage + collarbones) → <c>Head</c> (skull) →
        /// <c>Jaw</c>; <c>Arm_L</c>/<c>Arm_R</c> (shoulder pivots, arm bones, hand); <c>Leg_*</c> → <c>Shin_*</c> → <c>Foot_*</c>.
        /// <paramref name="dressBody"/>/<paramref name="dressHead"/> add the costume to those meshes. Returns the part indices.
        /// </summary>
        private static (int body, int head, int armLeft, int armRight) BuildSkeleton(ProceduralEnemyBlueprint bp, SkeletonLook look,
            System.Action<EnemyMeshBuilder> dressBody, System.Action<EnemyMeshBuilder> dressHead)
        {
            float k = look.Scale;
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, SkeletonHip * k, 0f), b =>
            {
                b.Matrix = Matrix4x4.Scale(Vector3.one * k);
                DrawSkeletonTorso(b, look);
                b.Matrix = Matrix4x4.Scale(Vector3.one * k);
                dressBody?.Invoke(b);
                b.Matrix = Matrix4x4.identity;
            });

            Quaternion lean = Quaternion.Euler(look.Hunch, 0f, 0f);
            Vector3 neck = lean * new Vector3(0f, 0.7f, 0.02f) * k;
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, neck, b =>
            {
                b.Matrix = Matrix4x4.Scale(Vector3.one * k);
                b.Segment(new Vector3(0f, -0.06f, -0.02f), new Vector3(0f, 0.06f, 0f), 0.035f, 0.03f, 4, look.Joint);
                DrawSkull(b, new Vector3(0f, 0.17f, 0.01f), 0.27f, look.Bone, look.Eye, coarse: false);
                b.Matrix = Matrix4x4.Scale(Vector3.one * k);
                dressHead?.Invoke(b);
                b.Matrix = Matrix4x4.identity;
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, (0.17f - 0.34f * 0.27f) * k, 0.01f * k),
                b => DrawJaw(b, Vector3.zero, 0.27f * k, look.Bone));

            int armLeft = -1, armRight = -1;
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                Vector3 shoulder = lean * new Vector3(0.24f * side, 0.6f, 0f) * k;
                int arm = bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, shoulder,
                    b => DrawSkeletonArm(b, side, look), Quaternion.Euler(-look.Hunch * 0.5f, 0f, 4f * side),
                    meshKey: right ? "SkeletonArm_R" : "SkeletonArm_L");
                if (right)
                {
                    armRight = arm;
                }
                else
                {
                    armLeft = arm;
                }

                int leg = bp.Mesh(right ? ProceduralEnemyParts.LegRight : ProceduralEnemyParts.LegLeft, -1,
                    new Vector3(0.11f * side * k, SkeletonHip * k, 0f), b => DrawSkeletonThigh(b, look), meshKey: "SkeletonThigh");
                int shin = bp.Mesh(right ? ProceduralEnemyParts.ShinRight : ProceduralEnemyParts.ShinLeft, leg,
                    new Vector3(0f, -0.46f * k, 0.03f * k), b => DrawSkeletonShin(b, look), meshKey: "SkeletonShin");
                bp.Mesh(right ? ProceduralEnemyParts.FootRight : ProceduralEnemyParts.FootLeft, shin,
                    new Vector3(0f, -0.43f * k, -0.03f * k), b => DrawSkeletonFoot(b, look), meshKey: "SkeletonFoot");
            }

            return (body, head, armLeft, armRight);
        }

        private static void DrawSkeletonTorso(EnemyMeshBuilder b, SkeletonLook look)
        {
            Matrix4x4 scale = b.Matrix;

            // Pelvis bowl and hip balls.
            b.Blob(new Vector3(0f, 0.02f, -0.01f), new Vector3(0.3f, 0.16f, 0.18f), look.Bone, coarse: true);
            b.Box(new Vector3(0f, -0.04f, 0.05f), new Vector3(0.12f, 0.08f, 0.06f), look.Joint);

            // Spine and rib cage, leaning with the hunch.
            b.Matrix = scale * Matrix4x4.Rotate(Quaternion.Euler(look.Hunch, 0f, 0f));
            b.Segment(new Vector3(0f, 0.04f, -0.06f), new Vector3(0f, 0.68f, -0.05f), 0.035f, 0.03f, 4, look.Joint);
            for (int i = 0; i < 4; i++)
            {
                float y = 0.5f - i * 0.085f;
                float w = 0.17f - i * 0.012f;
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Segment(new Vector3(0f, y, -0.06f), new Vector3(w * s, y - 0.02f, 0.0f), 0.017f, 0.017f, 4, look.Bone);
                    b.Segment(new Vector3(w * s, y - 0.02f, 0.0f), new Vector3(0.05f * s, y - 0.06f, 0.1f), 0.017f, 0.014f, 4, look.Bone);
                }
            }

            b.Box(new Vector3(0f, 0.42f, 0.1f), new Vector3(0.05f, 0.22f, 0.03f), look.Bone, Quaternion.Euler(-8f, 0f, 0f));
            // Collarbones / shoulder girdle.
            b.Box(new Vector3(0f, 0.6f, -0.02f), new Vector3(0.48f, 0.045f, 0.06f), look.Bone);
            b.Matrix = scale;
        }

        private static void DrawSkeletonArm(EnemyMeshBuilder b, float side, SkeletonLook look)
        {
            float k = look.Scale;
            b.Matrix = Matrix4x4.Scale(new Vector3(side * k, k, k));
            b.Blob(Vector3.zero, new Vector3(0.09f, 0.09f, 0.09f), look.Bone, coarse: true);
            b.Segment(new Vector3(0f, -0.02f, 0f), new Vector3(0.02f, -0.29f, 0f), 0.028f, 0.022f, 4, look.Bone);
            b.Blob(new Vector3(0.02f, -0.31f, 0f), new Vector3(0.065f, 0.065f, 0.065f), look.Joint, coarse: true);
            b.Segment(new Vector3(0.02f, -0.32f, 0.01f), new Vector3(0.035f, -0.55f, 0.06f), 0.02f, 0.017f, 4, look.Bone);
            b.Segment(new Vector3(0.0f, -0.32f, 0.0f), new Vector3(0.015f, -0.55f, 0.05f), 0.014f, 0.012f, 3, look.Joint);
            // Bony hand: palm and three fingers.
            b.Box(new Vector3(0.035f, -0.6f, 0.07f), new Vector3(0.05f, 0.07f, 0.06f), look.Bone);
            for (int f = -1; f <= 1; f++)
            {
                b.Segment(new Vector3(0.035f + 0.015f * f, -0.63f, 0.08f), new Vector3(0.04f + 0.02f * f, -0.7f, 0.1f), 0.008f, 0.005f, 3, look.Bone);
            }

            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawSkeletonThigh(EnemyMeshBuilder b, SkeletonLook look)
        {
            float k = look.Scale;
            b.Matrix = Matrix4x4.Scale(Vector3.one * k);
            b.Blob(Vector3.zero, new Vector3(0.08f, 0.08f, 0.08f), look.Joint, coarse: true);
            b.Segment(new Vector3(0f, -0.02f, 0f), new Vector3(0f, -0.44f, 0.03f), 0.034f, 0.028f, 4, look.Bone);
            b.Blob(new Vector3(0f, -0.46f, 0.04f), new Vector3(0.08f, 0.07f, 0.07f), look.Bone, coarse: true);
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawSkeletonShin(EnemyMeshBuilder b, SkeletonLook look)
        {
            float k = look.Scale;
            b.Matrix = Matrix4x4.Scale(Vector3.one * k);
            b.Segment(new Vector3(0f, -0.02f, 0f), new Vector3(0f, -0.42f, -0.03f), 0.028f, 0.022f, 4, look.Bone);
            b.Segment(new Vector3(0.025f, -0.03f, -0.01f), new Vector3(0.022f, -0.4f, -0.03f), 0.012f, 0.01f, 3, look.Joint);
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawSkeletonFoot(EnemyMeshBuilder b, SkeletonLook look)
        {
            float k = look.Scale;
            b.Matrix = Matrix4x4.Scale(Vector3.one * k);
            b.Blob(new Vector3(0f, 0f, 0f), new Vector3(0.06f, 0.06f, 0.06f), look.Joint, coarse: true);
            b.Box(new Vector3(0f, -0.03f, 0.06f), new Vector3(0.09f, 0.04f, 0.2f), look.Bone);
            for (int t = -1; t <= 1; t++)
            {
                b.Box(new Vector3(0.028f * t, -0.035f, 0.18f), new Vector3(0.018f, 0.03f, 0.06f), look.Joint);
            }

            b.Matrix = Matrix4x4.identity;
        }

        // ------------------------------------------------------------------
        // Caveira Flutuante
        // ------------------------------------------------------------------

        public static void BuildFloatingSkull(ProceduralEnemyBlueprint bp)
        {
            const float size = 0.6f;
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.42f, 0f), DrawFloatingSkull);
            bp.Mesh(ProceduralEnemyParts.Jaw, body, new Vector3(0f, -0.34f * size, 0f), b => DrawJaw(b, Vector3.zero, size, EnemySwatch.Ivory));
            bp.Mesh(ProceduralEnemyParts.Wisp, body, new Vector3(0f, -0.1f, -0.1f), DrawSkullWisps);
        }

        private static void DrawFloatingSkull(EnemyMeshBuilder b)
        {
            DrawSkull(b, Vector3.zero, 0.6f, EnemySwatch.Ivory, EnemySwatch.SoulFire, coarse: false);
            // Copper rivets and a crack: an artificer's skull, patched long ago.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Cylinder(new Vector3(0.27f * s, 0.06f, 0f), 0.045f, 0.045f, 0.04f, 6, EnemySwatch.Copper, EnemySwatch.Verdigris,
                    Quaternion.Euler(0f, 0f, -90f * s));
            }

            b.Box(new Vector3(0f, 0.31f, 0.02f), new Vector3(0.06f, 0.03f, 0.36f), EnemySwatch.Copper);
            b.Box(new Vector3(0.09f, 0.26f, 0.14f), new Vector3(0.012f, 0.02f, 0.14f), EnemySwatch.IvoryDark, Quaternion.Euler(0f, 30f, 0f));
            // Soul-fire glimmer leaking from the top of the sockets.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Cylinder(new Vector3(0.1f * s, 0.06f, 0.26f), 0.03f, 0f, 0.09f, 4, EnemySwatch.SoulFire, rotation: Quaternion.Euler(-25f, 0f, 0f));
            }
        }

        private static void DrawSkullWisps(EnemyMeshBuilder b)
        {
            // Tongues of green soul fire streaming down and back: it has nothing below the jaw.
            b.Cylinder(new Vector3(0f, 0.06f, 0f), 0.14f, 0f, 0.42f, 5, EnemySwatch.SoulFire, rotation: Quaternion.Euler(-155f, 0f, 0f));
            b.Cylinder(new Vector3(0.1f, 0.06f, 0.04f), 0.08f, 0f, 0.3f, 4, EnemySwatch.SoulFire, rotation: Quaternion.Euler(-150f, 0f, 20f));
            b.Cylinder(new Vector3(-0.1f, 0.06f, 0.04f), 0.08f, 0f, 0.28f, 4, EnemySwatch.SoulFire, rotation: Quaternion.Euler(-150f, 0f, -22f));
            b.Cylinder(new Vector3(0f, 0.1f, -0.1f), 0.07f, 0f, 0.3f, 4, EnemySwatch.Verdigris, rotation: Quaternion.Euler(-115f, 0f, 0f));
        }

        // ------------------------------------------------------------------
        // Guerreiro Esqueleto de Cobre
        // ------------------------------------------------------------------

        public static void BuildSkeletonWarrior(ProceduralEnemyBlueprint bp)
        {
            var look = new SkeletonLook { Bone = EnemySwatch.Copper, Joint = EnemySwatch.CopperDark, Eye = EnemySwatch.SoulFire, Scale = 1f };
            var (_, _, armLeft, armRight) = BuildSkeleton(bp, look, DressWarriorBody, DressWarriorHead);
            bp.Mesh(ProceduralEnemyParts.Shield, armLeft, new Vector3(-0.07f, -0.44f, 0.08f), DrawBoneShield, Quaternion.Euler(0f, -12f, 0f));
            bp.Mesh(ProceduralEnemyParts.Weapon, armRight, new Vector3(0.035f, -0.6f, 0.07f), DrawCopperSword, Quaternion.Euler(70f, 0f, 0f));
        }

        private static void DressWarriorBody(EnemyMeshBuilder b)
        {
            // Green patina patches, a belt and a tattered loincloth.
            b.Box(new Vector3(0.08f, 0.47f, 0.08f), new Vector3(0.06f, 0.05f, 0.04f), EnemySwatch.Verdigris);
            b.Box(new Vector3(-0.1f, 0.33f, 0.06f), new Vector3(0.05f, 0.04f, 0.04f), EnemySwatch.Verdigris);
            b.Cylinder(new Vector3(0f, 0.05f, 0f), 0.17f, 0.17f, 0.06f, 8, EnemySwatch.Leather, twist: 22.5f);
            b.Box(new Vector3(0f, -0.12f, 0.12f), new Vector3(0.16f, 0.26f, 0.02f), EnemySwatch.Shroud, Quaternion.Euler(-6f, 0f, 0f));
            b.Box(new Vector3(0f, -0.12f, -0.13f), new Vector3(0.2f, 0.24f, 0.02f), EnemySwatch.Shroud, Quaternion.Euler(8f, 0f, 0f));
            // Pauldron on the sword shoulder.
            b.Blob(new Vector3(0.24f, 0.63f, 0f), new Vector3(0.17f, 0.1f, 0.17f), EnemySwatch.Copper, coarse: true);
            b.Box(new Vector3(0.24f, 0.65f, 0f), new Vector3(0.18f, 0.02f, 0.18f), EnemySwatch.Verdigris);
        }

        private static void DressWarriorHead(EnemyMeshBuilder b)
        {
            // Copper helm with cheek guards and a verdigris crest.
            b.Blob(new Vector3(0f, 0.24f, -0.01f), new Vector3(0.27f, 0.18f, 0.29f), EnemySwatch.Copper);
            b.Cylinder(new Vector3(0f, 0.19f, -0.01f), 0.14f, 0.14f, 0.03f, 8, EnemySwatch.CopperDark);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.12f * s, 0.12f, 0.05f), new Vector3(0.025f, 0.12f, 0.1f), EnemySwatch.Copper);
            }

            b.Box(new Vector3(0f, 0.36f, -0.02f), new Vector3(0.03f, 0.08f, 0.26f), EnemySwatch.Verdigris);
            b.Box(new Vector3(0f, 0.3f, -0.17f), new Vector3(0.03f, 0.14f, 0.06f), EnemySwatch.Verdigris, Quaternion.Euler(-25f, 0f, 0f));
        }

        private static void DrawBoneShield(EnemyMeshBuilder b)
        {
            // Round shield of lashed ivory ribs with a copper boss, facing forward off the forearm.
            Quaternion facing = Quaternion.Euler(90f, 0f, 0f);
            b.Cylinder(new Vector3(0f, 0f, 0.02f), 0.26f, 0.26f, 0.04f, 8, EnemySwatch.Ivory, EnemySwatch.Ivory, facing, bottomCap: true, twist: 22.5f);
            for (int i = -1; i <= 1; i++)
            {
                b.Box(new Vector3(0.12f * i, 0f, 0.065f), new Vector3(0.03f, 0.44f, 0.015f), EnemySwatch.IvoryDark);
            }

            b.Cylinder(new Vector3(0f, 0f, 0.06f), 0.08f, 0.05f, 0.05f, 6, EnemySwatch.Copper, EnemySwatch.Verdigris, facing);
            b.Cylinder(new Vector3(0f, 0f, 0.06f), 0.27f, 0.27f, 0.01f, 8, EnemySwatch.CopperDark, rotation: facing, twist: 22.5f);
        }

        private static void DrawCopperSword(EnemyMeshBuilder b)
        {
            // Grip through the fist (pivot), crossguard, a leaf blade of copper with a gold edge.
            b.Segment(new Vector3(0f, -0.1f, 0f), new Vector3(0f, 0.1f, 0f), 0.02f, 0.02f, 4, EnemySwatch.Leather);
            b.Blob(new Vector3(0f, -0.11f, 0f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.Copper, coarse: true);
            b.Box(new Vector3(0f, 0.11f, 0f), new Vector3(0.05f, 0.03f, 0.2f), EnemySwatch.CopperDark);
            b.Plate(new[]
            {
                new Vector2(-0.03f, 0.12f), new Vector2(0.03f, 0.12f), new Vector2(0.05f, 0.4f),
                new Vector2(0.035f, 0.66f), new Vector2(0f, 0.78f), new Vector2(-0.035f, 0.66f), new Vector2(-0.05f, 0.4f),
            }, 0.01f, EnemySwatch.Copper, EnemySwatch.Gold);
            b.Box(new Vector3(0f, 0.4f, 0f), new Vector3(0.024f, 0.5f, 0.012f), EnemySwatch.Verdigris);
        }

        // ------------------------------------------------------------------
        // Arqueiro Esqueleto do Fosso
        // ------------------------------------------------------------------

        public static void BuildSkeletonArcher(ProceduralEnemyBlueprint bp)
        {
            var look = new SkeletonLook { Bone = EnemySwatch.Ivory, Joint = EnemySwatch.IvoryDark, Eye = EnemySwatch.SoulFire, Scale = 0.97f, Hunch = 4f };
            var (_, _, armLeft, _) = BuildSkeleton(bp, look, DressArcherBody, DressArcherHead);
            // The bow lies along the forearm at rest (pointing ahead); raising the arm stands it upright, string toward the archer.
            bp.Mesh(ProceduralEnemyParts.Weapon, armLeft, new Vector3(-0.035f, -0.6f, 0.07f), DrawLongbow, Quaternion.Euler(90f, 0f, 0f));
        }

        private static void DressArcherBody(EnemyMeshBuilder b)
        {
            // Ragged shoulder cape and a leather quiver of rib arrows with violet rune heads.
            b.Blob(new Vector3(0f, 0.6f, -0.05f), new Vector3(0.5f, 0.14f, 0.3f), EnemySwatch.Shroud, coarse: true);
            b.Box(new Vector3(0f, 0.38f, -0.15f), new Vector3(0.36f, 0.42f, 0.02f), EnemySwatch.Shroud, Quaternion.Euler(10f, 0f, 0f));
            Quaternion tilt = Quaternion.Euler(-12f, 0f, -22f);
            b.Cylinder(new Vector3(0.06f, 0.18f, -0.2f), 0.06f, 0.07f, 0.48f, 6, EnemySwatch.Leather, EnemySwatch.BarkDark, tilt, bottomCap: true);
            for (int i = 0; i < 3; i++)
            {
                Vector3 top = new Vector3(0.06f, 0.18f, -0.2f) + tilt * new Vector3(-0.025f + 0.025f * i, 0.56f, 0.01f * i);
                b.Segment(top - tilt * Vector3.up * 0.12f, top, 0.008f, 0.008f, 3, EnemySwatch.Ivory);
                b.Box(top + tilt * Vector3.up * 0.03f, new Vector3(0.035f, 0.06f, 0.01f), EnemySwatch.Sorcery, tilt);
            }

            b.Box(new Vector3(-0.02f, 0.38f, 0.0f), new Vector3(0.03f, 0.03f, 0.42f), EnemySwatch.Leather, Quaternion.Euler(0f, 0f, 40f));
        }

        private static void DressArcherHead(EnemyMeshBuilder b)
        {
            // Burial hood drawn up over the skull, its edge framing the face.
            b.Blob(new Vector3(0f, 0.2f, -0.07f), new Vector3(0.33f, 0.34f, 0.3f), EnemySwatch.Shroud, coarse: true);
            b.Cylinder(new Vector3(0f, 0.3f, -0.12f), 0.1f, 0f, 0.16f, 4, EnemySwatch.Shroud, rotation: Quaternion.Euler(-120f, 0f, 0f));
        }

        private static void DrawLongbow(EnemyMeshBuilder b)
        {
            // Recurved ivory-and-wood longbow along local y (grip at the pivot), limbs curving back toward the string (−z).
            Vector3[] limb = { new Vector3(0f, 0.06f, 0f), new Vector3(0f, 0.3f, -0.03f), new Vector3(0f, 0.52f, -0.1f), new Vector3(0f, 0.64f, -0.19f) };
            for (int s = -1; s <= 1; s += 2)
            {
                for (int i = 0; i < limb.Length - 1; i++)
                {
                    Vector3 a = new Vector3(limb[i].x, limb[i].y * s, limb[i].z);
                    Vector3 c = new Vector3(limb[i + 1].x, limb[i + 1].y * s, limb[i + 1].z);
                    b.Segment(a, c, 0.026f - 0.005f * i, 0.021f - 0.005f * i, 4, i == 0 ? EnemySwatch.BarkDark : EnemySwatch.Ivory);
                }

                b.Box(new Vector3(0f, 0.66f * s, -0.2f), new Vector3(0.02f, 0.05f, 0.03f), EnemySwatch.Copper);
            }

            b.Box(Vector3.zero, new Vector3(0.05f, 0.14f, 0.05f), EnemySwatch.Leather);
            b.Segment(new Vector3(0f, 0.65f, -0.2f), new Vector3(0f, -0.65f, -0.2f), 0.005f, 0.005f, 3, EnemySwatch.IvoryDark);
            // A nocked rune arrow resting on the grip.
            b.Segment(new Vector3(0f, 0f, -0.2f), new Vector3(0f, 0.0f, 0.38f), 0.008f, 0.008f, 3, EnemySwatch.Ivory);
            b.Cylinder(new Vector3(0f, 0f, 0.38f), 0.025f, 0f, 0.07f, 3, EnemySwatch.Sorcery, rotation: Quaternion.Euler(90f, 0f, 0f));
        }

        // ------------------------------------------------------------------
        // Feiticeiro das Ossadas
        // ------------------------------------------------------------------

        public static void BuildBoneSorcerer(ProceduralEnemyBlueprint bp)
        {
            var look = new SkeletonLook { Bone = EnemySwatch.Ivory, Joint = EnemySwatch.IvoryDark, Eye = EnemySwatch.Sorcery, Scale = 1.06f, Hunch = 14f };
            var (_, _, _, armRight) = BuildSkeleton(bp, look, DressSorcererBody, DressSorcererHead);
            int staff = bp.Mesh(ProceduralEnemyParts.Weapon, armRight, new Vector3(0.037f, -0.64f, 0.075f), DrawBoneStaff, Quaternion.Euler(8f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Orb, staff, new Vector3(0f, 1.1f, 0f), DrawSorcererOrb);
        }

        private static void DressSorcererBody(EnemyMeshBuilder b)
        {
            // Ragged burial robe from the shoulders to just above the ground, open over the rib cage.
            b.Cylinder(new Vector3(0f, -0.72f, -0.02f), 0.36f, 0.2f, 0.78f, 8, EnemySwatch.Shroud, twist: 22.5f);
            for (int i = 0; i < 8; i++)
            {
                float a = (i + 0.5f) / 8f * Mathf.PI * 2f;
                var at = new Vector3(Mathf.Cos(a) * 0.35f, -0.72f, Mathf.Sin(a) * 0.35f - 0.02f);
                b.Cylinder(at, 0.07f, 0f, 0.12f, 3, EnemySwatch.Shroud, rotation: Quaternion.Euler(180f, -a * Mathf.Rad2Deg, 0f));
            }

            b.Cylinder(new Vector3(0f, 0.06f, -0.06f), 0.21f, 0.27f, 0.58f, 8, EnemySwatch.Shroud, rotation: Quaternion.Euler(14f, 0f, 0f), twist: 22.5f);
            b.Blob(new Vector3(0f, 0.66f, -0.02f), new Vector3(0.62f, 0.14f, 0.34f), EnemySwatch.Shroud, coarse: true);
            // A sash of small bones and a belt of glowing violet runes.
            b.Cylinder(new Vector3(0f, 0.02f, 0f), 0.22f, 0.22f, 0.05f, 8, EnemySwatch.Copper, twist: 22.5f);
            for (int i = -1; i <= 1; i++)
            {
                b.Box(new Vector3(0.08f * i, 0.045f, 0.22f), new Vector3(0.04f, 0.03f, 0.012f), EnemySwatch.Sorcery);
            }

            for (int i = 0; i < 4; i++)
            {
                b.Blob(new Vector3(-0.18f + i * 0.09f, 0.5f - Mathf.Abs(i - 1.5f) * 0.05f, 0.14f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.IvoryDark, coarse: true);
            }
        }

        private static void DressSorcererHead(EnemyMeshBuilder b)
        {
            // Deep hood with a copper circlet whose three points glow violet.
            b.Blob(new Vector3(0f, 0.2f, -0.08f), new Vector3(0.36f, 0.38f, 0.32f), EnemySwatch.Shroud, coarse: true);
            b.Cylinder(new Vector3(0f, 0.28f, -0.01f), 0.15f, 0.15f, 0.03f, 8, EnemySwatch.Copper, twist: 22.5f);
            for (int i = -1; i <= 1; i++)
            {
                Quaternion r = Quaternion.Euler(-10f, 25f * i, 0f);
                b.Cylinder(new Vector3(0.07f * i, 0.3f, 0.13f - Mathf.Abs(i) * 0.04f), 0.025f, 0f, 0.09f, 4, EnemySwatch.Copper, rotation: r);
                b.Box(new Vector3(0.07f * i, 0.3f, 0.145f - Mathf.Abs(i) * 0.04f), new Vector3(0.025f, 0.025f, 0.01f), EnemySwatch.Sorcery, r);
            }
        }

        private static void DrawBoneStaff(EnemyMeshBuilder b)
        {
            // A long staff of joined femurs (fist at the pivot), a small skull on top and three bone claws round the orb.
            b.Segment(new Vector3(0f, -0.9f, 0f), new Vector3(0f, 0.15f, 0f), 0.025f, 0.03f, 5, EnemySwatch.IvoryDark);
            b.Segment(new Vector3(0f, 0.15f, 0f), new Vector3(0f, 0.86f, 0f), 0.03f, 0.028f, 5, EnemySwatch.Ivory);
            b.Blob(new Vector3(0f, 0.15f, 0f), new Vector3(0.07f, 0.07f, 0.07f), EnemySwatch.Copper, coarse: true);
            b.Blob(new Vector3(0f, -0.9f, 0f), new Vector3(0.06f, 0.06f, 0.06f), EnemySwatch.IvoryDark, coarse: true);
            DrawSkull(b, new Vector3(0f, 0.93f, 0.01f), 0.15f, EnemySwatch.Ivory, EnemySwatch.Sorcery);
            for (int i = 0; i < 3; i++)
            {
                Quaternion around = Quaternion.Euler(0f, i * 120f, 0f);
                b.Horn(new Vector3(0f, 1.0f, 0f) + around * new Vector3(0.05f, 0f, 0f), 0.018f, 0.22f, 3, 22f, EnemySwatch.IvoryDark,
                    around * Quaternion.Euler(0f, 0f, -20f), 4, around * Vector3.forward);
            }
        }

        private static void DrawSorcererOrb(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.18f, 0.18f, 0.18f), EnemySwatch.Sorcery);
            b.Blob(new Vector3(0.02f, 0.02f, 0.03f), new Vector3(0.08f, 0.08f, 0.08f), EnemySwatch.Fang, coarse: true);
        }

        // ------------------------------------------------------------------
        // O Colosso de Crânios
        // ------------------------------------------------------------------

        private const float ColossusHip = 1.3f;

        public static void BuildBoneColossus(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, ColossusHip, 0f), DrawColossusBody);
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.72f, -0.02f), DrawColossusCore);
            bp.Mesh(ProceduralEnemyParts.RibsLeft, body, new Vector3(-0.46f, 0.72f, 0.22f), b => DrawRibDoor(b, -1f), meshKey: "RibDoor_L");
            bp.Mesh(ProceduralEnemyParts.RibsRight, body, new Vector3(0.46f, 0.72f, 0.22f), b => DrawRibDoor(b, 1f), meshKey: "RibDoor_R");

            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 1.52f, 0.06f), DrawColossusHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.34f * 0.62f, 0.02f), b => DrawJaw(b, Vector3.zero, 0.62f, EnemySwatch.Ivory));

            // The loose skulls it throws: folded away at rest, unfolded by the rig from the second phase.
            int halo = bp.Node(ProceduralEnemyParts.Halo, body, new Vector3(0f, 1.25f, -0.08f), scale: Vector3.one * HaloFoldedScale);
            for (int i = 0; i < HaloSkulls; i++)
            {
                float a = i * Mathf.PI * 2f / HaloSkulls;
                bp.Mesh(ProceduralEnemyParts.HaloSkullPrefix + i, halo, new Vector3(Mathf.Sin(a) * 0.95f, (i % 2) * 0.12f, Mathf.Cos(a) * 0.95f),
                    b => DrawSkull(b, Vector3.zero, 0.3f, EnemySwatch.Ivory, EnemySwatch.SoulFire, withJaw: true),
                    Quaternion.Euler(0f, a * Mathf.Rad2Deg, 0f), meshKey: "HaloSkull");
            }

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                int arm = bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body,
                    new Vector3(0.74f * side, 1.3f, 0f), b => DrawColossusArm(b, side), Quaternion.Euler(-8f, 0f, 6f * side),
                    meshKey: right ? "ColossusArm_R" : "ColossusArm_L");
                if (right)
                {
                    bp.Mesh(ProceduralEnemyParts.Weapon, arm, new Vector3(0.12f, -1.42f, 0.16f), DrawFemurClub, Quaternion.Euler(22f, 0f, 0f));
                }

                int leg = bp.Mesh(right ? ProceduralEnemyParts.LegRight : ProceduralEnemyParts.LegLeft, -1,
                    new Vector3(0.42f * side, ColossusHip, 0f), b => DrawColossusThigh(b, side), meshKey: right ? "ColossusThigh_R" : "ColossusThigh_L");
                int shin = bp.Mesh(right ? ProceduralEnemyParts.ShinRight : ProceduralEnemyParts.ShinLeft, leg,
                    new Vector3(0f, -0.62f, 0.06f), b => DrawColossusShin(b, side), meshKey: right ? "ColossusShin_R" : "ColossusShin_L");
                bp.Mesh(right ? ProceduralEnemyParts.FootRight : ProceduralEnemyParts.FootLeft, shin,
                    new Vector3(0f, -0.6f, -0.06f), DrawColossusFoot, meshKey: "ColossusFoot");
            }
        }

        private static void DrawColossusBody(EnemyMeshBuilder b)
        {
            // Pelvis of fused bones, a thick spine, then rings of skulls stacked into a barrel chest, each looking outward.
            b.Blob(new Vector3(0f, 0.02f, -0.04f), new Vector3(1.0f, 0.42f, 0.66f), EnemySwatch.IvoryDark, coarse: true);
            b.Segment(new Vector3(0f, 0f, -0.2f), new Vector3(0f, 1.45f, -0.18f), 0.12f, 0.1f, 6, EnemySwatch.IvoryDark);

            DrawSkullRing(b, 0.32f, 0.42f, 5, 0.34f, 0f, frontGap: true);
            DrawSkullRing(b, 0.7f, 0.52f, 6, 0.38f, 30f, frontGap: true);
            DrawSkullRing(b, 1.08f, 0.56f, 6, 0.4f, 0f, frontGap: false);
            DrawSkullRing(b, 1.38f, 0.44f, 4, 0.36f, 45f, frontGap: false);

            // Copper bands of the Artificers binding the stack, verdigris rivets.
            b.Cylinder(new Vector3(0f, 0.5f, -0.02f), 0.5f, 0.52f, 0.06f, 8, EnemySwatch.Copper, twist: 22.5f);
            b.Cylinder(new Vector3(0f, 1.25f, -0.02f), 0.6f, 0.56f, 0.07f, 8, EnemySwatch.Copper, twist: 22.5f);
            for (int i = 0; i < 4; i++)
            {
                float a = (i * 90f + 45f) * Mathf.Deg2Rad;
                b.Box(new Vector3(Mathf.Sin(a) * 0.53f, 0.53f, Mathf.Cos(a) * 0.53f), new Vector3(0.06f, 0.06f, 0.06f), EnemySwatch.Verdigris,
                    Quaternion.Euler(0f, a * Mathf.Rad2Deg, 0f));
            }

            // Shoulder yoke under the pauldrons.
            b.Box(new Vector3(0f, 1.3f, -0.06f), new Vector3(1.5f, 0.16f, 0.36f), EnemySwatch.IvoryDark, top: EnemySwatch.Ivory);
        }

        /// <summary>A ring of <paramref name="count"/> skulls round the body axis; with <paramref name="frontGap"/> the front one is left out (the rib doors close it).</summary>
        private static void DrawSkullRing(EnemyMeshBuilder b, float y, float radius, int count, float size, float startDeg, bool frontGap)
        {
            for (int i = 0; i < count; i++)
            {
                float deg = startDeg + i * 360f / count;
                float wrapped = Mathf.Repeat(deg + 180f, 360f) - 180f;
                if (frontGap && Mathf.Abs(wrapped) < 40f)
                {
                    continue;
                }

                float a = deg * Mathf.Deg2Rad;
                var at = new Vector3(Mathf.Sin(a) * radius, y + ((i & 1) == 0 ? 0f : 0.05f), Mathf.Cos(a) * radius - 0.02f);
                DrawSkull(b, at, size, (i % 3) == 1 ? EnemySwatch.IvoryDark : EnemySwatch.Ivory, EnemySwatch.SoulFire,
                    Quaternion.Euler(-6f + (i % 2) * 10f, deg, (i % 3 - 1) * 8f), withJaw: (i & 1) == 0);
            }
        }

        private static void DrawColossusCore(EnemyMeshBuilder b)
        {
            // The heart of trapped sparks every skull shares; it shows through the gaps and blazes when the ribs open.
            b.Blob(Vector3.zero, new Vector3(0.5f, 0.56f, 0.5f), EnemySwatch.SoulFire);
            b.Crystal(new Vector3(0.08f, 0.1f, 0.12f), 0.05f, 0.22f, EnemySwatch.SoulFire, Quaternion.Euler(30f, 0f, -30f), 4);
            b.Crystal(new Vector3(-0.1f, -0.06f, 0.1f), 0.05f, 0.2f, EnemySwatch.SoulFire, Quaternion.Euler(40f, 0f, 35f), 4);
        }

        /// <summary>Half of the rib cage over the chest, hinged at its outer edge (<paramref name="side"/> −1 = left half).</summary>
        private static void DrawRibDoor(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            // Hinge post, four curved ribs reaching to the middle, a half-sternum.
            b.Segment(new Vector3(0f, -0.38f, 0f), new Vector3(0f, 0.4f, 0f), 0.05f, 0.05f, 5, EnemySwatch.IvoryDark);
            for (int i = 0; i < 4; i++)
            {
                float y = 0.3f - i * 0.2f;
                b.Segment(new Vector3(0f, y, 0f), new Vector3(-0.24f, y - 0.03f, 0.13f), 0.035f, 0.03f, 4, EnemySwatch.Ivory);
                b.Segment(new Vector3(-0.24f, y - 0.03f, 0.13f), new Vector3(-0.42f, y - 0.07f, 0.14f), 0.03f, 0.025f, 4, EnemySwatch.Ivory);
            }

            b.Box(new Vector3(-0.44f, -0.06f, 0.14f), new Vector3(0.06f, 0.74f, 0.05f), EnemySwatch.IvoryDark);
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawColossusHead(EnemyMeshBuilder b)
        {
            // A great skull with blazing sockets, and a crown of five small skulls.
            b.Segment(new Vector3(0f, -0.24f, -0.08f), new Vector3(0f, 0.02f, -0.04f), 0.11f, 0.1f, 6, EnemySwatch.IvoryDark);
            DrawSkull(b, Vector3.zero, 0.62f, EnemySwatch.Ivory, EnemySwatch.SoulFire, coarse: false);
            b.Box(new Vector3(0f, 0.04f, 0.29f), new Vector3(0.5f, 0.05f, 0.04f), EnemySwatch.Copper);
            for (int i = 0; i < 5; i++)
            {
                float deg = -90f + i * 45f;
                float a = deg * Mathf.Deg2Rad;
                DrawSkull(b, new Vector3(Mathf.Sin(a) * 0.2f, 0.37f + (i == 2 ? 0.06f : 0f), Mathf.Cos(a) * 0.12f - 0.04f), 0.2f,
                    i == 2 ? EnemySwatch.Ivory : EnemySwatch.IvoryDark, EnemySwatch.SoulFire, Quaternion.Euler(-10f, deg * 0.5f, 0f));
            }
        }

        private static void DrawColossusArm(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            // Skull pauldron, humerus, a skull for the elbow, twin forearm bones and a claw of finger bones.
            DrawSkull(b, new Vector3(0.06f, 0.08f, 0f), 0.44f, EnemySwatch.Ivory, EnemySwatch.SoulFire, Quaternion.Euler(-20f, 90f, 0f), withJaw: true);
            b.Segment(new Vector3(0.04f, -0.1f, 0f), new Vector3(0.1f, -0.72f, 0f), 0.12f, 0.1f, 6, EnemySwatch.IvoryDark);
            DrawSkull(b, new Vector3(0.12f, -0.78f, -0.02f), 0.26f, EnemySwatch.IvoryDark, EnemySwatch.SoulFire, Quaternion.Euler(0f, 60f, 0f));
            b.Segment(new Vector3(0.06f, -0.8f, 0.04f), new Vector3(0.1f, -1.34f, 0.14f), 0.08f, 0.06f, 5, EnemySwatch.Ivory);
            b.Segment(new Vector3(0.18f, -0.8f, 0.02f), new Vector3(0.15f, -1.32f, 0.12f), 0.06f, 0.05f, 5, EnemySwatch.IvoryDark);
            b.Cylinder(new Vector3(0.12f, -1.12f, 0.08f), 0.13f, 0.13f, 0.06f, 6, EnemySwatch.Copper, rotation: Quaternion.Euler(12f, 0f, 0f));
            b.Blob(new Vector3(0.12f, -1.42f, 0.16f), new Vector3(0.22f, 0.18f, 0.2f), EnemySwatch.Ivory, coarse: true);
            for (int f = -1; f <= 1; f++)
            {
                b.Horn(new Vector3(0.12f + 0.07f * f, -1.48f, 0.2f), 0.035f, 0.22f, 3, 22f, EnemySwatch.IvoryDark, Quaternion.Euler(160f, 0f, 0f), 4);
            }

            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawFemurClub(EnemyMeshBuilder b)
        {
            // A giant femur (fist at the pivot) with two skulls lashed to its knob by copper bands.
            b.Segment(new Vector3(0f, -0.3f, 0f), new Vector3(0f, 1.05f, 0f), 0.07f, 0.09f, 6, EnemySwatch.Ivory);
            b.Blob(new Vector3(0f, -0.32f, 0f), new Vector3(0.18f, 0.14f, 0.16f), EnemySwatch.IvoryDark, coarse: true);
            b.Blob(new Vector3(0f, 1.12f, 0f), new Vector3(0.34f, 0.3f, 0.3f), EnemySwatch.Ivory);
            DrawSkull(b, new Vector3(0f, 1.15f, 0.2f), 0.3f, EnemySwatch.IvoryDark, EnemySwatch.SoulFire, withJaw: true);
            DrawSkull(b, new Vector3(0f, 1.18f, -0.2f), 0.28f, EnemySwatch.Ivory, EnemySwatch.SoulFire, Quaternion.Euler(0f, 180f, 0f));
            b.Cylinder(new Vector3(0f, 0.86f, 0f), 0.11f, 0.11f, 0.05f, 6, EnemySwatch.Copper);
            b.Cylinder(new Vector3(0f, 0.95f, 0f), 0.11f, 0.11f, 0.04f, 6, EnemySwatch.Verdigris);
            for (int i = 0; i < 3; i++)
            {
                Quaternion r = Quaternion.Euler(0f, i * 120f, 0f) * Quaternion.Euler(0f, 0f, -70f);
                b.Cylinder(new Vector3(0f, 1.0f + i * 0.06f, 0f), 0.04f, 0f, 0.22f, 4, EnemySwatch.Copper, rotation: r);
            }
        }

        private static void DrawColossusThigh(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            b.Blob(Vector3.zero, new Vector3(0.3f, 0.28f, 0.3f), EnemySwatch.IvoryDark, coarse: true);
            b.Segment(new Vector3(0f, -0.05f, 0f), new Vector3(0f, -0.6f, 0.06f), 0.13f, 0.11f, 6, EnemySwatch.Ivory);
            b.Cylinder(new Vector3(0f, -0.32f, 0.03f), 0.15f, 0.15f, 0.06f, 6, EnemySwatch.Copper);
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawColossusShin(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            DrawSkull(b, new Vector3(0f, 0.02f, 0.1f), 0.3f, EnemySwatch.Ivory, EnemySwatch.SoulFire);
            b.Segment(new Vector3(0f, -0.08f, 0f), new Vector3(0f, -0.58f, -0.06f), 0.11f, 0.1f, 6, EnemySwatch.IvoryDark);
            b.Segment(new Vector3(0.1f, -0.1f, -0.02f), new Vector3(0.09f, -0.56f, -0.07f), 0.05f, 0.045f, 4, EnemySwatch.Ivory);
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawColossusFoot(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.02f, 0f), new Vector3(0.24f, 0.2f, 0.24f), EnemySwatch.IvoryDark, coarse: true);
            b.Box(new Vector3(0f, -0.02f, 0.1f), new Vector3(0.4f, 0.12f, 0.5f), EnemySwatch.IvoryDark, top: EnemySwatch.Ivory);
            for (int t = -1; t <= 1; t++)
            {
                b.Cylinder(new Vector3(0.13f * t, -0.04f, 0.33f), 0.05f, 0f, 0.14f, 4, EnemySwatch.Ivory, rotation: Quaternion.Euler(90f, 0f, 0f));
            }
        }
    }
}
